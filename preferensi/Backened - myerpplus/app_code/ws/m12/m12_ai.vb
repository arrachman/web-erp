Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _ 
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_ai
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_AiSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataAdditional(), dataRowAdditional() As String

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
        'aiid(0) As Integer, aicabang(1) As String, ailokasi(2) As String, aisumber(3) As String, aikategoripos(4) As String, 
        'aiautonotransaksi(5) As Integer, ainotransaksi(6) As String, aitgl(7) As Date, aikodepa(8) As , aikontak(9) As , 
        'aikontakperson(10) As String, aiuraian(11) As String, aicatatan(12) As String, aistatus(13) As Integer, aistatussebelumnya(14) As Integer, 
        'aijmlrevisi(15) As Integer, aicetakanke(16) As Integer, aiisclose(17) As Integer, aiinputuser(18) As , aiinputtgl(19) As DateTime, 
        'aimodifikasiuser(20) As , aimodifikasitgl(21) As DateTime, aiposting(22) As Integer, aipostingtgl(23) As DateTime, aicustomtext1(24) As String, 
        'aicustomtext2(25) As String, aicustomtext3(26) As String, aicustomtext4(27) As String, aicustomtext5(28) As String, aicustomint1(29) As Integer, 
        'aicustomint2(30) As Integer, aicustomint3(31) As Integer, aicustomdbl1(32) As Double, aicustomdbl2(33) As Double, aicustomdbl3(34) As Double, 
        'aicustomdate1(35) As Date, aicustomdate2(36) As Date, aicustomdate3(37) As Date, aijeniskategori(38) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'aiid, aicabang, ailokasi, aisumber, aikategoripos, aiautonotransaksi, ainotransaksi, 
        'aitgl, aikodepa, aikontak, aikontakperson, aiuraian, aicatatan, aistatus, 
        'aistatussebelumnya, aijmlrevisi, aicetakanke, aiisclose, aiinputuser, aiinputtgl, aimodifikasiuser, 
        'aimodifikasitgl, aiposting, aipostingtgl, aicustomtext1, aicustomtext2, aicustomtext3, aicustomtext4, 
        'aicustomtext5, aicustomint1, aicustomint2, aicustomint3, aicustomdbl1, aicustomdbl2, aicustomdbl3, 
        'aicustomdate1, aicustomdate2, aicustomdate3, aijeniskategori

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 39) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'aiid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "aiid required numeric." : GoTo selesai
        End If
        'aiautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "aiautonotransaksi required numeric." : GoTo selesai
        End If
        'aitgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "aitgl required date." : GoTo selesai
        End If
        'aistatus(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "aistatus required numeric." : GoTo selesai
        End If
        'aistatussebelumnya(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "aistatussebelumnya required numeric." : GoTo selesai
        End If
        'aijmlrevisi(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "aijmlrevisi required numeric." : GoTo selesai
        End If
        'aicetakanke(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "aicetakanke required numeric." : GoTo selesai
        End If
        'aiisclose(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "aiisclose required numeric." : GoTo selesai
        End If
        'aiinputtgl(19) As DateTime
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "aiinputtgl required date." : GoTo selesai
        End If
        'aimodifikasitgl(21) As DateTime
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "aimodifikasitgl required date." : GoTo selesai
        End If
        'aiposting(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "aiposting required numeric." : GoTo selesai
        End If
        'aipostingtgl(23) As DateTime
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "aipostingtgl required date." : GoTo selesai
        End If
        'aicustomint1(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "aicustomint1 required numeric." : GoTo selesai
        End If
        'aicustomint2(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "aicustomint2 required numeric." : GoTo selesai
        End If
        'aicustomint3(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "aicustomint3 required numeric." : GoTo selesai
        End If
        'aicustomdbl1(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "aicustomdbl1 required numeric." : GoTo selesai
        End If
        'aicustomdbl2(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "aicustomdbl2 required numeric." : GoTo selesai
        End If
        'aicustomdbl3(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "aicustomdbl3 required numeric." : GoTo selesai
        End If
        'aicustomdate1(35) As Date
        If (IsDate(dataUtama(35)) = False) Then
            result(2) = "aicustomdate1 required date." : GoTo selesai
        End If
        'aicustomdate2(36) As Date
        If (IsDate(dataUtama(36)) = False) Then
            result(2) = "aicustomdate2 required date." : GoTo selesai
        End If
        'aicustomdate3(37) As Date
        If (IsDate(dataUtama(37)) = False) Then
            result(2) = "aicustomdate3 required date." : GoTo selesai
        End If

        'aijeniskategori(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "aijeniskategori required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'aicabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "aicabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "aicabang should not be more than 25 character." : GoTo selesai
        End If

        'ailokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "ailokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "ailokasi should not be more than 25 character." : GoTo selesai
        End If

        'aisumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "aisumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "aisumber should not be more than 10 character." : GoTo selesai
        End If

        'aikategoripos(4) As String
        'If Len(dataUtama(4)) = 0 Then
        '    result(2) = "aikategoripos can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(4)) > 50 Then
            result(2) = "aikategoripos should not be more than 50 character." : GoTo selesai
        End If

        'ainotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "ainotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "ainotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'aitgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "aitgl can't be empty" : GoTo selesai
        End If

        'aikodepa(8) As 
        If Len(dataUtama(8)) = 0 Then
            result(2) = "aikodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 20 Then
            result(2) = "aikodepa should not be more than 20 character." : GoTo selesai
        End If

        'aikontak(9) As 
        If Len(dataUtama(9)) = 0 Then
            result(2) = "aikontak can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 20 Then
            result(2) = "aikontak should not be more than 20 character." : GoTo selesai
        End If

        'aiinputtgl(19) As DateTime
        If Len(dataUtama(19)) = 0 Then
            result(2) = "aiinputtgl can't be empty" : GoTo selesai
        End If

        'aimodifikasitgl(21) As DateTime
        If Len(dataUtama(21)) = 0 Then
            result(2) = "aimodifikasitgl can't be empty" : GoTo selesai
        End If

        'aipostingtgl(23) As DateTime
        If Len(dataUtama(23)) = 0 Then
            result(2) = "aipostingtgl can't be empty" : GoTo selesai
        End If

        'aicustomdbl1(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "aicustomdbl1 can't be empty" : GoTo selesai
        End If

        'aicustomdbl2(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "aicustomdbl2 can't be empty" : GoTo selesai
        End If

        'aicustomdbl3(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "aicustomdbl3 can't be empty" : GoTo selesai
        End If

        'aicustomdate1(35) As Date
        If Len(dataUtama(35)) = 0 Then
            result(2) = "aicustomdate1 can't be empty" : GoTo selesai
        End If

        'aicustomdate2(36) As Date
        If Len(dataUtama(36)) = 0 Then
            result(2) = "aicustomdate2 can't be empty" : GoTo selesai
        End If

        'aicustomdate3(37) As Date
        If Len(dataUtama(37)) = 0 Then
            result(2) = "aicustomdate3 can't be empty" : GoTo selesai
        End If



        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "aiid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ailokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aisumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aikategoripos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aiautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ainotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aikodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aikontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aikontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aiuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aistatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aistatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aijmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aicetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aiisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aiinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aiinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aimodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aimodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aiposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aipostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aijeniskategori", AsEnumTypeData.AsInt64)

        If AsDataTableTambahData(dtutama, "aiid~aicabang~ailokasi~aisumber~aikategoripos~aiautonotransaksi~ainotransaksi~aitgl~aikodepa~aikontak~aikontakperson~aiuraian~aicatatan~aistatus~aistatussebelumnya~aijmlrevisi~aicetakanke~aiisclose~aiinputuser~aiinputtgl~aimodifikasiuser~aimodifikasitgl~aiposting~aipostingtgl~aicustomtext1~aicustomtext2~aicustomtext3~aicustomtext4~aicustomtext5~aicustomint1~aicustomint2~aicustomint3~aicustomdbl1~aicustomdbl2~aicustomdbl3~aicustomdate1~aicustomdate2~aicustomdate3~aijeniskategori", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38)) = False Then
            result(2) = "insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idaidetail(0) As , idai(1) As , aikategori(2) As String, idbarang(3) As , operator(4) As String, 
        'jml1(5) As Double, jml2(6) As Double, customtext1(7) As String, customtext2(8) As String, customtext3(9) As String, 
        'customtext4(10) As String, customtext5(11) As String, customint1(12) As Integer, customint2(13) As Integer, customint3(14) As Integer, 
        'customdbl1(15) As Double, customdbl2(16) As Double, customdbl3(17) As Double, customdate1(18) As Date, customdate2(19) As Date, 
        'customdate3(20) As Date, tgl1(21) As Date, tgl2(22) As Date, nopromo(23) As String, nogrup (24) As String, catatan (25) As String, urutan(26) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idaidetail, idai, aikategori, idbarang, operator, jml1, jml2, 
        'customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, 
        'customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'tgl1, tgl2, nopromo, nogrup, catatan, urutan

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idaidetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idai", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "aikategori", AsEnumTypeData.AsString)
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
                result(2) = "Row : " & i & " - idaidetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idaidetail should not be more than 20 character." : GoTo selesai
            End If

            'idbi(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idai can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idai should not be more than 20 character." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idaidetail~idai~aikategori~idbarang~operator~jml1~jml2~customtext1~customtext2~customtext3~customtext4~customtext5~customint1~customint2~customint3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~tgl1~tgl2~nopromo~nogrup~catatan~urutan", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26)) = False Then
                result(2) = "Detail Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================

        'MAPPING BUAT WS DATA BONUS -------------------------------------------------------
        'idadditional(0) As , idai(1) As , idaidetail(2) As , idbarang(3) As , jml(4) As Double, 
        'satuan(5) As String, customtext1(6) As String, customtext2(7) As String, customtext3(8) As String, customtext4(9) As String, 
        'customtext5(10) As String, customint1(11) As Integer, customint2(12) As Integer, customint3(13) As Integer, customdbl1(14) As Double, 
        'customdbl2(15) As Double, customdbl3(16) As Double, customdate1(17) As Date, customdate2(18) As Date, customdate3(19) As Date, nogrup(20) As String

        'MAPPING BUAT FLEX DATA BONUS -----------------------------------------------------
        'idadditional, idai, idaidetail, idbarang, jml, satuan, customtext1, 
        'customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA BONUS ======================================================
        'SPLIT PARAMETER DATA BONUS
        dataAdditional = dataSplit(2).Split(sptRow)
        'END OF VALIDASI DAN SET DATA BONUS ===============================================

        'Buat datatable additional
        Dim dtadditional As New DataTable
        AsDataTableTambahField(dtadditional, "idadditional", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtadditional, "idai", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtadditional, "idaidetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtadditional, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtadditional, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "customint1", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtadditional, "customint2", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtadditional, "customint3", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtadditional, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "nogrup", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "urutan", AsEnumTypeData.AsInt64)

        'VALIDASI DAN SET DATA ROW BONUS ==================================================
        Dim JmlDtAdditional As Integer = dataAdditional.Length
        For i = 1 To JmlDtAdditional
            'SPLIT DATA DETAIL
            dataRowAdditional = dataAdditional(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA BONUS -----------------------------------
            'CEK ARRAY DATA BONUS
            If (dataRowAdditional.Length <> 22) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW BONUS ----------------------------

            'VALIDASI TIPE DATA BONUS ------------------------------------------
            'jml(4) As Double
            If (IsNumeric(dataRowAdditional(4)) = False) Then
                result(2) = "jml required numeric." : GoTo selesai
            End If
            'customint1(11) As Integer
            If (IsNumeric(dataRowAdditional(11)) = False) Then
                result(2) = "customint1 required numeric." : GoTo selesai
            End If
            'customint2(12) As Integer
            If (IsNumeric(dataRowAdditional(12)) = False) Then
                result(2) = "customint2 required numeric." : GoTo selesai
            End If
            'customint3(13) As Integer
            If (IsNumeric(dataRowAdditional(13)) = False) Then
                result(2) = "customint3 required numeric." : GoTo selesai
            End If
            'customdbl1(14) As Double
            If (IsNumeric(dataRowAdditional(14)) = False) Then
                result(2) = "customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(15) As Double
            If (IsNumeric(dataRowAdditional(15)) = False) Then
                result(2) = "customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(16) As Double
            If (IsNumeric(dataRowAdditional(16)) = False) Then
                result(2) = "customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(17) As Date
            If (IsDate(dataRowAdditional(17)) = False) Then
                result(2) = "customdate1 required date." : GoTo selesai
            End If
            'customdate2(18) As Date
            If (IsDate(dataRowAdditional(18)) = False) Then
                result(2) = "customdate2 required date." : GoTo selesai
            End If
            'customdate3(19) As Date
            If (IsDate(dataRowAdditional(19)) = False) Then
                result(2) = "customdate3 required date." : GoTo selesai
            End If
            'urutan(21) As Double
            If (IsNumeric(dataRowAdditional(21)) = False) Then
                result(2) = "urutan required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA BONUS -----------------------------------

            'VALIDASI DATA BONUS ---------------------------------------
            'idadditional(0) As 
            If Len(dataRowAdditional(0)) = 0 Then
                result(2) = "Row : " & i & " - idadditional can't be empty" : GoTo selesai
            End If
            If Len(dataRowAdditional(0)) > 20 Then
                result(2) = "Row : " & i & " - idadditional should not be more than 20 character." : GoTo selesai
            End If

            'idai(1) As 
            If Len(dataRowAdditional(1)) = 0 Then
                result(2) = "Row : " & i & " - idai can't be empty" : GoTo selesai
            End If
            If Len(dataRowAdditional(1)) > 20 Then
                result(2) = "Row : " & i & " - idai should not be more than 20 character." : GoTo selesai
            End If

            'idaidetail(2) As 
            If Len(dataRowAdditional(2)) = 0 Then
                result(2) = "Row : " & i & " - idaidetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowAdditional(2)) > 20 Then
                result(2) = "Row : " & i & " - idaidetail should not be more than 20 character." : GoTo selesai
            End If

            'idbarang(3) As 
            If Len(dataRowAdditional(3)) = 0 Then
                result(2) = "Row : " & i & " - idbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowAdditional(3)) > 20 Then
                result(2) = "Row : " & i & " - idbarang should not be more than 20 character." : GoTo selesai
            End If

            'jml(4) As Double
            If Len(dataRowAdditional(4)) = 0 Then
                result(2) = "Row : " & i & " - jml can't be empty" : GoTo selesai
            End If

            'satuan(5) As String
            If Len(dataRowAdditional(5)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowAdditional(5)) > 25 Then
                result(2) = "Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'customdbl1(14) As Double
            If Len(dataRowAdditional(14)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(15) As Double
            If Len(dataRowAdditional(15)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(16) As Double
            If Len(dataRowAdditional(16)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(17) As Date
            If Len(dataRowAdditional(17)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(18) As Date
            If Len(dataRowAdditional(18)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(19) As Date
            If Len(dataRowAdditional(19)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'urutan(21) As Date
            If Len(dataRowAdditional(21)) = 0 Then
                result(2) = "Row : " & i & " - urutan can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtadditional, "idadditional~idai~idaidetail~idbarang~jml~satuan~customtext1~customtext2~customtext3~customtext4~customtext5~customint1~customint2~customint3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~nogrup~urutan", dataRowAdditional(0) & "~" & dataRowAdditional(1) & "~" & dataRowAdditional(2) & "~" & dataRowAdditional(3) & "~" & dataRowAdditional(4) & "~" & dataRowAdditional(5) & "~" & dataRowAdditional(6) & "~" & dataRowAdditional(7) & "~" & dataRowAdditional(8) & "~" & dataRowAdditional(9) & "~" & dataRowAdditional(10) & "~" & dataRowAdditional(11) & "~" & dataRowAdditional(12) & "~" & dataRowAdditional(13) & "~" & dataRowAdditional(14) & "~" & dataRowAdditional(15) & "~" & dataRowAdditional(16) & "~" & dataRowAdditional(17) & "~" & dataRowAdditional(18) & "~" & dataRowAdditional(19) & "~" & dataRowAdditional(20) & "~" & dataRowAdditional(21)) = False Then
                result(2) = "Additional Row : " & i & " - insert into datatable failed." : GoTo selesai
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
                If isUpdate Then
                    result(4) = drutama("aiid")
                    notransaksi = drutama("ainotransaksi")

                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(aiid), ainotransaksi FROM M_12_Ai WHERE aiid=" & result(4), myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(aiid) FROM M_12_Ai WHERE ainotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        ''SIMPAN HISTORY ========================
                        'Dim SimpanHistory As New m12_ai_history
                        'Dim rsSimpanHistory As String = SimpanHistory.M12_Bi_HistorySimpan("" & paramSplit(0) & "★M12_Bi_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("bisumber")) & "▼" & FixQuotes(drutama("biid")) & "")
                        'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        'If (rsSplitResult(1) = 0) Then
                        '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        'End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M_12_Ai set aicabang  = '" & FixQuotes(drutama("aicabang")) & "', ailokasi  = '" & FixQuotes(drutama("ailokasi")) & "', aisumber  = '" & FixQuotes(drutama("aisumber")) & "', aikategoripos  = '" & FixQuotes(drutama("aikategoripos")) & "', aiautonotransaksi  = " & drutama("aiautonotransaksi") & ", ainotransaksi  = '" & FixQuotes(drutama("ainotransaksi")) & "', aitgl  = '" & FixQuotes(AsFormatTanggal(drutama("aitgl"))) & "', aikodepa  = '" & FixQuotes(drutama("aikodepa")) & "', aikontak  = '" & FixQuotes(drutama("aikontak")) & "', aikontakperson  = '" & FixQuotes(drutama("aikontakperson")) & "', aiuraian  = '" & FixQuotes(drutama("aiuraian")) & "', aicatatan  = '" & FixQuotes(drutama("aicatatan")) & "', aistatus  = " & drutama("aistatus") & ", aistatussebelumnya  = " & drutama("aistatussebelumnya") & ", aijmlrevisi  = " & drutama("aijmlrevisi") & ", aicetakanke  = " & drutama("aicetakanke") & ", aiisclose  = " & drutama("aiisclose") & ", aiinputuser  = '" & FixQuotes(drutama("aiinputuser")) & "', aimodifikasiuser  = '" & FixQuotes(drutama("aimodifikasiuser")) & "', aimodifikasitgl  = NOW(), aiposting  = " & drutama("aiposting") & ", aipostingtgl  = '" & FixQuotes(AsFormatTanggal(drutama("aipostingtgl"), "yyyy-MM-dd H:mm:ss")) & "', aicustomtext1  = '" & FixQuotes(drutama("aicustomtext1")) & "', aicustomtext2  = '" & FixQuotes(drutama("aicustomtext2")) & "', aicustomtext3  = '" & FixQuotes(drutama("aicustomtext3")) & "', aicustomtext4  = '" & FixQuotes(drutama("aicustomtext4")) & "', aicustomtext5  = '" & FixQuotes(drutama("aicustomtext5")) & "', aicustomint1  = " & drutama("aicustomint1") & ", aicustomint2  = " & drutama("aicustomint2") & ", aicustomint3  = " & drutama("aicustomint3") & ", aicustomdbl1  = '" & FixDouble(drutama("aicustomdbl1")) & "', aicustomdbl2  = '" & FixDouble(drutama("aicustomdbl2")) & "', aicustomdbl3  = '" & FixDouble(drutama("aicustomdbl3")) & "', aicustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("aicustomdate1"))) & "', aicustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("aicustomdate2"))) & "', aicustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("aicustomdate3"))) & "', aijeniskategori  = '" & FixQuotes(drutama("aijeniskategori")) & "' where aiid = " & drutama("aiid") & ""
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

                    If drutama("aiautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("aicabang"), drutama("ailokasi"), drutama("aisumber"), drutama("aitgl"))
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
                        notransaksi = drutama("ainotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(aiid) FROM m_12_ai WHERE ainotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M_12_Ai (aicabang, ailokasi, aisumber, aikategoripos, aiautonotransaksi, ainotransaksi, aitgl, aikodepa, aikontak, aikontakperson, aiuraian, aicatatan, aistatus, aistatussebelumnya, aijmlrevisi, aicetakanke, aiisclose, aiinputuser, aiinputtgl, aimodifikasiuser, aimodifikasitgl, aiposting, aipostingtgl, aicustomtext1, aicustomtext2, aicustomtext3, aicustomtext4, aicustomtext5, aicustomint1, aicustomint2, aicustomint3, aicustomdbl1, aicustomdbl2, aicustomdbl3, aicustomdate1, aicustomdate2, aicustomdate3, aijeniskategori) values('" & FixQuotes(drutama("aicabang")) & "', '" & FixQuotes(drutama("ailokasi")) & "', '" & FixQuotes(drutama("aisumber")) & "', '" & FixQuotes(drutama("aikategoripos")) & "', " & drutama("aiautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("aitgl"))) & "', '" & FixQuotes(drutama("aikodepa")) & "', '" & FixQuotes(drutama("aikontak")) & "', '" & FixQuotes(drutama("aikontakperson")) & "', '" & FixQuotes(drutama("aiuraian")) & "', '" & FixQuotes(drutama("aicatatan")) & "', " & drutama("aistatus") & ", " & drutama("aistatussebelumnya") & ", " & drutama("aijmlrevisi") & ", " & drutama("aicetakanke") & ", " & drutama("aiisclose") & ", '" & FixQuotes(drutama("aiinputuser")) & "', NOW(), '" & FixQuotes(drutama("aimodifikasiuser")) & "', '1971-01-01 00:00:00', 0, '1971-01-01 00:00:00', '" & FixQuotes(drutama("aicustomtext1")) & "', '" & FixQuotes(drutama("aicustomtext2")) & "', '" & FixQuotes(drutama("aicustomtext3")) & "', '" & FixQuotes(drutama("aicustomtext4")) & "', '" & FixQuotes(drutama("aicustomtext5")) & "', " & drutama("aicustomint1") & ", " & drutama("aicustomint2") & ", " & drutama("aicustomint3") & ", '" & FixDouble(drutama("aicustomdbl1")) & "', '" & FixDouble(drutama("aicustomdbl2")) & "', '" & FixDouble(drutama("aicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("aicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("aicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("aicustomdate3"))) & "', " & drutama("aijeniskategori") & ")"
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
                    dt2 = AsDataTableAmbilDariDBCon("select aiid from M_12_ai where ainotransaksi='" & notransaksi & "' AND aiinputuser= '" & drutama("aiinputuser") & "' order by aimodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M_12_Ai_Detail where idai = " & result(4)
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Hapus additional ketika update
                If (isUpdate) Then
                    sql = "Delete from M_12_Ai_Additional where idai = " & result(4)
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
                    Dim dtAdditionalGrup As New DataTable

                    For Each dr1 As DataRow In dtdetail.Rows

                        'CEK OPERATOR :
                        'JIKA BETWEEN (0) ATAU >= (1) MAKA BOLEH LEBIH DARI SATU KONDISI 
                        '=> BEBERAPA KONDISI BETWEEN DAN SATU KONDISI >= (1)
                        'JIKA KELIPATAN (2) MAKA HANYA BOLEH SATU KONDISI
                        Dim dtOperator As New DataTable
                        sql = "SELECT aid.aikategori as kategori, aid.idbarang as idbarang, aid.operator as operator, i.bkode, (CASE aid.operator WHEN 0 THEN 'Between' WHEN 1 THEN '>=' WHEN 2 THEN 'Multiple' ELSE 'Unknown' END) as operatornama FROM m_12_ai_detail aid JOIN m1_item i ON aid.idbarang = i.bid WHERE aid.aikategori = '" & FxDB(drutama("aikategoripos"), "") & "' AND aid.idbarang = '" & FxDB(dr1("idbarang"), "") & "' AND aid.idai = '" & result(4) & "' AND aid.idaidetail <> '" & FxDB(dr1("idaidetail"), "") & "' GROUP BY aid.operator ORDER BY aid.operator"
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
                        strValue2.Append("('" & FixQuotes(dr1("idaidetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("aikategori")) & "', '" & FixQuotes(dr1("idbarang")) & "', '" & FixQuotes(dr1("operator")) & "', '" & FixDouble(dr1("jml1")) & "', '" & FixDouble(dr1("jml2")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl2"))) & "', '" & FixQuotes(dr1("nopromo")) & "')")

                        'sql = "Insert into M_12_Bi_Detail(idbidetail, idbi, bikategori, idbarang, operator, jml1, jml2, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, tgl1, tgl2, nopromo) values" & strValue2.ToString & ""
                        sql = "Insert into M_12_Ai_Detail(idaidetail, idai, aikategori, idbarang, operator, jml1, jml2, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, tgl1, tgl2, nopromo, catatan, urutan) values('" & FixQuotes(dr1("idaidetail")) & "', " & result(4) & ", '" & FixQuotes(drutama("aikategoripos")) & "', '" & FixQuotes(dr1("idbarang")) & "', '" & FixQuotes(dr1("operator")) & "', '" & FixDouble(dr1("jml1")) & "', '" & FixDouble(dr1("jml2")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl2"))) & "', '" & notransaksi & "', '" & FixQuotes(dr1("catatan")) & "','" & FixQuotes(dr1("urutan")) & "')"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()


                        'ambil ID detail untuk diinsert ke additional
                        Dim iddetail As Integer
                        Dim dtidadditional As New DataTable
                        'Sql disesuaikan sendiri, untuk parameternya disesuaikan sendiri.
                        dtidadditional = AsDataTableAmbilDariDBCon("select idaidetail from M_12_ai_detail where idai='" & result(4) & "' and aikategori = '" & drutama("aikategoripos") & "' AND  idbarang = '" & dr1("idbarang") & "' AND  operator = '" & dr1("operator") & "' AND  jml1 = '" & dr1("jml1") & "' AND jml2 = '" & dr1("jml2") & "' order by idaidetail desc limit 1", myConn)
                        If dtidadditional.Rows.Count > 0 Then iddetail = dtidadditional.Rows(0)(0) Else result(2) = "#1 Detail transaction data not found." : Trans.Rollback() : GoTo selesai

                        'Proses Additional
                        If (dtadditional.Rows.Count > 0) Then
                            'AMBIL DETAIL BONUS SESUAI NO GRUP
                            dtAdditionalGrup = AsDataTableFilterSortDt(dtadditional, "nogrup = '" & dr1("nogrup") & "'")
                            If (dtAdditionalGrup.Rows.Count > 0) Then
                                strValue2.Clear()
                                For Each drAdditionalGrup As DataRow In dtAdditionalGrup.Rows
                                    strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                                    strValue2.Append("('" & FixQuotes(drAdditionalGrup("idadditional")) & "', " & result(4) & ", '" & iddetail & "', '" & FixQuotes(drAdditionalGrup("idbarang")) & "', '" & FixDouble(drAdditionalGrup("jml")) & "', '" & FixQuotes(drAdditionalGrup("satuan")) & "', '" & FixQuotes(drAdditionalGrup("customtext1")) & "', '" & FixQuotes(drAdditionalGrup("customtext2")) & "', '" & FixQuotes(drAdditionalGrup("customtext3")) & "', '" & FixQuotes(drAdditionalGrup("customtext4")) & "', '" & FixQuotes(drAdditionalGrup("customtext5")) & "', " & drAdditionalGrup("customint1") & ", " & drAdditionalGrup("customint2") & ", " & drAdditionalGrup("customint3") & ", '" & FixDouble(drAdditionalGrup("customdbl1")) & "', '" & FixDouble(drAdditionalGrup("customdbl2")) & "', '" & FixDouble(drAdditionalGrup("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drAdditionalGrup("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drAdditionalGrup("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drAdditionalGrup("customdate3"))) & "', '" & FixQuotes(drAdditionalGrup("urutan")) & "')")
                                Next

                                sql = "Insert into M_12_Ai_Additional(idadditional, idai, idaidetail,  idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, urutan) values" & strValue2.ToString & ""
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                            Else
                                result(2) = "Additional Transaction for No. Group : " & dr1("nogrup") & " does not found." : Trans.Rollback() : GoTo selesai
                            End If

                        Else
                            result(2) = "Additional Transaction data not found." : Trans.Rollback() : GoTo selesai
                        End If
                    Next

                Else
                    result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Update ke tabel Barang Additional
                If drutama("aistatus") = 2 Then
                    'JIKA PER KATEGORI, HAPUS DATA PER KATEGORI
                    If drutama("aijeniskategori") = 1 Then
                        'Cek apakah kategori pos sudah ada di tabel pos_additional_item, jika sudah ada, hapus data di tabel itu
                        Dim dtPOSAdditionalItem As New DataTable
                        dtPOSAdditionalItem = AsDataTableAmbilDariDBCon("select aiid from M_12_Pos_Additional_Item where aikategori = '" & drutama("aikategoripos") & "'", myConn)
                        Dim strValueItemUtama As New StringBuilder
                        Dim strValueItemDetail As New StringBuilder
                        If dtPOSAdditionalItem.Rows.Count > 0 Then
                            For Each drPOSAdditionalItem As DataRow In dtPOSAdditionalItem.Rows
                                'QUERY HAPUS POS BONUS ITEM UTAMA
                                strValueItemUtama.Append(IIf(Len(strValueItemUtama.ToString) = 0, "", " OR "))
                                strValueItemUtama.Append("aiid = '" & FixQuotes(drPOSAdditionalItem("aiid")) & "'")

                                'QUERY HAPUS POS BONUS ITEM DETAIL
                                strValueItemDetail.Append(IIf(Len(strValueItemDetail.ToString) = 0, "", " OR "))
                                strValueItemDetail.Append("idai = '" & FixQuotes(drPOSAdditionalItem("aiid")) & "'")
                            Next

                            'HAPUS POS BONUS ITEM UTAMA
                            sql = "Delete From m_12_pos_additional_item where " & strValueItemUtama.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                            'HAPUS POS BONUS ITEM DETAIL
                            sql = "Delete From m_12_pos_additional_item_detail where " & strValueItemDetail.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If

                    ElseIf drutama("aijeniskategori") = 2 Then 'PER CABANG
                        'ambil kategori pos sesuai cabang
                        Dim dtCatPOS As DataTable = AsDataTableAmbilDariDBCon("SELECT GROUP_CONCAT(" & Chr(34) & "'" & Chr(34) & ",l.lkategoripos," & Chr(34) & "'" & Chr(34) & ") as kategoripos FROM m1_location l WHERE l.lkategoripos <> '' AND l.lcabang = '" & FixQuotes(drutama("aicabang")) & "'", myConn)
                        If dtCatPOS.Rows.Count > 0 Then
                            If Len(FxDB(dtCatPOS.Rows(0)(0), "")) > 0 Then
                                'Cek apakah kategori pos sudah ada di tabel pos_additional_item, jika sudah ada, hapus data di tabel itu
                                Dim dtPOSAdditionalItem As New DataTable
                                dtPOSAdditionalItem = AsDataTableAmbilDariDBCon("select aiid from M_12_Pos_Additional_Item where aikategori IN (" & dtCatPOS.Rows(0)(0) & ")", myConn)
                                Dim strValueItemUtama As New StringBuilder
                                Dim strValueItemDetail As New StringBuilder
                                If dtPOSAdditionalItem.Rows.Count > 0 Then
                                    For Each drPOSAdditionalItem As DataRow In dtPOSAdditionalItem.Rows
                                        'QUERY HAPUS POS BONUS ITEM UTAMA
                                        strValueItemUtama.Append(IIf(Len(strValueItemUtama.ToString) = 0, "", " OR "))
                                        strValueItemUtama.Append("aiid = '" & FixQuotes(drPOSAdditionalItem("aiid")) & "'")

                                        'QUERY HAPUS POS BONUS ITEM DETAIL
                                        strValueItemDetail.Append(IIf(Len(strValueItemDetail.ToString) = 0, "", " OR "))
                                        strValueItemDetail.Append("idai = '" & FixQuotes(drPOSAdditionalItem("aiid")) & "'")
                                    Next

                                    'HAPUS POS BONUS ITEM UTAMA
                                    sql = "Delete From m_12_pos_additional_item where " & strValueItemUtama.ToString & ""
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()

                                    'HAPUS POS BONUS ITEM DETAIL
                                    sql = "Delete From m_12_pos_additional_item_detail where " & strValueItemDetail.ToString & ""
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
                        sql = "Delete From m_12_pos_additional_item"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'HAPUS POS BONUS ITEM DETAIL
                        sql = "Delete From m_12_pos_additional_item_detail"
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
                    dtdtl = AsDataTableAmbilDariDBCon("select * from M_12_Ai_Detail where idai = '" & result(4) & "' order by idai asc", myConn)
                    Dim dtbiadditional As New DataTable
                    'AMBIL DATA BI BONUS
                    dtbiadditional = AsDataTableAmbilDariDBCon("select * from M_12_Ai_Additional where idai = '" & result(4) & "' order by idai asc", myConn)

                    Dim strValueInsertAdditionalItem As New StringBuilder 'untuk query simpan di tabel additional utama
                    Dim strValueAdditionalItemDetail As New StringBuilder 'untuk query simpan di tabel additional detail
                    Dim idposadditionalitem As Integer 'untuk variabel id transaksi pos additional item utama
                    Dim dtselectId As New DataTable 'untuk query ambil id transaksi pos additional item
                    Dim dtAdditionalPenampung As New DataTable 'untuk menampung data bi additional
                    Dim dtKatPOS As New DataTable 'untuk menampung data kategori pos, jika jenis kategori 
                    strValueAdditionalItemDetail.Clear()

                    If dtdtl.Rows.Count > 0 Then

                        If drutama("aijeniskategori") = 1 Then 'JIKA PER KATEGORI
                            For Each drdtl As DataRow In dtdtl.Rows
                                'persiapan insert ke tabel m_12_pos_additional_item & tabel m_12_pos_additional_item_detail
                                strValueInsertAdditionalItem.Append(IIf(Len(strValueInsertAdditionalItem.ToString) = 0, "", ", "))
                                strValueInsertAdditionalItem.Append("('" & FixQuotes(drutama("aikategoripos")) & "', '" & FixQuotes(drdtl("idbarang")) & "', '" & FixQuotes(drdtl("operator")) & "', '" & FixDouble(drdtl("jml1")) & "', '" & FixDouble(drdtl("jml2")) & "', '" & FixQuotes(drdtl("customtext1")) & "', '" & FixQuotes(drdtl("customtext2")) & "', '" & FixQuotes(drdtl("customtext3")) & "', '" & FixQuotes(drdtl("customtext4")) & "', '" & FixQuotes(drdtl("customtext5")) & "', " & drdtl("customint1") & ", " & drdtl("customint2") & ", " & drdtl("customint3") & ", '" & FixDouble(drdtl("customdbl1")) & "', '" & FixDouble(drdtl("customdbl2")) & "', '" & FixDouble(drdtl("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl2"))) & "', '" & FixQuotes(drdtl("nopromo")) & "')")
                            Next

                            'insert ke tabel m_12_pos_additional_item
                            sql = "Insert into M_12_Pos_Additional_Item (aikategori, aiidbarang, aioperator, aijml1, aijml2, aicustomtext1, aicustomtext2, aicustomtext3, aicustomtext4, aicustomtext5, aicustomint1, aicustomint2, aicustomint3, aicustomdbl1, aicustomdbl2, aicustomdbl3, aicustomdate1, aicustomdate2, aicustomdate3, aitgl1, aitgl2, ainopromo) values " & strValueInsertAdditionalItem.ToString & ""
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
                                dtselectId = AsDataTableAmbilDariDBCon("select aiid from M_12_Pos_Additional_Item where ainopromo = '" & drdtl2("nopromo") & "' AND aikategori = '" & drdtl2("aikategori") & "' AND aiidbarang = '" & drdtl2("idbarang") & "' AND aioperator = '" & drdtl2("operator") & "' AND aijml1 = '" & drdtl2("jml1") & "' AND aijml2 = '" & drdtl2("jml2") & "' limit 1", myConn)
                                If dtselectId.Rows.Count > 0 Then idposadditionalitem = dtselectId.Rows(0)(0) Else result(2) = "Main POS Additional Item transaction data not found." : Trans.Rollback() : GoTo selesai

                                'filter data additional penampung, untuk dijadikan parameter simpan ke tabel pos additional detail
                                dtAdditionalPenampung = AsDataTableFilterSortDt(dtbiadditional, "idaidetail = '" & drdtl2("idaidetail") & "'")
                                If dtAdditionalPenampung.Rows.Count > 0 Then
                                    For Each drAdditionalPenampung As DataRow In dtAdditionalPenampung.Rows
                                        'parameter simpan ke tabel m_12_pos_additional_item_DETAIL
                                        strValueAdditionalItemDetail.Append(IIf(Len(strValueAdditionalItemDetail.ToString) = 0, "", ", "))
                                        strValueAdditionalItemDetail.Append("(" & idposadditionalitem & ", '" & FixQuotes(drAdditionalPenampung("idbarang")) & "', '" & FixDouble(drAdditionalPenampung("jml")) & "', '" & FixQuotes(drAdditionalPenampung("satuan")) & "', '" & FixQuotes(drAdditionalPenampung("customtext1")) & "', '" & FixQuotes(drAdditionalPenampung("customtext2")) & "', '" & FixQuotes(drAdditionalPenampung("customtext3")) & "', '" & FixQuotes(drAdditionalPenampung("customtext4")) & "', '" & FixQuotes(drAdditionalPenampung("customtext5")) & "', " & drAdditionalPenampung("customint1") & ", " & drAdditionalPenampung("customint2") & ", " & drAdditionalPenampung("customint3") & ", '" & FixDouble(drAdditionalPenampung("customdbl1")) & "', '" & FixDouble(drAdditionalPenampung("customdbl2")) & "', '" & FixDouble(drAdditionalPenampung("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drAdditionalPenampung("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drAdditionalPenampung("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drAdditionalPenampung("customdate3"))) & "')")
                                    Next
                                Else
                                    result(2) = "Main Transaction POS Additional Item data not found." : Trans.Rollback() : GoTo selesai
                                End If
                            Next

                            'INSERT KE TABEL POS BONUS DETAIL
                            sql = "Insert into M_12_Pos_Additional_Item_Detail(idai, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValueAdditionalItemDetail.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                        ElseIf drutama("aijeniskategori") = 2 Then 'JIKA PER CABANG
                            'ambil kategori pos sesuai cabang
                            Dim dtCatPOS As DataTable = AsDataTableAmbilDariDBCon("SELECT GROUP_CONCAT(" & Chr(34) & "'" & Chr(34) & ",l.lkategoripos," & Chr(34) & "'" & Chr(34) & ") as kategoripos FROM m1_location l WHERE l.lkategoripos <> '' AND l.lcabang = '" & FixQuotes(drutama("aicabang")) & "'", myConn)
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
                                                        'persiapan insert ke tabel m_12_pos_additional_item 
                                                        strValueInsertAdditionalItem.Append(IIf(Len(strValueInsertAdditionalItem.ToString) = 0, "", ", "))
                                                        strValueInsertAdditionalItem.Append("('" & FixQuotes(drKatPos("pckode")) & "', '" & FixQuotes(drdtl("idbarang")) & "', '" & FixQuotes(drdtl("operator")) & "', '" & FixDouble(drdtl("jml1")) & "', '" & FixDouble(drdtl("jml2")) & "', '" & FixQuotes(drdtl("customtext1")) & "', '" & FixQuotes(drdtl("customtext2")) & "', '" & FixQuotes(drdtl("customtext3")) & "', '" & FixQuotes(drdtl("customtext4")) & "', '" & FixQuotes(drdtl("customtext5")) & "', " & drdtl("customint1") & ", " & drdtl("customint2") & ", " & drdtl("customint3") & ", '" & FixDouble(drdtl("customdbl1")) & "', '" & FixDouble(drdtl("customdbl2")) & "', '" & FixDouble(drdtl("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl2"))) & "', '" & FixQuotes(drdtl("nopromo")) & "')")

                                                    Next
                                                End If

                                            Next
                                        Next
                                    End If

                                    'insert ke tabel m_12_pos_additional_item
                                    sql = "Insert into M_12_Pos_Additional_Item (aikategori, aiidbarang, aioperator, aijml1, aijml2, aicustomtext1, aicustomtext2, aicustomtext3, aicustomtext4, aicustomtext5, aicustomint1, aicustomint2, aicustomint3, aicustomdbl1, aicustomdbl2, aicustomdbl3, aicustomdate1, aicustomdate2, aicustomdate3, aitgl1, aitgl2, ainopromo) values " & strValueInsertAdditionalItem.ToString & ""
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
                                                        dtselectId = AsDataTableAmbilDariDBCon("select aiid from M_12_Pos_Additional_Item where ainopromo = '" & drdtl2("nopromo") & "' AND aikategori = '" & drKatPos("pckode") & "' AND aiidbarang = '" & drdtl2("idbarang") & "' AND aioperator = '" & drdtl2("operator") & "' AND aijml1 = '" & drdtl2("jml1") & "' AND aijml2 = '" & drdtl2("jml2") & "' limit 1", myConn)
                                                        If dtselectId.Rows.Count > 0 Then idposadditionalitem = dtselectId.Rows(0)(0) Else result(2) = "Main POS Additional Item transaction data not found." : Trans.Rollback() : GoTo selesai
                                                        'filter data additional penampung, untuk dijadikan parameter simpan ke tabel pos additional detail
                                                        dtAdditionalPenampung = AsDataTableFilterSortDt(dtbiadditional, "idaidetail = '" & drdtl2("idaidetail") & "'")
                                                        If dtAdditionalPenampung.Rows.Count > 0 Then
                                                            For Each drAdditionalPenampung As DataRow In dtAdditionalPenampung.Rows
                                                                'persiapan insert ke tabel m_12_pos_additional_item_DETAIL
                                                                strValueAdditionalItemDetail.Append(IIf(Len(strValueAdditionalItemDetail.ToString) = 0, "", ", "))
                                                                strValueAdditionalItemDetail.Append("(" & idposadditionalitem & ", '" & FixQuotes(drAdditionalPenampung("idbarang")) & "', '" & FixDouble(drAdditionalPenampung("jml")) & "', '" & FixQuotes(drAdditionalPenampung("satuan")) & "', '" & FixQuotes(drAdditionalPenampung("customtext1")) & "', '" & FixQuotes(drAdditionalPenampung("customtext2")) & "', '" & FixQuotes(drAdditionalPenampung("customtext3")) & "', '" & FixQuotes(drAdditionalPenampung("customtext4")) & "', '" & FixQuotes(drAdditionalPenampung("customtext5")) & "', " & drAdditionalPenampung("customint1") & ", " & drAdditionalPenampung("customint2") & ", " & drAdditionalPenampung("customint3") & ", '" & FixDouble(drAdditionalPenampung("customdbl1")) & "', '" & FixDouble(drAdditionalPenampung("customdbl2")) & "', '" & FixDouble(drAdditionalPenampung("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drAdditionalPenampung("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drAdditionalPenampung("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drAdditionalPenampung("customdate3"))) & "')")

                                                            Next
                                                        Else
                                                            result(2) = "Main Transaction POS Additional Item data not found." : Trans.Rollback() : GoTo selesai
                                                        End If
                                                    Next
                                                End If
                                            Next
                                        Next

                                        'INSERT KE TABEL POS SUBSTITUTION DETAIL
                                        sql = "Insert into M_12_Pos_Additional_Item_Detail(idai, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValueAdditionalItemDetail.ToString & ""
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
                                                'persiapan insert ke tabel m_12_pos_additional_item 
                                                strValueInsertAdditionalItem.Append(IIf(Len(strValueInsertAdditionalItem.ToString) = 0, "", ", "))
                                                strValueInsertAdditionalItem.Append("('" & FixQuotes(drKatPos("pckode")) & "', '" & FixQuotes(drdtl("idbarang")) & "', '" & FixQuotes(drdtl("operator")) & "', '" & FixDouble(drdtl("jml1")) & "', '" & FixDouble(drdtl("jml2")) & "', '" & FixQuotes(drdtl("customtext1")) & "', '" & FixQuotes(drdtl("customtext2")) & "', '" & FixQuotes(drdtl("customtext3")) & "', '" & FixQuotes(drdtl("customtext4")) & "', '" & FixQuotes(drdtl("customtext5")) & "', " & drdtl("customint1") & ", " & drdtl("customint2") & ", " & drdtl("customint3") & ", '" & FixDouble(drdtl("customdbl1")) & "', '" & FixDouble(drdtl("customdbl2")) & "', '" & FixDouble(drdtl("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl2"))) & "', '" & FixQuotes(drdtl("nopromo")) & "')")

                                            Next
                                        End If

                                    Next
                                Next
                            End If

                            'insert ke tabel m_12_pos_additional_item
                            sql = "Insert into M_12_Pos_Additional_Item (aikategori, aiidbarang, aioperator, aijml1, aijml2, aicustomtext1, aicustomtext2, aicustomtext3, aicustomtext4, aicustomtext5, aicustomint1, aicustomint2, aicustomint3, aicustomdbl1, aicustomdbl2, aicustomdbl3, aicustomdate1, aicustomdate2, aicustomdate3, aitgl1, aitgl2, ainopromo) values " & strValueInsertAdditionalItem.ToString & ""
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
                                                dtselectId = AsDataTableAmbilDariDBCon("select aiid from M_12_Pos_Additional_Item where ainopromo = '" & drdtl2("nopromo") & "' AND aikategori = '" & drKatPos("pckode") & "' AND aiidbarang = '" & drdtl2("idbarang") & "' AND aioperator = '" & drdtl2("operator") & "' AND aijml1 = '" & drdtl2("jml1") & "' AND aijml2 = '" & drdtl2("jml2") & "' limit 1", myConn)
                                                If dtselectId.Rows.Count > 0 Then idposadditionalitem = dtselectId.Rows(0)(0) Else result(2) = "Main POS Additional Item transaction data not found." : Trans.Rollback() : GoTo selesai
                                                'filter data additional penampung, untuk dijadikan parameter simpan ke tabel pos additional detail
                                                dtAdditionalPenampung = AsDataTableFilterSortDt(dtbiadditional, "idaidetail = '" & drdtl2("idaidetail") & "'")
                                                If dtAdditionalPenampung.Rows.Count > 0 Then
                                                    For Each drAdditionalPenampung As DataRow In dtAdditionalPenampung.Rows
                                                        'persiapan insert ke tabel m_12_pos_additional_item_DETAIL
                                                        strValueAdditionalItemDetail.Append(IIf(Len(strValueAdditionalItemDetail.ToString) = 0, "", ", "))
                                                        strValueAdditionalItemDetail.Append("(" & idposadditionalitem & ", '" & FixQuotes(drAdditionalPenampung("idbarang")) & "', '" & FixDouble(drAdditionalPenampung("jml")) & "', '" & FixQuotes(drAdditionalPenampung("satuan")) & "', '" & FixQuotes(drAdditionalPenampung("customtext1")) & "', '" & FixQuotes(drAdditionalPenampung("customtext2")) & "', '" & FixQuotes(drAdditionalPenampung("customtext3")) & "', '" & FixQuotes(drAdditionalPenampung("customtext4")) & "', '" & FixQuotes(drAdditionalPenampung("customtext5")) & "', " & drAdditionalPenampung("customint1") & ", " & drAdditionalPenampung("customint2") & ", " & drAdditionalPenampung("customint3") & ", '" & FixDouble(drAdditionalPenampung("customdbl1")) & "', '" & FixDouble(drAdditionalPenampung("customdbl2")) & "', '" & FixDouble(drAdditionalPenampung("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drAdditionalPenampung("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drAdditionalPenampung("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drAdditionalPenampung("customdate3"))) & "')")

                                                    Next
                                                Else
                                                    result(2) = "Main Transaction POS Additional Item data not found." : Trans.Rollback() : GoTo selesai
                                                End If
                                            Next
                                        End If
                                    Next
                                Next

                                'INSERT KE TABEL POS SUBSTITUTION DETAIL
                                sql = "Insert into M_12_Pos_Additional_Item_Detail(idai, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValueAdditionalItemDetail.ToString & ""
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
    Public Function M12_AiUpdateStatus(ByVal param As String) As String
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
            Filter = Filter.Replace("aikontakkode", "c.kkode")
            Filter = Filter.Replace("aikontaknama", "c.knama")
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
            Dim sumber As String = "AI", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Aitgl, Ainotransaksi, Aistatus FROM m_12_Ai WHERE Aiid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Aistatussebelumnya" : jnsaktivitas = 17
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
                dtutama = AsDataTableAmbilDariDBCon("SELECT * FROM M_12_ai WHERE aiid=" & idtransaksi, myConn)
                If (dtutama.Rows.Count > 0) Then
                    For Each drutama As DataRow In dtutama.Rows
                        'AMBIL DATA DETAIL
                        dtdetail = AsDataTableAmbilDariDBCon("SELECT * FROM M_12_Ai_Detail WHERE idai=" & idtransaksi, myConn)
                        If (dtdetail.Rows.Count > 0) Then
                            For Each drdetail As DataRow In dtdetail.Rows
                                Dim dtadditional As New DataTable
                                If drutama("aijeniskategori") = 1 Then 'JIKA PER KATEGORI
                                    Dim query As String = "SELECT aiid FROM m_12_pos_additional_item WHERE aikategori='" & drdetail("aikategori") & "'"
                                    dtadditional = AsDataTableAmbilDariDBCon(query, myConn)
                                    If dtadditional.Rows.Count > 0 Then
                                        For Each dradditional As DataRow In dtadditional.Rows
                                            'hapus data detail
                                            Dim strValue2 As New StringBuilder
                                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", "Or "))
                                            strValue2.Append("aiid = '" & FixQuotes(dradditional("aiid")) & "' ")
                                            sql = "Delete from M_12_pos_additional_item WHERE " & strValue2.ToString
                                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                            With objCmd
                                                .Connection = myConn
                                                .Transaction = Trans
                                                .CommandType = CommandType.Text
                                                .CommandText = sql
                                            End With
                                            objCmd.ExecuteNonQuery()

                                            Dim strValueItemDetail As New StringBuilder
                                            strValueItemDetail.Append(IIf(Len(strValueItemDetail.ToString) = 0, "", "Or "))
                                            strValueItemDetail.Append("idai = '" & FixQuotes(dradditional("aiid")) & "'")

                                            'hapus data detail
                                            sql = "Delete from M_12_pos_additional_item_Detail WHERE " & strValueItemDetail.ToString
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
                                    Dim query As String = "SELECT aiid FROM m_12_pos_additional_item WHERE sinopromo = '" & drdetail("nopromo") & "'"
                                    dtadditional = AsDataTableAmbilDariDBCon(query, myConn)
                                    If dtadditional.Rows.Count > 0 Then
                                        For Each drsubstitution As DataRow In dtadditional.Rows
                                            'hapus data detail
                                            Dim strValue2 As New StringBuilder
                                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", " OR "))
                                            strValue2.Append("aiid = '" & FixQuotes(drsubstitution("aiid")) & "' ")
                                            sql = "Delete from m_12_pos_additional_item WHERE " & strValue2.ToString
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
                                            strValueItemDetail.Append("idai = '" & FixQuotes(drsubstitution("aiid")) & "'")

                                            'hapus data detail
                                            sql = "Delete from m_12_pos_additional_item_Detail WHERE " & strValueItemDetail.ToString
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
            sql = "UPDATE M_12_Ai SET Aistatus = " & nilaiStatus & ", aimodifikasiuser='" & userid & "', aimodifikasitgl = NOW(), aiposting = 0, aipostingtgl = '1971-01-01 00:00:00', Aijmlrevisi = Aijmlrevisi + 1 WHERE aiid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_AiSearch(PostWsSearch(paramSplit(0), "M12_AiSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_AiDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("aikontakkode", "c.kkode")
            Filter = Filter.Replace("aikontaknama", "c.knama")
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
            Dim sumber As String = "AI", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT aiid, ainotransaksi FROM m_12_ai WHERE aiid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT aicabang, ailokasi, aisumber, aiautonotransaksi, ainotransaksi, aitgl"
            sql &= " FROM M_12_ai"
            sql &= " WHERE aiid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("aicabang")
                lokasi = dtNomorNext.Rows(0)("ailokasi")
                sumber = dtNomorNext.Rows(0)("aisumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("aiautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("ainotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("aitgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M_12_Ai_Detail WHERE idai = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M_12_Ai WHERE aiid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_AiSearch(PostWsSearch(paramSplit(0), "M12_AiSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_AiSimpanOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataAdditional(), dataRowAdditional() As String

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
        'aiid(0) As Integer, aicabang(1) As String, ailokasi(2) As String, aisumber(3) As String, aikategoripos(4) As String, 
        'aiautonotransaksi(5) As Integer, ainotransaksi(6) As String, aitgl(7) As Date, aikodepa(8) As , aikontak(9) As , 
        'aikontakperson(10) As String, aiuraian(11) As String, aicatatan(12) As String, aistatus(13) As Integer, aistatussebelumnya(14) As Integer, 
        'aijmlrevisi(15) As Integer, aicetakanke(16) As Integer, aiisclose(17) As Integer, aiinputuser(18) As , aiinputtgl(19) As DateTime, 
        'aimodifikasiuser(20) As , aimodifikasitgl(21) As DateTime, aiposting(22) As Integer, aipostingtgl(23) As DateTime, aicustomtext1(24) As String, 
        'aicustomtext2(25) As String, aicustomtext3(26) As String, aicustomtext4(27) As String, aicustomtext5(28) As String, aicustomint1(29) As Integer, 
        'aicustomint2(30) As Integer, aicustomint3(31) As Integer, aicustomdbl1(32) As Double, aicustomdbl2(33) As Double, aicustomdbl3(34) As Double, 
        'aicustomdate1(35) As Date, aicustomdate2(36) As Date, aicustomdate3(37) As Date, aijeniskategori(38) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'aiid, aicabang, ailokasi, aisumber, aikategoripos, aiautonotransaksi, ainotransaksi, 
        'aitgl, aikodepa, aikontak, aikontakperson, aiuraian, aicatatan, aistatus, 
        'aistatussebelumnya, aijmlrevisi, aicetakanke, aiisclose, aiinputuser, aiinputtgl, aimodifikasiuser, 
        'aimodifikasitgl, aiposting, aipostingtgl, aicustomtext1, aicustomtext2, aicustomtext3, aicustomtext4, 
        'aicustomtext5, aicustomint1, aicustomint2, aicustomint3, aicustomdbl1, aicustomdbl2, aicustomdbl3, 
        'aicustomdate1, aicustomdate2, aicustomdate3, aijeniskategori

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 39) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'aiid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "aiid required numeric." : GoTo selesai
        End If
        'aiautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "aiautonotransaksi required numeric." : GoTo selesai
        End If
        'aitgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "aitgl required date." : GoTo selesai
        End If
        'aistatus(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "aistatus required numeric." : GoTo selesai
        End If
        'aistatussebelumnya(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "aistatussebelumnya required numeric." : GoTo selesai
        End If
        'aijmlrevisi(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "aijmlrevisi required numeric." : GoTo selesai
        End If
        'aicetakanke(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "aicetakanke required numeric." : GoTo selesai
        End If
        'aiisclose(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "aiisclose required numeric." : GoTo selesai
        End If
        'aiinputtgl(19) As DateTime
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "aiinputtgl required date." : GoTo selesai
        End If
        'aimodifikasitgl(21) As DateTime
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "aimodifikasitgl required date." : GoTo selesai
        End If
        'aiposting(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "aiposting required numeric." : GoTo selesai
        End If
        'aipostingtgl(23) As DateTime
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "aipostingtgl required date." : GoTo selesai
        End If
        'aicustomint1(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "aicustomint1 required numeric." : GoTo selesai
        End If
        'aicustomint2(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "aicustomint2 required numeric." : GoTo selesai
        End If
        'aicustomint3(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "aicustomint3 required numeric." : GoTo selesai
        End If
        'aicustomdbl1(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "aicustomdbl1 required numeric." : GoTo selesai
        End If
        'aicustomdbl2(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "aicustomdbl2 required numeric." : GoTo selesai
        End If
        'aicustomdbl3(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "aicustomdbl3 required numeric." : GoTo selesai
        End If
        'aicustomdate1(35) As Date
        If (IsDate(dataUtama(35)) = False) Then
            result(2) = "aicustomdate1 required date." : GoTo selesai
        End If
        'aicustomdate2(36) As Date
        If (IsDate(dataUtama(36)) = False) Then
            result(2) = "aicustomdate2 required date." : GoTo selesai
        End If
        'aicustomdate3(37) As Date
        If (IsDate(dataUtama(37)) = False) Then
            result(2) = "aicustomdate3 required date." : GoTo selesai
        End If

        'aijeniskategori(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "aijeniskategori required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'aicabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "aicabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "aicabang should not be more than 25 character." : GoTo selesai
        End If

        'ailokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "ailokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "ailokasi should not be more than 25 character." : GoTo selesai
        End If

        'aisumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "aisumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "aisumber should not be more than 10 character." : GoTo selesai
        End If

        'aikategoripos(4) As String
        'If Len(dataUtama(4)) = 0 Then
        '    result(2) = "aikategoripos can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(4)) > 50 Then
            result(2) = "aikategoripos should not be more than 50 character." : GoTo selesai
        End If

        'ainotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "ainotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "ainotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'aitgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "aitgl can't be empty" : GoTo selesai
        End If

        'aikodepa(8) As 
        If Len(dataUtama(8)) = 0 Then
            result(2) = "aikodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 20 Then
            result(2) = "aikodepa should not be more than 20 character." : GoTo selesai
        End If

        'aikontak(9) As 
        If Len(dataUtama(9)) = 0 Then
            result(2) = "aikontak can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 20 Then
            result(2) = "aikontak should not be more than 20 character." : GoTo selesai
        End If

        'aiinputtgl(19) As DateTime
        If Len(dataUtama(19)) = 0 Then
            result(2) = "aiinputtgl can't be empty" : GoTo selesai
        End If

        'aimodifikasitgl(21) As DateTime
        If Len(dataUtama(21)) = 0 Then
            result(2) = "aimodifikasitgl can't be empty" : GoTo selesai
        End If

        'aipostingtgl(23) As DateTime
        If Len(dataUtama(23)) = 0 Then
            result(2) = "aipostingtgl can't be empty" : GoTo selesai
        End If

        'aicustomdbl1(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "aicustomdbl1 can't be empty" : GoTo selesai
        End If

        'aicustomdbl2(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "aicustomdbl2 can't be empty" : GoTo selesai
        End If

        'aicustomdbl3(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "aicustomdbl3 can't be empty" : GoTo selesai
        End If

        'aicustomdate1(35) As Date
        If Len(dataUtama(35)) = 0 Then
            result(2) = "aicustomdate1 can't be empty" : GoTo selesai
        End If

        'aicustomdate2(36) As Date
        If Len(dataUtama(36)) = 0 Then
            result(2) = "aicustomdate2 can't be empty" : GoTo selesai
        End If

        'aicustomdate3(37) As Date
        If Len(dataUtama(37)) = 0 Then
            result(2) = "aicustomdate3 can't be empty" : GoTo selesai
        End If



        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "aiid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ailokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aisumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aikategoripos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aiautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ainotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aikodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aikontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aikontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aiuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aistatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aistatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aijmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aicetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aiisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aiinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aiinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aimodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aimodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aiposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aipostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aijeniskategori", AsEnumTypeData.AsInt64)

        If AsDataTableTambahData(dtutama, "aiid~aicabang~ailokasi~aisumber~aikategoripos~aiautonotransaksi~ainotransaksi~aitgl~aikodepa~aikontak~aikontakperson~aiuraian~aicatatan~aistatus~aistatussebelumnya~aijmlrevisi~aicetakanke~aiisclose~aiinputuser~aiinputtgl~aimodifikasiuser~aimodifikasitgl~aiposting~aipostingtgl~aicustomtext1~aicustomtext2~aicustomtext3~aicustomtext4~aicustomtext5~aicustomint1~aicustomint2~aicustomint3~aicustomdbl1~aicustomdbl2~aicustomdbl3~aicustomdate1~aicustomdate2~aicustomdate3~aijeniskategori", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38)) = False Then
            result(2) = "insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idaidetail(0) As , idai(1) As , aikategori(2) As String, idbarang(3) As , operator(4) As String, 
        'jml1(5) As Double, jml2(6) As Double, customtext1(7) As String, customtext2(8) As String, customtext3(9) As String, 
        'customtext4(10) As String, customtext5(11) As String, customint1(12) As Integer, customint2(13) As Integer, customint3(14) As Integer, 
        'customdbl1(15) As Double, customdbl2(16) As Double, customdbl3(17) As Double, customdate1(18) As Date, customdate2(19) As Date, 
        'customdate3(20) As Date, tgl1(21) As Date, tgl2(22) As Date, nopromo(23) As String, nogrup (24) As String, catatan (25) As String, urutan(26) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idaidetail, idai, aikategori, idbarang, operator, jml1, jml2, 
        'customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, 
        'customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'tgl1, tgl2, nopromo, nogrup, catatan, urutan

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idaidetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idai", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "aikategori", AsEnumTypeData.AsString)
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
                result(2) = "Row : " & i & " - idaidetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idaidetail should not be more than 20 character." : GoTo selesai
            End If

            'idbi(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idai can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idai should not be more than 20 character." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idaidetail~idai~aikategori~idbarang~operator~jml1~jml2~customtext1~customtext2~customtext3~customtext4~customtext5~customint1~customint2~customint3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~tgl1~tgl2~nopromo~nogrup~catatan~urutan", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26)) = False Then
                result(2) = "Detail Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================

        'MAPPING BUAT WS DATA BONUS -------------------------------------------------------
        'idadditional(0) As , idai(1) As , idaidetail(2) As , idbarang(3) As , jml(4) As Double, 
        'satuan(5) As String, customtext1(6) As String, customtext2(7) As String, customtext3(8) As String, customtext4(9) As String, 
        'customtext5(10) As String, customint1(11) As Integer, customint2(12) As Integer, customint3(13) As Integer, customdbl1(14) As Double, 
        'customdbl2(15) As Double, customdbl3(16) As Double, customdate1(17) As Date, customdate2(18) As Date, customdate3(19) As Date, nogrup(20) As String

        'MAPPING BUAT FLEX DATA BONUS -----------------------------------------------------
        'idadditional, idai, idaidetail, idbarang, jml, satuan, customtext1, 
        'customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA BONUS ======================================================
        'SPLIT PARAMETER DATA BONUS
        dataAdditional = dataSplit(2).Split(sptRow)
        'END OF VALIDASI DAN SET DATA BONUS ===============================================

        'Buat datatable additional
        Dim dtadditional As New DataTable
        AsDataTableTambahField(dtadditional, "idadditional", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtadditional, "idai", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtadditional, "idaidetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtadditional, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtadditional, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "customint1", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtadditional, "customint2", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtadditional, "customint3", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtadditional, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "nogrup", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtadditional, "urutan", AsEnumTypeData.AsInt64)

        'VALIDASI DAN SET DATA ROW BONUS ==================================================
        Dim JmlDtAdditional As Integer = dataAdditional.Length
        For i = 1 To JmlDtAdditional
            'SPLIT DATA DETAIL
            dataRowAdditional = dataAdditional(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA BONUS -----------------------------------
            'CEK ARRAY DATA BONUS
            If (dataRowAdditional.Length <> 22) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW BONUS ----------------------------

            'VALIDASI TIPE DATA BONUS ------------------------------------------
            'jml(4) As Double
            If (IsNumeric(dataRowAdditional(4)) = False) Then
                result(2) = "jml required numeric." : GoTo selesai
            End If
            'customint1(11) As Integer
            If (IsNumeric(dataRowAdditional(11)) = False) Then
                result(2) = "customint1 required numeric." : GoTo selesai
            End If
            'customint2(12) As Integer
            If (IsNumeric(dataRowAdditional(12)) = False) Then
                result(2) = "customint2 required numeric." : GoTo selesai
            End If
            'customint3(13) As Integer
            If (IsNumeric(dataRowAdditional(13)) = False) Then
                result(2) = "customint3 required numeric." : GoTo selesai
            End If
            'customdbl1(14) As Double
            If (IsNumeric(dataRowAdditional(14)) = False) Then
                result(2) = "customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(15) As Double
            If (IsNumeric(dataRowAdditional(15)) = False) Then
                result(2) = "customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(16) As Double
            If (IsNumeric(dataRowAdditional(16)) = False) Then
                result(2) = "customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(17) As Date
            If (IsDate(dataRowAdditional(17)) = False) Then
                result(2) = "customdate1 required date." : GoTo selesai
            End If
            'customdate2(18) As Date
            If (IsDate(dataRowAdditional(18)) = False) Then
                result(2) = "customdate2 required date." : GoTo selesai
            End If
            'customdate3(19) As Date
            If (IsDate(dataRowAdditional(19)) = False) Then
                result(2) = "customdate3 required date." : GoTo selesai
            End If
            'urutan(21) As Double
            If (IsNumeric(dataRowAdditional(21)) = False) Then
                result(2) = "urutan required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA BONUS -----------------------------------

            'VALIDASI DATA BONUS ---------------------------------------
            'idadditional(0) As 
            If Len(dataRowAdditional(0)) = 0 Then
                result(2) = "Row : " & i & " - idadditional can't be empty" : GoTo selesai
            End If
            If Len(dataRowAdditional(0)) > 20 Then
                result(2) = "Row : " & i & " - idadditional should not be more than 20 character." : GoTo selesai
            End If

            'idai(1) As 
            If Len(dataRowAdditional(1)) = 0 Then
                result(2) = "Row : " & i & " - idai can't be empty" : GoTo selesai
            End If
            If Len(dataRowAdditional(1)) > 20 Then
                result(2) = "Row : " & i & " - idai should not be more than 20 character." : GoTo selesai
            End If

            'idaidetail(2) As 
            If Len(dataRowAdditional(2)) = 0 Then
                result(2) = "Row : " & i & " - idaidetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowAdditional(2)) > 20 Then
                result(2) = "Row : " & i & " - idaidetail should not be more than 20 character." : GoTo selesai
            End If

            'idbarang(3) As 
            If Len(dataRowAdditional(3)) = 0 Then
                result(2) = "Row : " & i & " - idbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowAdditional(3)) > 20 Then
                result(2) = "Row : " & i & " - idbarang should not be more than 20 character." : GoTo selesai
            End If

            'jml(4) As Double
            If Len(dataRowAdditional(4)) = 0 Then
                result(2) = "Row : " & i & " - jml can't be empty" : GoTo selesai
            End If

            'satuan(5) As String
            If Len(dataRowAdditional(5)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowAdditional(5)) > 25 Then
                result(2) = "Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'customdbl1(14) As Double
            If Len(dataRowAdditional(14)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(15) As Double
            If Len(dataRowAdditional(15)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(16) As Double
            If Len(dataRowAdditional(16)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(17) As Date
            If Len(dataRowAdditional(17)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(18) As Date
            If Len(dataRowAdditional(18)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(19) As Date
            If Len(dataRowAdditional(19)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'urutan(21) As Date
            If Len(dataRowAdditional(21)) = 0 Then
                result(2) = "Row : " & i & " - urutan can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtadditional, "idadditional~idai~idaidetail~idbarang~jml~satuan~customtext1~customtext2~customtext3~customtext4~customtext5~customint1~customint2~customint3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~nogrup~urutan", dataRowAdditional(0) & "~" & dataRowAdditional(1) & "~" & dataRowAdditional(2) & "~" & dataRowAdditional(3) & "~" & dataRowAdditional(4) & "~" & dataRowAdditional(5) & "~" & dataRowAdditional(6) & "~" & dataRowAdditional(7) & "~" & dataRowAdditional(8) & "~" & dataRowAdditional(9) & "~" & dataRowAdditional(10) & "~" & dataRowAdditional(11) & "~" & dataRowAdditional(12) & "~" & dataRowAdditional(13) & "~" & dataRowAdditional(14) & "~" & dataRowAdditional(15) & "~" & dataRowAdditional(16) & "~" & dataRowAdditional(17) & "~" & dataRowAdditional(18) & "~" & dataRowAdditional(19) & "~" & dataRowAdditional(20) & "~" & dataRowAdditional(21)) = False Then
                result(2) = "Additional Row : " & i & " - insert into datatable failed." : GoTo selesai
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
                    result(4) = drutama("aiid")
                    notransaksi = drutama("ainotransaksi")

                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(aiid), ainotransaksi FROM M_12_Ai WHERE aiid=" & result(4))
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(aiid) FROM M_12_Ai WHERE ainotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        ''SIMPAN HISTORY ========================
                        'Dim SimpanHistory As New m12_ai_history
                        'Dim rsSimpanHistory As String = SimpanHistory.M12_Bi_HistorySimpan("" & paramSplit(0) & "★M12_Bi_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("bisumber")) & "▼" & FixQuotes(drutama("biid")) & "")
                        'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        'If (rsSplitResult(1) = 0) Then
                        '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        'End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M_12_Ai set aicabang  = '" & FixQuotes(drutama("aicabang")) & "', ailokasi  = '" & FixQuotes(drutama("ailokasi")) & "', aisumber  = '" & FixQuotes(drutama("aisumber")) & "', aikategoripos  = '" & FixQuotes(drutama("aikategoripos")) & "', aiautonotransaksi  = " & drutama("aiautonotransaksi") & ", ainotransaksi  = '" & FixQuotes(drutama("ainotransaksi")) & "', aitgl  = '" & FixQuotes(AsFormatTanggal(drutama("aitgl"))) & "', aikodepa  = '" & FixQuotes(drutama("aikodepa")) & "', aikontak  = '" & FixQuotes(drutama("aikontak")) & "', aikontakperson  = '" & FixQuotes(drutama("aikontakperson")) & "', aiuraian  = '" & FixQuotes(drutama("aiuraian")) & "', aicatatan  = '" & FixQuotes(drutama("aicatatan")) & "', aistatus  = " & drutama("aistatus") & ", aistatussebelumnya  = " & drutama("aistatussebelumnya") & ", aijmlrevisi  = " & drutama("aijmlrevisi") & ", aicetakanke  = " & drutama("aicetakanke") & ", aiisclose  = " & drutama("aiisclose") & ", aiinputuser  = '" & FixQuotes(drutama("aiinputuser")) & "', aimodifikasiuser  = '" & FixQuotes(drutama("aimodifikasiuser")) & "', aimodifikasitgl  = NOW(), aiposting  = " & drutama("aiposting") & ", aipostingtgl  = '" & FixQuotes(AsFormatTanggal(drutama("aipostingtgl"), "yyyy-MM-dd H:mm:ss")) & "', aicustomtext1  = '" & FixQuotes(drutama("aicustomtext1")) & "', aicustomtext2  = '" & FixQuotes(drutama("aicustomtext2")) & "', aicustomtext3  = '" & FixQuotes(drutama("aicustomtext3")) & "', aicustomtext4  = '" & FixQuotes(drutama("aicustomtext4")) & "', aicustomtext5  = '" & FixQuotes(drutama("aicustomtext5")) & "', aicustomint1  = " & drutama("aicustomint1") & ", aicustomint2  = " & drutama("aicustomint2") & ", aicustomint3  = " & drutama("aicustomint3") & ", aicustomdbl1  = '" & FixDouble(drutama("aicustomdbl1")) & "', aicustomdbl2  = '" & FixDouble(drutama("aicustomdbl2")) & "', aicustomdbl3  = '" & FixDouble(drutama("aicustomdbl3")) & "', aicustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("aicustomdate1"))) & "', aicustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("aicustomdate2"))) & "', aicustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("aicustomdate3"))) & "', aijeniskategori  = '" & FixQuotes(drutama("aijeniskategori")) & "' where aiid = " & drutama("aiid") & ""
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

                    If drutama("aiautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("aicabang"), drutama("ailokasi"), drutama("aisumber"), drutama("aitgl"))
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
                        notransaksi = drutama("ainotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(aiid) FROM m_12_ai WHERE ainotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M_12_Ai (aicabang, ailokasi, aisumber, aikategoripos, aiautonotransaksi, ainotransaksi, aitgl, aikodepa, aikontak, aikontakperson, aiuraian, aicatatan, aistatus, aistatussebelumnya, aijmlrevisi, aicetakanke, aiisclose, aiinputuser, aiinputtgl, aimodifikasiuser, aimodifikasitgl, aiposting, aipostingtgl, aicustomtext1, aicustomtext2, aicustomtext3, aicustomtext4, aicustomtext5, aicustomint1, aicustomint2, aicustomint3, aicustomdbl1, aicustomdbl2, aicustomdbl3, aicustomdate1, aicustomdate2, aicustomdate3, aijeniskategori) values('" & FixQuotes(drutama("aicabang")) & "', '" & FixQuotes(drutama("ailokasi")) & "', '" & FixQuotes(drutama("aisumber")) & "', '" & FixQuotes(drutama("aikategoripos")) & "', " & drutama("aiautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("aitgl"))) & "', '" & FixQuotes(drutama("aikodepa")) & "', '" & FixQuotes(drutama("aikontak")) & "', '" & FixQuotes(drutama("aikontakperson")) & "', '" & FixQuotes(drutama("aiuraian")) & "', '" & FixQuotes(drutama("aicatatan")) & "', " & drutama("aistatus") & ", " & drutama("aistatussebelumnya") & ", " & drutama("aijmlrevisi") & ", " & drutama("aicetakanke") & ", " & drutama("aiisclose") & ", '" & FixQuotes(drutama("aiinputuser")) & "', NOW(), '" & FixQuotes(drutama("aimodifikasiuser")) & "', '1971-01-01 00:00:00', 0, '1971-01-01 00:00:00', '" & FixQuotes(drutama("aicustomtext1")) & "', '" & FixQuotes(drutama("aicustomtext2")) & "', '" & FixQuotes(drutama("aicustomtext3")) & "', '" & FixQuotes(drutama("aicustomtext4")) & "', '" & FixQuotes(drutama("aicustomtext5")) & "', " & drutama("aicustomint1") & ", " & drutama("aicustomint2") & ", " & drutama("aicustomint3") & ", '" & FixDouble(drutama("aicustomdbl1")) & "', '" & FixDouble(drutama("aicustomdbl2")) & "', '" & FixDouble(drutama("aicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("aicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("aicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("aicustomdate3"))) & "', " & drutama("aijeniskategori") & ")"
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
                    dt2 = AsDataTableAmbilDariDB("select aiid from M_12_ai where ainotransaksi='" & notransaksi & "' AND aiinputuser= '" & drutama("aiinputuser") & "' order by aimodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M_12_Ai_Detail where idai = " & result(4)
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Hapus additional ketika update
                If (isUpdate) Then
                    sql = "Delete from M_12_Ai_Additional where idai = " & result(4)
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
                    Dim dtAdditionalGrup As New DataTable

                    For Each dr1 As DataRow In dtdetail.Rows

                        'CEK OPERATOR :
                        'JIKA BETWEEN (0) ATAU >= (1) MAKA BOLEH LEBIH DARI SATU KONDISI 
                        '=> BEBERAPA KONDISI BETWEEN DAN SATU KONDISI >= (1)
                        'JIKA KELIPATAN (2) MAKA HANYA BOLEH SATU KONDISI
                        Dim dtOperator As New DataTable
                        sql = "SELECT aid.aikategori as kategori, aid.idbarang as idbarang, aid.operator as operator, i.bkode, (CASE aid.operator WHEN 0 THEN 'Between' WHEN 1 THEN '>=' WHEN 2 THEN 'Multiple' ELSE 'Unknown' END) as operatornama FROM m_12_ai_detail aid JOIN m1_item i ON aid.idbarang = i.bid WHERE aid.aikategori = '" & FxDB(drutama("aikategoripos"), "") & "' AND aid.idbarang = '" & FxDB(dr1("idbarang"), "") & "' AND aid.idai = '" & result(4) & "' AND aid.idaidetail <> '" & FxDB(dr1("idaidetail"), "") & "' GROUP BY aid.operator ORDER BY aid.operator"
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
                        strValue2.Append("('" & FixQuotes(dr1("idaidetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("aikategori")) & "', '" & FixQuotes(dr1("idbarang")) & "', '" & FixQuotes(dr1("operator")) & "', '" & FixDouble(dr1("jml1")) & "', '" & FixDouble(dr1("jml2")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl2"))) & "', '" & FixQuotes(dr1("nopromo")) & "')")

                        'sql = "Insert into M_12_Bi_Detail(idbidetail, idbi, bikategori, idbarang, operator, jml1, jml2, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, tgl1, tgl2, nopromo) values" & strValue2.ToString & ""
                        sql = "Insert into M_12_Ai_Detail(idaidetail, idai, aikategori, idbarang, operator, jml1, jml2, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, tgl1, tgl2, nopromo, catatan, urutan) values('" & FixQuotes(dr1("idaidetail")) & "', " & result(4) & ", '" & FixQuotes(drutama("aikategoripos")) & "', '" & FixQuotes(dr1("idbarang")) & "', '" & FixQuotes(dr1("operator")) & "', '" & FixDouble(dr1("jml1")) & "', '" & FixDouble(dr1("jml2")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl2"))) & "', '" & notransaksi & "', '" & FixQuotes(dr1("catatan")) & "','" & FixQuotes(dr1("urutan")) & "')"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()


                        'ambil ID detail untuk diinsert ke additional
                        Dim iddetail As Integer
                        Dim dtidadditional As New DataTable
                        'Sql disesuaikan sendiri, untuk parameternya disesuaikan sendiri.
                        dtidadditional = AsDataTableAmbilDariDB("select idaidetail from M_12_ai_detail where idai='" & result(4) & "' and aikategori = '" & drutama("aikategoripos") & "' AND  idbarang = '" & dr1("idbarang") & "' AND  operator = '" & dr1("operator") & "' AND  jml1 = '" & dr1("jml1") & "' AND jml2 = '" & dr1("jml2") & "' order by idaidetail desc limit 1")
                        If dtidadditional.Rows.Count > 0 Then iddetail = dtidadditional.Rows(0)(0) Else result(2) = "#1 Detail transaction data not found." : Trans.Rollback() : GoTo selesai

                        'Proses Additional
                        If (dtadditional.Rows.Count > 0) Then
                            'AMBIL DETAIL BONUS SESUAI NO GRUP
                            dtAdditionalGrup = AsDataTableFilterSortDt(dtadditional, "nogrup = '" & dr1("nogrup") & "'")
                            If (dtAdditionalGrup.Rows.Count > 0) Then
                                strValue2.Clear()
                                For Each drAdditionalGrup As DataRow In dtAdditionalGrup.Rows
                                    strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                                    strValue2.Append("('" & FixQuotes(drAdditionalGrup("idadditional")) & "', " & result(4) & ", '" & iddetail & "', '" & FixQuotes(drAdditionalGrup("idbarang")) & "', '" & FixDouble(drAdditionalGrup("jml")) & "', '" & FixQuotes(drAdditionalGrup("satuan")) & "', '" & FixQuotes(drAdditionalGrup("customtext1")) & "', '" & FixQuotes(drAdditionalGrup("customtext2")) & "', '" & FixQuotes(drAdditionalGrup("customtext3")) & "', '" & FixQuotes(drAdditionalGrup("customtext4")) & "', '" & FixQuotes(drAdditionalGrup("customtext5")) & "', " & drAdditionalGrup("customint1") & ", " & drAdditionalGrup("customint2") & ", " & drAdditionalGrup("customint3") & ", '" & FixDouble(drAdditionalGrup("customdbl1")) & "', '" & FixDouble(drAdditionalGrup("customdbl2")) & "', '" & FixDouble(drAdditionalGrup("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drAdditionalGrup("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drAdditionalGrup("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drAdditionalGrup("customdate3"))) & "', '" & FixQuotes(drAdditionalGrup("urutan")) & "')")
                                Next

                                sql = "Insert into M_12_Ai_Additional(idadditional, idai, idaidetail,  idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, urutan) values" & strValue2.ToString & ""
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = Con1
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                            Else
                                result(2) = "Additional Transaction for No. Group : " & dr1("nogrup") & " does not found." : Trans.Rollback() : GoTo selesai
                            End If

                        Else
                            result(2) = "Additional Transaction data not found." : Trans.Rollback() : GoTo selesai
                        End If
                    Next

                Else
                    result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Update ke tabel Barang Additional
                If drutama("aistatus") = 2 Then
                    'JIKA PER KATEGORI, HAPUS DATA PER KATEGORI
                    If drutama("aijeniskategori") = 1 Then
                        'Cek apakah kategori pos sudah ada di tabel pos_additional_item, jika sudah ada, hapus data di tabel itu
                        Dim dtPOSAdditionalItem As New DataTable
                        dtPOSAdditionalItem = AsDataTableAmbilDariDB("select aiid from M_12_Pos_Additional_Item where aikategori = '" & drutama("aikategoripos") & "'")
                        Dim strValueItemUtama As New StringBuilder
                        Dim strValueItemDetail As New StringBuilder
                        If dtPOSAdditionalItem.Rows.Count > 1 Then
                            For Each drPOSAdditionalItem As DataRow In dtPOSAdditionalItem.Rows
                                'QUERY HAPUS POS BONUS ITEM UTAMA
                                strValueItemUtama.Append(IIf(Len(strValueItemUtama.ToString) = 0, "", " OR "))
                                strValueItemUtama.Append("aiid = '" & FixQuotes(drPOSAdditionalItem("aiid")) & "'")

                                'QUERY HAPUS POS BONUS ITEM DETAIL
                                strValueItemDetail.Append(IIf(Len(strValueItemDetail.ToString) = 0, "", " OR "))
                                strValueItemDetail.Append("idai = '" & FixQuotes(drPOSAdditionalItem("aiid")) & "'")
                            Next

                            'HAPUS POS BONUS ITEM UTAMA
                            sql = "Delete From m_12_pos_additional_item where " & strValueItemUtama.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                            'HAPUS POS BONUS ITEM DETAIL
                            sql = "Delete From m_12_pos_additional_item_detail where " & strValueItemDetail.ToString & ""
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
                        sql = "Delete From m_12_pos_additional_item"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'HAPUS POS BONUS ITEM DETAIL
                        sql = "Delete From m_12_pos_additional_item_detail"
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
                    dtdtl = AsDataTableAmbilDariDB("select * from M_12_Ai_Detail where idai = '" & result(4) & "' order by idai asc")
                    Dim dtbiadditional As New DataTable
                    'AMBIL DATA BI BONUS
                    dtbiadditional = AsDataTableAmbilDariDB("select * from M_12_Ai_Additional where idai = '" & result(4) & "' order by idai asc")

                    Dim strValueInsertAdditionalItem As New StringBuilder 'untuk query simpan di tabel additional utama
                    Dim strValueAdditionalItemDetail As New StringBuilder 'untuk query simpan di tabel additional detail
                    Dim idposadditionalitem As Integer 'untuk variabel id transaksi pos additional item utama
                    Dim dtselectId As New DataTable 'untuk query ambil id transaksi pos additional item
                    Dim dtAdditionalPenampung As New DataTable 'untuk menampung data bi additional
                    Dim dtKatPOS As New DataTable 'untuk menampung data kategori pos, jika jenis kategori 
                    strValueAdditionalItemDetail.Clear()

                    If dtdtl.Rows.Count > 0 Then

                        If drutama("aijeniskategori") = 1 Then 'JIKA PER KATEGORI
                            For Each drdtl As DataRow In dtdtl.Rows
                                'persiapan insert ke tabel m_12_pos_additional_item & tabel m_12_pos_additional_item_detail
                                strValueInsertAdditionalItem.Append(IIf(Len(strValueInsertAdditionalItem.ToString) = 0, "", ", "))
                                strValueInsertAdditionalItem.Append("('" & FixQuotes(drutama("aikategoripos")) & "', '" & FixQuotes(drdtl("idbarang")) & "', '" & FixQuotes(drdtl("operator")) & "', '" & FixDouble(drdtl("jml1")) & "', '" & FixDouble(drdtl("jml2")) & "', '" & FixQuotes(drdtl("customtext1")) & "', '" & FixQuotes(drdtl("customtext2")) & "', '" & FixQuotes(drdtl("customtext3")) & "', '" & FixQuotes(drdtl("customtext4")) & "', '" & FixQuotes(drdtl("customtext5")) & "', " & drdtl("customint1") & ", " & drdtl("customint2") & ", " & drdtl("customint3") & ", '" & FixDouble(drdtl("customdbl1")) & "', '" & FixDouble(drdtl("customdbl2")) & "', '" & FixDouble(drdtl("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl2"))) & "', '" & FixQuotes(drdtl("nopromo")) & "')")
                            Next

                            'insert ke tabel m_12_pos_additional_item
                            sql = "Insert into M_12_Pos_Additional_Item (aikategori, aiidbarang, aioperator, aijml1, aijml2, aicustomtext1, aicustomtext2, aicustomtext3, aicustomtext4, aicustomtext5, aicustomint1, aicustomint2, aicustomint3, aicustomdbl1, aicustomdbl2, aicustomdbl3, aicustomdate1, aicustomdate2, aicustomdate3, aitgl1, aitgl2, ainopromo) values " & strValueInsertAdditionalItem.ToString & ""
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
                                dtselectId = AsDataTableAmbilDariDB("select aiid from M_12_Pos_Additional_Item where aikategori = '" & drdtl2("aikategori") & "' AND aiidbarang = '" & drdtl2("idbarang") & "' AND aioperator = '" & drdtl2("operator") & "' AND aijml1 = '" & drdtl2("jml1") & "' AND aijml2 = '" & drdtl2("jml2") & "' limit 1")
                                If dtselectId.Rows.Count > 0 Then idposadditionalitem = dtselectId.Rows(0)(0) Else result(2) = "Main POS Additional Item transaction data not found." : Trans.Rollback() : GoTo selesai

                                'filter data additional penampung, untuk dijadikan parameter simpan ke tabel pos additional detail
                                dtAdditionalPenampung = AsDataTableFilterSortDt(dtbiadditional, "idaidetail = '" & drdtl2("idaidetail") & "'")
                                If dtAdditionalPenampung.Rows.Count > 0 Then
                                    For Each drAdditionalPenampung As DataRow In dtAdditionalPenampung.Rows
                                        'parameter simpan ke tabel m_12_pos_additional_item_DETAIL
                                        strValueAdditionalItemDetail.Append(IIf(Len(strValueAdditionalItemDetail.ToString) = 0, "", ", "))
                                        strValueAdditionalItemDetail.Append("(" & idposadditionalitem & ", '" & FixQuotes(drAdditionalPenampung("idbarang")) & "', '" & FixDouble(drAdditionalPenampung("jml")) & "', '" & FixQuotes(drAdditionalPenampung("satuan")) & "', '" & FixQuotes(drAdditionalPenampung("customtext1")) & "', '" & FixQuotes(drAdditionalPenampung("customtext2")) & "', '" & FixQuotes(drAdditionalPenampung("customtext3")) & "', '" & FixQuotes(drAdditionalPenampung("customtext4")) & "', '" & FixQuotes(drAdditionalPenampung("customtext5")) & "', " & drAdditionalPenampung("customint1") & ", " & drAdditionalPenampung("customint2") & ", " & drAdditionalPenampung("customint3") & ", '" & FixDouble(drAdditionalPenampung("customdbl1")) & "', '" & FixDouble(drAdditionalPenampung("customdbl2")) & "', '" & FixDouble(drAdditionalPenampung("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drAdditionalPenampung("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drAdditionalPenampung("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drAdditionalPenampung("customdate3"))) & "')")
                                    Next
                                Else
                                    result(2) = "Main Transaction POS Additional Item data not found." : Trans.Rollback() : GoTo selesai
                                End If
                            Next
                            'INSERT KE TABEL POS BONUS DETAIL
                            sql = "Insert into M_12_Pos_Additional_Item_Detail(idai, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValueAdditionalItemDetail.ToString & ""
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
                                                'persiapan insert ke tabel m_12_pos_additional_item 
                                                strValueInsertAdditionalItem.Append(IIf(Len(strValueInsertAdditionalItem.ToString) = 0, "", ", "))
                                                strValueInsertAdditionalItem.Append("('" & FixQuotes(drKatPos("pckode")) & "', '" & FixQuotes(drdtl("idbarang")) & "', '" & FixQuotes(drdtl("operator")) & "', '" & FixDouble(drdtl("jml1")) & "', '" & FixDouble(drdtl("jml2")) & "', '" & FixQuotes(drdtl("customtext1")) & "', '" & FixQuotes(drdtl("customtext2")) & "', '" & FixQuotes(drdtl("customtext3")) & "', '" & FixQuotes(drdtl("customtext4")) & "', '" & FixQuotes(drdtl("customtext5")) & "', " & drdtl("customint1") & ", " & drdtl("customint2") & ", " & drdtl("customint3") & ", '" & FixDouble(drdtl("customdbl1")) & "', '" & FixDouble(drdtl("customdbl2")) & "', '" & FixDouble(drdtl("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl2"))) & "', '" & FixQuotes(drdtl("nopromo")) & "')")

                                            Next
                                        End If

                                    Next
                                Next
                            End If

                            'insert ke tabel m_12_pos_additional_item
                            sql = "Insert into M_12_Pos_Additional_Item (aikategori, aiidbarang, aioperator, aijml1, aijml2, aicustomtext1, aicustomtext2, aicustomtext3, aicustomtext4, aicustomtext5, aicustomint1, aicustomint2, aicustomint3, aicustomdbl1, aicustomdbl2, aicustomdbl3, aicustomdate1, aicustomdate2, aicustomdate3, aitgl1, aitgl2, ainopromo) values " & strValueInsertAdditionalItem.ToString & ""
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
                                                dtselectId = AsDataTableAmbilDariDB("select aiid from M_12_Pos_Additional_Item where aikategori = '" & drKatPos("pckode") & "' AND aiidbarang = '" & drdtl2("idbarang") & "' AND aioperator = '" & drdtl2("operator") & "' AND aijml1 = '" & drdtl2("jml1") & "' AND aijml2 = '" & drdtl2("jml2") & "' limit 1")
                                                If dtselectId.Rows.Count > 0 Then idposadditionalitem = dtselectId.Rows(0)(0) Else result(2) = "Main POS Additional Item transaction data not found." : Trans.Rollback() : GoTo selesai
                                                'filter data additional penampung, untuk dijadikan parameter simpan ke tabel pos additional detail
                                                dtAdditionalPenampung = AsDataTableFilterSortDt(dtbiadditional, "idaidetail = '" & drdtl2("idaidetail") & "'")
                                                If dtAdditionalPenampung.Rows.Count > 0 Then
                                                    For Each drAdditionalPenampung As DataRow In dtAdditionalPenampung.Rows
                                                        'persiapan insert ke tabel m_12_pos_additional_item_DETAIL
                                                        strValueAdditionalItemDetail.Append(IIf(Len(strValueAdditionalItemDetail.ToString) = 0, "", ", "))
                                                        strValueAdditionalItemDetail.Append("(" & idposadditionalitem & ", '" & FixQuotes(drAdditionalPenampung("idbarang")) & "', '" & FixDouble(drAdditionalPenampung("jml")) & "', '" & FixQuotes(drAdditionalPenampung("satuan")) & "', '" & FixQuotes(drAdditionalPenampung("customtext1")) & "', '" & FixQuotes(drAdditionalPenampung("customtext2")) & "', '" & FixQuotes(drAdditionalPenampung("customtext3")) & "', '" & FixQuotes(drAdditionalPenampung("customtext4")) & "', '" & FixQuotes(drAdditionalPenampung("customtext5")) & "', " & drAdditionalPenampung("customint1") & ", " & drAdditionalPenampung("customint2") & ", " & drAdditionalPenampung("customint3") & ", '" & FixDouble(drAdditionalPenampung("customdbl1")) & "', '" & FixDouble(drAdditionalPenampung("customdbl2")) & "', '" & FixDouble(drAdditionalPenampung("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drAdditionalPenampung("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drAdditionalPenampung("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drAdditionalPenampung("customdate3"))) & "')")

                                                    Next
                                                Else
                                                    result(2) = "Main Transaction POS Additional Item data not found." : Trans.Rollback() : GoTo selesai
                                                End If
                                            Next
                                        End If
                                    Next
                                Next

                                'INSERT KE TABEL POS SUBSTITUTION DETAIL
                                sql = "Insert into M_12_Pos_Additional_Item_Detail(idai, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValueAdditionalItemDetail.ToString & ""
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
    Public Function M12_AiUpdateStatusOld(ByVal param As String) As String
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
            Filter = Filter.Replace("aikontakkode", "c.kkode")
            Filter = Filter.Replace("aikontaknama", "c.knama")
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
            Dim sumber As String = "AI", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Aitgl, Ainotransaksi, Aistatus FROM m_12_Ai WHERE Aiid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Aistatussebelumnya" : jnsaktivitas = 17
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
                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDB("SELECT * FROM M_12_Ai_Detail WHERE idai=" & idtransaksi)
                If (dtdetail.Rows.Count > 0) Then
                    For Each drdetail As DataRow In dtdetail.Rows
                        Dim dtadditional As New DataTable
                        Dim query As String = "SELECT aiid FROM m_12_pos_additional_item WHERE aikategori='" & drdetail("aikategori") & "'"
                        dtadditional = AsDataTableAmbilDariDB(query)
                        If dtadditional.Rows.Count > 0 Then
                            For Each dradditional As DataRow In dtadditional.Rows
                                'hapus data detail
                                Dim strValue2 As New StringBuilder
                                strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", "Or "))
                                strValue2.Append("aiid = '" & FixQuotes(dradditional("aiid")) & "' ")
                                sql = "Delete from M_12_pos_additional_item WHERE " & strValue2.ToString
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = Con1
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()


                                Dim strValueItemDetail As New StringBuilder
                                strValueItemDetail.Append(IIf(Len(strValueItemDetail.ToString) = 0, "", "Or "))
                                strValueItemDetail.Append("idai = '" & FixQuotes(dradditional("aiid")) & "'")

                                'hapus data detail
                                sql = "Delete from M_12_pos_additional_item_Detail WHERE " & strValueItemDetail.ToString
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
            End If


            'update status utama
            sql = "UPDATE M_12_Ai SET Aistatus = " & nilaiStatus & ", aimodifikasiuser='" & userid & "', aimodifikasitgl = NOW(), aiposting = 0, aipostingtgl = '1971-01-01 00:00:00', Aijmlrevisi = Aijmlrevisi + 1 WHERE aiid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_AiSearch(PostWsSearch(paramSplit(0), "M12_AiSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_AiDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("aikontakkode", "c.kkode")
            Filter = Filter.Replace("aikontaknama", "c.knama")
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
            Dim sumber As String = "AI", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT aiid, ainotransaksi FROM m_12_ai WHERE aiid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT aicabang, ailokasi, aisumber, aiautonotransaksi, ainotransaksi, aitgl"
            sql &= " FROM M_12_ai"
            sql &= " WHERE aiid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("aicabang")
                lokasi = dtNomorNext.Rows(0)("ailokasi")
                sumber = dtNomorNext.Rows(0)("aisumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("aiautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("ainotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("aitgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M_12_Ai_Detail WHERE idai = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M_12_Ai WHERE aiid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_AiSearch(PostWsSearch(paramSplit(0), "M12_AiSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_AiGetdataById(ByVal param As String) As String

        'M12_AiGetdataById Utama --------------------------------------------------------
        'aiid, aicabang, ailokasi, aisumber, aikategoripos, aiautonotransaksi, ainotransaksi, 
        'aitgl, aikodepa, aikontak, aikontakperson, aiuraian, aicatatan, aistatus, 
        'aistatussebelumnya, aijmlrevisi, aicetakanke, aiisclose, aiinputuser, aiinputtgl, aimodifikasiuser, 
        'aimodifikasitgl, aiposting, aipostingtgl, aicustomtext1, aicustomtext2, aicustomtext3, aicustomtext4, 
        'aicustomtext5, aicustomint1, aicustomint2, aicustomint3, aicustomdbl1, aicustomdbl2, aicustomdbl3, 
        'aicustomdate1, aicustomdate2, aicustomdate3, aicabangnama, ailokasinama, aikontakkode, 
        'aikontaknama, aistatusnama, aistatussebelumnyanama, aiinputusernama, aimodifikasiusernama, aikategoriposnama, aijeniskategori

        'M12_AiGetdataById Detail -------------------------------------------------------
        'idaidetail, aikategori, idbarang, operator, jml1, jml2, customtext1, 
        'customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, tgl1, 
        'tgl2, nopromo, kodebarang, namabarang, catatan, urutan

        'M12_aiGetdataById Additional -------------------------------------------------------
        'idadditional, idaidetail, idbarang, jml, satuan, customtext1, customtext2, 
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

        Dim utama As String = "", detail As String = "", additional As String = "", idtransaksi As String = ""

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
            Filter = "aiid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "aiid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m12_ai_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("aiid"), 0), sptField,
                     FxDB(drutama("aicabang"), ""), sptField,
                     FxDB(drutama("ailokasi"), ""), sptField,
                     FxDB(drutama("aisumber"), ""), sptField,
                     FxDB(drutama("aikategoripos"), ""), sptField,
                     FxDB(drutama("aiautonotransaksi"), 0), sptField,
                     FxDB(drutama("ainotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aitgl"), ""), formatTgl), sptField,
                     FxDB(drutama("aikodepa"), ""), sptField,
                     FxDB(drutama("aikontak"), ""), sptField,
                     FxDB(drutama("aikontakperson"), ""), sptField,
                     FxDB(drutama("aiuraian"), ""), sptField,
                     FxDB(drutama("aicatatan"), ""), sptField,
                     FxDB(drutama("aistatus"), 0), sptField,
                     FxDB(drutama("aistatussebelumnya"), 0), sptField,
                     FxDB(drutama("aijmlrevisi"), 0), sptField,
                     FxDB(drutama("aicetakanke"), 0), sptField,
                     FxDB(drutama("aiisclose"), 0), sptField,
                     FxDB(drutama("aiinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aiinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("aimodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aimodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("aiposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("aipostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("aicustomtext1"), ""), sptField,
                     FxDB(drutama("aicustomtext2"), ""), sptField,
                     FxDB(drutama("aicustomtext3"), ""), sptField,
                     FxDB(drutama("aicustomtext4"), ""), sptField,
                     FxDB(drutama("aicustomtext5"), ""), sptField,
                     FxDB(drutama("aicustomint1"), 0), sptField,
                     FxDB(drutama("aicustomint2"), 0), sptField,
                     FxDB(drutama("aicustomint3"), 0), sptField,
                     FxDB(drutama("aicustomdbl1"), 0), sptField,
                     FxDB(drutama("aicustomdbl2"), 0), sptField,
                     FxDB(drutama("aicustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("aicustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("aicustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("aicustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("aicabangnama"), ""), sptField,
                     FxDB(drutama("ailokasinama"), ""), sptField,
                     FxDB(drutama("aikontakkode"), ""), sptField,
                     FxDB(drutama("aikontaknama"), ""), sptField,
                     FxDB(drutama("aistatusnama"), ""), sptField,
                     FxDB(drutama("aistatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("aiinputusernama"), ""), sptField,
                     FxDB(drutama("aimodifikasiusernama"), ""), sptField,
                     FxDB(drutama("aikategoriposnama"), ""), sptField,
                     FxDB(drutama("aijeniskategori"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idaidetail"), 0), sptField,
                     FxDB(dr("idai"), 0), sptField,
                     FxDB(dr("aikategori"), ""), sptField,
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
            Dim queryadditional As New m0_query
            sql = "select `aib`.`idadditional` AS `idadditional`, `aib`.`idaidetail` AS `idaidetail`,`aib`.`idai` AS `idai`,`aib`.`idbarang` AS `idbarang`,`aib`.`jml` AS `jml`,`aib`.`satuan` AS `satuan`,`aib`.`customtext1` AS `customtext1`,`aib`.`customtext2` AS `customtext2`,`aib`.`customtext3` AS `customtext3`,`aib`.`customtext4` AS `customtext4`,`aib`.`customtext5` AS `customtext5`,`aib`.`customint1` AS `customint1`,`aib`.`customint2` AS `customint2`,`aib`.`customint3` AS `customint3`,`aib`.`customdbl1` AS `customdbl1`,`aib`.`customdbl2` AS `customdbl2`,`aib`.`customdbl3` AS `customdbl3`,`aib`.`customdate1` AS `customdate1`,`aib`.`customdate2` AS `customdate2`,`aib`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`i`.`bnama` AS `namabarang`,`aib`.`urutan` AS `urutan` FROM `m_12_ai_additional` `aib` JOIN m1_item `i` ON (`aib`.`idbarang` = `i`.bid) WHERE `aib`.idai='" & idtransaksi & "' ORDER BY `aib`.`urutan` ASC"
            Dim dtadditional As New DataTable
            dtadditional = AmbilData("aplikasi1-M_12_Bi_Additional", "", "", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dradditional As DataRow In dtadditional.Rows
                additional = String.Concat(additional,
                     FxDB(dradditional("idadditional"), 0), sptField,
                     FxDB(dradditional("idaidetail"), 0), sptField,
                     FxDB(dradditional("idai"), 0), sptField,
                     FxDB(dradditional("idbarang"), 0), sptField,
                     FxDB(dradditional("jml"), 0), sptField,
                     FxDB(dradditional("satuan"), ""), sptField,
                     FxDB(dradditional("customtext1"), ""), sptField,
                     FxDB(dradditional("customtext2"), ""), sptField,
                     FxDB(dradditional("customtext3"), ""), sptField,
                     FxDB(dradditional("customtext4"), ""), sptField,
                     FxDB(dradditional("customtext5"), ""), sptField,
                     FxDB(dradditional("customint1"), 0), sptField,
                     FxDB(dradditional("customint2"), 0), sptField,
                     FxDB(dradditional("customint3"), 0), sptField,
                     FxDB(dradditional("customdbl1"), 0), sptField,
                     FxDB(dradditional("customdbl2"), 0), sptField,
                     FxDB(dradditional("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dradditional("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dradditional("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dradditional("customdate3"), ""), formatTgl), sptField,
                     FxDB(dradditional("kodebarang"), 0), sptField,
                     FxDB(dradditional("namabarang"), 0), sptField,
                     FxDB(dradditional("urutan"), 0), sptRow)
            Next
            additional = additional.Substring(0, additional.Length - sptRow.Length)

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, additional)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("aiid, aicabang, ailokasi, aisumber, aikategoripos, aiautonotransaksi, ainotransaksi, aitgl, aikodepa, aikontak, aikontakperson, aiuraian, aicatatan, aistatus, aistatussebelumnya, aijmlrevisi, aicetakanke, aiisclose, aiinputuser, aiinputtgl, aimodifikasiuser, aimodifikasitgl, aiposting, aipostingtgl, aicustomtext1, aicustomtext2, aicustomtext3, aicustomtext4, aicustomtext5, aicustomint1, aicustomint2, aicustomint3, aicustomdbl1, aicustomdbl2, aicustomdbl3, aicustomdate1, aicustomdate2, aicustomdate3, aicabangnama, ailokasinama, aikontakkode, aikontaknama, aistatusnama, aistatussebelumnyanama, aiinputusernama, aimodifikasiusernama, aikategoriposnama, aijeniskategori" & sptSubParam & "idaidetail, idai, aikategori, idbarang, operator, jml1, jml2, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, tgl1, tgl2, nopromo, kodebarang, namabarang, catatan, urutan" & sptSubParam & "idadditional, idaidetail, idai, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, namabarang, urutan"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_AiSearch(ByVal param As String) As String
        'M12_AiSearch --------------------------------------------------------
        'aiid, aicabang, ailokasi, aisumber, aikategoripos, aiautonotransaksi, ainotransaksi, 
        'aitgl, aikodepa, aikontak, aikontakperson, aiuraian, aicatatan, aistatus, 
        'aistatussebelumnya, aijmlrevisi, aicetakanke, aiisclose, aiinputuser, aiinputtgl, aimodifikasiuser, 
        'aimodifikasitgl, aiposting, aipostingtgl, aicustomtext1, aicustomtext2, aicustomtext3, aicustomtext4, 
        'aicustomtext5, aicustomint1, aicustomint2, aicustomint3, aicustomdbl1, aicustomdbl2, aicustomdbl3, 
        'aicustomdate1, aicustomdate2, aicustomdate3, aicabangnama, ailokasinama, aikontakkode, 
        'aikontaknama, aistatusnama, aistatussebelumnyanama, aiinputusernama, aimodifikasiusernama

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
        sql = "select `ai`.`aiid` AS `aiid`,`ai`.`aicabang` AS `aicabang`,`ai`.`ailokasi` AS `ailokasi`,`ai`.`aisumber` AS `aisumber`,`ai`.`aiautonotransaksi` AS `aiautonotransaksi`,`ai`.`ainotransaksi` AS `ainotransaksi`,`ai`.`aitgl` AS `aitgl`,`ai`.`aikodepa` AS `aikodepa`,`ai`.`aikontak` AS `aikontak`,`ai`.`aikontakperson` AS `aikontakperson`,`ai`.`aikategoripos` AS `aikategoripos`,`ai`.`aiuraian` AS `aiuraian`,`ai`.`aicatatan` AS `aicatatan`,`ai`.`aistatus` AS `aistatus`,`ai`.`aistatussebelumnya` AS `aistatussebelumnya`,`ai`.`aijmlrevisi` AS `aijmlrevisi`,`ai`.`aicetakanke` AS `aicetakanke`,`ai`.`aiisclose` AS `aiisclose`,`ai`.`aiinputuser` AS `aiinputuser`,`ai`.`aiinputtgl` AS `aiinputtgl`,`ai`.`aimodifikasiuser` AS `aimodifikasiuser`,`ai`.`aimodifikasitgl` AS `aimodifikasitgl`,`ai`.`aiposting` AS `aiposting`,`ai`.`aipostingtgl` AS `aipostingtgl`,`ai`.`aicustomtext1` AS `aicustomtext1`,`ai`.`aicustomtext2` AS `aicustomtext2`,`ai`.`aicustomtext3` AS `aicustomtext3`,`ai`.`aicustomtext4` AS `aicustomtext4`,`ai`.`aicustomtext5` AS `aicustomtext5`,`ai`.`aicustomint1` AS `aicustomint1`,`ai`.`aicustomint2` AS `aicustomint2`,`ai`.`aicustomint3` AS `aicustomint3`,`ai`.`aicustomdbl1` AS `aicustomdbl1`,`ai`.`aicustomdbl2` AS `aicustomdbl2`,`ai`.`aicustomdbl3` AS `aicustomdbl3`,`ai`.`aicustomdate1` AS `aicustomdate1`,`ai`.`aicustomdate2` AS `aicustomdate2`,`ai`.`aicustomdate3` AS `aicustomdate3`,`br`.`bnama` AS `aicabangnama`,`lc`.`lnama` AS `ailokasinama`,`c`.`kkode` AS `aikontakkode`,`c`.`knama` AS `aikontaknama`,`st1`.`nama` AS `aistatusnama`,`st2`.`nama` AS `aistatussebelumnyanama`,`u1`.`unama` AS `aiinputusernama`,`u2`.`unama` AS `aimodifikasiusernama` from (((((((`m_12_ai` `ai` left join `m1_branch` `br` on((`ai`.`aicabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`ai`.`ailokasi` = `lc`.`lkode`))) left join `m1_contact` `c` on((`ai`.`aikontak` = `c`.`kid`))) left join `m0_status` `st1` on((`ai`.`aistatus` = `st1`.`kode`))) left join `m0_status` `st2` on((`ai`.`aistatussebelumnya` = `st2`.`kode`))) left join `m0_user` `u1` on((`ai`.`aiinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`ai`.`aimodifikasiuser` = `u2`.`userid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Cr~M2_Cr_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("aiid"), 0), sptField,
                             FxDB(dr("aicabang"), ""), sptField,
                             FxDB(dr("ailokasi"), ""), sptField,
                             FxDB(dr("aisumber"), ""), sptField,
                             FxDB(dr("aikategoripos"), ""), sptField,
                             FxDB(dr("aiautonotransaksi"), 0), sptField,
                             FxDB(dr("ainotransaksi"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("aitgl"), ""), formatTgl), sptField,
                             FxDB(dr("aikodepa"), ""), sptField,
                             FxDB(dr("aikontak"), ""), sptField,
                             FxDB(dr("aikontakperson"), ""), sptField,
                             FxDB(dr("aiuraian"), ""), sptField,
                             FxDB(dr("aicatatan"), ""), sptField,
                             FxDB(dr("aistatus"), 0), sptField,
                             FxDB(dr("aistatussebelumnya"), 0), sptField,
                             FxDB(dr("aijmlrevisi"), 0), sptField,
                             FxDB(dr("aicetakanke"), 0), sptField,
                             FxDB(dr("aiisclose"), 0), sptField,
                             FxDB(dr("aiinputuser"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("aiinputtgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("aimodifikasiuser"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("aimodifikasitgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("aiposting"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("aipostingtgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("aicustomtext1"), ""), sptField,
                             FxDB(dr("aicustomtext2"), ""), sptField,
                             FxDB(dr("aicustomtext3"), ""), sptField,
                             FxDB(dr("aicustomtext4"), ""), sptField,
                             FxDB(dr("aicustomtext5"), ""), sptField,
                             FxDB(dr("aicustomint1"), 0), sptField,
                             FxDB(dr("aicustomint2"), 0), sptField,
                             FxDB(dr("aicustomint3"), 0), sptField,
                             FxDB(dr("aicustomdbl1"), 0), sptField,
                             FxDB(dr("aicustomdbl2"), 0), sptField,
                             FxDB(dr("aicustomdbl3"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("aicustomdate1"), ""), formatTgl), sptField,
                             AsFormatTanggal(FxDB(dr("aicustomdate2"), ""), formatTgl), sptField,
                             AsFormatTanggal(FxDB(dr("aicustomdate3"), ""), formatTgl), sptField,
                             FxDB(dr("aicabangnama"), ""), sptField,
                             FxDB(dr("ailokasinama"), ""), sptField,
                             FxDB(dr("aikontakkode"), ""), sptField,
                             FxDB(dr("aikontaknama"), ""), sptField,
                             FxDB(dr("aistatusnama"), ""), sptField,
                             FxDB(dr("aistatussebelumnyanama"), ""), sptField,
                             FxDB(dr("aiinputusernama"), ""), sptField,
                             FxDB(dr("aimodifikasiusernama"), ""), sptRow)

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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("aiid, aicabang, ailokasi, aisumber, aikategoripos, aiautonotransaksi, ainotransaksi, aitgl, aikodepa, aikontak, aikontakperson, aiuraian, aicatatan, aistatus, aistatussebelumnya, aijmlrevisi, aicetakanke, aiisclose, aiinputuser, aiinputtgl, aimodifikasiuser, aimodifikasitgl, aiposting, aipostingtgl, aicustomtext1, aicustomtext2, aicustomtext3, aicustomtext4, aicustomtext5, aicustomint1, aicustomint2, aicustomint3, aicustomdbl1, aicustomdbl2, aicustomdbl3, aicustomdate1, aicustomdate2, aicustomdate3, aicabangnama, ailokasinama, aikontakkode, aikontaknama, aistatusnama, aistatussebelumnyanama, aiinputusernama, aimodifikasiusernama"))

        Return wsResult
    End Function

End Class
