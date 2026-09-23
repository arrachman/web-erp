Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m7_ao
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M7_AoSimpan(ByVal param As String) As String
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
        'aoid(0) As , aocabang(1) As String, aolokasi(2) As String, aosumber(3) As String, aoautonotransaksi(4) As Integer, 
        'aonotransaksi(5) As String, aotgl(6) As Date, aokodepa(7) As , aosupplier(8) As , aosupplierkontak(9) As String, 
        'ao1alamat1(10) As String, ao1alamat2(11) As String, ao1alamat3(12) As String, ao2alamat1(13) As String, ao2alamat2(14) As String, 
        'ao2alamat3(15) As String, aobagianpembelian(16) As , aotgldipenuhi(17) As Date, aotermin(18) As String, aotgljatuhtempo(19) As Date, 
        'aouraian(20) As String, aocatatan(21) As String, aonoref(22) As String, aotglnoref(23) As Date, aotglpenutupan(24) As Date, 
        'aomatauang(25) As String, aokurs(26) As Double, aohargatermasukpajak(27) As Integer, aototal(28) As Double, aodiskonpersen(29) As String, 
        'aojmldiskon(30) As Double, aototalpajak1detail(31) As Double, aototalpajak2detail(32) As Double, aobiayalainpersen(33) As String, aobiayalain(34) As Double, 
        'aototaltransaksi(35) As Double, aojmlbayar(36) As Double, aorekdiskon(37) As String, aorekpajak1(38) As String, aorekpajak2(39) As String, 
        'aorekbiayalain(40) As String, aorekbayar(41) As String, aoidar(42) As , aoidab(43) As , aostatusae(44) As Integer, 
        'aostatus(45) As Integer, aostatussebelumnya(46) As Integer, aojmlrevisi(47) As Integer, 
        'aocetakanke(48) As Integer, aoinputuser(49) As , aoinputtgl(50) As DateTime, aomodifikasiuser(51) As , aomodifikasitgl(52) As DateTime, 
        'aoposting(53) As Integer, aopostingtgl(54) As DateTime, aoisclose(55) As Integer, aocustomtext1(56) As String, aocustomtext2(57) As String, 
        'aocustomtext3(58) As String, aocustomtext4(59) As String, aocustomtext5(60) As String, aocustomint1(61) As Integer, aocustomint2(62) As Integer, 
        'aocustomint3(63) As Integer, aocustomdbl1(64) As Double, aocustomdbl2(65) As Double, aocustomdbl3(66) As Double, aocustomdate1(67) As Date, 
        'aocustomdate2(68) As Date, aocustomdate3(69) As Date, aoidaq(70) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'aoid, aocabang, aolokasi, aosumber, aoautonotransaksi, aonotransaksi, aotgl, 
        'aokodepa, aosupplier, aosupplierkontak, ao1alamat1, ao1alamat2, ao1alamat3, ao2alamat1, 
        'ao2alamat2, ao2alamat3, aobagianpembelian, aotgldipenuhi, aotermin, aotgljatuhtempo, aouraian, 
        'aocatatan, aonoref, aotglnoref, aotglpenutupan, aomatauang, aokurs, aohargatermasukpajak, 
        'aototal, aodiskonpersen, aojmldiskon, aototalpajak1detail, aototalpajak2detail, aobiayalainpersen, aobiayalain, 
        'aototaltransaksi, aojmlbayar, aorekdiskon, aorekpajak1, aorekpajak2, aorekbiayalain, aorekbayar, 
        'aoidar, aoidab, aostatusae, aostatus, aostatussebelumnya, 
        'aojmlrevisi, aocetakanke, aoinputuser, aoinputtgl, aomodifikasiuser, aomodifikasitgl, aoposting, 
        'aopostingtgl, aoisclose, aocustomtext1, aocustomtext2, aocustomtext3, aocustomtext4, aocustomtext5, 
        'aocustomint1, aocustomint2, aocustomint3, aocustomdbl1, aocustomdbl2, aocustomdbl3, aocustomdate1, 
        'aocustomdate2, aocustomdate3, aoidaq

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 71) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'aoautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "aoautonotransaksi required numeric." : GoTo selesai
        End If
        'aotgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "aotgl required date." : GoTo selesai
        End If
        'aotgldipenuhi(17) As Date
        If (IsDate(dataUtama(17)) = False) Then
            result(2) = "aotgldipenuhi required date." : GoTo selesai
        End If
        'aotgljatuhtempo(19) As Date
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "aotgljatuhtempo required date." : GoTo selesai
        End If
        'aotglnoref(23) As Date
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "aotglnoref required date." : GoTo selesai
        End If
        'aotglpenutupan(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "aotglpenutupan required date." : GoTo selesai
        End If
        'aokurs(26) As Double
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "aokurs required numeric." : GoTo selesai
        End If
        'aohargatermasukpajak(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "aohargatermasukpajak required numeric." : GoTo selesai
        End If
        'aototal(28) As Double
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "aototal required numeric." : GoTo selesai
        End If
        'aojmldiskon(30) As Double
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "aojmldiskon required numeric." : GoTo selesai
        End If
        'aototalpajak1detail(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "aototalpajak1detail required numeric." : GoTo selesai
        End If
        'aototalpajak2detail(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "aototalpajak2detail required numeric." : GoTo selesai
        End If
        'aobiayalain(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "aobiayalain required numeric." : GoTo selesai
        End If
        'aototaltransaksi(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "aototaltransaksi required numeric." : GoTo selesai
        End If
        'aojmlbayar(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "aojmlbayar required numeric." : GoTo selesai
        End If
        'aostatusae(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "aostatusae required numeric." : GoTo selesai
        End If

        'aostatus(47) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "aostatus required numeric." : GoTo selesai
        End If
        'aostatussebelumnya(48) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "aostatussebelumnya required numeric." : GoTo selesai
        End If
        'aojmlrevisi(49) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "aojmlrevisi required numeric." : GoTo selesai
        End If
        'aocetakanke(50) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "aocetakanke required numeric." : GoTo selesai
        End If
        'aoinputtgl(52) As DateTime
        If (IsDate(dataUtama(50)) = False) Then
            result(2) = "aoinputtgl required date." : GoTo selesai
        End If
        'aomodifikasitgl(54) As DateTime
        If (IsDate(dataUtama(52)) = False) Then
            result(2) = "aomodifikasitgl required date." : GoTo selesai
        End If
        'aoposting(55) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "aoposting required numeric." : GoTo selesai
        End If
        'aopostingtgl(56) As DateTime
        If (IsDate(dataUtama(54)) = False) Then
            result(2) = "aopostingtgl required date." : GoTo selesai
        End If
        'aoisclose(57) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "aoisclose required numeric." : GoTo selesai
        End If
        'aocustomint1(63) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "aocustomint1 required numeric." : GoTo selesai
        End If
        'aocustomint2(64) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "aocustomint2 required numeric." : GoTo selesai
        End If
        'aocustomint3(65) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "aocustomint3 required numeric." : GoTo selesai
        End If
        'aocustomdbl1(66) As Double
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "aocustomdbl1 required numeric." : GoTo selesai
        End If
        'aocustomdbl2(67) As Double
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "aocustomdbl2 required numeric." : GoTo selesai
        End If
        'aocustomdbl3(68) As Double
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "aocustomdbl3 required numeric." : GoTo selesai
        End If
        'aocustomdate1(69) As Date
        If (IsDate(dataUtama(67)) = False) Then
            result(2) = "aocustomdate1 required date." : GoTo selesai
        End If
        'aocustomdate2(70) As Date
        If (IsDate(dataUtama(68)) = False) Then
            result(2) = "aocustomdate2 required date." : GoTo selesai
        End If
        'aocustomdate3(71) As Date
        If (IsDate(dataUtama(69)) = False) Then
            result(2) = "aocustomdate3 required date." : GoTo selesai
        End If
        'aoidaq(65) As Integer
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "aoidaq required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'aoid(0) As 
        If Len(dataUtama(0)) = 0 Then
            result(2) = "aoid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 20 Then
            result(2) = "aoid should not be more than 20 character." : GoTo selesai
        End If

        'aocabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "aocabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "aocabang should not be more than 25 character." : GoTo selesai
        End If

        'aolokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "aolokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "aolokasi should not be more than 25 character." : GoTo selesai
        End If

        'aosumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "aosumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "aosumber should not be more than 10 character." : GoTo selesai
        End If

        'aonotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "aonotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "aonotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'aotgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "aotgl can't be empty" : GoTo selesai
        End If

        'aokodepa(7) As 
        If Len(dataUtama(7)) = 0 Then
            result(2) = "aokodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 20 Then
            result(2) = "aokodepa should not be more than 20 character." : GoTo selesai
        End If

        'aosupplier(8) As 
        If Len(dataUtama(8)) = 0 Then
            result(2) = "aosupplier can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 20 Then
            result(2) = "aosupplier should not be more than 20 character." : GoTo selesai
        End If

        'aobagianpembelian(16) As 
        If Len(dataUtama(16)) = 0 Then
            result(2) = "aobagianpembelian can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(16)) > 20 Then
            result(2) = "aobagianpembelian should not be more than 20 character." : GoTo selesai
        End If

        'aotgldipenuhi(17) As Date
        If Len(dataUtama(17)) = 0 Then
            result(2) = "aotgldipenuhi can't be empty" : GoTo selesai
        End If

        'aotgljatuhtempo(19) As Date
        If Len(dataUtama(19)) = 0 Then
            result(2) = "aotgljatuhtempo can't be empty" : GoTo selesai
        End If

        'aotglnoref(23) As Date
        If Len(dataUtama(23)) = 0 Then
            result(2) = "aotglnoref can't be empty" : GoTo selesai
        End If

        'aotglpenutupan(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "aotglpenutupan can't be empty" : GoTo selesai
        End If

        'aomatauang(25) As String
        If Len(dataUtama(25)) = 0 Then
            result(2) = "aomatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(25)) > 25 Then
            result(2) = "aomatauang should not be more than 25 character." : GoTo selesai
        End If

        'aokurs(26) As Double
        If Len(dataUtama(26)) = 0 Then
            result(2) = "aokurs can't be empty" : GoTo selesai
        End If

        'aototal(28) As Double
        If Len(dataUtama(28)) = 0 Then
            result(2) = "aototal can't be empty" : GoTo selesai
        End If

        'aodiskonpersen(29) As String
        If Len(dataUtama(29)) = 0 Then
            result(2) = "aodiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(29)) > 25 Then
            result(2) = "aodiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'aojmldiskon(30) As Double
        If Len(dataUtama(30)) = 0 Then
            result(2) = "aojmldiskon can't be empty" : GoTo selesai
        End If

        'aototalpajak1detail(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "aototalpajak1detail can't be empty" : GoTo selesai
        End If

        'aototalpajak2detail(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "aototalpajak2detail can't be empty" : GoTo selesai
        End If

        'aobiayalainpersen(33) As String
        If Len(dataUtama(33)) = 0 Then
            result(2) = "aobiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(33)) > 25 Then
            result(2) = "aobiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'aobiayalain(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "aobiayalain can't be empty" : GoTo selesai
        End If

        'aototaltransaksi(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "aototaltransaksi can't be empty" : GoTo selesai
        End If

        'aojmlbayar(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "aojmlbayar can't be empty" : GoTo selesai
        End If

        'aoidar(42) As 
        If Len(dataUtama(42)) = 0 Then
            result(2) = "aoidar can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(42)) > 20 Then
            result(2) = "aoidar should not be more than 20 character." : GoTo selesai
        End If

        'aoidab(43) As 
        If Len(dataUtama(43)) = 0 Then
            result(2) = "aoidab can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(43)) > 20 Then
            result(2) = "aoidab should not be more than 20 character." : GoTo selesai
        End If

        'aoinputuser(51) As 
        If Len(dataUtama(49)) = 0 Then
            result(2) = "aoinputuser can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(49)) > 20 Then
            result(2) = "aoinputuser should not be more than 20 character." : GoTo selesai
        End If

        'aoinputtgl(52) As DateTime
        If Len(dataUtama(50)) = 0 Then
            result(2) = "aoinputtgl can't be empty" : GoTo selesai
        End If

        'aomodifikasiuser(53) As 
        If Len(dataUtama(51)) = 0 Then
            result(2) = "aomodifikasiuser can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(51)) > 20 Then
            result(2) = "aomodifikasiuser should not be more than 20 character." : GoTo selesai
        End If

        'aomodifikasitgl(54) As DateTime
        If Len(dataUtama(52)) = 0 Then
            result(2) = "aomodifikasitgl can't be empty" : GoTo selesai
        End If

        'aopostingtgl(56) As DateTime
        If Len(dataUtama(54)) = 0 Then
            result(2) = "aopostingtgl can't be empty" : GoTo selesai
        End If

        'aocustomdbl1(66) As Double
        If Len(dataUtama(64)) = 0 Then
            result(2) = "aocustomdbl1 can't be empty" : GoTo selesai
        End If

        'aocustomdbl2(67) As Double
        If Len(dataUtama(65)) = 0 Then
            result(2) = "aocustomdbl2 can't be empty" : GoTo selesai
        End If

        'aocustomdbl3(68) As Double
        If Len(dataUtama(66)) = 0 Then
            result(2) = "aocustomdbl3 can't be empty" : GoTo selesai
        End If

        'aocustomdate1(69) As Date
        If Len(dataUtama(67)) = 0 Then
            result(2) = "aocustomdate1 can't be empty" : GoTo selesai
        End If

        'aocustomdate2(70) As Date
        If Len(dataUtama(68)) = 0 Then
            result(2) = "aocustomdate2 can't be empty" : GoTo selesai
        End If

        'aocustomdate3(71) As Date
        If Len(dataUtama(69)) = 0 Then
            result(2) = "aocustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "aoid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aocabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aolokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aosumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aoautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aonotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aotgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aokodepa", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aosupplier", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aosupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ao1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ao1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ao1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ao2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ao2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ao2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aobagianpembelian", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aotgldipenuhi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aotermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aotgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aouraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aocatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aonoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aotglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aotglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aomatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aokurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aohargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aototal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aodiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aojmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aototalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aototalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aobiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aobiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aototaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aojmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aorekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aorekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aorekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aorekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aorekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aoidar", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aoidab", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aostatusae", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aostatusai", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aostatusrealisasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aostatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aostatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aojmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aocetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aoinputuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aoinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aomodifikasiuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aomodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aoposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aopostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aoisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aocustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aocustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aocustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aocustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aocustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aocustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aocustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aocustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aocustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aocustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aocustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aocustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aocustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aocustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aoidaq", AsEnumTypeData.AsDouble)
        If AsDataTableTambahData(dtutama, "aoid~aocabang~aolokasi~aosumber~aoautonotransaksi~aonotransaksi~aotgl~aokodepa~aosupplier~aosupplierkontak~ao1alamat1~ao1alamat2~ao1alamat3~ao2alamat1~ao2alamat2~ao2alamat3~aobagianpembelian~aotgldipenuhi~aotermin~aotgljatuhtempo~aouraian~aocatatan~aonoref~aotglnoref~aotglpenutupan~aomatauang~aokurs~aohargatermasukpajak~aototal~aodiskonpersen~aojmldiskon~aototalpajak1detail~aototalpajak2detail~aobiayalainpersen~aobiayalain~aototaltransaksi~aojmlbayar~aorekdiskon~aorekpajak1~aorekpajak2~aorekbiayalain~aorekbayar~aoidar~aoidab~aostatusae~aostatus~aostatussebelumnya~aojmlrevisi~aocetakanke~aoinputuser~aoinputtgl~aomodifikasiuser~aomodifikasitgl~aoposting~aopostingtgl~aoisclose~aocustomtext1~aocustomtext2~aocustomtext3~aocustomtext4~aocustomtext5~aocustomint1~aocustomint2~aocustomint3~aocustomdbl1~aocustomdbl2~aocustomdbl3~aocustomdate1~aocustomdate2~aocustomdate3~aoidaq", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idaodetail(0) As , idao(1) As , idasset(2) As , namaasset(3) As String, jml(4) As Double, 
        'matauang(5) As String, kurs(6) As Double, harga(7) As Double, diskon(8) As String, jmldiskon(9) As Double, 
        'pajak1(10) As String, jmlpajak1(11) As Double, pajak2(12) As String, jmlpajak2(13) As Double, cabang(14) As String, 
        'lokasi(15) As String, costcenter(16) As String, divisi(17) As String, subdivisi(18) As String, proyek(19) As String, 
        'catatan(20) As String, urutan(21) As Integer, idardetail(22) As , idaqdetail(23) As , idabdetail(24) As , 
        'jmlae(25) As Double, statusae(26) As Integer, isclose(27) As Integer, customtext1(28) As String, customtext2(29) As String, customtext3(30) As String, 
        'customdbl1(31) As Double, customdbl2(32) As Double, customdbl3(33) As Double, customdate1(34) As Date, customdate2(35) As Date, 
        'customdate3(36) As Date, satuan(37) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idaodetail, idao, idasset, namaasset, jml, matauang, kurs, 
        'harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, 
        'cabang, lokasi, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idardetail, idaqdetail, idabdetail, jmlae, statusae, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, satuan

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idaodetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idao", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idasset", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "namaasset", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "diskon", AsEnumTypeData.AsString)
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
        AsDataTableTambahField(dtdetail, "idaqdetail", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idabdetail", AsEnumTypeData.AsDouble)
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
        Dim ftExistOutstandingAR As String = "", ftOutstandingAR As String = "", updNilaiAR As String = "", updFilterAR As String = ""
        Dim ftExistOutstandingAQ As String = "", ftOutstandingAQ As String = "", updNilaiAQ As String = "", updFilterAQ As String = ""
        Dim updStokBooking As String = ""
        Dim idardetail As Integer = 0, idaqdetail As Integer = 0, jml As Double = 0

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
            'idaodetail(0) As 
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idaodetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idaodetail should not be more than 20 character." : GoTo selesai
            End If

            'idao(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idao can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idao should not be more than 20 character." : GoTo selesai
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

            'idaqdetail(23) As 
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - idaqdetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(23)) > 20 Then
                result(2) = "Row : " & i & " - idaqdetail should not be more than 20 character." : GoTo selesai
            End If

            'idabdetail(24) As 
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - idabdetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(24)) > 20 Then
                result(2) = "Row : " & i & " - idabdetail should not be more than 20 character." : GoTo selesai
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
            'customdate3(40) As Date
            If Len(dataRowDetail(37)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idaodetail~idao~idasset~namaasset~jml~matauang~kurs~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~costcenter~divisi~subdivisi~proyek~catatan~urutan~idardetail~idaqdetail~idabdetail~jmlae~statusae~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~satuan", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jml(8) As Double       , gudang(22) As String       , idprdetail(29) As Integer      , idaqdetail(31) As Integer
            jml = dataRowDetail(8) : idardetail = dataRowDetail(22) : idaqdetail = dataRowDetail(31)

            'VALIDASI OUTSTANDING -------------------------
            If idardetail <> 0 Then 'AR
                '1. CEK DATA EXIST ------------------------
                ftExistOutstandingAR = IIf(Len(ftExistOutstandingAR.ToString) = 0, "", ftExistOutstandingAR & " UNION ")
                ftExistOutstandingAR = String.Concat(ftExistOutstandingAR, "SELECT EXISTS(SELECT 1 FROM m7_ar_detail JOIN m4_ar ON idar = arid WHERE idardetail = '" & idardetail & "' AND (arstatus = 2 OR arstatus = 3 OR arstatus = 4 OR arstatus = 7) LIMIT 1) as rowExists, '" & idardetail & "' as idardetail")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jml", "idardetail=" & idardetail)
                ftOutstandingAR = IIf(Len(ftOutstandingAR.ToString) = 0, "", ftOutstandingAR & " OR ")
                ftOutstandingAR = String.Concat(ftOutstandingAR, " (ard.idardetail = " & idardetail & " AND " & Outstanding & " > (ard.jml - prd.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilaiAR = String.Concat("WHEN '" & idardetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiAR)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilterAR = IIf(Len(updFilterAR.ToString) = 0, "", updFilterAR & " OR ")
                updFilterAR = String.Concat(updFilterAR, "(idardetail = '" & idardetail & "')")
            End If

            If idaqdetail <> 0 Then 'aq
                '1. CEK DATA EXIST ------------------------
                ftExistOutstandingAQ = IIf(Len(ftExistOutstandingAQ.ToString) = 0, "", ftExistOutstandingAQ & " UNION ")
                ftExistOutstandingAQ = String.Concat(ftExistOutstandingAQ, "SELECT EXISTS(SELECT 1 FROM m7_aq_detail JOIN m7_aq ON idaq = aqid WHERE idaqdetail = '" & idaqdetail & "' AND (aqstatus = 2 OR aqstatus = 3 OR aqstatus = 4 OR aqstatus = 7) LIMIT 1) as rowExists, '" & idaqdetail & "' as idaqdetail")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jml", "idaqdetail=" & idaqdetail)
                ftOutstandingAQ = IIf(Len(ftOutstandingAQ.ToString) = 0, "", ftOutstandingAQ & " OR ")
                ftOutstandingAQ = String.Concat(ftOutstandingAQ, " (aqd.idaqdetail = " & idaqdetail & " AND " & Outstanding & " > (aqd.jml - aqd.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilaiAQ = String.Concat("WHEN '" & idaqdetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiAQ)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilterAQ = IIf(Len(updFilterAQ.ToString) = 0, "", updFilterAQ & " OR ")
                updFilterAQ = String.Concat(updFilterAQ, "(idaqdetail = '" & idaqdetail & "')")
            End If
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

            '5. SET NILAI UPDATE STOK BOOKING
            'updStokBooking = IIf(Len(updStokBooking.ToString) = 0, "", updStokBooking & ", ")
            'updStokBooking = String.Concat(updStokBooking, "('" & idbarang & "', '" & gudang & "', ('" & jml & "'))") ' idbarang, gudang, jmlbooking
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

                'SET TGL JATUH TEMPO ====================================
                Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                rsTglJT = F_TglJT(drutama("aotermin").ToString, AsFormatTanggal(drutama("aotgl")), "aotgl").Split(sptSubParam)
                If rsTglJT(0) = 0 Then
                    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                Else
                    drutama("aotgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                End If
                'END OF SET TGL JATUH TEMPO =============================

                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TOTAL = subtotal
                drutama("aototal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("aototalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("aototalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                drutama("aototaltransaksi") = Double.Parse(drutama("aototal")) - Double.Parse(drutama("aodiskonpersen")) + Double.Parse(drutama("aototalpajak1detail")) + Double.Parse(drutama("aototalpajak2detail")) + Double.Parse(drutama("aobiayalain"))
                'END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("aoid")
                    notransaksi = drutama("aonotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(aoid) FROM M7_Ao WHERE aoid=" & result(4))
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then
                        sql = "Update M7_Ao set aocabang  = '" & FixQuotes(drutama("aocabang")) & "', aolokasi  = '" & FixQuotes(drutama("aolokasi")) & "', aosumber  = '" & FixQuotes(drutama("aosumber")) & "', aoautonotransaksi  = " & drutama("aoautonotransaksi") & ", aonotransaksi  = '" & FixQuotes(drutama("aonotransaksi")) & "', aotgl  = '" & FixQuotes(AsFormatTanggal(drutama("aotgl"))) & "', aokodepa  = '" & FixQuotes(drutama("aokodepa")) & "', aosupplier  = '" & FixQuotes(drutama("aosupplier")) & "', aosupplierkontak  = '" & FixQuotes(drutama("aosupplierkontak")) & "', ao1alamat1  = '" & FixQuotes(drutama("ao1alamat1")) & "', ao1alamat2  = '" & FixQuotes(drutama("ao1alamat2")) & "', ao1alamat3  = '" & FixQuotes(drutama("ao1alamat3")) & "', ao2alamat1  = '" & FixQuotes(drutama("ao2alamat1")) & "', ao2alamat2  = '" & FixQuotes(drutama("ao2alamat2")) & "', ao2alamat3  = '" & FixQuotes(drutama("ao2alamat3")) & "', aobagianpembelian  = '" & FixQuotes(drutama("aobagianpembelian")) & "', aotgldipenuhi  = '" & FixQuotes(AsFormatTanggal(drutama("aotgldipenuhi"))) & "', aotermin  = '" & FixQuotes(drutama("aotermin")) & "', aotgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("aotgljatuhtempo"))) & "', aouraian  = '" & FixQuotes(drutama("aouraian")) & "', aocatatan  = '" & FixQuotes(drutama("aocatatan")) & "', aonoref  = '" & FixQuotes(drutama("aonoref")) & "', aotglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("aotglnoref"))) & "', aotglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("aotglpenutupan"))) & "', aomatauang  = '" & FixQuotes(drutama("aomatauang")) & "', aokurs  = '" & FixDouble(drutama("aokurs")) & "', aohargatermasukpajak  = " & drutama("aohargatermasukpajak") & ", aototal  = '" & FixDouble(drutama("aototal")) & "', aodiskonpersen  = '" & FixQuotes(drutama("aodiskonpersen")) & "', aojmldiskon  = '" & FixDouble(drutama("aojmldiskon")) & "', aototalpajak1detail  = '" & FixDouble(drutama("aototalpajak1detail")) & "', aototalpajak2detail  = '" & FixDouble(drutama("aototalpajak2detail")) & "', aobiayalainpersen  = '" & FixQuotes(drutama("aobiayalainpersen")) & "', aobiayalain  = '" & FixDouble(drutama("aobiayalain")) & "', aototaltransaksi  = '" & FixDouble(drutama("aototaltransaksi")) & "', aojmlbayar  = '" & FixDouble(drutama("aojmlbayar")) & "', aorekdiskon  = '" & FixQuotes(drutama("aorekdiskon")) & "', aorekpajak1  = '" & FixQuotes(drutama("aorekpajak1")) & "', aorekpajak2  = '" & FixQuotes(drutama("aorekpajak2")) & "', aorekbiayalain  = '" & FixQuotes(drutama("aorekbiayalain")) & "', aorekbayar  = '" & FixQuotes(drutama("aorekbayar")) & "', aoidar  = '" & FixQuotes(drutama("aoidar")) & "', aoidab  = '" & FixQuotes(drutama("aoidab")) & "', aostatusae  = " & drutama("aostatusae") & ", aostatus  = " & drutama("aostatus") & ", aostatussebelumnya  = " & drutama("aostatussebelumnya") & ", aojmlrevisi  = " & drutama("aojmlrevisi") & ", aocetakanke  = " & drutama("aocetakanke") & ", aomodifikasiuser  = '" & FixQuotes(drutama("aomodifikasiuser")) & "', aomodifikasitgl  = NOW(), aoposting  = " & drutama("aoposting") & ", aopostingtgl  = '" & FixQuotes(AsFormatTanggal(drutama("aopostingtgl"), "yyyy-MM-dd HH:mm:ss")) & "', aocustomtext1  = '" & FixQuotes(drutama("aocustomtext1")) & "', aocustomtext2  = '" & FixQuotes(drutama("aocustomtext2")) & "', aocustomtext3  = '" & FixQuotes(drutama("aocustomtext3")) & "', aocustomtext4  = '" & FixQuotes(drutama("aocustomtext4")) & "', aocustomtext5  = '" & FixQuotes(drutama("aocustomtext5")) & "', aocustomint1  = " & drutama("aocustomint1") & ", aocustomint2  = " & drutama("aocustomint2") & ", aocustomint3  = " & drutama("aocustomint3") & ", aocustomdbl1  = '" & FixDouble(drutama("aocustomdbl1")) & "', aocustomdbl2  = '" & FixDouble(drutama("aocustomdbl2")) & "', aocustomdbl3  = '" & FixDouble(drutama("aocustomdbl3")) & "', aocustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("aocustomdate1"))) & "', aocustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("aocustomdate2"))) & "', aocustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("aocustomdate3"))) & "', aoidaq  = '" & FixQuotes(drutama("aoidaq")) & "' where aoid = " & drutama("aoid") & ""
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

                    If drutama("aoautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("aocabang"), drutama("aolokasi"), drutama("aosumber"), drutama("aotgl"))
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
                        notransaksi = drutama("aonotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(aoid) FROM m7_ao WHERE aonotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M7_Ao (aocabang, aolokasi, aosumber, aoautonotransaksi, aonotransaksi, aotgl, aokodepa, aosupplier, aosupplierkontak, ao1alamat1, ao1alamat2, ao1alamat3, ao2alamat1, ao2alamat2, ao2alamat3, aobagianpembelian, aotgldipenuhi, aotermin, aotgljatuhtempo, aouraian, aocatatan, aonoref, aotglnoref, aotglpenutupan, aomatauang, aokurs, aohargatermasukpajak, aototal, aodiskonpersen, aojmldiskon, aototalpajak1detail, aototalpajak2detail, aobiayalainpersen, aobiayalain, aototaltransaksi, aojmlbayar, aorekdiskon, aorekpajak1, aorekpajak2, aorekbiayalain, aorekbayar, aoidar, aoidab, aostatusae, aostatus, aostatussebelumnya, aojmlrevisi, aocetakanke, aoinputuser, aoinputtgl, aomodifikasiuser, aomodifikasitgl, aoposting, aopostingtgl, aoisclose, aocustomtext1, aocustomtext2, aocustomtext3, aocustomtext4, aocustomtext5, aocustomint1, aocustomint2, aocustomint3, aocustomdbl1, aocustomdbl2, aocustomdbl3, aocustomdate1, aocustomdate2, aocustomdate3, aoidaq) values('" & FixQuotes(drutama("aocabang")) & "', '" & FixQuotes(drutama("aolokasi")) & "', '" & FixQuotes(drutama("aosumber")) & "', " & drutama("aoautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("aotgl"))) & "', '" & FixQuotes(drutama("aokodepa")) & "', '" & FixQuotes(drutama("aosupplier")) & "', '" & FixQuotes(drutama("aosupplierkontak")) & "', '" & FixQuotes(drutama("ao1alamat1")) & "', '" & FixQuotes(drutama("ao1alamat2")) & "', '" & FixQuotes(drutama("ao1alamat3")) & "', '" & FixQuotes(drutama("ao2alamat1")) & "', '" & FixQuotes(drutama("ao2alamat2")) & "', '" & FixQuotes(drutama("ao2alamat3")) & "', '" & FixQuotes(drutama("aobagianpembelian")) & "', '" & FixQuotes(AsFormatTanggal(drutama("aotgldipenuhi"))) & "', '" & FixQuotes(drutama("aotermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("aotgljatuhtempo"))) & "', '" & FixQuotes(drutama("aouraian")) & "', '" & FixQuotes(drutama("aocatatan")) & "', '" & FixQuotes(drutama("aonoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("aotglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("aotglpenutupan"))) & "', '" & FixQuotes(drutama("aomatauang")) & "', '" & FixDouble(drutama("aokurs")) & "', " & drutama("aohargatermasukpajak") & ", '" & FixDouble(drutama("aototal")) & "', '" & FixQuotes(drutama("aodiskonpersen")) & "', '" & FixDouble(drutama("aojmldiskon")) & "', '" & FixDouble(drutama("aototalpajak1detail")) & "', '" & FixDouble(drutama("aototalpajak2detail")) & "', '" & FixQuotes(drutama("aobiayalainpersen")) & "', '" & FixDouble(drutama("aobiayalain")) & "', '" & FixDouble(drutama("aototaltransaksi")) & "', '" & FixDouble(drutama("aojmlbayar")) & "', '" & FixQuotes(drutama("aorekdiskon")) & "', '" & FixQuotes(drutama("aorekpajak1")) & "', '" & FixQuotes(drutama("aorekpajak2")) & "', '" & FixQuotes(drutama("aorekbiayalain")) & "', '" & FixQuotes(drutama("aorekbayar")) & "', '" & FixQuotes(drutama("aoidar")) & "', '" & FixQuotes(drutama("aoidab")) & "', " & drutama("aostatusae") & ", " & drutama("aostatus") & ", " & drutama("aostatussebelumnya") & ", " & drutama("aojmlrevisi") & ", " & drutama("aocetakanke") & ", '" & FixQuotes(drutama("aoinputuser")) & "', NOW(), '" & FixQuotes(drutama("aomodifikasiuser")) & "', '1971-01-01', " & drutama("aoposting") & ", '" & FixQuotes(AsFormatTanggal(drutama("aopostingtgl"), "yyyy-MM-dd HH:mm:ss")) & "', " & drutama("aoisclose") & ", '" & FixQuotes(drutama("aocustomtext1")) & "', '" & FixQuotes(drutama("aocustomtext2")) & "', '" & FixQuotes(drutama("aocustomtext3")) & "', '" & FixQuotes(drutama("aocustomtext4")) & "', '" & FixQuotes(drutama("aocustomtext5")) & "', " & drutama("aocustomint1") & ", " & drutama("aocustomint2") & ", " & drutama("aocustomint3") & ", '" & FixDouble(drutama("aocustomdbl1")) & "', '" & FixDouble(drutama("aocustomdbl2")) & "', '" & FixDouble(drutama("aocustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("aocustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("aocustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("aocustomdate3"))) & "', '" & FixQuotes(drutama("aoidaq")) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select aoid from M7_ao where aonotransaksi='" & notransaksi & "' AND aoinputuser= '" & userid & "' order by aomodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                    result(4) = dt2.Rows(0)(0)
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M7_Ao_Detail where idao = " & result(4)
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
                        strValue2.Append("('" & FixQuotes(dr1("idaodetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("idasset")) & "', '" & FixQuotes(dr1("namaasset")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", '" & FixQuotes(dr1("idardetail")) & "', '" & FixQuotes(dr1("idaqdetail")) & "', '" & FixQuotes(dr1("idabdetail")) & "', '" & FixDouble(dr1("jmlae")) & "', " & dr1("statusae") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(dr1("satuan")) & "')")
                    Next
                    sql = "Insert into M7_Ao_Detail(idaodetail, idao, idasset, namaasset, jml, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, costcenter, divisi, subdivisi, proyek, catatan, urutan, idardetail, idaqdetail, idabdetail, jmlae, statusae, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, satuan) values" & strValue2.ToString & ""
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
                If drutama("aostatus") = 2 Then
                    If Len(updNilaiAR) > 0 Then 'AR
                        'UPDATE DETAIL
                        sql = "UPDATE m7_ar_detail SET jmlrealisasi = (CASE idardetail " & updNilaiAR & " ELSE jmlrealisasi END) WHERE " & updFilterAR
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idar FROM M7_ar_detail WHERE " & updFilterAR & " GROUP BY idar")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idar = '" & dr1("idar") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDB("SELECT idar, SUM(jml) as jml, SUM(jmlrealisasi) as jmlrealisasi FROM M7_ar_detail WHERE " & ftDetail & " GROUP BY idar")
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiAR = "" : updFilterAR = ""
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
                                updNilaiAR = String.Concat(updNilaiAR, "WHEN '" & dr1("idar") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterAR = IIf(Len(updFilterAR.ToString) = 0, "", updFilterAR & " OR ")
                                updFilterAR = String.Concat(updFilterAR, "(arid = '" & dr1("idar") & "')")
                            Next

                            sql = "UPDATE m7_ar SET arstatusrealisasi = (CASE arid " & updNilaiAR & " ELSE arstatusrealisasi END) WHERE " & updFilterAR
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

                    If Len(updNilaiAQ) > 0 Then 'AQ
                        result(2) = "gagal update AQ"
                        'UPDATE DETAIL
                        sql = "UPDATE m7_aq_detail SET jmlrealisasi = (CASE idaqdetail " & updNilaiAQ & " ELSE jmlrealisasi END) WHERE " & updFilterAQ
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idaq FROM m7_aq_detail WHERE " & updFilterAQ & " GROUP BY idaq")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idaq = '" & dr1("idaq") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDB("SELECT idaq, SUM(jml) as jml, SUM(jmlrealisasi) as jmlrealisasi FROM m7_aq_detail WHERE " & ftDetail & " GROUP BY idaq")
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiAQ = "" : updFilterAQ = ""
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
                                updNilaiAQ = String.Concat(updNilaiAQ, "WHEN '" & dr1("idaq") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterAQ = IIf(Len(updFilterAQ.ToString) = 0, "", updFilterAQ & " OR ")
                                updFilterAQ = String.Concat(updFilterAQ, "(aqid = '" & dr1("idaq") & "')")
                            Next

                            sql = "UPDATE m7_aq SET aqstatusrealisasi = (CASE aqid " & updNilaiAQ & " ELSE aqstatusrealisasi END) WHERE " & updFilterAQ
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
                'END OF UPDATE OUTSTANDING TRANSAKSI ================================================

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "AO", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M7_AoGetdataById(ByVal param As String) As String
        'M7_AoGetdataById Utama --------------------------------------------------------
        'aoid, aocabang, aolokasi, aosumber, aoautonotransaksi, aonotransaksi, aotgl, 
        'aokodepa, aosupplier, aosupplierkontak, ao1alamat1, ao1alamat2, ao1alamat3, ao2alamat1, 
        'ao2alamat2, ao2alamat3, aobagianpembelian, aotgldipenuhi, aotermin, aotgljatuhtempo, aouraian, 
        'aocatatan, aonoref, aotglnoref, aotglpenutupan, aomatauang, aokurs, aohargatermasukpajak, 
        'aototal, aodiskonpersen, aojmldiskon, aototalpajak1detail, aototalpajak2detail, aobiayalainpersen, aobiayalain, 
        'aototaltransaksi, aojmlbayar, aorekdiskon, aorekpajak1, aorekpajak2, aorekbiayalain, aorekbayar, 
        'aoidar, aoidaq, aoidab, aostatusae, aostatusrealisasi, aostatus, aostatussebelumnya, 
        'aojmlrevisi, aocetakanke, aoinputuser, aoinputtgl, aomodifikasiuser, aomodifikasitgl, aoposting, 
        'aopostingtgl, aoisclose, aocustomtext1, aocustomtext2, aocustomtext3, aocustomtext4, aocustomtext5, 
        'aocustomint1, aocustomint2, aocustomint3, aocustomdbl1, aocustomdbl2, aocustomdbl3, aocustomdate1, 
        'aocustomdate2, aocustomdate3, aocabangnama, aolokasinama, aosupplierkode, aosuppliernama, aobagianpembeliankode, 
        'aobagianpembeliannama, aoterminnama, aoterminharijatuhtempo, aorekdiskonnama, aorekpajak1nama, aorekpajak2nama, aorekbiayalainnama, 
        'aorekbayarnama, aonotransaksiar, aonotransaksiaq, aonotransaksiab, aostatusnama, aostatussebelumnyanama, aoinputusernama, 
        'aomodifikasiusernama

        'M7_AoGetdataById Detail -------------------------------------------------------
        'idaodetail, idao, idasset, namaasset, jml, matauang, 
        'kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, 
        'jmlpajak2, cabang, lokasi, costcenter, divisi, subdivisi, proyek, 
        'catatan, urutan, idardetail, idaqdetail, idabdetail, jmlae, statusae, 
        'jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3, pajak1nama, pajak1nilai, 
        'pajak2nama, pajak2nilai, cabangnama, lokasinama, costcenternama, divisinama, subdivisinama, 
        'proyeknama, arnotransaksi, aqnotransaksi, abnotransaksi, satuan

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
            Filter = "aoid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "aoid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m7_ao_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                 FxDB(drutama("aoid"), ""), sptField,
                     FxDB(drutama("aocabang"), ""), sptField,
                     FxDB(drutama("aolokasi"), ""), sptField,
                     FxDB(drutama("aosumber"), ""), sptField,
                     FxDB(drutama("aoautonotransaksi"), 0), sptField,
                     FxDB(drutama("aonotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aotgl"), ""), formatTgl), sptField,
                     FxDB(drutama("aokodepa"), ""), sptField,
                     FxDB(drutama("aosupplier"), ""), sptField,
                     FxDB(drutama("aosupplierkontak"), ""), sptField,
                     FxDB(drutama("ao1alamat1"), ""), sptField,
                     FxDB(drutama("ao1alamat2"), ""), sptField,
                     FxDB(drutama("ao1alamat3"), ""), sptField,
                     FxDB(drutama("ao2alamat1"), ""), sptField,
                     FxDB(drutama("ao2alamat2"), ""), sptField,
                     FxDB(drutama("ao2alamat3"), ""), sptField,
                     FxDB(drutama("aobagianpembelian"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aotgldipenuhi"), ""), formatTgl), sptField,
                     FxDB(drutama("aotermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aotgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("aouraian"), ""), sptField,
                     FxDB(drutama("aocatatan"), ""), sptField,
                     FxDB(drutama("aonoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aotglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("aotglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("aomatauang"), ""), sptField,
                     FxDB(drutama("aokurs"), 0), sptField,
                     FxDB(drutama("aohargatermasukpajak"), 0), sptField,
                     FxDB(drutama("aototal"), 0), sptField,
                     FxDB(drutama("aodiskonpersen"), ""), sptField,
                     FxDB(drutama("aojmldiskon"), 0), sptField,
                     FxDB(drutama("aototalpajak1detail"), 0), sptField,
                     FxDB(drutama("aototalpajak2detail"), 0), sptField,
                     FxDB(drutama("aobiayalainpersen"), ""), sptField,
                     FxDB(drutama("aobiayalain"), 0), sptField,
                     FxDB(drutama("aototaltransaksi"), 0), sptField,
                     FxDB(drutama("aojmlbayar"), 0), sptField,
                     FxDB(drutama("aorekdiskon"), ""), sptField,
                     FxDB(drutama("aorekpajak1"), ""), sptField,
                     FxDB(drutama("aorekpajak2"), ""), sptField,
                     FxDB(drutama("aorekbiayalain"), ""), sptField,
                     FxDB(drutama("aorekbayar"), ""), sptField,
                     FxDB(drutama("aoidar"), ""), sptField,
                     FxDB(drutama("aoidaq"), ""), sptField,
                     FxDB(drutama("aoidab"), ""), sptField,
                     FxDB(drutama("aostatusae"), 0), sptField,
                     FxDB(drutama("aostatusrealisasi"), 0), sptField,
                     FxDB(drutama("aostatus"), 0), sptField,
                     FxDB(drutama("aostatussebelumnya"), 0), sptField,
                     FxDB(drutama("aojmlrevisi"), 0), sptField,
                     FxDB(drutama("aocetakanke"), 0), sptField,
                     FxDB(drutama("aoinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aoinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("aomodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aomodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("aoposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("aopostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("aoisclose"), 0), sptField,
                     FxDB(drutama("aocustomtext1"), ""), sptField,
                     FxDB(drutama("aocustomtext2"), ""), sptField,
                     FxDB(drutama("aocustomtext3"), ""), sptField,
                     FxDB(drutama("aocustomtext4"), ""), sptField,
                     FxDB(drutama("aocustomtext5"), ""), sptField,
                     FxDB(drutama("aocustomint1"), 0), sptField,
                     FxDB(drutama("aocustomint2"), 0), sptField,
                     FxDB(drutama("aocustomint3"), 0), sptField,
                     FxDB(drutama("aocustomdbl1"), 0), sptField,
                     FxDB(drutama("aocustomdbl2"), 0), sptField,
                     FxDB(drutama("aocustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("aocustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("aocustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("aocustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("aocabangnama"), ""), sptField,
                     FxDB(drutama("aolokasinama"), ""), sptField,
                     FxDB(drutama("aosupplierkode"), ""), sptField,
                     FxDB(drutama("aosuppliernama"), ""), sptField,
                     FxDB(drutama("aobagianpembeliankode"), ""), sptField,
                     FxDB(drutama("aobagianpembeliannama"), ""), sptField,
                     FxDB(drutama("aoterminnama"), ""), sptField,
                     FxDB(drutama("aoterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("aorekdiskonnama"), ""), sptField,
                     FxDB(drutama("aorekpajak1nama"), ""), sptField,
                     FxDB(drutama("aorekpajak2nama"), ""), sptField,
                     FxDB(drutama("aorekbiayalainnama"), ""), sptField,
                     FxDB(drutama("aorekbayarnama"), ""), sptField,
                     FxDB(drutama("aonotransaksiar"), ""), sptField,
                     FxDB(drutama("aonotransaksiaq"), ""), sptField,
                     FxDB(drutama("aonotransaksiab"), ""), sptField,
                     FxDB(drutama("aostatusnama"), ""), sptField,
                     FxDB(drutama("aostatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("aoinputusernama"), ""), sptField,
                     FxDB(drutama("aomodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idaodetail"), ""), sptField,
                     FxDB(dr("idao"), ""), sptField,
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
                     FxDB(dr("idaqdetail"), ""), sptField,
                     FxDB(dr("idabdetail"), ""), sptField,
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
                     FxDB(dr("aqnotransaksi"), ""), sptField,
                     FxDB(dr("abnotransaksi"), ""), sptField,
                     FxDB(dr("satuan"), ""), sptRow)

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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("aoid, aocabang, aolokasi, aosumber, aoautonotransaksi, aonotransaksi, aotgl, aokodepa, aosupplier, aosupplierkontak, ao1alamat1, ao1alamat2, ao1alamat3, ao2alamat1, ao2alamat2, ao2alamat3, aobagianpembelian, aotgldipenuhi, aotermin, aotgljatuhtempo, aouraian, aocatatan, aonoref, aotglnoref, aotglpenutupan, aomatauang, aokurs, aohargatermasukpajak, aototal, aodiskonpersen, aojmldiskon, aototalpajak1detail, aototalpajak2detail, aobiayalainpersen, aobiayalain, aototaltransaksi, aojmlbayar, aorekdiskon, aorekpajak1, aorekpajak2, aorekbiayalain, aorekbayar, aoidar, aoidaq, aoidab, aostatusae, aostatusrealisasi, aostatus, aostatussebelumnya, aojmlrevisi, aocetakanke, aoinputuser, aoinputtgl, aomodifikasiuser, aomodifikasitgl, aoposting, aopostingtgl, aoisclose, aocustomtext1, aocustomtext2, aocustomtext3, aocustomtext4, aocustomtext5, aocustomint1, aocustomint2, aocustomint3, aocustomdbl1, aocustomdbl2, aocustomdbl3, aocustomdate1, aocustomdate2, aocustomdate3, aocabangnama, aolokasinama, aosupplierkode, aosuppliernama, aobagianpembeliankode, aobagianpembeliannama, aoterminnama, aoterminharijatuhtempo, aorekdiskonnama, aorekpajak1nama, aorekpajak2nama, aorekbiayalainnama, aorekbayarnama, aonotransaksiar, aonotransaksiaq, aonotransaksiab, aostatusnama, aostatussebelumnyanama, aoinputusernama, aomodifikasiusernama" & sptSubParam & "idaodetail, idao, idasset, namaasset, jml, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, costcenter, divisi, subdivisi, proyek, catatan, urutan, idardetail, idaqdetail, idabdetail, jmlae, statusae, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, costcenternama, divisinama, subdivisinama, proyeknama, arnotransaksi, aqnotransaksi, abnotransaksi, satuan"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M7_AoSearch(ByVal param As String) As String
        'M7_AoSearch --------------------------------------------------------
        'aoid, aocabang, aolokasi, aosumber, aoautonotransaksi, aonotransaksi, aotgl, 
        'aokodepa, aosupplier, aosupplierkontak, ao1alamat1, ao1alamat2, ao1alamat3, ao2alamat1, 
        'ao2alamat2, ao2alamat3, aobagianpembelian, aotgldipenuhi, aotermin, aotgljatuhtempo, aouraian, 
        'aocatatan, aonoref, aotglnoref, aotglpenutupan, aomatauang, aokurs, aohargatermasukpajak, 
        'aototal, aodiskonpersen, aojmldiskon, aototalpajak1detail, aototalpajak2detail, aobiayalainpersen, aobiayalain, 
        'aototaltransaksi, aojmlbayar, aorekdiskon, aorekpajak1, aorekpajak2, aorekbiayalain, aorekbayar, 
        'aoidpr, aoidaq, aoidab, aostatusae, aostatusrealisasi, aostatus, aostatussebelumnya, 
        'aojmlrevisi, aocetakanke, aoinputuser, aoinputtgl, aomodifikasiuser, aomodifikasitgl, aoposting, 
        'aopostingtgl, aoisclose, aocabangnama, aolokasinama, aosupplierkode, aosuppliernama, aobagianpembeliankode, 
        'aobagianpembeliannama, arnotransaksi, aqnotransaksi, abnotransaksi, aostatusnama, aostatussebelumnyanama, aoinputusernama, 
        'aomodifikasiusernama

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
            Filter = Filter.Replace("aosupplierkode", "c1.kkode")
            Filter = Filter.Replace("aosuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        Dim query As New m0_query
        sql = query.PanggilQuery("m7_ao_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_aq", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("aoid"), ""), sptField,
                     FxDB(dr("aocabang"), ""), sptField,
                     FxDB(dr("aolokasi"), ""), sptField,
                     FxDB(dr("aosumber"), ""), sptField,
                     FxDB(dr("aoautonotransaksi"), 0), sptField,
                     FxDB(dr("aonotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aotgl"), ""), formatTgl), sptField,
                     FxDB(dr("aokodepa"), ""), sptField,
                     FxDB(dr("aosupplier"), ""), sptField,
                     FxDB(dr("aosupplierkontak"), ""), sptField,
                     FxDB(dr("ao1alamat1"), ""), sptField,
                     FxDB(dr("ao1alamat2"), ""), sptField,
                     FxDB(dr("ao1alamat3"), ""), sptField,
                     FxDB(dr("ao2alamat1"), ""), sptField,
                     FxDB(dr("ao2alamat2"), ""), sptField,
                     FxDB(dr("ao2alamat3"), ""), sptField,
                     FxDB(dr("aobagianpembelian"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aotgldipenuhi"), ""), formatTgl), sptField,
                     FxDB(dr("aotermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aotgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("aouraian"), ""), sptField,
                     FxDB(dr("aocatatan"), ""), sptField,
                     FxDB(dr("aonoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aotglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("aotglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("aomatauang"), ""), sptField,
                     FxDB(dr("aokurs"), 0), sptField,
                     FxDB(dr("aohargatermasukpajak"), 0), sptField,
                     FxDB(dr("aototal"), 0), sptField,
                     FxDB(dr("aodiskonpersen"), ""), sptField,
                     FxDB(dr("aojmldiskon"), 0), sptField,
                     FxDB(dr("aototalpajak1detail"), 0), sptField,
                     FxDB(dr("aototalpajak2detail"), 0), sptField,
                     FxDB(dr("aobiayalainpersen"), ""), sptField,
                     FxDB(dr("aobiayalain"), 0), sptField,
                     FxDB(dr("aototaltransaksi"), 0), sptField,
                     FxDB(dr("aojmlbayar"), 0), sptField,
                     FxDB(dr("aorekdiskon"), ""), sptField,
                     FxDB(dr("aorekpajak1"), ""), sptField,
                     FxDB(dr("aorekpajak2"), ""), sptField,
                     FxDB(dr("aorekbiayalain"), ""), sptField,
                     FxDB(dr("aorekbayar"), ""), sptField,
                     FxDB(dr("aoidpr"), ""), sptField,
                     FxDB(dr("aoidaq"), ""), sptField,
                     FxDB(dr("aoidab"), ""), sptField,
                     FxDB(dr("aostatusae"), 0), sptField,
                     FxDB(dr("aostatusrealisasi"), 0), sptField,
                     FxDB(dr("aostatus"), 0), sptField,
                     FxDB(dr("aostatussebelumnya"), 0), sptField,
                     FxDB(dr("aojmlrevisi"), 0), sptField,
                     FxDB(dr("aocetakanke"), 0), sptField,
                     FxDB(dr("aoinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aoinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("aomodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aomodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("aoposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("aopostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("aoisclose"), 0), sptField,
                     FxDB(dr("aocabangnama"), ""), sptField,
                     FxDB(dr("aolokasinama"), ""), sptField,
                     FxDB(dr("aosupplierkode"), ""), sptField,
                     FxDB(dr("aosuppliernama"), ""), sptField,
                     FxDB(dr("aobagianpembeliankode"), ""), sptField,
                     FxDB(dr("aobagianpembeliannama"), ""), sptField,
                     FxDB(dr("arnotransaksi"), ""), sptField,
                     FxDB(dr("aqnotransaksi"), ""), sptField,
                     FxDB(dr("abnotransaksi"), ""), sptField,
                     FxDB(dr("aostatusnama"), ""), sptField,
                     FxDB(dr("aostatussebelumnyanama"), ""), sptField,
                     FxDB(dr("aoinputusernama"), ""), sptField,
                     FxDB(dr("aomodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("aoid, aocabang, aolokasi, aosumber, aoautonotransaksi, aonotransaksi, aotgl, aokodepa, aosupplier, aosupplierkontak, ao1alamat1, ao1alamat2, ao1alamat3, ao2alamat1, ao2alamat2, ao2alamat3, aobagianpembelian, aotgldipenuhi, aotermin, aotgljatuhtempo, aouraian, aocatatan, aonoref, aotglnoref, aotglpenutupan, aomatauang, aokurs, aohargatermasukpajak, aototal, aodiskonpersen, aojmldiskon, aototalpajak1detail, aototalpajak2detail, aobiayalainpersen, aobiayalain, aototaltransaksi, aojmlbayar, aorekdiskon, aorekpajak1, aorekpajak2, aorekbiayalain, aorekbayar, aoidpr, aoidaq, aoidab, aostatusae, aostatusrealisasi,aostatus, aostatussebelumnya, aojmlrevisi, aocetakanke, aoinputuser, aoinputtgl, aomodifikasiuser, aomodifikasitgl, aoposting, aopostingtgl,aoisclose,aocabangnama,aolokasinama, aosupplierkode, aosuppliernama, aobagianpembeliankode, aobagianpembeliannama, arnotransaksi, aqnotransaksi, abnotransaksi, aostatusnama, aostatussebelumnyanama, aoinputusernama, aomodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M7_Ao_Detail_VSearch(ByVal param As String) As String
        'M7_Ao_Detail_VSearch --------------------------------------------------------
        'idaodetail, idao, idasset, namaasset, jml, matauang, kurs, hargafix, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idardetail, idaqdetail, idabdetail 
        'jmlae, statusae, statusrealisasi, jmlrealisasi, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, aonotransaksi, 
        'aouraian, aocatatan, aonoref, aotgl, aotglnoref, aosupplierkontak, ao1alamat1, ao1alamat2, 
        'ao1alamat3, ao2alamat1, ao2alamat2, ao2alamat3, aotermin, aoterminnama, aoterminharijatuhtempo, 
        'aobagianpembelian, aobagianpembeliankode, aobagianpembeliannama, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, 
        'jmlsisaae, jmlsisaae, jmlsisarealisasi, aosupplier, aosupplierkode, aosuppliernama, satuan

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
            Filter = Filter.Replace("idbarang", "pod.idbarang")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m7_ao_detail_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M5_Sq_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idaodetail"), 0), sptField,
                     FxDB(dr("idao"), 0), sptField,
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
                     FxDB(dr("idaqdetail"), 0), sptField,
                     FxDB(dr("idabdetail"), 0), sptField,
                     FxDB(dr("jmlae"), 0), sptField,
                     FxDB(dr("statusae"), 0), sptField,
                     FxDB(dr("statusrealisasi"), 0), sptField,
                     FxDB(dr("jmlrealisasi"), 0), sptField,
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
                     FxDB(dr("aonotransaksi"), ""), sptField,
                     FxDB(dr("aouraian"), ""), sptField,
                     FxDB(dr("aocatatan"), ""), sptField,
                     FxDB(dr("aonoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aotgl"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("aotglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("aosupplierkontak"), ""), sptField,
                     FxDB(dr("ao1alamat1"), ""), sptField,
                     FxDB(dr("ao1alamat2"), ""), sptField,
                     FxDB(dr("ao1alamat3"), ""), sptField,
                     FxDB(dr("ao2alamat1"), ""), sptField,
                     FxDB(dr("ao2alamat2"), ""), sptField,
                     FxDB(dr("ao2alamat3"), ""), sptField,
                     FxDB(dr("aotermin"), ""), sptField,
                     FxDB(dr("aoterminnama"), ""), sptField,
                     FxDB(dr("aoterminharijatuhtempo"), 0), sptField,
                     FxDB(dr("aobagianpembelian"), 0), sptField,
                     FxDB(dr("aobagianpembeliankode"), ""), sptField,
                     FxDB(dr("aobagianpembeliannama"), ""), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("jmlsisaae"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
                     FxDB(dr("aosupplier"), ""), sptField,
                     FxDB(dr("aosupplierkode"), ""), sptField,
                     FxDB(dr("aosuppliernama"), ""), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idaodetail, idao, idasset, namaasset, jml, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, costcenter, divisi, subdivisi, proyek, catatan, urutan, idardetail, idaqdetail, idabdetail, jmlae, statusae, statusrealisasi, jmlrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, aonotransaksi, aouraian, aocatatan, aonoref, aotgl, aotglnoref, aosupplierkontak, ao1alamat1, ao1alamat2, ao1alamat3, ao2alamat1, ao2alamat2, ao2alamat3, aotermin, aoterminnama, aoterminharijatuhtempo, aobagianpembelian, aobagianpembeliankode, aobagianpembeliannama, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisaae, jmlsisarealisasi, aosupplier, aosupplierkode, aosuppliernama, satuan"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M7_AoTerkait(ByVal param As String) As String
        'M7_AoTerkait --------------------------------------------------------
        'aoid, aonotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "aoid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m7_ao_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("aoid"), 0), sptField,
                     FxDB(dr("aonotransaksi"), ""), sptField,
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
            result(2) = "Related AO data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("aoid, aonotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function


    <WebMethod()>
    Public Function M7_AoUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("aosupplierkode", "c1.kkode")
            Filter = Filter.Replace("aosuppliernama", "c1.knama")
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
            Dim sumber As String = "Po", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Aotgl, Aonotransaksi, Aostatus FROM M7_Ao WHERE Aoid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Aostatussebelumnya" : jnsaktivitas = 17
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
            'Dim SimpanHistory As New m4_po_history
            'Dim rsSimpanHistory As String = SimpanHistory.M4_Po_HistorySimpan("" & paramSplit(0) & "★M4_Po_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m7_ao_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                Dim idasset As Integer = 0, jml As Double = 0, idardetail As Integer = 0, idaqdetail As Integer = 0
                Dim updNilaiAR As String = "", updFilterAR As String = "", updNilaiAQ As String = "", updFilterAQ As String = ""
                Dim gudang As String = "", updStokBooking As String = ""

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDB("SELECT idasset, namaasset, satuan, jml, idardetail, idaqdetail, urutan FROM m7_ao_detail WHERE idao = '" & idtransaksi & "'")
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idasset = dr1("idasset") : jml = dr1("jml") : idardetail = dr1("idardetail") : idaqdetail = dr1("idaqdetail")

                        'UPDATE OUTSTANDING ---------------------------
                        If idardetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING PR
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jml", "idardetail=" & idardetail)
                            updNilaiAR = String.Concat("WHEN '" & idardetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiAR)
                            '2. SET FILTERUPDATE OUTSTANDING PR
                            updFilterAR = IIf(Len(updFilterAR.ToString) = 0, "", updFilterAR & " OR ")
                            updFilterAR = String.Concat(updFilterAR, "(idardetail = '" & idardetail & "')")
                        End If

                        If idaqdetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING RQ
                            Dim OutstandingAQ As Double = AsDataTableDSum(dtdetail, "jml", "idaqdetail=" & idaqdetail)
                            updNilaiAQ = String.Concat("WHEN '" & idaqdetail & "' THEN ROUND(jmlrealisasi - '" & OutstandingAQ & "', 5) ", updNilaiAQ)
                            '2. SET FILTERUPDATE OUTSTANDING RQ
                            updFilterAQ = IIf(Len(updFilterAQ.ToString) = 0, "", updFilterAQ & " OR ")
                            updFilterAQ = String.Concat(updFilterAQ, "(idaqdetail = '" & idaqdetail & "')")
                        End If
                        'END OF BUAT FILTER UNTUK UPDATE --------------------------

                        '3. SET NILAI UPDATE STOK BOOKING KELUAR -------------
                        updStokBooking = IIf(Len(updStokBooking.ToString) = 0, "", updStokBooking & ", ")
                        updStokBooking = String.Concat(updStokBooking, "('" & idasset & "', ('-" & jml & "'))") ' idbarang, kgudang, stok

                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                'UPDATE OUTSTANDING TRANSAKSI ====================================================
                If Len(updFilterAR) > 0 Then 'PR
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m7_ar_detail SET jmlrealisasi = (CASE idardetail " & updNilaiAR & " ELSE jmlrealisasi END) WHERE " & updFilterAR
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE OUTSTANDING UTAMA -----------------------
                    Dim ftDetail As String = "", statusOut As Integer = 0
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idar FROM M7_ar_detail WHERE " & updFilterAR & " GROUP BY idar")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idar = '" & dr1("idar") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDB("SELECT idar, SUM(jml) as jml, SUM(jmlrealisasi) as jmlrealisasi FROM M7_ar_detail WHERE " & ftDetail & " GROUP BY idar")
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiAR = "" : updFilterAR = ""
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
                            updNilaiAR = String.Concat(updNilaiAR, "WHEN '" & dr1("idar") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterAR = IIf(Len(updFilterAR.ToString) = 0, "", updFilterAR & " OR ")
                            updFilterAR = String.Concat(updFilterAR, "(arid = '" & dr1("idar") & "')")
                        Next

                        sql = "UPDATE m7_ar SET arstatusrealisasi = (CASE arid " & updNilaiAR & " ELSE arstatusrealisasi END) WHERE " & updFilterAR
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

                If Len(updFilterAQ) > 0 Then 'RQ
                    'UPDATE OUTSTANDING DETAIL -------------------
                    sql = "UPDATE m7_aq_detail SET jmlrealisasi = (CASE idaqdetail " & updNilaiAQ & " ELSE jmlrealisasi END) WHERE " & updFilterAQ
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idaq FROM m7_aq_detail WHERE " & updFilterAQ & " GROUP BY idaq")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idaq = '" & dr1("idaq") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDB("SELECT idaq, SUM(jml) as jml, SUM(jmlrealisasi) as jmlrealisasi FROM m7_sq_detail WHERE " & ftDetail & " GROUP BY idaq")
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiAQ = "" : updFilterAQ = ""
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
                            updNilaiAQ = String.Concat(updNilaiAQ, "WHEN '" & dr1("idaq") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterAQ = IIf(Len(updFilterAQ.ToString) = 0, "", updFilterAQ & " OR ")
                            updFilterAQ = String.Concat(updFilterAQ, "(aqid = '" & dr1("idaq") & "')")
                        Next

                        sql = "UPDATE m7_aq SET aqstatusrealisasi = (CASE aqid " & updNilaiAQ & " ELSE aqstatusrealisasi END) WHERE " & updFilterAQ
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
                'END OF UPDATE OUTSTANDING TRANSAKSI =============================================

            End If

            'update status utama
            sql = "UPDATE M7_Ao SET Aostatus = " & nilaiStatus & ", Aomodifikasiuser='" & userid & "', Aomodifikasitgl = NOW(), Aoposting = 0, Aopostingtgl = '1971-01-01 00:00:00', Aojmlrevisi = Aojmlrevisi + 1 WHERE Aoid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M7_AoSearch(PostWsSearch(paramSplit(0), "M7_AoSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M7_AoDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("aosupplierkode", "c1.kkode")
            Filter = Filter.Replace("aosuppliernama", "c1.knama")
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
            Dim sumber As String = "Ao", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Aoid, Aonotransaksi FROM M7_Ao WHERE Aoid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT aocabang, aolokasi, aosumber, aoautonotransaksi, aonotransaksi, aotgl"
            sql &= " FROM M7_ao"
            sql &= " WHERE aoid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("aocabang")
                lokasi = dtNomorNext.Rows(0)("aolokasi")
                sumber = dtNomorNext.Rows(0)("aosumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("aoautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("aonotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("aotgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M7_Ao_Detail WHERE idao = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M7_Ao WHERE aoid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M7_AoSearch(PostWsSearch(paramSplit(0), "M7_AoSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
