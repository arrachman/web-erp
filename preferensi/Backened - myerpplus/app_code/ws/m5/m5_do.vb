Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_do
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_DoSimpan(ByVal param As String) As String
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
        'doid(0) As Integer, docabang(1) As String, dolokasi(2) As String, dogudang(3) As String, doasalbarang(4) As String, 
        'doasalbarangkategori(5) As Integer, dojenispenjualan(6) As String, dojenispenjualankategori(7) As Integer, docarabayar(8) As Integer, dosumber(9) As String, 
        'doautonotransaksi(10) As Integer, donotransaksi(11) As String, dotgl(12) As Date, dokodepa(13) As Integer, docustomer(14) As Integer, 
        'docustomerkontak(15) As String, do1alamat1(16) As String, do1alamat2(17) As String, do1alamat3(18) As String, do2alamat1(19) As String, 
        'do2alamat2(20) As String, do2alamat3(21) As String, dobagianpenjualan(22) As Integer, dobagianpengiriman(23) As Integer, doekspedisi(24) As String, 
        'dotglkirim(25) As Date, dotermin(26) As String, dotgljatuhtempo(27) As Date, douraian(28) As String, docatatan(29) As String, 
        'donoref(30) As String, dotglnoref(31) As Date, dotglpenutupan(32) As Date, domatauang(33) As String, dokurs(34) As Double, 
        'dohargatermasukpajak(35) As Integer, dototal(36) As Double, dodiskonpersen(37) As String, dojmldiskon(38) As Double, dototalpajak1detail(39) As Double, 
        'dototalpajak2detail(40) As Double, dobiayalainpersen(41) As Double, dobiayalain(42) As Double, dototaltransaksi(43) As Double, dorekdiskon(44) As String, 
        'dorekpajak1(45) As String, dorekpajak2(46) As String, dorekbiayalain(47) As String, doidsq(48) As Integer, doidso(49) As Integer, 
        'doidpi(50) As Integer, doidpl(51) As Integer, dostatusdr(52) As Integer, dostatussi(53) As Integer, dostatusrnr(54) As Integer, 
        'dostatussr(55) As Integer, dostatus(56) As Integer, dostatussebelumnya(57) As Integer, dojmlrevisi(58) As Integer, docetakanke(59) As Integer, 
        'doinputuser(60) As Integer, doinputtgl(61) As DateTime, domodifikasiuser(62) As Integer, domodifikasitgl(63) As DateTime, doposting(64) As Integer, 
        'dotutupperiode(65) As Integer, doisclose(66) As Integer, docustomtext1(67) As String, docustomtext2(68) As String, docustomtext3(69) As String, 
        'docustomtext4(70) As String, docustomtext5(71) As String, docustomint1(72) As Integer, docustomint2(73) As Integer, docustomint3(74) As Integer, 
        'docustomdbl1(75) As Double, docustomdbl2(76) As Double, docustomdbl3(77) As Double, docustomdate1(78) As Date, docustomdate2(79) As Date, 
        'docustomdate3(80) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'doid, docabang, dolokasi, dogudang, doasalbarang, doasalbarangkategori, dojenispenjualan, 
        'dojenispenjualankategori, docarabayar, dosumber, doautonotransaksi, donotransaksi, dotgl, dokodepa, 
        'docustomer, docustomerkontak, do1alamat1, do1alamat2, do1alamat3, do2alamat1, do2alamat2, 
        'do2alamat3, dobagianpenjualan, dobagianpengiriman, doekspedisi, dotglkirim, dotermin, dotgljatuhtempo, 
        'douraian, docatatan, donoref, dotglnoref, dotglpenutupan, domatauang, dokurs, 
        'dohargatermasukpajak, dototal, dodiskonpersen, dojmldiskon, dototalpajak1detail, dototalpajak2detail, dobiayalainpersen, 
        'dobiayalain, dototaltransaksi, dorekdiskon, dorekpajak1, dorekpajak2, dorekbiayalain, doidsq, 
        'doidso, doidpi, doidpl, dostatusdr, dostatussi, dostatusrnr, dostatussr, 
        'dostatus, dostatussebelumnya, dojmlrevisi, docetakanke, doinputuser, doinputtgl, domodifikasiuser, 
        'domodifikasitgl, doposting, dotutupperiode, doisclose, docustomtext1, docustomtext2, docustomtext3, 
        'docustomtext4, docustomtext5, docustomint1, docustomint2, docustomint3, docustomdbl1, docustomdbl2, 
        'docustomdbl3, docustomdate1, docustomdate2, docustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 81) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'doid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "doid required numeric." : GoTo selesai
        End If
        'doasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "doasalbarangkategori required numeric." : GoTo selesai
        End If
        'dojenispenjualankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "dojenispenjualankategori required numeric." : GoTo selesai
        End If
        'docarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "docarabayar required numeric." : GoTo selesai
        End If
        'doautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "doautonotransaksi required numeric." : GoTo selesai
        End If
        'dotgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "dotgl required date." : GoTo selesai
        End If
        'dokodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "dokodepa required numeric." : GoTo selesai
        End If
        'docustomer(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "docustomer required numeric." : GoTo selesai
        End If
        'dobagianpenjualan(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "dobagianpenjualan required numeric." : GoTo selesai
        End If
        'dobagianpengiriman(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "dobagianpengiriman required numeric." : GoTo selesai
        End If
        'dotglkirim(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "dotglkirim required date." : GoTo selesai
        End If
        'dotgljatuhtempo(27) As Date
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "dotgljatuhtempo required date." : GoTo selesai
        End If
        'dotglnoref(31) As Date
        If (IsDate(dataUtama(31)) = False) Then
            result(2) = "dotglnoref required date." : GoTo selesai
        End If
        'dotglpenutupan(32) As Date
        If (IsDate(dataUtama(32)) = False) Then
            result(2) = "dotglpenutupan required date." : GoTo selesai
        End If
        'dokurs(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "dokurs required numeric." : GoTo selesai
        End If
        'dohargatermasukpajak(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "dohargatermasukpajak required numeric." : GoTo selesai
        End If
        'dototal(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "dototal required numeric." : GoTo selesai
        End If
        'dojmldiskon(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "dojmldiskon required numeric." : GoTo selesai
        End If
        'dototalpajak1detail(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "dototalpajak1detail required numeric." : GoTo selesai
        End If
        'dototalpajak2detail(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "dototalpajak2detail required numeric." : GoTo selesai
        End If
        ''dobiayalainpersen(41) As Double
        'If (IsNumeric(dataUtama(41)) = False) Then
        '    result(2) = "dobiayalainpersen required numeric." : GoTo selesai
        'End If
        'dobiayalain(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "dobiayalain required numeric." : GoTo selesai
        End If
        'dototaltransaksi(43) As Double
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "dototaltransaksi required numeric." : GoTo selesai
        End If
        'doidsq(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "doidsq required numeric." : GoTo selesai
        End If
        'doidso(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "doidso required numeric." : GoTo selesai
        End If
        'doidpi(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "doidpi required numeric." : GoTo selesai
        End If
        'doidpl(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "doidpl required numeric." : GoTo selesai
        End If
        'dostatusdr(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "dostatusdr required numeric." : GoTo selesai
        End If
        'dostatussi(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "dostatussi required numeric." : GoTo selesai
        End If
        'dostatusrnr(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "dostatusrnr required numeric." : GoTo selesai
        End If
        'dostatussr(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "dostatussr required numeric." : GoTo selesai
        End If
        'dostatus(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "dostatus required numeric." : GoTo selesai
        End If
        'dostatussebelumnya(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "dostatussebelumnya required numeric." : GoTo selesai
        End If
        'dojmlrevisi(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "dojmlrevisi required numeric." : GoTo selesai
        End If
        'docetakanke(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "docetakanke required numeric." : GoTo selesai
        End If
        'doinputuser(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "doinputuser required numeric." : GoTo selesai
        End If
        'doinputtgl(61) As DateTime
        If (IsDate(dataUtama(61)) = False) Then
            result(2) = "doinputtgl required date." : GoTo selesai
        End If
        'domodifikasiuser(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "domodifikasiuser required numeric." : GoTo selesai
        End If
        'domodifikasitgl(63) As DateTime
        If (IsDate(dataUtama(63)) = False) Then
            result(2) = "domodifikasitgl required date." : GoTo selesai
        End If
        'doposting(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "doposting required numeric." : GoTo selesai
        End If
        'dotutupperiode(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "dotutupperiode required numeric." : GoTo selesai
        End If
        'doisclose(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "doisclose required numeric." : GoTo selesai
        End If
        'docustomint1(72) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "docustomint1 required numeric." : GoTo selesai
        End If
        'docustomint2(73) As Integer
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "docustomint2 required numeric." : GoTo selesai
        End If
        'docustomint3(74) As Integer
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "docustomint3 required numeric." : GoTo selesai
        End If
        'docustomdbl1(75) As Double
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "docustomdbl1 required numeric." : GoTo selesai
        End If
        'docustomdbl2(76) As Double
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "docustomdbl2 required numeric." : GoTo selesai
        End If
        'docustomdbl3(77) As Double
        If (IsNumeric(dataUtama(77)) = False) Then
            result(2) = "docustomdbl3 required numeric." : GoTo selesai
        End If
        'docustomdate1(78) As Date
        If (IsDate(dataUtama(78)) = False) Then
            result(2) = "docustomdate1 required date." : GoTo selesai
        End If
        'docustomdate2(79) As Date
        If (IsDate(dataUtama(79)) = False) Then
            result(2) = "docustomdate2 required date." : GoTo selesai
        End If
        'docustomdate3(80) As Date
        If (IsDate(dataUtama(80)) = False) Then
            result(2) = "docustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'docabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "docabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "docabang should not be more than 25 character." : GoTo selesai
        End If

        'dolokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "dolokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "dolokasi should not be more than 25 character." : GoTo selesai
        End If

        'dogudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "dogudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "dogudang should not be more than 25 character." : GoTo selesai
        End If

        'dosumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "dosumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "dosumber should not be more than 10 character." : GoTo selesai
        End If

        'donotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "donotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "donotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'dotgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "dotgl can't be empty" : GoTo selesai
        End If

        'dotglkirim(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "dotglkirim can't be empty" : GoTo selesai
        End If

        'dotgljatuhtempo(27) As Date
        If Len(dataUtama(27)) = 0 Then
            result(2) = "dotgljatuhtempo can't be empty" : GoTo selesai
        End If

        'dotglnoref(31) As Date
        If Len(dataUtama(31)) = 0 Then
            result(2) = "dotglnoref can't be empty" : GoTo selesai
        End If

        'dotglpenutupan(32) As Date
        If Len(dataUtama(32)) = 0 Then
            result(2) = "dotglpenutupan can't be empty" : GoTo selesai
        End If

        'domatauang(33) As String
        If Len(dataUtama(33)) = 0 Then
            result(2) = "domatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(33)) > 25 Then
            result(2) = "domatauang should not be more than 25 character." : GoTo selesai
        End If

        'dokurs(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "dokurs can't be empty" : GoTo selesai
        End If

        'dototal(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "dototal can't be empty" : GoTo selesai
        End If

        'dodiskonpersen(37) As String
        If Len(dataUtama(37)) = 0 Then
            result(2) = "dodiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(37)) > 25 Then
            result(2) = "dodiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'dojmldiskon(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "dojmldiskon can't be empty" : GoTo selesai
        End If

        'dototalpajak1detail(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "dototalpajak1detail can't be empty" : GoTo selesai
        End If

        'dototalpajak2detail(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "dototalpajak2detail can't be empty" : GoTo selesai
        End If

        'dobiayalainpersen(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "dobiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(41)) > 25 Then
            result(2) = "dobiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'dobiayalain(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "dobiayalain can't be empty" : GoTo selesai
        End If

        'dototaltransaksi(43) As Double
        If Len(dataUtama(43)) = 0 Then
            result(2) = "dototaltransaksi can't be empty" : GoTo selesai
        End If

        'doinputtgl(61) As DateTime
        If Len(dataUtama(61)) = 0 Then
            result(2) = "doinputtgl can't be empty" : GoTo selesai
        End If

        'domodifikasitgl(63) As DateTime
        If Len(dataUtama(63)) = 0 Then
            result(2) = "domodifikasitgl can't be empty" : GoTo selesai
        End If

        'docustomdbl1(75) As Double
        If Len(dataUtama(75)) = 0 Then
            result(2) = "docustomdbl1 can't be empty" : GoTo selesai
        End If

        'docustomdbl2(76) As Double
        If Len(dataUtama(76)) = 0 Then
            result(2) = "docustomdbl2 can't be empty" : GoTo selesai
        End If

        'docustomdbl3(77) As Double
        If Len(dataUtama(77)) = 0 Then
            result(2) = "docustomdbl3 can't be empty" : GoTo selesai
        End If

        'docustomdate1(78) As Date
        If Len(dataUtama(78)) = 0 Then
            result(2) = "docustomdate1 can't be empty" : GoTo selesai
        End If

        'docustomdate2(79) As Date
        If Len(dataUtama(79)) = 0 Then
            result(2) = "docustomdate2 can't be empty" : GoTo selesai
        End If

        'docustomdate3(80) As Date
        If Len(dataUtama(80)) = 0 Then
            result(2) = "docustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "doid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "docabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dolokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dogudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "doasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "doasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dojenispenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dojenispenjualankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "docarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dosumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "doautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "donotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dotgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dokodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "docustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "docustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "do1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "do1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "do1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "do2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "do2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "do2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dobagianpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dobagianpengiriman", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "doekspedisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dotglkirim", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dotermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dotgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "douraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "docatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "donoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dotglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dotglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "domatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dokurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dohargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dototal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dodiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dojmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dototalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dototalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dobiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dobiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dototaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dorekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dorekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dorekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dorekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "doidsq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "doidso", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "doidpi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "doidpl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dostatusdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dostatussi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dostatusrnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dostatussr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dostatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dostatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dojmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "docetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "doinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "doinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "domodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "domodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "doposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dotutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "doisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "docustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "docustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "docustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "docustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "docustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "docustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "docustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "docustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "docustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "docustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "docustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "docustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "docustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "docustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "doid~docabang~dolokasi~dogudang~doasalbarang~doasalbarangkategori~dojenispenjualan~dojenispenjualankategori~docarabayar~dosumber~doautonotransaksi~donotransaksi~dotgl~dokodepa~docustomer~docustomerkontak~do1alamat1~do1alamat2~do1alamat3~do2alamat1~do2alamat2~do2alamat3~dobagianpenjualan~dobagianpengiriman~doekspedisi~dotglkirim~dotermin~dotgljatuhtempo~douraian~docatatan~donoref~dotglnoref~dotglpenutupan~domatauang~dokurs~dohargatermasukpajak~dototal~dodiskonpersen~dojmldiskon~dototalpajak1detail~dototalpajak2detail~dobiayalainpersen~dobiayalain~dototaltransaksi~dorekdiskon~dorekpajak1~dorekpajak2~dorekbiayalain~doidsq~doidso~doidpi~doidpl~dostatusdr~dostatussi~dostatusrnr~dostatussr~dostatus~dostatussebelumnya~dojmlrevisi~docetakanke~doinputuser~doinputtgl~domodifikasiuser~domodifikasitgl~doposting~dotutupperiode~doisclose~docustomtext1~docustomtext2~docustomtext3~docustomtext4~docustomtext5~docustomint1~docustomint2~docustomint3~docustomdbl1~docustomdbl2~docustomdbl3~docustomdate1~docustomdate2~docustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'iddodetail(0) As Integer, iddo(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, idhppkhususmasuk(12) As Integer, idhppfifomasuk(13) As Integer, harga(14) As Double, 
        'hpp(15) As Double, diskon(16) As String, jmldiskon(17) As Double, pajak1(18) As String, jmlpajak1(19) As Double, 
        'pajak2(20) As String, jmlpajak2(21) As Double, cabang(22) As String, lokasi(23) As String, gudangasal(24) As String, 
        'gudangtransit(25) As String, gudangtujuan(26) As String, rekpersediaan(27) As String, rekhargapokok(28) As String, rekdiskonpenjualan(29) As String, 
        'costcenter(30) As String, divisi(31) As String, subdivisi(32) As String, proyek(33) As String, catatan(34) As String, 
        'urutan(35) As Integer, idsqdetail(36) As Integer, idsodetail(37) As Integer, idpidetail(38) As Integer, idpldetail(39) As Integer, 
        'jmldr(40) As Double, statusdr(41) As Integer, jmlsi(42) As Double, statussi(43) As Integer, jmlrnr(44) As Double, 
        'statusrnr(45) As Integer, jmlsr(46) As Double, statussr(47) As Integer, isclose(48) As Integer, customtext1(49) As String, 
        'customtext2(50) As String, customtext3(51) As String, customdbl1(52) As Double, customdbl2(53) As Double, customdbl3(54) As Double, 
        'customdate1(55) As Date, customdate2(56) As Date, customdate3(57) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'iddodetail, iddo, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, idhppkhususmasuk, idhppfifomasuk, 
        'harga, hpp, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, 
        'jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, 
        'rekhargapokok, rekdiskonpenjualan, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idsqdetail, idsodetail, idpidetail, idpldetail, jmldr, statusdr, 
        'jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "iddodetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "iddo", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "idhppkhususmasuk", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idhppfifomasuk", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
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
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idsqdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idsodetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idpidetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idpldetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmldr", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusdr", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "transbarang", AsEnumTypeData.AsInt64)

        'Variabel ValidasiBatchSerial
        Dim ftBarang As String = ""
        Dim idbarang As Integer = 0

        'Variabel ValidasiSimpan
        Dim ftExistOutstandingSO As String = "", ftOutstandingSO As String = "", updNilaiSO As String = "", updFilterSO As String = ""
        Dim ftExistOutstandingPI As String = "", ftOutstandingPI As String = "", updNilaiPI As String = "", updFilterPI As String = ""
        Dim ftExistOutstandingPL As String = "", ftOutstandingPL As String = "", updNilaiPL As String = "", updFilterPL As String = ""
        Dim idsodetail As Integer = 0, idpidetail As Integer = 0, idpldetail As Integer = 0, jmlbarang As Double = 0
        Dim ftExistStok As String = "", ftStok As String = "", ftStokAvailable As String = ""
        Dim updStokOut As String = "", gudangOut As String = "", updStokOutBooking As String = ""
        Dim updStokIn As String = "", gudangIn As String = ""
        Dim dtCostCenter As New DataTable, vTransBarang As Integer = 1

        'FILTER SO, PI, PL, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftSO As String = "", ftPI As String = "", ftPL As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 58) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'iddodetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - iddodetail required numeric." : GoTo selesai
            End If
            'iddo(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - iddo required numeric." : GoTo selesai
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
            'idhppkhususmasuk(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - idhppkhususmasuk required numeric." : GoTo selesai
            End If
            'idhppfifomasuk(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - idhppfifomasuk required numeric." : GoTo selesai
            End If
            'harga(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'hpp(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - hpp required numeric." : GoTo selesai
            End If
            'jmldiskon(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - jmldiskon required numeric." : GoTo selesai
            End If
            'jmlpajak1(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak1 required numeric." : GoTo selesai
            End If
            'jmlpajak2(21) As Double
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak2 required numeric." : GoTo selesai
            End If
            'urutan(35) As Integer
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idsqdetail(36) As Integer
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - idsqdetail required numeric." : GoTo selesai
            End If
            'idsodetail(37) As Integer
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - idsodetail required numeric." : GoTo selesai
            End If
            'idpidetail(38) As Integer
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - idpidetail required numeric." : GoTo selesai
            End If
            'idpldetail(39) As Integer
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - idpldetail required numeric." : GoTo selesai
            End If
            'jmldr(40) As Double
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - jmldr required numeric." : GoTo selesai
            End If
            'statusdr(41) As Integer
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - statusdr required numeric." : GoTo selesai
            End If
            'jmlsi(42) As Double
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - jmlsi required numeric." : GoTo selesai
            End If
            'statussi(43) As Integer
            If (IsNumeric(dataRowDetail(43)) = False) Then
                result(2) = "Row : " & i & " - statussi required numeric." : GoTo selesai
            End If
            'jmlrnr(44) As Double
            If (IsNumeric(dataRowDetail(44)) = False) Then
                result(2) = "Row : " & i & " - jmlrnr required numeric." : GoTo selesai
            End If
            'statusrnr(45) As Integer
            If (IsNumeric(dataRowDetail(45)) = False) Then
                result(2) = "Row : " & i & " - statusrnr required numeric." : GoTo selesai
            End If
            'jmlsr(46) As Double
            If (IsNumeric(dataRowDetail(46)) = False) Then
                result(2) = "Row : " & i & " - jmlsr required numeric." : GoTo selesai
            End If
            'statussr(47) As Integer
            If (IsNumeric(dataRowDetail(47)) = False) Then
                result(2) = "Row : " & i & " - statussr required numeric." : GoTo selesai
            End If
            'isclose(48) As Integer
            If (IsNumeric(dataRowDetail(48)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(52) As Double
            If (IsNumeric(dataRowDetail(52)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(53) As Double
            If (IsNumeric(dataRowDetail(53)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(54) As Double
            If (IsNumeric(dataRowDetail(54)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(55) As Date
            If (IsDate(dataRowDetail(55)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(56) As Date
            If (IsDate(dataRowDetail(56)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(57) As Date
            If (IsDate(dataRowDetail(57)) = False) Then
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

            'hpp(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - hpp can't be empty" : GoTo selesai
            End If

            'diskon(16) As String
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - diskon can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(16)) > 25 Then
                result(2) = "Row : " & i & " - diskon should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskon(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskon can't be empty" : GoTo selesai
                'Else
                '    'HITUNG JMLDISKON : jml(5) As Double, harga(14) As Double, diskon(16) As String
                '    dataRowDetail(17) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(14)), FixQuotes(dataRowDetail(16).ToString))
            End If

            'jmlpajak1(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak1 can't be empty" : GoTo selesai
            End If

            'jmlpajak2(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak2 can't be empty" : GoTo selesai
            End If

            'gudangasal(24) As String
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - gudangasal can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(24)) > 25 Then
                result(2) = "Row : " & i & " - gudangasal should not be more than 25 character." : GoTo selesai
            End If

            'gudangtransit(25) As String
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - gudangtransit can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(25)) > 25 Then
                result(2) = "Row : " & i & " - gudangtransit should not be more than 25 character." : GoTo selesai
            End If

            'gudangtujuan(26) As String
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - gudangtujuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(26)) > 25 Then
                result(2) = "Row : " & i & " - gudangtujuan should not be more than 25 character." : GoTo selesai
            End If

            'jmldr(40) As Double
            If Len(dataRowDetail(40)) = 0 Then
                result(2) = "Row : " & i & " - jmldr can't be empty" : GoTo selesai
            End If

            'jmlsi(42) As Double
            If Len(dataRowDetail(42)) = 0 Then
                result(2) = "Row : " & i & " - jmlsi can't be empty" : GoTo selesai
            End If

            'jmlrnr(44) As Double
            If Len(dataRowDetail(44)) = 0 Then
                result(2) = "Row : " & i & " - jmlrnr can't be empty" : GoTo selesai
            End If

            'jmlsr(46) As Double
            If Len(dataRowDetail(46)) = 0 Then
                result(2) = "Row : " & i & " - jmlsr can't be empty" : GoTo selesai
            End If

            'customdbl1(52) As Double
            If Len(dataRowDetail(52)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(53) As Double
            If Len(dataRowDetail(53)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(54) As Double
            If Len(dataRowDetail(54)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(55) As Date
            If Len(dataRowDetail(55)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(56) As Date
            If Len(dataRowDetail(56)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(57) As Date
            If Len(dataRowDetail(57)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            vTransBarang = 1
            'costcenter(30)
            If Len(dataRowDetail(30)) > 0 Then
                sql = "SELECT ccakun FROM m1_cost_center WHERE cckode = '" & FixQuotes(dataRowDetail(30)) & "'"
                dtCostCenter = AsDataTableAmbilDariDBCon(sql, myConn)
                If dtCostCenter.Rows.Count > 0 Then
                    If Len(FxDB(dtCostCenter.Rows(0)(0), "")) > 0 Then
                        vTransBarang = 0
                    End If
                End If
            End If

            If AsDataTableTambahData(dtdetail, "iddodetail~iddo~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~idhppkhususmasuk~idhppfifomasuk~harga~hpp~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudangasal~gudangtransit~gudangtujuan~rekpersediaan~rekhargapokok~rekdiskonpenjualan~costcenter~divisi~subdivisi~proyek~catatan~urutan~idsqdetail~idsodetail~idpidetail~idpldetail~jmldr~statusdr~jmlsi~statussi~jmlrnr~statusrnr~jmlsr~statussr~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~transbarang", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56) & "~" & dataRowDetail(57) & "~" & vTransBarang) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'Set variabel -----------------------------------------------
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , gudangasal(24) As String      , gudangtransit(25) As String
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : gudangOut = dataRowDetail(24) : gudangIn = dataRowDetail(25)
            'idsodetail(37) As Integer     , idpidetail(38) As Integer      , idpldetail(39) As Integer
            idsodetail = dataRowDetail(37) : idpidetail = dataRowDetail(38) : idpldetail = dataRowDetail(39)

            'ValidasiBatchSerial
            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")

            'BUAT FILTER UNTUK VALIDASI ---------------------------------

            Dim Stok As Double = 0

            'VALIDASI STOK #1, CEK STOK ADA ATAU TIDAK
            If vTransBarang = 1 Then
                ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")
                Stok = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangasal='" & gudangOut & "' AND transbarang = 1")
            End If
            
            'VALIDASI STOK DIBAGI MENJADI 2 JENIS, YAKNI :
            'VALIDASI STOK #1, JIKA TERKAIT DARI SO MAKA CEK STOK PERGUDANG (TOTAL STOK PERGUDANG), KEMUDIAN KURANGI JMLBOOKING
            'VALIDASI STOK #2, JIKA TIDAK TERKAIT DARI SO MAKA CEK STOK AVAILABLE PERGUDANG (TOTAL STOK PERGUDANG - STOK BOOKING)


            'VALIDASI OUTSTANDING -------------------------
            If idsodetail <> 0 Then 'SO

                'CEK SO YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftSO = IIf(Len(ftSO.ToString) = 0, "", ftSO & " OR ")
                ftSO = String.Concat(ftSO, " (sod.idsodetail = " & idsodetail & ") ")

                If idpidetail = 0 And idpldetail = 0 Then
                    '1. CEK DATA EXIST ------------------------
                    ftExistOutstandingSO = IIf(Len(ftExistOutstandingSO.ToString) = 0, "", ftExistOutstandingSO & " UNION ")
                    'ftExistOutstandingSO = String.Concat(ftExistOutstandingSO, "SELECT EXISTS(SELECT 1 FROM m5_so_detail JOIN m5_so ON idso = soid WHERE idsodetail = '" & idsodetail & "' AND (sostatus = 2 OR sostatus = 3 OR sostatus = 4 OR sostatus = 7) LIMIT 1) as rowExists, '" & idsodetail & "' as idsodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
                    'ftExistOutstandingSO = String.Concat(ftExistOutstandingSO, "SELECT EXISTS(SELECT 1 FROM m5_so_detail JOIN m5_so ON idso = soid WHERE idsodetail = '" & idsodetail & "' AND (sostatus = 2 OR sostatus = 3) LIMIT 1) as rowExists, '" & idsodetail & "' as idsodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
                    ftExistOutstandingSO = String.Concat(ftExistOutstandingSO, "SELECT EXISTS(SELECT 1 FROM m5_so_detail JOIN m5_so ON idso = soid WHERE idsodetail = '" & idsodetail & "' AND (sostatus = 2 OR sostatus = 3 OR sostatus = 4) LIMIT 1) as rowExists, '" & idsodetail & "' as idsodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                    '2. CEK JML OUTSTANDING -------------------
                    Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsodetail = " & idsodetail & " And idpidetail = 0 And idpldetail = 0")
                    ftOutstandingSO = IIf(Len(ftOutstandingSO.ToString) = 0, "", ftOutstandingSO & " OR ")
                    ftOutstandingSO = String.Concat(ftOutstandingSO, " (sod.idsodetail = " & idsodetail & " AND " & Outstanding & " > (sod.jmlbarang - sod.jmlrealisasi)) ")

                    '3. SET NILAI UPDATE OUTSTANDING ----------
                    updNilaiSO = String.Concat("WHEN '" & idsodetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiSO)

                    '4. SET FILTER UPDATE OUTSTANDING ---------
                    updFilterSO = IIf(Len(updFilterSO.ToString) = 0, "", updFilterSO & " OR ")
                    updFilterSO = String.Concat(updFilterSO, "(idsodetail = '" & idsodetail & "')")
                End If

                If vTransBarang = 1 Then
                    'VALIDASI STOK #1, JIKA TERKAIT DARI SO MAKA CEK STOK PERGUDANG (TOTAL STOK PERGUDANG)
                    'CEK JML STOK KELUAR
                    ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
                    ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > isw.stok) ")
                End If
                
                ''SET NILAI UPDATE STOK BOOKING (MENGURANGI)
                'updStokOutBooking = IIf(Len(updStokOutBooking.ToString) = 0, "", updStokOutBooking & ", ")
                'updStokOutBooking = String.Concat(updStokOutBooking, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, gudang, jmlbooking

            Else

                If vTransBarang = 1 Then
                    'VALIDASI STOK #2, JIKA TIDAK TERKAIT DARI SO MAKA CEK STOK AVAILABLE PERGUDANG (TOTAL STOK PERGUDANG - STOK BOOKING)
                    'CEK JML STOK KELUAR
                    ftStokAvailable = IIf(Len(ftStokAvailable.ToString) = 0, "", ftStokAvailable & " OR ")
                    ftStokAvailable = String.Concat(ftStokAvailable, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")
                End If
                
            End If

            If idpidetail <> 0 Then 'PI
                'CEK PI YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftPI = IIf(Len(ftPI.ToString) = 0, "", ftPI & " OR ")
                ftPI = String.Concat(ftPI, " (pid.idpidetail = " & idpidetail & ") ")

                '1. CEK DATA EXIST ------------------------
                ftExistOutstandingPI = IIf(Len(ftExistOutstandingPI.ToString) = 0, "", ftExistOutstandingPI & " UNION ")
                ftExistOutstandingPI = String.Concat(ftExistOutstandingPI, "SELECT EXISTS(SELECT 1 FROM m5_pi_detail JOIN m5_pi ON idpi = piid WHERE idpidetail = '" & idpidetail & "' AND (pistatus = 2 OR pistatus = 3 OR pistatus = 4 OR pistatus = 7) LIMIT 1) as rowExists, '" & idpidetail & "' as idpidetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
                'ftExistOutstandingPI = String.Concat(ftExistOutstandingPI, "SELECT EXISTS(SELECT 1 FROM m5_pi_detail JOIN m5_pi ON idpi = piid WHERE idpidetail = '" & idpidetail & "' AND (pistatus = 2 OR pistatus = 3) LIMIT 1) as rowExists, '" & idpidetail & "' as idpidetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idpidetail=" & idpidetail)
                ftOutstandingPI = IIf(Len(ftOutstandingPI.ToString) = 0, "", ftOutstandingPI & " OR ")
                ftOutstandingPI = String.Concat(ftOutstandingPI, " (pid.idpidetail = " & idpidetail & " AND " & Outstanding & " > (pid.jmlbarang - pid.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilaiPI = String.Concat("WHEN '" & idpidetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiPI)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilterPI = IIf(Len(updFilterPI.ToString) = 0, "", updFilterPI & " OR ")
                updFilterPI = String.Concat(updFilterPI, "(idpidetail = '" & idpidetail & "')")
            End If

            If idpldetail <> 0 Then 'PL
                'CEK PL YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftPL = IIf(Len(ftPL.ToString) = 0, "", ftPL & " OR ")
                ftPL = String.Concat(ftPL, " (pld.idpldetail = " & idpldetail & ") ")

                '1. CEK DATA EXIST ------------------------
                ftExistOutstandingPL = IIf(Len(ftExistOutstandingPL.ToString) = 0, "", ftExistOutstandingPL & " UNION ")
                ftExistOutstandingPL = String.Concat(ftExistOutstandingPL, "SELECT EXISTS(SELECT 1 FROM m5_pl_detail JOIN m5_pl ON idpl = plid WHERE idpldetail = '" & idpldetail & "' AND (plstatus = 2 OR plstatus = 3 OR plstatus = 4 OR plstatus = 7) LIMIT 1) as rowExists, '" & idpldetail & "' as idpldetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
                'ftExistOutstandingPL = String.Concat(ftExistOutstandingPL, "SELECT EXISTS(SELECT 1 FROM m5_pl_detail JOIN m5_pl ON idpl = plid WHERE idpldetail = '" & idpldetail & "' AND (plstatus = 2 OR plstatus = 3) LIMIT 1) as rowExists, '" & idpldetail & "' as idpldetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idpldetail=" & idpldetail)
                ftOutstandingPL = IIf(Len(ftOutstandingPL.ToString) = 0, "", ftOutstandingPL & " OR ")
                ftOutstandingPL = String.Concat(ftOutstandingPL, " (pld.idpldetail = " & idpldetail & " AND " & Outstanding & " > (pld.jmlbarang - pld.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilaiPL = String.Concat("WHEN '" & idpldetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiPL)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilterPL = IIf(Len(updFilterPL.ToString) = 0, "", updFilterPL & " OR ")
                updFilterPL = String.Concat(updFilterPL, "(idpldetail = '" & idpldetail & "')")
            End If

            If vTransBarang = 1 Then
                'SET NILAI UPDATE STOK -------------------------------
                'SET NILAI UPDATE STOK KELUAR
                updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
                updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

                'SET NILAI UPDATE STOK MASUK
                updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
                updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok

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

        'ValidasiSimpan
        Dim ftExistBatch As String = "", ftBatch As String = ""
        Dim nbtkode As String = "", nbtgudang As String = "", nbtidbatchin As Integer = 0
        Dim updNilaiBatch As String = "", updFilterBatch As String = ""

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
                dataRowBatch(1) = 0
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

                'ValidasiSimpan
                'BUAT FILTER UNTUK VALIDASI ---------------------------------
                'nbtidbarang(2) As Integer , nbtkode(3) As String      , nbtjml(7) As Double         , nbtgudang(17) As String      , nbtidbatchin(18) As Integer
                idbarang = dataRowBatch(2) : nbtkode = dataRowBatch(3) : jmlbarang = dataRowBatch(7) : nbtgudang = dataRowBatch(17) : nbtidbatchin = dataRowBatch(18)

                'VALIDASI BATCH -------------------------------
                '1. CEK DATA EXIST BATCH KELUAR 
                ftExistBatch = IIf(Len(ftExistBatch.ToString) = 0, "", ftExistBatch & " UNION ")
                ftExistBatch = String.Concat(ftExistBatch, "SELECT EXISTS(SELECT 1 FROM m1_no_batch_in WHERE nbiidbatchin = '" & nbtidbatchin & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & nbtkode & "' as nbikode, '" & nbtgudang & "' as nbigudang FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML BATCH KELUAR 
                Dim jmlKeluar As Double = AsDataTableDSum(dtbatch, "nbtjml", "nbtidbatchin = " & nbtidbatchin & "")
                ftBatch = IIf(Len(ftBatch.ToString) = 0, "", ftBatch & " OR ")
                ftBatch = String.Concat(ftBatch, " (nbi.nbiidbatchin = " & nbtidbatchin & " AND " & jmlKeluar & " > nbi.nbijmlsisa) ")

                '3. SET NILAI UPDATE BATCH IN 
                updNilaiBatch = String.Concat("WHEN nbiidbatchin = '" & nbtidbatchin & "' THEN ROUND(nbijmlkeluar + '" & jmlKeluar & "', 5) ", updNilaiBatch)

                '4. SET FILTER UPDATE BATCH IN 
                updFilterBatch = IIf(Len(updFilterBatch.ToString) = 0, "", updFilterBatch & " OR ")
                updFilterBatch = String.Concat(updFilterBatch, "(nbiidbatchin = '" & nbtidbatchin & "')")
                'END OF BUAT FILTER UNTUK VALIDASI --------------------------

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

        'ValidasiSimpan
        Dim ftExistSerial As String = "", ftSerial As String = ""
        Dim nstkode As String = "", nstgudang As String = "", nstidserialin As Integer = 0
        Dim updNilaiSerial As String = "", updFilterSerial As String = ""

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
                dataRowSerial(1) = 0
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

                'ValidasiSimpan
                'BUAT FILTER UNTUK VALIDASI ---------------------------------
                'nstidbarang(2) As Integer  , nstkode(3) As String       , nstjml(7) As Double          , nstgudang(17) As String       , nstidserialin(18) As Integer
                idbarang = dataRowSerial(2) : nstkode = dataRowSerial(3) : jmlbarang = dataRowSerial(7) : nstgudang = dataRowSerial(17) : nstidserialin = dataRowSerial(18)

                'VALIDASI SERIAL -------------------------------
                '1. CEK DATA EXIST SERIAL KELUAR
                ftExistSerial = IIf(Len(ftExistSerial.ToString) = 0, "", ftExistSerial & " UNION ")
                ftExistSerial = String.Concat(ftExistSerial, "SELECT EXISTS(SELECT 1 FROM m1_no_serial_in WHERE nsiidserialin = '" & nstidserialin & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & nstkode & "' as nsikode, '" & nstgudang & "' as nsigudang FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML SERIAL KELUAR 
                Dim jmlKeluar As Double = AsDataTableDSum(dtserial, "nstjml", "nstidserialin = " & nstidserialin & "")
                ftSerial = IIf(Len(ftSerial.ToString) = 0, "", ftSerial & " OR ")
                ftSerial = String.Concat(ftSerial, " (nsi.nsiidserialin = " & nstidserialin & " AND " & jmlKeluar & " > nsi.nsijmlsisa) ")

                '3. SET NILAI UPDATE SERIAL IN 
                updNilaiSerial = String.Concat("WHEN nsiidserialin = '" & nstidserialin & "' THEN ROUND(nsijmlkeluar + '" & jmlKeluar & "', 5) ", updNilaiSerial)

                '4. SET FILTER UPDATE SERIAL IN 
                updFilterSerial = IIf(Len(updFilterSerial.ToString) = 0, "", updFilterSerial & " OR ")
                updFilterSerial = String.Concat(updFilterSerial, "(nsiidserialin = '" & nstidserialin & "')")
                'END OF BUAT FILTER UNTUK VALIDASI --------------------------

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
                    dataRowAsset(2) = 0
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
                vStatus = drutama("dostatus")
                vTgl = AsFormatTanggal(drutama("dotgl"))

                'CEK HAK AKSES STATUS ============================
                Dim vAkses As Integer = 0, msgAkses As String = ""
                'MODUL DAN MENU HARUS DISESUAIKAN
                Dim vModuleId As Integer = 5, vMenuId As Integer = 7
                Select Case drutama("dostatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("dotgl")), AsFormatTanggal(drutama("dotgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                If drutama("dostatus") = 2 Or drutama("dostatus") = 1 Or drutama("dostatus") = 8 Or drutama("dostatus") = 9 Or drutama("dostatus") = 10 Or drutama("dostatus") = 11 Then

                    'VALIDASI BATCH SERIAL ---------------
                    'ValidasiBatchSerial
                    Dim rsValidasi As String = ValidasiBatchSerial(dtdetail, dtbatch, dtserial, ftBarang, "jmlbarang", 0)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    'END OF VALIDASI BATCH SERIAL --------

                    'VALIDASI ASSET ----------------------
                    'ValidasiAsset
                    rsValidasi = ValidasiAsset(dtdetail, dtasset, ftBarang, "jmlbarang", 0)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    'END OF VALIDASI ASSET ---------------

                    'VALIDASI GUDANG ASSET ---------------
                    'ValidasiGudangAsset
                    rsValidasi = ValidasiGudangAsset(dtasset, gudangOut)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai

                    'ValidasiSimpan
                    rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstandingSO, ftOutstandingSO, ftExistOutstandingPI, ftOutstandingPI, ftExistOutstandingPL, ftOutstandingPL, ftExistStok, ftStok, ftStokAvailable, ftExistBatch, ftBatch, ftExistSerial, ftSerial, "gudangasal", ftSO, ftPI, ftPL, drutama("dohargatermasukpajak"))
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("dotermin").ToString, AsFormatTanggal(drutama("dotgl")), "dotgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("dotgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                ''PERHITUNGAN TOTAL UTAMA ================================
                ''DIAMBILKAN DARI DATA DETAIL

                ''TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                ''SUBTOTAL = (jml * harga) - jmldiskon
                'AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                'dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                ''TOTAL = subtotal
                'drutama("dototal") = AsDataTableDSum(dtdetail, "subtotal")

                ''TOTALPAJAK1 = jmlpajak1
                'drutama("dototalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                ''TOTALPAJAK2 = jmlpajak2
                'drutama("dototalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                ''JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                ''JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'If Integer.Parse(drutama("dohargatermasukpajak")) = 0 Then
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                '    drutama("dototaltransaksi") = Double.Parse(drutama("dototal")) - Double.Parse(drutama("dojmldiskon")) + Double.Parse(drutama("dototalpajak1detail")) + Double.Parse(drutama("dototalpajak2detail")) + Double.Parse(drutama("dobiayalain"))

                'Else
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                '    drutama("dototaltransaksi") = Double.Parse(drutama("dototal")) - Double.Parse(drutama("dojmldiskon")) + Double.Parse(drutama("dobiayalain"))

                'End If
                ''END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("doid")
                    notransaksi = drutama("donotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(doid), donotransaksi FROM M5_do WHERE doid='" & result(4) & "' AND dostatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("doautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("docabang"), drutama("dolokasi"), drutama("dosumber"), drutama("dotgl"), drutama("dosumber"), 5)
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(doid) FROM m5_do WHERE donotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_do_history
                        Dim rsSimpanHistory As String = SimpanHistory.m5_Do_HistorySimpan("" & paramSplit(0) & "★M5_Do_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("dosumber")) & "▼" & FixQuotes(drutama("doid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Do set docabang  = '" & FixQuotes(drutama("docabang")) & "', dolokasi  = '" & FixQuotes(drutama("dolokasi")) & "', dogudang  = '" & FixQuotes(drutama("dogudang")) & "', doasalbarang  = '" & FixQuotes(drutama("doasalbarang")) & "', doasalbarangkategori  = " & drutama("doasalbarangkategori") & ", dojenispenjualan  = '" & FixQuotes(drutama("dojenispenjualan")) & "', dojenispenjualankategori  = " & drutama("dojenispenjualankategori") & ", docarabayar  = " & drutama("docarabayar") & ", dosumber  = '" & FixQuotes(drutama("dosumber")) & "', doautonotransaksi  = " & drutama("doautonotransaksi") & ", donotransaksi  = '" & FixQuotes(notransaksi) & "', dotgl  = '" & FixQuotes(AsFormatTanggal(drutama("dotgl"))) & "', dokodepa  = " & drutama("dokodepa") & ", docustomer  = " & drutama("docustomer") & ", docustomerkontak  = '" & FixQuotes(drutama("docustomerkontak")) & "', do1alamat1  = '" & FixQuotes(drutama("do1alamat1")) & "', do1alamat2  = '" & FixQuotes(drutama("do1alamat2")) & "', do1alamat3  = '" & FixQuotes(drutama("do1alamat3")) & "', do2alamat1  = '" & FixQuotes(drutama("do2alamat1")) & "', do2alamat2  = '" & FixQuotes(drutama("do2alamat2")) & "', do2alamat3  = '" & FixQuotes(drutama("do2alamat3")) & "', dobagianpenjualan  = " & drutama("dobagianpenjualan") & ", dobagianpengiriman  = " & drutama("dobagianpengiriman") & ", doekspedisi  = '" & FixQuotes(drutama("doekspedisi")) & "', dotglkirim  = '" & FixQuotes(AsFormatTanggal(drutama("dotglkirim"))) & "', dotermin  = '" & FixQuotes(drutama("dotermin")) & "', dotgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("dotgljatuhtempo"))) & "', douraian  = '" & FixQuotes(drutama("douraian")) & "', docatatan  = '" & FixQuotes(drutama("docatatan")) & "', donoref  = '" & FixQuotes(drutama("donoref")) & "', dotglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("dotglnoref"))) & "', dotglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("dotglpenutupan"))) & "', domatauang  = '" & FixQuotes(drutama("domatauang")) & "', dokurs  = '" & FixDouble(drutama("dokurs")) & "', dohargatermasukpajak  = " & drutama("dohargatermasukpajak") & ", dototal  = '" & FixDouble(drutama("dototal")) & "', dodiskonpersen  = '" & FixQuotes(drutama("dodiskonpersen")) & "', dojmldiskon  = '" & FixDouble(drutama("dojmldiskon")) & "', dototalpajak1detail  = '" & FixDouble(drutama("dototalpajak1detail")) & "', dototalpajak2detail  = '" & FixDouble(drutama("dototalpajak2detail")) & "', dobiayalainpersen  = '" & FixDouble(drutama("dobiayalainpersen")) & "', dobiayalain  = '" & FixDouble(drutama("dobiayalain")) & "', dototaltransaksi  = '" & FixDouble(drutama("dototaltransaksi")) & "', dorekdiskon  = '" & FixQuotes(drutama("dorekdiskon")) & "', dorekpajak1  = '" & FixQuotes(drutama("dorekpajak1")) & "', dorekpajak2  = '" & FixQuotes(drutama("dorekpajak2")) & "', dorekbiayalain  = '" & FixQuotes(drutama("dorekbiayalain")) & "', doidsq  = " & drutama("doidsq") & ", doidso  = " & drutama("doidso") & ", doidpi  = " & drutama("doidpi") & ", doidpl  = " & drutama("doidpl") & ", dostatusdr  = " & drutama("dostatusdr") & ", dostatussi  = " & drutama("dostatussi") & ", dostatusrnr  = " & drutama("dostatusrnr") & ", dostatussr  = " & drutama("dostatussr") & ", dostatus  = " & drutama("dostatus") & ", dostatussebelumnya  = " & drutama("dostatussebelumnya") & ", dojmlrevisi  = dojmlrevisi+1, docetakanke  = " & drutama("docetakanke") & ", domodifikasiuser  = " & drutama("domodifikasiuser") & ", domodifikasitgl  = NOW(), doposting  = 0, dotutupperiode  = " & drutama("dotutupperiode") & ", docustomtext1  = '" & FixQuotes(drutama("docustomtext1")) & "', docustomtext2  = '" & FixQuotes(drutama("docustomtext2")) & "', docustomtext3  = '" & FixQuotes(drutama("docustomtext3")) & "', docustomtext4  = '" & FixQuotes(drutama("docustomtext4")) & "', docustomtext5  = '" & FixQuotes(drutama("docustomtext5")) & "', docustomint1  = " & drutama("docustomint1") & ", docustomint2  = " & drutama("docustomint2") & ", docustomint3  = " & drutama("docustomint3") & ", docustomdbl1  = '" & FixDouble(drutama("docustomdbl1")) & "', docustomdbl2  = '" & FixDouble(drutama("docustomdbl2")) & "', docustomdbl3  = '" & FixDouble(drutama("docustomdbl3")) & "', docustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("docustomdate1"))) & "', docustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("docustomdate2"))) & "', docustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("docustomdate3"))) & "' where doid = '" & drutama("doid") & "'"
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

                    If drutama("doautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("docabang"), drutama("dolokasi"), drutama("dosumber"), drutama("dotgl"), drutama("dosumber"), 5)
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
                        notransaksi = drutama("donotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(doid) FROM m5_do WHERE donotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Do (docabang, dolokasi, dogudang, doasalbarang, doasalbarangkategori, dojenispenjualan, dojenispenjualankategori, docarabayar, dosumber, doautonotransaksi, donotransaksi, dotgl, dokodepa, docustomer, docustomerkontak, do1alamat1, do1alamat2, do1alamat3, do2alamat1, do2alamat2, do2alamat3, dobagianpenjualan, dobagianpengiriman, doekspedisi, dotglkirim, dotermin, dotgljatuhtempo, douraian, docatatan, donoref, dotglnoref, dotglpenutupan, domatauang, dokurs, dohargatermasukpajak, dototal, dodiskonpersen, dojmldiskon, dototalpajak1detail, dototalpajak2detail, dobiayalainpersen, dobiayalain, dototaltransaksi, dorekdiskon, dorekpajak1, dorekpajak2, dorekbiayalain, doidsq, doidso, doidpi, doidpl, dostatusdr, dostatussi, dostatusrnr, dostatussr, dostatus, dostatussebelumnya, dojmlrevisi, docetakanke, doinputuser, doinputtgl, domodifikasiuser, domodifikasitgl, doposting, dotutupperiode, doisclose, docustomtext1, docustomtext2, docustomtext3, docustomtext4, docustomtext5, docustomint1, docustomint2, docustomint3, docustomdbl1, docustomdbl2, docustomdbl3, docustomdate1, docustomdate2, docustomdate3) values('" & FixQuotes(drutama("docabang")) & "', '" & FixQuotes(drutama("dolokasi")) & "', '" & FixQuotes(drutama("dogudang")) & "', '" & FixQuotes(drutama("doasalbarang")) & "', " & drutama("doasalbarangkategori") & ", '" & FixQuotes(drutama("dojenispenjualan")) & "', " & drutama("dojenispenjualankategori") & ", " & drutama("docarabayar") & ", '" & FixQuotes(drutama("dosumber")) & "', " & drutama("doautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("dotgl"))) & "', " & drutama("dokodepa") & ", " & drutama("docustomer") & ", '" & FixQuotes(drutama("docustomerkontak")) & "', '" & FixQuotes(drutama("do1alamat1")) & "', '" & FixQuotes(drutama("do1alamat2")) & "', '" & FixQuotes(drutama("do1alamat3")) & "', '" & FixQuotes(drutama("do2alamat1")) & "', '" & FixQuotes(drutama("do2alamat2")) & "', '" & FixQuotes(drutama("do2alamat3")) & "', " & drutama("dobagianpenjualan") & ", " & drutama("dobagianpengiriman") & ", '" & FixQuotes(drutama("doekspedisi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("dotglkirim"))) & "', '" & FixQuotes(drutama("dotermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("dotgljatuhtempo"))) & "', '" & FixQuotes(drutama("douraian")) & "', '" & FixQuotes(drutama("docatatan")) & "', '" & FixQuotes(drutama("donoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("dotglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("dotglpenutupan"))) & "', '" & FixQuotes(drutama("domatauang")) & "', '" & FixDouble(drutama("dokurs")) & "', " & drutama("dohargatermasukpajak") & ", '" & FixDouble(drutama("dototal")) & "', '" & FixQuotes(drutama("dodiskonpersen")) & "', '" & FixDouble(drutama("dojmldiskon")) & "', '" & FixDouble(drutama("dototalpajak1detail")) & "', '" & FixDouble(drutama("dototalpajak2detail")) & "', '" & FixDouble(drutama("dobiayalainpersen")) & "', '" & FixDouble(drutama("dobiayalain")) & "', '" & FixDouble(drutama("dototaltransaksi")) & "', '" & FixQuotes(drutama("dorekdiskon")) & "', '" & FixQuotes(drutama("dorekpajak1")) & "', '" & FixQuotes(drutama("dorekpajak2")) & "', '" & FixQuotes(drutama("dorekbiayalain")) & "', " & drutama("doidsq") & ", " & drutama("doidso") & ", " & drutama("doidpi") & ", " & drutama("doidpl") & ", " & drutama("dostatusdr") & ", " & drutama("dostatussi") & ", " & drutama("dostatusrnr") & ", " & drutama("dostatussr") & ", " & drutama("dostatus") & ", " & drutama("dostatussebelumnya") & ", " & drutama("dojmlrevisi") & ", " & drutama("docetakanke") & ", " & drutama("doinputuser") & ", NOW(), " & drutama("domodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("dotutupperiode") & ", " & drutama("doisclose") & ", '" & FixQuotes(drutama("docustomtext1")) & "', '" & FixQuotes(drutama("docustomtext2")) & "', '" & FixQuotes(drutama("docustomtext3")) & "', '" & FixQuotes(drutama("docustomtext4")) & "', '" & FixQuotes(drutama("docustomtext5")) & "', " & drutama("docustomint1") & ", " & drutama("docustomint2") & ", " & drutama("docustomint3") & ", '" & FixDouble(drutama("docustomdbl1")) & "', '" & FixDouble(drutama("docustomdbl2")) & "', '" & FixDouble(drutama("docustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("docustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("docustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("docustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select doid from M5_do where donotransaksi='" & notransaksi & "' AND doinputuser= '" & userid & "' order by domodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Do_Detail where iddo = '" & result(4) & "'"
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
                        If Not drutama("domatauang").ToString.Equals(dr1("matauang").ToString) Then
                            result(2) = "Row : " & dr1("urutan") & " - " & dr1("tipebarang") & " | " & dr1("namabarang") & " currency (" & dr1("matauang") & ") doesn't belong to the main transactions." : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF VALIDASI MATAUANG HARUS SAMA DENGAN TRANSAKSI UTAMA -------------


                        'SET HARGA DARI TRANSAKSI SEBELUMNYA ------------------------------------
                        If Double.Parse(dr1("idpldetail")) > 0 Then
                            'JIKA AMBIL PL MAKA SET HARGA DARI PL
                            sql = "SELECT jml, harga, diskon, pajak1, jmlpajak1, pajak2, jmlpajak2 FROM m5_pl_detail WHERE idpldetail = '" & FixDouble(dr1("idpldetail")) & "'"

                        ElseIf Double.Parse(dr1("idpidetail")) > 0 Then
                            'JIKA AMBIL PI MAKA SET HARGA DARI PI
                            sql = "SELECT jml, harga, diskon, pajak1, jmlpajak1, pajak2, jmlpajak2 FROM m5_pi_detail WHERE idpidetail = '" & FixDouble(dr1("idpidetail")) & "'"

                        ElseIf Double.Parse(dr1("idsodetail")) > 0 Then
                            'JIKA AMBIL SO MAKA SET HARGA DARI SO
                            sql = "SELECT jml, harga, diskon, pajak1, jmlpajak1, pajak2, jmlpajak2 FROM m5_so_detail WHERE idsodetail = '" & FixDouble(dr1("idsodetail")) & "'"

                        Else
                            sql = ""
                        End If

                        dtBefore = AsDataTableAmbilDariDBCon(sql, myConn)
                        If dtBefore.Rows.Count > 0 Then
                            'SET HARGA - ambil dari transaksi sebelumnya
                            dr1("harga") = Double.Parse(dtBefore.Rows(0)("harga"))

                            'SET DISKON - ambil dari transaksi sebelumnya
                            dr1("diskon") = dtBefore.Rows(0)("diskon")

                            'SET JMLDISKON - hitung diskon
                            dr1("jmldiskon") = F_Diskon(Double.Parse(dr1("jml")), Double.Parse(dr1("harga")), FixQuotes(dr1("diskon").ToString))

                            'SET PAJAK1 - ambil dari transaksi sebelumnya
                            dr1("pajak1") = dtBefore.Rows(0)("pajak1")

                            'SET JMLPAJAK1 - ambil dari transaksi sebelumnya = (jmlpajakbefore / jmlbefore) * jml
                            dr1("jmlpajak1") = (Double.Parse(dtBefore.Rows(0)("jmlpajak1")) / Double.Parse(dtBefore.Rows(0)("jml"))) * Double.Parse(dr1("jml"))

                            'SET PAJAK2 - ambil dari transaksi sebelumnya
                            dr1("pajak2") = dtBefore.Rows(0)("pajak2")

                            'SET JMLPAJAK2 - ambil dari transaksi sebelumnya = (jmlpajakbefore / jmlbefore) * jml
                            dr1("jmlpajak2") = (Double.Parse(dtBefore.Rows(0)("jmlpajak2")) / Double.Parse(dtBefore.Rows(0)("jml"))) * Double.Parse(dr1("jml"))
                        End If
                        'END OF SET HARGA DARI TRANSAKSI SEBELUMNYA -----------------------------


                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("iddodetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', " & dr1("idhppkhususmasuk") & ", " & dr1("idhppfifomasuk") & ", '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hpp")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("rekhargapokok")) & "', '" & FixQuotes(dr1("rekdiskonpenjualan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idsqdetail") & ", " & dr1("idsodetail") & ", " & dr1("idpidetail") & ", " & dr1("idpldetail") & ", '" & FixDouble(dr1("jmldr")) & "', " & dr1("statusdr") & ", '" & FixDouble(dr1("jmlsi")) & "', " & dr1("statussi") & ", '" & FixDouble(dr1("jmlrnr")) & "', " & dr1("statusrnr") & ", '" & FixDouble(dr1("jmlsr")) & "', " & dr1("statussr") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M5_Do_Detail(iddodetail, iddo, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, idhppkhususmasuk, idhppfifomasuk, harga, hpp, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekhargapokok, rekdiskonpenjualan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpidetail, idpldetail, jmldr, statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                    sql = "Delete from M1_No_Batch_Transaction where nbtidtransaksi  = '" & result(4) & "' AND nbtsumber = 'DO'"
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
                    sql = "Delete from M1_No_Serial_Transaction  where nstidtransaksi  = '" & result(4) & "' AND nstsumber = 'DO'"
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
                    sql = "Delete from M7_Asset_Transaction where atidutama = '" & result(4) & "' AND atsumber = 'DO'"
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


                If drutama("dostatus") = 2 Then
                    'UPDATE OUTSTANDING TRANSAKSI ======================================================
                    If Len(updNilaiSO) > 0 Then 'SO
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
                    End If

                    If Len(updNilaiPI) > 0 Then 'PI
                        'UPDATE DETAIL
                        sql = "UPDATE m5_pi_detail SET jmlrealisasi = (CASE idpidetail " & updNilaiPI & " ELSE jmlrealisasi END) WHERE " & updFilterPI
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idpi FROM m5_pi_detail WHERE " & updFilterPI & " GROUP BY idpi", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idpi = '" & dr1("idpi") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idpi, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_pi_detail WHERE " & ftDetail & " GROUP BY idpi", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiPI = "" : updFilterPI = ""
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
                                updNilaiPI = String.Concat(updNilaiPI, "WHEN '" & dr1("idpi") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterPI = IIf(Len(updFilterPI.ToString) = 0, "", updFilterPI & " OR ")
                                updFilterPI = String.Concat(updFilterPI, "(piid = '" & dr1("idpi") & "')")
                            Next

                            sql = "UPDATE m5_pi SET pistatusrealisasi = (CASE piid " & updNilaiPI & " ELSE pistatusrealisasi END) WHERE " & updFilterPI
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

                    If Len(updNilaiPL) > 0 Then 'PL
                        'UPDATE DETAIL
                        sql = "UPDATE m5_pl_detail SET jmlrealisasi = (CASE idpldetail " & updNilaiPL & " ELSE jmlrealisasi END) WHERE " & updFilterPL
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idpl FROM m5_pl_detail WHERE " & updFilterPL & " GROUP BY idpl", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idpl = '" & dr1("idpl") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idpl, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_pl_detail WHERE " & ftDetail & " GROUP BY idpl", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiPL = "" : updFilterPL = ""
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
                                updNilaiPL = String.Concat(updNilaiPL, "WHEN '" & dr1("idpl") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterPL = IIf(Len(updFilterPL.ToString) = 0, "", updFilterPL & " OR ")
                                updFilterPL = String.Concat(updFilterPL, "(plid = '" & dr1("idpl") & "')")
                            Next

                            sql = "UPDATE m5_pl SET plstatusrealisasi = (CASE plid " & updNilaiPL & " ELSE plstatusrealisasi END) WHERE " & updFilterPL
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


                    'AMBIL GUDANG TRANSIT DARI SETTING ==============================================
                    Dim SetGudang As String = ""
                    'GUDANG SETTING TRANSIT DIGUNAKAN UNTUK NO SERIAL DAN BATCH MASUK
                    'MISAL : GUDANG ASAL 'A', MAKA :
                    '-- NO SERIAL DAN BATCH GUDANG 'A' BERKURANG
                    '-- NO SERIAL DAN BATCH GUDANG TRANSIT BERTAMBAH
                    sql = "SELECT snilai FROM m0_setting WHERE smodule = 3 AND sgrup = 'defaultgudang' AND skode = 'GudangTransit'"
                    Dim dtSetGudang As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtSetGudang.Rows.Count > 0 Then
                        SetGudang = dtSetGudang.Rows(0)("snilai")
                    Else
                        result(2) = "Setting for Transit Warehouse not found." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF AMBIL GUDANG TRANSIT DARI SETTING =======================================


                    'INSERT NO BATCH ================================================================
                    If dtbatch.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder, strValue3 As New StringBuilder
                        For Each dr1 As DataRow In dtbatch.Rows
                            'QUERY INSERT NO BATCH OUT
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping             nboid,            nboidbatchin,                           nbogudang,                  nboidbarang,                           nbokode,                             nbosumber,            nboidtransaksi,                     nbosatuan,                         nbojmlkeluar,       nboisclose
                            strValue2.Append("(" & 0 & ", " & dr1("nbtidbatchin") & ", '" & FixQuotes(dr1("nbtgudang")) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', " & 0 & ")")

                            'QUERY INSERT NO BATCH IN
                            strValue3.Append(IIf(Len(strValue3.ToString) = 0, "", ", "))
                            'mapping        nbiidbatchin,                nbigudang,                nbiidbarang,                           nbikode,                             nbisumber,            nbiidtransaksi,                     nbisatuan,                 nbijmlmasuk,       nbijmlkeluar,                  nbijmlsisa, nbiisclose,                     nbicustomtext1,                             nbicustomtext2,                             nbicustomtext3,                             nbicustomdbl1,                             nbicustomdbl2,                             nbicustomdbl3,                                             nbicustomdate1,                                              nbicustomdate2,                                              nbicustomdate3
                            strValue3.Append("(" & 0 & ", '" & FixQuotes(SetGudang) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixQuotes(dr1("nbtcustomtext1")) & "', '" & FixQuotes(dr1("nbtcustomtext2")) & "', '" & FixQuotes(dr1("nbtcustomtext3")) & "', '" & FixDouble(dr1("nbtcustomdbl1")) & "', '" & FixDouble(dr1("nbtcustomdbl2")) & "', '" & FixDouble(dr1("nbtcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate3"))) & "')")
                        Next

                        'INSERT NO BATCH OUT ---------------------------------
                        sql = "Insert into M1_No_Batch_Out(nboid, nboidbatchin, nbogudang, nboidbarang, nbokode, nbosumber, nboidtransaksi, nbosatuan, nbojmlkeluar, nboisclose) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE NO BATCH IN KELUAR ---------------------------
                        If Len(updNilaiBatch) > 0 Then
                            sql = "UPDATE m1_no_batch_in SET nbijmlkeluar =  (CASE " & updNilaiBatch & " ELSE nbijmlkeluar END) WHERE " & updFilterBatch
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If

                        'INSERT NO BATCH IN MASUK ----------------------------
                        sql = "Insert into M1_No_Batch_In(nbiidbatchin, nbigudang, nbiidbarang, nbikode, nbisumber, nbiidtransaksi, nbisatuan, nbijmlmasuk, nbijmlkeluar, nbijmlsisa, nbiisclose, nbicustomtext1, nbicustomtext2, nbicustomtext3, nbicustomdbl1, nbicustomdbl2, nbicustomdbl3, nbicustomdate1, nbicustomdate2, nbicustomdate3) values" & strValue3.ToString & ""
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
                        Dim strValue2 As New StringBuilder, strValue3 As New StringBuilder
                        For Each dr1 As DataRow In dtserial.Rows
                            'QUERY INSERT NO SERIAL OUT
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping            nsoid,             nsoidserialin,                           nsogudang,                  nsoidbarang,                           nsokode,                             nsosumber,            nsoidtransaksi,                     nsosatuan,                          nsojmlkeluar,      nsoisclose
                            strValue2.Append("(" & 0 & ", " & dr1("nstidserialin") & ", '" & FixQuotes(dr1("nstgudang")) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', " & 0 & ")")

                            'QUERY INSERT NO SERIAL IN
                            strValue3.Append(IIf(Len(strValue3.ToString) = 0, "", ", "))
                            'mapping       nsiidserialin,                nsigudang,                nsiidbarang,                           nsikode,                             nsisumber,            nsiidtransaksi,                     nsisatuan,                       nsijmlmasuk, nsijmlkeluar,                  nsijmlsisa, nsiisclose,                     nsicustomtext1,                             nsicustomtext2,                             nsicustomtext3,                             nsicustomdbl1,                             nsicustomdbl2,                             nsicustomdbl3,                                             nsicustomdate1,                                              nsicustomdate2,                                              nsicustomdate3
                            strValue3.Append("(" & 0 & ", '" & FixQuotes(SetGudang) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixQuotes(dr1("nstcustomtext1")) & "', '" & FixQuotes(dr1("nstcustomtext2")) & "', '" & FixQuotes(dr1("nstcustomtext3")) & "', '" & FixDouble(dr1("nstcustomdbl1")) & "', '" & FixDouble(dr1("nstcustomdbl2")) & "', '" & FixDouble(dr1("nstcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate3"))) & "')")
                        Next

                        'INSERT NO SERIAL OUT --------------------------------
                        sql = "Insert into M1_No_Serial_Out(nsoid, nsoidserialin, nsogudang, nsoidbarang, nsokode, nsosumber, nsoidtransaksi, nsosatuan, nsojmlkeluar, nsoisclose) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE NO SERIAL IN KELUAR --------------------------
                        If Len(updNilaiSerial) > 0 Then
                            sql = "UPDATE m1_no_serial_in SET nsijmlkeluar =  (CASE " & updNilaiSerial & " ELSE nsijmlkeluar END) WHERE " & updFilterSerial
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If

                        'INSERT NO SERIAL IN MASUK ---------------------------
                        sql = "Insert into M1_No_Serial_In(nsiidserialin, nsigudang, nsiidbarang, nsikode, nsisumber, nsiidtransaksi, nsisatuan, nsijmlmasuk, nsijmlkeluar, nsijmlsisa, nsiisclose, nsicustomtext1, nsicustomtext2, nsicustomtext3, nsicustomdbl1, nsicustomdbl2, nsicustomdbl3, nsicustomdate1, nsicustomdate2, nsicustomdate3) values" & strValue3.ToString & ""
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
                            strValue2.Append(FixDouble(dr1("atasetid")))
                        Next
                        sql = "UPDATE m7_asset a SET a.agudang = '" & SetGudang & "' WHERE a.aid IN(" & strValue2.ToString & ")"
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
                    'MENGURANGI BOOKING HANYA UNTUK BARANG YG HPP NYA BUKAN KHUSUS (I) DAN TERKAIT DARI SO
                    sql = "INSERT INTO m1_item_booking (SELECT idbarang, gudangasal, jmlbarang * -1 FROM m5_do_detail JOIN m1_item ON idbarang = bid AND bjenis <> 'J' AND bhpp <> 'I' AND idsodetail <> 0 AND iddo = '" & result(4) & "') ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'If Len(updStokOutBooking) > 0 Then
                    '    sql = "INSERT INTO m1_item_booking (idbarang, gudang, jmlbooking) VALUES " & updStokOutBooking & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                    '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    '    With objCmd
                    '        .Connection = myconn
                    '        .Transaction = Trans
                    '        .CommandType = CommandType.Text
                    '        .CommandText = sql
                    '    End With
                    '    objCmd.ExecuteNonQuery()
                    'End If
                    'END OF UPDATE STOK BOOKING =====================================================


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

                    'STOK MASUK
                    If Len(updStokIn) > 0 Then
                        sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokIn & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
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


                    'COMPLETE COST CENTER
                    sql = "UPDATE m5_do_detail dod JOIN m1_cost_center cc ON dod.costcenter = cc.cckode JOIN m0_setting s2 ON s2.smodule = 0 AND s2.sgrup = 'options' AND s2.skode = 'DONonaktifCostcenter' AND s2.snilai = 1 SET cc.ccaktif = 0 WHERE dod.iddo = '" & result(4) & "';"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()


                    'INSERT ITEM TRANSACTION ========================================================
                    'AMBIL DATA DETAIL YANG BARU
                    'sql = "SELECT dod.iddodetail, dod.idbarang, dod.namabarang, dod.tipebarang, dod.jml, dod.satuan, dod.jmlbarang, dod.satuanbarang, dod.matauang, dod.kurs, dod.harga, dod.diskon, dod.jmldiskon, dod.hpp, dod.idhppkhususmasuk, dod.gudangasal, dod.gudangtransit, dod.gudangtujuan, dod.catatan, dod.costcenter, dod.divisi, dod.subdivisi, dod.proyek, `do`.doinputtgl, i.bhpp FROM m5_do_detail dod JOIN m5_do `do` ON dod.iddo = `do`.doid JOIN m1_item i ON dod.idbarang = i.bid WHERE dod.iddo = '" & result(4) & "'"
                    sql = "SELECT dod.iddodetail, dod.idbarang, dod.namabarang, dod.tipebarang, dod.jml, dod.satuan, dod.jmlbarang, dod.satuanbarang, dod.matauang, dod.kurs, dod.harga, dod.diskon, dod.jmldiskon, dod.hpp, dod.idhppkhususmasuk, dod.gudangasal, dod.gudangtransit, dod.gudangtujuan, dod.catatan, dod.costcenter, dod.divisi, dod.subdivisi, dod.proyek, `do`.doinputtgl, i.bhpp, (CASE LENGTH(IFNULL(cc.ccakun,'')) WHEN 0 THEN 1 ELSE 0 END) as transbarang FROM m5_do_detail dod JOIN m5_do `do` ON dod.iddo = `do`.doid JOIN m1_item i ON dod.idbarang = i.bid LEFT JOIN m1_cost_center cc ON dod.costcenter = cc.cckode WHERE dod.iddo = '" & result(4) & "'"
                    Dim dtDetailNew As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                    Dim hpp As Double = 0, jenismutasi As Double = 0, postinghpp As Double = 0
                    Dim strTransaksiBarang As New StringBuilder

                    If dtDetailNew.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtDetailNew.Rows
                            If Double.Parse(dr1("transbarang")) = 1 Then
                                'jenismutasi dan postinghpp 
                                '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 1
                                '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                '- untuk transaksi mutasi saja maka postinghpp = 0
                                postinghpp = 0

                                'hitung hpp = hpp
                                hpp = Double.Parse(dr1("hpp"))

                                'POSTING BARANG KELUAR (gudangasal)
                                jenismutasi = 0
                                'QUERY INSERT TRANSAKSI BARANG KELUAR
                                strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                                'mapping                        id,                             cabang,                                   lokasi,                                gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,             iddetail,                    notransaksi,                                                 tgl,                              kontak,               idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                  idhppikm,                idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("docabang")) & "', '" & FixQuotes(drutama("dolokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', " & drutama("dokodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("dosumber")) & "', " & result(4) & ", " & dr1("iddodetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("dotgl"))) & "', " & drutama("docustomer") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("douraian")) & "', '" & FixQuotes(drutama("docatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("doinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("doinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")

                                'POSTING BARANG MASUK (gudangtransit)
                                jenismutasi = 1
                                'QUERY INSERT TRANSAKSI BARANG MASUK
                                strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                                'mapping                        id,                             cabang,                                   lokasi,                                   gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,             iddetail,                    notransaksi,                                                 tgl,                              kontak,               idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                  idhppikm,                idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("docabang")) & "', '" & FixQuotes(drutama("dolokasi")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', " & drutama("dokodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("dosumber")) & "', " & result(4) & ", " & dr1("iddodetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("dotgl"))) & "', " & drutama("docustomer") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("douraian")) & "', '" & FixQuotes(drutama("docatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("doinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("doinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                            End If
                        Next

                        If Len(strTransaksiBarang.ToString) > 0 Then
                            sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
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
                        result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF INSERT ITEM TRANSACTION =================================================

                End If


                ''INSERT MSMQ HPP ====================================================================
                Dim sumber As String = "DO", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                'If drutama("dostatus") = 2 Then
                '    Dim Security As New ClsSecurity, mjid As String = "", hasilMsmq As String = ""
                '    'BUAT ID UNIQUE
                '    mjid = Security.MD5CalcString("C" & userid & sumber & result(4) & Now) 'RandomId.Generate(15)

                '    'MSMQ TABEL
                '    sql = "Insert into M0_Msmq_Cogs(mcid, mcsumber, mcidtransaksi, mcprogress, mcpesan, mctglantrian, mctglselesai, mcuserid) values ('" _
                '        & mjid & "', '" & sumber & "', '" & result(4) & "', '" & 0 & "', " & "''" & ", NOW(), '1971-01-01 00:00:00', '" & userid & "')"
                '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '    With objCmd
                '        .Connection = myConn
                '        .Transaction = Trans
                '        .CommandType = CommandType.Text
                '        .CommandText = sql
                '    End With
                '    objCmd.ExecuteNonQuery()

                '    'MSMQ ANTRIAN
                '    Dim ProsesHpp As String = F_getSetting(0, "accounting", "ProsesHpp")
                '    If ProsesHpp.Equals("0") = False Then
                '        hasilMsmq = SendMsmq(dirMsmq, "C", mjid, sumber, result(4), userid)
                '        If Len(hasilMsmq) > 0 Then
                '            result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
                '        End If
                '    End If

                'End If
                'END OF INSERT MSMQ HPP =============================================================


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
    Public Function M5_DoUpdateStatus(ByVal param As String) As String

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
        Dim dtdetail As DataTable, dtasset As DataTable
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
            Dim sumber As String = "DO", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Dotgl, Donotransaksi, Dostatus FROM M5_Do WHERE Doid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Dostatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m5_do_history
            Dim rsSimpanHistory As String = SimpanHistory.m5_Do_HistorySimpan("" & paramSplit(0) & "★M5_Do_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m5_do_terkait("doid = '" & idtransaksi & "'")
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


                Dim idbarang As Integer = 0, idsodetail As Integer = 0, idpidetail As Integer = 0, idpldetail As Integer = 0
                Dim idhppkhususmasuk As Integer = 0, jmlbarang As Double = 0
                Dim ftExistStok As String = "", ftStok As String = ""
                Dim gudangOut As String = "", updStokOut As String = ""
                Dim gudangIn As String = "", updStokIn As String = "", updStokInBooking As String = ""
                Dim updNilaiSO As String = "", updFilterSO As String = ""
                Dim updNilaiPI As String = "", updFilterPI As String = ""
                Dim updNilaiPL As String = "", updFilterPL As String = ""
                Dim updNilaiHppI As String = "", updFilterHppI As String = "", delFilterHppI As String = ""


                'VALIDASI JIKA SO CLOSE MAKA DO TIDAK DAPAT DI DRAFT --------------------------------------
                dtdetail = AsDataTableAmbilDariDBCon("SELECT so.sonotransaksi FROM m5_so so JOIN m5_so_detail sod ON so.soid = sod.idso JOIN m5_do_detail dod ON sod.idsodetail = dod.idsodetail AND dod.iddo = '" & FixDouble(idtransaksi) & "' AND so.sostatus NOT IN(2,3,4)", myConn)
                If dtdetail.Rows.Count > 0 Then
                    result(2) = "No. SO : " & dtdetail.Rows(0)("sonotransaksi") & " doesn't exists/yet approved in SO" : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI JIKA SO CLOSE MAKA DO TIDAK DAPAT DI DRAFT -------------------------------


                'AMBIL DATA DETAIL
                'dtdetail = AsDataTableAmbilDariDBCon("SELECT iddodetail, idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idsodetail, idpidetail, idpldetail, gudangasal, gudangtransit, idhppkhususmasuk, idhppfifomasuk, urutan FROM m5_do_detail WHERE iddo = '" & idtransaksi & "'", myConn)
                dtdetail = AsDataTableAmbilDariDBCon("SELECT iddodetail, idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idsodetail, idpidetail, idpldetail, gudangasal, gudangtransit, idhppkhususmasuk, idhppfifomasuk, urutan, (CASE LENGTH(IFNULL(cc.ccakun,'')) WHEN 0 THEN 1 ELSE 0 END) as transbarang FROM m5_do_detail dod LEFT JOIN m1_cost_center cc ON dod.costcenter = cc.cckode WHERE iddo = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        '1. SET NILAI
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang")
                        idsodetail = dr1("idsodetail") : idpidetail = dr1("idpidetail") : idpldetail = dr1("idpldetail")
                        gudangIn = dr1("gudangasal") : gudangOut = dr1("gudangtransit") : idhppkhususmasuk = dr1("idhppkhususmasuk")

                        '2. BUAT FILTER UPDATE OUTSTANDING
                        If idsodetail <> 0 Then

                            If idpidetail = 0 And idpldetail = 0 Then
                                '2.1 SET NILAI UPDATE OUTSTANDING SO
                                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsodetail=" & idsodetail)
                                updNilaiSO = String.Concat("WHEN '" & idsodetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiSO)

                                '2.2. SET FILTERUPDATE OUTSTANDING SO
                                updFilterSO = IIf(Len(updFilterSO.ToString) = 0, "", updFilterSO & " OR ")
                                updFilterSO = String.Concat(updFilterSO, "(idsodetail = '" & idsodetail & "')")
                            End If

                            ''SET NILAI UPDATE STOK BOOKING MASUK
                            'updStokInBooking = IIf(Len(updStokInBooking.ToString) = 0, "", updStokInBooking & ", ")
                            'updStokInBooking = String.Concat(updStokInBooking, "('" & idbarang & "', '" & gudangIn & "', ('" & jmlbarang & "'))") ' idbarang, kgudang, stok

                        End If

                        If idpidetail <> 0 Then
                            '2.1 SET NILAI UPDATE OUTSTANDING PI
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idpidetail=" & idpidetail)
                            updNilaiPI = String.Concat("WHEN '" & idpidetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiPI)

                            '2.2. SET FILTERUPDATE OUTSTANDING PI
                            updFilterPI = IIf(Len(updFilterPI.ToString) = 0, "", updFilterPI & " OR ")
                            updFilterPI = String.Concat(updFilterPI, "(idpidetail = '" & idpidetail & "')")
                        End If

                        If idpldetail <> 0 Then
                            '2.1 SET NILAI UPDATE OUTSTANDING PL
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idpldetail=" & idpldetail)
                            updNilaiPL = String.Concat("WHEN '" & idpldetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiPL)

                            '2.2. SET FILTERUPDATE OUTSTANDING PL
                            updFilterPL = IIf(Len(updFilterPL.ToString) = 0, "", updFilterPL & " OR ")
                            updFilterPL = String.Concat(updFilterPL, "(idpldetail = '" & idpldetail & "')")
                        End If

                        If Double.Parse(dr1("transbarang")) = 1 Then
                            'VALIDASI STOK -------------------------------
                            '1. CEK DATA EXIST
                            ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                            ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists,  bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                            '2. CEK JML STOK
                            Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangtransit='" & gudangOut & "'")
                            ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
                            ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > isw.stok) ")

                            '3. SET NILAI UPDATE STOK KELUAR
                            updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
                            updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

                            '4. SET NILAI UPDATE STOK MASUK
                            updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
                            updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok
                        End If
                        
                    Next

                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If


                'VALIDASI STOK ----------------------------------
                Dim rsValidasi As String = ValidasiSimpan(dtdetail, "", "", "", "", "", "", ftExistStok, ftStok, "", "", "", "", "", "", "", "", "", "")
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'END OF VALIDASI STOK ---------------------------

                'VALIDASI GUDANG ASSET ---------------
                'ValidasiGudangAsset
                dtasset = AsDataTableAmbilDariDBCon("SELECT atasetid, atidbarang, atkode FROM M7_Asset_Transaction WHERE atsumber = '" & sumber & "' AND atidutama = '" & idtransaksi & "' ", myConn)
                rsValidasi = ValidasiGudangAsset(dtasset, gudangOut)
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                'END OF VALIDASI GUDANG ASSET --------


                'UPDATE OUTSTANDING =============================================================
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

                If Len(updFilterPI) > 0 Then
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m5_pi_detail SET jmlrealisasi = (CASE idpidetail " & updNilaiPI & " ELSE jmlrealisasi END) WHERE " & updFilterPI
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idpi FROM m5_pi_detail WHERE " & updFilterPI & " GROUP BY idpi", myConn)
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idpi = '" & dr1("idpi") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDBCon("SELECT idpi, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_pi_detail WHERE " & ftDetail & " GROUP BY idpi", myConn)
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiPI = "" : updFilterPI = ""
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
                            updNilaiPI = String.Concat(updNilaiPI, "WHEN '" & dr1("idpi") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterPI = IIf(Len(updFilterPI.ToString) = 0, "", updFilterPI & " OR ")
                            updFilterPI = String.Concat(updFilterPI, "(piid = '" & dr1("idpi") & "')")
                        Next

                        sql = "UPDATE m5_pi SET pistatusrealisasi = (CASE piid " & updNilaiPI & " ELSE pistatusrealisasi END) WHERE " & updFilterPI
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

                If Len(updFilterPL) > 0 Then 'PL
                    'UPDATE OUTSTANDING DETAIL -------------------
                    sql = "UPDATE m5_pl_detail SET jmlrealisasi = (CASE idpldetail " & updNilaiPL & " ELSE jmlrealisasi END) WHERE " & updFilterPL
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idpl FROM m5_pl_detail WHERE " & updFilterPL & " GROUP BY idpl", myConn)
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idpl = '" & dr1("idpl") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDBCon("SELECT idpl, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_pl_detail WHERE " & ftDetail & " GROUP BY idpl", myConn)
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiPL = "" : updFilterPL = ""
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
                            updNilaiPL = String.Concat(updNilaiPL, "WHEN '" & dr1("idpl") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterPL = IIf(Len(updFilterPL.ToString) = 0, "", updFilterPL & " OR ")
                            updFilterPL = String.Concat(updFilterPL, "(plid = '" & dr1("idpl") & "')")
                        Next

                        sql = "UPDATE m5_pl SET plstatusrealisasi = (CASE plid " & updNilaiPL & " ELSE plstatusrealisasi END) WHERE " & updFilterPL
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
                'END OF UPDATE OUTSTANDING ======================================================


                'UPDATE NO BATCH ================================================================
                Dim updNilaiBatch As String = "", updFilterBatch As String = ""
                Dim dtBatch As DataTable = AsDataTableAmbilDariDBCon("SELECT nboidbatchin, nbogudang, nboidbarang, nbokode, nbojmlkeluar FROM m1_no_batch_out WHERE nbosumber = '" & sumber & "' AND nboidtransaksi = '" & idtransaksi & "'", myConn)
                If dtBatch.Rows.Count > 0 Then
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

                    'DELETE NO BATCH OUT --------------------------------
                    sql = "DELETE FROM m1_no_batch_out WHERE nbosumber = '" & sumber & "' AND nboidtransaksi = '" & idtransaksi & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE NO BATCH IN KELUAR --------------------------
                    For Each dr1 As DataRow In dtBatch.Rows
                        'SET NILAI UPDATE BATCH IN
                        Dim jmlKeluar As Double = AsDataTableDSum(dtBatch, "nbojmlkeluar", "nboidbatchin = " & dr1("nboidbatchin") & "")
                        updNilaiBatch = String.Concat("WHEN nbiidbatchin = '" & dr1("nboidbatchin") & "' THEN ROUND(nbijmlkeluar - '" & jmlKeluar & "', 5) ", updNilaiBatch)

                        'SET FILTER UPDATE BATCH IN
                        updFilterBatch = IIf(Len(updFilterBatch.ToString) = 0, "", updFilterBatch & " OR ")
                        updFilterBatch = String.Concat(updFilterBatch, "(nbiidbatchin = '" & dr1("nboidbatchin") & "')")
                    Next
                    If Len(updNilaiBatch) > 0 Then
                        sql = "UPDATE m1_no_batch_in SET nbijmlkeluar =  (CASE " & updNilaiBatch & " ELSE nbijmlkeluar END) WHERE " & updFilterBatch
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
                'END OF UPDATE NO BATCH =========================================================


                'UPDATE NO SERIAL ===============================================================
                Dim updNilaiSerial As String = "", updFilterSerial As String = ""
                Dim dtSerial As DataTable = AsDataTableAmbilDariDBCon("SELECT nsoidserialin, nsogudang, nsoidbarang, nsokode, nsojmlkeluar FROM m1_no_serial_out WHERE nsosumber = '" & sumber & "' AND nsoidtransaksi = '" & idtransaksi & "'", myConn)
                If dtSerial.Rows.Count > 0 Then
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

                    'DELETE NO SERIAL OUT -------------------------------
                    sql = "DELETE FROM m1_no_serial_out WHERE nsosumber = '" & sumber & "' AND nsoidtransaksi = '" & idtransaksi & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE NO SERIAL IN KELUAR -------------------------
                    For Each dr1 As DataRow In dtSerial.Rows
                        'SET NILAI UPDATE SERIAL IN
                        Dim jmlKeluar As Double = AsDataTableDSum(dtSerial, "nsojmlkeluar", "nsoidserialin = " & dr1("nsoidserialin") & "")
                        updNilaiSerial = String.Concat("WHEN nsiidserialin = '" & dr1("nsoidserialin") & "' THEN ROUND(nsijmlkeluar - '" & jmlKeluar & "', 5) ", updNilaiSerial)

                        'SET FILTER UPDATE SERIAL IN
                        updFilterSerial = IIf(Len(updFilterSerial.ToString) = 0, "", updFilterSerial & " OR ")
                        updFilterSerial = String.Concat(updFilterSerial, "(nsiidserialin = '" & dr1("nsoidserialin") & "')")
                    Next
                    If Len(updNilaiSerial) > 0 Then
                        sql = "UPDATE m1_no_serial_in SET nsijmlkeluar =  (CASE " & updNilaiSerial & " ELSE nsijmlkeluar END) WHERE " & updFilterSerial
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
                'END OF UPDATE NO SERIAL =======================================================


                'UPDATE NO ASSET ===============================================================
                If dtasset.Rows.Count > 0 Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtasset.Rows
                        'QUERY INSERT NO ASSET IN
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append(FixDouble(dr1("atasetid")))
                    Next
                    sql = "UPDATE m7_asset a SET a.agudang = '" & gudangIn & "' WHERE a.aid IN(" & strValue2.ToString & ")"
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


                'UPDATE STOK BOOKING ============================================================
                'MENAMBAH BOOKING HANYA UNTUK BARANG YG HPP NYA BUKAN KHUSUS (I) DAN TERKAIT DARI SO
                sql = "INSERT INTO m1_item_booking (SELECT idbarang, gudangasal, jmlbarang FROM m5_do_detail JOIN m1_item ON idbarang = bid AND bjenis <> 'J' AND bhpp <> 'I' AND idsodetail <> 0 AND iddo = '" & idtransaksi & "') ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'If Len(updStokInBooking) > 0 Then
                '    sql = "INSERT INTO m1_item_booking (idbarang, gudang, jmlbooking) VALUES " & updStokInBooking & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '    With objCmd
                '        .Connection = myconn
                '        .Transaction = Trans
                '        .CommandType = CommandType.Text
                '        .CommandText = sql
                '    End With
                '    objCmd.ExecuteNonQuery()
                'End If
                'END OF UPDATE STOK BOOKING =====================================================


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

                'STOK MASUK
                If Len(updStokIn) > 0 Then
                    sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokIn & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
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


                'UNCOMPLETE COST CENTER
                sql = "UPDATE m5_do_detail dod JOIN m1_cost_center cc ON dod.costcenter = cc.cckode JOIN m0_setting s2 ON s2.smodule = 0 AND s2.sgrup = 'options' AND s2.skode = 'DONonaktifCostcenter' AND s2.snilai = 1 SET cc.ccaktif = 1 WHERE dod.iddo = '" & idtransaksi & "';"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()


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
            sql = "UPDATE M5_Do SET Dostatus = " & nilaiStatus & ", Domodifikasiuser='" & userid & "', Domodifikasitgl = NOW(), Doposting = 0, Dopostingtgl = '1971-01-01 00:00:00', Dojmlrevisi = Dojmlrevisi + 1 WHERE Doid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_DoSearch(PostWsSearch(paramSplit(0), "M5_doSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M5_DoDelete(ByVal param As String) As String

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
            Dim sumber As String = "Do", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Doid, Donotransaksi FROM M5_Do WHERE Doid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT docabang, dolokasi, dosumber, doautonotransaksi, donotransaksi, dotgl"
            sql &= " FROM M5_do"
            sql &= " WHERE doid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("docabang")
                lokasi = dtNomorNext.Rows(0)("dolokasi")
                sumber = dtNomorNext.Rows(0)("dosumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("doautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("donotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("dotgl"))
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


            'DELETE DETAIL
            sql = "DELETE FROM M5_Do_Detail WHERE iddo = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE UTAMA
            sql = "DELETE FROM M5_Do WHERE doid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_DoSearch(PostWsSearch(paramSplit(0), "M5_DoSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M5_DoGetdataById(ByVal param As String) As String
        'M5_DoGetdataById Utama --------------------------------------------------------
        'doid, docabang, dolokasi, dogudang, doasalbarang, doasalbarangkategori, dojenispenjualan, 
        'dojenispenjualankategori, docarabayar, dosumber, doautonotransaksi, donotransaksi, dotgl, dokodepa, 
        'docustomer, docustomerkontak, do1alamat1, do1alamat2, do1alamat3, do2alamat1, do2alamat2, 
        'do2alamat3, dobagianpenjualan, dobagianpengiriman, doekspedisi, dotglkirim, dotermin, dotgljatuhtempo, 
        'douraian, docatatan, donoref, dotglnoref, dotglpenutupan, domatauang, dokurs, 
        'dohargatermasukpajak, dototal, dodiskonpersen, dojmldiskon, dototalpajak1detail, dototalpajak2detail, dobiayalainpersen, 
        'dobiayalain, dototaltransaksi, dorekdiskon, dorekpajak1, dorekpajak2, dorekbiayalain, doidsq, 
        'doidso, doidpi, doidpl, dostatusdr, dostatussi, dostatusrnr, dostatussr, 
        'dostatusrealisasi, dostatus, dostatussebelumnya, dojmlrevisi, docetakanke, doinputuser, doinputtgl, 
        'domodifikasiuser, domodifikasitgl, doposting, dopostingtgl, dotutupperiode, doisclose, docustomtext1, 
        'docustomtext2, docustomtext3, docustomtext4, docustomtext5, docustomint1, docustomint2, docustomint3, 
        'docustomdbl1, docustomdbl2, docustomdbl3, docustomdate1, docustomdate2, docustomdate3, docabangnama, 
        'dolokasinama, dogudangnama, docustomerkode, docustomernama, dobagianpenjualankode, dobagianpenjualannama, dobagianpengirimankode, 
        'dobagianpengirimannama, doekspedisinama, doterminnama, doterminharijatuhtempo, dorekdiskonnama, dorekpajak1nama, dorekpajak2nama, 
        'dorekbiayalainnama, donotransaksisq, donotransaksiso, donotransaksipi, donotransaksipl, dostatusnama, dostatussebelumnyanama, 
        'doinputusernama, domodifikasiusernama, ktingkatjual, kpkp

        'M5_DoGetdataById Detail --------------------------------------------------------
        'iddodetail, iddo, idbarang, namabarang, tipebarang, 
        'jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, 
        'idhppkhususmasuk, idhppfifomasuk, harga, hpp, diskon, jmldiskon, pajak1, 
        'jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, 
        'gudangtujuan, rekpersediaan, rekhargapokok, rekdiskonpenjualan, costcenter, divisi, subdivisi, 
        'proyek, catatan, urutan, idsqdetail, idsodetail, idpidetail, idpldetail, 
        'jmldr, statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, 
        'statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, 
        'bhpp, bjenis, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, 
        'pajak2nilai, cabangnama, lokasinama, gudangasalnama, gudangtransitnama, gudangtujuannama, costcenternama, 
        'divisinama, subdivisinama, proyeknama, sqnotransaksi, sonotransaksi, pinotransaksi, plnotransaksi, 
        'bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan

        'M5_DoGetdataById Batch --------------------------------------------------------
        'nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang

        'M5_DoGetdataById Serial --------------------------------------------------------
        'nstid, nstjenismutasi, nstidserialin, nstgudang,  nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, 
        'nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, 
        'nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang

        'M5_DoGetdataById Asset --------------------------------------------------------
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
        Dim sumber As String = "DO", asset As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M5_do~M5_do_Detail-" & idtransaksi

        'Redoace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi redoace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "doid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "doid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_do_getdata")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("doid"), 0), sptField,
                     FxDB(drutama("docabang"), ""), sptField,
                     FxDB(drutama("dolokasi"), ""), sptField,
                     FxDB(drutama("dogudang"), ""), sptField,
                     FxDB(drutama("doasalbarang"), ""), sptField,
                     FxDB(drutama("doasalbarangkategori"), 0), sptField,
                     FxDB(drutama("dojenispenjualan"), ""), sptField,
                     FxDB(drutama("dojenispenjualankategori"), 0), sptField,
                     FxDB(drutama("docarabayar"), 0), sptField,
                     FxDB(drutama("dosumber"), ""), sptField,
                     FxDB(drutama("doautonotransaksi"), 0), sptField,
                     FxDB(drutama("donotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("dotgl"), ""), formatTgl), sptField,
                     FxDB(drutama("dokodepa"), 0), sptField,
                     FxDB(drutama("docustomer"), 0), sptField,
                     FxDB(drutama("docustomerkontak"), ""), sptField,
                     FxDB(drutama("do1alamat1"), ""), sptField,
                     FxDB(drutama("do1alamat2"), ""), sptField,
                     FxDB(drutama("do1alamat3"), ""), sptField,
                     FxDB(drutama("do2alamat1"), ""), sptField,
                     FxDB(drutama("do2alamat2"), ""), sptField,
                     FxDB(drutama("do2alamat3"), ""), sptField,
                     FxDB(drutama("dobagianpenjualan"), 0), sptField,
                     FxDB(drutama("dobagianpengiriman"), 0), sptField,
                     FxDB(drutama("doekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("dotglkirim"), ""), formatTgl), sptField,
                     FxDB(drutama("dotermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("dotgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("douraian"), ""), sptField,
                     FxDB(drutama("docatatan"), ""), sptField,
                     FxDB(drutama("donoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("dotglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("dotglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("domatauang"), ""), sptField,
                     FxDB(drutama("dokurs"), 0), sptField,
                     FxDB(drutama("dohargatermasukpajak"), 0), sptField,
                     FxDB(drutama("dototal"), 0), sptField,
                     FxDB(drutama("dodiskonpersen"), ""), sptField,
                     FxDB(drutama("dojmldiskon"), 0), sptField,
                     FxDB(drutama("dototalpajak1detail"), 0), sptField,
                     FxDB(drutama("dototalpajak2detail"), 0), sptField,
                     FxDB(drutama("dobiayalainpersen"), 0), sptField,
                     FxDB(drutama("dobiayalain"), 0), sptField,
                     FxDB(drutama("dototaltransaksi"), 0), sptField,
                     FxDB(drutama("dorekdiskon"), ""), sptField,
                     FxDB(drutama("dorekpajak1"), ""), sptField,
                     FxDB(drutama("dorekpajak2"), ""), sptField,
                     FxDB(drutama("dorekbiayalain"), ""), sptField,
                     FxDB(drutama("doidsq"), 0), sptField,
                     FxDB(drutama("doidso"), 0), sptField,
                     FxDB(drutama("doidpi"), 0), sptField,
                     FxDB(drutama("doidpl"), 0), sptField,
                     FxDB(drutama("dostatusdr"), 0), sptField,
                     FxDB(drutama("dostatussi"), 0), sptField,
                     FxDB(drutama("dostatusrnr"), 0), sptField,
                     FxDB(drutama("dostatussr"), 0), sptField,
                     FxDB(drutama("dostatusrealisasi"), 0), sptField,
                     FxDB(drutama("dostatus"), 0), sptField,
                     FxDB(drutama("dostatussebelumnya"), 0), sptField,
                     FxDB(drutama("dojmlrevisi"), 0), sptField,
                     FxDB(drutama("docetakanke"), 0), sptField,
                     FxDB(drutama("doinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("doinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("domodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("domodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("doposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("dopostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("dotutupperiode"), 0), sptField,
                     FxDB(drutama("doisclose"), 0), sptField,
                     FxDB(drutama("docustomtext1"), ""), sptField,
                     FxDB(drutama("docustomtext2"), ""), sptField,
                     FxDB(drutama("docustomtext3"), ""), sptField,
                     FxDB(drutama("docustomtext4"), ""), sptField,
                     FxDB(drutama("docustomtext5"), ""), sptField,
                     FxDB(drutama("docustomint1"), 0), sptField,
                     FxDB(drutama("docustomint2"), 0), sptField,
                     FxDB(drutama("docustomint3"), 0), sptField,
                     FxDB(drutama("docustomdbl1"), 0), sptField,
                     FxDB(drutama("docustomdbl2"), 0), sptField,
                     FxDB(drutama("docustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("docustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("docustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("docustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("docabangnama"), ""), sptField,
                     FxDB(drutama("dolokasinama"), ""), sptField,
                     FxDB(drutama("dogudangnama"), ""), sptField,
                     FxDB(drutama("docustomerkode"), ""), sptField,
                     FxDB(drutama("docustomernama"), ""), sptField,
                     FxDB(drutama("dobagianpenjualankode"), ""), sptField,
                     FxDB(drutama("dobagianpenjualannama"), ""), sptField,
                     FxDB(drutama("dobagianpengirimankode"), ""), sptField,
                     FxDB(drutama("dobagianpengirimannama"), ""), sptField,
                     FxDB(drutama("doekspedisinama"), ""), sptField,
                     FxDB(drutama("doterminnama"), ""), sptField,
                     FxDB(drutama("doterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("dorekdiskonnama"), ""), sptField,
                     FxDB(drutama("dorekpajak1nama"), ""), sptField,
                     FxDB(drutama("dorekpajak2nama"), ""), sptField,
                     FxDB(drutama("dorekbiayalainnama"), ""), sptField,
                     FxDB(drutama("donotransaksisq"), ""), sptField,
                     FxDB(drutama("donotransaksiso"), ""), sptField,
                     FxDB(drutama("donotransaksipi"), ""), sptField,
                     FxDB(drutama("donotransaksipl"), ""), sptField,
                     FxDB(drutama("dostatusnama"), ""), sptField,
                     FxDB(drutama("dostatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("doinputusernama"), ""), sptField,
                     FxDB(drutama("domodifikasiusernama"), ""), sptField,
                     FxDB(drutama("ktingkatjual"), 0), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("iddodetail"), 0), sptField,
                     FxDB(dr("iddo"), 0), sptField,
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
                     FxDB(dr("idhppkhususmasuk"), 0), sptField,
                     FxDB(dr("idhppfifomasuk"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
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
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idsqdetail"), 0), sptField,
                     FxDB(dr("idsodetail"), 0), sptField,
                     FxDB(dr("idpidetail"), 0), sptField,
                     FxDB(dr("idpldetail"), 0), sptField,
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
                     FxDB(dr("sqnotransaksi"), ""), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("pinotransaksi"), ""), sptField,
                     FxDB(dr("plnotransaksi"), ""), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA BATCH
            sql = "select `nbt`.`nbtid` AS `nbtid`,`nbt`.`nbtjenismutasi` AS `nbtjenismutasi`,`nbt`.`nbtidbatchin` AS `nbtidbatchin`,`nbt`.`nbtgudang` AS `nbtgudang`,`nbt`.`nbtidbarang` AS `nbtidbarang`,`nbt`.`nbtkode` AS `nbtkode`,`nbt`.`nbtsumber` AS `nbtsumber`,`nbt`.`nbtidtransaksi` AS `nbtidtransaksi`,`nbt`.`nbtsatuan` AS `nbtsatuan`,`nbt`.`nbtjml` AS `nbtjml`,`nbt`.`nbtcustomtext1` AS `nbtcustomtext1`,`nbt`.`nbtcustomtext2` AS `nbtcustomtext2`,`nbt`.`nbtcustomtext3` AS `nbtcustomtext3`,`nbt`.`nbtcustomdbl1` AS `nbtcustomdbl1`,`nbt`.`nbtcustomdbl2` AS `nbtcustomdbl2`,`nbt`.`nbtcustomdbl3` AS `nbtcustomdbl3`,`nbt`.`nbtcustomdate1` AS `nbtcustomdate1`,`nbt`.`nbtcustomdate2` AS `nbtcustomdate2`,`nbt`.`nbtcustomdate3` AS `nbtcustomdate3`,`i`.`bkode` AS `kodebarang`, nbi.nbinotransaksi from ((`m1_no_batch_transaction` `nbt` join `m1_item` `i` on((`nbt`.`nbtidbarang` = `i`.`bid`))) left join `m1_no_batch_in` `nbi` on((`nbt`.`nbtidbatchin` = `nbi`.`nbiidbatchin`)))"
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
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("nbinotransaksi"), ""), sptRow)
            Next
            If batch.Length > 0 Then batch = batch.Substring(0, batch.Length - sptRow.Length) Else batch = batch

            'AMBIL DATA SERIAL
            sql = "select `nst`.`nstid` AS `nstid`,`nst`.`nstjenismutasi` AS `nstjenismutasi`,`nst`.`nstidserialin` AS `nstidserialin`,`nst`.`nstgudang` AS `nstgudang`,`nst`.`nstidbarang` AS `nstidbarang`,`nst`.`nstkode` AS `nstkode`,`nst`.`nstsumber` AS `nstsumber`,`nst`.`nstidtransaksi` AS `nstidtransaksi`,`nst`.`nstsatuan` AS `nstsatuan`,`nst`.`nstjml` AS `nstjml`,`nst`.`nstcustomtext1` AS `nstcustomtext1`,`nst`.`nstcustomtext2` AS `nstcustomtext2`,`nst`.`nstcustomtext3` AS `nstcustomtext3`,`nst`.`nstcustomdbl1` AS `nstcustomdbl1`,`nst`.`nstcustomdbl2` AS `nstcustomdbl2`,`nst`.`nstcustomdbl3` AS `nstcustomdbl3`,`nst`.`nstcustomdate1` AS `nstcustomdate1`,`nst`.`nstcustomdate2` AS `nstcustomdate2`,`nst`.`nstcustomdate3` AS `nstcustomdate3`,`i`.`bkode` AS `kodebarang`, IFNULL(nsinotransaksi,'') AS nsinotransaksi from ((`m1_no_serial_transaction` `nst` join `m1_item` `i` on((`nst`.`nstidbarang` = `i`.`bid`))) left join `m1_no_serial_in` `nsi` on((`nst`.`nstidserialin` = `nsi`.`nsiidserialin`)))"
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
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("nsinotransaksi"), ""), sptRow)
            Next
            If serial.Length > 0 Then
                serial = serial.Substring(0, serial.Length - sptRow.Length)
            Else
                serial = serial
            End If

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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("doid, docabang, dolokasi, dogudang, doasalbarang, doasalbarangkategori, dojenispenjualan, dojenispenjualankategori, docarabayar, dosumber, doautonotransaksi, donotransaksi, dotgl, dokodepa, docustomer, docustomerkontak, do1alamat1, do1alamat2, do1alamat3, do2alamat1, do2alamat2, do2alamat3, dobagianpenjualan, dobagianpengiriman, doekspedisi, dotglkirim, dotermin, dotgljatuhtempo, douraian, docatatan, donoref, dotglnoref, dotglpenutupan, domatauang, dokurs, dohargatermasukpajak, dototal, dodiskonpersen, dojmldiskon, dototalpajak1detail, dototalpajak2detail, dobiayalainpersen, dobiayalain, dototaltransaksi, dorekdiskon, dorekpajak1, dorekpajak2, dorekbiayalain, doidsq, doidso, doidpi, doidpl, dostatusdr, dostatussi, dostatusrnr, dostatussr, dostatusrealisasi, dostatus, dostatussebelumnya, dojmlrevisi, docetakanke, doinputuser, doinputtgl, domodifikasiuser, domodifikasitgl, doposting, dopostingtgl, dotutupperiode, doisclose, docustomtext1, docustomtext2, docustomtext3, docustomtext4, docustomtext5, docustomint1, docustomint2, docustomint3, docustomdbl1, docustomdbl2, docustomdbl3, docustomdate1, docustomdate2, docustomdate3, docabangnama, dolokasinama, dogudangnama, docustomerkode, docustomernama, dobagianpenjualankode, dobagianpenjualannama, dobagianpengirimankode, dobagianpengirimannama, doekspedisinama, doterminnama, doterminharijatuhtempo, dorekdiskonnama, dorekpajak1nama, dorekpajak2nama, dorekbiayalainnama, donotransaksisq, donotransaksiso, donotransaksipi, donotransaksipl, dostatusnama, dostatussebelumnyanama, doinputusernama, domodifikasiusernama, ktingkatjual, kpkp" &
                sptSubParam & "iddodetail, iddo, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, idhppkhususmasuk, idhppfifomasuk, harga, hpp, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekhargapokok, rekdiskonpenjualan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpidetail, idpldetail, jmldr, statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, basset, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangasalnama, gudangtransitnama, gudangtujuannama, costcenternama, divisinama, subdivisinama, proyeknama, sqnotransaksi, sonotransaksi, pinotransaksi, plnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan" &
                sptSubParam & "nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang, nbtnotransaksi" &
                sptSubParam & "nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang, nstnotransaksi" &
                sptSubParam & "atid, atasetid, atjenismutasi, atsumber, atidutama, atidbarang, atkode, atnama, atkategori, atcabang, atlokasi, atgudang, atdivisi, atsubdivisi, atcostcenter, atproyek, atcatatan, atnomor, attglbeli, attglpakai, atjml, atsatuan, atmatauang, atkurs, atharga, atdiskon, atjmldiskon, atpajak1, atjmlpajak1, atpajak2, atjmlpajak2, athargabeli, atnilairesidu, atumurekonomis, atbebanperbln, atakumulasibeban, atnilaibuku, atnilaipenyusutan, atmetode, attabelpenyusutan, atintangible, atfiskal, atatastengahbulan, atrekasset, atrekakumdepresiasi, atrekdepresiasi, atrekpenghapusan, atprodusen, attglpensiun, atpenyusutanke, atnilaimenurun, atdispose, atpembelian, atpenjualan, atlocked, atstatus, atstatussebelumnya, atisclose, atinputuser, atinputtgl, atmodifikasiuser, atmodifikasitgl, atcustomtext1, atcustomtext2, atcustomtext3, atcustomtext4, atcustomtext5, atcustomint1, atcustomint2, atcustomint3, atcustomint4, atcustomint5, atcustomdbl1, atcustomdbl2, atcustomdbl3, atcustomdbl4, atcustomdbl5, atcustomdate1, atcustomdate2, atcustomdate3, atcustomdate4, atcustomdate5, atkategorinama, atcabangnama, atlokasinama, atgudangnama, atdivisinama, atsubdivisinama, atcostcenternama, atproyeknama, atmetodenama, atpajak1nama, atpajak1nilai, atpajak2nama, atpajak2nilai, atrekassetnama, atrekakumdepresiasinama, atrekdepresiasinama, atrekpenghapusannama, atprodusenkode, atprodusennama, atstatusnama, atstatussebelumnyanama, atinputusernama, atmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_DoSearch(ByVal param As String) As String
        'M5_doSearch --------------------------------------------------------
        'doid, docabang, dolokasi, dogudang, doasalbarang, doasalbarangkategori, dojenispenjualan, 
        'dojenispenjualankategori, docarabayar, dosumber, doautonotransaksi, donotransaksi, dotgl, dokodepa, 
        'docustomer, docustomerkontak, do1alamat1, do1alamat2, do1alamat3, do2alamat1, do2alamat2, 
        'do2alamat3, dobagianpenjualan, dobagianpengiriman, doekspedisi, dotglkirim, dotermin, dotgljatuhtempo, 
        'douraian, docatatan, donoref, dotglnoref, dotglpenutupan, domatauang, dokurs, 
        'dohargatermasukpajak, dototal, dodiskonpersen, dojmldiskon, dototalpajak1detail, dototalpajak2detail, dobiayalainpersen, 
        'dobiayalain, dototaltransaksi, dorekdiskon, dorekpajak1, dorekpajak2, dorekbiayalain, doidsq, 
        'doidso, doidpi, doidpl, dostatusdr, dostatussi, dostatusrnr, dostatussr, 
        'dostatusrealisasi, dostatus, dostatussebelumnya, dojmlrevisi, docetakanke, doinputuser, doinputtgl, 
        'domodifikasiuser, domodifikasitgl, doposting, dopostingtgl, dotutupperiode, doisclose, docabangnama, 
        'dolokasinama, dogudangnama, docustomerkode, docustomernama, dobagianpenjualankode, dobagianpenjualannama, doekspedisinama, 
        'sqnotransaksi, sonotransaksi, pinotransaksi, plnotransaksi, dostatusnama, dostatussebelumnyanama, doinputusernama, 
        'domodifikasiusernama

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
        sql = query.PanggilQuery("m5_do_v")

        dt = AmbilData("aplikasi1-M5_do_V", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("doid"), 0), sptField,
                     FxDB(dr("docabang"), ""), sptField,
                     FxDB(dr("dolokasi"), ""), sptField,
                     FxDB(dr("dogudang"), ""), sptField,
                     FxDB(dr("doasalbarang"), ""), sptField,
                     FxDB(dr("doasalbarangkategori"), 0), sptField,
                     FxDB(dr("dojenispenjualan"), ""), sptField,
                     FxDB(dr("dojenispenjualankategori"), 0), sptField,
                     FxDB(dr("docarabayar"), 0), sptField,
                     FxDB(dr("dosumber"), ""), sptField,
                     FxDB(dr("doautonotransaksi"), 0), sptField,
                     FxDB(dr("donotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dotgl"), ""), formatTgl), sptField,
                     FxDB(dr("dokodepa"), 0), sptField,
                     FxDB(dr("docustomer"), 0), sptField,
                     FxDB(dr("docustomerkontak"), ""), sptField,
                     FxDB(dr("do1alamat1"), ""), sptField,
                     FxDB(dr("do1alamat2"), ""), sptField,
                     FxDB(dr("do1alamat3"), ""), sptField,
                     FxDB(dr("do2alamat1"), ""), sptField,
                     FxDB(dr("do2alamat2"), ""), sptField,
                     FxDB(dr("do2alamat3"), ""), sptField,
                     FxDB(dr("dobagianpenjualan"), 0), sptField,
                     FxDB(dr("dobagianpengiriman"), 0), sptField,
                     FxDB(dr("doekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dotglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("dotermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dotgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("douraian"), ""), sptField,
                     FxDB(dr("docatatan"), ""), sptField,
                     FxDB(dr("donoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dotglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("dotglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("domatauang"), ""), sptField,
                     FxDB(dr("dokurs"), 0), sptField,
                     FxDB(dr("dohargatermasukpajak"), 0), sptField,
                     FxDB(dr("dototal"), 0), sptField,
                     FxDB(dr("dodiskonpersen"), ""), sptField,
                     FxDB(dr("dojmldiskon"), 0), sptField,
                     FxDB(dr("dototalpajak1detail"), 0), sptField,
                     FxDB(dr("dototalpajak2detail"), 0), sptField,
                     FxDB(dr("dobiayalainpersen"), 0), sptField,
                     FxDB(dr("dobiayalain"), 0), sptField,
                     FxDB(dr("dototaltransaksi"), 0), sptField,
                     FxDB(dr("dorekdiskon"), ""), sptField,
                     FxDB(dr("dorekpajak1"), ""), sptField,
                     FxDB(dr("dorekpajak2"), ""), sptField,
                     FxDB(dr("dorekbiayalain"), ""), sptField,
                     FxDB(dr("doidsq"), 0), sptField,
                     FxDB(dr("doidso"), 0), sptField,
                     FxDB(dr("doidpi"), 0), sptField,
                     FxDB(dr("doidpl"), 0), sptField,
                     FxDB(dr("dostatusdr"), 0), sptField,
                     FxDB(dr("dostatussi"), 0), sptField,
                     FxDB(dr("dostatusrnr"), 0), sptField,
                     FxDB(dr("dostatussr"), 0), sptField,
                     FxDB(dr("dostatusrealisasi"), 0), sptField,
                     FxDB(dr("dostatus"), 0), sptField,
                     FxDB(dr("dostatussebelumnya"), 0), sptField,
                     FxDB(dr("dojmlrevisi"), 0), sptField,
                     FxDB(dr("docetakanke"), 0), sptField,
                     FxDB(dr("doinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("doinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("domodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("domodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("doposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("dopostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("dotutupperiode"), 0), sptField,
                     FxDB(dr("doisclose"), 0), sptField,
                     FxDB(dr("docabangnama"), ""), sptField,
                     FxDB(dr("dolokasinama"), ""), sptField,
                     FxDB(dr("dogudangnama"), ""), sptField,
                     FxDB(dr("docustomerkode"), ""), sptField,
                     FxDB(dr("docustomernama"), ""), sptField,
                     FxDB(dr("dobagianpenjualankode"), ""), sptField,
                     FxDB(dr("dobagianpenjualannama"), ""), sptField,
                     FxDB(dr("doekspedisinama"), ""), sptField,
                     FxDB(dr("sqnotransaksi"), ""), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("pinotransaksi"), ""), sptField,
                     FxDB(dr("plnotransaksi"), ""), sptField,
                     FxDB(dr("dostatusnama"), ""), sptField,
                     FxDB(dr("dostatussebelumnyanama"), ""), sptField,
                     FxDB(dr("doinputusernama"), ""), sptField,
                     FxDB(dr("domodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("doid, docabang, dolokasi, dogudang, doasalbarang, doasalbarangkategori, dojenispenjualan, dojenispenjualankategori, docarabayar, dosumber, doautonotransaksi, donotransaksi, dotgl, dokodepa, docustomer, docustomerkontak, do1alamat1, do1alamat2, do1alamat3, do2alamat1, do2alamat2, do2alamat3, dobagianpenjualan, dobagianpengiriman, doekspedisi, dotglkirim, dotermin, dotgljatuhtempo, douraian, docatatan, donoref, dotglnoref, dotglpenutupan, domatauang, dokurs, dohargatermasukpajak, dototal, dodiskonpersen, dojmldiskon, dototalpajak1detail, dototalpajak2detail, dobiayalainpersen, dobiayalain, dototaltransaksi, dorekdiskon, dorekpajak1, dorekpajak2, dorekbiayalain, doidsq, doidso, doidpi, doidpl, dostatusdr, dostatussi, dostatusrnr, dostatussr, dostatusrealisasi, dostatus, dostatussebelumnya, dojmlrevisi, docetakanke, doinputuser, doinputtgl, domodifikasiuser, domodifikasitgl, doposting, dopostingtgl, dotutupperiode, doisclose, docabangnama, dolokasinama, dogudangnama, docustomerkode, docustomernama, dobagianpenjualankode, dobagianpenjualannama, doekspedisinama, sqnotransaksi, sonotransaksi, pinotransaksi, plnotransaksi, dostatusnama, dostatussebelumnyanama, doinputusernama, domodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_Do_Detail_VSearch(ByVal param As String) As String
        'M5_Do_Detail_VSearch --------------------------------------------------------
        'iddodetail, iddo, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, idhppkhususmasuk, idhppfifomasuk, 
        'harga, hpp, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, 
        'jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, 
        'rekhargapokok, rekdiskonpenjualan, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idsqdetail, idsodetail, idpidetail, idpldetail, jmldr, statusdr, 
        'jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, 
        'statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3, donotransaksi, douraian, docatatan, 
        'donoref, dotglnoref, dotglkirim, docustomerkontak, do1alamat1, do1alamat2, do1alamat3, 
        'do2alamat1, do2alamat2, do2alamat3, dobagianpenjualan, dobagianpenjualankode, dobagianpenjualannama, dobagianpengirimankode, 
        'dobagianpengirimannama, doekspedisi, doekspedisinama, dotermin, doterminnama, doterminharijatuhtempo, kodebarang, 
        'bhpp, bhppaverage, bhargajual1, bjenis, brekpenjualan, bserial, bbatch, 
        'pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisadr, jmlsisasi, jmlsisarealisasi, 
        'bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan, basset, docustomer, docustomerkode, docustomernama, ktingkatjual,
        'domatauang, dokurs, dohargatermasukpajak, dotgljatuhtempo, kpkp,
        'pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, 
        'pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, pajak2akunjualnama, bkp

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim dol As String = ""

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
        'dol = query.PanggilQuery("m5_do_detail_v")
        'dol = "select `dod`.`iddodetail` AS `iddodetail`,`dod`.`iddo` AS `iddo`,`dod`.`idbarang` AS `idbarang`,`dod`.`namabarang` AS `namabarang`,`dod`.`tipebarang` AS `tipebarang`,`dod`.`jml` AS `jml`,`dod`.`satuan` AS `satuan`,`dod`.`nilaisatuan` AS `nilaisatuan`,`dod`.`jmlbarang` AS `jmlbarang`,`dod`.`satuanbarang` AS `satuanbarang`,`dod`.`matauang` AS `matauang`,`dod`.`kurs` AS `kurs`,`dod`.`idhppkhususmasuk` AS `idhppkhususmasuk`,`dod`.`idhppfifomasuk` AS `idhppfifomasuk`,`dod`.`harga` AS `harga`,`dod`.`hpp` AS `hpp`,`dod`.`diskon` AS `diskon`,`dod`.`jmldiskon` AS `jmldiskon`,`dod`.`pajak1` AS `pajak1`,`dod`.`jmlpajak1` AS `jmlpajak1`,`dod`.`pajak2` AS `pajak2`,`dod`.`jmlpajak2` AS `jmlpajak2`,`dod`.`cabang` AS `cabang`,`dod`.`lokasi` AS `lokasi`,`dod`.`gudangasal` AS `gudangasal`,`dod`.`gudangtransit` AS `gudangtransit`,`dod`.`gudangtujuan` AS `gudangtujuan`,`i`.`brekpersediaan` AS `rekpersediaan`,`dod`.`rekhargapokok` AS `rekhargapokok`,`dod`.`rekdiskonpenjualan` AS `rekdiskonpenjualan`,`dod`.`costcenter` AS `costcenter`,`dod`.`divisi` AS `divisi`,`dod`.`subdivisi` AS `subdivisi`,`dod`.`proyek` AS `proyek`,`dod`.`catatan` AS `catatan`,`dod`.`urutan` AS `urutan`,`dod`.`idsqdetail` AS `idsqdetail`,`dod`.`idsodetail` AS `idsodetail`,`dod`.`idpidetail` AS `idpidetail`,`dod`.`idpldetail` AS `idpldetail`,`dod`.`jmldr` AS `jmldr`,`dod`.`statusdr` AS `statusdr`,`dod`.`jmlsi` AS `jmlsi`,`dod`.`statussi` AS `statussi`,`dod`.`jmlrnr` AS `jmlrnr`,`dod`.`statusrnr` AS `statusrnr`,`dod`.`jmlsr` AS `jmlsr`,`dod`.`statussr` AS `statussr`,`dod`.`jmlrealisasi` AS `jmlrealisasi`,`dod`.`statusrealisasi` AS `statusrealisasi`,`dod`.`isclose` AS `isclose`,`dod`.`customtext1` AS `customtext1`,`dod`.`customtext2` AS `customtext2`,`dod`.`customtext3` AS `customtext3`,`dod`.`customdbl1` AS `customdbl1`,`dod`.`customdbl2` AS `customdbl2`,`dod`.`customdbl3` AS `customdbl3`,`dod`.`customdate1` AS `customdate1`,`dod`.`customdate2` AS `customdate2`,`dod`.`customdate3` AS `customdate3`,`do`.`donotransaksi` AS `donotransaksi`,`do`.`douraian` AS `douraian`,`do`.`docatatan` AS `docatatan`,`do`.`donoref` AS `donoref`,`do`.`dotglnoref` AS `dotglnoref`,`do`.`dotglkirim` AS `dotglkirim`,`do`.`docustomerkontak` AS `docustomerkontak`,`do`.`do1alamat1` AS `do1alamat1`,`do`.`do1alamat2` AS `do1alamat2`,`do`.`do1alamat3` AS `do1alamat3`,`do`.`do2alamat1` AS `do2alamat1`,`do`.`do2alamat2` AS `do2alamat2`,`do`.`do2alamat3` AS `do2alamat3`,`do`.`dobagianpenjualan` AS `dobagianpenjualan`,`c1`.`kkode` AS `dobagianpenjualankode`,`c1`.`knama` AS `dobagianpenjualannama`,`c2`.`kkode` AS `dobagianpengirimankode`,`c2`.`knama` AS `dobagianpengirimannama`,`do`.`doekspedisi` AS `doekspedisi`,`e`.`enama` AS `doekspedisinama`,`do`.`dotermin` AS `dotermin`,`tr`.`trnama` AS `doterminnama`,`tr`.`trharijatuhtempo` AS `doterminharijatuhtempo`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bhppaverage` AS `bhppaverage`,`i`.`bhargajual1` AS `bhargajual1`,`i`.`bjenis` AS `bjenis`,`i`.`brekpenjualan` AS `brekpenjualan`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,((`dod`.`jmlbarang` - `dod`.`jmldr`) / `dod`.`nilaisatuan`) AS `jmlsisadr`,(`dod`.`jmlbarang` - (`dod`.`jmlsi` / `dod`.`nilaisatuan`)) AS `jmlsisasi`,((`dod`.`jmlbarang` - `dod`.`jmlrealisasi`) / `dod`.`nilaisatuan`) AS `jmlsisarealisasi`, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan, i.basset from ((((((((`m5_do_detail` `dod` left join `m5_do` `do` on((`dod`.`iddo` = `do`.`doid`))) left join `m1_terms` `tr` on((`do`.`dotermin` = `tr`.`trkode`))) left join `m1_contact` `c1` on((`do`.`dobagianpenjualan` = `c1`.`kid`))) left join `m1_contact` `c2` on((`do`.`dobagianpengiriman` = `c2`.`kid`))) left join `m1_expedition` `e` on((`do`.`doekspedisi` = `e`.`ekode`))) left join `m1_item` `i` on((`dod`.`idbarang` = `i`.`bid`))) left join `m1_tax` `t1` on((`dod`.`pajak1` = `t1`.`tkode`))) left join `m1_tax` `t2` on((`dod`.`pajak2` = `t2`.`tkode`)))"
        dol = "select `dod`.`iddodetail` AS `iddodetail`,`dod`.`iddo` AS `iddo`,`dod`.`idbarang` AS `idbarang`,`dod`.`namabarang` AS `namabarang`,`dod`.`tipebarang` AS `tipebarang`,`dod`.`jml` AS `jml`,`dod`.`satuan` AS `satuan`,`dod`.`nilaisatuan` AS `nilaisatuan`,`dod`.`jmlbarang` AS `jmlbarang`,`dod`.`satuanbarang` AS `satuanbarang`,`dod`.`matauang` AS `matauang`,`dod`.`kurs` AS `kurs`,`dod`.`idhppkhususmasuk` AS `idhppkhususmasuk`,`dod`.`idhppfifomasuk` AS `idhppfifomasuk`,`dod`.`harga` AS `harga`,`dod`.`hpp` AS `hpp`,`dod`.`diskon` AS `diskon`,`dod`.`jmldiskon` AS `jmldiskon`,`dod`.`pajak1` AS `pajak1`,`dod`.`jmlpajak1` AS `jmlpajak1`,`dod`.`pajak2` AS `pajak2`,`dod`.`jmlpajak2` AS `jmlpajak2`,`dod`.`cabang` AS `cabang`,`dod`.`lokasi` AS `lokasi`,`dod`.`gudangasal` AS `gudangasal`,`dod`.`gudangtransit` AS `gudangtransit`,`dod`.`gudangtujuan` AS `gudangtujuan`,`i`.`brekpersediaan` AS `rekpersediaan`,`dod`.`rekhargapokok` AS `rekhargapokok`,`dod`.`rekdiskonpenjualan` AS `rekdiskonpenjualan`,`dod`.`costcenter` AS `costcenter`,`dod`.`divisi` AS `divisi`,`dod`.`subdivisi` AS `subdivisi`,`dod`.`proyek` AS `proyek`,`dod`.`catatan` AS `catatan`,`dod`.`urutan` AS `urutan`,`dod`.`idsqdetail` AS `idsqdetail`,`dod`.`idsodetail` AS `idsodetail`,`dod`.`idpidetail` AS `idpidetail`,`dod`.`idpldetail` AS `idpldetail`,`dod`.`jmldr` AS `jmldr`,`dod`.`statusdr` AS `statusdr`,`dod`.`jmlsi` AS `jmlsi`,`dod`.`statussi` AS `statussi`,`dod`.`jmlrnr` AS `jmlrnr`,`dod`.`statusrnr` AS `statusrnr`,`dod`.`jmlsr` AS `jmlsr`,`dod`.`statussr` AS `statussr`,`dod`.`jmlrealisasi` AS `jmlrealisasi`,`dod`.`statusrealisasi` AS `statusrealisasi`,`dod`.`isclose` AS `isclose`,`dod`.`customtext1` AS `customtext1`,`dod`.`customtext2` AS `customtext2`,`dod`.`customtext3` AS `customtext3`,`dod`.`customdbl1` AS `customdbl1`,`dod`.`customdbl2` AS `customdbl2`,`dod`.`customdbl3` AS `customdbl3`,`dod`.`customdate1` AS `customdate1`,`dod`.`customdate2` AS `customdate2`,`dod`.`customdate3` AS `customdate3`,`do`.`donotransaksi` AS `donotransaksi`,`do`.`douraian` AS `douraian`,`do`.`docatatan` AS `docatatan`,`do`.`donoref` AS `donoref`,`do`.`dotglnoref` AS `dotglnoref`,`do`.`dotglkirim` AS `dotglkirim`,`do`.`docustomerkontak` AS `docustomerkontak`,`do`.`do1alamat1` AS `do1alamat1`,`do`.`do1alamat2` AS `do1alamat2`,`do`.`do1alamat3` AS `do1alamat3`,`do`.`do2alamat1` AS `do2alamat1`,`do`.`do2alamat2` AS `do2alamat2`,`do`.`do2alamat3` AS `do2alamat3`,`do`.`dobagianpenjualan` AS `dobagianpenjualan`,`c1`.`kkode` AS `dobagianpenjualankode`,`c1`.`knama` AS `dobagianpenjualannama`,`c2`.`kkode` AS `dobagianpengirimankode`,`c2`.`knama` AS `dobagianpengirimannama`,`do`.`doekspedisi` AS `doekspedisi`,`e`.`enama` AS `doekspedisinama`,`do`.`dotermin` AS `dotermin`,`tr`.`trnama` AS `doterminnama`,`tr`.`trharijatuhtempo` AS `doterminharijatuhtempo`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bhppaverage` AS `bhppaverage`,`i`.`bhargajual1` AS `bhargajual1`,`i`.`bjenis` AS `bjenis`,`i`.`brekpenjualan` AS `brekpenjualan`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,((`dod`.`jmlbarang` - `dod`.`jmldr`) / `dod`.`nilaisatuan`) AS `jmlsisadr`,(`dod`.`jmlbarang` - (`dod`.`jmlsi` / `dod`.`nilaisatuan`)) AS `jmlsisasi`,((`dod`.`jmlbarang` - `dod`.`jmlrealisasi`) / `dod`.`nilaisatuan`) AS `jmlsisarealisasi`, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan, i.basset, `do`.docustomer, c3.kkode as docustomerkode, c3.knama as docustomernama, c3.ktingkatjual, `do`.domatauang, `do`.dokurs, `do`.dohargatermasukpajak, `do`.dotgljatuhtempo, c3.kpkp, t1.takunbeli as pajak1akunbeli, t1c1.cnama as pajak1akunbelinama, t1.takunjual as pajak1akunjual, t1c2.cnama as pajak1akunjualnama, t2.takunbeli as pajak2akunbeli, t2c1.cnama as pajak2akunbelinama, t2.takunjual as pajak2akunjual, t2c2.cnama as pajak2akunjualnama, i.bkp, d.dnama AS divisinama, sd.sdnama AS subdivisinama, cc.ccnama AS costcenternama, p.pnama AS proyeknama  from `m5_do_detail` `dod` left join `m5_do` `do` on `dod`.`iddo` = `do`.`doid` left join `m1_terms` `tr` on `do`.`dotermin` = `tr`.`trkode` left join `m1_contact` `c1` on `do`.`dobagianpenjualan` = `c1`.`kid` left join `m1_contact` `c2` on `do`.`dobagianpengiriman` = `c2`.`kid` left join `m1_expedition` `e` on `do`.`doekspedisi` = `e`.`ekode` left join `m1_item` `i` on `dod`.`idbarang` = `i`.`bid` left join `m1_tax` `t1` on `dod`.`pajak1` = `t1`.`tkode` left join `m1_tax` `t2` on `dod`.`pajak2` = `t2`.`tkode` left join m1_contact c3 on `do`.docustomer = c3.kid left join m1_coa t1c1 on t1.takunbeli = t1c1.cnomor left join m1_coa t1c2 on t1.takunjual = t1c2.cnomor left join m1_coa t2c1 on t2.takunbeli = t2c1.cnomor left join m1_coa t2c2 on t2.takunjual = t2c2.cnomor LEFT JOIN m1_division d ON d.dkode = dod.divisi LEFT JOIN m1_subdivision sd ON sd.sdkode = dod.subdivisi LEFT JOIN m1_cost_center cc ON cc.cckode = dod.costcenter LEFT JOIN m1_project p ON p.pkode = dod.proyek"

        dt = AmbilData("aplikasi1-M5_do_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , dol) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("iddodetail"), 0), sptField,
                     FxDB(dr("iddo"), 0), sptField,
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
                     FxDB(dr("idhppkhususmasuk"), 0), sptField,
                     FxDB(dr("idhppfifomasuk"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
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
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idsqdetail"), 0), sptField,
                     FxDB(dr("idsodetail"), 0), sptField,
                     FxDB(dr("idpidetail"), 0), sptField,
                     FxDB(dr("idpldetail"), 0), sptField,
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
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     FxDB(dr("donotransaksi"), ""), sptField,
                     FxDB(dr("douraian"), ""), sptField,
                     FxDB(dr("docatatan"), ""), sptField,
                     FxDB(dr("donoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dotglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("dotglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("docustomerkontak"), ""), sptField,
                     FxDB(dr("do1alamat1"), ""), sptField,
                     FxDB(dr("do1alamat2"), ""), sptField,
                     FxDB(dr("do1alamat3"), ""), sptField,
                     FxDB(dr("do2alamat1"), ""), sptField,
                     FxDB(dr("do2alamat2"), ""), sptField,
                     FxDB(dr("do2alamat3"), ""), sptField,
                     FxDB(dr("dobagianpenjualan"), 0), sptField,
                     FxDB(dr("dobagianpenjualankode"), ""), sptField,
                     FxDB(dr("dobagianpenjualannama"), ""), sptField,
                     FxDB(dr("dobagianpengirimankode"), ""), sptField,
                     FxDB(dr("dobagianpengirimannama"), ""), sptField,
                     FxDB(dr("doekspedisi"), ""), sptField,
                     FxDB(dr("doekspedisinama"), ""), sptField,
                     FxDB(dr("dotermin"), ""), sptField,
                     FxDB(dr("doterminnama"), ""), sptField,
                     FxDB(dr("doterminharijatuhtempo"), 0), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bhppaverage"), 0), sptField,
                     FxDB(dr("bhargajual1"), 0), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("brekpenjualan"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("jmlsisadr"), 0), sptField,
                     FxDB(dr("jmlsisasi"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptField,
                     FxDB(dr("basset"), 0), sptField,
                     FxDB(dr("docustomer"), ""), sptField,
                     FxDB(dr("docustomerkode"), ""), sptField,
                     FxDB(dr("docustomernama"), ""), sptField,
                     FxDB(dr("ktingkatjual"), 0), sptField,
                     FxDB(dr("domatauang"), ""), sptField,
                     FxDB(dr("dokurs"), 0), sptField,
                     FxDB(dr("dohargatermasukpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("dotgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("kpkp"), 0), sptField,
                     FxDB(dr("pajak1akunbeli"), ""), sptField,
                     FxDB(dr("pajak1akunbelinama"), ""), sptField,
                     FxDB(dr("pajak1akunjual"), ""), sptField,
                     FxDB(dr("pajak1akunjualnama"), ""), sptField,
                     FxDB(dr("pajak2akunbeli"), ""), sptField,
                     FxDB(dr("pajak2akunbelinama"), ""), sptField,
                     FxDB(dr("pajak2akunjual"), ""), sptField,
                     FxDB(dr("pajak2akunjualnama"), ""), sptField,
                     FxDB(dr("bkp"), 0), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("iddodetail, iddo, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, idhppkhususmasuk, idhppfifomasuk, harga, hpp, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekhargapokok, rekdiskonpenjualan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpidetail, idpldetail, jmldr, statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, donotransaksi, douraian, docatatan, donoref, dotglnoref, dotglkirim, docustomerkontak, do1alamat1, do1alamat2, do1alamat3, do2alamat1, do2alamat2, do2alamat3, dobagianpenjualan, dobagianpenjualankode, dobagianpenjualannama, dobagianpengirimankode, dobagianpengirimannama, doekspedisi, doekspedisinama, dotermin, doterminnama, doterminharijatuhtempo, kodebarang, bhpp, bhppaverage, bhargajual1, bjenis, brekpenjualan, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisadr, jmlsisasi, jmlsisarealisasi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan, basset, docustomer, docustomerkode, docustomernama, ktingkatjual, domatauang, dokurs, dohargatermasukpajak, dotgljatuhtempo, kpkp, pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, pajak2akunjualnama, bkp, divisinama, subdivisinama, costcenternama, proyeknama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_DoTerkait(ByVal param As String) As String
        'M5_DoTerkait --------------------------------------------------------
        'doid, donotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "doid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2) & " AND doid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "doid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.m5_do_terkait(Filter)


        dt = AmbilData("aplikasi1-m5_do_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("doid"), 0), sptField,
                     FxDB(dr("donotransaksi"), ""), sptField,
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
            result(2) = "Related DO data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("doid, donotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    Private Function ValidasiHppI(ByVal dtdetail As DataTable, ByVal ftBarang As String) As String
        Dim errmessage As String = "", sql As String = ""

        Dim dtval As New DataTable, dtbarang As New DataTable, dtHppI As New DataTable, dtLookup As New DataTable
        Dim ftExistHppI As String = "", ftHppI As String = "", filterLookup As String = ""
        Dim kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaisatuan As Double = 0, urutan As Double = 0, sisa As Double = 0

        '1. AMBIL BARANG HPP KHUSUS (I)
        dtbarang = AsDataTableAmbilDariDB("SELECT bid, bkode FROM m1_item WHERE (bjenis <> 'J') AND (bhpp = 'I') AND (" & ftBarang & ")")
        '2. CEK ID HPP KHUSUS MASUK
        If dtbarang.Rows.Count > 0 Then
            '3. PERULANGAN SEBANYAK BARANG HPP KHUSUS
            For Each dr1 As DataRow In dtbarang.Rows
                '4. AMBIL BARANG HPP KHUSUS DARI DETAIL
                dtHppI = AsDataTableFilterSortDt(dtdetail, "idbarang = '" & dr1("bid") & "'")
                If dtHppI.Rows.Count > 0 Then
                    For Each dr2 As DataRow In dtHppI.Rows
                        '5. BUAT FILTER CEK DATA EXIST HPP KHUSUS
                        ftExistHppI = IIf(Len(ftExistHppI.ToString) = 0, "", ftExistHppI & " UNION ")
                        ftExistHppI = String.Concat(ftExistHppI, "SELECT EXISTS(SELECT 1 FROM m1_cogs_special_in WHERE idhppikm = '" & dr2("idhppkhususmasuk") & "' LIMIT 1) as rowExists, '" & dr1("bid") & "' as idbarang, bkode FROM m1_item WHERE bid = '" & dr1("bid") & "'")
                        '6. BUAT FILTER CEK JML HPP KHUSUS
                        Dim StokHppI As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idhppkhususmasuk=" & dr2("idhppkhususmasuk") & "")
                        ftHppI = IIf(Len(ftHppI.ToString) = 0, "", ftHppI & " OR ")
                        ftHppI = String.Concat(ftHppI, " (csi.idhppikm = " & dr2("idhppkhususmasuk") & " AND " & StokHppI & " > csi.sisa) ")
                    Next
                End If
            Next

            'VALIDASI HPP KHUSUS (I) ------------------------------------
            'CEK DATA EXIST/TIDAK
            If Len(ftExistHppI) > 0 Then
                dtval = AsDataTableAmbilDariDB(ftExistHppI) 'ftExistHppI = rowExists, idbarang, bkode
                filterLookup = "rowExists = 0"
                dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
                If dtval.Rows.Count > 0 Then
                    'Ambil informasi utk errmessage
                    kodebarang = dtval.Rows(0)("bkode")

                    filterLookup = "idbarang=" & dtval.Rows(0)("idbarang")
                    dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    urutan = dtLookup.Rows(0)("urutan")

                    errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS Special list." : GoTo selesai
                End If
            End If

            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA YG TERSEDIA
            If Len(ftHppI) > 0 Then
                sql = "SELECT csi.idhppikm, csi.idbarang, csi.sisa, i.bkode FROM m1_cogs_special_in csi JOIN m1_item i ON csi.idbarang = i.bid WHERE " & ftHppI
                dtval = AsDataTableAmbilDariDB(sql)
                If dtval.Rows.Count > 0 Then
                    'Ambil informasi utk errmessage
                    kodebarang = dtval.Rows(0)("bkode")
                    sisa = dtval.Rows(0)("sisa")

                    filterLookup = "idhppkhususmasuk=" & dtval.Rows(0)("idhppikm")
                    dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                    If dtLookup.Rows.Count > 0 Then
                        tipebarang = dtLookup.Rows(0)("tipebarang")
                        namabarang = dtLookup.Rows(0)("namabarang")
                        satuan = dtLookup.Rows(0)("satuan")
                        nilaisatuan = dtLookup.Rows(0)("nilaiSatuan")
                        urutan = dtLookup.Rows(0)("urutan")
                    End If
                    errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of stock in COGS Special, item(s) available " & sisa / nilaisatuan & " " & satuan : GoTo selesai
                End If
            End If
            'END OF VALIDASI HPP KHUSUS (I) -----------------------------
        End If

selesai:
        Return errmessage
    End Function

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstandingSO As String, ByVal ftOutstandingSO As String, ByVal ftExistOutstandingPI As String, ByVal ftOutstandingPI As String, ByVal ftExistOutstandingPL As String, ByVal ftOutstandingPL As String, ByVal ftExistStok As String, ByVal ftStok As String, ByVal ftStokAvailable As String, ByVal ftExistBatch As String, ByVal ftBatch As String, ByVal ftExistSerial As String, ByVal ftSerial As String, ByVal gudangBatchSerial As String, ByVal ftSO As String, ByVal ftPI As String, ByVal ftPL As String, ByRef termasukPajak As String) As String
        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaiSatuan As Double = 0, sisa As Double = 0
        Dim filterLookup As String = "", urutan As String = "", gudang As String = "", noBatch As String = "", noSerial As String = ""

        'VALIDASI OUTSTANDING ---------------------------------------
        'SO
        If Len(ftExistOutstandingSO) > 0 Then 'ftExistOutstanding = rowExists, idsodetail, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingSO)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "idsodetail=" & dtval.Rows(0)("idsodetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in SO" : GoTo selesai
            End If

            'CEK SO YANG DIAMBIL
            'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
            If Len(ftSO) > 0 Then
                sql = "SELECT so.sonotransaksi as notransaksi, so.sohargatermasukpajak as termasukpajak, (CASE so.sohargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajaknama FROM m5_so_detail sod JOIN m5_so so ON sod.idso = so.soid WHERE " & ftSO & " GROUP BY so.sohargatermasukpajak"
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
                    sql = "SELECT i.bkode, sod.idsodetail, so.sonotransaksi as notransaksi, (CASE so.sohargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m5_so_detail sod JOIN m5_so so ON sod.idso = so.soid JOIN m1_item i ON sod.idbarang = i.bid WHERE (" & ftSO & ") AND so.sohargatermasukpajak <> " & termasukPajak & " ORDER BY sod.urutan"
                    dtval = AsDataTableAmbilDariDB(sql)
                    If dtval.Rows.Count > 0 Then
                        'Ambil informasi utk errmessage
                        kodebarang = dtval.Rows(0)("bkode")

                        filterLookup = "idsodetail = " & dtval.Rows(0)("idsodetail")
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
            'sql = "SELECT sod.idsodetail, (sod.jmlbarang - sod.jmlrealisasi) as sisarealisasi, i.bid, i.bkode FROM m5_so_detail AS sod INNER JOIN m1_item AS i ON sod.idbarang = i.bid WHERE " & ftOutstandingSO
            'dtval = AsDataTableAmbilDariDB(sql)
            'If dtval.Rows.Count > 0 Then
            '    'Ambil informasi utk errmessage
            '    kodebarang = dtval.Rows(0)("bkode")
            '    sisa = dtval.Rows(0)("sisarealisasi")

            '    filterLookup = "idsodetail=" & dtval.Rows(0)("idsodetail")
            '    dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
            '    If dtLookup.Rows.Count > 0 Then
            '        tipebarang = dtLookup.Rows(0)("tipebarang")
            '        namabarang = dtLookup.Rows(0)("namabarang")
            '        satuan = dtLookup.Rows(0)("satuan")
            '        nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
            '        urutan = dtLookup.Rows(0)("urutan")
            '    End If
            '    errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in SO, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            'End If
        End If

        'PI
        If Len(ftExistOutstandingPI) > 0 Then 'ftExistOutstanding = rowExists, idpidetail, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingPI)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "idpidetail=" & dtval.Rows(0)("idpidetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in PI" : GoTo selesai
            End If

            'CEK PI YANG DIAMBIL
            'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
            If Len(ftPI) > 0 Then
                sql = "SELECT pi.pinotransaksi as notransaksi, pi.pihargatermasukpajak as termasukpajak, (CASE pi.pihargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajaknama FROM m5_pi_detail pid JOIN m5_pi pi ON pid.idpi = pi.piid WHERE " & ftPI & " GROUP BY pi.pihargatermasukpajak"
                dtval = AsDataTableAmbilDariDB(sql)
                If dtval.Rows.Count > 1 Then
                    errmessage = "Include Tax Price can't join with Exclude Tax Price as one Transaction"
                    For Each dr1 As DataRow In dtval.Rows
                        errmessage &= ", " & dr1("notransaksi") & " " & dr1("termasukpajaknama")
                    Next
                    GoTo selesai

                ElseIf dtval.Rows.Count = 1 Then
                    If Len(dtval.Rows(0)("termasukpajak")) > 0 Then
                        If Len(ftExistOutstandingSO) > 0 Then
                            If Integer.Parse(termasukPajak) <> Integer.Parse(dtval.Rows(0)("termasukpajak")) Then
                                errmessage = "Include Tax Price can't join with Exclude Tax Price as one Transaction (SO and PI)" : GoTo selesai
                            End If
                        Else
                            termasukPajak = Integer.Parse(dtval.Rows(0)("termasukpajak"))
                        End If

                    End If

                End If

                'CEK TRANSAKSI HARGA TERMASUK PAJAK TIDAK BOLEH AMBIL TRANSAKSI HARGA TIDAK TERMASUK PAJAK, DAN SEBALIKNYA
                If Len(termasukPajak) > 0 Then
                    sql = "SELECT i.bkode, pid.idpidetail, pi.pinotransaksi as notransaksi, (CASE pi.pihargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m5_pi_detail pid JOIN m5_pi pi ON pid.idpi = pi.piid JOIN m1_item i ON pid.idbarang = i.bid WHERE (" & ftPI & ") AND pi.pihargatermasukpajak <> " & termasukPajak & " ORDER BY pid.urutan"
                    dtval = AsDataTableAmbilDariDB(sql)
                    If dtval.Rows.Count > 0 Then
                        'Ambil informasi utk errmessage
                        kodebarang = dtval.Rows(0)("bkode")

                        filterLookup = "idpidetail = " & dtval.Rows(0)("idpidetail")
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
            sql = "SELECT pid.idpidetail, (pid.jmlbarang - pid.jmlrealisasi) as sisarealisasi, i.bid, i.bkode FROM m5_pi_detail AS pid INNER JOIN m1_item AS i ON pid.idbarang = i.bid WHERE " & ftOutstandingPI
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisarealisasi")

                filterLookup = "idpidetail=" & dtval.Rows(0)("idpidetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in PI, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If

        'PL
        If Len(ftExistOutstandingPL) > 0 Then 'ftExistOutstanding = rowExists, idpldetail, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingPL)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "idpldetail=" & dtval.Rows(0)("idpldetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in PL" : GoTo selesai
            End If

            'CEK PL YANG DIAMBIL
            'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
            If Len(ftPL) > 0 Then
                sql = "SELECT pl.plnotransaksi as notransaksi, pl.plhargatermasukpajak as termasukpajak, (CASE pl.plhargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajaknama FROM m5_pl_detail pld JOIN m5_pl pl ON pld.idpl = pl.plid WHERE " & ftPL & " GROUP BY pl.plhargatermasukpajak"
                dtval = AsDataTableAmbilDariDB(sql)
                If dtval.Rows.Count > 1 Then
                    errmessage = "Include Tax Price can't join with Exclude Tax Price as one Transaction"
                    For Each dr1 As DataRow In dtval.Rows
                        errmessage &= ", " & dr1("notransaksi") & " " & dr1("termasukpajaknama")
                    Next
                    GoTo selesai

                ElseIf dtval.Rows.Count = 1 Then
                    If Len(dtval.Rows(0)("termasukpajak")) > 0 Then
                        If Len(ftExistOutstandingSO) > 0 And Len(ftExistOutstandingPI) > 0 Then
                            If Integer.Parse(termasukPajak) <> Integer.Parse(dtval.Rows(0)("termasukpajak")) Then
                                errmessage = "Include Tax Price can't join with Exclude Tax Price as one Transaction (PI and PL)" : GoTo selesai
                            End If

                        ElseIf Len(ftExistOutstandingSO) > 0 Then
                            If Integer.Parse(termasukPajak) <> Integer.Parse(dtval.Rows(0)("termasukpajak")) Then
                                errmessage = "Include Tax Price can't join with Exclude Tax Price as one Transaction (SO and PL)" : GoTo selesai
                            End If

                        ElseIf Len(ftExistOutstandingPI) > 0 Then
                            If Integer.Parse(termasukPajak) <> Integer.Parse(dtval.Rows(0)("termasukpajak")) Then
                                errmessage = "Include Tax Price can't join with Exclude Tax Price as one Transaction (PI and PL)" : GoTo selesai
                            End If

                        Else
                            termasukPajak = Integer.Parse(dtval.Rows(0)("termasukpajak"))
                        End If

                    End If

                End If

                'CEK TRANSAKSI HARGA TERMASUK PAJAK TIDAK BOLEH AMBIL TRANSAKSI HARGA TIDAK TERMASUK PAJAK, DAN SEBALIKNYA
                If Len(termasukPajak) > 0 Then
                    sql = "SELECT i.bkode, pld.idpldetail, pl.plnotransaksi as notransaksi, (CASE pl.plhargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m5_pl_detail pld JOIN m5_pl pl ON pld.idpl = pl.plid JOIN m1_item i ON pld.idbarang = i.bid WHERE (" & ftPL & ") AND pl.plhargatermasukpajak <> " & termasukPajak & " ORDER BY pld.urutan"
                    dtval = AsDataTableAmbilDariDB(sql)
                    If dtval.Rows.Count > 0 Then
                        'Ambil informasi utk errmessage
                        kodebarang = dtval.Rows(0)("bkode")

                        filterLookup = "idpldetail = " & dtval.Rows(0)("idpldetail")
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
            sql = "SELECT pld.idpldetail, (pld.jmlbarang - pld.jmlrealisasi) as sisarealisasi, i.bid, i.bkode FROM m5_pl_detail AS pld INNER JOIN m1_item AS i ON pld.idbarang = i.bid WHERE " & ftOutstandingPL
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisarealisasi")

                filterLookup = "idpldetail=" & dtval.Rows(0)("idpldetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in PL, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If
        'END OF VALIDASI OUTSTANDING --------------------------------


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

            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA STOK AVAILABLE PERGUDANG YG TERSEDIA
            If Len(ftStokAvailable) > 0 Then
                'sql = "SELECT isw.idbarang, isw.kgudang, isw.stok - IFNULL(isb.jmlbooking,0) as stok, i.bkode FROM m1_item_stock_warehouse isw JOIN m1_item i ON isw.idbarang = i.bid AND i.bjenis <> 'J' LEFT JOIN m1_item_booking isb ON isw.idbarang = isb.idbarang WHERE " & ftStokAvailable
                sql = "SELECT isw.idbarang, isw.kgudang, isw.stok - IFNULL(isb.jmlbooking,0) as stok, i.bkode FROM m1_item_stock_warehouse isw JOIN m1_item i ON isw.idbarang = i.bid AND i.bjenis <> 'J' LEFT JOIN m1_warehouse w ON isw.kgudang = w.wkode LEFT JOIN m1_item_booking isb ON isw.idbarang = isb.idbarang AND w.wbookingstok = 1 WHERE " & ftStokAvailable
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


        'VALIDASI BATCH ---------------------------------------------
        'CEK DATA EXIST/TIDAK
        If Len(ftExistBatch) > 0 Then
            dtval = AsDataTableAmbilDariDB(ftExistBatch) 'ftExistBatch = rowExists, idbarang, bkode, nbikode, nbigudang
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                noBatch = dtval.Rows(0)("nbikode")
                gudang = dtval.Rows(0)("nbigudang")

                filterLookup = "idbarang = " & dtval.Rows(0)("idbarang") & " AND " & gudangBatchSerial & " = '" & dtval.Rows(0)("nbigudang") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " | No. Batch : " & noBatch & " doesn't exists in No. Batch list." : GoTo selesai
            End If
        End If

        'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA BATCH YG TERSEDIA
        If Len(ftBatch) > 0 Then
            sql = "SELECT nbi.nbiidbarang, nbi.nbikode, nbi.nbigudang, nbi.nbijmlsisa, i.bkode FROM m1_no_batch_in nbi JOIN m1_item i ON nbi.nbiidbarang = i.bid WHERE " & ftBatch
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("nbijmlsisa")
                noBatch = dtval.Rows(0)("nbikode")
                gudang = dtval.Rows(0)("nbigudang")

                filterLookup = "idbarang = " & dtval.Rows(0)("nbiidbarang") & " AND " & gudangBatchSerial & " = '" & dtval.Rows(0)("nbigudang") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Test sisa :" & sisa & " nilaisatuan " & nilaiSatuan & "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " | No. Batch : " & noBatch & " exceeds the number of stock in No. Batch list, item(s) available " & sisa / nilaiSatuan & " " & satuan & "  " : GoTo selesai
            End If
        End If
        'END OF VALIDASI BATCH --------------------------------------

        'VALIDASI SERIAL ---------------------------------------------
        'CEK DATA EXIST/TIDAK
        If Len(ftExistSerial) > 0 Then
            dtval = AsDataTableAmbilDariDB(ftExistSerial) 'ftExistSerial = rowExists, idbarang, bkode, nsikode, nsigudang
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                noSerial = dtval.Rows(0)("nsikode")
                gudang = dtval.Rows(0)("nsigudang")

                filterLookup = "idbarang = " & dtval.Rows(0)("idbarang") & " AND " & gudangBatchSerial & " = '" & dtval.Rows(0)("nsigudang") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " | No. Serial : " & noSerial & " doesn't exists in No. Serial list." : GoTo selesai
            End If
        End If

        'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA SERIAL YG TERSEDIA
        If Len(ftSerial) > 0 Then
            sql = "SELECT nsi.nsiidbarang, nsi.nsikode, nsi.nsigudang, nsi.nsijmlsisa, i.bkode FROM m1_no_serial_in nsi JOIN m1_item i ON nsi.nsiidbarang = i.bid WHERE " & ftSerial
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("nsijmlsisa")
                noSerial = dtval.Rows(0)("nsikode")
                gudang = dtval.Rows(0)("nsigudang")

                filterLookup = "idbarang = " & dtval.Rows(0)("nsiidbarang") & " AND " & gudangBatchSerial & " = '" & dtval.Rows(0)("nsigudang") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " | No. Serial : " & noSerial & " exceeds the number of stock in No. Serial list, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If
        'END OF VALIDASI SERIAL --------------------------------------

selesai:
        Return errmessage
    End Function

    <WebMethod()>
    Public Function M5_DoSimpanOld(ByVal param As String) As String
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
        'doid(0) As Integer, docabang(1) As String, dolokasi(2) As String, dogudang(3) As String, doasalbarang(4) As String, 
        'doasalbarangkategori(5) As Integer, dojenispenjualan(6) As String, dojenispenjualankategori(7) As Integer, docarabayar(8) As Integer, dosumber(9) As String, 
        'doautonotransaksi(10) As Integer, donotransaksi(11) As String, dotgl(12) As Date, dokodepa(13) As Integer, docustomer(14) As Integer, 
        'docustomerkontak(15) As String, do1alamat1(16) As String, do1alamat2(17) As String, do1alamat3(18) As String, do2alamat1(19) As String, 
        'do2alamat2(20) As String, do2alamat3(21) As String, dobagianpenjualan(22) As Integer, dobagianpengiriman(23) As Integer, doekspedisi(24) As String, 
        'dotglkirim(25) As Date, dotermin(26) As String, dotgljatuhtempo(27) As Date, douraian(28) As String, docatatan(29) As String, 
        'donoref(30) As String, dotglnoref(31) As Date, dotglpenutupan(32) As Date, domatauang(33) As String, dokurs(34) As Double, 
        'dohargatermasukpajak(35) As Integer, dototal(36) As Double, dodiskonpersen(37) As String, dojmldiskon(38) As Double, dototalpajak1detail(39) As Double, 
        'dototalpajak2detail(40) As Double, dobiayalainpersen(41) As Double, dobiayalain(42) As Double, dototaltransaksi(43) As Double, dorekdiskon(44) As String, 
        'dorekpajak1(45) As String, dorekpajak2(46) As String, dorekbiayalain(47) As String, doidsq(48) As Integer, doidso(49) As Integer, 
        'doidpi(50) As Integer, doidpl(51) As Integer, dostatusdr(52) As Integer, dostatussi(53) As Integer, dostatusrnr(54) As Integer, 
        'dostatussr(55) As Integer, dostatus(56) As Integer, dostatussebelumnya(57) As Integer, dojmlrevisi(58) As Integer, docetakanke(59) As Integer, 
        'doinputuser(60) As Integer, doinputtgl(61) As DateTime, domodifikasiuser(62) As Integer, domodifikasitgl(63) As DateTime, doposting(64) As Integer, 
        'dotutupperiode(65) As Integer, doisclose(66) As Integer, docustomtext1(67) As String, docustomtext2(68) As String, docustomtext3(69) As String, 
        'docustomtext4(70) As String, docustomtext5(71) As String, docustomint1(72) As Integer, docustomint2(73) As Integer, docustomint3(74) As Integer, 
        'docustomdbl1(75) As Double, docustomdbl2(76) As Double, docustomdbl3(77) As Double, docustomdate1(78) As Date, docustomdate2(79) As Date, 
        'docustomdate3(80) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'doid, docabang, dolokasi, dogudang, doasalbarang, doasalbarangkategori, dojenispenjualan, 
        'dojenispenjualankategori, docarabayar, dosumber, doautonotransaksi, donotransaksi, dotgl, dokodepa, 
        'docustomer, docustomerkontak, do1alamat1, do1alamat2, do1alamat3, do2alamat1, do2alamat2, 
        'do2alamat3, dobagianpenjualan, dobagianpengiriman, doekspedisi, dotglkirim, dotermin, dotgljatuhtempo, 
        'douraian, docatatan, donoref, dotglnoref, dotglpenutupan, domatauang, dokurs, 
        'dohargatermasukpajak, dototal, dodiskonpersen, dojmldiskon, dototalpajak1detail, dototalpajak2detail, dobiayalainpersen, 
        'dobiayalain, dototaltransaksi, dorekdiskon, dorekpajak1, dorekpajak2, dorekbiayalain, doidsq, 
        'doidso, doidpi, doidpl, dostatusdr, dostatussi, dostatusrnr, dostatussr, 
        'dostatus, dostatussebelumnya, dojmlrevisi, docetakanke, doinputuser, doinputtgl, domodifikasiuser, 
        'domodifikasitgl, doposting, dotutupperiode, doisclose, docustomtext1, docustomtext2, docustomtext3, 
        'docustomtext4, docustomtext5, docustomint1, docustomint2, docustomint3, docustomdbl1, docustomdbl2, 
        'docustomdbl3, docustomdate1, docustomdate2, docustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 81) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'doid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "doid required numeric." : GoTo selesai
        End If
        'doasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "doasalbarangkategori required numeric." : GoTo selesai
        End If
        'dojenispenjualankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "dojenispenjualankategori required numeric." : GoTo selesai
        End If
        'docarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "docarabayar required numeric." : GoTo selesai
        End If
        'doautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "doautonotransaksi required numeric." : GoTo selesai
        End If
        'dotgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "dotgl required date." : GoTo selesai
        End If
        'dokodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "dokodepa required numeric." : GoTo selesai
        End If
        'docustomer(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "docustomer required numeric." : GoTo selesai
        End If
        'dobagianpenjualan(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "dobagianpenjualan required numeric." : GoTo selesai
        End If
        'dobagianpengiriman(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "dobagianpengiriman required numeric." : GoTo selesai
        End If
        'dotglkirim(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "dotglkirim required date." : GoTo selesai
        End If
        'dotgljatuhtempo(27) As Date
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "dotgljatuhtempo required date." : GoTo selesai
        End If
        'dotglnoref(31) As Date
        If (IsDate(dataUtama(31)) = False) Then
            result(2) = "dotglnoref required date." : GoTo selesai
        End If
        'dotglpenutupan(32) As Date
        If (IsDate(dataUtama(32)) = False) Then
            result(2) = "dotglpenutupan required date." : GoTo selesai
        End If
        'dokurs(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "dokurs required numeric." : GoTo selesai
        End If
        'dohargatermasukpajak(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "dohargatermasukpajak required numeric." : GoTo selesai
        End If
        'dototal(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "dototal required numeric." : GoTo selesai
        End If
        'dojmldiskon(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "dojmldiskon required numeric." : GoTo selesai
        End If
        'dototalpajak1detail(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "dototalpajak1detail required numeric." : GoTo selesai
        End If
        'dototalpajak2detail(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "dototalpajak2detail required numeric." : GoTo selesai
        End If
        ''dobiayalainpersen(41) As Double
        'If (IsNumeric(dataUtama(41)) = False) Then
        '    result(2) = "dobiayalainpersen required numeric." : GoTo selesai
        'End If
        'dobiayalain(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "dobiayalain required numeric." : GoTo selesai
        End If
        'dototaltransaksi(43) As Double
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "dototaltransaksi required numeric." : GoTo selesai
        End If
        'doidsq(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "doidsq required numeric." : GoTo selesai
        End If
        'doidso(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "doidso required numeric." : GoTo selesai
        End If
        'doidpi(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "doidpi required numeric." : GoTo selesai
        End If
        'doidpl(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "doidpl required numeric." : GoTo selesai
        End If
        'dostatusdr(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "dostatusdr required numeric." : GoTo selesai
        End If
        'dostatussi(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "dostatussi required numeric." : GoTo selesai
        End If
        'dostatusrnr(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "dostatusrnr required numeric." : GoTo selesai
        End If
        'dostatussr(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "dostatussr required numeric." : GoTo selesai
        End If
        'dostatus(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "dostatus required numeric." : GoTo selesai
        End If
        'dostatussebelumnya(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "dostatussebelumnya required numeric." : GoTo selesai
        End If
        'dojmlrevisi(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "dojmlrevisi required numeric." : GoTo selesai
        End If
        'docetakanke(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "docetakanke required numeric." : GoTo selesai
        End If
        'doinputuser(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "doinputuser required numeric." : GoTo selesai
        End If
        'doinputtgl(61) As DateTime
        If (IsDate(dataUtama(61)) = False) Then
            result(2) = "doinputtgl required date." : GoTo selesai
        End If
        'domodifikasiuser(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "domodifikasiuser required numeric." : GoTo selesai
        End If
        'domodifikasitgl(63) As DateTime
        If (IsDate(dataUtama(63)) = False) Then
            result(2) = "domodifikasitgl required date." : GoTo selesai
        End If
        'doposting(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "doposting required numeric." : GoTo selesai
        End If
        'dotutupperiode(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "dotutupperiode required numeric." : GoTo selesai
        End If
        'doisclose(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "doisclose required numeric." : GoTo selesai
        End If
        'docustomint1(72) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "docustomint1 required numeric." : GoTo selesai
        End If
        'docustomint2(73) As Integer
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "docustomint2 required numeric." : GoTo selesai
        End If
        'docustomint3(74) As Integer
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "docustomint3 required numeric." : GoTo selesai
        End If
        'docustomdbl1(75) As Double
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "docustomdbl1 required numeric." : GoTo selesai
        End If
        'docustomdbl2(76) As Double
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "docustomdbl2 required numeric." : GoTo selesai
        End If
        'docustomdbl3(77) As Double
        If (IsNumeric(dataUtama(77)) = False) Then
            result(2) = "docustomdbl3 required numeric." : GoTo selesai
        End If
        'docustomdate1(78) As Date
        If (IsDate(dataUtama(78)) = False) Then
            result(2) = "docustomdate1 required date." : GoTo selesai
        End If
        'docustomdate2(79) As Date
        If (IsDate(dataUtama(79)) = False) Then
            result(2) = "docustomdate2 required date." : GoTo selesai
        End If
        'docustomdate3(80) As Date
        If (IsDate(dataUtama(80)) = False) Then
            result(2) = "docustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'docabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "docabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "docabang should not be more than 25 character." : GoTo selesai
        End If

        'dolokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "dolokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "dolokasi should not be more than 25 character." : GoTo selesai
        End If

        'dogudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "dogudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "dogudang should not be more than 25 character." : GoTo selesai
        End If

        'dosumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "dosumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "dosumber should not be more than 10 character." : GoTo selesai
        End If

        'donotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "donotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "donotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'dotgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "dotgl can't be empty" : GoTo selesai
        End If

        'dotglkirim(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "dotglkirim can't be empty" : GoTo selesai
        End If

        'dotgljatuhtempo(27) As Date
        If Len(dataUtama(27)) = 0 Then
            result(2) = "dotgljatuhtempo can't be empty" : GoTo selesai
        End If

        'dotglnoref(31) As Date
        If Len(dataUtama(31)) = 0 Then
            result(2) = "dotglnoref can't be empty" : GoTo selesai
        End If

        'dotglpenutupan(32) As Date
        If Len(dataUtama(32)) = 0 Then
            result(2) = "dotglpenutupan can't be empty" : GoTo selesai
        End If

        'domatauang(33) As String
        If Len(dataUtama(33)) = 0 Then
            result(2) = "domatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(33)) > 25 Then
            result(2) = "domatauang should not be more than 25 character." : GoTo selesai
        End If

        'dokurs(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "dokurs can't be empty" : GoTo selesai
        End If

        'dototal(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "dototal can't be empty" : GoTo selesai
        End If

        'dodiskonpersen(37) As String
        If Len(dataUtama(37)) = 0 Then
            result(2) = "dodiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(37)) > 25 Then
            result(2) = "dodiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'dojmldiskon(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "dojmldiskon can't be empty" : GoTo selesai
        End If

        'dototalpajak1detail(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "dototalpajak1detail can't be empty" : GoTo selesai
        End If

        'dototalpajak2detail(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "dototalpajak2detail can't be empty" : GoTo selesai
        End If

        'dobiayalainpersen(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "dobiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(41)) > 25 Then
            result(2) = "dobiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'dobiayalain(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "dobiayalain can't be empty" : GoTo selesai
        End If

        'dototaltransaksi(43) As Double
        If Len(dataUtama(43)) = 0 Then
            result(2) = "dototaltransaksi can't be empty" : GoTo selesai
        End If

        'doinputtgl(61) As DateTime
        If Len(dataUtama(61)) = 0 Then
            result(2) = "doinputtgl can't be empty" : GoTo selesai
        End If

        'domodifikasitgl(63) As DateTime
        If Len(dataUtama(63)) = 0 Then
            result(2) = "domodifikasitgl can't be empty" : GoTo selesai
        End If

        'docustomdbl1(75) As Double
        If Len(dataUtama(75)) = 0 Then
            result(2) = "docustomdbl1 can't be empty" : GoTo selesai
        End If

        'docustomdbl2(76) As Double
        If Len(dataUtama(76)) = 0 Then
            result(2) = "docustomdbl2 can't be empty" : GoTo selesai
        End If

        'docustomdbl3(77) As Double
        If Len(dataUtama(77)) = 0 Then
            result(2) = "docustomdbl3 can't be empty" : GoTo selesai
        End If

        'docustomdate1(78) As Date
        If Len(dataUtama(78)) = 0 Then
            result(2) = "docustomdate1 can't be empty" : GoTo selesai
        End If

        'docustomdate2(79) As Date
        If Len(dataUtama(79)) = 0 Then
            result(2) = "docustomdate2 can't be empty" : GoTo selesai
        End If

        'docustomdate3(80) As Date
        If Len(dataUtama(80)) = 0 Then
            result(2) = "docustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "doid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "docabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dolokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dogudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "doasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "doasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dojenispenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dojenispenjualankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "docarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dosumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "doautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "donotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dotgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dokodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "docustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "docustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "do1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "do1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "do1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "do2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "do2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "do2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dobagianpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dobagianpengiriman", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "doekspedisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dotglkirim", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dotermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dotgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "douraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "docatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "donoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dotglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dotglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "domatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dokurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dohargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dototal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dodiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dojmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dototalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dototalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dobiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dobiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dototaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dorekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dorekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dorekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dorekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "doidsq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "doidso", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "doidpi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "doidpl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dostatusdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dostatussi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dostatusrnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dostatussr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dostatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dostatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dojmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "docetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "doinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "doinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "domodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "domodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "doposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dotutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "doisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "docustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "docustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "docustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "docustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "docustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "docustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "docustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "docustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "docustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "docustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "docustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "docustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "docustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "docustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "doid~docabang~dolokasi~dogudang~doasalbarang~doasalbarangkategori~dojenispenjualan~dojenispenjualankategori~docarabayar~dosumber~doautonotransaksi~donotransaksi~dotgl~dokodepa~docustomer~docustomerkontak~do1alamat1~do1alamat2~do1alamat3~do2alamat1~do2alamat2~do2alamat3~dobagianpenjualan~dobagianpengiriman~doekspedisi~dotglkirim~dotermin~dotgljatuhtempo~douraian~docatatan~donoref~dotglnoref~dotglpenutupan~domatauang~dokurs~dohargatermasukpajak~dototal~dodiskonpersen~dojmldiskon~dototalpajak1detail~dototalpajak2detail~dobiayalainpersen~dobiayalain~dototaltransaksi~dorekdiskon~dorekpajak1~dorekpajak2~dorekbiayalain~doidsq~doidso~doidpi~doidpl~dostatusdr~dostatussi~dostatusrnr~dostatussr~dostatus~dostatussebelumnya~dojmlrevisi~docetakanke~doinputuser~doinputtgl~domodifikasiuser~domodifikasitgl~doposting~dotutupperiode~doisclose~docustomtext1~docustomtext2~docustomtext3~docustomtext4~docustomtext5~docustomint1~docustomint2~docustomint3~docustomdbl1~docustomdbl2~docustomdbl3~docustomdate1~docustomdate2~docustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'iddodetail(0) As Integer, iddo(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, idhppkhususmasuk(12) As Integer, idhppfifomasuk(13) As Integer, harga(14) As Double, 
        'hpp(15) As Double, diskon(16) As String, jmldiskon(17) As Double, pajak1(18) As String, jmlpajak1(19) As Double, 
        'pajak2(20) As String, jmlpajak2(21) As Double, cabang(22) As String, lokasi(23) As String, gudangasal(24) As String, 
        'gudangtransit(25) As String, gudangtujuan(26) As String, rekpersediaan(27) As String, rekhargapokok(28) As String, rekdiskonpenjualan(29) As String, 
        'costcenter(30) As String, divisi(31) As String, subdivisi(32) As String, proyek(33) As String, catatan(34) As String, 
        'urutan(35) As Integer, idsqdetail(36) As Integer, idsodetail(37) As Integer, idpidetail(38) As Integer, idpldetail(39) As Integer, 
        'jmldr(40) As Double, statusdr(41) As Integer, jmlsi(42) As Double, statussi(43) As Integer, jmlrnr(44) As Double, 
        'statusrnr(45) As Integer, jmlsr(46) As Double, statussr(47) As Integer, isclose(48) As Integer, customtext1(49) As String, 
        'customtext2(50) As String, customtext3(51) As String, customdbl1(52) As Double, customdbl2(53) As Double, customdbl3(54) As Double, 
        'customdate1(55) As Date, customdate2(56) As Date, customdate3(57) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'iddodetail, iddo, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, idhppkhususmasuk, idhppfifomasuk, 
        'harga, hpp, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, 
        'jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, 
        'rekhargapokok, rekdiskonpenjualan, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idsqdetail, idsodetail, idpidetail, idpldetail, jmldr, statusdr, 
        'jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "iddodetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "iddo", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "idhppkhususmasuk", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idhppfifomasuk", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
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
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idsqdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idsodetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idpidetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idpldetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmldr", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusdr", AsEnumTypeData.AsInt64)
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

        'Variabel ValidasiBatchSerial
        Dim ftBarang As String = ""
        Dim idbarang As Integer = 0

        'Variabel ValidasiSimpan
        Dim ftExistOutstandingSO As String = "", ftOutstandingSO As String = "", updNilaiSO As String = "", updFilterSO As String = ""
        Dim ftExistOutstandingPI As String = "", ftOutstandingPI As String = "", updNilaiPI As String = "", updFilterPI As String = ""
        Dim ftExistOutstandingPL As String = "", ftOutstandingPL As String = "", updNilaiPL As String = "", updFilterPL As String = ""
        Dim idsodetail As Integer = 0, idpidetail As Integer = 0, idpldetail As Integer = 0, jmlbarang As Double = 0
        Dim ftExistStok As String = "", ftStok As String = "", ftStokAvailable As String = ""
        Dim updStokOut As String = "", gudangOut As String = "", updStokOutBooking As String = ""
        Dim updStokIn As String = "", gudangIn As String = ""

        'FILTER SO, PI, PL, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftSO As String = "", ftPI As String = "", ftPL As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 58) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'iddodetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - iddodetail required numeric." : GoTo selesai
            End If
            'iddo(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - iddo required numeric." : GoTo selesai
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
            'idhppkhususmasuk(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - idhppkhususmasuk required numeric." : GoTo selesai
            End If
            'idhppfifomasuk(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - idhppfifomasuk required numeric." : GoTo selesai
            End If
            'harga(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'hpp(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - hpp required numeric." : GoTo selesai
            End If
            'jmldiskon(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - jmldiskon required numeric." : GoTo selesai
            End If
            'jmlpajak1(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak1 required numeric." : GoTo selesai
            End If
            'jmlpajak2(21) As Double
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak2 required numeric." : GoTo selesai
            End If
            'urutan(35) As Integer
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idsqdetail(36) As Integer
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - idsqdetail required numeric." : GoTo selesai
            End If
            'idsodetail(37) As Integer
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - idsodetail required numeric." : GoTo selesai
            End If
            'idpidetail(38) As Integer
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - idpidetail required numeric." : GoTo selesai
            End If
            'idpldetail(39) As Integer
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - idpldetail required numeric." : GoTo selesai
            End If
            'jmldr(40) As Double
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - jmldr required numeric." : GoTo selesai
            End If
            'statusdr(41) As Integer
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - statusdr required numeric." : GoTo selesai
            End If
            'jmlsi(42) As Double
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - jmlsi required numeric." : GoTo selesai
            End If
            'statussi(43) As Integer
            If (IsNumeric(dataRowDetail(43)) = False) Then
                result(2) = "Row : " & i & " - statussi required numeric." : GoTo selesai
            End If
            'jmlrnr(44) As Double
            If (IsNumeric(dataRowDetail(44)) = False) Then
                result(2) = "Row : " & i & " - jmlrnr required numeric." : GoTo selesai
            End If
            'statusrnr(45) As Integer
            If (IsNumeric(dataRowDetail(45)) = False) Then
                result(2) = "Row : " & i & " - statusrnr required numeric." : GoTo selesai
            End If
            'jmlsr(46) As Double
            If (IsNumeric(dataRowDetail(46)) = False) Then
                result(2) = "Row : " & i & " - jmlsr required numeric." : GoTo selesai
            End If
            'statussr(47) As Integer
            If (IsNumeric(dataRowDetail(47)) = False) Then
                result(2) = "Row : " & i & " - statussr required numeric." : GoTo selesai
            End If
            'isclose(48) As Integer
            If (IsNumeric(dataRowDetail(48)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(52) As Double
            If (IsNumeric(dataRowDetail(52)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(53) As Double
            If (IsNumeric(dataRowDetail(53)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(54) As Double
            If (IsNumeric(dataRowDetail(54)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(55) As Date
            If (IsDate(dataRowDetail(55)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(56) As Date
            If (IsDate(dataRowDetail(56)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(57) As Date
            If (IsDate(dataRowDetail(57)) = False) Then
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

            'hpp(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - hpp can't be empty" : GoTo selesai
            End If

            'diskon(16) As String
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - diskon can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(16)) > 25 Then
                result(2) = "Row : " & i & " - diskon should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskon(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskon can't be empty" : GoTo selesai
                'Else
                '    'HITUNG JMLDISKON : jml(5) As Double, harga(14) As Double, diskon(16) As String
                '    dataRowDetail(17) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(14)), FixQuotes(dataRowDetail(16).ToString))
            End If

            'jmlpajak1(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak1 can't be empty" : GoTo selesai
            End If

            'jmlpajak2(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak2 can't be empty" : GoTo selesai
            End If

            'gudangasal(24) As String
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - gudangasal can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(24)) > 25 Then
                result(2) = "Row : " & i & " - gudangasal should not be more than 25 character." : GoTo selesai
            End If

            'gudangtransit(25) As String
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - gudangtransit can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(25)) > 25 Then
                result(2) = "Row : " & i & " - gudangtransit should not be more than 25 character." : GoTo selesai
            End If

            'gudangtujuan(26) As String
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - gudangtujuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(26)) > 25 Then
                result(2) = "Row : " & i & " - gudangtujuan should not be more than 25 character." : GoTo selesai
            End If

            'jmldr(40) As Double
            If Len(dataRowDetail(40)) = 0 Then
                result(2) = "Row : " & i & " - jmldr can't be empty" : GoTo selesai
            End If

            'jmlsi(42) As Double
            If Len(dataRowDetail(42)) = 0 Then
                result(2) = "Row : " & i & " - jmlsi can't be empty" : GoTo selesai
            End If

            'jmlrnr(44) As Double
            If Len(dataRowDetail(44)) = 0 Then
                result(2) = "Row : " & i & " - jmlrnr can't be empty" : GoTo selesai
            End If

            'jmlsr(46) As Double
            If Len(dataRowDetail(46)) = 0 Then
                result(2) = "Row : " & i & " - jmlsr can't be empty" : GoTo selesai
            End If

            'customdbl1(52) As Double
            If Len(dataRowDetail(52)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(53) As Double
            If Len(dataRowDetail(53)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(54) As Double
            If Len(dataRowDetail(54)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(55) As Date
            If Len(dataRowDetail(55)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(56) As Date
            If Len(dataRowDetail(56)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(57) As Date
            If Len(dataRowDetail(57)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "iddodetail~iddo~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~idhppkhususmasuk~idhppfifomasuk~harga~hpp~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudangasal~gudangtransit~gudangtujuan~rekpersediaan~rekhargapokok~rekdiskonpenjualan~costcenter~divisi~subdivisi~proyek~catatan~urutan~idsqdetail~idsodetail~idpidetail~idpldetail~jmldr~statusdr~jmlsi~statussi~jmlrnr~statusrnr~jmlsr~statussr~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56) & "~" & dataRowDetail(57)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'Set variabel -----------------------------------------------
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , gudangasal(24) As String      , gudangtransit(25) As String
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : gudangOut = dataRowDetail(24) : gudangIn = dataRowDetail(25)
            'idsodetail(37) As Integer     , idpidetail(38) As Integer      , idpldetail(39) As Integer
            idsodetail = dataRowDetail(37) : idpidetail = dataRowDetail(38) : idpldetail = dataRowDetail(39)

            'ValidasiBatchSerial
            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")

            'BUAT FILTER UNTUK VALIDASI ---------------------------------

            'VALIDASI STOK #1, CEK STOK ADA ATAU TIDAK
            ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
            ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")
            Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangasal='" & gudangOut & "'")

            'VALIDASI STOK DIBAGI MENJADI 2 JENIS, YAKNI :
            'VALIDASI STOK #1, JIKA TERKAIT DARI SO MAKA CEK STOK PERGUDANG (TOTAL STOK PERGUDANG), KEMUDIAN KURANGI JMLBOOKING
            'VALIDASI STOK #2, JIKA TIDAK TERKAIT DARI SO MAKA CEK STOK AVAILABLE PERGUDANG (TOTAL STOK PERGUDANG - STOK BOOKING)


            'VALIDASI OUTSTANDING -------------------------
            If idsodetail <> 0 Then 'SO

                'CEK SO YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftSO = IIf(Len(ftSO.ToString) = 0, "", ftSO & " OR ")
                ftSO = String.Concat(ftSO, " (sod.idsodetail = " & idsodetail & ") ")

                If idpidetail = 0 And idpldetail = 0 Then
                    '1. CEK DATA EXIST ------------------------
                    ftExistOutstandingSO = IIf(Len(ftExistOutstandingSO.ToString) = 0, "", ftExistOutstandingSO & " UNION ")
                    'ftExistOutstandingSO = String.Concat(ftExistOutstandingSO, "SELECT EXISTS(SELECT 1 FROM m5_so_detail JOIN m5_so ON idso = soid WHERE idsodetail = '" & idsodetail & "' AND (sostatus = 2 OR sostatus = 3 OR sostatus = 4 OR sostatus = 7) LIMIT 1) as rowExists, '" & idsodetail & "' as idsodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
                    'ftExistOutstandingSO = String.Concat(ftExistOutstandingSO, "SELECT EXISTS(SELECT 1 FROM m5_so_detail JOIN m5_so ON idso = soid WHERE idsodetail = '" & idsodetail & "' AND (sostatus = 2 OR sostatus = 3) LIMIT 1) as rowExists, '" & idsodetail & "' as idsodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
                    ftExistOutstandingSO = String.Concat(ftExistOutstandingSO, "SELECT EXISTS(SELECT 1 FROM m5_so_detail JOIN m5_so ON idso = soid WHERE idsodetail = '" & idsodetail & "' AND (sostatus = 2 OR sostatus = 3 OR sostatus = 4) LIMIT 1) as rowExists, '" & idsodetail & "' as idsodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                    '2. CEK JML OUTSTANDING -------------------
                    Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsodetail = " & idsodetail & " And idpidetail = 0 And idpldetail = 0")
                    ftOutstandingSO = IIf(Len(ftOutstandingSO.ToString) = 0, "", ftOutstandingSO & " OR ")
                    ftOutstandingSO = String.Concat(ftOutstandingSO, " (sod.idsodetail = " & idsodetail & " AND " & Outstanding & " > (sod.jmlbarang - sod.jmlrealisasi)) ")

                    '3. SET NILAI UPDATE OUTSTANDING ----------
                    updNilaiSO = String.Concat("WHEN '" & idsodetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiSO)

                    '4. SET FILTER UPDATE OUTSTANDING ---------
                    updFilterSO = IIf(Len(updFilterSO.ToString) = 0, "", updFilterSO & " OR ")
                    updFilterSO = String.Concat(updFilterSO, "(idsodetail = '" & idsodetail & "')")
                End If

                'VALIDASI STOK #1, JIKA TERKAIT DARI SO MAKA CEK STOK PERGUDANG (TOTAL STOK PERGUDANG)
                'CEK JML STOK KELUAR
                ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
                ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > isw.stok) ")

                ''SET NILAI UPDATE STOK BOOKING (MENGURANGI)
                'updStokOutBooking = IIf(Len(updStokOutBooking.ToString) = 0, "", updStokOutBooking & ", ")
                'updStokOutBooking = String.Concat(updStokOutBooking, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, gudang, jmlbooking

            Else

                'VALIDASI STOK #2, JIKA TIDAK TERKAIT DARI SO MAKA CEK STOK AVAILABLE PERGUDANG (TOTAL STOK PERGUDANG - STOK BOOKING)
                'CEK JML STOK KELUAR
                ftStokAvailable = IIf(Len(ftStokAvailable.ToString) = 0, "", ftStokAvailable & " OR ")
                ftStokAvailable = String.Concat(ftStokAvailable, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")

            End If

            If idpidetail <> 0 Then 'PI
                'CEK PI YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftPI = IIf(Len(ftPI.ToString) = 0, "", ftPI & " OR ")
                ftPI = String.Concat(ftPI, " (pid.idpidetail = " & idpidetail & ") ")

                '1. CEK DATA EXIST ------------------------
                ftExistOutstandingPI = IIf(Len(ftExistOutstandingPI.ToString) = 0, "", ftExistOutstandingPI & " UNION ")
                ftExistOutstandingPI = String.Concat(ftExistOutstandingPI, "SELECT EXISTS(SELECT 1 FROM m5_pi_detail JOIN m5_pi ON idpi = piid WHERE idpidetail = '" & idpidetail & "' AND (pistatus = 2 OR pistatus = 3 OR pistatus = 4 OR pistatus = 7) LIMIT 1) as rowExists, '" & idpidetail & "' as idpidetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
                'ftExistOutstandingPI = String.Concat(ftExistOutstandingPI, "SELECT EXISTS(SELECT 1 FROM m5_pi_detail JOIN m5_pi ON idpi = piid WHERE idpidetail = '" & idpidetail & "' AND (pistatus = 2 OR pistatus = 3) LIMIT 1) as rowExists, '" & idpidetail & "' as idpidetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idpidetail=" & idpidetail)
                ftOutstandingPI = IIf(Len(ftOutstandingPI.ToString) = 0, "", ftOutstandingPI & " OR ")
                ftOutstandingPI = String.Concat(ftOutstandingPI, " (pid.idpidetail = " & idpidetail & " AND " & Outstanding & " > (pid.jmlbarang - pid.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilaiPI = String.Concat("WHEN '" & idpidetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiPI)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilterPI = IIf(Len(updFilterPI.ToString) = 0, "", updFilterPI & " OR ")
                updFilterPI = String.Concat(updFilterPI, "(idpidetail = '" & idpidetail & "')")
            End If

            If idpldetail <> 0 Then 'PL
                'CEK PL YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftPL = IIf(Len(ftPL.ToString) = 0, "", ftPL & " OR ")
                ftPL = String.Concat(ftPL, " (pld.idpldetail = " & idpldetail & ") ")

                '1. CEK DATA EXIST ------------------------
                ftExistOutstandingPL = IIf(Len(ftExistOutstandingPL.ToString) = 0, "", ftExistOutstandingPL & " UNION ")
                ftExistOutstandingPL = String.Concat(ftExistOutstandingPL, "SELECT EXISTS(SELECT 1 FROM m5_pl_detail JOIN m5_pl ON idpl = plid WHERE idpldetail = '" & idpldetail & "' AND (plstatus = 2 OR plstatus = 3 OR plstatus = 4 OR plstatus = 7) LIMIT 1) as rowExists, '" & idpldetail & "' as idpldetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
                'ftExistOutstandingPL = String.Concat(ftExistOutstandingPL, "SELECT EXISTS(SELECT 1 FROM m5_pl_detail JOIN m5_pl ON idpl = plid WHERE idpldetail = '" & idpldetail & "' AND (plstatus = 2 OR plstatus = 3) LIMIT 1) as rowExists, '" & idpldetail & "' as idpldetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idpldetail=" & idpldetail)
                ftOutstandingPL = IIf(Len(ftOutstandingPL.ToString) = 0, "", ftOutstandingPL & " OR ")
                ftOutstandingPL = String.Concat(ftOutstandingPL, " (pld.idpldetail = " & idpldetail & " AND " & Outstanding & " > (pld.jmlbarang - pld.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilaiPL = String.Concat("WHEN '" & idpldetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiPL)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilterPL = IIf(Len(updFilterPL.ToString) = 0, "", updFilterPL & " OR ")
                updFilterPL = String.Concat(updFilterPL, "(idpldetail = '" & idpldetail & "')")
            End If

            'SET NILAI UPDATE STOK -------------------------------
            'SET NILAI UPDATE STOK KELUAR
            updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
            updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

            'SET NILAI UPDATE STOK MASUK
            updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
            updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok

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

        'ValidasiSimpan
        Dim ftExistBatch As String = "", ftBatch As String = ""
        Dim nbtkode As String = "", nbtgudang As String = "", nbtidbatchin As Integer = 0
        Dim updNilaiBatch As String = "", updFilterBatch As String = ""

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
                dataRowBatch(1) = 0
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

                'ValidasiSimpan
                'BUAT FILTER UNTUK VALIDASI ---------------------------------
                'nbtidbarang(2) As Integer , nbtkode(3) As String      , nbtjml(7) As Double         , nbtgudang(17) As String      , nbtidbatchin(18) As Integer
                idbarang = dataRowBatch(2) : nbtkode = dataRowBatch(3) : jmlbarang = dataRowBatch(7) : nbtgudang = dataRowBatch(17) : nbtidbatchin = dataRowBatch(18)

                'VALIDASI BATCH -------------------------------
                '1. CEK DATA EXIST BATCH KELUAR 
                ftExistBatch = IIf(Len(ftExistBatch.ToString) = 0, "", ftExistBatch & " UNION ")
                ftExistBatch = String.Concat(ftExistBatch, "SELECT EXISTS(SELECT 1 FROM m1_no_batch_in WHERE nbiidbatchin = '" & nbtidbatchin & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & nbtkode & "' as nbikode, '" & nbtgudang & "' as nbigudang FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML BATCH KELUAR 
                Dim jmlKeluar As Double = AsDataTableDSum(dtbatch, "nbtjml", "nbtidbatchin = " & nbtidbatchin & "")
                ftBatch = IIf(Len(ftBatch.ToString) = 0, "", ftBatch & " OR ")
                ftBatch = String.Concat(ftBatch, " (nbi.nbiidbatchin = " & nbtidbatchin & " AND " & jmlKeluar & " > nbi.nbijmlsisa) ")

                '3. SET NILAI UPDATE BATCH IN 
                updNilaiBatch = String.Concat("WHEN nbiidbatchin = '" & nbtidbatchin & "' THEN ROUND(nbijmlkeluar + '" & jmlKeluar & "', 5) ", updNilaiBatch)

                '4. SET FILTER UPDATE BATCH IN 
                updFilterBatch = IIf(Len(updFilterBatch.ToString) = 0, "", updFilterBatch & " OR ")
                updFilterBatch = String.Concat(updFilterBatch, "(nbiidbatchin = '" & nbtidbatchin & "')")
                'END OF BUAT FILTER UNTUK VALIDASI --------------------------

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

        'ValidasiSimpan
        Dim ftExistSerial As String = "", ftSerial As String = ""
        Dim nstkode As String = "", nstgudang As String = "", nstidserialin As Integer = 0
        Dim updNilaiSerial As String = "", updFilterSerial As String = ""

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
                dataRowSerial(1) = 0
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

                'ValidasiSimpan
                'BUAT FILTER UNTUK VALIDASI ---------------------------------
                'nstidbarang(2) As Integer  , nstkode(3) As String       , nstjml(7) As Double          , nstgudang(17) As String       , nstidserialin(18) As Integer
                idbarang = dataRowSerial(2) : nstkode = dataRowSerial(3) : jmlbarang = dataRowSerial(7) : nstgudang = dataRowSerial(17) : nstidserialin = dataRowSerial(18)

                'VALIDASI SERIAL -------------------------------
                '1. CEK DATA EXIST SERIAL KELUAR
                ftExistSerial = IIf(Len(ftExistSerial.ToString) = 0, "", ftExistSerial & " UNION ")
                ftExistSerial = String.Concat(ftExistSerial, "SELECT EXISTS(SELECT 1 FROM m1_no_serial_in WHERE nsiidserialin = '" & nstidserialin & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & nstkode & "' as nsikode, '" & nstgudang & "' as nsigudang FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML SERIAL KELUAR 
                Dim jmlKeluar As Double = AsDataTableDSum(dtserial, "nstjml", "nstidserialin = " & nstidserialin & "")
                ftSerial = IIf(Len(ftSerial.ToString) = 0, "", ftSerial & " OR ")
                ftSerial = String.Concat(ftSerial, " (nsi.nsiidserialin = " & nstidserialin & " AND " & jmlKeluar & " > nsi.nsijmlsisa) ")

                '3. SET NILAI UPDATE SERIAL IN 
                updNilaiSerial = String.Concat("WHEN nsiidserialin = '" & nstidserialin & "' THEN ROUND(nsijmlkeluar + '" & jmlKeluar & "', 5) ", updNilaiSerial)

                '4. SET FILTER UPDATE SERIAL IN 
                updFilterSerial = IIf(Len(updFilterSerial.ToString) = 0, "", updFilterSerial & " OR ")
                updFilterSerial = String.Concat(updFilterSerial, "(nsiidserialin = '" & nstidserialin & "')")
                'END OF BUAT FILTER UNTUK VALIDASI --------------------------

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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("dotgl")), AsFormatTanggal(drutama("dotgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                If drutama("dostatus") = 2 Then

                    'VALIDASI BATCH SERIAL ---------------
                    'ValidasiBatchSerial
                    Dim rsValidasi As String = ValidasiBatchSerial(dtdetail, dtbatch, dtserial, ftBarang, "jmlbarang", 0)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    'END OF VALIDASI BATCH SERIAL --------

                    'ValidasiSimpan
                    rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstandingSO, ftOutstandingSO, ftExistOutstandingPI, ftOutstandingPI, ftExistOutstandingPL, ftOutstandingPL, ftExistStok, ftStok, ftStokAvailable, ftExistBatch, ftBatch, ftExistSerial, ftSerial, "gudangasal", ftSO, ftPI, ftPL, drutama("dohargatermasukpajak"))
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("dotermin").ToString, AsFormatTanggal(drutama("dotgl")), "dotgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("dotgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                ''PERHITUNGAN TOTAL UTAMA ================================
                ''DIAMBILKAN DARI DATA DETAIL

                ''TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                ''SUBTOTAL = (jml * harga) - jmldiskon
                'AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                'dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                ''TOTAL = subtotal
                'drutama("dototal") = AsDataTableDSum(dtdetail, "subtotal")

                ''TOTALPAJAK1 = jmlpajak1
                'drutama("dototalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                ''TOTALPAJAK2 = jmlpajak2
                'drutama("dototalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                ''JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                ''JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'If Integer.Parse(drutama("dohargatermasukpajak")) = 0 Then
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                '    drutama("dototaltransaksi") = Double.Parse(drutama("dototal")) - Double.Parse(drutama("dojmldiskon")) + Double.Parse(drutama("dototalpajak1detail")) + Double.Parse(drutama("dototalpajak2detail")) + Double.Parse(drutama("dobiayalain"))

                'Else
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                '    drutama("dototaltransaksi") = Double.Parse(drutama("dototal")) - Double.Parse(drutama("dojmldiskon")) + Double.Parse(drutama("dobiayalain"))

                'End If
                ''END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("doid")
                    notransaksi = drutama("donotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(doid), donotransaksi FROM M5_do WHERE doid='" & result(4) & "' AND dostatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(doid) FROM m5_do WHERE donotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_do_history
                        Dim rsSimpanHistory As String = SimpanHistory.m5_Do_HistorySimpan("" & paramSplit(0) & "★M5_Do_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("dosumber")) & "▼" & FixQuotes(drutama("doid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Do set docabang  = '" & FixQuotes(drutama("docabang")) & "', dolokasi  = '" & FixQuotes(drutama("dolokasi")) & "', dogudang  = '" & FixQuotes(drutama("dogudang")) & "', doasalbarang  = '" & FixQuotes(drutama("doasalbarang")) & "', doasalbarangkategori  = " & drutama("doasalbarangkategori") & ", dojenispenjualan  = '" & FixQuotes(drutama("dojenispenjualan")) & "', dojenispenjualankategori  = " & drutama("dojenispenjualankategori") & ", docarabayar  = " & drutama("docarabayar") & ", dosumber  = '" & FixQuotes(drutama("dosumber")) & "', doautonotransaksi  = " & drutama("doautonotransaksi") & ", donotransaksi  = '" & FixQuotes(notransaksi) & "', dotgl  = '" & FixQuotes(AsFormatTanggal(drutama("dotgl"))) & "', dokodepa  = " & drutama("dokodepa") & ", docustomer  = " & drutama("docustomer") & ", docustomerkontak  = '" & FixQuotes(drutama("docustomerkontak")) & "', do1alamat1  = '" & FixQuotes(drutama("do1alamat1")) & "', do1alamat2  = '" & FixQuotes(drutama("do1alamat2")) & "', do1alamat3  = '" & FixQuotes(drutama("do1alamat3")) & "', do2alamat1  = '" & FixQuotes(drutama("do2alamat1")) & "', do2alamat2  = '" & FixQuotes(drutama("do2alamat2")) & "', do2alamat3  = '" & FixQuotes(drutama("do2alamat3")) & "', dobagianpenjualan  = " & drutama("dobagianpenjualan") & ", dobagianpengiriman  = " & drutama("dobagianpengiriman") & ", doekspedisi  = '" & FixQuotes(drutama("doekspedisi")) & "', dotglkirim  = '" & FixQuotes(AsFormatTanggal(drutama("dotglkirim"))) & "', dotermin  = '" & FixQuotes(drutama("dotermin")) & "', dotgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("dotgljatuhtempo"))) & "', douraian  = '" & FixQuotes(drutama("douraian")) & "', docatatan  = '" & FixQuotes(drutama("docatatan")) & "', donoref  = '" & FixQuotes(drutama("donoref")) & "', dotglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("dotglnoref"))) & "', dotglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("dotglpenutupan"))) & "', domatauang  = '" & FixQuotes(drutama("domatauang")) & "', dokurs  = '" & FixDouble(drutama("dokurs")) & "', dohargatermasukpajak  = " & drutama("dohargatermasukpajak") & ", dototal  = '" & FixDouble(drutama("dototal")) & "', dodiskonpersen  = '" & FixQuotes(drutama("dodiskonpersen")) & "', dojmldiskon  = '" & FixDouble(drutama("dojmldiskon")) & "', dototalpajak1detail  = '" & FixDouble(drutama("dototalpajak1detail")) & "', dototalpajak2detail  = '" & FixDouble(drutama("dototalpajak2detail")) & "', dobiayalainpersen  = '" & FixDouble(drutama("dobiayalainpersen")) & "', dobiayalain  = '" & FixDouble(drutama("dobiayalain")) & "', dototaltransaksi  = '" & FixDouble(drutama("dototaltransaksi")) & "', dorekdiskon  = '" & FixQuotes(drutama("dorekdiskon")) & "', dorekpajak1  = '" & FixQuotes(drutama("dorekpajak1")) & "', dorekpajak2  = '" & FixQuotes(drutama("dorekpajak2")) & "', dorekbiayalain  = '" & FixQuotes(drutama("dorekbiayalain")) & "', doidsq  = " & drutama("doidsq") & ", doidso  = " & drutama("doidso") & ", doidpi  = " & drutama("doidpi") & ", doidpl  = " & drutama("doidpl") & ", dostatusdr  = " & drutama("dostatusdr") & ", dostatussi  = " & drutama("dostatussi") & ", dostatusrnr  = " & drutama("dostatusrnr") & ", dostatussr  = " & drutama("dostatussr") & ", dostatus  = " & drutama("dostatus") & ", dostatussebelumnya  = " & drutama("dostatussebelumnya") & ", dojmlrevisi  = dojmlrevisi+1, docetakanke  = " & drutama("docetakanke") & ", domodifikasiuser  = " & drutama("domodifikasiuser") & ", domodifikasitgl  = NOW(), doposting  = 0, dotutupperiode  = " & drutama("dotutupperiode") & ", docustomtext1  = '" & FixQuotes(drutama("docustomtext1")) & "', docustomtext2  = '" & FixQuotes(drutama("docustomtext2")) & "', docustomtext3  = '" & FixQuotes(drutama("docustomtext3")) & "', docustomtext4  = '" & FixQuotes(drutama("docustomtext4")) & "', docustomtext5  = '" & FixQuotes(drutama("docustomtext5")) & "', docustomint1  = " & drutama("docustomint1") & ", docustomint2  = " & drutama("docustomint2") & ", docustomint3  = " & drutama("docustomint3") & ", docustomdbl1  = '" & FixDouble(drutama("docustomdbl1")) & "', docustomdbl2  = '" & FixDouble(drutama("docustomdbl2")) & "', docustomdbl3  = '" & FixDouble(drutama("docustomdbl3")) & "', docustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("docustomdate1"))) & "', docustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("docustomdate2"))) & "', docustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("docustomdate3"))) & "' where doid = '" & drutama("doid") & "'"
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

                    If drutama("doautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("docabang"), drutama("dolokasi"), drutama("dosumber"), drutama("dotgl"))
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
                        notransaksi = drutama("donotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(doid) FROM m5_do WHERE donotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Do (docabang, dolokasi, dogudang, doasalbarang, doasalbarangkategori, dojenispenjualan, dojenispenjualankategori, docarabayar, dosumber, doautonotransaksi, donotransaksi, dotgl, dokodepa, docustomer, docustomerkontak, do1alamat1, do1alamat2, do1alamat3, do2alamat1, do2alamat2, do2alamat3, dobagianpenjualan, dobagianpengiriman, doekspedisi, dotglkirim, dotermin, dotgljatuhtempo, douraian, docatatan, donoref, dotglnoref, dotglpenutupan, domatauang, dokurs, dohargatermasukpajak, dototal, dodiskonpersen, dojmldiskon, dototalpajak1detail, dototalpajak2detail, dobiayalainpersen, dobiayalain, dototaltransaksi, dorekdiskon, dorekpajak1, dorekpajak2, dorekbiayalain, doidsq, doidso, doidpi, doidpl, dostatusdr, dostatussi, dostatusrnr, dostatussr, dostatus, dostatussebelumnya, dojmlrevisi, docetakanke, doinputuser, doinputtgl, domodifikasiuser, domodifikasitgl, doposting, dotutupperiode, doisclose, docustomtext1, docustomtext2, docustomtext3, docustomtext4, docustomtext5, docustomint1, docustomint2, docustomint3, docustomdbl1, docustomdbl2, docustomdbl3, docustomdate1, docustomdate2, docustomdate3) values('" & FixQuotes(drutama("docabang")) & "', '" & FixQuotes(drutama("dolokasi")) & "', '" & FixQuotes(drutama("dogudang")) & "', '" & FixQuotes(drutama("doasalbarang")) & "', " & drutama("doasalbarangkategori") & ", '" & FixQuotes(drutama("dojenispenjualan")) & "', " & drutama("dojenispenjualankategori") & ", " & drutama("docarabayar") & ", '" & FixQuotes(drutama("dosumber")) & "', " & drutama("doautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("dotgl"))) & "', " & drutama("dokodepa") & ", " & drutama("docustomer") & ", '" & FixQuotes(drutama("docustomerkontak")) & "', '" & FixQuotes(drutama("do1alamat1")) & "', '" & FixQuotes(drutama("do1alamat2")) & "', '" & FixQuotes(drutama("do1alamat3")) & "', '" & FixQuotes(drutama("do2alamat1")) & "', '" & FixQuotes(drutama("do2alamat2")) & "', '" & FixQuotes(drutama("do2alamat3")) & "', " & drutama("dobagianpenjualan") & ", " & drutama("dobagianpengiriman") & ", '" & FixQuotes(drutama("doekspedisi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("dotglkirim"))) & "', '" & FixQuotes(drutama("dotermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("dotgljatuhtempo"))) & "', '" & FixQuotes(drutama("douraian")) & "', '" & FixQuotes(drutama("docatatan")) & "', '" & FixQuotes(drutama("donoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("dotglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("dotglpenutupan"))) & "', '" & FixQuotes(drutama("domatauang")) & "', '" & FixDouble(drutama("dokurs")) & "', " & drutama("dohargatermasukpajak") & ", '" & FixDouble(drutama("dototal")) & "', '" & FixQuotes(drutama("dodiskonpersen")) & "', '" & FixDouble(drutama("dojmldiskon")) & "', '" & FixDouble(drutama("dototalpajak1detail")) & "', '" & FixDouble(drutama("dototalpajak2detail")) & "', '" & FixDouble(drutama("dobiayalainpersen")) & "', '" & FixDouble(drutama("dobiayalain")) & "', '" & FixDouble(drutama("dototaltransaksi")) & "', '" & FixQuotes(drutama("dorekdiskon")) & "', '" & FixQuotes(drutama("dorekpajak1")) & "', '" & FixQuotes(drutama("dorekpajak2")) & "', '" & FixQuotes(drutama("dorekbiayalain")) & "', " & drutama("doidsq") & ", " & drutama("doidso") & ", " & drutama("doidpi") & ", " & drutama("doidpl") & ", " & drutama("dostatusdr") & ", " & drutama("dostatussi") & ", " & drutama("dostatusrnr") & ", " & drutama("dostatussr") & ", " & drutama("dostatus") & ", " & drutama("dostatussebelumnya") & ", " & drutama("dojmlrevisi") & ", " & drutama("docetakanke") & ", " & drutama("doinputuser") & ", NOW(), " & drutama("domodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("dotutupperiode") & ", " & drutama("doisclose") & ", '" & FixQuotes(drutama("docustomtext1")) & "', '" & FixQuotes(drutama("docustomtext2")) & "', '" & FixQuotes(drutama("docustomtext3")) & "', '" & FixQuotes(drutama("docustomtext4")) & "', '" & FixQuotes(drutama("docustomtext5")) & "', " & drutama("docustomint1") & ", " & drutama("docustomint2") & ", " & drutama("docustomint3") & ", '" & FixDouble(drutama("docustomdbl1")) & "', '" & FixDouble(drutama("docustomdbl2")) & "', '" & FixDouble(drutama("docustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("docustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("docustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("docustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select doid from M5_do where donotransaksi='" & notransaksi & "' AND doinputuser= '" & userid & "' order by domodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Do_Detail where iddo = '" & result(4) & "'"
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
                        If Not drutama("domatauang").ToString.Equals(dr1("matauang").ToString) Then
                            result(2) = "Row : " & dr1("urutan") & " - " & dr1("tipebarang") & " | " & dr1("namabarang") & " currency (" & dr1("matauang") & ") doesn't belong to the main transactions." : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF VALIDASI MATAUANG HARUS SAMA DENGAN TRANSAKSI UTAMA -------------


                        'SET HARGA DARI TRANSAKSI SEBELUMNYA ------------------------------------
                        If Double.Parse(dr1("idpldetail")) > 0 Then
                            'JIKA AMBIL PL MAKA SET HARGA DARI PL
                            sql = "SELECT jml, harga, diskon, pajak1, jmlpajak1, pajak2, jmlpajak2 FROM m5_pl_detail WHERE idpldetail = '" & FixDouble(dr1("idpldetail")) & "'"

                        ElseIf Double.Parse(dr1("idpidetail")) > 0 Then
                            'JIKA AMBIL PI MAKA SET HARGA DARI PI
                            sql = "SELECT jml, harga, diskon, pajak1, jmlpajak1, pajak2, jmlpajak2 FROM m5_pi_detail WHERE idpidetail = '" & FixDouble(dr1("idpidetail")) & "'"

                        ElseIf Double.Parse(dr1("idsodetail")) > 0 Then
                            'JIKA AMBIL SO MAKA SET HARGA DARI SO
                            sql = "SELECT jml, harga, diskon, pajak1, jmlpajak1, pajak2, jmlpajak2 FROM m5_so_detail WHERE idsodetail = '" & FixDouble(dr1("idsodetail")) & "'"

                        Else
                            sql = ""
                        End If

                        dtBefore = AsDataTableAmbilDariDB(sql)
                        If dtBefore.Rows.Count > 0 Then
                            'SET HARGA - ambil dari transaksi sebelumnya
                            dr1("harga") = Double.Parse(dtBefore.Rows(0)("harga"))

                            'SET DISKON - ambil dari transaksi sebelumnya
                            dr1("diskon") = dtBefore.Rows(0)("diskon")

                            'SET JMLDISKON - hitung diskon
                            dr1("jmldiskon") = F_Diskon(Double.Parse(dr1("jml")), Double.Parse(dr1("harga")), FixQuotes(dr1("diskon").ToString))

                            'SET PAJAK1 - ambil dari transaksi sebelumnya
                            dr1("pajak1") = dtBefore.Rows(0)("pajak1")

                            'SET JMLPAJAK1 - ambil dari transaksi sebelumnya = (jmlpajakbefore / jmlbefore) * jml
                            dr1("jmlpajak1") = (Double.Parse(dtBefore.Rows(0)("jmlpajak1")) / Double.Parse(dtBefore.Rows(0)("jml"))) * Double.Parse(dr1("jml"))

                            'SET PAJAK2 - ambil dari transaksi sebelumnya
                            dr1("pajak2") = dtBefore.Rows(0)("pajak2")

                            'SET JMLPAJAK2 - ambil dari transaksi sebelumnya = (jmlpajakbefore / jmlbefore) * jml
                            dr1("jmlpajak2") = (Double.Parse(dtBefore.Rows(0)("jmlpajak2")) / Double.Parse(dtBefore.Rows(0)("jml"))) * Double.Parse(dr1("jml"))
                        End If
                        'END OF SET HARGA DARI TRANSAKSI SEBELUMNYA -----------------------------


                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("iddodetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', " & dr1("idhppkhususmasuk") & ", " & dr1("idhppfifomasuk") & ", '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hpp")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("rekhargapokok")) & "', '" & FixQuotes(dr1("rekdiskonpenjualan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idsqdetail") & ", " & dr1("idsodetail") & ", " & dr1("idpidetail") & ", " & dr1("idpldetail") & ", '" & FixDouble(dr1("jmldr")) & "', " & dr1("statusdr") & ", '" & FixDouble(dr1("jmlsi")) & "', " & dr1("statussi") & ", '" & FixDouble(dr1("jmlrnr")) & "', " & dr1("statusrnr") & ", '" & FixDouble(dr1("jmlsr")) & "', " & dr1("statussr") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M5_Do_Detail(iddodetail, iddo, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, idhppkhususmasuk, idhppfifomasuk, harga, hpp, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekhargapokok, rekdiskonpenjualan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpidetail, idpldetail, jmldr, statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                    sql = "Delete from M1_No_Batch_Transaction where nbtidtransaksi  = '" & result(4) & "' AND nbtsumber = 'DO'"
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
                    sql = "Delete from M1_No_Serial_Transaction  where nstidtransaksi  = '" & result(4) & "' AND nstsumber = 'DO'"
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


                If drutama("dostatus") = 2 Then
                    'UPDATE OUTSTANDING TRANSAKSI ======================================================
                    If Len(updNilaiSO) > 0 Then 'SO
                        'UPDATE DETAIL
                        sql = "UPDATE m5_so_detail SET jmlrealisasi = (CASE idsodetail " & updNilaiSO & " ELSE jmlrealisasi END) WHERE " & updFilterSO
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idso FROM m5_so_detail WHERE " & updFilterSO & " GROUP BY idso")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idso = '" & dr1("idso") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDB("SELECT idso, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_so_detail WHERE " & ftDetail & " GROUP BY idso")
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
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                    End If

                    If Len(updNilaiPI) > 0 Then 'PI
                        'UPDATE DETAIL
                        sql = "UPDATE m5_pi_detail SET jmlrealisasi = (CASE idpidetail " & updNilaiPI & " ELSE jmlrealisasi END) WHERE " & updFilterPI
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idpi FROM m5_pi_detail WHERE " & updFilterPI & " GROUP BY idpi")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idpi = '" & dr1("idpi") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDB("SELECT idpi, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_pi_detail WHERE " & ftDetail & " GROUP BY idpi")
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiPI = "" : updFilterPI = ""
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
                                updNilaiPI = String.Concat(updNilaiPI, "WHEN '" & dr1("idpi") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterPI = IIf(Len(updFilterPI.ToString) = 0, "", updFilterPI & " OR ")
                                updFilterPI = String.Concat(updFilterPI, "(piid = '" & dr1("idpi") & "')")
                            Next

                            sql = "UPDATE m5_pi SET pistatusrealisasi = (CASE piid " & updNilaiPI & " ELSE pistatusrealisasi END) WHERE " & updFilterPI
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

                    If Len(updNilaiPL) > 0 Then 'PL
                        'UPDATE DETAIL
                        sql = "UPDATE m5_pl_detail SET jmlrealisasi = (CASE idpldetail " & updNilaiPL & " ELSE jmlrealisasi END) WHERE " & updFilterPL
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idpl FROM m5_pl_detail WHERE " & updFilterPL & " GROUP BY idpl")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idpl = '" & dr1("idpl") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDB("SELECT idpl, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_pl_detail WHERE " & ftDetail & " GROUP BY idpl")
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiPL = "" : updFilterPL = ""
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
                                updNilaiPL = String.Concat(updNilaiPL, "WHEN '" & dr1("idpl") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterPL = IIf(Len(updFilterPL.ToString) = 0, "", updFilterPL & " OR ")
                                updFilterPL = String.Concat(updFilterPL, "(plid = '" & dr1("idpl") & "')")
                            Next

                            sql = "UPDATE m5_pl SET plstatusrealisasi = (CASE plid " & updNilaiPL & " ELSE plstatusrealisasi END) WHERE " & updFilterPL
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


                    'AMBIL GUDANG TRANSIT DARI SETTING ==============================================
                    Dim SetGudang As String = ""
                    'GUDANG SETTING TRANSIT DIGUNAKAN UNTUK NO SERIAL DAN BATCH MASUK
                    'MISAL : GUDANG ASAL 'A', MAKA :
                    '-- NO SERIAL DAN BATCH GUDANG 'A' BERKURANG
                    '-- NO SERIAL DAN BATCH GUDANG TRANSIT BERTAMBAH
                    sql = "SELECT snilai FROM m0_setting WHERE smodule = 3 AND sgrup = 'defaultgudang' AND skode = 'GudangTransit'"
                    Dim dtSetGudang As DataTable = AsDataTableAmbilDariDB(sql)
                    If dtSetGudang.Rows.Count > 0 Then
                        SetGudang = dtSetGudang.Rows(0)("snilai")
                    Else
                        result(2) = "Setting for Transit Warehouse not found." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF AMBIL GUDANG TRANSIT DARI SETTING =======================================


                    'INSERT NO BATCH ================================================================
                    If dtbatch.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder, strValue3 As New StringBuilder
                        For Each dr1 As DataRow In dtbatch.Rows
                            'QUERY INSERT NO BATCH OUT
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping             nboid,            nboidbatchin,                           nbogudang,                  nboidbarang,                           nbokode,                             nbosumber,            nboidtransaksi,                     nbosatuan,                         nbojmlkeluar,       nboisclose
                            strValue2.Append("(" & 0 & ", " & dr1("nbtidbatchin") & ", '" & FixQuotes(dr1("nbtgudang")) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', " & 0 & ")")

                            'QUERY INSERT NO BATCH IN
                            strValue3.Append(IIf(Len(strValue3.ToString) = 0, "", ", "))
                            'mapping        nbiidbatchin,                nbigudang,                nbiidbarang,                           nbikode,                             nbisumber,            nbiidtransaksi,                     nbisatuan,                 nbijmlmasuk,       nbijmlkeluar,                  nbijmlsisa, nbiisclose,                     nbicustomtext1,                             nbicustomtext2,                             nbicustomtext3,                             nbicustomdbl1,                             nbicustomdbl2,                             nbicustomdbl3,                                             nbicustomdate1,                                              nbicustomdate2,                                              nbicustomdate3
                            strValue3.Append("(" & 0 & ", '" & FixQuotes(SetGudang) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixQuotes(dr1("nbtcustomtext1")) & "', '" & FixQuotes(dr1("nbtcustomtext2")) & "', '" & FixQuotes(dr1("nbtcustomtext3")) & "', '" & FixDouble(dr1("nbtcustomdbl1")) & "', '" & FixDouble(dr1("nbtcustomdbl2")) & "', '" & FixDouble(dr1("nbtcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate3"))) & "')")
                        Next

                        'INSERT NO BATCH OUT ---------------------------------
                        sql = "Insert into M1_No_Batch_Out(nboid, nboidbatchin, nbogudang, nboidbarang, nbokode, nbosumber, nboidtransaksi, nbosatuan, nbojmlkeluar, nboisclose) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE NO BATCH IN KELUAR ---------------------------
                        If Len(updNilaiBatch) > 0 Then
                            sql = "UPDATE m1_no_batch_in SET nbijmlkeluar =  (CASE " & updNilaiBatch & " ELSE nbijmlkeluar END) WHERE " & updFilterBatch
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If

                        'INSERT NO BATCH IN MASUK ----------------------------
                        sql = "Insert into M1_No_Batch_In(nbiidbatchin, nbigudang, nbiidbarang, nbikode, nbisumber, nbiidtransaksi, nbisatuan, nbijmlmasuk, nbijmlkeluar, nbijmlsisa, nbiisclose, nbicustomtext1, nbicustomtext2, nbicustomtext3, nbicustomdbl1, nbicustomdbl2, nbicustomdbl3, nbicustomdate1, nbicustomdate2, nbicustomdate3) values" & strValue3.ToString & ""
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
                        Dim strValue2 As New StringBuilder, strValue3 As New StringBuilder
                        For Each dr1 As DataRow In dtserial.Rows
                            'QUERY INSERT NO SERIAL OUT
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping            nsoid,             nsoidserialin,                           nsogudang,                  nsoidbarang,                           nsokode,                             nsosumber,            nsoidtransaksi,                     nsosatuan,                          nsojmlkeluar,      nsoisclose
                            strValue2.Append("(" & 0 & ", " & dr1("nstidserialin") & ", '" & FixQuotes(dr1("nstgudang")) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', " & 0 & ")")

                            'QUERY INSERT NO SERIAL IN
                            strValue3.Append(IIf(Len(strValue3.ToString) = 0, "", ", "))
                            'mapping       nsiidserialin,                nsigudang,                nsiidbarang,                           nsikode,                             nsisumber,            nsiidtransaksi,                     nsisatuan,                       nsijmlmasuk, nsijmlkeluar,                  nsijmlsisa, nsiisclose,                     nsicustomtext1,                             nsicustomtext2,                             nsicustomtext3,                             nsicustomdbl1,                             nsicustomdbl2,                             nsicustomdbl3,                                             nsicustomdate1,                                              nsicustomdate2,                                              nsicustomdate3
                            strValue3.Append("(" & 0 & ", '" & FixQuotes(SetGudang) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixQuotes(dr1("nstcustomtext1")) & "', '" & FixQuotes(dr1("nstcustomtext2")) & "', '" & FixQuotes(dr1("nstcustomtext3")) & "', '" & FixDouble(dr1("nstcustomdbl1")) & "', '" & FixDouble(dr1("nstcustomdbl2")) & "', '" & FixDouble(dr1("nstcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate3"))) & "')")
                        Next

                        'INSERT NO SERIAL OUT --------------------------------
                        sql = "Insert into M1_No_Serial_Out(nsoid, nsoidserialin, nsogudang, nsoidbarang, nsokode, nsosumber, nsoidtransaksi, nsosatuan, nsojmlkeluar, nsoisclose) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE NO SERIAL IN KELUAR --------------------------
                        If Len(updNilaiSerial) > 0 Then
                            sql = "UPDATE m1_no_serial_in SET nsijmlkeluar =  (CASE " & updNilaiSerial & " ELSE nsijmlkeluar END) WHERE " & updFilterSerial
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If

                        'INSERT NO SERIAL IN MASUK ---------------------------
                        sql = "Insert into M1_No_Serial_In(nsiidserialin, nsigudang, nsiidbarang, nsikode, nsisumber, nsiidtransaksi, nsisatuan, nsijmlmasuk, nsijmlkeluar, nsijmlsisa, nsiisclose, nsicustomtext1, nsicustomtext2, nsicustomtext3, nsicustomdbl1, nsicustomdbl2, nsicustomdbl3, nsicustomdate1, nsicustomdate2, nsicustomdate3) values" & strValue3.ToString & ""
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
                    'MENGURANGI BOOKING HANYA UNTUK BARANG YG HPP NYA BUKAN KHUSUS (I) DAN TERKAIT DARI SO
                    sql = "INSERT INTO m1_item_booking (SELECT idbarang, gudangasal, jmlbarang * -1 FROM m5_do_detail JOIN m1_item ON idbarang = bid AND bjenis <> 'J' AND bhpp <> 'I' AND idsodetail <> 0 AND iddo = '" & result(4) & "') ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'If Len(updStokOutBooking) > 0 Then
                    '    sql = "INSERT INTO m1_item_booking (idbarang, gudang, jmlbooking) VALUES " & updStokOutBooking & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                    '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    '    With objCmd
                    '        .Connection = Con1
                    '        .Transaction = Trans
                    '        .CommandType = CommandType.Text
                    '        .CommandText = sql
                    '    End With
                    '    objCmd.ExecuteNonQuery()
                    'End If
                    'END OF UPDATE STOK BOOKING =====================================================


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

                    'STOK MASUK
                    If Len(updStokIn) > 0 Then
                        sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokIn & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF UPDATE STOK =============================================================


                    'INSERT ITEM TRANSACTION ========================================================
                    'AMBIL DATA DETAIL YANG BARU
                    sql = "SELECT dod.iddodetail, dod.idbarang, dod.namabarang, dod.tipebarang, dod.jml, dod.satuan, dod.jmlbarang, dod.satuanbarang, dod.matauang, dod.kurs, dod.harga, dod.diskon, dod.jmldiskon, dod.hpp, dod.idhppkhususmasuk, dod.gudangasal, dod.gudangtransit, dod.gudangtujuan, dod.catatan, dod.costcenter, dod.divisi, dod.subdivisi, dod.proyek, `do`.doinputtgl, i.bhpp FROM m5_do_detail dod JOIN m5_do `do` ON dod.iddo = `do`.doid JOIN m1_item i ON dod.idbarang = i.bid WHERE dod.iddo = '" & result(4) & "'"
                    Dim dtDetailNew As DataTable = AsDataTableAmbilDariDB(sql)
                    Dim hpp As Double = 0, jenismutasi As Double = 0, postinghpp As Double = 0
                    Dim strTransaksiBarang As New StringBuilder

                    If dtDetailNew.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtDetailNew.Rows
                            'jenismutasi dan postinghpp 
                            '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 1
                            '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                            '- untuk transaksi mutasi saja maka postinghpp = 0
                            postinghpp = 0

                            'hitung hpp = hpp
                            hpp = Double.Parse(dr1("hpp"))

                            'POSTING BARANG KELUAR (gudangasal)
                            jenismutasi = 0
                            'QUERY INSERT TRANSAKSI BARANG KELUAR
                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                            'mapping                        id,                             cabang,                                   lokasi,                                gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,             iddetail,                    notransaksi,                                                 tgl,                              kontak,               idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                  idhppikm,                idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("docabang")) & "', '" & FixQuotes(drutama("dolokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', " & drutama("dokodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("dosumber")) & "', " & result(4) & ", " & dr1("iddodetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("dotgl"))) & "', " & drutama("docustomer") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("douraian")) & "', '" & FixQuotes(drutama("docatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("doinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("doinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")

                            'POSTING BARANG MASUK (gudangtransit)
                            jenismutasi = 1
                            'QUERY INSERT TRANSAKSI BARANG MASUK
                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                            'mapping                        id,                             cabang,                                   lokasi,                                   gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,             iddetail,                    notransaksi,                                                 tgl,                              kontak,               idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                  idhppikm,                idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("docabang")) & "', '" & FixQuotes(drutama("dolokasi")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', " & drutama("dokodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("dosumber")) & "', " & result(4) & ", " & dr1("iddodetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("dotgl"))) & "', " & drutama("docustomer") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("douraian")) & "', '" & FixQuotes(drutama("docatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("doinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("doinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                        Next

                        sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    Else
                        result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF INSERT ITEM TRANSACTION =================================================

                End If

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "DO", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M5_DoUpdateStatusOld(ByVal param As String) As String

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
            Dim sumber As String = "DO", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Dotgl, Donotransaksi, Dostatus FROM M5_Do WHERE Doid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Dostatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m5_do_history
            Dim rsSimpanHistory As String = SimpanHistory.m5_Do_HistorySimpan("" & paramSplit(0) & "★M5_Do_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m5_do_terkait("doid = '" & idtransaksi & "'")
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


                Dim idbarang As Integer = 0, idsodetail As Integer = 0, idpidetail As Integer = 0, idpldetail As Integer = 0
                Dim idhppkhususmasuk As Integer = 0, jmlbarang As Double = 0
                Dim ftExistStok As String = "", ftStok As String = ""
                Dim gudangOut As String = "", updStokOut As String = ""
                Dim gudangIn As String = "", updStokIn As String = "", updStokInBooking As String = ""
                Dim updNilaiSO As String = "", updFilterSO As String = ""
                Dim updNilaiPI As String = "", updFilterPI As String = ""
                Dim updNilaiPL As String = "", updFilterPL As String = ""
                Dim updNilaiHppI As String = "", updFilterHppI As String = "", delFilterHppI As String = ""


                'VALIDASI JIKA SO CLOSE MAKA DO TIDAK DAPAT DI DRAFT --------------------------------------
                dtdetail = AsDataTableAmbilDariDB("SELECT so.sonotransaksi FROM m5_so so JOIN m5_so_detail sod ON so.soid = sod.idso JOIN m5_do_detail dod ON sod.idsodetail = dod.idsodetail AND dod.iddo = '" & FixDouble(idtransaksi) & "' AND so.sostatus NOT IN(2,3,4)")
                If dtdetail.Rows.Count > 0 Then
                    result(2) = "No. SO : " & dtdetail.Rows(0)("sonotransaksi") & " doesn't exists/yet approved in SO" : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI JIKA SO CLOSE MAKA DO TIDAK DAPAT DI DRAFT -------------------------------


                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDB("SELECT iddodetail, idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idsodetail, idpidetail, idpldetail, gudangasal, gudangtransit, idhppkhususmasuk, idhppfifomasuk, urutan FROM m5_do_detail WHERE iddo = '" & idtransaksi & "'")
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        '1. SET NILAI
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang")
                        idsodetail = dr1("idsodetail") : idpidetail = dr1("idpidetail") : idpldetail = dr1("idpldetail")
                        gudangIn = dr1("gudangasal") : gudangOut = dr1("gudangtransit") : idhppkhususmasuk = dr1("idhppkhususmasuk")

                        '2. BUAT FILTER UPDATE OUTSTANDING
                        If idsodetail <> 0 Then

                            If idpidetail = 0 And idpldetail = 0 Then
                                '2.1 SET NILAI UPDATE OUTSTANDING SO
                                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsodetail=" & idsodetail)
                                updNilaiSO = String.Concat("WHEN '" & idsodetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiSO)

                                '2.2. SET FILTERUPDATE OUTSTANDING SO
                                updFilterSO = IIf(Len(updFilterSO.ToString) = 0, "", updFilterSO & " OR ")
                                updFilterSO = String.Concat(updFilterSO, "(idsodetail = '" & idsodetail & "')")
                            End If

                            ''SET NILAI UPDATE STOK BOOKING MASUK
                            'updStokInBooking = IIf(Len(updStokInBooking.ToString) = 0, "", updStokInBooking & ", ")
                            'updStokInBooking = String.Concat(updStokInBooking, "('" & idbarang & "', '" & gudangIn & "', ('" & jmlbarang & "'))") ' idbarang, kgudang, stok

                        End If

                        If idpidetail <> 0 Then
                            '2.1 SET NILAI UPDATE OUTSTANDING PI
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idpidetail=" & idpidetail)
                            updNilaiPI = String.Concat("WHEN '" & idpidetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiPI)

                            '2.2. SET FILTERUPDATE OUTSTANDING PI
                            updFilterPI = IIf(Len(updFilterPI.ToString) = 0, "", updFilterPI & " OR ")
                            updFilterPI = String.Concat(updFilterPI, "(idpidetail = '" & idpidetail & "')")
                        End If

                        If idpldetail <> 0 Then
                            '2.1 SET NILAI UPDATE OUTSTANDING PL
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idpldetail=" & idpldetail)
                            updNilaiPL = String.Concat("WHEN '" & idpldetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiPL)

                            '2.2. SET FILTERUPDATE OUTSTANDING PL
                            updFilterPL = IIf(Len(updFilterPL.ToString) = 0, "", updFilterPL & " OR ")
                            updFilterPL = String.Concat(updFilterPL, "(idpldetail = '" & idpldetail & "')")
                        End If

                        'VALIDASI STOK -------------------------------
                        '1. CEK DATA EXIST
                        ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                        ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists,  bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                        '2. CEK JML STOK
                        Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangtransit='" & gudangOut & "'")
                        ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
                        ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > isw.stok) ")

                        '3. SET NILAI UPDATE STOK KELUAR
                        updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
                        updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

                        '4. SET NILAI UPDATE STOK MASUK
                        updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
                        updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok

                    Next

                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                'VALIDASI STOK ----------------------------------
                Dim rsValidasi As String = ValidasiSimpan(dtdetail, "", "", "", "", "", "", ftExistStok, ftStok, "", "", "", "", "", "", "", "", "", "")
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'END OF VALIDASI STOK ---------------------------

                'UPDATE OUTSTANDING =============================================================
                If Len(updFilterSO) > 0 Then
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m5_so_detail SET jmlrealisasi = (CASE idsodetail " & updNilaiSO & " ELSE jmlrealisasi END) WHERE " & updFilterSO
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idso FROM m5_so_detail WHERE " & updFilterSO & " GROUP BY idso")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idso = '" & dr1("idso") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDB("SELECT idso, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_so_detail WHERE " & ftDetail & " GROUP BY idso")
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
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF UPDATE OUTSTANDING UTAMA ----------------
                End If

                If Len(updFilterPI) > 0 Then
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m5_pi_detail SET jmlrealisasi = (CASE idpidetail " & updNilaiPI & " ELSE jmlrealisasi END) WHERE " & updFilterPI
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idpi FROM m5_pi_detail WHERE " & updFilterPI & " GROUP BY idpi")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idpi = '" & dr1("idpi") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDB("SELECT idpi, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_pi_detail WHERE " & ftDetail & " GROUP BY idpi")
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiPI = "" : updFilterPI = ""
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
                            updNilaiPI = String.Concat(updNilaiPI, "WHEN '" & dr1("idpi") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterPI = IIf(Len(updFilterPI.ToString) = 0, "", updFilterPI & " OR ")
                            updFilterPI = String.Concat(updFilterPI, "(piid = '" & dr1("idpi") & "')")
                        Next

                        sql = "UPDATE m5_pi SET pistatusrealisasi = (CASE piid " & updNilaiPI & " ELSE pistatusrealisasi END) WHERE " & updFilterPI
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

                If Len(updFilterPL) > 0 Then 'PL
                    'UPDATE OUTSTANDING DETAIL -------------------
                    sql = "UPDATE m5_pl_detail SET jmlrealisasi = (CASE idpldetail " & updNilaiPL & " ELSE jmlrealisasi END) WHERE " & updFilterPL
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idpl FROM m5_pl_detail WHERE " & updFilterPL & " GROUP BY idpl")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idpl = '" & dr1("idpl") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDB("SELECT idpl, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_pl_detail WHERE " & ftDetail & " GROUP BY idpl")
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiPL = "" : updFilterPL = ""
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
                            updNilaiPL = String.Concat(updNilaiPL, "WHEN '" & dr1("idpl") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterPL = IIf(Len(updFilterPL.ToString) = 0, "", updFilterPL & " OR ")
                            updFilterPL = String.Concat(updFilterPL, "(plid = '" & dr1("idpl") & "')")
                        Next

                        sql = "UPDATE m5_pl SET plstatusrealisasi = (CASE plid " & updNilaiPL & " ELSE plstatusrealisasi END) WHERE " & updFilterPL
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
                'END OF UPDATE OUTSTANDING ======================================================


                'UPDATE NO BATCH ================================================================
                Dim updNilaiBatch As String = "", updFilterBatch As String = ""
                Dim dtBatch As DataTable = AsDataTableAmbilDariDB("SELECT nboidbatchin, nbogudang, nboidbarang, nbokode, nbojmlkeluar FROM m1_no_batch_out WHERE nbosumber = '" & sumber & "' AND nboidtransaksi = '" & idtransaksi & "'")
                If dtBatch.Rows.Count > 0 Then
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

                    'DELETE NO BATCH OUT --------------------------------
                    sql = "DELETE FROM m1_no_batch_out WHERE nbosumber = '" & sumber & "' AND nboidtransaksi = '" & idtransaksi & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE NO BATCH IN KELUAR --------------------------
                    For Each dr1 As DataRow In dtBatch.Rows
                        'SET NILAI UPDATE BATCH IN
                        Dim jmlKeluar As Double = AsDataTableDSum(dtBatch, "nbojmlkeluar", "nboidbatchin = " & dr1("nboidbatchin") & "")
                        updNilaiBatch = String.Concat("WHEN nbiidbatchin = '" & dr1("nboidbatchin") & "' THEN ROUND(nbijmlkeluar - '" & jmlKeluar & "', 5) ", updNilaiBatch)

                        'SET FILTER UPDATE BATCH IN
                        updFilterBatch = IIf(Len(updFilterBatch.ToString) = 0, "", updFilterBatch & " OR ")
                        updFilterBatch = String.Concat(updFilterBatch, "(nbiidbatchin = '" & dr1("nboidbatchin") & "')")
                    Next
                    If Len(updNilaiBatch) > 0 Then
                        sql = "UPDATE m1_no_batch_in SET nbijmlkeluar =  (CASE " & updNilaiBatch & " ELSE nbijmlkeluar END) WHERE " & updFilterBatch
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
                'END OF UPDATE NO BATCH =========================================================


                'UPDATE NO SERIAL ===============================================================
                Dim updNilaiSerial As String = "", updFilterSerial As String = ""
                Dim dtSerial As DataTable = AsDataTableAmbilDariDB("SELECT nsoidserialin, nsogudang, nsoidbarang, nsokode, nsojmlkeluar FROM m1_no_serial_out WHERE nsosumber = '" & sumber & "' AND nsoidtransaksi = '" & idtransaksi & "'")
                If dtSerial.Rows.Count > 0 Then
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

                    'DELETE NO SERIAL OUT -------------------------------
                    sql = "DELETE FROM m1_no_serial_out WHERE nsosumber = '" & sumber & "' AND nsoidtransaksi = '" & idtransaksi & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE NO SERIAL IN KELUAR -------------------------
                    For Each dr1 As DataRow In dtSerial.Rows
                        'SET NILAI UPDATE SERIAL IN
                        Dim jmlKeluar As Double = AsDataTableDSum(dtSerial, "nsojmlkeluar", "nsoidserialin = " & dr1("nsoidserialin") & "")
                        updNilaiSerial = String.Concat("WHEN nsiidserialin = '" & dr1("nsoidserialin") & "' THEN ROUND(nsijmlkeluar - '" & jmlKeluar & "', 5) ", updNilaiSerial)

                        'SET FILTER UPDATE SERIAL IN
                        updFilterSerial = IIf(Len(updFilterSerial.ToString) = 0, "", updFilterSerial & " OR ")
                        updFilterSerial = String.Concat(updFilterSerial, "(nsiidserialin = '" & dr1("nsoidserialin") & "')")
                    Next
                    If Len(updNilaiSerial) > 0 Then
                        sql = "UPDATE m1_no_serial_in SET nsijmlkeluar =  (CASE " & updNilaiSerial & " ELSE nsijmlkeluar END) WHERE " & updFilterSerial
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
                'END OF UPDATE NO SERIAL =======================================================


                'UPDATE STOK BOOKING ============================================================
                'MENAMBAH BOOKING HANYA UNTUK BARANG YG HPP NYA BUKAN KHUSUS (I) DAN TERKAIT DARI SO
                sql = "INSERT INTO m1_item_booking (SELECT idbarang, gudangasal, jmlbarang FROM m5_do_detail JOIN m1_item ON idbarang = bid AND bjenis <> 'J' AND bhpp <> 'I' AND idsodetail <> 0 AND iddo = '" & idtransaksi & "') ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'If Len(updStokInBooking) > 0 Then
                '    sql = "INSERT INTO m1_item_booking (idbarang, gudang, jmlbooking) VALUES " & updStokInBooking & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '    With objCmd
                '        .Connection = Con1
                '        .Transaction = Trans
                '        .CommandType = CommandType.Text
                '        .CommandText = sql
                '    End With
                '    objCmd.ExecuteNonQuery()
                'End If
                'END OF UPDATE STOK BOOKING =====================================================


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

                'STOK MASUK
                If Len(updStokIn) > 0 Then
                    sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokIn & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
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
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF DELETE TRANSAKSI BARANG =================================================

            End If

            'update status utama
            sql = "UPDATE M5_Do SET Dostatus = " & nilaiStatus & ", Domodifikasiuser='" & userid & "', Domodifikasitgl = NOW(), Doposting = 0, Dopostingtgl = '1971-01-01 00:00:00', Dojmlrevisi = Dojmlrevisi + 1 WHERE Doid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_DoSearch(PostWsSearch(paramSplit(0), "M5_doSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M5_DoDeleteOld(ByVal param As String) As String

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
            Dim sumber As String = "Do", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Doid, Donotransaksi FROM M5_Do WHERE Doid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT docabang, dolokasi, dosumber, doautonotransaksi, donotransaksi, dotgl"
            sql &= " FROM M5_do"
            sql &= " WHERE doid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("docabang")
                lokasi = dtNomorNext.Rows(0)("dolokasi")
                sumber = dtNomorNext.Rows(0)("dosumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("doautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("donotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("dotgl"))
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
            sql = "DELETE FROM M5_Do_Detail WHERE iddo = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE UTAMA
            sql = "DELETE FROM M5_Do WHERE doid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_DoSearch(PostWsSearch(paramSplit(0), "M5_DoSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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