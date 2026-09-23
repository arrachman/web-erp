Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_as
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_AsSimpan(ByVal param As String) As String
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

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean, tglLunas As String = ""

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
        'asid(0) As Integer, ascabang(1) As String, aslokasi(2) As String, asjenis(3) As Integer, assumber(4) As String, 
        'asautonotransaksi(5) As Integer, asnotransaksi(6) As String, astgl(7) As Date, askodepa(8) As Integer, askontak(9) As Integer, 
        'askontakperson(10) As String, as1alamat1(11) As String, as1alamat2(12) As String, as1alamat3(13) As String, as2alamat1(14) As String, 
        'as2alamat2(15) As String, as2alamat3(16) As String, asbagianterima(17) As Integer, astermin(18) As String, astgljatuhtempo(19) As Date, 
        'asidso(20) As Integer, asidip(21) As Integer, asnorek(22) As String, asuraian(23) As String, ascatatan(24) As String, 
        'asnoref(25) As String, astglnoref(26) As Date, asmatauang(27) As String, askurs(28) As Double, asjumlah(29) As Double, 
        'asjumlahvalas(30) As Double, asjumlahbayar(31) As Double, asjumlahbayarvalas(32) As Double, asstatusbayar(33) As Integer, astgllunas(34) As Date, 
        'ascostcenter(35) As String, asdivisi(36) As String, assubdivisi(37) As String, asproyek(38) As String, asstatus(39) As Integer, 
        'asstatussebelumnya(40) As Integer, asjmlrevisi(41) As Integer, ascetakanke(42) As Integer, asinputuser(43) As Integer, asinputtgl(44) As DateTime, 
        'asmodifikasiuser(45) As Integer, asmodifikasitgl(46) As DateTime, asposting(47) As Integer, asisclose(48) As Integer, ascustomtext1(49) As String, 
        'ascustomtext2(50) As String, ascustomtext3(51) As String, ascustomtext4(52) As String, ascustomtext5(53) As String, ascustomint1(54) As Integer, 
        'ascustomint2(55) As Integer, ascustomint3(56) As Integer, ascustomdbl1(57) As Double, ascustomdbl2(58) As Double, ascustomdbl3(59) As Double, 
        'ascustomdate1(60) As Date, ascustomdate2(61) As Date, ascustomdate3(62) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'asid, ascabang, aslokasi, asjenis, assumber, asautonotransaksi, asnotransaksi, 
        'astgl, askodepa, askontak, askontakperson, as1alamat1, as1alamat2, as1alamat3, 
        'as2alamat1, as2alamat2, as2alamat3, asbagianterima, astermin, astgljatuhtempo, asidso, 
        'asidip, asnorek, asuraian, ascatatan, asnoref, astglnoref, asmatauang, 
        'askurs, asjumlah, asjumlahvalas, asjumlahbayar, asjumlahbayarvalas, asstatusbayar, astgllunas, 
        'ascostcenter, asdivisi, assubdivisi, asproyek, asstatus, asstatussebelumnya, asjmlrevisi, 
        'ascetakanke, asinputuser, asinputtgl, asmodifikasiuser, asmodifikasitgl, asposting, asisclose, 
        'ascustomtext1, ascustomtext2, ascustomtext3, ascustomtext4, ascustomtext5, ascustomint1, ascustomint2, 
        'ascustomint3, ascustomdbl1, ascustomdbl2, ascustomdbl3, ascustomdate1, ascustomdate2, ascustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 63) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'asid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "asid required numeric." : GoTo selesai
        End If
        'asjenis(3) As Integer
        If (IsNumeric(dataUtama(3)) = False) Then
            result(2) = "asjenis required numeric." : GoTo selesai
        End If
        'asautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "asautonotransaksi required numeric." : GoTo selesai
        End If
        'astgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "astgl required date." : GoTo selesai
        End If
        'askodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "askodepa required numeric." : GoTo selesai
        End If
        'askontak(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "askontak required numeric." : GoTo selesai
        End If
        'asbagianterima(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "asbagianterima required numeric." : GoTo selesai
        End If
        'astgljatuhtempo(19) As Date
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "astgljatuhtempo required date." : GoTo selesai
        End If
        'asidso(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "asidso required numeric." : GoTo selesai
        End If
        'asidip(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "asidip required numeric." : GoTo selesai
        End If
        'astglnoref(26) As Date
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "astglnoref required date." : GoTo selesai
        End If
        'askurs(28) As Double
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "askurs required numeric." : GoTo selesai
        End If
        'asjumlah(29) As Double
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "asjumlah required numeric." : GoTo selesai
        End If
        'asjumlahvalas(30) As Double
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "asjumlahvalas required numeric." : GoTo selesai
        End If
        'asjumlahbayar(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "asjumlahbayar required numeric." : GoTo selesai
        End If
        'asjumlahbayarvalas(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "asjumlahbayarvalas required numeric." : GoTo selesai
        End If
        'asstatusbayar(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "asstatusbayar required numeric." : GoTo selesai
        End If
        'astgllunas(34) As Date
        If (IsDate(dataUtama(34)) = False) Then
            result(2) = "astgllunas required date." : GoTo selesai
        End If
        'asstatus(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "asstatus required numeric." : GoTo selesai
        End If
        'asstatussebelumnya(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "asstatussebelumnya required numeric." : GoTo selesai
        End If
        'asjmlrevisi(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "asjmlrevisi required numeric." : GoTo selesai
        End If
        'ascetakanke(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "ascetakanke required numeric." : GoTo selesai
        End If
        'asinputuser(43) As Integer
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "asinputuser required numeric." : GoTo selesai
        End If
        'asinputtgl(44) As DateTime
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "asinputtgl required date." : GoTo selesai
        End If
        'asmodifikasiuser(45) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "asmodifikasiuser required numeric." : GoTo selesai
        End If
        'asmodifikasitgl(46) As DateTime
        If (IsDate(dataUtama(46)) = False) Then
            result(2) = "asmodifikasitgl required date." : GoTo selesai
        End If
        'asposting(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "asposting required numeric." : GoTo selesai
        End If
        'asisclose(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "asisclose required numeric." : GoTo selesai
        End If
        'ascustomint1(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "ascustomint1 required numeric." : GoTo selesai
        End If
        'ascustomint2(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "ascustomint2 required numeric." : GoTo selesai
        End If
        'ascustomint3(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "ascustomint3 required numeric." : GoTo selesai
        End If
        'ascustomdbl1(57) As Double
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "ascustomdbl1 required numeric." : GoTo selesai
        End If
        'ascustomdbl2(58) As Double
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "ascustomdbl2 required numeric." : GoTo selesai
        End If
        'ascustomdbl3(59) As Double
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "ascustomdbl3 required numeric." : GoTo selesai
        End If
        'ascustomdate1(60) As Date
        If (IsDate(dataUtama(60)) = False) Then
            result(2) = "ascustomdate1 required date." : GoTo selesai
        End If
        'ascustomdate2(61) As Date
        If (IsDate(dataUtama(61)) = False) Then
            result(2) = "ascustomdate2 required date." : GoTo selesai
        End If
        'ascustomdate3(62) As Date
        If (IsDate(dataUtama(62)) = False) Then
            result(2) = "ascustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'ascabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "ascabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "ascabang should not be more than 25 character." : GoTo selesai
        End If

        'aslokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "aslokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "aslokasi should not be more than 25 character." : GoTo selesai
        End If

        'assumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "assumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "assumber should not be more than 10 character." : GoTo selesai
        End If

        'asnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "asnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "asnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'astgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "astgl can't be empty" : GoTo selesai
        End If
        'SET TGLTRANSAKSI ---> UNTUK UPDATE TGL LUNAS TRANSAKSI
        tglLunas = AsFormatTanggal(dataUtama(7))

        'astgljatuhtempo(19) As Date
        If Len(dataUtama(19)) = 0 Then
            result(2) = "astgljatuhtempo can't be empty" : GoTo selesai
        End If

        'asnorek(22) As String
        If Len(dataUtama(22)) = 0 Then
            result(2) = "asnorek can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(22)) > 25 Then
            result(2) = "asnorek should not be more than 25 character." : GoTo selesai
        End If

        'astglnoref(26) As Date
        If Len(dataUtama(26)) = 0 Then
            result(2) = "astglnoref can't be empty" : GoTo selesai
        End If

        'asmatauang(27) As String
        If Len(dataUtama(27)) = 0 Then
            result(2) = "asmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(27)) > 25 Then
            result(2) = "asmatauang should not be more than 25 character." : GoTo selesai
        End If

        'askurs(28) As Double
        If Len(dataUtama(28)) = 0 Then
            result(2) = "askurs can't be empty" : GoTo selesai
        End If

        'asjumlah(29) As Double
        If Len(dataUtama(29)) = 0 Then
            result(2) = "asjumlah can't be empty" : GoTo selesai
        End If

        'asjumlahvalas(30) As Double
        If Len(dataUtama(30)) = 0 Then
            result(2) = "asjumlahvalas can't be empty" : GoTo selesai
        End If

        'asjumlahbayar(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "asjumlahbayar can't be empty" : GoTo selesai
        End If

        'asjumlahbayarvalas(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "asjumlahbayarvalas can't be empty" : GoTo selesai
        End If

        'astgllunas(34) As Date
        If Len(dataUtama(34)) = 0 Then
            result(2) = "astgllunas can't be empty" : GoTo selesai
        End If

        'asinputtgl(44) As DateTime
        If Len(dataUtama(44)) = 0 Then
            result(2) = "asinputtgl can't be empty" : GoTo selesai
        End If

        'asmodifikasitgl(46) As DateTime
        If Len(dataUtama(46)) = 0 Then
            result(2) = "asmodifikasitgl can't be empty" : GoTo selesai
        End If

        'ascustomdbl1(57) As Double
        If Len(dataUtama(57)) = 0 Then
            result(2) = "ascustomdbl1 can't be empty" : GoTo selesai
        End If

        'ascustomdbl2(58) As Double
        If Len(dataUtama(58)) = 0 Then
            result(2) = "ascustomdbl2 can't be empty" : GoTo selesai
        End If

        'ascustomdbl3(59) As Double
        If Len(dataUtama(59)) = 0 Then
            result(2) = "ascustomdbl3 can't be empty" : GoTo selesai
        End If

        'ascustomdate1(60) As Date
        If Len(dataUtama(60)) = 0 Then
            result(2) = "ascustomdate1 can't be empty" : GoTo selesai
        End If

        'ascustomdate2(61) As Date
        If Len(dataUtama(61)) = 0 Then
            result(2) = "ascustomdate2 can't be empty" : GoTo selesai
        End If

        'ascustomdate3(62) As Date
        If Len(dataUtama(62)) = 0 Then
            result(2) = "ascustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "asid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aslokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asjenis", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "assumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "asnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "astgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "askodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "askontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "askontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "as1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "as1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "as1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "as2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "as2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "as2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asbagianterima", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "astermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "astgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asidso", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "asidip", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "asnorek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "astglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "askurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asjumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "asjumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "asjumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asjumlahbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asstatusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "astgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascostcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "assubdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asproyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "asstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "asjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ascetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "asinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "asinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "asmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "asisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ascustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ascustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ascustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ascustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "asid~ascabang~aslokasi~asjenis~assumber~asautonotransaksi~asnotransaksi~astgl~askodepa~askontak~askontakperson~as1alamat1~as1alamat2~as1alamat3~as2alamat1~as2alamat2~as2alamat3~asbagianterima~astermin~astgljatuhtempo~asidso~asidip~asnorek~asuraian~ascatatan~asnoref~astglnoref~asmatauang~askurs~asjumlah~asjumlahvalas~asjumlahbayar~asjumlahbayarvalas~asstatusbayar~astgllunas~ascostcenter~asdivisi~assubdivisi~asproyek~asstatus~asstatussebelumnya~asjmlrevisi~ascetakanke~asinputuser~asinputtgl~asmodifikasiuser~asmodifikasitgl~asposting~asisclose~ascustomtext1~ascustomtext2~ascustomtext3~ascustomtext4~ascustomtext5~ascustomint1~ascustomint2~ascustomint3~ascustomdbl1~ascustomdbl2~ascustomdbl3~ascustomdate1~ascustomdate2~ascustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idascarabayar(0) As Integer, idas(1) As Integer, carabayar(2) As Integer, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, nogiro(7) As String, tgljt(8) As Date, bank(9) As String, 
        'noacbank(10) As String, rekbank(11) As String, rekgiro(12) As String, catatan(13) As String, urutan(14) As Integer, 
        'idip(15) As Integer, isclose(16) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idascarabayar, idas, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, idip, isclose

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idascarabayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idas", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "idip", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt64)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'AMBIL MATA UANG FUNGSIONAL DARI SETTING ================
        Dim MUFungsional As String = ""
        Dim dtSetting As DataTable = AsDataTableAmbilDariDBCon("SELECT snilai FROM m0_setting WHERE (smodule='0') AND (sgrup='accounting') AND (skode='MataUangFungsional')", myConn)
        If dtSetting.Rows.Count > 0 Then
            MUFungsional = dtSetting.Rows(0)(0)
        Else
            result(2) = "Can't found 'Functional Currency' in Setting." : GoTo selesai
        End If
        'END OF AMBIL MATA UANG FUNGSIONAL DARI SETTING =========

        'VARIABEL VALIDASI OUTSTANDING, IP
        Dim ftExistOutstandingIP As String = "", ftOutstandingIP As String = "", updNilaiIP As String = "", updNilaiValasIP As String = "", updFilterIP As String = "", updTglLunasIP As String = ""
        Dim idip As Integer = 0, matauangDetail As String = ""
        Dim Outstanding As Double = 0, OutstandingValas As Double = 0

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 17) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idascarabayar(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "idascarabayar required numeric." : GoTo selesai
            End If
            'idas(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "idas required numeric." : GoTo selesai
            End If
            'carabayar(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "carabayar required numeric." : GoTo selesai
            End If
            'kurs(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "kurs required numeric." : GoTo selesai
            End If
            'jumlah(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "jumlah required numeric." : GoTo selesai
            End If
            'jumlahvalas(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "jumlahvalas required numeric." : GoTo selesai
            End If
            'tgljt(8) As Date
            If (IsDate(dataRowDetail(8)) = False) Then
                result(2) = "tgljt required date." : GoTo selesai
            End If
            'urutan(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "urutan required numeric." : GoTo selesai
            End If
            'idip(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "idip required numeric." : GoTo selesai
            End If
            'isclose(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "isclose required numeric." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idascarabayar~idas~carabayar~matauang~kurs~jumlah~jumlahvalas~nogiro~tgljt~bank~noacbank~rekbank~rekgiro~catatan~urutan~idip~isclose", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'Set variabel
            'idip(15) As Integer     , matauang(3) As String
            idip = dataRowDetail(15) : matauangDetail = dataRowDetail(3)

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'VALIDASI OUTSTANDING -------------------------
            If idip <> 0 Then 'IP
                '1. CEK DATA EXIST
                ftExistOutstandingIP = IIf(Len(ftExistOutstandingIP.ToString) = 0, "", ftExistOutstandingIP & " UNION ")
                ftExistOutstandingIP = String.Concat(ftExistOutstandingIP, "SELECT EXISTS(SELECT 1 FROM m5_ip WHERE ipid = '" & idip & "' AND (ipstatus = 2 OR ipstatus = 3 OR ipstatus = 4 OR ipstatus = 7) LIMIT 1) as rowExists, ipid, ipsumber, ipnotransaksi FROM m5_ip WHERE ipid = '" & idip & "'")

                '2. CEK JML OUTSTANDING
                Outstanding = AsDataTableDSum(dtdetail, "jumlah", "idip=" & idip)
                OutstandingValas = AsDataTableDSum(dtdetail, "jumlahvalas", "idip=" & idip)
                ftOutstandingIP = IIf(Len(ftOutstandingIP.ToString) = 0, "", ftOutstandingIP & " OR ")
                ftOutstandingIP = String.Concat(ftOutstandingIP, " (ip.ipid = '" & idip & "' AND (CASE ip.ipmatauang WHEN s.snilai THEN " & Outstanding & " > ip.ipjumlah - ip.ipjumlahbayar ELSE " & OutstandingValas & " > ip.ipjumlahvalas - ip.ipjumlahbayarvalas END)) ")

                '3. SET NILAI UPDATE OUTSTANDING
                updNilaiIP = String.Concat("WHEN '" & idip & "' THEN ROUND(ipjumlahbayar + '" & Outstanding & "', 5) ", updNilaiIP)
                updNilaiValasIP = String.Concat("WHEN '" & idip & "' THEN ROUND(ip.ipjumlahbayarvalas + '" & OutstandingValas & "', 5) ", updNilaiValasIP)

                '4. SET FILTER UPDATE OUTSTANDING
                updFilterIP = IIf(Len(updFilterIP.ToString) = 0, "", updFilterIP & " OR ")
                updFilterIP = String.Concat(updFilterIP, "(ip.ipid = '" & idip & "')")

                '5. SET NILAI TGLLUNAS TRANSAKSI
                If matauangDetail = MUFungsional Then
                    updTglLunasIP = String.Concat(" WHEN '" & idip & "' THEN (CASE WHEN ROUND(ip.ipjumlahbayar + '" & Outstanding & "', 5) >= ip.ipjumlah THEN '" & FixQuotes(tglLunas) & "' ELSE ip.iptgllunas END) ", updTglLunasIP)
                Else
                    updTglLunasIP = String.Concat(" WHEN '" & idip & "' THEN (CASE WHEN ROUND(ip.ipjumlahbayarvalas + '" & OutstandingValas & "', 5) >= ip.ipjumlahvalas THEN '" & FixQuotes(tglLunas) & "' ELSE ip.iptgllunas END) ", updTglLunasIP)
                End If
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
                Dim vModuleId As Integer = 5, vMenuId As Integer = 5
                Select Case drutama("asstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("astgl")), AsFormatTanggal(drutama("astgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "asmatauang", "asnorek", dtdetail, "rekbank")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("astermin").ToString, AsFormatTanggal(drutama("astgl")), "astgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("astgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                'HITUNG TOTAL BERDASARKAN DATA DETAIL ===================
                drutama("asjumlah") = AsDataTableDSum(dtdetail, "jumlah")
                drutama("asjumlahvalas") = AsDataTableDSum(dtdetail, "jumlahvalas")
                'END OF HITUNG TOTAL BERDASARKAN DATA DETAIL ============


                ''CEK TOTAL UTAMA DAN DETAIL =============================
                'Dim jumlah As Double = AsDataTableDSum(dtdetail, "jumlah")
                'Dim jumlahvalas As Double = AsDataTableDSum(dtdetail, "jumlahvalas")
                'If Double.Parse(drutama("asjumlah")) <> jumlah Then
                '    result(2) = "Total amount of main and detail are not balanced" : Trans.Rollback() : GoTo selesai
                'ElseIf Double.Parse(drutama("asjumlahvalas")) <> jumlahvalas Then
                '    result(2) = "Total amount of foreign main and detail are not balanced" : Trans.Rollback() : GoTo selesai
                'End If
                ''END OF CEK TOTAL UTAMA DAN DETAIL ======================


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("asstatus") = 2 Or drutama("asstatus") = 1 Or drutama("asstatus") = 8 Or drutama("asstatus") = 9 Or drutama("asstatus") = 10 Or drutama("asstatus") = 11 Then
                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstandingIP, ftOutstandingIP)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                If isUpdate Then
                    result(4) = drutama("asid")
                    notransaksi = drutama("asnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(asid), asnotransaksi FROM M5_as WHERE asid='" & result(4) & "' AND asstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("asautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("ascabang"), drutama("aslokasi"), drutama("assumber"), drutama("astgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(asid) FROM m5_as WHERE asnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_as_history
                        Dim rsSimpanHistory As String = SimpanHistory.M5_As_HistorySimpan("" & paramSplit(0) & "★M5_As_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("assumber")) & "▼" & FixQuotes(drutama("asid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_As set ascabang  = '" & FixQuotes(drutama("ascabang")) & "', aslokasi  = '" & FixQuotes(drutama("aslokasi")) & "', asjenis  = " & drutama("asjenis") & ", assumber  = '" & FixQuotes(drutama("assumber")) & "', asautonotransaksi  = " & drutama("asautonotransaksi") & ", asnotransaksi  = '" & notransaksi & "', astgl  = '" & FixQuotes(AsFormatTanggal(drutama("astgl"))) & "', askodepa  = " & drutama("askodepa") & ", askontak  = " & drutama("askontak") & ", askontakperson  = '" & FixQuotes(drutama("askontakperson")) & "', as1alamat1  = '" & FixQuotes(drutama("as1alamat1")) & "', as1alamat2  = '" & FixQuotes(drutama("as1alamat2")) & "', as1alamat3  = '" & FixQuotes(drutama("as1alamat3")) & "', as2alamat1  = '" & FixQuotes(drutama("as2alamat1")) & "', as2alamat2  = '" & FixQuotes(drutama("as2alamat2")) & "', as2alamat3  = '" & FixQuotes(drutama("as2alamat3")) & "', asbagianterima  = " & drutama("asbagianterima") & ", astermin  = '" & FixQuotes(drutama("astermin")) & "', astgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("astgljatuhtempo"))) & "', asidso  = " & drutama("asidso") & ", asidip  = " & drutama("asidip") & ", asnorek  = '" & FixQuotes(drutama("asnorek")) & "', asuraian  = '" & FixQuotes(drutama("asuraian")) & "', ascatatan  = '" & FixQuotes(drutama("ascatatan")) & "', asnoref  = '" & FixQuotes(drutama("asnoref")) & "', astglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("astglnoref"))) & "', asmatauang  = '" & FixQuotes(drutama("asmatauang")) & "', askurs  = '" & FixDouble(drutama("askurs")) & "', asjumlah  = '" & FixDouble(drutama("asjumlah")) & "', asjumlahvalas  = '" & FixDouble(drutama("asjumlahvalas")) & "', asjumlahbayar  = '" & FixDouble(drutama("asjumlahbayar")) & "', asjumlahbayarvalas  = '" & FixDouble(drutama("asjumlahbayarvalas")) & "', asstatusbayar  = " & drutama("asstatusbayar") & ", astgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("astgllunas"))) & "', ascostcenter  = '" & FixQuotes(drutama("ascostcenter")) & "', asdivisi  = '" & FixQuotes(drutama("asdivisi")) & "', assubdivisi  = '" & FixQuotes(drutama("assubdivisi")) & "', asproyek  = '" & FixQuotes(drutama("asproyek")) & "', asstatus  = " & drutama("asstatus") & ", asstatussebelumnya  = " & drutama("asstatussebelumnya") & ", asjmlrevisi  = asjmlrevisi+1, ascetakanke  = " & drutama("ascetakanke") & ", asmodifikasiuser  = " & drutama("asmodifikasiuser") & ", asmodifikasitgl  = NOW(), asposting  = 0, ascustomtext1  = '" & FixQuotes(drutama("ascustomtext1")) & "', ascustomtext2  = '" & FixQuotes(drutama("ascustomtext2")) & "', ascustomtext3  = '" & FixQuotes(drutama("ascustomtext3")) & "', ascustomtext4  = '" & FixQuotes(drutama("ascustomtext4")) & "', ascustomtext5  = '" & FixQuotes(drutama("ascustomtext5")) & "', ascustomint1  = " & drutama("ascustomint1") & ", ascustomint2  = " & drutama("ascustomint2") & ", ascustomint3  = " & drutama("ascustomint3") & ", ascustomdbl1  = '" & FixDouble(drutama("ascustomdbl1")) & "', ascustomdbl2  = '" & FixDouble(drutama("ascustomdbl2")) & "', ascustomdbl3  = '" & FixDouble(drutama("ascustomdbl3")) & "', ascustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("ascustomdate1"))) & "', ascustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("ascustomdate2"))) & "', ascustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("ascustomdate3"))) & "' where asid = '" & drutama("asid") & "'"
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

                    If drutama("asautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("ascabang"), drutama("aslokasi"), drutama("assumber"), drutama("astgl"))
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
                        notransaksi = drutama("asnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(asid) FROM m5_as WHERE asnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_As (ascabang, aslokasi, asjenis, assumber, asautonotransaksi, asnotransaksi, astgl, askodepa, askontak, askontakperson, as1alamat1, as1alamat2, as1alamat3, as2alamat1, as2alamat2, as2alamat3, asbagianterima, astermin, astgljatuhtempo, asidso, asidip, asnorek, asuraian, ascatatan, asnoref, astglnoref, asmatauang, askurs, asjumlah, asjumlahvalas, asjumlahbayar, asjumlahbayarvalas, asstatusbayar, astgllunas, ascostcenter, asdivisi, assubdivisi, asproyek, asstatus, asstatussebelumnya, asjmlrevisi, ascetakanke, asinputuser, asinputtgl, asmodifikasiuser, asmodifikasitgl, asposting, asisclose, ascustomtext1, ascustomtext2, ascustomtext3, ascustomtext4, ascustomtext5, ascustomint1, ascustomint2, ascustomint3, ascustomdbl1, ascustomdbl2, ascustomdbl3, ascustomdate1, ascustomdate2, ascustomdate3) values('" & FixQuotes(drutama("ascabang")) & "', '" & FixQuotes(drutama("aslokasi")) & "', " & drutama("asjenis") & ", '" & FixQuotes(drutama("assumber")) & "', " & drutama("asautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("astgl"))) & "', " & drutama("askodepa") & ", " & drutama("askontak") & ", '" & FixQuotes(drutama("askontakperson")) & "', '" & FixQuotes(drutama("as1alamat1")) & "', '" & FixQuotes(drutama("as1alamat2")) & "', '" & FixQuotes(drutama("as1alamat3")) & "', '" & FixQuotes(drutama("as2alamat1")) & "', '" & FixQuotes(drutama("as2alamat2")) & "', '" & FixQuotes(drutama("as2alamat3")) & "', " & drutama("asbagianterima") & ", '" & FixQuotes(drutama("astermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("astgljatuhtempo"))) & "', " & drutama("asidso") & ", " & drutama("asidip") & ", '" & FixQuotes(drutama("asnorek")) & "', '" & FixQuotes(drutama("asuraian")) & "', '" & FixQuotes(drutama("ascatatan")) & "', '" & FixQuotes(drutama("asnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("astglnoref"))) & "', '" & FixQuotes(drutama("asmatauang")) & "', '" & FixDouble(drutama("askurs")) & "', '" & FixDouble(drutama("asjumlah")) & "', '" & FixDouble(drutama("asjumlahvalas")) & "', '" & FixDouble(drutama("asjumlahbayar")) & "', '" & FixDouble(drutama("asjumlahbayarvalas")) & "', " & drutama("asstatusbayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("astgllunas"))) & "', '" & FixQuotes(drutama("ascostcenter")) & "', '" & FixQuotes(drutama("asdivisi")) & "', '" & FixQuotes(drutama("assubdivisi")) & "', '" & FixQuotes(drutama("asproyek")) & "', " & drutama("asstatus") & ", " & drutama("asstatussebelumnya") & ", " & drutama("asjmlrevisi") & ", " & drutama("ascetakanke") & ", " & drutama("asinputuser") & ", NOW(), " & drutama("asmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("asisclose") & ", '" & FixQuotes(drutama("ascustomtext1")) & "', '" & FixQuotes(drutama("ascustomtext2")) & "', '" & FixQuotes(drutama("ascustomtext3")) & "', '" & FixQuotes(drutama("ascustomtext4")) & "', '" & FixQuotes(drutama("ascustomtext5")) & "', " & drutama("ascustomint1") & ", " & drutama("ascustomint2") & ", " & drutama("ascustomint3") & ", '" & FixDouble(drutama("ascustomdbl1")) & "', '" & FixDouble(drutama("ascustomdbl2")) & "', '" & FixDouble(drutama("ascustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ascustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ascustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ascustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select asid from M5_as where asnotransaksi='" & notransaksi & "' AND asinputuser= '" & userid & "' order by asmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_As_Pay where idas = '" & result(4) & "'"
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
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idascarabayar") & ", " & result(4) & ", " & dr1("carabayar") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idip") & ", " & dr1("isclose") & ")")
                        'QUERY UNTUK INSERT GIRO
                        If dr1("carabayar") = 2 Then
                            strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", ", "))
                            strGiro.Append("('" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(drutama("assumber")) & "', " & result(4) & ", '" & FixQuotes(notransaksi) & "', " & drutama("askontak") & ", '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', " & 0 & ", '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', " & 0 & ", " & 0 & ", " & dr1("urutan") & ")")
                        End If
                    Next
                    sql = "Insert into M5_As_Pay(idascarabayar, idas, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, idip, isclose) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'INSERT GIRO JIKA STATUS APPROVED DAN CARABAYAR = 2
                    If drutama("asstatus") = 2 And Len(strGiro.ToString) > 0 Then
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

                If drutama("asstatus") = 2 Then
                    'UPDATE OUTSTANDING TRANSAKSI ===================================================
                    If Len(updNilaiIP) > 0 Then 'IP
                        'TRANSAKSI
                        sql = "UPDATE m5_ip ip LEFT JOIN m2_transaction_journal t ON ip.ipsumber = t.tsumber AND ip.ipid =  t.tidtransaksi AND ip.ipnotransaksi = t.tnotransaksi SET ip.ipjumlahbayar = (CASE ip.ipid " & updNilaiIP & " ELSE ip.ipjumlahbayar END), ip.ipjumlahbayarvalas = (CASE ip.ipid " & updNilaiValasIP & " ELSE ip.ipjumlahbayarvalas END), ip.iptgllunas = (CASE ip.ipid " & updTglLunasIP & " ELSE ip.iptgllunas END) WHERE " & updFilterIP
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'JURNAL
                        sql = "UPDATE m5_ip ip LEFT JOIN m2_transaction_journal t ON ip.ipsumber = t.tsumber AND ip.ipid =  t.tidtransaksi AND ip.ipnotransaksi = t.tnotransaksi SET t.tstatuslunas = ip.ipstatusbayar, t.ttgllunas = ip.iptgllunas WHERE " & updFilterIP
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF UPDATE OUTSTANDING TRANSAKSI ============================================
                End If

                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "AS", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("asstatus") = 2 Then
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
    Public Function M5_AsUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("askontakkode", "c1.kkode")
            Filter = Filter.Replace("askontaknama", "c1.knama")
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
            Dim sumber As String = "As", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Astgl, Asnotransaksi, Asstatus FROM M5_As WHERE Asid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Asstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m5_as_history
            Dim rsSimpanHistory As String = SimpanHistory.M5_As_HistorySimpan("" & paramSplit(0) & "★M5_As_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m5_as_terkait("asid = '" & idtransaksi & "'")
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                ''CEK STATUS GIRO
                'dtdetail = asdatatableambildaridbcon("SELECT glnogiro FROM m2_giro_list WHERE glsumber = 'AS' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "' AND glstatus <> 0")
                'If dtdetail.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai

                'UPDATE OUTSTANDING TRANSAKSI ===================================================
                Dim Outstanding As Double = 0, OutstandingValas As Double = 0, tglLunas = "1900-01-01"
                Dim updNilaiIP As String = "", updNilaiValasIP As String = "", updFilterIP As String = "", matauangDetail As String = ""
                Dim idip As Integer = 0

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT matauang, jumlah, jumlahvalas, idip FROM m5_as_pay WHERE idas = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        '1.SET NILAI VARIABEL
                        idip = dr1("idip") : matauangDetail = dr1("matauang")

                        '2. SET NILAI UPDATE OUTSTANDING
                        Outstanding = AsDataTableDSum(dtdetail, "jumlah", "idip = '" & idip & "'")
                        OutstandingValas = AsDataTableDSum(dtdetail, "jumlahvalas", "idip = '" & idip & "'")

                        '3. SET NILAI UPDATE OUTSTANDING
                        updNilaiIP = String.Concat("WHEN '" & idip & "' THEN ROUND(ip.ipjumlahbayar - '" & Outstanding & "', 5) ", updNilaiIP)
                        updNilaiValasIP = String.Concat("WHEN '" & idip & "' THEN ROUND(ip.ipjumlahbayarvalas - '" & OutstandingValas & "', 5) ", updNilaiValasIP)

                        '4. SET FILTER UPDATE OUTSTANDING
                        updFilterIP = IIf(Len(updFilterIP.ToString) = 0, "", updFilterIP & " OR ")
                        updFilterIP = String.Concat(updFilterIP, "(ip.ipid = '" & idip & "')")
                    Next

                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If
                'END OF UPDATE OUTSTANDING TRANSAKSI ============================================

                'UPDATE JMLBAYAR IP
                If Len(updNilaiIP) > 0 And Len(updNilaiValasIP) > 0 Then 'IP
                    'TRANSAKSI
                    sql = "UPDATE m5_ip ip LEFT JOIN m2_transaction_journal t ON ip.ipsumber = t.tsumber AND ip.ipid = t.tidtransaksi AND ip.ipnotransaksi = t.tnotransaksi SET ip.ipjumlahbayar = (CASE ip.ipid " & updNilaiIP & " ELSE ip.ipjumlahbayar END), ip.ipjumlahbayarvalas = (CASE ip.ipid " & updNilaiValasIP & " ELSE ip.ipjumlahbayarvalas END), ip.iptgllunas = '" & FixQuotes(tglLunas) & "' WHERE " & updFilterIP
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'JURNAL
                    sql = "UPDATE m5_ip ip LEFT JOIN m2_transaction_journal t ON ip.ipsumber = t.tsumber AND ip.ipid = t.tidtransaksi AND ip.ipnotransaksi = t.tnotransaksi SET t.tstatuslunas = ip.ipstatusbayar, t.ttgllunas = ip.iptgllunas WHERE " & updFilterIP
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'AS' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'DELETE GIRO
                sql = "DELETE FROM m2_giro_list WHERE glsumber = 'AS' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M5_As SET Asstatus = " & nilaiStatus & ", Asmodifikasiuser='" & userid & "', Asmodifikasitgl = NOW(), Asposting = 0, Aspostingtgl = '1971-01-01 00:00:00', Asjmlrevisi = Asjmlrevisi + 1 WHERE Asid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_AsSearch(PostWsSearch(paramSplit(0), "M5_AsSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M5_AsDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("askontakkode", "c1.kkode")
            Filter = Filter.Replace("askontaknama", "c1.knama")
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
            Dim sumber As String = "As", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Asid, Asnotransaksi FROM M5_As WHERE Asid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT ascabang, aslokasi, assumber, asautonotransaksi, asnotransaksi, astgl"
            sql &= " FROM M5_as"
            sql &= " WHERE asid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("ascabang")
                lokasi = dtNomorNext.Rows(0)("aslokasi")
                sumber = dtNomorNext.Rows(0)("assumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("asautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("asnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("astgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M5_As_Pay WHERE idas='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M5_As WHERE asid ='" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_AsSearch(PostWsSearch(paramSplit(0), "M5_AsSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M5_AsGetdataById(ByVal param As String) As String
        'M5_AsGetdataById Utama --------------------------------------------------------
        'asid, ascabang, aslokasi, asjenis, assumber, asautonotransaksi, asnotransaksi, 
        'astgl, askodepa, askontak, askontakperson, as1alamat1, as1alamat2, as1alamat3, 
        'as2alamat1, as2alamat2, as2alamat3, asbagianterima, astermin, astgljatuhtempo, asidso, 
        'asidip, asnorek, asuraian, ascatatan, asnoref, astglnoref, asmatauang, 
        'askurs, asjumlah, asjumlahvalas, asjumlahbayar, asjumlahbayarvalas, asstatusbayar, astgllunas, 
        'ascostcenter, asdivisi, assubdivisi, asproyek, asstatus, asstatussebelumnya, asjmlrevisi, 
        'ascetakanke, asinputuser, asinputtgl, asmodifikasiuser, asmodifikasitgl, asposting, aspostingtgl, 
        'asisclose, ascustomtext1, ascustomtext2, ascustomtext3, ascustomtext4, ascustomtext5, ascustomint1, 
        'ascustomint2, ascustomint3, ascustomdbl1, ascustomdbl2, ascustomdbl3, ascustomdate1, ascustomdate2, 
        'ascustomdate3, ascabangnama, aslokasinama, askontakkode, askontaknama, asbagianterimakode, asbagianterimanama, 
        'asterminnama, asterminharijatuhtempo, asnotransaksiso, asnotransaksiip, asnoreknama, ascostcenternama, asdivisinama, 
        'assubdivisinama, asproyeknama, asstatusnama, asstatussebelumnyanama, asinputusernama, asmodifikasiusernama, kpkp

        'M5_AsGetdataById Pay --------------------------------------------------------
        'idascarabayar, idas, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, 
        'tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, 
        'idip, isclose, carabayarnama, banknama, rekbanknama, rekgironama, ipnotransaksi

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

        Dim NmMemcached As String = "aplikasi1-M5_as~M5_as_Detail-" & idtransaksi

        'replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "asid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "asid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_as_getdata")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("asid"), 0), sptField,
                     FxDB(drutama("ascabang"), ""), sptField,
                     FxDB(drutama("aslokasi"), ""), sptField,
                     FxDB(drutama("asjenis"), 0), sptField,
                     FxDB(drutama("assumber"), ""), sptField,
                     FxDB(drutama("asautonotransaksi"), 0), sptField,
                     FxDB(drutama("asnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("astgl"), ""), formatTgl), sptField,
                     FxDB(drutama("askodepa"), 0), sptField,
                     FxDB(drutama("askontak"), 0), sptField,
                     FxDB(drutama("askontakperson"), ""), sptField,
                     FxDB(drutama("as1alamat1"), ""), sptField,
                     FxDB(drutama("as1alamat2"), ""), sptField,
                     FxDB(drutama("as1alamat3"), ""), sptField,
                     FxDB(drutama("as2alamat1"), ""), sptField,
                     FxDB(drutama("as2alamat2"), ""), sptField,
                     FxDB(drutama("as2alamat3"), ""), sptField,
                     FxDB(drutama("asbagianterima"), 0), sptField,
                     FxDB(drutama("astermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("astgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("asidso"), 0), sptField,
                     FxDB(drutama("asidip"), 0), sptField,
                     FxDB(drutama("asnorek"), ""), sptField,
                     FxDB(drutama("asuraian"), ""), sptField,
                     FxDB(drutama("ascatatan"), ""), sptField,
                     FxDB(drutama("asnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("astglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("asmatauang"), ""), sptField,
                     FxDB(drutama("askurs"), 0), sptField,
                     FxDB(drutama("asjumlah"), 0), sptField,
                     FxDB(drutama("asjumlahvalas"), 0), sptField,
                     FxDB(drutama("asjumlahbayar"), 0), sptField,
                     FxDB(drutama("asjumlahbayarvalas"), 0), sptField,
                     FxDB(drutama("asstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("astgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("ascostcenter"), ""), sptField,
                     FxDB(drutama("asdivisi"), ""), sptField,
                     FxDB(drutama("assubdivisi"), ""), sptField,
                     FxDB(drutama("asproyek"), ""), sptField,
                     FxDB(drutama("asstatus"), 0), sptField,
                     FxDB(drutama("asstatussebelumnya"), 0), sptField,
                     FxDB(drutama("asjmlrevisi"), 0), sptField,
                     FxDB(drutama("ascetakanke"), 0), sptField,
                     FxDB(drutama("asinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("asinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("asmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("asmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("asposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("aspostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("asisclose"), 0), sptField,
                     FxDB(drutama("ascustomtext1"), ""), sptField,
                     FxDB(drutama("ascustomtext2"), ""), sptField,
                     FxDB(drutama("ascustomtext3"), ""), sptField,
                     FxDB(drutama("ascustomtext4"), ""), sptField,
                     FxDB(drutama("ascustomtext5"), ""), sptField,
                     FxDB(drutama("ascustomint1"), 0), sptField,
                     FxDB(drutama("ascustomint2"), 0), sptField,
                     FxDB(drutama("ascustomint3"), 0), sptField,
                     FxDB(drutama("ascustomdbl1"), 0), sptField,
                     FxDB(drutama("ascustomdbl2"), 0), sptField,
                     FxDB(drutama("ascustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ascustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ascustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ascustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("ascabangnama"), ""), sptField,
                     FxDB(drutama("aslokasinama"), ""), sptField,
                     FxDB(drutama("askontakkode"), ""), sptField,
                     FxDB(drutama("askontaknama"), ""), sptField,
                     FxDB(drutama("asbagianterimakode"), ""), sptField,
                     FxDB(drutama("asbagianterimanama"), ""), sptField,
                     FxDB(drutama("asterminnama"), ""), sptField,
                     FxDB(drutama("asterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("asnotransaksiso"), ""), sptField,
                     FxDB(drutama("asnotransaksiip"), ""), sptField,
                     FxDB(drutama("asnoreknama"), ""), sptField,
                     FxDB(drutama("ascostcenternama"), ""), sptField,
                     FxDB(drutama("asdivisinama"), ""), sptField,
                     FxDB(drutama("assubdivisinama"), ""), sptField,
                     FxDB(drutama("asproyeknama"), ""), sptField,
                     FxDB(drutama("asstatusnama"), ""), sptField,
                     FxDB(drutama("asstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("asinputusernama"), ""), sptField,
                     FxDB(drutama("asmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idascarabayar"), 0), sptField,
                     FxDB(dr("idas"), 0), sptField,
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
                     FxDB(dr("idip"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("carabayarnama"), ""), sptField,
                     FxDB(dr("banknama"), ""), sptField,
                     FxDB(dr("rekbanknama"), ""), sptField,
                     FxDB(dr("rekgironama"), ""), sptField,
                     FxDB(dr("ipnotransaksi"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("asid, ascabang, aslokasi, asjenis, assumber, asautonotransaksi, asnotransaksi, astgl, askodepa, askontak, askontakperson, as1alamat1, as1alamat2, as1alamat3, as2alamat1, as2alamat2, as2alamat3, asbagianterima, astermin, astgljatuhtempo, asidso, asidip, asnorek, asuraian, ascatatan, asnoref, astglnoref, asmatauang, askurs, asjumlah, asjumlahvalas, asjumlahbayar, asjumlahbayarvalas, asstatusbayar, astgllunas, ascostcenter, asdivisi, assubdivisi, asproyek, asstatus, asstatussebelumnya, asjmlrevisi, ascetakanke, asinputuser, asinputtgl, asmodifikasiuser, asmodifikasitgl, asposting, aspostingtgl, asisclose, ascustomtext1, ascustomtext2, ascustomtext3, ascustomtext4, ascustomtext5, ascustomint1, ascustomint2, ascustomint3, ascustomdbl1, ascustomdbl2, ascustomdbl3, ascustomdate1, ascustomdate2, ascustomdate3, ascabangnama, aslokasinama, askontakkode, askontaknama, asbagianterimakode, asbagianterimanama, asterminnama, asterminharijatuhtempo, asnotransaksiso, asnotransaksiip, asnoreknama, ascostcenternama, asdivisinama, assubdivisinama, asproyeknama, asstatusnama, asstatussebelumnyanama, asinputusernama, asmodifikasiusernama, kpkp"), sptSubParam, ReplaceMapping("idascarabayar, idas, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, idip, isclose, carabayarnama, banknama, rekbanknama, rekgironama, ipnotransaksi"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_AsSearch(ByVal param As String) As String
        'M5_AsSearch --------------------------------------------------------
        'asid, ascabang, aslokasi, asjenis, assumber, asautonotransaksi, asnotransaksi, 
        'astgl, askodepa, askontak, askontakperson, as1alamat1, as1alamat2, as1alamat3, 
        'as2alamat1, as2alamat2, as2alamat3, asbagianterima, astermin, astgljatuhtempo, asidso, 
        'asidip, asnorek, asuraian, ascatatan, asnoref, astglnoref, asmatauang, 
        'askurs, asjumlah, asjumlahvalas, asjumlahbayar, asjumlahbayarvalas, asstatusbayar, astgllunas, 
        'ascostcenter, asdivisi, assubdivisi, asproyek, asstatus, asstatussebelumnya, asjmlrevisi, 
        'ascetakanke, asinputuser, asinputtgl, asmodifikasiuser, asmodifikasitgl, asposting, aspostingtgl, 
        'asisclose, ascabangnama, aslokasinama, asjenisnama, askontakkode, askontaknama, asbagianterimakode, 
        'asbagianterimanama, sonotransaksi, ipnotransaksi, asnoreknama, asstatusnama, asstatussebelumnyanama, asinputusernama, 
        'asmodifikasiusernama

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
            Filter = Filter.Replace("askontakkode", "c1.kkode")
            Filter = Filter.Replace("askontaknama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_as_v")

        dt = AmbilData("aplikasi1-M5_As", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("asid"), 0), sptField,
                     FxDB(dr("ascabang"), ""), sptField,
                     FxDB(dr("aslokasi"), ""), sptField,
                     FxDB(dr("asjenis"), 0), sptField,
                     FxDB(dr("assumber"), ""), sptField,
                     FxDB(dr("asautonotransaksi"), 0), sptField,
                     FxDB(dr("asnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("astgl"), ""), formatTgl), sptField,
                     FxDB(dr("askodepa"), 0), sptField,
                     FxDB(dr("askontak"), 0), sptField,
                     FxDB(dr("askontakperson"), ""), sptField,
                     FxDB(dr("as1alamat1"), ""), sptField,
                     FxDB(dr("as1alamat2"), ""), sptField,
                     FxDB(dr("as1alamat3"), ""), sptField,
                     FxDB(dr("as2alamat1"), ""), sptField,
                     FxDB(dr("as2alamat2"), ""), sptField,
                     FxDB(dr("as2alamat3"), ""), sptField,
                     FxDB(dr("asbagianterima"), 0), sptField,
                     FxDB(dr("astermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("astgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("asidso"), 0), sptField,
                     FxDB(dr("asidip"), 0), sptField,
                     FxDB(dr("asnorek"), ""), sptField,
                     FxDB(dr("asuraian"), ""), sptField,
                     FxDB(dr("ascatatan"), ""), sptField,
                     FxDB(dr("asnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("astglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("asmatauang"), ""), sptField,
                     FxDB(dr("askurs"), 0), sptField,
                     FxDB(dr("asjumlah"), 0), sptField,
                     FxDB(dr("asjumlahvalas"), 0), sptField,
                     FxDB(dr("asjumlahbayar"), 0), sptField,
                     FxDB(dr("asjumlahbayarvalas"), 0), sptField,
                     FxDB(dr("asstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("astgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("ascostcenter"), ""), sptField,
                     FxDB(dr("asdivisi"), ""), sptField,
                     FxDB(dr("assubdivisi"), ""), sptField,
                     FxDB(dr("asproyek"), ""), sptField,
                     FxDB(dr("asstatus"), 0), sptField,
                     FxDB(dr("asstatussebelumnya"), 0), sptField,
                     FxDB(dr("asjmlrevisi"), 0), sptField,
                     FxDB(dr("ascetakanke"), 0), sptField,
                     FxDB(dr("asinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("asinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("asmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("asmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("asposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("aspostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("asisclose"), 0), sptField,
                     FxDB(dr("ascabangnama"), ""), sptField,
                     FxDB(dr("aslokasinama"), ""), sptField,
                     FxDB(dr("asjenisnama"), ""), sptField,
                     FxDB(dr("askontakkode"), ""), sptField,
                     FxDB(dr("askontaknama"), ""), sptField,
                     FxDB(dr("asbagianterimakode"), ""), sptField,
                     FxDB(dr("asbagianterimanama"), ""), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("ipnotransaksi"), ""), sptField,
                     FxDB(dr("asnoreknama"), ""), sptField,
                     FxDB(dr("asstatusnama"), ""), sptField,
                     FxDB(dr("asstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("asinputusernama"), ""), sptField,
                     FxDB(dr("asmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("asid, ascabang, aslokasi, asjenis, assumber, asautonotransaksi, asnotransaksi, astgl, askodepa, askontak, askontakperson, as1alamat1, as1alamat2, as1alamat3, as2alamat1, as2alamat2, as2alamat3, asbagianterima, astermin, astgljatuhtempo, asidso, asidip, asnorek, asuraian, ascatatan, asnoref, astglnoref, asmatauang, askurs, asjumlah, asjumlahvalas, asjumlahbayar, asjumlahbayarvalas, asstatusbayar, astgllunas, ascostcenter, asdivisi, assubdivisi, asproyek, asstatus, asstatussebelumnya, asjmlrevisi, ascetakanke, asinputuser, asinputtgl, asmodifikasiuser, asmodifikasitgl, asposting, aspostingtgl, asisclose, ascabangnama, aslokasinama, asjenisnama, askontakkode, askontaknama, asbagianterimakode, asbagianterimanama, sonotransaksi, ipnotransaksi, asnoreknama, asstatusnama, asstatussebelumnyanama, asinputusernama, asmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_AsTerkait(ByVal param As String) As String
        'm5_AsTerkait --------------------------------------------------------
        'asid, asnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
        'modifikasitglterkait, jenisterkait

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strasrt(3), formatTgl(4), formatTglWaktu(5)

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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        Dim idtransaksi As String = ""
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "asid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2) & " AND asid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "asid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.m5_as_terkait(Filter)


        dt = AmbilData("aplikasi1-m5_as_Terkait", , sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("asid"), 0), sptField,
                     FxDB(dr("asnotransaksi"), ""), sptField,
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
            result(2) = "Related AS data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("asid, asnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_AsTerkait_S(ByVal param As String) As String
        'm5_AsTerkait --------------------------------------------------------
        'asid, asnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
        'modifikasitglterkait, jenisterkait

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strasrt(3), formatTgl(4), formatTglWaktu(5)

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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        Dim idtransaksi As String = ""
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "asid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2) & " AND asid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "asid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.m5_as_terkait(Filter)


        dt = AmbilData("aplikasi1-m5_as_Terkait", , sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("asid"), 0), sptField,
                     FxDB(dr("asnotransaksi"), ""), sptField,
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
            result(2) = "Related AS data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("asid, asnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function


    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstandingIP As String, ByVal ftOutstandingIP As String) As String
        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, notransaksi As String = "", sumber As String = "", matauang As String = "", sisa As Double = 0
        Dim filterLookup As String = "", urutan As String = ""

        'VALIDASI OUTSTANDING ---------------------------------------
        'IP
        If Len(ftExistOutstandingIP) > 0 Then 'ftExistOutstanding = rowExists, ipid, ipsumber, ipnotransaksi
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingIP)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                notransaksi = dtval.Rows(0)("ipnotransaksi")
                sumber = dtval.Rows(0)("ipsumber")

                filterLookup = "idpi=" & dtval.Rows(0)("ipid")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " doesn't exists/yet approved in IP" : GoTo selesai
            End If
        End If

        'PERBANDINGAN ANTARA SISA TRANSAKSI YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
        If Len(ftOutstandingIP) > 0 Then
            sql = "SELECT ip.ipid, ip.ipsumber, ip.ipnotransaksi, ip.ipmatauang, (CASE ip.ipmatauang WHEN s.snilai THEN ip.ipjumlah - ip.ipjumlahbayar ELSE ip.ipjumlahvalas - ip.ipjumlahbayarvalas END) ipsisatransaksi FROM m5_ip ip LEFT JOIN m0_setting s ON s.smodule =0 AND s.sgrup='accounting' AND s.skode = 'MataUangFungsional' WHERE " & ftOutstandingIP
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                notransaksi = dtval.Rows(0)("ipnotransaksi")
                sumber = dtval.Rows(0)("ipsumber")
                sisa = dtval.Rows(0)("ipsisatransaksi")
                matauang = dtval.Rows(0)("ipmatauang")

                filterLookup = "idip=" & dtval.Rows(0)("ipid")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " exceeds the amount of payment in IP, payment available " & matauang & " " & FormatNumber(sisa) : GoTo selesai
            End If
        End If
        'END OF VALIDASI OUTSTANDING --------------------------------

selesai:
        Return errmessage
    End Function

    <WebMethod()>
    Public Function M5_AsSimpanOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean, tglLunas As String = ""

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
        'asid(0) As Integer, ascabang(1) As String, aslokasi(2) As String, asjenis(3) As Integer, assumber(4) As String, 
        'asautonotransaksi(5) As Integer, asnotransaksi(6) As String, astgl(7) As Date, askodepa(8) As Integer, askontak(9) As Integer, 
        'askontakperson(10) As String, as1alamat1(11) As String, as1alamat2(12) As String, as1alamat3(13) As String, as2alamat1(14) As String, 
        'as2alamat2(15) As String, as2alamat3(16) As String, asbagianterima(17) As Integer, astermin(18) As String, astgljatuhtempo(19) As Date, 
        'asidso(20) As Integer, asidip(21) As Integer, asnorek(22) As String, asuraian(23) As String, ascatatan(24) As String, 
        'asnoref(25) As String, astglnoref(26) As Date, asmatauang(27) As String, askurs(28) As Double, asjumlah(29) As Double, 
        'asjumlahvalas(30) As Double, asjumlahbayar(31) As Double, asjumlahbayarvalas(32) As Double, asstatusbayar(33) As Integer, astgllunas(34) As Date, 
        'ascostcenter(35) As String, asdivisi(36) As String, assubdivisi(37) As String, asproyek(38) As String, asstatus(39) As Integer, 
        'asstatussebelumnya(40) As Integer, asjmlrevisi(41) As Integer, ascetakanke(42) As Integer, asinputuser(43) As Integer, asinputtgl(44) As DateTime, 
        'asmodifikasiuser(45) As Integer, asmodifikasitgl(46) As DateTime, asposting(47) As Integer, asisclose(48) As Integer, ascustomtext1(49) As String, 
        'ascustomtext2(50) As String, ascustomtext3(51) As String, ascustomtext4(52) As String, ascustomtext5(53) As String, ascustomint1(54) As Integer, 
        'ascustomint2(55) As Integer, ascustomint3(56) As Integer, ascustomdbl1(57) As Double, ascustomdbl2(58) As Double, ascustomdbl3(59) As Double, 
        'ascustomdate1(60) As Date, ascustomdate2(61) As Date, ascustomdate3(62) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'asid, ascabang, aslokasi, asjenis, assumber, asautonotransaksi, asnotransaksi, 
        'astgl, askodepa, askontak, askontakperson, as1alamat1, as1alamat2, as1alamat3, 
        'as2alamat1, as2alamat2, as2alamat3, asbagianterima, astermin, astgljatuhtempo, asidso, 
        'asidip, asnorek, asuraian, ascatatan, asnoref, astglnoref, asmatauang, 
        'askurs, asjumlah, asjumlahvalas, asjumlahbayar, asjumlahbayarvalas, asstatusbayar, astgllunas, 
        'ascostcenter, asdivisi, assubdivisi, asproyek, asstatus, asstatussebelumnya, asjmlrevisi, 
        'ascetakanke, asinputuser, asinputtgl, asmodifikasiuser, asmodifikasitgl, asposting, asisclose, 
        'ascustomtext1, ascustomtext2, ascustomtext3, ascustomtext4, ascustomtext5, ascustomint1, ascustomint2, 
        'ascustomint3, ascustomdbl1, ascustomdbl2, ascustomdbl3, ascustomdate1, ascustomdate2, ascustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 63) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'asid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "asid required numeric." : GoTo selesai
        End If
        'asjenis(3) As Integer
        If (IsNumeric(dataUtama(3)) = False) Then
            result(2) = "asjenis required numeric." : GoTo selesai
        End If
        'asautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "asautonotransaksi required numeric." : GoTo selesai
        End If
        'astgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "astgl required date." : GoTo selesai
        End If
        'askodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "askodepa required numeric." : GoTo selesai
        End If
        'askontak(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "askontak required numeric." : GoTo selesai
        End If
        'asbagianterima(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "asbagianterima required numeric." : GoTo selesai
        End If
        'astgljatuhtempo(19) As Date
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "astgljatuhtempo required date." : GoTo selesai
        End If
        'asidso(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "asidso required numeric." : GoTo selesai
        End If
        'asidip(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "asidip required numeric." : GoTo selesai
        End If
        'astglnoref(26) As Date
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "astglnoref required date." : GoTo selesai
        End If
        'askurs(28) As Double
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "askurs required numeric." : GoTo selesai
        End If
        'asjumlah(29) As Double
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "asjumlah required numeric." : GoTo selesai
        End If
        'asjumlahvalas(30) As Double
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "asjumlahvalas required numeric." : GoTo selesai
        End If
        'asjumlahbayar(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "asjumlahbayar required numeric." : GoTo selesai
        End If
        'asjumlahbayarvalas(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "asjumlahbayarvalas required numeric." : GoTo selesai
        End If
        'asstatusbayar(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "asstatusbayar required numeric." : GoTo selesai
        End If
        'astgllunas(34) As Date
        If (IsDate(dataUtama(34)) = False) Then
            result(2) = "astgllunas required date." : GoTo selesai
        End If
        'asstatus(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "asstatus required numeric." : GoTo selesai
        End If
        'asstatussebelumnya(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "asstatussebelumnya required numeric." : GoTo selesai
        End If
        'asjmlrevisi(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "asjmlrevisi required numeric." : GoTo selesai
        End If
        'ascetakanke(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "ascetakanke required numeric." : GoTo selesai
        End If
        'asinputuser(43) As Integer
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "asinputuser required numeric." : GoTo selesai
        End If
        'asinputtgl(44) As DateTime
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "asinputtgl required date." : GoTo selesai
        End If
        'asmodifikasiuser(45) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "asmodifikasiuser required numeric." : GoTo selesai
        End If
        'asmodifikasitgl(46) As DateTime
        If (IsDate(dataUtama(46)) = False) Then
            result(2) = "asmodifikasitgl required date." : GoTo selesai
        End If
        'asposting(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "asposting required numeric." : GoTo selesai
        End If
        'asisclose(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "asisclose required numeric." : GoTo selesai
        End If
        'ascustomint1(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "ascustomint1 required numeric." : GoTo selesai
        End If
        'ascustomint2(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "ascustomint2 required numeric." : GoTo selesai
        End If
        'ascustomint3(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "ascustomint3 required numeric." : GoTo selesai
        End If
        'ascustomdbl1(57) As Double
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "ascustomdbl1 required numeric." : GoTo selesai
        End If
        'ascustomdbl2(58) As Double
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "ascustomdbl2 required numeric." : GoTo selesai
        End If
        'ascustomdbl3(59) As Double
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "ascustomdbl3 required numeric." : GoTo selesai
        End If
        'ascustomdate1(60) As Date
        If (IsDate(dataUtama(60)) = False) Then
            result(2) = "ascustomdate1 required date." : GoTo selesai
        End If
        'ascustomdate2(61) As Date
        If (IsDate(dataUtama(61)) = False) Then
            result(2) = "ascustomdate2 required date." : GoTo selesai
        End If
        'ascustomdate3(62) As Date
        If (IsDate(dataUtama(62)) = False) Then
            result(2) = "ascustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'ascabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "ascabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "ascabang should not be more than 25 character." : GoTo selesai
        End If

        'aslokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "aslokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "aslokasi should not be more than 25 character." : GoTo selesai
        End If

        'assumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "assumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "assumber should not be more than 10 character." : GoTo selesai
        End If

        'asnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "asnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "asnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'astgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "astgl can't be empty" : GoTo selesai
        End If
        'SET TGLTRANSAKSI ---> UNTUK UPDATE TGL LUNAS TRANSAKSI
        tglLunas = AsFormatTanggal(dataUtama(7))

        'astgljatuhtempo(19) As Date
        If Len(dataUtama(19)) = 0 Then
            result(2) = "astgljatuhtempo can't be empty" : GoTo selesai
        End If

        'asnorek(22) As String
        If Len(dataUtama(22)) = 0 Then
            result(2) = "asnorek can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(22)) > 25 Then
            result(2) = "asnorek should not be more than 25 character." : GoTo selesai
        End If

        'astglnoref(26) As Date
        If Len(dataUtama(26)) = 0 Then
            result(2) = "astglnoref can't be empty" : GoTo selesai
        End If

        'asmatauang(27) As String
        If Len(dataUtama(27)) = 0 Then
            result(2) = "asmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(27)) > 25 Then
            result(2) = "asmatauang should not be more than 25 character." : GoTo selesai
        End If

        'askurs(28) As Double
        If Len(dataUtama(28)) = 0 Then
            result(2) = "askurs can't be empty" : GoTo selesai
        End If

        'asjumlah(29) As Double
        If Len(dataUtama(29)) = 0 Then
            result(2) = "asjumlah can't be empty" : GoTo selesai
        End If

        'asjumlahvalas(30) As Double
        If Len(dataUtama(30)) = 0 Then
            result(2) = "asjumlahvalas can't be empty" : GoTo selesai
        End If

        'asjumlahbayar(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "asjumlahbayar can't be empty" : GoTo selesai
        End If

        'asjumlahbayarvalas(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "asjumlahbayarvalas can't be empty" : GoTo selesai
        End If

        'astgllunas(34) As Date
        If Len(dataUtama(34)) = 0 Then
            result(2) = "astgllunas can't be empty" : GoTo selesai
        End If

        'asinputtgl(44) As DateTime
        If Len(dataUtama(44)) = 0 Then
            result(2) = "asinputtgl can't be empty" : GoTo selesai
        End If

        'asmodifikasitgl(46) As DateTime
        If Len(dataUtama(46)) = 0 Then
            result(2) = "asmodifikasitgl can't be empty" : GoTo selesai
        End If

        'ascustomdbl1(57) As Double
        If Len(dataUtama(57)) = 0 Then
            result(2) = "ascustomdbl1 can't be empty" : GoTo selesai
        End If

        'ascustomdbl2(58) As Double
        If Len(dataUtama(58)) = 0 Then
            result(2) = "ascustomdbl2 can't be empty" : GoTo selesai
        End If

        'ascustomdbl3(59) As Double
        If Len(dataUtama(59)) = 0 Then
            result(2) = "ascustomdbl3 can't be empty" : GoTo selesai
        End If

        'ascustomdate1(60) As Date
        If Len(dataUtama(60)) = 0 Then
            result(2) = "ascustomdate1 can't be empty" : GoTo selesai
        End If

        'ascustomdate2(61) As Date
        If Len(dataUtama(61)) = 0 Then
            result(2) = "ascustomdate2 can't be empty" : GoTo selesai
        End If

        'ascustomdate3(62) As Date
        If Len(dataUtama(62)) = 0 Then
            result(2) = "ascustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "asid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aslokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asjenis", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "assumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "asnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "astgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "askodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "askontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "askontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "as1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "as1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "as1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "as2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "as2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "as2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asbagianterima", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "astermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "astgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asidso", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "asidip", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "asnorek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "astglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "askurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asjumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "asjumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "asjumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asjumlahbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asstatusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "astgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascostcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "assubdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asproyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "asstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "asjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ascetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "asinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "asinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "asmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "asposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "asisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ascustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ascustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ascustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ascustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ascustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "asid~ascabang~aslokasi~asjenis~assumber~asautonotransaksi~asnotransaksi~astgl~askodepa~askontak~askontakperson~as1alamat1~as1alamat2~as1alamat3~as2alamat1~as2alamat2~as2alamat3~asbagianterima~astermin~astgljatuhtempo~asidso~asidip~asnorek~asuraian~ascatatan~asnoref~astglnoref~asmatauang~askurs~asjumlah~asjumlahvalas~asjumlahbayar~asjumlahbayarvalas~asstatusbayar~astgllunas~ascostcenter~asdivisi~assubdivisi~asproyek~asstatus~asstatussebelumnya~asjmlrevisi~ascetakanke~asinputuser~asinputtgl~asmodifikasiuser~asmodifikasitgl~asposting~asisclose~ascustomtext1~ascustomtext2~ascustomtext3~ascustomtext4~ascustomtext5~ascustomint1~ascustomint2~ascustomint3~ascustomdbl1~ascustomdbl2~ascustomdbl3~ascustomdate1~ascustomdate2~ascustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idascarabayar(0) As Integer, idas(1) As Integer, carabayar(2) As Integer, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, nogiro(7) As String, tgljt(8) As Date, bank(9) As String, 
        'noacbank(10) As String, rekbank(11) As String, rekgiro(12) As String, catatan(13) As String, urutan(14) As Integer, 
        'idip(15) As Integer, isclose(16) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idascarabayar, idas, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, idip, isclose

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idascarabayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idas", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "idip", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt64)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'AMBIL MATA UANG FUNGSIONAL DARI SETTING ================
        Dim MUFungsional As String = ""
        Dim dtSetting As DataTable = AsDataTableAmbilDariDB("SELECT snilai FROM m0_setting WHERE (smodule='0') AND (sgrup='accounting') AND (skode='MataUangFungsional')")
        If dtSetting.Rows.Count > 0 Then
            MUFungsional = dtSetting.Rows(0)(0)
        Else
            result(2) = "Can't found 'Functional Currency' in Setting." : GoTo selesai
        End If
        'END OF AMBIL MATA UANG FUNGSIONAL DARI SETTING =========

        'VARIABEL VALIDASI OUTSTANDING, IP
        Dim ftExistOutstandingIP As String = "", ftOutstandingIP As String = "", updNilaiIP As String = "", updNilaiValasIP As String = "", updFilterIP As String = "", updTglLunasIP As String = ""
        Dim idip As Integer = 0, matauangDetail As String = ""
        Dim Outstanding As Double = 0, OutstandingValas As Double = 0

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 17) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idascarabayar(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "idascarabayar required numeric." : GoTo selesai
            End If
            'idas(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "idas required numeric." : GoTo selesai
            End If
            'carabayar(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "carabayar required numeric." : GoTo selesai
            End If
            'kurs(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "kurs required numeric." : GoTo selesai
            End If
            'jumlah(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "jumlah required numeric." : GoTo selesai
            End If
            'jumlahvalas(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "jumlahvalas required numeric." : GoTo selesai
            End If
            'tgljt(8) As Date
            If (IsDate(dataRowDetail(8)) = False) Then
                result(2) = "tgljt required date." : GoTo selesai
            End If
            'urutan(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "urutan required numeric." : GoTo selesai
            End If
            'idip(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "idip required numeric." : GoTo selesai
            End If
            'isclose(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "isclose required numeric." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idascarabayar~idas~carabayar~matauang~kurs~jumlah~jumlahvalas~nogiro~tgljt~bank~noacbank~rekbank~rekgiro~catatan~urutan~idip~isclose", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'Set variabel
            'idip(15) As Integer     , matauang(3) As String
            idip = dataRowDetail(15) : matauangDetail = dataRowDetail(3)

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'VALIDASI OUTSTANDING -------------------------
            If idip <> 0 Then 'IP
                '1. CEK DATA EXIST
                ftExistOutstandingIP = IIf(Len(ftExistOutstandingIP.ToString) = 0, "", ftExistOutstandingIP & " UNION ")
                ftExistOutstandingIP = String.Concat(ftExistOutstandingIP, "SELECT EXISTS(SELECT 1 FROM m5_ip WHERE ipid = '" & idip & "' AND (ipstatus = 2 OR ipstatus = 3 OR ipstatus = 4 OR ipstatus = 7) LIMIT 1) as rowExists, ipid, ipsumber, ipnotransaksi FROM m5_ip WHERE ipid = '" & idip & "'")

                '2. CEK JML OUTSTANDING
                Outstanding = AsDataTableDSum(dtdetail, "jumlah", "idip=" & idip)
                OutstandingValas = AsDataTableDSum(dtdetail, "jumlahvalas", "idip=" & idip)
                ftOutstandingIP = IIf(Len(ftOutstandingIP.ToString) = 0, "", ftOutstandingIP & " OR ")
                ftOutstandingIP = String.Concat(ftOutstandingIP, " (ip.ipid = '" & idip & "' AND (CASE ip.ipmatauang WHEN s.snilai THEN " & Outstanding & " > ip.ipjumlah - ip.ipjumlahbayar ELSE " & OutstandingValas & " > ip.ipjumlahvalas - ip.ipjumlahbayarvalas END)) ")

                '3. SET NILAI UPDATE OUTSTANDING
                updNilaiIP = String.Concat("WHEN '" & idip & "' THEN ROUND(ipjumlahbayar + '" & Outstanding & "', 5) ", updNilaiIP)
                updNilaiValasIP = String.Concat("WHEN '" & idip & "' THEN ROUND(ip.ipjumlahbayarvalas + '" & OutstandingValas & "', 5) ", updNilaiValasIP)

                '4. SET FILTER UPDATE OUTSTANDING
                updFilterIP = IIf(Len(updFilterIP.ToString) = 0, "", updFilterIP & " OR ")
                updFilterIP = String.Concat(updFilterIP, "(ip.ipid = '" & idip & "')")

                '5. SET NILAI TGLLUNAS TRANSAKSI
                If matauangDetail = MUFungsional Then
                    updTglLunasIP = String.Concat(" WHEN '" & idip & "' THEN (CASE WHEN ROUND(ip.ipjumlahbayar + '" & Outstanding & "', 5) >= ip.ipjumlah THEN '" & FixQuotes(tglLunas) & "' ELSE ip.iptgllunas END) ", updTglLunasIP)
                Else
                    updTglLunasIP = String.Concat(" WHEN '" & idip & "' THEN (CASE WHEN ROUND(ip.ipjumlahbayarvalas + '" & OutstandingValas & "', 5) >= ip.ipjumlahvalas THEN '" & FixQuotes(tglLunas) & "' ELSE ip.iptgllunas END) ", updTglLunasIP)
                End If
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

                'CEK PERIODE AKUNTANSI ==================================
                Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("astgl")), AsFormatTanggal(drutama("astgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "asmatauang", "asnorek", dtdetail, "rekbank")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("astermin").ToString, AsFormatTanggal(drutama("astgl")), "astgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("astgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                'HITUNG TOTAL BERDASARKAN DATA DETAIL ===================
                drutama("asjumlah") = AsDataTableDSum(dtdetail, "jumlah")
                drutama("asjumlahvalas") = AsDataTableDSum(dtdetail, "jumlahvalas")
                'END OF HITUNG TOTAL BERDASARKAN DATA DETAIL ============


                ''CEK TOTAL UTAMA DAN DETAIL =============================
                'Dim jumlah As Double = AsDataTableDSum(dtdetail, "jumlah")
                'Dim jumlahvalas As Double = AsDataTableDSum(dtdetail, "jumlahvalas")
                'If Double.Parse(drutama("asjumlah")) <> jumlah Then
                '    result(2) = "Total amount of main and detail are not balanced" : Trans.Rollback() : GoTo selesai
                'ElseIf Double.Parse(drutama("asjumlahvalas")) <> jumlahvalas Then
                '    result(2) = "Total amount of foreign main and detail are not balanced" : Trans.Rollback() : GoTo selesai
                'End If
                ''END OF CEK TOTAL UTAMA DAN DETAIL ======================


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("asstatus") = 2 Then
                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstandingIP, ftOutstandingIP)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                If isUpdate Then
                    result(4) = drutama("asid")
                    notransaksi = drutama("asnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(asid), asnotransaksi FROM M5_as WHERE asid='" & result(4) & "' AND asstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(asid) FROM m5_as WHERE asnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_as_history
                        Dim rsSimpanHistory As String = SimpanHistory.M5_As_HistorySimpan("" & paramSplit(0) & "★M5_As_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("assumber")) & "▼" & FixQuotes(drutama("asid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_As set ascabang  = '" & FixQuotes(drutama("ascabang")) & "', aslokasi  = '" & FixQuotes(drutama("aslokasi")) & "', asjenis  = " & drutama("asjenis") & ", assumber  = '" & FixQuotes(drutama("assumber")) & "', asautonotransaksi  = " & drutama("asautonotransaksi") & ", asnotransaksi  = '" & notransaksi & "', astgl  = '" & FixQuotes(AsFormatTanggal(drutama("astgl"))) & "', askodepa  = " & drutama("askodepa") & ", askontak  = " & drutama("askontak") & ", askontakperson  = '" & FixQuotes(drutama("askontakperson")) & "', as1alamat1  = '" & FixQuotes(drutama("as1alamat1")) & "', as1alamat2  = '" & FixQuotes(drutama("as1alamat2")) & "', as1alamat3  = '" & FixQuotes(drutama("as1alamat3")) & "', as2alamat1  = '" & FixQuotes(drutama("as2alamat1")) & "', as2alamat2  = '" & FixQuotes(drutama("as2alamat2")) & "', as2alamat3  = '" & FixQuotes(drutama("as2alamat3")) & "', asbagianterima  = " & drutama("asbagianterima") & ", astermin  = '" & FixQuotes(drutama("astermin")) & "', astgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("astgljatuhtempo"))) & "', asidso  = " & drutama("asidso") & ", asidip  = " & drutama("asidip") & ", asnorek  = '" & FixQuotes(drutama("asnorek")) & "', asuraian  = '" & FixQuotes(drutama("asuraian")) & "', ascatatan  = '" & FixQuotes(drutama("ascatatan")) & "', asnoref  = '" & FixQuotes(drutama("asnoref")) & "', astglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("astglnoref"))) & "', asmatauang  = '" & FixQuotes(drutama("asmatauang")) & "', askurs  = '" & FixDouble(drutama("askurs")) & "', asjumlah  = '" & FixDouble(drutama("asjumlah")) & "', asjumlahvalas  = '" & FixDouble(drutama("asjumlahvalas")) & "', asjumlahbayar  = '" & FixDouble(drutama("asjumlahbayar")) & "', asjumlahbayarvalas  = '" & FixDouble(drutama("asjumlahbayarvalas")) & "', asstatusbayar  = " & drutama("asstatusbayar") & ", astgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("astgllunas"))) & "', ascostcenter  = '" & FixQuotes(drutama("ascostcenter")) & "', asdivisi  = '" & FixQuotes(drutama("asdivisi")) & "', assubdivisi  = '" & FixQuotes(drutama("assubdivisi")) & "', asproyek  = '" & FixQuotes(drutama("asproyek")) & "', asstatus  = " & drutama("asstatus") & ", asstatussebelumnya  = " & drutama("asstatussebelumnya") & ", asjmlrevisi  = asjmlrevisi+1, ascetakanke  = " & drutama("ascetakanke") & ", asmodifikasiuser  = " & drutama("asmodifikasiuser") & ", asmodifikasitgl  = NOW(), asposting  = 0, ascustomtext1  = '" & FixQuotes(drutama("ascustomtext1")) & "', ascustomtext2  = '" & FixQuotes(drutama("ascustomtext2")) & "', ascustomtext3  = '" & FixQuotes(drutama("ascustomtext3")) & "', ascustomtext4  = '" & FixQuotes(drutama("ascustomtext4")) & "', ascustomtext5  = '" & FixQuotes(drutama("ascustomtext5")) & "', ascustomint1  = " & drutama("ascustomint1") & ", ascustomint2  = " & drutama("ascustomint2") & ", ascustomint3  = " & drutama("ascustomint3") & ", ascustomdbl1  = '" & FixDouble(drutama("ascustomdbl1")) & "', ascustomdbl2  = '" & FixDouble(drutama("ascustomdbl2")) & "', ascustomdbl3  = '" & FixDouble(drutama("ascustomdbl3")) & "', ascustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("ascustomdate1"))) & "', ascustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("ascustomdate2"))) & "', ascustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("ascustomdate3"))) & "' where asid = '" & drutama("asid") & "'"
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

                    If drutama("asautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("ascabang"), drutama("aslokasi"), drutama("assumber"), drutama("astgl"))
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
                        notransaksi = drutama("asnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(asid) FROM m5_as WHERE asnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_As (ascabang, aslokasi, asjenis, assumber, asautonotransaksi, asnotransaksi, astgl, askodepa, askontak, askontakperson, as1alamat1, as1alamat2, as1alamat3, as2alamat1, as2alamat2, as2alamat3, asbagianterima, astermin, astgljatuhtempo, asidso, asidip, asnorek, asuraian, ascatatan, asnoref, astglnoref, asmatauang, askurs, asjumlah, asjumlahvalas, asjumlahbayar, asjumlahbayarvalas, asstatusbayar, astgllunas, ascostcenter, asdivisi, assubdivisi, asproyek, asstatus, asstatussebelumnya, asjmlrevisi, ascetakanke, asinputuser, asinputtgl, asmodifikasiuser, asmodifikasitgl, asposting, asisclose, ascustomtext1, ascustomtext2, ascustomtext3, ascustomtext4, ascustomtext5, ascustomint1, ascustomint2, ascustomint3, ascustomdbl1, ascustomdbl2, ascustomdbl3, ascustomdate1, ascustomdate2, ascustomdate3) values('" & FixQuotes(drutama("ascabang")) & "', '" & FixQuotes(drutama("aslokasi")) & "', " & drutama("asjenis") & ", '" & FixQuotes(drutama("assumber")) & "', " & drutama("asautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("astgl"))) & "', " & drutama("askodepa") & ", " & drutama("askontak") & ", '" & FixQuotes(drutama("askontakperson")) & "', '" & FixQuotes(drutama("as1alamat1")) & "', '" & FixQuotes(drutama("as1alamat2")) & "', '" & FixQuotes(drutama("as1alamat3")) & "', '" & FixQuotes(drutama("as2alamat1")) & "', '" & FixQuotes(drutama("as2alamat2")) & "', '" & FixQuotes(drutama("as2alamat3")) & "', " & drutama("asbagianterima") & ", '" & FixQuotes(drutama("astermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("astgljatuhtempo"))) & "', " & drutama("asidso") & ", " & drutama("asidip") & ", '" & FixQuotes(drutama("asnorek")) & "', '" & FixQuotes(drutama("asuraian")) & "', '" & FixQuotes(drutama("ascatatan")) & "', '" & FixQuotes(drutama("asnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("astglnoref"))) & "', '" & FixQuotes(drutama("asmatauang")) & "', '" & FixDouble(drutama("askurs")) & "', '" & FixDouble(drutama("asjumlah")) & "', '" & FixDouble(drutama("asjumlahvalas")) & "', '" & FixDouble(drutama("asjumlahbayar")) & "', '" & FixDouble(drutama("asjumlahbayarvalas")) & "', " & drutama("asstatusbayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("astgllunas"))) & "', '" & FixQuotes(drutama("ascostcenter")) & "', '" & FixQuotes(drutama("asdivisi")) & "', '" & FixQuotes(drutama("assubdivisi")) & "', '" & FixQuotes(drutama("asproyek")) & "', " & drutama("asstatus") & ", " & drutama("asstatussebelumnya") & ", " & drutama("asjmlrevisi") & ", " & drutama("ascetakanke") & ", " & drutama("asinputuser") & ", NOW(), " & drutama("asmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("asisclose") & ", '" & FixQuotes(drutama("ascustomtext1")) & "', '" & FixQuotes(drutama("ascustomtext2")) & "', '" & FixQuotes(drutama("ascustomtext3")) & "', '" & FixQuotes(drutama("ascustomtext4")) & "', '" & FixQuotes(drutama("ascustomtext5")) & "', " & drutama("ascustomint1") & ", " & drutama("ascustomint2") & ", " & drutama("ascustomint3") & ", '" & FixDouble(drutama("ascustomdbl1")) & "', '" & FixDouble(drutama("ascustomdbl2")) & "', '" & FixDouble(drutama("ascustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ascustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ascustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ascustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select asid from M5_as where asnotransaksi='" & notransaksi & "' AND asinputuser= '" & userid & "' order by asmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_As_Pay where idas = '" & result(4) & "'"
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
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idascarabayar") & ", " & result(4) & ", " & dr1("carabayar") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idip") & ", " & dr1("isclose") & ")")
                        'QUERY UNTUK INSERT GIRO
                        If dr1("carabayar") = 2 Then
                            strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", ", "))
                            strGiro.Append("('" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(drutama("assumber")) & "', " & result(4) & ", '" & FixQuotes(notransaksi) & "', " & drutama("askontak") & ", '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', " & 0 & ", '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', " & 0 & ", " & 0 & ", " & dr1("urutan") & ")")
                        End If
                    Next
                    sql = "Insert into M5_As_Pay(idascarabayar, idas, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, idip, isclose) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'INSERT GIRO JIKA STATUS APPROVED DAN CARABAYAR = 2
                    If drutama("asstatus") = 2 And Len(strGiro.ToString) > 0 Then
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

                If drutama("asstatus") = 2 Then
                    'UPDATE OUTSTANDING TRANSAKSI ===================================================
                    If Len(updNilaiIP) > 0 Then 'IP
                        'TRANSAKSI
                        sql = "UPDATE m5_ip ip LEFT JOIN m2_transaction_journal t ON ip.ipsumber = t.tsumber AND ip.ipid =  t.tidtransaksi AND ip.ipnotransaksi = t.tnotransaksi SET ip.ipjumlahbayar = (CASE ip.ipid " & updNilaiIP & " ELSE ip.ipjumlahbayar END), ip.ipjumlahbayarvalas = (CASE ip.ipid " & updNilaiValasIP & " ELSE ip.ipjumlahbayarvalas END), ip.iptgllunas = (CASE ip.ipid " & updTglLunasIP & " ELSE ip.iptgllunas END) WHERE " & updFilterIP
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'JURNAL
                        sql = "UPDATE m5_ip ip LEFT JOIN m2_transaction_journal t ON ip.ipsumber = t.tsumber AND ip.ipid =  t.tidtransaksi AND ip.ipnotransaksi = t.tnotransaksi SET t.tstatuslunas = ip.ipstatusbayar, t.ttgllunas = ip.iptgllunas WHERE " & updFilterIP
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF UPDATE OUTSTANDING TRANSAKSI ============================================
                End If

                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "AS", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("asstatus") = 2 Then
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
    Public Function M5_AsUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("askontakkode", "c1.kkode")
            Filter = Filter.Replace("askontaknama", "c1.knama")
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
            Dim sumber As String = "As", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Astgl, Asnotransaksi, Asstatus FROM M5_As WHERE Asid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Asstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m5_as_history
            Dim rsSimpanHistory As String = SimpanHistory.M5_As_HistorySimpan("" & paramSplit(0) & "★M5_As_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m5_as_terkait("asid = '" & idtransaksi & "'")
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                ''CEK STATUS GIRO
                'dtdetail = AsDataTableAmbilDariDB("SELECT glnogiro FROM m2_giro_list WHERE glsumber = 'AS' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "' AND glstatus <> 0")
                'If dtdetail.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai

                'UPDATE OUTSTANDING TRANSAKSI ===================================================
                Dim Outstanding As Double = 0, OutstandingValas As Double = 0, tglLunas = "1900-01-01"
                Dim updNilaiIP As String = "", updNilaiValasIP As String = "", updFilterIP As String = "", matauangDetail As String = ""
                Dim idip As Integer = 0

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDB("SELECT matauang, jumlah, jumlahvalas, idip FROM m5_as_pay WHERE idas = '" & idtransaksi & "'")
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        '1.SET NILAI VARIABEL
                        idip = dr1("idip") : matauangDetail = dr1("matauang")

                        '2. SET NILAI UPDATE OUTSTANDING
                        Outstanding = AsDataTableDSum(dtdetail, "jumlah", "idip = '" & idip & "'")
                        OutstandingValas = AsDataTableDSum(dtdetail, "jumlahvalas", "idip = '" & idip & "'")

                        '3. SET NILAI UPDATE OUTSTANDING
                        updNilaiIP = String.Concat("WHEN '" & idip & "' THEN ROUND(ip.ipjumlahbayar - '" & Outstanding & "', 5) ", updNilaiIP)
                        updNilaiValasIP = String.Concat("WHEN '" & idip & "' THEN ROUND(ip.ipjumlahbayarvalas - '" & OutstandingValas & "', 5) ", updNilaiValasIP)

                        '4. SET FILTER UPDATE OUTSTANDING
                        updFilterIP = IIf(Len(updFilterIP.ToString) = 0, "", updFilterIP & " OR ")
                        updFilterIP = String.Concat(updFilterIP, "(ip.ipid = '" & idip & "')")
                    Next

                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If
                'END OF UPDATE OUTSTANDING TRANSAKSI ============================================

                'UPDATE JMLBAYAR IP
                If Len(updNilaiIP) > 0 And Len(updNilaiValasIP) > 0 Then 'IP
                    'TRANSAKSI
                    sql = "UPDATE m5_ip ip LEFT JOIN m2_transaction_journal t ON ip.ipsumber = t.tsumber AND ip.ipid = t.tidtransaksi AND ip.ipnotransaksi = t.tnotransaksi SET ip.ipjumlahbayar = (CASE ip.ipid " & updNilaiIP & " ELSE ip.ipjumlahbayar END), ip.ipjumlahbayarvalas = (CASE ip.ipid " & updNilaiValasIP & " ELSE ip.ipjumlahbayarvalas END), ip.iptgllunas = '" & FixQuotes(tglLunas) & "' WHERE " & updFilterIP
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'JURNAL
                    sql = "UPDATE m5_ip ip LEFT JOIN m2_transaction_journal t ON ip.ipsumber = t.tsumber AND ip.ipid = t.tidtransaksi AND ip.ipnotransaksi = t.tnotransaksi SET t.tstatuslunas = ip.ipstatusbayar, t.ttgllunas = ip.iptgllunas WHERE " & updFilterIP
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'AS' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'DELETE GIRO
                sql = "DELETE FROM m2_giro_list WHERE glsumber = 'AS' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M5_As SET Asstatus = " & nilaiStatus & ", Asmodifikasiuser='" & userid & "', Asmodifikasitgl = NOW(), Asposting = 0, Aspostingtgl = '1971-01-01 00:00:00', Asjmlrevisi = Asjmlrevisi + 1 WHERE Asid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_AsSearch(PostWsSearch(paramSplit(0), "M5_AsSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M5_AsDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("askontakkode", "c1.kkode")
            Filter = Filter.Replace("askontaknama", "c1.knama")
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
            Dim sumber As String = "As", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Asid, Asnotransaksi FROM M5_As WHERE Asid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT ascabang, aslokasi, assumber, asautonotransaksi, asnotransaksi, astgl"
            sql &= " FROM M5_as"
            sql &= " WHERE asid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("ascabang")
                lokasi = dtNomorNext.Rows(0)("aslokasi")
                sumber = dtNomorNext.Rows(0)("assumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("asautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("asnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("astgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M5_As_Pay WHERE idas='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M5_As WHERE asid ='" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_AsSearch(PostWsSearch(paramSplit(0), "M5_AsSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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