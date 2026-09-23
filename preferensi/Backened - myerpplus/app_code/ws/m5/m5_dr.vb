Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_dr
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_DrSimpan(ByVal param As String) As String
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
        'drid(0) As Integer, drcabang(1) As String, drlokasi(2) As String, drgudang(3) As String, drasalbarang(4) As String, 
        'drasalbarangkategori(5) As Integer, drjenispenjualan(6) As String, drjenispenjualankategori(7) As Integer, drcarabayar(8) As Integer, drsumber(9) As String, 
        'drautonotransaksi(10) As Integer, drnotransaksi(11) As String, drtgl(12) As Date, drkodepa(13) As Integer, drcustomer(14) As Integer, 
        'drcustomerkontak(15) As String, dr1alamat1(16) As String, dr1alamat2(17) As String, dr1alamat3(18) As String, dr2alamat1(19) As String, 
        'dr2alamat2(20) As String, dr2alamat3(21) As String, drbagianpenjualan(22) As Integer, drbagianpengiriman(23) As Integer, drekspedisi(24) As String, 
        'drtglkirim(25) As Date, drtermin(26) As String, drtgljatuhtempo(27) As Date, druraian(28) As String, drcatatan(29) As String, 
        'drnoref(30) As String, drtglnoref(31) As Date, drtglpenutupan(32) As Date, drmatauang(33) As String, drkurs(34) As Double, 
        'drhargatermasukpajak(35) As Integer, drtotal(36) As Double, drdiskonpersen(37) As String, drjmldiskon(38) As Double, drtotalpajak1detail(39) As Double, 
        'drtotalpajak2detail(40) As Double, drbiayalainpersen(41) As Double, drbiayalain(42) As Double, drtotaltransaksi(43) As Double, drrekdiskon(44) As String, 
        'drrekpajak1(45) As String, drrekpajak2(46) As String, drrekbiayalain(47) As String, dridsq(48) As Integer, dridso(49) As Integer, 
        'dridpi(50) As Integer, dridpl(51) As Integer, driddo(52) As Integer, drstatussi(53) As Integer, drstatusrnr(54) As Integer, 
        'drstatussr(55) As Integer, drstatus(56) As Integer, drstatussebelumnya(57) As Integer, drjmlrevisi(58) As Integer, drcetakanke(59) As Integer, 
        'drinputuser(60) As Integer, drinputtgl(61) As DateTime, drmodifikasiuser(62) As Integer, drmodifikasitgl(63) As DateTime, drposting(64) As Integer, 
        'drtutupperiode(65) As Integer, drisclose(66) As Integer, drcustomtext1(67) As String, drcustomtext2(68) As String, drcustomtext3(69) As String, 
        'drcustomtext4(70) As String, drcustomtext5(71) As String, drcustomint1(72) As Integer, drcustomint2(73) As Integer, drcustomint3(74) As Integer, 
        'drcustomdbl1(75) As Double, drcustomdbl2(76) As Double, drcustomdbl3(77) As Double, drcustomdate1(78) As Date, drcustomdate2(79) As Date, 
        'drcustomdate3(80) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'drid, drcabang, drlokasi, drgudang, drasalbarang, drasalbarangkategori, drjenispenjualan, 
        'drjenispenjualankategori, drcarabayar, drsumber, drautonotransaksi, drnotransaksi, drtgl, drkodepa, 
        'drcustomer, drcustomerkontak, dr1alamat1, dr1alamat2, dr1alamat3, dr2alamat1, dr2alamat2, 
        'dr2alamat3, drbagianpenjualan, drbagianpengiriman, drekspedisi, drtglkirim, drtermin, drtgljatuhtempo, 
        'druraian, drcatatan, drnoref, drtglnoref, drtglpenutupan, drmatauang, drkurs, 
        'drhargatermasukpajak, drtotal, drdiskonpersen, drjmldiskon, drtotalpajak1detail, drtotalpajak2detail, drbiayalainpersen, 
        'drbiayalain, drtotaltransaksi, drrekdiskon, drrekpajak1, drrekpajak2, drrekbiayalain, dridsq, 
        'dridso, dridpi, dridpl, driddo, drstatussi, drstatusrnr, drstatussr, 
        'drstatus, drstatussebelumnya, drjmlrevisi, drcetakanke, drinputuser, drinputtgl, drmodifikasiuser, 
        'drmodifikasitgl, drposting, drtutupperiode, drisclose, drcustomtext1, drcustomtext2, drcustomtext3, 
        'drcustomtext4, drcustomtext5, drcustomint1, drcustomint2, drcustomint3, drcustomdbl1, drcustomdbl2, 
        'drcustomdbl3, drcustomdate1, drcustomdate2, drcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 81) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'drid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "drid required numeric." : GoTo selesai
        End If
        'drasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "drasalbarangkategori required numeric." : GoTo selesai
        End If
        'drjenispenjualankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "drjenispenjualankategori required numeric." : GoTo selesai
        End If
        'drcarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "drcarabayar required numeric." : GoTo selesai
        End If
        'drautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "drautonotransaksi required numeric." : GoTo selesai
        End If
        'drtgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "drtgl required date." : GoTo selesai
        End If
        'drkodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "drkodepa required numeric." : GoTo selesai
        End If
        'drcustomer(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "drcustomer required numeric." : GoTo selesai
        End If
        'drbagianpenjualan(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "drbagianpenjualan required numeric." : GoTo selesai
        End If
        'drbagianpengiriman(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "drbagianpengiriman required numeric." : GoTo selesai
        End If
        'drtglkirim(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "drtglkirim required date." : GoTo selesai
        End If
        'drtgljatuhtempo(27) As Date
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "drtgljatuhtempo required date." : GoTo selesai
        End If
        'drtglnoref(31) As Date
        If (IsDate(dataUtama(31)) = False) Then
            result(2) = "drtglnoref required date." : GoTo selesai
        End If
        'drtglpenutupan(32) As Date
        If (IsDate(dataUtama(32)) = False) Then
            result(2) = "drtglpenutupan required date." : GoTo selesai
        End If
        'drkurs(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "drkurs required numeric." : GoTo selesai
        End If
        'drhargatermasukpajak(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "drhargatermasukpajak required numeric." : GoTo selesai
        End If
        'drtotal(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "drtotal required numeric." : GoTo selesai
        End If
        'drjmldiskon(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "drjmldiskon required numeric." : GoTo selesai
        End If
        'drtotalpajak1detail(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "drtotalpajak1detail required numeric." : GoTo selesai
        End If
        'drtotalpajak2detail(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "drtotalpajak2detail required numeric." : GoTo selesai
        End If
        ''drbiayalainpersen(41) As Double
        'If (IsNumeric(dataUtama(41)) = False) Then
        '    result(2) = "drbiayalainpersen required numeric." : GoTo selesai
        'End If
        'drbiayalain(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "drbiayalain required numeric." : GoTo selesai
        End If
        'drtotaltransaksi(43) As Double
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "drtotaltransaksi required numeric." : GoTo selesai
        End If
        'dridsq(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "dridsq required numeric." : GoTo selesai
        End If
        'dridso(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "dridso required numeric." : GoTo selesai
        End If
        'dridpi(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "dridpi required numeric." : GoTo selesai
        End If
        'dridpl(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "dridpl required numeric." : GoTo selesai
        End If
        'driddo(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "driddo required numeric." : GoTo selesai
        End If
        'drstatussi(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "drstatussi required numeric." : GoTo selesai
        End If
        'drstatusrnr(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "drstatusrnr required numeric." : GoTo selesai
        End If
        'drstatussr(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "drstatussr required numeric." : GoTo selesai
        End If
        'drstatus(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "drstatus required numeric." : GoTo selesai
        End If
        'drstatussebelumnya(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "drstatussebelumnya required numeric." : GoTo selesai
        End If
        'drjmlrevisi(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "drjmlrevisi required numeric." : GoTo selesai
        End If
        'drcetakanke(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "drcetakanke required numeric." : GoTo selesai
        End If
        'drinputuser(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "drinputuser required numeric." : GoTo selesai
        End If
        'drinputtgl(61) As DateTime
        If (IsDate(dataUtama(61)) = False) Then
            result(2) = "drinputtgl required date." : GoTo selesai
        End If
        'drmodifikasiuser(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "drmodifikasiuser required numeric." : GoTo selesai
        End If
        'drmodifikasitgl(63) As DateTime
        If (IsDate(dataUtama(63)) = False) Then
            result(2) = "drmodifikasitgl required date." : GoTo selesai
        End If
        'drposting(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "drposting required numeric." : GoTo selesai
        End If
        'drtutupperiode(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "drtutupperiode required numeric." : GoTo selesai
        End If
        'drisclose(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "drisclose required numeric." : GoTo selesai
        End If
        'drcustomint1(72) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "drcustomint1 required numeric." : GoTo selesai
        End If
        'drcustomint2(73) As Integer
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "drcustomint2 required numeric." : GoTo selesai
        End If
        'drcustomint3(74) As Integer
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "drcustomint3 required numeric." : GoTo selesai
        End If
        'drcustomdbl1(75) As Double
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "drcustomdbl1 required numeric." : GoTo selesai
        End If
        'drcustomdbl2(76) As Double
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "drcustomdbl2 required numeric." : GoTo selesai
        End If
        'drcustomdbl3(77) As Double
        If (IsNumeric(dataUtama(77)) = False) Then
            result(2) = "drcustomdbl3 required numeric." : GoTo selesai
        End If
        'drcustomdate1(78) As Date
        If (IsDate(dataUtama(78)) = False) Then
            result(2) = "drcustomdate1 required date." : GoTo selesai
        End If
        'drcustomdate2(79) As Date
        If (IsDate(dataUtama(79)) = False) Then
            result(2) = "drcustomdate2 required date." : GoTo selesai
        End If
        'drcustomdate3(80) As Date
        If (IsDate(dataUtama(80)) = False) Then
            result(2) = "drcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'drcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "drcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "drcabang should not be more than 25 character." : GoTo selesai
        End If

        'drlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "drlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "drlokasi should not be more than 25 character." : GoTo selesai
        End If

        'drgudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "drgudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "drgudang should not be more than 25 character." : GoTo selesai
        End If

        'drsumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "drsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "drsumber should not be more than 10 character." : GoTo selesai
        End If

        'drnotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "drnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "drnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'drtgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "drtgl can't be empty" : GoTo selesai
        End If

        'drtglkirim(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "drtglkirim can't be empty" : GoTo selesai
        End If

        'drtgljatuhtempo(27) As Date
        If Len(dataUtama(27)) = 0 Then
            result(2) = "drtgljatuhtempo can't be empty" : GoTo selesai
        End If

        'drtglnoref(31) As Date
        If Len(dataUtama(31)) = 0 Then
            result(2) = "drtglnoref can't be empty" : GoTo selesai
        End If

        'drtglpenutupan(32) As Date
        If Len(dataUtama(32)) = 0 Then
            result(2) = "drtglpenutupan can't be empty" : GoTo selesai
        End If

        'drmatauang(33) As String
        If Len(dataUtama(33)) = 0 Then
            result(2) = "drmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(33)) > 25 Then
            result(2) = "drmatauang should not be more than 25 character." : GoTo selesai
        End If

        'drkurs(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "drkurs can't be empty" : GoTo selesai
        End If

        'drtotal(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "drtotal can't be empty" : GoTo selesai
        End If

        'drdiskonpersen(37) As String
        If Len(dataUtama(37)) = 0 Then
            result(2) = "drdiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(37)) > 25 Then
            result(2) = "drdiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'drjmldiskon(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "drjmldiskon can't be empty" : GoTo selesai
        End If

        'drtotalpajak1detail(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "drtotalpajak1detail can't be empty" : GoTo selesai
        End If

        'drtotalpajak2detail(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "drtotalpajak2detail can't be empty" : GoTo selesai
        End If

        'drbiayalainpersen(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "drbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(41)) > 25 Then
            result(2) = "drbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'drbiayalain(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "drbiayalain can't be empty" : GoTo selesai
        End If

        'drtotaltransaksi(43) As Double
        If Len(dataUtama(43)) = 0 Then
            result(2) = "drtotaltransaksi can't be empty" : GoTo selesai
        End If

        'drinputtgl(61) As DateTime
        If Len(dataUtama(61)) = 0 Then
            result(2) = "drinputtgl can't be empty" : GoTo selesai
        End If

        'drmodifikasitgl(63) As DateTime
        If Len(dataUtama(63)) = 0 Then
            result(2) = "drmodifikasitgl can't be empty" : GoTo selesai
        End If

        'drcustomdbl1(75) As Double
        If Len(dataUtama(75)) = 0 Then
            result(2) = "drcustomdbl1 can't be empty" : GoTo selesai
        End If

        'drcustomdbl2(76) As Double
        If Len(dataUtama(76)) = 0 Then
            result(2) = "drcustomdbl2 can't be empty" : GoTo selesai
        End If

        'drcustomdbl3(77) As Double
        If Len(dataUtama(77)) = 0 Then
            result(2) = "drcustomdbl3 can't be empty" : GoTo selesai
        End If

        'drcustomdate1(78) As Date
        If Len(dataUtama(78)) = 0 Then
            result(2) = "drcustomdate1 can't be empty" : GoTo selesai
        End If

        'drcustomdate2(79) As Date
        If Len(dataUtama(79)) = 0 Then
            result(2) = "drcustomdate2 can't be empty" : GoTo selesai
        End If

        'drcustomdate3(80) As Date
        If Len(dataUtama(80)) = 0 Then
            result(2) = "drcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "drid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drjenispenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drjenispenjualankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drcustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drcustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dr1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dr1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dr1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dr2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dr2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dr2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drbagianpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drbagianpengiriman", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drekspedisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drtglkirim", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drtermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drtgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "druraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drtglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drhargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drtotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drdiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drjmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drtotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drtotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drtotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drrekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drrekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drrekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drrekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dridsq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dridso", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dridpi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dridpl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "driddo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drstatussi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drstatusrnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drstatussr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drtutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "drid~drcabang~drlokasi~drgudang~drasalbarang~drasalbarangkategori~drjenispenjualan~drjenispenjualankategori~drcarabayar~drsumber~drautonotransaksi~drnotransaksi~drtgl~drkodepa~drcustomer~drcustomerkontak~dr1alamat1~dr1alamat2~dr1alamat3~dr2alamat1~dr2alamat2~dr2alamat3~drbagianpenjualan~drbagianpengiriman~drekspedisi~drtglkirim~drtermin~drtgljatuhtempo~druraian~drcatatan~drnoref~drtglnoref~drtglpenutupan~drmatauang~drkurs~drhargatermasukpajak~drtotal~drdiskonpersen~drjmldiskon~drtotalpajak1detail~drtotalpajak2detail~drbiayalainpersen~drbiayalain~drtotaltransaksi~drrekdiskon~drrekpajak1~drrekpajak2~drrekbiayalain~dridsq~dridso~dridpi~dridpl~driddo~drstatussi~drstatusrnr~drstatussr~drstatus~drstatussebelumnya~drjmlrevisi~drcetakanke~drinputuser~drinputtgl~drmodifikasiuser~drmodifikasitgl~drposting~drtutupperiode~drisclose~drcustomtext1~drcustomtext2~drcustomtext3~drcustomtext4~drcustomtext5~drcustomint1~drcustomint2~drcustomint3~drcustomdbl1~drcustomdbl2~drcustomdbl3~drcustomdate1~drcustomdate2~drcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'iddrdetail(0) As Integer, iddr(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, jmlkembali(6) As Double, satuan(7) As String, nilaisatuan(8) As Double, jmlbarang(9) As Double, 
        'jmlbarangkembali(10) As Double, satuanbarang(11) As String, matauang(12) As String, kurs(13) As Double, idhppkhususmasuk(14) As Integer, 
        'idhppfifomasuk(15) As Integer, harga(16) As Double, hpp(17) As Double, diskon(18) As String, jmldiskon(19) As Double, 
        'cabang(20) As String, lokasi(21) As String, gudangasal(22) As String, gudangtransit(23) As String, gudangtujuan(24) As String, 
        'gudangkembali(25) As String, rekpersediaan(26) As String, rekhargapokok(27) As String, rekdiskonpenjualan(28) As String, pajak1(29) As String, 
        'jmlpajak1(30) As Double, pajak2(31) As String, jmlpajak2(32) As Double, costcenter(33) As String, divisi(34) As String, 
        'subdivisi(35) As String, proyek(36) As String, catatan(37) As String, urutan(38) As Integer, idsqdetail(39) As Integer, 
        'idsodetail(40) As Integer, idpidetail(41) As Integer, idpldetail(42) As Integer, iddodetail(43) As Integer, jmlsi(44) As Double, 
        'statussi(45) As Integer, jmlrnr(46) As Double, statusrnr(47) As Integer, jmlsr(48) As Double, statussr(49) As Integer, 
        'isclose(50) As Integer, customtext1(51) As String, customtext2(52) As String, customtext3(53) As String, customdbl1(54) As Double, 
        'customdbl2(55) As Double, customdbl3(56) As Double, customdate1(57) As Date, customdate2(58) As Date, customdate3(59) As Date


        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'iddrdetail, iddr, idbarang, namabarang, tipebarang, jml, jmlkembali, 
        'satuan, nilaisatuan, jmlbarang, jmlbarangkembali, satuanbarang, matauang, kurs, 
        'idhppkhususmasuk, idhppfifomasuk, harga, hpp, diskon, jmldiskon, cabang, 
        'lokasi, gudangasal, gudangtransit, gudangtujuan, gudangkembali, rekpersediaan, rekhargapokok, 
        'rekdiskonpenjualan, pajak1, jmlpajak1, pajak2, jmlpajak2, costcenter, divisi, 
        'subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpidetail, 
        'idpldetail, iddodetail, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, 
        'statussr, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "iddrdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "iddr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmlkembali", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmlbarangkembali", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idhppkhususmasuk", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idhppfifomasuk", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "hpp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmldiskon", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangtransit", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangkembali", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekpersediaan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekhargapokok", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekdiskonpenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlpajak1", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "pajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlpajak2", AsEnumTypeData.AsDouble)
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
        AsDataTableTambahField(dtdetail, "iddodetail", AsEnumTypeData.AsInt64)
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

        'Variable ValidasiBatchSerial
        Dim ftBarang As String = "", ftBarangIn As String = "", ftBarangOut As String = ""

        'Variabel ValidasiSimpan
        Dim idbarang As Integer = 0, jmlbarang As Double = 0, jmlbarangkembali As Double = 0, iddodetail As Integer = 0
        Dim ftExistOutstandingDO As String = "", ftOutstandingDO As String = "", updNilaiDO As String = "", updFilterDO As String = ""
        Dim ftExistStok As String = "", ftStok As String = "", updStokOut As String = "", gudangOut As String = ""
        Dim updStokIn As String = "", gudangIn As String = ""
        Dim updStokInKembali As String = "", gudangInKembali As String = ""

        'FILTER DO, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftDO As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 60) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'iddrdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - iddrdetail required numeric." : GoTo selesai
            End If
            'iddr(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - iddr required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'jml(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'jmlkembali(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - jmlkembali required numeric." : GoTo selesai
            End If
            'nilaisatuan(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarang(9) As Double
            'jmlbarang = jml * nilaisatuan
            dataRowDetail(9) = Double.Parse(dataRowDetail(5)) * Double.Parse(dataRowDetail(8))
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - jmlbarang required numeric." : GoTo selesai
            End If
            'jmlbarangkembali(10) As Double
            'jmlbarangkembali = jmlkembali * nilaisatuan
            dataRowDetail(10) = Double.Parse(dataRowDetail(6)) * Double.Parse(dataRowDetail(8))
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - jmlbarangkembali required numeric." : GoTo selesai
            End If
            'kurs(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'idhppkhususmasuk(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - idhppkhususmasuk required numeric." : GoTo selesai
            End If
            'idhppfifomasuk(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - idhppfifomasuk required numeric." : GoTo selesai
            End If
            'harga(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'hpp(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - hpp required numeric." : GoTo selesai
            End If
            'jmldiskon(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - jmldiskon required numeric." : GoTo selesai
            End If
            'jmlpajak1(30) As Double
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak1 required numeric." : GoTo selesai
            End If
            'jmlpajak2(32) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak2 required numeric." : GoTo selesai
            End If
            'urutan(38) As Integer
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idsqdetail(39) As Integer
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - idsqdetail required numeric." : GoTo selesai
            End If
            'idsodetail(40) As Integer
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - idsodetail required numeric." : GoTo selesai
            End If
            'idpidetail(41) As Integer
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - idpidetail required numeric." : GoTo selesai
            End If
            'idpldetail(42) As Integer
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - idpldetail required numeric." : GoTo selesai
            End If
            'iddodetail(43) As Integer
            If (IsNumeric(dataRowDetail(43)) = False) Then
                result(2) = "Row : " & i & " - iddodetail required numeric." : GoTo selesai
            End If
            'jmlsi(44) As Double
            If (IsNumeric(dataRowDetail(44)) = False) Then
                result(2) = "Row : " & i & " - jmlsi required numeric." : GoTo selesai
            End If
            'statussi(45) As Integer
            If (IsNumeric(dataRowDetail(45)) = False) Then
                result(2) = "Row : " & i & " - statussi required numeric." : GoTo selesai
            End If
            'jmlrnr(46) As Double
            If (IsNumeric(dataRowDetail(46)) = False) Then
                result(2) = "Row : " & i & " - jmlrnr required numeric." : GoTo selesai
            End If
            'statusrnr(47) As Integer
            If (IsNumeric(dataRowDetail(47)) = False) Then
                result(2) = "Row : " & i & " - statusrnr required numeric." : GoTo selesai
            End If
            'jmlsr(48) As Double
            If (IsNumeric(dataRowDetail(48)) = False) Then
                result(2) = "Row : " & i & " - jmlsr required numeric." : GoTo selesai
            End If
            'statussr(49) As Integer
            If (IsNumeric(dataRowDetail(49)) = False) Then
                result(2) = "Row : " & i & " - statussr required numeric." : GoTo selesai
            End If
            'isclose(50) As Integer
            If (IsNumeric(dataRowDetail(50)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(54) As Double
            If (IsNumeric(dataRowDetail(54)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(55) As Double
            If (IsNumeric(dataRowDetail(55)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(56) As Double
            If (IsNumeric(dataRowDetail(56)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(57) As Date
            If (IsDate(dataRowDetail(57)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(58) As Date
            If (IsDate(dataRowDetail(58)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(59) As Date
            If (IsDate(dataRowDetail(59)) = False) Then
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
            If dataRowDetail(5) < 0 Then
                result(2) = "Row : " & i & " - jml can't be less than zero" : GoTo selesai
            End If

            'jmlkembali(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - jmlkembali can't be empty" : GoTo selesai
            End If
            If dataRowDetail(6) < 0 Then
                result(2) = "Row : " & i & " - jmlkembali can't be less than zero" : GoTo selesai
            End If

            If Double.Parse(dataRowDetail(5)) + Double.Parse(dataRowDetail(6)) <= 0 Then
                result(2) = "Row : " & i & " - jml and jmlkembali can't be less than or equal to zero" : GoTo selesai
            End If

            'satuan(7) As String
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(7)) > 25 Then
                result(2) = "Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'nilaisatuan(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmlbarang(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarang can't be empty" : GoTo selesai
            End If
            If dataRowDetail(9) < 0 Then
                result(2) = "Row : " & i & " - jmlbarang can't be less than zero" : GoTo selesai
            End If

            'jmlbarangkembali(10) As Double
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarangkembali can't be empty" : GoTo selesai
            End If
            If dataRowDetail(10) < 0 Then
                result(2) = "Row : " & i & " - jmlbarangkembali can't be less than zero" : GoTo selesai
            End If

            If Double.Parse(dataRowDetail(9)) + Double.Parse(dataRowDetail(10)) <= 0 Then
                result(2) = "Row : " & i & " - jmlbarang and jmlbarangkembali can't be less than or equal to zero" : GoTo selesai
            End If

            'satuanbarang(11) As String
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(11)) > 25 Then
                result(2) = "Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
            End If

            'matauang(12) As String
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(12)) > 25 Then
                result(2) = "Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'harga(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'hpp(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - hpp can't be empty" : GoTo selesai
            End If

            'diskon(18) As String
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - diskon can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(18)) > 25 Then
                result(2) = "Row : " & i & " - diskon should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskon(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskon can't be empty" : GoTo selesai
                'Else
                '    'HITUNG JMLDISKON : jml(5) As Double, harga(16) As Double, diskon(18) As String
                '    dataRowDetail(19) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(16)), FixQuotes(dataRowDetail(18).ToString))
            End If

            'gudangasal(22) As String
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - gudangasal can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(22)) > 25 Then
                result(2) = "Row : " & i & " - gudangasal should not be more than 25 character." : GoTo selesai
            End If

            'gudangtransit(23) As String
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - gudangtransit can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(23)) > 25 Then
                result(2) = "Row : " & i & " - gudangtransit should not be more than 25 character." : GoTo selesai
            End If

            'gudangtujuan(24) As String
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - gudangtujuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(24)) > 25 Then
                result(2) = "Row : " & i & " - gudangtujuan should not be more than 25 character." : GoTo selesai
            End If

            'gudangkembali(25) As String
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - gudangkembali can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(25)) > 25 Then
                result(2) = "Row : " & i & " - gudangkembali should not be more than 25 character." : GoTo selesai
            End If

            'jmlpajak1(30) As Double
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak1 can't be empty" : GoTo selesai
            End If

            'jmlpajak2(32) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak2 can't be empty" : GoTo selesai
            End If

            'jmlsi(44) As Double
            If Len(dataRowDetail(44)) = 0 Then
                result(2) = "Row : " & i & " - jmlsi can't be empty" : GoTo selesai
            End If

            'jmlrnr(46) As Double
            If Len(dataRowDetail(46)) = 0 Then
                result(2) = "Row : " & i & " - jmlrnr can't be empty" : GoTo selesai
            End If

            'jmlsr(48) As Double
            If Len(dataRowDetail(48)) = 0 Then
                result(2) = "Row : " & i & " - jmlsr can't be empty" : GoTo selesai
            End If

            'customdbl1(54) As Double
            If Len(dataRowDetail(54)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(55) As Double
            If Len(dataRowDetail(55)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(56) As Double
            If Len(dataRowDetail(56)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(57) As Date
            If Len(dataRowDetail(57)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(58) As Date
            If Len(dataRowDetail(58)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(59) As Date
            If Len(dataRowDetail(59)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "iddrdetail~iddr~idbarang~namabarang~tipebarang~jml~jmlkembali~satuan~nilaisatuan~jmlbarang~jmlbarangkembali~satuanbarang~matauang~kurs~idhppkhususmasuk~idhppfifomasuk~harga~hpp~diskon~jmldiskon~cabang~lokasi~gudangasal~gudangtransit~gudangtujuan~gudangkembali~rekpersediaan~rekhargapokok~rekdiskonpenjualan~pajak1~jmlpajak1~pajak2~jmlpajak2~costcenter~divisi~subdivisi~proyek~catatan~urutan~idsqdetail~idsodetail~idpidetail~idpldetail~iddodetail~jmlsi~statussi~jmlrnr~statusrnr~jmlsr~statussr~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56) & "~" & dataRowDetail(57) & "~" & dataRowDetail(58) & "~" & dataRowDetail(59)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If


            'Set Variabel -----------------------------------------------
            'idbarang(2) As Integer     , jmlbarang(9) As Double       , jmlbarangkembali(10) As Double
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(9) : jmlbarangkembali = dataRowDetail(10)
            'gudangtransit(23) As String  , gudangtujuan(24) As String   , gudangkembali(25) As String
            gudangOut = dataRowDetail(23) : gudangIn = dataRowDetail(24) : gudangInKembali = dataRowDetail(25)
            'iddodetail(43) As Integer
            iddodetail = dataRowDetail(43)


            'ValidasiBatchSerial
            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")

            If jmlbarangkembali > 0 Then
                'JIKA BARANG MASUK MAKA FILTER BATCH DAN SERIAL MASUK
                ftBarangIn = IIf(Len(ftBarangIn.ToString) = 0, "", ftBarangIn & " OR ")
                ftBarangIn = String.Concat(ftBarangIn, "(bid = '" & idbarang & "')")
            End If
            If jmlbarang > 0 Then
                'JIKA BARANG KELUAR MAKA FILTER BATCH DAN SERIAL KELUAR
                ftBarangOut = IIf(Len(ftBarangOut.ToString) = 0, "", ftBarangOut & " OR ")
                ftBarangOut = String.Concat(ftBarangOut, "(bid = '" & idbarang & "')")
            End If


            'ValidasiSimpan
            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'VALIDASI OUTSTANDING -------------------------
            If iddodetail <> 0 Then 'DO
                'CEK DO YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftDO = IIf(Len(ftDO.ToString) = 0, "", ftDO & " OR ")
                ftDO = String.Concat(ftDO, " (dod.iddodetail = " & iddodetail & ") ")

                '1. CEK DATA EXIST 
                ftExistOutstandingDO = IIf(Len(ftExistOutstandingDO.ToString) = 0, "", ftExistOutstandingDO & " UNION ")
                ftExistOutstandingDO = String.Concat(ftExistOutstandingDO, "SELECT EXISTS(SELECT 1 FROM m5_do_detail JOIN m5_do ON iddo = doid WHERE iddodetail = '" & iddodetail & "' AND (dostatus = 2 OR dostatus = 3 OR dostatus = 4 OR dostatus = 7) LIMIT 1) as rowExists, '" & iddodetail & "' as iddodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "iddodetail=" & iddodetail)
                Dim OutstandingKembali As Double = AsDataTableDSum(dtdetail, "jmlbarangkembali", "iddodetail=" & iddodetail)
                ftOutstandingDO = IIf(Len(ftOutstandingDO.ToString) = 0, "", ftOutstandingDO & " OR ")
                ftOutstandingDO = String.Concat(ftOutstandingDO, " (dod.iddodetail = " & iddodetail & " AND " & Outstanding + OutstandingKembali & " > (dod.jmlbarang - dod.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING
                updNilaiDO = String.Concat("WHEN '" & iddodetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding + OutstandingKembali & "', 5) ", updNilaiDO)

                '4. SET FILTER UPDATE OUTSTANDING
                updFilterDO = IIf(Len(updFilterDO.ToString) = 0, "", updFilterDO & " OR ")
                updFilterDO = String.Concat(updFilterDO, "(iddodetail = '" & iddodetail & "')")
            End If

            'VALIDASI STOK -------------------------------
            '1. CEK DATA EXIST STOK KELUAR
            ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
            ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

            '2. CEK JML STOK KELUAR
            Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangtransit='" & gudangOut & "'")
            ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
            ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > isw.stok) ")

            '3. SET NILAI UPDATE STOK KELUAR
            updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
            updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang + jmlbarangkembali & "'))") ' idbarang, kgudang, stok

            '4. SET NILAI UPDATE STOK MASUK
            updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
            updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok

            '5. SET NILAI UPDATE STOK KEMBALI
            updStokInKembali = IIf(Len(updStokInKembali.ToString) = 0, "", updStokInKembali & ", ")
            updStokInKembali = String.Concat(updStokInKembali, "('" & idbarang & "', '" & gudangInKembali & "', '" & jmlbarangkembali & "')") ' idbarang, kgudang, stok
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
        Dim jenismutasi As Double = 0
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
                'nbtjenismutasi(1) As Integer
                jenismutasi = dataRowBatch(1)
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
                'nstjenismutasi(1) As Integer
                jenismutasi = dataRowSerial(1)
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
                vStatus = drutama("drstatus")
                vTgl = AsFormatTanggal(drutama("drtgl"))

                'CEK HAK AKSES STATUS ============================
                Dim vAkses As Integer = 0, msgAkses As String = ""
                'MODUL DAN MENU HARUS DISESUAIKAN
                Dim vModuleId As Integer = 5, vMenuId As Integer = 8
                Select Case drutama("drstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("drtgl")), AsFormatTanggal(drutama("drtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("drstatus") = 2 Or drutama("drstatus") = 1 Or drutama("drstatus") = 8 Or drutama("drstatus") = 9 Or drutama("drstatus") = 10 Or drutama("drstatus") = 11 Then

                    Dim rsValidasi As String

                    'VALIDASI BATCH SERIAL IN ---------------
                    'ValidasiBatchSerial
                    If Len(ftBarangIn) > 0 Then
                        rsValidasi = ValidasiBatchSerial(dtdetail, dtbatch, dtserial, ftBarangIn, "jmlbarangkembali", 1)
                        If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai

                        'ValidasiAsset
                        rsValidasi = ValidasiAsset(dtdetail, dtasset, ftBarangIn, "jmlbarangkembali", 1)
                        If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    End If
                    'END OF VALIDASI BATCH SERIAL IN --------

                    'VALIDASI BATCH SERIAL OUT ---------------
                    'ValidasiBatchSerial
                    If Len(ftBarangOut) > 0 Then
                        rsValidasi = ValidasiBatchSerial(dtdetail, dtbatch, dtserial, ftBarangOut, "jmlbarang", 0)
                        If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai

                        'ValidasiAsset
                        rsValidasi = ValidasiAsset(dtdetail, dtasset, ftBarangOut, "jmlbarang", 0)
                        If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    End If
                    'END OF VALIDASI BATCH SERIAL OUT --------

                    'ValidasiGudangAsset
                    rsValidasi = ValidasiGudangAsset(dtasset, gudangOut)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai

                    rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstandingDO, ftOutstandingDO, ftExistStok, ftStok, ftExistBatch, ftBatch, ftExistSerial, ftSerial, "gudangtransit", ftDO, drutama("drhargatermasukpajak"))
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("drtermin").ToString, AsFormatTanggal(drutama("drtgl")), "drtgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("drtgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                ''PERHITUNGAN TOTAL UTAMA ================================
                ''DIAMBILKAN DARI DATA DETAIL

                ''TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                ''SUBTOTAL = (jml * harga) - jmldiskon
                'AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                'dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                ''TOTAL = subtotal
                'drutama("drtotal") = AsDataTableDSum(dtdetail, "subtotal")

                ''TOTALPAJAK1 = jmlpajak1
                'drutama("drtotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                ''TOTALPAJAK2 = jmlpajak2
                'drutama("drtotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                ''JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                ''JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'If Integer.Parse(drutama("drhargatermasukpajak")) = 0 Then
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                '    drutama("drtotaltransaksi") = Double.Parse(drutama("drtotal")) - Double.Parse(drutama("drjmldiskon")) + Double.Parse(drutama("drtotalpajak1detail")) + Double.Parse(drutama("drtotalpajak2detail")) + Double.Parse(drutama("drbiayalain"))

                'Else
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                '    drutama("drtotaltransaksi") = Double.Parse(drutama("drtotal")) - Double.Parse(drutama("drjmldiskon")) + Double.Parse(drutama("drbiayalain"))

                'End If
                ''END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("drid")
                    notransaksi = drutama("drnotransaksi")

                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(drid), drnotransaksi FROM M5_dr WHERE drid='" & result(4) & "' AND drstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("drautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("drcabang"), drutama("drlokasi"), drutama("drsumber"), drutama("drtgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(drid) FROM m5_dr WHERE drnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_dr_history
                        Dim rsSimpanHistory As String = SimpanHistory.m5_Dr_HistorySimpan("" & paramSplit(0) & "★M5_Dr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("drsumber")) & "▼" & FixQuotes(drutama("drid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Dr set drcabang  = '" & FixQuotes(drutama("drcabang")) & "', drlokasi  = '" & FixQuotes(drutama("drlokasi")) & "', drgudang  = '" & FixQuotes(drutama("drgudang")) & "', drasalbarang  = '" & FixQuotes(drutama("drasalbarang")) & "', drasalbarangkategori  = " & drutama("drasalbarangkategori") & ", drjenispenjualan  = '" & FixQuotes(drutama("drjenispenjualan")) & "', drjenispenjualankategori  = " & drutama("drjenispenjualankategori") & ", drcarabayar  = " & drutama("drcarabayar") & ", drsumber  = '" & FixQuotes(drutama("drsumber")) & "', drautonotransaksi  = " & drutama("drautonotransaksi") & ", drnotransaksi  = '" & FixQuotes(notransaksi) & "', drtgl  = '" & FixQuotes(AsFormatTanggal(drutama("drtgl"))) & "', drkodepa  = " & drutama("drkodepa") & ", drcustomer  = " & drutama("drcustomer") & ", drcustomerkontak  = '" & FixQuotes(drutama("drcustomerkontak")) & "', dr1alamat1  = '" & FixQuotes(drutama("dr1alamat1")) & "', dr1alamat2  = '" & FixQuotes(drutama("dr1alamat2")) & "', dr1alamat3  = '" & FixQuotes(drutama("dr1alamat3")) & "', dr2alamat1  = '" & FixQuotes(drutama("dr2alamat1")) & "', dr2alamat2  = '" & FixQuotes(drutama("dr2alamat2")) & "', dr2alamat3  = '" & FixQuotes(drutama("dr2alamat3")) & "', drbagianpenjualan  = " & drutama("drbagianpenjualan") & ", drbagianpengiriman  = " & drutama("drbagianpengiriman") & ", drekspedisi  = '" & FixQuotes(drutama("drekspedisi")) & "', drtglkirim  = '" & FixQuotes(AsFormatTanggal(drutama("drtglkirim"))) & "', drtermin  = '" & FixQuotes(drutama("drtermin")) & "', drtgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("drtgljatuhtempo"))) & "', druraian  = '" & FixQuotes(drutama("druraian")) & "', drcatatan  = '" & FixQuotes(drutama("drcatatan")) & "', drnoref  = '" & FixQuotes(drutama("drnoref")) & "', drtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("drtglnoref"))) & "', drtglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("drtglpenutupan"))) & "', drmatauang  = '" & FixQuotes(drutama("drmatauang")) & "', drkurs  = '" & FixDouble(drutama("drkurs")) & "', drhargatermasukpajak  = " & drutama("drhargatermasukpajak") & ", drtotal  = '" & FixDouble(drutama("drtotal")) & "', drdiskonpersen  = '" & FixQuotes(drutama("drdiskonpersen")) & "', drjmldiskon  = '" & FixDouble(drutama("drjmldiskon")) & "', drtotalpajak1detail  = '" & FixDouble(drutama("drtotalpajak1detail")) & "', drtotalpajak2detail  = '" & FixDouble(drutama("drtotalpajak2detail")) & "', drbiayalainpersen  = '" & FixDouble(drutama("drbiayalainpersen")) & "', drbiayalain  = '" & FixDouble(drutama("drbiayalain")) & "', drtotaltransaksi  = '" & FixDouble(drutama("drtotaltransaksi")) & "', drrekdiskon  = '" & FixQuotes(drutama("drrekdiskon")) & "', drrekpajak1  = '" & FixQuotes(drutama("drrekpajak1")) & "', drrekpajak2  = '" & FixQuotes(drutama("drrekpajak2")) & "', drrekbiayalain  = '" & FixQuotes(drutama("drrekbiayalain")) & "', dridsq  = " & drutama("dridsq") & ", dridso  = " & drutama("dridso") & ", dridpi  = " & drutama("dridpi") & ", dridpl  = " & drutama("dridpl") & ", driddo  = " & drutama("driddo") & ", drstatussi  = " & drutama("drstatussi") & ", drstatusrnr  = " & drutama("drstatusrnr") & ", drstatussr  = " & drutama("drstatussr") & ", drstatus  = " & drutama("drstatus") & ", drstatussebelumnya  = " & drutama("drstatussebelumnya") & ", drjmlrevisi  = drjmlrevisi+1, drcetakanke  = " & drutama("drcetakanke") & ", drmodifikasiuser  = " & drutama("drmodifikasiuser") & ", drmodifikasitgl  = NOW(), drposting  = 0, drtutupperiode  = " & drutama("drtutupperiode") & ", drcustomtext1  = '" & FixQuotes(drutama("drcustomtext1")) & "', drcustomtext2  = '" & FixQuotes(drutama("drcustomtext2")) & "', drcustomtext3  = '" & FixQuotes(drutama("drcustomtext3")) & "', drcustomtext4  = '" & FixQuotes(drutama("drcustomtext4")) & "', drcustomtext5  = '" & FixQuotes(drutama("drcustomtext5")) & "', drcustomint1  = " & drutama("drcustomint1") & ", drcustomint2  = " & drutama("drcustomint2") & ", drcustomint3  = " & drutama("drcustomint3") & ", drcustomdbl1  = '" & FixDouble(drutama("drcustomdbl1")) & "', drcustomdbl2  = '" & FixDouble(drutama("drcustomdbl2")) & "', drcustomdbl3  = '" & FixDouble(drutama("drcustomdbl3")) & "', drcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("drcustomdate1"))) & "', drcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("drcustomdate2"))) & "', drcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("drcustomdate3"))) & "' where drid = '" & drutama("drid") & "'"
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

                    If drutama("drautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("drcabang"), drutama("drlokasi"), drutama("drsumber"), drutama("drtgl"))
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
                        notransaksi = drutama("drnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(drid) FROM m5_dr WHERE drnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Dr (drcabang, drlokasi, drgudang, drasalbarang, drasalbarangkategori, drjenispenjualan, drjenispenjualankategori, drcarabayar, drsumber, drautonotransaksi, drnotransaksi, drtgl, drkodepa, drcustomer, drcustomerkontak, dr1alamat1, dr1alamat2, dr1alamat3, dr2alamat1, dr2alamat2, dr2alamat3, drbagianpenjualan, drbagianpengiriman, drekspedisi, drtglkirim, drtermin, drtgljatuhtempo, druraian, drcatatan, drnoref, drtglnoref, drtglpenutupan, drmatauang, drkurs, drhargatermasukpajak, drtotal, drdiskonpersen, drjmldiskon, drtotalpajak1detail, drtotalpajak2detail, drbiayalainpersen, drbiayalain, drtotaltransaksi, drrekdiskon, drrekpajak1, drrekpajak2, drrekbiayalain, dridsq, dridso, dridpi, dridpl, driddo, drstatussi, drstatusrnr, drstatussr, drstatus, drstatussebelumnya, drjmlrevisi, drcetakanke, drinputuser, drinputtgl, drmodifikasiuser, drmodifikasitgl, drposting, drtutupperiode, drisclose, drcustomtext1, drcustomtext2, drcustomtext3, drcustomtext4, drcustomtext5, drcustomint1, drcustomint2, drcustomint3, drcustomdbl1, drcustomdbl2, drcustomdbl3, drcustomdate1, drcustomdate2, drcustomdate3) values('" & FixQuotes(drutama("drcabang")) & "', '" & FixQuotes(drutama("drlokasi")) & "', '" & FixQuotes(drutama("drgudang")) & "', '" & FixQuotes(drutama("drasalbarang")) & "', " & drutama("drasalbarangkategori") & ", '" & FixQuotes(drutama("drjenispenjualan")) & "', " & drutama("drjenispenjualankategori") & ", " & drutama("drcarabayar") & ", '" & FixQuotes(drutama("drsumber")) & "', " & drutama("drautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("drtgl"))) & "', " & drutama("drkodepa") & ", " & drutama("drcustomer") & ", '" & FixQuotes(drutama("drcustomerkontak")) & "', '" & FixQuotes(drutama("dr1alamat1")) & "', '" & FixQuotes(drutama("dr1alamat2")) & "', '" & FixQuotes(drutama("dr1alamat3")) & "', '" & FixQuotes(drutama("dr2alamat1")) & "', '" & FixQuotes(drutama("dr2alamat2")) & "', '" & FixQuotes(drutama("dr2alamat3")) & "', " & drutama("drbagianpenjualan") & ", " & drutama("drbagianpengiriman") & ", '" & FixQuotes(drutama("drekspedisi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("drtglkirim"))) & "', '" & FixQuotes(drutama("drtermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("drtgljatuhtempo"))) & "', '" & FixQuotes(drutama("druraian")) & "', '" & FixQuotes(drutama("drcatatan")) & "', '" & FixQuotes(drutama("drnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("drtglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("drtglpenutupan"))) & "', '" & FixQuotes(drutama("drmatauang")) & "', '" & FixDouble(drutama("drkurs")) & "', " & drutama("drhargatermasukpajak") & ", '" & FixDouble(drutama("drtotal")) & "', '" & FixQuotes(drutama("drdiskonpersen")) & "', '" & FixDouble(drutama("drjmldiskon")) & "', '" & FixDouble(drutama("drtotalpajak1detail")) & "', '" & FixDouble(drutama("drtotalpajak2detail")) & "', '" & FixDouble(drutama("drbiayalainpersen")) & "', '" & FixDouble(drutama("drbiayalain")) & "', '" & FixDouble(drutama("drtotaltransaksi")) & "', '" & FixQuotes(drutama("drrekdiskon")) & "', '" & FixQuotes(drutama("drrekpajak1")) & "', '" & FixQuotes(drutama("drrekpajak2")) & "', '" & FixQuotes(drutama("drrekbiayalain")) & "', " & drutama("dridsq") & ", " & drutama("dridso") & ", " & drutama("dridpi") & ", " & drutama("dridpl") & ", " & drutama("driddo") & ", " & drutama("drstatussi") & ", " & drutama("drstatusrnr") & ", " & drutama("drstatussr") & ", " & drutama("drstatus") & ", " & drutama("drstatussebelumnya") & ", " & drutama("drjmlrevisi") & ", " & drutama("drcetakanke") & ", " & drutama("drinputuser") & ", NOW(), " & drutama("drmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("drtutupperiode") & ", " & drutama("drisclose") & ", '" & FixQuotes(drutama("drcustomtext1")) & "', '" & FixQuotes(drutama("drcustomtext2")) & "', '" & FixQuotes(drutama("drcustomtext3")) & "', '" & FixQuotes(drutama("drcustomtext4")) & "', '" & FixQuotes(drutama("drcustomtext5")) & "', " & drutama("drcustomint1") & ", " & drutama("drcustomint2") & ", " & drutama("drcustomint3") & ", '" & FixDouble(drutama("drcustomdbl1")) & "', '" & FixDouble(drutama("drcustomdbl2")) & "', '" & FixDouble(drutama("drcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("drcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("drcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("drcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select drid from M5_dr where drnotransaksi='" & notransaksi & "' AND drinputuser= '" & userid & "' order by drmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Dr_Detail where iddr = '" & result(4) & "'"
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
                        If Not drutama("drmatauang").ToString.Equals(dr1("matauang").ToString) Then
                            result(2) = "Row : " & dr1("urutan") & " - " & dr1("tipebarang") & " | " & dr1("namabarang") & " currency (" & dr1("matauang") & ") doesn't belong to the main transactions." : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF VALIDASI MATAUANG HARUS SAMA DENGAN TRANSAKSI UTAMA -------------


                        'SET HARGA DARI TRANSAKSI SEBELUMNYA ------------------------------------
                        If Double.Parse(dr1("iddodetail")) > 0 Then
                            'JIKA AMBIL DO MAKA SET HARGA DARI DO
                            sql = "SELECT jml, harga, diskon, pajak1, jmlpajak1, pajak2, jmlpajak2 FROM m5_do_detail WHERE iddodetail = '" & FixDouble(dr1("iddodetail")) & "'"

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
                        strValue2.Append("(" & dr1("iddrdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixDouble(dr1("jmlkembali")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixDouble(dr1("jmlbarangkembali")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', " & dr1("idhppkhususmasuk") & ", " & dr1("idhppfifomasuk") & ", '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hpp")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("gudangkembali")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("rekhargapokok")) & "', '" & FixQuotes(dr1("rekdiskonpenjualan")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idsqdetail") & ", " & dr1("idsodetail") & ", " & dr1("idpidetail") & ", " & dr1("idpldetail") & ", " & dr1("iddodetail") & ", '" & FixDouble(dr1("jmlsi")) & "', " & dr1("statussi") & ", '" & FixDouble(dr1("jmlrnr")) & "', " & dr1("statusrnr") & ", '" & FixDouble(dr1("jmlsr")) & "', " & dr1("statussr") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M5_Dr_Detail(iddrdetail, iddr, idbarang, namabarang, tipebarang, jml, jmlkembali, satuan, nilaisatuan, jmlbarang, jmlbarangkembali, satuanbarang, matauang, kurs, idhppkhususmasuk, idhppfifomasuk, harga, hpp, diskon, jmldiskon, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, gudangkembali, rekpersediaan, rekhargapokok, rekdiskonpenjualan, pajak1, jmlpajak1, pajak2, jmlpajak2, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpidetail, idpldetail, iddodetail, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                    sql = "Delete from M1_No_Batch_Transaction where nbtidtransaksi  = '" & result(4) & "' AND nbtsumber = 'DR'"
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
                    sql = "Delete from M1_No_Serial_Transaction  where nstidtransaksi  = '" & result(4) & "' AND nstsumber = 'DR'"
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
                    sql = "Delete from M7_Asset_Transaction where atidutama  = '" & result(4) & "' AND atsumber = 'DR'"
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


                If drutama("drstatus") = 2 Then
                    'UPDATE OUTSTANDING TRANSAKSI ======================================================
                    If Len(updNilaiDO) > 0 Then 'DO
                        'UPDATE DETAIL
                        sql = "UPDATE m5_do_detail SET jmlrealisasi = (CASE iddodetail " & updNilaiDO & " ELSE jmlrealisasi END) WHERE " & updFilterDO
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT iddo FROM m5_do_detail WHERE " & updFilterDO & " GROUP BY iddo", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(iddo = '" & dr1("iddo") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDBCon("SELECT iddo, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_do_detail WHERE " & ftDetail & " GROUP BY iddo", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiDO = "" : updFilterDO = ""
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
                                updNilaiDO = String.Concat(updNilaiDO, "WHEN '" & dr1("iddo") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterDO = IIf(Len(updFilterDO.ToString) = 0, "", updFilterDO & " OR ")
                                updFilterDO = String.Concat(updFilterDO, "(doid = '" & dr1("iddo") & "')")
                            Next

                            sql = "UPDATE m5_do SET dostatusrealisasi = (CASE doid " & updNilaiDO & " ELSE dostatusrealisasi END) WHERE " & updFilterDO
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
                            'mapping        nbiidbatchin,                     nbigudang,                  nbiidbarang,                           nbikode,                             nbisumber,            nbiidtransaksi,                     nbisatuan,                 nbijmlmasuk,       nbijmlkeluar,                  nbijmlsisa, nbiisclose,                     nbicustomtext1,                             nbicustomtext2,                             nbicustomtext3,                             nbicustomdbl1,                             nbicustomdbl2,                             nbicustomdbl3,                                             nbicustomdate1,                                              nbicustomdate2,                                              nbicustomdate3
                            strValue3.Append("(" & 0 & ", '" & FixQuotes(dr1("nbtgudang")) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixQuotes(dr1("nbtcustomtext1")) & "', '" & FixQuotes(dr1("nbtcustomtext2")) & "', '" & FixQuotes(dr1("nbtcustomtext3")) & "', '" & FixDouble(dr1("nbtcustomdbl1")) & "', '" & FixDouble(dr1("nbtcustomdbl2")) & "', '" & FixDouble(dr1("nbtcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate3"))) & "')")
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
                            'mapping       nsiidserialin,                     nsigudang,                  nsiidbarang,                           nsikode,                             nsisumber,            nsiidtransaksi,                     nsisatuan,                       nsijmlmasuk, nsijmlkeluar,                  nsijmlsisa, nsiisclose,                     nsicustomtext1,                             nsicustomtext2,                             nsicustomtext3,                             nsicustomdbl1,                             nsicustomdbl2,                             nsicustomdbl3,                                             nsicustomdate1,                                              nsicustomdate2,                                              nsicustomdate3
                            strValue3.Append("(" & 0 & ", '" & FixQuotes(dr1("nstgudang")) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixQuotes(dr1("nstcustomtext1")) & "', '" & FixQuotes(dr1("nstcustomtext2")) & "', '" & FixQuotes(dr1("nstcustomtext3")) & "', '" & FixDouble(dr1("nstcustomdbl1")) & "', '" & FixDouble(dr1("nstcustomdbl2")) & "', '" & FixDouble(dr1("nstcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate3"))) & "')")
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
                    Dim dtAssetIn As DataTable = AsDataTableFilterSortDt(dtasset, "atjenismutasi = '1'")
                    If dtAssetIn.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder
                        For Each dr1 As DataRow In dtAssetIn.Rows
                            'QUERY INSERT NO ASSET IN
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            strValue2.Append(FixDouble(dr1("atasetid")))
                        Next
                        sql = "UPDATE m7_asset a SET a.agudang = '" & gudangInKembali & "' WHERE a.aid IN(" & strValue2.ToString & ")"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If

                    Dim dtAssetOut As DataTable = AsDataTableFilterSortDt(dtasset, "atjenismutasi = '0'")
                    If dtAssetOut.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder
                        For Each dr1 As DataRow In dtAssetOut.Rows
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
                    'END OF INSERT NO ASSET ========================================================


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

                    'STOK KEMBALI
                    If Len(updStokInKembali) > 0 Then
                        sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokInKembali & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
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


                    'INSERT ITEM TRANSACTION ========================================================
                    'AMBIL DATA DETAIL YANG BARU
                    sql = "SELECT drd.iddrdetail, drd.idbarang, drd.namabarang, drd.tipebarang, drd.jml, drd.jmlbarang, drd.jmlkembali, drd.jmlbarangkembali, drd.satuan, drd.satuanbarang, drd.matauang, drd.kurs, drd.harga, drd.diskon, drd.jmldiskon, drd.hpp, drd.idhppkhususmasuk, drd.gudangasal, drd.gudangtransit, drd.gudangtujuan, drd.gudangkembali, drd.catatan, drd.costcenter, drd.divisi, drd.subdivisi, drd.proyek, dr.drinputtgl, i.bhpp FROM m5_dr_detail drd JOIN m5_dr dr ON drd.iddr = dr.drid JOIN m1_item i ON drd.idbarang = i.bid WHERE drd.iddr = '" & result(4) & "'"
                    Dim dtDetailNew As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                    Dim hpp As Double = 0, postinghpp As Double = 0
                    Dim strTransaksiBarang As New StringBuilder

                    Dim jmlTransaksi As Double = 0, jmlTransaksiKembali As Double = 0

                    If dtDetailNew.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtDetailNew.Rows
                            'jenismutasi dan postinghpp 
                            '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 1
                            '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                            '- untuk transaksi mutasi saja maka postinghpp = 0
                            postinghpp = 0

                            'jml
                            jmlTransaksi = Double.Parse(dr1("jml"))
                            jmlTransaksiKembali = Double.Parse(dr1("jmlkembali"))

                            'jmlbarang
                            jmlbarang = Double.Parse(dr1("jmlbarang"))
                            jmlbarangkembali = Double.Parse(dr1("jmlbarangkembali"))

                            'hitung hpp = hpp
                            hpp = Double.Parse(dr1("hpp"))

                            'POSTING BARANG KELUAR (gudangtransit) == jmlbarang + jmlbarangkembali
                            jenismutasi = 0
                            'QUERY INSERT TRANSAKSI BARANG KELUAR
                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                            'mapping                        id,                             cabang,                                   lokasi,                                   gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,             iddetail,                    notransaksi,                                                 tgl,                              kontak,               idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                                                       satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                  idhppikm,                idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("drcabang")) & "', '" & FixQuotes(drutama("drlokasi")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', " & drutama("drkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("drsumber")) & "', " & result(4) & ", " & dr1("iddrdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("drtgl"))) & "', " & drutama("drcustomer") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(jmlTransaksi + jmlTransaksiKembali) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(jmlbarang + jmlbarangkembali) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("druraian")) & "', '" & FixQuotes(drutama("drcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("drinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("drinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")

                            'POSTING BARANG MASUK (gudangkembali)
                            If jmlbarangkembali <> 0 Then
                                jenismutasi = 1
                                'QUERY INSERT TRANSAKSI BARANG MASUK
                                strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                                'mapping                        id,                             cabang,                                   lokasi,                                  gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,             iddetail,                    notransaksi,                                                 tgl,                              kontak,               idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                                         satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                  idhppikm,                idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("drcabang")) & "', '" & FixQuotes(drutama("drlokasi")) & "', '" & FixQuotes(dr1("gudangkembali")) & "', " & drutama("drkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("drsumber")) & "', " & result(4) & ", " & dr1("iddrdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("drtgl"))) & "', " & drutama("drcustomer") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(jmlTransaksiKembali) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(jmlbarangkembali) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("druraian")) & "', '" & FixQuotes(drutama("drcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("drinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("drinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                            End If

                            'POSTING BARANG MASUK (gudangtujuan)
                            jenismutasi = 1
                            'QUERY INSERT TRANSAKSI BARANG MASUK
                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                            'mapping                        id,                             cabang,                                   lokasi,                                  gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,             iddetail,                    notransaksi,                                                 tgl,                              kontak,               idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                                 satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                  idhppikm,                idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("drcabang")) & "', '" & FixQuotes(drutama("drlokasi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', " & drutama("drkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("drsumber")) & "', " & result(4) & ", " & dr1("iddrdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("drtgl"))) & "', " & drutama("drcustomer") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(jmlTransaksi) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("druraian")) & "', '" & FixQuotes(drutama("drcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("drinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("drinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                        Next

                        sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
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
                Dim sumber As String = "DR", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M5_DrUpdateStatus(ByVal param As String) As String

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
            Dim sumber As String = "DR", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Drtgl, Drnotransaksi, Drstatus FROM M5_Dr WHERE Drid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Drstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m5_dr_history
            Dim rsSimpanHistory As String = SimpanHistory.m5_Dr_HistorySimpan("" & paramSplit(0) & "★M5_Dr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m5_dr_terkait("drid = '" & idtransaksi & "'")
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


                Dim idbarang As Integer = 0, jmlbarang As Double = 0, jmlbarangkembali As Double = 0, iddodetail As Integer = 0
                Dim updNilaiDO As String = "", updFilterDO As String = ""
                Dim ftExistStok As String = "", ftStok As String = "", updStokOut As String = "", gudangOut As String = ""
                Dim ftExistStokKembali As String = "", ftStokKembali As String = "", updStokOutKembali As String = "", gudangOutKembali As String = ""
                Dim updStokIn As String = "", gudangIn As String = ""

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT iddrdetail, idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, jmlbarangkembali, iddodetail, gudangtransit, gudangtujuan, gudangkembali, idhppkhususmasuk, idhppfifomasuk, urutan FROM m5_dr_detail WHERE iddr = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        '1. SET NILAI
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : jmlbarangkembali = dr1("jmlbarangkembali")
                        gudangIn = dr1("gudangtransit") : gudangOut = dr1("gudangtujuan") : gudangOutKembali = dr1("gudangkembali")
                        iddodetail = dr1("iddodetail")

                        '2. BUAT FILTER UPDATE OUTSTANDING
                        If iddodetail <> 0 Then
                            '2.1 SET NILAI UPDATE OUTSTANDING DO
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "iddodetail=" & iddodetail)
                            Dim OutstandingKembali As Double = AsDataTableDSum(dtdetail, "jmlbarangkembali", "iddodetail=" & iddodetail)
                            updNilaiDO = String.Concat("WHEN '" & iddodetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding + OutstandingKembali & "', 5) ", updNilaiDO)

                            '2.2. SET FILTERUPDATE OUTSTANDING DO
                            updFilterDO = IIf(Len(updFilterDO.ToString) = 0, "", updFilterDO & " OR ")
                            updFilterDO = String.Concat(updFilterDO, "(iddodetail = '" & iddodetail & "')")
                        End If

                        'VALIDASI STOK -------------------------------
                        '1. CEK DATA EXIST STOK TUJUAN
                        ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                        ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists,  bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                        '2. CEK JML STOK TUJUAN
                        Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangtujuan='" & gudangOut & "'")
                        ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
                        ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > isw.stok) ")

                        '3. SET NILAI UPDATE STOK KELUAR TUJUAN
                        updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
                        updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

                        '4. CEK DATA EXIST STOK KEMBALI
                        ftExistStokKembali = IIf(Len(ftExistStokKembali.ToString) = 0, "", ftExistStokKembali & " UNION ")
                        ftExistStokKembali = String.Concat(ftExistStokKembali, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOutKembali & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists,  bkode, '" & gudangOutKembali & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                        '5. CEK JML STOK KEMBALI
                        Dim StokKembali As Double = AsDataTableDSum(dtdetail, "jmlbarangkembali", "idbarang=" & idbarang & " AND gudangkembali='" & gudangOutKembali & "'")
                        ftStokKembali = IIf(Len(ftStokKembali.ToString) = 0, "", ftStokKembali & " OR ")
                        ftStokKembali = String.Concat(ftStokKembali, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOutKembali & "' AND " & StokKembali & " > isw.stok) ")

                        '6. SET NILAI UPDATE STOK KELUAR KEMBALI
                        updStokOutKembali = IIf(Len(updStokOutKembali.ToString) = 0, "", updStokOutKembali & ", ")
                        updStokOutKembali = String.Concat(updStokOutKembali, "('" & idbarang & "', '" & gudangOutKembali & "', ('-" & jmlbarangkembali & "'))") ' idbarang, kgudang, stok

                        '7. SET NILAI UPDATE STOK MASUK 
                        updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
                        updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang + jmlbarangkembali & "')") ' idbarang, kgudang, stok

                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If


                'VALIDASI STOK ----------------------------------
                'STOK TUJUAN
                Dim rsValidasi As String = ValidasiSimpan(dtdetail, "", "", ftExistStok, ftStok, "", "", "", "", "", "", "")
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai

                'STOK KEMBALI
                rsValidasi = ValidasiSimpan(dtdetail, "", "", ftExistStokKembali, ftStokKembali, "", "", "", "", "", "", "")
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'END OF VALIDASI STOK ---------------------------


                'VALIDASI GUDANG ASSET ---------------
                'ValidasiGudangAsset
                dtasset = AsDataTableAmbilDariDBCon("SELECT atasetid, atidbarang, atkode FROM M7_Asset_Transaction WHERE atsumber = '" & sumber & "' AND atidutama = '" & idtransaksi & "' ", myConn)

                'GUDANG KELUAR
                rsValidasi = ValidasiGudangAsset(dtasset, gudangOut, 0)
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai

                'GUDANG KEMBALI
                rsValidasi = ValidasiGudangAsset(dtasset, gudangOutKembali, 1)
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                'END OF VALIDASI GUDANG ASSET --------


                'UPDATE OUTSTANDING =============================================================
                If Len(updFilterDO) > 0 Then
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m5_do_detail SET jmlrealisasi = (CASE iddodetail " & updNilaiDO & " ELSE jmlrealisasi END) WHERE " & updFilterDO
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT iddo FROM m5_do_detail WHERE " & updFilterDO & " GROUP BY iddo", myConn)
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(iddo = '" & dr1("iddo") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDBCon("SELECT iddo, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_do_detail WHERE " & ftDetail & " GROUP BY iddo", myConn)
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiDO = "" : updFilterDO = ""
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
                            updNilaiDO = String.Concat(updNilaiDO, "WHEN '" & dr1("iddo") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterDO = IIf(Len(updFilterDO.ToString) = 0, "", updFilterDO & " OR ")
                            updFilterDO = String.Concat(updFilterDO, "(doid = '" & dr1("iddo") & "')")
                        Next

                        sql = "UPDATE m5_do SET dostatusrealisasi = (CASE doid " & updNilaiDO & " ELSE dostatusrealisasi END) WHERE " & updFilterDO
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


                'UPDATE STOK ====================================================================
                'STOK KELUAR TUJUAN
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

                'STOK KELUAR KEMBALI
                If Len(updStokOut) > 0 Then
                    sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokOutKembali & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
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

            End If

            'update status utama
            sql = "UPDATE M5_Dr SET Drstatus = " & nilaiStatus & ", Drmodifikasiuser='" & userid & "', Drmodifikasitgl = NOW(), Drposting = 0, Drpostingtgl = '1971-01-01 00:00:00', Drjmlrevisi = Drjmlrevisi + 1 WHERE Drid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_DrSearch(PostWsSearch(paramSplit(0), "M5_DrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M5_DrDelete(ByVal param As String) As String

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
            Dim sumber As String = "DR", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Drid, Drnotransaksi FROM M5_Dr WHERE Drid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT drcabang, drlokasi, drsumber, drautonotransaksi, drnotransaksi, drtgl"
            sql &= " FROM M5_dr"
            sql &= " WHERE drid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("drcabang")
                lokasi = dtNomorNext.Rows(0)("drlokasi")
                sumber = dtNomorNext.Rows(0)("drsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("drautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("drnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("drtgl"))
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
            sql = "DELETE FROM M5_Dr_Detail WHERE iddr='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE UTAMA
            sql = "DELETE FROM M5_Dr WHERE drid='" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_DrSearch(PostWsSearch(paramSplit(0), "M5_DrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M5_DrGetdataById(ByVal param As String) As String
        'M5_DrGetdataById Utama --------------------------------------------------------
        'drid, drcabang, drlokasi, drgudang, drasalbarang, drasalbarangkategori, drjenispenjualan, 
        'drjenispenjualankategori, drcarabayar, drsumber, drautonotransaksi, drnotransaksi, drtgl, drkodepa, 
        'drcustomer, drcustomerkontak, dr1alamat1, dr1alamat2, dr1alamat3, dr2alamat1, dr2alamat2, 
        'dr2alamat3, drbagianpenjualan, drbagianpengiriman, drekspedisi, drtglkirim, drtermin, drtgljatuhtempo, 
        'druraian, drcatatan, drnoref, drtglnoref, drtglpenutupan, drmatauang, drkurs, 
        'drhargatermasukpajak, drtotal, drdiskonpersen, drjmldiskon, drtotalpajak1detail, drtotalpajak2detail, drbiayalainpersen, 
        'drbiayalain, drtotaltransaksi, drrekdiskon, drrekpajak1, drrekpajak2, drrekbiayalain, dridsq, 
        'dridso, dridpi, dridpl, driddo, drstatussi, drstatusrnr, drstatussr, 
        'drstatusrealisasi, drstatus, drstatussebelumnya, drjmlrevisi, drcetakanke, drinputuser, drinputtgl, 
        'drmodifikasiuser, drmodifikasitgl, drposting, drpostingtgl, drtutupperiode, drisclose, drcustomtext1, 
        'drcustomtext2, drcustomtext3, drcustomtext4, drcustomtext5, drcustomint1, drcustomint2, drcustomint3, 
        'drcustomdbl1, drcustomdbl2, drcustomdbl3, drcustomdate1, drcustomdate2, drcustomdate3, drcabangnama, 
        'drlokasinama, drgudangnama, drcustomerkode, drcustomernama, drbagianpenjualankode, drbagianpenjualannama, drbagianpengirimankode, 
        'drbagianpengirimannama, drekspedisinama, drterminnama, drterminharijatuhtempo, drrekdiskonnama, drrekpajak1nama, drrekpajak2nama, 
        'drrekbiayalainnama, drnotransaksisq, drnotransaksiso, drnotransaksipi, drnotransaksipl, drnotransaksido, drstatusnama, 
        'drstatussebelumnyanama, drinputusernama, drmodifikasiusernama, ktingkatjual, kpkp

        'M5_DrGetdataById Detail --------------------------------------------------------
        'iddrdetail, iddr, idbarang, namabarang, 
        'tipebarang, jml, jmlkembali, satuan, nilaisatuan, jmlbarang, jmlbarangkembali, 
        'satuanbarang, matauang, kurs, idhppkhususmasuk, idhppfifomasuk, harga, hpp, 
        'diskon, jmldiskon, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, 
        'gudangkembali, rekpersediaan, rekhargapokok, rekdiskonpenjualan, pajak1, jmlpajak1, pajak2, 
        'jmlpajak2, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idsqdetail, idsodetail, idpidetail, idpldetail, iddodetail, jmlsi, statussi, 
        'jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, 
        'pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangasalnama, 
        'gudangtransitnama, gudangtujuannama, gudangkembalinama, costcenternama, divisinama, subdivisinama, proyeknama, 
        'sonotransaksi, pinotransaksi, plnotransaksi, donotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan

        'M5_DrGetdataById Batch --------------------------------------------------------
        'nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang

        'M5_DrGetdataById Serial --------------------------------------------------------
        'nstid, nstjenismutasi, nstidserialin, nstgudang,  nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, 
        'nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, 
        'nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang

        'M5_DrGetdataById Asset --------------------------------------------------------
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
        Dim sumber As String = "DR", asset As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M5_dr~M5_dr_Detail-" & idtransaksi

        'Redrace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi redrace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "drid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "drid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_dr_getdata")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("drid"), 0), sptField,
                     FxDB(drutama("drcabang"), ""), sptField,
                     FxDB(drutama("drlokasi"), ""), sptField,
                     FxDB(drutama("drgudang"), ""), sptField,
                     FxDB(drutama("drasalbarang"), ""), sptField,
                     FxDB(drutama("drasalbarangkategori"), 0), sptField,
                     FxDB(drutama("drjenispenjualan"), ""), sptField,
                     FxDB(drutama("drjenispenjualankategori"), 0), sptField,
                     FxDB(drutama("drcarabayar"), 0), sptField,
                     FxDB(drutama("drsumber"), ""), sptField,
                     FxDB(drutama("drautonotransaksi"), 0), sptField,
                     FxDB(drutama("drnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("drtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("drkodepa"), 0), sptField,
                     FxDB(drutama("drcustomer"), 0), sptField,
                     FxDB(drutama("drcustomerkontak"), ""), sptField,
                     FxDB(drutama("dr1alamat1"), ""), sptField,
                     FxDB(drutama("dr1alamat2"), ""), sptField,
                     FxDB(drutama("dr1alamat3"), ""), sptField,
                     FxDB(drutama("dr2alamat1"), ""), sptField,
                     FxDB(drutama("dr2alamat2"), ""), sptField,
                     FxDB(drutama("dr2alamat3"), ""), sptField,
                     FxDB(drutama("drbagianpenjualan"), 0), sptField,
                     FxDB(drutama("drbagianpengiriman"), 0), sptField,
                     FxDB(drutama("drekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("drtglkirim"), ""), formatTgl), sptField,
                     FxDB(drutama("drtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("drtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("druraian"), ""), sptField,
                     FxDB(drutama("drcatatan"), ""), sptField,
                     FxDB(drutama("drnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("drtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("drtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("drmatauang"), ""), sptField,
                     FxDB(drutama("drkurs"), 0), sptField,
                     FxDB(drutama("drhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("drtotal"), 0), sptField,
                     FxDB(drutama("drdiskonpersen"), ""), sptField,
                     FxDB(drutama("drjmldiskon"), 0), sptField,
                     FxDB(drutama("drtotalpajak1detail"), 0), sptField,
                     FxDB(drutama("drtotalpajak2detail"), 0), sptField,
                     FxDB(drutama("drbiayalainpersen"), 0), sptField,
                     FxDB(drutama("drbiayalain"), 0), sptField,
                     FxDB(drutama("drtotaltransaksi"), 0), sptField,
                     FxDB(drutama("drrekdiskon"), ""), sptField,
                     FxDB(drutama("drrekpajak1"), ""), sptField,
                     FxDB(drutama("drrekpajak2"), ""), sptField,
                     FxDB(drutama("drrekbiayalain"), ""), sptField,
                     FxDB(drutama("dridsq"), 0), sptField,
                     FxDB(drutama("dridso"), 0), sptField,
                     FxDB(drutama("dridpi"), 0), sptField,
                     FxDB(drutama("dridpl"), 0), sptField,
                     FxDB(drutama("driddo"), 0), sptField,
                     FxDB(drutama("drstatussi"), 0), sptField,
                     FxDB(drutama("drstatusrnr"), 0), sptField,
                     FxDB(drutama("drstatussr"), 0), sptField,
                     FxDB(drutama("drstatusrealisasi"), 0), sptField,
                     FxDB(drutama("drstatus"), 0), sptField,
                     FxDB(drutama("drstatussebelumnya"), 0), sptField,
                     FxDB(drutama("drjmlrevisi"), 0), sptField,
                     FxDB(drutama("drcetakanke"), 0), sptField,
                     FxDB(drutama("drinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("drinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("drmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("drmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("drposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("drpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("drtutupperiode"), 0), sptField,
                     FxDB(drutama("drisclose"), 0), sptField,
                     FxDB(drutama("drcustomtext1"), ""), sptField,
                     FxDB(drutama("drcustomtext2"), ""), sptField,
                     FxDB(drutama("drcustomtext3"), ""), sptField,
                     FxDB(drutama("drcustomtext4"), ""), sptField,
                     FxDB(drutama("drcustomtext5"), ""), sptField,
                     FxDB(drutama("drcustomint1"), 0), sptField,
                     FxDB(drutama("drcustomint2"), 0), sptField,
                     FxDB(drutama("drcustomint3"), 0), sptField,
                     FxDB(drutama("drcustomdbl1"), 0), sptField,
                     FxDB(drutama("drcustomdbl2"), 0), sptField,
                     FxDB(drutama("drcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("drcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("drcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("drcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("drcabangnama"), ""), sptField,
                     FxDB(drutama("drlokasinama"), ""), sptField,
                     FxDB(drutama("drgudangnama"), ""), sptField,
                     FxDB(drutama("drcustomerkode"), ""), sptField,
                     FxDB(drutama("drcustomernama"), ""), sptField,
                     FxDB(drutama("drbagianpenjualankode"), ""), sptField,
                     FxDB(drutama("drbagianpenjualannama"), ""), sptField,
                     FxDB(drutama("drbagianpengirimankode"), ""), sptField,
                     FxDB(drutama("drbagianpengirimannama"), ""), sptField,
                     FxDB(drutama("drekspedisinama"), ""), sptField,
                     FxDB(drutama("drterminnama"), ""), sptField,
                     FxDB(drutama("drterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("drrekdiskonnama"), ""), sptField,
                     FxDB(drutama("drrekpajak1nama"), ""), sptField,
                     FxDB(drutama("drrekpajak2nama"), ""), sptField,
                     FxDB(drutama("drrekbiayalainnama"), ""), sptField,
                     FxDB(drutama("drnotransaksisq"), ""), sptField,
                     FxDB(drutama("drnotransaksiso"), ""), sptField,
                     FxDB(drutama("drnotransaksipi"), ""), sptField,
                     FxDB(drutama("drnotransaksipl"), ""), sptField,
                     FxDB(drutama("drnotransaksido"), ""), sptField,
                     FxDB(drutama("drstatusnama"), ""), sptField,
                     FxDB(drutama("drstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("drinputusernama"), ""), sptField,
                     FxDB(drutama("drmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("ktingkatjual"), 0), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("iddrdetail"), 0), sptField,
                     FxDB(dr("iddr"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("jmlkembali"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("jmlbarangkembali"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("idhppkhususmasuk"), 0), sptField,
                     FxDB(dr("idhppfifomasuk"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("diskon"), ""), sptField,
                     FxDB(dr("jmldiskon"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudangasal"), ""), sptField,
                     FxDB(dr("gudangtransit"), ""), sptField,
                     FxDB(dr("gudangtujuan"), ""), sptField,
                     FxDB(dr("gudangkembali"), ""), sptField,
                     FxDB(dr("rekpersediaan"), ""), sptField,
                     FxDB(dr("rekhargapokok"), ""), sptField,
                     FxDB(dr("rekdiskonpenjualan"), ""), sptField,
                     FxDB(dr("pajak1"), ""), sptField,
                     FxDB(dr("jmlpajak1"), 0), sptField,
                     FxDB(dr("pajak2"), ""), sptField,
                     FxDB(dr("jmlpajak2"), 0), sptField,
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
                     FxDB(dr("iddodetail"), 0), sptField,
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
                     FxDB(dr("gudangkembalinama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("pinotransaksi"), ""), sptField,
                     FxDB(dr("plnotransaksi"), ""), sptField,
                     FxDB(dr("donotransaksi"), ""), sptField,
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
            sql = "select `nst`.`nstid` AS `nstid`,`nst`.`nstjenismutasi` AS `nstjenismutasi`,`nst`.`nstidserialin` AS `nstidserialin`,`nst`.`nstgudang` AS `nstgudang`,`nst`.`nstidbarang` AS `nstidbarang`,`nst`.`nstkode` AS `nstkode`,`nst`.`nstsumber` AS `nstsumber`,`nst`.`nstidtransaksi` AS `nstidtransaksi`,`nst`.`nstsatuan` AS `nstsatuan`,`nst`.`nstjml` AS `nstjml`,`nst`.`nstcustomtext1` AS `nstcustomtext1`,`nst`.`nstcustomtext2` AS `nstcustomtext2`,`nst`.`nstcustomtext3` AS `nstcustomtext3`,`nst`.`nstcustomdbl1` AS `nstcustomdbl1`,`nst`.`nstcustomdbl2` AS `nstcustomdbl2`,`nst`.`nstcustomdbl3` AS `nstcustomdbl3`,`nst`.`nstcustomdate1` AS `nstcustomdate1`,`nst`.`nstcustomdate2` AS `nstcustomdate2`,`nst`.`nstcustomdate3` AS `nstcustomdate3`,`i`.`bkode` AS `kodebarang`, nsi.nsinotransaksi from ((`m1_no_serial_transaction` `nst` join `m1_item` `i` on((`nst`.`nstidbarang` = `i`.`bid`))) left join `m1_no_serial_in` `nsi` on((`nst`.`nstidserialin` = `nsi`.`nsiidserialin`)))"
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
            If serial.Length > 0 Then serial = serial.Substring(0, serial.Length - sptRow.Length) Else serial = serial


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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("drid, drcabang, drlokasi, drgudang, drasalbarang, drasalbarangkategori, drjenispenjualan, drjenispenjualankategori, drcarabayar, drsumber, drautonotransaksi, drnotransaksi, drtgl, drkodepa, drcustomer, drcustomerkontak, dr1alamat1, dr1alamat2, dr1alamat3, dr2alamat1, dr2alamat2, dr2alamat3, drbagianpenjualan, drbagianpengiriman, drekspedisi, drtglkirim, drtermin, drtgljatuhtempo, druraian, drcatatan, drnoref, drtglnoref, drtglpenutupan, drmatauang, drkurs, drhargatermasukpajak, drtotal, drdiskonpersen, drjmldiskon, drtotalpajak1detail, drtotalpajak2detail, drbiayalainpersen, drbiayalain, drtotaltransaksi, drrekdiskon, drrekpajak1, drrekpajak2, drrekbiayalain, dridsq, dridso, dridpi, dridpl, driddo, drstatussi, drstatusrnr, drstatussr, drstatusrealisasi, drstatus, drstatussebelumnya, drjmlrevisi, drcetakanke, drinputuser, drinputtgl, drmodifikasiuser, drmodifikasitgl, drposting, drpostingtgl, drtutupperiode, drisclose, drcustomtext1, drcustomtext2, drcustomtext3, drcustomtext4, drcustomtext5, drcustomint1, drcustomint2, drcustomint3, drcustomdbl1, drcustomdbl2, drcustomdbl3, drcustomdate1, drcustomdate2, drcustomdate3, drcabangnama, drlokasinama, drgudangnama, drcustomerkode, drcustomernama, drbagianpenjualankode, drbagianpenjualannama, drbagianpengirimankode, drbagianpengirimannama, drekspedisinama, drterminnama, drterminharijatuhtempo, drrekdiskonnama, drrekpajak1nama, drrekpajak2nama, drrekbiayalainnama, drnotransaksisq, drnotransaksiso, drnotransaksipi, drnotransaksipl, drnotransaksido, drstatusnama, drstatussebelumnyanama, drinputusernama, drmodifikasiusernama, ktingkatjual, kpkp" &
                                                                    sptSubParam & "iddrdetail, iddr, idbarang, namabarang, tipebarang, jml, jmlkembali, satuan, nilaisatuan, jmlbarang, jmlbarangkembali, satuanbarang, matauang, kurs, idhppkhususmasuk, idhppfifomasuk, harga, hpp, diskon, jmldiskon, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, gudangkembali, rekpersediaan, rekhargapokok, rekdiskonpenjualan, pajak1, jmlpajak1, pajak2, jmlpajak2, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpidetail, idpldetail, iddodetail, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, basset, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangasalnama, gudangtransitnama, gudangtujuannama, gudangkembalinama, costcenternama, divisinama, subdivisinama, proyeknama, sonotransaksi, pinotransaksi, plnotransaksi, donotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan" &
                                                                    sptSubParam & "nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang, nbtnotransaksi" &
                                                                    sptSubParam & "nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang, nstnotransaksi" &
                                                                    sptSubParam & "atid, atasetid, atjenismutasi, atsumber, atidutama, atidbarang, atkode, atnama, atkategori, atcabang, atlokasi, atgudang, atdivisi, atsubdivisi, atcostcenter, atproyek, atcatatan, atnomor, attglbeli, attglpakai, atjml, atsatuan, atmatauang, atkurs, atharga, atdiskon, atjmldiskon, atpajak1, atjmlpajak1, atpajak2, atjmlpajak2, athargabeli, atnilairesidu, atumurekonomis, atbebanperbln, atakumulasibeban, atnilaibuku, atnilaipenyusutan, atmetode, attabelpenyusutan, atintangible, atfiskal, atatastengahbulan, atrekasset, atrekakumdepresiasi, atrekdepresiasi, atrekpenghapusan, atprodusen, attglpensiun, atpenyusutanke, atnilaimenurun, atdispose, atpembelian, atpenjualan, atlocked, atstatus, atstatussebelumnya, atisclose, atinputuser, atinputtgl, atmodifikasiuser, atmodifikasitgl, atcustomtext1, atcustomtext2, atcustomtext3, atcustomtext4, atcustomtext5, atcustomint1, atcustomint2, atcustomint3, atcustomint4, atcustomint5, atcustomdbl1, atcustomdbl2, atcustomdbl3, atcustomdbl4, atcustomdbl5, atcustomdate1, atcustomdate2, atcustomdate3, atcustomdate4, atcustomdate5, atkategorinama, atcabangnama, atlokasinama, atgudangnama, atdivisinama, atsubdivisinama, atcostcenternama, atproyeknama, atmetodenama, atpajak1nama, atpajak1nilai, atpajak2nama, atpajak2nilai, atrekassetnama, atrekakumdepresiasinama, atrekdepresiasinama, atrekpenghapusannama, atprodusenkode, atprodusennama, atstatusnama, atstatussebelumnyanama, atinputusernama, atmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_DrSearch(ByVal param As String) As String
        'M5_DrSearch --------------------------------------------------------
        'drid, drcabang, drlokasi, drgudang, drasalbarang, drasalbarangkategori, drjenispenjualan, 
        'drjenispenjualankategori, drcarabayar, drsumber, drautonotransaksi, drnotransaksi, drtgl, drkodepa, 
        'drcustomer, drcustomerkontak, dr1alamat1, dr1alamat2, dr1alamat3, dr2alamat1, dr2alamat2, 
        'dr2alamat3, drbagianpenjualan, drbagianpengiriman, drekspedisi, drtglkirim, drtermin, drtgljatuhtempo, 
        'druraian, drcatatan, drnoref, drtglnoref, drtglpenutupan, drmatauang, drkurs, 
        'drhargatermasukpajak, drtotal, drdiskonpersen, drjmldiskon, drtotalpajak1detail, drtotalpajak2detail, drbiayalainpersen, 
        'drbiayalain, drtotaltransaksi, drrekdiskon, drrekpajak1, drrekpajak2, drrekbiayalain, dridsq, 
        'dridso, dridpi, dridpl, driddo, drstatussi, drstatusrnr, drstatussr, 
        'drstatusrealisasi, drstatus, drstatussebelumnya, drjmlrevisi, drcetakanke, drinputuser, drinputtgl, 
        'drmodifikasiuser, drmodifikasitgl, drposting, drpostingtgl, drtutupperiode, drisclose, drcabangnama, 
        'drlokasinama, drgudangnama, drcustomerkode, drcustomernama, drbagianpenjualankode, drbagianpenjualannama, drekspedisinama, 
        'sqnotransaksi, sonotransaksi, plnotransaksi, donotransaksi, drstatusnama, drstatussebelumnyanama, drinputusernama, 
        'drmodifikasiusernama

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
        sql = query.PanggilQuery("m5_dr_v")

        dt = AmbilData("aplikasi1-M5_dr_V", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("drid"), 0), sptField,
                     FxDB(dr("drcabang"), ""), sptField,
                     FxDB(dr("drlokasi"), ""), sptField,
                     FxDB(dr("drgudang"), ""), sptField,
                     FxDB(dr("drasalbarang"), ""), sptField,
                     FxDB(dr("drasalbarangkategori"), 0), sptField,
                     FxDB(dr("drjenispenjualan"), ""), sptField,
                     FxDB(dr("drjenispenjualankategori"), 0), sptField,
                     FxDB(dr("drcarabayar"), 0), sptField,
                     FxDB(dr("drsumber"), ""), sptField,
                     FxDB(dr("drautonotransaksi"), 0), sptField,
                     FxDB(dr("drnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("drtgl"), ""), formatTgl), sptField,
                     FxDB(dr("drkodepa"), 0), sptField,
                     FxDB(dr("drcustomer"), 0), sptField,
                     FxDB(dr("drcustomerkontak"), ""), sptField,
                     FxDB(dr("dr1alamat1"), ""), sptField,
                     FxDB(dr("dr1alamat2"), ""), sptField,
                     FxDB(dr("dr1alamat3"), ""), sptField,
                     FxDB(dr("dr2alamat1"), ""), sptField,
                     FxDB(dr("dr2alamat2"), ""), sptField,
                     FxDB(dr("dr2alamat3"), ""), sptField,
                     FxDB(dr("drbagianpenjualan"), 0), sptField,
                     FxDB(dr("drbagianpengiriman"), 0), sptField,
                     FxDB(dr("drekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("drtglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("drtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("drtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("druraian"), ""), sptField,
                     FxDB(dr("drcatatan"), ""), sptField,
                     FxDB(dr("drnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("drtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("drtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("drmatauang"), ""), sptField,
                     FxDB(dr("drkurs"), 0), sptField,
                     FxDB(dr("drhargatermasukpajak"), 0), sptField,
                     FxDB(dr("drtotal"), 0), sptField,
                     FxDB(dr("drdiskonpersen"), ""), sptField,
                     FxDB(dr("drjmldiskon"), 0), sptField,
                     FxDB(dr("drtotalpajak1detail"), 0), sptField,
                     FxDB(dr("drtotalpajak2detail"), 0), sptField,
                     FxDB(dr("drbiayalainpersen"), 0), sptField,
                     FxDB(dr("drbiayalain"), 0), sptField,
                     FxDB(dr("drtotaltransaksi"), 0), sptField,
                     FxDB(dr("drrekdiskon"), ""), sptField,
                     FxDB(dr("drrekpajak1"), ""), sptField,
                     FxDB(dr("drrekpajak2"), ""), sptField,
                     FxDB(dr("drrekbiayalain"), ""), sptField,
                     FxDB(dr("dridsq"), 0), sptField,
                     FxDB(dr("dridso"), 0), sptField,
                     FxDB(dr("dridpi"), 0), sptField,
                     FxDB(dr("dridpl"), 0), sptField,
                     FxDB(dr("driddo"), 0), sptField,
                     FxDB(dr("drstatussi"), 0), sptField,
                     FxDB(dr("drstatusrnr"), 0), sptField,
                     FxDB(dr("drstatussr"), 0), sptField,
                     FxDB(dr("drstatusrealisasi"), 0), sptField,
                     FxDB(dr("drstatus"), 0), sptField,
                     FxDB(dr("drstatussebelumnya"), 0), sptField,
                     FxDB(dr("drjmlrevisi"), 0), sptField,
                     FxDB(dr("drcetakanke"), 0), sptField,
                     FxDB(dr("drinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("drinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("drmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("drmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("drposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("drpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("drtutupperiode"), 0), sptField,
                     FxDB(dr("drisclose"), 0), sptField,
                     FxDB(dr("drcabangnama"), ""), sptField,
                     FxDB(dr("drlokasinama"), ""), sptField,
                     FxDB(dr("drgudangnama"), ""), sptField,
                     FxDB(dr("drcustomerkode"), ""), sptField,
                     FxDB(dr("drcustomernama"), ""), sptField,
                     FxDB(dr("drbagianpenjualankode"), ""), sptField,
                     FxDB(dr("drbagianpenjualannama"), ""), sptField,
                     FxDB(dr("drekspedisinama"), ""), sptField,
                     FxDB(dr("sqnotransaksi"), ""), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("plnotransaksi"), ""), sptField,
                     FxDB(dr("donotransaksi"), ""), sptField,
                     FxDB(dr("drstatusnama"), ""), sptField,
                     FxDB(dr("drstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("drinputusernama"), ""), sptField,
                     FxDB(dr("drmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("drid, drcabang, drlokasi, drgudang, drasalbarang, drasalbarangkategori, drjenispenjualan, drjenispenjualankategori, drcarabayar, drsumber, drautonotransaksi, drnotransaksi, drtgl, drkodepa, drcustomer, drcustomerkontak, dr1alamat1, dr1alamat2, dr1alamat3, dr2alamat1, dr2alamat2, dr2alamat3, drbagianpenjualan, drbagianpengiriman, drekspedisi, drtglkirim, drtermin, drtgljatuhtempo, druraian, drcatatan, drnoref, drtglnoref, drtglpenutupan, drmatauang, drkurs, drhargatermasukpajak, drtotal, drdiskonpersen, drjmldiskon, drtotalpajak1detail, drtotalpajak2detail, drbiayalainpersen, drbiayalain, drtotaltransaksi, drrekdiskon, drrekpajak1, drrekpajak2, drrekbiayalain, dridsq, dridso, dridpi, dridpl, driddo, drstatussi, drstatusrnr, drstatussr, drstatusrealisasi, drstatus, drstatussebelumnya, drjmlrevisi, drcetakanke, drinputuser, drinputtgl, drmodifikasiuser, drmodifikasitgl, drposting, drpostingtgl, drtutupperiode, drisclose, drcabangnama, drlokasinama, drgudangnama, drcustomerkode, drcustomernama, drbagianpenjualankode, drbagianpenjualannama, drekspedisinama, sqnotransaksi, sonotransaksi, plnotransaksi, donotransaksi, drstatusnama, drstatussebelumnyanama, drinputusernama, drmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_Dr_Detail_VSearch(ByVal param As String) As String
        'M5_Dr_Detail_VSearch --------------------------------------------------------
        'iddrdetail, iddr, idbarang, namabarang, tipebarang, jml, jmlkembali, 
        'satuan, nilaisatuan, jmlbarang, jmlbarangkembali, satuanbarang, matauang, kurs, 
        'idhppkhususmasuk, idhppfifomasuk, harga, hpp, diskon, jmldiskon, cabang, 
        'lokasi, gudangasal, gudangtransit, gudangtujuan, gudangkembali, rekpersediaan, rekhargapokok, 
        'rekdiskonpenjualan, pajak1, jmlpajak1, pajak2, jmlpajak2, costcenter, divisi, 
        'subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpidetail, 
        'idpldetail, iddodetail, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, 
        'statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, drnotransaksi, 
        'druraian, drcatatan, drnoref, drtglnoref, drtglkirim, drcustomerkontak, dr1alamat1, 
        'dr1alamat2, dr1alamat3, dr2alamat1, dr2alamat2, dr2alamat3, drbagianpenjualan, drbagianpenjualankode, 
        'drbagianpenjualannama, drbagianpengirimankode, drbagianpengirimannama, drekspedisi, drekspedisinama, drtermin, drterminnama, 
        'drterminharijatuhtempo, kodebarang, bhpp, bhppaverage, bhargajual1, bjenis, brekpenjualan, 
        'bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisasi, 
        'jmlsisarealisasi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan, basset,
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

        Dim drl As String = ""

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
        'drl = query.PanggilQuery("m5_dr_detail_v")
        drl = "select `drd`.`iddrdetail` AS `iddrdetail`,`drd`.`iddr` AS `iddr`,`drd`.`idbarang` AS `idbarang`,`drd`.`namabarang` AS `namabarang`,`drd`.`tipebarang` AS `tipebarang`,`drd`.`jml` AS `jml`,`drd`.`jmlkembali` AS `jmlkembali`,`drd`.`satuan` AS `satuan`,`drd`.`nilaisatuan` AS `nilaisatuan`,`drd`.`jmlbarang` AS `jmlbarang`,`drd`.`jmlbarangkembali` AS `jmlbarangkembali`,`drd`.`satuanbarang` AS `satuanbarang`,`drd`.`matauang` AS `matauang`,`drd`.`kurs` AS `kurs`,`drd`.`idhppkhususmasuk` AS `idhppkhususmasuk`,`drd`.`idhppfifomasuk` AS `idhppfifomasuk`,`drd`.`harga` AS `harga`,`drd`.`hpp` AS `hpp`,`drd`.`diskon` AS `diskon`,`drd`.`jmldiskon` AS `jmldiskon`,`drd`.`cabang` AS `cabang`,`drd`.`lokasi` AS `lokasi`,`drd`.`gudangasal` AS `gudangasal`,`drd`.`gudangtransit` AS `gudangtransit`,`drd`.`gudangtujuan` AS `gudangtujuan`,`drd`.`gudangkembali` AS `gudangkembali`,`i`.`brekpersediaan` AS `rekpersediaan`,`drd`.`rekhargapokok` AS `rekhargapokok`,`drd`.`rekdiskonpenjualan` AS `rekdiskonpenjualan`,`drd`.`pajak1` AS `pajak1`,`drd`.`jmlpajak1` AS `jmlpajak1`,`drd`.`pajak2` AS `pajak2`,`drd`.`jmlpajak2` AS `jmlpajak2`,`drd`.`costcenter` AS `costcenter`,`drd`.`divisi` AS `divisi`,`drd`.`subdivisi` AS `subdivisi`,`drd`.`proyek` AS `proyek`,`drd`.`catatan` AS `catatan`,`drd`.`urutan` AS `urutan`,`drd`.`idsqdetail` AS `idsqdetail`,`drd`.`idsodetail` AS `idsodetail`,`drd`.`idpidetail` AS `idpidetail`,`drd`.`idpldetail` AS `idpldetail`,`drd`.`iddodetail` AS `iddodetail`,`drd`.`jmlsi` AS `jmlsi`,`drd`.`statussi` AS `statussi`,`drd`.`jmlrnr` AS `jmlrnr`,`drd`.`statusrnr` AS `statusrnr`,`drd`.`jmlsr` AS `jmlsr`,`drd`.`statussr` AS `statussr`,`drd`.`jmlrealisasi` AS `jmlrealisasi`,`drd`.`statusrealisasi` AS `statusrealisasi`,`drd`.`isclose` AS `isclose`,`drd`.`customtext1` AS `customtext1`,`drd`.`customtext2` AS `customtext2`,`drd`.`customtext3` AS `customtext3`,`drd`.`customdbl1` AS `customdbl1`,`drd`.`customdbl2` AS `customdbl2`,`drd`.`customdbl3` AS `customdbl3`,`drd`.`customdate1` AS `customdate1`,`drd`.`customdate2` AS `customdate2`,`drd`.`customdate3` AS `customdate3`,`dr`.`drnotransaksi` AS `drnotransaksi`,`dr`.`druraian` AS `druraian`,`dr`.`drcatatan` AS `drcatatan`,`dr`.`drnoref` AS `drnoref`,`dr`.`drtglnoref` AS `drtglnoref`,`dr`.`drtglkirim` AS `drtglkirim`,`dr`.`drcustomerkontak` AS `drcustomerkontak`,`dr`.`dr1alamat1` AS `dr1alamat1`,`dr`.`dr1alamat2` AS `dr1alamat2`,`dr`.`dr1alamat3` AS `dr1alamat3`,`dr`.`dr2alamat1` AS `dr2alamat1`,`dr`.`dr2alamat2` AS `dr2alamat2`,`dr`.`dr2alamat3` AS `dr2alamat3`,`dr`.`drbagianpenjualan` AS `drbagianpenjualan`,`c1`.`kkode` AS `drbagianpenjualankode`,`c1`.`knama` AS `drbagianpenjualannama`,`c2`.`kkode` AS `drbagianpengirimankode`,`c2`.`knama` AS `drbagianpengirimannama`,`dr`.`drekspedisi` AS `drekspedisi`,`e`.`enama` AS `drekspedisinama`,`dr`.`drtermin` AS `drtermin`,`tr`.`trnama` AS `drterminnama`,`tr`.`trharijatuhtempo` AS `drterminharijatuhtempo`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bhppaverage` AS `bhppaverage`,`i`.`bhargajual1` AS `bhargajual1`,`i`.`bjenis` AS `bjenis`,`i`.`brekpenjualan` AS `brekpenjualan`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,((`drd`.`jmlbarang` - `drd`.`jmlsi`) / `drd`.`nilaisatuan`) AS `jmlsisasi`,((`drd`.`jmlbarang` - `drd`.`jmlrealisasi`) / `drd`.`nilaisatuan`) AS `jmlsisarealisasi`, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan, i.basset, t1.takunbeli as pajak1akunbeli, t1c1.cnama as pajak1akunbelinama, t1.takunjual as pajak1akunjual, t1c2.cnama as pajak1akunjualnama, t2.takunbeli as pajak2akunbeli, t2c1.cnama as pajak2akunbelinama, t2.takunjual as pajak2akunjual, t2c2.cnama as pajak2akunjualnama, cc.ccnama AS costcenternama, p.pnama AS proyeknama from `m5_dr_detail` `drd` left join `m5_dr` `dr` on `drd`.`iddr` = `dr`.`drid` left join `m1_expedition` `e` on `dr`.`drekspedisi` = `e`.`ekode` left join `m1_terms` `tr` on `dr`.`drtermin` = `tr`.`trkode` left join `m1_contact` `c1` on `dr`.`drbagianpenjualan` = `c1`.`kid` left join `m1_contact` `c2` on `dr`.`drbagianpengiriman` = `c2`.`kid` left join `m1_item` `i` on `drd`.`idbarang` = `i`.`bid` left join `m1_tax` `t1` on `drd`.`pajak1` = `t1`.`tkode` left join `m1_tax` `t2` on `drd`.`pajak2` = `t2`.`tkode` left join m1_coa t1c1 on t1.takunbeli = t1c1.cnomor left join m1_coa t1c2 on t1.takunjual = t1c2.cnomor left join m1_coa t2c1 on t2.takunbeli = t2c1.cnomor left join m1_coa t2c2 on t2.takunjual = t2c2.cnomor LEFT JOIN m1_cost_center cc ON cc.cckode = `drd`.`costcenter` LEFT JOIN m1_project p ON p.pkode = drd.proyek"

        dt = AmbilData("aplikasi1-M5_dr_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , drl) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("iddrdetail"), 0), sptField,
                     FxDB(dr("iddr"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("jmlkembali"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("jmlbarangkembali"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("idhppkhususmasuk"), 0), sptField,
                     FxDB(dr("idhppfifomasuk"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("diskon"), ""), sptField,
                     FxDB(dr("jmldiskon"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudangasal"), ""), sptField,
                     FxDB(dr("gudangtransit"), ""), sptField,
                     FxDB(dr("gudangtujuan"), ""), sptField,
                     FxDB(dr("gudangkembali"), ""), sptField,
                     FxDB(dr("rekpersediaan"), ""), sptField,
                     FxDB(dr("rekhargapokok"), ""), sptField,
                     FxDB(dr("rekdiskonpenjualan"), ""), sptField,
                     FxDB(dr("pajak1"), ""), sptField,
                     FxDB(dr("jmlpajak1"), 0), sptField,
                     FxDB(dr("pajak2"), ""), sptField,
                     FxDB(dr("jmlpajak2"), 0), sptField,
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
                     FxDB(dr("iddodetail"), 0), sptField,
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
                     FxDB(dr("drnotransaksi"), ""), sptField,
                     FxDB(dr("druraian"), ""), sptField,
                     FxDB(dr("drcatatan"), ""), sptField,
                     FxDB(dr("drnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("drtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("drtglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("drcustomerkontak"), ""), sptField,
                     FxDB(dr("dr1alamat1"), ""), sptField,
                     FxDB(dr("dr1alamat2"), ""), sptField,
                     FxDB(dr("dr1alamat3"), ""), sptField,
                     FxDB(dr("dr2alamat1"), ""), sptField,
                     FxDB(dr("dr2alamat2"), ""), sptField,
                     FxDB(dr("dr2alamat3"), ""), sptField,
                     FxDB(dr("drbagianpenjualan"), 0), sptField,
                     FxDB(dr("drbagianpenjualankode"), ""), sptField,
                     FxDB(dr("drbagianpenjualannama"), ""), sptField,
                     FxDB(dr("drbagianpengirimankode"), ""), sptField,
                     FxDB(dr("drbagianpengirimannama"), ""), sptField,
                     FxDB(dr("drekspedisi"), ""), sptField,
                     FxDB(dr("drekspedisinama"), ""), sptField,
                     FxDB(dr("drtermin"), ""), sptField,
                     FxDB(dr("drterminnama"), ""), sptField,
                     FxDB(dr("drterminharijatuhtempo"), 0), sptField,
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
                     FxDB(dr("jmlsisasi"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("iddrdetail, iddr, idbarang, namabarang, tipebarang, jml, jmlkembali, satuan, nilaisatuan, jmlbarang, jmlbarangkembali, satuanbarang, matauang, kurs, idhppkhususmasuk, idhppfifomasuk, harga, hpp, diskon, jmldiskon, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, gudangkembali, rekpersediaan, rekhargapokok, rekdiskonpenjualan, pajak1, jmlpajak1, pajak2, jmlpajak2, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpidetail, idpldetail, iddodetail, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, drnotransaksi, druraian, drcatatan, drnoref, drtglnoref, drtglkirim, drcustomerkontak, dr1alamat1, dr1alamat2, dr1alamat3, dr2alamat1, dr2alamat2, dr2alamat3, drbagianpenjualan, drbagianpenjualankode, drbagianpenjualannama, drbagianpengirimankode, drbagianpengirimannama, drekspedisi, drekspedisinama, drtermin, drterminnama, drterminharijatuhtempo, kodebarang, bhpp, bhppaverage, bhargajual1, bjenis, brekpenjualan, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisasi, jmlsisarealisasi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan, basset, pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, pajak2akunjualnama, costcenternama, proyeknama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_DrTerkait(ByVal param As String) As String
        'M5_DrTerkait --------------------------------------------------------
        'drid, drnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
        'modifikasitglterkait, jenisterkait

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isdrev(2), countPage(3), countRow(4)

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
            result(2) = "drid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2) & " AND drid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "drid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.m5_dr_terkait(Filter)


        dt = AmbilData("aplikasi1-m5_dr_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("drid"), 0), sptField,
                     FxDB(dr("drnotransaksi"), ""), sptField,
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
            result(2) = "Related DR data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("drid, drnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

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

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstandingDO As String, ByVal ftOutstandingDO As String, ByVal ftExistStok As String, ByVal ftStok As String, ByVal ftExistBatch As String, ByVal ftBatch As String, ByVal ftExistSerial As String, ByVal ftSerial As String, ByVal gudangBatchSerial As String, ByVal ftDO As String, ByRef termasukPajak As String) As String
        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaiSatuan As Double = 0, sisa As Double = 0
        Dim filterLookup As String = "", urutan As String = "", gudang As String = "", noBatch As String = "", noSerial As String = ""

        'VALIDASI OUTSTANDING ---------------------------------------
        'DO
        If Len(ftExistOutstandingDO) > 0 Then 'ftExistOutstanding = rowExists, iddodetail, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingDO)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "iddodetail=" & dtval.Rows(0)("iddodetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in DO" : GoTo selesai
            End If
        End If

        'CEK DO YANG DIAMBIL
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        If Len(ftDO) > 0 Then
            sql = "SELECT `do`.donotransaksi as notransaksi, `do`.dohargatermasukpajak as termasukpajak, (CASE `do`.dohargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajaknama FROM m5_do_detail dod JOIN m5_do `do` ON dod.iddo = `do`.doid WHERE " & ftDO & " GROUP BY `do`.dohargatermasukpajak"
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
                sql = "SELECT i.bkode, dod.iddodetail, `do`.donotransaksi as notransaksi, (CASE `do`.dohargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m5_do_detail dod JOIN m5_do `do` ON dod.iddo = `do`.doid JOIN m1_item i ON dod.idbarang = i.bid WHERE (" & ftDO & ") AND `do`.dohargatermasukpajak <> " & termasukPajak & " ORDER BY dod.urutan"
                dtval = AsDataTableAmbilDariDB(sql)
                If dtval.Rows.Count > 0 Then
                    'Ambil informasi utk errmessage
                    kodebarang = dtval.Rows(0)("bkode")

                    filterLookup = "iddodetail = " & dtval.Rows(0)("iddodetail")
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
        If Len(ftOutstandingDO) > 0 Then
            sql = "SELECT dod.iddodetail, (dod.jmlbarang - dod.jmlrealisasi) as sisarealisasi, i.bid, i.bkode FROM m5_do_detail AS dod INNER JOIN m1_item AS i ON dod.idbarang = i.bid WHERE " & ftOutstandingDO
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisarealisasi")

                filterLookup = "iddodetail=" & dtval.Rows(0)("iddodetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in DO, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
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
                'sql = "SELECT isw.idbarang, isw.kgudang, isw.stok, i.bkode FROM m1_item_stock_warehouse isw JOIN m1_item i ON isw.idbarang = i.bid AND i.bjenis <> 'J' WHERE " & ftStok
                'sql = "SELECT isw.idbarang, isw.kgudang, isw.stok - IFNULL(isb.jmlbooking,0) as stok, i.bkode FROM m1_item_stock_warehouse isw JOIN m1_item i ON isw.idbarang = i.bid AND i.bjenis <> 'J' LEFT JOIN m1_item_booking isb ON isw.idbarang = isb.idbarang WHERE " & ftStok
                sql = "SELECT isw.idbarang, isw.kgudang, isw.stok - IFNULL(isb.jmlbooking,0) as stok, i.bkode FROM m1_item_stock_warehouse isw JOIN m1_item i ON isw.idbarang = i.bid AND i.bjenis <> 'J' LEFT JOIN m1_warehouse w ON isw.kgudang = w.wkode LEFT JOIN m1_item_booking isb ON isw.idbarang = isb.idbarang AND w.wbookingstok = 1 WHERE " & ftStok
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
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " | No. Batch : " & noBatch & " exceeds the number of stock in No. Batch list, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
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
    Public Function M5_DrSimpanOld(ByVal param As String) As String
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
        'drid(0) As Integer, drcabang(1) As String, drlokasi(2) As String, drgudang(3) As String, drasalbarang(4) As String, 
        'drasalbarangkategori(5) As Integer, drjenispenjualan(6) As String, drjenispenjualankategori(7) As Integer, drcarabayar(8) As Integer, drsumber(9) As String, 
        'drautonotransaksi(10) As Integer, drnotransaksi(11) As String, drtgl(12) As Date, drkodepa(13) As Integer, drcustomer(14) As Integer, 
        'drcustomerkontak(15) As String, dr1alamat1(16) As String, dr1alamat2(17) As String, dr1alamat3(18) As String, dr2alamat1(19) As String, 
        'dr2alamat2(20) As String, dr2alamat3(21) As String, drbagianpenjualan(22) As Integer, drbagianpengiriman(23) As Integer, drekspedisi(24) As String, 
        'drtglkirim(25) As Date, drtermin(26) As String, drtgljatuhtempo(27) As Date, druraian(28) As String, drcatatan(29) As String, 
        'drnoref(30) As String, drtglnoref(31) As Date, drtglpenutupan(32) As Date, drmatauang(33) As String, drkurs(34) As Double, 
        'drhargatermasukpajak(35) As Integer, drtotal(36) As Double, drdiskonpersen(37) As String, drjmldiskon(38) As Double, drtotalpajak1detail(39) As Double, 
        'drtotalpajak2detail(40) As Double, drbiayalainpersen(41) As Double, drbiayalain(42) As Double, drtotaltransaksi(43) As Double, drrekdiskon(44) As String, 
        'drrekpajak1(45) As String, drrekpajak2(46) As String, drrekbiayalain(47) As String, dridsq(48) As Integer, dridso(49) As Integer, 
        'dridpi(50) As Integer, dridpl(51) As Integer, driddo(52) As Integer, drstatussi(53) As Integer, drstatusrnr(54) As Integer, 
        'drstatussr(55) As Integer, drstatus(56) As Integer, drstatussebelumnya(57) As Integer, drjmlrevisi(58) As Integer, drcetakanke(59) As Integer, 
        'drinputuser(60) As Integer, drinputtgl(61) As DateTime, drmodifikasiuser(62) As Integer, drmodifikasitgl(63) As DateTime, drposting(64) As Integer, 
        'drtutupperiode(65) As Integer, drisclose(66) As Integer, drcustomtext1(67) As String, drcustomtext2(68) As String, drcustomtext3(69) As String, 
        'drcustomtext4(70) As String, drcustomtext5(71) As String, drcustomint1(72) As Integer, drcustomint2(73) As Integer, drcustomint3(74) As Integer, 
        'drcustomdbl1(75) As Double, drcustomdbl2(76) As Double, drcustomdbl3(77) As Double, drcustomdate1(78) As Date, drcustomdate2(79) As Date, 
        'drcustomdate3(80) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'drid, drcabang, drlokasi, drgudang, drasalbarang, drasalbarangkategori, drjenispenjualan, 
        'drjenispenjualankategori, drcarabayar, drsumber, drautonotransaksi, drnotransaksi, drtgl, drkodepa, 
        'drcustomer, drcustomerkontak, dr1alamat1, dr1alamat2, dr1alamat3, dr2alamat1, dr2alamat2, 
        'dr2alamat3, drbagianpenjualan, drbagianpengiriman, drekspedisi, drtglkirim, drtermin, drtgljatuhtempo, 
        'druraian, drcatatan, drnoref, drtglnoref, drtglpenutupan, drmatauang, drkurs, 
        'drhargatermasukpajak, drtotal, drdiskonpersen, drjmldiskon, drtotalpajak1detail, drtotalpajak2detail, drbiayalainpersen, 
        'drbiayalain, drtotaltransaksi, drrekdiskon, drrekpajak1, drrekpajak2, drrekbiayalain, dridsq, 
        'dridso, dridpi, dridpl, driddo, drstatussi, drstatusrnr, drstatussr, 
        'drstatus, drstatussebelumnya, drjmlrevisi, drcetakanke, drinputuser, drinputtgl, drmodifikasiuser, 
        'drmodifikasitgl, drposting, drtutupperiode, drisclose, drcustomtext1, drcustomtext2, drcustomtext3, 
        'drcustomtext4, drcustomtext5, drcustomint1, drcustomint2, drcustomint3, drcustomdbl1, drcustomdbl2, 
        'drcustomdbl3, drcustomdate1, drcustomdate2, drcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 81) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'drid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "drid required numeric." : GoTo selesai
        End If
        'drasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "drasalbarangkategori required numeric." : GoTo selesai
        End If
        'drjenispenjualankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "drjenispenjualankategori required numeric." : GoTo selesai
        End If
        'drcarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "drcarabayar required numeric." : GoTo selesai
        End If
        'drautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "drautonotransaksi required numeric." : GoTo selesai
        End If
        'drtgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "drtgl required date." : GoTo selesai
        End If
        'drkodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "drkodepa required numeric." : GoTo selesai
        End If
        'drcustomer(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "drcustomer required numeric." : GoTo selesai
        End If
        'drbagianpenjualan(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "drbagianpenjualan required numeric." : GoTo selesai
        End If
        'drbagianpengiriman(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "drbagianpengiriman required numeric." : GoTo selesai
        End If
        'drtglkirim(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "drtglkirim required date." : GoTo selesai
        End If
        'drtgljatuhtempo(27) As Date
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "drtgljatuhtempo required date." : GoTo selesai
        End If
        'drtglnoref(31) As Date
        If (IsDate(dataUtama(31)) = False) Then
            result(2) = "drtglnoref required date." : GoTo selesai
        End If
        'drtglpenutupan(32) As Date
        If (IsDate(dataUtama(32)) = False) Then
            result(2) = "drtglpenutupan required date." : GoTo selesai
        End If
        'drkurs(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "drkurs required numeric." : GoTo selesai
        End If
        'drhargatermasukpajak(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "drhargatermasukpajak required numeric." : GoTo selesai
        End If
        'drtotal(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "drtotal required numeric." : GoTo selesai
        End If
        'drjmldiskon(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "drjmldiskon required numeric." : GoTo selesai
        End If
        'drtotalpajak1detail(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "drtotalpajak1detail required numeric." : GoTo selesai
        End If
        'drtotalpajak2detail(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "drtotalpajak2detail required numeric." : GoTo selesai
        End If
        ''drbiayalainpersen(41) As Double
        'If (IsNumeric(dataUtama(41)) = False) Then
        '    result(2) = "drbiayalainpersen required numeric." : GoTo selesai
        'End If
        'drbiayalain(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "drbiayalain required numeric." : GoTo selesai
        End If
        'drtotaltransaksi(43) As Double
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "drtotaltransaksi required numeric." : GoTo selesai
        End If
        'dridsq(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "dridsq required numeric." : GoTo selesai
        End If
        'dridso(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "dridso required numeric." : GoTo selesai
        End If
        'dridpi(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "dridpi required numeric." : GoTo selesai
        End If
        'dridpl(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "dridpl required numeric." : GoTo selesai
        End If
        'driddo(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "driddo required numeric." : GoTo selesai
        End If
        'drstatussi(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "drstatussi required numeric." : GoTo selesai
        End If
        'drstatusrnr(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "drstatusrnr required numeric." : GoTo selesai
        End If
        'drstatussr(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "drstatussr required numeric." : GoTo selesai
        End If
        'drstatus(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "drstatus required numeric." : GoTo selesai
        End If
        'drstatussebelumnya(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "drstatussebelumnya required numeric." : GoTo selesai
        End If
        'drjmlrevisi(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "drjmlrevisi required numeric." : GoTo selesai
        End If
        'drcetakanke(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "drcetakanke required numeric." : GoTo selesai
        End If
        'drinputuser(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "drinputuser required numeric." : GoTo selesai
        End If
        'drinputtgl(61) As DateTime
        If (IsDate(dataUtama(61)) = False) Then
            result(2) = "drinputtgl required date." : GoTo selesai
        End If
        'drmodifikasiuser(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "drmodifikasiuser required numeric." : GoTo selesai
        End If
        'drmodifikasitgl(63) As DateTime
        If (IsDate(dataUtama(63)) = False) Then
            result(2) = "drmodifikasitgl required date." : GoTo selesai
        End If
        'drposting(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "drposting required numeric." : GoTo selesai
        End If
        'drtutupperiode(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "drtutupperiode required numeric." : GoTo selesai
        End If
        'drisclose(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "drisclose required numeric." : GoTo selesai
        End If
        'drcustomint1(72) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "drcustomint1 required numeric." : GoTo selesai
        End If
        'drcustomint2(73) As Integer
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "drcustomint2 required numeric." : GoTo selesai
        End If
        'drcustomint3(74) As Integer
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "drcustomint3 required numeric." : GoTo selesai
        End If
        'drcustomdbl1(75) As Double
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "drcustomdbl1 required numeric." : GoTo selesai
        End If
        'drcustomdbl2(76) As Double
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "drcustomdbl2 required numeric." : GoTo selesai
        End If
        'drcustomdbl3(77) As Double
        If (IsNumeric(dataUtama(77)) = False) Then
            result(2) = "drcustomdbl3 required numeric." : GoTo selesai
        End If
        'drcustomdate1(78) As Date
        If (IsDate(dataUtama(78)) = False) Then
            result(2) = "drcustomdate1 required date." : GoTo selesai
        End If
        'drcustomdate2(79) As Date
        If (IsDate(dataUtama(79)) = False) Then
            result(2) = "drcustomdate2 required date." : GoTo selesai
        End If
        'drcustomdate3(80) As Date
        If (IsDate(dataUtama(80)) = False) Then
            result(2) = "drcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'drcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "drcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "drcabang should not be more than 25 character." : GoTo selesai
        End If

        'drlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "drlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "drlokasi should not be more than 25 character." : GoTo selesai
        End If

        'drgudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "drgudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "drgudang should not be more than 25 character." : GoTo selesai
        End If

        'drsumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "drsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "drsumber should not be more than 10 character." : GoTo selesai
        End If

        'drnotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "drnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "drnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'drtgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "drtgl can't be empty" : GoTo selesai
        End If

        'drtglkirim(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "drtglkirim can't be empty" : GoTo selesai
        End If

        'drtgljatuhtempo(27) As Date
        If Len(dataUtama(27)) = 0 Then
            result(2) = "drtgljatuhtempo can't be empty" : GoTo selesai
        End If

        'drtglnoref(31) As Date
        If Len(dataUtama(31)) = 0 Then
            result(2) = "drtglnoref can't be empty" : GoTo selesai
        End If

        'drtglpenutupan(32) As Date
        If Len(dataUtama(32)) = 0 Then
            result(2) = "drtglpenutupan can't be empty" : GoTo selesai
        End If

        'drmatauang(33) As String
        If Len(dataUtama(33)) = 0 Then
            result(2) = "drmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(33)) > 25 Then
            result(2) = "drmatauang should not be more than 25 character." : GoTo selesai
        End If

        'drkurs(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "drkurs can't be empty" : GoTo selesai
        End If

        'drtotal(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "drtotal can't be empty" : GoTo selesai
        End If

        'drdiskonpersen(37) As String
        If Len(dataUtama(37)) = 0 Then
            result(2) = "drdiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(37)) > 25 Then
            result(2) = "drdiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'drjmldiskon(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "drjmldiskon can't be empty" : GoTo selesai
        End If

        'drtotalpajak1detail(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "drtotalpajak1detail can't be empty" : GoTo selesai
        End If

        'drtotalpajak2detail(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "drtotalpajak2detail can't be empty" : GoTo selesai
        End If

        'drbiayalainpersen(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "drbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(41)) > 25 Then
            result(2) = "drbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'drbiayalain(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "drbiayalain can't be empty" : GoTo selesai
        End If

        'drtotaltransaksi(43) As Double
        If Len(dataUtama(43)) = 0 Then
            result(2) = "drtotaltransaksi can't be empty" : GoTo selesai
        End If

        'drinputtgl(61) As DateTime
        If Len(dataUtama(61)) = 0 Then
            result(2) = "drinputtgl can't be empty" : GoTo selesai
        End If

        'drmodifikasitgl(63) As DateTime
        If Len(dataUtama(63)) = 0 Then
            result(2) = "drmodifikasitgl can't be empty" : GoTo selesai
        End If

        'drcustomdbl1(75) As Double
        If Len(dataUtama(75)) = 0 Then
            result(2) = "drcustomdbl1 can't be empty" : GoTo selesai
        End If

        'drcustomdbl2(76) As Double
        If Len(dataUtama(76)) = 0 Then
            result(2) = "drcustomdbl2 can't be empty" : GoTo selesai
        End If

        'drcustomdbl3(77) As Double
        If Len(dataUtama(77)) = 0 Then
            result(2) = "drcustomdbl3 can't be empty" : GoTo selesai
        End If

        'drcustomdate1(78) As Date
        If Len(dataUtama(78)) = 0 Then
            result(2) = "drcustomdate1 can't be empty" : GoTo selesai
        End If

        'drcustomdate2(79) As Date
        If Len(dataUtama(79)) = 0 Then
            result(2) = "drcustomdate2 can't be empty" : GoTo selesai
        End If

        'drcustomdate3(80) As Date
        If Len(dataUtama(80)) = 0 Then
            result(2) = "drcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "drid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drjenispenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drjenispenjualankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drcustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drcustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dr1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dr1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dr1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dr2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dr2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dr2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drbagianpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drbagianpengiriman", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drekspedisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drtglkirim", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drtermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drtgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "druraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drtglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drhargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drtotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drdiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drjmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drtotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drtotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drtotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drrekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drrekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drrekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drrekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dridsq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dridso", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dridpi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dridpl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "driddo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drstatussi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drstatusrnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drstatussr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drtutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "drcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "drcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "drid~drcabang~drlokasi~drgudang~drasalbarang~drasalbarangkategori~drjenispenjualan~drjenispenjualankategori~drcarabayar~drsumber~drautonotransaksi~drnotransaksi~drtgl~drkodepa~drcustomer~drcustomerkontak~dr1alamat1~dr1alamat2~dr1alamat3~dr2alamat1~dr2alamat2~dr2alamat3~drbagianpenjualan~drbagianpengiriman~drekspedisi~drtglkirim~drtermin~drtgljatuhtempo~druraian~drcatatan~drnoref~drtglnoref~drtglpenutupan~drmatauang~drkurs~drhargatermasukpajak~drtotal~drdiskonpersen~drjmldiskon~drtotalpajak1detail~drtotalpajak2detail~drbiayalainpersen~drbiayalain~drtotaltransaksi~drrekdiskon~drrekpajak1~drrekpajak2~drrekbiayalain~dridsq~dridso~dridpi~dridpl~driddo~drstatussi~drstatusrnr~drstatussr~drstatus~drstatussebelumnya~drjmlrevisi~drcetakanke~drinputuser~drinputtgl~drmodifikasiuser~drmodifikasitgl~drposting~drtutupperiode~drisclose~drcustomtext1~drcustomtext2~drcustomtext3~drcustomtext4~drcustomtext5~drcustomint1~drcustomint2~drcustomint3~drcustomdbl1~drcustomdbl2~drcustomdbl3~drcustomdate1~drcustomdate2~drcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'iddrdetail(0) As Integer, iddr(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, jmlkembali(6) As Double, satuan(7) As String, nilaisatuan(8) As Double, jmlbarang(9) As Double, 
        'jmlbarangkembali(10) As Double, satuanbarang(11) As String, matauang(12) As String, kurs(13) As Double, idhppkhususmasuk(14) As Integer, 
        'idhppfifomasuk(15) As Integer, harga(16) As Double, hpp(17) As Double, diskon(18) As String, jmldiskon(19) As Double, 
        'cabang(20) As String, lokasi(21) As String, gudangasal(22) As String, gudangtransit(23) As String, gudangtujuan(24) As String, 
        'gudangkembali(25) As String, rekpersediaan(26) As String, rekhargapokok(27) As String, rekdiskonpenjualan(28) As String, pajak1(29) As String, 
        'jmlpajak1(30) As Double, pajak2(31) As String, jmlpajak2(32) As Double, costcenter(33) As String, divisi(34) As String, 
        'subdivisi(35) As String, proyek(36) As String, catatan(37) As String, urutan(38) As Integer, idsqdetail(39) As Integer, 
        'idsodetail(40) As Integer, idpidetail(41) As Integer, idpldetail(42) As Integer, iddodetail(43) As Integer, jmlsi(44) As Double, 
        'statussi(45) As Integer, jmlrnr(46) As Double, statusrnr(47) As Integer, jmlsr(48) As Double, statussr(49) As Integer, 
        'isclose(50) As Integer, customtext1(51) As String, customtext2(52) As String, customtext3(53) As String, customdbl1(54) As Double, 
        'customdbl2(55) As Double, customdbl3(56) As Double, customdate1(57) As Date, customdate2(58) As Date, customdate3(59) As Date


        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'iddrdetail, iddr, idbarang, namabarang, tipebarang, jml, jmlkembali, 
        'satuan, nilaisatuan, jmlbarang, jmlbarangkembali, satuanbarang, matauang, kurs, 
        'idhppkhususmasuk, idhppfifomasuk, harga, hpp, diskon, jmldiskon, cabang, 
        'lokasi, gudangasal, gudangtransit, gudangtujuan, gudangkembali, rekpersediaan, rekhargapokok, 
        'rekdiskonpenjualan, pajak1, jmlpajak1, pajak2, jmlpajak2, costcenter, divisi, 
        'subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpidetail, 
        'idpldetail, iddodetail, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, 
        'statussr, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "iddrdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "iddr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmlkembali", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmlbarangkembali", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idhppkhususmasuk", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idhppfifomasuk", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "hpp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmldiskon", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangtransit", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangkembali", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekpersediaan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekhargapokok", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekdiskonpenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlpajak1", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "pajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlpajak2", AsEnumTypeData.AsDouble)
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
        AsDataTableTambahField(dtdetail, "iddodetail", AsEnumTypeData.AsInt64)
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

        'Variable ValidasiBatchSerial
        Dim ftBarang As String = "", ftBarangIn As String = "", ftBarangOut As String = ""

        'Variabel ValidasiSimpan
        Dim idbarang As Integer = 0, jmlbarang As Double = 0, jmlbarangkembali As Double = 0, iddodetail As Integer = 0
        Dim ftExistOutstandingDO As String = "", ftOutstandingDO As String = "", updNilaiDO As String = "", updFilterDO As String = ""
        Dim ftExistStok As String = "", ftStok As String = "", updStokOut As String = "", gudangOut As String = ""
        Dim updStokIn As String = "", gudangIn As String = ""
        Dim updStokInKembali As String = "", gudangInKembali As String = ""

        'FILTER DO, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftDO As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 60) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'iddrdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - iddrdetail required numeric." : GoTo selesai
            End If
            'iddr(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - iddr required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'jml(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'jmlkembali(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - jmlkembali required numeric." : GoTo selesai
            End If
            'nilaisatuan(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarang(9) As Double
            'jmlbarang = jml * nilaisatuan
            dataRowDetail(9) = Double.Parse(dataRowDetail(5)) * Double.Parse(dataRowDetail(8))
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - jmlbarang required numeric." : GoTo selesai
            End If
            'jmlbarangkembali(10) As Double
            'jmlbarangkembali = jmlkembali * nilaisatuan
            dataRowDetail(10) = Double.Parse(dataRowDetail(6)) * Double.Parse(dataRowDetail(8))
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - jmlbarangkembali required numeric." : GoTo selesai
            End If
            'kurs(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'idhppkhususmasuk(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - idhppkhususmasuk required numeric." : GoTo selesai
            End If
            'idhppfifomasuk(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - idhppfifomasuk required numeric." : GoTo selesai
            End If
            'harga(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'hpp(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - hpp required numeric." : GoTo selesai
            End If
            'jmldiskon(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - jmldiskon required numeric." : GoTo selesai
            End If
            'jmlpajak1(30) As Double
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak1 required numeric." : GoTo selesai
            End If
            'jmlpajak2(32) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak2 required numeric." : GoTo selesai
            End If
            'urutan(38) As Integer
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idsqdetail(39) As Integer
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - idsqdetail required numeric." : GoTo selesai
            End If
            'idsodetail(40) As Integer
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - idsodetail required numeric." : GoTo selesai
            End If
            'idpidetail(41) As Integer
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - idpidetail required numeric." : GoTo selesai
            End If
            'idpldetail(42) As Integer
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - idpldetail required numeric." : GoTo selesai
            End If
            'iddodetail(43) As Integer
            If (IsNumeric(dataRowDetail(43)) = False) Then
                result(2) = "Row : " & i & " - iddodetail required numeric." : GoTo selesai
            End If
            'jmlsi(44) As Double
            If (IsNumeric(dataRowDetail(44)) = False) Then
                result(2) = "Row : " & i & " - jmlsi required numeric." : GoTo selesai
            End If
            'statussi(45) As Integer
            If (IsNumeric(dataRowDetail(45)) = False) Then
                result(2) = "Row : " & i & " - statussi required numeric." : GoTo selesai
            End If
            'jmlrnr(46) As Double
            If (IsNumeric(dataRowDetail(46)) = False) Then
                result(2) = "Row : " & i & " - jmlrnr required numeric." : GoTo selesai
            End If
            'statusrnr(47) As Integer
            If (IsNumeric(dataRowDetail(47)) = False) Then
                result(2) = "Row : " & i & " - statusrnr required numeric." : GoTo selesai
            End If
            'jmlsr(48) As Double
            If (IsNumeric(dataRowDetail(48)) = False) Then
                result(2) = "Row : " & i & " - jmlsr required numeric." : GoTo selesai
            End If
            'statussr(49) As Integer
            If (IsNumeric(dataRowDetail(49)) = False) Then
                result(2) = "Row : " & i & " - statussr required numeric." : GoTo selesai
            End If
            'isclose(50) As Integer
            If (IsNumeric(dataRowDetail(50)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(54) As Double
            If (IsNumeric(dataRowDetail(54)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(55) As Double
            If (IsNumeric(dataRowDetail(55)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(56) As Double
            If (IsNumeric(dataRowDetail(56)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(57) As Date
            If (IsDate(dataRowDetail(57)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(58) As Date
            If (IsDate(dataRowDetail(58)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(59) As Date
            If (IsDate(dataRowDetail(59)) = False) Then
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
            If dataRowDetail(5) < 0 Then
                result(2) = "Row : " & i & " - jml can't be less than zero" : GoTo selesai
            End If

            'jmlkembali(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - jmlkembali can't be empty" : GoTo selesai
            End If
            If dataRowDetail(6) < 0 Then
                result(2) = "Row : " & i & " - jmlkembali can't be less than zero" : GoTo selesai
            End If

            If Double.Parse(dataRowDetail(5)) + Double.Parse(dataRowDetail(6)) <= 0 Then
                result(2) = "Row : " & i & " - jml and jmlkembali can't be less than or equal to zero" : GoTo selesai
            End If

            'satuan(7) As String
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(7)) > 25 Then
                result(2) = "Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'nilaisatuan(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmlbarang(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarang can't be empty" : GoTo selesai
            End If
            If dataRowDetail(9) < 0 Then
                result(2) = "Row : " & i & " - jmlbarang can't be less than zero" : GoTo selesai
            End If

            'jmlbarangkembali(10) As Double
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarangkembali can't be empty" : GoTo selesai
            End If
            If dataRowDetail(10) < 0 Then
                result(2) = "Row : " & i & " - jmlbarangkembali can't be less than zero" : GoTo selesai
            End If

            If Double.Parse(dataRowDetail(9)) + Double.Parse(dataRowDetail(10)) <= 0 Then
                result(2) = "Row : " & i & " - jmlbarang and jmlbarangkembali can't be less than or equal to zero" : GoTo selesai
            End If

            'satuanbarang(11) As String
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(11)) > 25 Then
                result(2) = "Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
            End If

            'matauang(12) As String
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(12)) > 25 Then
                result(2) = "Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'harga(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'hpp(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - hpp can't be empty" : GoTo selesai
            End If

            'diskon(18) As String
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - diskon can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(18)) > 25 Then
                result(2) = "Row : " & i & " - diskon should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskon(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskon can't be empty" : GoTo selesai
                'Else
                '    'HITUNG JMLDISKON : jml(5) As Double, harga(16) As Double, diskon(18) As String
                '    dataRowDetail(19) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(16)), FixQuotes(dataRowDetail(18).ToString))
            End If

            'gudangasal(22) As String
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - gudangasal can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(22)) > 25 Then
                result(2) = "Row : " & i & " - gudangasal should not be more than 25 character." : GoTo selesai
            End If

            'gudangtransit(23) As String
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - gudangtransit can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(23)) > 25 Then
                result(2) = "Row : " & i & " - gudangtransit should not be more than 25 character." : GoTo selesai
            End If

            'gudangtujuan(24) As String
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - gudangtujuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(24)) > 25 Then
                result(2) = "Row : " & i & " - gudangtujuan should not be more than 25 character." : GoTo selesai
            End If

            'gudangkembali(25) As String
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - gudangkembali can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(25)) > 25 Then
                result(2) = "Row : " & i & " - gudangkembali should not be more than 25 character." : GoTo selesai
            End If

            'jmlpajak1(30) As Double
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak1 can't be empty" : GoTo selesai
            End If

            'jmlpajak2(32) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak2 can't be empty" : GoTo selesai
            End If

            'jmlsi(44) As Double
            If Len(dataRowDetail(44)) = 0 Then
                result(2) = "Row : " & i & " - jmlsi can't be empty" : GoTo selesai
            End If

            'jmlrnr(46) As Double
            If Len(dataRowDetail(46)) = 0 Then
                result(2) = "Row : " & i & " - jmlrnr can't be empty" : GoTo selesai
            End If

            'jmlsr(48) As Double
            If Len(dataRowDetail(48)) = 0 Then
                result(2) = "Row : " & i & " - jmlsr can't be empty" : GoTo selesai
            End If

            'customdbl1(54) As Double
            If Len(dataRowDetail(54)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(55) As Double
            If Len(dataRowDetail(55)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(56) As Double
            If Len(dataRowDetail(56)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(57) As Date
            If Len(dataRowDetail(57)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(58) As Date
            If Len(dataRowDetail(58)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(59) As Date
            If Len(dataRowDetail(59)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "iddrdetail~iddr~idbarang~namabarang~tipebarang~jml~jmlkembali~satuan~nilaisatuan~jmlbarang~jmlbarangkembali~satuanbarang~matauang~kurs~idhppkhususmasuk~idhppfifomasuk~harga~hpp~diskon~jmldiskon~cabang~lokasi~gudangasal~gudangtransit~gudangtujuan~gudangkembali~rekpersediaan~rekhargapokok~rekdiskonpenjualan~pajak1~jmlpajak1~pajak2~jmlpajak2~costcenter~divisi~subdivisi~proyek~catatan~urutan~idsqdetail~idsodetail~idpidetail~idpldetail~iddodetail~jmlsi~statussi~jmlrnr~statusrnr~jmlsr~statussr~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56) & "~" & dataRowDetail(57) & "~" & dataRowDetail(58) & "~" & dataRowDetail(59)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If


            'Set Variabel -----------------------------------------------
            'idbarang(2) As Integer     , jmlbarang(9) As Double       , jmlbarangkembali(10) As Double
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(9) : jmlbarangkembali = dataRowDetail(10)
            'gudangtransit(23) As String  , gudangtujuan(24) As String   , gudangkembali(25) As String
            gudangOut = dataRowDetail(23) : gudangIn = dataRowDetail(24) : gudangInKembali = dataRowDetail(25)
            'iddodetail(43) As Integer
            iddodetail = dataRowDetail(43)


            'ValidasiBatchSerial
            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")

            If jmlbarangkembali > 0 Then
                'JIKA BARANG MASUK MAKA FILTER BATCH DAN SERIAL MASUK
                ftBarangIn = IIf(Len(ftBarangIn.ToString) = 0, "", ftBarangIn & " OR ")
                ftBarangIn = String.Concat(ftBarangIn, "(bid = '" & idbarang & "')")
            End If
            If jmlbarang > 0 Then
                'JIKA BARANG KELUAR MAKA FILTER BATCH DAN SERIAL KELUAR
                ftBarangOut = IIf(Len(ftBarangOut.ToString) = 0, "", ftBarangOut & " OR ")
                ftBarangOut = String.Concat(ftBarangOut, "(bid = '" & idbarang & "')")
            End If


            'ValidasiSimpan
            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'VALIDASI OUTSTANDING -------------------------
            If iddodetail <> 0 Then 'DO
                'CEK DO YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftDO = IIf(Len(ftDO.ToString) = 0, "", ftDO & " OR ")
                ftDO = String.Concat(ftDO, " (dod.iddodetail = " & iddodetail & ") ")

                '1. CEK DATA EXIST 
                ftExistOutstandingDO = IIf(Len(ftExistOutstandingDO.ToString) = 0, "", ftExistOutstandingDO & " UNION ")
                ftExistOutstandingDO = String.Concat(ftExistOutstandingDO, "SELECT EXISTS(SELECT 1 FROM m5_do_detail JOIN m5_do ON iddo = doid WHERE iddodetail = '" & iddodetail & "' AND (dostatus = 2 OR dostatus = 3 OR dostatus = 4 OR dostatus = 7) LIMIT 1) as rowExists, '" & iddodetail & "' as iddodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "iddodetail=" & iddodetail)
                Dim OutstandingKembali As Double = AsDataTableDSum(dtdetail, "jmlbarangkembali", "iddodetail=" & iddodetail)
                ftOutstandingDO = IIf(Len(ftOutstandingDO.ToString) = 0, "", ftOutstandingDO & " OR ")
                ftOutstandingDO = String.Concat(ftOutstandingDO, " (dod.iddodetail = " & iddodetail & " AND " & Outstanding + OutstandingKembali & " > (dod.jmlbarang - dod.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING
                updNilaiDO = String.Concat("WHEN '" & iddodetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding + OutstandingKembali & "', 5) ", updNilaiDO)

                '4. SET FILTER UPDATE OUTSTANDING
                updFilterDO = IIf(Len(updFilterDO.ToString) = 0, "", updFilterDO & " OR ")
                updFilterDO = String.Concat(updFilterDO, "(iddodetail = '" & iddodetail & "')")
            End If

            'VALIDASI STOK -------------------------------
            '1. CEK DATA EXIST STOK KELUAR
            ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
            ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

            '2. CEK JML STOK KELUAR
            Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangtransit='" & gudangOut & "'")
            ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
            ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > isw.stok) ")

            '3. SET NILAI UPDATE STOK KELUAR
            updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
            updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang + jmlbarangkembali & "'))") ' idbarang, kgudang, stok

            '4. SET NILAI UPDATE STOK MASUK
            updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
            updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok

            '5. SET NILAI UPDATE STOK KEMBALI
            updStokInKembali = IIf(Len(updStokInKembali.ToString) = 0, "", updStokInKembali & ", ")
            updStokInKembali = String.Concat(updStokInKembali, "('" & idbarang & "', '" & gudangInKembali & "', '" & jmlbarangkembali & "')") ' idbarang, kgudang, stok
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
        Dim jenismutasi As Double = 0
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
                'nbtjenismutasi(1) As Integer
                jenismutasi = dataRowBatch(1)
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
                'nstjenismutasi(1) As Integer
                jenismutasi = dataRowSerial(1)
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("drtgl")), AsFormatTanggal(drutama("drtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("drstatus") = 2 Then

                    Dim rsValidasi As String

                    'VALIDASI BATCH SERIAL IN ---------------
                    'ValidasiBatchSerial
                    If Len(ftBarangIn) > 0 Then
                        rsValidasi = ValidasiBatchSerial(dtdetail, dtbatch, dtserial, ftBarangIn, "jmlbarangkembali", 1)
                        If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    End If
                    'END OF VALIDASI BATCH SERIAL IN --------

                    'VALIDASI BATCH SERIAL OUT ---------------
                    'ValidasiBatchSerial
                    If Len(ftBarangOut) > 0 Then
                        rsValidasi = ValidasiBatchSerial(dtdetail, dtbatch, dtserial, ftBarangOut, "jmlbarang", 0)
                        If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    End If
                    'END OF VALIDASI BATCH SERIAL OUT --------

                    rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstandingDO, ftOutstandingDO, ftExistStok, ftStok, ftExistBatch, ftBatch, ftExistSerial, ftSerial, "gudangtransit", ftDO, drutama("drhargatermasukpajak"))
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("drtermin").ToString, AsFormatTanggal(drutama("drtgl")), "drtgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("drtgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                ''PERHITUNGAN TOTAL UTAMA ================================
                ''DIAMBILKAN DARI DATA DETAIL

                ''TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                ''SUBTOTAL = (jml * harga) - jmldiskon
                'AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                'dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                ''TOTAL = subtotal
                'drutama("drtotal") = AsDataTableDSum(dtdetail, "subtotal")

                ''TOTALPAJAK1 = jmlpajak1
                'drutama("drtotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                ''TOTALPAJAK2 = jmlpajak2
                'drutama("drtotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                ''JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                ''JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'If Integer.Parse(drutama("drhargatermasukpajak")) = 0 Then
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                '    drutama("drtotaltransaksi") = Double.Parse(drutama("drtotal")) - Double.Parse(drutama("drjmldiskon")) + Double.Parse(drutama("drtotalpajak1detail")) + Double.Parse(drutama("drtotalpajak2detail")) + Double.Parse(drutama("drbiayalain"))

                'Else
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                '    drutama("drtotaltransaksi") = Double.Parse(drutama("drtotal")) - Double.Parse(drutama("drjmldiskon")) + Double.Parse(drutama("drbiayalain"))

                'End If
                ''END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("drid")
                    notransaksi = drutama("drnotransaksi")

                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(drid), drnotransaksi FROM M5_dr WHERE drid='" & result(4) & "' AND drstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(drid) FROM m5_dr WHERE drnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_dr_history
                        Dim rsSimpanHistory As String = SimpanHistory.m5_Dr_HistorySimpan("" & paramSplit(0) & "★M5_Dr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("drsumber")) & "▼" & FixQuotes(drutama("drid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Dr set drcabang  = '" & FixQuotes(drutama("drcabang")) & "', drlokasi  = '" & FixQuotes(drutama("drlokasi")) & "', drgudang  = '" & FixQuotes(drutama("drgudang")) & "', drasalbarang  = '" & FixQuotes(drutama("drasalbarang")) & "', drasalbarangkategori  = " & drutama("drasalbarangkategori") & ", drjenispenjualan  = '" & FixQuotes(drutama("drjenispenjualan")) & "', drjenispenjualankategori  = " & drutama("drjenispenjualankategori") & ", drcarabayar  = " & drutama("drcarabayar") & ", drsumber  = '" & FixQuotes(drutama("drsumber")) & "', drautonotransaksi  = " & drutama("drautonotransaksi") & ", drnotransaksi  = '" & FixQuotes(notransaksi) & "', drtgl  = '" & FixQuotes(AsFormatTanggal(drutama("drtgl"))) & "', drkodepa  = " & drutama("drkodepa") & ", drcustomer  = " & drutama("drcustomer") & ", drcustomerkontak  = '" & FixQuotes(drutama("drcustomerkontak")) & "', dr1alamat1  = '" & FixQuotes(drutama("dr1alamat1")) & "', dr1alamat2  = '" & FixQuotes(drutama("dr1alamat2")) & "', dr1alamat3  = '" & FixQuotes(drutama("dr1alamat3")) & "', dr2alamat1  = '" & FixQuotes(drutama("dr2alamat1")) & "', dr2alamat2  = '" & FixQuotes(drutama("dr2alamat2")) & "', dr2alamat3  = '" & FixQuotes(drutama("dr2alamat3")) & "', drbagianpenjualan  = " & drutama("drbagianpenjualan") & ", drbagianpengiriman  = " & drutama("drbagianpengiriman") & ", drekspedisi  = '" & FixQuotes(drutama("drekspedisi")) & "', drtglkirim  = '" & FixQuotes(AsFormatTanggal(drutama("drtglkirim"))) & "', drtermin  = '" & FixQuotes(drutama("drtermin")) & "', drtgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("drtgljatuhtempo"))) & "', druraian  = '" & FixQuotes(drutama("druraian")) & "', drcatatan  = '" & FixQuotes(drutama("drcatatan")) & "', drnoref  = '" & FixQuotes(drutama("drnoref")) & "', drtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("drtglnoref"))) & "', drtglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("drtglpenutupan"))) & "', drmatauang  = '" & FixQuotes(drutama("drmatauang")) & "', drkurs  = '" & FixDouble(drutama("drkurs")) & "', drhargatermasukpajak  = " & drutama("drhargatermasukpajak") & ", drtotal  = '" & FixDouble(drutama("drtotal")) & "', drdiskonpersen  = '" & FixQuotes(drutama("drdiskonpersen")) & "', drjmldiskon  = '" & FixDouble(drutama("drjmldiskon")) & "', drtotalpajak1detail  = '" & FixDouble(drutama("drtotalpajak1detail")) & "', drtotalpajak2detail  = '" & FixDouble(drutama("drtotalpajak2detail")) & "', drbiayalainpersen  = '" & FixDouble(drutama("drbiayalainpersen")) & "', drbiayalain  = '" & FixDouble(drutama("drbiayalain")) & "', drtotaltransaksi  = '" & FixDouble(drutama("drtotaltransaksi")) & "', drrekdiskon  = '" & FixQuotes(drutama("drrekdiskon")) & "', drrekpajak1  = '" & FixQuotes(drutama("drrekpajak1")) & "', drrekpajak2  = '" & FixQuotes(drutama("drrekpajak2")) & "', drrekbiayalain  = '" & FixQuotes(drutama("drrekbiayalain")) & "', dridsq  = " & drutama("dridsq") & ", dridso  = " & drutama("dridso") & ", dridpi  = " & drutama("dridpi") & ", dridpl  = " & drutama("dridpl") & ", driddo  = " & drutama("driddo") & ", drstatussi  = " & drutama("drstatussi") & ", drstatusrnr  = " & drutama("drstatusrnr") & ", drstatussr  = " & drutama("drstatussr") & ", drstatus  = " & drutama("drstatus") & ", drstatussebelumnya  = " & drutama("drstatussebelumnya") & ", drjmlrevisi  = drjmlrevisi+1, drcetakanke  = " & drutama("drcetakanke") & ", drmodifikasiuser  = " & drutama("drmodifikasiuser") & ", drmodifikasitgl  = NOW(), drposting  = 0, drtutupperiode  = " & drutama("drtutupperiode") & ", drcustomtext1  = '" & FixQuotes(drutama("drcustomtext1")) & "', drcustomtext2  = '" & FixQuotes(drutama("drcustomtext2")) & "', drcustomtext3  = '" & FixQuotes(drutama("drcustomtext3")) & "', drcustomtext4  = '" & FixQuotes(drutama("drcustomtext4")) & "', drcustomtext5  = '" & FixQuotes(drutama("drcustomtext5")) & "', drcustomint1  = " & drutama("drcustomint1") & ", drcustomint2  = " & drutama("drcustomint2") & ", drcustomint3  = " & drutama("drcustomint3") & ", drcustomdbl1  = '" & FixDouble(drutama("drcustomdbl1")) & "', drcustomdbl2  = '" & FixDouble(drutama("drcustomdbl2")) & "', drcustomdbl3  = '" & FixDouble(drutama("drcustomdbl3")) & "', drcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("drcustomdate1"))) & "', drcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("drcustomdate2"))) & "', drcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("drcustomdate3"))) & "' where drid = '" & drutama("drid") & "'"
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

                    If drutama("drautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("drcabang"), drutama("drlokasi"), drutama("drsumber"), drutama("drtgl"))
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
                        notransaksi = drutama("drnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(drid) FROM m5_dr WHERE drnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Dr (drcabang, drlokasi, drgudang, drasalbarang, drasalbarangkategori, drjenispenjualan, drjenispenjualankategori, drcarabayar, drsumber, drautonotransaksi, drnotransaksi, drtgl, drkodepa, drcustomer, drcustomerkontak, dr1alamat1, dr1alamat2, dr1alamat3, dr2alamat1, dr2alamat2, dr2alamat3, drbagianpenjualan, drbagianpengiriman, drekspedisi, drtglkirim, drtermin, drtgljatuhtempo, druraian, drcatatan, drnoref, drtglnoref, drtglpenutupan, drmatauang, drkurs, drhargatermasukpajak, drtotal, drdiskonpersen, drjmldiskon, drtotalpajak1detail, drtotalpajak2detail, drbiayalainpersen, drbiayalain, drtotaltransaksi, drrekdiskon, drrekpajak1, drrekpajak2, drrekbiayalain, dridsq, dridso, dridpi, dridpl, driddo, drstatussi, drstatusrnr, drstatussr, drstatus, drstatussebelumnya, drjmlrevisi, drcetakanke, drinputuser, drinputtgl, drmodifikasiuser, drmodifikasitgl, drposting, drtutupperiode, drisclose, drcustomtext1, drcustomtext2, drcustomtext3, drcustomtext4, drcustomtext5, drcustomint1, drcustomint2, drcustomint3, drcustomdbl1, drcustomdbl2, drcustomdbl3, drcustomdate1, drcustomdate2, drcustomdate3) values('" & FixQuotes(drutama("drcabang")) & "', '" & FixQuotes(drutama("drlokasi")) & "', '" & FixQuotes(drutama("drgudang")) & "', '" & FixQuotes(drutama("drasalbarang")) & "', " & drutama("drasalbarangkategori") & ", '" & FixQuotes(drutama("drjenispenjualan")) & "', " & drutama("drjenispenjualankategori") & ", " & drutama("drcarabayar") & ", '" & FixQuotes(drutama("drsumber")) & "', " & drutama("drautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("drtgl"))) & "', " & drutama("drkodepa") & ", " & drutama("drcustomer") & ", '" & FixQuotes(drutama("drcustomerkontak")) & "', '" & FixQuotes(drutama("dr1alamat1")) & "', '" & FixQuotes(drutama("dr1alamat2")) & "', '" & FixQuotes(drutama("dr1alamat3")) & "', '" & FixQuotes(drutama("dr2alamat1")) & "', '" & FixQuotes(drutama("dr2alamat2")) & "', '" & FixQuotes(drutama("dr2alamat3")) & "', " & drutama("drbagianpenjualan") & ", " & drutama("drbagianpengiriman") & ", '" & FixQuotes(drutama("drekspedisi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("drtglkirim"))) & "', '" & FixQuotes(drutama("drtermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("drtgljatuhtempo"))) & "', '" & FixQuotes(drutama("druraian")) & "', '" & FixQuotes(drutama("drcatatan")) & "', '" & FixQuotes(drutama("drnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("drtglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("drtglpenutupan"))) & "', '" & FixQuotes(drutama("drmatauang")) & "', '" & FixDouble(drutama("drkurs")) & "', " & drutama("drhargatermasukpajak") & ", '" & FixDouble(drutama("drtotal")) & "', '" & FixQuotes(drutama("drdiskonpersen")) & "', '" & FixDouble(drutama("drjmldiskon")) & "', '" & FixDouble(drutama("drtotalpajak1detail")) & "', '" & FixDouble(drutama("drtotalpajak2detail")) & "', '" & FixDouble(drutama("drbiayalainpersen")) & "', '" & FixDouble(drutama("drbiayalain")) & "', '" & FixDouble(drutama("drtotaltransaksi")) & "', '" & FixQuotes(drutama("drrekdiskon")) & "', '" & FixQuotes(drutama("drrekpajak1")) & "', '" & FixQuotes(drutama("drrekpajak2")) & "', '" & FixQuotes(drutama("drrekbiayalain")) & "', " & drutama("dridsq") & ", " & drutama("dridso") & ", " & drutama("dridpi") & ", " & drutama("dridpl") & ", " & drutama("driddo") & ", " & drutama("drstatussi") & ", " & drutama("drstatusrnr") & ", " & drutama("drstatussr") & ", " & drutama("drstatus") & ", " & drutama("drstatussebelumnya") & ", " & drutama("drjmlrevisi") & ", " & drutama("drcetakanke") & ", " & drutama("drinputuser") & ", NOW(), " & drutama("drmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("drtutupperiode") & ", " & drutama("drisclose") & ", '" & FixQuotes(drutama("drcustomtext1")) & "', '" & FixQuotes(drutama("drcustomtext2")) & "', '" & FixQuotes(drutama("drcustomtext3")) & "', '" & FixQuotes(drutama("drcustomtext4")) & "', '" & FixQuotes(drutama("drcustomtext5")) & "', " & drutama("drcustomint1") & ", " & drutama("drcustomint2") & ", " & drutama("drcustomint3") & ", '" & FixDouble(drutama("drcustomdbl1")) & "', '" & FixDouble(drutama("drcustomdbl2")) & "', '" & FixDouble(drutama("drcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("drcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("drcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("drcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select drid from M5_dr where drnotransaksi='" & notransaksi & "' AND drinputuser= '" & userid & "' order by drmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Dr_Detail where iddr = '" & result(4) & "'"
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
                        If Not drutama("drmatauang").ToString.Equals(dr1("matauang").ToString) Then
                            result(2) = "Row : " & dr1("urutan") & " - " & dr1("tipebarang") & " | " & dr1("namabarang") & " currency (" & dr1("matauang") & ") doesn't belong to the main transactions." : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF VALIDASI MATAUANG HARUS SAMA DENGAN TRANSAKSI UTAMA -------------


                        'SET HARGA DARI TRANSAKSI SEBELUMNYA ------------------------------------
                        If Double.Parse(dr1("iddodetail")) > 0 Then
                            'JIKA AMBIL DO MAKA SET HARGA DARI DO
                            sql = "SELECT jml, harga, diskon, pajak1, jmlpajak1, pajak2, jmlpajak2 FROM m5_do_detail WHERE iddodetail = '" & FixDouble(dr1("iddodetail")) & "'"

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
                        strValue2.Append("(" & dr1("iddrdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixDouble(dr1("jmlkembali")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixDouble(dr1("jmlbarangkembali")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', " & dr1("idhppkhususmasuk") & ", " & dr1("idhppfifomasuk") & ", '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hpp")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("gudangkembali")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("rekhargapokok")) & "', '" & FixQuotes(dr1("rekdiskonpenjualan")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idsqdetail") & ", " & dr1("idsodetail") & ", " & dr1("idpidetail") & ", " & dr1("idpldetail") & ", " & dr1("iddodetail") & ", '" & FixDouble(dr1("jmlsi")) & "', " & dr1("statussi") & ", '" & FixDouble(dr1("jmlrnr")) & "', " & dr1("statusrnr") & ", '" & FixDouble(dr1("jmlsr")) & "', " & dr1("statussr") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M5_Dr_Detail(iddrdetail, iddr, idbarang, namabarang, tipebarang, jml, jmlkembali, satuan, nilaisatuan, jmlbarang, jmlbarangkembali, satuanbarang, matauang, kurs, idhppkhususmasuk, idhppfifomasuk, harga, hpp, diskon, jmldiskon, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, gudangkembali, rekpersediaan, rekhargapokok, rekdiskonpenjualan, pajak1, jmlpajak1, pajak2, jmlpajak2, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpidetail, idpldetail, iddodetail, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                    sql = "Delete from M1_No_Batch_Transaction where nbtidtransaksi  = '" & result(4) & "' AND nbtsumber = 'DR'"
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
                    sql = "Delete from M1_No_Serial_Transaction  where nstidtransaksi  = '" & result(4) & "' AND nstsumber = 'DR'"
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


                If drutama("drstatus") = 2 Then
                    'UPDATE OUTSTANDING TRANSAKSI ======================================================
                    If Len(updNilaiDO) > 0 Then 'DO
                        'UPDATE DETAIL
                        sql = "UPDATE m5_do_detail SET jmlrealisasi = (CASE iddodetail " & updNilaiDO & " ELSE jmlrealisasi END) WHERE " & updFilterDO
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT iddo FROM m5_do_detail WHERE " & updFilterDO & " GROUP BY iddo")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(iddo = '" & dr1("iddo") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDB("SELECT iddo, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_do_detail WHERE " & ftDetail & " GROUP BY iddo")
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiDO = "" : updFilterDO = ""
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
                                updNilaiDO = String.Concat(updNilaiDO, "WHEN '" & dr1("iddo") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterDO = IIf(Len(updFilterDO.ToString) = 0, "", updFilterDO & " OR ")
                                updFilterDO = String.Concat(updFilterDO, "(doid = '" & dr1("iddo") & "')")
                            Next

                            sql = "UPDATE m5_do SET dostatusrealisasi = (CASE doid " & updNilaiDO & " ELSE dostatusrealisasi END) WHERE " & updFilterDO
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
                            'mapping        nbiidbatchin,                     nbigudang,                  nbiidbarang,                           nbikode,                             nbisumber,            nbiidtransaksi,                     nbisatuan,                 nbijmlmasuk,       nbijmlkeluar,                  nbijmlsisa, nbiisclose,                     nbicustomtext1,                             nbicustomtext2,                             nbicustomtext3,                             nbicustomdbl1,                             nbicustomdbl2,                             nbicustomdbl3,                                             nbicustomdate1,                                              nbicustomdate2,                                              nbicustomdate3
                            strValue3.Append("(" & 0 & ", '" & FixQuotes(dr1("nbtgudang")) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixQuotes(dr1("nbtcustomtext1")) & "', '" & FixQuotes(dr1("nbtcustomtext2")) & "', '" & FixQuotes(dr1("nbtcustomtext3")) & "', '" & FixDouble(dr1("nbtcustomdbl1")) & "', '" & FixDouble(dr1("nbtcustomdbl2")) & "', '" & FixDouble(dr1("nbtcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate3"))) & "')")
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
                            'mapping       nsiidserialin,                     nsigudang,                  nsiidbarang,                           nsikode,                             nsisumber,            nsiidtransaksi,                     nsisatuan,                       nsijmlmasuk, nsijmlkeluar,                  nsijmlsisa, nsiisclose,                     nsicustomtext1,                             nsicustomtext2,                             nsicustomtext3,                             nsicustomdbl1,                             nsicustomdbl2,                             nsicustomdbl3,                                             nsicustomdate1,                                              nsicustomdate2,                                              nsicustomdate3
                            strValue3.Append("(" & 0 & ", '" & FixQuotes(dr1("nstgudang")) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixQuotes(dr1("nstcustomtext1")) & "', '" & FixQuotes(dr1("nstcustomtext2")) & "', '" & FixQuotes(dr1("nstcustomtext3")) & "', '" & FixDouble(dr1("nstcustomdbl1")) & "', '" & FixDouble(dr1("nstcustomdbl2")) & "', '" & FixDouble(dr1("nstcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate3"))) & "')")
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

                    'STOK KEMBALI
                    If Len(updStokInKembali) > 0 Then
                        sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokInKembali & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
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
                    sql = "SELECT drd.iddrdetail, drd.idbarang, drd.namabarang, drd.tipebarang, drd.jml, drd.jmlbarang, drd.jmlkembali, drd.jmlbarangkembali, drd.satuan, drd.satuanbarang, drd.matauang, drd.kurs, drd.harga, drd.diskon, drd.jmldiskon, drd.hpp, drd.idhppkhususmasuk, drd.gudangasal, drd.gudangtransit, drd.gudangtujuan, drd.gudangkembali, drd.catatan, drd.costcenter, drd.divisi, drd.subdivisi, drd.proyek, dr.drinputtgl, i.bhpp FROM m5_dr_detail drd JOIN m5_dr dr ON drd.iddr = dr.drid JOIN m1_item i ON drd.idbarang = i.bid WHERE drd.iddr = '" & result(4) & "'"
                    Dim dtDetailNew As DataTable = AsDataTableAmbilDariDB(sql)
                    Dim hpp As Double = 0, postinghpp As Double = 0
                    Dim strTransaksiBarang As New StringBuilder

                    Dim jmlTransaksi As Double = 0, jmlTransaksiKembali As Double = 0

                    If dtDetailNew.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtDetailNew.Rows
                            'jenismutasi dan postinghpp 
                            '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 1
                            '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                            '- untuk transaksi mutasi saja maka postinghpp = 0
                            postinghpp = 0

                            'jml
                            jmlTransaksi = Double.Parse(dr1("jml"))
                            jmlTransaksiKembali = Double.Parse(dr1("jmlkembali"))

                            'jmlbarang
                            jmlbarang = Double.Parse(dr1("jmlbarang"))
                            jmlbarangkembali = Double.Parse(dr1("jmlbarangkembali"))

                            'hitung hpp = hpp
                            hpp = Double.Parse(dr1("hpp"))

                            'POSTING BARANG KELUAR (gudangtransit) == jmlbarang + jmlbarangkembali
                            jenismutasi = 0
                            'QUERY INSERT TRANSAKSI BARANG KELUAR
                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                            'mapping                        id,                             cabang,                                   lokasi,                                   gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,             iddetail,                    notransaksi,                                                 tgl,                              kontak,               idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                                                       satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                  idhppikm,                idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("drcabang")) & "', '" & FixQuotes(drutama("drlokasi")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', " & drutama("drkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("drsumber")) & "', " & result(4) & ", " & dr1("iddrdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("drtgl"))) & "', " & drutama("drcustomer") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(jmlTransaksi + jmlTransaksiKembali) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(jmlbarang + jmlbarangkembali) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("druraian")) & "', '" & FixQuotes(drutama("drcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("drinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("drinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")

                            'POSTING BARANG MASUK (gudangkembali)
                            If jmlbarangkembali <> 0 Then
                                jenismutasi = 1
                                'QUERY INSERT TRANSAKSI BARANG MASUK
                                strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                                'mapping                        id,                             cabang,                                   lokasi,                                  gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,             iddetail,                    notransaksi,                                                 tgl,                              kontak,               idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                                         satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                  idhppikm,                idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("drcabang")) & "', '" & FixQuotes(drutama("drlokasi")) & "', '" & FixQuotes(dr1("gudangkembali")) & "', " & drutama("drkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("drsumber")) & "', " & result(4) & ", " & dr1("iddrdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("drtgl"))) & "', " & drutama("drcustomer") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(jmlTransaksiKembali) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(jmlbarangkembali) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("druraian")) & "', '" & FixQuotes(drutama("drcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("drinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("drinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                            End If

                            'POSTING BARANG MASUK (gudangtujuan)
                            jenismutasi = 1
                            'QUERY INSERT TRANSAKSI BARANG MASUK
                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                            'mapping                        id,                             cabang,                                   lokasi,                                  gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,             iddetail,                    notransaksi,                                                 tgl,                              kontak,               idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                                 satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                  idhppikm,                idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("drcabang")) & "', '" & FixQuotes(drutama("drlokasi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', " & drutama("drkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("drsumber")) & "', " & result(4) & ", " & dr1("iddrdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("drtgl"))) & "', " & drutama("drcustomer") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(jmlTransaksi) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("druraian")) & "', '" & FixQuotes(drutama("drcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("drinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("drinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
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
                Dim sumber As String = "DR", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M5_DrUpdateStatusOld(ByVal param As String) As String

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
            Dim sumber As String = "DR", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Drtgl, Drnotransaksi, Drstatus FROM M5_Dr WHERE Drid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Drstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m5_dr_history
            Dim rsSimpanHistory As String = SimpanHistory.m5_Dr_HistorySimpan("" & paramSplit(0) & "★M5_Dr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m5_dr_terkait("drid = '" & idtransaksi & "'")
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


                Dim idbarang As Integer = 0, jmlbarang As Double = 0, jmlbarangkembali As Double = 0, iddodetail As Integer = 0
                Dim updNilaiDO As String = "", updFilterDO As String = ""
                Dim ftExistStok As String = "", ftStok As String = "", updStokOut As String = "", gudangOut As String = ""
                Dim ftExistStokKembali As String = "", ftStokKembali As String = "", updStokOutKembali As String = "", gudangOutKembali As String = ""
                Dim updStokIn As String = "", gudangIn As String = ""

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDB("SELECT iddrdetail, idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, jmlbarangkembali, iddodetail, gudangtransit, gudangtujuan, gudangkembali, idhppkhususmasuk, idhppfifomasuk, urutan FROM m5_dr_detail WHERE iddr = '" & idtransaksi & "'")
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        '1. SET NILAI
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : jmlbarangkembali = dr1("jmlbarangkembali")
                        gudangIn = dr1("gudangtransit") : gudangOut = dr1("gudangtujuan") : gudangOutKembali = dr1("gudangkembali")
                        iddodetail = dr1("iddodetail")

                        '2. BUAT FILTER UPDATE OUTSTANDING
                        If iddodetail <> 0 Then
                            '2.1 SET NILAI UPDATE OUTSTANDING DO
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "iddodetail=" & iddodetail)
                            Dim OutstandingKembali As Double = AsDataTableDSum(dtdetail, "jmlbarangkembali", "iddodetail=" & iddodetail)
                            updNilaiDO = String.Concat("WHEN '" & iddodetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding + OutstandingKembali & "', 5) ", updNilaiDO)

                            '2.2. SET FILTERUPDATE OUTSTANDING DO
                            updFilterDO = IIf(Len(updFilterDO.ToString) = 0, "", updFilterDO & " OR ")
                            updFilterDO = String.Concat(updFilterDO, "(iddodetail = '" & iddodetail & "')")
                        End If

                        'VALIDASI STOK -------------------------------
                        '1. CEK DATA EXIST STOK TUJUAN
                        ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                        ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists,  bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                        '2. CEK JML STOK TUJUAN
                        Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangtujuan='" & gudangOut & "'")
                        ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
                        ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > isw.stok) ")

                        '3. SET NILAI UPDATE STOK KELUAR TUJUAN
                        updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
                        updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

                        '4. CEK DATA EXIST STOK KEMBALI
                        ftExistStokKembali = IIf(Len(ftExistStokKembali.ToString) = 0, "", ftExistStokKembali & " UNION ")
                        ftExistStokKembali = String.Concat(ftExistStokKembali, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOutKembali & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists,  bkode, '" & gudangOutKembali & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                        '5. CEK JML STOK KEMBALI
                        Dim StokKembali As Double = AsDataTableDSum(dtdetail, "jmlbarangkembali", "idbarang=" & idbarang & " AND gudangkembali='" & gudangOutKembali & "'")
                        ftStokKembali = IIf(Len(ftStokKembali.ToString) = 0, "", ftStokKembali & " OR ")
                        ftStokKembali = String.Concat(ftStokKembali, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOutKembali & "' AND " & StokKembali & " > isw.stok) ")

                        '6. SET NILAI UPDATE STOK KELUAR KEMBALI
                        updStokOutKembali = IIf(Len(updStokOutKembali.ToString) = 0, "", updStokOutKembali & ", ")
                        updStokOutKembali = String.Concat(updStokOutKembali, "('" & idbarang & "', '" & gudangOutKembali & "', ('-" & jmlbarangkembali & "'))") ' idbarang, kgudang, stok

                        '7. SET NILAI UPDATE STOK MASUK 
                        updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
                        updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang + jmlbarangkembali & "')") ' idbarang, kgudang, stok

                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If


                'VALIDASI STOK ----------------------------------
                'STOK TUJUAN
                Dim rsValidasi As String = ValidasiSimpan(dtdetail, "", "", ftExistStok, ftStok, "", "", "", "", "", "", "")
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'STOK KEMBALI
                rsValidasi = ValidasiSimpan(dtdetail, "", "", ftExistStokKembali, ftStokKembali, "", "", "", "", "", "", "")
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'END OF VALIDASI STOK ---------------------------


                'UPDATE OUTSTANDING =============================================================
                If Len(updFilterDO) > 0 Then
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m5_do_detail SET jmlrealisasi = (CASE iddodetail " & updNilaiDO & " ELSE jmlrealisasi END) WHERE " & updFilterDO
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT iddo FROM m5_do_detail WHERE " & updFilterDO & " GROUP BY iddo")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(iddo = '" & dr1("iddo") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDB("SELECT iddo, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_do_detail WHERE " & ftDetail & " GROUP BY iddo")
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiDO = "" : updFilterDO = ""
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
                            updNilaiDO = String.Concat(updNilaiDO, "WHEN '" & dr1("iddo") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterDO = IIf(Len(updFilterDO.ToString) = 0, "", updFilterDO & " OR ")
                            updFilterDO = String.Concat(updFilterDO, "(doid = '" & dr1("iddo") & "')")
                        Next

                        sql = "UPDATE m5_do SET dostatusrealisasi = (CASE doid " & updNilaiDO & " ELSE dostatusrealisasi END) WHERE " & updFilterDO
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


                'UPDATE STOK ====================================================================
                'STOK KELUAR TUJUAN
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

                'STOK KELUAR KEMBALI
                If Len(updStokOut) > 0 Then
                    sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokOutKembali & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
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
            sql = "UPDATE M5_Dr SET Drstatus = " & nilaiStatus & ", Drmodifikasiuser='" & userid & "', Drmodifikasitgl = NOW(), Drposting = 0, Drpostingtgl = '1971-01-01 00:00:00', Drjmlrevisi = Drjmlrevisi + 1 WHERE Drid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_DrSearch(PostWsSearch(paramSplit(0), "M5_DrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M5_DrDeleteOld(ByVal param As String) As String

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
            Dim sumber As String = "DR", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Drid, Drnotransaksi FROM M5_Dr WHERE Drid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT drcabang, drlokasi, drsumber, drautonotransaksi, drnotransaksi, drtgl"
            sql &= " FROM M5_dr"
            sql &= " WHERE drid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("drcabang")
                lokasi = dtNomorNext.Rows(0)("drlokasi")
                sumber = dtNomorNext.Rows(0)("drsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("drautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("drnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("drtgl"))
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
            sql = "DELETE FROM M5_Dr_Detail WHERE iddr='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE UTAMA
            sql = "DELETE FROM M5_Dr WHERE drid='" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_DrSearch(PostWsSearch(paramSplit(0), "M5_DrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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