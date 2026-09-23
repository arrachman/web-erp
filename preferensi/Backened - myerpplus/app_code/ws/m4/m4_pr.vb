Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_pr
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_PrSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataTrans(), dataRowTrans() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "yyyy-MM-dd H:mm:ss" : Dim isUpdate As Boolean

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
        If (dataSplit.Length <> 2 And dataSplit.Length <> 3) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'prid(0) As Integer, prcabang(1) As String, prlokasi(2) As String, prgudang(3) As String, prasalbarang(4) As String, 
        'prasalbarangkategori(5) As Integer, prjenispembelian(6) As String, prjenispembeliankategori(7) As Integer, prcarabayar(8) As Integer, prsumber(9) As String, 
        'prautonotransaksi(10) As Integer, prnotransaksi(11) As String, prtgl(12) As Date, prkodepa(13) As Integer, prdimintaoleh(14) As Integer, 
        'prdimintaolehkontak(15) As String, prmintake(16) As Integer, prtgldipakai(17) As Date, prtermin(18) As String, prtgljatuhtempo(19) As Date, 
        'pruraian(20) As String, prcatatan(21) As String, prnoref(22) As String, prtglnoref(23) As Date, prtglpenutupan(24) As Date, 
        'prmatauang(25) As String, prkurs(26) As Double, prhargatermasukpajak(27) As Integer, prtotal(28) As Double, prdiskonpersen(29) As String, 
        'prjmldiskon(30) As Double, prtotalpajak1detail(31) As Double, prtotalpajak2detail(32) As Double, prbiayalainpersen(33) As String, prbiayalain(34) As Double, 
        'prtotaltransaksi(35) As Double, pridsq(36) As Integer, prstatuscs(37) As Integer, prstatusrq(38) As Integer, prstatuspo(39) As Integer, 
        'prstatusipc(40) As Integer, prstatusgrn(41) As Integer, prstatusri(42) As Integer, prstatusdnr(43) As Integer, prstatusprt(44) As Integer, 
        'prstatus(45) As Integer, prstatussebelumnya(46) As Integer, prjmlrevisi(47) As Integer, prcetakanke(48) As Integer, prinputuser(49) As Integer, 
        'prinputtgl(50) As DateTime, prmodifikasiuser(51) As Integer, prmodifikasitgl(52) As DateTime, prisclose(53) As Integer, prcustomtext1(54) As String, 
        'prcustomtext2(55) As String, prcustomtext3(56) As String, prcustomtext4(57) As String, prcustomtext5(58) As String, prcustomint1(59) As Integer, 
        'prcustomint2(60) As Integer, prcustomint3(61) As Integer, prcustomdbl1(62) As Double, prcustomdbl2(63) As Double, prcustomdbl3(64) As Double, 
        'prcustomdate1(65) As Date, prcustomdate2(66) As Date, prcustomdate3(67) As Date, prtglawal(68) As DateTime, prtglakhir(69) As DateTime

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'prid, prcabang, prlokasi, prgudang, prasalbarang, prasalbarangkategori, prjenispembelian, 
        'prjenispembeliankategori, prcarabayar, prsumber, prautonotransaksi, prnotransaksi, prtgl, prkodepa, 
        'prdimintaoleh, prdimintaolehkontak, prmintake, prtgldipakai, prtermin, prtgljatuhtempo, pruraian, 
        'prcatatan, prnoref, prtglnoref, prtglpenutupan, prmatauang, prkurs, prhargatermasukpajak, 
        'prtotal, prdiskonpersen, prjmldiskon, prtotalpajak1detail, prtotalpajak2detail, prbiayalainpersen, prbiayalain, 
        'prtotaltransaksi, pridsq, prstatuscs, prstatusrq, prstatuspo, prstatusipc, prstatusgrn, 
        'prstatusri, prstatusdnr, prstatusprt, prstatus, prstatussebelumnya, prjmlrevisi, prcetakanke, 
        'prinputuser, prinputtgl, prmodifikasiuser, prmodifikasitgl, prisclose, prcustomtext1, prcustomtext2, 
        'prcustomtext3, prcustomtext4, prcustomtext5, prcustomint1, prcustomint2, prcustomint3, prcustomdbl1, 
        'prcustomdbl2, prcustomdbl3, prcustomdate1, prcustomdate2, prcustomdate3, prtglawal, prtglakhir

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 68 And dataUtama.Length <> 70) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'prid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "prid required numeric." : GoTo selesai
        End If
        'prasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "prasalbarangkategori required numeric." : GoTo selesai
        End If
        'prjenispembeliankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "prjenispembeliankategori required numeric." : GoTo selesai
        End If
        'prcarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "prcarabayar required numeric." : GoTo selesai
        End If
        'prautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "prautonotransaksi required numeric." : GoTo selesai
        End If
        'prtgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "prtgl required date." : GoTo selesai
        End If
        'prkodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "prkodepa required numeric." : GoTo selesai
        End If
        'prdimintaoleh(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "prdimintaoleh required numeric." : GoTo selesai
        End If
        If (dataUtama(14) < 1) Then
            result(2) = "prdimintaoleh can't be empty." : GoTo selesai
        End If
        'prmintake(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "prmintake required numeric." : GoTo selesai
        End If
        'prtgldipakai(17) As Date
        If (IsDate(dataUtama(17)) = False) Then
            result(2) = "prtgldipakai required date." : GoTo selesai
        End If
        'prtgljatuhtempo(19) As Date
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "prtgljatuhtempo required date." : GoTo selesai
        End If
        'prtglnoref(23) As Date
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "prtglnoref required date." : GoTo selesai
        End If
        'prtglpenutupan(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "prtglpenutupan required date." : GoTo selesai
        End If
        'prkurs(26) As Double
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "prkurs required numeric." : GoTo selesai
        End If
        'prhargatermasukpajak(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "prhargatermasukpajak required numeric." : GoTo selesai
        End If
        'prtotal(28) As Double
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "prtotal required numeric." : GoTo selesai
        End If
        'prjmldiskon(30) As Double
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "prjmldiskon required numeric." : GoTo selesai
        End If
        'prtotalpajak1detail(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "prtotalpajak1detail required numeric." : GoTo selesai
        End If
        'prtotalpajak2detail(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "prtotalpajak2detail required numeric." : GoTo selesai
        End If
        'prbiayalain(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "prbiayalain required numeric." : GoTo selesai
        End If
        'prtotaltransaksi(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "prtotaltransaksi required numeric." : GoTo selesai
        End If
        'pridsq(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "pridsq required numeric." : GoTo selesai
        End If
        'prstatuscs(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "prstatuscs required numeric." : GoTo selesai
        End If
        'prstatusrq(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "prstatusrq required numeric." : GoTo selesai
        End If
        'prstatuspo(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "prstatuspo required numeric." : GoTo selesai
        End If
        'prstatusipc(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "prstatusipc required numeric." : GoTo selesai
        End If
        'prstatusgrn(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "prstatusgrn required numeric." : GoTo selesai
        End If
        'prstatusri(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "prstatusri required numeric." : GoTo selesai
        End If
        'prstatusdnr(43) As Integer
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "prstatusdnr required numeric." : GoTo selesai
        End If
        'prstatusprt(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "prstatusprt required numeric." : GoTo selesai
        End If
        'prstatus(45) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "prstatus required numeric." : GoTo selesai
        End If
        'prstatussebelumnya(46) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "prstatussebelumnya required numeric." : GoTo selesai
        End If
        'prjmlrevisi(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "prjmlrevisi required numeric." : GoTo selesai
        End If
        'prcetakanke(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "prcetakanke required numeric." : GoTo selesai
        End If
        'prinputuser(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "prinputuser required numeric." : GoTo selesai
        End If
        'prinputtgl(50) As DateTime
        If (IsDate(dataUtama(50)) = False) Then
            result(2) = "prinputtgl required date." : GoTo selesai
        End If
        'prmodifikasiuser(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "prmodifikasiuser required numeric." : GoTo selesai
        End If
        'prmodifikasitgl(52) As DateTime
        If (IsDate(dataUtama(52)) = False) Then
            result(2) = "prmodifikasitgl required date." : GoTo selesai
        End If
        'prisclose(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "prisclose required numeric." : GoTo selesai
        End If
        'prcustomint1(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "prcustomint1 required numeric." : GoTo selesai
        End If
        'prcustomint2(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "prcustomint2 required numeric." : GoTo selesai
        End If
        'prcustomint3(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "prcustomint3 required numeric." : GoTo selesai
        End If
        'prcustomdbl1(62) As Double
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "prcustomdbl1 required numeric." : GoTo selesai
        End If
        'prcustomdbl2(63) As Double
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "prcustomdbl2 required numeric." : GoTo selesai
        End If
        'prcustomdbl3(64) As Double
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "prcustomdbl3 required numeric." : GoTo selesai
        End If
        'prcustomdate1(65) As Date
        If (IsDate(dataUtama(65)) = False) Then
            result(2) = "prcustomdate1 required date." : GoTo selesai
        End If
        'prcustomdate2(66) As Date
        If (IsDate(dataUtama(66)) = False) Then
            result(2) = "prcustomdate2 required date." : GoTo selesai
        End If
        'prcustomdate3(67) As Date
        If (IsDate(dataUtama(67)) = False) Then
            result(2) = "prcustomdate3 required date." : GoTo selesai
        End If
        If dataUtama.Length > 68 Then
            'prtglawal(68) As DateTime
            If (IsDate(dataUtama(68)) = False) Then
                result(2) = "prtglawal required date." : GoTo selesai
            End If
            'prtglakhir(69) As DateTime
            If (IsDate(dataUtama(69)) = False) Then
                result(2) = "prtglakhir required date." : GoTo selesai
            End If
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'prcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "prcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "prcabang should not be more than 25 character." : GoTo selesai
        End If

        'prlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "prlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "prlokasi should not be more than 25 character." : GoTo selesai
        End If

        'prgudang(3) As String
        'If Len(dataUtama(3)) = 0 Then
        '    result(2) = "prgudang can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "prgudang should not be more than 25 character." : GoTo selesai
        End If

        'prsumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "prsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "prsumber should not be more than 10 character." : GoTo selesai
        End If

        'prnotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "prnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "prnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'prtgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "prtgl can't be empty" : GoTo selesai
        End If

        'prtgldipakai(17) As Date
        If Len(dataUtama(17)) = 0 Then
            result(2) = "prtgldipakai can't be empty" : GoTo selesai
        End If

        'prtgljatuhtempo(19) As Date
        If Len(dataUtama(19)) = 0 Then
            result(2) = "prtgljatuhtempo can't be empty" : GoTo selesai
        End If

        'prtglnoref(23) As Date
        If Len(dataUtama(23)) = 0 Then
            result(2) = "prtglnoref can't be empty" : GoTo selesai
        End If

        'prtglpenutupan(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "prtglpenutupan can't be empty" : GoTo selesai
        End If

        'prmatauang(25) As String
        If Len(dataUtama(25)) = 0 Then
            result(2) = "prmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(25)) > 25 Then
            result(2) = "prmatauang should not be more than 25 character." : GoTo selesai
        End If

        'prkurs(26) As Double
        If Len(dataUtama(26)) = 0 Then
            result(2) = "prkurs can't be empty" : GoTo selesai
        End If

        'prtotal(28) As Double
        If Len(dataUtama(28)) = 0 Then
            result(2) = "prtotal can't be empty" : GoTo selesai
        End If

        'prdiskonpersen(29) As String
        If Len(dataUtama(29)) = 0 Then
            result(2) = "prdiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(29)) > 25 Then
            result(2) = "prdiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'prjmldiskon(30) As Double
        If Len(dataUtama(30)) = 0 Then
            result(2) = "prjmldiskon can't be empty" : GoTo selesai
        End If

        'prtotalpajak1detail(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "prtotalpajak1detail can't be empty" : GoTo selesai
        End If

        'prtotalpajak2detail(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "prtotalpajak2detail can't be empty" : GoTo selesai
        End If

        'prbiayalainpersen(33) As String
        If Len(dataUtama(33)) = 0 Then
            result(2) = "prbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(33)) > 25 Then
            result(2) = "prbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'prbiayalain(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "prbiayalain can't be empty" : GoTo selesai
        End If

        'prtotaltransaksi(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "prtotaltransaksi can't be empty" : GoTo selesai
        End If

        'prinputtgl(50) As DateTime
        If Len(dataUtama(50)) = 0 Then
            result(2) = "prinputtgl can't be empty" : GoTo selesai
        End If

        'prmodifikasitgl(52) As DateTime
        If Len(dataUtama(52)) = 0 Then
            result(2) = "prmodifikasitgl can't be empty" : GoTo selesai
        End If

        'prcustomdbl1(62) As Double
        If Len(dataUtama(62)) = 0 Then
            result(2) = "prcustomdbl1 can't be empty" : GoTo selesai
        End If

        'prcustomdbl2(63) As Double
        If Len(dataUtama(63)) = 0 Then
            result(2) = "prcustomdbl2 can't be empty" : GoTo selesai
        End If

        'prcustomdbl3(64) As Double
        If Len(dataUtama(64)) = 0 Then
            result(2) = "prcustomdbl3 can't be empty" : GoTo selesai
        End If

        'prcustomdate1(65) As Date
        If Len(dataUtama(65)) = 0 Then
            result(2) = "prcustomdate1 can't be empty" : GoTo selesai
        End If

        'prcustomdate2(66) As Date
        If Len(dataUtama(66)) = 0 Then
            result(2) = "prcustomdate2 can't be empty" : GoTo selesai
        End If

        'prcustomdate3(67) As Date
        If Len(dataUtama(67)) = 0 Then
            result(2) = "prcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "prid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prjenispembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prjenispembeliankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prdimintaoleh", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prdimintaolehkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prmintake", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtgldipakai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pruraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prhargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prdiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prjmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pridsq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prstatuscs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prstatusrq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prstatuspo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prstatusipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prstatusgrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prstatusri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prstatusdnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prstatusprt", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtglawal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtglakhir", AsEnumTypeData.AsString)
        If dataUtama.Length > 68 Then
            If AsDataTableTambahData(dtutama, "prid~prcabang~prlokasi~prgudang~prasalbarang~prasalbarangkategori~prjenispembelian~prjenispembeliankategori~prcarabayar~prsumber~prautonotransaksi~prnotransaksi~prtgl~prkodepa~prdimintaoleh~prdimintaolehkontak~prmintake~prtgldipakai~prtermin~prtgljatuhtempo~pruraian~prcatatan~prnoref~prtglnoref~prtglpenutupan~prmatauang~prkurs~prhargatermasukpajak~prtotal~prdiskonpersen~prjmldiskon~prtotalpajak1detail~prtotalpajak2detail~prbiayalainpersen~prbiayalain~prtotaltransaksi~pridsq~prstatuscs~prstatusrq~prstatuspo~prstatusipc~prstatusgrn~prstatusri~prstatusdnr~prstatusprt~prstatus~prstatussebelumnya~prjmlrevisi~prcetakanke~prinputuser~prinputtgl~prmodifikasiuser~prmodifikasitgl~prisclose~prcustomtext1~prcustomtext2~prcustomtext3~prcustomtext4~prcustomtext5~prcustomint1~prcustomint2~prcustomint3~prcustomdbl1~prcustomdbl2~prcustomdbl3~prcustomdate1~prcustomdate2~prcustomdate3~prtglawal~prtglakhir", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69)) = False Then
                result(2) = "Insert into main datatable failed." : GoTo selesai
            End If
        Else
            If AsDataTableTambahData(dtutama, "prid~prcabang~prlokasi~prgudang~prasalbarang~prasalbarangkategori~prjenispembelian~prjenispembeliankategori~prcarabayar~prsumber~prautonotransaksi~prnotransaksi~prtgl~prkodepa~prdimintaoleh~prdimintaolehkontak~prmintake~prtgldipakai~prtermin~prtgljatuhtempo~pruraian~prcatatan~prnoref~prtglnoref~prtglpenutupan~prmatauang~prkurs~prhargatermasukpajak~prtotal~prdiskonpersen~prjmldiskon~prtotalpajak1detail~prtotalpajak2detail~prbiayalainpersen~prbiayalain~prtotaltransaksi~pridsq~prstatuscs~prstatusrq~prstatuspo~prstatusipc~prstatusgrn~prstatusri~prstatusdnr~prstatusprt~prstatus~prstatussebelumnya~prjmlrevisi~prcetakanke~prinputuser~prinputtgl~prmodifikasiuser~prmodifikasitgl~prisclose~prcustomtext1~prcustomtext2~prcustomtext3~prcustomtext4~prcustomtext5~prcustomint1~prcustomint2~prcustomint3~prcustomdbl1~prcustomdbl2~prcustomdbl3~prcustomdate1~prcustomdate2~prcustomdate3~prtglawal~prtglakhir", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & "1971-01-01 00:00:00" & "~" & "1971-01-01 00:00:00") = False Then
                result(2) = "Insert into main datatable failed." : GoTo selesai
            End If
        End If
        

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idprdetail(0) As Integer, idpr(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, diskon(13) As String, jmldiskon(14) As Double, 
        'pajak1(15) As String, jmlpajak1(16) As Double, pajak2(17) As String, jmlpajak2(18) As Double, hargajual(19) As Double, 
        'stokterakhir(20) As Double, supplier(21) As Integer, cabang(22) As String, lokasi(23) As String, gudang(24) As String, 
        'costcenter(25) As String, divisi(26) As String, subdivisi(27) As String, proyek(28) As String, catatan(29) As String, 
        'urutan(30) As Integer, idsqdetail(31) As Integer, jmlcs(32) As Double, statuscs(33) As Integer, jmlrq(34) As Double, 
        'statusrq(35) As Integer, jmlpo(36) As Double, statuspo(37) As Integer, jmlipc(38) As Double, statusipc(39) As Integer, 
        'jmlgrn(40) As Double, statusgrn(41) As Integer, jmlri(42) As Double, statusri(43) As Integer, jmldnr(44) As Double, 
        'statusdnr(45) As Integer, jmlprt(46) As Double, statusprt(47) As Integer, isclose(48) As Integer, customtext1(49) As String, 
        'customtext2(50) As String, customtext3(51) As String, customdbl1(52) As Double, customdbl2(53) As Double, customdbl3(54) As Double, 
        'customdate1(55) As Date, customdate2(56) As Date, customdate3(57) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idprdetail, idpr, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, 
        'jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, hargajual, stokterakhir, 
        'supplier, cabang, lokasi, gudang, costcenter, divisi, subdivisi, 
        'proyek, catatan, urutan, idsqdetail, jmlcs, statuscs, jmlrq, 
        'statusrq, jmlpo, statuspo, jmlipc, statusipc, jmlgrn, statusgrn, 
        'jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idprdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "diskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmldiskon", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "pajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlpajak1", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "pajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlpajak2", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "hargajual", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "stokterakhir", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "supplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idsqdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlcs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuscs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlrq", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusrq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlpo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuspo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlipc", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlgrn", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusgrn", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)

        'Variabel ValidasiSimpan
        Dim ftExistOutstanding As String = "", ftOutstanding As String = ""
        Dim updNilai As String = "", updFilter As String = ""
        Dim idbarang As Integer = 0, idsqdetail As Integer = 0, jmlbarang As Double = 0
        'Dim ftExistOutstandingPro As String = "", ftOutstandingPro As String = ""
        'Dim updNilaiPro As String = "", updFilterPro As String = "", proyek As String = ""
        'Dim cekProyek As Boolean = False, cekItemProyek As String = "", dtItemProyek As New DataTable

        ''CEK ITEM PR WAJIB PROYEK
        'Dim dtSett As DataTable = AsDataTableAmbilDariDBCon("SELECT smodule, sgrup, skode, snilai FROM m0_setting WHERE smodule = 4 AND sgrup = 'options' AND skode = 'PrRequiredProject'", myConn)
        'If dtSett.Rows.Count > 0 Then
        '    If Len(FxDB(dtSett.Rows(0)("snilai"), "")) > 0 Then
        '        dtItemProyek = AsDataTableAmbilDariDBCon("SELECT bid, bkode, bnama FROM m1_item WHERE " & FxDB(dtSett.Rows(0)("snilai"), "") & "", myConn)
        '        If dtItemProyek.Rows.Count > 0 Then
        '            cekProyek = True
        '        End If
        '    End If
        'End If

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
            'idprdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idprdetail required numeric." : GoTo selesai
            End If
            'idpr(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idpr required numeric." : GoTo selesai
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
            'harga(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'jmldiskon(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - jmldiskon required numeric." : GoTo selesai
            End If
            'jmlpajak1(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak1 required numeric." : GoTo selesai
            End If
            'jmlpajak2(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak2 required numeric." : GoTo selesai
            End If
            'hargajual(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - hargajual required numeric." : GoTo selesai
            End If
            'stokterakhir(20) As Double
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - stokterakhir required numeric." : GoTo selesai
            End If
            'supplier(21) As Integer
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - supplier required numeric." : GoTo selesai
            End If
            'urutan(30) As Integer
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idsqdetail(31) As Integer
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - idsqdetail required numeric." : GoTo selesai
            End If
            'jmlcs(32) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - jmlcs required numeric." : GoTo selesai
            End If
            'statuscs(33) As Integer
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - statuscs required numeric." : GoTo selesai
            End If
            'jmlrq(34) As Double
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - jmlrq required numeric." : GoTo selesai
            End If
            'statusrq(35) As Integer
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - statusrq required numeric." : GoTo selesai
            End If
            'jmlpo(36) As Double
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - jmlpo required numeric." : GoTo selesai
            End If
            'statuspo(37) As Integer
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - statuspo required numeric." : GoTo selesai
            End If
            'jmlipc(38) As Double
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - jmlipc required numeric." : GoTo selesai
            End If
            'statusipc(39) As Integer
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - statusipc required numeric." : GoTo selesai
            End If
            'jmlgrn(40) As Double
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - jmlgrn required numeric." : GoTo selesai
            End If
            'statusgrn(41) As Integer
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - statusgrn required numeric." : GoTo selesai
            End If
            'jmlri(42) As Double
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - jmlri required numeric." : GoTo selesai
            End If
            'statusri(43) As Integer
            If (IsNumeric(dataRowDetail(43)) = False) Then
                result(2) = "Row : " & i & " - statusri required numeric." : GoTo selesai
            End If
            'jmldnr(44) As Double
            If (IsNumeric(dataRowDetail(44)) = False) Then
                result(2) = "Row : " & i & " - jmldnr required numeric." : GoTo selesai
            End If
            'statusdnr(45) As Integer
            If (IsNumeric(dataRowDetail(45)) = False) Then
                result(2) = "Row : " & i & " - statusdnr required numeric." : GoTo selesai
            End If
            'jmlprt(46) As Double
            If (IsNumeric(dataRowDetail(46)) = False) Then
                result(2) = "Row : " & i & " - jmlprt required numeric." : GoTo selesai
            End If
            'statusprt(47) As Integer
            If (IsNumeric(dataRowDetail(47)) = False) Then
                result(2) = "Row : " & i & " - statusprt required numeric." : GoTo selesai
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

            'harga(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'diskon(13) As String
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - diskon can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(13)) > 25 Then
                result(2) = "Row : " & i & " - diskon should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskon(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskon can't be empty" : GoTo selesai
            Else
                'HITUNG JMLDISKON : jml(5) As Double, harga(12) As Double, diskon(13) As String
                dataRowDetail(14) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(12)), FixQuotes(dataRowDetail(13).ToString))
            End If

            'jmlpajak1(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak1 can't be empty" : GoTo selesai
            End If

            'jmlpajak2(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak2 can't be empty" : GoTo selesai
            End If

            'hargajual(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - hargajual can't be empty" : GoTo selesai
            End If

            'stokterakhir(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - stokterakhir can't be empty" : GoTo selesai
            End If

            ''proyek(28) As String
            'If cekProyek Then
            '    'idbarang(2) As Integer
            '    cekItemProyek = AsDataTableDLookup(dtItemProyek, "bkode", "bid = " & dataRowDetail(2), "xxxEOFxxx")
            '    If cekItemProyek <> "xxxEOFxxx" Then
            '        If Len(dataRowDetail(28)) = 0 Then
            '            result(2) = "Row : " & i & " - project can't be empty" : GoTo selesai
            '        End If
            '    End If
            'End If

            ''catatan(29) As String
            'If Len(dataRowDetail(29)) = 0 Then
            '    result(2) = "Row : " & i & " - catatan can't be empty" : GoTo selesai
            'End If

            'jmlcs(32) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - jmlcs can't be empty" : GoTo selesai
            End If

            'jmlrq(34) As Double
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Row : " & i & " - jmlrq can't be empty" : GoTo selesai
            End If

            'jmlpo(36) As Double
            If Len(dataRowDetail(36)) = 0 Then
                result(2) = "Row : " & i & " - jmlpo can't be empty" : GoTo selesai
            End If

            'jmlipc(38) As Double
            If Len(dataRowDetail(38)) = 0 Then
                result(2) = "Row : " & i & " - jmlipc can't be empty" : GoTo selesai
            End If

            'jmlgrn(40) As Double
            If Len(dataRowDetail(40)) = 0 Then
                result(2) = "Row : " & i & " - jmlgrn can't be empty" : GoTo selesai
            End If

            'jmlri(42) As Double
            If Len(dataRowDetail(42)) = 0 Then
                result(2) = "Row : " & i & " - jmlri can't be empty" : GoTo selesai
            End If

            'jmldnr(44) As Double
            If Len(dataRowDetail(44)) = 0 Then
                result(2) = "Row : " & i & " - jmldnr can't be empty" : GoTo selesai
            End If

            'jmlprt(46) As Double
            If Len(dataRowDetail(46)) = 0 Then
                result(2) = "Row : " & i & " - jmlprt can't be empty" : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idprdetail~idpr~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~hargajual~stokterakhir~supplier~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~idsqdetail~jmlcs~statuscs~jmlrq~statusrq~jmlpo~statuspo~jmlipc~statusipc~jmlgrn~statusgrn~jmlri~statusri~jmldnr~statusdnr~jmlprt~statusprt~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~subtotal", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56) & "~" & dataRowDetail(57) & "~" & ((dataRowDetail(5) * dataRowDetail(12)) - dataRowDetail(14)) * dataRowDetail(11)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , idsqdetail(31) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : idsqdetail = dataRowDetail(31)
            ''proyek(28) As String
            'proyek = dataRowDetail(28)

            'VALIDASI OUTSTANDING -------------------------
            If idsqdetail <> 0 Then
                '1. CEK DATA EXIST ------------------------
                ftExistOutstanding = IIf(Len(ftExistOutstanding.ToString) = 0, "", ftExistOutstanding & " UNION ")
                ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m5_sq_detail JOIN m5_sq ON idsq = sqid WHERE idsqdetail = '" & idsqdetail & "' AND (sqstatus = 2 OR sqstatus = 3 OR sqstatus = 4 OR sqstatus = 7) LIMIT 1) as rowExists, '" & idsqdetail & "' as idsqdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsqdetail=" & idsqdetail)
                ftOutstanding = IIf(Len(ftOutstanding.ToString) = 0, "", ftOutstanding & " OR ")
                ftOutstanding = String.Concat(ftOutstanding, " (sqd.idsqdetail = " & idsqdetail & " AND " & Outstanding & " > (sqd.jmlbarang - sqd.jmlpr)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilai = String.Concat("WHEN '" & idsqdetail & "' THEN ROUND(jmlpr + " & Outstanding & ", 5) ", updNilai)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                updFilter = String.Concat(updFilter, "(idsqdetail = '" & idsqdetail & "')")
            End If

            'If proyek <> "" Then
            '    '1. CEK DATA EXIST ------------------------
            '    ftExistOutstandingPro = IIf(Len(ftExistOutstandingPro.ToString) = 0, "", ftExistOutstandingPro & " UNION ")
            '    ftExistOutstandingPro = String.Concat(ftExistOutstandingPro, "SELECT EXISTS(SELECT 1 FROM m1_project WHERE pkode = '" & proyek & "' LIMIT 1) as rowExists, '" & proyek & "' as proyek")

            '    '2. CEK JML OUTSTANDING -------------------
            '    Dim Outstanding As Double = AsDataTableDSum(dtdetail, "subtotal", "proyek = '" & proyek & "'")
            '    ftOutstandingPro = IIf(Len(ftOutstandingPro.ToString) = 0, "", ftOutstandingPro & " OR ")
            '    ftOutstandingPro = String.Concat(ftOutstandingPro, " (pkode = '" & proyek & "' AND " & Outstanding & " > (pnilaikontrak - prealisasikontrak)) ")

            '    '3. SET NILAI UPDATE OUTSTANDING ----------
            '    updNilaiPro = String.Concat("WHEN '" & proyek & "' THEN ROUND(prealisasikontrak + " & Outstanding & ", 5) ", updNilaiPro)

            '    '4. SET FILTER UPDATE OUTSTANDING ---------
            '    updFilterPro = IIf(Len(updFilterPro.ToString) = 0, "", updFilterPro & " OR ")
            '    updFilterPro = String.Concat(updFilterPro, "(pkode = '" & proyek & "')")
            'End If
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'MAPPING BUAT WS DATA TRANS -------------------------------------------------------
        'idprtrans(0) As Integer, idpr(1) As Integer, sumber(2) As String, idtransaksi(3) As Integer, catatan(4) As String, 
        'urutan(5) As Integer, isclose(6) As Integer, customtext1(7) As String, customtext2(8) As String, customtext3(9) As String, 
        'customtext4(10) As String, customtext5(11) As String, customdbl1(12) As Double, customdbl2(13) As Double, customdbl3(14) As Double, 
        'customdbl4(15) As Double, customdbl5(16) As Double, customdate1(17) As Date, customdate2(18) As Date, customdate3(19) As Date, 
        'customdate4(20) As Date, customdate5(21) As Date

        'MAPPING BUAT FLEX DATA TRANS -----------------------------------------------------
        'idprtrans, idpr, sumber, idtransaksi, catatan, urutan, isclose, 
        'customtext1, customtext2, customtext3, customtext4, customtext5, customdbl1, customdbl2, 
        'customdbl3, customdbl4, customdbl5, customdate1, customdate2, customdate3, customdate4, 
        'customdate5

        'Buat datatable trans
        Dim dttrans As New DataTable
        AsDataTableTambahField(dttrans, "idprtrans", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "idpr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dttrans, "sumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "idtransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dttrans, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dttrans, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dttrans, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customdbl4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customdbl5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customdate4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customdate5", AsEnumTypeData.AsString)

        'CEK PARAMETER DATA TRANS
        If dataSplit.Length > 2 Then
            If dataSplit(2).Length > 0 Then

                'VALIDASI DAN SET DATA TRANS ======================================================
                'SPLIT PARAMETER DATA TRANS
                dataTrans = dataSplit(2).Split(sptRow)
                'END OF VALIDASI DAN SET DATA TRANS ===============================================

                'VALIDASI DAN SET DATA ROW TRANS ==================================================
                Dim JmlDtTrans As Integer = dataTrans.Length
                For i = 1 To JmlDtTrans
                    'SPLIT DATA TRANS
                    dataRowTrans = dataTrans(i - 1).Split(sptField)

                    'VALIDASI DAN SET ROW DATA TRANS -----------------------------------
                    'CEK ARRAY DATA TRANS
                    If (dataRowTrans.Length <> 22) Then
                        result(2) = "Trans Row : " & i & " - Invalid trans transaction data parameter." : GoTo selesai
                    End If
                    'END OF VALIDASI DAN SET DATA ROW TRANS ----------------------------

                    'VALIDASI TIPE DATA TRANS ------------------------------------------
                    'urutan(5) As Integer
                    If (IsNumeric(dataRowTrans(5)) = False) Then
                        result(2) = "Trans Row : " & i & "urutan required numeric." : GoTo selesai
                    End If
                    'isclose(6) As Integer
                    If (IsNumeric(dataRowTrans(6)) = False) Then
                        result(2) = "Trans Row : " & i & "isclose required numeric." : GoTo selesai
                    End If
                    'customdbl1(12) As Double
                    If (IsNumeric(dataRowTrans(12)) = False) Then
                        result(2) = "Trans Row : " & i & "customdbl1 required numeric." : GoTo selesai
                    End If
                    'customdbl2(13) As Double
                    If (IsNumeric(dataRowTrans(13)) = False) Then
                        result(2) = "Trans Row : " & i & "customdbl2 required numeric." : GoTo selesai
                    End If
                    'customdbl3(14) As Double
                    If (IsNumeric(dataRowTrans(14)) = False) Then
                        result(2) = "Trans Row : " & i & "customdbl3 required numeric." : GoTo selesai
                    End If
                    'customdbl4(15) As Double
                    If (IsNumeric(dataRowTrans(15)) = False) Then
                        result(2) = "Trans Row : " & i & "customdbl4 required numeric." : GoTo selesai
                    End If
                    'customdbl5(16) As Double
                    If (IsNumeric(dataRowTrans(16)) = False) Then
                        result(2) = "Trans Row : " & i & "customdbl5 required numeric." : GoTo selesai
                    End If
                    'customdate1(17) As Date
                    If (IsDate(dataRowTrans(17)) = False) Then
                        result(2) = "Trans Row : " & i & "customdate1 required date." : GoTo selesai
                    End If
                    'customdate2(18) As Date
                    If (IsDate(dataRowTrans(18)) = False) Then
                        result(2) = "Trans Row : " & i & "customdate2 required date." : GoTo selesai
                    End If
                    'customdate3(19) As Date
                    If (IsDate(dataRowTrans(19)) = False) Then
                        result(2) = "Trans Row : " & i & "customdate3 required date." : GoTo selesai
                    End If
                    'customdate4(20) As Date
                    If (IsDate(dataRowTrans(20)) = False) Then
                        result(2) = "Trans Row : " & i & "customdate4 required date." : GoTo selesai
                    End If
                    'customdate5(21) As Date
                    If (IsDate(dataRowTrans(21)) = False) Then
                        result(2) = "Trans Row : " & i & "customdate5 required date." : GoTo selesai
                    End If
                    'END OF VALIDASI TIPE DATA TRANS -----------------------------------

                    'VALIDASI DATA TRANS ---------------------------------------
                    'idprtrans(0) As Integer
                    If Len(dataRowTrans(0)) = 0 Then
                        result(2) = "Trans Row : " & i & " - idprtrans can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowTrans(0)) > 20 Then
                        result(2) = "Trans Row : " & i & " - idprtrans should not be more than 20 character." : GoTo selesai
                    End If

                    'idpr(1) As Integer
                    If Len(dataRowTrans(1)) = 0 Then
                        result(2) = "Trans Row : " & i & " - idpr can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowTrans(1)) > 20 Then
                        result(2) = "Trans Row : " & i & " - idpr should not be more than 20 character." : GoTo selesai
                    End If

                    'sumber(2) As String
                    If Len(dataRowTrans(2)) = 0 Then
                        result(2) = "Trans Row : " & i & " - sumber can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowTrans(2)) > 10 Then
                        result(2) = "Trans Row : " & i & " - sumber should not be more than 10 character." : GoTo selesai
                    End If

                    'idtransaksi(3) As Integer
                    If Len(dataRowTrans(3)) = 0 Then
                        result(2) = "Trans Row : " & i & " - idtransaksi can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowTrans(3)) > 20 Then
                        result(2) = "Trans Row : " & i & " - idtransaksi should not be more than 20 character." : GoTo selesai
                    End If

                    'customdbl1(12) As Double
                    If Len(dataRowTrans(12)) = 0 Then
                        result(2) = "Trans Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
                    End If

                    'customdbl2(13) As Double
                    If Len(dataRowTrans(13)) = 0 Then
                        result(2) = "Trans Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
                    End If

                    'customdbl3(14) As Double
                    If Len(dataRowTrans(14)) = 0 Then
                        result(2) = "Trans Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
                    End If

                    'customdbl4(15) As Double
                    If Len(dataRowTrans(15)) = 0 Then
                        result(2) = "Trans Row : " & i & " - customdbl4 can't be empty" : GoTo selesai
                    End If

                    'customdbl5(16) As Double
                    If Len(dataRowTrans(16)) = 0 Then
                        result(2) = "Trans Row : " & i & " - customdbl5 can't be empty" : GoTo selesai
                    End If

                    'customdate1(17) As Date
                    If Len(dataRowTrans(17)) = 0 Then
                        result(2) = "Trans Row : " & i & " - customdate1 can't be empty" : GoTo selesai
                    End If

                    'customdate2(18) As Date
                    If Len(dataRowTrans(18)) = 0 Then
                        result(2) = "Trans Row : " & i & " - customdate2 can't be empty" : GoTo selesai
                    End If

                    'customdate3(19) As Date
                    If Len(dataRowTrans(19)) = 0 Then
                        result(2) = "Trans Row : " & i & " - customdate3 can't be empty" : GoTo selesai
                    End If

                    'customdate4(20) As Date
                    If Len(dataRowTrans(20)) = 0 Then
                        result(2) = "Trans Row : " & i & " - customdate4 can't be empty" : GoTo selesai
                    End If

                    'customdate5(21) As Date
                    If Len(dataRowTrans(21)) = 0 Then
                        result(2) = "Trans Row : " & i & " - customdate5 can't be empty" : GoTo selesai
                    End If
                    'END OF VALIDASI DATA TRANS --------------------------------

                    If AsDataTableTambahData(dttrans, "idprtrans~idpr~sumber~idtransaksi~catatan~urutan~isclose~customtext1~customtext2~customtext3~customtext4~customtext5~customdbl1~customdbl2~customdbl3~customdbl4~customdbl5~customdate1~customdate2~customdate3~customdate4~customdate5", dataRowTrans(0) & "~" & dataRowTrans(1) & "~" & dataRowTrans(2) & "~" & dataRowTrans(3) & "~" & dataRowTrans(4) & "~" & dataRowTrans(5) & "~" & dataRowTrans(6) & "~" & dataRowTrans(7) & "~" & dataRowTrans(8) & "~" & dataRowTrans(9) & "~" & dataRowTrans(10) & "~" & dataRowTrans(11) & "~" & dataRowTrans(12) & "~" & dataRowTrans(13) & "~" & dataRowTrans(14) & "~" & dataRowTrans(15) & "~" & dataRowTrans(16) & "~" & dataRowTrans(17) & "~" & dataRowTrans(18) & "~" & dataRowTrans(19) & "~" & dataRowTrans(20) & "~" & dataRowTrans(21)) = False Then
                        result(2) = "Trans Row : " & i & " - insert into datatable failed." : GoTo selesai
                    End If

                Next
                'END OF VALIDASI DAN SET ROW DATA TRANS ===========================================

            End If
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
                Dim vModuleId As Integer = 4, vMenuId As Integer = 3
                Select Case drutama("prstatus")
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


                ''CEK PERIODE AKUNTANSI ==================================
                'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("prtgl")), AsFormatTanggal(drutama("prtgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("prstatus") = 2 Or drutama("prstatus") = 1 Or drutama("prstatus") = 8 Or drutama("prstatus") = 9 Or drutama("prstatus") = 10 Or drutama("prstatus") = 11 Then
                    'Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstanding, ftOutstanding, ftExistOutstandingPro, ftOutstandingPro)
                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstanding, ftOutstanding, "", "")
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("prtermin").ToString, AsFormatTanggal(drutama("prtgl")), "prtgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("prtgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                'AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                'dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                ''TOTAL = subtotal
                'drutama("prtotal") = AsDataTableDSum(dtdetail, "subtotal")

                ''TOTALPAJAK1 = jmlpajak1
                'drutama("prtotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                ''TOTALPAJAK2 = jmlpajak2
                'drutama("prtotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                ''JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                ''JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'If Integer.Parse(drutama("prhargatermasukpajak")) = 0 Then
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                '    drutama("prtotaltransaksi") = Double.Parse(drutama("prtotal")) - Double.Parse(drutama("prjmldiskon")) + Double.Parse(drutama("prtotalpajak1detail")) + Double.Parse(drutama("prtotalpajak2detail")) + Double.Parse(drutama("prbiayalain"))

                'Else
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                '    drutama("prtotaltransaksi") = Double.Parse(drutama("prtotal")) - Double.Parse(drutama("prjmldiskon")) + Double.Parse(drutama("prtotalpajak2detail")) + Double.Parse(drutama("prbiayalain"))

                'End If
                ''END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("prid")
                    notransaksi = drutama("prnotransaksi")

                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(prid), prnotransaksi FROM M4_pr WHERE prid='" & result(4) & "' AND prstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("prautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("prcabang"), drutama("prlokasi"), drutama("prsumber"), drutama("prtgl"), drutama("prsumber"), 4)
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(prid) FROM m4_pr WHERE prnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_pr_history
                        Dim rsSimpanHistory As String = SimpanHistory.M4_Pr_HistorySimpan("" & paramSplit(0) & "★M4_Pr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("prsumber")) & "▼" & FixQuotes(drutama("prid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M4_Pr set prcabang  = '" & FixQuotes(drutama("prcabang")) & "', prlokasi  = '" & FixQuotes(drutama("prlokasi")) & "', prgudang  = '" & FixQuotes(drutama("prgudang")) & "', prasalbarang  = '" & FixQuotes(drutama("prasalbarang")) & "', prasalbarangkategori  = " & drutama("prasalbarangkategori") & ", prjenispembelian  = '" & FixQuotes(drutama("prjenispembelian")) & "', prjenispembeliankategori  = " & drutama("prjenispembeliankategori") & ", prcarabayar  = " & drutama("prcarabayar") & ", prsumber  = '" & FixQuotes(drutama("prsumber")) & "', prautonotransaksi  = " & drutama("prautonotransaksi") & ", prnotransaksi  = '" & notransaksi & "', prtgl  = '" & FixQuotes(AsFormatTanggal(drutama("prtgl"))) & "', prkodepa  = " & drutama("prkodepa") & ", prdimintaoleh  = " & drutama("prdimintaoleh") & ", prdimintaolehkontak  = '" & FixQuotes(drutama("prdimintaolehkontak")) & "', prmintake  = " & drutama("prmintake") & ", prtgldipakai  = '" & FixQuotes(AsFormatTanggal(drutama("prtgldipakai"))) & "', prtermin  = '" & FixQuotes(drutama("prtermin")) & "', prtgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("prtgljatuhtempo"))) & "', pruraian  = '" & FixQuotes(drutama("pruraian")) & "', prcatatan  = '" & FixQuotes(drutama("prcatatan")) & "', prnoref  = '" & FixQuotes(drutama("prnoref")) & "', prtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("prtglnoref"))) & "', prtglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("prtglpenutupan"))) & "', prmatauang  = '" & FixQuotes(drutama("prmatauang")) & "', prkurs  = '" & FixDouble(drutama("prkurs")) & "', prhargatermasukpajak  = " & drutama("prhargatermasukpajak") & ", prtotal  = '" & FixDouble(drutama("prtotal")) & "', prdiskonpersen  = '" & FixQuotes(drutama("prdiskonpersen")) & "', prjmldiskon  = '" & FixDouble(drutama("prjmldiskon")) & "', prtotalpajak1detail  = '" & FixDouble(drutama("prtotalpajak1detail")) & "', prtotalpajak2detail  = '" & FixDouble(drutama("prtotalpajak2detail")) & "', prbiayalainpersen  = '" & FixQuotes(drutama("prbiayalainpersen")) & "', prbiayalain  = '" & FixDouble(drutama("prbiayalain")) & "', prtotaltransaksi  = '" & FixDouble(drutama("prtotaltransaksi")) & "', pridsq  = " & drutama("pridsq") & ", prstatuscs  = " & drutama("prstatuscs") & ", prstatusrq  = " & drutama("prstatusrq") & ", prstatuspo  = " & drutama("prstatuspo") & ", prstatusipc  = " & drutama("prstatusipc") & ", prstatusgrn  = " & drutama("prstatusgrn") & ", prstatusri  = " & drutama("prstatusri") & ", prstatusdnr  = " & drutama("prstatusdnr") & ", prstatusprt  = " & drutama("prstatusprt") & ", prstatus  = " & drutama("prstatus") & ", prstatussebelumnya  = " & drutama("prstatussebelumnya") & ", prjmlrevisi  = prjmlrevisi+1, prcetakanke  = " & drutama("prcetakanke") & ",  prmodifikasiuser  = " & drutama("prmodifikasiuser") & ", prmodifikasitgl  = NOW(), prcustomtext1  = '" & FixQuotes(drutama("prcustomtext1")) & "', prcustomtext2  = '" & FixQuotes(drutama("prcustomtext2")) & "', prcustomtext3  = '" & FixQuotes(drutama("prcustomtext3")) & "', prcustomtext4  = '" & FixQuotes(drutama("prcustomtext4")) & "', prcustomtext5  = '" & FixQuotes(drutama("prcustomtext5")) & "', prcustomint1  = " & drutama("prcustomint1") & ", prcustomint2  = " & drutama("prcustomint2") & ", prcustomint3  = " & drutama("prcustomint3") & ", prcustomdbl1  = '" & FixDouble(drutama("prcustomdbl1")) & "', prcustomdbl2  = '" & FixDouble(drutama("prcustomdbl2")) & "', prcustomdbl3  = '" & FixDouble(drutama("prcustomdbl3")) & "', prcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("prcustomdate1"))) & "', prcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("prcustomdate2"))) & "', prcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("prcustomdate3"))) & "', prtglawal  = '" & FixQuotes(AsFormatTanggal(drutama("prtglawal"), formatTglWaktu)) & "', prtglakhir  = '" & FixQuotes(AsFormatTanggal(drutama("prtglakhir"), formatTglWaktu)) & "' where prid = '" & drutama("prid") & "'"
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

                    If drutama("prautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("prcabang"), drutama("prlokasi"), drutama("prsumber"), drutama("prtgl"), drutama("prsumber"), 4)
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
                        notransaksi = drutama("prnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(prid) FROM m4_pr WHERE prnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Pr (prcabang, prlokasi, prgudang, prasalbarang, prasalbarangkategori, prjenispembelian, prjenispembeliankategori, prcarabayar, prsumber, prautonotransaksi, prnotransaksi, prtgl, prkodepa, prdimintaoleh, prdimintaolehkontak, prmintake, prtgldipakai, prtermin, prtgljatuhtempo, pruraian, prcatatan, prnoref, prtglnoref, prtglpenutupan, prmatauang, prkurs, prhargatermasukpajak, prtotal, prdiskonpersen, prjmldiskon, prtotalpajak1detail, prtotalpajak2detail, prbiayalainpersen, prbiayalain, prtotaltransaksi, pridsq, prstatuscs, prstatusrq, prstatuspo, prstatusipc, prstatusgrn, prstatusri, prstatusdnr, prstatusprt, prstatus, prstatussebelumnya, prjmlrevisi, prcetakanke, prinputuser, prinputtgl, prmodifikasiuser, prmodifikasitgl, prisclose, prcustomtext1, prcustomtext2, prcustomtext3, prcustomtext4, prcustomtext5, prcustomint1, prcustomint2, prcustomint3, prcustomdbl1, prcustomdbl2, prcustomdbl3, prcustomdate1, prcustomdate2, prcustomdate3, prtglawal, prtglakhir) values('" & FixQuotes(drutama("prcabang")) & "', '" & FixQuotes(drutama("prlokasi")) & "', '" & FixQuotes(drutama("prgudang")) & "', '" & FixQuotes(drutama("prasalbarang")) & "', " & drutama("prasalbarangkategori") & ", '" & FixQuotes(drutama("prjenispembelian")) & "', " & drutama("prjenispembeliankategori") & ", " & drutama("prcarabayar") & ", '" & FixQuotes(drutama("prsumber")) & "', " & drutama("prautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("prtgl"))) & "', " & drutama("prkodepa") & ", " & drutama("prdimintaoleh") & ", '" & FixQuotes(drutama("prdimintaolehkontak")) & "', " & drutama("prmintake") & ", '" & FixQuotes(AsFormatTanggal(drutama("prtgldipakai"))) & "', '" & FixQuotes(drutama("prtermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("prtgljatuhtempo"))) & "', '" & FixQuotes(drutama("pruraian")) & "', '" & FixQuotes(drutama("prcatatan")) & "', '" & FixQuotes(drutama("prnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("prtglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("prtglpenutupan"))) & "', '" & FixQuotes(drutama("prmatauang")) & "', '" & FixDouble(drutama("prkurs")) & "', " & drutama("prhargatermasukpajak") & ", '" & FixDouble(drutama("prtotal")) & "', '" & FixQuotes(drutama("prdiskonpersen")) & "', '" & FixDouble(drutama("prjmldiskon")) & "', '" & FixDouble(drutama("prtotalpajak1detail")) & "', '" & FixDouble(drutama("prtotalpajak2detail")) & "', '" & FixQuotes(drutama("prbiayalainpersen")) & "', '" & FixDouble(drutama("prbiayalain")) & "', '" & FixDouble(drutama("prtotaltransaksi")) & "', " & drutama("pridsq") & ", " & drutama("prstatuscs") & ", " & drutama("prstatusrq") & ", " & drutama("prstatuspo") & ", " & drutama("prstatusipc") & ", " & drutama("prstatusgrn") & ", " & drutama("prstatusri") & ", " & drutama("prstatusdnr") & ", " & drutama("prstatusprt") & ", " & drutama("prstatus") & ", " & drutama("prstatussebelumnya") & ", " & drutama("prjmlrevisi") & ", " & drutama("prcetakanke") & ", " & drutama("prinputuser") & ", NOW(), " & drutama("prmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("prisclose") & ", '" & FixQuotes(drutama("prcustomtext1")) & "', '" & FixQuotes(drutama("prcustomtext2")) & "', '" & FixQuotes(drutama("prcustomtext3")) & "', '" & FixQuotes(drutama("prcustomtext4")) & "', '" & FixQuotes(drutama("prcustomtext5")) & "', " & drutama("prcustomint1") & ", " & drutama("prcustomint2") & ", " & drutama("prcustomint3") & ", '" & FixDouble(drutama("prcustomdbl1")) & "', '" & FixDouble(drutama("prcustomdbl2")) & "', '" & FixDouble(drutama("prcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("prcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("prcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("prcustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("prtglawal"), formatTglWaktu)) & "', '" & FixQuotes(AsFormatTanggal(drutama("prtglakhir"), formatTglWaktu)) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select prid from M4_pr where prnotransaksi='" & notransaksi & "' AND prinputuser= '" & userid & "' order by prmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Pr_Detail where idpr = '" & result(4) & "'"
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
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idprdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixDouble(dr1("hargajual")) & "', '" & FixDouble(dr1("stokterakhir")) & "', " & dr1("supplier") & ", '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idsqdetail") & ", '" & FixDouble(dr1("jmlcs")) & "', " & dr1("statuscs") & ", '" & FixDouble(dr1("jmlrq")) & "', " & dr1("statusrq") & ", '" & FixDouble(dr1("jmlpo")) & "', " & dr1("statuspo") & ", '" & FixDouble(dr1("jmlipc")) & "', " & dr1("statusipc") & ", '" & FixDouble(dr1("jmlgrn")) & "', " & dr1("statusgrn") & ", '" & FixDouble(dr1("jmlri")) & "', " & dr1("statusri") & ", '" & FixDouble(dr1("jmldnr")) & "', " & dr1("statusdnr") & ", '" & FixDouble(dr1("jmlprt")) & "', " & dr1("statusprt") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Pr_Detail(idprdetail, idpr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, hargajual, stokterakhir, supplier, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, jmlcs, statuscs, jmlrq, statusrq, jmlpo, statuspo, jmlipc, statusipc, jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                'Hapus trans ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Pr_Trans where idpr = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses trans
                If (dttrans.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dttrans.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("idprtrans")) & "', " & result(4) & ", '" & FixQuotes(dr1("sumber")) & "', '" & FixQuotes(dr1("idtransaksi")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixDouble(dr1("customdbl4")) & "', '" & FixDouble(dr1("customdbl5")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate4"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate5"))) & "')")
                    Next
                    sql = "Insert into M4_Pr_Trans(idprtrans, idpr, sumber, idtransaksi, catatan, urutan, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdate1, customdate2, customdate3, customdate4, customdate5) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                If drutama("prstatus") = 2 Then
                    If Len(updNilai) > 0 Then
                        'UPDATE OUTSTANDING TRANSAKSI =======================================================
                        'UPDATE DETAIL
                        sql = "UPDATE m5_sq_detail SET jmlpr = (CASE idsqdetail " & updNilai & " ELSE jmlpr END) WHERE " & updFilter
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idsq FROM m5_sq_detail WHERE " & updFilter & " GROUP BY idsq", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idsq = '" & dr1("idsq") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idsq, SUM(jmlbarang) as jmlbarang, SUM(jmlpr) as jmlpr FROM m5_sq_detail WHERE " & ftDetail & " GROUP BY idsq", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilai = "" : updFilter = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlpr") >= dr1("jmlbarang") Then
                                    statusOut = 2
                                ElseIf dr1("jmlpr") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilai = String.Concat(updNilai, "WHEN '" & dr1("idsq") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                                updFilter = String.Concat(updFilter, "(sqid = '" & dr1("idsq") & "')")
                            Next

                            sql = "UPDATE m5_sq SET sqstatuspr = (CASE sqid " & updNilai & " ELSE sqstatuspr END) WHERE " & updFilter
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                        'END OF UPDATE OUTSTANDING TRANSAKSI ================================================
                    End If

                    'If Len(updNilaiPro) > 0 Then
                    '    'UPDATE OUTSTANDING TRANSAKSI =======================================================
                    '    'UPDATE DETAIL
                    '    sql = "UPDATE m1_project SET prealisasikontrak = (CASE pkode " & updNilaiPro & " ELSE prealisasikontrak END) WHERE " & updFilterPro
                    '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    '    With objCmd
                    '        .Connection = myConn
                    '        .Transaction = Trans
                    '        .CommandType = CommandType.Text
                    '        .CommandText = sql
                    '    End With
                    '    objCmd.ExecuteNonQuery()
                    '    'END OF UPDATE OUTSTANDING TRANSAKSI ================================================
                    'End If

                End If

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "Pr", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M4_PrUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("prdimintaolehkode", "c1.kkode")
            Filter = Filter.Replace("prdimintaolehnama", "c1.knama")
            Filter = Filter.Replace("prmintakekode", "c2.kkode")
            Filter = Filter.Replace("prmintakenama", "c2.knama")
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
            Dim sumber As String = "Pr", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Prtgl, Prnotransaksi, Prstatus FROM m4_Pr WHERE Prid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Prstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m4_pr_history
            Dim rsSimpanHistory As String = SimpanHistory.M4_Pr_HistorySimpan("" & paramSplit(0) & "★M4_Pr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m4_pr_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idsqdetail As Integer = 0
                Dim ftOutstanding As String = "", updNilai As String = "", updFilter As String = ""
                'Dim ftOutstandingPro As String = "", updNilaiPro As String = "", updFilterPro As String = "", proyek As String = ""

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idsqdetail, urutan, jml, harga, jmldiskon, kurs, proyek, ((jml * harga) - jmldiskon) * kurs as subtotal FROM m4_pr_detail WHERE idpr = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : idsqdetail = dr1("idsqdetail")
                        'proyek = dr1("proyek")

                        'UPDATE OUTSTANDING ---------------------------
                        If idsqdetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING ----------
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsqdetail=" & idsqdetail)
                            updNilai = String.Concat("WHEN '" & idsqdetail & "' THEN ROUND(jmlpr - '" & Outstanding & "', 5) ", updNilai)
                            '2. SET FILTERUPDATE OUTSTANDING ----------
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(idsqdetail = '" & idsqdetail & "')")
                        End If

                        'If proyek <> "" Then
                        '    '1. SET NILAI UPDATE OUTSTANDING ----------
                        '    Dim Outstanding As Double = AsDataTableDSum(dtdetail, "subtotal", "proyek='" & proyek & "'")
                        '    updNilaiPro = String.Concat("WHEN '" & proyek & "' THEN ROUND(prealisasikontrak - " & Outstanding & ", 5) ", updNilaipro)
                        '    '2. SET FILTERUPDATE OUTSTANDING ----------
                        '    updFilterPro = IIf(Len(updFilterPro.ToString) = 0, "", updFilterPro & " OR ")
                        '    updFilterPro = String.Concat(updFilterPro, "(pkode = '" & proyek & "')")
                        'End If
                        'END OF BUAT FILTER UNTUK UPDATE --------------------------
                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                If Len(updFilter) > 0 Then
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m5_sq_detail SET jmlpr = (CASE idsqdetail " & updNilai & " ELSE jmlpr END) WHERE " & updFilter
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idsq FROM m5_sq_detail WHERE " & updFilter & " GROUP BY idsq", myConn)
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idsq = '" & dr1("idsq") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDBCon("SELECT idsq, SUM(jmlbarang) as jmlbarang, SUM(jmlpr) as jmlpr FROM m5_sq_detail WHERE " & ftDetail & " GROUP BY idsq", myConn)
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilai = "" : updFilter = ""
                        For Each dr1 As DataRow In dtOut.Rows
                            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                            If dr1("jmlpr") >= dr1("jmlbarang") Then
                                statusOut = 2
                            ElseIf dr1("jmlpr") < 1 Then
                                statusOut = 0
                            Else
                                statusOut = 1
                            End If
                            '2. SET NILAI UPDATE OUTSTANDING
                            updNilai = String.Concat(updNilai, "WHEN '" & dr1("idsq") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(sqid = '" & dr1("idsq") & "')")
                        Next

                        sql = "UPDATE m5_sq SET sqstatuspr = (CASE sqid " & updNilai & " ELSE sqstatuspr END) WHERE " & updFilter
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

                'If Len(updFilterPro) > 0 Then
                '    'UPDATE OUTSTANDING DETAIL ----------------------
                '    sql = "UPDATE m1_project SET prealisasikontrak = (CASE pkode " & updNilaiPro & " ELSE prealisasikontrak END) WHERE " & updFilterPro
                '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '    With objCmd
                '        .Connection = myConn
                '        .Transaction = Trans
                '        .CommandType = CommandType.Text
                '        .CommandText = sql
                '    End With
                '    objCmd.ExecuteNonQuery()
                '    'END OF UPDATE OUTSTANDING DETAIL ---------------
                'End If

            End If

            'update status utama
            sql = "UPDATE m4_Pr SET Prstatus = " & nilaiStatus & ", Prmodifikasiuser='" & userid & "', Prmodifikasitgl = NOW(), Prposting = 0, Prpostingtgl = '1971-01-01 00:00:00', Prjmlrevisi = Prjmlrevisi + 1 WHERE Prid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_PrSearch(PostWsSearch(paramSplit(0), "M4_PrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_PrDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("prdimintaolehkode", "c1.kkode")
            Filter = Filter.Replace("prdimintaolehnama", "c1.knama")
            Filter = Filter.Replace("prmintakekode", "c2.kkode")
            Filter = Filter.Replace("prmintakenama", "c2.knama")
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
            Dim sumber As String = "Pr", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Prid, Prnotransaksi FROM m4_Pr WHERE Prid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT prcabang, prlokasi, prsumber, prautonotransaksi, prnotransaksi, prtgl"
            sql &= " FROM M4_pr"
            sql &= " WHERE prid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("prcabang")
                lokasi = dtNomorNext.Rows(0)("prlokasi")
                sumber = dtNomorNext.Rows(0)("prsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("prautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("prnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("prtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE TRANS
            sql = "DELETE FROM M4_Pr_Trans WHERE idpr ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M4_Pr_Detail WHERE idpr ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M4_Pr WHERE prid ='" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_PrSearch(PostWsSearch(paramSplit(0), "M4_PrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_PrGetdataById(ByVal param As String) As String

        'M4_PrGetdataById Utama --------------------------------------------------------
        'prid, prcabang, prlokasi, prgudang, prasalbarang, prasalbarangkategori, prjenispembelian, 
        'prjenispembeliankategori, prcarabayar, prsumber, prautonotransaksi, prnotransaksi, prtgl, prkodepa, 
        'prdimintaoleh, prdimintaolehkontak, prmintake, prtgldipakai, prtermin, prtgljatuhtempo, pruraian, 
        'prcatatan, prnoref, prtglnoref, prtglpenutupan, prmatauang, prkurs, prhargatermasukpajak, 
        'prtotal, prdiskonpersen, prjmldiskon, prtotalpajak1detail, prtotalpajak2detail, prbiayalainpersen, prbiayalain, 
        'prtotaltransaksi, pridsq, prstatuscs, prstatusrq, prstatuspo, prstatusipc, prstatusgrn, 
        'prstatusri, prstatusdnr, prstatusprt, prstatusrealisasi, prstatus, prstatussebelumnya, prjmlrevisi, 
        'prcetakanke, prinputuser, prinputtgl, prmodifikasiuser, prmodifikasitgl, prposting, prpostingtgl, 
        'prisclose, prcustomtext1, prcustomtext2, prcustomtext3, prcustomtext4, prcustomtext5, prcustomint1, 
        'prcustomint2, prcustomint3, prcustomdbl1, prcustomdbl2, prcustomdbl3, prcustomdate1, prcustomdate2, 
        'prcustomdate3, prcabangnama, prlokasinama, prgudangnama, prdimintaolehkode, prdimintaolehnama, prmintakekode, 
        'prmintakenama, prterminnama, prterminharijatuhtempo, prnotransaksisq, prstatusnama, prstatussebelumnyanama, prinputusernama, 
        'prmodifikasiusernama, prtglawal, prtglakhir, enama

        'M4_PrGetdataById Detail -------------------------------------------------------
        'idprdetail, idpr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, 
        'jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, 
        'pajak1, jmlpajak1, pajak2, jmlpajak2, hargajual, stokterakhir, supplier, 
        'cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, 
        'catatan, urutan, idsqdetail, jmlcs, statuscs, jmlrq, statusrq, 
        'jmlpo, statuspo, jmlipc, statusipc, jmlgrn, statusgrn, jmlri, 
        'statusri, jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, kodebarang, pajak1nama, pajak1nilai, pajak2nama, 
        'pajak2nilai, supplierkode, suppliernama, cabangnama, lokasinama, gudangnama, costcenternama, 
        'divisinama, subdivisinama, proyeknama, sqnotransaksi

        'M4_PrGetdataById Trans -------------------------------------------------------
        'idprtrans, idpr, sumber, idtransaksi, catatan, urutan, isclose, customtext1, 
        'customtext2, customtext3, customtext4, customtext5, customdbl1, customdbl2, customdbl3, 
        'customdbl4, customdbl5, customdate1, customdate2, customdate3, customdate4, customdate5,
        'notransaksi, tgltransaksi, kontak, kontakkode, kontaknama

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

        Dim utama As String = "", detail As String = "", trans As String = "", idtransaksi As String = ""

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
            Filter = "prid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "prid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m4_pr_getdata")
        sql = "select pr.prid AS prid, pr.prcabang AS prcabang, pr.prlokasi AS prlokasi, pr.prgudang AS prgudang, pr.prasalbarang AS prasalbarang, pr.prasalbarangkategori AS prasalbarangkategori, pr.prjenispembelian AS prjenispembelian, pr.prjenispembeliankategori AS prjenispembeliankategori, pr.prcarabayar AS prcarabayar, pr.prsumber AS prsumber, pr.prautonotransaksi AS prautonotransaksi, pr.prnotransaksi AS prnotransaksi, pr.prtgl AS prtgl, pr.prkodepa AS prkodepa, pr.prdimintaoleh AS prdimintaoleh, pr.prdimintaolehkontak AS prdimintaolehkontak, pr.prmintake AS prmintake, pr.prtgldipakai AS prtgldipakai, pr.prtermin AS prtermin, pr.prtgljatuhtempo AS prtgljatuhtempo, pr.pruraian AS pruraian, pr.prcatatan AS prcatatan, pr.prnoref AS prnoref, pr.prtglnoref AS prtglnoref, pr.prtglpenutupan AS prtglpenutupan, pr.prmatauang AS prmatauang, pr.prkurs AS prkurs, pr.prhargatermasukpajak AS prhargatermasukpajak, pr.prtotal AS prtotal, pr.prdiskonpersen AS prdiskonpersen, pr.prjmldiskon AS prjmldiskon, pr.prtotalpajak1detail AS prtotalpajak1detail, pr.prtotalpajak2detail AS prtotalpajak2detail, pr.prbiayalainpersen AS prbiayalainpersen, pr.prbiayalain AS prbiayalain, pr.prtotaltransaksi AS prtotaltransaksi, pr.pridsq AS pridsq, pr.prstatuscs AS prstatuscs, pr.prstatusrq AS prstatusrq, pr.prstatuspo AS prstatuspo, pr.prstatusipc AS prstatusipc, pr.prstatusgrn AS prstatusgrn, pr.prstatusri AS prstatusri, pr.prstatusdnr AS prstatusdnr, pr.prstatusprt AS prstatusprt, pr.prstatusrealisasi AS prstatusrealisasi, pr.prstatus AS prstatus, pr.prstatussebelumnya AS prstatussebelumnya, pr.prjmlrevisi AS prjmlrevisi, pr.prcetakanke AS prcetakanke, pr.prinputuser AS prinputuser, pr.prinputtgl AS prinputtgl, pr.prmodifikasiuser AS prmodifikasiuser, pr.prmodifikasitgl AS prmodifikasitgl, pr.prposting AS prposting, pr.prpostingtgl AS prpostingtgl, pr.prisclose AS prisclose, pr.prcustomtext1 AS prcustomtext1, pr.prcustomtext2 AS prcustomtext2, pr.prcustomtext3 AS prcustomtext3, pr.prcustomtext4 AS prcustomtext4, pr.prcustomtext5 AS prcustomtext5, pr.prcustomint1 AS prcustomint1, pr.prcustomint2 AS prcustomint2, pr.prcustomint3 AS prcustomint3, pr.prcustomdbl1 AS prcustomdbl1, pr.prcustomdbl2 AS prcustomdbl2, pr.prcustomdbl3 AS prcustomdbl3, pr.prcustomdate1 AS prcustomdate1, pr.prcustomdate2 AS prcustomdate2, pr.prcustomdate3 AS prcustomdate3, br.bnama AS prcabangnama, lc.lnama AS prlokasinama, wh.wnama AS prgudangnama, c1.kkode AS prdimintaolehkode, c1.knama AS prdimintaolehnama, c2.kkode AS prmintakekode, c2.knama AS prmintakenama, tr.trnama AS prterminnama, tr.trharijatuhtempo AS prterminharijatuhtempo, sq.sqnotransaksi AS prnotransaksisq, st1.nama AS prstatusnama, st2.nama AS prstatussebelumnyanama, u1.unama AS prinputusernama, u2.unama AS prmodifikasiusernama, pr.prtglawal, pr.prtglakhir, prd.idprdetail AS idprdetail, prd.idpr AS idpr, prd.idbarang AS idbarang, prd.namabarang AS namabarang, prd.tipebarang AS tipebarang, prd.jml AS jml, prd.satuan AS satuan, prd.nilaisatuan AS nilaisatuan, prd.jmlbarang AS jmlbarang, prd.satuanbarang AS satuanbarang, prd.matauang AS matauang, prd.kurs AS kurs, prd.harga AS harga, prd.diskon AS diskon, prd.jmldiskon AS jmldiskon, prd.pajak1 AS pajak1, prd.jmlpajak1 AS jmlpajak1, prd.pajak2 AS pajak2, prd.jmlpajak2 AS jmlpajak2, prd.hargajual AS hargajual, prd.stokterakhir AS stokterakhir, prd.supplier AS supplier, prd.cabang AS cabang, prd.lokasi AS lokasi, prd.gudang AS gudang, prd.costcenter AS costcenter, prd.divisi AS divisi, prd.subdivisi AS subdivisi, prd.proyek AS proyek, prd.catatan AS catatan, prd.urutan AS urutan, prd.idsqdetail AS idsqdetail, prd.jmlcs AS jmlcs, prd.statuscs AS statuscs, prd.jmlrq AS jmlrq, prd.statusrq AS statusrq, prd.jmlpo AS jmlpo, prd.statuspo AS statuspo, prd.jmlipc AS jmlipc, prd.statusipc AS statusipc, prd.jmlgrn AS jmlgrn, prd.statusgrn AS statusgrn, prd.jmlri AS jmlri, prd.statusri AS statusri, prd.jmldnr AS jmldnr, prd.statusdnr AS statusdnr, prd.jmlprt AS jmlprt, prd.statusprt AS statusprt, prd.jmlrealisasi AS jmlrealisasi, prd.statusrealisasi AS statusrealisasi, prd.isclose AS isclose, prd.customtext1 AS customtext1, prd.customtext2 AS customtext2, prd.customtext3 AS customtext3, prd.customdbl1 AS customdbl1, prd.customdbl2 AS customdbl2, prd.customdbl3 AS customdbl3, prd.customdate1 AS customdate1, prd.customdate2 AS customdate2, prd.customdate3 AS customdate3, i.bkode AS kodebarang, t1.tnama AS pajak1nama, t1.tnilai AS pajak1nilai, t2.tnama AS pajak2nama, t2.tnilai AS pajak2nilai, c3.kkode AS supplierkode, c3.knama AS suppliernama, brd.bnama AS cabangnama, lcd.lnama AS lokasinama, whd.wnama AS gudangnama, cc.ccnama AS costcenternama, d.dnama AS divisinama, sd.sdnama AS subdivisinama, p.pnama AS proyeknama, sq2.sqnotransaksi AS sqnotransaksi, e.enama from m4_pr pr join m4_pr_detail prd on prd.idpr = pr.prid left join m1_branch br on br.bkode = pr.prcabang left join m1_location lc on lc.lkode = pr.prlokasi left join m1_warehouse wh on wh.wkode = pr.prgudang left join m1_contact c1 on c1.kid = pr.prdimintaoleh left join m1_contact c2 on c2.kid = pr.prmintake left join m1_terms tr on pr.prtermin = tr.trkode left join m5_sq sq on pr.pridsq = sq.sqid left join m0_status st1 on st1.kode = pr.prstatus left join m0_status st2 on st2.kode = pr.prstatussebelumnya left join m0_user u1 on u1.userid = pr.prinputuser left join m0_user u2 on u2.userid = pr.prmodifikasiuser left join m1_item i on i.bid = prd.idbarang left join m1_tax t1 on prd.pajak1 = t1.tkode left join m1_tax t2 on prd.pajak2 = t2.tkode left join m1_contact c3 on prd.supplier = c3.kid left join m1_branch brd on prd.cabang = brd.bkode left join m1_location lcd on prd.lokasi = lcd.lkode left join m1_warehouse whd on prd.gudang = whd.wkode left join m1_cost_center cc on prd.costcenter = cc.cckode left join m1_division d on prd.divisi = d.dkode left join m1_subdivision sd on prd.subdivisi = sd.sdkode left join m1_project p on prd.proyek = p.pkode left join m5_sq_detail sqd on prd.idsqdetail = sqd.idsqdetail left join m5_sq sq2 on sqd.idsq = sq2.sqid left join m1_expedition e on pr.prcustomtext5 = e.ekode"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("prid"), 0), sptField,
                     FxDB(drutama("prcabang"), ""), sptField,
                     FxDB(drutama("prlokasi"), ""), sptField,
                     FxDB(drutama("prgudang"), ""), sptField,
                     FxDB(drutama("prasalbarang"), ""), sptField,
                     FxDB(drutama("prasalbarangkategori"), 0), sptField,
                     FxDB(drutama("prjenispembelian"), ""), sptField,
                     FxDB(drutama("prjenispembeliankategori"), 0), sptField,
                     FxDB(drutama("prcarabayar"), 0), sptField,
                     FxDB(drutama("prsumber"), ""), sptField,
                     FxDB(drutama("prautonotransaksi"), 0), sptField,
                     FxDB(drutama("prnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("prtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("prkodepa"), 0), sptField,
                     FxDB(drutama("prdimintaoleh"), 0), sptField,
                     FxDB(drutama("prdimintaolehkontak"), ""), sptField,
                     FxDB(drutama("prmintake"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prtgldipakai"), ""), formatTgl), sptField,
                     FxDB(drutama("prtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("prtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("pruraian"), ""), sptField,
                     FxDB(drutama("prcatatan"), ""), sptField,
                     FxDB(drutama("prnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("prtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("prtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("prmatauang"), ""), sptField,
                     FxDB(drutama("prkurs"), 0), sptField,
                     FxDB(drutama("prhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("prtotal"), 0), sptField,
                     FxDB(drutama("prdiskonpersen"), ""), sptField,
                     FxDB(drutama("prjmldiskon"), 0), sptField,
                     FxDB(drutama("prtotalpajak1detail"), 0), sptField,
                     FxDB(drutama("prtotalpajak2detail"), 0), sptField,
                     FxDB(drutama("prbiayalainpersen"), ""), sptField,
                     FxDB(drutama("prbiayalain"), 0), sptField,
                     FxDB(drutama("prtotaltransaksi"), 0), sptField,
                     FxDB(drutama("pridsq"), 0), sptField,
                     FxDB(drutama("prstatuscs"), 0), sptField,
                     FxDB(drutama("prstatusrq"), 0), sptField,
                     FxDB(drutama("prstatuspo"), 0), sptField,
                     FxDB(drutama("prstatusipc"), 0), sptField,
                     FxDB(drutama("prstatusgrn"), 0), sptField,
                     FxDB(drutama("prstatusri"), 0), sptField,
                     FxDB(drutama("prstatusdnr"), 0), sptField,
                     FxDB(drutama("prstatusprt"), 0), sptField,
                     FxDB(drutama("prstatusrealisasi"), 0), sptField,
                     FxDB(drutama("prstatus"), 0), sptField,
                     FxDB(drutama("prstatussebelumnya"), 0), sptField,
                     FxDB(drutama("prjmlrevisi"), 0), sptField,
                     FxDB(drutama("prcetakanke"), 0), sptField,
                     FxDB(drutama("prinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("prmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("prposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("prisclose"), 0), sptField,
                     FxDB(drutama("prcustomtext1"), ""), sptField,
                     FxDB(drutama("prcustomtext2"), ""), sptField,
                     FxDB(drutama("prcustomtext3"), ""), sptField,
                     FxDB(drutama("prcustomtext4"), ""), sptField,
                     FxDB(drutama("prcustomtext5"), ""), sptField,
                     FxDB(drutama("prcustomint1"), 0), sptField,
                     FxDB(drutama("prcustomint2"), 0), sptField,
                     FxDB(drutama("prcustomint3"), 0), sptField,
                     FxDB(drutama("prcustomdbl1"), 0), sptField,
                     FxDB(drutama("prcustomdbl2"), 0), sptField,
                     FxDB(drutama("prcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("prcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("prcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("prcabangnama"), ""), sptField,
                     FxDB(drutama("prlokasinama"), ""), sptField,
                     FxDB(drutama("prgudangnama"), ""), sptField,
                     FxDB(drutama("prdimintaolehkode"), ""), sptField,
                     FxDB(drutama("prdimintaolehnama"), ""), sptField,
                     FxDB(drutama("prmintakekode"), ""), sptField,
                     FxDB(drutama("prmintakenama"), ""), sptField,
                     FxDB(drutama("prterminnama"), ""), sptField,
                     FxDB(drutama("prterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("prnotransaksisq"), ""), sptField,
                     FxDB(drutama("prstatusnama"), ""), sptField,
                     FxDB(drutama("prstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("prinputusernama"), ""), sptField,
                     FxDB(drutama("prmodifikasiusernama"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("prtglawal"), ""), formatTglWaktu), sptField,
                     AsFormatTanggal(FxDB(drutama("prtglakhir"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("enama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idprdetail"), 0), sptField,
                     FxDB(dr("idpr"), 0), sptField,
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
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("diskon"), ""), sptField,
                     FxDB(dr("jmldiskon"), 0), sptField,
                     FxDB(dr("pajak1"), ""), sptField,
                     FxDB(dr("jmlpajak1"), 0), sptField,
                     FxDB(dr("pajak2"), ""), sptField,
                     FxDB(dr("jmlpajak2"), 0), sptField,
                     FxDB(dr("hargajual"), 0), sptField,
                     FxDB(dr("stokterakhir"), 0), sptField,
                     FxDB(dr("supplier"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idsqdetail"), 0), sptField,
                     FxDB(dr("jmlcs"), 0), sptField,
                     FxDB(dr("statuscs"), 0), sptField,
                     FxDB(dr("jmlrq"), 0), sptField,
                     FxDB(dr("statusrq"), 0), sptField,
                     FxDB(dr("jmlpo"), 0), sptField,
                     FxDB(dr("statuspo"), 0), sptField,
                     FxDB(dr("jmlipc"), 0), sptField,
                     FxDB(dr("statusipc"), 0), sptField,
                     FxDB(dr("jmlgrn"), 0), sptField,
                     FxDB(dr("statusgrn"), 0), sptField,
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
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("supplierkode"), ""), sptField,
                     FxDB(dr("suppliernama"), ""), sptField,
                     FxDB(dr("cabangnama"), ""), sptField,
                     FxDB(dr("lokasinama"), ""), sptField,
                     FxDB(dr("gudangnama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("sqnotransaksi"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA TRANS
            sql = "SELECT prtrans.idprtrans, prtrans.idpr, prtrans.sumber, prtrans.idtransaksi, prtrans.catatan, prtrans.urutan, prtrans.isclose, prtrans.customtext1, prtrans.customtext2, prtrans.customtext3, prtrans.customtext4, prtrans.customtext5, prtrans.customdbl1, prtrans.customdbl2, prtrans.customdbl3, prtrans.customdbl4, prtrans.customdbl5, prtrans.customdate1, prtrans.customdate2, prtrans.customdate3, prtrans.customdate4, prtrans.customdate5,m5do.donotransaksi as notransaksi, m5do.dotgl as tgltransaksi, m5do.docustomer as kontak, c.kkode as kontakkode,  c.knama as kontaknama FROM m4_pr_trans prtrans LEFT JOIN m5_do m5do  ON prtrans.sumber = m5do.dosumber AND prtrans.idtransaksi = m5do.doid LEFT JOIN m1_contact c ON m5do.docustomer = c.kid"
            Dim dttrans As New DataTable
            dttrans = AmbilData("aplikasi1-m1_no_trans_out", "prtrans.idpr = '" & idtransaksi & "'", "prtrans.urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dttrans.Rows
                trans = String.Concat(trans,
                     FxDB(dr("idprtrans"), 0), sptField,
                     FxDB(dr("idpr"), 0), sptField,
                     FxDB(dr("sumber"), ""), sptField,
                     FxDB(dr("idtransaksi"), 0), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customtext4"), ""), sptField,
                     FxDB(dr("customtext5"), ""), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     FxDB(dr("customdbl4"), 0), sptField,
                     FxDB(dr("customdbl5"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), "1900-01-01"), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), "1900-01-01"), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), "1900-01-01"), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate4"), "1900-01-01"), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate5"), "1900-01-01"), formatTgl), sptField,
                     FxDB(dr("notransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgltransaksi"), "1900-01-01"), formatTgl), sptField,
                     FxDB(dr("kontak"), 0), sptField,
                     FxDB(dr("kontakkode"), ""), sptField,
                     FxDB(dr("kontaknama"), ""), sptRow)
            Next
            If trans.Length > 0 Then trans = trans.Substring(0, trans.Length - sptRow.Length) Else trans = trans


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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, trans)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("prid, prcabang, prlokasi, prgudang, prasalbarang, prasalbarangkategori, prjenispembelian, prjenispembeliankategori, prcarabayar, prsumber, prautonotransaksi, prnotransaksi, prtgl, prkodepa, prdimintaoleh, prdimintaolehkontak, prmintake, prtgldipakai, prtermin, prtgljatuhtempo, pruraian, prcatatan, prnoref, prtglnoref, prtglpenutupan, prmatauang, prkurs, prhargatermasukpajak, prtotal, prdiskonpersen, prjmldiskon, prtotalpajak1detail, prtotalpajak2detail, prbiayalainpersen, prbiayalain, prtotaltransaksi, pridsq, prstatuscs, prstatusrq, prstatuspo, prstatusipc, prstatusgrn, prstatusri, prstatusdnr, prstatusprt, prstatusrealisasi, prstatus, prstatussebelumnya, prjmlrevisi, prcetakanke, prinputuser, prinputtgl, prmodifikasiuser, prmodifikasitgl, prposting, prpostingtgl, prisclose, prcustomtext1, prcustomtext2, prcustomtext3, prcustomtext4, prcustomtext5, prcustomint1, prcustomint2, prcustomint3, prcustomdbl1, prcustomdbl2, prcustomdbl3, prcustomdate1, prcustomdate2, prcustomdate3, prcabangnama, prlokasinama, prgudangnama, prdimintaolehkode, prdimintaolehnama, prmintakekode, prmintakenama, prterminnama, prterminharijatuhtempo, prnotransaksisq, prstatusnama, prstatussebelumnyanama, prinputusernama, prmodifikasiusernama, prtglawal, prtglakhir, enama" & sptSubParam & "idprdetail, idpr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, hargajual, stokterakhir, supplier, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, jmlcs, statuscs, jmlrq, statusrq, jmlpo, statuspo, jmlipc, statusipc, jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, supplierkode, suppliernama, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, sqnotransaksi" & sptSubParam & "idprtrans, idpr, sumber, idtransaksi, catatan, urutan, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdate1, customdate2, customdate3, customdate4, customdate5, notransaksi, tgltransaksi, kontak, kontakkode, kontaknama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_PrSearch(ByVal param As String) As String
        'M4_PrSearch --------------------------------------------------------
        'prid, prcabang, prlokasi, prgudang, prasalbarang, prasalbarangkategori, prjenispembelian, 
        'prjenispembeliankategori, prcarabayar, prsumber, prautonotransaksi, prnotransaksi, prtgl, prkodepa, 
        'prdimintaoleh, prdimintaolehkontak, prmintake, prtgldipakai, prtermin, prtgljatuhtempo, pruraian, 
        'prcatatan, prnoref, prtglnoref, prtglpenutupan, prmatauang, prkurs, prhargatermasukpajak, 
        'prtotal, prdiskonpersen, prjmldiskon, prtotalpajak1detail, prtotalpajak2detail, prbiayalainpersen, prbiayalain, 
        'prtotaltransaksi, pridsq, prstatuscs, prstatusrq, prstatuspo, prstatusipc, prstatusgrn, 
        'prstatusri, prstatusdnr, prstatusprt, prstatusrealisasi, prstatus, prstatussebelumnya, prjmlrevisi, 
        'prcetakanke, prinputuser, prinputtgl, prmodifikasiuser, prmodifikasitgl, prposting, prpostingtgl, 
        'prisclose, prcabangnama, prlokasinama, prgudangnama, prdimintaolehkode, prdimintaolehnama, prmintakekode, 
        'prmintakenama, sqnotransaksi, prstatusnama, prstatussebelumnyanama, prinputusernama, prmodifikasiusernama, 
        'prtglawal, prtglakhir, prcustomtext1

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
            Filter = Filter.Replace("prdimintaolehkode", "c1.kkode")
            Filter = Filter.Replace("prdimintaolehnama", "c1.knama")
            Filter = Filter.Replace("prmintakekode", "c2.kkode")
            Filter = Filter.Replace("prmintakenama", "c2.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m4_pr_v")
        sql = "select `pr`.`prid` AS `prid`,`pr`.`prcabang` AS `prcabang`,`pr`.`prlokasi` AS `prlokasi`,`pr`.`prgudang` AS `prgudang`,`pr`.`prasalbarang` AS `prasalbarang`,`pr`.`prasalbarangkategori` AS `prasalbarangkategori`,`pr`.`prjenispembelian` AS `prjenispembelian`,`pr`.`prjenispembeliankategori` AS `prjenispembeliankategori`,`pr`.`prcarabayar` AS `prcarabayar`,`pr`.`prsumber` AS `prsumber`,`pr`.`prautonotransaksi` AS `prautonotransaksi`,`pr`.`prnotransaksi` AS `prnotransaksi`,`pr`.`prtgl` AS `prtgl`,`pr`.`prkodepa` AS `prkodepa`,`pr`.`prdimintaoleh` AS `prdimintaoleh`,`pr`.`prdimintaolehkontak` AS `prdimintaolehkontak`,`pr`.`prmintake` AS `prmintake`,`pr`.`prtgldipakai` AS `prtgldipakai`,`pr`.`prtermin` AS `prtermin`,`pr`.`prtgljatuhtempo` AS `prtgljatuhtempo`,`pr`.`pruraian` AS `pruraian`,`pr`.`prcatatan` AS `prcatatan`,`pr`.`prnoref` AS `prnoref`,`pr`.`prtglnoref` AS `prtglnoref`,`pr`.`prtglpenutupan` AS `prtglpenutupan`,`pr`.`prmatauang` AS `prmatauang`,`pr`.`prkurs` AS `prkurs`,`pr`.`prhargatermasukpajak` AS `prhargatermasukpajak`,`pr`.`prtotal` AS `prtotal`,`pr`.`prdiskonpersen` AS `prdiskonpersen`,`pr`.`prjmldiskon` AS `prjmldiskon`,`pr`.`prtotalpajak1detail` AS `prtotalpajak1detail`,`pr`.`prtotalpajak2detail` AS `prtotalpajak2detail`,`pr`.`prbiayalainpersen` AS `prbiayalainpersen`,`pr`.`prbiayalain` AS `prbiayalain`,`pr`.`prtotaltransaksi` AS `prtotaltransaksi`,`pr`.`pridsq` AS `pridsq`,`pr`.`prstatuscs` AS `prstatuscs`,`pr`.`prstatusrq` AS `prstatusrq`,`pr`.`prstatuspo` AS `prstatuspo`,`pr`.`prstatusipc` AS `prstatusipc`,`pr`.`prstatusgrn` AS `prstatusgrn`,`pr`.`prstatusri` AS `prstatusri`,`pr`.`prstatusdnr` AS `prstatusdnr`,`pr`.`prstatusprt` AS `prstatusprt`,`pr`.`prstatusrealisasi` AS `prstatusrealisasi`,`pr`.`prstatus` AS `prstatus`,`pr`.`prstatussebelumnya` AS `prstatussebelumnya`,`pr`.`prjmlrevisi` AS `prjmlrevisi`,`pr`.`prcetakanke` AS `prcetakanke`,`pr`.`prinputuser` AS `prinputuser`,`pr`.`prinputtgl` AS `prinputtgl`,`pr`.`prmodifikasiuser` AS `prmodifikasiuser`,`pr`.`prmodifikasitgl` AS `prmodifikasitgl`,`pr`.`prposting` AS `prposting`,`pr`.`prpostingtgl` AS `prpostingtgl`,`pr`.`prisclose` AS `prisclose`,`br`.`bnama` AS `prcabangnama`,`lc`.`lnama` AS `prlokasinama`,`wh`.`wnama` AS `prgudangnama`,`c1`.`kkode` AS `prdimintaolehkode`,`c1`.`knama` AS `prdimintaolehnama`,`c2`.`kkode` AS `prmintakekode`,`c2`.`knama` AS `prmintakenama`,`sq`.`sqnotransaksi` AS `sqnotransaksi`,`st1`.`nama` AS `prstatusnama`,`st2`.`nama` AS `prstatussebelumnyanama`,`u1`.`unama` AS `prinputusernama`,`u2`.`unama` AS `prmodifikasiusernama`, pr.prtglawal, pr.prtglakhir, pr.prcustomtext1 from ((((((((((`m4_pr` `pr` left join `m1_branch` `br` on((`br`.`bkode` = `pr`.`prcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `pr`.`prlokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `pr`.`prgudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `pr`.`prdimintaoleh`))) left join `m1_contact` `c2` on((`c2`.`kid` = `pr`.`prmintake`))) left join `m5_sq` `sq` on((`pr`.`pridsq` = `sq`.`sqid`))) left join `m0_status` `st1` on((`st1`.`kode` = `pr`.`prstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `pr`.`prstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `pr`.`prinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `pr`.`prmodifikasiuser`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Pr", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("prid"), 0), sptField,
                     FxDB(dr("prcabang"), ""), sptField,
                     FxDB(dr("prlokasi"), ""), sptField,
                     FxDB(dr("prgudang"), ""), sptField,
                     FxDB(dr("prasalbarang"), ""), sptField,
                     FxDB(dr("prasalbarangkategori"), 0), sptField,
                     FxDB(dr("prjenispembelian"), ""), sptField,
                     FxDB(dr("prjenispembeliankategori"), 0), sptField,
                     FxDB(dr("prcarabayar"), 0), sptField,
                     FxDB(dr("prsumber"), ""), sptField,
                     FxDB(dr("prautonotransaksi"), 0), sptField,
                     FxDB(dr("prnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("prtgl"), ""), formatTgl), sptField,
                     FxDB(dr("prkodepa"), 0), sptField,
                     FxDB(dr("prdimintaoleh"), 0), sptField,
                     FxDB(dr("prdimintaolehkontak"), ""), sptField,
                     FxDB(dr("prmintake"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prtgldipakai"), ""), formatTgl), sptField,
                     FxDB(dr("prtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("prtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("pruraian"), ""), sptField,
                     FxDB(dr("prcatatan"), ""), sptField,
                     FxDB(dr("prnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("prtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("prtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("prmatauang"), ""), sptField,
                     FxDB(dr("prkurs"), 0), sptField,
                     FxDB(dr("prhargatermasukpajak"), 0), sptField,
                     FxDB(dr("prtotal"), 0), sptField,
                     FxDB(dr("prdiskonpersen"), ""), sptField,
                     FxDB(dr("prjmldiskon"), 0), sptField,
                     FxDB(dr("prtotalpajak1detail"), 0), sptField,
                     FxDB(dr("prtotalpajak2detail"), 0), sptField,
                     FxDB(dr("prbiayalainpersen"), ""), sptField,
                     FxDB(dr("prbiayalain"), 0), sptField,
                     FxDB(dr("prtotaltransaksi"), 0), sptField,
                     FxDB(dr("pridsq"), 0), sptField,
                     FxDB(dr("prstatuscs"), 0), sptField,
                     FxDB(dr("prstatusrq"), 0), sptField,
                     FxDB(dr("prstatuspo"), 0), sptField,
                     FxDB(dr("prstatusipc"), 0), sptField,
                     FxDB(dr("prstatusgrn"), 0), sptField,
                     FxDB(dr("prstatusri"), 0), sptField,
                     FxDB(dr("prstatusdnr"), 0), sptField,
                     FxDB(dr("prstatusprt"), 0), sptField,
                     FxDB(dr("prstatusrealisasi"), 0), sptField,
                     FxDB(dr("prstatus"), 0), sptField,
                     FxDB(dr("prstatussebelumnya"), 0), sptField,
                     FxDB(dr("prjmlrevisi"), 0), sptField,
                     FxDB(dr("prcetakanke"), 0), sptField,
                     FxDB(dr("prinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("prmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("prposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("prisclose"), 0), sptField,
                     FxDB(dr("prcabangnama"), ""), sptField,
                     FxDB(dr("prlokasinama"), ""), sptField,
                     FxDB(dr("prgudangnama"), ""), sptField,
                     FxDB(dr("prdimintaolehkode"), ""), sptField,
                     FxDB(dr("prdimintaolehnama"), ""), sptField,
                     FxDB(dr("prmintakekode"), ""), sptField,
                     FxDB(dr("prmintakenama"), ""), sptField,
                     FxDB(dr("sqnotransaksi"), ""), sptField,
                     FxDB(dr("prstatusnama"), ""), sptField,
                     FxDB(dr("prstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("prinputusernama"), ""), sptField,
                     FxDB(dr("prmodifikasiusernama"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("prtglawal"), ""), formatTglWaktu), sptField,
                     AsFormatTanggal(FxDB(dr("prtglakhir"), ""), formatTglWaktu), sptField,
                     FxDB(dr("prcustomtext1"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("prid, prcabang, prlokasi, prgudang, prasalbarang, prasalbarangkategori, prjenispembelian, prjenispembeliankategori, prcarabayar, prsumber, prautonotransaksi, prnotransaksi, prtgl, prkodepa, prdimintaoleh, prdimintaolehkontak, prmintake, prtgldipakai, prtermin, prtgljatuhtempo, pruraian, prcatatan, prnoref, prtglnoref, prtglpenutupan, prmatauang, prkurs, prhargatermasukpajak, prtotal, prdiskonpersen, prjmldiskon, prtotalpajak1detail, prtotalpajak2detail, prbiayalainpersen, prbiayalain, prtotaltransaksi, pridsq, prstatuscs, prstatusrq, prstatuspo, prstatusipc, prstatusgrn, prstatusri, prstatusdnr, prstatusprt, prstatusrealisasi, prstatus, prstatussebelumnya, prjmlrevisi, prcetakanke, prinputuser, prinputtgl, prmodifikasiuser, prmodifikasitgl, prposting, prpostingtgl, prisclose, prcabangnama, prlokasinama, prgudangnama, prdimintaolehkode, prdimintaolehnama, prmintakekode, prmintakenama, sqnotransaksi, prstatusnama, prstatussebelumnyanama, prinputusernama, prmodifikasiusernama, prtglawal, prtglakhir, prcustomtext1"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_Pr_Detail_VSearch(ByVal param As String) As String
        'M4_Pr_Detail_VSearch --------------------------------------------------------
        'idprdetail, idpr, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, 
        'jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, hargajual, stokterakhir, 
        'supplier, cabang, lokasi, gudang, costcenter, divisi, subdivisi, 
        'proyek, catatan, urutan, idsqdetail, jmlcs, statuscs, jmlrq, 
        'statusrq, jmlpo, statuspo, jmlipc, statusipc, jmlgrn, statusgrn, 
        'jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, 
        'statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3, prnotransaksi, prtgldipakai, pruraian, 
        'prcatatan, prnoref, prtglnoref, prtermin, prterminnama, prterminharijatuhtempo, kodebarang, 
        'pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, supplierkode, suppliernama, supplierk1alamat1, 
        'supplierk1alamat2, supplierk2alamat1, supplierk2alamat2, kontakperson, jmlsisacs, jmlsisarq, jmlsisapo, 
        'jmlsisarealisasi, bkp, bjmllapangan, bsatuanlapangan, bhargajual1, bhargajual2, 
        'bhargajual3, bhargajual4, bhargajual5, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, 
        'bdiskonjual5, 
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
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        Dim ftData As String = paramSplit(5)
        Dim ftDataSplit() As String = ftData.Split("|")
        Dim idSupplier As String = ftDataSplit(0)
        Dim vMatauang As String = ftDataSplit(1)

        ''PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m4_pr_detail_v")
        'sql = "select `prd`.`idprdetail` AS `idprdetail`,`prd`.`idpr` AS `idpr`,`prd`.`idbarang` AS `idbarang`,`prd`.`namabarang` AS `namabarang`,`prd`.`tipebarang` AS `tipebarang`,`prd`.`jml` AS `jml`,`prd`.`satuan` AS `satuan`,`prd`.`nilaisatuan` AS `nilaisatuan`,`prd`.`jmlbarang` AS `jmlbarang`,`prd`.`satuanbarang` AS `satuanbarang`,`prd`.`matauang` AS `matauang`,`prd`.`kurs` AS `kurs`,`prd`.`harga` AS `harga`,`prd`.`diskon` AS `diskon`,`prd`.`jmldiskon` AS `jmldiskon`,`prd`.`pajak1` AS `pajak1`,`prd`.`jmlpajak1` AS `jmlpajak1`,`prd`.`pajak2` AS `pajak2`,`prd`.`jmlpajak2` AS `jmlpajak2`,`prd`.`hargajual` AS `hargajual`,`prd`.`stokterakhir` AS `stokterakhir`,`prd`.`supplier` AS `supplier`,`prd`.`cabang` AS `cabang`,`prd`.`lokasi` AS `lokasi`,`prd`.`gudang` AS `gudang`,`prd`.`costcenter` AS `costcenter`,`prd`.`divisi` AS `divisi`,`prd`.`subdivisi` AS `subdivisi`,`prd`.`proyek` AS `proyek`,`prd`.`catatan` AS `catatan`,`prd`.`urutan` AS `urutan`,`prd`.`idsqdetail` AS `idsqdetail`,`prd`.`jmlcs` AS `jmlcs`,`prd`.`statuscs` AS `statuscs`,`prd`.`jmlrq` AS `jmlrq`,`prd`.`statusrq` AS `statusrq`,`prd`.`jmlpo` AS `jmlpo`,`prd`.`statuspo` AS `statuspo`,`prd`.`jmlipc` AS `jmlipc`,`prd`.`statusipc` AS `statusipc`,`prd`.`jmlgrn` AS `jmlgrn`,`prd`.`statusgrn` AS `statusgrn`,`prd`.`jmlri` AS `jmlri`,`prd`.`statusri` AS `statusri`,`prd`.`jmldnr` AS `jmldnr`,`prd`.`statusdnr` AS `statusdnr`,`prd`.`jmlprt` AS `jmlprt`,`prd`.`statusprt` AS `statusprt`,`prd`.`jmlrealisasi` AS `jmlrealisasi`,`prd`.`statusrealisasi` AS `statusrealisasi`,`prd`.`isclose` AS `isclose`,`prd`.`customtext1` AS `customtext1`,`prd`.`customtext2` AS `customtext2`,`prd`.`customtext3` AS `customtext3`,`prd`.`customdbl1` AS `customdbl1`,`prd`.`customdbl2` AS `customdbl2`,`prd`.`customdbl3` AS `customdbl3`,`prd`.`customdate1` AS `customdate1`,`prd`.`customdate2` AS `customdate2`,`prd`.`customdate3` AS `customdate3`,`pr`.`prnotransaksi` AS `prnotransaksi`,`pr`.`prtgldipakai` AS `prtgldipakai`,`pr`.`pruraian` AS `pruraian`,`pr`.`prcatatan` AS `prcatatan`,`pr`.`prnoref` AS `prnoref`,`pr`.`prtglnoref` AS `prtglnoref`,`pr`.`prtermin` AS `prtermin`,`tr`.`trnama` AS `prterminnama`,`tr`.`trharijatuhtempo` AS `prterminharijatuhtempo`,`i`.`bkode` AS `kodebarang`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,`c`.`kkode` AS `supplierkode`,`c`.`knama` AS `suppliernama`,`c`.`k1alamat1` AS `supplierk1alamat1`,`c`.`k1alamat2` AS `supplierk1alamat2`,`c`.`k2alamat1` AS `supplierk2alamat1`,`c`.`k2alamat2` AS `supplierk2alamat2`,`ca`.`kanama` AS `kontakperson`,((`prd`.`jmlbarang` - `prd`.`jmlcs`) / `prd`.`nilaisatuan`) AS `jmlsisacs`,((`prd`.`jmlbarang` - `prd`.`jmlrq`) / `prd`.`nilaisatuan`) AS `jmlsisarq`,((`prd`.`jmlbarang` - `prd`.`jmlpo`) / `prd`.`nilaisatuan`) AS `jmlsisapo`,((`prd`.`jmlbarang` - `prd`.`jmlrealisasi`) / `prd`.`nilaisatuan`) AS `jmlsisarealisasi`, i.bkp, i.bjmllapangan, i.bsatuanlapangan from (((((((`m4_pr_detail` `prd` left join `m4_pr` `pr` on((`prd`.`idpr` = `pr`.`prid`))) left join `m1_terms` `tr` on((`pr`.`prtermin` = `tr`.`trkode`))) left join `m1_item` `i` on((`i`.`bid` = `prd`.`idbarang`))) left join `m1_tax` `t1` on((`prd`.`pajak1` = `t1`.`tkode`))) left join `m1_tax` `t2` on((`prd`.`pajak2` = `t2`.`tkode`))) left join `m1_contact` `c` on((`prd`.`supplier` = `c`.`kid`))) left join `m1_contact_attention` `ca` on(((`c`.`kid` = `ca`.`kaidkontak`) and (`ca`.`kadefault` = 1))))"

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m4_pr_detail_v")
        'sql = "select `prd`.`idprdetail` AS `idprdetail`,`prd`.`idpr` AS `idpr`,`prd`.`idbarang` AS `idbarang`,`prd`.`namabarang` AS `namabarang`,`prd`.`tipebarang` AS `tipebarang`,`prd`.`jml` AS `jml`,`prd`.`satuan` AS `satuan`,`prd`.`nilaisatuan` AS `nilaisatuan`,`prd`.`jmlbarang` AS `jmlbarang`,`prd`.`satuanbarang` AS `satuanbarang`,`prd`.`matauang` AS `matauang`,`prd`.`kurs` AS `kurs`,`prd`.`harga` AS `harga`,`prd`.`diskon` AS `diskon`,`prd`.`jmldiskon` AS `jmldiskon`,`prd`.`pajak1` AS `pajak1`,`prd`.`jmlpajak1` AS `jmlpajak1`,`prd`.`pajak2` AS `pajak2`,`prd`.`jmlpajak2` AS `jmlpajak2`,`prd`.`hargajual` AS `hargajual`,`prd`.`stokterakhir` AS `stokterakhir`,`prd`.`supplier` AS `supplier`,`prd`.`cabang` AS `cabang`,`prd`.`lokasi` AS `lokasi`,`prd`.`gudang` AS `gudang`,`prd`.`costcenter` AS `costcenter`,`prd`.`divisi` AS `divisi`,`prd`.`subdivisi` AS `subdivisi`,`prd`.`proyek` AS `proyek`,`prd`.`catatan` AS `catatan`,`prd`.`urutan` AS `urutan`,`prd`.`idsqdetail` AS `idsqdetail`,`prd`.`jmlcs` AS `jmlcs`,`prd`.`statuscs` AS `statuscs`,`prd`.`jmlrq` AS `jmlrq`,`prd`.`statusrq` AS `statusrq`,`prd`.`jmlpo` AS `jmlpo`,`prd`.`statuspo` AS `statuspo`,`prd`.`jmlipc` AS `jmlipc`,`prd`.`statusipc` AS `statusipc`,`prd`.`jmlgrn` AS `jmlgrn`,`prd`.`statusgrn` AS `statusgrn`,`prd`.`jmlri` AS `jmlri`,`prd`.`statusri` AS `statusri`,`prd`.`jmldnr` AS `jmldnr`,`prd`.`statusdnr` AS `statusdnr`,`prd`.`jmlprt` AS `jmlprt`,`prd`.`statusprt` AS `statusprt`,`prd`.`jmlrealisasi` AS `jmlrealisasi`,`prd`.`statusrealisasi` AS `statusrealisasi`,`prd`.`isclose` AS `isclose`,`prd`.`customtext1` AS `customtext1`,`prd`.`customtext2` AS `customtext2`,`prd`.`customtext3` AS `customtext3`,`prd`.`customdbl1` AS `customdbl1`,`prd`.`customdbl2` AS `customdbl2`,`prd`.`customdbl3` AS `customdbl3`,`prd`.`customdate1` AS `customdate1`,`prd`.`customdate2` AS `customdate2`,`prd`.`customdate3` AS `customdate3`,`pr`.`prnotransaksi` AS `prnotransaksi`,`pr`.`prtgldipakai` AS `prtgldipakai`,`pr`.`pruraian` AS `pruraian`,`pr`.`prcatatan` AS `prcatatan`,`pr`.`prnoref` AS `prnoref`,`pr`.`prtglnoref` AS `prtglnoref`,`pr`.`prtermin` AS `prtermin`,`tr`.`trnama` AS `prterminnama`,`tr`.`trharijatuhtempo` AS `prterminharijatuhtempo`,`i`.`bkode` AS `kodebarang`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,`c`.`kkode` AS `supplierkode`,`c`.`knama` AS `suppliernama`,`c`.`k1alamat1` AS `supplierk1alamat1`,`c`.`k1alamat2` AS `supplierk1alamat2`,`c`.`k2alamat1` AS `supplierk2alamat1`,`c`.`k2alamat2` AS `supplierk2alamat2`,`ca`.`kanama` AS `kontakperson`,((`prd`.`jmlbarang` - `prd`.`jmlcs`) / `prd`.`nilaisatuan`) AS `jmlsisacs`,((`prd`.`jmlbarang` - `prd`.`jmlrq`) / `prd`.`nilaisatuan`) AS `jmlsisarq`,((`prd`.`jmlbarang` - `prd`.`jmlpo`) / `prd`.`nilaisatuan`) AS `jmlsisapo`,((`prd`.`jmlbarang` - `prd`.`jmlrealisasi`) / `prd`.`nilaisatuan`) AS `jmlsisarealisasi`, i.bkp, i.bjmllapangan, i.bsatuanlapangan from (((((((`m4_pr_detail` `prd` left join `m4_pr` `pr` on((`prd`.`idpr` = `pr`.`prid`))) left join `m1_terms` `tr` on((`pr`.`prtermin` = `tr`.`trkode`))) left join `m1_item` `i` on((`i`.`bid` = `prd`.`idbarang`))) left join `m1_tax` `t1` on((`prd`.`pajak1` = `t1`.`tkode`))) left join `m1_tax` `t2` on((`prd`.`pajak2` = `t2`.`tkode`))) left join `m1_contact` `c` on((`prd`.`supplier` = `c`.`kid`))) left join `m1_contact_attention` `ca` on(((`c`.`kid` = `ca`.`kaidkontak`) and (`ca`.`kadefault` = 1))))"

        'QUERY DIBEDAKAN
        If Len(idSupplier) > 0 Then
            'JIKA ISI ID SUPPLIER MAKA FILTER BARANG SESUAI SUPPLIER
            sql = "select prd.idprdetail AS idprdetail, prd.idpr AS idpr, prd.idbarang AS idbarang, prd.namabarang AS namabarang, prd.tipebarang AS tipebarang, prd.jml AS jml, prd.satuan AS satuan, prd.nilaisatuan AS nilaisatuan, prd.jmlbarang AS jmlbarang, prd.satuanbarang AS satuanbarang, prd.matauang AS matauang, prd.kurs AS kurs, prd.harga AS harga, prd.diskon AS diskon, prd.jmldiskon AS jmldiskon, prd.pajak1 AS pajak1, prd.jmlpajak1 AS jmlpajak1, prd.pajak2 AS pajak2, prd.jmlpajak2 AS jmlpajak2, prd.hargajual AS hargajual, prd.stokterakhir AS stokterakhir, prd.supplier AS supplier, prd.cabang AS cabang, prd.lokasi AS lokasi, prd.gudang AS gudang, prd.costcenter AS costcenter, prd.divisi AS divisi, prd.subdivisi AS subdivisi, prd.proyek AS proyek, prd.catatan AS catatan, prd.urutan AS urutan, prd.idsqdetail AS idsqdetail, prd.jmlcs AS jmlcs, prd.statuscs AS statuscs, prd.jmlrq AS jmlrq, prd.statusrq AS statusrq, prd.jmlpo AS jmlpo, prd.statuspo AS statuspo, prd.jmlipc AS jmlipc, prd.statusipc AS statusipc, prd.jmlgrn AS jmlgrn, prd.statusgrn AS statusgrn, prd.jmlri AS jmlri, prd.statusri AS statusri, prd.jmldnr AS jmldnr, prd.statusdnr AS statusdnr, prd.jmlprt AS jmlprt, prd.statusprt AS statusprt, prd.jmlrealisasi AS jmlrealisasi, prd.statusrealisasi AS statusrealisasi, prd.isclose AS isclose, prd.customtext1 AS customtext1, prd.customtext2 AS customtext2, prd.customtext3 AS customtext3, prd.customdbl1 AS customdbl1, prd.customdbl2 AS customdbl2, prd.customdbl3 AS customdbl3, prd.customdate1 AS customdate1, prd.customdate2 AS customdate2, prd.customdate3 AS customdate3, pr.prnotransaksi AS prnotransaksi, pr.prtgldipakai AS prtgldipakai, pr.pruraian AS pruraian, pr.prcatatan AS prcatatan, pr.prnoref AS prnoref, pr.prtglnoref AS prtglnoref, pr.prtermin AS prtermin, tr.trnama AS prterminnama, tr.trharijatuhtempo AS prterminharijatuhtempo, i.bkode AS kodebarang, t1.tnama AS pajak1nama, t1.tnilai AS pajak1nilai, t2.tnama AS pajak2nama, t2.tnilai AS pajak2nilai, c.kkode AS supplierkode, c.knama AS suppliernama, c.k1alamat1 AS supplierk1alamat1, c.k1alamat2 AS supplierk1alamat2, c.k2alamat1 AS supplierk2alamat1, c.k2alamat2 AS supplierk2alamat2, ca.kanama AS kontakperson, ((prd.jmlbarang - prd.jmlcs) / prd.nilaisatuan) AS jmlsisacs, ((prd.jmlbarang - prd.jmlrq) / prd.nilaisatuan) AS jmlsisarq, ((prd.jmlbarang - prd.jmlpo) / prd.nilaisatuan) AS jmlsisapo, ((prd.jmlbarang - prd.jmlrealisasi) / prd.nilaisatuan) AS jmlsisarealisasi, ((prd.jmlbarang - prd.jmlsq) / prd.nilaisatuan) AS jmlsisasq, i.bkp, i.bjmllapangan, i.bsatuanlapangan,i.bhargajual1 AS bhargajual1,i.bhargajual2 AS bhargajual2,i.bhargajual3 AS bhargajual3,i.bhargajual4 AS bhargajual4,i.bhargajual5 AS bhargajual5,i.bdiskonjual1 AS bdiskonjual1,i.bdiskonjual2 AS bdiskonjual2,i.bdiskonjual3 AS bdiskonjual3,i.bdiskonjual4 AS bdiskonjual4,i.bdiskonjual5 AS bdiskonjual5, t1.takunbeli as pajak1akunbeli, t1c1.cnama as pajak1akunbelinama, t1.takunjual as pajak1akunjual, t1c2.cnama as pajak1akunjualnama, t2.takunbeli as pajak2akunbeli, t2c1.cnama as pajak2akunbelinama, t2.takunjual as pajak2akunjual, t2c2.cnama as pajak2akunjualnama, i.bhargabeli, d.dnama AS divisinama, sd.sdnama AS subdivisinama, cc.ccnama AS costcenternama, p.pnama AS proyeknama, i.bcustom9 AS bdiskonbeli from m4_pr_detail prd join m4_pr pr on prd.idpr = pr.prid join m1_item i on i.bid = prd.idbarang join m1_item_supplier its on prd.idbarang = its.isidbarang and its.isidkontak = '" & FixQuotes(idSupplier) & "' left join m1_terms tr on pr.prtermin = tr.trkode left join m1_tax t1 on prd.pajak1 = t1.tkode left join m1_tax t2 on prd.pajak2 = t2.tkode left join m1_contact c on prd.supplier = c.kid left join m1_contact_attention ca on c.kid = ca.kaidkontak and ca.kadefault = 1 left join m1_coa t1c1 on t1.takunbeli = t1c1.cnomor left join m1_coa t1c2 on t1.takunjual = t1c2.cnomor left join m1_coa t2c1 on t2.takunbeli = t2c1.cnomor left join m1_coa t2c2 on t2.takunjual = t2c2.cnomor left join m1_item_price itp on i.bid = itp.khidbarang and itp.khmatauang = '" & vMatauang & "' left join m1_currency cr on cr.ckode = '" & vMatauang & "' LEFT JOIN m1_division d ON d.dkode = prd.divisi LEFT JOIN m1_subdivision sd ON sd.sdkode = prd.subdivisi LEFT JOIN m1_cost_center cc ON cc.cckode = prd.costcenter LEFT JOIN m1_project p ON p.pkode = prd.proyek "
        Else
            'JIKA TIDAK ISI ID SUPPLIER MAKA TAMPIL SEMUA BARANG
            sql = "select prd.idprdetail AS idprdetail, prd.idpr AS idpr, prd.idbarang AS idbarang, prd.namabarang AS namabarang, prd.tipebarang AS tipebarang, prd.jml AS jml, prd.satuan AS satuan, prd.nilaisatuan AS nilaisatuan, prd.jmlbarang AS jmlbarang, prd.satuanbarang AS satuanbarang, prd.matauang AS matauang, prd.kurs AS kurs, prd.harga AS harga, prd.diskon AS diskon, prd.jmldiskon AS jmldiskon, prd.pajak1 AS pajak1, prd.jmlpajak1 AS jmlpajak1, prd.pajak2 AS pajak2, prd.jmlpajak2 AS jmlpajak2, prd.hargajual AS hargajual, prd.stokterakhir AS stokterakhir, prd.supplier AS supplier, prd.cabang AS cabang, prd.lokasi AS lokasi, prd.gudang AS gudang, prd.costcenter AS costcenter, prd.divisi AS divisi, prd.subdivisi AS subdivisi, prd.proyek AS proyek, prd.catatan AS catatan, prd.urutan AS urutan, prd.idsqdetail AS idsqdetail, prd.jmlcs AS jmlcs, prd.statuscs AS statuscs, prd.jmlrq AS jmlrq, prd.statusrq AS statusrq, prd.jmlpo AS jmlpo, prd.statuspo AS statuspo, prd.jmlipc AS jmlipc, prd.statusipc AS statusipc, prd.jmlgrn AS jmlgrn, prd.statusgrn AS statusgrn, prd.jmlri AS jmlri, prd.statusri AS statusri, prd.jmldnr AS jmldnr, prd.statusdnr AS statusdnr, prd.jmlprt AS jmlprt, prd.statusprt AS statusprt, prd.jmlrealisasi AS jmlrealisasi, prd.statusrealisasi AS statusrealisasi, prd.isclose AS isclose, prd.customtext1 AS customtext1, prd.customtext2 AS customtext2, prd.customtext3 AS customtext3, prd.customdbl1 AS customdbl1, prd.customdbl2 AS customdbl2, prd.customdbl3 AS customdbl3, prd.customdate1 AS customdate1, prd.customdate2 AS customdate2, prd.customdate3 AS customdate3, pr.prnotransaksi AS prnotransaksi, pr.prtgldipakai AS prtgldipakai, pr.pruraian AS pruraian, pr.prcatatan AS prcatatan, pr.prnoref AS prnoref, pr.prtglnoref AS prtglnoref, pr.prtermin AS prtermin, tr.trnama AS prterminnama, tr.trharijatuhtempo AS prterminharijatuhtempo, i.bkode AS kodebarang, t1.tnama AS pajak1nama, t1.tnilai AS pajak1nilai, t2.tnama AS pajak2nama, t2.tnilai AS pajak2nilai, c.kkode AS supplierkode, c.knama AS suppliernama, c.k1alamat1 AS supplierk1alamat1, c.k1alamat2 AS supplierk1alamat2, c.k2alamat1 AS supplierk2alamat1, c.k2alamat2 AS supplierk2alamat2, ca.kanama AS kontakperson, ((prd.jmlbarang - prd.jmlcs) / prd.nilaisatuan) AS jmlsisacs, ((prd.jmlbarang - prd.jmlrq) / prd.nilaisatuan) AS jmlsisarq, ((prd.jmlbarang - prd.jmlpo) / prd.nilaisatuan) AS jmlsisapo, ((prd.jmlbarang - prd.jmlrealisasi) / prd.nilaisatuan) AS jmlsisarealisasi, ((prd.jmlbarang - prd.jmlsq) / prd.nilaisatuan) AS jmlsisasq, i.bkp, i.bjmllapangan, i.bsatuanlapangan,i.bhargajual1 AS bhargajual1,i.bhargajual2 AS bhargajual2,i.bhargajual3 AS bhargajual3,i.bhargajual4 AS bhargajual4,i.bhargajual5 AS bhargajual5,i.bdiskonjual1 AS bdiskonjual1,i.bdiskonjual2 AS bdiskonjual2,i.bdiskonjual3 AS bdiskonjual3,i.bdiskonjual4 AS bdiskonjual4,i.bdiskonjual5 AS bdiskonjual5, t1.takunbeli as pajak1akunbeli, t1c1.cnama as pajak1akunbelinama, t1.takunjual as pajak1akunjual, t1c2.cnama as pajak1akunjualnama, t2.takunbeli as pajak2akunbeli, t2c1.cnama as pajak2akunbelinama, t2.takunjual as pajak2akunjual, t2c2.cnama as pajak2akunjualnama, i.bhargabeli, d.dnama AS divisinama, sd.sdnama AS subdivisinama, cc.ccnama AS costcenternama, p.pnama AS proyeknama, i.bcustom9 AS bdiskonbeli  from m4_pr_detail prd join m4_pr pr on prd.idpr = pr.prid join m1_item i on i.bid = prd.idbarang left join m1_terms tr on pr.prtermin = tr.trkode left join m1_tax t1 on prd.pajak1 = t1.tkode left join m1_tax t2 on prd.pajak2 = t2.tkode left join m1_contact c on prd.supplier = c.kid left join m1_contact_attention ca on c.kid = ca.kaidkontak and ca.kadefault = 1 left join m1_coa t1c1 on t1.takunbeli = t1c1.cnomor left join m1_coa t1c2 on t1.takunjual = t1c2.cnomor left join m1_coa t2c1 on t2.takunbeli = t2c1.cnomor left join m1_coa t2c2 on t2.takunjual = t2c2.cnomor left join m1_item_price itp on i.bid = itp.khidbarang and itp.khmatauang = '" & vMatauang & "' left join m1_currency cr on cr.ckode = '" & vMatauang & "' LEFT JOIN m1_division d ON d.dkode = prd.divisi LEFT JOIN m1_subdivision sd ON sd.sdkode = prd.subdivisi LEFT JOIN m1_cost_center cc ON cc.cckode = prd.costcenter LEFT JOIN m1_project p ON p.pkode = prd.proyek "
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M5_Sq_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idprdetail"), 0), sptField,
                     FxDB(dr("idpr"), 0), sptField,
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
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("diskon"), ""), sptField,
                     FxDB(dr("jmldiskon"), 0), sptField,
                     FxDB(dr("pajak1"), ""), sptField,
                     FxDB(dr("jmlpajak1"), 0), sptField,
                     FxDB(dr("pajak2"), ""), sptField,
                     FxDB(dr("jmlpajak2"), 0), sptField,
                     FxDB(dr("hargajual"), 0), sptField,
                     FxDB(dr("stokterakhir"), 0), sptField,
                     FxDB(dr("supplier"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idsqdetail"), 0), sptField,
                     FxDB(dr("jmlcs"), 0), sptField,
                     FxDB(dr("statuscs"), 0), sptField,
                     FxDB(dr("jmlrq"), 0), sptField,
                     FxDB(dr("statusrq"), 0), sptField,
                     FxDB(dr("jmlpo"), 0), sptField,
                     FxDB(dr("statuspo"), 0), sptField,
                     FxDB(dr("jmlipc"), 0), sptField,
                     FxDB(dr("statusipc"), 0), sptField,
                     FxDB(dr("jmlgrn"), 0), sptField,
                     FxDB(dr("statusgrn"), 0), sptField,
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
                     FxDB(dr("prnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("prtgldipakai"), ""), formatTgl), sptField,
                     FxDB(dr("pruraian"), ""), sptField,
                     FxDB(dr("prcatatan"), ""), sptField,
                     FxDB(dr("prnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("prtglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("prtermin"), ""), sptField,
                     FxDB(dr("prterminnama"), ""), sptField,
                     FxDB(dr("prterminharijatuhtempo"), 0), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("supplierkode"), ""), sptField,
                     FxDB(dr("suppliernama"), ""), sptField,
                     FxDB(dr("supplierk1alamat1"), ""), sptField,
                     FxDB(dr("supplierk1alamat2"), ""), sptField,
                     FxDB(dr("supplierk2alamat1"), ""), sptField,
                     FxDB(dr("supplierk2alamat2"), ""), sptField,
                     FxDB(dr("kontakperson"), ""), sptField,
                     FxDB(dr("jmlsisacs"), 0), sptField,
                     FxDB(dr("jmlsisarq"), 0), sptField,
                     FxDB(dr("jmlsisapo"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
                     FxDB(dr("bkp"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), 0), sptField,
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
                     FxDB(dr("jmlsisasq"), 0), sptField,
                     FxDB(dr("pajak1akunbeli"), ""), sptField,
                     FxDB(dr("pajak1akunbelinama"), ""), sptField,
                     FxDB(dr("pajak1akunjual"), ""), sptField,
                     FxDB(dr("pajak1akunjualnama"), ""), sptField,
                     FxDB(dr("pajak2akunbeli"), ""), sptField,
                     FxDB(dr("pajak2akunbelinama"), ""), sptField,
                     FxDB(dr("pajak2akunjual"), ""), sptField,
                     FxDB(dr("bhargabeli"), 0), sptField,
                     FxDB(dr("pajak2akunjualnama"), 0), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("bdiskonbeli"), 0), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idprdetail, idpr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, hargajual, stokterakhir, supplier, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, jmlcs, statuscs, jmlrq, statusrq, jmlpo, statuspo, jmlipc, statusipc, jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, prnotransaksi, prtgldipakai, pruraian, prcatatan, prnoref, prtglnoref, prtermin, prterminnama, prterminharijatuhtempo, kodebarang, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, supplierkode, suppliernama, supplierk1alamat1, supplierk1alamat2, supplierk2alamat1, supplierk2alamat2, kontakperson, jmlsisacs, jmlsisarq, jmlsisapo, jmlsisarealisasi, bkp, bjmllapangan, bsatuanlapangan, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, jmlsisasq, pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, bhargabeli, pajak2akunjualnama, divisinama, subdivisinama, costcenternama, bdiskonbeli, proyeknama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_PrTerkait(ByVal param As String) As String
        'M4_PrTerkait --------------------------------------------------------
        'prid, prnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "prid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = m4_pr_terkait()
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("prid"), 0), sptField,
                     FxDB(dr("prnotransaksi"), ""), sptField,
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
            result(2) = "Related PR data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("prid, prnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstanding As String, ByVal ftOutstanding As String, ByVal ftExistOutstandingPro As String, ByVal ftOutstandingPro As String) As String
        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaiSatuan As Double = 0, sisa As Double = 0
        Dim filterLookup As String = "", gudang As String = "", urutan As String = ""

        'VALIDASI OUTSTANDING ---------------------------------------
        If Len(ftExistOutstanding) > 0 Then 'ftExistOutstanding = rowExists, idsqdetail, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstanding)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "idsqdetail=" & dtval.Rows(0)("idsqdetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in SQ" : GoTo selesai
            End If

            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
            sql = "SELECT sqd.idsqdetail, (sqd.jmlbarang - sqd.jmlpr) as sisapr, i.bid, i.bkode FROM m5_sq_detail AS sqd INNER JOIN m1_item AS i ON sqd.idbarang = i.bid WHERE " & ftOutstanding
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisapr")

                filterLookup = "idsqdetail=" & dtval.Rows(0)("idsqdetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in SQ, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If

        If Len(ftExistOutstandingPro) > 0 Then 'ftExistOutstanding = rowExists, proyek
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingPro)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("proyek")

                filterLookup = "proyek='" & dtval.Rows(0)("proyek") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & kodebarang & " doesn't exists in Project Data" : GoTo selesai
            End If

            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
            sql = "SELECT p.pkode as proyek, (p.pnilaikontrak - p.prealisasikontrak) as sisapr FROM m1_project p WHERE pnilaikontrak > 0 AND (" & ftOutstandingPro & ")"
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("proyek")
                sisa = dtval.Rows(0)("sisapr")

                filterLookup = "proyek='" & dtval.Rows(0)("proyek") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("matauang")
                    nilaiSatuan = dtLookup.Rows(0)("kurs")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " exceeds the amount of Project, outstanding available " & FormatNumber(sisa / nilaiSatuan) & " " & satuan : GoTo selesai
            End If
        End If

        'END OF VALIDASI OUTSTANDING --------------------------------
selesai:
        Return errmessage
    End Function

    <WebMethod()>
    Private Function m4_pr_terkait() As String
        Dim sql As String
        'query
        'sql = "select `pr`.`prid` AS `prid`,`pr`.`prnotransaksi` AS `prnotransaksi`,`rq`.`rqsumber` AS `sumber`,`rq`.`rqid` AS `idterkait`,`rq`.`rqnotransaksi` AS `noterkait`,`rq`.`rqtgl` AS `tglterkait`,`rq`.`rqinputtgl` AS `inputtglterkait`,`rq`.`rqmodifikasitgl` AS `modifikasitglterkait` from (((`m4_rq_detail` `rqd` join `m4_rq` `rq` on((`rqd`.`idrq` = `rq`.`rqid`))) join `m4_pr_detail` `prd` on((`prd`.`idprdetail` = `rqd`.`idprdetail`))) join `m4_pr` `pr` on((`pr`.`prid` = `prd`.`idpr`))) where ((`rq`.`rqstatus` = 2) or (`rq`.`rqstatus` = 3) or (`rq`.`rqstatus` = 4) or (`rq`.`rqstatus` = 7)) AND pr.prid='validtransaksi' group by `rq`.`rqid`,`pr`.`prid` union all select `pr`.`prid` AS `prid`,`pr`.`prnotransaksi` AS `prnotransaksi`,`po`.`posumber` AS `sumber`,`po`.`poid` AS `idterkait`,`po`.`ponotransaksi` AS `noterkait`,`po`.`potgl` AS `tglterkait`,`po`.`poinputtgl` AS `inputtglterkait`,`po`.`pomodifikasitgl` AS `modifikasitglterkait` from (((`m4_po_detail` `pod` join `m4_po` `po` on((`pod`.`idpo` = `po`.`poid`))) join `m4_pr_detail` `prd` on((`prd`.`idprdetail` = `pod`.`idprdetail`))) join `m4_pr` `pr` on((`pr`.`prid` = `prd`.`idpr`))) where ((`po`.`postatus` = 2) or (`po`.`postatus` = 3) or (`po`.`postatus` = 4) or (`po`.`postatus` = 7)) AND pr.prid='validtransaksi' group by `po`.`poid`,`pr`.`prid` union all select `pr`.`prid` AS `prid`,`pr`.`prnotransaksi` AS `prnotransaksi`,`grn`.`grnsumber` AS `sumber`,`grn`.`grnid` AS `idterkait`,`grn`.`grnnotransaksi` AS `noterkait`,`grn`.`grntgl` AS `tglterkait`,`grn`.`grninputtgl` AS `inputtglterkait`,`grn`.`grnmodifikasitgl` AS `modifikasitglterkait` from (((`m4_grn_detail` `grnd` join `m4_grn` `grn` on((`grnd`.`idgrn` = `grn`.`grnid`))) join `m4_pr_detail` `prd` on((`prd`.`idprdetail` = `grnd`.`idprdetail`))) join `m4_pr` `pr` on((`pr`.`prid` = `prd`.`idpr`))) where ((`grn`.`grnstatus` = 2) or (`grn`.`grnstatus` = 3) or (`grn`.`grnstatus` = 4) or (`grn`.`grnstatus` = 7)) AND pr.prid='validtransaksi' group by `grn`.`grnid`,`pr`.`prid` union all select `pr`.`prid` AS `prid`,`pr`.`prnotransaksi` AS `prnotransaksi`,`ri`.`risumber` AS `sumber`,`ri`.`riid` AS `idterkait`,`ri`.`rinotransaksi` AS `noterkait`,`ri`.`ritgl` AS `tglterkait`,`ri`.`riinputtgl` AS `inputtglterkait`,`ri`.`rimodifikasitgl` AS `modifikasitglterkait` from (((`m4_ri_detail` `rid` join `m4_ri` `ri` on((`rid`.`idri` = `ri`.`riid`))) join `m4_pr_detail` `prd` on((`prd`.`idprdetail` = `rid`.`idprdetail`))) join `m4_pr` `pr` on((`pr`.`prid` = `prd`.`idpr`))) where ((`ri`.`ristatus` = 2) or (`ri`.`ristatus` = 3) or (`ri`.`ristatus` = 4) or (`ri`.`ristatus` = 7)) AND pr.prid='validtransaksi' group by `ri`.`riid`,`pr`.`prid` union all select `pr`.`prid` AS `prid`,`pr`.`prnotransaksi` AS `prnotransaksi`,`dnr`.`dnrsumber` AS `sumber`,`dnr`.`dnrid` AS `idterkait`,`dnr`.`dnrnotransaksi` AS `noterkait`,`dnr`.`dnrtgl` AS `tglterkait`,`dnr`.`dnrinputtgl` AS `inputtglterkait`,`dnr`.`dnrmodifikasitgl` AS `modifikasitglterkait` from (((`m4_dnr_detail` `dnrd` join `m4_dnr` `dnr` on((`dnrd`.`iddnr` = `dnr`.`dnrid`))) join `m4_pr_detail` `prd` on((`prd`.`idprdetail` = `dnrd`.`idprdetail`))) join `m4_pr` `pr` on((`pr`.`prid` = `prd`.`idpr`))) where ((`dnr`.`dnrstatus` = 2) or (`dnr`.`dnrstatus` = 3) or (`dnr`.`dnrstatus` = 4) or (`dnr`.`dnrstatus` = 7)) AND pr.prid='validtransaksi' group by `dnr`.`dnrid`,`pr`.`prid` union all select `pr`.`prid` AS `prid`,`pr`.`prnotransaksi` AS `prnotransaksi`,`prt`.`prtsumber` AS `sumber`,`prt`.`prtid` AS `idterkait`,`prt`.`prtnotransaksi` AS `noterkait`,`prt`.`prttgl` AS `tglterkait`,`prt`.`prtinputtgl` AS `inputtglterkait`,`prt`.`prtmodifikasitgl` AS `modifikasitglterkait` from (((`m4_prt_detail` `prtd` join `m4_prt` `prt` on((`prtd`.`idprt` = `prt`.`prtid`))) join `m4_pr_detail` `prd` on((`prd`.`idprdetail` = `prtd`.`idprdetail`))) join `m4_pr` `pr` on((`pr`.`prid` = `prd`.`idpr`))) where ((`prt`.`prtstatus` = 2) or (`prt`.`prtstatus` = 3) or (`prt`.`prtstatus` = 4) or (`prt`.`prtstatus` = 7)) AND pr.prid='validtransaksi' group by `prt`.`prtid`,`pr`.`prid`"
        sql = "  select `pr`.`prid` AS `prid`, `pr`.`prnotransaksi` AS `prnotransaksi`, `sq`.`sqsumber` AS `sumber`, `sq`.`sqid` AS `idterkait`, `sq`.`sqnotransaksi` AS `noterkait`, `sq`.`sqtgl` AS `tglterkait`, `sq`.`sqinputtgl` AS `inputtglterkait`, `sq`.`sqmodifikasitgl` AS `modifikasitglterkait`, 1 AS `jenisterkait` from (((`m5_sq_detail` `sqd` join `m5_sq` `sq` on((`sqd`.`idsq` = `sq`.`sqid`))) join `m4_pr_detail` `prd` on((`prd`.`idprdetail` = `sqd`.`idprdetail`))) join `m4_pr` `pr` on((`pr`.`prid` = `prd`.`idpr`))) where (((`sq`.`sqstatus` = 2) or (`sq`.`sqstatus` = 3) or (`sq`.`sqstatus` = 4) or (`sq`.`sqstatus` = 7)) and (`pr`.`prid` = 'validtransaksi')) group by `sq`.`sqid`, `pr`.`prid` "
        sql &= " union all "
        sql &= " select `pr`.`prid` AS `prid`,`pr`.`prnotransaksi` AS `prnotransaksi`,`rq`.`rqsumber` AS `sumber`,`rq`.`rqid` AS `idterkait`,`rq`.`rqnotransaksi` AS `noterkait`,`rq`.`rqtgl` AS `tglterkait`,`rq`.`rqinputtgl` AS `inputtglterkait`,`rq`.`rqmodifikasitgl` AS `modifikasitglterkait`,1 AS `jenisterkait` from (((`m4_rq_detail` `rqd` join `m4_rq` `rq` on((`rqd`.`idrq` = `rq`.`rqid`))) join `m4_pr_detail` `prd` on((`prd`.`idprdetail` = `rqd`.`idprdetail`))) join `m4_pr` `pr` on((`pr`.`prid` = `prd`.`idpr`))) where (((`rq`.`rqstatus` = 2) or (`rq`.`rqstatus` = 3) or (`rq`.`rqstatus` = 4) or (`rq`.`rqstatus` = 7)) and (`pr`.`prid` = 'validtransaksi')) group by `rq`.`rqid`,`pr`.`prid` "
        sql &= " union all "
        sql &= " select `pr`.`prid` AS `prid`,`pr`.`prnotransaksi` AS `prnotransaksi`,`po`.`posumber` AS `sumber`,`po`.`poid` AS `idterkait`,`po`.`ponotransaksi` AS `noterkait`,`po`.`potgl` AS `tglterkait`,`po`.`poinputtgl` AS `inputtglterkait`,`po`.`pomodifikasitgl` AS `modifikasitglterkait`,1 AS `jenisterkait` from (((`m4_po_detail` `pod` join `m4_po` `po` on((`pod`.`idpo` = `po`.`poid`))) join `m4_pr_detail` `prd` on((`prd`.`idprdetail` = `pod`.`idprdetail`))) join `m4_pr` `pr` on((`pr`.`prid` = `prd`.`idpr`))) where (((`po`.`postatus` = 2) or (`po`.`postatus` = 3) or (`po`.`postatus` = 4) or (`po`.`postatus` = 7)) and (`pr`.`prid` = 'validtransaksi')) group by `po`.`poid`,`pr`.`prid` "
        sql &= " union all "
        sql &= " select `pr`.`prid` AS `prid`,`pr`.`prnotransaksi` AS `prnotransaksi`,`grn`.`grnsumber` AS `sumber`,`grn`.`grnid` AS `idterkait`,`grn`.`grnnotransaksi` AS `noterkait`,`grn`.`grntgl` AS `tglterkait`,`grn`.`grninputtgl` AS `inputtglterkait`,`grn`.`grnmodifikasitgl` AS `modifikasitglterkait`,1 AS `jenisterkait` from (((`m4_grn_detail` `grnd` join `m4_grn` `grn` on((`grnd`.`idgrn` = `grn`.`grnid`))) join `m4_pr_detail` `prd` on((`prd`.`idprdetail` = `grnd`.`idprdetail`))) join `m4_pr` `pr` on((`pr`.`prid` = `prd`.`idpr`))) where (((`grn`.`grnstatus` = 2) or (`grn`.`grnstatus` = 3) or (`grn`.`grnstatus` = 4) or (`grn`.`grnstatus` = 7)) and (`pr`.`prid` = 'validtransaksi')) group by `grn`.`grnid`,`pr`.`prid` "
        sql &= " union all "
        sql &= " select `pr`.`prid` AS `prid`,`pr`.`prnotransaksi` AS `prnotransaksi`,`ri`.`risumber` AS `sumber`,`ri`.`riid` AS `idterkait`,`ri`.`rinotransaksi` AS `noterkait`,`ri`.`ritgl` AS `tglterkait`,`ri`.`riinputtgl` AS `inputtglterkait`,`ri`.`rimodifikasitgl` AS `modifikasitglterkait`,1 AS `jenisterkait` from (((`m4_ri_detail` `rid` join `m4_ri` `ri` on((`rid`.`idri` = `ri`.`riid`))) join `m4_pr_detail` `prd` on((`prd`.`idprdetail` = `rid`.`idprdetail`))) join `m4_pr` `pr` on((`pr`.`prid` = `prd`.`idpr`))) where (((`ri`.`ristatus` = 2) or (`ri`.`ristatus` = 3) or (`ri`.`ristatus` = 4) or (`ri`.`ristatus` = 7)) and (`pr`.`prid` = 'validtransaksi')) group by `ri`.`riid`,`pr`.`prid` "
        sql &= " union all "
        sql &= " select `pr`.`prid` AS `prid`,`pr`.`prnotransaksi` AS `prnotransaksi`,`dnr`.`dnrsumber` AS `sumber`,`dnr`.`dnrid` AS `idterkait`,`dnr`.`dnrnotransaksi` AS `noterkait`,`dnr`.`dnrtgl` AS `tglterkait`,`dnr`.`dnrinputtgl` AS `inputtglterkait`,`dnr`.`dnrmodifikasitgl` AS `modifikasitglterkait`,1 AS `jenisterkait` from (((`m4_dnr_detail` `dnrd` join `m4_dnr` `dnr` on((`dnrd`.`iddnr` = `dnr`.`dnrid`))) join `m4_pr_detail` `prd` on((`prd`.`idprdetail` = `dnrd`.`idprdetail`))) join `m4_pr` `pr` on((`pr`.`prid` = `prd`.`idpr`))) where (((`dnr`.`dnrstatus` = 2) or (`dnr`.`dnrstatus` = 3) or (`dnr`.`dnrstatus` = 4) or (`dnr`.`dnrstatus` = 7)) and (`pr`.`prid` = 'validtransaksi')) group by `dnr`.`dnrid`,`pr`.`prid` "
        sql &= " union all "
        sql &= " select `pr`.`prid` AS `prid`,`pr`.`prnotransaksi` AS `prnotransaksi`,`prt`.`prtsumber` AS `sumber`,`prt`.`prtid` AS `idterkait`,`prt`.`prtnotransaksi` AS `noterkait`,`prt`.`prttgl` AS `tglterkait`,`prt`.`prtinputtgl` AS `inputtglterkait`,`prt`.`prtmodifikasitgl` AS `modifikasitglterkait`,1 AS `jenisterkait` from (((`m4_prt_detail` `prtd` join `m4_prt` `prt` on((`prtd`.`idprt` = `prt`.`prtid`))) join `m4_pr_detail` `prd` on((`prd`.`idprdetail` = `prtd`.`idprdetail`))) join `m4_pr` `pr` on((`pr`.`prid` = `prd`.`idpr`))) where (((`prt`.`prtstatus` = 2) or (`prt`.`prtstatus` = 3) or (`prt`.`prtstatus` = 4) or (`prt`.`prtstatus` = 7)) and (`pr`.`prid` = 'validtransaksi')) group by `prt`.`prtid`,`pr`.`prid` "

        Return sql
    End Function

    <WebMethod()>
    Public Function M4_PrSimpanOld(ByVal param As String) As String
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
        'prid(0) As Integer, prcabang(1) As String, prlokasi(2) As String, prgudang(3) As String, prasalbarang(4) As String, 
        'prasalbarangkategori(5) As Integer, prjenispembelian(6) As String, prjenispembeliankategori(7) As Integer, prcarabayar(8) As Integer, prsumber(9) As String, 
        'prautonotransaksi(10) As Integer, prnotransaksi(11) As String, prtgl(12) As Date, prkodepa(13) As Integer, prdimintaoleh(14) As Integer, 
        'prdimintaolehkontak(15) As String, prmintake(16) As Integer, prtgldipakai(17) As Date, prtermin(18) As String, prtgljatuhtempo(19) As Date, 
        'pruraian(20) As String, prcatatan(21) As String, prnoref(22) As String, prtglnoref(23) As Date, prtglpenutupan(24) As Date, 
        'prmatauang(25) As String, prkurs(26) As Double, prhargatermasukpajak(27) As Integer, prtotal(28) As Double, prdiskonpersen(29) As String, 
        'prjmldiskon(30) As Double, prtotalpajak1detail(31) As Double, prtotalpajak2detail(32) As Double, prbiayalainpersen(33) As String, prbiayalain(34) As Double, 
        'prtotaltransaksi(35) As Double, pridsq(36) As Integer, prstatuscs(37) As Integer, prstatusrq(38) As Integer, prstatuspo(39) As Integer, 
        'prstatusipc(40) As Integer, prstatusgrn(41) As Integer, prstatusri(42) As Integer, prstatusdnr(43) As Integer, prstatusprt(44) As Integer, 
        'prstatus(45) As Integer, prstatussebelumnya(46) As Integer, prjmlrevisi(47) As Integer, prcetakanke(48) As Integer, prinputuser(49) As Integer, 
        'prinputtgl(50) As DateTime, prmodifikasiuser(51) As Integer, prmodifikasitgl(52) As DateTime, prisclose(53) As Integer, prcustomtext1(54) As String, 
        'prcustomtext2(55) As String, prcustomtext3(56) As String, prcustomtext4(57) As String, prcustomtext5(58) As String, prcustomint1(59) As Integer, 
        'prcustomint2(60) As Integer, prcustomint3(61) As Integer, prcustomdbl1(62) As Double, prcustomdbl2(63) As Double, prcustomdbl3(64) As Double, 
        'prcustomdate1(65) As Date, prcustomdate2(66) As Date, prcustomdate3(67) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'prid, prcabang, prlokasi, prgudang, prasalbarang, prasalbarangkategori, prjenispembelian, 
        'prjenispembeliankategori, prcarabayar, prsumber, prautonotransaksi, prnotransaksi, prtgl, prkodepa, 
        'prdimintaoleh, prdimintaolehkontak, prmintake, prtgldipakai, prtermin, prtgljatuhtempo, pruraian, 
        'prcatatan, prnoref, prtglnoref, prtglpenutupan, prmatauang, prkurs, prhargatermasukpajak, 
        'prtotal, prdiskonpersen, prjmldiskon, prtotalpajak1detail, prtotalpajak2detail, prbiayalainpersen, prbiayalain, 
        'prtotaltransaksi, pridsq, prstatuscs, prstatusrq, prstatuspo, prstatusipc, prstatusgrn, 
        'prstatusri, prstatusdnr, prstatusprt, prstatus, prstatussebelumnya, prjmlrevisi, prcetakanke, 
        'prinputuser, prinputtgl, prmodifikasiuser, prmodifikasitgl, prisclose, prcustomtext1, prcustomtext2, 
        'prcustomtext3, prcustomtext4, prcustomtext5, prcustomint1, prcustomint2, prcustomint3, prcustomdbl1, 
        'prcustomdbl2, prcustomdbl3, prcustomdate1, prcustomdate2, prcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 68) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'prid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "prid required numeric." : GoTo selesai
        End If
        'prasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "prasalbarangkategori required numeric." : GoTo selesai
        End If
        'prjenispembeliankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "prjenispembeliankategori required numeric." : GoTo selesai
        End If
        'prcarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "prcarabayar required numeric." : GoTo selesai
        End If
        'prautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "prautonotransaksi required numeric." : GoTo selesai
        End If
        'prtgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "prtgl required date." : GoTo selesai
        End If
        'prkodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "prkodepa required numeric." : GoTo selesai
        End If
        'prdimintaoleh(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "prdimintaoleh required numeric." : GoTo selesai
        End If
        If (dataUtama(14) < 1) Then
            result(2) = "prdimintaoleh can't be empty." : GoTo selesai
        End If
        'prmintake(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "prmintake required numeric." : GoTo selesai
        End If
        'prtgldipakai(17) As Date
        If (IsDate(dataUtama(17)) = False) Then
            result(2) = "prtgldipakai required date." : GoTo selesai
        End If
        'prtgljatuhtempo(19) As Date
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "prtgljatuhtempo required date." : GoTo selesai
        End If
        'prtglnoref(23) As Date
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "prtglnoref required date." : GoTo selesai
        End If
        'prtglpenutupan(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "prtglpenutupan required date." : GoTo selesai
        End If
        'prkurs(26) As Double
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "prkurs required numeric." : GoTo selesai
        End If
        'prhargatermasukpajak(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "prhargatermasukpajak required numeric." : GoTo selesai
        End If
        'prtotal(28) As Double
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "prtotal required numeric." : GoTo selesai
        End If
        'prjmldiskon(30) As Double
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "prjmldiskon required numeric." : GoTo selesai
        End If
        'prtotalpajak1detail(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "prtotalpajak1detail required numeric." : GoTo selesai
        End If
        'prtotalpajak2detail(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "prtotalpajak2detail required numeric." : GoTo selesai
        End If
        'prbiayalain(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "prbiayalain required numeric." : GoTo selesai
        End If
        'prtotaltransaksi(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "prtotaltransaksi required numeric." : GoTo selesai
        End If
        'pridsq(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "pridsq required numeric." : GoTo selesai
        End If
        'prstatuscs(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "prstatuscs required numeric." : GoTo selesai
        End If
        'prstatusrq(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "prstatusrq required numeric." : GoTo selesai
        End If
        'prstatuspo(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "prstatuspo required numeric." : GoTo selesai
        End If
        'prstatusipc(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "prstatusipc required numeric." : GoTo selesai
        End If
        'prstatusgrn(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "prstatusgrn required numeric." : GoTo selesai
        End If
        'prstatusri(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "prstatusri required numeric." : GoTo selesai
        End If
        'prstatusdnr(43) As Integer
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "prstatusdnr required numeric." : GoTo selesai
        End If
        'prstatusprt(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "prstatusprt required numeric." : GoTo selesai
        End If
        'prstatus(45) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "prstatus required numeric." : GoTo selesai
        End If
        'prstatussebelumnya(46) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "prstatussebelumnya required numeric." : GoTo selesai
        End If
        'prjmlrevisi(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "prjmlrevisi required numeric." : GoTo selesai
        End If
        'prcetakanke(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "prcetakanke required numeric." : GoTo selesai
        End If
        'prinputuser(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "prinputuser required numeric." : GoTo selesai
        End If
        'prinputtgl(50) As DateTime
        If (IsDate(dataUtama(50)) = False) Then
            result(2) = "prinputtgl required date." : GoTo selesai
        End If
        'prmodifikasiuser(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "prmodifikasiuser required numeric." : GoTo selesai
        End If
        'prmodifikasitgl(52) As DateTime
        If (IsDate(dataUtama(52)) = False) Then
            result(2) = "prmodifikasitgl required date." : GoTo selesai
        End If
        'prisclose(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "prisclose required numeric." : GoTo selesai
        End If
        'prcustomint1(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "prcustomint1 required numeric." : GoTo selesai
        End If
        'prcustomint2(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "prcustomint2 required numeric." : GoTo selesai
        End If
        'prcustomint3(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "prcustomint3 required numeric." : GoTo selesai
        End If
        'prcustomdbl1(62) As Double
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "prcustomdbl1 required numeric." : GoTo selesai
        End If
        'prcustomdbl2(63) As Double
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "prcustomdbl2 required numeric." : GoTo selesai
        End If
        'prcustomdbl3(64) As Double
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "prcustomdbl3 required numeric." : GoTo selesai
        End If
        'prcustomdate1(65) As Date
        If (IsDate(dataUtama(65)) = False) Then
            result(2) = "prcustomdate1 required date." : GoTo selesai
        End If
        'prcustomdate2(66) As Date
        If (IsDate(dataUtama(66)) = False) Then
            result(2) = "prcustomdate2 required date." : GoTo selesai
        End If
        'prcustomdate3(67) As Date
        If (IsDate(dataUtama(67)) = False) Then
            result(2) = "prcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'prcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "prcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "prcabang should not be more than 25 character." : GoTo selesai
        End If

        'prlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "prlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "prlokasi should not be more than 25 character." : GoTo selesai
        End If

        'prgudang(3) As String
        'If Len(dataUtama(3)) = 0 Then
        '    result(2) = "prgudang can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "prgudang should not be more than 25 character." : GoTo selesai
        End If

        'prsumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "prsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "prsumber should not be more than 10 character." : GoTo selesai
        End If

        'prnotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "prnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "prnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'prtgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "prtgl can't be empty" : GoTo selesai
        End If

        'prtgldipakai(17) As Date
        If Len(dataUtama(17)) = 0 Then
            result(2) = "prtgldipakai can't be empty" : GoTo selesai
        End If

        'prtgljatuhtempo(19) As Date
        If Len(dataUtama(19)) = 0 Then
            result(2) = "prtgljatuhtempo can't be empty" : GoTo selesai
        End If

        'prtglnoref(23) As Date
        If Len(dataUtama(23)) = 0 Then
            result(2) = "prtglnoref can't be empty" : GoTo selesai
        End If

        'prtglpenutupan(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "prtglpenutupan can't be empty" : GoTo selesai
        End If

        'prmatauang(25) As String
        If Len(dataUtama(25)) = 0 Then
            result(2) = "prmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(25)) > 25 Then
            result(2) = "prmatauang should not be more than 25 character." : GoTo selesai
        End If

        'prkurs(26) As Double
        If Len(dataUtama(26)) = 0 Then
            result(2) = "prkurs can't be empty" : GoTo selesai
        End If

        'prtotal(28) As Double
        If Len(dataUtama(28)) = 0 Then
            result(2) = "prtotal can't be empty" : GoTo selesai
        End If

        'prdiskonpersen(29) As String
        If Len(dataUtama(29)) = 0 Then
            result(2) = "prdiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(29)) > 25 Then
            result(2) = "prdiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'prjmldiskon(30) As Double
        If Len(dataUtama(30)) = 0 Then
            result(2) = "prjmldiskon can't be empty" : GoTo selesai
        End If

        'prtotalpajak1detail(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "prtotalpajak1detail can't be empty" : GoTo selesai
        End If

        'prtotalpajak2detail(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "prtotalpajak2detail can't be empty" : GoTo selesai
        End If

        'prbiayalainpersen(33) As String
        If Len(dataUtama(33)) = 0 Then
            result(2) = "prbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(33)) > 25 Then
            result(2) = "prbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'prbiayalain(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "prbiayalain can't be empty" : GoTo selesai
        End If

        'prtotaltransaksi(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "prtotaltransaksi can't be empty" : GoTo selesai
        End If

        'prinputtgl(50) As DateTime
        If Len(dataUtama(50)) = 0 Then
            result(2) = "prinputtgl can't be empty" : GoTo selesai
        End If

        'prmodifikasitgl(52) As DateTime
        If Len(dataUtama(52)) = 0 Then
            result(2) = "prmodifikasitgl can't be empty" : GoTo selesai
        End If

        'prcustomdbl1(62) As Double
        If Len(dataUtama(62)) = 0 Then
            result(2) = "prcustomdbl1 can't be empty" : GoTo selesai
        End If

        'prcustomdbl2(63) As Double
        If Len(dataUtama(63)) = 0 Then
            result(2) = "prcustomdbl2 can't be empty" : GoTo selesai
        End If

        'prcustomdbl3(64) As Double
        If Len(dataUtama(64)) = 0 Then
            result(2) = "prcustomdbl3 can't be empty" : GoTo selesai
        End If

        'prcustomdate1(65) As Date
        If Len(dataUtama(65)) = 0 Then
            result(2) = "prcustomdate1 can't be empty" : GoTo selesai
        End If

        'prcustomdate2(66) As Date
        If Len(dataUtama(66)) = 0 Then
            result(2) = "prcustomdate2 can't be empty" : GoTo selesai
        End If

        'prcustomdate3(67) As Date
        If Len(dataUtama(67)) = 0 Then
            result(2) = "prcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "prid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prjenispembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prjenispembeliankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prdimintaoleh", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prdimintaolehkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prmintake", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtgldipakai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pruraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prhargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prdiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prjmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pridsq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prstatuscs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prstatusrq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prstatuspo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prstatusipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prstatusgrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prstatusri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prstatusdnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prstatusprt", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "prid~prcabang~prlokasi~prgudang~prasalbarang~prasalbarangkategori~prjenispembelian~prjenispembeliankategori~prcarabayar~prsumber~prautonotransaksi~prnotransaksi~prtgl~prkodepa~prdimintaoleh~prdimintaolehkontak~prmintake~prtgldipakai~prtermin~prtgljatuhtempo~pruraian~prcatatan~prnoref~prtglnoref~prtglpenutupan~prmatauang~prkurs~prhargatermasukpajak~prtotal~prdiskonpersen~prjmldiskon~prtotalpajak1detail~prtotalpajak2detail~prbiayalainpersen~prbiayalain~prtotaltransaksi~pridsq~prstatuscs~prstatusrq~prstatuspo~prstatusipc~prstatusgrn~prstatusri~prstatusdnr~prstatusprt~prstatus~prstatussebelumnya~prjmlrevisi~prcetakanke~prinputuser~prinputtgl~prmodifikasiuser~prmodifikasitgl~prisclose~prcustomtext1~prcustomtext2~prcustomtext3~prcustomtext4~prcustomtext5~prcustomint1~prcustomint2~prcustomint3~prcustomdbl1~prcustomdbl2~prcustomdbl3~prcustomdate1~prcustomdate2~prcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idprdetail(0) As Integer, idpr(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, diskon(13) As String, jmldiskon(14) As Double, 
        'pajak1(15) As String, jmlpajak1(16) As Double, pajak2(17) As String, jmlpajak2(18) As Double, hargajual(19) As Double, 
        'stokterakhir(20) As Double, supplier(21) As Integer, cabang(22) As String, lokasi(23) As String, gudang(24) As String, 
        'costcenter(25) As String, divisi(26) As String, subdivisi(27) As String, proyek(28) As String, catatan(29) As String, 
        'urutan(30) As Integer, idsqdetail(31) As Integer, jmlcs(32) As Double, statuscs(33) As Integer, jmlrq(34) As Double, 
        'statusrq(35) As Integer, jmlpo(36) As Double, statuspo(37) As Integer, jmlipc(38) As Double, statusipc(39) As Integer, 
        'jmlgrn(40) As Double, statusgrn(41) As Integer, jmlri(42) As Double, statusri(43) As Integer, jmldnr(44) As Double, 
        'statusdnr(45) As Integer, jmlprt(46) As Double, statusprt(47) As Integer, isclose(48) As Integer, customtext1(49) As String, 
        'customtext2(50) As String, customtext3(51) As String, customdbl1(52) As Double, customdbl2(53) As Double, customdbl3(54) As Double, 
        'customdate1(55) As Date, customdate2(56) As Date, customdate3(57) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idprdetail, idpr, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, 
        'jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, hargajual, stokterakhir, 
        'supplier, cabang, lokasi, gudang, costcenter, divisi, subdivisi, 
        'proyek, catatan, urutan, idsqdetail, jmlcs, statuscs, jmlrq, 
        'statusrq, jmlpo, statuspo, jmlipc, statusipc, jmlgrn, statusgrn, 
        'jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idprdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpr", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "diskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmldiskon", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "pajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlpajak1", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "pajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlpajak2", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "hargajual", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "stokterakhir", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "supplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idsqdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlcs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuscs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlrq", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusrq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlpo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuspo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlipc", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlgrn", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusgrn", AsEnumTypeData.AsInt64)
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

        'Variabel ValidasiSimpan
        Dim ftExistOutstanding As String = "", ftOutstanding As String = ""
        Dim updNilai As String = "", updFilter As String = ""
        Dim idbarang As Integer = 0, idsqdetail As Integer = 0, jmlbarang As Double = 0

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
            'idprdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idprdetail required numeric." : GoTo selesai
            End If
            'idpr(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idpr required numeric." : GoTo selesai
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
            'harga(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'jmldiskon(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - jmldiskon required numeric." : GoTo selesai
            End If
            'jmlpajak1(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak1 required numeric." : GoTo selesai
            End If
            'jmlpajak2(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak2 required numeric." : GoTo selesai
            End If
            'hargajual(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - hargajual required numeric." : GoTo selesai
            End If
            'stokterakhir(20) As Double
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - stokterakhir required numeric." : GoTo selesai
            End If
            'supplier(21) As Integer
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - supplier required numeric." : GoTo selesai
            End If
            'urutan(30) As Integer
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idsqdetail(31) As Integer
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - idsqdetail required numeric." : GoTo selesai
            End If
            'jmlcs(32) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - jmlcs required numeric." : GoTo selesai
            End If
            'statuscs(33) As Integer
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - statuscs required numeric." : GoTo selesai
            End If
            'jmlrq(34) As Double
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - jmlrq required numeric." : GoTo selesai
            End If
            'statusrq(35) As Integer
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - statusrq required numeric." : GoTo selesai
            End If
            'jmlpo(36) As Double
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - jmlpo required numeric." : GoTo selesai
            End If
            'statuspo(37) As Integer
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - statuspo required numeric." : GoTo selesai
            End If
            'jmlipc(38) As Double
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - jmlipc required numeric." : GoTo selesai
            End If
            'statusipc(39) As Integer
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - statusipc required numeric." : GoTo selesai
            End If
            'jmlgrn(40) As Double
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - jmlgrn required numeric." : GoTo selesai
            End If
            'statusgrn(41) As Integer
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - statusgrn required numeric." : GoTo selesai
            End If
            'jmlri(42) As Double
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - jmlri required numeric." : GoTo selesai
            End If
            'statusri(43) As Integer
            If (IsNumeric(dataRowDetail(43)) = False) Then
                result(2) = "Row : " & i & " - statusri required numeric." : GoTo selesai
            End If
            'jmldnr(44) As Double
            If (IsNumeric(dataRowDetail(44)) = False) Then
                result(2) = "Row : " & i & " - jmldnr required numeric." : GoTo selesai
            End If
            'statusdnr(45) As Integer
            If (IsNumeric(dataRowDetail(45)) = False) Then
                result(2) = "Row : " & i & " - statusdnr required numeric." : GoTo selesai
            End If
            'jmlprt(46) As Double
            If (IsNumeric(dataRowDetail(46)) = False) Then
                result(2) = "Row : " & i & " - jmlprt required numeric." : GoTo selesai
            End If
            'statusprt(47) As Integer
            If (IsNumeric(dataRowDetail(47)) = False) Then
                result(2) = "Row : " & i & " - statusprt required numeric." : GoTo selesai
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

            'harga(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'diskon(13) As String
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - diskon can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(13)) > 25 Then
                result(2) = "Row : " & i & " - diskon should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskon(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskon can't be empty" : GoTo selesai
            Else
                'HITUNG JMLDISKON : jml(5) As Double, harga(12) As Double, diskon(13) As String
                dataRowDetail(14) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(12)), FixQuotes(dataRowDetail(13).ToString))
            End If

            'jmlpajak1(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak1 can't be empty" : GoTo selesai
            End If

            'jmlpajak2(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak2 can't be empty" : GoTo selesai
            End If

            'hargajual(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - hargajual can't be empty" : GoTo selesai
            End If

            'stokterakhir(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - stokterakhir can't be empty" : GoTo selesai
            End If

            ''catatan(29) As String
            'If Len(dataRowDetail(29)) = 0 Then
            '    result(2) = "Row : " & i & " - catatan can't be empty" : GoTo selesai
            'End If

            'jmlcs(32) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - jmlcs can't be empty" : GoTo selesai
            End If

            'jmlrq(34) As Double
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Row : " & i & " - jmlrq can't be empty" : GoTo selesai
            End If

            'jmlpo(36) As Double
            If Len(dataRowDetail(36)) = 0 Then
                result(2) = "Row : " & i & " - jmlpo can't be empty" : GoTo selesai
            End If

            'jmlipc(38) As Double
            If Len(dataRowDetail(38)) = 0 Then
                result(2) = "Row : " & i & " - jmlipc can't be empty" : GoTo selesai
            End If

            'jmlgrn(40) As Double
            If Len(dataRowDetail(40)) = 0 Then
                result(2) = "Row : " & i & " - jmlgrn can't be empty" : GoTo selesai
            End If

            'jmlri(42) As Double
            If Len(dataRowDetail(42)) = 0 Then
                result(2) = "Row : " & i & " - jmlri can't be empty" : GoTo selesai
            End If

            'jmldnr(44) As Double
            If Len(dataRowDetail(44)) = 0 Then
                result(2) = "Row : " & i & " - jmldnr can't be empty" : GoTo selesai
            End If

            'jmlprt(46) As Double
            If Len(dataRowDetail(46)) = 0 Then
                result(2) = "Row : " & i & " - jmlprt can't be empty" : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idprdetail~idpr~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~hargajual~stokterakhir~supplier~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~idsqdetail~jmlcs~statuscs~jmlrq~statusrq~jmlpo~statuspo~jmlipc~statusipc~jmlgrn~statusgrn~jmlri~statusri~jmldnr~statusdnr~jmlprt~statusprt~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56) & "~" & dataRowDetail(57)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , idsqdetail(31) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : idsqdetail = dataRowDetail(31)

            'VALIDASI OUTSTANDING -------------------------
            If idsqdetail <> 0 Then
                '1. CEK DATA EXIST ------------------------
                ftExistOutstanding = IIf(Len(ftExistOutstanding.ToString) = 0, "", ftExistOutstanding & " UNION ")
                ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m5_sq_detail JOIN m5_sq ON idsq = sqid WHERE idsqdetail = '" & idsqdetail & "' AND (sqstatus = 2 OR sqstatus = 3 OR sqstatus = 4 OR sqstatus = 7) LIMIT 1) as rowExists, '" & idsqdetail & "' as idsqdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsqdetail=" & idsqdetail)
                ftOutstanding = IIf(Len(ftOutstanding.ToString) = 0, "", ftOutstanding & " OR ")
                ftOutstanding = String.Concat(ftOutstanding, " (sqd.idsqdetail = " & idsqdetail & " AND " & Outstanding & " > (sqd.jmlbarang - sqd.jmlpr)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilai = String.Concat("WHEN '" & idsqdetail & "' THEN ROUND(jmlpr + '" & Outstanding & "', 5) ", updNilai)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                updFilter = String.Concat(updFilter, "(idsqdetail = '" & idsqdetail & "')")
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
        Dim rowUpdate As Integer = 0

        Try
            'Proses utama
            If (dtutama.Rows.Count > 0) Then
                Dim drutama As DataRow = dtutama.Rows(0)

                ''CEK PERIODE AKUNTANSI ==================================
                'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("prtgl")), AsFormatTanggal(drutama("prtgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("prstatus") = 2 Then
                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstanding, ftOutstanding, "", "")
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("prtermin").ToString, AsFormatTanggal(drutama("prtgl")), "prtgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("prtgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TOTAL = subtotal
                drutama("prtotal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("prtotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("prtotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                If Integer.Parse(drutama("prhargatermasukpajak")) = 0 Then
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                    drutama("prtotaltransaksi") = Double.Parse(drutama("prtotal")) - Double.Parse(drutama("prjmldiskon")) + Double.Parse(drutama("prtotalpajak1detail")) + Double.Parse(drutama("prtotalpajak2detail")) + Double.Parse(drutama("prbiayalain"))

                Else
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                    drutama("prtotaltransaksi") = Double.Parse(drutama("prtotal")) - Double.Parse(drutama("prjmldiskon")) + Double.Parse(drutama("prbiayalain"))

                End If
                'END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("prid")
                    notransaksi = drutama("prnotransaksi")

                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(prid), prnotransaksi FROM M4_pr WHERE prid='" & result(4) & "' AND prstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(prid) FROM m4_pr WHERE prnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_pr_history
                        Dim rsSimpanHistory As String = SimpanHistory.M4_Pr_HistorySimpan("" & paramSplit(0) & "★M4_Pr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("prsumber")) & "▼" & FixQuotes(drutama("prid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M4_Pr set prcabang  = '" & FixQuotes(drutama("prcabang")) & "', prlokasi  = '" & FixQuotes(drutama("prlokasi")) & "', prgudang  = '" & FixQuotes(drutama("prgudang")) & "', prasalbarang  = '" & FixQuotes(drutama("prasalbarang")) & "', prasalbarangkategori  = " & drutama("prasalbarangkategori") & ", prjenispembelian  = '" & FixQuotes(drutama("prjenispembelian")) & "', prjenispembeliankategori  = " & drutama("prjenispembeliankategori") & ", prcarabayar  = " & drutama("prcarabayar") & ", prsumber  = '" & FixQuotes(drutama("prsumber")) & "', prautonotransaksi  = " & drutama("prautonotransaksi") & ", prnotransaksi  = '" & notransaksi & "', prtgl  = '" & FixQuotes(AsFormatTanggal(drutama("prtgl"))) & "', prkodepa  = " & drutama("prkodepa") & ", prdimintaoleh  = " & drutama("prdimintaoleh") & ", prdimintaolehkontak  = '" & FixQuotes(drutama("prdimintaolehkontak")) & "', prmintake  = " & drutama("prmintake") & ", prtgldipakai  = '" & FixQuotes(AsFormatTanggal(drutama("prtgldipakai"))) & "', prtermin  = '" & FixQuotes(drutama("prtermin")) & "', prtgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("prtgljatuhtempo"))) & "', pruraian  = '" & FixQuotes(drutama("pruraian")) & "', prcatatan  = '" & FixQuotes(drutama("prcatatan")) & "', prnoref  = '" & FixQuotes(drutama("prnoref")) & "', prtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("prtglnoref"))) & "', prtglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("prtglpenutupan"))) & "', prmatauang  = '" & FixQuotes(drutama("prmatauang")) & "', prkurs  = '" & FixDouble(drutama("prkurs")) & "', prhargatermasukpajak  = " & drutama("prhargatermasukpajak") & ", prtotal  = '" & FixDouble(drutama("prtotal")) & "', prdiskonpersen  = '" & FixQuotes(drutama("prdiskonpersen")) & "', prjmldiskon  = '" & FixDouble(drutama("prjmldiskon")) & "', prtotalpajak1detail  = '" & FixDouble(drutama("prtotalpajak1detail")) & "', prtotalpajak2detail  = '" & FixDouble(drutama("prtotalpajak2detail")) & "', prbiayalainpersen  = '" & FixQuotes(drutama("prbiayalainpersen")) & "', prbiayalain  = '" & FixDouble(drutama("prbiayalain")) & "', prtotaltransaksi  = '" & FixDouble(drutama("prtotaltransaksi")) & "', pridsq  = " & drutama("pridsq") & ", prstatuscs  = " & drutama("prstatuscs") & ", prstatusrq  = " & drutama("prstatusrq") & ", prstatuspo  = " & drutama("prstatuspo") & ", prstatusipc  = " & drutama("prstatusipc") & ", prstatusgrn  = " & drutama("prstatusgrn") & ", prstatusri  = " & drutama("prstatusri") & ", prstatusdnr  = " & drutama("prstatusdnr") & ", prstatusprt  = " & drutama("prstatusprt") & ", prstatus  = " & drutama("prstatus") & ", prstatussebelumnya  = " & drutama("prstatussebelumnya") & ", prjmlrevisi  = prjmlrevisi+1, prcetakanke  = " & drutama("prcetakanke") & ",  prmodifikasiuser  = " & drutama("prmodifikasiuser") & ", prmodifikasitgl  = NOW(), prcustomtext1  = '" & FixQuotes(drutama("prcustomtext1")) & "', prcustomtext2  = '" & FixQuotes(drutama("prcustomtext2")) & "', prcustomtext3  = '" & FixQuotes(drutama("prcustomtext3")) & "', prcustomtext4  = '" & FixQuotes(drutama("prcustomtext4")) & "', prcustomtext5  = '" & FixQuotes(drutama("prcustomtext5")) & "', prcustomint1  = " & drutama("prcustomint1") & ", prcustomint2  = " & drutama("prcustomint2") & ", prcustomint3  = " & drutama("prcustomint3") & ", prcustomdbl1  = '" & FixDouble(drutama("prcustomdbl1")) & "', prcustomdbl2  = '" & FixDouble(drutama("prcustomdbl2")) & "', prcustomdbl3  = '" & FixDouble(drutama("prcustomdbl3")) & "', prcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("prcustomdate1"))) & "', prcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("prcustomdate2"))) & "', prcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("prcustomdate3"))) & "' where prid = '" & drutama("prid") & "'"
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

                    If drutama("prautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("prcabang"), drutama("prlokasi"), drutama("prsumber"), drutama("prtgl"))
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
                        notransaksi = drutama("prnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(prid) FROM m4_pr WHERE prnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Pr (prcabang, prlokasi, prgudang, prasalbarang, prasalbarangkategori, prjenispembelian, prjenispembeliankategori, prcarabayar, prsumber, prautonotransaksi, prnotransaksi, prtgl, prkodepa, prdimintaoleh, prdimintaolehkontak, prmintake, prtgldipakai, prtermin, prtgljatuhtempo, pruraian, prcatatan, prnoref, prtglnoref, prtglpenutupan, prmatauang, prkurs, prhargatermasukpajak, prtotal, prdiskonpersen, prjmldiskon, prtotalpajak1detail, prtotalpajak2detail, prbiayalainpersen, prbiayalain, prtotaltransaksi, pridsq, prstatuscs, prstatusrq, prstatuspo, prstatusipc, prstatusgrn, prstatusri, prstatusdnr, prstatusprt, prstatus, prstatussebelumnya, prjmlrevisi, prcetakanke, prinputuser, prinputtgl, prmodifikasiuser, prmodifikasitgl, prisclose, prcustomtext1, prcustomtext2, prcustomtext3, prcustomtext4, prcustomtext5, prcustomint1, prcustomint2, prcustomint3, prcustomdbl1, prcustomdbl2, prcustomdbl3, prcustomdate1, prcustomdate2, prcustomdate3) values('" & FixQuotes(drutama("prcabang")) & "', '" & FixQuotes(drutama("prlokasi")) & "', '" & FixQuotes(drutama("prgudang")) & "', '" & FixQuotes(drutama("prasalbarang")) & "', " & drutama("prasalbarangkategori") & ", '" & FixQuotes(drutama("prjenispembelian")) & "', " & drutama("prjenispembeliankategori") & ", " & drutama("prcarabayar") & ", '" & FixQuotes(drutama("prsumber")) & "', " & drutama("prautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("prtgl"))) & "', " & drutama("prkodepa") & ", " & drutama("prdimintaoleh") & ", '" & FixQuotes(drutama("prdimintaolehkontak")) & "', " & drutama("prmintake") & ", '" & FixQuotes(AsFormatTanggal(drutama("prtgldipakai"))) & "', '" & FixQuotes(drutama("prtermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("prtgljatuhtempo"))) & "', '" & FixQuotes(drutama("pruraian")) & "', '" & FixQuotes(drutama("prcatatan")) & "', '" & FixQuotes(drutama("prnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("prtglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("prtglpenutupan"))) & "', '" & FixQuotes(drutama("prmatauang")) & "', '" & FixDouble(drutama("prkurs")) & "', " & drutama("prhargatermasukpajak") & ", '" & FixDouble(drutama("prtotal")) & "', '" & FixQuotes(drutama("prdiskonpersen")) & "', '" & FixDouble(drutama("prjmldiskon")) & "', '" & FixDouble(drutama("prtotalpajak1detail")) & "', '" & FixDouble(drutama("prtotalpajak2detail")) & "', '" & FixQuotes(drutama("prbiayalainpersen")) & "', '" & FixDouble(drutama("prbiayalain")) & "', '" & FixDouble(drutama("prtotaltransaksi")) & "', " & drutama("pridsq") & ", " & drutama("prstatuscs") & ", " & drutama("prstatusrq") & ", " & drutama("prstatuspo") & ", " & drutama("prstatusipc") & ", " & drutama("prstatusgrn") & ", " & drutama("prstatusri") & ", " & drutama("prstatusdnr") & ", " & drutama("prstatusprt") & ", " & drutama("prstatus") & ", " & drutama("prstatussebelumnya") & ", " & drutama("prjmlrevisi") & ", " & drutama("prcetakanke") & ", " & drutama("prinputuser") & ", NOW(), " & drutama("prmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("prisclose") & ", '" & FixQuotes(drutama("prcustomtext1")) & "', '" & FixQuotes(drutama("prcustomtext2")) & "', '" & FixQuotes(drutama("prcustomtext3")) & "', '" & FixQuotes(drutama("prcustomtext4")) & "', '" & FixQuotes(drutama("prcustomtext5")) & "', " & drutama("prcustomint1") & ", " & drutama("prcustomint2") & ", " & drutama("prcustomint3") & ", '" & FixDouble(drutama("prcustomdbl1")) & "', '" & FixDouble(drutama("prcustomdbl2")) & "', '" & FixDouble(drutama("prcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("prcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("prcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("prcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select prid from M4_pr where prnotransaksi='" & notransaksi & "' AND prinputuser= '" & userid & "' order by prmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Pr_Detail where idpr = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idprdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixDouble(dr1("hargajual")) & "', '" & FixDouble(dr1("stokterakhir")) & "', " & dr1("supplier") & ", '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idsqdetail") & ", '" & FixDouble(dr1("jmlcs")) & "', " & dr1("statuscs") & ", '" & FixDouble(dr1("jmlrq")) & "', " & dr1("statusrq") & ", '" & FixDouble(dr1("jmlpo")) & "', " & dr1("statuspo") & ", '" & FixDouble(dr1("jmlipc")) & "', " & dr1("statusipc") & ", '" & FixDouble(dr1("jmlgrn")) & "', " & dr1("statusgrn") & ", '" & FixDouble(dr1("jmlri")) & "', " & dr1("statusri") & ", '" & FixDouble(dr1("jmldnr")) & "', " & dr1("statusdnr") & ", '" & FixDouble(dr1("jmlprt")) & "', " & dr1("statusprt") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Pr_Detail(idprdetail, idpr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, hargajual, stokterakhir, supplier, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, jmlcs, statuscs, jmlrq, statusrq, jmlpo, statuspo, jmlipc, statusipc, jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                If drutama("prstatus") = 2 Then
                    If Len(updNilai) > 0 Then
                        'UPDATE OUTSTANDING TRANSAKSI =======================================================
                        'UPDATE DETAIL
                        sql = "UPDATE m5_sq_detail SET jmlpr = (CASE idsqdetail " & updNilai & " ELSE jmlpr END) WHERE " & updFilter
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idsq FROM m5_sq_detail WHERE " & updFilter & " GROUP BY idsq")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idsq = '" & dr1("idsq") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDB("SELECT idsq, SUM(jmlbarang) as jmlbarang, SUM(jmlpr) as jmlpr FROM m5_sq_detail WHERE " & ftDetail & " GROUP BY idsq")
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilai = "" : updFilter = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlpr") >= dr1("jmlbarang") Then
                                    statusOut = 2
                                ElseIf dr1("jmlpr") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilai = String.Concat(updNilai, "WHEN '" & dr1("idsq") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                                updFilter = String.Concat(updFilter, "(sqid = '" & dr1("idsq") & "')")
                            Next

                            sql = "UPDATE m5_sq SET sqstatuspr = (CASE sqid " & updNilai & " ELSE sqstatuspr END) WHERE " & updFilter
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
                Dim sumber As String = "Pr", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M4_PrUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("prdimintaolehkode", "c1.kkode")
            Filter = Filter.Replace("prdimintaolehnama", "c1.knama")
            Filter = Filter.Replace("prmintakekode", "c2.kkode")
            Filter = Filter.Replace("prmintakenama", "c2.knama")
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
            Dim sumber As String = "Pr", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Prtgl, Prnotransaksi, Prstatus FROM m4_Pr WHERE Prid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Prstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m4_pr_history
            Dim rsSimpanHistory As String = SimpanHistory.M4_Pr_HistorySimpan("" & paramSplit(0) & "★M4_Pr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m4_pr_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idsqdetail As Integer = 0
                Dim ftOutstanding As String = "", updNilai As String = "", updFilter As String = ""
                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDB("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idsqdetail, urutan FROM m4_pr_detail WHERE idpr = '" & idtransaksi & "'")
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : idsqdetail = dr1("idsqdetail")

                        'UPDATE OUTSTANDING ---------------------------
                        If idsqdetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING ----------
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsqdetail=" & idsqdetail)
                            updNilai = String.Concat("WHEN '" & idsqdetail & "' THEN ROUND(jmlpr - '" & Outstanding & "', 5) ", updNilai)
                            '2. SET FILTERUPDATE OUTSTANDING ----------
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(idsqdetail = '" & idsqdetail & "')")
                        End If
                        'END OF BUAT FILTER UNTUK UPDATE --------------------------
                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                If Len(updFilter) > 0 Then
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m5_sq_detail SET jmlpr = (CASE idsqdetail " & updNilai & " ELSE jmlpr END) WHERE " & updFilter
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idsq FROM m5_sq_detail WHERE " & updFilter & " GROUP BY idsq")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idsq = '" & dr1("idsq") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDB("SELECT idsq, SUM(jmlbarang) as jmlbarang, SUM(jmlpr) as jmlpr FROM m5_sq_detail WHERE " & ftDetail & " GROUP BY idsq")
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilai = "" : updFilter = ""
                        For Each dr1 As DataRow In dtOut.Rows
                            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                            If dr1("jmlpr") >= dr1("jmlbarang") Then
                                statusOut = 2
                            ElseIf dr1("jmlpr") < 1 Then
                                statusOut = 0
                            Else
                                statusOut = 1
                            End If
                            '2. SET NILAI UPDATE OUTSTANDING
                            updNilai = String.Concat(updNilai, "WHEN '" & dr1("idsq") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(sqid = '" & dr1("idsq") & "')")
                        Next

                        sql = "UPDATE m5_sq SET sqstatuspr = (CASE sqid " & updNilai & " ELSE sqstatuspr END) WHERE " & updFilter
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
            sql = "UPDATE m4_Pr SET Prstatus = " & nilaiStatus & ", Prmodifikasiuser='" & userid & "', Prmodifikasitgl = NOW(), Prposting = 0, Prpostingtgl = '1971-01-01 00:00:00', Prjmlrevisi = Prjmlrevisi + 1 WHERE Prid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_PrSearch(PostWsSearch(paramSplit(0), "M4_PrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_PrDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("prdimintaolehkode", "c1.kkode")
            Filter = Filter.Replace("prdimintaolehnama", "c1.knama")
            Filter = Filter.Replace("prmintakekode", "c2.kkode")
            Filter = Filter.Replace("prmintakenama", "c2.knama")
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
            Dim sumber As String = "Pr", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Prid, Prnotransaksi FROM m4_Pr WHERE Prid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT prcabang, prlokasi, prsumber, prautonotransaksi, prnotransaksi, prtgl"
            sql &= " FROM M4_pr"
            sql &= " WHERE prid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("prcabang")
                lokasi = dtNomorNext.Rows(0)("prlokasi")
                sumber = dtNomorNext.Rows(0)("prsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("prautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("prnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("prtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M4_Pr_Detail WHERE idpr ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M4_Pr WHERE prid ='" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_PrSearch(PostWsSearch(paramSplit(0), "M4_PrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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