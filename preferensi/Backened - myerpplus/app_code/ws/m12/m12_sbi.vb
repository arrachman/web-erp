Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_sbi
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_SbiSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataSubstitution(), dataRowSubstitution() As String

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
        If (dataSplit.Length <> 3) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'sbiid(0) As Integer, sbicabang(1) As String, sbilokasi(2) As String, sbisumber(3) As String, sbikategoripos(4) As String, 
        'sbiautonotransaksi(5) As Integer, sbinotransaksi(6) As String, sbitgl(7) As Date, sbikodepa(8) As , sbikontak(9) As , 
        'sbikontakperson(10) As String, sbiuraian(11) As String, sbicatatan(12) As String, sbistatus(13) As Integer, sbistatussebelumnya(14) As Integer, 
        'sbijmlrevisi(15) As Integer, sbicetakanke(16) As Integer, sbiisclose(17) As Integer, sbiinputuser(18) As , sbiinputtgl(19) As DateTime, 
        'sbimodifikasiuser(20) As , sbimodifikasitgl(21) As DateTime, sbiposting(22) As Integer, sbipostingtgl(23) As DateTime, sbicustomtext1(24) As String, 
        'sbicustomtext2(25) As String, sbicustomtext3(26) As String, sbicustomtext4(27) As String, sbicustomtext5(28) As String, sbicustomint1(29) As Integer, 
        'sbicustomint2(30) As Integer, sbicustomint3(31) As Integer, sbicustomdbl1(32) As Double, sbicustomdbl2(33) As Double, sbicustomdbl3(34) As Double, 
        'sbicustomdate1(35) As Date, sbicustomdate2(36) As Date, sbicustomdate3(37) As Date, sbijeniskategori(38) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'sbiid, sbicabang, sbilokasi, sbisumber, sbikategoripos, sbiautonotransaksi, sbinotransaksi, 
        'sbitgl, sbikodepa, sbikontak, sbikontakperson, sbiuraian, sbicatatan, sbistatus, 
        'sbistatussebelumnya, sbijmlrevisi, sbicetakanke, sbiisclose, sbiinputuser, sbiinputtgl, sbimodifikasiuser, 
        'sbimodifikasitgl, sbiposting, sbipostingtgl, sbicustomtext1, sbicustomtext2, sbicustomtext3, sbicustomtext4, 
        'sbicustomtext5, sbicustomint1, sbicustomint2, sbicustomint3, sbicustomdbl1, sbicustomdbl2, sbicustomdbl3, 
        'sbicustomdate1, sbicustomdate2, sbicustomdate3, sbijeniskategori

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 39) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'sbiid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "sbiid required numeric." : GoTo selesai
        End If
        'sbiautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "sbiautonotransaksi required numeric." : GoTo selesai
        End If
        'sbitgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "sbitgl required date." : GoTo selesai
        End If
        'sbistatus(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "sbistatus required numeric." : GoTo selesai
        End If
        'sbistatussebelumnya(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "sbistatussebelumnya required numeric." : GoTo selesai
        End If
        'sbijmlrevisi(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "sbijmlrevisi required numeric." : GoTo selesai
        End If
        'sbicetakanke(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "sbicetakanke required numeric." : GoTo selesai
        End If
        'sbiisclose(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "sbiisclose required numeric." : GoTo selesai
        End If
        'sbiinputtgl(19) As DateTime
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "sbiinputtgl required date." : GoTo selesai
        End If
        'sbimodifikasitgl(21) As DateTime
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "sbimodifikasitgl required date." : GoTo selesai
        End If
        'sbiposting(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "sbiposting required numeric." : GoTo selesai
        End If
        'sbipostingtgl(23) As DateTime
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "sbipostingtgl required date." : GoTo selesai
        End If
        'sbicustomint1(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "sbicustomint1 required numeric." : GoTo selesai
        End If
        'sbicustomint2(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "sbicustomint2 required numeric." : GoTo selesai
        End If
        'sbicustomint3(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "sbicustomint3 required numeric." : GoTo selesai
        End If
        'sbicustomdbl1(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "sbicustomdbl1 required numeric." : GoTo selesai
        End If
        'sbicustomdbl2(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "sbicustomdbl2 required numeric." : GoTo selesai
        End If
        'sbicustomdbl3(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "sbicustomdbl3 required numeric." : GoTo selesai
        End If
        'sbicustomdate1(35) As Date
        If (IsDate(dataUtama(35)) = False) Then
            result(2) = "sbicustomdate1 required date." : GoTo selesai
        End If
        'sbicustomdate2(36) As Date
        If (IsDate(dataUtama(36)) = False) Then
            result(2) = "sbicustomdate2 required date." : GoTo selesai
        End If
        'sbicustomdate3(37) As Date
        If (IsDate(dataUtama(37)) = False) Then
            result(2) = "sbicustomdate3 required date." : GoTo selesai
        End If

        'sbijeniskategori(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "sbijeniskategori required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'sbicabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "sbicabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "sbicabang should not be more than 25 character." : GoTo selesai
        End If

        'sbilokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "sbilokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "sbilokasi should not be more than 25 character." : GoTo selesai
        End If

        'sbisumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "sbisumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "sbisumber should not be more than 10 character." : GoTo selesai
        End If

        'sbikategoripos(4) As String
        'If Len(dataUtama(4)) = 0 Then
        '    result(2) = "sbikategoripos can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(4)) > 50 Then
            result(2) = "sbikategoripos should not be more than 50 character." : GoTo selesai
        End If

        'sbinotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "sbinotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "sbinotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'sbitgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "sbitgl can't be empty" : GoTo selesai
        End If

        'sbikodepa(8) As 
        If Len(dataUtama(8)) = 0 Then
            result(2) = "sbikodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 20 Then
            result(2) = "sbikodepa should not be more than 20 character." : GoTo selesai
        End If

        'sbikontak(9) As 
        If Len(dataUtama(9)) = 0 Then
            result(2) = "sbikontak can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 20 Then
            result(2) = "sbikontak should not be more than 20 character." : GoTo selesai
        End If

        'sbiinputtgl(19) As DateTime
        If Len(dataUtama(19)) = 0 Then
            result(2) = "sbiinputtgl can't be empty" : GoTo selesai
        End If

        'sbimodifikasitgl(21) As DateTime
        If Len(dataUtama(21)) = 0 Then
            result(2) = "sbimodifikasitgl can't be empty" : GoTo selesai
        End If

        'sbipostingtgl(23) As DateTime
        If Len(dataUtama(23)) = 0 Then
            result(2) = "sbipostingtgl can't be empty" : GoTo selesai
        End If

        'sbicustomdbl1(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "sbicustomdbl1 can't be empty" : GoTo selesai
        End If

        'sbicustomdbl2(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "sbicustomdbl2 can't be empty" : GoTo selesai
        End If

        'sbicustomdbl3(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "sbicustomdbl3 can't be empty" : GoTo selesai
        End If

        'sbicustomdate1(35) As Date
        If Len(dataUtama(35)) = 0 Then
            result(2) = "sbicustomdate1 can't be empty" : GoTo selesai
        End If

        'sbicustomdate2(36) As Date
        If Len(dataUtama(36)) = 0 Then
            result(2) = "sbicustomdate2 can't be empty" : GoTo selesai
        End If

        'sbicustomdate3(37) As Date
        If Len(dataUtama(37)) = 0 Then
            result(2) = "sbicustomdate3 can't be empty" : GoTo selesai
        End If



        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "sbiid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbilokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbisumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbikategoripos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbiautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbinotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbikodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbikontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbikontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbiuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbistatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbistatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbijmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbicetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbiisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbiinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbiinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbimodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbimodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbiposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbipostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbijeniskategori", AsEnumTypeData.AsInt64)

        If AsDataTableTambahData(dtutama, "sbiid~sbicabang~sbilokasi~sbisumber~sbikategoripos~sbiautonotransaksi~sbinotransaksi~sbitgl~sbikodepa~sbikontak~sbikontakperson~sbiuraian~sbicatatan~sbistatus~sbistatussebelumnya~sbijmlrevisi~sbicetakanke~sbiisclose~sbiinputuser~sbiinputtgl~sbimodifikasiuser~sbimodifikasitgl~sbiposting~sbipostingtgl~sbicustomtext1~sbicustomtext2~sbicustomtext3~sbicustomtext4~sbicustomtext5~sbicustomint1~sbicustomint2~sbicustomint3~sbicustomdbl1~sbicustomdbl2~sbicustomdbl3~sbicustomdate1~sbicustomdate2~sbicustomdate3~sbijeniskategori", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38)) = False Then
            result(2) = "insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idsbidetail(0) As , idsbi(1) As , sbikategori(2) As String, idbarang(3) As , operator(4) As String, 
        'jml1(5) As Double, jml2(6) As Double, customtext1(7) As String, customtext2(8) As String, customtext3(9) As String, 
        'customtext4(10) As String, customtext5(11) As String, customint1(12) As Integer, customint2(13) As Integer, customint3(14) As Integer, 
        'customdbl1(15) As Double, customdbl2(16) As Double, customdbl3(17) As Double, customdate1(18) As Date, customdate2(19) As Date, 
        'customdate3(20) As Date, tgl1(21) As Date, tgl2(22) As Date, nopromo(23) As String, nogrup (24) As String, catatan (25) As String, urutan(26) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idsbidetail, idsbi, sbikategori, idbarang, operator, jml1, jml2, 
        'customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, 
        'customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'tgl1, tgl2, nopromo, nogrup, catatan, urutan

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idsbidetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idsbi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sbikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "operator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nopromo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nogrup", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)

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
            'jml1(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "jml1 required numeric." : GoTo selesai
            End If
            'jml2(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "jml2 required numeric." : GoTo selesai
            End If
            'customint1(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "customint1 required numeric." : GoTo selesai
            End If
            'customint2(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "customint2 required numeric." : GoTo selesai
            End If
            'customint3(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "customint3 required numeric." : GoTo selesai
            End If
            'customdbl1(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(18) As Date
            If (IsDate(dataRowDetail(18)) = False) Then
                result(2) = "customdate1 required date." : GoTo selesai
            End If
            'customdate2(19) As Date
            If (IsDate(dataRowDetail(19)) = False) Then
                result(2) = "customdate2 required date." : GoTo selesai
            End If
            'customdate3(20) As Date
            If (IsDate(dataRowDetail(20)) = False) Then
                result(2) = "customdate3 required date." : GoTo selesai
            End If
            'tgl1(21) As Date
            If (IsDate(dataRowDetail(21)) = False) Then
                result(2) = "tgl1 required date." : GoTo selesai
            End If
            'tgl2(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "tgl2 required date." : GoTo selesai
            End If
            'urutan(26) As Double
            If (IsNumeric(dataRowDetail(26)) = False) Then
                result(2) = "urutan required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'idbidetail(0) As 
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idsbidetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idsbidetail should not be more than 20 character." : GoTo selesai
            End If

            'idbi(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idsbi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idsbi should not be more than 20 character." : GoTo selesai
            End If

            ''bikategori(2) As String
            'If Len(dataRowDetail(2)) = 0 Then
            '    result(2) = "Row : " & i & " - bikategori can't be empty" : GoTo selesai
            'End If
            'If Len(dataRowDetail(2)) > 25 Then
            '    result(2) = "Row : " & i & " - bikategori should not be more than 25 character." : GoTo selesai
            'End If

            'idbarang(3) As 
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - idbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 20 Then
                result(2) = "Row : " & i & " - idbarang should not be more than 20 character." : GoTo selesai
            End If

            'operator(4) As String
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - operator can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(4)) > 25 Then
                result(2) = "Row : " & i & " - operator should not be more than 25 character." : GoTo selesai
            End If

            'jml1(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - jml1 can't be empty" : GoTo selesai
            End If

            'jml2(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - jml2 can't be empty" : GoTo selesai
            End If

            'customdbl1(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(18) As Date
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(19) As Date
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(20) As Date
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idsbidetail~idsbi~sbikategori~idbarang~operator~jml1~jml2~customtext1~customtext2~customtext3~customtext4~customtext5~customint1~customint2~customint3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~tgl1~tgl2~nopromo~nogrup~catatan~urutan", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26)) = False Then
                result(2) = "Detail Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================

        'MAPPING BUAT WS DATA BONUS -------------------------------------------------------
        'idsubstitution(0) As , idsbi(1) As , idsbidetail(2) As , idbarang(3) As , jml(4) As Double, 
        'satuan(5) As String, customtext1(6) As String, customtext2(7) As String, customtext3(8) As String, customtext4(9) As String, 
        'customtext5(10) As String, customint1(11) As Integer, customint2(12) As Integer, customint3(13) As Integer, customdbl1(14) As Double, 
        'customdbl2(15) As Double, customdbl3(16) As Double, customdate1(17) As Date, customdate2(18) As Date, customdate3(19) As Date, nogrup(20) As String

        'MAPPING BUAT FLEX DATA BONUS -----------------------------------------------------
        'idsubstitution, idsbi, idsbidetail, idbarang, jml, satuan, customtext1, 
        'customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA BONUS ======================================================
        'SPLIT PARAMETER DATA BONUS
        dataSubstitution = dataSplit(2).Split(sptRow)
        'END OF VALIDASI DAN SET DATA BONUS ===============================================

        'Buat datatable substitution
        Dim dtsubstitution As New DataTable
        AsDataTableTambahField(dtsubstitution, "idsubstitution", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtsubstitution, "idsbi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtsubstitution, "idsbidetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtsubstitution, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtsubstitution, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "customint1", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtsubstitution, "customint2", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtsubstitution, "customint3", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtsubstitution, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "nogrup", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "urutan", AsEnumTypeData.AsInt64)

        'VALIDASI DAN SET DATA ROW BONUS ==================================================
        Dim JmlDtSubstitution As Integer = dataSubstitution.Length
        For i = 1 To JmlDtSubstitution
            'SPLIT DATA DETAIL
            dataRowSubstitution = dataSubstitution(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA BONUS -----------------------------------
            'CEK ARRAY DATA BONUS
            If (dataRowSubstitution.Length <> 22) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW BONUS ----------------------------

            'VALIDASI TIPE DATA BONUS ------------------------------------------
            'jml(4) As Double
            If (IsNumeric(dataRowSubstitution(4)) = False) Then
                result(2) = "jml required numeric." : GoTo selesai
            End If
            'customint1(11) As Integer
            If (IsNumeric(dataRowSubstitution(11)) = False) Then
                result(2) = "customint1 required numeric." : GoTo selesai
            End If
            'customint2(12) As Integer
            If (IsNumeric(dataRowSubstitution(12)) = False) Then
                result(2) = "customint2 required numeric." : GoTo selesai
            End If
            'customint3(13) As Integer
            If (IsNumeric(dataRowSubstitution(13)) = False) Then
                result(2) = "customint3 required numeric." : GoTo selesai
            End If
            'customdbl1(14) As Double
            If (IsNumeric(dataRowSubstitution(14)) = False) Then
                result(2) = "customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(15) As Double
            If (IsNumeric(dataRowSubstitution(15)) = False) Then
                result(2) = "customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(16) As Double
            If (IsNumeric(dataRowSubstitution(16)) = False) Then
                result(2) = "customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(17) As Date
            If (IsDate(dataRowSubstitution(17)) = False) Then
                result(2) = "customdate1 required date." : GoTo selesai
            End If
            'customdate2(18) As Date
            If (IsDate(dataRowSubstitution(18)) = False) Then
                result(2) = "customdate2 required date." : GoTo selesai
            End If
            'customdate3(19) As Date
            If (IsDate(dataRowSubstitution(19)) = False) Then
                result(2) = "customdate3 required date." : GoTo selesai
            End If
            'urutan(21) As Double
            If (IsNumeric(dataRowSubstitution(21)) = False) Then
                result(2) = "urutan required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA BONUS -----------------------------------

            'VALIDASI DATA BONUS ---------------------------------------
            'idsubstitution(0) As 
            If Len(dataRowSubstitution(0)) = 0 Then
                result(2) = "Row : " & i & " - idsubstitution can't be empty" : GoTo selesai
            End If
            If Len(dataRowSubstitution(0)) > 20 Then
                result(2) = "Row : " & i & " - idsubstitution should not be more than 20 character." : GoTo selesai
            End If

            'idsbi(1) As 
            If Len(dataRowSubstitution(1)) = 0 Then
                result(2) = "Row : " & i & " - idsbi can't be empty" : GoTo selesai
            End If
            If Len(dataRowSubstitution(1)) > 20 Then
                result(2) = "Row : " & i & " - idsbi should not be more than 20 character." : GoTo selesai
            End If

            'idsbidetail(2) As 
            If Len(dataRowSubstitution(2)) = 0 Then
                result(2) = "Row : " & i & " - idsbidetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowSubstitution(2)) > 20 Then
                result(2) = "Row : " & i & " - idsbidetail should not be more than 20 character." : GoTo selesai
            End If

            'idbarang(3) As 
            If Len(dataRowSubstitution(3)) = 0 Then
                result(2) = "Row : " & i & " - idbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowSubstitution(3)) > 20 Then
                result(2) = "Row : " & i & " - idbarang should not be more than 20 character." : GoTo selesai
            End If

            'jml(4) As Double
            If Len(dataRowSubstitution(4)) = 0 Then
                result(2) = "Row : " & i & " - jml can't be empty" : GoTo selesai
            End If

            'satuan(5) As String
            If Len(dataRowSubstitution(5)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowSubstitution(5)) > 25 Then
                result(2) = "Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'customdbl1(14) As Double
            If Len(dataRowSubstitution(14)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(15) As Double
            If Len(dataRowSubstitution(15)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(16) As Double
            If Len(dataRowSubstitution(16)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(17) As Date
            If Len(dataRowSubstitution(17)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(18) As Date
            If Len(dataRowSubstitution(18)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(19) As Date
            If Len(dataRowSubstitution(19)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'urutan(21) As Date
            If Len(dataRowSubstitution(21)) = 0 Then
                result(2) = "Row : " & i & " - urutan can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtsubstitution, "idsubstitution~idsbi~idsbidetail~idbarang~jml~satuan~customtext1~customtext2~customtext3~customtext4~customtext5~customint1~customint2~customint3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~nogrup~urutan", dataRowSubstitution(0) & "~" & dataRowSubstitution(1) & "~" & dataRowSubstitution(2) & "~" & dataRowSubstitution(3) & "~" & dataRowSubstitution(4) & "~" & dataRowSubstitution(5) & "~" & dataRowSubstitution(6) & "~" & dataRowSubstitution(7) & "~" & dataRowSubstitution(8) & "~" & dataRowSubstitution(9) & "~" & dataRowSubstitution(10) & "~" & dataRowSubstitution(11) & "~" & dataRowSubstitution(12) & "~" & dataRowSubstitution(13) & "~" & dataRowSubstitution(14) & "~" & dataRowSubstitution(15) & "~" & dataRowSubstitution(16) & "~" & dataRowSubstitution(17) & "~" & dataRowSubstitution(18) & "~" & dataRowSubstitution(19) & "~" & dataRowSubstitution(20) & "~" & dataRowSubstitution(21)) = False Then
                result(2) = "Substitution Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

        Next
        'END OF VALIDASI DAN SET ROW DATA BONUS ===========================================


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
                Dim vModuleId As Integer = 12, vMenuId As Integer = 56
                Select Case drutama("sbistatus")
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


                If isUpdate Then
                    result(4) = drutama("sbiid")
                    notransaksi = drutama("sbinotransaksi")

                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(sbiid), sbinotransaksi FROM M_12_Sbi WHERE sbiid=" & result(4), myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(sbiid) FROM M_12_Sbi WHERE sbinotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        ''SIMPAN HISTORY ========================
                        'Dim SimpanHistory As New m12_sbi_history
                        'Dim rsSimpanHistory As String = SimpanHistory.M12_Bi_HistorySimpan("" & paramSplit(0) & "★M12_Bi_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("bisumber")) & "▼" & FixQuotes(drutama("biid")) & "")
                        'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        'If (rsSplitResult(1) = 0) Then
                        '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        'End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M_12_Sbi set sbicabang  = '" & FixQuotes(drutama("sbicabang")) & "', sbilokasi  = '" & FixQuotes(drutama("sbilokasi")) & "', sbisumber  = '" & FixQuotes(drutama("sbisumber")) & "', sbikategoripos  = '" & FixQuotes(drutama("sbikategoripos")) & "', sbiautonotransaksi  = " & drutama("sbiautonotransaksi") & ", sbinotransaksi  = '" & FixQuotes(drutama("sbinotransaksi")) & "', sbitgl  = '" & FixQuotes(AsFormatTanggal(drutama("sbitgl"))) & "', sbikodepa  = '" & FixQuotes(drutama("sbikodepa")) & "', sbikontak  = '" & FixQuotes(drutama("sbikontak")) & "', sbikontakperson  = '" & FixQuotes(drutama("sbikontakperson")) & "', sbiuraian  = '" & FixQuotes(drutama("sbiuraian")) & "', sbicatatan  = '" & FixQuotes(drutama("sbicatatan")) & "', sbistatus  = " & drutama("sbistatus") & ", sbistatussebelumnya  = " & drutama("sbistatussebelumnya") & ", sbijmlrevisi  = " & drutama("sbijmlrevisi") & ", sbicetakanke  = " & drutama("sbicetakanke") & ", sbiisclose  = " & drutama("sbiisclose") & ", sbiinputuser  = '" & FixQuotes(drutama("sbiinputuser")) & "', sbimodifikasiuser  = '" & FixQuotes(drutama("sbimodifikasiuser")) & "', sbimodifikasitgl  = NOW(), sbiposting  = " & drutama("sbiposting") & ", sbipostingtgl  = '" & FixQuotes(AsFormatTanggal(drutama("sbipostingtgl"), "yyyy-MM-dd H:mm:ss")) & "', sbicustomtext1  = '" & FixQuotes(drutama("sbicustomtext1")) & "', sbicustomtext2  = '" & FixQuotes(drutama("sbicustomtext2")) & "', sbicustomtext3  = '" & FixQuotes(drutama("sbicustomtext3")) & "', sbicustomtext4  = '" & FixQuotes(drutama("sbicustomtext4")) & "', sbicustomtext5  = '" & FixQuotes(drutama("sbicustomtext5")) & "', sbicustomint1  = " & drutama("sbicustomint1") & ", sbicustomint2  = " & drutama("sbicustomint2") & ", sbicustomint3  = " & drutama("sbicustomint3") & ", sbicustomdbl1  = '" & FixDouble(drutama("sbicustomdbl1")) & "', sbicustomdbl2  = '" & FixDouble(drutama("sbicustomdbl2")) & "', sbicustomdbl3  = '" & FixDouble(drutama("sbicustomdbl3")) & "', sbicustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("sbicustomdate1"))) & "', sbicustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("sbicustomdate2"))) & "', sbicustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("sbicustomdate3"))) & "', sbijeniskategori  = '" & FixQuotes(drutama("sbijeniskategori")) & "' where sbiid = " & drutama("sbiid") & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Else
                        result(2) = "Transaction data not found." : GoTo selesai
                    End If
                Else

                    If drutama("sbiautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("sbicabang"), drutama("sbilokasi"), drutama("sbisumber"), drutama("sbitgl"))
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
                        notransaksi = drutama("sbinotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(sbiid) FROM m_12_sbi WHERE sbinotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M_12_Sbi (sbicabang, sbilokasi, sbisumber, sbikategoripos, sbiautonotransaksi, sbinotransaksi, sbitgl, sbikodepa, sbikontak, sbikontakperson, sbiuraian, sbicatatan, sbistatus, sbistatussebelumnya, sbijmlrevisi, sbicetakanke, sbiisclose, sbiinputuser, sbiinputtgl, sbimodifikasiuser, sbimodifikasitgl, sbiposting, sbipostingtgl, sbicustomtext1, sbicustomtext2, sbicustomtext3, sbicustomtext4, sbicustomtext5, sbicustomint1, sbicustomint2, sbicustomint3, sbicustomdbl1, sbicustomdbl2, sbicustomdbl3, sbicustomdate1, sbicustomdate2, sbicustomdate3, sbijeniskategori) values('" & FixQuotes(drutama("sbicabang")) & "', '" & FixQuotes(drutama("sbilokasi")) & "', '" & FixQuotes(drutama("sbisumber")) & "', '" & FixQuotes(drutama("sbikategoripos")) & "', " & drutama("sbiautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("sbitgl"))) & "', '" & FixQuotes(drutama("sbikodepa")) & "', '" & FixQuotes(drutama("sbikontak")) & "', '" & FixQuotes(drutama("sbikontakperson")) & "', '" & FixQuotes(drutama("sbiuraian")) & "', '" & FixQuotes(drutama("sbicatatan")) & "', " & drutama("sbistatus") & ", " & drutama("sbistatussebelumnya") & ", " & drutama("sbijmlrevisi") & ", " & drutama("sbicetakanke") & ", " & drutama("sbiisclose") & ", '" & FixQuotes(drutama("sbiinputuser")) & "', NOW(), '" & FixQuotes(drutama("sbimodifikasiuser")) & "', '1971-01-01 00:00:00', 0, '1971-01-01 00:00:00', '" & FixQuotes(drutama("sbicustomtext1")) & "', '" & FixQuotes(drutama("sbicustomtext2")) & "', '" & FixQuotes(drutama("sbicustomtext3")) & "', '" & FixQuotes(drutama("sbicustomtext4")) & "', '" & FixQuotes(drutama("sbicustomtext5")) & "', " & drutama("sbicustomint1") & ", " & drutama("sbicustomint2") & ", " & drutama("sbicustomint3") & ", '" & FixDouble(drutama("sbicustomdbl1")) & "', '" & FixDouble(drutama("sbicustomdbl2")) & "', '" & FixDouble(drutama("sbicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sbicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sbicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sbicustomdate3"))) & "', " & drutama("sbijeniskategori") & ")"
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
                    dt2 = AsDataTableAmbilDariDBCon("select sbiid from M_12_sbi where sbinotransaksi='" & notransaksi & "' AND sbiinputuser= '" & drutama("sbiinputuser") & "' order by sbimodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M_12_Sbi_Detail where idsbi = " & result(4)
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Hapus substitution ketika update
                If (isUpdate) Then
                    sql = "Delete from M_12_Sbi_Substitution where idsbi = " & result(4)
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
                    Dim dtSubstitutionGrup As New DataTable

                    For Each dr1 As DataRow In dtdetail.Rows

                        'CEK OPERATOR :
                        'JIKA BETWEEN (0) ATAU >= (1) MAKA BOLEH LEBIH DARI SATU KONDISI 
                        '=> BEBERAPA KONDISI BETWEEN DAN SATU KONDISI >= (1)
                        'JIKA KELIPATAN (2) MAKA HANYA BOLEH SATU KONDISI
                        Dim dtOperator As New DataTable
                        sql = "SELECT sbid.sbikategori as kategori, sbid.idbarang as idbarang, sbid.operator as operator, i.bkode, (CASE sbid.operator WHEN 0 THEN 'Between' WHEN 1 THEN '>=' WHEN 2 THEN 'Multiple' ELSE 'Unknown' END) as operatornama FROM m_12_sbi_detail sbid JOIN m1_item i ON sbid.idbarang = i.bid WHERE sbid.sbikategori = '" & FxDB(drutama("sbikategoripos"), "") & "' AND sbid.idbarang = '" & FxDB(dr1("idbarang"), "") & "' AND sbid.idsbi = '" & result(4) & "' AND sbid.idsbidetail <> '" & FxDB(dr1("idsbidetail"), "") & "' GROUP BY sbid.operator ORDER BY sbid.operator"
                        dtOperator = AsDataTableAmbilDariDBCon(sql, myConn)
                        'result(2) = sql : Trans.Rollback() : GoTo selesai
                        If dtOperator.Rows.Count > 0 Then

                            Dim vOperator As String = ""
                            Dim vIdBarang As Integer = 0
                            For Each drOperator As DataRow In dtOperator.Rows
                                vOperator = FxDB(drOperator("operator").ToString, "")
                                vIdBarang = FxDB(drOperator("idbarang").ToString, "")
                                If Len(vOperator) > 0 Then
                                    If vOperator = 2 Then
                                        'JIKA SUDAH TERDAPAT OPERATOR KELIPATAN (2)
                                        result(2) = "Item : " & FxDB(drOperator("bkode"), "") & " - already has '" & FxDB(drOperator("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                    Else
                                        'JIKA BELUM TERDAPAT OPERATOR KELIPATAN (2), CEK KONDISI OPERATOR YANG SUDAH DIINPUTKAN
                                        'JIKA OPERATOR YANG DIINPUTKAN ADALAH KELIPATAN (2) MAKA TAMPILKAN ERRMESSAGE
                                        'JIKA SUDAH TERDAPAT OPERATOR >= (1) DAN YANG DIINPUTKAN ADALAH OPERATOR >= (1) LAGI MAKA TAMPILKAN ERRMESSAGE
                                        If dr1("idbarang") = vIdBarang And (dr1("operator") = 2 Or (vOperator = 1 And dr1("operator") = vOperator)) Then
                                            result(2) = "Item : " & FxDB(drOperator("bkode"), "") & " - already has '" & FxDB(drOperator("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                        End If
                                    End If
                                End If
                            Next
                        End If

                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("idsbidetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("sbikategori")) & "', '" & FixQuotes(dr1("idbarang")) & "', '" & FixQuotes(dr1("operator")) & "', '" & FixDouble(dr1("jml1")) & "', '" & FixDouble(dr1("jml2")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl2"))) & "', '" & FixQuotes(dr1("nopromo")) & "')")

                        'sql = "Insert into M_12_Bi_Detail(idbidetail, idbi, bikategori, idbarang, operator, jml1, jml2, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, tgl1, tgl2, nopromo) values" & strValue2.ToString & ""
                        sql = "Insert into M_12_Sbi_Detail(idsbidetail, idsbi, sbikategori, idbarang, operator, jml1, jml2, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, tgl1, tgl2, nopromo, catatan, urutan) values('" & FixQuotes(dr1("idsbidetail")) & "', " & result(4) & ", '" & FixQuotes(drutama("sbikategoripos")) & "', '" & FixQuotes(dr1("idbarang")) & "', '" & FixQuotes(dr1("operator")) & "', '" & FixDouble(dr1("jml1")) & "', '" & FixDouble(dr1("jml2")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl2"))) & "', '" & notransaksi & "', '" & FixQuotes(dr1("catatan")) & "','" & FixQuotes(dr1("urutan")) & "')"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()


                        'ambil ID detail untuk diinsert ke substitution
                        Dim iddetail As Integer
                        Dim dtidsubstitution As New DataTable
                        'Sql disesuaikan sendiri, untuk parameternya disesuaikan sendiri.
                        dtidsubstitution = AsDataTableAmbilDariDBCon("select idsbidetail from M_12_sbi_detail where idsbi='" & result(4) & "' and sbikategori = '" & drutama("sbikategoripos") & "' AND  idbarang = '" & dr1("idbarang") & "' AND  operator = '" & dr1("operator") & "' AND  jml1 = '" & dr1("jml1") & "' AND jml2 = '" & dr1("jml2") & "' order by idsbidetail desc limit 1", myConn)
                        If dtidsubstitution.Rows.Count > 0 Then iddetail = dtidsubstitution.Rows(0)(0) Else result(2) = "#1 Detail transaction data not found." : Trans.Rollback() : GoTo selesai

                        'Proses Substitution
                        If (dtsubstitution.Rows.Count > 0) Then
                            'AMBIL DETAIL BONUS SESUAI NO GRUP
                            dtSubstitutionGrup = AsDataTableFilterSortDt(dtsubstitution, "nogrup = '" & dr1("nogrup") & "'")
                            If (dtSubstitutionGrup.Rows.Count > 0) Then
                                strValue2.Clear()
                                For Each drSubstitutionGrup As DataRow In dtSubstitutionGrup.Rows
                                    strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                                    strValue2.Append("('" & FixQuotes(drSubstitutionGrup("idsubstitution")) & "', " & result(4) & ", '" & iddetail & "', '" & FixQuotes(drSubstitutionGrup("idbarang")) & "', '" & FixDouble(drSubstitutionGrup("jml")) & "', '" & FixQuotes(drSubstitutionGrup("satuan")) & "', '" & FixQuotes(drSubstitutionGrup("customtext1")) & "', '" & FixQuotes(drSubstitutionGrup("customtext2")) & "', '" & FixQuotes(drSubstitutionGrup("customtext3")) & "', '" & FixQuotes(drSubstitutionGrup("customtext4")) & "', '" & FixQuotes(drSubstitutionGrup("customtext5")) & "', " & drSubstitutionGrup("customint1") & ", " & drSubstitutionGrup("customint2") & ", " & drSubstitutionGrup("customint3") & ", '" & FixDouble(drSubstitutionGrup("customdbl1")) & "', '" & FixDouble(drSubstitutionGrup("customdbl2")) & "', '" & FixDouble(drSubstitutionGrup("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drSubstitutionGrup("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drSubstitutionGrup("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drSubstitutionGrup("customdate3"))) & "', '" & FixQuotes(drSubstitutionGrup("urutan")) & "')")
                                Next

                                sql = "Insert into M_12_Sbi_Substitution(idsubstitution, idsbi, idsbidetail,  idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, urutan) values" & strValue2.ToString & ""
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                            Else
                                result(2) = "Substitution Transaction for No. Group : " & dr1("nogrup") & " does not found." : Trans.Rollback() : GoTo selesai
                            End If

                        Else
                            result(2) = "Substitution Transaction data not found." : Trans.Rollback() : GoTo selesai
                        End If
                    Next

                Else
                    result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Update ke tabel Barang Substitution
                If drutama("sbistatus") = 2 Then

                    'JIKA PER KATEGORI, HAPUS DATA PER KATEGORI
                    If drutama("sbijeniskategori") = 1 Then
                        'Cek apakah kategori pos sudah ada di tabel pos_substitution_item, jika sudah ada, hapus data di tabel itu
                        Dim dtPOSSubstitutionItem As New DataTable
                        dtPOSSubstitutionItem = AsDataTableAmbilDariDBCon("select siid from M_12_Pos_Substitution_Item where sikategori = '" & drutama("sbikategoripos") & "'", myConn)
                        Dim strValueItemUtama As New StringBuilder
                        Dim strValueItemDetail As New StringBuilder
                        If dtPOSSubstitutionItem.Rows.Count > 0 Then
                            For Each drPOSSubstitutionItem As DataRow In dtPOSSubstitutionItem.Rows
                                'QUERY HAPUS POS BONUS ITEM UTAMA
                                strValueItemUtama.Append(IIf(Len(strValueItemUtama.ToString) = 0, "", " OR "))
                                strValueItemUtama.Append("siid = '" & FixQuotes(drPOSSubstitutionItem("siid")) & "'")

                                'QUERY HAPUS POS BONUS ITEM DETAIL
                                strValueItemDetail.Append(IIf(Len(strValueItemDetail.ToString) = 0, "", " OR "))
                                strValueItemDetail.Append("idsi = '" & FixQuotes(drPOSSubstitutionItem("siid")) & "'")
                            Next

                            'HAPUS POS BONUS ITEM UTAMA
                            sql = "Delete From m_12_pos_substitution_item where " & strValueItemUtama.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                            'HAPUS POS BONUS ITEM DETAIL
                            sql = "Delete From m_12_pos_substitution_item_detail where " & strValueItemDetail.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If

                    ElseIf drutama("sbijeniskategori") = 2 Then 'PER CABANG
                        'ambil kategori pos sesuai cabang
                        Dim dtCatPOS As DataTable = AsDataTableAmbilDariDBCon("SELECT GROUP_CONCAT(" & Chr(34) & "'" & Chr(34) & ",l.lkategoripos," & Chr(34) & "'" & Chr(34) & ") as kategoripos FROM m1_location l WHERE l.lkategoripos <> '' AND l.lcabang = '" & FixQuotes(drutama("sbicabang")) & "'", myConn)
                        If dtCatPOS.Rows.Count > 0 Then
                            If Len(FxDB(dtCatPOS.Rows(0)(0), "")) > 0 Then
                                'Cek apakah kategori pos sudah ada di tabel pos_substitution_item, jika sudah ada, hapus data di tabel itu
                                Dim dtPOSSubstitutionItem As New DataTable
                                dtPOSSubstitutionItem = AsDataTableAmbilDariDBCon("select siid from M_12_Pos_Substitution_Item where sikategori IN (" & dtCatPOS.Rows(0)(0) & ")", myConn)
                                Dim strValueItemUtama As New StringBuilder
                                Dim strValueItemDetail As New StringBuilder
                                If dtPOSSubstitutionItem.Rows.Count > 0 Then
                                    For Each drPOSSubstitutionItem As DataRow In dtPOSSubstitutionItem.Rows
                                        'QUERY HAPUS POS BONUS ITEM UTAMA
                                        strValueItemUtama.Append(IIf(Len(strValueItemUtama.ToString) = 0, "", " OR "))
                                        strValueItemUtama.Append("siid = '" & FixQuotes(drPOSSubstitutionItem("siid")) & "'")

                                        'QUERY HAPUS POS BONUS ITEM DETAIL
                                        strValueItemDetail.Append(IIf(Len(strValueItemDetail.ToString) = 0, "", " OR "))
                                        strValueItemDetail.Append("idsi = '" & FixQuotes(drPOSSubstitutionItem("siid")) & "'")
                                    Next

                                    'HAPUS POS BONUS ITEM UTAMA
                                    sql = "Delete From m_12_pos_substitution_item where " & strValueItemUtama.ToString & ""
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()

                                    'HAPUS POS BONUS ITEM DETAIL
                                    sql = "Delete From m_12_pos_substitution_item_detail where " & strValueItemDetail.ToString & ""
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

                    Else 'JIKA SEMUA KATEGORI
                        'HAPUS POS BONUS ITEM UTAMA
                        sql = "Delete From m_12_pos_substitution_item"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'HAPUS POS BONUS ITEM DETAIL
                        sql = "Delete From m_12_pos_substitution_item_detail"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    End If


                    'AMBIL DATA BI DETAIL
                    Dim dtdtl As New DataTable
                    dtdtl = AsDataTableAmbilDariDBCon("select * from M_12_Sbi_Detail where idsbi = '" & result(4) & "' order by idsbi asc", myConn)
                    Dim dtbisubstitution As New DataTable
                    'AMBIL DATA BI BONUS
                    dtbisubstitution = AsDataTableAmbilDariDBCon("select * from M_12_Sbi_Substitution where idsbi = '" & result(4) & "' order by idsbi asc", myConn)

                    Dim strValueInsertSubstitutionItem As New StringBuilder 'untuk query simpan di tabel substitution utama
                    Dim strValueSubstitutionItemDetail As New StringBuilder 'untuk query simpan di tabel substitution detail
                    Dim idpossubstitutionitem As Integer 'untuk variabel id transaksi pos substitution item utama
                    Dim dtselectId As New DataTable 'untuk query ambil id transaksi pos substitution item
                    Dim dtSubstitutionPenampung As New DataTable 'untuk menampung data bi substitution
                    Dim dtKatPOS As New DataTable 'untuk menampung data kategori pos, jika jenis kategori 
                    strValueSubstitutionItemDetail.Clear()

                    If dtdtl.Rows.Count > 0 Then

                        If drutama("sbijeniskategori") = 1 Then 'JIKA PER KATEGORI
                            For Each drdtl As DataRow In dtdtl.Rows
                                'persiapan insert ke tabel m_12_pos_substitution_item & tabel m_12_pos_substitution_item_detail
                                strValueInsertSubstitutionItem.Append(IIf(Len(strValueInsertSubstitutionItem.ToString) = 0, "", ", "))
                                strValueInsertSubstitutionItem.Append("('" & FixQuotes(drutama("sbikategoripos")) & "', '" & FixQuotes(drdtl("idbarang")) & "', '" & FixQuotes(drdtl("operator")) & "', '" & FixDouble(drdtl("jml1")) & "', '" & FixDouble(drdtl("jml2")) & "', '" & FixQuotes(drdtl("customtext1")) & "', '" & FixQuotes(drdtl("customtext2")) & "', '" & FixQuotes(drdtl("customtext3")) & "', '" & FixQuotes(drdtl("customtext4")) & "', '" & FixQuotes(drdtl("customtext5")) & "', " & drdtl("customint1") & ", " & drdtl("customint2") & ", " & drdtl("customint3") & ", '" & FixDouble(drdtl("customdbl1")) & "', '" & FixDouble(drdtl("customdbl2")) & "', '" & FixDouble(drdtl("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl2"))) & "', '" & FixQuotes(drdtl("nopromo")) & "')")
                            Next

                            'insert ke tabel m_12_pos_substitution_item
                            sql = "Insert into M_12_Pos_Substitution_Item (sikategori, siidbarang, sioperator, sijml1, sijml2, sicustomtext1, sicustomtext2, sicustomtext3, sicustomtext4, sicustomtext5, sicustomint1, sicustomint2, sicustomint3, sicustomdbl1, sicustomdbl2, sicustomdbl3, sicustomdate1, sicustomdate2, sicustomdate3, sitgl1, sitgl2, sinopromo) values " & strValueInsertSubstitutionItem.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()


                            For Each drdtl2 As DataRow In dtdtl.Rows
                                'ambil id ketika simpan
                                dtselectId = AsDataTableAmbilDariDBCon("select siid from M_12_Pos_Substitution_Item where sinopromo = '" & drdtl2("nopromo") & "' AND sikategori = '" & drdtl2("sbikategori") & "' AND siidbarang = '" & drdtl2("idbarang") & "' AND sioperator = '" & drdtl2("operator") & "' AND sijml1 = '" & drdtl2("jml1") & "' AND sijml2 = '" & drdtl2("jml2") & "' limit 1", myConn)
                                If dtselectId.Rows.Count > 0 Then idpossubstitutionitem = dtselectId.Rows(0)(0) Else result(2) = "Main POS Substitution Item transaction data not found." : Trans.Rollback() : GoTo selesai

                                'filter data substitution penampung, untuk dijadikan parameter simpan ke tabel pos substitution detail
                                dtSubstitutionPenampung = AsDataTableFilterSortDt(dtbisubstitution, "idsbidetail = '" & drdtl2("idsbidetail") & "'")
                                If dtSubstitutionPenampung.Rows.Count > 0 Then
                                    For Each drSubstitutionPenampung As DataRow In dtSubstitutionPenampung.Rows
                                        'parameter simpan ke tabel m_12_pos_substitution_item_DETAIL
                                        strValueSubstitutionItemDetail.Append(IIf(Len(strValueSubstitutionItemDetail.ToString) = 0, "", ", "))
                                        strValueSubstitutionItemDetail.Append("(" & idpossubstitutionitem & ", '" & FixQuotes(drSubstitutionPenampung("idbarang")) & "', '" & FixDouble(drSubstitutionPenampung("jml")) & "', '" & FixQuotes(drSubstitutionPenampung("satuan")) & "', '" & FixQuotes(drSubstitutionPenampung("customtext1")) & "', '" & FixQuotes(drSubstitutionPenampung("customtext2")) & "', '" & FixQuotes(drSubstitutionPenampung("customtext3")) & "', '" & FixQuotes(drSubstitutionPenampung("customtext4")) & "', '" & FixQuotes(drSubstitutionPenampung("customtext5")) & "', " & drSubstitutionPenampung("customint1") & ", " & drSubstitutionPenampung("customint2") & ", " & drSubstitutionPenampung("customint3") & ", '" & FixDouble(drSubstitutionPenampung("customdbl1")) & "', '" & FixDouble(drSubstitutionPenampung("customdbl2")) & "', '" & FixDouble(drSubstitutionPenampung("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drSubstitutionPenampung("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drSubstitutionPenampung("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drSubstitutionPenampung("customdate3"))) & "')")
                                    Next
                                Else
                                    result(2) = "Main Transaction POS Substitution Item data not found." : Trans.Rollback() : GoTo selesai
                                End If
                            Next

                            'INSERT KE TABEL POS BONUS DETAIL
                            sql = "Insert into M_12_Pos_Substitution_Item_Detail(idsi, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValueSubstitutionItemDetail.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                        ElseIf drutama("sbijeniskategori") = 2 Then 'JIKA PER CABANG

                            'ambil kategori pos sesuai cabang
                            Dim dtCatPOS As DataTable = AsDataTableAmbilDariDBCon("SELECT GROUP_CONCAT(" & Chr(34) & "'" & Chr(34) & ",l.lkategoripos," & Chr(34) & "'" & Chr(34) & ") as kategoripos FROM m1_location l WHERE l.lkategoripos <> '' AND l.lcabang = '" & FixQuotes(drutama("sbicabang")) & "'", myConn)
                            If dtCatPOS.Rows.Count > 0 Then
                                If Len(FxDB(dtCatPOS.Rows(0)(0), "")) > 0 Then
                                    Dim dtPosItem As New DataTable 'variabel untuk cari data barang pos
                                    'CARI DATA KATEGORI POS
                                    dtKatPOS = AsDataTableAmbilDariDBCon("select pckode from m_12_pos_category WHERE pckode IN (" & dtCatPOS.Rows(0)(0) & ")", myConn)
                                    If dtKatPOS.Rows.Count > 0 Then 'JIKA DATA KATEGORI POS ADA, AMBIL DATA BARANG POS
                                        For Each drKatPos As DataRow In dtKatPOS.Rows
                                            For Each drdtl As DataRow In dtdtl.Rows
                                                'AMBIL DATA BARANG POS
                                                dtPosItem = AsDataTableAmbilDariDBCon("select piidbarang from M_12_Pos_Item where pikategori = '" & drKatPos("pckode") & "' AND piidbarang = '" & drdtl("idbarang") & "' order by pikategori asc", myConn)
                                                If dtPosItem.Rows.Count > 0 Then
                                                    For Each drPosItem As DataRow In dtPosItem.Rows
                                                        'persiapan insert ke tabel m_12_pos_substitution_item 
                                                        strValueInsertSubstitutionItem.Append(IIf(Len(strValueInsertSubstitutionItem.ToString) = 0, "", ", "))
                                                        strValueInsertSubstitutionItem.Append("('" & FixQuotes(drKatPos("pckode")) & "', '" & FixQuotes(drdtl("idbarang")) & "', '" & FixQuotes(drdtl("operator")) & "', '" & FixDouble(drdtl("jml1")) & "', '" & FixDouble(drdtl("jml2")) & "', '" & FixQuotes(drdtl("customtext1")) & "', '" & FixQuotes(drdtl("customtext2")) & "', '" & FixQuotes(drdtl("customtext3")) & "', '" & FixQuotes(drdtl("customtext4")) & "', '" & FixQuotes(drdtl("customtext5")) & "', " & drdtl("customint1") & ", " & drdtl("customint2") & ", " & drdtl("customint3") & ", '" & FixDouble(drdtl("customdbl1")) & "', '" & FixDouble(drdtl("customdbl2")) & "', '" & FixDouble(drdtl("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl2"))) & "', '" & FixQuotes(drdtl("nopromo")) & "')")

                                                    Next
                                                End If

                                            Next
                                        Next
                                    End If

                                    'insert ke tabel m_12_pos_substitution_item
                                    sql = "Insert into M_12_Pos_Substitution_Item (sikategori, siidbarang, sioperator, sijml1, sijml2, sicustomtext1, sicustomtext2, sicustomtext3, sicustomtext4, sicustomtext5, sicustomint1, sicustomint2, sicustomint3, sicustomdbl1, sicustomdbl2, sicustomdbl3, sicustomdate1, sicustomdate2, sicustomdate3, sitgl1, sitgl2, sinopromo) values " & strValueInsertSubstitutionItem.ToString & ""
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()

                                    If dtKatPOS.Rows.Count > 0 Then
                                        For Each drKatPos As DataRow In dtKatPOS.Rows
                                            For Each drdtl2 As DataRow In dtdtl.Rows
                                                'ambil id ketika simpan
                                                'AMBIL DATA BARANG POS
                                                dtPosItem = AsDataTableAmbilDariDBCon("select piidbarang from M_12_Pos_Item where pikategori = '" & drKatPos("pckode") & "' AND piidbarang = '" & drdtl2("idbarang") & "' order by pikategori asc", myConn)
                                                If dtPosItem.Rows.Count > 0 Then
                                                    For Each drPosItem As DataRow In dtPosItem.Rows
                                                        dtselectId = AsDataTableAmbilDariDBCon("select siid from M_12_Pos_Substitution_Item where sinopromo = '" & drdtl2("nopromo") & "' AND sikategori = '" & drKatPos("pckode") & "' AND siidbarang = '" & drdtl2("idbarang") & "' AND sioperator = '" & drdtl2("operator") & "' AND sijml1 = '" & drdtl2("jml1") & "' AND sijml2 = '" & drdtl2("jml2") & "' limit 1", myConn)
                                                        If dtselectId.Rows.Count > 0 Then idpossubstitutionitem = dtselectId.Rows(0)(0) Else result(2) = "Main POS Substitution Item transaction data not found." : Trans.Rollback() : GoTo selesai
                                                        'filter data substitution penampung, untuk dijadikan parameter simpan ke tabel pos substitution detail
                                                        dtSubstitutionPenampung = AsDataTableFilterSortDt(dtbisubstitution, "idsbidetail = '" & drdtl2("idsbidetail") & "'")
                                                        If dtSubstitutionPenampung.Rows.Count > 0 Then
                                                            For Each drSubstitutionPenampung As DataRow In dtSubstitutionPenampung.Rows
                                                                'persiapan insert ke tabel m_12_pos_substitution_item_DETAIL
                                                                strValueSubstitutionItemDetail.Append(IIf(Len(strValueSubstitutionItemDetail.ToString) = 0, "", ", "))
                                                                strValueSubstitutionItemDetail.Append("(" & idpossubstitutionitem & ", '" & FixQuotes(drSubstitutionPenampung("idbarang")) & "', '" & FixDouble(drSubstitutionPenampung("jml")) & "', '" & FixQuotes(drSubstitutionPenampung("satuan")) & "', '" & FixQuotes(drSubstitutionPenampung("customtext1")) & "', '" & FixQuotes(drSubstitutionPenampung("customtext2")) & "', '" & FixQuotes(drSubstitutionPenampung("customtext3")) & "', '" & FixQuotes(drSubstitutionPenampung("customtext4")) & "', '" & FixQuotes(drSubstitutionPenampung("customtext5")) & "', " & drSubstitutionPenampung("customint1") & ", " & drSubstitutionPenampung("customint2") & ", " & drSubstitutionPenampung("customint3") & ", '" & FixDouble(drSubstitutionPenampung("customdbl1")) & "', '" & FixDouble(drSubstitutionPenampung("customdbl2")) & "', '" & FixDouble(drSubstitutionPenampung("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drSubstitutionPenampung("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drSubstitutionPenampung("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drSubstitutionPenampung("customdate3"))) & "')")

                                                            Next
                                                        Else
                                                            result(2) = "Main Transaction POS Substitution Item data not found." : Trans.Rollback() : GoTo selesai
                                                        End If
                                                    Next
                                                End If
                                            Next
                                        Next

                                        'INSERT KE TABEL POS SUBSTITUTION DETAIL
                                        sql = "Insert into M_12_Pos_Substitution_Item_Detail(idsi, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValueSubstitutionItemDetail.ToString & ""
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

                        Else 'JIKA SEMUA KATEGORI

                            Dim dtPosItem As New DataTable 'variabel untuk cari data barang pos
                            'CARI DATA KATEGORI POS
                            dtKatPOS = AsDataTableAmbilDariDBCon("select pckode from m_12_pos_category", myConn)
                            If dtKatPOS.Rows.Count > 0 Then 'JIKA DATA KATEGORI POS ADA, AMBIL DATA BARANG POS
                                For Each drKatPos As DataRow In dtKatPOS.Rows
                                    For Each drdtl As DataRow In dtdtl.Rows
                                        'AMBIL DATA BARANG POS
                                        dtPosItem = AsDataTableAmbilDariDBCon("select piidbarang from M_12_Pos_Item where pikategori = '" & drKatPos("pckode") & "' AND piidbarang = '" & drdtl("idbarang") & "' order by pikategori asc", myConn)
                                        If dtPosItem.Rows.Count > 0 Then
                                            For Each drPosItem As DataRow In dtPosItem.Rows
                                                'persiapan insert ke tabel m_12_pos_substitution_item 
                                                strValueInsertSubstitutionItem.Append(IIf(Len(strValueInsertSubstitutionItem.ToString) = 0, "", ", "))
                                                strValueInsertSubstitutionItem.Append("('" & FixQuotes(drKatPos("pckode")) & "', '" & FixQuotes(drdtl("idbarang")) & "', '" & FixQuotes(drdtl("operator")) & "', '" & FixDouble(drdtl("jml1")) & "', '" & FixDouble(drdtl("jml2")) & "', '" & FixQuotes(drdtl("customtext1")) & "', '" & FixQuotes(drdtl("customtext2")) & "', '" & FixQuotes(drdtl("customtext3")) & "', '" & FixQuotes(drdtl("customtext4")) & "', '" & FixQuotes(drdtl("customtext5")) & "', " & drdtl("customint1") & ", " & drdtl("customint2") & ", " & drdtl("customint3") & ", '" & FixDouble(drdtl("customdbl1")) & "', '" & FixDouble(drdtl("customdbl2")) & "', '" & FixDouble(drdtl("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl2"))) & "', '" & FixQuotes(drdtl("nopromo")) & "')")

                                            Next
                                        End If

                                    Next
                                Next
                            End If

                            'insert ke tabel m_12_pos_substitution_item
                            sql = "Insert into M_12_Pos_Substitution_Item (sikategori, siidbarang, sioperator, sijml1, sijml2, sicustomtext1, sicustomtext2, sicustomtext3, sicustomtext4, sicustomtext5, sicustomint1, sicustomint2, sicustomint3, sicustomdbl1, sicustomdbl2, sicustomdbl3, sicustomdate1, sicustomdate2, sicustomdate3, sitgl1, sitgl2, sinopromo) values " & strValueInsertSubstitutionItem.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                            If dtKatPOS.Rows.Count > 0 Then
                                For Each drKatPos As DataRow In dtKatPOS.Rows
                                    For Each drdtl2 As DataRow In dtdtl.Rows
                                        'ambil id ketika simpan
                                        'AMBIL DATA BARANG POS
                                        dtPosItem = AsDataTableAmbilDariDBCon("select piidbarang from M_12_Pos_Item where pikategori = '" & drKatPos("pckode") & "' AND piidbarang = '" & drdtl2("idbarang") & "' order by pikategori asc", myConn)
                                        If dtPosItem.Rows.Count > 0 Then
                                            For Each drPosItem As DataRow In dtPosItem.Rows
                                                dtselectId = AsDataTableAmbilDariDBCon("select siid from M_12_Pos_Substitution_Item where sinopromo = '" & drdtl2("nopromo") & "' AND sikategori = '" & drKatPos("pckode") & "' AND siidbarang = '" & drdtl2("idbarang") & "' AND sioperator = '" & drdtl2("operator") & "' AND sijml1 = '" & drdtl2("jml1") & "' AND sijml2 = '" & drdtl2("jml2") & "' limit 1", myConn)
                                                If dtselectId.Rows.Count > 0 Then idpossubstitutionitem = dtselectId.Rows(0)(0) Else result(2) = "Main POS Substitution Item transaction data not found." : Trans.Rollback() : GoTo selesai
                                                'filter data substitution penampung, untuk dijadikan parameter simpan ke tabel pos substitution detail
                                                dtSubstitutionPenampung = AsDataTableFilterSortDt(dtbisubstitution, "idsbidetail = '" & drdtl2("idsbidetail") & "'")
                                                If dtSubstitutionPenampung.Rows.Count > 0 Then
                                                    For Each drSubstitutionPenampung As DataRow In dtSubstitutionPenampung.Rows
                                                        'persiapan insert ke tabel m_12_pos_substitution_item_DETAIL
                                                        strValueSubstitutionItemDetail.Append(IIf(Len(strValueSubstitutionItemDetail.ToString) = 0, "", ", "))
                                                        strValueSubstitutionItemDetail.Append("(" & idpossubstitutionitem & ", '" & FixQuotes(drSubstitutionPenampung("idbarang")) & "', '" & FixDouble(drSubstitutionPenampung("jml")) & "', '" & FixQuotes(drSubstitutionPenampung("satuan")) & "', '" & FixQuotes(drSubstitutionPenampung("customtext1")) & "', '" & FixQuotes(drSubstitutionPenampung("customtext2")) & "', '" & FixQuotes(drSubstitutionPenampung("customtext3")) & "', '" & FixQuotes(drSubstitutionPenampung("customtext4")) & "', '" & FixQuotes(drSubstitutionPenampung("customtext5")) & "', " & drSubstitutionPenampung("customint1") & ", " & drSubstitutionPenampung("customint2") & ", " & drSubstitutionPenampung("customint3") & ", '" & FixDouble(drSubstitutionPenampung("customdbl1")) & "', '" & FixDouble(drSubstitutionPenampung("customdbl2")) & "', '" & FixDouble(drSubstitutionPenampung("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drSubstitutionPenampung("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drSubstitutionPenampung("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drSubstitutionPenampung("customdate3"))) & "')")

                                                    Next
                                                Else
                                                    result(2) = "Main Transaction POS Substitution Item data not found." : Trans.Rollback() : GoTo selesai
                                                End If
                                            Next
                                        End If
                                    Next
                                Next

                                'INSERT KE TABEL POS SUBSTITUTION DETAIL
                                sql = "Insert into M_12_Pos_Substitution_Item_Detail(idsi, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValueSubstitutionItemDetail.ToString & ""
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

                    Else
                        result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai

                    End If

                End If

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
        'myConn.Close()
        'myConn = Nothing
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
    Public Function M12_SbiUpdateStatus(ByVal param As String) As String
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
            Filter = Filter.Replace("sbikontakkode", "c.kkode")
            Filter = Filter.Replace("sbikontaknama", "c.knama")
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
            Dim sumber As String = "SBI", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Sbitgl, Sbinotransaksi, Sbistatus FROM m_12_Sbi WHERE Sbiid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Sbistatussebelumnya" : jnsaktivitas = 17
                'CEK STATUS TRANSAKSI, JIKA <> 7 MAKA TIDAK BISA UNCLOSE
                If statusTransaksi <> 7 Then result(2) = "Transaction has not closed, it can't be unclose." : Trans.Rollback() : GoTo selesai
            Else
                jnsaktivitas = nilaiStatus
            End If

            'SET ISDELETE = TRUE JIKA STATUS TRANSAKSI = 2/3/4/7 DAN JNS AKTIVITAS <> 7(CLOSE) & 17(UNCLOSE)
            If ((statusTransaksi = 2 Or statusTransaksi = 3 Or statusTransaksi = 4 Or statusTransaksi = 7) And jnsaktivitas <> 7 And jnsaktivitas <> 17) Then isDelete = True

            'CEK PERIODE AKUNTANSI ==============================================================
            'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglTransaksi), AsFormatTanggal(tglTransaksi))
            'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
            'END OF CEK PERIODE AKUNTANSI =======================================================


            'SIMPAN HISTORY ========================
            'Dim SimpanHistory As New m_12_Bi_history
            'Dim rsSimpanHistory As String = SimpanHistory.m12_Bi_HistorySimpan("" & paramSplit(0) & "★M12_Bi_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            'If (rsSplitResult(1) = 0) Then
            '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            'End If
            'END OF SIMPAN HISTORY ==================


            If isDelete Then
                'AMBIL JENIS KATEGORI UTAMA
                Dim dtutama As New DataTable
                dtutama = AsDataTableAmbilDariDBCon("SELECT * FROM M_12_Sbi WHERE sbiid=" & idtransaksi, myConn)
                If (dtutama.Rows.Count > 0) Then
                    For Each drutama As DataRow In dtutama.Rows

                        'AMBIL DATA DETAIL
                        dtdetail = AsDataTableAmbilDariDBCon("SELECT * FROM M_12_Sbi_Detail WHERE idsbi=" & idtransaksi, myConn)
                        If (dtdetail.Rows.Count > 0) Then
                            For Each drdetail As DataRow In dtdetail.Rows
                                Dim dtsubstitution As New DataTable
                                If drutama("sbijeniskategori") = 1 Then 'JIKA PER KATEGORI
                                    Dim query As String = "SELECT siid FROM m_12_pos_substitution_item WHERE sikategori='" & drdetail("sbikategori") & "' AND sinopromo = '" & drdetail("nopromo") & "'"
                                    dtsubstitution = AsDataTableAmbilDariDBCon(query, myConn)
                                    If dtsubstitution.Rows.Count > 0 Then
                                        For Each drsubstitution As DataRow In dtsubstitution.Rows
                                            'hapus data detail
                                            Dim strValue2 As New StringBuilder
                                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", " AND "))
                                            strValue2.Append("siid = '" & FixQuotes(drsubstitution("siid")) & "' ")
                                            sql = "Delete from M_12_pos_substitution_item WHERE " & strValue2.ToString
                                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                            With objCmd
                                                .Connection = myConn
                                                .Transaction = Trans
                                                .CommandType = CommandType.Text
                                                .CommandText = sql
                                            End With
                                            objCmd.ExecuteNonQuery()


                                            Dim strValueItemDetail As New StringBuilder
                                            strValueItemDetail.Append(IIf(Len(strValueItemDetail.ToString) = 0, "", " AND "))
                                            strValueItemDetail.Append("idsi = '" & FixQuotes(drsubstitution("siid")) & "'")

                                            'hapus data detail
                                            sql = "Delete from M_12_pos_substitution_item_Detail WHERE " & strValueItemDetail.ToString
                                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                            With objCmd
                                                .Connection = myConn
                                                .Transaction = Trans
                                                .CommandType = CommandType.Text
                                                .CommandText = sql
                                            End With
                                            objCmd.ExecuteNonQuery()
                                        Next
                                    End If
                                Else 'JIKA SEMUA KATEGORI
                                    Dim query As String = "SELECT siid FROM m_12_pos_substitution_item WHERE sinopromo = '" & drdetail("nopromo") & "'"
                                    dtsubstitution = AsDataTableAmbilDariDBCon(query, myConn)
                                    If dtsubstitution.Rows.Count > 0 Then
                                        For Each drsubstitution As DataRow In dtsubstitution.Rows
                                            'hapus data detail
                                            Dim strValue2 As New StringBuilder
                                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", " OR "))
                                            strValue2.Append("siid = '" & FixQuotes(drsubstitution("siid")) & "' ")
                                            sql = "Delete from M_12_pos_substitution_item WHERE " & strValue2.ToString
                                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                            With objCmd
                                                .Connection = myConn
                                                .Transaction = Trans
                                                .CommandType = CommandType.Text
                                                .CommandText = sql
                                            End With
                                            objCmd.ExecuteNonQuery()


                                            Dim strValueItemDetail As New StringBuilder
                                            strValueItemDetail.Append(IIf(Len(strValueItemDetail.ToString) = 0, "", " OR "))
                                            strValueItemDetail.Append("idsi = '" & FixQuotes(drsubstitution("siid")) & "'")

                                            'hapus data detail
                                            sql = "Delete from M_12_pos_substitution_item_Detail WHERE " & strValueItemDetail.ToString
                                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                            With objCmd
                                                .Connection = myConn
                                                .Transaction = Trans
                                                .CommandType = CommandType.Text
                                                .CommandText = sql
                                            End With
                                            objCmd.ExecuteNonQuery()
                                        Next
                                    End If
                                End If

                            Next


                            'hapus data detail
                            'sql = "Delete from M_12_Bi_Detail WHERE idbidetail=" & idtransaksi
                            'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            'With objCmd
                            '    .Connection = myconn
                            '    .Transaction = Trans
                            '    .CommandType = CommandType.Text
                            '    .CommandText = sql
                            'End With
                            'objCmd.ExecuteNonQuery()

                            ''jika status unclose maka nilai status ambil dari status sebelumnya
                            'If (nilaiStatus = "unclose") Then
                            '    Dim dtstatusbefore As DataTable
                            '    dtstatusbefore = asdatatableambildaridbcon("SELECT Bistatussebelumnya FROM M_12_Bi WHERE biid=" & idtransaksi)
                            '    nilaiStatus = Val(dtstatusbefore.Rows(0)(0))
                            'End If

                        End If
                    Next
                End If

            End If

            'update status utama
            sql = "UPDATE M_12_Sbi SET Sbistatus = " & nilaiStatus & ", Sbimodifikasiuser='" & userid & "', Sbimodifikasitgl = NOW(), Sbiposting = 0, Sbipostingtgl = '1971-01-01 00:00:00', Sbijmlrevisi = Sbijmlrevisi + 1 WHERE Sbiid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_SbiSearch(PostWsSearch(paramSplit(0), "M12_SbiSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_SbiDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("sbikontakkode", "c.kkode")
            Filter = Filter.Replace("sbikontaknama", "c.knama")
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
            Dim sumber As String = "SBI", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT sbiid, sbinotransaksi FROM m_12_sbi WHERE sbiid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT sbicabang, sbilokasi, sbisumber, sbiautonotransaksi, sbinotransaksi, sbitgl"
            sql &= " FROM M_12_sbi"
            sql &= " WHERE sbiid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("sbicabang")
                lokasi = dtNomorNext.Rows(0)("sbilokasi")
                sumber = dtNomorNext.Rows(0)("sbisumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("sbiautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("sbinotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("sbitgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M_12_sbi_Detail WHERE idsbi = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M_12_sbi WHERE sbiid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_SbiSearch(PostWsSearch(paramSplit(0), "M12_SbiSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_SbiSimpanOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataSubstitution(), dataRowSubstitution() As String

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
        If (dataSplit.Length <> 3) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'sbiid(0) As Integer, sbicabang(1) As String, sbilokasi(2) As String, sbisumber(3) As String, sbikategoripos(4) As String, 
        'sbiautonotransaksi(5) As Integer, sbinotransaksi(6) As String, sbitgl(7) As Date, sbikodepa(8) As , sbikontak(9) As , 
        'sbikontakperson(10) As String, sbiuraian(11) As String, sbicatatan(12) As String, sbistatus(13) As Integer, sbistatussebelumnya(14) As Integer, 
        'sbijmlrevisi(15) As Integer, sbicetakanke(16) As Integer, sbiisclose(17) As Integer, sbiinputuser(18) As , sbiinputtgl(19) As DateTime, 
        'sbimodifikasiuser(20) As , sbimodifikasitgl(21) As DateTime, sbiposting(22) As Integer, sbipostingtgl(23) As DateTime, sbicustomtext1(24) As String, 
        'sbicustomtext2(25) As String, sbicustomtext3(26) As String, sbicustomtext4(27) As String, sbicustomtext5(28) As String, sbicustomint1(29) As Integer, 
        'sbicustomint2(30) As Integer, sbicustomint3(31) As Integer, sbicustomdbl1(32) As Double, sbicustomdbl2(33) As Double, sbicustomdbl3(34) As Double, 
        'sbicustomdate1(35) As Date, sbicustomdate2(36) As Date, sbicustomdate3(37) As Date, sbijeniskategori(38) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'sbiid, sbicabang, sbilokasi, sbisumber, sbikategoripos, sbiautonotransaksi, sbinotransaksi, 
        'sbitgl, sbikodepa, sbikontak, sbikontakperson, sbiuraian, sbicatatan, sbistatus, 
        'sbistatussebelumnya, sbijmlrevisi, sbicetakanke, sbiisclose, sbiinputuser, sbiinputtgl, sbimodifikasiuser, 
        'sbimodifikasitgl, sbiposting, sbipostingtgl, sbicustomtext1, sbicustomtext2, sbicustomtext3, sbicustomtext4, 
        'sbicustomtext5, sbicustomint1, sbicustomint2, sbicustomint3, sbicustomdbl1, sbicustomdbl2, sbicustomdbl3, 
        'sbicustomdate1, sbicustomdate2, sbicustomdate3, sbijeniskategori

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 39) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'sbiid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "sbiid required numeric." : GoTo selesai
        End If
        'sbiautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "sbiautonotransaksi required numeric." : GoTo selesai
        End If
        'sbitgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "sbitgl required date." : GoTo selesai
        End If
        'sbistatus(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "sbistatus required numeric." : GoTo selesai
        End If
        'sbistatussebelumnya(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "sbistatussebelumnya required numeric." : GoTo selesai
        End If
        'sbijmlrevisi(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "sbijmlrevisi required numeric." : GoTo selesai
        End If
        'sbicetakanke(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "sbicetakanke required numeric." : GoTo selesai
        End If
        'sbiisclose(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "sbiisclose required numeric." : GoTo selesai
        End If
        'sbiinputtgl(19) As DateTime
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "sbiinputtgl required date." : GoTo selesai
        End If
        'sbimodifikasitgl(21) As DateTime
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "sbimodifikasitgl required date." : GoTo selesai
        End If
        'sbiposting(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "sbiposting required numeric." : GoTo selesai
        End If
        'sbipostingtgl(23) As DateTime
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "sbipostingtgl required date." : GoTo selesai
        End If
        'sbicustomint1(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "sbicustomint1 required numeric." : GoTo selesai
        End If
        'sbicustomint2(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "sbicustomint2 required numeric." : GoTo selesai
        End If
        'sbicustomint3(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "sbicustomint3 required numeric." : GoTo selesai
        End If
        'sbicustomdbl1(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "sbicustomdbl1 required numeric." : GoTo selesai
        End If
        'sbicustomdbl2(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "sbicustomdbl2 required numeric." : GoTo selesai
        End If
        'sbicustomdbl3(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "sbicustomdbl3 required numeric." : GoTo selesai
        End If
        'sbicustomdate1(35) As Date
        If (IsDate(dataUtama(35)) = False) Then
            result(2) = "sbicustomdate1 required date." : GoTo selesai
        End If
        'sbicustomdate2(36) As Date
        If (IsDate(dataUtama(36)) = False) Then
            result(2) = "sbicustomdate2 required date." : GoTo selesai
        End If
        'sbicustomdate3(37) As Date
        If (IsDate(dataUtama(37)) = False) Then
            result(2) = "sbicustomdate3 required date." : GoTo selesai
        End If

        'sbijeniskategori(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "sbijeniskategori required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'sbicabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "sbicabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "sbicabang should not be more than 25 character." : GoTo selesai
        End If

        'sbilokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "sbilokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "sbilokasi should not be more than 25 character." : GoTo selesai
        End If

        'sbisumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "sbisumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "sbisumber should not be more than 10 character." : GoTo selesai
        End If

        'sbikategoripos(4) As String
        'If Len(dataUtama(4)) = 0 Then
        '    result(2) = "sbikategoripos can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(4)) > 50 Then
            result(2) = "sbikategoripos should not be more than 50 character." : GoTo selesai
        End If

        'sbinotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "sbinotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "sbinotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'sbitgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "sbitgl can't be empty" : GoTo selesai
        End If

        'sbikodepa(8) As 
        If Len(dataUtama(8)) = 0 Then
            result(2) = "sbikodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 20 Then
            result(2) = "sbikodepa should not be more than 20 character." : GoTo selesai
        End If

        'sbikontak(9) As 
        If Len(dataUtama(9)) = 0 Then
            result(2) = "sbikontak can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 20 Then
            result(2) = "sbikontak should not be more than 20 character." : GoTo selesai
        End If

        'sbiinputtgl(19) As DateTime
        If Len(dataUtama(19)) = 0 Then
            result(2) = "sbiinputtgl can't be empty" : GoTo selesai
        End If

        'sbimodifikasitgl(21) As DateTime
        If Len(dataUtama(21)) = 0 Then
            result(2) = "sbimodifikasitgl can't be empty" : GoTo selesai
        End If

        'sbipostingtgl(23) As DateTime
        If Len(dataUtama(23)) = 0 Then
            result(2) = "sbipostingtgl can't be empty" : GoTo selesai
        End If

        'sbicustomdbl1(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "sbicustomdbl1 can't be empty" : GoTo selesai
        End If

        'sbicustomdbl2(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "sbicustomdbl2 can't be empty" : GoTo selesai
        End If

        'sbicustomdbl3(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "sbicustomdbl3 can't be empty" : GoTo selesai
        End If

        'sbicustomdate1(35) As Date
        If Len(dataUtama(35)) = 0 Then
            result(2) = "sbicustomdate1 can't be empty" : GoTo selesai
        End If

        'sbicustomdate2(36) As Date
        If Len(dataUtama(36)) = 0 Then
            result(2) = "sbicustomdate2 can't be empty" : GoTo selesai
        End If

        'sbicustomdate3(37) As Date
        If Len(dataUtama(37)) = 0 Then
            result(2) = "sbicustomdate3 can't be empty" : GoTo selesai
        End If



        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "sbiid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbilokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbisumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbikategoripos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbiautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbinotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbikodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbikontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbikontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbiuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbistatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbistatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbijmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbicetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbiisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbiinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbiinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbimodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbimodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbiposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbipostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sbicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbicustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sbijeniskategori", AsEnumTypeData.AsInt64)

        If AsDataTableTambahData(dtutama, "sbiid~sbicabang~sbilokasi~sbisumber~sbikategoripos~sbiautonotransaksi~sbinotransaksi~sbitgl~sbikodepa~sbikontak~sbikontakperson~sbiuraian~sbicatatan~sbistatus~sbistatussebelumnya~sbijmlrevisi~sbicetakanke~sbiisclose~sbiinputuser~sbiinputtgl~sbimodifikasiuser~sbimodifikasitgl~sbiposting~sbipostingtgl~sbicustomtext1~sbicustomtext2~sbicustomtext3~sbicustomtext4~sbicustomtext5~sbicustomint1~sbicustomint2~sbicustomint3~sbicustomdbl1~sbicustomdbl2~sbicustomdbl3~sbicustomdate1~sbicustomdate2~sbicustomdate3~sbijeniskategori", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38)) = False Then
            result(2) = "insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idsbidetail(0) As , idsbi(1) As , sbikategori(2) As String, idbarang(3) As , operator(4) As String, 
        'jml1(5) As Double, jml2(6) As Double, customtext1(7) As String, customtext2(8) As String, customtext3(9) As String, 
        'customtext4(10) As String, customtext5(11) As String, customint1(12) As Integer, customint2(13) As Integer, customint3(14) As Integer, 
        'customdbl1(15) As Double, customdbl2(16) As Double, customdbl3(17) As Double, customdate1(18) As Date, customdate2(19) As Date, 
        'customdate3(20) As Date, tgl1(21) As Date, tgl2(22) As Date, nopromo(23) As String, nogrup (24) As String, catatan (25) As String, urutan(26) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idsbidetail, idsbi, sbikategori, idbarang, operator, jml1, jml2, 
        'customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, 
        'customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'tgl1, tgl2, nopromo, nogrup, catatan, urutan

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idsbidetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idsbi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sbikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "operator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nopromo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nogrup", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)

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
            'jml1(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "jml1 required numeric." : GoTo selesai
            End If
            'jml2(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "jml2 required numeric." : GoTo selesai
            End If
            'customint1(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "customint1 required numeric." : GoTo selesai
            End If
            'customint2(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "customint2 required numeric." : GoTo selesai
            End If
            'customint3(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "customint3 required numeric." : GoTo selesai
            End If
            'customdbl1(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(18) As Date
            If (IsDate(dataRowDetail(18)) = False) Then
                result(2) = "customdate1 required date." : GoTo selesai
            End If
            'customdate2(19) As Date
            If (IsDate(dataRowDetail(19)) = False) Then
                result(2) = "customdate2 required date." : GoTo selesai
            End If
            'customdate3(20) As Date
            If (IsDate(dataRowDetail(20)) = False) Then
                result(2) = "customdate3 required date." : GoTo selesai
            End If
            'tgl1(21) As Date
            If (IsDate(dataRowDetail(21)) = False) Then
                result(2) = "tgl1 required date." : GoTo selesai
            End If
            'tgl2(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "tgl2 required date." : GoTo selesai
            End If
            'urutan(26) As Double
            If (IsNumeric(dataRowDetail(26)) = False) Then
                result(2) = "urutan required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'idbidetail(0) As 
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idsbidetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idsbidetail should not be more than 20 character." : GoTo selesai
            End If

            'idbi(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idsbi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idsbi should not be more than 20 character." : GoTo selesai
            End If

            ''bikategori(2) As String
            'If Len(dataRowDetail(2)) = 0 Then
            '    result(2) = "Row : " & i & " - bikategori can't be empty" : GoTo selesai
            'End If
            'If Len(dataRowDetail(2)) > 25 Then
            '    result(2) = "Row : " & i & " - bikategori should not be more than 25 character." : GoTo selesai
            'End If

            'idbarang(3) As 
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - idbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 20 Then
                result(2) = "Row : " & i & " - idbarang should not be more than 20 character." : GoTo selesai
            End If

            'operator(4) As String
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - operator can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(4)) > 25 Then
                result(2) = "Row : " & i & " - operator should not be more than 25 character." : GoTo selesai
            End If

            'jml1(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - jml1 can't be empty" : GoTo selesai
            End If

            'jml2(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - jml2 can't be empty" : GoTo selesai
            End If

            'customdbl1(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(18) As Date
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(19) As Date
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(20) As Date
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idsbidetail~idsbi~sbikategori~idbarang~operator~jml1~jml2~customtext1~customtext2~customtext3~customtext4~customtext5~customint1~customint2~customint3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~tgl1~tgl2~nopromo~nogrup~catatan~urutan", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26)) = False Then
                result(2) = "Detail Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================

        'MAPPING BUAT WS DATA BONUS -------------------------------------------------------
        'idsubstitution(0) As , idsbi(1) As , idsbidetail(2) As , idbarang(3) As , jml(4) As Double, 
        'satuan(5) As String, customtext1(6) As String, customtext2(7) As String, customtext3(8) As String, customtext4(9) As String, 
        'customtext5(10) As String, customint1(11) As Integer, customint2(12) As Integer, customint3(13) As Integer, customdbl1(14) As Double, 
        'customdbl2(15) As Double, customdbl3(16) As Double, customdate1(17) As Date, customdate2(18) As Date, customdate3(19) As Date, nogrup(20) As String

        'MAPPING BUAT FLEX DATA BONUS -----------------------------------------------------
        'idsubstitution, idsbi, idsbidetail, idbarang, jml, satuan, customtext1, 
        'customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA BONUS ======================================================
        'SPLIT PARAMETER DATA BONUS
        dataSubstitution = dataSplit(2).Split(sptRow)
        'END OF VALIDASI DAN SET DATA BONUS ===============================================

        'Buat datatable substitution
        Dim dtsubstitution As New DataTable
        AsDataTableTambahField(dtsubstitution, "idsubstitution", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtsubstitution, "idsbi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtsubstitution, "idsbidetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtsubstitution, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtsubstitution, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "customint1", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtsubstitution, "customint2", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtsubstitution, "customint3", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtsubstitution, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "nogrup", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtsubstitution, "urutan", AsEnumTypeData.AsInt64)

        'VALIDASI DAN SET DATA ROW BONUS ==================================================
        Dim JmlDtSubstitution As Integer = dataSubstitution.Length
        For i = 1 To JmlDtSubstitution
            'SPLIT DATA DETAIL
            dataRowSubstitution = dataSubstitution(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA BONUS -----------------------------------
            'CEK ARRAY DATA BONUS
            If (dataRowSubstitution.Length <> 22) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW BONUS ----------------------------

            'VALIDASI TIPE DATA BONUS ------------------------------------------
            'jml(4) As Double
            If (IsNumeric(dataRowSubstitution(4)) = False) Then
                result(2) = "jml required numeric." : GoTo selesai
            End If
            'customint1(11) As Integer
            If (IsNumeric(dataRowSubstitution(11)) = False) Then
                result(2) = "customint1 required numeric." : GoTo selesai
            End If
            'customint2(12) As Integer
            If (IsNumeric(dataRowSubstitution(12)) = False) Then
                result(2) = "customint2 required numeric." : GoTo selesai
            End If
            'customint3(13) As Integer
            If (IsNumeric(dataRowSubstitution(13)) = False) Then
                result(2) = "customint3 required numeric." : GoTo selesai
            End If
            'customdbl1(14) As Double
            If (IsNumeric(dataRowSubstitution(14)) = False) Then
                result(2) = "customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(15) As Double
            If (IsNumeric(dataRowSubstitution(15)) = False) Then
                result(2) = "customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(16) As Double
            If (IsNumeric(dataRowSubstitution(16)) = False) Then
                result(2) = "customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(17) As Date
            If (IsDate(dataRowSubstitution(17)) = False) Then
                result(2) = "customdate1 required date." : GoTo selesai
            End If
            'customdate2(18) As Date
            If (IsDate(dataRowSubstitution(18)) = False) Then
                result(2) = "customdate2 required date." : GoTo selesai
            End If
            'customdate3(19) As Date
            If (IsDate(dataRowSubstitution(19)) = False) Then
                result(2) = "customdate3 required date." : GoTo selesai
            End If
            'urutan(21) As Double
            If (IsNumeric(dataRowSubstitution(21)) = False) Then
                result(2) = "urutan required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA BONUS -----------------------------------

            'VALIDASI DATA BONUS ---------------------------------------
            'idsubstitution(0) As 
            If Len(dataRowSubstitution(0)) = 0 Then
                result(2) = "Row : " & i & " - idsubstitution can't be empty" : GoTo selesai
            End If
            If Len(dataRowSubstitution(0)) > 20 Then
                result(2) = "Row : " & i & " - idsubstitution should not be more than 20 character." : GoTo selesai
            End If

            'idsbi(1) As 
            If Len(dataRowSubstitution(1)) = 0 Then
                result(2) = "Row : " & i & " - idsbi can't be empty" : GoTo selesai
            End If
            If Len(dataRowSubstitution(1)) > 20 Then
                result(2) = "Row : " & i & " - idsbi should not be more than 20 character." : GoTo selesai
            End If

            'idsbidetail(2) As 
            If Len(dataRowSubstitution(2)) = 0 Then
                result(2) = "Row : " & i & " - idsbidetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowSubstitution(2)) > 20 Then
                result(2) = "Row : " & i & " - idsbidetail should not be more than 20 character." : GoTo selesai
            End If

            'idbarang(3) As 
            If Len(dataRowSubstitution(3)) = 0 Then
                result(2) = "Row : " & i & " - idbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowSubstitution(3)) > 20 Then
                result(2) = "Row : " & i & " - idbarang should not be more than 20 character." : GoTo selesai
            End If

            'jml(4) As Double
            If Len(dataRowSubstitution(4)) = 0 Then
                result(2) = "Row : " & i & " - jml can't be empty" : GoTo selesai
            End If

            'satuan(5) As String
            If Len(dataRowSubstitution(5)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowSubstitution(5)) > 25 Then
                result(2) = "Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'customdbl1(14) As Double
            If Len(dataRowSubstitution(14)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(15) As Double
            If Len(dataRowSubstitution(15)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(16) As Double
            If Len(dataRowSubstitution(16)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(17) As Date
            If Len(dataRowSubstitution(17)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(18) As Date
            If Len(dataRowSubstitution(18)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(19) As Date
            If Len(dataRowSubstitution(19)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'urutan(21) As Date
            If Len(dataRowSubstitution(21)) = 0 Then
                result(2) = "Row : " & i & " - urutan can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtsubstitution, "idsubstitution~idsbi~idsbidetail~idbarang~jml~satuan~customtext1~customtext2~customtext3~customtext4~customtext5~customint1~customint2~customint3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~nogrup~urutan", dataRowSubstitution(0) & "~" & dataRowSubstitution(1) & "~" & dataRowSubstitution(2) & "~" & dataRowSubstitution(3) & "~" & dataRowSubstitution(4) & "~" & dataRowSubstitution(5) & "~" & dataRowSubstitution(6) & "~" & dataRowSubstitution(7) & "~" & dataRowSubstitution(8) & "~" & dataRowSubstitution(9) & "~" & dataRowSubstitution(10) & "~" & dataRowSubstitution(11) & "~" & dataRowSubstitution(12) & "~" & dataRowSubstitution(13) & "~" & dataRowSubstitution(14) & "~" & dataRowSubstitution(15) & "~" & dataRowSubstitution(16) & "~" & dataRowSubstitution(17) & "~" & dataRowSubstitution(18) & "~" & dataRowSubstitution(19) & "~" & dataRowSubstitution(20) & "~" & dataRowSubstitution(21)) = False Then
                result(2) = "Substitution Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

        Next
        'END OF VALIDASI DAN SET ROW DATA BONUS ===========================================


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
                If isUpdate Then
                    result(4) = drutama("sbiid")
                    notransaksi = drutama("sbinotransaksi")

                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(sbiid), sbinotransaksi FROM M_12_Sbi WHERE sbiid=" & result(4))
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(sbiid) FROM M_12_Sbi WHERE sbinotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        ''SIMPAN HISTORY ========================
                        'Dim SimpanHistory As New m12_sbi_history
                        'Dim rsSimpanHistory As String = SimpanHistory.M12_Bi_HistorySimpan("" & paramSplit(0) & "★M12_Bi_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("bisumber")) & "▼" & FixQuotes(drutama("biid")) & "")
                        'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        'If (rsSplitResult(1) = 0) Then
                        '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        'End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M_12_Sbi set sbicabang  = '" & FixQuotes(drutama("sbicabang")) & "', sbilokasi  = '" & FixQuotes(drutama("sbilokasi")) & "', sbisumber  = '" & FixQuotes(drutama("sbisumber")) & "', sbikategoripos  = '" & FixQuotes(drutama("sbikategoripos")) & "', sbiautonotransaksi  = " & drutama("sbiautonotransaksi") & ", sbinotransaksi  = '" & FixQuotes(drutama("sbinotransaksi")) & "', sbitgl  = '" & FixQuotes(AsFormatTanggal(drutama("sbitgl"))) & "', sbikodepa  = '" & FixQuotes(drutama("sbikodepa")) & "', sbikontak  = '" & FixQuotes(drutama("sbikontak")) & "', sbikontakperson  = '" & FixQuotes(drutama("sbikontakperson")) & "', sbiuraian  = '" & FixQuotes(drutama("sbiuraian")) & "', sbicatatan  = '" & FixQuotes(drutama("sbicatatan")) & "', sbistatus  = " & drutama("sbistatus") & ", sbistatussebelumnya  = " & drutama("sbistatussebelumnya") & ", sbijmlrevisi  = " & drutama("sbijmlrevisi") & ", sbicetakanke  = " & drutama("sbicetakanke") & ", sbiisclose  = " & drutama("sbiisclose") & ", sbiinputuser  = '" & FixQuotes(drutama("sbiinputuser")) & "', sbimodifikasiuser  = '" & FixQuotes(drutama("sbimodifikasiuser")) & "', sbimodifikasitgl  = NOW(), sbiposting  = " & drutama("sbiposting") & ", sbipostingtgl  = '" & FixQuotes(AsFormatTanggal(drutama("sbipostingtgl"), "yyyy-MM-dd H:mm:ss")) & "', sbicustomtext1  = '" & FixQuotes(drutama("sbicustomtext1")) & "', sbicustomtext2  = '" & FixQuotes(drutama("sbicustomtext2")) & "', sbicustomtext3  = '" & FixQuotes(drutama("sbicustomtext3")) & "', sbicustomtext4  = '" & FixQuotes(drutama("sbicustomtext4")) & "', sbicustomtext5  = '" & FixQuotes(drutama("sbicustomtext5")) & "', sbicustomint1  = " & drutama("sbicustomint1") & ", sbicustomint2  = " & drutama("sbicustomint2") & ", sbicustomint3  = " & drutama("sbicustomint3") & ", sbicustomdbl1  = '" & FixDouble(drutama("sbicustomdbl1")) & "', sbicustomdbl2  = '" & FixDouble(drutama("sbicustomdbl2")) & "', sbicustomdbl3  = '" & FixDouble(drutama("sbicustomdbl3")) & "', sbicustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("sbicustomdate1"))) & "', sbicustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("sbicustomdate2"))) & "', sbicustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("sbicustomdate3"))) & "', sbijeniskategori  = '" & FixQuotes(drutama("sbijeniskategori")) & "' where sbiid = " & drutama("sbiid") & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Else
                        result(2) = "Transaction data not found." : GoTo selesai
                    End If
                Else

                    If drutama("sbiautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("sbicabang"), drutama("sbilokasi"), drutama("sbisumber"), drutama("sbitgl"))
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
                        notransaksi = drutama("sbinotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(sbiid) FROM m_12_sbi WHERE sbinotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M_12_Sbi (sbicabang, sbilokasi, sbisumber, sbikategoripos, sbiautonotransaksi, sbinotransaksi, sbitgl, sbikodepa, sbikontak, sbikontakperson, sbiuraian, sbicatatan, sbistatus, sbistatussebelumnya, sbijmlrevisi, sbicetakanke, sbiisclose, sbiinputuser, sbiinputtgl, sbimodifikasiuser, sbimodifikasitgl, sbiposting, sbipostingtgl, sbicustomtext1, sbicustomtext2, sbicustomtext3, sbicustomtext4, sbicustomtext5, sbicustomint1, sbicustomint2, sbicustomint3, sbicustomdbl1, sbicustomdbl2, sbicustomdbl3, sbicustomdate1, sbicustomdate2, sbicustomdate3, sbijeniskategori) values('" & FixQuotes(drutama("sbicabang")) & "', '" & FixQuotes(drutama("sbilokasi")) & "', '" & FixQuotes(drutama("sbisumber")) & "', '" & FixQuotes(drutama("sbikategoripos")) & "', " & drutama("sbiautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("sbitgl"))) & "', '" & FixQuotes(drutama("sbikodepa")) & "', '" & FixQuotes(drutama("sbikontak")) & "', '" & FixQuotes(drutama("sbikontakperson")) & "', '" & FixQuotes(drutama("sbiuraian")) & "', '" & FixQuotes(drutama("sbicatatan")) & "', " & drutama("sbistatus") & ", " & drutama("sbistatussebelumnya") & ", " & drutama("sbijmlrevisi") & ", " & drutama("sbicetakanke") & ", " & drutama("sbiisclose") & ", '" & FixQuotes(drutama("sbiinputuser")) & "', NOW(), '" & FixQuotes(drutama("sbimodifikasiuser")) & "', '1971-01-01 00:00:00', 0, '1971-01-01 00:00:00', '" & FixQuotes(drutama("sbicustomtext1")) & "', '" & FixQuotes(drutama("sbicustomtext2")) & "', '" & FixQuotes(drutama("sbicustomtext3")) & "', '" & FixQuotes(drutama("sbicustomtext4")) & "', '" & FixQuotes(drutama("sbicustomtext5")) & "', " & drutama("sbicustomint1") & ", " & drutama("sbicustomint2") & ", " & drutama("sbicustomint3") & ", '" & FixDouble(drutama("sbicustomdbl1")) & "', '" & FixDouble(drutama("sbicustomdbl2")) & "', '" & FixDouble(drutama("sbicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sbicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sbicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sbicustomdate3"))) & "', " & drutama("sbijeniskategori") & ")"
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
                    dt2 = AsDataTableAmbilDariDB("select sbiid from M_12_sbi where sbinotransaksi='" & notransaksi & "' AND sbiinputuser= '" & drutama("sbiinputuser") & "' order by sbimodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M_12_Sbi_Detail where idsbi = " & result(4)
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Hapus substitution ketika update
                If (isUpdate) Then
                    sql = "Delete from M_12_Sbi_Substitution where idsbi = " & result(4)
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
                    Dim dtSubstitutionGrup As New DataTable

                    For Each dr1 As DataRow In dtdetail.Rows

                        'CEK OPERATOR :
                        'JIKA BETWEEN (0) ATAU >= (1) MAKA BOLEH LEBIH DARI SATU KONDISI 
                        '=> BEBERAPA KONDISI BETWEEN DAN SATU KONDISI >= (1)
                        'JIKA KELIPATAN (2) MAKA HANYA BOLEH SATU KONDISI
                        Dim dtOperator As New DataTable
                        sql = "SELECT sbid.sbikategori as kategori, sbid.idbarang as idbarang, sbid.operator as operator, i.bkode, (CASE sbid.operator WHEN 0 THEN 'Between' WHEN 1 THEN '>=' WHEN 2 THEN 'Multiple' ELSE 'Unknown' END) as operatornama FROM m_12_sbi_detail sbid JOIN m1_item i ON sbid.idbarang = i.bid WHERE sbid.sbikategori = '" & FxDB(drutama("sbikategoripos"), "") & "' AND sbid.idbarang = '" & FxDB(dr1("idbarang"), "") & "' AND sbid.idsbi = '" & result(4) & "' AND sbid.idsbidetail <> '" & FxDB(dr1("idsbidetail"), "") & "' GROUP BY sbid.operator ORDER BY sbid.operator"
                        dtOperator = AsDataTableAmbilDariDB(sql)
                        'result(2) = sql : Trans.Rollback() : GoTo selesai
                        If dtOperator.Rows.Count > 0 Then

                            Dim vOperator As String = ""
                            Dim vIdBarang As Integer = 0
                            For Each drOperator As DataRow In dtOperator.Rows
                                vOperator = FxDB(drOperator("operator").ToString, "")
                                vIdBarang = FxDB(drOperator("idbarang").ToString, "")
                                If Len(vOperator) > 0 Then
                                    If vOperator = 2 Then
                                        'JIKA SUDAH TERDAPAT OPERATOR KELIPATAN (2)
                                        result(2) = "Item : " & FxDB(drOperator("bkode"), "") & " - already has '" & FxDB(drOperator("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                    Else
                                        'JIKA BELUM TERDAPAT OPERATOR KELIPATAN (2), CEK KONDISI OPERATOR YANG SUDAH DIINPUTKAN
                                        'JIKA OPERATOR YANG DIINPUTKAN ADALAH KELIPATAN (2) MAKA TAMPILKAN ERRMESSAGE
                                        'JIKA SUDAH TERDAPAT OPERATOR >= (1) DAN YANG DIINPUTKAN ADALAH OPERATOR >= (1) LAGI MAKA TAMPILKAN ERRMESSAGE
                                        If dr1("idbarang") = vIdBarang And (dr1("operator") = 2 Or (vOperator = 1 And dr1("operator") = vOperator)) Then
                                            result(2) = "Item : " & FxDB(drOperator("bkode"), "") & " - already has '" & FxDB(drOperator("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                        End If
                                    End If
                                End If
                            Next
                        End If

                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("idsbidetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("sbikategori")) & "', '" & FixQuotes(dr1("idbarang")) & "', '" & FixQuotes(dr1("operator")) & "', '" & FixDouble(dr1("jml1")) & "', '" & FixDouble(dr1("jml2")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl2"))) & "', '" & FixQuotes(dr1("nopromo")) & "')")

                        'sql = "Insert into M_12_Bi_Detail(idbidetail, idbi, bikategori, idbarang, operator, jml1, jml2, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, tgl1, tgl2, nopromo) values" & strValue2.ToString & ""
                        sql = "Insert into M_12_Sbi_Detail(idsbidetail, idsbi, sbikategori, idbarang, operator, jml1, jml2, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, tgl1, tgl2, nopromo, catatan, urutan) values('" & FixQuotes(dr1("idsbidetail")) & "', " & result(4) & ", '" & FixQuotes(drutama("sbikategoripos")) & "', '" & FixQuotes(dr1("idbarang")) & "', '" & FixQuotes(dr1("operator")) & "', '" & FixDouble(dr1("jml1")) & "', '" & FixDouble(dr1("jml2")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl2"))) & "', '" & notransaksi & "', '" & FixQuotes(dr1("catatan")) & "','" & FixQuotes(dr1("urutan")) & "')"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()


                        'ambil ID detail untuk diinsert ke substitution
                        Dim iddetail As Integer
                        Dim dtidsubstitution As New DataTable
                        'Sql disesuaikan sendiri, untuk parameternya disesuaikan sendiri.
                        dtidsubstitution = AsDataTableAmbilDariDB("select idsbidetail from M_12_sbi_detail where idsbi='" & result(4) & "' and sbikategori = '" & drutama("sbikategoripos") & "' AND  idbarang = '" & dr1("idbarang") & "' AND  operator = '" & dr1("operator") & "' AND  jml1 = '" & dr1("jml1") & "' AND jml2 = '" & dr1("jml2") & "' order by idsbidetail desc limit 1")
                        If dtidsubstitution.Rows.Count > 0 Then iddetail = dtidsubstitution.Rows(0)(0) Else result(2) = "#1 Detail transaction data not found." : Trans.Rollback() : GoTo selesai

                        'Proses Substitution
                        If (dtsubstitution.Rows.Count > 0) Then
                            'AMBIL DETAIL BONUS SESUAI NO GRUP
                            dtSubstitutionGrup = AsDataTableFilterSortDt(dtsubstitution, "nogrup = '" & dr1("nogrup") & "'")
                            If (dtSubstitutionGrup.Rows.Count > 0) Then
                                strValue2.Clear()
                                For Each drSubstitutionGrup As DataRow In dtSubstitutionGrup.Rows
                                    strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                                    strValue2.Append("('" & FixQuotes(drSubstitutionGrup("idsubstitution")) & "', " & result(4) & ", '" & iddetail & "', '" & FixQuotes(drSubstitutionGrup("idbarang")) & "', '" & FixDouble(drSubstitutionGrup("jml")) & "', '" & FixQuotes(drSubstitutionGrup("satuan")) & "', '" & FixQuotes(drSubstitutionGrup("customtext1")) & "', '" & FixQuotes(drSubstitutionGrup("customtext2")) & "', '" & FixQuotes(drSubstitutionGrup("customtext3")) & "', '" & FixQuotes(drSubstitutionGrup("customtext4")) & "', '" & FixQuotes(drSubstitutionGrup("customtext5")) & "', " & drSubstitutionGrup("customint1") & ", " & drSubstitutionGrup("customint2") & ", " & drSubstitutionGrup("customint3") & ", '" & FixDouble(drSubstitutionGrup("customdbl1")) & "', '" & FixDouble(drSubstitutionGrup("customdbl2")) & "', '" & FixDouble(drSubstitutionGrup("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drSubstitutionGrup("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drSubstitutionGrup("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drSubstitutionGrup("customdate3"))) & "', '" & FixQuotes(drSubstitutionGrup("urutan")) & "')")
                                Next

                                sql = "Insert into M_12_Sbi_Substitution(idsubstitution, idsbi, idsbidetail,  idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, urutan) values" & strValue2.ToString & ""
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = Con1
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                            Else
                                result(2) = "Substitution Transaction for No. Group : " & dr1("nogrup") & " does not found." : Trans.Rollback() : GoTo selesai
                            End If

                        Else
                            result(2) = "Substitution Transaction data not found." : Trans.Rollback() : GoTo selesai
                        End If
                    Next

                Else
                    result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Update ke tabel Barang Substitution
                If drutama("sbistatus") = 2 Then
                    'JIKA PER KATEGORI, HAPUS DATA PER KATEGORI
                    If drutama("sbijeniskategori") = 1 Then
                        'Cek apakah kategori pos sudah ada di tabel pos_substitution_item, jika sudah ada, hapus data di tabel itu
                        Dim dtPOSSubstitutionItem As New DataTable
                        dtPOSSubstitutionItem = AsDataTableAmbilDariDB("select siid from M_12_Pos_Substitution_Item where sikategori = '" & drutama("sbikategoripos") & "'")
                        Dim strValueItemUtama As New StringBuilder
                        Dim strValueItemDetail As New StringBuilder
                        If dtPOSSubstitutionItem.Rows.Count > 1 Then
                            For Each drPOSSubstitutionItem As DataRow In dtPOSSubstitutionItem.Rows
                                'QUERY HAPUS POS BONUS ITEM UTAMA
                                strValueItemUtama.Append(IIf(Len(strValueItemUtama.ToString) = 0, "", " OR "))
                                strValueItemUtama.Append("siid = '" & FixQuotes(drPOSSubstitutionItem("siid")) & "'")

                                'QUERY HAPUS POS BONUS ITEM DETAIL
                                strValueItemDetail.Append(IIf(Len(strValueItemDetail.ToString) = 0, "", " OR "))
                                strValueItemDetail.Append("idsi = '" & FixQuotes(drPOSSubstitutionItem("siid")) & "'")
                            Next

                            'HAPUS POS BONUS ITEM UTAMA
                            sql = "Delete From m_12_pos_substitution_item where " & strValueItemUtama.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                            'HAPUS POS BONUS ITEM DETAIL
                            sql = "Delete From m_12_pos_substitution_item_detail where " & strValueItemDetail.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If

                    Else 'JIKA SEMUA KATEGORI
                        'HAPUS POS BONUS ITEM UTAMA
                        sql = "Delete From m_12_pos_substitution_item"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'HAPUS POS BONUS ITEM DETAIL
                        sql = "Delete From m_12_pos_substitution_item_detail"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    End If


                    'AMBIL DATA BI DETAIL
                    Dim dtdtl As New DataTable
                    dtdtl = AsDataTableAmbilDariDB("select * from M_12_Sbi_Detail where idsbi = '" & result(4) & "' order by idsbi asc")
                    Dim dtbisubstitution As New DataTable
                    'AMBIL DATA BI BONUS
                    dtbisubstitution = AsDataTableAmbilDariDB("select * from M_12_Sbi_Substitution where idsbi = '" & result(4) & "' order by idsbi asc")

                    Dim strValueInsertSubstitutionItem As New StringBuilder 'untuk query simpan di tabel substitution utama
                    Dim strValueSubstitutionItemDetail As New StringBuilder 'untuk query simpan di tabel substitution detail
                    Dim idpossubstitutionitem As Integer 'untuk variabel id transaksi pos substitution item utama
                    Dim dtselectId As New DataTable 'untuk query ambil id transaksi pos substitution item
                    Dim dtSubstitutionPenampung As New DataTable 'untuk menampung data bi substitution
                    Dim dtKatPOS As New DataTable 'untuk menampung data kategori pos, jika jenis kategori 
                    strValueSubstitutionItemDetail.Clear()

                    If dtdtl.Rows.Count > 0 Then

                        If drutama("sbijeniskategori") = 1 Then 'JIKA PER KATEGORI
                            For Each drdtl As DataRow In dtdtl.Rows
                                'persiapan insert ke tabel m_12_pos_substitution_item & tabel m_12_pos_substitution_item_detail
                                strValueInsertSubstitutionItem.Append(IIf(Len(strValueInsertSubstitutionItem.ToString) = 0, "", ", "))
                                strValueInsertSubstitutionItem.Append("('" & FixQuotes(drutama("sbikategoripos")) & "', '" & FixQuotes(drdtl("idbarang")) & "', '" & FixQuotes(drdtl("operator")) & "', '" & FixDouble(drdtl("jml1")) & "', '" & FixDouble(drdtl("jml2")) & "', '" & FixQuotes(drdtl("customtext1")) & "', '" & FixQuotes(drdtl("customtext2")) & "', '" & FixQuotes(drdtl("customtext3")) & "', '" & FixQuotes(drdtl("customtext4")) & "', '" & FixQuotes(drdtl("customtext5")) & "', " & drdtl("customint1") & ", " & drdtl("customint2") & ", " & drdtl("customint3") & ", '" & FixDouble(drdtl("customdbl1")) & "', '" & FixDouble(drdtl("customdbl2")) & "', '" & FixDouble(drdtl("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl2"))) & "', '" & FixQuotes(drdtl("nopromo")) & "')")
                            Next

                            'insert ke tabel m_12_pos_substitution_item
                            sql = "Insert into M_12_Pos_Substitution_Item (sikategori, siidbarang, sioperator, sijml1, sijml2, sicustomtext1, sicustomtext2, sicustomtext3, sicustomtext4, sicustomtext5, sicustomint1, sicustomint2, sicustomint3, sicustomdbl1, sicustomdbl2, sicustomdbl3, sicustomdate1, sicustomdate2, sicustomdate3, sitgl1, sitgl2, sinopromo) values " & strValueInsertSubstitutionItem.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()


                            For Each drdtl2 As DataRow In dtdtl.Rows
                                'ambil id ketika simpan
                                dtselectId = AsDataTableAmbilDariDB("select siid from M_12_Pos_Substitution_Item where sikategori = '" & drdtl2("sbikategori") & "' AND siidbarang = '" & drdtl2("idbarang") & "' AND sioperator = '" & drdtl2("operator") & "' AND sijml1 = '" & drdtl2("jml1") & "' AND sijml2 = '" & drdtl2("jml2") & "' limit 1")
                                If dtselectId.Rows.Count > 0 Then idpossubstitutionitem = dtselectId.Rows(0)(0) Else result(2) = "Main POS Substitution Item transaction data not found." : Trans.Rollback() : GoTo selesai

                                'filter data substitution penampung, untuk dijadikan parameter simpan ke tabel pos substitution detail
                                dtSubstitutionPenampung = AsDataTableFilterSortDt(dtbisubstitution, "idsbidetail = '" & drdtl2("idsbidetail") & "'")
                                If dtSubstitutionPenampung.Rows.Count > 0 Then
                                    For Each drSubstitutionPenampung As DataRow In dtSubstitutionPenampung.Rows
                                        'parameter simpan ke tabel m_12_pos_substitution_item_DETAIL
                                        strValueSubstitutionItemDetail.Append(IIf(Len(strValueSubstitutionItemDetail.ToString) = 0, "", ", "))
                                        strValueSubstitutionItemDetail.Append("(" & idpossubstitutionitem & ", '" & FixQuotes(drSubstitutionPenampung("idbarang")) & "', '" & FixDouble(drSubstitutionPenampung("jml")) & "', '" & FixQuotes(drSubstitutionPenampung("satuan")) & "', '" & FixQuotes(drSubstitutionPenampung("customtext1")) & "', '" & FixQuotes(drSubstitutionPenampung("customtext2")) & "', '" & FixQuotes(drSubstitutionPenampung("customtext3")) & "', '" & FixQuotes(drSubstitutionPenampung("customtext4")) & "', '" & FixQuotes(drSubstitutionPenampung("customtext5")) & "', " & drSubstitutionPenampung("customint1") & ", " & drSubstitutionPenampung("customint2") & ", " & drSubstitutionPenampung("customint3") & ", '" & FixDouble(drSubstitutionPenampung("customdbl1")) & "', '" & FixDouble(drSubstitutionPenampung("customdbl2")) & "', '" & FixDouble(drSubstitutionPenampung("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drSubstitutionPenampung("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drSubstitutionPenampung("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drSubstitutionPenampung("customdate3"))) & "')")
                                    Next
                                Else
                                    result(2) = "Main Transaction POS Substitution Item data not found." : Trans.Rollback() : GoTo selesai
                                End If
                            Next
                            'INSERT KE TABEL POS BONUS DETAIL
                            sql = "Insert into M_12_Pos_Substitution_Item_Detail(idsi, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValueSubstitutionItemDetail.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                        Else 'JIKA SEMUA KATEGORI

                            Dim dtPosItem As New DataTable 'variabel untuk cari data barang pos
                            'CARI DATA KATEGORI POS
                            dtKatPOS = AsDataTableAmbilDariDB("select * from m_12_pos_category")
                            If dtKatPOS.Rows.Count > 0 Then 'JIKA DATA KATEGORI POS ADA, AMBIL DATA BARANG POS
                                For Each drKatPos As DataRow In dtKatPOS.Rows
                                    For Each drdtl As DataRow In dtdtl.Rows
                                        'AMBIL DATA BARANG POS
                                        dtPosItem = AsDataTableAmbilDariDB("select * from M_12_Pos_Item where pikategori = '" & drKatPos("pckode") & "' AND piidbarang = '" & drdtl("idbarang") & "' order by pikategori asc")
                                        If dtPosItem.Rows.Count > 0 Then
                                            For Each drPosItem As DataRow In dtPosItem.Rows
                                                'persiapan insert ke tabel m_12_pos_substitution_item 
                                                strValueInsertSubstitutionItem.Append(IIf(Len(strValueInsertSubstitutionItem.ToString) = 0, "", ", "))
                                                strValueInsertSubstitutionItem.Append("('" & FixQuotes(drKatPos("pckode")) & "', '" & FixQuotes(drdtl("idbarang")) & "', '" & FixQuotes(drdtl("operator")) & "', '" & FixDouble(drdtl("jml1")) & "', '" & FixDouble(drdtl("jml2")) & "', '" & FixQuotes(drdtl("customtext1")) & "', '" & FixQuotes(drdtl("customtext2")) & "', '" & FixQuotes(drdtl("customtext3")) & "', '" & FixQuotes(drdtl("customtext4")) & "', '" & FixQuotes(drdtl("customtext5")) & "', " & drdtl("customint1") & ", " & drdtl("customint2") & ", " & drdtl("customint3") & ", '" & FixDouble(drdtl("customdbl1")) & "', '" & FixDouble(drdtl("customdbl2")) & "', '" & FixDouble(drdtl("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl2"))) & "', '" & FixQuotes(drdtl("nopromo")) & "')")

                                            Next
                                        End If

                                    Next
                                Next
                            End If

                            'insert ke tabel m_12_pos_substitution_item
                            sql = "Insert into M_12_Pos_Substitution_Item (sikategori, siidbarang, sioperator, sijml1, sijml2, sicustomtext1, sicustomtext2, sicustomtext3, sicustomtext4, sicustomtext5, sicustomint1, sicustomint2, sicustomint3, sicustomdbl1, sicustomdbl2, sicustomdbl3, sicustomdate1, sicustomdate2, sicustomdate3, sitgl1, sitgl2, sinopromo) values " & strValueInsertSubstitutionItem.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                            If dtKatPOS.Rows.Count > 0 Then
                                For Each drKatPos As DataRow In dtKatPOS.Rows
                                    For Each drdtl2 As DataRow In dtdtl.Rows
                                        'ambil id ketika simpan
                                        'AMBIL DATA BARANG POS
                                        dtPosItem = AsDataTableAmbilDariDB("select * from M_12_Pos_Item where pikategori = '" & drKatPos("pckode") & "' AND piidbarang = '" & drdtl2("idbarang") & "' order by pikategori asc")
                                        If dtPosItem.Rows.Count > 0 Then
                                            For Each drPosItem As DataRow In dtPosItem.Rows
                                                dtselectId = AsDataTableAmbilDariDB("select siid from M_12_Pos_Substitution_Item where sikategori = '" & drKatPos("pckode") & "' AND siidbarang = '" & drdtl2("idbarang") & "' AND sioperator = '" & drdtl2("operator") & "' AND sijml1 = '" & drdtl2("jml1") & "' AND sijml2 = '" & drdtl2("jml2") & "' limit 1")
                                                If dtselectId.Rows.Count > 0 Then idpossubstitutionitem = dtselectId.Rows(0)(0) Else result(2) = "Main POS Substitution Item transaction data not found." : Trans.Rollback() : GoTo selesai
                                                'filter data substitution penampung, untuk dijadikan parameter simpan ke tabel pos substitution detail
                                                dtSubstitutionPenampung = AsDataTableFilterSortDt(dtbisubstitution, "idsbidetail = '" & drdtl2("idsbidetail") & "'")
                                                If dtSubstitutionPenampung.Rows.Count > 0 Then
                                                    For Each drSubstitutionPenampung As DataRow In dtSubstitutionPenampung.Rows
                                                        'persiapan insert ke tabel m_12_pos_substitution_item_DETAIL
                                                        strValueSubstitutionItemDetail.Append(IIf(Len(strValueSubstitutionItemDetail.ToString) = 0, "", ", "))
                                                        strValueSubstitutionItemDetail.Append("(" & idpossubstitutionitem & ", '" & FixQuotes(drSubstitutionPenampung("idbarang")) & "', '" & FixDouble(drSubstitutionPenampung("jml")) & "', '" & FixQuotes(drSubstitutionPenampung("satuan")) & "', '" & FixQuotes(drSubstitutionPenampung("customtext1")) & "', '" & FixQuotes(drSubstitutionPenampung("customtext2")) & "', '" & FixQuotes(drSubstitutionPenampung("customtext3")) & "', '" & FixQuotes(drSubstitutionPenampung("customtext4")) & "', '" & FixQuotes(drSubstitutionPenampung("customtext5")) & "', " & drSubstitutionPenampung("customint1") & ", " & drSubstitutionPenampung("customint2") & ", " & drSubstitutionPenampung("customint3") & ", '" & FixDouble(drSubstitutionPenampung("customdbl1")) & "', '" & FixDouble(drSubstitutionPenampung("customdbl2")) & "', '" & FixDouble(drSubstitutionPenampung("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drSubstitutionPenampung("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drSubstitutionPenampung("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drSubstitutionPenampung("customdate3"))) & "')")

                                                    Next
                                                Else
                                                    result(2) = "Main Transaction POS Substitution Item data not found." : Trans.Rollback() : GoTo selesai
                                                End If
                                            Next
                                        End If
                                    Next
                                Next

                                'INSERT KE TABEL POS SUBSTITUTION DETAIL
                                sql = "Insert into M_12_Pos_Substitution_Item_Detail(idsi, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValueSubstitutionItemDetail.ToString & ""
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


                    Else
                        result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai
                    End If
                End If

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
        Con1.Close()
        Con1 = Nothing
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
    Public Function M12_SbiUpdateStatusOld(ByVal param As String) As String
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
            Filter = Filter.Replace("sbikontakkode", "c.kkode")
            Filter = Filter.Replace("sbikontaknama", "c.knama")
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
            Dim sumber As String = "SBI", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Sbitgl, Sbinotransaksi, Sbistatus FROM m_12_Sbi WHERE Sbiid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Sbistatussebelumnya" : jnsaktivitas = 17
                'CEK STATUS TRANSAKSI, JIKA <> 7 MAKA TIDAK BISA UNCLOSE
                If statusTransaksi <> 7 Then result(2) = "Transaction has not closed, it can't be unclose." : Trans.Rollback() : GoTo selesai
            Else
                jnsaktivitas = nilaiStatus
            End If

            'SET ISDELETE = TRUE JIKA STATUS TRANSAKSI = 2/3/4/7 DAN JNS AKTIVITAS <> 7(CLOSE) & 17(UNCLOSE)
            If ((statusTransaksi = 2 Or statusTransaksi = 3 Or statusTransaksi = 4 Or statusTransaksi = 7) And jnsaktivitas <> 7 And jnsaktivitas <> 17) Then isDelete = True

            'CEK PERIODE AKUNTANSI ==============================================================
            'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglTransaksi), AsFormatTanggal(tglTransaksi))
            'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
            'END OF CEK PERIODE AKUNTANSI =======================================================


            'SIMPAN HISTORY ========================
            'Dim SimpanHistory As New m_12_Bi_history
            'Dim rsSimpanHistory As String = SimpanHistory.m12_Bi_HistorySimpan("" & paramSplit(0) & "★M12_Bi_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            'If (rsSplitResult(1) = 0) Then
            '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            'End If
            'END OF SIMPAN HISTORY ==================


            If isDelete Then
                'AMBIL JENIS KATEGORI UTAMA
                Dim dtutama As New DataTable
                dtutama = AsDataTableAmbilDariDB("SELECT * FROM M_12_Sbi WHERE sbiid=" & idtransaksi)
                If (dtutama.Rows.Count > 0) Then
                    For Each drutama As DataRow In dtutama.Rows

                        'AMBIL DATA DETAIL
                        dtdetail = AsDataTableAmbilDariDB("SELECT * FROM M_12_Sbi_Detail WHERE idsbi=" & idtransaksi)
                        If (dtdetail.Rows.Count > 0) Then
                            For Each drdetail As DataRow In dtdetail.Rows
                                Dim dtsubstitution As New DataTable
                                If drutama("sbijeniskategori") = 1 Then 'JIKA PER KATEGORI
                                    Dim query As String = "SELECT siid FROM m_12_pos_substitution_item WHERE sikategori='" & drdetail("sbikategori") & "' AND sinopromo = '" & drdetail("nopromo") & "'"
                                    dtsubstitution = AsDataTableAmbilDariDB(query)
                                    If dtsubstitution.Rows.Count > 0 Then
                                        For Each drsubstitution As DataRow In dtsubstitution.Rows
                                            'hapus data detail
                                            Dim strValue2 As New StringBuilder
                                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", " AND "))
                                            strValue2.Append("siid = '" & FixQuotes(drsubstitution("siid")) & "' ")
                                            sql = "Delete from M_12_pos_substitution_item WHERE " & strValue2.ToString
                                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                            With objCmd
                                                .Connection = Con1
                                                .Transaction = Trans
                                                .CommandType = CommandType.Text
                                                .CommandText = sql
                                            End With
                                            objCmd.ExecuteNonQuery()


                                            Dim strValueItemDetail As New StringBuilder
                                            strValueItemDetail.Append(IIf(Len(strValueItemDetail.ToString) = 0, "", " AND "))
                                            strValueItemDetail.Append("idsi = '" & FixQuotes(drsubstitution("siid")) & "'")

                                            'hapus data detail
                                            sql = "Delete from M_12_pos_substitution_item_Detail WHERE " & strValueItemDetail.ToString
                                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                            With objCmd
                                                .Connection = Con1
                                                .Transaction = Trans
                                                .CommandType = CommandType.Text
                                                .CommandText = sql
                                            End With
                                            objCmd.ExecuteNonQuery()
                                        Next
                                    End If
                                Else 'JIKA SEMUA KATEGORI
                                    Dim query As String = "SELECT siid FROM m_12_pos_substitution_item WHERE sinopromo = '" & drdetail("nopromo") & "'"
                                    dtsubstitution = AsDataTableAmbilDariDB(query)
                                    If dtsubstitution.Rows.Count > 0 Then
                                        For Each drsubstitution As DataRow In dtsubstitution.Rows
                                            'hapus data detail
                                            Dim strValue2 As New StringBuilder
                                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", " OR "))
                                            strValue2.Append("siid = '" & FixQuotes(drsubstitution("siid")) & "' ")
                                            sql = "Delete from M_12_pos_substitution_item WHERE " & strValue2.ToString
                                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                            With objCmd
                                                .Connection = Con1
                                                .Transaction = Trans
                                                .CommandType = CommandType.Text
                                                .CommandText = sql
                                            End With
                                            objCmd.ExecuteNonQuery()


                                            Dim strValueItemDetail As New StringBuilder
                                            strValueItemDetail.Append(IIf(Len(strValueItemDetail.ToString) = 0, "", " OR "))
                                            strValueItemDetail.Append("idsi = '" & FixQuotes(drsubstitution("siid")) & "'")

                                            'hapus data detail
                                            sql = "Delete from M_12_pos_substitution_item_Detail WHERE " & strValueItemDetail.ToString
                                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                            With objCmd
                                                .Connection = Con1
                                                .Transaction = Trans
                                                .CommandType = CommandType.Text
                                                .CommandText = sql
                                            End With
                                            objCmd.ExecuteNonQuery()
                                        Next
                                    End If
                                End If

                            Next


                            'hapus data detail
                            'sql = "Delete from M_12_Bi_Detail WHERE idbidetail=" & idtransaksi
                            'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            'With objCmd
                            '    .Connection = Con1
                            '    .Transaction = Trans
                            '    .CommandType = CommandType.Text
                            '    .CommandText = sql
                            'End With
                            'objCmd.ExecuteNonQuery()

                            ''jika status unclose maka nilai status ambil dari status sebelumnya
                            'If (nilaiStatus = "unclose") Then
                            '    Dim dtstatusbefore As DataTable
                            '    dtstatusbefore = AsDataTableAmbilDariDB("SELECT Bistatussebelumnya FROM M_12_Bi WHERE biid=" & idtransaksi)
                            '    nilaiStatus = Val(dtstatusbefore.Rows(0)(0))
                            'End If

                        End If
                    Next
                End If

            End If

            'update status utama
            sql = "UPDATE M_12_Sbi SET Sbistatus = " & nilaiStatus & ", Sbimodifikasiuser='" & userid & "', Sbimodifikasitgl = NOW(), Sbiposting = 0, Sbipostingtgl = '1971-01-01 00:00:00', Sbijmlrevisi = Sbijmlrevisi + 1 WHERE Sbiid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_SbiSearch(PostWsSearch(paramSplit(0), "M12_SbiSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_SbiDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("sbikontakkode", "c.kkode")
            Filter = Filter.Replace("sbikontaknama", "c.knama")
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
            Dim sumber As String = "SBI", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT sbiid, sbinotransaksi FROM m_12_sbi WHERE sbiid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT sbicabang, sbilokasi, sbisumber, sbiautonotransaksi, sbinotransaksi, sbitgl"
            sql &= " FROM M_12_sbi"
            sql &= " WHERE sbiid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("sbicabang")
                lokasi = dtNomorNext.Rows(0)("sbilokasi")
                sumber = dtNomorNext.Rows(0)("sbisumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("sbiautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("sbinotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("sbitgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M_12_sbi_Detail WHERE idsbi = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M_12_sbi WHERE sbiid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_SbiSearch(PostWsSearch(paramSplit(0), "M12_SbiSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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

    <WebMethod()>
    Public Function M12_SbiGetdataById(ByVal param As String) As String

        'M12_SbiGetdataById Utama --------------------------------------------------------
        'sbiid, sbicabang, sbilokasi, sbisumber, sbikategoripos, sbiautonotransaksi, sbinotransaksi, 
        'sbitgl, sbikodepa, sbikontak, sbikontakperson, sbiuraian, sbicatatan, sbistatus, 
        'sbistatussebelumnya, sbijmlrevisi, sbicetakanke, sbiisclose, sbiinputuser, sbiinputtgl, sbimodifikasiuser, 
        'sbimodifikasitgl, sbiposting, sbipostingtgl, sbicustomtext1, sbicustomtext2, sbicustomtext3, sbicustomtext4, 
        'sbicustomtext5, sbicustomint1, sbicustomint2, sbicustomint3, sbicustomdbl1, sbicustomdbl2, sbicustomdbl3, 
        'sbicustomdate1, sbicustomdate2, sbicustomdate3, sbicabangnama, sbilokasinama, sbikontakkode, 
        'sbikontaknama, sbistatusnama, sbistatussebelumnyanama, sbiinputusernama, sbimodifikasiusernama, sbikategoriposnama, sbijeniskategori

        'M12_BiGetdataById Detail -------------------------------------------------------
        'idsbidetail, sbikategori, idbarang, operator, jml1, jml2, customtext1, 
        'customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, tgl1, 
        'tgl2, nopromo, kodebarang, namabarang, catatan, urutan

        'M12_BiGetdataById Substitution -------------------------------------------------------
        'idsubstitution, idsbidetail, idbarang, jml, satuan, customtext1, customtext2, 
        'customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, namabarang, urutan



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

        Dim utama As String = "", detail As String = "", substitution As String = "", idtransaksi As String = ""

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
            Filter = "sbiid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "sbiid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m12_sbi_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("sbiid"), 0), sptField,
                     FxDB(drutama("sbicabang"), ""), sptField,
                     FxDB(drutama("sbilokasi"), ""), sptField,
                     FxDB(drutama("sbisumber"), ""), sptField,
                     FxDB(drutama("sbikategoripos"), ""), sptField,
                     FxDB(drutama("sbiautonotransaksi"), 0), sptField,
                     FxDB(drutama("sbinotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sbitgl"), ""), formatTgl), sptField,
                     FxDB(drutama("sbikodepa"), ""), sptField,
                     FxDB(drutama("sbikontak"), ""), sptField,
                     FxDB(drutama("sbikontakperson"), ""), sptField,
                     FxDB(drutama("sbiuraian"), ""), sptField,
                     FxDB(drutama("sbicatatan"), ""), sptField,
                     FxDB(drutama("sbistatus"), 0), sptField,
                     FxDB(drutama("sbistatussebelumnya"), 0), sptField,
                     FxDB(drutama("sbijmlrevisi"), 0), sptField,
                     FxDB(drutama("sbicetakanke"), 0), sptField,
                     FxDB(drutama("sbiisclose"), 0), sptField,
                     FxDB(drutama("sbiinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sbiinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sbimodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sbimodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sbiposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sbipostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sbicustomtext1"), ""), sptField,
                     FxDB(drutama("sbicustomtext2"), ""), sptField,
                     FxDB(drutama("sbicustomtext3"), ""), sptField,
                     FxDB(drutama("sbicustomtext4"), ""), sptField,
                     FxDB(drutama("sbicustomtext5"), ""), sptField,
                     FxDB(drutama("sbicustomint1"), 0), sptField,
                     FxDB(drutama("sbicustomint2"), 0), sptField,
                     FxDB(drutama("sbicustomint3"), 0), sptField,
                     FxDB(drutama("sbicustomdbl1"), 0), sptField,
                     FxDB(drutama("sbicustomdbl2"), 0), sptField,
                     FxDB(drutama("sbicustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sbicustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sbicustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sbicustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("sbicabangnama"), ""), sptField,
                     FxDB(drutama("sbilokasinama"), ""), sptField,
                     FxDB(drutama("sbikontakkode"), ""), sptField,
                     FxDB(drutama("sbikontaknama"), ""), sptField,
                     FxDB(drutama("sbistatusnama"), ""), sptField,
                     FxDB(drutama("sbistatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("sbiinputusernama"), ""), sptField,
                     FxDB(drutama("sbimodifikasiusernama"), ""), sptField,
                     FxDB(drutama("sbikategoriposnama"), ""), sptField,
                     FxDB(drutama("sbijeniskategori"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idsbidetail"), 0), sptField,
                     FxDB(dr("idsbi"), 0), sptField,
                     FxDB(dr("sbikategori"), ""), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("operator"), 0), sptField,
                     FxDB(dr("jml1"), 0), sptField,
                     FxDB(dr("jml2"), 0), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customtext4"), ""), sptField,
                     FxDB(dr("customtext5"), ""), sptField,
                     FxDB(dr("customint1"), 0), sptField,
                     FxDB(dr("customint2"), 0), sptField,
                     FxDB(dr("customint3"), 0), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("tgl1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("tgl2"), ""), formatTgl), sptField,
                     FxDB(dr("nopromo"), ""), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA PAY
            'PANGGIL QUERY
            Dim querysubstitution As New m0_query
            sql = "select `sbib`.`idsubstitution` AS `idsubstitution`, `sbib`.`idsbidetail` AS `idsbidetail`,`sbib`.`idsbi` AS `idsbi`,`sbib`.`idbarang` AS `idbarang`,`sbib`.`jml` AS `jml`,`sbib`.`satuan` AS `satuan`,`sbib`.`customtext1` AS `customtext1`,`sbib`.`customtext2` AS `customtext2`,`sbib`.`customtext3` AS `customtext3`,`sbib`.`customtext4` AS `customtext4`,`sbib`.`customtext5` AS `customtext5`,`sbib`.`customint1` AS `customint1`,`sbib`.`customint2` AS `customint2`,`sbib`.`customint3` AS `customint3`,`sbib`.`customdbl1` AS `customdbl1`,`sbib`.`customdbl2` AS `customdbl2`,`sbib`.`customdbl3` AS `customdbl3`,`sbib`.`customdate1` AS `customdate1`,`sbib`.`customdate2` AS `customdate2`,`sbib`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`i`.`bnama` AS `namabarang`,`sbib`.`urutan` AS `urutan` FROM `m_12_sbi_substitution` `sbib` JOIN m1_item `i` ON (`sbib`.`idbarang` = `i`.bid) WHERE `sbib`.idsbi='" & idtransaksi & "' ORDER BY `sbib`.`urutan` ASC"
            Dim dtsubstitution As New DataTable
            dtsubstitution = AmbilData("aplikasi1-M_12_sbi_Substitution", "", "", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each drsubstitution As DataRow In dtsubstitution.Rows
                substitution = String.Concat(substitution,
                     FxDB(drsubstitution("idsubstitution"), 0), sptField,
                     FxDB(drsubstitution("idsbidetail"), 0), sptField,
                     FxDB(drsubstitution("idsbi"), 0), sptField,
                     FxDB(drsubstitution("idbarang"), 0), sptField,
                     FxDB(drsubstitution("jml"), 0), sptField,
                     FxDB(drsubstitution("satuan"), ""), sptField,
                     FxDB(drsubstitution("customtext1"), ""), sptField,
                     FxDB(drsubstitution("customtext2"), ""), sptField,
                     FxDB(drsubstitution("customtext3"), ""), sptField,
                     FxDB(drsubstitution("customtext4"), ""), sptField,
                     FxDB(drsubstitution("customtext5"), ""), sptField,
                     FxDB(drsubstitution("customint1"), 0), sptField,
                     FxDB(drsubstitution("customint2"), 0), sptField,
                     FxDB(drsubstitution("customint3"), 0), sptField,
                     FxDB(drsubstitution("customdbl1"), 0), sptField,
                     FxDB(drsubstitution("customdbl2"), 0), sptField,
                     FxDB(drsubstitution("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drsubstitution("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drsubstitution("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drsubstitution("customdate3"), ""), formatTgl), sptField,
                     FxDB(drsubstitution("kodebarang"), 0), sptField,
                     FxDB(drsubstitution("namabarang"), 0), sptField,
                     FxDB(drsubstitution("urutan"), 0), sptRow)
            Next
            substitution = substitution.Substring(0, substitution.Length - sptRow.Length)

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, substitution)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sbiid, sbicabang, sbilokasi, sbisumber, sbikategoripos, sbiautonotransaksi, sbinotransaksi, sbitgl, sbikodepa, sbikontak, sbikontakperson, sbiuraian, sbicatatan, sbistatus, sbistatussebelumnya, sbijmlrevisi, sbicetakanke, sbiisclose, sbiinputuser, sbiinputtgl, sbimodifikasiuser, sbimodifikasitgl, sbiposting, sbipostingtgl, sbicustomtext1, sbicustomtext2, sbicustomtext3, sbicustomtext4, sbicustomtext5, sbicustomint1, sbicustomint2, sbicustomint3, sbicustomdbl1, sbicustomdbl2, sbicustomdbl3, sbicustomdate1, sbicustomdate2, sbicustomdate3, sbicabangnama, sbilokasinama, sbikontakkode, sbikontaknama, sbistatusnama, sbistatussebelumnyanama, sbiinputusernama, sbimodifikasiusernama, sbikategoriposnama, sbijeniskategori" & sptSubParam & "idsbidetail, idsbi, sbikategori, idbarang, operator, jml1, jml2, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, tgl1, tgl2, nopromo, kodebarang, namabarang, catatan, urutan" & sptSubParam & "idsubstitution, idsbidetail, idsbi, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, namabarang, urutan"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_SbiSearch(ByVal param As String) As String
        'M12_SbiSearch --------------------------------------------------------
        'sbiid, sbicabang, sbilokasi, sbisumber, sbikategoripos, sbiautonotransaksi, sbinotransaksi, 
        'sbitgl, sbikodepa, sbikontak, sbikontakperson, sbiuraian, sbicatatan, sbistatus, 
        'sbistatussebelumnya, sbijmlrevisi, sbicetakanke, sbiisclose, sbiinputuser, sbiinputtgl, sbimodifikasiuser, 
        'sbimodifikasitgl, sbiposting, sbipostingtgl, sbicustomtext1, sbicustomtext2, sbicustomtext3, sbicustomtext4, 
        'sbicustomtext5, sbicustomint1, sbicustomint2, sbicustomint3, sbicustomdbl1, sbicustomdbl2, sbicustomdbl3, 
        'sbicustomdate1, sbicustomdate2, sbicustomdate3, sbicabangnama, sbilokasinama, sbikontakkode, 
        'sbikontaknama, sbistatusnama, sbistatussebelumnyanama, sbiinputusernama, sbimodifikasiusernama

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
            formatTglWaktu = "yyy-MM-dd Hh:mm:ss"
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
        sql = "select `sbi`.`sbiid` AS `sbiid`,`sbi`.`sbicabang` AS `sbicabang`,`sbi`.`sbilokasi` AS `sbilokasi`,`sbi`.`sbisumber` AS `sbisumber`,`sbi`.`sbiautonotransaksi` AS `sbiautonotransaksi`,`sbi`.`sbinotransaksi` AS `sbinotransaksi`,`sbi`.`sbitgl` AS `sbitgl`,`sbi`.`sbikodepa` AS `sbikodepa`,`sbi`.`sbikontak` AS `sbikontak`,`sbi`.`sbikontakperson` AS `sbikontakperson`,`sbi`.`sbikategoripos` AS `sbikategoripos`,`sbi`.`sbiuraian` AS `sbiuraian`,`sbi`.`sbicatatan` AS `sbicatatan`,`sbi`.`sbistatus` AS `sbistatus`,`sbi`.`sbistatussebelumnya` AS `sbistatussebelumnya`,`sbi`.`sbijmlrevisi` AS `sbijmlrevisi`,`sbi`.`sbicetakanke` AS `sbicetakanke`,`sbi`.`sbiisclose` AS `sbiisclose`,`sbi`.`sbiinputuser` AS `sbiinputuser`,`sbi`.`sbiinputtgl` AS `sbiinputtgl`,`sbi`.`sbimodifikasiuser` AS `sbimodifikasiuser`,`sbi`.`sbimodifikasitgl` AS `sbimodifikasitgl`,`sbi`.`sbiposting` AS `sbiposting`,`sbi`.`sbipostingtgl` AS `sbipostingtgl`,`sbi`.`sbicustomtext1` AS `sbicustomtext1`,`sbi`.`sbicustomtext2` AS `sbicustomtext2`,`sbi`.`sbicustomtext3` AS `sbicustomtext3`,`sbi`.`sbicustomtext4` AS `sbicustomtext4`,`sbi`.`sbicustomtext5` AS `sbicustomtext5`,`sbi`.`sbicustomint1` AS `sbicustomint1`,`sbi`.`sbicustomint2` AS `sbicustomint2`,`sbi`.`sbicustomint3` AS `sbicustomint3`,`sbi`.`sbicustomdbl1` AS `sbicustomdbl1`,`sbi`.`sbicustomdbl2` AS `sbicustomdbl2`,`sbi`.`sbicustomdbl3` AS `sbicustomdbl3`,`sbi`.`sbicustomdate1` AS `sbicustomdate1`,`sbi`.`sbicustomdate2` AS `sbicustomdate2`,`sbi`.`sbicustomdate3` AS `sbicustomdate3`,`br`.`bnama` AS `sbicabangnama`,`lc`.`lnama` AS `sbilokasinama`,`c`.`kkode` AS `sbikontakkode`,`c`.`knama` AS `sbikontaknama`,`st1`.`nama` AS `sbistatusnama`,`st2`.`nama` AS `sbistatussebelumnyanama`,`u1`.`unama` AS `sbiinputusernama`,`u2`.`unama` AS `sbimodifikasiusernama` from (((((((`m_12_sbi` `sbi` left join `m1_branch` `br` on((`sbi`.`sbicabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`sbi`.`sbilokasi` = `lc`.`lkode`))) left join `m1_contact` `c` on((`sbi`.`sbikontak` = `c`.`kid`))) left join `m0_status` `st1` on((`sbi`.`sbistatus` = `st1`.`kode`))) left join `m0_status` `st2` on((`sbi`.`sbistatussebelumnya` = `st2`.`kode`))) left join `m0_user` `u1` on((`sbi`.`sbiinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`sbi`.`sbimodifikasiuser` = `u2`.`userid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Cr~M2_Cr_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("sbiid"), 0), sptField,
                             FxDB(dr("sbicabang"), ""), sptField,
                             FxDB(dr("sbilokasi"), ""), sptField,
                             FxDB(dr("sbisumber"), ""), sptField,
                             FxDB(dr("sbikategoripos"), ""), sptField,
                             FxDB(dr("sbiautonotransaksi"), 0), sptField,
                             FxDB(dr("sbinotransaksi"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("sbitgl"), ""), formatTgl), sptField,
                             FxDB(dr("sbikodepa"), ""), sptField,
                             FxDB(dr("sbikontak"), ""), sptField,
                             FxDB(dr("sbikontakperson"), ""), sptField,
                             FxDB(dr("sbiuraian"), ""), sptField,
                             FxDB(dr("sbicatatan"), ""), sptField,
                             FxDB(dr("sbistatus"), 0), sptField,
                             FxDB(dr("sbistatussebelumnya"), 0), sptField,
                             FxDB(dr("sbijmlrevisi"), 0), sptField,
                             FxDB(dr("sbicetakanke"), 0), sptField,
                             FxDB(dr("sbiisclose"), 0), sptField,
                             FxDB(dr("sbiinputuser"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("sbiinputtgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("sbimodifikasiuser"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("sbimodifikasitgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("sbiposting"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("sbipostingtgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("sbicustomtext1"), ""), sptField,
                             FxDB(dr("sbicustomtext2"), ""), sptField,
                             FxDB(dr("sbicustomtext3"), ""), sptField,
                             FxDB(dr("sbicustomtext4"), ""), sptField,
                             FxDB(dr("sbicustomtext5"), ""), sptField,
                             FxDB(dr("sbicustomint1"), 0), sptField,
                             FxDB(dr("sbicustomint2"), 0), sptField,
                             FxDB(dr("sbicustomint3"), 0), sptField,
                             FxDB(dr("sbicustomdbl1"), 0), sptField,
                             FxDB(dr("sbicustomdbl2"), 0), sptField,
                             FxDB(dr("sbicustomdbl3"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("sbicustomdate1"), ""), formatTgl), sptField,
                             AsFormatTanggal(FxDB(dr("sbicustomdate2"), ""), formatTgl), sptField,
                             AsFormatTanggal(FxDB(dr("sbicustomdate3"), ""), formatTgl), sptField,
                             FxDB(dr("sbicabangnama"), ""), sptField,
                             FxDB(dr("sbilokasinama"), ""), sptField,
                             FxDB(dr("sbikontakkode"), ""), sptField,
                             FxDB(dr("sbikontaknama"), ""), sptField,
                             FxDB(dr("sbistatusnama"), ""), sptField,
                             FxDB(dr("sbistatussebelumnyanama"), ""), sptField,
                             FxDB(dr("sbiinputusernama"), ""), sptField,
                             FxDB(dr("sbimodifikasiusernama"), ""), sptRow)

            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = pg1.isPaging
            resultPaging(1) = pg1.isNext
            resultPaging(2) = pg1.isPrev
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sbiid, sbicabang, sbilokasi, sbisumber, sbikategoripos, sbiautonotransaksi, sbinotransaksi, sbitgl, sbikodepa, sbikontak, sbikontakperson, sbiuraian, sbicatatan, sbistatus, sbistatussebelumnya, sbijmlrevisi, sbicetakanke, sbiisclose, sbiinputuser, sbiinputtgl, sbimodifikasiuser, sbimodifikasitgl, sbiposting, sbipostingtgl, sbicustomtext1, sbicustomtext2, sbicustomtext3, sbicustomtext4, sbicustomtext5, sbicustomint1, sbicustomint2, sbicustomint3, sbicustomdbl1, sbicustomdbl2, sbicustomdbl3, sbicustomdate1, sbicustomdate2, sbicustomdate3, sbicabangnama, sbilokasinama, sbikontakkode, sbikontaknama, sbistatusnama, sbistatussebelumnyanama, sbiinputusernama, sbimodifikasiusernama"))

        Return wsResult
    End Function

End Class
