Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m7_aq
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M7_AqSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim nogrup As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean

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
        'aqid(0) As , aqcabang(1) As String, aqlokasi(2) As String, aqsumber(3) As String, aqautonogrup(4) As Integer, 
        'aqnogrup(5) As String, aqautonotransaksi(6) As Integer, aqnotransaksi(7) As String, aqtgl(8) As Date, aqkodepa(9) As , 
        'aqsupplier(10) As , aqsupplierkontak(11) As String, aq1alamat1(12) As String, aq1alamat2(13) As String, aq1alamat3(14) As String, 
        'aq2alamat1(15) As String, aq2alamat2(16) As String, aq2alamat3(17) As String, aqbagianpembelian(18) As , aqtgldipenuhi(19) As Date, 
        'aqtermin(20) As String, aqtgljatuhtempo(21) As Date, aquraian(22) As String, aqcatatan(23) As String, aqnoref(24) As String, 
        'aqtglnoref(25) As Date, aqtglpenutupan(26) As Date, aqmatauang(27) As String, aqkurs(28) As Double, aqhargatermasukpajak(29) As Integer, 
        'aqtotal(30) As Double, aqdiskonpersen(31) As String, aqdiskon(32) As Double, aqtotalpajak1detail(33) As Double, aqtotalpajak2detail(34) As Double, 
        'aqbiayalainpersen(35) As String, aqbiayalain(36) As Double, aqtotaltransaksi(37) As Double, aqidar(38) As , aqstatusao(39) As Integer, 
        'aqstatusae(40) As Integer, aqstatusai(41) As Integer, aqstatusrealisasi(42) As Integer, aqstatus(43) As Integer, aqstatussebelumnya(44) As Integer, 
        'aqjmlrevisi(45) As Integer, aqcetakanke(46) As Integer, aqinputuser(47) As , aqinputtgl(48) As DateTime, aqmodifikasiuser(49) As , 
        'aqmodifikasitgl(50) As DateTime, aqposting(51) As Integer, aqpostingtgl(52) As DateTime, aqisclose(53) As Integer, aqcustomtext1(54) As String, 
        'aqcustomtext2(55) As String, aqcustomtext3(56) As String, aqcustomtext4(57) As String, aqcustomtext5(58) As String, aqcustomint1(59) As Integer, 
        'aqcustomint2(60) As Integer, aqcustomint3(61) As Integer, aqcustomdbl1(62) As Double, aqcustomdbl2(63) As Double, aqcustomdbl3(64) As Double, 
        'aqcustomdate1(65) As Date, aqcustomdate2(66) As Date, aqcustomdate3(67) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'aqid, aqcabang, aqlokasi, aqsumber, aqautonogrup, aqnogrup, aqautonotransaksi, 
        'aqnotransaksi, aqtgl, aqkodepa, aqsupplier, aqsupplierkontak, aq1alamat1, aq1alamat2, 
        'aq1alamat3, aq2alamat1, aq2alamat2, aq2alamat3, aqbagianpembelian, aqtgldipenuhi, aqtermin, 
        'aqtgljatuhtempo, aquraian, aqcatatan, aqnoref, aqtglnoref, aqtglpenutupan, aqmatauang, 
        'aqkurs, aqhargatermasukpajak, aqtotal, aqdiskonpersen, aqdiskon, aqtotalpajak1detail, aqtotalpajak2detail, 
        'aqbiayalainpersen, aqbiayalain, aqtotaltransaksi, aqidar, aqstatusao, aqstatusae, aqstatusai, 
        'aqstatusrealisasi, aqstatus, aqstatussebelumnya, aqjmlrevisi, aqcetakanke, aqinputuser, aqinputtgl, 
        'aqmodifikasiuser, aqmodifikasitgl, aqposting, aqpostingtgl, aqisclose, aqcustomtext1, aqcustomtext2, 
        'aqcustomtext3, aqcustomtext4, aqcustomtext5, aqcustomint1, aqcustomint2, aqcustomint3, aqcustomdbl1, 
        'aqcustomdbl2, aqcustomdbl3, aqcustomdate1, aqcustomdate2, aqcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 66) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'aqautonogrup(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "aqautonogrup required numeric." : GoTo selesai
        End If
        'aqautonotransaksi(6) As Integer
        If (IsNumeric(dataUtama(6)) = False) Then
            result(2) = "aqautonotransaksi required numeric." : GoTo selesai
        End If
        'aqtgl(8) As Date
        If (IsDate(dataUtama(8)) = False) Then
            result(2) = "aqtgl required date." : GoTo selesai
        End If
        'aqtgldipenuhi(19) As Date
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "aqtgldipenuhi required date." : GoTo selesai
        End If
        'aqtgljatuhtempo(21) As Date
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "aqtgljatuhtempo required date." : GoTo selesai
        End If
        'aqtglnoref(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "aqtglnoref required date." : GoTo selesai
        End If
        'aqtglpenutupan(26) As Date
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "aqtglpenutupan required date." : GoTo selesai
        End If
        'aqkurs(28) As Double
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "aqkurs required numeric." : GoTo selesai
        End If
        'aqhargatermasukpajak(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "aqhargatermasukpajak required numeric." : GoTo selesai
        End If
        'aqtotal(30) As Double
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "aqtotal required numeric." : GoTo selesai
        End If
        'aqdiskon(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "aqdiskon required numeric." : GoTo selesai
        End If
        'aqtotalpajak1detail(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "aqtotalpajak1detail required numeric." : GoTo selesai
        End If
        'aqtotalpajak2detail(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "aqtotalpajak2detail required numeric." : GoTo selesai
        End If
        'aqbiayalain(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "aqbiayalain required numeric." : GoTo selesai
        End If
        'aqtotaltransaksi(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "aqtotaltransaksi required numeric." : GoTo selesai
        End If
        'aqstatusao(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "aqstatusao required numeric." : GoTo selesai
        End If
        'aqstatusae(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "aqstatusae required numeric." : GoTo selesai
        End If
        'aqstatus(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "aqstatus required numeric." : GoTo selesai
        End If
        'aqstatussebelumnya(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "aqstatussebelumnya required numeric." : GoTo selesai
        End If
        'aqjmlrevisi(43) As Integer
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "aqjmlrevisi required numeric." : GoTo selesai
        End If
        'aqcetakanke(46) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "aqcetakanke required numeric." : GoTo selesai
        End If
        'aqinputtgl(48) As DateTime
        If (IsDate(dataUtama(46)) = False) Then
            result(2) = "aqinputtgl required date." : GoTo selesai
        End If
        'aqmodifikasitgl(50) As DateTime
        If (IsDate(dataUtama(48)) = False) Then
            result(2) = "aqmodifikasitgl required date." : GoTo selesai
        End If
        'aqposting(51) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "aqposting required numeric." : GoTo selesai
        End If
        'aqpostingtgl(52) As DateTime
        If (IsDate(dataUtama(50)) = False) Then
            result(2) = "aqpostingtgl required date." : GoTo selesai
        End If
        'aqisclose(53) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "aqisclose required numeric." : GoTo selesai
        End If
        'aqcustomint1(59) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "aqcustomint1 required numeric." : GoTo selesai
        End If
        'aqcustomint2(60) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "aqcustomint2 required numeric." : GoTo selesai
        End If
        'aqcustomint3(61) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "aqcustomint3 required numeric." : GoTo selesai
        End If
        'aqcustomdbl1(62) As Double
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "aqcustomdbl1 required numeric." : GoTo selesai
        End If
        'aqcustomdbl2(63) As Double
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "aqcustomdbl2 required numeric." : GoTo selesai
        End If
        'aqcustomdbl3(64) As Double
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "aqcustomdbl3 required numeric." : GoTo selesai
        End If
        'aqcustomdate1(65) As Date
        If (IsDate(dataUtama(63)) = False) Then
            result(2) = "aqcustomdate1 required date." : GoTo selesai
        End If
        'aqcustomdate2(66) As Date
        If (IsDate(dataUtama(64)) = False) Then
            result(2) = "aqcustomdate2 required date." : GoTo selesai
        End If
        'aqcustomdate3(67) As Date
        If (IsDate(dataUtama(65)) = False) Then
            result(2) = "aqcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'aqid(0) As 
        If Len(dataUtama(0)) = 0 Then
            result(2) = "aqid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 20 Then
            result(2) = "aqid should not be more than 20 character." : GoTo selesai
        End If

        'aqcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "aqcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "aqcabang should not be more than 25 character." : GoTo selesai
        End If

        'aqlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "aqlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "aqlokasi should not be more than 25 character." : GoTo selesai
        End If

        'aqsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "aqsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "aqsumber should not be more than 10 character." : GoTo selesai
        End If

        'aqnotransaksi(7) As String
        If Len(dataUtama(7)) = 0 Then
            result(2) = "aqnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 50 Then
            result(2) = "aqnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'aqtgl(8) As Date
        If Len(dataUtama(8)) = 0 Then
            result(2) = "aqtgl can't be empty" : GoTo selesai
        End If

        'aqkodepa(9) As 
        If Len(dataUtama(9)) = 0 Then
            result(2) = "aqkodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 20 Then
            result(2) = "aqkodepa should not be more than 20 character." : GoTo selesai
        End If

        'aqsupplier(10) As 
        If Len(dataUtama(10)) = 0 Then
            result(2) = "aqsupplier can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(10)) > 20 Then
            result(2) = "aqsupplier should not be more than 20 character." : GoTo selesai
        End If

        'aqbagianpembelian(18) As 
        If Len(dataUtama(18)) = 0 Then
            result(2) = "aqbagianpembelian can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(18)) > 20 Then
            result(2) = "aqbagianpembelian should not be more than 20 character." : GoTo selesai
        End If

        'aqtgldipenuhi(19) As Date
        If Len(dataUtama(19)) = 0 Then
            result(2) = "aqtgldipenuhi can't be empty" : GoTo selesai
        End If

        'aqtgljatuhtempo(21) As Date
        If Len(dataUtama(21)) = 0 Then
            result(2) = "aqtgljatuhtempo can't be empty" : GoTo selesai
        End If

        'aqtglnoref(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "aqtglnoref can't be empty" : GoTo selesai
        End If

        'aqtglpenutupan(26) As Date
        If Len(dataUtama(26)) = 0 Then
            result(2) = "aqtglpenutupan can't be empty" : GoTo selesai
        End If

        'aqmatauang(27) As String
        If Len(dataUtama(27)) = 0 Then
            result(2) = "aqmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(27)) > 25 Then
            result(2) = "aqmatauang should not be more than 25 character." : GoTo selesai
        End If

        'aqkurs(28) As Double
        If Len(dataUtama(28)) = 0 Then
            result(2) = "aqkurs can't be empty" : GoTo selesai
        End If

        'aqtotal(30) As Double
        If Len(dataUtama(30)) = 0 Then
            result(2) = "aqtotal can't be empty" : GoTo selesai
        End If

        'aqdiskonpersen(31) As String
        If Len(dataUtama(31)) = 0 Then
            result(2) = "aqdiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(31)) > 25 Then
            result(2) = "aqdiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'aqdiskon(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "aqdiskon can't be empty" : GoTo selesai
        End If

        'aqtotalpajak1detail(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "aqtotalpajak1detail can't be empty" : GoTo selesai
        End If

        'aqtotalpajak2detail(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "aqtotalpajak2detail can't be empty" : GoTo selesai
        End If

        'aqbiayalainpersen(35) As String
        If Len(dataUtama(35)) = 0 Then
            result(2) = "aqbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(35)) > 25 Then
            result(2) = "aqbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'aqbiayalain(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "aqbiayalain can't be empty" : GoTo selesai
        End If

        'aqtotaltransaksi(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "aqtotaltransaksi can't be empty" : GoTo selesai
        End If

        'aqidar(38) As 
        If Len(dataUtama(38)) = 0 Then
            result(2) = "aqidar can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(38)) > 20 Then
            result(2) = "aqidar should not be more than 20 character." : GoTo selesai
        End If

        'aqinputuser(47) As 
        If Len(dataUtama(45)) = 0 Then
            result(2) = "aqinputuser can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(45)) > 20 Then
            result(2) = "aqinputuser should not be more than 20 character." : GoTo selesai
        End If

        'aqinputtgl(48) As DateTime
        If Len(dataUtama(46)) = 0 Then
            result(2) = "aqinputtgl can't be empty" : GoTo selesai
        End If

        'aqmodifikasiuser(49) As 
        If Len(dataUtama(47)) = 0 Then
            result(2) = "aqmodifikasiuser can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(48)) > 20 Then
            result(2) = "aqmodifikasiuser should not be more than 20 character." : GoTo selesai
        End If

        'aqmodifikasitgl(50) As DateTime
        If Len(dataUtama(49)) = 0 Then
            result(2) = "aqmodifikasitgl can't be empty" : GoTo selesai
        End If

        'aqpostingtgl(52) As DateTime
        If Len(dataUtama(51)) = 0 Then
            result(2) = "aqpostingtgl can't be empty" : GoTo selesai
        End If

        'aqcustomdbl1(62) As Double
        If Len(dataUtama(61)) = 0 Then
            result(2) = "aqcustomdbl1 can't be empty" : GoTo selesai
        End If

        'aqcustomdbl2(63) As Double
        If Len(dataUtama(62)) = 0 Then
            result(2) = "aqcustomdbl2 can't be empty" : GoTo selesai
        End If

        'aqcustomdbl3(64) As Double
        If Len(dataUtama(63)) = 0 Then
            result(2) = "aqcustomdbl3 can't be empty" : GoTo selesai
        End If


        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "aqid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqautonogrup", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aqnogrup", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aqnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqkodepa", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aqsupplier", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aqsupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aq1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aq1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aq1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aq2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aq2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aq2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqbagianpembelian", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aqtgldipenuhi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqtermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqtgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aquraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqtglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqhargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aqtotal", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aqdiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqtotalpajak1detail", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aqtotalpajak2detail", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aqbiayalainpersen", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aqbiayalain", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aqtotaltransaksi", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aqidar", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aqstatusao", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aqstatusae", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aqstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aqstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aqjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aqcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aqinputuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aqinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqmodifikasiuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aqmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aqpostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aqcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aqcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aqcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aqcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aqcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "aqid~aqcabang~aqlokasi~aqsumber~aqautonogrup~aqnogrup~aqautonotransaksi~aqnotransaksi~aqtgl~aqkodepa~aqsupplier~aqsupplierkontak~aq1alamat1~aq1alamat2~aq1alamat3~aq2alamat1~aq2alamat2~aq2alamat3~aqbagianpembelian~aqtgldipenuhi~aqtermin~aqtgljatuhtempo~aquraian~aqcatatan~aqnoref~aqtglnoref~aqtglpenutupan~aqmatauang~aqkurs~aqhargatermasukpajak~aqtotal~aqdiskonpersen~aqdiskon~aqtotalpajak1detail~aqtotalpajak2detail~aqbiayalainpersen~aqbiayalain~aqtotaltransaksi~aqidar~aqstatusao~aqstatusae~aqstatus~aqstatussebelumnya~aqjmlrevisi~aqcetakanke~aqinputuser~aqinputtgl~aqmodifikasiuser~aqmodifikasitgl~aqposting~aqpostingtgl~aqisclose~aqcustomtext1~aqcustomtext2~aqcustomtext3~aqcustomtext4~aqcustomtext5~aqcustomint1~aqcustomint2~aqcustomint3~aqcustomdbl1~aqcustomdbl2~aqcustomdbl3~aqcustomdate1~aqcustomdate2~aqcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idaqdetail(0) As , idaq(1) As , idasset(2) As , namaasset(3) As String, jml(4) As Double, 
        'matauang(5) As String, kurs(6) As Double, harga(7) As Double, diskon(8) As String, jmldiskon(9) As Double, 
        'pajak1(10) As String, jmlpajak1(11) As Double, pajak2(12) As String, jmlpajak2(13) As Double, cabang(14) As String, 
        'lokasi(15) As String, costcenter(16) As String, divisi(17) As String, subdivisi(18) As String, proyek(19) As String, 
        'catatan(20) As String, urutan(21) As Integer, idardetail(22) As , jmlao(23) As Double, statusao(24) As Integer, 
        'jmlae(25) As Double, statusae(26) As Integer, isclose(27) As Integer, customtext1(28) As String, customtext2(29) As String, customtext3(30) As String, 
        'customdbl1(31) As Double, customdbl2(32) As Double, customdbl3(33) As Double, customdate1(34) As Date, customdate2(35) As Date, 
        'customdate3(36) As Date, satuan(37) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idaqdetail, idaq, idasset, namaasset, jml, matauang, kurs, 
        'harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, 
        'cabang, lokasi, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idardetail, jmlao, statusao, jmlae, statusae, jmlai, 
        'statusai, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, satuan

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idaqdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idaq", AsEnumTypeData.AsDouble)
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
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idardetail", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmlao", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusao", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlae", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusae", AsEnumTypeData.AsInt64)
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
        Dim ftExistOutstanding As String = "", ftOutstanding As String = ""
        Dim updNilai As String = "", updFilter As String = ""
        Dim jml As Double = 0, idardetail As Integer = 0

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 38) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'jml(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "jml required numeric." : GoTo selesai
            End If
            'kurs(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "kurs required numeric." : GoTo selesai
            End If
            'harga(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "harga required numeric." : GoTo selesai
            End If
            'jmldiskon(9) As Double
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "jmldiskon required numeric." : GoTo selesai
            End If
            'jmlpajak1(11) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "jmlpajak1 required numeric." : GoTo selesai
            End If
            'jmlpajak2(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "jmlpajak2 required numeric." : GoTo selesai
            End If
            'urutan(21) As Integer
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "urutan required numeric." : GoTo selesai
            End If
            'jmlao(23) As Double
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "jmlao required numeric." : GoTo selesai
            End If
            'statusao(24) As Integer
            If (IsNumeric(dataRowDetail(24)) = False) Then
                result(2) = "statusao required numeric." : GoTo selesai
            End If
            'jmlae(25) As Double
            If (IsNumeric(dataRowDetail(25)) = False) Then
                result(2) = "jmlae required numeric." : GoTo selesai
            End If
            'statusae(26) As Integer
            If (IsNumeric(dataRowDetail(26)) = False) Then
                result(2) = "statusae required numeric." : GoTo selesai
            End If
            'isclose(31) As Integer
            If (IsNumeric(dataRowDetail(27)) = False) Then
                result(2) = "isclose required numeric." : GoTo selesai
            End If
            'customdbl1(35) As Double
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(36) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(37) As Double
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(38) As Date
            If (IsDate(dataRowDetail(34)) = False) Then
                result(2) = "customdate1 required date." : GoTo selesai
            End If
            'customdate2(39) As Date
            If (IsDate(dataRowDetail(35)) = False) Then
                result(2) = "customdate2 required date." : GoTo selesai
            End If
            'customdate3(40) As Date
            If (IsDate(dataRowDetail(36)) = False) Then
                result(2) = "customdate3 required date." : GoTo selesai
            End If

            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'idaqdetail(0) As 
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idaqdetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idaqdetail should not be more than 20 character." : GoTo selesai
            End If

            'idaq(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idaq can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idaq should not be more than 20 character." : GoTo selesai
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

            'idardetail(22) As 
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - idardetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(22)) > 20 Then
                result(2) = "Row : " & i & " - idardetail should not be more than 20 character." : GoTo selesai
            End If

            'jmlao(23) As Double
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - jmlao can't be empty" : GoTo selesai
            End If

            'jmlae(25) As Double
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - jmlae can't be empty" : GoTo selesai
            End If

            'customdbl1(35) As Double
            If Len(dataRowDetail(31)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(36) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(37) As Double
            If Len(dataRowDetail(33)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(38) As Date
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(39) As Date
            If Len(dataRowDetail(35)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(40) As Date
            If Len(dataRowDetail(36)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(37)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idaqdetail~idaq~idasset~namaasset~jml~matauang~kurs~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~costcenter~divisi~subdivisi~proyek~catatan~urutan~idardetail~jmlao~statusao~jmlae~statusae~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~satuan", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idasset(2) As Integer     , jml(8) As Double       , idprdetail(28) As Integer
            jml = dataRowDetail(4) : idardetail = dataRowDetail(22)

            'VALIDASI OUTSTANDING -------------------------
            If idardetail <> 0 Then
                '1. CEK DATA EXIST ------------------------
                ftExistOutstanding = IIf(Len(ftExistOutstanding.ToString) = 0, "", ftExistOutstanding & " UNION ")
                ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM M7_ar_detail JOIN M4_ar ON idar = arid WHERE idardetail = '" & idardetail & "' AND (arstatus = 2 OR arstatus = 3 OR arstatus = 4 OR arstatus = 7) LIMIT 1) as rowExists, '" & idardetail & "' as idardetail")

                '2. SET NILAI UPDATE OUTSTANDING ----------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jml", "idardetail=" & idardetail)
                updNilai = String.Concat("WHEN '" & idardetail & "' THEN ROUND(jmlaq + '" & Outstanding & "', 5) ", updNilai)

                '3. SET FILTER UPDATE OUTSTANDING ---------
                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                updFilter = String.Concat(updFilter, "(idardetail = '" & idardetail & "')")
            End If
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------
        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'SIMPAN KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0, autoNogrupOld As String = ""

        Try
            'Proses utama
            If (dtutama.Rows.Count > 0) Then
                Dim drutama As DataRow = dtutama.Rows(0)

                'SET TGL JATUH TEMPO ====================================
                Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                rsTglJT = F_TglJT(drutama("aqtermin").ToString, AsFormatTanggal(drutama("aqtgl")), "aqtgl").Split(sptSubParam)
                If rsTglJT(0) = 0 Then
                    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                Else
                    drutama("aqtgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                End If
                'END OF SET TGL JATUH TEMPO =============================

                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TOTAL = subtotal
                drutama("aqtotal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("aqtotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("aqtotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                drutama("aqtotaltransaksi") = Double.Parse(drutama("aqtotal")) - Double.Parse(drutama("aqdiskon")) + Double.Parse(drutama("aqtotalpajak1detail")) + Double.Parse(drutama("aqtotalpajak2detail")) + Double.Parse(drutama("aqbiayalain"))
                'END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("aqid")
                    notransaksi = drutama("aqnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(aqid), aqnotransaksi FROM M7_aq WHERE aqid='" & result(4) & "' AND aqstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then
                        sql = "Update M7_Aq set aqcabang  = '" & FixQuotes(drutama("aqcabang")) & "', aqlokasi  = '" & FixQuotes(drutama("aqlokasi")) & "', aqsumber  = '" & FixQuotes(drutama("aqsumber")) & "', aqautonogrup  = " & drutama("aqautonogrup") & ", aqnogrup  = '" & FixQuotes(drutama("aqnogrup")) & "', aqautonotransaksi  = " & drutama("aqautonotransaksi") & ", aqnotransaksi  = '" & FixQuotes(drutama("aqnotransaksi")) & "', aqtgl  = '" & FixQuotes(AsFormatTanggal(drutama("aqtgl"))) & "', aqkodepa  = '" & FixQuotes(drutama("aqkodepa")) & "', aqsupplier  = '" & FixQuotes(drutama("aqsupplier")) & "', aqsupplierkontak  = '" & FixQuotes(drutama("aqsupplierkontak")) & "', aq1alamat1  = '" & FixQuotes(drutama("aq1alamat1")) & "', aq1alamat2  = '" & FixQuotes(drutama("aq1alamat2")) & "', aq1alamat3  = '" & FixQuotes(drutama("aq1alamat3")) & "', aq2alamat1  = '" & FixQuotes(drutama("aq2alamat1")) & "', aq2alamat2  = '" & FixQuotes(drutama("aq2alamat2")) & "', aq2alamat3  = '" & FixQuotes(drutama("aq2alamat3")) & "', aqbagianpembelian  = '" & FixQuotes(drutama("aqbagianpembelian")) & "', aqtgldipenuhi  = '" & FixQuotes(AsFormatTanggal(drutama("aqtgldipenuhi"))) & "', aqtermin  = '" & FixQuotes(drutama("aqtermin")) & "', aqtgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("aqtgljatuhtempo"))) & "', aquraian  = '" & FixQuotes(drutama("aquraian")) & "', aqcatatan  = '" & FixQuotes(drutama("aqcatatan")) & "', aqnoref  = '" & FixQuotes(drutama("aqnoref")) & "', aqtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("aqtglnoref"))) & "', aqtglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("aqtglpenutupan"))) & "', aqmatauang  = '" & FixQuotes(drutama("aqmatauang")) & "', aqkurs  = '" & FixDouble(drutama("aqkurs")) & "', aqhargatermasukpajak  = " & drutama("aqhargatermasukpajak") & ", aqtotal  = '" & FixDouble(drutama("aqtotal")) & "', aqdiskonpersen  = '" & FixQuotes(drutama("aqdiskonpersen")) & "', aqdiskon  = '" & FixDouble(drutama("aqdiskon")) & "', aqtotalpajak1detail  = '" & FixDouble(drutama("aqtotalpajak1detail")) & "', aqtotalpajak2detail  = '" & FixDouble(drutama("aqtotalpajak2detail")) & "', aqbiayalainpersen  = '" & FixQuotes(drutama("aqbiayalainpersen")) & "', aqbiayalain  = '" & FixDouble(drutama("aqbiayalain")) & "', aqtotaltransaksi  = '" & FixDouble(drutama("aqtotaltransaksi")) & "', aqidar  = '" & FixQuotes(drutama("aqidar")) & "', aqstatusao  = " & drutama("aqstatusao") & ", aqstatusae  = " & drutama("aqstatusae") & ", aqstatus  = " & drutama("aqstatus") & ", aqstatussebelumnya  = " & drutama("aqstatussebelumnya") & ", aqjmlrevisi  = " & drutama("aqjmlrevisi") & ", aqcetakanke  = " & drutama("aqcetakanke") & ", aqmodifikasiuser  = '" & FixQuotes(drutama("aqmodifikasiuser")) & "', aqmodifikasitgl  = NOW(), aqposting  = " & drutama("aqposting") & ", aqpostingtgl  = '" & FixQuotes(AsFormatTanggal(drutama("aqpostingtgl"), "yyyy-MM-dd H:mm:ss")) & "', aqcustomtext1  = '" & FixQuotes(drutama("aqcustomtext1")) & "', aqcustomtext2  = '" & FixQuotes(drutama("aqcustomtext2")) & "', aqcustomtext3  = '" & FixQuotes(drutama("aqcustomtext3")) & "', aqcustomtext4  = '" & FixQuotes(drutama("aqcustomtext4")) & "', aqcustomtext5  = '" & FixQuotes(drutama("aqcustomtext5")) & "', aqcustomint1  = " & drutama("aqcustomint1") & ", aqcustomint2  = " & drutama("aqcustomint2") & ", aqcustomint3  = " & drutama("aqcustomint3") & ", aqcustomdbl1  = '" & FixDouble(drutama("aqcustomdbl1")) & "', aqcustomdbl2  = '" & FixDouble(drutama("aqcustomdbl2")) & "', aqcustomdbl3  = '" & FixDouble(drutama("aqcustomdbl3")) & "', aqcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("aqcustomdate1"))) & "', aqcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("aqcustomdate2"))) & "', aqcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("aqcustomdate3"))) & "' where aqid = " & drutama("aqid") & ""
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

                    If drutama("aqautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("aqcabang"), drutama("aqlokasi"), drutama("aqsumber"), drutama("aqtgl"))
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
                        notransaksi = drutama("aqnotransaksi")
                    End If

                    If drutama("aqautonogrup") = 1 Then
                        'GENERATE NOGRUP =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNogrup As String = wsM0_Nomor.M0_NogrupRQ(drutama("aqcabang"), drutama("aqlokasi"), drutama("aqtgl"))
                        Dim arrNogrup(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
                        arrNogrup = rsNogrup.Split(sptSubParam)
                        'cek success generate notransaksi
                        If (arrNogrup(0) = 1) Then
                            nogrup = arrNogrup(2)
                            'tambah query update m0_nomor_next
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = arrNogrup(3)
                            End With
                            objCmd.ExecuteNonQuery()
                        Else
                            result(2) = arrNogrup(1) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF GENERATE NOGRUP ==================================

                    Else
                        nogrup = drutama("aqnogrup")
                    End If
                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(aqid) FROM m7_aq WHERE aqnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============


                    If drutama("aqautonogrup") = 1 Then
                        'GENERATE NOGRUP =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNogrup As String = wsM0_Nomor.M0_NogrupRQ(drutama("aqcabang"), drutama("aqlokasi"), drutama("aqtgl"))
                        Dim arrNogrup(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
                        arrNogrup = rsNogrup.Split(sptSubParam)
                        'cek success generate notransaksi
                        If (arrNogrup(0) = 1) Then
                            nogrup = arrNogrup(2)
                            'tambah query update m0_nomor_next
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = arrNogrup(3)
                            End With
                            objCmd.ExecuteNonQuery()
                        Else
                            result(2) = arrNogrup(1) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF GENERATE NOGRUP ==================================

                    Else
                        nogrup = drutama("aqnogrup")
                    End If

                    sql = "Insert into M7_Aq (aqcabang, aqlokasi, aqsumber, aqautonogrup, aqnogrup, aqautonotransaksi, aqnotransaksi, aqtgl, aqkodepa, aqsupplier, aqsupplierkontak, aq1alamat1, aq1alamat2, aq1alamat3, aq2alamat1, aq2alamat2, aq2alamat3, aqbagianpembelian, aqtgldipenuhi, aqtermin, aqtgljatuhtempo, aquraian, aqcatatan, aqnoref, aqtglnoref, aqtglpenutupan, aqmatauang, aqkurs, aqhargatermasukpajak, aqtotal, aqdiskonpersen, aqdiskon, aqtotalpajak1detail, aqtotalpajak2detail, aqbiayalainpersen, aqbiayalain, aqtotaltransaksi, aqidar, aqstatusao, aqstatusae, aqstatus, aqstatussebelumnya, aqjmlrevisi, aqcetakanke, aqinputuser, aqinputtgl, aqmodifikasiuser, aqmodifikasitgl, aqposting, aqpostingtgl, aqisclose, aqcustomtext1, aqcustomtext2, aqcustomtext3, aqcustomtext4, aqcustomtext5, aqcustomint1, aqcustomint2, aqcustomint3, aqcustomdbl1, aqcustomdbl2, aqcustomdbl3, aqcustomdate1, aqcustomdate2, aqcustomdate3) values('" & FixQuotes(drutama("aqcabang")) & "', '" & FixQuotes(drutama("aqlokasi")) & "', '" & FixQuotes(drutama("aqsumber")) & "', " & drutama("aqautonogrup") & ", '" & nogrup & "', " & drutama("aqautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("aqtgl"))) & "', '" & FixQuotes(drutama("aqkodepa")) & "', '" & FixQuotes(drutama("aqsupplier")) & "', '" & FixQuotes(drutama("aqsupplierkontak")) & "', '" & FixQuotes(drutama("aq1alamat1")) & "', '" & FixQuotes(drutama("aq1alamat2")) & "', '" & FixQuotes(drutama("aq1alamat3")) & "', '" & FixQuotes(drutama("aq2alamat1")) & "', '" & FixQuotes(drutama("aq2alamat2")) & "', '" & FixQuotes(drutama("aq2alamat3")) & "', '" & FixQuotes(drutama("aqbagianpembelian")) & "', '" & FixQuotes(AsFormatTanggal(drutama("aqtgldipenuhi"))) & "', '" & FixQuotes(drutama("aqtermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("aqtgljatuhtempo"))) & "', '" & FixQuotes(drutama("aquraian")) & "', '" & FixQuotes(drutama("aqcatatan")) & "', '" & FixQuotes(drutama("aqnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("aqtglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("aqtglpenutupan"))) & "', '" & FixQuotes(drutama("aqmatauang")) & "', '" & FixDouble(drutama("aqkurs")) & "', " & drutama("aqhargatermasukpajak") & ", '" & FixDouble(drutama("aqtotal")) & "', '" & FixQuotes(drutama("aqdiskonpersen")) & "', '" & FixDouble(drutama("aqdiskon")) & "', '" & FixDouble(drutama("aqtotalpajak1detail")) & "', '" & FixDouble(drutama("aqtotalpajak2detail")) & "', '" & FixQuotes(drutama("aqbiayalainpersen")) & "', '" & FixDouble(drutama("aqbiayalain")) & "', '" & FixDouble(drutama("aqtotaltransaksi")) & "', '" & FixQuotes(drutama("aqidar")) & "', " & drutama("aqstatusao") & ", " & drutama("aqstatusae") & ", " & drutama("aqstatus") & ", " & drutama("aqstatussebelumnya") & ", " & drutama("aqjmlrevisi") & ", " & drutama("aqcetakanke") & ", '" & FixQuotes(drutama("aqinputuser")) & "', NOW(), '" & FixQuotes(drutama("aqmodifikasiuser")) & "', '1971-01-01 00:00:00', " & drutama("aqposting") & ", '1971-01-01 00:00:00', " & drutama("aqisclose") & ", '" & FixQuotes(drutama("aqcustomtext1")) & "', '" & FixQuotes(drutama("aqcustomtext2")) & "', '" & FixQuotes(drutama("aqcustomtext3")) & "', '" & FixQuotes(drutama("aqcustomtext4")) & "', '" & FixQuotes(drutama("aqcustomtext5")) & "', " & drutama("aqcustomint1") & ", " & drutama("aqcustomint2") & ", " & drutama("aqcustomint3") & ", '" & FixDouble(drutama("aqcustomdbl1")) & "', '" & FixDouble(drutama("aqcustomdbl2")) & "', '" & FixDouble(drutama("aqcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("aqcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("aqcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("aqcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select aqid from M7_aq where aqnotransaksi='" & notransaksi & "' AND aqinputuser= '" & userid & "' order by aqmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                    result(4) = dt2.Rows(0)(0)

                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M7_Aq_Detail where idaq = " & result(4)
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
                        strValue2.Append("('" & FixQuotes(dr1("idaqdetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("idasset")) & "', '" & FixQuotes(dr1("namaasset")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", '" & FixQuotes(dr1("idardetail")) & "', '" & FixDouble(dr1("jmlao")) & "', " & dr1("statusao") & ", '" & FixDouble(dr1("jmlae")) & "', " & dr1("statusae") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(dr1("satuan")) & "')")
                    Next
                    sql = "Insert into M7_Aq_Detail(idaqdetail, idaq, idasset, namaasset, jml, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, costcenter, divisi, subdivisi, proyek, catatan, urutan, idardetail, jmlao, statusao, jmlae, statusae, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, satuan) values" & strValue2.ToString & ""
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

                If drutama("aqstatus") = 2 Then
                    If Len(updNilai) > 0 Then
                        'UPDATE OUTSTANDING TRANSAKSI =======================================================
                        'UPDATE DETAIL
                        sql = "UPDATE M7_ar_detail SET jmlaq = (CASE idardetail " & updNilai & " ELSE jmlaq END) WHERE " & updFilter
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idar FROM M7_ar_detail WHERE " & updFilter & " GROUP BY idar")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idar = '" & dr1("idar") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDB("SELECT idar, SUM(jml) as jml, SUM(jmlaq) as jmlaq FROM M7_ar_detail WHERE " & ftDetail & " GROUP BY idar")
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilai = "" : updFilter = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlaq") >= dr1("jml") Then
                                    statusOut = 2
                                ElseIf dr1("jmlaq") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilai = String.Concat(updNilai, "WHEN '" & dr1("idar") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                                updFilter = String.Concat(updFilter, "(arid = '" & dr1("idar") & "')")
                            Next

                            sql = "UPDATE M7_ar SET arstatusaq = (CASE arid " & updNilai & " ELSE arstatusaq END) WHERE " & updFilter
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
                End If

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "AQ", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M7_AqGetdataById(ByVal param As String) As String

        'M7_AqGetdataById Utama --------------------------------------------------------
        'aqid, aqcabang, aqlokasi, aqsumber, aqautonogrup, aqnogrup, aqautonotransaksi, 
        'aqnotransaksi, aqtgl, aqkodepa, aqsupplier, aqsupplierkontak, aq1alamat1, aq1alamat2, 
        'aq1alamat3, aq2alamat1, aq2alamat2, aq2alamat3, aqbagianpembelian, aqtgldipenuhi, aqtermin, 
        'aqtgljatuhtempo, aquraian, aqcatatan, aqnoref, aqtglnoref, aqtglpenutupan, aqmatauang, 
        'aqkurs, aqhargatermasukpajak, aqtotal, aqdiskonpersen, aqdiskon, aqtotalpajak1detail, aqtotalpajak2detail, 
        'aqbiayalainpersen, aqbiayalain, aqtotaltransaksi, aqidar, aqstatusao, aqstatusae, aqstatusrealisasi, 
        'aqstatus, aqstatussebelumnya, aqjmlrevisi, aqcetakanke, aqinputuser, aqinputtgl, aqmodifikasiuser, 
        'aqmodifikasitgl, aqposting, aqpostingtgl, aqisclose, aqcustomtext1, aqcustomtext2, aqcustomtext3, 
        'aqcustomtext4, aqcustomtext5, aqcustomint1, aqcustomint2, aqcustomint3, aqcustomdbl1, aqcustomdbl2, 
        'aqcustomdbl3, aqcustomdate1, aqcustomdate2, aqcustomdate3, aqcabangnama, aqlokasinama, aqsupplierkode, 
        'aqsuppliernama, aqbagianpembeliankode, aqbagianpembeliannama, aqterminnama, aqtermindiskon1, aqterminharidiskon1, aqtermindiskon2, 
        'aqterminharidiskon2, aqtermindenda, aqtermindendaper, aqterminharijatuhtempo, aqnotransaksiar, aqstatusnama, aqstatussebelumnyanama, 
        'aqinputusernama, aqmodifikasiusernama

        'M7_AqGetdataById Detail -------------------------------------------------------
        'idaqdetail, idaq, idasset, namaasset, jml, 
        'matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, 
        'pajak2, jmlpajak2, cabang, lokasi, costcenter, divisi, subdivisi, 
        'proyek, catatan, urutan, idardetail, jmlao, statusao, jmlae, 
        'statusae, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, pajak1nama, 
        'pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, costcenternama, divisinama, 
        'subdivisinama, proyeknama, arnotransaksi, jmlsisaao, jmlsisarealisasi, satuan

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

        Dim NmMemcached As String = "aplikasi1-M4_aq~M4_aq_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "aqid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "aqid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m7_aq_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                  FxDB(drutama("aqid"), ""), sptField,
                     FxDB(drutama("aqcabang"), ""), sptField,
                     FxDB(drutama("aqlokasi"), ""), sptField,
                     FxDB(drutama("aqsumber"), ""), sptField,
                     FxDB(drutama("aqautonogrup"), 0), sptField,
                     FxDB(drutama("aqnogrup"), ""), sptField,
                     FxDB(drutama("aqautonotransaksi"), 0), sptField,
                     FxDB(drutama("aqnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aqtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("aqkodepa"), ""), sptField,
                     FxDB(drutama("aqsupplier"), ""), sptField,
                     FxDB(drutama("aqsupplierkontak"), ""), sptField,
                     FxDB(drutama("aq1alamat1"), ""), sptField,
                     FxDB(drutama("aq1alamat2"), ""), sptField,
                     FxDB(drutama("aq1alamat3"), ""), sptField,
                     FxDB(drutama("aq2alamat1"), ""), sptField,
                     FxDB(drutama("aq2alamat2"), ""), sptField,
                     FxDB(drutama("aq2alamat3"), ""), sptField,
                     FxDB(drutama("aqbagianpembelian"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aqtgldipenuhi"), ""), formatTgl), sptField,
                     FxDB(drutama("aqtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aqtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("aquraian"), ""), sptField,
                     FxDB(drutama("aqcatatan"), ""), sptField,
                     FxDB(drutama("aqnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aqtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("aqtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("aqmatauang"), ""), sptField,
                     FxDB(drutama("aqkurs"), 0), sptField,
                     FxDB(drutama("aqhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("aqtotal"), 0), sptField,
                     FxDB(drutama("aqdiskonpersen"), ""), sptField,
                     FxDB(drutama("aqdiskon"), 0), sptField,
                     FxDB(drutama("aqtotalpajak1detail"), 0), sptField,
                     FxDB(drutama("aqtotalpajak2detail"), 0), sptField,
                     FxDB(drutama("aqbiayalainpersen"), ""), sptField,
                     FxDB(drutama("aqbiayalain"), 0), sptField,
                     FxDB(drutama("aqtotaltransaksi"), 0), sptField,
                     FxDB(drutama("aqidar"), ""), sptField,
                     FxDB(drutama("aqstatusao"), 0), sptField,
                     FxDB(drutama("aqstatusae"), 0), sptField,
                     FxDB(drutama("aqstatusrealisasi"), 0), sptField,
                     FxDB(drutama("aqstatus"), 0), sptField,
                     FxDB(drutama("aqstatussebelumnya"), 0), sptField,
                     FxDB(drutama("aqjmlrevisi"), 0), sptField,
                     FxDB(drutama("aqcetakanke"), 0), sptField,
                     FxDB(drutama("aqinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aqinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("aqmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aqmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("aqposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("aqpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("aqisclose"), 0), sptField,
                     FxDB(drutama("aqcustomtext1"), ""), sptField,
                     FxDB(drutama("aqcustomtext2"), ""), sptField,
                     FxDB(drutama("aqcustomtext3"), ""), sptField,
                     FxDB(drutama("aqcustomtext4"), ""), sptField,
                     FxDB(drutama("aqcustomtext5"), ""), sptField,
                     FxDB(drutama("aqcustomint1"), 0), sptField,
                     FxDB(drutama("aqcustomint2"), 0), sptField,
                     FxDB(drutama("aqcustomint3"), 0), sptField,
                     FxDB(drutama("aqcustomdbl1"), 0), sptField,
                     FxDB(drutama("aqcustomdbl2"), 0), sptField,
                     FxDB(drutama("aqcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("aqcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("aqcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("aqcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("aqcabangnama"), ""), sptField,
                     FxDB(drutama("aqlokasinama"), ""), sptField,
                     FxDB(drutama("aqsupplierkode"), ""), sptField,
                     FxDB(drutama("aqsuppliernama"), ""), sptField,
                     FxDB(drutama("aqbagianpembeliankode"), ""), sptField,
                     FxDB(drutama("aqbagianpembeliannama"), ""), sptField,
                     FxDB(drutama("aqterminnama"), ""), sptField,
                     FxDB(drutama("aqtermindiskon1"), 0), sptField,
                     FxDB(drutama("aqterminharidiskon1"), 0), sptField,
                     FxDB(drutama("aqtermindiskon2"), 0), sptField,
                     FxDB(drutama("aqterminharidiskon2"), 0), sptField,
                     FxDB(drutama("aqtermindenda"), 0), sptField,
                     FxDB(drutama("aqtermindendaper"), 0), sptField,
                     FxDB(drutama("aqterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("aqnotransaksiar"), ""), sptField,
                     FxDB(drutama("aqstatusnama"), ""), sptField,
                     FxDB(drutama("aqstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("aqinputusernama"), ""), sptField,
                     FxDB(drutama("aqmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                    FxDB(dr("idaqdetail"), ""), sptField,
                     FxDB(dr("idaq"), ""), sptField,
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
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idardetail"), ""), sptField,
                     FxDB(dr("jmlao"), 0), sptField,
                     FxDB(dr("statusao"), 0), sptField,
                     FxDB(dr("jmlae"), 0), sptField,
                     FxDB(dr("statusae"), 0), sptField,
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
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("cabangnama"), ""), sptField,
                     FxDB(dr("lokasinama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("arnotransaksi"), ""), sptField,
                     FxDB(dr("jmlsisaao"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
                     FxDB(dr("satuan"), 0), sptRow)

            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

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
        strResultData = String.Concat(utama, sptSubParam, detail)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("aqid, aqcabang, aqlokasi, aqsumber, aqautonogrup, aqnogrup, aqautonotransaksi, aqnotransaksi, aqtgl, aqkodepa, aqsupplier, aqsupplierkontak, aq1alamat1, aq1alamat2, aq1alamat3, aq2alamat1, aq2alamat2, aq2alamat3, aqbagianpembelian, aqtgldipenuhi, aqtermin, aqtgljatuhtempo, aquraian, aqcatatan, aqnoref,aqtglnoref, aqtglpenutupan, aqmatauang, aqkurs, aqhargatermasukpajak, aqtotal, aqdiskonpersen, aqdiskon, aqtotalpajak1detail, aqtotalpajak2detail, aqbiayalainpersen, aqbiayalain, aqtotaltransaksi, aqidar, aqstatusao, aqstatusae, aqstatusrealisasi, aqstatus, aqstatussebelumnya, aqjmlrevisi, aqcetakanke, aqinputuser, aqinputtgl, aqmodifikasiuser, aqmodifikasitgl, aqposting, aqpostingtgl, aqisclose, aqcustomtext1, aqcustomtext2, aqcustomtext3, aqcustomtext4, aqcustomtext5, aqcustomint1, aqcustomint2, aqcustomint3, aqcustomdbl1, aqcustomdbl2, aqcustomdbl3, aqcustomdate1, aqcustomdate2, aqcustomdate3, aqcabangnama, aqlokasinama, aqsupplierkode, aqsuppliernama, aqbagianpembeliankode, aqbagianpembeliannama, aqterminnama, aqtermindiskon1, aqterminharidiskon1, aqtermindiskon2, aqterminharidiskon2, aqtermindenda, aqtermindendaper, aqterminharijatuhtempo, aqnotransaksiar, aqstatusnama, aqstatussebelumnyanama, aqinputusernama, aqmodifikasiusernama" & sptSubParam & "idaqdetail, idaq, idasset, namaasset, jml, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, costcenter, divisi, subdivisi, proyek, catatan, urutan, idardetail, jmlao, statusao, jmlae, statusae, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, costcenternama, divisinama, subdivisinama, proyeknama, arnotransaksi, jmlsisaao, jmlsisarealisasi, satuan"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M7_AqSearch(ByVal param As String) As String
        'M4_AqSearch --------------------------------------------------------
        'aqid, aqcabang, aqlokasi, aqsumber, aqautonogrup, aqnogrup, aqautonotransaksi, 
        'aqnotransaksi, aqtgl, aqkodepa, aqsupplier, aqsupplierkontak, aq1alamat1, aq1alamat2, 
        'aq1alamat3, aq2alamat1, aq2alamat2, aq2alamat3, aqbagianpembelian, aqtgldipenuhi, aqtermin, 
        'aqtgljatuhtempo, aquraian, aqcatatan, aqnoref, aqtglnoref, aqtglpenutupan, aqmatauang, 
        'aqkurs, aqhargatermasukpajak, aqtotal, aqdiskonpersen, aqdiskon, aqtotalpajak1detail, aqtotalpajak2detail, 
        'aqbiayalainpersen, aqbiayalain, aqtotaltransaksi, aqidar, aqstatusao, aqstatusae, aqstatusrealisasi, 
        'aqstatus, aqstatussebelumnya, aqjmlrevisi, aqcetakanke, aqinputuser, aqinputtgl, aqmodifikasiuser, 
        'aqmodifikasitgl, aqposting, aqpostingtgl, aqisclose, aqcabangnama, aqlokasinama, aqsupplierkode, 
        'aqsuppliernama, aqbagianpembeliankode, aqbagianpembeliannama, arnotransaksi, aqstatusnama, aqstatussebelumnyanama, aqinputusernama, 
        'aqmodifikasiusernama

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
            Filter = Filter.Replace("aqsupplierkode", "c1.kkode")
            Filter = Filter.Replace("aqsuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        Dim query As New m0_query
        sql = query.PanggilQuery("m7_aq_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_aq", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                   FxDB(dr("aqid"), ""), sptField,
                     FxDB(dr("aqcabang"), ""), sptField,
                     FxDB(dr("aqlokasi"), ""), sptField,
                     FxDB(dr("aqsumber"), ""), sptField,
                     FxDB(dr("aqautonogrup"), 0), sptField,
                     FxDB(dr("aqnogrup"), ""), sptField,
                     FxDB(dr("aqautonotransaksi"), 0), sptField,
                     FxDB(dr("aqnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aqtgl"), ""), formatTgl), sptField,
                     FxDB(dr("aqkodepa"), ""), sptField,
                     FxDB(dr("aqsupplier"), ""), sptField,
                     FxDB(dr("aqsupplierkontak"), ""), sptField,
                     FxDB(dr("aq1alamat1"), ""), sptField,
                     FxDB(dr("aq1alamat2"), ""), sptField,
                     FxDB(dr("aq1alamat3"), ""), sptField,
                     FxDB(dr("aq2alamat1"), ""), sptField,
                     FxDB(dr("aq2alamat2"), ""), sptField,
                     FxDB(dr("aq2alamat3"), ""), sptField,
                     FxDB(dr("aqbagianpembelian"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aqtgldipenuhi"), ""), formatTgl), sptField,
                     FxDB(dr("aqtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aqtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("aquraian"), ""), sptField,
                     FxDB(dr("aqcatatan"), ""), sptField,
                     FxDB(dr("aqnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aqtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("aqtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("aqmatauang"), ""), sptField,
                     FxDB(dr("aqkurs"), 0), sptField,
                     FxDB(dr("aqhargatermasukpajak"), 0), sptField,
                     FxDB(dr("aqtotal"), 0), sptField,
                     FxDB(dr("aqdiskonpersen"), ""), sptField,
                     FxDB(dr("aqdiskon"), 0), sptField,
                     FxDB(dr("aqtotalpajak1detail"), 0), sptField,
                     FxDB(dr("aqtotalpajak2detail"), 0), sptField,
                     FxDB(dr("aqbiayalainpersen"), ""), sptField,
                     FxDB(dr("aqbiayalain"), 0), sptField,
                     FxDB(dr("aqtotaltransaksi"), 0), sptField,
                     FxDB(dr("aqidar"), ""), sptField,
                     FxDB(dr("aqstatusao"), 0), sptField,
                     FxDB(dr("aqstatusae"), 0), sptField,
                     FxDB(dr("aqstatusrealisasi"), 0), sptField,
                     FxDB(dr("aqstatus"), 0), sptField,
                     FxDB(dr("aqstatussebelumnya"), 0), sptField,
                     FxDB(dr("aqjmlrevisi"), 0), sptField,
                     FxDB(dr("aqcetakanke"), 0), sptField,
                     FxDB(dr("aqinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aqinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("aqmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aqmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("aqposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("aqpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("aqisclose"), 0), sptField,
                     FxDB(dr("aqcabangnama"), ""), sptField,
                     FxDB(dr("aqlokasinama"), ""), sptField,
                     FxDB(dr("aqsupplierkode"), ""), sptField,
                     FxDB(dr("aqsuppliernama"), ""), sptField,
                     FxDB(dr("aqbagianpembeliankode"), ""), sptField,
                     FxDB(dr("aqbagianpembeliannama"), ""), sptField,
                     FxDB(dr("arnotransaksi"), ""), sptField,
                     FxDB(dr("aqstatusnama"), ""), sptField,
                     FxDB(dr("aqstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("aqinputusernama"), ""), sptField,
                     FxDB(dr("aqmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("aqid, aqcabang, aqlokasi, aqsumber, aqautonogrup, aqnogrup, aqautonotransaksi, aqnotransaksi, aqtgl, aqkodepa, aqsupplier, aqsupplierkontak, aq1alamat1, aq1alamat2, q1alamat3, aq2alamat1, aq2alamat2, aq2alamat3, aqbagianpembelian, aqtgldipenuhi, aqtermin, aqtgljatuhtempo, aquraian, aqcatatan, aqnoref, aqtglnoref, aqtglpenutupan, aqmatauang, aqkurs, aqhargatermasukpajak, aqtotal, aqdiskonpersen, aqdiskon, aqtotalpajak1detail, aqtotalpajak2detail, aqbiayalainpersen, aqbiayalain, aqtotaltransaksi, aqidar, aqstatusao, aqstatusae, aqstatusrealisasi, aqstatus, aqstatussebelumnya, aqjmlrevisi, aqcetakanke, aqinputuser, aqinputtgl, aqmodifikasiuser, aqmodifikasitgl, aqposting, aqpostingtgl, aqisclose, aqcabangnama, aqlokasinama, aqsupplierkode, aqsuppliernama, aqbagianpembeliankode, aqbagianpembeliannama, arnotransaksi, aqstatusnama, aqstatussebelumnyanama, aqinputusernama, aqmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M7_Aq_Detail_VSearch(ByVal param As String) As String
        'M7_Aq_Detail_VSearch --------------------------------------------------------
        'idaqdetail, idaq, idasset, namaasset, jml, matauang, kurs, harga, diskon, 
        'jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, 
        'costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idardetail, jmlao, statusao, jmlae, statusae, 
        'jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3, aqnotransaksi, aqtgldipenuhi, 
        'aquraian, aqcatatan, aqnoref, aqtglnoref, aqsupplierkontak, aq1alamat1, aq1alamat2, 
        'aq1alamat3, aq2alamat1, aq2alamat2, aq2alamat3, aqtermin, aqterminnama, aqterminharijatuhtempo, 
        'aqbagianpembelian, aqbagianpembeliankode, aqbagianpembeliannama, pajak1nama, pajak1nilai, pajak2nama, 
        'pajak2nilai, jmlsisaao, jmlsisarealisasi, arnotransaksi, satuan

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
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m7_aq_detail_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M5_Sq_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idaqdetail"), 0), sptField,
                     FxDB(dr("idaq"), 0), sptField,
                     FxDB(dr("idasset"), 0), sptField,
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
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idardetail"), 0), sptField,
                     FxDB(dr("jmlao"), 0), sptField,
                     FxDB(dr("statusao"), 0), sptField,
                     FxDB(dr("jmlae"), 0), sptField,
                     FxDB(dr("statusae"), 0), sptField,
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
                     FxDB(dr("aqnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aqtgldipenuhi"), ""), formatTgl), sptField,
                     FxDB(dr("aquraian"), ""), sptField,
                     FxDB(dr("aqcatatan"), ""), sptField,
                     FxDB(dr("aqnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aqtglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("aqsupplierkontak"), ""), sptField,
                     FxDB(dr("aq1alamat1"), ""), sptField,
                     FxDB(dr("aq1alamat2"), ""), sptField,
                     FxDB(dr("aq1alamat3"), ""), sptField,
                     FxDB(dr("aq2alamat1"), ""), sptField,
                     FxDB(dr("aq2alamat2"), ""), sptField,
                     FxDB(dr("aq2alamat3"), ""), sptField,
                     FxDB(dr("aqtermin"), ""), sptField,
                     FxDB(dr("aqterminnama"), ""), sptField,
                     FxDB(dr("aqterminharijatuhtempo"), 0), sptField,
                     FxDB(dr("aqbagianpembelian"), 0), sptField,
                     FxDB(dr("aqbagianpembeliankode"), ""), sptField,
                     FxDB(dr("aqbagianpembeliannama"), ""), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("jmlsisaao"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
                     FxDB(dr("arnotransaksi"), ""), sptField,
                     FxDB(dr("satuan"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idaqdetail, idaq, idasset, namaasset, jml, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, costcenter, divisi, subdivisi, proyek, catatan, urutan, idardetail, jmlao, statusao, jmlae, statusae, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, aqnotransaksi, aqtgldipenuhi, aquraian, aqcatatan, aqnoref, aqtglnoref, aqsupplierkontak, aq1alamat1, aq1alamat2, aq1alamat3, aq2alamat1, aq2alamat2, aq2alamat3, aqtermin, aqterminnama, aqterminharijatuhtempo, aqbagianpembelian, aqbagianpembeliankode, aqbagianpembeliannama, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisaao, jmlsisarealisasi, arnotransaksi, satuan"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M7_AqTerkait(ByVal param As String) As String
        'M7_AqTerkait --------------------------------------------------------
        'aqid, aqnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "aqid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m7_aq_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                    FxDB(dr("aqid"), 0), sptField,
                     FxDB(dr("aqnotransaksi"), ""), sptField,
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
            result(2) = sql
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("aqid, aqnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M7_AqUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("aqsupplierkode", "c1.kkode")
            Filter = Filter.Replace("aqsuppliernama", "c1.knama")
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
            Dim sumber As String = "Aq", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Aqtgl, Aqnotransaksi, Aqstatus FROM M7_Aq WHERE Aqid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Aqstatussebelumnya" : jnsaktivitas = 17
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
            'Dim SimpanHistory As New m4_rq_history
            'Dim rsSimpanHistory As String = SimpanHistory.M4_Rq_HistorySimpan("" & paramSplit(0) & "★M4_Rq_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            'If (rsSplitResult(1) = 0) Then
            '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            'End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then
                'CEK TERKAIT ====================================================================
                'PANGGIL QUERY TERKAIT
                Dim query As New m0_query
                sql = query.PanggilQuery("m7_aq_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                Dim idasset As Integer = 0, jml As Double = 0, idardetail As Integer = 0
                Dim ftOutstanding As String = "", updNilai As String = "", updFilter As String = ""
                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDB("SELECT idasset, namaasset, satuan, jml, idardetail, urutan FROM M7_aq_detail WHERE idaq = '" & idtransaksi & "'")
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idasset = dr1("idasset") : jml = dr1("jml") : idardetail = dr1("idardetail")

                        'UPDATE OUTSTANDING ---------------------------
                        If idardetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING ----------
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jml", "idardetail=" & idardetail)
                            updNilai = String.Concat("WHEN '" & idardetail & "' THEN ROUND(jmlaq - '" & Outstanding & "', 5) ", updNilai)
                            '2. SET FILTERUPDATE OUTSTANDING ----------
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(idardetail = '" & idardetail & "')")
                        End If
                        'END OF BUAT FILTER UNTUK UPDATE --------------------------
                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                If Len(updFilter) > 0 Then
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE M7_ar_detail SET jmlaq = (CASE idardetail " & updNilai & " ELSE jmlaq END) WHERE " & updFilter
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idar FROM M7_ar_detail WHERE " & updFilter & " GROUP BY idar")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idar = '" & dr1("idar") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDB("SELECT idar, SUM(jml) as jml, SUM(jmlaq) as jmlaq FROM M7_ar_detail WHERE " & ftDetail & " GROUP BY idar")
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilai = "" : updFilter = ""
                        For Each dr1 As DataRow In dtOut.Rows
                            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                            If dr1("jmlaq") >= dr1("jml") Then
                                statusOut = 2
                            ElseIf dr1("jmlaq") < 1 Then
                                statusOut = 0
                            Else
                                statusOut = 1
                            End If
                            '2. SET NILAI UPDATE OUTSTANDING
                            updNilai = String.Concat(updNilai, "WHEN '" & dr1("idar") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(arid = '" & dr1("idar") & "')")
                        Next

                        sql = "UPDATE M7_ar SET arstatusaq = (CASE arid " & updNilai & " ELSE arstatusaq END) WHERE " & updFilter
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
            End If

            'update status utama
            sql = "UPDATE M7_Aq SET Aqstatus = " & nilaiStatus & ", Aqmodifikasiuser='" & userid & "', Aqmodifikasitgl = NOW(), Aqposting = 0, Aqpostingtgl = '1971-01-01 00:00:00', Aqjmlrevisi = Aqjmlrevisi + 1 WHERE Aqid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M7_AqSearch(PostWsSearch(paramSplit(0), "M7_AqSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M7_AqDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("aqsupplierkode", "c1.kkode")
            Filter = Filter.Replace("aqsuppliernama", "c1.knama")
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
            Dim sumber As String = "Aq", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Aqid, Aqnotransaksi FROM M7_Aq WHERE Aqid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT aqcabang, aqlokasi, aqsumber, aqautonotransaksi, aqnotransaksi, aqtgl"
            sql &= " FROM M7_aq"
            sql &= " WHERE aqid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("aqcabang")
                lokasi = dtNomorNext.Rows(0)("aqlokasi")
                sumber = dtNomorNext.Rows(0)("aqsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("aqautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("aqnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("aqtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M7_Aq_Detail WHERE idaq = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M7_Aq WHERE aqid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M7_AqSearch(PostWsSearch(paramSplit(0), "M7_AqSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
