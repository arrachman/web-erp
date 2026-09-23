Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m2_cd
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M2_CdSimpan(ByVal param As String) As String
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
        'cdid(0) As Integer, cdcabang(1) As String, cdlokasi(2) As String, cdsumber(3) As String, cdautonotransaksi(4) As Integer, 
        'cdnotransaksi(5) As String, cdtgl(6) As Date, cdkodepa(7) As Integer, cdkontak(8) As Integer, cdkontakperson(9) As String, 
        'cdnorek(10) As String, cduraian(11) As String, cdcatatan(12) As String, cdmatauang(13) As String, cdkurs(14) As Double, 
        'cdjumlah(15) As Double, cdjumlahvalas(16) As Double, cdjumlahbayar(17) As Double, cdjumlahbayarvalas(18) As Double, cdstatusbayar(19) As Integer, 
        'cdtgllunas(20) As Date, cdstatus(21) As Integer, cdstatussebelumnya(22) As Integer, cdjmlrevisi(23) As Integer, cdcetakanke(24) As Integer, 
        'cdisclose(25) As Integer, cdinputuser(26) As Integer, cdinputtgl(27) As DateTime, cdmodifikasiuser(28) As Integer, cdmodifikasitgl(29) As DateTime, 
        'cdposting(30) As Integer, cdcustomtext1(31) As String, cdcustomtext2(32) As String, cdcustomtext3(33) As String, cdcustomtext4(34) As String, 
        'cdcustomtext5(35) As String, cdcustomint1(36) As Integer, cdcustomint2(37) As Integer, cdcustomint3(38) As Integer, cdcustomdbl1(39) As Double, 
        'cdcustomdbl2(40) As Double, cdcustomdbl3(41) As Double, cdcustomdate1(42) As Date, cdcustomdate2(43) As Date, cdcustomdate3(44) As Date


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'cdid, cdcabang, cdlokasi, cdsumber, cdautonotransaksi, cdnotransaksi, cdtgl, 
        'cdkodepa, cdkontak, cdkontakperson, cdnorek, cduraian, cdcatatan, cdmatauang, 
        'cdkurs, cdjumlah, cdjumlahvalas, cdjumlahbayar, cdjumlahbayarvalas, cdstatusbayar, cdtgllunas, 
        'cdstatus, cdstatussebelumnya, cdjmlrevisi, cdcetakanke, cdisclose, cdinputuser, cdinputtgl, 
        'cdmodifikasiuser, cdmodifikasitgl, cdposting, cdcustomtext1, cdcustomtext2, cdcustomtext3, cdcustomtext4, 
        'cdcustomtext5, cdcustomint1, cdcustomint2, cdcustomint3, cdcustomdbl1, cdcustomdbl2, cdcustomdbl3, 
        'cdcustomdate1, cdcustomdate2, cdcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 45) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'cdid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "cdid required numeric." : GoTo selesai
        End If
        'cdautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "cdautonotransaksi required numeric." : GoTo selesai
        End If
        'cdtgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "cdtgl required date." : GoTo selesai
        End If
        'cdkodepa(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "cdkodepa required numeric." : GoTo selesai
        End If
        'cdkontak(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "cdkontak required numeric." : GoTo selesai
        End If
        If (dataUtama(8) < 1) Then
            result(2) = "cdkontak can't be empty." : GoTo selesai
        End If
        'cdkurs(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "cdkurs required numeric." : GoTo selesai
        End If
        'cdjumlah(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "cdjumlah required numeric." : GoTo selesai
        End If
        'cdjumlahvalas(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "cdjumlahvalas required numeric." : GoTo selesai
        End If
        'cdjumlahbayar(17) As Double
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "cdjumlahbayar required numeric." : GoTo selesai
        End If
        'cdjumlahbayarvalas(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "cdjumlahbayarvalas required numeric." : GoTo selesai
        End If
        'cdstatusbayar(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "cdstatusbayar required numeric." : GoTo selesai
        End If
        'cdtgllunas(20) As Date
        If (IsDate(dataUtama(20)) = False) Then
            result(2) = "cdtgllunas required date." : GoTo selesai
        End If
        'cdstatus(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "cdstatus required numeric." : GoTo selesai
        End If
        'cdstatussebelumnya(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "cdstatussebelumnya required numeric." : GoTo selesai
        End If
        'cdjmlrevisi(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "cdjmlrevisi required numeric." : GoTo selesai
        End If
        'cdcetakanke(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "cdcetakanke required numeric." : GoTo selesai
        End If
        'cdisclose(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "cdisclose required numeric." : GoTo selesai
        End If
        'cdinputuser(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "cdinputuser required numeric." : GoTo selesai
        End If
        'cdinputtgl(27) As DateTime
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "cdinputtgl required date." : GoTo selesai
        End If
        'cdmodifikasiuser(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "cdmodifikasiuser required numeric." : GoTo selesai
        End If
        'cdmodifikasitgl(29) As DateTime
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "cdmodifikasitgl required date." : GoTo selesai
        End If
        'cdposting(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "cdposting required numeric." : GoTo selesai
        End If
        'cdcustomint1(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "cdcustomint1 required numeric." : GoTo selesai
        End If
        'cdcustomint2(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "cdcustomint2 required numeric." : GoTo selesai
        End If
        'cdcustomint3(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "cdcustomint3 required numeric." : GoTo selesai
        End If
        'cdcustomdbl1(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "cdcustomdbl1 required numeric." : GoTo selesai
        End If
        'cdcustomdbl2(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "cdcustomdbl2 required numeric." : GoTo selesai
        End If
        'cdcustomdbl3(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "cdcustomdbl3 required numeric." : GoTo selesai
        End If
        'cdcustomdate1(42) As Date
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "cdcustomdate1 required date." : GoTo selesai
        End If
        'cdcustomdate2(43) As Date
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "cdcustomdate2 required date." : GoTo selesai
        End If
        'cdcustomdate3(44) As Date
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "cdcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'cdcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "cdcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "cdcabang should not be more than 25 character." : GoTo selesai
        End If

        'cdlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "cdlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "cdlokasi should not be more than 25 character." : GoTo selesai
        End If

        'cdsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "cdsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "cdsumber should not be more than 10 character." : GoTo selesai
        End If

        'cdnotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "cdnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "cdnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'cdtgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "cdtgl can't be empty" : GoTo selesai
        End If

        'cdnorek(10) As String
        If Len(dataUtama(10)) = 0 Then
            result(2) = "cdnorek can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(10)) > 25 Then
            result(2) = "cdnorek should not be more than 25 character." : GoTo selesai
        End If

        'cdmatauang(13) As String
        If Len(dataUtama(13)) = 0 Then
            result(2) = "cdmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(13)) > 25 Then
            result(2) = "cdmatauang should not be more than 25 character." : GoTo selesai
        End If

        'cdkurs(14) As Double
        If Len(dataUtama(14)) = 0 Then
            result(2) = "cdkurs can't be empty" : GoTo selesai
        End If

        'cdjumlah(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "cdjumlah can't be empty" : GoTo selesai
        End If

        'cdjumlahvalas(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "cdjumlahvalas can't be empty" : GoTo selesai
        End If

        'cdjumlahbayar(17) As Double
        If Len(dataUtama(17)) = 0 Then
            result(2) = "cdjumlahbayar can't be empty" : GoTo selesai
        End If

        'cdjumlahbayarvalas(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "cdjumlahbayarvalas can't be empty" : GoTo selesai
        End If

        'cdinputtgl(27) As DateTime
        If Len(dataUtama(27)) = 0 Then
            result(2) = "cdinputtgl can't be empty" : GoTo selesai
        End If

        'cdmodifikasitgl(29) As DateTime
        If Len(dataUtama(29)) = 0 Then
            result(2) = "cdmodifikasitgl can't be empty" : GoTo selesai
        End If

        'cdcustomdbl1(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "cdcustomdbl1 can't be empty" : GoTo selesai
        End If

        'cdcustomdbl2(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "cdcustomdbl2 can't be empty" : GoTo selesai
        End If

        'cdcustomdbl3(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "cdcustomdbl3 can't be empty" : GoTo selesai
        End If

        'cdcustomdate1(42) As Date
        If Len(dataUtama(42)) = 0 Then
            result(2) = "cdcustomdate1 can't be empty" : GoTo selesai
        End If

        'cdcustomdate2(43) As Date
        If Len(dataUtama(43)) = 0 Then
            result(2) = "cdcustomdate2 can't be empty" : GoTo selesai
        End If

        'cdcustomdate3(44) As Date
        If Len(dataUtama(44)) = 0 Then
            result(2) = "cdcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "cdid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdnorek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cduraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdjumlah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdjumlahvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdjumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdjumlahbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdstatusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdtgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "cdid~cdcabang~cdlokasi~cdsumber~cdautonotransaksi~cdnotransaksi~cdtgl~cdkodepa~cdkontak~cdkontakperson~cdnorek~cduraian~cdcatatan~cdmatauang~cdkurs~cdjumlah~cdjumlahvalas~cdjumlahbayar~cdjumlahbayarvalas~cdstatusbayar~cdtgllunas~cdstatus~cdstatussebelumnya~cdjmlrevisi~cdcetakanke~cdisclose~cdinputuser~cdinputtgl~cdmodifikasiuser~cdmodifikasitgl~cdposting~cdcustomtext1~cdcustomtext2~cdcustomtext3~cdcustomtext4~cdcustomtext5~cdcustomint1~cdcustomint2~cdcustomint3~cdcustomdbl1~cdcustomdbl2~cdcustomdbl3~cdcustomdate1~cdcustomdate2~cdcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idcddetail(0) As Integer, idcd(1) As Integer, norek(2) As String, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, catatan(7) As String, costcenter(8) As String, divisi(9) As String, 
        'subdivisi(10) As String, proyek(11) As String, urutan(12) As Integer, isclose(13) As Integer, customtext1(14) As String, 
        'customtext2(15) As String, customtext3(16) As String, customdbl1(17) As Double, customdbl2(18) As Double, customdbl3(19) As Double, 
        'customdate1(20) As Date, customdate2(21) As Date, customdate3(22) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idcddetail, idcd, norek, matauang, kurs, jumlah, jumlahvalas, 
        'catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idcddetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idcd", AsEnumTypeData.AsInt64)
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
            'idcddetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idcddetail required numeric." : GoTo selesai
            End If
            'idcd(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idcd required numeric." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idcddetail~idcd~norek~matauang~kurs~jumlah~jumlahvalas~catatan~costcenter~divisi~subdivisi~proyek~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22)) = False Then
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
                Dim vModuleId As Integer = 2, vMenuId As Integer = 4
                Select Case drutama("cdstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("cdtgl")), AsFormatTanggal(drutama("cdtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "cdmatauang", "cdnorek", dtdetail, "norek")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                'CEK COA WAJIB COST CENTER ==============================
                If drutama("cdstatus") = 2 Then
                    Dim cekCoaCostCenter As String = ValidasiCoaRequiredCostCenter(strRekCostCenter, dtdetail)
                    If Len(cekCoaCostCenter) > 0 Then
                        result(2) = cekCoaCostCenter : Trans.Rollback() : GoTo selesai
                    End If
                End If
                'END OF CEK COA WAJIB COST CENTER =======================


                'HITUNG TOTAL BERDASARKAN DATA DETAIL ===================
                drutama("cdjumlah") = AsDataTableDSum(dtdetail, "jumlah")
                drutama("cdjumlahvalas") = AsDataTableDSum(dtdetail, "jumlahvalas")
                'END OF HITUNG TOTAL BERDASARKAN DATA DETAIL ============

                If isUpdate Then
                    result(4) = drutama("cdid")
                    notransaksi = drutama("cdnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(cdid), cdnotransaksi FROM M2_Cd WHERE cdid='" & result(4) & "' AND cdstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("cdautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("cdcabang"), drutama("cdlokasi"), drutama("cdsumber"), drutama("cdtgl"), drutama("cdsumber"), 2)
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(cdid) FROM m2_cd WHERE cdnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m2_cd_history
                        Dim rsSimpanHistory As String = SimpanHistory.M2_Cd_HistorySimpan("" & paramSplit(0) & "★M2_Cd_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("cdsumber")) & "▼" & FixQuotes(drutama("cdid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M2_Cd set cdcabang  = '" & FixQuotes(drutama("cdcabang")) & "', cdlokasi  = '" & FixQuotes(drutama("cdlokasi")) & "', cdsumber  = '" & FixQuotes(drutama("cdsumber")) & "', cdautonotransaksi  = " & drutama("cdautonotransaksi") & ", cdnotransaksi  = '" & notransaksi & "', cdtgl  = '" & FixQuotes(AsFormatTanggal(drutama("cdtgl"))) & "', cdkodepa  = " & drutama("cdkodepa") & ", cdkontak  = " & drutama("cdkontak") & ", cdkontakperson  = '" & FixQuotes(drutama("cdkontakperson")) & "', cdnorek  = '" & FixQuotes(drutama("cdnorek")) & "', cduraian  = '" & FixQuotes(drutama("cduraian")) & "', cdcatatan  = '" & FixQuotes(drutama("cdcatatan")) & "', cdmatauang  = '" & FixQuotes(drutama("cdmatauang")) & "', cdkurs  = '" & FixDouble(drutama("cdkurs")) & "', cdjumlah  = '" & FixDouble(drutama("cdjumlah")) & "', cdjumlahvalas  = '" & FixDouble(drutama("cdjumlahvalas")) & "', cdjumlahbayar  = '" & FixDouble(drutama("cdjumlahbayar")) & "', cdjumlahbayarvalas  = '" & FixDouble(drutama("cdjumlahbayarvalas")) & "', cdstatusbayar  = " & drutama("cdstatusbayar") & ", cdtgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("cdtgllunas"))) & "', cdstatus  = " & drutama("cdstatus") & ", cdstatussebelumnya  = " & drutama("cdstatussebelumnya") & ", cdjmlrevisi  = cdjmlrevisi + 1, cdcetakanke  = " & drutama("cdcetakanke") & ", cdisclose  = " & drutama("cdisclose") & ", cdmodifikasiuser  = " & drutama("cdmodifikasiuser") & ", cdmodifikasitgl  = NOW(), cdposting  = 0, cdcustomtext1  = '" & FixQuotes(drutama("cdcustomtext1")) & "', cdcustomtext2  = '" & FixQuotes(drutama("cdcustomtext2")) & "', cdcustomtext3  = '" & FixQuotes(drutama("cdcustomtext3")) & "', cdcustomtext4  = '" & FixQuotes(drutama("cdcustomtext4")) & "', cdcustomtext5  = '" & FixQuotes(drutama("cdcustomtext5")) & "', cdcustomint1  = " & drutama("cdcustomint1") & ", cdcustomint2  = " & drutama("cdcustomint2") & ", cdcustomint3  = " & drutama("cdcustomint3") & ", cdcustomdbl1  = '" & FixDouble(drutama("cdcustomdbl1")) & "', cdcustomdbl2  = '" & FixDouble(drutama("cdcustomdbl2")) & "', cdcustomdbl3  = '" & FixDouble(drutama("cdcustomdbl3")) & "', cdcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("cdcustomdate1"))) & "', cdcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("cdcustomdate2"))) & "', cdcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("cdcustomdate3"))) & "' where cdid = '" & drutama("cdid") & "'"
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

                    If drutama("cdautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("cdcabang"), drutama("cdlokasi"), drutama("cdsumber"), drutama("cdtgl"), drutama("cdsumber"), 2)
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
                        notransaksi = drutama("cdnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(cdid) FROM m2_cd WHERE cdnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M2_Cd (cdcabang, cdlokasi, cdsumber, cdautonotransaksi, cdnotransaksi, cdtgl, cdkodepa, cdkontak, cdkontakperson, cdnorek, cduraian, cdcatatan, cdmatauang, cdkurs, cdjumlah, cdjumlahvalas, cdjumlahbayar, cdjumlahbayarvalas, cdstatusbayar, cdtgllunas, cdstatus, cdstatussebelumnya, cdjmlrevisi, cdcetakanke, cdisclose, cdinputuser, cdinputtgl, cdmodifikasiuser, cdmodifikasitgl, cdposting, cdcustomtext1, cdcustomtext2, cdcustomtext3, cdcustomtext4, cdcustomtext5, cdcustomint1, cdcustomint2, cdcustomint3, cdcustomdbl1, cdcustomdbl2, cdcustomdbl3, cdcustomdate1, cdcustomdate2, cdcustomdate3) values('" & FixQuotes(drutama("cdcabang")) & "', '" & FixQuotes(drutama("cdlokasi")) & "', '" & FixQuotes(drutama("cdsumber")) & "', " & drutama("cdautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("cdtgl"))) & "', " & drutama("cdkodepa") & ", " & drutama("cdkontak") & ", '" & FixQuotes(drutama("cdkontakperson")) & "', '" & FixQuotes(drutama("cdnorek")) & "', '" & FixQuotes(drutama("cduraian")) & "', '" & FixQuotes(drutama("cdcatatan")) & "', '" & FixQuotes(drutama("cdmatauang")) & "', '" & FixDouble(drutama("cdkurs")) & "', '" & FixDouble(drutama("cdjumlah")) & "', '" & FixDouble(drutama("cdjumlahvalas")) & "', '" & FixDouble(drutama("cdjumlahbayar")) & "', '" & FixDouble(drutama("cdjumlahbayarvalas")) & "', " & drutama("cdstatusbayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("cdtgllunas"))) & "', " & drutama("cdstatus") & ", " & drutama("cdstatussebelumnya") & ", " & drutama("cdjmlrevisi") & ", " & drutama("cdcetakanke") & ", " & drutama("cdisclose") & ", " & drutama("cdinputuser") & ", NOW(), " & drutama("cdmodifikasiuser") & ", '1971-01-01 00:00:00', 0, '" & FixQuotes(drutama("cdcustomtext1")) & "', '" & FixQuotes(drutama("cdcustomtext2")) & "', '" & FixQuotes(drutama("cdcustomtext3")) & "', '" & FixQuotes(drutama("cdcustomtext4")) & "', '" & FixQuotes(drutama("cdcustomtext5")) & "', " & drutama("cdcustomint1") & ", " & drutama("cdcustomint2") & ", " & drutama("cdcustomint3") & ", '" & FixDouble(drutama("cdcustomdbl1")) & "', '" & FixDouble(drutama("cdcustomdbl2")) & "', '" & FixDouble(drutama("cdcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("cdcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("cdcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("cdcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select cdid from M2_Cd where cdnotransaksi='" & notransaksi & "' AND Cdinputuser= '" & userid & "' order by Cdmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M2_Cd_Detail where idcd = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idcddetail") & ", " & result(4) & ", '" & FixQuotes(dr1("norek")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M2_Cd_Detail(idcddetail, idcd, norek, matauang, kurs, jumlah, jumlahvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                Dim sumber As String = "CD", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("cdstatus") = 2 Then
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
    Public Function M2_CdUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("cdkontakkode", "c.kkode")
            Filter = Filter.Replace("cdkontaknama", "c.knama")
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
            Dim sumber As String = "Cd", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Cdtgl, Cdnotransaksi, Cdstatus FROM m2_Cd WHERE Cdid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Cdstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m2_cd_history
            Dim rsSimpanHistory As String = SimpanHistory.M2_Cd_HistorySimpan("" & paramSplit(0) & "★M2_Cd_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then
                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'CD' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M2_Cd SET Cdstatus = " & nilaiStatus & ", Cdmodifikasiuser='" & userid & "', Cdmodifikasitgl = NOW(), Cdposting = 0, Cdpostingtgl = '1971-01-01 00:00:00', Cdjmlrevisi = Cdjmlrevisi + 1 WHERE Cdid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_CdSearch(PostWsSearch(paramSplit(0), "M2_CdSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_CdDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("cdkontakkode", "c.kkode")
            Filter = Filter.Replace("cdkontaknama", "c.knama")
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
            Dim sumber As String = "Cd", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Cdid, Cdnotransaksi FROM m2_Cd WHERE Cdid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT cdcabang, cdlokasi, cdsumber, cdautonotransaksi, cdnotransaksi, cdtgl"
            sql &= " FROM M2_cd"
            sql &= " WHERE cdid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("cdcabang")
                lokasi = dtNomorNext.Rows(0)("cdlokasi")
                sumber = dtNomorNext.Rows(0)("cdsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("cdautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("cdnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("cdtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE JURNAL
            sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'CD' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M2_Cd_Detail WHERE idCd = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M2_Cd WHERE Cdid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_CdSearch(PostWsSearch(paramSplit(0), "M2_CdSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_CdGetdataById(ByVal param As String) As String

        'M2_CdGetdataById Utama --------------------------------------------------------
        'cdid, cdcabang, cdlokasi, cdsumber, cdautonotransaksi, cdnotransaksi, cdtgl, 
        'cdkodepa, cdkontak, cdkontakperson, cdnorek, cduraian, cdcatatan, cdmatauang, 
        'cdkurs, cdjumlah, cdjumlahvalas, cdjumlahbayar, cdjumlahbayarvalas, cdstatusbayar, cdtgllunas, 
        'cdstatus, cdstatussebelumnya, cdjmlrevisi, cdcetakanke, cdisclose, cdinputuser, cdinputtgl, 
        'cdmodifikasiuser, cdmodifikasitgl, cdposting, cdpostingtgl, cdcustomtext1, cdcustomtext2, cdcustomtext3, 
        'cdcustomtext4, cdcustomtext5, cdcustomint1, cdcustomint2, cdcustomint3, cdcustomdbl1, cdcustomdbl2, 
        'cdcustomdbl3, cdcustomdate1, cdcustomdate2, cdcustomdate3, cdcabangnama, cdlokasinama, cdkontakkode, 
        'cdkontaknama, cdnoreknama, cdstatusnama, cdstatussebelumnyanama, cdinputusernama, cdmodifikasiusernama

        'M2_CdGetdataById Detail -------------------------------------------------------
        'idcddetail, idcd, 
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

        Dim NmMemcached As String = "aplikasi1-M2_Cd~M2_Cd_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "cdid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "cdid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_cd_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("cdid"), 0), sptField,
                     FxDB(drutama("cdcabang"), ""), sptField,
                     FxDB(drutama("cdlokasi"), ""), sptField,
                     FxDB(drutama("cdsumber"), ""), sptField,
                     FxDB(drutama("cdautonotransaksi"), 0), sptField,
                     FxDB(drutama("cdnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("cdtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("cdkodepa"), 0), sptField,
                     FxDB(drutama("cdkontak"), 0), sptField,
                     FxDB(drutama("cdkontakperson"), ""), sptField,
                     FxDB(drutama("cdnorek"), ""), sptField,
                     FxDB(drutama("cduraian"), ""), sptField,
                     FxDB(drutama("cdcatatan"), ""), sptField,
                     FxDB(drutama("cdmatauang"), ""), sptField,
                     FxDB(drutama("cdkurs"), 0), sptField,
                     FxDB(drutama("cdjumlah"), 0), sptField,
                     FxDB(drutama("cdjumlahvalas"), 0), sptField,
                     FxDB(drutama("cdjumlahbayar"), 0), sptField,
                     FxDB(drutama("cdjumlahbayarvalas"), 0), sptField,
                     FxDB(drutama("cdstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("cdtgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("cdstatus"), 0), sptField,
                     FxDB(drutama("cdstatussebelumnya"), 0), sptField,
                     FxDB(drutama("cdjmlrevisi"), 0), sptField,
                     FxDB(drutama("cdcetakanke"), 0), sptField,
                     FxDB(drutama("cdisclose"), 0), sptField,
                     FxDB(drutama("cdinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("cdinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("cdmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("cdmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("cdposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("cdpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("cdcustomtext1"), ""), sptField,
                     FxDB(drutama("cdcustomtext2"), ""), sptField,
                     FxDB(drutama("cdcustomtext3"), ""), sptField,
                     FxDB(drutama("cdcustomtext4"), ""), sptField,
                     FxDB(drutama("cdcustomtext5"), ""), sptField,
                     FxDB(drutama("cdcustomint1"), 0), sptField,
                     FxDB(drutama("cdcustomint2"), 0), sptField,
                     FxDB(drutama("cdcustomint3"), 0), sptField,
                     FxDB(drutama("cdcustomdbl1"), 0), sptField,
                     FxDB(drutama("cdcustomdbl2"), 0), sptField,
                     FxDB(drutama("cdcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("cdcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("cdcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("cdcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("cdcabangnama"), ""), sptField,
                     FxDB(drutama("cdlokasinama"), ""), sptField,
                     FxDB(drutama("cdkontakkode"), ""), sptField,
                     FxDB(drutama("cdkontaknama"), ""), sptField,
                     FxDB(drutama("cdnoreknama"), ""), sptField,
                     FxDB(drutama("cdstatusnama"), ""), sptField,
                     FxDB(drutama("cdstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("cdinputusernama"), ""), sptField,
                     FxDB(drutama("cdmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idcddetail"), 0), sptField,
                     FxDB(dr("idcd"), 0), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("cdid, cdcabang, cdlokasi, cdsumber, cdautonotransaksi, cdnotransaksi, cdtgl, cdkodepa, cdkontak, cdkontakperson, cdnorek, cduraian, cdcatatan, cdmatauang, cdkurs, cdjumlah, cdjumlahvalas, cdjumlahbayar, cdjumlahbayarvalas, cdstatusbayar, cdtgllunas, cdstatus, cdstatussebelumnya, cdjmlrevisi, cdcetakanke, cdisclose, cdinputuser, cdinputtgl, cdmodifikasiuser, cdmodifikasitgl, cdposting, cdpostingtgl, cdcustomtext1, cdcustomtext2, cdcustomtext3, cdcustomtext4, cdcustomtext5, cdcustomint1, cdcustomint2, cdcustomint3, cdcustomdbl1, cdcustomdbl2, cdcustomdbl3, cdcustomdate1, cdcustomdate2, cdcustomdate3, cdcabangnama, cdlokasinama, cdkontakkode, cdkontaknama, cdnoreknama, cdstatusnama, cdstatussebelumnyanama, cdinputusernama, cdmodifikasiusernama" & sptSubParam & "idcddetail, idcd, norek, matauang, kurs, jumlah, jumlahvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, noreknama, costcenternama, divisinama, subdivisinama, proyeknama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_CdSearch(ByVal param As String) As String
        'M2_CdSearch --------------------------------------------------------
        'cdid, cdcabang, cdlokasi, cdsumber, cdautonotransaksi, cdnotransaksi, cdtgl, 
        'cdkodepa, cdkontak, cdkontakperson, cdnorek, cduraian, cdcatatan, cdmatauang, 
        'cdkurs, cdjumlah, cdjumlahvalas, cdjumlahbayar, cdjumlahbayarvalas, cdstatusbayar, cdtgllunas, 
        'cdstatus, cdstatussebelumnya, cdjmlrevisi, cdcetakanke, cdisclose, cdinputuser, cdinputtgl, 
        'cdmodifikasiuser, cdmodifikasitgl, cdposting, cdpostingtgl, cdcabangnama, cdlokasinama, cdkontakkode, 
        'cdkontaknama, cdnoreknama, cdstatusnama, cdstatussebelumnyanama, cdinputusernama, cdmodifikasiusernama

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
            Filter = Filter.Replace("cdkontakkode", "c.kkode")
            Filter = Filter.Replace("cdkontaknama", "c.knama")
            Filter = Filter.Replace("Cdkontaknama", "c.knama")
            Filter = Filter.Replace("Cdstatusnama", "`st1`.`nama`")
            Filter = Filter.Replace("Cdinputusernama", "`u1`.`unama`")
            Filter = Filter.Replace("Cdmodifikasiusernama", "`u2`.`unama`")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_cd_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Cd", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("cdid"), 0), sptField,
                     FxDB(dr("cdcabang"), ""), sptField,
                     FxDB(dr("cdlokasi"), ""), sptField,
                     FxDB(dr("cdsumber"), ""), sptField,
                     FxDB(dr("cdautonotransaksi"), 0), sptField,
                     FxDB(dr("cdnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("cdtgl"), ""), formatTgl), sptField,
                     FxDB(dr("cdkodepa"), 0), sptField,
                     FxDB(dr("cdkontak"), 0), sptField,
                     FxDB(dr("cdkontakperson"), ""), sptField,
                     FxDB(dr("cdnorek"), ""), sptField,
                     FxDB(dr("cduraian"), ""), sptField,
                     FxDB(dr("cdcatatan"), ""), sptField,
                     FxDB(dr("cdmatauang"), ""), sptField,
                     FxDB(dr("cdkurs"), 0), sptField,
                     FxDB(dr("cdjumlah"), 0), sptField,
                     FxDB(dr("cdjumlahvalas"), 0), sptField,
                     FxDB(dr("cdjumlahbayar"), 0), sptField,
                     FxDB(dr("cdjumlahbayarvalas"), 0), sptField,
                     FxDB(dr("cdstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("cdtgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("cdstatus"), 0), sptField,
                     FxDB(dr("cdstatussebelumnya"), 0), sptField,
                     FxDB(dr("cdjmlrevisi"), 0), sptField,
                     FxDB(dr("cdcetakanke"), 0), sptField,
                     FxDB(dr("cdisclose"), 0), sptField,
                     FxDB(dr("cdinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("cdinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("cdmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("cdmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("cdposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("cdpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("cdcabangnama"), ""), sptField,
                     FxDB(dr("cdlokasinama"), ""), sptField,
                     FxDB(dr("cdkontakkode"), ""), sptField,
                     FxDB(dr("cdkontaknama"), ""), sptField,
                     FxDB(dr("cdnoreknama"), ""), sptField,
                     FxDB(dr("cdstatusnama"), ""), sptField,
                     FxDB(dr("cdstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("cdinputusernama"), ""), sptField,
                     FxDB(dr("cdmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("cdid, cdcabang, cdlokasi, cdsumber, cdautonotransaksi, cdnotransaksi, cdtgl, cdkodepa, cdkontak, cdkontakperson, cdnorek, cduraian, cdcatatan, cdmatauang, cdkurs, cdjumlah, cdjumlahvalas, cdjumlahbayar, cdjumlahbayarvalas, cdstatusbayar, cdtgllunas, cdstatus, cdstatussebelumnya, cdjmlrevisi, cdcetakanke, cdisclose, cdinputuser, cdinputtgl, cdmodifikasiuser, cdmodifikasitgl, cdposting, cdpostingtgl, cdcabangnama, cdlokasinama, cdkontakkode, cdkontaknama, cdnoreknama, cdstatusnama, cdstatussebelumnyanama, cdinputusernama, cdmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_CdTerkait(ByVal param As String) As String
        'M2_CdTerkait --------------------------------------------------------
        'cdid, cdnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
                     FxDB(dr("cdid"), 0), sptField,
                     FxDB(dr("cdnotransaksi"), ""), sptField,
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
            result(2) = "Related CD data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("cdid, cdnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_CdSimpanOld(ByVal param As String) As String
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
        'cdid(0) As Integer, cdcabang(1) As String, cdlokasi(2) As String, cdsumber(3) As String, cdautonotransaksi(4) As Integer, 
        'cdnotransaksi(5) As String, cdtgl(6) As Date, cdkodepa(7) As Integer, cdkontak(8) As Integer, cdkontakperson(9) As String, 
        'cdnorek(10) As String, cduraian(11) As String, cdcatatan(12) As String, cdmatauang(13) As String, cdkurs(14) As Double, 
        'cdjumlah(15) As Double, cdjumlahvalas(16) As Double, cdjumlahbayar(17) As Double, cdjumlahbayarvalas(18) As Double, cdstatusbayar(19) As Integer, 
        'cdtgllunas(20) As Date, cdstatus(21) As Integer, cdstatussebelumnya(22) As Integer, cdjmlrevisi(23) As Integer, cdcetakanke(24) As Integer, 
        'cdisclose(25) As Integer, cdinputuser(26) As Integer, cdinputtgl(27) As DateTime, cdmodifikasiuser(28) As Integer, cdmodifikasitgl(29) As DateTime, 
        'cdposting(30) As Integer, cdcustomtext1(31) As String, cdcustomtext2(32) As String, cdcustomtext3(33) As String, cdcustomtext4(34) As String, 
        'cdcustomtext5(35) As String, cdcustomint1(36) As Integer, cdcustomint2(37) As Integer, cdcustomint3(38) As Integer, cdcustomdbl1(39) As Double, 
        'cdcustomdbl2(40) As Double, cdcustomdbl3(41) As Double, cdcustomdate1(42) As Date, cdcustomdate2(43) As Date, cdcustomdate3(44) As Date


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'cdid, cdcabang, cdlokasi, cdsumber, cdautonotransaksi, cdnotransaksi, cdtgl, 
        'cdkodepa, cdkontak, cdkontakperson, cdnorek, cduraian, cdcatatan, cdmatauang, 
        'cdkurs, cdjumlah, cdjumlahvalas, cdjumlahbayar, cdjumlahbayarvalas, cdstatusbayar, cdtgllunas, 
        'cdstatus, cdstatussebelumnya, cdjmlrevisi, cdcetakanke, cdisclose, cdinputuser, cdinputtgl, 
        'cdmodifikasiuser, cdmodifikasitgl, cdposting, cdcustomtext1, cdcustomtext2, cdcustomtext3, cdcustomtext4, 
        'cdcustomtext5, cdcustomint1, cdcustomint2, cdcustomint3, cdcustomdbl1, cdcustomdbl2, cdcustomdbl3, 
        'cdcustomdate1, cdcustomdate2, cdcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 45) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'cdid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "cdid required numeric." : GoTo selesai
        End If
        'cdautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "cdautonotransaksi required numeric." : GoTo selesai
        End If
        'cdtgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "cdtgl required date." : GoTo selesai
        End If
        'cdkodepa(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "cdkodepa required numeric." : GoTo selesai
        End If
        'cdkontak(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "cdkontak required numeric." : GoTo selesai
        End If
        If (dataUtama(8) < 1) Then
            result(2) = "cdkontak can't be empty." : GoTo selesai
        End If
        'cdkurs(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "cdkurs required numeric." : GoTo selesai
        End If
        'cdjumlah(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "cdjumlah required numeric." : GoTo selesai
        End If
        'cdjumlahvalas(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "cdjumlahvalas required numeric." : GoTo selesai
        End If
        'cdjumlahbayar(17) As Double
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "cdjumlahbayar required numeric." : GoTo selesai
        End If
        'cdjumlahbayarvalas(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "cdjumlahbayarvalas required numeric." : GoTo selesai
        End If
        'cdstatusbayar(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "cdstatusbayar required numeric." : GoTo selesai
        End If
        'cdtgllunas(20) As Date
        If (IsDate(dataUtama(20)) = False) Then
            result(2) = "cdtgllunas required date." : GoTo selesai
        End If
        'cdstatus(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "cdstatus required numeric." : GoTo selesai
        End If
        'cdstatussebelumnya(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "cdstatussebelumnya required numeric." : GoTo selesai
        End If
        'cdjmlrevisi(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "cdjmlrevisi required numeric." : GoTo selesai
        End If
        'cdcetakanke(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "cdcetakanke required numeric." : GoTo selesai
        End If
        'cdisclose(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "cdisclose required numeric." : GoTo selesai
        End If
        'cdinputuser(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "cdinputuser required numeric." : GoTo selesai
        End If
        'cdinputtgl(27) As DateTime
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "cdinputtgl required date." : GoTo selesai
        End If
        'cdmodifikasiuser(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "cdmodifikasiuser required numeric." : GoTo selesai
        End If
        'cdmodifikasitgl(29) As DateTime
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "cdmodifikasitgl required date." : GoTo selesai
        End If
        'cdposting(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "cdposting required numeric." : GoTo selesai
        End If
        'cdcustomint1(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "cdcustomint1 required numeric." : GoTo selesai
        End If
        'cdcustomint2(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "cdcustomint2 required numeric." : GoTo selesai
        End If
        'cdcustomint3(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "cdcustomint3 required numeric." : GoTo selesai
        End If
        'cdcustomdbl1(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "cdcustomdbl1 required numeric." : GoTo selesai
        End If
        'cdcustomdbl2(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "cdcustomdbl2 required numeric." : GoTo selesai
        End If
        'cdcustomdbl3(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "cdcustomdbl3 required numeric." : GoTo selesai
        End If
        'cdcustomdate1(42) As Date
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "cdcustomdate1 required date." : GoTo selesai
        End If
        'cdcustomdate2(43) As Date
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "cdcustomdate2 required date." : GoTo selesai
        End If
        'cdcustomdate3(44) As Date
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "cdcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'cdcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "cdcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "cdcabang should not be more than 25 character." : GoTo selesai
        End If

        'cdlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "cdlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "cdlokasi should not be more than 25 character." : GoTo selesai
        End If

        'cdsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "cdsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "cdsumber should not be more than 10 character." : GoTo selesai
        End If

        'cdnotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "cdnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "cdnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'cdtgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "cdtgl can't be empty" : GoTo selesai
        End If

        'cdnorek(10) As String
        If Len(dataUtama(10)) = 0 Then
            result(2) = "cdnorek can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(10)) > 25 Then
            result(2) = "cdnorek should not be more than 25 character." : GoTo selesai
        End If

        'cdmatauang(13) As String
        If Len(dataUtama(13)) = 0 Then
            result(2) = "cdmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(13)) > 25 Then
            result(2) = "cdmatauang should not be more than 25 character." : GoTo selesai
        End If

        'cdkurs(14) As Double
        If Len(dataUtama(14)) = 0 Then
            result(2) = "cdkurs can't be empty" : GoTo selesai
        End If

        'cdjumlah(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "cdjumlah can't be empty" : GoTo selesai
        End If

        'cdjumlahvalas(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "cdjumlahvalas can't be empty" : GoTo selesai
        End If

        'cdjumlahbayar(17) As Double
        If Len(dataUtama(17)) = 0 Then
            result(2) = "cdjumlahbayar can't be empty" : GoTo selesai
        End If

        'cdjumlahbayarvalas(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "cdjumlahbayarvalas can't be empty" : GoTo selesai
        End If

        'cdinputtgl(27) As DateTime
        If Len(dataUtama(27)) = 0 Then
            result(2) = "cdinputtgl can't be empty" : GoTo selesai
        End If

        'cdmodifikasitgl(29) As DateTime
        If Len(dataUtama(29)) = 0 Then
            result(2) = "cdmodifikasitgl can't be empty" : GoTo selesai
        End If

        'cdcustomdbl1(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "cdcustomdbl1 can't be empty" : GoTo selesai
        End If

        'cdcustomdbl2(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "cdcustomdbl2 can't be empty" : GoTo selesai
        End If

        'cdcustomdbl3(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "cdcustomdbl3 can't be empty" : GoTo selesai
        End If

        'cdcustomdate1(42) As Date
        If Len(dataUtama(42)) = 0 Then
            result(2) = "cdcustomdate1 can't be empty" : GoTo selesai
        End If

        'cdcustomdate2(43) As Date
        If Len(dataUtama(43)) = 0 Then
            result(2) = "cdcustomdate2 can't be empty" : GoTo selesai
        End If

        'cdcustomdate3(44) As Date
        If Len(dataUtama(44)) = 0 Then
            result(2) = "cdcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "cdid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdnorek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cduraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdjumlah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdjumlahvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdjumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdjumlahbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdstatusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdtgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cdcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cdcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "cdid~cdcabang~cdlokasi~cdsumber~cdautonotransaksi~cdnotransaksi~cdtgl~cdkodepa~cdkontak~cdkontakperson~cdnorek~cduraian~cdcatatan~cdmatauang~cdkurs~cdjumlah~cdjumlahvalas~cdjumlahbayar~cdjumlahbayarvalas~cdstatusbayar~cdtgllunas~cdstatus~cdstatussebelumnya~cdjmlrevisi~cdcetakanke~cdisclose~cdinputuser~cdinputtgl~cdmodifikasiuser~cdmodifikasitgl~cdposting~cdcustomtext1~cdcustomtext2~cdcustomtext3~cdcustomtext4~cdcustomtext5~cdcustomint1~cdcustomint2~cdcustomint3~cdcustomdbl1~cdcustomdbl2~cdcustomdbl3~cdcustomdate1~cdcustomdate2~cdcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idcddetail(0) As Integer, idcd(1) As Integer, norek(2) As String, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, catatan(7) As String, costcenter(8) As String, divisi(9) As String, 
        'subdivisi(10) As String, proyek(11) As String, urutan(12) As Integer, isclose(13) As Integer, customtext1(14) As String, 
        'customtext2(15) As String, customtext3(16) As String, customdbl1(17) As Double, customdbl2(18) As Double, customdbl3(19) As Double, 
        'customdate1(20) As Date, customdate2(21) As Date, customdate3(22) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idcddetail, idcd, norek, matauang, kurs, jumlah, jumlahvalas, 
        'catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idcddetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idcd", AsEnumTypeData.AsInt64)
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
            'idcddetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idcddetail required numeric." : GoTo selesai
            End If
            'idcd(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idcd required numeric." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idcddetail~idcd~norek~matauang~kurs~jumlah~jumlahvalas~catatan~costcenter~divisi~subdivisi~proyek~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22)) = False Then
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("cdtgl")), AsFormatTanggal(drutama("cdtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "cdmatauang", "cdnorek", dtdetail, "norek")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                'CEK COA WAJIB COST CENTER ==============================
                If drutama("cdstatus") = 2 Then
                    Dim cekCoaCostCenter As String = ValidasiCoaRequiredCostCenter(strRekCostCenter, dtdetail)
                    If Len(cekCoaCostCenter) > 0 Then
                        result(2) = cekCoaCostCenter : Trans.Rollback() : GoTo selesai
                    End If
                End If
                'END OF CEK COA WAJIB COST CENTER =======================


                'HITUNG TOTAL BERDASARKAN DATA DETAIL ===================
                drutama("cdjumlah") = AsDataTableDSum(dtdetail, "jumlah")
                drutama("cdjumlahvalas") = AsDataTableDSum(dtdetail, "jumlahvalas")
                'END OF HITUNG TOTAL BERDASARKAN DATA DETAIL ============

                If isUpdate Then
                    result(4) = drutama("cdid")
                    notransaksi = drutama("cdnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(cdid), cdnotransaksi FROM M2_Cd WHERE cdid='" & result(4) & "' AND cdstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(cdid) FROM m2_cd WHERE cdnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m2_cd_history
                        Dim rsSimpanHistory As String = SimpanHistory.M2_Cd_HistorySimpan("" & paramSplit(0) & "★M2_Cd_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("cdsumber")) & "▼" & FixQuotes(drutama("cdid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M2_Cd set cdcabang  = '" & FixQuotes(drutama("cdcabang")) & "', cdlokasi  = '" & FixQuotes(drutama("cdlokasi")) & "', cdsumber  = '" & FixQuotes(drutama("cdsumber")) & "', cdautonotransaksi  = " & drutama("cdautonotransaksi") & ", cdnotransaksi  = '" & notransaksi & "', cdtgl  = '" & FixQuotes(AsFormatTanggal(drutama("cdtgl"))) & "', cdkodepa  = " & drutama("cdkodepa") & ", cdkontak  = " & drutama("cdkontak") & ", cdkontakperson  = '" & FixQuotes(drutama("cdkontakperson")) & "', cdnorek  = '" & FixQuotes(drutama("cdnorek")) & "', cduraian  = '" & FixQuotes(drutama("cduraian")) & "', cdcatatan  = '" & FixQuotes(drutama("cdcatatan")) & "', cdmatauang  = '" & FixQuotes(drutama("cdmatauang")) & "', cdkurs  = '" & FixDouble(drutama("cdkurs")) & "', cdjumlah  = '" & FixDouble(drutama("cdjumlah")) & "', cdjumlahvalas  = '" & FixDouble(drutama("cdjumlahvalas")) & "', cdjumlahbayar  = '" & FixDouble(drutama("cdjumlahbayar")) & "', cdjumlahbayarvalas  = '" & FixDouble(drutama("cdjumlahbayarvalas")) & "', cdstatusbayar  = " & drutama("cdstatusbayar") & ", cdtgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("cdtgllunas"))) & "', cdstatus  = " & drutama("cdstatus") & ", cdstatussebelumnya  = " & drutama("cdstatussebelumnya") & ", cdjmlrevisi  = cdjmlrevisi + 1, cdcetakanke  = " & drutama("cdcetakanke") & ", cdisclose  = " & drutama("cdisclose") & ", cdmodifikasiuser  = " & drutama("cdmodifikasiuser") & ", cdmodifikasitgl  = NOW(), cdposting  = 0, cdcustomtext1  = '" & FixQuotes(drutama("cdcustomtext1")) & "', cdcustomtext2  = '" & FixQuotes(drutama("cdcustomtext2")) & "', cdcustomtext3  = '" & FixQuotes(drutama("cdcustomtext3")) & "', cdcustomtext4  = '" & FixQuotes(drutama("cdcustomtext4")) & "', cdcustomtext5  = '" & FixQuotes(drutama("cdcustomtext5")) & "', cdcustomint1  = " & drutama("cdcustomint1") & ", cdcustomint2  = " & drutama("cdcustomint2") & ", cdcustomint3  = " & drutama("cdcustomint3") & ", cdcustomdbl1  = '" & FixDouble(drutama("cdcustomdbl1")) & "', cdcustomdbl2  = '" & FixDouble(drutama("cdcustomdbl2")) & "', cdcustomdbl3  = '" & FixDouble(drutama("cdcustomdbl3")) & "', cdcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("cdcustomdate1"))) & "', cdcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("cdcustomdate2"))) & "', cdcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("cdcustomdate3"))) & "' where cdid = '" & drutama("cdid") & "'"
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

                    If drutama("cdautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("cdcabang"), drutama("cdlokasi"), drutama("cdsumber"), drutama("cdtgl"))
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
                        notransaksi = drutama("cdnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(cdid) FROM m2_cd WHERE cdnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M2_Cd (cdcabang, cdlokasi, cdsumber, cdautonotransaksi, cdnotransaksi, cdtgl, cdkodepa, cdkontak, cdkontakperson, cdnorek, cduraian, cdcatatan, cdmatauang, cdkurs, cdjumlah, cdjumlahvalas, cdjumlahbayar, cdjumlahbayarvalas, cdstatusbayar, cdtgllunas, cdstatus, cdstatussebelumnya, cdjmlrevisi, cdcetakanke, cdisclose, cdinputuser, cdinputtgl, cdmodifikasiuser, cdmodifikasitgl, cdposting, cdcustomtext1, cdcustomtext2, cdcustomtext3, cdcustomtext4, cdcustomtext5, cdcustomint1, cdcustomint2, cdcustomint3, cdcustomdbl1, cdcustomdbl2, cdcustomdbl3, cdcustomdate1, cdcustomdate2, cdcustomdate3) values('" & FixQuotes(drutama("cdcabang")) & "', '" & FixQuotes(drutama("cdlokasi")) & "', '" & FixQuotes(drutama("cdsumber")) & "', " & drutama("cdautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("cdtgl"))) & "', " & drutama("cdkodepa") & ", " & drutama("cdkontak") & ", '" & FixQuotes(drutama("cdkontakperson")) & "', '" & FixQuotes(drutama("cdnorek")) & "', '" & FixQuotes(drutama("cduraian")) & "', '" & FixQuotes(drutama("cdcatatan")) & "', '" & FixQuotes(drutama("cdmatauang")) & "', '" & FixDouble(drutama("cdkurs")) & "', '" & FixDouble(drutama("cdjumlah")) & "', '" & FixDouble(drutama("cdjumlahvalas")) & "', '" & FixDouble(drutama("cdjumlahbayar")) & "', '" & FixDouble(drutama("cdjumlahbayarvalas")) & "', " & drutama("cdstatusbayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("cdtgllunas"))) & "', " & drutama("cdstatus") & ", " & drutama("cdstatussebelumnya") & ", " & drutama("cdjmlrevisi") & ", " & drutama("cdcetakanke") & ", " & drutama("cdisclose") & ", " & drutama("cdinputuser") & ", NOW(), " & drutama("cdmodifikasiuser") & ", '1971-01-01 00:00:00', 0, '" & FixQuotes(drutama("cdcustomtext1")) & "', '" & FixQuotes(drutama("cdcustomtext2")) & "', '" & FixQuotes(drutama("cdcustomtext3")) & "', '" & FixQuotes(drutama("cdcustomtext4")) & "', '" & FixQuotes(drutama("cdcustomtext5")) & "', " & drutama("cdcustomint1") & ", " & drutama("cdcustomint2") & ", " & drutama("cdcustomint3") & ", '" & FixDouble(drutama("cdcustomdbl1")) & "', '" & FixDouble(drutama("cdcustomdbl2")) & "', '" & FixDouble(drutama("cdcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("cdcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("cdcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("cdcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select cdid from M2_Cd where cdnotransaksi='" & notransaksi & "' AND Cdinputuser= '" & userid & "' order by Cdmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M2_Cd_Detail where idcd = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idcddetail") & ", " & result(4) & ", '" & FixQuotes(dr1("norek")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M2_Cd_Detail(idcddetail, idcd, norek, matauang, kurs, jumlah, jumlahvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                Dim sumber As String = "CD", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("cdstatus") = 2 Then
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
    Public Function M2_CdUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("cdkontakkode", "c.kkode")
            Filter = Filter.Replace("cdkontaknama", "c.knama")
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
            Dim sumber As String = "Cd", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Cdtgl, Cdnotransaksi, Cdstatus FROM m2_Cd WHERE Cdid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Cdstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m2_cd_history
            Dim rsSimpanHistory As String = SimpanHistory.M2_Cd_HistorySimpan("" & paramSplit(0) & "★M2_Cd_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then
                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'CD' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M2_Cd SET Cdstatus = " & nilaiStatus & ", Cdmodifikasiuser='" & userid & "', Cdmodifikasitgl = NOW(), Cdposting = 0, Cdpostingtgl = '1971-01-01 00:00:00', Cdjmlrevisi = Cdjmlrevisi + 1 WHERE Cdid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_CdSearch(PostWsSearch(paramSplit(0), "M2_CdSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_CdDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("cdkontakkode", "c.kkode")
            Filter = Filter.Replace("cdkontaknama", "c.knama")
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
            Dim sumber As String = "Cd", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Cdid, Cdnotransaksi FROM m2_Cd WHERE Cdid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT cdcabang, cdlokasi, cdsumber, cdautonotransaksi, cdnotransaksi, cdtgl"
            sql &= " FROM M2_cd"
            sql &= " WHERE cdid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("cdcabang")
                lokasi = dtNomorNext.Rows(0)("cdlokasi")
                sumber = dtNomorNext.Rows(0)("cdsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("cdautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("cdnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("cdtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE JURNAL
            sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'CD' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M2_Cd_Detail WHERE idCd = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M2_Cd WHERE Cdid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_CdSearch(PostWsSearch(paramSplit(0), "M2_CdSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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