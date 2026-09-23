Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_ap
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_ApSimpan(ByVal param As String) As String
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
        'apid(0) As Integer, apcabang(1) As String, aplokasi(2) As String, apjenis(3) As Integer, apsumber(4) As String, 
        'apautonotransaksi(5) As Integer, apnotransaksi(6) As String, aptgl(7) As Date, apkodepa(8) As Integer, apkontak(9) As Integer, 
        'apkontakperson(10) As String, ap1alamat1(11) As String, ap1alamat2(12) As String, ap1alamat3(13) As String, ap2alamat1(14) As String, 
        'ap2alamat2(15) As String, ap2alamat3(16) As String, apbagianpembayaran(17) As Integer, aptermin(18) As String, aptgljatuhtempo(19) As Date, 
        'apidpo(20) As Integer, apnorek(21) As String, apuraian(22) As String, apcatatan(23) As String, apnoref(24) As String, 
        'aptglnoref(25) As Date, apmatauang(26) As String, apkurs(27) As Double, apjumlah(28) As Double, apjumlahvalas(29) As Double, 
        'apjumlahbayar(30) As Double, apjumlahbayarvalas(31) As Double, apstatusbayar(32) As Integer, aptgllunas(33) As Date, apcostcenter(34) As String, 
        'apdivisi(35) As String, apsubdivisi(36) As String, approyek(37) As String, apstatus(38) As Integer, apstatussebelumnya(39) As Integer, 
        'apjmlrevisi(40) As Integer, apcetakanke(41) As Integer, apinputuser(42) As Integer, apinputtgl(43) As DateTime, apmodifikasiuser(44) As Integer, 
        'apmodifikasitgl(45) As DateTime, apposting(46) As Integer, apisclose(47) As Integer, apcustomtext1(48) As String, apcustomtext2(49) As String, 
        'apcustomtext3(50) As String, apcustomtext4(51) As String, apcustomtext5(52) As String, apcustomint1(53) As Integer, apcustomint2(54) As Integer, 
        'apcustomint3(55) As Integer, apcustomdbl1(56) As Double, apcustomdbl2(57) As Double, apcustomdbl3(58) As Double, apcustomdate1(59) As Date, 
        'apcustomdate2(60) As Date, apcustomdate3(61) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'apid, apcabang, aplokasi, apjenis, apsumber, apautonotransaksi, apnotransaksi, 
        'aptgl, apkodepa, apkontak, apkontakperson, ap1alamat1, ap1alamat2, ap1alamat3, 
        'ap2alamat1, ap2alamat2, ap2alamat3, apbagianpembayaran, aptermin, aptgljatuhtempo, apidpo, 
        'apnorek, apuraian, apcatatan, apnoref, aptglnoref, apmatauang, apkurs, 
        'apjumlah, apjumlahvalas, apjumlahbayar, apjumlahbayarvalas, apstatusbayar, aptgllunas, apcostcenter, 
        'apdivisi, apsubdivisi, approyek, apstatus, apstatussebelumnya, apjmlrevisi, apcetakanke, 
        'apinputuser, apinputtgl, apmodifikasiuser, apmodifikasitgl, apposting, apisclose, apcustomtext1, 
        'apcustomtext2, apcustomtext3, apcustomtext4, apcustomtext5, apcustomint1, apcustomint2, apcustomint3, 
        'apcustomdbl1, apcustomdbl2, apcustomdbl3, apcustomdate1, apcustomdate2, apcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 62) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'apid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "apid required numeric." : GoTo selesai
        End If
        'apjenis(3) As Integer
        If (IsNumeric(dataUtama(3)) = False) Then
            result(2) = "apjenis required numeric." : GoTo selesai
        End If
        'apautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "apautonotransaksi required numeric." : GoTo selesai
        End If
        'aptgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "aptgl required date." : GoTo selesai
        End If
        'apkodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "apkodepa required numeric." : GoTo selesai
        End If
        'apkontak(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "apkontak required numeric." : GoTo selesai
        End If
        If (dataUtama(9) < 1) Then
            result(2) = "apkontak can't be empty." : GoTo selesai
        End If
        'apbagianpembayaran(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "apbagianpembayaran required numeric." : GoTo selesai
        End If
        'aptgljatuhtempo(19) As Date
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "aptgljatuhtempo required date." : GoTo selesai
        End If
        'apidpo(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "apidpo required numeric." : GoTo selesai
        End If
        'aptglnoref(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "aptglnoref required date." : GoTo selesai
        End If
        'apkurs(27) As Double
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "apkurs required numeric." : GoTo selesai
        End If
        'apjumlah(28) As Double
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "apjumlah required numeric." : GoTo selesai
        End If
        'apjumlahvalas(29) As Double
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "apjumlahvalas required numeric." : GoTo selesai
        End If
        'apjumlahbayar(30) As Double
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "apjumlahbayar required numeric." : GoTo selesai
        End If
        'apjumlahbayarvalas(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "apjumlahbayarvalas required numeric." : GoTo selesai
        End If
        'apstatusbayar(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "apstatusbayar required numeric." : GoTo selesai
        End If
        'aptgllunas(33) As Date
        If (IsDate(dataUtama(33)) = False) Then
            result(2) = "aptgllunas required date." : GoTo selesai
        End If
        'apstatus(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "apstatus required numeric." : GoTo selesai
        End If
        'apstatussebelumnya(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "apstatussebelumnya required numeric." : GoTo selesai
        End If
        'apjmlrevisi(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "apjmlrevisi required numeric." : GoTo selesai
        End If
        'apcetakanke(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "apcetakanke required numeric." : GoTo selesai
        End If
        'apinputuser(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "apinputuser required numeric." : GoTo selesai
        End If
        'apinputtgl(43) As DateTime
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "apinputtgl required date." : GoTo selesai
        End If
        'apmodifikasiuser(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "apmodifikasiuser required numeric." : GoTo selesai
        End If
        'apmodifikasitgl(45) As DateTime
        If (IsDate(dataUtama(45)) = False) Then
            result(2) = "apmodifikasitgl required date." : GoTo selesai
        End If
        'apposting(46) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "apposting required numeric." : GoTo selesai
        End If
        'apisclose(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "apisclose required numeric." : GoTo selesai
        End If
        'apcustomint1(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "apcustomint1 required numeric." : GoTo selesai
        End If
        'apcustomint2(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "apcustomint2 required numeric." : GoTo selesai
        End If
        'apcustomint3(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "apcustomint3 required numeric." : GoTo selesai
        End If
        'apcustomdbl1(56) As Double
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "apcustomdbl1 required numeric." : GoTo selesai
        End If
        'apcustomdbl2(57) As Double
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "apcustomdbl2 required numeric." : GoTo selesai
        End If
        'apcustomdbl3(58) As Double
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "apcustomdbl3 required numeric." : GoTo selesai
        End If
        'apcustomdate1(59) As Date
        If (IsDate(dataUtama(59)) = False) Then
            result(2) = "apcustomdate1 required date." : GoTo selesai
        End If
        'apcustomdate2(60) As Date
        If (IsDate(dataUtama(60)) = False) Then
            result(2) = "apcustomdate2 required date." : GoTo selesai
        End If
        'apcustomdate3(61) As Date
        If (IsDate(dataUtama(61)) = False) Then
            result(2) = "apcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'apcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "apcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "apcabang should not be more than 25 character." : GoTo selesai
        End If

        'aplokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "aplokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "aplokasi should not be more than 25 character." : GoTo selesai
        End If

        'apsumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "apsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "apsumber should not be more than 10 character." : GoTo selesai
        End If

        'apnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "apnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "apnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'aptgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "aptgl can't be empty" : GoTo selesai
        End If

        'aptgljatuhtempo(19) As Date
        If Len(dataUtama(19)) = 0 Then
            result(2) = "aptgljatuhtempo can't be empty" : GoTo selesai
        End If

        'apnorek(21) As String
        If Len(dataUtama(21)) = 0 Then
            result(2) = "apnorek can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(21)) > 25 Then
            result(2) = "apnorek should not be more than 25 character." : GoTo selesai
        End If

        'aptglnoref(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "aptglnoref can't be empty" : GoTo selesai
        End If

        'apmatauang(26) As String
        If Len(dataUtama(26)) = 0 Then
            result(2) = "apmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(26)) > 25 Then
            result(2) = "apmatauang should not be more than 25 character." : GoTo selesai
        End If

        'apkurs(27) As Double
        If Len(dataUtama(27)) = 0 Then
            result(2) = "apkurs can't be empty" : GoTo selesai
        End If

        'apjumlah(28) As Double
        If Len(dataUtama(28)) = 0 Then
            result(2) = "apjumlah can't be empty" : GoTo selesai
        End If

        'apjumlahvalas(29) As Double
        If Len(dataUtama(29)) = 0 Then
            result(2) = "apjumlahvalas can't be empty" : GoTo selesai
        End If

        'apjumlahbayar(30) As Double
        If Len(dataUtama(30)) = 0 Then
            result(2) = "apjumlahbayar can't be empty" : GoTo selesai
        End If

        'apjumlahbayarvalas(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "apjumlahbayarvalas can't be empty" : GoTo selesai
        End If

        'aptgllunas(33) As Date
        If Len(dataUtama(33)) = 0 Then
            result(2) = "aptgllunas can't be empty" : GoTo selesai
        End If

        'apinputtgl(43) As DateTime
        If Len(dataUtama(43)) = 0 Then
            result(2) = "apinputtgl can't be empty" : GoTo selesai
        End If

        'apmodifikasitgl(45) As DateTime
        If Len(dataUtama(45)) = 0 Then
            result(2) = "apmodifikasitgl can't be empty" : GoTo selesai
        End If

        'apcustomdbl1(56) As Double
        If Len(dataUtama(56)) = 0 Then
            result(2) = "apcustomdbl1 can't be empty" : GoTo selesai
        End If

        'apcustomdbl2(57) As Double
        If Len(dataUtama(57)) = 0 Then
            result(2) = "apcustomdbl2 can't be empty" : GoTo selesai
        End If

        'apcustomdbl3(58) As Double
        If Len(dataUtama(58)) = 0 Then
            result(2) = "apcustomdbl3 can't be empty" : GoTo selesai
        End If

        'apcustomdate1(59) As Date
        If Len(dataUtama(59)) = 0 Then
            result(2) = "apcustomdate1 can't be empty" : GoTo selesai
        End If

        'apcustomdate2(60) As Date
        If Len(dataUtama(60)) = 0 Then
            result(2) = "apcustomdate2 can't be empty" : GoTo selesai
        End If

        'apcustomdate3(61) As Date
        If Len(dataUtama(61)) = 0 Then
            result(2) = "apcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "apid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aplokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apjenis", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aptgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ap1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ap1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ap1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ap2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ap2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ap2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apbagianpembayaran", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aptermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aptgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apidpo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apnorek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aptglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apjumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "apjumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "apjumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apjumlahbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apstatusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aptgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcostcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apsubdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "approyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "apid~apcabang~aplokasi~apjenis~apsumber~apautonotransaksi~apnotransaksi~aptgl~apkodepa~apkontak~apkontakperson~ap1alamat1~ap1alamat2~ap1alamat3~ap2alamat1~ap2alamat2~ap2alamat3~apbagianpembayaran~aptermin~aptgljatuhtempo~apidpo~apnorek~apuraian~apcatatan~apnoref~aptglnoref~apmatauang~apkurs~apjumlah~apjumlahvalas~apjumlahbayar~apjumlahbayarvalas~apstatusbayar~aptgllunas~apcostcenter~apdivisi~apsubdivisi~approyek~apstatus~apstatussebelumnya~apjmlrevisi~apcetakanke~apinputuser~apinputtgl~apmodifikasiuser~apmodifikasitgl~apposting~apisclose~apcustomtext1~apcustomtext2~apcustomtext3~apcustomtext4~apcustomtext5~apcustomint1~apcustomint2~apcustomint3~apcustomdbl1~apcustomdbl2~apcustomdbl3~apcustomdate1~apcustomdate2~apcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idapcarabayar(0) As Integer, idap(1) As Integer, carabayar(2) As Integer, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, nogiro(7) As String, tgljt(8) As Date, bank(9) As String, 
        'noacbank(10) As String, rekbank(11) As String, rekgiro(12) As String, catatan(13) As String, urutan(14) As Integer, 
        'isclose(15) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idapcarabayar, idap, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, isclose

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idapcarabayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idap", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "carabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "nogiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgljt", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "bank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "noacbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekgiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt64)

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 16) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idapcarabayar(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idapcarabayar required numeric." : GoTo selesai
            End If
            'idap(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idap required numeric." : GoTo selesai
            End If
            'carabayar(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - carabayar required numeric." : GoTo selesai
            End If
            'kurs(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'jumlah(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - jumlah required numeric." : GoTo selesai
            End If
            'jumlahvalas(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - jumlahvalas required numeric." : GoTo selesai
            End If
            'tgljt(8) As Date
            If (IsDate(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - tgljt required date." : GoTo selesai
            End If
            'urutan(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'isclose(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'matauang(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 25 Then
                result(2) = "Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'jumlah(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - jumlah can't be empty" : GoTo selesai
            End If
            'If dataRowDetail(5) <= 0 Then
            '    result(2) = "Row : " & i & " - jumlah must be more than zero" : GoTo selesai
            'End If

            'jumlahvalas(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - jumlahvalas can't be empty" : GoTo selesai
            End If

            'tgljt(8) As Date
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - tgljt can't be empty" : GoTo selesai
            End If

            'rekbank(11) As String
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - rekbank can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(11)) > 25 Then
                result(2) = "Row : " & i & " - rekbank should not be more than 25 character." : GoTo selesai
            End If

            'JIKA CARABAYAR = GIRO, MAKA KOLOM DATA GIRO WAJIB DIISI
            If dataRowDetail(2) = 2 Then
                'nogiro(7) As String
                If Len(dataRowDetail(7)) = 0 Then
                    result(2) = "Row : " & i & " - nogiro can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(7)) > 25 Then
                    result(2) = "Row : " & i & " - nogiro should not be more than 25 character." : GoTo selesai
                End If

                'bank(9) As String
                If Len(dataRowDetail(9)) = 0 Then
                    result(2) = "Row : " & i & " - bank can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(9)) > 25 Then
                    result(2) = "Row : " & i & " - bank should not be more than 25 character." : GoTo selesai
                End If

                'noacbank(10) As String
                If Len(dataRowDetail(10)) = 0 Then
                    result(2) = "Row : " & i & " - noacbank can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(10)) > 50 Then
                    result(2) = "Row : " & i & " - noacbank should not be more than 50 character." : GoTo selesai
                End If

                'rekgiro(12) As String
                If Len(dataRowDetail(12)) = 0 Then
                    result(2) = "Row : " & i & " - rekgiro can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(12)) > 25 Then
                    result(2) = "Row : " & i & " - rekgiro should not be more than 25 character." : GoTo selesai
                End If
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idapcarabayar~idap~carabayar~matauang~kurs~jumlah~jumlahvalas~nogiro~tgljt~bank~noacbank~rekbank~rekgiro~catatan~urutan~isclose", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

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
                Dim vModuleId As Integer = 4, vMenuId As Integer = 8
                Select Case drutama("apstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("aptgl")), AsFormatTanggal(drutama("aptgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "apmatauang", "apnorek", dtdetail, "rekbank")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("aptermin").ToString, AsFormatTanggal(drutama("aptgl")), "aptgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("aptgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                'HITUNG TOTAL BERDASARKAN DATA DETAIL ===================
                drutama("apjumlah") = AsDataTableDSum(dtdetail, "jumlah")
                drutama("apjumlahvalas") = AsDataTableDSum(dtdetail, "jumlahvalas")
                'END OF HITUNG TOTAL BERDASARKAN DATA DETAIL ============


                ''CEK TOTAL UTAMA DAN DETAIL =============================
                'Dim jumlah As Double = AsDataTableDSum(dtdetail, "jumlah")
                'Dim jumlahvalas As Double = AsDataTableDSum(dtdetail, "jumlahvalas")
                'If Double.Parse(drutama("apjumlah")) <> jumlah Then
                '    result(2) = "Total amount of main and detail are not balanced" : Trans.Rollback() : GoTo selesai
                'ElseIf Double.Parse(drutama("apjumlahvalas")) <> jumlahvalas Then
                '    result(2) = "Total amount of foreign main and detail are not balanced" : Trans.Rollback() : GoTo selesai
                'End If
                ''END OF CEK TOTAL UTAMA DAN DETAIL ======================

                If isUpdate Then
                    result(4) = drutama("apid")
                    notransaksi = drutama("apnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(apid), apnotransaksi FROM M4_ap WHERE apid='" & result(4) & "' AND apstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("apautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("apcabang"), drutama("aplokasi"), drutama("apsumber"), drutama("aptgl"), drutama("apsumber"), 4)
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(apid) FROM m4_ap WHERE apnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_ap_history
                        Dim rsSimpanHistory As String = SimpanHistory.M4_Ap_HistorySimpan("" & paramSplit(0) & "★M4_Ap_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("apsumber")) & "▼" & FixQuotes(drutama("apid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M4_Ap set apcabang  = '" & FixQuotes(drutama("apcabang")) & "', aplokasi  = '" & FixQuotes(drutama("aplokasi")) & "', apjenis  = " & drutama("apjenis") & ", apsumber  = '" & FixQuotes(drutama("apsumber")) & "', apautonotransaksi  = " & drutama("apautonotransaksi") & ", apnotransaksi  = '" & notransaksi & "', aptgl  = '" & FixQuotes(AsFormatTanggal(drutama("aptgl"))) & "', apkodepa  = " & drutama("apkodepa") & ", apkontak  = " & drutama("apkontak") & ", apkontakperson  = '" & FixQuotes(drutama("apkontakperson")) & "', ap1alamat1  = '" & FixQuotes(drutama("ap1alamat1")) & "', ap1alamat2  = '" & FixQuotes(drutama("ap1alamat2")) & "', ap1alamat3  = '" & FixQuotes(drutama("ap1alamat3")) & "', ap2alamat1  = '" & FixQuotes(drutama("ap2alamat1")) & "', ap2alamat2  = '" & FixQuotes(drutama("ap2alamat2")) & "', ap2alamat3  = '" & FixQuotes(drutama("ap2alamat3")) & "', apbagianpembayaran  = " & drutama("apbagianpembayaran") & ", aptermin  = '" & FixQuotes(drutama("aptermin")) & "', aptgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("aptgljatuhtempo"))) & "', apidpo  = " & drutama("apidpo") & ", apnorek  = '" & FixQuotes(drutama("apnorek")) & "', apuraian  = '" & FixQuotes(drutama("apuraian")) & "', apcatatan  = '" & FixQuotes(drutama("apcatatan")) & "', apnoref  = '" & FixQuotes(drutama("apnoref")) & "', aptglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("aptglnoref"))) & "', apmatauang  = '" & FixQuotes(drutama("apmatauang")) & "', apkurs  = '" & FixDouble(drutama("apkurs")) & "', apjumlah  = '" & FixDouble(drutama("apjumlah")) & "', apjumlahvalas  = '" & FixDouble(drutama("apjumlahvalas")) & "', apjumlahbayar  = '" & FixDouble(drutama("apjumlahbayar")) & "', apjumlahbayarvalas  = '" & FixDouble(drutama("apjumlahbayarvalas")) & "', apstatusbayar  = " & drutama("apstatusbayar") & ", aptgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("aptgllunas"))) & "', apcostcenter  = '" & FixQuotes(drutama("apcostcenter")) & "', apdivisi  = '" & FixQuotes(drutama("apdivisi")) & "', apsubdivisi  = '" & FixQuotes(drutama("apsubdivisi")) & "', approyek  = '" & FixQuotes(drutama("approyek")) & "', apstatus  = " & drutama("apstatus") & ", apstatussebelumnya  = " & drutama("apstatussebelumnya") & ", apjmlrevisi  = apjmlrevisi+1, apcetakanke  = " & drutama("apcetakanke") & ", apmodifikasiuser  = " & drutama("apmodifikasiuser") & ", apmodifikasitgl  = NOW(), apposting  = 0, apcustomtext1  = '" & FixQuotes(drutama("apcustomtext1")) & "', apcustomtext2  = '" & FixQuotes(drutama("apcustomtext2")) & "', apcustomtext3  = '" & FixQuotes(drutama("apcustomtext3")) & "', apcustomtext4  = '" & FixQuotes(drutama("apcustomtext4")) & "', apcustomtext5  = '" & FixQuotes(drutama("apcustomtext5")) & "', apcustomint1  = " & drutama("apcustomint1") & ", apcustomint2  = " & drutama("apcustomint2") & ", apcustomint3  = " & drutama("apcustomint3") & ", apcustomdbl1  = '" & FixDouble(drutama("apcustomdbl1")) & "', apcustomdbl2  = '" & FixDouble(drutama("apcustomdbl2")) & "', apcustomdbl3  = '" & FixDouble(drutama("apcustomdbl3")) & "', apcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("apcustomdate1"))) & "', apcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("apcustomdate2"))) & "', apcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("apcustomdate3"))) & "' where apid = '" & drutama("apid") & "'"
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

                    If drutama("apautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("apcabang"), drutama("aplokasi"), drutama("apsumber"), drutama("aptgl"), drutama("apsumber"), 4)
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
                        notransaksi = drutama("apnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(apid) FROM m4_ap WHERE apnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Ap (apcabang, aplokasi, apjenis, apsumber, apautonotransaksi, apnotransaksi, aptgl, apkodepa, apkontak, apkontakperson, ap1alamat1, ap1alamat2, ap1alamat3, ap2alamat1, ap2alamat2, ap2alamat3, apbagianpembayaran, aptermin, aptgljatuhtempo, apidpo, apnorek, apuraian, apcatatan, apnoref, aptglnoref, apmatauang, apkurs, apjumlah, apjumlahvalas, apjumlahbayar, apjumlahbayarvalas, apstatusbayar, aptgllunas, apcostcenter, apdivisi, apsubdivisi, approyek, apstatus, apstatussebelumnya, apjmlrevisi, apcetakanke, apinputuser, apinputtgl, apmodifikasiuser, apmodifikasitgl, apposting, apisclose, apcustomtext1, apcustomtext2, apcustomtext3, apcustomtext4, apcustomtext5, apcustomint1, apcustomint2, apcustomint3, apcustomdbl1, apcustomdbl2, apcustomdbl3, apcustomdate1, apcustomdate2, apcustomdate3) values('" & FixQuotes(drutama("apcabang")) & "', '" & FixQuotes(drutama("aplokasi")) & "', " & drutama("apjenis") & ", '" & FixQuotes(drutama("apsumber")) & "', " & drutama("apautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("aptgl"))) & "', " & drutama("apkodepa") & ", " & drutama("apkontak") & ", '" & FixQuotes(drutama("apkontakperson")) & "', '" & FixQuotes(drutama("ap1alamat1")) & "', '" & FixQuotes(drutama("ap1alamat2")) & "', '" & FixQuotes(drutama("ap1alamat3")) & "', '" & FixQuotes(drutama("ap2alamat1")) & "', '" & FixQuotes(drutama("ap2alamat2")) & "', '" & FixQuotes(drutama("ap2alamat3")) & "', " & drutama("apbagianpembayaran") & ", '" & FixQuotes(drutama("aptermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("aptgljatuhtempo"))) & "', " & drutama("apidpo") & ", '" & FixQuotes(drutama("apnorek")) & "', '" & FixQuotes(drutama("apuraian")) & "', '" & FixQuotes(drutama("apcatatan")) & "', '" & FixQuotes(drutama("apnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("aptglnoref"))) & "', '" & FixQuotes(drutama("apmatauang")) & "', '" & FixDouble(drutama("apkurs")) & "', '" & FixDouble(drutama("apjumlah")) & "', '" & FixDouble(drutama("apjumlahvalas")) & "', '" & FixDouble(drutama("apjumlahbayar")) & "', '" & FixDouble(drutama("apjumlahbayarvalas")) & "', " & drutama("apstatusbayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("aptgllunas"))) & "', '" & FixQuotes(drutama("apcostcenter")) & "', '" & FixQuotes(drutama("apdivisi")) & "', '" & FixQuotes(drutama("apsubdivisi")) & "', '" & FixQuotes(drutama("approyek")) & "', " & drutama("apstatus") & ", " & drutama("apstatussebelumnya") & ", " & drutama("apjmlrevisi") & ", " & drutama("apcetakanke") & ", " & drutama("apinputuser") & ", NOW(), " & drutama("apmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("apisclose") & ", '" & FixQuotes(drutama("apcustomtext1")) & "', '" & FixQuotes(drutama("apcustomtext2")) & "', '" & FixQuotes(drutama("apcustomtext3")) & "', '" & FixQuotes(drutama("apcustomtext4")) & "', '" & FixQuotes(drutama("apcustomtext5")) & "', " & drutama("apcustomint1") & ", " & drutama("apcustomint2") & ", " & drutama("apcustomint3") & ", '" & FixDouble(drutama("apcustomdbl1")) & "', '" & FixDouble(drutama("apcustomdbl2")) & "', '" & FixDouble(drutama("apcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("apcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("apcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("apcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select apid from M4_ap where apnotransaksi='" & notransaksi & "' AND apinputuser= '" & userid & "' order by apmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Ap_Pay where idap = '" & result(4) & "'"
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
                    Dim strValue2 As New StringBuilder, strGiro As New StringBuilder
                    Dim rsCekGiro As String

                    For Each dr1 As DataRow In dtdetail.Rows

                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idapcarabayar") & ", " & result(4) & ", " & dr1("carabayar") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ")")

                        'QUERY UNTUK INSERT GIRO
                        If dr1("carabayar") = 2 Then

                            'CEK HAK AKSES APPROVED GIRO KELUAR =====================
                            If drutama("apstatus") = 2 Then
                                rsCekGiro = HakAksesGiro(4, 8, userid) 'MODULEID, MENUID, USERID SESUAI TRANSAKSI
                                If Len(rsCekGiro) <> 0 Then result(2) = rsCekGiro : Trans.Rollback() : GoTo selesai
                            End If
                            'END OF CEK HAK AKSES APPROVED GIRO KELUAR ==============

                            strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", ", "))
                            strGiro.Append("('" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(drutama("apsumber")) & "', " & result(4) & ", '" & FixQuotes(notransaksi) & "', " & drutama("apkontak") & ", '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', " & 1 & ", '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', " & 0 & ", " & 0 & ", " & dr1("urutan") & ")")
                        End If
                    Next
                    sql = "Insert into M4_Ap_Pay(idapcarabayar, idap, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'INSERT GIRO JIKA STATUS APPROVED DAN CARABAYAR = 2
                    If drutama("apstatus") = 2 And Len(strGiro.ToString) > 0 Then
                        sql = "Insert into M2_Giro_List(glnogiro, glsumber, glidtransaksi, glnotransaksi, glkontak, glrekbank, glrekgiro, gljenis, glbank, glnoacbank, glmatauang, glkurs, gljumlah, gljumlahvalas, gltgljthtempo, gltglcair, glstatus, glstatussebelumnya, glurutan) values" & strGiro.ToString & ""
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
                    result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "AP", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("apstatus") = 2 Then
                    Dim Security As New ClsSecurity, mjid As String = "", hasilMsmq As String = ""
                    'BUAT ID UNIQUE
                    mjid = Security.MD5CalcString(userid & sumber & result(4) & Now) 'RandomId.Generate(15)

                    'MSMQ TABEL
                    sql = "Insert into M0_Msmq_Journal(mjid, mjsumber, mjidtransaksi, mjprogress, mjpesan, mjtglantrian, mjtglselesai, mjuserid) values ('" _
                        & mjid & "', '" & sumber & "', '" & result(4) & "', '" & 0 & "', " & "''" & ", NOW(), '1971-01-01 00:00:00', '" & userid & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'MSMQ ANTRIAN
                    Dim PostingJurnal As String = F_getSetting(0, "accounting", "AutoPosting")
                    If PostingJurnal.Equals("0") = False Then
                        hasilMsmq = SendMsmq(dirMsmq, "J", mjid, sumber, result(4), userid)
                        If Len(hasilMsmq) > 0 Then
                            result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
                        End If
                    End If

                End If
                'END OF INSERT MSMQ JURNAL ==========================================================

                'INSERT USER LOG ====================================================================
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
    Public Function M4_ApUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("apkontakkode", "c1.kkode")
            Filter = Filter.Replace("apkontaknama", "c1.knama")
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
            Dim sumber As String = "Ap", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Aptgl, Apnotransaksi, Apstatus FROM m4_Ap WHERE Apid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Apstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m4_ap_history
            Dim rsSimpanHistory As String = SimpanHistory.M4_Ap_HistorySimpan("" & paramSplit(0) & "★M4_Ap_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m4_ap_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                ''CEK STATUS GIRO
                'dtdetail = AsDataTableAmbilDariDBCon("SELECT glnogiro FROM m2_giro_list WHERE glsumber = 'AP' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "' AND glstatus <> 0")
                'If dtdetail.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai

                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'AP' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'DELETE GIRO
                sql = "DELETE FROM m2_giro_list WHERE glsumber = 'AP' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'update status utama
            sql = "UPDATE m4_Ap SET Apstatus = " & nilaiStatus & ", Apmodifikasiuser='" & userid & "', Apmodifikasitgl = NOW(), Apposting = 0, Appostingtgl = '1971-01-01 00:00:00', Apjmlrevisi = Apjmlrevisi + 1 WHERE Apid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_ApSearch(PostWsSearch(paramSplit(0), "M4_ApSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_ApDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("apkontakkode", "c1.kkode")
            Filter = Filter.Replace("apkontaknama", "c1.knama")
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
            Dim sumber As String = "Ap", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Apid, Apnotransaksi FROM M4_Ap WHERE Apid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT apcabang, aplokasi, apsumber, apautonotransaksi, apnotransaksi, aptgl"
            sql &= " FROM M4_ap"
            sql &= " WHERE apid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("apcabang")
                lokasi = dtNomorNext.Rows(0)("aplokasi")
                sumber = dtNomorNext.Rows(0)("apsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("apautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("apnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("aptgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M4_Ap_Pay WHERE idap = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M4_Ap WHERE apid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_ApSearch(PostWsSearch(paramSplit(0), "M4_ApSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_ApGetdataById(ByVal param As String) As String
        'M4_ApGetdataById Utama --------------------------------------------------------
        'apid, apcabang, aplokasi, apjenis, apsumber, apautonotransaksi, apnotransaksi, 
        'aptgl, apkodepa, apkontak, apkontakperson, ap1alamat1, ap1alamat2, ap1alamat3, 
        'ap2alamat1, ap2alamat2, ap2alamat3, apbagianpembayaran, aptermin, aptgljatuhtempo, apidpo, 
        'apnorek, apuraian, apcatatan, apnoref, aptglnoref, apmatauang, apkurs, 
        'apjumlah, apjumlahvalas, apjumlahbayar, apjumlahbayarvalas, apstatusbayar, aptgllunas, apcostcenter, 
        'apdivisi, apsubdivisi, approyek, apstatus, apstatussebelumnya, apjmlrevisi, apcetakanke, 
        'apinputuser, apinputtgl, apmodifikasiuser, apmodifikasitgl, apposting, appostingtgl, apisclose, 
        'apcustomtext1, apcustomtext2, apcustomtext3, apcustomtext4, apcustomtext5, apcustomint1, apcustomint2, 
        'apcustomint3, apcustomdbl1, apcustomdbl2, apcustomdbl3, apcustomdate1, apcustomdate2, apcustomdate3, 
        'apcabangnama, aplokasinama, apkontakkode, apkontaknama, apbagianpembayarankode, apbagianpembayarannama, apterminnama, 
        'apterminharijatuhtempo, ponotransaksi, apnoreknama, apcostcenternama, apdivisinama, apsubdivisinama, approyeknama, 
        'apstatusnama, apstatussebelumnyanama, apinputusernama, apmodifikasiusernama, kpkp

        'M4_ApGetdataById Pay -------------------------------------------------------
        'idapcarabayar, idap, carabayar, matauang, 
        'kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, 
        'rekbank, rekgiro, catatan, urutan, isclose, carabayarnama, banknama, 
        'rekbanknama, rekgironama

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

        Dim NmMemcached As String = "aplikasi1-M4_Ap~M4_Ap_Pay-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "apid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "apid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_ap_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("apid"), 0), sptField,
                     FxDB(drutama("apcabang"), ""), sptField,
                     FxDB(drutama("aplokasi"), ""), sptField,
                     FxDB(drutama("apjenis"), 0), sptField,
                     FxDB(drutama("apsumber"), ""), sptField,
                     FxDB(drutama("apautonotransaksi"), 0), sptField,
                     FxDB(drutama("apnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aptgl"), ""), formatTgl), sptField,
                     FxDB(drutama("apkodepa"), 0), sptField,
                     FxDB(drutama("apkontak"), 0), sptField,
                     FxDB(drutama("apkontakperson"), ""), sptField,
                     FxDB(drutama("ap1alamat1"), ""), sptField,
                     FxDB(drutama("ap1alamat2"), ""), sptField,
                     FxDB(drutama("ap1alamat3"), ""), sptField,
                     FxDB(drutama("ap2alamat1"), ""), sptField,
                     FxDB(drutama("ap2alamat2"), ""), sptField,
                     FxDB(drutama("ap2alamat3"), ""), sptField,
                     FxDB(drutama("apbagianpembayaran"), 0), sptField,
                     FxDB(drutama("aptermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aptgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("apidpo"), 0), sptField,
                     FxDB(drutama("apnorek"), ""), sptField,
                     FxDB(drutama("apuraian"), ""), sptField,
                     FxDB(drutama("apcatatan"), ""), sptField,
                     FxDB(drutama("apnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aptglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("apmatauang"), ""), sptField,
                     FxDB(drutama("apkurs"), 0), sptField,
                     FxDB(drutama("apjumlah"), 0), sptField,
                     FxDB(drutama("apjumlahvalas"), 0), sptField,
                     FxDB(drutama("apjumlahbayar"), 0), sptField,
                     FxDB(drutama("apjumlahbayarvalas"), 0), sptField,
                     FxDB(drutama("apstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("aptgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("apcostcenter"), ""), sptField,
                     FxDB(drutama("apdivisi"), ""), sptField,
                     FxDB(drutama("apsubdivisi"), ""), sptField,
                     FxDB(drutama("approyek"), ""), sptField,
                     FxDB(drutama("apstatus"), 0), sptField,
                     FxDB(drutama("apstatussebelumnya"), 0), sptField,
                     FxDB(drutama("apjmlrevisi"), 0), sptField,
                     FxDB(drutama("apcetakanke"), 0), sptField,
                     FxDB(drutama("apinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("apinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("apmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("apmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("apposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("appostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("apisclose"), 0), sptField,
                     FxDB(drutama("apcustomtext1"), ""), sptField,
                     FxDB(drutama("apcustomtext2"), ""), sptField,
                     FxDB(drutama("apcustomtext3"), ""), sptField,
                     FxDB(drutama("apcustomtext4"), ""), sptField,
                     FxDB(drutama("apcustomtext5"), ""), sptField,
                     FxDB(drutama("apcustomint1"), 0), sptField,
                     FxDB(drutama("apcustomint2"), 0), sptField,
                     FxDB(drutama("apcustomint3"), 0), sptField,
                     FxDB(drutama("apcustomdbl1"), 0), sptField,
                     FxDB(drutama("apcustomdbl2"), 0), sptField,
                     FxDB(drutama("apcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("apcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("apcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("apcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("apcabangnama"), ""), sptField,
                     FxDB(drutama("aplokasinama"), ""), sptField,
                     FxDB(drutama("apkontakkode"), ""), sptField,
                     FxDB(drutama("apkontaknama"), ""), sptField,
                     FxDB(drutama("apbagianpembayarankode"), ""), sptField,
                     FxDB(drutama("apbagianpembayarannama"), ""), sptField,
                     FxDB(drutama("apterminnama"), ""), sptField,
                     FxDB(drutama("apterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("ponotransaksi"), ""), sptField,
                     FxDB(drutama("apnoreknama"), ""), sptField,
                     FxDB(drutama("apcostcenternama"), ""), sptField,
                     FxDB(drutama("apdivisinama"), ""), sptField,
                     FxDB(drutama("apsubdivisinama"), ""), sptField,
                     FxDB(drutama("approyeknama"), ""), sptField,
                     FxDB(drutama("apstatusnama"), ""), sptField,
                     FxDB(drutama("apstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("apinputusernama"), ""), sptField,
                     FxDB(drutama("apmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idapcarabayar"), 0), sptField,
                     FxDB(dr("idap"), 0), sptField,
                     FxDB(dr("carabayar"), 0), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("jumlah"), 0), sptField,
                     FxDB(dr("jumlahvalas"), 0), sptField,
                     FxDB(dr("nogiro"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgljt"), ""), formatTgl), sptField,
                     FxDB(dr("bank"), ""), sptField,
                     FxDB(dr("noacbank"), ""), sptField,
                     FxDB(dr("rekbank"), ""), sptField,
                     FxDB(dr("rekgiro"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("carabayarnama"), ""), sptField,
                     FxDB(dr("banknama"), ""), sptField,
                     FxDB(dr("rekbanknama"), ""), sptField,
                     FxDB(dr("rekgironama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("apid, apcabang, aplokasi, apjenis, apsumber, apautonotransaksi, apnotransaksi, aptgl, apkodepa, apkontak, apkontakperson, ap1alamat1, ap1alamat2, ap1alamat3, ap2alamat1, ap2alamat2, ap2alamat3, apbagianpembayaran, aptermin, aptgljatuhtempo, apidpo, apnorek, apuraian, apcatatan, apnoref, aptglnoref, apmatauang, apkurs, apjumlah, apjumlahvalas, apjumlahbayar, apjumlahbayarvalas, apstatusbayar, aptgllunas, apcostcenter, apdivisi, apsubdivisi, approyek, apstatus, apstatussebelumnya, apjmlrevisi, apcetakanke, apinputuser, apinputtgl, apmodifikasiuser, apmodifikasitgl, apposting, appostingtgl, apisclose, apcustomtext1, apcustomtext2, apcustomtext3, apcustomtext4, apcustomtext5, apcustomint1, apcustomint2, apcustomint3, apcustomdbl1, apcustomdbl2, apcustomdbl3, apcustomdate1, apcustomdate2, apcustomdate3, apcabangnama, aplokasinama, apkontakkode, apkontaknama, apbagianpembayarankode, apbagianpembayarannama, apterminnama, apterminharijatuhtempo, ponotransaksi, apnoreknama, apcostcenternama, apdivisinama, apsubdivisinama, approyeknama, apstatusnama, apstatussebelumnyanama, apinputusernama, apmodifikasiusernama, kpkp"), sptSubParam, ReplaceMapping("idapcarabayar, idap, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose, carabayarnama, banknama, rekbanknama, rekgironama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_ApSearch(ByVal param As String) As String
        'M4_ApSearch --------------------------------------------------------
        'apid, apcabang, aplokasi, apjenis, apsumber, apautonotransaksi, apnotransaksi, 
        'aptgl, apkodepa, apkontak, apkontakperson, ap1alamat1, ap1alamat2, ap1alamat3, 
        'ap2alamat1, ap2alamat2, ap2alamat3, apbagianpembayaran, aptermin, aptgljatuhtempo, apidpo, 
        'apnorek, apuraian, apcatatan, apnoref, aptglnoref, apmatauang, apkurs, 
        'apjumlah, apjumlahvalas, apjumlahbayar, apjumlahbayarvalas, apstatusbayar, aptgllunas, apcostcenter, 
        'apdivisi, apsubdivisi, approyek, apstatus, apstatussebelumnya, apjmlrevisi, apcetakanke, 
        'apinputuser, apinputtgl, apmodifikasiuser, apmodifikasitgl, apposting, appostingtgl, apisclose, 
        'apcabangnama, aplokasinama, apjenisnama, apkontakkode, apkontaknama, apbagianpembayarankode, apbagianpembayarannama, 
        'ponotransaksi, apnoreknama, apstatusnama, apstatussebelumnyanama, apinputusernama, apmodifikasiusernama

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
            Filter = Filter.Replace("apkontakkode", "c1.kkode")
            Filter = Filter.Replace("apkontaknama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_ap_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Ap", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("apid"), 0), sptField,
                     FxDB(dr("apcabang"), ""), sptField,
                     FxDB(dr("aplokasi"), ""), sptField,
                     FxDB(dr("apjenis"), 0), sptField,
                     FxDB(dr("apsumber"), ""), sptField,
                     FxDB(dr("apautonotransaksi"), 0), sptField,
                     FxDB(dr("apnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aptgl"), ""), formatTgl), sptField,
                     FxDB(dr("apkodepa"), 0), sptField,
                     FxDB(dr("apkontak"), 0), sptField,
                     FxDB(dr("apkontakperson"), ""), sptField,
                     FxDB(dr("ap1alamat1"), ""), sptField,
                     FxDB(dr("ap1alamat2"), ""), sptField,
                     FxDB(dr("ap1alamat3"), ""), sptField,
                     FxDB(dr("ap2alamat1"), ""), sptField,
                     FxDB(dr("ap2alamat2"), ""), sptField,
                     FxDB(dr("ap2alamat3"), ""), sptField,
                     FxDB(dr("apbagianpembayaran"), 0), sptField,
                     FxDB(dr("aptermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aptgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("apidpo"), 0), sptField,
                     FxDB(dr("apnorek"), ""), sptField,
                     FxDB(dr("apuraian"), ""), sptField,
                     FxDB(dr("apcatatan"), ""), sptField,
                     FxDB(dr("apnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aptglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("apmatauang"), ""), sptField,
                     FxDB(dr("apkurs"), 0), sptField,
                     FxDB(dr("apjumlah"), 0), sptField,
                     FxDB(dr("apjumlahvalas"), 0), sptField,
                     FxDB(dr("apjumlahbayar"), 0), sptField,
                     FxDB(dr("apjumlahbayarvalas"), 0), sptField,
                     FxDB(dr("apstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("aptgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("apcostcenter"), ""), sptField,
                     FxDB(dr("apdivisi"), ""), sptField,
                     FxDB(dr("apsubdivisi"), ""), sptField,
                     FxDB(dr("approyek"), ""), sptField,
                     FxDB(dr("apstatus"), 0), sptField,
                     FxDB(dr("apstatussebelumnya"), 0), sptField,
                     FxDB(dr("apjmlrevisi"), 0), sptField,
                     FxDB(dr("apcetakanke"), 0), sptField,
                     FxDB(dr("apinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("apinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("apmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("apmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("apposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("appostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("apisclose"), 0), sptField,
                     FxDB(dr("apcabangnama"), ""), sptField,
                     FxDB(dr("aplokasinama"), ""), sptField,
                     FxDB(dr("apjenisnama"), ""), sptField,
                     FxDB(dr("apkontakkode"), ""), sptField,
                     FxDB(dr("apkontaknama"), ""), sptField,
                     FxDB(dr("apbagianpembayarankode"), ""), sptField,
                     FxDB(dr("apbagianpembayarannama"), ""), sptField,
                     FxDB(dr("ponotransaksi"), ""), sptField,
                     FxDB(dr("apnoreknama"), ""), sptField,
                     FxDB(dr("apstatusnama"), ""), sptField,
                     FxDB(dr("apstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("apinputusernama"), ""), sptField,
                     FxDB(dr("apmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("apid, apcabang, aplokasi, apjenis, apsumber, apautonotransaksi, apnotransaksi, aptgl, apkodepa, apkontak, apkontakperson, ap1alamat1, ap1alamat2, ap1alamat3, ap2alamat1, ap2alamat2, ap2alamat3, apbagianpembayaran, aptermin, aptgljatuhtempo, apidpo, apnorek, apuraian, apcatatan, apnoref, aptglnoref, apmatauang, apkurs, apjumlah, apjumlahvalas, apjumlahbayar, apjumlahbayarvalas, apstatusbayar, aptgllunas, apcostcenter, apdivisi, apsubdivisi, approyek, apstatus, apstatussebelumnya, apjmlrevisi, apcetakanke, apinputuser, apinputtgl, apmodifikasiuser, apmodifikasitgl, apposting, appostingtgl, apisclose, apcabangnama, aplokasinama, apjenisnama, apkontakkode, apkontaknama, apbagianpembayarankode, apbagianpembayarannama, ponotransaksi, apnoreknama, apstatusnama, apstatussebelumnyanama, apinputusernama, apmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_ApTerkait(ByVal param As String) As String
        'M4_ApTerkait --------------------------------------------------------
        'apid, apnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "apid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m4_ap_terkait")
        sql = "select `ap`.`apid` AS `apid`,`ap`.`apnotransaksi` AS `apnotransaksi`,`po`.`posumber` AS `sumber`,`po`.`poid` AS `idterkait`,`po`.`ponotransaksi` AS `noterkait`,`po`.`potgl` AS `tglterkait`,`po`.`poinputtgl` AS `inputtglterkait`,`po`.`pomodifikasitgl` AS `modifikasitglterkait`,0 AS `jenisterkait` from (`m4_ap` `ap` join `m4_po` `po` on((`ap`.`apidpo` = `po`.`poid`))) where (`ap`.`apid` = 'validtransaksi') group by `po`.`poid`,`ap`.`apid` union all select `ap`.`apid` AS `apid`,`ap`.`apnotransaksi` AS `apnotransaksi`,`vpp`.`vppsumber` AS `sumber`,`vpp`.`vppid` AS `idterkait`,`vpp`.`vppnotransaksi` AS `noterkait`,`vpp`.`vpptgl` AS `tglterkait`,`vpp`.`vppinputtgl` AS `inputtglterkait`,`vpp`.`vppmodifikasitgl` AS `modifikasitglterkait`,1 AS `jenisterkait` from ((`m4_vpp_detail` `vppd` join `m4_vpp` `vpp` on((`vppd`.`idvpp` = `vpp`.`vppid`))) join `m4_ap` `ap` on((`vppd`.`idtransaksi` = `ap`.`apid`))) where ((`vppd`.`sumber` = 'AP') and ((`vpp`.`vppstatus` = 2) or (`vpp`.`vppstatus` = 3) or (`vpp`.`vppstatus` = 4) or (`vpp`.`vppstatus` = 7)) and (`ap`.`apid` = 'validtransaksi')) group by `vpp`.`vppid`,`ap`.`apid` union all select `ap`.`apid` AS `apid`,`ap`.`apnotransaksi` AS `apnotransaksi`,`vp`.`vpsumber` AS `sumber`,`vp`.`vpid` AS `idterkait`,`vp`.`vpnotransaksi` AS `noterkait`,`vp`.`vptgl` AS `tglterkait`,`vp`.`vpinputtgl` AS `inputtglterkait`,`vp`.`vpmodifikasitgl` AS `modifikasitglterkait`,1 AS `jenisterkait` from ((`m4_vp_detail` `vpd` join `m4_vp` `vp` on((`vpd`.`idvp` = `vp`.`vpid`))) join `m4_ap` `ap` on((`vpd`.`idtransaksi` = `ap`.`apid`))) where ((`vpd`.`sumber` = 'AP') and ((`vp`.`vpstatus` = 2) or (`vp`.`vpstatus` = 3) or (`vp`.`vpstatus` = 4) or (`vp`.`vpstatus` = 7)) and (`ap`.`apid` = 'validtransaksi')) group by `vp`.`vpid`,`ap`.`apid` union all select `ap`.`apid` AS `apid`,`ap`.`apnotransaksi` AS `apnotransaksi`,`sg`.`sgsumber` AS `sumber`,`sg`.`sgid` AS `idterkait`,`sg`.`sgnotransaksi` AS `noterkait`,`sg`.`sgtgl` AS `tglterkait`,`sg`.`sginputtgl` AS `inputtglterkait`,`sg`.`sgmodifikasitgl` AS `modifikasitglterkait`,1 AS `jenisterkait` from (((`m4_ap` `ap` join `m2_giro_list` `gl` on((`ap`.`apnotransaksi` = `gl`.`glnotransaksi`))) join `m2_sg_detail` `sgd` on((`gl`.`glnogiro` = `sgd`.`nogiro`))) join `m2_sg` `sg` on((`sgd`.`idsg` = `sg`.`sgid`))) where (((`sg`.`sgstatus` = 2) or (`sg`.`sgstatus` = 3) or (`sg`.`sgstatus` = 4) or (`sg`.`sgstatus` = 7)) and (`ap`.`apid` = 'validtransaksi')) group by `sg`.`sgid`,`ap`.`apid` union all select `ap`.`apid` AS `apid`,`ap`.`apnotransaksi` AS `apnotransaksi`,`sgc`.`sgcsumber` AS `sumber`,`sgc`.`sgcid` AS `idterkait`,`sgc`.`sgcnotransaksi` AS `noterkait`,`sgc`.`sgctgl` AS `tglterkait`,`sgc`.`sgcinputtgl` AS `inputtglterkait`,`sgc`.`sgcmodifikasitgl` AS `modifikasitglterkait`,1 AS `jenisterkait` from (((`m4_ap` `ap` join `m2_giro_list` `gl` on((`ap`.`apnotransaksi` = `gl`.`glnotransaksi`))) join `m2_sgc_detail` `sgcd` on((`gl`.`glnogiro` = `sgcd`.`nogiro`))) join `m2_sgc` `sgc` on((`sgcd`.`idsgc` = `sgc`.`sgcid`))) where (((`sgc`.`sgcstatus` = 2) or (`sgc`.`sgcstatus` = 3) or (`sgc`.`sgcstatus` = 4) or (`sgc`.`sgcstatus` = 7)) and (`ap`.`apid` = 'validtransaksi')) group by `sgc`.`sgcid`,`ap`.`apid` union all select `ap`.`apid` AS `apid`, `ap`.`apnotransaksi` AS `apnotransaksi`, `ri`.`risumber` AS `sumber`, `ri`.`riid` AS `idterkait`, `ri`.`rinotransaksi` AS `noterkait`, `ri`.`ritgl` AS `tglterkait`, `ri`.`riinputtgl` AS `inputtglterkait`, `ri`.`rimodifikasitgl` AS `modifikasitglterkait`, 1 AS `jenisterkait` from `m4_ri` `ri` join `m4_ap` `ap` on `ri`.`riidap` = `ap`.`apid` where ((`ri`.`ristatus` = 2) or (`ri`.`ristatus` = 3) or (`ri`.`ristatus` = 4) or (`ri`.`ristatus` = 7)) and (`ap`.`apid` = 'validtransaksi') group by `ri`.`riid`,`ap`.`apid` "
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("apid"), 0), sptField,
                     FxDB(dr("apnotransaksi"), ""), sptField,
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
            result(2) = "Related AP data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("apid, apnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_ApSimpanOld(ByVal param As String) As String
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
        'apid(0) As Integer, apcabang(1) As String, aplokasi(2) As String, apjenis(3) As Integer, apsumber(4) As String, 
        'apautonotransaksi(5) As Integer, apnotransaksi(6) As String, aptgl(7) As Date, apkodepa(8) As Integer, apkontak(9) As Integer, 
        'apkontakperson(10) As String, ap1alamat1(11) As String, ap1alamat2(12) As String, ap1alamat3(13) As String, ap2alamat1(14) As String, 
        'ap2alamat2(15) As String, ap2alamat3(16) As String, apbagianpembayaran(17) As Integer, aptermin(18) As String, aptgljatuhtempo(19) As Date, 
        'apidpo(20) As Integer, apnorek(21) As String, apuraian(22) As String, apcatatan(23) As String, apnoref(24) As String, 
        'aptglnoref(25) As Date, apmatauang(26) As String, apkurs(27) As Double, apjumlah(28) As Double, apjumlahvalas(29) As Double, 
        'apjumlahbayar(30) As Double, apjumlahbayarvalas(31) As Double, apstatusbayar(32) As Integer, aptgllunas(33) As Date, apcostcenter(34) As String, 
        'apdivisi(35) As String, apsubdivisi(36) As String, approyek(37) As String, apstatus(38) As Integer, apstatussebelumnya(39) As Integer, 
        'apjmlrevisi(40) As Integer, apcetakanke(41) As Integer, apinputuser(42) As Integer, apinputtgl(43) As DateTime, apmodifikasiuser(44) As Integer, 
        'apmodifikasitgl(45) As DateTime, apposting(46) As Integer, apisclose(47) As Integer, apcustomtext1(48) As String, apcustomtext2(49) As String, 
        'apcustomtext3(50) As String, apcustomtext4(51) As String, apcustomtext5(52) As String, apcustomint1(53) As Integer, apcustomint2(54) As Integer, 
        'apcustomint3(55) As Integer, apcustomdbl1(56) As Double, apcustomdbl2(57) As Double, apcustomdbl3(58) As Double, apcustomdate1(59) As Date, 
        'apcustomdate2(60) As Date, apcustomdate3(61) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'apid, apcabang, aplokasi, apjenis, apsumber, apautonotransaksi, apnotransaksi, 
        'aptgl, apkodepa, apkontak, apkontakperson, ap1alamat1, ap1alamat2, ap1alamat3, 
        'ap2alamat1, ap2alamat2, ap2alamat3, apbagianpembayaran, aptermin, aptgljatuhtempo, apidpo, 
        'apnorek, apuraian, apcatatan, apnoref, aptglnoref, apmatauang, apkurs, 
        'apjumlah, apjumlahvalas, apjumlahbayar, apjumlahbayarvalas, apstatusbayar, aptgllunas, apcostcenter, 
        'apdivisi, apsubdivisi, approyek, apstatus, apstatussebelumnya, apjmlrevisi, apcetakanke, 
        'apinputuser, apinputtgl, apmodifikasiuser, apmodifikasitgl, apposting, apisclose, apcustomtext1, 
        'apcustomtext2, apcustomtext3, apcustomtext4, apcustomtext5, apcustomint1, apcustomint2, apcustomint3, 
        'apcustomdbl1, apcustomdbl2, apcustomdbl3, apcustomdate1, apcustomdate2, apcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 62) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'apid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "apid required numeric." : GoTo selesai
        End If
        'apjenis(3) As Integer
        If (IsNumeric(dataUtama(3)) = False) Then
            result(2) = "apjenis required numeric." : GoTo selesai
        End If
        'apautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "apautonotransaksi required numeric." : GoTo selesai
        End If
        'aptgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "aptgl required date." : GoTo selesai
        End If
        'apkodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "apkodepa required numeric." : GoTo selesai
        End If
        'apkontak(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "apkontak required numeric." : GoTo selesai
        End If
        If (dataUtama(9) < 1) Then
            result(2) = "apkontak can't be empty." : GoTo selesai
        End If
        'apbagianpembayaran(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "apbagianpembayaran required numeric." : GoTo selesai
        End If
        'aptgljatuhtempo(19) As Date
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "aptgljatuhtempo required date." : GoTo selesai
        End If
        'apidpo(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "apidpo required numeric." : GoTo selesai
        End If
        'aptglnoref(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "aptglnoref required date." : GoTo selesai
        End If
        'apkurs(27) As Double
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "apkurs required numeric." : GoTo selesai
        End If
        'apjumlah(28) As Double
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "apjumlah required numeric." : GoTo selesai
        End If
        'apjumlahvalas(29) As Double
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "apjumlahvalas required numeric." : GoTo selesai
        End If
        'apjumlahbayar(30) As Double
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "apjumlahbayar required numeric." : GoTo selesai
        End If
        'apjumlahbayarvalas(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "apjumlahbayarvalas required numeric." : GoTo selesai
        End If
        'apstatusbayar(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "apstatusbayar required numeric." : GoTo selesai
        End If
        'aptgllunas(33) As Date
        If (IsDate(dataUtama(33)) = False) Then
            result(2) = "aptgllunas required date." : GoTo selesai
        End If
        'apstatus(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "apstatus required numeric." : GoTo selesai
        End If
        'apstatussebelumnya(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "apstatussebelumnya required numeric." : GoTo selesai
        End If
        'apjmlrevisi(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "apjmlrevisi required numeric." : GoTo selesai
        End If
        'apcetakanke(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "apcetakanke required numeric." : GoTo selesai
        End If
        'apinputuser(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "apinputuser required numeric." : GoTo selesai
        End If
        'apinputtgl(43) As DateTime
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "apinputtgl required date." : GoTo selesai
        End If
        'apmodifikasiuser(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "apmodifikasiuser required numeric." : GoTo selesai
        End If
        'apmodifikasitgl(45) As DateTime
        If (IsDate(dataUtama(45)) = False) Then
            result(2) = "apmodifikasitgl required date." : GoTo selesai
        End If
        'apposting(46) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "apposting required numeric." : GoTo selesai
        End If
        'apisclose(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "apisclose required numeric." : GoTo selesai
        End If
        'apcustomint1(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "apcustomint1 required numeric." : GoTo selesai
        End If
        'apcustomint2(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "apcustomint2 required numeric." : GoTo selesai
        End If
        'apcustomint3(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "apcustomint3 required numeric." : GoTo selesai
        End If
        'apcustomdbl1(56) As Double
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "apcustomdbl1 required numeric." : GoTo selesai
        End If
        'apcustomdbl2(57) As Double
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "apcustomdbl2 required numeric." : GoTo selesai
        End If
        'apcustomdbl3(58) As Double
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "apcustomdbl3 required numeric." : GoTo selesai
        End If
        'apcustomdate1(59) As Date
        If (IsDate(dataUtama(59)) = False) Then
            result(2) = "apcustomdate1 required date." : GoTo selesai
        End If
        'apcustomdate2(60) As Date
        If (IsDate(dataUtama(60)) = False) Then
            result(2) = "apcustomdate2 required date." : GoTo selesai
        End If
        'apcustomdate3(61) As Date
        If (IsDate(dataUtama(61)) = False) Then
            result(2) = "apcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'apcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "apcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "apcabang should not be more than 25 character." : GoTo selesai
        End If

        'aplokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "aplokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "aplokasi should not be more than 25 character." : GoTo selesai
        End If

        'apsumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "apsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "apsumber should not be more than 10 character." : GoTo selesai
        End If

        'apnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "apnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "apnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'aptgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "aptgl can't be empty" : GoTo selesai
        End If

        'aptgljatuhtempo(19) As Date
        If Len(dataUtama(19)) = 0 Then
            result(2) = "aptgljatuhtempo can't be empty" : GoTo selesai
        End If

        'apnorek(21) As String
        If Len(dataUtama(21)) = 0 Then
            result(2) = "apnorek can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(21)) > 25 Then
            result(2) = "apnorek should not be more than 25 character." : GoTo selesai
        End If

        'aptglnoref(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "aptglnoref can't be empty" : GoTo selesai
        End If

        'apmatauang(26) As String
        If Len(dataUtama(26)) = 0 Then
            result(2) = "apmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(26)) > 25 Then
            result(2) = "apmatauang should not be more than 25 character." : GoTo selesai
        End If

        'apkurs(27) As Double
        If Len(dataUtama(27)) = 0 Then
            result(2) = "apkurs can't be empty" : GoTo selesai
        End If

        'apjumlah(28) As Double
        If Len(dataUtama(28)) = 0 Then
            result(2) = "apjumlah can't be empty" : GoTo selesai
        End If

        'apjumlahvalas(29) As Double
        If Len(dataUtama(29)) = 0 Then
            result(2) = "apjumlahvalas can't be empty" : GoTo selesai
        End If

        'apjumlahbayar(30) As Double
        If Len(dataUtama(30)) = 0 Then
            result(2) = "apjumlahbayar can't be empty" : GoTo selesai
        End If

        'apjumlahbayarvalas(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "apjumlahbayarvalas can't be empty" : GoTo selesai
        End If

        'aptgllunas(33) As Date
        If Len(dataUtama(33)) = 0 Then
            result(2) = "aptgllunas can't be empty" : GoTo selesai
        End If

        'apinputtgl(43) As DateTime
        If Len(dataUtama(43)) = 0 Then
            result(2) = "apinputtgl can't be empty" : GoTo selesai
        End If

        'apmodifikasitgl(45) As DateTime
        If Len(dataUtama(45)) = 0 Then
            result(2) = "apmodifikasitgl can't be empty" : GoTo selesai
        End If

        'apcustomdbl1(56) As Double
        If Len(dataUtama(56)) = 0 Then
            result(2) = "apcustomdbl1 can't be empty" : GoTo selesai
        End If

        'apcustomdbl2(57) As Double
        If Len(dataUtama(57)) = 0 Then
            result(2) = "apcustomdbl2 can't be empty" : GoTo selesai
        End If

        'apcustomdbl3(58) As Double
        If Len(dataUtama(58)) = 0 Then
            result(2) = "apcustomdbl3 can't be empty" : GoTo selesai
        End If

        'apcustomdate1(59) As Date
        If Len(dataUtama(59)) = 0 Then
            result(2) = "apcustomdate1 can't be empty" : GoTo selesai
        End If

        'apcustomdate2(60) As Date
        If Len(dataUtama(60)) = 0 Then
            result(2) = "apcustomdate2 can't be empty" : GoTo selesai
        End If

        'apcustomdate3(61) As Date
        If Len(dataUtama(61)) = 0 Then
            result(2) = "apcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "apid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aplokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apjenis", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aptgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ap1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ap1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ap1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ap2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ap2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ap2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apbagianpembayaran", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aptermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aptgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apidpo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apnorek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aptglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apjumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "apjumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "apjumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apjumlahbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apstatusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aptgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcostcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apsubdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "approyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "apcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "apcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "apid~apcabang~aplokasi~apjenis~apsumber~apautonotransaksi~apnotransaksi~aptgl~apkodepa~apkontak~apkontakperson~ap1alamat1~ap1alamat2~ap1alamat3~ap2alamat1~ap2alamat2~ap2alamat3~apbagianpembayaran~aptermin~aptgljatuhtempo~apidpo~apnorek~apuraian~apcatatan~apnoref~aptglnoref~apmatauang~apkurs~apjumlah~apjumlahvalas~apjumlahbayar~apjumlahbayarvalas~apstatusbayar~aptgllunas~apcostcenter~apdivisi~apsubdivisi~approyek~apstatus~apstatussebelumnya~apjmlrevisi~apcetakanke~apinputuser~apinputtgl~apmodifikasiuser~apmodifikasitgl~apposting~apisclose~apcustomtext1~apcustomtext2~apcustomtext3~apcustomtext4~apcustomtext5~apcustomint1~apcustomint2~apcustomint3~apcustomdbl1~apcustomdbl2~apcustomdbl3~apcustomdate1~apcustomdate2~apcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idapcarabayar(0) As Integer, idap(1) As Integer, carabayar(2) As Integer, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, nogiro(7) As String, tgljt(8) As Date, bank(9) As String, 
        'noacbank(10) As String, rekbank(11) As String, rekgiro(12) As String, catatan(13) As String, urutan(14) As Integer, 
        'isclose(15) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idapcarabayar, idap, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, isclose

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idapcarabayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idap", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "carabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "nogiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgljt", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "bank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "noacbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekgiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt64)

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 16) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idapcarabayar(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idapcarabayar required numeric." : GoTo selesai
            End If
            'idap(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idap required numeric." : GoTo selesai
            End If
            'carabayar(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - carabayar required numeric." : GoTo selesai
            End If
            'kurs(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'jumlah(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - jumlah required numeric." : GoTo selesai
            End If
            'jumlahvalas(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - jumlahvalas required numeric." : GoTo selesai
            End If
            'tgljt(8) As Date
            If (IsDate(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - tgljt required date." : GoTo selesai
            End If
            'urutan(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'isclose(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'matauang(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 25 Then
                result(2) = "Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'jumlah(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - jumlah can't be empty" : GoTo selesai
            End If
            If dataRowDetail(5) <= 0 Then
                result(2) = "Row : " & i & " - jumlah must be more than zero" : GoTo selesai
            End If

            'jumlahvalas(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - jumlahvalas can't be empty" : GoTo selesai
            End If

            'tgljt(8) As Date
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - tgljt can't be empty" : GoTo selesai
            End If

            'rekbank(11) As String
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - rekbank can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(11)) > 25 Then
                result(2) = "Row : " & i & " - rekbank should not be more than 25 character." : GoTo selesai
            End If

            'JIKA CARABAYAR = GIRO, MAKA KOLOM DATA GIRO WAJIB DIISI
            If dataRowDetail(2) = 2 Then
                'nogiro(7) As String
                If Len(dataRowDetail(7)) = 0 Then
                    result(2) = "Row : " & i & " - nogiro can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(7)) > 25 Then
                    result(2) = "Row : " & i & " - nogiro should not be more than 25 character." : GoTo selesai
                End If

                'bank(9) As String
                If Len(dataRowDetail(9)) = 0 Then
                    result(2) = "Row : " & i & " - bank can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(9)) > 25 Then
                    result(2) = "Row : " & i & " - bank should not be more than 25 character." : GoTo selesai
                End If

                'noacbank(10) As String
                If Len(dataRowDetail(10)) = 0 Then
                    result(2) = "Row : " & i & " - noacbank can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(10)) > 50 Then
                    result(2) = "Row : " & i & " - noacbank should not be more than 50 character." : GoTo selesai
                End If

                'rekgiro(12) As String
                If Len(dataRowDetail(12)) = 0 Then
                    result(2) = "Row : " & i & " - rekgiro can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(12)) > 25 Then
                    result(2) = "Row : " & i & " - rekgiro should not be more than 25 character." : GoTo selesai
                End If
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idapcarabayar~idap~carabayar~matauang~kurs~jumlah~jumlahvalas~nogiro~tgljt~bank~noacbank~rekbank~rekgiro~catatan~urutan~isclose", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15)) = False Then
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

                'CEK PERIODE AKUNTANSI ==================================
                Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("aptgl")), AsFormatTanggal(drutama("aptgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "apmatauang", "apnorek", dtdetail, "rekbank")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("aptermin").ToString, AsFormatTanggal(drutama("aptgl")), "aptgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("aptgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                'HITUNG TOTAL BERDASARKAN DATA DETAIL ===================
                drutama("apjumlah") = AsDataTableDSum(dtdetail, "jumlah")
                drutama("apjumlahvalas") = AsDataTableDSum(dtdetail, "jumlahvalas")
                'END OF HITUNG TOTAL BERDASARKAN DATA DETAIL ============


                ''CEK TOTAL UTAMA DAN DETAIL =============================
                'Dim jumlah As Double = AsDataTableDSum(dtdetail, "jumlah")
                'Dim jumlahvalas As Double = AsDataTableDSum(dtdetail, "jumlahvalas")
                'If Double.Parse(drutama("apjumlah")) <> jumlah Then
                '    result(2) = "Total amount of main and detail are not balanced" : Trans.Rollback() : GoTo selesai
                'ElseIf Double.Parse(drutama("apjumlahvalas")) <> jumlahvalas Then
                '    result(2) = "Total amount of foreign main and detail are not balanced" : Trans.Rollback() : GoTo selesai
                'End If
                ''END OF CEK TOTAL UTAMA DAN DETAIL ======================

                If isUpdate Then
                    result(4) = drutama("apid")
                    notransaksi = drutama("apnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(apid), apnotransaksi FROM M4_ap WHERE apid='" & result(4) & "' AND apstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(apid) FROM m4_ap WHERE apnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_ap_history
                        Dim rsSimpanHistory As String = SimpanHistory.M4_Ap_HistorySimpan("" & paramSplit(0) & "★M4_Ap_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("apsumber")) & "▼" & FixQuotes(drutama("apid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M4_Ap set apcabang  = '" & FixQuotes(drutama("apcabang")) & "', aplokasi  = '" & FixQuotes(drutama("aplokasi")) & "', apjenis  = " & drutama("apjenis") & ", apsumber  = '" & FixQuotes(drutama("apsumber")) & "', apautonotransaksi  = " & drutama("apautonotransaksi") & ", apnotransaksi  = '" & notransaksi & "', aptgl  = '" & FixQuotes(AsFormatTanggal(drutama("aptgl"))) & "', apkodepa  = " & drutama("apkodepa") & ", apkontak  = " & drutama("apkontak") & ", apkontakperson  = '" & FixQuotes(drutama("apkontakperson")) & "', ap1alamat1  = '" & FixQuotes(drutama("ap1alamat1")) & "', ap1alamat2  = '" & FixQuotes(drutama("ap1alamat2")) & "', ap1alamat3  = '" & FixQuotes(drutama("ap1alamat3")) & "', ap2alamat1  = '" & FixQuotes(drutama("ap2alamat1")) & "', ap2alamat2  = '" & FixQuotes(drutama("ap2alamat2")) & "', ap2alamat3  = '" & FixQuotes(drutama("ap2alamat3")) & "', apbagianpembayaran  = " & drutama("apbagianpembayaran") & ", aptermin  = '" & FixQuotes(drutama("aptermin")) & "', aptgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("aptgljatuhtempo"))) & "', apidpo  = " & drutama("apidpo") & ", apnorek  = '" & FixQuotes(drutama("apnorek")) & "', apuraian  = '" & FixQuotes(drutama("apuraian")) & "', apcatatan  = '" & FixQuotes(drutama("apcatatan")) & "', apnoref  = '" & FixQuotes(drutama("apnoref")) & "', aptglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("aptglnoref"))) & "', apmatauang  = '" & FixQuotes(drutama("apmatauang")) & "', apkurs  = '" & FixDouble(drutama("apkurs")) & "', apjumlah  = '" & FixDouble(drutama("apjumlah")) & "', apjumlahvalas  = '" & FixDouble(drutama("apjumlahvalas")) & "', apjumlahbayar  = '" & FixDouble(drutama("apjumlahbayar")) & "', apjumlahbayarvalas  = '" & FixDouble(drutama("apjumlahbayarvalas")) & "', apstatusbayar  = " & drutama("apstatusbayar") & ", aptgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("aptgllunas"))) & "', apcostcenter  = '" & FixQuotes(drutama("apcostcenter")) & "', apdivisi  = '" & FixQuotes(drutama("apdivisi")) & "', apsubdivisi  = '" & FixQuotes(drutama("apsubdivisi")) & "', approyek  = '" & FixQuotes(drutama("approyek")) & "', apstatus  = " & drutama("apstatus") & ", apstatussebelumnya  = " & drutama("apstatussebelumnya") & ", apjmlrevisi  = apjmlrevisi+1, apcetakanke  = " & drutama("apcetakanke") & ", apmodifikasiuser  = " & drutama("apmodifikasiuser") & ", apmodifikasitgl  = NOW(), apposting  = 0, apcustomtext1  = '" & FixQuotes(drutama("apcustomtext1")) & "', apcustomtext2  = '" & FixQuotes(drutama("apcustomtext2")) & "', apcustomtext3  = '" & FixQuotes(drutama("apcustomtext3")) & "', apcustomtext4  = '" & FixQuotes(drutama("apcustomtext4")) & "', apcustomtext5  = '" & FixQuotes(drutama("apcustomtext5")) & "', apcustomint1  = " & drutama("apcustomint1") & ", apcustomint2  = " & drutama("apcustomint2") & ", apcustomint3  = " & drutama("apcustomint3") & ", apcustomdbl1  = '" & FixDouble(drutama("apcustomdbl1")) & "', apcustomdbl2  = '" & FixDouble(drutama("apcustomdbl2")) & "', apcustomdbl3  = '" & FixDouble(drutama("apcustomdbl3")) & "', apcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("apcustomdate1"))) & "', apcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("apcustomdate2"))) & "', apcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("apcustomdate3"))) & "' where apid = '" & drutama("apid") & "'"
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

                    If drutama("apautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("apcabang"), drutama("aplokasi"), drutama("apsumber"), drutama("aptgl"))
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
                        notransaksi = drutama("apnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(apid) FROM m4_ap WHERE apnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Ap (apcabang, aplokasi, apjenis, apsumber, apautonotransaksi, apnotransaksi, aptgl, apkodepa, apkontak, apkontakperson, ap1alamat1, ap1alamat2, ap1alamat3, ap2alamat1, ap2alamat2, ap2alamat3, apbagianpembayaran, aptermin, aptgljatuhtempo, apidpo, apnorek, apuraian, apcatatan, apnoref, aptglnoref, apmatauang, apkurs, apjumlah, apjumlahvalas, apjumlahbayar, apjumlahbayarvalas, apstatusbayar, aptgllunas, apcostcenter, apdivisi, apsubdivisi, approyek, apstatus, apstatussebelumnya, apjmlrevisi, apcetakanke, apinputuser, apinputtgl, apmodifikasiuser, apmodifikasitgl, apposting, apisclose, apcustomtext1, apcustomtext2, apcustomtext3, apcustomtext4, apcustomtext5, apcustomint1, apcustomint2, apcustomint3, apcustomdbl1, apcustomdbl2, apcustomdbl3, apcustomdate1, apcustomdate2, apcustomdate3) values('" & FixQuotes(drutama("apcabang")) & "', '" & FixQuotes(drutama("aplokasi")) & "', " & drutama("apjenis") & ", '" & FixQuotes(drutama("apsumber")) & "', " & drutama("apautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("aptgl"))) & "', " & drutama("apkodepa") & ", " & drutama("apkontak") & ", '" & FixQuotes(drutama("apkontakperson")) & "', '" & FixQuotes(drutama("ap1alamat1")) & "', '" & FixQuotes(drutama("ap1alamat2")) & "', '" & FixQuotes(drutama("ap1alamat3")) & "', '" & FixQuotes(drutama("ap2alamat1")) & "', '" & FixQuotes(drutama("ap2alamat2")) & "', '" & FixQuotes(drutama("ap2alamat3")) & "', " & drutama("apbagianpembayaran") & ", '" & FixQuotes(drutama("aptermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("aptgljatuhtempo"))) & "', " & drutama("apidpo") & ", '" & FixQuotes(drutama("apnorek")) & "', '" & FixQuotes(drutama("apuraian")) & "', '" & FixQuotes(drutama("apcatatan")) & "', '" & FixQuotes(drutama("apnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("aptglnoref"))) & "', '" & FixQuotes(drutama("apmatauang")) & "', '" & FixDouble(drutama("apkurs")) & "', '" & FixDouble(drutama("apjumlah")) & "', '" & FixDouble(drutama("apjumlahvalas")) & "', '" & FixDouble(drutama("apjumlahbayar")) & "', '" & FixDouble(drutama("apjumlahbayarvalas")) & "', " & drutama("apstatusbayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("aptgllunas"))) & "', '" & FixQuotes(drutama("apcostcenter")) & "', '" & FixQuotes(drutama("apdivisi")) & "', '" & FixQuotes(drutama("apsubdivisi")) & "', '" & FixQuotes(drutama("approyek")) & "', " & drutama("apstatus") & ", " & drutama("apstatussebelumnya") & ", " & drutama("apjmlrevisi") & ", " & drutama("apcetakanke") & ", " & drutama("apinputuser") & ", NOW(), " & drutama("apmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("apisclose") & ", '" & FixQuotes(drutama("apcustomtext1")) & "', '" & FixQuotes(drutama("apcustomtext2")) & "', '" & FixQuotes(drutama("apcustomtext3")) & "', '" & FixQuotes(drutama("apcustomtext4")) & "', '" & FixQuotes(drutama("apcustomtext5")) & "', " & drutama("apcustomint1") & ", " & drutama("apcustomint2") & ", " & drutama("apcustomint3") & ", '" & FixDouble(drutama("apcustomdbl1")) & "', '" & FixDouble(drutama("apcustomdbl2")) & "', '" & FixDouble(drutama("apcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("apcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("apcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("apcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select apid from M4_ap where apnotransaksi='" & notransaksi & "' AND apinputuser= '" & userid & "' order by apmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Ap_Pay where idap = '" & result(4) & "'"
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
                    Dim strValue2 As New StringBuilder, strGiro As New StringBuilder
                    Dim rsCekGiro As String

                    For Each dr1 As DataRow In dtdetail.Rows

                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idapcarabayar") & ", " & result(4) & ", " & dr1("carabayar") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ")")

                        'QUERY UNTUK INSERT GIRO
                        If dr1("carabayar") = 2 Then

                            'CEK HAK AKSES APPROVED GIRO KELUAR =====================
                            If drutama("apstatus") = 2 Then
                                rsCekGiro = HakAksesGiro(4, 8, userid) 'MODULEID, MENUID, USERID SESUAI TRANSAKSI
                                If Len(rsCekGiro) <> 0 Then result(2) = rsCekGiro : Trans.Rollback() : GoTo selesai
                            End If
                            'END OF CEK HAK AKSES APPROVED GIRO KELUAR ==============

                            strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", ", "))
                            strGiro.Append("('" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(drutama("apsumber")) & "', " & result(4) & ", '" & FixQuotes(notransaksi) & "', " & drutama("apkontak") & ", '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', " & 1 & ", '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', " & 0 & ", " & 0 & ", " & dr1("urutan") & ")")
                        End If
                    Next
                    sql = "Insert into M4_Ap_Pay(idapcarabayar, idap, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'INSERT GIRO JIKA STATUS APPROVED DAN CARABAYAR = 2
                    If drutama("apstatus") = 2 And Len(strGiro.ToString) > 0 Then
                        sql = "Insert into M2_Giro_List(glnogiro, glsumber, glidtransaksi, glnotransaksi, glkontak, glrekbank, glrekgiro, gljenis, glbank, glnoacbank, glmatauang, glkurs, gljumlah, gljumlahvalas, gltgljthtempo, gltglcair, glstatus, glstatussebelumnya, glurutan) values" & strGiro.ToString & ""
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
                    result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "AP", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("apstatus") = 2 Then
                    Dim Security As New ClsSecurity, mjid As String = "", hasilMsmq As String = ""
                    'BUAT ID UNIQUE
                    mjid = Security.MD5CalcString(userid & sumber & result(4) & Now) 'RandomId.Generate(15)

                    'MSMQ TABEL
                    sql = "Insert into M0_Msmq_Journal(mjid, mjsumber, mjidtransaksi, mjprogress, mjpesan, mjtglantrian, mjtglselesai, mjuserid) values ('" _
                        & mjid & "', '" & sumber & "', '" & result(4) & "', '" & 0 & "', " & "''" & ", NOW(), '1971-01-01 00:00:00', '" & userid & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'MSMQ ANTRIAN
                    Dim PostingJurnal As String = F_getSetting(0, "accounting", "AutoPosting")
                    If PostingJurnal.Equals("0") = False Then
                        hasilMsmq = SendMsmq(dirMsmq, "J", mjid, sumber, result(4), userid)
                        If Len(hasilMsmq) > 0 Then
                            result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
                        End If
                    End If

                End If
                'END OF INSERT MSMQ JURNAL ==========================================================

                'INSERT USER LOG ====================================================================
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
    Public Function M4_ApUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("apkontakkode", "c1.kkode")
            Filter = Filter.Replace("apkontaknama", "c1.knama")
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
            Dim sumber As String = "Ap", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Aptgl, Apnotransaksi, Apstatus FROM m4_Ap WHERE Apid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Apstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m4_ap_history
            Dim rsSimpanHistory As String = SimpanHistory.M4_Ap_HistorySimpan("" & paramSplit(0) & "★M4_Ap_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m4_ap_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                ''CEK STATUS GIRO
                'dtdetail = AsDataTableAmbilDariDB("SELECT glnogiro FROM m2_giro_list WHERE glsumber = 'AP' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "' AND glstatus <> 0")
                'If dtdetail.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai

                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'AP' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'DELETE GIRO
                sql = "DELETE FROM m2_giro_list WHERE glsumber = 'AP' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'update status utama
            sql = "UPDATE m4_Ap SET Apstatus = " & nilaiStatus & ", Apmodifikasiuser='" & userid & "', Apmodifikasitgl = NOW(), Apposting = 0, Appostingtgl = '1971-01-01 00:00:00', Apjmlrevisi = Apjmlrevisi + 1 WHERE Apid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_ApSearch(PostWsSearch(paramSplit(0), "M4_ApSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_ApDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("apkontakkode", "c1.kkode")
            Filter = Filter.Replace("apkontaknama", "c1.knama")
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
            Dim sumber As String = "Ap", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Apid, Apnotransaksi FROM M4_Ap WHERE Apid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT apcabang, aplokasi, apsumber, apautonotransaksi, apnotransaksi, aptgl"
            sql &= " FROM M4_ap"
            sql &= " WHERE apid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("apcabang")
                lokasi = dtNomorNext.Rows(0)("aplokasi")
                sumber = dtNomorNext.Rows(0)("apsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("apautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("apnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("aptgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M4_Ap_Pay WHERE idap = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M4_Ap WHERE apid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_ApSearch(PostWsSearch(paramSplit(0), "M4_ApSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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