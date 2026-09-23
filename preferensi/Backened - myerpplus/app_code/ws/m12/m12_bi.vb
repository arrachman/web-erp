Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_bi
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_BiSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataBonus(), dataRowBonus() As String

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
        'biid(0) As Integer, bicabang(1) As String, bilokasi(2) As String, bisumber(3) As String, bikategoripos(4) As String, 
        'biautonotransaksi(5) As Integer, binotransaksi(6) As String, bitgl(7) As Date, bikodepa(8) As , bikontak(9) As , 
        'bikontakperson(10) As String, biuraian(11) As String, bicatatan(12) As String, bistatus(13) As Integer, bistatussebelumnya(14) As Integer, 
        'bijmlrevisi(15) As Integer, bicetakanke(16) As Integer, biisclose(17) As Integer, biinputuser(18) As , biinputtgl(19) As DateTime, 
        'bimodifikasiuser(20) As , bimodifikasitgl(21) As DateTime, biposting(22) As Integer, bipostingtgl(23) As DateTime, bicustomtext1(24) As String, 
        'bicustomtext2(25) As String, bicustomtext3(26) As String, bicustomtext4(27) As String, bicustomtext5(28) As String, bicustomint1(29) As Integer, 
        'bicustomint2(30) As Integer, bicustomint3(31) As Integer, bicustomdbl1(32) As Double, bicustomdbl2(33) As Double, bicustomdbl3(34) As Double, 
        'bicustomdate1(35) As Date, bicustomdate2(36) As Date, bicustomdate3(37) As Date, bijeniskategori(38) As Integer, bijenis(39) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'biid, bicabang, bilokasi, bisumber, bikategoripos, biautonotransaksi, binotransaksi, 
        'bitgl, bikodepa, bikontak, bikontakperson, biuraian, bicatatan, bistatus, 
        'bistatussebelumnya, bijmlrevisi, bicetakanke, biisclose, biinputuser, biinputtgl, bimodifikasiuser, 
        'bimodifikasitgl, biposting, bipostingtgl, bicustomtext1, bicustomtext2, bicustomtext3, bicustomtext4, 
        'bicustomtext5, bicustomint1, bicustomint2, bicustomint3, bicustomdbl1, bicustomdbl2, bicustomdbl3, 
        'bicustomdate1, bicustomdate2, bicustomdate3, bijeniskategori, bijenis

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 40) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'biid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "biid required numeric." : GoTo selesai
        End If
        'biautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "biautonotransaksi required numeric." : GoTo selesai
        End If
        'bitgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "bitgl required date." : GoTo selesai
        End If
        'bistatus(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "bistatus required numeric." : GoTo selesai
        End If
        'bistatussebelumnya(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "bistatussebelumnya required numeric." : GoTo selesai
        End If
        'bijmlrevisi(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "bijmlrevisi required numeric." : GoTo selesai
        End If
        'bicetakanke(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "bicetakanke required numeric." : GoTo selesai
        End If
        'biisclose(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "biisclose required numeric." : GoTo selesai
        End If
        'biinputtgl(19) As DateTime
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "biinputtgl required date." : GoTo selesai
        End If
        'bimodifikasitgl(21) As DateTime
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "bimodifikasitgl required date." : GoTo selesai
        End If
        'biposting(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "biposting required numeric." : GoTo selesai
        End If
        'bipostingtgl(23) As DateTime
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "bipostingtgl required date." : GoTo selesai
        End If
        'bicustomint1(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "bicustomint1 required numeric." : GoTo selesai
        End If
        'bicustomint2(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "bicustomint2 required numeric." : GoTo selesai
        End If
        'bicustomint3(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "bicustomint3 required numeric." : GoTo selesai
        End If
        'bicustomdbl1(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "bicustomdbl1 required numeric." : GoTo selesai
        End If
        'bicustomdbl2(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "bicustomdbl2 required numeric." : GoTo selesai
        End If
        'bicustomdbl3(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "bicustomdbl3 required numeric." : GoTo selesai
        End If
        'bicustomdate1(35) As Date
        If (IsDate(dataUtama(35)) = False) Then
            result(2) = "bicustomdate1 required date." : GoTo selesai
        End If
        'bicustomdate2(36) As Date
        If (IsDate(dataUtama(36)) = False) Then
            result(2) = "bicustomdate2 required date." : GoTo selesai
        End If
        'bicustomdate3(37) As Date
        If (IsDate(dataUtama(37)) = False) Then
            result(2) = "bicustomdate3 required date." : GoTo selesai
        End If

        'bijeniskategori(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "bijeniskategori required numeric." : GoTo selesai
        End If

        'bijenis(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "bijenis required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'bicabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "bicabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "bicabang should not be more than 25 character." : GoTo selesai
        End If

        'bilokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "bilokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "bilokasi should not be more than 25 character." : GoTo selesai
        End If

        'bisumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "bisumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "bisumber should not be more than 10 character." : GoTo selesai
        End If

        'bikategoripos(4) As String
        'If Len(dataUtama(4)) = 0 Then
        '    result(2) = "bikategoripos can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(4)) > 50 Then
            result(2) = "bikategoripos should not be more than 50 character." : GoTo selesai
        End If

        'binotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "binotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "binotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'bitgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "bitgl can't be empty" : GoTo selesai
        End If

        'bikodepa(8) As 
        If Len(dataUtama(8)) = 0 Then
            result(2) = "bikodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 20 Then
            result(2) = "bikodepa should not be more than 20 character." : GoTo selesai
        End If

        'bikontak(9) As 
        If Len(dataUtama(9)) = 0 Then
            result(2) = "bikontak can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 20 Then
            result(2) = "bikontak should not be more than 20 character." : GoTo selesai
        End If

        'biinputtgl(19) As DateTime
        If Len(dataUtama(19)) = 0 Then
            result(2) = "biinputtgl can't be empty" : GoTo selesai
        End If

        'bimodifikasitgl(21) As DateTime
        If Len(dataUtama(21)) = 0 Then
            result(2) = "bimodifikasitgl can't be empty" : GoTo selesai
        End If

        'bipostingtgl(23) As DateTime
        If Len(dataUtama(23)) = 0 Then
            result(2) = "bipostingtgl can't be empty" : GoTo selesai
        End If

        'bicustomdbl1(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "bicustomdbl1 can't be empty" : GoTo selesai
        End If

        'bicustomdbl2(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "bicustomdbl2 can't be empty" : GoTo selesai
        End If

        'bicustomdbl3(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "bicustomdbl3 can't be empty" : GoTo selesai
        End If

        'bicustomdate1(35) As Date
        If Len(dataUtama(35)) = 0 Then
            result(2) = "bicustomdate1 can't be empty" : GoTo selesai
        End If

        'bicustomdate2(36) As Date
        If Len(dataUtama(36)) = 0 Then
            result(2) = "bicustomdate2 can't be empty" : GoTo selesai
        End If

        'bicustomdate3(37) As Date
        If Len(dataUtama(37)) = 0 Then
            result(2) = "bicustomdate3 can't be empty" : GoTo selesai
        End If



        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "biid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bilokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bisumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bikategoripos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "biautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "binotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bikodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bikontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bikontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "biuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bistatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bistatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bijmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bicetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "biisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "biinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "biinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bimodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bimodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "biposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bipostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bijeniskategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bijenis", AsEnumTypeData.AsInt64)

        If AsDataTableTambahData(dtutama, "biid~bicabang~bilokasi~bisumber~bikategoripos~biautonotransaksi~binotransaksi~bitgl~bikodepa~bikontak~bikontakperson~biuraian~bicatatan~bistatus~bistatussebelumnya~bijmlrevisi~bicetakanke~biisclose~biinputuser~biinputtgl~bimodifikasiuser~bimodifikasitgl~biposting~bipostingtgl~bicustomtext1~bicustomtext2~bicustomtext3~bicustomtext4~bicustomtext5~bicustomint1~bicustomint2~bicustomint3~bicustomdbl1~bicustomdbl2~bicustomdbl3~bicustomdate1~bicustomdate2~bicustomdate3~bijeniskategori~bijenis", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39)) = False Then
            result(2) = "insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idbidetail(0) As , idbi(1) As , bikategori(2) As String, idbarang(3) As , operator(4) As String, 
        'jml1(5) As Double, jml2(6) As Double, customtext1(7) As String, customtext2(8) As String, customtext3(9) As String, 
        'customtext4(10) As String, customtext5(11) As String, customint1(12) As Integer, customint2(13) As Integer, customint3(14) As Integer, 
        'customdbl1(15) As Double, customdbl2(16) As Double, customdbl3(17) As Double, customdate1(18) As Date, customdate2(19) As Date, 
        'customdate3(20) As Date, tgl1(21) As Date, tgl2(22) As Date, nopromo(23) As String, nogrup (24) As String, catatan (25) As String, urutan(26) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idbidetail, idbi, bikategori, idbarang, operator, jml1, jml2, 
        'customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, 
        'customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'tgl1, tgl2, nopromo, nogrup, catatan, urutan

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idbidetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idbi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "bikategori", AsEnumTypeData.AsString)
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
                result(2) = "Row : " & i & " - idbidetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idbidetail should not be more than 20 character." : GoTo selesai
            End If

            'idbi(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idbi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idbi should not be more than 20 character." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idbidetail~idbi~bikategori~idbarang~operator~jml1~jml2~customtext1~customtext2~customtext3~customtext4~customtext5~customint1~customint2~customint3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~tgl1~tgl2~nopromo~nogrup~catatan~urutan", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26)) = False Then
                result(2) = "Detail Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================

        'MAPPING BUAT WS DATA BONUS -------------------------------------------------------
        'idbonus(0) As , idbi(1) As , idbidetail(2) As , idbarang(3) As , jml(4) As Double, 
        'satuan(5) As String, customtext1(6) As String, customtext2(7) As String, customtext3(8) As String, customtext4(9) As String, 
        'customtext5(10) As String, customint1(11) As Integer, customint2(12) As Integer, customint3(13) As Integer, customdbl1(14) As Double, 
        'customdbl2(15) As Double, customdbl3(16) As Double, customdate1(17) As Date, customdate2(18) As Date, customdate3(19) As Date, nogrup(20) As String

        'MAPPING BUAT FLEX DATA BONUS -----------------------------------------------------
        'idbonus, idbi, idbidetail, idbarang, jml, satuan, customtext1, 
        'customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA BONUS ======================================================
        'SPLIT PARAMETER DATA BONUS
        dataBonus = dataSplit(2).Split(sptRow)
        'END OF VALIDASI DAN SET DATA BONUS ===============================================

        'Buat datatable bonus
        Dim dtbonus As New DataTable
        AsDataTableTambahField(dtbonus, "idbonus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbonus, "idbi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbonus, "idbidetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbonus, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbonus, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "customint1", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtbonus, "customint2", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtbonus, "customint3", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtbonus, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "nogrup", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "urutan", AsEnumTypeData.AsInt64)

        'VALIDASI DAN SET DATA ROW BONUS ==================================================
        Dim JmlDtBonus As Integer = dataBonus.Length
        For i = 1 To JmlDtBonus
            'SPLIT DATA DETAIL
            dataRowBonus = dataBonus(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA BONUS -----------------------------------
            'CEK ARRAY DATA BONUS
            If (dataRowBonus.Length <> 22) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW BONUS ----------------------------

            'VALIDASI TIPE DATA BONUS ------------------------------------------
            'jml(4) As Double
            If (IsNumeric(dataRowBonus(4)) = False) Then
                result(2) = "jml required numeric." : GoTo selesai
            End If
            'customint1(11) As Integer
            If (IsNumeric(dataRowBonus(11)) = False) Then
                result(2) = "customint1 required numeric." : GoTo selesai
            End If
            'customint2(12) As Integer
            If (IsNumeric(dataRowBonus(12)) = False) Then
                result(2) = "customint2 required numeric." : GoTo selesai
            End If
            'customint3(13) As Integer
            If (IsNumeric(dataRowBonus(13)) = False) Then
                result(2) = "customint3 required numeric." : GoTo selesai
            End If
            'customdbl1(14) As Double
            If (IsNumeric(dataRowBonus(14)) = False) Then
                result(2) = "customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(15) As Double
            If (IsNumeric(dataRowBonus(15)) = False) Then
                result(2) = "customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(16) As Double
            If (IsNumeric(dataRowBonus(16)) = False) Then
                result(2) = "customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(17) As Date
            If (IsDate(dataRowBonus(17)) = False) Then
                result(2) = "customdate1 required date." : GoTo selesai
            End If
            'customdate2(18) As Date
            If (IsDate(dataRowBonus(18)) = False) Then
                result(2) = "customdate2 required date." : GoTo selesai
            End If
            'customdate3(19) As Date
            If (IsDate(dataRowBonus(19)) = False) Then
                result(2) = "customdate3 required date." : GoTo selesai
            End If
            'urutan(21) As Double
            If (IsNumeric(dataRowBonus(21)) = False) Then
                result(2) = "urutan required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA BONUS -----------------------------------

            'VALIDASI DATA BONUS ---------------------------------------
            'idbonus(0) As 
            If Len(dataRowBonus(0)) = 0 Then
                result(2) = "Row : " & i & " - idbonus can't be empty" : GoTo selesai
            End If
            If Len(dataRowBonus(0)) > 20 Then
                result(2) = "Row : " & i & " - idbonus should not be more than 20 character." : GoTo selesai
            End If

            'idbi(1) As 
            If Len(dataRowBonus(1)) = 0 Then
                result(2) = "Row : " & i & " - idbi can't be empty" : GoTo selesai
            End If
            If Len(dataRowBonus(1)) > 20 Then
                result(2) = "Row : " & i & " - idbi should not be more than 20 character." : GoTo selesai
            End If

            'idbidetail(2) As 
            If Len(dataRowBonus(2)) = 0 Then
                result(2) = "Row : " & i & " - idbidetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowBonus(2)) > 20 Then
                result(2) = "Row : " & i & " - idbidetail should not be more than 20 character." : GoTo selesai
            End If

            'idbarang(3) As 
            If Len(dataRowBonus(3)) = 0 Then
                result(2) = "Row : " & i & " - idbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowBonus(3)) > 20 Then
                result(2) = "Row : " & i & " - idbarang should not be more than 20 character." : GoTo selesai
            End If

            'jml(4) As Double
            If Len(dataRowBonus(4)) = 0 Then
                result(2) = "Row : " & i & " - jml can't be empty" : GoTo selesai
            End If

            'satuan(5) As String
            If Len(dataRowBonus(5)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowBonus(5)) > 25 Then
                result(2) = "Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'customdbl1(14) As Double
            If Len(dataRowBonus(14)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(15) As Double
            If Len(dataRowBonus(15)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(16) As Double
            If Len(dataRowBonus(16)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(17) As Date
            If Len(dataRowBonus(17)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(18) As Date
            If Len(dataRowBonus(18)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(19) As Date
            If Len(dataRowBonus(19)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'urutan(21) As Date
            If Len(dataRowBonus(21)) = 0 Then
                result(2) = "Row : " & i & " - urutan can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtbonus, "idbonus~idbi~idbidetail~idbarang~jml~satuan~customtext1~customtext2~customtext3~customtext4~customtext5~customint1~customint2~customint3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~nogrup~urutan", dataRowBonus(0) & "~" & dataRowBonus(1) & "~" & dataRowBonus(2) & "~" & dataRowBonus(3) & "~" & dataRowBonus(4) & "~" & dataRowBonus(5) & "~" & dataRowBonus(6) & "~" & dataRowBonus(7) & "~" & dataRowBonus(8) & "~" & dataRowBonus(9) & "~" & dataRowBonus(10) & "~" & dataRowBonus(11) & "~" & dataRowBonus(12) & "~" & dataRowBonus(13) & "~" & dataRowBonus(14) & "~" & dataRowBonus(15) & "~" & dataRowBonus(16) & "~" & dataRowBonus(17) & "~" & dataRowBonus(18) & "~" & dataRowBonus(19) & "~" & dataRowBonus(20) & "~" & dataRowBonus(21)) = False Then
                result(2) = "Bonus Row : " & i & " - insert into datatable failed." : GoTo selesai
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
                Dim vModuleId As Integer = 12, vMenuId As Integer = 54
                Select Case drutama("bistatus")
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
                    result(4) = drutama("biid")
                    notransaksi = drutama("binotransaksi")

                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(biid), binotransaksi FROM M_12_Bi WHERE biid=" & result(4), myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(biid) FROM M_12_Bi WHERE binotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m12_bi_history
                        Dim rsSimpanHistory As String = SimpanHistory.M12_Bi_HistorySimpan("" & paramSplit(0) & "★M12_Bi_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("bisumber")) & "▼" & FixQuotes(drutama("biid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M_12_Bi set bicabang  = '" & FixQuotes(drutama("bicabang")) & "', bilokasi  = '" & FixQuotes(drutama("bilokasi")) & "', bisumber  = '" & FixQuotes(drutama("bisumber")) & "', bikategoripos  = '" & FixQuotes(drutama("bikategoripos")) & "', biautonotransaksi  = " & drutama("biautonotransaksi") & ", binotransaksi  = '" & FixQuotes(drutama("binotransaksi")) & "', bitgl  = '" & FixQuotes(AsFormatTanggal(drutama("bitgl"))) & "', bikodepa  = '" & FixQuotes(drutama("bikodepa")) & "', bikontak  = '" & FixQuotes(drutama("bikontak")) & "', bikontakperson  = '" & FixQuotes(drutama("bikontakperson")) & "', biuraian  = '" & FixQuotes(drutama("biuraian")) & "', bicatatan  = '" & FixQuotes(drutama("bicatatan")) & "', bistatus  = " & drutama("bistatus") & ", bistatussebelumnya  = " & drutama("bistatussebelumnya") & ", bijmlrevisi  = " & drutama("bijmlrevisi") & ", bicetakanke  = " & drutama("bicetakanke") & ", biisclose  = " & drutama("biisclose") & ", biinputuser  = '" & FixQuotes(drutama("biinputuser")) & "', bimodifikasiuser  = '" & FixQuotes(drutama("bimodifikasiuser")) & "', bimodifikasitgl  = NOW(), biposting  = " & drutama("biposting") & ", bipostingtgl  = '" & FixQuotes(AsFormatTanggal(drutama("bipostingtgl"), "yyyy-MM-dd H:mm:ss")) & "', bicustomtext1  = '" & FixQuotes(drutama("bicustomtext1")) & "', bicustomtext2  = '" & FixQuotes(drutama("bicustomtext2")) & "', bicustomtext3  = '" & FixQuotes(drutama("bicustomtext3")) & "', bicustomtext4  = '" & FixQuotes(drutama("bicustomtext4")) & "', bicustomtext5  = '" & FixQuotes(drutama("bicustomtext5")) & "', bicustomint1  = " & drutama("bicustomint1") & ", bicustomint2  = " & drutama("bicustomint2") & ", bicustomint3  = " & drutama("bicustomint3") & ", bicustomdbl1  = '" & FixDouble(drutama("bicustomdbl1")) & "', bicustomdbl2  = '" & FixDouble(drutama("bicustomdbl2")) & "', bicustomdbl3  = '" & FixDouble(drutama("bicustomdbl3")) & "', bicustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("bicustomdate1"))) & "', bicustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("bicustomdate2"))) & "', bicustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("bicustomdate3"))) & "', bijeniskategori  = '" & FixQuotes(drutama("bijeniskategori")) & "', bijenis = '" & FixQuotes(drutama("bijenis")) & "' where biid = " & drutama("biid") & ""
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

                    If drutama("biautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("bicabang"), drutama("bilokasi"), drutama("bisumber"), drutama("bitgl"))
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
                        notransaksi = drutama("binotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(biid) FROM m_12_bi WHERE binotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M_12_Bi (bicabang, bilokasi, bisumber, bikategoripos, biautonotransaksi, binotransaksi, bitgl, bikodepa, bikontak, bikontakperson, biuraian, bicatatan, bistatus, bistatussebelumnya, bijmlrevisi, bicetakanke, biisclose, biinputuser, biinputtgl, bimodifikasiuser, bimodifikasitgl, biposting, bipostingtgl, bicustomtext1, bicustomtext2, bicustomtext3, bicustomtext4, bicustomtext5, bicustomint1, bicustomint2, bicustomint3, bicustomdbl1, bicustomdbl2, bicustomdbl3, bicustomdate1, bicustomdate2, bicustomdate3, bijeniskategori, bijenis) values('" & FixQuotes(drutama("bicabang")) & "', '" & FixQuotes(drutama("bilokasi")) & "', '" & FixQuotes(drutama("bisumber")) & "', '" & FixQuotes(drutama("bikategoripos")) & "', " & drutama("biautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("bitgl"))) & "', '" & FixQuotes(drutama("bikodepa")) & "', '" & FixQuotes(drutama("bikontak")) & "', '" & FixQuotes(drutama("bikontakperson")) & "', '" & FixQuotes(drutama("biuraian")) & "', '" & FixQuotes(drutama("bicatatan")) & "', " & drutama("bistatus") & ", " & drutama("bistatussebelumnya") & ", " & drutama("bijmlrevisi") & ", " & drutama("bicetakanke") & ", " & drutama("biisclose") & ", '" & FixQuotes(drutama("biinputuser")) & "', NOW(), '" & FixQuotes(drutama("bimodifikasiuser")) & "', '1971-01-01 00:00:00', 0, '1971-01-01 00:00:00', '" & FixQuotes(drutama("bicustomtext1")) & "', '" & FixQuotes(drutama("bicustomtext2")) & "', '" & FixQuotes(drutama("bicustomtext3")) & "', '" & FixQuotes(drutama("bicustomtext4")) & "', '" & FixQuotes(drutama("bicustomtext5")) & "', " & drutama("bicustomint1") & ", " & drutama("bicustomint2") & ", " & drutama("bicustomint3") & ", '" & FixDouble(drutama("bicustomdbl1")) & "', '" & FixDouble(drutama("bicustomdbl2")) & "', '" & FixDouble(drutama("bicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("bicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("bicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("bicustomdate3"))) & "', " & drutama("bijeniskategori") & ", " & drutama("bijenis") & ")"
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
                    dt2 = AsDataTableAmbilDariDBCon("select biid from M_12_bi where binotransaksi='" & notransaksi & "' AND biinputuser= '" & drutama("biinputuser") & "' order by bimodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M_12_Bi_Detail where idbi = " & result(4)
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Hapus bonus ketika update
                If (isUpdate) Then
                    sql = "Delete from M_12_Bi_Bonus where idbi = " & result(4)
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
                    Dim dtBonusGrup As New DataTable

                    For Each dr1 As DataRow In dtdetail.Rows

                        'CEK OPERATOR :
                        'JIKA BETWEEN (0) ATAU >= (1) MAKA BOLEH LEBIH DARI SATU KONDISI 
                        '=> BEBERAPA KONDISI BETWEEN DAN SATU KONDISI >= (1)
                        'JIKA KELIPATAN (2) MAKA HANYA BOLEH SATU KONDISI
                        Dim dtOperator As New DataTable
                        sql = "SELECT bid.bikategori as kategori, bid.idbarang as idbarang, bid.operator as operator, i.bkode, (CASE bid.operator WHEN 0 THEN 'Between' WHEN 1 THEN '>=' WHEN 2 THEN 'Multiple' ELSE 'Unknown' END) as operatornama FROM m_12_bi_detail bid LEFT JOIN m1_item i ON bid.idbarang = i.bid WHERE bid.bikategori = '" & FxDB(drutama("bikategoripos"), "") & "' AND bid.idbarang = '" & FxDB(dr1("idbarang"), "") & "' AND bid.idbi = '" & result(4) & "' AND bid.idbidetail <> '" & FxDB(dr1("idbidetail"), "") & "' GROUP BY bid.operator ORDER BY bid.operator"
                        dtOperator = AsDataTableAmbilDariDBCon(sql, myConn)
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
                        strValue2.Append("('" & FixQuotes(dr1("idbidetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("bikategori")) & "', '" & FixQuotes(dr1("idbarang")) & "', '" & FixQuotes(dr1("operator")) & "', '" & FixDouble(dr1("jml1")) & "', '" & FixDouble(dr1("jml2")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl2"))) & "', '" & FixQuotes(dr1("nopromo")) & "')")

                        'sql = "Insert into M_12_Bi_Detail(idbidetail, idbi, bikategori, idbarang, operator, jml1, jml2, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, tgl1, tgl2, nopromo) values" & strValue2.ToString & ""
                        sql = "Insert into M_12_Bi_Detail(idbidetail, idbi, bikategori, idbarang, operator, jml1, jml2, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, tgl1, tgl2, nopromo, catatan, urutan) values('" & FixQuotes(dr1("idbidetail")) & "', " & result(4) & ", '" & FixQuotes(drutama("bikategoripos")) & "', '" & FixQuotes(dr1("idbarang")) & "', '" & FixQuotes(dr1("operator")) & "', '" & FixDouble(dr1("jml1")) & "', '" & FixDouble(dr1("jml2")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl2"))) & "', '" & notransaksi & "', '" & FixQuotes(dr1("catatan")) & "','" & FixQuotes(dr1("urutan")) & "')"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()


                        'ambil ID detail untuk diinsert ke bonus
                        Dim iddetail As Integer
                        Dim dtidbonus As New DataTable
                        'Sql disesuaikan sendiri, untuk parameternya disesuaikan sendiri.
                        dtidbonus = AsDataTableAmbilDariDBCon("select idbidetail from M_12_bi_detail where idbi='" & result(4) & "' and bikategori = '" & drutama("bikategoripos") & "' AND  idbarang = '" & dr1("idbarang") & "' AND  operator = '" & dr1("operator") & "' AND  jml1 = '" & dr1("jml1") & "' AND jml2 = '" & dr1("jml2") & "' order by idbidetail desc limit 1", myConn)
                        If dtidbonus.Rows.Count > 0 Then iddetail = dtidbonus.Rows(0)(0) Else result(2) = "#1 Detail transaction data not found." : Trans.Rollback() : GoTo selesai

                        'Proses Bonus
                        If (dtbonus.Rows.Count > 0) Then
                            'AMBIL DETAIL BONUS SESUAI NO GRUP
                            dtBonusGrup = AsDataTableFilterSortDt(dtbonus, "nogrup = '" & dr1("nogrup") & "'")
                            If (dtBonusGrup.Rows.Count > 0) Then
                                strValue2.Clear()
                                For Each drBonusGrup As DataRow In dtBonusGrup.Rows
                                    strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                                    strValue2.Append("('" & FixQuotes(drBonusGrup("idbonus")) & "', " & result(4) & ", '" & iddetail & "', '" & FixQuotes(drBonusGrup("idbarang")) & "', '" & FixDouble(drBonusGrup("jml")) & "', '" & FixQuotes(drBonusGrup("satuan")) & "', '" & FixQuotes(drBonusGrup("customtext1")) & "', '" & FixQuotes(drBonusGrup("customtext2")) & "', '" & FixQuotes(drBonusGrup("customtext3")) & "', '" & FixQuotes(drBonusGrup("customtext4")) & "', '" & FixQuotes(drBonusGrup("customtext5")) & "', " & drBonusGrup("customint1") & ", " & drBonusGrup("customint2") & ", " & drBonusGrup("customint3") & ", '" & FixDouble(drBonusGrup("customdbl1")) & "', '" & FixDouble(drBonusGrup("customdbl2")) & "', '" & FixDouble(drBonusGrup("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drBonusGrup("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drBonusGrup("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drBonusGrup("customdate3"))) & "', '" & FixQuotes(drBonusGrup("urutan")) & "')")
                                Next

                                sql = "Insert into M_12_Bi_Bonus(idbonus, idbi, idbidetail,  idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, urutan) values" & strValue2.ToString & ""
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                            Else
                                result(2) = "Bonus Transaction for No. Group : " & dr1("nogrup") & " does not found." : Trans.Rollback() : GoTo selesai
                            End If

                        Else
                            result(2) = "Bonus Transaction data not found." : Trans.Rollback() : GoTo selesai
                        End If
                    Next

                Else
                    result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Update ke tabel Barang Bonus
                If drutama("bistatus") = 2 Then
                    Dim nmTabel As String = ""
                    If drutama("bijenis") = 0 Then
                        nmTabel = "M_12_Pos_Bonus_Item"
                    Else
                        nmTabel = "M_12_Pos_Bonus_Trans"
                    End If

                    'JIKA PER KATEGORI, HAPUS DATA PER KATEGORI
                    If drutama("bijeniskategori") = 1 Then
                        'Cek apakah kategori pos sudah ada di tabel pos_bonus_item, jika sudah ada, hapus data di tabel itu
                        Dim dtPOSBonusItem As New DataTable
                        dtPOSBonusItem = AsDataTableAmbilDariDBCon("select biid from " & nmTabel & " where bikategori = '" & drutama("bikategoripos") & "'", myConn)
                        Dim strValueItemUtama As New StringBuilder
                        Dim strValueItemDetail As New StringBuilder
                        If dtPOSBonusItem.Rows.Count > 0 Then
                            For Each drPOSBonusItem As DataRow In dtPOSBonusItem.Rows
                                'QUERY HAPUS POS BONUS ITEM UTAMA
                                strValueItemUtama.Append(IIf(Len(strValueItemUtama.ToString) = 0, "", " OR "))
                                strValueItemUtama.Append("biid = '" & FixQuotes(drPOSBonusItem("biid")) & "'")

                                'QUERY HAPUS POS BONUS ITEM DETAIL
                                strValueItemDetail.Append(IIf(Len(strValueItemDetail.ToString) = 0, "", " OR "))
                                strValueItemDetail.Append("idbi = '" & FixQuotes(drPOSBonusItem("biid")) & "'")
                            Next

                            'HAPUS POS BONUS ITEM UTAMA
                            sql = "Delete From " & nmTabel & " where " & strValueItemUtama.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                            'HAPUS POS BONUS ITEM DETAIL
                            sql = "Delete From " & nmTabel & "_detail where " & strValueItemDetail.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If

                    ElseIf drutama("bijeniskategori") = 2 Then 'JIKA PER CABANG, HAPUS DATA PER KATEGORI SESUAI CABANG
                        'ambil kategori pos sesuai cabang
                        Dim dtCatPOS As DataTable = AsDataTableAmbilDariDBCon("SELECT GROUP_CONCAT(" & Chr(34) & "'" & Chr(34) & ",l.lkategoripos," & Chr(34) & "'" & Chr(34) & ") as kategoripos FROM m1_location l WHERE l.lkategoripos <> '' AND l.lcabang = '" & FixQuotes(drutama("bicabang")) & "'", myConn)
                        If dtCatPOS.Rows.Count > 0 Then
                            If Len(FxDB(dtCatPOS.Rows(0)(0), "")) > 0 Then
                                'Cek apakah kategori pos sudah ada di tabel pos_bonus_item, jika sudah ada, hapus data di tabel itu
                                Dim dtPOSBonusItem As New DataTable
                                dtPOSBonusItem = AsDataTableAmbilDariDBCon("select biid from " & nmTabel & " where bikategori IN (" & dtCatPOS.Rows(0)(0) & ")", myConn)
                                Dim strValueItemUtama As New StringBuilder
                                Dim strValueItemDetail As New StringBuilder
                                If dtPOSBonusItem.Rows.Count > 0 Then
                                    For Each drPOSBonusItem As DataRow In dtPOSBonusItem.Rows
                                        'QUERY HAPUS POS BONUS ITEM UTAMA
                                        strValueItemUtama.Append(IIf(Len(strValueItemUtama.ToString) = 0, "", " OR "))
                                        strValueItemUtama.Append("biid = '" & FixQuotes(drPOSBonusItem("biid")) & "'")

                                        'QUERY HAPUS POS BONUS ITEM DETAIL
                                        strValueItemDetail.Append(IIf(Len(strValueItemDetail.ToString) = 0, "", " OR "))
                                        strValueItemDetail.Append("idbi = '" & FixQuotes(drPOSBonusItem("biid")) & "'")
                                    Next

                                    'HAPUS POS BONUS ITEM UTAMA
                                    sql = "Delete From " & nmTabel & " where " & strValueItemUtama.ToString & ""
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()

                                    'HAPUS POS BONUS ITEM DETAIL
                                    sql = "Delete From " & nmTabel & "_detail where " & strValueItemDetail.ToString & ""
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
                        sql = "Delete From " & nmTabel & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'HAPUS POS BONUS ITEM DETAIL
                        sql = "Delete From " & nmTabel & "_detail"
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
                    dtdtl = AsDataTableAmbilDariDBCon("select * from M_12_Bi_Detail where idbi = '" & result(4) & "' order by idbi asc", myConn)
                    Dim dtbibonus As New DataTable
                    'AMBIL DATA BI BONUS
                    dtbibonus = AsDataTableAmbilDariDBCon("select * from M_12_Bi_Bonus where idbi = '" & result(4) & "' order by idbi asc", myConn)

                    Dim strValueInsertBonusItem As New StringBuilder 'untuk query simpan di tabel bonus utama
                    Dim strValueBonusItemDetail As New StringBuilder 'untuk query simpan di tabel bonus detail
                    Dim idposbonusitem As Integer 'untuk variabel id transaksi pos bonus item utama
                    Dim dtselectId As New DataTable 'untuk query ambil id transaksi pos bonus item
                    Dim dtBonusPenampung As New DataTable 'untuk menampung data bi bonus
                    Dim dtKatPOS As New DataTable 'untuk menampung data kategori pos, jika jenis kategori 
                    strValueBonusItemDetail.Clear()

                    If dtdtl.Rows.Count > 0 Then

                        If drutama("bijeniskategori") = 1 Then 'JIKA PER KATEGORI
                            For Each drdtl As DataRow In dtdtl.Rows
                                'persiapan insert ke tabel m_12_pos_bonus_item & tabel m_12_pos_bonus_item_detail
                                strValueInsertBonusItem.Append(IIf(Len(strValueInsertBonusItem.ToString) = 0, "", ", "))
                                strValueInsertBonusItem.Append("('" & FixQuotes(drutama("bikategoripos")) & "', '" & FixQuotes(drdtl("idbarang")) & "', '" & FixQuotes(drdtl("operator")) & "', '" & FixDouble(drdtl("jml1")) & "', '" & FixDouble(drdtl("jml2")) & "', '" & FixQuotes(drdtl("customtext1")) & "', '" & FixQuotes(drdtl("customtext2")) & "', '" & FixQuotes(drdtl("customtext3")) & "', '" & FixQuotes(drdtl("customtext4")) & "', '" & FixQuotes(drdtl("customtext5")) & "', " & drdtl("customint1") & ", " & drdtl("customint2") & ", " & drdtl("customint3") & ", '" & FixDouble(drdtl("customdbl1")) & "', '" & FixDouble(drdtl("customdbl2")) & "', '" & FixDouble(drdtl("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl2"))) & "', '" & FixQuotes(drdtl("nopromo")) & "')")
                            Next

                            'insert ke tabel m_12_pos_bonus_item
                            sql = "Insert into " & nmTabel & " (bikategori, biidbarang, bioperator, bijml1, bijml2, bicustomtext1, bicustomtext2, bicustomtext3, bicustomtext4, bicustomtext5, bicustomint1, bicustomint2, bicustomint3, bicustomdbl1, bicustomdbl2, bicustomdbl3, bicustomdate1, bicustomdate2, bicustomdate3, bitgl1, bitgl2, binopromo) values " & strValueInsertBonusItem.ToString & ""
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
                                dtselectId = AsDataTableAmbilDariDBCon("select biid from " & nmTabel & " where bikategori = '" & drdtl2("bikategori") & "' AND biidbarang = '" & drdtl2("idbarang") & "' AND bioperator = '" & drdtl2("operator") & "' AND bijml1 = '" & drdtl2("jml1") & "' AND bijml2 = '" & drdtl2("jml2") & "' limit 1", myConn)
                                If dtselectId.Rows.Count > 0 Then idposbonusitem = dtselectId.Rows(0)(0) Else result(2) = "Main POS Bonus Item transaction data not found." : Trans.Rollback() : GoTo selesai

                                'filter data bonus penampung, untuk dijadikan parameter simpan ke tabel pos bonus detail
                                dtBonusPenampung = AsDataTableFilterSortDt(dtbibonus, "idbidetail = '" & drdtl2("idbidetail") & "'")
                                If dtBonusPenampung.Rows.Count > 0 Then
                                    For Each drBonusPenampung As DataRow In dtBonusPenampung.Rows
                                        'parameter simpan ke tabel m_12_pos_bonus_item_DETAIL
                                        strValueBonusItemDetail.Append(IIf(Len(strValueBonusItemDetail.ToString) = 0, "", ", "))
                                        strValueBonusItemDetail.Append("(" & idposbonusitem & ", '" & FixQuotes(drBonusPenampung("idbarang")) & "', '" & FixDouble(drBonusPenampung("jml")) & "', '" & FixQuotes(drBonusPenampung("satuan")) & "', '" & FixQuotes(drBonusPenampung("customtext1")) & "', '" & FixQuotes(drBonusPenampung("customtext2")) & "', '" & FixQuotes(drBonusPenampung("customtext3")) & "', '" & FixQuotes(drBonusPenampung("customtext4")) & "', '" & FixQuotes(drBonusPenampung("customtext5")) & "', " & drBonusPenampung("customint1") & ", " & drBonusPenampung("customint2") & ", " & drBonusPenampung("customint3") & ", '" & FixDouble(drBonusPenampung("customdbl1")) & "', '" & FixDouble(drBonusPenampung("customdbl2")) & "', '" & FixDouble(drBonusPenampung("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drBonusPenampung("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drBonusPenampung("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drBonusPenampung("customdate3"))) & "')")
                                    Next
                                Else
                                    result(2) = "Main Transaction POS Bonus Item data not found." : Trans.Rollback() : GoTo selesai
                                End If
                            Next

                            'INSERT KE TABEL POS BONUS DETAIL
                            sql = "Insert into " & nmTabel & "_Detail(idbi, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValueBonusItemDetail.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                        ElseIf drutama("bijeniskategori") = 2 Then 'JIKA PER CABANG
                            'ambil kategori pos sesuai cabang
                            Dim dtCatPOS As DataTable = AsDataTableAmbilDariDBCon("SELECT GROUP_CONCAT(" & Chr(34) & "'" & Chr(34) & ",l.lkategoripos," & Chr(34) & "'" & Chr(34) & ") as kategoripos FROM m1_location l WHERE l.lkategoripos <> '' AND l.lcabang = '" & FixQuotes(drutama("bicabang")) & "'", myConn)
                            If dtCatPOS.Rows.Count > 0 Then
                                If Len(FxDB(dtCatPOS.Rows(0)(0), "")) > 0 Then
                                    Dim dtPosItem As New DataTable 'variabel untuk cari data barang pos
                                    'CARI DATA KATEGORI POS
                                    dtKatPOS = AsDataTableAmbilDariDBCon("select pckode from m_12_pos_category WHERE pckode IN (" & dtCatPOS.Rows(0)(0) & ")", myConn)
                                    If dtKatPOS.Rows.Count > 0 Then 'JIKA DATA KATEGORI POS ADA, AMBIL DATA BARANG POS
                                        For Each drKatPos As DataRow In dtKatPOS.Rows
                                            For Each drdtl As DataRow In dtdtl.Rows
                                                'persiapan insert ke tabel m_12_pos_bonus_item 
                                                strValueInsertBonusItem.Append(IIf(Len(strValueInsertBonusItem.ToString) = 0, "", ", "))
                                                strValueInsertBonusItem.Append("('" & FixQuotes(drKatPos("pckode")) & "', '" & FixQuotes(drdtl("idbarang")) & "', '" & FixQuotes(drdtl("operator")) & "', '" & FixDouble(drdtl("jml1")) & "', '" & FixDouble(drdtl("jml2")) & "', '" & FixQuotes(drdtl("customtext1")) & "', '" & FixQuotes(drdtl("customtext2")) & "', '" & FixQuotes(drdtl("customtext3")) & "', '" & FixQuotes(drdtl("customtext4")) & "', '" & FixQuotes(drdtl("customtext5")) & "', " & drdtl("customint1") & ", " & drdtl("customint2") & ", " & drdtl("customint3") & ", '" & FixDouble(drdtl("customdbl1")) & "', '" & FixDouble(drdtl("customdbl2")) & "', '" & FixDouble(drdtl("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl2"))) & "', '" & FixQuotes(drdtl("nopromo")) & "')")
                                            Next
                                        Next
                                    End If

                                    'insert ke tabel m_12_pos_bonus_item
                                    sql = "Insert into " & nmTabel & " (bikategori, biidbarang, bioperator, bijml1, bijml2, bicustomtext1, bicustomtext2, bicustomtext3, bicustomtext4, bicustomtext5, bicustomint1, bicustomint2, bicustomint3, bicustomdbl1, bicustomdbl2, bicustomdbl3, bicustomdate1, bicustomdate2, bicustomdate3, bitgl1, bitgl2, binopromo) values " & strValueInsertBonusItem.ToString & ""
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
                                                dtselectId = AsDataTableAmbilDariDBCon("select biid from " & nmTabel & " where bikategori = '" & drKatPos("pckode") & "' AND biidbarang = '" & drdtl2("idbarang") & "' AND bioperator = '" & drdtl2("operator") & "' AND bijml1 = '" & drdtl2("jml1") & "' AND bijml2 = '" & drdtl2("jml2") & "' limit 1", myConn)
                                                If dtselectId.Rows.Count > 0 Then idposbonusitem = dtselectId.Rows(0)(0) Else result(2) = "Main POS Bonus Item transaction data not found." : Trans.Rollback() : GoTo selesai
                                                'filter data bonus penampung, untuk dijadikan parameter simpan ke tabel pos bonus detail
                                                dtBonusPenampung = AsDataTableFilterSortDt(dtbibonus, "idbidetail = '" & drdtl2("idbidetail") & "'")
                                                If dtBonusPenampung.Rows.Count > 0 Then
                                                    For Each drBonusPenampung As DataRow In dtBonusPenampung.Rows
                                                        'persiapan insert ke tabel m_12_pos_bonus_item_DETAIL
                                                        strValueBonusItemDetail.Append(IIf(Len(strValueBonusItemDetail.ToString) = 0, "", ", "))
                                                        strValueBonusItemDetail.Append("(" & idposbonusitem & ", '" & FixQuotes(drBonusPenampung("idbarang")) & "', '" & FixDouble(drBonusPenampung("jml")) & "', '" & FixQuotes(drBonusPenampung("satuan")) & "', '" & FixQuotes(drBonusPenampung("customtext1")) & "', '" & FixQuotes(drBonusPenampung("customtext2")) & "', '" & FixQuotes(drBonusPenampung("customtext3")) & "', '" & FixQuotes(drBonusPenampung("customtext4")) & "', '" & FixQuotes(drBonusPenampung("customtext5")) & "', " & drBonusPenampung("customint1") & ", " & drBonusPenampung("customint2") & ", " & drBonusPenampung("customint3") & ", '" & FixDouble(drBonusPenampung("customdbl1")) & "', '" & FixDouble(drBonusPenampung("customdbl2")) & "', '" & FixDouble(drBonusPenampung("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drBonusPenampung("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drBonusPenampung("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drBonusPenampung("customdate3"))) & "')")
                                                    Next
                                                Else
                                                    result(2) = "Main Transaction POS Bonus Item data not found." : Trans.Rollback() : GoTo selesai
                                                End If
                                            Next
                                        Next

                                        'INSERT KE TABEL POS BONUS DETAIL
                                        sql = "Insert into " & nmTabel & "_Detail(idbi, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValueBonusItemDetail.ToString & ""
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
                                        'persiapan insert ke tabel m_12_pos_bonus_item 
                                        strValueInsertBonusItem.Append(IIf(Len(strValueInsertBonusItem.ToString) = 0, "", ", "))
                                        strValueInsertBonusItem.Append("('" & FixQuotes(drKatPos("pckode")) & "', '" & FixQuotes(drdtl("idbarang")) & "', '" & FixQuotes(drdtl("operator")) & "', '" & FixDouble(drdtl("jml1")) & "', '" & FixDouble(drdtl("jml2")) & "', '" & FixQuotes(drdtl("customtext1")) & "', '" & FixQuotes(drdtl("customtext2")) & "', '" & FixQuotes(drdtl("customtext3")) & "', '" & FixQuotes(drdtl("customtext4")) & "', '" & FixQuotes(drdtl("customtext5")) & "', " & drdtl("customint1") & ", " & drdtl("customint2") & ", " & drdtl("customint3") & ", '" & FixDouble(drdtl("customdbl1")) & "', '" & FixDouble(drdtl("customdbl2")) & "', '" & FixDouble(drdtl("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl2"))) & "', '" & FixQuotes(drdtl("nopromo")) & "')")
                                    Next
                                Next
                            End If

                            'insert ke tabel m_12_pos_bonus_item
                            sql = "Insert into " & nmTabel & " (bikategori, biidbarang, bioperator, bijml1, bijml2, bicustomtext1, bicustomtext2, bicustomtext3, bicustomtext4, bicustomtext5, bicustomint1, bicustomint2, bicustomint3, bicustomdbl1, bicustomdbl2, bicustomdbl3, bicustomdate1, bicustomdate2, bicustomdate3, bitgl1, bitgl2, binopromo) values " & strValueInsertBonusItem.ToString & ""
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
                                        dtselectId = AsDataTableAmbilDariDBCon("select biid from " & nmTabel & " where bikategori = '" & drKatPos("pckode") & "' AND biidbarang = '" & drdtl2("idbarang") & "' AND bioperator = '" & drdtl2("operator") & "' AND bijml1 = '" & drdtl2("jml1") & "' AND bijml2 = '" & drdtl2("jml2") & "' limit 1", myConn)
                                        If dtselectId.Rows.Count > 0 Then idposbonusitem = dtselectId.Rows(0)(0) Else result(2) = "Main POS Bonus Item transaction data not found." : Trans.Rollback() : GoTo selesai
                                        'filter data bonus penampung, untuk dijadikan parameter simpan ke tabel pos bonus detail
                                        dtBonusPenampung = AsDataTableFilterSortDt(dtbibonus, "idbidetail = '" & drdtl2("idbidetail") & "'")
                                        If dtBonusPenampung.Rows.Count > 0 Then
                                            For Each drBonusPenampung As DataRow In dtBonusPenampung.Rows
                                                'persiapan insert ke tabel m_12_pos_bonus_item_DETAIL
                                                strValueBonusItemDetail.Append(IIf(Len(strValueBonusItemDetail.ToString) = 0, "", ", "))
                                                strValueBonusItemDetail.Append("(" & idposbonusitem & ", '" & FixQuotes(drBonusPenampung("idbarang")) & "', '" & FixDouble(drBonusPenampung("jml")) & "', '" & FixQuotes(drBonusPenampung("satuan")) & "', '" & FixQuotes(drBonusPenampung("customtext1")) & "', '" & FixQuotes(drBonusPenampung("customtext2")) & "', '" & FixQuotes(drBonusPenampung("customtext3")) & "', '" & FixQuotes(drBonusPenampung("customtext4")) & "', '" & FixQuotes(drBonusPenampung("customtext5")) & "', " & drBonusPenampung("customint1") & ", " & drBonusPenampung("customint2") & ", " & drBonusPenampung("customint3") & ", '" & FixDouble(drBonusPenampung("customdbl1")) & "', '" & FixDouble(drBonusPenampung("customdbl2")) & "', '" & FixDouble(drBonusPenampung("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drBonusPenampung("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drBonusPenampung("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drBonusPenampung("customdate3"))) & "')")

                                            Next
                                        Else
                                            result(2) = "Main Transaction POS Bonus Item data not found." : Trans.Rollback() : GoTo selesai
                                        End If
                                    Next
                                Next

                                'INSERT KE TABEL POS BONUS DETAIL
                                sql = "Insert into " & nmTabel & "_Detail(idbi, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValueBonusItemDetail.ToString & ""
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
    Public Function M12_BiUpdateStatus(ByVal param As String) As String
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
            Filter = Filter.Replace("bikontakkode", "c.kkode")
            Filter = Filter.Replace("bikontaknama", "c.knama")
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
            Dim sumber As String = "BI", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Bitgl, Binotransaksi, Bistatus FROM m_12_Bi WHERE Biid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Bistatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m12_bi_history
            Dim rsSimpanHistory As String = SimpanHistory.M12_Bi_HistorySimpan("" & paramSplit(0) & "★M12_Bi_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================


            If isDelete Then
                'AMBIL JENIS KATEGORI UTAMA
                Dim dtutama As New DataTable
                dtutama = AsDataTableAmbilDariDBCon("SELECT * FROM M_12_Bi WHERE biid=" & idtransaksi, myConn)
                If (dtutama.Rows.Count > 0) Then
                    For Each drutama As DataRow In dtutama.Rows
                        Dim nmTabel As String = ""
                        If drutama("bijenis") = 0 Then
                            nmTabel = "M_12_Pos_Bonus_Item"
                        Else
                            nmTabel = "M_12_Pos_Bonus_Trans"
                        End If

                        'AMBIL DATA DETAIL
                        dtdetail = AsDataTableAmbilDariDBCon("SELECT * FROM M_12_Bi_Detail WHERE idbi=" & idtransaksi, myConn)
                        If (dtdetail.Rows.Count > 0) Then
                            For Each drdetail As DataRow In dtdetail.Rows
                                Dim dtbonus As New DataTable
                                If drutama("bijeniskategori") = 1 Then 'JIKA PER KATEGORI
                                    Dim query As String = "SELECT biid FROM " & nmTabel & " WHERE bikategori='" & drdetail("bikategori") & "' AND binopromo = '" & drdetail("nopromo") & "'"
                                    dtbonus = AsDataTableAmbilDariDBCon(query, myConn)
                                    If dtbonus.Rows.Count > 0 Then
                                        For Each drbonus As DataRow In dtbonus.Rows
                                            'hapus data detail
                                            Dim strValue2 As New StringBuilder
                                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", " AND "))
                                            strValue2.Append("biid = '" & FixQuotes(drbonus("biid")) & "' ")
                                            sql = "Delete from " & nmTabel & " WHERE " & strValue2.ToString
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
                                            strValueItemDetail.Append("idbi = '" & FixQuotes(drbonus("biid")) & "'")

                                            'hapus data detail
                                            sql = "Delete from " & nmTabel & "_Detail WHERE " & strValueItemDetail.ToString
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
                                    Dim query As String = "SELECT biid FROM " & nmTabel & " WHERE binopromo = '" & drdetail("nopromo") & "'"
                                    dtbonus = AsDataTableAmbilDariDBCon(query, myConn)
                                    If dtbonus.Rows.Count > 0 Then
                                        For Each drbonus As DataRow In dtbonus.Rows
                                            'hapus data detail
                                            Dim strValue2 As New StringBuilder
                                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", " OR "))
                                            strValue2.Append("biid = '" & FixQuotes(drbonus("biid")) & "' ")
                                            sql = "Delete from " & nmTabel & " WHERE " & strValue2.ToString
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
                                            strValueItemDetail.Append("idbi = '" & FixQuotes(drbonus("biid")) & "'")

                                            'hapus data detail
                                            sql = "Delete from " & nmTabel & "_Detail WHERE " & strValueItemDetail.ToString
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
            sql = "UPDATE M_12_Bi SET Bistatus = " & nilaiStatus & ", bimodifikasiuser='" & userid & "', bimodifikasitgl = NOW(), biposting = 0, bipostingtgl = '1971-01-01 00:00:00', Bijmlrevisi = Bijmlrevisi + 1 WHERE biid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_BiSearch(PostWsSearch(paramSplit(0), "M12_BiSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_BiDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("bikontakkode", "c.kkode")
            Filter = Filter.Replace("bikontaknama", "c.knama")
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
            Dim sumber As String = "BI", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT biid, binotransaksi FROM m_12_bi WHERE biid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT bicabang, bilokasi, bisumber, biautonotransaksi, binotransaksi, bitgl"
            sql &= " FROM M_12_bi"
            sql &= " WHERE biid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("bicabang")
                lokasi = dtNomorNext.Rows(0)("bilokasi")
                sumber = dtNomorNext.Rows(0)("bisumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("biautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("binotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("bitgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M_12_Bi_Detail WHERE idbi = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M_12_Bi WHERE biid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_BiSearch(PostWsSearch(paramSplit(0), "M12_BiSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_BiSimpanOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataBonus(), dataRowBonus() As String

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
        'biid(0) As Integer, bicabang(1) As String, bilokasi(2) As String, bisumber(3) As String, bikategoripos(4) As String, 
        'biautonotransaksi(5) As Integer, binotransaksi(6) As String, bitgl(7) As Date, bikodepa(8) As , bikontak(9) As , 
        'bikontakperson(10) As String, biuraian(11) As String, bicatatan(12) As String, bistatus(13) As Integer, bistatussebelumnya(14) As Integer, 
        'bijmlrevisi(15) As Integer, bicetakanke(16) As Integer, biisclose(17) As Integer, biinputuser(18) As , biinputtgl(19) As DateTime, 
        'bimodifikasiuser(20) As , bimodifikasitgl(21) As DateTime, biposting(22) As Integer, bipostingtgl(23) As DateTime, bicustomtext1(24) As String, 
        'bicustomtext2(25) As String, bicustomtext3(26) As String, bicustomtext4(27) As String, bicustomtext5(28) As String, bicustomint1(29) As Integer, 
        'bicustomint2(30) As Integer, bicustomint3(31) As Integer, bicustomdbl1(32) As Double, bicustomdbl2(33) As Double, bicustomdbl3(34) As Double, 
        'bicustomdate1(35) As Date, bicustomdate2(36) As Date, bicustomdate3(37) As Date, bijeniskategori(38) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'biid, bicabang, bilokasi, bisumber, bikategoripos, biautonotransaksi, binotransaksi, 
        'bitgl, bikodepa, bikontak, bikontakperson, biuraian, bicatatan, bistatus, 
        'bistatussebelumnya, bijmlrevisi, bicetakanke, biisclose, biinputuser, biinputtgl, bimodifikasiuser, 
        'bimodifikasitgl, biposting, bipostingtgl, bicustomtext1, bicustomtext2, bicustomtext3, bicustomtext4, 
        'bicustomtext5, bicustomint1, bicustomint2, bicustomint3, bicustomdbl1, bicustomdbl2, bicustomdbl3, 
        'bicustomdate1, bicustomdate2, bicustomdate3, bijeniskategori

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 39) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'biid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "biid required numeric." : GoTo selesai
        End If
        'biautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "biautonotransaksi required numeric." : GoTo selesai
        End If
        'bitgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "bitgl required date." : GoTo selesai
        End If
        'bistatus(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "bistatus required numeric." : GoTo selesai
        End If
        'bistatussebelumnya(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "bistatussebelumnya required numeric." : GoTo selesai
        End If
        'bijmlrevisi(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "bijmlrevisi required numeric." : GoTo selesai
        End If
        'bicetakanke(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "bicetakanke required numeric." : GoTo selesai
        End If
        'biisclose(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "biisclose required numeric." : GoTo selesai
        End If
        'biinputtgl(19) As DateTime
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "biinputtgl required date." : GoTo selesai
        End If
        'bimodifikasitgl(21) As DateTime
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "bimodifikasitgl required date." : GoTo selesai
        End If
        'biposting(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "biposting required numeric." : GoTo selesai
        End If
        'bipostingtgl(23) As DateTime
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "bipostingtgl required date." : GoTo selesai
        End If
        'bicustomint1(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "bicustomint1 required numeric." : GoTo selesai
        End If
        'bicustomint2(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "bicustomint2 required numeric." : GoTo selesai
        End If
        'bicustomint3(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "bicustomint3 required numeric." : GoTo selesai
        End If
        'bicustomdbl1(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "bicustomdbl1 required numeric." : GoTo selesai
        End If
        'bicustomdbl2(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "bicustomdbl2 required numeric." : GoTo selesai
        End If
        'bicustomdbl3(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "bicustomdbl3 required numeric." : GoTo selesai
        End If
        'bicustomdate1(35) As Date
        If (IsDate(dataUtama(35)) = False) Then
            result(2) = "bicustomdate1 required date." : GoTo selesai
        End If
        'bicustomdate2(36) As Date
        If (IsDate(dataUtama(36)) = False) Then
            result(2) = "bicustomdate2 required date." : GoTo selesai
        End If
        'bicustomdate3(37) As Date
        If (IsDate(dataUtama(37)) = False) Then
            result(2) = "bicustomdate3 required date." : GoTo selesai
        End If

        'bijeniskategori(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "bijeniskategori required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'bicabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "bicabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "bicabang should not be more than 25 character." : GoTo selesai
        End If

        'bilokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "bilokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "bilokasi should not be more than 25 character." : GoTo selesai
        End If

        'bisumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "bisumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "bisumber should not be more than 10 character." : GoTo selesai
        End If

        'bikategoripos(4) As String
        'If Len(dataUtama(4)) = 0 Then
        '    result(2) = "bikategoripos can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(4)) > 50 Then
            result(2) = "bikategoripos should not be more than 50 character." : GoTo selesai
        End If

        'binotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "binotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "binotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'bitgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "bitgl can't be empty" : GoTo selesai
        End If

        'bikodepa(8) As 
        If Len(dataUtama(8)) = 0 Then
            result(2) = "bikodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 20 Then
            result(2) = "bikodepa should not be more than 20 character." : GoTo selesai
        End If

        'bikontak(9) As 
        If Len(dataUtama(9)) = 0 Then
            result(2) = "bikontak can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 20 Then
            result(2) = "bikontak should not be more than 20 character." : GoTo selesai
        End If

        'biinputtgl(19) As DateTime
        If Len(dataUtama(19)) = 0 Then
            result(2) = "biinputtgl can't be empty" : GoTo selesai
        End If

        'bimodifikasitgl(21) As DateTime
        If Len(dataUtama(21)) = 0 Then
            result(2) = "bimodifikasitgl can't be empty" : GoTo selesai
        End If

        'bipostingtgl(23) As DateTime
        If Len(dataUtama(23)) = 0 Then
            result(2) = "bipostingtgl can't be empty" : GoTo selesai
        End If

        'bicustomdbl1(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "bicustomdbl1 can't be empty" : GoTo selesai
        End If

        'bicustomdbl2(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "bicustomdbl2 can't be empty" : GoTo selesai
        End If

        'bicustomdbl3(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "bicustomdbl3 can't be empty" : GoTo selesai
        End If

        'bicustomdate1(35) As Date
        If Len(dataUtama(35)) = 0 Then
            result(2) = "bicustomdate1 can't be empty" : GoTo selesai
        End If

        'bicustomdate2(36) As Date
        If Len(dataUtama(36)) = 0 Then
            result(2) = "bicustomdate2 can't be empty" : GoTo selesai
        End If

        'bicustomdate3(37) As Date
        If Len(dataUtama(37)) = 0 Then
            result(2) = "bicustomdate3 can't be empty" : GoTo selesai
        End If



        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "biid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bilokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bisumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bikategoripos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "biautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "binotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bikodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bikontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bikontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "biuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bistatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bistatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bijmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bicetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "biisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "biinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "biinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bimodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bimodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "biposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bipostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bicustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bijeniskategori", AsEnumTypeData.AsInt64)

        If AsDataTableTambahData(dtutama, "biid~bicabang~bilokasi~bisumber~bikategoripos~biautonotransaksi~binotransaksi~bitgl~bikodepa~bikontak~bikontakperson~biuraian~bicatatan~bistatus~bistatussebelumnya~bijmlrevisi~bicetakanke~biisclose~biinputuser~biinputtgl~bimodifikasiuser~bimodifikasitgl~biposting~bipostingtgl~bicustomtext1~bicustomtext2~bicustomtext3~bicustomtext4~bicustomtext5~bicustomint1~bicustomint2~bicustomint3~bicustomdbl1~bicustomdbl2~bicustomdbl3~bicustomdate1~bicustomdate2~bicustomdate3~bijeniskategori", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38)) = False Then
            result(2) = "insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idbidetail(0) As , idbi(1) As , bikategori(2) As String, idbarang(3) As , operator(4) As String, 
        'jml1(5) As Double, jml2(6) As Double, customtext1(7) As String, customtext2(8) As String, customtext3(9) As String, 
        'customtext4(10) As String, customtext5(11) As String, customint1(12) As Integer, customint2(13) As Integer, customint3(14) As Integer, 
        'customdbl1(15) As Double, customdbl2(16) As Double, customdbl3(17) As Double, customdate1(18) As Date, customdate2(19) As Date, 
        'customdate3(20) As Date, tgl1(21) As Date, tgl2(22) As Date, nopromo(23) As String, nogrup (24) As String, catatan (25) As String, urutan(26) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idbidetail, idbi, bikategori, idbarang, operator, jml1, jml2, 
        'customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, 
        'customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'tgl1, tgl2, nopromo, nogrup, catatan, urutan

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idbidetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idbi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "bikategori", AsEnumTypeData.AsString)
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
                result(2) = "Row : " & i & " - idbidetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idbidetail should not be more than 20 character." : GoTo selesai
            End If

            'idbi(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idbi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idbi should not be more than 20 character." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idbidetail~idbi~bikategori~idbarang~operator~jml1~jml2~customtext1~customtext2~customtext3~customtext4~customtext5~customint1~customint2~customint3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~tgl1~tgl2~nopromo~nogrup~catatan~urutan", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26)) = False Then
                result(2) = "Detail Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================

        'MAPPING BUAT WS DATA BONUS -------------------------------------------------------
        'idbonus(0) As , idbi(1) As , idbidetail(2) As , idbarang(3) As , jml(4) As Double, 
        'satuan(5) As String, customtext1(6) As String, customtext2(7) As String, customtext3(8) As String, customtext4(9) As String, 
        'customtext5(10) As String, customint1(11) As Integer, customint2(12) As Integer, customint3(13) As Integer, customdbl1(14) As Double, 
        'customdbl2(15) As Double, customdbl3(16) As Double, customdate1(17) As Date, customdate2(18) As Date, customdate3(19) As Date, nogrup(20) As String

        'MAPPING BUAT FLEX DATA BONUS -----------------------------------------------------
        'idbonus, idbi, idbidetail, idbarang, jml, satuan, customtext1, 
        'customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA BONUS ======================================================
        'SPLIT PARAMETER DATA BONUS
        dataBonus = dataSplit(2).Split(sptRow)
        'END OF VALIDASI DAN SET DATA BONUS ===============================================

        'Buat datatable bonus
        Dim dtbonus As New DataTable
        AsDataTableTambahField(dtbonus, "idbonus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbonus, "idbi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbonus, "idbidetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbonus, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbonus, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "customint1", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtbonus, "customint2", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtbonus, "customint3", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtbonus, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "nogrup", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbonus, "urutan", AsEnumTypeData.AsInt64)

        'VALIDASI DAN SET DATA ROW BONUS ==================================================
        Dim JmlDtBonus As Integer = dataBonus.Length
        For i = 1 To JmlDtBonus
            'SPLIT DATA DETAIL
            dataRowBonus = dataBonus(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA BONUS -----------------------------------
            'CEK ARRAY DATA BONUS
            If (dataRowBonus.Length <> 22) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW BONUS ----------------------------

            'VALIDASI TIPE DATA BONUS ------------------------------------------
            'jml(4) As Double
            If (IsNumeric(dataRowBonus(4)) = False) Then
                result(2) = "jml required numeric." : GoTo selesai
            End If
            'customint1(11) As Integer
            If (IsNumeric(dataRowBonus(11)) = False) Then
                result(2) = "customint1 required numeric." : GoTo selesai
            End If
            'customint2(12) As Integer
            If (IsNumeric(dataRowBonus(12)) = False) Then
                result(2) = "customint2 required numeric." : GoTo selesai
            End If
            'customint3(13) As Integer
            If (IsNumeric(dataRowBonus(13)) = False) Then
                result(2) = "customint3 required numeric." : GoTo selesai
            End If
            'customdbl1(14) As Double
            If (IsNumeric(dataRowBonus(14)) = False) Then
                result(2) = "customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(15) As Double
            If (IsNumeric(dataRowBonus(15)) = False) Then
                result(2) = "customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(16) As Double
            If (IsNumeric(dataRowBonus(16)) = False) Then
                result(2) = "customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(17) As Date
            If (IsDate(dataRowBonus(17)) = False) Then
                result(2) = "customdate1 required date." : GoTo selesai
            End If
            'customdate2(18) As Date
            If (IsDate(dataRowBonus(18)) = False) Then
                result(2) = "customdate2 required date." : GoTo selesai
            End If
            'customdate3(19) As Date
            If (IsDate(dataRowBonus(19)) = False) Then
                result(2) = "customdate3 required date." : GoTo selesai
            End If
            'urutan(21) As Double
            If (IsNumeric(dataRowBonus(21)) = False) Then
                result(2) = "urutan required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA BONUS -----------------------------------

            'VALIDASI DATA BONUS ---------------------------------------
            'idbonus(0) As 
            If Len(dataRowBonus(0)) = 0 Then
                result(2) = "Row : " & i & " - idbonus can't be empty" : GoTo selesai
            End If
            If Len(dataRowBonus(0)) > 20 Then
                result(2) = "Row : " & i & " - idbonus should not be more than 20 character." : GoTo selesai
            End If

            'idbi(1) As 
            If Len(dataRowBonus(1)) = 0 Then
                result(2) = "Row : " & i & " - idbi can't be empty" : GoTo selesai
            End If
            If Len(dataRowBonus(1)) > 20 Then
                result(2) = "Row : " & i & " - idbi should not be more than 20 character." : GoTo selesai
            End If

            'idbidetail(2) As 
            If Len(dataRowBonus(2)) = 0 Then
                result(2) = "Row : " & i & " - idbidetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowBonus(2)) > 20 Then
                result(2) = "Row : " & i & " - idbidetail should not be more than 20 character." : GoTo selesai
            End If

            'idbarang(3) As 
            If Len(dataRowBonus(3)) = 0 Then
                result(2) = "Row : " & i & " - idbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowBonus(3)) > 20 Then
                result(2) = "Row : " & i & " - idbarang should not be more than 20 character." : GoTo selesai
            End If

            'jml(4) As Double
            If Len(dataRowBonus(4)) = 0 Then
                result(2) = "Row : " & i & " - jml can't be empty" : GoTo selesai
            End If

            'satuan(5) As String
            If Len(dataRowBonus(5)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowBonus(5)) > 25 Then
                result(2) = "Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'customdbl1(14) As Double
            If Len(dataRowBonus(14)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(15) As Double
            If Len(dataRowBonus(15)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(16) As Double
            If Len(dataRowBonus(16)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(17) As Date
            If Len(dataRowBonus(17)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(18) As Date
            If Len(dataRowBonus(18)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(19) As Date
            If Len(dataRowBonus(19)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'urutan(21) As Date
            If Len(dataRowBonus(21)) = 0 Then
                result(2) = "Row : " & i & " - urutan can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtbonus, "idbonus~idbi~idbidetail~idbarang~jml~satuan~customtext1~customtext2~customtext3~customtext4~customtext5~customint1~customint2~customint3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~nogrup~urutan", dataRowBonus(0) & "~" & dataRowBonus(1) & "~" & dataRowBonus(2) & "~" & dataRowBonus(3) & "~" & dataRowBonus(4) & "~" & dataRowBonus(5) & "~" & dataRowBonus(6) & "~" & dataRowBonus(7) & "~" & dataRowBonus(8) & "~" & dataRowBonus(9) & "~" & dataRowBonus(10) & "~" & dataRowBonus(11) & "~" & dataRowBonus(12) & "~" & dataRowBonus(13) & "~" & dataRowBonus(14) & "~" & dataRowBonus(15) & "~" & dataRowBonus(16) & "~" & dataRowBonus(17) & "~" & dataRowBonus(18) & "~" & dataRowBonus(19) & "~" & dataRowBonus(20) & "~" & dataRowBonus(21)) = False Then
                result(2) = "Bonus Row : " & i & " - insert into datatable failed." : GoTo selesai
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
                    result(4) = drutama("biid")
                    notransaksi = drutama("binotransaksi")

                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(biid), binotransaksi FROM M_12_Bi WHERE biid=" & result(4))
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(biid) FROM M_12_Bi WHERE binotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m12_bi_history
                        Dim rsSimpanHistory As String = SimpanHistory.M12_Bi_HistorySimpan("" & paramSplit(0) & "★M12_Bi_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("bisumber")) & "▼" & FixQuotes(drutama("biid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M_12_Bi set bicabang  = '" & FixQuotes(drutama("bicabang")) & "', bilokasi  = '" & FixQuotes(drutama("bilokasi")) & "', bisumber  = '" & FixQuotes(drutama("bisumber")) & "', bikategoripos  = '" & FixQuotes(drutama("bikategoripos")) & "', biautonotransaksi  = " & drutama("biautonotransaksi") & ", binotransaksi  = '" & FixQuotes(drutama("binotransaksi")) & "', bitgl  = '" & FixQuotes(AsFormatTanggal(drutama("bitgl"))) & "', bikodepa  = '" & FixQuotes(drutama("bikodepa")) & "', bikontak  = '" & FixQuotes(drutama("bikontak")) & "', bikontakperson  = '" & FixQuotes(drutama("bikontakperson")) & "', biuraian  = '" & FixQuotes(drutama("biuraian")) & "', bicatatan  = '" & FixQuotes(drutama("bicatatan")) & "', bistatus  = " & drutama("bistatus") & ", bistatussebelumnya  = " & drutama("bistatussebelumnya") & ", bijmlrevisi  = " & drutama("bijmlrevisi") & ", bicetakanke  = " & drutama("bicetakanke") & ", biisclose  = " & drutama("biisclose") & ", biinputuser  = '" & FixQuotes(drutama("biinputuser")) & "', bimodifikasiuser  = '" & FixQuotes(drutama("bimodifikasiuser")) & "', bimodifikasitgl  = NOW(), biposting  = " & drutama("biposting") & ", bipostingtgl  = '" & FixQuotes(AsFormatTanggal(drutama("bipostingtgl"), "yyyy-MM-dd H:mm:ss")) & "', bicustomtext1  = '" & FixQuotes(drutama("bicustomtext1")) & "', bicustomtext2  = '" & FixQuotes(drutama("bicustomtext2")) & "', bicustomtext3  = '" & FixQuotes(drutama("bicustomtext3")) & "', bicustomtext4  = '" & FixQuotes(drutama("bicustomtext4")) & "', bicustomtext5  = '" & FixQuotes(drutama("bicustomtext5")) & "', bicustomint1  = " & drutama("bicustomint1") & ", bicustomint2  = " & drutama("bicustomint2") & ", bicustomint3  = " & drutama("bicustomint3") & ", bicustomdbl1  = '" & FixDouble(drutama("bicustomdbl1")) & "', bicustomdbl2  = '" & FixDouble(drutama("bicustomdbl2")) & "', bicustomdbl3  = '" & FixDouble(drutama("bicustomdbl3")) & "', bicustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("bicustomdate1"))) & "', bicustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("bicustomdate2"))) & "', bicustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("bicustomdate3"))) & "', bijeniskategori  = '" & FixQuotes(drutama("bijeniskategori")) & "' where biid = " & drutama("biid") & ""
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

                    If drutama("biautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("bicabang"), drutama("bilokasi"), drutama("bisumber"), drutama("bitgl"))
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
                        notransaksi = drutama("binotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(biid) FROM m_12_bi WHERE binotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M_12_Bi (bicabang, bilokasi, bisumber, bikategoripos, biautonotransaksi, binotransaksi, bitgl, bikodepa, bikontak, bikontakperson, biuraian, bicatatan, bistatus, bistatussebelumnya, bijmlrevisi, bicetakanke, biisclose, biinputuser, biinputtgl, bimodifikasiuser, bimodifikasitgl, biposting, bipostingtgl, bicustomtext1, bicustomtext2, bicustomtext3, bicustomtext4, bicustomtext5, bicustomint1, bicustomint2, bicustomint3, bicustomdbl1, bicustomdbl2, bicustomdbl3, bicustomdate1, bicustomdate2, bicustomdate3, bijeniskategori) values('" & FixQuotes(drutama("bicabang")) & "', '" & FixQuotes(drutama("bilokasi")) & "', '" & FixQuotes(drutama("bisumber")) & "', '" & FixQuotes(drutama("bikategoripos")) & "', " & drutama("biautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("bitgl"))) & "', '" & FixQuotes(drutama("bikodepa")) & "', '" & FixQuotes(drutama("bikontak")) & "', '" & FixQuotes(drutama("bikontakperson")) & "', '" & FixQuotes(drutama("biuraian")) & "', '" & FixQuotes(drutama("bicatatan")) & "', " & drutama("bistatus") & ", " & drutama("bistatussebelumnya") & ", " & drutama("bijmlrevisi") & ", " & drutama("bicetakanke") & ", " & drutama("biisclose") & ", '" & FixQuotes(drutama("biinputuser")) & "', NOW(), '" & FixQuotes(drutama("bimodifikasiuser")) & "', '1971-01-01 00:00:00', 0, '1971-01-01 00:00:00', '" & FixQuotes(drutama("bicustomtext1")) & "', '" & FixQuotes(drutama("bicustomtext2")) & "', '" & FixQuotes(drutama("bicustomtext3")) & "', '" & FixQuotes(drutama("bicustomtext4")) & "', '" & FixQuotes(drutama("bicustomtext5")) & "', " & drutama("bicustomint1") & ", " & drutama("bicustomint2") & ", " & drutama("bicustomint3") & ", '" & FixDouble(drutama("bicustomdbl1")) & "', '" & FixDouble(drutama("bicustomdbl2")) & "', '" & FixDouble(drutama("bicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("bicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("bicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("bicustomdate3"))) & "', " & drutama("bijeniskategori") & ")"
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
                    dt2 = AsDataTableAmbilDariDB("select biid from M_12_bi where binotransaksi='" & notransaksi & "' AND biinputuser= '" & drutama("biinputuser") & "' order by bimodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M_12_Bi_Detail where idbi = " & result(4)
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Hapus bonus ketika update
                If (isUpdate) Then
                    sql = "Delete from M_12_Bi_Bonus where idbi = " & result(4)
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
                    Dim dtBonusGrup As New DataTable

                    For Each dr1 As DataRow In dtdetail.Rows

                        'CEK OPERATOR :
                        'JIKA BETWEEN (0) ATAU >= (1) MAKA BOLEH LEBIH DARI SATU KONDISI 
                        '=> BEBERAPA KONDISI BETWEEN DAN SATU KONDISI >= (1)
                        'JIKA KELIPATAN (2) MAKA HANYA BOLEH SATU KONDISI
                        Dim dtOperator As New DataTable
                        sql = "SELECT bid.bikategori as kategori, bid.idbarang as idbarang, bid.operator as operator, i.bkode, (CASE bid.operator WHEN 0 THEN 'Between' WHEN 1 THEN '>=' WHEN 2 THEN 'Multiple' ELSE 'Unknown' END) as operatornama FROM m_12_bi_detail bid JOIN m1_item i ON bid.idbarang = i.bid WHERE bid.bikategori = '" & FxDB(drutama("bikategoripos"), "") & "' AND bid.idbarang = '" & FxDB(dr1("idbarang"), "") & "' AND bid.idbi = '" & result(4) & "' AND bid.idbidetail <> '" & FxDB(dr1("idbidetail"), "") & "' GROUP BY bid.operator ORDER BY bid.operator"
                        dtOperator = AsDataTableAmbilDariDB(sql)
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
                        strValue2.Append("('" & FixQuotes(dr1("idbidetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("bikategori")) & "', '" & FixQuotes(dr1("idbarang")) & "', '" & FixQuotes(dr1("operator")) & "', '" & FixDouble(dr1("jml1")) & "', '" & FixDouble(dr1("jml2")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl2"))) & "', '" & FixQuotes(dr1("nopromo")) & "')")

                        'sql = "Insert into M_12_Bi_Detail(idbidetail, idbi, bikategori, idbarang, operator, jml1, jml2, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, tgl1, tgl2, nopromo) values" & strValue2.ToString & ""
                        sql = "Insert into M_12_Bi_Detail(idbidetail, idbi, bikategori, idbarang, operator, jml1, jml2, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, tgl1, tgl2, nopromo, catatan, urutan) values('" & FixQuotes(dr1("idbidetail")) & "', " & result(4) & ", '" & FixQuotes(drutama("bikategoripos")) & "', '" & FixQuotes(dr1("idbarang")) & "', '" & FixQuotes(dr1("operator")) & "', '" & FixDouble(dr1("jml1")) & "', '" & FixDouble(dr1("jml2")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl2"))) & "', '" & notransaksi & "', '" & FixQuotes(dr1("catatan")) & "','" & FixQuotes(dr1("urutan")) & "')"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()


                        'ambil ID detail untuk diinsert ke bonus
                        Dim iddetail As Integer
                        Dim dtidbonus As New DataTable
                        'Sql disesuaikan sendiri, untuk parameternya disesuaikan sendiri.
                        dtidbonus = AsDataTableAmbilDariDB("select idbidetail from M_12_bi_detail where idbi='" & result(4) & "' and bikategori = '" & drutama("bikategoripos") & "' AND  idbarang = '" & dr1("idbarang") & "' AND  operator = '" & dr1("operator") & "' AND  jml1 = '" & dr1("jml1") & "' AND jml2 = '" & dr1("jml2") & "' order by idbidetail desc limit 1")
                        If dtidbonus.Rows.Count > 0 Then iddetail = dtidbonus.Rows(0)(0) Else result(2) = "#1 Detail transaction data not found." : Trans.Rollback() : GoTo selesai

                        'Proses Bonus
                        If (dtbonus.Rows.Count > 0) Then
                            'AMBIL DETAIL BONUS SESUAI NO GRUP
                            dtBonusGrup = AsDataTableFilterSortDt(dtbonus, "nogrup = '" & dr1("nogrup") & "'")
                            If (dtBonusGrup.Rows.Count > 0) Then
                                strValue2.Clear()
                                For Each drBonusGrup As DataRow In dtBonusGrup.Rows
                                    strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                                    strValue2.Append("('" & FixQuotes(drBonusGrup("idbonus")) & "', " & result(4) & ", '" & iddetail & "', '" & FixQuotes(drBonusGrup("idbarang")) & "', '" & FixDouble(drBonusGrup("jml")) & "', '" & FixQuotes(drBonusGrup("satuan")) & "', '" & FixQuotes(drBonusGrup("customtext1")) & "', '" & FixQuotes(drBonusGrup("customtext2")) & "', '" & FixQuotes(drBonusGrup("customtext3")) & "', '" & FixQuotes(drBonusGrup("customtext4")) & "', '" & FixQuotes(drBonusGrup("customtext5")) & "', " & drBonusGrup("customint1") & ", " & drBonusGrup("customint2") & ", " & drBonusGrup("customint3") & ", '" & FixDouble(drBonusGrup("customdbl1")) & "', '" & FixDouble(drBonusGrup("customdbl2")) & "', '" & FixDouble(drBonusGrup("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drBonusGrup("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drBonusGrup("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drBonusGrup("customdate3"))) & "', '" & FixQuotes(drBonusGrup("urutan")) & "')")
                                Next

                                sql = "Insert into M_12_Bi_Bonus(idbonus, idbi, idbidetail,  idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, urutan) values" & strValue2.ToString & ""
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = Con1
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                            Else
                                result(2) = "Bonus Transaction for No. Group : " & dr1("nogrup") & " does not found." : Trans.Rollback() : GoTo selesai
                            End If

                        Else
                            result(2) = "Bonus Transaction data not found." : Trans.Rollback() : GoTo selesai
                        End If
                    Next

                Else
                    result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Update ke tabel Barang Bonus
                If drutama("bistatus") = 2 Then
                    'JIKA PER KATEGORI, HAPUS DATA PER KATEGORI
                    If drutama("bijeniskategori") = 1 Then
                        'Cek apakah kategori pos sudah ada di tabel pos_bonus_item, jika sudah ada, hapus data di tabel itu
                        Dim dtPOSBonusItem As New DataTable
                        dtPOSBonusItem = AsDataTableAmbilDariDB("select biid from M_12_Pos_Bonus_Item where bikategori = '" & drutama("bikategoripos") & "'")
                        Dim strValueItemUtama As New StringBuilder
                        Dim strValueItemDetail As New StringBuilder
                        If dtPOSBonusItem.Rows.Count > 1 Then
                            For Each drPOSBonusItem As DataRow In dtPOSBonusItem.Rows
                                'QUERY HAPUS POS BONUS ITEM UTAMA
                                strValueItemUtama.Append(IIf(Len(strValueItemUtama.ToString) = 0, "", " OR "))
                                strValueItemUtama.Append("biid = '" & FixQuotes(drPOSBonusItem("biid")) & "'")

                                'QUERY HAPUS POS BONUS ITEM DETAIL
                                strValueItemDetail.Append(IIf(Len(strValueItemDetail.ToString) = 0, "", " OR "))
                                strValueItemDetail.Append("idbi = '" & FixQuotes(drPOSBonusItem("biid")) & "'")
                            Next

                            'HAPUS POS BONUS ITEM UTAMA
                            sql = "Delete From m_12_pos_bonus_item where " & strValueItemUtama.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                            'HAPUS POS BONUS ITEM DETAIL
                            sql = "Delete From m_12_pos_bonus_item_detail where " & strValueItemDetail.ToString & ""
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
                        sql = "Delete From m_12_pos_bonus_item"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'HAPUS POS BONUS ITEM DETAIL
                        sql = "Delete From m_12_pos_bonus_item_detail"
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
                    dtdtl = AsDataTableAmbilDariDB("select * from M_12_Bi_Detail where idbi = '" & result(4) & "' order by idbi asc")
                    Dim dtbibonus As New DataTable
                    'AMBIL DATA BI BONUS
                    dtbibonus = AsDataTableAmbilDariDB("select * from M_12_Bi_Bonus where idbi = '" & result(4) & "' order by idbi asc")

                    Dim strValueInsertBonusItem As New StringBuilder 'untuk query simpan di tabel bonus utama
                    Dim strValueBonusItemDetail As New StringBuilder 'untuk query simpan di tabel bonus detail
                    Dim idposbonusitem As Integer 'untuk variabel id transaksi pos bonus item utama
                    Dim dtselectId As New DataTable 'untuk query ambil id transaksi pos bonus item
                    Dim dtBonusPenampung As New DataTable 'untuk menampung data bi bonus
                    Dim dtKatPOS As New DataTable 'untuk menampung data kategori pos, jika jenis kategori 
                    strValueBonusItemDetail.Clear()

                    If dtdtl.Rows.Count > 0 Then

                        If drutama("bijeniskategori") = 1 Then 'JIKA PER KATEGORI
                            For Each drdtl As DataRow In dtdtl.Rows
                                'persiapan insert ke tabel m_12_pos_bonus_item & tabel m_12_pos_bonus_item_detail
                                strValueInsertBonusItem.Append(IIf(Len(strValueInsertBonusItem.ToString) = 0, "", ", "))
                                strValueInsertBonusItem.Append("('" & FixQuotes(drutama("bikategoripos")) & "', '" & FixQuotes(drdtl("idbarang")) & "', '" & FixQuotes(drdtl("operator")) & "', '" & FixDouble(drdtl("jml1")) & "', '" & FixDouble(drdtl("jml2")) & "', '" & FixQuotes(drdtl("customtext1")) & "', '" & FixQuotes(drdtl("customtext2")) & "', '" & FixQuotes(drdtl("customtext3")) & "', '" & FixQuotes(drdtl("customtext4")) & "', '" & FixQuotes(drdtl("customtext5")) & "', " & drdtl("customint1") & ", " & drdtl("customint2") & ", " & drdtl("customint3") & ", '" & FixDouble(drdtl("customdbl1")) & "', '" & FixDouble(drdtl("customdbl2")) & "', '" & FixDouble(drdtl("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl2"))) & "', '" & FixQuotes(drdtl("nopromo")) & "')")
                            Next

                            'insert ke tabel m_12_pos_bonus_item
                            sql = "Insert into M_12_Pos_Bonus_Item (bikategori, biidbarang, bioperator, bijml1, bijml2, bicustomtext1, bicustomtext2, bicustomtext3, bicustomtext4, bicustomtext5, bicustomint1, bicustomint2, bicustomint3, bicustomdbl1, bicustomdbl2, bicustomdbl3, bicustomdate1, bicustomdate2, bicustomdate3, bitgl1, bitgl2, binopromo) values " & strValueInsertBonusItem.ToString & ""
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
                                dtselectId = AsDataTableAmbilDariDB("select biid from M_12_Pos_Bonus_Item where bikategori = '" & drdtl2("bikategori") & "' AND biidbarang = '" & drdtl2("idbarang") & "' AND bioperator = '" & drdtl2("operator") & "' AND bijml1 = '" & drdtl2("jml1") & "' AND bijml2 = '" & drdtl2("jml2") & "' limit 1")
                                If dtselectId.Rows.Count > 0 Then idposbonusitem = dtselectId.Rows(0)(0) Else result(2) = "Main POS Bonus Item transaction data not found." : Trans.Rollback() : GoTo selesai

                                'filter data bonus penampung, untuk dijadikan parameter simpan ke tabel pos bonus detail
                                dtBonusPenampung = AsDataTableFilterSortDt(dtbibonus, "idbidetail = '" & drdtl2("idbidetail") & "'")
                                If dtBonusPenampung.Rows.Count > 0 Then
                                    For Each drBonusPenampung As DataRow In dtBonusPenampung.Rows
                                        'parameter simpan ke tabel m_12_pos_bonus_item_DETAIL
                                        strValueBonusItemDetail.Append(IIf(Len(strValueBonusItemDetail.ToString) = 0, "", ", "))
                                        strValueBonusItemDetail.Append("(" & idposbonusitem & ", '" & FixQuotes(drBonusPenampung("idbarang")) & "', '" & FixDouble(drBonusPenampung("jml")) & "', '" & FixQuotes(drBonusPenampung("satuan")) & "', '" & FixQuotes(drBonusPenampung("customtext1")) & "', '" & FixQuotes(drBonusPenampung("customtext2")) & "', '" & FixQuotes(drBonusPenampung("customtext3")) & "', '" & FixQuotes(drBonusPenampung("customtext4")) & "', '" & FixQuotes(drBonusPenampung("customtext5")) & "', " & drBonusPenampung("customint1") & ", " & drBonusPenampung("customint2") & ", " & drBonusPenampung("customint3") & ", '" & FixDouble(drBonusPenampung("customdbl1")) & "', '" & FixDouble(drBonusPenampung("customdbl2")) & "', '" & FixDouble(drBonusPenampung("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drBonusPenampung("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drBonusPenampung("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drBonusPenampung("customdate3"))) & "')")
                                    Next
                                Else
                                    result(2) = "Main Transaction POS Bonus Item data not found." : Trans.Rollback() : GoTo selesai
                                End If
                            Next
                            'INSERT KE TABEL POS BONUS DETAIL
                            sql = "Insert into M_12_Pos_Bonus_Item_Detail(idbi, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValueBonusItemDetail.ToString & ""
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
                                                'persiapan insert ke tabel m_12_pos_bonus_item 
                                                strValueInsertBonusItem.Append(IIf(Len(strValueInsertBonusItem.ToString) = 0, "", ", "))
                                                strValueInsertBonusItem.Append("('" & FixQuotes(drKatPos("pckode")) & "', '" & FixQuotes(drdtl("idbarang")) & "', '" & FixQuotes(drdtl("operator")) & "', '" & FixDouble(drdtl("jml1")) & "', '" & FixDouble(drdtl("jml2")) & "', '" & FixQuotes(drdtl("customtext1")) & "', '" & FixQuotes(drdtl("customtext2")) & "', '" & FixQuotes(drdtl("customtext3")) & "', '" & FixQuotes(drdtl("customtext4")) & "', '" & FixQuotes(drdtl("customtext5")) & "', " & drdtl("customint1") & ", " & drdtl("customint2") & ", " & drdtl("customint3") & ", '" & FixDouble(drdtl("customdbl1")) & "', '" & FixDouble(drdtl("customdbl2")) & "', '" & FixDouble(drdtl("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl2"))) & "', '" & FixQuotes(drdtl("nopromo")) & "')")

                                            Next
                                        End If

                                    Next
                                Next
                            End If

                            'insert ke tabel m_12_pos_bonus_item
                            sql = "Insert into M_12_Pos_Bonus_Item (bikategori, biidbarang, bioperator, bijml1, bijml2, bicustomtext1, bicustomtext2, bicustomtext3, bicustomtext4, bicustomtext5, bicustomint1, bicustomint2, bicustomint3, bicustomdbl1, bicustomdbl2, bicustomdbl3, bicustomdate1, bicustomdate2, bicustomdate3, bitgl1, bitgl2, binopromo) values " & strValueInsertBonusItem.ToString & ""
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
                                                dtselectId = AsDataTableAmbilDariDB("select biid from M_12_Pos_Bonus_Item where bikategori = '" & drKatPos("pckode") & "' AND biidbarang = '" & drdtl2("idbarang") & "' AND bioperator = '" & drdtl2("operator") & "' AND bijml1 = '" & drdtl2("jml1") & "' AND bijml2 = '" & drdtl2("jml2") & "' limit 1")
                                                If dtselectId.Rows.Count > 0 Then idposbonusitem = dtselectId.Rows(0)(0) Else result(2) = "Main POS Bonus Item transaction data not found." : Trans.Rollback() : GoTo selesai
                                                'filter data bonus penampung, untuk dijadikan parameter simpan ke tabel pos bonus detail
                                                dtBonusPenampung = AsDataTableFilterSortDt(dtbibonus, "idbidetail = '" & drdtl2("idbidetail") & "'")
                                                If dtBonusPenampung.Rows.Count > 0 Then
                                                    For Each drBonusPenampung As DataRow In dtBonusPenampung.Rows
                                                        'persiapan insert ke tabel m_12_pos_bonus_item_DETAIL
                                                        strValueBonusItemDetail.Append(IIf(Len(strValueBonusItemDetail.ToString) = 0, "", ", "))
                                                        strValueBonusItemDetail.Append("(" & idposbonusitem & ", '" & FixQuotes(drBonusPenampung("idbarang")) & "', '" & FixDouble(drBonusPenampung("jml")) & "', '" & FixQuotes(drBonusPenampung("satuan")) & "', '" & FixQuotes(drBonusPenampung("customtext1")) & "', '" & FixQuotes(drBonusPenampung("customtext2")) & "', '" & FixQuotes(drBonusPenampung("customtext3")) & "', '" & FixQuotes(drBonusPenampung("customtext4")) & "', '" & FixQuotes(drBonusPenampung("customtext5")) & "', " & drBonusPenampung("customint1") & ", " & drBonusPenampung("customint2") & ", " & drBonusPenampung("customint3") & ", '" & FixDouble(drBonusPenampung("customdbl1")) & "', '" & FixDouble(drBonusPenampung("customdbl2")) & "', '" & FixDouble(drBonusPenampung("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drBonusPenampung("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drBonusPenampung("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drBonusPenampung("customdate3"))) & "')")

                                                    Next
                                                Else
                                                    result(2) = "Main Transaction POS Bonus Item data not found." : Trans.Rollback() : GoTo selesai
                                                End If
                                            Next
                                        End If
                                    Next
                                Next

                                'INSERT KE TABEL POS BONUS DETAIL
                                sql = "Insert into M_12_Pos_Bonus_Item_Detail(idbi, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValueBonusItemDetail.ToString & ""
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
    Public Function M12_BiUpdateStatusOld(ByVal param As String) As String
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
            Filter = Filter.Replace("bikontakkode", "c.kkode")
            Filter = Filter.Replace("bikontaknama", "c.knama")
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
            Dim sumber As String = "BI", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Bitgl, Binotransaksi, Bistatus FROM m_12_Bi WHERE Biid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Bistatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m12_bi_history
            Dim rsSimpanHistory As String = SimpanHistory.M12_Bi_HistorySimpan("" & paramSplit(0) & "★M12_Bi_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================


            If isDelete Then
                'AMBIL JENIS KATEGORI UTAMA
                Dim dtutama As New DataTable
                dtutama = AsDataTableAmbilDariDB("SELECT * FROM M_12_Bi WHERE biid=" & idtransaksi)
                If (dtutama.Rows.Count > 0) Then
                    For Each drutama As DataRow In dtutama.Rows

                        'AMBIL DATA DETAIL
                        dtdetail = AsDataTableAmbilDariDB("SELECT * FROM M_12_Bi_Detail WHERE idbi=" & idtransaksi)
                        If (dtdetail.Rows.Count > 0) Then
                            For Each drdetail As DataRow In dtdetail.Rows
                                Dim dtbonus As New DataTable
                                If drutama("bijeniskategori") = 1 Then 'JIKA PER KATEGORI
                                    Dim query As String = "SELECT biid FROM m_12_pos_bonus_item WHERE bikategori='" & drdetail("bikategori") & "' AND binopromo = '" & drdetail("nopromo") & "'"
                                    dtbonus = AsDataTableAmbilDariDB(query)
                                    If dtbonus.Rows.Count > 0 Then
                                        For Each drbonus As DataRow In dtbonus.Rows
                                            'hapus data detail
                                            Dim strValue2 As New StringBuilder
                                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", " AND "))
                                            strValue2.Append("biid = '" & FixQuotes(drbonus("biid")) & "' ")
                                            sql = "Delete from M_12_pos_bonus_item WHERE " & strValue2.ToString
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
                                            strValueItemDetail.Append("idbi = '" & FixQuotes(drbonus("biid")) & "'")

                                            'hapus data detail
                                            sql = "Delete from M_12_pos_bonus_item_Detail WHERE " & strValueItemDetail.ToString
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
                                    Dim query As String = "SELECT biid FROM m_12_pos_bonus_item WHERE binopromo = '" & drdetail("nopromo") & "'"
                                    dtbonus = AsDataTableAmbilDariDB(query)
                                    If dtbonus.Rows.Count > 0 Then
                                        For Each drbonus As DataRow In dtbonus.Rows
                                            'hapus data detail
                                            Dim strValue2 As New StringBuilder
                                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", " OR "))
                                            strValue2.Append("biid = '" & FixQuotes(drbonus("biid")) & "' ")
                                            sql = "Delete from M_12_pos_bonus_item WHERE " & strValue2.ToString
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
                                            strValueItemDetail.Append("idbi = '" & FixQuotes(drbonus("biid")) & "'")

                                            'hapus data detail
                                            sql = "Delete from M_12_pos_bonus_item_Detail WHERE " & strValueItemDetail.ToString
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
            sql = "UPDATE M_12_Bi SET Bistatus = " & nilaiStatus & ", bimodifikasiuser='" & userid & "', bimodifikasitgl = NOW(), biposting = 0, bipostingtgl = '1971-01-01 00:00:00', Bijmlrevisi = Bijmlrevisi + 1 WHERE biid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_BiSearch(PostWsSearch(paramSplit(0), "M12_BiSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_BiDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("bikontakkode", "c.kkode")
            Filter = Filter.Replace("bikontaknama", "c.knama")
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
            Dim sumber As String = "BI", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT biid, binotransaksi FROM m_12_bi WHERE biid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT bicabang, bilokasi, bisumber, biautonotransaksi, binotransaksi, bitgl"
            sql &= " FROM M_12_bi"
            sql &= " WHERE biid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("bicabang")
                lokasi = dtNomorNext.Rows(0)("bilokasi")
                sumber = dtNomorNext.Rows(0)("bisumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("biautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("binotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("bitgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M_12_Bi_Detail WHERE idbi = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M_12_Bi WHERE biid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_BiSearch(PostWsSearch(paramSplit(0), "M12_BiSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_BiGetdataById(ByVal param As String) As String

        'M12_BiGetdataById Utama --------------------------------------------------------
        'biid, bicabang, bilokasi, bisumber, bikategoripos, biautonotransaksi, binotransaksi, 
        'bitgl, bikodepa, bikontak, bikontakperson, biuraian, bicatatan, bistatus, 
        'bistatussebelumnya, bijmlrevisi, bicetakanke, biisclose, biinputuser, biinputtgl, bimodifikasiuser, 
        'bimodifikasitgl, biposting, bipostingtgl, bicustomtext1, bicustomtext2, bicustomtext3, bicustomtext4, 
        'bicustomtext5, bicustomint1, bicustomint2, bicustomint3, bicustomdbl1, bicustomdbl2, bicustomdbl3, 
        'bicustomdate1, bicustomdate2, bicustomdate3, bicabangnama, bilokasinama, bikontakkode, 
        'bikontaknama, bistatusnama, bistatussebelumnyanama, biinputusernama, bimodifikasiusernama, bikategoriposnama, bijeniskategori, bijenis

        'M12_BiGetdataById Detail -------------------------------------------------------
        'idbidetail, bikategori, idbarang, operator, jml1, jml2, customtext1, 
        'customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, tgl1, 
        'tgl2, nopromo, kodebarang, namabarang, catatan, urutan

        'M12_BiGetdataById Bonus -------------------------------------------------------
        'idbonus, idbidetail, idbarang, jml, satuan, customtext1, customtext2, 
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

        Dim utama As String = "", detail As String = "", bonus As String = "", idtransaksi As String = ""

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
            Filter = "biid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "biid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m12_bi_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("biid"), 0), sptField,
                     FxDB(drutama("bicabang"), ""), sptField,
                     FxDB(drutama("bilokasi"), ""), sptField,
                     FxDB(drutama("bisumber"), ""), sptField,
                     FxDB(drutama("bikategoripos"), ""), sptField,
                     FxDB(drutama("biautonotransaksi"), 0), sptField,
                     FxDB(drutama("binotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("bitgl"), ""), formatTgl), sptField,
                     FxDB(drutama("bikodepa"), ""), sptField,
                     FxDB(drutama("bikontak"), ""), sptField,
                     FxDB(drutama("bikontakperson"), ""), sptField,
                     FxDB(drutama("biuraian"), ""), sptField,
                     FxDB(drutama("bicatatan"), ""), sptField,
                     FxDB(drutama("bistatus"), 0), sptField,
                     FxDB(drutama("bistatussebelumnya"), 0), sptField,
                     FxDB(drutama("bijmlrevisi"), 0), sptField,
                     FxDB(drutama("bicetakanke"), 0), sptField,
                     FxDB(drutama("biisclose"), 0), sptField,
                     FxDB(drutama("biinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("biinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("bimodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("bimodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("biposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bipostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("bicustomtext1"), ""), sptField,
                     FxDB(drutama("bicustomtext2"), ""), sptField,
                     FxDB(drutama("bicustomtext3"), ""), sptField,
                     FxDB(drutama("bicustomtext4"), ""), sptField,
                     FxDB(drutama("bicustomtext5"), ""), sptField,
                     FxDB(drutama("bicustomint1"), 0), sptField,
                     FxDB(drutama("bicustomint2"), 0), sptField,
                     FxDB(drutama("bicustomint3"), 0), sptField,
                     FxDB(drutama("bicustomdbl1"), 0), sptField,
                     FxDB(drutama("bicustomdbl2"), 0), sptField,
                     FxDB(drutama("bicustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bicustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("bicustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("bicustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("bicabangnama"), ""), sptField,
                     FxDB(drutama("bilokasinama"), ""), sptField,
                     FxDB(drutama("bikontakkode"), ""), sptField,
                     FxDB(drutama("bikontaknama"), ""), sptField,
                     FxDB(drutama("bistatusnama"), ""), sptField,
                     FxDB(drutama("bistatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("biinputusernama"), ""), sptField,
                     FxDB(drutama("bimodifikasiusernama"), ""), sptField,
                     FxDB(drutama("bikategoriposnama"), ""), sptField,
                     FxDB(drutama("bijeniskategori"), 0), sptField,
                     FxDB(drutama("bijenis"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idbidetail"), 0), sptField,
                     FxDB(dr("idbi"), 0), sptField,
                     FxDB(dr("bikategori"), ""), sptField,
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
            Dim querybonus As New m0_query
            sql = "select `bib`.`idbonus` AS `idbonus`, `bib`.`idbidetail` AS `idbidetail`,`bib`.`idbi` AS `idbi`,`bib`.`idbarang` AS `idbarang`,`bib`.`jml` AS `jml`,`bib`.`satuan` AS `satuan`,`bib`.`customtext1` AS `customtext1`,`bib`.`customtext2` AS `customtext2`,`bib`.`customtext3` AS `customtext3`,`bib`.`customtext4` AS `customtext4`,`bib`.`customtext5` AS `customtext5`,`bib`.`customint1` AS `customint1`,`bib`.`customint2` AS `customint2`,`bib`.`customint3` AS `customint3`,`bib`.`customdbl1` AS `customdbl1`,`bib`.`customdbl2` AS `customdbl2`,`bib`.`customdbl3` AS `customdbl3`,`bib`.`customdate1` AS `customdate1`,`bib`.`customdate2` AS `customdate2`,`bib`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`i`.`bnama` AS `namabarang`,`bib`.`urutan` AS `urutan` FROM `m_12_bi_bonus` `bib` JOIN m1_item `i` ON (`bib`.`idbarang` = `i`.bid) WHERE `bib`.idbi='" & idtransaksi & "' ORDER BY `bib`.`urutan` ASC"
            Dim dtbonus As New DataTable
            dtbonus = AmbilData("aplikasi1-M_12_Bi_Bonus", "", "", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each drbonus As DataRow In dtbonus.Rows
                bonus = String.Concat(bonus,
                     FxDB(drbonus("idbonus"), 0), sptField,
                     FxDB(drbonus("idbidetail"), 0), sptField,
                     FxDB(drbonus("idbi"), 0), sptField,
                     FxDB(drbonus("idbarang"), 0), sptField,
                     FxDB(drbonus("jml"), 0), sptField,
                     FxDB(drbonus("satuan"), ""), sptField,
                     FxDB(drbonus("customtext1"), ""), sptField,
                     FxDB(drbonus("customtext2"), ""), sptField,
                     FxDB(drbonus("customtext3"), ""), sptField,
                     FxDB(drbonus("customtext4"), ""), sptField,
                     FxDB(drbonus("customtext5"), ""), sptField,
                     FxDB(drbonus("customint1"), 0), sptField,
                     FxDB(drbonus("customint2"), 0), sptField,
                     FxDB(drbonus("customint3"), 0), sptField,
                     FxDB(drbonus("customdbl1"), 0), sptField,
                     FxDB(drbonus("customdbl2"), 0), sptField,
                     FxDB(drbonus("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drbonus("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drbonus("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drbonus("customdate3"), ""), formatTgl), sptField,
                     FxDB(drbonus("kodebarang"), 0), sptField,
                     FxDB(drbonus("namabarang"), 0), sptField,
                     FxDB(drbonus("urutan"), 0), sptRow)
            Next
            bonus = bonus.Substring(0, bonus.Length - sptRow.Length)

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, bonus)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("biid, bicabang, bilokasi, bisumber, bikategoripos, biautonotransaksi, binotransaksi, bitgl, bikodepa, bikontak, bikontakperson, biuraian, bicatatan, bistatus, bistatussebelumnya, bijmlrevisi, bicetakanke, biisclose, biinputuser, biinputtgl, bimodifikasiuser, bimodifikasitgl, biposting, bipostingtgl, bicustomtext1, bicustomtext2, bicustomtext3, bicustomtext4, bicustomtext5, bicustomint1, bicustomint2, bicustomint3, bicustomdbl1, bicustomdbl2, bicustomdbl3, bicustomdate1, bicustomdate2, bicustomdate3, bicabangnama, bilokasinama, bikontakkode, bikontaknama, bistatusnama, bistatussebelumnyanama, biinputusernama, bimodifikasiusernama, bikategoriposnama, bijeniskategori, bijenis" & sptSubParam & "idbidetail, idbi, bikategori, idbarang, operator, jml1, jml2, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, tgl1, tgl2, nopromo, kodebarang, namabarang, catatan, urutan" & sptSubParam & "idbonus, idbidetail, idbi, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, namabarang, urutan"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_BiSearch(ByVal param As String) As String
        'M12_BiSearch --------------------------------------------------------
        'biid, bicabang, bilokasi, bisumber, bikategoripos, biautonotransaksi, binotransaksi, 
        'bitgl, bikodepa, bikontak, bikontakperson, biuraian, bicatatan, bistatus, 
        'bistatussebelumnya, bijmlrevisi, bicetakanke, biisclose, biinputuser, biinputtgl, bimodifikasiuser, 
        'bimodifikasitgl, biposting, bipostingtgl, bicustomtext1, bicustomtext2, bicustomtext3, bicustomtext4, 
        'bicustomtext5, bicustomint1, bicustomint2, bicustomint3, bicustomdbl1, bicustomdbl2, bicustomdbl3, 
        'bicustomdate1, bicustomdate2, bicustomdate3, bicabangnama, bilokasinama, bikontakkode, 
        'bikontaknama, bistatusnama, bistatussebelumnyanama, biinputusernama, bimodifikasiusernama

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
        sql = "select `bi`.`biid` AS `biid`,`bi`.`bicabang` AS `bicabang`,`bi`.`bilokasi` AS `bilokasi`,`bi`.`bisumber` AS `bisumber`,`bi`.`biautonotransaksi` AS `biautonotransaksi`,`bi`.`binotransaksi` AS `binotransaksi`,`bi`.`bitgl` AS `bitgl`,`bi`.`bikodepa` AS `bikodepa`,`bi`.`bikontak` AS `bikontak`,`bi`.`bikontakperson` AS `bikontakperson`,`bi`.`bikategoripos` AS `bikategoripos`,`bi`.`biuraian` AS `biuraian`,`bi`.`bicatatan` AS `bicatatan`,`bi`.`bistatus` AS `bistatus`,`bi`.`bistatussebelumnya` AS `bistatussebelumnya`,`bi`.`bijmlrevisi` AS `bijmlrevisi`,`bi`.`bicetakanke` AS `bicetakanke`,`bi`.`biisclose` AS `biisclose`,`bi`.`biinputuser` AS `biinputuser`,`bi`.`biinputtgl` AS `biinputtgl`,`bi`.`bimodifikasiuser` AS `bimodifikasiuser`,`bi`.`bimodifikasitgl` AS `bimodifikasitgl`,`bi`.`biposting` AS `biposting`,`bi`.`bipostingtgl` AS `bipostingtgl`,`bi`.`bicustomtext1` AS `bicustomtext1`,`bi`.`bicustomtext2` AS `bicustomtext2`,`bi`.`bicustomtext3` AS `bicustomtext3`,`bi`.`bicustomtext4` AS `bicustomtext4`,`bi`.`bicustomtext5` AS `bicustomtext5`,`bi`.`bicustomint1` AS `bicustomint1`,`bi`.`bicustomint2` AS `bicustomint2`,`bi`.`bicustomint3` AS `bicustomint3`,`bi`.`bicustomdbl1` AS `bicustomdbl1`,`bi`.`bicustomdbl2` AS `bicustomdbl2`,`bi`.`bicustomdbl3` AS `bicustomdbl3`,`bi`.`bicustomdate1` AS `bicustomdate1`,`bi`.`bicustomdate2` AS `bicustomdate2`,`bi`.`bicustomdate3` AS `bicustomdate3`,`br`.`bnama` AS `bicabangnama`,`lc`.`lnama` AS `bilokasinama`,`c`.`kkode` AS `bikontakkode`,`c`.`knama` AS `bikontaknama`,`st1`.`nama` AS `bistatusnama`,`st2`.`nama` AS `bistatussebelumnyanama`,`u1`.`unama` AS `biinputusernama`,`u2`.`unama` AS `bimodifikasiusernama` from (((((((`m_12_bi` `bi` left join `m1_branch` `br` on((`bi`.`bicabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`bi`.`bilokasi` = `lc`.`lkode`))) left join `m1_contact` `c` on((`bi`.`bikontak` = `c`.`kid`))) left join `m0_status` `st1` on((`bi`.`bistatus` = `st1`.`kode`))) left join `m0_status` `st2` on((`bi`.`bistatussebelumnya` = `st2`.`kode`))) left join `m0_user` `u1` on((`bi`.`biinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`bi`.`bimodifikasiuser` = `u2`.`userid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M12_Bi", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("biid"), 0), sptField,
                             FxDB(dr("bicabang"), ""), sptField,
                             FxDB(dr("bilokasi"), ""), sptField,
                             FxDB(dr("bisumber"), ""), sptField,
                             FxDB(dr("bikategoripos"), ""), sptField,
                             FxDB(dr("biautonotransaksi"), 0), sptField,
                             FxDB(dr("binotransaksi"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("bitgl"), ""), formatTgl), sptField,
                             FxDB(dr("bikodepa"), ""), sptField,
                             FxDB(dr("bikontak"), ""), sptField,
                             FxDB(dr("bikontakperson"), ""), sptField,
                             FxDB(dr("biuraian"), ""), sptField,
                             FxDB(dr("bicatatan"), ""), sptField,
                             FxDB(dr("bistatus"), 0), sptField,
                             FxDB(dr("bistatussebelumnya"), 0), sptField,
                             FxDB(dr("bijmlrevisi"), 0), sptField,
                             FxDB(dr("bicetakanke"), 0), sptField,
                             FxDB(dr("biisclose"), 0), sptField,
                             FxDB(dr("biinputuser"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("biinputtgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("bimodifikasiuser"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("bimodifikasitgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("biposting"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("bipostingtgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("bicustomtext1"), ""), sptField,
                             FxDB(dr("bicustomtext2"), ""), sptField,
                             FxDB(dr("bicustomtext3"), ""), sptField,
                             FxDB(dr("bicustomtext4"), ""), sptField,
                             FxDB(dr("bicustomtext5"), ""), sptField,
                             FxDB(dr("bicustomint1"), 0), sptField,
                             FxDB(dr("bicustomint2"), 0), sptField,
                             FxDB(dr("bicustomint3"), 0), sptField,
                             FxDB(dr("bicustomdbl1"), 0), sptField,
                             FxDB(dr("bicustomdbl2"), 0), sptField,
                             FxDB(dr("bicustomdbl3"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("bicustomdate1"), ""), formatTgl), sptField,
                             AsFormatTanggal(FxDB(dr("bicustomdate2"), ""), formatTgl), sptField,
                             AsFormatTanggal(FxDB(dr("bicustomdate3"), ""), formatTgl), sptField,
                             FxDB(dr("bicabangnama"), ""), sptField,
                             FxDB(dr("bilokasinama"), ""), sptField,
                             FxDB(dr("bikontakkode"), ""), sptField,
                             FxDB(dr("bikontaknama"), ""), sptField,
                             FxDB(dr("bistatusnama"), ""), sptField,
                             FxDB(dr("bistatussebelumnyanama"), ""), sptField,
                             FxDB(dr("biinputusernama"), ""), sptField,
                             FxDB(dr("bimodifikasiusernama"), ""), sptRow)

            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = pg1.isPaging
            resultPaging(1) = pg1.isNext
            resultPaging(2) = pg1.isPrev
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Transaction data not found. - 1"
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("biid, bicabang, bilokasi, bisumber, bikategoripos, biautonotransaksi, binotransaksi, bitgl, bikodepa, bikontak, bikontakperson, biuraian, bicatatan, bistatus, bistatussebelumnya, bijmlrevisi, bicetakanke, biisclose, biinputuser, biinputtgl, bimodifikasiuser, bimodifikasitgl, biposting, bipostingtgl, bicustomtext1, bicustomtext2, bicustomtext3, bicustomtext4, bicustomtext5, bicustomint1, bicustomint2, bicustomint3, bicustomdbl1, bicustomdbl2, bicustomdbl3, bicustomdate1, bicustomdate2, bicustomdate3, bicabangnama, bilokasinama, bikontakkode, bikontaknama, bistatusnama, bistatussebelumnyanama, biinputusernama, bimodifikasiusernama"))

        Return wsResult
    End Function

End Class
