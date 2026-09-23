Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m7_ae
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M7_AeSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataMaster(), dataRowMaster() As String

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
        If (dataSplit.Length <> 3) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'aeid(0) As , aecabang(1) As String, aelokasi(2) As String, aesumber(3) As String, aeautonotransaksi(4) As Integer, 
        'aenotransaksi(5) As String, aetgl(6) As Date, aekodepa(7) As , aesupplier(8) As , aesupplierkontak(9) As String, 
        'ae1alamat1(10) As String, ae1alamat2(11) As String, ae1alamat3(12) As String, ae2alamat1(13) As String, ae2alamat2(14) As String, 
        'ae2alamat3(15) As String, aebagianpembelian(16) As , aetermin(17) As String, aetgljatuhtempo(18) As Date, aeuraian(19) As String, 
        'aecatatan(20) As String, aenoref(21) As String, aetglnoref(22) As Date, aetglpenutupan(23) As Date, aematauang(24) As String, 
        'aekurs(25) As Double, aehargatermasukpajak(26) As Integer, aetotal(27) As Double, aediskonpersen(28) As String, aejmldiskon(29) As Double, 
        'aetotalpajak1detail(30) As Double, aetotalpajak2detail(31) As Double, aebiayalainpersen(32) As String, aebiayalain(33) As Double, aetotaltransaksi(34) As Double, 
        'aejmlbayar(35) As Double, aerekdiskon(36) As String, aerekpajak1(37) As String, aerekpajak2(38) As String, aerekbiayalain(39) As String, 
        'aerekbayar(40) As String, aeidar(41) As , aeidaq(42) As , aeidab(43) As , aeidao(44) As , 
        'aestatus(45) As Integer, aestatussebelumnya(46) As Integer, aejmlrevisi(47) As Integer, 
        'aecetakanke(48) As Integer, aeinputuser(49) As , aeinputtgl(50) As DateTime, aemodifikasiuser(51) As , aemodifikasitgl(52) As DateTime, 
        'aeposting(53) As Integer, aepostingtgl(54) As DateTime, aetutupperiode(55) As Integer, aeisclose(56) As Integer, aecustomtext1(57) As String, 
        'aecustomtext2(58) As String, aecustomtext3(59) As String, aecustomtext4(60) As String, aecustomtext5(61) As String, aecustomint1(62) As Integer, 
        'aecustomint2(63) As Integer, aecustomint3(64) As Integer, aecustomdbl1(65) As Double, aecustomdbl2(66) As Double, aecustomdbl3(67) As Double, 
        'aecustomdate1(68) As Date, aecustomdate2(69) As Date, aecustomdate3(70) As Date, aecarabayar(71) as Integer, aestatuslunas(72) as Integer, aetgllunas(73) as Date,
        'aenofakturpajak(74) As String, aesdhbayarpajak(75) As Integer, aetglbayarpajak(76) As Date


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'aeid, aecabang, aelokasi, aesumber, aeautonotransaksi, aenotransaksi, aetgl, 
        'aekodepa, aesupplier, aesupplierkontak, ae1alamat1, ae1alamat2, ae1alamat3, ae2alamat1, 
        'ae2alamat2, ae2alamat3, aebagianpembelian, aetermin, aetgljatuhtempo, aeuraian, aecatatan, 
        'aenoref, aetglnoref, aetglpenutupan, aematauang, aekurs, aehargatermasukpajak, aetotal, 
        'aediskonpersen, aejmldiskon, aetotalpajak1detail, aetotalpajak2detail, aebiayalainpersen, aebiayalain, aetotaltransaksi, 
        'aejmlbayar, aerekdiskon, aerekpajak1, aerekpajak2, aerekbiayalain, aerekbayar, aeidar, 
        'aeidaq, aeidab, aeidao, aestatus, aestatussebelumnya, 
        'aejmlrevisi, aecetakanke, aeinputuser, aeinputtgl, aemodifikasiuser, aemodifikasitgl, aeposting, 
        'aepostingtgl, aetutupperiode, aeisclose, aecustomtext1, aecustomtext2, aecustomtext3, aecustomtext4, 
        'aecustomtext5, aecustomint1, aecustomint2, aecustomint3, aecustomdbl1, aecustomdbl2, aecustomdbl3, 
        'aecustomdate1, aecustomdate2, aecustomdate3, aecarabayar, aestatuslunas, aetgllunas,
        'aenofakturpajak, aesdhbayarpajak, aetglbayarpajak

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 77) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'aeautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "aeautonotransaksi required numeric." : GoTo selesai
        End If
        'aetgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "aetgl required date." : GoTo selesai
        End If
        'aetgljatuhtempo(18) As Date
        If (IsDate(dataUtama(18)) = False) Then
            result(2) = "aetgljatuhtempo required date." : GoTo selesai
        End If
        'aetglnoref(22) As Date
        If (IsDate(dataUtama(22)) = False) Then
            result(2) = "aetglnoref required date." : GoTo selesai
        End If
        'aetglpenutupan(23) As Date
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "aetglpenutupan required date." : GoTo selesai
        End If
        'aekurs(25) As Double
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "aekurs required numeric." : GoTo selesai
        End If
        'aehargatermasukpajak(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "aehargatermasukpajak required numeric." : GoTo selesai
        End If
        'aetotal(27) As Double
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "aetotal required numeric." : GoTo selesai
        End If
        'aejmldiskon(29) As Double
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "aejmldiskon required numeric." : GoTo selesai
        End If
        'aetotalpajak1detail(30) As Double
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "aetotalpajak1detail required numeric." : GoTo selesai
        End If
        'aetotalpajak2detail(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "aetotalpajak2detail required numeric." : GoTo selesai
        End If
        'aebiayalain(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "aebiayalain required numeric." : GoTo selesai
        End If
        'aetotaltransaksi(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "aetotaltransaksi required numeric." : GoTo selesai
        End If
        'aejmlbayar(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "aejmlbayar required numeric." : GoTo selesai
        End If
        'aestatus(45) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "aestatus required numeric." : GoTo selesai
        End If
        'aestatussebelumnya(46) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "aestatussebelumnya required numeric." : GoTo selesai
        End If
        'aejmlrevisi(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "aejmlrevisi required numeric." : GoTo selesai
        End If
        'aecetakanke(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "aecetakanke required numeric." : GoTo selesai
        End If
        'aeinputtgl(50) As DateTime
        If (IsDate(dataUtama(50)) = False) Then
            result(2) = "aeinputtgl required date." : GoTo selesai
        End If
        'aemodifikasitgl(52) As DateTime
        If (IsDate(dataUtama(52)) = False) Then
            result(2) = "aemodifikasitgl required date." : GoTo selesai
        End If
        'aeposting(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "aeposting required numeric." : GoTo selesai
        End If
        'aepostingtgl(54) As DateTime
        If (IsDate(dataUtama(54)) = False) Then
            result(2) = "aepostingtgl required date." : GoTo selesai
        End If
        'aetutupperiode(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "aetutupperiode required numeric." : GoTo selesai
        End If
        'aeisclose(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "aeisclose required numeric." : GoTo selesai
        End If
        'aecustomint1(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "aecustomint1 required numeric." : GoTo selesai
        End If
        'aecustomint2(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "aecustomint2 required numeric." : GoTo selesai
        End If
        'aecustomint3(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "aecustomint3 required numeric." : GoTo selesai
        End If
        'aecustomdbl1(65) As Double
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "aecustomdbl1 required numeric." : GoTo selesai
        End If
        'aecustomdbl2(66) As Double
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "aecustomdbl2 required numeric." : GoTo selesai
        End If
        'aecustomdbl3(67) As Double
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "aecustomdbl3 required numeric." : GoTo selesai
        End If
        'aecustomdate1(68) As Date
        If (IsDate(dataUtama(68)) = False) Then
            result(2) = "aecustomdate1 required date." : GoTo selesai
        End If
        'aecustomdate2(69) As Date
        If (IsDate(dataUtama(69)) = False) Then
            result(2) = "aecustomdate2 required date." : GoTo selesai
        End If
        'aecustomdate3(70) As Date
        If (IsDate(dataUtama(70)) = False) Then
            result(2) = "aecustomdate3 required date." : GoTo selesai
        End If
        'aecarabayar(71) As Date
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "aecarabayar required numberic." : GoTo selesai
        End If
        'aestatuslunas(72) As Date
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "aestatuslunas required numeric." : GoTo selesai
        End If
        'aetgllunas(73) As Date
        If (IsDate(dataUtama(73)) = False) Then
            result(2) = "aetgllunas required date." : GoTo selesai
        End If
        'aesdhbayarpajak(75) As Date
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "aesdhbayarpajak required numeric." : GoTo selesai
        End If
        'aetglbayarpajak(76) As Date
        If (IsDate(dataUtama(76)) = False) Then
            result(2) = "aetglbayarpajak required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'aeid(0) As 
        If Len(dataUtama(0)) = 0 Then
            result(2) = "aeid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 20 Then
            result(2) = "aeid should not be more than 20 character." : GoTo selesai
        End If

        'aecabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "aecabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "aecabang should not be more than 25 character." : GoTo selesai
        End If

        'aelokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "aelokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "aelokasi should not be more than 25 character." : GoTo selesai
        End If

        'aesumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "aesumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "aesumber should not be more than 10 character." : GoTo selesai
        End If

        'aenotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "aenotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "aenotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'aetgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "aetgl can't be empty" : GoTo selesai
        End If

        'aekodepa(7) As 
        If Len(dataUtama(7)) = 0 Then
            result(2) = "aekodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 20 Then
            result(2) = "aekodepa should not be more than 20 character." : GoTo selesai
        End If

        'aesupplier(8) As 
        If Len(dataUtama(8)) = 0 Then
            result(2) = "aesupplier can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 20 Then
            result(2) = "aesupplier should not be more than 20 character." : GoTo selesai
        End If

        'aebagianpembelian(16) As 
        If Len(dataUtama(16)) = 0 Then
            result(2) = "aebagianpembelian can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(16)) > 20 Then
            result(2) = "aebagianpembelian should not be more than 20 character." : GoTo selesai
        End If

        'aetgljatuhtempo(18) As Date
        If Len(dataUtama(18)) = 0 Then
            result(2) = "aetgljatuhtempo can't be empty" : GoTo selesai
        End If

        'aetglnoref(22) As Date
        If Len(dataUtama(22)) = 0 Then
            result(2) = "aetglnoref can't be empty" : GoTo selesai
        End If

        'aetglpenutupan(23) As Date
        If Len(dataUtama(23)) = 0 Then
            result(2) = "aetglpenutupan can't be empty" : GoTo selesai
        End If

        'aematauang(24) As String
        If Len(dataUtama(24)) = 0 Then
            result(2) = "aematauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(24)) > 25 Then
            result(2) = "aematauang should not be more than 25 character." : GoTo selesai
        End If

        'aekurs(25) As Double
        If Len(dataUtama(25)) = 0 Then
            result(2) = "aekurs can't be empty" : GoTo selesai
        End If

        'aetotal(27) As Double
        If Len(dataUtama(27)) = 0 Then
            result(2) = "aetotal can't be empty" : GoTo selesai
        End If

        'aediskonpersen(28) As String
        If Len(dataUtama(28)) = 0 Then
            result(2) = "aediskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(28)) > 25 Then
            result(2) = "aediskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'aejmldiskon(29) As Double
        If Len(dataUtama(29)) = 0 Then
            result(2) = "aejmldiskon can't be empty" : GoTo selesai
        End If

        'aetotalpajak1detail(30) As Double
        If Len(dataUtama(30)) = 0 Then
            result(2) = "aetotalpajak1detail can't be empty" : GoTo selesai
        End If

        'aetotalpajak2detail(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "aetotalpajak2detail can't be empty" : GoTo selesai
        End If

        'aebiayalainpersen(32) As String
        If Len(dataUtama(32)) = 0 Then
            result(2) = "aebiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(32)) > 25 Then
            result(2) = "aebiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'aebiayalain(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "aebiayalain can't be empty" : GoTo selesai
        End If

        'aetotaltransaksi(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "aetotaltransaksi can't be empty" : GoTo selesai
        End If

        'aejmlbayar(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "aejmlbayar can't be empty" : GoTo selesai
        End If

        'aeidar(41) As 
        If Len(dataUtama(41)) = 0 Then
            result(2) = "aeidar can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(41)) > 20 Then
            result(2) = "aeidar should not be more than 20 character." : GoTo selesai
        End If

        'aeidaq(42) As 
        If Len(dataUtama(42)) = 0 Then
            result(2) = "aeidaq can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(42)) > 20 Then
            result(2) = "aeidaq should not be more than 20 character." : GoTo selesai
        End If

        'aeidab(43) As 
        If Len(dataUtama(43)) = 0 Then
            result(2) = "aeidab can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(43)) > 20 Then
            result(2) = "aeidab should not be more than 20 character." : GoTo selesai
        End If

        'aeidao(44) As 
        If Len(dataUtama(44)) = 0 Then
            result(2) = "aeidao can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(44)) > 20 Then
            result(2) = "aeidao should not be more than 20 character." : GoTo selesai
        End If

        'aeinputuser(49) As 
        If Len(dataUtama(49)) = 0 Then
            result(2) = "aeinputuser can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(49)) > 20 Then
            result(2) = "aeinputuser should not be more than 20 character." : GoTo selesai
        End If

        'aeinputtgl(50) As DateTime
        If Len(dataUtama(50)) = 0 Then
            result(2) = "aeinputtgl can't be empty" : GoTo selesai
        End If

        'aemodifikasiuser(51) As 
        If Len(dataUtama(51)) = 0 Then
            result(2) = "aemodifikasiuser can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(51)) > 20 Then
            result(2) = "aemodifikasiuser should not be more than 20 character." : GoTo selesai
        End If

        'aemodifikasitgl(52) As DateTime
        If Len(dataUtama(52)) = 0 Then
            result(2) = "aemodifikasitgl can't be empty" : GoTo selesai
        End If

        'aepostingtgl(54) As DateTime
        If Len(dataUtama(54)) = 0 Then
            result(2) = "aepostingtgl can't be empty" : GoTo selesai
        End If

        'aecustomdbl1(65) As Double
        If Len(dataUtama(65)) = 0 Then
            result(2) = "aecustomdbl1 can't be empty" : GoTo selesai
        End If

        'aecustomdbl2(66) As Double
        If Len(dataUtama(66)) = 0 Then
            result(2) = "aecustomdbl2 can't be empty" : GoTo selesai
        End If

        'aecustomdbl3(67) As Double
        If Len(dataUtama(67)) = 0 Then
            result(2) = "aecustomdbl3 can't be empty" : GoTo selesai
        End If

        'aecustomdate1(68) As Date
        If Len(dataUtama(68)) = 0 Then
            result(2) = "aecustomdate1 can't be empty" : GoTo selesai
        End If

        'aecustomdate2(69) As Date
        If Len(dataUtama(69)) = 0 Then
            result(2) = "aecustomdate2 can't be empty" : GoTo selesai
        End If

        'aecustomdate3(70) As Date
        If Len(dataUtama(70)) = 0 Then
            result(2) = "aecustomdate3 can't be empty" : GoTo selesai
        End If

        'aecarabayar(71) As Date
        If (Len(dataUtama(71)) = False) Then
            result(2) = "aecarabayar can't be empty" : GoTo selesai
        End If
        'aestatuslunas(72) As Date
        If (Len(dataUtama(72)) = False) Then
            result(2) = "aestatuslunas can't be empty" : GoTo selesai
        End If
        'aetgllunas(73) As Date
        If (Len(dataUtama(73)) = False) Then
            result(2) = "aetgllunas can't be empty" : GoTo selesai
        End If
        'aesdhbayarpajak(75) As Date
        If (Len(dataUtama(75)) = False) Then
            result(2) = "aesdhbayarpajak can't be empty" : GoTo selesai
        End If
        'aetglbayarpajak(76) As Date
        If (Len(dataUtama(76)) = False) Then
            result(2) = "aetglbayarpajak can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "aeid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aecabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aelokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aesumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aeautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aenotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aetgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aekodepa", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aesupplier", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aesupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ae1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ae1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ae1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ae2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ae2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ae2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aebagianpembelian", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aetermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aetgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aeuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aecatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aenoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aetglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aetglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aematauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aekurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aehargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aetotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aediskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aejmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aetotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aetotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aebiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aebiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aetotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aejmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aerekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aerekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aerekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aerekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aerekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aeidar", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aeidaq", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aeidab", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aeidao", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aestatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aestatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aejmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aecetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aeinputuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aeinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aemodifikasiuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aemodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aeposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aepostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aetutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aeisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aecustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aecustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aecustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aecustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aecustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aecustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aecustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aecustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aecustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aecustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aecustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aecustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aecustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aecustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aecarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aestatuslunas", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aetgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aenofakturpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aesdhbayarpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aetglbayarpajak", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "aeid~aecabang~aelokasi~aesumber~aeautonotransaksi~aenotransaksi~aetgl~aekodepa~aesupplier~aesupplierkontak~ae1alamat1~ae1alamat2~ae1alamat3~ae2alamat1~ae2alamat2~ae2alamat3~aebagianpembelian~aetermin~aetgljatuhtempo~aeuraian~aecatatan~aenoref~aetglnoref~aetglpenutupan~aematauang~aekurs~aehargatermasukpajak~aetotal~aediskonpersen~aejmldiskon~aetotalpajak1detail~aetotalpajak2detail~aebiayalainpersen~aebiayalain~aetotaltransaksi~aejmlbayar~aerekdiskon~aerekpajak1~aerekpajak2~aerekbiayalain~aerekbayar~aeidar~aeidaq~aeidab~aeidao~aestatus~aestatussebelumnya~aejmlrevisi~aecetakanke~aeinputuser~aeinputtgl~aemodifikasiuser~aemodifikasitgl~aeposting~aepostingtgl~aetutupperiode~aeisclose~aecustomtext1~aecustomtext2~aecustomtext3~aecustomtext4~aecustomtext5~aecustomint1~aecustomint2~aecustomint3~aecustomdbl1~aecustomdbl2~aecustomdbl3~aecustomdate1~aecustomdate2~aecustomdate3~aecarabayar~aestatuslunas~aetgllunas~aenofakturpajak~aesdhbayarpajak~aetglbayarpajak", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idaedetail(0) As , idae(1) As , idasset(2) As , namaasset(3) As String, jml(4) As Double, 
        'matauang(5) As String, kurs(6) As Double, harga(7) As Double, diskon(8) As String, jmldiskon(9) As Double, 
        'pajak1(10) As String, jmlpajak1(11) As Double, pajak2(12) As String, jmlpajak2(13) As Double, cabang(14) As String, 
        'lokasi(15) As String, rekasset(16) As String, rekdiskonpembelian(17) As String, rekhutangpembelian(18) As String, costcenter(19) As String, 
        'divisi(20) As String, subdivisi(21) As String, proyek(22) As String, catatan(23) As String, urutan(24) As Integer, 
        'idardetail(25) As , idaqdetail(26) As , idabdetail(27) As , idaodetail(28) As, isclose(39) As Integer, customtext1(30) As String, 
        'customtext2(31) As String, customtext3(32) As String, customdbl1(33) As Double, customdbl2(34) As Double, customdbl3(35) As Double, 
        'customdate1(36) As Date, customdate2(37) As Date, customdate3(38) As Date, satuan(39) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idaedetail, idae, idasset, namaasset, jml, matauang, kurs, 
        'harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, 
        'cabang, lokasi, rekasset, rekdiskonpembelian, rekhutangpembelian, costcenter, divisi, 
        'subdivisi, proyek, catatan, urutan, idardetail, idaqdetail, idabdetail, 
        'idaodetail, isclose, customtext1, 
        'customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3, satuan

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idaedetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idae", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idasset", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "namaasset", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "diskon", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmldiskon", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "pajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlpajak1", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "pajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlpajak2", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekasset", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekdiskonpembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekhutangpembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idardetail", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idaqdetail", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idabdetail", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idaodetail", AsEnumTypeData.AsDouble)
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
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)

        'Variabel ValidasiSimpan
        Dim ftExistOutstandingAO As String = "", ftOutstandingAO As String = "", updNilaiAO As String = "", updFilterAO As String = ""
        Dim namabarang As String = "", idaodetail As Integer = 0, jml As Double = 0
        Dim lokasi As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 40) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'jml(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'kurs(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'harga(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'jmldiskon(9) As Double
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - jmldiskon required numeric." : GoTo selesai
            End If
            'jmlpajak1(11) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak1 required numeric." : GoTo selesai
            End If
            'jmlpajak2(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak2 required numeric." : GoTo selesai
            End If
            'urutan(24) As Integer
            If (IsNumeric(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'isclose(29) As Integer
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(33) As Double
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(34) As Double
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(35) As Double
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(36) As Date
            If (IsDate(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(37) As Date
            If (IsDate(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(38) As Date
            If (IsDate(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'idaedetail(0) As 
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idaedetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idaedetail should not be more than 20 character." : GoTo selesai
            End If

            'idae(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idae can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idae should not be more than 20 character." : GoTo selesai
            End If

            'idasset(2) As 
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - idasset can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 20 Then
                result(2) = "Row : " & i & " - idasset should not be more than 20 character." : GoTo selesai
            End If

            'namaasset(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - namaasset can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 100 Then
                result(2) = "Row : " & i & " - namaasset should not be more than 100 character." : GoTo selesai
            End If

            'jml(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - jml can't be empty" : GoTo selesai
            End If

            'matauang(5) As String
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(5)) > 25 Then
                result(2) = "Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'harga(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'diskon(8) As String
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - diskon can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(8)) > 25 Then
                result(2) = "Row : " & i & " - diskon should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskon(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskon can't be empty" : GoTo selesai
            End If

            'jmlpajak1(11) As Double
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak1 can't be empty" : GoTo selesai
            End If

            'jmlpajak2(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak2 can't be empty" : GoTo selesai
            End If

            'idardetail(25) As 
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - idardetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(25)) > 20 Then
                result(2) = "Row : " & i & " - idardetail should not be more than 20 character." : GoTo selesai
            End If

            'idaqdetail(26) As 
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - idaqdetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(26)) > 20 Then
                result(2) = "Row : " & i & " - idaqdetail should not be more than 20 character." : GoTo selesai
            End If

            'idabdetail(27) As 
            If Len(dataRowDetail(27)) = 0 Then
                result(2) = "Row : " & i & " - idabdetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(27)) > 20 Then
                result(2) = "Row : " & i & " - idabdetail should not be more than 20 character." : GoTo selesai
            End If

            'idaodetail(28) As 
            If Len(dataRowDetail(28)) = 0 Then
                result(2) = "Row : " & i & " - idaodetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(28)) > 20 Then
                result(2) = "Row : " & i & " - idaodetail should not be more than 20 character." : GoTo selesai
            End If

            'customdbl1(33) As Double
            If Len(dataRowDetail(33)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(34) As Double
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(35) As Double
            If Len(dataRowDetail(35)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(36) As Date
            If Len(dataRowDetail(36)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(37) As Date
            If Len(dataRowDetail(37)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(38) As Date
            If Len(dataRowDetail(38)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'satuan(39) As Date
            If Len(dataRowDetail(39)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idaedetail~idae~idasset~namaasset~jml~matauang~kurs~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~rekasset~rekdiskonpembelian~rekhutangpembelian~costcenter~divisi~subdivisi~proyek~catatan~urutan~idardetail~idaqdetail~idabdetail~idaodetail~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~satuan", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If


            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'namaasset(3) As Integer     , jml(4) As Double       , idaodetail(36) As Integer
            namabarang = dataRowDetail(3) : jml = dataRowDetail(4) : lokasi = dataRowDetail(15) : idaodetail = dataRowDetail(28)

            'VALIDASI OUTSTANDING -------------------------
            If idaodetail <> 0 Then 'PO
                '1. CEK DATA EXIST ------------------------
                ftExistOutstandingAO = IIf(Len(ftExistOutstandingAO.ToString) = 0, "", ftExistOutstandingAO & " UNION ")
                ftExistOutstandingAO = String.Concat(ftExistOutstandingAO, "SELECT EXISTS(SELECT 1 FROM m7_ao_detail JOIN m7_ao ON idao = aoid WHERE idaodetail = '" & idaodetail & "' AND (aostatus = 2 OR aostatus = 3 OR aostatus = 4 OR aostatus = 7) LIMIT 1) as rowExists, '" & idaodetail & "' as idaodetail")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jml", "idaodetail=" & idaodetail)
                ftOutstandingAO = IIf(Len(ftOutstandingAO.ToString) = 0, "", ftOutstandingAO & " OR ")
                ftOutstandingAO = String.Concat(ftOutstandingAO, " (aod.idaodetail = " & idaodetail & " AND " & Outstanding & " > (aod.jml - pod.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilaiAO = String.Concat("WHEN '" & idaodetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiAO)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilterAO = IIf(Len(updFilterAO.ToString) = 0, "", updFilterAO & " OR ")
                updFilterAO = String.Concat(updFilterAO, "(idaodetail = '" & idaodetail & "')")
            End If

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'aid(0) As Integer, akode(1) As String, anama(2) As String, akategori(3) As String, acabang(4) As String, 
        'alokasi(5) As String, adivisi(6) As String, asubdivisi(7) As String, acatatan(8) As String, anomor(9) As String, 
        'atglbeli(10) As Date, atglpakai(11) As Date, amatauang(12) As String, akurs(13) As Double, ahargabeli(14) As Double, 
        'anilairesidu(15) As Double, aumurekonomis(16) As Double, abebanperbln(17) As Double, aakumulasibeban(18) As Double, anilaibuku(19) As Double, 
        'ametode(20) As Integer, atabelpenyusutan(21) As String, aintangible(22) As Integer, afiskal(23) As Integer, aatastengahbulan(24) As Integer, 
        'arekasset(25) As String, arekakumdepresiasi(26) As String, arekdepresiasi(27) As String, arekpenghapusan(28) As String, aprodusen(29) As Integer, 
        'atglpensiun(30) As Date, apenyusutanke(31) As Double, anilaimenurun(32) As Double, adispose(33) As Integer, apembelian(34) As Integer, 
        'apenjualan(35) As Integer, alocked(36) As Integer, astatus(37) As Integer, astatussebelumnya(38) As Integer, aisclose(39) As Integer, 
        'ainputuser(40) As Integer, ainputtgl(41) As DateTime, amodifikasiuser(42) As Integer, amodifikasitgl(43) As DateTime, acustomtext1(44) As String, 
        'acustomtext2(45) As String, acustomtext3(46) As String, acustomtext4(47) As String, acustomtext5(48) As String, acustomint1(49) As Integer, 
        'acustomint2(50) As Integer, acustomint3(51) As Integer, acustomdbl1(52) As Double, acustomdbl2(53) As Double, acustomdbl3(54) As Double, 
        'acustomdate1(55) As Date, acustomdate2(56) As Date, acustomdate3(57) As Date, asatuan(58) As String, aharga(59) As String, adiskon(60) As String,
        'ajmldiskon(61) As String, apajak1(62) As String, ajmlpajak1(63) As String, apajak2(64) As String, ajmlpajak2(65) As String,

        'MAPPING BUAT FLEX --------------------------------------------------------
        'aid, akode, anama, akategori, acabang, alokasi, adivisi, 
        'asubdivisi, acatatan, anomor, atglbeli, atglpakai, amatauang, akurs, 
        'ahargabeli, anilairesidu, aumurekonomis, abebanperbln, aakumulasibeban, anilaibuku, ametode, 
        'atabelpenyusutan, aintangible, afiskal, aatastengahbulan, arekasset, arekakumdepresiasi, arekdepresiasi, 
        'arekpenghapusan, aprodusen, atglpensiun, apenyusutanke, anilaimenurun, adispose, apembelian, 
        'apenjualan, alocked, astatus, astatussebelumnya, aisclose, ainputuser, ainputtgl, 
        'amodifikasiuser, amodifikasitgl, acustomtext1, acustomtext2, acustomtext3, acustomtext4, acustomtext5, 
        'acustomint1, acustomint2, acustomint3, acustomdbl1, acustomdbl2, acustomdbl3, acustomdate1, 
        'acustomdate2, acustomdate3, asatuan, aharga, adiskon, ajmldiskon, apajak1, ajmlpajak1, apajak2, ajmlpajak2

        'VALIDASI DAN SET DATA MASTER ======================================================
        'SPLIT PARAMETER DATA MASTER
        dataMaster = dataSplit(2).Split(sptRow)
        'END OF VALIDASI DAN SET DATA MASTER ===============================================


        'Buat datatable MASTER
        Dim dtmaster As New DataTable
        AsDataTableTambahField(dtmaster, "aid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "akode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "anama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "akategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "alokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "adivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "asubdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "anomor", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "atglbeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "atglpakai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "amatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "akurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "ahargabeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "anilairesidu", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "aumurekonomis", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "abebanperbln", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "aakumulasibeban", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "anilaibuku", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "ametode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "atabelpenyusutan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "aintangible", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "afiskal", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "aatastengahbulan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "arekasset", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "arekakumdepresiasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "arekdepresiasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "arekpenghapusan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "aprodusen", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtmaster, "atglpensiun", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "apenyusutanke", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "anilaimenurun", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "adispose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "apembelian", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "apenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "alocked", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "astatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "astatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "aisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "ainputuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtmaster, "ainputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "amodifikasiuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtmaster, "amodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "acustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "acustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "acustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "asatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "aharga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtmaster, "adiskon", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtmaster, "ajmldiskon", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtmaster, "apajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "ajmlpajak1", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtmaster, "apajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "ajmlpajak2", AsEnumTypeData.AsDouble)

        'VALIDASI DAN SET DATA ROW MASTER ==================================================
        Dim JmlDtMaster As Integer = dataMaster.Length
        For i = 1 To JmlDtMaster
            'SPLIT DATA MASTER
            dataRowMaster = dataMaster(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA MASTER -----------------------------------
            'CEK ARRAY DATA MASTER
            If (dataRowMaster.Length <> 66) Then
                result(2) = "Row : " & i & " - Invalid master transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW MASTER ----------------------------

            'VALIDASI TIPE DATA ==========================================================
            'aid(0) As Integer
            If (IsNumeric(dataRowMaster(0)) = False) Then
                result(2) = "aid required numeric." : GoTo selesai
            End If
            'atglbeli(10) As Date
            If (IsDate(dataRowMaster(10)) = False) Then
                result(2) = "atglbeli required date." : GoTo selesai
            End If
            'atglpakai(11) As Date
            If (IsDate(dataRowMaster(11)) = False) Then
                result(2) = "atglpakai required date." : GoTo selesai
            End If
            'akurs(13) As Double
            If (IsNumeric(dataRowMaster(13)) = False) Then
                result(2) = "akurs required numeric." : GoTo selesai
            End If
            'ahargabeli(14) As Double
            If (IsNumeric(dataRowMaster(14)) = False) Then
                result(2) = "ahargabeli required numeric." : GoTo selesai
            End If
            'anilairesidu(15) As Double
            If (IsNumeric(dataRowMaster(15)) = False) Then
                result(2) = "anilairesidu required numeric." : GoTo selesai
            End If
            'aumurekonomis(16) As Double
            If (IsNumeric(dataRowMaster(16)) = False) Then
                result(2) = "aumurekonomis required numeric." : GoTo selesai
            End If
            'abebanperbln(17) As Double
            If (IsNumeric(dataRowMaster(17)) = False) Then
                result(2) = "abebanperbln required numeric." : GoTo selesai
            End If
            'aakumulasibeban(18) As Double
            If (IsNumeric(dataRowMaster(18)) = False) Then
                result(2) = "aakumulasibeban required numeric." : GoTo selesai
            End If
            'anilaibuku(19) As Double
            If (IsNumeric(dataRowMaster(19)) = False) Then
                result(2) = "anilaibuku required numeric." : GoTo selesai
            End If
            'ametode(20) As Integer
            If (IsNumeric(dataRowMaster(20)) = False) Then
                result(2) = "ametode required numeric." : GoTo selesai
            End If
            'aintangible(22) As Integer
            If (IsNumeric(dataRowMaster(22)) = False) Then
                result(2) = "aintangible required numeric." : GoTo selesai
            End If
            'afiskal(23) As Integer
            If (IsNumeric(dataRowMaster(23)) = False) Then
                result(2) = "afiskal required numeric." : GoTo selesai
            End If
            'aatastengahbulan(24) As Integer
            If (IsNumeric(dataRowMaster(24)) = False) Then
                result(2) = "aatastengahbulan required numeric." : GoTo selesai
            End If
            'aprodusen(29) As Integer
            If (IsNumeric(dataRowMaster(29)) = False) Then
                result(2) = "aprodusen required numeric." : GoTo selesai
            End If
            'atglpensiun(30) As Date
            If (IsDate(dataRowMaster(30)) = False) Then
                result(2) = "atglpensiun required date." : GoTo selesai
            End If
            'apenyusutanke(31) As Double
            If (IsNumeric(dataRowMaster(31)) = False) Then
                result(2) = "apenyusutanke required numeric." : GoTo selesai
            End If
            'anilaimenurun(32) As Double
            If (IsNumeric(dataRowMaster(32)) = False) Then
                result(2) = "anilaimenurun required numeric." : GoTo selesai
            End If
            'adispose(33) As Integer
            If (IsNumeric(dataRowMaster(33)) = False) Then
                result(2) = "adispose required numeric." : GoTo selesai
            End If
            'apembelian(34) As Integer
            If (IsNumeric(dataRowMaster(34)) = False) Then
                result(2) = "apembelian required numeric." : GoTo selesai
            End If
            'apenjualan(35) As Integer
            If (IsNumeric(dataRowMaster(35)) = False) Then
                result(2) = "apenjualan required numeric." : GoTo selesai
            End If
            'alocked(36) As Integer
            If (IsNumeric(dataRowMaster(36)) = False) Then
                result(2) = "alocked required numeric." : GoTo selesai
            End If
            'astatus(37) As Integer
            If (IsNumeric(dataRowMaster(37)) = False) Then
                result(2) = "astatus required numeric." : GoTo selesai
            End If
            'astatussebelumnya(38) As Integer
            If (IsNumeric(dataRowMaster(38)) = False) Then
                result(2) = "astatussebelumnya required numeric." : GoTo selesai
            End If
            'aisclose(39) As Integer
            If (IsNumeric(dataRowMaster(39)) = False) Then
                result(2) = "aisclose required numeric." : GoTo selesai
            End If
            'ainputuser(40) As Integer
            If (IsNumeric(dataRowMaster(40)) = False) Then
                result(2) = "ainputuser required numeric." : GoTo selesai
            End If
            'ainputtgl(41) As DateTime
            If (IsDate(dataRowMaster(41)) = False) Then
                result(2) = "ainputtgl required date." : GoTo selesai
            End If
            'amodifikasiuser(42) As Integer
            If (IsNumeric(dataRowMaster(42)) = False) Then
                result(2) = "amodifikasiuser required numeric." : GoTo selesai
            End If
            'amodifikasitgl(43) As DateTime
            If (IsDate(dataRowMaster(43)) = False) Then
                result(2) = "amodifikasitgl required date." : GoTo selesai
            End If
            'acustomint1(49) As Integer
            If (IsNumeric(dataRowMaster(49)) = False) Then
                result(2) = "acustomint1 required numeric." : GoTo selesai
            End If
            'acustomint2(50) As Integer
            If (IsNumeric(dataRowMaster(50)) = False) Then
                result(2) = "acustomint2 required numeric." : GoTo selesai
            End If
            'acustomint3(51) As Integer
            If (IsNumeric(dataRowMaster(51)) = False) Then
                result(2) = "acustomint3 required numeric." : GoTo selesai
            End If
            'acustomdbl1(52) As Double
            If (IsNumeric(dataRowMaster(52)) = False) Then
                result(2) = "acustomdbl1 required numeric." : GoTo selesai
            End If
            'acustomdbl2(53) As Double
            If (IsNumeric(dataRowMaster(53)) = False) Then
                result(2) = "acustomdbl2 required numeric." : GoTo selesai
            End If
            'acustomdbl3(54) As Double
            If (IsNumeric(dataRowMaster(54)) = False) Then
                result(2) = "acustomdbl3 required numeric." : GoTo selesai
            End If
            'acustomdate1(55) As Date
            If (IsDate(dataRowMaster(55)) = False) Then
                result(2) = "acustomdate1 required date." : GoTo selesai
            End If
            'acustomdate2(56) As Date
            If (IsDate(dataRowMaster(56)) = False) Then
                result(2) = "acustomdate2 required date." : GoTo selesai
            End If
            'acustomdate3(57) As Date
            If (IsDate(dataRowMaster(57)) = False) Then
                result(2) = "acustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA ===================================================

            'VALIDASI DATA ===============================================================
            'akode(1) As String
            If Len(dataRowMaster(1)) = 0 Then
                result(2) = "akode can't be empty" : GoTo selesai
            End If
            If Len(dataRowMaster(1)) > 25 Then
                result(2) = "akode should not be more than 25 character." : GoTo selesai
            End If

            'anama(2) As String
            If Len(dataRowMaster(2)) = 0 Then
                result(2) = "anama can't be empty" : GoTo selesai
            End If
            If Len(dataRowMaster(2)) > 100 Then
                result(2) = "anama should not be more than 100 character." : GoTo selesai
            End If

            'akategori(3) As String
            If Len(dataRowMaster(3)) = 0 Then
                result(2) = "akategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowMaster(3)) > 25 Then
                result(2) = "akategori should not be more than 25 character." : GoTo selesai
            End If

            'atglbeli(10) As Date
            If Len(dataRowMaster(10)) = 0 Then
                result(2) = "atglbeli can't be empty" : GoTo selesai
            End If

            'atglpakai(11) As Date
            If Len(dataRowMaster(11)) = 0 Then
                result(2) = "atglpakai can't be empty" : GoTo selesai
            End If

            'amatauang(12) As String
            If Len(dataRowMaster(12)) = 0 Then
                result(2) = "amatauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowMaster(12)) > 25 Then
                result(2) = "amatauang should not be more than 25 character." : GoTo selesai
            End If

            'akurs(13) As Double
            If Len(dataRowMaster(13)) = 0 Then
                result(2) = "akurs can't be empty" : GoTo selesai
            End If

            'ahargabeli(14) As Double
            If Len(dataRowMaster(14)) = 0 Then
                result(2) = "ahargabeli can't be empty" : GoTo selesai
            End If

            'anilairesidu(15) As Double
            If Len(dataRowMaster(15)) = 0 Then
                result(2) = "anilairesidu can't be empty" : GoTo selesai
            End If

            'aumurekonomis(16) As Double
            If Len(dataRowMaster(16)) = 0 Then
                result(2) = "aumurekonomis can't be empty" : GoTo selesai
            End If

            'abebanperbln(17) As Double
            If Len(dataRowMaster(17)) = 0 Then
                result(2) = "abebanperbln can't be empty" : GoTo selesai
            End If

            'aakumulasibeban(18) As Double
            If Len(dataRowMaster(18)) = 0 Then
                result(2) = "aakumulasibeban can't be empty" : GoTo selesai
            End If

            'anilaibuku(19) As Double
            If Len(dataRowMaster(19)) = 0 Then
                result(2) = "anilaibuku can't be empty" : GoTo selesai
            End If

            'arekasset(25) As String
            If Len(dataRowMaster(25)) = 0 Then
                result(2) = "arekasset can't be empty" : GoTo selesai
            End If
            If Len(dataRowMaster(25)) > 25 Then
                result(2) = "arekasset should not be more than 25 character." : GoTo selesai
            End If

            'arekakumdepresiasi(26) As String
            If Len(dataRowMaster(26)) = 0 Then
                result(2) = "arekakumdepresiasi can't be empty" : GoTo selesai
            End If
            If Len(dataRowMaster(26)) > 25 Then
                result(2) = "arekakumdepresiasi should not be more than 25 character." : GoTo selesai
            End If

            'arekdepresiasi(27) As String
            If Len(dataRowMaster(27)) = 0 Then
                result(2) = "arekdepresiasi can't be empty" : GoTo selesai
            End If
            If Len(dataRowMaster(27)) > 25 Then
                result(2) = "arekdepresiasi should not be more than 25 character." : GoTo selesai
            End If

            'atglpensiun(30) As Date
            If Len(dataRowMaster(30)) = 0 Then
                result(2) = "atglpensiun can't be empty" : GoTo selesai
            End If

            'apenyusutanke(31) As Double
            If Len(dataRowMaster(31)) = 0 Then
                result(2) = "apenyusutanke can't be empty" : GoTo selesai
            End If

            'anilaimenurun(32) As Double
            If Len(dataRowMaster(32)) = 0 Then
                result(2) = "anilaimenurun can't be empty" : GoTo selesai
            End If

            'ainputtgl(41) As DateTime
            If Len(dataRowMaster(41)) = 0 Then
                result(2) = "ainputtgl can't be empty" : GoTo selesai
            End If

            'amodifikasitgl(43) As DateTime
            If Len(dataRowMaster(43)) = 0 Then
                result(2) = "amodifikasitgl can't be empty" : GoTo selesai
            End If

            'acustomdbl1(52) As Double
            If Len(dataRowMaster(52)) = 0 Then
                result(2) = "acustomdbl1 can't be empty" : GoTo selesai
            End If

            'acustomdbl2(53) As Double
            If Len(dataRowMaster(53)) = 0 Then
                result(2) = "acustomdbl2 can't be empty" : GoTo selesai
            End If

            'acustomdbl3(54) As Double
            If Len(dataRowMaster(54)) = 0 Then
                result(2) = "acustomdbl3 can't be empty" : GoTo selesai
            End If

            'acustomdate1(55) As Date
            If Len(dataRowMaster(55)) = 0 Then
                result(2) = "acustomdate1 can't be empty" : GoTo selesai
            End If

            'acustomdate2(56) As Date
            If Len(dataRowMaster(56)) = 0 Then
                result(2) = "acustomdate2 can't be empty" : GoTo selesai
            End If

            'acustomdate3(57) As Date
            If Len(dataRowMaster(57)) = 0 Then
                result(2) = "acustomdate3 can't be empty" : GoTo selesai
            End If

            'asatuan(58) As Date
            If Len(dataRowMaster(58)) = 0 Then
                result(2) = "asatuan can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA MASTER========================================================

            If AsDataTableTambahData(dtmaster, "aid~akode~anama~akategori~acabang~alokasi~adivisi~asubdivisi~acatatan~anomor~atglbeli~atglpakai~amatauang~akurs~ahargabeli~anilairesidu~aumurekonomis~abebanperbln~aakumulasibeban~anilaibuku~ametode~atabelpenyusutan~aintangible~afiskal~aatastengahbulan~arekasset~arekakumdepresiasi~arekdepresiasi~arekpenghapusan~aprodusen~atglpensiun~apenyusutanke~anilaimenurun~adispose~apembelian~apenjualan~alocked~astatus~astatussebelumnya~aisclose~ainputuser~ainputtgl~amodifikasiuser~amodifikasitgl~acustomtext1~acustomtext2~acustomtext3~acustomtext4~acustomtext5~acustomint1~acustomint2~acustomint3~acustomdbl1~acustomdbl2~acustomdbl3~acustomdate1~acustomdate2~acustomdate3~asatuan~aharga~adiskon~ajmldiskon~apajak1~ajmlpajak1~apajak2~ajmlpajak2", dataRowMaster(0) & "~" & dataRowMaster(1) & "~" & dataRowMaster(2) & "~" & dataRowMaster(3) & "~" & dataRowMaster(4) & "~" & dataRowMaster(5) & "~" & dataRowMaster(6) & "~" & dataRowMaster(7) & "~" & dataRowMaster(8) & "~" & dataRowMaster(9) & "~" & dataRowMaster(10) & "~" & dataRowMaster(11) & "~" & dataRowMaster(12) & "~" & dataRowMaster(13) & "~" & dataRowMaster(14) & "~" & dataRowMaster(15) & "~" & dataRowMaster(16) & "~" & dataRowMaster(17) & "~" & dataRowMaster(18) & "~" & dataRowMaster(19) & "~" & dataRowMaster(20) & "~" & dataRowMaster(21) & "~" & dataRowMaster(22) & "~" & dataRowMaster(23) & "~" & dataRowMaster(24) & "~" & dataRowMaster(25) & "~" & dataRowMaster(26) & "~" & dataRowMaster(27) & "~" & dataRowMaster(28) & "~" & dataRowMaster(29) & "~" & dataRowMaster(30) & "~" & dataRowMaster(31) & "~" & dataRowMaster(32) & "~" & dataRowMaster(33) & "~" & dataRowMaster(34) & "~" & dataRowMaster(35) & "~" & dataRowMaster(36) & "~" & dataRowMaster(37) & "~" & dataRowMaster(38) & "~" & dataRowMaster(39) & "~" & dataRowMaster(40) & "~" & dataRowMaster(41) & "~" & dataRowMaster(42) & "~" & dataRowMaster(43) & "~" & dataRowMaster(44) & "~" & dataRowMaster(45) & "~" & dataRowMaster(46) & "~" & dataRowMaster(47) & "~" & dataRowMaster(48) & "~" & dataRowMaster(49) & "~" & dataRowMaster(50) & "~" & dataRowMaster(51) & "~" & dataRowMaster(52) & "~" & dataRowMaster(53) & "~" & dataRowMaster(54) & "~" & dataRowMaster(55) & "~" & dataRowMaster(56) & "~" & dataRowMaster(57) & "~" & dataRowMaster(58) & "~" & dataRowMaster(59) & "~" & dataRowMaster(60) & "~" & dataRowMaster(61) & "~" & dataRowMaster(62) & "~" & dataRowMaster(63) & "~" & dataRowMaster(64) & "~" & dataRowMaster(65)) = False Then
                result(2) = "Insert into master datatable failed." : GoTo selesai
            End If
        Next
        'END OF VALIDASI DAN SET ROW DATA MASTER ===========================================


        'SIMPAN KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0, idae As Integer = 0

        Try
            'Proses utama
            If (dtutama.Rows.Count > 0) Then
                Dim drutama As DataRow = dtutama.Rows(0)

                'CEK PERIODE AKUNTANSI ==================================
                Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("aetgl")), AsFormatTanggal(drutama("aetgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                'If drutama("aestatus") = 2 Then
                'Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstandingAO, ftOutstandingAO)
                'If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'End If
                'END OF VALIDASI SIMPAN =================================


                'SET TGL JATUH TEMPO ====================================
                Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                rsTglJT = F_TglJT(drutama("aetermin").ToString, AsFormatTanggal(drutama("aetgl")), "aetgl").Split(sptSubParam)
                If rsTglJT(0) = 0 Then
                    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                Else
                    drutama("aetgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                End If
                'END OF SET TGL JATUH TEMPO =============================

                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TOTAL = subtotal
                drutama("aetotal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("aetotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("aetotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                drutama("aetotaltransaksi") = Double.Parse(drutama("aetotal")) - Double.Parse(drutama("aediskonpersen")) + Double.Parse(drutama("aetotalpajak1detail")) + Double.Parse(drutama("aetotalpajak2detail")) + Double.Parse(drutama("aebiayalain"))
                'END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("aeid")
                    notransaksi = drutama("aenotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(aeid) FROM M7_Ae WHERE aeid=" & result(4))
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then
                        sql = "Update M7_Ae set aecabang  = '" & FixQuotes(drutama("aecabang")) & "', aelokasi  = '" & FixQuotes(drutama("aelokasi")) & "', aesumber  = '" & FixQuotes(drutama("aesumber")) & "', aeautonotransaksi  = " & drutama("aeautonotransaksi") & ", aenotransaksi  = '" & FixQuotes(drutama("aenotransaksi")) & "', aetgl  = '" & FixQuotes(AsFormatTanggal(drutama("aetgl"))) & "', aekodepa  = '" & FixQuotes(drutama("aekodepa")) & "', aesupplier  = '" & FixQuotes(drutama("aesupplier")) & "', aesupplierkontak  = '" & FixQuotes(drutama("aesupplierkontak")) & "', ae1alamat1  = '" & FixQuotes(drutama("ae1alamat1")) & "', ae1alamat2  = '" & FixQuotes(drutama("ae1alamat2")) & "', ae1alamat3  = '" & FixQuotes(drutama("ae1alamat3")) & "', ae2alamat1  = '" & FixQuotes(drutama("ae2alamat1")) & "', ae2alamat2  = '" & FixQuotes(drutama("ae2alamat2")) & "', ae2alamat3  = '" & FixQuotes(drutama("ae2alamat3")) & "', aebagianpembelian  = '" & FixQuotes(drutama("aebagianpembelian")) & "', aetermin  = '" & FixQuotes(drutama("aetermin")) & "', aetgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("aetgljatuhtempo"))) & "', aeuraian  = '" & FixQuotes(drutama("aeuraian")) & "', aecatatan  = '" & FixQuotes(drutama("aecatatan")) & "', aenoref  = '" & FixQuotes(drutama("aenoref")) & "', aetglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("aetglnoref"))) & "', aetglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("aetglpenutupan"))) & "', aematauang  = '" & FixQuotes(drutama("aematauang")) & "', aekurs  = '" & FixDouble(drutama("aekurs")) & "', aehargatermasukpajak  = " & drutama("aehargatermasukpajak") & ", aetotal  = '" & FixDouble(drutama("aetotal")) & "', aediskonpersen  = '" & FixQuotes(drutama("aediskonpersen")) & "', aejmldiskon  = '" & FixDouble(drutama("aejmldiskon")) & "', aetotalpajak1detail  = '" & FixDouble(drutama("aetotalpajak1detail")) & "', aetotalpajak2detail  = '" & FixDouble(drutama("aetotalpajak2detail")) & "', aebiayalainpersen  = '" & FixQuotes(drutama("aebiayalainpersen")) & "', aebiayalain  = '" & FixDouble(drutama("aebiayalain")) & "', aetotaltransaksi  = '" & FixDouble(drutama("aetotaltransaksi")) & "', aejmlbayar  = '" & FixDouble(drutama("aejmlbayar")) & "', aerekdiskon  = '" & FixQuotes(drutama("aerekdiskon")) & "', aerekpajak1  = '" & FixQuotes(drutama("aerekpajak1")) & "', aerekpajak2  = '" & FixQuotes(drutama("aerekpajak2")) & "', aerekbiayalain  = '" & FixQuotes(drutama("aerekbiayalain")) & "', aerekbayar  = '" & FixQuotes(drutama("aerekbayar")) & "', aeidar  = '" & FixQuotes(drutama("aeidar")) & "', aeidaq  = '" & FixQuotes(drutama("aeidaq")) & "', aeidab  = '" & FixQuotes(drutama("aeidab")) & "', aeidao  = '" & FixQuotes(drutama("aeidao")) & "', aestatus  = " & drutama("aestatus") & ", aestatussebelumnya  = " & drutama("aestatussebelumnya") & ", aejmlrevisi  = " & drutama("aejmlrevisi") & ", aecetakanke  = " & drutama("aecetakanke") & ", aemodifikasiuser  = '" & FixQuotes(drutama("aemodifikasiuser")) & "', aemodifikasitgl  = NOW(), aeposting  = " & drutama("aeposting") & ", aepostingtgl  = '" & FixQuotes(AsFormatTanggal(drutama("aepostingtgl"), "yyyy-MM-dd H:mm:ss")) & "', aetutupperiode  = " & drutama("aetutupperiode") & ", aecustomtext1  = '" & FixQuotes(drutama("aecustomtext1")) & "', aecustomtext2  = '" & FixQuotes(drutama("aecustomtext2")) & "', aecustomtext3  = '" & FixQuotes(drutama("aecustomtext3")) & "', aecustomtext4  = '" & FixQuotes(drutama("aecustomtext4")) & "', aecustomtext5  = '" & FixQuotes(drutama("aecustomtext5")) & "', aecustomint1  = " & drutama("aecustomint1") & ", aecustomint2  = " & drutama("aecustomint2") & ", aecustomint3  = " & drutama("aecustomint3") & ", aecustomdbl1  = '" & FixDouble(drutama("aecustomdbl1")) & "', aecustomdbl2  = '" & FixDouble(drutama("aecustomdbl2")) & "', aecustomdbl3  = '" & FixDouble(drutama("aecustomdbl3")) & "', aecustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("aecustomdate1"))) & "', aecustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("aecustomdate2"))) & "', aecustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("aecustomdate3"))) & "', aecarabayar  = '" & FixQuotes(drutama("aecarabayar")) & "', aestatuslunas  = '" & FixQuotes(drutama("aestatuslunas")) & "', aetgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("aetgllunas"))) & "', aenofakturpajak  = '" & FixQuotes(drutama("aenofakturpajak")) & "', aesdhbayarpajak  = '" & FixQuotes(drutama("aesdhbayarpajak")) & "', aetglbayarpajak  = '" & FixQuotes(AsFormatTanggal(drutama("aetglbayarpajak"))) & "' where aeid = " & drutama("aeid") & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Else
                        result(2) = "Transaction data not found." : GoTo selesai
                    End If
                Else

                    If drutama("aeautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("aecabang"), drutama("aelokasi"), drutama("aesumber"), drutama("aetgl"))
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
                        notransaksi = drutama("aenotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(aeid) FROM m7_ae WHERE aenotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M7_Ae (aecabang, aelokasi, aesumber, aeautonotransaksi, aenotransaksi, aetgl, aekodepa, aesupplier, aesupplierkontak, ae1alamat1, ae1alamat2, ae1alamat3, ae2alamat1, ae2alamat2, ae2alamat3, aebagianpembelian, aetermin, aetgljatuhtempo, aeuraian, aecatatan, aenoref, aetglnoref, aetglpenutupan, aematauang, aekurs, aehargatermasukpajak, aetotal, aediskonpersen, aejmldiskon, aetotalpajak1detail, aetotalpajak2detail, aebiayalainpersen, aebiayalain, aetotaltransaksi, aejmlbayar, aerekdiskon, aerekpajak1, aerekpajak2, aerekbiayalain, aerekbayar, aeidar, aeidaq, aeidab, aeidao, aestatus, aestatussebelumnya, aejmlrevisi, aecetakanke, aeinputuser, aeinputtgl, aemodifikasiuser, aemodifikasitgl, aeposting, aepostingtgl, aetutupperiode, aeisclose, aecustomtext1, aecustomtext2, aecustomtext3, aecustomtext4, aecustomtext5, aecustomint1, aecustomint2, aecustomint3, aecustomdbl1, aecustomdbl2, aecustomdbl3, aecustomdate1, aecustomdate2, aecustomdate3, aecarabayar, aestatuslunas, aetgllunas, aenofakturpajak, aesdhbayarpajak, aetglbayarpajak) values('" & FixQuotes(drutama("aecabang")) & "', '" & FixQuotes(drutama("aelokasi")) & "', '" & FixQuotes(drutama("aesumber")) & "', " & drutama("aeautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("aetgl"))) & "', '" & FixQuotes(drutama("aekodepa")) & "', '" & FixQuotes(drutama("aesupplier")) & "', '" & FixQuotes(drutama("aesupplierkontak")) & "', '" & FixQuotes(drutama("ae1alamat1")) & "', '" & FixQuotes(drutama("ae1alamat2")) & "', '" & FixQuotes(drutama("ae1alamat3")) & "', '" & FixQuotes(drutama("ae2alamat1")) & "', '" & FixQuotes(drutama("ae2alamat2")) & "', '" & FixQuotes(drutama("ae2alamat3")) & "', '" & FixQuotes(drutama("aebagianpembelian")) & "', '" & FixQuotes(drutama("aetermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("aetgljatuhtempo"))) & "', '" & FixQuotes(drutama("aeuraian")) & "', '" & FixQuotes(drutama("aecatatan")) & "', '" & FixQuotes(drutama("aenoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("aetglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("aetglpenutupan"))) & "', '" & FixQuotes(drutama("aematauang")) & "', '" & FixDouble(drutama("aekurs")) & "', " & drutama("aehargatermasukpajak") & ", '" & FixDouble(drutama("aetotal")) & "', '" & FixQuotes(drutama("aediskonpersen")) & "', '" & FixDouble(drutama("aejmldiskon")) & "', '" & FixDouble(drutama("aetotalpajak1detail")) & "', '" & FixDouble(drutama("aetotalpajak2detail")) & "', '" & FixQuotes(drutama("aebiayalainpersen")) & "', '" & FixDouble(drutama("aebiayalain")) & "', '" & FixDouble(drutama("aetotaltransaksi")) & "', '" & FixDouble(drutama("aejmlbayar")) & "', '" & FixQuotes(drutama("aerekdiskon")) & "', '" & FixQuotes(drutama("aerekpajak1")) & "', '" & FixQuotes(drutama("aerekpajak2")) & "', '" & FixQuotes(drutama("aerekbiayalain")) & "', '" & FixQuotes(drutama("aerekbayar")) & "', '" & FixQuotes(drutama("aeidar")) & "', '" & FixQuotes(drutama("aeidaq")) & "', '" & FixQuotes(drutama("aeidab")) & "', '" & FixQuotes(drutama("aeidao")) & "', " & drutama("aestatus") & ", " & drutama("aestatussebelumnya") & ", " & drutama("aejmlrevisi") & ", " & drutama("aecetakanke") & ", '" & FixQuotes(drutama("aeinputuser")) & "', NOW(), '" & FixQuotes(drutama("aemodifikasiuser")) & "', '1971-01-01', " & drutama("aeposting") & ", '" & FixQuotes(AsFormatTanggal(drutama("aepostingtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("aetutupperiode") & ", " & drutama("aeisclose") & ", '" & FixQuotes(drutama("aecustomtext1")) & "', '" & FixQuotes(drutama("aecustomtext2")) & "', '" & FixQuotes(drutama("aecustomtext3")) & "', '" & FixQuotes(drutama("aecustomtext4")) & "', '" & FixQuotes(drutama("aecustomtext5")) & "', " & drutama("aecustomint1") & ", " & drutama("aecustomint2") & ", " & drutama("aecustomint3") & ", '" & FixDouble(drutama("aecustomdbl1")) & "', '" & FixDouble(drutama("aecustomdbl2")) & "', '" & FixDouble(drutama("aecustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("aecustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("aecustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("aecustomdate3"))) & "', '" & FixQuotes(drutama("aecarabayar")) & "', '" & FixQuotes(drutama("aestatuslunas")) & "', '" & FixQuotes(AsFormatTanggal(drutama("aetgllunas"))) & "', '" & FixQuotes(drutama("aenofakturpajak")) & "', '" & FixQuotes(drutama("aesdhbayarpajak")) & "', '" & FixQuotes(AsFormatTanggal(drutama("aetglbayarpajak"))) & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                Dim dt2 As New DataTable
                'Sql disesuaikan sendiri, untuk parameternya disesuaikan sendiri.
                dt2 = AsDataTableAmbilDariDB("select aeid from M7_ae where aenotransaksi='" & notransaksi & "' AND aeinputuser= '" & userid & "' order by aemodifikasitgl desc limit 1")
                If dt2.Rows.Count > 0 Then idae = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                'idae = dt2.Rows(0)(0)

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M7_Ae_Detail where idae = " & idae
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
                    Dim idasset As String
                    Dim strValue2 As New StringBuilder
                    Dim dr1 As DataRow
                    For i = 0 To dataMaster.Length - 1
                        dr1 = dtdetail.Rows(i)
                        dataRowMaster = dataMaster(i).Split(sptField)
                        If (drutama("aestatus") = 2) Then
                            sql = "Insert into M7_Asset (akode, anama, akategori, acabang, alokasi, adivisi, asubdivisi, acatatan, anomor, atglbeli, atglpakai, amatauang, akurs, ahargabeli, anilairesidu, aumurekonomis, abebanperbln, aakumulasibeban, anilaibuku, ametode, atabelpenyusutan, aintangible, afiskal, aatastengahbulan, arekasset, arekakumdepresiasi, arekdepresiasi, arekpenghapusan, aprodusen, atglpensiun, apenyusutanke, anilaimenurun, adispose, apembelian, apenjualan, alocked, astatus, astatussebelumnya, aisclose, ainputuser, ainputtgl, amodifikasiuser, amodifikasitgl, acustomtext1, acustomtext2, acustomtext3, acustomtext4, acustomtext5, acustomint1, acustomint2, acustomint3, acustomdbl1, acustomdbl2, acustomdbl3, acustomdate1, acustomdate2, acustomdate3, asatuan, aharga, adiskon, ajmldiskon, apajak1, ajmlpajak1, apajak2, ajmlpajak2) values('" & FixQuotes(dataRowMaster(1)) & "', '" & FixQuotes(dataRowMaster(2)) & "', '" & FixQuotes(dataRowMaster(3)) & "', '" & FixQuotes(dataRowMaster(4)) & "', '" & FixQuotes(dataRowMaster(5)) & "', '" & FixQuotes(dataRowMaster(6)) & "', '" & FixQuotes(dataRowMaster(7)) & "', '" & FixQuotes(dataRowMaster(8)) & "', '" & FixQuotes(dataRowMaster(9)) & "', '" & FixQuotes(AsFormatTanggal(dataRowMaster(10))) & "', '" & FixQuotes(AsFormatTanggal(dataRowMaster(11))) & "', '" & FixQuotes(dataRowMaster(12)) & "', '" & FixDouble(dataRowMaster(13)) & "', '" & FixDouble(dataRowMaster(14)) & "', '" & FixDouble(dataRowMaster(15)) & "', '" & FixDouble(dataRowMaster(16)) & "', '" & FixDouble(dataRowMaster(17)) & "', '" & FixDouble(dataRowMaster(18)) & "', '" & FixDouble(dataRowMaster(19)) & "', " & dataRowMaster(20) & ", '" & FixQuotes(dataRowMaster(21)) & "', " & dataRowMaster(22) & ", " & dataRowMaster(23) & ", " & dataRowMaster(24) & ", '" & FixQuotes(dataRowMaster(25)) & "', '" & FixQuotes(dataRowMaster(26)) & "', '" & FixQuotes(dataRowMaster(27)) & "', '" & FixQuotes(dataRowMaster(28)) & "', " & dataRowMaster(29) & ", '" & FixQuotes(AsFormatTanggal(dataRowMaster(30))) & "', '" & FixDouble(dataRowMaster(31)) & "', '" & FixDouble(dataRowMaster(32)) & "', " & dataRowMaster(33) & ", " & dataRowMaster(34) & ", " & dataRowMaster(35) & ", " & dataRowMaster(36) & ", " & dataRowMaster(37) & ", " & dataRowMaster(38) & ", " & dataRowMaster(39) & ", " & dataRowMaster(40) & ", NOW(), " & dataRowMaster(42) & ", '1971-01-01 00:00:00', '" & FixQuotes(dataRowMaster(44)) & "', '" & FixQuotes(dataRowMaster(45)) & "', '" & FixQuotes(dataRowMaster(46)) & "', '" & FixQuotes(dataRowMaster(47)) & "', '" & FixQuotes(dataRowMaster(48)) & "', " & dataRowMaster(49) & ", " & dataRowMaster(50) & ", " & dataRowMaster(51) & ", '" & FixDouble(dataRowMaster(52)) & "', '" & FixDouble(dataRowMaster(53)) & "', '" & FixDouble(dataRowMaster(54)) & "', '" & FixQuotes(AsFormatTanggal(dataRowMaster(55))) & "', '" & FixQuotes(AsFormatTanggal(dataRowMaster(56))) & "', '" & FixQuotes(AsFormatTanggal(dataRowMaster(57))) & "', '" & FixQuotes(dataRowMaster(58)) & "', '" & FixQuotes(dataRowMaster(59)) & "', '" & FixQuotes(dataRowMaster(60)) & "', '" & FixQuotes(dataRowMaster(61)) & "', '" & FixQuotes(dataRowMaster(62)) & "', '" & FixQuotes(dataRowMaster(63)) & "', '" & FixQuotes(dataRowMaster(64)) & "', '" & FixQuotes(dataRowMaster(65)) & "')"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                            Dim dt3 As New DataTable
                            'Sql disesuaikan sendiri, untuk parameternya disesuaikan sendiri.
                            dt3 = AsDataTableAmbilDariDB("select aid from M7_Asset where akode = '" & dataRowMaster(1) & "' order by aid desc limit 1")
                            If (dt3.Rows.Count = 0) Then
                                result(2) = "id asset not found" : Trans.Rollback() : GoTo selesai
                            Else
                                idasset = dt3(0)(0)
                            End If
                        Else
                            idasset = 0
                        End If

                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & idae & ", '" & idasset & "', '" & FixQuotes(dr1("namaasset")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("rekasset")) & "', '" & FixQuotes(dr1("rekdiskonpembelian")) & "', '" & FixQuotes(dr1("rekhutangpembelian")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", '" & FixQuotes(dr1("idardetail")) & "', '" & FixQuotes(dr1("idaqdetail")) & "', '" & FixQuotes(dr1("idabdetail")) & "', '" & FixQuotes(dr1("idaodetail")) & "', " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(dr1("satuan")) & "')")
                    Next
                    sql = "Insert into M7_Ae_Detail(idae, idasset, namaasset, jml, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, rekasset, rekdiskonpembelian, rekhutangpembelian, costcenter, divisi, subdivisi, proyek, catatan, urutan, idardetail, idaqdetail, idabdetail, idaodetail, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, satuan) values" & strValue2.ToString & ""
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


                'UPDATE OUTSTANDING TRANSAKSI ==========================================================
                If drutama("aestatus") = 2 Then
                    If Len(updNilaiAO) > 0 Then 'PO
                        'UPDATE DETAIL
                        sql = "UPDATE m7_ao_detail SET jmlrealisasi = (CASE idaodetail " & updNilaiAO & " ELSE jmlrealisasi END) WHERE " & updFilterAO
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idao FROM M7_ao_detail WHERE " & updFilterAO & " GROUP BY idao")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idao = '" & dr1("idao") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDB("SELECT idao, SUM(jml) as jml, SUM(jmlrealisasi) as jmlrealisasi FROM M7_ao_detail WHERE " & ftDetail & " GROUP BY idao")
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiAO = "" : updFilterAO = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlrealisasi") >= dr1("jml") Then
                                    statusOut = 2
                                ElseIf dr1("jmlrealisasi") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilaiAO = String.Concat(updNilaiAO, "WHEN '" & dr1("idao") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterAO = IIf(Len(updFilterAO.ToString) = 0, "", updFilterAO & " OR ")
                                updFilterAO = String.Concat(updFilterAO, "(aoid = '" & dr1("idao") & "')")
                            Next

                            sql = "UPDATE m7_ao SET aostatusrealisasi = (CASE aoid " & updNilaiAO & " ELSE aostatusrealisasi END) WHERE " & updFilterAO
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


                'INSERT USER LOG ====================================================================
                'ambil moduleid dan menuid dari m0_nomor
                Dim sumber As String = "AE", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
        Con1.Close()
        Con1 = Nothing
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
    Public Function M7_AeGetdataById(ByVal param As String) As String
        'M7_AeGetdataById Utama --------------------------------------------------------
        'aeid, aecabang, aelokasi, aesumber, aeautonotransaksi, aenotransaksi, aetgl, 
        'aekodepa, aesupplier, aesupplierkontak, ae1alamat1, ae1alamat2, ae1alamat3, ae2alamat1, 
        'ae2alamat2, ae2alamat3, aebagianpembelian, aetermin, aetgljatuhtempo, aeuraian, aecatatan, 
        'aenoref, aetglnoref, aetglpenutupan, aematauang, aekurs, aehargatermasukpajak, aetotal, 
        'aediskonpersen, aejmldiskon, aetotalpajak1detail, aetotalpajak2detail, aebiayalainpersen, aebiayalain, aetotaltransaksi, 
        'aejmlbayar, aestatuslunas, aetgllunas, aenofakturpajak, aesdhbayarpajak, aetglbayarpajak, aerekdiskon, 
        'aerekpajak1, aerekpajak2, aerekbiayalain, aerekbayar, aeidar, aeidaq, aeidab, 
        'aeidao, aestatusrealisasi, aestatus, aestatussebelumnya, aejmlrevisi, aecetakanke, aeinputuser, 
        'aeinputtgl, aemodifikasiuser, aemodifikasitgl, aeposting, aepostingtgl, aetutupperiode, aeisclose, 
        'aecustomtext1, aecustomtext2, aecustomtext3, aecustomtext4, aecustomtext5, aecustomint1, aecustomint2, 
        'aecustomint3, aecustomdbl1, aecustomdbl2, aecustomdbl3, aecustomdate1, aecustomdate2, aecustomdate3, 
        'aecabangnama, aelokasinama, aesupplierkode, aesuppliernama, aebagianpembeliankode, aebagianpembeliannama, aeterminnama, 
        'aeterminharijatuhtempo, aerekdiskonnama, aerekpajak1nama, aerekpajak2nama, aerekbiayalainnama, aerekbayarnama, aenotransaksiao, 
        'aestatusnama, aestatussebelumnyanama, aeinputusernama, aemodifikasiusernama, aecarabayar

        'M7_AeGetdataById Detail -------------------------------------------------------
        'idaedetail, idae, idasset, 
        'namaasset, jml, matauang, kurs, harga, diskon, jmldiskon, 
        'pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, rekdiskonpembelian, 
        'costcenter, divisi, subdivisi, proyek, catatan, urutan, idardetail, 
        'idaqdetail, idabdetail, idaodetail, jmlrealisasi, statusrealisasi, isclose, customtext1, 
        'customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3, kodeasset, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, 
        'lokasinama, costcenternama, divisinama, subdivisinama, aonotransaksi, satuan

        'M7_AeGetdataById Asset -------------------------------------------------------
        'aid, akode, anama, akategori, acabang, alokasi, adivisi, 
        'asubdivisi, acatatan, anomor, atglbeli, atglpakai, amatauang, akurs, 
        'ahargabeli, anilairesidu, aumurekonomis, abebanperbln, aakumulasibeban, anilaibuku, ametode, 
        'atabelpenyusutan, aintangible, afiskal, aatastengahbulan, arekasset, arekakumdepresiasi, arekdepresiasi, 
        'arekpenghapusan, aprodusen, atglpensiun, apenyusutanke, anilaimenurun, adispose, apembelian, 
        'apenjualan, alocked, astatus, astatussebelumnya, aisclose, ainputuser, ainputtgl, 
        'amodifikasiuser, amodifikasitgl, akategorinama, acabangnama, alokasinama, adivisinama, asubdivisinama, 
        'ametodenama, arekassetnama, arekakumdepresiasinama, arekdepresiasinama, arekpenghapusannama, aprodusenkode, aprodusennama, 
        'astatusnama, astatussebelumnyanama, ainputusernama, amodifikasiusernama, acustomtext1, acustomtext2, acustomtext3, acustomtext4, acustomtext5, acustomint1, acustomint2, acustomint3, acustomdbl1, acustomdbl2, acustomdbl3, acustomdate1,
        'acustomdate2, acustomdate3, asatuan, aharga, adiskon, ajmldiskon, apajak1, ajmlpajak1, apajak2, ajmlpajak2


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

        Dim utama As String = "", detail As String = "", asset As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M4_Rq~M4_Rq_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "aeid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "aeid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m7_ae_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                 FxDB(drutama("aeid"), ""), sptField,
                     FxDB(drutama("aecabang"), ""), sptField,
                     FxDB(drutama("aelokasi"), ""), sptField,
                     FxDB(drutama("aesumber"), ""), sptField,
                     FxDB(drutama("aeautonotransaksi"), 0), sptField,
                     FxDB(drutama("aenotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aetgl"), ""), formatTgl), sptField,
                     FxDB(drutama("aekodepa"), ""), sptField,
                     FxDB(drutama("aesupplier"), ""), sptField,
                     FxDB(drutama("aesupplierkontak"), ""), sptField,
                     FxDB(drutama("ae1alamat1"), ""), sptField,
                     FxDB(drutama("ae1alamat2"), ""), sptField,
                     FxDB(drutama("ae1alamat3"), ""), sptField,
                     FxDB(drutama("ae2alamat1"), ""), sptField,
                     FxDB(drutama("ae2alamat2"), ""), sptField,
                     FxDB(drutama("ae2alamat3"), ""), sptField,
                     FxDB(drutama("aebagianpembelian"), ""), sptField,
                     FxDB(drutama("aetermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aetgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("aeuraian"), ""), sptField,
                     FxDB(drutama("aecatatan"), ""), sptField,
                     FxDB(drutama("aenoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aetglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("aetglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("aematauang"), ""), sptField,
                     FxDB(drutama("aekurs"), 0), sptField,
                     FxDB(drutama("aehargatermasukpajak"), 0), sptField,
                     FxDB(drutama("aetotal"), 0), sptField,
                     FxDB(drutama("aediskonpersen"), ""), sptField,
                     FxDB(drutama("aejmldiskon"), 0), sptField,
                     FxDB(drutama("aetotalpajak1detail"), 0), sptField,
                     FxDB(drutama("aetotalpajak2detail"), 0), sptField,
                     FxDB(drutama("aebiayalainpersen"), ""), sptField,
                     FxDB(drutama("aebiayalain"), 0), sptField,
                     FxDB(drutama("aetotaltransaksi"), 0), sptField,
                     FxDB(drutama("aejmlbayar"), 0), sptField,
                     FxDB(drutama("aestatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("aetgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("aenofakturpajak"), ""), sptField,
                     FxDB(drutama("aesdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("aetglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(drutama("aerekdiskon"), ""), sptField,
                     FxDB(drutama("aerekpajak1"), ""), sptField,
                     FxDB(drutama("aerekpajak2"), ""), sptField,
                     FxDB(drutama("aerekbiayalain"), ""), sptField,
                     FxDB(drutama("aerekbayar"), ""), sptField,
                     FxDB(drutama("aeidar"), ""), sptField,
                     FxDB(drutama("aeidaq"), ""), sptField,
                     FxDB(drutama("aeidab"), ""), sptField,
                     FxDB(drutama("aeidao"), ""), sptField,
                     FxDB(drutama("aestatusrealisasi"), 0), sptField,
                     FxDB(drutama("aestatus"), 0), sptField,
                     FxDB(drutama("aestatussebelumnya"), 0), sptField,
                     FxDB(drutama("aejmlrevisi"), 0), sptField,
                     FxDB(drutama("aecetakanke"), 0), sptField,
                     FxDB(drutama("aeinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aeinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("aemodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aemodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("aeposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("aepostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("aetutupperiode"), 0), sptField,
                     FxDB(drutama("aeisclose"), 0), sptField,
                     FxDB(drutama("aecustomtext1"), ""), sptField,
                     FxDB(drutama("aecustomtext2"), ""), sptField,
                     FxDB(drutama("aecustomtext3"), ""), sptField,
                     FxDB(drutama("aecustomtext4"), ""), sptField,
                     FxDB(drutama("aecustomtext5"), ""), sptField,
                     FxDB(drutama("aecustomint1"), 0), sptField,
                     FxDB(drutama("aecustomint2"), 0), sptField,
                     FxDB(drutama("aecustomint3"), 0), sptField,
                     FxDB(drutama("aecustomdbl1"), 0), sptField,
                     FxDB(drutama("aecustomdbl2"), 0), sptField,
                     FxDB(drutama("aecustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("aecustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("aecustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("aecustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("aecabangnama"), ""), sptField,
                     FxDB(drutama("aelokasinama"), ""), sptField,
                     FxDB(drutama("aesupplierkode"), ""), sptField,
                     FxDB(drutama("aesuppliernama"), ""), sptField,
                     FxDB(drutama("aebagianpembeliankode"), ""), sptField,
                     FxDB(drutama("aebagianpembeliannama"), ""), sptField,
                     FxDB(drutama("aeterminnama"), ""), sptField,
                     FxDB(drutama("aeterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("aerekdiskonnama"), ""), sptField,
                     FxDB(drutama("aerekpajak1nama"), ""), sptField,
                     FxDB(drutama("aerekpajak2nama"), ""), sptField,
                     FxDB(drutama("aerekbiayalainnama"), ""), sptField,
                     FxDB(drutama("aerekbayarnama"), ""), sptField,
                     FxDB(drutama("aenotransaksiao"), ""), sptField,
                     FxDB(drutama("aestatusnama"), ""), sptField,
                     FxDB(drutama("aestatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("aeinputusernama"), ""), sptField,
                     FxDB(drutama("aemodifikasiusernama"), ""), sptField,
                     FxDB(drutama("aecarabayar"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idaedetail"), ""), sptField,
                     FxDB(dr("idae"), ""), sptField,
                     FxDB(dr("idasset"), ""), sptField,
                     FxDB(dr("namaasset"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
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
                     FxDB(dr("rekdiskonpembelian"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idardetail"), ""), sptField,
                     FxDB(dr("idaqdetail"), ""), sptField,
                     FxDB(dr("idabdetail"), ""), sptField,
                     FxDB(dr("idaodetail"), ""), sptField,
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
                     FxDB(dr("kodeasset"), ""), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("cabangnama"), ""), sptField,
                     FxDB(dr("lokasinama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("aonotransaksi"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("satuan"), ""), sptRow)

            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA Asset
            sql = "select `a`.`aid` AS `aid`,`a`.`akode` AS `akode`,`a`.`anama` AS `anama`,`a`.`akategori` AS `akategori`,`a`.`acabang` AS `acabang`,`a`.`alokasi` AS `alokasi`,`a`.`adivisi` AS `adivisi`,`a`.`asubdivisi` AS `asubdivisi`,`a`.`acatatan` AS `acatatan`,`a`.`anomor` AS `anomor`,`a`.`atglbeli` AS `atglbeli`,`a`.`atglpakai` AS `atglpakai`,`a`.`amatauang` AS `amatauang`,`a`.`akurs` AS `akurs`,`a`.`ahargabeli` AS `ahargabeli`,`a`.`anilairesidu` AS `anilairesidu`,`a`.`aumurekonomis` AS `aumurekonomis`,`a`.`abebanperbln` AS `abebanperbln`,`a`.`aakumulasibeban` AS `aakumulasibeban`,`a`.`anilaibuku` AS `anilaibuku`,`a`.`ametode` AS `ametode`,`a`.`atabelpenyusutan` AS `atabelpenyusutan`,`a`.`aintangible` AS `aintangible`,`a`.`afiskal` AS `afiskal`,`a`.`aatastengahbulan` AS `aatastengahbulan`,`a`.`arekasset` AS `arekasset`,`a`.`arekakumdepresiasi` AS `arekakumdepresiasi`,`a`.`arekdepresiasi` AS `arekdepresiasi`,`a`.`arekpenghapusan` AS `arekpenghapusan`,`a`.`aprodusen` AS `aprodusen`,`a`.`atglpensiun` AS `atglpensiun`,`a`.`apenyusutanke` AS `apenyusutanke`,`a`.`anilaimenurun` AS `anilaimenurun`,`a`.`adispose` AS `adispose`,`a`.`apembelian` AS `apembelian`,`a`.`apenjualan` AS `apenjualan`,`a`.`alocked` AS `alocked`,`a`.`astatus` AS `astatus`,`a`.`astatussebelumnya` AS `astatussebelumnya`,`a`.`aisclose` AS `aisclose`,`a`.`ainputuser` AS `ainputuser`,`a`.`ainputtgl` AS `ainputtgl`,`a`.`amodifikasiuser` AS `amodifikasiuser`,`a`.`amodifikasitgl` AS `amodifikasitgl`,`ac`.`acnama` AS `akategorinama`,`br`.`bnama` AS `acabangnama`,`l`.`lnama` AS `alokasinama`,`d`.`dnama` AS `adivisinama`,`sd`.`sdnama` AS `asubdivisinama`,`dc`.`nama` AS `ametodenama`,`coa1`.`cnama` AS `arekassetnama`,`coa2`.`cnama` AS `arekakumdepresiasinama`,`coa3`.`cnama` AS `arekdepresiasinama`,`coa4`.`cnama` AS `arekpenghapusannama`,`c1`.`kkode` AS `aprodusenkode`,`c1`.`knama` AS `aprodusennama`,`sp1`.`nama` AS `astatusnama`,`sp2`.`nama` AS `astatussebelumnyanama`,`u1`.`unama` AS `ainputusernama`,`u2`.`unama` AS `amodifikasiusernama`,`a`.`acustomtext1` AS `acustomtext1`,`a`.`acustomtext2` AS `acustomtext2`,`a`.`acustomtext3` AS `acustomtext3`,`a`.`acustomtext4` AS `acustomtext4`,`a`.`acustomtext5` AS `acustomtext5`,`a`.`acustomint1` AS `acustomint1`,`a`.`acustomint2` AS `acustomint2`,`a`.`acustomint3` AS `acustomint3`,`a`.`acustomdbl1` AS `acustomdbl1`,`a`.`acustomdbl2` AS `acustomdbl2`,`a`.`acustomdbl3` AS `acustomdbl3`,`a`.`acustomdate1` AS `acustomdate1`,`a`.`acustomdate2` AS `acustomdate2`,`a`.`acustomdate3` AS `acustomdate3`,`a`.`asatuan` AS `asatuan`, `a`.`aharga` AS `aharga`, `a`.`adiskon` AS `adiskon`,`a`.`ajmldiskon` AS `ajmldiskon`,`a`.`apajak1` AS `apajak1`,`a`.`ajmlpajak1` AS `ajmlpajak1`,`a`.`apajak2` AS `apajak2`,`a`.`ajmlpajak2` AS `ajmlpajak2` from ((((((((((((((((`m7_asset` `a` left join `m7_asset_category` `ac` on((`a`.`akategori` = `ac`.`ackode`))) left join `m1_branch` `br` on((`a`.`acabang` = `br`.`bkode`))) left join `m1_location` `l` on((`a`.`alokasi` = `l`.`lkode`))) left join `m1_division` `d` on((`a`.`adivisi` = `d`.`dkode`))) left join `m1_subdivision` `sd` on((`a`.`asubdivisi` = `sd`.`sdkode`))) left join `m7_depreciation_category` `dc` on((`a`.`ametode` = `dc`.`kode`))) left join `m1_coa` `coa1` on((`a`.`arekasset` = `coa1`.`cnomor`))) left join `m1_coa` `coa2` on((`a`.`arekakumdepresiasi` = `coa2`.`cnomor`))) left join `m1_coa` `coa3` on((`a`.`arekdepresiasi` = `coa3`.`cnomor`))) left join `m1_coa` `coa4` on((`a`.`arekpenghapusan` = `coa4`.`cnomor`))) left join `m1_contact` `c1` on((`a`.`aprodusen` = `c1`.`kid`))) left join `m0_status_progress` `sp1` on((`a`.`astatus` = `sp1`.`kode`))) left join `m0_status_progress` `sp2` on((`a`.`astatussebelumnya` = `sp2`.`kode`))) left join `m0_user` `u1` on((`a`.`ainputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`a`.`amodifikasiuser` = `u2`.`userid`))) left join `m7_ae_detail` `ae` on((`a`.`aid` = `ae`.`idasset`)))"
            Dim dtasset As New DataTable
            dtasset = AmbilData("aplikasi1-m7_asset", "idae = '" & idtransaksi & "'", "", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtasset.Rows
                asset = String.Concat(asset,
                      FxDB(dr("aid"), ""), sptField,
                     FxDB(dr("akode"), ""), sptField,
                     FxDB(dr("anama"), ""), sptField,
                     FxDB(dr("akategori"), ""), sptField,
                     FxDB(dr("acabang"), ""), sptField,
                     FxDB(dr("alokasi"), ""), sptField,
                     FxDB(dr("adivisi"), ""), sptField,
                     FxDB(dr("asubdivisi"), ""), sptField,
                     FxDB(dr("acatatan"), ""), sptField,
                     FxDB(dr("anomor"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("atglbeli"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("atglpakai"), ""), formatTgl), sptField,
                     FxDB(dr("amatauang"), ""), sptField,
                     FxDB(dr("akurs"), 0), sptField,
                     FxDB(dr("ahargabeli"), 0), sptField,
                     FxDB(dr("anilairesidu"), 0), sptField,
                     FxDB(dr("aumurekonomis"), 0), sptField,
                     FxDB(dr("abebanperbln"), 0), sptField,
                     FxDB(dr("aakumulasibeban"), 0), sptField,
                     FxDB(dr("anilaibuku"), 0), sptField,
                     FxDB(dr("ametode"), 0), sptField,
                     FxDB(dr("atabelpenyusutan"), ""), sptField,
                     FxDB(dr("aintangible"), 0), sptField,
                     FxDB(dr("afiskal"), 0), sptField,
                     FxDB(dr("aatastengahbulan"), 0), sptField,
                     FxDB(dr("arekasset"), ""), sptField,
                     FxDB(dr("arekakumdepresiasi"), ""), sptField,
                     FxDB(dr("arekdepresiasi"), ""), sptField,
                     FxDB(dr("arekpenghapusan"), ""), sptField,
                     FxDB(dr("aprodusen"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("atglpensiun"), ""), formatTgl), sptField,
                     FxDB(dr("apenyusutanke"), 0), sptField,
                     FxDB(dr("anilaimenurun"), 0), sptField,
                     FxDB(dr("adispose"), 0), sptField,
                     FxDB(dr("apembelian"), 0), sptField,
                     FxDB(dr("apenjualan"), 0), sptField,
                     FxDB(dr("alocked"), 0), sptField,
                     FxDB(dr("astatus"), 0), sptField,
                     FxDB(dr("astatussebelumnya"), 0), sptField,
                     FxDB(dr("aisclose"), 0), sptField,
                     FxDB(dr("ainputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ainputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("amodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("amodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("akategorinama"), ""), sptField,
                     FxDB(dr("acabangnama"), ""), sptField,
                     FxDB(dr("alokasinama"), ""), sptField,
                     FxDB(dr("adivisinama"), ""), sptField,
                     FxDB(dr("asubdivisinama"), ""), sptField,
                     FxDB(dr("ametodenama"), ""), sptField,
                     FxDB(dr("arekassetnama"), ""), sptField,
                     FxDB(dr("arekakumdepresiasinama"), ""), sptField,
                     FxDB(dr("arekdepresiasinama"), ""), sptField,
                     FxDB(dr("arekpenghapusannama"), ""), sptField,
                     FxDB(dr("aprodusenkode"), ""), sptField,
                     FxDB(dr("aprodusennama"), ""), sptField,
                     FxDB(dr("astatusnama"), ""), sptField,
                     FxDB(dr("astatussebelumnyanama"), ""), sptField,
                     FxDB(dr("ainputusernama"), ""), sptField,
                     FxDB(dr("amodifikasiusernama"), ""), sptField,
                     FxDB(dr("acustomtext1"), ""), sptField,
                     FxDB(dr("acustomtext2"), ""), sptField,
                     FxDB(dr("acustomtext3"), ""), sptField,
                     FxDB(dr("acustomtext4"), ""), sptField,
                     FxDB(dr("acustomtext5"), ""), sptField,
                     FxDB(dr("acustomint1"), 0), sptField,
                     FxDB(dr("acustomint2"), 0), sptField,
                     FxDB(dr("acustomint3"), 0), sptField,
                     FxDB(dr("acustomdbl1"), 0), sptField,
                     FxDB(dr("acustomdbl2"), 0), sptField,
                     FxDB(dr("acustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("acustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("acustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("acustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("asatuan"), ""), sptField,
                     FxDB(dr("aharga"), ""), sptField,
                     FxDB(dr("adiskon"), ""), sptField,
                     FxDB(dr("ajmldiskon"), ""), sptField,
                     FxDB(dr("apajak1"), ""), sptField,
                     FxDB(dr("ajmlpajak1"), ""), sptField,
                     FxDB(dr("apajak2"), ""), sptField,
                     FxDB(dr("ajmlpajak2"), ""), sptRow)
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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, asset)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("aeid, aecabang, aelokasi, aesumber, aeautonotransaksi, aenotransaksi, aetgl, aekodepa, aesupplier, aesupplierkontak, ae1alamat1, ae1alamat2, ae1alamat3, ae2alamat1, ae2alamat2, ae2alamat3, aebagianpembelian, aetermin, aetgljatuhtempo, aeuraian, aecatatan, aenoref, aetglnoref, aetglpenutupan, aematauang, aekurs, aehargatermasukpajak, aetotal, aediskonpersen, aejmldiskon, aetotalpajak1detail, aetotalpajak2detail, aebiayalainpersen, aebiayalain, aetotaltransaksi, aejmlbayar, aestatuslunas, aetgllunas, aenofakturpajak, aesdhbayarpajak, aetglbayarpajak, aerekdiskon, aerekpajak1, aerekpajak2, aerekbiayalain, aerekbayar, aeidar, aeidaq, aeidab, aeidao, aestatusrealisasi, aestatus, aestatussebelumnya, aejmlrevisi, aecetakanke, aeinputuser, aeinputtgl, aemodifikasiuser, aemodifikasitgl, aeposting, aepostingtgl, aetutupperiode, aeisclose, aecustomtext1, aecustomtext2, aecustomtext3, aecustomtext4, aecustomtext5, aecustomint1, aecustomint2, aecustomint3, aecustomdbl1, aecustomdbl2, aecustomdbl3, aecustomdate1, aecustomdate2, aecustomdate3, aecabangnama, aelokasinama, aesupplierkode, aesuppliernama, aebagianpembeliankode, aebagianpembeliannama, aeterminnama,aeterminharijatuhtempo, aerekdiskonnama, aerekpajak1nama, aerekpajak2nama, aerekbiayalainnama, aerekbayarnama, aenotransaksiao, aestatusnama, aestatussebelumnyanama, aeinputusernama, aemodifikasiusernama, aecarabayar" & sptSubParam & "idaedetail, idae, idasset, namaasset, jml, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, rekdiskonpembelian, costcenter, divisi, subdivisi, proyek, catatan, urutan, idardetail, idaqdetail, idabdetail, idaodetail, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodeasset, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, costcenternama, subdivisinama, aonotransaksi, divisinama,  satuan" & sptSubParam & "aid, akode, anama, akategori, acabang, alokasi, adivisi, asubdivisi, acatatan, anomor, atglbeli, atglpakai, amatauang, akurs, ahargabeli, anilairesidu, aumurekonomis, abebanperbln, aakumulasibeban, anilaibuku, ametode, atabelpenyusutan, aintangible, afiskal, aatastengahbulan, arekasset, arekakumdepresiasi,arekdepresiasi, arekpenghapusan, aprodusen, atglpensiun, apenyusutanke, anilaimenurun, adispose, apembelian, apenjualan, alocked, astatus, astatussebelumnya, aisclose, ainputuser, ainputtgl, amodifikasiuser, amodifikasitgl, akategorinama, acabangnama, alokasinama,adivisinama, asubdivisinama, ametodenama, arekassetnama, arekakumdepresiasinama, arekdepresiasinama, arekpenghapusannama, aprodusenkode, aprodusennama, astatusnama, astatussebelumnyanama, ainputusernama, amodifikasiusernama, acustomtext1, acustomtext2, acustomtext3, acustomtext4, acustomtext5, acustomint1, acustomint2, acustomint3, acustomdbl1, acustomdbl2, acustomdbl3, acustomdate1, acustomdate2, acustomdate3, asatuan, aharga, adiskon, ajmldiskon, apajak1, ajmlpajak1, apajak2, ajmlpajak2"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M7_AeSearch(ByVal param As String) As String
        'M7_AeSearch --------------------------------------------------------
        'aeid, aecabang, aelokasi, aesumber, aeautonotransaksi, aenotransaksi, aetgl, 
        'aekodepa, aesupplier, aesupplierkontak, ae1alamat1, ae1alamat2, ae1alamat3, ae2alamat1, 
        'ae2alamat2, ae2alamat3, aebagianpembelian, aetermin, aetgljatuhtempo, aeuraian, aecatatan, 
        'aenoref, aetglnoref, aetglpenutupan, aematauang, aekurs, aehargatermasukpajak, aetotal, 
        'aediskonpersen, aejmldiskon, aetotalpajak1detail, aetotalpajak2detail, aebiayalainpersen, aebiayalain, aetotaltransaksi, 
        'aejmlbayar, aestatuslunas, aetgllunas, aenofakturpajak, aesdhbayarpajak, aetglbayarpajak, aerekdiskon, 
        'aerekpajak1, aerekpajak2, aerekbiayalain, aerekbayar, aeidar, aeidaq, aeidab, 
        'aeidao, aestatusrealisasi, aestatus, aestatussebelumnya, aejmlrevisi, aecetakanke, aeinputuser, 
        'aeinputtgl, aemodifikasiuser, aemodifikasitgl, aeposting, aepostingtgl, aetutupperiode, aeisclose, 
        'aecabangnama, aelokasinama, aesupplierkode, aesuppliernama, aebagianpembeliankode, aebagianpembeliannama, aonotransaksi, 
        'aestatusnama, aestatussebelumnyanama, aeinputusernama, aemodifikasiusernama

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
            Filter = Filter.Replace("aesupplierkode", "c1.kkode")
            Filter = Filter.Replace("aesuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        Dim query As New m0_query
        sql = query.PanggilQuery("m7_ae_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Rq", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                    FxDB(dr("aeid"), ""), sptField,
                     FxDB(dr("aecabang"), ""), sptField,
                     FxDB(dr("aelokasi"), ""), sptField,
                     FxDB(dr("aesumber"), ""), sptField,
                     FxDB(dr("aeautonotransaksi"), 0), sptField,
                     FxDB(dr("aenotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aetgl"), ""), formatTgl), sptField,
                     FxDB(dr("aekodepa"), ""), sptField,
                     FxDB(dr("aesupplier"), ""), sptField,
                     FxDB(dr("aesupplierkontak"), ""), sptField,
                     FxDB(dr("ae1alamat1"), ""), sptField,
                     FxDB(dr("ae1alamat2"), ""), sptField,
                     FxDB(dr("ae1alamat3"), ""), sptField,
                     FxDB(dr("ae2alamat1"), ""), sptField,
                     FxDB(dr("ae2alamat2"), ""), sptField,
                     FxDB(dr("ae2alamat3"), ""), sptField,
                     FxDB(dr("aebagianpembelian"), ""), sptField,
                     FxDB(dr("aetermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aetgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("aeuraian"), ""), sptField,
                     FxDB(dr("aecatatan"), ""), sptField,
                     FxDB(dr("aenoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aetglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("aetglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("aematauang"), ""), sptField,
                     FxDB(dr("aekurs"), 0), sptField,
                     FxDB(dr("aehargatermasukpajak"), 0), sptField,
                     FxDB(dr("aetotal"), 0), sptField,
                     FxDB(dr("aediskonpersen"), ""), sptField,
                     FxDB(dr("aejmldiskon"), 0), sptField,
                     FxDB(dr("aetotalpajak1detail"), 0), sptField,
                     FxDB(dr("aetotalpajak2detail"), 0), sptField,
                     FxDB(dr("aebiayalainpersen"), ""), sptField,
                     FxDB(dr("aebiayalain"), 0), sptField,
                     FxDB(dr("aetotaltransaksi"), 0), sptField,
                     FxDB(dr("aejmlbayar"), 0), sptField,
                     FxDB(dr("aestatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("aetgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("aenofakturpajak"), ""), sptField,
                     FxDB(dr("aesdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("aetglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(dr("aerekdiskon"), ""), sptField,
                     FxDB(dr("aerekpajak1"), ""), sptField,
                     FxDB(dr("aerekpajak2"), ""), sptField,
                     FxDB(dr("aerekbiayalain"), ""), sptField,
                     FxDB(dr("aerekbayar"), ""), sptField,
                     FxDB(dr("aeidar"), ""), sptField,
                     FxDB(dr("aeidaq"), ""), sptField,
                     FxDB(dr("aeidab"), ""), sptField,
                     FxDB(dr("aeidao"), ""), sptField,
                     FxDB(dr("aestatusrealisasi"), 0), sptField,
                     FxDB(dr("aestatus"), 0), sptField,
                     FxDB(dr("aestatussebelumnya"), 0), sptField,
                     FxDB(dr("aejmlrevisi"), 0), sptField,
                     FxDB(dr("aecetakanke"), 0), sptField,
                     FxDB(dr("aeinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aeinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("aemodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aemodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("aeposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("aepostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("aetutupperiode"), 0), sptField,
                     FxDB(dr("aeisclose"), 0), sptField,
                     FxDB(dr("aecabangnama"), ""), sptField,
                     FxDB(dr("aelokasinama"), ""), sptField,
                     FxDB(dr("aesupplierkode"), ""), sptField,
                     FxDB(dr("aesuppliernama"), ""), sptField,
                     FxDB(dr("aebagianpembeliankode"), ""), sptField,
                     FxDB(dr("aebagianpembeliannama"), ""), sptField,
                     FxDB(dr("aonotransaksi"), ""), sptField,
                     FxDB(dr("aestatusnama"), ""), sptField,
                     FxDB(dr("aestatussebelumnyanama"), ""), sptField,
                     FxDB(dr("aeinputusernama"), ""), sptField,
                     FxDB(dr("aemodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("aeid, aecabang, aelokasi, aesumber, aeautonotransaksi, aenotransaksi, aetgl, ekodepa, aesupplier, aesupplierkontak, ae1alamat1, ae1alamat2, ae1alamat3, ae2alamat1, ae2alamat2, ae2alamat3, aebagianpembelian, aetermin, aetgljatuhtempo, aeuraian, aecatatan, aenoref, aetglnoref, aetglpenutupan, aematauang, aekurs, aehargatermasukpajak, aetotal, aediskonpersen, aejmldiskon, aetotalpajak1detail, aetotalpajak2detail, aebiayalainpersen, aebiayalain, aetotaltransaksi, aejmlbayar, aestatuslunas, aetgllunas, aenofakturpajak, aesdhbayarpajak, aetglbayarpajak, aerekdiskon, aerekpajak1, aerekpajak2, aerekbiayalain, aerekbayar, aeidar, aeidaq, aeidab, aeidao, aestatusrealisasi, aestatus, aestatussebelumnya, aejmlrevisi, aecetakanke, aeinputuser, aeinputtgl, aemodifikasiuser, aemodifikasitgl, aeposting, aepostingtgl, aetutupperiode, aeisclose, aecabangnama, aelokasinama, aesupplierkode, aesuppliernama, aebagianpembeliankode, aebagianpembeliannama, aonotransaksi, aestatusnama, aestatussebelumnyanama, aeinputusernama, aemodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M7_AeTerkait(ByVal param As String) As String
        'M7_AeTerkait --------------------------------------------------------
        'aeid, rinotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "riid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m7_ae_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("aeid"), 0), sptField,
                     FxDB(dr("aenotransaksi"), ""), sptField,
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
            result(2) = "Related AE data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("aeid, aenotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function



End Class
