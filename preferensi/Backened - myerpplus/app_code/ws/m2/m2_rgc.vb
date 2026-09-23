Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m2_rgc
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M2_RgcSimpan(ByVal param As String) As String
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
        'rgcid(0) As Integer, rgccabang(1) As String, rgclokasi(2) As String, rgcsumber(3) As String, rgcjenis(4) As Integer, 
        'rgcautonotransaksi(5) As Integer, rgcnotransaksi(6) As String, rgctgl(7) As Date, rgckodepa(8) As Integer, rgckontak(9) As Integer, 
        'rgckontakperson(10) As String, rgcuraian(11) As String, rgccatatan(12) As String, rgcmatauang(13) As String, rgckurs(14) As Double, 
        'rgcjumlah(15) As Double, rgcjumlahvalas(16) As Double, rgcidrg(17) As Integer, rgcstatus(18) As Integer, rgcstatussebelumnya(19) As Integer, 
        'rgcjmlrevisi(20) As Integer, rgccetakanke(21) As Integer, rgcisclose(22) As Integer, rgcinputuser(23) As Integer, rgcinputtgl(24) As DateTime, 
        'rgcmodifikasiuser(25) As Integer, rgcmodifikasitgl(26) As DateTime, rgcposting(27) As Integer, rgccustomtext1(28) As String, rgccustomtext2(29) As String, 
        'rgccustomtext3(30) As String, rgccustomtext4(31) As String, rgccustomtext5(32) As String, rgccustomint1(33) As Integer, rgccustomint2(34) As Integer, 
        'rgccustomint3(35) As Integer, rgccustomdbl1(36) As Double, rgccustomdbl2(37) As Double, rgccustomdbl3(38) As Double, rgccustomdate1(39) As Date, 
        'rgccustomdate2(40) As Date, rgccustomdate3(41) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'rgcid, rgccabang, rgclokasi, rgcsumber, rgcjenis, rgcautonotransaksi, rgcnotransaksi, 
        'rgctgl, rgckodepa, rgckontak, rgckontakperson, rgcuraian, rgccatatan, rgcmatauang, 
        'rgckurs, rgcjumlah, rgcjumlahvalas, rgcidrg, rgcstatus, rgcstatussebelumnya, rgcjmlrevisi, 
        'rgccetakanke, rgcisclose, rgcinputuser, rgcinputtgl, rgcmodifikasiuser, rgcmodifikasitgl, rgcposting, 
        'rgccustomtext1, rgccustomtext2, rgccustomtext3, rgccustomtext4, rgccustomtext5, rgccustomint1, rgccustomint2, 
        'rgccustomint3, rgccustomdbl1, rgccustomdbl2, rgccustomdbl3, rgccustomdate1, rgccustomdate2, rgccustomdate3


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 42) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'rgcid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "rgcid required numeric." : GoTo selesai
        End If
        'rgcjenis(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "rgcjenis required numeric." : GoTo selesai
        End If
        'rgcautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "rgcautonotransaksi required numeric." : GoTo selesai
        End If
        'rgctgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "rgctgl required date." : GoTo selesai
        End If
        'rgckodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "rgckodepa required numeric." : GoTo selesai
        End If
        'rgckontak(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "rgckontak required numeric." : GoTo selesai
        End If
        If (dataUtama(9) < 1) Then
            result(2) = "rgckontak can't be empty." : GoTo selesai
        End If
        'rgckurs(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "rgckurs required numeric." : GoTo selesai
        End If
        'rgcjumlah(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "rgcjumlah required numeric." : GoTo selesai
        End If
        'rgcjumlahvalas(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "rgcjumlahvalas required numeric." : GoTo selesai
        End If
        'rgcidrg(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "rgcidrg required numeric." : GoTo selesai
        End If
        'rgcstatus(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "rgcstatus required numeric." : GoTo selesai
        End If
        'rgcstatussebelumnya(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "rgcstatussebelumnya required numeric." : GoTo selesai
        End If
        'rgcjmlrevisi(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "rgcjmlrevisi required numeric." : GoTo selesai
        End If
        'rgccetakanke(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "rgccetakanke required numeric." : GoTo selesai
        End If
        'rgcisclose(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "rgcisclose required numeric." : GoTo selesai
        End If
        'rgcinputuser(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "rgcinputuser required numeric." : GoTo selesai
        End If
        'rgcinputtgl(24) As DateTime
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "rgcinputtgl required date." : GoTo selesai
        End If
        'rgcmodifikasiuser(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "rgcmodifikasiuser required numeric." : GoTo selesai
        End If
        'rgcmodifikasitgl(26) As DateTime
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "rgcmodifikasitgl required date." : GoTo selesai
        End If
        'rgcposting(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "rgcposting required numeric." : GoTo selesai
        End If
        'rgccustomint1(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "rgccustomint1 required numeric." : GoTo selesai
        End If
        'rgccustomint2(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "rgccustomint2 required numeric." : GoTo selesai
        End If
        'rgccustomint3(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "rgccustomint3 required numeric." : GoTo selesai
        End If
        'rgccustomdbl1(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "rgccustomdbl1 required numeric." : GoTo selesai
        End If
        'rgccustomdbl2(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "rgccustomdbl2 required numeric." : GoTo selesai
        End If
        'rgccustomdbl3(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "rgccustomdbl3 required numeric." : GoTo selesai
        End If
        'rgccustomdate1(39) As Date
        If (IsDate(dataUtama(39)) = False) Then
            result(2) = "rgccustomdate1 required date." : GoTo selesai
        End If
        'rgccustomdate2(40) As Date
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "rgccustomdate2 required date." : GoTo selesai
        End If
        'rgccustomdate3(41) As Date
        If (IsDate(dataUtama(41)) = False) Then
            result(2) = "rgccustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'rgccabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "rgccabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "rgccabang should not be more than 25 character." : GoTo selesai
        End If

        'rgclokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "rgclokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "rgclokasi should not be more than 25 character." : GoTo selesai
        End If

        'rgcsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "rgcsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "rgcsumber should not be more than 10 character." : GoTo selesai
        End If

        'rgcnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "rgcnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "rgcnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'rgctgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "rgctgl can't be empty" : GoTo selesai
        End If

        'rgcmatauang(13) As String
        If Len(dataUtama(13)) = 0 Then
            result(2) = "rgcmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(13)) > 25 Then
            result(2) = "rgcmatauang should not be more than 25 character." : GoTo selesai
        End If

        'rgckurs(14) As Double
        If Len(dataUtama(14)) = 0 Then
            result(2) = "rgckurs can't be empty" : GoTo selesai
        End If

        'rgcjumlah(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "rgcjumlah can't be empty" : GoTo selesai
        End If

        'rgcjumlahvalas(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "rgcjumlahvalas can't be empty" : GoTo selesai
        End If

        'rgcinputtgl(24) As DateTime
        If Len(dataUtama(24)) = 0 Then
            result(2) = "rgcinputtgl can't be empty" : GoTo selesai
        End If

        'rgcmodifikasitgl(26) As DateTime
        If Len(dataUtama(26)) = 0 Then
            result(2) = "rgcmodifikasitgl can't be empty" : GoTo selesai
        End If

        'rgccustomdbl1(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "rgccustomdbl1 can't be empty" : GoTo selesai
        End If

        'rgccustomdbl2(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "rgccustomdbl2 can't be empty" : GoTo selesai
        End If

        'rgccustomdbl3(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "rgccustomdbl3 can't be empty" : GoTo selesai
        End If

        'rgccustomdate1(39) As Date
        If Len(dataUtama(39)) = 0 Then
            result(2) = "rgccustomdate1 can't be empty" : GoTo selesai
        End If

        'rgccustomdate2(40) As Date
        If Len(dataUtama(40)) = 0 Then
            result(2) = "rgccustomdate2 can't be empty" : GoTo selesai
        End If

        'rgccustomdate3(41) As Date
        If Len(dataUtama(41)) = 0 Then
            result(2) = "rgccustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "rgcid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgccabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgclokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcjenis", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgctgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgckodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgckontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgckontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgccatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgckurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcjumlah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcjumlahvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcidrg", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgccetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgccustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgccustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgccustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgccustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgccustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgccustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgccustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgccustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgccustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgccustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgccustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgccustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgccustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgccustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "rgcid~rgccabang~rgclokasi~rgcsumber~rgcjenis~rgcautonotransaksi~rgcnotransaksi~rgctgl~rgckodepa~rgckontak~rgckontakperson~rgcuraian~rgccatatan~rgcmatauang~rgckurs~rgcjumlah~rgcjumlahvalas~rgcidrg~rgcstatus~rgcstatussebelumnya~rgcjmlrevisi~rgccetakanke~rgcisclose~rgcinputuser~rgcinputtgl~rgcmodifikasiuser~rgcmodifikasitgl~rgcposting~rgccustomtext1~rgccustomtext2~rgccustomtext3~rgccustomtext4~rgccustomtext5~rgccustomint1~rgccustomint2~rgccustomint3~rgccustomdbl1~rgccustomdbl2~rgccustomdbl3~rgccustomdate1~rgccustomdate2~rgccustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idrgcdetail(0) As Integer, idrgc(1) As Integer, nogiro(2) As String, kontak(3) As Integer, matauang(4) As String, 
        'kurs(5) As Double, jumlah(6) As Double, jumlahvalas(7) As Double, bank(8) As String, noacbank(9) As String, 
        'rekbank(10) As String, rekgiro(11) As String, tgljatuhtempo(12) As Date, catatan(13) As String, urutan(14) As Integer, 
        'statusgiro(15) As Integer, idrgdetail(16) As Integer, isclose(17) As Integer, customtext1(18) As String, customtext2(19) As String, 
        'customtext3(20) As String, customdbl1(21) As Double, customdbl2(22) As Double, customdbl3(23) As Double, customdate1(24) As Date, 
        'customdate2(25) As Date, customdate3(26) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idrgcdetail, idrgc, nogiro, kontak, matauang, kurs, jumlah, 
        'jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, 
        'urutan, statusgiro, idrgdetail, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idrgcdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idrgc", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "idrgdetail", AsEnumTypeData.AsInt64)
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
            'idrgcdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idrgcdetail required numeric." : GoTo selesai
            End If
            'idrgc(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idrgc required numeric." : GoTo selesai
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
            'idrgdetail(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - idrgdetail required numeric." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idrgcdetail~idrgc~nogiro~kontak~matauang~kurs~jumlah~jumlahvalas~bank~noacbank~rekbank~rekgiro~tgljatuhtempo~catatan~urutan~statusgiro~idrgdetail~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26)) = False Then
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
                Dim vModuleId As Integer = 2, vMenuId As Integer = 11
                Select Case drutama("rgcstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("rgctgl")), AsFormatTanggal(drutama("rgctgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================

                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("rgcstatus") = 2 Or drutama("rgcstatus") = 1 Or drutama("rgcstatus") = 8 Or drutama("rgcstatus") = 9 Or drutama("rgcstatus") = 10 Or drutama("rgcstatus") = 11 Then
                    Dim rsValidasi As String = ValidasiSimpan(ftExistGiro, ftGiro)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================

                'HITUNG TOTAL BERDASARKAN DATA DETAIL ===================
                drutama("rgcjumlah") = AsDataTableDSum(dtdetail, "jumlah")
                drutama("rgcjumlahvalas") = AsDataTableDSum(dtdetail, "jumlahvalas")
                'END OF HITUNG TOTAL BERDASARKAN DATA DETAIL ============

                If isUpdate Then
                    result(4) = drutama("rgcid")
                    notransaksi = drutama("rgcnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(rgcid), rgcnotransaksi FROM M2_rgc WHERE rgcid='" & result(4) & "' AND rgcstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("rgcautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rgccabang"), drutama("rgclokasi"), drutama("rgcsumber"), drutama("rgctgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(rgcid) FROM m2_rgc WHERE rgcnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m2_rgc_history
                        Dim rsSimpanHistory As String = SimpanHistory.M2_Rgc_HistorySimpan("" & paramSplit(0) & "★M2_Rgc_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("rgcsumber")) & "▼" & FixQuotes(drutama("rgcid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M2_Rgc set rgccabang  = '" & FixQuotes(drutama("rgccabang")) & "', rgclokasi  = '" & FixQuotes(drutama("rgclokasi")) & "', rgcsumber  = '" & FixQuotes(drutama("rgcsumber")) & "', rgcjenis  = " & drutama("rgcjenis") & ", rgcautonotransaksi  = " & drutama("rgcautonotransaksi") & ", rgcnotransaksi  = '" & notransaksi & "', rgctgl  = '" & FixQuotes(AsFormatTanggal(drutama("rgctgl"))) & "', rgckodepa  = " & drutama("rgckodepa") & ", rgckontak  = " & drutama("rgckontak") & ", rgckontakperson  = '" & FixQuotes(drutama("rgckontakperson")) & "', rgcuraian  = '" & FixQuotes(drutama("rgcuraian")) & "', rgccatatan  = '" & FixQuotes(drutama("rgccatatan")) & "', rgcmatauang  = '" & FixQuotes(drutama("rgcmatauang")) & "', rgckurs  = '" & FixDouble(drutama("rgckurs")) & "', rgcjumlah  = '" & FixDouble(drutama("rgcjumlah")) & "', rgcjumlahvalas  = '" & FixDouble(drutama("rgcjumlahvalas")) & "', rgcidrg  = " & drutama("rgcidrg") & ", rgcstatus  = " & drutama("rgcstatus") & ", rgcstatussebelumnya  = " & drutama("rgcstatussebelumnya") & ", rgcjmlrevisi  = rgcjmlrevisi+1, rgccetakanke  = " & drutama("rgccetakanke") & ", rgcisclose  = " & drutama("rgcisclose") & ", rgcmodifikasiuser  = " & drutama("rgcmodifikasiuser") & ", rgcmodifikasitgl  = NOW(), rgcposting  = 0, rgccustomtext1  = '" & FixQuotes(drutama("rgccustomtext1")) & "', rgccustomtext2  = '" & FixQuotes(drutama("rgccustomtext2")) & "', rgccustomtext3  = '" & FixQuotes(drutama("rgccustomtext3")) & "', rgccustomtext4  = '" & FixQuotes(drutama("rgccustomtext4")) & "', rgccustomtext5  = '" & FixQuotes(drutama("rgccustomtext5")) & "', rgccustomint1  = " & drutama("rgccustomint1") & ", rgccustomint2  = " & drutama("rgccustomint2") & ", rgccustomint3  = " & drutama("rgccustomint3") & ", rgccustomdbl1  = '" & FixDouble(drutama("rgccustomdbl1")) & "', rgccustomdbl2  = '" & FixDouble(drutama("rgccustomdbl2")) & "', rgccustomdbl3  = '" & FixDouble(drutama("rgccustomdbl3")) & "', rgccustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("rgccustomdate1"))) & "', rgccustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("rgccustomdate2"))) & "', rgccustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("rgccustomdate3"))) & "' where rgcid = '" & drutama("rgcid") & "'"
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

                    If drutama("rgcautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rgccabang"), drutama("rgclokasi"), drutama("rgcsumber"), drutama("rgctgl"))
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
                        notransaksi = drutama("rgcnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(rgcid) FROM m2_rgc WHERE rgcnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M2_Rgc (rgccabang, rgclokasi, rgcsumber, rgcjenis, rgcautonotransaksi, rgcnotransaksi, rgctgl, rgckodepa, rgckontak, rgckontakperson, rgcuraian, rgccatatan, rgcmatauang, rgckurs, rgcjumlah, rgcjumlahvalas, rgcidrg, rgcstatus, rgcstatussebelumnya, rgcjmlrevisi, rgccetakanke, rgcisclose, rgcinputuser, rgcinputtgl, rgcmodifikasiuser, rgcmodifikasitgl, rgcposting, rgccustomtext1, rgccustomtext2, rgccustomtext3, rgccustomtext4, rgccustomtext5, rgccustomint1, rgccustomint2, rgccustomint3, rgccustomdbl1, rgccustomdbl2, rgccustomdbl3, rgccustomdate1, rgccustomdate2, rgccustomdate3) values('" & FixQuotes(drutama("rgccabang")) & "', '" & FixQuotes(drutama("rgclokasi")) & "', '" & FixQuotes(drutama("rgcsumber")) & "', " & drutama("rgcjenis") & ", " & drutama("rgcautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("rgctgl"))) & "', " & drutama("rgckodepa") & ", " & drutama("rgckontak") & ", '" & FixQuotes(drutama("rgckontakperson")) & "', '" & FixQuotes(drutama("rgcuraian")) & "', '" & FixQuotes(drutama("rgccatatan")) & "', '" & FixQuotes(drutama("rgcmatauang")) & "', '" & FixDouble(drutama("rgckurs")) & "', '" & FixDouble(drutama("rgcjumlah")) & "', '" & FixDouble(drutama("rgcjumlahvalas")) & "', " & drutama("rgcidrg") & ", " & drutama("rgcstatus") & ", " & drutama("rgcstatussebelumnya") & ", " & drutama("rgcjmlrevisi") & ", " & drutama("rgccetakanke") & ", " & drutama("rgcisclose") & ", " & drutama("rgcinputuser") & ", NOW(), " & drutama("rgcmodifikasiuser") & ", '1971-01-01 00:00:00', 0, '" & FixQuotes(drutama("rgccustomtext1")) & "', '" & FixQuotes(drutama("rgccustomtext2")) & "', '" & FixQuotes(drutama("rgccustomtext3")) & "', '" & FixQuotes(drutama("rgccustomtext4")) & "', '" & FixQuotes(drutama("rgccustomtext5")) & "', " & drutama("rgccustomint1") & ", " & drutama("rgccustomint2") & ", " & drutama("rgccustomint3") & ", '" & FixDouble(drutama("rgccustomdbl1")) & "', '" & FixDouble(drutama("rgccustomdbl2")) & "', '" & FixDouble(drutama("rgccustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rgccustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rgccustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rgccustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select rgcid from M2_rgc where rgcnotransaksi='" & notransaksi & "' AND rgcinputuser= '" & userid & "' order by rgcmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M2_Rgc_Detail where idrgc = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idrgcdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("nogiro")) & "', " & dr1("kontak") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljatuhtempo"))) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("statusgiro") & ", " & dr1("idrgdetail") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                        If drutama("rgcstatus") = 2 Then
                            'filter
                            strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", " OR "))
                            strGiro.Append("(glnogiro = '" & FixQuotes(dr1("nogiro")) & "')")
                            'rekgiro
                            strRekgiro.Append(" WHEN '" & FixQuotes(dr1("nogiro")) & "' THEN '" & FixQuotes(dr1("rekgiro")) & "' ")
                        End If
                    Next
                    sql = "Insert into M2_Rgc_Detail(idrgcdetail, idrgc, nogiro, kontak, matauang, kurs, jumlah, jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, urutan, statusgiro, idrgdetail, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'update glstatus, gltglcair, glrekgiro m2_giro_list
                    If drutama("rgcstatus") = 2 Then
                        'cek status giro
                        Dim dtValidasi As DataTable = AsDataTableAmbilDariDBCon("SELECT glnogiro FROM m2_giro_list WHERE glstatus = 1 AND (" & strGiro.ToString & ")", myConn)
                        If dtValidasi.Rows.Count > 0 Then result(2) = "Can't update giro '" & dtValidasi.Rows(0)(0) & "'. It has related transactions." : Trans.Rollback() : GoTo selesai

                        'update giro                   glstatus                                , gltglcair                              , glrekgiro                                                                  filter
                        sql = "UPDATE m2_giro_list SET glstatus = '" & drutama("rgcjenis") & "', gltglcair = '" & drutama("rgctgl") & "', glrekgiro = (CASE glnogiro " & strRekgiro.ToString & " ELSE glrekgiro END) WHERE " & strGiro.ToString & ""
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
                Dim sumber As String = "RGC", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("rgcstatus") = 2 Then
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
    Public Function M2_RgcUpdateStatus(ByVal param As String) As String

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
            Dim sumber As String = "Rgc", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Rgctgl, Rgcnotransaksi, Rgcstatus FROM m2_Rgc WHERE Rgcid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Rgcstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m2_rgc_history
            Dim rsSimpanHistory As String = SimpanHistory.M2_Rgc_HistorySimpan("" & paramSplit(0) & "★M2_Rgc_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m2_rgc_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                'PROSES GIRO ====================================================================
                Dim strGiro As New StringBuilder
                'ambil giro dari detail
                dtdetail = AsDataTableAmbilDariDBCon("SELECT nogiro FROM m2_rgc_detail WHERE idrgc = '" & idtransaksi & "'", myConn)
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
                        Dim dtValidasi As DataTable = AsDataTableFilterSortDt(dtdetail, "glstatus = '1'")
                        If dtValidasi.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai

                        'AMBIL REKENING GIRO MASUK DARI M0_SETTING
                        Dim dtrekgiro As DataTable = AsDataTableAmbilDariDBCon("SELECT snilai FROM m0_setting WHERE smodule=0 AND sgrup='akun' AND skode='GiroMasuk'", myConn)
                        Dim rekgiro As String = ""
                        If dtrekgiro.Rows.Count > 0 Then
                            rekgiro = dtrekgiro.Rows(0)(0).ToString
                        Else
                            result(2) = "Setting Giro In CoA not found." : Trans.Rollback() : GoTo selesai
                        End If

                        'UPDATE STATUS GIRO MENJADI BELUM CAIR (0) DAN REKGIRO = SETTING PIUTANG GIRO MASUK
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
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'RGC' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M2_Rgc SET Rgcstatus = " & nilaiStatus & ", Rgcmodifikasiuser='" & userid & "', Rgcmodifikasitgl = NOW(), Rgcposting = 0, Rgcpostingtgl = '1971-01-01 00:00:00', Rgcjmlrevisi = Rgcjmlrevisi + 1 WHERE Rgcid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_RgcSearch(PostWsSearch(paramSplit(0), "M2_RgcSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_RgcDelete(ByVal param As String) As String

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
            Dim sumber As String = "Rgc", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Rgcid, Rgcnotransaksi FROM m2_Rgc WHERE Rgcid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT rgccabang, rgclokasi, rgcsumber, rgcautonotransaksi, rgcnotransaksi, rgctgl"
            sql &= " FROM M2_rgc"
            sql &= " WHERE rgcid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("rgccabang")
                lokasi = dtNomorNext.Rows(0)("rgclokasi")
                sumber = dtNomorNext.Rows(0)("rgcsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("rgcautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("rgcnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("rgctgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE JURNAL
            sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'RGC' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M2_Rgc_Detail WHERE idRgc = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M2_Rgc WHERE Rgcid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_RgcSearch(PostWsSearch(paramSplit(0), "M2_RgcSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_RgcGetdataById(ByVal param As String) As String

        'M2_RgcGetdataById Utama --------------------------------------------------------
        'rgcid, rgccabang, rgclokasi, rgcsumber, rgcjenis, rgcautonotransaksi, rgcnotransaksi, 
        'rgctgl, rgckodepa, rgckontak, rgckontakperson, rgcuraian, rgccatatan, rgcmatauang, 
        'rgckurs, rgcjumlah, rgcjumlahvalas, rgcidrg, rgcstatus, rgcstatussebelumnya, rgcjmlrevisi, 
        'rgccetakanke, rgcisclose, rgcinputuser, rgcinputtgl, rgcmodifikasiuser, rgcmodifikasitgl, rgcposting, 
        'rgcpostingtgl, rgccustomtext1, rgccustomtext2, rgccustomtext3, rgccustomtext4, rgccustomtext5, rgccustomint1, 
        'rgccustomint2, rgccustomint3, rgccustomdbl1, rgccustomdbl2, rgccustomdbl3, rgccustomdate1, rgccustomdate2, 
        'rgccustomdate3, rgccabangnama, rgclokasinama, rgcjenisnama, rgckontakkode, rgckontaknama, rgcnotransaksirg, 
        'rgcstatusnama, rgcstatussebelumnyanama, rgcinputusernama, rgcmodifikasiusernama

        'M2_RgcGetdataById Detail -------------------------------------------------------
        'idrgcdetail, idrgc, nogiro, kontak, 
        'matauang, kurs, jumlah, jumlahvalas, bank, noacbank, rekbank, 
        'rekgiro, tgljatuhtempo, catatan, urutan, statusgiro, idrgdetail, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3, kontakkode, kontaknama, banknama, rekbanknama, rekgironama, 
        'statusgironama, rgnotransaksi

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

        Dim NmMemcached As String = "aplikasi1-M2_Rgc~M2_Rgc_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "rgcid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "rgcid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_rgc_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("rgcid"), 0), sptField,
                     FxDB(drutama("rgccabang"), ""), sptField,
                     FxDB(drutama("rgclokasi"), ""), sptField,
                     FxDB(drutama("rgcsumber"), ""), sptField,
                     FxDB(drutama("rgcjenis"), 0), sptField,
                     FxDB(drutama("rgcautonotransaksi"), 0), sptField,
                     FxDB(drutama("rgcnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rgctgl"), ""), formatTgl), sptField,
                     FxDB(drutama("rgckodepa"), 0), sptField,
                     FxDB(drutama("rgckontak"), 0), sptField,
                     FxDB(drutama("rgckontakperson"), ""), sptField,
                     FxDB(drutama("rgcuraian"), ""), sptField,
                     FxDB(drutama("rgccatatan"), ""), sptField,
                     FxDB(drutama("rgcmatauang"), ""), sptField,
                     FxDB(drutama("rgckurs"), 0), sptField,
                     FxDB(drutama("rgcjumlah"), 0), sptField,
                     FxDB(drutama("rgcjumlahvalas"), 0), sptField,
                     FxDB(drutama("rgcidrg"), 0), sptField,
                     FxDB(drutama("rgcstatus"), 0), sptField,
                     FxDB(drutama("rgcstatussebelumnya"), 0), sptField,
                     FxDB(drutama("rgcjmlrevisi"), 0), sptField,
                     FxDB(drutama("rgccetakanke"), 0), sptField,
                     FxDB(drutama("rgcisclose"), 0), sptField,
                     FxDB(drutama("rgcinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rgcinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rgcmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rgcmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rgcposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rgcpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rgccustomtext1"), ""), sptField,
                     FxDB(drutama("rgccustomtext2"), ""), sptField,
                     FxDB(drutama("rgccustomtext3"), ""), sptField,
                     FxDB(drutama("rgccustomtext4"), ""), sptField,
                     FxDB(drutama("rgccustomtext5"), ""), sptField,
                     FxDB(drutama("rgccustomint1"), 0), sptField,
                     FxDB(drutama("rgccustomint2"), 0), sptField,
                     FxDB(drutama("rgccustomint3"), 0), sptField,
                     FxDB(drutama("rgccustomdbl1"), 0), sptField,
                     FxDB(drutama("rgccustomdbl2"), 0), sptField,
                     FxDB(drutama("rgccustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rgccustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rgccustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rgccustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("rgccabangnama"), ""), sptField,
                     FxDB(drutama("rgclokasinama"), ""), sptField,
                     FxDB(drutama("rgcjenisnama"), ""), sptField,
                     FxDB(drutama("rgckontakkode"), ""), sptField,
                     FxDB(drutama("rgckontaknama"), ""), sptField,
                     FxDB(drutama("rgcnotransaksirg"), ""), sptField,
                     FxDB(drutama("rgcstatusnama"), ""), sptField,
                     FxDB(drutama("rgcstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("rgcinputusernama"), ""), sptField,
                     FxDB(drutama("rgcmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idrgcdetail"), 0), sptField,
                     FxDB(dr("idrgc"), 0), sptField,
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
                     FxDB(dr("idrgdetail"), 0), sptField,
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
                     FxDB(dr("rgnotransaksi"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rgcid, rgccabang, rgclokasi, rgcsumber, rgcjenis, rgcautonotransaksi, rgcnotransaksi, rgctgl, rgckodepa, rgckontak, rgckontakperson, rgcuraian, rgccatatan, rgcmatauang, rgckurs, rgcjumlah, rgcjumlahvalas, rgcidrg, rgcstatus, rgcstatussebelumnya, rgcjmlrevisi, rgccetakanke, rgcisclose, rgcinputuser, rgcinputtgl, rgcmodifikasiuser, rgcmodifikasitgl, rgcposting, rgcpostingtgl, rgccustomtext1, rgccustomtext2, rgccustomtext3, rgccustomtext4, rgccustomtext5, rgccustomint1, rgccustomint2, rgccustomint3, rgccustomdbl1, rgccustomdbl2, rgccustomdbl3, rgccustomdate1, rgccustomdate2, rgccustomdate3, rgccabangnama, rgclokasinama, rgcjenisnama, rgckontakkode, rgckontaknama, rgcnotransaksirg, rgcstatusnama, rgcstatussebelumnyanama, rgcinputusernama, rgcmodifikasiusernama" & sptSubParam & "idrgcdetail, idrgc, nogiro, kontak, matauang, kurs, jumlah, jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, urutan, statusgiro, idrgdetail, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kontakkode, kontaknama, banknama, rekbanknama, rekgironama, statusgironama, rgnotransaksi"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_RgcSearch(ByVal param As String) As String
        'M2_RgcSearch --------------------------------------------------------
        'rgcid, rgccabang, rgclokasi, rgcsumber, rgcjenis, rgcautonotransaksi, rgcnotransaksi, 
        'rgctgl, rgckodepa, rgckontak, rgckontakperson, rgcuraian, rgccatatan, rgcmatauang, 
        'rgckurs, rgcjumlah, rgcjumlahvalas, rgcidrg, rgcstatus, rgcstatussebelumnya, rgcjmlrevisi, 
        'rgccetakanke, rgcisclose, rgcinputuser, rgcinputtgl, rgcmodifikasiuser, rgcmodifikasitgl, rgcposting, 
        'rgcpostingtgl, rgccabangnama, rgclokasinama, rgcjenisnama, rgckontakkode, rgckontaknama, rgcnotransaksirg, 
        'rgcstatusnama, rgcstatussebelumnyanama, rgcinputusernama, rgcmodifikasiusernama

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
        sql = query.PanggilQuery("m2_rgc_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Rgc", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rgcid"), 0), sptField,
                     FxDB(dr("rgccabang"), ""), sptField,
                     FxDB(dr("rgclokasi"), ""), sptField,
                     FxDB(dr("rgcsumber"), ""), sptField,
                     FxDB(dr("rgcjenis"), 0), sptField,
                     FxDB(dr("rgcautonotransaksi"), 0), sptField,
                     FxDB(dr("rgcnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rgctgl"), ""), formatTgl), sptField,
                     FxDB(dr("rgckodepa"), 0), sptField,
                     FxDB(dr("rgckontak"), 0), sptField,
                     FxDB(dr("rgckontakperson"), ""), sptField,
                     FxDB(dr("rgcuraian"), ""), sptField,
                     FxDB(dr("rgccatatan"), ""), sptField,
                     FxDB(dr("rgcmatauang"), ""), sptField,
                     FxDB(dr("rgckurs"), 0), sptField,
                     FxDB(dr("rgcjumlah"), 0), sptField,
                     FxDB(dr("rgcjumlahvalas"), 0), sptField,
                     FxDB(dr("rgcidrg"), 0), sptField,
                     FxDB(dr("rgcstatus"), 0), sptField,
                     FxDB(dr("rgcstatussebelumnya"), 0), sptField,
                     FxDB(dr("rgcjmlrevisi"), 0), sptField,
                     FxDB(dr("rgccetakanke"), 0), sptField,
                     FxDB(dr("rgcisclose"), 0), sptField,
                     FxDB(dr("rgcinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rgcinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rgcmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rgcmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rgcposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rgcpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rgccabangnama"), ""), sptField,
                     FxDB(dr("rgclokasinama"), ""), sptField,
                     FxDB(dr("rgcjenisnama"), ""), sptField,
                     FxDB(dr("rgckontakkode"), ""), sptField,
                     FxDB(dr("rgckontaknama"), ""), sptField,
                     FxDB(dr("rgcnotransaksirg"), ""), sptField,
                     FxDB(dr("rgcstatusnama"), ""), sptField,
                     FxDB(dr("rgcstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("rgcinputusernama"), ""), sptField,
                     FxDB(dr("rgcmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rgcid, rgccabang, rgclokasi, rgcsumber, rgcjenis, rgcautonotransaksi, rgcnotransaksi, rgctgl, rgckodepa, rgckontak, rgckontakperson, rgcuraian, rgccatatan, rgcmatauang, rgckurs, rgcjumlah, rgcjumlahvalas, rgcidrg, rgcstatus, rgcstatussebelumnya, rgcjmlrevisi, rgccetakanke, rgcisclose, rgcinputuser, rgcinputtgl, rgcmodifikasiuser, rgcmodifikasitgl, rgcposting, rgcpostingtgl, rgccabangnama, rgclokasinama, rgcjenisnama, rgckontakkode, rgckontaknama, rgcnotransaksirg, rgcstatusnama, rgcstatussebelumnyanama, rgcinputusernama, rgcmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_RgcTerkait(ByVal param As String) As String
        'M2_RgcTerkait --------------------------------------------------------
        'rgcid, rgcnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
        sql = query.PanggilQuery("m2_rgc_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rgcid"), 0), sptField,
                     FxDB(dr("rgcnotransaksi"), ""), sptField,
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
            result(2) = "Related RGC data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rgcid, rgcnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

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
                        sql = "SELECT glnogiro, rgnotransaksi FROM m2_giro_list JOIN m2_rg_detail ON glnogiro = nogiro JOIN m2_rg ON idrg = rgid WHERE (rgstatus = 2 OR rgstatus = 3 OR rgstatus = 4 OR rgstatus = 7) AND (glnogiro = '" & FixQuotes(dtvalidasi.Rows(0)("glnogiro")) & "') LIMIT 1"
                        dtvalidasi = AsDataTableAmbilDariDB(sql)
                        If (dtvalidasi.Rows.Count > 0) Then
                            hasil = "Giro : " & dtvalidasi.Rows(0)(0) & " - has disbursed in transaction : " & dtvalidasi.Rows(0)(1) : GoTo selesai
                        End If
                    Case Else
                        sql = "SELECT glnogiro, rgcnotransaksi FROM m2_giro_list JOIN m2_rgc_detail ON glnogiro = nogiro JOIN m2_rgc ON idrgc = rgcid WHERE (rgcstatus = 2 OR rgcstatus = 3 OR rgcstatus = 4 OR rgcstatus = 7) AND (glnogiro = '" & FixQuotes(dtvalidasi.Rows(0)("glnogiro")) & "') LIMIT 1"
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
    Public Function M2_RgcSimpanOld(ByVal param As String) As String
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
        'rgcid(0) As Integer, rgccabang(1) As String, rgclokasi(2) As String, rgcsumber(3) As String, rgcjenis(4) As Integer, 
        'rgcautonotransaksi(5) As Integer, rgcnotransaksi(6) As String, rgctgl(7) As Date, rgckodepa(8) As Integer, rgckontak(9) As Integer, 
        'rgckontakperson(10) As String, rgcuraian(11) As String, rgccatatan(12) As String, rgcmatauang(13) As String, rgckurs(14) As Double, 
        'rgcjumlah(15) As Double, rgcjumlahvalas(16) As Double, rgcidrg(17) As Integer, rgcstatus(18) As Integer, rgcstatussebelumnya(19) As Integer, 
        'rgcjmlrevisi(20) As Integer, rgccetakanke(21) As Integer, rgcisclose(22) As Integer, rgcinputuser(23) As Integer, rgcinputtgl(24) As DateTime, 
        'rgcmodifikasiuser(25) As Integer, rgcmodifikasitgl(26) As DateTime, rgcposting(27) As Integer, rgccustomtext1(28) As String, rgccustomtext2(29) As String, 
        'rgccustomtext3(30) As String, rgccustomtext4(31) As String, rgccustomtext5(32) As String, rgccustomint1(33) As Integer, rgccustomint2(34) As Integer, 
        'rgccustomint3(35) As Integer, rgccustomdbl1(36) As Double, rgccustomdbl2(37) As Double, rgccustomdbl3(38) As Double, rgccustomdate1(39) As Date, 
        'rgccustomdate2(40) As Date, rgccustomdate3(41) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'rgcid, rgccabang, rgclokasi, rgcsumber, rgcjenis, rgcautonotransaksi, rgcnotransaksi, 
        'rgctgl, rgckodepa, rgckontak, rgckontakperson, rgcuraian, rgccatatan, rgcmatauang, 
        'rgckurs, rgcjumlah, rgcjumlahvalas, rgcidrg, rgcstatus, rgcstatussebelumnya, rgcjmlrevisi, 
        'rgccetakanke, rgcisclose, rgcinputuser, rgcinputtgl, rgcmodifikasiuser, rgcmodifikasitgl, rgcposting, 
        'rgccustomtext1, rgccustomtext2, rgccustomtext3, rgccustomtext4, rgccustomtext5, rgccustomint1, rgccustomint2, 
        'rgccustomint3, rgccustomdbl1, rgccustomdbl2, rgccustomdbl3, rgccustomdate1, rgccustomdate2, rgccustomdate3


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 42) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'rgcid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "rgcid required numeric." : GoTo selesai
        End If
        'rgcjenis(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "rgcjenis required numeric." : GoTo selesai
        End If
        'rgcautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "rgcautonotransaksi required numeric." : GoTo selesai
        End If
        'rgctgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "rgctgl required date." : GoTo selesai
        End If
        'rgckodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "rgckodepa required numeric." : GoTo selesai
        End If
        'rgckontak(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "rgckontak required numeric." : GoTo selesai
        End If
        If (dataUtama(9) < 1) Then
            result(2) = "rgckontak can't be empty." : GoTo selesai
        End If
        'rgckurs(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "rgckurs required numeric." : GoTo selesai
        End If
        'rgcjumlah(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "rgcjumlah required numeric." : GoTo selesai
        End If
        'rgcjumlahvalas(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "rgcjumlahvalas required numeric." : GoTo selesai
        End If
        'rgcidrg(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "rgcidrg required numeric." : GoTo selesai
        End If
        'rgcstatus(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "rgcstatus required numeric." : GoTo selesai
        End If
        'rgcstatussebelumnya(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "rgcstatussebelumnya required numeric." : GoTo selesai
        End If
        'rgcjmlrevisi(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "rgcjmlrevisi required numeric." : GoTo selesai
        End If
        'rgccetakanke(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "rgccetakanke required numeric." : GoTo selesai
        End If
        'rgcisclose(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "rgcisclose required numeric." : GoTo selesai
        End If
        'rgcinputuser(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "rgcinputuser required numeric." : GoTo selesai
        End If
        'rgcinputtgl(24) As DateTime
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "rgcinputtgl required date." : GoTo selesai
        End If
        'rgcmodifikasiuser(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "rgcmodifikasiuser required numeric." : GoTo selesai
        End If
        'rgcmodifikasitgl(26) As DateTime
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "rgcmodifikasitgl required date." : GoTo selesai
        End If
        'rgcposting(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "rgcposting required numeric." : GoTo selesai
        End If
        'rgccustomint1(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "rgccustomint1 required numeric." : GoTo selesai
        End If
        'rgccustomint2(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "rgccustomint2 required numeric." : GoTo selesai
        End If
        'rgccustomint3(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "rgccustomint3 required numeric." : GoTo selesai
        End If
        'rgccustomdbl1(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "rgccustomdbl1 required numeric." : GoTo selesai
        End If
        'rgccustomdbl2(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "rgccustomdbl2 required numeric." : GoTo selesai
        End If
        'rgccustomdbl3(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "rgccustomdbl3 required numeric." : GoTo selesai
        End If
        'rgccustomdate1(39) As Date
        If (IsDate(dataUtama(39)) = False) Then
            result(2) = "rgccustomdate1 required date." : GoTo selesai
        End If
        'rgccustomdate2(40) As Date
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "rgccustomdate2 required date." : GoTo selesai
        End If
        'rgccustomdate3(41) As Date
        If (IsDate(dataUtama(41)) = False) Then
            result(2) = "rgccustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'rgccabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "rgccabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "rgccabang should not be more than 25 character." : GoTo selesai
        End If

        'rgclokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "rgclokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "rgclokasi should not be more than 25 character." : GoTo selesai
        End If

        'rgcsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "rgcsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "rgcsumber should not be more than 10 character." : GoTo selesai
        End If

        'rgcnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "rgcnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "rgcnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'rgctgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "rgctgl can't be empty" : GoTo selesai
        End If

        'rgcmatauang(13) As String
        If Len(dataUtama(13)) = 0 Then
            result(2) = "rgcmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(13)) > 25 Then
            result(2) = "rgcmatauang should not be more than 25 character." : GoTo selesai
        End If

        'rgckurs(14) As Double
        If Len(dataUtama(14)) = 0 Then
            result(2) = "rgckurs can't be empty" : GoTo selesai
        End If

        'rgcjumlah(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "rgcjumlah can't be empty" : GoTo selesai
        End If

        'rgcjumlahvalas(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "rgcjumlahvalas can't be empty" : GoTo selesai
        End If

        'rgcinputtgl(24) As DateTime
        If Len(dataUtama(24)) = 0 Then
            result(2) = "rgcinputtgl can't be empty" : GoTo selesai
        End If

        'rgcmodifikasitgl(26) As DateTime
        If Len(dataUtama(26)) = 0 Then
            result(2) = "rgcmodifikasitgl can't be empty" : GoTo selesai
        End If

        'rgccustomdbl1(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "rgccustomdbl1 can't be empty" : GoTo selesai
        End If

        'rgccustomdbl2(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "rgccustomdbl2 can't be empty" : GoTo selesai
        End If

        'rgccustomdbl3(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "rgccustomdbl3 can't be empty" : GoTo selesai
        End If

        'rgccustomdate1(39) As Date
        If Len(dataUtama(39)) = 0 Then
            result(2) = "rgccustomdate1 can't be empty" : GoTo selesai
        End If

        'rgccustomdate2(40) As Date
        If Len(dataUtama(40)) = 0 Then
            result(2) = "rgccustomdate2 can't be empty" : GoTo selesai
        End If

        'rgccustomdate3(41) As Date
        If Len(dataUtama(41)) = 0 Then
            result(2) = "rgccustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "rgcid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgccabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgclokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcjenis", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgctgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgckodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgckontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgckontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgccatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgckurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcjumlah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcjumlahvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcidrg", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgccetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgccustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgccustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgccustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgccustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgccustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgccustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgccustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgccustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgccustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgccustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgccustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgccustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgccustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgccustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "rgcid~rgccabang~rgclokasi~rgcsumber~rgcjenis~rgcautonotransaksi~rgcnotransaksi~rgctgl~rgckodepa~rgckontak~rgckontakperson~rgcuraian~rgccatatan~rgcmatauang~rgckurs~rgcjumlah~rgcjumlahvalas~rgcidrg~rgcstatus~rgcstatussebelumnya~rgcjmlrevisi~rgccetakanke~rgcisclose~rgcinputuser~rgcinputtgl~rgcmodifikasiuser~rgcmodifikasitgl~rgcposting~rgccustomtext1~rgccustomtext2~rgccustomtext3~rgccustomtext4~rgccustomtext5~rgccustomint1~rgccustomint2~rgccustomint3~rgccustomdbl1~rgccustomdbl2~rgccustomdbl3~rgccustomdate1~rgccustomdate2~rgccustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idrgcdetail(0) As Integer, idrgc(1) As Integer, nogiro(2) As String, kontak(3) As Integer, matauang(4) As String, 
        'kurs(5) As Double, jumlah(6) As Double, jumlahvalas(7) As Double, bank(8) As String, noacbank(9) As String, 
        'rekbank(10) As String, rekgiro(11) As String, tgljatuhtempo(12) As Date, catatan(13) As String, urutan(14) As Integer, 
        'statusgiro(15) As Integer, idrgdetail(16) As Integer, isclose(17) As Integer, customtext1(18) As String, customtext2(19) As String, 
        'customtext3(20) As String, customdbl1(21) As Double, customdbl2(22) As Double, customdbl3(23) As Double, customdate1(24) As Date, 
        'customdate2(25) As Date, customdate3(26) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idrgcdetail, idrgc, nogiro, kontak, matauang, kurs, jumlah, 
        'jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, 
        'urutan, statusgiro, idrgdetail, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idrgcdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idrgc", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "idrgdetail", AsEnumTypeData.AsInt64)
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
            'idrgcdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idrgcdetail required numeric." : GoTo selesai
            End If
            'idrgc(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idrgc required numeric." : GoTo selesai
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
            'idrgdetail(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - idrgdetail required numeric." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idrgcdetail~idrgc~nogiro~kontak~matauang~kurs~jumlah~jumlahvalas~bank~noacbank~rekbank~rekgiro~tgljatuhtempo~catatan~urutan~statusgiro~idrgdetail~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26)) = False Then
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("rgctgl")), AsFormatTanggal(drutama("rgctgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================

                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("rgcstatus") = 2 Then
                    Dim rsValidasi As String = ValidasiSimpan(ftExistGiro, ftGiro)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================

                'HITUNG TOTAL BERDASARKAN DATA DETAIL ===================
                drutama("rgcjumlah") = AsDataTableDSum(dtdetail, "jumlah")
                drutama("rgcjumlahvalas") = AsDataTableDSum(dtdetail, "jumlahvalas")
                'END OF HITUNG TOTAL BERDASARKAN DATA DETAIL ============

                If isUpdate Then
                    result(4) = drutama("rgcid")
                    notransaksi = drutama("rgcnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(rgcid), rgcnotransaksi FROM M2_rgc WHERE rgcid='" & result(4) & "' AND rgcstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(rgcid) FROM m2_rgc WHERE rgcnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m2_rgc_history
                        Dim rsSimpanHistory As String = SimpanHistory.M2_Rgc_HistorySimpan("" & paramSplit(0) & "★M2_Rgc_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("rgcsumber")) & "▼" & FixQuotes(drutama("rgcid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M2_Rgc set rgccabang  = '" & FixQuotes(drutama("rgccabang")) & "', rgclokasi  = '" & FixQuotes(drutama("rgclokasi")) & "', rgcsumber  = '" & FixQuotes(drutama("rgcsumber")) & "', rgcjenis  = " & drutama("rgcjenis") & ", rgcautonotransaksi  = " & drutama("rgcautonotransaksi") & ", rgcnotransaksi  = '" & notransaksi & "', rgctgl  = '" & FixQuotes(AsFormatTanggal(drutama("rgctgl"))) & "', rgckodepa  = " & drutama("rgckodepa") & ", rgckontak  = " & drutama("rgckontak") & ", rgckontakperson  = '" & FixQuotes(drutama("rgckontakperson")) & "', rgcuraian  = '" & FixQuotes(drutama("rgcuraian")) & "', rgccatatan  = '" & FixQuotes(drutama("rgccatatan")) & "', rgcmatauang  = '" & FixQuotes(drutama("rgcmatauang")) & "', rgckurs  = '" & FixDouble(drutama("rgckurs")) & "', rgcjumlah  = '" & FixDouble(drutama("rgcjumlah")) & "', rgcjumlahvalas  = '" & FixDouble(drutama("rgcjumlahvalas")) & "', rgcidrg  = " & drutama("rgcidrg") & ", rgcstatus  = " & drutama("rgcstatus") & ", rgcstatussebelumnya  = " & drutama("rgcstatussebelumnya") & ", rgcjmlrevisi  = rgcjmlrevisi+1, rgccetakanke  = " & drutama("rgccetakanke") & ", rgcisclose  = " & drutama("rgcisclose") & ", rgcmodifikasiuser  = " & drutama("rgcmodifikasiuser") & ", rgcmodifikasitgl  = NOW(), rgcposting  = 0, rgccustomtext1  = '" & FixQuotes(drutama("rgccustomtext1")) & "', rgccustomtext2  = '" & FixQuotes(drutama("rgccustomtext2")) & "', rgccustomtext3  = '" & FixQuotes(drutama("rgccustomtext3")) & "', rgccustomtext4  = '" & FixQuotes(drutama("rgccustomtext4")) & "', rgccustomtext5  = '" & FixQuotes(drutama("rgccustomtext5")) & "', rgccustomint1  = " & drutama("rgccustomint1") & ", rgccustomint2  = " & drutama("rgccustomint2") & ", rgccustomint3  = " & drutama("rgccustomint3") & ", rgccustomdbl1  = '" & FixDouble(drutama("rgccustomdbl1")) & "', rgccustomdbl2  = '" & FixDouble(drutama("rgccustomdbl2")) & "', rgccustomdbl3  = '" & FixDouble(drutama("rgccustomdbl3")) & "', rgccustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("rgccustomdate1"))) & "', rgccustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("rgccustomdate2"))) & "', rgccustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("rgccustomdate3"))) & "' where rgcid = '" & drutama("rgcid") & "'"
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

                    If drutama("rgcautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rgccabang"), drutama("rgclokasi"), drutama("rgcsumber"), drutama("rgctgl"))
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
                        notransaksi = drutama("rgcnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(rgcid) FROM m2_rgc WHERE rgcnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M2_Rgc (rgccabang, rgclokasi, rgcsumber, rgcjenis, rgcautonotransaksi, rgcnotransaksi, rgctgl, rgckodepa, rgckontak, rgckontakperson, rgcuraian, rgccatatan, rgcmatauang, rgckurs, rgcjumlah, rgcjumlahvalas, rgcidrg, rgcstatus, rgcstatussebelumnya, rgcjmlrevisi, rgccetakanke, rgcisclose, rgcinputuser, rgcinputtgl, rgcmodifikasiuser, rgcmodifikasitgl, rgcposting, rgccustomtext1, rgccustomtext2, rgccustomtext3, rgccustomtext4, rgccustomtext5, rgccustomint1, rgccustomint2, rgccustomint3, rgccustomdbl1, rgccustomdbl2, rgccustomdbl3, rgccustomdate1, rgccustomdate2, rgccustomdate3) values('" & FixQuotes(drutama("rgccabang")) & "', '" & FixQuotes(drutama("rgclokasi")) & "', '" & FixQuotes(drutama("rgcsumber")) & "', " & drutama("rgcjenis") & ", " & drutama("rgcautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("rgctgl"))) & "', " & drutama("rgckodepa") & ", " & drutama("rgckontak") & ", '" & FixQuotes(drutama("rgckontakperson")) & "', '" & FixQuotes(drutama("rgcuraian")) & "', '" & FixQuotes(drutama("rgccatatan")) & "', '" & FixQuotes(drutama("rgcmatauang")) & "', '" & FixDouble(drutama("rgckurs")) & "', '" & FixDouble(drutama("rgcjumlah")) & "', '" & FixDouble(drutama("rgcjumlahvalas")) & "', " & drutama("rgcidrg") & ", " & drutama("rgcstatus") & ", " & drutama("rgcstatussebelumnya") & ", " & drutama("rgcjmlrevisi") & ", " & drutama("rgccetakanke") & ", " & drutama("rgcisclose") & ", " & drutama("rgcinputuser") & ", NOW(), " & drutama("rgcmodifikasiuser") & ", '1971-01-01 00:00:00', 0, '" & FixQuotes(drutama("rgccustomtext1")) & "', '" & FixQuotes(drutama("rgccustomtext2")) & "', '" & FixQuotes(drutama("rgccustomtext3")) & "', '" & FixQuotes(drutama("rgccustomtext4")) & "', '" & FixQuotes(drutama("rgccustomtext5")) & "', " & drutama("rgccustomint1") & ", " & drutama("rgccustomint2") & ", " & drutama("rgccustomint3") & ", '" & FixDouble(drutama("rgccustomdbl1")) & "', '" & FixDouble(drutama("rgccustomdbl2")) & "', '" & FixDouble(drutama("rgccustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rgccustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rgccustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rgccustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select rgcid from M2_rgc where rgcnotransaksi='" & notransaksi & "' AND rgcinputuser= '" & userid & "' order by rgcmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M2_Rgc_Detail where idrgc = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idrgcdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("nogiro")) & "', " & dr1("kontak") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljatuhtempo"))) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("statusgiro") & ", " & dr1("idrgdetail") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                        If drutama("rgcstatus") = 2 Then
                            'filter
                            strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", " OR "))
                            strGiro.Append("(glnogiro = '" & FixQuotes(dr1("nogiro")) & "')")
                            'rekgiro
                            strRekgiro.Append(" WHEN '" & FixQuotes(dr1("nogiro")) & "' THEN '" & FixQuotes(dr1("rekgiro")) & "' ")
                        End If
                    Next
                    sql = "Insert into M2_Rgc_Detail(idrgcdetail, idrgc, nogiro, kontak, matauang, kurs, jumlah, jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, urutan, statusgiro, idrgdetail, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'update glstatus, gltglcair, glrekgiro m2_giro_list
                    If drutama("rgcstatus") = 2 Then
                        'cek status giro
                        Dim dtValidasi As DataTable = AsDataTableAmbilDariDB("SELECT glnogiro FROM m2_giro_list WHERE glstatus = 1 AND (" & strGiro.ToString & ")")
                        If dtValidasi.Rows.Count > 0 Then result(2) = "Can't update giro '" & dtValidasi.Rows(0)(0) & "'. It has related transactions." : Trans.Rollback() : GoTo selesai

                        'update giro                   glstatus                                , gltglcair                              , glrekgiro                                                                  filter
                        sql = "UPDATE m2_giro_list SET glstatus = '" & drutama("rgcjenis") & "', gltglcair = '" & drutama("rgctgl") & "', glrekgiro = (CASE glnogiro " & strRekgiro.ToString & " ELSE glrekgiro END) WHERE " & strGiro.ToString & ""
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
                Dim sumber As String = "RGC", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("rgcstatus") = 2 Then
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
    Public Function M2_RgcUpdateStatusOld(ByVal param As String) As String

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
            Dim sumber As String = "Rgc", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Rgctgl, Rgcnotransaksi, Rgcstatus FROM m2_Rgc WHERE Rgcid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Rgcstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m2_rgc_history
            Dim rsSimpanHistory As String = SimpanHistory.M2_Rgc_HistorySimpan("" & paramSplit(0) & "★M2_Rgc_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m2_rgc_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                'PROSES GIRO ====================================================================
                Dim strGiro As New StringBuilder
                'ambil giro dari detail
                dtdetail = AsDataTableAmbilDariDB("SELECT nogiro FROM m2_rgc_detail WHERE idrgc = '" & idtransaksi & "'")
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

                        'AMBIL REKENING GIRO MASUK DARI M0_SETTING
                        Dim dtrekgiro As DataTable = AsDataTableAmbilDariDB("SELECT snilai FROM m0_setting WHERE smodule=0 AND sgrup='akun' AND skode='GiroMasuk'")
                        Dim rekgiro As String = ""
                        If dtrekgiro.Rows.Count > 0 Then
                            rekgiro = dtrekgiro.Rows(0)(0).ToString
                        Else
                            result(2) = "Setting Giro In CoA not found." : Trans.Rollback() : GoTo selesai
                        End If

                        'UPDATE STATUS GIRO MENJADI BELUM CAIR (0) DAN REKGIRO = SETTING PIUTANG GIRO MASUK
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
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'RGC' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M2_Rgc SET Rgcstatus = " & nilaiStatus & ", Rgcmodifikasiuser='" & userid & "', Rgcmodifikasitgl = NOW(), Rgcposting = 0, Rgcpostingtgl = '1971-01-01 00:00:00', Rgcjmlrevisi = Rgcjmlrevisi + 1 WHERE Rgcid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_RgcSearch(PostWsSearch(paramSplit(0), "M2_RgcSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_RgcDeleteOld(ByVal param As String) As String

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
            Dim sumber As String = "Rgc", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Rgcid, Rgcnotransaksi FROM m2_Rgc WHERE Rgcid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT rgccabang, rgclokasi, rgcsumber, rgcautonotransaksi, rgcnotransaksi, rgctgl"
            sql &= " FROM M2_rgc"
            sql &= " WHERE rgcid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("rgccabang")
                lokasi = dtNomorNext.Rows(0)("rgclokasi")
                sumber = dtNomorNext.Rows(0)("rgcsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("rgcautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("rgcnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("rgctgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE JURNAL
            sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'RGC' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M2_Rgc_Detail WHERE idRgc = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M2_Rgc WHERE Rgcid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_RgcSearch(PostWsSearch(paramSplit(0), "M2_RgcSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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