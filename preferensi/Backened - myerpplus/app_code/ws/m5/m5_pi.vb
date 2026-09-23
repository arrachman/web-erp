Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_pi
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_PiSimpan(ByVal param As String) As String
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
        'piid(0) As Integer, picabang(1) As String, pilokasi(2) As String, pigudang(3) As String, piasalbarang(4) As String, 
        'piasalbarangkategori(5) As Integer, pijenispenjualan(6) As String, pijenispenjualankategori(7) As Integer, picarabayar(8) As Integer, pisumber(9) As String, 
        'piautonotransaksi(10) As Integer, pinotransaksi(11) As String, pitgl(12) As Date, pikodepa(13) As Integer, picustomer(14) As Integer, 
        'picustomerkontak(15) As String, pi1alamat1(16) As String, pi1alamat2(17) As String, pi1alamat3(18) As String, pi2alamat1(19) As String, 
        'pi2alamat2(20) As String, pi2alamat3(21) As String, pibagianpenjualan(22) As Integer, piekspedisi(23) As String, pitglkirim(24) As Date, 
        'pitermin(25) As String, pitgljatuhtempo(26) As Date, piuraian(27) As String, picatatan(28) As String, pinoref(29) As String, 
        'pitglnoref(30) As Date, pitglpenutupan(31) As Date, pimatauang(32) As String, pikurs(33) As Double, pihargatermasukpajak(34) As Integer, 
        'pitotal(35) As Double, pidiskonpersen(36) As String, pijmldiskon(37) As Double, pitotalpajak1detail(38) As Double, pitotalpajak2detail(39) As Double, 
        'pibiayalainpersen(40) As Double, pibiayalain(41) As Double, pitotaltransaksi(42) As Double, pijmlbayar(43) As Double, pirekdiskon(44) As String, 
        'pirekpajak1(45) As String, pirekpajak2(46) As String, pirekbiayalain(47) As String, pirekbayar(48) As String, piidsq(49) As Integer, 
        'piidso(50) As Integer, pistatuspl(51) As Integer, pistatusdo(52) As Integer, pistatusdr(53) As Integer, pistatussi(54) As Integer, 
        'pistatusrnr(55) As Integer, pistatussr(56) As Integer, pistatus(57) As Integer, pistatussebelumnya(58) As Integer, pijmlrevisi(59) As Integer, 
        'picetakanke(60) As Integer, piinputuser(61) As Integer, piinputtgl(62) As DateTime, pimodifikasiuser(63) As Integer, pimodifikasitgl(64) As DateTime, 
        'piisclose(65) As Integer, pitutupperiode(66) As Integer, picustomtext1(67) As String, picustomtext2(68) As String, picustomtext3(69) As String, 
        'picustomtext4(70) As String, picustomtext5(71) As String, picustomint1(72) As Integer, picustomint2(73) As Integer, picustomint3(74) As Integer, 
        'picustomdbl1(75) As Double, picustomdbl2(76) As Double, picustomdbl3(77) As Double, picustomdate1(78) As Date, picustomdate2(79) As Date, 
        'picustomdate3(80) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'piid, picabang, pilokasi, pigudang, piasalbarang, piasalbarangkategori, pijenispenjualan, 
        'pijenispenjualankategori, picarabayar, pisumber, piautonotransaksi, pinotransaksi, pitgl, pikodepa, 
        'picustomer, picustomerkontak, pi1alamat1, pi1alamat2, pi1alamat3, pi2alamat1, pi2alamat2, 
        'pi2alamat3, pibagianpenjualan, piekspedisi, pitglkirim, pitermin, pitgljatuhtempo, piuraian, 
        'picatatan, pinoref, pitglnoref, pitglpenutupan, pimatauang, pikurs, pihargatermasukpajak, 
        'pitotal, pidiskonpersen, pijmldiskon, pitotalpajak1detail, pitotalpajak2detail, pibiayalainpersen, pibiayalain, 
        'pitotaltransaksi, pijmlbayar, pirekdiskon, pirekpajak1, pirekpajak2, pirekbiayalain, pirekbayar, 
        'piidsq, piidso, pistatuspl, pistatusdo, pistatusdr, pistatussi, pistatusrnr, 
        'pistatussr, pistatus, pistatussebelumnya, pijmlrevisi, picetakanke, piinputuser, piinputtgl, 
        'pimodifikasiuser, pimodifikasitgl, piisclose, pitutupperiode, picustomtext1, picustomtext2, picustomtext3, 
        'picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, picustomdbl1, picustomdbl2, 
        'picustomdbl3, picustomdate1, picustomdate2, picustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 81) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'piid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "piid required numeric." : GoTo selesai
        End If
        'piasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "piasalbarangkategori required numeric." : GoTo selesai
        End If
        'pijenispenjualankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "pijenispenjualankategori required numeric." : GoTo selesai
        End If
        'picarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "picarabayar required numeric." : GoTo selesai
        End If
        'piautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "piautonotransaksi required numeric." : GoTo selesai
        End If
        'pitgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "pitgl required date." : GoTo selesai
        End If
        'pikodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "pikodepa required numeric." : GoTo selesai
        End If
        'picustomer(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "picustomer required numeric." : GoTo selesai
        End If
        'pibagianpenjualan(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "pibagianpenjualan required numeric." : GoTo selesai
        End If
        'pitglkirim(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "pitglkirim required date." : GoTo selesai
        End If
        'pitgljatuhtempo(26) As Date
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "pitgljatuhtempo required date." : GoTo selesai
        End If
        'pitglnoref(30) As Date
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "pitglnoref required date." : GoTo selesai
        End If
        'pitglpenutupan(31) As Date
        If (IsDate(dataUtama(31)) = False) Then
            result(2) = "pitglpenutupan required date." : GoTo selesai
        End If
        'pikurs(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "pikurs required numeric." : GoTo selesai
        End If
        'pihargatermasukpajak(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "pihargatermasukpajak required numeric." : GoTo selesai
        End If
        'pitotal(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "pitotal required numeric." : GoTo selesai
        End If
        'pijmldiskon(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "pijmldiskon required numeric." : GoTo selesai
        End If
        'pitotalpajak1detail(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "pitotalpajak1detail required numeric." : GoTo selesai
        End If
        'pitotalpajak2detail(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "pitotalpajak2detail required numeric." : GoTo selesai
        End If
        ''pibiayalainpersen(40) As Double
        'If (IsNumeric(dataUtama(40)) = False) Then
        '    result(2) = "pibiayalainpersen required numeric." : GoTo selesai
        'End If
        'pibiayalain(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "pibiayalain required numeric." : GoTo selesai
        End If
        'pitotaltransaksi(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "pitotaltransaksi required numeric." : GoTo selesai
        End If
        'pijmlbayar(43) As Double
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "pijmlbayar required numeric." : GoTo selesai
        End If
        'piidsq(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "piidsq required numeric." : GoTo selesai
        End If
        'piidso(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "piidso required numeric." : GoTo selesai
        End If
        'pistatuspl(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "pistatuspl required numeric." : GoTo selesai
        End If
        'pistatusdo(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "pistatusdo required numeric." : GoTo selesai
        End If
        'pistatusdr(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "pistatusdr required numeric." : GoTo selesai
        End If
        'pistatussi(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "pistatussi required numeric." : GoTo selesai
        End If
        'pistatusrnr(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "pistatusrnr required numeric." : GoTo selesai
        End If
        'pistatussr(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "pistatussr required numeric." : GoTo selesai
        End If
        'pistatus(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "pistatus required numeric." : GoTo selesai
        End If
        'pistatussebelumnya(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "pistatussebelumnya required numeric." : GoTo selesai
        End If
        'pijmlrevisi(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "pijmlrevisi required numeric." : GoTo selesai
        End If
        'picetakanke(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "picetakanke required numeric." : GoTo selesai
        End If
        'piinputuser(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "piinputuser required numeric." : GoTo selesai
        End If
        'piinputtgl(62) As DateTime
        If (IsDate(dataUtama(62)) = False) Then
            result(2) = "piinputtgl required date." : GoTo selesai
        End If
        'pimodifikasiuser(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "pimodifikasiuser required numeric." : GoTo selesai
        End If
        'pimodifikasitgl(64) As DateTime
        If (IsDate(dataUtama(64)) = False) Then
            result(2) = "pimodifikasitgl required date." : GoTo selesai
        End If
        'piisclose(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "piisclose required numeric." : GoTo selesai
        End If
        'pitutupperiode(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "pitutupperiode required numeric." : GoTo selesai
        End If
        'picustomint1(72) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "picustomint1 required numeric." : GoTo selesai
        End If
        'picustomint2(73) As Integer
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "picustomint2 required numeric." : GoTo selesai
        End If
        'picustomint3(74) As Integer
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "picustomint3 required numeric." : GoTo selesai
        End If
        'picustomdbl1(75) As Double
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "picustomdbl1 required numeric." : GoTo selesai
        End If
        'picustomdbl2(76) As Double
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "picustomdbl2 required numeric." : GoTo selesai
        End If
        'picustomdbl3(77) As Double
        If (IsNumeric(dataUtama(77)) = False) Then
            result(2) = "picustomdbl3 required numeric." : GoTo selesai
        End If
        'picustomdate1(78) As Date
        If (IsDate(dataUtama(78)) = False) Then
            result(2) = "picustomdate1 required date." : GoTo selesai
        End If
        'picustomdate2(79) As Date
        If (IsDate(dataUtama(79)) = False) Then
            result(2) = "picustomdate2 required date." : GoTo selesai
        End If
        'picustomdate3(80) As Date
        If (IsDate(dataUtama(80)) = False) Then
            result(2) = "picustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'picabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "picabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "picabang should not be more than 25 character." : GoTo selesai
        End If

        'pilokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "pilokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "pilokasi should not be more than 25 character." : GoTo selesai
        End If

        'pigudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "pigudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "pigudang should not be more than 25 character." : GoTo selesai
        End If

        'pisumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "pisumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "pisumber should not be more than 10 character." : GoTo selesai
        End If

        'pinotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "pinotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "pinotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'pitgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "pitgl can't be empty" : GoTo selesai
        End If

        'pitglkirim(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "pitglkirim can't be empty" : GoTo selesai
        End If

        'pitgljatuhtempo(26) As Date
        If Len(dataUtama(26)) = 0 Then
            result(2) = "pitgljatuhtempo can't be empty" : GoTo selesai
        End If

        'pitglnoref(30) As Date
        If Len(dataUtama(30)) = 0 Then
            result(2) = "pitglnoref can't be empty" : GoTo selesai
        End If

        'pitglpenutupan(31) As Date
        If Len(dataUtama(31)) = 0 Then
            result(2) = "pitglpenutupan can't be empty" : GoTo selesai
        End If

        'pimatauang(32) As String
        If Len(dataUtama(32)) = 0 Then
            result(2) = "pimatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(32)) > 25 Then
            result(2) = "pimatauang should not be more than 25 character." : GoTo selesai
        End If

        'pikurs(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "pikurs can't be empty" : GoTo selesai
        End If

        'pitotal(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "pitotal can't be empty" : GoTo selesai
        End If

        'pidiskonpersen(36) As String
        If Len(dataUtama(36)) = 0 Then
            result(2) = "pidiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(36)) > 25 Then
            result(2) = "pidiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'pijmldiskon(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "pijmldiskon can't be empty" : GoTo selesai
        End If

        'pitotalpajak1detail(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "pitotalpajak1detail can't be empty" : GoTo selesai
        End If

        'pitotalpajak2detail(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "pitotalpajak2detail can't be empty" : GoTo selesai
        End If

        'pibiayalainpersen(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "pibiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(40)) > 25 Then
            result(2) = "pibiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'pibiayalain(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "pibiayalain can't be empty" : GoTo selesai
        End If

        'pitotaltransaksi(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "pitotaltransaksi can't be empty" : GoTo selesai
        End If

        'pijmlbayar(43) As Double
        If Len(dataUtama(43)) = 0 Then
            result(2) = "pijmlbayar can't be empty" : GoTo selesai
        End If

        'piinputtgl(62) As DateTime
        If Len(dataUtama(62)) = 0 Then
            result(2) = "piinputtgl can't be empty" : GoTo selesai
        End If

        'pimodifikasitgl(64) As DateTime
        If Len(dataUtama(64)) = 0 Then
            result(2) = "pimodifikasitgl can't be empty" : GoTo selesai
        End If

        'picustomdbl1(75) As Double
        If Len(dataUtama(75)) = 0 Then
            result(2) = "picustomdbl1 can't be empty" : GoTo selesai
        End If

        'picustomdbl2(76) As Double
        If Len(dataUtama(76)) = 0 Then
            result(2) = "picustomdbl2 can't be empty" : GoTo selesai
        End If

        'picustomdbl3(77) As Double
        If Len(dataUtama(77)) = 0 Then
            result(2) = "picustomdbl3 can't be empty" : GoTo selesai
        End If

        'picustomdate1(78) As Date
        If Len(dataUtama(78)) = 0 Then
            result(2) = "picustomdate1 can't be empty" : GoTo selesai
        End If

        'picustomdate2(79) As Date
        If Len(dataUtama(79)) = 0 Then
            result(2) = "picustomdate2 can't be empty" : GoTo selesai
        End If

        'picustomdate3(80) As Date
        If Len(dataUtama(80)) = 0 Then
            result(2) = "picustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "piid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "picabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pilokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pigudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pijenispenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pijenispenjualankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "picarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pisumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pinotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pikodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "picustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "picustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pi1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pi1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pi1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pi2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pi2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pi2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pibagianpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piekspedisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pitglkirim", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pitermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pitgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "picatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pinoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pitglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pitglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pimatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pikurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pihargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pitotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pidiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pijmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pitotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pitotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pibiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pibiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pitotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pijmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pirekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pirekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pirekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pirekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pirekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piidsq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piidso", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pistatuspl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pistatusdo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pistatusdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pistatussi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pistatusrnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pistatussr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pistatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pistatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pijmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "picetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pimodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pimodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pitutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "picustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "picustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "picustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "picustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "picustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "picustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "picustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "picustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "picustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "picustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "picustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "picustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "picustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "picustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "piid~picabang~pilokasi~pigudang~piasalbarang~piasalbarangkategori~pijenispenjualan~pijenispenjualankategori~picarabayar~pisumber~piautonotransaksi~pinotransaksi~pitgl~pikodepa~picustomer~picustomerkontak~pi1alamat1~pi1alamat2~pi1alamat3~pi2alamat1~pi2alamat2~pi2alamat3~pibagianpenjualan~piekspedisi~pitglkirim~pitermin~pitgljatuhtempo~piuraian~picatatan~pinoref~pitglnoref~pitglpenutupan~pimatauang~pikurs~pihargatermasukpajak~pitotal~pidiskonpersen~pijmldiskon~pitotalpajak1detail~pitotalpajak2detail~pibiayalainpersen~pibiayalain~pitotaltransaksi~pijmlbayar~pirekdiskon~pirekpajak1~pirekpajak2~pirekbiayalain~pirekbayar~piidsq~piidso~pistatuspl~pistatusdo~pistatusdr~pistatussi~pistatusrnr~pistatussr~pistatus~pistatussebelumnya~pijmlrevisi~picetakanke~piinputuser~piinputtgl~pimodifikasiuser~pimodifikasitgl~piisclose~pitutupperiode~picustomtext1~picustomtext2~picustomtext3~picustomtext4~picustomtext5~picustomint1~picustomint2~picustomint3~picustomdbl1~picustomdbl2~picustomdbl3~picustomdate1~picustomdate2~picustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idpidetail(0) As Integer, idpi(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, diskon(13) As String, jmldiskon(14) As Double, 
        'pajak1(15) As String, jmlpajak1(16) As Double, pajak2(17) As String, jmlpajak2(18) As Double, cabang(19) As String, 
        'lokasi(20) As String, gudang(21) As String, costcenter(22) As String, divisi(23) As String, subdivisi(24) As String, 
        'proyek(25) As String, catatan(26) As String, urutan(27) As Integer, idsqdetail(28) As Integer, idsodetail(29) As Integer, 
        'jmlpl(30) As Double, statuspl(31) As Integer, jmldo(32) As Double, statusdo(33) As Integer, jmldr(34) As Double, 
        'statusdr(35) As Integer, jmlsi(36) As Double, statussi(37) As Integer, jmlrnr(38) As Double, statusrnr(39) As Integer, 
        'jmlsr(40) As Double, statussr(41) As Integer, isclose(42) As Integer, customtext1(43) As String, customtext2(44) As String, 
        'customtext3(45) As String, customdbl1(46) As Double, customdbl2(47) As Double, customdbl3(48) As Double, customdate1(49) As Date, 
        'customdate2(50) As Date, customdate3(51) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idpidetail, idpi, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, 
        'jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, 
        'gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idsqdetail, idsodetail, jmlpl, statuspl, jmldo, statusdo, jmldr, 
        'statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idpidetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpi", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "idsqdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idsodetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlpl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuspl", AsEnumTypeData.AsInt64)
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
        Dim ftExistOutstanding As String = "", ftOutstanding As String = ""
        Dim updNilai As String = "", updFilter As String = ""
        Dim idbarang As Integer = 0, idsodetail As Integer = 0, jmlbarang As Double = 0

        'Validasi Harga dibawah harga jual
        Dim ftLowerPrice As String = "", kurs As Double = 0, harga As Double = 0

        'FILTER SO, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftSO As String = ""

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
            'idpidetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idpidetail required numeric." : GoTo selesai
            End If
            'idpi(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idpi required numeric." : GoTo selesai
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
            'idsqdetail(28) As Integer
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - idsqdetail required numeric." : GoTo selesai
            End If
            'idsodetail(29) As Integer
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - idsodetail required numeric." : GoTo selesai
            End If
            'jmlpl(30) As Double
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - jmlpl required numeric." : GoTo selesai
            End If
            'statuspl(31) As Integer
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - statuspl required numeric." : GoTo selesai
            End If
            'jmldo(32) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - jmldo required numeric." : GoTo selesai
            End If
            'statusdo(33) As Integer
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - statusdo required numeric." : GoTo selesai
            End If
            'jmldr(34) As Double
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - jmldr required numeric." : GoTo selesai
            End If
            'statusdr(35) As Integer
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - statusdr required numeric." : GoTo selesai
            End If
            'jmlsi(36) As Double
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - jmlsi required numeric." : GoTo selesai
            End If
            'statussi(37) As Integer
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - statussi required numeric." : GoTo selesai
            End If
            'jmlrnr(38) As Double
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - jmlrnr required numeric." : GoTo selesai
            End If
            'statusrnr(39) As Integer
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - statusrnr required numeric." : GoTo selesai
            End If
            'jmlsr(40) As Double
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - jmlsr required numeric." : GoTo selesai
            End If
            'statussr(41) As Integer
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - statussr required numeric." : GoTo selesai
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

            'jmlpl(30) As Double
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Row : " & i & " - jmlpl can't be empty" : GoTo selesai
            End If

            'jmldo(32) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - jmldo can't be empty" : GoTo selesai
            End If

            'jmldr(34) As Double
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Row : " & i & " - jmldr can't be empty" : GoTo selesai
            End If

            'jmlsi(36) As Double
            If Len(dataRowDetail(36)) = 0 Then
                result(2) = "Row : " & i & " - jmlsi can't be empty" : GoTo selesai
            End If

            'jmlrnr(38) As Double
            If Len(dataRowDetail(38)) = 0 Then
                result(2) = "Row : " & i & " - jmlrnr can't be empty" : GoTo selesai
            End If

            'jmlsr(40) As Double
            If Len(dataRowDetail(40)) = 0 Then
                result(2) = "Row : " & i & " - jmlsr can't be empty" : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idpidetail~idpi~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~idsqdetail~idsodetail~jmlpl~statuspl~jmldo~statusdo~jmldr~statusdr~jmlsi~statussi~jmlrnr~statusrnr~jmlsr~statussr~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , idsodetail(29) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : idsodetail = dataRowDetail(29)
            'kurs(11) As Double                    , harga(12) As Double
            kurs = Double.Parse(dataRowDetail(11)) : harga = Double.Parse(dataRowDetail(12))

            'VALIDASI OUTSTANDING -------------------------
            If idsodetail <> 0 Then 'SO
                'CEK SO YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftSO = IIf(Len(ftSO.ToString) = 0, "", ftSO & " OR ")
                ftSO = String.Concat(ftSO, " (sod.idsodetail = " & idsodetail & ") ")

                '1. CEK DATA EXIST ------------------------
                ftExistOutstanding = IIf(Len(ftExistOutstanding.ToString) = 0, "", ftExistOutstanding & " UNION ")
                'ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m5_so_detail JOIN m5_so ON idso = soid WHERE idsodetail = '" & idsodetail & "' AND (sostatus = 2 OR sostatus = 3 OR sostatus = 4 OR sostatus = 7) LIMIT 1) as rowExists, '" & idsodetail & "' as idsodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
                'ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m5_so_detail JOIN m5_so ON idso = soid WHERE idsodetail = '" & idsodetail & "' AND (sostatus = 2 OR sostatus = 3) LIMIT 1) as rowExists, '" & idsodetail & "' as idsodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
                ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m5_so_detail JOIN m5_so ON idso = soid WHERE idsodetail = '" & idsodetail & "' AND (sostatus = 2 OR sostatus = 3 OR sostatus = 4) LIMIT 1) as rowExists, '" & idsodetail & "' as idsodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsodetail=" & idsodetail)
                ftOutstanding = IIf(Len(ftOutstanding.ToString) = 0, "", ftOutstanding & " OR ")
                ftOutstanding = String.Concat(ftOutstanding, " (sod.idsodetail = " & idsodetail & " AND " & Outstanding & " > (sod.jmlbarang - sod.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilai = String.Concat("WHEN '" & idsodetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilai)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                updFilter = String.Concat(updFilter, "(idsodetail = '" & idsodetail & "')")
            End If

            'Validasi harga dibawah harga jual
            ftLowerPrice = IIf(Len(ftLowerPrice.ToString) = 0, "", ftLowerPrice & " OR ")
            ftLowerPrice = String.Concat(ftLowerPrice, "(bid = '" & idbarang & "' AND bhargajual1 > " & FixDouble(harga * kurs) & ")")
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
                Dim vModuleId As Integer = 5, vMenuId As Integer = 9
                Select Case drutama("pistatus")
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("pitgl")), AsFormatTanggal(drutama("pitgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                If drutama("pistatus") = 2 Or drutama("pistatus") = 1 Or drutama("pistatus") = 8 Or drutama("pistatus") = 9 Or drutama("pistatus") = 10 Or drutama("pistatus") = 11 Then
                    'VALIDASI HAK AKSES PENJUALAN DIBAWAH HARGA JUAL
                    '0 = Insert, 1 = Update/Draft, 2 = Delete, 3 = GetData, 4 = Approved1, 5 = Approved2, 6 = Approved3, 
                    '7 = Approved4, 8 = Approved, 9 = Close/Unclose, 10 = Journal, 11 = History, 12 = Setting Grid
                    Dim rsHakAksesLowerPrice As String = HakAksesLowerPrice(5, 10, 8, userid, dtdetail, ftLowerPrice) 'MODULEID, MENUID, INDEKS AKSES, USERID, DATA DETAIL, FILTER BARANG SESUAI TRANSAKSI
                    If Len(rsHakAksesLowerPrice) <> 0 Then result(2) = rsHakAksesLowerPrice : Trans.Rollback() : GoTo selesai

                    'ValidasiSimpan
                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstanding, ftOutstanding, ftSO, drutama("pihargatermasukpajak"))
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("pitermin").ToString, AsFormatTanggal(drutama("pitgl")), "aptgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("pitgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TOTAL = subtotal
                drutama("pitotal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("pitotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("pitotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                If Integer.Parse(drutama("pihargatermasukpajak")) = 0 Then
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                    drutama("pitotaltransaksi") = Double.Parse(drutama("pitotal")) - Double.Parse(drutama("pijmldiskon")) + Double.Parse(drutama("pitotalpajak1detail")) + Double.Parse(drutama("pitotalpajak2detail")) + Double.Parse(drutama("pibiayalain"))

                Else
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                    drutama("pitotaltransaksi") = Double.Parse(drutama("pitotal")) - Double.Parse(drutama("pijmldiskon")) + Double.Parse(drutama("pitotalpajak2detail")) + Double.Parse(drutama("pibiayalain"))

                End If
                'END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("piid")
                    notransaksi = drutama("pinotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(piid), pinotransaksi FROM M5_pi WHERE piid='" & result(4) & "' AND pistatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("piautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("picabang"), drutama("pilokasi"), drutama("pisumber"), drutama("pitgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(piid) FROM m5_pi WHERE pinotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_pi_history
                        Dim rsSimpanHistory As String = SimpanHistory.M5_Pi_HistorySimpan("" & paramSplit(0) & "★M5_Pi_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("pisumber")) & "▼" & FixQuotes(drutama("piid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Pi set picabang  = '" & FixQuotes(drutama("picabang")) & "', pilokasi  = '" & FixQuotes(drutama("pilokasi")) & "', pigudang  = '" & FixQuotes(drutama("pigudang")) & "', piasalbarang  = '" & FixQuotes(drutama("piasalbarang")) & "', piasalbarangkategori  = " & drutama("piasalbarangkategori") & ", pijenispenjualan  = '" & FixQuotes(drutama("pijenispenjualan")) & "', pijenispenjualankategori  = " & drutama("pijenispenjualankategori") & ", picarabayar  = " & drutama("picarabayar") & ", pisumber  = '" & FixQuotes(drutama("pisumber")) & "', piautonotransaksi  = " & drutama("piautonotransaksi") & ", pinotransaksi  = '" & FixQuotes(notransaksi) & "', pitgl  = '" & FixQuotes(AsFormatTanggal(drutama("pitgl"))) & "', pikodepa  = " & drutama("pikodepa") & ", picustomer  = " & drutama("picustomer") & ", picustomerkontak  = '" & FixQuotes(drutama("picustomerkontak")) & "', pi1alamat1  = '" & FixQuotes(drutama("pi1alamat1")) & "', pi1alamat2  = '" & FixQuotes(drutama("pi1alamat2")) & "', pi1alamat3  = '" & FixQuotes(drutama("pi1alamat3")) & "', pi2alamat1  = '" & FixQuotes(drutama("pi2alamat1")) & "', pi2alamat2  = '" & FixQuotes(drutama("pi2alamat2")) & "', pi2alamat3  = '" & FixQuotes(drutama("pi2alamat3")) & "', pibagianpenjualan  = " & drutama("pibagianpenjualan") & ", piekspedisi  = '" & FixQuotes(drutama("piekspedisi")) & "', pitglkirim  = '" & FixQuotes(AsFormatTanggal(drutama("pitglkirim"))) & "', pitermin  = '" & FixQuotes(drutama("pitermin")) & "', pitgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("pitgljatuhtempo"))) & "', piuraian  = '" & FixQuotes(drutama("piuraian")) & "', picatatan  = '" & FixQuotes(drutama("picatatan")) & "', pinoref  = '" & FixQuotes(drutama("pinoref")) & "', pitglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("pitglnoref"))) & "', pitglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("pitglpenutupan"))) & "', pimatauang  = '" & FixQuotes(drutama("pimatauang")) & "', pikurs  = '" & FixDouble(drutama("pikurs")) & "', pihargatermasukpajak  = " & drutama("pihargatermasukpajak") & ", pitotal  = '" & FixDouble(drutama("pitotal")) & "', pidiskonpersen  = '" & FixQuotes(drutama("pidiskonpersen")) & "', pijmldiskon  = '" & FixDouble(drutama("pijmldiskon")) & "', pitotalpajak1detail  = '" & FixDouble(drutama("pitotalpajak1detail")) & "', pitotalpajak2detail  = '" & FixDouble(drutama("pitotalpajak2detail")) & "', pibiayalainpersen  = '" & FixDouble(drutama("pibiayalainpersen")) & "', pibiayalain  = '" & FixDouble(drutama("pibiayalain")) & "', pitotaltransaksi  = '" & FixDouble(drutama("pitotaltransaksi")) & "', pijmlbayar  = '" & FixDouble(drutama("pijmlbayar")) & "', pirekdiskon  = '" & FixQuotes(drutama("pirekdiskon")) & "', pirekpajak1  = '" & FixQuotes(drutama("pirekpajak1")) & "', pirekpajak2  = '" & FixQuotes(drutama("pirekpajak2")) & "', pirekbiayalain  = '" & FixQuotes(drutama("pirekbiayalain")) & "', pirekbayar  = '" & FixQuotes(drutama("pirekbayar")) & "', piidsq  = " & drutama("piidsq") & ", piidso  = " & drutama("piidso") & ", pistatuspl  = " & drutama("pistatuspl") & ", pistatusdo  = " & drutama("pistatusdo") & ", pistatusdr  = " & drutama("pistatusdr") & ", pistatussi  = " & drutama("pistatussi") & ", pistatusrnr  = " & drutama("pistatusrnr") & ", pistatussr  = " & drutama("pistatussr") & ", pistatus  = " & drutama("pistatus") & ", pistatussebelumnya  = " & drutama("pistatussebelumnya") & ", pijmlrevisi  = pijmlrevisi+1, picetakanke  = " & drutama("picetakanke") & ", pimodifikasiuser  = " & drutama("pimodifikasiuser") & ", pimodifikasitgl  = NOW(), pitutupperiode  = " & drutama("pitutupperiode") & ", picustomtext1  = '" & FixQuotes(drutama("picustomtext1")) & "', picustomtext2  = '" & FixQuotes(drutama("picustomtext2")) & "', picustomtext3  = '" & FixQuotes(drutama("picustomtext3")) & "', picustomtext4  = '" & FixQuotes(drutama("picustomtext4")) & "', picustomtext5  = '" & FixQuotes(drutama("picustomtext5")) & "', picustomint1  = " & drutama("picustomint1") & ", picustomint2  = " & drutama("picustomint2") & ", picustomint3  = " & drutama("picustomint3") & ", picustomdbl1  = '" & FixDouble(drutama("picustomdbl1")) & "', picustomdbl2  = '" & FixDouble(drutama("picustomdbl2")) & "', picustomdbl3  = '" & FixDouble(drutama("picustomdbl3")) & "', picustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("picustomdate1"))) & "', picustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("picustomdate2"))) & "', picustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("picustomdate3"))) & "' where piid = '" & drutama("piid") & "'"
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

                    If drutama("piautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("picabang"), drutama("pilokasi"), drutama("pisumber"), drutama("pitgl"))
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
                        notransaksi = drutama("pinotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(piid) FROM m5_pi WHERE pinotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Pi (picabang, pilokasi, pigudang, piasalbarang, piasalbarangkategori, pijenispenjualan, pijenispenjualankategori, picarabayar, pisumber, piautonotransaksi, pinotransaksi, pitgl, pikodepa, picustomer, picustomerkontak, pi1alamat1, pi1alamat2, pi1alamat3, pi2alamat1, pi2alamat2, pi2alamat3, pibagianpenjualan, piekspedisi, pitglkirim, pitermin, pitgljatuhtempo, piuraian, picatatan, pinoref, pitglnoref, pitglpenutupan, pimatauang, pikurs, pihargatermasukpajak, pitotal, pidiskonpersen, pijmldiskon, pitotalpajak1detail, pitotalpajak2detail, pibiayalainpersen, pibiayalain, pitotaltransaksi, pijmlbayar, pirekdiskon, pirekpajak1, pirekpajak2, pirekbiayalain, pirekbayar, piidsq, piidso, pistatuspl, pistatusdo, pistatusdr, pistatussi, pistatusrnr, pistatussr, pistatus, pistatussebelumnya, pijmlrevisi, picetakanke, piinputuser, piinputtgl, pimodifikasiuser, pimodifikasitgl, piisclose, pitutupperiode, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3) values('" & FixQuotes(drutama("picabang")) & "', '" & FixQuotes(drutama("pilokasi")) & "', '" & FixQuotes(drutama("pigudang")) & "', '" & FixQuotes(drutama("piasalbarang")) & "', " & drutama("piasalbarangkategori") & ", '" & FixQuotes(drutama("pijenispenjualan")) & "', " & drutama("pijenispenjualankategori") & ", " & drutama("picarabayar") & ", '" & FixQuotes(drutama("pisumber")) & "', " & drutama("piautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("pitgl"))) & "', " & drutama("pikodepa") & ", " & drutama("picustomer") & ", '" & FixQuotes(drutama("picustomerkontak")) & "', '" & FixQuotes(drutama("pi1alamat1")) & "', '" & FixQuotes(drutama("pi1alamat2")) & "', '" & FixQuotes(drutama("pi1alamat3")) & "', '" & FixQuotes(drutama("pi2alamat1")) & "', '" & FixQuotes(drutama("pi2alamat2")) & "', '" & FixQuotes(drutama("pi2alamat3")) & "', " & drutama("pibagianpenjualan") & ", '" & FixQuotes(drutama("piekspedisi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pitglkirim"))) & "', '" & FixQuotes(drutama("pitermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pitgljatuhtempo"))) & "', '" & FixQuotes(drutama("piuraian")) & "', '" & FixQuotes(drutama("picatatan")) & "', '" & FixQuotes(drutama("pinoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pitglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("pitglpenutupan"))) & "', '" & FixQuotes(drutama("pimatauang")) & "', '" & FixDouble(drutama("pikurs")) & "', " & drutama("pihargatermasukpajak") & ", '" & FixDouble(drutama("pitotal")) & "', '" & FixQuotes(drutama("pidiskonpersen")) & "', '" & FixDouble(drutama("pijmldiskon")) & "', '" & FixDouble(drutama("pitotalpajak1detail")) & "', '" & FixDouble(drutama("pitotalpajak2detail")) & "', '" & FixDouble(drutama("pibiayalainpersen")) & "', '" & FixDouble(drutama("pibiayalain")) & "', '" & FixDouble(drutama("pitotaltransaksi")) & "', '" & FixDouble(drutama("pijmlbayar")) & "', '" & FixQuotes(drutama("pirekdiskon")) & "', '" & FixQuotes(drutama("pirekpajak1")) & "', '" & FixQuotes(drutama("pirekpajak2")) & "', '" & FixQuotes(drutama("pirekbiayalain")) & "', '" & FixQuotes(drutama("pirekbayar")) & "', " & drutama("piidsq") & ", " & drutama("piidso") & ", " & drutama("pistatuspl") & ", " & drutama("pistatusdo") & ", " & drutama("pistatusdr") & ", " & drutama("pistatussi") & ", " & drutama("pistatusrnr") & ", " & drutama("pistatussr") & ", " & drutama("pistatus") & ", " & drutama("pistatussebelumnya") & ", " & drutama("pijmlrevisi") & ", " & drutama("picetakanke") & ", " & drutama("piinputuser") & ", NOW(), " & drutama("pimodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("piisclose") & ", " & drutama("pitutupperiode") & ", '" & FixQuotes(drutama("picustomtext1")) & "', '" & FixQuotes(drutama("picustomtext2")) & "', '" & FixQuotes(drutama("picustomtext3")) & "', '" & FixQuotes(drutama("picustomtext4")) & "', '" & FixQuotes(drutama("picustomtext5")) & "', " & drutama("picustomint1") & ", " & drutama("picustomint2") & ", " & drutama("picustomint3") & ", '" & FixDouble(drutama("picustomdbl1")) & "', '" & FixDouble(drutama("picustomdbl2")) & "', '" & FixDouble(drutama("picustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("picustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("picustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("picustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select piid from M5_pi where pinotransaksi='" & notransaksi & "' AND piinputuser= '" & userid & "' order by pimodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Pi_Detail where idpi = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idpidetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idsqdetail") & ", " & dr1("idsodetail") & ", '" & FixDouble(dr1("jmlpl")) & "', " & dr1("statuspl") & ", '" & FixDouble(dr1("jmldo")) & "', " & dr1("statusdo") & ", '" & FixDouble(dr1("jmldr")) & "', " & dr1("statusdr") & ", '" & FixDouble(dr1("jmlsi")) & "', " & dr1("statussi") & ", '" & FixDouble(dr1("jmlrnr")) & "', " & dr1("statusrnr") & ", '" & FixDouble(dr1("jmlsr")) & "', " & dr1("statussr") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M5_Pi_Detail(idpidetail, idpi, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                If drutama("pistatus") = 2 Then
                    If Len(updNilai) > 0 Then
                        'UPDATE OUTSTANDING TRANSAKSI =======================================================
                        'UPDATE DETAIL
                        sql = "UPDATE m5_so_detail SET jmlrealisasi = (CASE idsodetail " & updNilai & " ELSE jmlrealisasi END) WHERE " & updFilter
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idso FROM m5_so_detail WHERE " & updFilter & " GROUP BY idso", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idso = '" & dr1("idso") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idso, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_so_detail WHERE " & ftDetail & " GROUP BY idso", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilai = "" : updFilter = ""
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
                                updNilai = String.Concat(updNilai, "WHEN '" & dr1("idso") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                                updFilter = String.Concat(updFilter, "(soid = '" & dr1("idso") & "')")
                            Next

                            sql = "UPDATE m5_so SET sostatusrealisasi = (CASE soid " & updNilai & " ELSE sostatusrealisasi END) WHERE " & updFilter
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
                Dim sumber As String = "PI", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M5_PiUpdateStatus(ByVal param As String) As String
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
            Dim sumber As String = "Pi", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Pitgl, Pinotransaksi, Pistatus FROM M5_Pi WHERE Piid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Pistatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m5_pi_history
            Dim rsSimpanHistory As String = SimpanHistory.M5_Pi_HistorySimpan("" & paramSplit(0) & "★M5_Pi_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m5_pi_terkait("piid = '" & idtransaksi & "'")
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idsodetail As Integer = 0
                Dim ftOutstanding As String = "", updNilai As String = "", updFilter As String = ""
                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idsodetail, urutan FROM m5_pi_detail WHERE idpi = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : idsodetail = dr1("idsodetail")

                        'UPDATE OUTSTANDING ---------------------------
                        If idsodetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING ----------
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsodetail=" & idsodetail)
                            updNilai = String.Concat("WHEN '" & idsodetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilai)

                            '2. SET FILTERUPDATE OUTSTANDING ----------
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(idsodetail = '" & idsodetail & "')")
                        End If
                        'END OF BUAT FILTER UNTUK UPDATE --------------------------
                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                If Len(updFilter) > 0 Then
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m5_so_detail SET jmlrealisasi = (CASE idsodetail " & updNilai & " ELSE jmlrealisasi END) WHERE " & updFilter
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idso FROM m5_so_detail WHERE " & updFilter & " GROUP BY idso", myConn)
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idso = '" & dr1("idso") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDBCon("SELECT idso, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_so_detail WHERE " & ftDetail & " GROUP BY idso", myConn)
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilai = "" : updFilter = ""
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
                            updNilai = String.Concat(updNilai, "WHEN '" & dr1("idso") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(soid = '" & dr1("idso") & "')")
                        Next

                        sql = "UPDATE m5_so SET sostatusrealisasi = (CASE soid " & updNilai & " ELSE sostatusrealisasi END) WHERE " & updFilter
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
            sql = "UPDATE M5_Pi SET Pistatus = " & nilaiStatus & ", Pimodifikasiuser='" & userid & "', Pimodifikasitgl = NOW(), Piposting = 0, Pipostingtgl = '1971-01-01 00:00:00', Pijmlrevisi = Pijmlrevisi + 1 WHERE Piid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_PiSearch(PostWsSearch(paramSplit(0), "M5_piSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M5_PiDelete(ByVal param As String) As String

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
            Dim sumber As String = "Pi", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Piid, Pinotransaksi FROM M5_Pi WHERE Piid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT picabang, pilokasi, pisumber, piautonotransaksi, pinotransaksi, pitgl"
            sql &= " FROM M5_pi"
            sql &= " WHERE piid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("picabang")
                lokasi = dtNomorNext.Rows(0)("pilokasi")
                sumber = dtNomorNext.Rows(0)("pisumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("piautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("pinotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("pitgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M5_Pi_Detail WHERE idpi = " & idtransaksi
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M5_Pi WHERE piid = " & idtransaksi
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
            Dim paramSearch As String = M5_PiSearch(PostWsSearch(paramSplit(0), "M5_PiSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M5_PiGetdataById(ByVal param As String) As String
        'M5_PiGetdataById Utama --------------------------------------------------------
        'piid, picabang, pilokasi, pigudang, piasalbarang, piasalbarangkategori, pijenispenjualan, 
        'pijenispenjualankategori, picarabayar, pisumber, piautonotransaksi, pinotransaksi, pitgl, pikodepa, 
        'picustomer, picustomerkontak, pi1alamat1, pi1alamat2, pi1alamat3, pi2alamat1, pi2alamat2, 
        'pi2alamat3, pibagianpenjualan, piekspedisi, pitglkirim, pitermin, pitgljatuhtempo, piuraian, 
        'picatatan, pinoref, pitglnoref, pitglpenutupan, pimatauang, pikurs, pihargatermasukpajak, 
        'pitotal, pidiskonpersen, pijmldiskon, pitotalpajak1detail, pitotalpajak2detail, pibiayalainpersen, pibiayalain, 
        'pitotaltransaksi, pijmlbayar, pirekdiskon, pirekpajak1, pirekpajak2, pirekbiayalain, pirekbayar, 
        'piidsq, piidso, pistatuspl, pistatusdo, pistatusdr, pistatussi, pistatusrnr, 
        'pistatussr, pistatusrealisasi, pistatus, pistatussebelumnya, pijmlrevisi, picetakanke, piinputuser, 
        'piinputtgl, pimodifikasiuser, pimodifikasitgl, piposting, pipostingtgl, piisclose, pitutupperiode, 
        'picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, 
        'picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3, 
        'picabangnama, pilokasinama, pigudangnama, picustomerkode, picustomernama, pibagianpenjualankode, pibagianpenjualannama, 
        'piekspedisinama, piterminnama, piterminharijatuhtempo, pirekdiskonnama, pirekpajak1nama, pirekpajak2nama, pirekbiayalainnama, 
        'pirekbayarnama, pinotransaksiso, pistatusnama, pistatussebelumnyanama, piinputusernama, pimodifikasiusernama, ktingkatjual, kpkp

        'M5_PiGetdataById Detail --------------------------------------------------------
        'idpidetail, idpi, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, 
        'jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, 
        'pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, 
        'costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, 
        'idsodetail, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, 
        'jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, 
        'statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, 
        'bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, 
        'lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, sonotransaksi

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

        Dim NmMemcached As String = "aplikasi1-M5_pi~M5_pi_Detail-" & idtransaksi

        'Repiace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi repiace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "piid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "piid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_pi_getdata")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("piid"), 0), sptField,
                     FxDB(drutama("picabang"), ""), sptField,
                     FxDB(drutama("pilokasi"), ""), sptField,
                     FxDB(drutama("pigudang"), ""), sptField,
                     FxDB(drutama("piasalbarang"), ""), sptField,
                     FxDB(drutama("piasalbarangkategori"), 0), sptField,
                     FxDB(drutama("pijenispenjualan"), ""), sptField,
                     FxDB(drutama("pijenispenjualankategori"), 0), sptField,
                     FxDB(drutama("picarabayar"), 0), sptField,
                     FxDB(drutama("pisumber"), ""), sptField,
                     FxDB(drutama("piautonotransaksi"), 0), sptField,
                     FxDB(drutama("pinotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pitgl"), ""), formatTgl), sptField,
                     FxDB(drutama("pikodepa"), 0), sptField,
                     FxDB(drutama("picustomer"), 0), sptField,
                     FxDB(drutama("picustomerkontak"), ""), sptField,
                     FxDB(drutama("pi1alamat1"), ""), sptField,
                     FxDB(drutama("pi1alamat2"), ""), sptField,
                     FxDB(drutama("pi1alamat3"), ""), sptField,
                     FxDB(drutama("pi2alamat1"), ""), sptField,
                     FxDB(drutama("pi2alamat2"), ""), sptField,
                     FxDB(drutama("pi2alamat3"), ""), sptField,
                     FxDB(drutama("pibagianpenjualan"), 0), sptField,
                     FxDB(drutama("piekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pitglkirim"), ""), formatTgl), sptField,
                     FxDB(drutama("pitermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pitgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("piuraian"), ""), sptField,
                     FxDB(drutama("picatatan"), ""), sptField,
                     FxDB(drutama("pinoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pitglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pitglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("pimatauang"), ""), sptField,
                     FxDB(drutama("pikurs"), 0), sptField,
                     FxDB(drutama("pihargatermasukpajak"), 0), sptField,
                     FxDB(drutama("pitotal"), 0), sptField,
                     FxDB(drutama("pidiskonpersen"), ""), sptField,
                     FxDB(drutama("pijmldiskon"), 0), sptField,
                     FxDB(drutama("pitotalpajak1detail"), 0), sptField,
                     FxDB(drutama("pitotalpajak2detail"), 0), sptField,
                     FxDB(drutama("pibiayalainpersen"), 0), sptField,
                     FxDB(drutama("pibiayalain"), 0), sptField,
                     FxDB(drutama("pitotaltransaksi"), 0), sptField,
                     FxDB(drutama("pijmlbayar"), 0), sptField,
                     FxDB(drutama("pirekdiskon"), ""), sptField,
                     FxDB(drutama("pirekpajak1"), ""), sptField,
                     FxDB(drutama("pirekpajak2"), ""), sptField,
                     FxDB(drutama("pirekbiayalain"), ""), sptField,
                     FxDB(drutama("pirekbayar"), ""), sptField,
                     FxDB(drutama("piidsq"), 0), sptField,
                     FxDB(drutama("piidso"), 0), sptField,
                     FxDB(drutama("pistatuspl"), 0), sptField,
                     FxDB(drutama("pistatusdo"), 0), sptField,
                     FxDB(drutama("pistatusdr"), 0), sptField,
                     FxDB(drutama("pistatussi"), 0), sptField,
                     FxDB(drutama("pistatusrnr"), 0), sptField,
                     FxDB(drutama("pistatussr"), 0), sptField,
                     FxDB(drutama("pistatusrealisasi"), 0), sptField,
                     FxDB(drutama("pistatus"), 0), sptField,
                     FxDB(drutama("pistatussebelumnya"), 0), sptField,
                     FxDB(drutama("pijmlrevisi"), 0), sptField,
                     FxDB(drutama("picetakanke"), 0), sptField,
                     FxDB(drutama("piinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("piinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pimodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pimodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("piposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pipostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("piisclose"), 0), sptField,
                     FxDB(drutama("pitutupperiode"), 0), sptField,
                     FxDB(drutama("picustomtext1"), ""), sptField,
                     FxDB(drutama("picustomtext2"), ""), sptField,
                     FxDB(drutama("picustomtext3"), ""), sptField,
                     FxDB(drutama("picustomtext4"), ""), sptField,
                     FxDB(drutama("picustomtext5"), ""), sptField,
                     FxDB(drutama("picustomint1"), 0), sptField,
                     FxDB(drutama("picustomint2"), 0), sptField,
                     FxDB(drutama("picustomint3"), 0), sptField,
                     FxDB(drutama("picustomdbl1"), 0), sptField,
                     FxDB(drutama("picustomdbl2"), 0), sptField,
                     FxDB(drutama("picustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("picustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("picustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("picustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("picabangnama"), ""), sptField,
                     FxDB(drutama("pilokasinama"), ""), sptField,
                     FxDB(drutama("pigudangnama"), ""), sptField,
                     FxDB(drutama("picustomerkode"), ""), sptField,
                     FxDB(drutama("picustomernama"), ""), sptField,
                     FxDB(drutama("pibagianpenjualankode"), ""), sptField,
                     FxDB(drutama("pibagianpenjualannama"), ""), sptField,
                     FxDB(drutama("piekspedisinama"), ""), sptField,
                     FxDB(drutama("piterminnama"), ""), sptField,
                     FxDB(drutama("piterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("pirekdiskonnama"), ""), sptField,
                     FxDB(drutama("pirekpajak1nama"), ""), sptField,
                     FxDB(drutama("pirekpajak2nama"), ""), sptField,
                     FxDB(drutama("pirekbiayalainnama"), ""), sptField,
                     FxDB(drutama("pirekbayarnama"), ""), sptField,
                     FxDB(drutama("pinotransaksiso"), ""), sptField,
                     FxDB(drutama("pistatusnama"), ""), sptField,
                     FxDB(drutama("pistatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("piinputusernama"), ""), sptField,
                     FxDB(drutama("pimodifikasiusernama"), ""), sptField,
                     FxDB(drutama("ktingkatjual"), 0), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idpidetail"), 0), sptField,
                     FxDB(dr("idpi"), 0), sptField,
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
                     FxDB(dr("idsqdetail"), 0), sptField,
                     FxDB(dr("idsodetail"), 0), sptField,
                     FxDB(dr("jmlpl"), 0), sptField,
                     FxDB(dr("statuspl"), 0), sptField,
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
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
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
                     FxDB(dr("sonotransaksi"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("piid, picabang, pilokasi, pigudang, piasalbarang, piasalbarangkategori, pijenispenjualan, pijenispenjualankategori, picarabayar, pisumber, piautonotransaksi, pinotransaksi, pitgl, pikodepa, picustomer, picustomerkontak, pi1alamat1, pi1alamat2, pi1alamat3, pi2alamat1, pi2alamat2, pi2alamat3, pibagianpenjualan, piekspedisi, pitglkirim, pitermin, pitgljatuhtempo, piuraian, picatatan, pinoref, pitglnoref, pitglpenutupan, pimatauang, pikurs, pihargatermasukpajak, pitotal, pidiskonpersen, pijmldiskon, pitotalpajak1detail, pitotalpajak2detail, pibiayalainpersen, pibiayalain, pitotaltransaksi, pijmlbayar, pirekdiskon, pirekpajak1, pirekpajak2, pirekbiayalain, pirekbayar, piidsq, piidso, pistatuspl, pistatusdo, pistatusdr, pistatussi, pistatusrnr, pistatussr, pistatusrealisasi, pistatus, pistatussebelumnya, pijmlrevisi, picetakanke, piinputuser, piinputtgl, pimodifikasiuser, pimodifikasitgl, piposting, pipostingtgl, piisclose, pitutupperiode, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3, picabangnama, pilokasinama, pigudangnama, picustomerkode, picustomernama, pibagianpenjualankode, pibagianpenjualannama, piekspedisinama, piterminnama, piterminharijatuhtempo, pirekdiskonnama, pirekpajak1nama, pirekpajak2nama, pirekbiayalainnama, pirekbayarnama, pinotransaksiso, pistatusnama, pistatussebelumnyanama, piinputusernama, pimodifikasiusernama, ktingkatjual, kpkp"), sptSubParam, ReplaceMapping("idpidetail, idpi, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, sonotransaksi"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_PiSearch(ByVal param As String) As String
        'M5_PiSearch --------------------------------------------------------
        'piid, picabang, pilokasi, pigudang, piasalbarang, piasalbarangkategori, pijenispenjualan, 
        'pijenispenjualankategori, picarabayar, pisumber, piautonotransaksi, pinotransaksi, pitgl, pikodepa, 
        'picustomer, picustomerkontak, pi1alamat1, pi1alamat2, pi1alamat3, pi2alamat1, pi2alamat2, 
        'pi2alamat3, pibagianpenjualan, piekspedisi, pitglkirim, pitermin, pitgljatuhtempo, piuraian, 
        'picatatan, pinoref, pitglnoref, pitglpenutupan, pimatauang, pikurs, pihargatermasukpajak, 
        'pitotal, pidiskonpersen, pijmldiskon, pitotalpajak1detail, pitotalpajak2detail, pibiayalainpersen, pibiayalain, 
        'pitotaltransaksi, pijmlbayar, pirekdiskon, pirekpajak1, pirekpajak2, pirekbiayalain, pirekbayar, 
        'piidsq, piidso, pistatuspl, pistatusdo, pistatusdr, pistatussi, pistatusrnr, 
        'pistatussr, pistatusrealisasi, pistatus, pistatussebelumnya, pijmlrevisi, picetakanke, piinputuser, 
        'piinputtgl, pimodifikasiuser, pimodifikasitgl, piposting, pipostingtgl, piisclose, pitutupperiode, 
        'picabangnama, pilokasinama, pigudangnama, picustomerkode, picustomernama, pibagianpenjualankode, pibagianpenjualannama, 
        'piekspedisinama, sqnotransaksi, sonotransaksi, pistatusnama, pistatussebelumnyanama, piinputusernama, pimodifikasiusernama

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
        sql = query.PanggilQuery("m5_pi_v")

        dt = AmbilData("aplikasi1-M5_pi_V", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("piid"), 0), sptField,
                     FxDB(dr("picabang"), ""), sptField,
                     FxDB(dr("pilokasi"), ""), sptField,
                     FxDB(dr("pigudang"), ""), sptField,
                     FxDB(dr("piasalbarang"), ""), sptField,
                     FxDB(dr("piasalbarangkategori"), 0), sptField,
                     FxDB(dr("pijenispenjualan"), ""), sptField,
                     FxDB(dr("pijenispenjualankategori"), 0), sptField,
                     FxDB(dr("picarabayar"), 0), sptField,
                     FxDB(dr("pisumber"), ""), sptField,
                     FxDB(dr("piautonotransaksi"), 0), sptField,
                     FxDB(dr("pinotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pitgl"), ""), formatTgl), sptField,
                     FxDB(dr("pikodepa"), 0), sptField,
                     FxDB(dr("picustomer"), 0), sptField,
                     FxDB(dr("picustomerkontak"), ""), sptField,
                     FxDB(dr("pi1alamat1"), ""), sptField,
                     FxDB(dr("pi1alamat2"), ""), sptField,
                     FxDB(dr("pi1alamat3"), ""), sptField,
                     FxDB(dr("pi2alamat1"), ""), sptField,
                     FxDB(dr("pi2alamat2"), ""), sptField,
                     FxDB(dr("pi2alamat3"), ""), sptField,
                     FxDB(dr("pibagianpenjualan"), 0), sptField,
                     FxDB(dr("piekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pitglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("pitermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pitgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("piuraian"), ""), sptField,
                     FxDB(dr("picatatan"), ""), sptField,
                     FxDB(dr("pinoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pitglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("pitglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("pimatauang"), ""), sptField,
                     FxDB(dr("pikurs"), 0), sptField,
                     FxDB(dr("pihargatermasukpajak"), 0), sptField,
                     FxDB(dr("pitotal"), 0), sptField,
                     FxDB(dr("pidiskonpersen"), ""), sptField,
                     FxDB(dr("pijmldiskon"), 0), sptField,
                     FxDB(dr("pitotalpajak1detail"), 0), sptField,
                     FxDB(dr("pitotalpajak2detail"), 0), sptField,
                     FxDB(dr("pibiayalainpersen"), 0), sptField,
                     FxDB(dr("pibiayalain"), 0), sptField,
                     FxDB(dr("pitotaltransaksi"), 0), sptField,
                     FxDB(dr("pijmlbayar"), 0), sptField,
                     FxDB(dr("pirekdiskon"), ""), sptField,
                     FxDB(dr("pirekpajak1"), ""), sptField,
                     FxDB(dr("pirekpajak2"), ""), sptField,
                     FxDB(dr("pirekbiayalain"), ""), sptField,
                     FxDB(dr("pirekbayar"), ""), sptField,
                     FxDB(dr("piidsq"), 0), sptField,
                     FxDB(dr("piidso"), 0), sptField,
                     FxDB(dr("pistatuspl"), 0), sptField,
                     FxDB(dr("pistatusdo"), 0), sptField,
                     FxDB(dr("pistatusdr"), 0), sptField,
                     FxDB(dr("pistatussi"), 0), sptField,
                     FxDB(dr("pistatusrnr"), 0), sptField,
                     FxDB(dr("pistatussr"), 0), sptField,
                     FxDB(dr("pistatusrealisasi"), 0), sptField,
                     FxDB(dr("pistatus"), 0), sptField,
                     FxDB(dr("pistatussebelumnya"), 0), sptField,
                     FxDB(dr("pijmlrevisi"), 0), sptField,
                     FxDB(dr("picetakanke"), 0), sptField,
                     FxDB(dr("piinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("piinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pimodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pimodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("piposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pipostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("piisclose"), 0), sptField,
                     FxDB(dr("pitutupperiode"), 0), sptField,
                     FxDB(dr("picabangnama"), ""), sptField,
                     FxDB(dr("pilokasinama"), ""), sptField,
                     FxDB(dr("pigudangnama"), ""), sptField,
                     FxDB(dr("picustomerkode"), ""), sptField,
                     FxDB(dr("picustomernama"), ""), sptField,
                     FxDB(dr("pibagianpenjualankode"), ""), sptField,
                     FxDB(dr("pibagianpenjualannama"), ""), sptField,
                     FxDB(dr("piekspedisinama"), ""), sptField,
                     FxDB(dr("sqnotransaksi"), ""), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("pistatusnama"), ""), sptField,
                     FxDB(dr("pistatussebelumnyanama"), ""), sptField,
                     FxDB(dr("piinputusernama"), ""), sptField,
                     FxDB(dr("pimodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("piid, picabang, pilokasi, pigudang, piasalbarang, piasalbarangkategori, pijenispenjualan, pijenispenjualankategori, picarabayar, pisumber, piautonotransaksi, pinotransaksi, pitgl, pikodepa, picustomer, picustomerkontak, pi1alamat1, pi1alamat2, pi1alamat3, pi2alamat1, pi2alamat2, pi2alamat3, pibagianpenjualan, piekspedisi, pitglkirim, pitermin, pitgljatuhtempo, piuraian, picatatan, pinoref, pitglnoref, pitglpenutupan, pimatauang, pikurs, pihargatermasukpajak, pitotal, pidiskonpersen, pijmldiskon, pitotalpajak1detail, pitotalpajak2detail, pibiayalainpersen, pibiayalain, pitotaltransaksi, pijmlbayar, pirekdiskon, pirekpajak1, pirekpajak2, pirekbiayalain, pirekbayar, piidsq, piidso, pistatuspl, pistatusdo, pistatusdr, pistatussi, pistatusrnr, pistatussr, pistatusrealisasi, pistatus, pistatussebelumnya, pijmlrevisi, picetakanke, piinputuser, piinputtgl, pimodifikasiuser, pimodifikasitgl, piposting, pipostingtgl, piisclose, pitutupperiode, picabangnama, pilokasinama, pigudangnama, picustomerkode, picustomernama, pibagianpenjualankode, pibagianpenjualannama, piekspedisinama, sqnotransaksi, sonotransaksi, pistatusnama, pistatussebelumnyanama, piinputusernama, pimodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_Pi_Detail_VSearch(ByVal param As String) As String
        'M5_Pi_Detail_VSearch --------------------------------------------------------
        'idpidetail, idpi, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, 
        'jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, 
        'gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idsqdetail, idsodetail, jmlpl, statuspl, jmldo, statusdo, jmldr, 
        'statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, 
        'jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3, pinotransaksi, piuraian, 
        'picatatan, pinoref, pitglnoref, pitglkirim, picustomerkontak, pi1alamat1, pi1alamat2, 
        'pi1alamat3, pi2alamat1, pi2alamat2, pi2alamat3, pibagianpenjualan, pibagianpenjualankode, pibagianpenjualannama, 
        'piekspedisi, piekspedisinama, pitermin, piterminnama, piterminharijatuhtempo, kodebarang, bhpp, 
        'bhppaverage, bhargajual1, bjenis, brekpersediaan, brekhargapokok, brekdiskonpenjualan, brekpenjualan, bserial, 
        'bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisapl, jmlsisado, 
        'jmlsisadr, jmlsisasi, jmlsisarealisasi, jmllapangan, satuanlapangan, basset,
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

        Dim pil As String = ""

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
        'pil = query.PanggilQuery("m5_pi_detail_v")
        pil = "select `pid`.`idpidetail` AS `idpidetail`,`pid`.`idpi` AS `idpi`,`pid`.`idbarang` AS `idbarang`,`pid`.`namabarang` AS `namabarang`,`pid`.`tipebarang` AS `tipebarang`,`pid`.`jml` AS `jml`,`pid`.`satuan` AS `satuan`,`pid`.`nilaisatuan` AS `nilaisatuan`,`pid`.`jmlbarang` AS `jmlbarang`,`pid`.`satuanbarang` AS `satuanbarang`,`pid`.`matauang` AS `matauang`,`pid`.`kurs` AS `kurs`,`pid`.`harga` AS `harga`,`pid`.`diskon` AS `diskon`,`pid`.`jmldiskon` AS `jmldiskon`,`pid`.`pajak1` AS `pajak1`,`pid`.`jmlpajak1` AS `jmlpajak1`,`pid`.`pajak2` AS `pajak2`,`pid`.`jmlpajak2` AS `jmlpajak2`,`pid`.`cabang` AS `cabang`,`pid`.`lokasi` AS `lokasi`,`pid`.`gudang` AS `gudang`,`pid`.`costcenter` AS `costcenter`,`pid`.`divisi` AS `divisi`,`pid`.`subdivisi` AS `subdivisi`,`pid`.`proyek` AS `proyek`,`pid`.`catatan` AS `catatan`,`pid`.`urutan` AS `urutan`,`pid`.`idsqdetail` AS `idsqdetail`,`pid`.`idsodetail` AS `idsodetail`,`pid`.`jmlpl` AS `jmlpl`,`pid`.`statuspl` AS `statuspl`,`pid`.`jmldo` AS `jmldo`,`pid`.`statusdo` AS `statusdo`,`pid`.`jmldr` AS `jmldr`,`pid`.`statusdr` AS `statusdr`,`pid`.`jmlsi` AS `jmlsi`,`pid`.`statussi` AS `statussi`,`pid`.`jmlrnr` AS `jmlrnr`,`pid`.`statusrnr` AS `statusrnr`,`pid`.`jmlsr` AS `jmlsr`,`pid`.`statussr` AS `statussr`,`pid`.`jmlrealisasi` AS `jmlrealisasi`,`pid`.`statusrealisasi` AS `statusrealisasi`,`pid`.`isclose` AS `isclose`,`pid`.`customtext1` AS `customtext1`,`pid`.`customtext2` AS `customtext2`,`pid`.`customtext3` AS `customtext3`,`pid`.`customdbl1` AS `customdbl1`,`pid`.`customdbl2` AS `customdbl2`,`pid`.`customdbl3` AS `customdbl3`,`pid`.`customdate1` AS `customdate1`,`pid`.`customdate2` AS `customdate2`,`pid`.`customdate3` AS `customdate3`,`pi`.`pinotransaksi` AS `pinotransaksi`,`pi`.`piuraian` AS `piuraian`,`pi`.`picatatan` AS `picatatan`,`pi`.`pinoref` AS `pinoref`,`pi`.`pitglnoref` AS `pitglnoref`,`pi`.`pitglkirim` AS `pitglkirim`,`pi`.`picustomerkontak` AS `picustomerkontak`,`pi`.`pi1alamat1` AS `pi1alamat1`,`pi`.`pi1alamat2` AS `pi1alamat2`,`pi`.`pi1alamat3` AS `pi1alamat3`,`pi`.`pi2alamat1` AS `pi2alamat1`,`pi`.`pi2alamat2` AS `pi2alamat2`,`pi`.`pi2alamat3` AS `pi2alamat3`,`pi`.`pibagianpenjualan` AS `pibagianpenjualan`,`c1`.`kkode` AS `pibagianpenjualankode`,`c1`.`knama` AS `pibagianpenjualannama`,`pi`.`piekspedisi` AS `piekspedisi`,`e`.`enama` AS `piekspedisinama`,`pi`.`pitermin` AS `pitermin`,`tr`.`trnama` AS `piterminnama`,`tr`.`trharijatuhtempo` AS `piterminharijatuhtempo`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bhppaverage` AS `bhppaverage`,`i`.`bhargajual1` AS `bhargajual1`,`i`.`bjenis` AS `bjenis`,`i`.`brekpersediaan` AS `brekpersediaan`,`i`.`brekhargapokok` AS `brekhargapokok`,`i`.`brekdiskonpenjualan` AS `brekdiskonpenjualan`,`i`.`brekpenjualan` AS `brekpenjualan`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,((`pid`.`jmlbarang` - `pid`.`jmlpl`) / `pid`.`nilaisatuan`) AS `jmlsisapl`,((`pid`.`jmlbarang` - `pid`.`jmldo`) / `pid`.`nilaisatuan`) AS `jmlsisado`,((`pid`.`jmlbarang` - `pid`.`jmldr`) / `pid`.`nilaisatuan`) AS `jmlsisadr`,((`pid`.`jmlbarang` - `pid`.`jmlsi`) / `pid`.`nilaisatuan`) AS `jmlsisasi`,((`pid`.`jmlbarang` - `pid`.`jmlrealisasi`) / `pid`.`nilaisatuan`) AS `jmlsisarealisasi`, i.bjmllapangan, i.bsatuanlapangan, i.basset, t1.takunbeli as pajak1akunbeli, t1c1.cnama as pajak1akunbelinama, t1.takunjual as pajak1akunjual, t1c2.cnama as pajak1akunjualnama, t2.takunbeli as pajak2akunbeli, t2c1.cnama as pajak2akunbelinama, t2.takunjual as pajak2akunjual, t2c2.cnama as pajak2akunjualnama, d.dnama AS divisinama, sd.sdnama AS subdivisinama, cc.ccnama AS costcenternama, p.pnama AS proyeknama from `m5_pi_detail` `pid` left join `m5_pi` `pi` on `pid`.`idpi` = `pi`.`piid` left join `m1_terms` `tr` on `pi`.`pitermin` = `tr`.`trkode` left join `m1_contact` `c1` on `pi`.`pibagianpenjualan` = `c1`.`kid` left join `m1_expedition` `e` on `pi`.`piekspedisi` = `e`.`ekode` left join `m1_item` `i` on `pid`.`idbarang` = `i`.`bid` left join `m1_tax` `t1` on `pid`.`pajak1` = `t1`.`tkode` left join `m1_tax` `t2` on `pid`.`pajak2` = `t2`.`tkode` left join m1_coa t1c1 on t1.takunbeli = t1c1.cnomor left join m1_coa t1c2 on t1.takunjual = t1c2.cnomor left join m1_coa t2c1 on t2.takunbeli = t2c1.cnomor left join m1_coa t2c2 on t2.takunjual = t2c2.cnomor LEFT JOIN m1_division d ON d.dkode = pid.divisi LEFT JOIN m1_subdivision sd ON sd.sdkode = pid.subdivisi LEFT JOIN m1_cost_center cc ON cc.cckode = pid.costcenter LEFT JOIN m1_project p ON p.pkode = pid.proyek"

        dt = AmbilData("aplikasi1-M5_pi_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , pil) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idpidetail"), 0), sptField,
                     FxDB(dr("idpi"), 0), sptField,
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
                     FxDB(dr("idsqdetail"), 0), sptField,
                     FxDB(dr("idsodetail"), 0), sptField,
                     FxDB(dr("jmlpl"), 0), sptField,
                     FxDB(dr("statuspl"), 0), sptField,
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
                     FxDB(dr("pinotransaksi"), ""), sptField,
                     FxDB(dr("piuraian"), ""), sptField,
                     FxDB(dr("picatatan"), ""), sptField,
                     FxDB(dr("pinoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pitglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("pitglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("picustomerkontak"), ""), sptField,
                     FxDB(dr("pi1alamat1"), ""), sptField,
                     FxDB(dr("pi1alamat2"), ""), sptField,
                     FxDB(dr("pi1alamat3"), ""), sptField,
                     FxDB(dr("pi2alamat1"), ""), sptField,
                     FxDB(dr("pi2alamat2"), ""), sptField,
                     FxDB(dr("pi2alamat3"), ""), sptField,
                     FxDB(dr("pibagianpenjualan"), 0), sptField,
                     FxDB(dr("pibagianpenjualankode"), ""), sptField,
                     FxDB(dr("pibagianpenjualannama"), ""), sptField,
                     FxDB(dr("piekspedisi"), ""), sptField,
                     FxDB(dr("piekspedisinama"), ""), sptField,
                     FxDB(dr("pitermin"), ""), sptField,
                     FxDB(dr("piterminnama"), ""), sptField,
                     FxDB(dr("piterminharijatuhtempo"), 0), sptField,
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
                     FxDB(dr("jmlsisapl"), 0), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idpidetail, idpi, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, pinotransaksi, piuraian, picatatan, pinoref, pitglnoref, pitglkirim, picustomerkontak, pi1alamat1, pi1alamat2, pi1alamat3, pi2alamat1, pi2alamat2, pi2alamat3, pibagianpenjualan, pibagianpenjualankode, pibagianpenjualannama, piekspedisi, piekspedisinama, pitermin, piterminnama, piterminharijatuhtempo, kodebarang, bhpp, bhppaverage, bhargajual1, bjenis, brekpersediaan, brekhargapokok, brekdiskonpenjualan, brekpenjualan, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisapl, jmlsisado, jmlsisadr, jmlsisasi, jmlsisarealisasi, bjmllapangan, bsatuanlapangan, basset, pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, pajak2akunjualnama, divisinama, subdivisinama, costcenternama, proyeknama "))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_PiTerkait(ByVal param As String) As String
        'M5_PiTerkait --------------------------------------------------------
        'piid, pinotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "piid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2) & " AND piid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "piid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.m5_pi_terkait(Filter)

        dt = AmbilData("aplikasi1-m5_pi_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each pi As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(pi("piid"), 0), sptField,
                     FxDB(pi("pinotransaksi"), ""), sptField,
                     FxDB(pi("sumber"), ""), sptField,
                     FxDB(pi("idterkait"), 0), sptField,
                     FxDB(pi("noterkait"), ""), sptField,
                     AsFormatTanggal(FxDB(pi("tglterkait"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(pi("inputtglterkait"), ""), formatTglWaktu), sptField,
                     AsFormatTanggal(FxDB(pi("modifikasitglterkait"), ""), formatTglWaktu), sptField,
                     FxDB(pi("jenisterkait"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Related PI data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("piid, pinotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstanding As String, ByVal ftOutstanding As String, ByVal ftSO As String, ByVal termasukPajak As String) As String
        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaiSatuan As Double = 0, sisa As Double = 0
        Dim filterLookup As String = "", gudang As String = "", urutan As String = ""

        'VALIDASI OUTSTANDING ---------------------------------------
        If Len(ftExistOutstanding) > 0 Then 'ftExistOutstanding = rowExists, idsodetail, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstanding)
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
                sql = "SELECT so.sonotransaksi as notransaksi, (CASE so.sohargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m5_so_detail sod JOIN m5_so so ON sod.idso = so.soid WHERE " & ftSO & " GROUP BY so.sohargatermasukpajak"
                dtval = AsDataTableAmbilDariDB(sql)
                If dtval.Rows.Count > 1 Then
                    errmessage = "Include Tax Price can't join with Exclude Tax Price as one Transaction"
                    For Each dr1 As DataRow In dtval.Rows
                        errmessage &= ", " & dr1("notransaksi") & " " & dr1("termasukpajak")
                    Next
                    GoTo selesai
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
            sql = "SELECT sod.idsodetail, (sod.jmlbarang - sod.jmlrealisasi) as sisarealisasi, i.bid, i.bkode FROM m5_so_detail AS sod INNER JOIN m1_item AS i ON sod.idbarang = i.bid WHERE " & ftOutstanding
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
        'END OF VALIDASI OUTSTANDING --------------------------------
selesai:
        Return errmessage
    End Function

    <WebMethod()>
    Public Function M5_PiSimpanOld(ByVal param As String) As String
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
        'piid(0) As Integer, picabang(1) As String, pilokasi(2) As String, pigudang(3) As String, piasalbarang(4) As String, 
        'piasalbarangkategori(5) As Integer, pijenispenjualan(6) As String, pijenispenjualankategori(7) As Integer, picarabayar(8) As Integer, pisumber(9) As String, 
        'piautonotransaksi(10) As Integer, pinotransaksi(11) As String, pitgl(12) As Date, pikodepa(13) As Integer, picustomer(14) As Integer, 
        'picustomerkontak(15) As String, pi1alamat1(16) As String, pi1alamat2(17) As String, pi1alamat3(18) As String, pi2alamat1(19) As String, 
        'pi2alamat2(20) As String, pi2alamat3(21) As String, pibagianpenjualan(22) As Integer, piekspedisi(23) As String, pitglkirim(24) As Date, 
        'pitermin(25) As String, pitgljatuhtempo(26) As Date, piuraian(27) As String, picatatan(28) As String, pinoref(29) As String, 
        'pitglnoref(30) As Date, pitglpenutupan(31) As Date, pimatauang(32) As String, pikurs(33) As Double, pihargatermasukpajak(34) As Integer, 
        'pitotal(35) As Double, pidiskonpersen(36) As String, pijmldiskon(37) As Double, pitotalpajak1detail(38) As Double, pitotalpajak2detail(39) As Double, 
        'pibiayalainpersen(40) As Double, pibiayalain(41) As Double, pitotaltransaksi(42) As Double, pijmlbayar(43) As Double, pirekdiskon(44) As String, 
        'pirekpajak1(45) As String, pirekpajak2(46) As String, pirekbiayalain(47) As String, pirekbayar(48) As String, piidsq(49) As Integer, 
        'piidso(50) As Integer, pistatuspl(51) As Integer, pistatusdo(52) As Integer, pistatusdr(53) As Integer, pistatussi(54) As Integer, 
        'pistatusrnr(55) As Integer, pistatussr(56) As Integer, pistatus(57) As Integer, pistatussebelumnya(58) As Integer, pijmlrevisi(59) As Integer, 
        'picetakanke(60) As Integer, piinputuser(61) As Integer, piinputtgl(62) As DateTime, pimodifikasiuser(63) As Integer, pimodifikasitgl(64) As DateTime, 
        'piisclose(65) As Integer, pitutupperiode(66) As Integer, picustomtext1(67) As String, picustomtext2(68) As String, picustomtext3(69) As String, 
        'picustomtext4(70) As String, picustomtext5(71) As String, picustomint1(72) As Integer, picustomint2(73) As Integer, picustomint3(74) As Integer, 
        'picustomdbl1(75) As Double, picustomdbl2(76) As Double, picustomdbl3(77) As Double, picustomdate1(78) As Date, picustomdate2(79) As Date, 
        'picustomdate3(80) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'piid, picabang, pilokasi, pigudang, piasalbarang, piasalbarangkategori, pijenispenjualan, 
        'pijenispenjualankategori, picarabayar, pisumber, piautonotransaksi, pinotransaksi, pitgl, pikodepa, 
        'picustomer, picustomerkontak, pi1alamat1, pi1alamat2, pi1alamat3, pi2alamat1, pi2alamat2, 
        'pi2alamat3, pibagianpenjualan, piekspedisi, pitglkirim, pitermin, pitgljatuhtempo, piuraian, 
        'picatatan, pinoref, pitglnoref, pitglpenutupan, pimatauang, pikurs, pihargatermasukpajak, 
        'pitotal, pidiskonpersen, pijmldiskon, pitotalpajak1detail, pitotalpajak2detail, pibiayalainpersen, pibiayalain, 
        'pitotaltransaksi, pijmlbayar, pirekdiskon, pirekpajak1, pirekpajak2, pirekbiayalain, pirekbayar, 
        'piidsq, piidso, pistatuspl, pistatusdo, pistatusdr, pistatussi, pistatusrnr, 
        'pistatussr, pistatus, pistatussebelumnya, pijmlrevisi, picetakanke, piinputuser, piinputtgl, 
        'pimodifikasiuser, pimodifikasitgl, piisclose, pitutupperiode, picustomtext1, picustomtext2, picustomtext3, 
        'picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, picustomdbl1, picustomdbl2, 
        'picustomdbl3, picustomdate1, picustomdate2, picustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 81) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'piid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "piid required numeric." : GoTo selesai
        End If
        'piasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "piasalbarangkategori required numeric." : GoTo selesai
        End If
        'pijenispenjualankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "pijenispenjualankategori required numeric." : GoTo selesai
        End If
        'picarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "picarabayar required numeric." : GoTo selesai
        End If
        'piautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "piautonotransaksi required numeric." : GoTo selesai
        End If
        'pitgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "pitgl required date." : GoTo selesai
        End If
        'pikodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "pikodepa required numeric." : GoTo selesai
        End If
        'picustomer(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "picustomer required numeric." : GoTo selesai
        End If
        'pibagianpenjualan(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "pibagianpenjualan required numeric." : GoTo selesai
        End If
        'pitglkirim(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "pitglkirim required date." : GoTo selesai
        End If
        'pitgljatuhtempo(26) As Date
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "pitgljatuhtempo required date." : GoTo selesai
        End If
        'pitglnoref(30) As Date
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "pitglnoref required date." : GoTo selesai
        End If
        'pitglpenutupan(31) As Date
        If (IsDate(dataUtama(31)) = False) Then
            result(2) = "pitglpenutupan required date." : GoTo selesai
        End If
        'pikurs(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "pikurs required numeric." : GoTo selesai
        End If
        'pihargatermasukpajak(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "pihargatermasukpajak required numeric." : GoTo selesai
        End If
        'pitotal(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "pitotal required numeric." : GoTo selesai
        End If
        'pijmldiskon(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "pijmldiskon required numeric." : GoTo selesai
        End If
        'pitotalpajak1detail(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "pitotalpajak1detail required numeric." : GoTo selesai
        End If
        'pitotalpajak2detail(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "pitotalpajak2detail required numeric." : GoTo selesai
        End If
        ''pibiayalainpersen(40) As Double
        'If (IsNumeric(dataUtama(40)) = False) Then
        '    result(2) = "pibiayalainpersen required numeric." : GoTo selesai
        'End If
        'pibiayalain(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "pibiayalain required numeric." : GoTo selesai
        End If
        'pitotaltransaksi(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "pitotaltransaksi required numeric." : GoTo selesai
        End If
        'pijmlbayar(43) As Double
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "pijmlbayar required numeric." : GoTo selesai
        End If
        'piidsq(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "piidsq required numeric." : GoTo selesai
        End If
        'piidso(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "piidso required numeric." : GoTo selesai
        End If
        'pistatuspl(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "pistatuspl required numeric." : GoTo selesai
        End If
        'pistatusdo(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "pistatusdo required numeric." : GoTo selesai
        End If
        'pistatusdr(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "pistatusdr required numeric." : GoTo selesai
        End If
        'pistatussi(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "pistatussi required numeric." : GoTo selesai
        End If
        'pistatusrnr(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "pistatusrnr required numeric." : GoTo selesai
        End If
        'pistatussr(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "pistatussr required numeric." : GoTo selesai
        End If
        'pistatus(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "pistatus required numeric." : GoTo selesai
        End If
        'pistatussebelumnya(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "pistatussebelumnya required numeric." : GoTo selesai
        End If
        'pijmlrevisi(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "pijmlrevisi required numeric." : GoTo selesai
        End If
        'picetakanke(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "picetakanke required numeric." : GoTo selesai
        End If
        'piinputuser(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "piinputuser required numeric." : GoTo selesai
        End If
        'piinputtgl(62) As DateTime
        If (IsDate(dataUtama(62)) = False) Then
            result(2) = "piinputtgl required date." : GoTo selesai
        End If
        'pimodifikasiuser(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "pimodifikasiuser required numeric." : GoTo selesai
        End If
        'pimodifikasitgl(64) As DateTime
        If (IsDate(dataUtama(64)) = False) Then
            result(2) = "pimodifikasitgl required date." : GoTo selesai
        End If
        'piisclose(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "piisclose required numeric." : GoTo selesai
        End If
        'pitutupperiode(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "pitutupperiode required numeric." : GoTo selesai
        End If
        'picustomint1(72) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "picustomint1 required numeric." : GoTo selesai
        End If
        'picustomint2(73) As Integer
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "picustomint2 required numeric." : GoTo selesai
        End If
        'picustomint3(74) As Integer
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "picustomint3 required numeric." : GoTo selesai
        End If
        'picustomdbl1(75) As Double
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "picustomdbl1 required numeric." : GoTo selesai
        End If
        'picustomdbl2(76) As Double
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "picustomdbl2 required numeric." : GoTo selesai
        End If
        'picustomdbl3(77) As Double
        If (IsNumeric(dataUtama(77)) = False) Then
            result(2) = "picustomdbl3 required numeric." : GoTo selesai
        End If
        'picustomdate1(78) As Date
        If (IsDate(dataUtama(78)) = False) Then
            result(2) = "picustomdate1 required date." : GoTo selesai
        End If
        'picustomdate2(79) As Date
        If (IsDate(dataUtama(79)) = False) Then
            result(2) = "picustomdate2 required date." : GoTo selesai
        End If
        'picustomdate3(80) As Date
        If (IsDate(dataUtama(80)) = False) Then
            result(2) = "picustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'picabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "picabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "picabang should not be more than 25 character." : GoTo selesai
        End If

        'pilokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "pilokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "pilokasi should not be more than 25 character." : GoTo selesai
        End If

        'pigudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "pigudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "pigudang should not be more than 25 character." : GoTo selesai
        End If

        'pisumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "pisumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "pisumber should not be more than 10 character." : GoTo selesai
        End If

        'pinotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "pinotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "pinotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'pitgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "pitgl can't be empty" : GoTo selesai
        End If

        'pitglkirim(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "pitglkirim can't be empty" : GoTo selesai
        End If

        'pitgljatuhtempo(26) As Date
        If Len(dataUtama(26)) = 0 Then
            result(2) = "pitgljatuhtempo can't be empty" : GoTo selesai
        End If

        'pitglnoref(30) As Date
        If Len(dataUtama(30)) = 0 Then
            result(2) = "pitglnoref can't be empty" : GoTo selesai
        End If

        'pitglpenutupan(31) As Date
        If Len(dataUtama(31)) = 0 Then
            result(2) = "pitglpenutupan can't be empty" : GoTo selesai
        End If

        'pimatauang(32) As String
        If Len(dataUtama(32)) = 0 Then
            result(2) = "pimatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(32)) > 25 Then
            result(2) = "pimatauang should not be more than 25 character." : GoTo selesai
        End If

        'pikurs(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "pikurs can't be empty" : GoTo selesai
        End If

        'pitotal(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "pitotal can't be empty" : GoTo selesai
        End If

        'pidiskonpersen(36) As String
        If Len(dataUtama(36)) = 0 Then
            result(2) = "pidiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(36)) > 25 Then
            result(2) = "pidiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'pijmldiskon(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "pijmldiskon can't be empty" : GoTo selesai
        End If

        'pitotalpajak1detail(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "pitotalpajak1detail can't be empty" : GoTo selesai
        End If

        'pitotalpajak2detail(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "pitotalpajak2detail can't be empty" : GoTo selesai
        End If

        'pibiayalainpersen(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "pibiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(40)) > 25 Then
            result(2) = "pibiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'pibiayalain(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "pibiayalain can't be empty" : GoTo selesai
        End If

        'pitotaltransaksi(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "pitotaltransaksi can't be empty" : GoTo selesai
        End If

        'pijmlbayar(43) As Double
        If Len(dataUtama(43)) = 0 Then
            result(2) = "pijmlbayar can't be empty" : GoTo selesai
        End If

        'piinputtgl(62) As DateTime
        If Len(dataUtama(62)) = 0 Then
            result(2) = "piinputtgl can't be empty" : GoTo selesai
        End If

        'pimodifikasitgl(64) As DateTime
        If Len(dataUtama(64)) = 0 Then
            result(2) = "pimodifikasitgl can't be empty" : GoTo selesai
        End If

        'picustomdbl1(75) As Double
        If Len(dataUtama(75)) = 0 Then
            result(2) = "picustomdbl1 can't be empty" : GoTo selesai
        End If

        'picustomdbl2(76) As Double
        If Len(dataUtama(76)) = 0 Then
            result(2) = "picustomdbl2 can't be empty" : GoTo selesai
        End If

        'picustomdbl3(77) As Double
        If Len(dataUtama(77)) = 0 Then
            result(2) = "picustomdbl3 can't be empty" : GoTo selesai
        End If

        'picustomdate1(78) As Date
        If Len(dataUtama(78)) = 0 Then
            result(2) = "picustomdate1 can't be empty" : GoTo selesai
        End If

        'picustomdate2(79) As Date
        If Len(dataUtama(79)) = 0 Then
            result(2) = "picustomdate2 can't be empty" : GoTo selesai
        End If

        'picustomdate3(80) As Date
        If Len(dataUtama(80)) = 0 Then
            result(2) = "picustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "piid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "picabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pilokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pigudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pijenispenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pijenispenjualankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "picarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pisumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pinotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pikodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "picustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "picustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pi1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pi1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pi1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pi2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pi2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pi2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pibagianpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piekspedisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pitglkirim", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pitermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pitgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "picatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pinoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pitglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pitglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pimatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pikurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pihargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pitotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pidiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pijmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pitotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pitotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pibiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pibiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pitotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pijmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pirekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pirekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pirekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pirekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pirekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piidsq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piidso", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pistatuspl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pistatusdo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pistatusdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pistatussi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pistatusrnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pistatussr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pistatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pistatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pijmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "picetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pimodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pimodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pitutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "picustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "picustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "picustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "picustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "picustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "picustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "picustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "picustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "picustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "picustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "picustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "picustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "picustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "picustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "piid~picabang~pilokasi~pigudang~piasalbarang~piasalbarangkategori~pijenispenjualan~pijenispenjualankategori~picarabayar~pisumber~piautonotransaksi~pinotransaksi~pitgl~pikodepa~picustomer~picustomerkontak~pi1alamat1~pi1alamat2~pi1alamat3~pi2alamat1~pi2alamat2~pi2alamat3~pibagianpenjualan~piekspedisi~pitglkirim~pitermin~pitgljatuhtempo~piuraian~picatatan~pinoref~pitglnoref~pitglpenutupan~pimatauang~pikurs~pihargatermasukpajak~pitotal~pidiskonpersen~pijmldiskon~pitotalpajak1detail~pitotalpajak2detail~pibiayalainpersen~pibiayalain~pitotaltransaksi~pijmlbayar~pirekdiskon~pirekpajak1~pirekpajak2~pirekbiayalain~pirekbayar~piidsq~piidso~pistatuspl~pistatusdo~pistatusdr~pistatussi~pistatusrnr~pistatussr~pistatus~pistatussebelumnya~pijmlrevisi~picetakanke~piinputuser~piinputtgl~pimodifikasiuser~pimodifikasitgl~piisclose~pitutupperiode~picustomtext1~picustomtext2~picustomtext3~picustomtext4~picustomtext5~picustomint1~picustomint2~picustomint3~picustomdbl1~picustomdbl2~picustomdbl3~picustomdate1~picustomdate2~picustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idpidetail(0) As Integer, idpi(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, diskon(13) As String, jmldiskon(14) As Double, 
        'pajak1(15) As String, jmlpajak1(16) As Double, pajak2(17) As String, jmlpajak2(18) As Double, cabang(19) As String, 
        'lokasi(20) As String, gudang(21) As String, costcenter(22) As String, divisi(23) As String, subdivisi(24) As String, 
        'proyek(25) As String, catatan(26) As String, urutan(27) As Integer, idsqdetail(28) As Integer, idsodetail(29) As Integer, 
        'jmlpl(30) As Double, statuspl(31) As Integer, jmldo(32) As Double, statusdo(33) As Integer, jmldr(34) As Double, 
        'statusdr(35) As Integer, jmlsi(36) As Double, statussi(37) As Integer, jmlrnr(38) As Double, statusrnr(39) As Integer, 
        'jmlsr(40) As Double, statussr(41) As Integer, isclose(42) As Integer, customtext1(43) As String, customtext2(44) As String, 
        'customtext3(45) As String, customdbl1(46) As Double, customdbl2(47) As Double, customdbl3(48) As Double, customdate1(49) As Date, 
        'customdate2(50) As Date, customdate3(51) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idpidetail, idpi, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, 
        'jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, 
        'gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idsqdetail, idsodetail, jmlpl, statuspl, jmldo, statusdo, jmldr, 
        'statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idpidetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpi", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "idsqdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idsodetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlpl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuspl", AsEnumTypeData.AsInt64)
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
        Dim ftExistOutstanding As String = "", ftOutstanding As String = ""
        Dim updNilai As String = "", updFilter As String = ""
        Dim idbarang As Integer = 0, idsodetail As Integer = 0, jmlbarang As Double = 0

        'Validasi Harga dibawah harga jual
        Dim ftLowerPrice As String = "", kurs As Double = 0, harga As Double = 0

        'FILTER SO, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftSO As String = ""

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
            'idpidetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idpidetail required numeric." : GoTo selesai
            End If
            'idpi(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idpi required numeric." : GoTo selesai
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
            'idsqdetail(28) As Integer
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - idsqdetail required numeric." : GoTo selesai
            End If
            'idsodetail(29) As Integer
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - idsodetail required numeric." : GoTo selesai
            End If
            'jmlpl(30) As Double
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - jmlpl required numeric." : GoTo selesai
            End If
            'statuspl(31) As Integer
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - statuspl required numeric." : GoTo selesai
            End If
            'jmldo(32) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - jmldo required numeric." : GoTo selesai
            End If
            'statusdo(33) As Integer
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - statusdo required numeric." : GoTo selesai
            End If
            'jmldr(34) As Double
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - jmldr required numeric." : GoTo selesai
            End If
            'statusdr(35) As Integer
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - statusdr required numeric." : GoTo selesai
            End If
            'jmlsi(36) As Double
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - jmlsi required numeric." : GoTo selesai
            End If
            'statussi(37) As Integer
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - statussi required numeric." : GoTo selesai
            End If
            'jmlrnr(38) As Double
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - jmlrnr required numeric." : GoTo selesai
            End If
            'statusrnr(39) As Integer
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - statusrnr required numeric." : GoTo selesai
            End If
            'jmlsr(40) As Double
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - jmlsr required numeric." : GoTo selesai
            End If
            'statussr(41) As Integer
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - statussr required numeric." : GoTo selesai
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

            'jmlpl(30) As Double
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Row : " & i & " - jmlpl can't be empty" : GoTo selesai
            End If

            'jmldo(32) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - jmldo can't be empty" : GoTo selesai
            End If

            'jmldr(34) As Double
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Row : " & i & " - jmldr can't be empty" : GoTo selesai
            End If

            'jmlsi(36) As Double
            If Len(dataRowDetail(36)) = 0 Then
                result(2) = "Row : " & i & " - jmlsi can't be empty" : GoTo selesai
            End If

            'jmlrnr(38) As Double
            If Len(dataRowDetail(38)) = 0 Then
                result(2) = "Row : " & i & " - jmlrnr can't be empty" : GoTo selesai
            End If

            'jmlsr(40) As Double
            If Len(dataRowDetail(40)) = 0 Then
                result(2) = "Row : " & i & " - jmlsr can't be empty" : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idpidetail~idpi~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~idsqdetail~idsodetail~jmlpl~statuspl~jmldo~statusdo~jmldr~statusdr~jmlsi~statussi~jmlrnr~statusrnr~jmlsr~statussr~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , idsodetail(29) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : idsodetail = dataRowDetail(29)
            'kurs(11) As Double                    , harga(12) As Double
            kurs = Double.Parse(dataRowDetail(11)) : harga = Double.Parse(dataRowDetail(12))

            'VALIDASI OUTSTANDING -------------------------
            If idsodetail <> 0 Then 'SO
                'CEK SO YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftSO = IIf(Len(ftSO.ToString) = 0, "", ftSO & " OR ")
                ftSO = String.Concat(ftSO, " (sod.idsodetail = " & idsodetail & ") ")

                '1. CEK DATA EXIST ------------------------
                ftExistOutstanding = IIf(Len(ftExistOutstanding.ToString) = 0, "", ftExistOutstanding & " UNION ")
                'ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m5_so_detail JOIN m5_so ON idso = soid WHERE idsodetail = '" & idsodetail & "' AND (sostatus = 2 OR sostatus = 3 OR sostatus = 4 OR sostatus = 7) LIMIT 1) as rowExists, '" & idsodetail & "' as idsodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
                'ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m5_so_detail JOIN m5_so ON idso = soid WHERE idsodetail = '" & idsodetail & "' AND (sostatus = 2 OR sostatus = 3) LIMIT 1) as rowExists, '" & idsodetail & "' as idsodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
                ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m5_so_detail JOIN m5_so ON idso = soid WHERE idsodetail = '" & idsodetail & "' AND (sostatus = 2 OR sostatus = 3 OR sostatus = 4) LIMIT 1) as rowExists, '" & idsodetail & "' as idsodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsodetail=" & idsodetail)
                ftOutstanding = IIf(Len(ftOutstanding.ToString) = 0, "", ftOutstanding & " OR ")
                ftOutstanding = String.Concat(ftOutstanding, " (sod.idsodetail = " & idsodetail & " AND " & Outstanding & " > (sod.jmlbarang - sod.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilai = String.Concat("WHEN '" & idsodetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilai)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                updFilter = String.Concat(updFilter, "(idsodetail = '" & idsodetail & "')")
            End If

            'Validasi harga dibawah harga jual
            ftLowerPrice = IIf(Len(ftLowerPrice.ToString) = 0, "", ftLowerPrice & " OR ")
            ftLowerPrice = String.Concat(ftLowerPrice, "(bid = '" & idbarang & "' AND bhargajual1 > " & FixDouble(harga * kurs) & ")")
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("pitgl")), AsFormatTanggal(drutama("pitgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                If drutama("pistatus") = 2 Then
                    'VALIDASI HAK AKSES PENJUALAN DIBAWAH HARGA JUAL
                    '0 = Insert, 1 = Update/Draft, 2 = Delete, 3 = GetData, 4 = Approved1, 5 = Approved2, 6 = Approved3, 
                    '7 = Approved4, 8 = Approved, 9 = Close/Unclose, 10 = Journal, 11 = History, 12 = Setting Grid
                    Dim rsHakAksesLowerPrice As String = HakAksesLowerPrice(5, 10, 8, userid, dtdetail, ftLowerPrice) 'MODULEID, MENUID, INDEKS AKSES, USERID, DATA DETAIL, FILTER BARANG SESUAI TRANSAKSI
                    If Len(rsHakAksesLowerPrice) <> 0 Then result(2) = rsHakAksesLowerPrice : Trans.Rollback() : GoTo selesai

                    'ValidasiSimpan
                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstanding, ftOutstanding, ftSO, drutama("pihargatermasukpajak"))
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("pitermin").ToString, AsFormatTanggal(drutama("pitgl")), "aptgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("pitgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TOTAL = subtotal
                drutama("pitotal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("pitotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("pitotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                If Integer.Parse(drutama("pihargatermasukpajak")) = 0 Then
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                    drutama("pitotaltransaksi") = Double.Parse(drutama("pitotal")) - Double.Parse(drutama("pijmldiskon")) + Double.Parse(drutama("pitotalpajak1detail")) + Double.Parse(drutama("pitotalpajak2detail")) + Double.Parse(drutama("pibiayalain"))

                Else
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                    drutama("pitotaltransaksi") = Double.Parse(drutama("pitotal")) - Double.Parse(drutama("pijmldiskon")) + Double.Parse(drutama("pibiayalain"))

                End If
                'END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("piid")
                    notransaksi = drutama("pinotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(piid), pinotransaksi FROM M5_pi WHERE piid='" & result(4) & "' AND pistatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(piid) FROM m5_pi WHERE pinotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_pi_history
                        Dim rsSimpanHistory As String = SimpanHistory.M5_Pi_HistorySimpan("" & paramSplit(0) & "★M5_Pi_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("pisumber")) & "▼" & FixQuotes(drutama("piid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Pi set picabang  = '" & FixQuotes(drutama("picabang")) & "', pilokasi  = '" & FixQuotes(drutama("pilokasi")) & "', pigudang  = '" & FixQuotes(drutama("pigudang")) & "', piasalbarang  = '" & FixQuotes(drutama("piasalbarang")) & "', piasalbarangkategori  = " & drutama("piasalbarangkategori") & ", pijenispenjualan  = '" & FixQuotes(drutama("pijenispenjualan")) & "', pijenispenjualankategori  = " & drutama("pijenispenjualankategori") & ", picarabayar  = " & drutama("picarabayar") & ", pisumber  = '" & FixQuotes(drutama("pisumber")) & "', piautonotransaksi  = " & drutama("piautonotransaksi") & ", pinotransaksi  = '" & FixQuotes(notransaksi) & "', pitgl  = '" & FixQuotes(AsFormatTanggal(drutama("pitgl"))) & "', pikodepa  = " & drutama("pikodepa") & ", picustomer  = " & drutama("picustomer") & ", picustomerkontak  = '" & FixQuotes(drutama("picustomerkontak")) & "', pi1alamat1  = '" & FixQuotes(drutama("pi1alamat1")) & "', pi1alamat2  = '" & FixQuotes(drutama("pi1alamat2")) & "', pi1alamat3  = '" & FixQuotes(drutama("pi1alamat3")) & "', pi2alamat1  = '" & FixQuotes(drutama("pi2alamat1")) & "', pi2alamat2  = '" & FixQuotes(drutama("pi2alamat2")) & "', pi2alamat3  = '" & FixQuotes(drutama("pi2alamat3")) & "', pibagianpenjualan  = " & drutama("pibagianpenjualan") & ", piekspedisi  = '" & FixQuotes(drutama("piekspedisi")) & "', pitglkirim  = '" & FixQuotes(AsFormatTanggal(drutama("pitglkirim"))) & "', pitermin  = '" & FixQuotes(drutama("pitermin")) & "', pitgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("pitgljatuhtempo"))) & "', piuraian  = '" & FixQuotes(drutama("piuraian")) & "', picatatan  = '" & FixQuotes(drutama("picatatan")) & "', pinoref  = '" & FixQuotes(drutama("pinoref")) & "', pitglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("pitglnoref"))) & "', pitglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("pitglpenutupan"))) & "', pimatauang  = '" & FixQuotes(drutama("pimatauang")) & "', pikurs  = '" & FixDouble(drutama("pikurs")) & "', pihargatermasukpajak  = " & drutama("pihargatermasukpajak") & ", pitotal  = '" & FixDouble(drutama("pitotal")) & "', pidiskonpersen  = '" & FixQuotes(drutama("pidiskonpersen")) & "', pijmldiskon  = '" & FixDouble(drutama("pijmldiskon")) & "', pitotalpajak1detail  = '" & FixDouble(drutama("pitotalpajak1detail")) & "', pitotalpajak2detail  = '" & FixDouble(drutama("pitotalpajak2detail")) & "', pibiayalainpersen  = '" & FixDouble(drutama("pibiayalainpersen")) & "', pibiayalain  = '" & FixDouble(drutama("pibiayalain")) & "', pitotaltransaksi  = '" & FixDouble(drutama("pitotaltransaksi")) & "', pijmlbayar  = '" & FixDouble(drutama("pijmlbayar")) & "', pirekdiskon  = '" & FixQuotes(drutama("pirekdiskon")) & "', pirekpajak1  = '" & FixQuotes(drutama("pirekpajak1")) & "', pirekpajak2  = '" & FixQuotes(drutama("pirekpajak2")) & "', pirekbiayalain  = '" & FixQuotes(drutama("pirekbiayalain")) & "', pirekbayar  = '" & FixQuotes(drutama("pirekbayar")) & "', piidsq  = " & drutama("piidsq") & ", piidso  = " & drutama("piidso") & ", pistatuspl  = " & drutama("pistatuspl") & ", pistatusdo  = " & drutama("pistatusdo") & ", pistatusdr  = " & drutama("pistatusdr") & ", pistatussi  = " & drutama("pistatussi") & ", pistatusrnr  = " & drutama("pistatusrnr") & ", pistatussr  = " & drutama("pistatussr") & ", pistatus  = " & drutama("pistatus") & ", pistatussebelumnya  = " & drutama("pistatussebelumnya") & ", pijmlrevisi  = pijmlrevisi+1, picetakanke  = " & drutama("picetakanke") & ", pimodifikasiuser  = " & drutama("pimodifikasiuser") & ", pimodifikasitgl  = NOW(), pitutupperiode  = " & drutama("pitutupperiode") & ", picustomtext1  = '" & FixQuotes(drutama("picustomtext1")) & "', picustomtext2  = '" & FixQuotes(drutama("picustomtext2")) & "', picustomtext3  = '" & FixQuotes(drutama("picustomtext3")) & "', picustomtext4  = '" & FixQuotes(drutama("picustomtext4")) & "', picustomtext5  = '" & FixQuotes(drutama("picustomtext5")) & "', picustomint1  = " & drutama("picustomint1") & ", picustomint2  = " & drutama("picustomint2") & ", picustomint3  = " & drutama("picustomint3") & ", picustomdbl1  = '" & FixDouble(drutama("picustomdbl1")) & "', picustomdbl2  = '" & FixDouble(drutama("picustomdbl2")) & "', picustomdbl3  = '" & FixDouble(drutama("picustomdbl3")) & "', picustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("picustomdate1"))) & "', picustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("picustomdate2"))) & "', picustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("picustomdate3"))) & "' where piid = '" & drutama("piid") & "'"
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

                    If drutama("piautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("picabang"), drutama("pilokasi"), drutama("pisumber"), drutama("pitgl"))
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
                        notransaksi = drutama("pinotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(piid) FROM m5_pi WHERE pinotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Pi (picabang, pilokasi, pigudang, piasalbarang, piasalbarangkategori, pijenispenjualan, pijenispenjualankategori, picarabayar, pisumber, piautonotransaksi, pinotransaksi, pitgl, pikodepa, picustomer, picustomerkontak, pi1alamat1, pi1alamat2, pi1alamat3, pi2alamat1, pi2alamat2, pi2alamat3, pibagianpenjualan, piekspedisi, pitglkirim, pitermin, pitgljatuhtempo, piuraian, picatatan, pinoref, pitglnoref, pitglpenutupan, pimatauang, pikurs, pihargatermasukpajak, pitotal, pidiskonpersen, pijmldiskon, pitotalpajak1detail, pitotalpajak2detail, pibiayalainpersen, pibiayalain, pitotaltransaksi, pijmlbayar, pirekdiskon, pirekpajak1, pirekpajak2, pirekbiayalain, pirekbayar, piidsq, piidso, pistatuspl, pistatusdo, pistatusdr, pistatussi, pistatusrnr, pistatussr, pistatus, pistatussebelumnya, pijmlrevisi, picetakanke, piinputuser, piinputtgl, pimodifikasiuser, pimodifikasitgl, piisclose, pitutupperiode, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3) values('" & FixQuotes(drutama("picabang")) & "', '" & FixQuotes(drutama("pilokasi")) & "', '" & FixQuotes(drutama("pigudang")) & "', '" & FixQuotes(drutama("piasalbarang")) & "', " & drutama("piasalbarangkategori") & ", '" & FixQuotes(drutama("pijenispenjualan")) & "', " & drutama("pijenispenjualankategori") & ", " & drutama("picarabayar") & ", '" & FixQuotes(drutama("pisumber")) & "', " & drutama("piautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("pitgl"))) & "', " & drutama("pikodepa") & ", " & drutama("picustomer") & ", '" & FixQuotes(drutama("picustomerkontak")) & "', '" & FixQuotes(drutama("pi1alamat1")) & "', '" & FixQuotes(drutama("pi1alamat2")) & "', '" & FixQuotes(drutama("pi1alamat3")) & "', '" & FixQuotes(drutama("pi2alamat1")) & "', '" & FixQuotes(drutama("pi2alamat2")) & "', '" & FixQuotes(drutama("pi2alamat3")) & "', " & drutama("pibagianpenjualan") & ", '" & FixQuotes(drutama("piekspedisi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pitglkirim"))) & "', '" & FixQuotes(drutama("pitermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pitgljatuhtempo"))) & "', '" & FixQuotes(drutama("piuraian")) & "', '" & FixQuotes(drutama("picatatan")) & "', '" & FixQuotes(drutama("pinoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pitglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("pitglpenutupan"))) & "', '" & FixQuotes(drutama("pimatauang")) & "', '" & FixDouble(drutama("pikurs")) & "', " & drutama("pihargatermasukpajak") & ", '" & FixDouble(drutama("pitotal")) & "', '" & FixQuotes(drutama("pidiskonpersen")) & "', '" & FixDouble(drutama("pijmldiskon")) & "', '" & FixDouble(drutama("pitotalpajak1detail")) & "', '" & FixDouble(drutama("pitotalpajak2detail")) & "', '" & FixDouble(drutama("pibiayalainpersen")) & "', '" & FixDouble(drutama("pibiayalain")) & "', '" & FixDouble(drutama("pitotaltransaksi")) & "', '" & FixDouble(drutama("pijmlbayar")) & "', '" & FixQuotes(drutama("pirekdiskon")) & "', '" & FixQuotes(drutama("pirekpajak1")) & "', '" & FixQuotes(drutama("pirekpajak2")) & "', '" & FixQuotes(drutama("pirekbiayalain")) & "', '" & FixQuotes(drutama("pirekbayar")) & "', " & drutama("piidsq") & ", " & drutama("piidso") & ", " & drutama("pistatuspl") & ", " & drutama("pistatusdo") & ", " & drutama("pistatusdr") & ", " & drutama("pistatussi") & ", " & drutama("pistatusrnr") & ", " & drutama("pistatussr") & ", " & drutama("pistatus") & ", " & drutama("pistatussebelumnya") & ", " & drutama("pijmlrevisi") & ", " & drutama("picetakanke") & ", " & drutama("piinputuser") & ", NOW(), " & drutama("pimodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("piisclose") & ", " & drutama("pitutupperiode") & ", '" & FixQuotes(drutama("picustomtext1")) & "', '" & FixQuotes(drutama("picustomtext2")) & "', '" & FixQuotes(drutama("picustomtext3")) & "', '" & FixQuotes(drutama("picustomtext4")) & "', '" & FixQuotes(drutama("picustomtext5")) & "', " & drutama("picustomint1") & ", " & drutama("picustomint2") & ", " & drutama("picustomint3") & ", '" & FixDouble(drutama("picustomdbl1")) & "', '" & FixDouble(drutama("picustomdbl2")) & "', '" & FixDouble(drutama("picustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("picustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("picustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("picustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select piid from M5_pi where pinotransaksi='" & notransaksi & "' AND piinputuser= '" & userid & "' order by pimodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Pi_Detail where idpi = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idpidetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idsqdetail") & ", " & dr1("idsodetail") & ", '" & FixDouble(dr1("jmlpl")) & "', " & dr1("statuspl") & ", '" & FixDouble(dr1("jmldo")) & "', " & dr1("statusdo") & ", '" & FixDouble(dr1("jmldr")) & "', " & dr1("statusdr") & ", '" & FixDouble(dr1("jmlsi")) & "', " & dr1("statussi") & ", '" & FixDouble(dr1("jmlrnr")) & "', " & dr1("statusrnr") & ", '" & FixDouble(dr1("jmlsr")) & "', " & dr1("statussr") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M5_Pi_Detail(idpidetail, idpi, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                If drutama("pistatus") = 2 Then
                    If Len(updNilai) > 0 Then
                        'UPDATE OUTSTANDING TRANSAKSI =======================================================
                        'UPDATE DETAIL
                        sql = "UPDATE m5_so_detail SET jmlrealisasi = (CASE idsodetail " & updNilai & " ELSE jmlrealisasi END) WHERE " & updFilter
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idso FROM m5_so_detail WHERE " & updFilter & " GROUP BY idso")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idso = '" & dr1("idso") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDB("SELECT idso, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_so_detail WHERE " & ftDetail & " GROUP BY idso")
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilai = "" : updFilter = ""
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
                                updNilai = String.Concat(updNilai, "WHEN '" & dr1("idso") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                                updFilter = String.Concat(updFilter, "(soid = '" & dr1("idso") & "')")
                            Next

                            sql = "UPDATE m5_so SET sostatusrealisasi = (CASE soid " & updNilai & " ELSE sostatusrealisasi END) WHERE " & updFilter
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
                Dim sumber As String = "PI", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M5_PiUpdateStatusOld(ByVal param As String) As String
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
            Dim sumber As String = "Pi", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Pitgl, Pinotransaksi, Pistatus FROM M5_Pi WHERE Piid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Pistatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m5_pi_history
            Dim rsSimpanHistory As String = SimpanHistory.M5_Pi_HistorySimpan("" & paramSplit(0) & "★M5_Pi_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m5_pi_terkait("piid = '" & idtransaksi & "'")
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idsodetail As Integer = 0
                Dim ftOutstanding As String = "", updNilai As String = "", updFilter As String = ""
                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDB("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idsodetail, urutan FROM m5_pi_detail WHERE idpi = '" & idtransaksi & "'")
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : idsodetail = dr1("idsodetail")

                        'UPDATE OUTSTANDING ---------------------------
                        If idsodetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING ----------
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsodetail=" & idsodetail)
                            updNilai = String.Concat("WHEN '" & idsodetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilai)

                            '2. SET FILTERUPDATE OUTSTANDING ----------
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(idsodetail = '" & idsodetail & "')")
                        End If
                        'END OF BUAT FILTER UNTUK UPDATE --------------------------
                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                If Len(updFilter) > 0 Then
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m5_so_detail SET jmlrealisasi = (CASE idsodetail " & updNilai & " ELSE jmlrealisasi END) WHERE " & updFilter
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idso FROM m5_so_detail WHERE " & updFilter & " GROUP BY idso")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idso = '" & dr1("idso") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDB("SELECT idso, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_so_detail WHERE " & ftDetail & " GROUP BY idso")
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilai = "" : updFilter = ""
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
                            updNilai = String.Concat(updNilai, "WHEN '" & dr1("idso") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(soid = '" & dr1("idso") & "')")
                        Next

                        sql = "UPDATE m5_so SET sostatusrealisasi = (CASE soid " & updNilai & " ELSE sostatusrealisasi END) WHERE " & updFilter
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
            sql = "UPDATE M5_Pi SET Pistatus = " & nilaiStatus & ", Pimodifikasiuser='" & userid & "', Pimodifikasitgl = NOW(), Piposting = 0, Pipostingtgl = '1971-01-01 00:00:00', Pijmlrevisi = Pijmlrevisi + 1 WHERE Piid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_PiSearch(PostWsSearch(paramSplit(0), "M5_piSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M5_PiDeleteOld(ByVal param As String) As String

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
            Dim sumber As String = "Pi", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Piid, Pinotransaksi FROM M5_Pi WHERE Piid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT picabang, pilokasi, pisumber, piautonotransaksi, pinotransaksi, pitgl"
            sql &= " FROM M5_pi"
            sql &= " WHERE piid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("picabang")
                lokasi = dtNomorNext.Rows(0)("pilokasi")
                sumber = dtNomorNext.Rows(0)("pisumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("piautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("pinotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("pitgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M5_Pi_Detail WHERE idpi = " & idtransaksi
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M5_Pi WHERE piid = " & idtransaksi
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
            Dim paramSearch As String = M5_PiSearch(PostWsSearch(paramSplit(0), "M5_PiSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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