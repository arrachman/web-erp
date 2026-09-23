Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m2_cr
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M2_CrSimpan(ByVal param As String) As String
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
        'crid(0) As Integer, crcabang(1) As String, crlokasi(2) As String, crsumber(3) As String, crautonotransaksi(4) As Integer, 
        'crnotransaksi(5) As String, crtgl(6) As Date, crkodepa(7) As Integer, crkontak(8) As Integer, crkontakperson(9) As String, 
        'crnorek(10) As String, cruraian(11) As String, crcatatan(12) As String, crmatauang(13) As String, crkurs(14) As Double, 
        'crjumlah(15) As Double, crjumlahvalas(16) As Double, crjumlahbayar(17) As Double, crjumlahbayarvalas(18) As Double, crstatusbayar(19) As Integer, 
        'crtgllunas(20) As Date, crstatus(21) As Integer, crstatussebelumnya(22) As Integer, crjmlrevisi(23) As Integer, crcetakanke(24) As Integer, 
        'crisclose(25) As Integer, crinputuser(26) As Integer, crinputtgl(27) As DateTime, crmodifikasiuser(28) As Integer, crmodifikasitgl(29) As DateTime, 
        'crposting(30) As Integer, crcustomtext1(31) As String, crcustomtext2(32) As String, crcustomtext3(33) As String, crcustomtext4(34) As String, 
        'crcustomtext5(35) As String, crcustomint1(36) As Integer, crcustomint2(37) As Integer, crcustomint3(38) As Integer, crcustomdbl1(39) As Double, 
        'crcustomdbl2(40) As Double, crcustomdbl3(41) As Double, crcustomdate1(42) As Date, crcustomdate2(43) As Date, crcustomdate3(44) As Date


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'crid, crcabang, crlokasi, crsumber, crautonotransaksi, crnotransaksi, crtgl, 
        'crkodepa, crkontak, crkontakperson, crnorek, cruraian, crcatatan, crmatauang, 
        'crkurs, crjumlah, crjumlahvalas, crjumlahbayar, crjumlahbayarvalas, crstatusbayar, crtgllunas, 
        'crstatus, crstatussebelumnya, crjmlrevisi, crcetakanke, crisclose, crinputuser, crinputtgl, 
        'crmodifikasiuser, crmodifikasitgl, crposting, crcustomtext1, crcustomtext2, crcustomtext3, crcustomtext4, 
        'crcustomtext5, crcustomint1, crcustomint2, crcustomint3, crcustomdbl1, crcustomdbl2, crcustomdbl3, 
        'crcustomdate1, crcustomdate2, crcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 45) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'crid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "crid required numeric." : GoTo selesai
        End If
        'crautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "crautonotransaksi required numeric." : GoTo selesai
        End If
        'crtgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "crtgl required date." : GoTo selesai
        End If
        'crkodepa(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "crkodepa required numeric." : GoTo selesai
        End If
        'crkontak(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "crkontak required numeric." : GoTo selesai
        End If
        If (dataUtama(8) < 1) Then
            result(2) = "crkontak can't be empty." : GoTo selesai
        End If
        'crkurs(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "crkurs required numeric." : GoTo selesai
        End If
        'crjumlah(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "crjumlah required numeric." : GoTo selesai
        End If
        'crjumlahvalas(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "crjumlahvalas required numeric." : GoTo selesai
        End If
        'crjumlahbayar(17) As Double
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "crjumlahbayar required numeric." : GoTo selesai
        End If
        'crjumlahbayarvalas(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "crjumlahbayarvalas required numeric." : GoTo selesai
        End If
        'crstatusbayar(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "crstatusbayar required numeric." : GoTo selesai
        End If
        'crtgllunas(20) As Date
        If (IsDate(dataUtama(20)) = False) Then
            result(2) = "crtgllunas required date." : GoTo selesai
        End If
        'crstatus(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "crstatus required numeric." : GoTo selesai
        End If
        'crstatussebelumnya(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "crstatussebelumnya required numeric." : GoTo selesai
        End If
        'crjmlrevisi(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "crjmlrevisi required numeric." : GoTo selesai
        End If
        'crcetakanke(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "crcetakanke required numeric." : GoTo selesai
        End If
        'crisclose(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "crisclose required numeric." : GoTo selesai
        End If
        'crinputuser(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "crinputuser required numeric." : GoTo selesai
        End If
        'crinputtgl(27) As DateTime
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "crinputtgl required date." : GoTo selesai
        End If
        'crmodifikasiuser(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "crmodifikasiuser required numeric." : GoTo selesai
        End If
        'crmodifikasitgl(29) As DateTime
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "crmodifikasitgl required date." : GoTo selesai
        End If
        'crposting(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "crposting required numeric." : GoTo selesai
        End If
        'crcustomint1(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "crcustomint1 required numeric." : GoTo selesai
        End If
        'crcustomint2(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "crcustomint2 required numeric." : GoTo selesai
        End If
        'crcustomint3(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "crcustomint3 required numeric." : GoTo selesai
        End If
        'crcustomdbl1(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "crcustomdbl1 required numeric." : GoTo selesai
        End If
        'crcustomdbl2(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "crcustomdbl2 required numeric." : GoTo selesai
        End If
        'crcustomdbl3(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "crcustomdbl3 required numeric." : GoTo selesai
        End If
        'crcustomdate1(42) As Date
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "crcustomdate1 required date." : GoTo selesai
        End If
        'crcustomdate2(43) As Date
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "crcustomdate2 required date." : GoTo selesai
        End If
        'crcustomdate3(44) As Date
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "crcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'crcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "crcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "crcabang should not be more than 25 character." : GoTo selesai
        End If

        'crlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "crlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "crlokasi should not be more than 25 character." : GoTo selesai
        End If

        'crsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "crsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "crsumber should not be more than 10 character." : GoTo selesai
        End If

        'crnotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "crnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "crnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'crtgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "crtgl can't be empty" : GoTo selesai
        End If

        'crnorek(10) As String
        If Len(dataUtama(10)) = 0 Then
            result(2) = "crnorek can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(10)) > 25 Then
            result(2) = "crnorek should not be more than 25 character." : GoTo selesai
        End If

        'crmatauang(13) As String
        If Len(dataUtama(13)) = 0 Then
            result(2) = "crmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(13)) > 25 Then
            result(2) = "crmatauang should not be more than 25 character." : GoTo selesai
        End If

        'crkurs(14) As Double
        If Len(dataUtama(14)) = 0 Then
            result(2) = "crkurs can't be empty" : GoTo selesai
        End If

        'crjumlah(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "crjumlah can't be empty" : GoTo selesai
        End If

        'crjumlahvalas(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "crjumlahvalas can't be empty" : GoTo selesai
        End If

        'crjumlahbayar(17) As Double
        If Len(dataUtama(17)) = 0 Then
            result(2) = "crjumlahbayar can't be empty" : GoTo selesai
        End If

        'crjumlahbayarvalas(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "crjumlahbayarvalas can't be empty" : GoTo selesai
        End If

        'crinputtgl(27) As DateTime
        If Len(dataUtama(27)) = 0 Then
            result(2) = "crinputtgl can't be empty" : GoTo selesai
        End If

        'crmodifikasitgl(29) As DateTime
        If Len(dataUtama(29)) = 0 Then
            result(2) = "crmodifikasitgl can't be empty" : GoTo selesai
        End If

        'crcustomdbl1(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "crcustomdbl1 can't be empty" : GoTo selesai
        End If

        'crcustomdbl2(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "crcustomdbl2 can't be empty" : GoTo selesai
        End If

        'crcustomdbl3(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "crcustomdbl3 can't be empty" : GoTo selesai
        End If

        'crcustomdate1(42) As Date
        If Len(dataUtama(42)) = 0 Then
            result(2) = "crcustomdate1 can't be empty" : GoTo selesai
        End If

        'crcustomdate2(43) As Date
        If Len(dataUtama(43)) = 0 Then
            result(2) = "crcustomdate2 can't be empty" : GoTo selesai
        End If

        'crcustomdate3(44) As Date
        If Len(dataUtama(44)) = 0 Then
            result(2) = "crcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "crid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crnorek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cruraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crjumlah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crjumlahvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crjumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crjumlahbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crstatusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crtgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "crid~crcabang~crlokasi~crsumber~crautonotransaksi~crnotransaksi~crtgl~crkodepa~crkontak~crkontakperson~crnorek~cruraian~crcatatan~crmatauang~crkurs~crjumlah~crjumlahvalas~crjumlahbayar~crjumlahbayarvalas~crstatusbayar~crtgllunas~crstatus~crstatussebelumnya~crjmlrevisi~crcetakanke~crisclose~crinputuser~crinputtgl~crmodifikasiuser~crmodifikasitgl~crposting~crcustomtext1~crcustomtext2~crcustomtext3~crcustomtext4~crcustomtext5~crcustomint1~crcustomint2~crcustomint3~crcustomdbl1~crcustomdbl2~crcustomdbl3~crcustomdate1~crcustomdate2~crcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idcrdetail(0) As Integer, idcr(1) As Integer, norek(2) As String, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, catatan(7) As String, costcenter(8) As String, divisi(9) As String, 
        'subdivisi(10) As String, proyek(11) As String, urutan(12) As Integer, isclose(13) As Integer, customtext1(14) As String, 
        'customtext2(15) As String, customtext3(16) As String, customdbl1(17) As Double, customdbl2(18) As Double, customdbl3(19) As Double, 
        'customdate1(20) As Date, customdate2(21) As Date, customdate3(22) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idcrdetail, idcr, norek, matauang, kurs, jumlah, jumlahvalas, 
        'catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idcrdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idcr", AsEnumTypeData.AsInt64)
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
            'idcrdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idcrdetail required numeric." : GoTo selesai
            End If
            'idcr(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idcr required numeric." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idcrdetail~idcr~norek~matauang~kurs~jumlah~jumlahvalas~catatan~costcenter~divisi~subdivisi~proyek~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22)) = False Then
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
                Dim vModuleId As Integer = 2, vMenuId As Integer = 3
                Select Case drutama("crstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("crtgl")), AsFormatTanggal(drutama("crtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "crmatauang", "crnorek", dtdetail, "norek")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                'CEK COA WAJIB COST CENTER ==============================
                If drutama("crstatus") = 2 Then
                    Dim cekCoaCostCenter As String = ValidasiCoaRequiredCostCenter(strRekCostCenter, dtdetail)
                    If Len(cekCoaCostCenter) > 0 Then
                        result(2) = cekCoaCostCenter : Trans.Rollback() : GoTo selesai
                    End If
                End If
                'END OF CEK COA WAJIB COST CENTER =======================


                'HITUNG TOTAL BERDASARKAN DATA DETAIL ===================
                drutama("crjumlah") = AsDataTableDSum(dtdetail, "jumlah")
                drutama("crjumlahvalas") = AsDataTableDSum(dtdetail, "jumlahvalas")
                'END OF HITUNG TOTAL BERDASARKAN DATA DETAIL ============

                If isUpdate Then
                    result(4) = drutama("crid")
                    notransaksi = drutama("crnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(crid), crnotransaksi FROM M2_Cr WHERE crid='" & result(4) & "' AND crstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("crautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("crcabang"), drutama("crlokasi"), drutama("crsumber"), drutama("crtgl"), drutama("crsumber"), 2)
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(crid) FROM m2_cr WHERE crnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============


                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m2_cr_history
                        Dim rsSimpanHistory As String = SimpanHistory.M2_Cr_HistorySimpan("" & paramSplit(0) & "★M2_Cr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("crsumber")) & "▼" & FixQuotes(drutama("crid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================


                        sql = "Update M2_Cr set crcabang  = '" & FixQuotes(drutama("crcabang")) & "', crlokasi  = '" & FixQuotes(drutama("crlokasi")) & "', crsumber  = '" & FixQuotes(drutama("crsumber")) & "', crautonotransaksi  = " & drutama("crautonotransaksi") & ", crnotransaksi  = '" & notransaksi & "', crtgl  = '" & FixQuotes(AsFormatTanggal(drutama("crtgl"))) & "', crkodepa  = " & drutama("crkodepa") & ", crkontak  = " & drutama("crkontak") & ", crkontakperson  = '" & FixQuotes(drutama("crkontakperson")) & "', crnorek  = '" & FixQuotes(drutama("crnorek")) & "', cruraian  = '" & FixQuotes(drutama("cruraian")) & "', crcatatan  = '" & FixQuotes(drutama("crcatatan")) & "', crmatauang  = '" & FixQuotes(drutama("crmatauang")) & "', crkurs  = '" & FixDouble(drutama("crkurs")) & "', crjumlah  = '" & FixDouble(drutama("crjumlah")) & "', crjumlahvalas  = '" & FixDouble(drutama("crjumlahvalas")) & "', crjumlahbayar  = '" & FixDouble(drutama("crjumlahbayar")) & "', crjumlahbayarvalas  = '" & FixDouble(drutama("crjumlahbayarvalas")) & "', crstatusbayar  = " & drutama("crstatusbayar") & ", crtgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("crtgllunas"))) & "', crstatus  = " & drutama("crstatus") & ", crstatussebelumnya  = " & drutama("crstatussebelumnya") & ", crjmlrevisi  = crjmlrevisi+1, crcetakanke  = " & drutama("crcetakanke") & ", crisclose  = " & drutama("crisclose") & ", crmodifikasiuser  = " & drutama("crmodifikasiuser") & ", crmodifikasitgl  = NOW(), crposting  = 0, crcustomtext1  = '" & FixQuotes(drutama("crcustomtext1")) & "', crcustomtext2  = '" & FixQuotes(drutama("crcustomtext2")) & "', crcustomtext3  = '" & FixQuotes(drutama("crcustomtext3")) & "', crcustomtext4  = '" & FixQuotes(drutama("crcustomtext4")) & "', crcustomtext5  = '" & FixQuotes(drutama("crcustomtext5")) & "', crcustomint1  = " & drutama("crcustomint1") & ", crcustomint2  = " & drutama("crcustomint2") & ", crcustomint3  = " & drutama("crcustomint3") & ", crcustomdbl1  = '" & FixDouble(drutama("crcustomdbl1")) & "', crcustomdbl2  = '" & FixDouble(drutama("crcustomdbl2")) & "', crcustomdbl3  = '" & FixDouble(drutama("crcustomdbl3")) & "', crcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("crcustomdate1"))) & "', crcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("crcustomdate2"))) & "', crcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("crcustomdate3"))) & "' where crid = '" & drutama("crid") & "'"
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

                    If drutama("crautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("crcabang"), drutama("crlokasi"), drutama("crsumber"), drutama("crtgl"), drutama("crsumber"), 2)
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
                        notransaksi = drutama("crnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(crid) FROM m2_cr WHERE crnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M2_Cr (crcabang, crlokasi, crsumber, crautonotransaksi, crnotransaksi, crtgl, crkodepa, crkontak, crkontakperson, crnorek, cruraian, crcatatan, crmatauang, crkurs, crjumlah, crjumlahvalas, crjumlahbayar, crjumlahbayarvalas, crstatusbayar, crtgllunas, crstatus, crstatussebelumnya, crjmlrevisi, crcetakanke, crisclose, crinputuser, crinputtgl, crmodifikasiuser, crmodifikasitgl, crposting, crcustomtext1, crcustomtext2, crcustomtext3, crcustomtext4, crcustomtext5, crcustomint1, crcustomint2, crcustomint3, crcustomdbl1, crcustomdbl2, crcustomdbl3, crcustomdate1, crcustomdate2, crcustomdate3) values('" & FixQuotes(drutama("crcabang")) & "', '" & FixQuotes(drutama("crlokasi")) & "', '" & FixQuotes(drutama("crsumber")) & "', " & drutama("crautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("crtgl"))) & "', " & drutama("crkodepa") & ", " & drutama("crkontak") & ", '" & FixQuotes(drutama("crkontakperson")) & "', '" & FixQuotes(drutama("crnorek")) & "', '" & FixQuotes(drutama("cruraian")) & "', '" & FixQuotes(drutama("crcatatan")) & "', '" & FixQuotes(drutama("crmatauang")) & "', '" & FixDouble(drutama("crkurs")) & "', '" & FixDouble(drutama("crjumlah")) & "', '" & FixDouble(drutama("crjumlahvalas")) & "', '" & FixDouble(drutama("crjumlahbayar")) & "', '" & FixDouble(drutama("crjumlahbayarvalas")) & "', " & drutama("crstatusbayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("crtgllunas"))) & "', " & drutama("crstatus") & ", " & drutama("crstatussebelumnya") & ", " & drutama("crjmlrevisi") & ", " & drutama("crcetakanke") & ", " & drutama("crisclose") & ", " & drutama("crinputuser") & ", NOW(), " & drutama("crmodifikasiuser") & ", '1971-01-01 00:00:00', 0, '" & FixQuotes(drutama("crcustomtext1")) & "', '" & FixQuotes(drutama("crcustomtext2")) & "', '" & FixQuotes(drutama("crcustomtext3")) & "', '" & FixQuotes(drutama("crcustomtext4")) & "', '" & FixQuotes(drutama("crcustomtext5")) & "', " & drutama("crcustomint1") & ", " & drutama("crcustomint2") & ", " & drutama("crcustomint3") & ", '" & FixDouble(drutama("crcustomdbl1")) & "', '" & FixDouble(drutama("crcustomdbl2")) & "', '" & FixDouble(drutama("crcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("crcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("crcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("crcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select crid from M2_Cr where crnotransaksi='" & notransaksi & "' AND Crinputuser= '" & userid & "' order by Crmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M2_Cr_Detail where idcr = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idcrdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("norek")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M2_Cr_Detail(idcrdetail, idcr, norek, matauang, kurs, jumlah, jumlahvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                Dim sumber As String = "CR", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("crstatus") = 2 Then
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
    Public Function M2_CrUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("crkontakkode", "c.kkode")
            Filter = Filter.Replace("crkontaknama", "c.knama")
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
            Dim sumber As String = "Cr", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Crtgl, Crnotransaksi, Crstatus FROM m2_Cr WHERE Crid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Crstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m2_cr_history
            Dim rsSimpanHistory As String = SimpanHistory.M2_Cr_HistorySimpan("" & paramSplit(0) & "★M2_Cr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================


            If isDelete Then
                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'CR' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M2_Cr SET Crstatus = " & nilaiStatus & ", crmodifikasiuser='" & userid & "', crmodifikasitgl = NOW(), crposting = 0, crpostingtgl = '1971-01-01 00:00:00', Crjmlrevisi = Crjmlrevisi + 1 WHERE crid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_CrSearch(PostWsSearch(paramSplit(0), "M2_CrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_CrDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("crkontakkode", "c.kkode")
            Filter = Filter.Replace("crkontaknama", "c.knama")
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
            Dim sumber As String = "CR", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT crid, crnotransaksi FROM m2_cr WHERE crid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT crcabang, crlokasi, crsumber, crautonotransaksi, crnotransaksi, crtgl"
            sql &= " FROM M2_cr"
            sql &= " WHERE crid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("crcabang")
                lokasi = dtNomorNext.Rows(0)("crlokasi")
                sumber = dtNomorNext.Rows(0)("crsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("crautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("crnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("crtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE JURNAL
            sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'CR' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M2_Cr_Detail WHERE idcr = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M2_Cr WHERE crid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_CrSearch(PostWsSearch(paramSplit(0), "M2_CrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_CrGetdataById(ByVal param As String) As String

        'M2_CrGetdataById Utama --------------------------------------------------------
        'crid, crcabang, crlokasi, crsumber, crautonotransaksi, crnotransaksi, crtgl, 
        'crkodepa, crkontak, crkontakperson, crnorek, cruraian, crcatatan, crmatauang, 
        'crkurs, crjumlah, crjumlahvalas, crjumlahbayar, crjumlahbayarvalas, crstatusbayar, crtgllunas, 
        'crstatus, crstatussebelumnya, crjmlrevisi, crcetakanke, crisclose, crinputuser, crinputtgl, 
        'crmodifikasiuser, crmodifikasitgl, crposting, crpostingtgl, crcustomtext1, crcustomtext2, crcustomtext3, 
        'crcustomtext4, crcustomtext5, crcustomint1, crcustomint2, crcustomint3, crcustomdbl1, crcustomdbl2, 
        'crcustomdbl3, crcustomdate1, crcustomdate2, crcustomdate3, crcabangnama, crlokasinama, crkontakkode, 
        'crkontaknama, crnoreknama, crstatusnama, crstatussebelumnyanama, crinputusernama, crmodifikasiusernama

        'M2_CrGetdataById Detail -------------------------------------------------------
        'idcrdetail, idcr, 
        'norek, matauang, kurs, jumlah, jumlahvalas, catatan, costcenter, 
        'divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, 
        'customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'noreknama, costcenternama, divisinama, subdivisinama, proyeknama

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

        Dim NmMemcached As String = "aplikasi1-M2_Cr~M2_Cr_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "crid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "crid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_cr_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("crid"), 0), sptField,
                     FxDB(drutama("crcabang"), ""), sptField,
                     FxDB(drutama("crlokasi"), ""), sptField,
                     FxDB(drutama("crsumber"), ""), sptField,
                     FxDB(drutama("crautonotransaksi"), 0), sptField,
                     FxDB(drutama("crnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("crtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("crkodepa"), 0), sptField,
                     FxDB(drutama("crkontak"), 0), sptField,
                     FxDB(drutama("crkontakperson"), ""), sptField,
                     FxDB(drutama("crnorek"), ""), sptField,
                     FxDB(drutama("cruraian"), ""), sptField,
                     FxDB(drutama("crcatatan"), ""), sptField,
                     FxDB(drutama("crmatauang"), ""), sptField,
                     FxDB(drutama("crkurs"), 0), sptField,
                     FxDB(drutama("crjumlah"), 0), sptField,
                     FxDB(drutama("crjumlahvalas"), 0), sptField,
                     FxDB(drutama("crjumlahbayar"), 0), sptField,
                     FxDB(drutama("crjumlahbayarvalas"), 0), sptField,
                     FxDB(drutama("crstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("crtgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("crstatus"), 0), sptField,
                     FxDB(drutama("crstatussebelumnya"), 0), sptField,
                     FxDB(drutama("crjmlrevisi"), 0), sptField,
                     FxDB(drutama("crcetakanke"), 0), sptField,
                     FxDB(drutama("crisclose"), 0), sptField,
                     FxDB(drutama("crinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("crinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("crmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("crmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("crposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("crpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("crcustomtext1"), ""), sptField,
                     FxDB(drutama("crcustomtext2"), ""), sptField,
                     FxDB(drutama("crcustomtext3"), ""), sptField,
                     FxDB(drutama("crcustomtext4"), ""), sptField,
                     FxDB(drutama("crcustomtext5"), ""), sptField,
                     FxDB(drutama("crcustomint1"), 0), sptField,
                     FxDB(drutama("crcustomint2"), 0), sptField,
                     FxDB(drutama("crcustomint3"), 0), sptField,
                     FxDB(drutama("crcustomdbl1"), 0), sptField,
                     FxDB(drutama("crcustomdbl2"), 0), sptField,
                     FxDB(drutama("crcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("crcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("crcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("crcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("crcabangnama"), ""), sptField,
                     FxDB(drutama("crlokasinama"), ""), sptField,
                     FxDB(drutama("crkontakkode"), ""), sptField,
                     FxDB(drutama("crkontaknama"), ""), sptField,
                     FxDB(drutama("crnoreknama"), ""), sptField,
                     FxDB(drutama("crstatusnama"), ""), sptField,
                     FxDB(drutama("crstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("crinputusernama"), ""), sptField,
                     FxDB(drutama("crmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idcrdetail"), 0), sptField,
                     FxDB(dr("idcr"), 0), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("crid, crcabang, crlokasi, crsumber, crautonotransaksi, crnotransaksi, crtgl, crkodepa, crkontak, crkontakperson, crnorek, cruraian, crcatatan, crmatauang, crkurs, crjumlah, crjumlahvalas, crjumlahbayar, crjumlahbayarvalas, crstatusbayar, crtgllunas, crstatus, crstatussebelumnya, crjmlrevisi, crcetakanke, crisclose, crinputuser, crinputtgl, crmodifikasiuser, crmodifikasitgl, crposting, crpostingtgl, crcustomtext1, crcustomtext2, crcustomtext3, crcustomtext4, crcustomtext5, crcustomint1, crcustomint2, crcustomint3, crcustomdbl1, crcustomdbl2, crcustomdbl3, crcustomdate1, crcustomdate2, crcustomdate3, crcabangnama, crlokasinama, crkontakkode, crkontaknama, crnoreknama, crstatusnama, crstatussebelumnyanama, crinputusernama, crmodifikasiusernama" & sptSubParam & "idcrdetail, idcr, norek, matauang, kurs, jumlah, jumlahvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, noreknama, costcenternama, divisinama, subdivisinama, proyeknama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_CrSearch(ByVal param As String) As String
        'M2_CrSearch --------------------------------------------------------
        'crid, crcabang, crlokasi, crsumber, crautonotransaksi, crnotransaksi, crtgl, 
        'crkodepa, crkontak, crkontakperson, crnorek, cruraian, crcatatan, crmatauang, 
        'crkurs, crjumlah, crjumlahvalas, crjumlahbayar, crjumlahbayarvalas, crstatusbayar, crtgllunas, 
        'crstatus, crstatussebelumnya, crjmlrevisi, crcetakanke, crisclose, crinputuser, crinputtgl, 
        'crmodifikasiuser, crmodifikasitgl, crposting, crpostingtgl, crcabangnama, crlokasinama, crkontakkode, 
        'crkontaknama, crnoreknama, crstatusnama, crstatussebelumnyanama, crinputusernama, crmodifikasiusernama

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
            Filter = Filter.Replace("crkontakkode", "c.kkode")
            Filter = Filter.Replace("crkontaknama", "c.knama")
            Filter = Filter.Replace("Crkontaknama", "c.knama")
            Filter = Filter.Replace("Crstatusnama", "`st1`.`nama`")
            Filter = Filter.Replace("Crinputusernama", "`u1`.`unama`")
            Filter = Filter.Replace("Crmodifikasiusernama", "`u2`.`unama`")
            Filter = Filter.Replace("Crcabangnama", "`br`.`bnama`")
            Filter = Filter.Replace("Crlokasinama", "`lc`.`lnama`")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_cr_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Cr", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("crid"), 0), sptField,
                     FxDB(dr("crcabang"), ""), sptField,
                     FxDB(dr("crlokasi"), ""), sptField,
                     FxDB(dr("crsumber"), ""), sptField,
                     FxDB(dr("crautonotransaksi"), 0), sptField,
                     FxDB(dr("crnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("crtgl"), ""), formatTgl), sptField,
                     FxDB(dr("crkodepa"), 0), sptField,
                     FxDB(dr("crkontak"), 0), sptField,
                     FxDB(dr("crkontakperson"), ""), sptField,
                     FxDB(dr("crnorek"), ""), sptField,
                     FxDB(dr("cruraian"), ""), sptField,
                     FxDB(dr("crcatatan"), ""), sptField,
                     FxDB(dr("crmatauang"), ""), sptField,
                     FxDB(dr("crkurs"), 0), sptField,
                     FxDB(dr("crjumlah"), 0), sptField,
                     FxDB(dr("crjumlahvalas"), 0), sptField,
                     FxDB(dr("crjumlahbayar"), 0), sptField,
                     FxDB(dr("crjumlahbayarvalas"), 0), sptField,
                     FxDB(dr("crstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("crtgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("crstatus"), 0), sptField,
                     FxDB(dr("crstatussebelumnya"), 0), sptField,
                     FxDB(dr("crjmlrevisi"), 0), sptField,
                     FxDB(dr("crcetakanke"), 0), sptField,
                     FxDB(dr("crisclose"), 0), sptField,
                     FxDB(dr("crinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("crinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("crmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("crmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("crposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("crpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("crcabangnama"), ""), sptField,
                     FxDB(dr("crlokasinama"), ""), sptField,
                     FxDB(dr("crkontakkode"), ""), sptField,
                     FxDB(dr("crkontaknama"), ""), sptField,
                     FxDB(dr("crnoreknama"), ""), sptField,
                     FxDB(dr("crstatusnama"), ""), sptField,
                     FxDB(dr("crstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("crinputusernama"), ""), sptField,
                     FxDB(dr("crmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("crid, crcabang, crlokasi, crsumber, crautonotransaksi, crnotransaksi, crtgl, crkodepa, crkontak, crkontakperson, crnorek, cruraian, crcatatan, crmatauang, crkurs, crjumlah, crjumlahvalas, crjumlahbayar, crjumlahbayarvalas, crstatusbayar, crtgllunas, crstatus, crstatussebelumnya, crjmlrevisi, crcetakanke, crisclose, crinputuser, crinputtgl, crmodifikasiuser, crmodifikasitgl, crposting, crpostingtgl, crcabangnama, crlokasinama, crkontakkode, crkontaknama, crnoreknama, crstatusnama, crstatussebelumnyanama, crinputusernama, crmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_CrTerkait(ByVal param As String) As String
        'M2_CrTerkait --------------------------------------------------------
        'crid, crnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
                     FxDB(dr("crid"), 0), sptField,
                     FxDB(dr("crnotransaksi"), ""), sptField,
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
            result(2) = "Related CR data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("crid, crnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_CrSimpanOld(ByVal param As String) As String
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
        'crid(0) As Integer, crcabang(1) As String, crlokasi(2) As String, crsumber(3) As String, crautonotransaksi(4) As Integer, 
        'crnotransaksi(5) As String, crtgl(6) As Date, crkodepa(7) As Integer, crkontak(8) As Integer, crkontakperson(9) As String, 
        'crnorek(10) As String, cruraian(11) As String, crcatatan(12) As String, crmatauang(13) As String, crkurs(14) As Double, 
        'crjumlah(15) As Double, crjumlahvalas(16) As Double, crjumlahbayar(17) As Double, crjumlahbayarvalas(18) As Double, crstatusbayar(19) As Integer, 
        'crtgllunas(20) As Date, crstatus(21) As Integer, crstatussebelumnya(22) As Integer, crjmlrevisi(23) As Integer, crcetakanke(24) As Integer, 
        'crisclose(25) As Integer, crinputuser(26) As Integer, crinputtgl(27) As DateTime, crmodifikasiuser(28) As Integer, crmodifikasitgl(29) As DateTime, 
        'crposting(30) As Integer, crcustomtext1(31) As String, crcustomtext2(32) As String, crcustomtext3(33) As String, crcustomtext4(34) As String, 
        'crcustomtext5(35) As String, crcustomint1(36) As Integer, crcustomint2(37) As Integer, crcustomint3(38) As Integer, crcustomdbl1(39) As Double, 
        'crcustomdbl2(40) As Double, crcustomdbl3(41) As Double, crcustomdate1(42) As Date, crcustomdate2(43) As Date, crcustomdate3(44) As Date


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'crid, crcabang, crlokasi, crsumber, crautonotransaksi, crnotransaksi, crtgl, 
        'crkodepa, crkontak, crkontakperson, crnorek, cruraian, crcatatan, crmatauang, 
        'crkurs, crjumlah, crjumlahvalas, crjumlahbayar, crjumlahbayarvalas, crstatusbayar, crtgllunas, 
        'crstatus, crstatussebelumnya, crjmlrevisi, crcetakanke, crisclose, crinputuser, crinputtgl, 
        'crmodifikasiuser, crmodifikasitgl, crposting, crcustomtext1, crcustomtext2, crcustomtext3, crcustomtext4, 
        'crcustomtext5, crcustomint1, crcustomint2, crcustomint3, crcustomdbl1, crcustomdbl2, crcustomdbl3, 
        'crcustomdate1, crcustomdate2, crcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 45) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'crid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "crid required numeric." : GoTo selesai
        End If
        'crautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "crautonotransaksi required numeric." : GoTo selesai
        End If
        'crtgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "crtgl required date." : GoTo selesai
        End If
        'crkodepa(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "crkodepa required numeric." : GoTo selesai
        End If
        'crkontak(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "crkontak required numeric." : GoTo selesai
        End If
        If (dataUtama(8) < 1) Then
            result(2) = "crkontak can't be empty." : GoTo selesai
        End If
        'crkurs(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "crkurs required numeric." : GoTo selesai
        End If
        'crjumlah(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "crjumlah required numeric." : GoTo selesai
        End If
        'crjumlahvalas(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "crjumlahvalas required numeric." : GoTo selesai
        End If
        'crjumlahbayar(17) As Double
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "crjumlahbayar required numeric." : GoTo selesai
        End If
        'crjumlahbayarvalas(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "crjumlahbayarvalas required numeric." : GoTo selesai
        End If
        'crstatusbayar(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "crstatusbayar required numeric." : GoTo selesai
        End If
        'crtgllunas(20) As Date
        If (IsDate(dataUtama(20)) = False) Then
            result(2) = "crtgllunas required date." : GoTo selesai
        End If
        'crstatus(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "crstatus required numeric." : GoTo selesai
        End If
        'crstatussebelumnya(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "crstatussebelumnya required numeric." : GoTo selesai
        End If
        'crjmlrevisi(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "crjmlrevisi required numeric." : GoTo selesai
        End If
        'crcetakanke(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "crcetakanke required numeric." : GoTo selesai
        End If
        'crisclose(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "crisclose required numeric." : GoTo selesai
        End If
        'crinputuser(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "crinputuser required numeric." : GoTo selesai
        End If
        'crinputtgl(27) As DateTime
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "crinputtgl required date." : GoTo selesai
        End If
        'crmodifikasiuser(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "crmodifikasiuser required numeric." : GoTo selesai
        End If
        'crmodifikasitgl(29) As DateTime
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "crmodifikasitgl required date." : GoTo selesai
        End If
        'crposting(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "crposting required numeric." : GoTo selesai
        End If
        'crcustomint1(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "crcustomint1 required numeric." : GoTo selesai
        End If
        'crcustomint2(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "crcustomint2 required numeric." : GoTo selesai
        End If
        'crcustomint3(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "crcustomint3 required numeric." : GoTo selesai
        End If
        'crcustomdbl1(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "crcustomdbl1 required numeric." : GoTo selesai
        End If
        'crcustomdbl2(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "crcustomdbl2 required numeric." : GoTo selesai
        End If
        'crcustomdbl3(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "crcustomdbl3 required numeric." : GoTo selesai
        End If
        'crcustomdate1(42) As Date
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "crcustomdate1 required date." : GoTo selesai
        End If
        'crcustomdate2(43) As Date
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "crcustomdate2 required date." : GoTo selesai
        End If
        'crcustomdate3(44) As Date
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "crcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'crcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "crcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "crcabang should not be more than 25 character." : GoTo selesai
        End If

        'crlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "crlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "crlokasi should not be more than 25 character." : GoTo selesai
        End If

        'crsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "crsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "crsumber should not be more than 10 character." : GoTo selesai
        End If

        'crnotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "crnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "crnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'crtgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "crtgl can't be empty" : GoTo selesai
        End If

        'crnorek(10) As String
        If Len(dataUtama(10)) = 0 Then
            result(2) = "crnorek can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(10)) > 25 Then
            result(2) = "crnorek should not be more than 25 character." : GoTo selesai
        End If

        'crmatauang(13) As String
        If Len(dataUtama(13)) = 0 Then
            result(2) = "crmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(13)) > 25 Then
            result(2) = "crmatauang should not be more than 25 character." : GoTo selesai
        End If

        'crkurs(14) As Double
        If Len(dataUtama(14)) = 0 Then
            result(2) = "crkurs can't be empty" : GoTo selesai
        End If

        'crjumlah(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "crjumlah can't be empty" : GoTo selesai
        End If

        'crjumlahvalas(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "crjumlahvalas can't be empty" : GoTo selesai
        End If

        'crjumlahbayar(17) As Double
        If Len(dataUtama(17)) = 0 Then
            result(2) = "crjumlahbayar can't be empty" : GoTo selesai
        End If

        'crjumlahbayarvalas(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "crjumlahbayarvalas can't be empty" : GoTo selesai
        End If

        'crinputtgl(27) As DateTime
        If Len(dataUtama(27)) = 0 Then
            result(2) = "crinputtgl can't be empty" : GoTo selesai
        End If

        'crmodifikasitgl(29) As DateTime
        If Len(dataUtama(29)) = 0 Then
            result(2) = "crmodifikasitgl can't be empty" : GoTo selesai
        End If

        'crcustomdbl1(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "crcustomdbl1 can't be empty" : GoTo selesai
        End If

        'crcustomdbl2(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "crcustomdbl2 can't be empty" : GoTo selesai
        End If

        'crcustomdbl3(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "crcustomdbl3 can't be empty" : GoTo selesai
        End If

        'crcustomdate1(42) As Date
        If Len(dataUtama(42)) = 0 Then
            result(2) = "crcustomdate1 can't be empty" : GoTo selesai
        End If

        'crcustomdate2(43) As Date
        If Len(dataUtama(43)) = 0 Then
            result(2) = "crcustomdate2 can't be empty" : GoTo selesai
        End If

        'crcustomdate3(44) As Date
        If Len(dataUtama(44)) = 0 Then
            result(2) = "crcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "crid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crnorek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cruraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crjumlah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crjumlahvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crjumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crjumlahbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crstatusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crtgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "crcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "crcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "crid~crcabang~crlokasi~crsumber~crautonotransaksi~crnotransaksi~crtgl~crkodepa~crkontak~crkontakperson~crnorek~cruraian~crcatatan~crmatauang~crkurs~crjumlah~crjumlahvalas~crjumlahbayar~crjumlahbayarvalas~crstatusbayar~crtgllunas~crstatus~crstatussebelumnya~crjmlrevisi~crcetakanke~crisclose~crinputuser~crinputtgl~crmodifikasiuser~crmodifikasitgl~crposting~crcustomtext1~crcustomtext2~crcustomtext3~crcustomtext4~crcustomtext5~crcustomint1~crcustomint2~crcustomint3~crcustomdbl1~crcustomdbl2~crcustomdbl3~crcustomdate1~crcustomdate2~crcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idcrdetail(0) As Integer, idcr(1) As Integer, norek(2) As String, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, catatan(7) As String, costcenter(8) As String, divisi(9) As String, 
        'subdivisi(10) As String, proyek(11) As String, urutan(12) As Integer, isclose(13) As Integer, customtext1(14) As String, 
        'customtext2(15) As String, customtext3(16) As String, customdbl1(17) As Double, customdbl2(18) As Double, customdbl3(19) As Double, 
        'customdate1(20) As Date, customdate2(21) As Date, customdate3(22) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idcrdetail, idcr, norek, matauang, kurs, jumlah, jumlahvalas, 
        'catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idcrdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idcr", AsEnumTypeData.AsInt64)
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
            'idcrdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idcrdetail required numeric." : GoTo selesai
            End If
            'idcr(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idcr required numeric." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idcrdetail~idcr~norek~matauang~kurs~jumlah~jumlahvalas~catatan~costcenter~divisi~subdivisi~proyek~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22)) = False Then
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("crtgl")), AsFormatTanggal(drutama("crtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "crmatauang", "crnorek", dtdetail, "norek")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                'CEK COA WAJIB COST CENTER ==============================
                If drutama("crstatus") = 2 Then
                    Dim cekCoaCostCenter As String = ValidasiCoaRequiredCostCenter(strRekCostCenter, dtdetail)
                    If Len(cekCoaCostCenter) > 0 Then
                        result(2) = cekCoaCostCenter : Trans.Rollback() : GoTo selesai
                    End If
                End If
                'END OF CEK COA WAJIB COST CENTER =======================


                'HITUNG TOTAL BERDASARKAN DATA DETAIL ===================
                drutama("crjumlah") = AsDataTableDSum(dtdetail, "jumlah")
                drutama("crjumlahvalas") = AsDataTableDSum(dtdetail, "jumlahvalas")
                'END OF HITUNG TOTAL BERDASARKAN DATA DETAIL ============

                If isUpdate Then
                    result(4) = drutama("crid")
                    notransaksi = drutama("crnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(crid), crnotransaksi FROM M2_Cr WHERE crid='" & result(4) & "' AND crstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(crid) FROM m2_cr WHERE crnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============


                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m2_cr_history
                        Dim rsSimpanHistory As String = SimpanHistory.M2_Cr_HistorySimpan("" & paramSplit(0) & "★M2_Cr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("crsumber")) & "▼" & FixQuotes(drutama("crid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================


                        sql = "Update M2_Cr set crcabang  = '" & FixQuotes(drutama("crcabang")) & "', crlokasi  = '" & FixQuotes(drutama("crlokasi")) & "', crsumber  = '" & FixQuotes(drutama("crsumber")) & "', crautonotransaksi  = " & drutama("crautonotransaksi") & ", crnotransaksi  = '" & notransaksi & "', crtgl  = '" & FixQuotes(AsFormatTanggal(drutama("crtgl"))) & "', crkodepa  = " & drutama("crkodepa") & ", crkontak  = " & drutama("crkontak") & ", crkontakperson  = '" & FixQuotes(drutama("crkontakperson")) & "', crnorek  = '" & FixQuotes(drutama("crnorek")) & "', cruraian  = '" & FixQuotes(drutama("cruraian")) & "', crcatatan  = '" & FixQuotes(drutama("crcatatan")) & "', crmatauang  = '" & FixQuotes(drutama("crmatauang")) & "', crkurs  = '" & FixDouble(drutama("crkurs")) & "', crjumlah  = '" & FixDouble(drutama("crjumlah")) & "', crjumlahvalas  = '" & FixDouble(drutama("crjumlahvalas")) & "', crjumlahbayar  = '" & FixDouble(drutama("crjumlahbayar")) & "', crjumlahbayarvalas  = '" & FixDouble(drutama("crjumlahbayarvalas")) & "', crstatusbayar  = " & drutama("crstatusbayar") & ", crtgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("crtgllunas"))) & "', crstatus  = " & drutama("crstatus") & ", crstatussebelumnya  = " & drutama("crstatussebelumnya") & ", crjmlrevisi  = crjmlrevisi+1, crcetakanke  = " & drutama("crcetakanke") & ", crisclose  = " & drutama("crisclose") & ", crmodifikasiuser  = " & drutama("crmodifikasiuser") & ", crmodifikasitgl  = NOW(), crposting  = 0, crcustomtext1  = '" & FixQuotes(drutama("crcustomtext1")) & "', crcustomtext2  = '" & FixQuotes(drutama("crcustomtext2")) & "', crcustomtext3  = '" & FixQuotes(drutama("crcustomtext3")) & "', crcustomtext4  = '" & FixQuotes(drutama("crcustomtext4")) & "', crcustomtext5  = '" & FixQuotes(drutama("crcustomtext5")) & "', crcustomint1  = " & drutama("crcustomint1") & ", crcustomint2  = " & drutama("crcustomint2") & ", crcustomint3  = " & drutama("crcustomint3") & ", crcustomdbl1  = '" & FixDouble(drutama("crcustomdbl1")) & "', crcustomdbl2  = '" & FixDouble(drutama("crcustomdbl2")) & "', crcustomdbl3  = '" & FixDouble(drutama("crcustomdbl3")) & "', crcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("crcustomdate1"))) & "', crcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("crcustomdate2"))) & "', crcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("crcustomdate3"))) & "' where crid = '" & drutama("crid") & "'"
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

                    If drutama("crautonotransaksi") = 1 Then
                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("crcabang"), drutama("crlokasi"), drutama("crsumber"), drutama("crtgl"))
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
                        notransaksi = drutama("crnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(crid) FROM m2_cr WHERE crnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M2_Cr (crcabang, crlokasi, crsumber, crautonotransaksi, crnotransaksi, crtgl, crkodepa, crkontak, crkontakperson, crnorek, cruraian, crcatatan, crmatauang, crkurs, crjumlah, crjumlahvalas, crjumlahbayar, crjumlahbayarvalas, crstatusbayar, crtgllunas, crstatus, crstatussebelumnya, crjmlrevisi, crcetakanke, crisclose, crinputuser, crinputtgl, crmodifikasiuser, crmodifikasitgl, crposting, crcustomtext1, crcustomtext2, crcustomtext3, crcustomtext4, crcustomtext5, crcustomint1, crcustomint2, crcustomint3, crcustomdbl1, crcustomdbl2, crcustomdbl3, crcustomdate1, crcustomdate2, crcustomdate3) values('" & FixQuotes(drutama("crcabang")) & "', '" & FixQuotes(drutama("crlokasi")) & "', '" & FixQuotes(drutama("crsumber")) & "', " & drutama("crautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("crtgl"))) & "', " & drutama("crkodepa") & ", " & drutama("crkontak") & ", '" & FixQuotes(drutama("crkontakperson")) & "', '" & FixQuotes(drutama("crnorek")) & "', '" & FixQuotes(drutama("cruraian")) & "', '" & FixQuotes(drutama("crcatatan")) & "', '" & FixQuotes(drutama("crmatauang")) & "', '" & FixDouble(drutama("crkurs")) & "', '" & FixDouble(drutama("crjumlah")) & "', '" & FixDouble(drutama("crjumlahvalas")) & "', '" & FixDouble(drutama("crjumlahbayar")) & "', '" & FixDouble(drutama("crjumlahbayarvalas")) & "', " & drutama("crstatusbayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("crtgllunas"))) & "', " & drutama("crstatus") & ", " & drutama("crstatussebelumnya") & ", " & drutama("crjmlrevisi") & ", " & drutama("crcetakanke") & ", " & drutama("crisclose") & ", " & drutama("crinputuser") & ", NOW(), " & drutama("crmodifikasiuser") & ", '1971-01-01 00:00:00', 0, '" & FixQuotes(drutama("crcustomtext1")) & "', '" & FixQuotes(drutama("crcustomtext2")) & "', '" & FixQuotes(drutama("crcustomtext3")) & "', '" & FixQuotes(drutama("crcustomtext4")) & "', '" & FixQuotes(drutama("crcustomtext5")) & "', " & drutama("crcustomint1") & ", " & drutama("crcustomint2") & ", " & drutama("crcustomint3") & ", '" & FixDouble(drutama("crcustomdbl1")) & "', '" & FixDouble(drutama("crcustomdbl2")) & "', '" & FixDouble(drutama("crcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("crcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("crcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("crcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select crid from M2_Cr where crnotransaksi='" & notransaksi & "' AND Crinputuser= '" & userid & "' order by Crmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M2_Cr_Detail where idcr = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idcrdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("norek")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M2_Cr_Detail(idcrdetail, idcr, norek, matauang, kurs, jumlah, jumlahvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                Dim sumber As String = "CR", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("crstatus") = 2 Then
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
    Public Function M2_CrUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("crkontakkode", "c.kkode")
            Filter = Filter.Replace("crkontaknama", "c.knama")
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
            Dim sumber As String = "Cr", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Crtgl, Crnotransaksi, Crstatus FROM m2_Cr WHERE Crid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Crstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m2_cr_history
            Dim rsSimpanHistory As String = SimpanHistory.M2_Cr_HistorySimpan("" & paramSplit(0) & "★M2_Cr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================


            If isDelete Then
                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'CR' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M2_Cr SET Crstatus = " & nilaiStatus & ", crmodifikasiuser='" & userid & "', crmodifikasitgl = NOW(), crposting = 0, crpostingtgl = '1971-01-01 00:00:00', Crjmlrevisi = Crjmlrevisi + 1 WHERE crid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_CrSearch(PostWsSearch(paramSplit(0), "M2_CrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_CrDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("crkontakkode", "c.kkode")
            Filter = Filter.Replace("crkontaknama", "c.knama")
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
            Dim sumber As String = "CR", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT crid, crnotransaksi FROM m2_cr WHERE crid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT crcabang, crlokasi, crsumber, crautonotransaksi, crnotransaksi, crtgl"
            sql &= " FROM M2_cr"
            sql &= " WHERE crid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("crcabang")
                lokasi = dtNomorNext.Rows(0)("crlokasi")
                sumber = dtNomorNext.Rows(0)("crsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("crautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("crnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("crtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE JURNAL
            sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'CR' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M2_Cr_Detail WHERE idcr = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M2_Cr WHERE crid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_CrSearch(PostWsSearch(paramSplit(0), "M2_CrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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