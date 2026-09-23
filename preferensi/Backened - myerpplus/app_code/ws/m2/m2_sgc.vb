Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m2_sgc
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M2_SgcSimpan(ByVal param As String) As String
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
        'sgcid(0) As Integer, sgccabang(1) As String, sgclokasi(2) As String, sgcsumber(3) As String, sgcjenis(4) As Integer, 
        'sgcautonotransaksi(5) As Integer, sgcnotransaksi(6) As String, sgctgl(7) As Date, sgckodepa(8) As Integer, sgckontak(9) As Integer, 
        'sgckontakperson(10) As String, sgcuraian(11) As String, sgccatatan(12) As String, sgcmatauang(13) As String, sgckurs(14) As Double, 
        'sgcjumlah(15) As Double, sgcjumlahvalas(16) As Double, sgcidsg(17) As Integer, sgcstatus(18) As Integer, sgcstatussebelumnya(19) As Integer, 
        'sgcjmlrevisi(20) As Integer, sgccetakanke(21) As Integer, sgcisclose(22) As Integer, sgcinputuser(23) As Integer, sgcinputtgl(24) As DateTime, 
        'sgcmodifikasiuser(25) As Integer, sgcmodifikasitgl(26) As DateTime, sgcposting(27) As Integer, sgccustomtext1(28) As String, sgccustomtext2(29) As String, 
        'sgccustomtext3(30) As String, sgccustomtext4(31) As String, sgccustomtext5(32) As String, sgccustomint1(33) As Integer, sgccustomint2(34) As Integer, 
        'sgccustomint3(35) As Integer, sgccustomdbl1(36) As Double, sgccustomdbl2(37) As Double, sgccustomdbl3(38) As Double, sgccustomdate1(39) As Date, 
        'sgccustomdate2(40) As Date, sgccustomdate3(41) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'sgcid, sgccabang, sgclokasi, sgcsumber, sgcjenis, sgcautonotransaksi, sgcnotransaksi, 
        'sgctgl, sgckodepa, sgckontak, sgckontakperson, sgcuraian, sgccatatan, sgcmatauang, 
        'sgckurs, sgcjumlah, sgcjumlahvalas, sgcidsg, sgcstatus, sgcstatussebelumnya, sgcjmlrevisi, 
        'sgccetakanke, sgcisclose, sgcinputuser, sgcinputtgl, sgcmodifikasiuser, sgcmodifikasitgl, sgcposting, 
        'sgccustomtext1, sgccustomtext2, sgccustomtext3, sgccustomtext4, sgccustomtext5, sgccustomint1, sgccustomint2, 
        'sgccustomint3, sgccustomdbl1, sgccustomdbl2, sgccustomdbl3, sgccustomdate1, sgccustomdate2, sgccustomdate3


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 42) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'sgcid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "sgcid required numeric." : GoTo selesai
        End If
        'sgcjenis(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "sgcjenis required numeric." : GoTo selesai
        End If
        'sgcautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "sgcautonotransaksi required numeric." : GoTo selesai
        End If
        'sgctgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "sgctgl required date." : GoTo selesai
        End If
        'sgckodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "sgckodepa required numeric." : GoTo selesai
        End If
        'sgckontak(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "sgckontak required numeric." : GoTo selesai
        End If
        If (dataUtama(9) < 1) Then
            result(2) = "sgckontak can't be empty." : GoTo selesai
        End If
        'sgckurs(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "sgckurs required numeric." : GoTo selesai
        End If
        'sgcjumlah(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "sgcjumlah required numeric." : GoTo selesai
        End If
        'sgcjumlahvalas(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "sgcjumlahvalas required numeric." : GoTo selesai
        End If
        'sgcidsg(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "sgcidsg required numeric." : GoTo selesai
        End If
        'sgcstatus(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "sgcstatus required numeric." : GoTo selesai
        End If
        'sgcstatussebelumnya(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "sgcstatussebelumnya required numeric." : GoTo selesai
        End If
        'sgcjmlrevisi(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "sgcjmlrevisi required numeric." : GoTo selesai
        End If
        'sgccetakanke(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "sgccetakanke required numeric." : GoTo selesai
        End If
        'sgcisclose(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "sgcisclose required numeric." : GoTo selesai
        End If
        'sgcinputuser(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "sgcinputuser required numeric." : GoTo selesai
        End If
        'sgcinputtgl(24) As DateTime
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "sgcinputtgl required date." : GoTo selesai
        End If
        'sgcmodifikasiuser(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "sgcmodifikasiuser required numeric." : GoTo selesai
        End If
        'sgcmodifikasitgl(26) As DateTime
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "sgcmodifikasitgl required date." : GoTo selesai
        End If
        'sgcposting(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "sgcposting required numeric." : GoTo selesai
        End If
        'sgccustomint1(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "sgccustomint1 required numeric." : GoTo selesai
        End If
        'sgccustomint2(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "sgccustomint2 required numeric." : GoTo selesai
        End If
        'sgccustomint3(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "sgccustomint3 required numeric." : GoTo selesai
        End If
        'sgccustomdbl1(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "sgccustomdbl1 required numeric." : GoTo selesai
        End If
        'sgccustomdbl2(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "sgccustomdbl2 required numeric." : GoTo selesai
        End If
        'sgccustomdbl3(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "sgccustomdbl3 required numeric." : GoTo selesai
        End If
        'sgccustomdate1(39) As Date
        If (IsDate(dataUtama(39)) = False) Then
            result(2) = "sgccustomdate1 required date." : GoTo selesai
        End If
        'sgccustomdate2(40) As Date
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "sgccustomdate2 required date." : GoTo selesai
        End If
        'sgccustomdate3(41) As Date
        If (IsDate(dataUtama(41)) = False) Then
            result(2) = "sgccustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'sgccabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "sgccabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "sgccabang should not be more than 25 character." : GoTo selesai
        End If

        'sgclokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "sgclokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "sgclokasi should not be more than 25 character." : GoTo selesai
        End If

        'sgcsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "sgcsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "sgcsumber should not be more than 10 character." : GoTo selesai
        End If

        'sgcnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "sgcnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "sgcnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'sgctgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "sgctgl can't be empty" : GoTo selesai
        End If

        'sgcmatauang(13) As String
        If Len(dataUtama(13)) = 0 Then
            result(2) = "sgcmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(13)) > 25 Then
            result(2) = "sgcmatauang should not be more than 25 character." : GoTo selesai
        End If

        'sgckurs(14) As Double
        If Len(dataUtama(14)) = 0 Then
            result(2) = "sgckurs can't be empty" : GoTo selesai
        End If

        'sgcjumlah(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "sgcjumlah can't be empty" : GoTo selesai
        End If

        'sgcjumlahvalas(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "sgcjumlahvalas can't be empty" : GoTo selesai
        End If

        'sgcinputtgl(24) As DateTime
        If Len(dataUtama(24)) = 0 Then
            result(2) = "sgcinputtgl can't be empty" : GoTo selesai
        End If

        'sgcmodifikasitgl(26) As DateTime
        If Len(dataUtama(26)) = 0 Then
            result(2) = "sgcmodifikasitgl can't be empty" : GoTo selesai
        End If

        'sgccustomdbl1(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "sgccustomdbl1 can't be empty" : GoTo selesai
        End If

        'sgccustomdbl2(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "sgccustomdbl2 can't be empty" : GoTo selesai
        End If

        'sgccustomdbl3(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "sgccustomdbl3 can't be empty" : GoTo selesai
        End If

        'sgccustomdate1(39) As Date
        If Len(dataUtama(39)) = 0 Then
            result(2) = "sgccustomdate1 can't be empty" : GoTo selesai
        End If

        'sgccustomdate2(40) As Date
        If Len(dataUtama(40)) = 0 Then
            result(2) = "sgccustomdate2 can't be empty" : GoTo selesai
        End If

        'sgccustomdate3(41) As Date
        If Len(dataUtama(41)) = 0 Then
            result(2) = "sgccustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "sgcid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgccabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgclokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcjenis", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgctgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgckodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgckontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgckontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgccatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgckurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcjumlah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcjumlahvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcidsg", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgccetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgccustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgccustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgccustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgccustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgccustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgccustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgccustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgccustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgccustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgccustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgccustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgccustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgccustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgccustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "sgcid~sgccabang~sgclokasi~sgcsumber~sgcjenis~sgcautonotransaksi~sgcnotransaksi~sgctgl~sgckodepa~sgckontak~sgckontakperson~sgcuraian~sgccatatan~sgcmatauang~sgckurs~sgcjumlah~sgcjumlahvalas~sgcidsg~sgcstatus~sgcstatussebelumnya~sgcjmlrevisi~sgccetakanke~sgcisclose~sgcinputuser~sgcinputtgl~sgcmodifikasiuser~sgcmodifikasitgl~sgcposting~sgccustomtext1~sgccustomtext2~sgccustomtext3~sgccustomtext4~sgccustomtext5~sgccustomint1~sgccustomint2~sgccustomint3~sgccustomdbl1~sgccustomdbl2~sgccustomdbl3~sgccustomdate1~sgccustomdate2~sgccustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idsgcdetail(0) As Integer, idsgc(1) As Integer, nogiro(2) As String, kontak(3) As Integer, matauang(4) As String, 
        'kurs(5) As Double, jumlah(6) As Double, jumlahvalas(7) As Double, bank(8) As String, noacbank(9) As String, 
        'rekbank(10) As String, rekgiro(11) As String, tgljatuhtempo(12) As Date, catatan(13) As String, urutan(14) As Integer, 
        'statusgiro(15) As Integer, idsgdetail(16) As Integer, isclose(17) As Integer, customtext1(18) As String, customtext2(19) As String, 
        'customtext3(20) As String, customdbl1(21) As Double, customdbl2(22) As Double, customdbl3(23) As Double, customdate1(24) As Date, 
        'customdate2(25) As Date, customdate3(26) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idsgcdetail, idsgc, nogiro, kontak, matauang, kurs, jumlah, 
        'jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, 
        'urutan, statusgiro, idsgdetail, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idsgcdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idsgc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "nogiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "bank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "noacbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekgiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "statusgiro", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idsgdetail", AsEnumTypeData.AsInt64)
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


        'Variabel Validasi
        Dim ftExistGiro As String = "", ftGiro As String = "", vNogiro As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 27) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idsgcdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idsgcdetail required numeric." : GoTo selesai
            End If
            'idsgc(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idsgc required numeric." : GoTo selesai
            End If
            'kontak(3) As Integer
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - kontak required numeric." : GoTo selesai
            End If
            'kurs(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'jumlah(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - jumlah required numeric." : GoTo selesai
            End If
            'jumlahvalas(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - jumlahvalas required numeric." : GoTo selesai
            End If
            'tgljatuhtempo(12) As Date
            If (IsDate(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - tgljatuhtempo required date." : GoTo selesai
            End If
            'urutan(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'statusgiro(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - statusgiro required numeric." : GoTo selesai
            End If
            'idsgdetail(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - idsgdetail required numeric." : GoTo selesai
            End If
            'isclose(17) As Integer
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(21) As Double
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(22) As Double
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(23) As Double
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(24) As Date
            If (IsDate(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(25) As Date
            If (IsDate(dataRowDetail(25)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(26) As Date
            If (IsDate(dataRowDetail(26)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'nogiro(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - nogiro can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - nogiro should not be more than 25 character." : GoTo selesai
            End If

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

            'jumlah(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - jumlah can't be empty" : GoTo selesai
            End If

            'jumlahvalas(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - jumlahvalas can't be empty" : GoTo selesai
            End If

            'tgljatuhtempo(12) As Date
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - tgljatuhtempo can't be empty" : GoTo selesai
            End If

            'customdbl1(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(22) As Double
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(23) As Double
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(24) As Date
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(25) As Date
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(26) As Date
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idsgcdetail~idsgc~nogiro~kontak~matauang~kurs~jumlah~jumlahvalas~bank~noacbank~rekbank~rekgiro~tgljatuhtempo~catatan~urutan~statusgiro~idsgdetail~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If


            'BUAT FILTER VALIDASI STATUS GIRO ---------------------------
            'nogiro(2) As String
            vNogiro = dataRowDetail(2)

            'CEK DATA EXIST
            ftExistGiro = IIf(Len(ftExistGiro.ToString) = 0, "", ftExistGiro & " UNION ")
            ftExistGiro = String.Concat(ftExistGiro, "SELECT EXISTS(SELECT 1 FROM m2_giro_list WHERE glnogiro = '" & vNogiro & "' LIMIT 1) as rowExists, '" & vNogiro & "' as glnogiro")

            'Validasi Status Giro
            ftGiro = IIf(Len(ftGiro.ToString) = 0, "", ftGiro & " OR ")
            ftGiro = String.Concat(ftGiro, "(glnogiro = '" & vNogiro & "')")
            'END OF BUAT FILTER VALIDASI STATUS GIRO --------------------

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
                Dim vModuleId As Integer = 2, vMenuId As Integer = 12
                Select Case drutama("sgcstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("sgctgl")), AsFormatTanggal(drutama("sgctgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================

                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("sgcstatus") = 2 Or drutama("sgcstatus") = 1 Or drutama("sgcstatus") = 8 Or drutama("sgcstatus") = 9 Or drutama("sgcstatus") = 10 Or drutama("sgcstatus") = 11 Then
                    Dim rsValidasi As String = ValidasiSimpan(ftExistGiro, ftGiro)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================

                'HITUNG TOTAL BERDASARKAN DATA DETAIL ===================
                drutama("sgcjumlah") = AsDataTableDSum(dtdetail, "jumlah")
                drutama("sgcjumlahvalas") = AsDataTableDSum(dtdetail, "jumlahvalas")
                'END OF HITUNG TOTAL BERDASARKAN DATA DETAIL ============

                If isUpdate Then
                    result(4) = drutama("sgcid")
                    notransaksi = drutama("sgcnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(sgcid), sgcnotransaksi FROM M2_sgc WHERE sgcid='" & result(4) & "' AND sgcstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("sgcautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("sgccabang"), drutama("sgclokasi"), drutama("sgcsumber"), drutama("sgctgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(sgcid) FROM m2_sgc WHERE sgcnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m2_sgc_history
                        Dim rsSimpanHistory As String = SimpanHistory.M2_Sgc_HistorySimpan("" & paramSplit(0) & "★M2_Sgc_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("sgcsumber")) & "▼" & FixQuotes(drutama("sgcid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M2_Sgc set sgccabang  = '" & FixQuotes(drutama("sgccabang")) & "', sgclokasi  = '" & FixQuotes(drutama("sgclokasi")) & "', sgcsumber  = '" & FixQuotes(drutama("sgcsumber")) & "', sgcjenis  = " & drutama("sgcjenis") & ", sgcautonotransaksi  = " & drutama("sgcautonotransaksi") & ", sgcnotransaksi  = '" & notransaksi & "', sgctgl  = '" & FixQuotes(AsFormatTanggal(drutama("sgctgl"))) & "', sgckodepa  = " & drutama("sgckodepa") & ", sgckontak  = " & drutama("sgckontak") & ", sgckontakperson  = '" & FixQuotes(drutama("sgckontakperson")) & "', sgcuraian  = '" & FixQuotes(drutama("sgcuraian")) & "', sgccatatan  = '" & FixQuotes(drutama("sgccatatan")) & "', sgcmatauang  = '" & FixQuotes(drutama("sgcmatauang")) & "', sgckurs  = '" & FixDouble(drutama("sgckurs")) & "', sgcjumlah  = '" & FixDouble(drutama("sgcjumlah")) & "', sgcjumlahvalas  = '" & FixDouble(drutama("sgcjumlahvalas")) & "', sgcidsg  = " & drutama("sgcidsg") & ", sgcstatus  = " & drutama("sgcstatus") & ", sgcstatussebelumnya  = " & drutama("sgcstatussebelumnya") & ", sgcjmlrevisi  = sgcjmlrevisi+1, sgccetakanke  = " & drutama("sgccetakanke") & ", sgcisclose  = " & drutama("sgcisclose") & ", sgcmodifikasiuser  = " & drutama("sgcmodifikasiuser") & ", sgcmodifikasitgl  = NOW(), sgcposting  = 0, sgccustomtext1  = '" & FixQuotes(drutama("sgccustomtext1")) & "', sgccustomtext2  = '" & FixQuotes(drutama("sgccustomtext2")) & "', sgccustomtext3  = '" & FixQuotes(drutama("sgccustomtext3")) & "', sgccustomtext4  = '" & FixQuotes(drutama("sgccustomtext4")) & "', sgccustomtext5  = '" & FixQuotes(drutama("sgccustomtext5")) & "', sgccustomint1  = " & drutama("sgccustomint1") & ", sgccustomint2  = " & drutama("sgccustomint2") & ", sgccustomint3  = " & drutama("sgccustomint3") & ", sgccustomdbl1  = '" & FixDouble(drutama("sgccustomdbl1")) & "', sgccustomdbl2  = '" & FixDouble(drutama("sgccustomdbl2")) & "', sgccustomdbl3  = '" & FixDouble(drutama("sgccustomdbl3")) & "', sgccustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("sgccustomdate1"))) & "', sgccustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("sgccustomdate2"))) & "', sgccustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("sgccustomdate3"))) & "' where sgcid = '" & drutama("sgcid") & "'"
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

                    If drutama("sgcautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("sgccabang"), drutama("sgclokasi"), drutama("sgcsumber"), drutama("sgctgl"))
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
                        notransaksi = drutama("sgcnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(sgcid) FROM m2_sgc WHERE sgcnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M2_Sgc (sgccabang, sgclokasi, sgcsumber, sgcjenis, sgcautonotransaksi, sgcnotransaksi, sgctgl, sgckodepa, sgckontak, sgckontakperson, sgcuraian, sgccatatan, sgcmatauang, sgckurs, sgcjumlah, sgcjumlahvalas, sgcidsg, sgcstatus, sgcstatussebelumnya, sgcjmlrevisi, sgccetakanke, sgcisclose, sgcinputuser, sgcinputtgl, sgcmodifikasiuser, sgcmodifikasitgl, sgcposting, sgccustomtext1, sgccustomtext2, sgccustomtext3, sgccustomtext4, sgccustomtext5, sgccustomint1, sgccustomint2, sgccustomint3, sgccustomdbl1, sgccustomdbl2, sgccustomdbl3, sgccustomdate1, sgccustomdate2, sgccustomdate3) values('" & FixQuotes(drutama("sgccabang")) & "', '" & FixQuotes(drutama("sgclokasi")) & "', '" & FixQuotes(drutama("sgcsumber")) & "', " & drutama("sgcjenis") & ", " & drutama("sgcautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("sgctgl"))) & "', " & drutama("sgckodepa") & ", " & drutama("sgckontak") & ", '" & FixQuotes(drutama("sgckontakperson")) & "', '" & FixQuotes(drutama("sgcuraian")) & "', '" & FixQuotes(drutama("sgccatatan")) & "', '" & FixQuotes(drutama("sgcmatauang")) & "', '" & FixDouble(drutama("sgckurs")) & "', '" & FixDouble(drutama("sgcjumlah")) & "', '" & FixDouble(drutama("sgcjumlahvalas")) & "', " & drutama("sgcidsg") & ", " & drutama("sgcstatus") & ", " & drutama("sgcstatussebelumnya") & ", " & drutama("sgcjmlrevisi") & ", " & drutama("sgccetakanke") & ", " & drutama("sgcisclose") & ", " & drutama("sgcinputuser") & ", NOW(), " & drutama("sgcmodifikasiuser") & ", '1971-01-01 00:00:00', 0, '" & FixQuotes(drutama("sgccustomtext1")) & "', '" & FixQuotes(drutama("sgccustomtext2")) & "', '" & FixQuotes(drutama("sgccustomtext3")) & "', '" & FixQuotes(drutama("sgccustomtext4")) & "', '" & FixQuotes(drutama("sgccustomtext5")) & "', " & drutama("sgccustomint1") & ", " & drutama("sgccustomint2") & ", " & drutama("sgccustomint3") & ", '" & FixDouble(drutama("sgccustomdbl1")) & "', '" & FixDouble(drutama("sgccustomdbl2")) & "', '" & FixDouble(drutama("sgccustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sgccustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sgccustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sgccustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select sgcid from M2_sgc where sgcnotransaksi='" & notransaksi & "' AND sgcinputuser= '" & userid & "' order by sgcmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M2_Sgc_Detail where idsgc = '" & result(4) & "'"
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
                    Dim strValue2 As New StringBuilder, strGiro As New StringBuilder, strRekgiro As New StringBuilder
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idsgcdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("nogiro")) & "', " & dr1("kontak") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljatuhtempo"))) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("statusgiro") & ", " & dr1("idsgdetail") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                        If drutama("sgcstatus") = 2 Then
                            'filter
                            strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", " OR "))
                            strGiro.Append("(glnogiro = '" & FixQuotes(dr1("nogiro")) & "')")
                            'rekgiro
                            strRekgiro.Append(" WHEN '" & FixQuotes(dr1("nogiro")) & "' THEN '" & FixQuotes(dr1("rekgiro")) & "' ")
                        End If
                    Next
                    sql = "Insert into M2_Sgc_Detail(idsgcdetail, idsgc, nogiro, kontak, matauang, kurs, jumlah, jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, urutan, statusgiro, idsgdetail, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'update glstatus, gltglcair, glrekgiro m2_giro_list
                    If drutama("sgcstatus") = 2 Then
                        'cek status giro
                        Dim dtValidasi As DataTable = AsDataTableAmbilDariDBCon("SELECT glnogiro FROM m2_giro_list WHERE glstatus = 1 AND (" & strGiro.ToString & ")", myConn)
                        If dtValidasi.Rows.Count > 0 Then result(2) = "Can't update giro '" & dtValidasi.Rows(0)(0) & "'. It has related transactions." : Trans.Rollback() : GoTo selesai

                        'update giro                   glstatus                                , gltglcair                              , glrekgiro                                                                            filter
                        sql = "UPDATE m2_giro_list SET glstatus = '" & drutama("sgcjenis") & "', gltglcair = '" & drutama("sgctgl") & "', glrekgiro = (CASE glnogiro " & strRekgiro.ToString & " ELSE glrekgiro END) WHERE " & strGiro.ToString & ""
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
                Dim sumber As String = "SGC", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("sgcstatus") = 2 Then
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
    Public Function M2_SgcUpdateStatus(ByVal param As String) As String

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
            Dim sumber As String = "Sgc", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Sgctgl, Sgcnotransaksi, Sgcstatus FROM m2_Sgc WHERE Sgcid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Sgcstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m2_sgc_history
            Dim rsSimpanHistory As String = SimpanHistory.M2_Sgc_HistorySimpan("" & paramSplit(0) & "★M2_Sgc_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m2_sgc_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                'If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                'PROSES GIRO ====================================================================
                Dim strGiro As New StringBuilder
                'ambil giro dari detail
                dtdetail = AsDataTableAmbilDariDBCon("SELECT nogiro FROM m2_sgc_detail WHERE idsgc = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    'buat filter query untuk ambil giro m2_giro_list
                    For Each dr1 As DataRow In dtdetail.Rows
                        strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", " OR "))
                        strGiro.Append("(glnogiro = '" & FixQuotes(dr1("nogiro")) & "')")
                    Next
                    'ambil giro dari m2_giro_list
                    dtdetail = AsDataTableAmbilDariDBCon("SELECT glnogiro, glstatus FROM m2_giro_list WHERE (" & strGiro.ToString & ")", myConn)
                    If dtdetail.Rows.Count > 0 Then
                        'cek giro yang sudah terkait dengan notransaksi lain
                        'Dim dtValidasi As DataTable = AsDataTableFilterSortDt(dtdetail, "glstatus = '1'")
                        'If dtValidasi.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai


                        'AMBIL REKENING GIRO KELUAR DARI M0_SETTING
                        Dim dtrekgiro As DataTable = AsDataTableAmbilDariDBCon("SELECT snilai FROM m0_setting WHERE smodule=0 AND sgrup='akun' AND skode='GiroKeluar'", myConn)
                        Dim rekgiro As String = ""
                        If dtrekgiro.Rows.Count > 0 Then
                            rekgiro = dtrekgiro.Rows(0)(0).ToString
                        Else
                            result(2) = "Setting Giro Out CoA not found." : Trans.Rollback() : GoTo selesai
                        End If

                        'UPDATE STATUS GIRO MENJADI BELUM CAIR (0) DAN REKGIRO = SETTING PIUTANG GIRO KELUAR
                        'update m2_giro_list           glstatus                                                                                                                 filter
                        sql = "UPDATE m2_giro_list SET glstatus = '0', gltglcair = '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', glrekgiro = '" & rekgiro & "' WHERE (" & strGiro.ToString & ")"
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
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If
                'END OF PROSES GIRO =============================================================

                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'SGC' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M2_Sgc SET Sgcstatus = " & nilaiStatus & ", Sgcmodifikasiuser='" & userid & "', Sgcmodifikasitgl = NOW(), Sgcposting = 0, Sgcpostingtgl = '1971-01-01 00:00:00', Sgcjmlrevisi = Sgcjmlrevisi + 1 WHERE Sgcid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_SgcSearch(PostWsSearch(paramSplit(0), "M2_SgcSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_SgcDelete(ByVal param As String) As String

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
            Dim sumber As String = "Sgc", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Sgcid, Sgcnotransaksi FROM m2_Sgc WHERE Sgcid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT sgccabang, sgclokasi, sgcsumber, sgcautonotransaksi, sgcnotransaksi, sgctgl"
            sql &= " FROM M2_sgc"
            sql &= " WHERE sgcid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("sgccabang")
                lokasi = dtNomorNext.Rows(0)("sgclokasi")
                sumber = dtNomorNext.Rows(0)("sgcsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("sgcautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("sgcnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("sgctgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE JURNAL
            sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'SGC' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M2_Sgc_Detail WHERE idSgc = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M2_Sgc WHERE Sgcid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_SgcSearch(PostWsSearch(paramSplit(0), "M2_SgcSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_SgcGetdataById(ByVal param As String) As String

        'M2_SgcGetdataById Utama --------------------------------------------------------
        'sgcid, sgccabang, sgclokasi, sgcsumber, sgcjenis, sgcautonotransaksi, sgcnotransaksi, 
        'sgctgl, sgckodepa, sgckontak, sgckontakperson, sgcuraian, sgccatatan, sgcmatauang, 
        'sgckurs, sgcjumlah, sgcjumlahvalas, sgcidsg, sgcstatus, sgcstatussebelumnya, sgcjmlrevisi, 
        'sgccetakanke, sgcisclose, sgcinputuser, sgcinputtgl, sgcmodifikasiuser, sgcmodifikasitgl, sgcposting, 
        'sgcpostingtgl, sgccustomtext1, sgccustomtext2, sgccustomtext3, sgccustomtext4, sgccustomtext5, sgccustomint1, 
        'sgccustomint2, sgccustomint3, sgccustomdbl1, sgccustomdbl2, sgccustomdbl3, sgccustomdate1, sgccustomdate2, 
        'sgccustomdate3, sgccabangnama, sgclokasinama, sgcjenisnama, sgckontakkode, sgckontaknama, sgcnotransaksisg, 
        'sgcstatusnama, sgcstatussebelumnyanama, sgcinputusernama, sgcmodifikasiusernama

        'M2_SgcGetdataById Detail -------------------------------------------------------
        'idsgcdetail, idsgc, nogiro, kontak, 
        'matauang, kurs, jumlah, jumlahvalas, bank, noacbank, rekbank, 
        'rekgiro, tgljatuhtempo, catatan, urutan, statusgiro, idsgdetail, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3, kontakkode, kontaknama, banknama, rekbanknama, rekgironama, 
        'statusgironama, sgnotransaksi

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

        Dim NmMemcached As String = "aplikasi1-M2_Sgc~M2_Sgc_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "sgcid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "sgcid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_sgc_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                     FxDB(drutama("sgcid"), 0), sptField,
                     FxDB(drutama("sgccabang"), ""), sptField,
                     FxDB(drutama("sgclokasi"), ""), sptField,
                     FxDB(drutama("sgcsumber"), ""), sptField,
                     FxDB(drutama("sgcjenis"), 0), sptField,
                     FxDB(drutama("sgcautonotransaksi"), 0), sptField,
                     FxDB(drutama("sgcnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sgctgl"), ""), formatTgl), sptField,
                     FxDB(drutama("sgckodepa"), 0), sptField,
                     FxDB(drutama("sgckontak"), 0), sptField,
                     FxDB(drutama("sgckontakperson"), ""), sptField,
                     FxDB(drutama("sgcuraian"), ""), sptField,
                     FxDB(drutama("sgccatatan"), ""), sptField,
                     FxDB(drutama("sgcmatauang"), ""), sptField,
                     FxDB(drutama("sgckurs"), 0), sptField,
                     FxDB(drutama("sgcjumlah"), 0), sptField,
                     FxDB(drutama("sgcjumlahvalas"), 0), sptField,
                     FxDB(drutama("sgcidsg"), 0), sptField,
                     FxDB(drutama("sgcstatus"), 0), sptField,
                     FxDB(drutama("sgcstatussebelumnya"), 0), sptField,
                     FxDB(drutama("sgcjmlrevisi"), 0), sptField,
                     FxDB(drutama("sgccetakanke"), 0), sptField,
                     FxDB(drutama("sgcisclose"), 0), sptField,
                     FxDB(drutama("sgcinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sgcinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sgcmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sgcmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sgcposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sgcpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sgccustomtext1"), ""), sptField,
                     FxDB(drutama("sgccustomtext2"), ""), sptField,
                     FxDB(drutama("sgccustomtext3"), ""), sptField,
                     FxDB(drutama("sgccustomtext4"), ""), sptField,
                     FxDB(drutama("sgccustomtext5"), ""), sptField,
                     FxDB(drutama("sgccustomint1"), 0), sptField,
                     FxDB(drutama("sgccustomint2"), 0), sptField,
                     FxDB(drutama("sgccustomint3"), 0), sptField,
                     FxDB(drutama("sgccustomdbl1"), 0), sptField,
                     FxDB(drutama("sgccustomdbl2"), 0), sptField,
                     FxDB(drutama("sgccustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sgccustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sgccustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sgccustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("sgccabangnama"), ""), sptField,
                     FxDB(drutama("sgclokasinama"), ""), sptField,
                     FxDB(drutama("sgcjenisnama"), ""), sptField,
                     FxDB(drutama("sgckontakkode"), ""), sptField,
                     FxDB(drutama("sgckontaknama"), ""), sptField,
                     FxDB(drutama("sgcnotransaksisg"), ""), sptField,
                     FxDB(drutama("sgcstatusnama"), ""), sptField,
                     FxDB(drutama("sgcstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("sgcinputusernama"), ""), sptField,
                     FxDB(drutama("sgcmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idsgcdetail"), 0), sptField,
                     FxDB(dr("idsgc"), 0), sptField,
                     FxDB(dr("nogiro"), ""), sptField,
                     FxDB(dr("kontak"), 0), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("jumlah"), 0), sptField,
                     FxDB(dr("jumlahvalas"), 0), sptField,
                     FxDB(dr("bank"), ""), sptField,
                     FxDB(dr("noacbank"), ""), sptField,
                     FxDB(dr("rekbank"), ""), sptField,
                     FxDB(dr("rekgiro"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("statusgiro"), 0), sptField,
                     FxDB(dr("idsgdetail"), 0), sptField,
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
                     FxDB(dr("kontakkode"), ""), sptField,
                     FxDB(dr("kontaknama"), ""), sptField,
                     FxDB(dr("banknama"), ""), sptField,
                     FxDB(dr("rekbanknama"), ""), sptField,
                     FxDB(dr("rekgironama"), ""), sptField,
                     FxDB(dr("statusgironama"), ""), sptField,
                     FxDB(dr("sgnotransaksi"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sgcid, sgccabang, sgclokasi, sgcsumber, sgcjenis, sgcautonotransaksi, sgcnotransaksi, sgctgl, sgckodepa, sgckontak, sgckontakperson, sgcuraian, sgccatatan, sgcmatauang, sgckurs, sgcjumlah, sgcjumlahvalas, sgcidsg, sgcstatus, sgcstatussebelumnya, sgcjmlrevisi, sgccetakanke, sgcisclose, sgcinputuser, sgcinputtgl, sgcmodifikasiuser, sgcmodifikasitgl, sgcposting, sgcpostingtgl, sgccustomtext1, sgccustomtext2, sgccustomtext3, sgccustomtext4, sgccustomtext5, sgccustomint1, sgccustomint2, sgccustomint3, sgccustomdbl1, sgccustomdbl2, sgccustomdbl3, sgccustomdate1, sgccustomdate2, sgccustomdate3, sgccabangnama, sgclokasinama, sgcjenisnama, sgckontakkode, sgckontaknama, sgcnotransaksisg, sgcstatusnama, sgcstatussebelumnyanama, sgcinputusernama, sgcmodifikasiusernama" & sptSubParam & "idsgcdetail, idsgc, nogiro, kontak, matauang, kurs, jumlah, jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, urutan, statusgiro, idsgdetail, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kontakkode, kontaknama, banknama, rekbanknama, rekgironama, statusgironama, sgnotransaksi"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_SgcSearch(ByVal param As String) As String
        'M2_SgcSearch --------------------------------------------------------
        'sgcid, sgccabang, sgclokasi, sgcsumber, sgcjenis, sgcautonotransaksi, sgcnotransaksi, 
        'sgctgl, sgckodepa, sgckontak, sgckontakperson, sgcuraian, sgccatatan, sgcmatauang, 
        'sgckurs, sgcjumlah, sgcjumlahvalas, sgcidsg, sgcstatus, sgcstatussebelumnya, sgcjmlrevisi, 
        'sgccetakanke, sgcisclose, sgcinputuser, sgcinputtgl, sgcmodifikasiuser, sgcmodifikasitgl, sgcposting, 
        'sgcpostingtgl, sgccabangnama, sgclokasinama, sgcjenisnama, sgckontakkode, sgckontaknama, sgcnotransaksisg, 
        'sgcstatusnama, sgcstatussebelumnyanama, sgcinputusernama, sgcmodifikasiusernama

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
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_sgc_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Sgc", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("sgcid"), 0), sptField,
                     FxDB(dr("sgccabang"), ""), sptField,
                     FxDB(dr("sgclokasi"), ""), sptField,
                     FxDB(dr("sgcsumber"), ""), sptField,
                     FxDB(dr("sgcjenis"), 0), sptField,
                     FxDB(dr("sgcautonotransaksi"), 0), sptField,
                     FxDB(dr("sgcnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sgctgl"), ""), formatTgl), sptField,
                     FxDB(dr("sgckodepa"), 0), sptField,
                     FxDB(dr("sgckontak"), 0), sptField,
                     FxDB(dr("sgckontakperson"), ""), sptField,
                     FxDB(dr("sgcuraian"), ""), sptField,
                     FxDB(dr("sgccatatan"), ""), sptField,
                     FxDB(dr("sgcmatauang"), ""), sptField,
                     FxDB(dr("sgckurs"), 0), sptField,
                     FxDB(dr("sgcjumlah"), 0), sptField,
                     FxDB(dr("sgcjumlahvalas"), 0), sptField,
                     FxDB(dr("sgcidsg"), 0), sptField,
                     FxDB(dr("sgcstatus"), 0), sptField,
                     FxDB(dr("sgcstatussebelumnya"), 0), sptField,
                     FxDB(dr("sgcjmlrevisi"), 0), sptField,
                     FxDB(dr("sgccetakanke"), 0), sptField,
                     FxDB(dr("sgcisclose"), 0), sptField,
                     FxDB(dr("sgcinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sgcinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sgcmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sgcmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sgcposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sgcpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sgccabangnama"), ""), sptField,
                     FxDB(dr("sgclokasinama"), ""), sptField,
                     FxDB(dr("sgcjenisnama"), ""), sptField,
                     FxDB(dr("sgckontakkode"), ""), sptField,
                     FxDB(dr("sgckontaknama"), ""), sptField,
                     FxDB(dr("sgcnotransaksisg"), ""), sptField,
                     FxDB(dr("sgcstatusnama"), ""), sptField,
                     FxDB(dr("sgcstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("sgcinputusernama"), ""), sptField,
                     FxDB(dr("sgcmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sgcid, sgccabang, sgclokasi, sgcsumber, sgcjenis, sgcautonotransaksi, sgcnotransaksi, sgctgl, sgckodepa, sgckontak, sgckontakperson, sgcuraian, sgccatatan, sgcmatauang, sgckurs, sgcjumlah, sgcjumlahvalas, sgcidsg, sgcstatus, sgcstatussebelumnya, sgcjmlrevisi, sgccetakanke, sgcisclose, sgcinputuser, sgcinputtgl, sgcmodifikasiuser, sgcmodifikasitgl, sgcposting, sgcpostingtgl, sgccabangnama, sgclokasinama, sgcjenisnama, sgckontakkode, sgckontaknama, sgcnotransaksisg, sgcstatusnama, sgcstatussebelumnyanama, sgcinputusernama, sgcmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_SgcTerkait(ByVal param As String) As String
        'M2_SgcTerkait --------------------------------------------------------
        'sgcid, sgcnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "rgid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_sgc_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("sgcid"), 0), sptField,
                     FxDB(dr("sgcnotransaksi"), ""), sptField,
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
            result(2) = "Related SGC data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sgcid, sgcnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    Public Function ValidasiSimpan(ByVal filterExist As String, ByVal filter As String) As String
        Dim hasil As String = "", sql As String = ""
        Dim dtvalidasi As New DataTable

        'VALIDASI EXIST GIRO =============================================
        If Len(filterExist) > 0 Then
            dtvalidasi = AsDataTableAmbilDariDB(filterExist) 'rowExists, glnogiro
            dtvalidasi = AsDataTableFilterLimit(dtvalidasi, "rowExists = 0", , , 1)
            If (dtvalidasi.Rows.Count > 0) Then
                hasil = "Giro : " & dtvalidasi.Rows(0)("glnogiro") & " - doesn't exist in Giro List." : GoTo selesai
            End If
        End If
        'END OF VALIDASI EXIST GIRO ======================================

        'VALIDASI STATUS GIRO ============================================
        If Len(filter) > 0 Then
            'filter giro dikurangi 2 karakter terakhir untuk menghilangkan 'or' terakhir
            'filter = filter.Substring(0, filter.Length - 2)

            sql = "SELECT glnogiro, glstatus FROM m2_giro_list WHERE (glstatus <> 0) AND (" & filter & ") LIMIT 1"
            dtvalidasi = AsDataTableAmbilDariDB(sql)
            If (dtvalidasi.Rows.Count > 0) Then
                Select Case Double.Parse(dtvalidasi.Rows(0)("glstatus"))
                    Case 1
                        sql = "SELECT glnogiro, sgnotransaksi FROM m2_giro_list JOIN m2_sg_detail ON glnogiro = nogiro JOIN m2_sg ON idsg = sgid WHERE (sgstatus = 2 OR sgstatus = 3 OR sgstatus = 4 OR sgstatus = 7) AND (glnogiro = '" & FixQuotes(dtvalidasi.Rows(0)("glnogiro")) & "') LIMIT 1"
                        dtvalidasi = AsDataTableAmbilDariDB(sql)
                        If (dtvalidasi.Rows.Count > 0) Then
                            hasil = "Giro : " & dtvalidasi.Rows(0)(0) & " - has disbursed in transaction : " & dtvalidasi.Rows(0)(1) : GoTo selesai
                        End If
                    Case Else
                        sql = "SELECT glnogiro, sgcnotransaksi FROM m2_giro_list JOIN m2_sgc_detail ON glnogiro = nogiro JOIN m2_sgc ON idsgc = sgcid WHERE (sgcstatus = 2 OR sgcstatus = 3 OR sgcstatus = 4 OR sgcstatus = 7) AND (glnogiro = '" & FixQuotes(dtvalidasi.Rows(0)("glnogiro")) & "') LIMIT 1"
                        dtvalidasi = AsDataTableAmbilDariDB(sql)
                        If (dtvalidasi.Rows.Count > 0) Then
                            hasil = "Giro : " & dtvalidasi.Rows(0)(0) & " - has rejected/canceled in transaction : " & dtvalidasi.Rows(0)(1) : GoTo selesai
                        End If
                End Select
            End If
        End If
        'END OF VALIDASI STATUS GIRO =====================================

selesai:
        Return hasil
    End Function

    <WebMethod()>
    Public Function M2_SgcSimpanOld(ByVal param As String) As String
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
        'sgcid(0) As Integer, sgccabang(1) As String, sgclokasi(2) As String, sgcsumber(3) As String, sgcjenis(4) As Integer, 
        'sgcautonotransaksi(5) As Integer, sgcnotransaksi(6) As String, sgctgl(7) As Date, sgckodepa(8) As Integer, sgckontak(9) As Integer, 
        'sgckontakperson(10) As String, sgcuraian(11) As String, sgccatatan(12) As String, sgcmatauang(13) As String, sgckurs(14) As Double, 
        'sgcjumlah(15) As Double, sgcjumlahvalas(16) As Double, sgcidsg(17) As Integer, sgcstatus(18) As Integer, sgcstatussebelumnya(19) As Integer, 
        'sgcjmlrevisi(20) As Integer, sgccetakanke(21) As Integer, sgcisclose(22) As Integer, sgcinputuser(23) As Integer, sgcinputtgl(24) As DateTime, 
        'sgcmodifikasiuser(25) As Integer, sgcmodifikasitgl(26) As DateTime, sgcposting(27) As Integer, sgccustomtext1(28) As String, sgccustomtext2(29) As String, 
        'sgccustomtext3(30) As String, sgccustomtext4(31) As String, sgccustomtext5(32) As String, sgccustomint1(33) As Integer, sgccustomint2(34) As Integer, 
        'sgccustomint3(35) As Integer, sgccustomdbl1(36) As Double, sgccustomdbl2(37) As Double, sgccustomdbl3(38) As Double, sgccustomdate1(39) As Date, 
        'sgccustomdate2(40) As Date, sgccustomdate3(41) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'sgcid, sgccabang, sgclokasi, sgcsumber, sgcjenis, sgcautonotransaksi, sgcnotransaksi, 
        'sgctgl, sgckodepa, sgckontak, sgckontakperson, sgcuraian, sgccatatan, sgcmatauang, 
        'sgckurs, sgcjumlah, sgcjumlahvalas, sgcidsg, sgcstatus, sgcstatussebelumnya, sgcjmlrevisi, 
        'sgccetakanke, sgcisclose, sgcinputuser, sgcinputtgl, sgcmodifikasiuser, sgcmodifikasitgl, sgcposting, 
        'sgccustomtext1, sgccustomtext2, sgccustomtext3, sgccustomtext4, sgccustomtext5, sgccustomint1, sgccustomint2, 
        'sgccustomint3, sgccustomdbl1, sgccustomdbl2, sgccustomdbl3, sgccustomdate1, sgccustomdate2, sgccustomdate3


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 42) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'sgcid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "sgcid required numeric." : GoTo selesai
        End If
        'sgcjenis(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "sgcjenis required numeric." : GoTo selesai
        End If
        'sgcautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "sgcautonotransaksi required numeric." : GoTo selesai
        End If
        'sgctgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "sgctgl required date." : GoTo selesai
        End If
        'sgckodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "sgckodepa required numeric." : GoTo selesai
        End If
        'sgckontak(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "sgckontak required numeric." : GoTo selesai
        End If
        If (dataUtama(9) < 1) Then
            result(2) = "sgckontak can't be empty." : GoTo selesai
        End If
        'sgckurs(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "sgckurs required numeric." : GoTo selesai
        End If
        'sgcjumlah(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "sgcjumlah required numeric." : GoTo selesai
        End If
        'sgcjumlahvalas(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "sgcjumlahvalas required numeric." : GoTo selesai
        End If
        'sgcidsg(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "sgcidsg required numeric." : GoTo selesai
        End If
        'sgcstatus(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "sgcstatus required numeric." : GoTo selesai
        End If
        'sgcstatussebelumnya(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "sgcstatussebelumnya required numeric." : GoTo selesai
        End If
        'sgcjmlrevisi(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "sgcjmlrevisi required numeric." : GoTo selesai
        End If
        'sgccetakanke(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "sgccetakanke required numeric." : GoTo selesai
        End If
        'sgcisclose(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "sgcisclose required numeric." : GoTo selesai
        End If
        'sgcinputuser(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "sgcinputuser required numeric." : GoTo selesai
        End If
        'sgcinputtgl(24) As DateTime
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "sgcinputtgl required date." : GoTo selesai
        End If
        'sgcmodifikasiuser(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "sgcmodifikasiuser required numeric." : GoTo selesai
        End If
        'sgcmodifikasitgl(26) As DateTime
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "sgcmodifikasitgl required date." : GoTo selesai
        End If
        'sgcposting(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "sgcposting required numeric." : GoTo selesai
        End If
        'sgccustomint1(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "sgccustomint1 required numeric." : GoTo selesai
        End If
        'sgccustomint2(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "sgccustomint2 required numeric." : GoTo selesai
        End If
        'sgccustomint3(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "sgccustomint3 required numeric." : GoTo selesai
        End If
        'sgccustomdbl1(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "sgccustomdbl1 required numeric." : GoTo selesai
        End If
        'sgccustomdbl2(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "sgccustomdbl2 required numeric." : GoTo selesai
        End If
        'sgccustomdbl3(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "sgccustomdbl3 required numeric." : GoTo selesai
        End If
        'sgccustomdate1(39) As Date
        If (IsDate(dataUtama(39)) = False) Then
            result(2) = "sgccustomdate1 required date." : GoTo selesai
        End If
        'sgccustomdate2(40) As Date
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "sgccustomdate2 required date." : GoTo selesai
        End If
        'sgccustomdate3(41) As Date
        If (IsDate(dataUtama(41)) = False) Then
            result(2) = "sgccustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'sgccabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "sgccabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "sgccabang should not be more than 25 character." : GoTo selesai
        End If

        'sgclokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "sgclokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "sgclokasi should not be more than 25 character." : GoTo selesai
        End If

        'sgcsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "sgcsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "sgcsumber should not be more than 10 character." : GoTo selesai
        End If

        'sgcnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "sgcnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "sgcnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'sgctgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "sgctgl can't be empty" : GoTo selesai
        End If

        'sgcmatauang(13) As String
        If Len(dataUtama(13)) = 0 Then
            result(2) = "sgcmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(13)) > 25 Then
            result(2) = "sgcmatauang should not be more than 25 character." : GoTo selesai
        End If

        'sgckurs(14) As Double
        If Len(dataUtama(14)) = 0 Then
            result(2) = "sgckurs can't be empty" : GoTo selesai
        End If

        'sgcjumlah(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "sgcjumlah can't be empty" : GoTo selesai
        End If

        'sgcjumlahvalas(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "sgcjumlahvalas can't be empty" : GoTo selesai
        End If

        'sgcinputtgl(24) As DateTime
        If Len(dataUtama(24)) = 0 Then
            result(2) = "sgcinputtgl can't be empty" : GoTo selesai
        End If

        'sgcmodifikasitgl(26) As DateTime
        If Len(dataUtama(26)) = 0 Then
            result(2) = "sgcmodifikasitgl can't be empty" : GoTo selesai
        End If

        'sgccustomdbl1(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "sgccustomdbl1 can't be empty" : GoTo selesai
        End If

        'sgccustomdbl2(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "sgccustomdbl2 can't be empty" : GoTo selesai
        End If

        'sgccustomdbl3(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "sgccustomdbl3 can't be empty" : GoTo selesai
        End If

        'sgccustomdate1(39) As Date
        If Len(dataUtama(39)) = 0 Then
            result(2) = "sgccustomdate1 can't be empty" : GoTo selesai
        End If

        'sgccustomdate2(40) As Date
        If Len(dataUtama(40)) = 0 Then
            result(2) = "sgccustomdate2 can't be empty" : GoTo selesai
        End If

        'sgccustomdate3(41) As Date
        If Len(dataUtama(41)) = 0 Then
            result(2) = "sgccustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "sgcid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgccabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgclokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcjenis", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgctgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgckodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgckontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgckontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgccatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgckurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcjumlah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcjumlahvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcidsg", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgccetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgccustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgccustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgccustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgccustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgccustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgccustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgccustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgccustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgccustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgccustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgccustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgccustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgccustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgccustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "sgcid~sgccabang~sgclokasi~sgcsumber~sgcjenis~sgcautonotransaksi~sgcnotransaksi~sgctgl~sgckodepa~sgckontak~sgckontakperson~sgcuraian~sgccatatan~sgcmatauang~sgckurs~sgcjumlah~sgcjumlahvalas~sgcidsg~sgcstatus~sgcstatussebelumnya~sgcjmlrevisi~sgccetakanke~sgcisclose~sgcinputuser~sgcinputtgl~sgcmodifikasiuser~sgcmodifikasitgl~sgcposting~sgccustomtext1~sgccustomtext2~sgccustomtext3~sgccustomtext4~sgccustomtext5~sgccustomint1~sgccustomint2~sgccustomint3~sgccustomdbl1~sgccustomdbl2~sgccustomdbl3~sgccustomdate1~sgccustomdate2~sgccustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idsgcdetail(0) As Integer, idsgc(1) As Integer, nogiro(2) As String, kontak(3) As Integer, matauang(4) As String, 
        'kurs(5) As Double, jumlah(6) As Double, jumlahvalas(7) As Double, bank(8) As String, noacbank(9) As String, 
        'rekbank(10) As String, rekgiro(11) As String, tgljatuhtempo(12) As Date, catatan(13) As String, urutan(14) As Integer, 
        'statusgiro(15) As Integer, idsgdetail(16) As Integer, isclose(17) As Integer, customtext1(18) As String, customtext2(19) As String, 
        'customtext3(20) As String, customdbl1(21) As Double, customdbl2(22) As Double, customdbl3(23) As Double, customdate1(24) As Date, 
        'customdate2(25) As Date, customdate3(26) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idsgcdetail, idsgc, nogiro, kontak, matauang, kurs, jumlah, 
        'jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, 
        'urutan, statusgiro, idsgdetail, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idsgcdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idsgc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "nogiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "bank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "noacbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekgiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "statusgiro", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idsgdetail", AsEnumTypeData.AsInt64)
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


        'Variabel Validasi
        Dim ftExistGiro As String = "", ftGiro As String = "", vNogiro As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 27) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idsgcdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idsgcdetail required numeric." : GoTo selesai
            End If
            'idsgc(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idsgc required numeric." : GoTo selesai
            End If
            'kontak(3) As Integer
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - kontak required numeric." : GoTo selesai
            End If
            'kurs(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'jumlah(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - jumlah required numeric." : GoTo selesai
            End If
            'jumlahvalas(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - jumlahvalas required numeric." : GoTo selesai
            End If
            'tgljatuhtempo(12) As Date
            If (IsDate(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - tgljatuhtempo required date." : GoTo selesai
            End If
            'urutan(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'statusgiro(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - statusgiro required numeric." : GoTo selesai
            End If
            'idsgdetail(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - idsgdetail required numeric." : GoTo selesai
            End If
            'isclose(17) As Integer
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(21) As Double
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(22) As Double
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(23) As Double
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(24) As Date
            If (IsDate(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(25) As Date
            If (IsDate(dataRowDetail(25)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(26) As Date
            If (IsDate(dataRowDetail(26)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'nogiro(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - nogiro can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - nogiro should not be more than 25 character." : GoTo selesai
            End If

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

            'jumlah(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - jumlah can't be empty" : GoTo selesai
            End If

            'jumlahvalas(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - jumlahvalas can't be empty" : GoTo selesai
            End If

            'tgljatuhtempo(12) As Date
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - tgljatuhtempo can't be empty" : GoTo selesai
            End If

            'customdbl1(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(22) As Double
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(23) As Double
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(24) As Date
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(25) As Date
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(26) As Date
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idsgcdetail~idsgc~nogiro~kontak~matauang~kurs~jumlah~jumlahvalas~bank~noacbank~rekbank~rekgiro~tgljatuhtempo~catatan~urutan~statusgiro~idsgdetail~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If


            'BUAT FILTER VALIDASI STATUS GIRO ---------------------------
            'nogiro(2) As String
            vNogiro = dataRowDetail(2)

            'CEK DATA EXIST
            ftExistGiro = IIf(Len(ftExistGiro.ToString) = 0, "", ftExistGiro & " UNION ")
            ftExistGiro = String.Concat(ftExistGiro, "SELECT EXISTS(SELECT 1 FROM m2_giro_list WHERE glnogiro = '" & vNogiro & "' LIMIT 1) as rowExists, '" & vNogiro & "' as glnogiro")

            'Validasi Status Giro
            ftGiro = IIf(Len(ftGiro.ToString) = 0, "", ftGiro & " OR ")
            ftGiro = String.Concat(ftGiro, "(glnogiro = '" & vNogiro & "')")
            'END OF BUAT FILTER VALIDASI STATUS GIRO --------------------

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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("sgctgl")), AsFormatTanggal(drutama("sgctgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================

                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("sgcstatus") = 2 Then
                    Dim rsValidasi As String = ValidasiSimpan(ftExistGiro, ftGiro)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================

                'HITUNG TOTAL BERDASARKAN DATA DETAIL ===================
                drutama("sgcjumlah") = AsDataTableDSum(dtdetail, "jumlah")
                drutama("sgcjumlahvalas") = AsDataTableDSum(dtdetail, "jumlahvalas")
                'END OF HITUNG TOTAL BERDASARKAN DATA DETAIL ============

                If isUpdate Then
                    result(4) = drutama("sgcid")
                    notransaksi = drutama("sgcnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(sgcid), sgcnotransaksi FROM M2_sgc WHERE sgcid='" & result(4) & "' AND sgcstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(sgcid) FROM m2_sgc WHERE sgcnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m2_sgc_history
                        Dim rsSimpanHistory As String = SimpanHistory.M2_Sgc_HistorySimpan("" & paramSplit(0) & "★M2_Sgc_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("sgcsumber")) & "▼" & FixQuotes(drutama("sgcid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M2_Sgc set sgccabang  = '" & FixQuotes(drutama("sgccabang")) & "', sgclokasi  = '" & FixQuotes(drutama("sgclokasi")) & "', sgcsumber  = '" & FixQuotes(drutama("sgcsumber")) & "', sgcjenis  = " & drutama("sgcjenis") & ", sgcautonotransaksi  = " & drutama("sgcautonotransaksi") & ", sgcnotransaksi  = '" & notransaksi & "', sgctgl  = '" & FixQuotes(AsFormatTanggal(drutama("sgctgl"))) & "', sgckodepa  = " & drutama("sgckodepa") & ", sgckontak  = " & drutama("sgckontak") & ", sgckontakperson  = '" & FixQuotes(drutama("sgckontakperson")) & "', sgcuraian  = '" & FixQuotes(drutama("sgcuraian")) & "', sgccatatan  = '" & FixQuotes(drutama("sgccatatan")) & "', sgcmatauang  = '" & FixQuotes(drutama("sgcmatauang")) & "', sgckurs  = '" & FixDouble(drutama("sgckurs")) & "', sgcjumlah  = '" & FixDouble(drutama("sgcjumlah")) & "', sgcjumlahvalas  = '" & FixDouble(drutama("sgcjumlahvalas")) & "', sgcidsg  = " & drutama("sgcidsg") & ", sgcstatus  = " & drutama("sgcstatus") & ", sgcstatussebelumnya  = " & drutama("sgcstatussebelumnya") & ", sgcjmlrevisi  = sgcjmlrevisi+1, sgccetakanke  = " & drutama("sgccetakanke") & ", sgcisclose  = " & drutama("sgcisclose") & ", sgcmodifikasiuser  = " & drutama("sgcmodifikasiuser") & ", sgcmodifikasitgl  = NOW(), sgcposting  = 0, sgccustomtext1  = '" & FixQuotes(drutama("sgccustomtext1")) & "', sgccustomtext2  = '" & FixQuotes(drutama("sgccustomtext2")) & "', sgccustomtext3  = '" & FixQuotes(drutama("sgccustomtext3")) & "', sgccustomtext4  = '" & FixQuotes(drutama("sgccustomtext4")) & "', sgccustomtext5  = '" & FixQuotes(drutama("sgccustomtext5")) & "', sgccustomint1  = " & drutama("sgccustomint1") & ", sgccustomint2  = " & drutama("sgccustomint2") & ", sgccustomint3  = " & drutama("sgccustomint3") & ", sgccustomdbl1  = '" & FixDouble(drutama("sgccustomdbl1")) & "', sgccustomdbl2  = '" & FixDouble(drutama("sgccustomdbl2")) & "', sgccustomdbl3  = '" & FixDouble(drutama("sgccustomdbl3")) & "', sgccustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("sgccustomdate1"))) & "', sgccustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("sgccustomdate2"))) & "', sgccustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("sgccustomdate3"))) & "' where sgcid = '" & drutama("sgcid") & "'"
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

                    If drutama("sgcautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("sgccabang"), drutama("sgclokasi"), drutama("sgcsumber"), drutama("sgctgl"))
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
                        notransaksi = drutama("sgcnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(sgcid) FROM m2_sgc WHERE sgcnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M2_Sgc (sgccabang, sgclokasi, sgcsumber, sgcjenis, sgcautonotransaksi, sgcnotransaksi, sgctgl, sgckodepa, sgckontak, sgckontakperson, sgcuraian, sgccatatan, sgcmatauang, sgckurs, sgcjumlah, sgcjumlahvalas, sgcidsg, sgcstatus, sgcstatussebelumnya, sgcjmlrevisi, sgccetakanke, sgcisclose, sgcinputuser, sgcinputtgl, sgcmodifikasiuser, sgcmodifikasitgl, sgcposting, sgccustomtext1, sgccustomtext2, sgccustomtext3, sgccustomtext4, sgccustomtext5, sgccustomint1, sgccustomint2, sgccustomint3, sgccustomdbl1, sgccustomdbl2, sgccustomdbl3, sgccustomdate1, sgccustomdate2, sgccustomdate3) values('" & FixQuotes(drutama("sgccabang")) & "', '" & FixQuotes(drutama("sgclokasi")) & "', '" & FixQuotes(drutama("sgcsumber")) & "', " & drutama("sgcjenis") & ", " & drutama("sgcautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("sgctgl"))) & "', " & drutama("sgckodepa") & ", " & drutama("sgckontak") & ", '" & FixQuotes(drutama("sgckontakperson")) & "', '" & FixQuotes(drutama("sgcuraian")) & "', '" & FixQuotes(drutama("sgccatatan")) & "', '" & FixQuotes(drutama("sgcmatauang")) & "', '" & FixDouble(drutama("sgckurs")) & "', '" & FixDouble(drutama("sgcjumlah")) & "', '" & FixDouble(drutama("sgcjumlahvalas")) & "', " & drutama("sgcidsg") & ", " & drutama("sgcstatus") & ", " & drutama("sgcstatussebelumnya") & ", " & drutama("sgcjmlrevisi") & ", " & drutama("sgccetakanke") & ", " & drutama("sgcisclose") & ", " & drutama("sgcinputuser") & ", NOW(), " & drutama("sgcmodifikasiuser") & ", '1971-01-01 00:00:00', 0, '" & FixQuotes(drutama("sgccustomtext1")) & "', '" & FixQuotes(drutama("sgccustomtext2")) & "', '" & FixQuotes(drutama("sgccustomtext3")) & "', '" & FixQuotes(drutama("sgccustomtext4")) & "', '" & FixQuotes(drutama("sgccustomtext5")) & "', " & drutama("sgccustomint1") & ", " & drutama("sgccustomint2") & ", " & drutama("sgccustomint3") & ", '" & FixDouble(drutama("sgccustomdbl1")) & "', '" & FixDouble(drutama("sgccustomdbl2")) & "', '" & FixDouble(drutama("sgccustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sgccustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sgccustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sgccustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select sgcid from M2_sgc where sgcnotransaksi='" & notransaksi & "' AND sgcinputuser= '" & userid & "' order by sgcmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M2_Sgc_Detail where idsgc = '" & result(4) & "'"
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
                    Dim strValue2 As New StringBuilder, strGiro As New StringBuilder, strRekgiro As New StringBuilder
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idsgcdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("nogiro")) & "', " & dr1("kontak") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljatuhtempo"))) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("statusgiro") & ", " & dr1("idsgdetail") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                        If drutama("sgcstatus") = 2 Then
                            'filter
                            strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", " OR "))
                            strGiro.Append("(glnogiro = '" & FixQuotes(dr1("nogiro")) & "')")
                            'rekgiro
                            strRekgiro.Append(" WHEN '" & FixQuotes(dr1("nogiro")) & "' THEN '" & FixQuotes(dr1("rekgiro")) & "' ")
                        End If
                    Next
                    sql = "Insert into M2_Sgc_Detail(idsgcdetail, idsgc, nogiro, kontak, matauang, kurs, jumlah, jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, urutan, statusgiro, idsgdetail, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'update glstatus, gltglcair, glrekgiro m2_giro_list
                    If drutama("sgcstatus") = 2 Then
                        'cek status giro
                        Dim dtValidasi As DataTable = AsDataTableAmbilDariDB("SELECT glnogiro FROM m2_giro_list WHERE glstatus = 1 AND (" & strGiro.ToString & ")")
                        If dtValidasi.Rows.Count > 0 Then result(2) = "Can't update giro '" & dtValidasi.Rows(0)(0) & "'. It has related transactions." : Trans.Rollback() : GoTo selesai

                        'update giro                   glstatus                                , gltglcair                              , glrekgiro                                                                            filter
                        sql = "UPDATE m2_giro_list SET glstatus = '" & drutama("sgcjenis") & "', gltglcair = '" & drutama("sgctgl") & "', glrekgiro = (CASE glnogiro " & strRekgiro.ToString & " ELSE glrekgiro END) WHERE " & strGiro.ToString & ""
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
                Dim sumber As String = "SGC", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("sgcstatus") = 2 Then
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
    Public Function M2_SgcUpdateStatusOld(ByVal param As String) As String

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
            Dim sumber As String = "Sgc", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Sgctgl, Sgcnotransaksi, Sgcstatus FROM m2_Sgc WHERE Sgcid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Sgcstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m2_sgc_history
            Dim rsSimpanHistory As String = SimpanHistory.M2_Sgc_HistorySimpan("" & paramSplit(0) & "★M2_Sgc_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m2_sgc_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                'PROSES GIRO ====================================================================
                Dim strGiro As New StringBuilder
                'ambil giro dari detail
                dtdetail = AsDataTableAmbilDariDB("SELECT nogiro FROM m2_sgc_detail WHERE idsgc = '" & idtransaksi & "'")
                If dtdetail.Rows.Count > 0 Then
                    'buat filter query untuk ambil giro m2_giro_list
                    For Each dr1 As DataRow In dtdetail.Rows
                        strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", " OR "))
                        strGiro.Append("(glnogiro = '" & FixQuotes(dr1("nogiro")) & "')")
                    Next
                    'ambil giro dari m2_giro_list
                    dtdetail = AsDataTableAmbilDariDB("SELECT glnogiro, glstatus FROM m2_giro_list WHERE (" & strGiro.ToString & ")")
                    If dtdetail.Rows.Count > 0 Then
                        'cek giro yang sudah terkait dengan notransaksi lain
                        Dim dtValidasi As DataTable = AsDataTableFilterSortDt(dtdetail, "glstatus = '1'")
                        If dtValidasi.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai


                        'AMBIL REKENING GIRO KELUAR DARI M0_SETTING
                        Dim dtrekgiro As DataTable = AsDataTableAmbilDariDB("SELECT snilai FROM m0_setting WHERE smodule=0 AND sgrup='akun' AND skode='GiroKeluar'")
                        Dim rekgiro As String = ""
                        If dtrekgiro.Rows.Count > 0 Then
                            rekgiro = dtrekgiro.Rows(0)(0).ToString
                        Else
                            result(2) = "Setting Giro Out CoA not found." : Trans.Rollback() : GoTo selesai
                        End If

                        'UPDATE STATUS GIRO MENJADI BELUM CAIR (0) DAN REKGIRO = SETTING PIUTANG GIRO KELUAR
                        'update m2_giro_list           glstatus                                                                                                                 filter
                        sql = "UPDATE m2_giro_list SET glstatus = '0', gltglcair = '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', glrekgiro = '" & rekgiro & "' WHERE (" & strGiro.ToString & ")"
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
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If
                'END OF PROSES GIRO =============================================================

                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'SGC' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M2_Sgc SET Sgcstatus = " & nilaiStatus & ", Sgcmodifikasiuser='" & userid & "', Sgcmodifikasitgl = NOW(), Sgcposting = 0, Sgcpostingtgl = '1971-01-01 00:00:00', Sgcjmlrevisi = Sgcjmlrevisi + 1 WHERE Sgcid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_SgcSearch(PostWsSearch(paramSplit(0), "M2_SgcSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_SgcDeleteOld(ByVal param As String) As String

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
            Dim sumber As String = "Sgc", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Sgcid, Sgcnotransaksi FROM m2_Sgc WHERE Sgcid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT sgccabang, sgclokasi, sgcsumber, sgcautonotransaksi, sgcnotransaksi, sgctgl"
            sql &= " FROM M2_sgc"
            sql &= " WHERE sgcid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("sgccabang")
                lokasi = dtNomorNext.Rows(0)("sgclokasi")
                sumber = dtNomorNext.Rows(0)("sgcsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("sgcautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("sgcnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("sgctgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE JURNAL
            sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'SGC' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M2_Sgc_Detail WHERE idSgc = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M2_Sgc WHERE Sgcid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_SgcSearch(PostWsSearch(paramSplit(0), "M2_SgcSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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