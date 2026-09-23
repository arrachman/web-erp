Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_pie
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_PieSimpan(ByVal param As String) As String
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
        'pieid(0) As , piecabang(1) As String, pielokasi(2) As String, piesumber(3) As String, pieautonotransaksi(4) As Integer, 
        'pienotransaksi(5) As String, pietgl(6) As Date, piekodepa(7) As , piekontak(8) As , piekontakperson(9) As String, 
        'pie1alamat1(10) As String, pie1alamat2(11) As String, pie1alamat3(12) As String, pie2alamat1(13) As String, pie2alamat2(14) As String, 
        'pie2alamat3(15) As String, pieuraian(16) As String, piecatatan(17) As String, pienoref(18) As String, pietglnoref(19) As Date, 
        'piestatus(20) As Integer, piestatussebelumnya(21) As Integer, piejmlrevisi(22) As Integer, piecetakanke(23) As Integer, pieinputuser(24) As , 
        'pieinputtgl(25) As DateTime, piemodifikasiuser(26) As , piemodifikasitgl(27) As DateTime, pieposting(28) As Integer, piepostingtgl(29) As DateTime, 
        'pieisclose(30) As Integer, piecustomtext1(31) As String, piecustomtext2(32) As String, piecustomtext3(33) As String, piecustomtext4(34) As String, 
        'piecustomtext5(35) As String, piecustomint1(36) As Integer, piecustomint2(37) As Integer, piecustomint3(38) As Integer, piecustomdbl1(39) As Double, 
        'piecustomdbl2(40) As Double, piecustomdbl3(41) As Double, piecustomdate1(42) As Date, piecustomdate2(43) As Date, piecustomdate3(44) As Date


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'pieid, piecabang, pielokasi, piesumber, pieautonotransaksi, pienotransaksi, pietgl, 
        'piekodepa, piekontak, piekontakperson, pie1alamat1, pie1alamat2, pie1alamat3, pie2alamat1, 
        'pie2alamat2, pie2alamat3, pieuraian, piecatatan, pienoref, pietglnoref, piestatus, 
        'piestatussebelumnya, piejmlrevisi, piecetakanke, pieinputuser, pieinputtgl, piemodifikasiuser, piemodifikasitgl, 
        'pieposting, piepostingtgl, pieisclose, piecustomtext1, piecustomtext2, piecustomtext3, piecustomtext4, 
        'piecustomtext5, piecustomint1, piecustomint2, piecustomint3, piecustomdbl1, piecustomdbl2, piecustomdbl3, 
        'piecustomdate1, piecustomdate2, piecustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 45) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'pieautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "pieautonotransaksi required numeric." : GoTo selesai
        End If
        'pietgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "pietgl required date." : GoTo selesai
        End If
        'pietglnoref(19) As Date
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "pietglnoref required date." : GoTo selesai
        End If
        'piestatus(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "piestatus required numeric." : GoTo selesai
        End If
        'piestatussebelumnya(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "piestatussebelumnya required numeric." : GoTo selesai
        End If
        'piejmlrevisi(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "piejmlrevisi required numeric." : GoTo selesai
        End If
        'piecetakanke(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "piecetakanke required numeric." : GoTo selesai
        End If
        'pieinputtgl(25) As DateTime
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "pieinputtgl required date." : GoTo selesai
        End If
        'piemodifikasitgl(27) As DateTime
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "piemodifikasitgl required date." : GoTo selesai
        End If
        'pieposting(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "pieposting required numeric." : GoTo selesai
        End If
        'piepostingtgl(29) As DateTime
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "piepostingtgl required date." : GoTo selesai
        End If
        'pieisclose(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "pieisclose required numeric." : GoTo selesai
        End If
        'piecustomint1(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "piecustomint1 required numeric." : GoTo selesai
        End If
        'piecustomint2(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "piecustomint2 required numeric." : GoTo selesai
        End If
        'piecustomint3(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "piecustomint3 required numeric." : GoTo selesai
        End If
        'piecustomdbl1(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "piecustomdbl1 required numeric." : GoTo selesai
        End If
        'piecustomdbl2(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "piecustomdbl2 required numeric." : GoTo selesai
        End If
        'piecustomdbl3(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "piecustomdbl3 required numeric." : GoTo selesai
        End If
        'piecustomdate1(42) As Date
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "piecustomdate1 required date." : GoTo selesai
        End If
        'piecustomdate2(43) As Date
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "piecustomdate2 required date." : GoTo selesai
        End If
        'piecustomdate3(44) As Date
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "piecustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'pieid(0) As 
        If Len(dataUtama(0)) = 0 Then
            result(2) = "pieid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 20 Then
            result(2) = "pieid should not be more than 20 character." : GoTo selesai
        End If

        'piecabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "piecabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "piecabang should not be more than 25 character." : GoTo selesai
        End If

        'pielokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "pielokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "pielokasi should not be more than 25 character." : GoTo selesai
        End If

        'piesumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "piesumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "piesumber should not be more than 10 character." : GoTo selesai
        End If

        'pienotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "pienotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "pienotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'pietgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "pietgl can't be empty" : GoTo selesai
        End If

        'piekodepa(7) As 
        If Len(dataUtama(7)) = 0 Then
            result(2) = "piekodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 20 Then
            result(2) = "piekodepa should not be more than 20 character." : GoTo selesai
        End If

        'piekontak(8) As 
        If Len(dataUtama(8)) = 0 Then
            result(2) = "piekontak can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 20 Then
            result(2) = "piekontak should not be more than 20 character." : GoTo selesai
        End If

        'pietglnoref(19) As Date
        If Len(dataUtama(19)) = 0 Then
            result(2) = "pietglnoref can't be empty" : GoTo selesai
        End If

        'pieinputtgl(25) As DateTime
        If Len(dataUtama(25)) = 0 Then
            result(2) = "pieinputtgl can't be empty" : GoTo selesai
        End If

        'piemodifikasitgl(27) As DateTime
        If Len(dataUtama(27)) = 0 Then
            result(2) = "piemodifikasitgl can't be empty" : GoTo selesai
        End If

        'piepostingtgl(29) As DateTime
        If Len(dataUtama(29)) = 0 Then
            result(2) = "piepostingtgl can't be empty" : GoTo selesai
        End If

        'piecustomdbl1(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "piecustomdbl1 can't be empty" : GoTo selesai
        End If

        'piecustomdbl2(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "piecustomdbl2 can't be empty" : GoTo selesai
        End If

        'piecustomdbl3(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "piecustomdbl3 can't be empty" : GoTo selesai
        End If

        'piecustomdate1(42) As Date
        If Len(dataUtama(42)) = 0 Then
            result(2) = "piecustomdate1 can't be empty" : GoTo selesai
        End If

        'piecustomdate2(43) As Date
        If Len(dataUtama(43)) = 0 Then
            result(2) = "piecustomdate2 can't be empty" : GoTo selesai
        End If

        'piecustomdate3(44) As Date
        If Len(dataUtama(44)) = 0 Then
            result(2) = "piecustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "pieid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piecabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pielokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piesumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pieautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pienotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pietgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piekodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piekontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piekontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pie1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pie1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pie1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pie2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pie2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pie2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pieuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piecatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pienoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pietglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piestatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piestatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piejmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piecetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pieinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pieinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piemodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piemodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pieposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piepostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pieisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piecustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piecustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piecustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piecustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piecustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piecustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piecustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piecustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piecustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piecustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piecustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piecustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piecustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piecustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahData(dtutama, "pieid~piecabang~pielokasi~piesumber~pieautonotransaksi~pienotransaksi~pietgl~piekodepa~piekontak~piekontakperson~pie1alamat1~pie1alamat2~pie1alamat3~pie2alamat1~pie2alamat2~pie2alamat3~pieuraian~piecatatan~pienoref~pietglnoref~piestatus~piestatussebelumnya~piejmlrevisi~piecetakanke~pieinputuser~pieinputtgl~piemodifikasiuser~piemodifikasitgl~pieposting~piepostingtgl~pieisclose~piecustomtext1~piecustomtext2~piecustomtext3~piecustomtext4~piecustomtext5~piecustomint1~piecustomint2~piecustomint3~piecustomdbl1~piecustomdbl2~piecustomdbl3~piecustomdate1~piecustomdate2~piecustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44))

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idpiedetail(0) As , idpie(1) As , sumber(2) As String, idtransaksi(3) As , catatan(4) As String, 
        'urutan(5) As Integer, isclose(6) As Integer, customtext1(7) As String, customtext2(8) As String, customtext3(9) As String, 
        'customdbl1(10) As Double, customdbl2(11) As Double, customdbl3(12) As Double, customdate1(13) As Date, customdate2(14) As Date, 
        'customdate3(15) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idpiedetail, idpie, sumber, idtransaksi, catatan, urutan, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idpiedetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpie", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idtransaksi", AsEnumTypeData.AsInt64)
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


        'VARIABEL VALIDASI OUTSTANDING
        Dim sumberDetail As String = "", idtransaksiDetail As Integer = 0
        Dim ftExistRI As String = "", ftBelumPieRI As String = ""
        Dim ftExistPRT As String = "", ftBelumPiePRT As String = ""


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
            'idpiedetail(0) As 
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idpiedetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idpiedetail should not be more than 20 character." : GoTo selesai
            End If

            'idpie(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idpie can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idpie should not be more than 20 character." : GoTo selesai
            End If

            'sumber(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - sumber can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 10 Then
                result(2) = "Row : " & i & " - sumber should not be more than 10 character." : GoTo selesai
            End If

            'idtransaksi(3) As 
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - idtransaksi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 20 Then
                result(2) = "Row : " & i & " - idtransaksi should not be more than 20 character." : GoTo selesai
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

            AsDataTableTambahData(dtdetail, "idpiedetail~idpie~sumber~idtransaksi~catatan~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15))


            'VALIDASI OUTSTANDING ---------------------------------------
            'SET SUMBER DAN IDTRANSAKSI
            'sumber(2) As String,               idtransaksi(3) As
            sumberDetail = dataRowDetail(2) : idtransaksiDetail = dataRowDetail(3)

            Select Case sumberDetail
                Case "RI"
                    'CEK DATA EXIST
                    ftExistRI = IIf(Len(ftExistRI.ToString) = 0, "", ftExistRI & " UNION ")
                    ftExistRI = String.Concat(ftExistRI, "SELECT EXISTS(SELECT 1 FROM m4_ri WHERE riid = '" & idtransaksiDetail & "' AND ristatus IN(2,3,4,7) LIMIT 1) as rowExists, riid, risumber, rinotransaksi FROM m4_ri WHERE riid = '" & idtransaksiDetail & "'")

                    'CEK OUTSTANDING
                    ftBelumPieRI = IIf(Len(ftBelumPieRI.ToString) = 0, "", ftBelumPieRI & " OR ")
                    ftBelumPieRI = String.Concat(ftBelumPieRI, " (pied.sumber = '" & FixQuotes(sumberDetail) & "' AND pied.idtransaksi = '" & FixDouble(idtransaksiDetail) & "') ")

                Case "PRT"
                    'CEK DATA EXIST
                    ftExistPRT = IIf(Len(ftExistPRT.ToString) = 0, "", ftExistPRT & " UNION ")
                    ftExistPRT = String.Concat(ftExistPRT, "SELECT EXISTS(SELECT 1 FROM m4_prt WHERE prtid = '" & idtransaksiDetail & "' AND prtstatus IN(2,3,4,7) LIMIT 1) as rowExists, prtid, prtsumber, prtnotransaksi FROM m4_prt WHERE prtid = '" & idtransaksiDetail & "'")

                    'CEK OUTSTANDING
                    ftBelumPiePRT = IIf(Len(ftBelumPiePRT.ToString) = 0, "", ftBelumPiePRT & " OR ")
                    ftBelumPiePRT = String.Concat(ftBelumPiePRT, " (pied.sumber = '" & FixQuotes(sumberDetail) & "' AND pied.idtransaksi = '" & FixDouble(idtransaksiDetail) & "') ")
            End Select
            'END OF VALIDASI OUTSTANDING --------------------------------

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
                Dim vModuleId As Integer = 4, vMenuId As Integer = 56
                Select Case drutama("piestatus")
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("pietgl")), AsFormatTanggal(drutama("pietgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================

                'VALIDASI SIMPAN =========================================
                If drutama("piestatus") = 2 Or drutama("piestatus") = 1 Or drutama("piestatus") = 8 Or drutama("piestatus") = 9 Or drutama("piestatus") = 10 Or drutama("piestatus") = 11 Then
                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistRI, ftBelumPieRI, ftExistPRT, ftBelumPiePRT)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN ==================================

                If isUpdate Then
                    result(4) = drutama("pieid")
                    notransaksi = drutama("pienotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(pieid), pienotransaksi FROM M4_pie WHERE pieid='" & result(4) & "' AND piestatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("pieautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("piecabang"), drutama("pielokasi"), drutama("piesumber"), drutama("pietgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(pieid) FROM m4_pie WHERE pienotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_pie_history
                        Dim rsSimpanHistory As String = SimpanHistory.M4_Pie_HistorySimpan("" & paramSplit(0) & "★M4_Pie_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("piesumber")) & "▼" & FixQuotes(drutama("pieid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M4_Pie set piecabang  = '" & FixQuotes(drutama("piecabang")) & "', pielokasi  = '" & FixQuotes(drutama("pielokasi")) & "', piesumber  = '" & FixQuotes(drutama("piesumber")) & "', pieautonotransaksi  = " & drutama("pieautonotransaksi") & ", pienotransaksi  = '" & FixQuotes(notransaksi) & "', pietgl  = '" & FixQuotes(AsFormatTanggal(drutama("pietgl"))) & "', piekodepa  = '" & FixQuotes(drutama("piekodepa")) & "', piekontak  = '" & FixQuotes(drutama("piekontak")) & "', piekontakperson  = '" & FixQuotes(drutama("piekontakperson")) & "', pie1alamat1  = '" & FixQuotes(drutama("pie1alamat1")) & "', pie1alamat2  = '" & FixQuotes(drutama("pie1alamat2")) & "', pie1alamat3  = '" & FixQuotes(drutama("pie1alamat3")) & "', pie2alamat1  = '" & FixQuotes(drutama("pie2alamat1")) & "', pie2alamat2  = '" & FixQuotes(drutama("pie2alamat2")) & "', pie2alamat3  = '" & FixQuotes(drutama("pie2alamat3")) & "', pieuraian  = '" & FixQuotes(drutama("pieuraian")) & "', piecatatan  = '" & FixQuotes(drutama("piecatatan")) & "', pienoref  = '" & FixQuotes(drutama("pienoref")) & "', pietglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("pietglnoref"))) & "', piestatus  = " & drutama("piestatus") & ", piestatussebelumnya  = " & drutama("piestatussebelumnya") & ", piejmlrevisi  = " & drutama("piejmlrevisi") & ", piecetakanke  = " & drutama("piecetakanke") & ", pieinputuser  = '" & FixQuotes(drutama("pieinputuser")) & "', pieinputtgl  = '" & FixQuotes(AsFormatTanggal(drutama("pieinputtgl"), "yyyy-MM-dd HH:mm:ss")) & "', piemodifikasiuser  = '" & FixQuotes(drutama("piemodifikasiuser")) & "', piemodifikasitgl  = NOW(), pieposting  = " & drutama("pieposting") & ", piepostingtgl  = '" & FixQuotes(AsFormatTanggal(drutama("piepostingtgl"), "yyyy-MM-dd HH:mm:ss")) & "', piecustomtext1  = '" & FixQuotes(drutama("piecustomtext1")) & "', piecustomtext2  = '" & FixQuotes(drutama("piecustomtext2")) & "', piecustomtext3  = '" & FixQuotes(drutama("piecustomtext3")) & "', piecustomtext4  = '" & FixQuotes(drutama("piecustomtext4")) & "', piecustomtext5  = '" & FixQuotes(drutama("piecustomtext5")) & "', piecustomint1  = " & drutama("piecustomint1") & ", piecustomint2  = " & drutama("piecustomint2") & ", piecustomint3  = " & drutama("piecustomint3") & ", piecustomdbl1  = '" & FixDouble(drutama("piecustomdbl1")) & "', piecustomdbl2  = '" & FixDouble(drutama("piecustomdbl2")) & "', piecustomdbl3  = '" & FixDouble(drutama("piecustomdbl3")) & "', piecustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("piecustomdate1"))) & "', piecustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("piecustomdate2"))) & "', piecustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("piecustomdate3"))) & "' where pieid = " & drutama("pieid") & ""
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

                    If drutama("pieautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("piecabang"), drutama("pielokasi"), drutama("piesumber"), drutama("pietgl"))
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
                        notransaksi = drutama("pienotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(pieid) FROM m4_pie WHERE pienotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Pie (piecabang, pielokasi, piesumber, pieautonotransaksi, pienotransaksi, pietgl, piekodepa, piekontak, piekontakperson, pie1alamat1, pie1alamat2, pie1alamat3, pie2alamat1, pie2alamat2, pie2alamat3, pieuraian, piecatatan, pienoref, pietglnoref, piestatus, piestatussebelumnya, piejmlrevisi, piecetakanke, pieinputuser, pieinputtgl, piemodifikasiuser, piemodifikasitgl, pieposting, piepostingtgl, pieisclose, piecustomtext1, piecustomtext2, piecustomtext3, piecustomtext4, piecustomtext5, piecustomint1, piecustomint2, piecustomint3, piecustomdbl1, piecustomdbl2, piecustomdbl3, piecustomdate1, piecustomdate2, piecustomdate3) values('" & FixQuotes(drutama("piecabang")) & "', '" & FixQuotes(drutama("pielokasi")) & "', '" & FixQuotes(drutama("piesumber")) & "', " & drutama("pieautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("pietgl"))) & "', '" & FixQuotes(drutama("piekodepa")) & "', '" & FixQuotes(drutama("piekontak")) & "', '" & FixQuotes(drutama("piekontakperson")) & "', '" & FixQuotes(drutama("pie1alamat1")) & "', '" & FixQuotes(drutama("pie1alamat2")) & "', '" & FixQuotes(drutama("pie1alamat3")) & "', '" & FixQuotes(drutama("pie2alamat1")) & "', '" & FixQuotes(drutama("pie2alamat2")) & "', '" & FixQuotes(drutama("pie2alamat3")) & "', '" & FixQuotes(drutama("pieuraian")) & "', '" & FixQuotes(drutama("piecatatan")) & "', '" & FixQuotes(drutama("pienoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pietglnoref"))) & "', " & drutama("piestatus") & ", " & drutama("piestatussebelumnya") & ", " & drutama("piejmlrevisi") & ", " & drutama("piecetakanke") & ", '" & FixQuotes(drutama("pieinputuser")) & "', NOW(), '" & FixQuotes(drutama("piemodifikasiuser")) & "', NOW(), " & drutama("pieposting") & ", '" & FixQuotes(AsFormatTanggal(drutama("piepostingtgl"), "yyyy-MM-dd HH:mm:ss")) & "', " & drutama("pieisclose") & ", '" & FixQuotes(drutama("piecustomtext1")) & "', '" & FixQuotes(drutama("piecustomtext2")) & "', '" & FixQuotes(drutama("piecustomtext3")) & "', '" & FixQuotes(drutama("piecustomtext4")) & "', '" & FixQuotes(drutama("piecustomtext5")) & "', " & drutama("piecustomint1") & ", " & drutama("piecustomint2") & ", " & drutama("piecustomint3") & ", '" & FixDouble(drutama("piecustomdbl1")) & "', '" & FixDouble(drutama("piecustomdbl2")) & "', '" & FixDouble(drutama("piecustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("piecustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("piecustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("piecustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select pieid from M4_pie where pienotransaksi='" & notransaksi & "' AND pieinputuser= '" & userid & "' order by piemodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Pie_Detail where idpie = '" & result(4) & "'"
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
                        strValue2.Append("('" & FixQuotes(dr1("idpiedetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("sumber")) & "', '" & FixQuotes(dr1("idtransaksi")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Pie_Detail(idpiedetail, idpie, sumber, idtransaksi, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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


                'UPDATE OUTSTANDING TRANSAKSI ==========================================================
                If drutama("piestatus") = 2 Then
                    'UPDATE M4 RI (SET STATUSPIE DAN TGL JATUH TEMPO SESUAI SETTING BERDASARKAN TERMIN DAN TGL PIE)
                    sql = "UPDATE m4_pie pie JOIN m4_pie_detail pied ON pie.pieid = pied.idpie JOIN m4_ri ri ON pied.sumber = ri.risumber AND pied.idtransaksi = ri.riid LEFT JOIN m0_setting s ON s.smodule = 4 AND s.sgrup = 'tukarfaktur' AND s.skode = 'UpdateTglJatuhTempoRI' AND s.snilai = 1 LEFT JOIN m1_terms tr ON ri.ritermin = tr.trkode SET ri.ristatuspie = 1, ri.ritglpie = pie.pietgl, ri.ritgljatuhtempo = (CASE IFNULL(s.snilai,0) WHEN 1 THEN DATE_ADD(pie.pietgl,INTERVAL IFNULL(tr.trharijatuhtempo,0) DAY) ELSE ri.ritgljatuhtempo END) WHERE pie.pieid = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE M4 PRT (SET STATUSPIE DAN TGL JATUH TEMPO SESUAI SETTING BERDASARKAN TERMIN DAN TGL PIE)
                    sql = "UPDATE m4_pie pie JOIN m4_pie_detail pied ON pie.pieid = pied.idpie JOIN m4_prt prt ON pied.sumber = prt.prtsumber AND pied.idtransaksi = prt.prtid LEFT JOIN m0_setting s ON s.smodule = 4 AND s.sgrup = 'tukarfaktur' AND s.skode = 'UpdateTglJatuhTempoPRT' AND s.snilai = 1 LEFT JOIN m1_terms tr ON prt.prttermin = tr.trkode SET prt.prtstatuspie = 1, prt.prttglpie = pie.pietgl, prt.prttgljatuhtempo = (CASE IFNULL(s.snilai,0) WHEN 1 THEN DATE_ADD(pie.pietgl,INTERVAL IFNULL(tr.trharijatuhtempo,0) DAY) ELSE prt.prttgljatuhtempo END) WHERE pie.pieid = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE OUTSTANDING TRANSAKSI ===================================================


                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "PIE", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                'If drutama("piestatus") = 2 Then
                '    Dim Security As New ClsSecurity, mjid As String = "", hasilMsmq As String = ""
                '    'BUAT ID UNIQUE
                '    mjid = Security.MD5CalcString(userid & sumber & result(4) & Now) 'RandomId.Generate(15)

                '    'MSMQ TABEL
                '    'sql = "Insert into M0_Msmq_Journal(mjid, mjsumber, mjidtransaksi, mjprogress, mjpesan, mjtglantrian, mjtglselesai, mjuserid) values ('" _
                '    '    & mjid & "', '" & sumber & "', '" & result(4) & "', '" & 0 & "', " & "''" & ", NOW(), '1971-01-01 00:00:00', '" & userid & "')"
                '    'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '    'With objCmd
                '    '    .Connection = myconn
                '    '    .Transaction = Trans
                '    '    .CommandType = CommandType.Text
                '    '    .CommandText = sql
                '    'End With
                '    'objCmd.ExecuteNonQuery()

                '    'MSMQ ANTRIAN
                '    'Dim PostingJurnal As String = F_getSetting(0, "accounting", "AutoPosting")
                '    'If PostingJurnal.Equals("0") = False Then
                '    '    hasilMsmq = SendMsmq(dirMsmq, "J", mjid, sumber, result(4), userid)
                '    '    If Len(hasilMsmq) > 0 Then
                '    '        result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
                '    '    End If
                '    'End If

                'End If
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
    Public Function M4_PieUpdateStatus(ByVal param As String) As String

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
            'Filter = Filter.Replace("piekontakkode", "c1.kkode")
            'Filter = Filter.Replace("piekontaknama", "c1.knama")
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
            Dim sumber As String = "Pie", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Pietgl, pienotransaksi, piestatus FROM m4_Pie WHERE Pieid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Piestatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m4_pie_history
            Dim rsSimpanHistory As String = SimpanHistory.M4_Pie_HistorySimpan("" & paramSplit(0) & "★M4_Pie_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                'Dim query As New m0_query
                'sql = query.PanggilQuery("m4_pie_terkait")
                'sql = sql.Replace("validtransaksi", idtransaksi)
                'Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql)
                'dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                'If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================


                'UPDATE OUTSTANDING TRANSAKSI ===================================================
                'UPDATE M4 RI (SET STATUSPIE DAN TGL JATUH TEMPO SESUAI SETTING BERDASARKAN TERMIN DAN TGL PIE)
                sql = "UPDATE m4_pie pie JOIN m4_pie_detail pied ON pie.pieid = pied.idpie JOIN m4_ri ri ON pied.sumber = ri.risumber AND pied.idtransaksi = ri.riid LEFT JOIN m0_setting s ON s.smodule = 4 AND s.sgrup = 'tukarfaktur' AND s.skode = 'UpdateTglJatuhTempoRI' AND s.snilai = 1 LEFT JOIN m1_terms tr ON ri.ritermin = tr.trkode SET ri.ristatuspie = 0, ri.ritglpie = '1900-01-01', ri.ritgljatuhtempo = (CASE IFNULL(s.snilai,0) WHEN 1 THEN '2100-12-31' ELSE ri.ritgljatuhtempo END) WHERE pie.pieid = '" & idtransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'UPDATE M4 PRT (SET STATUSPIE DAN TGL JATUH TEMPO SESUAI SETTING BERDASARKAN TERMIN DAN TGL PIE)
                sql = "UPDATE m4_pie pie JOIN m4_pie_detail pied ON pie.pieid = pied.idpie JOIN m4_prt prt ON pied.sumber = prt.prtsumber AND pied.idtransaksi = prt.prtid LEFT JOIN m0_setting s ON s.smodule = 4 AND s.sgrup = 'tukarfaktur' AND s.skode = 'UpdateTglJatuhTempoPRT' AND s.snilai = 1 LEFT JOIN m1_terms tr ON prt.prttermin = tr.trkode SET prt.prtstatuspie = 0, prt.prttglpie = '1900-01-01', prt.prttgljatuhtempo = (CASE IFNULL(s.snilai,0) WHEN 1 THEN '2100-12-31' ELSE prt.prttgljatuhtempo END) WHERE pie.pieid = '" & idtransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF UPDATE OUTSTANDING TRANSAKSI ============================================

            End If

            'update status utama
            sql = "UPDATE m4_Pie SET Piestatus = " & nilaiStatus & ", Piemodifikasiuser='" & userid & "', Piemodifikasitgl = NOW(), Pieposting = 0, Piepostingtgl = '1971-01-01 00:00:00', Piejmlrevisi = Piejmlrevisi + 1 WHERE Pieid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_PieSearch(PostWsSearch(paramSplit(0), "M4_PieSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_PieDelete(ByVal param As String) As String

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
            'Filter = Filter.Replace("piekontakkode", "c1.kkode")
            'Filter = Filter.Replace("piekontaknama", "c1.knama")
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
            Dim sumber As String = "Pie", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Pieid, Pienotransaksi FROM M4_Pie WHERE Pieid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT piecabang, pielokasi, piesumber, pieautonotransaksi, pienotransaksi, pietgl"
            sql &= " FROM M4_pie"
            sql &= " WHERE pieid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("piecabang")
                lokasi = dtNomorNext.Rows(0)("pielokasi")
                sumber = dtNomorNext.Rows(0)("piesumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("pieautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("pienotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("pietgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M4_pie_Detail WHERE idpie = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M4_pie WHERE pieid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_PieSearch(PostWsSearch(paramSplit(0), "M4_PieSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_PieSearch(ByVal param As String) As String
        'M4_PieSearch --------------------------------------------------------
        'pieid, piecabang, pielokasi, piesumber, pienotransaksi, pietgl, pieuraian, 
        'piecatatan, piestatus, piestatussebelumnya, pieinputuser, pieinputtgl, piemodifikasiuser, piemodifikasitgl, 
        'piecabangnama, pielokasinama, piestatusnama, piestatussebelumnyanama, pieinputusernama, piemodifikasiusernama

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
            Filter = Filter.Replace("piesupplierkode", "c.kkode")
            Filter = Filter.Replace("piesuppliernama", "c.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select `pie`.`pieid` AS `pieid`,`pie`.`piecabang` AS `piecabang`,`pie`.`pielokasi` AS `pielokasi`,`pie`.`piesumber` AS `piesumber`,`pie`.`pienotransaksi` AS `pienotransaksi`,`pie`.`pietgl` AS `pietgl`,`pie`.`pieuraian` AS `pieuraian`,`pie`.`piecatatan` AS `piecatatan`,`pie`.`piestatus` AS `piestatus`,`pie`.`piestatussebelumnya` AS `piestatussebelumnya`,`pie`.`pieinputuser` AS `pieinputuser`,`pie`.`pieinputtgl` AS `pieinputtgl`,`pie`.`piemodifikasiuser` AS `piemodifikasiuser`,`pie`.`piemodifikasitgl` AS `piemodifikasitgl`,`br`.`bnama` AS `piecabangnama`,`lc`.`lnama` AS `pielokasinama`,`st1`.`nama` AS `piestatusnama`,`st2`.`nama` AS `piestatussebelumnyanama`,`u1`.`unama` AS `pieinputusernama`,`u2`.`unama` AS `piemodifikasiusernama`,IFNULL(c.kkode,'') AS supplierkode, ifnull(c.knama,'') AS suppliernama from ((((((`m4_pie` `pie` join `m1_branch` `br` on((`pie`.`piecabang` = `br`.`bkode`))) join `m1_location` `lc` on((`pie`.`pielokasi` = `lc`.`lkode`))) join `m0_status` `st1` on((`pie`.`piestatus` = `st1`.`kode`))) join `m0_status` `st2` on((`pie`.`piestatussebelumnya` = `st2`.`kode`))) join `m0_user` `u1` on((`pie`.`pieinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`pie`.`piemodifikasiuser` = `u2`.`userid`))) LEFT JOIN m1_contact c ON c.kid = pie.piekontak"       'BUKA KONEKSI

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Po", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("pieid"), ""), sptField,
                     FxDB(dr("piecabang"), ""), sptField,
                     FxDB(dr("pielokasi"), ""), sptField,
                     FxDB(dr("piesumber"), ""), sptField,
                     FxDB(dr("pienotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pietgl"), ""), formatTgl), sptField,
                     FxDB(dr("pieuraian"), ""), sptField,
                     FxDB(dr("piecatatan"), ""), sptField,
                     FxDB(dr("piestatus"), 0), sptField,
                     FxDB(dr("piestatussebelumnya"), 0), sptField,
                     FxDB(dr("pieinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pieinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("piemodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("piemodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("piecabangnama"), ""), sptField,
                     FxDB(dr("pielokasinama"), ""), sptField,
                     FxDB(dr("piestatusnama"), ""), sptField,
                     FxDB(dr("piestatussebelumnyanama"), ""), sptField,
                     FxDB(dr("pieinputusernama"), ""), sptField,
                     FxDB(dr("piemodifikasiusernama"), ""), sptField,
                     FxDB(dr("supplierkode"), ""), sptField,
                     FxDB(dr("suppliernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pieid, piecabang, pielokasi, piesumber, pienotransaksi, pietgl, pieuraian, piecatatan, piestatus, piestatussebelumnya, pieinputuser, pieinputtgl, piemodifikasiuser, piemodifikasitgl, piecabangnama, pielokasinama, piestatusnama, piestatussebelumnyanama, pieinputusernama, piemodifikasiusernama, piesupplierkode, piesuppliernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_PieGetdataById(ByVal param As String) As String

        'M4_PieGetdataById Utama --------------------------------------------------------
        'pieid, piecabang, pielokasi, piesumber, pieautonotransaksi, pienotransaksi, pietgl, 
        'piekodepa, piekontak, piekontakperson, pie1alamat1, pie1alamat2, pie1alamat3, pie2alamat1, 
        'pie2alamat2, pie2alamat3, pieuraian, piecatatan, pienoref, pietglnoref, piestatus, 
        'piestatussebelumnya, piejmlrevisi, piecetakanke, pieinputuser, pieinputtgl, piemodifikasiuser, piemodifikasitgl, 
        'pieposting, piepostingtgl, pieisclose, piecustomtext1, piecustomtext2, piecustomtext3, piecustomtext4, 
        'piecustomtext5, piecustomint1, piecustomint2, piecustomint3, piecustomdbl1, piecustomdbl2, piecustomdbl3, 
        'piecustomdate1, piecustomdate2, piecustomdate3

        'M4_PieGetdataById Detail -------------------------------------------------------
        'idpiedetail, idpie, sumber, idtransaksi, 
        'catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3, cabang, lokasi, 
        'gudang, notransaksi, tgl, supplier, supplierkode, suppliernama, supplierkontak, 
        'termin, uraian, matauang, kurs, totaltransaksi, jmlbayar

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
            Filter = "pieid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "pieid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select `pie`.`pieid` AS `pieid`,`pie`.`piecabang` AS `piecabang`,`pie`.`pielokasi` AS `pielokasi`,`pie`.`piesumber` AS `piesumber`,`pie`.`pieautonotransaksi` AS `pieautonotransaksi`,`pie`.`pienotransaksi` AS `pienotransaksi`,`pie`.`pietgl` AS `pietgl`,`pie`.`piekodepa` AS `piekodepa`,`pie`.`piekontak` AS `piekontak`,`pie`.`piekontakperson` AS `piekontakperson`,`pie`.`pie1alamat1` AS `pie1alamat1`,`pie`.`pie1alamat2` AS `pie1alamat2`,`pie`.`pie1alamat3` AS `pie1alamat3`,`pie`.`pie2alamat1` AS `pie2alamat1`,`pie`.`pie2alamat2` AS `pie2alamat2`,`pie`.`pie2alamat3` AS `pie2alamat3`,`pie`.`pieuraian` AS `pieuraian`,`pie`.`piecatatan` AS `piecatatan`,`pie`.`pienoref` AS `pienoref`,`pie`.`pietglnoref` AS `pietglnoref`,`pie`.`piestatus` AS `piestatus`,`pie`.`piestatussebelumnya` AS `piestatussebelumnya`,`pie`.`piejmlrevisi` AS `piejmlrevisi`,`pie`.`piecetakanke` AS `piecetakanke`,`pie`.`pieinputuser` AS `pieinputuser`,`pie`.`pieinputtgl` AS `pieinputtgl`,`pie`.`piemodifikasiuser` AS `piemodifikasiuser`,`pie`.`piemodifikasitgl` AS `piemodifikasitgl`,`pie`.`pieposting` AS `pieposting`,`pie`.`piepostingtgl` AS `piepostingtgl`,`pie`.`pieisclose` AS `pieisclose`,`pie`.`piecustomtext1` AS `piecustomtext1`,`pie`.`piecustomtext2` AS `piecustomtext2`,`pie`.`piecustomtext3` AS `piecustomtext3`,`pie`.`piecustomtext4` AS `piecustomtext4`,`pie`.`piecustomtext5` AS `piecustomtext5`,`pie`.`piecustomint1` AS `piecustomint1`,`pie`.`piecustomint2` AS `piecustomint2`,`pie`.`piecustomint3` AS `piecustomint3`,`pie`.`piecustomdbl1` AS `piecustomdbl1`,`pie`.`piecustomdbl2` AS `piecustomdbl2`,`pie`.`piecustomdbl3` AS `piecustomdbl3`,`pie`.`piecustomdate1` AS `piecustomdate1`,`pie`.`piecustomdate2` AS `piecustomdate2`,`pie`.`piecustomdate3` AS `piecustomdate3`,`pied`.`idpiedetail` AS `idpiedetail`,`pied`.`idpie` AS `idpie`,`pied`.`sumber` AS `sumber`,`pied`.`idtransaksi` AS `idtransaksi`,`pied`.`catatan` AS `catatan`,`pied`.`urutan` AS `urutan`,`pied`.`isclose` AS `isclose`,`pied`.`customtext1` AS `customtext1`,`pied`.`customtext2` AS `customtext2`,`pied`.`customtext3` AS `customtext3`,`pied`.`customdbl1` AS `customdbl1`,`pied`.`customdbl2` AS `customdbl2`,`pied`.`customdbl3` AS `customdbl3`,`pied`.`customdate1` AS `customdate1`,`pied`.`customdate2` AS `customdate2`,`pied`.`customdate3` AS `customdate3`,ifnull(`ri`.`ricabang`,`prt`.`prtcabang`) AS `cabang`,ifnull(`ri`.`rilokasi`,`prt`.`prtlokasi`) AS `lokasi`,ifnull(`ri`.`rigudang`,`prt`.`prtgudang`) AS `gudang`,ifnull(`ri`.`rinotransaksi`,`prt`.`prtnotransaksi`) AS `notransaksi`,ifnull(`ri`.`ritgl`,`prt`.`prttgl`) AS `tgl`,ifnull(`ri`.`risupplier`,`prt`.`prtsupplier`) AS `supplier`,ifnull(`c`.`kkode`,'') AS `supplierkode`,ifnull(`c`.`knama`,'') AS `suppliernama`,ifnull(`ri`.`risupplierkontak`,`prt`.`prtsupplierkontak`) AS `supplierkontak`,ifnull(`ri`.`ritermin`,`prt`.`prttermin`) AS `termin`,ifnull(`ri`.`riuraian`,`prt`.`prturaian`) AS `uraian`,ifnull(`ri`.`rimatauang`,`prt`.`prtmatauang`) AS `matauang`,ifnull(`ri`.`rikurs`,`prt`.`prtkurs`) AS `kurs`,ifnull(`ri`.`ritotaltransaksi`,`prt`.`prttotaltransaksi`) AS `totaltransaksi`,ifnull(`ri`.`rijmlbayar`,`prt`.`prtjmlbayar`) AS `jmlbayar` from ((((`m4_pie` `pie` join `m4_pie_detail` `pied` on((`pie`.`pieid` = `pied`.`idpie`))) left join `m4_ri` `ri` on(((`pied`.`sumber` = `ri`.`risumber`) and (`pied`.`idtransaksi` = `ri`.`riid`)))) left join `m4_prt` `prt` on(((`pied`.`sumber` = `prt`.`prtsumber`) and (`pied`.`idtransaksi` = `prt`.`prtid`)))) left join `m1_contact` `c` on((ifnull(`ri`.`risupplier`,`prt`.`prtsupplier`) = `c`.`kid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                     FxDB(drutama("pieid"), ""), sptField,
                     FxDB(drutama("piecabang"), ""), sptField,
                     FxDB(drutama("pielokasi"), ""), sptField,
                     FxDB(drutama("piesumber"), ""), sptField,
                     FxDB(drutama("pieautonotransaksi"), 0), sptField,
                     FxDB(drutama("pienotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pietgl"), ""), formatTgl), sptField,
                     FxDB(drutama("piekodepa"), ""), sptField,
                     FxDB(drutama("piekontak"), ""), sptField,
                     FxDB(drutama("piekontakperson"), ""), sptField,
                     FxDB(drutama("pie1alamat1"), ""), sptField,
                     FxDB(drutama("pie1alamat2"), ""), sptField,
                     FxDB(drutama("pie1alamat3"), ""), sptField,
                     FxDB(drutama("pie2alamat1"), ""), sptField,
                     FxDB(drutama("pie2alamat2"), ""), sptField,
                     FxDB(drutama("pie2alamat3"), ""), sptField,
                     FxDB(drutama("pieuraian"), ""), sptField,
                     FxDB(drutama("piecatatan"), ""), sptField,
                     FxDB(drutama("pienoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pietglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("piestatus"), 0), sptField,
                     FxDB(drutama("piestatussebelumnya"), 0), sptField,
                     FxDB(drutama("piejmlrevisi"), 0), sptField,
                     FxDB(drutama("piecetakanke"), 0), sptField,
                     FxDB(drutama("pieinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pieinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("piemodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("piemodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pieposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("piepostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pieisclose"), 0), sptField,
                     FxDB(drutama("piecustomtext1"), ""), sptField,
                     FxDB(drutama("piecustomtext2"), ""), sptField,
                     FxDB(drutama("piecustomtext3"), ""), sptField,
                     FxDB(drutama("piecustomtext4"), ""), sptField,
                     FxDB(drutama("piecustomtext5"), ""), sptField,
                     FxDB(drutama("piecustomint1"), 0), sptField,
                     FxDB(drutama("piecustomint2"), 0), sptField,
                     FxDB(drutama("piecustomint3"), 0), sptField,
                     FxDB(drutama("piecustomdbl1"), 0), sptField,
                     FxDB(drutama("piecustomdbl2"), 0), sptField,
                     FxDB(drutama("piecustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("piecustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("piecustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("piecustomdate3"), ""), formatTgl))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idpiedetail"), ""), sptField,
                     FxDB(dr("idpie"), ""), sptField,
                     FxDB(dr("sumber"), ""), sptField,
                     FxDB(dr("idtransaksi"), ""), sptField,
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
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("notransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                     FxDB(dr("supplier"), ""), sptField,
                     FxDB(dr("supplierkode"), ""), sptField,
                     FxDB(dr("suppliernama"), ""), sptField,
                     FxDB(dr("supplierkontak"), ""), sptField,
                     FxDB(dr("termin"), ""), sptField,
                     FxDB(dr("uraian"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("totaltransaksi"), 0), sptField,
                     FxDB(dr("jmlbayar"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pieid, piecabang, pielokasi, piesumber, pieautonotransaksi, pienotransaksi, pietgl, piekodepa, piekontak, piekontakperson, pie1alamat1, pie1alamat2, pie1alamat3, pie2alamat1, pie2alamat2, pie2alamat3, pieuraian, piecatatan, pienoref, pietglnoref, piestatus, piestatussebelumnya, piejmlrevisi, piecetakanke, pieinputuser, pieinputtgl, piemodifikasiuser, piemodifikasitgl, pieposting, piepostingtgl, pieisclose, piecustomtext1, piecustomtext2, piecustomtext3, piecustomtext4, piecustomtext5, piecustomint1, piecustomint2, piecustomint3, piecustomdbl1, piecustomdbl2, piecustomdbl3, piecustomdate1, piecustomdate2, piecustomdate3" & sptSubParam & "idpiedetail, idpie, sumber, idtransaksi, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, cabang, lokasi, gudang, notransaksi, tgl, supplier, supplierkode, suppliernama, supplierkontak, termin, uraian, matauang, kurs, totaltransaksi, jmlbayar"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_PieTakedataSearch(ByVal param As String) As String
        'M4_PieTakedataSearch --------------------------------------------------------
        'sumber, id, cabang, lokasi, gudang, notransaksi, tgl, 
        'supplier, supplierkode, suppliernama, supplierkontak, termin, uraian, catatan, 
        'matauang, kurs, totaltransaksi, jmlbayar

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
        sql = M4_PieTakedata_Query(Filter)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Pr_Detail", "", Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("sumber"), ""), sptField,
                     FxDB(dr("id"), ""), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("notransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                     FxDB(dr("supplier"), ""), sptField,
                     FxDB(dr("supplierkode"), ""), sptField,
                     FxDB(dr("suppliernama"), ""), sptField,
                     FxDB(dr("supplierkontak"), ""), sptField,
                     FxDB(dr("termin"), ""), sptField,
                     FxDB(dr("uraian"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("totaltransaksi"), 0), sptField,
                     FxDB(dr("jmlbayar"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sumber, id, cabang, lokasi, gudang, notransaksi, tgl, supplier, supplierkode, suppliernama, supplierkontak, termin, uraian, catatan, matauang, kurs, totaltransaksi, jmlbayar"))

        Return wsResult
    End Function

    Public Function M4_PieTakedata_Query(ByVal strFilter As String) As String
        Dim sql As String
        Dim filter1 As String = "", filter2 As String = ""

        'Replace Filter
        If (strFilter.Length > 0) Then
            filter1 = strFilter
            filter1 = filter1.Replace("sumber", "ri.risumber")
            filter1 = filter1.Replace("id", "ri.riid")
            filter1 = filter1.Replace("notransaksi", "ri.rinotransaksi")
            filter1 = filter1.Replace("tgl", "ri.ritgl")
            filter1 = filter1.Replace("supplierkode", "c1.kkode")
            filter1 = filter1.Replace("suppliernama", "c1.knama")
            filter1 = filter1.Replace("supplier", "ri.risupplier")
            filter1 = filter1.Replace("supplierkontak", "ri.risupplierkontak")
            filter1 = filter1.Replace("termin", "ri.ritermin")
            filter1 = filter1.Replace("uraian", "ri.riuraian")
            filter1 = filter1.Replace("catatan", "ri.ricatatan")
            filter1 = filter1.Replace("matauang", "ri.rimatauang")
            filter1 = filter1.Replace("kurs", "ri.rikurs")
            filter1 = filter1.Replace("totaltransaksi", "ri.ritotaltransaksi")
            filter1 = filter1.Replace("jmlbayar", "ri.rijmlbayar")
            'filter1 = filter1.Replace("status", "ri.ristatus")
            filter1 = filter1.Replace("statuspie", "ri.ristatuspie")

            filter2 = strFilter
            filter2 = filter2.Replace("sumber", "prt.prtsumber")
            filter2 = filter2.Replace("id", "prt.prtid")
            filter2 = filter2.Replace("notransaksi", "prt.prtnotransaksi")
            filter2 = filter2.Replace("tgl", "prt.prttgl")
            filter2 = filter2.Replace("supplierkode", "c1.kkode")
            filter2 = filter2.Replace("suppliernama", "c1.knama")
            filter2 = filter2.Replace("supplier", "prt.prtsupplier")
            filter2 = filter2.Replace("supplierkontak", "prt.prtsupplierkontak")
            filter2 = filter2.Replace("termin", "prt.prttermin")
            filter2 = filter2.Replace("uraian", "prt.prturaian")
            filter2 = filter2.Replace("catatan", "prt.prtcatatan")
            filter2 = filter2.Replace("matauang", "prt.prtmatauang")
            filter2 = filter2.Replace("kurs", "prt.prtkurs")
            filter2 = filter2.Replace("totaltransaksi", "prt.prttotaltransaksi")
            filter2 = filter2.Replace("jmlbayar", "prt.prtjmlbayar")
            'filter2 = filter2.Replace("status", "prt.prtstatus")
            filter2 = filter2.Replace("statuspie", "prt.prtstatuspie")

        End If


        filter1 = " WHERE ri.ristatus IN(2,3,4,7) AND " & filter1

        filter2 = " WHERE prt.prtstatus IN(2,3,4,7) AND " & filter2


        'RI
        sql = "  (select ri.risumber as sumber, ri.riid AS id, ri.ricabang AS cabang, ri.rilokasi AS lokasi, ri.rigudang AS gudang, ri.rinotransaksi AS notransaksi, ri.ritgl AS tgl, ri.risupplier AS supplier, c1.kkode AS supplierkode, c1.knama AS suppliernama, ri.risupplierkontak AS supplierkontak, ri.ritermin AS termin, ri.riuraian AS uraian, ri.ricatatan AS catatan, ri.rimatauang AS matauang, ri.rikurs AS kurs, ri.ritotaltransaksi AS totaltransaksi, ri.rijmlbayar AS jmlbayar from m4_ri ri left join m1_contact c1 on ri.risupplier = c1.kid " & filter1 & ") "
        'PRT
        sql &= " UNION ALL "
        sql &= " (select prt.prtsumber as sumber, prt.prtid AS id, prt.prtcabang AS cabang, prt.prtlokasi AS lokasi, prt.prtgudang AS gudang, prt.prtnotransaksi AS notransaksi, prt.prttgl AS tgl, prt.prtsupplier AS supplier, c1.kkode AS supplierkode, c1.knama AS suppliernama, prt.prtsupplierkontak AS supplierkontak, prt.prttermin AS termin, prt.prturaian AS uraian, prt.prtcatatan AS catatan, prt.prtmatauang AS matauang, prt.prtkurs AS kurs, prt.prttotaltransaksi AS totaltransaksi, prt.prtjmlbayar AS jmlbayar from m4_prt prt left join m1_contact c1 on prt.prtsupplier = c1.kid " & filter2 & ") "

        Return sql
    End Function

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, _
                                    ByVal ftExistRI As String, ByVal ftBelumPieRI As String, _
                                    ByVal ftExistPRT As String, ByVal ftBelumPiePRT As String) As String

        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, sumber As String = "", notransaksi As String = "", matauang As String = "", tgl As String = ""
        Dim filterLookup As String = "", urutan As String = "", sisa As Double = 0


        'VALIDASI TRANSAKSI OUTSTANDING ------------------------------
        'RI
        If Len(ftExistRI) > 0 Then 'ftExistOutstanding = rowExists, riid, risumber, rinotransaksi
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistRI)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                notransaksi = dtval.Rows(0)("rinotransaksi")
                sumber = dtval.Rows(0)("risumber")

                filterLookup = "sumber = '" & dtval.Rows(0)("risumber") & "' AND idtransaksi = '" & dtval.Rows(0)("riid") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " doesn't exists/yet approved in RI" : GoTo selesai
            End If
        End If

        'CEK SUDAH PIE ATAU BELUM
        If Len(ftBelumPieRI) > 0 Then
            sql = "  SELECT pie.pieid, pie.pienotransaksi, ri.risumber as sumber, ri.riid as id, ri.rinotransaksi as notransaksi "
            sql &= " FROM m4_pie pie "
            sql &= " JOIN m4_pie_detail pied ON pie.pieid = pied.idpie "
            sql &= " JOIN m4_ri ri ON pied.sumber = ri.risumber AND pied.idtransaksi = ri.riid "
            sql &= " WHERE pie.piestatus IN(2,3,4,7) "
            sql &= " AND (" & ftBelumPieRI & ") "
            sql &= " GROUP BY pie.pieid, ri.riid "
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                notransaksi = dtval.Rows(0)("notransaksi")
                sumber = dtval.Rows(0)("sumber")

                filterLookup = "sumber = '" & sumber & "' AND idtransaksi = '" & dtval.Rows(0)("id") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " has related transaction on " & dtval.Rows(0)("pienotransaksi") & "" : GoTo selesai
            End If
        End If
        'END OF VALIDASI TRANSAKSI OUTSTANDING -----------------------


        'VALIDASI TRANSAKSI OUTSTANDING ------------------------------
        'PRT
        If Len(ftExistPRT) > 0 Then 'ftExistOutstanding = rowExists, prtid, prtsumber, prtnotransaksi
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistPRT)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                notransaksi = dtval.Rows(0)("prtnotransaksi")
                sumber = dtval.Rows(0)("prtsumber")

                filterLookup = "sumber = '" & dtval.Rows(0)("prtsumber") & "' AND idtransaksi = '" & dtval.Rows(0)("prtid") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " doesn't exists/yet approved in PRT" : GoTo selesai
            End If
        End If

        'CEK SUDAH PIE ATAU BELUM
        If Len(ftBelumPiePRT) > 0 Then
            sql = "  SELECT pie.pieid, pie.pienotransaksi, prt.prtsumber as sumber, prt.prtid as id, prt.prtnotransaksi as notransaksi "
            sql &= " FROM m4_pie pie "
            sql &= " JOIN m4_pie_detail pied ON pie.pieid = pied.idpie "
            sql &= " JOIN m4_prt prt ON pied.sumber = prt.prtsumber AND pied.idtransaksi = prt.prtid "
            sql &= " WHERE pie.piestatus IN(2,3,4,7) "
            sql &= " AND (" & ftBelumPiePRT & ") "
            sql &= " GROUP BY pie.pieid, prt.prtid "
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                notransaksi = dtval.Rows(0)("notransaksi")
                sumber = dtval.Rows(0)("sumber")

                filterLookup = "sumber = '" & sumber & "' AND idtransaksi = '" & dtval.Rows(0)("id") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " has related transaction on " & dtval.Rows(0)("pienotransaksi") & "" : GoTo selesai
            End If
        End If
        'END OF VALIDASI TRANSAKSI OUTSTANDING -----------------------

selesai:
        Return errmessage
    End Function

    <WebMethod()>
    Public Function M4_PieSimpanOld(ByVal param As String) As String
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
        'pieid(0) As , piecabang(1) As String, pielokasi(2) As String, piesumber(3) As String, pieautonotransaksi(4) As Integer, 
        'pienotransaksi(5) As String, pietgl(6) As Date, piekodepa(7) As , piekontak(8) As , piekontakperson(9) As String, 
        'pie1alamat1(10) As String, pie1alamat2(11) As String, pie1alamat3(12) As String, pie2alamat1(13) As String, pie2alamat2(14) As String, 
        'pie2alamat3(15) As String, pieuraian(16) As String, piecatatan(17) As String, pienoref(18) As String, pietglnoref(19) As Date, 
        'piestatus(20) As Integer, piestatussebelumnya(21) As Integer, piejmlrevisi(22) As Integer, piecetakanke(23) As Integer, pieinputuser(24) As , 
        'pieinputtgl(25) As DateTime, piemodifikasiuser(26) As , piemodifikasitgl(27) As DateTime, pieposting(28) As Integer, piepostingtgl(29) As DateTime, 
        'pieisclose(30) As Integer, piecustomtext1(31) As String, piecustomtext2(32) As String, piecustomtext3(33) As String, piecustomtext4(34) As String, 
        'piecustomtext5(35) As String, piecustomint1(36) As Integer, piecustomint2(37) As Integer, piecustomint3(38) As Integer, piecustomdbl1(39) As Double, 
        'piecustomdbl2(40) As Double, piecustomdbl3(41) As Double, piecustomdate1(42) As Date, piecustomdate2(43) As Date, piecustomdate3(44) As Date


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'pieid, piecabang, pielokasi, piesumber, pieautonotransaksi, pienotransaksi, pietgl, 
        'piekodepa, piekontak, piekontakperson, pie1alamat1, pie1alamat2, pie1alamat3, pie2alamat1, 
        'pie2alamat2, pie2alamat3, pieuraian, piecatatan, pienoref, pietglnoref, piestatus, 
        'piestatussebelumnya, piejmlrevisi, piecetakanke, pieinputuser, pieinputtgl, piemodifikasiuser, piemodifikasitgl, 
        'pieposting, piepostingtgl, pieisclose, piecustomtext1, piecustomtext2, piecustomtext3, piecustomtext4, 
        'piecustomtext5, piecustomint1, piecustomint2, piecustomint3, piecustomdbl1, piecustomdbl2, piecustomdbl3, 
        'piecustomdate1, piecustomdate2, piecustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 45) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'pieautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "pieautonotransaksi required numeric." : GoTo selesai
        End If
        'pietgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "pietgl required date." : GoTo selesai
        End If
        'pietglnoref(19) As Date
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "pietglnoref required date." : GoTo selesai
        End If
        'piestatus(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "piestatus required numeric." : GoTo selesai
        End If
        'piestatussebelumnya(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "piestatussebelumnya required numeric." : GoTo selesai
        End If
        'piejmlrevisi(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "piejmlrevisi required numeric." : GoTo selesai
        End If
        'piecetakanke(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "piecetakanke required numeric." : GoTo selesai
        End If
        'pieinputtgl(25) As DateTime
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "pieinputtgl required date." : GoTo selesai
        End If
        'piemodifikasitgl(27) As DateTime
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "piemodifikasitgl required date." : GoTo selesai
        End If
        'pieposting(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "pieposting required numeric." : GoTo selesai
        End If
        'piepostingtgl(29) As DateTime
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "piepostingtgl required date." : GoTo selesai
        End If
        'pieisclose(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "pieisclose required numeric." : GoTo selesai
        End If
        'piecustomint1(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "piecustomint1 required numeric." : GoTo selesai
        End If
        'piecustomint2(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "piecustomint2 required numeric." : GoTo selesai
        End If
        'piecustomint3(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "piecustomint3 required numeric." : GoTo selesai
        End If
        'piecustomdbl1(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "piecustomdbl1 required numeric." : GoTo selesai
        End If
        'piecustomdbl2(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "piecustomdbl2 required numeric." : GoTo selesai
        End If
        'piecustomdbl3(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "piecustomdbl3 required numeric." : GoTo selesai
        End If
        'piecustomdate1(42) As Date
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "piecustomdate1 required date." : GoTo selesai
        End If
        'piecustomdate2(43) As Date
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "piecustomdate2 required date." : GoTo selesai
        End If
        'piecustomdate3(44) As Date
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "piecustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'pieid(0) As 
        If Len(dataUtama(0)) = 0 Then
            result(2) = "pieid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 20 Then
            result(2) = "pieid should not be more than 20 character." : GoTo selesai
        End If

        'piecabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "piecabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "piecabang should not be more than 25 character." : GoTo selesai
        End If

        'pielokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "pielokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "pielokasi should not be more than 25 character." : GoTo selesai
        End If

        'piesumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "piesumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "piesumber should not be more than 10 character." : GoTo selesai
        End If

        'pienotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "pienotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "pienotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'pietgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "pietgl can't be empty" : GoTo selesai
        End If

        'piekodepa(7) As 
        If Len(dataUtama(7)) = 0 Then
            result(2) = "piekodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 20 Then
            result(2) = "piekodepa should not be more than 20 character." : GoTo selesai
        End If

        'piekontak(8) As 
        If Len(dataUtama(8)) = 0 Then
            result(2) = "piekontak can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 20 Then
            result(2) = "piekontak should not be more than 20 character." : GoTo selesai
        End If

        'pietglnoref(19) As Date
        If Len(dataUtama(19)) = 0 Then
            result(2) = "pietglnoref can't be empty" : GoTo selesai
        End If

        'pieinputtgl(25) As DateTime
        If Len(dataUtama(25)) = 0 Then
            result(2) = "pieinputtgl can't be empty" : GoTo selesai
        End If

        'piemodifikasitgl(27) As DateTime
        If Len(dataUtama(27)) = 0 Then
            result(2) = "piemodifikasitgl can't be empty" : GoTo selesai
        End If

        'piepostingtgl(29) As DateTime
        If Len(dataUtama(29)) = 0 Then
            result(2) = "piepostingtgl can't be empty" : GoTo selesai
        End If

        'piecustomdbl1(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "piecustomdbl1 can't be empty" : GoTo selesai
        End If

        'piecustomdbl2(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "piecustomdbl2 can't be empty" : GoTo selesai
        End If

        'piecustomdbl3(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "piecustomdbl3 can't be empty" : GoTo selesai
        End If

        'piecustomdate1(42) As Date
        If Len(dataUtama(42)) = 0 Then
            result(2) = "piecustomdate1 can't be empty" : GoTo selesai
        End If

        'piecustomdate2(43) As Date
        If Len(dataUtama(43)) = 0 Then
            result(2) = "piecustomdate2 can't be empty" : GoTo selesai
        End If

        'piecustomdate3(44) As Date
        If Len(dataUtama(44)) = 0 Then
            result(2) = "piecustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "pieid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piecabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pielokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piesumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pieautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pienotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pietgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piekodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piekontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piekontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pie1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pie1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pie1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pie2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pie2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pie2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pieuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piecatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pienoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pietglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piestatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piestatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piejmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piecetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pieinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pieinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piemodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piemodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pieposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piepostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pieisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piecustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piecustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piecustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piecustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piecustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piecustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piecustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piecustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "piecustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piecustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piecustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piecustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piecustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "piecustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahData(dtutama, "pieid~piecabang~pielokasi~piesumber~pieautonotransaksi~pienotransaksi~pietgl~piekodepa~piekontak~piekontakperson~pie1alamat1~pie1alamat2~pie1alamat3~pie2alamat1~pie2alamat2~pie2alamat3~pieuraian~piecatatan~pienoref~pietglnoref~piestatus~piestatussebelumnya~piejmlrevisi~piecetakanke~pieinputuser~pieinputtgl~piemodifikasiuser~piemodifikasitgl~pieposting~piepostingtgl~pieisclose~piecustomtext1~piecustomtext2~piecustomtext3~piecustomtext4~piecustomtext5~piecustomint1~piecustomint2~piecustomint3~piecustomdbl1~piecustomdbl2~piecustomdbl3~piecustomdate1~piecustomdate2~piecustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44))

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idpiedetail(0) As , idpie(1) As , sumber(2) As String, idtransaksi(3) As , catatan(4) As String, 
        'urutan(5) As Integer, isclose(6) As Integer, customtext1(7) As String, customtext2(8) As String, customtext3(9) As String, 
        'customdbl1(10) As Double, customdbl2(11) As Double, customdbl3(12) As Double, customdate1(13) As Date, customdate2(14) As Date, 
        'customdate3(15) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idpiedetail, idpie, sumber, idtransaksi, catatan, urutan, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idpiedetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpie", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idtransaksi", AsEnumTypeData.AsInt64)
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


        'VARIABEL VALIDASI OUTSTANDING
        Dim sumberDetail As String = "", idtransaksiDetail As Integer = 0
        Dim ftExistRI As String = "", ftBelumPieRI As String = ""
        Dim ftExistPRT As String = "", ftBelumPiePRT As String = ""


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
            'idpiedetail(0) As 
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idpiedetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idpiedetail should not be more than 20 character." : GoTo selesai
            End If

            'idpie(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idpie can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idpie should not be more than 20 character." : GoTo selesai
            End If

            'sumber(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - sumber can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 10 Then
                result(2) = "Row : " & i & " - sumber should not be more than 10 character." : GoTo selesai
            End If

            'idtransaksi(3) As 
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - idtransaksi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 20 Then
                result(2) = "Row : " & i & " - idtransaksi should not be more than 20 character." : GoTo selesai
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

            AsDataTableTambahData(dtdetail, "idpiedetail~idpie~sumber~idtransaksi~catatan~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15))


            'VALIDASI OUTSTANDING ---------------------------------------
            'SET SUMBER DAN IDTRANSAKSI
            'sumber(2) As String,               idtransaksi(3) As
            sumberDetail = dataRowDetail(2) : idtransaksiDetail = dataRowDetail(3)

            Select Case sumberDetail
                Case "RI"
                    'CEK DATA EXIST
                    ftExistRI = IIf(Len(ftExistRI.ToString) = 0, "", ftExistRI & " UNION ")
                    ftExistRI = String.Concat(ftExistRI, "SELECT EXISTS(SELECT 1 FROM m4_ri WHERE riid = '" & idtransaksiDetail & "' AND ristatus IN(2,3,4,7) LIMIT 1) as rowExists, riid, risumber, rinotransaksi FROM m4_ri WHERE riid = '" & idtransaksiDetail & "'")

                    'CEK OUTSTANDING
                    ftBelumPieRI = IIf(Len(ftBelumPieRI.ToString) = 0, "", ftBelumPieRI & " OR ")
                    ftBelumPieRI = String.Concat(ftBelumPieRI, " (pied.sumber = '" & FixQuotes(sumberDetail) & "' AND pied.idtransaksi = '" & FixDouble(idtransaksiDetail) & "') ")

                Case "PRT"
                    'CEK DATA EXIST
                    ftExistPRT = IIf(Len(ftExistPRT.ToString) = 0, "", ftExistPRT & " UNION ")
                    ftExistPRT = String.Concat(ftExistPRT, "SELECT EXISTS(SELECT 1 FROM m4_prt WHERE prtid = '" & idtransaksiDetail & "' AND prtstatus IN(2,3,4,7) LIMIT 1) as rowExists, prtid, prtsumber, prtnotransaksi FROM m4_prt WHERE prtid = '" & idtransaksiDetail & "'")

                    'CEK OUTSTANDING
                    ftBelumPiePRT = IIf(Len(ftBelumPiePRT.ToString) = 0, "", ftBelumPiePRT & " OR ")
                    ftBelumPiePRT = String.Concat(ftBelumPiePRT, " (pied.sumber = '" & FixQuotes(sumberDetail) & "' AND pied.idtransaksi = '" & FixDouble(idtransaksiDetail) & "') ")
            End Select
            'END OF VALIDASI OUTSTANDING --------------------------------

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

                ''CEK PERIODE AKUNTANSI ==================================
                'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("pietgl")), AsFormatTanggal(drutama("pietgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================

                'VALIDASI SIMPAN =========================================
                If drutama("piestatus") = 2 Then
                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistRI, ftBelumPieRI, ftExistPRT, ftBelumPiePRT)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN ==================================

                If isUpdate Then
                    result(4) = drutama("pieid")
                    notransaksi = drutama("pienotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(pieid), pienotransaksi FROM M4_pie WHERE pieid='" & result(4) & "' AND piestatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(pieid) FROM m4_pie WHERE pienotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_pie_history
                        Dim rsSimpanHistory As String = SimpanHistory.M4_Pie_HistorySimpan("" & paramSplit(0) & "★M4_Pie_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("piesumber")) & "▼" & FixQuotes(drutama("pieid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M4_Pie set piecabang  = '" & FixQuotes(drutama("piecabang")) & "', pielokasi  = '" & FixQuotes(drutama("pielokasi")) & "', piesumber  = '" & FixQuotes(drutama("piesumber")) & "', pieautonotransaksi  = " & drutama("pieautonotransaksi") & ", pienotransaksi  = '" & FixQuotes(notransaksi) & "', pietgl  = '" & FixQuotes(AsFormatTanggal(drutama("pietgl"))) & "', piekodepa  = '" & FixQuotes(drutama("piekodepa")) & "', piekontak  = '" & FixQuotes(drutama("piekontak")) & "', piekontakperson  = '" & FixQuotes(drutama("piekontakperson")) & "', pie1alamat1  = '" & FixQuotes(drutama("pie1alamat1")) & "', pie1alamat2  = '" & FixQuotes(drutama("pie1alamat2")) & "', pie1alamat3  = '" & FixQuotes(drutama("pie1alamat3")) & "', pie2alamat1  = '" & FixQuotes(drutama("pie2alamat1")) & "', pie2alamat2  = '" & FixQuotes(drutama("pie2alamat2")) & "', pie2alamat3  = '" & FixQuotes(drutama("pie2alamat3")) & "', pieuraian  = '" & FixQuotes(drutama("pieuraian")) & "', piecatatan  = '" & FixQuotes(drutama("piecatatan")) & "', pienoref  = '" & FixQuotes(drutama("pienoref")) & "', pietglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("pietglnoref"))) & "', piestatus  = " & drutama("piestatus") & ", piestatussebelumnya  = " & drutama("piestatussebelumnya") & ", piejmlrevisi  = " & drutama("piejmlrevisi") & ", piecetakanke  = " & drutama("piecetakanke") & ", pieinputuser  = '" & FixQuotes(drutama("pieinputuser")) & "', pieinputtgl  = '" & FixQuotes(AsFormatTanggal(drutama("pieinputtgl"), "yyyy-MM-dd HH:mm:ss")) & "', piemodifikasiuser  = '" & FixQuotes(drutama("piemodifikasiuser")) & "', piemodifikasitgl  = '" & FixQuotes(AsFormatTanggal(drutama("piemodifikasitgl"), "yyyy-MM-dd HH:mm:ss")) & "', pieposting  = " & drutama("pieposting") & ", piepostingtgl  = '" & FixQuotes(AsFormatTanggal(drutama("piepostingtgl"), "yyyy-MM-dd HH:mm:ss")) & "', piecustomtext1  = '" & FixQuotes(drutama("piecustomtext1")) & "', piecustomtext2  = '" & FixQuotes(drutama("piecustomtext2")) & "', piecustomtext3  = '" & FixQuotes(drutama("piecustomtext3")) & "', piecustomtext4  = '" & FixQuotes(drutama("piecustomtext4")) & "', piecustomtext5  = '" & FixQuotes(drutama("piecustomtext5")) & "', piecustomint1  = " & drutama("piecustomint1") & ", piecustomint2  = " & drutama("piecustomint2") & ", piecustomint3  = " & drutama("piecustomint3") & ", piecustomdbl1  = '" & FixDouble(drutama("piecustomdbl1")) & "', piecustomdbl2  = '" & FixDouble(drutama("piecustomdbl2")) & "', piecustomdbl3  = '" & FixDouble(drutama("piecustomdbl3")) & "', piecustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("piecustomdate1"))) & "', piecustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("piecustomdate2"))) & "', piecustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("piecustomdate3"))) & "' where pieid = " & drutama("pieid") & ""
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

                    If drutama("pieautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("piecabang"), drutama("pielokasi"), drutama("piesumber"), drutama("pietgl"))
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
                        notransaksi = drutama("pienotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(pieid) FROM m4_pie WHERE pienotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Pie (piecabang, pielokasi, piesumber, pieautonotransaksi, pienotransaksi, pietgl, piekodepa, piekontak, piekontakperson, pie1alamat1, pie1alamat2, pie1alamat3, pie2alamat1, pie2alamat2, pie2alamat3, pieuraian, piecatatan, pienoref, pietglnoref, piestatus, piestatussebelumnya, piejmlrevisi, piecetakanke, pieinputuser, pieinputtgl, piemodifikasiuser, piemodifikasitgl, pieposting, piepostingtgl, pieisclose, piecustomtext1, piecustomtext2, piecustomtext3, piecustomtext4, piecustomtext5, piecustomint1, piecustomint2, piecustomint3, piecustomdbl1, piecustomdbl2, piecustomdbl3, piecustomdate1, piecustomdate2, piecustomdate3) values('" & FixQuotes(drutama("piecabang")) & "', '" & FixQuotes(drutama("pielokasi")) & "', '" & FixQuotes(drutama("piesumber")) & "', " & drutama("pieautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("pietgl"))) & "', '" & FixQuotes(drutama("piekodepa")) & "', '" & FixQuotes(drutama("piekontak")) & "', '" & FixQuotes(drutama("piekontakperson")) & "', '" & FixQuotes(drutama("pie1alamat1")) & "', '" & FixQuotes(drutama("pie1alamat2")) & "', '" & FixQuotes(drutama("pie1alamat3")) & "', '" & FixQuotes(drutama("pie2alamat1")) & "', '" & FixQuotes(drutama("pie2alamat2")) & "', '" & FixQuotes(drutama("pie2alamat3")) & "', '" & FixQuotes(drutama("pieuraian")) & "', '" & FixQuotes(drutama("piecatatan")) & "', '" & FixQuotes(drutama("pienoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pietglnoref"))) & "', " & drutama("piestatus") & ", " & drutama("piestatussebelumnya") & ", " & drutama("piejmlrevisi") & ", " & drutama("piecetakanke") & ", '" & FixQuotes(drutama("pieinputuser")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pieinputtgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(drutama("piemodifikasiuser")) & "', '" & FixQuotes(AsFormatTanggal(drutama("piemodifikasitgl"), "yyyy-MM-dd HH:mm:ss")) & "', " & drutama("pieposting") & ", '" & FixQuotes(AsFormatTanggal(drutama("piepostingtgl"), "yyyy-MM-dd HH:mm:ss")) & "', " & drutama("pieisclose") & ", '" & FixQuotes(drutama("piecustomtext1")) & "', '" & FixQuotes(drutama("piecustomtext2")) & "', '" & FixQuotes(drutama("piecustomtext3")) & "', '" & FixQuotes(drutama("piecustomtext4")) & "', '" & FixQuotes(drutama("piecustomtext5")) & "', " & drutama("piecustomint1") & ", " & drutama("piecustomint2") & ", " & drutama("piecustomint3") & ", '" & FixDouble(drutama("piecustomdbl1")) & "', '" & FixDouble(drutama("piecustomdbl2")) & "', '" & FixDouble(drutama("piecustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("piecustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("piecustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("piecustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select pieid from M4_pie where pienotransaksi='" & notransaksi & "' AND pieinputuser= '" & userid & "' order by piemodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Pie_Detail where idpie = '" & result(4) & "'"
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
                    Dim strValue2 As New StringBuilder, strGiro As New StringBuilder

                    For Each dr1 As DataRow In dtdetail.Rows

                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("idpiedetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("sumber")) & "', '" & FixQuotes(dr1("idtransaksi")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Pie_Detail(idpiedetail, idpie, sumber, idtransaksi, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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


                'UPDATE OUTSTANDING TRANSAKSI ==========================================================
                If drutama("piestatus") = 2 Then
                    'UPDATE M4 RI (SET STATUSPIE DAN TGL JATUH TEMPO SESUAI SETTING BERDASARKAN TERMIN DAN TGL PIE)
                    sql = "UPDATE m4_pie pie JOIN m4_pie_detail pied ON pie.pieid = pied.idpie JOIN m4_ri ri ON pied.sumber = ri.risumber AND pied.idtransaksi = ri.riid LEFT JOIN m0_setting s ON s.smodule = 4 AND s.sgrup = 'tukarfaktur' AND s.skode = 'UpdateTglJatuhTempoRI' AND s.snilai = 1 LEFT JOIN m1_terms tr ON ri.ritermin = tr.trkode SET ri.ristatuspie = 1, ri.ritglpie = pie.pietgl, ri.ritgljatuhtempo = (CASE IFNULL(s.snilai,0) WHEN 1 THEN DATE_ADD(pie.pietgl,INTERVAL IFNULL(tr.trharijatuhtempo,0) DAY) ELSE ri.ritgljatuhtempo END) WHERE pie.pieid = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE M4 PRT (SET STATUSPIE DAN TGL JATUH TEMPO SESUAI SETTING BERDASARKAN TERMIN DAN TGL PIE)
                    sql = "UPDATE m4_pie pie JOIN m4_pie_detail pied ON pie.pieid = pied.idpie JOIN m4_prt prt ON pied.sumber = prt.prtsumber AND pied.idtransaksi = prt.prtid LEFT JOIN m0_setting s ON s.smodule = 4 AND s.sgrup = 'tukarfaktur' AND s.skode = 'UpdateTglJatuhTempoPRT' AND s.snilai = 1 LEFT JOIN m1_terms tr ON prt.prttermin = tr.trkode SET prt.prtstatuspie = 1, prt.prttglpie = pie.pietgl, prt.prttgljatuhtempo = (CASE IFNULL(s.snilai,0) WHEN 1 THEN DATE_ADD(pie.pietgl,INTERVAL IFNULL(tr.trharijatuhtempo,0) DAY) ELSE prt.prttgljatuhtempo END) WHERE pie.pieid = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE OUTSTANDING TRANSAKSI ===================================================


                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "PIE", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                'If drutama("piestatus") = 2 Then
                '    Dim Security As New ClsSecurity, mjid As String = "", hasilMsmq As String = ""
                '    'BUAT ID UNIQUE
                '    mjid = Security.MD5CalcString(userid & sumber & result(4) & Now) 'RandomId.Generate(15)

                '    'MSMQ TABEL
                '    'sql = "Insert into M0_Msmq_Journal(mjid, mjsumber, mjidtransaksi, mjprogress, mjpesan, mjtglantrian, mjtglselesai, mjuserid) values ('" _
                '    '    & mjid & "', '" & sumber & "', '" & result(4) & "', '" & 0 & "', " & "''" & ", NOW(), '1971-01-01 00:00:00', '" & userid & "')"
                '    'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '    'With objCmd
                '    '    .Connection = Con1
                '    '    .Transaction = Trans
                '    '    .CommandType = CommandType.Text
                '    '    .CommandText = sql
                '    'End With
                '    'objCmd.ExecuteNonQuery()

                '    'MSMQ ANTRIAN
                '    'Dim PostingJurnal As String = F_getSetting(0, "accounting", "AutoPosting")
                '    'If PostingJurnal.Equals("0") = False Then
                '    '    hasilMsmq = SendMsmq(dirMsmq, "J", mjid, sumber, result(4), userid)
                '    '    If Len(hasilMsmq) > 0 Then
                '    '        result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
                '    '    End If
                '    'End If

                'End If
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
    Public Function M4_PieUpdateStatusOld(ByVal param As String) As String

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
            'Filter = Filter.Replace("piekontakkode", "c1.kkode")
            'Filter = Filter.Replace("piekontaknama", "c1.knama")
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
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)
        Try

            'PERSIAPAN INSERT USER LOG ==========================================================
            Dim sumber As String = "Pie", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Pietgl, pienotransaksi, piestatus FROM m4_Pie WHERE Pieid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Piestatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m4_pie_history
            Dim rsSimpanHistory As String = SimpanHistory.M4_Pie_HistorySimpan("" & paramSplit(0) & "★M4_Pie_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                'Dim query As New m0_query
                'sql = query.PanggilQuery("m4_pie_terkait")
                'sql = sql.Replace("validtransaksi", idtransaksi)
                'Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                'dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                'If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================


                'UPDATE OUTSTANDING TRANSAKSI ===================================================
                'UPDATE M4 RI (SET STATUSPIE DAN TGL JATUH TEMPO SESUAI SETTING BERDASARKAN TERMIN DAN TGL PIE)
                sql = "UPDATE m4_pie pie JOIN m4_pie_detail pied ON pie.pieid = pied.idpie JOIN m4_ri ri ON pied.sumber = ri.risumber AND pied.idtransaksi = ri.riid LEFT JOIN m0_setting s ON s.smodule = 4 AND s.sgrup = 'tukarfaktur' AND s.skode = 'UpdateTglJatuhTempoRI' AND s.snilai = 1 LEFT JOIN m1_terms tr ON ri.ritermin = tr.trkode SET ri.ristatuspie = 0, ri.ritglpie = '1900-01-01', ri.ritgljatuhtempo = (CASE IFNULL(s.snilai,0) WHEN 1 THEN '2100-12-31' ELSE ri.ritgljatuhtempo END) WHERE pie.pieid = '" & idtransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'UPDATE M4 PRT (SET STATUSPIE DAN TGL JATUH TEMPO SESUAI SETTING BERDASARKAN TERMIN DAN TGL PIE)
                sql = "UPDATE m4_pie pie JOIN m4_pie_detail pied ON pie.pieid = pied.idpie JOIN m4_prt prt ON pied.sumber = prt.prtsumber AND pied.idtransaksi = prt.prtid LEFT JOIN m0_setting s ON s.smodule = 4 AND s.sgrup = 'tukarfaktur' AND s.skode = 'UpdateTglJatuhTempoPRT' AND s.snilai = 1 LEFT JOIN m1_terms tr ON prt.prttermin = tr.trkode SET prt.prtstatuspie = 0, prt.prttglpie = '1900-01-01', prt.prttgljatuhtempo = (CASE IFNULL(s.snilai,0) WHEN 1 THEN '2100-12-31' ELSE prt.prttgljatuhtempo END) WHERE pie.pieid = '" & idtransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF UPDATE OUTSTANDING TRANSAKSI ============================================

            End If

            'update status utama
            sql = "UPDATE m4_Pie SET Piestatus = " & nilaiStatus & ", Piemodifikasiuser='" & userid & "', Piemodifikasitgl = NOW(), Pieposting = 0, Piepostingtgl = '1971-01-01 00:00:00', Piejmlrevisi = Piejmlrevisi + 1 WHERE Pieid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_PieSearch(PostWsSearch(paramSplit(0), "M4_PieSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_PieDeleteOld(ByVal param As String) As String

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
            'Filter = Filter.Replace("piekontakkode", "c1.kkode")
            'Filter = Filter.Replace("piekontaknama", "c1.knama")
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
            Dim sumber As String = "Pie", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Pieid, Pienotransaksi FROM M4_Pie WHERE Pieid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT piecabang, pielokasi, piesumber, pieautonotransaksi, pienotransaksi, pietgl"
            sql &= " FROM M4_pie"
            sql &= " WHERE pieid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("piecabang")
                lokasi = dtNomorNext.Rows(0)("pielokasi")
                sumber = dtNomorNext.Rows(0)("piesumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("pieautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("pienotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("pietgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M4_pie_Detail WHERE idpie = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M4_pie WHERE pieid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_PieSearch(PostWsSearch(paramSplit(0), "M4_PieSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
