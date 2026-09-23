Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_rq
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_RqSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

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
        'rqid(0) As Integer, rqcabang(1) As String, rqlokasi(2) As String, rqgudang(3) As String, rqasalbarang(4) As String, 
        'rqasalbarangkategori(5) As Integer, rqjenispembelian(6) As String, rqjenispembeliankategori(7) As Integer, rqcarabayar(8) As Integer, rqsumber(9) As String, 
        'rqautonogrup(10) As Integer, rqnogrup(11) As String, rqautonotransaksi(12) As Integer, rqnotransaksi(13) As String, rqtgl(14) As Date, 
        'rqkodepa(15) As Integer, rqsupplier(16) As Integer, rqsupplierkontak(17) As String, rq1alamat1(18) As String, rq1alamat2(19) As String, 
        'rq1alamat3(20) As String, rq2alamat1(21) As String, rq2alamat2(22) As String, rq2alamat3(23) As String, rqbagianpembelian(24) As Integer, 
        'rqtgldipenuhi(25) As Date, rqtermin(26) As String, rqtgljatuhtempo(27) As Date, rquraian(28) As String, rqcatatan(29) As String, 
        'rqnoref(30) As String, rqtglnoref(31) As Date, rqtglpenutupan(32) As Date, rqmatauang(33) As String, rqkurs(34) As Double, 
        'rqhargatermasukpajak(35) As Integer, rqtotal(36) As Double, rqdiskonpersen(37) As String, rqdiskon(38) As Double, rqtotalpajak1detail(39) As Double, 
        'rqtotalpajak2detail(40) As Double, rqbiayalainpersen(41) As String, rqbiayalain(42) As Double, rqtotaltransaksi(43) As Double, rqidpr(44) As Integer, 
        'rqidcs(45) As Integer, rqstatuspo(46) As Integer, rqstatusipc(47) As Integer, rqstatusgrn(48) As Integer, rqstatusri(49) As Integer, 
        'rqstatusdnr(50) As Integer, rqstatusprt(51) As Integer, rqstatus(52) As Integer, rqstatussebelumnya(53) As Integer, rqjmlrevisi(54) As Integer, 
        'rqcetakanke(55) As Integer, rqinputuser(56) As Integer, rqinputtgl(57) As DateTime, rqmodifikasiuser(58) As Integer, rqmodifikasitgl(59) As DateTime, 
        'rqisclose(60) As Integer, rqcustomtext1(61) As String, rqcustomtext2(62) As String, rqcustomtext3(63) As String, rqcustomtext4(64) As String, 
        'rqcustomtext5(65) As String, rqcustomint1(66) As Integer, rqcustomint2(67) As Integer, rqcustomint3(68) As Integer, rqcustomdbl1(69) As Double, 
        'rqcustomdbl2(70) As Double, rqcustomdbl3(71) As Double, rqcustomdate1(72) As Date, rqcustomdate2(73) As Date, rqcustomdate3(74) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'rqid, rqcabang, rqlokasi, rqgudang, rqasalbarang, rqasalbarangkategori, rqjenispembelian, 
        'rqjenispembeliankategori, rqcarabayar, rqsumber, rqautonogrup, rqnogrup, rqautonotransaksi, rqnotransaksi, 
        'rqtgl, rqkodepa, rqsupplier, rqsupplierkontak, rq1alamat1, rq1alamat2, rq1alamat3, 
        'rq2alamat1, rq2alamat2, rq2alamat3, rqbagianpembelian, rqtgldipenuhi, rqtermin, rqtgljatuhtempo, 
        'rquraian, rqcatatan, rqnoref, rqtglnoref, rqtglpenutupan, rqmatauang, rqkurs, 
        'rqhargatermasukpajak, rqtotal, rqdiskonpersen, rqdiskon, rqtotalpajak1detail, rqtotalpajak2detail, rqbiayalainpersen, 
        'rqbiayalain, rqtotaltransaksi, rqidpr, rqidcs, rqstatuspo, rqstatusipc, rqstatusgrn, 
        'rqstatusri, rqstatusdnr, rqstatusprt, rqstatus, rqstatussebelumnya, rqjmlrevisi, rqcetakanke, 
        'rqinputuser, rqinputtgl, rqmodifikasiuser, rqmodifikasitgl, rqisclose, rqcustomtext1, rqcustomtext2, 
        'rqcustomtext3, rqcustomtext4, rqcustomtext5, rqcustomint1, rqcustomint2, rqcustomint3, rqcustomdbl1, 
        'rqcustomdbl2, rqcustomdbl3, rqcustomdate1, rqcustomdate2, rqcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 75) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'rqid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "rqid required numeric." : GoTo selesai
        End If
        'rqasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "rqasalbarangkategori required numeric." : GoTo selesai
        End If
        'rqjenispembeliankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "rqjenispembeliankategori required numeric." : GoTo selesai
        End If
        'rqcarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "rqcarabayar required numeric." : GoTo selesai
        End If
        'rqautonogrup(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "rqautonogrup required numeric." : GoTo selesai
        End If
        'rqautonotransaksi(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "rqautonotransaksi required numeric." : GoTo selesai
        End If
        'rqtgl(14) As Date
        If (IsDate(dataUtama(14)) = False) Then
            result(2) = "rqtgl required date." : GoTo selesai
        End If
        'rqkodepa(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "rqkodepa required numeric." : GoTo selesai
        End If
        'rqsupplier(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "rqsupplier required numeric." : GoTo selesai
        End If
        If (dataUtama(16) < 1) Then
            result(2) = "rqsupplier can't be empty." : GoTo selesai
        End If
        'rqbagianpembelian(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "rqbagianpembelian required numeric." : GoTo selesai
        End If
        'rqtgldipenuhi(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "rqtgldipenuhi required date." : GoTo selesai
        End If
        'rqtgljatuhtempo(27) As Date
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "rqtgljatuhtempo required date." : GoTo selesai
        End If
        'rqtglnoref(31) As Date
        If (IsDate(dataUtama(31)) = False) Then
            result(2) = "rqtglnoref required date." : GoTo selesai
        End If
        'rqtglpenutupan(32) As Date
        If (IsDate(dataUtama(32)) = False) Then
            result(2) = "rqtglpenutupan required date." : GoTo selesai
        End If
        'rqkurs(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "rqkurs required numeric." : GoTo selesai
        End If
        'rqhargatermasukpajak(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "rqhargatermasukpajak required numeric." : GoTo selesai
        End If
        'rqtotal(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "rqtotal required numeric." : GoTo selesai
        End If
        'rqdiskon(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "rqdiskon required numeric." : GoTo selesai
        End If
        'rqtotalpajak1detail(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "rqtotalpajak1detail required numeric." : GoTo selesai
        End If
        'rqtotalpajak2detail(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "rqtotalpajak2detail required numeric." : GoTo selesai
        End If
        'rqbiayalain(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "rqbiayalain required numeric." : GoTo selesai
        End If
        'rqtotaltransaksi(43) As Double
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "rqtotaltransaksi required numeric." : GoTo selesai
        End If
        'rqidpr(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "rqidpr required numeric." : GoTo selesai
        End If
        'rqidcs(45) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "rqidcs required numeric." : GoTo selesai
        End If
        'rqstatuspo(46) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "rqstatuspo required numeric." : GoTo selesai
        End If
        'rqstatusipc(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "rqstatusipc required numeric." : GoTo selesai
        End If
        'rqstatusgrn(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "rqstatusgrn required numeric." : GoTo selesai
        End If
        'rqstatusri(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "rqstatusri required numeric." : GoTo selesai
        End If
        'rqstatusdnr(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "rqstatusdnr required numeric." : GoTo selesai
        End If
        'rqstatusprt(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "rqstatusprt required numeric." : GoTo selesai
        End If
        'rqstatus(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "rqstatus required numeric." : GoTo selesai
        End If
        'rqstatussebelumnya(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "rqstatussebelumnya required numeric." : GoTo selesai
        End If
        'rqjmlrevisi(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "rqjmlrevisi required numeric." : GoTo selesai
        End If
        'rqcetakanke(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "rqcetakanke required numeric." : GoTo selesai
        End If
        'rqinputuser(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "rqinputuser required numeric." : GoTo selesai
        End If
        'rqinputtgl(57) As DateTime
        If (IsDate(dataUtama(57)) = False) Then
            result(2) = "rqinputtgl required date." : GoTo selesai
        End If
        'rqmodifikasiuser(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "rqmodifikasiuser required numeric." : GoTo selesai
        End If
        'rqmodifikasitgl(59) As DateTime
        If (IsDate(dataUtama(59)) = False) Then
            result(2) = "rqmodifikasitgl required date." : GoTo selesai
        End If
        'rqisclose(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "rqisclose required numeric." : GoTo selesai
        End If
        'rqcustomint1(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "rqcustomint1 required numeric." : GoTo selesai
        End If
        'rqcustomint2(67) As Integer
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "rqcustomint2 required numeric." : GoTo selesai
        End If
        'rqcustomint3(68) As Integer
        If (IsNumeric(dataUtama(68)) = False) Then
            result(2) = "rqcustomint3 required numeric." : GoTo selesai
        End If
        'rqcustomdbl1(69) As Double
        If (IsNumeric(dataUtama(69)) = False) Then
            result(2) = "rqcustomdbl1 required numeric." : GoTo selesai
        End If
        'rqcustomdbl2(70) As Double
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "rqcustomdbl2 required numeric." : GoTo selesai
        End If
        'rqcustomdbl3(71) As Double
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "rqcustomdbl3 required numeric." : GoTo selesai
        End If
        'rqcustomdate1(72) As Date
        If (IsDate(dataUtama(72)) = False) Then
            result(2) = "rqcustomdate1 required date." : GoTo selesai
        End If
        'rqcustomdate2(73) As Date
        If (IsDate(dataUtama(73)) = False) Then
            result(2) = "rqcustomdate2 required date." : GoTo selesai
        End If
        'rqcustomdate3(74) As Date
        If (IsDate(dataUtama(74)) = False) Then
            result(2) = "rqcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'rqcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "rqcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "rqcabang should not be more than 25 character." : GoTo selesai
        End If

        'rqlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "rqlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "rqlokasi should not be more than 25 character." : GoTo selesai
        End If

        'rqgudang(3) As String
        'If Len(dataUtama(3)) = 0 Then
        '    result(2) = "rqgudang can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "rqgudang should not be more than 25 character." : GoTo selesai
        End If

        'rqsumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "rqsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "rqsumber should not be more than 10 character." : GoTo selesai
        End If

        'rqnotransaksi(13) As String
        If Len(dataUtama(13)) = 0 Then
            result(2) = "rqnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(13)) > 50 Then
            result(2) = "rqnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'rqtgl(14) As Date
        If Len(dataUtama(14)) = 0 Then
            result(2) = "rqtgl can't be empty" : GoTo selesai
        End If

        'rqtgldipenuhi(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "rqtgldipenuhi can't be empty" : GoTo selesai
        End If

        'rqtgljatuhtempo(27) As Date
        If Len(dataUtama(27)) = 0 Then
            result(2) = "rqtgljatuhtempo can't be empty" : GoTo selesai
        End If

        'rqtglnoref(31) As Date
        If Len(dataUtama(31)) = 0 Then
            result(2) = "rqtglnoref can't be empty" : GoTo selesai
        End If

        'rqtglpenutupan(32) As Date
        If Len(dataUtama(32)) = 0 Then
            result(2) = "rqtglpenutupan can't be empty" : GoTo selesai
        End If

        'rqmatauang(33) As String
        If Len(dataUtama(33)) = 0 Then
            result(2) = "rqmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(33)) > 25 Then
            result(2) = "rqmatauang should not be more than 25 character." : GoTo selesai
        End If

        'rqkurs(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "rqkurs can't be empty" : GoTo selesai
        End If

        'rqtotal(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "rqtotal can't be empty" : GoTo selesai
        End If

        'rqdiskonpersen(37) As String
        If Len(dataUtama(37)) = 0 Then
            result(2) = "rqdiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(37)) > 25 Then
            result(2) = "rqdiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'rqdiskon(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "rqdiskon can't be empty" : GoTo selesai
        End If

        'rqtotalpajak1detail(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "rqtotalpajak1detail can't be empty" : GoTo selesai
        End If

        'rqtotalpajak2detail(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "rqtotalpajak2detail can't be empty" : GoTo selesai
        End If

        'rqbiayalainpersen(41) As String
        If Len(dataUtama(41)) = 0 Then
            result(2) = "rqbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(41)) > 25 Then
            result(2) = "rqbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'rqbiayalain(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "rqbiayalain can't be empty" : GoTo selesai
        End If

        'rqtotaltransaksi(43) As Double
        If Len(dataUtama(43)) = 0 Then
            result(2) = "rqtotaltransaksi can't be empty" : GoTo selesai
        End If

        'rqinputtgl(57) As DateTime
        If Len(dataUtama(57)) = 0 Then
            result(2) = "rqinputtgl can't be empty" : GoTo selesai
        End If

        'rqmodifikasitgl(59) As DateTime
        If Len(dataUtama(59)) = 0 Then
            result(2) = "rqmodifikasitgl can't be empty" : GoTo selesai
        End If

        'rqcustomdbl1(69) As Double
        If Len(dataUtama(69)) = 0 Then
            result(2) = "rqcustomdbl1 can't be empty" : GoTo selesai
        End If

        'rqcustomdbl2(70) As Double
        If Len(dataUtama(70)) = 0 Then
            result(2) = "rqcustomdbl2 can't be empty" : GoTo selesai
        End If

        'rqcustomdbl3(71) As Double
        If Len(dataUtama(71)) = 0 Then
            result(2) = "rqcustomdbl3 can't be empty" : GoTo selesai
        End If

        'rqcustomdate1(72) As Date
        If Len(dataUtama(72)) = 0 Then
            result(2) = "rqcustomdate1 can't be empty" : GoTo selesai
        End If

        'rqcustomdate2(73) As Date
        If Len(dataUtama(73)) = 0 Then
            result(2) = "rqcustomdate2 can't be empty" : GoTo selesai
        End If

        'rqcustomdate3(74) As Date
        If Len(dataUtama(74)) = 0 Then
            result(2) = "rqcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "rqid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqjenispembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqjenispembeliankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqautonogrup", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqnogrup", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqsupplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqsupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rq1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rq1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rq1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rq2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rq2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rq2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqbagianpembelian", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqtgldipenuhi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqtermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqtgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rquraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqtglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqhargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqtotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqdiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqtotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqtotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqtotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqidpr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqidcs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqstatuspo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqstatusipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqstatusgrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqstatusri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqstatusdnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqstatusprt", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "rqid~rqcabang~rqlokasi~rqgudang~rqasalbarang~rqasalbarangkategori~rqjenispembelian~rqjenispembeliankategori~rqcarabayar~rqsumber~rqautonogrup~rqnogrup~rqautonotransaksi~rqnotransaksi~rqtgl~rqkodepa~rqsupplier~rqsupplierkontak~rq1alamat1~rq1alamat2~rq1alamat3~rq2alamat1~rq2alamat2~rq2alamat3~rqbagianpembelian~rqtgldipenuhi~rqtermin~rqtgljatuhtempo~rquraian~rqcatatan~rqnoref~rqtglnoref~rqtglpenutupan~rqmatauang~rqkurs~rqhargatermasukpajak~rqtotal~rqdiskonpersen~rqdiskon~rqtotalpajak1detail~rqtotalpajak2detail~rqbiayalainpersen~rqbiayalain~rqtotaltransaksi~rqidpr~rqidcs~rqstatuspo~rqstatusipc~rqstatusgrn~rqstatusri~rqstatusdnr~rqstatusprt~rqstatus~rqstatussebelumnya~rqjmlrevisi~rqcetakanke~rqinputuser~rqinputtgl~rqmodifikasiuser~rqmodifikasitgl~rqisclose~rqcustomtext1~rqcustomtext2~rqcustomtext3~rqcustomtext4~rqcustomtext5~rqcustomint1~rqcustomint2~rqcustomint3~rqcustomdbl1~rqcustomdbl2~rqcustomdbl3~rqcustomdate1~rqcustomdate2~rqcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idrqdetail(0) As Integer, idrq(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, diskon(13) As String, jmldiskon(14) As Double, 
        'pajak1(15) As String, jmlpajak1(16) As Double, pajak2(17) As String, jmlpajak2(18) As Double, cabang(19) As String, 
        'lokasi(20) As String, gudang(21) As String, costcenter(22) As String, divisi(23) As String, subdivisi(24) As String, 
        'proyek(25) As String, catatan(26) As String, urutan(27) As Integer, idprdetail(28) As Integer, idcsdetail(29) As Integer, 
        'jmlpo(30) As Double, statuspo(31) As Integer, jmlipc(32) As Double, statusipc(33) As Integer, jmlgrn(34) As Double, 
        'statusgrn(35) As Integer, jmlri(36) As Double, statusri(37) As Integer, jmldnr(38) As Double, statusdnr(39) As Integer, 
        'jmlprt(40) As Double, statusprt(41) As Integer, isclose(42) As Integer, customtext1(43) As String, customtext2(44) As String, 
        'customtext3(45) As String, customdbl1(46) As Double, customdbl2(47) As Double, customdbl3(48) As Double, customdate1(49) As Date, 
        'customdate2(50) As Date, customdate3(51) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idrqdetail, idrq, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, 
        'jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, 
        'gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idprdetail, idcsdetail, jmlpo, statuspo, jmlipc, statusipc, jmlgrn, 
        'statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idrqdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idrq", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idprdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idcsdetail", AsEnumTypeData.AsInt64)
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
        Dim idbarang As Integer = 0, idprdetail As Integer = 0, jmlbarang As Double = 0

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 52) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idrqdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idrqdetail required numeric." : GoTo selesai
            End If
            'idrq(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idrq required numeric." : GoTo selesai
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
            'urutan(27) As Integer
            If (IsNumeric(dataRowDetail(27)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idprdetail(28) As Integer
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - idprdetail required numeric." : GoTo selesai
            End If
            'idcsdetail(29) As Integer
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - idcsdetail required numeric." : GoTo selesai
            End If
            'jmlpo(30) As Double
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - jmlpo required numeric." : GoTo selesai
            End If
            'statuspo(31) As Integer
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - statuspo required numeric." : GoTo selesai
            End If
            'jmlipc(32) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - jmlipc required numeric." : GoTo selesai
            End If
            'statusipc(33) As Integer
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - statusipc required numeric." : GoTo selesai
            End If
            'jmlgrn(34) As Double
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - jmlgrn required numeric." : GoTo selesai
            End If
            'statusgrn(35) As Integer
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - statusgrn required numeric." : GoTo selesai
            End If
            'jmlri(36) As Double
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - jmlri required numeric." : GoTo selesai
            End If
            'statusri(37) As Integer
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - statusri required numeric." : GoTo selesai
            End If
            'jmldnr(38) As Double
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - jmldnr required numeric." : GoTo selesai
            End If
            'statusdnr(39) As Integer
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - statusdnr required numeric." : GoTo selesai
            End If
            'jmlprt(40) As Double
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - jmlprt required numeric." : GoTo selesai
            End If
            'statusprt(41) As Integer
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - statusprt required numeric." : GoTo selesai
            End If
            'isclose(42) As Integer
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(46) As Double
            If (IsNumeric(dataRowDetail(46)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(47) As Double
            If (IsNumeric(dataRowDetail(47)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(48) As Double
            If (IsNumeric(dataRowDetail(48)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(49) As Date
            If (IsDate(dataRowDetail(49)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(50) As Date
            If (IsDate(dataRowDetail(50)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(51) As Date
            If (IsDate(dataRowDetail(51)) = False) Then
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
            'If dataRowDetail(12) <= 0 Then
            '    result(2) = "Row : " & i & " - harga can't be less than or equal to zero" : GoTo selesai
            'End If

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

            'jmlpo(30) As Double
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Row : " & i & " - jmlpo can't be empty" : GoTo selesai
            End If

            'jmlipc(32) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - jmlipc can't be empty" : GoTo selesai
            End If

            'jmlgrn(34) As Double
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Row : " & i & " - jmlgrn can't be empty" : GoTo selesai
            End If

            'jmlri(36) As Double
            If Len(dataRowDetail(36)) = 0 Then
                result(2) = "Row : " & i & " - jmlri can't be empty" : GoTo selesai
            End If

            'jmldnr(38) As Double
            If Len(dataRowDetail(38)) = 0 Then
                result(2) = "Row : " & i & " - jmldnr can't be empty" : GoTo selesai
            End If

            'jmlprt(40) As Double
            If Len(dataRowDetail(40)) = 0 Then
                result(2) = "Row : " & i & " - jmlprt can't be empty" : GoTo selesai
            End If

            'customdbl1(46) As Double
            If Len(dataRowDetail(46)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(47) As Double
            If Len(dataRowDetail(47)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(48) As Double
            If Len(dataRowDetail(48)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(49) As Date
            If Len(dataRowDetail(49)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(50) As Date
            If Len(dataRowDetail(50)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(51) As Date
            If Len(dataRowDetail(51)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idrqdetail~idrq~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~idprdetail~idcsdetail~jmlpo~statuspo~jmlipc~statusipc~jmlgrn~statusgrn~jmlri~statusri~jmldnr~statusdnr~jmlprt~statusprt~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , idprdetail(28) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : idprdetail = dataRowDetail(28)

            'VALIDASI OUTSTANDING -------------------------
            If idprdetail <> 0 Then
                '1. CEK DATA EXIST ------------------------
                ftExistOutstanding = IIf(Len(ftExistOutstanding.ToString) = 0, "", ftExistOutstanding & " UNION ")
                ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM M4_pr_detail JOIN M4_pr ON idpr = prid WHERE idprdetail = '" & idprdetail & "' AND (prstatus = 2 OR prstatus = 3 OR prstatus = 4 OR prstatus = 7) LIMIT 1) as rowExists, '" & idprdetail & "' as idprdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. SET NILAI UPDATE OUTSTANDING ----------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idprdetail=" & idprdetail)
                updNilai = String.Concat("WHEN '" & idprdetail & "' THEN ROUND(jmlrq + '" & Outstanding & "', 5) ", updNilai)

                '3. SET FILTER UPDATE OUTSTANDING ---------
                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                updFilter = String.Concat(updFilter, "(idprdetail = '" & idprdetail & "')")
            End If
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'SIMPAN KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0, autoNogrupOld As String = ""

        Try
            'Proses utama
            If (dtutama.Rows.Count > 0) Then
                Dim drutama As DataRow = dtutama.Rows(0)


                'CEK HAK AKSES STATUS ============================
                Dim vAkses As Integer = 0, msgAkses As String = ""
                'MODUL DAN MENU HARUS DISESUAIKAN
                Dim vModuleId As Integer = 4, vMenuId As Integer = 5
                Select Case drutama("rqstatus")
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("rqtgl")), AsFormatTanggal(drutama("rqtgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================

                Dim prTglAwal As String = "1971-01-01 00:00:00", prTglAkhir As String = "1971-01-01 00:00:00"
                Dim vStatusLelang As String = FixDouble(drutama("rqcustomint1"))

                If vStatusLelang <> 0 Or isUpdate Then
                    'CEK BISA INSERT/UPDATE ATAU TIDAK
                    'BERDASARKAN HAK AKSES CUSTOM BISA TRANSAKSI RQ DILUAR TGL AWAL DAN TGL AKHIR PR ATAU TIDAK
                    sql = "SELECT pr.prtglawal, pr.prtglakhir, (CASE IFNULL(pr.prid,0) WHEN 0 THEN 1 ELSE (CASE IFNULL(rc.rcakses,1) WHEN 0 THEN (CASE WHEN NOW() >= pr.prtglawal AND NOW() <= pr.prtglakhir THEN 1 ELSE 0 END) ELSE 1 END) END) as editable FROM m0_userlogin ul JOIN m0_user u ON ul.uluser = u.userid AND ul.ulid = '" & FixQuotes(paramSplit(0)) & "' JOIN m0_user_role ur ON u.userid = ur.userid LEFT JOIN m0_role_custom rc ON ur.role = rc.rcrole and rc.rcmoduleid = 4 and rc.rcidpc = 4 LEFT JOIN m4_pr pr ON pr.prid = '" & FixDouble(FxDB(drutama("rqidpr"), 0)) & "' GROUP BY pr.prid"
                    'sql = "SELECT pr.prtglawal, pr.prtglakhir, (CASE IFNULL(pr.prid,0) WHEN 0 THEN 1 ELSE (CASE IFNULL(rc.rcakses,0) WHEN 0 THEN (CASE WHEN NOW() >= pr.prtglawal AND NOW() <= pr.prtglakhir THEN 1 ELSE 0 END) ELSE 1 END) END) as editable FROM m0_userlogin ul JOIN m0_user u ON ul.uluser = u.userid AND ul.ulid = '" & FixQuotes(paramSplit(0)) & "' JOIN m0_user_role ur ON u.userid = ur.userid LEFT JOIN m0_role_custom rc ON ur.role = rc.rcrole and rc.rcmoduleid = 4 and rc.rcidpc = 4 LEFT JOIN m4_pr pr ON pr.prid = '" & FixDouble(FxDB(drutama("rqidpr"), 0)) & "' GROUP BY pr.prid"
                    Dim dtCustomRQ As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtCustomRQ.Rows.Count > 0 Then

                        prTglAwal = AsFormatTanggal(FxDB(dtCustomRQ.Rows(0)("prtglawal"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss")
                        prTglAkhir = AsFormatTanggal(FxDB(dtCustomRQ.Rows(0)("prtglakhir"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss")

                        If FxDB(dtCustomRQ.Rows(0)("editable"), 1) = 0 Then
                            result(2) = "Can't insert/update RQ outside " & AsFormatTanggal(prTglAwal, "dd/MM/yyyy H:mm:ss") & " and " & AsFormatTanggal(prTglAkhir, "dd/MM/yyyy H:mm:ss") : Trans.Rollback() : GoTo selesai
                        End If
                    End If
                End If


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("rqstatus") = 2 Or drutama("rqstatus") = 1 Or drutama("rqstatus") = 8 Or drutama("rqstatus") = 9 Or drutama("rqstatus") = 10 Or drutama("rqstatus") = 11 Then
                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstanding)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai

                    'JIKA APPROVED MAKA STATUS LELANG MENANG
                    vStatusLelang = 2
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("rqtermin").ToString, AsFormatTanggal(drutama("rqtgl")), "rqtgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("rqtgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TOTAL = subtotal
                drutama("rqtotal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("rqtotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("rqtotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                If Integer.Parse(drutama("rqhargatermasukpajak")) = 0 Then
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                    drutama("rqtotaltransaksi") = Double.Parse(drutama("rqtotal")) - Double.Parse(drutama("rqdiskon")) + Double.Parse(drutama("rqtotalpajak1detail")) + Double.Parse(drutama("rqtotalpajak2detail")) + Double.Parse(drutama("rqbiayalain"))

                Else
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                    drutama("rqtotaltransaksi") = Double.Parse(drutama("rqtotal")) - Double.Parse(drutama("rqdiskon")) + Double.Parse(drutama("rqtotalpajak2detail")) + Double.Parse(drutama("rqbiayalain"))

                End If
                'END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("rqid")
                    notransaksi = drutama("rqnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(rqid), rqnotransaksi, rqautonogrup FROM M4_rq WHERE rqid='" & result(4) & "' AND rqstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)
                    autoNogrupOld = dtupdate.Rows(0)(2)

                    If (rowUpdate > 0) Then

                        If drutama("rqautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rqcabang"), drutama("rqlokasi"), drutama("rqsumber"), drutama("rqtgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(rqid) FROM m4_rq WHERE rqnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_rq_history
                        Dim rsSimpanHistory As String = SimpanHistory.M4_Rq_HistorySimpan("" & paramSplit(0) & "★M4_Rq_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("rqsumber")) & "▼" & FixQuotes(drutama("rqid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        'GENERATE NOGRUP BARU JIKA AUTONOGRUP LAMA = 0 DAN AUTONOGRUP BARU = 1
                        If drutama("rqautonogrup") = "1" And autoNogrupOld = "0" Then
                            'GENERATE NOGRUP =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNogrup As String = wsM0_Nomor.M0_NogrupRQ(drutama("rqcabang"), drutama("rqlokasi"), drutama("rqtgl"))
                            Dim arrNogrup(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
                            arrNogrup = rsNogrup.Split(sptSubParam)
                            'cek success generate notransaksi
                            If (arrNogrup(0) = 1) Then
                                nogrup = arrNogrup(2)
                                'tambah query update m0_nomor_next
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
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
                            nogrup = drutama("rqnogrup")
                        End If


                        'SET STATUS LELANG, JIKA TGL EDIT DIANTARA TGL AWAL DAN TGL AKHIR PR MAKA STATUS LELANG = PROSES
                        Dim tglSekarang As String = AsFormatTanggal(Date.Now, "yyyy-MM-dd H:mm:ss")
                        If vStatusLelang <> 2 And vStatusLelang <> 4 And tglSekarang >= prTglAwal And tglSekarang <= prTglAkhir Then
                            vStatusLelang = 1
                        End If

                        sql = "Update M4_Rq set rqcabang  = '" & FixQuotes(drutama("rqcabang")) & "', rqlokasi  = '" & FixQuotes(drutama("rqlokasi")) & "', rqgudang  = '" & FixQuotes(drutama("rqgudang")) & "', rqasalbarang  = '" & FixQuotes(drutama("rqasalbarang")) & "', rqasalbarangkategori  = " & drutama("rqasalbarangkategori") & ", rqjenispembelian  = '" & FixQuotes(drutama("rqjenispembelian")) & "', rqjenispembeliankategori  = " & drutama("rqjenispembeliankategori") & ", rqcarabayar  = " & drutama("rqcarabayar") & ", rqsumber  = '" & FixQuotes(drutama("rqsumber")) & "', rqautonogrup  = " & drutama("rqautonogrup") & ", rqnogrup  = '" & FixQuotes(nogrup) & "', rqautonotransaksi  = " & drutama("rqautonotransaksi") & ", rqnotransaksi  = '" & notransaksi & "', rqtgl  = '" & FixQuotes(AsFormatTanggal(drutama("rqtgl"))) & "', rqkodepa  = " & drutama("rqkodepa") & ", rqsupplier  = " & drutama("rqsupplier") & ", rqsupplierkontak  = '" & FixQuotes(drutama("rqsupplierkontak")) & "', rq1alamat1  = '" & FixQuotes(drutama("rq1alamat1")) & "', rq1alamat2  = '" & FixQuotes(drutama("rq1alamat2")) & "', rq1alamat3  = '" & FixQuotes(drutama("rq1alamat3")) & "', rq2alamat1  = '" & FixQuotes(drutama("rq2alamat1")) & "', rq2alamat2  = '" & FixQuotes(drutama("rq2alamat2")) & "', rq2alamat3  = '" & FixQuotes(drutama("rq2alamat3")) & "', rqbagianpembelian  = " & drutama("rqbagianpembelian") & ", rqtgldipenuhi  = '" & FixQuotes(AsFormatTanggal(drutama("rqtgldipenuhi"))) & "', rqtermin  = '" & FixQuotes(drutama("rqtermin")) & "', rqtgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("rqtgljatuhtempo"))) & "', rquraian  = '" & FixQuotes(drutama("rquraian")) & "', rqcatatan  = '" & FixQuotes(drutama("rqcatatan")) & "', rqnoref  = '" & FixQuotes(drutama("rqnoref")) & "', rqtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("rqtglnoref"))) & "', rqtglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("rqtglpenutupan"))) & "', rqmatauang  = '" & FixQuotes(drutama("rqmatauang")) & "', rqkurs  = '" & FixDouble(drutama("rqkurs")) & "', rqhargatermasukpajak  = " & drutama("rqhargatermasukpajak") & ", rqtotal  = '" & FixDouble(drutama("rqtotal")) & "', rqdiskonpersen  = '" & FixQuotes(drutama("rqdiskonpersen")) & "', rqdiskon  = '" & FixDouble(drutama("rqdiskon")) & "', rqtotalpajak1detail  = '" & FixDouble(drutama("rqtotalpajak1detail")) & "', rqtotalpajak2detail  = '" & FixDouble(drutama("rqtotalpajak2detail")) & "', rqbiayalainpersen  = '" & FixQuotes(drutama("rqbiayalainpersen")) & "', rqbiayalain  = '" & FixDouble(drutama("rqbiayalain")) & "', rqtotaltransaksi  = '" & FixDouble(drutama("rqtotaltransaksi")) & "', rqidpr  = " & drutama("rqidpr") & ", rqidcs  = " & drutama("rqidcs") & ", rqstatuspo  = " & drutama("rqstatuspo") & ", rqstatusipc  = " & drutama("rqstatusipc") & ", rqstatusgrn  = " & drutama("rqstatusgrn") & ", rqstatusri  = " & drutama("rqstatusri") & ", rqstatusdnr  = " & drutama("rqstatusdnr") & ", rqstatusprt  = " & drutama("rqstatusprt") & ", rqstatus  = " & drutama("rqstatus") & ", rqstatussebelumnya  = " & drutama("rqstatussebelumnya") & ", rqjmlrevisi  = rqjmlrevisi+1, rqcetakanke  = " & drutama("rqcetakanke") & ", rqmodifikasiuser  = " & drutama("rqmodifikasiuser") & ", rqmodifikasitgl  = NOW(), rqcustomtext1  = '" & FixQuotes(drutama("rqcustomtext1")) & "', rqcustomtext2  = '" & FixQuotes(drutama("rqcustomtext2")) & "', rqcustomtext3  = '" & FixQuotes(drutama("rqcustomtext3")) & "', rqcustomtext4  = '" & FixQuotes(drutama("rqcustomtext4")) & "', rqcustomtext5  = '" & FixQuotes(drutama("rqcustomtext5")) & "', rqcustomint1  = " & vStatusLelang & ", rqcustomint2  = " & drutama("rqcustomint2") & ", rqcustomint3  = " & drutama("rqcustomint3") & ", rqcustomdbl1  = '" & FixDouble(drutama("rqcustomdbl1")) & "', rqcustomdbl2  = '" & FixDouble(drutama("rqcustomdbl2")) & "', rqcustomdbl3  = '" & FixDouble(drutama("rqcustomdbl3")) & "', rqcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("rqcustomdate1"))) & "', rqcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("rqcustomdate2"))) & "', rqcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("rqcustomdate3"))) & "' where rqid = '" & drutama("rqid") & "'"
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

                    If drutama("rqautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rqcabang"), drutama("rqlokasi"), drutama("rqsumber"), drutama("rqtgl"))
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
                        notransaksi = drutama("rqnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(rqid) FROM m4_rq WHERE rqnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============



                    If drutama("rqautonogrup") = 1 Then
                        'GENERATE NOGRUP =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNogrup As String = wsM0_Nomor.M0_NogrupRQ(drutama("rqcabang"), drutama("rqlokasi"), drutama("rqtgl"))
                        Dim arrNogrup(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
                        arrNogrup = rsNogrup.Split(sptSubParam)
                        'cek success generate notransaksi
                        If (arrNogrup(0) = 1) Then
                            nogrup = arrNogrup(2)
                            'tambah query update m0_nomor_next
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
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
                        nogrup = drutama("rqnogrup")
                    End If

                    sql = "Insert into M4_Rq (rqcabang, rqlokasi, rqgudang, rqasalbarang, rqasalbarangkategori, rqjenispembelian, rqjenispembeliankategori, rqcarabayar, rqsumber, rqautonogrup, rqnogrup, rqautonotransaksi, rqnotransaksi, rqtgl, rqkodepa, rqsupplier, rqsupplierkontak, rq1alamat1, rq1alamat2, rq1alamat3, rq2alamat1, rq2alamat2, rq2alamat3, rqbagianpembelian, rqtgldipenuhi, rqtermin, rqtgljatuhtempo, rquraian, rqcatatan, rqnoref, rqtglnoref, rqtglpenutupan, rqmatauang, rqkurs, rqhargatermasukpajak, rqtotal, rqdiskonpersen, rqdiskon, rqtotalpajak1detail, rqtotalpajak2detail, rqbiayalainpersen, rqbiayalain, rqtotaltransaksi, rqidpr, rqidcs, rqstatuspo, rqstatusipc, rqstatusgrn, rqstatusri, rqstatusdnr, rqstatusprt, rqstatus, rqstatussebelumnya, rqjmlrevisi, rqcetakanke, rqinputuser, rqinputtgl, rqmodifikasiuser, rqmodifikasitgl, rqisclose, rqcustomtext1, rqcustomtext2, rqcustomtext3, rqcustomtext4, rqcustomtext5, rqcustomint1, rqcustomint2, rqcustomint3, rqcustomdbl1, rqcustomdbl2, rqcustomdbl3, rqcustomdate1, rqcustomdate2, rqcustomdate3) values('" & FixQuotes(drutama("rqcabang")) & "', '" & FixQuotes(drutama("rqlokasi")) & "', '" & FixQuotes(drutama("rqgudang")) & "', '" & FixQuotes(drutama("rqasalbarang")) & "', " & drutama("rqasalbarangkategori") & ", '" & FixQuotes(drutama("rqjenispembelian")) & "', " & drutama("rqjenispembeliankategori") & ", " & drutama("rqcarabayar") & ", '" & FixQuotes(drutama("rqsumber")) & "', " & drutama("rqautonogrup") & ", '" & FixQuotes(nogrup) & "', " & drutama("rqautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("rqtgl"))) & "', " & drutama("rqkodepa") & ", " & drutama("rqsupplier") & ", '" & FixQuotes(drutama("rqsupplierkontak")) & "', '" & FixQuotes(drutama("rq1alamat1")) & "', '" & FixQuotes(drutama("rq1alamat2")) & "', '" & FixQuotes(drutama("rq1alamat3")) & "', '" & FixQuotes(drutama("rq2alamat1")) & "', '" & FixQuotes(drutama("rq2alamat2")) & "', '" & FixQuotes(drutama("rq2alamat3")) & "', " & drutama("rqbagianpembelian") & ", '" & FixQuotes(AsFormatTanggal(drutama("rqtgldipenuhi"))) & "', '" & FixQuotes(drutama("rqtermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rqtgljatuhtempo"))) & "', '" & FixQuotes(drutama("rquraian")) & "', '" & FixQuotes(drutama("rqcatatan")) & "', '" & FixQuotes(drutama("rqnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rqtglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rqtglpenutupan"))) & "', '" & FixQuotes(drutama("rqmatauang")) & "', '" & FixDouble(drutama("rqkurs")) & "', " & drutama("rqhargatermasukpajak") & ", '" & FixDouble(drutama("rqtotal")) & "', '" & FixQuotes(drutama("rqdiskonpersen")) & "', '" & FixDouble(drutama("rqdiskon")) & "', '" & FixDouble(drutama("rqtotalpajak1detail")) & "', '" & FixDouble(drutama("rqtotalpajak2detail")) & "', '" & FixQuotes(drutama("rqbiayalainpersen")) & "', '" & FixDouble(drutama("rqbiayalain")) & "', '" & FixDouble(drutama("rqtotaltransaksi")) & "', " & drutama("rqidpr") & ", " & drutama("rqidcs") & ", " & drutama("rqstatuspo") & ", " & drutama("rqstatusipc") & ", " & drutama("rqstatusgrn") & ", " & drutama("rqstatusri") & ", " & drutama("rqstatusdnr") & ", " & drutama("rqstatusprt") & ", " & drutama("rqstatus") & ", " & drutama("rqstatussebelumnya") & ", " & drutama("rqjmlrevisi") & ", " & drutama("rqcetakanke") & ", " & drutama("rqinputuser") & ", NOW(), " & drutama("rqmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("rqisclose") & ", '" & FixQuotes(drutama("rqcustomtext1")) & "', '" & FixQuotes(drutama("rqcustomtext2")) & "', '" & FixQuotes(drutama("rqcustomtext3")) & "', '" & FixQuotes(drutama("rqcustomtext4")) & "', '" & FixQuotes(drutama("rqcustomtext5")) & "', " & drutama("rqcustomint1") & ", " & drutama("rqcustomint2") & ", " & drutama("rqcustomint3") & ", '" & FixDouble(drutama("rqcustomdbl1")) & "', '" & FixDouble(drutama("rqcustomdbl2")) & "', '" & FixDouble(drutama("rqcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rqcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rqcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rqcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select rqid from M4_rq where rqnotransaksi='" & notransaksi & "' AND rqinputuser= '" & userid & "' order by rqmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'UPDATE RANKING BERDASARKAN TOTAL TRANSAKSI TERENDAH DENGAN ID PR YANG SAMA
                sql = "SELECT rk.rqid, @rq_ranking := @rq_ranking + 1 as nourut FROM (SELECT rq.rqid FROM m4_rq rq WHERE rq.rqidpr = '" & FixDouble(drutama("rqidpr")) & "' AND rq.rqidpr <> 0 ORDER BY rq.rqtotaltransaksi ASC) as rk, (SELECT @rq_ranking := 0) AS variableInit "
                Dim dtRanking As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                If dtRanking.Rows.Count > 0 Then
                    Dim strValue As New StringBuilder
                    For Each dr1 As DataRow In dtRanking.Rows
                        strValue.Append(" WHEN " & FixDouble(FxDB(dr1("rqid"), 0)) & " THEN " & FixDouble(FxDB(dr1("nourut"), 0)))
                    Next
                    If Len(strValue.ToString) > 0 Then
                        sql = "UPDATE m4_rq SET rqcustomint2 = (CASE rqid " & strValue.ToString & " ELSE 0 END) WHERE rqidpr = '" & FixDouble(drutama("rqidpr")) & "'"
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


                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Rq_Detail where idrq = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idrqdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idprdetail") & ", " & dr1("idcsdetail") & ", '" & FixDouble(dr1("jmlpo")) & "', " & dr1("statuspo") & ", '" & FixDouble(dr1("jmlipc")) & "', " & dr1("statusipc") & ", '" & FixDouble(dr1("jmlgrn")) & "', " & dr1("statusgrn") & ", '" & FixDouble(dr1("jmlri")) & "', " & dr1("statusri") & ", '" & FixDouble(dr1("jmldnr")) & "', " & dr1("statusdnr") & ", '" & FixDouble(dr1("jmlprt")) & "', " & dr1("statusprt") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Rq_Detail(idrqdetail, idrq, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, jmlpo, statuspo, jmlipc, statusipc, jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                If drutama("rqstatus") = 2 Then

                    'UPDATE RQ LAIN MENJADI KALAH, REJECT, UNTUK PR YANG SAMA
                    sql = "UPDATE m4_rq SET rqcustomint1 = 3, rqstatus = 6 WHERE rqid <> '" & result(4) & "' AND rqidpr = '" & drutama("rqidpr") & "' AND rqstatus NOT IN(2,3,4,7) AND rqcustomint1 NOT IN(2, 4)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()


                    If Len(updNilai) > 0 Then
                        'UPDATE OUTSTANDING TRANSAKSI =======================================================
                        'UPDATE DETAIL
                        sql = "UPDATE M4_pr_detail SET jmlrq = (CASE idprdetail " & updNilai & " ELSE jmlrq END) WHERE " & updFilter
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idpr FROM M4_pr_detail WHERE " & updFilter & " GROUP BY idpr", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idpr = '" & dr1("idpr") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idpr, SUM(jmlbarang) as jmlbarang, SUM(jmlrq) as jmlrq FROM M4_pr_detail WHERE " & ftDetail & " GROUP BY idpr", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilai = "" : updFilter = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlrq") >= dr1("jmlbarang") Then
                                    statusOut = 2
                                ElseIf dr1("jmlrq") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilai = String.Concat(updNilai, "WHEN '" & dr1("idpr") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                                updFilter = String.Concat(updFilter, "(prid = '" & dr1("idpr") & "')")
                            Next

                            sql = "UPDATE M4_pr SET prstatusrq = (CASE prid " & updNilai & " ELSE prstatusrq END) WHERE " & updFilter
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
                End If

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "RQ", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M4_RqUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("rqsupplierkode", "c1.kkode")
            Filter = Filter.Replace("rqsuppliernama", "c1.knama")
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
            Dim sumber As String = "Rq", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Rqtgl, Rqnotransaksi, Rqstatus FROM M4_Rq WHERE Rqid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Rqstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m4_rq_history
            Dim rsSimpanHistory As String = SimpanHistory.M4_Rq_HistorySimpan("" & paramSplit(0) & "★M4_Rq_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m4_rq_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================


                'AMBIL IDPR
                Dim idpr As Double = 0
                Dim dtPR As DataTable = AsDataTableAmbilDariDBCon("SELECT rqidpr FROM m4_rq WHERE rqid = '" & FixDouble(idtransaksi) & "'", myConn)
                If dtPR.Rows.Count > 0 Then
                    idpr = FixDouble(FxDB(dtPR.Rows(0)("rqidpr"), 0))
                End If

                'CEK BISA INSERT/UPDATE ATAU TIDAK
                'BERDASARKAN HAK AKSES CUSTOM BISA TRANSAKSI RQ DILUAR TGL AWAL DAN TGL AKHIR PR ATAU TIDAK
                sql = "SELECT pr.prtglawal, pr.prtglakhir, (CASE IFNULL(pr.prid,0) WHEN 0 THEN 1 ELSE (CASE IFNULL(rc.rcakses,1) WHEN 0 THEN (CASE WHEN NOW() >= pr.prtglawal AND NOW() <= pr.prtglakhir THEN 1 ELSE 0 END) ELSE 1 END) END) as editable FROM m0_userlogin ul JOIN m0_user u ON ul.uluser = u.userid AND ul.ulid = '" & FixQuotes(paramSplit(0)) & "' JOIN m0_user_role ur ON u.userid = ur.userid LEFT JOIN m0_role_custom rc ON ur.role = rc.rcrole and rc.rcmoduleid = 4 and rc.rcidpc = 4 LEFT JOIN m4_pr pr ON pr.prid = '" & FixDouble(FxDB(idpr, 0)) & "' GROUP BY pr.prid"
                Dim dtCustomRQ As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                If dtCustomRQ.Rows.Count > 0 Then
                    If FxDB(dtCustomRQ.Rows(0)("editable"), 1) = 0 Then
                        result(2) = "Can't insert/update RQ outside " & AsFormatTanggal(dtCustomRQ.Rows(0)("prtglawal"), "dd/MM/yyyy H:mm:ss") & " and " & AsFormatTanggal(dtCustomRQ.Rows(0)("prtglakhir"), "dd/MM/yyyy H:mm:ss")
                    End If
                End If


                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idprdetail As Integer = 0
                Dim ftOutstanding As String = "", updNilai As String = "", updFilter As String = ""
                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idprdetail, urutan FROM M4_rq_detail WHERE idrq = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : idprdetail = dr1("idprdetail")

                        'UPDATE OUTSTANDING ---------------------------
                        If idprdetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING ----------
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idprdetail=" & idprdetail)
                            updNilai = String.Concat("WHEN '" & idprdetail & "' THEN ROUND(jmlrq - '" & Outstanding & "', 5) ", updNilai)
                            '2. SET FILTERUPDATE OUTSTANDING ----------
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(idprdetail = '" & idprdetail & "')")
                        End If
                        'END OF BUAT FILTER UNTUK UPDATE --------------------------
                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                If Len(updFilter) > 0 Then
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE M4_pr_detail SET jmlrq = (CASE idprdetail " & updNilai & " ELSE jmlrq END) WHERE " & updFilter
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idpr FROM M4_pr_detail WHERE " & updFilter & " GROUP BY idpr", myConn)
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idpr = '" & dr1("idpr") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDBCon("SELECT idpr, SUM(jmlbarang) as jmlbarang, SUM(jmlrq) as jmlrq FROM M4_pr_detail WHERE " & ftDetail & " GROUP BY idpr", myConn)
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilai = "" : updFilter = ""
                        For Each dr1 As DataRow In dtOut.Rows
                            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                            If dr1("jmlrq") >= dr1("jmlbarang") Then
                                statusOut = 2
                            ElseIf dr1("jmlrq") < 1 Then
                                statusOut = 0
                            Else
                                statusOut = 1
                            End If
                            '2. SET NILAI UPDATE OUTSTANDING
                            updNilai = String.Concat(updNilai, "WHEN '" & dr1("idpr") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(prid = '" & dr1("idpr") & "')")
                        Next

                        sql = "UPDATE M4_pr SET prstatusrq = (CASE prid " & updNilai & " ELSE prstatusrq END) WHERE " & updFilter
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
            End If

            'update status utama
            sql = "UPDATE M4_Rq SET Rqstatus = " & nilaiStatus & ", Rqmodifikasiuser='" & userid & "', Rqmodifikasitgl = NOW(), Rqposting = 0, Rqpostingtgl = '1971-01-01 00:00:00', Rqjmlrevisi = Rqjmlrevisi + 1 WHERE Rqid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_RqSearch(PostWsSearch(paramSplit(0), "M4_RqSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_RqDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("rqsupplierkode", "c1.kkode")
            Filter = Filter.Replace("rqsuppliernama", "c1.knama")
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
            Dim sumber As String = "Rq", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Rqid, Rqnotransaksi FROM M4_Rq WHERE Rqid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT rqcabang, rqlokasi, rqsumber, rqautonotransaksi, rqnotransaksi, rqtgl"
            sql &= " FROM M4_rq"
            sql &= " WHERE rqid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("rqcabang")
                lokasi = dtNomorNext.Rows(0)("rqlokasi")
                sumber = dtNomorNext.Rows(0)("rqsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("rqautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("rqnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("rqtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M4_Rq_Detail WHERE idrq = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M4_Rq WHERE rqid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_RqSearch(PostWsSearch(paramSplit(0), "M4_RqSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_RqGetdataById(ByVal param As String) As String

        'M4_RqGetdataById Utama --------------------------------------------------------
        'rqid, rqcabang, rqlokasi, rqgudang, rqasalbarang, rqasalbarangkategori, rqjenispembelian, 
        'rqjenispembeliankategori, rqcarabayar, rqsumber, rqautonogrup, rqnogrup, rqautonotransaksi, rqnotransaksi, 
        'rqtgl, rqkodepa, rqsupplier, rqsupplierkontak, rq1alamat1, rq1alamat2, rq1alamat3, 
        'rq2alamat1, rq2alamat2, rq2alamat3, rqbagianpembelian, rqtgldipenuhi, rqtermin, rqtgljatuhtempo, 
        'rquraian, rqcatatan, rqnoref, rqtglnoref, rqtglpenutupan, rqmatauang, rqkurs, 
        'rqhargatermasukpajak, rqtotal, rqdiskonpersen, rqdiskon, rqtotalpajak1detail, rqtotalpajak2detail, rqbiayalainpersen, 
        'rqbiayalain, rqtotaltransaksi, rqidpr, rqidcs, rqstatuspo, rqstatusipc, rqstatusgrn, 
        'rqstatusri, rqstatusdnr, rqstatusprt, rqstatusrealisasi, rqstatus, rqstatussebelumnya, rqjmlrevisi, 
        'rqcetakanke, rqinputuser, rqinputtgl, rqmodifikasiuser, rqmodifikasitgl, rqposting, rqpostingtgl, 
        'rqisclose, rqcustomtext1, rqcustomtext2, rqcustomtext3, rqcustomtext4, rqcustomtext5, rqcustomint1, 
        'rqcustomint2, rqcustomint3, rqcustomdbl1, rqcustomdbl2, rqcustomdbl3, rqcustomdate1, rqcustomdate2, 
        'rqcustomdate3, rqcabangnama, rqlokasinama, rqgudangnama, rqsupplierkode, rqsuppliernama, rqbagianpembeliankode, 
        'rqbagianpembeliannama, rqterminnama, rqtermindiskon1, rqterminharidiskon1, rqtermindiskon2, rqterminharidiskon2, rqtermindenda, 
        'rqtermindendaper, rqterminharijatuhtempo, rqnotransaksipr, rqnotransaksics, rqstatusnama, rqstatussebelumnyanama, rqinputusernama, 
        'rqmodifikasiusernama, kpkp

        'M4_RqGetdataById Detail -------------------------------------------------------
        'idrqdetail, idrq, idbarang, namabarang, tipebarang, jml, 
        'satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idprdetail, idcsdetail, jmlpo, statuspo, jmlipc, statusipc, 
        'jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, 
        'statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, 
        'pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, 
        'costcenternama, divisinama, subdivisinama, proyeknama, prnotransaksi, csnotransaksi, jmlsisapo, 
        'jmlsisarealisasi

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

        Dim NmMemcached As String = "aplikasi1-M4_Rq~M4_Rq_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "rqid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "rqid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m4_rq_getdata")
        sql = "select `rq`.`rqid` AS `rqid`,`rq`.`rqcabang` AS `rqcabang`,`rq`.`rqlokasi` AS `rqlokasi`,`rq`.`rqgudang` AS `rqgudang`,`rq`.`rqasalbarang` AS `rqasalbarang`,`rq`.`rqasalbarangkategori` AS `rqasalbarangkategori`,`rq`.`rqjenispembelian` AS `rqjenispembelian`,`rq`.`rqjenispembeliankategori` AS `rqjenispembeliankategori`,`rq`.`rqcarabayar` AS `rqcarabayar`,`rq`.`rqsumber` AS `rqsumber`,`rq`.`rqautonogrup` AS `rqautonogrup`,`rq`.`rqnogrup` AS `rqnogrup`,`rq`.`rqautonotransaksi` AS `rqautonotransaksi`,`rq`.`rqnotransaksi` AS `rqnotransaksi`,`rq`.`rqtgl` AS `rqtgl`,`rq`.`rqkodepa` AS `rqkodepa`,`rq`.`rqsupplier` AS `rqsupplier`,`rq`.`rqsupplierkontak` AS `rqsupplierkontak`,`rq`.`rq1alamat1` AS `rq1alamat1`,`rq`.`rq1alamat2` AS `rq1alamat2`,`rq`.`rq1alamat3` AS `rq1alamat3`,`rq`.`rq2alamat1` AS `rq2alamat1`,`rq`.`rq2alamat2` AS `rq2alamat2`,`rq`.`rq2alamat3` AS `rq2alamat3`,`rq`.`rqbagianpembelian` AS `rqbagianpembelian`,`rq`.`rqtgldipenuhi` AS `rqtgldipenuhi`,`rq`.`rqtermin` AS `rqtermin`,`rq`.`rqtgljatuhtempo` AS `rqtgljatuhtempo`,`rq`.`rquraian` AS `rquraian`,`rq`.`rqcatatan` AS `rqcatatan`,`rq`.`rqnoref` AS `rqnoref`,`rq`.`rqtglnoref` AS `rqtglnoref`,`rq`.`rqtglpenutupan` AS `rqtglpenutupan`,`rq`.`rqmatauang` AS `rqmatauang`,`rq`.`rqkurs` AS `rqkurs`,`rq`.`rqhargatermasukpajak` AS `rqhargatermasukpajak`,`rq`.`rqtotal` AS `rqtotal`,`rq`.`rqdiskonpersen` AS `rqdiskonpersen`,`rq`.`rqdiskon` AS `rqdiskon`,`rq`.`rqtotalpajak1detail` AS `rqtotalpajak1detail`,`rq`.`rqtotalpajak2detail` AS `rqtotalpajak2detail`,`rq`.`rqbiayalainpersen` AS `rqbiayalainpersen`,`rq`.`rqbiayalain` AS `rqbiayalain`,`rq`.`rqtotaltransaksi` AS `rqtotaltransaksi`,`rq`.`rqidpr` AS `rqidpr`,`rq`.`rqidcs` AS `rqidcs`,`rq`.`rqstatuspo` AS `rqstatuspo`,`rq`.`rqstatusipc` AS `rqstatusipc`,`rq`.`rqstatusgrn` AS `rqstatusgrn`,`rq`.`rqstatusri` AS `rqstatusri`,`rq`.`rqstatusdnr` AS `rqstatusdnr`,`rq`.`rqstatusprt` AS `rqstatusprt`,`rq`.`rqstatusrealisasi` AS `rqstatusrealisasi`,`rq`.`rqstatus` AS `rqstatus`,`rq`.`rqstatussebelumnya` AS `rqstatussebelumnya`,`rq`.`rqjmlrevisi` AS `rqjmlrevisi`,`rq`.`rqcetakanke` AS `rqcetakanke`,`rq`.`rqinputuser` AS `rqinputuser`,`rq`.`rqinputtgl` AS `rqinputtgl`,`rq`.`rqmodifikasiuser` AS `rqmodifikasiuser`,`rq`.`rqmodifikasitgl` AS `rqmodifikasitgl`,`rq`.`rqposting` AS `rqposting`,`rq`.`rqpostingtgl` AS `rqpostingtgl`,`rq`.`rqisclose` AS `rqisclose`,`rq`.`rqcustomtext1` AS `rqcustomtext1`,`rq`.`rqcustomtext2` AS `rqcustomtext2`,`rq`.`rqcustomtext3` AS `rqcustomtext3`,`rq`.`rqcustomtext4` AS `rqcustomtext4`,`rq`.`rqcustomtext5` AS `rqcustomtext5`,`rq`.`rqcustomint1` AS `rqcustomint1`,`rq`.`rqcustomint2` AS `rqcustomint2`,`rq`.`rqcustomint3` AS `rqcustomint3`,`rq`.`rqcustomdbl1` AS `rqcustomdbl1`,`rq`.`rqcustomdbl2` AS `rqcustomdbl2`,`rq`.`rqcustomdbl3` AS `rqcustomdbl3`,`rq`.`rqcustomdate1` AS `rqcustomdate1`,`rq`.`rqcustomdate2` AS `rqcustomdate2`,`rq`.`rqcustomdate3` AS `rqcustomdate3`,`br`.`bnama` AS `rqcabangnama`,`lc`.`lnama` AS `rqlokasinama`,`wh`.`wnama` AS `rqgudangnama`,`c1`.`kkode` AS `rqsupplierkode`,`c1`.`knama` AS `rqsuppliernama`,`c2`.`kkode` AS `rqbagianpembeliankode`,`c2`.`knama` AS `rqbagianpembeliannama`,`tr`.`trnama` AS `rqterminnama`,`tr`.`trdiskon1` AS `rqtermindiskon1`,`tr`.`trharidiskon1` AS `rqterminharidiskon1`,`tr`.`trdiskon2` AS `rqtermindiskon2`,`tr`.`trharidiskon2` AS `rqterminharidiskon2`,`tr`.`trdenda` AS `rqtermindenda`,`tr`.`trdendaper` AS `rqtermindendaper`,`tr`.`trharijatuhtempo` AS `rqterminharijatuhtempo`,`pr`.`prnotransaksi` AS `rqnotransaksipr`,`cs`.`csnotransaksi` AS `rqnotransaksics`,`st1`.`nama` AS `rqstatusnama`,`st2`.`nama` AS `rqstatussebelumnyanama`,`u1`.`unama` AS `rqinputusernama`,`u2`.`unama` AS `rqmodifikasiusernama`,`rqd`.`idrqdetail` AS `idrqdetail`,`rqd`.`idrq` AS `idrq`,`rqd`.`idbarang` AS `idbarang`,`rqd`.`namabarang` AS `namabarang`,`rqd`.`tipebarang` AS `tipebarang`,`rqd`.`jml` AS `jml`,`rqd`.`satuan` AS `satuan`,`rqd`.`nilaisatuan` AS `nilaisatuan`,`rqd`.`jmlbarang` AS `jmlbarang`,`rqd`.`satuanbarang` AS `satuanbarang`,`rqd`.`matauang` AS `matauang`,`rqd`.`kurs` AS `kurs`,`rqd`.`harga` AS `harga`,`rqd`.`diskon` AS `diskon`,`rqd`.`jmldiskon` AS `jmldiskon`,`rqd`.`pajak1` AS `pajak1`,`rqd`.`jmlpajak1` AS `jmlpajak1`,`rqd`.`pajak2` AS `pajak2`,`rqd`.`jmlpajak2` AS `jmlpajak2`,`rqd`.`cabang` AS `cabang`,`rqd`.`lokasi` AS `lokasi`,`rqd`.`gudang` AS `gudang`,`rqd`.`costcenter` AS `costcenter`,`rqd`.`divisi` AS `divisi`,`rqd`.`subdivisi` AS `subdivisi`,`rqd`.`proyek` AS `proyek`,`rqd`.`catatan` AS `catatan`,`rqd`.`urutan` AS `urutan`,`rqd`.`idprdetail` AS `idprdetail`,`rqd`.`idcsdetail` AS `idcsdetail`,`rqd`.`jmlpo` AS `jmlpo`,`rqd`.`statuspo` AS `statuspo`,`rqd`.`jmlipc` AS `jmlipc`,`rqd`.`statusipc` AS `statusipc`,`rqd`.`jmlgrn` AS `jmlgrn`,`rqd`.`statusgrn` AS `statusgrn`,`rqd`.`jmlri` AS `jmlri`,`rqd`.`statusri` AS `statusri`,`rqd`.`jmldnr` AS `jmldnr`,`rqd`.`statusdnr` AS `statusdnr`,`rqd`.`jmlprt` AS `jmlprt`,`rqd`.`statusprt` AS `statusprt`,`rqd`.`jmlrealisasi` AS `jmlrealisasi`,`rqd`.`statusrealisasi` AS `statusrealisasi`,`rqd`.`isclose` AS `isclose`,`rqd`.`customtext1` AS `customtext1`,`rqd`.`customtext2` AS `customtext2`,`rqd`.`customtext3` AS `customtext3`,`rqd`.`customdbl1` AS `customdbl1`,`rqd`.`customdbl2` AS `customdbl2`,`rqd`.`customdbl3` AS `customdbl3`,`rqd`.`customdate1` AS `customdate1`,`rqd`.`customdate2` AS `customdate2`,`rqd`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,`brd`.`bnama` AS `cabangnama`,`lcd`.`lnama` AS `lokasinama`,`whd`.`wnama` AS `gudangnama`,`cc`.`ccnama` AS `costcenternama`,`d`.`dnama` AS `divisinama`,`sd`.`sdnama` AS `subdivisinama`,`p`.`pnama` AS `proyeknama`,`pr2`.`prnotransaksi` AS `prnotransaksi`,`cs2`.`csnotransaksi` AS `csnotransaksi`,((`rqd`.`jmlbarang` - `rqd`.`jmlpo`) / `rqd`.`nilaisatuan`) AS `jmlsisapo`,((`rqd`.`jmlbarang` - `rqd`.`jmlrealisasi`) / `rqd`.`nilaisatuan`) AS `jmlsisarealisasi`, c1.kpkp from (((((((((((((((((((((((((((`m4_rq` `rq` join `m4_rq_detail` `rqd` on((`rq`.`rqid` = `rqd`.`idrq`))) left join `m1_branch` `br` on((`br`.`bkode` = `rq`.`rqcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `rq`.`rqlokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `rq`.`rqgudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `rq`.`rqsupplier`))) left join `m1_contact` `c2` on((`c2`.`kid` = `rq`.`rqbagianpembelian`))) left join `m1_terms` `tr` on((`rq`.`rqtermin` = `tr`.`trkode`))) left join `m4_pr` `pr` on((`rq`.`rqidpr` = `pr`.`prid`))) left join `m4_cs` `cs` on((`rq`.`rqidcs` = `cs`.`csid`))) left join `m0_status` `st1` on((`st1`.`kode` = `rq`.`rqstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `rq`.`rqstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `rq`.`rqinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `rq`.`rqmodifikasiuser`))) left join `m1_item` `i` on((`i`.`bid` = `rqd`.`idbarang`))) left join `m1_tax` `t1` on((`rqd`.`pajak1` = `t1`.`tkode`))) left join `m1_tax` `t2` on((`rqd`.`pajak2` = `t2`.`tkode`))) left join `m1_branch` `brd` on((`rqd`.`cabang` = `brd`.`bkode`))) left join `m1_location` `lcd` on((`rqd`.`lokasi` = `lcd`.`lkode`))) left join `m1_warehouse` `whd` on((`rqd`.`gudang` = `whd`.`wkode`))) left join `m1_cost_center` `cc` on((`rqd`.`costcenter` = `cc`.`cckode`))) left join `m1_division` `d` on((`rqd`.`divisi` = `d`.`dkode`))) left join `m1_subdivision` `sd` on((`rqd`.`subdivisi` = `sd`.`sdkode`))) left join `m1_project` `p` on((`rqd`.`proyek` = `p`.`pkode`))) left join `m4_pr_detail` `prd` on((`rqd`.`idprdetail` = `prd`.`idprdetail`))) left join `m4_pr` `pr2` on((`prd`.`idpr` = `pr2`.`prid`))) left join `m4_cs_detail` `csd` on((`rqd`.`idcsdetail` = `csd`.`idcsdetail`))) left join `m4_cs` `cs2` on((`csd`.`idcs` = `cs2`.`csid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("rqid"), 0), sptField,
                     FxDB(drutama("rqcabang"), ""), sptField,
                     FxDB(drutama("rqlokasi"), ""), sptField,
                     FxDB(drutama("rqgudang"), ""), sptField,
                     FxDB(drutama("rqasalbarang"), ""), sptField,
                     FxDB(drutama("rqasalbarangkategori"), 0), sptField,
                     FxDB(drutama("rqjenispembelian"), ""), sptField,
                     FxDB(drutama("rqjenispembeliankategori"), 0), sptField,
                     FxDB(drutama("rqcarabayar"), 0), sptField,
                     FxDB(drutama("rqsumber"), ""), sptField,
                     FxDB(drutama("rqautonogrup"), 0), sptField,
                     FxDB(drutama("rqnogrup"), ""), sptField,
                     FxDB(drutama("rqautonotransaksi"), 0), sptField,
                     FxDB(drutama("rqnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rqtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("rqkodepa"), 0), sptField,
                     FxDB(drutama("rqsupplier"), 0), sptField,
                     FxDB(drutama("rqsupplierkontak"), ""), sptField,
                     FxDB(drutama("rq1alamat1"), ""), sptField,
                     FxDB(drutama("rq1alamat2"), ""), sptField,
                     FxDB(drutama("rq1alamat3"), ""), sptField,
                     FxDB(drutama("rq2alamat1"), ""), sptField,
                     FxDB(drutama("rq2alamat2"), ""), sptField,
                     FxDB(drutama("rq2alamat3"), ""), sptField,
                     FxDB(drutama("rqbagianpembelian"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rqtgldipenuhi"), ""), formatTgl), sptField,
                     FxDB(drutama("rqtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rqtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("rquraian"), ""), sptField,
                     FxDB(drutama("rqcatatan"), ""), sptField,
                     FxDB(drutama("rqnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rqtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rqtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("rqmatauang"), ""), sptField,
                     FxDB(drutama("rqkurs"), 0), sptField,
                     FxDB(drutama("rqhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("rqtotal"), 0), sptField,
                     FxDB(drutama("rqdiskonpersen"), ""), sptField,
                     FxDB(drutama("rqdiskon"), 0), sptField,
                     FxDB(drutama("rqtotalpajak1detail"), 0), sptField,
                     FxDB(drutama("rqtotalpajak2detail"), 0), sptField,
                     FxDB(drutama("rqbiayalainpersen"), ""), sptField,
                     FxDB(drutama("rqbiayalain"), 0), sptField,
                     FxDB(drutama("rqtotaltransaksi"), 0), sptField,
                     FxDB(drutama("rqidpr"), 0), sptField,
                     FxDB(drutama("rqidcs"), 0), sptField,
                     FxDB(drutama("rqstatuspo"), 0), sptField,
                     FxDB(drutama("rqstatusipc"), 0), sptField,
                     FxDB(drutama("rqstatusgrn"), 0), sptField,
                     FxDB(drutama("rqstatusri"), 0), sptField,
                     FxDB(drutama("rqstatusdnr"), 0), sptField,
                     FxDB(drutama("rqstatusprt"), 0), sptField,
                     FxDB(drutama("rqstatusrealisasi"), 0), sptField,
                     FxDB(drutama("rqstatus"), 0), sptField,
                     FxDB(drutama("rqstatussebelumnya"), 0), sptField,
                     FxDB(drutama("rqjmlrevisi"), 0), sptField,
                     FxDB(drutama("rqcetakanke"), 0), sptField,
                     FxDB(drutama("rqinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rqinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rqmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rqmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rqposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rqpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rqisclose"), 0), sptField,
                     FxDB(drutama("rqcustomtext1"), ""), sptField,
                     FxDB(drutama("rqcustomtext2"), ""), sptField,
                     FxDB(drutama("rqcustomtext3"), ""), sptField,
                     FxDB(drutama("rqcustomtext4"), ""), sptField,
                     FxDB(drutama("rqcustomtext5"), ""), sptField,
                     FxDB(drutama("rqcustomint1"), 0), sptField,
                     FxDB(drutama("rqcustomint2"), 0), sptField,
                     FxDB(drutama("rqcustomint3"), 0), sptField,
                     FxDB(drutama("rqcustomdbl1"), 0), sptField,
                     FxDB(drutama("rqcustomdbl2"), 0), sptField,
                     FxDB(drutama("rqcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rqcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rqcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rqcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("rqcabangnama"), ""), sptField,
                     FxDB(drutama("rqlokasinama"), ""), sptField,
                     FxDB(drutama("rqgudangnama"), ""), sptField,
                     FxDB(drutama("rqsupplierkode"), ""), sptField,
                     FxDB(drutama("rqsuppliernama"), ""), sptField,
                     FxDB(drutama("rqbagianpembeliankode"), ""), sptField,
                     FxDB(drutama("rqbagianpembeliannama"), ""), sptField,
                     FxDB(drutama("rqterminnama"), ""), sptField,
                     FxDB(drutama("rqtermindiskon1"), 0), sptField,
                     FxDB(drutama("rqterminharidiskon1"), 0), sptField,
                     FxDB(drutama("rqtermindiskon2"), 0), sptField,
                     FxDB(drutama("rqterminharidiskon2"), 0), sptField,
                     FxDB(drutama("rqtermindenda"), 0), sptField,
                     FxDB(drutama("rqtermindendaper"), 0), sptField,
                     FxDB(drutama("rqterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("rqnotransaksipr"), ""), sptField,
                     FxDB(drutama("rqnotransaksics"), ""), sptField,
                     FxDB(drutama("rqstatusnama"), ""), sptField,
                     FxDB(drutama("rqstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("rqinputusernama"), ""), sptField,
                     FxDB(drutama("rqmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idrqdetail"), 0), sptField,
                     FxDB(dr("idrq"), 0), sptField,
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
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idprdetail"), 0), sptField,
                     FxDB(dr("idcsdetail"), 0), sptField,
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
                     FxDB(dr("cabangnama"), ""), sptField,
                     FxDB(dr("lokasinama"), ""), sptField,
                     FxDB(dr("gudangnama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("prnotransaksi"), ""), sptField,
                     FxDB(dr("csnotransaksi"), ""), sptField,
                     FxDB(dr("jmlsisapo"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptRow)

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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rqid, rqcabang, rqlokasi, rqgudang, rqasalbarang, rqasalbarangkategori, rqjenispembelian, rqjenispembeliankategori, rqcarabayar, rqsumber, rqautonogrup, rqnogrup, rqautonotransaksi, rqnotransaksi, rqtgl, rqkodepa, rqsupplier, rqsupplierkontak, rq1alamat1, rq1alamat2, rq1alamat3, rq2alamat1, rq2alamat2, rq2alamat3, rqbagianpembelian, rqtgldipenuhi, rqtermin, rqtgljatuhtempo, rquraian, rqcatatan, rqnoref, rqtglnoref, rqtglpenutupan, rqmatauang, rqkurs, rqhargatermasukpajak, rqtotal, rqdiskonpersen, rqdiskon, rqtotalpajak1detail, rqtotalpajak2detail, rqbiayalainpersen, rqbiayalain, rqtotaltransaksi, rqidpr, rqidcs, rqstatuspo, rqstatusipc, rqstatusgrn, rqstatusri, rqstatusdnr, rqstatusprt, rqstatusrealisasi, rqstatus, rqstatussebelumnya, rqjmlrevisi, rqcetakanke, rqinputuser, rqinputtgl, rqmodifikasiuser, rqmodifikasitgl, rqposting, rqpostingtgl, rqisclose, rqcustomtext1, rqcustomtext2, rqcustomtext3, rqcustomtext4, rqcustomtext5, rqcustomint1, rqcustomint2, rqcustomint3, rqcustomdbl1, rqcustomdbl2, rqcustomdbl3, rqcustomdate1, rqcustomdate2, rqcustomdate3, rqcabangnama, rqlokasinama, rqgudangnama, rqsupplierkode, rqsuppliernama, rqbagianpembeliankode, rqbagianpembeliannama, rqterminnama, rqtermindiskon1, rqterminharidiskon1, rqtermindiskon2, rqterminharidiskon2, rqtermindenda, rqtermindendaper, rqterminharijatuhtempo, rqnotransaksipr, rqnotransaksics, rqstatusnama, rqstatussebelumnyanama, rqinputusernama, rqmodifikasiusernama, kpkp" & sptSubParam & "idrqdetail, idrq, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, jmlpo, statuspo, jmlipc, statusipc, jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, prnotransaksi, csnotransaksi, jmlsisapo, jmlsisarealisasi"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_RqSearch(ByVal param As String) As String
        'M4_RqSearch --------------------------------------------------------
        'rqid, rqcabang, rqlokasi, rqgudang, rqasalbarang, rqasalbarangkategori, rqjenispembelian, 
        'rqjenispembeliankategori, rqcarabayar, rqsumber, rqautonogrup, rqnogrup, rqautonotransaksi, rqnotransaksi, 
        'rqtgl, rqkodepa, rqsupplier, rqsupplierkontak, rq1alamat1, rq1alamat2, rq1alamat3, 
        'rq2alamat1, rq2alamat2, rq2alamat3, rqbagianpembelian, rqtgldipenuhi, rqtermin, rqtgljatuhtempo, 
        'rquraian, rqcatatan, rqnoref, rqtglnoref, rqtglpenutupan, rqmatauang, rqkurs, 
        'rqhargatermasukpajak, rqtotal, rqdiskonpersen, rqdiskon, rqtotalpajak1detail, rqtotalpajak2detail, rqbiayalainpersen, 
        'rqbiayalain, rqtotaltransaksi, rqidpr, rqidcs, rqstatuspo, rqstatusipc, rqstatusgrn, 
        'rqstatusri, rqstatusdnr, rqstatusprt, rqstatusrealisasi, rqstatus, rqstatussebelumnya, rqjmlrevisi, 
        'rqcetakanke, rqinputuser, rqinputtgl, rqmodifikasiuser, rqmodifikasitgl, rqposting, rqpostingtgl, 
        'rqisclose, rqcabangnama, rqlokasinama, rqgudangnama, rqsupplierkode, rqsuppliernama, rqbagianpembeliankode, 
        'rqbagianpembeliannama, prnotransaksi, csnotransaksi, rqstatusnama, rqstatussebelumnyanama, rqinputusernama, 
        'rqmodifikasiusernama, prtotaltransaksi, rqtgllelang, rqcustomint1, rqcustomint1nama, rqcustomint2, variasi

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
            Filter = pagingSplit(2) & " AND (CASE IFNULL(rc.rcakses,0) WHEN 0 THEN rq.rqsupplier = u.ukontak ELSE rq.rqsupplier LIKE '%' END)"
            '#Taruh fungsi replace disini...
        Else
            Filter = " (CASE IFNULL(rc.rcakses,0) WHEN 0 THEN rq.rqsupplier = u.ukontak ELSE rq.rqsupplier LIKE '%' END)"
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'Dim query As New m0_query
        'sql = query.PanggilQuery("m4_rq_v")
        'sql = "select rq.rqid AS rqid, rq.rqcabang AS rqcabang, rq.rqlokasi AS rqlokasi, rq.rqgudang AS rqgudang, rq.rqasalbarang AS rqasalbarang, rq.rqasalbarangkategori AS rqasalbarangkategori, rq.rqjenispembelian AS rqjenispembelian, rq.rqjenispembeliankategori AS rqjenispembeliankategori, rq.rqcarabayar AS rqcarabayar, rq.rqsumber AS rqsumber, rq.rqautonogrup AS rqautonogrup, rq.rqnogrup AS rqnogrup, rq.rqautonotransaksi AS rqautonotransaksi, rq.rqnotransaksi AS rqnotransaksi, rq.rqtgl AS rqtgl, rq.rqkodepa AS rqkodepa, rq.rqsupplier AS rqsupplier, rq.rqsupplierkontak AS rqsupplierkontak, rq.rq1alamat1 AS rq1alamat1, rq.rq1alamat2 AS rq1alamat2, rq.rq1alamat3 AS rq1alamat3, rq.rq2alamat1 AS rq2alamat1, rq.rq2alamat2 AS rq2alamat2, rq.rq2alamat3 AS rq2alamat3, rq.rqbagianpembelian AS rqbagianpembelian, rq.rqtgldipenuhi AS rqtgldipenuhi, rq.rqtermin AS rqtermin, rq.rqtgljatuhtempo AS rqtgljatuhtempo, rq.rquraian AS rquraian, rq.rqcatatan AS rqcatatan, rq.rqnoref AS rqnoref, rq.rqtglnoref AS rqtglnoref, rq.rqtglpenutupan AS rqtglpenutupan, rq.rqmatauang AS rqmatauang, rq.rqkurs AS rqkurs, rq.rqhargatermasukpajak AS rqhargatermasukpajak, rq.rqtotal AS rqtotal, rq.rqdiskonpersen AS rqdiskonpersen, rq.rqdiskon AS rqdiskon, rq.rqtotalpajak1detail AS rqtotalpajak1detail, rq.rqtotalpajak2detail AS rqtotalpajak2detail, rq.rqbiayalainpersen AS rqbiayalainpersen, rq.rqbiayalain AS rqbiayalain, rq.rqtotaltransaksi AS rqtotaltransaksi, rq.rqidpr AS rqidpr, rq.rqidcs AS rqidcs, rq.rqstatuspo AS rqstatuspo, rq.rqstatusipc AS rqstatusipc, rq.rqstatusgrn AS rqstatusgrn, rq.rqstatusri AS rqstatusri, rq.rqstatusdnr AS rqstatusdnr, rq.rqstatusprt AS rqstatusprt, rq.rqstatusrealisasi AS rqstatusrealisasi, rq.rqstatus AS rqstatus, rq.rqstatussebelumnya AS rqstatussebelumnya, rq.rqjmlrevisi AS rqjmlrevisi, rq.rqcetakanke AS rqcetakanke, rq.rqinputuser AS rqinputuser, rq.rqinputtgl AS rqinputtgl, rq.rqmodifikasiuser AS rqmodifikasiuser, rq.rqmodifikasitgl AS rqmodifikasitgl, rq.rqposting AS rqposting, rq.rqpostingtgl AS rqpostingtgl, rq.rqisclose AS rqisclose, br.bnama AS rqcabangnama, lc.lnama AS rqlokasinama, wh.wnama AS rqgudangnama, c1.kkode AS rqsupplierkode, c1.knama AS rqsuppliernama, c2.kkode AS rqbagianpembeliankode, c2.knama AS rqbagianpembeliannama, pr.prnotransaksi AS prnotransaksi, cs.csnotransaksi AS csnotransaksi, st1.nama AS rqstatusnama, st2.nama AS rqstatussebelumnyanama, u1.unama AS rqinputusernama, u2.unama AS rqmodifikasiusernama, pr.prnotransaksi, pr.prtotaltransaksi, pr.prtglawal, pr.prtglakhir, rq.rqcustomint1, srq.nama as rqcustomint1nama, rq.rqcustomint2 from m4_rq rq left join m1_branch br on br.bkode = rq.rqcabang left join m1_location lc on lc.lkode = rq.rqlokasi left join m1_warehouse wh on wh.wkode = rq.rqgudang left join m1_contact c1 on c1.kid = rq.rqsupplier left join m1_contact c2 on c2.kid = rq.rqbagianpembelian left join m4_pr pr on rq.rqidpr = pr.prid left join m4_cs cs on rq.rqidcs = cs.csid left join m0_status st1 on st1.kode = rq.rqstatus left join m0_status st2 on st2.kode = rq.rqstatussebelumnya left join m0_user u1 on u1.userid = rq.rqinputuser left join m0_user u2 on u2.userid = rq.rqmodifikasiuser left join m0_status_rq srq on rq.rqcustomint1 = srq.kode"
        sql = "select (pr.prtotaltransaksi - rq.rqtotaltransaksi) AS rqvariasi, rq.rqid AS rqid, rq.rqcabang AS rqcabang, rq.rqlokasi AS rqlokasi, rq.rqgudang AS rqgudang, rq.rqasalbarang AS rqasalbarang, rq.rqasalbarangkategori AS rqasalbarangkategori, rq.rqjenispembelian AS rqjenispembelian, rq.rqjenispembeliankategori AS rqjenispembeliankategori, rq.rqcarabayar AS rqcarabayar, rq.rqsumber AS rqsumber, rq.rqautonogrup AS rqautonogrup, rq.rqnogrup AS rqnogrup, rq.rqautonotransaksi AS rqautonotransaksi, rq.rqnotransaksi AS rqnotransaksi, rq.rqtgl AS rqtgl, rq.rqkodepa AS rqkodepa, rq.rqsupplier AS rqsupplier, rq.rqsupplierkontak AS rqsupplierkontak, rq.rq1alamat1 AS rq1alamat1, rq.rq1alamat2 AS rq1alamat2, rq.rq1alamat3 AS rq1alamat3, rq.rq2alamat1 AS rq2alamat1, rq.rq2alamat2 AS rq2alamat2, rq.rq2alamat3 AS rq2alamat3, rq.rqbagianpembelian AS rqbagianpembelian, rq.rqtgldipenuhi AS rqtgldipenuhi, rq.rqtermin AS rqtermin, rq.rqtgljatuhtempo AS rqtgljatuhtempo, rq.rquraian AS rquraian, rq.rqcatatan AS rqcatatan, rq.rqnoref AS rqnoref, rq.rqtglnoref AS rqtglnoref, rq.rqtglpenutupan AS rqtglpenutupan, rq.rqmatauang AS rqmatauang, rq.rqkurs AS rqkurs, rq.rqhargatermasukpajak AS rqhargatermasukpajak, rq.rqtotal AS rqtotal, rq.rqdiskonpersen AS rqdiskonpersen, rq.rqdiskon AS rqdiskon, rq.rqtotalpajak1detail AS rqtotalpajak1detail, rq.rqtotalpajak2detail AS rqtotalpajak2detail, rq.rqbiayalainpersen AS rqbiayalainpersen, rq.rqbiayalain AS rqbiayalain, rq.rqtotaltransaksi AS rqtotaltransaksi, rq.rqidpr AS rqidpr, rq.rqidcs AS rqidcs, rq.rqstatuspo AS rqstatuspo, rq.rqstatusipc AS rqstatusipc, rq.rqstatusgrn AS rqstatusgrn, rq.rqstatusri AS rqstatusri, rq.rqstatusdnr AS rqstatusdnr, rq.rqstatusprt AS rqstatusprt, rq.rqstatusrealisasi AS rqstatusrealisasi, rq.rqstatus AS rqstatus, rq.rqstatussebelumnya AS rqstatussebelumnya, rq.rqjmlrevisi AS rqjmlrevisi, rq.rqcetakanke AS rqcetakanke, rq.rqinputuser AS rqinputuser, rq.rqinputtgl AS rqinputtgl, rq.rqmodifikasiuser AS rqmodifikasiuser, rq.rqmodifikasitgl AS rqmodifikasitgl, rq.rqposting AS rqposting, rq.rqpostingtgl AS rqpostingtgl, rq.rqisclose AS rqisclose, br.bnama AS rqcabangnama, lc.lnama AS rqlokasinama, wh.wnama AS rqgudangnama, c1.kkode AS rqsupplierkode, c1.knama AS rqsuppliernama, c2.kkode AS rqbagianpembeliankode, c2.knama AS rqbagianpembeliannama, pr.prnotransaksi AS prnotransaksi, cs.csnotransaksi AS csnotransaksi, st1.nama AS rqstatusnama, st2.nama AS rqstatussebelumnyanama, u1.unama AS rqinputusernama, u2.unama AS rqmodifikasiusernama, pr.prnotransaksi, pr.prtotaltransaksi, pr.prtglawal, pr.prtglakhir, rq.rqcustomint1, srq.nama as rqcustomint1nama, rq.rqcustomint2 from m4_rq rq join m0_userlogin ul on ul.ulid = '" & FixQuotes(paramSplit(0)) & "' join m0_user_role ur on ul.uluser = ur.userid join m0_user u on ul.uluser = u.userid left join m0_role_custom rc on ur.role = rc.rcrole and rc.rcmoduleid = 4 and rc.rcidpc = 3 left join m1_branch br on br.bkode = rq.rqcabang left join m1_location lc on lc.lkode = rq.rqlokasi left join m1_warehouse wh on wh.wkode = rq.rqgudang left join m1_contact c1 on c1.kid = rq.rqsupplier left join m1_contact c2 on c2.kid = rq.rqbagianpembelian left join m4_pr pr on rq.rqidpr = pr.prid left join m4_cs cs on rq.rqidcs = cs.csid left join m0_status st1 on st1.kode = rq.rqstatus left join m0_status st2 on st2.kode = rq.rqstatussebelumnya left join m0_user u1 on u1.userid = rq.rqinputuser left join m0_user u2 on u2.userid = rq.rqmodifikasiuser left join m0_status_rq srq on rq.rqcustomint1 = srq.kode"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Rq", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "rq.rqid", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rqid"), 0), sptField,
                     FxDB(dr("rqcabang"), ""), sptField,
                     FxDB(dr("rqlokasi"), ""), sptField,
                     FxDB(dr("rqgudang"), ""), sptField,
                     FxDB(dr("rqasalbarang"), ""), sptField,
                     FxDB(dr("rqasalbarangkategori"), 0), sptField,
                     FxDB(dr("rqjenispembelian"), ""), sptField,
                     FxDB(dr("rqjenispembeliankategori"), 0), sptField,
                     FxDB(dr("rqcarabayar"), 0), sptField,
                     FxDB(dr("rqsumber"), ""), sptField,
                     FxDB(dr("rqautonogrup"), 0), sptField,
                     FxDB(dr("rqnogrup"), ""), sptField,
                     FxDB(dr("rqautonotransaksi"), 0), sptField,
                     FxDB(dr("rqnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rqtgl"), ""), formatTgl), sptField,
                     FxDB(dr("rqkodepa"), 0), sptField,
                     FxDB(dr("rqsupplier"), 0), sptField,
                     FxDB(dr("rqsupplierkontak"), ""), sptField,
                     FxDB(dr("rq1alamat1"), ""), sptField,
                     FxDB(dr("rq1alamat2"), ""), sptField,
                     FxDB(dr("rq1alamat3"), ""), sptField,
                     FxDB(dr("rq2alamat1"), ""), sptField,
                     FxDB(dr("rq2alamat2"), ""), sptField,
                     FxDB(dr("rq2alamat3"), ""), sptField,
                     FxDB(dr("rqbagianpembelian"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rqtgldipenuhi"), ""), formatTgl), sptField,
                     FxDB(dr("rqtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rqtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("rquraian"), ""), sptField,
                     FxDB(dr("rqcatatan"), ""), sptField,
                     FxDB(dr("rqnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rqtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("rqtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("rqmatauang"), ""), sptField,
                     FxDB(dr("rqkurs"), 0), sptField,
                     FxDB(dr("rqhargatermasukpajak"), 0), sptField,
                     FxDB(dr("rqtotal"), 0), sptField,
                     FxDB(dr("rqdiskonpersen"), ""), sptField,
                     FxDB(dr("rqdiskon"), 0), sptField,
                     FxDB(dr("rqtotalpajak1detail"), 0), sptField,
                     FxDB(dr("rqtotalpajak2detail"), 0), sptField,
                     FxDB(dr("rqbiayalainpersen"), ""), sptField,
                     FxDB(dr("rqbiayalain"), 0), sptField,
                     FxDB(dr("rqtotaltransaksi"), 0), sptField,
                     FxDB(dr("rqidpr"), 0), sptField,
                     FxDB(dr("rqidcs"), 0), sptField,
                     FxDB(dr("rqstatuspo"), 0), sptField,
                     FxDB(dr("rqstatusipc"), 0), sptField,
                     FxDB(dr("rqstatusgrn"), 0), sptField,
                     FxDB(dr("rqstatusri"), 0), sptField,
                     FxDB(dr("rqstatusdnr"), 0), sptField,
                     FxDB(dr("rqstatusprt"), 0), sptField,
                     FxDB(dr("rqstatusrealisasi"), 0), sptField,
                     FxDB(dr("rqstatus"), 0), sptField,
                     FxDB(dr("rqstatussebelumnya"), 0), sptField,
                     FxDB(dr("rqjmlrevisi"), 0), sptField,
                     FxDB(dr("rqcetakanke"), 0), sptField,
                     FxDB(dr("rqinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rqinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rqmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rqmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rqposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rqpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rqisclose"), 0), sptField,
                     FxDB(dr("rqcabangnama"), ""), sptField,
                     FxDB(dr("rqlokasinama"), ""), sptField,
                     FxDB(dr("rqgudangnama"), ""), sptField,
                     FxDB(dr("rqsupplierkode"), ""), sptField,
                     FxDB(dr("rqsuppliernama"), ""), sptField,
                     FxDB(dr("rqbagianpembeliankode"), ""), sptField,
                     FxDB(dr("rqbagianpembeliannama"), ""), sptField,
                     FxDB(dr("prnotransaksi"), ""), sptField,
                     FxDB(dr("csnotransaksi"), ""), sptField,
                     FxDB(dr("rqstatusnama"), ""), sptField,
                     FxDB(dr("rqstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("rqinputusernama"), ""), sptField,
                     FxDB(dr("rqmodifikasiusernama"), ""), sptField,
                     FxDB(dr("prtotaltransaksi"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prtglawal"), "1971-01-01 00:00:00"), formatTglWaktu) & " - " & AsFormatTanggal(FxDB(dr("prtglakhir"), "1971-01-01 00:00:00"), formatTglWaktu), sptField,
                     FxDB(dr("rqcustomint1"), 0), sptField,
                     FxDB(dr("rqcustomint1nama"), ""), sptField,
                     FxDB(dr("rqcustomint2"), 0), sptField,
                     FxDB(dr("rqvariasi"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rqid, rqcabang, rqlokasi, rqgudang, rqasalbarang, rqasalbarangkategori, rqjenispembelian, rqjenispembeliankategori, rqcarabayar, rqsumber, rqautonogrup, rqnogrup, rqautonotransaksi, rqnotransaksi, rqtgl, rqkodepa, rqsupplier, rqsupplierkontak, rq1alamat1, rq1alamat2, rq1alamat3, rq2alamat1, rq2alamat2, rq2alamat3, rqbagianpembelian, rqtgldipenuhi, rqtermin, rqtgljatuhtempo, rquraian, rqcatatan, rqnoref, rqtglnoref, rqtglpenutupan, rqmatauang, rqkurs, rqhargatermasukpajak, rqtotal, rqdiskonpersen, rqdiskon, rqtotalpajak1detail, rqtotalpajak2detail, rqbiayalainpersen, rqbiayalain, rqtotaltransaksi, rqidpr, rqidcs, rqstatuspo, rqstatusipc, rqstatusgrn, rqstatusri, rqstatusdnr, rqstatusprt, rqstatusrealisasi, rqstatus, rqstatussebelumnya, rqjmlrevisi, rqcetakanke, rqinputuser, rqinputtgl, rqmodifikasiuser, rqmodifikasitgl, rqposting, rqpostingtgl, rqisclose, rqcabangnama, rqlokasinama, rqgudangnama, rqsupplierkode, rqsuppliernama, rqbagianpembeliankode, rqbagianpembeliannama, prnotransaksi, csnotransaksi, rqstatusnama, rqstatussebelumnyanama, rqinputusernama, rqmodifikasiusernama, prtotaltransaksi, rqtgllelang, rqcustomint1, rqcustomint1nama, rqcustomint2, rqvariasi"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_RqTerkait(ByVal param As String) As String
        'M4_RqTerkait --------------------------------------------------------
        'rqid, rqnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "rqid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_rq_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rqid"), 0), sptField,
                     FxDB(dr("rqnotransaksi"), ""), sptField,
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
            result(2) = "Related RQ data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rqid, rqnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_Rq_Detail_VSearch(ByVal param As String) As String
        'M4_Rq_Detail_VSearch --------------------------------------------------------
        'idrqdetail, idrq, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, 
        'jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, 
        'gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idprdetail, idcsdetail, jmlpo, statuspo, jmlipc, statusipc, jmlgrn, 
        'statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, 
        'jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3, rqnotransaksi, rqtgldipenuhi, 
        'rquraian, rqcatatan, rqnoref, rqtglnoref, rqsupplierkontak, rq1alamat1, rq1alamat2, 
        'rq1alamat3, rq2alamat1, rq2alamat2, rq2alamat3, rqtermin, rqterminnama, rqterminharijatuhtempo, 
        'rqbagianpembelian, rqbagianpembeliankode, rqbagianpembeliannama, kodebarang, pajak1nama, pajak1nilai, pajak2nama, 
        'pajak2nilai, jmlsisapo, jmlsisarealisasi, prnotransaksi, bjmllapangan, bsatuanlapangan,
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

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m4_rq_detail_v")
        sql = "select `rqd`.`idrqdetail` AS `idrqdetail`,`rqd`.`idrq` AS `idrq`,`rqd`.`idbarang` AS `idbarang`,`rqd`.`namabarang` AS `namabarang`,`rqd`.`tipebarang` AS `tipebarang`,`rqd`.`jml` AS `jml`,`rqd`.`satuan` AS `satuan`,`rqd`.`nilaisatuan` AS `nilaisatuan`,`rqd`.`jmlbarang` AS `jmlbarang`,`rqd`.`satuanbarang` AS `satuanbarang`,`rqd`.`matauang` AS `matauang`,`rqd`.`kurs` AS `kurs`,`rqd`.`harga` AS `harga`,`rqd`.`diskon` AS `diskon`,`rqd`.`jmldiskon` AS `jmldiskon`,`rqd`.`pajak1` AS `pajak1`,`rqd`.`jmlpajak1` AS `jmlpajak1`,`rqd`.`pajak2` AS `pajak2`,`rqd`.`jmlpajak2` AS `jmlpajak2`,`rqd`.`cabang` AS `cabang`,`rqd`.`lokasi` AS `lokasi`,`rqd`.`gudang` AS `gudang`,`rqd`.`costcenter` AS `costcenter`,`rqd`.`divisi` AS `divisi`,`rqd`.`subdivisi` AS `subdivisi`,`rqd`.`proyek` AS `proyek`,`rqd`.`catatan` AS `catatan`,`rqd`.`urutan` AS `urutan`,`rqd`.`idprdetail` AS `idprdetail`,`rqd`.`idcsdetail` AS `idcsdetail`,`rqd`.`jmlpo` AS `jmlpo`,`rqd`.`statuspo` AS `statuspo`,`rqd`.`jmlipc` AS `jmlipc`,`rqd`.`statusipc` AS `statusipc`,`rqd`.`jmlgrn` AS `jmlgrn`,`rqd`.`statusgrn` AS `statusgrn`,`rqd`.`jmlri` AS `jmlri`,`rqd`.`statusri` AS `statusri`,`rqd`.`jmldnr` AS `jmldnr`,`rqd`.`statusdnr` AS `statusdnr`,`rqd`.`jmlprt` AS `jmlprt`,`rqd`.`statusprt` AS `statusprt`,`rqd`.`jmlrealisasi` AS `jmlrealisasi`,`rqd`.`statusrealisasi` AS `statusrealisasi`,`rqd`.`isclose` AS `isclose`,`rqd`.`customtext1` AS `customtext1`,`rqd`.`customtext2` AS `customtext2`,`rqd`.`customtext3` AS `customtext3`,`rqd`.`customdbl1` AS `customdbl1`,`rqd`.`customdbl2` AS `customdbl2`,`rqd`.`customdbl3` AS `customdbl3`,`rqd`.`customdate1` AS `customdate1`,`rqd`.`customdate2` AS `customdate2`,`rqd`.`customdate3` AS `customdate3`,`rq`.`rqnotransaksi` AS `rqnotransaksi`,`rq`.`rqtgldipenuhi` AS `rqtgldipenuhi`,`rq`.`rquraian` AS `rquraian`,`rq`.`rqcatatan` AS `rqcatatan`,`rq`.`rqnoref` AS `rqnoref`,`rq`.`rqtglnoref` AS `rqtglnoref`,`rq`.`rqsupplierkontak` AS `rqsupplierkontak`,`rq`.`rq1alamat1` AS `rq1alamat1`,`rq`.`rq1alamat2` AS `rq1alamat2`,`rq`.`rq1alamat3` AS `rq1alamat3`,`rq`.`rq2alamat1` AS `rq2alamat1`,`rq`.`rq2alamat2` AS `rq2alamat2`,`rq`.`rq2alamat3` AS `rq2alamat3`,`rq`.`rqtermin` AS `rqtermin`,`tr`.`trnama` AS `rqterminnama`,`tr`.`trharijatuhtempo` AS `rqterminharijatuhtempo`,`rq`.`rqbagianpembelian` AS `rqbagianpembelian`,`c1`.`kkode` AS `rqbagianpembeliankode`,`c1`.`knama` AS `rqbagianpembeliannama`,`i`.`bkode` AS `kodebarang`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,((`rqd`.`jmlbarang` - `rqd`.`jmlpo`) / `rqd`.`nilaisatuan`) AS `jmlsisapo`,((`rqd`.`jmlbarang` - `rqd`.`jmlrealisasi`) / `rqd`.`nilaisatuan`) AS `jmlsisarealisasi`,`pr`.`prnotransaksi` AS `prnotransaksi`, i.bjmllapangan, i.bsatuanlapangan, t1.takunbeli as pajak1akunbeli, t1c1.cnama as pajak1akunbelinama, t1.takunjual as pajak1akunjual, t1c2.cnama as pajak1akunjualnama, t2.takunbeli as pajak2akunbeli, t2c1.cnama as pajak2akunbelinama, t2.takunjual as pajak2akunjual, t2c2.cnama as pajak2akunjualnama, i.bkp from `m4_rq_detail` `rqd` left join `m4_rq` `rq` on `rqd`.`idrq` = `rq`.`rqid` left join `m1_terms` `tr` on `rq`.`rqtermin` = `tr`.`trkode` left join `m1_contact` `c1` on `rq`.`rqbagianpembelian` = `c1`.`kid` left join `m1_item` `i` on `rqd`.`idbarang` = `i`.`bid` left join `m1_tax` `t1` on `rqd`.`pajak1` = `t1`.`tkode` left join `m1_tax` `t2` on `rqd`.`pajak2` = `t2`.`tkode` left join `m4_pr_detail` `prd` on `rqd`.`idprdetail` = `prd`.`idprdetail` left join `m4_pr` `pr` on `prd`.`idpr` = `pr`.`prid` left join m1_coa t1c1 on t1.takunbeli = t1c1.cnomor left join m1_coa t1c2 on t1.takunjual = t1c2.cnomor left join m1_coa t2c1 on t2.takunbeli = t2c1.cnomor left join m1_coa t2c2 on t2.takunjual = t2c2.cnomor"


        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M5_Sq_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idrqdetail"), 0), sptField,
                     FxDB(dr("idrq"), 0), sptField,
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
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idprdetail"), 0), sptField,
                     FxDB(dr("idcsdetail"), 0), sptField,
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
                     FxDB(dr("rqnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rqtgldipenuhi"), ""), formatTgl), sptField,
                     FxDB(dr("rquraian"), ""), sptField,
                     FxDB(dr("rqcatatan"), ""), sptField,
                     FxDB(dr("rqnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rqtglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("rqsupplierkontak"), ""), sptField,
                     FxDB(dr("rq1alamat1"), ""), sptField,
                     FxDB(dr("rq1alamat2"), ""), sptField,
                     FxDB(dr("rq1alamat3"), ""), sptField,
                     FxDB(dr("rq2alamat1"), ""), sptField,
                     FxDB(dr("rq2alamat2"), ""), sptField,
                     FxDB(dr("rq2alamat3"), ""), sptField,
                     FxDB(dr("rqtermin"), ""), sptField,
                     FxDB(dr("rqterminnama"), ""), sptField,
                     FxDB(dr("rqterminharijatuhtempo"), 0), sptField,
                     FxDB(dr("rqbagianpembelian"), 0), sptField,
                     FxDB(dr("rqbagianpembeliankode"), ""), sptField,
                     FxDB(dr("rqbagianpembeliannama"), ""), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("jmlsisapo"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
                     FxDB(dr("prnotransaksi"), ""), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptField,
                     FxDB(dr("pajak1akunbeli"), ""), sptField,
                     FxDB(dr("pajak1akunbelinama"), ""), sptField,
                     FxDB(dr("pajak1akunjual"), ""), sptField,
                     FxDB(dr("pajak1akunjualnama"), ""), sptField,
                     FxDB(dr("pajak2akunbeli"), ""), sptField,
                     FxDB(dr("pajak2akunbelinama"), ""), sptField,
                     FxDB(dr("pajak2akunjual"), ""), sptField,
                     FxDB(dr("pajak2akunjualnama"), ""), sptField,
                     FxDB(dr("bkp"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idrqdetail, idrq, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, jmlpo, statuspo, jmlipc, statusipc, jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, rqnotransaksi, rqtgldipenuhi, rquraian, rqcatatan, rqnoref, rqtglnoref, rqsupplierkontak, rq1alamat1, rq1alamat2, rq1alamat3, rq2alamat1, rq2alamat2, rq2alamat3, rqtermin, rqterminnama, rqterminharijatuhtempo, rqbagianpembelian, rqbagianpembeliankode, rqbagianpembeliannama, kodebarang, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisapo, jmlsisarealisasi, prnotransaksi, bjmllapangan, bsatuanlapangan, pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, pajak2akunjualnama, bkp"))

        Return wsResult
    End Function

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstanding As String) As String
        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, kodebarang As String = "", tipebarang As String = "", namabarang As String = ""
        Dim filterLookup As String = "", urutan As String = ""

        'VALIDASI OUTSTANDING ---------------------------------------
        If Len(ftExistOutstanding) > 0 Then 'ftExistOutstanding = rowExists, idprdetail, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstanding)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "idprdetail=" & dtval.Rows(0)("idprdetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in PR" : GoTo selesai
            End If
        End If
        'END OF VALIDASI OUTSTANDING --------------------------------
selesai:
        Return errmessage
    End Function

    <WebMethod()>
    Public Function M4_RqSimpanOld(ByVal param As String) As String
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
        'rqid(0) As Integer, rqcabang(1) As String, rqlokasi(2) As String, rqgudang(3) As String, rqasalbarang(4) As String, 
        'rqasalbarangkategori(5) As Integer, rqjenispembelian(6) As String, rqjenispembeliankategori(7) As Integer, rqcarabayar(8) As Integer, rqsumber(9) As String, 
        'rqautonogrup(10) As Integer, rqnogrup(11) As String, rqautonotransaksi(12) As Integer, rqnotransaksi(13) As String, rqtgl(14) As Date, 
        'rqkodepa(15) As Integer, rqsupplier(16) As Integer, rqsupplierkontak(17) As String, rq1alamat1(18) As String, rq1alamat2(19) As String, 
        'rq1alamat3(20) As String, rq2alamat1(21) As String, rq2alamat2(22) As String, rq2alamat3(23) As String, rqbagianpembelian(24) As Integer, 
        'rqtgldipenuhi(25) As Date, rqtermin(26) As String, rqtgljatuhtempo(27) As Date, rquraian(28) As String, rqcatatan(29) As String, 
        'rqnoref(30) As String, rqtglnoref(31) As Date, rqtglpenutupan(32) As Date, rqmatauang(33) As String, rqkurs(34) As Double, 
        'rqhargatermasukpajak(35) As Integer, rqtotal(36) As Double, rqdiskonpersen(37) As String, rqdiskon(38) As Double, rqtotalpajak1detail(39) As Double, 
        'rqtotalpajak2detail(40) As Double, rqbiayalainpersen(41) As String, rqbiayalain(42) As Double, rqtotaltransaksi(43) As Double, rqidpr(44) As Integer, 
        'rqidcs(45) As Integer, rqstatuspo(46) As Integer, rqstatusipc(47) As Integer, rqstatusgrn(48) As Integer, rqstatusri(49) As Integer, 
        'rqstatusdnr(50) As Integer, rqstatusprt(51) As Integer, rqstatus(52) As Integer, rqstatussebelumnya(53) As Integer, rqjmlrevisi(54) As Integer, 
        'rqcetakanke(55) As Integer, rqinputuser(56) As Integer, rqinputtgl(57) As DateTime, rqmodifikasiuser(58) As Integer, rqmodifikasitgl(59) As DateTime, 
        'rqisclose(60) As Integer, rqcustomtext1(61) As String, rqcustomtext2(62) As String, rqcustomtext3(63) As String, rqcustomtext4(64) As String, 
        'rqcustomtext5(65) As String, rqcustomint1(66) As Integer, rqcustomint2(67) As Integer, rqcustomint3(68) As Integer, rqcustomdbl1(69) As Double, 
        'rqcustomdbl2(70) As Double, rqcustomdbl3(71) As Double, rqcustomdate1(72) As Date, rqcustomdate2(73) As Date, rqcustomdate3(74) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'rqid, rqcabang, rqlokasi, rqgudang, rqasalbarang, rqasalbarangkategori, rqjenispembelian, 
        'rqjenispembeliankategori, rqcarabayar, rqsumber, rqautonogrup, rqnogrup, rqautonotransaksi, rqnotransaksi, 
        'rqtgl, rqkodepa, rqsupplier, rqsupplierkontak, rq1alamat1, rq1alamat2, rq1alamat3, 
        'rq2alamat1, rq2alamat2, rq2alamat3, rqbagianpembelian, rqtgldipenuhi, rqtermin, rqtgljatuhtempo, 
        'rquraian, rqcatatan, rqnoref, rqtglnoref, rqtglpenutupan, rqmatauang, rqkurs, 
        'rqhargatermasukpajak, rqtotal, rqdiskonpersen, rqdiskon, rqtotalpajak1detail, rqtotalpajak2detail, rqbiayalainpersen, 
        'rqbiayalain, rqtotaltransaksi, rqidpr, rqidcs, rqstatuspo, rqstatusipc, rqstatusgrn, 
        'rqstatusri, rqstatusdnr, rqstatusprt, rqstatus, rqstatussebelumnya, rqjmlrevisi, rqcetakanke, 
        'rqinputuser, rqinputtgl, rqmodifikasiuser, rqmodifikasitgl, rqisclose, rqcustomtext1, rqcustomtext2, 
        'rqcustomtext3, rqcustomtext4, rqcustomtext5, rqcustomint1, rqcustomint2, rqcustomint3, rqcustomdbl1, 
        'rqcustomdbl2, rqcustomdbl3, rqcustomdate1, rqcustomdate2, rqcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 75) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'rqid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "rqid required numeric." : GoTo selesai
        End If
        'rqasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "rqasalbarangkategori required numeric." : GoTo selesai
        End If
        'rqjenispembeliankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "rqjenispembeliankategori required numeric." : GoTo selesai
        End If
        'rqcarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "rqcarabayar required numeric." : GoTo selesai
        End If
        'rqautonogrup(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "rqautonogrup required numeric." : GoTo selesai
        End If
        'rqautonotransaksi(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "rqautonotransaksi required numeric." : GoTo selesai
        End If
        'rqtgl(14) As Date
        If (IsDate(dataUtama(14)) = False) Then
            result(2) = "rqtgl required date." : GoTo selesai
        End If
        'rqkodepa(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "rqkodepa required numeric." : GoTo selesai
        End If
        'rqsupplier(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "rqsupplier required numeric." : GoTo selesai
        End If
        If (dataUtama(16) < 1) Then
            result(2) = "rqsupplier can't be empty." : GoTo selesai
        End If
        'rqbagianpembelian(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "rqbagianpembelian required numeric." : GoTo selesai
        End If
        'rqtgldipenuhi(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "rqtgldipenuhi required date." : GoTo selesai
        End If
        'rqtgljatuhtempo(27) As Date
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "rqtgljatuhtempo required date." : GoTo selesai
        End If
        'rqtglnoref(31) As Date
        If (IsDate(dataUtama(31)) = False) Then
            result(2) = "rqtglnoref required date." : GoTo selesai
        End If
        'rqtglpenutupan(32) As Date
        If (IsDate(dataUtama(32)) = False) Then
            result(2) = "rqtglpenutupan required date." : GoTo selesai
        End If
        'rqkurs(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "rqkurs required numeric." : GoTo selesai
        End If
        'rqhargatermasukpajak(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "rqhargatermasukpajak required numeric." : GoTo selesai
        End If
        'rqtotal(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "rqtotal required numeric." : GoTo selesai
        End If
        'rqdiskon(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "rqdiskon required numeric." : GoTo selesai
        End If
        'rqtotalpajak1detail(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "rqtotalpajak1detail required numeric." : GoTo selesai
        End If
        'rqtotalpajak2detail(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "rqtotalpajak2detail required numeric." : GoTo selesai
        End If
        'rqbiayalain(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "rqbiayalain required numeric." : GoTo selesai
        End If
        'rqtotaltransaksi(43) As Double
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "rqtotaltransaksi required numeric." : GoTo selesai
        End If
        'rqidpr(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "rqidpr required numeric." : GoTo selesai
        End If
        'rqidcs(45) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "rqidcs required numeric." : GoTo selesai
        End If
        'rqstatuspo(46) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "rqstatuspo required numeric." : GoTo selesai
        End If
        'rqstatusipc(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "rqstatusipc required numeric." : GoTo selesai
        End If
        'rqstatusgrn(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "rqstatusgrn required numeric." : GoTo selesai
        End If
        'rqstatusri(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "rqstatusri required numeric." : GoTo selesai
        End If
        'rqstatusdnr(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "rqstatusdnr required numeric." : GoTo selesai
        End If
        'rqstatusprt(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "rqstatusprt required numeric." : GoTo selesai
        End If
        'rqstatus(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "rqstatus required numeric." : GoTo selesai
        End If
        'rqstatussebelumnya(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "rqstatussebelumnya required numeric." : GoTo selesai
        End If
        'rqjmlrevisi(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "rqjmlrevisi required numeric." : GoTo selesai
        End If
        'rqcetakanke(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "rqcetakanke required numeric." : GoTo selesai
        End If
        'rqinputuser(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "rqinputuser required numeric." : GoTo selesai
        End If
        'rqinputtgl(57) As DateTime
        If (IsDate(dataUtama(57)) = False) Then
            result(2) = "rqinputtgl required date." : GoTo selesai
        End If
        'rqmodifikasiuser(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "rqmodifikasiuser required numeric." : GoTo selesai
        End If
        'rqmodifikasitgl(59) As DateTime
        If (IsDate(dataUtama(59)) = False) Then
            result(2) = "rqmodifikasitgl required date." : GoTo selesai
        End If
        'rqisclose(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "rqisclose required numeric." : GoTo selesai
        End If
        'rqcustomint1(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "rqcustomint1 required numeric." : GoTo selesai
        End If
        'rqcustomint2(67) As Integer
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "rqcustomint2 required numeric." : GoTo selesai
        End If
        'rqcustomint3(68) As Integer
        If (IsNumeric(dataUtama(68)) = False) Then
            result(2) = "rqcustomint3 required numeric." : GoTo selesai
        End If
        'rqcustomdbl1(69) As Double
        If (IsNumeric(dataUtama(69)) = False) Then
            result(2) = "rqcustomdbl1 required numeric." : GoTo selesai
        End If
        'rqcustomdbl2(70) As Double
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "rqcustomdbl2 required numeric." : GoTo selesai
        End If
        'rqcustomdbl3(71) As Double
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "rqcustomdbl3 required numeric." : GoTo selesai
        End If
        'rqcustomdate1(72) As Date
        If (IsDate(dataUtama(72)) = False) Then
            result(2) = "rqcustomdate1 required date." : GoTo selesai
        End If
        'rqcustomdate2(73) As Date
        If (IsDate(dataUtama(73)) = False) Then
            result(2) = "rqcustomdate2 required date." : GoTo selesai
        End If
        'rqcustomdate3(74) As Date
        If (IsDate(dataUtama(74)) = False) Then
            result(2) = "rqcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'rqcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "rqcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "rqcabang should not be more than 25 character." : GoTo selesai
        End If

        'rqlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "rqlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "rqlokasi should not be more than 25 character." : GoTo selesai
        End If

        'rqgudang(3) As String
        'If Len(dataUtama(3)) = 0 Then
        '    result(2) = "rqgudang can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "rqgudang should not be more than 25 character." : GoTo selesai
        End If

        'rqsumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "rqsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "rqsumber should not be more than 10 character." : GoTo selesai
        End If

        'rqnotransaksi(13) As String
        If Len(dataUtama(13)) = 0 Then
            result(2) = "rqnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(13)) > 50 Then
            result(2) = "rqnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'rqtgl(14) As Date
        If Len(dataUtama(14)) = 0 Then
            result(2) = "rqtgl can't be empty" : GoTo selesai
        End If

        'rqtgldipenuhi(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "rqtgldipenuhi can't be empty" : GoTo selesai
        End If

        'rqtgljatuhtempo(27) As Date
        If Len(dataUtama(27)) = 0 Then
            result(2) = "rqtgljatuhtempo can't be empty" : GoTo selesai
        End If

        'rqtglnoref(31) As Date
        If Len(dataUtama(31)) = 0 Then
            result(2) = "rqtglnoref can't be empty" : GoTo selesai
        End If

        'rqtglpenutupan(32) As Date
        If Len(dataUtama(32)) = 0 Then
            result(2) = "rqtglpenutupan can't be empty" : GoTo selesai
        End If

        'rqmatauang(33) As String
        If Len(dataUtama(33)) = 0 Then
            result(2) = "rqmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(33)) > 25 Then
            result(2) = "rqmatauang should not be more than 25 character." : GoTo selesai
        End If

        'rqkurs(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "rqkurs can't be empty" : GoTo selesai
        End If

        'rqtotal(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "rqtotal can't be empty" : GoTo selesai
        End If

        'rqdiskonpersen(37) As String
        If Len(dataUtama(37)) = 0 Then
            result(2) = "rqdiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(37)) > 25 Then
            result(2) = "rqdiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'rqdiskon(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "rqdiskon can't be empty" : GoTo selesai
        End If

        'rqtotalpajak1detail(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "rqtotalpajak1detail can't be empty" : GoTo selesai
        End If

        'rqtotalpajak2detail(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "rqtotalpajak2detail can't be empty" : GoTo selesai
        End If

        'rqbiayalainpersen(41) As String
        If Len(dataUtama(41)) = 0 Then
            result(2) = "rqbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(41)) > 25 Then
            result(2) = "rqbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'rqbiayalain(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "rqbiayalain can't be empty" : GoTo selesai
        End If

        'rqtotaltransaksi(43) As Double
        If Len(dataUtama(43)) = 0 Then
            result(2) = "rqtotaltransaksi can't be empty" : GoTo selesai
        End If

        'rqinputtgl(57) As DateTime
        If Len(dataUtama(57)) = 0 Then
            result(2) = "rqinputtgl can't be empty" : GoTo selesai
        End If

        'rqmodifikasitgl(59) As DateTime
        If Len(dataUtama(59)) = 0 Then
            result(2) = "rqmodifikasitgl can't be empty" : GoTo selesai
        End If

        'rqcustomdbl1(69) As Double
        If Len(dataUtama(69)) = 0 Then
            result(2) = "rqcustomdbl1 can't be empty" : GoTo selesai
        End If

        'rqcustomdbl2(70) As Double
        If Len(dataUtama(70)) = 0 Then
            result(2) = "rqcustomdbl2 can't be empty" : GoTo selesai
        End If

        'rqcustomdbl3(71) As Double
        If Len(dataUtama(71)) = 0 Then
            result(2) = "rqcustomdbl3 can't be empty" : GoTo selesai
        End If

        'rqcustomdate1(72) As Date
        If Len(dataUtama(72)) = 0 Then
            result(2) = "rqcustomdate1 can't be empty" : GoTo selesai
        End If

        'rqcustomdate2(73) As Date
        If Len(dataUtama(73)) = 0 Then
            result(2) = "rqcustomdate2 can't be empty" : GoTo selesai
        End If

        'rqcustomdate3(74) As Date
        If Len(dataUtama(74)) = 0 Then
            result(2) = "rqcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "rqid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqjenispembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqjenispembeliankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqautonogrup", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqnogrup", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqsupplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqsupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rq1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rq1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rq1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rq2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rq2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rq2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqbagianpembelian", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqtgldipenuhi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqtermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqtgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rquraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqtglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqhargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqtotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqdiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqtotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqtotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqtotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqidpr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqidcs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqstatuspo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqstatusipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqstatusgrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqstatusri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqstatusdnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqstatusprt", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rqcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rqcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "rqid~rqcabang~rqlokasi~rqgudang~rqasalbarang~rqasalbarangkategori~rqjenispembelian~rqjenispembeliankategori~rqcarabayar~rqsumber~rqautonogrup~rqnogrup~rqautonotransaksi~rqnotransaksi~rqtgl~rqkodepa~rqsupplier~rqsupplierkontak~rq1alamat1~rq1alamat2~rq1alamat3~rq2alamat1~rq2alamat2~rq2alamat3~rqbagianpembelian~rqtgldipenuhi~rqtermin~rqtgljatuhtempo~rquraian~rqcatatan~rqnoref~rqtglnoref~rqtglpenutupan~rqmatauang~rqkurs~rqhargatermasukpajak~rqtotal~rqdiskonpersen~rqdiskon~rqtotalpajak1detail~rqtotalpajak2detail~rqbiayalainpersen~rqbiayalain~rqtotaltransaksi~rqidpr~rqidcs~rqstatuspo~rqstatusipc~rqstatusgrn~rqstatusri~rqstatusdnr~rqstatusprt~rqstatus~rqstatussebelumnya~rqjmlrevisi~rqcetakanke~rqinputuser~rqinputtgl~rqmodifikasiuser~rqmodifikasitgl~rqisclose~rqcustomtext1~rqcustomtext2~rqcustomtext3~rqcustomtext4~rqcustomtext5~rqcustomint1~rqcustomint2~rqcustomint3~rqcustomdbl1~rqcustomdbl2~rqcustomdbl3~rqcustomdate1~rqcustomdate2~rqcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idrqdetail(0) As Integer, idrq(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, diskon(13) As String, jmldiskon(14) As Double, 
        'pajak1(15) As String, jmlpajak1(16) As Double, pajak2(17) As String, jmlpajak2(18) As Double, cabang(19) As String, 
        'lokasi(20) As String, gudang(21) As String, costcenter(22) As String, divisi(23) As String, subdivisi(24) As String, 
        'proyek(25) As String, catatan(26) As String, urutan(27) As Integer, idprdetail(28) As Integer, idcsdetail(29) As Integer, 
        'jmlpo(30) As Double, statuspo(31) As Integer, jmlipc(32) As Double, statusipc(33) As Integer, jmlgrn(34) As Double, 
        'statusgrn(35) As Integer, jmlri(36) As Double, statusri(37) As Integer, jmldnr(38) As Double, statusdnr(39) As Integer, 
        'jmlprt(40) As Double, statusprt(41) As Integer, isclose(42) As Integer, customtext1(43) As String, customtext2(44) As String, 
        'customtext3(45) As String, customdbl1(46) As Double, customdbl2(47) As Double, customdbl3(48) As Double, customdate1(49) As Date, 
        'customdate2(50) As Date, customdate3(51) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idrqdetail, idrq, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, 
        'jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, 
        'gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idprdetail, idcsdetail, jmlpo, statuspo, jmlipc, statusipc, jmlgrn, 
        'statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idrqdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idrq", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idprdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idcsdetail", AsEnumTypeData.AsInt64)
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
        Dim idbarang As Integer = 0, idprdetail As Integer = 0, jmlbarang As Double = 0

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 52) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idrqdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idrqdetail required numeric." : GoTo selesai
            End If
            'idrq(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idrq required numeric." : GoTo selesai
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
            'urutan(27) As Integer
            If (IsNumeric(dataRowDetail(27)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idprdetail(28) As Integer
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - idprdetail required numeric." : GoTo selesai
            End If
            'idcsdetail(29) As Integer
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - idcsdetail required numeric." : GoTo selesai
            End If
            'jmlpo(30) As Double
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - jmlpo required numeric." : GoTo selesai
            End If
            'statuspo(31) As Integer
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - statuspo required numeric." : GoTo selesai
            End If
            'jmlipc(32) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - jmlipc required numeric." : GoTo selesai
            End If
            'statusipc(33) As Integer
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - statusipc required numeric." : GoTo selesai
            End If
            'jmlgrn(34) As Double
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - jmlgrn required numeric." : GoTo selesai
            End If
            'statusgrn(35) As Integer
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - statusgrn required numeric." : GoTo selesai
            End If
            'jmlri(36) As Double
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - jmlri required numeric." : GoTo selesai
            End If
            'statusri(37) As Integer
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - statusri required numeric." : GoTo selesai
            End If
            'jmldnr(38) As Double
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - jmldnr required numeric." : GoTo selesai
            End If
            'statusdnr(39) As Integer
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - statusdnr required numeric." : GoTo selesai
            End If
            'jmlprt(40) As Double
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - jmlprt required numeric." : GoTo selesai
            End If
            'statusprt(41) As Integer
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - statusprt required numeric." : GoTo selesai
            End If
            'isclose(42) As Integer
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(46) As Double
            If (IsNumeric(dataRowDetail(46)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(47) As Double
            If (IsNumeric(dataRowDetail(47)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(48) As Double
            If (IsNumeric(dataRowDetail(48)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(49) As Date
            If (IsDate(dataRowDetail(49)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(50) As Date
            If (IsDate(dataRowDetail(50)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(51) As Date
            If (IsDate(dataRowDetail(51)) = False) Then
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
            'If dataRowDetail(12) <= 0 Then
            '    result(2) = "Row : " & i & " - harga can't be less than or equal to zero" : GoTo selesai
            'End If

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

            'jmlpo(30) As Double
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Row : " & i & " - jmlpo can't be empty" : GoTo selesai
            End If

            'jmlipc(32) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - jmlipc can't be empty" : GoTo selesai
            End If

            'jmlgrn(34) As Double
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Row : " & i & " - jmlgrn can't be empty" : GoTo selesai
            End If

            'jmlri(36) As Double
            If Len(dataRowDetail(36)) = 0 Then
                result(2) = "Row : " & i & " - jmlri can't be empty" : GoTo selesai
            End If

            'jmldnr(38) As Double
            If Len(dataRowDetail(38)) = 0 Then
                result(2) = "Row : " & i & " - jmldnr can't be empty" : GoTo selesai
            End If

            'jmlprt(40) As Double
            If Len(dataRowDetail(40)) = 0 Then
                result(2) = "Row : " & i & " - jmlprt can't be empty" : GoTo selesai
            End If

            'customdbl1(46) As Double
            If Len(dataRowDetail(46)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(47) As Double
            If Len(dataRowDetail(47)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(48) As Double
            If Len(dataRowDetail(48)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(49) As Date
            If Len(dataRowDetail(49)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(50) As Date
            If Len(dataRowDetail(50)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(51) As Date
            If Len(dataRowDetail(51)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idrqdetail~idrq~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~idprdetail~idcsdetail~jmlpo~statuspo~jmlipc~statusipc~jmlgrn~statusgrn~jmlri~statusri~jmldnr~statusdnr~jmlprt~statusprt~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , idprdetail(28) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : idprdetail = dataRowDetail(28)

            'VALIDASI OUTSTANDING -------------------------
            If idprdetail <> 0 Then
                '1. CEK DATA EXIST ------------------------
                ftExistOutstanding = IIf(Len(ftExistOutstanding.ToString) = 0, "", ftExistOutstanding & " UNION ")
                ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM M4_pr_detail JOIN M4_pr ON idpr = prid WHERE idprdetail = '" & idprdetail & "' AND (prstatus = 2 OR prstatus = 3 OR prstatus = 4 OR prstatus = 7) LIMIT 1) as rowExists, '" & idprdetail & "' as idprdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. SET NILAI UPDATE OUTSTANDING ----------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idprdetail=" & idprdetail)
                updNilai = String.Concat("WHEN '" & idprdetail & "' THEN ROUND(jmlrq + '" & Outstanding & "', 5) ", updNilai)

                '3. SET FILTER UPDATE OUTSTANDING ---------
                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                updFilter = String.Concat(updFilter, "(idprdetail = '" & idprdetail & "')")
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

                ''CEK PERIODE AKUNTANSI ==================================
                'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("rqtgl")), AsFormatTanggal(drutama("rqtgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("rqstatus") = 2 Then
                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstanding)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("rqtermin").ToString, AsFormatTanggal(drutama("rqtgl")), "rqtgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("rqtgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TOTAL = subtotal
                drutama("rqtotal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("rqtotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("rqtotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                If Integer.Parse(drutama("rqhargatermasukpajak")) = 0 Then
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                    drutama("rqtotaltransaksi") = Double.Parse(drutama("rqtotal")) - Double.Parse(drutama("rqdiskon")) + Double.Parse(drutama("rqtotalpajak1detail")) + Double.Parse(drutama("rqtotalpajak2detail")) + Double.Parse(drutama("rqbiayalain"))

                Else
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                    drutama("rqtotaltransaksi") = Double.Parse(drutama("rqtotal")) - Double.Parse(drutama("rqdiskon")) + Double.Parse(drutama("rqbiayalain"))

                End If
                'END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("rqid")
                    notransaksi = drutama("rqnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(rqid), rqnotransaksi, rqautonogrup FROM M4_rq WHERE rqid='" & result(4) & "' AND rqstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)
                    autoNogrupOld = dtupdate.Rows(0)(2)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(rqid) FROM m4_rq WHERE rqnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_rq_history
                        Dim rsSimpanHistory As String = SimpanHistory.M4_Rq_HistorySimpan("" & paramSplit(0) & "★M4_Rq_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("rqsumber")) & "▼" & FixQuotes(drutama("rqid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        'GENERATE NOGRUP BARU JIKA AUTONOGRUP LAMA = 0 DAN AUTONOGRUP BARU = 1
                        If drutama("rqautonogrup") = "1" And autoNogrupOld = "0" Then
                            'GENERATE NOGRUP =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNogrup As String = wsM0_Nomor.M0_NogrupRQ(drutama("rqcabang"), drutama("rqlokasi"), drutama("rqtgl"))
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
                            nogrup = drutama("rqnogrup")
                        End If

                        sql = "Update M4_Rq set rqcabang  = '" & FixQuotes(drutama("rqcabang")) & "', rqlokasi  = '" & FixQuotes(drutama("rqlokasi")) & "', rqgudang  = '" & FixQuotes(drutama("rqgudang")) & "', rqasalbarang  = '" & FixQuotes(drutama("rqasalbarang")) & "', rqasalbarangkategori  = " & drutama("rqasalbarangkategori") & ", rqjenispembelian  = '" & FixQuotes(drutama("rqjenispembelian")) & "', rqjenispembeliankategori  = " & drutama("rqjenispembeliankategori") & ", rqcarabayar  = " & drutama("rqcarabayar") & ", rqsumber  = '" & FixQuotes(drutama("rqsumber")) & "', rqautonogrup  = " & drutama("rqautonogrup") & ", rqnogrup  = '" & FixQuotes(nogrup) & "', rqautonotransaksi  = " & drutama("rqautonotransaksi") & ", rqnotransaksi  = '" & notransaksi & "', rqtgl  = '" & FixQuotes(AsFormatTanggal(drutama("rqtgl"))) & "', rqkodepa  = " & drutama("rqkodepa") & ", rqsupplier  = " & drutama("rqsupplier") & ", rqsupplierkontak  = '" & FixQuotes(drutama("rqsupplierkontak")) & "', rq1alamat1  = '" & FixQuotes(drutama("rq1alamat1")) & "', rq1alamat2  = '" & FixQuotes(drutama("rq1alamat2")) & "', rq1alamat3  = '" & FixQuotes(drutama("rq1alamat3")) & "', rq2alamat1  = '" & FixQuotes(drutama("rq2alamat1")) & "', rq2alamat2  = '" & FixQuotes(drutama("rq2alamat2")) & "', rq2alamat3  = '" & FixQuotes(drutama("rq2alamat3")) & "', rqbagianpembelian  = " & drutama("rqbagianpembelian") & ", rqtgldipenuhi  = '" & FixQuotes(AsFormatTanggal(drutama("rqtgldipenuhi"))) & "', rqtermin  = '" & FixQuotes(drutama("rqtermin")) & "', rqtgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("rqtgljatuhtempo"))) & "', rquraian  = '" & FixQuotes(drutama("rquraian")) & "', rqcatatan  = '" & FixQuotes(drutama("rqcatatan")) & "', rqnoref  = '" & FixQuotes(drutama("rqnoref")) & "', rqtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("rqtglnoref"))) & "', rqtglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("rqtglpenutupan"))) & "', rqmatauang  = '" & FixQuotes(drutama("rqmatauang")) & "', rqkurs  = '" & FixDouble(drutama("rqkurs")) & "', rqhargatermasukpajak  = " & drutama("rqhargatermasukpajak") & ", rqtotal  = '" & FixDouble(drutama("rqtotal")) & "', rqdiskonpersen  = '" & FixQuotes(drutama("rqdiskonpersen")) & "', rqdiskon  = '" & FixDouble(drutama("rqdiskon")) & "', rqtotalpajak1detail  = '" & FixDouble(drutama("rqtotalpajak1detail")) & "', rqtotalpajak2detail  = '" & FixDouble(drutama("rqtotalpajak2detail")) & "', rqbiayalainpersen  = '" & FixQuotes(drutama("rqbiayalainpersen")) & "', rqbiayalain  = '" & FixDouble(drutama("rqbiayalain")) & "', rqtotaltransaksi  = '" & FixDouble(drutama("rqtotaltransaksi")) & "', rqidpr  = " & drutama("rqidpr") & ", rqidcs  = " & drutama("rqidcs") & ", rqstatuspo  = " & drutama("rqstatuspo") & ", rqstatusipc  = " & drutama("rqstatusipc") & ", rqstatusgrn  = " & drutama("rqstatusgrn") & ", rqstatusri  = " & drutama("rqstatusri") & ", rqstatusdnr  = " & drutama("rqstatusdnr") & ", rqstatusprt  = " & drutama("rqstatusprt") & ", rqstatus  = " & drutama("rqstatus") & ", rqstatussebelumnya  = " & drutama("rqstatussebelumnya") & ", rqjmlrevisi  = rqjmlrevisi+1, rqcetakanke  = " & drutama("rqcetakanke") & ", rqmodifikasiuser  = " & drutama("rqmodifikasiuser") & ", rqmodifikasitgl  = NOW(), rqcustomtext1  = '" & FixQuotes(drutama("rqcustomtext1")) & "', rqcustomtext2  = '" & FixQuotes(drutama("rqcustomtext2")) & "', rqcustomtext3  = '" & FixQuotes(drutama("rqcustomtext3")) & "', rqcustomtext4  = '" & FixQuotes(drutama("rqcustomtext4")) & "', rqcustomtext5  = '" & FixQuotes(drutama("rqcustomtext5")) & "', rqcustomint1  = " & drutama("rqcustomint1") & ", rqcustomint2  = " & drutama("rqcustomint2") & ", rqcustomint3  = " & drutama("rqcustomint3") & ", rqcustomdbl1  = '" & FixDouble(drutama("rqcustomdbl1")) & "', rqcustomdbl2  = '" & FixDouble(drutama("rqcustomdbl2")) & "', rqcustomdbl3  = '" & FixDouble(drutama("rqcustomdbl3")) & "', rqcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("rqcustomdate1"))) & "', rqcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("rqcustomdate2"))) & "', rqcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("rqcustomdate3"))) & "' where rqid = '" & drutama("rqid") & "'"
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

                    If drutama("rqautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rqcabang"), drutama("rqlokasi"), drutama("rqsumber"), drutama("rqtgl"))
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
                        notransaksi = drutama("rqnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(rqid) FROM m4_rq WHERE rqnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============



                    If drutama("rqautonogrup") = 1 Then
                        'GENERATE NOGRUP =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNogrup As String = wsM0_Nomor.M0_NogrupRQ(drutama("rqcabang"), drutama("rqlokasi"), drutama("rqtgl"))
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
                        nogrup = drutama("rqnogrup")
                    End If

                    sql = "Insert into M4_Rq (rqcabang, rqlokasi, rqgudang, rqasalbarang, rqasalbarangkategori, rqjenispembelian, rqjenispembeliankategori, rqcarabayar, rqsumber, rqautonogrup, rqnogrup, rqautonotransaksi, rqnotransaksi, rqtgl, rqkodepa, rqsupplier, rqsupplierkontak, rq1alamat1, rq1alamat2, rq1alamat3, rq2alamat1, rq2alamat2, rq2alamat3, rqbagianpembelian, rqtgldipenuhi, rqtermin, rqtgljatuhtempo, rquraian, rqcatatan, rqnoref, rqtglnoref, rqtglpenutupan, rqmatauang, rqkurs, rqhargatermasukpajak, rqtotal, rqdiskonpersen, rqdiskon, rqtotalpajak1detail, rqtotalpajak2detail, rqbiayalainpersen, rqbiayalain, rqtotaltransaksi, rqidpr, rqidcs, rqstatuspo, rqstatusipc, rqstatusgrn, rqstatusri, rqstatusdnr, rqstatusprt, rqstatus, rqstatussebelumnya, rqjmlrevisi, rqcetakanke, rqinputuser, rqinputtgl, rqmodifikasiuser, rqmodifikasitgl, rqisclose, rqcustomtext1, rqcustomtext2, rqcustomtext3, rqcustomtext4, rqcustomtext5, rqcustomint1, rqcustomint2, rqcustomint3, rqcustomdbl1, rqcustomdbl2, rqcustomdbl3, rqcustomdate1, rqcustomdate2, rqcustomdate3) values('" & FixQuotes(drutama("rqcabang")) & "', '" & FixQuotes(drutama("rqlokasi")) & "', '" & FixQuotes(drutama("rqgudang")) & "', '" & FixQuotes(drutama("rqasalbarang")) & "', " & drutama("rqasalbarangkategori") & ", '" & FixQuotes(drutama("rqjenispembelian")) & "', " & drutama("rqjenispembeliankategori") & ", " & drutama("rqcarabayar") & ", '" & FixQuotes(drutama("rqsumber")) & "', " & drutama("rqautonogrup") & ", '" & FixQuotes(nogrup) & "', " & drutama("rqautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("rqtgl"))) & "', " & drutama("rqkodepa") & ", " & drutama("rqsupplier") & ", '" & FixQuotes(drutama("rqsupplierkontak")) & "', '" & FixQuotes(drutama("rq1alamat1")) & "', '" & FixQuotes(drutama("rq1alamat2")) & "', '" & FixQuotes(drutama("rq1alamat3")) & "', '" & FixQuotes(drutama("rq2alamat1")) & "', '" & FixQuotes(drutama("rq2alamat2")) & "', '" & FixQuotes(drutama("rq2alamat3")) & "', " & drutama("rqbagianpembelian") & ", '" & FixQuotes(AsFormatTanggal(drutama("rqtgldipenuhi"))) & "', '" & FixQuotes(drutama("rqtermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rqtgljatuhtempo"))) & "', '" & FixQuotes(drutama("rquraian")) & "', '" & FixQuotes(drutama("rqcatatan")) & "', '" & FixQuotes(drutama("rqnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rqtglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rqtglpenutupan"))) & "', '" & FixQuotes(drutama("rqmatauang")) & "', '" & FixDouble(drutama("rqkurs")) & "', " & drutama("rqhargatermasukpajak") & ", '" & FixDouble(drutama("rqtotal")) & "', '" & FixQuotes(drutama("rqdiskonpersen")) & "', '" & FixDouble(drutama("rqdiskon")) & "', '" & FixDouble(drutama("rqtotalpajak1detail")) & "', '" & FixDouble(drutama("rqtotalpajak2detail")) & "', '" & FixQuotes(drutama("rqbiayalainpersen")) & "', '" & FixDouble(drutama("rqbiayalain")) & "', '" & FixDouble(drutama("rqtotaltransaksi")) & "', " & drutama("rqidpr") & ", " & drutama("rqidcs") & ", " & drutama("rqstatuspo") & ", " & drutama("rqstatusipc") & ", " & drutama("rqstatusgrn") & ", " & drutama("rqstatusri") & ", " & drutama("rqstatusdnr") & ", " & drutama("rqstatusprt") & ", " & drutama("rqstatus") & ", " & drutama("rqstatussebelumnya") & ", " & drutama("rqjmlrevisi") & ", " & drutama("rqcetakanke") & ", " & drutama("rqinputuser") & ", NOW(), " & drutama("rqmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("rqisclose") & ", '" & FixQuotes(drutama("rqcustomtext1")) & "', '" & FixQuotes(drutama("rqcustomtext2")) & "', '" & FixQuotes(drutama("rqcustomtext3")) & "', '" & FixQuotes(drutama("rqcustomtext4")) & "', '" & FixQuotes(drutama("rqcustomtext5")) & "', " & drutama("rqcustomint1") & ", " & drutama("rqcustomint2") & ", " & drutama("rqcustomint3") & ", '" & FixDouble(drutama("rqcustomdbl1")) & "', '" & FixDouble(drutama("rqcustomdbl2")) & "', '" & FixDouble(drutama("rqcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rqcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rqcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rqcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select rqid from M4_rq where rqnotransaksi='" & notransaksi & "' AND rqinputuser= '" & userid & "' order by rqmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Rq_Detail where idrq = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idrqdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idprdetail") & ", " & dr1("idcsdetail") & ", '" & FixDouble(dr1("jmlpo")) & "', " & dr1("statuspo") & ", '" & FixDouble(dr1("jmlipc")) & "', " & dr1("statusipc") & ", '" & FixDouble(dr1("jmlgrn")) & "', " & dr1("statusgrn") & ", '" & FixDouble(dr1("jmlri")) & "', " & dr1("statusri") & ", '" & FixDouble(dr1("jmldnr")) & "', " & dr1("statusdnr") & ", '" & FixDouble(dr1("jmlprt")) & "', " & dr1("statusprt") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Rq_Detail(idrqdetail, idrq, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, jmlpo, statuspo, jmlipc, statusipc, jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                If drutama("rqstatus") = 2 Then
                    If Len(updNilai) > 0 Then
                        'UPDATE OUTSTANDING TRANSAKSI =======================================================
                        'UPDATE DETAIL
                        sql = "UPDATE M4_pr_detail SET jmlrq = (CASE idprdetail " & updNilai & " ELSE jmlrq END) WHERE " & updFilter
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idpr FROM M4_pr_detail WHERE " & updFilter & " GROUP BY idpr")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idpr = '" & dr1("idpr") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDB("SELECT idpr, SUM(jmlbarang) as jmlbarang, SUM(jmlrq) as jmlrq FROM M4_pr_detail WHERE " & ftDetail & " GROUP BY idpr")
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilai = "" : updFilter = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlrq") >= dr1("jmlbarang") Then
                                    statusOut = 2
                                ElseIf dr1("jmlrq") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilai = String.Concat(updNilai, "WHEN '" & dr1("idpr") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                                updFilter = String.Concat(updFilter, "(prid = '" & dr1("idpr") & "')")
                            Next

                            sql = "UPDATE M4_pr SET prstatusrq = (CASE prid " & updNilai & " ELSE prstatusrq END) WHERE " & updFilter
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
                Dim sumber As String = "RQ", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M4_RqUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("rqsupplierkode", "c1.kkode")
            Filter = Filter.Replace("rqsuppliernama", "c1.knama")
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
            Dim sumber As String = "Rq", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Rqtgl, Rqnotransaksi, Rqstatus FROM M4_Rq WHERE Rqid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Rqstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m4_rq_history
            Dim rsSimpanHistory As String = SimpanHistory.M4_Rq_HistorySimpan("" & paramSplit(0) & "★M4_Rq_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m4_rq_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idprdetail As Integer = 0
                Dim ftOutstanding As String = "", updNilai As String = "", updFilter As String = ""
                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDB("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idprdetail, urutan FROM M4_rq_detail WHERE idrq = '" & idtransaksi & "'")
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : idprdetail = dr1("idprdetail")

                        'UPDATE OUTSTANDING ---------------------------
                        If idprdetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING ----------
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idprdetail=" & idprdetail)
                            updNilai = String.Concat("WHEN '" & idprdetail & "' THEN ROUND(jmlrq - '" & Outstanding & "', 5) ", updNilai)
                            '2. SET FILTERUPDATE OUTSTANDING ----------
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(idprdetail = '" & idprdetail & "')")
                        End If
                        'END OF BUAT FILTER UNTUK UPDATE --------------------------
                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                If Len(updFilter) > 0 Then
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE M4_pr_detail SET jmlrq = (CASE idprdetail " & updNilai & " ELSE jmlrq END) WHERE " & updFilter
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idpr FROM M4_pr_detail WHERE " & updFilter & " GROUP BY idpr")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idpr = '" & dr1("idpr") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDB("SELECT idpr, SUM(jmlbarang) as jmlbarang, SUM(jmlrq) as jmlrq FROM M4_pr_detail WHERE " & ftDetail & " GROUP BY idpr")
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilai = "" : updFilter = ""
                        For Each dr1 As DataRow In dtOut.Rows
                            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                            If dr1("jmlrq") >= dr1("jmlbarang") Then
                                statusOut = 2
                            ElseIf dr1("jmlrq") < 1 Then
                                statusOut = 0
                            Else
                                statusOut = 1
                            End If
                            '2. SET NILAI UPDATE OUTSTANDING
                            updNilai = String.Concat(updNilai, "WHEN '" & dr1("idpr") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(prid = '" & dr1("idpr") & "')")
                        Next

                        sql = "UPDATE M4_pr SET prstatusrq = (CASE prid " & updNilai & " ELSE prstatusrq END) WHERE " & updFilter
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
            sql = "UPDATE M4_Rq SET Rqstatus = " & nilaiStatus & ", Rqmodifikasiuser='" & userid & "', Rqmodifikasitgl = NOW(), Rqposting = 0, Rqpostingtgl = '1971-01-01 00:00:00', Rqjmlrevisi = Rqjmlrevisi + 1 WHERE Rqid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_RqSearch(PostWsSearch(paramSplit(0), "M4_RqSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_RqDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("rqsupplierkode", "c1.kkode")
            Filter = Filter.Replace("rqsuppliernama", "c1.knama")
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
            Dim sumber As String = "Rq", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Rqid, Rqnotransaksi FROM M4_Rq WHERE Rqid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT rqcabang, rqlokasi, rqsumber, rqautonotransaksi, rqnotransaksi, rqtgl"
            sql &= " FROM M4_rq"
            sql &= " WHERE rqid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("rqcabang")
                lokasi = dtNomorNext.Rows(0)("rqlokasi")
                sumber = dtNomorNext.Rows(0)("rqsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("rqautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("rqnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("rqtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M4_Rq_Detail WHERE idrq = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M4_Rq WHERE rqid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_RqSearch(PostWsSearch(paramSplit(0), "M4_RqSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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