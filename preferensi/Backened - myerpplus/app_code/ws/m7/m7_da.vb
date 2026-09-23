Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m7_da
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M7_DaSimpan(ByVal param As String) As String
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
        'daid(0) As Integer, dacabang(1) As String, dalokasi(2) As String, dagudang(3) As String, dasumber(4) As String, 
        'daautonotransaksi(5) As Integer, danotransaksi(6) As String, datgl(7) As Date, dakodepa(8) As Integer, damatauang(9) As String, 
        'dakurs(10) As Double, dabagianda(11) As Integer, dabagiandakontak(12) As String, dauraian(13) As String, dacatatan(14) As String, 
        'danoref(15) As String, datglnoref(16) As Date, dastatus(17) As Integer, dastatussebelumnya(18) As Integer, dajmlrevisi(19) As Integer, 
        'dacetakanke(20) As Integer, dainputuser(21) As Integer, dainputtgl(22) As DateTime, damodifikasiuser(23) As Integer, damodifikasitgl(24) As DateTime, 
        'daposting(25) As Integer, dapostingtgl(26) As DateTime, datutupperiode(27) As Integer, daisclose(28) As Integer, dacustomtext1(29) As String, 
        'dacustomtext2(30) As String, dacustomtext3(31) As String, dacustomtext4(32) As String, dacustomtext5(33) As String, dacustomint1(34) As Integer, 
        'dacustomint2(35) As Integer, dacustomint3(36) As Integer, dacustomdbl1(37) As Double, dacustomdbl2(38) As Double, dacustomdbl3(39) As Double, 
        'dacustomdate1(40) As Date, dacustomdate2(41) As Date, dacustomdate3(42) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'daid, dacabang, dalokasi, dagudang, dasumber, daautonotransaksi, danotransaksi, 
        'datgl, dakodepa, damatauang, dakurs, dabagianda, dabagiandakontak, dauraian, 
        'dacatatan, danoref, datglnoref, dastatus, dastatussebelumnya, dajmlrevisi, dacetakanke, 
        'dainputuser, dainputtgl, damodifikasiuser, damodifikasitgl, daposting, dapostingtgl, datutupperiode, 
        'daisclose, dacustomtext1, dacustomtext2, dacustomtext3, dacustomtext4, dacustomtext5, dacustomint1, 
        'dacustomint2, dacustomint3, dacustomdbl1, dacustomdbl2, dacustomdbl3, dacustomdate1, dacustomdate2, 
        'dacustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 43) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'daid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "daid required numeric." : GoTo selesai
        End If
        'daautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "daautonotransaksi required numeric." : GoTo selesai
        End If
        'datgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "datgl required date." : GoTo selesai
        End If
        'dakodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "dakodepa required numeric." : GoTo selesai
        End If
        'dakurs(10) As Double
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "dakurs required numeric." : GoTo selesai
        End If
        'dabagianda(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "dabagianda required numeric." : GoTo selesai
        End If
        'datglnoref(16) As Date
        If (IsDate(dataUtama(16)) = False) Then
            result(2) = "datglnoref required date." : GoTo selesai
        End If
        'dastatus(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "dastatus required numeric." : GoTo selesai
        End If
        'dastatussebelumnya(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "dastatussebelumnya required numeric." : GoTo selesai
        End If
        'dajmlrevisi(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "dajmlrevisi required numeric." : GoTo selesai
        End If
        'dacetakanke(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "dacetakanke required numeric." : GoTo selesai
        End If
        'dainputuser(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "dainputuser required numeric." : GoTo selesai
        End If
        'dainputtgl(22) As DateTime
        If (IsDate(dataUtama(22)) = False) Then
            result(2) = "dainputtgl required date." : GoTo selesai
        End If
        'damodifikasiuser(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "damodifikasiuser required numeric." : GoTo selesai
        End If
        'damodifikasitgl(24) As DateTime
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "damodifikasitgl required date." : GoTo selesai
        End If
        'daposting(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "daposting required numeric." : GoTo selesai
        End If
        'dapostingtgl(26) As DateTime
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "dapostingtgl required date." : GoTo selesai
        End If
        'datutupperiode(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "datutupperiode required numeric." : GoTo selesai
        End If
        'daisclose(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "daisclose required numeric." : GoTo selesai
        End If
        'dacustomint1(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "dacustomint1 required numeric." : GoTo selesai
        End If
        'dacustomint2(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "dacustomint2 required numeric." : GoTo selesai
        End If
        'dacustomint3(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "dacustomint3 required numeric." : GoTo selesai
        End If
        'dacustomdbl1(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "dacustomdbl1 required numeric." : GoTo selesai
        End If
        'dacustomdbl2(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "dacustomdbl2 required numeric." : GoTo selesai
        End If
        'dacustomdbl3(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "dacustomdbl3 required numeric." : GoTo selesai
        End If
        'dacustomdate1(40) As Date
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "dacustomdate1 required date." : GoTo selesai
        End If
        'dacustomdate2(41) As Date
        If (IsDate(dataUtama(41)) = False) Then
            result(2) = "dacustomdate2 required date." : GoTo selesai
        End If
        'dacustomdate3(42) As Date
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "dacustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'dacabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "dacabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "dacabang should not be more than 25 character." : GoTo selesai
        End If

        'dalokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "dalokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "dalokasi should not be more than 25 character." : GoTo selesai
        End If

        'dasumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "dasumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "dasumber should not be more than 10 character." : GoTo selesai
        End If

        'danotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "danotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "danotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'datgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "datgl can't be empty" : GoTo selesai
        End If

        'damatauang(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "damatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 25 Then
            result(2) = "damatauang should not be more than 25 character." : GoTo selesai
        End If

        'dakurs(10) As Double
        If Len(dataUtama(10)) = 0 Then
            result(2) = "dakurs can't be empty" : GoTo selesai
        End If

        'datglnoref(16) As Date
        If Len(dataUtama(16)) = 0 Then
            result(2) = "datglnoref can't be empty" : GoTo selesai
        End If

        'dainputtgl(22) As DateTime
        If Len(dataUtama(22)) = 0 Then
            result(2) = "dainputtgl can't be empty" : GoTo selesai
        End If

        'damodifikasitgl(24) As DateTime
        If Len(dataUtama(24)) = 0 Then
            result(2) = "damodifikasitgl can't be empty" : GoTo selesai
        End If

        'dapostingtgl(26) As DateTime
        If Len(dataUtama(26)) = 0 Then
            result(2) = "dapostingtgl can't be empty" : GoTo selesai
        End If

        'dacustomdbl1(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "dacustomdbl1 can't be empty" : GoTo selesai
        End If

        'dacustomdbl2(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "dacustomdbl2 can't be empty" : GoTo selesai
        End If

        'dacustomdbl3(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "dacustomdbl3 can't be empty" : GoTo selesai
        End If

        'dacustomdate1(40) As Date
        If Len(dataUtama(40)) = 0 Then
            result(2) = "dacustomdate1 can't be empty" : GoTo selesai
        End If

        'dacustomdate2(41) As Date
        If Len(dataUtama(41)) = 0 Then
            result(2) = "dacustomdate2 can't be empty" : GoTo selesai
        End If

        'dacustomdate3(42) As Date
        If Len(dataUtama(42)) = 0 Then
            result(2) = "dacustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "daid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dacabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dalokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dagudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dasumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "daautonotransaksi", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "danotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "datgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dakodepa", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "damatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dakurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dabagianda", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "dabagiandakontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dauraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dacatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "danoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "datglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dastatus", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "dastatussebelumnya", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "dajmlrevisi", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "dacetakanke", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "dainputuser", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "dainputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "damodifikasiuser", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "damodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "daposting", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "dapostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "datutupperiode", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "daisclose", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "dacustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dacustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dacustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dacustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dacustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dacustomint1", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "dacustomint2", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "dacustomint3", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "dacustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dacustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dacustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dacustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dacustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dacustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "daid~dacabang~dalokasi~dagudang~dasumber~daautonotransaksi~danotransaksi~datgl~dakodepa~damatauang~dakurs~dabagianda~dabagiandakontak~dauraian~dacatatan~danoref~datglnoref~dastatus~dastatussebelumnya~dajmlrevisi~dacetakanke~dainputuser~dainputtgl~damodifikasiuser~damodifikasitgl~daposting~dapostingtgl~datutupperiode~daisclose~dacustomtext1~dacustomtext2~dacustomtext3~dacustomtext4~dacustomtext5~dacustomint1~dacustomint2~dacustomint3~dacustomdbl1~dacustomdbl2~dacustomdbl3~dacustomdate1~dacustomdate2~dacustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'iddadetail(0) As Integer, idda(1) As Integer, idaset(2) As Integer, penyusutanke(3) As Integer, matauang(4) As String, 
        'kurs(5) As Double, nilaipenyusutan(6) As Double, nilaibukusebelumnya(7) As Double, costcenter(8) As String, divisi(9) As String, 
        'subdivisi(10) As String, proyek(11) As String, catatan(12) As String, urutan(13) As Integer, isclose(14) As Integer, 
        'customtext1(15) As String, customtext2(16) As String, customtext3(17) As String, customdbl1(18) As Double, customdbl2(19) As Double, 
        'customdbl3(20) As Double, customdate1(21) As Date, customdate2(22) As Date, customdate3(23) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'iddadetail, idda, idaset, penyusutanke, matauang, kurs, nilaipenyusutan, 
        'nilaibukusebelumnya, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "iddadetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idda", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtdetail, "idaset", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtdetail, "penyusutanke", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaipenyusutan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaibukusebelumnya", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt16)
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
            If (dataRowDetail.Length <> 24) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'iddadetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "iddadetail required numeric." : GoTo selesai
            End If
            'idda(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "idda required numeric." : GoTo selesai
            End If
            'idaset(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "idaset required numeric." : GoTo selesai
            End If
            'penyusutanke(3) As Integer
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "penyusutanke required numeric." : GoTo selesai
            End If
            'kurs(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "kurs required numeric." : GoTo selesai
            End If
            'nilaipenyusutan(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "nilaipenyusutan required numeric." : GoTo selesai
            End If
            'nilaibukusebelumnya(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "nilaibukusebelumnya required numeric." : GoTo selesai
            End If
            'urutan(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "urutan required numeric." : GoTo selesai
            End If
            'isclose(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "isclose required numeric." : GoTo selesai
            End If
            'customdbl1(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(20) As Double
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(21) As Date
            If (IsDate(dataRowDetail(21)) = False) Then
                result(2) = "customdate1 required date." : GoTo selesai
            End If
            'customdate2(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "customdate2 required date." : GoTo selesai
            End If
            'customdate3(23) As Date
            If (IsDate(dataRowDetail(23)) = False) Then
                result(2) = "customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'matauang(4) As String
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(4)) > 25 Then
                result(2) = "Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'nilaipenyusutan(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - nilaipenyusutan can't be empty" : GoTo selesai
            End If

            'nilaibukusebelumnya(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - nilaibukusebelumnya can't be empty" : GoTo selesai
            End If

            'customdbl1(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(21) As Date
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(23) As Date
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "iddadetail~idda~idaset~penyusutanke~matauang~kurs~nilaipenyusutan~nilaibukusebelumnya~costcenter~divisi~subdivisi~proyek~catatan~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23)) = False Then
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
                Dim vModuleId As Integer = 7, vMenuId As Integer = 12
                Select Case drutama("dastatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("datgl")), AsFormatTanggal(drutama("datgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                If isUpdate Then
                    result(4) = drutama("daid")
                    notransaksi = drutama("danotransaksi")

                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(daid), danotransaksi FROM M7_Da WHERE daid='" & result(4) & "' AND dastatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("daautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("dacabang"), drutama("dalokasi"), drutama("dasumber"), drutama("datgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(daid) FROM M7_Da WHERE danotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        sql = "Update M7_Da set dacabang  = '" & FixQuotes(drutama("dacabang")) & "', dalokasi  = '" & FixQuotes(drutama("dalokasi")) & "', dagudang  = '" & FixQuotes(drutama("dagudang")) & "', dasumber  = '" & FixQuotes(drutama("dasumber")) & "', daautonotransaksi  = " & drutama("daautonotransaksi") & ", danotransaksi  = '" & notransaksi & "', datgl  = '" & FixQuotes(AsFormatTanggal(drutama("datgl"))) & "', dakodepa  = " & drutama("dakodepa") & ", damatauang  = '" & FixQuotes(drutama("damatauang")) & "', dakurs  = '" & FixDouble(drutama("dakurs")) & "', dabagianda  = " & drutama("dabagianda") & ", dabagiandakontak  = '" & FixQuotes(drutama("dabagiandakontak")) & "', dauraian  = '" & FixQuotes(drutama("dauraian")) & "', dacatatan  = '" & FixQuotes(drutama("dacatatan")) & "', danoref  = '" & FixQuotes(drutama("danoref")) & "', datglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("datglnoref"))) & "', dastatus  = " & drutama("dastatus") & ", dastatussebelumnya  = " & drutama("dastatussebelumnya") & ", dajmlrevisi  = dajmlrevisi+1, dacetakanke  = " & drutama("dacetakanke") & ", damodifikasiuser  = " & drutama("damodifikasiuser") & ", damodifikasitgl  = NOW(), daposting  = 0, datutupperiode  = " & drutama("datutupperiode") & ", dacustomtext1  = '" & FixQuotes(drutama("dacustomtext1")) & "', dacustomtext2  = '" & FixQuotes(drutama("dacustomtext2")) & "', dacustomtext3  = '" & FixQuotes(drutama("dacustomtext3")) & "', dacustomtext4  = '" & FixQuotes(drutama("dacustomtext4")) & "', dacustomtext5  = '" & FixQuotes(drutama("dacustomtext5")) & "', dacustomint1  = " & drutama("dacustomint1") & ", dacustomint2  = " & drutama("dacustomint2") & ", dacustomint3  = " & drutama("dacustomint3") & ", dacustomdbl1  = '" & FixDouble(drutama("dacustomdbl1")) & "', dacustomdbl2  = '" & FixDouble(drutama("dacustomdbl2")) & "', dacustomdbl3  = '" & FixDouble(drutama("dacustomdbl3")) & "', dacustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("dacustomdate1"))) & "', dacustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("dacustomdate2"))) & "', dacustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("dacustomdate3"))) & "' where daid = '" & drutama("daid") & "'"
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

                    If drutama("daautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("dacabang"), drutama("dalokasi"), drutama("dasumber"), drutama("datgl"))
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
                        notransaksi = drutama("danotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(daid) FROM M7_Da WHERE danotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M7_Da (dacabang, dalokasi, dagudang, dasumber, daautonotransaksi, danotransaksi, datgl, dakodepa, damatauang, dakurs, dabagianda, dabagiandakontak, dauraian, dacatatan, danoref, datglnoref, dastatus, dastatussebelumnya, dajmlrevisi, dacetakanke, dainputuser, dainputtgl, damodifikasiuser, damodifikasitgl, daposting, datutupperiode, daisclose, dacustomtext1, dacustomtext2, dacustomtext3, dacustomtext4, dacustomtext5, dacustomint1, dacustomint2, dacustomint3, dacustomdbl1, dacustomdbl2, dacustomdbl3, dacustomdate1, dacustomdate2, dacustomdate3) values('" & FixQuotes(drutama("dacabang")) & "', '" & FixQuotes(drutama("dalokasi")) & "', '" & FixQuotes(drutama("dagudang")) & "', '" & FixQuotes(drutama("dasumber")) & "', " & drutama("daautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("datgl"))) & "', " & drutama("dakodepa") & ", '" & FixQuotes(drutama("damatauang")) & "', '" & FixDouble(drutama("dakurs")) & "', " & drutama("dabagianda") & ", '" & FixQuotes(drutama("dabagiandakontak")) & "', '" & FixQuotes(drutama("dauraian")) & "', '" & FixQuotes(drutama("dacatatan")) & "', '" & FixQuotes(drutama("danoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("datglnoref"))) & "', " & drutama("dastatus") & ", " & drutama("dastatussebelumnya") & ", " & drutama("dajmlrevisi") & ", " & drutama("dacetakanke") & ", " & drutama("dainputuser") & ", NOW(), " & drutama("damodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("datutupperiode") & ", " & drutama("daisclose") & ", '" & FixQuotes(drutama("dacustomtext1")) & "', '" & FixQuotes(drutama("dacustomtext2")) & "', '" & FixQuotes(drutama("dacustomtext3")) & "', '" & FixQuotes(drutama("dacustomtext4")) & "', '" & FixQuotes(drutama("dacustomtext5")) & "', " & drutama("dacustomint1") & ", " & drutama("dacustomint2") & ", " & drutama("dacustomint3") & ", '" & FixDouble(drutama("dacustomdbl1")) & "', '" & FixDouble(drutama("dacustomdbl2")) & "', '" & FixDouble(drutama("dacustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("dacustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("dacustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("dacustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select daid from M7_Da where danotransaksi='" & notransaksi & "' AND dainputuser= '" & userid & "' order by damodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M7_Da_Detail where idda = '" & result(4) & "'"
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
                        'QUERY INSERT DETAIL
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("iddadetail") & ", " & result(4) & ", " & dr1("idaset") & ", " & dr1("penyusutanke") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("nilaipenyusutan")) & "', '" & FixDouble(dr1("nilaibukusebelumnya")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")

                        'PROSES TRANSAKSI TERKAIT -----------------------------------------
                        If drutama("dastatus") = 2 Then
                            'UPDATE PENYUSUTAN KE, AKUMULASI BEBAN, NILAI BUKU PADA MASTER ASET
                            sql = "UPDATE m7_asset SET apenyusutanke = apenyusutanke + 1, aakumulasibeban = aakumulasibeban + (" & Double.Parse(FixDouble(dr1("nilaipenyusutan"))) & " * " & Double.Parse(FixDouble(dr1("kurs"))) & "), anilaibuku = anilaibuku - (" & Double.Parse(FixDouble(dr1("nilaipenyusutan"))) & " * " & Double.Parse(FixDouble(dr1("kurs"))) & ") WHERE aid = '" & FixDouble(dr1("idaset")) & "'"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                        'END OF PROSES TRANSAKSI TERKAIT ----------------------------------
                    Next
                    sql = "Insert into M7_Da_Detail(iddadetail, idda, idaset, penyusutanke, matauang, kurs, nilaipenyusutan, nilaibukusebelumnya, costcenter, divisi, subdivisi, proyek, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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


                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "Da", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("Dastatus") = 2 Then
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


                'INSERT USER LOG ==================================================================
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
    Public Function M7_DaUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("dabagiandakode", "c1.kkode")
            Filter = Filter.Replace("dabagiandanama", "c1.knama")
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
            Dim sumber As String = "Da", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Datgl, Danotransaksi, Dastatus FROM M7_Da WHERE Daid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Dastatussebelumnya" : jnsaktivitas = 17
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

            If isDelete Then

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT iddadetail FROM m7_da_detail WHERE idda = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    Dim iddetail As Double = 0

                    For Each dr1 As DataRow In dtdetail.Rows
                        'SET VARIABEL
                        iddetail = Double.Parse(dr1("iddadetail"))

                        'UPDATE PENYUSUTAN KE, AKUMULASI BEBAN, NILAI BUKU PADA MASTER ASET
                        sql = "UPDATE m7_asset a JOIN m7_da_detail dad ON a.aid = dad.idaset SET a.apenyusutanke = a.apenyusutanke - 1, a.aakumulasibeban = a.aakumulasibeban - (dad.nilaipenyusutan * dad.kurs), a.anilaibuku = a.anilaibuku + (dad.nilaipenyusutan * dad.kurs) WHERE dad.iddadetail = '" & FixDouble(iddetail) & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Next

                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If


                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'Da' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M7_Da SET Dastatus = " & nilaiStatus & ", Damodifikasiuser='" & userid & "', Damodifikasitgl = NOW(), Daposting = 0, Dapostingtgl = '1971-01-01 00:00:00', Dajmlrevisi = Dajmlrevisi + 1 WHERE Daid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M7_DaSearch(PostWsSearch(paramSplit(0), "M7_DaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M7_DaDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("dabagiandakode", "c1.kkode")
            Filter = Filter.Replace("dabagiandanama", "c1.knama")
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
            Dim sumber As String = "Da", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Daid, Danotransaksi FROM M7_Da WHERE Daid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT dacabang, dalokasi, dasumber, daautonotransaksi, danotransaksi, datgl"
            sql &= " FROM M7_da"
            sql &= " WHERE daid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("dacabang")
                lokasi = dtNomorNext.Rows(0)("dalokasi")
                sumber = dtNomorNext.Rows(0)("dasumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("daautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("danotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("datgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE JURNAL
            sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'Da' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M7_Da_Detail WHERE idDa = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M7_Da WHERE Daid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M7_DaSearch(PostWsSearch(paramSplit(0), "M7_DaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M7_DaGetdataById(ByVal param As String) As String

        'M7_DaGetdataById Utama --------------------------------------------------------
        'daid, dacabang, dalokasi, dagudang, dasumber, daautonotransaksi, danotransaksi, 
        'datgl, dakodepa, damatauang, dakurs, dabagianda, dabagiandakontak, dauraian, 
        'dacatatan, danoref, datglnoref, dastatus, dastatussebelumnya, dajmlrevisi, dacetakanke, 
        'dainputuser, dainputtgl, damodifikasiuser, damodifikasitgl, daposting, dapostingtgl, datutupperiode, 
        'daisclose, dacustomtext1, dacustomtext2, dacustomtext3, dacustomtext4, dacustomtext5, dacustomint1, 
        'dacustomint2, dacustomint3, dacustomdbl1, dacustomdbl2, dacustomdbl3, dacustomdate1, dacustomdate2, 
        'dacustomdate3, dacabangnama, dalokasinama, dagudangnama, dabagiandakode, dabagiandanama, dastatusnama, 
        'dastatussebelumnyanama, dainputusernama, damodifikasiusernama

        'M7_DaGetdataById Detail -------------------------------------------------------
        'iddadetail, idda, idaset, penyusutanke, 
        'matauang, kurs, nilaipenyusutan, nilaibukusebelumnya, costcenter, divisi, subdivisi, 
        'proyek, catatan, urutan, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, asetkode, 
        'asetnama, asetkategori, asetkategorinama, asettglbeli, asethargabeli, asetnilairesidu, asetumurekonomis, 
        'asetbebanperbln, asetakumulasibeban, asetnilaibuku, asetmetode, asetmetodenama, asetstatus, asetstatusnama

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

        Dim NmMemcached As String = "aplikasi1-M7_Da~M7_Da_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "Daid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "Daid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m7_da_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("daid"), ""), sptField,
                     FxDB(drutama("dacabang"), ""), sptField,
                     FxDB(drutama("dalokasi"), ""), sptField,
                     FxDB(drutama("dagudang"), ""), sptField,
                     FxDB(drutama("dasumber"), ""), sptField,
                     FxDB(drutama("daautonotransaksi"), 0), sptField,
                     FxDB(drutama("danotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("datgl"), ""), formatTgl), sptField,
                     FxDB(drutama("dakodepa"), ""), sptField,
                     FxDB(drutama("damatauang"), ""), sptField,
                     FxDB(drutama("dakurs"), 0), sptField,
                     FxDB(drutama("dabagianda"), ""), sptField,
                     FxDB(drutama("dabagiandakontak"), ""), sptField,
                     FxDB(drutama("dauraian"), ""), sptField,
                     FxDB(drutama("dacatatan"), ""), sptField,
                     FxDB(drutama("danoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("datglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("dastatus"), 0), sptField,
                     FxDB(drutama("dastatussebelumnya"), 0), sptField,
                     FxDB(drutama("dajmlrevisi"), 0), sptField,
                     FxDB(drutama("dacetakanke"), 0), sptField,
                     FxDB(drutama("dainputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("dainputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("damodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("damodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("daposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("dapostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("datutupperiode"), 0), sptField,
                     FxDB(drutama("daisclose"), 0), sptField,
                     FxDB(drutama("dacustomtext1"), ""), sptField,
                     FxDB(drutama("dacustomtext2"), ""), sptField,
                     FxDB(drutama("dacustomtext3"), ""), sptField,
                     FxDB(drutama("dacustomtext4"), ""), sptField,
                     FxDB(drutama("dacustomtext5"), ""), sptField,
                     FxDB(drutama("dacustomint1"), 0), sptField,
                     FxDB(drutama("dacustomint2"), 0), sptField,
                     FxDB(drutama("dacustomint3"), 0), sptField,
                     FxDB(drutama("dacustomdbl1"), 0), sptField,
                     FxDB(drutama("dacustomdbl2"), 0), sptField,
                     FxDB(drutama("dacustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("dacustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("dacustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("dacustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("dacabangnama"), ""), sptField,
                     FxDB(drutama("dalokasinama"), ""), sptField,
                     FxDB(drutama("dagudangnama"), ""), sptField,
                     FxDB(drutama("dabagiandakode"), ""), sptField,
                     FxDB(drutama("dabagiandanama"), ""), sptField,
                     FxDB(drutama("dastatusnama"), ""), sptField,
                     FxDB(drutama("dastatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("dainputusernama"), ""), sptField,
                     FxDB(drutama("damodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("iddadetail"), ""), sptField,
                     FxDB(dr("idda"), ""), sptField,
                     FxDB(dr("idaset"), ""), sptField,
                     FxDB(dr("penyusutanke"), 0), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("nilaipenyusutan"), 0), sptField,
                     FxDB(dr("nilaibukusebelumnya"), 0), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
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
                     FxDB(dr("asetkode"), ""), sptField,
                     FxDB(dr("asetnama"), ""), sptField,
                     FxDB(dr("asetkategori"), ""), sptField,
                     FxDB(dr("asetkategorinama"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("asettglbeli"), "1900-01-01"), formatTgl), sptField,
                     FxDB(dr("asethargabeli"), 0), sptField,
                     FxDB(dr("asetnilairesidu"), 0), sptField,
                     FxDB(dr("asetumurekonomis"), 0), sptField,
                     FxDB(dr("asetbebanperbln"), 0), sptField,
                     FxDB(dr("asetakumulasibeban"), 0), sptField,
                     FxDB(dr("asetnilaibuku"), 0), sptField,
                     FxDB(dr("asetmetode"), 0), sptField,
                     FxDB(dr("asetmetodenama"), ""), sptField,
                     FxDB(dr("asetstatus"), 0), sptField,
                     FxDB(dr("asetstatusnama"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "DA transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, detail)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("daid, dacabang, dalokasi, dagudang, dasumber, daautonotransaksi, danotransaksi, datgl, dakodepa, damatauang, dakurs, dabagianda, dabagiandakontak, dauraian, dacatatan, danoref, datglnoref, dastatus, dastatussebelumnya, dajmlrevisi, dacetakanke, dainputuser, dainputtgl, damodifikasiuser, damodifikasitgl, daposting, dapostingtgl, datutupperiode, daisclose, dacustomtext1, dacustomtext2, dacustomtext3, dacustomtext4, dacustomtext5, dacustomint1, dacustomint2, dacustomint3, dacustomdbl1, dacustomdbl2, dacustomdbl3, dacustomdate1, dacustomdate2, dacustomdate3, dacabangnama, dalokasinama, dagudangnama, dabagiandakode, dabagiandanama, dastatusnama, dastatussebelumnyanama, dainputusernama, damodifikasiusernama" & sptSubParam & "iddadetail, idda, idaset, penyusutanke, matauang, kurs, nilaipenyusutan, nilaibukusebelumnya, costcenter, divisi, subdivisi, proyek, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, asetkode, asetnama, asetkategori, asetkategorinama, asettglbeli, asethargabeli, asetnilairesidu, asetumurekonomis, asetbebanperbln, asetakumulasibeban, asetnilaibuku, asetmetode, asetmetodenama, asetstatus, asetstatusnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M7_DaSearch(ByVal param As String) As String
        'M7_DaSearch --------------------------------------------------------
        'daid, dacabang, dalokasi, dagudang, dasumber, daautonotransaksi, danotransaksi, 
        'datgl, dakodepa, damatauang, dakurs, dabagianda, dabagiandakontak, dauraian, 
        'dacatatan, danoref, datglnoref, dastatus, dastatussebelumnya, dajmlrevisi, dacetakanke, 
        'dainputuser, dainputtgl, damodifikasiuser, damodifikasitgl, daposting, dapostingtgl, datutupperiode, 
        'daisclose, dacabangnama, dalokasinama, dagudangnama, dabagiandakode, dabagiandanama, dastatusnama, 
        'dastatussebelumnyanama, dainputusernama, damodifikasiusernama

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
            Filter = Filter.Replace("dabagiandakode", "c1.kkode")
            Filter = Filter.Replace("dabagiandanama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m7_da_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M7_Da", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("daid"), ""), sptField,
                     FxDB(dr("dacabang"), ""), sptField,
                     FxDB(dr("dalokasi"), ""), sptField,
                     FxDB(dr("dagudang"), ""), sptField,
                     FxDB(dr("dasumber"), ""), sptField,
                     FxDB(dr("daautonotransaksi"), 0), sptField,
                     FxDB(dr("danotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("datgl"), ""), formatTgl), sptField,
                     FxDB(dr("dakodepa"), ""), sptField,
                     FxDB(dr("damatauang"), ""), sptField,
                     FxDB(dr("dakurs"), 0), sptField,
                     FxDB(dr("dabagianda"), ""), sptField,
                     FxDB(dr("dabagiandakontak"), ""), sptField,
                     FxDB(dr("dauraian"), ""), sptField,
                     FxDB(dr("dacatatan"), ""), sptField,
                     FxDB(dr("danoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("datglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("dastatus"), 0), sptField,
                     FxDB(dr("dastatussebelumnya"), 0), sptField,
                     FxDB(dr("dajmlrevisi"), 0), sptField,
                     FxDB(dr("dacetakanke"), 0), sptField,
                     FxDB(dr("dainputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dainputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("damodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("damodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("daposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("dapostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("datutupperiode"), 0), sptField,
                     FxDB(dr("daisclose"), 0), sptField,
                     FxDB(dr("dacabangnama"), ""), sptField,
                     FxDB(dr("dalokasinama"), ""), sptField,
                     FxDB(dr("dagudangnama"), ""), sptField,
                     FxDB(dr("dabagiandakode"), ""), sptField,
                     FxDB(dr("dabagiandanama"), ""), sptField,
                     FxDB(dr("dastatusnama"), ""), sptField,
                     FxDB(dr("dastatussebelumnyanama"), ""), sptField,
                     FxDB(dr("dainputusernama"), ""), sptField,
                     FxDB(dr("damodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("daid, dacabang, dalokasi, dagudang, dasumber, daautonotransaksi, danotransaksi, datgl, dakodepa, damatauang, dakurs, dabagianda, dabagiandakontak, dauraian, dacatatan, danoref, datglnoref, dastatus, dastatussebelumnya, dajmlrevisi, dacetakanke, dainputuser, dainputtgl, damodifikasiuser, damodifikasitgl, daposting, dapostingtgl, datutupperiode, daisclose, dacabangnama, dalokasinama, dagudangnama, dabagiandakode, dabagiandanama, dastatusnama, dastatussebelumnyanama, dainputusernama, damodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M7_DaTerkait(ByVal param As String) As String
        'M7_DaTerkait --------------------------------------------------------
        'daid, danotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "daid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        ''PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("M7_Da_terkait")
        'sql = sql.Replace("validtransaksi", idtransaksi)

        ''BUKA KONEKSI
        'Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        'Con1.Open()

        'dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        'pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("daid"), 0), sptField,
                     FxDB(dr("danotransaksi"), ""), sptField,
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
            result(2) = "Related DA data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("daid, danotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M7_DaSimpanOld(ByVal param As String) As String
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
        'daid(0) As Integer, dacabang(1) As String, dalokasi(2) As String, dagudang(3) As String, dasumber(4) As String, 
        'daautonotransaksi(5) As Integer, danotransaksi(6) As String, datgl(7) As Date, dakodepa(8) As Integer, damatauang(9) As String, 
        'dakurs(10) As Double, dabagianda(11) As Integer, dabagiandakontak(12) As String, dauraian(13) As String, dacatatan(14) As String, 
        'danoref(15) As String, datglnoref(16) As Date, dastatus(17) As Integer, dastatussebelumnya(18) As Integer, dajmlrevisi(19) As Integer, 
        'dacetakanke(20) As Integer, dainputuser(21) As Integer, dainputtgl(22) As DateTime, damodifikasiuser(23) As Integer, damodifikasitgl(24) As DateTime, 
        'daposting(25) As Integer, dapostingtgl(26) As DateTime, datutupperiode(27) As Integer, daisclose(28) As Integer, dacustomtext1(29) As String, 
        'dacustomtext2(30) As String, dacustomtext3(31) As String, dacustomtext4(32) As String, dacustomtext5(33) As String, dacustomint1(34) As Integer, 
        'dacustomint2(35) As Integer, dacustomint3(36) As Integer, dacustomdbl1(37) As Double, dacustomdbl2(38) As Double, dacustomdbl3(39) As Double, 
        'dacustomdate1(40) As Date, dacustomdate2(41) As Date, dacustomdate3(42) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'daid, dacabang, dalokasi, dagudang, dasumber, daautonotransaksi, danotransaksi, 
        'datgl, dakodepa, damatauang, dakurs, dabagianda, dabagiandakontak, dauraian, 
        'dacatatan, danoref, datglnoref, dastatus, dastatussebelumnya, dajmlrevisi, dacetakanke, 
        'dainputuser, dainputtgl, damodifikasiuser, damodifikasitgl, daposting, dapostingtgl, datutupperiode, 
        'daisclose, dacustomtext1, dacustomtext2, dacustomtext3, dacustomtext4, dacustomtext5, dacustomint1, 
        'dacustomint2, dacustomint3, dacustomdbl1, dacustomdbl2, dacustomdbl3, dacustomdate1, dacustomdate2, 
        'dacustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 43) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'daid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "daid required numeric." : GoTo selesai
        End If
        'daautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "daautonotransaksi required numeric." : GoTo selesai
        End If
        'datgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "datgl required date." : GoTo selesai
        End If
        'dakodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "dakodepa required numeric." : GoTo selesai
        End If
        'dakurs(10) As Double
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "dakurs required numeric." : GoTo selesai
        End If
        'dabagianda(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "dabagianda required numeric." : GoTo selesai
        End If
        'datglnoref(16) As Date
        If (IsDate(dataUtama(16)) = False) Then
            result(2) = "datglnoref required date." : GoTo selesai
        End If
        'dastatus(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "dastatus required numeric." : GoTo selesai
        End If
        'dastatussebelumnya(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "dastatussebelumnya required numeric." : GoTo selesai
        End If
        'dajmlrevisi(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "dajmlrevisi required numeric." : GoTo selesai
        End If
        'dacetakanke(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "dacetakanke required numeric." : GoTo selesai
        End If
        'dainputuser(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "dainputuser required numeric." : GoTo selesai
        End If
        'dainputtgl(22) As DateTime
        If (IsDate(dataUtama(22)) = False) Then
            result(2) = "dainputtgl required date." : GoTo selesai
        End If
        'damodifikasiuser(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "damodifikasiuser required numeric." : GoTo selesai
        End If
        'damodifikasitgl(24) As DateTime
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "damodifikasitgl required date." : GoTo selesai
        End If
        'daposting(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "daposting required numeric." : GoTo selesai
        End If
        'dapostingtgl(26) As DateTime
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "dapostingtgl required date." : GoTo selesai
        End If
        'datutupperiode(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "datutupperiode required numeric." : GoTo selesai
        End If
        'daisclose(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "daisclose required numeric." : GoTo selesai
        End If
        'dacustomint1(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "dacustomint1 required numeric." : GoTo selesai
        End If
        'dacustomint2(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "dacustomint2 required numeric." : GoTo selesai
        End If
        'dacustomint3(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "dacustomint3 required numeric." : GoTo selesai
        End If
        'dacustomdbl1(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "dacustomdbl1 required numeric." : GoTo selesai
        End If
        'dacustomdbl2(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "dacustomdbl2 required numeric." : GoTo selesai
        End If
        'dacustomdbl3(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "dacustomdbl3 required numeric." : GoTo selesai
        End If
        'dacustomdate1(40) As Date
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "dacustomdate1 required date." : GoTo selesai
        End If
        'dacustomdate2(41) As Date
        If (IsDate(dataUtama(41)) = False) Then
            result(2) = "dacustomdate2 required date." : GoTo selesai
        End If
        'dacustomdate3(42) As Date
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "dacustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'dacabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "dacabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "dacabang should not be more than 25 character." : GoTo selesai
        End If

        'dalokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "dalokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "dalokasi should not be more than 25 character." : GoTo selesai
        End If

        'dasumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "dasumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "dasumber should not be more than 10 character." : GoTo selesai
        End If

        'danotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "danotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "danotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'datgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "datgl can't be empty" : GoTo selesai
        End If

        'damatauang(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "damatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 25 Then
            result(2) = "damatauang should not be more than 25 character." : GoTo selesai
        End If

        'dakurs(10) As Double
        If Len(dataUtama(10)) = 0 Then
            result(2) = "dakurs can't be empty" : GoTo selesai
        End If

        'datglnoref(16) As Date
        If Len(dataUtama(16)) = 0 Then
            result(2) = "datglnoref can't be empty" : GoTo selesai
        End If

        'dainputtgl(22) As DateTime
        If Len(dataUtama(22)) = 0 Then
            result(2) = "dainputtgl can't be empty" : GoTo selesai
        End If

        'damodifikasitgl(24) As DateTime
        If Len(dataUtama(24)) = 0 Then
            result(2) = "damodifikasitgl can't be empty" : GoTo selesai
        End If

        'dapostingtgl(26) As DateTime
        If Len(dataUtama(26)) = 0 Then
            result(2) = "dapostingtgl can't be empty" : GoTo selesai
        End If

        'dacustomdbl1(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "dacustomdbl1 can't be empty" : GoTo selesai
        End If

        'dacustomdbl2(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "dacustomdbl2 can't be empty" : GoTo selesai
        End If

        'dacustomdbl3(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "dacustomdbl3 can't be empty" : GoTo selesai
        End If

        'dacustomdate1(40) As Date
        If Len(dataUtama(40)) = 0 Then
            result(2) = "dacustomdate1 can't be empty" : GoTo selesai
        End If

        'dacustomdate2(41) As Date
        If Len(dataUtama(41)) = 0 Then
            result(2) = "dacustomdate2 can't be empty" : GoTo selesai
        End If

        'dacustomdate3(42) As Date
        If Len(dataUtama(42)) = 0 Then
            result(2) = "dacustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "daid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dacabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dalokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dagudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dasumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "daautonotransaksi", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "danotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "datgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dakodepa", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "damatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dakurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dabagianda", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "dabagiandakontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dauraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dacatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "danoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "datglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dastatus", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "dastatussebelumnya", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "dajmlrevisi", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "dacetakanke", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "dainputuser", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "dainputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "damodifikasiuser", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "damodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "daposting", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "dapostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "datutupperiode", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "daisclose", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "dacustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dacustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dacustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dacustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dacustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dacustomint1", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "dacustomint2", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "dacustomint3", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "dacustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dacustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dacustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dacustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dacustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dacustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "daid~dacabang~dalokasi~dagudang~dasumber~daautonotransaksi~danotransaksi~datgl~dakodepa~damatauang~dakurs~dabagianda~dabagiandakontak~dauraian~dacatatan~danoref~datglnoref~dastatus~dastatussebelumnya~dajmlrevisi~dacetakanke~dainputuser~dainputtgl~damodifikasiuser~damodifikasitgl~daposting~dapostingtgl~datutupperiode~daisclose~dacustomtext1~dacustomtext2~dacustomtext3~dacustomtext4~dacustomtext5~dacustomint1~dacustomint2~dacustomint3~dacustomdbl1~dacustomdbl2~dacustomdbl3~dacustomdate1~dacustomdate2~dacustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'iddadetail(0) As Integer, idda(1) As Integer, idaset(2) As Integer, penyusutanke(3) As Integer, matauang(4) As String, 
        'kurs(5) As Double, nilaipenyusutan(6) As Double, nilaibukusebelumnya(7) As Double, costcenter(8) As String, divisi(9) As String, 
        'subdivisi(10) As String, proyek(11) As String, catatan(12) As String, urutan(13) As Integer, isclose(14) As Integer, 
        'customtext1(15) As String, customtext2(16) As String, customtext3(17) As String, customdbl1(18) As Double, customdbl2(19) As Double, 
        'customdbl3(20) As Double, customdate1(21) As Date, customdate2(22) As Date, customdate3(23) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'iddadetail, idda, idaset, penyusutanke, matauang, kurs, nilaipenyusutan, 
        'nilaibukusebelumnya, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "iddadetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idda", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtdetail, "idaset", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtdetail, "penyusutanke", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaipenyusutan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaibukusebelumnya", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt16)
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
            If (dataRowDetail.Length <> 24) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'iddadetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "iddadetail required numeric." : GoTo selesai
            End If
            'idda(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "idda required numeric." : GoTo selesai
            End If
            'idaset(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "idaset required numeric." : GoTo selesai
            End If
            'penyusutanke(3) As Integer
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "penyusutanke required numeric." : GoTo selesai
            End If
            'kurs(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "kurs required numeric." : GoTo selesai
            End If
            'nilaipenyusutan(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "nilaipenyusutan required numeric." : GoTo selesai
            End If
            'nilaibukusebelumnya(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "nilaibukusebelumnya required numeric." : GoTo selesai
            End If
            'urutan(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "urutan required numeric." : GoTo selesai
            End If
            'isclose(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "isclose required numeric." : GoTo selesai
            End If
            'customdbl1(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(20) As Double
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(21) As Date
            If (IsDate(dataRowDetail(21)) = False) Then
                result(2) = "customdate1 required date." : GoTo selesai
            End If
            'customdate2(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "customdate2 required date." : GoTo selesai
            End If
            'customdate3(23) As Date
            If (IsDate(dataRowDetail(23)) = False) Then
                result(2) = "customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'matauang(4) As String
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(4)) > 25 Then
                result(2) = "Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'nilaipenyusutan(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - nilaipenyusutan can't be empty" : GoTo selesai
            End If

            'nilaibukusebelumnya(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - nilaibukusebelumnya can't be empty" : GoTo selesai
            End If

            'customdbl1(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(21) As Date
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(23) As Date
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "iddadetail~idda~idaset~penyusutanke~matauang~kurs~nilaipenyusutan~nilaibukusebelumnya~costcenter~divisi~subdivisi~proyek~catatan~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23)) = False Then
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("datgl")), AsFormatTanggal(drutama("datgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                If isUpdate Then
                    result(4) = drutama("daid")
                    notransaksi = drutama("danotransaksi")

                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(daid), danotransaksi FROM M7_Da WHERE daid='" & result(4) & "' AND dastatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(daid) FROM M7_Da WHERE danotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        sql = "Update M7_Da set dacabang  = '" & FixQuotes(drutama("dacabang")) & "', dalokasi  = '" & FixQuotes(drutama("dalokasi")) & "', dagudang  = '" & FixQuotes(drutama("dagudang")) & "', dasumber  = '" & FixQuotes(drutama("dasumber")) & "', daautonotransaksi  = " & drutama("daautonotransaksi") & ", danotransaksi  = '" & notransaksi & "', datgl  = '" & FixQuotes(AsFormatTanggal(drutama("datgl"))) & "', dakodepa  = " & drutama("dakodepa") & ", damatauang  = '" & FixQuotes(drutama("damatauang")) & "', dakurs  = '" & FixDouble(drutama("dakurs")) & "', dabagianda  = " & drutama("dabagianda") & ", dabagiandakontak  = '" & FixQuotes(drutama("dabagiandakontak")) & "', dauraian  = '" & FixQuotes(drutama("dauraian")) & "', dacatatan  = '" & FixQuotes(drutama("dacatatan")) & "', danoref  = '" & FixQuotes(drutama("danoref")) & "', datglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("datglnoref"))) & "', dastatus  = " & drutama("dastatus") & ", dastatussebelumnya  = " & drutama("dastatussebelumnya") & ", dajmlrevisi  = dajmlrevisi+1, dacetakanke  = " & drutama("dacetakanke") & ", damodifikasiuser  = " & drutama("damodifikasiuser") & ", damodifikasitgl  = NOW(), daposting  = 0, datutupperiode  = " & drutama("datutupperiode") & ", dacustomtext1  = '" & FixQuotes(drutama("dacustomtext1")) & "', dacustomtext2  = '" & FixQuotes(drutama("dacustomtext2")) & "', dacustomtext3  = '" & FixQuotes(drutama("dacustomtext3")) & "', dacustomtext4  = '" & FixQuotes(drutama("dacustomtext4")) & "', dacustomtext5  = '" & FixQuotes(drutama("dacustomtext5")) & "', dacustomint1  = " & drutama("dacustomint1") & ", dacustomint2  = " & drutama("dacustomint2") & ", dacustomint3  = " & drutama("dacustomint3") & ", dacustomdbl1  = '" & FixDouble(drutama("dacustomdbl1")) & "', dacustomdbl2  = '" & FixDouble(drutama("dacustomdbl2")) & "', dacustomdbl3  = '" & FixDouble(drutama("dacustomdbl3")) & "', dacustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("dacustomdate1"))) & "', dacustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("dacustomdate2"))) & "', dacustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("dacustomdate3"))) & "' where daid = '" & drutama("daid") & "'"
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

                    If drutama("daautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("dacabang"), drutama("dalokasi"), drutama("dasumber"), drutama("datgl"))
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
                        notransaksi = drutama("danotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(daid) FROM M7_Da WHERE danotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M7_Da (dacabang, dalokasi, dagudang, dasumber, daautonotransaksi, danotransaksi, datgl, dakodepa, damatauang, dakurs, dabagianda, dabagiandakontak, dauraian, dacatatan, danoref, datglnoref, dastatus, dastatussebelumnya, dajmlrevisi, dacetakanke, dainputuser, dainputtgl, damodifikasiuser, damodifikasitgl, daposting, datutupperiode, daisclose, dacustomtext1, dacustomtext2, dacustomtext3, dacustomtext4, dacustomtext5, dacustomint1, dacustomint2, dacustomint3, dacustomdbl1, dacustomdbl2, dacustomdbl3, dacustomdate1, dacustomdate2, dacustomdate3) values('" & FixQuotes(drutama("dacabang")) & "', '" & FixQuotes(drutama("dalokasi")) & "', '" & FixQuotes(drutama("dagudang")) & "', '" & FixQuotes(drutama("dasumber")) & "', " & drutama("daautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("datgl"))) & "', " & drutama("dakodepa") & ", '" & FixQuotes(drutama("damatauang")) & "', '" & FixDouble(drutama("dakurs")) & "', " & drutama("dabagianda") & ", '" & FixQuotes(drutama("dabagiandakontak")) & "', '" & FixQuotes(drutama("dauraian")) & "', '" & FixQuotes(drutama("dacatatan")) & "', '" & FixQuotes(drutama("danoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("datglnoref"))) & "', " & drutama("dastatus") & ", " & drutama("dastatussebelumnya") & ", " & drutama("dajmlrevisi") & ", " & drutama("dacetakanke") & ", " & drutama("dainputuser") & ", NOW(), " & drutama("damodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("datutupperiode") & ", " & drutama("daisclose") & ", '" & FixQuotes(drutama("dacustomtext1")) & "', '" & FixQuotes(drutama("dacustomtext2")) & "', '" & FixQuotes(drutama("dacustomtext3")) & "', '" & FixQuotes(drutama("dacustomtext4")) & "', '" & FixQuotes(drutama("dacustomtext5")) & "', " & drutama("dacustomint1") & ", " & drutama("dacustomint2") & ", " & drutama("dacustomint3") & ", '" & FixDouble(drutama("dacustomdbl1")) & "', '" & FixDouble(drutama("dacustomdbl2")) & "', '" & FixDouble(drutama("dacustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("dacustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("dacustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("dacustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select daid from M7_Da where danotransaksi='" & notransaksi & "' AND dainputuser= '" & userid & "' order by damodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M7_Da_Detail where idda = '" & result(4) & "'"
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
                        'QUERY INSERT DETAIL
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("iddadetail") & ", " & result(4) & ", " & dr1("idaset") & ", " & dr1("penyusutanke") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("nilaipenyusutan")) & "', '" & FixDouble(dr1("nilaibukusebelumnya")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")

                        'PROSES TRANSAKSI TERKAIT -----------------------------------------
                        If drutama("dastatus") = 2 Then
                            'UPDATE PENYUSUTAN KE, AKUMULASI BEBAN, NILAI BUKU PADA MASTER ASET
                            sql = "UPDATE m7_asset SET apenyusutanke = apenyusutanke + 1, aakumulasibeban = aakumulasibeban + (" & Double.Parse(FixDouble(dr1("nilaipenyusutan"))) & " * " & Double.Parse(FixDouble(dr1("kurs"))) & "), anilaibuku = anilaibuku - (" & Double.Parse(FixDouble(dr1("nilaipenyusutan"))) & " * " & Double.Parse(FixDouble(dr1("kurs"))) & ") WHERE aid = '" & FixDouble(dr1("idaset")) & "'"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                        'END OF PROSES TRANSAKSI TERKAIT ----------------------------------
                    Next
                    sql = "Insert into M7_Da_Detail(iddadetail, idda, idaset, penyusutanke, matauang, kurs, nilaipenyusutan, nilaibukusebelumnya, costcenter, divisi, subdivisi, proyek, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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


                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "Da", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("Dastatus") = 2 Then
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


                'INSERT USER LOG ==================================================================
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
    Public Function M7_DaUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("dabagiandakode", "c1.kkode")
            Filter = Filter.Replace("dabagiandanama", "c1.knama")
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
            Dim sumber As String = "Da", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Datgl, Danotransaksi, Dastatus FROM M7_Da WHERE Daid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Dastatussebelumnya" : jnsaktivitas = 17
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

            If isDelete Then

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDB("SELECT iddadetail FROM m7_da_detail WHERE idda = '" & idtransaksi & "'")
                If dtdetail.Rows.Count > 0 Then
                    Dim iddetail As Double = 0

                    For Each dr1 As DataRow In dtdetail.Rows
                        'SET VARIABEL
                        iddetail = Double.Parse(dr1("iddadetail"))

                        'UPDATE PENYUSUTAN KE, AKUMULASI BEBAN, NILAI BUKU PADA MASTER ASET
                        sql = "UPDATE m7_asset a JOIN m7_da_detail dad ON a.aid = dad.idaset SET a.apenyusutanke = a.apenyusutanke - 1, a.aakumulasibeban = a.aakumulasibeban - (dad.nilaipenyusutan * dad.kurs), a.anilaibuku = a.anilaibuku + (dad.nilaipenyusutan * dad.kurs) WHERE dad.iddadetail = '" & FixDouble(iddetail) & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Next

                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If


                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'Da' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M7_Da SET Dastatus = " & nilaiStatus & ", Damodifikasiuser='" & userid & "', Damodifikasitgl = NOW(), Daposting = 0, Dapostingtgl = '1971-01-01 00:00:00', Dajmlrevisi = Dajmlrevisi + 1 WHERE Daid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M7_DaSearch(PostWsSearch(paramSplit(0), "M7_DaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M7_DaDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("dabagiandakode", "c1.kkode")
            Filter = Filter.Replace("dabagiandanama", "c1.knama")
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
            Dim sumber As String = "Da", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Daid, Danotransaksi FROM M7_Da WHERE Daid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT dacabang, dalokasi, dasumber, daautonotransaksi, danotransaksi, datgl"
            sql &= " FROM M7_da"
            sql &= " WHERE daid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("dacabang")
                lokasi = dtNomorNext.Rows(0)("dalokasi")
                sumber = dtNomorNext.Rows(0)("dasumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("daautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("danotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("datgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE JURNAL
            sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'Da' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M7_Da_Detail WHERE idDa = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M7_Da WHERE Daid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M7_DaSearch(PostWsSearch(paramSplit(0), "M7_DaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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