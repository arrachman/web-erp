Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_st
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_StSimpan(ByVal param As String) As String
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
        'stid(0) As Integer, stcabang(1) As String, stlokasi(2) As String, stsumber(3) As String, stkategoripos(4) As String, 
        'stautonotransaksi(5) As Integer, stnotransaksi(6) As String, sttgl(7) As Date, stkodepa(8) As , stkontak(9) As , 
        'stkontakperson(10) As String, sturaian(11) As String, stcatatan(12) As String, ststatus(13) As Integer, ststatussebelumnya(14) As Integer, 
        'stjmlrevisi(15) As Integer, stcetakanke(16) As Integer, stisclose(17) As Integer, stinputuser(18) As , stinputtgl(19) As DateTime, 
        'stmodifikasiuser(20) As , stmodifikasiuser(21) As DateTime, stposting(22) As Integer, stpostingtgl(23) As DateTime, stcustomtext1(24) As String, 
        'stcustomtext2(25) As String, stcustomtext3(26) As String, stcustomtext4(27) As String, stcustomtext5(28) As String, stcustomint1(29) As Integer, 
        'stcustomint2(30) As Integer, stcustomint3(31) As Integer, stcustomdbl1(32) As Double, stcustomdbl2(33) As Double, stcustomdbl3(34) As Double, 
        'stcustomdate1(35) As Date, stcustomdate2(36) As Date, stcustomdate3(37) As Date, stjenispoint(38) As Int

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'stid, stcabang, stlokasi, stsumber, stkategoripos, stautonotransaksi, stnotransaksi, 
        'sttgl, stkodepa, stkontak, stkontakperson, sturaian, stcatatan, ststatus, 
        'ststatussebelumnya, stjmlrevisi, stcetakanke, stisclose, stinputuser, stinputtgl, stmodifikasiuser, 
        'stmodifikasiuser, stposting, stpostingtgl, stcustomtext1, stcustomtext2, stcustomtext3, stcustomtext4, 
        'stcustomtext5, stcustomint1, stcustomint2, stcustomint3, stcustomdbl1, stcustomdbl2, stcustomdbl3, 
        'stcustomdate1, stcustomdate2, stcustomdate3, stjenispoint

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 39) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'stid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "stid required numeric." : GoTo selesai
        End If
        'stautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "stautonotransaksi required numeric." : GoTo selesai
        End If
        'sttgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "sttgl required date." : GoTo selesai
        End If
        'ststatus(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "ststatus required numeric." : GoTo selesai
        End If
        'ststatussebelumnya(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "ststatussebelumnya required numeric." : GoTo selesai
        End If
        'stjmlrevisi(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "stjmlrevisi required numeric." : GoTo selesai
        End If
        'stcetakanke(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "stcetakanke required numeric." : GoTo selesai
        End If
        'stisclose(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "stisclose required numeric." : GoTo selesai
        End If
        'stinputtgl(19) As DateTime
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "stinputtgl required date." : GoTo selesai
        End If
        'stmodifikasiuser(21) As DateTime
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "stmodifikasiuser required date." : GoTo selesai
        End If
        'stposting(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "stposting required numeric." : GoTo selesai
        End If
        'stpostingtgl(23) As DateTime
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "stpostingtgl required date." : GoTo selesai
        End If
        'stcustomint1(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "stcustomint1 required numeric." : GoTo selesai
        End If
        'stcustomint2(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "stcustomint2 required numeric." : GoTo selesai
        End If
        'stcustomint3(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "stcustomint3 required numeric." : GoTo selesai
        End If
        'stcustomdbl1(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "stcustomdbl1 required numeric." : GoTo selesai
        End If
        'stcustomdbl2(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "stcustomdbl2 required numeric." : GoTo selesai
        End If
        'stcustomdbl3(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "stcustomdbl3 required numeric." : GoTo selesai
        End If
        'stcustomdate1(35) As Date
        If (IsDate(dataUtama(35)) = False) Then
            result(2) = "stcustomdate1 required date." : GoTo selesai
        End If
        'stcustomdate2(36) As Date
        If (IsDate(dataUtama(36)) = False) Then
            result(2) = "stcustomdate2 required date." : GoTo selesai
        End If
        'stcustomdate3(37) As Date
        If (IsDate(dataUtama(37)) = False) Then
            result(2) = "stcustomdate3 required date." : GoTo selesai
        End If

        'stjenispoint(38) As Date
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "stjenispoint required numeric." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'stcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "stcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "stcabang should not be more than 25 character." : GoTo selesai
        End If

        'stlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "stlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "stlokasi should not be more than 25 character." : GoTo selesai
        End If

        'stsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "stsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "stsumber should not be more than 10 character." : GoTo selesai
        End If

        'stkategoripos(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "stkategoripos can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 50 Then
            result(2) = "stkategoripos should not be more than 50 character." : GoTo selesai
        End If

        'stnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "stnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "stnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'sttgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "sttgl can't be empty" : GoTo selesai
        End If

        'stkodepa(8) As 
        If Len(dataUtama(8)) = 0 Then
            result(2) = "stkodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 20 Then
            result(2) = "stkodepa should not be more than 20 character." : GoTo selesai
        End If

        'stkontak(9) As 
        If Len(dataUtama(9)) = 0 Then
            result(2) = "stkontak can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 20 Then
            result(2) = "stkontak should not be more than 20 character." : GoTo selesai
        End If

        'stinputtgl(19) As DateTime
        If Len(dataUtama(19)) = 0 Then
            result(2) = "stinputtgl can't be empty" : GoTo selesai
        End If

        'stmodifikasiuser(21) As DateTime
        If Len(dataUtama(21)) = 0 Then
            result(2) = "stmodifikasiuser can't be empty" : GoTo selesai
        End If

        'stpostingtgl(23) As DateTime
        If Len(dataUtama(23)) = 0 Then
            result(2) = "stpostingtgl can't be empty" : GoTo selesai
        End If

        'stcustomdbl1(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "stcustomdbl1 can't be empty" : GoTo selesai
        End If

        'stcustomdbl2(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "stcustomdbl2 can't be empty" : GoTo selesai
        End If

        'stcustomdbl3(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "stcustomdbl3 can't be empty" : GoTo selesai
        End If

        'stcustomdate1(35) As Date
        If Len(dataUtama(35)) = 0 Then
            result(2) = "stcustomdate1 can't be empty" : GoTo selesai
        End If

        'stcustomdate2(36) As Date
        If Len(dataUtama(36)) = 0 Then
            result(2) = "stcustomdate2 can't be empty" : GoTo selesai
        End If

        'stcustomdate3(37) As Date
        If Len(dataUtama(37)) = 0 Then
            result(2) = "stcustomdate3 can't be empty" : GoTo selesai
        End If

        'stjenispoint(38) As Date
        If Len(dataUtama(38)) = 0 Then
            result(2) = "stjenispoint can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "stid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stkategoripos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sttgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sturaian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ststatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ststatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stpostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stjenispoint", AsEnumTypeData.AsInt64)

        If AsDataTableTambahData(dtutama, "stid~stcabang~stlokasi~stsumber~stkategoripos~stautonotransaksi~stnotransaksi~sttgl~stkodepa~stkontak~stkontakperson~sturaian~stcatatan~ststatus~ststatussebelumnya~stjmlrevisi~stcetakanke~stisclose~stinputuser~stinputtgl~stmodifikasiuser~stmodifikasitgl~stposting~stpostingtgl~stcustomtext1~stcustomtext2~stcustomtext3~stcustomtext4~stcustomtext5~stcustomint1~stcustomint2~stcustomint3~stcustomdbl1~stcustomdbl2~stcustomdbl3~stcustomdate1~stcustomdate2~stcustomdate3~stjenispoint", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38)) = False Then
            result(2) = "insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'iddidetail, iddi, dikategori, idbarang, operator, jml1, jml2, 
        'nilai, tgl1, tgl2, jam1, jam2, catatan, 
        'urutan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, 
        'customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3, nopromo

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idstdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idst", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "stkategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "operator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jam1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jam2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "nopromo", AsEnumTypeData.AsString)

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 29) Then
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

            'tgl1(8) As Date
            If (IsDate(dataRowDetail(8)) = False) Then
                result(2) = "tgl1 required date." : GoTo selesai
            End If
            'tgl2(9) As Date
            If (IsDate(dataRowDetail(9)) = False) Then
                result(2) = "tgl2 required date." : GoTo selesai
            End If
            'jam1(10) As Date
            If (IsDate(dataRowDetail(10)) = False) Then
                result(2) = "jam1 required date." : GoTo selesai
            End If
            'jam2(11) As Date
            If (IsDate(dataRowDetail(11)) = False) Then
                result(2) = "jam2 required date." : GoTo selesai
            End If
            'customint1(19) As Integer
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "customint1 required numeric." : GoTo selesai
            End If
            'customint2(20) As Integer
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "customint2 required numeric." : GoTo selesai
            End If
            'customint3(21) As Integer
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "customint3 required numeric." : GoTo selesai
            End If
            'customdbl1(22) As Double
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(23) As Double
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(24) As Double
            If (IsNumeric(dataRowDetail(24)) = False) Then
                result(2) = "customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(25) As Date
            If (IsDate(dataRowDetail(25)) = False) Then
                result(2) = "customdate1 required date." : GoTo selesai
            End If
            'customdate2(26) As Date
            If (IsDate(dataRowDetail(26)) = False) Then
                result(2) = "customdate2 required date." : GoTo selesai
            End If
            'customdate3(27) As Date
            If (IsDate(dataRowDetail(27)) = False) Then
                result(2) = "customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'iddidetail(0) As 
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - iddidetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - iddidetail should not be more than 20 character." : GoTo selesai
            End If

            'iddi(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - iddi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - iddi should not be more than 20 character." : GoTo selesai
            End If

            'dikategori(2) As String
            'If Len(dataRowDetail(2)) = 0 Then
            '    result(2) = "Row : " & i & " - dikategori can't be empty" : GoTo selesai
            'End If
            'If Len(dataRowDetail(2)) > 25 Then
            '    result(2) = "Row : " & i & " - dikategori should not be more than 25 character." : GoTo selesai
            'End If

            If dataUtama(38) = 1 Then 'JIKA JENIS POINT = ITEM BASED
                'idbarang(3) As 
                If Len(dataRowDetail(3)) = 0 Then
                    result(2) = "Row : " & i & " - idbarang can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(3)) > 20 Then
                    result(2) = "Row : " & i & " - idbarang should not be more than 20 character." : GoTo selesai
                End If
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

            'nilai(7) As String
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - nilai can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(7)) > 25 Then
                result(2) = "Row : " & i & " - nilai should not be more than 25 character." : GoTo selesai
            End If

            'tgl1(8) As Date
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - tgl1 can't be empty" : GoTo selesai
            End If

            'tgl2(9) As Date
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - tgl2 can't be empty" : GoTo selesai
            End If

            'jam1(10) As Date
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - jam1 can't be empty" : GoTo selesai
            End If

            'jam2(11) As Date
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - jam2 can't be empty" : GoTo selesai
            End If

            'urutan(13) As 
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - urutan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(14)) > 20 Then
                result(2) = "Row : " & i & " - urutan should not be more than 20 character." : GoTo selesai
            End If

            'customdbl1(22) As Double
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(23) As Double
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(24) As Double
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(25) As Date
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(26) As Date
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(27) As Date
            If Len(dataRowDetail(27)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            AsDataTableTambahData(dtdetail, "idstdetail~idst~stkategori~idbarang~operator~jml1~jml2~nilai~tgl1~tgl2~jam1~jam2~catatan~urutan~customtext1~customtext2~customtext3~customtext4~customtext5~customint1~customint2~customint3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~nopromo", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28))

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
                Dim vModuleId As Integer = 12, vMenuId As Integer = 69
                Select Case drutama("ststatus")
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
                    result(4) = drutama("stid")
                    notransaksi = drutama("stnotransaksi")

                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(stid), stnotransaksi FROM M_12_St WHERE stid=" & result(4), myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(stid) FROM M_12_St WHERE stnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        'Dim SimpanHistory As New m12_di_history
                        'Dim rsSimpanHistory As String = SimpanHistory.M12_Di_HistorySimpan("" & paramSplit(0) & "★M12_Di_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("disumber")) & "▼" & FixQuotes(drutama("diid")) & "")
                        'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        'If (rsSplitResult(1) = 0) Then
                        '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        'End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M_12_St set stcabang  = '" & FixQuotes(drutama("stcabang")) & "', stlokasi  = '" & FixQuotes(drutama("stlokasi")) & "', stsumber  = '" & FixQuotes(drutama("stsumber")) & "', stkategoripos  = '" & FixQuotes(drutama("stkategoripos")) & "', stautonotransaksi  = " & drutama("stautonotransaksi") & ", stnotransaksi  = '" & FixQuotes(drutama("stnotransaksi")) & "', sttgl  = '" & FixQuotes(AsFormatTanggal(drutama("sttgl"))) & "', stkodepa  = '" & FixQuotes(drutama("stkodepa")) & "', stkontak  = '" & FixQuotes(drutama("stkontak")) & "', stkontakperson  = '" & FixQuotes(drutama("stkontakperson")) & "', sturaian  = '" & FixQuotes(drutama("sturaian")) & "', stcatatan  = '" & FixQuotes(drutama("stcatatan")) & "', ststatus  = " & drutama("ststatus") & ", ststatussebelumnya  = " & drutama("ststatussebelumnya") & ", stjmlrevisi  = " & drutama("stjmlrevisi") & ", stcetakanke  = " & drutama("stcetakanke") & ", stisclose  = " & drutama("stisclose") & ", stmodifikasiuser  = '" & FixQuotes(drutama("stmodifikasiuser")) & "', stmodifikasitgl  = NOW(), stposting  = " & drutama("stposting") & ", stpostingtgl  = '" & FixQuotes(AsFormatTanggal(drutama("stpostingtgl"), "yyyy-MM-dd H:mm:ss")) & "', stcustomtext1  = '" & FixQuotes(drutama("stcustomtext1")) & "', stcustomtext2  = '" & FixQuotes(drutama("stcustomtext2")) & "', stcustomtext3  = '" & FixQuotes(drutama("stcustomtext3")) & "', stcustomtext4  = '" & FixQuotes(drutama("stcustomtext4")) & "', stcustomtext5  = '" & FixQuotes(drutama("stcustomtext5")) & "', stcustomint1  = " & drutama("stcustomint1") & ", stcustomint2  = " & drutama("stcustomint2") & ", stcustomint3  = " & drutama("stcustomint3") & ", stcustomdbl1  = '" & FixDouble(drutama("stcustomdbl1")) & "', stcustomdbl2  = '" & FixDouble(drutama("stcustomdbl2")) & "', stcustomdbl3  = '" & FixDouble(drutama("stcustomdbl3")) & "', stcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("stcustomdate1"))) & "', stcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("stcustomdate2"))) & "', stcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("stcustomdate3"))) & "', stjenispoint  = '" & FixQuotes(drutama("stjenispoint")) & "' where stid = " & drutama("stid") & ""
                        'result(2) = sql : Trans.Rollback() : GoTo selesai
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

                    If drutama("stautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("stcabang"), drutama("stlokasi"), drutama("stsumber"), drutama("sttgl"))
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
                        notransaksi = drutama("stnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(stid) FROM m_12_st WHERE stnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M_12_st (stcabang, stlokasi, stsumber, stkategoripos, stautonotransaksi, stnotransaksi, sttgl, stkodepa, stkontak, stkontakperson, sturaian, stcatatan, ststatus, ststatussebelumnya, stjmlrevisi, stcetakanke, stisclose, stinputuser, stinputtgl, stposting, stpostingtgl, stcustomtext1, stcustomtext2, stcustomtext3, stcustomtext4, stcustomtext5, stcustomint1, stcustomint2, stcustomint3, stcustomdbl1, stcustomdbl2, stcustomdbl3, stcustomdate1, stcustomdate2, stcustomdate3, stjenispoint) values('" & FixQuotes(drutama("stcabang")) & "', '" & FixQuotes(drutama("stlokasi")) & "', '" & FixQuotes(drutama("stsumber")) & "', '" & FixQuotes(drutama("stkategoripos")) & "', " & drutama("stautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("sttgl"))) & "', '" & FixQuotes(drutama("stkodepa")) & "', '" & FixQuotes(drutama("stkontak")) & "', '" & FixQuotes(drutama("stkontakperson")) & "', '" & FixQuotes(drutama("sturaian")) & "', '" & FixQuotes(drutama("stcatatan")) & "', " & drutama("ststatus") & ", " & drutama("ststatussebelumnya") & ", " & drutama("stjmlrevisi") & ", " & drutama("stcetakanke") & ", " & drutama("stisclose") & ", '" & FixQuotes(drutama("stinputuser")) & "', NOW(), 0, '1971-01-01 00:00:00', '" & FixQuotes(drutama("stcustomtext1")) & "', '" & FixQuotes(drutama("stcustomtext2")) & "', '" & FixQuotes(drutama("stcustomtext3")) & "', '" & FixQuotes(drutama("stcustomtext4")) & "', '" & FixQuotes(drutama("stcustomtext5")) & "', " & drutama("stcustomint1") & ", " & drutama("stcustomint2") & ", " & drutama("stcustomint3") & ", '" & FixDouble(drutama("stcustomdbl1")) & "', '" & FixDouble(drutama("stcustomdbl2")) & "', '" & FixDouble(drutama("stcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("stcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("stcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("stcustomdate3"))) & "', '" & FixQuotes(drutama("stjenispoint")) & "')"
                    'result(2) = sql : Trans.Rollback() : GoTo selesai
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
                    dt2 = AsDataTableAmbilDariDBCon("select stid from M_12_st where stnotransaksi='" & notransaksi & "' AND stinputuser= '" & drutama("stinputuser") & "' order by stmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If isUpdate = True Then
                    sql = "Delete from M_12_St_Detail where idst = " & result(4)
                    'result(2) = sql : Trans.Rollback() : GoTo selesai
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
                        strValue2.Append("('" & FixQuotes(dr1("idstdetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("stkategori")) & "', '" & FixQuotes(dr1("idbarang")) & "', '" & FixQuotes(dr1("operator")) & "', '" & FixDouble(dr1("jml1")) & "', '" & FixDouble(dr1("jml2")) & "', '" & FixQuotes(dr1("nilai")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl2"))) & "', '" & FixQuotes(dr1("jam1")) & "', '" & FixQuotes(dr1("jam2")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("urutan")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & notransaksi & "')")
                    Next
                    sql = "Insert into M_12_St_Detail(idstdetail, idst, stkategori, idbarang, operator, jml1, jml2, nilai, tgl1, tgl2, jam1, jam2, catatan, urutan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, nopromo) values" & strValue2.ToString & ""
                    'result(2) = sql : Trans.Rollback() : GoTo selesai
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


                'Update ke tabel Barang Discount
                If drutama("ststatus") = 2 Then 'JIKA STATUS APPROVED
                    If drutama("stjenispoint") = 1 Then 'JIKA JENIS POINT ITEM BASED

                        'Cek apakah kategori pos sudah ada di tabel pos_point_item, jika sudah ada, hapus data di tabel itu
                        'HAPUS POS POINT ITEM
                        sql = "Delete From m_12_pos_point_item where pikategori = '" & drutama("stkategoripos") & "'"
                        'result(2) = sql : Trans.Rollback() : GoTo selesai
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    Else 'JIKA JENIS POINT NOMINAL BASED
                        'Cek apakah kategori pos sudah ada di tabel pos_point_transaction, jika sudah ada, hapus data di tabel itu
                        'HAPUS POS POINT TRANSACTION
                        sql = "Delete From m_12_pos_point_transaction where ptkategori = '" & drutama("stkategoripos") & "'"
                        'result(2) = sql : Trans.Rollback() : GoTo selesai
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If

                    Dim dtdtl As New DataTable
                    dtdtl = AsDataTableAmbilDariDBCon("select * from M_12_ST_Detail where idst = '" & result(4) & "' order by idst asc", myConn)
                    'result(2) = dtdtl.Rows.Count & "" : Trans.Rollback() : GoTo selesai
                    Dim strInsertPoint As New StringBuilder 'untuk query simpan di tabel bonus utama
                    Dim dtKatPOS As New DataTable 'untuk menampung data kategori pos, jika jenis kategori 

                    If dtdtl.Rows.Count > 0 Then

                        If drutama("stjenispoint") = 1 Then 'JIKA JENIS POINT ITEM BASED
                            For Each drdtl As DataRow In dtdtl.Rows
                                'persiapan insert ke tabel m_12_pos_point_item
                                strInsertPoint.Append(IIf(Len(strInsertPoint.ToString) = 0, "", ", "))
                                strInsertPoint.Append("('" & FixQuotes(drutama("stkategoripos")) & "', '" & FixQuotes(drdtl("idbarang")) & "', '" & FixQuotes(drdtl("operator")) & "', '" & FixDouble(drdtl("jml1")) & "', '" & FixDouble(drdtl("jml2")) & "', '" & FixQuotes(drdtl("nilai")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl2"))) & "', '" & FixQuotes(drdtl("customtext1")) & "', '" & FixQuotes(drdtl("customtext2")) & "', '" & FixQuotes(drdtl("customtext3")) & "', '" & FixQuotes(drdtl("customtext4")) & "', '" & FixQuotes(drdtl("customtext5")) & "', " & drdtl("customint1") & ", " & drdtl("customint2") & ", " & drdtl("customint3") & ", '" & FixDouble(drdtl("customdbl1")) & "', '" & FixDouble(drdtl("customdbl2")) & "', '" & FixDouble(drdtl("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate3"))) & "', '" & FixQuotes(drdtl("nopromo")) & "')")
                                'result(2) = strValueDiscountItem.ToString : Trans.Rollback() : GoTo selesai
                            Next

                            'insert ke tabel m_12_pos_discount_item
                            sql = "Insert into M_12_Pos_Point_Item(pikategori, piidbarang, pioperator, pijml1, pijml2, pijmlpoint, pitgl1, pitgl2, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3, pinopromo) values" & strInsertPoint.ToString & ""
                            'result(2) = sql : Trans.Rollback() : GoTo selesai
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        Else 'JIKA JENIS POINT NOMINAL BASED
                            For Each drdtl As DataRow In dtdtl.Rows
                                'persiapan insert ke tabel m_12_pos_point_transaction
                                strInsertPoint.Append(IIf(Len(strInsertPoint.ToString) = 0, "", ", "))
                                strInsertPoint.Append("('" & FixQuotes(drutama("stkategoripos")) & "', '" & FixQuotes(drdtl("operator")) & "', '" & FixDouble(drdtl("jml1")) & "', '" & FixDouble(drdtl("jml2")) & "', '" & FixQuotes(drdtl("nilai")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl2"))) & "', '" & FixQuotes(drdtl("customtext1")) & "', '" & FixQuotes(drdtl("customtext2")) & "', '" & FixQuotes(drdtl("customtext3")) & "', '" & FixQuotes(drdtl("customtext4")) & "', '" & FixQuotes(drdtl("customtext5")) & "', " & drdtl("customint1") & ", " & drdtl("customint2") & ", " & drdtl("customint3") & ", '" & FixDouble(drdtl("customdbl1")) & "', '" & FixDouble(drdtl("customdbl2")) & "', '" & FixDouble(drdtl("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate3"))) & "', '" & FixQuotes(drdtl("nopromo")) & "')")
                                'result(2) = strValueDiscountItem.ToString : Trans.Rollback() : GoTo selesai
                            Next

                            'insert ke tabel m_12_pos_point_transaction
                            sql = "Insert into M_12_Pos_Point_Transaction(ptkategori, ptoperator, ptjml1, ptjml2, ptjmlpoint, pttgl1, pttgl2, ptcustomtext1, ptcustomtext2, ptcustomtext3, ptcustomtext4, ptcustomtext5, ptcustomint1, ptcustomint2, ptcustomint3, ptcustomdbl1, ptcustomdbl2, ptcustomdbl3, ptcustomdate1, ptcustomdate2, ptcustomdate3, ptnopromo) values" & strInsertPoint.ToString & ""
                            'result(2) = sql : Trans.Rollback() : GoTo selesai
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
                        result(2) = "Main Transaction POS Discount Item data not found." : Trans.Rollback() : GoTo selesai
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
        myConn.Close()
        myConn = Nothing
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
    Public Function M12_StUpdateStatus(ByVal param As String) As String
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
            Filter = Filter.Replace("stkontakkode", "c.kkode")
            Filter = Filter.Replace("stkontaknama", "c.knama")
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
            Dim sumber As String = "ST", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Sttgl, Stnotransaksi, Ststatus FROM m_12_St WHERE Stid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Distatussebelumnya" : jnsaktivitas = 17
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
            'Dim SimpanHistory As New m12_di_history
            'Dim rsSimpanHistory As String = SimpanHistory.M12_Di_HistorySimpan("" & paramSplit(0) & "★M12_Di_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            'If (rsSplitResult(1) = 0) Then
            '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            'End If
            'END OF SIMPAN HISTORY ==================


            If isDelete Then
                Dim dtutama As New DataTable
                dtutama = AsDataTableAmbilDariDBCon("SELECT * FROM M_12_St WHERE stid=" & idtransaksi, myConn)
                If (dtutama.Rows.Count > 0) Then
                    For Each drutama As DataRow In dtutama.Rows
                        'AMBIL DATA DETAIL
                        dtdetail = AsDataTableAmbilDariDBCon("SELECT * FROM M_12_St_Detail WHERE idst=" & idtransaksi, myConn)
                        'result(2) = dtdetail.Rows.Count & "" : Trans.Rollback() : GoTo selesai
                        If (dtdetail.Rows.Count > 0) Then
                            For Each drdetail As DataRow In dtdetail.Rows
                                If drutama("stjenispoint") = 1 Then 'JIKA JENIS POINT ITEM BASED
                                    'hapus data detail
                                    Dim strValue2 As New StringBuilder
                                    sql = "Delete from M_12_pos_point_item WHERE pikategori='" & drutama("stkategoripos") & "' AND pinopromo = '" & drutama("stnotransaksi") & "'"
                                    'result(2) = sql : Trans.Rollback() : GoTo selesai
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()

                                Else 'JIKA JENIS KATEGORI NOMINAL BASED
                                    'hapus data detail
                                    Dim strValue2 As New StringBuilder
                                    sql = "Delete from M_12_pos_point_transaction WHERE ptkategori='" & drutama("stkategoripos") & "' AND ptnopromo = '" & drutama("stnotransaksi") & "'"
                                    'result(2) = sql : Trans.Rollback() : GoTo selesai
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()
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
            sql = "UPDATE M_12_St SET Ststatus = " & nilaiStatus & ", stmodifikasiuser='" & userid & "', stmodifikasitgl = NOW(), stposting = 0, stpostingtgl = '1971-01-01 00:00:00', Stjmlrevisi = Stjmlrevisi + 1 WHERE stid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_StSearch(PostWsSearch(paramSplit(0), "M12_StSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_StDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("stkontakkode", "c.kkode")
            Filter = Filter.Replace("stkontaknama", "c.knama")
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
            Dim sumber As String = "ST", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT stid, stnotransaksi FROM m_12_st WHERE stid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT stcabang, stlokasi, stsumber, stautonotransaksi, stnotransaksi, sttgl"
            sql &= " FROM M_12_st"
            sql &= " WHERE stid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("stcabang")
                lokasi = dtNomorNext.Rows(0)("stlokasi")
                sumber = dtNomorNext.Rows(0)("stsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("stautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("stnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("sttgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M_12_St_Detail WHERE idst = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M_12_St WHERE stid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_StSearch(PostWsSearch(paramSplit(0), "M12_StSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_StSimpanOld(ByVal param As String) As String
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
        'stid(0) As Integer, stcabang(1) As String, stlokasi(2) As String, stsumber(3) As String, stkategoripos(4) As String, 
        'stautonotransaksi(5) As Integer, stnotransaksi(6) As String, sttgl(7) As Date, stkodepa(8) As , stkontak(9) As , 
        'stkontakperson(10) As String, sturaian(11) As String, stcatatan(12) As String, ststatus(13) As Integer, ststatussebelumnya(14) As Integer, 
        'stjmlrevisi(15) As Integer, stcetakanke(16) As Integer, stisclose(17) As Integer, stinputuser(18) As , stinputtgl(19) As DateTime, 
        'stmodifikasiuser(20) As , stmodifikasiuser(21) As DateTime, stposting(22) As Integer, stpostingtgl(23) As DateTime, stcustomtext1(24) As String, 
        'stcustomtext2(25) As String, stcustomtext3(26) As String, stcustomtext4(27) As String, stcustomtext5(28) As String, stcustomint1(29) As Integer, 
        'stcustomint2(30) As Integer, stcustomint3(31) As Integer, stcustomdbl1(32) As Double, stcustomdbl2(33) As Double, stcustomdbl3(34) As Double, 
        'stcustomdate1(35) As Date, stcustomdate2(36) As Date, stcustomdate3(37) As Date, stjenispoint(38) As Int

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'stid, stcabang, stlokasi, stsumber, stkategoripos, stautonotransaksi, stnotransaksi, 
        'sttgl, stkodepa, stkontak, stkontakperson, sturaian, stcatatan, ststatus, 
        'ststatussebelumnya, stjmlrevisi, stcetakanke, stisclose, stinputuser, stinputtgl, stmodifikasiuser, 
        'stmodifikasiuser, stposting, stpostingtgl, stcustomtext1, stcustomtext2, stcustomtext3, stcustomtext4, 
        'stcustomtext5, stcustomint1, stcustomint2, stcustomint3, stcustomdbl1, stcustomdbl2, stcustomdbl3, 
        'stcustomdate1, stcustomdate2, stcustomdate3, stjenispoint

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 39) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'stid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "stid required numeric." : GoTo selesai
        End If
        'stautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "stautonotransaksi required numeric." : GoTo selesai
        End If
        'sttgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "sttgl required date." : GoTo selesai
        End If
        'ststatus(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "ststatus required numeric." : GoTo selesai
        End If
        'ststatussebelumnya(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "ststatussebelumnya required numeric." : GoTo selesai
        End If
        'stjmlrevisi(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "stjmlrevisi required numeric." : GoTo selesai
        End If
        'stcetakanke(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "stcetakanke required numeric." : GoTo selesai
        End If
        'stisclose(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "stisclose required numeric." : GoTo selesai
        End If
        'stinputtgl(19) As DateTime
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "stinputtgl required date." : GoTo selesai
        End If
        'stmodifikasiuser(21) As DateTime
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "stmodifikasiuser required date." : GoTo selesai
        End If
        'stposting(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "stposting required numeric." : GoTo selesai
        End If
        'stpostingtgl(23) As DateTime
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "stpostingtgl required date." : GoTo selesai
        End If
        'stcustomint1(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "stcustomint1 required numeric." : GoTo selesai
        End If
        'stcustomint2(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "stcustomint2 required numeric." : GoTo selesai
        End If
        'stcustomint3(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "stcustomint3 required numeric." : GoTo selesai
        End If
        'stcustomdbl1(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "stcustomdbl1 required numeric." : GoTo selesai
        End If
        'stcustomdbl2(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "stcustomdbl2 required numeric." : GoTo selesai
        End If
        'stcustomdbl3(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "stcustomdbl3 required numeric." : GoTo selesai
        End If
        'stcustomdate1(35) As Date
        If (IsDate(dataUtama(35)) = False) Then
            result(2) = "stcustomdate1 required date." : GoTo selesai
        End If
        'stcustomdate2(36) As Date
        If (IsDate(dataUtama(36)) = False) Then
            result(2) = "stcustomdate2 required date." : GoTo selesai
        End If
        'stcustomdate3(37) As Date
        If (IsDate(dataUtama(37)) = False) Then
            result(2) = "stcustomdate3 required date." : GoTo selesai
        End If

        'stjenispoint(38) As Date
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "stjenispoint required numeric." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'stcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "stcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "stcabang should not be more than 25 character." : GoTo selesai
        End If

        'stlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "stlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "stlokasi should not be more than 25 character." : GoTo selesai
        End If

        'stsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "stsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "stsumber should not be more than 10 character." : GoTo selesai
        End If

        'stkategoripos(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "stkategoripos can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 50 Then
            result(2) = "stkategoripos should not be more than 50 character." : GoTo selesai
        End If

        'stnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "stnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "stnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'sttgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "sttgl can't be empty" : GoTo selesai
        End If

        'stkodepa(8) As 
        If Len(dataUtama(8)) = 0 Then
            result(2) = "stkodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 20 Then
            result(2) = "stkodepa should not be more than 20 character." : GoTo selesai
        End If

        'stkontak(9) As 
        If Len(dataUtama(9)) = 0 Then
            result(2) = "stkontak can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 20 Then
            result(2) = "stkontak should not be more than 20 character." : GoTo selesai
        End If

        'stinputtgl(19) As DateTime
        If Len(dataUtama(19)) = 0 Then
            result(2) = "stinputtgl can't be empty" : GoTo selesai
        End If

        'stmodifikasiuser(21) As DateTime
        If Len(dataUtama(21)) = 0 Then
            result(2) = "stmodifikasiuser can't be empty" : GoTo selesai
        End If

        'stpostingtgl(23) As DateTime
        If Len(dataUtama(23)) = 0 Then
            result(2) = "stpostingtgl can't be empty" : GoTo selesai
        End If

        'stcustomdbl1(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "stcustomdbl1 can't be empty" : GoTo selesai
        End If

        'stcustomdbl2(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "stcustomdbl2 can't be empty" : GoTo selesai
        End If

        'stcustomdbl3(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "stcustomdbl3 can't be empty" : GoTo selesai
        End If

        'stcustomdate1(35) As Date
        If Len(dataUtama(35)) = 0 Then
            result(2) = "stcustomdate1 can't be empty" : GoTo selesai
        End If

        'stcustomdate2(36) As Date
        If Len(dataUtama(36)) = 0 Then
            result(2) = "stcustomdate2 can't be empty" : GoTo selesai
        End If

        'stcustomdate3(37) As Date
        If Len(dataUtama(37)) = 0 Then
            result(2) = "stcustomdate3 can't be empty" : GoTo selesai
        End If

        'stjenispoint(38) As Date
        If Len(dataUtama(38)) = 0 Then
            result(2) = "stjenispoint can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "stid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stkategoripos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sttgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sturaian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ststatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ststatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stpostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "stcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "stjenispoint", AsEnumTypeData.AsInt64)

        If AsDataTableTambahData(dtutama, "stid~stcabang~stlokasi~stsumber~stkategoripos~stautonotransaksi~stnotransaksi~sttgl~stkodepa~stkontak~stkontakperson~sturaian~stcatatan~ststatus~ststatussebelumnya~stjmlrevisi~stcetakanke~stisclose~stinputuser~stinputtgl~stmodifikasiuser~stmodifikasitgl~stposting~stpostingtgl~stcustomtext1~stcustomtext2~stcustomtext3~stcustomtext4~stcustomtext5~stcustomint1~stcustomint2~stcustomint3~stcustomdbl1~stcustomdbl2~stcustomdbl3~stcustomdate1~stcustomdate2~stcustomdate3~stjenispoint", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38)) = False Then
            result(2) = "insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'iddidetail, iddi, dikategori, idbarang, operator, jml1, jml2, 
        'nilai, tgl1, tgl2, jam1, jam2, catatan, 
        'urutan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, 
        'customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3, nopromo

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idstdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idst", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "stkategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "operator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jam1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jam2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "nopromo", AsEnumTypeData.AsString)

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 29) Then
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

            'tgl1(8) As Date
            If (IsDate(dataRowDetail(8)) = False) Then
                result(2) = "tgl1 required date." : GoTo selesai
            End If
            'tgl2(9) As Date
            If (IsDate(dataRowDetail(9)) = False) Then
                result(2) = "tgl2 required date." : GoTo selesai
            End If
            'jam1(10) As Date
            If (IsDate(dataRowDetail(10)) = False) Then
                result(2) = "jam1 required date." : GoTo selesai
            End If
            'jam2(11) As Date
            If (IsDate(dataRowDetail(11)) = False) Then
                result(2) = "jam2 required date." : GoTo selesai
            End If
            'customint1(19) As Integer
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "customint1 required numeric." : GoTo selesai
            End If
            'customint2(20) As Integer
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "customint2 required numeric." : GoTo selesai
            End If
            'customint3(21) As Integer
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "customint3 required numeric." : GoTo selesai
            End If
            'customdbl1(22) As Double
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(23) As Double
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(24) As Double
            If (IsNumeric(dataRowDetail(24)) = False) Then
                result(2) = "customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(25) As Date
            If (IsDate(dataRowDetail(25)) = False) Then
                result(2) = "customdate1 required date." : GoTo selesai
            End If
            'customdate2(26) As Date
            If (IsDate(dataRowDetail(26)) = False) Then
                result(2) = "customdate2 required date." : GoTo selesai
            End If
            'customdate3(27) As Date
            If (IsDate(dataRowDetail(27)) = False) Then
                result(2) = "customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'iddidetail(0) As 
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - iddidetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - iddidetail should not be more than 20 character." : GoTo selesai
            End If

            'iddi(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - iddi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - iddi should not be more than 20 character." : GoTo selesai
            End If

            'dikategori(2) As String
            'If Len(dataRowDetail(2)) = 0 Then
            '    result(2) = "Row : " & i & " - dikategori can't be empty" : GoTo selesai
            'End If
            'If Len(dataRowDetail(2)) > 25 Then
            '    result(2) = "Row : " & i & " - dikategori should not be more than 25 character." : GoTo selesai
            'End If

            If dataUtama(38) = 1 Then 'JIKA JENIS POINT = ITEM BASED
                'idbarang(3) As 
                If Len(dataRowDetail(3)) = 0 Then
                    result(2) = "Row : " & i & " - idbarang can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(3)) > 20 Then
                    result(2) = "Row : " & i & " - idbarang should not be more than 20 character." : GoTo selesai
                End If
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

            'nilai(7) As String
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - nilai can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(7)) > 25 Then
                result(2) = "Row : " & i & " - nilai should not be more than 25 character." : GoTo selesai
            End If

            'tgl1(8) As Date
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - tgl1 can't be empty" : GoTo selesai
            End If

            'tgl2(9) As Date
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - tgl2 can't be empty" : GoTo selesai
            End If

            'jam1(10) As Date
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - jam1 can't be empty" : GoTo selesai
            End If

            'jam2(11) As Date
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - jam2 can't be empty" : GoTo selesai
            End If

            'urutan(13) As 
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - urutan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(14)) > 20 Then
                result(2) = "Row : " & i & " - urutan should not be more than 20 character." : GoTo selesai
            End If

            'customdbl1(22) As Double
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(23) As Double
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(24) As Double
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(25) As Date
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(26) As Date
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(27) As Date
            If Len(dataRowDetail(27)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            AsDataTableTambahData(dtdetail, "idstdetail~idst~stkategori~idbarang~operator~jml1~jml2~nilai~tgl1~tgl2~jam1~jam2~catatan~urutan~customtext1~customtext2~customtext3~customtext4~customtext5~customint1~customint2~customint3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~nopromo", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28))

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
                If isUpdate Then
                    result(4) = drutama("stid")
                    notransaksi = drutama("stnotransaksi")

                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(stid), stnotransaksi FROM M_12_St WHERE stid=" & result(4))
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(stid) FROM M_12_St WHERE stnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        'Dim SimpanHistory As New m12_di_history
                        'Dim rsSimpanHistory As String = SimpanHistory.M12_Di_HistorySimpan("" & paramSplit(0) & "★M12_Di_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("disumber")) & "▼" & FixQuotes(drutama("diid")) & "")
                        'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        'If (rsSplitResult(1) = 0) Then
                        '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        'End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M_12_St set stcabang  = '" & FixQuotes(drutama("stcabang")) & "', stlokasi  = '" & FixQuotes(drutama("stlokasi")) & "', stsumber  = '" & FixQuotes(drutama("stsumber")) & "', stkategoripos  = '" & FixQuotes(drutama("stkategoripos")) & "', stautonotransaksi  = " & drutama("stautonotransaksi") & ", stnotransaksi  = '" & FixQuotes(drutama("stnotransaksi")) & "', sttgl  = '" & FixQuotes(AsFormatTanggal(drutama("sttgl"))) & "', stkodepa  = '" & FixQuotes(drutama("stkodepa")) & "', stkontak  = '" & FixQuotes(drutama("stkontak")) & "', stkontakperson  = '" & FixQuotes(drutama("stkontakperson")) & "', sturaian  = '" & FixQuotes(drutama("sturaian")) & "', stcatatan  = '" & FixQuotes(drutama("stcatatan")) & "', ststatus  = " & drutama("ststatus") & ", ststatussebelumnya  = " & drutama("ststatussebelumnya") & ", stjmlrevisi  = " & drutama("stjmlrevisi") & ", stcetakanke  = " & drutama("stcetakanke") & ", stisclose  = " & drutama("stisclose") & ", stmodifikasiuser  = '" & FixQuotes(drutama("stmodifikasiuser")) & "', stmodifikasitgl  = NOW(), stposting  = " & drutama("stposting") & ", stpostingtgl  = '" & FixQuotes(AsFormatTanggal(drutama("stpostingtgl"), "yyyy-MM-dd H:mm:ss")) & "', stcustomtext1  = '" & FixQuotes(drutama("stcustomtext1")) & "', stcustomtext2  = '" & FixQuotes(drutama("stcustomtext2")) & "', stcustomtext3  = '" & FixQuotes(drutama("stcustomtext3")) & "', stcustomtext4  = '" & FixQuotes(drutama("stcustomtext4")) & "', stcustomtext5  = '" & FixQuotes(drutama("stcustomtext5")) & "', stcustomint1  = " & drutama("stcustomint1") & ", stcustomint2  = " & drutama("stcustomint2") & ", stcustomint3  = " & drutama("stcustomint3") & ", stcustomdbl1  = '" & FixDouble(drutama("stcustomdbl1")) & "', stcustomdbl2  = '" & FixDouble(drutama("stcustomdbl2")) & "', stcustomdbl3  = '" & FixDouble(drutama("stcustomdbl3")) & "', stcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("stcustomdate1"))) & "', stcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("stcustomdate2"))) & "', stcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("stcustomdate3"))) & "', stjenispoint  = '" & FixQuotes(drutama("stjenispoint")) & "' where stid = " & drutama("stid") & ""
                        'result(2) = sql : Trans.Rollback() : GoTo selesai
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

                    If drutama("stautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("stcabang"), drutama("stlokasi"), drutama("stsumber"), drutama("sttgl"))
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
                        notransaksi = drutama("stnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(stid) FROM m_12_st WHERE stnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M_12_st (stcabang, stlokasi, stsumber, stkategoripos, stautonotransaksi, stnotransaksi, sttgl, stkodepa, stkontak, stkontakperson, sturaian, stcatatan, ststatus, ststatussebelumnya, stjmlrevisi, stcetakanke, stisclose, stinputuser, stinputtgl, stposting, stpostingtgl, stcustomtext1, stcustomtext2, stcustomtext3, stcustomtext4, stcustomtext5, stcustomint1, stcustomint2, stcustomint3, stcustomdbl1, stcustomdbl2, stcustomdbl3, stcustomdate1, stcustomdate2, stcustomdate3, stjenispoint) values('" & FixQuotes(drutama("stcabang")) & "', '" & FixQuotes(drutama("stlokasi")) & "', '" & FixQuotes(drutama("stsumber")) & "', '" & FixQuotes(drutama("stkategoripos")) & "', " & drutama("stautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("sttgl"))) & "', '" & FixQuotes(drutama("stkodepa")) & "', '" & FixQuotes(drutama("stkontak")) & "', '" & FixQuotes(drutama("stkontakperson")) & "', '" & FixQuotes(drutama("sturaian")) & "', '" & FixQuotes(drutama("stcatatan")) & "', " & drutama("ststatus") & ", " & drutama("ststatussebelumnya") & ", " & drutama("stjmlrevisi") & ", " & drutama("stcetakanke") & ", " & drutama("stisclose") & ", '" & FixQuotes(drutama("stinputuser")) & "', NOW(), 0, '1971-01-01 00:00:00', '" & FixQuotes(drutama("stcustomtext1")) & "', '" & FixQuotes(drutama("stcustomtext2")) & "', '" & FixQuotes(drutama("stcustomtext3")) & "', '" & FixQuotes(drutama("stcustomtext4")) & "', '" & FixQuotes(drutama("stcustomtext5")) & "', " & drutama("stcustomint1") & ", " & drutama("stcustomint2") & ", " & drutama("stcustomint3") & ", '" & FixDouble(drutama("stcustomdbl1")) & "', '" & FixDouble(drutama("stcustomdbl2")) & "', '" & FixDouble(drutama("stcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("stcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("stcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("stcustomdate3"))) & "', '" & FixQuotes(drutama("stjenispoint")) & "')"
                    'result(2) = sql : Trans.Rollback() : GoTo selesai
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
                    dt2 = AsDataTableAmbilDariDB("select stid from M_12_st where stnotransaksi='" & notransaksi & "' AND stinputuser= '" & drutama("stinputuser") & "' order by stmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If isUpdate = True Then
                    sql = "Delete from M_12_St_Detail where idst = " & result(4)
                    'result(2) = sql : Trans.Rollback() : GoTo selesai
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
                        strValue2.Append("('" & FixQuotes(dr1("idstdetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("stkategori")) & "', '" & FixQuotes(dr1("idbarang")) & "', '" & FixQuotes(dr1("operator")) & "', '" & FixDouble(dr1("jml1")) & "', '" & FixDouble(dr1("jml2")) & "', '" & FixQuotes(dr1("nilai")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl2"))) & "', '" & FixQuotes(dr1("jam1")) & "', '" & FixQuotes(dr1("jam2")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("urutan")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & notransaksi & "')")
                    Next
                    sql = "Insert into M_12_St_Detail(idstdetail, idst, stkategori, idbarang, operator, jml1, jml2, nilai, tgl1, tgl2, jam1, jam2, catatan, urutan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, nopromo) values" & strValue2.ToString & ""
                    'result(2) = sql : Trans.Rollback() : GoTo selesai
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


                'Update ke tabel Barang Discount
                If drutama("ststatus") = 2 Then 'JIKA STATUS APPROVED
                    If drutama("stjenispoint") = 1 Then 'JIKA JENIS POINT ITEM BASED

                        'Cek apakah kategori pos sudah ada di tabel pos_point_item, jika sudah ada, hapus data di tabel itu
                        'HAPUS POS POINT ITEM
                        sql = "Delete From m_12_pos_point_item where pikategori = '" & drutama("stkategoripos") & "'"
                        'result(2) = sql : Trans.Rollback() : GoTo selesai
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    Else 'JIKA JENIS POINT NOMINAL BASED
                        'Cek apakah kategori pos sudah ada di tabel pos_point_transaction, jika sudah ada, hapus data di tabel itu
                        'HAPUS POS POINT TRANSACTION
                        sql = "Delete From m_12_pos_point_transaction where ptkategori = '" & drutama("stkategoripos") & "'"
                        'result(2) = sql : Trans.Rollback() : GoTo selesai
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If

                    Dim dtdtl As New DataTable
                    dtdtl = AsDataTableAmbilDariDB("select * from M_12_ST_Detail where idst = '" & result(4) & "' order by idst asc")
                    'result(2) = dtdtl.Rows.Count & "" : Trans.Rollback() : GoTo selesai
                    Dim strInsertPoint As New StringBuilder 'untuk query simpan di tabel bonus utama
                    Dim dtKatPOS As New DataTable 'untuk menampung data kategori pos, jika jenis kategori 

                    If dtdtl.Rows.Count > 0 Then

                        If drutama("stjenispoint") = 1 Then 'JIKA JENIS POINT ITEM BASED
                            For Each drdtl As DataRow In dtdtl.Rows
                                'persiapan insert ke tabel m_12_pos_point_item
                                strInsertPoint.Append(IIf(Len(strInsertPoint.ToString) = 0, "", ", "))
                                strInsertPoint.Append("('" & FixQuotes(drutama("stkategoripos")) & "', '" & FixQuotes(drdtl("idbarang")) & "', '" & FixQuotes(drdtl("operator")) & "', '" & FixDouble(drdtl("jml1")) & "', '" & FixDouble(drdtl("jml2")) & "', '" & FixQuotes(drdtl("nilai")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl2"))) & "', '" & FixQuotes(drdtl("customtext1")) & "', '" & FixQuotes(drdtl("customtext2")) & "', '" & FixQuotes(drdtl("customtext3")) & "', '" & FixQuotes(drdtl("customtext4")) & "', '" & FixQuotes(drdtl("customtext5")) & "', " & drdtl("customint1") & ", " & drdtl("customint2") & ", " & drdtl("customint3") & ", '" & FixDouble(drdtl("customdbl1")) & "', '" & FixDouble(drdtl("customdbl2")) & "', '" & FixDouble(drdtl("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate3"))) & "', '" & FixQuotes(drdtl("nopromo")) & "')")
                                'result(2) = strValueDiscountItem.ToString : Trans.Rollback() : GoTo selesai
                            Next

                            'insert ke tabel m_12_pos_discount_item
                            sql = "Insert into M_12_Pos_Point_Item(pikategori, piidbarang, pioperator, pijml1, pijml2, pijmlpoint, pitgl1, pitgl2, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3, pinopromo) values" & strInsertPoint.ToString & ""
                            'result(2) = sql : Trans.Rollback() : GoTo selesai
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        Else 'JIKA JENIS POINT NOMINAL BASED
                            For Each drdtl As DataRow In dtdtl.Rows
                                'persiapan insert ke tabel m_12_pos_point_transaction
                                strInsertPoint.Append(IIf(Len(strInsertPoint.ToString) = 0, "", ", "))
                                strInsertPoint.Append("('" & FixQuotes(drutama("stkategoripos")) & "', '" & FixQuotes(drdtl("operator")) & "', '" & FixDouble(drdtl("jml1")) & "', '" & FixDouble(drdtl("jml2")) & "', '" & FixQuotes(drdtl("nilai")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl2"))) & "', '" & FixQuotes(drdtl("customtext1")) & "', '" & FixQuotes(drdtl("customtext2")) & "', '" & FixQuotes(drdtl("customtext3")) & "', '" & FixQuotes(drdtl("customtext4")) & "', '" & FixQuotes(drdtl("customtext5")) & "', " & drdtl("customint1") & ", " & drdtl("customint2") & ", " & drdtl("customint3") & ", '" & FixDouble(drdtl("customdbl1")) & "', '" & FixDouble(drdtl("customdbl2")) & "', '" & FixDouble(drdtl("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate3"))) & "', '" & FixQuotes(drdtl("nopromo")) & "')")
                                'result(2) = strValueDiscountItem.ToString : Trans.Rollback() : GoTo selesai
                            Next

                            'insert ke tabel m_12_pos_point_transaction
                            sql = "Insert into M_12_Pos_Point_Transaction(ptkategori, ptoperator, ptjml1, ptjml2, ptjmlpoint, pttgl1, pttgl2, ptcustomtext1, ptcustomtext2, ptcustomtext3, ptcustomtext4, ptcustomtext5, ptcustomint1, ptcustomint2, ptcustomint3, ptcustomdbl1, ptcustomdbl2, ptcustomdbl3, ptcustomdate1, ptcustomdate2, ptcustomdate3, ptnopromo) values" & strInsertPoint.ToString & ""
                            'result(2) = sql : Trans.Rollback() : GoTo selesai
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
                        result(2) = "Main Transaction POS Discount Item data not found." : Trans.Rollback() : GoTo selesai
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
    Public Function M12_StUpdateStatusOld(ByVal param As String) As String
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
            Filter = Filter.Replace("stkontakkode", "c.kkode")
            Filter = Filter.Replace("stkontaknama", "c.knama")
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
            Dim sumber As String = "ST", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Sttgl, Stnotransaksi, Ststatus FROM m_12_St WHERE Stid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Distatussebelumnya" : jnsaktivitas = 17
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
            'Dim SimpanHistory As New m12_di_history
            'Dim rsSimpanHistory As String = SimpanHistory.M12_Di_HistorySimpan("" & paramSplit(0) & "★M12_Di_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            'If (rsSplitResult(1) = 0) Then
            '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            'End If
            'END OF SIMPAN HISTORY ==================


            If isDelete Then
                Dim dtutama As New DataTable
                dtutama = AsDataTableAmbilDariDB("SELECT * FROM M_12_St WHERE stid=" & idtransaksi)
                If (dtutama.Rows.Count > 0) Then
                    For Each drutama As DataRow In dtutama.Rows
                        'AMBIL DATA DETAIL
                        dtdetail = AsDataTableAmbilDariDB("SELECT * FROM M_12_St_Detail WHERE idst=" & idtransaksi)
                        'result(2) = dtdetail.Rows.Count & "" : Trans.Rollback() : GoTo selesai
                        If (dtdetail.Rows.Count > 0) Then
                            For Each drdetail As DataRow In dtdetail.Rows
                                If drutama("stjenispoint") = 1 Then 'JIKA JENIS POINT ITEM BASED
                                    'hapus data detail
                                    Dim strValue2 As New StringBuilder
                                    sql = "Delete from M_12_pos_point_item WHERE pikategori='" & drutama("stkategoripos") & "' AND pinopromo = '" & drutama("stnotransaksi") & "'"
                                    'result(2) = sql : Trans.Rollback() : GoTo selesai
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = Con1
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()

                                Else 'JIKA JENIS KATEGORI NOMINAL BASED
                                    'hapus data detail
                                    Dim strValue2 As New StringBuilder
                                    sql = "Delete from M_12_pos_point_transaction WHERE ptkategori='" & drutama("stkategoripos") & "' AND ptnopromo = '" & drutama("stnotransaksi") & "'"
                                    'result(2) = sql : Trans.Rollback() : GoTo selesai
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = Con1
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()
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
            sql = "UPDATE M_12_St SET Ststatus = " & nilaiStatus & ", stmodifikasiuser='" & userid & "', stmodifikasitgl = NOW(), stposting = 0, stpostingtgl = '1971-01-01 00:00:00', Stjmlrevisi = Stjmlrevisi + 1 WHERE stid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_StSearch(PostWsSearch(paramSplit(0), "M12_StSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_StDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("stkontakkode", "c.kkode")
            Filter = Filter.Replace("stkontaknama", "c.knama")
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
            Dim sumber As String = "ST", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT stid, stnotransaksi FROM m_12_st WHERE stid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT stcabang, stlokasi, stsumber, stautonotransaksi, stnotransaksi, sttgl"
            sql &= " FROM M_12_st"
            sql &= " WHERE stid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("stcabang")
                lokasi = dtNomorNext.Rows(0)("stlokasi")
                sumber = dtNomorNext.Rows(0)("stsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("stautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("stnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("sttgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M_12_St_Detail WHERE idst = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M_12_St WHERE stid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_StSearch(PostWsSearch(paramSplit(0), "M12_StSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_StGetdataById(ByVal param As String) As String

        'M12_StGetdataById Utama --------------------------------------------------------
        'stid, stcabang, stlokasi, stsumber, stkategoripos, stautonotransaksi, stnotransaksi, 
        'sttgl, stkodepa, stkontak, stkontakperson, sturaian, stcatatan, ststatus, 
        'ststatussebelumnya, stjmlrevisi, stcetakanke, stisclose, stinputuser, stinputtgl, stmodifikasiuser, 
        'stmodifikasiuser, stposting, stpostingtgl, stcustomtext1, stcustomtext2, stcustomtext3, stcustomtext4, 
        'stcustomtext5, stcustomint1, stcustomint2, stcustomint3, stcustomdbl1, stcustomdbl2, stcustomdbl3, 
        'stcustomdate1, stcustomdate2, stcustomdate3, stcabangnama, stlokasinama, stkontakkode, stkontaknama
        'ststatusnama, ststatussebelumnyanama, stinputusernama, stmodifikasiusernama, stkategoriposnama, stjenispoint

        'M12_StGetdataById Detail -------------------------------------------------------
        'idstdetail, idst, stkategori, idbarang, operator, jml1, jml2,
        'nilai, tgl1, tgl2, jam1, jam2, catatan, urutan, 
        'customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, 
        'customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'nopromo, kodebarang, namabarang



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

        Dim utama As String = "", detail As String = "", discount As String = "", idtransaksi As String = ""

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
            Filter = "stid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "stid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select `st`.`stid` AS `stid`,`st`.`stcabang` AS `stcabang`,`st`.`stlokasi` AS `stlokasi`,`st`.`stsumber` AS `stsumber`,`st`.`stkategoripos` AS `stkategoripos`,`st`.`stautonotransaksi` AS `stautonotransaksi`,`st`.`stnotransaksi` AS `stnotransaksi`,`st`.`sttgl` AS `sttgl`,`st`.`stkodepa` AS `stkodepa`,`st`.`stkontak` AS `stkontak`,`st`.`stkontakperson` AS `stkontakperson`,`st`.`sturaian` AS `sturaian`,`st`.`stcatatan` AS `stcatatan`,`st`.`ststatus` AS `ststatus`,`st`.`ststatussebelumnya` AS `ststatussebelumnya`,`st`.`stjmlrevisi` AS `stjmlrevisi`,`st`.`stcetakanke` AS `stcetakanke`,`st`.`stisclose` AS `stisclose`,`st`.`stinputuser` AS `stinputuser`,`st`.`stinputtgl` AS `stinputtgl`,`st`.`stmodifikasiuser` AS `stmodifikasiuser`,`st`.`stmodifikasitgl` AS `stmodifikasitgl`,`st`.`stposting` AS `stposting`,`st`.`stpostingtgl` AS `stpostingtgl`,`st`.`stcustomtext1` AS `stcustomtext1`,`st`.`stcustomtext2` AS `stcustomtext2`,`st`.`stcustomtext3` AS `stcustomtext3`,`st`.`stcustomtext4` AS `stcustomtext4`,`st`.`stcustomtext5` AS `stcustomtext5`,`st`.`stcustomint1` AS `stcustomint1`,`st`.`stcustomint2` AS `stcustomint2`,`st`.`stcustomint3` AS `stcustomint3`,`st`.`stcustomdbl1` AS `stcustomdbl1`,`st`.`stcustomdbl2` AS `stcustomdbl2`,`st`.`stcustomdbl3` AS `stcustomdbl3`,`st`.`stcustomdate1` AS `stcustomdate1`,`st`.`stcustomdate2` AS `stcustomdate2`,`st`.`stcustomdate3` AS `stcustomdate3`,`br`.`bnama` AS `stcabangnama`,`lc`.`lnama` AS `stlokasinama`,`c`.`kkode` AS `stkontakkode`,`c`.`knama` AS `stkontaknama`,`st1`.`nama` AS `ststatusnama`,`st2`.`nama` AS `ststatussebelumnyanama`,`u1`.`unama` AS `stinputusernama`,`u2`.`unama` AS `stmodifikasiusernama`,`pc`.`pcnama` AS `stkategoriposnama`,`st`.`stjenispoint` AS `stjenispoint`,`std`.`idstdetail` AS `idstdetail`,`std`.`idst` AS `idst`,`std`.`stkategori` AS `stkategori`,`std`.`idbarang` AS `idbarang`,`std`.`operator` AS `operator`,`std`.`jml1` AS `jml1`,`std`.`jml2` AS `jml2`,`std`.`nilai` AS `nilai`,`std`.`customtext1` AS `customtext1`,`std`.`customtext2` AS `customtext2`,`std`.`customtext3` AS `customtext3`,`std`.`customtext4` AS `customtext4`,`std`.`customtext5` AS `customtext5`,`std`.`customint1` AS `customint1`,`std`.`customint2` AS `customint2`,`std`.`customint3` AS `customint3`,`std`.`customdbl1` AS `customdbl1`,`std`.`customdbl2` AS `customdbl2`,`std`.`customdbl3` AS `customdbl3`,`std`.`customdate1` AS `customdate1`,`std`.`customdate2` AS `customdate2`,`std`.`customdate3` AS `customdate3`,`std`.`tgl1` AS `tgl1`,`std`.`tgl2` AS `tgl2`,`std`.`nopromo` AS `nopromo`,`std`.`jam1` AS `jam1`,`std`.`jam2` AS `jam2`,`i`.`bkode` AS `kodebarang`,`i`.`bnama` AS `namabarang`, `std`.`catatan` AS `catatan`, `std`.`urutan` AS `urutan`  from ((((((((((`m_12_st` `st` join `m_12_st_detail` `std` on((`st`.`stid` = `std`.`idst`))) left join `m1_branch` `br` on((`st`.`stcabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`st`.`stlokasi` = `lc`.`lkode`))) left join `m1_contact` `c` on((`st`.`stkontak` = `c`.`kid`))) left join `m0_status` `st1` on((`st`.`ststatus` = `st1`.`kode`))) left join `m0_status` `st2` on((`st`.`ststatussebelumnya` = `st2`.`kode`))) left join `m0_user` `u1` on((`st`.`stinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`st`.`stmodifikasiuser` = `u2`.`userid`))) left join `m1_item` `i` on((`std`.`idbarang` = `i`.`bid`)))  left join `m_12_pos_category` `pc` on((`st`.`stkategoripos` = `pc`.`pckode`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("stid"), 0), sptField,
                     FxDB(drutama("stcabang"), ""), sptField,
                     FxDB(drutama("stlokasi"), ""), sptField,
                     FxDB(drutama("stsumber"), ""), sptField,
                     FxDB(drutama("stkategoripos"), ""), sptField,
                     FxDB(drutama("stautonotransaksi"), 0), sptField,
                     FxDB(drutama("stnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sttgl"), ""), formatTgl), sptField,
                     FxDB(drutama("stkodepa"), ""), sptField,
                     FxDB(drutama("stkontak"), ""), sptField,
                     FxDB(drutama("stkontakperson"), ""), sptField,
                     FxDB(drutama("sturaian"), ""), sptField,
                     FxDB(drutama("stcatatan"), ""), sptField,
                     FxDB(drutama("ststatus"), 0), sptField,
                     FxDB(drutama("ststatussebelumnya"), 0), sptField,
                     FxDB(drutama("stjmlrevisi"), 0), sptField,
                     FxDB(drutama("stcetakanke"), 0), sptField,
                     FxDB(drutama("stisclose"), 0), sptField,
                     FxDB(drutama("stinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("stinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("stmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("stmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("stposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("stpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("stcustomtext1"), ""), sptField,
                     FxDB(drutama("stcustomtext2"), ""), sptField,
                     FxDB(drutama("stcustomtext3"), ""), sptField,
                     FxDB(drutama("stcustomtext4"), ""), sptField,
                     FxDB(drutama("stcustomtext5"), ""), sptField,
                     FxDB(drutama("stcustomint1"), 0), sptField,
                     FxDB(drutama("stcustomint2"), 0), sptField,
                     FxDB(drutama("stcustomint3"), 0), sptField,
                     FxDB(drutama("stcustomdbl1"), 0), sptField,
                     FxDB(drutama("stcustomdbl2"), 0), sptField,
                     FxDB(drutama("stcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("stcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("stcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("stcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("stcabangnama"), ""), sptField,
                     FxDB(drutama("stlokasinama"), ""), sptField,
                     FxDB(drutama("stkontakkode"), ""), sptField,
                     FxDB(drutama("stkontaknama"), ""), sptField,
                     FxDB(drutama("ststatusnama"), ""), sptField,
                     FxDB(drutama("ststatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("stinputusernama"), ""), sptField,
                     FxDB(drutama("stmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("stkategoriposnama"), ""), sptField,
                     FxDB(drutama("stjenispoint"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idstdetail"), ""), sptField,
                     FxDB(dr("idst"), ""), sptField,
                     FxDB(dr("stkategori"), ""), sptField,
                     FxDB(dr("idbarang"), ""), sptField,
                     FxDB(dr("operator"), ""), sptField,
                     FxDB(dr("jml1"), 0), sptField,
                     FxDB(dr("jml2"), 0), sptField,
                     FxDB(dr("nilai"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("tgl2"), ""), formatTgl), sptField,
                     FxDB(dr("jam1"), ""), sptField,
                     FxDB(dr("jam2"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), ""), sptField,
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
                     FxDB(dr("nopromo"), ""), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("namabarang"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("stid, stcabang, stlokasi, stsumber, stkategoripos, stautonotransaksi, stnotransaksi, sttgl, stkodepa, stkontak, stkontakperson, sturaian, stcatatan, ststatus, ststatussebelumnya, stjmlrevisi, stcetakanke, stisclose, stinputuser, stinputtgl, stmodifikasiuser, stmodifikasitgl, stposting, stpostingtgl, stcustomtext1, stcustomtext2, stcustomtext3, stcustomtext4, stcustomtext5, stcustomint1, stcustomint2, stcustomint3, stcustomdbl1, stcustomdbl2, stcustomdbl3, stcustomdate1, stcustomdate2, stcustomdate3, stcabangnama, stlokasinama, stkontakkode, stkontaknama, ststatusnama, ststatussebelumnyanama, stinputusernama, stmodifikasiusernama, stkategoriposnama, stjenispoint" & sptSubParam & "idstdetail, idst, stkategori, idbarang, operator, jml1, jml2, nilai, tgl1, tgl2, jam1, jam2, catatan, urutan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, nopromo, kodebarang, namabarang"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_StSearch(ByVal param As String) As String
        'M12_StSearch --------------------------------------------------------
        'stid, stcabang, stlokasi, stsumber, stkategoripos, stautonotransaksi, stnotransaksi, 
        'sttgl, stkodepa, stkontak, stkontakperson, sturaian, stcatatan, ststatus, 
        'ststatussebelumnya, stjmlrevisi, stcetakanke, stisclose, stinputuser, stinputtgl, stmodifikasiuser, 
        'stmodifikasitgl, stposting, stpostingtgl, stcustomtext1, stcustomtext2, stcustomtext3, stcustomtext4, 
        'stcustomtext5, stcustomint1, stcustomint2, stcustomint3, stcustomdbl1, stcustomdbl2, stcustomdbl3, 
        'stcustomdate1, stcustomdate2, stcustomdate3, stcabangnama, stlokasinama, stkontakkode, 
        'stkontaknama, ststatusnama, ststatussebelumnyanama, stinputusernama, stmodifikasiusernama

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
            formatTglWaktu = "yyy-MM-dd H:mm:ss"
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
        sql = "select `st`.`stid` AS `stid`, `st`.`stcabang` AS `stcabang`, `st`.`stlokasi` AS `stlokasi`, `st`.`stsumber` AS `stsumber`, `st`.`stautonotransaksi` AS `stautonotransaksi`, `st`.`stnotransaksi` AS `stnotransaksi`, `st`.`sttgl` AS `sttgl`, `st`.`stkodepa` AS `stkodepa`, `st`.`stkontak` AS `stkontak`, `st`.`stkontakperson` AS `stkontakperson`, `st`.`stkategoripos` AS `stkategoripos`, `st`.`sturaian` AS `sturaian`, `st`.`stcatatan` AS `stcatatan`, `st`.`ststatus` AS `ststatus`, `st`.`ststatussebelumnya` AS `ststatussebelumnya`, `st`.`stjmlrevisi` AS `stjmlrevisi`, `st`.`stcetakanke` AS `stcetakanke`, `st`.`stisclose` AS `stisclose`, `st`.`stinputuser` AS `stinputuser`, `st`.`stinputtgl` AS `stinputtgl`, `st`.`stmodifikasiuser` AS `stmodifikasiuser`, `st`.`stmodifikasitgl` AS `stmodifikasitgl`, `st`.`stposting` AS `stposting`, `st`.`stpostingtgl` AS `stpostingtgl`, `st`.`stcustomtext1` AS `stcustomtext1`, `st`.`stcustomtext2` AS `stcustomtext2`, `st`.`stcustomtext3` AS `stcustomtext3`, `st`.`stcustomtext4` AS `stcustomtext4`, `st`.`stcustomtext5` AS `stcustomtext5`, `st`.`stcustomint1` AS `stcustomint1`, `st`.`stcustomint2` AS `stcustomint2`, `st`.`stcustomint3` AS `stcustomint3`, `st`.`stcustomdbl1` AS `stcustomdbl1`, `st`.`stcustomdbl2` AS `stcustomdbl2`, `st`.`stcustomdbl3` AS `stcustomdbl3`, `st`.`stcustomdate1` AS `stcustomdate1`, `st`.`stcustomdate2` AS `stcustomdate2`, `st`.`stcustomdate3` AS `stcustomdate3`, `br`.`bnama` AS `stcabangnama`, `lc`.`lnama` AS `stlokasinama`, `c`.`kkode` AS `stkontakkode`, `c`.`knama` AS `stkontaknama`, `st1`.`nama` AS `ststatusnama`, `st2`.`nama` AS `ststatussebelumnyanama`, `u1`.`unama` AS `stinputusernama`, `u2`.`unama` AS `stmodifikasiusernama` from (((((((`m_12_st` `st` left join `m1_branch` `br` on((`st`.`stcabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`st`.`stlokasi` = `lc`.`lkode`))) left join `m1_contact` `c` on((`st`.`stkontak` = `c`.`kid`))) left join `m0_status` `st1` on((`st`.`ststatus` = `st1`.`kode`))) left join `m0_status` `st2` on((`st`.`ststatussebelumnya` = `st2`.`kode`))) left join `m0_user` `u1` on((`st`.`stinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`st`.`stmodifikasiuser` = `u2`.`userid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Cr~M2_Cr_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("stid"), 0), sptField,
                             FxDB(dr("stcabang"), ""), sptField,
                             FxDB(dr("stlokasi"), ""), sptField,
                             FxDB(dr("stsumber"), ""), sptField,
                             FxDB(dr("stkategoripos"), ""), sptField,
                             FxDB(dr("stautonotransaksi"), 0), sptField,
                             FxDB(dr("stnotransaksi"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("sttgl"), ""), formatTgl), sptField,
                             FxDB(dr("stkodepa"), ""), sptField,
                             FxDB(dr("stkontak"), ""), sptField,
                             FxDB(dr("stkontakperson"), ""), sptField,
                             FxDB(dr("sturaian"), ""), sptField,
                             FxDB(dr("stcatatan"), ""), sptField,
                             FxDB(dr("ststatus"), 0), sptField,
                             FxDB(dr("ststatussebelumnya"), 0), sptField,
                             FxDB(dr("stjmlrevisi"), 0), sptField,
                             FxDB(dr("stcetakanke"), 0), sptField,
                             FxDB(dr("stisclose"), 0), sptField,
                             FxDB(dr("stinputuser"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("stinputtgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("stmodifikasiuser"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("stmodifikasitgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("stposting"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("stpostingtgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("stcustomtext1"), ""), sptField,
                             FxDB(dr("stcustomtext2"), ""), sptField,
                             FxDB(dr("stcustomtext3"), ""), sptField,
                             FxDB(dr("stcustomtext4"), ""), sptField,
                             FxDB(dr("stcustomtext5"), ""), sptField,
                             FxDB(dr("stcustomint1"), 0), sptField,
                             FxDB(dr("stcustomint2"), 0), sptField,
                             FxDB(dr("stcustomint3"), 0), sptField,
                             FxDB(dr("stcustomdbl1"), 0), sptField,
                             FxDB(dr("stcustomdbl2"), 0), sptField,
                             FxDB(dr("stcustomdbl3"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("stcustomdate1"), ""), formatTgl), sptField,
                             AsFormatTanggal(FxDB(dr("stcustomdate2"), ""), formatTgl), sptField,
                             AsFormatTanggal(FxDB(dr("stcustomdate3"), ""), formatTgl), sptField,
                             FxDB(dr("stcabangnama"), ""), sptField,
                             FxDB(dr("stlokasinama"), ""), sptField,
                             FxDB(dr("stkontakkode"), ""), sptField,
                             FxDB(dr("stkontaknama"), ""), sptField,
                             FxDB(dr("ststatusnama"), ""), sptField,
                             FxDB(dr("ststatussebelumnyanama"), ""), sptField,
                             FxDB(dr("stinputusernama"), ""), sptField,
                             FxDB(dr("stmodifikasiusernama"), ""), sptRow)

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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("stid, stcabang, stlokasi, stsumber, stkategoripos, stautonotransaksi, stnotransaksi, sttgl, stkodepa, stkontak, stkontakperson, sturaian, stcatatan, ststatus, ststatussebelumnya, stjmlrevisi, stcetakanke, stisclose, stinputuser, stinputtgl, stmodifikasiuser, stmodifikasitgl, stposting, stpostingtgl, stcustomtext1, stcustomtext2, stcustomtext3, stcustomtext4, stcustomtext5, stcustomint1, stcustomint2, stcustomint3, stcustomdbl1, stcustomdbl2, stcustomdbl3, stcustomdate1, stcustomdate2, stcustomdate3, stcabangnama, stlokasinama, stkontakkode, stkontaknama, ststatusnama, ststatussebelumnyanama, stinputusernama, stmodifikasiusernama"))

        Return wsResult
    End Function

End Class
