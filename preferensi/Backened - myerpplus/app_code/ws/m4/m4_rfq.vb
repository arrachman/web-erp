Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_rfq
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_RfqSimpan(ByVal param As String) As String
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
        'rfqid(0) As , rfqcabang(1) As String, rfqlokasi(2) As String, rfqsumber(3) As String, rfqautonotransaksi(4) As Integer, 
        'rfqnotransaksi(5) As String, rfqtgl(6) As Date, rfqkodepa(7) As , rfqidpr(8) As , rfqkontakperson(9) As String, 
        'rfq1alamat1(10) As String, rfq1alamat2(11) As String, rfq1alamat3(12) As String, rfq2alamat1(13) As String, rfq2alamat2(14) As String, 
        'rfq2alamat3(15) As String, rfquraian(16) As String, rfqcatatan(17) As String, rfqnoref(18) As String, rfqtglnoref(19) As Date, 
        'rfqstatus(20) As Integer, rfqstatussebelumnya(21) As Integer, rfqjmlrevisi(22) As Integer, rfqcetakanke(23) As Integer, rfqinputuser(24) As , 
        'rfqinputtgl(25) As DateTime, rfqmodifikasiuser(26) As , rfqmodifikasitgl(27) As DateTime, rfqposting(28) As Integer, rfqpostingtgl(29) As DateTime, 
        'rfqisclose(30) As Integer, rfqcustomtext1(31) As String, rfqcustomtext2(32) As String, rfqcustomtext3(33) As String, rfqcustomtext4(34) As String, 
        'rfqcustomtext5(35) As String, rfqcustomint1(36) As Integer, rfqcustomint2(37) As Integer, rfqcustomint3(38) As Integer, rfqcustomdbl1(39) As Double, 
        'rfqcustomdbl2(40) As Double, rfqcustomdbl3(41) As Double, rfqcustomdate1(42) As Date, rfqcustomdate2(43) As Date, rfqcustomdate3(44) As Date
        'rfqtglawal(45) As DateTime, rfqtglakhir(46) As DateTime


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'rfqid, rfqcabang, rfqlokasi, rfqsumber, rfqautonotransaksi, rfqnotransaksi, rfqtgl, 
        'rfqkodepa, rfqidpr, rfqkontakperson, rfq1alamat1, rfq1alamat2, rfq1alamat3, rfq2alamat1, 
        'rfq2alamat2, rfq2alamat3, rfquraian, rfqcatatan, rfqnoref, rfqtglnoref, rfqstatus, 
        'rfqstatussebelumnya, rfqjmlrevisi, rfqcetakanke, rfqinputuser, rfqinputtgl, rfqmodifikasiuser, rfqmodifikasitgl, 
        'rfqposting, rfqpostingtgl, rfqisclose, rfqcustomtext1, rfqcustomtext2, rfqcustomtext3, rfqcustomtext4, 
        'rfqcustomtext5, rfqcustomint1, rfqcustomint2, rfqcustomint3, rfqcustomdbl1, rfqcustomdbl2, rfqcustomdbl3, 
        'rfqcustomdate1, rfqcustomdate2, rfqcustomdate3, rfqtglawal, rfqtglakhir

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 47) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'rfqautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "rfqautonotransaksi required numeric." : GoTo selesai
        End If
        'rfqtgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "rfqtgl required date." : GoTo selesai
        End If
        'rfqtglnoref(19) As Date
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "rfqtglnoref required date." : GoTo selesai
        End If
        'rfqstatus(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "rfqstatus required numeric." : GoTo selesai
        End If
        'rfqstatussebelumnya(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "rfqstatussebelumnya required numeric." : GoTo selesai
        End If
        'rfqjmlrevisi(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "rfqjmlrevisi required numeric." : GoTo selesai
        End If
        'rfqcetakanke(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "rfqcetakanke required numeric." : GoTo selesai
        End If
        'rfqinputtgl(25) As DateTime
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "rfqinputtgl required date." : GoTo selesai
        End If
        'rfqmodifikasitgl(27) As DateTime
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "rfqmodifikasitgl required date." : GoTo selesai
        End If
        'rfqposting(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "rfqposting required numeric." : GoTo selesai
        End If
        'rfqpostingtgl(29) As DateTime
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "rfqpostingtgl required date." : GoTo selesai
        End If
        'rfqisclose(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "rfqisclose required numeric." : GoTo selesai
        End If
        'rfqcustomint1(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "rfqcustomint1 required numeric." : GoTo selesai
        End If
        'rfqcustomint2(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "rfqcustomint2 required numeric." : GoTo selesai
        End If
        'rfqcustomint3(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "rfqcustomint3 required numeric." : GoTo selesai
        End If
        'rfqcustomdbl1(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "rfqcustomdbl1 required numeric." : GoTo selesai
        End If
        'rfqcustomdbl2(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "rfqcustomdbl2 required numeric." : GoTo selesai
        End If
        'rfqcustomdbl3(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "rfqcustomdbl3 required numeric." : GoTo selesai
        End If
        'rfqcustomdate1(42) As Date
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "rfqcustomdate1 required date." : GoTo selesai
        End If
        'rfqcustomdate2(43) As Date
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "rfqcustomdate2 required date." : GoTo selesai
        End If
        'rfqcustomdate3(44) As Date
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "rfqcustomdate3 required date." : GoTo selesai
        End If

        If dataUtama.Length > 45 Then
            'rfqtglawal(68) As DateTime
            If (IsDate(dataUtama(45)) = False) Then
                result(2) = "rfqtglawal required date." : GoTo selesai
            End If
            'rfqtglakhir(69) As DateTime
            If (IsDate(dataUtama(46)) = False) Then
                result(2) = "rfqtglakhir required date." : GoTo selesai
            End If
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'rfqid(0) As 
        If Len(dataUtama(0)) = 0 Then
            result(2) = "rfqid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 20 Then
            result(2) = "rfqid should not be more than 20 character." : GoTo selesai
        End If

        'rfqcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "rfqcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "rfqcabang should not be more than 25 character." : GoTo selesai
        End If

        'rfqlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "rfqlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "rfqlokasi should not be more than 25 character." : GoTo selesai
        End If

        'rfqsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "rfqsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "rfqsumber should not be more than 10 character." : GoTo selesai
        End If

        'rfqnotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "rfqnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "rfqnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'rfqtgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "rfqtgl can't be empty" : GoTo selesai
        End If

        'rfqkodepa(7) As 
        If Len(dataUtama(7)) = 0 Then
            result(2) = "rfqkodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 20 Then
            result(2) = "rfqkodepa should not be more than 20 character." : GoTo selesai
        End If

        'rfqidpr(8) As 
        If Len(dataUtama(8)) = 0 Then
            result(2) = "rfqidpr can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 20 Then
            result(2) = "rfqidpr should not be more than 20 character." : GoTo selesai
        End If

        'rfqtglnoref(19) As Date
        If Len(dataUtama(19)) = 0 Then
            result(2) = "rfqtglnoref can't be empty" : GoTo selesai
        End If

        'rfqinputtgl(25) As DateTime
        If Len(dataUtama(25)) = 0 Then
            result(2) = "rfqinputtgl can't be empty" : GoTo selesai
        End If

        'rfqmodifikasitgl(27) As DateTime
        If Len(dataUtama(27)) = 0 Then
            result(2) = "rfqmodifikasitgl can't be empty" : GoTo selesai
        End If

        'rfqpostingtgl(29) As DateTime
        If Len(dataUtama(29)) = 0 Then
            result(2) = "rfqpostingtgl can't be empty" : GoTo selesai
        End If

        'rfqcustomdbl1(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "rfqcustomdbl1 can't be empty" : GoTo selesai
        End If

        'rfqcustomdbl2(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "rfqcustomdbl2 can't be empty" : GoTo selesai
        End If

        'rfqcustomdbl3(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "rfqcustomdbl3 can't be empty" : GoTo selesai
        End If

        'rfqcustomdate1(42) As Date
        If Len(dataUtama(42)) = 0 Then
            result(2) = "rfqcustomdate1 can't be empty" : GoTo selesai
        End If

        'rfqcustomdate2(43) As Date
        If Len(dataUtama(43)) = 0 Then
            result(2) = "rfqcustomdate2 can't be empty" : GoTo selesai
        End If

        'rfqcustomdate3(44) As Date
        If Len(dataUtama(44)) = 0 Then
            result(2) = "rfqcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "rfqid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfqcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfqlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfqsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfqautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfqnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfqtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfqkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfqidpr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfqkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfq1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfq1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfq1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfq2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfq2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfq2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfquraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfqcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfqnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfqtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfqstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfqstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfqjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfqcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfqinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfqinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfqmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfqmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfqposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfqpostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfqisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfqcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfqcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfqcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfqcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfqcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfqcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfqcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfqcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfqcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfqcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfqcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfqcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfqcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfqcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfqtglawal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfqtglakhir", AsEnumTypeData.AsString)
        AsDataTableTambahData(dtutama, "rfqid~rfqcabang~rfqlokasi~rfqsumber~rfqautonotransaksi~rfqnotransaksi~rfqtgl~rfqkodepa~rfqidpr~rfqkontakperson~rfq1alamat1~rfq1alamat2~rfq1alamat3~rfq2alamat1~rfq2alamat2~rfq2alamat3~rfquraian~rfqcatatan~rfqnoref~rfqtglnoref~rfqstatus~rfqstatussebelumnya~rfqjmlrevisi~rfqcetakanke~rfqinputuser~rfqinputtgl~rfqmodifikasiuser~rfqmodifikasitgl~rfqposting~rfqpostingtgl~rfqisclose~rfqcustomtext1~rfqcustomtext2~rfqcustomtext3~rfqcustomtext4~rfqcustomtext5~rfqcustomint1~rfqcustomint2~rfqcustomint3~rfqcustomdbl1~rfqcustomdbl2~rfqcustomdbl3~rfqcustomdate1~rfqcustomdate2~rfqcustomdate3~rfqtglawal~rfqtglakhir", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46))

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idrfqdetail(0) As , idrfq(1) As , sumber(2) As String, idkontak(3) As , catatan(4) As String, 
        'urutan(5) As Integer, isclose(6) As Integer, customtext1(7) As String, customtext2(8) As String, customtext3(9) As String, 
        'customdbl1(10) As Double, customdbl2(11) As Double, customdbl3(12) As Double, customdate1(13) As Date, customdate2(14) As Date, 
        'customdate3(15) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idrfqdetail, idrfq, sumber, idkontak, catatan, urutan, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idrfqdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idrfq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
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
            If (dataRowDetail.Length <> 16) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'urutan(5) As Integer
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "urutan required numeric." : GoTo selesai
            End If
            'isclose(6) As Integer
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "isclose required numeric." : GoTo selesai
            End If
            'customdbl1(10) As Double
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(11) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(13) As Date
            If (IsDate(dataRowDetail(13)) = False) Then
                result(2) = "customdate1 required date." : GoTo selesai
            End If
            'customdate2(14) As Date
            If (IsDate(dataRowDetail(14)) = False) Then
                result(2) = "customdate2 required date." : GoTo selesai
            End If
            'customdate3(15) As Date
            If (IsDate(dataRowDetail(15)) = False) Then
                result(2) = "customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'idrfqdetail(0) As 
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idrfqdetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idrfqdetail should not be more than 20 character." : GoTo selesai
            End If

            'idrfq(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idrfq can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idrfq should not be more than 20 character." : GoTo selesai
            End If

            ''sumber(2) As String
            'If Len(dataRowDetail(2)) = 0 Then
            '    result(2) = "Row : " & i & " - sumber can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(2)) > 10 Then
                result(2) = "Row : " & i & " - sumber should not be more than 10 character." : GoTo selesai
            End If

            'idkontak(3) As 
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - idkontak can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 20 Then
                result(2) = "Row : " & i & " - idkontak should not be more than 20 character." : GoTo selesai
            End If

            'customdbl1(10) As Double
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(11) As Double
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(13) As Date
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(14) As Date
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(15) As Date
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            AsDataTableTambahData(dtdetail, "idrfqdetail~idrfq~sumber~idkontak~catatan~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15))

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
                Dim vModuleId As Integer = 4, vMenuId As Integer = 59
                Select Case drutama("rfqstatus")
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


                ''CEK PERIODE AKUNTANSI ==================================
                'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("rfqtgl")), AsFormatTanggal(drutama("rfqtgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================

                If isUpdate Then
                    result(4) = drutama("rfqid")
                    notransaksi = drutama("rfqnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(rfqid), rfqnotransaksi FROM M4_rfq WHERE rfqid='" & result(4) & "' AND rfqstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("rfqautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rfqcabang"), drutama("rfqlokasi"), drutama("rfqsumber"), drutama("rfqtgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(rfqid) FROM m4_rfq WHERE rfqnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_rfq_history
                        Dim rsSimpanHistory As String = SimpanHistory.M4_Rfq_HistorySimpan("" & paramSplit(0) & "★M4_Rfq_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("rfqsumber")) & "▼" & FixQuotes(drutama("rfqid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================
                        sql = "Update M4_Rfq set rfqcabang  = '" & FixQuotes(drutama("rfqcabang")) & "', rfqlokasi  = '" & FixQuotes(drutama("rfqlokasi")) & "', rfqsumber  = '" & FixQuotes(drutama("rfqsumber")) & "', rfqautonotransaksi  = " & drutama("rfqautonotransaksi") & ", rfqnotransaksi  = '" & FixQuotes(notransaksi) & "', rfqtgl  = '" & FixQuotes(AsFormatTanggal(drutama("rfqtgl"))) & "', rfqkodepa  = '" & FixQuotes(drutama("rfqkodepa")) & "', rfqidpr  = '" & FixQuotes(drutama("rfqidpr")) & "', rfqkontakperson  = '" & FixQuotes(drutama("rfqkontakperson")) & "', rfq1alamat1  = '" & FixQuotes(drutama("rfq1alamat1")) & "', rfq1alamat2  = '" & FixQuotes(drutama("rfq1alamat2")) & "', rfq1alamat3  = '" & FixQuotes(drutama("rfq1alamat3")) & "', rfq2alamat1  = '" & FixQuotes(drutama("rfq2alamat1")) & "', rfq2alamat2  = '" & FixQuotes(drutama("rfq2alamat2")) & "', rfq2alamat3  = '" & FixQuotes(drutama("rfq2alamat3")) & "', rfquraian  = '" & FixQuotes(drutama("rfquraian")) & "', rfqcatatan  = '" & FixQuotes(drutama("rfqcatatan")) & "', rfqnoref  = '" & FixQuotes(drutama("rfqnoref")) & "', rfqtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("rfqtglnoref"))) & "', rfqstatus  = " & drutama("rfqstatus") & ", rfqstatussebelumnya  = " & drutama("rfqstatussebelumnya") & ", rfqjmlrevisi  = " & drutama("rfqjmlrevisi") & ", rfqcetakanke  = " & drutama("rfqcetakanke") & ", rfqinputuser  = '" & FixQuotes(drutama("rfqinputuser")) & "', rfqinputtgl  = '" & FixQuotes(AsFormatTanggal(drutama("rfqinputtgl"), "yyyy-MM-dd HH:mm:ss")) & "', rfqmodifikasiuser  = '" & FixQuotes(drutama("rfqmodifikasiuser")) & "', rfqmodifikasitgl  = '" & FixQuotes(AsFormatTanggal(drutama("rfqmodifikasitgl"), "yyyy-MM-dd HH:mm:ss")) & "', rfqposting  = " & drutama("rfqposting") & ", rfqpostingtgl  = '" & FixQuotes(AsFormatTanggal(drutama("rfqpostingtgl"), "yyyy-MM-dd HH:mm:ss")) & "', rfqcustomtext1  = '" & FixQuotes(drutama("rfqcustomtext1")) & "', rfqcustomtext2  = '" & FixQuotes(drutama("rfqcustomtext2")) & "', rfqcustomtext3  = '" & FixQuotes(drutama("rfqcustomtext3")) & "', rfqcustomtext4  = '" & FixQuotes(drutama("rfqcustomtext4")) & "', rfqcustomtext5  = '" & FixQuotes(drutama("rfqcustomtext5")) & "', rfqcustomint1  = " & drutama("rfqcustomint1") & ", rfqcustomint2  = " & drutama("rfqcustomint2") & ", rfqcustomint3  = " & drutama("rfqcustomint3") & ", rfqcustomdbl1  = '" & FixDouble(drutama("rfqcustomdbl1")) & "', rfqcustomdbl2  = '" & FixDouble(drutama("rfqcustomdbl2")) & "', rfqcustomdbl3  = '" & FixDouble(drutama("rfqcustomdbl3")) & "', rfqcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("rfqcustomdate1"))) & "', rfqcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("rfqcustomdate2"))) & "', rfqcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("rfqcustomdate3"))) & "', rfqtglawal  = '" & FixQuotes(drutama("rfqtglawal")) & "', rfqtglakhir  = '" & FixQuotes(drutama("rfqtglakhir")) & "' where rfqid = " & drutama("rfqid") & ""
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

                    If drutama("rfqautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rfqcabang"), drutama("rfqlokasi"), drutama("rfqsumber"), drutama("rfqtgl"))
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
                        notransaksi = drutama("rfqnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(rfqid) FROM m4_rfq WHERE rfqnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Rfq (rfqcabang, rfqlokasi, rfqsumber, rfqautonotransaksi, rfqnotransaksi, rfqtgl, rfqkodepa, rfqidpr, rfqkontakperson, rfq1alamat1, rfq1alamat2, rfq1alamat3, rfq2alamat1, rfq2alamat2, rfq2alamat3, rfquraian, rfqcatatan, rfqnoref, rfqtglnoref, rfqstatus, rfqstatussebelumnya, rfqjmlrevisi, rfqcetakanke, rfqinputuser, rfqinputtgl, rfqmodifikasiuser, rfqmodifikasitgl, rfqposting, rfqpostingtgl, rfqisclose, rfqcustomtext1, rfqcustomtext2, rfqcustomtext3, rfqcustomtext4, rfqcustomtext5, rfqcustomint1, rfqcustomint2, rfqcustomint3, rfqcustomdbl1, rfqcustomdbl2, rfqcustomdbl3, rfqcustomdate1, rfqcustomdate2, rfqcustomdate3, rfqtglawal, rfqtglakhir) values('" & FixQuotes(drutama("rfqcabang")) & "', '" & FixQuotes(drutama("rfqlokasi")) & "', '" & FixQuotes(drutama("rfqsumber")) & "', " & drutama("rfqautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("rfqtgl"))) & "', '" & FixQuotes(drutama("rfqkodepa")) & "', '" & FixQuotes(drutama("rfqidpr")) & "', '" & FixQuotes(drutama("rfqkontakperson")) & "', '" & FixQuotes(drutama("rfq1alamat1")) & "', '" & FixQuotes(drutama("rfq1alamat2")) & "', '" & FixQuotes(drutama("rfq1alamat3")) & "', '" & FixQuotes(drutama("rfq2alamat1")) & "', '" & FixQuotes(drutama("rfq2alamat2")) & "', '" & FixQuotes(drutama("rfq2alamat3")) & "', '" & FixQuotes(drutama("rfquraian")) & "', '" & FixQuotes(drutama("rfqcatatan")) & "', '" & FixQuotes(drutama("rfqnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rfqtglnoref"))) & "', " & drutama("rfqstatus") & ", " & drutama("rfqstatussebelumnya") & ", " & drutama("rfqjmlrevisi") & ", " & drutama("rfqcetakanke") & ", '" & FixQuotes(drutama("rfqinputuser")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rfqinputtgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(drutama("rfqmodifikasiuser")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rfqmodifikasitgl"), "yyyy-MM-dd HH:mm:ss")) & "', " & drutama("rfqposting") & ", '" & FixQuotes(AsFormatTanggal(drutama("rfqpostingtgl"), "yyyy-MM-dd HH:mm:ss")) & "', " & drutama("rfqisclose") & ", '" & FixQuotes(drutama("rfqcustomtext1")) & "', '" & FixQuotes(drutama("rfqcustomtext2")) & "', '" & FixQuotes(drutama("rfqcustomtext3")) & "', '" & FixQuotes(drutama("rfqcustomtext4")) & "', '" & FixQuotes(drutama("rfqcustomtext5")) & "', " & drutama("rfqcustomint1") & ", " & drutama("rfqcustomint2") & ", " & drutama("rfqcustomint3") & ", '" & FixDouble(drutama("rfqcustomdbl1")) & "', '" & FixDouble(drutama("rfqcustomdbl2")) & "', '" & FixDouble(drutama("rfqcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rfqcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rfqcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rfqcustomdate3"))) & "', '" & FixQuotes(drutama("rfqtglawal")) & "', '" & FixQuotes(drutama("rfqtglakhir")) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select rfqid from M4_rfq where rfqnotransaksi='" & notransaksi & "' AND rfqinputuser= '" & userid & "' order by rfqmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Rfq_Detail where idrfq = '" & result(4) & "'"
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
                    Dim strValue2 As New StringBuilder, strGiro As New StringBuilder

                    For Each dr1 As DataRow In dtdetail.Rows

                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("idrfqdetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("sumber")) & "', '" & FixQuotes(dr1("idkontak")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Rfq_Detail(idrfqdetail, idrfq, sumber, idkontak, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                Dim sumber As String = "RFQ", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0

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
    Public Function M4_RfqUpdateStatus(ByVal param As String) As String

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
        'If Len(pagingSplit(5)) = 0 Then
        formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        ' Else
        'formatTglWaktu = pagingSplit(5)
        'End If
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

        'VALIDASI DAN SET ISDELETE =========================================================
        'CEK ISDELETE
        If (IsNumeric(paramSplit(4)) = False) Then
            result(2) = "isdelete required numeric." : GoTo selesai
        Else
            'SET ISDELETE
            If (Val(paramSplit(4)) = 1) Then
                isDelete = True
            Else
                isDelete = False
            End If
        End If
        'END OF VALIDASI DAN SET ISDELETE ==================================================

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
            Dim sumber As String = "Rfq", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Rfqtgl, rfqnotransaksi, rfqstatus FROM m4_Rfq WHERE Rfqid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Rfqstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m4_rfq_history
            Dim rsSimpanHistory As String = SimpanHistory.M4_Rfq_HistorySimpan("" & paramSplit(0) & "★M4_Rfq_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            'update status utama
            sql = "UPDATE m4_Rfq SET Rfqstatus = " & nilaiStatus & ", Rfqmodifikasiuser='" & userid & "', Rfqmodifikasitgl = NOW(), Rfqposting = 0, Rfqpostingtgl = '1971-01-01 00:00:00', Rfqjmlrevisi = Rfqjmlrevisi + 1 WHERE Rfqid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_RfqSearch(PostWsSearch(paramSplit(0), "M4_RfqSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_RfqDelete(ByVal param As String) As String

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
            Dim sumber As String = "Rfq", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Rfqid, Rfqnotransaksi FROM M4_Rfq WHERE Rfqid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT rfqcabang, rfqlokasi, rfqsumber, rfqautonotransaksi, rfqnotransaksi, rfqtgl"
            sql &= " FROM M4_rfq"
            sql &= " WHERE rfqid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("rfqcabang")
                lokasi = dtNomorNext.Rows(0)("rfqlokasi")
                sumber = dtNomorNext.Rows(0)("rfqsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("rfqautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("rfqnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("rfqtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M4_rfq_Detail WHERE idrfq = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M4_rfq WHERE rfqid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_RfqSearch(PostWsSearch(paramSplit(0), "M4_RfqSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_RfqSearch(ByVal param As String) As String
        'M4_RfqSearch --------------------------------------------------------
        'rfqid, rfqcabang, rfqlokasi, rfqsumber, rfqnotransaksi, rfqtgl, rfquraian, 
        'rfqcatatan, rfqstatus, rfqstatussebelumnya, rfqinputuser, rfqinputtgl, rfqmodifikasiuser, rfqmodifikasitgl, 
        'rfqcabangnama, rfqlokasinama, rfqstatusnama, rfqstatussebelumnyanama, rfqinputusernama, rfqmodifikasiusernama,
        'rfqidpr, rfqnotransaksipr, rfqtglawal, rfqtglakhir

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
            'Filter = Filter.Replace("posupplierkode", "c1.kkode")
            'Filter = Filter.Replace("posuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        sql = "select rfq.rfqid AS rfqid, rfq.rfqtglawal, rfq.rfqtglakhir , rfq.rfqcabang AS rfqcabang, rfq.rfqlokasi AS rfqlokasi, rfq.rfqsumber AS rfqsumber, rfq.rfqnotransaksi AS rfqnotransaksi, rfq.rfqtgl AS rfqtgl, rfq.rfquraian AS rfquraian, rfq.rfqcatatan AS rfqcatatan, rfq.rfqstatus AS rfqstatus, rfq.rfqstatussebelumnya AS rfqstatussebelumnya, rfq.rfqinputuser AS rfqinputuser, rfq.rfqinputtgl AS rfqinputtgl, rfq.rfqmodifikasiuser AS rfqmodifikasiuser, rfq.rfqmodifikasitgl AS rfqmodifikasitgl, br.bnama AS rfqcabangnama, lc.lnama AS rfqlokasinama, st1.nama AS rfqstatusnama, st2.nama AS rfqstatussebelumnyanama, u1.unama AS rfqinputusernama, u2.unama AS rfqmodifikasiusernama, rfq.rfqidpr, pr.prnotransaksi as rfqnotransaksipr from m4_rfq rfq join m1_branch br on rfq.rfqcabang = br.bkode join m1_location lc on rfq.rfqlokasi = lc.lkode join m0_status st1 on rfq.rfqstatus = st1.kode join m0_status st2 on rfq.rfqstatussebelumnya = st2.kode join m0_user u1 on rfq.rfqinputuser = u1.userid left join m0_user u2 on rfq.rfqmodifikasiuser = u2.userid left join m4_pr pr on rfq.rfqidpr = pr.prid"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Po", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rfqid"), ""), sptField,
                     FxDB(dr("rfqcabang"), ""), sptField,
                     FxDB(dr("rfqlokasi"), ""), sptField,
                     FxDB(dr("rfqsumber"), ""), sptField,
                     FxDB(dr("rfqnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rfqtgl"), ""), formatTgl), sptField,
                     FxDB(dr("rfquraian"), ""), sptField,
                     FxDB(dr("rfqcatatan"), ""), sptField,
                     FxDB(dr("rfqstatus"), 0), sptField,
                     FxDB(dr("rfqstatussebelumnya"), 0), sptField,
                     FxDB(dr("rfqinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rfqinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rfqmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rfqmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rfqcabangnama"), ""), sptField,
                     FxDB(dr("rfqlokasinama"), ""), sptField,
                     FxDB(dr("rfqstatusnama"), ""), sptField,
                     FxDB(dr("rfqstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("rfqinputusernama"), ""), sptField,
                     FxDB(dr("rfqmodifikasiusernama"), ""), sptField,
                     FxDB(dr("rfqidpr"), 0), sptField,
                     FxDB(dr("rfqnotransaksipr"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rfqtglawal"), ""), formatTglWaktu), sptField,
                     AsFormatTanggal(FxDB(dr("rfqtglakhir"), ""), formatTglWaktu), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rfqid, rfqcabang, rfqlokasi, rfqsumber, rfqnotransaksi, rfqtgl, rfquraian, rfqcatatan, rfqstatus, rfqstatussebelumnya, rfqinputuser, rfqinputtgl, rfqmodifikasiuser, rfqmodifikasitgl, rfqcabangnama, rfqlokasinama, rfqstatusnama, rfqstatussebelumnyanama, rfqinputusernama, rfqmodifikasiusernama, rfqidpr, rfqnotransaksipr, rfqtglawal, rfqtglakhir"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_RfqGetdataById(ByVal param As String) As String

        'M4_RfqGetdataById Utama --------------------------------------------------------
        'rfqid, rfqcabang, rfqlokasi, rfqsumber, rfqautonotransaksi, rfqnotransaksi, rfqtgl, 
        'rfqkodepa, rfqidpr, rfqkontakperson, rfq1alamat1, rfq1alamat2, rfq1alamat3, rfq2alamat1, 
        'rfq2alamat2, rfq2alamat3, rfquraian, rfqcatatan, rfqnoref, rfqtglnoref, rfqstatus, 
        'rfqstatussebelumnya, rfqjmlrevisi, rfqcetakanke, rfqinputuser, rfqinputtgl, rfqmodifikasiuser, rfqmodifikasitgl, 
        'rfqposting, rfqpostingtgl, rfqisclose, rfqcustomtext1, rfqcustomtext2, rfqcustomtext3, rfqcustomtext4, 
        'rfqcustomtext5, rfqcustomint1, rfqcustomint2, rfqcustomint3, rfqcustomdbl1, rfqcustomdbl2, rfqcustomdbl3, 
        'rfqcustomdate1, rfqcustomdate2, rfqcustomdate3, rfqnotransaksipr, rfqtglawal, rfqtglakhir

        'M4_RfqGetdataById Detail -------------------------------------------------------
        'idrfqdetail, idrfq, sumber, idkontak, 
        'catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3, kodekontak, namakontak

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

        Dim NmMemcached As String = "aplikasi1-M4_Pr~M4_Pr_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "rfqid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "rfqid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select rfq.rfqid AS rfqid, rfq.rfqtglawal, rfq.rfqtglakhir, rfq.rfqcabang AS rfqcabang, rfq.rfqlokasi AS rfqlokasi, rfq.rfqsumber AS rfqsumber, rfq.rfqautonotransaksi AS rfqautonotransaksi, rfq.rfqnotransaksi AS rfqnotransaksi, rfq.rfqtgl AS rfqtgl, rfq.rfqkodepa AS rfqkodepa, rfq.rfqidpr AS rfqidpr, rfq.rfqkontakperson AS rfqkontakperson, rfq.rfq1alamat1 AS rfq1alamat1, rfq.rfq1alamat2 AS rfq1alamat2, rfq.rfq1alamat3 AS rfq1alamat3, rfq.rfq2alamat1 AS rfq2alamat1, rfq.rfq2alamat2 AS rfq2alamat2, rfq.rfq2alamat3 AS rfq2alamat3, rfq.rfquraian AS rfquraian, rfq.rfqcatatan AS rfqcatatan, rfq.rfqnoref AS rfqnoref, rfq.rfqtglnoref AS rfqtglnoref, rfq.rfqstatus AS rfqstatus, rfq.rfqstatussebelumnya AS rfqstatussebelumnya, rfq.rfqjmlrevisi AS rfqjmlrevisi, rfq.rfqcetakanke AS rfqcetakanke, rfq.rfqinputuser AS rfqinputuser, rfq.rfqinputtgl AS rfqinputtgl, rfq.rfqmodifikasiuser AS rfqmodifikasiuser, rfq.rfqmodifikasitgl AS rfqmodifikasitgl, rfq.rfqposting AS rfqposting, rfq.rfqpostingtgl AS rfqpostingtgl, rfq.rfqisclose AS rfqisclose, rfq.rfqcustomtext1 AS rfqcustomtext1, rfq.rfqcustomtext2 AS rfqcustomtext2, rfq.rfqcustomtext3 AS rfqcustomtext3, rfq.rfqcustomtext4 AS rfqcustomtext4, rfq.rfqcustomtext5 AS rfqcustomtext5, rfq.rfqcustomint1 AS rfqcustomint1, rfq.rfqcustomint2 AS rfqcustomint2, rfq.rfqcustomint3 AS rfqcustomint3, rfq.rfqcustomdbl1 AS rfqcustomdbl1, rfq.rfqcustomdbl2 AS rfqcustomdbl2, rfq.rfqcustomdbl3 AS rfqcustomdbl3, rfq.rfqcustomdate1 AS rfqcustomdate1, rfq.rfqcustomdate2 AS rfqcustomdate2, rfq.rfqcustomdate3 AS rfqcustomdate3, pr.prnotransaksi as rfqnotransaksipr, rfqd.idrfqdetail AS idrfqdetail, rfqd.idrfq AS idrfq, rfqd.sumber AS sumber, rfqd.idkontak AS idkontak, rfqd.catatan AS catatan, rfqd.urutan AS urutan, rfqd.isclose AS isclose, rfqd.customtext1 AS customtext1, rfqd.customtext2 AS customtext2, rfqd.customtext3 AS customtext3, rfqd.customdbl1 AS customdbl1, rfqd.customdbl2 AS customdbl2, rfqd.customdbl3 AS customdbl3, rfqd.customdate1 AS customdate1, rfqd.customdate2 AS customdate2, rfqd.customdate3 AS customdate3, c.kkode as kodekontak, c.knama as namakontak from m4_rfq rfq join m4_rfq_detail rfqd on rfq.rfqid = rfqd.idrfq left join m4_pr pr on rfq.rfqidpr = pr.prid left join m1_contact c on rfqd.idkontak = c.kid"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                     FxDB(drutama("rfqid"), ""), sptField,
                     FxDB(drutama("rfqcabang"), ""), sptField,
                     FxDB(drutama("rfqlokasi"), ""), sptField,
                     FxDB(drutama("rfqsumber"), ""), sptField,
                     FxDB(drutama("rfqautonotransaksi"), 0), sptField,
                     FxDB(drutama("rfqnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rfqtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("rfqkodepa"), ""), sptField,
                     FxDB(drutama("rfqidpr"), ""), sptField,
                     FxDB(drutama("rfqkontakperson"), ""), sptField,
                     FxDB(drutama("rfq1alamat1"), ""), sptField,
                     FxDB(drutama("rfq1alamat2"), ""), sptField,
                     FxDB(drutama("rfq1alamat3"), ""), sptField,
                     FxDB(drutama("rfq2alamat1"), ""), sptField,
                     FxDB(drutama("rfq2alamat2"), ""), sptField,
                     FxDB(drutama("rfq2alamat3"), ""), sptField,
                     FxDB(drutama("rfquraian"), ""), sptField,
                     FxDB(drutama("rfqcatatan"), ""), sptField,
                     FxDB(drutama("rfqnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rfqtglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("rfqstatus"), 0), sptField,
                     FxDB(drutama("rfqstatussebelumnya"), 0), sptField,
                     FxDB(drutama("rfqjmlrevisi"), 0), sptField,
                     FxDB(drutama("rfqcetakanke"), 0), sptField,
                     FxDB(drutama("rfqinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rfqinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rfqmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rfqmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rfqposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rfqpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rfqisclose"), 0), sptField,
                     FxDB(drutama("rfqcustomtext1"), ""), sptField,
                     FxDB(drutama("rfqcustomtext2"), ""), sptField,
                     FxDB(drutama("rfqcustomtext3"), ""), sptField,
                     FxDB(drutama("rfqcustomtext4"), ""), sptField,
                     FxDB(drutama("rfqcustomtext5"), ""), sptField,
                     FxDB(drutama("rfqcustomint1"), 0), sptField,
                     FxDB(drutama("rfqcustomint2"), 0), sptField,
                     FxDB(drutama("rfqcustomint3"), 0), sptField,
                     FxDB(drutama("rfqcustomdbl1"), 0), sptField,
                     FxDB(drutama("rfqcustomdbl2"), 0), sptField,
                     FxDB(drutama("rfqcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rfqcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rfqcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rfqcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("rfqnotransaksipr"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rfqtglawal"), ""), formatTglWaktu), sptField,
                     AsFormatTanggal(FxDB(drutama("rfqtglakhir"), ""), formatTglWaktu))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idrfqdetail"), ""), sptField,
                     FxDB(dr("idrfq"), ""), sptField,
                     FxDB(dr("sumber"), ""), sptField,
                     FxDB(dr("idkontak"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
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
                     FxDB(dr("kodekontak"), ""), sptField,
                     FxDB(dr("namakontak"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, detail)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rfqid, rfqcabang, rfqlokasi, rfqsumber, rfqautonotransaksi, rfqnotransaksi, rfqtgl, rfqkodepa, rfqidpr, rfqkontakperson, rfq1alamat1, rfq1alamat2, rfq1alamat3, rfq2alamat1, rfq2alamat2, rfq2alamat3, rfquraian, rfqcatatan, rfqnoref, rfqtglnoref, rfqstatus, rfqstatussebelumnya, rfqjmlrevisi, rfqcetakanke, rfqinputuser, rfqinputtgl, rfqmodifikasiuser, rfqmodifikasitgl, rfqposting, rfqpostingtgl, rfqisclose, rfqcustomtext1, rfqcustomtext2, rfqcustomtext3, rfqcustomtext4, rfqcustomtext5, rfqcustomint1, rfqcustomint2, rfqcustomint3, rfqcustomdbl1, rfqcustomdbl2, rfqcustomdbl3, rfqcustomdate1, rfqcustomdate2, rfqcustomdate3, rfqnotransaksipr, rfqtglawal, rfqtglakhir" & sptSubParam & "idrfqdetail, idrfq, sumber, idkontak, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodekontak, namakontak"))

        Return wsResult
    End Function

   
End Class
