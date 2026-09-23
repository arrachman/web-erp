Imports System.Web
Imports System.Web.Services
'Imports System.Web.Services.Protocols
'Imports System.Web.Script.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization

'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m7_asset
    Inherits System.Web.Services.WebService
    Public ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi

    <WebMethod()>
    Public Function M7_AssetSimpan(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean

        Dim pg1 As New RsPaging
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = ""

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


        'MAPPING BUAT WS ----------------------------------------------------------
        'aid(0) As Integer, akode(1) As String, anama(2) As String, akategori(3) As String, acabang(4) As String, 
        'alokasi(5) As String, adivisi(6) As String, asubdivisi(7) As String, acatatan(8) As String, anomor(9) As String, 
        'atglbeli(10) As Date, atglpakai(11) As Date, amatauang(12) As String, akurs(13) As Double, ahargabeli(14) As Double, 
        'anilairesidu(15) As Double, aumurekonomis(16) As Double, abebanperbln(17) As Double, aakumulasibeban(18) As Double, anilaibuku(19) As Double, 
        'ametode(20) As Integer, atabelpenyusutan(21) As String, aintangible(22) As Integer, afiskal(23) As Integer, aatastengahbulan(24) As Integer, 
        'arekasset(25) As String, arekakumdepresiasi(26) As String, arekdepresiasi(27) As String, arekpenghapusan(28) As String, aprodusen(29) As Integer, 
        'atglpensiun(30) As Date, apenyusutanke(31) As Double, anilaimenurun(32) As Double, adispose(33) As Integer, apembelian(34) As Integer, 
        'apenjualan(35) As Integer, alocked(36) As Integer, astatus(37) As Integer, astatussebelumnya(38) As Integer, aisclose(39) As Integer, 
        'ainputuser(40) As Integer, ainputtgl(41) As DateTime, amodifikasiuser(42) As Integer, amodifikasitgl(43) As DateTime, acustomtext1(44) As String, 
        'acustomtext2(45) As String, acustomtext3(46) As String, acustomtext4(47) As String, acustomtext5(48) As String, acustomint1(49) As Integer, 
        'acustomint2(50) As Integer, acustomint3(51) As Integer, acustomdbl1(52) As Double, acustomdbl2(53) As Double, acustomdbl3(54) As Double, 
        'acustomdate1(55) As Date, acustomdate2(56) As Date, acustomdate3(57) As Date,
        'acostcenter(58) As String, aproyek(59) As String, ajml(60) As Double, asatuan(61) As String, aharga(62) As Double, adiskon(63) As String, 
        'ajmldiskon(64) As Double, apajak1(65) As String, ajmlpajak1(66) As Double, apajak2(67) As String, ajmlpajak2(68) As Double

        'MAPPING BUAT FLEX --------------------------------------------------------
        'aid, akode, anama, akategori, acabang, alokasi, adivisi, 
        'asubdivisi, acatatan, anomor, atglbeli, atglpakai, amatauang, akurs, 
        'ahargabeli, anilairesidu, aumurekonomis, abebanperbln, aakumulasibeban, anilaibuku, ametode, 
        'atabelpenyusutan, aintangible, afiskal, aatastengahbulan, arekasset, arekakumdepresiasi, arekdepresiasi, 
        'arekpenghapusan, aprodusen, atglpensiun, apenyusutanke, anilaimenurun, adispose, apembelian, 
        'apenjualan, alocked, astatus, astatussebelumnya, aisclose, ainputuser, ainputtgl, 
        'amodifikasiuser, amodifikasitgl, acustomtext1, acustomtext2, acustomtext3, acustomtext4, acustomtext5, 
        'acustomint1, acustomint2, acustomint3, acustomdbl1, acustomdbl2, acustomdbl3, acustomdate1, 
        'acustomdate2, acustomdate3, acostcenter, aproyek, ajml, asatuan, aharga, adiskon, ajmldiskon, 
        'apajak1, ajmlpajak1, apajak2, ajmlpajak2


        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 69) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================


        'VALIDASI TIPE DATA ==========================================================
        'aid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "aid required numeric." : GoTo selesai
        End If
        'atglbeli(10) As Date
        If (IsDate(dataUtama(10)) = False) Then
            result(2) = "atglbeli required date." : GoTo selesai
        End If
        'atglpakai(11) As Date
        If (IsDate(dataUtama(11)) = False) Then
            result(2) = "atglpakai required date." : GoTo selesai
        End If
        'akurs(13) As Double
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "akurs required numeric." : GoTo selesai
        End If
        'ahargabeli(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "ahargabeli required numeric." : GoTo selesai
        End If
        'anilairesidu(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "anilairesidu required numeric." : GoTo selesai
        End If
        'aumurekonomis(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "aumurekonomis required numeric." : GoTo selesai
        End If
        'abebanperbln(17) As Double
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "abebanperbln required numeric." : GoTo selesai
        End If
        'aakumulasibeban(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "aakumulasibeban required numeric." : GoTo selesai
        End If
        'anilaibuku(19) As Double
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "anilaibuku required numeric." : GoTo selesai
        End If
        'ametode(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "ametode required numeric." : GoTo selesai
        End If
        'aintangible(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "aintangible required numeric." : GoTo selesai
        End If
        'afiskal(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "afiskal required numeric." : GoTo selesai
        End If
        'aatastengahbulan(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "aatastengahbulan required numeric." : GoTo selesai
        End If
        'aprodusen(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "aprodusen required numeric." : GoTo selesai
        End If
        'atglpensiun(30) As Date
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "atglpensiun required date." : GoTo selesai
        End If
        'apenyusutanke(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "apenyusutanke required numeric." : GoTo selesai
        End If
        'anilaimenurun(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "anilaimenurun required numeric." : GoTo selesai
        End If
        'adispose(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "adispose required numeric." : GoTo selesai
        End If
        'apembelian(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "apembelian required numeric." : GoTo selesai
        End If
        'apenjualan(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "apenjualan required numeric." : GoTo selesai
        End If
        'alocked(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "alocked required numeric." : GoTo selesai
        End If
        'astatus(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "astatus required numeric." : GoTo selesai
        End If
        'astatussebelumnya(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "astatussebelumnya required numeric." : GoTo selesai
        End If
        'aisclose(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "aisclose required numeric." : GoTo selesai
        End If
        'ainputuser(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "ainputuser required numeric." : GoTo selesai
        End If
        'ainputtgl(41) As DateTime
        If (IsDate(dataUtama(41)) = False) Then
            result(2) = "ainputtgl required date." : GoTo selesai
        End If
        'amodifikasiuser(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "amodifikasiuser required numeric." : GoTo selesai
        End If
        'amodifikasitgl(43) As DateTime
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "amodifikasitgl required date." : GoTo selesai
        End If
        'acustomint1(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "acustomint1 required numeric." : GoTo selesai
        End If
        'acustomint2(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "acustomint2 required numeric." : GoTo selesai
        End If
        'acustomint3(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "acustomint3 required numeric." : GoTo selesai
        End If
        'acustomdbl1(52) As Double
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "acustomdbl1 required numeric." : GoTo selesai
        End If
        'acustomdbl2(53) As Double
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "acustomdbl2 required numeric." : GoTo selesai
        End If
        'acustomdbl3(54) As Double
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "acustomdbl3 required numeric." : GoTo selesai
        End If
        'acustomdate1(55) As Date
        If (IsDate(dataUtama(55)) = False) Then
            result(2) = "acustomdate1 required date." : GoTo selesai
        End If
        'acustomdate2(56) As Date
        If (IsDate(dataUtama(56)) = False) Then
            result(2) = "acustomdate2 required date." : GoTo selesai
        End If
        'acustomdate3(57) As Date
        If (IsDate(dataUtama(57)) = False) Then
            result(2) = "acustomdate3 required date." : GoTo selesai
        End If

        'ajml(60) As Double
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "ajml required numeric." : GoTo selesai
        End If
        'aharga(62) As Double
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "aharga required numeric." : GoTo selesai
        End If
        'ajmldiskon(64) As Double
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "ajmldiskon required numeric." : GoTo selesai
        End If
        'ajmlpajak1(66) As Double
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "ajmlpajak1 required numeric." : GoTo selesai
        End If
        'ajmlpajak2(68) As Double
        If (IsNumeric(dataUtama(68)) = False) Then
            result(2) = "ajmlpajak2 required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA ===================================================


        'VALIDASI DATA ===============================================================
        'akode(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "akode can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 500 Then
            result(2) = "akode should not be more than 500 character." : GoTo selesai
        End If

        'anama(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "anama can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 500 Then
            result(2) = "anama should not be more than 500 character." : GoTo selesai
        End If

        'akategori(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "akategori can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "akategori should not be more than 25 character." : GoTo selesai
        End If

        'atglbeli(10) As Date
        If Len(dataUtama(10)) = 0 Then
            result(2) = "atglbeli can't be empty" : GoTo selesai
        End If

        'atglpakai(11) As Date
        If Len(dataUtama(11)) = 0 Then
            result(2) = "atglpakai can't be empty" : GoTo selesai
        End If

        'amatauang(12) As String
        If Len(dataUtama(12)) = 0 Then
            result(2) = "amatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(12)) > 25 Then
            result(2) = "amatauang should not be more than 25 character." : GoTo selesai
        End If

        'akurs(13) As Double
        If Len(dataUtama(13)) = 0 Then
            result(2) = "akurs can't be empty" : GoTo selesai
        End If

        'ahargabeli(14) As Double
        If Len(dataUtama(14)) = 0 Then
            result(2) = "ahargabeli can't be empty" : GoTo selesai
        End If

        'anilairesidu(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "anilairesidu can't be empty" : GoTo selesai
        End If

        'aumurekonomis(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "aumurekonomis can't be empty" : GoTo selesai
        End If

        'abebanperbln(17) As Double
        If Len(dataUtama(17)) = 0 Then
            result(2) = "abebanperbln can't be empty" : GoTo selesai
        End If

        'aakumulasibeban(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "aakumulasibeban can't be empty" : GoTo selesai
        End If

        'anilaibuku(19) As Double
        If Len(dataUtama(19)) = 0 Then
            result(2) = "anilaibuku can't be empty" : GoTo selesai
        End If

        'arekasset(25) As String
        If Len(dataUtama(25)) = 0 Then
            result(2) = "arekasset can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(25)) > 25 Then
            result(2) = "arekasset should not be more than 25 character." : GoTo selesai
        End If

        'arekakumdepresiasi(26) As String
        If Len(dataUtama(26)) = 0 Then
            result(2) = "arekakumdepresiasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(26)) > 25 Then
            result(2) = "arekakumdepresiasi should not be more than 25 character." : GoTo selesai
        End If

        'arekdepresiasi(27) As String
        If Len(dataUtama(27)) = 0 Then
            result(2) = "arekdepresiasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(27)) > 25 Then
            result(2) = "arekdepresiasi should not be more than 25 character." : GoTo selesai
        End If

        'atglpensiun(30) As Date
        If Len(dataUtama(30)) = 0 Then
            result(2) = "atglpensiun can't be empty" : GoTo selesai
        End If

        'apenyusutanke(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "apenyusutanke can't be empty" : GoTo selesai
        End If

        'anilaimenurun(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "anilaimenurun can't be empty" : GoTo selesai
        End If

        'ainputtgl(41) As DateTime
        If Len(dataUtama(41)) = 0 Then
            result(2) = "ainputtgl can't be empty" : GoTo selesai
        End If

        'amodifikasitgl(43) As DateTime
        If Len(dataUtama(43)) = 0 Then
            result(2) = "amodifikasitgl can't be empty" : GoTo selesai
        End If

        'acustomdbl1(52) As Double
        If Len(dataUtama(52)) = 0 Then
            result(2) = "acustomdbl1 can't be empty" : GoTo selesai
        End If

        'acustomdbl2(53) As Double
        If Len(dataUtama(53)) = 0 Then
            result(2) = "acustomdbl2 can't be empty" : GoTo selesai
        End If

        'acustomdbl3(54) As Double
        If Len(dataUtama(54)) = 0 Then
            result(2) = "acustomdbl3 can't be empty" : GoTo selesai
        End If

        'acustomdate1(55) As Date
        If Len(dataUtama(55)) = 0 Then
            result(2) = "acustomdate1 can't be empty" : GoTo selesai
        End If

        'acustomdate2(56) As Date
        If Len(dataUtama(56)) = 0 Then
            result(2) = "acustomdate2 can't be empty" : GoTo selesai
        End If

        'acustomdate3(57) As Date
        If Len(dataUtama(57)) = 0 Then
            result(2) = "acustomdate3 can't be empty" : GoTo selesai
        End If

        'acostcenter(58) As String
        If Len(dataUtama(58)) > 25 Then
            result(2) = "acostcenter should not be more than 25 character." : GoTo selesai
        End If

        'aproyek(59) As String
        If Len(dataUtama(59)) > 25 Then
            result(2) = "aproyek should not be more than 25 character." : GoTo selesai
        End If

        'ajml(60) As Double
        If Len(dataUtama(60)) = 0 Then
            result(2) = "ajml can't be empty" : GoTo selesai
        End If
        If dataUtama(60) <= 0 Then
            result(2) = "ajml can't be less than or equal to zero" : GoTo selesai
        End If

        'asatuan(61) As String
        If Len(dataUtama(61)) = 0 Then
            result(2) = "asatuan can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 25 Then
            result(2) = "asatuan should not be more than 25 character." : GoTo selesai
        End If

        'aharga(62) As Double
        If Len(dataUtama(62)) = 0 Then
            result(2) = "aharga can't be empty" : GoTo selesai
        End If

        'adiskon(63) As String
        If Len(dataUtama(63)) = 0 Then
            result(2) = "adiskon can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(63)) > 25 Then
            result(2) = "adiskon should not be more than 25 character." : GoTo selesai
        End If

        'ajmldiskon(64) As Double
        If Len(dataUtama(64)) = 0 Then
            result(2) = "ajmldiskon can't be empty" : GoTo selesai
        Else
            'HITUNG JMLDISKON : ajml(60) As Double, aharga(62) As Double, adiskon(63) As String
            dataUtama(64) = F_Diskon(Double.Parse(dataUtama(60)), Double.Parse(dataUtama(62)), FixQuotes(dataUtama(63).ToString))
        End If

        'ajmlpajak1(66) As Double
        If Len(dataUtama(66)) = 0 Then
            result(2) = "ajmlpajak1 can't be empty" : GoTo selesai
        End If

        'ajmlpajak2(68) As Double
        If Len(dataUtama(68)) = 0 Then
            result(2) = "ajmlpajak2 can't be empty" : GoTo selesai
        End If
        'END OF VALIDASI DATA ========================================================


        'SIMPAN KE DATABASE ==========================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0

        Try
            If isUpdate Then
                'JIKA UPDATE CEK JML ROW PADA DATABASE
                dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(aid) FROM M7_Asset WHERE aid = '" & dataUtama(0) & "'")
                rowUpdate = dtupdate.Rows(0)(0)

                If (rowUpdate > 0) Then
                    'SIMPAN HISTORY ========================
                    Dim SimpanHistory As New m7_asset_history
                    Dim assetSimpanHistory As String = SimpanHistory.M7_Asset_HistorySimpan("" & paramSplit(0) & "★M7_Asset_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(dataUtama(0)) & "")
                    Dim assetSplit() As String = assetSimpanHistory.Split(sptParam)
                    Dim assetSplitResult() As String = assetSplit(0).Split(sptSubParam)
                    'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                    If (assetSplitResult(1) = 0) Then
                        result(2) = "Insert history failed : " & assetSplitResult(2) : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF SIMPAN HISTORY ==================

                    sql = "Update M7_Asset set akode  = '" & FixQuotes(dataUtama(1)) & "', anama  = '" & FixQuotes(dataUtama(2)) & "', akategori  = '" & FixQuotes(dataUtama(3)) & "', acabang  = '" & FixQuotes(dataUtama(4)) & "', alokasi  = '" & FixQuotes(dataUtama(5)) & "', adivisi  = '" & FixQuotes(dataUtama(6)) & "', asubdivisi  = '" & FixQuotes(dataUtama(7)) & "', acatatan  = '" & FixQuotes(dataUtama(8)) & "', anomor  = '" & FixQuotes(dataUtama(9)) & "', atglbeli  = '" & FixQuotes(AsFormatTanggal(dataUtama(10))) & "', atglpakai  = '" & FixQuotes(AsFormatTanggal(dataUtama(11))) & "', amatauang  = '" & FixQuotes(dataUtama(12)) & "', akurs  = '" & FixDouble(dataUtama(13)) & "', ahargabeli  = '" & FixDouble(dataUtama(14)) & "', anilairesidu  = '" & FixDouble(dataUtama(15)) & "', aumurekonomis  = '" & FixDouble(dataUtama(16)) & "', abebanperbln  = '" & FixDouble(dataUtama(17)) & "', aakumulasibeban  = '" & FixDouble(dataUtama(18)) & "', anilaibuku  = '" & FixDouble(dataUtama(19)) & "', ametode  = " & dataUtama(20) & ", atabelpenyusutan  = '" & FixQuotes(dataUtama(21)) & "', aintangible  = " & dataUtama(22) & ", afiskal  = " & dataUtama(23) & ", aatastengahbulan  = " & dataUtama(24) & ", arekasset  = '" & FixQuotes(dataUtama(25)) & "', arekakumdepresiasi  = '" & FixQuotes(dataUtama(26)) & "', arekdepresiasi  = '" & FixQuotes(dataUtama(27)) & "', arekpenghapusan  = '" & FixQuotes(dataUtama(28)) & "', aprodusen  = " & dataUtama(29) & ", atglpensiun  = '" & FixQuotes(AsFormatTanggal(dataUtama(30))) & "', apenyusutanke  = '" & FixDouble(dataUtama(31)) & "', anilaimenurun  = '" & FixDouble(dataUtama(32)) & "', adispose  = " & dataUtama(33) & ", apembelian  = " & dataUtama(34) & ", apenjualan  = " & dataUtama(35) & ", alocked  = " & dataUtama(36) & ", astatus  = " & dataUtama(37) & ", astatussebelumnya  = " & dataUtama(38) & ", aisclose  = " & dataUtama(39) & ", amodifikasiuser  = " & dataUtama(42) & ", amodifikasitgl  = NOW(), acustomtext1  = '" & FixQuotes(dataUtama(44)) & "', acustomtext2  = '" & FixQuotes(dataUtama(45)) & "', acustomtext3  = '" & FixQuotes(dataUtama(46)) & "', acustomtext4  = '" & FixQuotes(dataUtama(47)) & "', acustomtext5  = '" & FixQuotes(dataUtama(48)) & "', acustomint1  = " & dataUtama(49) & ", acustomint2  = " & dataUtama(50) & ", acustomint3  = " & dataUtama(51) & ", acustomdbl1  = '" & FixDouble(dataUtama(52)) & "', acustomdbl2  = '" & FixDouble(dataUtama(53)) & "', acustomdbl3  = '" & FixDouble(dataUtama(54)) & "', acustomdate1  = '" & FixQuotes(AsFormatTanggal(dataUtama(55))) & "', acustomdate2  = '" & FixQuotes(AsFormatTanggal(dataUtama(56))) & "', acustomdate3  = '" & FixQuotes(AsFormatTanggal(dataUtama(57))) & "', acostcenter = '" & FixQuotes(dataUtama(58)) & "', aproyek = '" & FixQuotes(dataUtama(59)) & "', ajml = '" & FixDouble(dataUtama(60)) & "', asatuan = '" & FixQuotes(dataUtama(61)) & "', aharga = '" & FixDouble(dataUtama(62)) & "', adiskon = '" & FixQuotes(dataUtama(63)) & "', ajmldiskon = '" & FixDouble(dataUtama(64)) & "', apajak1 = '" & FixQuotes(dataUtama(65)) & "', ajmlpajak1 = '" & FixDouble(dataUtama(66)) & "', apajak2 = '" & FixQuotes(dataUtama(67)) & "', ajmlpajak2 = '" & FixDouble(dataUtama(68)) & "' where aid = '" & dataUtama(0) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Else
                    result(2) = "Data not found." : GoTo selesai
                End If

            Else
                sql = "Insert into M7_Asset (akode, anama, akategori, acabang, alokasi, adivisi, asubdivisi, acatatan, anomor, atglbeli, atglpakai, amatauang, akurs, ahargabeli, anilairesidu, aumurekonomis, abebanperbln, aakumulasibeban, anilaibuku, ametode, atabelpenyusutan, aintangible, afiskal, aatastengahbulan, arekasset, arekakumdepresiasi, arekdepresiasi, arekpenghapusan, aprodusen, atglpensiun, apenyusutanke, anilaimenurun, adispose, apembelian, apenjualan, alocked, astatus, astatussebelumnya, aisclose, ainputuser, ainputtgl, amodifikasiuser, amodifikasitgl, acustomtext1, acustomtext2, acustomtext3, acustomtext4, acustomtext5, acustomint1, acustomint2, acustomint3, acustomdbl1, acustomdbl2, acustomdbl3, acustomdate1, acustomdate2, acustomdate3, acostcenter, aproyek, ajml, asatuan, aharga, adiskon, ajmldiskon, apajak1, ajmlpajak1, apajak2, ajmlpajak2) values('" & FixQuotes(dataUtama(1)) & "', '" & FixQuotes(dataUtama(2)) & "', '" & FixQuotes(dataUtama(3)) & "', '" & FixQuotes(dataUtama(4)) & "', '" & FixQuotes(dataUtama(5)) & "', '" & FixQuotes(dataUtama(6)) & "', '" & FixQuotes(dataUtama(7)) & "', '" & FixQuotes(dataUtama(8)) & "', '" & FixQuotes(dataUtama(9)) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(10))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(11))) & "', '" & FixQuotes(dataUtama(12)) & "', '" & FixDouble(dataUtama(13)) & "', '" & FixDouble(dataUtama(14)) & "', '" & FixDouble(dataUtama(15)) & "', '" & FixDouble(dataUtama(16)) & "', '" & FixDouble(dataUtama(17)) & "', '" & FixDouble(dataUtama(18)) & "', '" & FixDouble(dataUtama(19)) & "', " & dataUtama(20) & ", '" & FixQuotes(dataUtama(21)) & "', " & dataUtama(22) & ", " & dataUtama(23) & ", " & dataUtama(24) & ", '" & FixQuotes(dataUtama(25)) & "', '" & FixQuotes(dataUtama(26)) & "', '" & FixQuotes(dataUtama(27)) & "', '" & FixQuotes(dataUtama(28)) & "', " & dataUtama(29) & ", '" & FixQuotes(AsFormatTanggal(dataUtama(30))) & "', '" & FixDouble(dataUtama(31)) & "', '" & FixDouble(dataUtama(32)) & "', " & dataUtama(33) & ", " & dataUtama(34) & ", " & dataUtama(35) & ", " & dataUtama(36) & ", " & dataUtama(37) & ", " & dataUtama(38) & ", " & dataUtama(39) & ", " & dataUtama(40) & ", NOW(), " & dataUtama(42) & ", '1971-01-01 00:00:00', '" & FixQuotes(dataUtama(44)) & "', '" & FixQuotes(dataUtama(45)) & "', '" & FixQuotes(dataUtama(46)) & "', '" & FixQuotes(dataUtama(47)) & "', '" & FixQuotes(dataUtama(48)) & "', " & dataUtama(49) & ", " & dataUtama(50) & ", " & dataUtama(51) & ", '" & FixDouble(dataUtama(52)) & "', '" & FixDouble(dataUtama(53)) & "', '" & FixDouble(dataUtama(54)) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(55))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(56))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(57))) & "', '" & FixQuotes(dataUtama(58)) & "', '" & FixQuotes(dataUtama(59)) & "', '" & FixDouble(dataUtama(60)) & "', '" & FixQuotes(dataUtama(61)) & "', '" & FixDouble(dataUtama(62)) & "', '" & FixQuotes(dataUtama(63)) & "', '" & FixDouble(dataUtama(64)) & "', '" & FixQuotes(dataUtama(65)) & "', '" & FixDouble(dataUtama(66)) & "', '" & FixQuotes(dataUtama(67)) & "', '" & FixDouble(dataUtama(68)) & "')"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)


            'AMBIL DATA =============================================================
            Dim paramSearch As String = M7_AssetSearch(PostWsSearch(paramSplit(0), "M7_AssetSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
            result(2) = "Transaction Rollback : " & ex.Message
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
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M7_AssetDelete(ByVal param As String) As String

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

        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""
        Dim Filter As String = "", Sorting As String = ""

        Dim pg1 As New RsPaging

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
        If ClsValidKey.ApaBisaAkses(1, 1, 2) = False Then
            result(2) = "Access denied for delete data"
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


        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(5)) = False) Then
            result(2) = "aid required numeric." : GoTo selesai
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

            'CEK TERKAIT =============================================================
            Dim paramTerkait As String = M7_AssetTerkait(PostWsTerkait(paramSplit(0), "M7_AssetTerkait", pagingSplit(0), pagingSplit(1), "", "", formatTgl, formatTglWaktu, idtransaksi))
            Dim hasilTerkait As New RsHasilWsSearch
            hasilTerkait = GetWsSearch(paramTerkait)
            If hasilTerkait.success = 1 Then
                result(2) = "It has related transactions."

                resultPaging(0) = hasilTerkait.isPaging
                resultPaging(1) = hasilTerkait.isNext
                resultPaging(2) = hasilTerkait.isPrevious
                resultPaging(3) = hasilTerkait.countPage
                resultPaging(4) = hasilTerkait.countRow

                search = hasilTerkait.data : Trans.Rollback() : GoTo selesai
            End If
            'END OF CEK TERKAIT ======================================================

            'SIMPAN HISTORY ========================
            Dim SimpanHistory As New m7_asset_history
            Dim assetSimpanHistory As String = SimpanHistory.M7_Asset_HistorySimpan("" & paramSplit(0) & "★M7_Asset_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(idtransaksi) & "")
            Dim assetSplit() As String = assetSimpanHistory.Split(sptParam)
            Dim assetSplitResult() As String = assetSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (assetSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & assetSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            'DELETE
            sql = "DELETE FROM M7_Asset WHERE aid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            Trans.Commit()  '*** Commit Transaction ***'.

            result(1) = 1
            result(2) = ""
            result(3) = 0
            result(4) = idtransaksi

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M7_AssetSearch(PostWsSearch(paramSplit(0), "M7_AssetSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M7_AssetSearchSerenity(ByVal param As String) As String
        'M7_AssetSearch --------------------------------------------------------
        'aid, akode, anama, akategori, acabang, alokasi, adivisi, 
        'asubdivisi, acatatan, anomor, atglbeli, atglpakai, amatauang, akurs, 
        'ahargabeli, anilairesidu, aumurekonomis, abebanperbln, aakumulasibeban, anilaibuku, ametode, 
        'atabelpenyusutan, aintangible, afiskal, aatastengahbulan, arekasset, arekakumdepresiasi, arekdepresiasi, 
        'arekpenghapusan, aprodusen, atglpensiun, apenyusutanke, anilaimenurun, adispose, apembelian, 
        'apenjualan, alocked, astatus, astatussebelumnya, aisclose, ainputuser, ainputtgl, 
        'amodifikasiuser, amodifikasitgl, akategorinama, acabangnama, alokasinama, adivisinama, asubdivisinama, 
        'ametodenama, arekassetnama, arekakumdepresiasinama, arekdepresiasinama, arekpenghapusannama, aprodusenkode, aprodusennama, 
        'astatusnama, astatussebelumnyanama, ainputusernama, amodifikasiusernama, acostcenter, aproyek, ajml, 
        'asatuan, aharga, adiskon, ajmldiskon, apajak1, ajmlpajak1, apajak2, 
        'ajmlpajak2, acostcenternama, aproyeknama, apajak1nama, apajak1nilai, apajak2nama, apajak2nilai, anilaipenyusutan

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

        ''PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m7_asset_v")
        sql &= "SELECT "
        sql &= "a.Aid,"
        sql &= "a.Akode,"
        sql &= "a.Anama,"
        sql &= "a.Akategori,"
        sql &= "a.Adivisi,"
        sql &= "a.Asubdivisi,"
        sql &= "a.Acatatan,"
        sql &= "a.Atglbeli,"
        sql &= "a.Amatauang,"
        sql &= "a.Akurs,"
        sql &= "a.Ahargabeli,"
        sql &= "a.Anilairesidu,"
        sql &= "a.Aumurekonomis,"
        sql &= "a.Abebanperbln,"
        sql &= "a.Aakumulasibeban,"
        sql &= "a.Anilaibuku,"
        sql &= "a.Ametode,"
        sql &= "a.Apenyusutanke,"
        sql &= "a.Astatus,"
        sql &= "a.Astatussebelumnya,"
        sql &= "a.Aisclose,"
        sql &= "ac.acnama As Akategorinama,"
        sql &= "d.dnama AS Adivisinama,"
        sql &= "sd.sdnama AS Asubdivisinama,"
        sql &= "dc.nama As Ametodenama,"
        sql &= "sp1.nama AS Astatusnama,"
        sql &= "a.Acostcenter,"
        sql &= "a.Aproyek,"
        sql &= "cc.ccnama AS Acostcenternama,"
        sql &= "p.pnama AS Aproyeknama,"
        sql &= "(CASE WHEN a.anilaibuku < a.abebanperbln THEN a.anilaibuku ELSE a.abebanperbln END) AS Anilaipenyusutan "
        sql &= "FROM m7_asset a left join m7_asset_category ac on a.akategori = ac.ackode left join m1_branch br on a.acabang = br.bkode left join m1_location l on a.alokasi = l.lkode left join m1_division d on a.adivisi = d.dkode left join m1_subdivision sd on a.asubdivisi = sd.sdkode left join m7_depreciation_category dc on a.ametode = dc.kode left join m1_coa coa1 on a.arekasset = coa1.cnomor left join m1_coa coa2 on a.arekakumdepresiasi = coa2.cnomor left join m1_coa coa3 on a.arekdepresiasi = coa3.cnomor left join m1_coa coa4 on a.arekpenghapusan = coa4.cnomor left join m1_contact c1 on a.aprodusen = c1.kid left join m0_status_progress sp1 on a.astatus = sp1.kode left join m0_status_progress sp2 on a.astatussebelumnya = sp2.kode left join m0_user u1 on a.ainputuser = u1.userid left join m0_user u2 on a.amodifikasiuser = u2.userid left join m1_cost_center cc on a.acostcenter = cc.cckode left join m1_project p on a.aproyek = p.pkode left join m1_tax t1 on a.apajak1 = t1.tkode left join m1_tax t2 on a.apajak2 = t2.tkode "

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M7_Asset_v", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("Aid"), ""), sptField,
                     FxDB(dr("Akode"), ""), sptField,
                     FxDB(dr("Anama"), ""), sptField,
                     FxDB(dr("Akategori"), ""), sptField,
                     FxDB(dr("Adivisi"), ""), sptField,
                     FxDB(dr("Asubdivisi"), ""), sptField,
                     FxDB(dr("Acatatan"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("atglbeli"), ""), formatTgl), sptField,
                     FxDB(dr("Amatauang"), ""), sptField,
                     FxDB(dr("Akurs"), 0), sptField,
                     FxDB(dr("Ahargabeli"), 0), sptField,
                     FxDB(dr("Anilairesidu"), 0), sptField,
                     FxDB(dr("Aumurekonomis"), 0), sptField,
                     FxDB(dr("Abebanperbln"), 0), sptField,
                     FxDB(dr("Aakumulasibeban"), 0), sptField,
                     FxDB(dr("Anilaibuku"), 0), sptField,
                     FxDB(dr("Ametode"), 0), sptField,
                     FxDB(dr("Apenyusutanke"), 0), sptField,
                     FxDB(dr("Astatus"), 0), sptField,
                     FxDB(dr("Astatussebelumnya"), 0), sptField,
                     FxDB(dr("Aisclose"), 0), sptField,
                     FxDB(dr("Akategorinama"), ""), sptField,
                     FxDB(dr("Adivisinama"), ""), sptField,
                     FxDB(dr("Asubdivisinama"), ""), sptField,
                     FxDB(dr("Ametodenama"), ""), sptField,
                     FxDB(dr("Astatusnama"), ""), sptField,
                     FxDB(dr("Acostcenter"), ""), sptField,
                     FxDB(dr("Aproyek"), ""), sptField,
                     FxDB(dr("Acostcenternama"), ""), sptField,
                     FxDB(dr("Aproyeknama"), ""), sptField,
                     FxDB(dr("Anilaipenyusutan"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Asset data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("Aid, Akode, Anama, Akategori, Adivisi, Asubdivisi, Acatatan, Atglbeli, Amatauang, Akurs, Ahargabeli, Anilairesidu, Aumurekonomis, Abebanperbln, Aakumulasibeban, Anilaibuku, Ametode, Apenyusutanke, Astatus, Astatussebelumnya, Aisclose, Akategorinama, Adivisinama, Asubdivisinama, Ametodenama, Astatusnama, Acostcenter, Aproyek, Acostcenternama, Aproyeknama, Anilaipenyusutan"))

        Return wsResult
    End Function

	
    <WebMethod()>
    Public Function M7_AssetSearch(ByVal param As String) As String
        'M7_AssetSearch --------------------------------------------------------
        'aid, akode, anama, akategori, acabang, alokasi, adivisi, 
        'asubdivisi, acatatan, anomor, atglbeli, atglpakai, amatauang, akurs, 
        'ahargabeli, anilairesidu, aumurekonomis, abebanperbln, aakumulasibeban, anilaibuku, ametode, 
        'atabelpenyusutan, aintangible, afiskal, aatastengahbulan, arekasset, arekakumdepresiasi, arekdepresiasi, 
        'arekpenghapusan, aprodusen, atglpensiun, apenyusutanke, anilaimenurun, adispose, apembelian, 
        'apenjualan, alocked, astatus, astatussebelumnya, aisclose, ainputuser, ainputtgl, 
        'amodifikasiuser, amodifikasitgl, akategorinama, acabangnama, alokasinama, adivisinama, asubdivisinama, 
        'ametodenama, arekassetnama, arekakumdepresiasinama, arekdepresiasinama, arekpenghapusannama, aprodusenkode, aprodusennama, 
        'astatusnama, astatussebelumnyanama, ainputusernama, amodifikasiusernama, acostcenter, aproyek, ajml, 
        'asatuan, aharga, adiskon, ajmldiskon, apajak1, ajmlpajak1, apajak2, 
        'ajmlpajak2, acostcenternama, aproyeknama, apajak1nama, apajak1nilai, apajak2nama, apajak2nilai, anilaipenyusutan

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

        ''PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m7_asset_v")
        sql = "select a.aid AS aid, a.akode AS akode, a.anama AS anama, a.akategori AS akategori, a.acabang AS acabang, a.alokasi AS alokasi, a.adivisi AS adivisi, a.asubdivisi AS asubdivisi, a.acatatan AS acatatan, a.anomor AS anomor, a.atglbeli AS atglbeli, a.atglpakai AS atglpakai, a.amatauang AS amatauang, a.akurs AS akurs, a.ahargabeli AS ahargabeli, a.anilairesidu AS anilairesidu, a.aumurekonomis AS aumurekonomis, a.abebanperbln AS abebanperbln, a.aakumulasibeban AS aakumulasibeban, a.anilaibuku AS anilaibuku, (CASE WHEN a.anilaibuku < a.abebanperbln THEN a.anilaibuku ELSE a.abebanperbln END) as anilaipenyusutan, a.ametode AS ametode, a.atabelpenyusutan AS atabelpenyusutan, a.aintangible AS aintangible, a.afiskal AS afiskal, a.aatastengahbulan AS aatastengahbulan, a.arekasset AS arekasset, a.arekakumdepresiasi AS arekakumdepresiasi, a.arekdepresiasi AS arekdepresiasi, a.arekpenghapusan AS arekpenghapusan, a.aprodusen AS aprodusen, a.atglpensiun AS atglpensiun, a.apenyusutanke AS apenyusutanke, a.anilaimenurun AS anilaimenurun, a.adispose AS adispose, a.apembelian AS apembelian, a.apenjualan AS apenjualan, a.alocked AS alocked, a.astatus AS astatus, a.astatussebelumnya AS astatussebelumnya, a.aisclose AS aisclose, a.ainputuser AS ainputuser, a.ainputtgl AS ainputtgl, a.amodifikasiuser AS amodifikasiuser, a.amodifikasitgl AS amodifikasitgl, a.aidbarang AS aidbarang, ac.acnama AS akategorinama, br.bnama AS acabangnama, l.lnama AS alokasinama, d.dnama AS adivisinama, sd.sdnama AS asubdivisinama, dc.nama AS ametodenama, coa1.cnama AS arekassetnama, coa2.cnama AS arekakumdepresiasinama, coa3.cnama AS arekdepresiasinama, coa4.cnama AS arekpenghapusannama, c1.kkode AS aprodusenkode, c1.knama AS aprodusennama, sp1.nama AS astatusnama, sp2.nama AS astatussebelumnyanama, u1.unama AS ainputusernama, u2.unama AS amodifikasiusernama, a.acostcenter AS acostcenter, a.aproyek AS aproyek, a.ajml AS ajml, a.asatuan AS asatuan, a.aharga AS aharga, a.adiskon AS adiskon, a.ajmldiskon AS ajmldiskon, a.apajak1 AS apajak1, a.ajmlpajak1 AS ajmlpajak1, a.apajak2 AS apajak2, a.ajmlpajak2 AS ajmlpajak2, cc.ccnama AS acostcenternama, p.pnama AS aproyeknama, t1.tnama AS apajak1nama, ifnull(t1.tnilai, 0) AS apajak1nilai, t2.tnama AS apajak2nama, ifnull(t2.tnilai, 0) AS apajak2nilai from m7_asset a left join m7_asset_category ac on a.akategori = ac.ackode left join m1_branch br on a.acabang = br.bkode left join m1_location l on a.alokasi = l.lkode left join m1_division d on a.adivisi = d.dkode left join m1_subdivision sd on a.asubdivisi = sd.sdkode left join m7_depreciation_category dc on a.ametode = dc.kode left join m1_coa coa1 on a.arekasset = coa1.cnomor left join m1_coa coa2 on a.arekakumdepresiasi = coa2.cnomor left join m1_coa coa3 on a.arekdepresiasi = coa3.cnomor left join m1_coa coa4 on a.arekpenghapusan = coa4.cnomor left join m1_contact c1 on a.aprodusen = c1.kid left join m0_status_progress sp1 on a.astatus = sp1.kode left join m0_status_progress sp2 on a.astatussebelumnya = sp2.kode left join m0_user u1 on a.ainputuser = u1.userid left join m0_user u2 on a.amodifikasiuser = u2.userid left join m1_cost_center cc on a.acostcenter = cc.cckode left join m1_project p on a.aproyek = p.pkode left join m1_tax t1 on a.apajak1 = t1.tkode left join m1_tax t2 on a.apajak2 = t2.tkode"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M7_Asset_v", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("aid"), ""), sptField,
                     FxDB(dr("akode"), ""), sptField,
                     FxDB(dr("anama"), ""), sptField,
                     FxDB(dr("akategori"), ""), sptField,
                     FxDB(dr("acabang"), ""), sptField,
                     FxDB(dr("alokasi"), ""), sptField,
                     FxDB(dr("adivisi"), ""), sptField,
                     FxDB(dr("asubdivisi"), ""), sptField,
                     FxDB(dr("acatatan"), ""), sptField,
                     FxDB(dr("anomor"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("atglbeli"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("atglpakai"), ""), formatTgl), sptField,
                     FxDB(dr("amatauang"), ""), sptField,
                     FxDB(dr("akurs"), 0), sptField,
                     FxDB(dr("ahargabeli"), 0), sptField,
                     FxDB(dr("anilairesidu"), 0), sptField,
                     FxDB(dr("aumurekonomis"), 0), sptField,
                     FxDB(dr("abebanperbln"), 0), sptField,
                     FxDB(dr("aakumulasibeban"), 0), sptField,
                     FxDB(dr("anilaibuku"), 0), sptField,
                     FxDB(dr("ametode"), 0), sptField,
                     FxDB(dr("atabelpenyusutan"), ""), sptField,
                     FxDB(dr("aintangible"), 0), sptField,
                     FxDB(dr("afiskal"), 0), sptField,
                     FxDB(dr("aatastengahbulan"), 0), sptField,
                     FxDB(dr("arekasset"), ""), sptField,
                     FxDB(dr("arekakumdepresiasi"), ""), sptField,
                     FxDB(dr("arekdepresiasi"), ""), sptField,
                     FxDB(dr("arekpenghapusan"), ""), sptField,
                     FxDB(dr("aprodusen"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("atglpensiun"), ""), formatTgl), sptField,
                     FxDB(dr("apenyusutanke"), 0), sptField,
                     FxDB(dr("anilaimenurun"), 0), sptField,
                     FxDB(dr("adispose"), 0), sptField,
                     FxDB(dr("apembelian"), 0), sptField,
                     FxDB(dr("apenjualan"), 0), sptField,
                     FxDB(dr("alocked"), 0), sptField,
                     FxDB(dr("astatus"), 0), sptField,
                     FxDB(dr("astatussebelumnya"), 0), sptField,
                     FxDB(dr("aisclose"), 0), sptField,
                     FxDB(dr("ainputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ainputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("amodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("amodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("akategorinama"), ""), sptField,
                     FxDB(dr("acabangnama"), ""), sptField,
                     FxDB(dr("alokasinama"), ""), sptField,
                     FxDB(dr("adivisinama"), ""), sptField,
                     FxDB(dr("asubdivisinama"), ""), sptField,
                     FxDB(dr("ametodenama"), ""), sptField,
                     FxDB(dr("arekassetnama"), ""), sptField,
                     FxDB(dr("arekakumdepresiasinama"), ""), sptField,
                     FxDB(dr("arekdepresiasinama"), ""), sptField,
                     FxDB(dr("arekpenghapusannama"), ""), sptField,
                     FxDB(dr("aprodusenkode"), ""), sptField,
                     FxDB(dr("aprodusennama"), ""), sptField,
                     FxDB(dr("astatusnama"), ""), sptField,
                     FxDB(dr("astatussebelumnyanama"), ""), sptField,
                     FxDB(dr("ainputusernama"), ""), sptField,
                     FxDB(dr("amodifikasiusernama"), ""), sptField,
                     FxDB(dr("acostcenter"), ""), sptField,
                     FxDB(dr("aproyek"), ""), sptField,
                     FxDB(dr("ajml"), 0), sptField,
                     FxDB(dr("asatuan"), ""), sptField,
                     FxDB(dr("aharga"), 0), sptField,
                     FxDB(dr("adiskon"), ""), sptField,
                     FxDB(dr("ajmldiskon"), 0), sptField,
                     FxDB(dr("apajak1"), ""), sptField,
                     FxDB(dr("ajmlpajak1"), 0), sptField,
                     FxDB(dr("apajak2"), ""), sptField,
                     FxDB(dr("ajmlpajak2"), 0), sptField,
                     FxDB(dr("acostcenternama"), ""), sptField,
                     FxDB(dr("aproyeknama"), ""), sptField,
                     FxDB(dr("apajak1nama"), ""), sptField,
                     FxDB(dr("apajak1nilai"), 0), sptField,
                     FxDB(dr("apajak2nama"), ""), sptField,
                     FxDB(dr("apajak2nilai"), 0), sptField,
                     FxDB(dr("anilaipenyusutan"), 0), sptField,
                     FxDB(dr("aidbarang"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Asset data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("aid, akode, anama, akategori, acabang, alokasi, adivisi, asubdivisi, acatatan, anomor, atglbeli, atglpakai, amatauang, akurs, ahargabeli, anilairesidu, aumurekonomis, abebanperbln, aakumulasibeban, anilaibuku, ametode, atabelpenyusutan, aintangible, afiskal, aatastengahbulan, arekasset, arekakumdepresiasi, arekdepresiasi, arekpenghapusan, aprodusen, atglpensiun, apenyusutanke, anilaimenurun, adispose, apembelian, apenjualan, alocked, astatus, astatussebelumnya, aisclose, ainputuser, ainputtgl, amodifikasiuser, amodifikasitgl, akategorinama, acabangnama, alokasinama, adivisinama, asubdivisinama, ametodenama, arekassetnama, arekakumdepresiasinama, arekdepresiasinama, arekpenghapusannama, aprodusenkode, aprodusennama, astatusnama, astatussebelumnyanama, ainputusernama, amodifikasiusernama, acostcenter, aproyek, ajml, asatuan, aharga, adiskon, ajmldiskon, apajak1, ajmlpajak1, apajak2, ajmlpajak2, acostcenternama, aproyeknama, apajak1nama, apajak1nilai, apajak2nama, apajak2nilai, anilaipenyusutan, aidbarang"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M7_AssetCekId(ByVal param As String) As String

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim idSplit(1) As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", idtransaksi As String = ""

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
        If ClsValidKey.ApaBisaAkses(1, 1, 2) = False Then
            result(2) = "Access denied for delete data"
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


        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (Len(paramSplit(5)) = 0) Then
            result(2) = "akode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================


        'CEK DI DATABASE ================================================================
        Dim dt As DataTable
        Dim exist As Integer = 0
        dt = AsDataTableAmbilDariDB("SELECT COUNT(akode) FROM M7_Asset WHERE akode = '" & idtransaksi & "'")
        exist = dt.Rows(0)(0)

        If (exist > 0) Then
            result(2) = "'" & idtransaksi & "' already exist for column akode." : GoTo selesai
        End If

        result(1) = 1
        result(2) = ""
        result(3) = 0
        result(4) = idtransaksi
        'END OF CEK DI DATABASE ==========================================================


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
    Public Function M7_AssetTerkait(ByVal param As String) As String
        'M7_AssetTerkait --------------------------------------------------------
        'akode, anama, sumber, idterkait

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
        'CEK IDTRANSAKSI
        Dim idtransaksi As String = ""
        If (IsNumeric(paramSplit(5)) = False) Then
            result(2) = "aid required numeric." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m7_asset_terkait")
        sql = sql.Replace("valkode", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("akode"), ""), sptField,
                             FxDB(dr("anama"), ""), sptField,
                             FxDB(dr("sumber"), ""), sptField,
                             FxDB(dr("idterkait"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Related Asset data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("akode, anama, sumber, idterkait"))

        Return wsResult
    End Function

End Class