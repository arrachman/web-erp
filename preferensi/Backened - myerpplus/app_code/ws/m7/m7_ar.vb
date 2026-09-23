Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m7_ar
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M7_ArSimpan(ByVal param As String) As String
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
        'arid(0) As , arcabang(1) As String, arlokasi(2) As String, arsumber(3) As String, arautonotransaksi(4) As Integer, 
        'arnotransaksi(5) As String, artgl(6) As Date, arkodepa(7) As , ardimintaoleh(8) As , ardimintaolehkontak(9) As String, 
        'armintake(10) As , artgldipakai(11) As Date, artermin(12) As String, artgljatuhtempo(13) As Date, aruraian(14) As String, 
        'arcatatan(15) As String, arnoref(16) As String, artglnoref(17) As Date, artglpenutupan(18) As Date, armatauang(19) As String, 
        'arkurs(20) As Double, arhargatermasukpajak(21) As Integer, artotal(22) As Double, ardiskonpersen(23) As String, arjmldiskon(24) As Double, 
        'artotalpajak1detail(25) As Double, artotalpajak2detail(26) As Double, arbiayalainpersen(27) As String, arbiayalain(28) As Double, artotaltransaksi(29) As Double, 
        'arstatusaq(30) As Integer, arstatusao(31) As Integer, arstatusae(32) As Integer, 
        'arstatus(33) As Integer, arstatussebelumnya(34) As Integer, arjmlrevisi(35) As Integer, arcetakanke(36) As Integer, arinputuser(37) As , 
        'arinputtgl(38) As DateTime, armodifikasiuser(39) As , armodifikasitgl(40) As DateTime, arposting(41) As Integer, arpostingtgl(42) As DateTime, 
        'arisclose(43) As Integer, arcustomtext1(44) As String, arcustomtext2(45) As String, arcustomtext3(46) As String, arcustomtext4(47) As String, 
        'arcustomtext5(48) As String, arcustomint1(49) As Integer, arcustomint2(50) As Integer, arcustomint3(51) As Integer, arcustomdbl1(52) As Double, 
        'arcustomdbl2(53) As Double, arcustomdbl3(54) As Double, arcustomdate1(55) As Date, arcustomdate2(56) As Date, arcustomdate3(57) As Date


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'arid, arcabang, arlokasi, arsumber, arautonotransaksi, arnotransaksi, artgl, 
        'arkodepa, ardimintaoleh, ardimintaolehkontak, armintake, artgldipakai, artermin, artgljatuhtempo, 
        'aruraian, arcatatan, arnoref, artglnoref, artglpenutupan, armatauang, arkurs, 
        'arhargatermasukpajak, artotal, ardiskonpersen, arjmldiskon, artotalpajak1detail, artotalpajak2detail, arbiayalainpersen, 
        'arbiayalain, artotaltransaksi, arstatusaq, arstatusao, arstatusae, arstatusai, arstatusrealisasi, 
        'arstatus, arstatussebelumnya, arjmlrevisi, arcetakanke, arinputuser, arinputtgl, armodifikasiuser, 
        'armodifikasitgl, arposting, arpostingtgl, arisclose, arcustomtext1, arcustomtext2, arcustomtext3, 
        'arcustomtext4, arcustomtext5, arcustomint1, arcustomint2, arcustomint3, arcustomdbl1, arcustomdbl2, 
        'arcustomdbl3, arcustomdate1, arcustomdate2, arcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 58) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'arautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "arautonotransaksi required numeric." : GoTo selesai
        End If
        'artgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "artgl required date." : GoTo selesai
        End If
        'artgldipakai(11) As Date
        If (IsDate(dataUtama(11)) = False) Then
            result(2) = "artgldipakai required date." : GoTo selesai
        End If
        'artgljatuhtempo(13) As Date
        If (IsDate(dataUtama(13)) = False) Then
            result(2) = "artgljatuhtempo required date." : GoTo selesai
        End If
        'artglnoref(17) As Date
        If (IsDate(dataUtama(17)) = False) Then
            result(2) = "artglnoref required date." : GoTo selesai
        End If
        'artglpenutupan(18) As Date
        If (IsDate(dataUtama(18)) = False) Then
            result(2) = "artglpenutupan required date." : GoTo selesai
        End If
        'arkurs(20) As Double
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "arkurs required numeric." : GoTo selesai
        End If
        'arhargatermasukpajak(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "arhargatermasukpajak required numeric." : GoTo selesai
        End If
        'artotal(22) As Double
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "artotal required numeric." : GoTo selesai
        End If
        'arjmldiskon(24) As Double
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "arjmldiskon required numeric." : GoTo selesai
        End If
        'artotalpajak1detail(25) As Double
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "artotalpajak1detail required numeric." : GoTo selesai
        End If
        'artotalpajak2detail(26) As Double
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "artotalpajak2detail required numeric." : GoTo selesai
        End If
        'arbiayalain(28) As Double
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "arbiayalain required numeric." : GoTo selesai
        End If
        'artotaltransaksi(29) As Double
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "artotaltransaksi required numeric." : GoTo selesai
        End If
        'arstatusaq(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "arstatusaq required numeric." : GoTo selesai
        End If
        'arstatusao(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "arstatusao required numeric." : GoTo selesai
        End If
        'arstatusae(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "arstatusae required numeric." : GoTo selesai
        End If
        'arstatus(35) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "arstatus required numeric." : GoTo selesai
        End If
        'arstatussebelumnya(36) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "arstatussebelumnya required numeric." : GoTo selesai
        End If
        'arjmlrevisi(37) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "arjmlrevisi required numeric." : GoTo selesai
        End If
        'arcetakanke(38) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "arcetakanke required numeric." : GoTo selesai
        End If
        'arinputtgl(40) As DateTime
        If (IsDate(dataUtama(38)) = False) Then
            result(2) = "arinputtgl required date." : GoTo selesai
        End If
        'armodifikasitgl(42) As DateTime
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "armodifikasitgl required date." : GoTo selesai
        End If
        'arposting(43) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "arposting required numeric." : GoTo selesai
        End If
        'arpostingtgl(44) As DateTime
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "arpostingtgl required date." : GoTo selesai
        End If
        'arisclose(45) As Integer
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "arisclose required numeric." : GoTo selesai
        End If
        'arcustomint1(51) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "arcustomint1 required numeric." : GoTo selesai
        End If
        'arcustomint2(52) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "arcustomint2 required numeric." : GoTo selesai
        End If
        'arcustomint3(53) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "arcustomint3 required numeric." : GoTo selesai
        End If
        'arcustomdbl1(54) As Double
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "arcustomdbl1 required numeric." : GoTo selesai
        End If
        'arcustomdbl2(55) As Double
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "arcustomdbl2 required numeric." : GoTo selesai
        End If
        'arcustomdbl3(56) As Double
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "arcustomdbl3 required numeric." : GoTo selesai
        End If
        'arcustomdate1(57) As Date
        If (IsDate(dataUtama(55)) = False) Then
            result(2) = "arcustomdate1 required date." : GoTo selesai
        End If
        'arcustomdate2(58) As Date
        If (IsDate(dataUtama(56)) = False) Then
            result(2) = "arcustomdate2 required date." : GoTo selesai
        End If
        'arcustomdate3(59) As Date
        If (IsDate(dataUtama(57)) = False) Then
            result(2) = "arcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'arid(0) As 
        If Len(dataUtama(0)) = 0 Then
            result(2) = "arid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 20 Then
            result(2) = "arid should not be more than 20 character." : GoTo selesai
        End If

        'arcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "arcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "arcabang should not be more than 25 character." : GoTo selesai
        End If

        'arlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "arlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "arlokasi should not be more than 25 character." : GoTo selesai
        End If

        'arsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "arsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "arsumber should not be more than 10 character." : GoTo selesai
        End If

        'arnotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "arnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "arnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'artgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "artgl can't be empty" : GoTo selesai
        End If

        'arkodepa(7) As 
        If Len(dataUtama(7)) = 0 Then
            result(2) = "arkodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 20 Then
            result(2) = "arkodepa should not be more than 20 character." : GoTo selesai
        End If

        'ardimintaoleh(8) As 
        If Len(dataUtama(8)) = 0 Then
            result(2) = "ardimintaoleh can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 20 Then
            result(2) = "ardimintaoleh should not be more than 20 character." : GoTo selesai
        End If

        'armintake(10) As 
        If Len(dataUtama(10)) = 0 Then
            result(2) = "armintake can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(10)) > 20 Then
            result(2) = "armintake should not be more than 20 character." : GoTo selesai
        End If

        'artgldipakai(11) As Date
        If Len(dataUtama(11)) = 0 Then
            result(2) = "artgldipakai can't be empty" : GoTo selesai
        End If

        'artgljatuhtempo(13) As Date
        If Len(dataUtama(13)) = 0 Then
            result(2) = "artgljatuhtempo can't be empty" : GoTo selesai
        End If

        'artglnoref(17) As Date
        If Len(dataUtama(17)) = 0 Then
            result(2) = "artglnoref can't be empty" : GoTo selesai
        End If

        'artglpenutupan(18) As Date
        If Len(dataUtama(18)) = 0 Then
            result(2) = "artglpenutupan can't be empty" : GoTo selesai
        End If

        'armatauang(19) As String
        If Len(dataUtama(19)) = 0 Then
            result(2) = "armatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(19)) > 25 Then
            result(2) = "armatauang should not be more than 25 character." : GoTo selesai
        End If

        'arkurs(20) As Double
        If Len(dataUtama(20)) = 0 Then
            result(2) = "arkurs can't be empty" : GoTo selesai
        End If

        'artotal(22) As Double
        If Len(dataUtama(22)) = 0 Then
            result(2) = "artotal can't be empty" : GoTo selesai
        End If

        'ardiskonpersen(23) As String
        If Len(dataUtama(23)) = 0 Then
            result(2) = "ardiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(23)) > 25 Then
            result(2) = "ardiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'arjmldiskon(24) As Double
        If Len(dataUtama(24)) = 0 Then
            result(2) = "arjmldiskon can't be empty" : GoTo selesai
        End If

        'artotalpajak1detail(25) As Double
        If Len(dataUtama(25)) = 0 Then
            result(2) = "artotalpajak1detail can't be empty" : GoTo selesai
        End If

        'artotalpajak2detail(26) As Double
        If Len(dataUtama(26)) = 0 Then
            result(2) = "artotalpajak2detail can't be empty" : GoTo selesai
        End If

        'arbiayalainpersen(27) As String
        If Len(dataUtama(27)) = 0 Then
            result(2) = "arbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(27)) > 25 Then
            result(2) = "arbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'arbiayalain(28) As Double
        If Len(dataUtama(28)) = 0 Then
            result(2) = "arbiayalain can't be empty" : GoTo selesai
        End If

        'artotaltransaksi(29) As Double
        If Len(dataUtama(29)) = 0 Then
            result(2) = "artotaltransaksi can't be empty" : GoTo selesai
        End If

        'arinputuser(39) As 
        If Len(dataUtama(37)) = 0 Then
            result(2) = "arinputuser can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(37)) > 20 Then
            result(2) = "arinputuser should not be more than 20 character." : GoTo selesai
        End If

        'arinputtgl(40) As DateTime
        If Len(dataUtama(38)) = 0 Then
            result(2) = "arinputtgl can't be empty" : GoTo selesai
        End If

        'armodifikasiuser(41) As 
        If Len(dataUtama(39)) = 0 Then
            result(2) = "armodifikasiuser can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(39)) > 20 Then
            result(2) = "armodifikasiuser should not be more than 20 character." : GoTo selesai
        End If

        'armodifikasitgl(42) As DateTime
        If Len(dataUtama(40)) = 0 Then
            result(2) = "armodifikasitgl can't be empty" : GoTo selesai
        End If

        'arpostingtgl(44) As DateTime
        If Len(dataUtama(42)) = 0 Then
            result(2) = "arpostingtgl can't be empty" : GoTo selesai
        End If

        'arcustomdbl1(54) As Double
        If Len(dataUtama(52)) = 0 Then
            result(2) = "arcustomdbl1 can't be empty" : GoTo selesai
        End If

        'arcustomdbl2(55) As Double
        If Len(dataUtama(53)) = 0 Then
            result(2) = "arcustomdbl2 can't be empty" : GoTo selesai
        End If

        'arcustomdbl3(56) As Double
        If Len(dataUtama(54)) = 0 Then
            result(2) = "arcustomdbl3 can't be empty" : GoTo selesai
        End If

        'arcustomdate1(57) As Date
        If Len(dataUtama(55)) = 0 Then
            result(2) = "arcustomdate1 can't be empty" : GoTo selesai
        End If

        'arcustomdate2(58) As Date
        If Len(dataUtama(56)) = 0 Then
            result(2) = "arcustomdate2 can't be empty" : GoTo selesai
        End If

        'arcustomdate3(59) As Date
        If Len(dataUtama(57)) = 0 Then
            result(2) = "arcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "arid", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "arcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "arlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "arsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "arautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "arnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "artgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "arkodepa", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "ardimintaoleh", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "ardimintaolehkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "armintake", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "artgldipakai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "artermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "artgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aruraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "arcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "arnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "artglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "artglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "armatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "arkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "arhargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "artotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ardiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "arjmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "artotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "artotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "arbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "arbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "artotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "arstatusaq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "arstatusao", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "arstatusae", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "arstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "arstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "arjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "arcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "arinputuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "arinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "armodifikasiuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "armodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "arposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "arpostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "arisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "arcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "arcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "arcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "arcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "arcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "arcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "arcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "arcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "arcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "arcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "arcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "arcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "arcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "arcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "arid~arcabang~arlokasi~arsumber~arautonotransaksi~arnotransaksi~artgl~arkodepa~ardimintaoleh~ardimintaolehkontak~armintake~artgldipakai~artermin~artgljatuhtempo~aruraian~arcatatan~arnoref~artglnoref~artglpenutupan~armatauang~arkurs~arhargatermasukpajak~artotal~ardiskonpersen~arjmldiskon~artotalpajak1detail~artotalpajak2detail~arbiayalainpersen~arbiayalain~artotaltransaksi~arstatusaq~arstatusao~arstatusae~arstatus~arstatussebelumnya~arjmlrevisi~arcetakanke~arinputuser~arinputtgl~armodifikasiuser~armodifikasitgl~arposting~arpostingtgl~arisclose~arcustomtext1~arcustomtext2~arcustomtext3~arcustomtext4~arcustomtext5~arcustomint1~arcustomint2~arcustomint3~arcustomdbl1~arcustomdbl2~arcustomdbl3~arcustomdate1~arcustomdate2~arcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idardetail(0) As , idar(1) As , idasset(2) As , namaasset(3) As String, jml(4) As Double, 
        'matauang(5) As String, kurs(6) As Double, harga(7) As Double, diskon(8) As String, jmldiskon(9) As Double, 
        'pajak1(10) As String, jmlpajak1(11) As Double, pajak2(12) As String, jmlpajak2(13) As Double, cabang(14) As String, 
        'lokasi(15) As String, costcenter(16) As String, divisi(17) As String, subdivisi(18) As String, proyek(19) As String, 
        'catatan(20) As String, urutan(21) As Integer, jmlaq(22) As Double, statusaq(23) As Integer, jmlao(24) As Double, 
        'statusao(25) As Integer, jmlae(26) As Double, statusae(27) As Integer, isclose(28) As Integer, customtext1(29) As String, customtext2(30) As String, 
        'customtext3(31) As String, customdbl1(32) As Double, customdbl2(33) As Double, customdbl3(34) As Double, customdate1(35) As Date, 
        'customdate2(36) As Date, customdate3(37) As Date, satuan(38) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idardetail, idar, idasset, namaasset, jml, matauang, kurs, 
        'harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, 
        'cabang, lokasi, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, jmlaq, statusaq, jmlao, statusao, jmlae, statusae, 
        'isclose, customtext1, customtext2, 
        'customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3,satuan


        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idardetail", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idar", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idasset", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "namaasset", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsDouble)
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
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlaq", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusaq", AsEnumTypeData.AsInt64)
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

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 39) Then
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
            'jmlaq(22) As Double
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "jmlaq required numeric." : GoTo selesai
            End If
            'statusaq(23) As Integer
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "statusaq required numeric." : GoTo selesai
            End If
            'jmlao(24) As Double
            If (IsNumeric(dataRowDetail(24)) = False) Then
                result(2) = "jmlao required numeric." : GoTo selesai
            End If
            'statusao(25) As Integer
            If (IsNumeric(dataRowDetail(25)) = False) Then
                result(2) = "statusao required numeric." : GoTo selesai
            End If
            'jmlae(26) As Double
            If (IsNumeric(dataRowDetail(26)) = False) Then
                result(2) = "jmlae required numeric." : GoTo selesai
            End If
            'statusae(27) As Integer
            If (IsNumeric(dataRowDetail(27)) = False) Then
                result(2) = "statusae required numeric." : GoTo selesai
            End If
            'isclose(32) As Integer
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "isclose required numeric." : GoTo selesai
            End If
            'customdbl1(36) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(37) As Double
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(38) As Double
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(39) As Date
            If (IsDate(dataRowDetail(35)) = False) Then
                result(2) = "customdate1 required date." : GoTo selesai
            End If
            'customdate2(40) As Date
            If (IsDate(dataRowDetail(36)) = False) Then
                result(2) = "customdate2 required date." : GoTo selesai
            End If
            'customdate3(41) As Date
            If (IsDate(dataRowDetail(37)) = False) Then
                result(2) = "customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'idardetail(0) As 
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idardetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idardetail should not be more than 20 character." : GoTo selesai
            End If

            'idar(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idar can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idar should not be more than 20 character." : GoTo selesai
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

            'jmlaq(22) As Double
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - jmlaq can't be empty" : GoTo selesai
            End If

            'jmlao(24) As Double
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - jmlao can't be empty" : GoTo selesai
            End If

            'jmlae(26) As Double
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - jmlae can't be empty" : GoTo selesai
            End If

            'customdbl1(36) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(37) As Double
            If Len(dataRowDetail(33)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(38) As Double
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(39) As Date
            If Len(dataRowDetail(35)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(40) As Date
            If Len(dataRowDetail(36)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(41) As Date
            If Len(dataRowDetail(37)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(38)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idardetail~idar~idasset~namaasset~jml~matauang~kurs~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~costcenter~divisi~subdivisi~proyek~catatan~urutan~jmlaq~statusaq~jmlao~statusao~jmlae~statusae~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~satuan", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38)) = False Then
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
                Dim drutama As DataRow = dtutama.Rows(0)

                'SET TGL JATUH TEMPO ====================================
                Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                rsTglJT = F_TglJT(drutama("artermin").ToString, AsFormatTanggal(drutama("artgl")), "artgl").Split(sptSubParam)
                If rsTglJT(0) = 0 Then
                    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                Else
                    drutama("artgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                End If
                'END OF SET TGL JATUH TEMPO =============================

                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TOTAL = subtotal
                drutama("artotal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("artotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("artotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                drutama("artotaltransaksi") = Double.Parse(drutama("artotal")) - Double.Parse(drutama("arjmldiskon")) + Double.Parse(drutama("artotalpajak1detail")) + Double.Parse(drutama("artotalpajak2detail")) + Double.Parse(drutama("arbiayalain"))
                'END OF PERHITUNGAN TOTAL UTAMA =========================

                If isUpdate Then
                    result(4) = drutama("arid")
                    notransaksi = drutama("arnotransaksi")

                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(arid), arnotransaksi FROM M7_ar WHERE arid='" & result(4) & "' AND arstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then
                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(arid) FROM m7_ar WHERE arnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        sql = "Update M7_Ar set arcabang  = '" & FixQuotes(drutama("arcabang")) & "', arlokasi  = '" & FixQuotes(drutama("arlokasi")) & "', arsumber  = '" & FixQuotes(drutama("arsumber")) & "', arautonotransaksi  = " & drutama("arautonotransaksi") & ", arnotransaksi  = '" & FixQuotes(drutama("arnotransaksi")) & "', artgl  = '" & FixQuotes(AsFormatTanggal(drutama("artgl"))) & "', arkodepa  = '" & FixQuotes(drutama("arkodepa")) & "', ardimintaoleh  = '" & FixQuotes(drutama("ardimintaoleh")) & "', ardimintaolehkontak  = '" & FixQuotes(drutama("ardimintaolehkontak")) & "', armintake  = '" & FixQuotes(drutama("armintake")) & "', artgldipakai  = '" & FixQuotes(AsFormatTanggal(drutama("artgldipakai"))) & "', artermin  = '" & FixQuotes(drutama("artermin")) & "', artgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("artgljatuhtempo"))) & "', aruraian  = '" & FixQuotes(drutama("aruraian")) & "', arcatatan  = '" & FixQuotes(drutama("arcatatan")) & "', arnoref  = '" & FixQuotes(drutama("arnoref")) & "', artglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("artglnoref"))) & "', artglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("artglpenutupan"))) & "', armatauang  = '" & FixQuotes(drutama("armatauang")) & "', arkurs  = '" & FixDouble(drutama("arkurs")) & "', arhargatermasukpajak  = " & drutama("arhargatermasukpajak") & ", artotal  = '" & FixDouble(drutama("artotal")) & "', ardiskonpersen  = '" & FixQuotes(drutama("ardiskonpersen")) & "', arjmldiskon  = '" & FixDouble(drutama("arjmldiskon")) & "', artotalpajak1detail  = '" & FixDouble(drutama("artotalpajak1detail")) & "', artotalpajak2detail  = '" & FixDouble(drutama("artotalpajak2detail")) & "', arbiayalainpersen  = '" & FixQuotes(drutama("arbiayalainpersen")) & "', arbiayalain  = '" & FixDouble(drutama("arbiayalain")) & "', artotaltransaksi  = '" & FixDouble(drutama("artotaltransaksi")) & "', arstatusaq  = " & drutama("arstatusaq") & ", arstatusao  = " & drutama("arstatusao") & ", arstatusae  = " & drutama("arstatusae") & ", arstatus  = " & drutama("arstatus") & ", arstatussebelumnya  = " & drutama("arstatussebelumnya") & ", arjmlrevisi  = arjmlrevisi+1, arcetakanke  = " & drutama("arcetakanke") & ", armodifikasiuser  = '" & FixQuotes(drutama("armodifikasiuser")) & "', armodifikasitgl  = NOW(), arposting  = " & drutama("arposting") & ", arpostingtgl  = '" & FixQuotes(AsFormatTanggal(drutama("arpostingtgl"), "yyyy-MM-dd H:mm:ss")) & "', arcustomtext1  = '" & FixQuotes(drutama("arcustomtext1")) & "', arcustomtext2  = '" & FixQuotes(drutama("arcustomtext2")) & "', arcustomtext3  = '" & FixQuotes(drutama("arcustomtext3")) & "', arcustomtext4  = '" & FixQuotes(drutama("arcustomtext4")) & "', arcustomtext5  = '" & FixQuotes(drutama("arcustomtext5")) & "', arcustomint1  = " & drutama("arcustomint1") & ", arcustomint2  = " & drutama("arcustomint2") & ", arcustomint3  = " & drutama("arcustomint3") & ", arcustomdbl1  = '" & FixDouble(drutama("arcustomdbl1")) & "', arcustomdbl2  = '" & FixDouble(drutama("arcustomdbl2")) & "', arcustomdbl3  = '" & FixDouble(drutama("arcustomdbl3")) & "', arcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("arcustomdate1"))) & "', arcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("arcustomdate2"))) & "', arcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("arcustomdate3"))) & "' where arid = " & drutama("arid") & ""
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

                    If drutama("arautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("arcabang"), drutama("arlokasi"), drutama("arsumber"), drutama("artgl"))
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
                        notransaksi = drutama("arnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(arid) FROM m7_ar WHERE arnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M7_Ar (arcabang, arlokasi, arsumber, arautonotransaksi, arnotransaksi, artgl, arkodepa, ardimintaoleh, ardimintaolehkontak, armintake, artgldipakai, artermin, artgljatuhtempo, aruraian, arcatatan, arnoref, artglnoref, artglpenutupan, armatauang, arkurs, arhargatermasukpajak, artotal, ardiskonpersen, arjmldiskon, artotalpajak1detail, artotalpajak2detail, arbiayalainpersen, arbiayalain, artotaltransaksi, arstatusaq, arstatusao, arstatusae, arstatus, arstatussebelumnya, arjmlrevisi, arcetakanke, arinputuser, arinputtgl, armodifikasiuser, armodifikasitgl, arposting, arpostingtgl, arisclose, arcustomtext1, arcustomtext2, arcustomtext3, arcustomtext4, arcustomtext5, arcustomint1, arcustomint2, arcustomint3, arcustomdbl1, arcustomdbl2, arcustomdbl3, arcustomdate1, arcustomdate2, arcustomdate3) values('" & FixQuotes(drutama("arcabang")) & "', '" & FixQuotes(drutama("arlokasi")) & "', '" & FixQuotes(drutama("arsumber")) & "', " & drutama("arautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("artgl"))) & "', '" & FixQuotes(drutama("arkodepa")) & "', '" & FixQuotes(drutama("ardimintaoleh")) & "', '" & FixQuotes(drutama("ardimintaolehkontak")) & "', '" & FixQuotes(drutama("armintake")) & "', '" & FixQuotes(AsFormatTanggal(drutama("artgldipakai"))) & "', '" & FixQuotes(drutama("artermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("artgljatuhtempo"))) & "', '" & FixQuotes(drutama("aruraian")) & "', '" & FixQuotes(drutama("arcatatan")) & "', '" & FixQuotes(drutama("arnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("artglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("artglpenutupan"))) & "', '" & FixQuotes(drutama("armatauang")) & "', '" & FixDouble(drutama("arkurs")) & "', " & drutama("arhargatermasukpajak") & ", '" & FixDouble(drutama("artotal")) & "', '" & FixQuotes(drutama("ardiskonpersen")) & "', '" & FixDouble(drutama("arjmldiskon")) & "', '" & FixDouble(drutama("artotalpajak1detail")) & "', '" & FixDouble(drutama("artotalpajak2detail")) & "', '" & FixQuotes(drutama("arbiayalainpersen")) & "', '" & FixDouble(drutama("arbiayalain")) & "', '" & FixDouble(drutama("artotaltransaksi")) & "', " & drutama("arstatusaq") & ", " & drutama("arstatusao") & ", " & drutama("arstatusae") & ", " & drutama("arstatus") & ", " & drutama("arstatussebelumnya") & ", " & drutama("arjmlrevisi") & ", " & drutama("arcetakanke") & ", '" & FixQuotes(drutama("arinputuser")) & "', NOW(), '" & FixQuotes(drutama("armodifikasiuser")) & "', '1971-01-01 00:00:00', " & drutama("arposting") & ", '1971-01-01 00:00:00', " & drutama("arisclose") & ", '" & FixQuotes(drutama("arcustomtext1")) & "', '" & FixQuotes(drutama("arcustomtext2")) & "', '" & FixQuotes(drutama("arcustomtext3")) & "', '" & FixQuotes(drutama("arcustomtext4")) & "', '" & FixQuotes(drutama("arcustomtext5")) & "', " & drutama("arcustomint1") & ", " & drutama("arcustomint2") & ", " & drutama("arcustomint3") & ", '" & FixDouble(drutama("arcustomdbl1")) & "', '" & FixDouble(drutama("arcustomdbl2")) & "', '" & FixDouble(drutama("arcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("arcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("arcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("arcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select arid from M7_ar where arnotransaksi='" & notransaksi & "' AND arinputuser= '" & userid & "' order by armodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                    result(4) = dt2.Rows(0)(0)
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M7_Ar_Detail where idar = " & result(4)
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
                        strValue2.Append("('" & FixQuotes(dr1("idardetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("idasset")) & "', '" & FixQuotes(dr1("namaasset")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", '" & FixDouble(dr1("jmlaq")) & "', " & dr1("statusaq") & ", '" & FixDouble(dr1("jmlao")) & "', " & dr1("statusao") & ", '" & FixDouble(dr1("jmlae")) & "', " & dr1("statusae") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixDouble(dr1("satuan")) & "')")
                    Next
                    sql = "Insert into M7_Ar_Detail(idardetail, idar, idasset, namaasset, jml, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlaq, statusaq, jmlao, statusao, jmlae, statusae, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, satuan) values" & strValue2.ToString & ""
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

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "Ar", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M7_ArGetdataById(ByVal param As String) As String

        'M7_ArGetdataById Utama --------------------------------------------------------
        'arid, arcabang, arlokasi, arsumber, arautonotransaksi, arnotransaksi, artgl, 
        'arkodepa, ardimintaoleh, ardimintaolehkontak, armintake, artgldipakai, artermin, artgljatuhtempo, 
        'aruraian, arcatatan, arnoref, artglnoref, artglpenutupan, armatauang, arkurs, 
        'arhargatermasukpajak, artotal, ardiskonpersen, arjmldiskon, artotalpajak1detail, artotalpajak2detail, arbiayalainpersen, 
        'arbiayalain, artotaltransaksi, arstatusaq, arstatusao, arstatusae, arstatusrealisasi, arstatus, 
        'arstatussebelumnya, arjmlrevisi, arcetakanke, arinputuser, arinputtgl, armodifikasiuser, armodifikasitgl, 
        'arposting, arpostingtgl, arisclose, arcustomtext1, arcustomtext2, arcustomtext3, arcustomtext4, 
        'arcustomtext5, arcustomint1, arcustomint2, arcustomint3, arcustomdbl1, arcustomdbl2, arcustomdbl3, 
        'arcustomdate1, arcustomdate2, arcustomdate3, arcabangnama, arlokasinama, ardimintaolehkode, ardimintaolehnama, 
        'armintakekode, armintakenama, arterminnama, arterminharijatuhtempo, arstatusnama, arstatussebelumnyanama, arinputusernama, 
        'armodifikasiusernama

        'M7_ArGetdataById Detail -------------------------------------------------------
        'idardetail, idar, idasset, namaasset, jml, matauang, 
        'kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, 
        'jmlpajak2, cabang, lokasi, costcenter, divisi, subdivisi, proyek, 
        'catatan, urutan, jmlaq, statusaq, jmlao, statusao, jmlae, 
        'statusae, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, pajak1nama, 
        'pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, costcenternama, divisinama, 
        'subdivisinama, proyeknama, satuan

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

        Dim NmMemcached As String = "aplikasi1-M4_Pr~M4_Pr_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "arid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "arid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m7_ar_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                     FxDB(drutama("arid"), ""), sptField,
                     FxDB(drutama("arcabang"), ""), sptField,
                     FxDB(drutama("arlokasi"), ""), sptField,
                     FxDB(drutama("arsumber"), ""), sptField,
                     FxDB(drutama("arautonotransaksi"), 0), sptField,
                     FxDB(drutama("arnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("artgl"), ""), formatTgl), sptField,
                     FxDB(drutama("arkodepa"), ""), sptField,
                     FxDB(drutama("ardimintaoleh"), ""), sptField,
                     FxDB(drutama("ardimintaolehkontak"), ""), sptField,
                     FxDB(drutama("armintake"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("artgldipakai"), ""), formatTgl), sptField,
                     FxDB(drutama("artermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("artgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("aruraian"), ""), sptField,
                     FxDB(drutama("arcatatan"), ""), sptField,
                     FxDB(drutama("arnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("artglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("artglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("armatauang"), ""), sptField,
                     FxDB(drutama("arkurs"), 0), sptField,
                     FxDB(drutama("arhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("artotal"), 0), sptField,
                     FxDB(drutama("ardiskonpersen"), ""), sptField,
                     FxDB(drutama("arjmldiskon"), 0), sptField,
                     FxDB(drutama("artotalpajak1detail"), 0), sptField,
                     FxDB(drutama("artotalpajak2detail"), 0), sptField,
                     FxDB(drutama("arbiayalainpersen"), ""), sptField,
                     FxDB(drutama("arbiayalain"), 0), sptField,
                     FxDB(drutama("artotaltransaksi"), 0), sptField,
                     FxDB(drutama("arstatusaq"), 0), sptField,
                     FxDB(drutama("arstatusao"), 0), sptField,
                     FxDB(drutama("arstatusae"), 0), sptField,
                     FxDB(drutama("arstatusrealisasi"), 0), sptField,
                     FxDB(drutama("arstatus"), 0), sptField,
                     FxDB(drutama("arstatussebelumnya"), 0), sptField,
                     FxDB(drutama("arjmlrevisi"), 0), sptField,
                     FxDB(drutama("arcetakanke"), 0), sptField,
                     FxDB(drutama("arinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("arinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("armodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("armodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("arposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("arpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("arisclose"), 0), sptField,
                     FxDB(drutama("arcustomtext1"), ""), sptField,
                     FxDB(drutama("arcustomtext2"), ""), sptField,
                     FxDB(drutama("arcustomtext3"), ""), sptField,
                     FxDB(drutama("arcustomtext4"), ""), sptField,
                     FxDB(drutama("arcustomtext5"), ""), sptField,
                     FxDB(drutama("arcustomint1"), 0), sptField,
                     FxDB(drutama("arcustomint2"), 0), sptField,
                     FxDB(drutama("arcustomint3"), 0), sptField,
                     FxDB(drutama("arcustomdbl1"), 0), sptField,
                     FxDB(drutama("arcustomdbl2"), 0), sptField,
                     FxDB(drutama("arcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("arcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("arcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("arcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("arcabangnama"), ""), sptField,
                     FxDB(drutama("arlokasinama"), ""), sptField,
                     FxDB(drutama("ardimintaolehkode"), ""), sptField,
                     FxDB(drutama("ardimintaolehnama"), ""), sptField,
                     FxDB(drutama("armintakekode"), ""), sptField,
                     FxDB(drutama("armintakenama"), ""), sptField,
                     FxDB(drutama("arterminnama"), ""), sptField,
                     FxDB(drutama("arterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("arstatusnama"), ""), sptField,
                     FxDB(drutama("arstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("arinputusernama"), ""), sptField,
                     FxDB(drutama("armodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idardetail"), ""), sptField,
                     FxDB(dr("idar"), ""), sptField,
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
                     FxDB(dr("jmlaq"), 0), sptField,
                     FxDB(dr("statusaq"), 0), sptField,
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
            result(2) = "transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, detail)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("arid, arcabang, arlokasi, arsumber, arautonotransaksi, arnotransaksi, artgl, arkodepa, ardimintaoleh, ardimintaolehkontak, armintake, artgldipakai, artermin, artgljatuhtempo, aruraian, arcatatan, arnoref, artglnoref, artglpenutupan, armatauang, arkurs, arhargatermasukpajak, artotal, ardiskonpersen, arjmldiskon,artotalpajak1detail, artotalpajak2detail, arbiayalainpersen, arbiayalain, artotaltransaksi, arstatusaq, arstatusao, arstatusae, arstatusrealisasi, arstatus, arstatussebelumnya, arjmlrevisi, arcetakanke, arinputuser, arinputtgl, armodifikasiuser, armodifikasitgl, arposting, arpostingtgl, arisclose, arcustomtext1, arcustomtext2, arcustomtext3, arcustomtext4, arcustomtext5, arcustomint1, arcustomint2, arcustomint3, arcustomdbl1, arcustomdbl2, arcustomdbl3, arcustomdate1, arcustomdate2, arcustomdate3, arcabangnama, arlokasinama, ardimintaolehkode, ardimintaolehnama, armintakekode, armintakenama, arterminnama, arterminharijatuhtempo, arstatusnama, arstatussebelumnyanama, arinputusernama, armodifikasiusernama" & sptSubParam & "idardetail, idar, idasset, namaasset, jml, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlaq, statusaq, jmlao, statusao, jmlae, statusae, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, costcenternama, divisinama, subdivisinama, proyeknama, satuan"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M7_ArSearch(ByVal param As String) As String
        'M7_ArSearch --------------------------------------------------------
        'arid, arcabang, arlokasi, arsumber, arautonotransaksi, arnotransaksi, artgl, 
        'arkodepa, ardimintaoleh, ardimintaolehkontak, armintake, artgldipakai, artermin, artgljatuhtempo, 
        'aruraian, arcatatan, arnoref, artglnoref, artglpenutupan, armatauang, arkurs, 
        'arhargatermasukpajak, artotal, ardiskonpersen, arjmldiskon, artotalpajak1detail, artotalpajak2detail, arbiayalainpersen, 
        'arbiayalain, artotaltransaksi, arstatusrq, arstatuspo, arstatusgrn, arstatusrealisasi, arstatus, 
        'arstatussebelumnya, arjmlrevisi, arcetakanke, arinputuser, arinputtgl, armodifikasiuser, armodifikasitgl, 
        'arposting, arpostingtgl, arisclose, arcabangnama, arlokasinama, ardimintaolehkode, ardimintaolehnama, 
        'armintakekode, armintakenama, arstatusnama, arstatussebelumnyanama, arinputusernama, armodifikasiusernama

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
            Filter = Filter.Replace("ardimintaolehkode", "c1.kkode")
            Filter = Filter.Replace("ardimintaolehnama", "c1.knama")
            Filter = Filter.Replace("armintakekode", "c2.kkode")
            Filter = Filter.Replace("armintakenama", "c2.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m7_ar_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Pr", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("arid"), ""), sptField,
                     FxDB(dr("arcabang"), ""), sptField,
                     FxDB(dr("arlokasi"), ""), sptField,
                     FxDB(dr("arsumber"), ""), sptField,
                     FxDB(dr("arautonotransaksi"), 0), sptField,
                     FxDB(dr("arnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("artgl"), ""), formatTgl), sptField,
                     FxDB(dr("arkodepa"), ""), sptField,
                     FxDB(dr("ardimintaoleh"), ""), sptField,
                     FxDB(dr("ardimintaolehkontak"), ""), sptField,
                     FxDB(dr("armintake"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("artgldipakai"), ""), formatTgl), sptField,
                     FxDB(dr("artermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("artgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("aruraian"), ""), sptField,
                     FxDB(dr("arcatatan"), ""), sptField,
                     FxDB(dr("arnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("artglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("artglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("armatauang"), ""), sptField,
                     FxDB(dr("arkurs"), 0), sptField,
                     FxDB(dr("arhargatermasukpajak"), 0), sptField,
                     FxDB(dr("artotal"), 0), sptField,
                     FxDB(dr("ardiskonpersen"), ""), sptField,
                     FxDB(dr("arjmldiskon"), 0), sptField,
                     FxDB(dr("artotalpajak1detail"), 0), sptField,
                     FxDB(dr("artotalpajak2detail"), 0), sptField,
                     FxDB(dr("arbiayalainpersen"), ""), sptField,
                     FxDB(dr("arbiayalain"), 0), sptField,
                     FxDB(dr("artotaltransaksi"), 0), sptField,
                     FxDB(dr("arstatusrq"), 0), sptField,
                     FxDB(dr("arstatuspo"), 0), sptField,
                     FxDB(dr("arstatusgrn"), 0), sptField,
                     FxDB(dr("arstatusrealisasi"), 0), sptField,
                     FxDB(dr("arstatus"), 0), sptField,
                     FxDB(dr("arstatussebelumnya"), 0), sptField,
                     FxDB(dr("arjmlrevisi"), 0), sptField,
                     FxDB(dr("arcetakanke"), 0), sptField,
                     FxDB(dr("arinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("arinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("armodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("armodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("arposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("arpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("arisclose"), 0), sptField,
                     FxDB(dr("arcabangnama"), ""), sptField,
                     FxDB(dr("arlokasinama"), ""), sptField,
                     FxDB(dr("ardimintaolehkode"), ""), sptField,
                     FxDB(dr("ardimintaolehnama"), ""), sptField,
                     FxDB(dr("armintakekode"), ""), sptField,
                     FxDB(dr("armintakenama"), ""), sptField,
                     FxDB(dr("arstatusnama"), ""), sptField,
                     FxDB(dr("arstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("arinputusernama"), ""), sptField,
                     FxDB(dr("armodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("arid, arcabang, arlokasi, arsumber, arautonotransaksi, arnotransaksi, artgl, arkodepa, ardimintaoleh, ardimintaolehkontak, armintake, artgldipakai, artermin, artgljatuhtempo, aruraian, arcatatan, arnoref, artglnoref, artglpenutupan, armatauang, arkurs, arhargatermasukpajak, artotal, ardiskonpersen, arjmldiskon, artotalpajak1detail, artotalpajak2detail, arbiayalainpersen, arbiayalain, artotaltransaksi, arstatusrq, arstatuspo, arstatusgrn, arstatusrealisasi, arstatus, arstatussebelumnya, arjmlrevisi, arcetakanke, arinputuser, arinputtgl, armodifikasiuser, armodifikasitgl, arposting, arpostingtgl, arisclose, arcabangnama, arlokasinama, ardimintaolehkode, ardimintaolehnama, armintakekode, armintakenama, arstatusnama, arstatussebelumnyanama, arinputusernama, armodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M7_Ar_Detail_VSearch(ByVal param As String) As String
        'M7_Ar_Detail_VSearch --------------------------------------------------------
        'idardetail, idar, idasset, namaasset, jml, matauang, kurs, harga, diskon, 
        'jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, 
        'cabang, lokasi, costcenter, divisi, subdivisi, 
        'proyek, catatan, urutan, jmlaq, 
        'statusaq, jmlao, statusao, jmlae, statusae, jmlrealisasi, 
        'statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3, arnotransaksi, artgldipakai, aruraian, 
        'arcatatan, arnoref, artglnoref, artermin, arterminnama, arterminharijatuhtempo, 
        'pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisaaq, jmlsisaao, 
        'jmlsisarealisasi, satuan

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
        sql = query.PanggilQuery("m7_ar_detail_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M5_Sq_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idardetail"), 0), sptField,
                     FxDB(dr("idar"), 0), sptField,
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
                     FxDB(dr("jmlaq"), 0), sptField,
                     FxDB(dr("statusaq"), 0), sptField,
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
                     FxDB(dr("arnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("artgldipakai"), ""), formatTgl), sptField,
                     FxDB(dr("aruraian"), ""), sptField,
                     FxDB(dr("arcatatan"), ""), sptField,
                     FxDB(dr("arnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("artglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("artermin"), ""), sptField,
                     FxDB(dr("arterminnama"), ""), sptField,
                     FxDB(dr("arterminharijatuhtempo"), 0), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("jmlsisaaq"), 0), sptField,
                     FxDB(dr("jmlsisaao"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idardetail, idar, idasset, namaasset, jml, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlaq, statusaq, jmlao, statusao, jmlae, statusae, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, arnotransaksi, artgldipakai, aruraian, arcatatan, arnoref, artglnoref, artermin, arterminnama, arterminharijatuhtempo, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisaaq, jmlsisaao, jmlsisarealisasi, satuan"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M7_ArTerkait(ByVal param As String) As String
        'M7_ArTerkait --------------------------------------------------------
        'arid, arnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "arid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m7_ar_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("arid"), 0), sptField,
                     FxDB(dr("arnotransaksi"), ""), sptField,
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
            result(2) = "Related AR data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("arid, arnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M7_ArUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("ardimintaolehkode", "c1.kkode")
            Filter = Filter.Replace("ardimintaolehnama", "c1.knama")
            Filter = Filter.Replace("armintakekode", "c2.kkode")
            Filter = Filter.Replace("armintakenama", "c2.knama")
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
            Dim sumber As String = "Ar", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Artgl, Arnotransaksi, Arstatus FROM m7_Ar WHERE Arid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Arstatussebelumnya" : jnsaktivitas = 17
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
            'Dim SimpanHistory As New m4_pr_history
            'Dim rsSimpanHistory As String = SimpanHistory.M4_Pr_HistorySimpan("" & paramSplit(0) & "★M4_Pr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m7_ar_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                Dim idasset As Integer = 0, jml As Double = 0, idsqdetail As Integer = 0
                Dim ftOutstanding As String = "", updNilai As String = "", updFilter As String = ""
                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDB("SELECT idasset, namaasset, satuan, jml, urutan FROM m7_ar_detail WHERE idar = '" & idtransaksi & "'")
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idasset = dr1("idasset") : jml = dr1("jml")
                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If
            End If

            'update status utama
            sql = "UPDATE m7_Ar SET Arstatus = " & nilaiStatus & ", Armodifikasiuser='" & userid & "', Armodifikasitgl = NOW(), Arposting = 0, Arpostingtgl = '1971-01-01 00:00:00', Arjmlrevisi = Arjmlrevisi + 1 WHERE Arid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M7_ArSearch(PostWsSearch(paramSplit(0), "M7_ArSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M7_ArDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("ardimintaolehkode", "c1.kkode")
            Filter = Filter.Replace("ardimintaolehnama", "c1.knama")
            Filter = Filter.Replace("armintakekode", "c2.kkode")
            Filter = Filter.Replace("armintakenama", "c2.knama")
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
            Dim sumber As String = "Ar", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Arid, Arnotransaksi FROM m7_Ar WHERE Arid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT arcabang, arlokasi, arsumber, arautonotransaksi, arnotransaksi, artgl"
            sql &= " FROM M7_ar"
            sql &= " WHERE arid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("arcabang")
                lokasi = dtNomorNext.Rows(0)("arlokasi")
                sumber = dtNomorNext.Rows(0)("arsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("arautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("arnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("artgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M7_Ar_Detail WHERE idar ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M7_Ar WHERE arid ='" & idtransaksi & "'"
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
            Dim paramSearch As String = M7_ArSearch(PostWsSearch(paramSplit(0), "M7_ArSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
