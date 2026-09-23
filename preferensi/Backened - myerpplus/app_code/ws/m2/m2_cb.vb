Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m2_cb
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M2_CbSimpan(ByVal param As String) As String
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
        'cbid(0) As Integer, cbcabang(1) As String, cblokasi(2) As String, cbsumber(3) As String, cbautonotransaksi(4) As Integer, 
        'cbnotransaksi(5) As String, cbtgl(6) As Date, cbkodepa(7) As Integer, cbkontak(8) As Integer, cbkontakperson(9) As String, 
        'cburaian(10) As String, cbcatatan(11) As String, cbmatauang(12) As String, cbkurs(13) As Double, cbdebit(14) As Double, 
        'cbdebitvalas(15) As Double, cbkredit(16) As Double, cbkreditvalas(17) As Double, cbjumlahbayar(18) As Double, cbjumlahbayarvalas(19) As Double, 
        'cbstatusbayar(20) As Integer, cbtgllunas(21) As Date, cbstatus(22) As Integer, cbstatussebelumnya(23) As Integer, cbjmlrevisi(24) As Integer, 
        'cbcetakanke(25) As Integer, cbisclose(26) As Integer, cbinputuser(27) As Integer, cbinputtgl(28) As DateTime, cbmodifikasiuser(29) As Integer, 
        'cbmodifikasitgl(30) As DateTime, cbposting(31) As Integer, cbcustomtext1(32) As String, cbcustomtext2(33) As String, cbcustomtext3(34) As String, 
        'cbcustomtext4(35) As String, cbcustomtext5(36) As String, cbcustomint1(37) As Integer, cbcustomint2(38) As Integer, cbcustomint3(39) As Integer, 
        'cbcustomdbl1(40) As Double, cbcustomdbl2(41) As Double, cbcustomdbl3(42) As Double, cbcustomdate1(43) As Date, cbcustomdate2(44) As Date, 
        'cbcustomdate3(45) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'cbid, cbcabang, cblokasi, cbsumber, cbautonotransaksi, cbnotransaksi, cbtgl, 
        'cbkodepa, cbkontak, cbkontakperson, cburaian, cbcatatan, cbmatauang, cbkurs, 
        'cbdebit, cbdebitvalas, cbkredit, cbkreditvalas, cbjumlahbayar, cbjumlahbayarvalas, cbstatusbayar, 
        'cbtgllunas, cbstatus, cbstatussebelumnya, cbjmlrevisi, cbcetakanke, cbisclose, cbinputuser, 
        'cbinputtgl, cbmodifikasiuser, cbmodifikasitgl, cbposting, cbcustomtext1, cbcustomtext2, cbcustomtext3, 
        'cbcustomtext4, cbcustomtext5, cbcustomint1, cbcustomint2, cbcustomint3, cbcustomdbl1, cbcustomdbl2, 
        'cbcustomdbl3, cbcustomdate1, cbcustomdate2, cbcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 46) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'cbid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "cbid required numeric." : GoTo selesai
        End If
        'cbautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "cbautonotransaksi required numeric." : GoTo selesai
        End If
        'cbtgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "cbtgl required date." : GoTo selesai
        End If
        'cbkodepa(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "cbkodepa required numeric." : GoTo selesai
        End If
        'cbkontak(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "cbkontak required numeric." : GoTo selesai
        End If
        If (dataUtama(8) < 1) Then
            result(2) = "cbkontak can't be empty." : GoTo selesai
        End If
        'cbkurs(13) As Double
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "cbkurs required numeric." : GoTo selesai
        End If
        'cbdebit(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "cbdebit required numeric." : GoTo selesai
        End If
        'cbdebitvalas(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "cbdebitvalas required numeric." : GoTo selesai
        End If
        'cbkredit(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "cbkredit required numeric." : GoTo selesai
        End If
        'cbkreditvalas(17) As Double
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "cbkreditvalas required numeric." : GoTo selesai
        End If
        'cbjumlahbayar(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "cbjumlahbayar required numeric." : GoTo selesai
        End If
        'cbjumlahbayarvalas(19) As Double
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "cbjumlahbayarvalas required numeric." : GoTo selesai
        End If
        'cbstatusbayar(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "cbstatusbayar required numeric." : GoTo selesai
        End If
        'cbtgllunas(21) As Date
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "cbtgllunas required date." : GoTo selesai
        End If
        'cbstatus(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "cbstatus required numeric." : GoTo selesai
        End If
        'cbstatussebelumnya(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "cbstatussebelumnya required numeric." : GoTo selesai
        End If
        'cbjmlrevisi(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "cbjmlrevisi required numeric." : GoTo selesai
        End If
        'cbcetakanke(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "cbcetakanke required numeric." : GoTo selesai
        End If
        'cbisclose(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "cbisclose required numeric." : GoTo selesai
        End If
        'cbinputuser(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "cbinputuser required numeric." : GoTo selesai
        End If
        'cbinputtgl(28) As DateTime
        If (IsDate(dataUtama(28)) = False) Then
            result(2) = "cbinputtgl required date." : GoTo selesai
        End If
        'cbmodifikasiuser(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "cbmodifikasiuser required numeric." : GoTo selesai
        End If
        'cbmodifikasitgl(30) As DateTime
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "cbmodifikasitgl required date." : GoTo selesai
        End If
        'cbposting(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "cbposting required numeric." : GoTo selesai
        End If
        'cbcustomint1(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "cbcustomint1 required numeric." : GoTo selesai
        End If
        'cbcustomint2(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "cbcustomint2 required numeric." : GoTo selesai
        End If
        'cbcustomint3(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "cbcustomint3 required numeric." : GoTo selesai
        End If
        'cbcustomdbl1(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "cbcustomdbl1 required numeric." : GoTo selesai
        End If
        'cbcustomdbl2(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "cbcustomdbl2 required numeric." : GoTo selesai
        End If
        'cbcustomdbl3(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "cbcustomdbl3 required numeric." : GoTo selesai
        End If
        'cbcustomdate1(43) As Date
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "cbcustomdate1 required date." : GoTo selesai
        End If
        'cbcustomdate2(44) As Date
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "cbcustomdate2 required date." : GoTo selesai
        End If
        'cbcustomdate3(45) As Date
        If (IsDate(dataUtama(45)) = False) Then
            result(2) = "cbcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'cbcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "cbcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "cbcabang should not be more than 25 character." : GoTo selesai
        End If

        'cblokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "cblokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "cblokasi should not be more than 25 character." : GoTo selesai
        End If

        'cbsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "cbsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "cbsumber should not be more than 10 character." : GoTo selesai
        End If

        'cbnotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "cbnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "cbnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'cbtgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "cbtgl can't be empty" : GoTo selesai
        End If

        'cbmatauang(12) As String
        If Len(dataUtama(12)) = 0 Then
            result(2) = "cbmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(12)) > 25 Then
            result(2) = "cbmatauang should not be more than 25 character." : GoTo selesai
        End If

        'cbkurs(13) As Double
        If Len(dataUtama(13)) = 0 Then
            result(2) = "cbkurs can't be empty" : GoTo selesai
        End If

        'cbdebit(14) As Double
        If Len(dataUtama(14)) = 0 Then
            result(2) = "cbdebit can't be empty" : GoTo selesai
        End If

        'cbdebitvalas(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "cbdebitvalas can't be empty" : GoTo selesai
        End If

        'cbkredit(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "cbkredit can't be empty" : GoTo selesai
        End If

        'cbkreditvalas(17) As Double
        If Len(dataUtama(17)) = 0 Then
            result(2) = "cbkreditvalas can't be empty" : GoTo selesai
        End If

        'cbjumlahbayar(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "cbjumlahbayar can't be empty" : GoTo selesai
        End If

        'cbjumlahbayarvalas(19) As Double
        If Len(dataUtama(19)) = 0 Then
            result(2) = "cbjumlahbayarvalas can't be empty" : GoTo selesai
        End If

        'cbinputtgl(28) As DateTime
        If Len(dataUtama(28)) = 0 Then
            result(2) = "cbinputtgl can't be empty" : GoTo selesai
        End If

        'cbmodifikasitgl(30) As DateTime
        If Len(dataUtama(30)) = 0 Then
            result(2) = "cbmodifikasitgl can't be empty" : GoTo selesai
        End If

        'cbcustomdbl1(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "cbcustomdbl1 can't be empty" : GoTo selesai
        End If

        'cbcustomdbl2(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "cbcustomdbl2 can't be empty" : GoTo selesai
        End If

        'cbcustomdbl3(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "cbcustomdbl3 can't be empty" : GoTo selesai
        End If

        'cbcustomdate1(43) As Date
        If Len(dataUtama(43)) = 0 Then
            result(2) = "cbcustomdate1 can't be empty" : GoTo selesai
        End If

        'cbcustomdate2(44) As Date
        If Len(dataUtama(44)) = 0 Then
            result(2) = "cbcustomdate2 can't be empty" : GoTo selesai
        End If

        'cbcustomdate3(45) As Date
        If Len(dataUtama(45)) = 0 Then
            result(2) = "cbcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "cbid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cblokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cburaian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbdebit", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "cbdebitvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "cbkredit", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "cbkreditvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "cbjumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbjumlahbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbstatusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbtgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "cbid~cbcabang~cblokasi~cbsumber~cbautonotransaksi~cbnotransaksi~cbtgl~cbkodepa~cbkontak~cbkontakperson~cburaian~cbcatatan~cbmatauang~cbkurs~cbdebit~cbdebitvalas~cbkredit~cbkreditvalas~cbjumlahbayar~cbjumlahbayarvalas~cbstatusbayar~cbtgllunas~cbstatus~cbstatussebelumnya~cbjmlrevisi~cbcetakanke~cbisclose~cbinputuser~cbinputtgl~cbmodifikasiuser~cbmodifikasitgl~cbposting~cbcustomtext1~cbcustomtext2~cbcustomtext3~cbcustomtext4~cbcustomtext5~cbcustomint1~cbcustomint2~cbcustomint3~cbcustomdbl1~cbcustomdbl2~cbcustomdbl3~cbcustomdate1~cbcustomdate2~cbcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idcbdetail(0) As Integer, idcb(1) As Integer, norek(2) As String, matauang(3) As String, kurs(4) As Double, 
        'debit(5) As Double, debitvalas(6) As Double, kredit(7) As Double, kreditvalas(8) As Double, catatan(9) As String, 
        'costcenter(10) As String, divisi(11) As String, subdivisi(12) As String, proyek(13) As String, urutan(14) As Integer, 
        'isclose(15) As Integer, customtext1(16) As String, customtext2(17) As String, customtext3(18) As String, customdbl1(19) As Double, 
        'customdbl2(20) As Double, customdbl3(21) As Double, customdate1(22) As Date, customdate2(23) As Date, customdate3(24) As Date


        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idcbdetail, idcb, norek, matauang, kurs, debit, debitvalas, 
        'kredit, kreditvalas, catatan, costcenter, divisi, subdivisi, proyek, 
        'urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idcbdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idcb", AsEnumTypeData.AsInt64)
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
            'idcbdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idcbdetail required numeric." : GoTo selesai
            End If
            'idcb(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idcb required numeric." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idcbdetail~idcb~norek~matauang~kurs~debit~debitvalas~kredit~kreditvalas~catatan~costcenter~divisi~subdivisi~proyek~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            strRekCostCenter = IIf(Len(strRekCostCenter.ToString) = 0, "", strRekCostCenter & " OR ")
            strRekCostCenter = String.Concat(strRekCostCenter, "(cnomor = '" & dataRowDetail(2) & "')")

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'MAPPING BUAT WS DATA PAY -------------------------------------------------------
        'idcbcarabayar(0) As Integer, idcb(1) As Integer, jenisgiro(2) As String, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, nogiro(7) As String, tgljt(8) As Date, bank(9) As String, 
        'noacbank(10) As String, rekbank(11) As String, rekgiro(12) As String, catatan(13) As String, urutan(14) As Integer

        'MAPPING BUAT FLEX DATA PAY -----------------------------------------------------
        'idcbcarabayar, idcb, jenisgiro, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan

        'VALIDASI DAN SET DATA PAY ======================================================
        'SPLIT PARAMETER DATA PAY

        'Buat datatable pay
        Dim dtpay As New DataTable
        AsDataTableTambahField(dtpay, "idcbcarabayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "idcb", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpay, "jenisgiro", AsEnumTypeData.AsInt64)
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


        'VALIDASI DAN SET DATA ROW GIRO ==================================================
        If Len(dataSplit(2)) > 0 Then
            'SPLIT PARAMETER DATA GIRO
            dataGiro = dataSplit(2).Split(sptRow)

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
                'idcbcarabayar(0) As Integer
                If (IsNumeric(dataRowGiro(0)) = False) Then
                    result(2) = "Giro Row : " & i & " - idcbcarabayar required numeric." : GoTo selesai
                End If
                'idcb(1) As Integer
                If (IsNumeric(dataRowGiro(1)) = False) Then
                    result(2) = "Giro Row : " & i & " - idcb required numeric." : GoTo selesai
                End If
                'jenisgiro(2) As Integer
                If (IsNumeric(dataRowGiro(2)) = False) Then
                    result(2) = "Giro Row : " & i & " - jenisgiro required numeric." : GoTo selesai
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
                If dataRowGiro(5) < 1 Then
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

                If AsDataTableTambahData(dtpay, "idcbcarabayar~idcb~jenisgiro~matauang~kurs~jumlah~jumlahvalas~nogiro~tgljt~bank~noacbank~rekbank~rekgiro~catatan~urutan", dataRowGiro(0) & "~" & dataRowGiro(1) & "~" & dataRowGiro(2) & "~" & dataRowGiro(3) & "~" & dataRowGiro(4) & "~" & dataRowGiro(5) & "~" & dataRowGiro(6) & "~" & dataRowGiro(7) & "~" & dataRowGiro(8) & "~" & dataRowGiro(9) & "~" & dataRowGiro(10) & "~" & dataRowGiro(11) & "~" & dataRowGiro(12) & "~" & dataRowGiro(13) & "~" & dataRowGiro(14)) = False Then
                    result(2) = "Giro Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
        End If
        'END OF VALIDASI DAN SET ROW DATA GIRO ===========================================


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
                Dim vModuleId As Integer = 2, vMenuId As Integer = 65
                Select Case drutama("cbstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("cbtgl")), AsFormatTanggal(drutama("cbtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "cbmatauang", "", dtdetail, "norek")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                'CEK COA WAJIB COST CENTER ==============================
                If drutama("cbstatus") = 2 Then
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

                ''VALIDASI NOMINAL HARUS SEIMBANG
                'If debit <> kredit Then
                '    result(2) = "Total debits and credits in detail are not balanced." : GoTo selesai
                'End If
                'If debitvalas <> kreditvalas Then
                '    result(2) = "Total foreign debits and credits in detail are not balanced." : GoTo selesai
                'End If

                'HITUNG TOTAL TOTAL BERDASARKAN DATA DETAIL
                drutama("cbdebit") = debit
                drutama("cbdebitvalas") = debitvalas
                drutama("cbkredit") = kredit
                drutama("cbkreditvalas") = kreditvalas
                'END OF VALIDASI NOMINAL HARUS SEIMBANG =================


                If isUpdate Then
                    result(4) = drutama("cbid")
                    notransaksi = drutama("cbnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(cbid), cbnotransaksi FROM M2_cb WHERE cbid='" & result(4) & "' AND cbstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("cbautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("cbcabang"), drutama("cblokasi"), drutama("cbsumber"), drutama("cbtgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(cbid) FROM m2_cb WHERE cbnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m2_cb_history
                        Dim rsSimpanHistory As String = SimpanHistory.M2_Cb_HistorySimpan("" & paramSplit(0) & "★M2_Cb_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("cbsumber")) & "▼" & FixQuotes(drutama("cbid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M2_cb set cbcabang  = '" & FixQuotes(drutama("cbcabang")) & "', cblokasi  = '" & FixQuotes(drutama("cblokasi")) & "', cbsumber  = '" & FixQuotes(drutama("cbsumber")) & "', cbautonotransaksi  = " & drutama("cbautonotransaksi") & ", cbnotransaksi  = '" & notransaksi & "', cbtgl  = '" & FixQuotes(AsFormatTanggal(drutama("cbtgl"))) & "', cbkodepa  = " & drutama("cbkodepa") & ", cbkontak  = " & drutama("cbkontak") & ", cbkontakperson  = '" & FixQuotes(drutama("cbkontakperson")) & "', cburaian  = '" & FixQuotes(drutama("cburaian")) & "', cbcatatan  = '" & FixQuotes(drutama("cbcatatan")) & "', cbmatauang  = '" & FixQuotes(drutama("cbmatauang")) & "', cbkurs  = '" & FixDouble(drutama("cbkurs")) & "', cbdebit  = '" & FixDouble(drutama("cbdebit")) & "', cbdebitvalas  = '" & FixDouble(drutama("cbdebitvalas")) & "', cbkredit  = '" & FixDouble(drutama("cbkredit")) & "', cbkreditvalas  = '" & FixDouble(drutama("cbkreditvalas")) & "', cbjumlahbayar  = '" & FixDouble(drutama("cbjumlahbayar")) & "', cbjumlahbayarvalas  = '" & FixDouble(drutama("cbjumlahbayarvalas")) & "', cbstatusbayar  = " & drutama("cbstatusbayar") & ", cbtgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("cbtgllunas"))) & "', cbstatus  = " & drutama("cbstatus") & ", cbstatussebelumnya  = " & drutama("cbstatussebelumnya") & ", cbjmlrevisi  = cbjmlrevisi+1, cbcetakanke  = " & drutama("cbcetakanke") & ", cbisclose  = " & drutama("cbisclose") & ", cbmodifikasiuser  = " & drutama("cbmodifikasiuser") & ", cbmodifikasitgl  = NOW(), cbposting  = 0, cbcustomtext1  = '" & FixQuotes(drutama("cbcustomtext1")) & "', cbcustomtext2  = '" & FixQuotes(drutama("cbcustomtext2")) & "', cbcustomtext3  = '" & FixQuotes(drutama("cbcustomtext3")) & "', cbcustomtext4  = '" & FixQuotes(drutama("cbcustomtext4")) & "', cbcustomtext5  = '" & FixQuotes(drutama("cbcustomtext5")) & "', cbcustomint1  = " & drutama("cbcustomint1") & ", cbcustomint2  = " & drutama("cbcustomint2") & ", cbcustomint3  = " & drutama("cbcustomint3") & ", cbcustomdbl1  = '" & FixDouble(drutama("cbcustomdbl1")) & "', cbcustomdbl2  = '" & FixDouble(drutama("cbcustomdbl2")) & "', cbcustomdbl3  = '" & FixDouble(drutama("cbcustomdbl3")) & "', cbcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("cbcustomdate1"))) & "', cbcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("cbcustomdate2"))) & "', cbcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("cbcustomdate3"))) & "' where cbid = '" & drutama("cbid") & "'"
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

                    If drutama("cbautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("cbcabang"), drutama("cblokasi"), drutama("cbsumber"), drutama("cbtgl"))
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
                        notransaksi = drutama("cbnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(cbid) FROM m2_cb WHERE cbnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M2_cb (cbcabang, cblokasi, cbsumber, cbautonotransaksi, cbnotransaksi, cbtgl, cbkodepa, cbkontak, cbkontakperson, cburaian, cbcatatan, cbmatauang, cbkurs, cbdebit, cbdebitvalas, cbkredit, cbkreditvalas, cbjumlahbayar, cbjumlahbayarvalas, cbstatusbayar, cbtgllunas, cbstatus, cbstatussebelumnya, cbjmlrevisi, cbcetakanke, cbisclose, cbinputuser, cbinputtgl, cbmodifikasiuser, cbmodifikasitgl, cbposting, cbcustomtext1, cbcustomtext2, cbcustomtext3, cbcustomtext4, cbcustomtext5, cbcustomint1, cbcustomint2, cbcustomint3, cbcustomdbl1, cbcustomdbl2, cbcustomdbl3, cbcustomdate1, cbcustomdate2, cbcustomdate3) values('" & FixQuotes(drutama("cbcabang")) & "', '" & FixQuotes(drutama("cblokasi")) & "', '" & FixQuotes(drutama("cbsumber")) & "', " & drutama("cbautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("cbtgl"))) & "', " & drutama("cbkodepa") & ", " & drutama("cbkontak") & ", '" & FixQuotes(drutama("cbkontakperson")) & "', '" & FixQuotes(drutama("cburaian")) & "', '" & FixQuotes(drutama("cbcatatan")) & "', '" & FixQuotes(drutama("cbmatauang")) & "', '" & FixDouble(drutama("cbkurs")) & "', '" & FixDouble(drutama("cbdebit")) & "', '" & FixDouble(drutama("cbdebitvalas")) & "', '" & FixDouble(drutama("cbkredit")) & "', '" & FixDouble(drutama("cbkreditvalas")) & "', '" & FixDouble(drutama("cbjumlahbayar")) & "', '" & FixDouble(drutama("cbjumlahbayarvalas")) & "', " & drutama("cbstatusbayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("cbtgllunas"))) & "', " & drutama("cbstatus") & ", " & drutama("cbstatussebelumnya") & ", " & drutama("cbjmlrevisi") & ", " & drutama("cbcetakanke") & ", " & drutama("cbisclose") & ", " & drutama("cbinputuser") & ", NOW(), " & drutama("cbmodifikasiuser") & ", '1971-01-01 00:00:00', 0, '" & FixQuotes(drutama("cbcustomtext1")) & "', '" & FixQuotes(drutama("cbcustomtext2")) & "', '" & FixQuotes(drutama("cbcustomtext3")) & "', '" & FixQuotes(drutama("cbcustomtext4")) & "', '" & FixQuotes(drutama("cbcustomtext5")) & "', " & drutama("cbcustomint1") & ", " & drutama("cbcustomint2") & ", " & drutama("cbcustomint3") & ", '" & FixDouble(drutama("cbcustomdbl1")) & "', '" & FixDouble(drutama("cbcustomdbl2")) & "', '" & FixDouble(drutama("cbcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("cbcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("cbcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("cbcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select cbid from M2_cb where cbnotransaksi='" & notransaksi & "' AND cbinputuser= '" & userid & "' order by cbmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M2_cb_Detail where idcb = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idcbdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("norek")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("debit")) & "', '" & FixDouble(dr1("debitvalas")) & "', '" & FixDouble(dr1("kredit")) & "', '" & FixDouble(dr1("kreditvalas")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M2_cb_Detail(idcbdetail, idcb, norek, matauang, kurs, debit, debitvalas, kredit, kreditvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                    sql = "Delete from M2_Cb_Pay where idcb = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idcbcarabayar") & ", " & result(4) & ", " & dr1("jenisgiro") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ")")
                        'query untuk insert giro
                        If drutama("cbstatus") = 2 Then
                            strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", ", "))
                            strGiro.Append("('" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(drutama("cbsumber")) & "', " & result(4) & ", '" & FixQuotes(notransaksi) & "', " & drutama("cbkontak") & ", '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', " & dr1("jenisgiro") & ", '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', " & 0 & ", " & 0 & ", " & dr1("urutan") & ")")
                        End If
                    Next
                    sql = "Insert into M2_Cb_Pay(idcbcarabayar, idcb, jenisgiro, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'insert giro jika status approved
                    If drutama("cbstatus") = 2 And Len(strGiro.ToString) > 0 Then
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
                Dim sumber As String = "CB", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("cbstatus") = 2 Then
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
    Public Function M2_CbUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("cbkontakkode", "c1.kkode")
            Filter = Filter.Replace("cbkontaknama", "c1.knama")
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
            Dim sumber As String = "Cb", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Cbtgl, Cbnotransaksi, Cbstatus FROM m2_Cb WHERE Cbid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Cbstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m2_cb_history
            Dim rsSimpanHistory As String = SimpanHistory.M2_Cb_HistorySimpan("" & paramSplit(0) & "★M2_Cb_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m2_cb_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                ''CEK STATUS GIRO
                'dtdetail = AsDataTableAmbilDariDBCon("SELECT glnogiro FROM m2_giro_list WHERE glsumber = 'Cb' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "' AND glstatus <> 0")
                'If dtdetail.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai

                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'Cb' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'DELETE GIRO
                sql = "DELETE FROM m2_giro_list WHERE glsumber = 'Cb' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M2_Cb SET Cbstatus = " & nilaiStatus & ", Cbmodifikasiuser='" & userid & "', Cbmodifikasitgl = NOW(), Cbposting = 0, Cbpostingtgl = '1971-01-01 00:00:00', Cbjmlrevisi = Cbjmlrevisi + 1 WHERE Cbid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_CbSearch(PostWsSearch(paramSplit(0), "M2_CbSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_CbDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("cbkontakkode", "c1.kkode")
            Filter = Filter.Replace("cbkontaknama", "c1.knama")
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
            Dim sumber As String = "Cb", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Cbid, Cbnotransaksi FROM M2_Cb WHERE Cbid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT cbcabang, cblokasi, cbsumber, cbautonotransaksi, cbnotransaksi, cbtgl"
            sql &= " FROM M2_cb"
            sql &= " WHERE cbid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("cbcabang")
                lokasi = dtNomorNext.Rows(0)("cblokasi")
                sumber = dtNomorNext.Rows(0)("cbsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("cbautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("cbnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("cbtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE JURNAL
            sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'Cb' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE GIRO
            sql = "DELETE FROM m2_giro_list WHERE glsumber = 'Cb' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE PAY
            sql = "DELETE FROM M2_Cb_Pay WHERE idCb = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M2_Cb_Detail WHERE idCb = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M2_Cb WHERE Cbid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_CbSearch(PostWsSearch(paramSplit(0), "M2_CbSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_CbGetdataById(ByVal param As String) As String

        'M2_CbGetdataById Utama --------------------------------------------------------
        'cbid, cbcabang, cblokasi, cbsumber, cbautonotransaksi, cbnotransaksi, cbtgl, 
        'cbkodepa, cbkontak, cbkontakperson, cburaian, cbcatatan, cbmatauang, cbkurs, 
        'cbdebit, cbdebitvalas, cbkredit, cbkreditvalas, cbjumlahbayar, cbjumlahbayarvalas, cbstatusbayar, 
        'cbtgllunas, cbstatus, cbstatussebelumnya, cbjmlrevisi, cbcetakanke, cbisclose, cbinputuser, 
        'cbinputtgl, cbmodifikasiuser, cbmodifikasitgl, cbposting, cbpostingtgl, cbcustomtext1, cbcustomtext2, 
        'cbcustomtext3, cbcustomtext4, cbcustomtext5, cbcustomint1, cbcustomint2, cbcustomint3, cbcustomdbl1, 
        'cbcustomdbl2, cbcustomdbl3, cbcustomdate1, cbcustomdate2, cbcustomdate3, cbcabangnama, cblokasinama, 
        'cbkontakkode, cbkontaknama, cbstatusnama, cbstatussebelumnyanama, cbinputusernama, cbmodifikasiusernama

        'M2_CbGetdataById Detail -------------------------------------------------------
        'idcbdetail, idcb, 
        'norek, matauang, kurs, debit, debitvalas, kredit, kreditvalas, 
        'catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3, noreknama, costcenternama, divisinama, subdivisinama, proyeknama

        'M2_CbGetdataById Pay -------------------------------------------------------
        'idcbcarabayar, idcb, jenisgiro, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, banknama, rekbanknama, rekgironama

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

        Dim NmMemcached As String = "aplikasi1-M2_Cb~M2_Cb_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "cbid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "cbid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_cb_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("cbid"), 0), sptField,
                     FxDB(drutama("cbcabang"), ""), sptField,
                     FxDB(drutama("cblokasi"), ""), sptField,
                     FxDB(drutama("cbsumber"), ""), sptField,
                     FxDB(drutama("cbautonotransaksi"), 0), sptField,
                     FxDB(drutama("cbnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("cbtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("cbkodepa"), 0), sptField,
                     FxDB(drutama("cbkontak"), 0), sptField,
                     FxDB(drutama("cbkontakperson"), ""), sptField,
                     FxDB(drutama("cburaian"), ""), sptField,
                     FxDB(drutama("cbcatatan"), ""), sptField,
                     FxDB(drutama("cbmatauang"), ""), sptField,
                     FxDB(drutama("cbkurs"), 0), sptField,
                     FxDB(drutama("cbdebit"), 0), sptField,
                     FxDB(drutama("cbdebitvalas"), 0), sptField,
                     FxDB(drutama("cbkredit"), 0), sptField,
                     FxDB(drutama("cbkreditvalas"), 0), sptField,
                     FxDB(drutama("cbjumlahbayar"), 0), sptField,
                     FxDB(drutama("cbjumlahbayarvalas"), 0), sptField,
                     FxDB(drutama("cbstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("cbtgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("cbstatus"), 0), sptField,
                     FxDB(drutama("cbstatussebelumnya"), 0), sptField,
                     FxDB(drutama("cbjmlrevisi"), 0), sptField,
                     FxDB(drutama("cbcetakanke"), 0), sptField,
                     FxDB(drutama("cbisclose"), 0), sptField,
                     FxDB(drutama("cbinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("cbinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("cbmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("cbmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("cbposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("cbpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("cbcustomtext1"), ""), sptField,
                     FxDB(drutama("cbcustomtext2"), ""), sptField,
                     FxDB(drutama("cbcustomtext3"), ""), sptField,
                     FxDB(drutama("cbcustomtext4"), ""), sptField,
                     FxDB(drutama("cbcustomtext5"), ""), sptField,
                     FxDB(drutama("cbcustomint1"), 0), sptField,
                     FxDB(drutama("cbcustomint2"), 0), sptField,
                     FxDB(drutama("cbcustomint3"), 0), sptField,
                     FxDB(drutama("cbcustomdbl1"), 0), sptField,
                     FxDB(drutama("cbcustomdbl2"), 0), sptField,
                     FxDB(drutama("cbcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("cbcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("cbcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("cbcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("cbcabangnama"), ""), sptField,
                     FxDB(drutama("cblokasinama"), ""), sptField,
                     FxDB(drutama("cbkontakkode"), ""), sptField,
                     FxDB(drutama("cbkontaknama"), ""), sptField,
                     FxDB(drutama("cbstatusnama"), ""), sptField,
                     FxDB(drutama("cbstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("cbinputusernama"), ""), sptField,
                     FxDB(drutama("cbmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idcbdetail"), 0), sptField,
                     FxDB(dr("idcb"), 0), sptField,
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

            'AMBIL DATA PAY
            'PANGGIL QUERY
            Dim querygiro As New m0_query
            sql = querygiro.PanggilQuery("m2_cb_pay_v")

            Dim dtgiro As New DataTable
            dtgiro = AmbilData("aplikasi1-M2_Giro_List", "cbp.idcb='" & idtransaksi & "'", "cbp.urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtgiro.Rows
                giro = String.Concat(giro,
                     FxDB(dr("idcbcarabayar"), 0), sptField,
                     FxDB(dr("idcb"), 0), sptField,
                     FxDB(dr("jenisgiro"), 0), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("cbid, cbcabang, cblokasi, cbsumber, cbautonotransaksi, cbnotransaksi, cbtgl, cbkodepa, cbkontak, cbkontakperson, cburaian, cbcatatan, cbmatauang, cbkurs, cbdebit, cbdebitvalas, cbkredit, cbkreditvalas, cbjumlahbayar, cbjumlahbayarvalas, cbstatusbayar, cbtgllunas, cbstatus, cbstatussebelumnya, cbjmlrevisi, cbcetakanke, cbisclose, cbinputuser, cbinputtgl, cbmodifikasiuser, cbmodifikasitgl, cbposting, cbpostingtgl, cbcustomtext1, cbcustomtext2, cbcustomtext3, cbcustomtext4, cbcustomtext5, cbcustomint1, cbcustomint2, cbcustomint3, cbcustomdbl1, cbcustomdbl2, cbcustomdbl3, cbcustomdate1, cbcustomdate2, cbcustomdate3, cbcabangnama, cblokasinama, cbkontakkode, cbkontaknama, cbstatusnama, cbstatussebelumnyanama, cbinputusernama, cbmodifikasiusernama" & sptSubParam & "idcbdetail, idcb, norek, matauang, kurs, debit, debitvalas, kredit, kreditvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, noreknama, costcenternama, divisinama, subdivisinama, proyeknama" & sptSubParam & "idcbcarabayar, idcb, jenisgiro, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, banknama, rekbanknama, rekgironama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_CbSearch(ByVal param As String) As String
        'M2_CbSearch --------------------------------------------------------
        'cbid, cbcabang, cblokasi, cbsumber, cbautonotransaksi, cbnotransaksi, cbtgl, 
        'cbkodepa, cbkontak, cbkontakperson, cburaian, cbcatatan, cbmatauang, cbkurs, 
        'cbdebit, cbdebitvalas, cbkredit, cbkreditvalas, cbjumlahbayar, cbjumlahbayarvalas, cbstatusbayar, 
        'cbtgllunas, cbstatus, cbstatussebelumnya, cbjmlrevisi, cbcetakanke, cbisclose, cbinputuser, 
        'cbinputtgl, cbmodifikasiuser, cbmodifikasitgl, cbposting, cbpostingtgl, cbcabangnama, cblokasinama, 
        'cbkontakkode, cbkontaknama, cbstatusnama, cbstatussebelumnyanama, cbinputusernama, cbmodifikasiusernama

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
            Filter = Filter.Replace("cbkontakkode", "c1.kkode")
            Filter = Filter.Replace("cbkontaknama", "c1.knama")
            Filter = Filter.Replace("Cbkontaknama", "c1.knama")
            Filter = Filter.Replace("Cbstatusnama", "`st1`.`nama`")
            Filter = Filter.Replace("Cbinputusernama", "`u1`.`unama`")
            Filter = Filter.Replace("Cbmodifikasiusernama", "`u2`.`unama`")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_cb_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Cb", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("cbid"), 0), sptField,
                     FxDB(dr("cbcabang"), ""), sptField,
                     FxDB(dr("cblokasi"), ""), sptField,
                     FxDB(dr("cbsumber"), ""), sptField,
                     FxDB(dr("cbautonotransaksi"), 0), sptField,
                     FxDB(dr("cbnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("cbtgl"), ""), formatTgl), sptField,
                     FxDB(dr("cbkodepa"), 0), sptField,
                     FxDB(dr("cbkontak"), 0), sptField,
                     FxDB(dr("cbkontakperson"), ""), sptField,
                     FxDB(dr("cburaian"), ""), sptField,
                     FxDB(dr("cbcatatan"), ""), sptField,
                     FxDB(dr("cbmatauang"), ""), sptField,
                     FxDB(dr("cbkurs"), 0), sptField,
                     FxDB(dr("cbdebit"), 0), sptField,
                     FxDB(dr("cbdebitvalas"), 0), sptField,
                     FxDB(dr("cbkredit"), 0), sptField,
                     FxDB(dr("cbkreditvalas"), 0), sptField,
                     FxDB(dr("cbjumlahbayar"), 0), sptField,
                     FxDB(dr("cbjumlahbayarvalas"), 0), sptField,
                     FxDB(dr("cbstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("cbtgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("cbstatus"), 0), sptField,
                     FxDB(dr("cbstatussebelumnya"), 0), sptField,
                     FxDB(dr("cbjmlrevisi"), 0), sptField,
                     FxDB(dr("cbcetakanke"), 0), sptField,
                     FxDB(dr("cbisclose"), 0), sptField,
                     FxDB(dr("cbinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("cbinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("cbmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("cbmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("cbposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("cbpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("cbcabangnama"), ""), sptField,
                     FxDB(dr("cblokasinama"), ""), sptField,
                     FxDB(dr("cbkontakkode"), ""), sptField,
                     FxDB(dr("cbkontaknama"), ""), sptField,
                     FxDB(dr("cbstatusnama"), ""), sptField,
                     FxDB(dr("cbstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("cbinputusernama"), ""), sptField,
                     FxDB(dr("cbmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("cbid, cbcabang, cblokasi, cbsumber, cbautonotransaksi, cbnotransaksi, cbtgl, cbkodepa, cbkontak, cbkontakperson, cburaian, cbcatatan, cbmatauang, cbkurs, cbdebit, cbdebitvalas, cbkredit, cbkreditvalas, cbjumlahbayar, cbjumlahbayarvalas, cbstatusbayar, cbtgllunas, cbstatus, cbstatussebelumnya, cbjmlrevisi, cbcetakanke, cbisclose, cbinputuser, cbinputtgl, cbmodifikasiuser, cbmodifikasitgl, cbposting, cbpostingtgl, cbcabangnama, cblokasinama, cbkontakkode, cbkontaknama, cbstatusnama, cbstatussebelumnyanama, cbinputusernama, cbmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_CbTerkait(ByVal param As String) As String
        'M2_CbTerkait --------------------------------------------------------
        'cbid, cbnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "cbid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_cb_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("cbid"), 0), sptField,
                     FxDB(dr("cbnotransaksi"), ""), sptField,
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
            result(2) = "Related CB data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("cbid, cbnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_CbSimpanOld(ByVal param As String) As String
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
        'cbid(0) As Integer, cbcabang(1) As String, cblokasi(2) As String, cbsumber(3) As String, cbautonotransaksi(4) As Integer, 
        'cbnotransaksi(5) As String, cbtgl(6) As Date, cbkodepa(7) As Integer, cbkontak(8) As Integer, cbkontakperson(9) As String, 
        'cburaian(10) As String, cbcatatan(11) As String, cbmatauang(12) As String, cbkurs(13) As Double, cbdebit(14) As Double, 
        'cbdebitvalas(15) As Double, cbkredit(16) As Double, cbkreditvalas(17) As Double, cbjumlahbayar(18) As Double, cbjumlahbayarvalas(19) As Double, 
        'cbstatusbayar(20) As Integer, cbtgllunas(21) As Date, cbstatus(22) As Integer, cbstatussebelumnya(23) As Integer, cbjmlrevisi(24) As Integer, 
        'cbcetakanke(25) As Integer, cbisclose(26) As Integer, cbinputuser(27) As Integer, cbinputtgl(28) As DateTime, cbmodifikasiuser(29) As Integer, 
        'cbmodifikasitgl(30) As DateTime, cbposting(31) As Integer, cbcustomtext1(32) As String, cbcustomtext2(33) As String, cbcustomtext3(34) As String, 
        'cbcustomtext4(35) As String, cbcustomtext5(36) As String, cbcustomint1(37) As Integer, cbcustomint2(38) As Integer, cbcustomint3(39) As Integer, 
        'cbcustomdbl1(40) As Double, cbcustomdbl2(41) As Double, cbcustomdbl3(42) As Double, cbcustomdate1(43) As Date, cbcustomdate2(44) As Date, 
        'cbcustomdate3(45) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'cbid, cbcabang, cblokasi, cbsumber, cbautonotransaksi, cbnotransaksi, cbtgl, 
        'cbkodepa, cbkontak, cbkontakperson, cburaian, cbcatatan, cbmatauang, cbkurs, 
        'cbdebit, cbdebitvalas, cbkredit, cbkreditvalas, cbjumlahbayar, cbjumlahbayarvalas, cbstatusbayar, 
        'cbtgllunas, cbstatus, cbstatussebelumnya, cbjmlrevisi, cbcetakanke, cbisclose, cbinputuser, 
        'cbinputtgl, cbmodifikasiuser, cbmodifikasitgl, cbposting, cbcustomtext1, cbcustomtext2, cbcustomtext3, 
        'cbcustomtext4, cbcustomtext5, cbcustomint1, cbcustomint2, cbcustomint3, cbcustomdbl1, cbcustomdbl2, 
        'cbcustomdbl3, cbcustomdate1, cbcustomdate2, cbcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 46) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'cbid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "cbid required numeric." : GoTo selesai
        End If
        'cbautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "cbautonotransaksi required numeric." : GoTo selesai
        End If
        'cbtgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "cbtgl required date." : GoTo selesai
        End If
        'cbkodepa(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "cbkodepa required numeric." : GoTo selesai
        End If
        'cbkontak(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "cbkontak required numeric." : GoTo selesai
        End If
        If (dataUtama(8) < 1) Then
            result(2) = "cbkontak can't be empty." : GoTo selesai
        End If
        'cbkurs(13) As Double
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "cbkurs required numeric." : GoTo selesai
        End If
        'cbdebit(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "cbdebit required numeric." : GoTo selesai
        End If
        'cbdebitvalas(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "cbdebitvalas required numeric." : GoTo selesai
        End If
        'cbkredit(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "cbkredit required numeric." : GoTo selesai
        End If
        'cbkreditvalas(17) As Double
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "cbkreditvalas required numeric." : GoTo selesai
        End If
        'cbjumlahbayar(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "cbjumlahbayar required numeric." : GoTo selesai
        End If
        'cbjumlahbayarvalas(19) As Double
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "cbjumlahbayarvalas required numeric." : GoTo selesai
        End If
        'cbstatusbayar(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "cbstatusbayar required numeric." : GoTo selesai
        End If
        'cbtgllunas(21) As Date
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "cbtgllunas required date." : GoTo selesai
        End If
        'cbstatus(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "cbstatus required numeric." : GoTo selesai
        End If
        'cbstatussebelumnya(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "cbstatussebelumnya required numeric." : GoTo selesai
        End If
        'cbjmlrevisi(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "cbjmlrevisi required numeric." : GoTo selesai
        End If
        'cbcetakanke(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "cbcetakanke required numeric." : GoTo selesai
        End If
        'cbisclose(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "cbisclose required numeric." : GoTo selesai
        End If
        'cbinputuser(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "cbinputuser required numeric." : GoTo selesai
        End If
        'cbinputtgl(28) As DateTime
        If (IsDate(dataUtama(28)) = False) Then
            result(2) = "cbinputtgl required date." : GoTo selesai
        End If
        'cbmodifikasiuser(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "cbmodifikasiuser required numeric." : GoTo selesai
        End If
        'cbmodifikasitgl(30) As DateTime
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "cbmodifikasitgl required date." : GoTo selesai
        End If
        'cbposting(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "cbposting required numeric." : GoTo selesai
        End If
        'cbcustomint1(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "cbcustomint1 required numeric." : GoTo selesai
        End If
        'cbcustomint2(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "cbcustomint2 required numeric." : GoTo selesai
        End If
        'cbcustomint3(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "cbcustomint3 required numeric." : GoTo selesai
        End If
        'cbcustomdbl1(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "cbcustomdbl1 required numeric." : GoTo selesai
        End If
        'cbcustomdbl2(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "cbcustomdbl2 required numeric." : GoTo selesai
        End If
        'cbcustomdbl3(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "cbcustomdbl3 required numeric." : GoTo selesai
        End If
        'cbcustomdate1(43) As Date
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "cbcustomdate1 required date." : GoTo selesai
        End If
        'cbcustomdate2(44) As Date
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "cbcustomdate2 required date." : GoTo selesai
        End If
        'cbcustomdate3(45) As Date
        If (IsDate(dataUtama(45)) = False) Then
            result(2) = "cbcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'cbcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "cbcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "cbcabang should not be more than 25 character." : GoTo selesai
        End If

        'cblokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "cblokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "cblokasi should not be more than 25 character." : GoTo selesai
        End If

        'cbsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "cbsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "cbsumber should not be more than 10 character." : GoTo selesai
        End If

        'cbnotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "cbnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "cbnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'cbtgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "cbtgl can't be empty" : GoTo selesai
        End If

        'cbmatauang(12) As String
        If Len(dataUtama(12)) = 0 Then
            result(2) = "cbmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(12)) > 25 Then
            result(2) = "cbmatauang should not be more than 25 character." : GoTo selesai
        End If

        'cbkurs(13) As Double
        If Len(dataUtama(13)) = 0 Then
            result(2) = "cbkurs can't be empty" : GoTo selesai
        End If

        'cbdebit(14) As Double
        If Len(dataUtama(14)) = 0 Then
            result(2) = "cbdebit can't be empty" : GoTo selesai
        End If

        'cbdebitvalas(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "cbdebitvalas can't be empty" : GoTo selesai
        End If

        'cbkredit(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "cbkredit can't be empty" : GoTo selesai
        End If

        'cbkreditvalas(17) As Double
        If Len(dataUtama(17)) = 0 Then
            result(2) = "cbkreditvalas can't be empty" : GoTo selesai
        End If

        'cbjumlahbayar(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "cbjumlahbayar can't be empty" : GoTo selesai
        End If

        'cbjumlahbayarvalas(19) As Double
        If Len(dataUtama(19)) = 0 Then
            result(2) = "cbjumlahbayarvalas can't be empty" : GoTo selesai
        End If

        'cbinputtgl(28) As DateTime
        If Len(dataUtama(28)) = 0 Then
            result(2) = "cbinputtgl can't be empty" : GoTo selesai
        End If

        'cbmodifikasitgl(30) As DateTime
        If Len(dataUtama(30)) = 0 Then
            result(2) = "cbmodifikasitgl can't be empty" : GoTo selesai
        End If

        'cbcustomdbl1(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "cbcustomdbl1 can't be empty" : GoTo selesai
        End If

        'cbcustomdbl2(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "cbcustomdbl2 can't be empty" : GoTo selesai
        End If

        'cbcustomdbl3(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "cbcustomdbl3 can't be empty" : GoTo selesai
        End If

        'cbcustomdate1(43) As Date
        If Len(dataUtama(43)) = 0 Then
            result(2) = "cbcustomdate1 can't be empty" : GoTo selesai
        End If

        'cbcustomdate2(44) As Date
        If Len(dataUtama(44)) = 0 Then
            result(2) = "cbcustomdate2 can't be empty" : GoTo selesai
        End If

        'cbcustomdate3(45) As Date
        If Len(dataUtama(45)) = 0 Then
            result(2) = "cbcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "cbid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cblokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cburaian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbdebit", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "cbdebitvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "cbkredit", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "cbkreditvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "cbjumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbjumlahbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbstatusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbtgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cbcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cbcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "cbid~cbcabang~cblokasi~cbsumber~cbautonotransaksi~cbnotransaksi~cbtgl~cbkodepa~cbkontak~cbkontakperson~cburaian~cbcatatan~cbmatauang~cbkurs~cbdebit~cbdebitvalas~cbkredit~cbkreditvalas~cbjumlahbayar~cbjumlahbayarvalas~cbstatusbayar~cbtgllunas~cbstatus~cbstatussebelumnya~cbjmlrevisi~cbcetakanke~cbisclose~cbinputuser~cbinputtgl~cbmodifikasiuser~cbmodifikasitgl~cbposting~cbcustomtext1~cbcustomtext2~cbcustomtext3~cbcustomtext4~cbcustomtext5~cbcustomint1~cbcustomint2~cbcustomint3~cbcustomdbl1~cbcustomdbl2~cbcustomdbl3~cbcustomdate1~cbcustomdate2~cbcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idcbdetail(0) As Integer, idcb(1) As Integer, norek(2) As String, matauang(3) As String, kurs(4) As Double, 
        'debit(5) As Double, debitvalas(6) As Double, kredit(7) As Double, kreditvalas(8) As Double, catatan(9) As String, 
        'costcenter(10) As String, divisi(11) As String, subdivisi(12) As String, proyek(13) As String, urutan(14) As Integer, 
        'isclose(15) As Integer, customtext1(16) As String, customtext2(17) As String, customtext3(18) As String, customdbl1(19) As Double, 
        'customdbl2(20) As Double, customdbl3(21) As Double, customdate1(22) As Date, customdate2(23) As Date, customdate3(24) As Date


        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idcbdetail, idcb, norek, matauang, kurs, debit, debitvalas, 
        'kredit, kreditvalas, catatan, costcenter, divisi, subdivisi, proyek, 
        'urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idcbdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idcb", AsEnumTypeData.AsInt64)
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
            'idcbdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idcbdetail required numeric." : GoTo selesai
            End If
            'idcb(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idcb required numeric." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idcbdetail~idcb~norek~matauang~kurs~debit~debitvalas~kredit~kreditvalas~catatan~costcenter~divisi~subdivisi~proyek~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            strRekCostCenter = IIf(Len(strRekCostCenter.ToString) = 0, "", strRekCostCenter & " OR ")
            strRekCostCenter = String.Concat(strRekCostCenter, "(cnomor = '" & dataRowDetail(2) & "')")

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'MAPPING BUAT WS DATA PAY -------------------------------------------------------
        'idcbcarabayar(0) As Integer, idcb(1) As Integer, jenisgiro(2) As String, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, nogiro(7) As String, tgljt(8) As Date, bank(9) As String, 
        'noacbank(10) As String, rekbank(11) As String, rekgiro(12) As String, catatan(13) As String, urutan(14) As Integer

        'MAPPING BUAT FLEX DATA PAY -----------------------------------------------------
        'idcbcarabayar, idcb, jenisgiro, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan

        'VALIDASI DAN SET DATA PAY ======================================================
        'SPLIT PARAMETER DATA PAY

        'Buat datatable pay
        Dim dtpay As New DataTable
        AsDataTableTambahField(dtpay, "idcbcarabayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "idcb", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpay, "jenisgiro", AsEnumTypeData.AsInt64)
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


        'VALIDASI DAN SET DATA ROW GIRO ==================================================
        If Len(dataSplit(2)) > 0 Then
            'SPLIT PARAMETER DATA GIRO
            dataGiro = dataSplit(2).Split(sptRow)

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
                'idcbcarabayar(0) As Integer
                If (IsNumeric(dataRowGiro(0)) = False) Then
                    result(2) = "Giro Row : " & i & " - idcbcarabayar required numeric." : GoTo selesai
                End If
                'idcb(1) As Integer
                If (IsNumeric(dataRowGiro(1)) = False) Then
                    result(2) = "Giro Row : " & i & " - idcb required numeric." : GoTo selesai
                End If
                'jenisgiro(2) As Integer
                If (IsNumeric(dataRowGiro(2)) = False) Then
                    result(2) = "Giro Row : " & i & " - jenisgiro required numeric." : GoTo selesai
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
                If dataRowGiro(5) < 1 Then
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

                If AsDataTableTambahData(dtpay, "idcbcarabayar~idcb~jenisgiro~matauang~kurs~jumlah~jumlahvalas~nogiro~tgljt~bank~noacbank~rekbank~rekgiro~catatan~urutan", dataRowGiro(0) & "~" & dataRowGiro(1) & "~" & dataRowGiro(2) & "~" & dataRowGiro(3) & "~" & dataRowGiro(4) & "~" & dataRowGiro(5) & "~" & dataRowGiro(6) & "~" & dataRowGiro(7) & "~" & dataRowGiro(8) & "~" & dataRowGiro(9) & "~" & dataRowGiro(10) & "~" & dataRowGiro(11) & "~" & dataRowGiro(12) & "~" & dataRowGiro(13) & "~" & dataRowGiro(14)) = False Then
                    result(2) = "Giro Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
        End If
        'END OF VALIDASI DAN SET ROW DATA GIRO ===========================================


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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("cbtgl")), AsFormatTanggal(drutama("cbtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "cbmatauang", "", dtdetail, "norek")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                'CEK COA WAJIB COST CENTER ==============================
                If drutama("cbstatus") = 2 Then
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

                ''VALIDASI NOMINAL HARUS SEIMBANG
                'If debit <> kredit Then
                '    result(2) = "Total debits and credits in detail are not balanced." : GoTo selesai
                'End If
                'If debitvalas <> kreditvalas Then
                '    result(2) = "Total foreign debits and credits in detail are not balanced." : GoTo selesai
                'End If

                'HITUNG TOTAL TOTAL BERDASARKAN DATA DETAIL
                drutama("cbdebit") = debit
                drutama("cbdebitvalas") = debitvalas
                drutama("cbkredit") = kredit
                drutama("cbkreditvalas") = kreditvalas
                'END OF VALIDASI NOMINAL HARUS SEIMBANG =================


                If isUpdate Then
                    result(4) = drutama("cbid")
                    notransaksi = drutama("cbnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(cbid), cbnotransaksi FROM M2_cb WHERE cbid='" & result(4) & "' AND cbstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(cbid) FROM m2_cb WHERE cbnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m2_cb_history
                        Dim rsSimpanHistory As String = SimpanHistory.M2_Cb_HistorySimpan("" & paramSplit(0) & "★M2_Cb_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("cbsumber")) & "▼" & FixQuotes(drutama("cbid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M2_cb set cbcabang  = '" & FixQuotes(drutama("cbcabang")) & "', cblokasi  = '" & FixQuotes(drutama("cblokasi")) & "', cbsumber  = '" & FixQuotes(drutama("cbsumber")) & "', cbautonotransaksi  = " & drutama("cbautonotransaksi") & ", cbnotransaksi  = '" & notransaksi & "', cbtgl  = '" & FixQuotes(AsFormatTanggal(drutama("cbtgl"))) & "', cbkodepa  = " & drutama("cbkodepa") & ", cbkontak  = " & drutama("cbkontak") & ", cbkontakperson  = '" & FixQuotes(drutama("cbkontakperson")) & "', cburaian  = '" & FixQuotes(drutama("cburaian")) & "', cbcatatan  = '" & FixQuotes(drutama("cbcatatan")) & "', cbmatauang  = '" & FixQuotes(drutama("cbmatauang")) & "', cbkurs  = '" & FixDouble(drutama("cbkurs")) & "', cbdebit  = '" & FixDouble(drutama("cbdebit")) & "', cbdebitvalas  = '" & FixDouble(drutama("cbdebitvalas")) & "', cbkredit  = '" & FixDouble(drutama("cbkredit")) & "', cbkreditvalas  = '" & FixDouble(drutama("cbkreditvalas")) & "', cbjumlahbayar  = '" & FixDouble(drutama("cbjumlahbayar")) & "', cbjumlahbayarvalas  = '" & FixDouble(drutama("cbjumlahbayarvalas")) & "', cbstatusbayar  = " & drutama("cbstatusbayar") & ", cbtgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("cbtgllunas"))) & "', cbstatus  = " & drutama("cbstatus") & ", cbstatussebelumnya  = " & drutama("cbstatussebelumnya") & ", cbjmlrevisi  = cbjmlrevisi+1, cbcetakanke  = " & drutama("cbcetakanke") & ", cbisclose  = " & drutama("cbisclose") & ", cbmodifikasiuser  = " & drutama("cbmodifikasiuser") & ", cbmodifikasitgl  = NOW(), cbposting  = 0, cbcustomtext1  = '" & FixQuotes(drutama("cbcustomtext1")) & "', cbcustomtext2  = '" & FixQuotes(drutama("cbcustomtext2")) & "', cbcustomtext3  = '" & FixQuotes(drutama("cbcustomtext3")) & "', cbcustomtext4  = '" & FixQuotes(drutama("cbcustomtext4")) & "', cbcustomtext5  = '" & FixQuotes(drutama("cbcustomtext5")) & "', cbcustomint1  = " & drutama("cbcustomint1") & ", cbcustomint2  = " & drutama("cbcustomint2") & ", cbcustomint3  = " & drutama("cbcustomint3") & ", cbcustomdbl1  = '" & FixDouble(drutama("cbcustomdbl1")) & "', cbcustomdbl2  = '" & FixDouble(drutama("cbcustomdbl2")) & "', cbcustomdbl3  = '" & FixDouble(drutama("cbcustomdbl3")) & "', cbcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("cbcustomdate1"))) & "', cbcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("cbcustomdate2"))) & "', cbcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("cbcustomdate3"))) & "' where cbid = '" & drutama("cbid") & "'"
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

                    If drutama("cbautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("cbcabang"), drutama("cblokasi"), drutama("cbsumber"), drutama("cbtgl"))
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
                        notransaksi = drutama("cbnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(cbid) FROM m2_cb WHERE cbnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M2_cb (cbcabang, cblokasi, cbsumber, cbautonotransaksi, cbnotransaksi, cbtgl, cbkodepa, cbkontak, cbkontakperson, cburaian, cbcatatan, cbmatauang, cbkurs, cbdebit, cbdebitvalas, cbkredit, cbkreditvalas, cbjumlahbayar, cbjumlahbayarvalas, cbstatusbayar, cbtgllunas, cbstatus, cbstatussebelumnya, cbjmlrevisi, cbcetakanke, cbisclose, cbinputuser, cbinputtgl, cbmodifikasiuser, cbmodifikasitgl, cbposting, cbcustomtext1, cbcustomtext2, cbcustomtext3, cbcustomtext4, cbcustomtext5, cbcustomint1, cbcustomint2, cbcustomint3, cbcustomdbl1, cbcustomdbl2, cbcustomdbl3, cbcustomdate1, cbcustomdate2, cbcustomdate3) values('" & FixQuotes(drutama("cbcabang")) & "', '" & FixQuotes(drutama("cblokasi")) & "', '" & FixQuotes(drutama("cbsumber")) & "', " & drutama("cbautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("cbtgl"))) & "', " & drutama("cbkodepa") & ", " & drutama("cbkontak") & ", '" & FixQuotes(drutama("cbkontakperson")) & "', '" & FixQuotes(drutama("cburaian")) & "', '" & FixQuotes(drutama("cbcatatan")) & "', '" & FixQuotes(drutama("cbmatauang")) & "', '" & FixDouble(drutama("cbkurs")) & "', '" & FixDouble(drutama("cbdebit")) & "', '" & FixDouble(drutama("cbdebitvalas")) & "', '" & FixDouble(drutama("cbkredit")) & "', '" & FixDouble(drutama("cbkreditvalas")) & "', '" & FixDouble(drutama("cbjumlahbayar")) & "', '" & FixDouble(drutama("cbjumlahbayarvalas")) & "', " & drutama("cbstatusbayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("cbtgllunas"))) & "', " & drutama("cbstatus") & ", " & drutama("cbstatussebelumnya") & ", " & drutama("cbjmlrevisi") & ", " & drutama("cbcetakanke") & ", " & drutama("cbisclose") & ", " & drutama("cbinputuser") & ", NOW(), " & drutama("cbmodifikasiuser") & ", '1971-01-01 00:00:00', 0, '" & FixQuotes(drutama("cbcustomtext1")) & "', '" & FixQuotes(drutama("cbcustomtext2")) & "', '" & FixQuotes(drutama("cbcustomtext3")) & "', '" & FixQuotes(drutama("cbcustomtext4")) & "', '" & FixQuotes(drutama("cbcustomtext5")) & "', " & drutama("cbcustomint1") & ", " & drutama("cbcustomint2") & ", " & drutama("cbcustomint3") & ", '" & FixDouble(drutama("cbcustomdbl1")) & "', '" & FixDouble(drutama("cbcustomdbl2")) & "', '" & FixDouble(drutama("cbcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("cbcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("cbcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("cbcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select cbid from M2_cb where cbnotransaksi='" & notransaksi & "' AND cbinputuser= '" & userid & "' order by cbmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M2_cb_Detail where idcb = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idcbdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("norek")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("debit")) & "', '" & FixDouble(dr1("debitvalas")) & "', '" & FixDouble(dr1("kredit")) & "', '" & FixDouble(dr1("kreditvalas")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M2_cb_Detail(idcbdetail, idcb, norek, matauang, kurs, debit, debitvalas, kredit, kreditvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                    sql = "Delete from M2_Cb_Pay where idcb = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idcbcarabayar") & ", " & result(4) & ", " & dr1("jenisgiro") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ")")
                        'query untuk insert giro
                        If drutama("cbstatus") = 2 Then
                            strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", ", "))
                            strGiro.Append("('" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(drutama("cbsumber")) & "', " & result(4) & ", '" & FixQuotes(notransaksi) & "', " & drutama("cbkontak") & ", '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', " & dr1("jenisgiro") & ", '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', " & 0 & ", " & 0 & ", " & dr1("urutan") & ")")
                        End If
                    Next
                    sql = "Insert into M2_Cb_Pay(idcbcarabayar, idcb, jenisgiro, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'insert giro jika status approved
                    If drutama("cbstatus") = 2 And Len(strGiro.ToString) > 0 Then
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
                Dim sumber As String = "CB", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("cbstatus") = 2 Then
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
    Public Function M2_CbUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("cbkontakkode", "c1.kkode")
            Filter = Filter.Replace("cbkontaknama", "c1.knama")
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
            Dim sumber As String = "Cb", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Cbtgl, Cbnotransaksi, Cbstatus FROM m2_Cb WHERE Cbid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Cbstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m2_cb_history
            Dim rsSimpanHistory As String = SimpanHistory.M2_Cb_HistorySimpan("" & paramSplit(0) & "★M2_Cb_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m2_cb_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                ''CEK STATUS GIRO
                'dtdetail = AsDataTableAmbilDariDB("SELECT glnogiro FROM m2_giro_list WHERE glsumber = 'Cb' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "' AND glstatus <> 0")
                'If dtdetail.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai

                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'Cb' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'DELETE GIRO
                sql = "DELETE FROM m2_giro_list WHERE glsumber = 'Cb' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M2_Cb SET Cbstatus = " & nilaiStatus & ", Cbmodifikasiuser='" & userid & "', Cbmodifikasitgl = NOW(), Cbposting = 0, Cbpostingtgl = '1971-01-01 00:00:00', Cbjmlrevisi = Cbjmlrevisi + 1 WHERE Cbid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_CbSearch(PostWsSearch(paramSplit(0), "M2_CbSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_CbDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("cbkontakkode", "c1.kkode")
            Filter = Filter.Replace("cbkontaknama", "c1.knama")
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
            Dim sumber As String = "Cb", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Cbid, Cbnotransaksi FROM M2_Cb WHERE Cbid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT cbcabang, cblokasi, cbsumber, cbautonotransaksi, cbnotransaksi, cbtgl"
            sql &= " FROM M2_cb"
            sql &= " WHERE cbid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("cbcabang")
                lokasi = dtNomorNext.Rows(0)("cblokasi")
                sumber = dtNomorNext.Rows(0)("cbsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("cbautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("cbnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("cbtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE JURNAL
            sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'Cb' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE GIRO
            sql = "DELETE FROM m2_giro_list WHERE glsumber = 'Cb' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE PAY
            sql = "DELETE FROM M2_Cb_Pay WHERE idCb = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M2_Cb_Detail WHERE idCb = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M2_Cb WHERE Cbid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_CbSearch(PostWsSearch(paramSplit(0), "M2_CbSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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