Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_grn
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_GrnSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataCost(), dataRowCost() As String
        Dim dataBatch(), dataRowBatch(), dataSerial(), dataRowSerial(), dataAsset(), dataRowAsset() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim isUpdate As Boolean
        Dim formatTgl As String = "", formatTglWaktu As String = ""

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
        If (dataSplit.Length <> 5 And dataSplit.Length <> 6) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'grnid(0) As Integer, grncabang(1) As String, grnlokasi(2) As String, grngudang(3) As String, grnasalbarang(4) As String, 
        'grnasalbarangkategori(5) As Integer, grnjenispembelian(6) As String, grnjenispembeliankategori(7) As Integer, grncarabayar(8) As Integer, grnsumber(9) As String, 
        'grnautonotransaksi(10) As Integer, grnnotransaksi(11) As String, grntgl(12) As Date, grnkodepa(13) As Integer, grnsupplier(14) As Integer, 
        'grnsupplierkontak(15) As String, grn1alamat1(16) As String, grn1alamat2(17) As String, grn1alamat3(18) As String, grn2alamat1(19) As String, 
        'grn2alamat2(20) As String, grn2alamat3(21) As String, grnbagianpembelian(22) As Integer, grntermin(23) As String, grntgljatuhtempo(24) As Date, 
        'grnuraian(25) As String, grncatatan(26) As String, grnnoref(27) As String, grntglnoref(28) As Date, grntglpenutupan(29) As Date, 
        'grnmatauang(30) As String, grnkurs(31) As Double, grnhargatermasukpajak(32) As Integer, grntotal(33) As Double, grndiskonpersen(34) As String, 
        'grnjmldiskon(35) As Double, grntotalpajak1detail(36) As Double, grntotalpajak2detail(37) As Double, grnbiayalainpersen(38) As String, grnbiayalain(39) As Double, 
        'grntotaltransaksi(40) As Double, grnjmlbayar(41) As Double, grnrekdiskon(42) As String, grnrekpajak1(43) As String, grnrekpajak2(44) As String, 
        'grnrekbiayalain(45) As String, grnrekbayar(46) As String, grnidpr(47) As Integer, grnidcs(48) As Integer, grnidrq(49) As Integer, 
        'grnidbs(50) As Integer, grnidpo(51) As Integer, grnidipc(52) As Integer, grnstatusri(53) As Integer, grnstatusdnr(54) As Integer, 
        'grnstatusprt(55) As Integer, grnstatus(56) As Integer, grnstatussebelumnya(57) As Integer, grnjmlrevisi(58) As Integer, grncetakanke(59) As Integer, 
        'grninputuser(60) As Integer, grninputtgl(61) As DateTime, grnmodifikasiuser(62) As Integer, grnmodifikasitgl(63) As DateTime, grnposting(64) As Integer, 
        'grntutupperiode(65) As Integer, grnisclose(66) As Integer, grncustomtext1(67) As String, grncustomtext2(68) As String, grncustomtext3(69) As String, 
        'grncustomtext4(70) As String, grncustomtext5(71) As String, grncustomint1(72) As Integer, grncustomint2(73) As Integer, grncustomint3(74) As Integer, 
        'grncustomdbl1(75) As Double, grncustomdbl2(76) As Double, grncustomdbl3(77) As Double, grncustomdate1(78) As Date, grncustomdate2(79) As Date, 
        'grncustomdate3(80) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'grnid, grncabang, grnlokasi, grngudang, grnasalbarang, grnasalbarangkategori, grnjenispembelian, 
        'grnjenispembeliankategori, grncarabayar, grnsumber, grnautonotransaksi, grnnotransaksi, grntgl, grnkodepa, 
        'grnsupplier, grnsupplierkontak, grn1alamat1, grn1alamat2, grn1alamat3, grn2alamat1, grn2alamat2, 
        'grn2alamat3, grnbagianpembelian, grntermin, grntgljatuhtempo, grnuraian, grncatatan, grnnoref, 
        'grntglnoref, grntglpenutupan, grnmatauang, grnkurs, grnhargatermasukpajak, grntotal, grndiskonpersen, 
        'grnjmldiskon, grntotalpajak1detail, grntotalpajak2detail, grnbiayalainpersen, grnbiayalain, grntotaltransaksi, grnjmlbayar, 
        'grnrekdiskon, grnrekpajak1, grnrekpajak2, grnrekbiayalain, grnrekbayar, grnidpr, grnidcs, 
        'grnidrq, grnidbs, grnidpo, grnidipc, grnstatusri, grnstatusdnr, grnstatusprt, 
        'grnstatus, grnstatussebelumnya, grnjmlrevisi, grncetakanke, grninputuser, grninputtgl, grnmodifikasiuser, 
        'grnmodifikasitgl, grnposting, grntutupperiode, grnisclose, grncustomtext1, grncustomtext2, grncustomtext3, 
        'grncustomtext4, grncustomtext5, grncustomint1, grncustomint2, grncustomint3, grncustomdbl1, grncustomdbl2, 
        'grncustomdbl3, grncustomdate1, grncustomdate2, grncustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 81) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'grnid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "grnid required numeric." : GoTo selesai
        End If
        'grnasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "grnasalbarangkategori required numeric." : GoTo selesai
        End If
        'grnjenispembeliankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "grnjenispembeliankategori required numeric." : GoTo selesai
        End If
        'grncarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "grncarabayar required numeric." : GoTo selesai
        End If
        'grnautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "grnautonotransaksi required numeric." : GoTo selesai
        End If
        'grntgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "grntgl required date." : GoTo selesai
        End If
        'grnkodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "grnkodepa required numeric." : GoTo selesai
        End If
        'grnsupplier(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "grnsupplier required numeric." : GoTo selesai
        End If
        If (dataUtama(14) < 1) Then
            result(2) = "grnsupplier can't be empty." : GoTo selesai
        End If
        'grnbagianpembelian(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "grnbagianpembelian required numeric." : GoTo selesai
        End If
        'grntgljatuhtempo(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "grntgljatuhtempo required date." : GoTo selesai
        End If
        'grntglnoref(28) As Date
        If (IsDate(dataUtama(28)) = False) Then
            result(2) = "grntglnoref required date." : GoTo selesai
        End If
        'grntglpenutupan(29) As Date
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "grntglpenutupan required date." : GoTo selesai
        End If
        'grnkurs(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "grnkurs required numeric." : GoTo selesai
        End If
        'grnhargatermasukpajak(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "grnhargatermasukpajak required numeric." : GoTo selesai
        End If
        'grntotal(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "grntotal required numeric." : GoTo selesai
        End If
        'grnjmldiskon(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "grnjmldiskon required numeric." : GoTo selesai
        End If
        'grntotalpajak1detail(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "grntotalpajak1detail required numeric." : GoTo selesai
        End If
        'grntotalpajak2detail(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "grntotalpajak2detail required numeric." : GoTo selesai
        End If
        'grnbiayalain(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "grnbiayalain required numeric." : GoTo selesai
        End If
        'grntotaltransaksi(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "grntotaltransaksi required numeric." : GoTo selesai
        End If
        'grnjmlbayar(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "grnjmlbayar required numeric." : GoTo selesai
        End If
        'grnidpr(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "grnidpr required numeric." : GoTo selesai
        End If
        'grnidcs(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "grnidcs required numeric." : GoTo selesai
        End If
        'grnidrq(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "grnidrq required numeric." : GoTo selesai
        End If
        'grnidbs(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "grnidbs required numeric." : GoTo selesai
        End If
        'grnidpo(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "grnidpo required numeric." : GoTo selesai
        End If
        'grnidipc(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "grnidipc required numeric." : GoTo selesai
        End If
        'grnstatusri(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "grnstatusri required numeric." : GoTo selesai
        End If
        'grnstatusdnr(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "grnstatusdnr required numeric." : GoTo selesai
        End If
        'grnstatusprt(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "grnstatusprt required numeric." : GoTo selesai
        End If
        'grnstatus(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "grnstatus required numeric." : GoTo selesai
        End If
        'grnstatussebelumnya(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "grnstatussebelumnya required numeric." : GoTo selesai
        End If
        'grnjmlrevisi(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "grnjmlrevisi required numeric." : GoTo selesai
        End If
        'grncetakanke(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "grncetakanke required numeric." : GoTo selesai
        End If
        'grninputuser(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "grninputuser required numeric." : GoTo selesai
        End If
        'grninputtgl(61) As DateTime
        If (IsDate(dataUtama(61)) = False) Then
            result(2) = "grninputtgl required date." : GoTo selesai
        End If
        'grnmodifikasiuser(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "grnmodifikasiuser required numeric." : GoTo selesai
        End If
        'grnmodifikasitgl(63) As DateTime
        If (IsDate(dataUtama(63)) = False) Then
            result(2) = "grnmodifikasitgl required date." : GoTo selesai
        End If
        'grnposting(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "grnposting required numeric." : GoTo selesai
        End If
        'grntutupperiode(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "grntutupperiode required numeric." : GoTo selesai
        End If
        'grnisclose(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "grnisclose required numeric." : GoTo selesai
        End If
        'grncustomint1(72) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "grncustomint1 required numeric." : GoTo selesai
        End If
        'grncustomint2(73) As Integer
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "grncustomint2 required numeric." : GoTo selesai
        End If
        'grncustomint3(74) As Integer
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "grncustomint3 required numeric." : GoTo selesai
        End If
        'grncustomdbl1(75) As Double
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "grncustomdbl1 required numeric." : GoTo selesai
        End If
        'grncustomdbl2(76) As Double
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "grncustomdbl2 required numeric." : GoTo selesai
        End If
        'grncustomdbl3(77) As Double
        If (IsNumeric(dataUtama(77)) = False) Then
            result(2) = "grncustomdbl3 required numeric." : GoTo selesai
        End If
        'grncustomdate1(78) As Date
        If (IsDate(dataUtama(78)) = False) Then
            result(2) = "grncustomdate1 required date." : GoTo selesai
        End If
        'grncustomdate2(79) As Date
        If (IsDate(dataUtama(79)) = False) Then
            result(2) = "grncustomdate2 required date." : GoTo selesai
        End If
        'grncustomdate3(80) As Date
        If (IsDate(dataUtama(80)) = False) Then
            result(2) = "grncustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'grncabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "grncabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "grncabang should not be more than 25 character." : GoTo selesai
        End If

        'grnlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "grnlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "grnlokasi should not be more than 25 character." : GoTo selesai
        End If

        'grngudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "grngudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "grngudang should not be more than 25 character." : GoTo selesai
        End If

        'grnsumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "grnsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "grnsumber should not be more than 10 character." : GoTo selesai
        End If

        'grnnotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "grnnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "grnnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'grntgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "grntgl can't be empty" : GoTo selesai
        End If

        'grntgljatuhtempo(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "grntgljatuhtempo can't be empty" : GoTo selesai
        End If

        'grntglnoref(28) As Date
        If Len(dataUtama(28)) = 0 Then
            result(2) = "grntglnoref can't be empty" : GoTo selesai
        End If

        'grntglpenutupan(29) As Date
        If Len(dataUtama(29)) = 0 Then
            result(2) = "grntglpenutupan can't be empty" : GoTo selesai
        End If

        'grnmatauang(30) As String
        If Len(dataUtama(30)) = 0 Then
            result(2) = "grnmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(30)) > 25 Then
            result(2) = "grnmatauang should not be more than 25 character." : GoTo selesai
        End If

        'grnkurs(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "grnkurs can't be empty" : GoTo selesai
        End If

        'grntotal(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "grntotal can't be empty" : GoTo selesai
        End If

        'grndiskonpersen(34) As String
        If Len(dataUtama(34)) = 0 Then
            result(2) = "grndiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(34)) > 25 Then
            result(2) = "grndiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'grnjmldiskon(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "grnjmldiskon can't be empty" : GoTo selesai
        End If

        'grntotalpajak1detail(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "grntotalpajak1detail can't be empty" : GoTo selesai
        End If

        'grntotalpajak2detail(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "grntotalpajak2detail can't be empty" : GoTo selesai
        End If

        'grnbiayalainpersen(38) As String
        If Len(dataUtama(38)) = 0 Then
            result(2) = "grnbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(38)) > 25 Then
            result(2) = "grnbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'grnbiayalain(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "grnbiayalain can't be empty" : GoTo selesai
        End If

        'grntotaltransaksi(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "grntotaltransaksi can't be empty" : GoTo selesai
        End If

        'grnjmlbayar(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "grnjmlbayar can't be empty" : GoTo selesai
        End If

        'grninputtgl(61) As DateTime
        If Len(dataUtama(61)) = 0 Then
            result(2) = "grninputtgl can't be empty" : GoTo selesai
        End If

        'grnmodifikasitgl(63) As DateTime
        If Len(dataUtama(63)) = 0 Then
            result(2) = "grnmodifikasitgl can't be empty" : GoTo selesai
        End If

        'grncustomdbl1(75) As Double
        If Len(dataUtama(75)) = 0 Then
            result(2) = "grncustomdbl1 can't be empty" : GoTo selesai
        End If

        'grncustomdbl2(76) As Double
        If Len(dataUtama(76)) = 0 Then
            result(2) = "grncustomdbl2 can't be empty" : GoTo selesai
        End If

        'grncustomdbl3(77) As Double
        If Len(dataUtama(77)) = 0 Then
            result(2) = "grncustomdbl3 can't be empty" : GoTo selesai
        End If

        'grncustomdate1(78) As Date
        If Len(dataUtama(78)) = 0 Then
            result(2) = "grncustomdate1 can't be empty" : GoTo selesai
        End If

        'grncustomdate2(79) As Date
        If Len(dataUtama(79)) = 0 Then
            result(2) = "grncustomdate2 can't be empty" : GoTo selesai
        End If

        'grncustomdate3(80) As Date
        If Len(dataUtama(80)) = 0 Then
            result(2) = "grncustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "grnid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grncabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grngudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnjenispembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnjenispembeliankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grncarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grntgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnsupplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnsupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grn1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grn1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grn1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grn2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grn2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grn2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnbagianpembelian", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grntermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grntgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grncatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grntglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grntglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnhargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grntotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grndiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnjmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grntotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grntotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grntotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnjmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnrekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnrekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnrekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnrekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnrekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnidpr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnidcs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnidrq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnidbs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnidpo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnidipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnstatusri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnstatusdnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnstatusprt", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grncetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grninputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grninputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grntutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grncustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grncustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grncustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grncustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grncustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grncustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grncustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grncustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grncustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grncustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grncustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grncustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grncustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grncustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "grnid~grncabang~grnlokasi~grngudang~grnasalbarang~grnasalbarangkategori~grnjenispembelian~grnjenispembeliankategori~grncarabayar~grnsumber~grnautonotransaksi~grnnotransaksi~grntgl~grnkodepa~grnsupplier~grnsupplierkontak~grn1alamat1~grn1alamat2~grn1alamat3~grn2alamat1~grn2alamat2~grn2alamat3~grnbagianpembelian~grntermin~grntgljatuhtempo~grnuraian~grncatatan~grnnoref~grntglnoref~grntglpenutupan~grnmatauang~grnkurs~grnhargatermasukpajak~grntotal~grndiskonpersen~grnjmldiskon~grntotalpajak1detail~grntotalpajak2detail~grnbiayalainpersen~grnbiayalain~grntotaltransaksi~grnjmlbayar~grnrekdiskon~grnrekpajak1~grnrekpajak2~grnrekbiayalain~grnrekbayar~grnidpr~grnidcs~grnidrq~grnidbs~grnidpo~grnidipc~grnstatusri~grnstatusdnr~grnstatusprt~grnstatus~grnstatussebelumnya~grnjmlrevisi~grncetakanke~grninputuser~grninputtgl~grnmodifikasiuser~grnmodifikasitgl~grnposting~grntutupperiode~grnisclose~grncustomtext1~grncustomtext2~grncustomtext3~grncustomtext4~grncustomtext5~grncustomint1~grncustomint2~grncustomint3~grncustomdbl1~grncustomdbl2~grncustomdbl3~grncustomdate1~grncustomdate2~grncustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idgrndetail(0) As Integer, idgrn(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, hargafix(12) As Integer, harga(13) As Double, diskon(14) As String, 
        'jmldiskon(15) As Double, pajak1(16) As String, jmlpajak1(17) As Double, pajak2(18) As String, jmlpajak2(19) As Double, 
        'cabang(20) As String, lokasi(21) As String, gudang(22) As String, rekpersediaan(23) As String, rekdiskonpembelian(24) As String, 
        'rekhutangsementara(25) As String, costcenter(26) As String, divisi(27) As String, subdivisi(28) As String, proyek(29) As String, 
        'catatan(30) As String, urutan(31) As Integer, idprdetail(32) As Integer, idcsdetail(33) As Integer, idrqdetail(34) As Integer, 
        'idbsdetail(35) As Integer, idpodetail(36) As Integer, idipcdetail(37) As Integer, jmlri(38) As Double, statusri(39) As Integer, 
        'jmldnr(40) As Double, statusdnr(41) As Integer, jmlprt(42) As Double, statusprt(43) As Integer, isclose(44) As Integer, 
        'customtext1(45) As String, customtext2(46) As String, customtext3(47) As String, customdbl1(48) As Double, customdbl2(49) As Double, 
        'customdbl3(50) As Double, customdate1(51) As Date, customdate2(52) As Date, customdate3(53) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idgrndetail, idgrn, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, gudang, rekpersediaan, rekdiskonpembelian, rekhutangsementara, costcenter, divisi, 
        'subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, 
        'idbsdetail, idpodetail, idipcdetail, jmlri, statusri, jmldnr, statusdnr, 
        'jmlprt, statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idgrndetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idgrn", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "rekpersediaan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekdiskonpembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekhutangsementara", AsEnumTypeData.AsString)
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
        AsDataTableTambahField(dtdetail, "jmlri", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmldnr", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusdnr", AsEnumTypeData.AsInt64)
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
        Dim ftExistOutstandingPO As String = "", ftOutstandingPO As String = "", updNilaiPO As String = "", updFilterPO As String = ""
        Dim idbarang As Integer = 0, idpodetail As Integer = 0, jmlbarang As Double = 0
        Dim gudang As String = "", updStokOutBooking As String = ""

        'FILTER PO, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftPO As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 54) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idgrndetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idgrndetail required numeric." : GoTo selesai
            End If
            'idgrn(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idgrn required numeric." : GoTo selesai
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
            'harga(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'jmldiskon(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - jmldiskon required numeric." : GoTo selesai
            End If
            'jmlpajak1(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak1 required numeric." : GoTo selesai
            End If
            'jmlpajak2(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak2 required numeric." : GoTo selesai
            End If
            'urutan(31) As Integer
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idprdetail(32) As Integer
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - idprdetail required numeric." : GoTo selesai
            End If
            'idcsdetail(33) As Integer
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - idcsdetail required numeric." : GoTo selesai
            End If
            'idrqdetail(34) As Integer
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - idrqdetail required numeric." : GoTo selesai
            End If
            'idbsdetail(35) As Integer
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - idbsdetail required numeric." : GoTo selesai
            End If
            'idpodetail(36) As Integer
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - idpodetail required numeric." : GoTo selesai
            End If
            'idipcdetail(37) As Integer
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - idipcdetail required numeric." : GoTo selesai
            End If
            'jmlri(38) As Double
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - jmlri required numeric." : GoTo selesai
            End If
            'statusri(39) As Integer
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - statusri required numeric." : GoTo selesai
            End If
            'jmldnr(40) As Double
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - jmldnr required numeric." : GoTo selesai
            End If
            'statusdnr(41) As Integer
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - statusdnr required numeric." : GoTo selesai
            End If
            'jmlprt(42) As Double
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - jmlprt required numeric." : GoTo selesai
            End If
            'statusprt(43) As Integer
            If (IsNumeric(dataRowDetail(43)) = False) Then
                result(2) = "Row : " & i & " - statusprt required numeric." : GoTo selesai
            End If
            'isclose(44) As Integer
            If (IsNumeric(dataRowDetail(44)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(48) As Double
            If (IsNumeric(dataRowDetail(48)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(49) As Double
            If (IsNumeric(dataRowDetail(49)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(50) As Double
            If (IsNumeric(dataRowDetail(50)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(51) As Date
            If (IsDate(dataRowDetail(51)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(52) As Date
            If (IsDate(dataRowDetail(52)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(53) As Date
            If (IsDate(dataRowDetail(53)) = False) Then
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

            'harga(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If
            'If dataRowDetail(13) <= 0 Then
            '    result(2) = "Row : " & i & " - harga can't be less than or equal to zero" : GoTo selesai
            'End If

            'diskon(14) As String
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - diskon can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(14)) > 25 Then
                result(2) = "Row : " & i & " - diskon should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskon(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskon can't be empty" : GoTo selesai
                'Else
                '    'HITUNG JMLDISKON : jml(5) As Double, harga(13) As Double, diskon(14) As String
                '    dataRowDetail(15) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(13)), FixQuotes(dataRowDetail(14).ToString))
            End If

            'jmlpajak1(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak1 can't be empty" : GoTo selesai
            End If

            'jmlpajak2(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak2 can't be empty" : GoTo selesai
            End If

            'gudang(22) As String
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - gudang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(22)) > 25 Then
                result(2) = "Row : " & i & " - gudang should not be more than 25 character." : GoTo selesai
            End If

            'jmlri(38) As Double
            If Len(dataRowDetail(38)) = 0 Then
                result(2) = "Row : " & i & " - jmlri can't be empty" : GoTo selesai
            End If

            'jmldnr(40) As Double
            If Len(dataRowDetail(40)) = 0 Then
                result(2) = "Row : " & i & " - jmldnr can't be empty" : GoTo selesai
            End If

            'jmlprt(42) As Double
            If Len(dataRowDetail(42)) = 0 Then
                result(2) = "Row : " & i & " - jmlprt can't be empty" : GoTo selesai
            End If

            'customdbl1(48) As Double
            If Len(dataRowDetail(48)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(49) As Double
            If Len(dataRowDetail(49)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(50) As Double
            If Len(dataRowDetail(50)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(51) As Date
            If Len(dataRowDetail(51)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(52) As Date
            If Len(dataRowDetail(52)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(53) As Date
            If Len(dataRowDetail(53)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idgrndetail~idgrn~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~hargafix~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudang~rekpersediaan~rekdiskonpembelian~rekhutangsementara~costcenter~divisi~subdivisi~proyek~catatan~urutan~idprdetail~idcsdetail~idrqdetail~idbsdetail~idpodetail~idipcdetail~jmlri~statusri~jmldnr~statusdnr~jmlprt~statusprt~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'Set variabel
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , gudang(22) As String        , idpodetail(36) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : gudang = dataRowDetail(22) : idpodetail = dataRowDetail(36)

            'ValidasiBatchSerial
            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'VALIDASI OUTSTANDING -------------------------
            If idpodetail <> 0 Then 'PO
                'CEK PO YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftPO = IIf(Len(ftPO.ToString) = 0, "", ftPO & " OR ")
                ftPO = String.Concat(ftPO, " (pod.idpodetail = " & idpodetail & ") ")

                '1. CEK DATA EXIST
                ftExistOutstandingPO = IIf(Len(ftExistOutstandingPO.ToString) = 0, "", ftExistOutstandingPO & " UNION ")
                ftExistOutstandingPO = String.Concat(ftExistOutstandingPO, "SELECT EXISTS(SELECT 1 FROM m4_po_detail JOIN m4_po ON idpo = poid WHERE idpodetail = '" & idpodetail & "' AND (postatus = 2 OR postatus = 3 OR postatus = 4 OR postatus = 7) LIMIT 1) as rowExists, '" & idpodetail & "' as idpodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idpodetail=" & idpodetail)
                ftOutstandingPO = IIf(Len(ftOutstandingPO.ToString) = 0, "", ftOutstandingPO & " OR ")
                ftOutstandingPO = String.Concat(ftOutstandingPO, " (pod.idpodetail = " & idpodetail & " AND (CASE pod.jmlbarang WHEN 0 THEN pod.jmlbarang < 0 ELSE " & Outstanding & " > ROUND(pod.jmlbarang - pod.jmlrealisasi + (pod.jmlbarang * s.snilai), 5) END)) ")

                '3. SET NILAI UPDATE OUTSTANDING
                updNilaiPO = String.Concat("WHEN '" & idpodetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiPO)

                '4. SET FILTER UPDATE OUTSTANDING
                updFilterPO = IIf(Len(updFilterPO.ToString) = 0, "", updFilterPO & " OR ")
                updFilterPO = String.Concat(updFilterPO, "(idpodetail = '" & idpodetail & "')")

                'SET NILAI UPDATE STOK BOOKING (MENGURANGI)
                updStokOutBooking = IIf(Len(updStokOutBooking.ToString) = 0, "", updStokOutBooking & ", ")
                updStokOutBooking = String.Concat(updStokOutBooking, "('" & idbarang & "', '" & gudang & "', ('-" & jmlbarang & "'))") ' idbarang, gudang, jmlbooking
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


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idgrncost(0) As Integer, idgrn(1) As Integer, kodecost(2) As String, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, rekdebit(6) As String, rekkredit(7) As String, kontak(8) As Integer, termasukhpp(9) As Integer, 
        'catatan(10) As String, costcenter(11) As String, divisi(12) As String, subdivisi(13) As String, proyek(14) As String, 
        'urutan(15) As Integer, idprcost(16) As Integer, idcscost(17) As Integer, idrqcost(18) As Integer, idbscost(19) As Integer, 
        'idpocost(20) As Integer, idipccost(21) As Integer, jumlahri(22) As Double, statusri(23) As Integer, jumlahbayar(24) As Double, 
        'statusbayar(25) As Integer, isclose(26) As Integer, customtext1(27) As String, customtext2(28) As String, customtext3(29) As String, 
        'customdbl1(30) As Double, customdbl2(31) As Double, customdbl3(32) As Double, customdate1(33) As Date, customdate2(34) As Date, 
        'customdate3(35) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idgrncost, idgrn, kodecost, matauang, kurs, jumlah, rekdebit, 
        'rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, 
        'proyek, urutan, idprcost, idcscost, idrqcost, idbscost, idpocost, 
        'idipccost, jumlahri, statusri, jumlahbayar, statusbayar, isclose, customtext1, 
        'customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3

        'Buat datatable cost
        Dim dtcost As New DataTable
        AsDataTableTambahField(dtcost, "idgrncost", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "idgrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "kodecost", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "jumlah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "rekdebit", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "rekkredit", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "kontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "termasukhpp", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtcost, "jumlahri", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "statusri", AsEnumTypeData.AsInt64)
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

        'CEK PARAMETER DATA COST
        If dataSplit(4).Length > 0 Then

            'VALIDASI DAN SET DATA COST ======================================================
            'SPLIT PARAMETER DATA COST
            dataCost = dataSplit(4).Split(sptRow)
            'END OF VALIDASI DAN SET DATA COST ===============================================

            'VALIDASI DAN SET DATA ROW Cost ==================================================
            Dim JmlDtCost As Integer = dataCost.Length
            For i = 1 To JmlDtCost
                'SPLIT DATA Cost
                dataRowCost = dataCost(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA Cost -----------------------------------
                'CEK ARRAY DATA Cost
                If (dataRowCost.Length <> 36) Then
                    result(2) = "Cost Row : " & i & " - Invalid Cost transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW Cost ----------------------------

                'VALIDASI TIPE DATA Cost ------------------------------------------
                'idgrncost(0) As Integer
                If (IsNumeric(dataRowCost(0)) = False) Then
                    result(2) = "Cost Row : " & i & " - idgrncost required numeric." : GoTo selesai
                End If
                'idgrn(1) As Integer
                If (IsNumeric(dataRowCost(1)) = False) Then
                    result(2) = "Cost Row : " & i & " - idgrn required numeric." : GoTo selesai
                End If
                'kurs(4) As Double
                If (IsNumeric(dataRowCost(4)) = False) Then
                    result(2) = "Cost Row : " & i & " - kurs required numeric." : GoTo selesai
                End If
                'jumlah(5) As Double
                If (IsNumeric(dataRowCost(5)) = False) Then
                    result(2) = "Cost Row : " & i & " - jumlah required numeric." : GoTo selesai
                End If
                'kontak(8) As Integer
                If (IsNumeric(dataRowCost(8)) = False) Then
                    result(2) = "Cost Row : " & i & " - kontak required numeric." : GoTo selesai
                End If
                'termasukhpp(9) As Integer
                If (IsNumeric(dataRowCost(9)) = False) Then
                    result(2) = "Cost Row : " & i & " - termasukhpp required numeric." : GoTo selesai
                End If
                'urutan(15) As Integer
                If (IsNumeric(dataRowCost(15)) = False) Then
                    result(2) = "Cost Row : " & i & " - urutan required numeric." : GoTo selesai
                End If
                'idprcost(16) As Integer
                If (IsNumeric(dataRowCost(16)) = False) Then
                    result(2) = "Cost Row : " & i & " - idprcost required numeric." : GoTo selesai
                End If
                'idcscost(17) As Integer
                If (IsNumeric(dataRowCost(17)) = False) Then
                    result(2) = "Cost Row : " & i & " - idcscost required numeric." : GoTo selesai
                End If
                'idrqcost(18) As Integer
                If (IsNumeric(dataRowCost(18)) = False) Then
                    result(2) = "Cost Row : " & i & " - idrqcost required numeric." : GoTo selesai
                End If
                'idbscost(19) As Integer
                If (IsNumeric(dataRowCost(19)) = False) Then
                    result(2) = "Cost Row : " & i & " - idbscost required numeric." : GoTo selesai
                End If
                'idpocost(20) As Integer
                If (IsNumeric(dataRowCost(20)) = False) Then
                    result(2) = "Cost Row : " & i & " - idpocost required numeric." : GoTo selesai
                End If
                'idipccost(21) As Integer
                If (IsNumeric(dataRowCost(21)) = False) Then
                    result(2) = "Cost Row : " & i & " - idipccost required numeric." : GoTo selesai
                End If
                'jumlahri(22) As Double
                If (IsNumeric(dataRowCost(22)) = False) Then
                    result(2) = "Cost Row : " & i & " - jumlahri required numeric." : GoTo selesai
                End If
                'statusri(23) As Integer
                If (IsNumeric(dataRowCost(23)) = False) Then
                    result(2) = "Cost Row : " & i & " - statusri required numeric." : GoTo selesai
                End If
                'jumlahbayar(24) As Double
                If (IsNumeric(dataRowCost(24)) = False) Then
                    result(2) = "Cost Row : " & i & " - jumlahbayar required numeric." : GoTo selesai
                End If
                'statusbayar(25) As Integer
                If (IsNumeric(dataRowCost(25)) = False) Then
                    result(2) = "Cost Row : " & i & " - statusbayar required numeric." : GoTo selesai
                End If
                'isclose(26) As Integer
                If (IsNumeric(dataRowCost(26)) = False) Then
                    result(2) = "Cost Row : " & i & " - isclose required numeric." : GoTo selesai
                End If
                'customdbl1(30) As Double
                If (IsNumeric(dataRowCost(30)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdbl1 required numeric." : GoTo selesai
                End If
                'customdbl2(31) As Double
                If (IsNumeric(dataRowCost(31)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdbl2 required numeric." : GoTo selesai
                End If
                'customdbl3(32) As Double
                If (IsNumeric(dataRowCost(32)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdbl3 required numeric." : GoTo selesai
                End If
                'customdate1(33) As Date
                If (IsDate(dataRowCost(33)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdate1 required date." : GoTo selesai
                End If
                'customdate2(34) As Date
                If (IsDate(dataRowCost(34)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdate2 required date." : GoTo selesai
                End If
                'customdate3(35) As Date
                If (IsDate(dataRowCost(35)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdate3 required date." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA Cost -----------------------------------

                'VALIDASI DATA Cost ---------------------------------------
                'kodecost(2) As String
                If Len(dataRowCost(2)) = 0 Then
                    result(2) = "Cost Row : " & i & " - kodecost can't be empty" : GoTo selesai
                End If
                If Len(dataRowCost(2)) > 25 Then
                    result(2) = "Cost Row : " & i & " - kodecost should not be more than 25 character." : GoTo selesai
                End If

                'matauang(3) As String
                If Len(dataRowCost(3)) = 0 Then
                    result(2) = "Cost Row : " & i & " - matauang can't be empty" : GoTo selesai
                End If
                If Len(dataRowCost(3)) > 25 Then
                    result(2) = "Cost Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
                End If

                'kurs(4) As Double
                If Len(dataRowCost(4)) = 0 Then
                    result(2) = "Cost Row : " & i & " - kurs can't be empty" : GoTo selesai
                End If

                'jumlah(5) As Double
                If Len(dataRowCost(5)) = 0 Then
                    result(2) = "Cost Row : " & i & " - jumlah can't be empty" : GoTo selesai
                End If

                If dataRowCost(9) = 0 Then
                    If Len(dataRowCost(6)) = 0 Then
                        result(2) = "Cost Row : " & i & " - rekdebit can't be empty" : GoTo selesai
                    End If
                End If
                If Len(dataRowCost(6)) > 25 Then
                    result(2) = "Cost Row : " & i & " - rekdebit should not be more than 25 character." : GoTo selesai
                End If

                'rekkredit(7) As String
                If Len(dataRowCost(7)) = 0 Then
                    result(2) = "Cost Row : " & i & " - rekkredit can't be empty" : GoTo selesai
                End If
                If Len(dataRowCost(7)) > 25 Then
                    result(2) = "Cost Row : " & i & " - rekkredit should not be more than 25 character." : GoTo selesai
                End If

                'jumlahri(22) As Double
                If Len(dataRowCost(22)) = 0 Then
                    result(2) = "Cost Row : " & i & " - jumlahri can't be empty" : GoTo selesai
                End If

                'jumlahbayar(24) As Double
                If Len(dataRowCost(24)) = 0 Then
                    result(2) = "Cost Row : " & i & " - jumlahbayar can't be empty" : GoTo selesai
                End If

                'customdbl1(30) As Double
                If Len(dataRowCost(30)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
                End If

                'customdbl2(31) As Double
                If Len(dataRowCost(31)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
                End If

                'customdbl3(32) As Double
                If Len(dataRowCost(32)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
                End If

                'customdate1(33) As Date
                If Len(dataRowCost(33)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdate1 can't be empty" : GoTo selesai
                End If

                'customdate2(34) As Date
                If Len(dataRowCost(34)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdate2 can't be empty" : GoTo selesai
                End If

                'customdate3(35) As Date
                If Len(dataRowCost(35)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdate3 can't be empty" : GoTo selesai
                End If

                'END OF VALIDASI DATA Cost --------------------------------

                If AsDataTableTambahData(dtcost, "idgrncost~idgrn~kodecost~matauang~kurs~jumlah~rekdebit~rekkredit~kontak~termasukhpp~catatan~costcenter~divisi~subdivisi~proyek~urutan~idprcost~idcscost~idrqcost~idbscost~idpocost~idipccost~jumlahri~statusri~jumlahbayar~statusbayar~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowCost(0) & "~" & dataRowCost(1) & "~" & dataRowCost(2) & "~" & dataRowCost(3) & "~" & dataRowCost(4) & "~" & dataRowCost(5) & "~" & dataRowCost(6) & "~" & dataRowCost(7) & "~" & dataRowCost(8) & "~" & dataRowCost(9) & "~" & dataRowCost(10) & "~" & dataRowCost(11) & "~" & dataRowCost(12) & "~" & dataRowCost(13) & "~" & dataRowCost(14) & "~" & dataRowCost(15) & "~" & dataRowCost(16) & "~" & dataRowCost(17) & "~" & dataRowCost(18) & "~" & dataRowCost(19) & "~" & dataRowCost(20) & "~" & dataRowCost(21) & "~" & dataRowCost(22) & "~" & dataRowCost(23) & "~" & dataRowCost(24) & "~" & dataRowCost(25) & "~" & dataRowCost(26) & "~" & dataRowCost(27) & "~" & dataRowCost(28) & "~" & dataRowCost(29) & "~" & dataRowCost(30) & "~" & dataRowCost(31) & "~" & dataRowCost(32) & "~" & dataRowCost(33) & "~" & dataRowCost(34) & "~" & dataRowCost(35)) = False Then
                    result(2) = "Cost Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA COST ===========================================
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
        If dataSplit.Length > 5 Then
            If dataSplit(5).Length > 0 Then

                'VALIDASI DAN SET DATA ASSET ======================================================
                'SPLIT PARAMETER DATA ASSET
                dataAsset = dataSplit(5).Split(sptRow)
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
                vStatus = drutama("grnstatus")
                vTgl = AsFormatTanggal(drutama("grntgl"))


                'CEK HAK AKSES STATUS ============================
                Dim vAkses As Integer = 0, msgAkses As String = ""
                'MODUL DAN MENU HARUS DISESUAIKAN
                Dim vModuleId As Integer = 4, vMenuId As Integer = 10
                Select Case drutama("grnstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("grntgl")), AsFormatTanggal(drutama("grntgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("grnstatus") = 2 Or drutama("grnstatus") = 1 Or drutama("grnstatus") = 8 Or drutama("grnstatus") = 9 Or drutama("grnstatus") = 10 Or drutama("grnstatus") = 11 Then

                    'VALIDASI BATCH SERIAL ---------------
                    'ValidasiBatchSerial
                    Dim rsValidasi As String = ValidasiBatchSerial(dtdetail, dtbatch, dtserial, ftBarang, "jmlbarang", 1)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    'END OF VALIDASI BATCH SERIAL --------

                    'VALIDASI ASSET ----------------------
                    'ValidasiAsset
                    rsValidasi = ValidasiAsset(dtdetail, dtasset, ftBarang, "jmlbarang", 1)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    'END OF VALIDASI ASSET ---------------

                    rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstandingPO, ftOutstandingPO, "", "", "", "", ftPO, drutama("grnhargatermasukpajak"))
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai

                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("grntermin").ToString, AsFormatTanggal(drutama("grntgl")), "grntgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("grntgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                ''PERHITUNGAN TOTAL UTAMA ================================
                ''DIAMBILKAN DARI DATA DETAIL

                ''TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                ''SUBTOTAL = (jml * harga) - jmldiskon
                'AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                'dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                ''TOTAL = subtotal
                'drutama("grntotal") = AsDataTableDSum(dtdetail, "subtotal")

                ''TOTALPAJAK1 = jmlpajak1
                'drutama("grntotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                ''TOTALPAJAK2 = jmlpajak2
                'drutama("grntotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                ''JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                ''JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'If Integer.Parse(drutama("grnhargatermasukpajak")) = 0 Then
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                '    drutama("grntotaltransaksi") = Double.Parse(drutama("grntotal")) - Double.Parse(drutama("grnjmldiskon")) + Double.Parse(drutama("grntotalpajak1detail")) + Double.Parse(drutama("grntotalpajak2detail")) + Double.Parse(drutama("grnbiayalain"))

                'Else
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                '    drutama("grntotaltransaksi") = Double.Parse(drutama("grntotal")) - Double.Parse(drutama("grnjmldiskon")) + Double.Parse(drutama("grnbiayalain"))

                'End If
                ''END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("grnid")
                    notransaksi = drutama("grnnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(grnid), grnnotransaksi FROM M4_grn WHERE grnid='" & result(4) & "' AND grnstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("grnautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("grncabang"), drutama("grnlokasi"), drutama("grnsumber"), drutama("grntgl"), drutama("grnsumber"), 4)
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(grnid) FROM m4_grn WHERE grnnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_grn_history
                        Dim rsSimpanHistory As String = SimpanHistory.m4_Grn_HistorySimpan("" & paramSplit(0) & "★M4_Grn_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("grnsumber")) & "▼" & FixQuotes(drutama("grnid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M4_Grn set grncabang  = '" & FixQuotes(drutama("grncabang")) & "', grnlokasi  = '" & FixQuotes(drutama("grnlokasi")) & "', grngudang  = '" & FixQuotes(drutama("grngudang")) & "', grnasalbarang  = '" & FixQuotes(drutama("grnasalbarang")) & "', grnasalbarangkategori  = " & drutama("grnasalbarangkategori") & ", grnjenispembelian  = '" & FixQuotes(drutama("grnjenispembelian")) & "', grnjenispembeliankategori  = " & drutama("grnjenispembeliankategori") & ", grncarabayar  = " & drutama("grncarabayar") & ", grnsumber  = '" & FixQuotes(drutama("grnsumber")) & "', grnautonotransaksi  = " & drutama("grnautonotransaksi") & ", grnnotransaksi  = '" & FixQuotes(notransaksi) & "', grntgl  = '" & FixQuotes(AsFormatTanggal(drutama("grntgl"))) & "', grnkodepa  = " & drutama("grnkodepa") & ", grnsupplier  = " & drutama("grnsupplier") & ", grnsupplierkontak  = '" & FixQuotes(drutama("grnsupplierkontak")) & "', grn1alamat1  = '" & FixQuotes(drutama("grn1alamat1")) & "', grn1alamat2  = '" & FixQuotes(drutama("grn1alamat2")) & "', grn1alamat3  = '" & FixQuotes(drutama("grn1alamat3")) & "', grn2alamat1  = '" & FixQuotes(drutama("grn2alamat1")) & "', grn2alamat2  = '" & FixQuotes(drutama("grn2alamat2")) & "', grn2alamat3  = '" & FixQuotes(drutama("grn2alamat3")) & "', grnbagianpembelian  = " & drutama("grnbagianpembelian") & ", grntermin  = '" & FixQuotes(drutama("grntermin")) & "', grntgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("grntgljatuhtempo"))) & "', grnuraian  = '" & FixQuotes(drutama("grnuraian")) & "', grncatatan  = '" & FixQuotes(drutama("grncatatan")) & "', grnnoref  = '" & FixQuotes(drutama("grnnoref")) & "', grntglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("grntglnoref"))) & "', grntglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("grntglpenutupan"))) & "', grnmatauang  = '" & FixQuotes(drutama("grnmatauang")) & "', grnkurs  = '" & FixDouble(drutama("grnkurs")) & "', grnhargatermasukpajak  = " & drutama("grnhargatermasukpajak") & ", grntotal  = '" & FixDouble(drutama("grntotal")) & "', grndiskonpersen  = '" & FixQuotes(drutama("grndiskonpersen")) & "', grnjmldiskon  = '" & FixDouble(drutama("grnjmldiskon")) & "', grntotalpajak1detail  = '" & FixDouble(drutama("grntotalpajak1detail")) & "', grntotalpajak2detail  = '" & FixDouble(drutama("grntotalpajak2detail")) & "', grnbiayalainpersen  = '" & FixQuotes(drutama("grnbiayalainpersen")) & "', grnbiayalain  = '" & FixDouble(drutama("grnbiayalain")) & "', grntotaltransaksi  = '" & FixDouble(drutama("grntotaltransaksi")) & "', grnjmlbayar  = '" & FixDouble(drutama("grnjmlbayar")) & "', grnrekdiskon  = '" & FixQuotes(drutama("grnrekdiskon")) & "', grnrekpajak1  = '" & FixQuotes(drutama("grnrekpajak1")) & "', grnrekpajak2  = '" & FixQuotes(drutama("grnrekpajak2")) & "', grnrekbiayalain  = '" & FixQuotes(drutama("grnrekbiayalain")) & "', grnrekbayar  = '" & FixQuotes(drutama("grnrekbayar")) & "', grnidpr  = " & drutama("grnidpr") & ", grnidcs  = " & drutama("grnidcs") & ", grnidrq  = " & drutama("grnidrq") & ", grnidbs  = " & drutama("grnidbs") & ", grnidpo  = " & drutama("grnidpo") & ", grnidipc  = " & drutama("grnidipc") & ", grnstatusri  = " & drutama("grnstatusri") & ", grnstatusdnr  = " & drutama("grnstatusdnr") & ", grnstatusprt  = " & drutama("grnstatusprt") & ", grnstatus  = " & drutama("grnstatus") & ", grnstatussebelumnya  = " & drutama("grnstatussebelumnya") & ", grnjmlrevisi  = grnjmlrevisi+1, grncetakanke  = " & drutama("grncetakanke") & ", grnmodifikasiuser  = " & drutama("grnmodifikasiuser") & ", grnmodifikasitgl  = NOW(), grnposting  = 0, grntutupperiode  = " & drutama("grntutupperiode") & ", grncustomtext1  = '" & FixQuotes(drutama("grncustomtext1")) & "', grncustomtext2  = '" & FixQuotes(drutama("grncustomtext2")) & "', grncustomtext3  = '" & FixQuotes(drutama("grncustomtext3")) & "', grncustomtext4  = '" & FixQuotes(drutama("grncustomtext4")) & "', grncustomtext5  = '" & FixQuotes(drutama("grncustomtext5")) & "', grncustomint1  = " & drutama("grncustomint1") & ", grncustomint2  = " & drutama("grncustomint2") & ", grncustomint3  = " & drutama("grncustomint3") & ", grncustomdbl1  = '" & FixDouble(drutama("grncustomdbl1")) & "', grncustomdbl2  = '" & FixDouble(drutama("grncustomdbl2")) & "', grncustomdbl3  = '" & FixDouble(drutama("grncustomdbl3")) & "', grncustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("grncustomdate1"))) & "', grncustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("grncustomdate2"))) & "', grncustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("grncustomdate3"))) & "' where grnid = '" & drutama("grnid") & "'"
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

                    If drutama("grnautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("grncabang"), drutama("grnlokasi"), drutama("grnsumber"), drutama("grntgl"), drutama("grnsumber"), 4)
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
                        notransaksi = drutama("grnnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(grnid) FROM m4_grn WHERE grnnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Grn (grncabang, grnlokasi, grngudang, grnasalbarang, grnasalbarangkategori, grnjenispembelian, grnjenispembeliankategori, grncarabayar, grnsumber, grnautonotransaksi, grnnotransaksi, grntgl, grnkodepa, grnsupplier, grnsupplierkontak, grn1alamat1, grn1alamat2, grn1alamat3, grn2alamat1, grn2alamat2, grn2alamat3, grnbagianpembelian, grntermin, grntgljatuhtempo, grnuraian, grncatatan, grnnoref, grntglnoref, grntglpenutupan, grnmatauang, grnkurs, grnhargatermasukpajak, grntotal, grndiskonpersen, grnjmldiskon, grntotalpajak1detail, grntotalpajak2detail, grnbiayalainpersen, grnbiayalain, grntotaltransaksi, grnjmlbayar, grnrekdiskon, grnrekpajak1, grnrekpajak2, grnrekbiayalain, grnrekbayar, grnidpr, grnidcs, grnidrq, grnidbs, grnidpo, grnidipc, grnstatusri, grnstatusdnr, grnstatusprt, grnstatus, grnstatussebelumnya, grnjmlrevisi, grncetakanke, grninputuser, grninputtgl, grnmodifikasiuser, grnmodifikasitgl, grnposting, grntutupperiode, grnisclose, grncustomtext1, grncustomtext2, grncustomtext3, grncustomtext4, grncustomtext5, grncustomint1, grncustomint2, grncustomint3, grncustomdbl1, grncustomdbl2, grncustomdbl3, grncustomdate1, grncustomdate2, grncustomdate3) values('" & FixQuotes(drutama("grncabang")) & "', '" & FixQuotes(drutama("grnlokasi")) & "', '" & FixQuotes(drutama("grngudang")) & "', '" & FixQuotes(drutama("grnasalbarang")) & "', " & drutama("grnasalbarangkategori") & ", '" & FixQuotes(drutama("grnjenispembelian")) & "', " & drutama("grnjenispembeliankategori") & ", " & drutama("grncarabayar") & ", '" & FixQuotes(drutama("grnsumber")) & "', " & drutama("grnautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("grntgl"))) & "', " & drutama("grnkodepa") & ", " & drutama("grnsupplier") & ", '" & FixQuotes(drutama("grnsupplierkontak")) & "', '" & FixQuotes(drutama("grn1alamat1")) & "', '" & FixQuotes(drutama("grn1alamat2")) & "', '" & FixQuotes(drutama("grn1alamat3")) & "', '" & FixQuotes(drutama("grn2alamat1")) & "', '" & FixQuotes(drutama("grn2alamat2")) & "', '" & FixQuotes(drutama("grn2alamat3")) & "', " & drutama("grnbagianpembelian") & ", '" & FixQuotes(drutama("grntermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("grntgljatuhtempo"))) & "', '" & FixQuotes(drutama("grnuraian")) & "', '" & FixQuotes(drutama("grncatatan")) & "', '" & FixQuotes(drutama("grnnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("grntglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("grntglpenutupan"))) & "', '" & FixQuotes(drutama("grnmatauang")) & "', '" & FixDouble(drutama("grnkurs")) & "', " & drutama("grnhargatermasukpajak") & ", '" & FixDouble(drutama("grntotal")) & "', '" & FixQuotes(drutama("grndiskonpersen")) & "', '" & FixDouble(drutama("grnjmldiskon")) & "', '" & FixDouble(drutama("grntotalpajak1detail")) & "', '" & FixDouble(drutama("grntotalpajak2detail")) & "', '" & FixQuotes(drutama("grnbiayalainpersen")) & "', '" & FixDouble(drutama("grnbiayalain")) & "', '" & FixDouble(drutama("grntotaltransaksi")) & "', '" & FixDouble(drutama("grnjmlbayar")) & "', '" & FixQuotes(drutama("grnrekdiskon")) & "', '" & FixQuotes(drutama("grnrekpajak1")) & "', '" & FixQuotes(drutama("grnrekpajak2")) & "', '" & FixQuotes(drutama("grnrekbiayalain")) & "', '" & FixQuotes(drutama("grnrekbayar")) & "', " & drutama("grnidpr") & ", " & drutama("grnidcs") & ", " & drutama("grnidrq") & ", " & drutama("grnidbs") & ", " & drutama("grnidpo") & ", " & drutama("grnidipc") & ", " & drutama("grnstatusri") & ", " & drutama("grnstatusdnr") & ", " & drutama("grnstatusprt") & ", " & drutama("grnstatus") & ", " & drutama("grnstatussebelumnya") & ", " & drutama("grnjmlrevisi") & ", " & drutama("grncetakanke") & ", " & drutama("grninputuser") & ", NOW(), " & drutama("grnmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("grntutupperiode") & ", " & drutama("grnisclose") & ", '" & FixQuotes(drutama("grncustomtext1")) & "', '" & FixQuotes(drutama("grncustomtext2")) & "', '" & FixQuotes(drutama("grncustomtext3")) & "', '" & FixQuotes(drutama("grncustomtext4")) & "', '" & FixQuotes(drutama("grncustomtext5")) & "', " & drutama("grncustomint1") & ", " & drutama("grncustomint2") & ", " & drutama("grncustomint3") & ", '" & FixDouble(drutama("grncustomdbl1")) & "', '" & FixDouble(drutama("grncustomdbl2")) & "', '" & FixDouble(drutama("grncustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("grncustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("grncustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("grncustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select grnid from M4_grn where grnnotransaksi='" & notransaksi & "' AND grninputuser= '" & userid & "' order by grnmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Grn_Detail where idgrn = '" & result(4) & "'"
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
                    Dim dtPO As New DataTable
                    Dim strValue2 As New StringBuilder

                    For Each dr1 As DataRow In dtdetail.Rows

                        'VALIDASI MATAUANG HARUS SAMA DENGAN TRANSAKSI UTAMA --------------------
                        If Not drutama("grnmatauang").ToString.Equals(dr1("matauang").ToString) Then
                            result(2) = "Row : " & dr1("urutan") & " - " & dr1("tipebarang") & " | " & dr1("namabarang") & " currency (" & dr1("matauang") & ") doesn't belong to the main transactions." : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF VALIDASI MATAUANG HARUS SAMA DENGAN TRANSAKSI UTAMA -------------


                        'SET HARGA DARI PO ------------------------------------------------------
                        sql = "SELECT jml, harga, diskon, pajak1, jmlpajak1, pajak2, jmlpajak2, IFNULL(t1.tnilai,0) as nilaipajak1, IFNULL(t2.tnilai,0) as nilaipajak2 FROM m4_po_detail pod LEFT JOIN m1_tax t1 ON pod.pajak1 = t1.tkode LEFT JOIN m1_tax t2 ON pod.pajak2 = t2.tkode WHERE idpodetail = '" & FixDouble(dr1("idpodetail")) & "'"
                        dtPO = AsDataTableAmbilDariDBCon(sql, myConn)
                        If dtPO.Rows.Count > 0 Then
                            'SET HARGA - ambil dari PO
                            dr1("harga") = Double.Parse(dtPO.Rows(0)("harga"))

                            'SET DISKON - ambil dari PO
                            dr1("diskon") = dtPO.Rows(0)("diskon")

                            'SET JMLDISKON - hitung diskon
                            dr1("jmldiskon") = F_Diskon(Double.Parse(dr1("jml")), Double.Parse(dr1("harga")), FixQuotes(dr1("diskon").ToString))

                            'SET PAJAK1 - ambil dari po
                            dr1("pajak1") = dtPO.Rows(0)("pajak1")

                            'SET PAJAK2 - ambil dari po
                            dr1("pajak2") = dtPO.Rows(0)("pajak2")

                            ''SET JMLPAJAK1 - ambil dari po = (jmlpajakpo / jmlpo) * jml
                            'dr1("jmlpajak1") = (Double.Parse(dtPO.Rows(0)("jmlpajak1")) / Double.Parse(dtPO.Rows(0)("jml"))) * Double.Parse(dr1("jml"))

                            ''SET JMLPAJAK2 - ambil dari po = (jmlpajakpo / jmlpo) * jml
                            'dr1("jmlpajak2") = (Double.Parse(dtPO.Rows(0)("jmlpajak2")) / Double.Parse(dtPO.Rows(0)("jml"))) * Double.Parse(dr1("jml"))

                            If drutama("grnhargatermasukpajak") = 1 Then

                                'SET JMLPAJAK1
                                dr1("jmlpajak1") = (((Decimal.Parse(dr1("jml")) * Decimal.Parse(dr1("harga"))) - Decimal.Parse(dr1("jmldiskon"))) / (100 + Decimal.Parse(dtPO.Rows(0)("nilaipajak1")))) * Decimal.Parse(dtPO.Rows(0)("nilaipajak1"))

                                'SET JMLPAJAK2
                                dr1("jmlpajak2") = (((Decimal.Parse(dr1("jml")) * Decimal.Parse(dr1("harga"))) - Decimal.Parse(dr1("jmldiskon"))) / (100 + Decimal.Parse(dtPO.Rows(0)("nilaipajak1")))) * Decimal.Parse(dtPO.Rows(0)("nilaipajak2"))

                            Else

                                'SET JMLPAJAK1
                                dr1("jmlpajak1") = ((Decimal.Parse(dr1("jml")) * Decimal.Parse(dr1("harga"))) - Decimal.Parse(dr1("jmldiskon"))) * (Decimal.Parse(dtPO.Rows(0)("nilaipajak1")) / 100)

                                'SET JMLPAJAK2
                                dr1("jmlpajak2") = ((Decimal.Parse(dr1("jml")) * Decimal.Parse(dr1("harga"))) - Decimal.Parse(dr1("jmldiskon"))) * (Decimal.Parse(dtPO.Rows(0)("nilaipajak2")) / 100)

                            End If

                        End If
                        'END OF SET HARGA DARI PO -----------------------------------------------


                        'QUERY INSERT DETAIL
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idgrndetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', " & dr1("hargafix") & ", '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("rekdiskonpembelian")) & "', '" & FixQuotes(dr1("rekhutangsementara")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idprdetail") & ", " & dr1("idcsdetail") & ", " & dr1("idrqdetail") & ", " & dr1("idbsdetail") & ", " & dr1("idpodetail") & ", " & dr1("idipcdetail") & ", '" & FixDouble(dr1("jmlri")) & "', " & dr1("statusri") & ", '" & FixDouble(dr1("jmldnr")) & "', " & dr1("statusdnr") & ", '" & FixDouble(dr1("jmlprt")) & "', " & dr1("statusprt") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Grn_Detail(idgrndetail, idgrn, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, rekpersediaan, rekdiskonpembelian, rekhutangsementara, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                'Hapus cost ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_grn_Cost where idgrn = " & result(4)
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
                        strValue2.Append("(" & dr1("idgrncost") & ", " & result(4) & ", '" & FixQuotes(dr1("kodecost")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixQuotes(dr1("rekdebit")) & "', '" & FixQuotes(dr1("rekkredit")) & "', " & dr1("kontak") & ", " & dr1("termasukhpp") & ", '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("idprcost") & ", " & dr1("idcscost") & ", " & dr1("idrqcost") & ", " & dr1("idbscost") & ", " & dr1("idpocost") & ", " & dr1("idipccost") & ", '" & FixDouble(dr1("jumlahri")) & "', " & dr1("statusri") & ", '" & FixDouble(dr1("jumlahbayar")) & "', " & dr1("statusbayar") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Grn_Cost(idgrncost, idgrn, kodecost, matauang, kurs, jumlah, rekdebit, rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, proyek, urutan, idprcost, idcscost, idrqcost, idbscost, idpocost, idipccost, jumlahri, statusri, jumlahbayar, statusbayar, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Hapus batch ketika update
                If (isUpdate) Then
                    sql = "Delete from M1_No_Batch_Transaction  where nbtidtransaksi  = '" & result(4) & "' AND nbtsumber = 'GRN'"
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
                    sql = "Delete from M1_No_Serial_Transaction where nstidtransaksi  = '" & result(4) & "' AND nstsumber = 'GRN'"
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
                    sql = "Delete from M7_Asset_Transaction where atidutama  = '" & result(4) & "' AND atsumber = 'GRN'"
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

                If drutama("grnstatus") = 2 Then

                    'UPDATE OUTSTANDING TRANSAKSI ======================================================
                    If Len(updNilaiPO) > 0 Then 'PO
                        'UPDATE DETAIL
                        sql = "UPDATE m4_po_detail SET jmlrealisasi = (CASE idpodetail " & updNilaiPO & " ELSE jmlrealisasi END) WHERE " & updFilterPO
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idpo FROM m4_po_detail WHERE " & updFilterPO & " GROUP BY idpo", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idpo = '" & dr1("idpo") & "')")
                            Next
                        End If
                        'dtOut = AsDataTableAmbilDariDBCon("SELECT idpo, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m4_po_detail WHERE " & ftDetail & " GROUP BY idpo", myConn)
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idpo, SUM(jmlbarang) as jmlbarang, SUM((CASE jmlbarang WHEN 0 THEN -1 ELSE jmlrealisasi END)) as jmlrealisasi FROM m4_po_detail WHERE " & ftDetail & " GROUP BY idpo", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiPO = "" : updFilterPO = ""
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
                                updNilaiPO = String.Concat(updNilaiPO, "WHEN '" & dr1("idpo") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterPO = IIf(Len(updFilterPO.ToString) = 0, "", updFilterPO & " OR ")
                                updFilterPO = String.Concat(updFilterPO, "(poid = '" & dr1("idpo") & "')")
                            Next

                            sql = "UPDATE m4_po SET postatusrealisasi = (CASE poid " & updNilaiPO & " ELSE postatusrealisasi END) WHERE " & updFilterPO
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


                    'INSERT NO ASSET ===============================================================
                    If dtasset.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder
                        For Each dr1 As DataRow In dtasset.Rows
                            'QUERY INSERT NO ASSET IN
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            strValue2.Append("('" & 0 & "', '" & FixQuotes(dr1("atkode")) & "', '" & FixQuotes(dr1("atnama")) & "', '" & FixQuotes(dr1("atkategori")) & "', '" & FixQuotes(dr1("atcabang")) & "', '" & FixQuotes(dr1("atlokasi")) & "', '" & FixQuotes(dr1("atgudang")) & "', '" & FixQuotes(dr1("atdivisi")) & "', '" & FixQuotes(dr1("atsubdivisi")) & "', '" & FixQuotes(dr1("atcostcenter")) & "', '" & FixQuotes(dr1("atproyek")) & "', '" & FixQuotes(dr1("atcatatan")) & "', '" & FixQuotes(dr1("atnomor")) & "', '" & FixQuotes(AsFormatTanggal(dr1("attglbeli"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("attglpakai"))) & "', '" & FixDouble(dr1("atjml")) & "', '" & FixQuotes(dr1("atsatuan")) & "', '" & FixQuotes(dr1("atmatauang")) & "', '" & FixDouble(dr1("atkurs")) & "', '" & FixDouble(dr1("atharga")) & "', '" & FixQuotes(dr1("atdiskon")) & "', '" & FixDouble(dr1("atjmldiskon")) & "', '" & FixQuotes(dr1("atpajak1")) & "', '" & FixDouble(dr1("atjmlpajak1")) & "', '" & FixQuotes(dr1("atpajak2")) & "', '" & FixDouble(dr1("atjmlpajak2")) & "', '" & FixDouble(dr1("athargabeli")) & "', '" & FixDouble(dr1("atnilairesidu")) & "', '" & FixDouble(dr1("atumurekonomis")) & "', '" & FixDouble(dr1("atbebanperbln")) & "', '" & FixDouble(dr1("atakumulasibeban")) & "', '" & FixDouble(dr1("atnilaibuku")) & "', " & dr1("atmetode") & ", '" & FixQuotes(dr1("attabelpenyusutan")) & "', " & dr1("atintangible") & ", " & dr1("atfiskal") & ", " & dr1("atatastengahbulan") & ", '" & FixQuotes(dr1("atrekasset")) & "', '" & FixQuotes(dr1("atrekakumdepresiasi")) & "', '" & FixQuotes(dr1("atrekdepresiasi")) & "', '" & FixQuotes(dr1("atrekpenghapusan")) & "', '" & FixQuotes(dr1("atprodusen")) & "', '" & FixQuotes(AsFormatTanggal(dr1("attglpensiun"))) & "', '" & FixDouble(dr1("atpenyusutanke")) & "', '" & FixDouble(dr1("atnilaimenurun")) & "', " & dr1("atdispose") & ", " & dr1("atpembelian") & ", " & dr1("atpenjualan") & ", " & dr1("atlocked") & ", " & dr1("atstatus") & ", " & dr1("atstatussebelumnya") & ", " & dr1("atisclose") & ", '" & FixQuotes(dr1("atinputuser")) & "', '" & FixQuotes(AsFormatTanggal(dr1("atinputtgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(dr1("atmodifikasiuser")) & "', '" & FixQuotes(AsFormatTanggal(dr1("atmodifikasitgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(dr1("atcustomtext1")) & "', '" & FixQuotes(dr1("atcustomtext2")) & "', '" & FixQuotes(dr1("atcustomtext3")) & "', '" & FixQuotes(dr1("atcustomtext4")) & "', '" & FixQuotes(dr1("atcustomtext5")) & "', " & dr1("atcustomint1") & ", " & dr1("atcustomint2") & ", " & dr1("atcustomint3") & ", " & dr1("atcustomint4") & ", " & dr1("atcustomint5") & ", '" & FixDouble(dr1("atcustomdbl1")) & "', '" & FixDouble(dr1("atcustomdbl2")) & "', '" & FixDouble(dr1("atcustomdbl3")) & "', '" & FixDouble(dr1("atcustomdbl4")) & "', '" & FixDouble(dr1("atcustomdbl5")) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate4"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate5"))) & "', '" & FixQuotes(dr1("atidbarang")) & "')")
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


                    'UPDATE STOK BOOKING ============================================================
                    If Len(updStokOutBooking) > 0 Then
                        sql = "INSERT INTO m1_item_booking_po (idbarang, gudang, jmlbooking) VALUES " & updStokOutBooking & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF UPDATE STOK BOOKING =====================================================


                    'AMBIL DATA DETAIL YANG BARU ++++++++++++++++++++++++++++++++++++++++++++++++++++
                    'Dim dtDetailNew As DataTable = AsDataTableAmbilDariDBCon("SELECT grnd.idgrndetail, grnd.idbarang, grnd.namabarang, grnd.tipebarang, grnd.jml, grnd.satuan, grnd.jmlbarang, grnd.satuanbarang, grnd.matauang, grnd.kurs, grnd.harga, grnd.diskon, grnd.jmldiskon, grnd.gudang, grnd.catatan, grnd.costcenter, grnd.divisi, grnd.subdivisi, grnd.proyek, grn.grninputtgl, i.bhpp, grnd.jmlpajak1, grnd.jmlpajak2 FROM m4_grn_detail grnd JOIN m4_grn grn ON grnd.idgrn = grn.grnid JOIN m1_item i ON grnd.idbarang = i.bid WHERE grnd.idgrn = '" & result(4) & "' ORDER BY grnd.urutan", myConn)
                    Dim dtDetailNew As DataTable = AsDataTableAmbilDariDBCon("SELECT grnd.idgrndetail, grnd.idbarang, grnd.namabarang, grnd.tipebarang, grnd.jml, grnd.satuan, grnd.jmlbarang, grnd.satuanbarang, grnd.matauang, grnd.kurs, grnd.harga, grnd.diskon, grnd.jmldiskon, grnd.gudang, grnd.catatan, grnd.costcenter, grnd.divisi, grnd.subdivisi, grnd.proyek, grn.grninputtgl, i.bhpp, grnd.jmlpajak1, grnd.jmlpajak2, (CASE LENGTH(IFNULL(cc.ccakun,'')) WHEN 0 THEN 1 ELSE 0 END) as transbarang FROM m4_grn_detail grnd JOIN m4_grn grn ON grnd.idgrn = grn.grnid JOIN m1_item i ON grnd.idbarang = i.bid LEFT JOIN m1_cost_center cc ON grnd.costcenter = cc.cckode WHERE grnd.idgrn = '" & result(4) & "' ORDER BY grnd.urutan", myConn)

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
                                    strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("grncabang")) & "', '" & FixQuotes(drutama("grnlokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', " & drutama("grnkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("grnsumber")) & "', " & result(4) & ", " & dr1("idgrndetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("grntgl"))) & "', " & drutama("grnsupplier") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & 0 & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("grnuraian")) & "', '" & FixQuotes(drutama("grncatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr1("grninputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("grninputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
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
                                    If drutama("grnhargatermasukpajak") = 0 Then
                                        sql = "UPDATE m1_item LEFT JOIN m0_setting ON smodule = 0 AND sgrup = 'options' AND skode = 'PembelianUpdateHargaBeli' SET bstok = '" & FixDouble(saldojml) & "', bhargabeli = (CASE IFNULL(snilai,0) WHEN 1 THEN '" & FixDouble((Double.Parse(dr1("harga")) * Double.Parse(dr1("kurs"))) - ((Double.Parse(dr1("jmldiskon")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs"))) + ((Double.Parse(dr1("jmlpajak1")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs"))) + ((Double.Parse(dr1("jmlpajak2")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs")))) & "' ELSE bhargabeli END), baktiftgl = '" & drutama("grntgl") & "' WHERE bid = '" & idbarang & "'"
                                    Else
                                        sql = "UPDATE m1_item LEFT JOIN m0_setting ON smodule = 0 AND sgrup = 'options' AND skode = 'PembelianUpdateHargaBeli' SET bstok = '" & FixDouble(saldojml) & "', bhargabeli = (CASE IFNULL(snilai,0) WHEN 1 THEN '" & FixDouble((Double.Parse(dr1("harga")) * Double.Parse(dr1("kurs"))) - ((Double.Parse(dr1("jmldiskon")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs")))) & "' ELSE bhargabeli END), baktiftgl = '" & drutama("grntgl") & "' WHERE bid = '" & idbarang & "'"
                                    End If
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
                Dim sumber As String = "GRN", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("grnstatus") = 2 Then
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
    Public Function M4_GrnUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("grnsupplierkode", "c1.kkode")
            Filter = Filter.Replace("grnsuppliernama", "c1.knama")
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
            Dim sumber As String = "GRN", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Grntgl, Grnnotransaksi, Grnstatus FROM M4_Grn WHERE Grnid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Grnstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m4_grn_history
            Dim rsSimpanHistory As String = SimpanHistory.m4_Grn_HistorySimpan("" & paramSplit(0) & "★M4_Grn_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m4_grn_terkait")
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


                'UPDATE STOK DAN OUTSTANDING ====================================================
                Dim ftHppI As String = "", ftHppF As String = ""
                Dim ftExistStok As String = "", ftStok As String = ""
                Dim updNilaiPO As String = "", updFilterPO As String = ""
                Dim updStokOut As String = "", gudangOut As String = "", updStokInBooking As String = ""
                Dim updStokBarang As String = "", ftStokBarang As String = ""
                Dim idbarang As Integer = 0, idgrndetail As Integer = 0, idpodetail As Integer = 0, jmlbarang As Double = 0

                'AMBIL DATA DETAIL
                'dtdetail = AsDataTableAmbilDariDBCon("SELECT idgrndetail, idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idpodetail, gudang, urutan FROM m4_grn_detail WHERE idgrn = '" & idtransaksi & "'", myConn)
                dtdetail = AsDataTableAmbilDariDBCon("SELECT idgrndetail, idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idpodetail, gudang, urutan, (CASE LENGTH(IFNULL(cc.ccakun,'')) WHEN 0 THEN 1 ELSE 0 END) as transbarang FROM m4_grn_detail grnd LEFT JOIN m1_cost_center cc ON grnd.costcenter = cc.cckode WHERE idgrn = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        '1. SET NILAI
                        idbarang = dr1("idbarang") : idgrndetail = dr1("idgrndetail") : idpodetail = dr1("idpodetail") : jmlbarang = dr1("jmlbarang") : gudangOut = dr1("gudang")

                        '2. BUAT FILTER CEK HPP KHUSUS(I)
                        ftHppI = IIf(Len(ftHppI.ToString) = 0, "", ftHppI & " OR ")
                        ftHppI = String.Concat(ftHppI, "(idbarang = '" & idbarang & "' AND idtransaksi = '" & idgrndetail & "' AND sumber = 'GRN')")

                        '3. BUAT FILER CEK HPP FIFO(F)
                        ftHppF = IIf(Len(ftHppF.ToString) = 0, "", ftHppF & " OR ")
                        ftHppF = String.Concat(ftHppF, "(cfiidbarang = '" & idbarang & "' AND cfiidtransaksi = '" & idgrndetail & "' AND cfisumber = 'GRN')")

                        '4. BUAT FILTER CEK STOCK EXIST
                        If dr1("transbarang") = 1 Then
                            ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                            ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                            '5. BUAT FILTER CEK JML STOCK
                            Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudang='" & gudangOut & "' AND transbarang = 1")
                            ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
                            'ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")
                            ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > isw.stok) ")

                            '6. SET NILAI UPDATE STOK KELUAR
                            updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
                            updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok
                        End If

                        '7. BUAT FILTER UPDATE OUTSTANDING
                        If idpodetail <> 0 Then
                            '7.1 SET NILAI UPDATE OUTSTANDING
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idpodetail=" & idpodetail)
                            updNilaiPO = String.Concat("WHEN '" & idpodetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiPO)

                            '7.2. SET FILTERUPDATE OUTSTANDING
                            updFilterPO = IIf(Len(updFilterPO.ToString) = 0, "", updFilterPO & " OR ")
                            updFilterPO = String.Concat(updFilterPO, "(idpodetail = '" & idpodetail & "')")

                            'SET NILAI UPDATE STOK BOOKING MASUK
                            updStokInBooking = IIf(Len(updStokInBooking.ToString) = 0, "", updStokInBooking & ", ")
                            updStokInBooking = String.Concat(updStokInBooking, "('" & idbarang & "', '" & gudangOut & "', ('" & jmlbarang & "'))") ' idbarang, kgudang, stok
                        End If

                        '8 SET NILAI UPDATE STOK BARANG
                        If dr1("transbarang") = 1 Then
                            Dim stokBarang As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND transbarang = 1")
                            updStokBarang = String.Concat("WHEN '" & idbarang & "' THEN ROUND(bstok - '" & stokBarang & "', 5) ", updStokBarang)

                            '9. SET FILTERUPDATE STOK BARANG
                            ftStokBarang = IIf(Len(ftStokBarang.ToString) = 0, "", ftStokBarang & " OR ")
                            ftStokBarang = String.Concat(ftStokBarang, "(bid = '" & idbarang & "')")
                        End If

                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                'VALIDASI HPP, STOK ==========================================================
                'ValidasiSimpan
                Dim rsValidasi As String = ValidasiSimpan(dtdetail, "", "", ftHppI, ftHppF, ftExistStok, ftStok, "", "")
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'END OF VALIDASI HPP, STOK ===================================================


                If Len(updFilterPO) > 0 Then
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m4_po_detail SET jmlrealisasi = (CASE idpodetail " & updNilaiPO & " ELSE jmlrealisasi END) WHERE " & updFilterPO
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idpo FROM m4_po_detail WHERE " & updFilterPO & " GROUP BY idpo", myConn)
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idpo = '" & dr1("idpo") & "')")
                        Next
                    End If
                    'dtOut = AsDataTableAmbilDariDBCon("SELECT idpo, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m4_po_detail WHERE " & ftDetail & " GROUP BY idpo", myConn)
                    dtOut = AsDataTableAmbilDariDBCon("SELECT idpo, SUM(jmlbarang) as jmlbarang, SUM((CASE jmlbarang WHEN 0 THEN -1 ELSE jmlrealisasi END)) as jmlrealisasi FROM m4_po_detail WHERE " & ftDetail & " GROUP BY idpo", myConn)
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiPO = "" : updFilterPO = ""
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
                            updNilaiPO = String.Concat(updNilaiPO, "WHEN '" & dr1("idpo") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterPO = IIf(Len(updFilterPO.ToString) = 0, "", updFilterPO & " OR ")
                            updFilterPO = String.Concat(updFilterPO, "(poid = '" & dr1("idpo") & "')")
                        Next

                        sql = "UPDATE m4_po SET postatusrealisasi = (CASE poid " & updNilaiPO & " ELSE postatusrealisasi END) WHERE " & updFilterPO
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
                'END OF UPDATE STOK DAN OUTSTANDING ===========================================


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


                'DELETE NO ASSET IN MASUK --------------------------
                sql = "DELETE a FROM m7_asset_transaction atr JOIN m4_grn grn ON atr.atsumber = grn.grnsumber AND atr.atidutama = grn.grnid AND grn.grnid = '" & idtransaksi & "' JOIN m7_asset a ON atr.atkode = a.akode"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()


                'UPDATE STOK BOOKING ============================================================
                If Len(updStokInBooking) > 0 Then
                    sql = "INSERT INTO m1_item_booking_po (idbarang, gudang, jmlbooking) VALUES " & updStokInBooking & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE STOK BOOKING =====================================================


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
                'sql = "  UPDATE m1_item i"
                'sql &= " JOIN m4_grn_detail grnd ON i.bid = grnd.idbarang AND grnd.idgrn = '" & FixDouble(idtransaksi) & "'"
                'sql &= " LEFT JOIN"
                'sql &= " (SELECT i.bid as idbarang, ROUND(SUM(it.jmlbarang * it.hpp) / SUM(it.jmlbarang),2) as hppaverage"
                'sql &= " FROM m1_item_transaction it"
                'sql &= " JOIN m1_item i ON it.idbarang = i.bid AND it.jenismutasi = 1"
                'sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1"
                'sql &= " JOIN m4_grn_detail grnd ON it.idbarang = grnd.idbarang AND grnd.idgrn = '" & FixDouble(idtransaksi) & "'"
                'sql &= " JOIN m4_grn grn ON grnd.idgrn = grn.grnid AND CONCAT(it.sumber,it.idutama) <> CONCAT(grn.grnsumber,grn.grnid)"
                'sql &= " GROUP BY it.idbarang) as h ON i.bid = h.idbarang"
                'sql &= " SET i.bhppaverage = (CASE i.bjenis WHEN 'P' THEN (CASE i.bstok WHEN 0 THEN 0 ELSE IFNULL(h.hppaverage,0) END) ELSE IFNULL(h.hppaverage,0) END)"

                Dim dtTotalFungsional As DataTable = AsDataTableAmbilDariDBCon("SELECT SUM((CASE grn.grnhargatermasukpajak WHEN 0 THEN ((grnd.jml * grnd.harga) - grnd.jmldiskon) * grnd.kurs ELSE ((grnd.jml * grnd.harga) - grnd.jmldiskon - grnd.jmlpajak1 - grnd.jmlpajak2) * grnd.kurs END)) as total FROM m4_grn_detail grnd JOIN m4_grn grn ON grnd.idgrn = grn.grnid WHERE grnd.idgrn = '" & FixDouble(idtransaksi) & "'", myConn)
                Dim dtBiayaFungsional As DataTable = AsDataTableAmbilDariDBCon("SELECT IFNULL(SUM(grnc.jumlah * grnc.kurs),0) as biaya FROM m4_grn grn LEFT JOIN m4_grn_cost grnc ON grn.grnid = grnc.idgrn AND grnc.termasukhpp = 1 WHERE grn.grnid = '" & FixDouble(idtransaksi) & "'", myConn)
                Dim vTotalFungsional As Double = 0, vBiayaFungsional As Double = 0
                If dtTotalFungsional.Rows.Count > 0 Then
                    vTotalFungsional = Double.Parse(FixDouble(FxDB(dtTotalFungsional.Rows(0)("total"), 0)))
                End If
                If dtBiayaFungsional.Rows.Count > 0 Then
                    vBiayaFungsional = Double.Parse(FixDouble(FxDB(dtBiayaFungsional.Rows(0)("biaya"), 0)))
                End If

                sql = "  UPDATE m1_item i"
                sql &= " JOIN ("
                sql &= " SELECT grnd.idbarang, "
                sql &= " ROUND((CASE " & FixDouble(vTotalFungsional) & " "
                sql &= " WHEN 0 THEN (SUM((CASE grn.grnhargatermasukpajak WHEN 0 THEN ((grnd.jml * grnd.harga) - grnd.jmldiskon) * grnd.kurs ELSE ((grnd.jml * grnd.harga) - grnd.jmldiskon - grnd.jmlpajak1 - grnd.jmlpajak2) * grnd.kurs END))) "
                sql &= " ELSE (SUM((CASE grn.grnhargatermasukpajak WHEN 0 THEN ((grnd.jml * grnd.harga) - grnd.jmldiskon) * grnd.kurs ELSE ((grnd.jml * grnd.harga) - grnd.jmldiskon - grnd.jmlpajak1 - grnd.jmlpajak2) * grnd.kurs END))) "
                sql &= " + (((SUM((CASE grn.grnhargatermasukpajak WHEN 0 THEN ((grnd.jml * grnd.harga) - grnd.jmldiskon) * grnd.kurs ELSE ((grnd.jml * grnd.harga) - grnd.jmldiskon - grnd.jmlpajak1 - grnd.jmlpajak2) * grnd.kurs END))) "
                sql &= " / " & FixDouble(vTotalFungsional) & ") * " & FixDouble(vBiayaFungsional) & ") END), 2) as nilai, "
                sql &= " SUM(grnd.jmlbarang) as jumlah "
                sql &= " FROM m4_grn_detail grnd "
                sql &= " JOIN m4_grn grn ON grnd.idgrn = grn.grnid "
                sql &= " WHERE grnd.idgrn = '" & FixDouble(idtransaksi) & "'"
                sql &= " GROUP BY grnd.idbarang"
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
            sql = "UPDATE M4_Grn SET Grnstatus = " & nilaiStatus & ", Grnmodifikasiuser='" & userid & "', Grnmodifikasitgl = NOW(), Grnposting = 0, Grnpostingtgl = '1971-01-01 00:00:00', Grnjmlrevisi = Grnjmlrevisi + 1 WHERE Grnid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_GrnSearch(PostWsSearch(paramSplit(0), "M4_GrnSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_GrnDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("grnsupplierkode", "c1.kkode")
            Filter = Filter.Replace("grnsuppliernama", "c1.knama")
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
            Dim sumber As String = "GRN", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Grnid, Grnnotransaksi FROM M4_Grn WHERE Grnid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT grncabang, grnlokasi, grnsumber, grnautonotransaksi, grnnotransaksi, grntgl"
            sql &= " FROM M4_grn"
            sql &= " WHERE grnid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("grncabang")
                lokasi = dtNomorNext.Rows(0)("grnlokasi")
                sumber = dtNomorNext.Rows(0)("grnsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("grnautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("grnnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("grntgl"))
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


            'DELETE COST
            sql = "DELETE FROM M4_grn_Cost WHERE idgrn ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE DETAIL
            sql = "DELETE FROM M4_Grn_Detail WHERE idgrn = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE UTAMA
            sql = "DELETE FROM M4_Grn WHERE grnid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_GrnSearch(PostWsSearch(paramSplit(0), "M4_GrnSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_GrnGetdataById(ByVal param As String) As String

        'M4_GrnGetdataById Utama --------------------------------------------------------
        'grnid, grncabang, grnlokasi, grngudang, grnasalbarang, grnasalbarangkategori, grnjenispembelian, 
        'grnjenispembeliankategori, grncarabayar, grnsumber, grnautonotransaksi, grnnotransaksi, grntgl, grnkodepa, 
        'grnsupplier, grnsupplierkontak, grn1alamat1, grn1alamat2, grn1alamat3, grn2alamat1, grn2alamat2, 
        'grn2alamat3, grnbagianpembelian, grntermin, grntgljatuhtempo, grnuraian, grncatatan, grnnoref, 
        'grntglnoref, grntglpenutupan, grnmatauang, grnkurs, grnhargatermasukpajak, grntotal, grndiskonpersen, 
        'grnjmldiskon, grntotalpajak1detail, grntotalpajak2detail, grnbiayalainpersen, grnbiayalain, grntotaltransaksi, grnjmlbayar, 
        'grnrekdiskon, grnrekpajak1, grnrekpajak2, grnrekbiayalain, grnrekbayar, grnidpr, grnidcs, 
        'grnidrq, grnidbs, grnidpo, grnidipc, grnstatusri, grnstatusdnr, grnstatusprt, 
        'grnstatusrealisasi, grnstatus, grnstatussebelumnya, grnjmlrevisi, grncetakanke, grninputuser, grninputtgl, 
        'grnmodifikasiuser, grnmodifikasitgl, grnposting, grnpostingtgl, grntutupperiode, grnisclose, grncustomtext1, 
        'grncustomtext2, grncustomtext3, grncustomtext4, grncustomtext5, grncustomint1, grncustomint2, grncustomint3, 
        'grncustomdbl1, grncustomdbl2, grncustomdbl3, grncustomdate1, grncustomdate2, grncustomdate3, grncabangnama, 
        'grnlokasinama, grngudangnama, grnsupplierkode, grnsuppliernama, grnbagianpembeliankode, grnbagianpembeliannama, grnterminnama, 
        'grnterminharijatuhtempo, grnrekdiskonnama, grnrekpajak1nama, grnrekpajak2nama, grnrekbiayalainnama, grnrekbayarnama, grnnotransaksipr, 
        'grnnotransaksics, grnnotransaksirq, grnnotransaksibs, grnnotransaksipo, grnnotransaksiipc, grnstatusnama, grnstatussebelumnyanama, 
        'grninputusernama, grnmodifikasiusernama, kpkp

        'M4_GrnGetdataById Detail -------------------------------------------------------
        'idgrndetail, idgrn, idbarang, namabarang, tipebarang, 
        'jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, 
        'hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, 
        'jmlpajak2, cabang, lokasi, gudang, rekpersediaan, rekdiskonpembelian, rekhutangsementara, 
        'costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, 
        'idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, jmlri, statusri, 
        'jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, 
        'pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, 
        'costcenternama, divisinama, subdivisinama, proyeknama, ponotransaksi, ipcnotransaksi, 
        'bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan

        'M4_GrnGetdataById Batch --------------------------------------------------------
        'nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang

        'M4_GrnGetdataById Serial --------------------------------------------------------
        'nstid, nstjenismutasi, nstidserialin, nstgudang,  nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, 
        'nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, 
        'nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang

        'M4_GrnGetdataById Cost --------------------------------------------------------
        'idgrncost, idgrn, kodecost, matauang, kurs, jumlah, rekdebit, 
        'rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, 
        'proyek, urutan, idprcost, idcscost, idrqcost, idbscost, idpocost, 
        'idipccost, jumlahri, statusri, jumlahbayar, statusbayar, isclose, customtext1, 
        'customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3, kodecostnama, rekdebitnama, rekkreditnama, kontakkode, kontaknama, costcenternama, 
        'divisinama, subdivisinama

        'M4_GrnGetdataById Asset --------------------------------------------------------
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

        Dim utama As String = "", detail As String = "", batch As String = "", serial As String = "", cost As String = "", idtransaksi As String = ""
        Dim sumber As String = "GRN", asset As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M4_Grn~M4_Grn_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "grnid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "grnid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m4_grn_getdata")
        sql = "select `grn`.`grnid` AS `grnid`,`grn`.`grncabang` AS `grncabang`,`grn`.`grnlokasi` AS `grnlokasi`,`grn`.`grngudang` AS `grngudang`,`grn`.`grnasalbarang` AS `grnasalbarang`,`grn`.`grnasalbarangkategori` AS `grnasalbarangkategori`,`grn`.`grnjenispembelian` AS `grnjenispembelian`,`grn`.`grnjenispembeliankategori` AS `grnjenispembeliankategori`,`grn`.`grncarabayar` AS `grncarabayar`,`grn`.`grnsumber` AS `grnsumber`,`grn`.`grnautonotransaksi` AS `grnautonotransaksi`,`grn`.`grnnotransaksi` AS `grnnotransaksi`,`grn`.`grntgl` AS `grntgl`,`grn`.`grnkodepa` AS `grnkodepa`,`grn`.`grnsupplier` AS `grnsupplier`,`grn`.`grnsupplierkontak` AS `grnsupplierkontak`,`grn`.`grn1alamat1` AS `grn1alamat1`,`grn`.`grn1alamat2` AS `grn1alamat2`,`grn`.`grn1alamat3` AS `grn1alamat3`,`grn`.`grn2alamat1` AS `grn2alamat1`,`grn`.`grn2alamat2` AS `grn2alamat2`,`grn`.`grn2alamat3` AS `grn2alamat3`,`grn`.`grnbagianpembelian` AS `grnbagianpembelian`,`grn`.`grntermin` AS `grntermin`,`grn`.`grntgljatuhtempo` AS `grntgljatuhtempo`,`grn`.`grnuraian` AS `grnuraian`,`grn`.`grncatatan` AS `grncatatan`,`grn`.`grnnoref` AS `grnnoref`,`grn`.`grntglnoref` AS `grntglnoref`,`grn`.`grntglpenutupan` AS `grntglpenutupan`,`grn`.`grnmatauang` AS `grnmatauang`,`grn`.`grnkurs` AS `grnkurs`,`grn`.`grnhargatermasukpajak` AS `grnhargatermasukpajak`,`grn`.`grntotal` AS `grntotal`,`grn`.`grndiskonpersen` AS `grndiskonpersen`,`grn`.`grnjmldiskon` AS `grnjmldiskon`,`grn`.`grntotalpajak1detail` AS `grntotalpajak1detail`,`grn`.`grntotalpajak2detail` AS `grntotalpajak2detail`,`grn`.`grnbiayalainpersen` AS `grnbiayalainpersen`,`grn`.`grnbiayalain` AS `grnbiayalain`,`grn`.`grntotaltransaksi` AS `grntotaltransaksi`,`grn`.`grnjmlbayar` AS `grnjmlbayar`,`grn`.`grnrekdiskon` AS `grnrekdiskon`,`grn`.`grnrekpajak1` AS `grnrekpajak1`,`grn`.`grnrekpajak2` AS `grnrekpajak2`,`grn`.`grnrekbiayalain` AS `grnrekbiayalain`,`grn`.`grnrekbayar` AS `grnrekbayar`,`grn`.`grnidpr` AS `grnidpr`,`grn`.`grnidcs` AS `grnidcs`,`grn`.`grnidrq` AS `grnidrq`,`grn`.`grnidbs` AS `grnidbs`,`grn`.`grnidpo` AS `grnidpo`,`grn`.`grnidipc` AS `grnidipc`,`grn`.`grnstatusri` AS `grnstatusri`,`grn`.`grnstatusdnr` AS `grnstatusdnr`,`grn`.`grnstatusprt` AS `grnstatusprt`,`grn`.`grnstatusrealisasi` AS `grnstatusrealisasi`,`grn`.`grnstatus` AS `grnstatus`,`grn`.`grnstatussebelumnya` AS `grnstatussebelumnya`,`grn`.`grnjmlrevisi` AS `grnjmlrevisi`,`grn`.`grncetakanke` AS `grncetakanke`,`grn`.`grninputuser` AS `grninputuser`,`grn`.`grninputtgl` AS `grninputtgl`,`grn`.`grnmodifikasiuser` AS `grnmodifikasiuser`,`grn`.`grnmodifikasitgl` AS `grnmodifikasitgl`,`grn`.`grnposting` AS `grnposting`,`grn`.`grnpostingtgl` AS `grnpostingtgl`,`grn`.`grntutupperiode` AS `grntutupperiode`,`grn`.`grnisclose` AS `grnisclose`,`grn`.`grncustomtext1` AS `grncustomtext1`,`grn`.`grncustomtext2` AS `grncustomtext2`,`grn`.`grncustomtext3` AS `grncustomtext3`,`grn`.`grncustomtext4` AS `grncustomtext4`,`grn`.`grncustomtext5` AS `grncustomtext5`,`grn`.`grncustomint1` AS `grncustomint1`,`grn`.`grncustomint2` AS `grncustomint2`,`grn`.`grncustomint3` AS `grncustomint3`,`grn`.`grncustomdbl1` AS `grncustomdbl1`,`grn`.`grncustomdbl2` AS `grncustomdbl2`,`grn`.`grncustomdbl3` AS `grncustomdbl3`,`grn`.`grncustomdate1` AS `grncustomdate1`,`grn`.`grncustomdate2` AS `grncustomdate2`,`grn`.`grncustomdate3` AS `grncustomdate3`,`br`.`bnama` AS `grncabangnama`,`lc`.`lnama` AS `grnlokasinama`,`wh`.`wnama` AS `grngudangnama`,`c1`.`kkode` AS `grnsupplierkode`,`c1`.`knama` AS `grnsuppliernama`,`c2`.`kkode` AS `grnbagianpembeliankode`,`c2`.`knama` AS `grnbagianpembeliannama`,`tr`.`trnama` AS `grnterminnama`,`tr`.`trharijatuhtempo` AS `grnterminharijatuhtempo`,`coa1`.`cnama` AS `grnrekdiskonnama`,`coa2`.`cnama` AS `grnrekpajak1nama`,`coa3`.`cnama` AS `grnrekpajak2nama`,`coa4`.`cnama` AS `grnrekbiayalainnama`,`coa5`.`cnama` AS `grnrekbayarnama`,`pr`.`prnotransaksi` AS `grnnotransaksipr`,`cs`.`csnotransaksi` AS `grnnotransaksics`,`rq`.`rqnotransaksi` AS `grnnotransaksirq`,`bs`.`bsnotransaksi` AS `grnnotransaksibs`,`po`.`ponotransaksi` AS `grnnotransaksipo`,`ipc`.`ipcnotransaksi` AS `grnnotransaksiipc`,`st1`.`nama` AS `grnstatusnama`,`st2`.`nama` AS `grnstatussebelumnyanama`,`u1`.`unama` AS `grninputusernama`,`u2`.`unama` AS `grnmodifikasiusernama`,`grnd`.`idgrndetail` AS `idgrndetail`,`grnd`.`idgrn` AS `idgrn`,`grnd`.`idbarang` AS `idbarang`,`grnd`.`namabarang` AS `namabarang`,`grnd`.`tipebarang` AS `tipebarang`,`grnd`.`jml` AS `jml`,`grnd`.`satuan` AS `satuan`,`grnd`.`nilaisatuan` AS `nilaisatuan`,`grnd`.`jmlbarang` AS `jmlbarang`,`grnd`.`satuanbarang` AS `satuanbarang`,`grnd`.`matauang` AS `matauang`,`grnd`.`kurs` AS `kurs`,`grnd`.`hargafix` AS `hargafix`,`grnd`.`harga` AS `harga`,`grnd`.`diskon` AS `diskon`,`grnd`.`jmldiskon` AS `jmldiskon`,`grnd`.`pajak1` AS `pajak1`,`grnd`.`jmlpajak1` AS `jmlpajak1`,`grnd`.`pajak2` AS `pajak2`,`grnd`.`jmlpajak2` AS `jmlpajak2`,`grnd`.`cabang` AS `cabang`,`grnd`.`lokasi` AS `lokasi`,`grnd`.`gudang` AS `gudang`,`i`.`brekpersediaan` AS `rekpersediaan`,`grnd`.`rekdiskonpembelian` AS `rekdiskonpembelian`,`s`.`snilai` AS `rekhutangsementara`,`grnd`.`costcenter` AS `costcenter`,`grnd`.`divisi` AS `divisi`,`grnd`.`subdivisi` AS `subdivisi`,`grnd`.`proyek` AS `proyek`,`grnd`.`catatan` AS `catatan`,`grnd`.`urutan` AS `urutan`,`grnd`.`idprdetail` AS `idprdetail`,`grnd`.`idcsdetail` AS `idcsdetail`,`grnd`.`idrqdetail` AS `idrqdetail`,`grnd`.`idbsdetail` AS `idbsdetail`,`grnd`.`idpodetail` AS `idpodetail`,`grnd`.`idipcdetail` AS `idipcdetail`,`grnd`.`jmlri` AS `jmlri`,`grnd`.`statusri` AS `statusri`,`grnd`.`jmldnr` AS `jmldnr`,`grnd`.`statusdnr` AS `statusdnr`,`grnd`.`jmlprt` AS `jmlprt`,`grnd`.`statusprt` AS `statusprt`,`grnd`.`jmlrealisasi` AS `jmlrealisasi`,`grnd`.`statusrealisasi` AS `statusrealisasi`,`grnd`.`isclose` AS `isclose`,`grnd`.`customtext1` AS `customtext1`,`grnd`.`customtext2` AS `customtext2`,`grnd`.`customtext3` AS `customtext3`,`grnd`.`customdbl1` AS `customdbl1`,`grnd`.`customdbl2` AS `customdbl2`,`grnd`.`customdbl3` AS `customdbl3`,`grnd`.`customdate1` AS `customdate1`,`grnd`.`customdate2` AS `customdate2`,`grnd`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bjenis` AS `bjenis`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`i`.`basset` AS `basset`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,`brd`.`bnama` AS `cabangnama`,`lcd`.`lnama` AS `lokasinama`,`whd`.`wnama` AS `gudangnama`,`cc`.`ccnama` AS `costcenternama`,`d`.`dnama` AS `divisinama`,`sd`.`sdnama` AS `subdivisinama`,`p`.`pnama` AS `proyeknama`,`po2`.`ponotransaksi` AS `ponotransaksi`,`ipc2`.`ipcnotransaksi` AS `ipcnotransaksi`, c1.kpkp, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan from (((((((((((((((((((((((((((((((((((((`m4_grn` `grn` join `m4_grn_detail` `grnd` on((`grn`.`grnid` = `grnd`.`idgrn`))) left join `m1_branch` `br` on((`br`.`bkode` = `grn`.`grncabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `grn`.`grnlokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `grn`.`grngudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `grn`.`grnsupplier`))) left join `m1_contact` `c2` on((`c2`.`kid` = `grn`.`grnbagianpembelian`))) left join `m1_terms` `tr` on((`grn`.`grntermin` = `tr`.`trkode`))) left join `m1_coa` `coa1` on((`grn`.`grnrekdiskon` = `coa1`.`cnomor`))) left join `m1_coa` `coa2` on((`grn`.`grnrekpajak1` = `coa2`.`cnomor`))) left join `m1_coa` `coa3` on((`grn`.`grnrekpajak2` = `coa3`.`cnomor`))) left join `m1_coa` `coa4` on((`grn`.`grnrekbiayalain` = `coa4`.`cnomor`))) left join `m1_coa` `coa5` on((`grn`.`grnrekbayar` = `coa5`.`cnomor`))) left join `m4_pr` `pr` on((`grn`.`grnidpr` = `pr`.`prid`))) left join `m4_cs` `cs` on((`grn`.`grnidcs` = `cs`.`csid`))) left join `m4_rq` `rq` on((`grn`.`grnidrq` = `rq`.`rqid`))) left join `m4_bs` `bs` on((`grn`.`grnidbs` = `bs`.`bsid`))) left join `m4_po` `po` on((`grn`.`grnidpo` = `po`.`poid`))) left join `m4_ipc` `ipc` on((`grn`.`grnidipc` = `ipc`.`ipcid`))) left join `m0_status` `st1` on((`st1`.`kode` = `grn`.`grnstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `grn`.`grnstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `grn`.`grninputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `grn`.`grnmodifikasiuser`))) left join `m1_item` `i` on((`i`.`bid` = `grnd`.`idbarang`))) left join `m1_tax` `t1` on((`grnd`.`pajak1` = `t1`.`tkode`))) left join `m1_tax` `t2` on((`grnd`.`pajak2` = `t2`.`tkode`))) left join `m1_branch` `brd` on((`grnd`.`cabang` = `brd`.`bkode`))) left join `m1_location` `lcd` on((`grnd`.`lokasi` = `lcd`.`lkode`))) left join `m1_warehouse` `whd` on((`grnd`.`gudang` = `whd`.`wkode`))) left join `m1_cost_center` `cc` on((`grnd`.`costcenter` = `cc`.`cckode`))) left join `m1_division` `d` on((`grnd`.`divisi` = `d`.`dkode`))) left join `m1_subdivision` `sd` on((`grnd`.`subdivisi` = `sd`.`sdkode`))) left join `m1_project` `p` on((`grnd`.`proyek` = `p`.`pkode`))) left join `m4_po_detail` `pod` on((`grnd`.`idpodetail` = `pod`.`idpodetail`))) left join `m4_po` `po2` on((`pod`.`idpo` = `po2`.`poid`))) left join `m4_ipc_detail` `ipcd` on((`grnd`.`idipcdetail` = `ipcd`.`idipcdetail`))) left join `m4_ipc` `ipc2` on((`ipcd`.`idipc` = `ipc2`.`ipcid`))) left join `m0_setting` `s` on(((`s`.`smodule` = 0) and (`s`.`sgrup` = 'akun') and (`s`.`skode` = 'HutangSementara'))))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("grnid"), 0), sptField,
                     FxDB(drutama("grncabang"), ""), sptField,
                     FxDB(drutama("grnlokasi"), ""), sptField,
                     FxDB(drutama("grngudang"), ""), sptField,
                     FxDB(drutama("grnasalbarang"), ""), sptField,
                     FxDB(drutama("grnasalbarangkategori"), 0), sptField,
                     FxDB(drutama("grnjenispembelian"), ""), sptField,
                     FxDB(drutama("grnjenispembeliankategori"), 0), sptField,
                     FxDB(drutama("grncarabayar"), 0), sptField,
                     FxDB(drutama("grnsumber"), ""), sptField,
                     FxDB(drutama("grnautonotransaksi"), 0), sptField,
                     FxDB(drutama("grnnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("grntgl"), ""), formatTgl), sptField,
                     FxDB(drutama("grnkodepa"), 0), sptField,
                     FxDB(drutama("grnsupplier"), 0), sptField,
                     FxDB(drutama("grnsupplierkontak"), ""), sptField,
                     FxDB(drutama("grn1alamat1"), ""), sptField,
                     FxDB(drutama("grn1alamat2"), ""), sptField,
                     FxDB(drutama("grn1alamat3"), ""), sptField,
                     FxDB(drutama("grn2alamat1"), ""), sptField,
                     FxDB(drutama("grn2alamat2"), ""), sptField,
                     FxDB(drutama("grn2alamat3"), ""), sptField,
                     FxDB(drutama("grnbagianpembelian"), 0), sptField,
                     FxDB(drutama("grntermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("grntgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("grnuraian"), ""), sptField,
                     FxDB(drutama("grncatatan"), ""), sptField,
                     FxDB(drutama("grnnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("grntglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("grntglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("grnmatauang"), ""), sptField,
                     FxDB(drutama("grnkurs"), 0), sptField,
                     FxDB(drutama("grnhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("grntotal"), 0), sptField,
                     FxDB(drutama("grndiskonpersen"), ""), sptField,
                     FxDB(drutama("grnjmldiskon"), 0), sptField,
                     FxDB(drutama("grntotalpajak1detail"), 0), sptField,
                     FxDB(drutama("grntotalpajak2detail"), 0), sptField,
                     FxDB(drutama("grnbiayalainpersen"), ""), sptField,
                     FxDB(drutama("grnbiayalain"), 0), sptField,
                     FxDB(drutama("grntotaltransaksi"), 0), sptField,
                     FxDB(drutama("grnjmlbayar"), 0), sptField,
                     FxDB(drutama("grnrekdiskon"), ""), sptField,
                     FxDB(drutama("grnrekpajak1"), ""), sptField,
                     FxDB(drutama("grnrekpajak2"), ""), sptField,
                     FxDB(drutama("grnrekbiayalain"), ""), sptField,
                     FxDB(drutama("grnrekbayar"), ""), sptField,
                     FxDB(drutama("grnidpr"), 0), sptField,
                     FxDB(drutama("grnidcs"), 0), sptField,
                     FxDB(drutama("grnidrq"), 0), sptField,
                     FxDB(drutama("grnidbs"), 0), sptField,
                     FxDB(drutama("grnidpo"), 0), sptField,
                     FxDB(drutama("grnidipc"), 0), sptField,
                     FxDB(drutama("grnstatusri"), 0), sptField,
                     FxDB(drutama("grnstatusdnr"), 0), sptField,
                     FxDB(drutama("grnstatusprt"), 0), sptField,
                     FxDB(drutama("grnstatusrealisasi"), 0), sptField,
                     FxDB(drutama("grnstatus"), 0), sptField,
                     FxDB(drutama("grnstatussebelumnya"), 0), sptField,
                     FxDB(drutama("grnjmlrevisi"), 0), sptField,
                     FxDB(drutama("grncetakanke"), 0), sptField,
                     FxDB(drutama("grninputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("grninputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("grnmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("grnmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("grnposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("grnpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("grntutupperiode"), 0), sptField,
                     FxDB(drutama("grnisclose"), 0), sptField,
                     FxDB(drutama("grncustomtext1"), ""), sptField,
                     FxDB(drutama("grncustomtext2"), ""), sptField,
                     FxDB(drutama("grncustomtext3"), ""), sptField,
                     FxDB(drutama("grncustomtext4"), ""), sptField,
                     FxDB(drutama("grncustomtext5"), ""), sptField,
                     FxDB(drutama("grncustomint1"), 0), sptField,
                     FxDB(drutama("grncustomint2"), 0), sptField,
                     FxDB(drutama("grncustomint3"), 0), sptField,
                     FxDB(drutama("grncustomdbl1"), 0), sptField,
                     FxDB(drutama("grncustomdbl2"), 0), sptField,
                     FxDB(drutama("grncustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("grncustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("grncustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("grncustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("grncabangnama"), ""), sptField,
                     FxDB(drutama("grnlokasinama"), ""), sptField,
                     FxDB(drutama("grngudangnama"), ""), sptField,
                     FxDB(drutama("grnsupplierkode"), ""), sptField,
                     FxDB(drutama("grnsuppliernama"), ""), sptField,
                     FxDB(drutama("grnbagianpembeliankode"), ""), sptField,
                     FxDB(drutama("grnbagianpembeliannama"), ""), sptField,
                     FxDB(drutama("grnterminnama"), ""), sptField,
                     FxDB(drutama("grnterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("grnrekdiskonnama"), ""), sptField,
                     FxDB(drutama("grnrekpajak1nama"), ""), sptField,
                     FxDB(drutama("grnrekpajak2nama"), ""), sptField,
                     FxDB(drutama("grnrekbiayalainnama"), ""), sptField,
                     FxDB(drutama("grnrekbayarnama"), ""), sptField,
                     FxDB(drutama("grnnotransaksipr"), ""), sptField,
                     FxDB(drutama("grnnotransaksics"), ""), sptField,
                     FxDB(drutama("grnnotransaksirq"), ""), sptField,
                     FxDB(drutama("grnnotransaksibs"), ""), sptField,
                     FxDB(drutama("grnnotransaksipo"), ""), sptField,
                     FxDB(drutama("grnnotransaksiipc"), ""), sptField,
                     FxDB(drutama("grnstatusnama"), ""), sptField,
                     FxDB(drutama("grnstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("grninputusernama"), ""), sptField,
                     FxDB(drutama("grnmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idgrndetail"), 0), sptField,
                     FxDB(dr("idgrn"), 0), sptField,
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
                     FxDB(dr("rekpersediaan"), ""), sptField,
                     FxDB(dr("rekdiskonpembelian"), ""), sptField,
                     FxDB(dr("rekhutangsementara"), ""), sptField,
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
                     FxDB(dr("jmlri"), 0), sptField,
                     FxDB(dr("statusri"), 0), sptField,
                     FxDB(dr("jmldnr"), 0), sptField,
                     FxDB(dr("statusdnr"), 0), sptField,
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
                     FxDB(dr("gudangnama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("ponotransaksi"), ""), sptField,
                     FxDB(dr("ipcnotransaksi"), ""), sptField,
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

            'AMBIL DATA COST
            sql = "SELECT grnc.idgrncost, grnc.idgrn, grnc.kodecost, grnc.matauang, grnc.kurs, grnc.jumlah, grnc.rekdebit, grnc.rekkredit, grnc.kontak, grnc.termasukhpp, grnc.catatan, grnc.costcenter, grnc.divisi, grnc.subdivisi, grnc.proyek, grnc.urutan, grnc.idprcost, grnc.idcscost, grnc.idrqcost, grnc.idbscost, grnc.idpocost, grnc.idipccost, grnc.jumlahri, grnc.statusri, grnc.jumlahbayar, grnc.statusbayar, grnc.isclose, grnc.customtext1, grnc.customtext2, grnc.customtext3, grnc.customdbl1, grnc.customdbl2, grnc.customdbl3, grnc.customdate1, grnc.customdate2, grnc.customdate3, oc.ocnama AS kodecostnama, coa1.cnama AS rekdebitnama, coa2.cnama AS rekkreditnama, c.kkode AS kontakkode, c.knama AS kontaknama, cc.ccnama AS costcenternama, d.dnama AS divisinama, sd.sddivisi AS subdivisinama FROM m4_grn_cost grnc JOIN m4_grn grn ON grnc.idgrn = grn.grnid LEFT JOIN m1_other_cost oc ON grnc.kodecost = oc.ockode LEFT JOIN m1_coa coa1 ON grnc.rekdebit = coa1.cnomor LEFT JOIN m1_coa coa2 ON grnc.rekkredit = coa2.cnomor LEFT JOIN m1_contact c ON grnc.kontak = c.kid LEFT JOIN m1_cost_center cc ON grnc.costcenter = cc.cckode LEFT JOIN m1_division d ON grnc.divisi = d.dkode LEFT JOIN m1_subdivision sd ON grnc.subdivisi = sd.sdkode"
            Dim dtcost As New DataTable
            dtcost = AmbilData("aplikasi1-m4_grn_cost", Filter, "grnc.urutan", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtcost.Rows
                cost = String.Concat(cost,
                     FxDB(dr("idgrncost"), ""), sptField,
                     FxDB(dr("idgrn"), ""), sptField,
                     FxDB(dr("kodecost"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("jumlah"), 0), sptField,
                     FxDB(dr("rekdebit"), ""), sptField,
                     FxDB(dr("rekkredit"), ""), sptField,
                     FxDB(dr("kontak"), ""), sptField,
                     FxDB(dr("termasukhpp"), 0), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idprcost"), ""), sptField,
                     FxDB(dr("idcscost"), ""), sptField,
                     FxDB(dr("idrqcost"), ""), sptField,
                     FxDB(dr("idbscost"), ""), sptField,
                     FxDB(dr("idpocost"), ""), sptField,
                     FxDB(dr("idipccost"), ""), sptField,
                     FxDB(dr("jumlahri"), 0), sptField,
                     FxDB(dr("statusri"), 0), sptField,
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
                     FxDB(dr("kontakkode"), ""), sptField,
                     FxDB(dr("kontaknama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptRow)
            Next
            If cost.Length > 0 Then cost = cost.Substring(0, cost.Length - sptRow.Length) Else cost = cost

            'AMBIL DATA ASSET
            'sql = "select atr.atid AS atid, atr.atasetid AS atasetid, atr.atjenismutasi AS atjenismutasi, atr.atsumber AS atsumber, atr.atidutama AS atidutama,atr.atidbarang AS atidbarang,atr.atkode AS atkode, atr.atnama AS atnama, atr.atkategori AS atkategori, atr.atcabang AS atcabang, atr.atlokasi AS atlokasi, atr.atgudang AS atgudang,atr.atdivisi AS atdivisi, atr.atsubdivisi AS atsubdivisi, atr.atcostcenter AS atcostcenter, atr.atproyek AS atproyek, atr.atcatatan AS atcatatan, atr.atnomor AS atnomor, atr.attglbeli AS attglbeli, atr.attglpakai AS attglpakai, atr.atjml AS atjml, atr.atsatuan AS atsatuan, atr.atmatauang AS atmatauang, atr.atkurs AS atkurs, atr.atharga AS atharga, atr.atdiskon AS atdiskon, atr.atjmldiskon AS atjmldiskon, atr.atpajak1 AS atpajak1, atr.atjmlpajak1 AS atjmlpajak1, atr.atpajak2 AS atpajak2, atr.atjmlpajak2 AS atjmlpajak2, atr.athargabeli AS athargabeli, atr.atnilairesidu AS atnilairesidu, atr.atumurekonomis AS atumurekonomis, atr.atbebanperbln AS atbebanperbln, atr.atakumulasibeban AS atakumulasibeban, atr.atnilaibuku AS atnilaibuku, (CASE WHEN atr.atnilaibuku < atr.atbebanperbln THEN atr.atnilaibuku ELSE atr.atbebanperbln END) as atnilaipenyusutan, atr.atmetode AS atmetode, atr.attabelpenyusutan AS attabelpenyusutan, atr.atintangible AS atintangible, atr.atfiskal AS atfiskal, atr.atatastengahbulan AS atatastengahbulan, atr.atrekasset AS atrekasset, atr.atrekakumdepresiasi AS atrekakumdepresiasi, atr.atrekdepresiasi AS atrekdepresiasi, atr.atrekpenghapusan AS atrekpenghapusan, atr.atprodusen AS atprodusen, atr.attglpensiun AS attglpensiun, atr.atpenyusutanke AS atpenyusutanke, atr.atnilaimenurun AS atnilaimenurun, atr.atdispose AS atdispose, atr.atpembelian AS atpembelian, atr.atpenjualan AS atpenjualan, atr.atlocked AS atlocked, atr.atstatus AS atstatus, atr.atstatussebelumnya AS atstatussebelumnya, atr.atisclose AS atisclose, atr.atinputuser AS atinputuser, atr.atinputtgl AS atinputtgl, atr.atmodifikasiuser AS atmodifikasiuser, atr.atmodifikasitgl AS atmodifikasitgl, atr.atcustomtext1,atr.atcustomtext2,atr.atcustomtext3,atr.atcustomtext4,atr.atcustomtext5,atr.atcustomint1,atr.atcustomint2,atr.atcustomint3,atr.atcustomint4,atr.atcustomint5,atr.atcustomdbl1,atr.atcustomdbl2,atr.atcustomdbl3,atr.atcustomdbl4,atr.atcustomdbl5,atr.atcustomdate1,atr.atcustomdate2,atr.atcustomdate3,atr.atcustomdate4,atr.atcustomdate5,ac.acnama AS atkategorinama, br.bnama AS atcabangnama, l.lnama AS atlokasinama, w.wnama AS atgudangnama,d.dnama AS atdivisinama, sd.sdnama AS atsubdivisinama, cc.ccnama AS atcostcenternama, p.pnama AS atproyeknama, dc.nama AS atmetodenama, t1.tnama AS atpajak1nama, ifnull(t1.tnilai, 0) AS atpajak1nilai, t2.tnama AS atpajak2nama, ifnull(t2.tnilai, 0) AS atpajak2nilai,coa1.cnama AS atrekassetnama, coa2.cnama AS atrekakumdepresiasinama, coa3.cnama AS atrekdepresiasinama, coa4.cnama AS atrekpenghapusannama, c1.kkode AS atprodusenkode, c1.knama AS atprodusennama,  sp1.nama AS atstatusnama,  sp2.nama AS atstatussebelumnyanama,  u1.unama AS atinputusernama,  u2.unama AS atmodifikasiusernama from m7_asset_transaction atr left join m7_asset_category ac on atr.atkategori = ac.ackode left join m1_branch br on atr.atcabang = br.bkode left join m1_location l on atr.atlokasi = l.lkode left join m1_warehouse w on atr.atgudang = w.wkode left join m1_division d on atr.atdivisi = d.dkode left join m1_subdivision sd on atr.atsubdivisi = sd.sdkode left join m7_depreciation_category dc on atr.atmetode = dc.kode left join m1_coa coa1 on atr.atrekasset = coa1.cnomor left join m1_coa coa2 on atr.atrekakumdepresiasi = coa2.cnomor left join m1_coa coa3 on atr.atrekdepresiasi = coa3.cnomor left join m1_coa coa4 on atr.atrekpenghapusan = coa4.cnomor left join m1_contact c1 on atr.atprodusen = c1.kid left join m0_status_progress sp1 on atr.atstatus = sp1.kode left join m0_status_progress sp2 on atr.atstatussebelumnya = sp2.kode left join m0_user u1 on atr.atinputuser = u1.userid left join m0_user u2 on atr.atmodifikasiuser = u2.userid left join m1_cost_center cc on atr.atcostcenter = cc.cckode left join m1_project p on atr.atproyek = p.pkode left join m1_tax t1 on atr.atpajak1 = t1.tkode left join m1_tax t2 on atr.atpajak2 = t2.tkode"
            sql = "select atr.atid AS atid, atr.atasetid AS atasetid, atr.atjenismutasi AS atjenismutasi, atr.atsumber AS atsumber, atr.atidutama AS atidutama,atr.atidbarang AS atidbarang,atr.atkode AS atkode, atr.atnama AS atnama, atr.atkategori AS atkategori, atr.atcabang AS atcabang, atr.atlokasi AS atlokasi, atr.atgudang AS atgudang,atr.atdivisi AS atdivisi, atr.atsubdivisi AS atsubdivisi, atr.atcostcenter AS atcostcenter, atr.atproyek AS atproyek, atr.atcatatan AS atcatatan, atr.atnomor AS atnomor, atr.attglbeli AS attglbeli, atr.attglpakai AS attglpakai, atr.atjml AS atjml, atr.atsatuan AS atsatuan, atr.atmatauang AS atmatauang, atr.atkurs AS atkurs, atr.atharga AS atharga, atr.atdiskon AS atdiskon, atr.atjmldiskon AS atjmldiskon, atr.atpajak1 AS atpajak1, atr.atjmlpajak1 AS atjmlpajak1, atr.atpajak2 AS atpajak2, atr.atjmlpajak2 AS atjmlpajak2, atr.athargabeli AS athargabeli, atr.atnilairesidu AS atnilairesidu, atr.atumurekonomis AS atumurekonomis, atr.atbebanperbln AS atbebanperbln, atr.atakumulasibeban AS atakumulasibeban, atr.atnilaibuku AS atnilaibuku, (CASE WHEN atr.atnilaibuku < atr.atbebanperbln THEN atr.atnilaibuku ELSE atr.atbebanperbln END) as atnilaipenyusutan, atr.atmetode AS atmetode, atr.attabelpenyusutan AS attabelpenyusutan, atr.atintangible AS atintangible, atr.atfiskal AS atfiskal, atr.atatastengahbulan AS atatastengahbulan, atr.atrekasset AS atrekasset, atr.atrekakumdepresiasi AS atrekakumdepresiasi, atr.atrekdepresiasi AS atrekdepresiasi, atr.atrekpenghapusan AS atrekpenghapusan, atr.atprodusen AS atprodusen, atr.attglpensiun AS attglpensiun, atr.atpenyusutanke AS atpenyusutanke, atr.atnilaimenurun AS atnilaimenurun, atr.atdispose AS atdispose, atr.atpembelian AS atpembelian, atr.atpenjualan AS atpenjualan, atr.atlocked AS atlocked, atr.atstatus AS atstatus, atr.atstatussebelumnya AS atstatussebelumnya, atr.atisclose AS atisclose, atr.atinputuser AS atinputuser, atr.atinputtgl AS atinputtgl, atr.atmodifikasiuser AS atmodifikasiuser, atr.atmodifikasitgl AS atmodifikasitgl, atr.atcustomtext1,atr.atcustomtext2,atr.atcustomtext3,atr.atcustomtext4,atr.atcustomtext5,atr.atcustomint1,atr.atcustomint2,atr.atcustomint3,atr.atcustomint4,atr.atcustomint5,atr.atcustomdbl1,atr.atcustomdbl2,atr.atcustomdbl3,atr.atcustomdbl4,atr.atcustomdbl5,atr.atcustomdate1,atr.atcustomdate2,atr.atcustomdate3,atr.atcustomdate4,atr.atcustomdate5,ac.acnama AS atkategorinama, br.bnama AS atcabangnama, l.lnama AS atlokasinama, w.wnama AS atgudangnama,d.dnama AS atdivisinama, sd.sdnama AS atsubdivisinama, cc.ccnama AS atcostcenternama, p.pnama AS atproyeknama, dc.nama AS atmetodenama, t1.tnama AS atpajak1nama, ifnull(t1.tnilai, 0) AS atpajak1nilai, t2.tnama AS atpajak2nama, ifnull(t2.tnilai, 0) AS atpajak2nilai,coa1.cnama AS atrekassetnama, coa2.cnama AS atrekakumdepresiasinama, coa3.cnama AS atrekdepresiasinama, coa4.cnama AS atrekpenghapusannama, c1.kkode AS atprodusenkode, c1.knama AS atprodusennama,  sp1.nama AS atstatusnama,  sp2.nama AS atstatussebelumnyanama,  u1.unama AS atinputusernama,  u2.unama AS atmodifikasiusernama, i.bkode AS kodebarang from m7_asset_transaction atr left join m7_asset_category ac on atr.atkategori = ac.ackode left join m1_branch br on atr.atcabang = br.bkode left join m1_location l on atr.atlokasi = l.lkode left join m1_warehouse w on atr.atgudang = w.wkode left join m1_division d on atr.atdivisi = d.dkode left join m1_subdivision sd on atr.atsubdivisi = sd.sdkode left join m7_depreciation_category dc on atr.atmetode = dc.kode left join m1_coa coa1 on atr.atrekasset = coa1.cnomor left join m1_coa coa2 on atr.atrekakumdepresiasi = coa2.cnomor left join m1_coa coa3 on atr.atrekdepresiasi = coa3.cnomor left join m1_coa coa4 on atr.atrekpenghapusan = coa4.cnomor left join m1_contact c1 on atr.atprodusen = c1.kid left join m0_status_progress sp1 on atr.atstatus = sp1.kode left join m0_status_progress sp2 on atr.atstatussebelumnya = sp2.kode left join m0_user u1 on atr.atinputuser = u1.userid left join m0_user u2 on atr.atmodifikasiuser = u2.userid left join m1_cost_center cc on atr.atcostcenter = cc.cckode left join m1_project p on atr.atproyek = p.pkode left join m1_tax t1 on atr.atpajak1 = t1.tkode left join m1_tax t2 on atr.atpajak2 = t2.tkode JOIN m1_item i ON atr.atidbarang = i.bid"
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
                     FxDB(dr("atmodifikasiusernama"), ""), sptField,
                     FxDB(dr("kodebarang"), ""), sptRow)
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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, batch, sptSubParam, serial, sptSubParam, cost, sptSubParam, asset)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("grnid, grncabang, grnlokasi, grngudang, grnasalbarang, grnasalbarangkategori, grnjenispembelian, grnjenispembeliankategori, grncarabayar, grnsumber, grnautonotransaksi, grnnotransaksi, grntgl, grnkodepa, grnsupplier, grnsupplierkontak, grn1alamat1, grn1alamat2, grn1alamat3, grn2alamat1, grn2alamat2, grn2alamat3, grnbagianpembelian, grntermin, grntgljatuhtempo, grnuraian, grncatatan, grnnoref, grntglnoref, grntglpenutupan, grnmatauang, grnkurs, grnhargatermasukpajak, grntotal, grndiskonpersen, grnjmldiskon, grntotalpajak1detail, grntotalpajak2detail, grnbiayalainpersen, grnbiayalain, grntotaltransaksi, grnjmlbayar, grnrekdiskon, grnrekpajak1, grnrekpajak2, grnrekbiayalain, grnrekbayar, grnidpr, grnidcs, grnidrq, grnidbs, grnidpo, grnidipc, grnstatusri, grnstatusdnr, grnstatusprt, grnstatusrealisasi, grnstatus, grnstatussebelumnya, grnjmlrevisi, grncetakanke, grninputuser, grninputtgl, grnmodifikasiuser, grnmodifikasitgl, grnposting, grnpostingtgl, grntutupperiode, grnisclose, grncustomtext1, grncustomtext2, grncustomtext3, grncustomtext4, grncustomtext5, grncustomint1, grncustomint2, grncustomint3, grncustomdbl1, grncustomdbl2, grncustomdbl3, grncustomdate1, grncustomdate2, grncustomdate3, grncabangnama, grnlokasinama, grngudangnama, grnsupplierkode, grnsuppliernama, grnbagianpembeliankode, grnbagianpembeliannama, grnterminnama, grnterminharijatuhtempo, grnrekdiskonnama, grnrekpajak1nama, grnrekpajak2nama, grnrekbiayalainnama, grnrekbayarnama, grnnotransaksipr, grnnotransaksics, grnnotransaksirq, grnnotransaksibs, grnnotransaksipo, grnnotransaksiipc, grnstatusnama, grnstatussebelumnyanama, grninputusernama, grnmodifikasiusernama, kpkp" & sptSubParam & "idgrndetail, idgrn, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, rekpersediaan, rekdiskonpembelian, rekhutangsementara, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, basset, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, ponotransaksi, ipcnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan" & sptSubParam & "nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang" & sptSubParam & "nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang" & sptSubParam & "idgrncost, idgrn, kodecost, matauang, kurs, jumlah, rekdebit, rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, proyek, urutan, idprcost, idcscost, idrqcost, idbscost, idpocost, idipccost, jumlahri, statusri, jumlahbayar, statusbayar, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodecostnama, rekdebitnama, rekkreditnama, kontakkode, kontaknama, costcenternama, divisinama, subdivisinama" & sptSubParam & "atid, atasetid, atjenismutasi, atsumber, atidutama, atidbarang, atkode, atnama, atkategori, atcabang, atlokasi, atgudang, atdivisi, atsubdivisi, atcostcenter, atproyek, atcatatan, atnomor, attglbeli, attglpakai, atjml, atsatuan, atmatauang, atkurs, atharga, atdiskon, atjmldiskon, atpajak1, atjmlpajak1, atpajak2, atjmlpajak2, athargabeli, atnilairesidu, atumurekonomis, atbebanperbln, atakumulasibeban, atnilaibuku, atnilaipenyusutan, atmetode, attabelpenyusutan, atintangible, atfiskal, atatastengahbulan, atrekasset, atrekakumdepresiasi, atrekdepresiasi, atrekpenghapusan, atprodusen, attglpensiun, atpenyusutanke, atnilaimenurun, atdispose, atpembelian, atpenjualan, atlocked, atstatus, atstatussebelumnya, atisclose, atinputuser, atinputtgl, atmodifikasiuser, atmodifikasitgl, atcustomtext1, atcustomtext2, atcustomtext3, atcustomtext4, atcustomtext5, atcustomint1, atcustomint2, atcustomint3, atcustomint4, atcustomint5, atcustomdbl1, atcustomdbl2, atcustomdbl3, atcustomdbl4, atcustomdbl5, atcustomdate1, atcustomdate2, atcustomdate3, atcustomdate4, atcustomdate5, atkategorinama, atcabangnama, atlokasinama, atgudangnama, atdivisinama, atsubdivisinama, atcostcenternama, atproyeknama, atmetodenama, atpajak1nama, atpajak1nilai, atpajak2nama, atpajak2nilai, atrekassetnama, atrekakumdepresiasinama, atrekdepresiasinama, atrekpenghapusannama, atprodusenkode, atprodusennama, atstatusnama, atstatussebelumnyanama, atinputusernama, atmodifikasiusernama, kodebarang"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_GrnSearch(ByVal param As String) As String
        'M4_GrnSearch --------------------------------------------------------
        'grnid, grncabang, grnlokasi, grngudang, grnasalbarang, grnasalbarangkategori, grnjenispembelian, 
        'grnjenispembeliankategori, grncarabayar, grnsumber, grnautonotransaksi, grnnotransaksi, grntgl, grnkodepa, 
        'grnsupplier, grnsupplierkontak, grn1alamat1, grn1alamat2, grn1alamat3, grn2alamat1, grn2alamat2, 
        'grn2alamat3, grnbagianpembelian, grntermin, grntgljatuhtempo, grnuraian, grncatatan, grnnoref, 
        'grntglnoref, grntglpenutupan, grnmatauang, grnkurs, grnhargatermasukpajak, grntotal, grndiskonpersen, 
        'grnjmldiskon, grntotalpajak1detail, grntotalpajak2detail, grnbiayalainpersen, grnbiayalain, grntotaltransaksi, grnjmlbayar, 
        'grnrekdiskon, grnrekpajak1, grnrekpajak2, grnrekbiayalain, grnrekbayar, grnidpr, grnidcs, 
        'grnidrq, grnidbs, grnidpo, grnidipc, grnstatusri, grnstatusdnr, grnstatusprt, 
        'grnstatusrealisasi, grnstatus, grnstatussebelumnya, grnjmlrevisi, grncetakanke, grninputuser, grninputtgl, 
        'grnmodifikasiuser, grnmodifikasitgl, grnposting, grnpostingtgl, grntutupperiode, grnisclose, grncabangnama, 
        'grnlokasinama, grngudangnama, grnsupplierkode, grnsuppliernama, grnbagianpembeliankode, grnbagianpembeliannama, prnotransaksi, 
        'csnotransaksi, rqnotransaksi, bsnotransaksi, ponotransaksi, ipcnotransaksi, grnstatusnama, grnstatussebelumnyanama, 
        'grninputusernama, grnmodifikasiusernama

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
            Filter = Filter.Replace("grnsupplierkode", "c1.kkode")
            Filter = Filter.Replace("grnsuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_grn_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Grn", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("grnid"), 0), sptField,
                     FxDB(dr("grncabang"), ""), sptField,
                     FxDB(dr("grnlokasi"), ""), sptField,
                     FxDB(dr("grngudang"), ""), sptField,
                     FxDB(dr("grnasalbarang"), ""), sptField,
                     FxDB(dr("grnasalbarangkategori"), 0), sptField,
                     FxDB(dr("grnjenispembelian"), ""), sptField,
                     FxDB(dr("grnjenispembeliankategori"), 0), sptField,
                     FxDB(dr("grncarabayar"), 0), sptField,
                     FxDB(dr("grnsumber"), ""), sptField,
                     FxDB(dr("grnautonotransaksi"), 0), sptField,
                     FxDB(dr("grnnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("grntgl"), ""), formatTgl), sptField,
                     FxDB(dr("grnkodepa"), 0), sptField,
                     FxDB(dr("grnsupplier"), 0), sptField,
                     FxDB(dr("grnsupplierkontak"), ""), sptField,
                     FxDB(dr("grn1alamat1"), ""), sptField,
                     FxDB(dr("grn1alamat2"), ""), sptField,
                     FxDB(dr("grn1alamat3"), ""), sptField,
                     FxDB(dr("grn2alamat1"), ""), sptField,
                     FxDB(dr("grn2alamat2"), ""), sptField,
                     FxDB(dr("grn2alamat3"), ""), sptField,
                     FxDB(dr("grnbagianpembelian"), 0), sptField,
                     FxDB(dr("grntermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("grntgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("grnuraian"), ""), sptField,
                     FxDB(dr("grncatatan"), ""), sptField,
                     FxDB(dr("grnnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("grntglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("grntglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("grnmatauang"), ""), sptField,
                     FxDB(dr("grnkurs"), 0), sptField,
                     FxDB(dr("grnhargatermasukpajak"), 0), sptField,
                     FxDB(dr("grntotal"), 0), sptField,
                     FxDB(dr("grndiskonpersen"), ""), sptField,
                     FxDB(dr("grnjmldiskon"), 0), sptField,
                     FxDB(dr("grntotalpajak1detail"), 0), sptField,
                     FxDB(dr("grntotalpajak2detail"), 0), sptField,
                     FxDB(dr("grnbiayalainpersen"), ""), sptField,
                     FxDB(dr("grnbiayalain"), 0), sptField,
                     FxDB(dr("grntotaltransaksi"), 0), sptField,
                     FxDB(dr("grnjmlbayar"), 0), sptField,
                     FxDB(dr("grnrekdiskon"), ""), sptField,
                     FxDB(dr("grnrekpajak1"), ""), sptField,
                     FxDB(dr("grnrekpajak2"), ""), sptField,
                     FxDB(dr("grnrekbiayalain"), ""), sptField,
                     FxDB(dr("grnrekbayar"), ""), sptField,
                     FxDB(dr("grnidpr"), 0), sptField,
                     FxDB(dr("grnidcs"), 0), sptField,
                     FxDB(dr("grnidrq"), 0), sptField,
                     FxDB(dr("grnidbs"), 0), sptField,
                     FxDB(dr("grnidpo"), 0), sptField,
                     FxDB(dr("grnidipc"), 0), sptField,
                     FxDB(dr("grnstatusri"), 0), sptField,
                     FxDB(dr("grnstatusdnr"), 0), sptField,
                     FxDB(dr("grnstatusprt"), 0), sptField,
                     FxDB(dr("grnstatusrealisasi"), 0), sptField,
                     FxDB(dr("grnstatus"), 0), sptField,
                     FxDB(dr("grnstatussebelumnya"), 0), sptField,
                     FxDB(dr("grnjmlrevisi"), 0), sptField,
                     FxDB(dr("grncetakanke"), 0), sptField,
                     FxDB(dr("grninputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("grninputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("grnmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("grnmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("grnposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("grnpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("grntutupperiode"), 0), sptField,
                     FxDB(dr("grnisclose"), 0), sptField,
                     FxDB(dr("grncabangnama"), ""), sptField,
                     FxDB(dr("grnlokasinama"), ""), sptField,
                     FxDB(dr("grngudangnama"), ""), sptField,
                     FxDB(dr("grnsupplierkode"), ""), sptField,
                     FxDB(dr("grnsuppliernama"), ""), sptField,
                     FxDB(dr("grnbagianpembeliankode"), ""), sptField,
                     FxDB(dr("grnbagianpembeliannama"), ""), sptField,
                     FxDB(dr("prnotransaksi"), ""), sptField,
                     FxDB(dr("csnotransaksi"), ""), sptField,
                     FxDB(dr("rqnotransaksi"), ""), sptField,
                     FxDB(dr("bsnotransaksi"), ""), sptField,
                     FxDB(dr("ponotransaksi"), ""), sptField,
                     FxDB(dr("ipcnotransaksi"), ""), sptField,
                     FxDB(dr("grnstatusnama"), ""), sptField,
                     FxDB(dr("grnstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("grninputusernama"), ""), sptField,
                     FxDB(dr("grnmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("grnid, grncabang, grnlokasi, grngudang, grnasalbarang, grnasalbarangkategori, grnjenispembelian, grnjenispembeliankategori, grncarabayar, grnsumber, grnautonotransaksi, grnnotransaksi, grntgl, grnkodepa, grnsupplier, grnsupplierkontak, grn1alamat1, grn1alamat2, grn1alamat3, grn2alamat1, grn2alamat2, grn2alamat3, grnbagianpembelian, grntermin, grntgljatuhtempo, grnuraian, grncatatan, grnnoref, grntglnoref, grntglpenutupan, grnmatauang, grnkurs, grnhargatermasukpajak, grntotal, grndiskonpersen, grnjmldiskon, grntotalpajak1detail, grntotalpajak2detail, grnbiayalainpersen, grnbiayalain, grntotaltransaksi, grnjmlbayar, grnrekdiskon, grnrekpajak1, grnrekpajak2, grnrekbiayalain, grnrekbayar, grnidpr, grnidcs, grnidrq, grnidbs, grnidpo, grnidipc, grnstatusri, grnstatusdnr, grnstatusprt, grnstatusrealisasi, grnstatus, grnstatussebelumnya, grnjmlrevisi, grncetakanke, grninputuser, grninputtgl, grnmodifikasiuser, grnmodifikasitgl, grnposting, grnpostingtgl, grntutupperiode, grnisclose, grncabangnama, grnlokasinama, grngudangnama, grnsupplierkode, grnsuppliernama, grnbagianpembeliankode, grnbagianpembeliannama, prnotransaksi, csnotransaksi, rqnotransaksi, bsnotransaksi, ponotransaksi, ipcnotransaksi, grnstatusnama, grnstatussebelumnyanama, grninputusernama, grnmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_GrnTerkait(ByVal param As String) As String
        'M4_GrnTerkait --------------------------------------------------------
        'grnid, grnnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "grnid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_grn_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("grnid"), 0), sptField,
                     FxDB(dr("grnnotransaksi"), ""), sptField,
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
            result(2) = "Related GRN data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("grnid, grnnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_Grn_Detail_VSearch(ByVal param As String) As String
        'M4_Grn_Detail_VSearch --------------------------------------------------------
        'idgrndetail, idgrn, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, gudang, rekpersediaan, rekdiskonpembelian, rekhutangsementara, costcenter, divisi, 
        'subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, 
        'idbsdetail, idpodetail, idipcdetail, jmlri, statusri, jmldnr, statusdnr, 
        'jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, 
        'customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'grnnotransaksi, grnuraian, grncatatan, grnnoref, grntglnoref, grnsupplierkontak, grn1alamat1, 
        'grn1alamat2, grn1alamat3, grn2alamat1, grn2alamat2, grn2alamat3, grntermin, grnterminnama, 
        'grnterminharijatuhtempo, grnbagianpembelian, grnbagianpembeliankode, grnbagianpembeliannama, kodebarang, bhpp, bjenis, 
        'bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisari, 
        'jmlsisarealisasi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan, jmlsisats, basset, ambilnotransaksi,
        'pocustomtext1, pocustomtext2, grnsupplier, grnsupplierkode, grnsuppliernama, grntgljatuhtempo,
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
            Filter = Filter.Replace("idbarang", "grnd.idbarang")
            Filter = Filter.Replace("statusrealisasi", "grnd.statusrealisasi")
            Filter = Filter.Replace("isclose", "grnd.isclose")
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m4_grn_detail_v")
        sql = "select grnd.idgrndetail AS idgrndetail, grnd.idgrn AS idgrn, grnd.idbarang AS idbarang, grnd.namabarang AS namabarang, grnd.tipebarang AS tipebarang, grnd.jml AS jml, grnd.satuan AS satuan, grnd.nilaisatuan AS nilaisatuan, grnd.jmlbarang AS jmlbarang, grnd.satuanbarang AS satuanbarang, grnd.matauang AS matauang, grnd.kurs AS kurs, grnd.hargafix AS hargafix, grnd.harga AS harga, grnd.diskon AS diskon, grnd.jmldiskon AS jmldiskon, grnd.pajak1 AS pajak1, grnd.jmlpajak1 AS jmlpajak1, grnd.pajak2 AS pajak2, grnd.jmlpajak2 AS jmlpajak2, grnd.cabang AS cabang, grnd.lokasi AS lokasi, grnd.gudang AS gudang, i.brekpersediaan AS rekpersediaan, grnd.rekdiskonpembelian AS rekdiskonpembelian, s.snilai AS rekhutangsementara, grnd.costcenter AS costcenter, grnd.divisi AS divisi, grnd.subdivisi AS subdivisi, grnd.proyek AS proyek, grnd.catatan AS catatan, grnd.urutan AS urutan, grnd.idprdetail AS idprdetail, grnd.idcsdetail AS idcsdetail, grnd.idrqdetail AS idrqdetail, grnd.idbsdetail AS idbsdetail, grnd.idpodetail AS idpodetail, grnd.idipcdetail AS idipcdetail, grnd.jmlri AS jmlri, grnd.statusri AS statusri, grnd.jmldnr AS jmldnr, grnd.statusdnr AS statusdnr, grnd.jmlprt AS jmlprt, grnd.statusprt AS statusprt, grnd.jmlrealisasi AS jmlrealisasi, grnd.statusrealisasi AS statusrealisasi, grnd.isclose AS isclose, grnd.customtext1 AS customtext1, grnd.customtext2 AS customtext2, grnd.customtext3 AS customtext3, grnd.customdbl1 AS customdbl1, grnd.customdbl2 AS customdbl2, grnd.customdbl3 AS customdbl3, grnd.customdate1 AS customdate1, grnd.customdate2 AS customdate2, grnd.customdate3 AS customdate3, grn.grnnotransaksi AS grnnotransaksi, grn.grnuraian AS grnuraian, grn.grncatatan AS grncatatan, grn.grnnoref AS grnnoref, grn.grntglnoref AS grntglnoref, grn.grnsupplierkontak AS grnsupplierkontak, grn.grn1alamat1 AS grn1alamat1, grn.grn1alamat2 AS grn1alamat2, grn.grn1alamat3 AS grn1alamat3, grn.grn2alamat1 AS grn2alamat1, grn.grn2alamat2 AS grn2alamat2, grn.grn2alamat3 AS grn2alamat3, grn.grntermin AS grntermin, tr.trnama AS grnterminnama, tr.trharijatuhtempo AS grnterminharijatuhtempo, grn.grnbagianpembelian AS grnbagianpembelian, c1.kkode AS grnbagianpembeliankode, c1.knama AS grnbagianpembeliannama, i.bkode AS kodebarang, i.bhpp AS bhpp, i.bjenis AS bjenis, i.bserial AS bserial, i.bbatch AS bbatch, t1.tnama AS pajak1nama, t1.tnilai AS pajak1nilai, t2.tnama AS pajak2nama, t2.tnilai AS pajak2nilai, ((grnd.jmlbarang - grnd.jmlri) / grnd.nilaisatuan) AS jmlsisari, ((grnd.jmlbarang - grnd.jmlrealisasi) / grnd.nilaisatuan) AS jmlsisarealisasi, ((grnd.jmlbarang - grnd.jmlts) / grnd.nilaisatuan) AS jmlsisats, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan, i.basset, po.ponotransaksi, po.pocustomtext1, po.pocustomtext2, grn.grnsupplier as grnsupplier, k.kkode AS grnsupplierkode, k.knama AS grnsuppliernama, grn.grntgljatuhtempo, t1.takunbeli as pajak1akunbeli, t1c1.cnama as pajak1akunbelinama, t1.takunjual as pajak1akunjual, t1c2.cnama as pajak1akunjualnama, t2.takunbeli as pajak2akunbeli, t2c1.cnama as pajak2akunbelinama, t2.takunjual as pajak2akunjual, t2c2.cnama as pajak2akunjualnama, d.dnama AS divisinama, sd.sdnama AS subdivisinama, cc.ccnama AS costcenternama, p.pnama AS proyeknama from m4_grn_detail grnd join m4_grn grn on grnd.idgrn = grn.grnid join m1_item i on grnd.idbarang = i.bid join m1_contact k on grn.grnsupplier = k.kid left join m1_terms tr on grn.grntermin = tr.trkode left join m1_contact c1 on grn.grnbagianpembelian = c1.kid left join m1_tax t1 on grnd.pajak1 = t1.tkode left join m1_tax t2 on grnd.pajak2 = t2.tkode left join m0_setting s on s.smodule = 0 and s.sgrup = 'akun' and s.skode = 'HutangSementara' left join m4_po_detail pod on grnd.idpodetail = pod.idpodetail left join m4_po po on pod.idpo = po.poid left join m1_coa t1c1 on t1.takunbeli = t1c1.cnomor left join m1_coa t1c2 on t1.takunjual = t1c2.cnomor left join m1_coa t2c1 on t2.takunbeli = t2c1.cnomor left join m1_coa t2c2 on t2.takunjual = t2c2.cnomor LEFT JOIN m1_division d ON d.dkode = grnd.divisi LEFT JOIN m1_subdivision sd ON sd.sdkode = grnd.subdivisi LEFT JOIN m1_cost_center cc ON cc.cckode = grnd.costcenter LEFT JOIN m1_project p ON p.pkode = grnd.proyek"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        Dim ambilGRN As String = 2
        'AMBIL SETTING, PAKAI CABANG ATAU TIDAK
        Dim rsSetting As String = F_getSetting(4, "options", "NoTransaksiRI")
        If Len(rsSetting) > 0 Then ambilGRN = rsSetting

        dt = AmbilData("aplikasi1-M5_Sq_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idgrndetail"), 0), sptField,
                     FxDB(dr("idgrn"), 0), sptField,
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
                     FxDB(dr("rekpersediaan"), ""), sptField,
                     FxDB(dr("rekdiskonpembelian"), ""), sptField,
                     FxDB(dr("rekhutangsementara"), ""), sptField,
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
                     FxDB(dr("jmlri"), 0), sptField,
                     FxDB(dr("statusri"), 0), sptField,
                     FxDB(dr("jmldnr"), 0), sptField,
                     FxDB(dr("statusdnr"), 0), sptField,
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
                     FxDB(dr("grnnotransaksi"), ""), sptField,
                     FxDB(dr("grnuraian"), ""), sptField,
                     FxDB(dr("grncatatan"), ""), sptField,
                     FxDB(dr("grnnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("grntglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("grnsupplierkontak"), ""), sptField,
                     FxDB(dr("grn1alamat1"), ""), sptField,
                     FxDB(dr("grn1alamat2"), ""), sptField,
                     FxDB(dr("grn1alamat3"), ""), sptField,
                     FxDB(dr("grn2alamat1"), ""), sptField,
                     FxDB(dr("grn2alamat2"), ""), sptField,
                     FxDB(dr("grn2alamat3"), ""), sptField,
                     FxDB(dr("grntermin"), ""), sptField,
                     FxDB(dr("grnterminnama"), ""), sptField,
                     FxDB(dr("grnterminharijatuhtempo"), 0), sptField,
                     FxDB(dr("grnbagianpembelian"), 0), sptField,
                     FxDB(dr("grnbagianpembeliankode"), ""), sptField,
                     FxDB(dr("grnbagianpembeliannama"), ""), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("jmlsisari"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptField,
                     FxDB(dr("jmlsisats"), 0), sptField,
                     FxDB(dr("basset"), 0), sptField,
                     FxDB(IIf(ambilGRN = 2, dr("grnnotransaksi"), dr("ponotransaksi")), ""), sptField,
                     FxDB(dr("pocustomtext1"), ""), sptField,
                     FxDB(dr("pocustomtext2"), ""), sptField,
                     FxDB(dr("grnsupplier"), 0), sptField,
                     FxDB(dr("grnsupplierkode"), ""), sptField,
                     FxDB(dr("grnsuppliernama"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("grntgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("pajak1akunbeli"), ""), sptField,
                     FxDB(dr("pajak1akunbelinama"), ""), sptField,
                     FxDB(dr("pajak1akunjual"), ""), sptField,
                     FxDB(dr("pajak1akunjualnama"), ""), sptField,
                     FxDB(dr("pajak2akunbeli"), ""), sptField,
                     FxDB(dr("pajak2akunbelinama"), ""), sptField,
                     FxDB(dr("pajak2akunjual"), ""), sptField,
                     FxDB(dr("pajak2akunjualnama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idgrndetail, idgrn, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, rekpersediaan, rekdiskonpembelian, rekhutangsementara, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, grnnotransaksi, grnuraian, grncatatan, grnnoref, grntglnoref, grnsupplierkontak, grn1alamat1, grn1alamat2, grn1alamat3, grn2alamat1, grn2alamat2, grn2alamat3, grntermin, grnterminnama, grnterminharijatuhtempo, grnbagianpembelian, grnbagianpembeliankode, grnbagianpembeliannama, kodebarang, bhpp, bjenis, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisari, jmlsisarealisasi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan, jmlsisats, basset, ambilnotransaksi, pocustomtext1, pocustomtext2, grnsupplier, grnsupplierkode, grnsuppliernama, grntgljatuhtempo, pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, pajak2akunjualnama, divisinama, subdivisinama, costcenternama, proyeknama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_Grn_Detail_Cost(ByVal param As String) As String
        'M4_Grn_Detail_Cost --------------------------------------------------------
        'Detail
        'idgrndetail, idgrn, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, gudang, rekpersediaan, rekdiskonpembelian, rekhutangsementara, costcenter, divisi, 
        'subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, 
        'idbsdetail, idpodetail, idipcdetail, jmlri, statusri, jmldnr, statusdnr, 
        'jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, 
        'customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'grnnotransaksi, grnuraian, grncatatan, grnnoref, grntglnoref, grnsupplierkontak, grn1alamat1, 
        'grn1alamat2, grn1alamat3, grn2alamat1, grn2alamat2, grn2alamat3, grntermin, grnterminnama, 
        'grnterminharijatuhtempo, grnbagianpembelian, grnbagianpembeliankode, grnbagianpembeliannama, kodebarang, bhpp, bjenis, 
        'bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisari, 
        'jmlsisarealisasi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan, jmlsisats, basset, ambilnotransaksi,
        'pocustomtext1, pocustomtext2, grnsupplier, grnsupplierkode, grnsuppliernama, grntgljatuhtempo,
        'pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, 
        'pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, pajak2akunjualnama

        'Cost
        'idgrncost, idgrn, kodecost, matauang, kurs, jumlah, rekdebit, 
        'rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, 
        'proyek, urutan, idprcost, idcscost, idrqcost, idbscost, idpocost, 
        'idipccost, jumlahri, statusri, jumlahbayar, statusbayar, isclose, customtext1, 
        'customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3, kodecostnama, rekdebitnama, rekkreditnama, kontakkode, kontaknama, costcenternama, 
        'divisinama, subdivisinama

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = "", cost As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Filter1 As String = "", Sorting1 As String = "", Filter2 As String = "", Sorting2 As String = ""
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

        'FILTER DIBAGI MENJADI 2, DETAIL DAN COST
        'VALIDASI PAGING KHUSUS, FILTER DAN SORTING UNTUK 2 TABEL
        Dim filterSplit(2) As String, sortingSplit(2) As String

        filterSplit = pagingSplit(2).Split(sptRow)
        If (filterSplit.Length <> 2) Then
            result(2) = "Invalid filter parameter." : GoTo selesai
        End If
        'Replace disesuaikan dengan kebutuhan
        If (filterSplit(0).Length > 0) Then
            Filter1 = filterSplit(0)
            '#Taruh fungsi replace disini...
            Filter1 = Filter1.Replace("idbarang", "grnd.idbarang")
            Filter1 = Filter1.Replace("statusrealisasi", "grnd.statusrealisasi")
            Filter1 = Filter1.Replace("isclose", "grnd.isclose")
        End If
        If (filterSplit(1).Length > 0) Then
            Filter2 = filterSplit(1)
            '#Taruh fungsi replace disini...
        End If

        sortingSplit = pagingSplit(3).Split(sptRow)
        If (sortingSplit.Length <> 2) Then
            result(2) = "Invalid sorting parameter." : GoTo selesai
        End If
        If (sortingSplit(0).Length > 0) Then
            Sorting1 = sortingSplit(0)
            '#Taruh fungsi replace disini...
        End If
        If (sortingSplit(1).Length > 0) Then
            Sorting2 = sortingSplit(1)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select grnd.idgrndetail AS idgrndetail, grnd.idgrn AS idgrn, grnd.idbarang AS idbarang, grnd.namabarang AS namabarang, grnd.tipebarang AS tipebarang, grnd.jml AS jml, grnd.satuan AS satuan, grnd.nilaisatuan AS nilaisatuan, grnd.jmlbarang AS jmlbarang, grnd.satuanbarang AS satuanbarang, grnd.matauang AS matauang, grnd.kurs AS kurs, grnd.hargafix AS hargafix, grnd.harga AS harga, grnd.diskon AS diskon, grnd.jmldiskon AS jmldiskon, grnd.pajak1 AS pajak1, grnd.jmlpajak1 AS jmlpajak1, grnd.pajak2 AS pajak2, grnd.jmlpajak2 AS jmlpajak2, grnd.cabang AS cabang, grnd.lokasi AS lokasi, grnd.gudang AS gudang, i.brekpersediaan AS rekpersediaan, grnd.rekdiskonpembelian AS rekdiskonpembelian, s.snilai AS rekhutangsementara, grnd.costcenter AS costcenter, grnd.divisi AS divisi, grnd.subdivisi AS subdivisi, grnd.proyek AS proyek, grnd.catatan AS catatan, grnd.urutan AS urutan, grnd.idprdetail AS idprdetail, grnd.idcsdetail AS idcsdetail, grnd.idrqdetail AS idrqdetail, grnd.idbsdetail AS idbsdetail, grnd.idpodetail AS idpodetail, grnd.idipcdetail AS idipcdetail, grnd.jmlri AS jmlri, grnd.statusri AS statusri, grnd.jmldnr AS jmldnr, grnd.statusdnr AS statusdnr, grnd.jmlprt AS jmlprt, grnd.statusprt AS statusprt, grnd.jmlrealisasi AS jmlrealisasi, grnd.statusrealisasi AS statusrealisasi, grnd.isclose AS isclose, grnd.customtext1 AS customtext1, grnd.customtext2 AS customtext2, grnd.customtext3 AS customtext3, grnd.customdbl1 AS customdbl1, grnd.customdbl2 AS customdbl2, grnd.customdbl3 AS customdbl3, grnd.customdate1 AS customdate1, grnd.customdate2 AS customdate2, grnd.customdate3 AS customdate3, grn.grnnotransaksi AS grnnotransaksi, grn.grnuraian AS grnuraian, grn.grncatatan AS grncatatan, grn.grnnoref AS grnnoref, grn.grntglnoref AS grntglnoref, grn.grnsupplierkontak AS grnsupplierkontak, grn.grn1alamat1 AS grn1alamat1, grn.grn1alamat2 AS grn1alamat2, grn.grn1alamat3 AS grn1alamat3, grn.grn2alamat1 AS grn2alamat1, grn.grn2alamat2 AS grn2alamat2, grn.grn2alamat3 AS grn2alamat3, grn.grntermin AS grntermin, tr.trnama AS grnterminnama, tr.trharijatuhtempo AS grnterminharijatuhtempo, grn.grnbagianpembelian AS grnbagianpembelian, c1.kkode AS grnbagianpembeliankode, c1.knama AS grnbagianpembeliannama, i.bkode AS kodebarang, i.bhpp AS bhpp, i.bjenis AS bjenis, i.bserial AS bserial, i.bbatch AS bbatch, t1.tnama AS pajak1nama, t1.tnilai AS pajak1nilai, t2.tnama AS pajak2nama, t2.tnilai AS pajak2nilai, ((grnd.jmlbarang - grnd.jmlri) / grnd.nilaisatuan) AS jmlsisari, ((grnd.jmlbarang - grnd.jmlrealisasi) / grnd.nilaisatuan) AS jmlsisarealisasi, ((grnd.jmlbarang - grnd.jmlts) / grnd.nilaisatuan) AS jmlsisats, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan, i.basset, po.ponotransaksi, po.pocustomtext1, po.pocustomtext2, grn.grnsupplier as grnsupplier, k.kkode AS grnsupplierkode, k.knama AS grnsuppliernama, grn.grntgljatuhtempo, t1.takunbeli as pajak1akunbeli, t1c1.cnama as pajak1akunbelinama, t1.takunjual as pajak1akunjual, t1c2.cnama as pajak1akunjualnama, t2.takunbeli as pajak2akunbeli, t2c1.cnama as pajak2akunbelinama, t2.takunjual as pajak2akunjual, t2c2.cnama as pajak2akunjualnama from m4_grn_detail grnd join m4_grn grn on grnd.idgrn = grn.grnid join m1_item i on grnd.idbarang = i.bid join m1_contact k on grn.grnsupplier = k.kid left join m1_terms tr on grn.grntermin = tr.trkode left join m1_contact c1 on grn.grnbagianpembelian = c1.kid left join m1_tax t1 on grnd.pajak1 = t1.tkode left join m1_tax t2 on grnd.pajak2 = t2.tkode left join m0_setting s on s.smodule = 0 and s.sgrup = 'akun' and s.skode = 'HutangSementara' left join m4_po_detail pod on grnd.idpodetail = pod.idpodetail left join m4_po po on pod.idpo = po.poid left join m1_coa t1c1 on t1.takunbeli = t1c1.cnomor left join m1_coa t1c2 on t1.takunjual = t1c2.cnomor left join m1_coa t2c1 on t2.takunbeli = t2c1.cnomor left join m1_coa t2c2 on t2.takunjual = t2c2.cnomor"
        'result(2) = sql : GoTo selesai
        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        Dim ambilGRN As String = 2
        'AMBIL SETTING, PAKAI CABANG ATAU TIDAK
        Dim rsSetting As String = F_getSetting(4, "options", "NoTransaksiRI")
        If Len(rsSetting) > 0 Then ambilGRN = rsSetting

        dt = AmbilData("aplikasi1-M4_Grn_Detail", Filter1, Sorting1, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idgrndetail"), 0), sptField,
                     FxDB(dr("idgrn"), 0), sptField,
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
                     FxDB(dr("rekpersediaan"), ""), sptField,
                     FxDB(dr("rekdiskonpembelian"), ""), sptField,
                     FxDB(dr("rekhutangsementara"), ""), sptField,
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
                     FxDB(dr("jmlri"), 0), sptField,
                     FxDB(dr("statusri"), 0), sptField,
                     FxDB(dr("jmldnr"), 0), sptField,
                     FxDB(dr("statusdnr"), 0), sptField,
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
                     FxDB(dr("grnnotransaksi"), ""), sptField,
                     FxDB(dr("grnuraian"), ""), sptField,
                     FxDB(dr("grncatatan"), ""), sptField,
                     FxDB(dr("grnnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("grntglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("grnsupplierkontak"), ""), sptField,
                     FxDB(dr("grn1alamat1"), ""), sptField,
                     FxDB(dr("grn1alamat2"), ""), sptField,
                     FxDB(dr("grn1alamat3"), ""), sptField,
                     FxDB(dr("grn2alamat1"), ""), sptField,
                     FxDB(dr("grn2alamat2"), ""), sptField,
                     FxDB(dr("grn2alamat3"), ""), sptField,
                     FxDB(dr("grntermin"), ""), sptField,
                     FxDB(dr("grnterminnama"), ""), sptField,
                     FxDB(dr("grnterminharijatuhtempo"), 0), sptField,
                     FxDB(dr("grnbagianpembelian"), 0), sptField,
                     FxDB(dr("grnbagianpembeliankode"), ""), sptField,
                     FxDB(dr("grnbagianpembeliannama"), ""), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("jmlsisari"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptField,
                     FxDB(dr("jmlsisats"), 0), sptField,
                     FxDB(dr("basset"), 0), sptField,
                     FxDB(IIf(ambilGRN = 2, dr("grnnotransaksi"), dr("ponotransaksi")), ""), sptField,
                     FxDB(dr("pocustomtext1"), ""), sptField,
                     FxDB(dr("pocustomtext2"), ""), sptField,
                     FxDB(dr("grnsupplier"), 0), sptField,
                     FxDB(dr("grnsupplierkode"), ""), sptField,
                     FxDB(dr("grnsuppliernama"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("grntgljatuhtempo"), ""), formatTgl), sptField,
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

            'AMBIL DATA COST
            sql = "SELECT grnc.idgrncost, grnc.idgrn, grnc.kodecost, grnc.matauang, grnc.kurs, grnc.jumlah, grnc.rekdebit, grnc.rekkredit, grnc.kontak, grnc.termasukhpp, grnc.catatan, grnc.costcenter, grnc.divisi, grnc.subdivisi, grnc.proyek, grnc.urutan, grnc.idprcost, grnc.idcscost, grnc.idrqcost, grnc.idbscost, grnc.idpocost, grnc.idipccost, grnc.jumlahri, grnc.statusri, grnc.jumlahbayar, grnc.statusbayar, grnc.isclose, grnc.customtext1, grnc.customtext2, grnc.customtext3, grnc.customdbl1, grnc.customdbl2, grnc.customdbl3, grnc.customdate1, grnc.customdate2, grnc.customdate3, oc.ocnama AS kodecostnama, coa1.cnama AS rekdebitnama, coa2.cnama AS rekkreditnama, c.kkode AS kontakkode, c.knama AS kontaknama, cc.ccnama AS costcenternama, d.dnama AS divisinama, sd.sddivisi AS subdivisinama FROM m4_grn_cost grnc JOIN m4_grn grn ON grnc.idgrn = grn.grnid LEFT JOIN m1_other_cost oc ON grnc.kodecost = oc.ockode LEFT JOIN m1_coa coa1 ON grnc.rekdebit = coa1.cnomor LEFT JOIN m1_coa coa2 ON grnc.rekkredit = coa2.cnomor LEFT JOIN m1_contact c ON grnc.kontak = c.kid LEFT JOIN m1_cost_center cc ON grnc.costcenter = cc.cckode LEFT JOIN m1_division d ON grnc.divisi = d.dkode LEFT JOIN m1_subdivision sd ON grnc.subdivisi = sd.sdkode"
            Dim dtcost As New DataTable
            dtcost = AmbilData("aplikasi1-m4_grn_cost", Filter2, Sorting2, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtcost.Rows
                cost = String.Concat(cost,
                     FxDB(dr("idgrncost"), ""), sptField,
                     FxDB(dr("idgrn"), ""), sptField,
                     FxDB(dr("kodecost"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("jumlah"), 0), sptField,
                     FxDB(dr("rekdebit"), ""), sptField,
                     FxDB(dr("rekkredit"), ""), sptField,
                     FxDB(dr("kontak"), ""), sptField,
                     FxDB(dr("termasukhpp"), 0), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idprcost"), ""), sptField,
                     FxDB(dr("idcscost"), ""), sptField,
                     FxDB(dr("idrqcost"), ""), sptField,
                     FxDB(dr("idbscost"), ""), sptField,
                     FxDB(dr("idpocost"), ""), sptField,
                     FxDB(dr("idipccost"), ""), sptField,
                     FxDB(dr("jumlahri"), 0), sptField,
                     FxDB(dr("statusri"), 0), sptField,
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
                     FxDB(dr("kontakkode"), ""), sptField,
                     FxDB(dr("kontaknama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptRow)
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
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search, sptSubParam, cost)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idgrndetail, idgrn, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, rekpersediaan, rekdiskonpembelian, rekhutangsementara, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, grnnotransaksi, grnuraian, grncatatan, grnnoref, grntglnoref, grnsupplierkontak, grn1alamat1, grn1alamat2, grn1alamat3, grn2alamat1, grn2alamat2, grn2alamat3, grntermin, grnterminnama, grnterminharijatuhtempo, grnbagianpembelian, grnbagianpembeliankode, grnbagianpembeliannama, kodebarang, bhpp, bjenis, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisari, jmlsisarealisasi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan, jmlsisats, basset, ambilnotransaksi, pocustomtext1, pocustomtext2, grnsupplier, grnsupplierkode, grnsuppliernama, grntgljatuhtempo, pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, pajak2akunjualnama" & sptSubParam & "idgrncost, idgrn, kodecost, matauang, kurs, jumlah, rekdebit, rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, proyek, urutan, idprcost, idcscost, idrqcost, idbscost, idpocost, idipccost, jumlahri, statusri, jumlahbayar, statusbayar, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodecostnama, rekdebitnama, rekkreditnama, kontakkode, kontaknama, costcenternama, divisinama, subdivisinama"))

        Return wsResult
    End Function

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstandingPO As String, ByVal ftOutstandingPO As String, ByVal ftHppI As String, ByVal ftHppF As String, ByVal ftExistStok As String, ByVal ftStok As String, ByVal ftPO As String, ByRef termasukPajak As String) As String
        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaiSatuan As Double = 0, sisa As Double = 0
        Dim filterLookup As String = "", urutan As String = "", gudang As String = ""

        'VALIDASI OUTSTANDING ---------------------------------------
        'PO
        If Len(ftExistOutstandingPO) > 0 Then 'ftExistOutstanding = rowExists, idpodetail, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingPO)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "idpodetail=" & dtval.Rows(0)("idpodetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in PO" : GoTo selesai
            End If
        End If

        'CEK PO YANG DIAMBIL
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        If Len(ftPO) > 0 Then
            sql = "SELECT po.ponotransaksi as notransaksi, po.pohargatermasukpajak as termasukpajak, (CASE po.pohargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajaknama FROM m4_po_detail pod JOIN m4_po po ON pod.idpo = po.poid WHERE " & ftPO & " GROUP BY po.pohargatermasukpajak"
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
        End If

        'CEK TRANSAKSI HARGA TERMASUK PAJAK TIDAK BOLEH AMBIL TRANSAKSI HARGA TIDAK TERMASUK PAJAK, DAN SEBALIKNYA
        If Len(termasukPajak) > 0 Then
            sql = "SELECT i.bkode, pod.idpodetail, po.ponotransaksi as notransaksi, (CASE po.pohargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m4_po_detail pod JOIN m4_po po ON pod.idpo = po.poid JOIN m1_item i ON pod.idbarang = i.bid WHERE (" & ftPO & ") AND po.pohargatermasukpajak <> " & termasukPajak & " ORDER BY pod.urutan"
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "idpodetail = " & dtval.Rows(0)("idpodetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & ". " & dtval.Rows(0)("notransaksi") & " " & dtval.Rows(0)("termasukpajak") : GoTo selesai
            End If
        End If

        'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
        If Len(ftOutstandingPO) > 0 Then
            sql = "SELECT pod.idpodetail, ROUND(pod.jmlbarang - pod.jmlrealisasi, 5) as sisarealisasi, i.bid, i.bkode FROM m4_po_detail AS pod INNER JOIN m1_item AS i ON pod.idbarang = i.bid JOIN m0_setting s ON s.smodule = 4 AND s.sgrup = 'options' AND s.skode = 'GRNLebihDariPO' WHERE " & ftOutstandingPO
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisarealisasi")

                filterLookup = "idpodetail=" & dtval.Rows(0)("idpodetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in PO, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
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
    Public Function M4_GrnSimpanOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataBatch(), dataRowBatch(), dataSerial(), dataRowSerial(), dataCost(), dataRowCost() As String

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
        If (dataSplit.Length <> 5) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'grnid(0) As Integer, grncabang(1) As String, grnlokasi(2) As String, grngudang(3) As String, grnasalbarang(4) As String, 
        'grnasalbarangkategori(5) As Integer, grnjenispembelian(6) As String, grnjenispembeliankategori(7) As Integer, grncarabayar(8) As Integer, grnsumber(9) As String, 
        'grnautonotransaksi(10) As Integer, grnnotransaksi(11) As String, grntgl(12) As Date, grnkodepa(13) As Integer, grnsupplier(14) As Integer, 
        'grnsupplierkontak(15) As String, grn1alamat1(16) As String, grn1alamat2(17) As String, grn1alamat3(18) As String, grn2alamat1(19) As String, 
        'grn2alamat2(20) As String, grn2alamat3(21) As String, grnbagianpembelian(22) As Integer, grntermin(23) As String, grntgljatuhtempo(24) As Date, 
        'grnuraian(25) As String, grncatatan(26) As String, grnnoref(27) As String, grntglnoref(28) As Date, grntglpenutupan(29) As Date, 
        'grnmatauang(30) As String, grnkurs(31) As Double, grnhargatermasukpajak(32) As Integer, grntotal(33) As Double, grndiskonpersen(34) As String, 
        'grnjmldiskon(35) As Double, grntotalpajak1detail(36) As Double, grntotalpajak2detail(37) As Double, grnbiayalainpersen(38) As String, grnbiayalain(39) As Double, 
        'grntotaltransaksi(40) As Double, grnjmlbayar(41) As Double, grnrekdiskon(42) As String, grnrekpajak1(43) As String, grnrekpajak2(44) As String, 
        'grnrekbiayalain(45) As String, grnrekbayar(46) As String, grnidpr(47) As Integer, grnidcs(48) As Integer, grnidrq(49) As Integer, 
        'grnidbs(50) As Integer, grnidpo(51) As Integer, grnidipc(52) As Integer, grnstatusri(53) As Integer, grnstatusdnr(54) As Integer, 
        'grnstatusprt(55) As Integer, grnstatus(56) As Integer, grnstatussebelumnya(57) As Integer, grnjmlrevisi(58) As Integer, grncetakanke(59) As Integer, 
        'grninputuser(60) As Integer, grninputtgl(61) As DateTime, grnmodifikasiuser(62) As Integer, grnmodifikasitgl(63) As DateTime, grnposting(64) As Integer, 
        'grntutupperiode(65) As Integer, grnisclose(66) As Integer, grncustomtext1(67) As String, grncustomtext2(68) As String, grncustomtext3(69) As String, 
        'grncustomtext4(70) As String, grncustomtext5(71) As String, grncustomint1(72) As Integer, grncustomint2(73) As Integer, grncustomint3(74) As Integer, 
        'grncustomdbl1(75) As Double, grncustomdbl2(76) As Double, grncustomdbl3(77) As Double, grncustomdate1(78) As Date, grncustomdate2(79) As Date, 
        'grncustomdate3(80) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'grnid, grncabang, grnlokasi, grngudang, grnasalbarang, grnasalbarangkategori, grnjenispembelian, 
        'grnjenispembeliankategori, grncarabayar, grnsumber, grnautonotransaksi, grnnotransaksi, grntgl, grnkodepa, 
        'grnsupplier, grnsupplierkontak, grn1alamat1, grn1alamat2, grn1alamat3, grn2alamat1, grn2alamat2, 
        'grn2alamat3, grnbagianpembelian, grntermin, grntgljatuhtempo, grnuraian, grncatatan, grnnoref, 
        'grntglnoref, grntglpenutupan, grnmatauang, grnkurs, grnhargatermasukpajak, grntotal, grndiskonpersen, 
        'grnjmldiskon, grntotalpajak1detail, grntotalpajak2detail, grnbiayalainpersen, grnbiayalain, grntotaltransaksi, grnjmlbayar, 
        'grnrekdiskon, grnrekpajak1, grnrekpajak2, grnrekbiayalain, grnrekbayar, grnidpr, grnidcs, 
        'grnidrq, grnidbs, grnidpo, grnidipc, grnstatusri, grnstatusdnr, grnstatusprt, 
        'grnstatus, grnstatussebelumnya, grnjmlrevisi, grncetakanke, grninputuser, grninputtgl, grnmodifikasiuser, 
        'grnmodifikasitgl, grnposting, grntutupperiode, grnisclose, grncustomtext1, grncustomtext2, grncustomtext3, 
        'grncustomtext4, grncustomtext5, grncustomint1, grncustomint2, grncustomint3, grncustomdbl1, grncustomdbl2, 
        'grncustomdbl3, grncustomdate1, grncustomdate2, grncustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 81) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'grnid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "grnid required numeric." : GoTo selesai
        End If
        'grnasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "grnasalbarangkategori required numeric." : GoTo selesai
        End If
        'grnjenispembeliankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "grnjenispembeliankategori required numeric." : GoTo selesai
        End If
        'grncarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "grncarabayar required numeric." : GoTo selesai
        End If
        'grnautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "grnautonotransaksi required numeric." : GoTo selesai
        End If
        'grntgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "grntgl required date." : GoTo selesai
        End If
        'grnkodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "grnkodepa required numeric." : GoTo selesai
        End If
        'grnsupplier(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "grnsupplier required numeric." : GoTo selesai
        End If
        If (dataUtama(14) < 1) Then
            result(2) = "grnsupplier can't be empty." : GoTo selesai
        End If
        'grnbagianpembelian(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "grnbagianpembelian required numeric." : GoTo selesai
        End If
        'grntgljatuhtempo(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "grntgljatuhtempo required date." : GoTo selesai
        End If
        'grntglnoref(28) As Date
        If (IsDate(dataUtama(28)) = False) Then
            result(2) = "grntglnoref required date." : GoTo selesai
        End If
        'grntglpenutupan(29) As Date
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "grntglpenutupan required date." : GoTo selesai
        End If
        'grnkurs(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "grnkurs required numeric." : GoTo selesai
        End If
        'grnhargatermasukpajak(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "grnhargatermasukpajak required numeric." : GoTo selesai
        End If
        'grntotal(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "grntotal required numeric." : GoTo selesai
        End If
        'grnjmldiskon(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "grnjmldiskon required numeric." : GoTo selesai
        End If
        'grntotalpajak1detail(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "grntotalpajak1detail required numeric." : GoTo selesai
        End If
        'grntotalpajak2detail(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "grntotalpajak2detail required numeric." : GoTo selesai
        End If
        'grnbiayalain(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "grnbiayalain required numeric." : GoTo selesai
        End If
        'grntotaltransaksi(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "grntotaltransaksi required numeric." : GoTo selesai
        End If
        'grnjmlbayar(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "grnjmlbayar required numeric." : GoTo selesai
        End If
        'grnidpr(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "grnidpr required numeric." : GoTo selesai
        End If
        'grnidcs(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "grnidcs required numeric." : GoTo selesai
        End If
        'grnidrq(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "grnidrq required numeric." : GoTo selesai
        End If
        'grnidbs(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "grnidbs required numeric." : GoTo selesai
        End If
        'grnidpo(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "grnidpo required numeric." : GoTo selesai
        End If
        'grnidipc(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "grnidipc required numeric." : GoTo selesai
        End If
        'grnstatusri(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "grnstatusri required numeric." : GoTo selesai
        End If
        'grnstatusdnr(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "grnstatusdnr required numeric." : GoTo selesai
        End If
        'grnstatusprt(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "grnstatusprt required numeric." : GoTo selesai
        End If
        'grnstatus(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "grnstatus required numeric." : GoTo selesai
        End If
        'grnstatussebelumnya(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "grnstatussebelumnya required numeric." : GoTo selesai
        End If
        'grnjmlrevisi(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "grnjmlrevisi required numeric." : GoTo selesai
        End If
        'grncetakanke(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "grncetakanke required numeric." : GoTo selesai
        End If
        'grninputuser(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "grninputuser required numeric." : GoTo selesai
        End If
        'grninputtgl(61) As DateTime
        If (IsDate(dataUtama(61)) = False) Then
            result(2) = "grninputtgl required date." : GoTo selesai
        End If
        'grnmodifikasiuser(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "grnmodifikasiuser required numeric." : GoTo selesai
        End If
        'grnmodifikasitgl(63) As DateTime
        If (IsDate(dataUtama(63)) = False) Then
            result(2) = "grnmodifikasitgl required date." : GoTo selesai
        End If
        'grnposting(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "grnposting required numeric." : GoTo selesai
        End If
        'grntutupperiode(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "grntutupperiode required numeric." : GoTo selesai
        End If
        'grnisclose(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "grnisclose required numeric." : GoTo selesai
        End If
        'grncustomint1(72) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "grncustomint1 required numeric." : GoTo selesai
        End If
        'grncustomint2(73) As Integer
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "grncustomint2 required numeric." : GoTo selesai
        End If
        'grncustomint3(74) As Integer
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "grncustomint3 required numeric." : GoTo selesai
        End If
        'grncustomdbl1(75) As Double
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "grncustomdbl1 required numeric." : GoTo selesai
        End If
        'grncustomdbl2(76) As Double
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "grncustomdbl2 required numeric." : GoTo selesai
        End If
        'grncustomdbl3(77) As Double
        If (IsNumeric(dataUtama(77)) = False) Then
            result(2) = "grncustomdbl3 required numeric." : GoTo selesai
        End If
        'grncustomdate1(78) As Date
        If (IsDate(dataUtama(78)) = False) Then
            result(2) = "grncustomdate1 required date." : GoTo selesai
        End If
        'grncustomdate2(79) As Date
        If (IsDate(dataUtama(79)) = False) Then
            result(2) = "grncustomdate2 required date." : GoTo selesai
        End If
        'grncustomdate3(80) As Date
        If (IsDate(dataUtama(80)) = False) Then
            result(2) = "grncustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'grncabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "grncabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "grncabang should not be more than 25 character." : GoTo selesai
        End If

        'grnlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "grnlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "grnlokasi should not be more than 25 character." : GoTo selesai
        End If

        'grngudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "grngudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "grngudang should not be more than 25 character." : GoTo selesai
        End If

        'grnsumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "grnsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "grnsumber should not be more than 10 character." : GoTo selesai
        End If

        'grnnotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "grnnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "grnnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'grntgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "grntgl can't be empty" : GoTo selesai
        End If

        'grntgljatuhtempo(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "grntgljatuhtempo can't be empty" : GoTo selesai
        End If

        'grntglnoref(28) As Date
        If Len(dataUtama(28)) = 0 Then
            result(2) = "grntglnoref can't be empty" : GoTo selesai
        End If

        'grntglpenutupan(29) As Date
        If Len(dataUtama(29)) = 0 Then
            result(2) = "grntglpenutupan can't be empty" : GoTo selesai
        End If

        'grnmatauang(30) As String
        If Len(dataUtama(30)) = 0 Then
            result(2) = "grnmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(30)) > 25 Then
            result(2) = "grnmatauang should not be more than 25 character." : GoTo selesai
        End If

        'grnkurs(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "grnkurs can't be empty" : GoTo selesai
        End If

        'grntotal(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "grntotal can't be empty" : GoTo selesai
        End If

        'grndiskonpersen(34) As String
        If Len(dataUtama(34)) = 0 Then
            result(2) = "grndiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(34)) > 25 Then
            result(2) = "grndiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'grnjmldiskon(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "grnjmldiskon can't be empty" : GoTo selesai
        End If

        'grntotalpajak1detail(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "grntotalpajak1detail can't be empty" : GoTo selesai
        End If

        'grntotalpajak2detail(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "grntotalpajak2detail can't be empty" : GoTo selesai
        End If

        'grnbiayalainpersen(38) As String
        If Len(dataUtama(38)) = 0 Then
            result(2) = "grnbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(38)) > 25 Then
            result(2) = "grnbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'grnbiayalain(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "grnbiayalain can't be empty" : GoTo selesai
        End If

        'grntotaltransaksi(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "grntotaltransaksi can't be empty" : GoTo selesai
        End If

        'grnjmlbayar(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "grnjmlbayar can't be empty" : GoTo selesai
        End If

        'grninputtgl(61) As DateTime
        If Len(dataUtama(61)) = 0 Then
            result(2) = "grninputtgl can't be empty" : GoTo selesai
        End If

        'grnmodifikasitgl(63) As DateTime
        If Len(dataUtama(63)) = 0 Then
            result(2) = "grnmodifikasitgl can't be empty" : GoTo selesai
        End If

        'grncustomdbl1(75) As Double
        If Len(dataUtama(75)) = 0 Then
            result(2) = "grncustomdbl1 can't be empty" : GoTo selesai
        End If

        'grncustomdbl2(76) As Double
        If Len(dataUtama(76)) = 0 Then
            result(2) = "grncustomdbl2 can't be empty" : GoTo selesai
        End If

        'grncustomdbl3(77) As Double
        If Len(dataUtama(77)) = 0 Then
            result(2) = "grncustomdbl3 can't be empty" : GoTo selesai
        End If

        'grncustomdate1(78) As Date
        If Len(dataUtama(78)) = 0 Then
            result(2) = "grncustomdate1 can't be empty" : GoTo selesai
        End If

        'grncustomdate2(79) As Date
        If Len(dataUtama(79)) = 0 Then
            result(2) = "grncustomdate2 can't be empty" : GoTo selesai
        End If

        'grncustomdate3(80) As Date
        If Len(dataUtama(80)) = 0 Then
            result(2) = "grncustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "grnid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grncabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grngudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnjenispembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnjenispembeliankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grncarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grntgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnsupplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnsupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grn1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grn1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grn1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grn2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grn2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grn2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnbagianpembelian", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grntermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grntgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grncatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grntglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grntglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnhargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grntotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grndiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnjmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grntotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grntotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grntotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnjmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnrekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnrekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnrekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnrekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnrekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnidpr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnidcs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnidrq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnidbs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnidpo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnidipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnstatusri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnstatusdnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnstatusprt", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grncetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grninputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grninputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grnposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grntutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grnisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grncustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grncustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grncustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grncustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grncustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grncustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grncustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grncustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "grncustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grncustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grncustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grncustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grncustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "grncustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "grnid~grncabang~grnlokasi~grngudang~grnasalbarang~grnasalbarangkategori~grnjenispembelian~grnjenispembeliankategori~grncarabayar~grnsumber~grnautonotransaksi~grnnotransaksi~grntgl~grnkodepa~grnsupplier~grnsupplierkontak~grn1alamat1~grn1alamat2~grn1alamat3~grn2alamat1~grn2alamat2~grn2alamat3~grnbagianpembelian~grntermin~grntgljatuhtempo~grnuraian~grncatatan~grnnoref~grntglnoref~grntglpenutupan~grnmatauang~grnkurs~grnhargatermasukpajak~grntotal~grndiskonpersen~grnjmldiskon~grntotalpajak1detail~grntotalpajak2detail~grnbiayalainpersen~grnbiayalain~grntotaltransaksi~grnjmlbayar~grnrekdiskon~grnrekpajak1~grnrekpajak2~grnrekbiayalain~grnrekbayar~grnidpr~grnidcs~grnidrq~grnidbs~grnidpo~grnidipc~grnstatusri~grnstatusdnr~grnstatusprt~grnstatus~grnstatussebelumnya~grnjmlrevisi~grncetakanke~grninputuser~grninputtgl~grnmodifikasiuser~grnmodifikasitgl~grnposting~grntutupperiode~grnisclose~grncustomtext1~grncustomtext2~grncustomtext3~grncustomtext4~grncustomtext5~grncustomint1~grncustomint2~grncustomint3~grncustomdbl1~grncustomdbl2~grncustomdbl3~grncustomdate1~grncustomdate2~grncustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idgrndetail(0) As Integer, idgrn(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, hargafix(12) As Integer, harga(13) As Double, diskon(14) As String, 
        'jmldiskon(15) As Double, pajak1(16) As String, jmlpajak1(17) As Double, pajak2(18) As String, jmlpajak2(19) As Double, 
        'cabang(20) As String, lokasi(21) As String, gudang(22) As String, rekpersediaan(23) As String, rekdiskonpembelian(24) As String, 
        'rekhutangsementara(25) As String, costcenter(26) As String, divisi(27) As String, subdivisi(28) As String, proyek(29) As String, 
        'catatan(30) As String, urutan(31) As Integer, idprdetail(32) As Integer, idcsdetail(33) As Integer, idrqdetail(34) As Integer, 
        'idbsdetail(35) As Integer, idpodetail(36) As Integer, idipcdetail(37) As Integer, jmlri(38) As Double, statusri(39) As Integer, 
        'jmldnr(40) As Double, statusdnr(41) As Integer, jmlprt(42) As Double, statusprt(43) As Integer, isclose(44) As Integer, 
        'customtext1(45) As String, customtext2(46) As String, customtext3(47) As String, customdbl1(48) As Double, customdbl2(49) As Double, 
        'customdbl3(50) As Double, customdate1(51) As Date, customdate2(52) As Date, customdate3(53) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idgrndetail, idgrn, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, gudang, rekpersediaan, rekdiskonpembelian, rekhutangsementara, costcenter, divisi, 
        'subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, 
        'idbsdetail, idpodetail, idipcdetail, jmlri, statusri, jmldnr, statusdnr, 
        'jmlprt, statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idgrndetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idgrn", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "rekpersediaan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekdiskonpembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekhutangsementara", AsEnumTypeData.AsString)
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
        AsDataTableTambahField(dtdetail, "jmlri", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmldnr", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusdnr", AsEnumTypeData.AsInt64)
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
        Dim ftExistOutstandingPO As String = "", ftOutstandingPO As String = "", updNilaiPO As String = "", updFilterPO As String = ""
        Dim idbarang As Integer = 0, idpodetail As Integer = 0, jmlbarang As Double = 0
        Dim gudang As String = "", updStokOutBooking As String = ""

        'FILTER PO, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftPO As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 54) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idgrndetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idgrndetail required numeric." : GoTo selesai
            End If
            'idgrn(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idgrn required numeric." : GoTo selesai
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
            'harga(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'jmldiskon(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - jmldiskon required numeric." : GoTo selesai
            End If
            'jmlpajak1(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak1 required numeric." : GoTo selesai
            End If
            'jmlpajak2(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak2 required numeric." : GoTo selesai
            End If
            'urutan(31) As Integer
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idprdetail(32) As Integer
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - idprdetail required numeric." : GoTo selesai
            End If
            'idcsdetail(33) As Integer
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - idcsdetail required numeric." : GoTo selesai
            End If
            'idrqdetail(34) As Integer
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - idrqdetail required numeric." : GoTo selesai
            End If
            'idbsdetail(35) As Integer
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - idbsdetail required numeric." : GoTo selesai
            End If
            'idpodetail(36) As Integer
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - idpodetail required numeric." : GoTo selesai
            End If
            'idipcdetail(37) As Integer
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - idipcdetail required numeric." : GoTo selesai
            End If
            'jmlri(38) As Double
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - jmlri required numeric." : GoTo selesai
            End If
            'statusri(39) As Integer
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - statusri required numeric." : GoTo selesai
            End If
            'jmldnr(40) As Double
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - jmldnr required numeric." : GoTo selesai
            End If
            'statusdnr(41) As Integer
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - statusdnr required numeric." : GoTo selesai
            End If
            'jmlprt(42) As Double
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - jmlprt required numeric." : GoTo selesai
            End If
            'statusprt(43) As Integer
            If (IsNumeric(dataRowDetail(43)) = False) Then
                result(2) = "Row : " & i & " - statusprt required numeric." : GoTo selesai
            End If
            'isclose(44) As Integer
            If (IsNumeric(dataRowDetail(44)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(48) As Double
            If (IsNumeric(dataRowDetail(48)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(49) As Double
            If (IsNumeric(dataRowDetail(49)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(50) As Double
            If (IsNumeric(dataRowDetail(50)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(51) As Date
            If (IsDate(dataRowDetail(51)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(52) As Date
            If (IsDate(dataRowDetail(52)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(53) As Date
            If (IsDate(dataRowDetail(53)) = False) Then
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

            'harga(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If
            'If dataRowDetail(13) <= 0 Then
            '    result(2) = "Row : " & i & " - harga can't be less than or equal to zero" : GoTo selesai
            'End If

            'diskon(14) As String
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - diskon can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(14)) > 25 Then
                result(2) = "Row : " & i & " - diskon should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskon(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskon can't be empty" : GoTo selesai
                'Else
                '    'HITUNG JMLDISKON : jml(5) As Double, harga(13) As Double, diskon(14) As String
                '    dataRowDetail(15) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(13)), FixQuotes(dataRowDetail(14).ToString))
            End If

            'jmlpajak1(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak1 can't be empty" : GoTo selesai
            End If

            'jmlpajak2(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak2 can't be empty" : GoTo selesai
            End If

            'gudang(22) As String
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - gudang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(22)) > 25 Then
                result(2) = "Row : " & i & " - gudang should not be more than 25 character." : GoTo selesai
            End If

            'jmlri(38) As Double
            If Len(dataRowDetail(38)) = 0 Then
                result(2) = "Row : " & i & " - jmlri can't be empty" : GoTo selesai
            End If

            'jmldnr(40) As Double
            If Len(dataRowDetail(40)) = 0 Then
                result(2) = "Row : " & i & " - jmldnr can't be empty" : GoTo selesai
            End If

            'jmlprt(42) As Double
            If Len(dataRowDetail(42)) = 0 Then
                result(2) = "Row : " & i & " - jmlprt can't be empty" : GoTo selesai
            End If

            'customdbl1(48) As Double
            If Len(dataRowDetail(48)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(49) As Double
            If Len(dataRowDetail(49)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(50) As Double
            If Len(dataRowDetail(50)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(51) As Date
            If Len(dataRowDetail(51)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(52) As Date
            If Len(dataRowDetail(52)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(53) As Date
            If Len(dataRowDetail(53)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idgrndetail~idgrn~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~hargafix~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudang~rekpersediaan~rekdiskonpembelian~rekhutangsementara~costcenter~divisi~subdivisi~proyek~catatan~urutan~idprdetail~idcsdetail~idrqdetail~idbsdetail~idpodetail~idipcdetail~jmlri~statusri~jmldnr~statusdnr~jmlprt~statusprt~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'Set variabel
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , gudang(22) As String        , idpodetail(36) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : gudang = dataRowDetail(22) : idpodetail = dataRowDetail(36)

            'ValidasiBatchSerial
            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'VALIDASI OUTSTANDING -------------------------
            If idpodetail <> 0 Then 'PO
                'CEK PO YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftPO = IIf(Len(ftPO.ToString) = 0, "", ftPO & " OR ")
                ftPO = String.Concat(ftPO, " (pod.idpodetail = " & idpodetail & ") ")

                '1. CEK DATA EXIST
                ftExistOutstandingPO = IIf(Len(ftExistOutstandingPO.ToString) = 0, "", ftExistOutstandingPO & " UNION ")
                ftExistOutstandingPO = String.Concat(ftExistOutstandingPO, "SELECT EXISTS(SELECT 1 FROM m4_po_detail JOIN m4_po ON idpo = poid WHERE idpodetail = '" & idpodetail & "' AND (postatus = 2 OR postatus = 3 OR postatus = 4 OR postatus = 7) LIMIT 1) as rowExists, '" & idpodetail & "' as idpodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idpodetail=" & idpodetail)
                ftOutstandingPO = IIf(Len(ftOutstandingPO.ToString) = 0, "", ftOutstandingPO & " OR ")
                ftOutstandingPO = String.Concat(ftOutstandingPO, " (pod.idpodetail = " & idpodetail & " AND " & Outstanding & " > (pod.jmlbarang - pod.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING
                updNilaiPO = String.Concat("WHEN '" & idpodetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiPO)

                '4. SET FILTER UPDATE OUTSTANDING
                updFilterPO = IIf(Len(updFilterPO.ToString) = 0, "", updFilterPO & " OR ")
                updFilterPO = String.Concat(updFilterPO, "(idpodetail = '" & idpodetail & "')")

                'SET NILAI UPDATE STOK BOOKING (MENGURANGI)
                updStokOutBooking = IIf(Len(updStokOutBooking.ToString) = 0, "", updStokOutBooking & ", ")
                updStokOutBooking = String.Concat(updStokOutBooking, "('" & idbarang & "', '" & gudang & "', ('-" & jmlbarang & "'))") ' idbarang, gudang, jmlbooking
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


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idgrncost(0) As Integer, idgrn(1) As Integer, kodecost(2) As String, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, rekdebit(6) As String, rekkredit(7) As String, kontak(8) As Integer, termasukhpp(9) As Integer, 
        'catatan(10) As String, costcenter(11) As String, divisi(12) As String, subdivisi(13) As String, proyek(14) As String, 
        'urutan(15) As Integer, idprcost(16) As Integer, idcscost(17) As Integer, idrqcost(18) As Integer, idbscost(19) As Integer, 
        'idpocost(20) As Integer, idipccost(21) As Integer, jumlahri(22) As Double, statusri(23) As Integer, jumlahbayar(24) As Double, 
        'statusbayar(25) As Integer, isclose(26) As Integer, customtext1(27) As String, customtext2(28) As String, customtext3(29) As String, 
        'customdbl1(30) As Double, customdbl2(31) As Double, customdbl3(32) As Double, customdate1(33) As Date, customdate2(34) As Date, 
        'customdate3(35) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idgrncost, idgrn, kodecost, matauang, kurs, jumlah, rekdebit, 
        'rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, 
        'proyek, urutan, idprcost, idcscost, idrqcost, idbscost, idpocost, 
        'idipccost, jumlahri, statusri, jumlahbayar, statusbayar, isclose, customtext1, 
        'customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3

        'Buat datatable cost
        Dim dtcost As New DataTable
        AsDataTableTambahField(dtcost, "idgrncost", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "idgrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "kodecost", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "jumlah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "rekdebit", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "rekkredit", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "kontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "termasukhpp", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtcost, "jumlahri", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "statusri", AsEnumTypeData.AsInt64)
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

        'CEK PARAMETER DATA COST
        If dataSplit(4).Length > 0 Then

            'VALIDASI DAN SET DATA COST ======================================================
            'SPLIT PARAMETER DATA COST
            dataCost = dataSplit(4).Split(sptRow)
            'END OF VALIDASI DAN SET DATA COST ===============================================

            'VALIDASI DAN SET DATA ROW Cost ==================================================
            Dim JmlDtCost As Integer = dataCost.Length
            For i = 1 To JmlDtCost
                'SPLIT DATA Cost
                dataRowCost = dataCost(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA Cost -----------------------------------
                'CEK ARRAY DATA Cost
                If (dataRowCost.Length <> 36) Then
                    result(2) = "Row : " & i & " - Invalid Cost transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW Cost ----------------------------

                'VALIDASI TIPE DATA Cost ------------------------------------------
                'idgrncost(0) As Integer
                If (IsNumeric(dataRowCost(0)) = False) Then
                    result(2) = "Cost Row : " & i & " - idgrncost required numeric." : GoTo selesai
                End If
                'idgrn(1) As Integer
                If (IsNumeric(dataRowCost(1)) = False) Then
                    result(2) = "Cost Row : " & i & " - idgrn required numeric." : GoTo selesai
                End If
                'kurs(4) As Double
                If (IsNumeric(dataRowCost(4)) = False) Then
                    result(2) = "Cost Row : " & i & " - kurs required numeric." : GoTo selesai
                End If
                'jumlah(5) As Double
                If (IsNumeric(dataRowCost(5)) = False) Then
                    result(2) = "Cost Row : " & i & " - jumlah required numeric." : GoTo selesai
                End If
                'kontak(8) As Integer
                If (IsNumeric(dataRowCost(8)) = False) Then
                    result(2) = "Cost Row : " & i & " - kontak required numeric." : GoTo selesai
                End If
                'termasukhpp(9) As Integer
                If (IsNumeric(dataRowCost(9)) = False) Then
                    result(2) = "Cost Row : " & i & " - termasukhpp required numeric." : GoTo selesai
                End If
                'urutan(15) As Integer
                If (IsNumeric(dataRowCost(15)) = False) Then
                    result(2) = "Cost Row : " & i & " - urutan required numeric." : GoTo selesai
                End If
                'idprcost(16) As Integer
                If (IsNumeric(dataRowCost(16)) = False) Then
                    result(2) = "Cost Row : " & i & " - idprcost required numeric." : GoTo selesai
                End If
                'idcscost(17) As Integer
                If (IsNumeric(dataRowCost(17)) = False) Then
                    result(2) = "Cost Row : " & i & " - idcscost required numeric." : GoTo selesai
                End If
                'idrqcost(18) As Integer
                If (IsNumeric(dataRowCost(18)) = False) Then
                    result(2) = "Cost Row : " & i & " - idrqcost required numeric." : GoTo selesai
                End If
                'idbscost(19) As Integer
                If (IsNumeric(dataRowCost(19)) = False) Then
                    result(2) = "Cost Row : " & i & " - idbscost required numeric." : GoTo selesai
                End If
                'idpocost(20) As Integer
                If (IsNumeric(dataRowCost(20)) = False) Then
                    result(2) = "Cost Row : " & i & " - idpocost required numeric." : GoTo selesai
                End If
                'idipccost(21) As Integer
                If (IsNumeric(dataRowCost(21)) = False) Then
                    result(2) = "Cost Row : " & i & " - idipccost required numeric." : GoTo selesai
                End If
                'jumlahri(22) As Double
                If (IsNumeric(dataRowCost(22)) = False) Then
                    result(2) = "Cost Row : " & i & " - jumlahri required numeric." : GoTo selesai
                End If
                'statusri(23) As Integer
                If (IsNumeric(dataRowCost(23)) = False) Then
                    result(2) = "Cost Row : " & i & " - statusri required numeric." : GoTo selesai
                End If
                'jumlahbayar(24) As Double
                If (IsNumeric(dataRowCost(24)) = False) Then
                    result(2) = "Cost Row : " & i & " - jumlahbayar required numeric." : GoTo selesai
                End If
                'statusbayar(25) As Integer
                If (IsNumeric(dataRowCost(25)) = False) Then
                    result(2) = "Cost Row : " & i & " - statusbayar required numeric." : GoTo selesai
                End If
                'isclose(26) As Integer
                If (IsNumeric(dataRowCost(26)) = False) Then
                    result(2) = "Cost Row : " & i & " - isclose required numeric." : GoTo selesai
                End If
                'customdbl1(30) As Double
                If (IsNumeric(dataRowCost(30)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdbl1 required numeric." : GoTo selesai
                End If
                'customdbl2(31) As Double
                If (IsNumeric(dataRowCost(31)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdbl2 required numeric." : GoTo selesai
                End If
                'customdbl3(32) As Double
                If (IsNumeric(dataRowCost(32)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdbl3 required numeric." : GoTo selesai
                End If
                'customdate1(33) As Date
                If (IsDate(dataRowCost(33)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdate1 required date." : GoTo selesai
                End If
                'customdate2(34) As Date
                If (IsDate(dataRowCost(34)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdate2 required date." : GoTo selesai
                End If
                'customdate3(35) As Date
                If (IsDate(dataRowCost(35)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdate3 required date." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA Cost -----------------------------------

                'VALIDASI DATA Cost ---------------------------------------
                'kodecost(2) As String
                If Len(dataRowCost(2)) = 0 Then
                    result(2) = "Cost Row : " & i & " - kodecost can't be empty" : GoTo selesai
                End If
                If Len(dataRowCost(2)) > 25 Then
                    result(2) = "Cost Row : " & i & " - kodecost should not be more than 25 character." : GoTo selesai
                End If

                'matauang(3) As String
                If Len(dataRowCost(3)) = 0 Then
                    result(2) = "Cost Row : " & i & " - matauang can't be empty" : GoTo selesai
                End If
                If Len(dataRowCost(3)) > 25 Then
                    result(2) = "Cost Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
                End If

                'kurs(4) As Double
                If Len(dataRowCost(4)) = 0 Then
                    result(2) = "Cost Row : " & i & " - kurs can't be empty" : GoTo selesai
                End If

                'jumlah(5) As Double
                If Len(dataRowCost(5)) = 0 Then
                    result(2) = "Cost Row : " & i & " - jumlah can't be empty" : GoTo selesai
                End If

                If dataRowCost(9) = 0 Then
                    If Len(dataRowCost(6)) = 0 Then
                        result(2) = "Cost Row : " & i & " - rekdebit can't be empty" : GoTo selesai
                    End If
                End If
                If Len(dataRowCost(6)) > 25 Then
                    result(2) = "Cost Row : " & i & " - rekdebit should not be more than 25 character." : GoTo selesai
                End If

                'rekkredit(7) As String
                If Len(dataRowCost(7)) = 0 Then
                    result(2) = "Cost Row : " & i & " - rekkredit can't be empty" : GoTo selesai
                End If
                If Len(dataRowCost(7)) > 25 Then
                    result(2) = "Cost Row : " & i & " - rekkredit should not be more than 25 character." : GoTo selesai
                End If

                'jumlahri(22) As Double
                If Len(dataRowCost(22)) = 0 Then
                    result(2) = "Cost Row : " & i & " - jumlahri can't be empty" : GoTo selesai
                End If

                'jumlahbayar(24) As Double
                If Len(dataRowCost(24)) = 0 Then
                    result(2) = "Cost Row : " & i & " - jumlahbayar can't be empty" : GoTo selesai
                End If

                'customdbl1(30) As Double
                If Len(dataRowCost(30)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
                End If

                'customdbl2(31) As Double
                If Len(dataRowCost(31)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
                End If

                'customdbl3(32) As Double
                If Len(dataRowCost(32)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
                End If

                'customdate1(33) As Date
                If Len(dataRowCost(33)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdate1 can't be empty" : GoTo selesai
                End If

                'customdate2(34) As Date
                If Len(dataRowCost(34)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdate2 can't be empty" : GoTo selesai
                End If

                'customdate3(35) As Date
                If Len(dataRowCost(35)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdate3 can't be empty" : GoTo selesai
                End If

                'END OF VALIDASI DATA Cost --------------------------------

                If AsDataTableTambahData(dtcost, "idgrncost~idgrn~kodecost~matauang~kurs~jumlah~rekdebit~rekkredit~kontak~termasukhpp~catatan~costcenter~divisi~subdivisi~proyek~urutan~idprcost~idcscost~idrqcost~idbscost~idpocost~idipccost~jumlahri~statusri~jumlahbayar~statusbayar~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowCost(0) & "~" & dataRowCost(1) & "~" & dataRowCost(2) & "~" & dataRowCost(3) & "~" & dataRowCost(4) & "~" & dataRowCost(5) & "~" & dataRowCost(6) & "~" & dataRowCost(7) & "~" & dataRowCost(8) & "~" & dataRowCost(9) & "~" & dataRowCost(10) & "~" & dataRowCost(11) & "~" & dataRowCost(12) & "~" & dataRowCost(13) & "~" & dataRowCost(14) & "~" & dataRowCost(15) & "~" & dataRowCost(16) & "~" & dataRowCost(17) & "~" & dataRowCost(18) & "~" & dataRowCost(19) & "~" & dataRowCost(20) & "~" & dataRowCost(21) & "~" & dataRowCost(22) & "~" & dataRowCost(23) & "~" & dataRowCost(24) & "~" & dataRowCost(25) & "~" & dataRowCost(26) & "~" & dataRowCost(27) & "~" & dataRowCost(28) & "~" & dataRowCost(29) & "~" & dataRowCost(30) & "~" & dataRowCost(31) & "~" & dataRowCost(32) & "~" & dataRowCost(33) & "~" & dataRowCost(34) & "~" & dataRowCost(35)) = False Then
                    result(2) = "Cost Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA COST ===========================================

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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("grntgl")), AsFormatTanggal(drutama("grntgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("grnstatus") = 2 Then

                    'VALIDASI BATCH SERIAL ---------------
                    'ValidasiBatchSerial
                    Dim rsValidasi As String = ValidasiBatchSerial(dtdetail, dtbatch, dtserial, ftBarang, "jmlbarang", 1)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    'END OF VALIDASI BATCH SERIAL --------

                    rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstandingPO, ftOutstandingPO, "", "", "", "", ftPO, drutama("grnhargatermasukpajak"))
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("grntermin").ToString, AsFormatTanggal(drutama("grntgl")), "grntgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("grntgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                ''PERHITUNGAN TOTAL UTAMA ================================
                ''DIAMBILKAN DARI DATA DETAIL

                ''TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                ''SUBTOTAL = (jml * harga) - jmldiskon
                'AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                'dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                ''TOTAL = subtotal
                'drutama("grntotal") = AsDataTableDSum(dtdetail, "subtotal")

                ''TOTALPAJAK1 = jmlpajak1
                'drutama("grntotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                ''TOTALPAJAK2 = jmlpajak2
                'drutama("grntotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                ''JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                ''JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'If Integer.Parse(drutama("grnhargatermasukpajak")) = 0 Then
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                '    drutama("grntotaltransaksi") = Double.Parse(drutama("grntotal")) - Double.Parse(drutama("grnjmldiskon")) + Double.Parse(drutama("grntotalpajak1detail")) + Double.Parse(drutama("grntotalpajak2detail")) + Double.Parse(drutama("grnbiayalain"))

                'Else
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                '    drutama("grntotaltransaksi") = Double.Parse(drutama("grntotal")) - Double.Parse(drutama("grnjmldiskon")) + Double.Parse(drutama("grnbiayalain"))

                'End If
                ''END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("grnid")
                    notransaksi = drutama("grnnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(grnid), grnnotransaksi FROM M4_grn WHERE grnid='" & result(4) & "' AND grnstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(grnid) FROM m4_grn WHERE grnnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_grn_history
                        Dim rsSimpanHistory As String = SimpanHistory.m4_Grn_HistorySimpan("" & paramSplit(0) & "★M4_Grn_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("grnsumber")) & "▼" & FixQuotes(drutama("grnid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M4_Grn set grncabang  = '" & FixQuotes(drutama("grncabang")) & "', grnlokasi  = '" & FixQuotes(drutama("grnlokasi")) & "', grngudang  = '" & FixQuotes(drutama("grngudang")) & "', grnasalbarang  = '" & FixQuotes(drutama("grnasalbarang")) & "', grnasalbarangkategori  = " & drutama("grnasalbarangkategori") & ", grnjenispembelian  = '" & FixQuotes(drutama("grnjenispembelian")) & "', grnjenispembeliankategori  = " & drutama("grnjenispembeliankategori") & ", grncarabayar  = " & drutama("grncarabayar") & ", grnsumber  = '" & FixQuotes(drutama("grnsumber")) & "', grnautonotransaksi  = " & drutama("grnautonotransaksi") & ", grnnotransaksi  = '" & FixQuotes(notransaksi) & "', grntgl  = '" & FixQuotes(AsFormatTanggal(drutama("grntgl"))) & "', grnkodepa  = " & drutama("grnkodepa") & ", grnsupplier  = " & drutama("grnsupplier") & ", grnsupplierkontak  = '" & FixQuotes(drutama("grnsupplierkontak")) & "', grn1alamat1  = '" & FixQuotes(drutama("grn1alamat1")) & "', grn1alamat2  = '" & FixQuotes(drutama("grn1alamat2")) & "', grn1alamat3  = '" & FixQuotes(drutama("grn1alamat3")) & "', grn2alamat1  = '" & FixQuotes(drutama("grn2alamat1")) & "', grn2alamat2  = '" & FixQuotes(drutama("grn2alamat2")) & "', grn2alamat3  = '" & FixQuotes(drutama("grn2alamat3")) & "', grnbagianpembelian  = " & drutama("grnbagianpembelian") & ", grntermin  = '" & FixQuotes(drutama("grntermin")) & "', grntgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("grntgljatuhtempo"))) & "', grnuraian  = '" & FixQuotes(drutama("grnuraian")) & "', grncatatan  = '" & FixQuotes(drutama("grncatatan")) & "', grnnoref  = '" & FixQuotes(drutama("grnnoref")) & "', grntglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("grntglnoref"))) & "', grntglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("grntglpenutupan"))) & "', grnmatauang  = '" & FixQuotes(drutama("grnmatauang")) & "', grnkurs  = '" & FixDouble(drutama("grnkurs")) & "', grnhargatermasukpajak  = " & drutama("grnhargatermasukpajak") & ", grntotal  = '" & FixDouble(drutama("grntotal")) & "', grndiskonpersen  = '" & FixQuotes(drutama("grndiskonpersen")) & "', grnjmldiskon  = '" & FixDouble(drutama("grnjmldiskon")) & "', grntotalpajak1detail  = '" & FixDouble(drutama("grntotalpajak1detail")) & "', grntotalpajak2detail  = '" & FixDouble(drutama("grntotalpajak2detail")) & "', grnbiayalainpersen  = '" & FixQuotes(drutama("grnbiayalainpersen")) & "', grnbiayalain  = '" & FixDouble(drutama("grnbiayalain")) & "', grntotaltransaksi  = '" & FixDouble(drutama("grntotaltransaksi")) & "', grnjmlbayar  = '" & FixDouble(drutama("grnjmlbayar")) & "', grnrekdiskon  = '" & FixQuotes(drutama("grnrekdiskon")) & "', grnrekpajak1  = '" & FixQuotes(drutama("grnrekpajak1")) & "', grnrekpajak2  = '" & FixQuotes(drutama("grnrekpajak2")) & "', grnrekbiayalain  = '" & FixQuotes(drutama("grnrekbiayalain")) & "', grnrekbayar  = '" & FixQuotes(drutama("grnrekbayar")) & "', grnidpr  = " & drutama("grnidpr") & ", grnidcs  = " & drutama("grnidcs") & ", grnidrq  = " & drutama("grnidrq") & ", grnidbs  = " & drutama("grnidbs") & ", grnidpo  = " & drutama("grnidpo") & ", grnidipc  = " & drutama("grnidipc") & ", grnstatusri  = " & drutama("grnstatusri") & ", grnstatusdnr  = " & drutama("grnstatusdnr") & ", grnstatusprt  = " & drutama("grnstatusprt") & ", grnstatus  = " & drutama("grnstatus") & ", grnstatussebelumnya  = " & drutama("grnstatussebelumnya") & ", grnjmlrevisi  = grnjmlrevisi+1, grncetakanke  = " & drutama("grncetakanke") & ", grnmodifikasiuser  = " & drutama("grnmodifikasiuser") & ", grnmodifikasitgl  = NOW(), grnposting  = 0, grntutupperiode  = " & drutama("grntutupperiode") & ", grncustomtext1  = '" & FixQuotes(drutama("grncustomtext1")) & "', grncustomtext2  = '" & FixQuotes(drutama("grncustomtext2")) & "', grncustomtext3  = '" & FixQuotes(drutama("grncustomtext3")) & "', grncustomtext4  = '" & FixQuotes(drutama("grncustomtext4")) & "', grncustomtext5  = '" & FixQuotes(drutama("grncustomtext5")) & "', grncustomint1  = " & drutama("grncustomint1") & ", grncustomint2  = " & drutama("grncustomint2") & ", grncustomint3  = " & drutama("grncustomint3") & ", grncustomdbl1  = '" & FixDouble(drutama("grncustomdbl1")) & "', grncustomdbl2  = '" & FixDouble(drutama("grncustomdbl2")) & "', grncustomdbl3  = '" & FixDouble(drutama("grncustomdbl3")) & "', grncustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("grncustomdate1"))) & "', grncustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("grncustomdate2"))) & "', grncustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("grncustomdate3"))) & "' where grnid = '" & drutama("grnid") & "'"
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

                    If drutama("grnautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("grncabang"), drutama("grnlokasi"), drutama("grnsumber"), drutama("grntgl"))
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
                        notransaksi = drutama("grnnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(grnid) FROM m4_grn WHERE grnnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Grn (grncabang, grnlokasi, grngudang, grnasalbarang, grnasalbarangkategori, grnjenispembelian, grnjenispembeliankategori, grncarabayar, grnsumber, grnautonotransaksi, grnnotransaksi, grntgl, grnkodepa, grnsupplier, grnsupplierkontak, grn1alamat1, grn1alamat2, grn1alamat3, grn2alamat1, grn2alamat2, grn2alamat3, grnbagianpembelian, grntermin, grntgljatuhtempo, grnuraian, grncatatan, grnnoref, grntglnoref, grntglpenutupan, grnmatauang, grnkurs, grnhargatermasukpajak, grntotal, grndiskonpersen, grnjmldiskon, grntotalpajak1detail, grntotalpajak2detail, grnbiayalainpersen, grnbiayalain, grntotaltransaksi, grnjmlbayar, grnrekdiskon, grnrekpajak1, grnrekpajak2, grnrekbiayalain, grnrekbayar, grnidpr, grnidcs, grnidrq, grnidbs, grnidpo, grnidipc, grnstatusri, grnstatusdnr, grnstatusprt, grnstatus, grnstatussebelumnya, grnjmlrevisi, grncetakanke, grninputuser, grninputtgl, grnmodifikasiuser, grnmodifikasitgl, grnposting, grntutupperiode, grnisclose, grncustomtext1, grncustomtext2, grncustomtext3, grncustomtext4, grncustomtext5, grncustomint1, grncustomint2, grncustomint3, grncustomdbl1, grncustomdbl2, grncustomdbl3, grncustomdate1, grncustomdate2, grncustomdate3) values('" & FixQuotes(drutama("grncabang")) & "', '" & FixQuotes(drutama("grnlokasi")) & "', '" & FixQuotes(drutama("grngudang")) & "', '" & FixQuotes(drutama("grnasalbarang")) & "', " & drutama("grnasalbarangkategori") & ", '" & FixQuotes(drutama("grnjenispembelian")) & "', " & drutama("grnjenispembeliankategori") & ", " & drutama("grncarabayar") & ", '" & FixQuotes(drutama("grnsumber")) & "', " & drutama("grnautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("grntgl"))) & "', " & drutama("grnkodepa") & ", " & drutama("grnsupplier") & ", '" & FixQuotes(drutama("grnsupplierkontak")) & "', '" & FixQuotes(drutama("grn1alamat1")) & "', '" & FixQuotes(drutama("grn1alamat2")) & "', '" & FixQuotes(drutama("grn1alamat3")) & "', '" & FixQuotes(drutama("grn2alamat1")) & "', '" & FixQuotes(drutama("grn2alamat2")) & "', '" & FixQuotes(drutama("grn2alamat3")) & "', " & drutama("grnbagianpembelian") & ", '" & FixQuotes(drutama("grntermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("grntgljatuhtempo"))) & "', '" & FixQuotes(drutama("grnuraian")) & "', '" & FixQuotes(drutama("grncatatan")) & "', '" & FixQuotes(drutama("grnnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("grntglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("grntglpenutupan"))) & "', '" & FixQuotes(drutama("grnmatauang")) & "', '" & FixDouble(drutama("grnkurs")) & "', " & drutama("grnhargatermasukpajak") & ", '" & FixDouble(drutama("grntotal")) & "', '" & FixQuotes(drutama("grndiskonpersen")) & "', '" & FixDouble(drutama("grnjmldiskon")) & "', '" & FixDouble(drutama("grntotalpajak1detail")) & "', '" & FixDouble(drutama("grntotalpajak2detail")) & "', '" & FixQuotes(drutama("grnbiayalainpersen")) & "', '" & FixDouble(drutama("grnbiayalain")) & "', '" & FixDouble(drutama("grntotaltransaksi")) & "', '" & FixDouble(drutama("grnjmlbayar")) & "', '" & FixQuotes(drutama("grnrekdiskon")) & "', '" & FixQuotes(drutama("grnrekpajak1")) & "', '" & FixQuotes(drutama("grnrekpajak2")) & "', '" & FixQuotes(drutama("grnrekbiayalain")) & "', '" & FixQuotes(drutama("grnrekbayar")) & "', " & drutama("grnidpr") & ", " & drutama("grnidcs") & ", " & drutama("grnidrq") & ", " & drutama("grnidbs") & ", " & drutama("grnidpo") & ", " & drutama("grnidipc") & ", " & drutama("grnstatusri") & ", " & drutama("grnstatusdnr") & ", " & drutama("grnstatusprt") & ", " & drutama("grnstatus") & ", " & drutama("grnstatussebelumnya") & ", " & drutama("grnjmlrevisi") & ", " & drutama("grncetakanke") & ", " & drutama("grninputuser") & ", NOW(), " & drutama("grnmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("grntutupperiode") & ", " & drutama("grnisclose") & ", '" & FixQuotes(drutama("grncustomtext1")) & "', '" & FixQuotes(drutama("grncustomtext2")) & "', '" & FixQuotes(drutama("grncustomtext3")) & "', '" & FixQuotes(drutama("grncustomtext4")) & "', '" & FixQuotes(drutama("grncustomtext5")) & "', " & drutama("grncustomint1") & ", " & drutama("grncustomint2") & ", " & drutama("grncustomint3") & ", '" & FixDouble(drutama("grncustomdbl1")) & "', '" & FixDouble(drutama("grncustomdbl2")) & "', '" & FixDouble(drutama("grncustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("grncustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("grncustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("grncustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select grnid from M4_grn where grnnotransaksi='" & notransaksi & "' AND grninputuser= '" & userid & "' order by grnmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Grn_Detail where idgrn = '" & result(4) & "'"
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
                    Dim dtPO As New DataTable
                    Dim strValue2 As New StringBuilder

                    For Each dr1 As DataRow In dtdetail.Rows

                        'VALIDASI MATAUANG HARUS SAMA DENGAN TRANSAKSI UTAMA --------------------
                        If Not drutama("grnmatauang").ToString.Equals(dr1("matauang").ToString) Then
                            result(2) = "Row : " & dr1("urutan") & " - " & dr1("tipebarang") & " | " & dr1("namabarang") & " currency (" & dr1("matauang") & ") doesn't belong to the main transactions." : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF VALIDASI MATAUANG HARUS SAMA DENGAN TRANSAKSI UTAMA -------------


                        'SET HARGA DARI PO ------------------------------------------------------
                        sql = "SELECT jml, harga, diskon, pajak1, jmlpajak1, pajak2, jmlpajak2 FROM m4_po_detail WHERE idpodetail = '" & FixDouble(dr1("idpodetail")) & "'"
                        dtPO = AsDataTableAmbilDariDB(sql)
                        If dtPO.Rows.Count > 0 Then
                            'SET HARGA - ambil dari PO
                            dr1("harga") = Double.Parse(dtPO.Rows(0)("harga"))

                            'SET DISKON - ambil dari PO
                            dr1("diskon") = dtPO.Rows(0)("diskon")

                            'SET JMLDISKON - hitung diskon
                            dr1("jmldiskon") = F_Diskon(Double.Parse(dr1("jml")), Double.Parse(dr1("harga")), FixQuotes(dr1("diskon").ToString))

                            'SET PAJAK1 - ambil dari po
                            dr1("pajak1") = dtPO.Rows(0)("pajak1")

                            'SET JMLPAJAK1 - ambil dari po = (jmlpajakpo / jmlpo) * jml
                            dr1("jmlpajak1") = (Double.Parse(dtPO.Rows(0)("jmlpajak1")) / Double.Parse(dtPO.Rows(0)("jml"))) * Double.Parse(dr1("jml"))

                            'SET PAJAK2 - ambil dari po
                            dr1("pajak2") = dtPO.Rows(0)("pajak2")

                            'SET JMLPAJAK2 - ambil dari po = (jmlpajakpo / jmlpo) * jml
                            dr1("jmlpajak2") = (Double.Parse(dtPO.Rows(0)("jmlpajak2")) / Double.Parse(dtPO.Rows(0)("jml"))) * Double.Parse(dr1("jml"))
                        End If
                        'END OF SET HARGA DARI PO -----------------------------------------------


                        'QUERY INSERT DETAIL
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idgrndetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', " & dr1("hargafix") & ", '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("rekdiskonpembelian")) & "', '" & FixQuotes(dr1("rekhutangsementara")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idprdetail") & ", " & dr1("idcsdetail") & ", " & dr1("idrqdetail") & ", " & dr1("idbsdetail") & ", " & dr1("idpodetail") & ", " & dr1("idipcdetail") & ", '" & FixDouble(dr1("jmlri")) & "', " & dr1("statusri") & ", '" & FixDouble(dr1("jmldnr")) & "', " & dr1("statusdnr") & ", '" & FixDouble(dr1("jmlprt")) & "', " & dr1("statusprt") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Grn_Detail(idgrndetail, idgrn, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, rekpersediaan, rekdiskonpembelian, rekhutangsementara, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                'Hapus cost ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_grn_Cost where idgrn = " & result(4)
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
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
                        strValue2.Append("(" & dr1("idgrncost") & ", " & result(4) & ", '" & FixQuotes(dr1("kodecost")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixQuotes(dr1("rekdebit")) & "', '" & FixQuotes(dr1("rekkredit")) & "', " & dr1("kontak") & ", " & dr1("termasukhpp") & ", '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("idprcost") & ", " & dr1("idcscost") & ", " & dr1("idrqcost") & ", " & dr1("idbscost") & ", " & dr1("idpocost") & ", " & dr1("idipccost") & ", '" & FixDouble(dr1("jumlahri")) & "', " & dr1("statusri") & ", '" & FixDouble(dr1("jumlahbayar")) & "', " & dr1("statusbayar") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Grn_Cost(idgrncost, idgrn, kodecost, matauang, kurs, jumlah, rekdebit, rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, proyek, urutan, idprcost, idcscost, idrqcost, idbscost, idpocost, idipccost, jumlahri, statusri, jumlahbayar, statusbayar, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Hapus batch ketika update
                If (isUpdate) Then
                    sql = "Delete from M1_No_Batch_Transaction  where nbtidtransaksi  = '" & result(4) & "' AND nbtsumber = 'GRN'"
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
                    sql = "Delete from M1_No_Serial_Transaction where nstidtransaksi  = '" & result(4) & "' AND nstsumber = 'GRN'"
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


                If drutama("grnstatus") = 2 Then

                    'UPDATE OUTSTANDING TRANSAKSI ======================================================
                    If Len(updNilaiPO) > 0 Then 'PO
                        'UPDATE DETAIL
                        sql = "UPDATE m4_po_detail SET jmlrealisasi = (CASE idpodetail " & updNilaiPO & " ELSE jmlrealisasi END) WHERE " & updFilterPO
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idpo FROM m4_po_detail WHERE " & updFilterPO & " GROUP BY idpo")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idpo = '" & dr1("idpo") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDB("SELECT idpo, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m4_po_detail WHERE " & ftDetail & " GROUP BY idpo")
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiPO = "" : updFilterPO = ""
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
                                updNilaiPO = String.Concat(updNilaiPO, "WHEN '" & dr1("idpo") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterPO = IIf(Len(updFilterPO.ToString) = 0, "", updFilterPO & " OR ")
                                updFilterPO = String.Concat(updFilterPO, "(poid = '" & dr1("idpo") & "')")
                            Next

                            sql = "UPDATE m4_po SET postatusrealisasi = (CASE poid " & updNilaiPO & " ELSE postatusrealisasi END) WHERE " & updFilterPO
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


                    'UPDATE STOK BOOKING ============================================================
                    If Len(updStokOutBooking) > 0 Then
                        sql = "INSERT INTO m1_item_booking_po (idbarang, gudang, jmlbooking) VALUES " & updStokOutBooking & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF UPDATE STOK BOOKING =====================================================


                    'AMBIL DATA DETAIL YANG BARU ++++++++++++++++++++++++++++++++++++++++++++++++++++
                    Dim dtDetailNew As DataTable = AsDataTableAmbilDariDB("SELECT grnd.idgrndetail, grnd.idbarang, grnd.namabarang, grnd.tipebarang, grnd.jml, grnd.satuan, grnd.jmlbarang, grnd.satuanbarang, grnd.matauang, grnd.kurs, grnd.harga, grnd.diskon, grnd.jmldiskon, grnd.gudang, grnd.catatan, grnd.costcenter, grnd.divisi, grnd.subdivisi, grnd.proyek, grn.grninputtgl, i.bhpp, grnd.jmlpajak1, grnd.jmlpajak2 FROM m4_grn_detail grnd JOIN m4_grn grn ON grnd.idgrn = grn.grnid JOIN m1_item i ON grnd.idbarang = i.bid WHERE grnd.idgrn = '" & result(4) & "' ORDER BY grnd.urutan")

                    Dim hpp As Double = 0, postinghpp As Double = 0, bstok As Double = 0
                    Dim jenismutasi As Double = 0, saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0
                    Dim strTransaksiBarang As New StringBuilder, dtSaldo As New DataTable

                    If dtDetailNew.Rows.Count > 0 Then

                        'INSERT ITEM TRANSACTION ====================================================
                        For Each dr1 As DataRow In dtDetailNew.Rows
                            'SET NILAI VARIABEL
                            idbarang = Double.Parse(dr1("idbarang"))
                            jmlbarang = Double.Parse(dr1("jmlbarang"))
                            gudang = dr1("gudang")

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
                                'mapping                        id,                            cabang,                                    lokasi,                               gudang,                         kodepa,           jenismutasi,                              sumber,                     idutama,             iddetail,                      notransaksi,                                                  tgl,                            kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,          idhppikm,  idhppikk,                hpp,                                  uraian,                                    catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                              inputtgl,                                                inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("grncabang")) & "', '" & FixQuotes(drutama("grnlokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', " & drutama("grnkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("grnsumber")) & "', " & result(4) & ", " & dr1("idgrndetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("grntgl"))) & "', " & drutama("grnsupplier") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & 0 & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("grnuraian")) & "', '" & FixQuotes(drutama("grncatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr1("grninputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("grninputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
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
                                'sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "', bhargabeli = '" & FixDouble(Double.Parse(dr1("kurs")) * Double.Parse(dr1("harga"))) & "' WHERE bid = '" & idbarang & "'"
                                If drutama("grnhargatermasukpajak") = 0 Then
                                    sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "', bhargabeli = '" & FixDouble((Double.Parse(dr1("harga")) * Double.Parse(dr1("kurs"))) - ((Double.Parse(dr1("jmldiskon")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs"))) + ((Double.Parse(dr1("jmlpajak1")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs"))) + ((Double.Parse(dr1("jmlpajak2")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs")))) & "' WHERE bid = '" & idbarang & "'"
                                Else
                                    sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "', bhargabeli = '" & FixDouble((Double.Parse(dr1("harga")) * Double.Parse(dr1("kurs"))) - ((Double.Parse(dr1("jmldiskon")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs")))) & "' WHERE bid = '" & idbarang & "'"
                                End If
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


                'INSERT MSMQ COGS ===================================================================
                Dim sumber As String = "GRN", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("grnstatus") = 2 Then
                    Dim Security As New ClsSecurity, mjid As String = "", hasilMsmq As String = ""
                    'BUAT ID UNIQUE
                    mjid = Security.MD5CalcString(userid & sumber & result(4) & Now) 'RandomId.Generate(15)

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
                'END OF INSERT MSMQ COGS ============================================================


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
    Public Function M4_GrnUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("grnsupplierkode", "c1.kkode")
            Filter = Filter.Replace("grnsuppliernama", "c1.knama")
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
            Dim sumber As String = "GRN", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Grntgl, Grnnotransaksi, Grnstatus FROM M4_Grn WHERE Grnid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================



            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Grnstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m4_grn_history
            Dim rsSimpanHistory As String = SimpanHistory.m4_Grn_HistorySimpan("" & paramSplit(0) & "★M4_Grn_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m4_grn_terkait")
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


                'UPDATE STOK DAN OUTSTANDING ====================================================
                Dim ftHppI As String = "", ftHppF As String = ""
                Dim ftExistStok As String = "", ftStok As String = ""
                Dim updNilaiPO As String = "", updFilterPO As String = ""
                Dim updStokOut As String = "", gudangOut As String = "", updStokInBooking As String = ""
                Dim updStokBarang As String = "", ftStokBarang As String = ""
                Dim idbarang As Integer = 0, idgrndetail As Integer = 0, idpodetail As Integer = 0, jmlbarang As Double = 0

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDB("SELECT idgrndetail, idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idpodetail, gudang, urutan FROM m4_grn_detail WHERE idgrn = '" & idtransaksi & "'")
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        '1. SET NILAI
                        idbarang = dr1("idbarang") : idgrndetail = dr1("idgrndetail") : idpodetail = dr1("idpodetail") : jmlbarang = dr1("jmlbarang") : gudangOut = dr1("gudang")

                        '2. BUAT FILTER CEK HPP KHUSUS(I)
                        ftHppI = IIf(Len(ftHppI.ToString) = 0, "", ftHppI & " OR ")
                        ftHppI = String.Concat(ftHppI, "(idbarang = '" & idbarang & "' AND idtransaksi = '" & idgrndetail & "' AND sumber = 'GRN')")

                        '3. BUAT FILER CEK HPP FIFO(F)
                        ftHppF = IIf(Len(ftHppF.ToString) = 0, "", ftHppF & " OR ")
                        ftHppF = String.Concat(ftHppF, "(cfiidbarang = '" & idbarang & "' AND cfiidtransaksi = '" & idgrndetail & "' AND cfisumber = 'GRN')")

                        '4. BUAT FILTER CEK STOCK EXIST
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

                        '7. BUAT FILTER UPDATE OUTSTANDING
                        If idpodetail <> 0 Then
                            '7.1 SET NILAI UPDATE OUTSTANDING
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idpodetail=" & idpodetail)
                            updNilaiPO = String.Concat("WHEN '" & idpodetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiPO)

                            '7.2. SET FILTERUPDATE OUTSTANDING
                            updFilterPO = IIf(Len(updFilterPO.ToString) = 0, "", updFilterPO & " OR ")
                            updFilterPO = String.Concat(updFilterPO, "(idpodetail = '" & idpodetail & "')")

                            'SET NILAI UPDATE STOK BOOKING MASUK
                            updStokInBooking = IIf(Len(updStokInBooking.ToString) = 0, "", updStokInBooking & ", ")
                            updStokInBooking = String.Concat(updStokInBooking, "('" & idbarang & "', '" & gudangOut & "', ('" & jmlbarang & "'))") ' idbarang, kgudang, stok
                        End If

                        '8 SET NILAI UPDATE STOK BARANG
                        Dim stokBarang As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang)
                        updStokBarang = String.Concat("WHEN '" & idbarang & "' THEN ROUND(bstok - '" & stokBarang & "', 5) ", updStokBarang)

                        '9. SET FILTERUPDATE STOK BARANG
                        ftStokBarang = IIf(Len(ftStokBarang.ToString) = 0, "", ftStokBarang & " OR ")
                        ftStokBarang = String.Concat(ftStokBarang, "(bid = '" & idbarang & "')")

                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                'VALIDASI HPP, STOK ==========================================================
                'ValidasiSimpan
                Dim rsValidasi As String = ValidasiSimpan(dtdetail, "", "", ftHppI, ftHppF, ftExistStok, ftStok, "", "")
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'END OF VALIDASI HPP, STOK ===================================================


                If Len(updFilterPO) > 0 Then
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m4_po_detail SET jmlrealisasi = (CASE idpodetail " & updNilaiPO & " ELSE jmlrealisasi END) WHERE " & updFilterPO
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idpo FROM m4_po_detail WHERE " & updFilterPO & " GROUP BY idpo")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idpo = '" & dr1("idpo") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDB("SELECT idpo, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m4_po_detail WHERE " & ftDetail & " GROUP BY idpo")
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiPO = "" : updFilterPO = ""
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
                            updNilaiPO = String.Concat(updNilaiPO, "WHEN '" & dr1("idpo") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterPO = IIf(Len(updFilterPO.ToString) = 0, "", updFilterPO & " OR ")
                            updFilterPO = String.Concat(updFilterPO, "(poid = '" & dr1("idpo") & "')")
                        Next

                        sql = "UPDATE m4_po SET postatusrealisasi = (CASE poid " & updNilaiPO & " ELSE postatusrealisasi END) WHERE " & updFilterPO
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
                'END OF UPDATE STOK DAN OUTSTANDING ===========================================


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


                'UPDATE STOK BOOKING ============================================================
                If Len(updStokInBooking) > 0 Then
                    sql = "INSERT INTO m1_item_booking_po (idbarang, gudang, jmlbooking) VALUES " & updStokInBooking & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE STOK BOOKING =====================================================


                'UPDATE STOK ==================================================================
                'STOK KELUAR
                sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokOut & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

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
                'END OF UPDATE STOK ===========================================================


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


                'UPDATE BHPPAVERAGE M1_ITEM ===================================================
                'sql = "  UPDATE m1_item i"
                'sql &= " JOIN m4_grn_detail grnd ON i.bid = grnd.idbarang AND grnd.idgrn = '" & FixDouble(idtransaksi) & "'"
                'sql &= " LEFT JOIN"
                'sql &= " (SELECT i.bid as idbarang, ROUND(SUM(it.jmlbarang * it.hpp) / SUM(it.jmlbarang),2) as hppaverage"
                'sql &= " FROM m1_item_transaction it"
                'sql &= " JOIN m1_item i ON it.idbarang = i.bid AND it.jenismutasi = 1"
                'sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1"
                'sql &= " JOIN m4_grn_detail grnd ON it.idbarang = grnd.idbarang AND grnd.idgrn = '" & FixDouble(idtransaksi) & "'"
                'sql &= " JOIN m4_grn grn ON grnd.idgrn = grn.grnid AND CONCAT(it.sumber,it.idutama) <> CONCAT(grn.grnsumber,grn.grnid)"
                'sql &= " GROUP BY it.idbarang) as h ON i.bid = h.idbarang"
                'sql &= " SET i.bhppaverage = (CASE i.bjenis WHEN 'P' THEN (CASE i.bstok WHEN 0 THEN 0 ELSE IFNULL(h.hppaverage,0) END) ELSE IFNULL(h.hppaverage,0) END)"

                Dim dtTotalFungsional As DataTable = AsDataTableAmbilDariDB("SELECT SUM((CASE grn.grnhargatermasukpajak WHEN 0 THEN ((grnd.jml * grnd.harga) - grnd.jmldiskon) * grnd.kurs ELSE ((grnd.jml * grnd.harga) - grnd.jmldiskon - grnd.jmlpajak1 - grnd.jmlpajak2) * grnd.kurs END)) as total FROM m4_grn_detail grnd JOIN m4_grn grn ON grnd.idgrn = grn.grnid WHERE grnd.idgrn = '" & FixDouble(idtransaksi) & "'")
                Dim dtBiayaFungsional As DataTable = AsDataTableAmbilDariDB("SELECT IFNULL(SUM(grnc.jumlah * grnc.kurs),0) as biaya FROM m4_grn grn LEFT JOIN m4_grn_cost grnc ON grn.grnid = grnc.idgrn AND grnc.termasukhpp = 1 WHERE grn.grnid = '" & FixDouble(idtransaksi) & "'")
                Dim vTotalFungsional As Double = 0, vBiayaFungsional As Double = 0
                If dtTotalFungsional.Rows.Count > 0 Then
                    vTotalFungsional = Double.Parse(FixDouble(FxDB(dtTotalFungsional.Rows(0)("total"), 0)))
                End If
                If dtBiayaFungsional.Rows.Count > 0 Then
                    vBiayaFungsional = Double.Parse(FixDouble(FxDB(dtBiayaFungsional.Rows(0)("biaya"), 0)))
                End If

                sql = "  UPDATE m1_item i"
                sql &= " JOIN ("
                sql &= " SELECT grnd.idbarang, "
                sql &= " ROUND((CASE " & FixDouble(vTotalFungsional) & " "
                sql &= " WHEN 0 THEN (SUM((CASE grn.grnhargatermasukpajak WHEN 0 THEN ((grnd.jml * grnd.harga) - grnd.jmldiskon) * grnd.kurs ELSE ((grnd.jml * grnd.harga) - grnd.jmldiskon - grnd.jmlpajak1 - grnd.jmlpajak2) * grnd.kurs END))) "
                sql &= " ELSE (SUM((CASE grn.grnhargatermasukpajak WHEN 0 THEN ((grnd.jml * grnd.harga) - grnd.jmldiskon) * grnd.kurs ELSE ((grnd.jml * grnd.harga) - grnd.jmldiskon - grnd.jmlpajak1 - grnd.jmlpajak2) * grnd.kurs END))) "
                sql &= " + (((SUM((CASE grn.grnhargatermasukpajak WHEN 0 THEN ((grnd.jml * grnd.harga) - grnd.jmldiskon) * grnd.kurs ELSE ((grnd.jml * grnd.harga) - grnd.jmldiskon - grnd.jmlpajak1 - grnd.jmlpajak2) * grnd.kurs END))) "
                sql &= " / " & FixDouble(vTotalFungsional) & ") * " & FixDouble(vBiayaFungsional) & ") END), 2) as nilai, "
                sql &= " SUM(grnd.jmlbarang) as jumlah "
                sql &= " FROM m4_grn_detail grnd "
                sql &= " JOIN m4_grn grn ON grnd.idgrn = grn.grnid "
                sql &= " WHERE grnd.idgrn = '" & FixDouble(idtransaksi) & "'"
                sql &= " GROUP BY grnd.idbarang"
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
            sql = "UPDATE M4_Grn SET Grnstatus = " & nilaiStatus & ", Grnmodifikasiuser='" & userid & "', Grnmodifikasitgl = NOW(), Grnposting = 0, Grnpostingtgl = '1971-01-01 00:00:00', Grnjmlrevisi = Grnjmlrevisi + 1 WHERE Grnid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_GrnSearch(PostWsSearch(paramSplit(0), "M4_GrnSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_GrnDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("grnsupplierkode", "c1.kkode")
            Filter = Filter.Replace("grnsuppliernama", "c1.knama")
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
            Dim sumber As String = "GRN", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Grnid, Grnnotransaksi FROM M4_Grn WHERE Grnid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT grncabang, grnlokasi, grnsumber, grnautonotransaksi, grnnotransaksi, grntgl"
            sql &= " FROM M4_grn"
            sql &= " WHERE grnid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("grncabang")
                lokasi = dtNomorNext.Rows(0)("grnlokasi")
                sumber = dtNomorNext.Rows(0)("grnsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("grnautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("grnnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("grntgl"))
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

            'DELETE COST
            sql = "DELETE FROM M4_grn_Cost WHERE idgrn ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M4_Grn_Detail WHERE idgrn = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE UTAMA
            sql = "DELETE FROM M4_Grn WHERE grnid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_GrnSearch(PostWsSearch(paramSplit(0), "M4_GrnSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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