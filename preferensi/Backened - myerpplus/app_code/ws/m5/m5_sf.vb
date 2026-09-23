Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_sf
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_SfSimpan(ByVal param As String) As String
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
        'sfid(0) As Integer, sfcabang(1) As String, sflokasi(2) As String, sfgudang(3) As String, sfasalbarang(4) As String, 
        'sfasalbarangkategori(5) As Integer, sfjenispenjualan(6) As String, sfjenispenjualankategori(7) As Integer, sfcarabayar(8) As Integer, sfsumber(9) As String, 
        'sfautonotransaksi(10) As Integer, sfnotransaksi(11) As String, sftgl(12) As Date, sfkodepa(13) As Integer, sfcustomer(14) As Integer, 
        'sfcustomerkontak(15) As String, sf1alamat1(16) As String, sf1alamat2(17) As String, sf1alamat3(18) As String, sf2alamat1(19) As String, 
        'sf2alamat2(20) As String, sf2alamat3(21) As String, sfbagianpenjualan(22) As Integer, sftglkirim(23) As Date, sftermin(24) As String, 
        'sftgljatuhtempo(25) As Date, sfuraian(26) As String, sfcatatan(27) As String, sfnoref(28) As String, sftglnoref(29) As Date, 
        'sftglpenutupan(30) As Date, sfmatauang(31) As String, sfkurs(32) As Double, sfhargatermasukpajak(33) As Integer, sftotal(34) As Double, 
        'sfdiskonpersen(35) As String, sfjmldiskon(36) As Double, sftotalpajak1detail(37) As Double, sftotalpajak2detail(38) As Double, sfbiayalainpersen(39) As Double, 
        'sfbiayalain(40) As Double, sftotaltransaksi(41) As Double, sfstatuspr(42) As Integer, sfstatusso(43) As Integer, sfstatuspl(44) As Integer, 
        'sfstatusdo(45) As Integer, sfstatusdr(46) As Integer, sfstatuspi(47) As Integer, sfstatussi(48) As Integer, sfstatusrnr(49) As Integer, 
        'sfstatussr(50) As Integer, sfstatus(51) As Integer, sfstatussebelumnya(52) As Integer, sfjmlrevisi(53) As Integer, sfcetakanke(54) As Integer, 
        'sfinputuser(55) As Integer, sfinputtgl(56) As DateTime, sfmodifikasiuser(57) As Integer, sfmodifikasitgl(58) As DateTime, sfisclose(59) As Integer, 
        'sfcustomtext1(60) As String, sfcustomtext2(61) As String, sfcustomtext3(62) As String, sfcustomtext4(63) As String, sfcustomtext5(64) As String, 
        'sfcustomint1(65) As Integer, sfcustomint2(66) As Integer, sfcustomint3(67) As Integer, sfcustomdbl1(68) As Double, sfcustomdbl2(69) As Double, 
        'sfcustomdbl3(70) As Double, sfcustomdate1(71) As Date, sfcustomdate2(72) As Date, sfcustomdate3(73) As Date, sfidpr(74) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'sfid, sfcabang, sflokasi, sfgudang, sfasalbarang, sfasalbarangkategori, sfjenispenjualan, 
        'sfjenispenjualankategori, sfcarabayar, sfsumber, sfautonotransaksi, sfnotransaksi, sftgl, sfkodepa, 
        'sfcustomer, sfcustomerkontak, sf1alamat1, sf1alamat2, sf1alamat3, sf2alamat1, sf2alamat2, 
        'sf2alamat3, sfbagianpenjualan, sftglkirim, sftermin, sftgljatuhtempo, sfuraian, sfcatatan, 
        'sfnoref, sftglnoref, sftglpenutupan, sfmatauang, sfkurs, sfhargatermasukpajak, sftotal, 
        'sfdiskonpersen, sfjmldiskon, sftotalpajak1detail, sftotalpajak2detail, sfbiayalainpersen, sfbiayalain, sftotaltransaksi, 
        'sfstatuspr, sfstatusso, sfstatuspl, sfstatusdo, sfstatusdr, sfstatuspi, sfstatussi, 
        'sfstatusrnr, sfstatussr, sfstatus, sfstatussebelumnya, sfjmlrevisi, sfcetakanke, sfinputuser, 
        'sfinputtgl, sfmodifikasiuser, sfmodifikasitgl, sfisclose, sfcustomtext1, sfcustomtext2, sfcustomtext3, 
        'sfcustomtext4, sfcustomtext5, sfcustomint1, sfcustomint2, sfcustomint3, sfcustomdbl1, sfcustomdbl2, 
        'sfcustomdbl3, sfcustomdate1, sfcustomdate2, sfcustomdate3, sfidpr

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 75) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'sfid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "sfid required numeric." : GoTo selesai
        End If
        'sfasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "sfasalbarangkategori required numeric." : GoTo selesai
        End If
        'sfjenispenjualankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "sfjenispenjualankategori required numeric." : GoTo selesai
        End If
        'sfcarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "sfcarabayar required numeric." : GoTo selesai
        End If
        'sfautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "sfautonotransaksi required numeric." : GoTo selesai
        End If
        'sftgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "sftgl required date." : GoTo selesai
        End If
        'sfkodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "sfkodepa required numeric." : GoTo selesai
        End If
        'sfcustomer(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "sfcustomer required numeric." : GoTo selesai
        End If
        If (dataUtama(14) < 1) Then
            result(2) = "sfcustomer can't be empty." : GoTo selesai
        End If
        'sfbagianpenjualan(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "sfbagianpenjualan required numeric." : GoTo selesai
        End If
        If (dataUtama(22) < 1) Then
            result(2) = "sfbagianpenjualan can't be empty." : GoTo selesai
        End If
        'sftglkirim(23) As Date
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "sftglkirim required date." : GoTo selesai
        End If
        'sftgljatuhtempo(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "sftgljatuhtempo required date." : GoTo selesai
        End If
        'sftglnoref(29) As Date
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "sftglnoref required date." : GoTo selesai
        End If
        'sftglpenutupan(30) As Date
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "sftglpenutupan required date." : GoTo selesai
        End If
        'sfkurs(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "sfkurs required numeric." : GoTo selesai
        End If
        'sfhargatermasukpajak(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "sfhargatermasukpajak required numeric." : GoTo selesai
        End If
        'sftotal(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "sftotal required numeric." : GoTo selesai
        End If
        'sfjmldiskon(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "sfjmldiskon required numeric." : GoTo selesai
        End If
        'sftotalpajak1detail(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "sftotalpajak1detail required numeric." : GoTo selesai
        End If
        'sftotalpajak2detail(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "sftotalpajak2detail required numeric." : GoTo selesai
        End If
        ''sfbiayalainpersen(39) As Double
        'If (IsNumeric(dataUtama(39)) = False) Then
        '    result(2) = "sfbiayalainpersen required numeric." : GoTo selesai
        'End If
        'sfbiayalain(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "sfbiayalain required numeric." : GoTo selesai
        End If
        'sftotaltransaksi(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "sftotaltransaksi required numeric." : GoTo selesai
        End If
        'sfstatuspr(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "sfstatuspr required numeric." : GoTo selesai
        End If
        'sfstatusso(43) As Integer
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "sfstatusso required numeric." : GoTo selesai
        End If
        'sfstatuspl(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "sfstatuspl required numeric." : GoTo selesai
        End If
        'sfstatusdo(45) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "sfstatusdo required numeric." : GoTo selesai
        End If
        'sfstatusdr(46) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "sfstatusdr required numeric." : GoTo selesai
        End If
        'sfstatuspi(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "sfstatuspi required numeric." : GoTo selesai
        End If
        'sfstatussi(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "sfstatussi required numeric." : GoTo selesai
        End If
        'sfstatusrnr(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "sfstatusrnr required numeric." : GoTo selesai
        End If
        'sfstatussr(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "sfstatussr required numeric." : GoTo selesai
        End If
        'sfstatus(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "sfstatus required numeric." : GoTo selesai
        End If
        'sfstatussebelumnya(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "sfstatussebelumnya required numeric." : GoTo selesai
        End If
        'sfjmlrevisi(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "sfjmlrevisi required numeric." : GoTo selesai
        End If
        'sfcetakanke(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "sfcetakanke required numeric." : GoTo selesai
        End If
        'sfinputuser(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "sfinputuser required numeric." : GoTo selesai
        End If
        'sfinputtgl(56) As DateTime
        If (IsDate(dataUtama(56)) = False) Then
            result(2) = "sfinputtgl required date." : GoTo selesai
        End If
        'sfmodifikasiuser(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "sfmodifikasiuser required numeric." : GoTo selesai
        End If
        'sfmodifikasitgl(58) As DateTime
        If (IsDate(dataUtama(58)) = False) Then
            result(2) = "sfmodifikasitgl required date." : GoTo selesai
        End If
        'sfisclose(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "sfisclose required numeric." : GoTo selesai
        End If
        'sfcustomint1(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "sfcustomint1 required numeric." : GoTo selesai
        End If
        'sfcustomint2(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "sfcustomint2 required numeric." : GoTo selesai
        End If
        'sfcustomint3(67) As Integer
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "sfcustomint3 required numeric." : GoTo selesai
        End If
        'sfcustomdbl1(68) As Double
        If (IsNumeric(dataUtama(68)) = False) Then
            result(2) = "sfcustomdbl1 required numeric." : GoTo selesai
        End If
        'sfcustomdbl2(69) As Double
        If (IsNumeric(dataUtama(69)) = False) Then
            result(2) = "sfcustomdbl2 required numeric." : GoTo selesai
        End If
        'sfcustomdbl3(70) As Double
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "sfcustomdbl3 required numeric." : GoTo selesai
        End If
        'sfcustomdate1(71) As Date
        If (IsDate(dataUtama(71)) = False) Then
            result(2) = "sfcustomdate1 required date." : GoTo selesai
        End If
        'sfcustomdate2(72) As Date
        If (IsDate(dataUtama(72)) = False) Then
            result(2) = "sfcustomdate2 required date." : GoTo selesai
        End If
        'sfcustomdate3(73) As Date
        If (IsDate(dataUtama(73)) = False) Then
            result(2) = "sfcustomdate3 required date." : GoTo selesai
        End If
        'sfidpr(74) As Integer
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "sfidpr required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'sfcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "sfcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "sfcabang should not be more than 25 character." : GoTo selesai
        End If

        'sflokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "sflokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "sflokasi should not be more than 25 character." : GoTo selesai
        End If

        'sfgudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "sfgudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "sfgudang should not be more than 25 character." : GoTo selesai
        End If

        'sfsumber(9) As String
		dataUtama(9) = "SF"
        If Len(dataUtama(9)) = 0 Then
            result(2) = "sfsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "sfsumber should not be more than 10 character." : GoTo selesai
        End If

        'sfnotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "sfnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "sfnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'sftgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "sftgl can't be empty" : GoTo selesai
        End If

        'sftglkirim(23) As Date
        If Len(dataUtama(23)) = 0 Then
            result(2) = "sftglkirim can't be empty" : GoTo selesai
        End If

        'sftgljatuhtempo(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "sftgljatuhtempo can't be empty" : GoTo selesai
        End If

        'sftglnoref(29) As Date
        If Len(dataUtama(29)) = 0 Then
            result(2) = "sftglnoref can't be empty" : GoTo selesai
        End If

        'sftglpenutupan(30) As Date
        If Len(dataUtama(30)) = 0 Then
            result(2) = "sftglpenutupan can't be empty" : GoTo selesai
        End If

        'sfmatauang(31) As String
        If Len(dataUtama(31)) = 0 Then
            result(2) = "sfmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(31)) > 25 Then
            result(2) = "sfmatauang should not be more than 25 character." : GoTo selesai
        End If

        'sfkurs(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "sfkurs can't be empty" : GoTo selesai
        End If

        'sftotal(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "sftotal can't be empty" : GoTo selesai
        End If

        'sfdiskonpersen(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "sfdiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(35)) > 25 Then
            result(2) = "sfdiskonpersen should not be more than 25 character" : GoTo selesai
        End If

        'sfjmldiskon(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "sfjmldiskon can't be empty" : GoTo selesai
        End If

        'sftotalpajak1detail(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "sftotalpajak1detail can't be empty" : GoTo selesai
        End If

        'sftotalpajak2detail(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "sftotalpajak2detail can't be empty" : GoTo selesai
        End If

        'sfbiayalainpersen(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "sfbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(39)) > 25 Then
            result(2) = "sfbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'sfbiayalain(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "sfbiayalain can't be empty" : GoTo selesai
        End If

        'sftotaltransaksi(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "sftotaltransaksi can't be empty" : GoTo selesai
        End If

        'sfinputtgl(56) As DateTime
        If Len(dataUtama(56)) = 0 Then
            result(2) = "sfinputtgl can't be empty" : GoTo selesai
        End If

        'sfmodifikasitgl(58) As DateTime
        If Len(dataUtama(58)) = 0 Then
            result(2) = "sfmodifikasitgl can't be empty" : GoTo selesai
        End If

        'sfcustomdbl1(68) As Double
        If Len(dataUtama(68)) = 0 Then
            result(2) = "sfcustomdbl1 can't be empty" : GoTo selesai
        End If

        'sfcustomdbl2(69) As Double
        If Len(dataUtama(69)) = 0 Then
            result(2) = "sfcustomdbl2 can't be empty" : GoTo selesai
        End If

        'sfcustomdbl3(70) As Double
        If Len(dataUtama(70)) = 0 Then
            result(2) = "sfcustomdbl3 can't be empty" : GoTo selesai
        End If

        'sfcustomdate1(71) As Date
        If Len(dataUtama(71)) = 0 Then
            result(2) = "sfcustomdate1 can't be empty" : GoTo selesai
        End If

        'sfcustomdate2(72) As Date
        If Len(dataUtama(72)) = 0 Then
            result(2) = "sfcustomdate2 can't be empty" : GoTo selesai
        End If

        'sfcustomdate3(73) As Date
        If Len(dataUtama(73)) = 0 Then
            result(2) = "sfcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "sfid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sflokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sfjenispenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfjenispenjualankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sfcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sfsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sfnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sftgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sfcustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sfcustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sf1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sf1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sf1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sf2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sf2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sf2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfbagianpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sftglkirim", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sftermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sftgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sftglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sftglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfhargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sftotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfdiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfjmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sftotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sftotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sftotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfstatuspr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sfstatusso", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sfstatuspl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sfstatusdo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sfstatusdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sfstatuspi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sfstatussi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sfstatusrnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sfstatussr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sfstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sfstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sfjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sfcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sfinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sfinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sfmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sfcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sfcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sfcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sfcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sfidpr", AsEnumTypeData.AsInt64)
        If AsDataTableTambahData(dtutama, "sfid~sfcabang~sflokasi~sfgudang~sfasalbarang~sfasalbarangkategori~sfjenispenjualan~sfjenispenjualankategori~sfcarabayar~sfsumber~sfautonotransaksi~sfnotransaksi~sftgl~sfkodepa~sfcustomer~sfcustomerkontak~sf1alamat1~sf1alamat2~sf1alamat3~sf2alamat1~sf2alamat2~sf2alamat3~sfbagianpenjualan~sftglkirim~sftermin~sftgljatuhtempo~sfuraian~sfcatatan~sfnoref~sftglnoref~sftglpenutupan~sfmatauang~sfkurs~sfhargatermasukpajak~sftotal~sfdiskonpersen~sfjmldiskon~sftotalpajak1detail~sftotalpajak2detail~sfbiayalainpersen~sfbiayalain~sftotaltransaksi~sfstatuspr~sfstatusso~sfstatuspl~sfstatusdo~sfstatusdr~sfstatuspi~sfstatussi~sfstatusrnr~sfstatussr~sfstatus~sfstatussebelumnya~sfjmlrevisi~sfcetakanke~sfinputuser~sfinputtgl~sfmodifikasiuser~sfmodifikasitgl~sfisclose~sfcustomtext1~sfcustomtext2~sfcustomtext3~sfcustomtext4~sfcustomtext5~sfcustomint1~sfcustomint2~sfcustomint3~sfcustomdbl1~sfcustomdbl2~sfcustomdbl3~sfcustomdate1~sfcustomdate2~sfcustomdate3~sfidpr", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idsfdetail(0) As Integer, idsf(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
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
        'idsfdetail, idsf, idbarang, namabarang, tipebarang, jml, satuan, 
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
        AsDataTableTambahField(dtdetail, "idsfdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idsf", AsEnumTypeData.AsInt64)
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
            'idsfdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idsfdetail required numeric." : GoTo selesai
            End If
            'idsf(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idsf required numeric." : GoTo selesai
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
                result(2) = "Row : " & i & " - idprdetail required numeric." : GoTo selesai
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
            If dataRowDetail(5) <= 0 Then
                result(2) = "Row : " & i & " - jml can't be less than or equal to zero" : GoTo selesai
            End If

            ''satuan(6) As String
            'If Len(dataRowDetail(6)) = 0 Then
            '    result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            'End If
            'If Len(dataRowDetail(6)) > 25 Then
            '    result(2) = "Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            'End If

            ''nilaisatuan(7) As Double
            'If Len(dataRowDetail(7)) = 0 Then
            '    result(2) = "Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            'End If

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

            If AsDataTableTambahData(dtdetail, "idsfdetail~idsf~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~jmlpr~statuspr~jmlso~statusso~jmlpl~statuspl~jmldo~statusdo~jmldr~statusdr~jmlpi~statuspi~jmlsi~statussi~jmlrnr~statusrnr~jmlsr~statussr~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~idprdetail", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56)) = False Then
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
                ftOutstanding = String.Concat(ftOutstanding, " (prd.idprdetail = " & idprdetail & " AND " & Outstanding & " > (prd.jmlbarang - prd.jmlsf)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilai = String.Concat("WHEN '" & idprdetail & "' THEN ROUND(jmlsf + '" & Outstanding & "', 5) ", updNilai)

                '4. SET FILTER UPDATE OUTSTANDING ---------
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
        Dim rowUpdate As Integer = 0

        Try
            'Proses utama
            If (dtutama.Rows.Count > 0) Then
                Dim drutama As DataRow = dtutama.Rows(0)

                'CEK HAK AKSES STATUS ============================
                Dim vAkses As Integer = 0, msgAkses As String = ""
                'MODUL DAN MENU HARUS DISESUAIKAN
                Dim vModuleId As Integer = 5, vMenuId As Integer = 75
                Select Case drutama("sfstatus")
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("sftgl")), AsFormatTanggal(drutama("sftgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("sfstatus") = 2 Or drutama("sfstatus") = 1 Or drutama("sfstatus") = 8 Or drutama("sfstatus") = 9 Or drutama("sfstatus") = 10 Or drutama("sfstatus") = 11 Then
                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstanding, ftOutstanding)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("sftermin").ToString, AsFormatTanggal(drutama("sftgl")), "sftgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("sftgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TOTAL = subtotal
                drutama("sftotal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("sftotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("sftotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                If Integer.Parse(drutama("sfhargatermasukpajak")) = 0 Then
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                    drutama("sftotaltransaksi") = Double.Parse(drutama("sftotal")) - Double.Parse(drutama("sfjmldiskon")) + Double.Parse(drutama("sftotalpajak1detail")) + Double.Parse(drutama("sftotalpajak2detail")) + Double.Parse(drutama("sfbiayalain"))

                Else
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                    drutama("sftotaltransaksi") = Double.Parse(drutama("sftotal")) - Double.Parse(drutama("sfjmldiskon")) + Double.Parse(drutama("sftotalpajak2detail")) + Double.Parse(drutama("sfbiayalain"))

                End If
                'END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("sfid")
                    notransaksi = drutama("sfnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(sfid), sfnotransaksi FROM M5_sf WHERE sfid='" & result(4) & "' AND sfstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("sfautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("sfcabang"), drutama("sflokasi"), drutama("sfsumber"), drutama("sftgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(sfid) FROM m5_sf WHERE sfnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_sf_history
                        Dim rsSimpanHistory As String = SimpanHistory.M5_Sf_HistorySimpan("" & paramSplit(0) & "★M5_Sf_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("sfsumber")) & "▼" & FixQuotes(drutama("sfid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Sf set sfcabang  = '" & FixQuotes(drutama("sfcabang")) & "', sflokasi  = '" & FixQuotes(drutama("sflokasi")) & "', sfgudang  = '" & FixQuotes(drutama("sfgudang")) & "', sfasalbarang  = '" & FixQuotes(drutama("sfasalbarang")) & "', sfasalbarangkategori  = " & drutama("sfasalbarangkategori") & ", sfjenispenjualan  = '" & FixQuotes(drutama("sfjenispenjualan")) & "', sfjenispenjualankategori  = " & drutama("sfjenispenjualankategori") & ", sfcarabayar  = " & drutama("sfcarabayar") & ", sfsumber  = '" & FixQuotes(drutama("sfsumber")) & "', sfautonotransaksi  = " & drutama("sfautonotransaksi") & ", sfnotransaksi  = '" & notransaksi & "', sftgl  = '" & FixQuotes(AsFormatTanggal(drutama("sftgl"))) & "', sfkodepa  = " & drutama("sfkodepa") & ", sfcustomer  = " & drutama("sfcustomer") & ", sfcustomerkontak  = '" & FixQuotes(drutama("sfcustomerkontak")) & "', sf1alamat1  = '" & FixQuotes(drutama("sf1alamat1")) & "', sf1alamat2  = '" & FixQuotes(drutama("sf1alamat2")) & "', sf1alamat3  = '" & FixQuotes(drutama("sf1alamat3")) & "', sf2alamat1  = '" & FixQuotes(drutama("sf2alamat1")) & "', sf2alamat2  = '" & FixQuotes(drutama("sf2alamat2")) & "', sf2alamat3  = '" & FixQuotes(drutama("sf2alamat3")) & "', sfbagianpenjualan  = " & drutama("sfbagianpenjualan") & ", sftglkirim  = '" & FixQuotes(AsFormatTanggal(drutama("sftglkirim"))) & "', sftermin  = '" & FixQuotes(drutama("sftermin")) & "', sftgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("sftgljatuhtempo"))) & "', sfuraian  = '" & FixQuotes(drutama("sfuraian")) & "', sfcatatan  = '" & FixQuotes(drutama("sfcatatan")) & "', sfnoref  = '" & FixQuotes(drutama("sfnoref")) & "', sftglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("sftglnoref"))) & "', sftglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("sftglpenutupan"))) & "', sfmatauang  = '" & FixQuotes(drutama("sfmatauang")) & "', sfkurs  = '" & FixDouble(drutama("sfkurs")) & "', sfhargatermasukpajak  = " & drutama("sfhargatermasukpajak") & ", sftotal  = '" & FixDouble(drutama("sftotal")) & "', sfdiskonpersen  = '" & FixDouble(drutama("sfdiskonpersen")) & "', sfjmldiskon  = '" & FixDouble(drutama("sfjmldiskon")) & "', sftotalpajak1detail  = '" & FixDouble(drutama("sftotalpajak1detail")) & "', sftotalpajak2detail  = '" & FixDouble(drutama("sftotalpajak2detail")) & "', sfbiayalainpersen  = '" & FixDouble(drutama("sfbiayalainpersen")) & "', sfbiayalain  = '" & FixDouble(drutama("sfbiayalain")) & "', sftotaltransaksi  = '" & FixDouble(drutama("sftotaltransaksi")) & "', sfstatuspr  = " & drutama("sfstatuspr") & ", sfstatusso  = " & drutama("sfstatusso") & ", sfstatuspl  = " & drutama("sfstatuspl") & ", sfstatusdo  = " & drutama("sfstatusdo") & ", sfstatusdr  = " & drutama("sfstatusdr") & ", sfstatuspi  = " & drutama("sfstatuspi") & ", sfstatussi  = " & drutama("sfstatussi") & ", sfstatusrnr  = " & drutama("sfstatusrnr") & ", sfstatussr  = " & drutama("sfstatussr") & ", sfstatus  = " & drutama("sfstatus") & ", sfstatussebelumnya  = " & drutama("sfstatussebelumnya") & ", sfjmlrevisi  = sfjmlrevisi+1, sfcetakanke  = " & drutama("sfcetakanke") & ", sfmodifikasiuser  = " & drutama("sfmodifikasiuser") & ", sfmodifikasitgl  = NOW(), sfcustomtext1  = '" & FixQuotes(drutama("sfcustomtext1")) & "', sfcustomtext2  = '" & FixQuotes(drutama("sfcustomtext2")) & "', sfcustomtext3  = '" & FixQuotes(drutama("sfcustomtext3")) & "', sfcustomtext4  = '" & FixQuotes(drutama("sfcustomtext4")) & "', sfcustomtext5  = '" & FixQuotes(drutama("sfcustomtext5")) & "', sfcustomint1  = " & drutama("sfcustomint1") & ", sfcustomint2  = " & drutama("sfcustomint2") & ", sfcustomint3  = " & drutama("sfcustomint3") & ", sfcustomdbl1  = '" & FixDouble(drutama("sfcustomdbl1")) & "', sfcustomdbl2  = '" & FixDouble(drutama("sfcustomdbl2")) & "', sfcustomdbl3  = '" & FixDouble(drutama("sfcustomdbl3")) & "', sfcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("sfcustomdate1"))) & "', sfcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("sfcustomdate2"))) & "', sfcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("sfcustomdate3"))) & "', sfidpr  = '" & FixDouble(drutama("sfidpr")) & "' where sfid = '" & drutama("sfid") & "'"
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

                    If drutama("sfautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("sfcabang"), drutama("sflokasi"), drutama("sfsumber"), drutama("sftgl"))
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
                        notransaksi = drutama("sfnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(sfid) FROM m5_sf WHERE sfnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Sf (sfcabang, sflokasi, sfgudang, sfasalbarang, sfasalbarangkategori, sfjenispenjualan, sfjenispenjualankategori, sfcarabayar, sfsumber, sfautonotransaksi, sfnotransaksi, sftgl, sfkodepa, sfcustomer, sfcustomerkontak, sf1alamat1, sf1alamat2, sf1alamat3, sf2alamat1, sf2alamat2, sf2alamat3, sfbagianpenjualan, sftglkirim, sftermin, sftgljatuhtempo, sfuraian, sfcatatan, sfnoref, sftglnoref, sftglpenutupan, sfmatauang, sfkurs, sfhargatermasukpajak, sftotal, sfdiskonpersen, sfjmldiskon, sftotalpajak1detail, sftotalpajak2detail, sfbiayalainpersen, sfbiayalain, sftotaltransaksi, sfstatuspr, sfstatusso, sfstatuspl, sfstatusdo, sfstatusdr, sfstatuspi, sfstatussi, sfstatusrnr, sfstatussr, sfstatus, sfstatussebelumnya, sfjmlrevisi, sfcetakanke, sfinputuser, sfinputtgl, sfmodifikasiuser, sfmodifikasitgl, sfisclose, sfcustomtext1, sfcustomtext2, sfcustomtext3, sfcustomtext4, sfcustomtext5, sfcustomint1, sfcustomint2, sfcustomint3, sfcustomdbl1, sfcustomdbl2, sfcustomdbl3, sfcustomdate1, sfcustomdate2, sfcustomdate3, sfidpr) values('" & FixQuotes(drutama("sfcabang")) & "', '" & FixQuotes(drutama("sflokasi")) & "', '" & FixQuotes(drutama("sfgudang")) & "', '" & FixQuotes(drutama("sfasalbarang")) & "', " & drutama("sfasalbarangkategori") & ", '" & FixQuotes(drutama("sfjenispenjualan")) & "', " & drutama("sfjenispenjualankategori") & ", " & drutama("sfcarabayar") & ", '" & FixQuotes(drutama("sfsumber")) & "', " & drutama("sfautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("sftgl"))) & "', " & drutama("sfkodepa") & ", " & drutama("sfcustomer") & ", '" & FixQuotes(drutama("sfcustomerkontak")) & "', '" & FixQuotes(drutama("sf1alamat1")) & "', '" & FixQuotes(drutama("sf1alamat2")) & "', '" & FixQuotes(drutama("sf1alamat3")) & "', '" & FixQuotes(drutama("sf2alamat1")) & "', '" & FixQuotes(drutama("sf2alamat2")) & "', '" & FixQuotes(drutama("sf2alamat3")) & "', " & drutama("sfbagianpenjualan") & ", '" & FixQuotes(AsFormatTanggal(drutama("sftglkirim"))) & "', '" & FixQuotes(drutama("sftermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sftgljatuhtempo"))) & "', '" & FixQuotes(drutama("sfuraian")) & "', '" & FixQuotes(drutama("sfcatatan")) & "', '" & FixQuotes(drutama("sfnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sftglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sftglpenutupan"))) & "', '" & FixQuotes(drutama("sfmatauang")) & "', '" & FixDouble(drutama("sfkurs")) & "', " & drutama("sfhargatermasukpajak") & ", '" & FixDouble(drutama("sftotal")) & "', '" & FixDouble(drutama("sfdiskonpersen")) & "', '" & FixDouble(drutama("sfjmldiskon")) & "', '" & FixDouble(drutama("sftotalpajak1detail")) & "', '" & FixDouble(drutama("sftotalpajak2detail")) & "', '" & FixDouble(drutama("sfbiayalainpersen")) & "', '" & FixDouble(drutama("sfbiayalain")) & "', '" & FixDouble(drutama("sftotaltransaksi")) & "', " & drutama("sfstatuspr") & ", " & drutama("sfstatusso") & ", " & drutama("sfstatuspl") & ", " & drutama("sfstatusdo") & ", " & drutama("sfstatusdr") & ", " & drutama("sfstatuspi") & ", " & drutama("sfstatussi") & ", " & drutama("sfstatusrnr") & ", " & drutama("sfstatussr") & ", " & drutama("sfstatus") & ", " & drutama("sfstatussebelumnya") & ", " & drutama("sfjmlrevisi") & ", " & drutama("sfcetakanke") & ", " & drutama("sfinputuser") & ", NOW(), " & drutama("sfmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("sfisclose") & ", '" & FixQuotes(drutama("sfcustomtext1")) & "', '" & FixQuotes(drutama("sfcustomtext2")) & "', '" & FixQuotes(drutama("sfcustomtext3")) & "', '" & FixQuotes(drutama("sfcustomtext4")) & "', '" & FixQuotes(drutama("sfcustomtext5")) & "', " & drutama("sfcustomint1") & ", " & drutama("sfcustomint2") & ", " & drutama("sfcustomint3") & ", '" & FixDouble(drutama("sfcustomdbl1")) & "', '" & FixDouble(drutama("sfcustomdbl2")) & "', '" & FixDouble(drutama("sfcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sfcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sfcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sfcustomdate3"))) & "', '" & FixDouble(drutama("sfidpr")) & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    Dim dt2 As New DataTable
                    'Sfl disesuaikan sendiri, untuk parameternya disesuaikan sendiri.
                    dt2 = AsDataTableAmbilDariDBCon("select sfid from M5_sf where sfnotransaksi='" & notransaksi & "' AND sfinputuser= '" & userid & "' order by sfmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Sf_Detail where idsf = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idsfdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", '" & FixDouble(dr1("jmlpr")) & "', " & dr1("statuspr") & ", '" & FixDouble(dr1("jmlso")) & "', " & dr1("statusso") & ", '" & FixDouble(dr1("jmlpl")) & "', " & dr1("statuspl") & ", '" & FixDouble(dr1("jmldo")) & "', " & dr1("statusdo") & ", '" & FixDouble(dr1("jmldr")) & "', " & dr1("statusdr") & ", '" & FixDouble(dr1("jmlpi")) & "', " & dr1("statuspi") & ", '" & FixDouble(dr1("jmlsi")) & "', " & dr1("statussi") & ", '" & FixDouble(dr1("jmlrnr")) & "', " & dr1("statusrnr") & ", '" & FixDouble(dr1("jmlsr")) & "', " & dr1("statussr") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixDouble(dr1("idprdetail")) & "')")
                    Next
                    sql = "Insert into M5_Sf_Detail(idsfdetail, idsf, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlpr, statuspr, jmlso, statusso, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, idprdetail) values" & strValue2.ToString & ""
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


                If drutama("sfstatus") = 2 Then
                    If Len(updNilai) > 0 Then
                        'UPDATE OUTSTANDING TRANSAKSI =======================================================
                        'UPDATE DETAIL
                        sql = "UPDATE m4_pr_detail SET jmlsf = (CASE idprdetail " & updNilai & " ELSE jmlsf END) WHERE " & updFilter
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
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idpr, SUM(jmlbarang) as jmlbarang, SUM(jmlsf) as jmlsf FROM m4_pr_detail WHERE " & ftDetail & " GROUP BY idpr", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilai = "" : updFilter = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlsf") >= dr1("jmlbarang") Then
                                    statusOut = 2
                                ElseIf dr1("jmlsf") < 1 Then
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

                            sql = "UPDATE m4_pr SET prstatussf = (CASE prid " & updNilai & " ELSE prstatussf END) WHERE " & updFilter
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
                Dim sumber As String = "SF", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M5_SfUpdateStatus(ByVal param As String) As String
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
            Dim sumber As String = "Sf", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Sftgl, Sfnotransaksi, Sfstatus FROM M5_Sf WHERE Sfid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Sfstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m5_sf_history
            Dim rsSimpanHistory As String = SimpanHistory.M5_Sf_HistorySimpan("" & paramSplit(0) & "★M5_Sf_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                'END OF CEK TERKAIT =============================================================
            End If


            Dim idbarang As Integer = 0, jmlbarang As Double = 0, idprdetail As Integer = 0
            Dim ftOutstanding As String = "", updNilai As String = "", updFilter As String = ""
            'AMBIL DATA DETAIL
            dtdetail = AsDataTableAmbilDariDBCon("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idprdetail, urutan FROM m5_sf_detail WHERE idsf = '" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 0 Then
                For Each dr1 As DataRow In dtdetail.Rows
                    'BUAT FILTER UNTUK UPDATE ---------------------------------
                    idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : idprdetail = dr1("idprdetail")

                    'UPDATE OUTSTANDING ---------------------------
                    If idprdetail <> 0 Then
                        '1. SET NILAI UPDATE OUTSTANDING ----------
                        Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idprdetail=" & idprdetail)
                        updNilai = String.Concat("WHEN '" & idprdetail & "' THEN ROUND(jmlsf - '" & Outstanding & "', 5) ", updNilai)
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
                sql = "UPDATE m4_pr_detail SET jmlsf = (CASE idprdetail " & updNilai & " ELSE jmlsf END) WHERE " & updFilter
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
                dtOut = AsDataTableAmbilDariDBCon("SELECT idpr, SUM(jmlbarang) as jmlbarang, SUM(jmlsf) as jmlsf FROM m4_pr_detail WHERE " & ftDetail & " GROUP BY idpr", myConn)
                If dtOut.Rows.Count > 0 Then
                    'KOSONGKAN VARIABEL NILAI DAN FILTER
                    updNilai = "" : updFilter = ""
                    For Each dr1 As DataRow In dtOut.Rows
                        '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                        If dr1("jmlsf") >= dr1("jmlbarang") Then
                            statusOut = 2
                        ElseIf dr1("jmlsf") < 1 Then
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

                    sql = "UPDATE m4_pr SET prstatussf = (CASE prid " & updNilai & " ELSE prstatussf END) WHERE " & updFilter
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
            sql = "UPDATE M5_Sf SET Sfstatus = " & nilaiStatus & ", Sfmodifikasiuser='" & userid & "', Sfmodifikasitgl = NOW(), Sfposting = 0, Sfpostingtgl = '1971-01-01 00:00:00', Sfjmlrevisi = Sfjmlrevisi + 1 WHERE Sfid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_SfSearch(PostWsSearch(paramSplit(0), "M5_SfSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M5_SfDelete(ByVal param As String) As String

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
            Dim sumber As String = "Sf", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Sfid, Sfnotransaksi FROM M5_Sf WHERE Sfid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT sfcabang, sflokasi, sfsumber, sfautonotransaksi, sfnotransaksi, sftgl"
            sql &= " FROM M5_sf"
            sql &= " WHERE sfid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("sfcabang")
                lokasi = dtNomorNext.Rows(0)("sflokasi")
                sumber = dtNomorNext.Rows(0)("sfsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("sfautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("sfnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("sftgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M5_Sf_Detail WHERE idsf = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M5_Sf WHERE sfid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_SfSearch(PostWsSearch(paramSplit(0), "M5_SfSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M5_SfGetdataById(ByVal param As String) As String
        'M5_Sf_GetdataById Utama --------------------------------------------------------
        'sfid, sfcabang, sflokasi, sfgudang, sfasalbarang, sfasalbarangkategori, sfjenispenjualan, 
        'sfjenispenjualankategori, sfcarabayar, sfsumber, sfautonotransaksi, sfnotransaksi, sftgl, sfkodepa, 
        'sfcustomer, sfcustomerkontak, sf1alamat1, sf1alamat2, sf1alamat3, sf2alamat1, sf2alamat2, 
        'sf2alamat3, sfbagianpenjualan, sftglkirim, sftermin, sftgljatuhtempo, sfuraian, sfcatatan, 
        'sfnoref, sftglnoref, sftglpenutupan, sfmatauang, sfkurs, sfhargatermasukpajak, sftotal, 
        'sfdiskonpersen, sfjmldiskon, sftotalpajak1detail, sftotalpajak2detail, sfbiayalainpersen, sfbiayalain, sftotaltransaksi, 
        'sfstatuspr, sfstatusso, sfstatuspl, sfstatusdo, sfstatusdr, sfstatuspi, sfstatussi, 
        'sfstatusrnr, sfstatussr, sfstatusrealisasi, sfstatus, sfstatussebelumnya, sfjmlrevisi, sfcetakanke, 
        'sfinputuser, sfinputtgl, sfmodifikasiuser, sfmodifikasitgl, sfposting, sfpostingtgl, sfisclose, 
        'sfcustomtext1, sfcustomtext2, sfcustomtext3, sfcustomtext4, sfcustomtext5, sfcustomint1, sfcustomint2, 
        'sfcustomint3, sfcustomdbl1, sfcustomdbl2, sfcustomdbl3, sfcustomdate1, sfcustomdate2, sfcustomdate3, 
        'sfcabangnama, sflokasinama, sfgudangnama, sfcustomerkode, sfcustomernama, sfbagianpenjualankode, sfbagianpenjualannama, 
        'sfterminnama, sfterminharijatuhtempo, sfstatusnama, sfstatussebelumnyanama, sfinputusernama, sfmodifikasiusernama, ktingkatjual, kpkp,
        'sfidpr, sfnotransaksipr

        'M5_Sf_GetdataById Detail --------------------------------------------------------
        'idsfdetail, idsf, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, 
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

        Dim utama As String = "", detail As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M5_Sf~M5_Sf_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "sfid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "sfid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m5_sf_getdata")
        sql = "select sf.sfid AS sfid,sf.sfcabang AS sfcabang,sf.sflokasi AS sflokasi,sf.sfgudang AS sfgudang,sf.sfasalbarang AS sfasalbarang,sf.sfasalbarangkategori AS sfasalbarangkategori,sf.sfjenispenjualan AS sfjenispenjualan,sf.sfjenispenjualankategori AS sfjenispenjualankategori,sf.sfcarabayar AS sfcarabayar,sf.sfsumber AS sfsumber,sf.sfautonotransaksi AS sfautonotransaksi,sf.sfnotransaksi AS sfnotransaksi,sf.sftgl AS sftgl,sf.sfkodepa AS sfkodepa,sf.sfcustomer AS sfcustomer,sf.sfcustomerkontak AS sfcustomerkontak,sf.sf1alamat1 AS sf1alamat1,sf.sf1alamat2 AS sf1alamat2,sf.sf1alamat3 AS sf1alamat3,sf.sf2alamat1 AS sf2alamat1,sf.sf2alamat2 AS sf2alamat2,sf.sf2alamat3 AS sf2alamat3,sf.sfbagianpenjualan AS sfbagianpenjualan,sf.sftglkirim AS sftglkirim,sf.sftermin AS sftermin,sf.sftgljatuhtempo AS sftgljatuhtempo,sf.sfuraian AS sfuraian,sf.sfcatatan AS sfcatatan,sf.sfnoref AS sfnoref,sf.sftglnoref AS sftglnoref,sf.sftglpenutupan AS sftglpenutupan,sf.sfmatauang AS sfmatauang,sf.sfkurs AS sfkurs,sf.sfhargatermasukpajak AS sfhargatermasukpajak,sf.sftotal AS sftotal,sf.sfdiskonpersen AS sfdiskonpersen,sf.sfjmldiskon AS sfjmldiskon,sf.sftotalpajak1detail AS sftotalpajak1detail,sf.sftotalpajak2detail AS sftotalpajak2detail,sf.sfbiayalainpersen AS sfbiayalainpersen,sf.sfbiayalain AS sfbiayalain,sf.sftotaltransaksi AS sftotaltransaksi,sf.sfstatuspr AS sfstatuspr,sf.sfstatusso AS sfstatusso,sf.sfstatuspl AS sfstatuspl,sf.sfstatusdo AS sfstatusdo,sf.sfstatusdr AS sfstatusdr,sf.sfstatuspi AS sfstatuspi,sf.sfstatussi AS sfstatussi,sf.sfstatusrnr AS sfstatusrnr,sf.sfstatussr AS sfstatussr,sf.sfstatusrealisasi AS sfstatusrealisasi,sf.sfstatus AS sfstatus,sf.sfstatussebelumnya AS sfstatussebelumnya,sf.sfjmlrevisi AS sfjmlrevisi,sf.sfcetakanke AS sfcetakanke,sf.sfinputuser AS sfinputuser,sf.sfinputtgl AS sfinputtgl,sf.sfmodifikasiuser AS sfmodifikasiuser,sf.sfmodifikasitgl AS sfmodifikasitgl,sf.sfposting AS sfposting,sf.sfpostingtgl AS sfpostingtgl,sf.sfisclose AS sfisclose,sf.sfcustomtext1 AS sfcustomtext1,sf.sfcustomtext2 AS sfcustomtext2,sf.sfcustomtext3 AS sfcustomtext3,sf.sfcustomtext4 AS sfcustomtext4,sf.sfcustomtext5 AS sfcustomtext5,sf.sfcustomint1 AS sfcustomint1,sf.sfcustomint2 AS sfcustomint2,sf.sfcustomint3 AS sfcustomint3,sf.sfcustomdbl1 AS sfcustomdbl1,sf.sfcustomdbl2 AS sfcustomdbl2,sf.sfcustomdbl3 AS sfcustomdbl3,sf.sfcustomdate1 AS sfcustomdate1,sf.sfcustomdate2 AS sfcustomdate2,sf.sfcustomdate3 AS sfcustomdate3,br.bnama AS sfcabangnama,lc.lnama AS sflokasinama,wh.wnama AS sfgudangnama,c1.ktingkatjual,c1.kkode AS sfcustomerkode,c1.knama AS sfcustomernama,c2.kkode AS sfbagianpenjualankode,c2.knama AS sfbagianpenjualannama,tr.trnama AS sfterminnama,tr.trharijatuhtempo AS sfterminharijatuhtempo,st1.nama AS sfstatusnama,st2.nama AS sfstatussebelumnyanama,u1.unama AS sfinputusernama,u2.unama AS sfmodifikasiusernama,sfd.idsfdetail AS idsfdetail,sfd.idsf AS idsf,sfd.idbarang AS idbarang,sfd.namabarang AS namabarang,sfd.tipebarang AS tipebarang,sfd.jml AS jml,sfd.satuan AS satuan,sfd.nilaisatuan AS nilaisatuan,sfd.jmlbarang AS jmlbarang,sfd.satuanbarang AS satuanbarang,sfd.matauang AS matauang,sfd.kurs AS kurs,sfd.harga AS harga,sfd.diskon AS diskon,sfd.jmldiskon AS jmldiskon,sfd.pajak1 AS pajak1,sfd.jmlpajak1 AS jmlpajak1,sfd.pajak2 AS pajak2,sfd.jmlpajak2 AS jmlpajak2,sfd.cabang AS cabang,sfd.lokasi AS lokasi,sfd.gudang AS gudang,sfd.costcenter AS costcenter,sfd.divisi AS divisi,sfd.subdivisi AS subdivisi,sfd.proyek AS proyek,sfd.catatan AS catatan,sfd.urutan AS urutan,sfd.jmlpr AS jmlpr,sfd.statuspr AS statuspr,sfd.jmlso AS jmlso,sfd.statusso AS statusso,sfd.jmlpl AS jmlpl,sfd.statuspl AS statuspl,sfd.jmldo AS jmldo,sfd.statusdo AS statusdo,sfd.jmldr AS jmldr,sfd.statusdr AS statusdr,sfd.jmlpi AS jmlpi,sfd.statuspi AS statuspi,sfd.jmlsi AS jmlsi,sfd.statussi AS statussi,sfd.jmlrnr AS jmlrnr,sfd.statusrnr AS statusrnr,sfd.jmlsr AS jmlsr,sfd.statussr AS statussr,sfd.jmlrealisasi AS jmlrealisasi,sfd.statusrealisasi AS statusrealisasi,sfd.isclose AS isclose,sfd.customtext1 AS customtext1,sfd.customtext2 AS customtext2,sfd.customtext3 AS customtext3,sfd.customdbl1 AS customdbl1,sfd.customdbl2 AS customdbl2,sfd.customdbl3 AS customdbl3,sfd.customdate1 AS customdate1,sfd.customdate2 AS customdate2,sfd.customdate3 AS customdate3,i.bkode AS kodebarang,t1.tnama AS pajak1nama,t1.tnilai AS pajak1nilai,t2.tnama AS pajak2nama,t2.tnilai AS pajak2nilai,brd.bnama AS cabangnama,lcd.lnama AS lokasinama,whd.wnama AS gudangnama,cc.ccnama AS costcenternama,d.dnama AS divisinama,sd.sdnama AS subdivisinama,p.pnama AS proyeknama, c1.kpkp, sf.sfidpr, pru.prnotransaksi as sfnotransaksipr, sfd.idprdetail, pr.prnotransaksi from m5_sf sf  join m5_sf_detail sfd on sf.sfid = sfd.idsf left join m1_branch br on br.bkode = sf.sfcabang left join m1_location lc on lc.lkode = sf.sflokasi left join m1_warehouse wh on wh.wkode = sf.sfgudang left join m1_contact c1 on c1.kid = sf.sfcustomer left join m1_contact c2 on c2.kid = sf.sfbagianpenjualan left join m1_terms tr on sf.sftermin = tr.trkode left join m0_status st1 on st1.kode = sf.sfstatus left join m0_status st2 on st2.kode = sf.sfstatussebelumnya left join m0_user u1 on u1.userid = sf.sfinputuser left join m0_user u2 on u2.userid = sf.sfmodifikasiuser left join m1_item i on i.bid = sfd.idbarang left join m1_tax t1 on sfd.pajak1 = t1.tkode left join m1_tax t2 on sfd.pajak2 = t2.tkode left join m1_branch brd on sfd.cabang = brd.bkode left join m1_location lcd on sfd.lokasi = lcd.lkode left join m1_warehouse whd on sfd.gudang = whd.wkode left join m1_cost_center cc on sfd.costcenter = cc.cckode left join m1_division d on sfd.divisi = d.dkode left join m1_project p on sfd.proyek = p.pkode left join m1_subdivision sd on sfd.subdivisi = sd.sdkode left join m4_pr pru on sf.sfidpr = pru.prid left join m4_pr_detail prd ON sfd.idprdetail = prd.idprdetail left join m4_pr pr on prd.idpr = pr.prid"

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("sfid"), 0), sptField,
                     FxDB(drutama("sfcabang"), ""), sptField,
                     FxDB(drutama("sflokasi"), ""), sptField,
                     FxDB(drutama("sfgudang"), ""), sptField,
                     FxDB(drutama("sfasalbarang"), ""), sptField,
                     FxDB(drutama("sfasalbarangkategori"), 0), sptField,
                     FxDB(drutama("sfjenispenjualan"), ""), sptField,
                     FxDB(drutama("sfjenispenjualankategori"), 0), sptField,
                     FxDB(drutama("sfcarabayar"), 0), sptField,
                     FxDB(drutama("sfsumber"), ""), sptField,
                     FxDB(drutama("sfautonotransaksi"), 0), sptField,
                     FxDB(drutama("sfnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sftgl"), ""), formatTgl), sptField,
                     FxDB(drutama("sfkodepa"), 0), sptField,
                     FxDB(drutama("sfcustomer"), 0), sptField,
                     FxDB(drutama("sfcustomerkontak"), ""), sptField,
                     FxDB(drutama("sf1alamat1"), ""), sptField,
                     FxDB(drutama("sf1alamat2"), ""), sptField,
                     FxDB(drutama("sf1alamat3"), ""), sptField,
                     FxDB(drutama("sf2alamat1"), ""), sptField,
                     FxDB(drutama("sf2alamat2"), ""), sptField,
                     FxDB(drutama("sf2alamat3"), ""), sptField,
                     FxDB(drutama("sfbagianpenjualan"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sftglkirim"), ""), formatTgl), sptField,
                     FxDB(drutama("sftermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sftgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("sfuraian"), ""), sptField,
                     FxDB(drutama("sfcatatan"), ""), sptField,
                     FxDB(drutama("sfnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sftglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sftglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("sfmatauang"), ""), sptField,
                     FxDB(drutama("sfkurs"), 0), sptField,
                     FxDB(drutama("sfhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("sftotal"), 0), sptField,
                     FxDB(drutama("sfdiskonpersen"), ""), sptField,
                     FxDB(drutama("sfjmldiskon"), 0), sptField,
                     FxDB(drutama("sftotalpajak1detail"), 0), sptField,
                     FxDB(drutama("sftotalpajak2detail"), 0), sptField,
                     FxDB(drutama("sfbiayalainpersen"), 0), sptField,
                     FxDB(drutama("sfbiayalain"), 0), sptField,
                     FxDB(drutama("sftotaltransaksi"), 0), sptField,
                     FxDB(drutama("sfstatuspr"), 0), sptField,
                     FxDB(drutama("sfstatusso"), 0), sptField,
                     FxDB(drutama("sfstatuspl"), 0), sptField,
                     FxDB(drutama("sfstatusdo"), 0), sptField,
                     FxDB(drutama("sfstatusdr"), 0), sptField,
                     FxDB(drutama("sfstatuspi"), 0), sptField,
                     FxDB(drutama("sfstatussi"), 0), sptField,
                     FxDB(drutama("sfstatusrnr"), 0), sptField,
                     FxDB(drutama("sfstatussr"), 0), sptField,
                     FxDB(drutama("sfstatusrealisasi"), 0), sptField,
                     FxDB(drutama("sfstatus"), 0), sptField,
                     FxDB(drutama("sfstatussebelumnya"), 0), sptField,
                     FxDB(drutama("sfjmlrevisi"), 0), sptField,
                     FxDB(drutama("sfcetakanke"), 0), sptField,
                     FxDB(drutama("sfinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sfinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sfmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sfmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sfposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sfpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sfisclose"), 0), sptField,
                     FxDB(drutama("sfcustomtext1"), ""), sptField,
                     FxDB(drutama("sfcustomtext2"), ""), sptField,
                     FxDB(drutama("sfcustomtext3"), ""), sptField,
                     FxDB(drutama("sfcustomtext4"), ""), sptField,
                     FxDB(drutama("sfcustomtext5"), ""), sptField,
                     FxDB(drutama("sfcustomint1"), 0), sptField,
                     FxDB(drutama("sfcustomint2"), 0), sptField,
                     FxDB(drutama("sfcustomint3"), 0), sptField,
                     FxDB(drutama("sfcustomdbl1"), 0), sptField,
                     FxDB(drutama("sfcustomdbl2"), 0), sptField,
                     FxDB(drutama("sfcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sfcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sfcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sfcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("sfcabangnama"), ""), sptField,
                     FxDB(drutama("sflokasinama"), ""), sptField,
                     FxDB(drutama("sfgudangnama"), ""), sptField,
                     FxDB(drutama("sfcustomerkode"), ""), sptField,
                     FxDB(drutama("sfcustomernama"), ""), sptField,
                     FxDB(drutama("sfbagianpenjualankode"), ""), sptField,
                     FxDB(drutama("sfbagianpenjualannama"), ""), sptField,
                     FxDB(drutama("sfterminnama"), ""), sptField,
                     FxDB(drutama("sfterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("sfstatusnama"), ""), sptField,
                     FxDB(drutama("sfstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("sfinputusernama"), ""), sptField,
                     FxDB(drutama("sfmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("ktingkatjual"), 0), sptField,
                     FxDB(drutama("kpkp"), 0), sptField,
                     FxDB(drutama("sfidpr"), 0), sptField,
                     FxDB(drutama("sfnotransaksipr"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idsfdetail"), 0), sptField,
                     FxDB(dr("idsf"), 0), sptField,
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
        strResultData = String.Concat(utama, sptSubParam, detail)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sfid, sfcabang, sflokasi, sfgudang, sfasalbarang, sfasalbarangkategori, sfjenispenjualan, sfjenispenjualankategori, sfcarabayar, sfsumber, sfautonotransaksi, sfnotransaksi, sftgl, sfkodepa, sfcustomer, sfcustomerkontak, sf1alamat1, sf1alamat2, sf1alamat3, sf2alamat1, sf2alamat2, sf2alamat3, sfbagianpenjualan, sftglkirim, sftermin, sftgljatuhtempo, sfuraian, sfcatatan, sfnoref, sftglnoref, sftglpenutupan, sfmatauang, sfkurs, sfhargatermasukpajak, sftotal, sfdiskonpersen, sfjmldiskon, sftotalpajak1detail, sftotalpajak2detail, sfbiayalainpersen, sfbiayalain, sftotaltransaksi, sfstatuspr, sfstatusso, sfstatuspl, sfstatusdo, sfstatusdr, sfstatuspi, sfstatussi, sfstatusrnr, sfstatussr, sfstatusrealisasi, sfstatus, sfstatussebelumnya, sfjmlrevisi, sfcetakanke, sfinputuser, sfinputtgl, sfmodifikasiuser, sfmodifikasitgl, sfposting, sfpostingtgl, sfisclose, sfcustomtext1, sfcustomtext2, sfcustomtext3, sfcustomtext4, sfcustomtext5, sfcustomint1, sfcustomint2, sfcustomint3, sfcustomdbl1, sfcustomdbl2, sfcustomdbl3, sfcustomdate1, sfcustomdate2, sfcustomdate3, sfcabangnama, sflokasinama, sfgudangnama, sfcustomerkode, sfcustomernama, sfbagianpenjualankode, sfbagianpenjualannama, sfterminnama, sfterminharijatuhtempo, sfstatusnama, sfstatussebelumnyanama, sfinputusernama, sfmodifikasiusernama, ktingkatjual, kpkp, sfidpr, sfnotransaksipr" & sptSubParam & "idsfdetail, idsf, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlpr, statuspr, jmlso, statusso, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, idprdetail, prnotransaksi"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SfSearch(ByVal param As String) As String
        'M5_SfSearch --------------------------------------------------------
        'sfid, sfcabang, sflokasi, sfgudang, sfasalbarang, sfasalbarangkategori, sfjenispenjualan, 
        'sfjenispenjualankategori, sfcarabayar, sfsumber, sfautonotransaksi, sfnotransaksi, sftgl, sfkodepa, 
        'sfcustomer, sfcustomerkontak, sf1alamat1, sf1alamat2, sf1alamat3, sf2alamat1, sf2alamat2, 
        'sf2alamat3, sfbagianpenjualan, sftglkirim, sftermin, sftgljatuhtempo, sfuraian, sfcatatan, 
        'sfnoref, sftglnoref, sftglpenutupan, sfmatauang, sfkurs, sfhargatermasukpajak, sftotal, 
        'sfdiskonpersen, sfjmldiskon, sftotalpajak1detail, sftotalpajak2detail, sfbiayalainpersen, sfbiayalain, sftotaltransaksi, 
        'sfstatuspr, sfstatusso, sfstatuspl, sfstatusdo, sfstatusdr, sfstatuspi, sfstatussi, 
        'sfstatusrnr, sfstatussr, sfstatusrealisasi, sfstatus, sfstatussebelumnya, sfjmlrevisi, sfcetakanke, 
        'sfinputuser, sfinputtgl, sfmodifikasiuser, sfmodifikasitgl, sfposting, sfpostingtgl, sfisclose, 
        'sfcabangnama, sflokasinama, sfgudangnama, sfcustomerkode, sfcustomernama, sfbagianpenjualankode, sfbagianpenjualannama, 
        'sfstatusnama, sfstatussebelumnyanama, sfinputusernama, sfmodifikasiusernama


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
        sql = "select `sf`.`sfid` AS `sfid`,`sf`.`sfcabang` AS `sfcabang`,`sf`.`sflokasi` AS `sflokasi`,`sf`.`sfgudang` AS `sfgudang`,`sf`.`sfasalbarang` AS `sfasalbarang`,`sf`.`sfasalbarangkategori` AS `sfasalbarangkategori`,`sf`.`sfjenispenjualan` AS `sfjenispenjualan`,`sf`.`sfjenispenjualankategori` AS `sfjenispenjualankategori`,`sf`.`sfcarabayar` AS `sfcarabayar`,`sf`.`sfsumber` AS `sfsumber`,`sf`.`sfautonotransaksi` AS `sfautonotransaksi`,`sf`.`sfnotransaksi` AS `sfnotransaksi`,`sf`.`sftgl` AS `sftgl`,`sf`.`sfkodepa` AS `sfkodepa`,`sf`.`sfcustomer` AS `sfcustomer`,`sf`.`sfcustomerkontak` AS `sfcustomerkontak`,`sf`.`sf1alamat1` AS `sf1alamat1`,`sf`.`sf1alamat2` AS `sf1alamat2`,`sf`.`sf1alamat3` AS `sf1alamat3`,`sf`.`sf2alamat1` AS `sf2alamat1`,`sf`.`sf2alamat2` AS `sf2alamat2`,`sf`.`sf2alamat3` AS `sf2alamat3`,`sf`.`sfbagianpenjualan` AS `sfbagianpenjualan`,`sf`.`sftglkirim` AS `sftglkirim`,`sf`.`sftermin` AS `sftermin`,`sf`.`sftgljatuhtempo` AS `sftgljatuhtempo`,`sf`.`sfuraian` AS `sfuraian`,`sf`.`sfcatatan` AS `sfcatatan`,`sf`.`sfnoref` AS `sfnoref`,`sf`.`sftglnoref` AS `sftglnoref`,`sf`.`sftglpenutupan` AS `sftglpenutupan`,`sf`.`sfmatauang` AS `sfmatauang`,`sf`.`sfkurs` AS `sfkurs`,`sf`.`sfhargatermasukpajak` AS `sfhargatermasukpajak`,`sf`.`sftotal` AS `sftotal`,`sf`.`sfdiskonpersen` AS `sfdiskonpersen`,`sf`.`sfjmldiskon` AS `sfjmldiskon`,`sf`.`sftotalpajak1detail` AS `sftotalpajak1detail`,`sf`.`sftotalpajak2detail` AS `sftotalpajak2detail`,`sf`.`sfbiayalainpersen` AS `sfbiayalainpersen`,`sf`.`sfbiayalain` AS `sfbiayalain`,`sf`.`sftotaltransaksi` AS `sftotaltransaksi`,`sf`.`sfstatuspr` AS `sfstatuspr`,`sf`.`sfstatusso` AS `sfstatusso`,`sf`.`sfstatuspl` AS `sfstatuspl`,`sf`.`sfstatusdo` AS `sfstatusdo`,`sf`.`sfstatusdr` AS `sfstatusdr`,`sf`.`sfstatuspi` AS `sfstatuspi`,`sf`.`sfstatussi` AS `sfstatussi`,`sf`.`sfstatusrnr` AS `sfstatusrnr`,`sf`.`sfstatussr` AS `sfstatussr`,`sf`.`sfstatusrealisasi` AS `sfstatusrealisasi`,`sf`.`sfstatus` AS `sfstatus`,`sf`.`sfstatussebelumnya` AS `sfstatussebelumnya`,`sf`.`sfjmlrevisi` AS `sfjmlrevisi`,`sf`.`sfcetakanke` AS `sfcetakanke`,`sf`.`sfinputuser` AS `sfinputuser`,`sf`.`sfinputtgl` AS `sfinputtgl`,`sf`.`sfmodifikasiuser` AS `sfmodifikasiuser`,`sf`.`sfmodifikasitgl` AS `sfmodifikasitgl`,`sf`.`sfposting` AS `sfposting`,`sf`.`sfpostingtgl` AS `sfpostingtgl`,`sf`.`sfisclose` AS `sfisclose`,`br`.`bnama` AS `sfcabangnama`,`lc`.`lnama` AS `sflokasinama`,`wh`.`wnama` AS `sfgudangnama`,`c1`.`kkode` AS `sfcustomerkode`,`c1`.`knama` AS `sfcustomernama`,`c2`.`kkode` AS `sfbagianpenjualankode`,`c2`.`knama` AS `sfbagianpenjualannama`,`st1`.`nama` AS `sfstatusnama`,`st2`.`nama` AS `sfstatussebelumnyanama`,`u1`.`unama` AS `sfinputusernama`,`u2`.`unama` AS `sfmodifikasiusernama` from (((((((((`m5_sf` `sf` left join `m1_branch` `br` on((`br`.`bkode` = `sf`.`sfcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `sf`.`sflokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `sf`.`sfgudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `sf`.`sfcustomer`))) left join `m1_contact` `c2` on((`c2`.`kid` = `sf`.`sfbagianpenjualan`))) left join `m0_status` `st1` on((`st1`.`kode` = `sf`.`sfstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `sf`.`sfstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `sf`.`sfinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `sf`.`sfmodifikasiuser`)))"

        dt = AmbilData("aplikasi1-M5_Sf_V", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("sfid"), 0), sptField,
                     FxDB(dr("sfcabang"), ""), sptField,
                     FxDB(dr("sflokasi"), ""), sptField,
                     FxDB(dr("sfgudang"), ""), sptField,
                     FxDB(dr("sfasalbarang"), ""), sptField,
                     FxDB(dr("sfasalbarangkategori"), 0), sptField,
                     FxDB(dr("sfjenispenjualan"), ""), sptField,
                     FxDB(dr("sfjenispenjualankategori"), 0), sptField,
                     FxDB(dr("sfcarabayar"), 0), sptField,
                     FxDB(dr("sfsumber"), ""), sptField,
                     FxDB(dr("sfautonotransaksi"), 0), sptField,
                     FxDB(dr("sfnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sftgl"), ""), formatTgl), sptField,
                     FxDB(dr("sfkodepa"), 0), sptField,
                     FxDB(dr("sfcustomer"), 0), sptField,
                     FxDB(dr("sfcustomerkontak"), ""), sptField,
                     FxDB(dr("sf1alamat1"), ""), sptField,
                     FxDB(dr("sf1alamat2"), ""), sptField,
                     FxDB(dr("sf1alamat3"), ""), sptField,
                     FxDB(dr("sf2alamat1"), ""), sptField,
                     FxDB(dr("sf2alamat2"), ""), sptField,
                     FxDB(dr("sf2alamat3"), ""), sptField,
                     FxDB(dr("sfbagianpenjualan"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sftglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("sftermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sftgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("sfuraian"), ""), sptField,
                     FxDB(dr("sfcatatan"), ""), sptField,
                     FxDB(dr("sfnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sftglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sftglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("sfmatauang"), ""), sptField,
                     FxDB(dr("sfkurs"), 0), sptField,
                     FxDB(dr("sfhargatermasukpajak"), 0), sptField,
                     FxDB(dr("sftotal"), 0), sptField,
                     FxDB(dr("sfdiskonpersen"), ""), sptField,
                     FxDB(dr("sfjmldiskon"), 0), sptField,
                     FxDB(dr("sftotalpajak1detail"), 0), sptField,
                     FxDB(dr("sftotalpajak2detail"), 0), sptField,
                     FxDB(dr("sfbiayalainpersen"), 0), sptField,
                     FxDB(dr("sfbiayalain"), 0), sptField,
                     FxDB(dr("sftotaltransaksi"), 0), sptField,
                     FxDB(dr("sfstatuspr"), 0), sptField,
                     FxDB(dr("sfstatusso"), 0), sptField,
                     FxDB(dr("sfstatuspl"), 0), sptField,
                     FxDB(dr("sfstatusdo"), 0), sptField,
                     FxDB(dr("sfstatusdr"), 0), sptField,
                     FxDB(dr("sfstatuspi"), 0), sptField,
                     FxDB(dr("sfstatussi"), 0), sptField,
                     FxDB(dr("sfstatusrnr"), 0), sptField,
                     FxDB(dr("sfstatussr"), 0), sptField,
                     FxDB(dr("sfstatusrealisasi"), 0), sptField,
                     FxDB(dr("sfstatus"), 0), sptField,
                     FxDB(dr("sfstatussebelumnya"), 0), sptField,
                     FxDB(dr("sfjmlrevisi"), 0), sptField,
                     FxDB(dr("sfcetakanke"), 0), sptField,
                     FxDB(dr("sfinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sfinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sfmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sfmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sfposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sfpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sfisclose"), 0), sptField,
                     FxDB(dr("sfcabangnama"), ""), sptField,
                     FxDB(dr("sflokasinama"), ""), sptField,
                     FxDB(dr("sfgudangnama"), ""), sptField,
                     FxDB(dr("sfcustomerkode"), ""), sptField,
                     FxDB(dr("sfcustomernama"), ""), sptField,
                     FxDB(dr("sfbagianpenjualankode"), ""), sptField,
                     FxDB(dr("sfbagianpenjualannama"), ""), sptField,
                     FxDB(dr("sfstatusnama"), ""), sptField,
                     FxDB(dr("sfstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("sfinputusernama"), ""), sptField,
                     FxDB(dr("sfmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sfid, sfcabang, sflokasi, sfgudang, sfasalbarang, sfasalbarangkategori, sfjenispenjualan, sfjenispenjualankategori, sfcarabayar, sfsumber, sfautonotransaksi, sfnotransaksi, sftgl, sfkodepa, sfcustomer, sfcustomerkontak, sf1alamat1, sf1alamat2, sf1alamat3, sf2alamat1, sf2alamat2, sf2alamat3, sfbagianpenjualan, sftglkirim, sftermin, sftgljatuhtempo, sfuraian, sfcatatan, sfnoref, sftglnoref, sftglpenutupan, sfmatauang, sfkurs, sfhargatermasukpajak, sftotal, sfdiskonpersen, sfjmldiskon, sftotalpajak1detail, sftotalpajak2detail, sfbiayalainpersen, sfbiayalain, sftotaltransaksi, sfstatuspr, sfstatusso, sfstatuspl, sfstatusdo, sfstatusdr, sfstatuspi, sfstatussi, sfstatusrnr, sfstatussr, sfstatusrealisasi, sfstatus, sfstatussebelumnya, sfjmlrevisi, sfcetakanke, sfinputuser, sfinputtgl, sfmodifikasiuser, sfmodifikasitgl, sfposting, sfpostingtgl, sfisclose, sfcabangnama, sflokasinama, sfgudangnama, sfcustomerkode, sfcustomernama, sfbagianpenjualankode, sfbagianpenjualannama, sfstatusnama, sfstatussebelumnyanama, sfinputusernama, sfmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_Sf_Detail_VSearch(ByVal param As String) As String
        'M5_Sf_Detail_VSearch --------------------------------------------------------
        'idsfdetail, idsf, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, 
        'jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, 
        'gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'jmlpr, statuspr, jmlso, statusso, jmlpl, statuspl, jmldo, 
        'statusdo, jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, 
        'jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3, sfnotransaksi, sfuraian, sfcatatan, sfnoref, sftglnoref, 
        'sftglkirim, sfcustomerkontak, sf1alamat1, sf1alamat2, sf1alamat3, sf2alamat1, sf2alamat2, 
        'sf2alamat3, sfbagianpenjualan, sfbagianpenjualankode, sfbagianpenjualannama, sftermin, sfterminnama, sfterminharijatuhtempo, 
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
        'sql = query.PanggilQuery("m5_sf_detail_v")
        sql = "select `sfd`.`idsfdetail` AS `idsfdetail`,`sfd`.`idsf` AS `idsf`,`sfd`.`idbarang` AS `idbarang`,`sfd`.`namabarang` AS `namabarang`,`sfd`.`tipebarang` AS `tipebarang`,`sfd`.`jml` AS `jml`,`sfd`.`satuan` AS `satuan`,`sfd`.`nilaisatuan` AS `nilaisatuan`,`sfd`.`jmlbarang` AS `jmlbarang`,`sfd`.`satuanbarang` AS `satuanbarang`,`sfd`.`matauang` AS `matauang`,`sfd`.`kurs` AS `kurs`,`sfd`.`harga` AS `harga`,`sfd`.`diskon` AS `diskon`,`sfd`.`jmldiskon` AS `jmldiskon`,`sfd`.`pajak1` AS `pajak1`,`sfd`.`jmlpajak1` AS `jmlpajak1`,`sfd`.`pajak2` AS `pajak2`,`sfd`.`jmlpajak2` AS `jmlpajak2`,`sfd`.`cabang` AS `cabang`,`sfd`.`lokasi` AS `lokasi`,`sfd`.`gudang` AS `gudang`,`sfd`.`costcenter` AS `costcenter`,`sfd`.`divisi` AS `divisi`,`sfd`.`subdivisi` AS `subdivisi`,`sfd`.`proyek` AS `proyek`,`sfd`.`catatan` AS `catatan`,`sfd`.`urutan` AS `urutan`,`sfd`.`jmlpr` AS `jmlpr`,`sfd`.`statuspr` AS `statuspr`,`sfd`.`jmlso` AS `jmlso`,`sfd`.`statusso` AS `statusso`,`sfd`.`jmlpl` AS `jmlpl`,`sfd`.`statuspl` AS `statuspl`,`sfd`.`jmldo` AS `jmldo`,`sfd`.`statusdo` AS `statusdo`,`sfd`.`jmldr` AS `jmldr`,`sfd`.`statusdr` AS `statusdr`,`sfd`.`jmlpi` AS `jmlpi`,`sfd`.`statuspi` AS `statuspi`,`sfd`.`jmlsi` AS `jmlsi`,`sfd`.`statussi` AS `statussi`,`sfd`.`jmlrnr` AS `jmlrnr`,`sfd`.`statusrnr` AS `statusrnr`,`sfd`.`jmlsr` AS `jmlsr`,`sfd`.`statussr` AS `statussr`,`sfd`.`jmlrealisasi` AS `jmlrealisasi`,`sfd`.`statusrealisasi` AS `statusrealisasi`,`sfd`.`isclose` AS `isclose`,`sfd`.`customtext1` AS `customtext1`,`sfd`.`customtext2` AS `customtext2`,`sfd`.`customtext3` AS `customtext3`,`sfd`.`customdbl1` AS `customdbl1`,`sfd`.`customdbl2` AS `customdbl2`,`sfd`.`customdbl3` AS `customdbl3`,`sfd`.`customdate1` AS `customdate1`,`sfd`.`customdate2` AS `customdate2`,`sfd`.`customdate3` AS `customdate3`,`sf`.`sfnotransaksi` AS `sfnotransaksi`,`sf`.`sfuraian` AS `sfuraian`,`sf`.`sfcatatan` AS `sfcatatan`,`sf`.`sfnoref` AS `sfnoref`,`sf`.`sftglnoref` AS `sftglnoref`,`sf`.`sftglkirim` AS `sftglkirim`,`sf`.`sfcustomerkontak` AS `sfcustomerkontak`,`sf`.`sf1alamat1` AS `sf1alamat1`,`sf`.`sf1alamat2` AS `sf1alamat2`,`sf`.`sf1alamat3` AS `sf1alamat3`,`sf`.`sf2alamat1` AS `sf2alamat1`,`sf`.`sf2alamat2` AS `sf2alamat2`,`sf`.`sf2alamat3` AS `sf2alamat3`,`sf`.`sfbagianpenjualan` AS `sfbagianpenjualan`,`c1`.`kkode` AS `sfbagianpenjualankode`,`c1`.`knama` AS `sfbagianpenjualannama`,`sf`.`sftermin` AS `sftermin`,`tr`.`trnama` AS `sfterminnama`,`tr`.`trharijatuhtempo` AS `sfterminharijatuhtempo`,`i`.`bkode` AS `kodebarang`,`i`.`bhargabeli` AS `bhargabeli`,`i`.`bstok` AS `bstok`,`i`.`bsuplier` AS `bsuplier`,`c2`.`kkode` AS `bsuplierkode`,`c2`.`knama` AS `bsupliernama`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,((`sfd`.`jmlbarang` - `sfd`.`jmlpr`) / `sfd`.`nilaisatuan`) AS `jmlsisapr`,((`sfd`.`jmlbarang` - `sfd`.`jmlso`) / `sfd`.`nilaisatuan`) AS `jmlsisaso`,((`sfd`.`jmlbarang` - `sfd`.`jmlrealisasi`) / `sfd`.`nilaisatuan`) AS `jmlsisarealisasi`, i.bjmllapangan, i.bsatuanlapangan, i.basset, t1.takunbeli as pajak1akunbeli, t1c1.cnama as pajak1akunbelinama, t1.takunjual as pajak1akunjual, t1c2.cnama as pajak1akunjualnama, t2.takunbeli as pajak2akunbeli, t2c1.cnama as pajak2akunbelinama, t2.takunjual as pajak2akunjual, t2c2.cnama as pajak2akunjualnama from `m5_sf_detail` `sfd` left join `m5_sf` `sf` on `sfd`.`idsf` = `sf`.`sfid` left join `m1_contact` `c1` on `sf`.`sfbagianpenjualan` = `c1`.`kid` left join `m1_terms` `tr` on `sf`.`sftermin` = `tr`.`trkode` left join `m1_item` `i` on `sfd`.`idbarang` = `i`.`bid` left join `m1_tax` `t1` on `sfd`.`pajak1` = `t1`.`tkode` left join `m1_tax` `t2` on `sfd`.`pajak2` = `t2`.`tkode` left join `m1_contact` `c2` on `i`.`bsuplier` = `c2`.`kid` left join m1_coa t1c1 on t1.takunbeli = t1c1.cnomor left join m1_coa t1c2 on t1.takunjual = t1c2.cnomor left join m1_coa t2c1 on t2.takunbeli = t2c1.cnomor left join m1_coa t2c2 on t2.takunjual = t2c2.cnomor"

        dt = AmbilData("aplikasi1-M5_Sf_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idsfdetail"), 0), sptField,
                     FxDB(dr("idsf"), 0), sptField,
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
                     FxDB(dr("sfnotransaksi"), ""), sptField,
                     FxDB(dr("sfuraian"), ""), sptField,
                     FxDB(dr("sfcatatan"), ""), sptField,
                     FxDB(dr("sfnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sftglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sftglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("sfcustomerkontak"), ""), sptField,
                     FxDB(dr("sf1alamat1"), ""), sptField,
                     FxDB(dr("sf1alamat2"), ""), sptField,
                     FxDB(dr("sf1alamat3"), ""), sptField,
                     FxDB(dr("sf2alamat1"), ""), sptField,
                     FxDB(dr("sf2alamat2"), ""), sptField,
                     FxDB(dr("sf2alamat3"), ""), sptField,
                     FxDB(dr("sfbagianpenjualan"), 0), sptField,
                     FxDB(dr("sfbagianpenjualankode"), ""), sptField,
                     FxDB(dr("sfbagianpenjualannama"), ""), sptField,
                     FxDB(dr("sftermin"), ""), sptField,
                     FxDB(dr("sfterminnama"), ""), sptField,
                     FxDB(dr("sfterminharijatuhtempo"), 0), sptField,
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
                     FxDB(dr("pajak2akunjualnama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idsfdetail, idsf, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlpr, statuspr, jmlso, statusso, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, sfnotransaksi, sfuraian, sfcatatan, sfnoref, sftglnoref, sftglkirim, sfcustomerkontak, sf1alamat1, sf1alamat2, sf1alamat3, sf2alamat1, sf2alamat2, sf2alamat3, sfbagianpenjualan, sfbagianpenjualankode, sfbagianpenjualannama, sftermin, sfterminnama, sfterminharijatuhtempo, kodebarang, bhargabeli, bstok, bsuplier, bsuplierkode, bsupliernama, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisapr, jmlsisaso, jmlsisarealisasi, bjmllapangan, bsatuanlapangan, basset, pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, pajak2akunjualnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SfTerkait(ByVal param As String) As String
        'M5_SfTerkait --------------------------------------------------------
        'sfid, sfnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "sfid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2) & " AND sfid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "sfid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        ''PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.m5_sf_terkait(Filter)
        sql = m5_sf_terkait(Filter)


        dt = AmbilData("aplikasi1-m5_sf_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("sfid"), 0), sptField,
                     FxDB(dr("sfnotransaksi"), ""), sptField,
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
            result(2) = "Related SF data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sfid, sfnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SfTerkait_S(ByVal param As String) As String
        'M5_SfTerkait --------------------------------------------------------
        'sfid, sfnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "sfid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2) & " AND sfid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "sfid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        ''PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.m5_sf_terkait(Filter)
        sql = m5_sf_terkait(Filter)


        dt = AmbilData("aplikasi1-m5_sf_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("sfid"), 0), sptField,
                     FxDB(dr("sfnotransaksi"), ""), sptField,
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
            result(2) = "Related SF data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sfid, sfnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

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
            sql = "SELECT prd.idprdetail, (prd.jmlbarang - prd.jmlsf) as sisasf, i.bid, i.bkode FROM m4_pr_detail AS prd INNER JOIN m1_item AS i ON prd.idbarang = i.bid WHERE " & ftOutstanding
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisasf")

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
    Public Function m5_sf_terkait(ByVal strFilter As String) As String
        Dim sql As String
        Dim filter1 As String = "", filter2 As String = "", filter3 As String = "", filter4 As String = "", filter5 As String = "", filter6 As String = "", filter7 As String = "", filter8 As String = "", filter9 As String = ""

        'Replace Filter & Sort
        If (strFilter.Length > 0) Then
            'filter1 = strFilter
            'filter1 = filter1 & " AND ((`m4_pr`.`prstatus` = 2) or (`m4_pr`.`prstatus` = 3) or (`m4_pr`.`prstatus` = 4) or (`m4_pr`.`prstatus` = 7))"

            filter2 = strFilter
            filter2 = filter2 & " AND ((`m5_so`.`sostatus` = 2) or (`m5_so`.`sostatus` = 3) or (`m5_so`.`sostatus` = 4) or (`m5_so`.`sostatus` = 7))"

            'filter3 = strFilter
            'filter3 = filter3 & " AND ((`m5_pl`.`plstatus` = 2) or (`m5_pl`.`plstatus` = 3) or (`m5_pl`.`plstatus` = 4) or (`m5_pl`.`plstatus` = 7))"

            'filter4 = strFilter
            'filter4 = filter4 & " AND ((`m5_do`.`dostatus` = 2) or (`m5_do`.`dostatus` = 3) or (`m5_do`.`dostatus` = 4) or (`m5_do`.`dostatus` = 7))"

            'filter5 = strFilter
            'filter5 = filter5 & " AND ((`m5_dr`.`drstatus` = 2) or (`m5_dr`.`drstatus` = 3) or (`m5_dr`.`drstatus` = 4) or (`m5_dr`.`drstatus` = 7))"

            'filter6 = strFilter
            'filter6 = filter6 & " AND ((`m5_pi`.`pistatus` = 2) or (`m5_pi`.`pistatus` = 3) or (`m5_pi`.`pistatus` = 4) or (`m5_pi`.`pistatus` = 7))"

            'filter7 = strFilter
            'filter7 = filter7 & " AND ((`m5_si`.`sistatus` = 2) or (`m5_si`.`sistatus` = 3) or (`m5_si`.`sistatus` = 4) or (`m5_si`.`sistatus` = 7))"

            'filter8 = strFilter
            'filter8 = filter8 & " AND ((`m5_rnr`.`rnrstatus` = 2) or (`m5_rnr`.`rnrstatus` = 3) or (`m5_rnr`.`rnrstatus` = 4) or (`m5_rnr`.`rnrstatus` = 7))"

            'filter9 = strFilter
            'filter9 = filter9 & " AND ((`m5_sr`.`srstatus` = 2) or (`m5_sr`.`srstatus` = 3) or (`m5_sr`.`srstatus` = 4) or (`m5_sr`.`srstatus` = 7))"
        Else
            'Default filter
            'filter1 = "((`m4_pr`.`prstatus` = 2) or (`m4_pr`.`prstatus` = 3) or (`m4_pr`.`prstatus` = 4) or (`m4_pr`.`prstatus` = 7))"
            filter2 = "((`m5_so`.`sostatus` = 2) or (`m5_so`.`sostatus` = 3) or (`m5_so`.`sostatus` = 4) or (`m5_so`.`sostatus` = 7))"
            'filter3 = "((`m5_pl`.`plstatus` = 2) or (`m5_pl`.`plstatus` = 3) or (`m5_pl`.`plstatus` = 4) or (`m5_pl`.`plstatus` = 7))"
            'filter4 = "((`m5_do`.`dostatus` = 2) or (`m5_do`.`dostatus` = 3) or (`m5_do`.`dostatus` = 4) or (`m5_do`.`dostatus` = 7))"
            'filter5 = "((`m5_dr`.`drstatus` = 2) or (`m5_dr`.`drstatus` = 3) or (`m5_dr`.`drstatus` = 4) or (`m5_dr`.`drstatus` = 7))"
            'filter6 = "((`m5_pi`.`pistatus` = 2) or (`m5_pi`.`pistatus` = 3) or (`m5_pi`.`pistatus` = 4) or (`m5_pi`.`pistatus` = 7))"
            'filter7 = "((`m5_si`.`sistatus` = 2) or (`m5_si`.`sistatus` = 3) or (`m5_si`.`sistatus` = 4) or (`m5_si`.`sistatus` = 7))"
            'filter8 = "((`m5_rnr`.`rnrstatus` = 2) or (`m5_rnr`.`rnrstatus` = 3) or (`m5_rnr`.`rnrstatus` = 4) or (`m5_rnr`.`rnrstatus` = 7))"
            'filter9 = "((`m5_sr`.`srstatus` = 2) or (`m5_sr`.`srstatus` = 3) or (`m5_sr`.`srstatus` = 4) or (`m5_sr`.`srstatus` = 7))"
        End If

        'If Len(filter1) > 0 Then filter1 = " WHERE " & filter1
        If Len(filter2) > 0 Then filter2 = " WHERE " & filter2
        'If Len(filter3) > 0 Then filter3 = " WHERE " & filter3
        'If Len(filter4) > 0 Then filter4 = " WHERE " & filter4
        'If Len(filter5) > 0 Then filter5 = " WHERE " & filter5
        'If Len(filter6) > 0 Then filter6 = " WHERE " & filter6
        'If Len(filter7) > 0 Then filter7 = " WHERE " & filter7
        'If Len(filter8) > 0 Then filter8 = " WHERE " & filter8
        'If Len(filter9) > 0 Then filter9 = " WHERE " & filter9


        'sql = "select `sf`.`sfid` AS `sfid`,`sf`.`sfnotransaksi` AS `sfnotransaksi`,'PR' AS `sumber`,`m4_pr`.`prid` AS `idterkait`,`m4_pr`.`prnotransaksi` AS `noterkait`,`m4_pr`.`prtgl` AS `tglterkait`,`m4_pr`.`prinputtgl` AS `inputtglterkait`,`m4_pr`.`prmodifikasitgl` AS `modifikasitglterkait`, 0 as jenisterkait from (((`m5_sf_detail` `sfd` join `m5_sf` `sf` on((`sfd`.`idsf` = `sf`.`sfid`))) join `m4_pr_detail` on((`m4_pr_detail`.`idprdetail` = `sfd`.`idprdetail`))) join `m4_pr` on((`m4_pr_detail`.`idpr` = `m4_pr`.`prid`))) " & filter1 & "  group by `sf`.`sfid`, `m4_pr`.`prid` "
        'sql &= " UNION ALL "
        'sql &= "select `sf`.`sfid` AS `sfid`,`sf`.`sfnotransaksi` AS `sfnotransaksi`,'PR' AS `sumber`,`m4_pr`.`prid` AS `idterkait`,`m4_pr`.`prnotransaksi` AS `noterkait`,`m4_pr`.`prtgl` AS `tglterkait`,`m4_pr`.`prinputtgl` AS `inputtglterkait`,`m4_pr`.`prmodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_sf_detail` `sfd` join `m5_sf` `sf` on((`sfd`.`idsf` = `sf`.`sfid`))) join `m4_pr_detail` on((`m4_pr_detail`.`idsfdetail` = `sfd`.`idsfdetail`))) join `m4_pr` on((`m4_pr_detail`.`idpr` = `m4_pr`.`prid`))) " & filter1 & "  group by `sf`.`sfid`, `m4_pr`.`prid` "
        'sql &= " UNION ALL "
        sql = " select `sf`.`sfid` AS `sfid`,`sf`.`sfnotransaksi` AS `sfnotransaksi`,'SO' AS `sumber`,`m5_so`.`soid` AS `idterkait`,`m5_so`.`sonotransaksi` AS `noterkait`, `m5_so`.`sotgl` AS `tglterkait`,`m5_so`.`soinputtgl` AS `inputtglterkait`,`m5_so`.`somodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from `m5_sf` `sf` join `m5_so` on `sf`.`sfid` = `m5_so`.`socustomdbl1` " & filter2 & "  group by `sf`.`sfid`, `m5_so`.`soid`  "
        'sql &= " UNION ALL "
        'sql &= "select `sf`.`sfid` AS `sfid`,`sf`.`sfnotransaksi` AS `sfnotransaksi`,'PL' AS `sumber`,`m5_pl`.`plid` AS `idterkait`,`m5_pl`.`plnotransaksi` AS `noterkait`,`m5_pl`.`pltgl` AS `tglterkait`,`m5_pl`.`plinputtgl` AS `inputtglterkait`,`m5_pl`.`plmodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_sf_detail` `sfd` join `m5_sf` `sf` on((`sfd`.`idsf` = `sf`.`sfid`))) join `m5_pl_detail` on((`m5_pl_detail`.`idsfdetail` = `sfd`.`idsfdetail`))) join `m5_pl` on((`m5_pl_detail`.`idpl` = `m5_pl`.`plid`))) " & filter3 & "  group by `sf`.`sfid`, `m5_pl`.`plid` "
        'sql &= " UNION ALL "
        'sql &= "select `sf`.`sfid` AS `sfid`,`sf`.`sfnotransaksi` AS `sfnotransaksi`,'DO' AS `sumber`,`m5_do`.`doid` AS `idterkait`,`m5_do`.`donotransaksi` AS `noterkait`,`m5_do`.`dotgl` AS `tglterkait`,`m5_do`.`doinputtgl` AS `inputtglterkait`,`m5_do`.`domodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_sf_detail` `sfd` join `m5_sf` `sf` on((`sfd`.`idsf` = `sf`.`sfid`))) join `m5_do_detail` on((`m5_do_detail`.`idsfdetail` = `sfd`.`idsfdetail`))) join `m5_do` on((`m5_do_detail`.`iddo` = `m5_do`.`doid`))) " & filter4 & "  group by `sf`.`sfid`, `m5_do`.`doid` "
        'sql &= " UNION ALL "
        'sql &= "select `sf`.`sfid` AS `sfid`,`sf`.`sfnotransaksi` AS `sfnotransaksi`,'DR' AS `sumber`,`m5_dr`.`drid` AS `idterkait`,`m5_dr`.`drnotransaksi` AS `noterkait`,`m5_dr`.`drtgl` AS `tglterkait`,`m5_dr`.`drinputtgl` AS `inputtglterkait`,`m5_dr`.`drmodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_sf_detail` `sfd` join `m5_sf` `sf` on((`sfd`.`idsf` = `sf`.`sfid`))) join `m5_dr_detail` on((`m5_dr_detail`.`idsfdetail` = `sfd`.`idsfdetail`))) join `m5_dr` on((`m5_dr_detail`.`iddr` = `m5_dr`.`drid`))) " & filter5 & "  group by `sf`.`sfid`, `m5_dr`.`drid` "
        'sql &= " UNION ALL "
        'sql &= "select `sf`.`sfid` AS `sfid`,`sf`.`sfnotransaksi` AS `sfnotransaksi`,'PI' AS `sumber`,`m5_pi`.`piid` AS `idterkait`,`m5_pi`.`pinotransaksi` AS `noterkait`,`m5_pi`.`pitgl` AS `tglterkait`,`m5_pi`.`piinputtgl` AS `inputtglterkait`,`m5_pi`.`pimodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_sf_detail` `sfd` join `m5_sf` `sf` on((`sfd`.`idsf` = `sf`.`sfid`))) join `m5_pi_detail` on((`m5_pi_detail`.`idsfdetail` = `sfd`.`idsfdetail`))) join `m5_pi` on((`m5_pi_detail`.`idpi` = `m5_pi`.`piid`))) " & filter6 & "  group by `sf`.`sfid`, `m5_pi`.`piid` "
        'sql &= " UNION ALL "
        'sql &= "select `sf`.`sfid` AS `sfid`,`sf`.`sfnotransaksi` AS `sfnotransaksi`,'SI' AS `sumber`,`m5_si`.`siid` AS `idterkait`,`m5_si`.`sinotransaksi` AS `noterkait`,`m5_si`.`sitgl` AS `tglterkait`,`m5_si`.`siinputtgl` AS `inputtglterkait`,`m5_si`.`simodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_sf_detail` `sfd` join `m5_sf` `sf` on((`sfd`.`idsf` = `sf`.`sfid`))) join `m5_si_detail` on((`m5_si_detail`.`idsfdetail` = `sfd`.`idsfdetail`))) join `m5_si` on((`m5_si_detail`.`idsi` = `m5_si`.`siid`))) " & filter7 & "  group by `sf`.`sfid`, `m5_si`.`siid` "
        'sql &= " UNION ALL "
        'sql &= "select `sf`.`sfid` AS `sfid`,`sf`.`sfnotransaksi` AS `sfnotransaksi`,'RNR' AS `sumber`,`m5_rnr`.`rnrid` AS `idterkait`,`m5_rnr`.`rnrnotransaksi` AS `noterkait`,`m5_rnr`.`rnrtgl` AS `tglterkait`,`m5_rnr`.`rnrinputtgl` AS `inputtglterkait`,`m5_rnr`.`rnrmodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_sf_detail` `sfd` join `m5_sf` `sf` on((`sfd`.`idsf` = `sf`.`sfid`))) join `m5_rnr_detail` on((`m5_rnr_detail`.`idsfdetail` = `sfd`.`idsfdetail`))) join `m5_rnr` on((`m5_rnr_detail`.`idrnr` = `m5_rnr`.`rnrid`))) " & filter8 & "  group by `sf`.`sfid`, `m5_rnr`.`rnrid` "
        'sql &= " UNION ALL "
        'sql &= "select `sf`.`sfid` AS `sfid`,`sf`.`sfnotransaksi` AS `sfnotransaksi`,'SR' AS `sumber`,`m5_sr`.`srid` AS `idterkait`,`m5_sr`.`srnotransaksi` AS `noterkait`,`m5_sr`.`srtgl` AS `tglterkait`,`m5_sr`.`srinputtgl` AS `inputtglterkait`,`m5_sr`.`srmodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_sf_detail` `sfd` join `m5_sf` `sf` on((`sfd`.`idsf` = `sf`.`sfid`))) join `m5_sr_detail` on((`m5_sr_detail`.`idsfdetail` = `sfd`.`idsfdetail`))) join `m5_sr` on((`m5_sr_detail`.`idsr` = `m5_sr`.`srid`))) " & filter9 & "  group by `sf`.`sfid`, `m5_sr`.`srid`"

        Return sql
    End Function


End Class