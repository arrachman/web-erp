Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_sie
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_SieSimpan(ByVal param As String) As String
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
        'sieid(0) As , siecabang(1) As String, sielokasi(2) As String, siesumber(3) As String, sieautonotransaksi(4) As Integer, 
        'sienotransaksi(5) As String, sietgl(6) As Date, siekodepa(7) As , siekontak(8) As , siekontakperson(9) As String, 
        'sie1alamat1(10) As String, sie1alamat2(11) As String, sie1alamat3(12) As String, sie2alamat1(13) As String, sie2alamat2(14) As String, 
        'sie2alamat3(15) As String, sieuraian(16) As String, siecatatan(17) As String, sienoref(18) As String, sietglnoref(19) As Date, 
        'siestatus(20) As Integer, siestatussebelumnya(21) As Integer, siejmlrevisi(22) As Integer, siecetakanke(23) As Integer, sieinputuser(24) As , 
        'sieinputtgl(25) As DateTime, siemodifikasiuser(26) As , siemodifikasitgl(27) As DateTime, sieposting(28) As Integer, siepostingtgl(29) As DateTime, 
        'sieisclose(30) As Integer, siecustomtext1(31) As String, siecustomtext2(32) As String, siecustomtext3(33) As String, siecustomtext4(34) As String, 
        'siecustomtext5(35) As String, siecustomint1(36) As Integer, siecustomint2(37) As Integer, siecustomint3(38) As Integer, siecustomdbl1(39) As Double, 
        'siecustomdbl2(40) As Double, siecustomdbl3(41) As Double, siecustomdate1(42) As Date, siecustomdate2(43) As Date, siecustomdate3(44) As Date


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'sieid, siecabang, sielokasi, siesumber, sieautonotransaksi, sienotransaksi, sietgl, 
        'siekodepa, siekontak, siekontakperson, sie1alamat1, sie1alamat2, sie1alamat3, sie2alamat1, 
        'sie2alamat2, sie2alamat3, sieuraian, siecatatan, sienoref, sietglnoref, siestatus, 
        'siestatussebelumnya, siejmlrevisi, siecetakanke, sieinputuser, sieinputtgl, siemodifikasiuser, siemodifikasitgl, 
        'sieposting, siepostingtgl, sieisclose, siecustomtext1, siecustomtext2, siecustomtext3, siecustomtext4, 
        'siecustomtext5, siecustomint1, siecustomint2, siecustomint3, siecustomdbl1, siecustomdbl2, siecustomdbl3, 
        'siecustomdate1, siecustomdate2, siecustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 45) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'sieautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "sieautonotransaksi required numeric." : GoTo selesai
        End If
        'sietgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "sietgl required date." : GoTo selesai
        End If
        'sietglnoref(19) As Date
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "sietglnoref required date." : GoTo selesai
        End If
        'siestatus(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "siestatus required numeric." : GoTo selesai
        End If
        'siestatussebelumnya(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "siestatussebelumnya required numeric." : GoTo selesai
        End If
        'siejmlrevisi(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "siejmlrevisi required numeric." : GoTo selesai
        End If
        'siecetakanke(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "siecetakanke required numeric." : GoTo selesai
        End If
        'sieinputtgl(25) As DateTime
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "sieinputtgl required date." : GoTo selesai
        End If
        'siemodifikasitgl(27) As DateTime
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "siemodifikasitgl required date." : GoTo selesai
        End If
        'sieposting(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "sieposting required numeric." : GoTo selesai
        End If
        'siepostingtgl(29) As DateTime
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "siepostingtgl required date." : GoTo selesai
        End If
        'sieisclose(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "sieisclose required numeric." : GoTo selesai
        End If
        'siecustomint1(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "siecustomint1 required numeric." : GoTo selesai
        End If
        'siecustomint2(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "siecustomint2 required numeric." : GoTo selesai
        End If
        'siecustomint3(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "siecustomint3 required numeric." : GoTo selesai
        End If
        'siecustomdbl1(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "siecustomdbl1 required numeric." : GoTo selesai
        End If
        'siecustomdbl2(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "siecustomdbl2 required numeric." : GoTo selesai
        End If
        'siecustomdbl3(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "siecustomdbl3 required numeric." : GoTo selesai
        End If
        'siecustomdate1(42) As Date
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "siecustomdate1 required date." : GoTo selesai
        End If
        'siecustomdate2(43) As Date
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "siecustomdate2 required date." : GoTo selesai
        End If
        'siecustomdate3(44) As Date
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "siecustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'sieid(0) As 
        If Len(dataUtama(0)) = 0 Then
            result(2) = "sieid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 20 Then
            result(2) = "sieid should not be more than 20 character." : GoTo selesai
        End If

        'siecabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "siecabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "siecabang should not be more than 25 character." : GoTo selesai
        End If

        'sielokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "sielokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "sielokasi should not be more than 25 character." : GoTo selesai
        End If

        'siesumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "siesumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "siesumber should not be more than 10 character." : GoTo selesai
        End If

        'sienotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "sienotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "sienotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'sietgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "sietgl can't be empty" : GoTo selesai
        End If

        'siekodepa(7) As 
        If Len(dataUtama(7)) = 0 Then
            result(2) = "siekodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 20 Then
            result(2) = "siekodepa should not be more than 20 character." : GoTo selesai
        End If

        'siekontak(8) As 
        If Len(dataUtama(8)) = 0 Then
            result(2) = "siekontak can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 20 Then
            result(2) = "siekontak should not be more than 20 character." : GoTo selesai
        End If

        'sietglnoref(19) As Date
        If Len(dataUtama(19)) = 0 Then
            result(2) = "sietglnoref can't be empty" : GoTo selesai
        End If

        'sieinputtgl(25) As DateTime
        If Len(dataUtama(25)) = 0 Then
            result(2) = "sieinputtgl can't be empty" : GoTo selesai
        End If

        'siemodifikasitgl(27) As DateTime
        If Len(dataUtama(27)) = 0 Then
            result(2) = "siemodifikasitgl can't be empty" : GoTo selesai
        End If

        'siepostingtgl(29) As DateTime
        If Len(dataUtama(29)) = 0 Then
            result(2) = "siepostingtgl can't be empty" : GoTo selesai
        End If

        'siecustomdbl1(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "siecustomdbl1 can't be empty" : GoTo selesai
        End If

        'siecustomdbl2(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "siecustomdbl2 can't be empty" : GoTo selesai
        End If

        'siecustomdbl3(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "siecustomdbl3 can't be empty" : GoTo selesai
        End If

        'siecustomdate1(42) As Date
        If Len(dataUtama(42)) = 0 Then
            result(2) = "siecustomdate1 can't be empty" : GoTo selesai
        End If

        'siecustomdate2(43) As Date
        If Len(dataUtama(43)) = 0 Then
            result(2) = "siecustomdate2 can't be empty" : GoTo selesai
        End If

        'siecustomdate3(44) As Date
        If Len(dataUtama(44)) = 0 Then
            result(2) = "siecustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "sieid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siecabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sielokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siesumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sieautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sienotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sietgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siekodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "siekontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "siekontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sie1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sie1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sie1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sie2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sie2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sie2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sieuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siecatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sienoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sietglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siestatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "siestatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "siejmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "siecetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sieinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sieinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siemodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "siemodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sieposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "siepostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sieisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "siecustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siecustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siecustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siecustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siecustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siecustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "siecustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "siecustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "siecustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siecustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siecustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siecustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siecustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siecustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahData(dtutama, "sieid~siecabang~sielokasi~siesumber~sieautonotransaksi~sienotransaksi~sietgl~siekodepa~siekontak~siekontakperson~sie1alamat1~sie1alamat2~sie1alamat3~sie2alamat1~sie2alamat2~sie2alamat3~sieuraian~siecatatan~sienoref~sietglnoref~siestatus~siestatussebelumnya~siejmlrevisi~siecetakanke~sieinputuser~sieinputtgl~siemodifikasiuser~siemodifikasitgl~sieposting~siepostingtgl~sieisclose~siecustomtext1~siecustomtext2~siecustomtext3~siecustomtext4~siecustomtext5~siecustomint1~siecustomint2~siecustomint3~siecustomdbl1~siecustomdbl2~siecustomdbl3~siecustomdate1~siecustomdate2~siecustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44))

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idsiedetail(0) As , idsie(1) As , sumber(2) As String, idtransaksi(3) As , catatan(4) As String, 
        'urutan(5) As Integer, isclose(6) As Integer, customtext1(7) As String, customtext2(8) As String, customtext3(9) As String, 
        'customdbl1(10) As Double, customdbl2(11) As Double, customdbl3(12) As Double, customdate1(13) As Date, customdate2(14) As Date, 
        'customdate3(15) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idsiedetail, idsie, sumber, idtransaksi, catatan, urutan, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idsiedetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idsie", AsEnumTypeData.AsInt64)
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
        Dim ftExistSI As String = "", ftBelumSieSI As String = ""
        Dim ftExistSR As String = "", ftBelumSieSR As String = ""


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
            'idsiedetail(0) As 
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idsiedetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idsiedetail should not be more than 20 character." : GoTo selesai
            End If

            'idsie(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idsie can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idsie should not be more than 20 character." : GoTo selesai
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

            AsDataTableTambahData(dtdetail, "idsiedetail~idsie~sumber~idtransaksi~catatan~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15))


            'VALIDASI OUTSTANDING ---------------------------------------
            'SET SUMBER DAN IDTRANSAKSI
            'sumber(2) As String,               idtransaksi(3) As
            sumberDetail = dataRowDetail(2) : idtransaksiDetail = dataRowDetail(3)

            Select Case sumberDetail
                Case "SI"
                    'CEK DATA EXIST
                    ftExistSI = IIf(Len(ftExistSI.ToString) = 0, "", ftExistSI & " UNION ")
                    ftExistSI = String.Concat(ftExistSI, "SELECT EXISTS(SELECT 1 FROM M5_Si WHERE siid = '" & idtransaksiDetail & "' AND sistatus IN(2,3,4,7) LIMIT 1) as rowExists, siid, sisumber, sinotransaksi FROM M5_Si WHERE siid = '" & idtransaksiDetail & "'")

                    'CEK OUTSTANDING
                    ftBelumSieSI = IIf(Len(ftBelumSieSI.ToString) = 0, "", ftBelumSieSI & " OR ")
                    ftBelumSieSI = String.Concat(ftBelumSieSI, " (sied.sumber = '" & FixQuotes(sumberDetail) & "' AND sied.idtransaksi = '" & FixDouble(idtransaksiDetail) & "') ")

                Case "SR"
                    'CEK DATA EXIST
                    ftExistSR = IIf(Len(ftExistSR.ToString) = 0, "", ftExistSR & " UNION ")
                    ftExistSR = String.Concat(ftExistSR, "SELECT EXISTS(SELECT 1 FROM M5_Sr WHERE srid = '" & idtransaksiDetail & "' AND srstatus IN(2,3,4,7) LIMIT 1) as rowExists, srid, srsumber, srnotransaksi FROM M5_Sr WHERE srid = '" & idtransaksiDetail & "'")

                    'CEK OUTSTANDING
                    ftBelumSieSR = IIf(Len(ftBelumSieSR.ToString) = 0, "", ftBelumSieSR & " OR ")
                    ftBelumSieSR = String.Concat(ftBelumSieSR, " (sied.sumber = '" & FixQuotes(sumberDetail) & "' AND sied.idtransaksi = '" & FixDouble(idtransaksiDetail) & "') ")
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
                Dim vModuleId As Integer = 5, vMenuId As Integer = 68
                Select Case drutama("siestatus")
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("sietgl")), AsFormatTanggal(drutama("sietgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================

                'VALIDASI SIMPAN =========================================
                If drutama("siestatus") = 2 Or drutama("siestatus") = 1 Or drutama("siestatus") = 8 Or drutama("siestatus") = 9 Or drutama("siestatus") = 10 Or drutama("siestatus") = 11 Then
                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistSI, ftBelumSieSI, ftExistSR, ftBelumSieSR)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN ==================================

                If isUpdate Then
                    result(4) = drutama("sieid")
                    notransaksi = drutama("sienotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(sieid), sienotransaksi FROM M5_sie WHERE sieid='" & result(4) & "' AND siestatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("sieautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("siecabang"), drutama("sielokasi"), drutama("siesumber"), drutama("sietgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(sieid) FROM M5_sie WHERE sienotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_sie_history
                        Dim rsSimpanHistory As String = SimpanHistory.M5_Sie_HistorySimpan("" & paramSplit(0) & "★M5_Sie_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("siesumber")) & "▼" & FixQuotes(drutama("sieid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Sie set siecabang  = '" & FixQuotes(drutama("siecabang")) & "', sielokasi  = '" & FixQuotes(drutama("sielokasi")) & "', siesumber  = '" & FixQuotes(drutama("siesumber")) & "', sieautonotransaksi  = " & drutama("sieautonotransaksi") & ", sienotransaksi  = '" & FixQuotes(notransaksi) & "', sietgl  = '" & FixQuotes(AsFormatTanggal(drutama("sietgl"))) & "', siekodepa  = '" & FixQuotes(drutama("siekodepa")) & "', siekontak  = '" & FixQuotes(drutama("siekontak")) & "', siekontakperson  = '" & FixQuotes(drutama("siekontakperson")) & "', sie1alamat1  = '" & FixQuotes(drutama("sie1alamat1")) & "', sie1alamat2  = '" & FixQuotes(drutama("sie1alamat2")) & "', sie1alamat3  = '" & FixQuotes(drutama("sie1alamat3")) & "', sie2alamat1  = '" & FixQuotes(drutama("sie2alamat1")) & "', sie2alamat2  = '" & FixQuotes(drutama("sie2alamat2")) & "', sie2alamat3  = '" & FixQuotes(drutama("sie2alamat3")) & "', sieuraian  = '" & FixQuotes(drutama("sieuraian")) & "', siecatatan  = '" & FixQuotes(drutama("siecatatan")) & "', sienoref  = '" & FixQuotes(drutama("sienoref")) & "', sietglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("sietglnoref"))) & "', siestatus  = " & drutama("siestatus") & ", siestatussebelumnya  = " & drutama("siestatussebelumnya") & ", siejmlrevisi  = " & drutama("siejmlrevisi") & ", siecetakanke  = " & drutama("siecetakanke") & ", sieinputuser  = '" & FixQuotes(drutama("sieinputuser")) & "', siemodifikasiuser  = '" & FixQuotes(drutama("siemodifikasiuser")) & "', siemodifikasitgl  = NOW(), sieposting  = " & drutama("sieposting") & ", siepostingtgl  = '" & FixQuotes(AsFormatTanggal(drutama("siepostingtgl"), "yyyy-MM-dd HH:mm:ss")) & "', siecustomtext1  = '" & FixQuotes(drutama("siecustomtext1")) & "', siecustomtext2  = '" & FixQuotes(drutama("siecustomtext2")) & "', siecustomtext3  = '" & FixQuotes(drutama("siecustomtext3")) & "', siecustomtext4  = '" & FixQuotes(drutama("siecustomtext4")) & "', siecustomtext5  = '" & FixQuotes(drutama("siecustomtext5")) & "', siecustomint1  = " & drutama("siecustomint1") & ", siecustomint2  = " & drutama("siecustomint2") & ", siecustomint3  = " & drutama("siecustomint3") & ", siecustomdbl1  = '" & FixDouble(drutama("siecustomdbl1")) & "', siecustomdbl2  = '" & FixDouble(drutama("siecustomdbl2")) & "', siecustomdbl3  = '" & FixDouble(drutama("siecustomdbl3")) & "', siecustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("siecustomdate1"))) & "', siecustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("siecustomdate2"))) & "', siecustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("siecustomdate3"))) & "' where sieid = " & drutama("sieid") & ""
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

                    If drutama("sieautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("siecabang"), drutama("sielokasi"), drutama("siesumber"), drutama("sietgl"))
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
                        notransaksi = drutama("sienotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(sieid) FROM M5_sie WHERE sienotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Sie (siecabang, sielokasi, siesumber, sieautonotransaksi, sienotransaksi, sietgl, siekodepa, siekontak, siekontakperson, sie1alamat1, sie1alamat2, sie1alamat3, sie2alamat1, sie2alamat2, sie2alamat3, sieuraian, siecatatan, sienoref, sietglnoref, siestatus, siestatussebelumnya, siejmlrevisi, siecetakanke, sieinputuser, sieinputtgl, siemodifikasiuser, siemodifikasitgl, sieposting, siepostingtgl, sieisclose, siecustomtext1, siecustomtext2, siecustomtext3, siecustomtext4, siecustomtext5, siecustomint1, siecustomint2, siecustomint3, siecustomdbl1, siecustomdbl2, siecustomdbl3, siecustomdate1, siecustomdate2, siecustomdate3) values('" & FixQuotes(drutama("siecabang")) & "', '" & FixQuotes(drutama("sielokasi")) & "', '" & FixQuotes(drutama("siesumber")) & "', " & drutama("sieautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("sietgl"))) & "', '" & FixQuotes(drutama("siekodepa")) & "', '" & FixQuotes(drutama("siekontak")) & "', '" & FixQuotes(drutama("siekontakperson")) & "', '" & FixQuotes(drutama("sie1alamat1")) & "', '" & FixQuotes(drutama("sie1alamat2")) & "', '" & FixQuotes(drutama("sie1alamat3")) & "', '" & FixQuotes(drutama("sie2alamat1")) & "', '" & FixQuotes(drutama("sie2alamat2")) & "', '" & FixQuotes(drutama("sie2alamat3")) & "', '" & FixQuotes(drutama("sieuraian")) & "', '" & FixQuotes(drutama("siecatatan")) & "', '" & FixQuotes(drutama("sienoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sietglnoref"))) & "', " & drutama("siestatus") & ", " & drutama("siestatussebelumnya") & ", " & drutama("siejmlrevisi") & ", " & drutama("siecetakanke") & ", '" & FixQuotes(drutama("sieinputuser")) & "', NOW(), '" & FixQuotes(drutama("siemodifikasiuser")) & "', '1971-01-01 00:00:00', " & drutama("sieposting") & ", NOW(), " & drutama("sieisclose") & ", '" & FixQuotes(drutama("siecustomtext1")) & "', '" & FixQuotes(drutama("siecustomtext2")) & "', '" & FixQuotes(drutama("siecustomtext3")) & "', '" & FixQuotes(drutama("siecustomtext4")) & "', '" & FixQuotes(drutama("siecustomtext5")) & "', " & drutama("siecustomint1") & ", " & drutama("siecustomint2") & ", " & drutama("siecustomint3") & ", '" & FixDouble(drutama("siecustomdbl1")) & "', '" & FixDouble(drutama("siecustomdbl2")) & "', '" & FixDouble(drutama("siecustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("siecustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("siecustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("siecustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select sieid from M5_sie where sienotransaksi='" & notransaksi & "' AND sieinputuser= '" & userid & "' order by siemodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Sie_Detail where idsie = '" & result(4) & "'"
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
                        strValue2.Append("('" & FixQuotes(dr1("idsiedetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("sumber")) & "', '" & FixQuotes(dr1("idtransaksi")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M5_Sie_Detail(idsiedetail, idsie, sumber, idtransaksi, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                If drutama("siestatus") = 2 Then
                    'UPDATE M5 SI (SET STATUSPIE DAN TGL JATUH TEMPO SESUAI SETTING BERDASARKAN TERMIN DAN TGL PIE)
                    sql = "UPDATE M5_sie sie JOIN M5_sie_detail sied ON sie.sieid = sied.idsie JOIN M5_Si si ON sied.sumber = si.sisumber AND sied.idtransaksi = si.siid LEFT JOIN m0_setting s ON s.smodule = 5 AND s.sgrup = 'tukarfaktur' AND s.skode = 'UpdateTglJatuhTempoSI' AND s.snilai = 1 LEFT JOIN m1_terms tr ON si.sitermin = tr.trkode SET si.sistatussie = 1, si.sitglsie = sie.sietgl, si.sitgljatuhtempo = (CASE IFNULL(s.snilai,0) WHEN 1 THEN DATE_ADD(sie.sietgl,INTERVAL IFNULL(tr.trharijatuhtempo,0) DAY) ELSE si.sitgljatuhtempo END) WHERE sie.sieid = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE M5 SR (SET STATUSPIE DAN TGL JATUH TEMPO SESUAI SETTING BERDASARKAN TERMIN DAN TGL PIE)
                    sql = "UPDATE M5_sie sie JOIN M5_sie_detail sied ON sie.sieid = sied.idsie JOIN M5_Sr sr ON sied.sumber = sr.srsumber AND sied.idtransaksi = sr.srid LEFT JOIN m0_setting s ON s.smodule = 5 AND s.sgrup = 'tukarfaktur' AND s.skode = 'UpdateTglJatuhTempoSR' AND s.snilai = 1 LEFT JOIN m1_terms tr ON sr.srtermin = tr.trkode SET sr.srstatussie = 1, sr.srtglsie = sie.sietgl, sr.srtgljatuhtempo = (CASE IFNULL(s.snilai,0) WHEN 1 THEN DATE_ADD(sie.sietgl,INTERVAL IFNULL(tr.trharijatuhtempo,0) DAY) ELSE sr.srtgljatuhtempo END) WHERE sie.sieid = '" & result(4) & "'"
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
                'If drutama("siestatus") = 2 Then
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
    Public Function M5_SieUpdateStatus(ByVal param As String) As String

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
            'Filter = Filter.Replace("siekontakkode", "c1.kkode")
            'Filter = Filter.Replace("siekontaknama", "c1.knama")
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
            Dim sumber As String = "Sie", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Sietgl, sienotransaksi, siestatus FROM M5_Sie WHERE Sieid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Siestatussebelumnya" : jnsaktivitas = 17
                'CEK STATUS TRANSAKSI, JIKA <> 7 MAKA TIDAK BISA UNCLOSE
                If statusTransaksi <> 7 Then result(2) = "Transaction has not closed, it can't be unclose." : Trans.Rollback() : GoTo selesai
            Else
                jnsaktivitas = nilaiStatus
            End If

            'SET ISDELETE = TRUE JIKA STATUS TRANSAKSI = 2/3/4/7 DAN JNS AKTIVITAS <> 7(CLOSE) & 17(UNCLOSE)
            If ((statusTransaksi = 2 Or statusTransaksi = 3 Or statusTransaksi = 4 Or statusTransaksi = 7) And jnsaktivitas <> 7 And jnsaktivitas <> 17) Then isDelete = True

            ''CEK PERIODE AKUNTANSI ==============================================================
            'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglTransaksi), AsFormatTanggal(tglTransaksi))
            'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
            ''END OF CEK PERIODE AKUNTANSI =======================================================

            'SIMPAN HISTORY ========================
            Dim SimpanHistory As New m5_sie_history
            Dim rsSimpanHistory As String = SimpanHistory.M5_Sie_HistorySimpan("" & paramSplit(0) & "★M5_Sie_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                'sql = query.PanggilQuery("M5_sie_terkait")
                'sql = sql.Replace("validtransaksi", idtransaksi)
                'Dim dtTerkait As DataTable = asdatatableambildaridbcon(sql)
                'dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                'If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================


                'UPDATE OUTSTANDING TRANSAKSI ===================================================
                'UPDATE M5 SI (SET STATUSPIE DAN TGL JATUH TEMPO SESUAI SETTING BERDASARKAN TERMIN DAN TGL PIE)
                sql = "UPDATE M5_sie sie JOIN M5_sie_detail sied ON sie.sieid = sied.idsie JOIN M5_Si si ON sied.sumber = si.sisumber AND sied.idtransaksi = si.siid LEFT JOIN m0_setting s ON s.smodule = 5 AND s.sgrup = 'tukarfaktur' AND s.skode = 'UpdateTglJatuhTempoSI' AND s.snilai = 1 LEFT JOIN m1_terms tr ON si.sitermin = tr.trkode SET si.sistatussie = 0, si.sitglsie = '1900-01-01', si.sitgljatuhtempo = (CASE IFNULL(s.snilai,0) WHEN 1 THEN '2100-12-31' ELSE si.sitgljatuhtempo END) WHERE sie.sieid = '" & idtransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'UPDATE M5 SR (SET STATUSPIE DAN TGL JATUH TEMPO SESUAI SETTING BERDASARKAN TERMIN DAN TGL PIE)
                sql = "UPDATE M5_sie sie JOIN M5_sie_detail sied ON sie.sieid = sied.idsie JOIN M5_Sr sr ON sied.sumber = sr.srsumber AND sied.idtransaksi = sr.srid LEFT JOIN m0_setting s ON s.smodule = 5 AND s.sgrup = 'tukarfaktur' AND s.skode = 'UpdateTglJatuhTempoSR' AND s.snilai = 1 LEFT JOIN m1_terms tr ON sr.srtermin = tr.trkode SET sr.srstatussie = 0, sr.srtglsie = '1900-01-01', sr.srtgljatuhtempo = (CASE IFNULL(s.snilai,0) WHEN 1 THEN '2100-12-31' ELSE sr.srtgljatuhtempo END) WHERE sie.sieid = '" & idtransaksi & "'"
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
            sql = "UPDATE M5_Sie SET Siestatus = " & nilaiStatus & ", Siemodifikasiuser='" & userid & "', Siemodifikasitgl = NOW(), Sieposting = 0, Siepostingtgl = '1971-01-01 00:00:00', Siejmlrevisi = Siejmlrevisi + 1 WHERE Sieid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_SieSearch(PostWsSearch(paramSplit(0), "M5_SieSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M5_SieDelete(ByVal param As String) As String

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
            'Filter = Filter.Replace("siekontakkode", "c1.kkode")
            'Filter = Filter.Replace("siekontaknama", "c1.knama")
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
            Dim sumber As String = "Sie", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Sieid, Sienotransaksi FROM M5_Sie WHERE Sieid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT siecabang, sielokasi, siesumber, sieautonotransaksi, sienotransaksi, sietgl"
            sql &= " FROM M5_sie"
            sql &= " WHERE sieid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("siecabang")
                lokasi = dtNomorNext.Rows(0)("sielokasi")
                sumber = dtNomorNext.Rows(0)("siesumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("sieautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("sienotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("sietgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M5_sie_Detail WHERE idsie = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M5_sie WHERE sieid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_SieSearch(PostWsSearch(paramSplit(0), "M5_SieSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M5_SieSearch(ByVal param As String) As String
        'M5_SieSearch --------------------------------------------------------
        'sieid, siecabang, sielokasi, siesumber, sienotransaksi, sietgl, sieuraian, 
        'siecatatan, siestatus, siestatussebelumnya, sieinputuser, sieinputtgl, siemodifikasiuser, siemodifikasitgl, 
        'siecabangnama, sielokasinama, siestatusnama, siestatussebelumnyanama, sieinputusernama, siemodifikasiusernama

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
            'Filter = Filter.Replace("pocustomerkode", "c1.kkode")
            'Filter = Filter.Replace("pocustomernama", "c1.knama")
            Filter = Filter.Replace("siecustomerkode", "c.kkode")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        If Filter.Length > 0 Then
            Filter = " where " + Filter
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = "select `sie`.`sieid` AS `sieid`,`sie`.`siecabang` AS `siecabang`,`sie`.`sielokasi` AS `sielokasi`,`sie`.`siesumber` AS `siesumber`,`sie`.`sienotransaksi` AS `sienotransaksi`,`sie`.`sietgl` AS `sietgl`,`sie`.`sieuraian` AS `sieuraian`,`sie`.`siecatatan` AS `siecatatan`,`sie`.`siestatus` AS `siestatus`,`sie`.`siestatussebelumnya` AS `siestatussebelumnya`,`sie`.`sieinputuser` AS `sieinputuser`,`sie`.`sieinputtgl` AS `sieinputtgl`,`sie`.`siemodifikasiuser` AS `siemodifikasiuser`,`sie`.`siemodifikasitgl` AS `siemodifikasitgl`,`br`.`bnama` AS `siecabangnama`,`lc`.`lnama` AS `sielokasinama`,`st1`.`nama` AS `siestatusnama`,`st2`.`nama` AS `siestatussebelumnyanama`,`u1`.`unama` AS `sieinputusernama`,`u2`.`unama` AS `siemodifikasiusernama` from ((((((`M5_sie` `sie` join `m1_branch` `br` on((`sie`.`siecabang` = `br`.`bkode`))) join `m1_location` `lc` on((`sie`.`sielokasi` = `lc`.`lkode`))) join `m0_status` `st1` on((`sie`.`siestatus` = `st1`.`kode`))) join `m0_status` `st2` on((`sie`.`siestatussebelumnya` = `st2`.`kode`))) join `m0_user` `u1` on((`sie`.`sieinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`sie`.`siemodifikasiuser` = `u2`.`userid`)))"
        sql = "select `sie`.`sieid` AS `sieid`,`sie`.`siecabang` AS `siecabang`,`sie`.`sielokasi` AS `sielokasi`,`sie`.`siesumber` AS `siesumber`,`sie`.`sienotransaksi` AS `sienotransaksi`,`sie`.`sietgl` AS `sietgl`,`sie`.`sieuraian` AS `sieuraian`,`sie`.`siecatatan` AS `siecatatan`,`sie`.`siestatus` AS `siestatus`,`sie`.`siestatussebelumnya` AS `siestatussebelumnya`,`sie`.`sieinputuser` AS `sieinputuser`,`sie`.`sieinputtgl` AS `sieinputtgl`,`sie`.`siemodifikasiuser` AS `siemodifikasiuser`,`sie`.`siemodifikasitgl` AS `siemodifikasitgl`,`br`.`bnama` AS `siecabangnama`,`lc`.`lnama` AS `sielokasinama`,`st1`.`nama` AS `siestatusnama`,`st2`.`nama` AS `siestatussebelumnyanama`,`u1`.`unama` AS `sieinputusernama`,`u2`.`unama` AS `siemodifikasiusernama`,`sie`.`siekontak` AS `siekontak` ,`c`.`kkode` AS `siecustomerkode` ,`c`.`knama` AS `siecustomernama` from (((((((((`M5_sie` `sie` join `m5_sie_detail` `sied` on((`sied`.`idsie` = `sie`.`sieid`))) left join `m5_si` `si` on((`sied`.`idtransaksi` = `si`.`siid` AND `sied`.`sumber` = `si`.`sisumber`))) left join `m1_contact` `c` on((`si`.`sicustomer` = `c`.`kid`))) join `m1_branch` `br` on((`sie`.`siecabang` = `br`.`bkode`))) join `m1_location` `lc` on((`sie`.`sielokasi` = `lc`.`lkode`))) join `m0_status` `st1` on((`sie`.`siestatus` = `st1`.`kode`))) join `m0_status` `st2` on((`sie`.`siestatussebelumnya` = `st2`.`kode`))) join `m0_user` `u1` on((`sie`.`sieinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`sie`.`siemodifikasiuser` = `u2`.`userid`))) " + Filter + " group by `sie`.`sieid`"


        'result(2) = "test " & sql

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M5_Po", "", Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("sieid"), ""), sptField,
                     FxDB(dr("siecabang"), ""), sptField,
                     FxDB(dr("sielokasi"), ""), sptField,
                     FxDB(dr("siesumber"), ""), sptField,
                     FxDB(dr("sienotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sietgl"), ""), formatTgl), sptField,
                     FxDB(dr("sieuraian"), ""), sptField,
                     FxDB(dr("siecatatan"), ""), sptField,
                     FxDB(dr("siestatus"), 0), sptField,
                     FxDB(dr("siestatussebelumnya"), 0), sptField,
                     FxDB(dr("sieinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sieinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("siemodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("siemodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("siecabangnama"), ""), sptField,
                     FxDB(dr("sielokasinama"), ""), sptField,
                     FxDB(dr("siestatusnama"), ""), sptField,
                     FxDB(dr("siestatussebelumnyanama"), ""), sptField,
                     FxDB(dr("sieinputusernama"), ""), sptField,
                     FxDB(dr("siemodifikasiusernama"), ""), sptField,
                     FxDB(dr("siekontak"), 0), sptField,
                     FxDB(dr("siecustomerkode"), ""), sptField,
                     FxDB(dr("siecustomernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sieid, siecabang, sielokasi, siesumber, sienotransaksi, sietgl, sieuraian, siecatatan, siestatus, siestatussebelumnya, sieinputuser, sieinputtgl, siemodifikasiuser, siemodifikasitgl, siecabangnama, sielokasinama, siestatusnama, siestatussebelumnyanama, sieinputusernama, siemodifikasiusernama, siekontak, siecustomerkode, siecustomernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SieSearch_custom(ByVal param As String) As String
        'M5_SieSearch --------------------------------------------------------
        'sieid, siecabang, sielokasi, siesumber, sienotransaksi, sietgl, sieuraian, 
        'siecatatan, siestatus, siestatussebelumnya, sieinputuser, sieinputtgl, siemodifikasiuser, siemodifikasitgl, 
        'siecabangnama, sielokasinama, siestatusnama, siestatussebelumnyanama, sieinputusernama, siemodifikasiusernama
        'siecustomer, siecustomerkode, siecustomernama

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
            'Filter = Filter.Replace("pocustomerkode", "c1.kkode")
            Filter = Filter.Replace("siecustomerkode", "c.kkode")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        If Filter.Length > 0 Then
            Filter = " where " + Filter
        End If


        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select `sie`.`sieid` AS `sieid`,`sie`.`siecabang` AS `siecabang`,`sie`.`sielokasi` AS `sielokasi`,`sie`.`siesumber` AS `siesumber`,`sie`.`sienotransaksi` AS `sienotransaksi`,`sie`.`sietgl` AS `sietgl`,`sie`.`sieuraian` AS `sieuraian`,`sie`.`siecatatan` AS `siecatatan`,`sie`.`siestatus` AS `siestatus`,`sie`.`siestatussebelumnya` AS `siestatussebelumnya`,`sie`.`sieinputuser` AS `sieinputuser`,`sie`.`sieinputtgl` AS `sieinputtgl`,`sie`.`siemodifikasiuser` AS `siemodifikasiuser`,`sie`.`siemodifikasitgl` AS `siemodifikasitgl`,`br`.`bnama` AS `siecabangnama`,`lc`.`lnama` AS `sielokasinama`,`st1`.`nama` AS `siestatusnama`,`st2`.`nama` AS `siestatussebelumnyanama`,`u1`.`unama` AS `sieinputusernama`,`u2`.`unama` AS `siemodifikasiusernama`,`sie`.`siekontak` AS `siekontak` ,`c`.`kkode` AS `siecustomerkode` ,`c`.`knama` AS `siecustomernama` from (((((((((`M5_sie` `sie` join `m5_sie_detail` `sied` on((`sied`.`idsie` = `sie`.`sieid`))) join `m5_si` `si` on((`sied`.`idtransaksi` = `si`.`siid` AND `sied`.`sumber` = `si`.`sisumber`))) join `m1_contact` `c` on((`si`.`sicustomer` = `c`.`kid`))) join `m1_branch` `br` on((`sie`.`siecabang` = `br`.`bkode`))) join `m1_location` `lc` on((`sie`.`sielokasi` = `lc`.`lkode`))) join `m0_status` `st1` on((`sie`.`siestatus` = `st1`.`kode`))) join `m0_status` `st2` on((`sie`.`siestatussebelumnya` = `st2`.`kode`))) join `m0_user` `u1` on((`sie`.`sieinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`sie`.`siemodifikasiuser` = `u2`.`userid`))) " + Filter + " group by `sie`.`sieid`"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M5_Po", "", Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("sieid"), ""), sptField,
                     FxDB(dr("siecabang"), ""), sptField,
                     FxDB(dr("sielokasi"), ""), sptField,
                     FxDB(dr("siesumber"), ""), sptField,
                     FxDB(dr("sienotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sietgl"), ""), formatTgl), sptField,
                     FxDB(dr("sieuraian"), ""), sptField,
                     FxDB(dr("siecatatan"), ""), sptField,
                     FxDB(dr("siestatus"), 0), sptField,
                     FxDB(dr("siestatussebelumnya"), 0), sptField,
                     FxDB(dr("sieinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sieinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("siemodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("siemodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("siecabangnama"), ""), sptField,
                     FxDB(dr("sielokasinama"), ""), sptField,
                     FxDB(dr("siestatusnama"), ""), sptField,
                     FxDB(dr("siestatussebelumnyanama"), ""), sptField,
                     FxDB(dr("sieinputusernama"), ""), sptField,
                     FxDB(dr("siemodifikasiusernama"), ""), sptField,
                     FxDB(dr("siekontak"), 0), sptField,
                     FxDB(dr("siecustomerkode"), ""), sptField,
                     FxDB(dr("siecustomernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sieid, siecabang, sielokasi, siesumber, sienotransaksi, sietgl, sieuraian, siecatatan, siestatus, siestatussebelumnya, sieinputuser, sieinputtgl, siemodifikasiuser, siemodifikasitgl, siecabangnama, sielokasinama, siestatusnama, siestatussebelumnyanama, sieinputusernama, siemodifikasiusernama, siekontak, siecustomerkode, siecustomernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SieSearchLama(ByVal param As String) As String
        'M5_SieSearch --------------------------------------------------------
        'sieid, siecabang, sielokasi, siesumber, sienotransaksi, sietgl, sieuraian, 
        'siecatatan, siestatus, siestatussebelumnya, sieinputuser, sieinputtgl, siemodifikasiuser, siemodifikasitgl, 
        'siecabangnama, sielokasinama, siestatusnama, siestatussebelumnyanama, sieinputusernama, siemodifikasiusernama

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
            'Filter = Filter.Replace("pocustomerkode", "c1.kkode")
            'Filter = Filter.Replace("pocustomernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select `sie`.`sieid` AS `sieid`,`sie`.`siecabang` AS `siecabang`,`sie`.`sielokasi` AS `sielokasi`,`sie`.`siesumber` AS `siesumber`,`sie`.`sienotransaksi` AS `sienotransaksi`,`sie`.`sietgl` AS `sietgl`,`sie`.`sieuraian` AS `sieuraian`,`sie`.`siecatatan` AS `siecatatan`,`sie`.`siestatus` AS `siestatus`,`sie`.`siestatussebelumnya` AS `siestatussebelumnya`,`sie`.`sieinputuser` AS `sieinputuser`,`sie`.`sieinputtgl` AS `sieinputtgl`,`sie`.`siemodifikasiuser` AS `siemodifikasiuser`,`sie`.`siemodifikasitgl` AS `siemodifikasitgl`,`br`.`bnama` AS `siecabangnama`,`lc`.`lnama` AS `sielokasinama`,`st1`.`nama` AS `siestatusnama`,`st2`.`nama` AS `siestatussebelumnyanama`,`u1`.`unama` AS `sieinputusernama`,`u2`.`unama` AS `siemodifikasiusernama` from ((((((`M5_sie` `sie` join `m1_branch` `br` on((`sie`.`siecabang` = `br`.`bkode`))) join `m1_location` `lc` on((`sie`.`sielokasi` = `lc`.`lkode`))) join `m0_status` `st1` on((`sie`.`siestatus` = `st1`.`kode`))) join `m0_status` `st2` on((`sie`.`siestatussebelumnya` = `st2`.`kode`))) join `m0_user` `u1` on((`sie`.`sieinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`sie`.`siemodifikasiuser` = `u2`.`userid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M5_Po", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("sieid"), ""), sptField,
                     FxDB(dr("siecabang"), ""), sptField,
                     FxDB(dr("sielokasi"), ""), sptField,
                     FxDB(dr("siesumber"), ""), sptField,
                     FxDB(dr("sienotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sietgl"), ""), formatTgl), sptField,
                     FxDB(dr("sieuraian"), ""), sptField,
                     FxDB(dr("siecatatan"), ""), sptField,
                     FxDB(dr("siestatus"), 0), sptField,
                     FxDB(dr("siestatussebelumnya"), 0), sptField,
                     FxDB(dr("sieinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sieinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("siemodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("siemodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("siecabangnama"), ""), sptField,
                     FxDB(dr("sielokasinama"), ""), sptField,
                     FxDB(dr("siestatusnama"), ""), sptField,
                     FxDB(dr("siestatussebelumnyanama"), ""), sptField,
                     FxDB(dr("sieinputusernama"), ""), sptField,
                     FxDB(dr("siemodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sieid, siecabang, sielokasi, siesumber, sienotransaksi, sietgl, sieuraian, siecatatan, siestatus, siestatussebelumnya, sieinputuser, sieinputtgl, siemodifikasiuser, siemodifikasitgl, siecabangnama, sielokasinama, siestatusnama, siestatussebelumnyanama, sieinputusernama, siemodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SieGetdataById(ByVal param As String) As String

        'M5_SieGetdataById Utama --------------------------------------------------------
        'sieid, siecabang, sielokasi, siesumber, sieautonotransaksi, sienotransaksi, sietgl, 
        'siekodepa, siekontak, siekontakperson, sie1alamat1, sie1alamat2, sie1alamat3, sie2alamat1, 
        'sie2alamat2, sie2alamat3, sieuraian, siecatatan, sienoref, sietglnoref, siestatus, 
        'siestatussebelumnya, siejmlrevisi, siecetakanke, sieinputuser, sieinputtgl, siemodifikasiuser, siemodifikasitgl, 
        'sieposting, siepostingtgl, sieisclose, siecustomtext1, siecustomtext2, siecustomtext3, siecustomtext4, 
        'siecustomtext5, siecustomint1, siecustomint2, siecustomint3, siecustomdbl1, siecustomdbl2, siecustomdbl3, 
        'siecustomdate1, siecustomdate2, siecustomdate3

        'M5_SieGetdataById Detail -------------------------------------------------------
        'idsiedetail, idsie, sumber, idtransaksi, 
        'catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3, cabang, lokasi, 
        'gudang, notransaksi, tgl, customer, customerkode, customernama, customerkontak, 
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

        Dim NmMemcached As String = "aplikasi1-M5_Pr~M5_Pr_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "sieid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "sieid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select `sie`.`sieid` AS `sieid`,`sie`.`siecabang` AS `siecabang`,`sie`.`sielokasi` AS `sielokasi`,`sie`.`siesumber` AS `siesumber`,`sie`.`sieautonotransaksi` AS `sieautonotransaksi`,`sie`.`sienotransaksi` AS `sienotransaksi`,`sie`.`sietgl` AS `sietgl`,`sie`.`siekodepa` AS `siekodepa`,`sie`.`siekontak` AS `siekontak`,`sie`.`siekontakperson` AS `siekontakperson`,`sie`.`sie1alamat1` AS `sie1alamat1`,`sie`.`sie1alamat2` AS `sie1alamat2`,`sie`.`sie1alamat3` AS `sie1alamat3`,`sie`.`sie2alamat1` AS `sie2alamat1`,`sie`.`sie2alamat2` AS `sie2alamat2`,`sie`.`sie2alamat3` AS `sie2alamat3`,`sie`.`sieuraian` AS `sieuraian`,`sie`.`siecatatan` AS `siecatatan`,`sie`.`sienoref` AS `sienoref`,`sie`.`sietglnoref` AS `sietglnoref`,`sie`.`siestatus` AS `siestatus`,`sie`.`siestatussebelumnya` AS `siestatussebelumnya`,`sie`.`siejmlrevisi` AS `siejmlrevisi`,`sie`.`siecetakanke` AS `siecetakanke`,`sie`.`sieinputuser` AS `sieinputuser`,`sie`.`sieinputtgl` AS `sieinputtgl`,`sie`.`siemodifikasiuser` AS `siemodifikasiuser`,`sie`.`siemodifikasitgl` AS `siemodifikasitgl`,`sie`.`sieposting` AS `sieposting`,`sie`.`siepostingtgl` AS `siepostingtgl`,`sie`.`sieisclose` AS `sieisclose`,`sie`.`siecustomtext1` AS `siecustomtext1`,`sie`.`siecustomtext2` AS `siecustomtext2`,`sie`.`siecustomtext3` AS `siecustomtext3`,`sie`.`siecustomtext4` AS `siecustomtext4`,`sie`.`siecustomtext5` AS `siecustomtext5`,`sie`.`siecustomint1` AS `siecustomint1`,`sie`.`siecustomint2` AS `siecustomint2`,`sie`.`siecustomint3` AS `siecustomint3`,`sie`.`siecustomdbl1` AS `siecustomdbl1`,`sie`.`siecustomdbl2` AS `siecustomdbl2`,`sie`.`siecustomdbl3` AS `siecustomdbl3`,`sie`.`siecustomdate1` AS `siecustomdate1`,`sie`.`siecustomdate2` AS `siecustomdate2`,`sie`.`siecustomdate3` AS `siecustomdate3`,`sied`.`idsiedetail` AS `idsiedetail`,`sied`.`idsie` AS `idsie`,`sied`.`sumber` AS `sumber`,`sied`.`idtransaksi` AS `idtransaksi`,`sied`.`catatan` AS `catatan`,`sied`.`urutan` AS `urutan`,`sied`.`isclose` AS `isclose`,`sied`.`customtext1` AS `customtext1`,`sied`.`customtext2` AS `customtext2`,`sied`.`customtext3` AS `customtext3`,`sied`.`customdbl1` AS `customdbl1`,`sied`.`customdbl2` AS `customdbl2`,`sied`.`customdbl3` AS `customdbl3`,`sied`.`customdate1` AS `customdate1`,`sied`.`customdate2` AS `customdate2`,`sied`.`customdate3` AS `customdate3`,ifnull(`si`.`sicabang`,`sr`.`srcabang`) AS `cabang`,ifnull(`si`.`silokasi`,`sr`.`srlokasi`) AS `lokasi`,ifnull(`si`.`sigudang`,`sr`.`srgudang`) AS `gudang`,ifnull(`si`.`sinotransaksi`,`sr`.`srnotransaksi`) AS `notransaksi`,ifnull(`si`.`sitgl`,`sr`.`srtgl`) AS `tgl`,ifnull(`si`.`sicustomer`,`sr`.`srcustomer`) AS `customer`,ifnull(`c`.`kkode`,'') AS `customerkode`,ifnull(`c`.`knama`,'') AS `customernama`,ifnull(`si`.`sicustomerkontak`,`sr`.`srcustomerkontak`) AS `customerkontak`,ifnull(`si`.`sitermin`,`sr`.`srtermin`) AS `termin`,ifnull(`si`.`siuraian`,`sr`.`sruraian`) AS `uraian`,ifnull(`si`.`simatauang`,`sr`.`srmatauang`) AS `matauang`,ifnull(`si`.`sikurs`,`sr`.`srkurs`) AS `kurs`,ifnull(`si`.`sitotaltransaksi`,`sr`.`srtotaltransaksi`) AS `totaltransaksi`,ifnull(`si`.`sijmlbayar`,`sr`.`srjmlbayar`) AS `jmlbayar` from ((((`M5_sie` `sie` join `M5_sie_detail` `sied` on((`sie`.`sieid` = `sied`.`idsie`))) left join `M5_Si` `si` on(((`sied`.`sumber` = `si`.`sisumber`) and (`sied`.`idtransaksi` = `si`.`siid`)))) left join `M5_Sr` `sr` on(((`sied`.`sumber` = `sr`.`srsumber`) and (`sied`.`idtransaksi` = `sr`.`srid`)))) left join `m1_contact` `c` on((ifnull(`si`.`sicustomer`,`sr`.`srcustomer`) = `c`.`kid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                     FxDB(drutama("sieid"), ""), sptField,
                     FxDB(drutama("siecabang"), ""), sptField,
                     FxDB(drutama("sielokasi"), ""), sptField,
                     FxDB(drutama("siesumber"), ""), sptField,
                     FxDB(drutama("sieautonotransaksi"), 0), sptField,
                     FxDB(drutama("sienotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sietgl"), ""), formatTgl), sptField,
                     FxDB(drutama("siekodepa"), ""), sptField,
                     FxDB(drutama("siekontak"), ""), sptField,
                     FxDB(drutama("siekontakperson"), ""), sptField,
                     FxDB(drutama("sie1alamat1"), ""), sptField,
                     FxDB(drutama("sie1alamat2"), ""), sptField,
                     FxDB(drutama("sie1alamat3"), ""), sptField,
                     FxDB(drutama("sie2alamat1"), ""), sptField,
                     FxDB(drutama("sie2alamat2"), ""), sptField,
                     FxDB(drutama("sie2alamat3"), ""), sptField,
                     FxDB(drutama("sieuraian"), ""), sptField,
                     FxDB(drutama("siecatatan"), ""), sptField,
                     FxDB(drutama("sienoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sietglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("siestatus"), 0), sptField,
                     FxDB(drutama("siestatussebelumnya"), 0), sptField,
                     FxDB(drutama("siejmlrevisi"), 0), sptField,
                     FxDB(drutama("siecetakanke"), 0), sptField,
                     FxDB(drutama("sieinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sieinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("siemodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("siemodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sieposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("siepostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sieisclose"), 0), sptField,
                     FxDB(drutama("siecustomtext1"), ""), sptField,
                     FxDB(drutama("siecustomtext2"), ""), sptField,
                     FxDB(drutama("siecustomtext3"), ""), sptField,
                     FxDB(drutama("siecustomtext4"), ""), sptField,
                     FxDB(drutama("siecustomtext5"), ""), sptField,
                     FxDB(drutama("siecustomint1"), 0), sptField,
                     FxDB(drutama("siecustomint2"), 0), sptField,
                     FxDB(drutama("siecustomint3"), 0), sptField,
                     FxDB(drutama("siecustomdbl1"), 0), sptField,
                     FxDB(drutama("siecustomdbl2"), 0), sptField,
                     FxDB(drutama("siecustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("siecustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("siecustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("siecustomdate3"), ""), formatTgl))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idsiedetail"), ""), sptField,
                     FxDB(dr("idsie"), ""), sptField,
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
                     FxDB(dr("customer"), ""), sptField,
                     FxDB(dr("customerkode"), ""), sptField,
                     FxDB(dr("customernama"), ""), sptField,
                     FxDB(dr("customerkontak"), ""), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sieid, siecabang, sielokasi, siesumber, sieautonotransaksi, sienotransaksi, sietgl, siekodepa, siekontak, siekontakperson, sie1alamat1, sie1alamat2, sie1alamat3, sie2alamat1, sie2alamat2, sie2alamat3, sieuraian, siecatatan, sienoref, sietglnoref, siestatus, siestatussebelumnya, siejmlrevisi, siecetakanke, sieinputuser, sieinputtgl, siemodifikasiuser, siemodifikasitgl, sieposting, siepostingtgl, sieisclose, siecustomtext1, siecustomtext2, siecustomtext3, siecustomtext4, siecustomtext5, siecustomint1, siecustomint2, siecustomint3, siecustomdbl1, siecustomdbl2, siecustomdbl3, siecustomdate1, siecustomdate2, siecustomdate3" & sptSubParam & "idsiedetail, idsie, sumber, idtransaksi, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, cabang, lokasi, gudang, notransaksi, tgl, customer, customerkode, customernama, customerkontak, termin, uraian, matauang, kurs, totaltransaksi, jmlbayar"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SieTakedataSearch(ByVal param As String) As String
        'M5_SieTakedataSearch --------------------------------------------------------
        'sumber, id, cabang, lokasi, gudang, notransaksi, tgl, 
        'customer, customerkode, customernama, customerkontak, termin, uraian, catatan, 
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
        sql = M5_SieTakedata_Query(Filter)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M5_Pr_Detail", "", Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
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
                     FxDB(dr("customer"), ""), sptField,
                     FxDB(dr("customerkode"), ""), sptField,
                     FxDB(dr("customernama"), ""), sptField,
                     FxDB(dr("customerkontak"), ""), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sumber, id, cabang, lokasi, gudang, notransaksi, tgl, customer, customerkode, customernama, customerkontak, termin, uraian, catatan, matauang, kurs, totaltransaksi, jmlbayar"))

        Return wsResult
    End Function

    Public Function M5_SieTakedata_Query(ByVal strFilter As String) As String
        Dim sql As String
        Dim filter1 As String = "", filter2 As String = ""

        'Replace Filter
        If (strFilter.Length > 0) Then
            filter1 = strFilter
            filter1 = filter1.Replace("sumber", "si.sisumber")
            filter1 = filter1.Replace("id", "si.siid")
            filter1 = filter1.Replace("notransaksi", "si.sinotransaksi")
            filter1 = filter1.Replace("tgl", "si.sitgl")
            filter1 = filter1.Replace("customerkode", "c1.kkode")
            filter1 = filter1.Replace("customernama", "c1.knama")
            filter1 = filter1.Replace("customer", "si.sicustomer")
            filter1 = filter1.Replace("customerkontak", "si.sicustomerkontak")
            filter1 = filter1.Replace("termin", "si.sitermin")
            filter1 = filter1.Replace("uraian", "si.siuraian")
            filter1 = filter1.Replace("catatan", "si.sicatatan")
            filter1 = filter1.Replace("matauang", "si.simatauang")
            filter1 = filter1.Replace("kurs", "si.sikurs")
            filter1 = filter1.Replace("totaltransaksi", "si.sitotaltransaksi")
            filter1 = filter1.Replace("jmlbayar", "si.sijmlbayar")
            'filter1 = filter1.Replace("status", "si.sistatus")
            filter1 = filter1.Replace("statussie", "si.sistatussie")

            filter2 = strFilter
            filter2 = filter2.Replace("sumber", "sr.srsumber")
            filter2 = filter2.Replace("id", "sr.srid")
            filter2 = filter2.Replace("notransaksi", "sr.srnotransaksi")
            filter2 = filter2.Replace("tgl", "sr.srtgl")
            filter2 = filter2.Replace("customerkode", "c1.kkode")
            filter2 = filter2.Replace("customernama", "c1.knama")
            filter2 = filter2.Replace("customer", "sr.srcustomer")
            filter2 = filter2.Replace("customerkontak", "sr.srcustomerkontak")
            filter2 = filter2.Replace("termin", "sr.srtermin")
            filter2 = filter2.Replace("uraian", "sr.sruraian")
            filter2 = filter2.Replace("catatan", "sr.srcatatan")
            filter2 = filter2.Replace("matauang", "sr.srmatauang")
            filter2 = filter2.Replace("kurs", "sr.srkurs")
            filter2 = filter2.Replace("totaltransaksi", "sr.srtotaltransaksi")
            filter2 = filter2.Replace("jmlbayar", "sr.srjmlbayar")
            'filter2 = filter2.Replace("status", "sr.srstatus")
            filter2 = filter2.Replace("statussie", "sr.srstatussie")

        End If


        filter1 = " WHERE si.sistatus IN(2,3,4,7) AND " & filter1

        filter2 = " WHERE sr.srstatus IN(2,3,4,7) AND " & filter2


        'SI
        sql = "  (select si.sisumber as sumber, si.siid AS id, si.sicabang AS cabang, si.silokasi AS lokasi, si.sigudang AS gudang, si.sinotransaksi AS notransaksi, si.sitgl AS tgl, si.sicustomer AS customer, c1.kkode AS customerkode, c1.knama AS customernama, si.sicustomerkontak AS customerkontak, si.sitermin AS termin, si.siuraian AS uraian, si.sicatatan AS catatan, si.simatauang AS matauang, si.sikurs AS kurs, si.sitotaltransaksi AS totaltransaksi, si.sijmlbayar AS jmlbayar from M5_Si si left join m1_contact c1 on si.sicustomer = c1.kid " & filter1 & ") "
        'SR
        sql &= " UNION ALL "
        sql &= " (select sr.srsumber as sumber, sr.srid AS id, sr.srcabang AS cabang, sr.srlokasi AS lokasi, sr.srgudang AS gudang, sr.srnotransaksi AS notransaksi, sr.srtgl AS tgl, sr.srcustomer AS customer, c1.kkode AS customerkode, c1.knama AS customernama, sr.srcustomerkontak AS customerkontak, sr.srtermin AS termin, sr.sruraian AS uraian, sr.srcatatan AS catatan, sr.srmatauang AS matauang, sr.srkurs AS kurs, sr.srtotaltransaksi AS totaltransaksi, sr.srjmlbayar AS jmlbayar from M5_Sr sr left join m1_contact c1 on sr.srcustomer = c1.kid " & filter2 & ") "

        Return sql
    End Function

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, _
                                    ByVal ftExistSI As String, ByVal ftBelumSieSI As String, _
                                    ByVal ftExistSR As String, ByVal ftBelumSieSR As String) As String

        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, sumber As String = "", notransaksi As String = "", matauang As String = "", tgl As String = ""
        Dim filterLookup As String = "", urutan As String = "", sisa As Double = 0


        'VALIDASI TRANSAKSI OUTSTANDING ------------------------------
        'SI
        If Len(ftExistSI) > 0 Then 'ftExistOutstanding = rowExists, siid, sisumber, sinotransaksi
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistSI)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                notransaksi = dtval.Rows(0)("sinotransaksi")
                sumber = dtval.Rows(0)("sisumber")

                filterLookup = "sumber = '" & dtval.Rows(0)("sisumber") & "' AND idtransaksi = '" & dtval.Rows(0)("siid") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " doesn't exists/yet approved in SI" : GoTo selesai
            End If
        End If

        'CEK SUDAH PIE ATAU BELUM
        If Len(ftBelumSieSI) > 0 Then
            sql = "  SELECT sie.sieid, sie.sienotransaksi, si.sisumber as sumber, si.siid as id, si.sinotransaksi as notransaksi "
            sql &= " FROM M5_sie sie "
            sql &= " JOIN M5_sie_detail sied ON sie.sieid = sied.idsie "
            sql &= " JOIN M5_Si si ON sied.sumber = si.sisumber AND sied.idtransaksi = si.siid "
            sql &= " WHERE sie.siestatus IN(2,3,4,7) "
            sql &= " AND (" & ftBelumSieSI & ") "
            sql &= " GROUP BY sie.sieid, si.siid "
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                notransaksi = dtval.Rows(0)("notransaksi")
                sumber = dtval.Rows(0)("sumber")

                filterLookup = "sumber = '" & sumber & "' AND idtransaksi = '" & dtval.Rows(0)("id") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " has related transaction on " & dtval.Rows(0)("sienotransaksi") & "" : GoTo selesai
            End If
        End If
        'END OF VALIDASI TRANSAKSI OUTSTANDING -----------------------


        'VALIDASI TRANSAKSI OUTSTANDING ------------------------------
        'SR
        If Len(ftExistSR) > 0 Then 'ftExistOutstanding = rowExists, srid, srsumber, srnotransaksi
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistSR)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                notransaksi = dtval.Rows(0)("srnotransaksi")
                sumber = dtval.Rows(0)("srsumber")

                filterLookup = "sumber = '" & dtval.Rows(0)("srsumber") & "' AND idtransaksi = '" & dtval.Rows(0)("srid") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " doesn't exists/yet approved in SR" : GoTo selesai
            End If
        End If

        'CEK SUDAH PIE ATAU BELUM
        If Len(ftBelumSieSR) > 0 Then
            sql = "  SELECT sie.sieid, sie.sienotransaksi, sr.srsumber as sumber, sr.srid as id, sr.srnotransaksi as notransaksi "
            sql &= " FROM M5_sie sie "
            sql &= " JOIN M5_sie_detail sied ON sie.sieid = sied.idsie "
            sql &= " JOIN M5_Sr sr ON sied.sumber = sr.srsumber AND sied.idtransaksi = sr.srid "
            sql &= " WHERE sie.siestatus IN(2,3,4,7) "
            sql &= " AND (" & ftBelumSieSR & ") "
            sql &= " GROUP BY sie.sieid, sr.srid "
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                notransaksi = dtval.Rows(0)("notransaksi")
                sumber = dtval.Rows(0)("sumber")

                filterLookup = "sumber = '" & sumber & "' AND idtransaksi = '" & dtval.Rows(0)("id") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " has related transaction on " & dtval.Rows(0)("sienotransaksi") & "" : GoTo selesai
            End If
        End If
        'END OF VALIDASI TRANSAKSI OUTSTANDING -----------------------

selesai:
        Return errmessage
    End Function

    <WebMethod()>
    Public Function M5_SieSimpanOld(ByVal param As String) As String
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
        'sieid(0) As , siecabang(1) As String, sielokasi(2) As String, siesumber(3) As String, sieautonotransaksi(4) As Integer, 
        'sienotransaksi(5) As String, sietgl(6) As Date, siekodepa(7) As , siekontak(8) As , siekontakperson(9) As String, 
        'sie1alamat1(10) As String, sie1alamat2(11) As String, sie1alamat3(12) As String, sie2alamat1(13) As String, sie2alamat2(14) As String, 
        'sie2alamat3(15) As String, sieuraian(16) As String, siecatatan(17) As String, sienoref(18) As String, sietglnoref(19) As Date, 
        'siestatus(20) As Integer, siestatussebelumnya(21) As Integer, siejmlrevisi(22) As Integer, siecetakanke(23) As Integer, sieinputuser(24) As , 
        'sieinputtgl(25) As DateTime, siemodifikasiuser(26) As , siemodifikasitgl(27) As DateTime, sieposting(28) As Integer, siepostingtgl(29) As DateTime, 
        'sieisclose(30) As Integer, siecustomtext1(31) As String, siecustomtext2(32) As String, siecustomtext3(33) As String, siecustomtext4(34) As String, 
        'siecustomtext5(35) As String, siecustomint1(36) As Integer, siecustomint2(37) As Integer, siecustomint3(38) As Integer, siecustomdbl1(39) As Double, 
        'siecustomdbl2(40) As Double, siecustomdbl3(41) As Double, siecustomdate1(42) As Date, siecustomdate2(43) As Date, siecustomdate3(44) As Date


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'sieid, siecabang, sielokasi, siesumber, sieautonotransaksi, sienotransaksi, sietgl, 
        'siekodepa, siekontak, siekontakperson, sie1alamat1, sie1alamat2, sie1alamat3, sie2alamat1, 
        'sie2alamat2, sie2alamat3, sieuraian, siecatatan, sienoref, sietglnoref, siestatus, 
        'siestatussebelumnya, siejmlrevisi, siecetakanke, sieinputuser, sieinputtgl, siemodifikasiuser, siemodifikasitgl, 
        'sieposting, siepostingtgl, sieisclose, siecustomtext1, siecustomtext2, siecustomtext3, siecustomtext4, 
        'siecustomtext5, siecustomint1, siecustomint2, siecustomint3, siecustomdbl1, siecustomdbl2, siecustomdbl3, 
        'siecustomdate1, siecustomdate2, siecustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 45) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'sieautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "sieautonotransaksi required numeric." : GoTo selesai
        End If
        'sietgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "sietgl required date." : GoTo selesai
        End If
        'sietglnoref(19) As Date
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "sietglnoref required date." : GoTo selesai
        End If
        'siestatus(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "siestatus required numeric." : GoTo selesai
        End If
        'siestatussebelumnya(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "siestatussebelumnya required numeric." : GoTo selesai
        End If
        'siejmlrevisi(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "siejmlrevisi required numeric." : GoTo selesai
        End If
        'siecetakanke(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "siecetakanke required numeric." : GoTo selesai
        End If
        'sieinputtgl(25) As DateTime
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "sieinputtgl required date." : GoTo selesai
        End If
        'siemodifikasitgl(27) As DateTime
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "siemodifikasitgl required date." : GoTo selesai
        End If
        'sieposting(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "sieposting required numeric." : GoTo selesai
        End If
        'siepostingtgl(29) As DateTime
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "siepostingtgl required date." : GoTo selesai
        End If
        'sieisclose(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "sieisclose required numeric." : GoTo selesai
        End If
        'siecustomint1(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "siecustomint1 required numeric." : GoTo selesai
        End If
        'siecustomint2(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "siecustomint2 required numeric." : GoTo selesai
        End If
        'siecustomint3(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "siecustomint3 required numeric." : GoTo selesai
        End If
        'siecustomdbl1(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "siecustomdbl1 required numeric." : GoTo selesai
        End If
        'siecustomdbl2(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "siecustomdbl2 required numeric." : GoTo selesai
        End If
        'siecustomdbl3(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "siecustomdbl3 required numeric." : GoTo selesai
        End If
        'siecustomdate1(42) As Date
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "siecustomdate1 required date." : GoTo selesai
        End If
        'siecustomdate2(43) As Date
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "siecustomdate2 required date." : GoTo selesai
        End If
        'siecustomdate3(44) As Date
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "siecustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'sieid(0) As 
        If Len(dataUtama(0)) = 0 Then
            result(2) = "sieid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 20 Then
            result(2) = "sieid should not be more than 20 character." : GoTo selesai
        End If

        'siecabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "siecabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "siecabang should not be more than 25 character." : GoTo selesai
        End If

        'sielokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "sielokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "sielokasi should not be more than 25 character." : GoTo selesai
        End If

        'siesumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "siesumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "siesumber should not be more than 10 character." : GoTo selesai
        End If

        'sienotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "sienotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "sienotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'sietgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "sietgl can't be empty" : GoTo selesai
        End If

        'siekodepa(7) As 
        If Len(dataUtama(7)) = 0 Then
            result(2) = "siekodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 20 Then
            result(2) = "siekodepa should not be more than 20 character." : GoTo selesai
        End If

        'siekontak(8) As 
        If Len(dataUtama(8)) = 0 Then
            result(2) = "siekontak can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 20 Then
            result(2) = "siekontak should not be more than 20 character." : GoTo selesai
        End If

        'sietglnoref(19) As Date
        If Len(dataUtama(19)) = 0 Then
            result(2) = "sietglnoref can't be empty" : GoTo selesai
        End If

        'sieinputtgl(25) As DateTime
        If Len(dataUtama(25)) = 0 Then
            result(2) = "sieinputtgl can't be empty" : GoTo selesai
        End If

        'siemodifikasitgl(27) As DateTime
        If Len(dataUtama(27)) = 0 Then
            result(2) = "siemodifikasitgl can't be empty" : GoTo selesai
        End If

        'siepostingtgl(29) As DateTime
        If Len(dataUtama(29)) = 0 Then
            result(2) = "siepostingtgl can't be empty" : GoTo selesai
        End If

        'siecustomdbl1(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "siecustomdbl1 can't be empty" : GoTo selesai
        End If

        'siecustomdbl2(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "siecustomdbl2 can't be empty" : GoTo selesai
        End If

        'siecustomdbl3(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "siecustomdbl3 can't be empty" : GoTo selesai
        End If

        'siecustomdate1(42) As Date
        If Len(dataUtama(42)) = 0 Then
            result(2) = "siecustomdate1 can't be empty" : GoTo selesai
        End If

        'siecustomdate2(43) As Date
        If Len(dataUtama(43)) = 0 Then
            result(2) = "siecustomdate2 can't be empty" : GoTo selesai
        End If

        'siecustomdate3(44) As Date
        If Len(dataUtama(44)) = 0 Then
            result(2) = "siecustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "sieid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siecabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sielokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siesumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sieautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sienotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sietgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siekodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "siekontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "siekontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sie1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sie1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sie1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sie2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sie2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sie2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sieuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siecatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sienoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sietglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siestatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "siestatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "siejmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "siecetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sieinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sieinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siemodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "siemodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sieposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "siepostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sieisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "siecustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siecustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siecustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siecustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siecustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siecustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "siecustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "siecustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "siecustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siecustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siecustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siecustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siecustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siecustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahData(dtutama, "sieid~siecabang~sielokasi~siesumber~sieautonotransaksi~sienotransaksi~sietgl~siekodepa~siekontak~siekontakperson~sie1alamat1~sie1alamat2~sie1alamat3~sie2alamat1~sie2alamat2~sie2alamat3~sieuraian~siecatatan~sienoref~sietglnoref~siestatus~siestatussebelumnya~siejmlrevisi~siecetakanke~sieinputuser~sieinputtgl~siemodifikasiuser~siemodifikasitgl~sieposting~siepostingtgl~sieisclose~siecustomtext1~siecustomtext2~siecustomtext3~siecustomtext4~siecustomtext5~siecustomint1~siecustomint2~siecustomint3~siecustomdbl1~siecustomdbl2~siecustomdbl3~siecustomdate1~siecustomdate2~siecustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44))

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idsiedetail(0) As , idsie(1) As , sumber(2) As String, idtransaksi(3) As , catatan(4) As String, 
        'urutan(5) As Integer, isclose(6) As Integer, customtext1(7) As String, customtext2(8) As String, customtext3(9) As String, 
        'customdbl1(10) As Double, customdbl2(11) As Double, customdbl3(12) As Double, customdate1(13) As Date, customdate2(14) As Date, 
        'customdate3(15) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idsiedetail, idsie, sumber, idtransaksi, catatan, urutan, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idsiedetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idsie", AsEnumTypeData.AsInt64)
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
        Dim ftExistSI As String = "", ftBelumSieSI As String = ""
        Dim ftExistSR As String = "", ftBelumSieSR As String = ""


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
            'idsiedetail(0) As 
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idsiedetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idsiedetail should not be more than 20 character." : GoTo selesai
            End If

            'idsie(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idsie can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idsie should not be more than 20 character." : GoTo selesai
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

            AsDataTableTambahData(dtdetail, "idsiedetail~idsie~sumber~idtransaksi~catatan~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15))


            'VALIDASI OUTSTANDING ---------------------------------------
            'SET SUMBER DAN IDTRANSAKSI
            'sumber(2) As String,               idtransaksi(3) As
            sumberDetail = dataRowDetail(2) : idtransaksiDetail = dataRowDetail(3)

            Select Case sumberDetail
                Case "SI"
                    'CEK DATA EXIST
                    ftExistSI = IIf(Len(ftExistSI.ToString) = 0, "", ftExistSI & " UNION ")
                    ftExistSI = String.Concat(ftExistSI, "SELECT EXISTS(SELECT 1 FROM M5_Si WHERE siid = '" & idtransaksiDetail & "' AND sistatus IN(2,3,4,7) LIMIT 1) as rowExists, siid, sisumber, sinotransaksi FROM M5_Si WHERE siid = '" & idtransaksiDetail & "'")

                    'CEK OUTSTANDING
                    ftBelumSieSI = IIf(Len(ftBelumSieSI.ToString) = 0, "", ftBelumSieSI & " OR ")
                    ftBelumSieSI = String.Concat(ftBelumSieSI, " (sied.sumber = '" & FixQuotes(sumberDetail) & "' AND sied.idtransaksi = '" & FixDouble(idtransaksiDetail) & "') ")

                Case "SR"
                    'CEK DATA EXIST
                    ftExistSR = IIf(Len(ftExistSR.ToString) = 0, "", ftExistSR & " UNION ")
                    ftExistSR = String.Concat(ftExistSR, "SELECT EXISTS(SELECT 1 FROM M5_Sr WHERE srid = '" & idtransaksiDetail & "' AND srstatus IN(2,3,4,7) LIMIT 1) as rowExists, srid, srsumber, srnotransaksi FROM M5_Sr WHERE srid = '" & idtransaksiDetail & "'")

                    'CEK OUTSTANDING
                    ftBelumSieSR = IIf(Len(ftBelumSieSR.ToString) = 0, "", ftBelumSieSR & " OR ")
                    ftBelumSieSR = String.Concat(ftBelumSieSR, " (sied.sumber = '" & FixQuotes(sumberDetail) & "' AND sied.idtransaksi = '" & FixDouble(idtransaksiDetail) & "') ")
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("sietgl")), AsFormatTanggal(drutama("sietgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================

                'VALIDASI SIMPAN =========================================
                If drutama("siestatus") = 2 Then
                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistSI, ftBelumSieSI, ftExistSR, ftBelumSieSR)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN ==================================

                If isUpdate Then
                    result(4) = drutama("sieid")
                    notransaksi = drutama("sienotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(sieid), sienotransaksi FROM M5_sie WHERE sieid='" & result(4) & "' AND siestatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(sieid) FROM M5_sie WHERE sienotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_sie_history
                        Dim rsSimpanHistory As String = SimpanHistory.M5_Sie_HistorySimpan("" & paramSplit(0) & "★M5_Sie_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("siesumber")) & "▼" & FixQuotes(drutama("sieid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Sie set siecabang  = '" & FixQuotes(drutama("siecabang")) & "', sielokasi  = '" & FixQuotes(drutama("sielokasi")) & "', siesumber  = '" & FixQuotes(drutama("siesumber")) & "', sieautonotransaksi  = " & drutama("sieautonotransaksi") & ", sienotransaksi  = '" & FixQuotes(notransaksi) & "', sietgl  = '" & FixQuotes(AsFormatTanggal(drutama("sietgl"))) & "', siekodepa  = '" & FixQuotes(drutama("siekodepa")) & "', siekontak  = '" & FixQuotes(drutama("siekontak")) & "', siekontakperson  = '" & FixQuotes(drutama("siekontakperson")) & "', sie1alamat1  = '" & FixQuotes(drutama("sie1alamat1")) & "', sie1alamat2  = '" & FixQuotes(drutama("sie1alamat2")) & "', sie1alamat3  = '" & FixQuotes(drutama("sie1alamat3")) & "', sie2alamat1  = '" & FixQuotes(drutama("sie2alamat1")) & "', sie2alamat2  = '" & FixQuotes(drutama("sie2alamat2")) & "', sie2alamat3  = '" & FixQuotes(drutama("sie2alamat3")) & "', sieuraian  = '" & FixQuotes(drutama("sieuraian")) & "', siecatatan  = '" & FixQuotes(drutama("siecatatan")) & "', sienoref  = '" & FixQuotes(drutama("sienoref")) & "', sietglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("sietglnoref"))) & "', siestatus  = " & drutama("siestatus") & ", siestatussebelumnya  = " & drutama("siestatussebelumnya") & ", siejmlrevisi  = " & drutama("siejmlrevisi") & ", siecetakanke  = " & drutama("siecetakanke") & ", sieinputuser  = '" & FixQuotes(drutama("sieinputuser")) & "', sieinputtgl  = '" & FixQuotes(AsFormatTanggal(drutama("sieinputtgl"), "yyyy-MM-dd HH:mm:ss")) & "', siemodifikasiuser  = '" & FixQuotes(drutama("siemodifikasiuser")) & "', siemodifikasitgl  = '" & FixQuotes(AsFormatTanggal(drutama("siemodifikasitgl"), "yyyy-MM-dd HH:mm:ss")) & "', sieposting  = " & drutama("sieposting") & ", siepostingtgl  = '" & FixQuotes(AsFormatTanggal(drutama("siepostingtgl"), "yyyy-MM-dd HH:mm:ss")) & "', siecustomtext1  = '" & FixQuotes(drutama("siecustomtext1")) & "', siecustomtext2  = '" & FixQuotes(drutama("siecustomtext2")) & "', siecustomtext3  = '" & FixQuotes(drutama("siecustomtext3")) & "', siecustomtext4  = '" & FixQuotes(drutama("siecustomtext4")) & "', siecustomtext5  = '" & FixQuotes(drutama("siecustomtext5")) & "', siecustomint1  = " & drutama("siecustomint1") & ", siecustomint2  = " & drutama("siecustomint2") & ", siecustomint3  = " & drutama("siecustomint3") & ", siecustomdbl1  = '" & FixDouble(drutama("siecustomdbl1")) & "', siecustomdbl2  = '" & FixDouble(drutama("siecustomdbl2")) & "', siecustomdbl3  = '" & FixDouble(drutama("siecustomdbl3")) & "', siecustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("siecustomdate1"))) & "', siecustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("siecustomdate2"))) & "', siecustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("siecustomdate3"))) & "' where sieid = " & drutama("sieid") & ""
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

                    If drutama("sieautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("siecabang"), drutama("sielokasi"), drutama("siesumber"), drutama("sietgl"))
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
                        notransaksi = drutama("sienotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(sieid) FROM M5_sie WHERE sienotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Sie (siecabang, sielokasi, siesumber, sieautonotransaksi, sienotransaksi, sietgl, siekodepa, siekontak, siekontakperson, sie1alamat1, sie1alamat2, sie1alamat3, sie2alamat1, sie2alamat2, sie2alamat3, sieuraian, siecatatan, sienoref, sietglnoref, siestatus, siestatussebelumnya, siejmlrevisi, siecetakanke, sieinputuser, sieinputtgl, siemodifikasiuser, siemodifikasitgl, sieposting, siepostingtgl, sieisclose, siecustomtext1, siecustomtext2, siecustomtext3, siecustomtext4, siecustomtext5, siecustomint1, siecustomint2, siecustomint3, siecustomdbl1, siecustomdbl2, siecustomdbl3, siecustomdate1, siecustomdate2, siecustomdate3) values('" & FixQuotes(drutama("siecabang")) & "', '" & FixQuotes(drutama("sielokasi")) & "', '" & FixQuotes(drutama("siesumber")) & "', " & drutama("sieautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("sietgl"))) & "', '" & FixQuotes(drutama("siekodepa")) & "', '" & FixQuotes(drutama("siekontak")) & "', '" & FixQuotes(drutama("siekontakperson")) & "', '" & FixQuotes(drutama("sie1alamat1")) & "', '" & FixQuotes(drutama("sie1alamat2")) & "', '" & FixQuotes(drutama("sie1alamat3")) & "', '" & FixQuotes(drutama("sie2alamat1")) & "', '" & FixQuotes(drutama("sie2alamat2")) & "', '" & FixQuotes(drutama("sie2alamat3")) & "', '" & FixQuotes(drutama("sieuraian")) & "', '" & FixQuotes(drutama("siecatatan")) & "', '" & FixQuotes(drutama("sienoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sietglnoref"))) & "', " & drutama("siestatus") & ", " & drutama("siestatussebelumnya") & ", " & drutama("siejmlrevisi") & ", " & drutama("siecetakanke") & ", '" & FixQuotes(drutama("sieinputuser")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sieinputtgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(drutama("siemodifikasiuser")) & "', '" & FixQuotes(AsFormatTanggal(drutama("siemodifikasitgl"), "yyyy-MM-dd HH:mm:ss")) & "', " & drutama("sieposting") & ", '" & FixQuotes(AsFormatTanggal(drutama("siepostingtgl"), "yyyy-MM-dd HH:mm:ss")) & "', " & drutama("sieisclose") & ", '" & FixQuotes(drutama("siecustomtext1")) & "', '" & FixQuotes(drutama("siecustomtext2")) & "', '" & FixQuotes(drutama("siecustomtext3")) & "', '" & FixQuotes(drutama("siecustomtext4")) & "', '" & FixQuotes(drutama("siecustomtext5")) & "', " & drutama("siecustomint1") & ", " & drutama("siecustomint2") & ", " & drutama("siecustomint3") & ", '" & FixDouble(drutama("siecustomdbl1")) & "', '" & FixDouble(drutama("siecustomdbl2")) & "', '" & FixDouble(drutama("siecustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("siecustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("siecustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("siecustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select sieid from M5_sie where sienotransaksi='" & notransaksi & "' AND sieinputuser= '" & userid & "' order by siemodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Sie_Detail where idsie = '" & result(4) & "'"
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
                        strValue2.Append("('" & FixQuotes(dr1("idsiedetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("sumber")) & "', '" & FixQuotes(dr1("idtransaksi")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M5_Sie_Detail(idsiedetail, idsie, sumber, idtransaksi, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                If drutama("siestatus") = 2 Then
                    'UPDATE M5 SI (SET STATUSPIE DAN TGL JATUH TEMPO SESUAI SETTING BERDASARKAN TERMIN DAN TGL PIE)
                    sql = "UPDATE M5_sie sie JOIN M5_sie_detail sied ON sie.sieid = sied.idsie JOIN M5_Si si ON sied.sumber = si.sisumber AND sied.idtransaksi = si.siid LEFT JOIN m0_setting s ON s.smodule = 5 AND s.sgrup = 'tukarfaktur' AND s.skode = 'UpdateTglJatuhTempoSI' AND s.snilai = 1 LEFT JOIN m1_terms tr ON si.sitermin = tr.trkode SET si.sistatussie = 1, si.sitglsie = sie.sietgl, si.sitgljatuhtempo = (CASE IFNULL(s.snilai,0) WHEN 1 THEN DATE_ADD(sie.sietgl,INTERVAL IFNULL(tr.trharijatuhtempo,0) DAY) ELSE si.sitgljatuhtempo END) WHERE sie.sieid = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE M5 SR (SET STATUSPIE DAN TGL JATUH TEMPO SESUAI SETTING BERDASARKAN TERMIN DAN TGL PIE)
                    sql = "UPDATE M5_sie sie JOIN M5_sie_detail sied ON sie.sieid = sied.idsie JOIN M5_Sr sr ON sied.sumber = sr.srsumber AND sied.idtransaksi = sr.srid LEFT JOIN m0_setting s ON s.smodule = 5 AND s.sgrup = 'tukarfaktur' AND s.skode = 'UpdateTglJatuhTempoSR' AND s.snilai = 1 LEFT JOIN m1_terms tr ON sr.srtermin = tr.trkode SET sr.srstatussie = 1, sr.srtglsie = sie.sietgl, sr.srtgljatuhtempo = (CASE IFNULL(s.snilai,0) WHEN 1 THEN DATE_ADD(sie.sietgl,INTERVAL IFNULL(tr.trharijatuhtempo,0) DAY) ELSE sr.srtgljatuhtempo END) WHERE sie.sieid = '" & result(4) & "'"
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
                'If drutama("siestatus") = 2 Then
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
    Public Function M5_SieUpdateStatusOld(ByVal param As String) As String

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
            'Filter = Filter.Replace("siekontakkode", "c1.kkode")
            'Filter = Filter.Replace("siekontaknama", "c1.knama")
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
            Dim sumber As String = "Sie", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Sietgl, sienotransaksi, siestatus FROM M5_Sie WHERE Sieid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Siestatussebelumnya" : jnsaktivitas = 17
                'CEK STATUS TRANSAKSI, JIKA <> 7 MAKA TIDAK BISA UNCLOSE
                If statusTransaksi <> 7 Then result(2) = "Transaction has not closed, it can't be unclose." : Trans.Rollback() : GoTo selesai
            Else
                jnsaktivitas = nilaiStatus
            End If

            'SET ISDELETE = TRUE JIKA STATUS TRANSAKSI = 2/3/4/7 DAN JNS AKTIVITAS <> 7(CLOSE) & 17(UNCLOSE)
            If ((statusTransaksi = 2 Or statusTransaksi = 3 Or statusTransaksi = 4 Or statusTransaksi = 7) And jnsaktivitas <> 7 And jnsaktivitas <> 17) Then isDelete = True

            ''CEK PERIODE AKUNTANSI ==============================================================
            'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglTransaksi), AsFormatTanggal(tglTransaksi))
            'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
            ''END OF CEK PERIODE AKUNTANSI =======================================================

            'SIMPAN HISTORY ========================
            Dim SimpanHistory As New m5_sie_history
            Dim rsSimpanHistory As String = SimpanHistory.M5_Sie_HistorySimpan("" & paramSplit(0) & "★M5_Sie_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                'sql = query.PanggilQuery("M5_sie_terkait")
                'sql = sql.Replace("validtransaksi", idtransaksi)
                'Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                'dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                'If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================


                'UPDATE OUTSTANDING TRANSAKSI ===================================================
                'UPDATE M5 SI (SET STATUSPIE DAN TGL JATUH TEMPO SESUAI SETTING BERDASARKAN TERMIN DAN TGL PIE)
                sql = "UPDATE M5_sie sie JOIN M5_sie_detail sied ON sie.sieid = sied.idsie JOIN M5_Si si ON sied.sumber = si.sisumber AND sied.idtransaksi = si.siid LEFT JOIN m0_setting s ON s.smodule = 5 AND s.sgrup = 'tukarfaktur' AND s.skode = 'UpdateTglJatuhTempoSI' AND s.snilai = 1 LEFT JOIN m1_terms tr ON si.sitermin = tr.trkode SET si.sistatussie = 0, si.sitglsie = '1900-01-01', si.sitgljatuhtempo = (CASE IFNULL(s.snilai,0) WHEN 1 THEN '2100-12-31' ELSE si.sitgljatuhtempo END) WHERE sie.sieid = '" & idtransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'UPDATE M5 SR (SET STATUSPIE DAN TGL JATUH TEMPO SESUAI SETTING BERDASARKAN TERMIN DAN TGL PIE)
                sql = "UPDATE M5_sie sie JOIN M5_sie_detail sied ON sie.sieid = sied.idsie JOIN M5_Sr sr ON sied.sumber = sr.srsumber AND sied.idtransaksi = sr.srid LEFT JOIN m0_setting s ON s.smodule = 5 AND s.sgrup = 'tukarfaktur' AND s.skode = 'UpdateTglJatuhTempoSR' AND s.snilai = 1 LEFT JOIN m1_terms tr ON sr.srtermin = tr.trkode SET sr.srstatussie = 0, sr.srtglsie = '1900-01-01', sr.srtgljatuhtempo = (CASE IFNULL(s.snilai,0) WHEN 1 THEN '2100-12-31' ELSE sr.srtgljatuhtempo END) WHERE sie.sieid = '" & idtransaksi & "'"
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
            sql = "UPDATE M5_Sie SET Siestatus = " & nilaiStatus & ", Siemodifikasiuser='" & userid & "', Siemodifikasitgl = NOW(), Sieposting = 0, Siepostingtgl = '1971-01-01 00:00:00', Siejmlrevisi = Siejmlrevisi + 1 WHERE Sieid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_SieSearch(PostWsSearch(paramSplit(0), "M5_SieSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M5_SieDeleteOld(ByVal param As String) As String

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
            'Filter = Filter.Replace("siekontakkode", "c1.kkode")
            'Filter = Filter.Replace("siekontaknama", "c1.knama")
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
            Dim sumber As String = "Sie", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Sieid, Sienotransaksi FROM M5_Sie WHERE Sieid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT siecabang, sielokasi, siesumber, sieautonotransaksi, sienotransaksi, sietgl"
            sql &= " FROM M5_sie"
            sql &= " WHERE sieid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("siecabang")
                lokasi = dtNomorNext.Rows(0)("sielokasi")
                sumber = dtNomorNext.Rows(0)("siesumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("sieautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("sienotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("sietgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M5_sie_Detail WHERE idsie = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M5_sie WHERE sieid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_SieSearch(PostWsSearch(paramSplit(0), "M5_SieSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
