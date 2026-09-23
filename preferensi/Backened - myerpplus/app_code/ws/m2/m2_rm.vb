Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m2_rm
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M2_RmSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataGiro(), dataRowGiro() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean
        Dim strRekCostCenter As String = ""

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
        'rmid(0) As Integer, rmcabang(1) As String, rmlokasi(2) As String, rmsumber(3) As String, rmautonotransaksi(4) As Integer, 
        'rmnotransaksi(5) As String, rmtgl(6) As Date, rmkodepa(7) As Integer, rmcarabayar(8) As Integer, rmkontak(9) As Integer, 
        'rmkontakperson(10) As String, rmnorek(11) As String, rmuraian(12) As String, rmcatatan(13) As String, rmmatauang(14) As String, 
        'rmkurs(15) As Double, rmjumlah(16) As Double, rmjumlahvalas(17) As Double, rmjumlahbayar(18) As Double, rmjumlahbayarvalas(19) As Double, 
        'rmstatusbayar(20) As Integer, rmtgllunas(21) As Date, rmstatus(22) As Integer, rmstatussebelumnya(23) As Integer, rmjmlrevisi(24) As Integer, 
        'rmcetakanke(25) As Integer, rmisclose(26) As Integer, rminputuser(27) As Integer, rminputtgl(28) As DateTime, rmmodifikasiuser(29) As Integer, 
        'rmmodifikasitgl(30) As DateTime, rmposting(31) As Integer, rmcustomtext1(32) As String, rmcustomtext2(33) As String, rmcustomtext3(34) As String, 
        'rmcustomtext4(35) As String, rmcustomtext5(36) As String, rmcustomint1(37) As Integer, rmcustomint2(38) As Integer, rmcustomint3(39) As Integer, 
        'rmcustomdbl1(40) As Double, rmcustomdbl2(41) As Double, rmcustomdbl3(42) As Double, rmcustomdate1(43) As Date, rmcustomdate2(44) As Date, 
        'rmcustomdate3(45) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'rmid, rmcabang, rmlokasi, rmsumber, rmautonotransaksi, rmnotransaksi, rmtgl, 
        'rmkodepa, rmcarabayar, rmkontak, rmkontakperson, rmnorek, rmuraian, rmcatatan, 
        'rmmatauang, rmkurs, rmjumlah, rmjumlahvalas, rmjumlahbayar, rmjumlahbayarvalas, rmstatusbayar, 
        'rmtgllunas, rmstatus, rmstatussebelumnya, rmjmlrevisi, rmcetakanke, rmisclose, rminputuser, 
        'rminputtgl, rmmodifikasiuser, rmmodifikasitgl, rmposting, rmcustomtext1, rmcustomtext2, rmcustomtext3, 
        'rmcustomtext4, rmcustomtext5, rmcustomint1, rmcustomint2, rmcustomint3, rmcustomdbl1, rmcustomdbl2, 
        'rmcustomdbl3, rmcustomdate1, rmcustomdate2, rmcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 46) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'rmid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "rmid required numeric." : GoTo selesai
        End If
        'rmautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "rmautonotransaksi required numeric." : GoTo selesai
        End If
        'rmtgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "rmtgl required date." : GoTo selesai
        End If
        'rmkodepa(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "rmkodepa required numeric." : GoTo selesai
        End If
        'rmcarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "rmcarabayar required numeric." : GoTo selesai
        End If
        'rmkontak(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "rmkontak required numeric." : GoTo selesai
        End If
        If (dataUtama(9) < 1) Then
            result(2) = "rmkontak can't be empty." : GoTo selesai
        End If
        'rmkurs(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "rmkurs required numeric." : GoTo selesai
        End If
        'rmjumlah(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "rmjumlah required numeric." : GoTo selesai
        End If
        'rmjumlahvalas(17) As Double
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "rmjumlahvalas required numeric." : GoTo selesai
        End If
        'rmjumlahbayar(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "rmjumlahbayar required numeric." : GoTo selesai
        End If
        'rmjumlahbayarvalas(19) As Double
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "rmjumlahbayarvalas required numeric." : GoTo selesai
        End If
        'rmstatusbayar(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "rmstatusbayar required numeric." : GoTo selesai
        End If
        'rmtgllunas(21) As Date
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "rmtgllunas required date." : GoTo selesai
        End If
        'rmstatus(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "rmstatus required numeric." : GoTo selesai
        End If
        'rmstatussebelumnya(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "rmstatussebelumnya required numeric." : GoTo selesai
        End If
        'rmjmlrevisi(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "rmjmlrevisi required numeric." : GoTo selesai
        End If
        'rmcetakanke(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "rmcetakanke required numeric." : GoTo selesai
        End If
        'rmisclose(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "rmisclose required numeric." : GoTo selesai
        End If
        'rminputuser(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "rminputuser required numeric." : GoTo selesai
        End If
        'rminputtgl(28) As DateTime
        If (IsDate(dataUtama(28)) = False) Then
            result(2) = "rminputtgl required date." : GoTo selesai
        End If
        'rmmodifikasiuser(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "rmmodifikasiuser required numeric." : GoTo selesai
        End If
        'rmmodifikasitgl(30) As DateTime
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "rmmodifikasitgl required date." : GoTo selesai
        End If
        'rmposting(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "rmposting required numeric." : GoTo selesai
        End If
        'rmcustomint1(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "rmcustomint1 required numeric." : GoTo selesai
        End If
        'rmcustomint2(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "rmcustomint2 required numeric." : GoTo selesai
        End If
        'rmcustomint3(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "rmcustomint3 required numeric." : GoTo selesai
        End If
        'rmcustomdbl1(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "rmcustomdbl1 required numeric." : GoTo selesai
        End If
        'rmcustomdbl2(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "rmcustomdbl2 required numeric." : GoTo selesai
        End If
        'rmcustomdbl3(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "rmcustomdbl3 required numeric." : GoTo selesai
        End If
        'rmcustomdate1(43) As Date
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "rmcustomdate1 required date." : GoTo selesai
        End If
        'rmcustomdate2(44) As Date
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "rmcustomdate2 required date." : GoTo selesai
        End If
        'rmcustomdate3(45) As Date
        If (IsDate(dataUtama(45)) = False) Then
            result(2) = "rmcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'rmcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "rmcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "rmcabang should not be more than 25 character." : GoTo selesai
        End If

        'rmlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "rmlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "rmlokasi should not be more than 25 character." : GoTo selesai
        End If

        'rmsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "rmsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "rmsumber should not be more than 10 character." : GoTo selesai
        End If

        'rmnotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "rmnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "rmnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'rmtgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "rmtgl can't be empty" : GoTo selesai
        End If

        'rmnorek(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "rmnorek can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 25 Then
            result(2) = "rmnorek should not be more than 25 character." : GoTo selesai
        End If

        'rmmatauang(14) As String
        If Len(dataUtama(14)) = 0 Then
            result(2) = "rmmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(14)) > 25 Then
            result(2) = "rmmatauang should not be more than 25 character." : GoTo selesai
        End If

        'rmkurs(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "rmkurs can't be empty" : GoTo selesai
        End If

        'rmjumlah(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "rmjumlah can't be empty" : GoTo selesai
        End If

        'rmjumlahvalas(17) As Double
        If Len(dataUtama(17)) = 0 Then
            result(2) = "rmjumlahvalas can't be empty" : GoTo selesai
        End If

        'rmjumlahbayarvalas(19) As Double
        If Len(dataUtama(19)) = 0 Then
            result(2) = "rmjumlahbayarvalas can't be empty" : GoTo selesai
        End If

        'rminputtgl(28) As DateTime
        If Len(dataUtama(28)) = 0 Then
            result(2) = "rminputtgl can't be empty" : GoTo selesai
        End If

        'rmmodifikasitgl(30) As DateTime
        If Len(dataUtama(30)) = 0 Then
            result(2) = "rmmodifikasitgl can't be empty" : GoTo selesai
        End If

        'rmcustomdbl1(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "rmcustomdbl1 can't be empty" : GoTo selesai
        End If

        'rmcustomdbl2(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "rmcustomdbl2 can't be empty" : GoTo selesai
        End If

        'rmcustomdbl3(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "rmcustomdbl3 can't be empty" : GoTo selesai
        End If

        'rmcustomdate1(43) As Date
        If Len(dataUtama(43)) = 0 Then
            result(2) = "rmcustomdate1 can't be empty" : GoTo selesai
        End If

        'rmcustomdate2(44) As Date
        If Len(dataUtama(44)) = 0 Then
            result(2) = "rmcustomdate2 can't be empty" : GoTo selesai
        End If

        'rmcustomdate3(45) As Date
        If Len(dataUtama(45)) = 0 Then
            result(2) = "rmcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "rmid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmnorek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmjumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "rmjumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "rmjumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmjumlahbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmstatusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmtgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rminputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rminputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "rmid~rmcabang~rmlokasi~rmsumber~rmautonotransaksi~rmnotransaksi~rmtgl~rmkodepa~rmcarabayar~rmkontak~rmkontakperson~rmnorek~rmuraian~rmcatatan~rmmatauang~rmkurs~rmjumlah~rmjumlahvalas~rmjumlahbayar~rmjumlahbayarvalas~rmstatusbayar~rmtgllunas~rmstatus~rmstatussebelumnya~rmjmlrevisi~rmcetakanke~rmisclose~rminputuser~rminputtgl~rmmodifikasiuser~rmmodifikasitgl~rmposting~rmcustomtext1~rmcustomtext2~rmcustomtext3~rmcustomtext4~rmcustomtext5~rmcustomint1~rmcustomint2~rmcustomint3~rmcustomdbl1~rmcustomdbl2~rmcustomdbl3~rmcustomdate1~rmcustomdate2~rmcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idrmdetail(0) As Integer, idrm(1) As Integer, norek(2) As String, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, catatan(7) As String, costcenter(8) As String, divisi(9) As String, 
        'subdivisi(10) As String, proyek(11) As String, urutan(12) As Integer, isclose(13) As Integer, customtext1(14) As String, 
        'customtext2(15) As String, customtext3(16) As String, customdbl1(17) As Double, customdbl2(18) As Double, customdbl3(19) As Double, 
        'customdate1(20) As Date, customdate2(21) As Date, customdate3(22) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idrmdetail, idrm, norek, matauang, kurs, jumlah, jumlahvalas, 
        'catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idrmdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idrm", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "norek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
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
            If (dataRowDetail.Length <> 23) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idrmdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idrmdetail required numeric." : GoTo selesai
            End If
            'idrm(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idrm required numeric." : GoTo selesai
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
            'urutan(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'isclose(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(20) As Date
            If (IsDate(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(21) As Date
            If (IsDate(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'norek(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - norek can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - norek should not be more than 25 character." : GoTo selesai
            End If

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
            If dataRowDetail(5) = 0 Then
                result(2) = "Row : " & i & " - jumlah can't be zero" : GoTo selesai
            End If

            'jumlahvalas(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - jumlahvalas can't be empty" : GoTo selesai
            End If

            'customdbl1(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(20) As Date
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(21) As Date
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idrmdetail~idrm~norek~matauang~kurs~jumlah~jumlahvalas~catatan~costcenter~divisi~subdivisi~proyek~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            strRekCostCenter = IIf(Len(strRekCostCenter.ToString) = 0, "", strRekCostCenter & " OR ")
            strRekCostCenter = String.Concat(strRekCostCenter, "(cnomor = '" & dataRowDetail(2) & "')")

        Next

        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================

        'MAPPING BUAT WS DATA PAY -------------------------------------------------------
        'idrmcarabayar(0) As Integer, idrm(1) As Integer, carabayar(2) As String, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, nogiro(7) As String, tgljt(8) As Date, bank(9) As String, 
        'noacbank(10) As String, rekbank(11) As String, rekgiro(12) As String, catatan(13) As String, urutan(14) As Integer

        'MAPPING BUAT FLEX DATA PAY -----------------------------------------------------
        'idrmcarabayar, idrm, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan

        'VALIDASI DAN SET DATA PAY ======================================================
        'SPLIT PARAMETER DATA PAY

        'Buat datatable pay
        Dim dtpay As New DataTable
        AsDataTableTambahField(dtpay, "idrmcarabayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "idrm", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpay, "carabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpay, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "jumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtpay, "jumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtpay, "nogiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "tgljt", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "bank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "noacbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "rekbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "rekgiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "urutan", AsEnumTypeData.AsInt64)

        'CEK CARABAYAR, JIKA GIRO MAKA HARUS KIRIM DATA GIRO
        Dim carabayar As Integer = 0
        If (dtutama.Rows.Count > 0) Then
            Dim drutama As DataRow = dtutama.Rows(0)
            carabayar = Val(drutama("rmcarabayar"))
        Else
            result(2) = "#1. Main transaction data not found." : GoTo selesai
        End If

        If (carabayar = 2 And Len(dataSplit(2)) = 0) Then
            result(2) = "Giro data data not found." : GoTo selesai

        ElseIf (carabayar = 2 And Len(dataSplit(2)) <> 0) Then
            dataGiro = dataSplit(2).Split(sptRow)

            'VALIDASI DAN SET DATA ROW GIRO ==================================================
            Dim JmlDtGiro As Integer = dataGiro.Length
            For i = 1 To JmlDtGiro
                'SPLIT DATA GIRO
                dataRowGiro = dataGiro(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA GIRO -----------------------------------
                'CEK ARRAY DATA GIRO
                If (dataRowGiro.Length <> 15) Then
                    result(2) = "Giro Row : " & i & " - Invalid giro transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW GIRO ----------------------------

                'VALIDASI TIPE DATA GIRO ------------------------------------------
                'idrmcarabayar(0) As Integer
                If (IsNumeric(dataRowGiro(0)) = False) Then
                    result(2) = "Giro Row : " & i & " - idrmcarabayar required numeric." : GoTo selesai
                End If
                'idrm(1) As Integer
                If (IsNumeric(dataRowGiro(1)) = False) Then
                    result(2) = "Giro Row : " & i & " - idrm required numeric." : GoTo selesai
                End If
                'carabayar(2) As Integer
                If (IsNumeric(dataRowGiro(2)) = False) Then
                    result(2) = "Giro Row : " & i & " - carabayar required numeric." : GoTo selesai
                End If
                'kurs(4) As Double
                If (IsNumeric(dataRowGiro(4)) = False) Then
                    result(2) = "Giro Row : " & i & " - kurs required numeric." : GoTo selesai
                End If
                'jumlah(5) As Double
                If (IsNumeric(dataRowGiro(5)) = False) Then
                    result(2) = "Giro Row : " & i & " - jumlah required numeric." : GoTo selesai
                End If
                'jumlahvalas(6) As Double
                If (IsNumeric(dataRowGiro(6)) = False) Then
                    result(2) = "Giro Row : " & i & " - jumlahvalas required numeric." : GoTo selesai
                End If
                'tgljt(8) As Date
                If (IsDate(dataRowGiro(8)) = False) Then
                    result(2) = "Giro Row : " & i & " - tgljt required date." : GoTo selesai
                End If
                'urutan(14) As Integer
                If (IsNumeric(dataRowGiro(14)) = False) Then
                    result(2) = "Giro Row : " & i & " - urutan required numeric." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA GIRO -----------------------------------

                'VALIDASI DATA GIRO ---------------------------------------
                'matauang(3) As String
                If Len(dataRowGiro(3)) = 0 Then
                    result(2) = "Giro Row : " & i & " - matauang can't be empty" : GoTo selesai
                End If
                If Len(dataRowGiro(3)) > 25 Then
                    result(2) = "Giro Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
                End If

                'kurs(4) As Double
                If Len(dataRowGiro(4)) = 0 Then
                    result(2) = "Giro Row : " & i & " - kurs can't be empty" : GoTo selesai
                End If

                'jumlah(5) As Double
                If Len(dataRowGiro(5)) = 0 Then
                    result(2) = "Giro Row : " & i & " - jumlah can't be empty" : GoTo selesai
                End If
                If dataRowGiro(5) <= 0 Then
                    result(2) = "Giro Row : " & i & " - jumlah must be more than zero" : GoTo selesai
                End If

                'jumlahvalas(6) As Double
                If Len(dataRowGiro(6)) = 0 Then
                    result(2) = "Giro Row : " & i & " - jumlahvalas can't be empty" : GoTo selesai
                End If

                'nogiro(7) As String
                If Len(dataRowGiro(7)) = 0 Then
                    result(2) = "Giro Row : " & i & " - nogiro can't be empty" : GoTo selesai
                End If
                If Len(dataRowGiro(7)) > 25 Then
                    result(2) = "Giro Row : " & i & " - nogiro should not be more than 25 character." : GoTo selesai
                End If

                'tgljt(8) As Date
                If Len(dataRowGiro(8)) = 0 Then
                    result(2) = "Giro Row : " & i & " - tgljt can't be empty" : GoTo selesai
                End If

                'bank(9) As String
                If Len(dataRowGiro(9)) = 0 Then
                    result(2) = "Giro Row : " & i & " - bank can't be empty" : GoTo selesai
                End If
                If Len(dataRowGiro(9)) > 25 Then
                    result(2) = "Giro Row : " & i & " - bank should not be more than 25 character." : GoTo selesai
                End If

                'noacbank(10) As String
                If Len(dataRowGiro(10)) = 0 Then
                    result(2) = "Giro Row : " & i & " - noacbank can't be empty" : GoTo selesai
                End If
                If Len(dataRowGiro(10)) > 50 Then
                    result(2) = "Giro Row : " & i & " - noacbank should not be more than 50 character." : GoTo selesai
                End If

                'rekbank(11) As String
                If Len(dataRowGiro(11)) = 0 Then
                    result(2) = "Giro Row : " & i & " - rekbank can't be empty" : GoTo selesai
                End If
                If Len(dataRowGiro(11)) > 25 Then
                    result(2) = "Giro Row : " & i & " - rekbank should not be more than 25 character." : GoTo selesai
                End If

                'rekgiro(12) As String
                If Len(dataRowGiro(12)) = 0 Then
                    result(2) = "Giro Row : " & i & " - rekgiro can't be empty" : GoTo selesai
                End If
                If Len(dataRowGiro(12)) > 25 Then
                    result(2) = "Giro Row : " & i & " - rekgiro should not be more than 25 character." : GoTo selesai
                End If
                'END OF VALIDASI DATA GIRO --------------------------------

                If AsDataTableTambahData(dtpay, "idrmcarabayar~idrm~carabayar~matauang~kurs~jumlah~jumlahvalas~nogiro~tgljt~bank~noacbank~rekbank~rekgiro~catatan~urutan", dataRowGiro(0) & "~" & dataRowGiro(1) & "~" & dataRowGiro(2) & "~" & dataRowGiro(3) & "~" & dataRowGiro(4) & "~" & dataRowGiro(5) & "~" & dataRowGiro(6) & "~" & dataRowGiro(7) & "~" & dataRowGiro(8) & "~" & dataRowGiro(9) & "~" & dataRowGiro(10) & "~" & dataRowGiro(11) & "~" & dataRowGiro(12) & "~" & dataRowGiro(13) & "~" & dataRowGiro(14)) = False Then
                    result(2) = "Giro Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next

            'END OF VALIDASI DAN SET ROW DATA GIRO ===========================================

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
                Dim vModuleId As Integer = 2, vMenuId As Integer = 5
                Select Case drutama("rmstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("rmtgl")), AsFormatTanggal(drutama("rmtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "rmmatauang", "rmnorek", dtdetail, "norek")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                'CEK COA WAJIB COST CENTER ==============================
                If drutama("rmstatus") = 2 Then
                    Dim cekCoaCostCenter As String = ValidasiCoaRequiredCostCenter(strRekCostCenter, dtdetail)
                    If Len(cekCoaCostCenter) > 0 Then
                        result(2) = cekCoaCostCenter : Trans.Rollback() : GoTo selesai
                    End If
                End If
                'END OF CEK COA WAJIB COST CENTER =======================


                'VALIDASI NOMINAL HARUS SEIMBANG ========================
                Dim jml As Double = 0, jmlvalas As Double = 0, jmlgiro As Double = 0, jmlgirovalas As Double = 0
                jml = AsDataTableDSum(dtdetail, "jumlah")
                jmlvalas = AsDataTableDSum(dtdetail, "jumlahvalas")
                jmlgiro = AsDataTableDSum(dtpay, "jumlah")
                jmlgirovalas = AsDataTableDSum(dtpay, "jumlahvalas")

                ''AMBIL SETTING FORMAT NOMINAL
                'Dim digitGroup As String = "", pemisahDesimal As String = "", digitDesimal As Integer = 0
                'Dim FNominal As String = GetSettingNominal(digitGroup, pemisahDesimal, digitDesimal)
                'If len(FNominal) <> 0 Then result(2) = FNominal : Trans.Rollback() : GoTo selesai

                ''BULATKAN NOMINAL DETAIL SESUAI SETTING FORMAT NOMINAL
                'jml = math.round(jml, digitDesimal)
                'jmlvalas = math.round(jmlvalas, digitDesimal)
                'jmlgiro = math.round(jmlgiro, digitDesimal)
                'jmlgirovalas = math.round(jmlgirovalas, digitDesimal)

                'JML DETAIL DAN GIRO HARUS SEIMBANG
                If carabayar = 2 Then
                    If jml <> jmlgiro Then
                        result(2) = "Total amount of detail and giro are not balanced." : GoTo selesai
                    End If
                    If jmlvalas <> jmlgirovalas Then
                        result(2) = "Total amount of foreign detail and giro are not balanced." : GoTo selesai
                    End If
                End If

                'HITUNG TOTAL TOTAL BERDASARKAN DATA DETAIL
                drutama("rmjumlah") = jml
                drutama("rmjumlahvalas") = jmlvalas
                'END OF VALIDASI NOMINAL HARUS SEIMBANG =================


                If isUpdate Then
                    result(4) = drutama("rmid")
                    notransaksi = drutama("rmnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(rmid), rmnotransaksi FROM M2_Rm WHERE rmid='" & result(4) & "' AND rmstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("rmautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rmcabang"), drutama("rmlokasi"), drutama("rmsumber"), drutama("rmtgl"), drutama("rmsumber"), 2)
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(rmid) FROM m2_rm WHERE rmnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m2_rm_history
                        Dim rsSimpanHistory As String = SimpanHistory.M2_Rm_HistorySimpan("" & paramSplit(0) & "★M2_Rm_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("rmsumber")) & "▼" & FixQuotes(drutama("rmid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M2_Rm set rmcabang  = '" & FixQuotes(drutama("rmcabang")) & "', rmlokasi  = '" & FixQuotes(drutama("rmlokasi")) & "', rmsumber  = '" & FixQuotes(drutama("rmsumber")) & "', rmautonotransaksi  = " & drutama("rmautonotransaksi") & ", rmnotransaksi  = '" & notransaksi & "', rmtgl  = '" & FixQuotes(AsFormatTanggal(drutama("rmtgl"))) & "', rmkodepa  = " & drutama("rmkodepa") & ", rmcarabayar  = " & drutama("rmcarabayar") & ", rmkontak  = " & drutama("rmkontak") & ", rmkontakperson  = '" & FixQuotes(drutama("rmkontakperson")) & "', rmnorek  = '" & FixQuotes(drutama("rmnorek")) & "', rmuraian  = '" & FixQuotes(drutama("rmuraian")) & "', rmcatatan  = '" & FixQuotes(drutama("rmcatatan")) & "', rmmatauang  = '" & FixQuotes(drutama("rmmatauang")) & "', rmkurs  = '" & FixDouble(drutama("rmkurs")) & "', rmjumlah  = '" & FixDouble(drutama("rmjumlah")) & "', rmjumlahvalas  = '" & FixDouble(drutama("rmjumlahvalas")) & "', rmjumlahbayar  = '" & FixDouble(drutama("rmjumlahbayar")) & "', rmjumlahbayarvalas  = '" & FixDouble(drutama("rmjumlahbayarvalas")) & "', rmstatusbayar  = " & drutama("rmstatusbayar") & ", rmtgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("rmtgllunas"))) & "', rmstatus  = " & drutama("rmstatus") & ", rmstatussebelumnya  = " & drutama("rmstatussebelumnya") & ", rmjmlrevisi  = rmjmlrevisi+1, rmcetakanke  = " & drutama("rmcetakanke") & ", rmisclose  = " & drutama("rmisclose") & ", rmmodifikasiuser  = " & drutama("rmmodifikasiuser") & ", rmmodifikasitgl  = NOW(), rmposting  = 0, rmcustomtext1  = '" & FixQuotes(drutama("rmcustomtext1")) & "', rmcustomtext2  = '" & FixQuotes(drutama("rmcustomtext2")) & "', rmcustomtext3  = '" & FixQuotes(drutama("rmcustomtext3")) & "', rmcustomtext4  = '" & FixQuotes(drutama("rmcustomtext4")) & "', rmcustomtext5  = '" & FixQuotes(drutama("rmcustomtext5")) & "', rmcustomint1  = " & drutama("rmcustomint1") & ", rmcustomint2  = " & drutama("rmcustomint2") & ", rmcustomint3  = " & drutama("rmcustomint3") & ", rmcustomdbl1  = '" & FixDouble(drutama("rmcustomdbl1")) & "', rmcustomdbl2  = '" & FixDouble(drutama("rmcustomdbl2")) & "', rmcustomdbl3  = '" & FixDouble(drutama("rmcustomdbl3")) & "', rmcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("rmcustomdate1"))) & "', rmcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("rmcustomdate2"))) & "', rmcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("rmcustomdate3"))) & "' where rmid = '" & drutama("rmid") & "'"
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

                    If drutama("rmautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rmcabang"), drutama("rmlokasi"), drutama("rmsumber"), drutama("rmtgl"), drutama("rmsumber"), 2)
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
                        notransaksi = drutama("rmnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(rmid) FROM m2_rm WHERE rmnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M2_Rm (rmcabang, rmlokasi, rmsumber, rmautonotransaksi, rmnotransaksi, rmtgl, rmkodepa, rmcarabayar, rmkontak, rmkontakperson, rmnorek, rmuraian, rmcatatan, rmmatauang, rmkurs, rmjumlah, rmjumlahvalas, rmjumlahbayar, rmjumlahbayarvalas, rmstatusbayar, rmtgllunas, rmstatus, rmstatussebelumnya, rmjmlrevisi, rmcetakanke, rmisclose, rminputuser, rminputtgl, rmmodifikasiuser, rmmodifikasitgl, rmposting, rmcustomtext1, rmcustomtext2, rmcustomtext3, rmcustomtext4, rmcustomtext5, rmcustomint1, rmcustomint2, rmcustomint3, rmcustomdbl1, rmcustomdbl2, rmcustomdbl3, rmcustomdate1, rmcustomdate2, rmcustomdate3) values('" & FixQuotes(drutama("rmcabang")) & "', '" & FixQuotes(drutama("rmlokasi")) & "', '" & FixQuotes(drutama("rmsumber")) & "', " & drutama("rmautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("rmtgl"))) & "', " & drutama("rmkodepa") & ", " & drutama("rmcarabayar") & ", " & drutama("rmkontak") & ", '" & FixQuotes(drutama("rmkontakperson")) & "', '" & FixQuotes(drutama("rmnorek")) & "', '" & FixQuotes(drutama("rmuraian")) & "', '" & FixQuotes(drutama("rmcatatan")) & "', '" & FixQuotes(drutama("rmmatauang")) & "', '" & FixDouble(drutama("rmkurs")) & "', '" & FixDouble(drutama("rmjumlah")) & "', '" & FixDouble(drutama("rmjumlahvalas")) & "', '" & FixDouble(drutama("rmjumlahbayar")) & "', '" & FixDouble(drutama("rmjumlahbayarvalas")) & "', " & drutama("rmstatusbayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("rmtgllunas"))) & "', " & drutama("rmstatus") & ", " & drutama("rmstatussebelumnya") & ", " & drutama("rmjmlrevisi") & ", " & drutama("rmcetakanke") & ", " & drutama("rmisclose") & ", " & drutama("rminputuser") & ", NOW(), " & drutama("rmmodifikasiuser") & ", '1971-01-01 00:00:00', 0, '" & FixQuotes(drutama("rmcustomtext1")) & "', '" & FixQuotes(drutama("rmcustomtext2")) & "', '" & FixQuotes(drutama("rmcustomtext3")) & "', '" & FixQuotes(drutama("rmcustomtext4")) & "', '" & FixQuotes(drutama("rmcustomtext5")) & "', " & drutama("rmcustomint1") & ", " & drutama("rmcustomint2") & ", " & drutama("rmcustomint3") & ", '" & FixDouble(drutama("rmcustomdbl1")) & "', '" & FixDouble(drutama("rmcustomdbl2")) & "', '" & FixDouble(drutama("rmcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rmcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rmcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rmcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select rmid from M2_Rm where rmnotransaksi='" & notransaksi & "' AND Rminputuser= '" & userid & "' order by Rmmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M2_Rm_Detail where idrm = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idrmdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("norek")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M2_Rm_Detail(idrmdetail, idrm, norek, matauang, kurs, jumlah, jumlahvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                'Hapus detail pay ketika update
                If (isUpdate) Then
                    sql = "Delete from M2_Rm_Pay where idrm = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses detail pay
                If (dtpay.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder, strGiro As New StringBuilder
                    For Each dr1 As DataRow In dtpay.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idrmcarabayar") & ", " & result(4) & ", " & dr1("carabayar") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ")")
                        'query untuk insert giro
                        If drutama("rmstatus") = 2 Then
                            strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", ", "))
                            strGiro.Append("('" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(drutama("rmsumber")) & "', " & result(4) & ", '" & FixQuotes(notransaksi) & "', " & drutama("rmkontak") & ", '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', " & 0 & ", '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', " & 0 & ", " & 0 & ", " & dr1("urutan") & ")")
                        End If
                    Next
                    sql = "Insert into M2_Rm_Pay(idrmcarabayar, idrm, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'insert giro jika status approved
                    If drutama("rmstatus") = 2 And Len(strGiro.ToString) > 0 Then
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
                End If

                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "RM", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("rmstatus") = 2 Then
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
    Public Function M2_RmUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("rmkontakkode", "c1.kkode")
            Filter = Filter.Replace("rmkontaknama", "c1.knama")
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
            Dim sumber As String = "Rm", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Rmtgl, Rmnotransaksi, Rmstatus FROM m2_Rm WHERE Rmid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Rmstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m2_rm_history
            Dim rsSimpanHistory As String = SimpanHistory.M2_Rm_HistorySimpan("" & paramSplit(0) & "★M2_Rm_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m2_rm_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                'CEK STATUS GIRO
                dtdetail = AsDataTableAmbilDariDBCon("SELECT glnogiro FROM m2_giro_list WHERE glsumber = 'RM' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "' AND glstatus <> 0", myConn)
                If dtdetail.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai

                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'RM' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'DELETE GIRO
                sql = "DELETE FROM m2_giro_list WHERE glsumber = 'RM' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M2_Rm SET Rmstatus = " & nilaiStatus & ", Rmmodifikasiuser='" & userid & "', Rmmodifikasitgl = NOW(), Rmposting = 0, Rmpostingtgl = '1971-01-01 00:00:00', Rmjmlrevisi = Rmjmlrevisi + 1 WHERE Rmid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_RmSearch(PostWsSearch(paramSplit(0), "M2_RmSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_RmDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("rmkontakkode", "c1.kkode")
            Filter = Filter.Replace("rmkontaknama", "c1.knama")
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
            Dim sumber As String = "Rm", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Rmid, Rmnotransaksi FROM m2_Rm WHERE Rmid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT rmcabang, rmlokasi, rmsumber, rmautonotransaksi, rmnotransaksi, rmtgl"
            sql &= " FROM M2_rm"
            sql &= " WHERE rmid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("rmcabang")
                lokasi = dtNomorNext.Rows(0)("rmlokasi")
                sumber = dtNomorNext.Rows(0)("rmsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("rmautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("rmnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("rmtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE JURNAL
            sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'RM' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE GIRO
            sql = "DELETE FROM m2_giro_list WHERE glsumber = 'RM' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE PAY
            sql = "DELETE FROM M2_Rm_Pay WHERE idRm = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M2_Rm_Detail WHERE idRm = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M2_Rm WHERE Rmid = '" & idtransaksi & "'"
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
                Dim rsNomorNext As String = M0_DeleteNotransaksi(cabang, lokasi, sumber, tgl, notransaksi, sumber, 2)
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
            Dim paramSearch As String = M2_RmSearch(PostWsSearch(paramSplit(0), "M2_RmSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_RmGetdataById(ByVal param As String) As String

        'M2_RmGetdataById Utama --------------------------------------------------------
        'rmid, rmcabang, rmlokasi, rmsumber, rmautonotransaksi, rmnotransaksi, rmtgl, 
        'rmkodepa, rmcarabayar, rmkontak, rmkontakperson, rmnorek, rmuraian, rmcatatan, 
        'rmmatauang, rmkurs, rmjumlah, rmjumlahvalas, rmjumlahbayar, rmjumlahbayarvalas, rmstatusbayar, 
        'rmtgllunas, rmstatus, rmstatussebelumnya, rmjmlrevisi, rmcetakanke, rmisclose, rminputuser, 
        'rminputtgl, rmmodifikasiuser, rmmodifikasitgl, rmposting, rmpostingtgl, rmcustomtext1, rmcustomtext2, 
        'rmcustomtext3, rmcustomtext4, rmcustomtext5, rmcustomint1, rmcustomint2, rmcustomint3, rmcustomdbl1, 
        'rmcustomdbl2, rmcustomdbl3, rmcustomdate1, rmcustomdate2, rmcustomdate3, rmcabangnama, rmlokasinama, 
        'rmcarabayarnama, rmkontakkode, rmkontaknama, rmnoreknama, rmstatusnama, rmstatussebelumnyanama, rminputusernama, 
        'rmmodifikasiusernama

        'M2_RmGetdataById Detail -------------------------------------------------------
        'idrmdetail, idrm, norek, matauang, kurs, jumlah, jumlahvalas, 
        'catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3, noreknama, costcenternama, divisinama, subdivisinama, proyeknama

        'M2_RmGetdataById Pay -------------------------------------------------------
        'idrmcarabayar, idrm, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, carabayarnama, banknama, rekbanknama, rekgironama

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
        Dim Filter As String = "", Sorting As String = "", notransaksi As String = ""
        Dim dt As New DataTable

        Dim utama As String = "", detail As String = "", giro As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M2_Rm~M2_Rm_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "rmid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "rmid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_rm_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            notransaksi = FxDB(drutama("rmnotransaksi"), "")
            utama = String.Concat(FxDB(drutama("rmid"), 0), sptField,
                     FxDB(drutama("rmcabang"), ""), sptField,
                     FxDB(drutama("rmlokasi"), ""), sptField,
                     FxDB(drutama("rmsumber"), ""), sptField,
                     FxDB(drutama("rmautonotransaksi"), 0), sptField,
                     FxDB(drutama("rmnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rmtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("rmkodepa"), 0), sptField,
                     FxDB(drutama("rmcarabayar"), 0), sptField,
                     FxDB(drutama("rmkontak"), 0), sptField,
                     FxDB(drutama("rmkontakperson"), ""), sptField,
                     FxDB(drutama("rmnorek"), ""), sptField,
                     FxDB(drutama("rmuraian"), ""), sptField,
                     FxDB(drutama("rmcatatan"), ""), sptField,
                     FxDB(drutama("rmmatauang"), ""), sptField,
                     FxDB(drutama("rmkurs"), 0), sptField,
                     FxDB(drutama("rmjumlah"), 0), sptField,
                     FxDB(drutama("rmjumlahvalas"), 0), sptField,
                     FxDB(drutama("rmjumlahbayar"), 0), sptField,
                     FxDB(drutama("rmjumlahbayarvalas"), 0), sptField,
                     FxDB(drutama("rmstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rmtgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("rmstatus"), 0), sptField,
                     FxDB(drutama("rmstatussebelumnya"), 0), sptField,
                     FxDB(drutama("rmjmlrevisi"), 0), sptField,
                     FxDB(drutama("rmcetakanke"), 0), sptField,
                     FxDB(drutama("rmisclose"), 0), sptField,
                     FxDB(drutama("rminputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rminputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rmmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rmmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rmposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rmpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rmcustomtext1"), ""), sptField,
                     FxDB(drutama("rmcustomtext2"), ""), sptField,
                     FxDB(drutama("rmcustomtext3"), ""), sptField,
                     FxDB(drutama("rmcustomtext4"), ""), sptField,
                     FxDB(drutama("rmcustomtext5"), ""), sptField,
                     FxDB(drutama("rmcustomint1"), 0), sptField,
                     FxDB(drutama("rmcustomint2"), 0), sptField,
                     FxDB(drutama("rmcustomint3"), 0), sptField,
                     FxDB(drutama("rmcustomdbl1"), 0), sptField,
                     FxDB(drutama("rmcustomdbl2"), 0), sptField,
                     FxDB(drutama("rmcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rmcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rmcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rmcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("rmcabangnama"), ""), sptField,
                     FxDB(drutama("rmlokasinama"), ""), sptField,
                     FxDB(drutama("rmcarabayarnama"), ""), sptField,
                     FxDB(drutama("rmkontakkode"), ""), sptField,
                     FxDB(drutama("rmkontaknama"), ""), sptField,
                     FxDB(drutama("rmnoreknama"), ""), sptField,
                     FxDB(drutama("rmstatusnama"), ""), sptField,
                     FxDB(drutama("rmstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("rminputusernama"), ""), sptField,
                     FxDB(drutama("rmmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idrmdetail"), 0), sptField,
                     FxDB(dr("idrm"), 0), sptField,
                     FxDB(dr("norek"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("jumlah"), 0), sptField,
                     FxDB(dr("jumlahvalas"), 0), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
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
                     FxDB(dr("noreknama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA PAY
            'PANGGIL QUERY
            Dim querygiro As New m0_query
            sql = querygiro.PanggilQuery("m2_rm_pay_v")

            Dim dtgiro As New DataTable
            dtgiro = AmbilData("aplikasi1-M2_Giro_List", "rmp.idrm='" & idtransaksi & "'", "rmp.urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtgiro.Rows
                giro = String.Concat(giro,
                     FxDB(dr("idrmcarabayar"), 0), sptField,
                     FxDB(dr("idrm"), 0), sptField,
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
                     FxDB(dr("carabayarnama"), ""), sptField,
                     FxDB(dr("banknama"), ""), sptField,
                     FxDB(dr("rekbanknama"), ""), sptField,
                     FxDB(dr("rekgironama"), ""), sptRow)
            Next
            If giro.Length > 0 Then giro = giro.Substring(0, giro.Length - sptRow.Length) Else giro = giro

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, giro)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rmid, rmcabang, rmlokasi, rmsumber, rmautonotransaksi, rmnotransaksi, rmtgl, rmkodepa, rmcarabayar, rmkontak, rmkontakperson, rmnorek, rmuraian, rmcatatan, rmmatauang, rmkurs, rmjumlah, rmjumlahvalas, rmjumlahbayar, rmjumlahbayarvalas, rmstatusbayar, rmtgllunas, rmstatus, rmstatussebelumnya, rmjmlrevisi, rmcetakanke, rmisclose, rminputuser, rminputtgl, rmmodifikasiuser, rmmodifikasitgl, rmposting, rmpostingtgl, rmcustomtext1, rmcustomtext2, rmcustomtext3, rmcustomtext4, rmcustomtext5, rmcustomint1, rmcustomint2, rmcustomint3, rmcustomdbl1, rmcustomdbl2, rmcustomdbl3, rmcustomdate1, rmcustomdate2, rmcustomdate3, rmcabangnama, rmlokasinama, rmcarabayarnama, rmkontakkode, rmkontaknama, rmnoreknama, rmstatusnama, rmstatussebelumnyanama, rminputusernama, rmmodifikasiusernama" & sptSubParam & "idrmdetail, idrm, norek, matauang, kurs, jumlah, jumlahvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, noreknama, costcenternama, divisinama, subdivisinama, proyeknama" & sptSubParam & "idrmcarabayar, idrm, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, carabayarnama, banknama, rekbanknama, rekgironama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_RmSearch(ByVal param As String) As String
        'M2_RmSearch --------------------------------------------------------
        'rmid, rmcabang, rmlokasi, rmsumber, rmautonotransaksi, rmnotransaksi, rmtgl, 
        'rmkodepa, rmcarabayar, rmkontak, rmkontakperson, rmnorek, rmuraian, rmcatatan, 
        'rmmatauang, rmkurs, rmjumlah, rmjumlahvalas, rmjumlahbayar, rmjumlahbayarvalas, rmstatusbayar, 
        'rmtgllunas, rmstatus, rmstatussebelumnya, rmjmlrevisi, rmcetakanke, rmisclose, rminputuser, 
        'rminputtgl, rmmodifikasiuser, rmmodifikasitgl, rmposting, rmpostingtgl, rmcabangnama, rmlokasinama, 
        'rmcarabayarnama, rmkontakkode, rmkontaknama, rmnoreknama, rmstatusnama, rmstatussebelumnyanama, rminputusernama, 
        'rmmodifikasiusernama

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
            Filter = Filter.Replace("rmkontakkode", "c1.kkode")
            Filter = Filter.Replace("rmkontaknama", "c1.knama")
            Filter = Filter.Replace("Rmkontaknama", "`c1`.`knama`")
            Filter = Filter.Replace("Rmstatusnama", "`st1`.`nama`")
            Filter = Filter.Replace("Rmcarabayarnama", "`pm`.`nama`")
            Filter = Filter.Replace("Rminputusernama", "`u1`.`unama`")
            Filter = Filter.Replace("Rmmodifikasiusernama", "`u2`.`unama`")
            Filter = Filter.Replace("Rmcabangnama", "`br`.`bnama`")
            Filter = Filter.Replace("Rmlokasinama", "`lc`.`lnama`")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_rm_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Rm", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rmid"), 0), sptField,
                     FxDB(dr("rmcabang"), ""), sptField,
                     FxDB(dr("rmlokasi"), ""), sptField,
                     FxDB(dr("rmsumber"), ""), sptField,
                     FxDB(dr("rmautonotransaksi"), 0), sptField,
                     FxDB(dr("rmnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rmtgl"), ""), formatTgl), sptField,
                     FxDB(dr("rmkodepa"), 0), sptField,
                     FxDB(dr("rmcarabayar"), 0), sptField,
                     FxDB(dr("rmkontak"), 0), sptField,
                     FxDB(dr("rmkontakperson"), ""), sptField,
                     FxDB(dr("rmnorek"), ""), sptField,
                     FxDB(dr("rmuraian"), ""), sptField,
                     FxDB(dr("rmcatatan"), ""), sptField,
                     FxDB(dr("rmmatauang"), ""), sptField,
                     FxDB(dr("rmkurs"), 0), sptField,
                     FxDB(dr("rmjumlah"), 0), sptField,
                     FxDB(dr("rmjumlahvalas"), 0), sptField,
                     FxDB(dr("rmjumlahbayar"), 0), sptField,
                     FxDB(dr("rmjumlahbayarvalas"), 0), sptField,
                     FxDB(dr("rmstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rmtgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("rmstatus"), 0), sptField,
                     FxDB(dr("rmstatussebelumnya"), 0), sptField,
                     FxDB(dr("rmjmlrevisi"), 0), sptField,
                     FxDB(dr("rmcetakanke"), 0), sptField,
                     FxDB(dr("rmisclose"), 0), sptField,
                     FxDB(dr("rminputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rminputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rmmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rmmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rmposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rmpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rmcabangnama"), ""), sptField,
                     FxDB(dr("rmlokasinama"), ""), sptField,
                     FxDB(dr("rmcarabayarnama"), ""), sptField,
                     FxDB(dr("rmkontakkode"), ""), sptField,
                     FxDB(dr("rmkontaknama"), ""), sptField,
                     FxDB(dr("rmnoreknama"), ""), sptField,
                     FxDB(dr("rmstatusnama"), ""), sptField,
                     FxDB(dr("rmstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("rminputusernama"), ""), sptField,
                     FxDB(dr("rmmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rmid, rmcabang, rmlokasi, rmsumber, rmautonotransaksi, rmnotransaksi, rmtgl, rmkodepa, rmcarabayar, rmkontak, rmkontakperson, rmnorek, rmuraian, rmcatatan, rmmatauang, rmkurs, rmjumlah, rmjumlahvalas, rmjumlahbayar, rmjumlahbayarvalas, rmstatusbayar, rmtgllunas, rmstatus, rmstatussebelumnya, rmjmlrevisi, rmcetakanke, rmisclose, rminputuser, rminputtgl, rmmodifikasiuser, rmmodifikasitgl, rmposting, rmpostingtgl, rmcabangnama, rmlokasinama, rmcarabayarnama, rmkontakkode, rmkontaknama, rmnoreknama, rmstatusnama, rmstatussebelumnyanama, rminputusernama, rmmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_RmTerkait(ByVal param As String) As String
        'M2_RmTerkait --------------------------------------------------------
        'rmid, rmnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "rmid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_rm_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rmid"), 0), sptField,
                     FxDB(dr("rmnotransaksi"), ""), sptField,
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
            result(2) = "Related RM data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rmid, rmnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_RmSimpanOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataGiro(), dataRowGiro() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean
        Dim strRekCostCenter As String = ""

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
        'rmid(0) As Integer, rmcabang(1) As String, rmlokasi(2) As String, rmsumber(3) As String, rmautonotransaksi(4) As Integer, 
        'rmnotransaksi(5) As String, rmtgl(6) As Date, rmkodepa(7) As Integer, rmcarabayar(8) As Integer, rmkontak(9) As Integer, 
        'rmkontakperson(10) As String, rmnorek(11) As String, rmuraian(12) As String, rmcatatan(13) As String, rmmatauang(14) As String, 
        'rmkurs(15) As Double, rmjumlah(16) As Double, rmjumlahvalas(17) As Double, rmjumlahbayar(18) As Double, rmjumlahbayarvalas(19) As Double, 
        'rmstatusbayar(20) As Integer, rmtgllunas(21) As Date, rmstatus(22) As Integer, rmstatussebelumnya(23) As Integer, rmjmlrevisi(24) As Integer, 
        'rmcetakanke(25) As Integer, rmisclose(26) As Integer, rminputuser(27) As Integer, rminputtgl(28) As DateTime, rmmodifikasiuser(29) As Integer, 
        'rmmodifikasitgl(30) As DateTime, rmposting(31) As Integer, rmcustomtext1(32) As String, rmcustomtext2(33) As String, rmcustomtext3(34) As String, 
        'rmcustomtext4(35) As String, rmcustomtext5(36) As String, rmcustomint1(37) As Integer, rmcustomint2(38) As Integer, rmcustomint3(39) As Integer, 
        'rmcustomdbl1(40) As Double, rmcustomdbl2(41) As Double, rmcustomdbl3(42) As Double, rmcustomdate1(43) As Date, rmcustomdate2(44) As Date, 
        'rmcustomdate3(45) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'rmid, rmcabang, rmlokasi, rmsumber, rmautonotransaksi, rmnotransaksi, rmtgl, 
        'rmkodepa, rmcarabayar, rmkontak, rmkontakperson, rmnorek, rmuraian, rmcatatan, 
        'rmmatauang, rmkurs, rmjumlah, rmjumlahvalas, rmjumlahbayar, rmjumlahbayarvalas, rmstatusbayar, 
        'rmtgllunas, rmstatus, rmstatussebelumnya, rmjmlrevisi, rmcetakanke, rmisclose, rminputuser, 
        'rminputtgl, rmmodifikasiuser, rmmodifikasitgl, rmposting, rmcustomtext1, rmcustomtext2, rmcustomtext3, 
        'rmcustomtext4, rmcustomtext5, rmcustomint1, rmcustomint2, rmcustomint3, rmcustomdbl1, rmcustomdbl2, 
        'rmcustomdbl3, rmcustomdate1, rmcustomdate2, rmcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 46) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'rmid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "rmid required numeric." : GoTo selesai
        End If
        'rmautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "rmautonotransaksi required numeric." : GoTo selesai
        End If
        'rmtgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "rmtgl required date." : GoTo selesai
        End If
        'rmkodepa(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "rmkodepa required numeric." : GoTo selesai
        End If
        'rmcarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "rmcarabayar required numeric." : GoTo selesai
        End If
        'rmkontak(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "rmkontak required numeric." : GoTo selesai
        End If
        If (dataUtama(9) < 1) Then
            result(2) = "rmkontak can't be empty." : GoTo selesai
        End If
        'rmkurs(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "rmkurs required numeric." : GoTo selesai
        End If
        'rmjumlah(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "rmjumlah required numeric." : GoTo selesai
        End If
        'rmjumlahvalas(17) As Double
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "rmjumlahvalas required numeric." : GoTo selesai
        End If
        'rmjumlahbayar(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "rmjumlahbayar required numeric." : GoTo selesai
        End If
        'rmjumlahbayarvalas(19) As Double
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "rmjumlahbayarvalas required numeric." : GoTo selesai
        End If
        'rmstatusbayar(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "rmstatusbayar required numeric." : GoTo selesai
        End If
        'rmtgllunas(21) As Date
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "rmtgllunas required date." : GoTo selesai
        End If
        'rmstatus(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "rmstatus required numeric." : GoTo selesai
        End If
        'rmstatussebelumnya(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "rmstatussebelumnya required numeric." : GoTo selesai
        End If
        'rmjmlrevisi(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "rmjmlrevisi required numeric." : GoTo selesai
        End If
        'rmcetakanke(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "rmcetakanke required numeric." : GoTo selesai
        End If
        'rmisclose(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "rmisclose required numeric." : GoTo selesai
        End If
        'rminputuser(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "rminputuser required numeric." : GoTo selesai
        End If
        'rminputtgl(28) As DateTime
        If (IsDate(dataUtama(28)) = False) Then
            result(2) = "rminputtgl required date." : GoTo selesai
        End If
        'rmmodifikasiuser(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "rmmodifikasiuser required numeric." : GoTo selesai
        End If
        'rmmodifikasitgl(30) As DateTime
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "rmmodifikasitgl required date." : GoTo selesai
        End If
        'rmposting(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "rmposting required numeric." : GoTo selesai
        End If
        'rmcustomint1(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "rmcustomint1 required numeric." : GoTo selesai
        End If
        'rmcustomint2(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "rmcustomint2 required numeric." : GoTo selesai
        End If
        'rmcustomint3(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "rmcustomint3 required numeric." : GoTo selesai
        End If
        'rmcustomdbl1(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "rmcustomdbl1 required numeric." : GoTo selesai
        End If
        'rmcustomdbl2(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "rmcustomdbl2 required numeric." : GoTo selesai
        End If
        'rmcustomdbl3(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "rmcustomdbl3 required numeric." : GoTo selesai
        End If
        'rmcustomdate1(43) As Date
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "rmcustomdate1 required date." : GoTo selesai
        End If
        'rmcustomdate2(44) As Date
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "rmcustomdate2 required date." : GoTo selesai
        End If
        'rmcustomdate3(45) As Date
        If (IsDate(dataUtama(45)) = False) Then
            result(2) = "rmcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'rmcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "rmcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "rmcabang should not be more than 25 character." : GoTo selesai
        End If

        'rmlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "rmlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "rmlokasi should not be more than 25 character." : GoTo selesai
        End If

        'rmsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "rmsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "rmsumber should not be more than 10 character." : GoTo selesai
        End If

        'rmnotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "rmnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "rmnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'rmtgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "rmtgl can't be empty" : GoTo selesai
        End If

        'rmnorek(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "rmnorek can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 25 Then
            result(2) = "rmnorek should not be more than 25 character." : GoTo selesai
        End If

        'rmmatauang(14) As String
        If Len(dataUtama(14)) = 0 Then
            result(2) = "rmmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(14)) > 25 Then
            result(2) = "rmmatauang should not be more than 25 character." : GoTo selesai
        End If

        'rmkurs(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "rmkurs can't be empty" : GoTo selesai
        End If

        'rmjumlah(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "rmjumlah can't be empty" : GoTo selesai
        End If

        'rmjumlahvalas(17) As Double
        If Len(dataUtama(17)) = 0 Then
            result(2) = "rmjumlahvalas can't be empty" : GoTo selesai
        End If

        'rmjumlahbayarvalas(19) As Double
        If Len(dataUtama(19)) = 0 Then
            result(2) = "rmjumlahbayarvalas can't be empty" : GoTo selesai
        End If

        'rminputtgl(28) As DateTime
        If Len(dataUtama(28)) = 0 Then
            result(2) = "rminputtgl can't be empty" : GoTo selesai
        End If

        'rmmodifikasitgl(30) As DateTime
        If Len(dataUtama(30)) = 0 Then
            result(2) = "rmmodifikasitgl can't be empty" : GoTo selesai
        End If

        'rmcustomdbl1(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "rmcustomdbl1 can't be empty" : GoTo selesai
        End If

        'rmcustomdbl2(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "rmcustomdbl2 can't be empty" : GoTo selesai
        End If

        'rmcustomdbl3(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "rmcustomdbl3 can't be empty" : GoTo selesai
        End If

        'rmcustomdate1(43) As Date
        If Len(dataUtama(43)) = 0 Then
            result(2) = "rmcustomdate1 can't be empty" : GoTo selesai
        End If

        'rmcustomdate2(44) As Date
        If Len(dataUtama(44)) = 0 Then
            result(2) = "rmcustomdate2 can't be empty" : GoTo selesai
        End If

        'rmcustomdate3(45) As Date
        If Len(dataUtama(45)) = 0 Then
            result(2) = "rmcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "rmid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmnorek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmjumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "rmjumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "rmjumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmjumlahbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmstatusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmtgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rminputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rminputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "rmid~rmcabang~rmlokasi~rmsumber~rmautonotransaksi~rmnotransaksi~rmtgl~rmkodepa~rmcarabayar~rmkontak~rmkontakperson~rmnorek~rmuraian~rmcatatan~rmmatauang~rmkurs~rmjumlah~rmjumlahvalas~rmjumlahbayar~rmjumlahbayarvalas~rmstatusbayar~rmtgllunas~rmstatus~rmstatussebelumnya~rmjmlrevisi~rmcetakanke~rmisclose~rminputuser~rminputtgl~rmmodifikasiuser~rmmodifikasitgl~rmposting~rmcustomtext1~rmcustomtext2~rmcustomtext3~rmcustomtext4~rmcustomtext5~rmcustomint1~rmcustomint2~rmcustomint3~rmcustomdbl1~rmcustomdbl2~rmcustomdbl3~rmcustomdate1~rmcustomdate2~rmcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idrmdetail(0) As Integer, idrm(1) As Integer, norek(2) As String, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, catatan(7) As String, costcenter(8) As String, divisi(9) As String, 
        'subdivisi(10) As String, proyek(11) As String, urutan(12) As Integer, isclose(13) As Integer, customtext1(14) As String, 
        'customtext2(15) As String, customtext3(16) As String, customdbl1(17) As Double, customdbl2(18) As Double, customdbl3(19) As Double, 
        'customdate1(20) As Date, customdate2(21) As Date, customdate3(22) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idrmdetail, idrm, norek, matauang, kurs, jumlah, jumlahvalas, 
        'catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idrmdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idrm", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "norek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
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
            If (dataRowDetail.Length <> 23) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idrmdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idrmdetail required numeric." : GoTo selesai
            End If
            'idrm(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idrm required numeric." : GoTo selesai
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
            'urutan(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'isclose(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(20) As Date
            If (IsDate(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(21) As Date
            If (IsDate(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'norek(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - norek can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - norek should not be more than 25 character." : GoTo selesai
            End If

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
            If dataRowDetail(5) = 0 Then
                result(2) = "Row : " & i & " - jumlah can't be zero" : GoTo selesai
            End If

            'jumlahvalas(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - jumlahvalas can't be empty" : GoTo selesai
            End If

            'customdbl1(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(20) As Date
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(21) As Date
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idrmdetail~idrm~norek~matauang~kurs~jumlah~jumlahvalas~catatan~costcenter~divisi~subdivisi~proyek~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            strRekCostCenter = IIf(Len(strRekCostCenter.ToString) = 0, "", strRekCostCenter & " OR ")
            strRekCostCenter = String.Concat(strRekCostCenter, "(cnomor = '" & dataRowDetail(2) & "')")

        Next

        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================

        'MAPPING BUAT WS DATA PAY -------------------------------------------------------
        'idrmcarabayar(0) As Integer, idrm(1) As Integer, carabayar(2) As String, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, nogiro(7) As String, tgljt(8) As Date, bank(9) As String, 
        'noacbank(10) As String, rekbank(11) As String, rekgiro(12) As String, catatan(13) As String, urutan(14) As Integer

        'MAPPING BUAT FLEX DATA PAY -----------------------------------------------------
        'idrmcarabayar, idrm, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan

        'VALIDASI DAN SET DATA PAY ======================================================
        'SPLIT PARAMETER DATA PAY

        'Buat datatable pay
        Dim dtpay As New DataTable
        AsDataTableTambahField(dtpay, "idrmcarabayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "idrm", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpay, "carabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpay, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "jumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtpay, "jumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtpay, "nogiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "tgljt", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "bank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "noacbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "rekbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "rekgiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "urutan", AsEnumTypeData.AsInt64)

        'CEK CARABAYAR, JIKA GIRO MAKA HARUS KIRIM DATA GIRO
        Dim carabayar As Integer = 0
        If (dtutama.Rows.Count > 0) Then
            Dim drutama As DataRow = dtutama.Rows(0)
            carabayar = Val(drutama("rmcarabayar"))
        Else
            result(2) = "#1. Main transaction data not found." : GoTo selesai
        End If

        If (carabayar = 2 And Len(dataSplit(2)) = 0) Then
            result(2) = "Giro data data not found." : GoTo selesai

        ElseIf (carabayar = 2 And Len(dataSplit(2)) <> 0) Then
            dataGiro = dataSplit(2).Split(sptRow)

            'VALIDASI DAN SET DATA ROW GIRO ==================================================
            Dim JmlDtGiro As Integer = dataGiro.Length
            For i = 1 To JmlDtGiro
                'SPLIT DATA GIRO
                dataRowGiro = dataGiro(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA GIRO -----------------------------------
                'CEK ARRAY DATA GIRO
                If (dataRowGiro.Length <> 15) Then
                    result(2) = "Giro Row : " & i & " - Invalid giro transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW GIRO ----------------------------

                'VALIDASI TIPE DATA GIRO ------------------------------------------
                'idrmcarabayar(0) As Integer
                If (IsNumeric(dataRowGiro(0)) = False) Then
                    result(2) = "Giro Row : " & i & " - idrmcarabayar required numeric." : GoTo selesai
                End If
                'idrm(1) As Integer
                If (IsNumeric(dataRowGiro(1)) = False) Then
                    result(2) = "Giro Row : " & i & " - idrm required numeric." : GoTo selesai
                End If
                'carabayar(2) As Integer
                If (IsNumeric(dataRowGiro(2)) = False) Then
                    result(2) = "Giro Row : " & i & " - carabayar required numeric." : GoTo selesai
                End If
                'kurs(4) As Double
                If (IsNumeric(dataRowGiro(4)) = False) Then
                    result(2) = "Giro Row : " & i & " - kurs required numeric." : GoTo selesai
                End If
                'jumlah(5) As Double
                If (IsNumeric(dataRowGiro(5)) = False) Then
                    result(2) = "Giro Row : " & i & " - jumlah required numeric." : GoTo selesai
                End If
                'jumlahvalas(6) As Double
                If (IsNumeric(dataRowGiro(6)) = False) Then
                    result(2) = "Giro Row : " & i & " - jumlahvalas required numeric." : GoTo selesai
                End If
                'tgljt(8) As Date
                If (IsDate(dataRowGiro(8)) = False) Then
                    result(2) = "Giro Row : " & i & " - tgljt required date." : GoTo selesai
                End If
                'urutan(14) As Integer
                If (IsNumeric(dataRowGiro(14)) = False) Then
                    result(2) = "Giro Row : " & i & " - urutan required numeric." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA GIRO -----------------------------------

                'VALIDASI DATA GIRO ---------------------------------------
                'matauang(3) As String
                If Len(dataRowGiro(3)) = 0 Then
                    result(2) = "Giro Row : " & i & " - matauang can't be empty" : GoTo selesai
                End If
                If Len(dataRowGiro(3)) > 25 Then
                    result(2) = "Giro Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
                End If

                'kurs(4) As Double
                If Len(dataRowGiro(4)) = 0 Then
                    result(2) = "Giro Row : " & i & " - kurs can't be empty" : GoTo selesai
                End If

                'jumlah(5) As Double
                If Len(dataRowGiro(5)) = 0 Then
                    result(2) = "Giro Row : " & i & " - jumlah can't be empty" : GoTo selesai
                End If
                If dataRowGiro(5) <= 0 Then
                    result(2) = "Giro Row : " & i & " - jumlah must be more than zero" : GoTo selesai
                End If

                'jumlahvalas(6) As Double
                If Len(dataRowGiro(6)) = 0 Then
                    result(2) = "Giro Row : " & i & " - jumlahvalas can't be empty" : GoTo selesai
                End If

                'nogiro(7) As String
                If Len(dataRowGiro(7)) = 0 Then
                    result(2) = "Giro Row : " & i & " - nogiro can't be empty" : GoTo selesai
                End If
                If Len(dataRowGiro(7)) > 25 Then
                    result(2) = "Giro Row : " & i & " - nogiro should not be more than 25 character." : GoTo selesai
                End If

                'tgljt(8) As Date
                If Len(dataRowGiro(8)) = 0 Then
                    result(2) = "Giro Row : " & i & " - tgljt can't be empty" : GoTo selesai
                End If

                'bank(9) As String
                If Len(dataRowGiro(9)) = 0 Then
                    result(2) = "Giro Row : " & i & " - bank can't be empty" : GoTo selesai
                End If
                If Len(dataRowGiro(9)) > 25 Then
                    result(2) = "Giro Row : " & i & " - bank should not be more than 25 character." : GoTo selesai
                End If

                'noacbank(10) As String
                If Len(dataRowGiro(10)) = 0 Then
                    result(2) = "Giro Row : " & i & " - noacbank can't be empty" : GoTo selesai
                End If
                If Len(dataRowGiro(10)) > 50 Then
                    result(2) = "Giro Row : " & i & " - noacbank should not be more than 50 character." : GoTo selesai
                End If

                'rekbank(11) As String
                If Len(dataRowGiro(11)) = 0 Then
                    result(2) = "Giro Row : " & i & " - rekbank can't be empty" : GoTo selesai
                End If
                If Len(dataRowGiro(11)) > 25 Then
                    result(2) = "Giro Row : " & i & " - rekbank should not be more than 25 character." : GoTo selesai
                End If

                'rekgiro(12) As String
                If Len(dataRowGiro(12)) = 0 Then
                    result(2) = "Giro Row : " & i & " - rekgiro can't be empty" : GoTo selesai
                End If
                If Len(dataRowGiro(12)) > 25 Then
                    result(2) = "Giro Row : " & i & " - rekgiro should not be more than 25 character." : GoTo selesai
                End If
                'END OF VALIDASI DATA GIRO --------------------------------

                If AsDataTableTambahData(dtpay, "idrmcarabayar~idrm~carabayar~matauang~kurs~jumlah~jumlahvalas~nogiro~tgljt~bank~noacbank~rekbank~rekgiro~catatan~urutan", dataRowGiro(0) & "~" & dataRowGiro(1) & "~" & dataRowGiro(2) & "~" & dataRowGiro(3) & "~" & dataRowGiro(4) & "~" & dataRowGiro(5) & "~" & dataRowGiro(6) & "~" & dataRowGiro(7) & "~" & dataRowGiro(8) & "~" & dataRowGiro(9) & "~" & dataRowGiro(10) & "~" & dataRowGiro(11) & "~" & dataRowGiro(12) & "~" & dataRowGiro(13) & "~" & dataRowGiro(14)) = False Then
                    result(2) = "Giro Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next

            'END OF VALIDASI DAN SET ROW DATA GIRO ===========================================

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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("rmtgl")), AsFormatTanggal(drutama("rmtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "rmmatauang", "rmnorek", dtdetail, "norek")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                'CEK COA WAJIB COST CENTER ==============================
                If drutama("rmstatus") = 2 Then
                    Dim cekCoaCostCenter As String = ValidasiCoaRequiredCostCenter(strRekCostCenter, dtdetail)
                    If Len(cekCoaCostCenter) > 0 Then
                        result(2) = cekCoaCostCenter : Trans.Rollback() : GoTo selesai
                    End If
                End If
                'END OF CEK COA WAJIB COST CENTER =======================


                'VALIDASI NOMINAL HARUS SEIMBANG ========================
                Dim jml As Double = 0, jmlvalas As Double = 0, jmlgiro As Double = 0, jmlgirovalas As Double = 0
                jml = AsDataTableDSum(dtdetail, "jumlah")
                jmlvalas = AsDataTableDSum(dtdetail, "jumlahvalas")
                jmlgiro = AsDataTableDSum(dtpay, "jumlah")
                jmlgirovalas = AsDataTableDSum(dtpay, "jumlahvalas")

                ''AMBIL SETTING FORMAT NOMINAL
                'Dim digitGroup As String = "", pemisahDesimal As String = "", digitDesimal As Integer = 0
                'Dim FNominal As String = GetSettingNominal(digitGroup, pemisahDesimal, digitDesimal)
                'If len(FNominal) <> 0 Then result(2) = FNominal : Trans.Rollback() : GoTo selesai

                ''BULATKAN NOMINAL DETAIL SESUAI SETTING FORMAT NOMINAL
                'jml = math.round(jml, digitDesimal)
                'jmlvalas = math.round(jmlvalas, digitDesimal)
                'jmlgiro = math.round(jmlgiro, digitDesimal)
                'jmlgirovalas = math.round(jmlgirovalas, digitDesimal)

                'JML DETAIL DAN GIRO HARUS SEIMBANG
                If carabayar = 2 Then
                    If jml <> jmlgiro Then
                        result(2) = "Total amount of detail and giro are not balanced." : GoTo selesai
                    End If
                    If jmlvalas <> jmlgirovalas Then
                        result(2) = "Total amount of foreign detail and giro are not balanced." : GoTo selesai
                    End If
                End If

                'HITUNG TOTAL TOTAL BERDASARKAN DATA DETAIL
                drutama("rmjumlah") = jml
                drutama("rmjumlahvalas") = jmlvalas
                'END OF VALIDASI NOMINAL HARUS SEIMBANG =================


                If isUpdate Then
                    result(4) = drutama("rmid")
                    notransaksi = drutama("rmnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(rmid), rmnotransaksi FROM M2_Rm WHERE rmid='" & result(4) & "' AND rmstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(rmid) FROM m2_rm WHERE rmnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m2_rm_history
                        Dim rsSimpanHistory As String = SimpanHistory.M2_Rm_HistorySimpan("" & paramSplit(0) & "★M2_Rm_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("rmsumber")) & "▼" & FixQuotes(drutama("rmid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M2_Rm set rmcabang  = '" & FixQuotes(drutama("rmcabang")) & "', rmlokasi  = '" & FixQuotes(drutama("rmlokasi")) & "', rmsumber  = '" & FixQuotes(drutama("rmsumber")) & "', rmautonotransaksi  = " & drutama("rmautonotransaksi") & ", rmnotransaksi  = '" & notransaksi & "', rmtgl  = '" & FixQuotes(AsFormatTanggal(drutama("rmtgl"))) & "', rmkodepa  = " & drutama("rmkodepa") & ", rmcarabayar  = " & drutama("rmcarabayar") & ", rmkontak  = " & drutama("rmkontak") & ", rmkontakperson  = '" & FixQuotes(drutama("rmkontakperson")) & "', rmnorek  = '" & FixQuotes(drutama("rmnorek")) & "', rmuraian  = '" & FixQuotes(drutama("rmuraian")) & "', rmcatatan  = '" & FixQuotes(drutama("rmcatatan")) & "', rmmatauang  = '" & FixQuotes(drutama("rmmatauang")) & "', rmkurs  = '" & FixDouble(drutama("rmkurs")) & "', rmjumlah  = '" & FixDouble(drutama("rmjumlah")) & "', rmjumlahvalas  = '" & FixDouble(drutama("rmjumlahvalas")) & "', rmjumlahbayar  = '" & FixDouble(drutama("rmjumlahbayar")) & "', rmjumlahbayarvalas  = '" & FixDouble(drutama("rmjumlahbayarvalas")) & "', rmstatusbayar  = " & drutama("rmstatusbayar") & ", rmtgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("rmtgllunas"))) & "', rmstatus  = " & drutama("rmstatus") & ", rmstatussebelumnya  = " & drutama("rmstatussebelumnya") & ", rmjmlrevisi  = rmjmlrevisi+1, rmcetakanke  = " & drutama("rmcetakanke") & ", rmisclose  = " & drutama("rmisclose") & ", rmmodifikasiuser  = " & drutama("rmmodifikasiuser") & ", rmmodifikasitgl  = NOW(), rmposting  = 0, rmcustomtext1  = '" & FixQuotes(drutama("rmcustomtext1")) & "', rmcustomtext2  = '" & FixQuotes(drutama("rmcustomtext2")) & "', rmcustomtext3  = '" & FixQuotes(drutama("rmcustomtext3")) & "', rmcustomtext4  = '" & FixQuotes(drutama("rmcustomtext4")) & "', rmcustomtext5  = '" & FixQuotes(drutama("rmcustomtext5")) & "', rmcustomint1  = " & drutama("rmcustomint1") & ", rmcustomint2  = " & drutama("rmcustomint2") & ", rmcustomint3  = " & drutama("rmcustomint3") & ", rmcustomdbl1  = '" & FixDouble(drutama("rmcustomdbl1")) & "', rmcustomdbl2  = '" & FixDouble(drutama("rmcustomdbl2")) & "', rmcustomdbl3  = '" & FixDouble(drutama("rmcustomdbl3")) & "', rmcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("rmcustomdate1"))) & "', rmcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("rmcustomdate2"))) & "', rmcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("rmcustomdate3"))) & "' where rmid = '" & drutama("rmid") & "'"
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

                    If drutama("rmautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rmcabang"), drutama("rmlokasi"), drutama("rmsumber"), drutama("rmtgl"))
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
                        notransaksi = drutama("rmnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(rmid) FROM m2_rm WHERE rmnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M2_Rm (rmcabang, rmlokasi, rmsumber, rmautonotransaksi, rmnotransaksi, rmtgl, rmkodepa, rmcarabayar, rmkontak, rmkontakperson, rmnorek, rmuraian, rmcatatan, rmmatauang, rmkurs, rmjumlah, rmjumlahvalas, rmjumlahbayar, rmjumlahbayarvalas, rmstatusbayar, rmtgllunas, rmstatus, rmstatussebelumnya, rmjmlrevisi, rmcetakanke, rmisclose, rminputuser, rminputtgl, rmmodifikasiuser, rmmodifikasitgl, rmposting, rmcustomtext1, rmcustomtext2, rmcustomtext3, rmcustomtext4, rmcustomtext5, rmcustomint1, rmcustomint2, rmcustomint3, rmcustomdbl1, rmcustomdbl2, rmcustomdbl3, rmcustomdate1, rmcustomdate2, rmcustomdate3) values('" & FixQuotes(drutama("rmcabang")) & "', '" & FixQuotes(drutama("rmlokasi")) & "', '" & FixQuotes(drutama("rmsumber")) & "', " & drutama("rmautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("rmtgl"))) & "', " & drutama("rmkodepa") & ", " & drutama("rmcarabayar") & ", " & drutama("rmkontak") & ", '" & FixQuotes(drutama("rmkontakperson")) & "', '" & FixQuotes(drutama("rmnorek")) & "', '" & FixQuotes(drutama("rmuraian")) & "', '" & FixQuotes(drutama("rmcatatan")) & "', '" & FixQuotes(drutama("rmmatauang")) & "', '" & FixDouble(drutama("rmkurs")) & "', '" & FixDouble(drutama("rmjumlah")) & "', '" & FixDouble(drutama("rmjumlahvalas")) & "', '" & FixDouble(drutama("rmjumlahbayar")) & "', '" & FixDouble(drutama("rmjumlahbayarvalas")) & "', " & drutama("rmstatusbayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("rmtgllunas"))) & "', " & drutama("rmstatus") & ", " & drutama("rmstatussebelumnya") & ", " & drutama("rmjmlrevisi") & ", " & drutama("rmcetakanke") & ", " & drutama("rmisclose") & ", " & drutama("rminputuser") & ", NOW(), " & drutama("rmmodifikasiuser") & ", '1971-01-01 00:00:00', 0, '" & FixQuotes(drutama("rmcustomtext1")) & "', '" & FixQuotes(drutama("rmcustomtext2")) & "', '" & FixQuotes(drutama("rmcustomtext3")) & "', '" & FixQuotes(drutama("rmcustomtext4")) & "', '" & FixQuotes(drutama("rmcustomtext5")) & "', " & drutama("rmcustomint1") & ", " & drutama("rmcustomint2") & ", " & drutama("rmcustomint3") & ", '" & FixDouble(drutama("rmcustomdbl1")) & "', '" & FixDouble(drutama("rmcustomdbl2")) & "', '" & FixDouble(drutama("rmcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rmcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rmcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rmcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select rmid from M2_Rm where rmnotransaksi='" & notransaksi & "' AND Rminputuser= '" & userid & "' order by Rmmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M2_Rm_Detail where idrm = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idrmdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("norek")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M2_Rm_Detail(idrmdetail, idrm, norek, matauang, kurs, jumlah, jumlahvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                'Hapus detail pay ketika update
                If (isUpdate) Then
                    sql = "Delete from M2_Rm_Pay where idrm = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses detail pay
                If (dtpay.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder, strGiro As New StringBuilder
                    For Each dr1 As DataRow In dtpay.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idrmcarabayar") & ", " & result(4) & ", " & dr1("carabayar") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ")")
                        'query untuk insert giro
                        If drutama("rmstatus") = 2 Then
                            strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", ", "))
                            strGiro.Append("('" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(drutama("rmsumber")) & "', " & result(4) & ", '" & FixQuotes(notransaksi) & "', " & drutama("rmkontak") & ", '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', " & 0 & ", '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', " & 0 & ", " & 0 & ", " & dr1("urutan") & ")")
                        End If
                    Next
                    sql = "Insert into M2_Rm_Pay(idrmcarabayar, idrm, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'insert giro jika status approved
                    If drutama("rmstatus") = 2 And Len(strGiro.ToString) > 0 Then
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
                End If

                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "RM", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("rmstatus") = 2 Then
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
    Public Function M2_RmUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("rmkontakkode", "c1.kkode")
            Filter = Filter.Replace("rmkontaknama", "c1.knama")
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
            Dim sumber As String = "Rm", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Rmtgl, Rmnotransaksi, Rmstatus FROM m2_Rm WHERE Rmid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Rmstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m2_rm_history
            Dim rsSimpanHistory As String = SimpanHistory.M2_Rm_HistorySimpan("" & paramSplit(0) & "★M2_Rm_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m2_rm_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                'CEK STATUS GIRO
                dtdetail = AsDataTableAmbilDariDB("SELECT glnogiro FROM m2_giro_list WHERE glsumber = 'RM' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "' AND glstatus <> 0")
                If dtdetail.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai

                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'RM' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'DELETE GIRO
                sql = "DELETE FROM m2_giro_list WHERE glsumber = 'RM' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M2_Rm SET Rmstatus = " & nilaiStatus & ", Rmmodifikasiuser='" & userid & "', Rmmodifikasitgl = NOW(), Rmposting = 0, Rmpostingtgl = '1971-01-01 00:00:00', Rmjmlrevisi = Rmjmlrevisi + 1 WHERE Rmid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_RmSearch(PostWsSearch(paramSplit(0), "M2_RmSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_RmDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("rmkontakkode", "c1.kkode")
            Filter = Filter.Replace("rmkontaknama", "c1.knama")
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
            Dim sumber As String = "Rm", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Rmid, Rmnotransaksi FROM m2_Rm WHERE Rmid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT rmcabang, rmlokasi, rmsumber, rmautonotransaksi, rmnotransaksi, rmtgl"
            sql &= " FROM M2_rm"
            sql &= " WHERE rmid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("rmcabang")
                lokasi = dtNomorNext.Rows(0)("rmlokasi")
                sumber = dtNomorNext.Rows(0)("rmsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("rmautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("rmnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("rmtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE JURNAL
            sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'RM' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE GIRO
            sql = "DELETE FROM m2_giro_list WHERE glsumber = 'RM' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE PAY
            sql = "DELETE FROM M2_Rm_Pay WHERE idRm = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M2_Rm_Detail WHERE idRm = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M2_Rm WHERE Rmid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_RmSearch(PostWsSearch(paramSplit(0), "M2_RmSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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