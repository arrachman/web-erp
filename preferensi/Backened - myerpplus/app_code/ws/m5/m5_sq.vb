Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_sq
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_SqSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataBahan() As String

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
        'sqid(0) As Integer, sqcabang(1) As String, sqlokasi(2) As String, sqgudang(3) As String, sqasalbarang(4) As String, 
        'sqasalbarangkategori(5) As Integer, sqjenispenjualan(6) As String, sqjenispenjualankategori(7) As Integer, sqcarabayar(8) As Integer, sqsumber(9) As String, 
        'sqautonotransaksi(10) As Integer, sqnotransaksi(11) As String, sqtgl(12) As Date, sqkodepa(13) As Integer, sqcustomer(14) As Integer, 
        'sqcustomerkontak(15) As String, sq1alamat1(16) As String, sq1alamat2(17) As String, sq1alamat3(18) As String, sq2alamat1(19) As String, 
        'sq2alamat2(20) As String, sq2alamat3(21) As String, sqbagianpenjualan(22) As Integer, sqtglkirim(23) As Date, sqtermin(24) As String, 
        'sqtgljatuhtempo(25) As Date, squraian(26) As String, sqcatatan(27) As String, sqnoref(28) As String, sqtglnoref(29) As Date, 
        'sqtglpenutupan(30) As Date, sqmatauang(31) As String, sqkurs(32) As Double, sqhargatermasukpajak(33) As Integer, sqtotal(34) As Double, 
        'sqdiskonpersen(35) As String, sqjmldiskon(36) As Double, sqtotalpajak1detail(37) As Double, sqtotalpajak2detail(38) As Double, sqbiayalainpersen(39) As Double, 
        'sqbiayalain(40) As Double, sqtotaltransaksi(41) As Double, sqstatuspr(42) As Integer, sqstatusso(43) As Integer, sqstatuspl(44) As Integer, 
        'sqstatusdo(45) As Integer, sqstatusdr(46) As Integer, sqstatuspi(47) As Integer, sqstatussi(48) As Integer, sqstatusrnr(49) As Integer, 
        'sqstatussr(50) As Integer, sqstatus(51) As Integer, sqstatussebelumnya(52) As Integer, sqjmlrevisi(53) As Integer, sqcetakanke(54) As Integer, 
        'sqinputuser(55) As Integer, sqinputtgl(56) As DateTime, sqmodifikasiuser(57) As Integer, sqmodifikasitgl(58) As DateTime, sqisclose(59) As Integer, 
        'sqcustomtext1(60) As String, sqcustomtext2(61) As String, sqcustomtext3(62) As String, sqcustomtext4(63) As String, sqcustomtext5(64) As String, 
        'sqcustomint1(65) As Integer, sqcustomint2(66) As Integer, sqcustomint3(67) As Integer, sqcustomdbl1(68) As Double, sqcustomdbl2(69) As Double, 
        'sqcustomdbl3(70) As Double, sqcustomdate1(71) As Date, sqcustomdate2(72) As Date, sqcustomdate3(73) As Date, sqidpr(74) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'sqid, sqcabang, sqlokasi, sqgudang, sqasalbarang, sqasalbarangkategori, sqjenispenjualan, 
        'sqjenispenjualankategori, sqcarabayar, sqsumber, sqautonotransaksi, sqnotransaksi, sqtgl, sqkodepa, 
        'sqcustomer, sqcustomerkontak, sq1alamat1, sq1alamat2, sq1alamat3, sq2alamat1, sq2alamat2, 
        'sq2alamat3, sqbagianpenjualan, sqtglkirim, sqtermin, sqtgljatuhtempo, squraian, sqcatatan, 
        'sqnoref, sqtglnoref, sqtglpenutupan, sqmatauang, sqkurs, sqhargatermasukpajak, sqtotal, 
        'sqdiskonpersen, sqjmldiskon, sqtotalpajak1detail, sqtotalpajak2detail, sqbiayalainpersen, sqbiayalain, sqtotaltransaksi, 
        'sqstatuspr, sqstatusso, sqstatuspl, sqstatusdo, sqstatusdr, sqstatuspi, sqstatussi, 
        'sqstatusrnr, sqstatussr, sqstatus, sqstatussebelumnya, sqjmlrevisi, sqcetakanke, sqinputuser, 
        'sqinputtgl, sqmodifikasiuser, sqmodifikasitgl, sqisclose, sqcustomtext1, sqcustomtext2, sqcustomtext3, 
        'sqcustomtext4, sqcustomtext5, sqcustomint1, sqcustomint2, sqcustomint3, sqcustomdbl1, sqcustomdbl2, 
        'sqcustomdbl3, sqcustomdate1, sqcustomdate2, sqcustomdate3, sqidpr

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 75) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'sqid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "sqid required numeric." : GoTo selesai
        End If
        'sqasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "sqasalbarangkategori required numeric." : GoTo selesai
        End If
        'sqjenispenjualankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "sqjenispenjualankategori required numeric." : GoTo selesai
        End If
        'sqcarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "sqcarabayar required numeric." : GoTo selesai
        End If
        'sqautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "sqautonotransaksi required numeric." : GoTo selesai
        End If
        'sqtgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "sqtgl required date." : GoTo selesai
        End If
        'sqkodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "sqkodepa required numeric." : GoTo selesai
        End If
        'sqcustomer(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "sqcustomer required numeric." : GoTo selesai
        End If
        If (dataUtama(14) < 1) Then
            result(2) = "sqcustomer can't be empty." : GoTo selesai
        End If
        'sqbagianpenjualan(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "sqbagianpenjualan required numeric." : GoTo selesai
        End If
        If (dataUtama(22) < 1) Then
            result(2) = "sqbagianpenjualan can't be empty." : GoTo selesai
        End If
        'sqtglkirim(23) As Date
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "sqtglkirim required date." : GoTo selesai
        End If
        'sqtgljatuhtempo(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "sqtgljatuhtempo required date." : GoTo selesai
        End If
        'sqtglnoref(29) As Date
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "sqtglnoref required date." : GoTo selesai
        End If
        'sqtglpenutupan(30) As Date
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "sqtglpenutupan required date." : GoTo selesai
        End If
        'sqkurs(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "sqkurs required numeric." : GoTo selesai
        End If
        'sqhargatermasukpajak(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "sqhargatermasukpajak required numeric." : GoTo selesai
        End If
        'sqtotal(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "sqtotal required numeric." : GoTo selesai
        End If
        'sqjmldiskon(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "sqjmldiskon required numeric." : GoTo selesai
        End If
        'sqtotalpajak1detail(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "sqtotalpajak1detail required numeric." : GoTo selesai
        End If
        'sqtotalpajak2detail(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "sqtotalpajak2detail required numeric." : GoTo selesai
        End If
        ''sqbiayalainpersen(39) As Double
        'If (IsNumeric(dataUtama(39)) = False) Then
        '    result(2) = "sqbiayalainpersen required numeric." : GoTo selesai
        'End If
        'sqbiayalain(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "sqbiayalain required numeric." : GoTo selesai
        End If
        'sqtotaltransaksi(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "sqtotaltransaksi required numeric." : GoTo selesai
        End If
        'sqstatuspr(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "sqstatuspr required numeric." : GoTo selesai
        End If
        'sqstatusso(43) As Integer
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "sqstatusso required numeric." : GoTo selesai
        End If
        'sqstatuspl(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "sqstatuspl required numeric." : GoTo selesai
        End If
        'sqstatusdo(45) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "sqstatusdo required numeric." : GoTo selesai
        End If
        'sqstatusdr(46) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "sqstatusdr required numeric." : GoTo selesai
        End If
        'sqstatuspi(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "sqstatuspi required numeric." : GoTo selesai
        End If
        'sqstatussi(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "sqstatussi required numeric." : GoTo selesai
        End If
        'sqstatusrnr(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "sqstatusrnr required numeric." : GoTo selesai
        End If
        'sqstatussr(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "sqstatussr required numeric." : GoTo selesai
        End If
        'sqstatus(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "sqstatus required numeric." : GoTo selesai
        End If
        'sqstatussebelumnya(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "sqstatussebelumnya required numeric." : GoTo selesai
        End If
        'sqjmlrevisi(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "sqjmlrevisi required numeric." : GoTo selesai
        End If
        'sqcetakanke(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "sqcetakanke required numeric." : GoTo selesai
        End If
        'sqinputuser(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "sqinputuser required numeric." : GoTo selesai
        End If
        'sqinputtgl(56) As DateTime
        If (IsDate(dataUtama(56)) = False) Then
            result(2) = "sqinputtgl required date." : GoTo selesai
        End If
        'sqmodifikasiuser(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "sqmodifikasiuser required numeric." : GoTo selesai
        End If
        'sqmodifikasitgl(58) As DateTime
        If (IsDate(dataUtama(58)) = False) Then
            result(2) = "sqmodifikasitgl required date." : GoTo selesai
        End If
        'sqisclose(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "sqisclose required numeric." : GoTo selesai
        End If
        'sqcustomint1(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "sqcustomint1 required numeric." : GoTo selesai
        End If
        'sqcustomint2(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "sqcustomint2 required numeric." : GoTo selesai
        End If
        'sqcustomint3(67) As Integer
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "sqcustomint3 required numeric." : GoTo selesai
        End If
        'sqcustomdbl1(68) As Double
        If (IsNumeric(dataUtama(68)) = False) Then
            result(2) = "sqcustomdbl1 required numeric." : GoTo selesai
        End If
        'sqcustomdbl2(69) As Double
        If (IsNumeric(dataUtama(69)) = False) Then
            result(2) = "sqcustomdbl2 required numeric." : GoTo selesai
        End If
        'sqcustomdbl3(70) As Double
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "sqcustomdbl3 required numeric." : GoTo selesai
        End If
        'sqcustomdate1(71) As Date
        If (IsDate(dataUtama(71)) = False) Then
            result(2) = "sqcustomdate1 required date." : GoTo selesai
        End If
        'sqcustomdate2(72) As Date
        If (IsDate(dataUtama(72)) = False) Then
            result(2) = "sqcustomdate2 required date." : GoTo selesai
        End If
        'sqcustomdate3(73) As Date
        If (IsDate(dataUtama(73)) = False) Then
            result(2) = "sqcustomdate3 required date." : GoTo selesai
        End If
        'sqidpr(74) As Integer
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "sqidpr required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'sqcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "sqcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "sqcabang should not be more than 25 character." : GoTo selesai
        End If

        'sqlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "sqlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "sqlokasi should not be more than 25 character." : GoTo selesai
        End If

        'sqgudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "sqgudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "sqgudang should not be more than 25 character." : GoTo selesai
        End If

        'sqsumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "sqsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "sqsumber should not be more than 10 character." : GoTo selesai
        End If

        'sqnotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "sqnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "sqnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'sqtgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "sqtgl can't be empty" : GoTo selesai
        End If

        'sqtglkirim(23) As Date
        If Len(dataUtama(23)) = 0 Then
            result(2) = "sqtglkirim can't be empty" : GoTo selesai
        End If

        'sqtgljatuhtempo(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "sqtgljatuhtempo can't be empty" : GoTo selesai
        End If

        'sqtglnoref(29) As Date
        If Len(dataUtama(29)) = 0 Then
            result(2) = "sqtglnoref can't be empty" : GoTo selesai
        End If

        'sqtglpenutupan(30) As Date
        If Len(dataUtama(30)) = 0 Then
            result(2) = "sqtglpenutupan can't be empty" : GoTo selesai
        End If

        'sqmatauang(31) As String
        If Len(dataUtama(31)) = 0 Then
            result(2) = "sqmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(31)) > 25 Then
            result(2) = "sqmatauang should not be more than 25 character." : GoTo selesai
        End If

        'sqkurs(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "sqkurs can't be empty" : GoTo selesai
        End If

        'sqtotal(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "sqtotal can't be empty" : GoTo selesai
        End If

        'sqdiskonpersen(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "sqdiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(35)) > 25 Then
            result(2) = "sqdiskonpersen should not be more than 25 character" : GoTo selesai
        End If

        'sqjmldiskon(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "sqjmldiskon can't be empty" : GoTo selesai
        End If

        'sqtotalpajak1detail(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "sqtotalpajak1detail can't be empty" : GoTo selesai
        End If

        'sqtotalpajak2detail(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "sqtotalpajak2detail can't be empty" : GoTo selesai
        End If

        'sqbiayalainpersen(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "sqbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(39)) > 25 Then
            result(2) = "sqbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'sqbiayalain(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "sqbiayalain can't be empty" : GoTo selesai
        End If

        'sqtotaltransaksi(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "sqtotaltransaksi can't be empty" : GoTo selesai
        End If

        'sqinputtgl(56) As DateTime
        If Len(dataUtama(56)) = 0 Then
            result(2) = "sqinputtgl can't be empty" : GoTo selesai
        End If

        'sqmodifikasitgl(58) As DateTime
        If Len(dataUtama(58)) = 0 Then
            result(2) = "sqmodifikasitgl can't be empty" : GoTo selesai
        End If

        'sqcustomdbl1(68) As Double
        If Len(dataUtama(68)) = 0 Then
            result(2) = "sqcustomdbl1 can't be empty" : GoTo selesai
        End If

        'sqcustomdbl2(69) As Double
        If Len(dataUtama(69)) = 0 Then
            result(2) = "sqcustomdbl2 can't be empty" : GoTo selesai
        End If

        'sqcustomdbl3(70) As Double
        If Len(dataUtama(70)) = 0 Then
            result(2) = "sqcustomdbl3 can't be empty" : GoTo selesai
        End If

        'sqcustomdate1(71) As Date
        If Len(dataUtama(71)) = 0 Then
            result(2) = "sqcustomdate1 can't be empty" : GoTo selesai
        End If

        'sqcustomdate2(72) As Date
        If Len(dataUtama(72)) = 0 Then
            result(2) = "sqcustomdate2 can't be empty" : GoTo selesai
        End If

        'sqcustomdate3(73) As Date
        If Len(dataUtama(73)) = 0 Then
            result(2) = "sqcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "sqid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqjenispenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqjenispenjualankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqcustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqcustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sq1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sq1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sq1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sq2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sq2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sq2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqbagianpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqtglkirim", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqtermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqtgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "squraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqtglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqhargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqtotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqdiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqjmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqtotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqtotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqtotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqstatuspr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqstatusso", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqstatuspl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqstatusdo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqstatusdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqstatuspi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqstatussi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqstatusrnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqstatussr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqidpr", AsEnumTypeData.AsInt64)
        If AsDataTableTambahData(dtutama, "sqid~sqcabang~sqlokasi~sqgudang~sqasalbarang~sqasalbarangkategori~sqjenispenjualan~sqjenispenjualankategori~sqcarabayar~sqsumber~sqautonotransaksi~sqnotransaksi~sqtgl~sqkodepa~sqcustomer~sqcustomerkontak~sq1alamat1~sq1alamat2~sq1alamat3~sq2alamat1~sq2alamat2~sq2alamat3~sqbagianpenjualan~sqtglkirim~sqtermin~sqtgljatuhtempo~squraian~sqcatatan~sqnoref~sqtglnoref~sqtglpenutupan~sqmatauang~sqkurs~sqhargatermasukpajak~sqtotal~sqdiskonpersen~sqjmldiskon~sqtotalpajak1detail~sqtotalpajak2detail~sqbiayalainpersen~sqbiayalain~sqtotaltransaksi~sqstatuspr~sqstatusso~sqstatuspl~sqstatusdo~sqstatusdr~sqstatuspi~sqstatussi~sqstatusrnr~sqstatussr~sqstatus~sqstatussebelumnya~sqjmlrevisi~sqcetakanke~sqinputuser~sqinputtgl~sqmodifikasiuser~sqmodifikasitgl~sqisclose~sqcustomtext1~sqcustomtext2~sqcustomtext3~sqcustomtext4~sqcustomtext5~sqcustomint1~sqcustomint2~sqcustomint3~sqcustomdbl1~sqcustomdbl2~sqcustomdbl3~sqcustomdate1~sqcustomdate2~sqcustomdate3~sqidpr", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idsqdetail(0) As Integer, idsq(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, diskon(13) As String, jmldiskon(14) As Double, 
        'pajak1(15) As String, jmlpajak1(16) As Double, pajak2(17) As String, jmlpajak2(18) As Double, cabang(19) As String, 
        'lokasi(20) As String, gudang(21) As String, costcenter(22) As String, divisi(23) As String, subdivisi(24) As String, 
        'proyek(25) As String, catatan(26) As String, urutan(27) As Integer, jmlpr(28) As Double, statuspr(29) As Integer, 
        'jmlso(30) As Double, statusso(31) As Integer, jmlpl(32) As Double, statuspl(33) As Integer, jmldo(34) As Double, 
        'statusdo(35) As Integer, jmldr(36) As Double, statusdr(37) As Integer, jmlpi(38) As Double, statuspi(39) As Integer, 
        'jmlsi(40) As Double, statussi(41) As Integer, jmlrnr(42) As Double, statusrnr(43) As Integer, jmlsr(44) As Double, 
        'statussr(45) As Integer, isclose(46) As Integer, customtext1(47) As String, customtext2(48) As String, customtext3(49) As String, 
        'customdbl1(50) As Double, customdbl2(51) As Double, customdbl3(52) As Double, customdate1(53) As Date, customdate2(54) As Date, 
        'customdate3(55) As Date, idprdetail(56) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idsqdetail, idsq, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, 
        'jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, 
        'gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'jmlpr, statuspr, jmlso, statusso, jmlpl, statuspl, jmldo, 
        'statusdo, jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, 
        'jmlrnr, statusrnr, jmlsr, statussr, isclose, customtext1, customtext2, 
        'customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, idprdetail


        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idsqdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idsq", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "jmlpr", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuspr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlso", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusso", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlpl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuspl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmldo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusdo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmldr", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlpi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuspi", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "idprdetail", AsEnumTypeData.AsInt64)

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
            If (dataRowDetail.Length <> 57) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idsqdetail(0) As Integer
            'dataRowDetail(0)=0
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idsqdetail required numeric." : GoTo selesai
            End If
            'idsq(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idsq required numeric." : GoTo selesai
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
            'jmlpr(28) As Double
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - jmlpr required numeric." : GoTo selesai
            End If
            'statuspr(29) As Integer
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - statuspr required numeric." : GoTo selesai
            End If
            'jmlso(30) As Double
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - jmlso required numeric." : GoTo selesai
            End If
            'statusso(31) As Integer
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - statusso required numeric." : GoTo selesai
            End If
            'jmlpl(32) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - jmlpl required numeric." : GoTo selesai
            End If
            'statuspl(33) As Integer
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - statuspl required numeric." : GoTo selesai
            End If
            'jmldo(34) As Double
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - jmldo required numeric." : GoTo selesai
            End If
            'statusdo(35) As Integer
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - statusdo required numeric." : GoTo selesai
            End If
            'jmldr(36) As Double
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - jmldr required numeric." : GoTo selesai
            End If
            'statusdr(37) As Integer
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - statusdr required numeric." : GoTo selesai
            End If
            'jmlpi(38) As Double
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - jmlpi required numeric." : GoTo selesai
            End If
            'statuspi(39) As Integer
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - statuspi required numeric." : GoTo selesai
            End If
            'jmlsi(40) As Double
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - jmlsi required numeric." : GoTo selesai
            End If
            'statussi(41) As Integer
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - statussi required numeric." : GoTo selesai
            End If
            'jmlrnr(42) As Double
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - jmlrnr required numeric." : GoTo selesai
            End If
            'statusrnr(43) As Integer
            If (IsNumeric(dataRowDetail(43)) = False) Then
                result(2) = "Row : " & i & " - statusrnr required numeric." : GoTo selesai
            End If
            'jmlsr(44) As Double
            If (IsNumeric(dataRowDetail(44)) = False) Then
                result(2) = "Row : " & i & " - jmlsr required numeric." : GoTo selesai
            End If
            'statussr(45) As Integer
            If (IsNumeric(dataRowDetail(45)) = False) Then
                result(2) = "Row : " & i & " - statussr required numeric." : GoTo selesai
            End If
            'isclose(46) As Integer
            If (IsNumeric(dataRowDetail(46)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(50) As Double
            If (IsNumeric(dataRowDetail(50)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(51) As Double
            If (IsNumeric(dataRowDetail(51)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(52) As Double
            If (IsNumeric(dataRowDetail(52)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(53) As Date
            If (IsDate(dataRowDetail(53)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(54) As Date
            If (IsDate(dataRowDetail(54)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(55) As Date
            If (IsDate(dataRowDetail(55)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'idprdetail(56) As Integer
            If (IsNumeric(dataRowDetail(56)) = False) Then
                result(2) = "Row : " & i & " - Data Detail >> idprdetail required numeric." : GoTo selesai
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
            'If dataRowDetail(5) <= 0 Then
            '    result(2) = "Row : " & i & " - jml can't be less than or equal to zero" : GoTo selesai
            'End If

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
            'If dataRowDetail(8) <= 0 Then
            '    result(2) = "Row : " & i & " - jmlbarang can't be less than or equal to zero" : GoTo selesai
            'End If

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

            'diskon(13) As Double
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

            'jmlpr(28) As Double
            If Len(dataRowDetail(28)) = 0 Then
                result(2) = "Row : " & i & " - jmlpr can't be empty" : GoTo selesai
            End If

            'jmlso(30) As Double
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Row : " & i & " - jmlso can't be empty" : GoTo selesai
            End If

            'jmlpl(32) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - jmlpl can't be empty" : GoTo selesai
            End If

            'jmldo(34) As Double
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Row : " & i & " - jmldo can't be empty" : GoTo selesai
            End If

            'jmldr(36) As Double
            If Len(dataRowDetail(36)) = 0 Then
                result(2) = "Row : " & i & " - jmldr can't be empty" : GoTo selesai
            End If

            'jmlpi(38) As Double
            If Len(dataRowDetail(38)) = 0 Then
                result(2) = "Row : " & i & " - jmlpi can't be empty" : GoTo selesai
            End If

            'jmlsi(40) As Double
            If Len(dataRowDetail(40)) = 0 Then
                result(2) = "Row : " & i & " - jmlsi can't be empty" : GoTo selesai
            End If

            'jmlrnr(42) As Double
            If Len(dataRowDetail(42)) = 0 Then
                result(2) = "Row : " & i & " - jmlrnr can't be empty" : GoTo selesai
            End If

            'jmlsr(44) As Double
            If Len(dataRowDetail(44)) = 0 Then
                result(2) = "Row : " & i & " - jmlsr can't be empty" : GoTo selesai
            End If

            'customdbl1(50) As Double
            If Len(dataRowDetail(50)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(51) As Double
            If Len(dataRowDetail(51)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(52) As Double
            If Len(dataRowDetail(52)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(53) As Date
            If Len(dataRowDetail(53)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(54) As Date
            If Len(dataRowDetail(54)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(55) As Date
            If Len(dataRowDetail(55)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idsqdetail~idsq~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~jmlpr~statuspr~jmlso~statusso~jmlpl~statuspl~jmldo~statusdo~jmldr~statusdr~jmlpi~statuspi~jmlsi~statussi~jmlrnr~statusrnr~jmlsr~statussr~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~idprdetail", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , idprdetail(56) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : idprdetail = dataRowDetail(56)

            'VALIDASI OUTSTANDING -------------------------
            If idprdetail <> 0 Then
                '1. CEK DATA EXIST ------------------------
                ftExistOutstanding = IIf(Len(ftExistOutstanding.ToString) = 0, "", ftExistOutstanding & " UNION ")
                ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m4_pr_detail JOIN m4_pr ON idpr = prid WHERE idprdetail = '" & idprdetail & "' AND (prstatus = 2 OR prstatus = 3 OR prstatus = 4 OR prstatus = 7) LIMIT 1) as rowExists, '" & idprdetail & "' as idprdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idprdetail=" & idprdetail)
                ftOutstanding = IIf(Len(ftOutstanding.ToString) = 0, "", ftOutstanding & " OR ")
                ftOutstanding = String.Concat(ftOutstanding, " (prd.idprdetail = " & idprdetail & " AND " & Outstanding & " > (prd.jmlbarang - prd.jmlsq)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilai = String.Concat("WHEN '" & idprdetail & "' THEN ROUND(jmlsq + '" & Outstanding & "', 5) ", updNilai)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                updFilter = String.Concat(updFilter, "(idprdetail = '" & idprdetail & "')")
            End If
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================



        'Buat datatable detail
        Dim dtbahan As New DataTable
        AsDataTableTambahField(dtbahan, "idsqout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbahan, "idsq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbahan, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbahan, "kodebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbahan, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbahan, "jml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtbahan, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbahan, "hargajual", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtbahan, "subtotal", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtbahan, "standar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbahan, "hargabeli", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtbahan, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbahan, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbahan, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbahan, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbahan, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbahan, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbahan, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbahan, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbahan, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbahan, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbahan, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbahan, "idbarangdetail", AsEnumTypeData.AsInt64)

        If dataSplit.Length > 2 Then
            If dataSplit(2).Length > 0 And dataSplit(2).Length > 4 Then

                'VALIDASI DAN SET DATA ROW DETAIL ==================================================
                dataBahan = dataSplit(2).Split(sptRow)

                Dim JmlDtBahan As Integer = dataBahan.Length
                For i = 1 To JmlDtBahan
                    'SPLIT DATA DETAIL
                    dataRowDetail = dataBahan(i - 1).Split(sptField)

                    'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
                    'CEK ARRAY DATA DETAIL
                    If (dataRowDetail.Length <> 23) Then
                        result(2) = "Row Bahan : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
                    End If
                    'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

                    'VALIDASI TIPE DATA DETAIL ------------------------------------------
                    'idsqdetail(0) As Integer
                    If (IsNumeric(dataRowDetail(0)) = False) Then
                        result(2) = "Row : " & i & " - idsqout required numeric." : GoTo selesai
                    End If
                    'idsq(1) As Integer
                    If (IsNumeric(dataRowDetail(1)) = False) Then
                        result(2) = "Row : " & i & " - idsq required numeric." : GoTo selesai
                    End If
                    'idbarang(2) As Integer
                    If (IsNumeric(dataRowDetail(2)) = False) Then
                        result(2) = "Row : " & i & " - idbarang required numeric." : GoTo selesai
                    End If
                    'jml(5) As Double
                    If (IsNumeric(dataRowDetail(5)) = False) Then
                        result(2) = "Row : " & i & " - jml required numeric." : GoTo selesai
                    End If
                    'harga(12) As Double
                    If (IsNumeric(dataRowDetail(7)) = False) Then
                        result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
                    End If
                    'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

                    'VALIDASI DATA DETAIL ---------------------------------------
                    'namabarang(3) As String
                    'If Len(dataRowDetail(3)) = 0 Then
                    '    result(2) = "Row : " & i & " - namabarang can't be empty" : GoTo selesai
                    'End If
                    'If Len(dataRowDetail(3)) > 100 Then
                    '    result(2) = "Row : " & i & " - namabarang should not be more than 100 character." : GoTo selesai
                    'End If

                    'jml(5) As Double
                    If Len(dataRowDetail(5)) = 0 Then
                        result(2) = "Row : " & i & " - jml can't be empty" : GoTo selesai
                    End If
                    'If dataRowDetail(5) <= 0 Then
                    '    result(2) = "Row : " & i & " - jml can't be less than or equal to zero" : GoTo selesai
                    'End If
                    'harga(12) As Double
                    If Len(dataRowDetail(7)) = 0 Then
                        result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
                    End If
                    'END OF VALIDASI DATA DETAIL --------------------------------

                    If AsDataTableTambahData(dtbahan, "idsqout~idsq~idbarang~kodebarang~namabarang~jml~satuan~hargajual~subtotal~standar~hargabeli~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~idbarangdetail", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22)) = False Then
                        result(2) = "Row Bahan : " & i & " - insert into datatable failed." : GoTo selesai
                    End If
                Next
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
                Dim vModuleId As Integer = 5, vMenuId As Integer = 3
                Select Case drutama("sqstatus")
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("sqtgl")), AsFormatTanggal(drutama("sqtgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("sqstatus") = 2 Or drutama("sqstatus") = 1 Or drutama("sqstatus") = 8 Or drutama("sqstatus") = 9 Or drutama("sqstatus") = 10 Or drutama("sqstatus") = 11 Then
                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstanding, ftOutstanding)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("sqtermin").ToString, AsFormatTanggal(drutama("sqtgl")), "sqtgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("sqtgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TOTAL = subtotal
                drutama("sqtotal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("sqtotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("sqtotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                If Integer.Parse(drutama("sqhargatermasukpajak")) = 0 Then
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                    drutama("sqtotaltransaksi") = Double.Parse(drutama("sqtotal")) - Double.Parse(drutama("sqjmldiskon")) + Double.Parse(drutama("sqtotalpajak1detail")) + Double.Parse(drutama("sqtotalpajak2detail")) + Double.Parse(drutama("sqbiayalain"))

                Else
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                    drutama("sqtotaltransaksi") = Double.Parse(drutama("sqtotal")) - Double.Parse(drutama("sqjmldiskon")) + Double.Parse(drutama("sqtotalpajak2detail")) + Double.Parse(drutama("sqbiayalain"))

                End If
                'END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("sqid")
                    notransaksi = drutama("sqnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(sqid), sqnotransaksi FROM M5_sq WHERE sqid='" & result(4) & "' AND sqstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("sqautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            'Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("sqcabang"), drutama("sqlokasi"), drutama("sqsumber"), drutama("sqtgl"))
                            'Coba Kawata
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("sqcabang"), drutama("sqlokasi"), drutama("sqsumber"), drutama("sqtgl"), , , , userid)
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
                        If notransaksi.ToUpper <> dtupdate.Rows(0)(1).ToString.ToUpper Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(sqid) FROM m5_sq WHERE sqnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_sq_history
                        Dim rsSimpanHistory As String = SimpanHistory.M5_Sq_HistorySimpan("" & paramSplit(0) & "★M5_Sq_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("sqsumber")) & "▼" & FixQuotes(drutama("sqid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Sq set sqcabang  = '" & FixQuotes(drutama("sqcabang")) & "', sqlokasi  = '" & FixQuotes(drutama("sqlokasi")) & "', sqgudang  = '" & FixQuotes(drutama("sqgudang")) & "', sqasalbarang  = '" & FixQuotes(drutama("sqasalbarang")) & "', sqasalbarangkategori  = " & drutama("sqasalbarangkategori") & ", sqjenispenjualan  = '" & FixQuotes(drutama("sqjenispenjualan")) & "', sqjenispenjualankategori  = " & drutama("sqjenispenjualankategori") & ", sqcarabayar  = " & drutama("sqcarabayar") & ", sqsumber  = '" & FixQuotes(drutama("sqsumber")) & "', sqautonotransaksi  = " & drutama("sqautonotransaksi") & ", sqnotransaksi  = '" & notransaksi & "', sqtgl  = '" & FixQuotes(AsFormatTanggal(drutama("sqtgl"))) & "', sqkodepa  = " & drutama("sqkodepa") & ", sqcustomer  = " & drutama("sqcustomer") & ", sqcustomerkontak  = '" & FixQuotes(drutama("sqcustomerkontak")) & "', sq1alamat1  = '" & FixQuotes(drutama("sq1alamat1")) & "', sq1alamat2  = '" & FixQuotes(drutama("sq1alamat2")) & "', sq1alamat3  = '" & FixQuotes(drutama("sq1alamat3")) & "', sq2alamat1  = '" & FixQuotes(drutama("sq2alamat1")) & "', sq2alamat2  = '" & FixQuotes(drutama("sq2alamat2")) & "', sq2alamat3  = '" & FixQuotes(drutama("sq2alamat3")) & "', sqbagianpenjualan  = " & drutama("sqbagianpenjualan") & ", sqtglkirim  = '" & FixQuotes(AsFormatTanggal(drutama("sqtglkirim"))) & "', sqtermin  = '" & FixQuotes(drutama("sqtermin")) & "', sqtgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("sqtgljatuhtempo"))) & "', squraian  = '" & FixQuotes(drutama("squraian")) & "', sqcatatan  = '" & FixQuotes(drutama("sqcatatan")) & "', sqnoref  = '" & FixQuotes(drutama("sqnoref")) & "', sqtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("sqtglnoref"))) & "', sqtglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("sqtglpenutupan"))) & "', sqmatauang  = '" & FixQuotes(drutama("sqmatauang")) & "', sqkurs  = '" & FixDouble(drutama("sqkurs")) & "', sqhargatermasukpajak  = " & drutama("sqhargatermasukpajak") & ", sqtotal  = '" & FixDouble(drutama("sqtotal")) & "', sqdiskonpersen  = '" & FixDouble(drutama("sqdiskonpersen")) & "', sqjmldiskon  = '" & FixDouble(drutama("sqjmldiskon")) & "', sqtotalpajak1detail  = '" & FixDouble(drutama("sqtotalpajak1detail")) & "', sqtotalpajak2detail  = '" & FixDouble(drutama("sqtotalpajak2detail")) & "', sqbiayalainpersen  = '" & FixDouble(drutama("sqbiayalainpersen")) & "', sqbiayalain  = '" & FixDouble(drutama("sqbiayalain")) & "', sqtotaltransaksi  = '" & FixDouble(drutama("sqtotaltransaksi")) & "', sqstatuspr  = " & drutama("sqstatuspr") & ", sqstatusso  = " & drutama("sqstatusso") & ", sqstatuspl  = " & drutama("sqstatuspl") & ", sqstatusdo  = " & drutama("sqstatusdo") & ", sqstatusdr  = " & drutama("sqstatusdr") & ", sqstatuspi  = " & drutama("sqstatuspi") & ", sqstatussi  = " & drutama("sqstatussi") & ", sqstatusrnr  = " & drutama("sqstatusrnr") & ", sqstatussr  = " & drutama("sqstatussr") & ", sqstatus  = " & drutama("sqstatus") & ", sqstatussebelumnya  = " & drutama("sqstatussebelumnya") & ", sqjmlrevisi  = sqjmlrevisi+1, sqcetakanke  = " & drutama("sqcetakanke") & ", sqmodifikasiuser  = " & drutama("sqmodifikasiuser") & ", sqmodifikasitgl  = NOW(), sqcustomtext1  = '" & FixQuotes(drutama("sqcustomtext1")) & "', sqcustomtext2  = '" & FixQuotes(drutama("sqcustomtext2")) & "', sqcustomtext3  = '" & FixQuotes(drutama("sqcustomtext3")) & "', sqcustomtext4  = '" & FixQuotes(drutama("sqcustomtext4")) & "', sqcustomtext5  = '" & FixQuotes(drutama("sqcustomtext5")) & "', sqcustomint1  = " & drutama("sqcustomint1") & ", sqcustomint2  = " & drutama("sqcustomint2") & ", sqcustomint3  = " & drutama("sqcustomint3") & ", sqcustomdbl1  = '" & FixDouble(drutama("sqcustomdbl1")) & "', sqcustomdbl2  = '" & FixDouble(drutama("sqcustomdbl2")) & "', sqcustomdbl3  = '" & FixDouble(drutama("sqcustomdbl3")) & "', sqcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("sqcustomdate1"))) & "', sqcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("sqcustomdate2"))) & "', sqcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("sqcustomdate3"))) & "', sqidpr  = '" & FixDouble(drutama("sqidpr")) & "' where sqid = '" & drutama("sqid") & "'"
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

                    If drutama("sqautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("sqcabang"), drutama("sqlokasi"), drutama("sqsumber"), drutama("sqtgl"), , , , userid)
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
                        notransaksi = drutama("sqnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(sqid) FROM m5_sq WHERE sqnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Sq (sqcabang, sqlokasi, sqgudang, sqasalbarang, sqasalbarangkategori, sqjenispenjualan, sqjenispenjualankategori, sqcarabayar, sqsumber, sqautonotransaksi, sqnotransaksi, sqtgl, sqkodepa, sqcustomer, sqcustomerkontak, sq1alamat1, sq1alamat2, sq1alamat3, sq2alamat1, sq2alamat2, sq2alamat3, sqbagianpenjualan, sqtglkirim, sqtermin, sqtgljatuhtempo, squraian, sqcatatan, sqnoref, sqtglnoref, sqtglpenutupan, sqmatauang, sqkurs, sqhargatermasukpajak, sqtotal, sqdiskonpersen, sqjmldiskon, sqtotalpajak1detail, sqtotalpajak2detail, sqbiayalainpersen, sqbiayalain, sqtotaltransaksi, sqstatuspr, sqstatusso, sqstatuspl, sqstatusdo, sqstatusdr, sqstatuspi, sqstatussi, sqstatusrnr, sqstatussr, sqstatus, sqstatussebelumnya, sqjmlrevisi, sqcetakanke, sqinputuser, sqinputtgl, sqmodifikasiuser, sqmodifikasitgl, sqisclose, sqcustomtext1, sqcustomtext2, sqcustomtext3, sqcustomtext4, sqcustomtext5, sqcustomint1, sqcustomint2, sqcustomint3, sqcustomdbl1, sqcustomdbl2, sqcustomdbl3, sqcustomdate1, sqcustomdate2, sqcustomdate3, sqidpr) values('" & FixQuotes(drutama("sqcabang")) & "', '" & FixQuotes(drutama("sqlokasi")) & "', '" & FixQuotes(drutama("sqgudang")) & "', '" & FixQuotes(drutama("sqasalbarang")) & "', " & drutama("sqasalbarangkategori") & ", '" & FixQuotes(drutama("sqjenispenjualan")) & "', " & drutama("sqjenispenjualankategori") & ", " & drutama("sqcarabayar") & ", '" & FixQuotes(drutama("sqsumber")) & "', " & drutama("sqautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("sqtgl"))) & "', " & drutama("sqkodepa") & ", " & drutama("sqcustomer") & ", '" & FixQuotes(drutama("sqcustomerkontak")) & "', '" & FixQuotes(drutama("sq1alamat1")) & "', '" & FixQuotes(drutama("sq1alamat2")) & "', '" & FixQuotes(drutama("sq1alamat3")) & "', '" & FixQuotes(drutama("sq2alamat1")) & "', '" & FixQuotes(drutama("sq2alamat2")) & "', '" & FixQuotes(drutama("sq2alamat3")) & "', " & drutama("sqbagianpenjualan") & ", '" & FixQuotes(AsFormatTanggal(drutama("sqtglkirim"))) & "', '" & FixQuotes(drutama("sqtermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sqtgljatuhtempo"))) & "', '" & FixQuotes(drutama("squraian")) & "', '" & FixQuotes(drutama("sqcatatan")) & "', '" & FixQuotes(drutama("sqnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sqtglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sqtglpenutupan"))) & "', '" & FixQuotes(drutama("sqmatauang")) & "', '" & FixDouble(drutama("sqkurs")) & "', " & drutama("sqhargatermasukpajak") & ", '" & FixDouble(drutama("sqtotal")) & "', '" & FixDouble(drutama("sqdiskonpersen")) & "', '" & FixDouble(drutama("sqjmldiskon")) & "', '" & FixDouble(drutama("sqtotalpajak1detail")) & "', '" & FixDouble(drutama("sqtotalpajak2detail")) & "', '" & FixDouble(drutama("sqbiayalainpersen")) & "', '" & FixDouble(drutama("sqbiayalain")) & "', '" & FixDouble(drutama("sqtotaltransaksi")) & "', " & drutama("sqstatuspr") & ", " & drutama("sqstatusso") & ", " & drutama("sqstatuspl") & ", " & drutama("sqstatusdo") & ", " & drutama("sqstatusdr") & ", " & drutama("sqstatuspi") & ", " & drutama("sqstatussi") & ", " & drutama("sqstatusrnr") & ", " & drutama("sqstatussr") & ", " & drutama("sqstatus") & ", " & drutama("sqstatussebelumnya") & ", " & drutama("sqjmlrevisi") & ", " & drutama("sqcetakanke") & ", " & drutama("sqinputuser") & ", NOW(), " & drutama("sqmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("sqisclose") & ", '" & FixQuotes(drutama("sqcustomtext1")) & "', '" & FixQuotes(drutama("sqcustomtext2")) & "', '" & FixQuotes(drutama("sqcustomtext3")) & "', '" & FixQuotes(drutama("sqcustomtext4")) & "', '" & FixQuotes(drutama("sqcustomtext5")) & "', " & drutama("sqcustomint1") & ", " & drutama("sqcustomint2") & ", " & drutama("sqcustomint3") & ", '" & FixDouble(drutama("sqcustomdbl1")) & "', '" & FixDouble(drutama("sqcustomdbl2")) & "', '" & FixDouble(drutama("sqcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sqcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sqcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sqcustomdate3"))) & "', '" & FixDouble(drutama("sqidpr")) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select sqid from M5_sq where sqnotransaksi='" & notransaksi & "' AND sqinputuser= '" & userid & "' order by sqmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Sq_Detail where idsq = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idsqdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", '" & FixDouble(dr1("jmlpr")) & "', " & dr1("statuspr") & ", '" & FixDouble(dr1("jmlso")) & "', " & dr1("statusso") & ", '" & FixDouble(dr1("jmlpl")) & "', " & dr1("statuspl") & ", '" & FixDouble(dr1("jmldo")) & "', " & dr1("statusdo") & ", '" & FixDouble(dr1("jmldr")) & "', " & dr1("statusdr") & ", '" & FixDouble(dr1("jmlpi")) & "', " & dr1("statuspi") & ", '" & FixDouble(dr1("jmlsi")) & "', " & dr1("statussi") & ", '" & FixDouble(dr1("jmlrnr")) & "', " & dr1("statusrnr") & ", '" & FixDouble(dr1("jmlsr")) & "', " & dr1("statussr") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixDouble(dr1("idprdetail")) & "')")
                    Next
                    sql = "Insert into M5_Sq_Detail(idsqdetail, idsq, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlpr, statuspr, jmlso, statusso, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, idprdetail) values" & strValue2.ToString & ""
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

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Sq_Out_Bahan where idsq = '" & result(4) & "'"
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
                If (dtbahan.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtbahan.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idsqout") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("kodebarang")) & "', '" & FixQuotes(dr1("namabarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("hargajual")) & "', '" & FixDouble(dr1("subtotal")) & "', " & dr1("standar") & ", '" & FixDouble(dr1("hargabeli")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', " & dr1("idbarangdetail") & ")")
                    Next
                    sql = "Insert into M5_Sq_out_Bahan(idsqout, idsq, idbarang, kodebarang, namabarang, jml, satuan, hargajual, subtotal, standar, hargabeli, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, idbarangdetail) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'Else
                    '    result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                If drutama("sqstatus") = 2 Then
                    If Len(updNilai) > 0 Then
                        'UPDATE OUTSTANDING TRANSAKSI =======================================================
                        'UPDATE DETAIL
                        sql = "UPDATE m4_pr_detail SET jmlsq = (CASE idprdetail " & updNilai & " ELSE jmlsq END) WHERE " & updFilter
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idpr FROM m4_pr_detail WHERE " & updFilter & " GROUP BY idpr", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idpr = '" & dr1("idpr") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idpr, SUM(jmlbarang) as jmlbarang, SUM(jmlsq) as jmlsq FROM m4_pr_detail WHERE " & ftDetail & " GROUP BY idpr", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilai = "" : updFilter = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlsq") >= dr1("jmlbarang") Then
                                    statusOut = 2
                                ElseIf dr1("jmlsq") < 1 Then
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

                            sql = "UPDATE m4_pr SET prstatussq = (CASE prid " & updNilai & " ELSE prstatussq END) WHERE " & updFilter
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
                Dim sumber As String = "SQ", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M5_SqUpdateStatus(ByVal param As String) As String
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
            Dim sumber As String = "Sq", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Sqtgl, Sqnotransaksi, Sqstatus FROM M5_Sq WHERE Sqid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Sqstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m5_sq_history
            Dim rsSimpanHistory As String = SimpanHistory.M5_Sq_HistorySimpan("" & paramSplit(0) & "★M5_Sq_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m5_sq_terkait("sqid = '" & idtransaksi & "'")
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================
            End If


            Dim idbarang As Integer = 0, jmlbarang As Double = 0, idprdetail As Integer = 0
            Dim ftOutstanding As String = "", updNilai As String = "", updFilter As String = ""
            'AMBIL DATA DETAIL
            dtdetail = AsDataTableAmbilDariDBCon("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idprdetail, urutan FROM m5_sq_detail WHERE idsq = '" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 0 Then
                For Each dr1 As DataRow In dtdetail.Rows
                    'BUAT FILTER UNTUK UPDATE ---------------------------------
                    idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : idprdetail = dr1("idprdetail")

                    'UPDATE OUTSTANDING ---------------------------
                    If idprdetail <> 0 Then
                        '1. SET NILAI UPDATE OUTSTANDING ----------
                        Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idprdetail=" & idprdetail)
                        updNilai = String.Concat("WHEN '" & idprdetail & "' THEN ROUND(jmlsq - '" & Outstanding & "', 5) ", updNilai)
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
                sql = "UPDATE m4_pr_detail SET jmlsq = (CASE idprdetail " & updNilai & " ELSE jmlsq END) WHERE " & updFilter
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
                Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idpr FROM m4_pr_detail WHERE " & updFilter & " GROUP BY idpr", myConn)
                If dtOut.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtOut.Rows
                        ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                        ftDetail = String.Concat(ftDetail, "(idpr = '" & dr1("idpr") & "')")
                    Next
                End If
                dtOut = AsDataTableAmbilDariDBCon("SELECT idpr, SUM(jmlbarang) as jmlbarang, SUM(jmlsq) as jmlsq FROM m4_pr_detail WHERE " & ftDetail & " GROUP BY idpr", myConn)
                If dtOut.Rows.Count > 0 Then
                    'KOSONGKAN VARIABEL NILAI DAN FILTER
                    updNilai = "" : updFilter = ""
                    For Each dr1 As DataRow In dtOut.Rows
                        '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                        If dr1("jmlsq") >= dr1("jmlbarang") Then
                            statusOut = 2
                        ElseIf dr1("jmlsq") < 1 Then
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

                    sql = "UPDATE m4_pr SET prstatussq = (CASE prid " & updNilai & " ELSE prstatussq END) WHERE " & updFilter
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


            'update status utama
            sql = "UPDATE M5_Sq SET Sqstatus = " & nilaiStatus & ", Sqmodifikasiuser='" & userid & "', Sqmodifikasitgl = NOW(), Sqposting = 0, Sqpostingtgl = '1971-01-01 00:00:00', Sqjmlrevisi = Sqjmlrevisi + 1 WHERE Sqid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_SqSearch(PostWsSearch(paramSplit(0), "M5_SqSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M5_SqDelete(ByVal param As String) As String

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
            Dim sumber As String = "Sq", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Sqid, Sqnotransaksi FROM M5_Sq WHERE Sqid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = "", sqinputuser As Integer = 0
            sql = "  SELECT sqcabang, sqlokasi, sqsumber, sqautonotransaksi, sqnotransaksi, sqtgl, sqinputuser"
            sql &= " FROM M5_sq"
            sql &= " WHERE sqid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("sqcabang")
                lokasi = dtNomorNext.Rows(0)("sqlokasi")
                sumber = dtNomorNext.Rows(0)("sqsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("sqautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("sqnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("sqtgl"))
                sqinputuser = dtNomorNext.Rows(0)("sqinputuser")
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M5_Sq_Detail WHERE idsq = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M5_Sq WHERE sqid = '" & idtransaksi & "'"
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
                Dim rsNomorNext As String = M0_DeleteNotransaksi(cabang, lokasi, sumber, tgl, notransaksi, , , , sqinputuser)
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
            Dim paramSearch As String = M5_SqSearch(PostWsSearch(paramSplit(0), "M5_SqSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M5_SqGetdataById(ByVal param As String) As String
        'M5_Sq_GetdataById Utama --------------------------------------------------------
        'sqid, sqcabang, sqlokasi, sqgudang, sqasalbarang, sqasalbarangkategori, sqjenispenjualan, 
        'sqjenispenjualankategori, sqcarabayar, sqsumber, sqautonotransaksi, sqnotransaksi, sqtgl, sqkodepa, 
        'sqcustomer, sqcustomerkontak, sq1alamat1, sq1alamat2, sq1alamat3, sq2alamat1, sq2alamat2, 
        'sq2alamat3, sqbagianpenjualan, sqtglkirim, sqtermin, sqtgljatuhtempo, squraian, sqcatatan, 
        'sqnoref, sqtglnoref, sqtglpenutupan, sqmatauang, sqkurs, sqhargatermasukpajak, sqtotal, 
        'sqdiskonpersen, sqjmldiskon, sqtotalpajak1detail, sqtotalpajak2detail, sqbiayalainpersen, sqbiayalain, sqtotaltransaksi, 
        'sqstatuspr, sqstatusso, sqstatuspl, sqstatusdo, sqstatusdr, sqstatuspi, sqstatussi, 
        'sqstatusrnr, sqstatussr, sqstatusrealisasi, sqstatus, sqstatussebelumnya, sqjmlrevisi, sqcetakanke, 
        'sqinputuser, sqinputtgl, sqmodifikasiuser, sqmodifikasitgl, sqposting, sqpostingtgl, sqisclose, 
        'sqcustomtext1, sqcustomtext2, sqcustomtext3, sqcustomtext4, sqcustomtext5, sqcustomint1, sqcustomint2, 
        'sqcustomint3, sqcustomdbl1, sqcustomdbl2, sqcustomdbl3, sqcustomdate1, sqcustomdate2, sqcustomdate3, 
        'sqcabangnama, sqlokasinama, sqgudangnama, sqcustomerkode, sqcustomernama, sqbagianpenjualankode, sqbagianpenjualannama, 
        'sqterminnama, sqterminharijatuhtempo, sqstatusnama, sqstatussebelumnyanama, sqinputusernama, sqmodifikasiusernama, ktingkatjual, kpkp,
        'sqidpr, sqnotransaksipr

        'M5_Sq_GetdataById Detail --------------------------------------------------------
        'idsqdetail, idsq, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, 
        'jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, 
        'pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, 
        'costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlpr, 
        'statuspr, jmlso, statusso, jmlpl, statuspl, jmldo, statusdo, 
        'jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, jmlrnr, 
        'statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, 
        'customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3, kodebarang, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, 
        'lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, idprdetail, prnotransaksi

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

        Dim utama As String = "", detail As String = "", bahan As String = "", shipping As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M5_Sq~M5_Sq_Detail-" & idtransaksi
        Dim Filter2 As String = ""

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "sqid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "sqid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        'Set filter detail 2
        If Len(Filter2) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter2 = "idsq='" & idtransaksi & "'"
        Else ' jika filter diisi
            Filter2 = "idsq='" & idtransaksi & "' and " & Filter2
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m5_sq_getdata")
        sql = "select sq.sqid AS sqid,sq.sqcabang AS sqcabang,sq.sqlokasi AS sqlokasi,sq.sqgudang AS sqgudang,sq.sqasalbarang AS sqasalbarang,sq.sqasalbarangkategori AS sqasalbarangkategori,sq.sqjenispenjualan AS sqjenispenjualan,sq.sqjenispenjualankategori AS sqjenispenjualankategori,sq.sqcarabayar AS sqcarabayar,sq.sqsumber AS sqsumber,sq.sqautonotransaksi AS sqautonotransaksi,sq.sqnotransaksi AS sqnotransaksi,sq.sqtgl AS sqtgl,sq.sqkodepa AS sqkodepa,sq.sqcustomer AS sqcustomer,sq.sqcustomerkontak AS sqcustomerkontak,sq.sq1alamat1 AS sq1alamat1,sq.sq1alamat2 AS sq1alamat2,sq.sq1alamat3 AS sq1alamat3,sq.sq2alamat1 AS sq2alamat1,sq.sq2alamat2 AS sq2alamat2,sq.sq2alamat3 AS sq2alamat3,sq.sqbagianpenjualan AS sqbagianpenjualan,sq.sqtglkirim AS sqtglkirim,sq.sqtermin AS sqtermin,sq.sqtgljatuhtempo AS sqtgljatuhtempo,sq.squraian AS squraian,sq.sqcatatan AS sqcatatan,sq.sqnoref AS sqnoref,sq.sqtglnoref AS sqtglnoref,sq.sqtglpenutupan AS sqtglpenutupan,sq.sqmatauang AS sqmatauang,sq.sqkurs AS sqkurs,sq.sqhargatermasukpajak AS sqhargatermasukpajak,sq.sqtotal AS sqtotal,sq.sqdiskonpersen AS sqdiskonpersen,sq.sqjmldiskon AS sqjmldiskon,sq.sqtotalpajak1detail AS sqtotalpajak1detail,sq.sqtotalpajak2detail AS sqtotalpajak2detail,sq.sqbiayalainpersen AS sqbiayalainpersen,sq.sqbiayalain AS sqbiayalain,sq.sqtotaltransaksi AS sqtotaltransaksi,sq.sqstatuspr AS sqstatuspr,sq.sqstatusso AS sqstatusso,sq.sqstatuspl AS sqstatuspl,sq.sqstatusdo AS sqstatusdo,sq.sqstatusdr AS sqstatusdr,sq.sqstatuspi AS sqstatuspi,sq.sqstatussi AS sqstatussi,sq.sqstatusrnr AS sqstatusrnr,sq.sqstatussr AS sqstatussr,sq.sqstatusrealisasi AS sqstatusrealisasi,sq.sqstatus AS sqstatus,sq.sqstatussebelumnya AS sqstatussebelumnya,sq.sqjmlrevisi AS sqjmlrevisi,sq.sqcetakanke AS sqcetakanke,sq.sqinputuser AS sqinputuser,sq.sqinputtgl AS sqinputtgl,sq.sqmodifikasiuser AS sqmodifikasiuser,sq.sqmodifikasitgl AS sqmodifikasitgl,sq.sqposting AS sqposting,sq.sqpostingtgl AS sqpostingtgl,sq.sqisclose AS sqisclose,sq.sqcustomtext1 AS sqcustomtext1,sq.sqcustomtext2 AS sqcustomtext2,sq.sqcustomtext3 AS sqcustomtext3,sq.sqcustomtext4 AS sqcustomtext4,sq.sqcustomtext5 AS sqcustomtext5,sq.sqcustomint1 AS sqcustomint1,sq.sqcustomint2 AS sqcustomint2,sq.sqcustomint3 AS sqcustomint3,sq.sqcustomdbl1 AS sqcustomdbl1,sq.sqcustomdbl2 AS sqcustomdbl2,sq.sqcustomdbl3 AS sqcustomdbl3,sq.sqcustomdate1 AS sqcustomdate1,sq.sqcustomdate2 AS sqcustomdate2,sq.sqcustomdate3 AS sqcustomdate3,br.bnama AS sqcabangnama,lc.lnama AS sqlokasinama,wh.wnama AS sqgudangnama,c1.ktingkatjual,c1.kkode AS sqcustomerkode,c1.knama AS sqcustomernama,c2.kkode AS sqbagianpenjualankode,c2.knama AS sqbagianpenjualannama,tr.trnama AS sqterminnama,tr.trharijatuhtempo AS sqterminharijatuhtempo,st1.nama AS sqstatusnama,st2.nama AS sqstatussebelumnyanama,u1.unama AS sqinputusernama,u2.unama AS sqmodifikasiusernama,sqd.idsqdetail AS idsqdetail,sqd.idsq AS idsq,sqd.idbarang AS idbarang,sqd.namabarang AS namabarang,sqd.tipebarang AS tipebarang,sqd.jml AS jml,sqd.satuan AS satuan,sqd.nilaisatuan AS nilaisatuan,sqd.jmlbarang AS jmlbarang,sqd.satuanbarang AS satuanbarang,sqd.matauang AS matauang,sqd.kurs AS kurs,sqd.harga AS harga,sqd.diskon AS diskon,sqd.jmldiskon AS jmldiskon,sqd.pajak1 AS pajak1,sqd.jmlpajak1 AS jmlpajak1,sqd.pajak2 AS pajak2,sqd.jmlpajak2 AS jmlpajak2,sqd.cabang AS cabang,sqd.lokasi AS lokasi,sqd.gudang AS gudang,sqd.costcenter AS costcenter,sqd.divisi AS divisi,sqd.subdivisi AS subdivisi,sqd.proyek AS proyek,sqd.catatan AS catatan,sqd.urutan AS urutan,sqd.jmlpr AS jmlpr,sqd.statuspr AS statuspr,sqd.jmlso AS jmlso,sqd.statusso AS statusso,sqd.jmlpl AS jmlpl,sqd.statuspl AS statuspl,sqd.jmldo AS jmldo,sqd.statusdo AS statusdo,sqd.jmldr AS jmldr,sqd.statusdr AS statusdr,sqd.jmlpi AS jmlpi,sqd.statuspi AS statuspi,sqd.jmlsi AS jmlsi,sqd.statussi AS statussi,sqd.jmlrnr AS jmlrnr,sqd.statusrnr AS statusrnr,sqd.jmlsr AS jmlsr,sqd.statussr AS statussr,sqd.jmlrealisasi AS jmlrealisasi,sqd.statusrealisasi AS statusrealisasi,sqd.isclose AS isclose,sqd.customtext1 AS customtext1,sqd.customtext2 AS customtext2,sqd.customtext3 AS customtext3,sqd.customdbl1 AS customdbl1,sqd.customdbl2 AS customdbl2,sqd.customdbl3 AS customdbl3,sqd.customdate1 AS customdate1,sqd.customdate2 AS customdate2,sqd.customdate3 AS customdate3,i.bkode AS kodebarang,t1.tnama AS pajak1nama,t1.tnilai AS pajak1nilai,t2.tnama AS pajak2nama,t2.tnilai AS pajak2nilai,brd.bnama AS cabangnama,lcd.lnama AS lokasinama,whd.wnama AS gudangnama,cc.ccnama AS costcenternama,d.dnama AS divisinama,sd.sdnama AS subdivisinama,p.pnama AS proyeknama, c1.kpkp, sq.sqidpr, pru.prnotransaksi as sqnotransaksipr, sqd.idprdetail, pr.prnotransaksi from m5_sq sq  join m5_sq_detail sqd on sq.sqid = sqd.idsq left join m1_branch br on br.bkode = sq.sqcabang left join m1_location lc on lc.lkode = sq.sqlokasi left join m1_warehouse wh on wh.wkode = sq.sqgudang left join m1_contact c1 on c1.kid = sq.sqcustomer left join m1_contact c2 on c2.kid = sq.sqbagianpenjualan left join m1_terms tr on sq.sqtermin = tr.trkode left join m0_status st1 on st1.kode = sq.sqstatus left join m0_status st2 on st2.kode = sq.sqstatussebelumnya left join m0_user u1 on u1.userid = sq.sqinputuser left join m0_user u2 on u2.userid = sq.sqmodifikasiuser left join m1_item i on i.bid = sqd.idbarang left join m1_tax t1 on sqd.pajak1 = t1.tkode left join m1_tax t2 on sqd.pajak2 = t2.tkode left join m1_branch brd on sqd.cabang = brd.bkode left join m1_location lcd on sqd.lokasi = lcd.lkode left join m1_warehouse whd on sqd.gudang = whd.wkode left join m1_cost_center cc on sqd.costcenter = cc.cckode left join m1_division d on sqd.divisi = d.dkode left join m1_project p on sqd.proyek = p.pkode left join m1_subdivision sd on sqd.subdivisi = sd.sdkode left join m4_pr pru on sq.sqidpr = pru.prid left join m4_pr_detail prd ON sqd.idprdetail = prd.idprdetail left join m4_pr pr on prd.idpr = pr.prid"

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("sqid"), 0), sptField,
                     FxDB(drutama("sqcabang"), ""), sptField,
                     FxDB(drutama("sqlokasi"), ""), sptField,
                     FxDB(drutama("sqgudang"), ""), sptField,
                     FxDB(drutama("sqasalbarang"), ""), sptField,
                     FxDB(drutama("sqasalbarangkategori"), 0), sptField,
                     FxDB(drutama("sqjenispenjualan"), ""), sptField,
                     FxDB(drutama("sqjenispenjualankategori"), 0), sptField,
                     FxDB(drutama("sqcarabayar"), 0), sptField,
                     FxDB(drutama("sqsumber"), ""), sptField,
                     FxDB(drutama("sqautonotransaksi"), 0), sptField,
                     FxDB(drutama("sqnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sqtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("sqkodepa"), 0), sptField,
                     FxDB(drutama("sqcustomer"), 0), sptField,
                     FxDB(drutama("sqcustomerkontak"), ""), sptField,
                     FxDB(drutama("sq1alamat1"), ""), sptField,
                     FxDB(drutama("sq1alamat2"), ""), sptField,
                     FxDB(drutama("sq1alamat3"), ""), sptField,
                     FxDB(drutama("sq2alamat1"), ""), sptField,
                     FxDB(drutama("sq2alamat2"), ""), sptField,
                     FxDB(drutama("sq2alamat3"), ""), sptField,
                     FxDB(drutama("sqbagianpenjualan"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sqtglkirim"), ""), formatTgl), sptField,
                     FxDB(drutama("sqtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sqtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("squraian"), ""), sptField,
                     FxDB(drutama("sqcatatan"), ""), sptField,
                     FxDB(drutama("sqnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sqtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sqtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("sqmatauang"), ""), sptField,
                     FxDB(drutama("sqkurs"), 0), sptField,
                     FxDB(drutama("sqhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("sqtotal"), 0), sptField,
                     FxDB(drutama("sqdiskonpersen"), ""), sptField,
                     FxDB(drutama("sqjmldiskon"), 0), sptField,
                     FxDB(drutama("sqtotalpajak1detail"), 0), sptField,
                     FxDB(drutama("sqtotalpajak2detail"), 0), sptField,
                     FxDB(drutama("sqbiayalainpersen"), 0), sptField,
                     FxDB(drutama("sqbiayalain"), 0), sptField,
                     FxDB(drutama("sqtotaltransaksi"), 0), sptField,
                     FxDB(drutama("sqstatuspr"), 0), sptField,
                     FxDB(drutama("sqstatusso"), 0), sptField,
                     FxDB(drutama("sqstatuspl"), 0), sptField,
                     FxDB(drutama("sqstatusdo"), 0), sptField,
                     FxDB(drutama("sqstatusdr"), 0), sptField,
                     FxDB(drutama("sqstatuspi"), 0), sptField,
                     FxDB(drutama("sqstatussi"), 0), sptField,
                     FxDB(drutama("sqstatusrnr"), 0), sptField,
                     FxDB(drutama("sqstatussr"), 0), sptField,
                     FxDB(drutama("sqstatusrealisasi"), 0), sptField,
                     FxDB(drutama("sqstatus"), 0), sptField,
                     FxDB(drutama("sqstatussebelumnya"), 0), sptField,
                     FxDB(drutama("sqjmlrevisi"), 0), sptField,
                     FxDB(drutama("sqcetakanke"), 0), sptField,
                     FxDB(drutama("sqinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sqinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sqmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sqmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sqposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sqpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sqisclose"), 0), sptField,
                     FxDB(drutama("sqcustomtext1"), ""), sptField,
                     FxDB(drutama("sqcustomtext2"), ""), sptField,
                     FxDB(drutama("sqcustomtext3"), ""), sptField,
                     FxDB(drutama("sqcustomtext4"), ""), sptField,
                     FxDB(drutama("sqcustomtext5"), ""), sptField,
                     FxDB(drutama("sqcustomint1"), 0), sptField,
                     FxDB(drutama("sqcustomint2"), 0), sptField,
                     FxDB(drutama("sqcustomint3"), 0), sptField,
                     FxDB(drutama("sqcustomdbl1"), 0), sptField,
                     FxDB(drutama("sqcustomdbl2"), 0), sptField,
                     FxDB(drutama("sqcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sqcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sqcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sqcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("sqcabangnama"), ""), sptField,
                     FxDB(drutama("sqlokasinama"), ""), sptField,
                     FxDB(drutama("sqgudangnama"), ""), sptField,
                     FxDB(drutama("sqcustomerkode"), ""), sptField,
                     FxDB(drutama("sqcustomernama"), ""), sptField,
                     FxDB(drutama("sqbagianpenjualankode"), ""), sptField,
                     FxDB(drutama("sqbagianpenjualannama"), ""), sptField,
                     FxDB(drutama("sqterminnama"), ""), sptField,
                     FxDB(drutama("sqterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("sqstatusnama"), ""), sptField,
                     FxDB(drutama("sqstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("sqinputusernama"), ""), sptField,
                     FxDB(drutama("sqmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("ktingkatjual"), 0), sptField,
                     FxDB(drutama("kpkp"), 0), sptField,
                     FxDB(drutama("sqidpr"), 0), sptField,
                     FxDB(drutama("sqnotransaksipr"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idsqdetail"), 0), sptField,
                     FxDB(dr("idsq"), 0), sptField,
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
                     FxDB(dr("jmlpr"), 0), sptField,
                     FxDB(dr("statuspr"), 0), sptField,
                     FxDB(dr("jmlso"), 0), sptField,
                     FxDB(dr("statusso"), 0), sptField,
                     FxDB(dr("jmlpl"), 0), sptField,
                     FxDB(dr("statuspl"), 0), sptField,
                     FxDB(dr("jmldo"), 0), sptField,
                     FxDB(dr("statusdo"), 0), sptField,
                     FxDB(dr("jmldr"), 0), sptField,
                     FxDB(dr("statusdr"), 0), sptField,
                     FxDB(dr("jmlpi"), 0), sptField,
                     FxDB(dr("statuspi"), 0), sptField,
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
                     FxDB(dr("idprdetail"), 0), sptField,
                     FxDB(dr("prnotransaksi"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            sql = "SELECT a.*, i.bstok as stokreal FROM m5_sq_out_bahan a JOIN m5_sq b ON a.idsq = b.sqid AND b.sqid = " & idtransaksi & " JOIN m1_item i ON a.idbarang = i.bid"
            Dim dtout As New DataTable
            dtout = AmbilData("aplikasi1-M5_SQ_Out_Bahan", "", "urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)
            For Each dr As DataRow In dtout.Rows
                bahan = String.Concat(bahan, FxDB(dr("idsqout"), 0), sptField,
                     FxDB(dr("idsq"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("hargajual"), 0), sptField,
                     FxDB(dr("subtotal"), 0), sptField,
                     FxDB(dr("standar"), 0), sptField,
                     FxDB(dr("hargabeli"), 0), sptField,
                     FxDB(dr("urutan"), 0), sptField,
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
                     FxDB(dr("idbarangdetail"), 0), sptField,
                     FxDB(dr("stokreal"), 0), sptRow)
            Next
            'bahan = bahan.Substring(0, bahan.Length - sptRow.Length)
            If bahan.Length > 0 Then bahan = bahan.Substring(0, bahan.Length - sptRow.Length) Else bahan = bahan

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, bahan)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sqid, sqcabang, sqlokasi, sqgudang, sqasalbarang, sqasalbarangkategori, sqjenispenjualan, sqjenispenjualankategori, sqcarabayar, sqsumber, sqautonotransaksi, sqnotransaksi, sqtgl, sqkodepa, sqcustomer, sqcustomerkontak, sq1alamat1, sq1alamat2, sq1alamat3, sq2alamat1, sq2alamat2, sq2alamat3, sqbagianpenjualan, sqtglkirim, sqtermin, sqtgljatuhtempo, squraian, sqcatatan, sqnoref, sqtglnoref, sqtglpenutupan, sqmatauang, sqkurs, sqhargatermasukpajak, sqtotal, sqdiskonpersen, sqjmldiskon, sqtotalpajak1detail, sqtotalpajak2detail, sqbiayalainpersen, sqbiayalain, sqtotaltransaksi, sqstatuspr, sqstatusso, sqstatuspl, sqstatusdo, sqstatusdr, sqstatuspi, sqstatussi, sqstatusrnr, sqstatussr, sqstatusrealisasi, sqstatus, sqstatussebelumnya, sqjmlrevisi, sqcetakanke, sqinputuser, sqinputtgl, sqmodifikasiuser, sqmodifikasitgl, sqposting, sqpostingtgl, sqisclose, sqcustomtext1, sqcustomtext2, sqcustomtext3, sqcustomtext4, sqcustomtext5, sqcustomint1, sqcustomint2, sqcustomint3, sqcustomdbl1, sqcustomdbl2, sqcustomdbl3, sqcustomdate1, sqcustomdate2, sqcustomdate3, sqcabangnama, sqlokasinama, sqgudangnama, sqcustomerkode, sqcustomernama, sqbagianpenjualankode, sqbagianpenjualannama, sqterminnama, sqterminharijatuhtempo, sqstatusnama, sqstatussebelumnyanama, sqinputusernama, sqmodifikasiusernama, ktingkatjual, kpkp, sqidpr, sqnotransaksipr" & sptSubParam & "idsqdetail, idsq, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlpr, statuspr, jmlso, statusso, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, idprdetail, prnotransaksi" & sptSubParam & "idsqout, idsq, idbarang, namabarang, jml, satuan, hargajual, subtotal, standar, hargabeli, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, idbarangdetail, stokreal"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SqSearch(ByVal param As String) As String
        'M5_SqSearch --------------------------------------------------------
        'sqid, sqcabang, sqlokasi, sqgudang, sqasalbarang, sqasalbarangkategori, sqjenispenjualan, 
        'sqjenispenjualankategori, sqcarabayar, sqsumber, sqautonotransaksi, sqnotransaksi, sqtgl, sqkodepa, 
        'sqcustomer, sqcustomerkontak, sq1alamat1, sq1alamat2, sq1alamat3, sq2alamat1, sq2alamat2, 
        'sq2alamat3, sqbagianpenjualan, sqtglkirim, sqtermin, sqtgljatuhtempo, squraian, sqcatatan, 
        'sqnoref, sqtglnoref, sqtglpenutupan, sqmatauang, sqkurs, sqhargatermasukpajak, sqtotal, 
        'sqdiskonpersen, sqjmldiskon, sqtotalpajak1detail, sqtotalpajak2detail, sqbiayalainpersen, sqbiayalain, sqtotaltransaksi, 
        'sqstatuspr, sqstatusso, sqstatuspl, sqstatusdo, sqstatusdr, sqstatuspi, sqstatussi, 
        'sqstatusrnr, sqstatussr, sqstatusrealisasi, sqstatus, sqstatussebelumnya, sqjmlrevisi, sqcetakanke, 
        'sqinputuser, sqinputtgl, sqmodifikasiuser, sqmodifikasitgl, sqposting, sqpostingtgl, sqisclose, 
        'sqcabangnama, sqlokasinama, sqgudangnama, sqcustomerkode, sqcustomernama, sqbagianpenjualankode, sqbagianpenjualannama, 
        'sqstatusnama, sqstatussebelumnyanama, sqinputusernama, sqmodifikasiusernama


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

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_sq_v")

        dt = AmbilData("aplikasi1-M5_Sq_V", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("sqid"), 0), sptField,
                     FxDB(dr("sqcabang"), ""), sptField,
                     FxDB(dr("sqlokasi"), ""), sptField,
                     FxDB(dr("sqgudang"), ""), sptField,
                     FxDB(dr("sqasalbarang"), ""), sptField,
                     FxDB(dr("sqasalbarangkategori"), 0), sptField,
                     FxDB(dr("sqjenispenjualan"), ""), sptField,
                     FxDB(dr("sqjenispenjualankategori"), 0), sptField,
                     FxDB(dr("sqcarabayar"), 0), sptField,
                     FxDB(dr("sqsumber"), ""), sptField,
                     FxDB(dr("sqautonotransaksi"), 0), sptField,
                     FxDB(dr("sqnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sqtgl"), ""), formatTgl), sptField,
                     FxDB(dr("sqkodepa"), 0), sptField,
                     FxDB(dr("sqcustomer"), 0), sptField,
                     FxDB(dr("sqcustomerkontak"), ""), sptField,
                     FxDB(dr("sq1alamat1"), ""), sptField,
                     FxDB(dr("sq1alamat2"), ""), sptField,
                     FxDB(dr("sq1alamat3"), ""), sptField,
                     FxDB(dr("sq2alamat1"), ""), sptField,
                     FxDB(dr("sq2alamat2"), ""), sptField,
                     FxDB(dr("sq2alamat3"), ""), sptField,
                     FxDB(dr("sqbagianpenjualan"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sqtglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("sqtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sqtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("squraian"), ""), sptField,
                     FxDB(dr("sqcatatan"), ""), sptField,
                     FxDB(dr("sqnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sqtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sqtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("sqmatauang"), ""), sptField,
                     FxDB(dr("sqkurs"), 0), sptField,
                     FxDB(dr("sqhargatermasukpajak"), 0), sptField,
                     FxDB(dr("sqtotal"), 0), sptField,
                     FxDB(dr("sqdiskonpersen"), ""), sptField,
                     FxDB(dr("sqjmldiskon"), 0), sptField,
                     FxDB(dr("sqtotalpajak1detail"), 0), sptField,
                     FxDB(dr("sqtotalpajak2detail"), 0), sptField,
                     FxDB(dr("sqbiayalainpersen"), 0), sptField,
                     FxDB(dr("sqbiayalain"), 0), sptField,
                     FxDB(dr("sqtotaltransaksi"), 0), sptField,
                     FxDB(dr("sqstatuspr"), 0), sptField,
                     FxDB(dr("sqstatusso"), 0), sptField,
                     FxDB(dr("sqstatuspl"), 0), sptField,
                     FxDB(dr("sqstatusdo"), 0), sptField,
                     FxDB(dr("sqstatusdr"), 0), sptField,
                     FxDB(dr("sqstatuspi"), 0), sptField,
                     FxDB(dr("sqstatussi"), 0), sptField,
                     FxDB(dr("sqstatusrnr"), 0), sptField,
                     FxDB(dr("sqstatussr"), 0), sptField,
                     FxDB(dr("sqstatusrealisasi"), 0), sptField,
                     FxDB(dr("sqstatus"), 0), sptField,
                     FxDB(dr("sqstatussebelumnya"), 0), sptField,
                     FxDB(dr("sqjmlrevisi"), 0), sptField,
                     FxDB(dr("sqcetakanke"), 0), sptField,
                     FxDB(dr("sqinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sqinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sqmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sqmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sqposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sqpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sqisclose"), 0), sptField,
                     FxDB(dr("sqcabangnama"), ""), sptField,
                     FxDB(dr("sqlokasinama"), ""), sptField,
                     FxDB(dr("sqgudangnama"), ""), sptField,
                     FxDB(dr("sqcustomerkode"), ""), sptField,
                     FxDB(dr("sqcustomernama"), ""), sptField,
                     FxDB(dr("sqbagianpenjualankode"), ""), sptField,
                     FxDB(dr("sqbagianpenjualannama"), ""), sptField,
                     FxDB(dr("sqstatusnama"), ""), sptField,
                     FxDB(dr("sqstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("sqinputusernama"), ""), sptField,
                     FxDB(dr("sqmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sqid, sqcabang, sqlokasi, sqgudang, sqasalbarang, sqasalbarangkategori, sqjenispenjualan, sqjenispenjualankategori, sqcarabayar, sqsumber, sqautonotransaksi, sqnotransaksi, sqtgl, sqkodepa, sqcustomer, sqcustomerkontak, sq1alamat1, sq1alamat2, sq1alamat3, sq2alamat1, sq2alamat2, sq2alamat3, sqbagianpenjualan, sqtglkirim, sqtermin, sqtgljatuhtempo, squraian, sqcatatan, sqnoref, sqtglnoref, sqtglpenutupan, sqmatauang, sqkurs, sqhargatermasukpajak, sqtotal, sqdiskonpersen, sqjmldiskon, sqtotalpajak1detail, sqtotalpajak2detail, sqbiayalainpersen, sqbiayalain, sqtotaltransaksi, sqstatuspr, sqstatusso, sqstatuspl, sqstatusdo, sqstatusdr, sqstatuspi, sqstatussi, sqstatusrnr, sqstatussr, sqstatusrealisasi, sqstatus, sqstatussebelumnya, sqjmlrevisi, sqcetakanke, sqinputuser, sqinputtgl, sqmodifikasiuser, sqmodifikasitgl, sqposting, sqpostingtgl, sqisclose, sqcabangnama, sqlokasinama, sqgudangnama, sqcustomerkode, sqcustomernama, sqbagianpenjualankode, sqbagianpenjualannama, sqstatusnama, sqstatussebelumnyanama, sqinputusernama, sqmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_Sq_Detail_VSearch(ByVal param As String) As String
        'M5_Sq_Detail_VSearch --------------------------------------------------------
        'idsqdetail, idsq, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, 
        'jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, 
        'gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'jmlpr, statuspr, jmlso, statusso, jmlpl, statuspl, jmldo, 
        'statusdo, jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, 
        'jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3, sqnotransaksi, squraian, sqcatatan, sqnoref, sqtglnoref, 
        'sqtglkirim, sqcustomerkontak, sq1alamat1, sq1alamat2, sq1alamat3, sq2alamat1, sq2alamat2, 
        'sq2alamat3, sqbagianpenjualan, sqbagianpenjualankode, sqbagianpenjualannama, sqtermin, sqterminnama, sqterminharijatuhtempo, 
        'kodebarang, bhargabeli, bstok, bsuplier, bsuplierkode, bsupliernama, pajak1nama, 
        'pajak1nilai, pajak2nama, pajak2nilai, jmlsisapr, jmlsisaso, jmlsisarealisasi, bjmllapangan, bsatuanlapangan, basset,
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
        'sql = query.PanggilQuery("m5_sq_detail_v")
        'sql = "select `sqd`.`idsqdetail` AS `idsqdetail`,`sqd`.`idsq` AS `idsq`,`sqd`.`idbarang` AS `idbarang`,`sqd`.`namabarang` AS `namabarang`,`sqd`.`tipebarang` AS `tipebarang`,`sqd`.`jml` AS `jml`,`sqd`.`satuan` AS `satuan`,`sqd`.`nilaisatuan` AS `nilaisatuan`,`sqd`.`jmlbarang` AS `jmlbarang`,`sqd`.`satuanbarang` AS `satuanbarang`,`sqd`.`matauang` AS `matauang`,`sqd`.`kurs` AS `kurs`,`sqd`.`harga` AS `harga`,`sqd`.`diskon` AS `diskon`,`sqd`.`jmldiskon` AS `jmldiskon`,`sqd`.`pajak1` AS `pajak1`,`sqd`.`jmlpajak1` AS `jmlpajak1`,`sqd`.`pajak2` AS `pajak2`,`sqd`.`jmlpajak2` AS `jmlpajak2`,`sqd`.`cabang` AS `cabang`,`sqd`.`lokasi` AS `lokasi`,`sqd`.`gudang` AS `gudang`,`sqd`.`costcenter` AS `costcenter`,`sqd`.`divisi` AS `divisi`,`sqd`.`subdivisi` AS `subdivisi`,`sqd`.`proyek` AS `proyek`,`sqd`.`catatan` AS `catatan`,`sqd`.`urutan` AS `urutan`,`sqd`.`jmlpr` AS `jmlpr`,`sqd`.`statuspr` AS `statuspr`,`sqd`.`jmlso` AS `jmlso`,`sqd`.`statusso` AS `statusso`,`sqd`.`jmlpl` AS `jmlpl`,`sqd`.`statuspl` AS `statuspl`,`sqd`.`jmldo` AS `jmldo`,`sqd`.`statusdo` AS `statusdo`,`sqd`.`jmldr` AS `jmldr`,`sqd`.`statusdr` AS `statusdr`,`sqd`.`jmlpi` AS `jmlpi`,`sqd`.`statuspi` AS `statuspi`,`sqd`.`jmlsi` AS `jmlsi`,`sqd`.`statussi` AS `statussi`,`sqd`.`jmlrnr` AS `jmlrnr`,`sqd`.`statusrnr` AS `statusrnr`,`sqd`.`jmlsr` AS `jmlsr`,`sqd`.`statussr` AS `statussr`,`sqd`.`jmlrealisasi` AS `jmlrealisasi`,`sqd`.`statusrealisasi` AS `statusrealisasi`,`sqd`.`isclose` AS `isclose`,`sqd`.`customtext1` AS `customtext1`,`sqd`.`customtext2` AS `customtext2`,`sqd`.`customtext3` AS `customtext3`,`sqd`.`customdbl1` AS `customdbl1`,`sqd`.`customdbl2` AS `customdbl2`,`sqd`.`customdbl3` AS `customdbl3`,`sqd`.`customdate1` AS `customdate1`,`sqd`.`customdate2` AS `customdate2`,`sqd`.`customdate3` AS `customdate3`,`sq`.`sqnotransaksi` AS `sqnotransaksi`,`sq`.`squraian` AS `squraian`,`sq`.`sqcatatan` AS `sqcatatan`,`sq`.`sqnoref` AS `sqnoref`,`sq`.`sqtglnoref` AS `sqtglnoref`,`sq`.`sqtglkirim` AS `sqtglkirim`,`sq`.`sqcustomerkontak` AS `sqcustomerkontak`,`sq`.`sq1alamat1` AS `sq1alamat1`,`sq`.`sq1alamat2` AS `sq1alamat2`,`sq`.`sq1alamat3` AS `sq1alamat3`,`sq`.`sq2alamat1` AS `sq2alamat1`,`sq`.`sq2alamat2` AS `sq2alamat2`,`sq`.`sq2alamat3` AS `sq2alamat3`,`sq`.`sqbagianpenjualan` AS `sqbagianpenjualan`,`c1`.`kkode` AS `sqbagianpenjualankode`,`c1`.`knama` AS `sqbagianpenjualannama`,`sq`.`sqtermin` AS `sqtermin`,`tr`.`trnama` AS `sqterminnama`,`tr`.`trharijatuhtempo` AS `sqterminharijatuhtempo`,`i`.`bkode` AS `kodebarang`,`i`.`bhargabeli` AS `bhargabeli`,`i`.`bstok` AS `bstok`,`i`.`bsuplier` AS `bsuplier`,`c2`.`kkode` AS `bsuplierkode`,`c2`.`knama` AS `bsupliernama`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,((`sqd`.`jmlbarang` - `sqd`.`jmlpr`) / `sqd`.`nilaisatuan`) AS `jmlsisapr`,((`sqd`.`jmlbarang` - `sqd`.`jmlso`) / `sqd`.`nilaisatuan`) AS `jmlsisaso`,((`sqd`.`jmlbarang` - `sqd`.`jmlrealisasi`) / `sqd`.`nilaisatuan`) AS `jmlsisarealisasi`, i.bjmllapangan, i.bsatuanlapangan, i.basset, t1.takunbeli as pajak1akunbeli, t1c1.cnama as pajak1akunbelinama, t1.takunjual as pajak1akunjual, t1c2.cnama as pajak1akunjualnama, t2.takunbeli as pajak2akunbeli, t2c1.cnama as pajak2akunbelinama, t2.takunjual as pajak2akunjual, t2c2.cnama as pajak2akunjualnama from `m5_sq_detail` `sqd` left join `m5_sq` `sq` on `sqd`.`idsq` = `sq`.`sqid` left join `m1_contact` `c1` on `sq`.`sqbagianpenjualan` = `c1`.`kid` left join `m1_terms` `tr` on `sq`.`sqtermin` = `tr`.`trkode` left join `m1_item` `i` on `sqd`.`idbarang` = `i`.`bid` left join `m1_tax` `t1` on `sqd`.`pajak1` = `t1`.`tkode` left join `m1_tax` `t2` on `sqd`.`pajak2` = `t2`.`tkode` left join `m1_contact` `c2` on `i`.`bsuplier` = `c2`.`kid` left join m1_coa t1c1 on t1.takunbeli = t1c1.cnomor left join m1_coa t1c2 on t1.takunjual = t1c2.cnomor left join m1_coa t2c1 on t2.takunbeli = t2c1.cnomor left join m1_coa t2c2 on t2.takunjual = t2c2.cnomor"
        sql = "select `sqd`.`idsqdetail` AS `idsqdetail`,`sqd`.`idsq` AS `idsq`,`sqd`.`idbarang` AS `idbarang`,`sqd`.`namabarang` AS `namabarang`,`sqd`.`tipebarang` AS `tipebarang`,`sqd`.`jml` AS `jml`,`sqd`.`satuan` AS `satuan`,`sqd`.`nilaisatuan` AS `nilaisatuan`,`sqd`.`jmlbarang` AS `jmlbarang`,`sqd`.`satuanbarang` AS `satuanbarang`,`sqd`.`matauang` AS `matauang`,`sqd`.`kurs` AS `kurs`,`sqd`.`harga` AS `harga`,`sqd`.`diskon` AS `diskon`,`sqd`.`jmldiskon` AS `jmldiskon`,`sqd`.`pajak1` AS `pajak1`,`sqd`.`jmlpajak1` AS `jmlpajak1`,`sqd`.`pajak2` AS `pajak2`,`sqd`.`jmlpajak2` AS `jmlpajak2`,`sqd`.`cabang` AS `cabang`,`sqd`.`lokasi` AS `lokasi`,`sqd`.`gudang` AS `gudang`,`sqd`.`costcenter` AS `costcenter`,`sqd`.`divisi` AS `divisi`,`sqd`.`subdivisi` AS `subdivisi`,`sqd`.`proyek` AS `proyek`,`sqd`.`catatan` AS `catatan`,`sqd`.`urutan` AS `urutan`,`sqd`.`jmlpr` AS `jmlpr`,`sqd`.`statuspr` AS `statuspr`,`sqd`.`jmlso` AS `jmlso`,`sqd`.`statusso` AS `statusso`,`sqd`.`jmlpl` AS `jmlpl`,`sqd`.`statuspl` AS `statuspl`,`sqd`.`jmldo` AS `jmldo`,`sqd`.`statusdo` AS `statusdo`,`sqd`.`jmldr` AS `jmldr`,`sqd`.`statusdr` AS `statusdr`,`sqd`.`jmlpi` AS `jmlpi`,`sqd`.`statuspi` AS `statuspi`,`sqd`.`jmlsi` AS `jmlsi`,`sqd`.`statussi` AS `statussi`,`sqd`.`jmlrnr` AS `jmlrnr`,`sqd`.`statusrnr` AS `statusrnr`,`sqd`.`jmlsr` AS `jmlsr`,`sqd`.`statussr` AS `statussr`,`sqd`.`jmlrealisasi` AS `jmlrealisasi`,`sqd`.`statusrealisasi` AS `statusrealisasi`,`sqd`.`isclose` AS `isclose`,`sqd`.`customtext1` AS `customtext1`,`sqd`.`customtext2` AS `customtext2`,`sqd`.`customtext3` AS `customtext3`,`sqd`.`customdbl1` AS `customdbl1`,`sqd`.`customdbl2` AS `customdbl2`,`sqd`.`customdbl3` AS `customdbl3`,`sqd`.`customdate1` AS `customdate1`,`sqd`.`customdate2` AS `customdate2`,`sqd`.`customdate3` AS `customdate3`,sq.*,`c1`.`kkode` AS `sqbagianpenjualankode`,`c1`.`knama` AS `sqbagianpenjualannama`,`sq`.`sqtermin` AS `sqtermin`,`tr`.`trnama` AS `sqterminnama`,`tr`.`trharijatuhtempo` AS `sqterminharijatuhtempo`,`i`.`bkode` AS `kodebarang`,`i`.`bhargabeli` AS `bhargabeli`,`i`.`bstok` AS `bstok`,`i`.`bsuplier` AS `bsuplier`,`c2`.`kkode` AS `bsuplierkode`,`c2`.`knama` AS `bsupliernama`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,((`sqd`.`jmlbarang` - `sqd`.`jmlpr`) / `sqd`.`nilaisatuan`) AS `jmlsisapr`,((`sqd`.`jmlbarang` - `sqd`.`jmlso`) / `sqd`.`nilaisatuan`) AS `jmlsisaso`,((`sqd`.`jmlbarang` - `sqd`.`jmlrealisasi`) / `sqd`.`nilaisatuan`) AS `jmlsisarealisasi`, i.bjmllapangan, i.bsatuanlapangan, i.basset, t1.takunbeli as pajak1akunbeli, t1c1.cnama as pajak1akunbelinama, t1.takunjual as pajak1akunjual, t1c2.cnama as pajak1akunjualnama, t2.takunbeli as pajak2akunbeli, t2c1.cnama as pajak2akunbelinama, t2.takunjual as pajak2akunjual, t2c2.cnama as pajak2akunjualnama, d.dnama AS divisinama, sd.sdnama AS subdivisinama, cc.ccnama AS costcenternama, p.pnama AS proyeknama, i.bhargajual1, i.bdiskonjual1, i.bdiskonjual2, i.bdiskonjual3, i.bdiskonjual4, i.bdiskonjual5, i.bdiskonjual6 from `m5_sq_detail` `sqd` left join `m5_sq` `sq` on `sqd`.`idsq` = `sq`.`sqid` left join `m1_contact` `c1` on `sq`.`sqbagianpenjualan` = `c1`.`kid` left join `m1_terms` `tr` on `sq`.`sqtermin` = `tr`.`trkode` left join `m1_item` `i` on `sqd`.`idbarang` = `i`.`bid` left join `m1_tax` `t1` on `sqd`.`pajak1` = `t1`.`tkode` left join `m1_tax` `t2` on `sqd`.`pajak2` = `t2`.`tkode` left join `m1_contact` `c2` on `i`.`bsuplier` = `c2`.`kid` left join m1_coa t1c1 on t1.takunbeli = t1c1.cnomor left join m1_coa t1c2 on t1.takunjual = t1c2.cnomor left join m1_coa t2c1 on t2.takunbeli = t2c1.cnomor left join m1_coa t2c2 on t2.takunjual = t2c2.cnomor LEFT JOIN m1_division d ON d.dkode = sqd.divisi LEFT JOIN m1_subdivision sd ON sd.sdkode = sqd.subdivisi LEFT JOIN m1_cost_center cc ON cc.cckode = sqd.costcenter LEFT JOIN m1_project p ON p.pkode = sqd.proyek"
        dt = AmbilData("aplikasi1-M5_Sq_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idsqdetail"), 0), sptField,
                     FxDB(dr("idsq"), 0), sptField,
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
                     FxDB(dr("jmlpr"), 0), sptField,
                     FxDB(dr("statuspr"), 0), sptField,
                     FxDB(dr("jmlso"), 0), sptField,
                     FxDB(dr("statusso"), 0), sptField,
                     FxDB(dr("jmlpl"), 0), sptField,
                     FxDB(dr("statuspl"), 0), sptField,
                     FxDB(dr("jmldo"), 0), sptField,
                     FxDB(dr("statusdo"), 0), sptField,
                     FxDB(dr("jmldr"), 0), sptField,
                     FxDB(dr("statusdr"), 0), sptField,
                     FxDB(dr("jmlpi"), 0), sptField,
                     FxDB(dr("statuspi"), 0), sptField,
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
                     FxDB(dr("sqnotransaksi"), ""), sptField,
                     FxDB(dr("squraian"), ""), sptField,
                     FxDB(dr("sqcatatan"), ""), sptField,
                     FxDB(dr("sqnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sqtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sqtglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("sqcustomerkontak"), ""), sptField,
                     FxDB(dr("sq1alamat1"), ""), sptField,
                     FxDB(dr("sq1alamat2"), ""), sptField,
                     FxDB(dr("sq1alamat3"), ""), sptField,
                     FxDB(dr("sq2alamat1"), ""), sptField,
                     FxDB(dr("sq2alamat2"), ""), sptField,
                     FxDB(dr("sq2alamat3"), ""), sptField,
                     FxDB(dr("sqbagianpenjualan"), 0), sptField,
                     FxDB(dr("sqbagianpenjualankode"), ""), sptField,
                     FxDB(dr("sqbagianpenjualannama"), ""), sptField,
                     FxDB(dr("sqtermin"), ""), sptField,
                     FxDB(dr("sqterminnama"), ""), sptField,
                     FxDB(dr("sqterminharijatuhtempo"), 0), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("bhargabeli"), 0), sptField,
                     FxDB(dr("bstok"), 0), sptField,
                     FxDB(dr("bsuplier"), 0), sptField,
                     FxDB(dr("bsuplierkode"), ""), sptField,
                     FxDB(dr("bsupliernama"), ""), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("jmlsisapr"), 0), sptField,
                     FxDB(dr("jmlsisaso"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptField,
                     FxDB(dr("basset"), 0), sptField,
                     FxDB(dr("pajak1akunbeli"), ""), sptField,
                     FxDB(dr("pajak1akunbelinama"), ""), sptField,
                     FxDB(dr("pajak1akunjual"), ""), sptField,
                     FxDB(dr("pajak1akunjualnama"), ""), sptField,
                     FxDB(dr("pajak2akunbeli"), ""), sptField,
                     FxDB(dr("pajak2akunbelinama"), ""), sptField,
                     FxDB(dr("pajak2akunjual"), ""), sptField,
                     FxDB(dr("pajak2akunjualnama"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sqcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sqcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sqcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("sqcustomtext4"), ""), sptField,
                     FxDB(dr("sqcustomtext5"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("bdiskonjual1"), 0), sptField,
                     FxDB(dr("bdiskonjual2"), 0), sptField,
                     FxDB(dr("bdiskonjual3"), 0), sptField,
                     FxDB(dr("bdiskonjual4"), 0), sptField,
                     FxDB(dr("bdiskonjual5"), 0), sptField,
                     FxDB(dr("bdiskonjual6"), 0), sptField,
                     FxDB(dr("bhargajual1"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idsqdetail, idsq, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlpr, statuspr, jmlso, statusso, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, sqnotransaksi, squraian, sqcatatan, sqnoref, sqtglnoref, sqtglkirim, sqcustomerkontak, sq1alamat1, sq1alamat2, sq1alamat3, sq2alamat1, sq2alamat2, sq2alamat3, sqbagianpenjualan, sqbagianpenjualankode, sqbagianpenjualannama, sqtermin, sqterminnama, sqterminharijatuhtempo, kodebarang, bhargabeli, bstok, bsuplier, bsuplierkode, bsupliernama, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisapr, jmlsisaso, jmlsisarealisasi, bjmllapangan, bsatuanlapangan, basset, pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, pajak2akunjualnama, sqcustomdate1, sqcustomdate2, sqcustomdate3, sqcustomtext4, sqcustomtext5, divisinama, subdivisinama, costcenternama, proyeknama, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bdiskonjual6, bhargajual1"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SqTerkait(ByVal param As String) As String
        'M5_SqTerkait --------------------------------------------------------
        'sqid, sqnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "sqid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2) & " AND sqid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "sqid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        ''PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.m5_sq_terkait(Filter)
        sql = m5_sq_terkait(Filter)


        dt = AmbilData("aplikasi1-m5_sq_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("sqid"), 0), sptField,
                     FxDB(dr("sqnotransaksi"), ""), sptField,
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
            result(2) = "Related SQ data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sqid, sqnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SqTerkait_S(ByVal param As String) As String
        'M5_SqTerkait --------------------------------------------------------
        'sqid, sqnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "sqid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2) & " AND sqid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "sqid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        ''PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.m5_sq_terkait(Filter)
        sql = m5_sq_terkait(Filter)


        dt = AmbilData("aplikasi1-m5_sq_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("sqid"), 0), sptField,
                     FxDB(dr("sqnotransaksi"), ""), sptField,
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
            result(2) = "Related SQ data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idtransaksi, notransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function


    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstanding As String, ByVal ftOutstanding As String) As String
        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaiSatuan As Double = 0, sisa As Double = 0
        Dim filterLookup As String = "", gudang As String = "", urutan As String = ""

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

            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
            sql = "SELECT prd.idprdetail, (prd.jmlbarang - prd.jmlsq) as sisasq, i.bid, i.bkode FROM m4_pr_detail AS prd INNER JOIN m1_item AS i ON prd.idbarang = i.bid WHERE " & ftOutstanding
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisasq")

                filterLookup = "idprdetail=" & dtval.Rows(0)("idprdetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in PR, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If
        'END OF VALIDASI OUTSTANDING --------------------------------
selesai:
        Return errmessage
    End Function

    <WebMethod()>
    Public Function m5_sq_terkait(ByVal strFilter As String) As String
        Dim sql As String
        Dim filter1 As String = "", filter2 As String = "", filter3 As String = "", filter4 As String = "", filter5 As String = "", filter6 As String = "", filter7 As String = "", filter8 As String = "", filter9 As String = ""

        'Replace Filter & Sort
        If (strFilter.Length > 0) Then
            filter1 = strFilter
            filter1 = filter1 & " AND ((`m4_pr`.`prstatus` = 2) or (`m4_pr`.`prstatus` = 3) or (`m4_pr`.`prstatus` = 4) or (`m4_pr`.`prstatus` = 7))"

            filter2 = strFilter
            filter2 = filter2 & " AND ((`m5_so`.`sostatus` = 2) or (`m5_so`.`sostatus` = 3) or (`m5_so`.`sostatus` = 4) or (`m5_so`.`sostatus` = 7))"

            filter3 = strFilter
            filter3 = filter3 & " AND ((`m5_pl`.`plstatus` = 2) or (`m5_pl`.`plstatus` = 3) or (`m5_pl`.`plstatus` = 4) or (`m5_pl`.`plstatus` = 7))"

            filter4 = strFilter
            filter4 = filter4 & " AND ((`m5_do`.`dostatus` = 2) or (`m5_do`.`dostatus` = 3) or (`m5_do`.`dostatus` = 4) or (`m5_do`.`dostatus` = 7))"

            filter5 = strFilter
            filter5 = filter5 & " AND ((`m5_dr`.`drstatus` = 2) or (`m5_dr`.`drstatus` = 3) or (`m5_dr`.`drstatus` = 4) or (`m5_dr`.`drstatus` = 7))"

            filter6 = strFilter
            filter6 = filter6 & " AND ((`m5_pi`.`pistatus` = 2) or (`m5_pi`.`pistatus` = 3) or (`m5_pi`.`pistatus` = 4) or (`m5_pi`.`pistatus` = 7))"

            filter7 = strFilter
            filter7 = filter7 & " AND ((`m5_si`.`sistatus` = 2) or (`m5_si`.`sistatus` = 3) or (`m5_si`.`sistatus` = 4) or (`m5_si`.`sistatus` = 7))"

            filter8 = strFilter
            filter8 = filter8 & " AND ((`m5_rnr`.`rnrstatus` = 2) or (`m5_rnr`.`rnrstatus` = 3) or (`m5_rnr`.`rnrstatus` = 4) or (`m5_rnr`.`rnrstatus` = 7))"

            filter9 = strFilter
            filter9 = filter9 & " AND ((`m5_sr`.`srstatus` = 2) or (`m5_sr`.`srstatus` = 3) or (`m5_sr`.`srstatus` = 4) or (`m5_sr`.`srstatus` = 7))"
        Else
            'Default filter
            filter1 = "((`m4_pr`.`prstatus` = 2) or (`m4_pr`.`prstatus` = 3) or (`m4_pr`.`prstatus` = 4) or (`m4_pr`.`prstatus` = 7))"
            filter2 = "((`m5_so`.`sostatus` = 2) or (`m5_so`.`sostatus` = 3) or (`m5_so`.`sostatus` = 4) or (`m5_so`.`sostatus` = 7))"
            filter3 = "((`m5_pl`.`plstatus` = 2) or (`m5_pl`.`plstatus` = 3) or (`m5_pl`.`plstatus` = 4) or (`m5_pl`.`plstatus` = 7))"
            filter4 = "((`m5_do`.`dostatus` = 2) or (`m5_do`.`dostatus` = 3) or (`m5_do`.`dostatus` = 4) or (`m5_do`.`dostatus` = 7))"
            filter5 = "((`m5_dr`.`drstatus` = 2) or (`m5_dr`.`drstatus` = 3) or (`m5_dr`.`drstatus` = 4) or (`m5_dr`.`drstatus` = 7))"
            filter6 = "((`m5_pi`.`pistatus` = 2) or (`m5_pi`.`pistatus` = 3) or (`m5_pi`.`pistatus` = 4) or (`m5_pi`.`pistatus` = 7))"
            filter7 = "((`m5_si`.`sistatus` = 2) or (`m5_si`.`sistatus` = 3) or (`m5_si`.`sistatus` = 4) or (`m5_si`.`sistatus` = 7))"
            filter8 = "((`m5_rnr`.`rnrstatus` = 2) or (`m5_rnr`.`rnrstatus` = 3) or (`m5_rnr`.`rnrstatus` = 4) or (`m5_rnr`.`rnrstatus` = 7))"
            filter9 = "((`m5_sr`.`srstatus` = 2) or (`m5_sr`.`srstatus` = 3) or (`m5_sr`.`srstatus` = 4) or (`m5_sr`.`srstatus` = 7))"
        End If

        If Len(filter1) > 0 Then filter1 = " WHERE " & filter1
        If Len(filter2) > 0 Then filter2 = " WHERE " & filter2
        If Len(filter3) > 0 Then filter3 = " WHERE " & filter3
        If Len(filter4) > 0 Then filter4 = " WHERE " & filter4
        If Len(filter5) > 0 Then filter5 = " WHERE " & filter5
        If Len(filter6) > 0 Then filter6 = " WHERE " & filter6
        If Len(filter7) > 0 Then filter7 = " WHERE " & filter7
        If Len(filter8) > 0 Then filter8 = " WHERE " & filter8
        If Len(filter9) > 0 Then filter9 = " WHERE " & filter9


        sql = "select `sq`.`sqid` AS `sqid`,`sq`.`sqnotransaksi` AS `sqnotransaksi`,'PR' AS `sumber`,`m4_pr`.`prid` AS `idterkait`,`m4_pr`.`prnotransaksi` AS `noterkait`,`m4_pr`.`prtgl` AS `tglterkait`,`m4_pr`.`prinputtgl` AS `inputtglterkait`,`m4_pr`.`prmodifikasitgl` AS `modifikasitglterkait`, 0 as jenisterkait from (((`m5_sq_detail` `sqd` join `m5_sq` `sq` on((`sqd`.`idsq` = `sq`.`sqid`))) join `m4_pr_detail` on((`m4_pr_detail`.`idprdetail` = `sqd`.`idprdetail`))) join `m4_pr` on((`m4_pr_detail`.`idpr` = `m4_pr`.`prid`))) " & filter1 & "  group by `sq`.`sqid`, `m4_pr`.`prid` "
        sql &= " UNION ALL "
        sql &= "select `sq`.`sqid` AS `sqid`,`sq`.`sqnotransaksi` AS `sqnotransaksi`,'PR' AS `sumber`,`m4_pr`.`prid` AS `idterkait`,`m4_pr`.`prnotransaksi` AS `noterkait`,`m4_pr`.`prtgl` AS `tglterkait`,`m4_pr`.`prinputtgl` AS `inputtglterkait`,`m4_pr`.`prmodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_sq_detail` `sqd` join `m5_sq` `sq` on((`sqd`.`idsq` = `sq`.`sqid`))) join `m4_pr_detail` on((`m4_pr_detail`.`idsqdetail` = `sqd`.`idsqdetail`))) join `m4_pr` on((`m4_pr_detail`.`idpr` = `m4_pr`.`prid`))) " & filter1 & "  group by `sq`.`sqid`, `m4_pr`.`prid` "
        sql &= " UNION ALL "
        sql &= "select `sq`.`sqid` AS `sqid`,`sq`.`sqnotransaksi` AS `sqnotransaksi`,'SO' AS `sumber`,`m5_so`.`soid` AS `idterkait`,`m5_so`.`sonotransaksi` AS `noterkait`,`m5_so`.`sotgl` AS `tglterkait`,`m5_so`.`soinputtgl` AS `inputtglterkait`,`m5_so`.`somodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_sq_detail` `sqd` join `m5_sq` `sq` on((`sqd`.`idsq` = `sq`.`sqid`))) join `m5_so_detail` on((`m5_so_detail`.`idsqdetail` = `sqd`.`idsqdetail`))) join `m5_so` on((`m5_so_detail`.`idso` = `m5_so`.`soid`))) " & filter2 & "  group by `sq`.`sqid`, `m5_so`.`soid` "
        sql &= " UNION ALL "
        sql &= "select `sq`.`sqid` AS `sqid`,`sq`.`sqnotransaksi` AS `sqnotransaksi`,'PL' AS `sumber`,`m5_pl`.`plid` AS `idterkait`,`m5_pl`.`plnotransaksi` AS `noterkait`,`m5_pl`.`pltgl` AS `tglterkait`,`m5_pl`.`plinputtgl` AS `inputtglterkait`,`m5_pl`.`plmodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_sq_detail` `sqd` join `m5_sq` `sq` on((`sqd`.`idsq` = `sq`.`sqid`))) join `m5_pl_detail` on((`m5_pl_detail`.`idsqdetail` = `sqd`.`idsqdetail`))) join `m5_pl` on((`m5_pl_detail`.`idpl` = `m5_pl`.`plid`))) " & filter3 & "  group by `sq`.`sqid`, `m5_pl`.`plid` "
        sql &= " UNION ALL "
        sql &= "select `sq`.`sqid` AS `sqid`,`sq`.`sqnotransaksi` AS `sqnotransaksi`,'DO' AS `sumber`,`m5_do`.`doid` AS `idterkait`,`m5_do`.`donotransaksi` AS `noterkait`,`m5_do`.`dotgl` AS `tglterkait`,`m5_do`.`doinputtgl` AS `inputtglterkait`,`m5_do`.`domodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_sq_detail` `sqd` join `m5_sq` `sq` on((`sqd`.`idsq` = `sq`.`sqid`))) join `m5_do_detail` on((`m5_do_detail`.`idsqdetail` = `sqd`.`idsqdetail`))) join `m5_do` on((`m5_do_detail`.`iddo` = `m5_do`.`doid`))) " & filter4 & "  group by `sq`.`sqid`, `m5_do`.`doid` "
        sql &= " UNION ALL "
        sql &= "select `sq`.`sqid` AS `sqid`,`sq`.`sqnotransaksi` AS `sqnotransaksi`,'DR' AS `sumber`,`m5_dr`.`drid` AS `idterkait`,`m5_dr`.`drnotransaksi` AS `noterkait`,`m5_dr`.`drtgl` AS `tglterkait`,`m5_dr`.`drinputtgl` AS `inputtglterkait`,`m5_dr`.`drmodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_sq_detail` `sqd` join `m5_sq` `sq` on((`sqd`.`idsq` = `sq`.`sqid`))) join `m5_dr_detail` on((`m5_dr_detail`.`idsqdetail` = `sqd`.`idsqdetail`))) join `m5_dr` on((`m5_dr_detail`.`iddr` = `m5_dr`.`drid`))) " & filter5 & "  group by `sq`.`sqid`, `m5_dr`.`drid` "
        sql &= " UNION ALL "
        sql &= "select `sq`.`sqid` AS `sqid`,`sq`.`sqnotransaksi` AS `sqnotransaksi`,'PI' AS `sumber`,`m5_pi`.`piid` AS `idterkait`,`m5_pi`.`pinotransaksi` AS `noterkait`,`m5_pi`.`pitgl` AS `tglterkait`,`m5_pi`.`piinputtgl` AS `inputtglterkait`,`m5_pi`.`pimodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_sq_detail` `sqd` join `m5_sq` `sq` on((`sqd`.`idsq` = `sq`.`sqid`))) join `m5_pi_detail` on((`m5_pi_detail`.`idsqdetail` = `sqd`.`idsqdetail`))) join `m5_pi` on((`m5_pi_detail`.`idpi` = `m5_pi`.`piid`))) " & filter6 & "  group by `sq`.`sqid`, `m5_pi`.`piid` "
        sql &= " UNION ALL "
        sql &= "select `sq`.`sqid` AS `sqid`,`sq`.`sqnotransaksi` AS `sqnotransaksi`,'SI' AS `sumber`,`m5_si`.`siid` AS `idterkait`,`m5_si`.`sinotransaksi` AS `noterkait`,`m5_si`.`sitgl` AS `tglterkait`,`m5_si`.`siinputtgl` AS `inputtglterkait`,`m5_si`.`simodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_sq_detail` `sqd` join `m5_sq` `sq` on((`sqd`.`idsq` = `sq`.`sqid`))) join `m5_si_detail` on((`m5_si_detail`.`idsqdetail` = `sqd`.`idsqdetail`))) join `m5_si` on((`m5_si_detail`.`idsi` = `m5_si`.`siid`))) " & filter7 & "  group by `sq`.`sqid`, `m5_si`.`siid` "
        sql &= " UNION ALL "
        sql &= "select `sq`.`sqid` AS `sqid`,`sq`.`sqnotransaksi` AS `sqnotransaksi`,'RNR' AS `sumber`,`m5_rnr`.`rnrid` AS `idterkait`,`m5_rnr`.`rnrnotransaksi` AS `noterkait`,`m5_rnr`.`rnrtgl` AS `tglterkait`,`m5_rnr`.`rnrinputtgl` AS `inputtglterkait`,`m5_rnr`.`rnrmodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_sq_detail` `sqd` join `m5_sq` `sq` on((`sqd`.`idsq` = `sq`.`sqid`))) join `m5_rnr_detail` on((`m5_rnr_detail`.`idsqdetail` = `sqd`.`idsqdetail`))) join `m5_rnr` on((`m5_rnr_detail`.`idrnr` = `m5_rnr`.`rnrid`))) " & filter8 & "  group by `sq`.`sqid`, `m5_rnr`.`rnrid` "
        sql &= " UNION ALL "
        sql &= "select `sq`.`sqid` AS `sqid`,`sq`.`sqnotransaksi` AS `sqnotransaksi`,'SR' AS `sumber`,`m5_sr`.`srid` AS `idterkait`,`m5_sr`.`srnotransaksi` AS `noterkait`,`m5_sr`.`srtgl` AS `tglterkait`,`m5_sr`.`srinputtgl` AS `inputtglterkait`,`m5_sr`.`srmodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_sq_detail` `sqd` join `m5_sq` `sq` on((`sqd`.`idsq` = `sq`.`sqid`))) join `m5_sr_detail` on((`m5_sr_detail`.`idsqdetail` = `sqd`.`idsqdetail`))) join `m5_sr` on((`m5_sr_detail`.`idsr` = `m5_sr`.`srid`))) " & filter9 & "  group by `sq`.`sqid`, `m5_sr`.`srid`"

        Return sql
    End Function

    <WebMethod()>
    Public Function M5_SqSimpanOld(ByVal param As String) As String
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
        'sqid(0) As Integer, sqcabang(1) As String, sqlokasi(2) As String, sqgudang(3) As String, sqasalbarang(4) As String, 
        'sqasalbarangkategori(5) As Integer, sqjenispenjualan(6) As String, sqjenispenjualankategori(7) As Integer, sqcarabayar(8) As Integer, sqsumber(9) As String, 
        'sqautonotransaksi(10) As Integer, sqnotransaksi(11) As String, sqtgl(12) As Date, sqkodepa(13) As Integer, sqcustomer(14) As Integer, 
        'sqcustomerkontak(15) As String, sq1alamat1(16) As String, sq1alamat2(17) As String, sq1alamat3(18) As String, sq2alamat1(19) As String, 
        'sq2alamat2(20) As String, sq2alamat3(21) As String, sqbagianpenjualan(22) As Integer, sqtglkirim(23) As Date, sqtermin(24) As String, 
        'sqtgljatuhtempo(25) As Date, squraian(26) As String, sqcatatan(27) As String, sqnoref(28) As String, sqtglnoref(29) As Date, 
        'sqtglpenutupan(30) As Date, sqmatauang(31) As String, sqkurs(32) As Double, sqhargatermasukpajak(33) As Integer, sqtotal(34) As Double, 
        'sqdiskonpersen(35) As String, sqjmldiskon(36) As Double, sqtotalpajak1detail(37) As Double, sqtotalpajak2detail(38) As Double, sqbiayalainpersen(39) As Double, 
        'sqbiayalain(40) As Double, sqtotaltransaksi(41) As Double, sqstatuspr(42) As Integer, sqstatusso(43) As Integer, sqstatuspl(44) As Integer, 
        'sqstatusdo(45) As Integer, sqstatusdr(46) As Integer, sqstatuspi(47) As Integer, sqstatussi(48) As Integer, sqstatusrnr(49) As Integer, 
        'sqstatussr(50) As Integer, sqstatus(51) As Integer, sqstatussebelumnya(52) As Integer, sqjmlrevisi(53) As Integer, sqcetakanke(54) As Integer, 
        'sqinputuser(55) As Integer, sqinputtgl(56) As DateTime, sqmodifikasiuser(57) As Integer, sqmodifikasitgl(58) As DateTime, sqisclose(59) As Integer, 
        'sqcustomtext1(60) As String, sqcustomtext2(61) As String, sqcustomtext3(62) As String, sqcustomtext4(63) As String, sqcustomtext5(64) As String, 
        'sqcustomint1(65) As Integer, sqcustomint2(66) As Integer, sqcustomint3(67) As Integer, sqcustomdbl1(68) As Double, sqcustomdbl2(69) As Double, 
        'sqcustomdbl3(70) As Double, sqcustomdate1(71) As Date, sqcustomdate2(72) As Date, sqcustomdate3(73) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'sqid, sqcabang, sqlokasi, sqgudang, sqasalbarang, sqasalbarangkategori, sqjenispenjualan, 
        'sqjenispenjualankategori, sqcarabayar, sqsumber, sqautonotransaksi, sqnotransaksi, sqtgl, sqkodepa, 
        'sqcustomer, sqcustomerkontak, sq1alamat1, sq1alamat2, sq1alamat3, sq2alamat1, sq2alamat2, 
        'sq2alamat3, sqbagianpenjualan, sqtglkirim, sqtermin, sqtgljatuhtempo, squraian, sqcatatan, 
        'sqnoref, sqtglnoref, sqtglpenutupan, sqmatauang, sqkurs, sqhargatermasukpajak, sqtotal, 
        'sqdiskonpersen, sqjmldiskon, sqtotalpajak1detail, sqtotalpajak2detail, sqbiayalainpersen, sqbiayalain, sqtotaltransaksi, 
        'sqstatuspr, sqstatusso, sqstatuspl, sqstatusdo, sqstatusdr, sqstatuspi, sqstatussi, 
        'sqstatusrnr, sqstatussr, sqstatus, sqstatussebelumnya, sqjmlrevisi, sqcetakanke, sqinputuser, 
        'sqinputtgl, sqmodifikasiuser, sqmodifikasitgl, sqisclose, sqcustomtext1, sqcustomtext2, sqcustomtext3, 
        'sqcustomtext4, sqcustomtext5, sqcustomint1, sqcustomint2, sqcustomint3, sqcustomdbl1, sqcustomdbl2, 
        'sqcustomdbl3, sqcustomdate1, sqcustomdate2, sqcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 74) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'sqid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "sqid required numeric." : GoTo selesai
        End If
        'sqasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "sqasalbarangkategori required numeric." : GoTo selesai
        End If
        'sqjenispenjualankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "sqjenispenjualankategori required numeric." : GoTo selesai
        End If
        'sqcarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "sqcarabayar required numeric." : GoTo selesai
        End If
        'sqautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "sqautonotransaksi required numeric." : GoTo selesai
        End If
        'sqtgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "sqtgl required date." : GoTo selesai
        End If
        'sqkodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "sqkodepa required numeric." : GoTo selesai
        End If
        'sqcustomer(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "sqcustomer required numeric." : GoTo selesai
        End If
        If (dataUtama(14) < 1) Then
            result(2) = "sqcustomer can't be empty." : GoTo selesai
        End If
        'sqbagianpenjualan(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "sqbagianpenjualan required numeric." : GoTo selesai
        End If
        If (dataUtama(22) < 1) Then
            result(2) = "sqbagianpenjualan can't be empty." : GoTo selesai
        End If
        'sqtglkirim(23) As Date
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "sqtglkirim required date." : GoTo selesai
        End If
        'sqtgljatuhtempo(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "sqtgljatuhtempo required date." : GoTo selesai
        End If
        'sqtglnoref(29) As Date
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "sqtglnoref required date." : GoTo selesai
        End If
        'sqtglpenutupan(30) As Date
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "sqtglpenutupan required date." : GoTo selesai
        End If
        'sqkurs(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "sqkurs required numeric." : GoTo selesai
        End If
        'sqhargatermasukpajak(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "sqhargatermasukpajak required numeric." : GoTo selesai
        End If
        'sqtotal(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "sqtotal required numeric." : GoTo selesai
        End If
        'sqjmldiskon(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "sqjmldiskon required numeric." : GoTo selesai
        End If
        'sqtotalpajak1detail(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "sqtotalpajak1detail required numeric." : GoTo selesai
        End If
        'sqtotalpajak2detail(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "sqtotalpajak2detail required numeric." : GoTo selesai
        End If
        ''sqbiayalainpersen(39) As Double
        'If (IsNumeric(dataUtama(39)) = False) Then
        '    result(2) = "sqbiayalainpersen required numeric." : GoTo selesai
        'End If
        'sqbiayalain(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "sqbiayalain required numeric." : GoTo selesai
        End If
        'sqtotaltransaksi(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "sqtotaltransaksi required numeric." : GoTo selesai
        End If
        'sqstatuspr(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "sqstatuspr required numeric." : GoTo selesai
        End If
        'sqstatusso(43) As Integer
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "sqstatusso required numeric." : GoTo selesai
        End If
        'sqstatuspl(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "sqstatuspl required numeric." : GoTo selesai
        End If
        'sqstatusdo(45) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "sqstatusdo required numeric." : GoTo selesai
        End If
        'sqstatusdr(46) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "sqstatusdr required numeric." : GoTo selesai
        End If
        'sqstatuspi(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "sqstatuspi required numeric." : GoTo selesai
        End If
        'sqstatussi(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "sqstatussi required numeric." : GoTo selesai
        End If
        'sqstatusrnr(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "sqstatusrnr required numeric." : GoTo selesai
        End If
        'sqstatussr(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "sqstatussr required numeric." : GoTo selesai
        End If
        'sqstatus(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "sqstatus required numeric." : GoTo selesai
        End If
        'sqstatussebelumnya(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "sqstatussebelumnya required numeric." : GoTo selesai
        End If
        'sqjmlrevisi(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "sqjmlrevisi required numeric." : GoTo selesai
        End If
        'sqcetakanke(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "sqcetakanke required numeric." : GoTo selesai
        End If
        'sqinputuser(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "sqinputuser required numeric." : GoTo selesai
        End If
        'sqinputtgl(56) As DateTime
        If (IsDate(dataUtama(56)) = False) Then
            result(2) = "sqinputtgl required date." : GoTo selesai
        End If
        'sqmodifikasiuser(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "sqmodifikasiuser required numeric." : GoTo selesai
        End If
        'sqmodifikasitgl(58) As DateTime
        If (IsDate(dataUtama(58)) = False) Then
            result(2) = "sqmodifikasitgl required date." : GoTo selesai
        End If
        'sqisclose(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "sqisclose required numeric." : GoTo selesai
        End If
        'sqcustomint1(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "sqcustomint1 required numeric." : GoTo selesai
        End If
        'sqcustomint2(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "sqcustomint2 required numeric." : GoTo selesai
        End If
        'sqcustomint3(67) As Integer
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "sqcustomint3 required numeric." : GoTo selesai
        End If
        'sqcustomdbl1(68) As Double
        If (IsNumeric(dataUtama(68)) = False) Then
            result(2) = "sqcustomdbl1 required numeric." : GoTo selesai
        End If
        'sqcustomdbl2(69) As Double
        If (IsNumeric(dataUtama(69)) = False) Then
            result(2) = "sqcustomdbl2 required numeric." : GoTo selesai
        End If
        'sqcustomdbl3(70) As Double
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "sqcustomdbl3 required numeric." : GoTo selesai
        End If
        'sqcustomdate1(71) As Date
        If (IsDate(dataUtama(71)) = False) Then
            result(2) = "sqcustomdate1 required date." : GoTo selesai
        End If
        'sqcustomdate2(72) As Date
        If (IsDate(dataUtama(72)) = False) Then
            result(2) = "sqcustomdate2 required date." : GoTo selesai
        End If
        'sqcustomdate3(73) As Date
        If (IsDate(dataUtama(73)) = False) Then
            result(2) = "sqcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'sqcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "sqcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "sqcabang should not be more than 25 character." : GoTo selesai
        End If

        'sqlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "sqlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "sqlokasi should not be more than 25 character." : GoTo selesai
        End If

        'sqgudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "sqgudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "sqgudang should not be more than 25 character." : GoTo selesai
        End If

        'sqsumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "sqsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "sqsumber should not be more than 10 character." : GoTo selesai
        End If

        'sqnotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "sqnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "sqnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'sqtgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "sqtgl can't be empty" : GoTo selesai
        End If

        'sqtglkirim(23) As Date
        If Len(dataUtama(23)) = 0 Then
            result(2) = "sqtglkirim can't be empty" : GoTo selesai
        End If

        'sqtgljatuhtempo(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "sqtgljatuhtempo can't be empty" : GoTo selesai
        End If

        'sqtglnoref(29) As Date
        If Len(dataUtama(29)) = 0 Then
            result(2) = "sqtglnoref can't be empty" : GoTo selesai
        End If

        'sqtglpenutupan(30) As Date
        If Len(dataUtama(30)) = 0 Then
            result(2) = "sqtglpenutupan can't be empty" : GoTo selesai
        End If

        'sqmatauang(31) As String
        If Len(dataUtama(31)) = 0 Then
            result(2) = "sqmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(31)) > 25 Then
            result(2) = "sqmatauang should not be more than 25 character." : GoTo selesai
        End If

        'sqkurs(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "sqkurs can't be empty" : GoTo selesai
        End If

        'sqtotal(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "sqtotal can't be empty" : GoTo selesai
        End If

        'sqdiskonpersen(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "sqdiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(35)) > 25 Then
            result(2) = "sqdiskonpersen should not be more than 25 character" : GoTo selesai
        End If

        'sqjmldiskon(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "sqjmldiskon can't be empty" : GoTo selesai
        End If

        'sqtotalpajak1detail(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "sqtotalpajak1detail can't be empty" : GoTo selesai
        End If

        'sqtotalpajak2detail(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "sqtotalpajak2detail can't be empty" : GoTo selesai
        End If

        'sqbiayalainpersen(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "sqbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(39)) > 25 Then
            result(2) = "sqbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'sqbiayalain(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "sqbiayalain can't be empty" : GoTo selesai
        End If

        'sqtotaltransaksi(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "sqtotaltransaksi can't be empty" : GoTo selesai
        End If

        'sqinputtgl(56) As DateTime
        If Len(dataUtama(56)) = 0 Then
            result(2) = "sqinputtgl can't be empty" : GoTo selesai
        End If

        'sqmodifikasitgl(58) As DateTime
        If Len(dataUtama(58)) = 0 Then
            result(2) = "sqmodifikasitgl can't be empty" : GoTo selesai
        End If

        'sqcustomdbl1(68) As Double
        If Len(dataUtama(68)) = 0 Then
            result(2) = "sqcustomdbl1 can't be empty" : GoTo selesai
        End If

        'sqcustomdbl2(69) As Double
        If Len(dataUtama(69)) = 0 Then
            result(2) = "sqcustomdbl2 can't be empty" : GoTo selesai
        End If

        'sqcustomdbl3(70) As Double
        If Len(dataUtama(70)) = 0 Then
            result(2) = "sqcustomdbl3 can't be empty" : GoTo selesai
        End If

        'sqcustomdate1(71) As Date
        If Len(dataUtama(71)) = 0 Then
            result(2) = "sqcustomdate1 can't be empty" : GoTo selesai
        End If

        'sqcustomdate2(72) As Date
        If Len(dataUtama(72)) = 0 Then
            result(2) = "sqcustomdate2 can't be empty" : GoTo selesai
        End If

        'sqcustomdate3(73) As Date
        If Len(dataUtama(73)) = 0 Then
            result(2) = "sqcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "sqid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqjenispenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqjenispenjualankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqcustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqcustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sq1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sq1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sq1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sq2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sq2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sq2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqbagianpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqtglkirim", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqtermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqtgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "squraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqtglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqhargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqtotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqdiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqjmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqtotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqtotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqtotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqstatuspr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqstatusso", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqstatuspl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqstatusdo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqstatusdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqstatuspi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqstatussi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqstatusrnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqstatussr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sqcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sqcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "sqid~sqcabang~sqlokasi~sqgudang~sqasalbarang~sqasalbarangkategori~sqjenispenjualan~sqjenispenjualankategori~sqcarabayar~sqsumber~sqautonotransaksi~sqnotransaksi~sqtgl~sqkodepa~sqcustomer~sqcustomerkontak~sq1alamat1~sq1alamat2~sq1alamat3~sq2alamat1~sq2alamat2~sq2alamat3~sqbagianpenjualan~sqtglkirim~sqtermin~sqtgljatuhtempo~squraian~sqcatatan~sqnoref~sqtglnoref~sqtglpenutupan~sqmatauang~sqkurs~sqhargatermasukpajak~sqtotal~sqdiskonpersen~sqjmldiskon~sqtotalpajak1detail~sqtotalpajak2detail~sqbiayalainpersen~sqbiayalain~sqtotaltransaksi~sqstatuspr~sqstatusso~sqstatuspl~sqstatusdo~sqstatusdr~sqstatuspi~sqstatussi~sqstatusrnr~sqstatussr~sqstatus~sqstatussebelumnya~sqjmlrevisi~sqcetakanke~sqinputuser~sqinputtgl~sqmodifikasiuser~sqmodifikasitgl~sqisclose~sqcustomtext1~sqcustomtext2~sqcustomtext3~sqcustomtext4~sqcustomtext5~sqcustomint1~sqcustomint2~sqcustomint3~sqcustomdbl1~sqcustomdbl2~sqcustomdbl3~sqcustomdate1~sqcustomdate2~sqcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idsqdetail(0) As Integer, idsq(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, diskon(13) As String, jmldiskon(14) As Double, 
        'pajak1(15) As String, jmlpajak1(16) As Double, pajak2(17) As String, jmlpajak2(18) As Double, cabang(19) As String, 
        'lokasi(20) As String, gudang(21) As String, costcenter(22) As String, divisi(23) As String, subdivisi(24) As String, 
        'proyek(25) As String, catatan(26) As String, urutan(27) As Integer, jmlpr(28) As Double, statuspr(29) As Integer, 
        'jmlso(30) As Double, statusso(31) As Integer, jmlpl(32) As Double, statuspl(33) As Integer, jmldo(34) As Double, 
        'statusdo(35) As Integer, jmldr(36) As Double, statusdr(37) As Integer, jmlpi(38) As Double, statuspi(39) As Integer, 
        'jmlsi(40) As Double, statussi(41) As Integer, jmlrnr(42) As Double, statusrnr(43) As Integer, jmlsr(44) As Double, 
        'statussr(45) As Integer, isclose(46) As Integer, customtext1(47) As String, customtext2(48) As String, customtext3(49) As String, 
        'customdbl1(50) As Double, customdbl2(51) As Double, customdbl3(52) As Double, customdate1(53) As Date, customdate2(54) As Date, 
        'customdate3(55) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idsqdetail, idsq, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, 
        'jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, 
        'gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'jmlpr, statuspr, jmlso, statusso, jmlpl, statuspl, jmldo, 
        'statusdo, jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, 
        'jmlrnr, statusrnr, jmlsr, statussr, isclose, customtext1, customtext2, 
        'customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3


        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idsqdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idsq", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "jmlpr", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuspr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlso", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusso", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlpl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuspl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmldo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusdo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmldr", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlpi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuspi", AsEnumTypeData.AsInt64)
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

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 56) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idsqdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idsqdetail required numeric." : GoTo selesai
            End If
            'idsq(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idsq required numeric." : GoTo selesai
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
            'jmlpr(28) As Double
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - jmlpr required numeric." : GoTo selesai
            End If
            'statuspr(29) As Integer
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - statuspr required numeric." : GoTo selesai
            End If
            'jmlso(30) As Double
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - jmlso required numeric." : GoTo selesai
            End If
            'statusso(31) As Integer
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - statusso required numeric." : GoTo selesai
            End If
            'jmlpl(32) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - jmlpl required numeric." : GoTo selesai
            End If
            'statuspl(33) As Integer
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - statuspl required numeric." : GoTo selesai
            End If
            'jmldo(34) As Double
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - jmldo required numeric." : GoTo selesai
            End If
            'statusdo(35) As Integer
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - statusdo required numeric." : GoTo selesai
            End If
            'jmldr(36) As Double
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - jmldr required numeric." : GoTo selesai
            End If
            'statusdr(37) As Integer
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - statusdr required numeric." : GoTo selesai
            End If
            'jmlpi(38) As Double
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - jmlpi required numeric." : GoTo selesai
            End If
            'statuspi(39) As Integer
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - statuspi required numeric." : GoTo selesai
            End If
            'jmlsi(40) As Double
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - jmlsi required numeric." : GoTo selesai
            End If
            'statussi(41) As Integer
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - statussi required numeric." : GoTo selesai
            End If
            'jmlrnr(42) As Double
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - jmlrnr required numeric." : GoTo selesai
            End If
            'statusrnr(43) As Integer
            If (IsNumeric(dataRowDetail(43)) = False) Then
                result(2) = "Row : " & i & " - statusrnr required numeric." : GoTo selesai
            End If
            'jmlsr(44) As Double
            If (IsNumeric(dataRowDetail(44)) = False) Then
                result(2) = "Row : " & i & " - jmlsr required numeric." : GoTo selesai
            End If
            'statussr(45) As Integer
            If (IsNumeric(dataRowDetail(45)) = False) Then
                result(2) = "Row : " & i & " - statussr required numeric." : GoTo selesai
            End If
            'isclose(46) As Integer
            If (IsNumeric(dataRowDetail(46)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(50) As Double
            If (IsNumeric(dataRowDetail(50)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(51) As Double
            If (IsNumeric(dataRowDetail(51)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(52) As Double
            If (IsNumeric(dataRowDetail(52)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(53) As Date
            If (IsDate(dataRowDetail(53)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(54) As Date
            If (IsDate(dataRowDetail(54)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(55) As Date
            If (IsDate(dataRowDetail(55)) = False) Then
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
            'If dataRowDetail(5) <= 0 Then
            '    result(2) = "Row : " & i & " - jml can't be less than or equal to zero" : GoTo selesai
            'End If

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
            'If dataRowDetail(8) <= 0 Then
            '    result(2) = "Row : " & i & " - jmlbarang can't be less than or equal to zero" : GoTo selesai
            'End If

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

            'diskon(13) As Double
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

            'jmlpr(28) As Double
            If Len(dataRowDetail(28)) = 0 Then
                result(2) = "Row : " & i & " - jmlpr can't be empty" : GoTo selesai
            End If

            'jmlso(30) As Double
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Row : " & i & " - jmlso can't be empty" : GoTo selesai
            End If

            'jmlpl(32) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - jmlpl can't be empty" : GoTo selesai
            End If

            'jmldo(34) As Double
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Row : " & i & " - jmldo can't be empty" : GoTo selesai
            End If

            'jmldr(36) As Double
            If Len(dataRowDetail(36)) = 0 Then
                result(2) = "Row : " & i & " - jmldr can't be empty" : GoTo selesai
            End If

            'jmlpi(38) As Double
            If Len(dataRowDetail(38)) = 0 Then
                result(2) = "Row : " & i & " - jmlpi can't be empty" : GoTo selesai
            End If

            'jmlsi(40) As Double
            If Len(dataRowDetail(40)) = 0 Then
                result(2) = "Row : " & i & " - jmlsi can't be empty" : GoTo selesai
            End If

            'jmlrnr(42) As Double
            If Len(dataRowDetail(42)) = 0 Then
                result(2) = "Row : " & i & " - jmlrnr can't be empty" : GoTo selesai
            End If

            'jmlsr(44) As Double
            If Len(dataRowDetail(44)) = 0 Then
                result(2) = "Row : " & i & " - jmlsr can't be empty" : GoTo selesai
            End If

            'customdbl1(50) As Double
            If Len(dataRowDetail(50)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(51) As Double
            If Len(dataRowDetail(51)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(52) As Double
            If Len(dataRowDetail(52)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(53) As Date
            If Len(dataRowDetail(53)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(54) As Date
            If Len(dataRowDetail(54)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(55) As Date
            If Len(dataRowDetail(55)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idsqdetail~idsq~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~jmlpr~statuspr~jmlso~statusso~jmlpl~statuspl~jmldo~statusdo~jmldr~statusdr~jmlpi~statuspi~jmlsi~statussi~jmlrnr~statusrnr~jmlsr~statussr~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55)) = False Then
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

                ''CEK PERIODE AKUNTANSI ==================================
                'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("sqtgl")), AsFormatTanggal(drutama("sqtgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("sqtermin").ToString, AsFormatTanggal(drutama("sqtgl")), "sqtgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("sqtgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TOTAL = subtotal
                drutama("sqtotal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("sqtotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("sqtotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                If Integer.Parse(drutama("sqhargatermasukpajak")) = 0 Then
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                    drutama("sqtotaltransaksi") = Double.Parse(drutama("sqtotal")) - Double.Parse(drutama("sqjmldiskon")) + Double.Parse(drutama("sqtotalpajak1detail")) + Double.Parse(drutama("sqtotalpajak2detail")) + Double.Parse(drutama("sqbiayalain"))

                Else
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                    drutama("sqtotaltransaksi") = Double.Parse(drutama("sqtotal")) - Double.Parse(drutama("sqjmldiskon")) + Double.Parse(drutama("sqbiayalain"))

                End If
                'END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("sqid")
                    notransaksi = drutama("sqnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(sqid), sqnotransaksi FROM M5_sq WHERE sqid='" & result(4) & "' AND sqstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(sqid) FROM m5_sq WHERE sqnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_sq_history
                        Dim rsSimpanHistory As String = SimpanHistory.M5_Sq_HistorySimpan("" & paramSplit(0) & "★M5_Sq_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("sqsumber")) & "▼" & FixQuotes(drutama("sqid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Sq set sqcabang  = '" & FixQuotes(drutama("sqcabang")) & "', sqlokasi  = '" & FixQuotes(drutama("sqlokasi")) & "', sqgudang  = '" & FixQuotes(drutama("sqgudang")) & "', sqasalbarang  = '" & FixQuotes(drutama("sqasalbarang")) & "', sqasalbarangkategori  = " & drutama("sqasalbarangkategori") & ", sqjenispenjualan  = '" & FixQuotes(drutama("sqjenispenjualan")) & "', sqjenispenjualankategori  = " & drutama("sqjenispenjualankategori") & ", sqcarabayar  = " & drutama("sqcarabayar") & ", sqsumber  = '" & FixQuotes(drutama("sqsumber")) & "', sqautonotransaksi  = " & drutama("sqautonotransaksi") & ", sqnotransaksi  = '" & notransaksi & "', sqtgl  = '" & FixQuotes(AsFormatTanggal(drutama("sqtgl"))) & "', sqkodepa  = " & drutama("sqkodepa") & ", sqcustomer  = " & drutama("sqcustomer") & ", sqcustomerkontak  = '" & FixQuotes(drutama("sqcustomerkontak")) & "', sq1alamat1  = '" & FixQuotes(drutama("sq1alamat1")) & "', sq1alamat2  = '" & FixQuotes(drutama("sq1alamat2")) & "', sq1alamat3  = '" & FixQuotes(drutama("sq1alamat3")) & "', sq2alamat1  = '" & FixQuotes(drutama("sq2alamat1")) & "', sq2alamat2  = '" & FixQuotes(drutama("sq2alamat2")) & "', sq2alamat3  = '" & FixQuotes(drutama("sq2alamat3")) & "', sqbagianpenjualan  = " & drutama("sqbagianpenjualan") & ", sqtglkirim  = '" & FixQuotes(AsFormatTanggal(drutama("sqtglkirim"))) & "', sqtermin  = '" & FixQuotes(drutama("sqtermin")) & "', sqtgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("sqtgljatuhtempo"))) & "', squraian  = '" & FixQuotes(drutama("squraian")) & "', sqcatatan  = '" & FixQuotes(drutama("sqcatatan")) & "', sqnoref  = '" & FixQuotes(drutama("sqnoref")) & "', sqtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("sqtglnoref"))) & "', sqtglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("sqtglpenutupan"))) & "', sqmatauang  = '" & FixQuotes(drutama("sqmatauang")) & "', sqkurs  = '" & FixDouble(drutama("sqkurs")) & "', sqhargatermasukpajak  = " & drutama("sqhargatermasukpajak") & ", sqtotal  = '" & FixDouble(drutama("sqtotal")) & "', sqdiskonpersen  = '" & FixDouble(drutama("sqdiskonpersen")) & "', sqjmldiskon  = '" & FixDouble(drutama("sqjmldiskon")) & "', sqtotalpajak1detail  = '" & FixDouble(drutama("sqtotalpajak1detail")) & "', sqtotalpajak2detail  = '" & FixDouble(drutama("sqtotalpajak2detail")) & "', sqbiayalainpersen  = '" & FixDouble(drutama("sqbiayalainpersen")) & "', sqbiayalain  = '" & FixDouble(drutama("sqbiayalain")) & "', sqtotaltransaksi  = '" & FixDouble(drutama("sqtotaltransaksi")) & "', sqstatuspr  = " & drutama("sqstatuspr") & ", sqstatusso  = " & drutama("sqstatusso") & ", sqstatuspl  = " & drutama("sqstatuspl") & ", sqstatusdo  = " & drutama("sqstatusdo") & ", sqstatusdr  = " & drutama("sqstatusdr") & ", sqstatuspi  = " & drutama("sqstatuspi") & ", sqstatussi  = " & drutama("sqstatussi") & ", sqstatusrnr  = " & drutama("sqstatusrnr") & ", sqstatussr  = " & drutama("sqstatussr") & ", sqstatus  = " & drutama("sqstatus") & ", sqstatussebelumnya  = " & drutama("sqstatussebelumnya") & ", sqjmlrevisi  = sqjmlrevisi+1, sqcetakanke  = " & drutama("sqcetakanke") & ", sqmodifikasiuser  = " & drutama("sqmodifikasiuser") & ", sqmodifikasitgl  = NOW(), sqcustomtext1  = '" & FixQuotes(drutama("sqcustomtext1")) & "', sqcustomtext2  = '" & FixQuotes(drutama("sqcustomtext2")) & "', sqcustomtext3  = '" & FixQuotes(drutama("sqcustomtext3")) & "', sqcustomtext4  = '" & FixQuotes(drutama("sqcustomtext4")) & "', sqcustomtext5  = '" & FixQuotes(drutama("sqcustomtext5")) & "', sqcustomint1  = " & drutama("sqcustomint1") & ", sqcustomint2  = " & drutama("sqcustomint2") & ", sqcustomint3  = " & drutama("sqcustomint3") & ", sqcustomdbl1  = '" & FixDouble(drutama("sqcustomdbl1")) & "', sqcustomdbl2  = '" & FixDouble(drutama("sqcustomdbl2")) & "', sqcustomdbl3  = '" & FixDouble(drutama("sqcustomdbl3")) & "', sqcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("sqcustomdate1"))) & "', sqcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("sqcustomdate2"))) & "', sqcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("sqcustomdate3"))) & "' where sqid = '" & drutama("sqid") & "'"
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

                    If drutama("sqautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("sqcabang"), drutama("sqlokasi"), drutama("sqsumber"), drutama("sqtgl"))
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
                        notransaksi = drutama("sqnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(sqid) FROM m5_sq WHERE sqnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Sq (sqcabang, sqlokasi, sqgudang, sqasalbarang, sqasalbarangkategori, sqjenispenjualan, sqjenispenjualankategori, sqcarabayar, sqsumber, sqautonotransaksi, sqnotransaksi, sqtgl, sqkodepa, sqcustomer, sqcustomerkontak, sq1alamat1, sq1alamat2, sq1alamat3, sq2alamat1, sq2alamat2, sq2alamat3, sqbagianpenjualan, sqtglkirim, sqtermin, sqtgljatuhtempo, squraian, sqcatatan, sqnoref, sqtglnoref, sqtglpenutupan, sqmatauang, sqkurs, sqhargatermasukpajak, sqtotal, sqdiskonpersen, sqjmldiskon, sqtotalpajak1detail, sqtotalpajak2detail, sqbiayalainpersen, sqbiayalain, sqtotaltransaksi, sqstatuspr, sqstatusso, sqstatuspl, sqstatusdo, sqstatusdr, sqstatuspi, sqstatussi, sqstatusrnr, sqstatussr, sqstatus, sqstatussebelumnya, sqjmlrevisi, sqcetakanke, sqinputuser, sqinputtgl, sqmodifikasiuser, sqmodifikasitgl, sqisclose, sqcustomtext1, sqcustomtext2, sqcustomtext3, sqcustomtext4, sqcustomtext5, sqcustomint1, sqcustomint2, sqcustomint3, sqcustomdbl1, sqcustomdbl2, sqcustomdbl3, sqcustomdate1, sqcustomdate2, sqcustomdate3) values('" & FixQuotes(drutama("sqcabang")) & "', '" & FixQuotes(drutama("sqlokasi")) & "', '" & FixQuotes(drutama("sqgudang")) & "', '" & FixQuotes(drutama("sqasalbarang")) & "', " & drutama("sqasalbarangkategori") & ", '" & FixQuotes(drutama("sqjenispenjualan")) & "', " & drutama("sqjenispenjualankategori") & ", " & drutama("sqcarabayar") & ", '" & FixQuotes(drutama("sqsumber")) & "', " & drutama("sqautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("sqtgl"))) & "', " & drutama("sqkodepa") & ", " & drutama("sqcustomer") & ", '" & FixQuotes(drutama("sqcustomerkontak")) & "', '" & FixQuotes(drutama("sq1alamat1")) & "', '" & FixQuotes(drutama("sq1alamat2")) & "', '" & FixQuotes(drutama("sq1alamat3")) & "', '" & FixQuotes(drutama("sq2alamat1")) & "', '" & FixQuotes(drutama("sq2alamat2")) & "', '" & FixQuotes(drutama("sq2alamat3")) & "', " & drutama("sqbagianpenjualan") & ", '" & FixQuotes(AsFormatTanggal(drutama("sqtglkirim"))) & "', '" & FixQuotes(drutama("sqtermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sqtgljatuhtempo"))) & "', '" & FixQuotes(drutama("squraian")) & "', '" & FixQuotes(drutama("sqcatatan")) & "', '" & FixQuotes(drutama("sqnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sqtglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sqtglpenutupan"))) & "', '" & FixQuotes(drutama("sqmatauang")) & "', '" & FixDouble(drutama("sqkurs")) & "', " & drutama("sqhargatermasukpajak") & ", '" & FixDouble(drutama("sqtotal")) & "', '" & FixDouble(drutama("sqdiskonpersen")) & "', '" & FixDouble(drutama("sqjmldiskon")) & "', '" & FixDouble(drutama("sqtotalpajak1detail")) & "', '" & FixDouble(drutama("sqtotalpajak2detail")) & "', '" & FixDouble(drutama("sqbiayalainpersen")) & "', '" & FixDouble(drutama("sqbiayalain")) & "', '" & FixDouble(drutama("sqtotaltransaksi")) & "', " & drutama("sqstatuspr") & ", " & drutama("sqstatusso") & ", " & drutama("sqstatuspl") & ", " & drutama("sqstatusdo") & ", " & drutama("sqstatusdr") & ", " & drutama("sqstatuspi") & ", " & drutama("sqstatussi") & ", " & drutama("sqstatusrnr") & ", " & drutama("sqstatussr") & ", " & drutama("sqstatus") & ", " & drutama("sqstatussebelumnya") & ", " & drutama("sqjmlrevisi") & ", " & drutama("sqcetakanke") & ", " & drutama("sqinputuser") & ", NOW(), " & drutama("sqmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("sqisclose") & ", '" & FixQuotes(drutama("sqcustomtext1")) & "', '" & FixQuotes(drutama("sqcustomtext2")) & "', '" & FixQuotes(drutama("sqcustomtext3")) & "', '" & FixQuotes(drutama("sqcustomtext4")) & "', '" & FixQuotes(drutama("sqcustomtext5")) & "', " & drutama("sqcustomint1") & ", " & drutama("sqcustomint2") & ", " & drutama("sqcustomint3") & ", '" & FixDouble(drutama("sqcustomdbl1")) & "', '" & FixDouble(drutama("sqcustomdbl2")) & "', '" & FixDouble(drutama("sqcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sqcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sqcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sqcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select sqid from M5_sq where sqnotransaksi='" & notransaksi & "' AND sqinputuser= '" & userid & "' order by sqmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Sq_Detail where idsq = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idsqdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", '" & FixDouble(dr1("jmlpr")) & "', " & dr1("statuspr") & ", '" & FixDouble(dr1("jmlso")) & "', " & dr1("statusso") & ", '" & FixDouble(dr1("jmlpl")) & "', " & dr1("statuspl") & ", '" & FixDouble(dr1("jmldo")) & "', " & dr1("statusdo") & ", '" & FixDouble(dr1("jmldr")) & "', " & dr1("statusdr") & ", '" & FixDouble(dr1("jmlpi")) & "', " & dr1("statuspi") & ", '" & FixDouble(dr1("jmlsi")) & "', " & dr1("statussi") & ", '" & FixDouble(dr1("jmlrnr")) & "', " & dr1("statusrnr") & ", '" & FixDouble(dr1("jmlsr")) & "', " & dr1("statussr") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M5_Sq_Detail(idsqdetail, idsq, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlpr, statuspr, jmlso, statusso, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                Dim sumber As String = "SQ", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M5_SqUpdateStatusOld(ByVal param As String) As String
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
            Dim sumber As String = "Sq", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Sqtgl, Sqnotransaksi, Sqstatus FROM M5_Sq WHERE Sqid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Sqstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m5_sq_history
            Dim rsSimpanHistory As String = SimpanHistory.M5_Sq_HistorySimpan("" & paramSplit(0) & "★M5_Sq_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m5_sq_terkait("sqid = '" & idtransaksi & "'")
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================
            End If

            'update status utama
            sql = "UPDATE M5_Sq SET Sqstatus = " & nilaiStatus & ", Sqmodifikasiuser='" & userid & "', Sqmodifikasitgl = NOW(), Sqposting = 0, Sqpostingtgl = '1971-01-01 00:00:00', Sqjmlrevisi = Sqjmlrevisi + 1 WHERE Sqid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_SqSearch(PostWsSearch(paramSplit(0), "M5_SqSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M5_SqDeleteOld(ByVal param As String) As String

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
            Dim sumber As String = "Sq", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Sqid, Sqnotransaksi FROM M5_Sq WHERE Sqid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT sqcabang, sqlokasi, sqsumber, sqautonotransaksi, sqnotransaksi, sqtgl"
            sql &= " FROM M5_sq"
            sql &= " WHERE sqid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("sqcabang")
                lokasi = dtNomorNext.Rows(0)("sqlokasi")
                sumber = dtNomorNext.Rows(0)("sqsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("sqautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("sqnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("sqtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M5_Sq_Detail WHERE idsq = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M5_Sq WHERE sqid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_SqSearch(PostWsSearch(paramSplit(0), "M5_SqSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M5_SQ_OUT_BAHAN(ByVal param As String) As String
        'M5_SQ_OUT_BAHAN --------------------------------------------------------
        'sqo.*

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
        sql = "SELECT sqo.idbarang, sqo.namabarang, i.btipe tipebarang, sqo.jml, sqo.satuan, u.unilai nilaisatuan, (sqo.jml * u.unilai) jmlbarang, i.bsatuan satuanbarang, sqd.matauang, sqd.kurs, sqo.hargajual harga, sqo.hargabeli hpp, 0 idhppkhususmasuk, 0 idhppfifomasuk, i.brekpersediaan rekpersediaan, sqd.cabang, sqd.lokasi, sqd.gudang gudangasal, sqd.gudang gudangproduksi, sqd.gudang gudangtujuan, sqd.costcenter, sqd.divisi, sqd.subdivisi, sqd.proyek, sqd.catatan, sqo.urutan, 0 idbom, 0 idbomout, sqo.customtext1, sqo.customtext2, sqo.customtext3, sqo.customdbl1, sqo.customdbl2, sqo.customdbl3, sqo.customdate1, sqo.customdate2, sqo.customdate3, sqo.kodebarang, i.bhpp, i.bjenis, i.bserial, i.bbatch, '' costcenternama, '' divisinama, '' subdivisinama, '' proyeknama, sq.sqnotransaksi notransaksi, i.bjmllapangan, i.bsatuanlapangan, 0 prosentase, 0 stokakhir, sqo.hargabeli, 0 stokreal FROM m5_sq_out_bahan sqo JOIN m1_item i ON i.bid = sqo.idbarang JOIN m1_unit u ON u.ukode = i.bsatuan JOIN m5_sq_detail sqd ON sqd.idsq = sqo.idsq AND sqd.idbarang = sqo.idbarangdetail JOIN m5_so_detail sod ON sod.idsqdetail = sqd.idsqdetail LEFT JOIN m5_sq sq ON sq.sqid = sqo.idsq"

        dt = AmbilData("aplikasi1-m5_sq_out_bahan", Filter, , True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
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
                    FxDB(dr("hpp"), 0), sptField,
                    FxDB(dr("idhppkhususmasuk"), 0), sptField,
                    FxDB(dr("idhppfifomasuk"), 0), sptField,
                    FxDB(dr("rekpersediaan"), ""), sptField,
                    FxDB(dr("cabang"), ""), sptField,
                    FxDB(dr("lokasi"), ""), sptField,
                    FxDB(dr("gudangasal"), ""), sptField,
                    FxDB(dr("gudangproduksi"), ""), sptField,
                    FxDB(dr("gudangtujuan"), ""), sptField,
                    FxDB(dr("costcenter"), ""), sptField,
                    FxDB(dr("divisi"), ""), sptField,
                    FxDB(dr("subdivisi"), ""), sptField,
                    FxDB(dr("proyek"), ""), sptField,
                    FxDB(dr("catatan"), ""), sptField,
                    FxDB(dr("urutan"), 0), sptField,
                    FxDB(dr("idbom"), 0), sptField,
                    FxDB(dr("idbomout"), 0), sptField,
                    FxDB(dr("customtext1"), ""), sptField,
                    FxDB(dr("customtext2"), ""), sptField,
                    FxDB(dr("customtext3"), ""), sptField,
                    FxDB(dr("customdbl1"), ""), sptField,
                    FxDB(dr("customdbl2"), ""), sptField,
                    FxDB(dr("customdbl3"), ""), sptField,
                    FxDB(dr("customdate1"), ""), sptField,
                    FxDB(dr("customdate2"), ""), sptField,
                    FxDB(dr("customdate3"), ""), sptField,
                    FxDB(dr("kodebarang"), ""), sptField,
                    FxDB(dr("bhpp"), 0), sptField,
                    FxDB(dr("bjenis"), ""), sptField,
                    FxDB(dr("bserial"), ""), sptField,
                    FxDB(dr("bbatch"), ""), sptField,
                    FxDB(dr("costcenternama"), ""), sptField,
                    FxDB(dr("divisinama"), ""), sptField,
                    FxDB(dr("subdivisinama"), ""), sptField,
                    FxDB(dr("proyeknama"), ""), sptField,
                    FxDB(dr("notransaksi"), ""), sptField,
                    FxDB(dr("bjmllapangan"), 0), sptField,
                    FxDB(dr("bsatuanlapangan"), ""), sptField,
                    FxDB(dr("prosentase"), 0), sptField,
                    FxDB(dr("stokakhir"), 0), sptField,
                    FxDB(dr("hargabeli"), 0), sptField,
                    FxDB(dr("stokreal"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbom, idbomout, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, subdivisinama, proyeknama, notransaksi, bjmllapangan, bsatuanlapangan, prosentase, stokakhir, hargabeli, stokreal"))

        Return wsResult
    End Function

End Class