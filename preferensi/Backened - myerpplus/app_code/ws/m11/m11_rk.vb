Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m11_rk
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M11_RkSimpan(ByVal param As String) As String

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
        'rkid(0) As Integer, rkcabang(1) As String, rklokasi(2) As String, rksumber(3) As String, 
        'rkautonotransaksi(4) As Integer, rknotransaksi(5) As String, rktgl(6) As Date, rkkodepa(7) As Integer, rkkontak(8) As Integer, 
        'rkkontakperson(9) As String, rkalamat(10) As String, rkbagianterima(11) As Integer, rktermin(12) As String, rktgljatuhtempo(13) As Date, 
        'rknorek(14) As String, rkuraian(15) As String, rkcatatan(16) As String, 
        'rknoref(17) As String, rktglnoref(18) As Date, rkmatauang(19) As String, rkkurs(20) As Double, rkjumlah(21) As Double, 
        'rkjumlahvalas(22) As Double, rkjumlahbayar(23) As Double, rkjumlahbayarvalas(24) As Double, rkstatusbayar(25) As Integer, rktgllunas(26) As Date, 
        'rkcostcenter(27) As String, rkdivisi(28) As String, rksubdivisi(29) As String, rkproyek(30) As String, rkstatus(31) As Integer, 
        'rkstatussebelumnya(32) As Integer, rkjmlrevisi(33) As Integer, rkcetakanke(34) As Integer, rkinputuser(35) As Integer, rkinputtgl(36) As DateTime, 
        'rkmodifikasiuser(37) As Integer, rkmodifikasitgl(38) As DateTime, rkposting(39) As Integer, rkisclose(40) As Integer, rkcustomtext1(41) As String, 
        'rkcustomtext2(42) As String, rkcustomtext3(43) As String, rkcustomtext4(44) As String, rkcustomtext5(45) As String, rkcustomint1(46) As Integer, 
        'rkcustomint2(47) As Integer, rkcustomint3(48) As Integer, rkcustomdbl1(49) As Double, rkcustomdbl2(50) As Double, rkcustomdbl3(51) As Double, 
        'rkcustomdate1(52) As Date, rkcustomdate2(53) As Date, rkcustomdate3(54) As Date, rkidkj(55) As Integer
        'rkperawatan(56) As STring, rkkategoripasien(57) As String, rkkamar(58) As String, rkawalankatpasien(59) As String

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'rkid, rkcabang, rklokasi, rksumber, rkautonotransaksi, rknotransaksi, 
        'rktgl, rkkodepa, rkkontak, rkkontakperson, rkalamat, rkbagianterima, rktermin, rktgljatuhtempo, 
        'rknorek, rkuraian, rkcatatan, rknoref, rktglnoref, rkmatauang, 
        'rkkurs, rkjumlah, rkjumlahvalas, rkjumlahbayar, rkjumlahbayarvalas, rkstatusbayar, rktgllunas, 
        'rkcostcenter, rkdivisi, rksubdivisi, rkproyek, rkstatus, rkstatussebelumnya, rkjmlrevisi, 
        'rkcetakanke, rkinputuser, rkinputtgl, rkmodifikasiuser, rkmodifikasitgl, rkposting, rkisclose, 
        'rkcustomtext1, rkcustomtext2, rkcustomtext3, rkcustomtext4, rkcustomtext5, rkcustomint1, rkcustomint2, 
        'rkcustomint3, rkcustomdbl1, rkcustomdbl2, rkcustomdbl3, rkcustomdate1, rkcustomdate2, rkcustomdate3, rkidkj
        'rkperawatan, rkkategoripasien, rkkamar, rkawalankatpasien

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 62) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'asid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "rkid required numeric." : GoTo selesai
        End If
        'rkautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "rkautonotransaksi required numeric." : GoTo selesai
        End If
        'rktgl(7) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "rktgl required date." : GoTo selesai
        End If
        'rkkodepa(8) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "rkkodepa required numeric." : GoTo selesai
        End If
        'rkkontak(9) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "rkkontak required numeric." : GoTo selesai
        End If
        'rkbagianterima(17) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "rkbagianterima required numeric." : GoTo selesai
        End If
        'rktgljatuhtempo(19) As Date
        If (IsDate(dataUtama(13)) = False) Then
            result(2) = "rktgljatuhtempo required date." : GoTo selesai
        End If
        'rktglnoref(26) As Date
        If (IsDate(dataUtama(18)) = False) Then
            result(2) = "rktglnoref required date." : GoTo selesai
        End If
        'rkkurs(28) As Double
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "rkkurs required numeric." : GoTo selesai
        End If
        'rkjumlah(29) As Double
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "rkjumlah required numeric." : GoTo selesai
        End If
        'rkjumlahvalas(30) As Double
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "rkjumlahvalas required numeric." : GoTo selesai
        End If
        'rkjumlahbayar(31) As Double
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "rkjumlahbayar required numeric." : GoTo selesai
        End If
        'rkjumlahbayarvalas(32) As Double
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "rkjumlahbayarvalas required numeric." : GoTo selesai
        End If
        'rkstatusbayar(33) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "rkstatusbayar required numeric." : GoTo selesai
        End If
        'rktgllunas(34) As Date
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "rktgllunas required date." : GoTo selesai
        End If
        'rkstatus(39) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "rkstatus required numeric." : GoTo selesai
        End If
        'rkstatussebelumnya(40) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "rkstatussebelumnya required numeric." : GoTo selesai
        End If
        'rkjmlrevisi(41) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "rkjmlrevisi required numeric." : GoTo selesai
        End If
        'rkcetakanke(42) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "rkcetakanke required numeric." : GoTo selesai
        End If
        'rkinputuser(43) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "rkinputuser required numeric." : GoTo selesai
        End If
        'rkinputtgl(44) As DateTime
        If (IsDate(dataUtama(36)) = False) Then
            result(2) = "rkinputtgl required date." : GoTo selesai
        End If
        'rkmodifikasiuser(45) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "rkmodifikasiuser required numeric." : GoTo selesai
        End If
        'rkmodifikasitgl(46) As DateTime
        If (IsDate(dataUtama(38)) = False) Then
            result(2) = "rkmodifikasitgl required date." : GoTo selesai
        End If
        'rkposting(47) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "rkposting required numeric." : GoTo selesai
        End If
        'rkisclose(48) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "rkisclose required numeric." : GoTo selesai
        End If
        'rkcustomint1(54) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "rkcustomint1 required numeric." : GoTo selesai
        End If
        'rkcustomint2(55) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "rkcustomint2 required numeric." : GoTo selesai
        End If
        'rkcustomint3(56) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "rkcustomint3 required numeric." : GoTo selesai
        End If
        'rkcustomdbl1(57) As Double
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "rkcustomdbl1 required numeric." : GoTo selesai
        End If
        'rkcustomdbl2(58) As Double
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "rkcustomdbl2 required numeric." : GoTo selesai
        End If
        'rkcustomdbl3(59) As Double
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "rkcustomdbl3 required numeric." : GoTo selesai
        End If
        'rkcustomdate1(60) As Date
        If (IsDate(dataUtama(52)) = False) Then
            result(2) = "rkcustomdate1 required date." : GoTo selesai
        End If
        'rkcustomdate2(61) As Date
        If (IsDate(dataUtama(53)) = False) Then
            result(2) = "rkcustomdate2 required date." : GoTo selesai
        End If
        'rkcustomdate3(62) As Date
        If (IsDate(dataUtama(54)) = False) Then
            result(2) = "rkcustomdate3 required date." : GoTo selesai
        End If

        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "rkidkj required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'rkcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "rkcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "rkcabang should not be more than 25 character." : GoTo selesai
        End If

        'rklokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "rklokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "rklokasi should not be more than 25 character." : GoTo selesai
        End If

        'rksumber(4) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "rksumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "rksumber should not be more than 10 character." : GoTo selesai
        End If

        'rknotransaksi(6) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "rknotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "rknotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'rktgl(7) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "rktgl can't be empty" : GoTo selesai
        End If
        'SET TGLTRANSAKSI ---> UNTUK UPDATE TGL LUNAS TRANSAKSI
        'tglLunas = AsFormatTanggal(dataUtama(7))

        'rktgljatuhtempo(19) As Date
        If Len(dataUtama(13)) = 0 Then
            result(2) = "rktgljatuhtempo can't be empty" : GoTo selesai
        End If

        'rknorek(22) As String
        If Len(dataUtama(14)) = 0 Then
            result(2) = "rknorek can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(14)) > 25 Then
            result(2) = "rknorek should not be more than 25 character." : GoTo selesai
        End If

        'rktglnoref(26) As Date
        If Len(dataUtama(18)) = 0 Then
            result(2) = "rktglnoref can't be empty" : GoTo selesai
        End If

        'rkmatauang(27) As String
        If Len(dataUtama(19)) = 0 Then
            result(2) = "rkmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(19)) > 25 Then
            result(2) = "rkmatauang should not be more than 25 character." : GoTo selesai
        End If

        'rkkurs(28) As Double
        If Len(dataUtama(20)) = 0 Then
            result(2) = "rkkurs can't be empty" : GoTo selesai
        End If

        'rkjumlah(29) As Double
        If Len(dataUtama(21)) = 0 Then
            result(2) = "rkjumlah can't be empty" : GoTo selesai
        End If

        'rkjumlahvalas(30) As Double
        If Len(dataUtama(22)) = 0 Then
            result(2) = "rkjumlahvalas can't be empty" : GoTo selesai
        End If

        'rkjumlahbayar(31) As Double
        If Len(dataUtama(23)) = 0 Then
            result(2) = "rkjumlahbayar can't be empty" : GoTo selesai
        End If

        'rkjumlahbayarvalas(32) As Double
        If Len(dataUtama(24)) = 0 Then
            result(2) = "rkjumlahbayarvalas can't be empty" : GoTo selesai
        End If

        'rktgllunas(34) As Date
        If Len(dataUtama(26)) = 0 Then
            result(2) = "rktgllunas can't be empty" : GoTo selesai
        End If

        'rkinputtgl(44) As DateTime
        If Len(dataUtama(36)) = 0 Then
            result(2) = "rkinputtgl can't be empty" : GoTo selesai
        End If

        'rkmodifikasitgl(46) As DateTime
        If Len(dataUtama(38)) = 0 Then
            result(2) = "rkmodifikasitgl can't be empty" : GoTo selesai
        End If

        'rkcustomdbl1(57) As Double
        If Len(dataUtama(49)) = 0 Then
            result(2) = "rkcustomdbl1 can't be empty" : GoTo selesai
        End If

        'rkcustomdbl2(58) As Double
        If Len(dataUtama(50)) = 0 Then
            result(2) = "rkcustomdbl2 can't be empty" : GoTo selesai
        End If

        'rkcustomdbl3(59) As Double
        If Len(dataUtama(51)) = 0 Then
            result(2) = "rkcustomdbl3 can't be empty" : GoTo selesai
        End If

        'rkcustomdate1(60) As Date
        If Len(dataUtama(52)) = 0 Then
            result(2) = "rkcustomdate1 can't be empty" : GoTo selesai
        End If

        'rkcustomdate2(61) As Date
        If Len(dataUtama(53)) = 0 Then
            result(2) = "rkcustomdate2 can't be empty" : GoTo selesai
        End If

        'rkcustomdate3(62) As Date
        If Len(dataUtama(54)) = 0 Then
            result(2) = "rkcustomdate3 can't be empty" : GoTo selesai
        End If

        'rkpelanggan(56) As Date
        If Len(dataUtama(56)) > 10 Then
            result(2) = "rkpelanggan should not be more than 10 character." : GoTo selesai
        End If

        'rkkategoripasien(57) As Date
        If Len(dataUtama(57)) > 10 Then
            result(2) = "rkkategoripasien should not be more than 10 character." : GoTo selesai
        End If

        'rkkamar(58) As Date
        If Len(dataUtama(58)) > 100 Then
            result(2) = "rkkamar should not be more than 100 character." : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'result(2) = "berhasil hore" : GoTo selesai
        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "rkid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rklokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rksumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rknotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rktgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rkkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rkkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkalamat", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkbagianterima", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rktermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rktgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rknorek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rknoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rktglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkkurs", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "rkjumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "rkjumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "rkjumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkjumlahbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkstatusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rktgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkcostcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rksubdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkproyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rkstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rkjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rkcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rkinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rkinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rkmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rkisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rkcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rkcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rkcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rkcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkidkj", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rkperawatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkkategoripasien", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkkamar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkawalankatpasien", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rkkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rkjenistransaksi", AsEnumTypeData.AsInt64)
        If AsDataTableTambahData(dtutama, "rkid~rkcabang~rklokasi~rksumber~rkautonotransaksi~rknotransaksi~rktgl~rkkodepa~rkkontak~rkkontakperson~rkalamat~rkbagianterima~rktermin~rktgljatuhtempo~rknorek~rkuraian~rkcatatan~rknoref~rktglnoref~rkmatauang~rkkurs~rkjumlah~rkjumlahvalas~rkjumlahbayar~rkjumlahbayarvalas~rkstatusbayar~rktgllunas~rkcostcenter~rkdivisi~rksubdivisi~rkproyek~rkstatus~rkstatussebelumnya~rkjmlrevisi~rkcetakanke~rkinputuser~rkinputtgl~rkmodifikasiuser~rkmodifikasitgl~rkposting~rkisclose~rkcustomtext1~rkcustomtext2~rkcustomtext3~rkcustomtext4~rkcustomtext5~rkcustomint1~rkcustomint2~rkcustomint3~rkcustomdbl1~rkcustomdbl2~rkcustomdbl3~rkcustomdate1~rkcustomdate2~rkcustomdate3~rkidkj~rkperawatan~rkkategoripasien~rkkamar~rkawalankatpasien~rkkategori~rkjenistransaksi", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'result(2) = "berhasil horee" : GoTo selesai

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idrkcarabayar(0) As Integer, idrk(1) As Integer, carabayar(2) As Integer, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, nogiro(7) As String, tgljt(8) As Date, bank(9) As String, 
        'noacbank(10) As String, rekbank(11) As String, rekgiro(12) As String, catatan(13) As String, urutan(14) As Integer, 
        'isclose(15) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idrkcarabayar, idrk, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, isclose

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idrkcarabayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idrk", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "carabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsDouble)
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
            If (dataRowDetail.Length <> 16) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idascarabayar(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "idrkcarabayar required numeric." : GoTo selesai
            End If
            'idas(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "idrk required numeric." : GoTo selesai
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
            'isclose(16) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
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

            If AsDataTableTambahData(dtdetail, "idrkcarabayar~idrk~carabayar~matauang~kurs~jumlah~jumlahvalas~nogiro~tgljt~bank~noacbank~rekbank~rekgiro~catatan~urutan~isclose", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'Set variabel
            'idip(15) As Integer     , matauang(3) As String
            ''idip = dataRowDetail(15) : matauangDetail = dataRowDetail(3)

            ' ''BUAT FILTER UNTUK VALIDASI ---------------------------------
            ' ''ValidasiSimpan
            ' ''VALIDASI OUTSTANDING -------------------------
            ''If idip <> 0 Then 'IP
            ''    '1. CEK DATA EXIST
            ''    ftExistOutstandingIP = IIf(Len(ftExistOutstandingIP.ToString) = 0, "", ftExistOutstandingIP & " UNION ")
            ''    ftExistOutstandingIP = String.Concat(ftExistOutstandingIP, "SELECT EXISTS(SELECT 1 FROM m5_ip WHERE ipid = '" & idip & "' AND (ipstatus = 2 OR ipstatus = 3 OR ipstatus = 4 OR ipstatus = 7) LIMIT 1) as rowExists, ipid, ipsumber, ipnotransaksi FROM m5_ip WHERE ipid = '" & idip & "'")

            ''    '2. CEK JML OUTSTANDING
            ''    Outstanding = AsDataTableDSum(dtdetail, "jumlah", "idip=" & idip)
            ''    OutstandingValas = AsDataTableDSum(dtdetail, "jumlahvalas", "idip=" & idip)
            ''    ftOutstandingIP = IIf(Len(ftOutstandingIP.ToString) = 0, "", ftOutstandingIP & " OR ")
            ''    ftOutstandingIP = String.Concat(ftOutstandingIP, " (ip.ipid = '" & idip & "' AND (CASE ip.ipmatauang WHEN s.snilai THEN " & Outstanding & " > ip.ipjumlah - ip.ipjumlahbayar ELSE " & OutstandingValas & " > ip.ipjumlahvalas - ip.ipjumlahbayarvalas END)) ")

            ''    '3. SET NILAI UPDATE OUTSTANDING
            ''    updNilaiIP = String.Concat("WHEN '" & idip & "' THEN ipjumlahbayar + '" & Outstanding & "' ", updNilaiIP)
            ''    updNilaiValasIP = String.Concat("WHEN '" & idip & "' THEN ip.ipjumlahbayarvalas + '" & OutstandingValas & "' ", updNilaiValasIP)

            ''    '4. SET FILTER UPDATE OUTSTANDING
            ''    updFilterIP = IIf(Len(updFilterIP.ToString) = 0, "", updFilterIP & " OR ")
            ''    updFilterIP = String.Concat(updFilterIP, "(ip.ipid = '" & idip & "')")

            ''    '5. SET NILAI TGLLUNAS TRANSAKSI
            ''    If matauangDetail = MUFungsional Then
            ''        updTglLunasIP = String.Concat(" WHEN '" & idip & "' THEN (CASE WHEN ip.ipjumlahbayar >= ip.ipjumlah THEN '" & FixQuotes(tglLunas) & "' ELSE ip.iptgllunas END) ", updTglLunasIP)
            ''    Else
            ''        updTglLunasIP = String.Concat(" WHEN '" & idip & "' THEN (CASE WHEN ip.ipjumlahbayarvalas >= ip.ipjumlahvalas THEN '" & FixQuotes(tglLunas) & "' ELSE ip.iptgllunas END) ", updTglLunasIP)
            ''    End If
            ''End If
            ' ''END OF BUAT FILTER UNTUK VALIDASI --------------------------

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
                Dim vModuleId As Integer = 11, vMenuId As Integer = 20
                Select Case drutama("rkstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("rktgl")), AsFormatTanggal(drutama("rktgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "rkmatauang", "rknorek", dtdetail, "rekbank")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                'SET TGL JATUH TEMPO ====================================
                Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                rsTglJT = F_TglJT(drutama("rktermin").ToString, AsFormatTanggal(drutama("rktgl")), "rktgl").Split(sptSubParam)
                If rsTglJT(0) = 0 Then
                    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                Else
                    drutama("rktgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                End If
                'END OF SET TGL JATUH TEMPO =============================


                'HITUNG TOTAL BERDASARKAN DATA DETAIL ===================
                drutama("rkjumlah") = AsDataTableDSum(dtdetail, "jumlah")
                drutama("rkjumlahvalas") = AsDataTableDSum(dtdetail, "jumlahvalas")
                'END OF HITUNG TOTAL BERDASARKAN DATA DETAIL ============


                'CEK TOTAL UTAMA DAN DETAIL =============================
                Dim jumlah As Double = AsDataTableDSum(dtdetail, "jumlah")
                Dim jumlahvalas As Double = AsDataTableDSum(dtdetail, "jumlahvalas")
                If Double.Parse(drutama("rkjumlah")) <> jumlah Then
                    result(2) = "Total amount of main and detail are not balanced" : Trans.Rollback() : GoTo selesai
                ElseIf Double.Parse(drutama("rkjumlahvalas")) <> jumlahvalas Then
                    result(2) = "Total amount of foreign main and detail are not balanced" : Trans.Rollback() : GoTo selesai
                End If
                'END OF CEK TOTAL UTAMA DAN DETAIL ======================


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                ''If drutama("rkstatus") = 2 Then
                ''    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstandingIP, ftOutstandingIP)
                ''    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                ''End If
                'END OF VALIDASI SIMPAN =================================


                If isUpdate Then
                    result(4) = drutama("rkid")
                    notransaksi = drutama("rknotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(rkid), rknotransaksi FROM m_11_rk WHERE rkid='" & result(4) & "' AND rkstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(rkid) FROM m_11_rk WHERE rknotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        ''SIMPAN HISTORY ========================
                        'Dim SimpanHistory As New m5_as_history
                        'Dim rsSimpanHistory As String = SimpanHistory.M5_As_HistorySimpan("" & paramSplit(0) & "★M5_As_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("assumber")) & "▼" & FixQuotes(drutama("asid")) & "")
                        'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        'If (rsSplitResult(1) = 0) Then
                        '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        'End If
                        ''END OF SIMPAN HISTORY ==================

                        sql = "Update m_11_rk set rkcabang  = '" & FixQuotes(drutama("rkcabang")) & "', rklokasi  = '" & FixQuotes(drutama("rklokasi")) & "', rksumber  = '" & FixQuotes(drutama("rksumber")) & "', rkautonotransaksi  = " & drutama("rkautonotransaksi") & ", rknotransaksi  = '" & notransaksi & "', rktgl  = '" & FixQuotes(AsFormatTanggal(drutama("rktgl"))) & "', rkkodepa  = " & drutama("rkkodepa") & ", rkkontak  = " & drutama("rkkontak") & ", rkkontakperson  = '" & FixQuotes(drutama("rkkontakperson")) & "', rkalamat  = '" & FixQuotes(drutama("rkalamat")) & "', rkbagianterima  = " & drutama("rkbagianterima") & ", rktermin  = '" & FixQuotes(drutama("rktermin")) & "', rktgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("rktgljatuhtempo"))) & "', rknorek  = '" & FixQuotes(drutama("rknorek")) & "', rkuraian  = '" & FixQuotes(drutama("rkuraian")) & "', rkcatatan  = '" & FixQuotes(drutama("rkcatatan")) & "', rknoref  = '" & FixQuotes(drutama("rknoref")) & "', rktglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("rktglnoref"))) & "', rkmatauang  = '" & FixQuotes(drutama("rkmatauang")) & "', rkkurs  = '" & FixDouble(drutama("rkkurs")) & "', rkjumlah  = '" & FixDouble(drutama("rkjumlah")) & "', rkjumlahvalas  = '" & FixDouble(drutama("rkjumlahvalas")) & "', rkjumlahbayar  = '" & FixDouble(drutama("rkjumlahbayar")) & "', rkjumlahbayarvalas  = '" & FixDouble(drutama("rkjumlahbayarvalas")) & "', rkstatusbayar  = " & drutama("rkstatusbayar") & ", rktgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("rktgllunas"))) & "', rkcostcenter  = '" & FixQuotes(drutama("rkcostcenter")) & "', rkdivisi  = '" & FixQuotes(drutama("rkdivisi")) & "', rksubdivisi  = '" & FixQuotes(drutama("rksubdivisi")) & "', rkproyek  = '" & FixQuotes(drutama("rkproyek")) & "', rkstatus  = " & drutama("rkstatus") & ", rkstatussebelumnya  = " & drutama("rkstatussebelumnya") & ", rkjmlrevisi  = rkjmlrevisi+1, rkcetakanke  = " & drutama("rkcetakanke") & ", rkmodifikasiuser  = " & drutama("rkmodifikasiuser") & ", rkmodifikasitgl  = NOW(), rkposting  = 0, rkcustomtext1  = '" & FixQuotes(drutama("rkcustomtext1")) & "', rkcustomtext2  = '" & FixQuotes(drutama("rkcustomtext2")) & "', rkcustomtext3  = '" & FixQuotes(drutama("rkcustomtext3")) & "', rkcustomtext4  = '" & FixQuotes(drutama("rkcustomtext4")) & "', rkcustomtext5  = '" & FixQuotes(drutama("rkcustomtext5")) & "', rkcustomint1  = " & drutama("rkcustomint1") & ", rkcustomint2  = " & drutama("rkcustomint2") & ", rkcustomint3  = " & drutama("rkcustomint3") & ", rkcustomdbl1  = '" & FixDouble(drutama("rkcustomdbl1")) & "', rkcustomdbl2  = '" & FixDouble(drutama("rkcustomdbl2")) & "', rkcustomdbl3  = '" & FixDouble(drutama("rkcustomdbl3")) & "', rkcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("rkcustomdate1"))) & "', rkcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("rkcustomdate2"))) & "', rkcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("rkcustomdate3"))) & "', rkidkj = " & drutama("rkidkj") & ", rkperawatan = '" & FixQuotes(drutama("rkperawatan")) & "', rkkategoripasien = '" & FixQuotes(drutama("rkkategoripasien")) & "', rkkamar = '" & FixQuotes(drutama("rkkamar")) & "', rkkategori = " & drutama("rkkategori") & ", rkjenistransaksi = " & drutama("rkjenistransaksi") & " where rkid = '" & drutama("rkid") & "'"
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

                    If drutama("rkautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_NotransaksiKJ(drutama("rkperawatan"), drutama("rkawalankatpasien"), drutama("rksumber"), drutama("rktgl"))
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
                        notransaksi = drutama("rknotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(rkid) FROM m_11_rk WHERE rknotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into m_11_rk (rkcabang, rklokasi, rksumber, rkautonotransaksi, rknotransaksi, rktgl, rkkodepa, rkkontak, rkkontakperson, rkalamat, rkbagianterima, rktermin, rktgljatuhtempo, rknorek, rkuraian, rkcatatan, rknoref, rktglnoref, rkmatauang, rkkurs, rkjumlah, rkjumlahvalas, rkjumlahbayar, rkjumlahbayarvalas, rkstatusbayar, rktgllunas, rkcostcenter, rkdivisi, rksubdivisi, rkproyek, rkstatus, rkstatussebelumnya, rkjmlrevisi, rkcetakanke, rkinputuser, rkinputtgl, rkmodifikasiuser, rkmodifikasitgl, rkposting, rkisclose, rkcustomtext1, rkcustomtext2, rkcustomtext3, rkcustomtext4, rkcustomtext5, rkcustomint1, rkcustomint2, rkcustomint3, rkcustomdbl1, rkcustomdbl2, rkcustomdbl3, rkcustomdate1, rkcustomdate2, rkcustomdate3, rkidkj, rkperawatan, rkkategoripasien, rkkamar, rkkategori, rkjenistransaksi) values('" & FixQuotes(drutama("rkcabang")) & "', '" & FixQuotes(drutama("rklokasi")) & "', '" & FixQuotes(drutama("rksumber")) & "', " & drutama("rkautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("rktgl"))) & "', " & drutama("rkkodepa") & ", " & drutama("rkkontak") & ", '" & FixQuotes(drutama("rkkontakperson")) & "', '" & FixQuotes(drutama("rkalamat")) & "', " & drutama("rkbagianterima") & ", '" & FixQuotes(drutama("rktermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rktgljatuhtempo"))) & "', '" & FixQuotes(drutama("rknorek")) & "', '" & FixQuotes(drutama("rkuraian")) & "', '" & FixQuotes(drutama("rkcatatan")) & "', '" & FixQuotes(drutama("rknoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rktglnoref"))) & "', '" & FixQuotes(drutama("rkmatauang")) & "', '" & FixDouble(drutama("rkkurs")) & "', '" & FixDouble(drutama("rkjumlah")) & "', '" & FixDouble(drutama("rkjumlahvalas")) & "', '" & FixDouble(drutama("rkjumlahbayar")) & "', '" & FixDouble(drutama("rkjumlahbayarvalas")) & "', " & drutama("rkstatusbayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("rktgllunas"))) & "', '" & FixQuotes(drutama("rkcostcenter")) & "', '" & FixQuotes(drutama("rkdivisi")) & "', '" & FixQuotes(drutama("rksubdivisi")) & "', '" & FixQuotes(drutama("rkproyek")) & "', " & drutama("rkstatus") & ", " & drutama("rkstatussebelumnya") & ", " & drutama("rkjmlrevisi") & ", " & drutama("rkcetakanke") & ", " & drutama("rkinputuser") & ", NOW(), " & drutama("rkmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("rkisclose") & ", '" & FixQuotes(drutama("rkcustomtext1")) & "', '" & FixQuotes(drutama("rkcustomtext2")) & "', '" & FixQuotes(drutama("rkcustomtext3")) & "', '" & FixQuotes(drutama("rkcustomtext4")) & "', '" & FixQuotes(drutama("rkcustomtext5")) & "', " & drutama("rkcustomint1") & ", " & drutama("rkcustomint2") & ", " & drutama("rkcustomint3") & ", '" & FixDouble(drutama("rkcustomdbl1")) & "', '" & FixDouble(drutama("rkcustomdbl2")) & "', '" & FixDouble(drutama("rkcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rkcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rkcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rkcustomdate3"))) & "'," & drutama("rkidkj") & ", '" & FixQuotes(drutama("rkperawatan")) & "', '" & FixQuotes(drutama("rkkategoripasien")) & "', '" & FixQuotes(drutama("rkkamar")) & "', " & drutama("rkkategori") & ", " & drutama("rkjenistransaksi") & ")"
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
                    dt2 = AsDataTableAmbilDariDBCon("select rkid from m_11_rk where rknotransaksi='" & notransaksi & "' AND rkinputuser= '" & userid & "' order by rkmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from m_11_rk_pay where idrk = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idrkcarabayar") & ", " & result(4) & ", " & dr1("carabayar") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ")")
                        'QUERY UNTUK INSERT GIRO
                        If dr1("carabayar") = 2 Then
                            strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", ", "))
                            strGiro.Append("('" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(drutama("rksumber")) & "', " & result(4) & ", '" & FixQuotes(notransaksi) & "', " & drutama("rkkontak") & ", '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', " & 0 & ", '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', " & 0 & ", " & 0 & ", " & dr1("urutan") & ")")
                        End If
                    Next
                    sql = "Insert into m_11_rk_pay(idrkcarabayar, idrk, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'INSERT GIRO JIKA STATUS APPROVED DAN CARABAYAR = 2
                    If drutama("rkstatus") = 2 And Len(strGiro.ToString) > 0 Then
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

                If drutama("rkstatus") = 2 Then
                    If drutama("rkjenistransaksi") = 0 Then
                        Dim dtCekKunjungan As DataTable = AsDataTableAmbilDariDBCon("SELECT kjstatus, kjnotransaksi FROM m_11_kj WHERE kjid='" & drutama("rkidkj") & "'", myConn)
                        Dim cekKunjungan As Double = Val(dtCekKunjungan.Rows(0)(0))
                        If cekKunjungan > 0 Then
                            sql = "Update M_11_Kj set kjstatus = 3 where kjid = '" & drutama("rkidkj") & "'"
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

                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "RK", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("rkstatus") = 2 Then
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
                    hasilMsmq = SendMsmq(dirMsmq, "J", mjid, sumber, result(4), userid)
                    If Len(hasilMsmq) > 0 Then
                        result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
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
    Public Function M11_RkUpdateStatus(ByVal param As String) As String

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
            '    '#Taruh fungsi replace disini...
            '    Filter = Filter.Replace("askontakkode", "c1.kkode")
            '    Filter = Filter.Replace("askontaknama", "c1.knama")
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
            Dim sumber As String = "Rk", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0, idkj As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Rktgl, Rknotransaksi, Rkstatus, rkidkj FROM M_11_Rk WHERE Rkid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
                'idkj
                idkj = Integer.Parse(FxDB(dtdetail(1)(3), 0))
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Rkstatussebelumnya" : jnsaktivitas = 17
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

            ''SIMPAN HISTORY ========================
            'Dim SimpanHistory As New m5_as_history
            'Dim rsSimpanHistory As String = SimpanHistory.M5_As_HistorySimpan("" & paramSplit(0) & "★M5_As_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            'If (rsSplitResult(1) = 0) Then
            '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            'End If
            ''END OF SIMPAN HISTORY ==================

            If isDelete Then
                'CEK TERKAIT ====================================================================
                'PANGGIL QUERY TERKAIT
                Dim query As New m0_query
                'sql = query.m5_as_terkait("rkid = '" & idtransaksi & "'")

                sql = query.PanggilQuery("m11_rk_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)

                'BUKA KONEKSI
                myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
                myConn.Open()

                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                'UPDATE STATUS KJ ===============================================================
                'CEK TRANSAKSI TERKAIT KJ
                sql = "  SELECT * FROM ( "
                sql &= " SELECT a.akid as idterkait, a.aksumber as sumber FROM m_11_ak a JOIN m_11_kj kj ON a.akidkj = kj.kjid AND a.akstatus IN(2,3,4,7) AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " UNION ALL "
                sql &= " SELECT a.kmid as idterkait, a.kmsumber as sumber FROM m_11_km a JOIN m_11_kj kj ON a.kmidkj = kj.kjid AND a.kmstatus IN(2,3,4,7) AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " UNION ALL "
                sql &= " SELECT a.kwid as idterkait, a.kwsumber as sumber FROM m_11_kw a JOIN m_11_kw_detail b ON a.kwid = b.idkw AND a.kwstatus IN(2,3,4,7) JOIN m_11_kj kj ON b.sumber = 'KJ' AND b.idtransaksi = kj.kjid AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " UNION ALL "
                sql &= " SELECT a.lbid as idterkait, a.lbsumber as sumber FROM m_11_lb a JOIN m_11_kj kj ON a.lbidkj = kj.kjid AND a.lbstatus IN(2,3,4,7) AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " UNION ALL "
                sql &= " SELECT a.luid as idterkait, a.lusumber as sumber FROM m_11_lu a JOIN m_11_kj kj ON a.luidkj = kj.kjid AND a.lustatus IN(2,3,4,7) AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " UNION ALL "
                sql &= " SELECT a.rkid as idterkait, a.rksumber as sumber FROM m_11_rk a JOIN m_11_kj kj ON a.rkidkj = kj.kjid AND a.rkstatus IN(2,3,4,7) AND a.rkid <> '" & FixDouble(idtransaksi) & "' AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " UNION ALL "
                sql &= " SELECT a.rmid as idterkait, 'RM' as sumber FROM m_11_rm a JOIN m_11_kj kj ON a.rmidkj = kj.kjid AND a.rmstatus IN(2,3,4,7) AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " UNION ALL "
                sql &= " SELECT a.roid as idterkait, a.rosumber as sumber FROM m_11_ro a JOIN m_11_kj kj ON a.roidkj = kj.kjid AND a.rostatus IN(2,3,4,7) AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " ) as terkait "
                sql &= " ORDER BY terkait.sumber = 'KW' DESC, terkait.sumber ASC "
                dtdetail = AsDataTableAmbilDariDBCon(sql, myConn)
                If dtdetail.Rows.Count > 0 Then
                    'JIKA KJ MEMILIKI TRANSAKSI TERKAIT
                    If FxDB(dtdetail.Rows(0)("sumber"), "").ToUpper.Equals("KW") Then
                        'JIKA ADA KJ TERKAIT KW MAKA STATUS KJ = 4 (COMPLETE)
                        sql = "UPDATE M_11_Kj SET kjstatus = 4 WHERE kjid = '" & FixDouble(idkj) & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    Else
                        'JIKA ADA KJ TERKAIT SELAIN KW MAKA STATUS KJ = 3 (INPROGRESS)
                        sql = "UPDATE M_11_Kj SET kjstatus = 3 WHERE kjid = '" & FixDouble(idkj) & "'"
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
                    'JIKA KJ TIDAK MEMILIKI TRANSAKSI TERKAIT, STATUS KJ = 2 (APPROVED)
                    sql = "UPDATE M_11_Kj SET kjstatus = 2 WHERE kjid = '" & FixDouble(idkj) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                End If
                'END OF UPDATE STATUS KJ ========================================================

                ''CEK STATUS GIRO
                'dtdetail = AsDataTableAmbilDariDBCon("SELECT glnogiro FROM m2_giro_list WHERE glsumber = 'AS' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "' AND glstatus <> 0")
                'If dtdetail.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai

                ''UPDATE OUTSTANDING TRANSAKSI ===================================================
                'Dim Outstanding As Double = 0, OutstandingValas As Double = 0, tglLunas = "1900-01-01"
                'Dim updNilaiIP As String = "", updNilaiValasIP As String = "", updFilterIP As String = "", matauangDetail As String = ""
                'Dim idip As Integer = 0

                ''AMBIL DATA DETAIL
                'dtdetail = AsDataTableAmbilDariDBCon("SELECT matauang, jumlah, jumlahvalas, idip FROM m5_as_pay WHERE idas = '" & idtransaksi & "'")
                'If dtdetail.Rows.Count > 0 Then
                '    For Each dr1 As DataRow In dtdetail.Rows
                '        '1.SET NILAI VARIABEL
                '        idip = dr1("idip") : matauangDetail = dr1("matauang")

                '        '2. SET NILAI UPDATE OUTSTANDING
                '        Outstanding = AsDataTableDSum(dtdetail, "jumlah", "idip = '" & idip & "'")
                '        OutstandingValas = AsDataTableDSum(dtdetail, "jumlahvalas", "idip = '" & idip & "'")

                '        '3. SET NILAI UPDATE OUTSTANDING
                '        updNilaiIP = String.Concat("WHEN '" & idip & "' THEN ip.ipjumlahbayar - '" & Outstanding & "' ", updNilaiIP)
                '        updNilaiValasIP = String.Concat("WHEN '" & idip & "' THEN ip.ipjumlahbayarvalas - '" & OutstandingValas & "' ", updNilaiValasIP)

                '        '4. SET FILTER UPDATE OUTSTANDING
                '        updFilterIP = IIf(Len(updFilterIP.ToString) = 0, "", updFilterIP & " OR ")
                '        updFilterIP = String.Concat(updFilterIP, "(ip.ipid = '" & idip & "')")
                '    Next

                'Else
                '    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                'End If
                ''END OF UPDATE OUTSTANDING TRANSAKSI ============================================

                ''UPDATE JMLBAYAR IP
                'If Len(updNilaiIP) > 0 And Len(updNilaiValasIP) > 0 Then 'IP
                '    'UPDATE UTAMA
                '    sql = "UPDATE m5_ip ip LEFT JOIN m2_transaction_journal t ON ip.ipsumber = t.tsumber AND ip.ipid = t.tidtransaksi AND ip.ipnotransaksi = t.tnotransaksi SET ip.ipjumlahbayar = (CASE ip.ipid " & updNilaiIP & " ELSE ip.ipjumlahbayar END), ip.ipjumlahbayarvalas = (CASE ip.ipid " & updNilaiValasIP & " ELSE ip.ipjumlahbayarvalas END), ip.iptgllunas = '" & FixQuotes(tglLunas) & "', t.tstatuslunas = ip.ipstatusbayar, t.ttgllunas = ip.iptgllunas WHERE " & updFilterIP
                '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '    With objCmd
                '        .Connection = myconn
                '        .Transaction = Trans
                '        .CommandType = CommandType.Text
                '        .CommandText = sql
                '    End With
                '    objCmd.ExecuteNonQuery()
                'End If

                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'RK' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'DELETE GIRO
                sql = "DELETE FROM m2_giro_list WHERE glsumber = 'RK' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M_11_rk SET rkstatus = " & nilaiStatus & ", rkmodifikasiuser='" & userid & "', rkmodifikasitgl = NOW(), rkposting = 0, rkpostingtgl = '1971-01-01 00:00:00', rkjmlrevisi = rkjmlrevisi + 1 WHERE rkid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M11_RkSearch(PostWsSearch(paramSplit(0), "M11_RkSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M11_RkDelete(ByVal param As String) As String

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
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        '    Filter = Filter.Replace("askontakkode", "c1.kkode")
        '    Filter = Filter.Replace("askontaknama", "c1.knama")
        'End If
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
            Dim sumber As String = "Rk", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT rkid, rknotransaksi FROM M_11_rk WHERE rkid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT rkcabang, rklokasi, rksumber, rkautonotransaksi, rknotransaksi, rktgl"
            sql &= " FROM M_11_rk"
            sql &= " WHERE rkid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("rkcabang")
                lokasi = dtNomorNext.Rows(0)("rklokasi")
                sumber = dtNomorNext.Rows(0)("rksumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("rkautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("rknotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("rktgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M_11_rk_Pay WHERE idrk='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M_11_rk WHERE rkid ='" & idtransaksi & "'"
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
            Dim paramSearch As String = M11_RkSearch(PostWsSearch(paramSplit(0), "M11_RkSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M11_RkGetdataById(ByVal param As String) As String
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
        'assubdivisinama, asproyeknama, asstatusnama, asstatussebelumnyanama, asinputusernama, asmodifikasiusernama,
        'rkperawatan, rkkategoripasien, rkkamar, rkkategoripasiennama, rkkamarnama

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

        Dim NmMemcached As String = "aplikasi1-M_11_rk~M_11_rk_Detail-" & idtransaksi

        'replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "rkid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "rkid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m11_rk_getdata")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("rkid"), 0), sptField,
                     FxDB(drutama("rkcabang"), ""), sptField,
                     FxDB(drutama("rklokasi"), ""), sptField,
                     FxDB(drutama("rksumber"), ""), sptField,
                     FxDB(drutama("rkautonotransaksi"), 0), sptField,
                     FxDB(drutama("rknotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rktgl"), ""), formatTgl), sptField,
                     FxDB(drutama("rkkodepa"), 0), sptField,
                     FxDB(drutama("rkkontak"), 0), sptField,
                     FxDB(drutama("rkkontakperson"), ""), sptField,
                     FxDB(drutama("rkalamat"), ""), sptField,
                     FxDB(drutama("rkbagianterima"), 0), sptField,
                     FxDB(drutama("rktermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rktgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("rknorek"), ""), sptField,
                     FxDB(drutama("rkuraian"), ""), sptField,
                     FxDB(drutama("rkcatatan"), ""), sptField,
                     FxDB(drutama("rknoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rktglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("rkmatauang"), ""), sptField,
                     FxDB(drutama("rkkurs"), 0), sptField,
                     FxDB(drutama("rkjumlah"), 0), sptField,
                     FxDB(drutama("rkjumlahvalas"), 0), sptField,
                     FxDB(drutama("rkjumlahbayar"), 0), sptField,
                     FxDB(drutama("rkjumlahbayarvalas"), 0), sptField,
                     FxDB(drutama("rkstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rktgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("rkcostcenter"), ""), sptField,
                     FxDB(drutama("rkdivisi"), ""), sptField,
                     FxDB(drutama("rksubdivisi"), ""), sptField,
                     FxDB(drutama("rkproyek"), ""), sptField,
                     FxDB(drutama("rkstatus"), 0), sptField,
                     FxDB(drutama("rkstatussebelumnya"), 0), sptField,
                     FxDB(drutama("rkjmlrevisi"), 0), sptField,
                     FxDB(drutama("rkcetakanke"), 0), sptField,
                     FxDB(drutama("rkinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rkinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rkmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rkmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rkposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rkpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rkisclose"), 0), sptField,
                     FxDB(drutama("rkcustomtext1"), ""), sptField,
                     FxDB(drutama("rkcustomtext2"), ""), sptField,
                     FxDB(drutama("rkcustomtext3"), ""), sptField,
                     FxDB(drutama("rkcustomtext4"), ""), sptField,
                     FxDB(drutama("rkcustomtext5"), ""), sptField,
                     FxDB(drutama("rkcustomint1"), 0), sptField,
                     FxDB(drutama("rkcustomint2"), 0), sptField,
                     FxDB(drutama("rkcustomint3"), 0), sptField,
                     FxDB(drutama("rkcustomdbl1"), 0), sptField,
                     FxDB(drutama("rkcustomdbl2"), 0), sptField,
                     FxDB(drutama("rkcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rkcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rkcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rkcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("rkidkj"), 0), sptField,
                     FxDB(drutama("rkcabangnama"), ""), sptField,
                     FxDB(drutama("rklokasinama"), ""), sptField,
                     FxDB(drutama("rkkontakkode"), ""), sptField,
                     FxDB(drutama("rkkontaknama"), ""), sptField,
                     FxDB(drutama("rkbagianterimakode"), ""), sptField,
                     FxDB(drutama("rkbagianterimanama"), ""), sptField,
                     FxDB(drutama("rkterminnama"), ""), sptField,
                     FxDB(drutama("rkterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("rknoreknama"), ""), sptField,
                     FxDB(drutama("rkcostcenternama"), ""), sptField,
                     FxDB(drutama("rkdivisinama"), ""), sptField,
                     FxDB(drutama("rksubdivisinama"), ""), sptField,
                     FxDB(drutama("rkproyeknama"), ""), sptField,
                     FxDB(drutama("rkstatusnama"), ""), sptField,
                     FxDB(drutama("rkstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("rkinputusernama"), ""), sptField,
                     FxDB(drutama("rkmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("rknotransaksikj"), ""), sptField,
                     FxDB(drutama("rknamakj"), ""), sptField,
                     FxDB(drutama("rkperawatan"), ""), sptField,
                     FxDB(drutama("rkkategoripasien"), ""), sptField,
                     FxDB(drutama("rkkamar"), ""), sptField,
                     FxDB(drutama("rkkategoripasiennama"), ""), sptField,
                     FxDB(drutama("rkkamarnama"), ""), sptField,
                     FxDB(drutama("rkkategori"), 0), sptField,
      FxDB(drutama("rkjenistransaksi"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idrkcarabayar"), 0), sptField,
                     FxDB(dr("idrk"), 0), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rkid, rkcabang, rklokasi, rksumber, rkautonotransaksi, rknotransaksi, rktgl, rkkodepa, rkkontak, rkkontakperson, rkalamat, rkbagianterima, rktermin, rktgljatuhtempo, rknorek, rkuraian, rkcatatan, rknoref, rktglnoref, rkmatauang, rkkurs, rkjumlah, rkjumlahvalas, rkjumlahbayar, rkjumlahbayarvalas, rkstatusbayar, rktgllunas, rkcostcenter, rkdivisi, rksubdivisi, rkproyek, rkstatus, rkstatussebelumnya, rkjmlrevisi, rkcetakanke, rkinputuser, rkinputtgl, rkmodifikasiuser, rkmodifikasitgl, rkposting, rkpostingtgl, rkisclose, rkcustomtext1, rkcustomtext2, rkcustomtext3, rkcustomtext4, rkcustomtext5, rkcustomint1, rkcustomint2, rkcustomint3, rkcustomdbl1, rkcustomdbl2, rkcustomdbl3, rkcustomdate1, rkcustomdate2, rkcustomdate3, rkidkj, rkcabangnama, rklokasinama, rkkontakkode, rkkontaknama, rkbagianterimakode, rkbagianterimanama, rkterminnama, rkterminharijatuhtempo, rknoreknama, rkcostcenternama, rkdivisinama, rksubdivisinama, rkproyeknama, rkstatusnama, rkstatussebelumnyanama, rkinputusernama, rkmodifikasiusernama, rknotransaksikj, rknamakj, rkperawatan, rkkategoripasien, rkkamar, rkkategoripasiennama, rkkamarnama, rkkategori, rkjenistransaksi"), sptSubParam, ReplaceMapping("idrkcarabayar, idrk, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose, carabayarnama, banknama, rekbanknama, rekgironama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_RkSearch(ByVal param As String) As String
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
        'asmodifikasiusernama, rkperawatan, rkkategoripasien, rkkamar

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
            Filter = Filter.Replace("rknorm", "kj.kjnopasien")
            Filter = Filter.Replace("rknama", "kj.kjnama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m11_rk_v")
        'dt = AsDataTableAmbilDariDB("select `m11rk`.`rkid` AS `rkid`,`m11rk`.`rkcabang` AS `rkcabang`,`m11rk`.`rklokasi` AS `rklokasi`,`m11rk`.`rksumber` AS `rksumber`,`m11rk`.`rkautonotransaksi` AS `rkautonotransaksi`,`m11rk`.`rknotransaksi` AS `rknotransaksi`,`m11rk`.`rktgl` AS `rktgl`,`m11rk`.`rkkodepa` AS `rkkodepa`,`m11rk`.`rkkontak` AS `rkkontak`,`m11rk`.`rkkontakperson` AS `rkkontakperson`,`m11rk`.`rkalamat` AS `rkalamat`,`m11rk`.`rkbagianterima` AS `rkbagianterima`,`m11rk`.`rktermin` AS `rktermin`,`m11rk`.`rktgljatuhtempo` AS `rktgljatuhtempo`,`m11rk`.`rknorek` AS `rknorek`,`m11rk`.`rkuraian` AS `rkuraian`,`m11rk`.`rkcatatan` AS `rkcatatan`,`m11rk`.`rknoref` AS `rknoref`,`m11rk`.`rktglnoref` AS `rktglnoref`,`m11rk`.`rkmatauang` AS `rkmatauang`,`m11rk`.`rkkurs` AS `rkkurs`,`m11rk`.`rkjumlah` AS `rkjumlah`,`m11rk`.`rkjumlahvalas` AS `rkjumlahvalas`,`m11rk`.`rkjumlahbayar` AS `rkjumlahbayar`,`m11rk`.`rkjumlahbayarvalas` AS `rkjumlahbayarvalas`,`m11rk`.`rkstatusbayar` AS `rkstatusbayar`,`m11rk`.`rktgllunas` AS `rktgllunas`,`m11rk`.`rkcostcenter` AS `rkcostcenter`,`m11rk`.`rkdivisi` AS `rkdivisi`,`m11rk`.`rksubdivisi` AS `rksubdivisi`,`m11rk`.`rkproyek` AS `rkproyek`,`m11rk`.`rkstatus` AS `rkstatus`,`m11rk`.`rkstatussebelumnya` AS `rkstatussebelumnya`,`m11rk`.`rkjmlrevisi` AS `rkjmlrevisi`,`m11rk`.`rkcetakanke` AS `rkcetakanke`,`m11rk`.`rkinputuser` AS `rkinputuser`,`m11rk`.`rkinputtgl` AS `rkinputtgl`,`m11rk`.`rkmodifikasiuser` AS `rkmodifikasiuser`,`m11rk`.`rkmodifikasitgl` AS `rkmodifikasitgl`,`m11rk`.`rkposting` AS `rkposting`,`m11rk`.`rkpostingtgl` AS `rkpostingtgl`,`m11rk`.`rkisclose` AS `rkisclose`,`br`.`bnama` AS `rkcabangnama`,`lc`.`lnama` AS `rklokasinama`,`c1`.`ckode` AS `rkkontakkode`,`c1`.`cnama` AS `rkkontaknama`,`c2`.`kkode` AS `rkbagianterimakode`,`c2`.`knama` AS `rkbagianterimanama`,`coa`.`cnama` AS `rknoreknama`,`st1`.`nama` AS `rkstatusnama`,`st2`.`nama` AS `rkstatussebelumnyanama`,`u1`.`unama` AS `rkinputusernama`,`u2`.`unama` AS `rkmodifikasiusernama` from (((((((((`m_11_rk` `m11rk` left join `m1_branch` `br` on((`m11rk`.`rkcabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`m11rk`.`rklokasi` = `lc`.`lkode`))) left join `m1_colleague` `c1` on((`m11rk`.`rkkontak` = `c1`.`cid`))) left join `m1_contact` `c2` on((`m11rk`.`rkbagianterima` = `c2`.`kid`))) left join `m1_coa` `coa` on((`m11rk`.`rknorek` = `coa`.`cnomor`))) left join `m0_status` `st1` on((`m11rk`.`rkstatus` = `st1`.`kode`))) left join `m0_status` `st2` on((`m11rk`.`rkstatussebelumnya` = `st2`.`kode`))) left join `m0_user` `u1` on((`m11rk`.`rkinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`m11rk`.`rkmodifikasiuser` = `u2`.`userid`)))")
        'result(2) = "jumlah = " + dt.Rows.Count.ToString : GoTo selesai
        dt = AmbilData("aplikasi1-m11_rk_v", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rkid"), 0), sptField,
                     FxDB(dr("rkcabang"), ""), sptField,
                     FxDB(dr("rklokasi"), ""), sptField,
                     FxDB(dr("rksumber"), ""), sptField,
                     FxDB(dr("rkautonotransaksi"), 0), sptField,
                     FxDB(dr("rknotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rktgl"), ""), formatTgl), sptField,
                     FxDB(dr("rkkodepa"), 0), sptField,
                     FxDB(dr("rkkontak"), 0), sptField,
                     FxDB(dr("rkkontakperson"), ""), sptField,
                     FxDB(dr("rkalamat"), ""), sptField,
                     FxDB(dr("rkbagianterima"), 0), sptField,
                     FxDB(dr("rktermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rktgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("rknorek"), ""), sptField,
                     FxDB(dr("rkuraian"), ""), sptField,
                     FxDB(dr("rkcatatan"), ""), sptField,
                     FxDB(dr("rknoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rktglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("rkmatauang"), ""), sptField,
                     FxDB(dr("rkkurs"), 0), sptField,
                     FxDB(dr("rkjumlah"), 0), sptField,
                     FxDB(dr("rkjumlahvalas"), 0), sptField,
                     FxDB(dr("rkjumlahbayar"), 0), sptField,
                     FxDB(dr("rkjumlahbayarvalas"), 0), sptField,
                     FxDB(dr("rkstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rktgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("rkcostcenter"), ""), sptField,
                     FxDB(dr("rkdivisi"), ""), sptField,
                     FxDB(dr("rksubdivisi"), ""), sptField,
                     FxDB(dr("rkproyek"), ""), sptField,
                     FxDB(dr("rkstatus"), 0), sptField,
                     FxDB(dr("rkstatussebelumnya"), 0), sptField,
                     FxDB(dr("rkjmlrevisi"), 0), sptField,
                     FxDB(dr("rkcetakanke"), 0), sptField,
                     FxDB(dr("rkinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rkinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rkmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rkmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rkposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rkpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rkisclose"), 0), sptField,
                     FxDB(dr("rkidkj"), 0), sptField,
                     FxDB(dr("rkcabangnama"), ""), sptField,
                     FxDB(dr("rklokasinama"), ""), sptField,
                     FxDB(dr("rkkontakkode"), ""), sptField,
                     FxDB(dr("rkkontaknama"), ""), sptField,
                     FxDB(dr("rkbagianterimakode"), ""), sptField,
                     FxDB(dr("rkbagianterimanama"), ""), sptField,
                     FxDB(dr("rknoreknama"), ""), sptField,
                     FxDB(dr("rkstatusnama"), ""), sptField,
                     FxDB(dr("rkstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("rkinputusernama"), ""), sptField,
                     FxDB(dr("rkmodifikasiusernama"), ""), sptField,
                     FxDB(dr("rknotransaksikj"), ""), sptField,
                     FxDB(dr("rkperawatan"), ""), sptField,
                     FxDB(dr("rkkategoripasien"), ""), sptField,
                     FxDB(dr("rkkamar"), ""), sptField,
                     FxDB(dr("rknorm"), ""), sptField,
                     FxDB(dr("rknama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rkid, rkcabang, rklokasi, rksumber, rkautonotransaksi, rknotransaksi, rktgl, rkkodepa, rkkontak, rkkontakperson, rkalamat, rkbagianterima, rktermin, rktgljatuhtempo, rknorek, rkuraian, rkcatatan, rknoref, rktglnoref, rkmatauang, rkkurs, rkjumlah, rkjumlahvalas, rkjumlahbayar, rkjumlahbayarvalas, rkstatusbayar, rktgllunas, rkcostcenter, rkdivisi, rksubdivisi, rkproyek, rkstatus, rkstatussebelumnya, rkjmlrevisi, rkcetakanke, rkinputuser, rkinputtgl, rkmodifikasiuser, rkmodifikasitgl, rkposting, rkpostingtgl, rkisclose, rkidkj, rkcabangnama, rklokasinama, rkkontakkode, rkkontaknama, rkbagianterimakode, rkbagianterimanama, rknoreknama, rkstatusnama, rkstatussebelumnyanama, rkinputusernama, rkmodifikasiusernama, rknotransaksikj, rkkamar, rkperawatan, rkkategoripasien, rknorm, rknama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_RkTerkait(ByVal param As String) As String
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
            result(2) = "rkid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m11_rk_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-m11_rk_terkait", , sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rkid"), 0), sptField,
                     FxDB(dr("rknotransaksi"), ""), sptField,
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
            result(2) = "Related RK data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rkid, rknotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

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

End Class