Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m2_aj
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M2_AjSimpan(ByVal param As String) As String
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
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'ajid(0) As Integer, ajcabang(1) As String, ajlokasi(2) As String, ajsumber(3) As String, ajautonotransaksi(4) As Integer, 
        'ajnotransaksi(5) As String, ajtgl(6) As Date, ajkodepa(7) As Integer, ajkontak(8) As Integer, ajkontakperson(9) As String, 
        'ajuraian(10) As String, ajcatatan(11) As String, ajmatauang(12) As String, ajkurs(13) As Double, ajdebit(14) As Double, 
        'ajdebitvalas(15) As Double, ajkredit(16) As Double, ajkreditvalas(17) As Double, ajjumlahbayar(18) As Double, ajjumlahbayarvalas(19) As Double, 
        'ajstatusbayar(20) As Integer, ajtgllunas(21) As Date, ajstatus(22) As Integer, ajstatussebelumnya(23) As Integer, ajjmlrevisi(24) As Integer, 
        'ajcetakanke(25) As Integer, ajisclose(26) As Integer, ajinputuser(27) As Integer, ajinputtgl(28) As DateTime, ajmodifikasiuser(29) As Integer, 
        'ajmodifikasitgl(30) As DateTime, ajposting(31) As Integer, ajcustomtext1(32) As String, ajcustomtext2(33) As String, ajcustomtext3(34) As String, 
        'ajcustomtext4(35) As String, ajcustomtext5(36) As String, ajcustomint1(37) As Integer, ajcustomint2(38) As Integer, ajcustomint3(39) As Integer, 
        'ajcustomdbl1(40) As Double, ajcustomdbl2(41) As Double, ajcustomdbl3(42) As Double, ajcustomdate1(43) As Date, ajcustomdate2(44) As Date, 
        'ajcustomdate3(45) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'ajid, ajcabang, ajlokasi, ajsumber, ajautonotransaksi, ajnotransaksi, ajtgl, 
        'ajkodepa, ajkontak, ajkontakperson, ajuraian, ajcatatan, ajmatauang, ajkurs, 
        'ajdebit, ajdebitvalas, ajkredit, ajkreditvalas, ajjumlahbayar, ajjumlahbayarvalas, ajstatusbayar, 
        'ajtgllunas, ajstatus, ajstatussebelumnya, ajjmlrevisi, ajcetakanke, ajisclose, ajinputuser, 
        'ajinputtgl, ajmodifikasiuser, ajmodifikasitgl, ajposting, ajcustomtext1, ajcustomtext2, ajcustomtext3, 
        'ajcustomtext4, ajcustomtext5, ajcustomint1, ajcustomint2, ajcustomint3, ajcustomdbl1, ajcustomdbl2, 
        'ajcustomdbl3, ajcustomdate1, ajcustomdate2, ajcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 46) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'ajid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "ajid required numeric." : GoTo selesai
        End If
        'ajautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "ajautonotransaksi required numeric." : GoTo selesai
        End If
        'ajtgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "ajtgl required date." : GoTo selesai
        End If
        'ajkodepa(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "ajkodepa required numeric." : GoTo selesai
        End If
        'ajkontak(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "ajkontak required numeric." : GoTo selesai
        End If
        If (dataUtama(8) < 1) Then
            result(2) = "ajkontak can't be empty." : GoTo selesai
        End If
        'ajkurs(13) As Double
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "ajkurs required numeric." : GoTo selesai
        End If
        'ajdebit(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "ajdebit required numeric." : GoTo selesai
        End If
        'ajdebitvalas(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "ajdebitvalas required numeric." : GoTo selesai
        End If
        'ajkredit(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "ajkredit required numeric." : GoTo selesai
        End If
        'ajkreditvalas(17) As Double
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "ajkreditvalas required numeric." : GoTo selesai
        End If
        'ajjumlahbayar(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "ajjumlahbayar required numeric." : GoTo selesai
        End If
        'ajjumlahbayarvalas(19) As Double
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "ajjumlahbayarvalas required numeric." : GoTo selesai
        End If
        'ajstatusbayar(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "ajstatusbayar required numeric." : GoTo selesai
        End If
        'ajtgllunas(21) As Date
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "ajtgllunas required date." : GoTo selesai
        End If
        'ajstatus(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "ajstatus required numeric." : GoTo selesai
        End If
        'ajstatussebelumnya(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "ajstatussebelumnya required numeric." : GoTo selesai
        End If
        'ajjmlrevisi(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "ajjmlrevisi required numeric." : GoTo selesai
        End If
        'ajcetakanke(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "ajcetakanke required numeric." : GoTo selesai
        End If
        'ajisclose(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "ajisclose required numeric." : GoTo selesai
        End If
        'ajinputuser(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "ajinputuser required numeric." : GoTo selesai
        End If
        'ajinputtgl(28) As DateTime
        If (IsDate(dataUtama(28)) = False) Then
            result(2) = "ajinputtgl required date." : GoTo selesai
        End If
        'ajmodifikasiuser(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "ajmodifikasiuser required numeric." : GoTo selesai
        End If
        'ajmodifikasitgl(30) As DateTime
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "ajmodifikasitgl required date." : GoTo selesai
        End If
        'ajposting(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "ajposting required numeric." : GoTo selesai
        End If
        'ajcustomint1(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "ajcustomint1 required numeric." : GoTo selesai
        End If
        'ajcustomint2(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "ajcustomint2 required numeric." : GoTo selesai
        End If
        'ajcustomint3(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "ajcustomint3 required numeric." : GoTo selesai
        End If
        'ajcustomdbl1(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "ajcustomdbl1 required numeric." : GoTo selesai
        End If
        'ajcustomdbl2(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "ajcustomdbl2 required numeric." : GoTo selesai
        End If
        'ajcustomdbl3(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "ajcustomdbl3 required numeric." : GoTo selesai
        End If
        'ajcustomdate1(43) As Date
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "ajcustomdate1 required date." : GoTo selesai
        End If
        'ajcustomdate2(44) As Date
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "ajcustomdate2 required date." : GoTo selesai
        End If
        'ajcustomdate3(45) As Date
        If (IsDate(dataUtama(45)) = False) Then
            result(2) = "ajcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'ajcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "ajcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "ajcabang should not be more than 25 character." : GoTo selesai
        End If

        'ajlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "ajlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "ajlokasi should not be more than 25 character." : GoTo selesai
        End If

        'ajsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "ajsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "ajsumber should not be more than 10 character." : GoTo selesai
        End If

        'ajnotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "ajnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "ajnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'ajtgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "ajtgl can't be empty" : GoTo selesai
        End If

        'ajmatauang(12) As String
        If Len(dataUtama(12)) = 0 Then
            result(2) = "ajmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(12)) > 25 Then
            result(2) = "ajmatauang should not be more than 25 character." : GoTo selesai
        End If

        'ajkurs(13) As Double
        If Len(dataUtama(13)) = 0 Then
            result(2) = "ajkurs can't be empty" : GoTo selesai
        End If

        'ajdebit(14) As Double
        If Len(dataUtama(14)) = 0 Then
            result(2) = "ajdebit can't be empty" : GoTo selesai
        End If

        'ajdebitvalas(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "ajdebitvalas can't be empty" : GoTo selesai
        End If

        'ajkredit(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "ajkredit can't be empty" : GoTo selesai
        End If

        'ajkreditvalas(17) As Double
        If Len(dataUtama(17)) = 0 Then
            result(2) = "ajkreditvalas can't be empty" : GoTo selesai
        End If

        'ajjumlahbayar(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "ajjumlahbayar can't be empty" : GoTo selesai
        End If

        'ajjumlahbayarvalas(19) As Double
        If Len(dataUtama(19)) = 0 Then
            result(2) = "ajjumlahbayarvalas can't be empty" : GoTo selesai
        End If

        'ajinputtgl(28) As DateTime
        If Len(dataUtama(28)) = 0 Then
            result(2) = "ajinputtgl can't be empty" : GoTo selesai
        End If

        'ajmodifikasitgl(30) As DateTime
        If Len(dataUtama(30)) = 0 Then
            result(2) = "ajmodifikasitgl can't be empty" : GoTo selesai
        End If

        'ajcustomdbl1(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "ajcustomdbl1 can't be empty" : GoTo selesai
        End If

        'ajcustomdbl2(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "ajcustomdbl2 can't be empty" : GoTo selesai
        End If

        'ajcustomdbl3(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "ajcustomdbl3 can't be empty" : GoTo selesai
        End If

        'ajcustomdate1(43) As Date
        If Len(dataUtama(43)) = 0 Then
            result(2) = "ajcustomdate1 can't be empty" : GoTo selesai
        End If

        'ajcustomdate2(44) As Date
        If Len(dataUtama(44)) = 0 Then
            result(2) = "ajcustomdate2 can't be empty" : GoTo selesai
        End If

        'ajcustomdate3(45) As Date
        If Len(dataUtama(45)) = 0 Then
            result(2) = "ajcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "ajid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajdebit", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "ajdebitvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "ajkredit", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "ajkreditvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "ajjumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajjumlahbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajstatusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajtgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "ajid~ajcabang~ajlokasi~ajsumber~ajautonotransaksi~ajnotransaksi~ajtgl~ajkodepa~ajkontak~ajkontakperson~ajuraian~ajcatatan~ajmatauang~ajkurs~ajdebit~ajdebitvalas~ajkredit~ajkreditvalas~ajjumlahbayar~ajjumlahbayarvalas~ajstatusbayar~ajtgllunas~ajstatus~ajstatussebelumnya~ajjmlrevisi~ajcetakanke~ajisclose~ajinputuser~ajinputtgl~ajmodifikasiuser~ajmodifikasitgl~ajposting~ajcustomtext1~ajcustomtext2~ajcustomtext3~ajcustomtext4~ajcustomtext5~ajcustomint1~ajcustomint2~ajcustomint3~ajcustomdbl1~ajcustomdbl2~ajcustomdbl3~ajcustomdate1~ajcustomdate2~ajcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idajdetail(0) As Integer, idaj(1) As Integer, norek(2) As String, matauang(3) As String, kurs(4) As Double, 
        'debit(5) As Double, debitvalas(6) As Double, kredit(7) As Double, kreditvalas(8) As Double, catatan(9) As String, 
        'costcenter(10) As String, divisi(11) As String, subdivisi(12) As String, proyek(13) As String, urutan(14) As Integer, 
        'isclose(15) As Integer, customtext1(16) As String, customtext2(17) As String, customtext3(18) As String, customdbl1(19) As Double, 
        'customdbl2(20) As Double, customdbl3(21) As Double, customdate1(22) As Date, customdate2(23) As Date, customdate3(24) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idajdetail, idaj, norek, matauang, kurs, debit, debitvalas, 
        'kredit, kreditvalas, catatan, costcenter, divisi, subdivisi, proyek, 
        'urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idajdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idaj", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "norek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "debit", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "debitvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "kredit", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "kreditvalas", AsEnumTypeData.AsDouble)
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
            If (dataRowDetail.Length <> 25) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idajdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idajdetail required numeric." : GoTo selesai
            End If
            'idaj(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idaj required numeric." : GoTo selesai
            End If
            'kurs(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'debit(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - debit required numeric." : GoTo selesai
            End If
            'debitvalas(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - debitvalas required numeric." : GoTo selesai
            End If
            'kredit(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - kredit required numeric." : GoTo selesai
            End If
            'kreditvalas(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - kreditvalas required numeric." : GoTo selesai
            End If
            'urutan(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'isclose(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(20) As Double
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(21) As Double
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(23) As Date
            If (IsDate(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(24) As Date
            If (IsDate(dataRowDetail(24)) = False) Then
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

            'debit(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - debit can't be empty" : GoTo selesai
            End If

            'debitvalas(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - debitvalas can't be empty" : GoTo selesai
            End If

            'kredit(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - kredit can't be empty" : GoTo selesai
            End If

            'kreditvalas(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - kreditvalas can't be empty" : GoTo selesai
            End If

            'validasi jumlah debit dan kredit tidak boleh diisi keduanya
            If dataRowDetail(5) = 0 And dataRowDetail(7) = 0 Then
                result(2) = "Row : " & i & " - debits and credits can't be zero" : GoTo selesai
            End If
            If dataRowDetail(5) <> 0 And dataRowDetail(7) <> 0 Then
                result(2) = "Row : " & i & " - debits and credits can't be filled in both" : GoTo selesai
            End If
            If dataRowDetail(6) <> 0 And dataRowDetail(8) <> 0 Then
                result(2) = "Row : " & i & " - foreign debits and credits can't be filled in both" : GoTo selesai
            End If

            'customdbl1(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(23) As Date
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(24) As Date
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idajdetail~idaj~norek~matauang~kurs~debit~debitvalas~kredit~kreditvalas~catatan~costcenter~divisi~subdivisi~proyek~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            strRekCostCenter = IIf(Len(strRekCostCenter.ToString) = 0, "", strRekCostCenter & " OR ")
            strRekCostCenter = String.Concat(strRekCostCenter, "(cnomor = '" & dataRowDetail(2) & "')")

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
                Dim vModuleId As Integer = 2, vMenuId As Integer = 8
                Select Case drutama("ajstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("ajtgl")), AsFormatTanggal(drutama("ajtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "ajmatauang", "", dtdetail, "norek")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                'CEK COA WAJIB COST CENTER ==============================
                If drutama("ajstatus") = 2 Then
                    Dim cekCoaCostCenter As String = ValidasiCoaRequiredCostCenter(strRekCostCenter, dtdetail)
                    If Len(cekCoaCostCenter) > 0 Then
                        result(2) = cekCoaCostCenter : Trans.Rollback() : GoTo selesai
                    End If
                End If
                'END OF CEK COA WAJIB COST CENTER =======================


                'VALIDASI NOMINAL HARUS SEIMBANG ========================
                Dim debit As Double = 0, kredit As Double = 0, debitvalas As Double = 0, kreditvalas As Double = 0
                debit = AsDataTableDSum(dtdetail, "debit")
                debitvalas = AsDataTableDSum(dtdetail, "debitvalas")
                kredit = AsDataTableDSum(dtdetail, "kredit")
                kreditvalas = AsDataTableDSum(dtdetail, "kreditvalas")

                ''AMBIL SETTING FORMAT NOMINAL
                'Dim digitGroup As String = "", pemisahDesimal As String = "", digitDesimal As Integer = 0
                'Dim FNominal As String = GetSettingNominal(digitGroup, pemisahDesimal, digitDesimal)
                'If len(FNominal) <> 0 Then result(2) = FNominal : Trans.Rollback() : GoTo selesai

                ''BULATKAN NOMINAL DETAIL SESUAI SETTING FORMAT NOMINAL
                'debit = math.round(debit, digitDesimal)
                'debitvalas = math.round(debitvalas, digitDesimal)
                'kredit = math.round(kredit, digitDesimal)
                'kreditvalas = math.round(kreditvalas, digitDesimal)

                'VALIDASI NOMINAL HARUS SEIMBANG
                If Math.Round(debit, 2) <> Math.Round(kredit, 2) Then
                    result(2) = "Total debits and credits in detail are not balanced." : GoTo selesai
                End If
                If Math.Round(debitvalas, 2) <> Math.Round(kreditvalas, 2) Then
                    result(2) = "Total foreign debits and credits in detail are not balanced." : GoTo selesai
                End If

                'HITUNG TOTAL TOTAL BERDASARKAN DATA DETAIL
                drutama("ajdebit") = debit
                drutama("ajdebitvalas") = debitvalas
                drutama("ajkredit") = kredit
                drutama("ajkreditvalas") = kreditvalas
                'END OF VALIDASI NOMINAL HARUS SEIMBANG =================


                If isUpdate Then
                    result(4) = drutama("ajid")
                    notransaksi = drutama("ajnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(ajid), ajnotransaksi FROM M2_aj WHERE ajid='" & result(4) & "' AND ajstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("ajautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("ajcabang"), drutama("ajlokasi"), drutama("ajsumber"), drutama("ajtgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(ajid) FROM m2_aj WHERE ajnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m2_aj_history
                        Dim rsSimpanHistory As String = SimpanHistory.M2_Aj_HistorySimpan("" & paramSplit(0) & "★M2_Aj_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("ajsumber")) & "▼" & FixQuotes(drutama("ajid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M2_Aj set ajcabang  = '" & FixQuotes(drutama("ajcabang")) & "', ajlokasi  = '" & FixQuotes(drutama("ajlokasi")) & "', ajsumber  = '" & FixQuotes(drutama("ajsumber")) & "', ajautonotransaksi  = " & drutama("ajautonotransaksi") & ", ajnotransaksi  = '" & notransaksi & "', ajtgl  = '" & FixQuotes(AsFormatTanggal(drutama("ajtgl"))) & "', ajkodepa  = " & drutama("ajkodepa") & ", ajkontak  = " & drutama("ajkontak") & ", ajkontakperson  = '" & FixQuotes(drutama("ajkontakperson")) & "', ajuraian  = '" & FixQuotes(drutama("ajuraian")) & "', ajcatatan  = '" & FixQuotes(drutama("ajcatatan")) & "', ajmatauang  = '" & FixQuotes(drutama("ajmatauang")) & "', ajkurs  = '" & FixDouble(drutama("ajkurs")) & "', ajdebit  = '" & FixDouble(drutama("ajdebit")) & "', ajdebitvalas  = '" & FixDouble(drutama("ajdebitvalas")) & "', ajkredit  = '" & FixDouble(drutama("ajkredit")) & "', ajkreditvalas  = '" & FixDouble(drutama("ajkreditvalas")) & "', ajjumlahbayar  = '" & FixDouble(drutama("ajjumlahbayar")) & "', ajjumlahbayarvalas  = '" & FixDouble(drutama("ajjumlahbayarvalas")) & "', ajstatusbayar  = " & drutama("ajstatusbayar") & ", ajtgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("ajtgllunas"))) & "', ajstatus  = " & drutama("ajstatus") & ", ajstatussebelumnya  = " & drutama("ajstatussebelumnya") & ", ajjmlrevisi  = ajjmlrevisi + 1, ajcetakanke  = " & drutama("ajcetakanke") & ", ajisclose  = " & drutama("ajisclose") & ", ajmodifikasiuser  = " & drutama("ajmodifikasiuser") & ", ajmodifikasitgl  = NOW(), ajposting  = 0, ajcustomtext1  = '" & FixQuotes(drutama("ajcustomtext1")) & "', ajcustomtext2  = '" & FixQuotes(drutama("ajcustomtext2")) & "', ajcustomtext3  = '" & FixQuotes(drutama("ajcustomtext3")) & "', ajcustomtext4  = '" & FixQuotes(drutama("ajcustomtext4")) & "', ajcustomtext5  = '" & FixQuotes(drutama("ajcustomtext5")) & "', ajcustomint1  = " & drutama("ajcustomint1") & ", ajcustomint2  = " & drutama("ajcustomint2") & ", ajcustomint3  = " & drutama("ajcustomint3") & ", ajcustomdbl1  = '" & FixDouble(drutama("ajcustomdbl1")) & "', ajcustomdbl2  = '" & FixDouble(drutama("ajcustomdbl2")) & "', ajcustomdbl3  = '" & FixDouble(drutama("ajcustomdbl3")) & "', ajcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("ajcustomdate1"))) & "', ajcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("ajcustomdate2"))) & "', ajcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("ajcustomdate3"))) & "' where ajid = '" & drutama("ajid") & "'"
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

                    If drutama("ajautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("ajcabang"), drutama("ajlokasi"), drutama("ajsumber"), drutama("ajtgl"))
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
                        notransaksi = drutama("ajnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(ajid) FROM m2_aj WHERE ajnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M2_Aj (ajcabang, ajlokasi, ajsumber, ajautonotransaksi, ajnotransaksi, ajtgl, ajkodepa, ajkontak, ajkontakperson, ajuraian, ajcatatan, ajmatauang, ajkurs, ajdebit, ajdebitvalas, ajkredit, ajkreditvalas, ajjumlahbayar, ajjumlahbayarvalas, ajstatusbayar, ajtgllunas, ajstatus, ajstatussebelumnya, ajjmlrevisi, ajcetakanke, ajisclose, ajinputuser, ajinputtgl, ajmodifikasiuser, ajmodifikasitgl, ajposting, ajcustomtext1, ajcustomtext2, ajcustomtext3, ajcustomtext4, ajcustomtext5, ajcustomint1, ajcustomint2, ajcustomint3, ajcustomdbl1, ajcustomdbl2, ajcustomdbl3, ajcustomdate1, ajcustomdate2, ajcustomdate3) values('" & FixQuotes(drutama("ajcabang")) & "', '" & FixQuotes(drutama("ajlokasi")) & "', '" & FixQuotes(drutama("ajsumber")) & "', " & drutama("ajautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("ajtgl"))) & "', " & drutama("ajkodepa") & ", " & drutama("ajkontak") & ", '" & FixQuotes(drutama("ajkontakperson")) & "', '" & FixQuotes(drutama("ajuraian")) & "', '" & FixQuotes(drutama("ajcatatan")) & "', '" & FixQuotes(drutama("ajmatauang")) & "', '" & FixDouble(drutama("ajkurs")) & "', '" & FixDouble(drutama("ajdebit")) & "', '" & FixDouble(drutama("ajdebitvalas")) & "', '" & FixDouble(drutama("ajkredit")) & "', '" & FixDouble(drutama("ajkreditvalas")) & "', '" & FixDouble(drutama("ajjumlahbayar")) & "', '" & FixDouble(drutama("ajjumlahbayarvalas")) & "', " & drutama("ajstatusbayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("ajtgllunas"))) & "', " & drutama("ajstatus") & ", " & drutama("ajstatussebelumnya") & ", " & drutama("ajjmlrevisi") & ", " & drutama("ajcetakanke") & ", " & drutama("ajisclose") & ", " & drutama("ajinputuser") & ", NOW(), " & drutama("ajmodifikasiuser") & ", '1971-01-01 00:00:00', 0, '" & FixQuotes(drutama("ajcustomtext1")) & "', '" & FixQuotes(drutama("ajcustomtext2")) & "', '" & FixQuotes(drutama("ajcustomtext3")) & "', '" & FixQuotes(drutama("ajcustomtext4")) & "', '" & FixQuotes(drutama("ajcustomtext5")) & "', " & drutama("ajcustomint1") & ", " & drutama("ajcustomint2") & ", " & drutama("ajcustomint3") & ", '" & FixDouble(drutama("ajcustomdbl1")) & "', '" & FixDouble(drutama("ajcustomdbl2")) & "', '" & FixDouble(drutama("ajcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ajcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ajcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ajcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select ajid from M2_aj where ajnotransaksi='" & notransaksi & "' AND ajinputuser= '" & userid & "' order by ajmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M2_Aj_Detail where idaj = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idajdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("norek")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("debit")) & "', '" & FixDouble(dr1("debitvalas")) & "', '" & FixDouble(dr1("kredit")) & "', '" & FixDouble(dr1("kreditvalas")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M2_Aj_Detail(idajdetail, idaj, norek, matauang, kurs, debit, debitvalas, kredit, kreditvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                Dim sumber As String = "AJ", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("ajstatus") = 2 Then
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
    Public Function M2_AjUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("ajkontakkode", "c1.kkode")
            Filter = Filter.Replace("ajkontaknama", "c1.knama")
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
            Dim sumber As String = "Aj", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Ajtgl, Ajnotransaksi, Ajstatus FROM m2_Aj WHERE Ajid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Ajstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m2_aj_history
            Dim rsSimpanHistory As String = SimpanHistory.M2_Aj_HistorySimpan("" & paramSplit(0) & "★M2_Aj_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then
                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'Aj' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M2_Aj SET Ajstatus = " & nilaiStatus & ", Ajmodifikasiuser='" & userid & "', Ajmodifikasitgl = NOW(), Ajposting = 0, Ajpostingtgl = '1971-01-01 00:00:00', Ajjmlrevisi = Ajjmlrevisi + 1 WHERE Ajid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_AjSearch(PostWsSearch(paramSplit(0), "M2_AjSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_AjDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("ajkontakkode", "c1.kkode")
            Filter = Filter.Replace("ajkontaknama", "c1.knama")
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
            Dim sumber As String = "Aj", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Ajid, Ajnotransaksi FROM m2_Aj WHERE Ajid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT ajcabang, ajlokasi, ajsumber, ajautonotransaksi, ajnotransaksi, ajtgl"
            sql &= " FROM M2_Aj"
            sql &= " WHERE Ajid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("ajcabang")
                lokasi = dtNomorNext.Rows(0)("ajlokasi")
                sumber = dtNomorNext.Rows(0)("ajsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("ajautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("ajnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("ajtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE JURNAL
            sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'Aj' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M2_Aj_Detail WHERE idAj = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M2_Aj WHERE Ajid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_AjSearch(PostWsSearch(paramSplit(0), "M2_AjSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_AjGetdataById(ByVal param As String) As String

        'M2_AjGetdataById Utama --------------------------------------------------------
        'ajid, ajcabang, ajlokasi, ajsumber, ajautonotransaksi, ajnotransaksi, ajtgl, 
        'ajkodepa, ajkontak, ajkontakperson, ajuraian, ajcatatan, ajmatauang, ajkurs, 
        'ajdebit, ajdebitvalas, ajkredit, ajkreditvalas, ajjumlahbayar, ajjumlahbayarvalas, ajstatusbayar, 
        'ajtgllunas, ajstatus, ajstatussebelumnya, ajjmlrevisi, ajcetakanke, ajisclose, ajinputuser, 
        'ajinputtgl, ajmodifikasiuser, ajmodifikasitgl, ajposting, ajpostingtgl, ajcustomtext1, ajcustomtext2, 
        'ajcustomtext3, ajcustomtext4, ajcustomtext5, ajcustomint1, ajcustomint2, ajcustomint3, ajcustomdbl1, 
        'ajcustomdbl2, ajcustomdbl3, ajcustomdate1, ajcustomdate2, ajcustomdate3, ajcabangnama, ajlokasinama, 
        'ajkontakkode, ajkontaknama, ajstatusnama, ajstatussebelumnyanama, ajinputusernama, ajmodifikasiusernama

        'M2_AjGetdataById Detail -------------------------------------------------------
        'idajdetail, idaj, norek, matauang, kurs, debit, debitvalas, kredit, 
        'kreditvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, noreknama, costcenternama, divisinama, subdivisinama, 
        'proyeknama

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

        Dim NmMemcached As String = "aplikasi1-M2_Aj~M2_Aj_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "ajid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "ajid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_aj_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("ajid"), 0), sptField,
                     FxDB(drutama("ajcabang"), ""), sptField,
                     FxDB(drutama("ajlokasi"), ""), sptField,
                     FxDB(drutama("ajsumber"), ""), sptField,
                     FxDB(drutama("ajautonotransaksi"), 0), sptField,
                     FxDB(drutama("ajnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ajtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("ajkodepa"), 0), sptField,
                     FxDB(drutama("ajkontak"), 0), sptField,
                     FxDB(drutama("ajkontakperson"), ""), sptField,
                     FxDB(drutama("ajuraian"), ""), sptField,
                     FxDB(drutama("ajcatatan"), ""), sptField,
                     FxDB(drutama("ajmatauang"), ""), sptField,
                     FxDB(drutama("ajkurs"), 0), sptField,
                     FxDB(drutama("ajdebit"), 0), sptField,
                     FxDB(drutama("ajdebitvalas"), 0), sptField,
                     FxDB(drutama("ajkredit"), 0), sptField,
                     FxDB(drutama("ajkreditvalas"), 0), sptField,
                     FxDB(drutama("ajjumlahbayar"), 0), sptField,
                     FxDB(drutama("ajjumlahbayarvalas"), 0), sptField,
                     FxDB(drutama("ajstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ajtgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("ajstatus"), 0), sptField,
                     FxDB(drutama("ajstatussebelumnya"), 0), sptField,
                     FxDB(drutama("ajjmlrevisi"), 0), sptField,
                     FxDB(drutama("ajcetakanke"), 0), sptField,
                     FxDB(drutama("ajisclose"), 0), sptField,
                     FxDB(drutama("ajinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ajinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("ajmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ajmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("ajposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ajpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("ajcustomtext1"), ""), sptField,
                     FxDB(drutama("ajcustomtext2"), ""), sptField,
                     FxDB(drutama("ajcustomtext3"), ""), sptField,
                     FxDB(drutama("ajcustomtext4"), ""), sptField,
                     FxDB(drutama("ajcustomtext5"), ""), sptField,
                     FxDB(drutama("ajcustomint1"), 0), sptField,
                     FxDB(drutama("ajcustomint2"), 0), sptField,
                     FxDB(drutama("ajcustomint3"), 0), sptField,
                     FxDB(drutama("ajcustomdbl1"), 0), sptField,
                     FxDB(drutama("ajcustomdbl2"), 0), sptField,
                     FxDB(drutama("ajcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ajcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ajcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ajcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("ajcabangnama"), ""), sptField,
                     FxDB(drutama("ajlokasinama"), ""), sptField,
                     FxDB(drutama("ajkontakkode"), ""), sptField,
                     FxDB(drutama("ajkontaknama"), ""), sptField,
                     FxDB(drutama("ajstatusnama"), ""), sptField,
                     FxDB(drutama("ajstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("ajinputusernama"), ""), sptField,
                     FxDB(drutama("ajmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idajdetail"), 0), sptField,
                     FxDB(dr("idaj"), 0), sptField,
                     FxDB(dr("norek"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("debit"), 0), sptField,
                     FxDB(dr("debitvalas"), 0), sptField,
                     FxDB(dr("kredit"), 0), sptField,
                     FxDB(dr("kreditvalas"), 0), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ajid, ajcabang, ajlokasi, ajsumber, ajautonotransaksi, ajnotransaksi, ajtgl, ajkodepa, ajkontak, ajkontakperson, ajuraian, ajcatatan, ajmatauang, ajkurs, ajdebit, ajdebitvalas, ajkredit, ajkreditvalas, ajjumlahbayar, ajjumlahbayarvalas, ajstatusbayar, ajtgllunas, ajstatus, ajstatussebelumnya, ajjmlrevisi, ajcetakanke, ajisclose, ajinputuser, ajinputtgl, ajmodifikasiuser, ajmodifikasitgl, ajposting, ajpostingtgl, ajcustomtext1, ajcustomtext2, ajcustomtext3, ajcustomtext4, ajcustomtext5, ajcustomint1, ajcustomint2, ajcustomint3, ajcustomdbl1, ajcustomdbl2, ajcustomdbl3, ajcustomdate1, ajcustomdate2, ajcustomdate3, ajcabangnama, ajlokasinama, ajkontakkode, ajkontaknama, ajstatusnama, ajstatussebelumnyanama, ajinputusernama, ajmodifikasiusernama" & sptSubParam & "idajdetail, idaj, norek, matauang, kurs, debit, debitvalas, kredit, kreditvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, noreknama, costcenternama, divisinama, subdivisinama, proyeknama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_AjSearch(ByVal param As String) As String
        'M2_AjSearch --------------------------------------------------------
        'ajid, ajcabang, ajlokasi, ajsumber, ajautonotransaksi, ajnotransaksi, ajtgl, 
        'ajkodepa, ajkontak, ajkontakperson, ajuraian, ajcatatan, ajmatauang, ajkurs, 
        'ajdebit, ajdebitvalas, ajkredit, ajkreditvalas, ajjumlahbayar, ajjumlahbayarvalas, ajstatusbayar, 
        'ajtgllunas, ajstatus, ajstatussebelumnya, ajjmlrevisi, ajcetakanke, ajisclose, ajinputuser, 
        'ajinputtgl, ajmodifikasiuser, ajmodifikasitgl, ajposting, ajpostingtgl, ajcabangnama, ajlokasinama, 
        'ajkontakkode, ajkontaknama, ajstatusnama, ajstatussebelumnyanama, ajinputusernama, ajmodifikasiusernama

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
            Filter = Filter.Replace("ajkontakkode", "c1.kkode")
            Filter = Filter.Replace("ajkontaknama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_aj_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Aj", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("ajid"), 0), sptField,
                     FxDB(dr("ajcabang"), ""), sptField,
                     FxDB(dr("ajlokasi"), ""), sptField,
                     FxDB(dr("ajsumber"), ""), sptField,
                     FxDB(dr("ajautonotransaksi"), 0), sptField,
                     FxDB(dr("ajnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ajtgl"), ""), formatTgl), sptField,
                     FxDB(dr("ajkodepa"), 0), sptField,
                     FxDB(dr("ajkontak"), 0), sptField,
                     FxDB(dr("ajkontakperson"), ""), sptField,
                     FxDB(dr("ajuraian"), ""), sptField,
                     FxDB(dr("ajcatatan"), ""), sptField,
                     FxDB(dr("ajmatauang"), ""), sptField,
                     FxDB(dr("ajkurs"), 0), sptField,
                     FxDB(dr("ajdebit"), 0), sptField,
                     FxDB(dr("ajdebitvalas"), 0), sptField,
                     FxDB(dr("ajkredit"), 0), sptField,
                     FxDB(dr("ajkreditvalas"), 0), sptField,
                     FxDB(dr("ajjumlahbayar"), 0), sptField,
                     FxDB(dr("ajjumlahbayarvalas"), 0), sptField,
                     FxDB(dr("ajstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ajtgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("ajstatus"), 0), sptField,
                     FxDB(dr("ajstatussebelumnya"), 0), sptField,
                     FxDB(dr("ajjmlrevisi"), 0), sptField,
                     FxDB(dr("ajcetakanke"), 0), sptField,
                     FxDB(dr("ajisclose"), 0), sptField,
                     FxDB(dr("ajinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ajinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ajmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ajmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ajposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ajpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ajcabangnama"), ""), sptField,
                     FxDB(dr("ajlokasinama"), ""), sptField,
                     FxDB(dr("ajkontakkode"), ""), sptField,
                     FxDB(dr("ajkontaknama"), ""), sptField,
                     FxDB(dr("ajstatusnama"), ""), sptField,
                     FxDB(dr("ajstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("ajinputusernama"), ""), sptField,
                     FxDB(dr("ajmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ajid, ajcabang, ajlokasi, ajsumber, ajautonotransaksi, ajnotransaksi, ajtgl, ajkodepa, ajkontak, ajkontakperson, ajuraian, ajcatatan, ajmatauang, ajkurs, ajdebit, ajdebitvalas, ajkredit, ajkreditvalas, ajjumlahbayar, ajjumlahbayarvalas, ajstatusbayar, ajtgllunas, ajstatus, ajstatussebelumnya, ajjmlrevisi, ajcetakanke, ajisclose, ajinputuser, ajinputtgl, ajmodifikasiuser, ajmodifikasitgl, ajposting, ajpostingtgl, ajcabangnama, ajlokasinama, ajkontakkode, ajkontaknama, ajstatusnama, ajstatussebelumnyanama, ajinputusernama, ajmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_AjTerkait(ByVal param As String) As String
        'M2_AjTerkait --------------------------------------------------------
        'ajid, ajnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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

        ''PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m2_rm_terkait")
        'sql = sql.Replace("validtransaksi", idtransaksi)

        ''BUKA KONEKSI
        'Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        'Con1.Open()

        'dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        'pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("ajid"), 0), sptField,
                     FxDB(dr("ajnotransaksi"), ""), sptField,
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
            result(2) = "Related AJ data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ajid, ajnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_AjSimpanOld(ByVal param As String) As String
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
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'ajid(0) As Integer, ajcabang(1) As String, ajlokasi(2) As String, ajsumber(3) As String, ajautonotransaksi(4) As Integer, 
        'ajnotransaksi(5) As String, ajtgl(6) As Date, ajkodepa(7) As Integer, ajkontak(8) As Integer, ajkontakperson(9) As String, 
        'ajuraian(10) As String, ajcatatan(11) As String, ajmatauang(12) As String, ajkurs(13) As Double, ajdebit(14) As Double, 
        'ajdebitvalas(15) As Double, ajkredit(16) As Double, ajkreditvalas(17) As Double, ajjumlahbayar(18) As Double, ajjumlahbayarvalas(19) As Double, 
        'ajstatusbayar(20) As Integer, ajtgllunas(21) As Date, ajstatus(22) As Integer, ajstatussebelumnya(23) As Integer, ajjmlrevisi(24) As Integer, 
        'ajcetakanke(25) As Integer, ajisclose(26) As Integer, ajinputuser(27) As Integer, ajinputtgl(28) As DateTime, ajmodifikasiuser(29) As Integer, 
        'ajmodifikasitgl(30) As DateTime, ajposting(31) As Integer, ajcustomtext1(32) As String, ajcustomtext2(33) As String, ajcustomtext3(34) As String, 
        'ajcustomtext4(35) As String, ajcustomtext5(36) As String, ajcustomint1(37) As Integer, ajcustomint2(38) As Integer, ajcustomint3(39) As Integer, 
        'ajcustomdbl1(40) As Double, ajcustomdbl2(41) As Double, ajcustomdbl3(42) As Double, ajcustomdate1(43) As Date, ajcustomdate2(44) As Date, 
        'ajcustomdate3(45) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'ajid, ajcabang, ajlokasi, ajsumber, ajautonotransaksi, ajnotransaksi, ajtgl, 
        'ajkodepa, ajkontak, ajkontakperson, ajuraian, ajcatatan, ajmatauang, ajkurs, 
        'ajdebit, ajdebitvalas, ajkredit, ajkreditvalas, ajjumlahbayar, ajjumlahbayarvalas, ajstatusbayar, 
        'ajtgllunas, ajstatus, ajstatussebelumnya, ajjmlrevisi, ajcetakanke, ajisclose, ajinputuser, 
        'ajinputtgl, ajmodifikasiuser, ajmodifikasitgl, ajposting, ajcustomtext1, ajcustomtext2, ajcustomtext3, 
        'ajcustomtext4, ajcustomtext5, ajcustomint1, ajcustomint2, ajcustomint3, ajcustomdbl1, ajcustomdbl2, 
        'ajcustomdbl3, ajcustomdate1, ajcustomdate2, ajcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 46) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'ajid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "ajid required numeric." : GoTo selesai
        End If
        'ajautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "ajautonotransaksi required numeric." : GoTo selesai
        End If
        'ajtgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "ajtgl required date." : GoTo selesai
        End If
        'ajkodepa(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "ajkodepa required numeric." : GoTo selesai
        End If
        'ajkontak(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "ajkontak required numeric." : GoTo selesai
        End If
        If (dataUtama(8) < 1) Then
            result(2) = "ajkontak can't be empty." : GoTo selesai
        End If
        'ajkurs(13) As Double
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "ajkurs required numeric." : GoTo selesai
        End If
        'ajdebit(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "ajdebit required numeric." : GoTo selesai
        End If
        'ajdebitvalas(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "ajdebitvalas required numeric." : GoTo selesai
        End If
        'ajkredit(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "ajkredit required numeric." : GoTo selesai
        End If
        'ajkreditvalas(17) As Double
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "ajkreditvalas required numeric." : GoTo selesai
        End If
        'ajjumlahbayar(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "ajjumlahbayar required numeric." : GoTo selesai
        End If
        'ajjumlahbayarvalas(19) As Double
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "ajjumlahbayarvalas required numeric." : GoTo selesai
        End If
        'ajstatusbayar(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "ajstatusbayar required numeric." : GoTo selesai
        End If
        'ajtgllunas(21) As Date
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "ajtgllunas required date." : GoTo selesai
        End If
        'ajstatus(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "ajstatus required numeric." : GoTo selesai
        End If
        'ajstatussebelumnya(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "ajstatussebelumnya required numeric." : GoTo selesai
        End If
        'ajjmlrevisi(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "ajjmlrevisi required numeric." : GoTo selesai
        End If
        'ajcetakanke(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "ajcetakanke required numeric." : GoTo selesai
        End If
        'ajisclose(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "ajisclose required numeric." : GoTo selesai
        End If
        'ajinputuser(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "ajinputuser required numeric." : GoTo selesai
        End If
        'ajinputtgl(28) As DateTime
        If (IsDate(dataUtama(28)) = False) Then
            result(2) = "ajinputtgl required date." : GoTo selesai
        End If
        'ajmodifikasiuser(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "ajmodifikasiuser required numeric." : GoTo selesai
        End If
        'ajmodifikasitgl(30) As DateTime
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "ajmodifikasitgl required date." : GoTo selesai
        End If
        'ajposting(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "ajposting required numeric." : GoTo selesai
        End If
        'ajcustomint1(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "ajcustomint1 required numeric." : GoTo selesai
        End If
        'ajcustomint2(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "ajcustomint2 required numeric." : GoTo selesai
        End If
        'ajcustomint3(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "ajcustomint3 required numeric." : GoTo selesai
        End If
        'ajcustomdbl1(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "ajcustomdbl1 required numeric." : GoTo selesai
        End If
        'ajcustomdbl2(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "ajcustomdbl2 required numeric." : GoTo selesai
        End If
        'ajcustomdbl3(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "ajcustomdbl3 required numeric." : GoTo selesai
        End If
        'ajcustomdate1(43) As Date
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "ajcustomdate1 required date." : GoTo selesai
        End If
        'ajcustomdate2(44) As Date
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "ajcustomdate2 required date." : GoTo selesai
        End If
        'ajcustomdate3(45) As Date
        If (IsDate(dataUtama(45)) = False) Then
            result(2) = "ajcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'ajcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "ajcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "ajcabang should not be more than 25 character." : GoTo selesai
        End If

        'ajlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "ajlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "ajlokasi should not be more than 25 character." : GoTo selesai
        End If

        'ajsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "ajsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "ajsumber should not be more than 10 character." : GoTo selesai
        End If

        'ajnotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "ajnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "ajnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'ajtgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "ajtgl can't be empty" : GoTo selesai
        End If

        'ajmatauang(12) As String
        If Len(dataUtama(12)) = 0 Then
            result(2) = "ajmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(12)) > 25 Then
            result(2) = "ajmatauang should not be more than 25 character." : GoTo selesai
        End If

        'ajkurs(13) As Double
        If Len(dataUtama(13)) = 0 Then
            result(2) = "ajkurs can't be empty" : GoTo selesai
        End If

        'ajdebit(14) As Double
        If Len(dataUtama(14)) = 0 Then
            result(2) = "ajdebit can't be empty" : GoTo selesai
        End If

        'ajdebitvalas(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "ajdebitvalas can't be empty" : GoTo selesai
        End If

        'ajkredit(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "ajkredit can't be empty" : GoTo selesai
        End If

        'ajkreditvalas(17) As Double
        If Len(dataUtama(17)) = 0 Then
            result(2) = "ajkreditvalas can't be empty" : GoTo selesai
        End If

        'ajjumlahbayar(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "ajjumlahbayar can't be empty" : GoTo selesai
        End If

        'ajjumlahbayarvalas(19) As Double
        If Len(dataUtama(19)) = 0 Then
            result(2) = "ajjumlahbayarvalas can't be empty" : GoTo selesai
        End If

        'ajinputtgl(28) As DateTime
        If Len(dataUtama(28)) = 0 Then
            result(2) = "ajinputtgl can't be empty" : GoTo selesai
        End If

        'ajmodifikasitgl(30) As DateTime
        If Len(dataUtama(30)) = 0 Then
            result(2) = "ajmodifikasitgl can't be empty" : GoTo selesai
        End If

        'ajcustomdbl1(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "ajcustomdbl1 can't be empty" : GoTo selesai
        End If

        'ajcustomdbl2(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "ajcustomdbl2 can't be empty" : GoTo selesai
        End If

        'ajcustomdbl3(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "ajcustomdbl3 can't be empty" : GoTo selesai
        End If

        'ajcustomdate1(43) As Date
        If Len(dataUtama(43)) = 0 Then
            result(2) = "ajcustomdate1 can't be empty" : GoTo selesai
        End If

        'ajcustomdate2(44) As Date
        If Len(dataUtama(44)) = 0 Then
            result(2) = "ajcustomdate2 can't be empty" : GoTo selesai
        End If

        'ajcustomdate3(45) As Date
        If Len(dataUtama(45)) = 0 Then
            result(2) = "ajcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "ajid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajdebit", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "ajdebitvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "ajkredit", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "ajkreditvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "ajjumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajjumlahbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajstatusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajtgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ajcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ajcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "ajid~ajcabang~ajlokasi~ajsumber~ajautonotransaksi~ajnotransaksi~ajtgl~ajkodepa~ajkontak~ajkontakperson~ajuraian~ajcatatan~ajmatauang~ajkurs~ajdebit~ajdebitvalas~ajkredit~ajkreditvalas~ajjumlahbayar~ajjumlahbayarvalas~ajstatusbayar~ajtgllunas~ajstatus~ajstatussebelumnya~ajjmlrevisi~ajcetakanke~ajisclose~ajinputuser~ajinputtgl~ajmodifikasiuser~ajmodifikasitgl~ajposting~ajcustomtext1~ajcustomtext2~ajcustomtext3~ajcustomtext4~ajcustomtext5~ajcustomint1~ajcustomint2~ajcustomint3~ajcustomdbl1~ajcustomdbl2~ajcustomdbl3~ajcustomdate1~ajcustomdate2~ajcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idajdetail(0) As Integer, idaj(1) As Integer, norek(2) As String, matauang(3) As String, kurs(4) As Double, 
        'debit(5) As Double, debitvalas(6) As Double, kredit(7) As Double, kreditvalas(8) As Double, catatan(9) As String, 
        'costcenter(10) As String, divisi(11) As String, subdivisi(12) As String, proyek(13) As String, urutan(14) As Integer, 
        'isclose(15) As Integer, customtext1(16) As String, customtext2(17) As String, customtext3(18) As String, customdbl1(19) As Double, 
        'customdbl2(20) As Double, customdbl3(21) As Double, customdate1(22) As Date, customdate2(23) As Date, customdate3(24) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idajdetail, idaj, norek, matauang, kurs, debit, debitvalas, 
        'kredit, kreditvalas, catatan, costcenter, divisi, subdivisi, proyek, 
        'urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idajdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idaj", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "norek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "debit", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "debitvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "kredit", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "kreditvalas", AsEnumTypeData.AsDouble)
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
            If (dataRowDetail.Length <> 25) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idajdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idajdetail required numeric." : GoTo selesai
            End If
            'idaj(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idaj required numeric." : GoTo selesai
            End If
            'kurs(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'debit(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - debit required numeric." : GoTo selesai
            End If
            'debitvalas(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - debitvalas required numeric." : GoTo selesai
            End If
            'kredit(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - kredit required numeric." : GoTo selesai
            End If
            'kreditvalas(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - kreditvalas required numeric." : GoTo selesai
            End If
            'urutan(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'isclose(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(20) As Double
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(21) As Double
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(23) As Date
            If (IsDate(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(24) As Date
            If (IsDate(dataRowDetail(24)) = False) Then
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

            'debit(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - debit can't be empty" : GoTo selesai
            End If

            'debitvalas(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - debitvalas can't be empty" : GoTo selesai
            End If

            'kredit(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - kredit can't be empty" : GoTo selesai
            End If

            'kreditvalas(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - kreditvalas can't be empty" : GoTo selesai
            End If

            'validasi jumlah debit dan kredit tidak boleh diisi keduanya
            If dataRowDetail(5) = 0 And dataRowDetail(7) = 0 Then
                result(2) = "Row : " & i & " - debits and credits can't be zero" : GoTo selesai
            End If
            If dataRowDetail(5) <> 0 And dataRowDetail(7) <> 0 Then
                result(2) = "Row : " & i & " - debits and credits can't be filled in both" : GoTo selesai
            End If
            If dataRowDetail(6) <> 0 And dataRowDetail(8) <> 0 Then
                result(2) = "Row : " & i & " - foreign debits and credits can't be filled in both" : GoTo selesai
            End If

            'customdbl1(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(23) As Date
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(24) As Date
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idajdetail~idaj~norek~matauang~kurs~debit~debitvalas~kredit~kreditvalas~catatan~costcenter~divisi~subdivisi~proyek~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            strRekCostCenter = IIf(Len(strRekCostCenter.ToString) = 0, "", strRekCostCenter & " OR ")
            strRekCostCenter = String.Concat(strRekCostCenter, "(cnomor = '" & dataRowDetail(2) & "')")

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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("ajtgl")), AsFormatTanggal(drutama("ajtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "ajmatauang", "", dtdetail, "norek")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                'CEK COA WAJIB COST CENTER ==============================
                If drutama("ajstatus") = 2 Then
                    Dim cekCoaCostCenter As String = ValidasiCoaRequiredCostCenter(strRekCostCenter, dtdetail)
                    If Len(cekCoaCostCenter) > 0 Then
                        result(2) = cekCoaCostCenter : Trans.Rollback() : GoTo selesai
                    End If
                End If
                'END OF CEK COA WAJIB COST CENTER =======================


                'VALIDASI NOMINAL HARUS SEIMBANG ========================
                Dim debit As Double = 0, kredit As Double = 0, debitvalas As Double = 0, kreditvalas As Double = 0
                debit = AsDataTableDSum(dtdetail, "debit")
                debitvalas = AsDataTableDSum(dtdetail, "debitvalas")
                kredit = AsDataTableDSum(dtdetail, "kredit")
                kreditvalas = AsDataTableDSum(dtdetail, "kreditvalas")

                ''AMBIL SETTING FORMAT NOMINAL
                'Dim digitGroup As String = "", pemisahDesimal As String = "", digitDesimal As Integer = 0
                'Dim FNominal As String = GetSettingNominal(digitGroup, pemisahDesimal, digitDesimal)
                'If len(FNominal) <> 0 Then result(2) = FNominal : Trans.Rollback() : GoTo selesai

                ''BULATKAN NOMINAL DETAIL SESUAI SETTING FORMAT NOMINAL
                'debit = math.round(debit, digitDesimal)
                'debitvalas = math.round(debitvalas, digitDesimal)
                'kredit = math.round(kredit, digitDesimal)
                'kreditvalas = math.round(kreditvalas, digitDesimal)

                'VALIDASI NOMINAL HARUS SEIMBANG
                If debit <> kredit Then
                    result(2) = "Total debits and credits in detail are not balanced." : GoTo selesai
                End If
                If debitvalas <> kreditvalas Then
                    result(2) = "Total foreign debits and credits in detail are not balanced." : GoTo selesai
                End If

                'HITUNG TOTAL TOTAL BERDASARKAN DATA DETAIL
                drutama("ajdebit") = debit
                drutama("ajdebitvalas") = debitvalas
                drutama("ajkredit") = kredit
                drutama("ajkreditvalas") = kreditvalas
                'END OF VALIDASI NOMINAL HARUS SEIMBANG =================


                If isUpdate Then
                    result(4) = drutama("ajid")
                    notransaksi = drutama("ajnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(ajid), ajnotransaksi FROM M2_aj WHERE ajid='" & result(4) & "' AND ajstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(ajid) FROM m2_aj WHERE ajnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m2_aj_history
                        Dim rsSimpanHistory As String = SimpanHistory.M2_Aj_HistorySimpan("" & paramSplit(0) & "★M2_Aj_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("ajsumber")) & "▼" & FixQuotes(drutama("ajid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M2_Aj set ajcabang  = '" & FixQuotes(drutama("ajcabang")) & "', ajlokasi  = '" & FixQuotes(drutama("ajlokasi")) & "', ajsumber  = '" & FixQuotes(drutama("ajsumber")) & "', ajautonotransaksi  = " & drutama("ajautonotransaksi") & ", ajnotransaksi  = '" & notransaksi & "', ajtgl  = '" & FixQuotes(AsFormatTanggal(drutama("ajtgl"))) & "', ajkodepa  = " & drutama("ajkodepa") & ", ajkontak  = " & drutama("ajkontak") & ", ajkontakperson  = '" & FixQuotes(drutama("ajkontakperson")) & "', ajuraian  = '" & FixQuotes(drutama("ajuraian")) & "', ajcatatan  = '" & FixQuotes(drutama("ajcatatan")) & "', ajmatauang  = '" & FixQuotes(drutama("ajmatauang")) & "', ajkurs  = '" & FixDouble(drutama("ajkurs")) & "', ajdebit  = '" & FixDouble(drutama("ajdebit")) & "', ajdebitvalas  = '" & FixDouble(drutama("ajdebitvalas")) & "', ajkredit  = '" & FixDouble(drutama("ajkredit")) & "', ajkreditvalas  = '" & FixDouble(drutama("ajkreditvalas")) & "', ajjumlahbayar  = '" & FixDouble(drutama("ajjumlahbayar")) & "', ajjumlahbayarvalas  = '" & FixDouble(drutama("ajjumlahbayarvalas")) & "', ajstatusbayar  = " & drutama("ajstatusbayar") & ", ajtgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("ajtgllunas"))) & "', ajstatus  = " & drutama("ajstatus") & ", ajstatussebelumnya  = " & drutama("ajstatussebelumnya") & ", ajjmlrevisi  = ajjmlrevisi + 1, ajcetakanke  = " & drutama("ajcetakanke") & ", ajisclose  = " & drutama("ajisclose") & ", ajmodifikasiuser  = " & drutama("ajmodifikasiuser") & ", ajmodifikasitgl  = NOW(), ajposting  = 0, ajcustomtext1  = '" & FixQuotes(drutama("ajcustomtext1")) & "', ajcustomtext2  = '" & FixQuotes(drutama("ajcustomtext2")) & "', ajcustomtext3  = '" & FixQuotes(drutama("ajcustomtext3")) & "', ajcustomtext4  = '" & FixQuotes(drutama("ajcustomtext4")) & "', ajcustomtext5  = '" & FixQuotes(drutama("ajcustomtext5")) & "', ajcustomint1  = " & drutama("ajcustomint1") & ", ajcustomint2  = " & drutama("ajcustomint2") & ", ajcustomint3  = " & drutama("ajcustomint3") & ", ajcustomdbl1  = '" & FixDouble(drutama("ajcustomdbl1")) & "', ajcustomdbl2  = '" & FixDouble(drutama("ajcustomdbl2")) & "', ajcustomdbl3  = '" & FixDouble(drutama("ajcustomdbl3")) & "', ajcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("ajcustomdate1"))) & "', ajcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("ajcustomdate2"))) & "', ajcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("ajcustomdate3"))) & "' where ajid = '" & drutama("ajid") & "'"
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

                    If drutama("ajautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("ajcabang"), drutama("ajlokasi"), drutama("ajsumber"), drutama("ajtgl"))
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
                        notransaksi = drutama("ajnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(ajid) FROM m2_aj WHERE ajnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M2_Aj (ajcabang, ajlokasi, ajsumber, ajautonotransaksi, ajnotransaksi, ajtgl, ajkodepa, ajkontak, ajkontakperson, ajuraian, ajcatatan, ajmatauang, ajkurs, ajdebit, ajdebitvalas, ajkredit, ajkreditvalas, ajjumlahbayar, ajjumlahbayarvalas, ajstatusbayar, ajtgllunas, ajstatus, ajstatussebelumnya, ajjmlrevisi, ajcetakanke, ajisclose, ajinputuser, ajinputtgl, ajmodifikasiuser, ajmodifikasitgl, ajposting, ajcustomtext1, ajcustomtext2, ajcustomtext3, ajcustomtext4, ajcustomtext5, ajcustomint1, ajcustomint2, ajcustomint3, ajcustomdbl1, ajcustomdbl2, ajcustomdbl3, ajcustomdate1, ajcustomdate2, ajcustomdate3) values('" & FixQuotes(drutama("ajcabang")) & "', '" & FixQuotes(drutama("ajlokasi")) & "', '" & FixQuotes(drutama("ajsumber")) & "', " & drutama("ajautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("ajtgl"))) & "', " & drutama("ajkodepa") & ", " & drutama("ajkontak") & ", '" & FixQuotes(drutama("ajkontakperson")) & "', '" & FixQuotes(drutama("ajuraian")) & "', '" & FixQuotes(drutama("ajcatatan")) & "', '" & FixQuotes(drutama("ajmatauang")) & "', '" & FixDouble(drutama("ajkurs")) & "', '" & FixDouble(drutama("ajdebit")) & "', '" & FixDouble(drutama("ajdebitvalas")) & "', '" & FixDouble(drutama("ajkredit")) & "', '" & FixDouble(drutama("ajkreditvalas")) & "', '" & FixDouble(drutama("ajjumlahbayar")) & "', '" & FixDouble(drutama("ajjumlahbayarvalas")) & "', " & drutama("ajstatusbayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("ajtgllunas"))) & "', " & drutama("ajstatus") & ", " & drutama("ajstatussebelumnya") & ", " & drutama("ajjmlrevisi") & ", " & drutama("ajcetakanke") & ", " & drutama("ajisclose") & ", " & drutama("ajinputuser") & ", NOW(), " & drutama("ajmodifikasiuser") & ", '1971-01-01 00:00:00', 0, '" & FixQuotes(drutama("ajcustomtext1")) & "', '" & FixQuotes(drutama("ajcustomtext2")) & "', '" & FixQuotes(drutama("ajcustomtext3")) & "', '" & FixQuotes(drutama("ajcustomtext4")) & "', '" & FixQuotes(drutama("ajcustomtext5")) & "', " & drutama("ajcustomint1") & ", " & drutama("ajcustomint2") & ", " & drutama("ajcustomint3") & ", '" & FixDouble(drutama("ajcustomdbl1")) & "', '" & FixDouble(drutama("ajcustomdbl2")) & "', '" & FixDouble(drutama("ajcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ajcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ajcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ajcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select ajid from M2_aj where ajnotransaksi='" & notransaksi & "' AND ajinputuser= '" & userid & "' order by ajmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M2_Aj_Detail where idaj = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idajdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("norek")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("debit")) & "', '" & FixDouble(dr1("debitvalas")) & "', '" & FixDouble(dr1("kredit")) & "', '" & FixDouble(dr1("kreditvalas")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M2_Aj_Detail(idajdetail, idaj, norek, matauang, kurs, debit, debitvalas, kredit, kreditvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                Dim sumber As String = "AJ", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("ajstatus") = 2 Then
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
    Public Function M2_AjUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("ajkontakkode", "c1.kkode")
            Filter = Filter.Replace("ajkontaknama", "c1.knama")
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
            Dim sumber As String = "Aj", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Ajtgl, Ajnotransaksi, Ajstatus FROM m2_Aj WHERE Ajid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Ajstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m2_aj_history
            Dim rsSimpanHistory As String = SimpanHistory.M2_Aj_HistorySimpan("" & paramSplit(0) & "★M2_Aj_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then
                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'Aj' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M2_Aj SET Ajstatus = " & nilaiStatus & ", Ajmodifikasiuser='" & userid & "', Ajmodifikasitgl = NOW(), Ajposting = 0, Ajpostingtgl = '1971-01-01 00:00:00', Ajjmlrevisi = Ajjmlrevisi + 1 WHERE Ajid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_AjSearch(PostWsSearch(paramSplit(0), "M2_AjSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_AjDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("ajkontakkode", "c1.kkode")
            Filter = Filter.Replace("ajkontaknama", "c1.knama")
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
            Dim sumber As String = "Aj", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Ajid, Ajnotransaksi FROM m2_Aj WHERE Ajid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT ajcabang, ajlokasi, ajsumber, ajautonotransaksi, ajnotransaksi, ajtgl"
            sql &= " FROM M2_Aj"
            sql &= " WHERE Ajid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("ajcabang")
                lokasi = dtNomorNext.Rows(0)("ajlokasi")
                sumber = dtNomorNext.Rows(0)("ajsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("ajautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("ajnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("ajtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE JURNAL
            sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'Aj' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M2_Aj_Detail WHERE idAj = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M2_Aj WHERE Ajid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_AjSearch(PostWsSearch(paramSplit(0), "M2_AjSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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