Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_pl
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_PlSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataPack(), dataRowPack() As String

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
        'plid(0) As Integer, plcabang(1) As String, pllokasi(2) As String, plgudang(3) As String, plasalbarang(4) As String, 
        'plasalbarangkategori(5) As Integer, pljenispenjualan(6) As String, pljenispenjualankategori(7) As Integer, plcarabayar(8) As Integer, plsumber(9) As String, 
        'plautonotransaksi(10) As Integer, plnotransaksi(11) As String, pltgl(12) As Date, plkodepa(13) As Integer, plcustomer(14) As Integer, 
        'plcustomerkontak(15) As String, pl1alamat1(16) As String, pl1alamat2(17) As String, pl1alamat3(18) As String, pl2alamat1(19) As String, 
        'pl2alamat2(20) As String, pl2alamat3(21) As String, plbagianpenjualan(22) As Integer, plbagianpengepakan(23) As Integer, plekspedisi(24) As String, 
        'pltglkirim(25) As Date, pltermin(26) As String, pltgljatuhtempo(27) As Date, pluraian(28) As String, plcatatan(29) As String, 
        'plnoref(30) As String, pltglnoref(31) As Date, pltglpenutupan(32) As Date, plmatauang(33) As String, plkurs(34) As Double, 
        'plhargatermasukpajak(35) As Integer, pltotal(36) As Double, pldiskonpersen(37) As String, pljmldiskon(38) As Double, pltotalpajak1detail(39) As Double, 
        'pltotalpajak2detail(40) As Double, plbiayalainpersen(41) As Double, plbiayalain(42) As Double, pltotaltransaksi(43) As Double, plrekdiskon(44) As String, 
        'plrekpajak1(45) As String, plrekpajak2(46) As String, plrekbiayalain(47) As String, plidsq(48) As Integer, plidso(49) As Integer, 
        'plidpi(50) As Integer, plstatusdo(51) As Integer, plstatusdr(52) As Integer, plstatussi(53) As Integer, plstatusrnr(54) As Integer, 
        'plstatussr(55) As Integer, plstatus(56) As Integer, plstatussebelumnya(57) As Integer, pljmlrevisi(58) As Integer, plcetakanke(59) As Integer, 
        'plinputuser(60) As Integer, plinputtgl(61) As DateTime, plmodifikasiuser(62) As Integer, plmodifikasitgl(63) As DateTime, plisclose(64) As Integer, 
        'plcustomtext1(65) As String, plcustomtext2(66) As String, plcustomtext3(67) As String, plcustomtext4(68) As String, plcustomtext5(69) As String, 
        'plcustomint1(70) As Integer, plcustomint2(71) As Integer, plcustomint3(72) As Integer, plcustomdbl1(73) As Double, plcustomdbl2(74) As Double, 
        'plcustomdbl3(75) As Double, plcustomdate1(76) As Date, plcustomdate2(77) As Date, plcustomdate3(78) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'plid, plcabang, pllokasi, plgudang, plasalbarang, plasalbarangkategori, pljenispenjualan, 
        'pljenispenjualankategori, plcarabayar, plsumber, plautonotransaksi, plnotransaksi, pltgl, plkodepa, 
        'plcustomer, plcustomerkontak, pl1alamat1, pl1alamat2, pl1alamat3, pl2alamat1, pl2alamat2, 
        'pl2alamat3, plbagianpenjualan, plbagianpengepakan, plekspedisi, pltglkirim, pltermin, pltgljatuhtempo, 
        'pluraian, plcatatan, plnoref, pltglnoref, pltglpenutupan, plmatauang, plkurs, 
        'plhargatermasukpajak, pltotal, pldiskonpersen, pljmldiskon, pltotalpajak1detail, pltotalpajak2detail, plbiayalainpersen, 
        'plbiayalain, pltotaltransaksi, plrekdiskon, plrekpajak1, plrekpajak2, plrekbiayalain, plidsq, 
        'plidso, plidpi, plstatusdo, plstatusdr, plstatussi, plstatusrnr, plstatussr, 
        'plstatus, plstatussebelumnya, pljmlrevisi, plcetakanke, plinputuser, plinputtgl, plmodifikasiuser, 
        'plmodifikasitgl, plisclose, plcustomtext1, plcustomtext2, plcustomtext3, plcustomtext4, plcustomtext5, 
        'plcustomint1, plcustomint2, plcustomint3, plcustomdbl1, plcustomdbl2, plcustomdbl3, plcustomdate1, 
        'plcustomdate2, plcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 79) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'plid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "plid required numeric." : GoTo selesai
        End If
        'plasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "plasalbarangkategori required numeric." : GoTo selesai
        End If
        'pljenispenjualankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "pljenispenjualankategori required numeric." : GoTo selesai
        End If
        'plcarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "plcarabayar required numeric." : GoTo selesai
        End If
        'plautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "plautonotransaksi required numeric." : GoTo selesai
        End If
        'pltgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "pltgl required date." : GoTo selesai
        End If
        'plkodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "plkodepa required numeric." : GoTo selesai
        End If
        'plcustomer(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "plcustomer required numeric." : GoTo selesai
        End If
        'plbagianpenjualan(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "plbagianpenjualan required numeric." : GoTo selesai
        End If
        'plbagianpengepakan(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "plbagianpengepakan required numeric." : GoTo selesai
        End If
        'pltglkirim(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "pltglkirim required date." : GoTo selesai
        End If
        'pltgljatuhtempo(27) As Date
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "pltgljatuhtempo required date." : GoTo selesai
        End If
        'pltglnoref(31) As Date
        If (IsDate(dataUtama(31)) = False) Then
            result(2) = "pltglnoref required date." : GoTo selesai
        End If
        'pltglpenutupan(32) As Date
        If (IsDate(dataUtama(32)) = False) Then
            result(2) = "pltglpenutupan required date." : GoTo selesai
        End If
        'plkurs(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "plkurs required numeric." : GoTo selesai
        End If
        'plhargatermasukpajak(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "plhargatermasukpajak required numeric." : GoTo selesai
        End If
        'pltotal(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "pltotal required numeric." : GoTo selesai
        End If
        'pljmldiskon(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "pljmldiskon required numeric." : GoTo selesai
        End If
        'pltotalpajak1detail(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "pltotalpajak1detail required numeric." : GoTo selesai
        End If
        'pltotalpajak2detail(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "pltotalpajak2detail required numeric." : GoTo selesai
        End If
        ''plbiayalainpersen(41) As Double
        'If (IsNumeric(dataUtama(41)) = False) Then
        '    result(2) = "plbiayalainpersen required numeric." : GoTo selesai
        'End If
        'plbiayalain(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "plbiayalain required numeric." : GoTo selesai
        End If
        'pltotaltransaksi(43) As Double
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "pltotaltransaksi required numeric." : GoTo selesai
        End If
        'plidsq(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "plidsq required numeric." : GoTo selesai
        End If
        'plidso(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "plidso required numeric." : GoTo selesai
        End If
        'plidpi(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "plidpi required numeric." : GoTo selesai
        End If
        'plstatusdo(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "plstatusdo required numeric." : GoTo selesai
        End If
        'plstatusdr(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "plstatusdr required numeric." : GoTo selesai
        End If
        'plstatussi(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "plstatussi required numeric." : GoTo selesai
        End If
        'plstatusrnr(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "plstatusrnr required numeric." : GoTo selesai
        End If
        'plstatussr(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "plstatussr required numeric." : GoTo selesai
        End If
        'plstatus(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "plstatus required numeric." : GoTo selesai
        End If
        'plstatussebelumnya(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "plstatussebelumnya required numeric." : GoTo selesai
        End If
        'pljmlrevisi(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "pljmlrevisi required numeric." : GoTo selesai
        End If
        'plcetakanke(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "plcetakanke required numeric." : GoTo selesai
        End If
        'plinputuser(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "plinputuser required numeric." : GoTo selesai
        End If
        'plinputtgl(61) As DateTime
        If (IsDate(dataUtama(61)) = False) Then
            result(2) = "plinputtgl required date." : GoTo selesai
        End If
        'plmodifikasiuser(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "plmodifikasiuser required numeric." : GoTo selesai
        End If
        'plmodifikasitgl(63) As DateTime
        If (IsDate(dataUtama(63)) = False) Then
            result(2) = "plmodifikasitgl required date." : GoTo selesai
        End If
        'plisclose(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "plisclose required numeric." : GoTo selesai
        End If
        'plcustomint1(70) As Integer
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "plcustomint1 required numeric." : GoTo selesai
        End If
        'plcustomint2(71) As Integer
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "plcustomint2 required numeric." : GoTo selesai
        End If
        'plcustomint3(72) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "plcustomint3 required numeric." : GoTo selesai
        End If
        'plcustomdbl1(73) As Double
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "plcustomdbl1 required numeric." : GoTo selesai
        End If
        'plcustomdbl2(74) As Double
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "plcustomdbl2 required numeric." : GoTo selesai
        End If
        'plcustomdbl3(75) As Double
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "plcustomdbl3 required numeric." : GoTo selesai
        End If
        'plcustomdate1(76) As Date
        If (IsDate(dataUtama(76)) = False) Then
            result(2) = "plcustomdate1 required date." : GoTo selesai
        End If
        'plcustomdate2(77) As Date
        If (IsDate(dataUtama(77)) = False) Then
            result(2) = "plcustomdate2 required date." : GoTo selesai
        End If
        'plcustomdate3(78) As Date
        If (IsDate(dataUtama(78)) = False) Then
            result(2) = "plcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'plcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "plcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "plcabang should not be more than 25 character." : GoTo selesai
        End If

        'pllokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "pllokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "pllokasi should not be more than 25 character." : GoTo selesai
        End If

        'plgudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "plgudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "plgudang should not be more than 25 character." : GoTo selesai
        End If

        'plsumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "plsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "plsumber should not be more than 10 character." : GoTo selesai
        End If

        'plnotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "plnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "plnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'pltgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "pltgl can't be empty" : GoTo selesai
        End If

        'pltglkirim(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "pltglkirim can't be empty" : GoTo selesai
        End If

        'pltgljatuhtempo(27) As Date
        If Len(dataUtama(27)) = 0 Then
            result(2) = "pltgljatuhtempo can't be empty" : GoTo selesai
        End If

        'pltglnoref(31) As Date
        If Len(dataUtama(31)) = 0 Then
            result(2) = "pltglnoref can't be empty" : GoTo selesai
        End If

        'pltglpenutupan(32) As Date
        If Len(dataUtama(32)) = 0 Then
            result(2) = "pltglpenutupan can't be empty" : GoTo selesai
        End If

        'plmatauang(33) As String
        If Len(dataUtama(33)) = 0 Then
            result(2) = "plmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(33)) > 25 Then
            result(2) = "plmatauang should not be more than 25 character." : GoTo selesai
        End If

        'plkurs(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "plkurs can't be empty" : GoTo selesai
        End If

        'pltotal(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "pltotal can't be empty" : GoTo selesai
        End If

        'pldiskonpersen(37) As String
        If Len(dataUtama(37)) = 0 Then
            result(2) = "pldiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(37)) > 25 Then
            result(2) = "pldiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'pljmldiskon(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "pljmldiskon can't be empty" : GoTo selesai
        End If

        'pltotalpajak1detail(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "pltotalpajak1detail can't be empty" : GoTo selesai
        End If

        'pltotalpajak2detail(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "pltotalpajak2detail can't be empty" : GoTo selesai
        End If

        'plbiayalainpersen(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "plbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(41)) > 25 Then
            result(2) = "plbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'plbiayalain(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "plbiayalain can't be empty" : GoTo selesai
        End If

        'pltotaltransaksi(43) As Double
        If Len(dataUtama(43)) = 0 Then
            result(2) = "pltotaltransaksi can't be empty" : GoTo selesai
        End If

        'plinputtgl(61) As DateTime
        If Len(dataUtama(61)) = 0 Then
            result(2) = "plinputtgl can't be empty" : GoTo selesai
        End If

        'plmodifikasitgl(63) As DateTime
        If Len(dataUtama(63)) = 0 Then
            result(2) = "plmodifikasitgl can't be empty" : GoTo selesai
        End If

        'plcustomdbl1(73) As Double
        If Len(dataUtama(73)) = 0 Then
            result(2) = "plcustomdbl1 can't be empty" : GoTo selesai
        End If

        'plcustomdbl2(74) As Double
        If Len(dataUtama(74)) = 0 Then
            result(2) = "plcustomdbl2 can't be empty" : GoTo selesai
        End If

        'plcustomdbl3(75) As Double
        If Len(dataUtama(75)) = 0 Then
            result(2) = "plcustomdbl3 can't be empty" : GoTo selesai
        End If

        'plcustomdate1(76) As Date
        If Len(dataUtama(76)) = 0 Then
            result(2) = "plcustomdate1 can't be empty" : GoTo selesai
        End If

        'plcustomdate2(77) As Date
        If Len(dataUtama(77)) = 0 Then
            result(2) = "plcustomdate2 can't be empty" : GoTo selesai
        End If

        'plcustomdate3(78) As Date
        If Len(dataUtama(78)) = 0 Then
            result(2) = "plcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "plid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pllokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pljenispenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pljenispenjualankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pltgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plcustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plcustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pl1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pl1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pl1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pl2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pl2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pl2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plbagianpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plbagianpengepakan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plekspedisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pltglkirim", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pltermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pltgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pluraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pltglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pltglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plhargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pltotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pldiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pljmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pltotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pltotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pltotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plrekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plrekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plrekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plrekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plidsq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plidso", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plidpi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plstatusdo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plstatusdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plstatussi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plstatusrnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plstatussr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pljmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "plid~plcabang~pllokasi~plgudang~plasalbarang~plasalbarangkategori~pljenispenjualan~pljenispenjualankategori~plcarabayar~plsumber~plautonotransaksi~plnotransaksi~pltgl~plkodepa~plcustomer~plcustomerkontak~pl1alamat1~pl1alamat2~pl1alamat3~pl2alamat1~pl2alamat2~pl2alamat3~plbagianpenjualan~plbagianpengepakan~plekspedisi~pltglkirim~pltermin~pltgljatuhtempo~pluraian~plcatatan~plnoref~pltglnoref~pltglpenutupan~plmatauang~plkurs~plhargatermasukpajak~pltotal~pldiskonpersen~pljmldiskon~pltotalpajak1detail~pltotalpajak2detail~plbiayalainpersen~plbiayalain~pltotaltransaksi~plrekdiskon~plrekpajak1~plrekpajak2~plrekbiayalain~plidsq~plidso~plidpi~plstatusdo~plstatusdr~plstatussi~plstatusrnr~plstatussr~plstatus~plstatussebelumnya~pljmlrevisi~plcetakanke~plinputuser~plinputtgl~plmodifikasiuser~plmodifikasitgl~plisclose~plcustomtext1~plcustomtext2~plcustomtext3~plcustomtext4~plcustomtext5~plcustomint1~plcustomint2~plcustomint3~plcustomdbl1~plcustomdbl2~plcustomdbl3~plcustomdate1~plcustomdate2~plcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idpldetail(0) As Integer, idpl(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'nopack(5) As Integer, jml(6) As Double, satuan(7) As String, nilaisatuan(8) As Double, jmlbarang(9) As Double, 
        'satuanbarang(10) As String, matauang(11) As String, kurs(12) As Double, harga(13) As Double, diskon(14) As String, 
        'jmldiskon(15) As Double, pajak1(16) As String, jmlpajak1(17) As Double, pajak2(18) As String, jmlpajak2(19) As Double, 
        'cabang(20) As String, lokasi(21) As String, gudang(22) As String, costcenter(23) As String, divisi(24) As String, 
        'subdivisi(25) As String, proyek(26) As String, catatan(27) As String, urutan(28) As Integer, idsqdetail(29) As Integer, 
        'idsodetail(30) As Integer, idpidetail(31) As Integer, jmldo(32) As Double, statusdo(33) As Integer, jmldr(34) As Double, 
        'statusdr(35) As Integer, jmlsi(36) As Double, statussi(37) As Integer, jmlrnr(38) As Double, statusrnr(39) As Integer, 
        'jmlsr(40) As Double, statussr(41) As Integer, isclose(42) As Integer, customtext1(43) As String, customtext2(44) As String, 
        'customtext3(45) As String, customdbl1(46) As Double, customdbl2(47) As Double, customdbl3(48) As Double, customdate1(49) As Date, 
        'customdate2(50) As Date, customdate3(51) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idpldetail, idpl, idbarang, namabarang, tipebarang, nopack, jml, 
        'satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idsqdetail, idsodetail, idpidetail, jmldo, statusdo, jmldr, 
        'statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idpldetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nopack", AsEnumTypeData.AsString)
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
        AsDataTableTambahField(dtdetail, "idsqdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idsodetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idpidetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmldo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusdo", AsEnumTypeData.AsInt64)
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

        'Variabel ValidasiSimpan
        Dim ftExistOutstandingSO As String = "", ftOutstandingSO As String = "", updNilaiSO As String = "", updFilterSO As String = ""
        Dim ftExistOutstandingPI As String = "", ftOutstandingPI As String = "", updNilaiPI As String = "", updFilterPI As String = ""
        Dim idbarang As Integer = 0, idsodetail As Integer = 0, idpidetail As Integer = 0, jmlbarang As Double = 0

        'FILTER SO DAN PI, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftSO As String = "", ftPI As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 52) Then
                result(2) = "Detail Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idpldetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Detail Row : " & i & " - idpldetail required numeric." : GoTo selesai
            End If
            'idpl(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Detail Row : " & i & " - idpl required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Detail Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            ''nopack(5) As Integer
            'If (IsNumeric(dataRowDetail(5)) = False) Then
            '    result(2) = "Detail Row : " & i & " - nopack required numeric." : GoTo selesai
            'End If
            'jml(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Detail Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'nilaisatuan(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Detail Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarang(9) As Double
            'jmlbarang = jml * nilaisatuan
            dataRowDetail(9) = Double.Parse(dataRowDetail(6)) * Double.Parse(dataRowDetail(8))
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Detail Row : " & i & " - jmlbarang required numeric." : GoTo selesai
            End If
            'kurs(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Detail Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'harga(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Detail Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'jmldiskon(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Detail Row : " & i & " - jmldiskon required numeric." : GoTo selesai
            End If
            'jmlpajak1(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Detail Row : " & i & " - jmlpajak1 required numeric." : GoTo selesai
            End If
            'jmlpajak2(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Detail Row : " & i & " - jmlpajak2 required numeric." : GoTo selesai
            End If
            'urutan(28) As Integer
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Detail Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idsqdetail(29) As Integer
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Detail Row : " & i & " - idsqdetail required numeric." : GoTo selesai
            End If
            'idsodetail(30) As Integer
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Detail Row : " & i & " - idsodetail required numeric." : GoTo selesai
            End If
            'idpidetail(31) As Integer
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Detail Row : " & i & " - idpidetail required numeric." : GoTo selesai
            End If
            'jmldo(32) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Detail Row : " & i & " - jmldo required numeric." : GoTo selesai
            End If
            'statusdo(33) As Integer
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Detail Row : " & i & " - statusdo required numeric." : GoTo selesai
            End If
            'jmldr(34) As Double
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Detail Row : " & i & " - jmldr required numeric." : GoTo selesai
            End If
            'statusdr(35) As Integer
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Detail Row : " & i & " - statusdr required numeric." : GoTo selesai
            End If
            'jmlsi(36) As Double
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Detail Row : " & i & " - jmlsi required numeric." : GoTo selesai
            End If
            'statussi(37) As Integer
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Detail Row : " & i & " - statussi required numeric." : GoTo selesai
            End If
            'jmlrnr(38) As Double
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Detail Row : " & i & " - jmlrnr required numeric." : GoTo selesai
            End If
            'statusrnr(39) As Integer
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Detail Row : " & i & " - statusrnr required numeric." : GoTo selesai
            End If
            'jmlsr(40) As Double
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Detail Row : " & i & " - jmlsr required numeric." : GoTo selesai
            End If
            'statussr(41) As Integer
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Detail Row : " & i & " - statussr required numeric." : GoTo selesai
            End If
            'isclose(42) As Integer
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Detail Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(46) As Double
            If (IsNumeric(dataRowDetail(46)) = False) Then
                result(2) = "Detail Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(47) As Double
            If (IsNumeric(dataRowDetail(47)) = False) Then
                result(2) = "Detail Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(48) As Double
            If (IsNumeric(dataRowDetail(48)) = False) Then
                result(2) = "Detail Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(49) As Date
            If (IsDate(dataRowDetail(49)) = False) Then
                result(2) = "Detail Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(50) As Date
            If (IsDate(dataRowDetail(50)) = False) Then
                result(2) = "Detail Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(51) As Date
            If (IsDate(dataRowDetail(51)) = False) Then
                result(2) = "Detail Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'namabarang(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Detail Row : " & i & " - namabarang can't be empty" : GoTo selesai
            End If
            'If Len(dataRowDetail(3)) > 100 Then
            '    result(2) = "Detail Row : " & i & " - namabarang should not be more than 100 character." : GoTo selesai
            'End If

            'nopack(5) As String
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Detail Row : " & i & " - nopack can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(5)) > 25 Then
                result(2) = "Detail Row : " & i & " - nopack should not be more than 25 character." : GoTo selesai
            End If

            'jml(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Detail Row : " & i & " - jml can't be empty" : GoTo selesai
            End If
            If dataRowDetail(6) <= 0 Then
                result(2) = "Detail Row : " & i & " - jml can't be less than or equal to zero" : GoTo selesai
            End If

            'satuan(7) As String
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Detail Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(7)) > 25 Then
                result(2) = "Detail Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'nilaisatuan(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Detail Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmlbarang(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Detail Row : " & i & " - jmlbarang can't be empty" : GoTo selesai
            End If
            If dataRowDetail(9) <= 0 Then
                result(2) = "Detail Row : " & i & " - jmlbarang can't be less than or equal to zero" : GoTo selesai
            End If

            'satuanbarang(10) As String
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Detail Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(10)) > 25 Then
                result(2) = "Detail Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
            End If

            'matauang(11) As String
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Detail Row : " & i & " - matauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(11)) > 25 Then
                result(2) = "Detail Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Detail Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'harga(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Detail Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'diskon(14) As String
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Detail Row : " & i & " - diskon can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(14)) > 25 Then
                result(2) = "Detail Row : " & i & " - diskon should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskon(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Detail Row : " & i & " - jmldiskon can't be empty" : GoTo selesai
                'Else
                '    'HITUNG JMLDISKON : jml(6) As Double, harga(13) As Double, diskon(14) As String
                '    dataRowDetail(15) = F_Diskon(Double.Parse(dataRowDetail(6)), Double.Parse(dataRowDetail(13)), FixQuotes(dataRowDetail(14).ToString))
            End If

            'jmlpajak1(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Detail Row : " & i & " - jmlpajak1 can't be empty" : GoTo selesai
            End If

            'jmlpajak2(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Detail Row : " & i & " - jmlpajak2 can't be empty" : GoTo selesai
            End If

            'jmldo(32) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Detail Row : " & i & " - jmldo can't be empty" : GoTo selesai
            End If

            'jmldr(34) As Double
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Detail Row : " & i & " - jmldr can't be empty" : GoTo selesai
            End If

            'jmlsi(36) As Double
            If Len(dataRowDetail(36)) = 0 Then
                result(2) = "Detail Row : " & i & " - jmlsi can't be empty" : GoTo selesai
            End If

            'jmlrnr(38) As Double
            If Len(dataRowDetail(38)) = 0 Then
                result(2) = "Detail Row : " & i & " - jmlrnr can't be empty" : GoTo selesai
            End If

            'jmlsr(40) As Double
            If Len(dataRowDetail(40)) = 0 Then
                result(2) = "Detail Row : " & i & " - jmlsr can't be empty" : GoTo selesai
            End If

            'customdbl1(46) As Double
            If Len(dataRowDetail(46)) = 0 Then
                result(2) = "Detail Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(47) As Double
            If Len(dataRowDetail(47)) = 0 Then
                result(2) = "Detail Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(48) As Double
            If Len(dataRowDetail(48)) = 0 Then
                result(2) = "Detail Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(49) As Date
            If Len(dataRowDetail(49)) = 0 Then
                result(2) = "Detail Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(50) As Date
            If Len(dataRowDetail(50)) = 0 Then
                result(2) = "Detail Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(51) As Date
            If Len(dataRowDetail(51)) = 0 Then
                result(2) = "Detail Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idpldetail~idpl~idbarang~namabarang~tipebarang~nopack~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~idsqdetail~idsodetail~idpidetail~jmldo~statusdo~jmldr~statusdr~jmlsi~statussi~jmlrnr~statusrnr~jmlsr~statussr~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51)) = False Then
                result(2) = "Detail Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , idsodetail(30) As Integer      , idpidetail(31) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : idsodetail = dataRowDetail(30) : idpidetail = dataRowDetail(31)

            'VALIDASI OUTSTANDING -------------------------
            If idsodetail <> 0 Then 'SO
                'CEK SO YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftSO = IIf(Len(ftSO.ToString) = 0, "", ftSO & " OR ")
                ftSO = String.Concat(ftSO, " (sod.idsodetail = " & idsodetail & ") ")

                '1. CEK DATA EXIST ------------------------
                ftExistOutstandingSO = IIf(Len(ftExistOutstandingSO.ToString) = 0, "", ftExistOutstandingSO & " UNION ")
                'ftExistOutstandingSO = String.Concat(ftExistOutstandingSO, "SELECT EXISTS(SELECT 1 FROM m5_so_detail JOIN m5_so ON idso = soid WHERE idsodetail = '" & idsodetail & "' AND (sostatus = 2 OR sostatus = 3 OR sostatus = 4 OR sostatus = 7) LIMIT 1) as rowExists, '" & idsodetail & "' as idsodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
                'ftExistOutstandingSO = String.Concat(ftExistOutstandingSO, "SELECT EXISTS(SELECT 1 FROM m5_so_detail JOIN m5_so ON idso = soid WHERE idsodetail = '" & idsodetail & "' AND (sostatus = 2 OR sostatus = 3) LIMIT 1) as rowExists, '" & idsodetail & "' as idsodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
                ftExistOutstandingSO = String.Concat(ftExistOutstandingSO, "SELECT EXISTS(SELECT 1 FROM m5_so_detail JOIN m5_so ON idso = soid WHERE idsodetail = '" & idsodetail & "' AND (sostatus = 2 OR sostatus = 3 OR sostatus = 4 LIMIT 1) as rowExists, '" & idsodetail & "' as idsodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                If idpidetail = 0 Then
                    '2. CEK JML OUTSTANDING -------------------
                    Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsodetail=" & idsodetail)
                    ftOutstandingSO = IIf(Len(ftOutstandingSO.ToString) = 0, "", ftOutstandingSO & " OR ")
                    ftOutstandingSO = String.Concat(ftOutstandingSO, " (sod.idsodetail = " & idsodetail & " AND " & Outstanding & " > (sod.jmlbarang - sod.jmlrealisasi)) ")

                    '3. SET NILAI UPDATE OUTSTANDING ----------
                    updNilaiSO = String.Concat("WHEN '" & idsodetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiSO)

                    '4. SET FILTER UPDATE OUTSTANDING ---------
                    updFilterSO = IIf(Len(updFilterSO.ToString) = 0, "", updFilterSO & " OR ")
                    updFilterSO = String.Concat(updFilterSO, "(idsodetail = '" & idsodetail & "')")
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
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'MAPPING BUAT WS DATA PACK -------------------------------------------------------
        'idplpack(0) As Integer, idpl(1) As Integer, nopack(2) As Integer, catatan(3) As String, bentuk(4) As String, 
        'berat(5) As String, urutan(6) As Integer, customtext1(7) As String, customtext2(8) As String, customtext3(9) As String, 
        'customdbl1(10) As Double, customdbl2(11) As Double, customdbl3(12) As Double, customdate1(13) As Date, customdate2(14) As Date, 
        'customdate3(15) As Date

        'MAPPING BUAT FLEX DATA PACK -----------------------------------------------------
        'idplpack, idpl, nopack, catatan, bentuk, berat, urutan, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA PACK ======================================================
        'SPLIT PARAMETER DATA PACK
        dataPack = dataSplit(2).Split(sptRow)
        'END OF VALIDASI DAN SET DATA PACK ===============================================

        'Buat datatable pack
        Dim dtpack As New DataTable
        AsDataTableTambahField(dtpack, "idplpack", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "idpl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpack, "nopack", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "bentuk", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "berat", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpack, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "customdate3", AsEnumTypeData.AsString)

        'VALIDASI DAN SET DATA ROW PACK ==================================================
        Dim JmlDtPack As Integer = dataPack.Length
        For i = 1 To JmlDtPack
            'SPLIT DATA DETAIL
            dataRowPack = dataPack(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA PACK -----------------------------------
            'CEK ARRAY DATA PACK
            If (dataRowPack.Length <> 16) Then
                result(2) = "Pack Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW PACK ----------------------------

            'VALIDASI TIPE DATA PACK ------------------------------------------
            'idplpack(0) As Integer
            If (IsNumeric(dataRowPack(0)) = False) Then
                result(2) = "Pack Row : " & i & " - idplpack required numeric." : GoTo selesai
            End If
            'idpl(1) As Integer
            If (IsNumeric(dataRowPack(1)) = False) Then
                result(2) = "Pack Row : " & i & " - idpl required numeric." : GoTo selesai
            End If
            ''nopack(2) As Integer
            'If (IsNumeric(dataRowPack(2)) = False) Then
            '    result(2) = "Pack Row : " & i & " - nopack required numeric." : GoTo selesai
            'End If
            'urutan(6) As Integer
            If (IsNumeric(dataRowPack(6)) = False) Then
                result(2) = "Pack Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'customdbl1(10) As Double
            If (IsNumeric(dataRowPack(10)) = False) Then
                result(2) = "Pack Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(11) As Double
            If (IsNumeric(dataRowPack(11)) = False) Then
                result(2) = "Pack Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(12) As Double
            If (IsNumeric(dataRowPack(12)) = False) Then
                result(2) = "Pack Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(13) As Date
            If (IsDate(dataRowPack(13)) = False) Then
                result(2) = "Pack Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(14) As Date
            If (IsDate(dataRowPack(14)) = False) Then
                result(2) = "Pack Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(15) As Date
            If (IsDate(dataRowPack(15)) = False) Then
                result(2) = "Pack Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA PACK -----------------------------------

            'VALIDASI DATA PACK ---------------------------------------
            'nopack(2) As String
            If Len(dataRowPack(2)) = 0 Then
                result(2) = "Pack Row : " & i & " - nopack can't be empty" : GoTo selesai
            End If
            If Len(dataRowPack(2)) > 25 Then
                result(2) = "Pack Row : " & i & " - nopack should not be more than 25 character." : GoTo selesai
            End If

            'customdbl1(10) As Double
            If Len(dataRowPack(10)) = 0 Then
                result(2) = "Pack Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(11) As Double
            If Len(dataRowPack(11)) = 0 Then
                result(2) = "Pack Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(12) As Double
            If Len(dataRowPack(12)) = 0 Then
                result(2) = "Pack Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(13) As Date
            If Len(dataRowPack(13)) = 0 Then
                result(2) = "Pack Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(14) As Date
            If Len(dataRowPack(14)) = 0 Then
                result(2) = "Pack Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(15) As Date
            If Len(dataRowPack(15)) = 0 Then
                result(2) = "Pack Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA PACK --------------------------------

            If AsDataTableTambahData(dtpack, "idplpack~idpl~nopack~catatan~bentuk~berat~urutan~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowPack(0) & "~" & dataRowPack(1) & "~" & dataRowPack(2) & "~" & dataRowPack(3) & "~" & dataRowPack(4) & "~" & dataRowPack(5) & "~" & dataRowPack(6) & "~" & dataRowPack(7) & "~" & dataRowPack(8) & "~" & dataRowPack(9) & "~" & dataRowPack(10) & "~" & dataRowPack(11) & "~" & dataRowPack(12) & "~" & dataRowPack(13) & "~" & dataRowPack(14) & "~" & dataRowPack(15)) = False Then
                result(2) = "Pack Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

        Next
        'END OF VALIDASI DAN SET ROW DATA PACK ===========================================


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
                Dim vModuleId As Integer = 5, vMenuId As Integer = 6
                Select Case drutama("plstatus")
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("pltgl")), AsFormatTanggal(drutama("pltgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("plstatus") = 2 Or drutama("plstatus") = 1 Or drutama("plstatus") = 8 Or drutama("plstatus") = 9 Or drutama("plstatus") = 10 Or drutama("plstatus") = 11 Then
                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstandingSO, ftOutstandingSO, ftExistOutstandingPI, ftOutstandingPI, ftSO, ftPI, drutama("plhargatermasukpajak"))
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("pltermin").ToString, AsFormatTanggal(drutama("pltgl")), "pltgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("pltgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                ''PERHITUNGAN TOTAL UTAMA ================================
                ''DIAMBILKAN DARI DATA DETAIL

                ''TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                ''SUBTOTAL = (jml * harga) - jmldiskon
                'AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                'dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                ''TOTAL = subtotal
                'drutama("pltotal") = AsDataTableDSum(dtdetail, "subtotal")

                ''TOTALPAJAK1 = jmlpajak1
                'drutama("pltotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                ''TOTALPAJAK2 = jmlpajak2
                'drutama("pltotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                ''JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                ''JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'If Integer.Parse(drutama("plhargatermasukpajak")) = 0 Then
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                '    drutama("pltotaltransaksi") = Double.Parse(drutama("pltotal")) - Double.Parse(drutama("pljmldiskon")) + Double.Parse(drutama("pltotalpajak1detail")) + Double.Parse(drutama("pltotalpajak2detail")) + Double.Parse(drutama("plbiayalain"))

                'Else
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                '    drutama("pltotaltransaksi") = Double.Parse(drutama("pltotal")) - Double.Parse(drutama("pljmldiskon")) + Double.Parse(drutama("plbiayalain"))

                'End If
                ''END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("plid")
                    notransaksi = drutama("plnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(plid), plnotransaksi FROM M5_pl WHERE plid='" & result(4) & "' AND plstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("plautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("plcabang"), drutama("pllokasi"), drutama("plsumber"), drutama("pltgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(plid) FROM m5_pl WHERE plnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_pl_history
                        Dim rsSimpanHistory As String = SimpanHistory.M5_Pl_HistorySimpan("" & paramSplit(0) & "★M5_Pl_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("plsumber")) & "▼" & FixQuotes(drutama("plid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Pl set plcabang  = '" & FixQuotes(drutama("plcabang")) & "', pllokasi  = '" & FixQuotes(drutama("pllokasi")) & "', plgudang  = '" & FixQuotes(drutama("plgudang")) & "', plasalbarang  = '" & FixQuotes(drutama("plasalbarang")) & "', plasalbarangkategori  = " & drutama("plasalbarangkategori") & ", pljenispenjualan  = '" & FixQuotes(drutama("pljenispenjualan")) & "', pljenispenjualankategori  = " & drutama("pljenispenjualankategori") & ", plcarabayar  = " & drutama("plcarabayar") & ", plsumber  = '" & FixQuotes(drutama("plsumber")) & "', plautonotransaksi  = " & drutama("plautonotransaksi") & ", plnotransaksi  = '" & FixQuotes(notransaksi) & "', pltgl  = '" & FixQuotes(AsFormatTanggal(drutama("pltgl"))) & "', plkodepa  = " & drutama("plkodepa") & ", plcustomer  = " & drutama("plcustomer") & ", plcustomerkontak  = '" & FixQuotes(drutama("plcustomerkontak")) & "', pl1alamat1  = '" & FixQuotes(drutama("pl1alamat1")) & "', pl1alamat2  = '" & FixQuotes(drutama("pl1alamat2")) & "', pl1alamat3  = '" & FixQuotes(drutama("pl1alamat3")) & "', pl2alamat1  = '" & FixQuotes(drutama("pl2alamat1")) & "', pl2alamat2  = '" & FixQuotes(drutama("pl2alamat2")) & "', pl2alamat3  = '" & FixQuotes(drutama("pl2alamat3")) & "', plbagianpenjualan  = " & drutama("plbagianpenjualan") & ", plbagianpengepakan  = " & drutama("plbagianpengepakan") & ", plekspedisi  = '" & FixQuotes(drutama("plekspedisi")) & "', pltglkirim  = '" & FixQuotes(AsFormatTanggal(drutama("pltglkirim"))) & "', pltermin  = '" & FixQuotes(drutama("pltermin")) & "', pltgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("pltgljatuhtempo"))) & "', pluraian  = '" & FixQuotes(drutama("pluraian")) & "', plcatatan  = '" & FixQuotes(drutama("plcatatan")) & "', plnoref  = '" & FixQuotes(drutama("plnoref")) & "', pltglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("pltglnoref"))) & "', pltglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("pltglpenutupan"))) & "', plmatauang  = '" & FixQuotes(drutama("plmatauang")) & "', plkurs  = '" & FixDouble(drutama("plkurs")) & "', plhargatermasukpajak  = " & drutama("plhargatermasukpajak") & ", pltotal  = '" & FixDouble(drutama("pltotal")) & "', pldiskonpersen  = '" & FixQuotes(drutama("pldiskonpersen")) & "', pljmldiskon  = '" & FixDouble(drutama("pljmldiskon")) & "', pltotalpajak1detail  = '" & FixDouble(drutama("pltotalpajak1detail")) & "', pltotalpajak2detail  = '" & FixDouble(drutama("pltotalpajak2detail")) & "', plbiayalainpersen  = '" & FixDouble(drutama("plbiayalainpersen")) & "', plbiayalain  = '" & FixDouble(drutama("plbiayalain")) & "', pltotaltransaksi  = '" & FixDouble(drutama("pltotaltransaksi")) & "', plrekdiskon  = '" & FixQuotes(drutama("plrekdiskon")) & "', plrekpajak1  = '" & FixQuotes(drutama("plrekpajak1")) & "', plrekpajak2  = '" & FixQuotes(drutama("plrekpajak2")) & "', plrekbiayalain  = '" & FixQuotes(drutama("plrekbiayalain")) & "', plidsq  = " & drutama("plidsq") & ", plidso  = " & drutama("plidso") & ", plidpi  = " & drutama("plidpi") & ", plstatusdo  = " & drutama("plstatusdo") & ", plstatusdr  = " & drutama("plstatusdr") & ", plstatussi  = " & drutama("plstatussi") & ", plstatusrnr  = " & drutama("plstatusrnr") & ", plstatussr  = " & drutama("plstatussr") & ", plstatus  = " & drutama("plstatus") & ", plstatussebelumnya  = " & drutama("plstatussebelumnya") & ", pljmlrevisi  = pljmlrevisi+1, plcetakanke  = " & drutama("plcetakanke") & ", plmodifikasiuser  = " & drutama("plmodifikasiuser") & ", plmodifikasitgl  = NOW(), plcustomtext1  = '" & FixQuotes(drutama("plcustomtext1")) & "', plcustomtext2  = '" & FixQuotes(drutama("plcustomtext2")) & "', plcustomtext3  = '" & FixQuotes(drutama("plcustomtext3")) & "', plcustomtext4  = '" & FixQuotes(drutama("plcustomtext4")) & "', plcustomtext5  = '" & FixQuotes(drutama("plcustomtext5")) & "', plcustomint1  = " & drutama("plcustomint1") & ", plcustomint2  = " & drutama("plcustomint2") & ", plcustomint3  = " & drutama("plcustomint3") & ", plcustomdbl1  = '" & FixDouble(drutama("plcustomdbl1")) & "', plcustomdbl2  = '" & FixDouble(drutama("plcustomdbl2")) & "', plcustomdbl3  = '" & FixDouble(drutama("plcustomdbl3")) & "', plcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("plcustomdate1"))) & "', plcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("plcustomdate2"))) & "', plcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("plcustomdate3"))) & "' where plid = '" & drutama("plid") & "'"
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

                    If drutama("plautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("plcabang"), drutama("pllokasi"), drutama("plsumber"), drutama("pltgl"))
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
                        notransaksi = drutama("plnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(plid) FROM m5_pl WHERE plnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Pl (plcabang, pllokasi, plgudang, plasalbarang, plasalbarangkategori, pljenispenjualan, pljenispenjualankategori, plcarabayar, plsumber, plautonotransaksi, plnotransaksi, pltgl, plkodepa, plcustomer, plcustomerkontak, pl1alamat1, pl1alamat2, pl1alamat3, pl2alamat1, pl2alamat2, pl2alamat3, plbagianpenjualan, plbagianpengepakan, plekspedisi, pltglkirim, pltermin, pltgljatuhtempo, pluraian, plcatatan, plnoref, pltglnoref, pltglpenutupan, plmatauang, plkurs, plhargatermasukpajak, pltotal, pldiskonpersen, pljmldiskon, pltotalpajak1detail, pltotalpajak2detail, plbiayalainpersen, plbiayalain, pltotaltransaksi, plrekdiskon, plrekpajak1, plrekpajak2, plrekbiayalain, plidsq, plidso, plidpi, plstatusdo, plstatusdr, plstatussi, plstatusrnr, plstatussr, plstatus, plstatussebelumnya, pljmlrevisi, plcetakanke, plinputuser, plinputtgl, plmodifikasiuser, plmodifikasitgl, plisclose, plcustomtext1, plcustomtext2, plcustomtext3, plcustomtext4, plcustomtext5, plcustomint1, plcustomint2, plcustomint3, plcustomdbl1, plcustomdbl2, plcustomdbl3, plcustomdate1, plcustomdate2, plcustomdate3) values('" & FixQuotes(drutama("plcabang")) & "', '" & FixQuotes(drutama("pllokasi")) & "', '" & FixQuotes(drutama("plgudang")) & "', '" & FixQuotes(drutama("plasalbarang")) & "', " & drutama("plasalbarangkategori") & ", '" & FixQuotes(drutama("pljenispenjualan")) & "', " & drutama("pljenispenjualankategori") & ", " & drutama("plcarabayar") & ", '" & FixQuotes(drutama("plsumber")) & "', " & drutama("plautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("pltgl"))) & "', " & drutama("plkodepa") & ", " & drutama("plcustomer") & ", '" & FixQuotes(drutama("plcustomerkontak")) & "', '" & FixQuotes(drutama("pl1alamat1")) & "', '" & FixQuotes(drutama("pl1alamat2")) & "', '" & FixQuotes(drutama("pl1alamat3")) & "', '" & FixQuotes(drutama("pl2alamat1")) & "', '" & FixQuotes(drutama("pl2alamat2")) & "', '" & FixQuotes(drutama("pl2alamat3")) & "', " & drutama("plbagianpenjualan") & ", " & drutama("plbagianpengepakan") & ", '" & FixQuotes(drutama("plekspedisi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pltglkirim"))) & "', '" & FixQuotes(drutama("pltermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pltgljatuhtempo"))) & "', '" & FixQuotes(drutama("pluraian")) & "', '" & FixQuotes(drutama("plcatatan")) & "', '" & FixQuotes(drutama("plnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pltglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("pltglpenutupan"))) & "', '" & FixQuotes(drutama("plmatauang")) & "', '" & FixDouble(drutama("plkurs")) & "', " & drutama("plhargatermasukpajak") & ", '" & FixDouble(drutama("pltotal")) & "', '" & FixQuotes(drutama("pldiskonpersen")) & "', '" & FixDouble(drutama("pljmldiskon")) & "', '" & FixDouble(drutama("pltotalpajak1detail")) & "', '" & FixDouble(drutama("pltotalpajak2detail")) & "', '" & FixDouble(drutama("plbiayalainpersen")) & "', '" & FixDouble(drutama("plbiayalain")) & "', '" & FixDouble(drutama("pltotaltransaksi")) & "', '" & FixQuotes(drutama("plrekdiskon")) & "', '" & FixQuotes(drutama("plrekpajak1")) & "', '" & FixQuotes(drutama("plrekpajak2")) & "', '" & FixQuotes(drutama("plrekbiayalain")) & "', " & drutama("plidsq") & ", " & drutama("plidso") & ", " & drutama("plidpi") & ", " & drutama("plstatusdo") & ", " & drutama("plstatusdr") & ", " & drutama("plstatussi") & ", " & drutama("plstatusrnr") & ", " & drutama("plstatussr") & ", " & drutama("plstatus") & ", " & drutama("plstatussebelumnya") & ", " & drutama("pljmlrevisi") & ", " & drutama("plcetakanke") & ", " & drutama("plinputuser") & ", NOW(), " & drutama("plmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("plisclose") & ", '" & FixQuotes(drutama("plcustomtext1")) & "', '" & FixQuotes(drutama("plcustomtext2")) & "', '" & FixQuotes(drutama("plcustomtext3")) & "', '" & FixQuotes(drutama("plcustomtext4")) & "', '" & FixQuotes(drutama("plcustomtext5")) & "', " & drutama("plcustomint1") & ", " & drutama("plcustomint2") & ", " & drutama("plcustomint3") & ", '" & FixDouble(drutama("plcustomdbl1")) & "', '" & FixDouble(drutama("plcustomdbl2")) & "', '" & FixDouble(drutama("plcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("plcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("plcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("plcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select plid from M5_pl where plnotransaksi='" & notransaksi & "' AND plinputuser= '" & userid & "' order by plmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Pl_Detail where idpl = '" & result(4) & "'"
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
                        If Not drutama("plmatauang").ToString.Equals(dr1("matauang").ToString) Then
                            result(2) = "Row : " & dr1("urutan") & " - " & dr1("tipebarang") & " | " & dr1("namabarang") & " currency (" & dr1("matauang") & ") doesn't belong to the main transactions." : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF VALIDASI MATAUANG HARUS SAMA DENGAN TRANSAKSI UTAMA -------------


                        'SET HARGA DARI TRANSAKSI SEBELUMNYA ------------------------------------
                        If Double.Parse(dr1("idpidetail")) > 0 Then
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
                        strValue2.Append("(" & dr1("idpldetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & dr1("nopack") & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idsqdetail") & ", " & dr1("idsodetail") & ", " & dr1("idpidetail") & ", '" & FixDouble(dr1("jmldo")) & "', " & dr1("statusdo") & ", '" & FixDouble(dr1("jmldr")) & "', " & dr1("statusdr") & ", '" & FixDouble(dr1("jmlsi")) & "', " & dr1("statussi") & ", '" & FixDouble(dr1("jmlrnr")) & "', " & dr1("statusrnr") & ", '" & FixDouble(dr1("jmlsr")) & "', " & dr1("statussr") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M5_Pl_Detail(idpldetail, idpl, idbarang, namabarang, tipebarang, nopack, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpidetail, jmldo, statusdo, jmldr, statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                'Hapus pack ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Pl_Pack where idpl = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses pack
                If (dtpack.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtpack.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idplpack") & ", " & result(4) & ", '" & dr1("nopack") & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("bentuk")) & "', '" & FixQuotes(dr1("berat")) & "', " & dr1("urutan") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M5_Pl_Pack(idplpack, idpl, nopack, catatan, bentuk, berat, urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Else
                    result(2) = "Pack Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'UPDATE OUTSTANDING TRANSAKSI ==========================================================
                If drutama("plstatus") = 2 Then
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
                End If
                'END OF UPDATE OUTSTANDING TRANSAKSI ================================================

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "PL", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M5_PlUpdateStatus(ByVal param As String) As String

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
            Dim sumber As String = "Pl", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Pltgl, Plnotransaksi, Plstatus FROM M5_Pl WHERE Plid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Plstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m5_pl_history
            Dim rsSimpanHistory As String = SimpanHistory.M5_Pl_HistorySimpan("" & paramSplit(0) & "★M5_Pl_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m5_pl_terkait("plid = '" & idtransaksi & "'")
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idsodetail As Integer = 0, idpidetail As Integer = 0
                Dim updNilaiSO As String = "", updFilterSO As String = "", updNilaiPI As String = "", updFilterPI As String = ""
                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idsodetail, idpidetail, urutan FROM m5_pl_detail WHERE idpl = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : idsodetail = dr1("idsodetail") : idpidetail = dr1("idpidetail")

                        'UPDATE OUTSTANDING ---------------------------
                        If idsodetail <> 0 Then
                            If idpidetail = 0 Then
                                '1. SET NILAI UPDATE OUTSTANDING SO
                                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsodetail=" & idsodetail)
                                updNilaiSO = String.Concat("WHEN '" & idsodetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiSO)

                                '2. SET FILTERUPDATE OUTSTANDING SO
                                updFilterSO = IIf(Len(updFilterSO.ToString) = 0, "", updFilterSO & " OR ")
                                updFilterSO = String.Concat(updFilterSO, "(idsodetail = '" & idsodetail & "')")
                            End If
                        End If

                        If idpidetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING PI
                            Dim Outstandingpi As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idpidetail=" & idpidetail)
                            updNilaiPI = String.Concat("WHEN '" & idpidetail & "' THEN ROUND(jmlrealisasi - '" & Outstandingpi & "', 5) ", updNilaiPI)

                            '2. SET FILTERUPDATE OUTSTANDING PI
                            updFilterPI = IIf(Len(updFilterPI.ToString) = 0, "", updFilterPI & " OR ")
                            updFilterPI = String.Concat(updFilterPI, "(idpidetail = '" & idpidetail & "')")
                        End If
                        'END OF BUAT FILTER UNTUK UPDATE --------------------------
                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                'UPDATE OUTSTANDING TRANSAKSI ====================================================
                If Len(updFilterSO) > 0 Then 'SO
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
                End If

                If Len(updFilterPI) > 0 Then 'PI
                    'UPDATE OUTSTANDING DETAIL -------------------
                    sql = "UPDATE m5_pi_detail SET jmlrealisasi = (CASE idpidetail " & updNilaiPI & " ELSE jmlrealisasi END) WHERE " & updFilterPI
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
                'END OF UPDATE OUTSTANDING TRANSAKSI =============================================

            End If

            'update status utama
            sql = "UPDATE M5_Pl SET Plstatus = " & nilaiStatus & ", Plmodifikasiuser='" & userid & "', Plmodifikasitgl = NOW(), Plposting = 0, Plpostingtgl = '1971-01-01 00:00:00', Pljmlrevisi = Pljmlrevisi + 1 WHERE Plid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_PlSearch(PostWsSearch(paramSplit(0), "M5_PlSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M5_PlDelete(ByVal param As String) As String

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
            Dim sumber As String = "Pl", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Plid, Plnotransaksi FROM M5_Pl WHERE Plid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT plcabang, pllokasi, plsumber, plautonotransaksi, plnotransaksi, pltgl"
            sql &= " FROM M5_pl"
            sql &= " WHERE plid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("plcabang")
                lokasi = dtNomorNext.Rows(0)("pllokasi")
                sumber = dtNomorNext.Rows(0)("plsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("plautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("plnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("pltgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE PACK
            sql = "DELETE FROM M5_Pl_Pack WHERE idpl ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M5_Pl_Detail WHERE idpl ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M5_Pl WHERE plid ='" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_PlSearch(PostWsSearch(paramSplit(0), "M5_PlSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M5_PlGetdataById(ByVal param As String) As String
        'M5_PlGetdataById Utama --------------------------------------------------------
        'plid, plcabang, pllokasi, plgudang, plasalbarang, plasalbarangkategori, pljenispenjualan, 
        'pljenispenjualankategori, plcarabayar, plsumber, plautonotransaksi, plnotransaksi, pltgl, plkodepa, 
        'plcustomer, plcustomerkontak, pl1alamat1, pl1alamat2, pl1alamat3, pl2alamat1, pl2alamat2, 
        'pl2alamat3, plbagianpenjualan, plbagianpengepakan, plekspedisi, pltglkirim, pltermin, pltgljatuhtempo, 
        'pluraian, plcatatan, plnoref, pltglnoref, pltglpenutupan, plmatauang, plkurs, 
        'plhargatermasukpajak, pltotal, pldiskonpersen, pljmldiskon, pltotalpajak1detail, pltotalpajak2detail, plbiayalainpersen, 
        'plbiayalain, pltotaltransaksi, plrekdiskon, plrekpajak1, plrekpajak2, plrekbiayalain, plidsq, 
        'plidso, plidpi, plstatusdo, plstatusdr, plstatussi, plstatusrnr, plstatussr, 
        'plstatusrealisasi, plstatus, plstatussebelumnya, pljmlrevisi, plcetakanke, plinputuser, plinputtgl, 
        'plmodifikasiuser, plmodifikasitgl, plposting, plpostingtgl, plisclose, plcustomtext1, plcustomtext2, 
        'plcustomtext3, plcustomtext4, plcustomtext5, plcustomint1, plcustomint2, plcustomint3, plcustomdbl1, 
        'plcustomdbl2, plcustomdbl3, plcustomdate1, plcustomdate2, plcustomdate3, plcabangnama, pllokasinama, 
        'plgudangnama, plcustomerkode, plcustomernama, plbagianpenjualankode, plbagianpenjualannama, plbagianpengepakankode, plbagianpengepakannama, 
        'plekspedisinama, plterminnama, plterminharijatuhtempo, plrekdiskonnama, plrekpajak1nama, plrekpajak2nama, plrekbiayalainnama, 
        'plnotransaksisq, plnotransaksiso, plnotransaksipi, plstatusnama, plstatussebelumnyanama, 
        'plinputusernama, plmodifikasiusernama, ktingkatjual, kpkp

        'M5_PlGetdataById Detail --------------------------------------------------------
        'idpldetail, idpl, idbarang, namabarang, tipebarang, nopack, jml, 
        'satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idsqdetail, idsodetail, idpidetail, jmldo, statusdo, jmldr, 
        'statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, 
        'jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, pajak1nama, 
        'pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, 
        'divisinama, subdivisinama, proyeknama, sqnotransaksi, sonotransaksi, pinotransaksi

        'M5_PlGetdataById Pack --------------------------------------------------------
        'idplpack, idpl, nopack, catatan, bentuk, berat, 
        'urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3

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

        Dim utama As String = "", detail As String = "", pack As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M5_pl~M5_pl_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "plid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "plid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_pl_getdata")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("plid"), 0), sptField,
                     FxDB(drutama("plcabang"), ""), sptField,
                     FxDB(drutama("pllokasi"), ""), sptField,
                     FxDB(drutama("plgudang"), ""), sptField,
                     FxDB(drutama("plasalbarang"), ""), sptField,
                     FxDB(drutama("plasalbarangkategori"), 0), sptField,
                     FxDB(drutama("pljenispenjualan"), ""), sptField,
                     FxDB(drutama("pljenispenjualankategori"), 0), sptField,
                     FxDB(drutama("plcarabayar"), 0), sptField,
                     FxDB(drutama("plsumber"), ""), sptField,
                     FxDB(drutama("plautonotransaksi"), 0), sptField,
                     FxDB(drutama("plnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pltgl"), ""), formatTgl), sptField,
                     FxDB(drutama("plkodepa"), 0), sptField,
                     FxDB(drutama("plcustomer"), 0), sptField,
                     FxDB(drutama("plcustomerkontak"), ""), sptField,
                     FxDB(drutama("pl1alamat1"), ""), sptField,
                     FxDB(drutama("pl1alamat2"), ""), sptField,
                     FxDB(drutama("pl1alamat3"), ""), sptField,
                     FxDB(drutama("pl2alamat1"), ""), sptField,
                     FxDB(drutama("pl2alamat2"), ""), sptField,
                     FxDB(drutama("pl2alamat3"), ""), sptField,
                     FxDB(drutama("plbagianpenjualan"), 0), sptField,
                     FxDB(drutama("plbagianpengepakan"), 0), sptField,
                     FxDB(drutama("plekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pltglkirim"), ""), formatTgl), sptField,
                     FxDB(drutama("pltermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pltgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("pluraian"), ""), sptField,
                     FxDB(drutama("plcatatan"), ""), sptField,
                     FxDB(drutama("plnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pltglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pltglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("plmatauang"), ""), sptField,
                     FxDB(drutama("plkurs"), 0), sptField,
                     FxDB(drutama("plhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("pltotal"), 0), sptField,
                     FxDB(drutama("pldiskonpersen"), ""), sptField,
                     FxDB(drutama("pljmldiskon"), 0), sptField,
                     FxDB(drutama("pltotalpajak1detail"), 0), sptField,
                     FxDB(drutama("pltotalpajak2detail"), 0), sptField,
                     FxDB(drutama("plbiayalainpersen"), 0), sptField,
                     FxDB(drutama("plbiayalain"), 0), sptField,
                     FxDB(drutama("pltotaltransaksi"), 0), sptField,
                     FxDB(drutama("plrekdiskon"), ""), sptField,
                     FxDB(drutama("plrekpajak1"), ""), sptField,
                     FxDB(drutama("plrekpajak2"), ""), sptField,
                     FxDB(drutama("plrekbiayalain"), ""), sptField,
                     FxDB(drutama("plidsq"), 0), sptField,
                     FxDB(drutama("plidso"), 0), sptField,
                     FxDB(drutama("plidpi"), 0), sptField,
                     FxDB(drutama("plstatusdo"), 0), sptField,
                     FxDB(drutama("plstatusdr"), 0), sptField,
                     FxDB(drutama("plstatussi"), 0), sptField,
                     FxDB(drutama("plstatusrnr"), 0), sptField,
                     FxDB(drutama("plstatussr"), 0), sptField,
                     FxDB(drutama("plstatusrealisasi"), 0), sptField,
                     FxDB(drutama("plstatus"), 0), sptField,
                     FxDB(drutama("plstatussebelumnya"), 0), sptField,
                     FxDB(drutama("pljmlrevisi"), 0), sptField,
                     FxDB(drutama("plcetakanke"), 0), sptField,
                     FxDB(drutama("plinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("plinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("plmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("plmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("plposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("plpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("plisclose"), 0), sptField,
                     FxDB(drutama("plcustomtext1"), ""), sptField,
                     FxDB(drutama("plcustomtext2"), ""), sptField,
                     FxDB(drutama("plcustomtext3"), ""), sptField,
                     FxDB(drutama("plcustomtext4"), ""), sptField,
                     FxDB(drutama("plcustomtext5"), ""), sptField,
                     FxDB(drutama("plcustomint1"), 0), sptField,
                     FxDB(drutama("plcustomint2"), 0), sptField,
                     FxDB(drutama("plcustomint3"), 0), sptField,
                     FxDB(drutama("plcustomdbl1"), 0), sptField,
                     FxDB(drutama("plcustomdbl2"), 0), sptField,
                     FxDB(drutama("plcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("plcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("plcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("plcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("plcabangnama"), ""), sptField,
                     FxDB(drutama("pllokasinama"), ""), sptField,
                     FxDB(drutama("plgudangnama"), ""), sptField,
                     FxDB(drutama("plcustomerkode"), ""), sptField,
                     FxDB(drutama("plcustomernama"), ""), sptField,
                     FxDB(drutama("plbagianpenjualankode"), ""), sptField,
                     FxDB(drutama("plbagianpenjualannama"), ""), sptField,
                     FxDB(drutama("plbagianpengepakankode"), ""), sptField,
                     FxDB(drutama("plbagianpengepakannama"), ""), sptField,
                     FxDB(drutama("plekspedisinama"), ""), sptField,
                     FxDB(drutama("plterminnama"), ""), sptField,
                     FxDB(drutama("plterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("plrekdiskonnama"), ""), sptField,
                     FxDB(drutama("plrekpajak1nama"), ""), sptField,
                     FxDB(drutama("plrekpajak2nama"), ""), sptField,
                     FxDB(drutama("plrekbiayalainnama"), ""), sptField,
                     FxDB(drutama("plnotransaksisq"), ""), sptField,
                     FxDB(drutama("plnotransaksiso"), ""), sptField,
                     FxDB(drutama("plnotransaksipi"), ""), sptField,
                     FxDB(drutama("plstatusnama"), ""), sptField,
                     FxDB(drutama("plstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("plinputusernama"), ""), sptField,
                     FxDB(drutama("plmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("ktingkatjual"), 0), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idpldetail"), 0), sptField,
                     FxDB(dr("idpl"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("nopack"), 0), sptField,
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
                     FxDB(dr("idsqdetail"), 0), sptField,
                     FxDB(dr("idsodetail"), 0), sptField,
                     FxDB(dr("idpidetail"), 0), sptField,
                     FxDB(dr("jmldo"), 0), sptField,
                     FxDB(dr("statusdo"), 0), sptField,
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
                     FxDB(dr("sqnotransaksi"), ""), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("pinotransaksi"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            Dim dtpack As New DataTable
            dtpack = AmbilData("aplikasi1-M5_Pl_Pack", "idpl='" & idtransaksi & "'", "urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1) ' Ambil data ke databases

            For Each dr As DataRow In dtpack.Rows
                pack = String.Concat(pack,
                     FxDB(dr("idplpack"), 0), sptField,
                     FxDB(dr("idpl"), 0), sptField,
                     FxDB(dr("nopack"), 0), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("bentuk"), ""), sptField,
                     FxDB(dr("berat"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptRow)
            Next
            pack = pack.Substring(0, pack.Length - sptRow.Length)

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, pack)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("plid, plcabang, pllokasi, plgudang, plasalbarang, plasalbarangkategori, pljenispenjualan, pljenispenjualankategori, plcarabayar, plsumber, plautonotransaksi, plnotransaksi, pltgl, plkodepa, plcustomer, plcustomerkontak, pl1alamat1, pl1alamat2, pl1alamat3, pl2alamat1, pl2alamat2, pl2alamat3, plbagianpenjualan, plbagianpengepakan, plekspedisi, pltglkirim, pltermin, pltgljatuhtempo, pluraian, plcatatan, plnoref, pltglnoref, pltglpenutupan, plmatauang, plkurs, plhargatermasukpajak, pltotal, pldiskonpersen, pljmldiskon, pltotalpajak1detail, pltotalpajak2detail, plbiayalainpersen, plbiayalain, pltotaltransaksi, plrekdiskon, plrekpajak1, plrekpajak2, plrekbiayalain, plidsq, plidso, plidpi, plstatusdo, plstatusdr, plstatussi, plstatusrnr, plstatussr, plstatusrealisasi, plstatus, plstatussebelumnya, pljmlrevisi, plcetakanke, plinputuser, plinputtgl, plmodifikasiuser, plmodifikasitgl, plposting, plpostingtgl, plisclose, plcustomtext1, plcustomtext2, plcustomtext3, plcustomtext4, plcustomtext5, plcustomint1, plcustomint2, plcustomint3, plcustomdbl1, plcustomdbl2, plcustomdbl3, plcustomdate1, plcustomdate2, plcustomdate3, plcabangnama, pllokasinama, plgudangnama, plcustomerkode, plcustomernama, plbagianpenjualankode, plbagianpenjualannama, plbagianpengepakankode, plbagianpengepakannama, plekspedisinama, plterminnama, plterminharijatuhtempo, plrekdiskonnama, plrekpajak1nama, plrekpajak2nama, plrekbiayalainnama, plnotransaksisq, plnotransaksiso, plnotransaksipi, plstatusnama, plstatussebelumnyanama, plinputusernama, plmodifikasiusernama, ktingkatjual, kpkp" & sptSubParam & "idpldetail, idpl, idbarang, namabarang, tipebarang, nopack, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpidetail, jmldo, statusdo, jmldr, statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, sqnotransaksi, sonotransaksi, pinotransaksi" & sptSubParam & "idplpack, idpl, nopack, catatan, bentuk, berat, urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_PlSearch(ByVal param As String) As String
        'M5_PlSearch --------------------------------------------------------
        'plid, plcabang, pllokasi, plgudang, plasalbarang, plasalbarangkategori, pljenispenjualan, 
        'pljenispenjualankategori, plcarabayar, plsumber, plautonotransaksi, plnotransaksi, pltgl, plkodepa, 
        'plcustomer, plcustomerkontak, pl1alamat1, pl1alamat2, pl1alamat3, pl2alamat1, pl2alamat2, 
        'pl2alamat3, plbagianpenjualan, plbagianpengepakan, plekspedisi, pltglkirim, pltermin, pltgljatuhtempo, 
        'pluraian, plcatatan, plnoref, pltglnoref, pltglpenutupan, plmatauang, plkurs, 
        'plhargatermasukpajak, pltotal, pldiskonpersen, pljmldiskon, pltotalpajak1detail, pltotalpajak2detail, plbiayalainpersen, 
        'plbiayalain, pltotaltransaksi, plrekdiskon, plrekpajak1, plrekpajak2, plrekbiayalain, plidsq, 
        'plidso, plidpi, plstatusdo, plstatusdr, plstatussi, plstatusrnr, plstatussr, 
        'plstatusrealisasi, plstatus, plstatussebelumnya, pljmlrevisi, plcetakanke, plinputuser, plinputtgl, 
        'plmodifikasiuser, plmodifikasitgl, plposting, plpostingtgl, plisclose, plcabangnama, pllokasinama, 
        'plgudangnama, plcustomerkode, plcustomernama, plbagianpenjualankode, plbagianpenjualannama, plekspedisinama, sqnotransaksi, 
        'sonotransaksi, plstatusnama, plstatussebelumnyanama, plinputusernama, plmodifikasiusernama

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strplrt(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", sorting As String = ""
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
            sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_pl_v")

        dt = AmbilData("aplikasi1-M5_pl_v", Filter, sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("plid"), 0), sptField,
                     FxDB(dr("plcabang"), ""), sptField,
                     FxDB(dr("pllokasi"), ""), sptField,
                     FxDB(dr("plgudang"), ""), sptField,
                     FxDB(dr("plasalbarang"), ""), sptField,
                     FxDB(dr("plasalbarangkategori"), 0), sptField,
                     FxDB(dr("pljenispenjualan"), ""), sptField,
                     FxDB(dr("pljenispenjualankategori"), 0), sptField,
                     FxDB(dr("plcarabayar"), 0), sptField,
                     FxDB(dr("plsumber"), ""), sptField,
                     FxDB(dr("plautonotransaksi"), 0), sptField,
                     FxDB(dr("plnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pltgl"), ""), formatTgl), sptField,
                     FxDB(dr("plkodepa"), 0), sptField,
                     FxDB(dr("plcustomer"), 0), sptField,
                     FxDB(dr("plcustomerkontak"), ""), sptField,
                     FxDB(dr("pl1alamat1"), ""), sptField,
                     FxDB(dr("pl1alamat2"), ""), sptField,
                     FxDB(dr("pl1alamat3"), ""), sptField,
                     FxDB(dr("pl2alamat1"), ""), sptField,
                     FxDB(dr("pl2alamat2"), ""), sptField,
                     FxDB(dr("pl2alamat3"), ""), sptField,
                     FxDB(dr("plbagianpenjualan"), 0), sptField,
                     FxDB(dr("plbagianpengepakan"), 0), sptField,
                     FxDB(dr("plekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pltglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("pltermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pltgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("pluraian"), ""), sptField,
                     FxDB(dr("plcatatan"), ""), sptField,
                     FxDB(dr("plnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pltglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("pltglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("plmatauang"), ""), sptField,
                     FxDB(dr("plkurs"), 0), sptField,
                     FxDB(dr("plhargatermasukpajak"), 0), sptField,
                     FxDB(dr("pltotal"), 0), sptField,
                     FxDB(dr("pldiskonpersen"), ""), sptField,
                     FxDB(dr("pljmldiskon"), 0), sptField,
                     FxDB(dr("pltotalpajak1detail"), 0), sptField,
                     FxDB(dr("pltotalpajak2detail"), 0), sptField,
                     FxDB(dr("plbiayalainpersen"), 0), sptField,
                     FxDB(dr("plbiayalain"), 0), sptField,
                     FxDB(dr("pltotaltransaksi"), 0), sptField,
                     FxDB(dr("plrekdiskon"), ""), sptField,
                     FxDB(dr("plrekpajak1"), ""), sptField,
                     FxDB(dr("plrekpajak2"), ""), sptField,
                     FxDB(dr("plrekbiayalain"), ""), sptField,
                     FxDB(dr("plidsq"), 0), sptField,
                     FxDB(dr("plidso"), 0), sptField,
                     FxDB(dr("plidpi"), 0), sptField,
                     FxDB(dr("plstatusdo"), 0), sptField,
                     FxDB(dr("plstatusdr"), 0), sptField,
                     FxDB(dr("plstatussi"), 0), sptField,
                     FxDB(dr("plstatusrnr"), 0), sptField,
                     FxDB(dr("plstatussr"), 0), sptField,
                     FxDB(dr("plstatusrealisasi"), 0), sptField,
                     FxDB(dr("plstatus"), 0), sptField,
                     FxDB(dr("plstatussebelumnya"), 0), sptField,
                     FxDB(dr("pljmlrevisi"), 0), sptField,
                     FxDB(dr("plcetakanke"), 0), sptField,
                     FxDB(dr("plinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("plinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("plmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("plmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("plposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("plpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("plisclose"), 0), sptField,
                     FxDB(dr("plcabangnama"), ""), sptField,
                     FxDB(dr("pllokasinama"), ""), sptField,
                     FxDB(dr("plgudangnama"), ""), sptField,
                     FxDB(dr("plcustomerkode"), ""), sptField,
                     FxDB(dr("plcustomernama"), ""), sptField,
                     FxDB(dr("plbagianpenjualankode"), ""), sptField,
                     FxDB(dr("plbagianpenjualannama"), ""), sptField,
                     FxDB(dr("plekspedisinama"), ""), sptField,
                     FxDB(dr("sqnotransaksi"), ""), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("plstatusnama"), ""), sptField,
                     FxDB(dr("plstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("plinputusernama"), ""), sptField,
                     FxDB(dr("plmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("plid, plcabang, pllokasi, plgudang, plasalbarang, plasalbarangkategori, pljenispenjualan, pljenispenjualankategori, plcarabayar, plsumber, plautonotransaksi, plnotransaksi, pltgl, plkodepa, plcustomer, plcustomerkontak, pl1alamat1, pl1alamat2, pl1alamat3, pl2alamat1, pl2alamat2, pl2alamat3, plbagianpenjualan, plbagianpengepakan, plekspedisi, pltglkirim, pltermin, pltgljatuhtempo, pluraian, plcatatan, plnoref, pltglnoref, pltglpenutupan, plmatauang, plkurs, plhargatermasukpajak, pltotal, pldiskonpersen, pljmldiskon, pltotalpajak1detail, pltotalpajak2detail, plbiayalainpersen, plbiayalain, pltotaltransaksi, plrekdiskon, plrekpajak1, plrekpajak2, plrekbiayalain, plidsq, plidso, plidpi, plstatusdo, plstatusdr, plstatussi, plstatusrnr, plstatussr, plstatusrealisasi, plstatus, plstatussebelumnya, pljmlrevisi, plcetakanke, plinputuser, plinputtgl, plmodifikasiuser, plmodifikasitgl, plposting, plpostingtgl, plisclose, plcabangnama, pllokasinama, plgudangnama, plcustomerkode, plcustomernama, plbagianpenjualankode, plbagianpenjualannama, plekspedisinama, sqnotransaksi, sonotransaksi, plstatusnama, plstatussebelumnyanama, plinputusernama, plmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_PlTerkait(ByVal param As String) As String
        'M5_PlTerkait --------------------------------------------------------
        'plid, plnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "plid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2) & " AND plid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "plid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.m5_pl_terkait(Filter)


        dt = AmbilData("aplikasi1-m5_pl_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each pl As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(pl("plid"), 0), sptField,
                     FxDB(pl("plnotransaksi"), ""), sptField,
                     FxDB(pl("sumber"), ""), sptField,
                     FxDB(pl("idterkait"), 0), sptField,
                     FxDB(pl("noterkait"), ""), sptField,
                     AsFormatTanggal(FxDB(pl("tglterkait"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(pl("inputtglterkait"), ""), formatTglWaktu), sptField,
                     AsFormatTanggal(FxDB(pl("modifikasitglterkait"), ""), formatTglWaktu), sptField,
                     FxDB(pl("jenisterkait"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Related PL data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("plid, plnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_Pl_Detail_VSearch(ByVal param As String) As String
        'M5_Pl_Detail_VSearch --------------------------------------------------------
        'idpldetail, idpl, idbarang, namabarang, tipebarang, nopack, jml, 
        'satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idsqdetail, idsodetail, idpidetail, jmldo, statusdo, jmldr, 
        'statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, 
        'jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3, plnotransaksi, pluraian, 
        'plcatatan, plnoref, pltglnoref, pltglkirim, plcustomerkontak, pl1alamat1, pl1alamat2, 
        'pl1alamat3, pl2alamat1, pl2alamat2, pl2alamat3, plbagianpenjualan, plbagianpenjualankode, plbagianpenjualannama, 
        'plbagianpengepakankode, plbagianpengepakannama, plekspedisi, plekspedisinama, pltermin, plterminnama, plterminharijatuhtempo, 
        'kodebarang, bhpp, bhppaverage, bhargajual1, bjenis, brekpersediaan, brekhargapokok, brekdiskonpenjualan, 
        'brekpenjualan, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, 
        'jmlsisado, jmlsisadr, jmlsisasi, jmlsisarealisasi, bjmllapangan, bsatuanlapangan, basset,
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

        Dim sol As String = ""

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
        'sol = query.PanggilQuery("m5_pl_detail_v")
        sol = "select `pld`.`idpldetail` AS `idpldetail`,`pld`.`idpl` AS `idpl`,`pld`.`idbarang` AS `idbarang`,`pld`.`namabarang` AS `namabarang`,`pld`.`tipebarang` AS `tipebarang`,`pld`.`nopack` AS `nopack`,`pld`.`jml` AS `jml`,`pld`.`satuan` AS `satuan`,`pld`.`nilaisatuan` AS `nilaisatuan`,`pld`.`jmlbarang` AS `jmlbarang`,`pld`.`satuanbarang` AS `satuanbarang`,`pld`.`matauang` AS `matauang`,`pld`.`kurs` AS `kurs`,`pld`.`harga` AS `harga`,`pld`.`diskon` AS `diskon`,`pld`.`jmldiskon` AS `jmldiskon`,`pld`.`pajak1` AS `pajak1`,`pld`.`jmlpajak1` AS `jmlpajak1`,`pld`.`pajak2` AS `pajak2`,`pld`.`jmlpajak2` AS `jmlpajak2`,`pld`.`cabang` AS `cabang`,`pld`.`lokasi` AS `lokasi`,`pld`.`gudang` AS `gudang`,`pld`.`costcenter` AS `costcenter`,`pld`.`divisi` AS `divisi`,`pld`.`subdivisi` AS `subdivisi`,`pld`.`proyek` AS `proyek`,`pld`.`catatan` AS `catatan`,`pld`.`urutan` AS `urutan`,`pld`.`idsqdetail` AS `idsqdetail`,`pld`.`idsodetail` AS `idsodetail`,`pld`.`idpidetail` AS `idpidetail`,`pld`.`jmldo` AS `jmldo`,`pld`.`statusdo` AS `statusdo`,`pld`.`jmldr` AS `jmldr`,`pld`.`statusdr` AS `statusdr`,`pld`.`jmlsi` AS `jmlsi`,`pld`.`statussi` AS `statussi`,`pld`.`jmlrnr` AS `jmlrnr`,`pld`.`statusrnr` AS `statusrnr`,`pld`.`jmlsr` AS `jmlsr`,`pld`.`statussr` AS `statussr`,`pld`.`jmlrealisasi` AS `jmlrealisasi`,`pld`.`statusrealisasi` AS `statusrealisasi`,`pld`.`isclose` AS `isclose`,`pld`.`customtext1` AS `customtext1`,`pld`.`customtext2` AS `customtext2`,`pld`.`customtext3` AS `customtext3`,`pld`.`customdbl1` AS `customdbl1`,`pld`.`customdbl2` AS `customdbl2`,`pld`.`customdbl3` AS `customdbl3`,`pld`.`customdate1` AS `customdate1`,`pld`.`customdate2` AS `customdate2`,`pld`.`customdate3` AS `customdate3`,`pl`.`plnotransaksi` AS `plnotransaksi`,`pl`.`pluraian` AS `pluraian`,`pl`.`plcatatan` AS `plcatatan`,`pl`.`plnoref` AS `plnoref`,`pl`.`pltglnoref` AS `pltglnoref`,`pl`.`pltglkirim` AS `pltglkirim`,`pl`.`plcustomerkontak` AS `plcustomerkontak`,`pl`.`pl1alamat1` AS `pl1alamat1`,`pl`.`pl1alamat2` AS `pl1alamat2`,`pl`.`pl1alamat3` AS `pl1alamat3`,`pl`.`pl2alamat1` AS `pl2alamat1`,`pl`.`pl2alamat2` AS `pl2alamat2`,`pl`.`pl2alamat3` AS `pl2alamat3`,`pl`.`plbagianpenjualan` AS `plbagianpenjualan`,`c1`.`kkode` AS `plbagianpenjualankode`,`c1`.`knama` AS `plbagianpenjualannama`,`c2`.`kkode` AS `plbagianpengepakankode`,`c2`.`knama` AS `plbagianpengepakannama`,`pl`.`plekspedisi` AS `plekspedisi`,`e`.`enama` AS `plekspedisinama`,`pl`.`pltermin` AS `pltermin`,`tr`.`trnama` AS `plterminnama`,`tr`.`trharijatuhtempo` AS `plterminharijatuhtempo`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bhppaverage` AS `bhppaverage`,`i`.`bhargajual1` AS `bhargajual1`,`i`.`bjenis` AS `bjenis`,`i`.`brekpersediaan` AS `brekpersediaan`,`i`.`brekhargapokok` AS `brekhargapokok`,`i`.`brekdiskonpenjualan` AS `brekdiskonpenjualan`,`i`.`brekpenjualan` AS `brekpenjualan`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,((`pld`.`jmlbarang` - `pld`.`jmldo`) / `pld`.`nilaisatuan`) AS `jmlsisado`,((`pld`.`jmlbarang` - `pld`.`jmldr`) / `pld`.`nilaisatuan`) AS `jmlsisadr`,((`pld`.`jmlbarang` - `pld`.`jmlsi`) / `pld`.`nilaisatuan`) AS `jmlsisasi`,((`pld`.`jmlbarang` - `pld`.`jmlrealisasi`) / `pld`.`nilaisatuan`) AS `jmlsisarealisasi`, i.bjmllapangan, i.bsatuanlapangan, i.basset, t1.takunbeli as pajak1akunbeli, t1c1.cnama as pajak1akunbelinama, t1.takunjual as pajak1akunjual, t1c2.cnama as pajak1akunjualnama, t2.takunbeli as pajak2akunbeli, t2c1.cnama as pajak2akunbelinama, t2.takunjual as pajak2akunjual, t2c2.cnama as pajak2akunjualnama, d.dnama AS divisinama, sd.sdnama AS subdivisinama, cc.ccnama AS costcenternama, p.pnama AS proyeknama from `m5_pl_detail` `pld` left join `m5_pl` `pl` on `pld`.`idpl` = `pl`.`plid` left join `m1_terms` `tr` on `pl`.`pltermin` = `tr`.`trkode` left join `m1_contact` `c1` on `pl`.`plbagianpenjualan` = `c1`.`kid` left join `m1_contact` `c2` on `pl`.`plbagianpengepakan` = `c2`.`kid` left join `m1_expedition` `e` on `pl`.`plekspedisi` = `e`.`ekode` left join `m1_item` `i` on `pld`.`idbarang` = `i`.`bid` left join `m1_tax` `t1` on `pld`.`pajak1` = `t1`.`tkode` left join `m1_tax` `t2` on `pld`.`pajak2` = `t2`.`tkode` left join m1_coa t1c1 on t1.takunbeli = t1c1.cnomor left join m1_coa t1c2 on t1.takunjual = t1c2.cnomor left join m1_coa t2c1 on t2.takunbeli = t2c1.cnomor left join m1_coa t2c2 on t2.takunjual = t2c2.cnomor LEFT JOIN m1_division d ON d.dkode = pld.divisi LEFT JOIN m1_subdivision sd ON sd.sdkode = pld.subdivisi LEFT JOIN m1_cost_center cc ON cc.cckode = pld.costcenter LEFT JOIN m1_project p ON p.pkode = pld.proyek"

        dt = AmbilData("aplikasi1-M5_pl_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sol) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idpldetail"), 0), sptField,
                     FxDB(dr("idpl"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("nopack"), 0), sptField,
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
                     FxDB(dr("idsqdetail"), 0), sptField,
                     FxDB(dr("idsodetail"), 0), sptField,
                     FxDB(dr("idpidetail"), 0), sptField,
                     FxDB(dr("jmldo"), 0), sptField,
                     FxDB(dr("statusdo"), 0), sptField,
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
                     FxDB(dr("plnotransaksi"), ""), sptField,
                     FxDB(dr("pluraian"), ""), sptField,
                     FxDB(dr("plcatatan"), ""), sptField,
                     FxDB(dr("plnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pltglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("pltglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("plcustomerkontak"), ""), sptField,
                     FxDB(dr("pl1alamat1"), ""), sptField,
                     FxDB(dr("pl1alamat2"), ""), sptField,
                     FxDB(dr("pl1alamat3"), ""), sptField,
                     FxDB(dr("pl2alamat1"), ""), sptField,
                     FxDB(dr("pl2alamat2"), ""), sptField,
                     FxDB(dr("pl2alamat3"), ""), sptField,
                     FxDB(dr("plbagianpenjualan"), 0), sptField,
                     FxDB(dr("plbagianpenjualankode"), ""), sptField,
                     FxDB(dr("plbagianpenjualannama"), ""), sptField,
                     FxDB(dr("plbagianpengepakankode"), ""), sptField,
                     FxDB(dr("plbagianpengepakannama"), ""), sptField,
                     FxDB(dr("plekspedisi"), ""), sptField,
                     FxDB(dr("plekspedisinama"), ""), sptField,
                     FxDB(dr("pltermin"), ""), sptField,
                     FxDB(dr("plterminnama"), ""), sptField,
                     FxDB(dr("plterminharijatuhtempo"), 0), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bhppaverage"), 0), sptField,
                     FxDB(dr("bhargajual1"), 0), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("brekpersediaan"), ""), sptField,
                     FxDB(dr("brekhargapokok"), ""), sptField,
                     FxDB(dr("brekdiskonpenjualan"), ""), sptField,
                     FxDB(dr("brekpenjualan"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("jmlsisado"), 0), sptField,
                     FxDB(dr("jmlsisadr"), 0), sptField,
                     FxDB(dr("jmlsisasi"), 0), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idpldetail, idpl, idbarang, namabarang, tipebarang, nopack, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpidetail, jmldo, statusdo, jmldr, statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, plnotransaksi, pluraian, plcatatan, plnoref, pltglnoref, pltglkirim, plcustomerkontak, pl1alamat1, pl1alamat2, pl1alamat3, pl2alamat1, pl2alamat2, pl2alamat3, plbagianpenjualan, plbagianpenjualankode, plbagianpenjualannama, plbagianpengepakankode, plbagianpengepakannama, plekspedisi, plekspedisinama, pltermin, plterminnama, plterminharijatuhtempo, kodebarang, bhpp, bhppaverage, bhargajual1, bjenis, brekpersediaan, brekhargapokok, brekdiskonpenjualan, brekpenjualan, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisado, jmlsisadr, jmlsisasi, jmlsisarealisasi, bjmllapangan, bsatuanlapangan, basset, pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, pajak2akunjualnama, divisinama, subdivisinama, costcenternama, proyeknama"))

        Return wsResult
    End Function

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstandingSO As String, ByVal ftOutstandingSO As String, ByVal ftExistOutstandingPI As String, ByVal ftOutstandingPI As String, ByVal ftSO As String, ByVal ftPI As String, ByRef termasukPajak As String) As String
        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaiSatuan As Double = 0, sisa As Double = 0
        Dim filterLookup As String = "", urutan As String = ""

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

            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
            sql = "SELECT sod.idsodetail, (sod.jmlbarang - sod.jmlrealisasi) as sisarealisasi, i.bid, i.bkode FROM m5_so_detail AS sod INNER JOIN m1_item AS i ON sod.idbarang = i.bid WHERE " & ftOutstandingSO
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisarealisasi")

                filterLookup = "idsodetail=" & dtval.Rows(0)("idsodetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in SO, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
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
        'END OF VALIDASI OUTSTANDING --------------------------------

selesai:
        Return errmessage
    End Function

    <WebMethod()>
    Public Function M5_PlSimpanOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataPack(), dataRowPack() As String

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
        'plid(0) As Integer, plcabang(1) As String, pllokasi(2) As String, plgudang(3) As String, plasalbarang(4) As String, 
        'plasalbarangkategori(5) As Integer, pljenispenjualan(6) As String, pljenispenjualankategori(7) As Integer, plcarabayar(8) As Integer, plsumber(9) As String, 
        'plautonotransaksi(10) As Integer, plnotransaksi(11) As String, pltgl(12) As Date, plkodepa(13) As Integer, plcustomer(14) As Integer, 
        'plcustomerkontak(15) As String, pl1alamat1(16) As String, pl1alamat2(17) As String, pl1alamat3(18) As String, pl2alamat1(19) As String, 
        'pl2alamat2(20) As String, pl2alamat3(21) As String, plbagianpenjualan(22) As Integer, plbagianpengepakan(23) As Integer, plekspedisi(24) As String, 
        'pltglkirim(25) As Date, pltermin(26) As String, pltgljatuhtempo(27) As Date, pluraian(28) As String, plcatatan(29) As String, 
        'plnoref(30) As String, pltglnoref(31) As Date, pltglpenutupan(32) As Date, plmatauang(33) As String, plkurs(34) As Double, 
        'plhargatermasukpajak(35) As Integer, pltotal(36) As Double, pldiskonpersen(37) As String, pljmldiskon(38) As Double, pltotalpajak1detail(39) As Double, 
        'pltotalpajak2detail(40) As Double, plbiayalainpersen(41) As Double, plbiayalain(42) As Double, pltotaltransaksi(43) As Double, plrekdiskon(44) As String, 
        'plrekpajak1(45) As String, plrekpajak2(46) As String, plrekbiayalain(47) As String, plidsq(48) As Integer, plidso(49) As Integer, 
        'plidpi(50) As Integer, plstatusdo(51) As Integer, plstatusdr(52) As Integer, plstatussi(53) As Integer, plstatusrnr(54) As Integer, 
        'plstatussr(55) As Integer, plstatus(56) As Integer, plstatussebelumnya(57) As Integer, pljmlrevisi(58) As Integer, plcetakanke(59) As Integer, 
        'plinputuser(60) As Integer, plinputtgl(61) As DateTime, plmodifikasiuser(62) As Integer, plmodifikasitgl(63) As DateTime, plisclose(64) As Integer, 
        'plcustomtext1(65) As String, plcustomtext2(66) As String, plcustomtext3(67) As String, plcustomtext4(68) As String, plcustomtext5(69) As String, 
        'plcustomint1(70) As Integer, plcustomint2(71) As Integer, plcustomint3(72) As Integer, plcustomdbl1(73) As Double, plcustomdbl2(74) As Double, 
        'plcustomdbl3(75) As Double, plcustomdate1(76) As Date, plcustomdate2(77) As Date, plcustomdate3(78) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'plid, plcabang, pllokasi, plgudang, plasalbarang, plasalbarangkategori, pljenispenjualan, 
        'pljenispenjualankategori, plcarabayar, plsumber, plautonotransaksi, plnotransaksi, pltgl, plkodepa, 
        'plcustomer, plcustomerkontak, pl1alamat1, pl1alamat2, pl1alamat3, pl2alamat1, pl2alamat2, 
        'pl2alamat3, plbagianpenjualan, plbagianpengepakan, plekspedisi, pltglkirim, pltermin, pltgljatuhtempo, 
        'pluraian, plcatatan, plnoref, pltglnoref, pltglpenutupan, plmatauang, plkurs, 
        'plhargatermasukpajak, pltotal, pldiskonpersen, pljmldiskon, pltotalpajak1detail, pltotalpajak2detail, plbiayalainpersen, 
        'plbiayalain, pltotaltransaksi, plrekdiskon, plrekpajak1, plrekpajak2, plrekbiayalain, plidsq, 
        'plidso, plidpi, plstatusdo, plstatusdr, plstatussi, plstatusrnr, plstatussr, 
        'plstatus, plstatussebelumnya, pljmlrevisi, plcetakanke, plinputuser, plinputtgl, plmodifikasiuser, 
        'plmodifikasitgl, plisclose, plcustomtext1, plcustomtext2, plcustomtext3, plcustomtext4, plcustomtext5, 
        'plcustomint1, plcustomint2, plcustomint3, plcustomdbl1, plcustomdbl2, plcustomdbl3, plcustomdate1, 
        'plcustomdate2, plcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 79) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'plid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "plid required numeric." : GoTo selesai
        End If
        'plasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "plasalbarangkategori required numeric." : GoTo selesai
        End If
        'pljenispenjualankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "pljenispenjualankategori required numeric." : GoTo selesai
        End If
        'plcarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "plcarabayar required numeric." : GoTo selesai
        End If
        'plautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "plautonotransaksi required numeric." : GoTo selesai
        End If
        'pltgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "pltgl required date." : GoTo selesai
        End If
        'plkodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "plkodepa required numeric." : GoTo selesai
        End If
        'plcustomer(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "plcustomer required numeric." : GoTo selesai
        End If
        'plbagianpenjualan(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "plbagianpenjualan required numeric." : GoTo selesai
        End If
        'plbagianpengepakan(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "plbagianpengepakan required numeric." : GoTo selesai
        End If
        'pltglkirim(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "pltglkirim required date." : GoTo selesai
        End If
        'pltgljatuhtempo(27) As Date
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "pltgljatuhtempo required date." : GoTo selesai
        End If
        'pltglnoref(31) As Date
        If (IsDate(dataUtama(31)) = False) Then
            result(2) = "pltglnoref required date." : GoTo selesai
        End If
        'pltglpenutupan(32) As Date
        If (IsDate(dataUtama(32)) = False) Then
            result(2) = "pltglpenutupan required date." : GoTo selesai
        End If
        'plkurs(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "plkurs required numeric." : GoTo selesai
        End If
        'plhargatermasukpajak(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "plhargatermasukpajak required numeric." : GoTo selesai
        End If
        'pltotal(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "pltotal required numeric." : GoTo selesai
        End If
        'pljmldiskon(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "pljmldiskon required numeric." : GoTo selesai
        End If
        'pltotalpajak1detail(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "pltotalpajak1detail required numeric." : GoTo selesai
        End If
        'pltotalpajak2detail(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "pltotalpajak2detail required numeric." : GoTo selesai
        End If
        ''plbiayalainpersen(41) As Double
        'If (IsNumeric(dataUtama(41)) = False) Then
        '    result(2) = "plbiayalainpersen required numeric." : GoTo selesai
        'End If
        'plbiayalain(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "plbiayalain required numeric." : GoTo selesai
        End If
        'pltotaltransaksi(43) As Double
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "pltotaltransaksi required numeric." : GoTo selesai
        End If
        'plidsq(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "plidsq required numeric." : GoTo selesai
        End If
        'plidso(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "plidso required numeric." : GoTo selesai
        End If
        'plidpi(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "plidpi required numeric." : GoTo selesai
        End If
        'plstatusdo(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "plstatusdo required numeric." : GoTo selesai
        End If
        'plstatusdr(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "plstatusdr required numeric." : GoTo selesai
        End If
        'plstatussi(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "plstatussi required numeric." : GoTo selesai
        End If
        'plstatusrnr(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "plstatusrnr required numeric." : GoTo selesai
        End If
        'plstatussr(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "plstatussr required numeric." : GoTo selesai
        End If
        'plstatus(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "plstatus required numeric." : GoTo selesai
        End If
        'plstatussebelumnya(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "plstatussebelumnya required numeric." : GoTo selesai
        End If
        'pljmlrevisi(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "pljmlrevisi required numeric." : GoTo selesai
        End If
        'plcetakanke(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "plcetakanke required numeric." : GoTo selesai
        End If
        'plinputuser(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "plinputuser required numeric." : GoTo selesai
        End If
        'plinputtgl(61) As DateTime
        If (IsDate(dataUtama(61)) = False) Then
            result(2) = "plinputtgl required date." : GoTo selesai
        End If
        'plmodifikasiuser(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "plmodifikasiuser required numeric." : GoTo selesai
        End If
        'plmodifikasitgl(63) As DateTime
        If (IsDate(dataUtama(63)) = False) Then
            result(2) = "plmodifikasitgl required date." : GoTo selesai
        End If
        'plisclose(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "plisclose required numeric." : GoTo selesai
        End If
        'plcustomint1(70) As Integer
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "plcustomint1 required numeric." : GoTo selesai
        End If
        'plcustomint2(71) As Integer
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "plcustomint2 required numeric." : GoTo selesai
        End If
        'plcustomint3(72) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "plcustomint3 required numeric." : GoTo selesai
        End If
        'plcustomdbl1(73) As Double
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "plcustomdbl1 required numeric." : GoTo selesai
        End If
        'plcustomdbl2(74) As Double
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "plcustomdbl2 required numeric." : GoTo selesai
        End If
        'plcustomdbl3(75) As Double
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "plcustomdbl3 required numeric." : GoTo selesai
        End If
        'plcustomdate1(76) As Date
        If (IsDate(dataUtama(76)) = False) Then
            result(2) = "plcustomdate1 required date." : GoTo selesai
        End If
        'plcustomdate2(77) As Date
        If (IsDate(dataUtama(77)) = False) Then
            result(2) = "plcustomdate2 required date." : GoTo selesai
        End If
        'plcustomdate3(78) As Date
        If (IsDate(dataUtama(78)) = False) Then
            result(2) = "plcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'plcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "plcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "plcabang should not be more than 25 character." : GoTo selesai
        End If

        'pllokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "pllokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "pllokasi should not be more than 25 character." : GoTo selesai
        End If

        'plgudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "plgudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "plgudang should not be more than 25 character." : GoTo selesai
        End If

        'plsumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "plsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "plsumber should not be more than 10 character." : GoTo selesai
        End If

        'plnotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "plnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "plnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'pltgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "pltgl can't be empty" : GoTo selesai
        End If

        'pltglkirim(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "pltglkirim can't be empty" : GoTo selesai
        End If

        'pltgljatuhtempo(27) As Date
        If Len(dataUtama(27)) = 0 Then
            result(2) = "pltgljatuhtempo can't be empty" : GoTo selesai
        End If

        'pltglnoref(31) As Date
        If Len(dataUtama(31)) = 0 Then
            result(2) = "pltglnoref can't be empty" : GoTo selesai
        End If

        'pltglpenutupan(32) As Date
        If Len(dataUtama(32)) = 0 Then
            result(2) = "pltglpenutupan can't be empty" : GoTo selesai
        End If

        'plmatauang(33) As String
        If Len(dataUtama(33)) = 0 Then
            result(2) = "plmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(33)) > 25 Then
            result(2) = "plmatauang should not be more than 25 character." : GoTo selesai
        End If

        'plkurs(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "plkurs can't be empty" : GoTo selesai
        End If

        'pltotal(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "pltotal can't be empty" : GoTo selesai
        End If

        'pldiskonpersen(37) As String
        If Len(dataUtama(37)) = 0 Then
            result(2) = "pldiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(37)) > 25 Then
            result(2) = "pldiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'pljmldiskon(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "pljmldiskon can't be empty" : GoTo selesai
        End If

        'pltotalpajak1detail(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "pltotalpajak1detail can't be empty" : GoTo selesai
        End If

        'pltotalpajak2detail(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "pltotalpajak2detail can't be empty" : GoTo selesai
        End If

        'plbiayalainpersen(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "plbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(41)) > 25 Then
            result(2) = "plbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'plbiayalain(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "plbiayalain can't be empty" : GoTo selesai
        End If

        'pltotaltransaksi(43) As Double
        If Len(dataUtama(43)) = 0 Then
            result(2) = "pltotaltransaksi can't be empty" : GoTo selesai
        End If

        'plinputtgl(61) As DateTime
        If Len(dataUtama(61)) = 0 Then
            result(2) = "plinputtgl can't be empty" : GoTo selesai
        End If

        'plmodifikasitgl(63) As DateTime
        If Len(dataUtama(63)) = 0 Then
            result(2) = "plmodifikasitgl can't be empty" : GoTo selesai
        End If

        'plcustomdbl1(73) As Double
        If Len(dataUtama(73)) = 0 Then
            result(2) = "plcustomdbl1 can't be empty" : GoTo selesai
        End If

        'plcustomdbl2(74) As Double
        If Len(dataUtama(74)) = 0 Then
            result(2) = "plcustomdbl2 can't be empty" : GoTo selesai
        End If

        'plcustomdbl3(75) As Double
        If Len(dataUtama(75)) = 0 Then
            result(2) = "plcustomdbl3 can't be empty" : GoTo selesai
        End If

        'plcustomdate1(76) As Date
        If Len(dataUtama(76)) = 0 Then
            result(2) = "plcustomdate1 can't be empty" : GoTo selesai
        End If

        'plcustomdate2(77) As Date
        If Len(dataUtama(77)) = 0 Then
            result(2) = "plcustomdate2 can't be empty" : GoTo selesai
        End If

        'plcustomdate3(78) As Date
        If Len(dataUtama(78)) = 0 Then
            result(2) = "plcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "plid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pllokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pljenispenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pljenispenjualankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pltgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plcustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plcustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pl1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pl1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pl1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pl2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pl2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pl2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plbagianpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plbagianpengepakan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plekspedisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pltglkirim", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pltermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pltgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pluraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pltglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pltglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plhargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pltotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pldiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pljmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pltotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pltotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pltotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plrekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plrekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plrekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plrekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plidsq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plidso", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plidpi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plstatusdo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plstatusdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plstatussi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plstatusrnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plstatussr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pljmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "plcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "plcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "plid~plcabang~pllokasi~plgudang~plasalbarang~plasalbarangkategori~pljenispenjualan~pljenispenjualankategori~plcarabayar~plsumber~plautonotransaksi~plnotransaksi~pltgl~plkodepa~plcustomer~plcustomerkontak~pl1alamat1~pl1alamat2~pl1alamat3~pl2alamat1~pl2alamat2~pl2alamat3~plbagianpenjualan~plbagianpengepakan~plekspedisi~pltglkirim~pltermin~pltgljatuhtempo~pluraian~plcatatan~plnoref~pltglnoref~pltglpenutupan~plmatauang~plkurs~plhargatermasukpajak~pltotal~pldiskonpersen~pljmldiskon~pltotalpajak1detail~pltotalpajak2detail~plbiayalainpersen~plbiayalain~pltotaltransaksi~plrekdiskon~plrekpajak1~plrekpajak2~plrekbiayalain~plidsq~plidso~plidpi~plstatusdo~plstatusdr~plstatussi~plstatusrnr~plstatussr~plstatus~plstatussebelumnya~pljmlrevisi~plcetakanke~plinputuser~plinputtgl~plmodifikasiuser~plmodifikasitgl~plisclose~plcustomtext1~plcustomtext2~plcustomtext3~plcustomtext4~plcustomtext5~plcustomint1~plcustomint2~plcustomint3~plcustomdbl1~plcustomdbl2~plcustomdbl3~plcustomdate1~plcustomdate2~plcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idpldetail(0) As Integer, idpl(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'nopack(5) As Integer, jml(6) As Double, satuan(7) As String, nilaisatuan(8) As Double, jmlbarang(9) As Double, 
        'satuanbarang(10) As String, matauang(11) As String, kurs(12) As Double, harga(13) As Double, diskon(14) As String, 
        'jmldiskon(15) As Double, pajak1(16) As String, jmlpajak1(17) As Double, pajak2(18) As String, jmlpajak2(19) As Double, 
        'cabang(20) As String, lokasi(21) As String, gudang(22) As String, costcenter(23) As String, divisi(24) As String, 
        'subdivisi(25) As String, proyek(26) As String, catatan(27) As String, urutan(28) As Integer, idsqdetail(29) As Integer, 
        'idsodetail(30) As Integer, idpidetail(31) As Integer, jmldo(32) As Double, statusdo(33) As Integer, jmldr(34) As Double, 
        'statusdr(35) As Integer, jmlsi(36) As Double, statussi(37) As Integer, jmlrnr(38) As Double, statusrnr(39) As Integer, 
        'jmlsr(40) As Double, statussr(41) As Integer, isclose(42) As Integer, customtext1(43) As String, customtext2(44) As String, 
        'customtext3(45) As String, customdbl1(46) As Double, customdbl2(47) As Double, customdbl3(48) As Double, customdate1(49) As Date, 
        'customdate2(50) As Date, customdate3(51) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idpldetail, idpl, idbarang, namabarang, tipebarang, nopack, jml, 
        'satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idsqdetail, idsodetail, idpidetail, jmldo, statusdo, jmldr, 
        'statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idpldetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nopack", AsEnumTypeData.AsString)
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
        AsDataTableTambahField(dtdetail, "idsqdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idsodetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idpidetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmldo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusdo", AsEnumTypeData.AsInt64)
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

        'Variabel ValidasiSimpan
        Dim ftExistOutstandingSO As String = "", ftOutstandingSO As String = "", updNilaiSO As String = "", updFilterSO As String = ""
        Dim ftExistOutstandingPI As String = "", ftOutstandingPI As String = "", updNilaiPI As String = "", updFilterPI As String = ""
        Dim idbarang As Integer = 0, idsodetail As Integer = 0, idpidetail As Integer = 0, jmlbarang As Double = 0

        'FILTER SO DAN PI, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftSO As String = "", ftPI As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 52) Then
                result(2) = "Detail Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idpldetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Detail Row : " & i & " - idpldetail required numeric." : GoTo selesai
            End If
            'idpl(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Detail Row : " & i & " - idpl required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Detail Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            ''nopack(5) As Integer
            'If (IsNumeric(dataRowDetail(5)) = False) Then
            '    result(2) = "Detail Row : " & i & " - nopack required numeric." : GoTo selesai
            'End If
            'jml(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Detail Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'nilaisatuan(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Detail Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarang(9) As Double
            'jmlbarang = jml * nilaisatuan
            dataRowDetail(9) = Double.Parse(dataRowDetail(6)) * Double.Parse(dataRowDetail(8))
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Detail Row : " & i & " - jmlbarang required numeric." : GoTo selesai
            End If
            'kurs(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Detail Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'harga(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Detail Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'jmldiskon(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Detail Row : " & i & " - jmldiskon required numeric." : GoTo selesai
            End If
            'jmlpajak1(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Detail Row : " & i & " - jmlpajak1 required numeric." : GoTo selesai
            End If
            'jmlpajak2(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Detail Row : " & i & " - jmlpajak2 required numeric." : GoTo selesai
            End If
            'urutan(28) As Integer
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Detail Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idsqdetail(29) As Integer
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Detail Row : " & i & " - idsqdetail required numeric." : GoTo selesai
            End If
            'idsodetail(30) As Integer
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Detail Row : " & i & " - idsodetail required numeric." : GoTo selesai
            End If
            'idpidetail(31) As Integer
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Detail Row : " & i & " - idpidetail required numeric." : GoTo selesai
            End If
            'jmldo(32) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Detail Row : " & i & " - jmldo required numeric." : GoTo selesai
            End If
            'statusdo(33) As Integer
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Detail Row : " & i & " - statusdo required numeric." : GoTo selesai
            End If
            'jmldr(34) As Double
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Detail Row : " & i & " - jmldr required numeric." : GoTo selesai
            End If
            'statusdr(35) As Integer
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Detail Row : " & i & " - statusdr required numeric." : GoTo selesai
            End If
            'jmlsi(36) As Double
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Detail Row : " & i & " - jmlsi required numeric." : GoTo selesai
            End If
            'statussi(37) As Integer
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Detail Row : " & i & " - statussi required numeric." : GoTo selesai
            End If
            'jmlrnr(38) As Double
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Detail Row : " & i & " - jmlrnr required numeric." : GoTo selesai
            End If
            'statusrnr(39) As Integer
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Detail Row : " & i & " - statusrnr required numeric." : GoTo selesai
            End If
            'jmlsr(40) As Double
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Detail Row : " & i & " - jmlsr required numeric." : GoTo selesai
            End If
            'statussr(41) As Integer
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Detail Row : " & i & " - statussr required numeric." : GoTo selesai
            End If
            'isclose(42) As Integer
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Detail Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(46) As Double
            If (IsNumeric(dataRowDetail(46)) = False) Then
                result(2) = "Detail Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(47) As Double
            If (IsNumeric(dataRowDetail(47)) = False) Then
                result(2) = "Detail Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(48) As Double
            If (IsNumeric(dataRowDetail(48)) = False) Then
                result(2) = "Detail Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(49) As Date
            If (IsDate(dataRowDetail(49)) = False) Then
                result(2) = "Detail Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(50) As Date
            If (IsDate(dataRowDetail(50)) = False) Then
                result(2) = "Detail Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(51) As Date
            If (IsDate(dataRowDetail(51)) = False) Then
                result(2) = "Detail Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'namabarang(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Detail Row : " & i & " - namabarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 100 Then
                result(2) = "Detail Row : " & i & " - namabarang should not be more than 100 character." : GoTo selesai
            End If

            'nopack(5) As String
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Detail Row : " & i & " - nopack can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(5)) > 25 Then
                result(2) = "Detail Row : " & i & " - nopack should not be more than 25 character." : GoTo selesai
            End If

            'jml(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Detail Row : " & i & " - jml can't be empty" : GoTo selesai
            End If
            If dataRowDetail(6) <= 0 Then
                result(2) = "Detail Row : " & i & " - jml can't be less than or equal to zero" : GoTo selesai
            End If

            'satuan(7) As String
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Detail Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(7)) > 25 Then
                result(2) = "Detail Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'nilaisatuan(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Detail Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmlbarang(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Detail Row : " & i & " - jmlbarang can't be empty" : GoTo selesai
            End If
            If dataRowDetail(9) <= 0 Then
                result(2) = "Detail Row : " & i & " - jmlbarang can't be less than or equal to zero" : GoTo selesai
            End If

            'satuanbarang(10) As String
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Detail Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(10)) > 25 Then
                result(2) = "Detail Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
            End If

            'matauang(11) As String
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Detail Row : " & i & " - matauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(11)) > 25 Then
                result(2) = "Detail Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Detail Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'harga(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Detail Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'diskon(14) As String
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Detail Row : " & i & " - diskon can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(14)) > 25 Then
                result(2) = "Detail Row : " & i & " - diskon should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskon(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Detail Row : " & i & " - jmldiskon can't be empty" : GoTo selesai
                'Else
                '    'HITUNG JMLDISKON : jml(6) As Double, harga(13) As Double, diskon(14) As String
                '    dataRowDetail(15) = F_Diskon(Double.Parse(dataRowDetail(6)), Double.Parse(dataRowDetail(13)), FixQuotes(dataRowDetail(14).ToString))
            End If

            'jmlpajak1(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Detail Row : " & i & " - jmlpajak1 can't be empty" : GoTo selesai
            End If

            'jmlpajak2(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Detail Row : " & i & " - jmlpajak2 can't be empty" : GoTo selesai
            End If

            'jmldo(32) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Detail Row : " & i & " - jmldo can't be empty" : GoTo selesai
            End If

            'jmldr(34) As Double
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Detail Row : " & i & " - jmldr can't be empty" : GoTo selesai
            End If

            'jmlsi(36) As Double
            If Len(dataRowDetail(36)) = 0 Then
                result(2) = "Detail Row : " & i & " - jmlsi can't be empty" : GoTo selesai
            End If

            'jmlrnr(38) As Double
            If Len(dataRowDetail(38)) = 0 Then
                result(2) = "Detail Row : " & i & " - jmlrnr can't be empty" : GoTo selesai
            End If

            'jmlsr(40) As Double
            If Len(dataRowDetail(40)) = 0 Then
                result(2) = "Detail Row : " & i & " - jmlsr can't be empty" : GoTo selesai
            End If

            'customdbl1(46) As Double
            If Len(dataRowDetail(46)) = 0 Then
                result(2) = "Detail Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(47) As Double
            If Len(dataRowDetail(47)) = 0 Then
                result(2) = "Detail Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(48) As Double
            If Len(dataRowDetail(48)) = 0 Then
                result(2) = "Detail Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(49) As Date
            If Len(dataRowDetail(49)) = 0 Then
                result(2) = "Detail Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(50) As Date
            If Len(dataRowDetail(50)) = 0 Then
                result(2) = "Detail Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(51) As Date
            If Len(dataRowDetail(51)) = 0 Then
                result(2) = "Detail Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idpldetail~idpl~idbarang~namabarang~tipebarang~nopack~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~idsqdetail~idsodetail~idpidetail~jmldo~statusdo~jmldr~statusdr~jmlsi~statussi~jmlrnr~statusrnr~jmlsr~statussr~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51)) = False Then
                result(2) = "Detail Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , idsodetail(30) As Integer      , idpidetail(31) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : idsodetail = dataRowDetail(30) : idpidetail = dataRowDetail(31)

            'VALIDASI OUTSTANDING -------------------------
            If idsodetail <> 0 Then 'SO
                'CEK SO YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftSO = IIf(Len(ftSO.ToString) = 0, "", ftSO & " OR ")
                ftSO = String.Concat(ftSO, " (sod.idsodetail = " & idsodetail & ") ")

                '1. CEK DATA EXIST ------------------------
                ftExistOutstandingSO = IIf(Len(ftExistOutstandingSO.ToString) = 0, "", ftExistOutstandingSO & " UNION ")
                'ftExistOutstandingSO = String.Concat(ftExistOutstandingSO, "SELECT EXISTS(SELECT 1 FROM m5_so_detail JOIN m5_so ON idso = soid WHERE idsodetail = '" & idsodetail & "' AND (sostatus = 2 OR sostatus = 3 OR sostatus = 4 OR sostatus = 7) LIMIT 1) as rowExists, '" & idsodetail & "' as idsodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
                'ftExistOutstandingSO = String.Concat(ftExistOutstandingSO, "SELECT EXISTS(SELECT 1 FROM m5_so_detail JOIN m5_so ON idso = soid WHERE idsodetail = '" & idsodetail & "' AND (sostatus = 2 OR sostatus = 3) LIMIT 1) as rowExists, '" & idsodetail & "' as idsodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
                ftExistOutstandingSO = String.Concat(ftExistOutstandingSO, "SELECT EXISTS(SELECT 1 FROM m5_so_detail JOIN m5_so ON idso = soid WHERE idsodetail = '" & idsodetail & "' AND (sostatus = 2 OR sostatus = 3 OR sostatus = 4 LIMIT 1) as rowExists, '" & idsodetail & "' as idsodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                If idpidetail = 0 Then
                    '2. CEK JML OUTSTANDING -------------------
                    Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsodetail=" & idsodetail)
                    ftOutstandingSO = IIf(Len(ftOutstandingSO.ToString) = 0, "", ftOutstandingSO & " OR ")
                    ftOutstandingSO = String.Concat(ftOutstandingSO, " (sod.idsodetail = " & idsodetail & " AND " & Outstanding & " > (sod.jmlbarang - sod.jmlrealisasi)) ")

                    '3. SET NILAI UPDATE OUTSTANDING ----------
                    updNilaiSO = String.Concat("WHEN '" & idsodetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiSO)

                    '4. SET FILTER UPDATE OUTSTANDING ---------
                    updFilterSO = IIf(Len(updFilterSO.ToString) = 0, "", updFilterSO & " OR ")
                    updFilterSO = String.Concat(updFilterSO, "(idsodetail = '" & idsodetail & "')")
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
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'MAPPING BUAT WS DATA PACK -------------------------------------------------------
        'idplpack(0) As Integer, idpl(1) As Integer, nopack(2) As Integer, catatan(3) As String, bentuk(4) As String, 
        'berat(5) As String, urutan(6) As Integer, customtext1(7) As String, customtext2(8) As String, customtext3(9) As String, 
        'customdbl1(10) As Double, customdbl2(11) As Double, customdbl3(12) As Double, customdate1(13) As Date, customdate2(14) As Date, 
        'customdate3(15) As Date

        'MAPPING BUAT FLEX DATA PACK -----------------------------------------------------
        'idplpack, idpl, nopack, catatan, bentuk, berat, urutan, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA PACK ======================================================
        'SPLIT PARAMETER DATA PACK
        dataPack = dataSplit(2).Split(sptRow)
        'END OF VALIDASI DAN SET DATA PACK ===============================================

        'Buat datatable pack
        Dim dtpack As New DataTable
        AsDataTableTambahField(dtpack, "idplpack", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "idpl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpack, "nopack", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "bentuk", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "berat", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpack, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpack, "customdate3", AsEnumTypeData.AsString)

        'VALIDASI DAN SET DATA ROW PACK ==================================================
        Dim JmlDtPack As Integer = dataPack.Length
        For i = 1 To JmlDtPack
            'SPLIT DATA DETAIL
            dataRowPack = dataPack(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA PACK -----------------------------------
            'CEK ARRAY DATA PACK
            If (dataRowPack.Length <> 16) Then
                result(2) = "Pack Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW PACK ----------------------------

            'VALIDASI TIPE DATA PACK ------------------------------------------
            'idplpack(0) As Integer
            If (IsNumeric(dataRowPack(0)) = False) Then
                result(2) = "Pack Row : " & i & " - idplpack required numeric." : GoTo selesai
            End If
            'idpl(1) As Integer
            If (IsNumeric(dataRowPack(1)) = False) Then
                result(2) = "Pack Row : " & i & " - idpl required numeric." : GoTo selesai
            End If
            ''nopack(2) As Integer
            'If (IsNumeric(dataRowPack(2)) = False) Then
            '    result(2) = "Pack Row : " & i & " - nopack required numeric." : GoTo selesai
            'End If
            'urutan(6) As Integer
            If (IsNumeric(dataRowPack(6)) = False) Then
                result(2) = "Pack Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'customdbl1(10) As Double
            If (IsNumeric(dataRowPack(10)) = False) Then
                result(2) = "Pack Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(11) As Double
            If (IsNumeric(dataRowPack(11)) = False) Then
                result(2) = "Pack Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(12) As Double
            If (IsNumeric(dataRowPack(12)) = False) Then
                result(2) = "Pack Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(13) As Date
            If (IsDate(dataRowPack(13)) = False) Then
                result(2) = "Pack Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(14) As Date
            If (IsDate(dataRowPack(14)) = False) Then
                result(2) = "Pack Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(15) As Date
            If (IsDate(dataRowPack(15)) = False) Then
                result(2) = "Pack Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA PACK -----------------------------------

            'VALIDASI DATA PACK ---------------------------------------
            'nopack(2) As String
            If Len(dataRowPack(2)) = 0 Then
                result(2) = "Pack Row : " & i & " - nopack can't be empty" : GoTo selesai
            End If
            If Len(dataRowPack(2)) > 25 Then
                result(2) = "Pack Row : " & i & " - nopack should not be more than 25 character." : GoTo selesai
            End If

            'customdbl1(10) As Double
            If Len(dataRowPack(10)) = 0 Then
                result(2) = "Pack Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(11) As Double
            If Len(dataRowPack(11)) = 0 Then
                result(2) = "Pack Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(12) As Double
            If Len(dataRowPack(12)) = 0 Then
                result(2) = "Pack Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(13) As Date
            If Len(dataRowPack(13)) = 0 Then
                result(2) = "Pack Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(14) As Date
            If Len(dataRowPack(14)) = 0 Then
                result(2) = "Pack Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(15) As Date
            If Len(dataRowPack(15)) = 0 Then
                result(2) = "Pack Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA PACK --------------------------------

            If AsDataTableTambahData(dtpack, "idplpack~idpl~nopack~catatan~bentuk~berat~urutan~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowPack(0) & "~" & dataRowPack(1) & "~" & dataRowPack(2) & "~" & dataRowPack(3) & "~" & dataRowPack(4) & "~" & dataRowPack(5) & "~" & dataRowPack(6) & "~" & dataRowPack(7) & "~" & dataRowPack(8) & "~" & dataRowPack(9) & "~" & dataRowPack(10) & "~" & dataRowPack(11) & "~" & dataRowPack(12) & "~" & dataRowPack(13) & "~" & dataRowPack(14) & "~" & dataRowPack(15)) = False Then
                result(2) = "Pack Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

        Next
        'END OF VALIDASI DAN SET ROW DATA PACK ===========================================


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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("pltgl")), AsFormatTanggal(drutama("pltgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("plstatus") = 2 Then
                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstandingSO, ftOutstandingSO, ftExistOutstandingPI, ftOutstandingPI, ftSO, ftPI, drutama("plhargatermasukpajak"))
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("pltermin").ToString, AsFormatTanggal(drutama("pltgl")), "pltgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("pltgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                ''PERHITUNGAN TOTAL UTAMA ================================
                ''DIAMBILKAN DARI DATA DETAIL

                ''TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                ''SUBTOTAL = (jml * harga) - jmldiskon
                'AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                'dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                ''TOTAL = subtotal
                'drutama("pltotal") = AsDataTableDSum(dtdetail, "subtotal")

                ''TOTALPAJAK1 = jmlpajak1
                'drutama("pltotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                ''TOTALPAJAK2 = jmlpajak2
                'drutama("pltotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                ''JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                ''JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'If Integer.Parse(drutama("plhargatermasukpajak")) = 0 Then
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                '    drutama("pltotaltransaksi") = Double.Parse(drutama("pltotal")) - Double.Parse(drutama("pljmldiskon")) + Double.Parse(drutama("pltotalpajak1detail")) + Double.Parse(drutama("pltotalpajak2detail")) + Double.Parse(drutama("plbiayalain"))

                'Else
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                '    drutama("pltotaltransaksi") = Double.Parse(drutama("pltotal")) - Double.Parse(drutama("pljmldiskon")) + Double.Parse(drutama("plbiayalain"))

                'End If
                ''END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("plid")
                    notransaksi = drutama("plnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(plid), plnotransaksi FROM M5_pl WHERE plid='" & result(4) & "' AND plstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(plid) FROM m5_pl WHERE plnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_pl_history
                        Dim rsSimpanHistory As String = SimpanHistory.M5_Pl_HistorySimpan("" & paramSplit(0) & "★M5_Pl_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("plsumber")) & "▼" & FixQuotes(drutama("plid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Pl set plcabang  = '" & FixQuotes(drutama("plcabang")) & "', pllokasi  = '" & FixQuotes(drutama("pllokasi")) & "', plgudang  = '" & FixQuotes(drutama("plgudang")) & "', plasalbarang  = '" & FixQuotes(drutama("plasalbarang")) & "', plasalbarangkategori  = " & drutama("plasalbarangkategori") & ", pljenispenjualan  = '" & FixQuotes(drutama("pljenispenjualan")) & "', pljenispenjualankategori  = " & drutama("pljenispenjualankategori") & ", plcarabayar  = " & drutama("plcarabayar") & ", plsumber  = '" & FixQuotes(drutama("plsumber")) & "', plautonotransaksi  = " & drutama("plautonotransaksi") & ", plnotransaksi  = '" & FixQuotes(notransaksi) & "', pltgl  = '" & FixQuotes(AsFormatTanggal(drutama("pltgl"))) & "', plkodepa  = " & drutama("plkodepa") & ", plcustomer  = " & drutama("plcustomer") & ", plcustomerkontak  = '" & FixQuotes(drutama("plcustomerkontak")) & "', pl1alamat1  = '" & FixQuotes(drutama("pl1alamat1")) & "', pl1alamat2  = '" & FixQuotes(drutama("pl1alamat2")) & "', pl1alamat3  = '" & FixQuotes(drutama("pl1alamat3")) & "', pl2alamat1  = '" & FixQuotes(drutama("pl2alamat1")) & "', pl2alamat2  = '" & FixQuotes(drutama("pl2alamat2")) & "', pl2alamat3  = '" & FixQuotes(drutama("pl2alamat3")) & "', plbagianpenjualan  = " & drutama("plbagianpenjualan") & ", plbagianpengepakan  = " & drutama("plbagianpengepakan") & ", plekspedisi  = '" & FixQuotes(drutama("plekspedisi")) & "', pltglkirim  = '" & FixQuotes(AsFormatTanggal(drutama("pltglkirim"))) & "', pltermin  = '" & FixQuotes(drutama("pltermin")) & "', pltgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("pltgljatuhtempo"))) & "', pluraian  = '" & FixQuotes(drutama("pluraian")) & "', plcatatan  = '" & FixQuotes(drutama("plcatatan")) & "', plnoref  = '" & FixQuotes(drutama("plnoref")) & "', pltglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("pltglnoref"))) & "', pltglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("pltglpenutupan"))) & "', plmatauang  = '" & FixQuotes(drutama("plmatauang")) & "', plkurs  = '" & FixDouble(drutama("plkurs")) & "', plhargatermasukpajak  = " & drutama("plhargatermasukpajak") & ", pltotal  = '" & FixDouble(drutama("pltotal")) & "', pldiskonpersen  = '" & FixQuotes(drutama("pldiskonpersen")) & "', pljmldiskon  = '" & FixDouble(drutama("pljmldiskon")) & "', pltotalpajak1detail  = '" & FixDouble(drutama("pltotalpajak1detail")) & "', pltotalpajak2detail  = '" & FixDouble(drutama("pltotalpajak2detail")) & "', plbiayalainpersen  = '" & FixDouble(drutama("plbiayalainpersen")) & "', plbiayalain  = '" & FixDouble(drutama("plbiayalain")) & "', pltotaltransaksi  = '" & FixDouble(drutama("pltotaltransaksi")) & "', plrekdiskon  = '" & FixQuotes(drutama("plrekdiskon")) & "', plrekpajak1  = '" & FixQuotes(drutama("plrekpajak1")) & "', plrekpajak2  = '" & FixQuotes(drutama("plrekpajak2")) & "', plrekbiayalain  = '" & FixQuotes(drutama("plrekbiayalain")) & "', plidsq  = " & drutama("plidsq") & ", plidso  = " & drutama("plidso") & ", plidpi  = " & drutama("plidpi") & ", plstatusdo  = " & drutama("plstatusdo") & ", plstatusdr  = " & drutama("plstatusdr") & ", plstatussi  = " & drutama("plstatussi") & ", plstatusrnr  = " & drutama("plstatusrnr") & ", plstatussr  = " & drutama("plstatussr") & ", plstatus  = " & drutama("plstatus") & ", plstatussebelumnya  = " & drutama("plstatussebelumnya") & ", pljmlrevisi  = pljmlrevisi+1, plcetakanke  = " & drutama("plcetakanke") & ", plmodifikasiuser  = " & drutama("plmodifikasiuser") & ", plmodifikasitgl  = NOW(), plcustomtext1  = '" & FixQuotes(drutama("plcustomtext1")) & "', plcustomtext2  = '" & FixQuotes(drutama("plcustomtext2")) & "', plcustomtext3  = '" & FixQuotes(drutama("plcustomtext3")) & "', plcustomtext4  = '" & FixQuotes(drutama("plcustomtext4")) & "', plcustomtext5  = '" & FixQuotes(drutama("plcustomtext5")) & "', plcustomint1  = " & drutama("plcustomint1") & ", plcustomint2  = " & drutama("plcustomint2") & ", plcustomint3  = " & drutama("plcustomint3") & ", plcustomdbl1  = '" & FixDouble(drutama("plcustomdbl1")) & "', plcustomdbl2  = '" & FixDouble(drutama("plcustomdbl2")) & "', plcustomdbl3  = '" & FixDouble(drutama("plcustomdbl3")) & "', plcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("plcustomdate1"))) & "', plcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("plcustomdate2"))) & "', plcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("plcustomdate3"))) & "' where plid = '" & drutama("plid") & "'"
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

                    If drutama("plautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("plcabang"), drutama("pllokasi"), drutama("plsumber"), drutama("pltgl"))
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
                        notransaksi = drutama("plnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(plid) FROM m5_pl WHERE plnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Pl (plcabang, pllokasi, plgudang, plasalbarang, plasalbarangkategori, pljenispenjualan, pljenispenjualankategori, plcarabayar, plsumber, plautonotransaksi, plnotransaksi, pltgl, plkodepa, plcustomer, plcustomerkontak, pl1alamat1, pl1alamat2, pl1alamat3, pl2alamat1, pl2alamat2, pl2alamat3, plbagianpenjualan, plbagianpengepakan, plekspedisi, pltglkirim, pltermin, pltgljatuhtempo, pluraian, plcatatan, plnoref, pltglnoref, pltglpenutupan, plmatauang, plkurs, plhargatermasukpajak, pltotal, pldiskonpersen, pljmldiskon, pltotalpajak1detail, pltotalpajak2detail, plbiayalainpersen, plbiayalain, pltotaltransaksi, plrekdiskon, plrekpajak1, plrekpajak2, plrekbiayalain, plidsq, plidso, plidpi, plstatusdo, plstatusdr, plstatussi, plstatusrnr, plstatussr, plstatus, plstatussebelumnya, pljmlrevisi, plcetakanke, plinputuser, plinputtgl, plmodifikasiuser, plmodifikasitgl, plisclose, plcustomtext1, plcustomtext2, plcustomtext3, plcustomtext4, plcustomtext5, plcustomint1, plcustomint2, plcustomint3, plcustomdbl1, plcustomdbl2, plcustomdbl3, plcustomdate1, plcustomdate2, plcustomdate3) values('" & FixQuotes(drutama("plcabang")) & "', '" & FixQuotes(drutama("pllokasi")) & "', '" & FixQuotes(drutama("plgudang")) & "', '" & FixQuotes(drutama("plasalbarang")) & "', " & drutama("plasalbarangkategori") & ", '" & FixQuotes(drutama("pljenispenjualan")) & "', " & drutama("pljenispenjualankategori") & ", " & drutama("plcarabayar") & ", '" & FixQuotes(drutama("plsumber")) & "', " & drutama("plautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("pltgl"))) & "', " & drutama("plkodepa") & ", " & drutama("plcustomer") & ", '" & FixQuotes(drutama("plcustomerkontak")) & "', '" & FixQuotes(drutama("pl1alamat1")) & "', '" & FixQuotes(drutama("pl1alamat2")) & "', '" & FixQuotes(drutama("pl1alamat3")) & "', '" & FixQuotes(drutama("pl2alamat1")) & "', '" & FixQuotes(drutama("pl2alamat2")) & "', '" & FixQuotes(drutama("pl2alamat3")) & "', " & drutama("plbagianpenjualan") & ", " & drutama("plbagianpengepakan") & ", '" & FixQuotes(drutama("plekspedisi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pltglkirim"))) & "', '" & FixQuotes(drutama("pltermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pltgljatuhtempo"))) & "', '" & FixQuotes(drutama("pluraian")) & "', '" & FixQuotes(drutama("plcatatan")) & "', '" & FixQuotes(drutama("plnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pltglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("pltglpenutupan"))) & "', '" & FixQuotes(drutama("plmatauang")) & "', '" & FixDouble(drutama("plkurs")) & "', " & drutama("plhargatermasukpajak") & ", '" & FixDouble(drutama("pltotal")) & "', '" & FixQuotes(drutama("pldiskonpersen")) & "', '" & FixDouble(drutama("pljmldiskon")) & "', '" & FixDouble(drutama("pltotalpajak1detail")) & "', '" & FixDouble(drutama("pltotalpajak2detail")) & "', '" & FixDouble(drutama("plbiayalainpersen")) & "', '" & FixDouble(drutama("plbiayalain")) & "', '" & FixDouble(drutama("pltotaltransaksi")) & "', '" & FixQuotes(drutama("plrekdiskon")) & "', '" & FixQuotes(drutama("plrekpajak1")) & "', '" & FixQuotes(drutama("plrekpajak2")) & "', '" & FixQuotes(drutama("plrekbiayalain")) & "', " & drutama("plidsq") & ", " & drutama("plidso") & ", " & drutama("plidpi") & ", " & drutama("plstatusdo") & ", " & drutama("plstatusdr") & ", " & drutama("plstatussi") & ", " & drutama("plstatusrnr") & ", " & drutama("plstatussr") & ", " & drutama("plstatus") & ", " & drutama("plstatussebelumnya") & ", " & drutama("pljmlrevisi") & ", " & drutama("plcetakanke") & ", " & drutama("plinputuser") & ", NOW(), " & drutama("plmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("plisclose") & ", '" & FixQuotes(drutama("plcustomtext1")) & "', '" & FixQuotes(drutama("plcustomtext2")) & "', '" & FixQuotes(drutama("plcustomtext3")) & "', '" & FixQuotes(drutama("plcustomtext4")) & "', '" & FixQuotes(drutama("plcustomtext5")) & "', " & drutama("plcustomint1") & ", " & drutama("plcustomint2") & ", " & drutama("plcustomint3") & ", '" & FixDouble(drutama("plcustomdbl1")) & "', '" & FixDouble(drutama("plcustomdbl2")) & "', '" & FixDouble(drutama("plcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("plcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("plcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("plcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select plid from M5_pl where plnotransaksi='" & notransaksi & "' AND plinputuser= '" & userid & "' order by plmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Pl_Detail where idpl = '" & result(4) & "'"
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
                        If Not drutama("plmatauang").ToString.Equals(dr1("matauang").ToString) Then
                            result(2) = "Row : " & dr1("urutan") & " - " & dr1("tipebarang") & " | " & dr1("namabarang") & " currency (" & dr1("matauang") & ") doesn't belong to the main transactions." : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF VALIDASI MATAUANG HARUS SAMA DENGAN TRANSAKSI UTAMA -------------


                        'SET HARGA DARI TRANSAKSI SEBELUMNYA ------------------------------------
                        If Double.Parse(dr1("idpidetail")) > 0 Then
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
                        strValue2.Append("(" & dr1("idpldetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & dr1("nopack") & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idsqdetail") & ", " & dr1("idsodetail") & ", " & dr1("idpidetail") & ", '" & FixDouble(dr1("jmldo")) & "', " & dr1("statusdo") & ", '" & FixDouble(dr1("jmldr")) & "', " & dr1("statusdr") & ", '" & FixDouble(dr1("jmlsi")) & "', " & dr1("statussi") & ", '" & FixDouble(dr1("jmlrnr")) & "', " & dr1("statusrnr") & ", '" & FixDouble(dr1("jmlsr")) & "', " & dr1("statussr") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M5_Pl_Detail(idpldetail, idpl, idbarang, namabarang, tipebarang, nopack, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpidetail, jmldo, statusdo, jmldr, statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                'Hapus pack ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Pl_Pack where idpl = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses pack
                If (dtpack.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtpack.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idplpack") & ", " & result(4) & ", '" & dr1("nopack") & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("bentuk")) & "', '" & FixQuotes(dr1("berat")) & "', " & dr1("urutan") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M5_Pl_Pack(idplpack, idpl, nopack, catatan, bentuk, berat, urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Else
                    result(2) = "Pack Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'UPDATE OUTSTANDING TRANSAKSI ==========================================================
                If drutama("plstatus") = 2 Then
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
                End If
                'END OF UPDATE OUTSTANDING TRANSAKSI ================================================

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "PL", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M5_PlUpdateStatusOld(ByVal param As String) As String

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
            Dim sumber As String = "Pl", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Pltgl, Plnotransaksi, Plstatus FROM M5_Pl WHERE Plid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Plstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m5_pl_history
            Dim rsSimpanHistory As String = SimpanHistory.M5_Pl_HistorySimpan("" & paramSplit(0) & "★M5_Pl_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m5_pl_terkait("plid = '" & idtransaksi & "'")
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idsodetail As Integer = 0, idpidetail As Integer = 0
                Dim updNilaiSO As String = "", updFilterSO As String = "", updNilaiPI As String = "", updFilterPI As String = ""
                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDB("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idsodetail, idpidetail, urutan FROM m5_pl_detail WHERE idpl = '" & idtransaksi & "'")
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : idsodetail = dr1("idsodetail") : idpidetail = dr1("idpidetail")

                        'UPDATE OUTSTANDING ---------------------------
                        If idsodetail <> 0 Then
                            If idpidetail = 0 Then
                                '1. SET NILAI UPDATE OUTSTANDING SO
                                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsodetail=" & idsodetail)
                                updNilaiSO = String.Concat("WHEN '" & idsodetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiSO)

                                '2. SET FILTERUPDATE OUTSTANDING SO
                                updFilterSO = IIf(Len(updFilterSO.ToString) = 0, "", updFilterSO & " OR ")
                                updFilterSO = String.Concat(updFilterSO, "(idsodetail = '" & idsodetail & "')")
                            End If
                        End If

                        If idpidetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING PI
                            Dim Outstandingpi As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idpidetail=" & idpidetail)
                            updNilaiPI = String.Concat("WHEN '" & idpidetail & "' THEN ROUND(jmlrealisasi - '" & Outstandingpi & "', 5) ", updNilaiPI)

                            '2. SET FILTERUPDATE OUTSTANDING PI
                            updFilterPI = IIf(Len(updFilterPI.ToString) = 0, "", updFilterPI & " OR ")
                            updFilterPI = String.Concat(updFilterPI, "(idpidetail = '" & idpidetail & "')")
                        End If
                        'END OF BUAT FILTER UNTUK UPDATE --------------------------
                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                'UPDATE OUTSTANDING TRANSAKSI ====================================================
                If Len(updFilterSO) > 0 Then 'SO
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
                End If

                If Len(updFilterPI) > 0 Then 'PI
                    'UPDATE OUTSTANDING DETAIL -------------------
                    sql = "UPDATE m5_pi_detail SET jmlrealisasi = (CASE idpidetail " & updNilaiPI & " ELSE jmlrealisasi END) WHERE " & updFilterPI
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
                'END OF UPDATE OUTSTANDING TRANSAKSI =============================================

            End If

            'update status utama
            sql = "UPDATE M5_Pl SET Plstatus = " & nilaiStatus & ", Plmodifikasiuser='" & userid & "', Plmodifikasitgl = NOW(), Plposting = 0, Plpostingtgl = '1971-01-01 00:00:00', Pljmlrevisi = Pljmlrevisi + 1 WHERE Plid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_PlSearch(PostWsSearch(paramSplit(0), "M5_PlSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M5_PlDeleteOld(ByVal param As String) As String

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
            Dim sumber As String = "Pl", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Plid, Plnotransaksi FROM M5_Pl WHERE Plid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT plcabang, pllokasi, plsumber, plautonotransaksi, plnotransaksi, pltgl"
            sql &= " FROM M5_pl"
            sql &= " WHERE plid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("plcabang")
                lokasi = dtNomorNext.Rows(0)("pllokasi")
                sumber = dtNomorNext.Rows(0)("plsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("plautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("plnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("pltgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE PACK
            sql = "DELETE FROM M5_Pl_Pack WHERE idpl ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M5_Pl_Detail WHERE idpl ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M5_Pl WHERE plid ='" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_PlSearch(PostWsSearch(paramSplit(0), "M5_PlSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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