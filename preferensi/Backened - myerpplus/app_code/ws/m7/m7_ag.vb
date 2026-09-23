Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m7_ag
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M7_AgSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataMaster(), dataRowMaster() As String

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
        'agid(0) As , agcabang(1) As String, aglokasi(2) As String, agsumber(3) As String, agautonotransaksi(4) As Integer, 
        'agnotransaksi(5) As String, agtgl(6) As Date, agkodepa(7) As , agbagianag(8) As , agbagianagkontak(9) As String, 
        'agmatauang(10) As String, agkurs(11) As Double, aguraian(12) As String, agcatatan(13) As String, agnoref(14) As String, 
        'agtglnoref(15) As Date, agstatus(16) As Integer, agstatussebelumnya(17) As Integer, agjmlrevisi(18) As Integer, agcetakanke(19) As Integer, 
        'aginputuser(20) As , aginputtgl(21) As DateTime, agmodifikasiuser(22) As , agmodifikasitgl(23) As DateTime, agposting(24) As Integer, 
        'agpostingtgl(25) As DateTime, agtutupperiode(26) As Integer, agisclose(27) As Integer, agcustomtext1(28) As String, agcustomtext2(29) As String, 
        'agcustomtext3(30) As String, agcustomtext4(31) As String, agcustomtext5(32) As String, agcustomint1(33) As Integer, agcustomint2(34) As Integer, 
        'agcustomint3(35) As Integer, agcustomdbl1(36) As Double, agcustomdbl2(37) As Double, agcustomdbl3(38) As Double, agcustomdate1(39) As Date, 
        'agcustomdate2(40) As Date, agcustomdate3(41) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'agid, agcabang, aglokasi, agsumber, agautonotransaksi, agnotransaksi, agtgl, 
        'agkodepa, agbagianag, agbagianagkontak, agmatauang, agkurs, aguraian, agcatatan, 
        'agnoref, agtglnoref, agstatus, agstatussebelumnya, agjmlrevisi, agcetakanke, aginputuser, 
        'aginputtgl, agmodifikasiuser, agmodifikasitgl, agposting, agpostingtgl, agtutupperiode, agisclose, 
        'agcustomtext1, agcustomtext2, agcustomtext3, agcustomtext4, agcustomtext5, agcustomint1, agcustomint2, 
        'agcustomint3, agcustomdbl1, agcustomdbl2, agcustomdbl3, agcustomdate1, agcustomdate2, agcustomdate3



        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 42) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'agautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "agautonotransaksi required numeric." : GoTo selesai
        End If
        'agtgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "agtgl required date." : GoTo selesai
        End If
        'agkurs(11) As Double
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "agkurs required numeric." : GoTo selesai
        End If
        'agtglnoref(15) As Date
        If (IsDate(dataUtama(15)) = False) Then
            result(2) = "agtglnoref required date." : GoTo selesai
        End If
        'agstatus(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "agstatus required numeric." : GoTo selesai
        End If
        'agstatussebelumnya(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "agstatussebelumnya required numeric." : GoTo selesai
        End If
        'agjmlrevisi(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "agjmlrevisi required numeric." : GoTo selesai
        End If
        'agcetakanke(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "agcetakanke required numeric." : GoTo selesai
        End If
        'aginputtgl(21) As DateTime
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "aginputtgl required date." : GoTo selesai
        End If
        'agmodifikasitgl(23) As DateTime
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "agmodifikasitgl required date." : GoTo selesai
        End If
        'agposting(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "agposting required numeric." : GoTo selesai
        End If
        'agpostingtgl(25) As DateTime
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "agpostingtgl required date." : GoTo selesai
        End If
        'agtutupperiode(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "agtutupperiode required numeric." : GoTo selesai
        End If
        'agisclose(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "agisclose required numeric." : GoTo selesai
        End If
        'agcustomint1(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "agcustomint1 required numeric." : GoTo selesai
        End If
        'agcustomint2(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "agcustomint2 required numeric." : GoTo selesai
        End If
        'agcustomint3(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "agcustomint3 required numeric." : GoTo selesai
        End If
        'agcustomdbl1(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "agcustomdbl1 required numeric." : GoTo selesai
        End If
        'agcustomdbl2(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "agcustomdbl2 required numeric." : GoTo selesai
        End If
        'agcustomdbl3(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "agcustomdbl3 required numeric." : GoTo selesai
        End If
        'agcustomdate1(39) As Date
        If (IsDate(dataUtama(39)) = False) Then
            result(2) = "agcustomdate1 required date." : GoTo selesai
        End If
        'agcustomdate2(40) As Date
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "agcustomdate2 required date." : GoTo selesai
        End If
        'agcustomdate3(41) As Date
        If (IsDate(dataUtama(41)) = False) Then
            result(2) = "agcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'agid(0) As 
        If Len(dataUtama(0)) = 0 Then
            result(2) = "agid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 20 Then
            result(2) = "agid should not be more than 20 character." : GoTo selesai
        End If

        'agcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "agcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "agcabang should not be more than 25 character." : GoTo selesai
        End If

        'aglokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "aglokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "aglokasi should not be more than 25 character." : GoTo selesai
        End If

        'agsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "agsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "agsumber should not be more than 10 character." : GoTo selesai
        End If

        'agnotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "agnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "agnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'agtgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "agtgl can't be empty" : GoTo selesai
        End If

        'agkodepa(7) As 
        If Len(dataUtama(7)) = 0 Then
            result(2) = "agkodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 20 Then
            result(2) = "agkodepa should not be more than 20 character." : GoTo selesai
        End If

        'agbagianag(8) As 
        If Len(dataUtama(8)) = 0 Then
            result(2) = "agbagianag can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 20 Then
            result(2) = "agbagianag should not be more than 20 character." : GoTo selesai
        End If

        'agmatauang(10) As String
        If Len(dataUtama(10)) = 0 Then
            result(2) = "agmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(10)) > 25 Then
            result(2) = "agmatauang should not be more than 25 character." : GoTo selesai
        End If

        'agkurs(11) As Double
        If Len(dataUtama(11)) = 0 Then
            result(2) = "agkurs can't be empty" : GoTo selesai
        End If

        'agtglnoref(15) As Date
        If Len(dataUtama(15)) = 0 Then
            result(2) = "agtglnoref can't be empty" : GoTo selesai
        End If

        'aginputuser(20) As 
        If Len(dataUtama(20)) = 0 Then
            result(2) = "aginputuser can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(20)) > 20 Then
            result(2) = "aginputuser should not be more than 20 character." : GoTo selesai
        End If

        'aginputtgl(21) As DateTime
        If Len(dataUtama(21)) = 0 Then
            result(2) = "aginputtgl can't be empty" : GoTo selesai
        End If

        'agmodifikasiuser(22) As 
        If Len(dataUtama(22)) = 0 Then
            result(2) = "agmodifikasiuser can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(22)) > 20 Then
            result(2) = "agmodifikasiuser should not be more than 20 character." : GoTo selesai
        End If

        'agmodifikasitgl(23) As DateTime
        If Len(dataUtama(23)) = 0 Then
            result(2) = "agmodifikasitgl can't be empty" : GoTo selesai
        End If

        'agpostingtgl(25) As DateTime
        If Len(dataUtama(25)) = 0 Then
            result(2) = "agpostingtgl can't be empty" : GoTo selesai
        End If

        'agcustomdbl1(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "agcustomdbl1 can't be empty" : GoTo selesai
        End If

        'agcustomdbl2(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "agcustomdbl2 can't be empty" : GoTo selesai
        End If

        'agcustomdbl3(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "agcustomdbl3 can't be empty" : GoTo selesai
        End If

        'agcustomdate1(39) As Date
        If Len(dataUtama(39)) = 0 Then
            result(2) = "agcustomdate1 can't be empty" : GoTo selesai
        End If

        'agcustomdate2(40) As Date
        If Len(dataUtama(40)) = 0 Then
            result(2) = "agcustomdate2 can't be empty" : GoTo selesai
        End If

        'agcustomdate3(41) As Date
        If Len(dataUtama(41)) = 0 Then
            result(2) = "agcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "agid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "agcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aglokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "agsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "agautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "agnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "agtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "agkodepa", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "agbagianag", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "agbagianagkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "agmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "agkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aguraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "agcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "agnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "agtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "agstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "agstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "agjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "agcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aginputuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "aginputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "agmodifikasiuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "agmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "agposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "agpostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "agtutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "agisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "agcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "agcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "agcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "agcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "agcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "agcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "agcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "agcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "agcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "agcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "agcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "agcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "agcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "agcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "agid~agcabang~aglokasi~agsumber~agautonotransaksi~agnotransaksi~agtgl~agkodepa~agbagianag~agbagianagkontak~agmatauang~agkurs~aguraian~agcatatan~agnoref~agtglnoref~agstatus~agstatussebelumnya~agjmlrevisi~agcetakanke~aginputuser~aginputtgl~agmodifikasiuser~agmodifikasitgl~agposting~agpostingtgl~agtutupperiode~agisclose~agcustomtext1~agcustomtext2~agcustomtext3~agcustomtext4~agcustomtext5~agcustomint1~agcustomint2~agcustomint3~agcustomdbl1~agcustomdbl2~agcustomdbl3~agcustomdate1~agcustomdate2~agcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idagdetail(0) As , idag(1) As , idasset(2) As , namaasset(3) As String, jml(4) As Double, 
        'matauang(5) As String, kurs(6) As Double, hargabeli(7) As Double, rekasset(8) As String, cabang(9) As String, 
        'lokasi(10) As String, costcenter(11) As String, divisi(12) As String, subdivisi(13) As String, proyek(14) As String, 
        'catatan(15) As String, urutan(16) As Integer, isclose(17) As Integer, customtext1(18) As String, customtext2(19) As String, 
        'customtext3(20) As String, customdbl1(21) As Double, customdbl2(22) As Double, customdbl3(23) As Double, customdate1(24) As Date, 
        'customdate2(25) As Date, customdate3(26) As Date, satuan(27) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idagdetail, idag, idasset, namaasset, jml, matauang, kurs, 
        'hargabeli, rekasset, cabang, lokasi, costcenter, divisi, subdivisi, 
        'proyek, catatan, urutan, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, satuan

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idagdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idag", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idasset", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "namaasset", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargabeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekasset", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
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
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 28) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'jml(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'kurs(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'hargabeli(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - hargabeli required numeric." : GoTo selesai
            End If
            'urutan(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
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
            'idagdetail(0) As 
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idagdetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idagdetail should not be more than 20 character." : GoTo selesai
            End If

            'idag(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idag can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idag should not be more than 20 character." : GoTo selesai
            End If

            'idasset(2) As 
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - idasset can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 20 Then
                result(2) = "Row : " & i & " - idasset should not be more than 20 character." : GoTo selesai
            End If

            'namaasset(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - namaasset can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 100 Then
                result(2) = "Row : " & i & " - namaasset should not be more than 100 character." : GoTo selesai
            End If

            'jml(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - jml can't be empty" : GoTo selesai
            End If

            'matauang(5) As String
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(5)) > 25 Then
                result(2) = "Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'hargabeli(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - hargabeli can't be empty" : GoTo selesai
            End If

            'rekasset(8) As String
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - rekasset can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(8)) > 25 Then
                result(2) = "Row : " & i & " - rekasset should not be more than 25 character." : GoTo selesai
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

            'customdate3(27) As Date
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idagdetail~idag~idasset~namaasset~jml~matauang~kurs~hargabeli~rekasset~cabang~lokasi~costcenter~divisi~subdivisi~proyek~catatan~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~satuan", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'aid(0) As Integer, akode(1) As String, anama(2) As String, akategori(3) As String, acabang(4) As String, 
        'alokasi(5) As String, adivisi(6) As String, asubdivisi(7) As String, acatatan(8) As String, anomor(9) As String, 
        'atglbeli(10) As Date, atglpakai(11) As Date, amatauang(12) As String, akurs(13) As Double, ahargabeli(14) As Double, 
        'anilairesidu(15) As Double, aumurekonomis(16) As Double, abebanperbln(17) As Double, aakumulasibeban(18) As Double, anilaibuku(19) As Double, 
        'ametode(20) As Integer, atabelpenyusutan(21) As String, aintangible(22) As Integer, afiskal(23) As Integer, aatastengahbulan(24) As Integer, 
        'arekasset(25) As String, arekakumdepresiasi(26) As String, arekdepresiasi(27) As String, arekpenghapusan(28) As String, aprodusen(29) As Integer, 
        'atglpensiun(30) As Date, apenyusutanke(31) As Double, anilaimenurun(32) As Double, adispose(33) As Integer, apembelian(34) As Integer, 
        'apenjualan(35) As Integer, alocked(36) As Integer, astatus(37) As Integer, astatussebelumnya(38) As Integer, aisclose(39) As Integer, 
        'ainputuser(40) As Integer, ainputtgl(41) As DateTime, amodifikasiuser(42) As Integer, amodifikasitgl(43) As DateTime, acustomtext1(44) As String, 
        'acustomtext2(45) As String, acustomtext3(46) As String, acustomtext4(47) As String, acustomtext5(48) As String, acustomint1(49) As Integer, 
        'acustomint2(50) As Integer, acustomint3(51) As Integer, acustomdbl1(52) As Double, acustomdbl2(53) As Double, acustomdbl3(54) As Double, 
        'acustomdate1(55) As Date, acustomdate2(56) As Date, acustomdate3(57) As Date, asatuan(58) As String, aharga(59) As String, adiskon(60) As String,
        'ajmldiskon(61) As String, apajak1(62) As String, ajmlpajak1(63) As String, apajak2(64) As String, ajmlpajak2(65) As String,

        'MAPPING BUAT FLEX --------------------------------------------------------
        'aid, akode, anama, akategori, acabang, alokasi, adivisi, 
        'asubdivisi, acatatan, anomor, atglbeli, atglpakai, amatauang, akurs, 
        'ahargabeli, anilairesidu, aumurekonomis, abebanperbln, aakumulasibeban, anilaibuku, ametode, 
        'atabelpenyusutan, aintangible, afiskal, aatastengahbulan, arekasset, arekakumdepresiasi, arekdepresiasi, 
        'arekpenghapusan, aprodusen, atglpensiun, apenyusutanke, anilaimenurun, adispose, apembelian, 
        'apenjualan, alocked, astatus, astatussebelumnya, aisclose, ainputuser, ainputtgl, 
        'amodifikasiuser, amodifikasitgl, acustomtext1, acustomtext2, acustomtext3, acustomtext4, acustomtext5, 
        'acustomint1, acustomint2, acustomint3, acustomdbl1, acustomdbl2, acustomdbl3, acustomdate1, 
        'acustomdate2, acustomdate3, asatuan, aharga, adiskon, ajmldiskon, apajak1, ajmlpajak1, apajak2, ajmlpajak2

        'VALIDASI DAN SET DATA MASTER ======================================================
        'SPLIT PARAMETER DATA MASTER
        dataMaster = dataSplit(2).Split(sptRow)
        'END OF VALIDASI DAN SET DATA MASTER ===============================================


        'Buat datatable MASTER
        Dim dtmaster As New DataTable
        AsDataTableTambahField(dtmaster, "aid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "akode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "anama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "akategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "alokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "adivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "asubdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "anomor", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "atglbeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "atglpakai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "amatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "akurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "ahargabeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "anilairesidu", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "aumurekonomis", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "abebanperbln", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "aakumulasibeban", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "anilaibuku", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "ametode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "atabelpenyusutan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "aintangible", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "afiskal", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "aatastengahbulan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "arekasset", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "arekakumdepresiasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "arekdepresiasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "arekpenghapusan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "aprodusen", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtmaster, "atglpensiun", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "apenyusutanke", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "anilaimenurun", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "adispose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "apembelian", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "apenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "alocked", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "astatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "astatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "aisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "ainputuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtmaster, "ainputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "amodifikasiuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtmaster, "amodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "acustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "acustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmaster, "acustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "acustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "asatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "aharga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtmaster, "adiskon", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtmaster, "ajmldiskon", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtmaster, "apajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "ajmlpajak1", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtmaster, "apajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmaster, "ajmlpajak2", AsEnumTypeData.AsDouble)

        'VALIDASI DAN SET DATA ROW MASTER ==================================================
        Dim JmlDtMaster As Integer = dataMaster.Length
        For i = 1 To JmlDtMaster
            'SPLIT DATA MASTER
            dataRowMaster = dataMaster(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA MASTER -----------------------------------
            'CEK ARRAY DATA MASTER
            If (dataRowMaster.Length <> 66) Then
                result(2) = "Row : " & i & " - Invalid master transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW MASTER ----------------------------

            'VALIDASI TIPE DATA ==========================================================
            'aid(0) As Integer
            If (IsNumeric(dataRowMaster(0)) = False) Then
                result(2) = "aid required numeric." : GoTo selesai
            End If
            'atglbeli(10) As Date
            If (IsDate(dataRowMaster(10)) = False) Then
                result(2) = "atglbeli required date." : GoTo selesai
            End If
            'atglpakai(11) As Date
            If (IsDate(dataRowMaster(11)) = False) Then
                result(2) = "atglpakai required date." : GoTo selesai
            End If
            'akurs(13) As Double
            If (IsNumeric(dataRowMaster(13)) = False) Then
                result(2) = "akurs required numeric." : GoTo selesai
            End If
            'ahargabeli(14) As Double
            If (IsNumeric(dataRowMaster(14)) = False) Then
                result(2) = "ahargabeli required numeric." : GoTo selesai
            End If
            'anilairesidu(15) As Double
            If (IsNumeric(dataRowMaster(15)) = False) Then
                result(2) = "anilairesidu required numeric." : GoTo selesai
            End If
            'aumurekonomis(16) As Double
            If (IsNumeric(dataRowMaster(16)) = False) Then
                result(2) = "aumurekonomis required numeric." : GoTo selesai
            End If
            'abebanperbln(17) As Double
            If (IsNumeric(dataRowMaster(17)) = False) Then
                result(2) = "abebanperbln required numeric." : GoTo selesai
            End If
            'aakumulasibeban(18) As Double
            If (IsNumeric(dataRowMaster(18)) = False) Then
                result(2) = "aakumulasibeban required numeric." : GoTo selesai
            End If
            'anilaibuku(19) As Double
            If (IsNumeric(dataRowMaster(19)) = False) Then
                result(2) = "anilaibuku required numeric." : GoTo selesai
            End If
            'ametode(20) As Integer
            If (IsNumeric(dataRowMaster(20)) = False) Then
                result(2) = "ametode required numeric." : GoTo selesai
            End If
            'aintangible(22) As Integer
            If (IsNumeric(dataRowMaster(22)) = False) Then
                result(2) = "aintangible required numeric." : GoTo selesai
            End If
            'afiskal(23) As Integer
            If (IsNumeric(dataRowMaster(23)) = False) Then
                result(2) = "afiskal required numeric." : GoTo selesai
            End If
            'aatastengahbulan(24) As Integer
            If (IsNumeric(dataRowMaster(24)) = False) Then
                result(2) = "aatastengahbulan required numeric." : GoTo selesai
            End If
            'aprodusen(29) As Integer
            If (IsNumeric(dataRowMaster(29)) = False) Then
                result(2) = "aprodusen required numeric." : GoTo selesai
            End If
            'atglpensiun(30) As Date
            If (IsDate(dataRowMaster(30)) = False) Then
                result(2) = "atglpensiun required date." : GoTo selesai
            End If
            'apenyusutanke(31) As Double
            If (IsNumeric(dataRowMaster(31)) = False) Then
                result(2) = "apenyusutanke required numeric." : GoTo selesai
            End If
            'anilaimenurun(32) As Double
            If (IsNumeric(dataRowMaster(32)) = False) Then
                result(2) = "anilaimenurun required numeric." : GoTo selesai
            End If
            'adispose(33) As Integer
            If (IsNumeric(dataRowMaster(33)) = False) Then
                result(2) = "adispose required numeric." : GoTo selesai
            End If
            'apembelian(34) As Integer
            If (IsNumeric(dataRowMaster(34)) = False) Then
                result(2) = "apembelian required numeric." : GoTo selesai
            End If
            'apenjualan(35) As Integer
            If (IsNumeric(dataRowMaster(35)) = False) Then
                result(2) = "apenjualan required numeric." : GoTo selesai
            End If
            'alocked(36) As Integer
            If (IsNumeric(dataRowMaster(36)) = False) Then
                result(2) = "alocked required numeric." : GoTo selesai
            End If
            'astatus(37) As Integer
            If (IsNumeric(dataRowMaster(37)) = False) Then
                result(2) = "astatus required numeric." : GoTo selesai
            End If
            'astatussebelumnya(38) As Integer
            If (IsNumeric(dataRowMaster(38)) = False) Then
                result(2) = "astatussebelumnya required numeric." : GoTo selesai
            End If
            'aisclose(39) As Integer
            If (IsNumeric(dataRowMaster(39)) = False) Then
                result(2) = "aisclose required numeric." : GoTo selesai
            End If
            'ainputuser(40) As Integer
            If (IsNumeric(dataRowMaster(40)) = False) Then
                result(2) = "ainputuser required numeric." : GoTo selesai
            End If
            'ainputtgl(41) As DateTime
            If (IsDate(dataRowMaster(41)) = False) Then
                result(2) = "ainputtgl required date." : GoTo selesai
            End If
            'amodifikasiuser(42) As Integer
            If (IsNumeric(dataRowMaster(42)) = False) Then
                result(2) = "amodifikasiuser required numeric." : GoTo selesai
            End If
            'amodifikasitgl(43) As DateTime
            If (IsDate(dataRowMaster(43)) = False) Then
                result(2) = "amodifikasitgl required date." : GoTo selesai
            End If
            'acustomint1(49) As Integer
            If (IsNumeric(dataRowMaster(49)) = False) Then
                result(2) = "acustomint1 required numeric." : GoTo selesai
            End If
            'acustomint2(50) As Integer
            If (IsNumeric(dataRowMaster(50)) = False) Then
                result(2) = "acustomint2 required numeric." : GoTo selesai
            End If
            'acustomint3(51) As Integer
            If (IsNumeric(dataRowMaster(51)) = False) Then
                result(2) = "acustomint3 required numeric." : GoTo selesai
            End If
            'acustomdbl1(52) As Double
            If (IsNumeric(dataRowMaster(52)) = False) Then
                result(2) = "acustomdbl1 required numeric." : GoTo selesai
            End If
            'acustomdbl2(53) As Double
            If (IsNumeric(dataRowMaster(53)) = False) Then
                result(2) = "acustomdbl2 required numeric." : GoTo selesai
            End If
            'acustomdbl3(54) As Double
            If (IsNumeric(dataRowMaster(54)) = False) Then
                result(2) = "acustomdbl3 required numeric." : GoTo selesai
            End If
            'acustomdate1(55) As Date
            If (IsDate(dataRowMaster(55)) = False) Then
                result(2) = "acustomdate1 required date." : GoTo selesai
            End If
            'acustomdate2(56) As Date
            If (IsDate(dataRowMaster(56)) = False) Then
                result(2) = "acustomdate2 required date." : GoTo selesai
            End If
            'acustomdate3(57) As Date
            If (IsDate(dataRowMaster(57)) = False) Then
                result(2) = "acustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA ===================================================

            'VALIDASI DATA ===============================================================
            'akode(1) As String
            If Len(dataRowMaster(1)) = 0 Then
                result(2) = "akode can't be empty" : GoTo selesai
            End If
            If Len(dataRowMaster(1)) > 25 Then
                result(2) = "akode should not be more than 25 character." : GoTo selesai
            End If

            'anama(2) As String
            If Len(dataRowMaster(2)) = 0 Then
                result(2) = "anama can't be empty" : GoTo selesai
            End If
            If Len(dataRowMaster(2)) > 100 Then
                result(2) = "anama should not be more than 100 character." : GoTo selesai
            End If

            'akategori(3) As String
            If Len(dataRowMaster(3)) = 0 Then
                result(2) = "akategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowMaster(3)) > 25 Then
                result(2) = "akategori should not be more than 25 character." : GoTo selesai
            End If

            'atglbeli(10) As Date
            If Len(dataRowMaster(10)) = 0 Then
                result(2) = "atglbeli can't be empty" : GoTo selesai
            End If

            'atglpakai(11) As Date
            If Len(dataRowMaster(11)) = 0 Then
                result(2) = "atglpakai can't be empty" : GoTo selesai
            End If

            'amatauang(12) As String
            If Len(dataRowMaster(12)) = 0 Then
                result(2) = "amatauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowMaster(12)) > 25 Then
                result(2) = "amatauang should not be more than 25 character." : GoTo selesai
            End If

            'akurs(13) As Double
            If Len(dataRowMaster(13)) = 0 Then
                result(2) = "akurs can't be empty" : GoTo selesai
            End If

            'ahargabeli(14) As Double
            If Len(dataRowMaster(14)) = 0 Then
                result(2) = "ahargabeli can't be empty" : GoTo selesai
            End If

            'anilairesidu(15) As Double
            If Len(dataRowMaster(15)) = 0 Then
                result(2) = "anilairesidu can't be empty" : GoTo selesai
            End If

            'aumurekonomis(16) As Double
            If Len(dataRowMaster(16)) = 0 Then
                result(2) = "aumurekonomis can't be empty" : GoTo selesai
            End If

            'abebanperbln(17) As Double
            If Len(dataRowMaster(17)) = 0 Then
                result(2) = "abebanperbln can't be empty" : GoTo selesai
            End If

            'aakumulasibeban(18) As Double
            If Len(dataRowMaster(18)) = 0 Then
                result(2) = "aakumulasibeban can't be empty" : GoTo selesai
            End If

            'anilaibuku(19) As Double
            If Len(dataRowMaster(19)) = 0 Then
                result(2) = "anilaibuku can't be empty" : GoTo selesai
            End If

            'arekasset(25) As String
            If Len(dataRowMaster(25)) = 0 Then
                result(2) = "arekasset can't be empty" : GoTo selesai
            End If
            If Len(dataRowMaster(25)) > 25 Then
                result(2) = "arekasset should not be more than 25 character." : GoTo selesai
            End If

            'arekakumdepresiasi(26) As String
            If Len(dataRowMaster(26)) = 0 Then
                result(2) = "arekakumdepresiasi can't be empty" : GoTo selesai
            End If
            If Len(dataRowMaster(26)) > 25 Then
                result(2) = "arekakumdepresiasi should not be more than 25 character." : GoTo selesai
            End If

            'arekdepresiasi(27) As String
            If Len(dataRowMaster(27)) = 0 Then
                result(2) = "arekdepresiasi can't be empty" : GoTo selesai
            End If
            If Len(dataRowMaster(27)) > 25 Then
                result(2) = "arekdepresiasi should not be more than 25 character." : GoTo selesai
            End If

            'atglpensiun(30) As Date
            If Len(dataRowMaster(30)) = 0 Then
                result(2) = "atglpensiun can't be empty" : GoTo selesai
            End If

            'apenyusutanke(31) As Double
            If Len(dataRowMaster(31)) = 0 Then
                result(2) = "apenyusutanke can't be empty" : GoTo selesai
            End If

            'anilaimenurun(32) As Double
            If Len(dataRowMaster(32)) = 0 Then
                result(2) = "anilaimenurun can't be empty" : GoTo selesai
            End If

            'ainputtgl(41) As DateTime
            If Len(dataRowMaster(41)) = 0 Then
                result(2) = "ainputtgl can't be empty" : GoTo selesai
            End If

            'amodifikasitgl(43) As DateTime
            If Len(dataRowMaster(43)) = 0 Then
                result(2) = "amodifikasitgl can't be empty" : GoTo selesai
            End If

            'acustomdbl1(52) As Double
            If Len(dataRowMaster(52)) = 0 Then
                result(2) = "acustomdbl1 can't be empty" : GoTo selesai
            End If

            'acustomdbl2(53) As Double
            If Len(dataRowMaster(53)) = 0 Then
                result(2) = "acustomdbl2 can't be empty" : GoTo selesai
            End If

            'acustomdbl3(54) As Double
            If Len(dataRowMaster(54)) = 0 Then
                result(2) = "acustomdbl3 can't be empty" : GoTo selesai
            End If

            'acustomdate1(55) As Date
            If Len(dataRowMaster(55)) = 0 Then
                result(2) = "acustomdate1 can't be empty" : GoTo selesai
            End If

            'acustomdate2(56) As Date
            If Len(dataRowMaster(56)) = 0 Then
                result(2) = "acustomdate2 can't be empty" : GoTo selesai
            End If

            'acustomdate3(57) As Date
            If Len(dataRowMaster(57)) = 0 Then
                result(2) = "acustomdate3 can't be empty" : GoTo selesai
            End If

            'asatuan(58) As Date
            If Len(dataRowMaster(58)) = 0 Then
                result(2) = "asatuan can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA MASTER========================================================

            If AsDataTableTambahData(dtmaster, "aid~akode~anama~akategori~acabang~alokasi~adivisi~asubdivisi~acatatan~anomor~atglbeli~atglpakai~amatauang~akurs~ahargabeli~anilairesidu~aumurekonomis~abebanperbln~aakumulasibeban~anilaibuku~ametode~atabelpenyusutan~aintangible~afiskal~aatastengahbulan~arekasset~arekakumdepresiasi~arekdepresiasi~arekpenghapusan~aprodusen~atglpensiun~apenyusutanke~anilaimenurun~adispose~apembelian~apenjualan~alocked~astatus~astatussebelumnya~aisclose~ainputuser~ainputtgl~amodifikasiuser~amodifikasitgl~acustomtext1~acustomtext2~acustomtext3~acustomtext4~acustomtext5~acustomint1~acustomint2~acustomint3~acustomdbl1~acustomdbl2~acustomdbl3~acustomdate1~acustomdate2~acustomdate3~asatuan~aharga~adiskon~ajmldiskon~apajak1~ajmlpajak1~apajak2~ajmlpajak2", dataRowMaster(0) & "~" & dataRowMaster(1) & "~" & dataRowMaster(2) & "~" & dataRowMaster(3) & "~" & dataRowMaster(4) & "~" & dataRowMaster(5) & "~" & dataRowMaster(6) & "~" & dataRowMaster(7) & "~" & dataRowMaster(8) & "~" & dataRowMaster(9) & "~" & dataRowMaster(10) & "~" & dataRowMaster(11) & "~" & dataRowMaster(12) & "~" & dataRowMaster(13) & "~" & dataRowMaster(14) & "~" & dataRowMaster(15) & "~" & dataRowMaster(16) & "~" & dataRowMaster(17) & "~" & dataRowMaster(18) & "~" & dataRowMaster(19) & "~" & dataRowMaster(20) & "~" & dataRowMaster(21) & "~" & dataRowMaster(22) & "~" & dataRowMaster(23) & "~" & dataRowMaster(24) & "~" & dataRowMaster(25) & "~" & dataRowMaster(26) & "~" & dataRowMaster(27) & "~" & dataRowMaster(28) & "~" & dataRowMaster(29) & "~" & dataRowMaster(30) & "~" & dataRowMaster(31) & "~" & dataRowMaster(32) & "~" & dataRowMaster(33) & "~" & dataRowMaster(34) & "~" & dataRowMaster(35) & "~" & dataRowMaster(36) & "~" & dataRowMaster(37) & "~" & dataRowMaster(38) & "~" & dataRowMaster(39) & "~" & dataRowMaster(40) & "~" & dataRowMaster(41) & "~" & dataRowMaster(42) & "~" & dataRowMaster(43) & "~" & dataRowMaster(44) & "~" & dataRowMaster(45) & "~" & dataRowMaster(46) & "~" & dataRowMaster(47) & "~" & dataRowMaster(48) & "~" & dataRowMaster(49) & "~" & dataRowMaster(50) & "~" & dataRowMaster(51) & "~" & dataRowMaster(52) & "~" & dataRowMaster(53) & "~" & dataRowMaster(54) & "~" & dataRowMaster(55) & "~" & dataRowMaster(56) & "~" & dataRowMaster(57) & "~" & dataRowMaster(58) & "~" & dataRowMaster(59) & "~" & dataRowMaster(60) & "~" & dataRowMaster(61) & "~" & dataRowMaster(62) & "~" & dataRowMaster(63) & "~" & dataRowMaster(64) & "~" & dataRowMaster(65)) = False Then
                result(2) = "Insert into master datatable failed." : GoTo selesai
            End If
        Next
        'END OF VALIDASI DAN SET ROW DATA MASTER ===========================================

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
                Dim dr1 As DataRow = dtutama.Rows(0)
                If isUpdate Then
                    result(4) = dr1("agid")
                    notransaksi = dr1("agnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(agid) FROM M7_Ag WHERE agid=" & result(4))
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then
                        sql = "Update M7_Ag set agcabang  = '" & FixQuotes(dr1("agcabang")) & "', aglokasi  = '" & FixQuotes(dr1("aglokasi")) & "', agsumber  = '" & FixQuotes(dr1("agsumber")) & "', agautonotransaksi  = " & dr1("agautonotransaksi") & ", agnotransaksi  = '" & FixQuotes(dr1("agnotransaksi")) & "', agtgl  = '" & FixQuotes(AsFormatTanggal(dr1("agtgl"))) & "', agkodepa  = '" & FixQuotes(dr1("agkodepa")) & "', agbagianag  = '" & FixQuotes(dr1("agbagianag")) & "', agbagianagkontak  = '" & FixQuotes(dr1("agbagianagkontak")) & "', agmatauang  = '" & FixQuotes(dr1("agmatauang")) & "', agkurs  = '" & FixDouble(dr1("agkurs")) & "', aguraian  = '" & FixQuotes(dr1("aguraian")) & "', agcatatan  = '" & FixQuotes(dr1("agcatatan")) & "', agnoref  = '" & FixQuotes(dr1("agnoref")) & "', agtglnoref  = '" & FixQuotes(AsFormatTanggal(dr1("agtglnoref"))) & "', agstatus  = " & dr1("agstatus") & ", agstatussebelumnya  = " & dr1("agstatussebelumnya") & ", agjmlrevisi  = " & dr1("agjmlrevisi") & ", agcetakanke  = " & dr1("agcetakanke") & ", aginputuser  = '" & FixQuotes(dr1("aginputuser")) & "', aginputtgl  = '" & FixQuotes(AsFormatTanggal(dr1("aginputtgl"), "yyyy-MM-dd H:mm:ss")) & "', agmodifikasiuser  = '" & FixQuotes(dr1("agmodifikasiuser")) & "', agmodifikasitgl  = NOW(), agposting  = " & dr1("agposting") & ", agpostingtgl  = '" & FixQuotes(AsFormatTanggal(dr1("agpostingtgl"), "yyyy-MM-dd H:mm:ss")) & "', agtutupperiode  = " & dr1("agtutupperiode") & ", agcustomtext1  = '" & FixQuotes(dr1("agcustomtext1")) & "', agcustomtext2  = '" & FixQuotes(dr1("agcustomtext2")) & "', agcustomtext3  = '" & FixQuotes(dr1("agcustomtext3")) & "', agcustomtext4  = '" & FixQuotes(dr1("agcustomtext4")) & "', agcustomtext5  = '" & FixQuotes(dr1("agcustomtext5")) & "', agcustomint1  = " & dr1("agcustomint1") & ", agcustomint2  = " & dr1("agcustomint2") & ", agcustomint3  = " & dr1("agcustomint3") & ", agcustomdbl1  = '" & FixDouble(dr1("agcustomdbl1")) & "', agcustomdbl2  = '" & FixDouble(dr1("agcustomdbl2")) & "', agcustomdbl3  = '" & FixDouble(dr1("agcustomdbl3")) & "', agcustomdate1  = '" & FixQuotes(AsFormatTanggal(dr1("agcustomdate1"))) & "', agcustomdate2  = '" & FixQuotes(AsFormatTanggal(dr1("agcustomdate2"))) & "', agcustomdate3  = '" & FixQuotes(AsFormatTanggal(dr1("agcustomdate3"))) & "' where agid = " & dr1("agid") & ""
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
                    If dr1("agautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(dr1("agcabang"), dr1("aglokasi"), dr1("agsumber"), dr1("agtgl"))
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
                        notransaksi = dr1("agnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(agid) FROM m7_ag WHERE agnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M7_Ag (agcabang, aglokasi, agsumber, agautonotransaksi, agnotransaksi, agtgl, agkodepa, agbagianag, agbagianagkontak, agmatauang, agkurs, aguraian, agcatatan, agnoref, agtglnoref, agstatus, agstatussebelumnya, agjmlrevisi, agcetakanke, aginputuser, aginputtgl, agmodifikasiuser, agmodifikasitgl, agposting, agpostingtgl, agtutupperiode, agisclose, agcustomtext1, agcustomtext2, agcustomtext3, agcustomtext4, agcustomtext5, agcustomint1, agcustomint2, agcustomint3, agcustomdbl1, agcustomdbl2, agcustomdbl3, agcustomdate1, agcustomdate2, agcustomdate3) values('" & FixQuotes(dr1("agcabang")) & "', '" & FixQuotes(dr1("aglokasi")) & "', '" & FixQuotes(dr1("agsumber")) & "', " & dr1("agautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(dr1("agtgl"))) & "', '" & FixQuotes(dr1("agkodepa")) & "', '" & FixQuotes(dr1("agbagianag")) & "', '" & FixQuotes(dr1("agbagianagkontak")) & "', '" & FixQuotes(dr1("agmatauang")) & "', '" & FixDouble(dr1("agkurs")) & "', '" & FixQuotes(dr1("aguraian")) & "', '" & FixQuotes(dr1("agcatatan")) & "', '" & FixQuotes(dr1("agnoref")) & "', '" & FixQuotes(AsFormatTanggal(dr1("agtglnoref"))) & "', " & dr1("agstatus") & ", " & dr1("agstatussebelumnya") & ", " & dr1("agjmlrevisi") & ", " & dr1("agcetakanke") & ", '" & FixQuotes(dr1("aginputuser")) & "', NOW(), '" & FixQuotes(dr1("agmodifikasiuser")) & "', '1971-01-01 00:00:00', " & dr1("agposting") & ", '" & FixQuotes(AsFormatTanggal(dr1("agpostingtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("agtutupperiode") & ", " & dr1("agisclose") & ", '" & FixQuotes(dr1("agcustomtext1")) & "', '" & FixQuotes(dr1("agcustomtext2")) & "', '" & FixQuotes(dr1("agcustomtext3")) & "', '" & FixQuotes(dr1("agcustomtext4")) & "', '" & FixQuotes(dr1("agcustomtext5")) & "', " & dr1("agcustomint1") & ", " & dr1("agcustomint2") & ", " & dr1("agcustomint3") & ", '" & FixDouble(dr1("agcustomdbl1")) & "', '" & FixDouble(dr1("agcustomdbl2")) & "', '" & FixDouble(dr1("agcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("agcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("agcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("agcustomdate3"))) & "')"
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
                result(2) = "#1. Main transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Dim dt2 As New DataTable
            'Sql disesuaikan sendiri, untuk parameternya disesuaikan sendiri.
            dt2 = AsDataTableAmbilDariDB("select agid from M7_ag where agnotransaksi='" & notransaksi & "' AND aginputuser= '" & userid & "' order by agmodifikasitgl desc limit 1")
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai

            'Hapus detail ketika update
            If (isUpdate) Then
                sql = "Delete from M7_Ag_Detail where idag = " & result(4)
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
                Dim dr1 As DataRow
                For i = 0 To dataMaster.Length - 1
                    dr1 = dtdetail.Rows(i)
                    dataRowMaster = dataMaster(i).Split(sptField)
                    sql = "Insert into M7_Asset (akode, anama, akategori, acabang, alokasi, adivisi, asubdivisi, acatatan, anomor, atglbeli, atglpakai, amatauang, akurs, ahargabeli, anilairesidu, aumurekonomis, abebanperbln, aakumulasibeban, anilaibuku, ametode, atabelpenyusutan, aintangible, afiskal, aatastengahbulan, arekasset, arekakumdepresiasi, arekdepresiasi, arekpenghapusan, aprodusen, atglpensiun, apenyusutanke, anilaimenurun, adispose, apembelian, apenjualan, alocked, astatus, astatussebelumnya, aisclose, ainputuser, ainputtgl, amodifikasiuser, amodifikasitgl, acustomtext1, acustomtext2, acustomtext3, acustomtext4, acustomtext5, acustomint1, acustomint2, acustomint3, acustomdbl1, acustomdbl2, acustomdbl3, acustomdate1, acustomdate2, acustomdate3, asatuan, aharga, adiskon, ajmldiskon, apajak1, ajmlpajak1, apajak2, ajmlpajak2) values('" & FixQuotes(dataRowMaster(1)) & "', '" & FixQuotes(dataRowMaster(2)) & "', '" & FixQuotes(dataRowMaster(3)) & "', '" & FixQuotes(dataRowMaster(4)) & "', '" & FixQuotes(dataRowMaster(5)) & "', '" & FixQuotes(dataRowMaster(6)) & "', '" & FixQuotes(dataRowMaster(7)) & "', '" & FixQuotes(dataRowMaster(8)) & "', '" & FixQuotes(dataRowMaster(9)) & "', '" & FixQuotes(AsFormatTanggal(dataRowMaster(10))) & "', '" & FixQuotes(AsFormatTanggal(dataRowMaster(11))) & "', '" & FixQuotes(dataRowMaster(12)) & "', '" & FixDouble(dataRowMaster(13)) & "', '" & FixDouble(dataRowMaster(14)) & "', '" & FixDouble(dataRowMaster(15)) & "', '" & FixDouble(dataRowMaster(16)) & "', '" & FixDouble(dataRowMaster(17)) & "', '" & FixDouble(dataRowMaster(18)) & "', '" & FixDouble(dataRowMaster(19)) & "', " & dataRowMaster(20) & ", '" & FixQuotes(dataRowMaster(21)) & "', " & dataRowMaster(22) & ", " & dataRowMaster(23) & ", " & dataRowMaster(24) & ", '" & FixQuotes(dataRowMaster(25)) & "', '" & FixQuotes(dataRowMaster(26)) & "', '" & FixQuotes(dataRowMaster(27)) & "', '" & FixQuotes(dataRowMaster(28)) & "', " & dataRowMaster(29) & ", '" & FixQuotes(AsFormatTanggal(dataRowMaster(30))) & "', '" & FixDouble(dataRowMaster(31)) & "', '" & FixDouble(dataRowMaster(32)) & "', " & dataRowMaster(33) & ", " & dataRowMaster(34) & ", " & dataRowMaster(35) & ", " & dataRowMaster(36) & ", " & dataRowMaster(37) & ", " & dataRowMaster(38) & ", " & dataRowMaster(39) & ", " & dataRowMaster(40) & ", NOW(), " & dataRowMaster(42) & ", '1971-01-01 00:00:00', '" & FixQuotes(dataRowMaster(44)) & "', '" & FixQuotes(dataRowMaster(45)) & "', '" & FixQuotes(dataRowMaster(46)) & "', '" & FixQuotes(dataRowMaster(47)) & "', '" & FixQuotes(dataRowMaster(48)) & "', " & dataRowMaster(49) & ", " & dataRowMaster(50) & ", " & dataRowMaster(51) & ", '" & FixDouble(dataRowMaster(52)) & "', '" & FixDouble(dataRowMaster(53)) & "', '" & FixDouble(dataRowMaster(54)) & "', '" & FixQuotes(AsFormatTanggal(dataRowMaster(55))) & "', '" & FixQuotes(AsFormatTanggal(dataRowMaster(56))) & "', '" & FixQuotes(AsFormatTanggal(dataRowMaster(57))) & "', '" & FixQuotes(dataRowMaster(58)) & "', '" & FixQuotes(dataRowMaster(59)) & "', '" & FixQuotes(dataRowMaster(60)) & "', '" & FixQuotes(dataRowMaster(61)) & "', '" & FixQuotes(dataRowMaster(62)) & "', '" & FixQuotes(dataRowMaster(63)) & "', '" & FixQuotes(dataRowMaster(64)) & "', '" & FixQuotes(dataRowMaster(65)) & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    Dim dt3 As New DataTable
                    'Sql disesuaikan sendiri, untuk parameternya disesuaikan sendiri.
                    dt3 = AsDataTableAmbilDariDB("select aid from M7_Asset where akode = '" & dataRowMaster(1) & "' order by aid desc limit 1")
                    If (dt3.Rows.Count = 0) Then
                        result(2) = "id asset not found" : Trans.Rollback() : GoTo selesai
                    End If
                    strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Append("('" & FixQuotes(dr1("idagdetail")) & "', " & result(4) & ", '" & dt3.Rows(0)(0) & "', '" & FixQuotes(dr1("namaasset")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("hargabeli")) & "', '" & FixQuotes(dr1("rekasset")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(dr1("satuan")) & "')")
                Next
                sql = "Insert into M7_Ag_Detail(idagdetail, idag, idasset, namaasset, jml, matauang, kurs, hargabeli, rekasset, cabang, lokasi, costcenter, divisi, subdivisi, proyek, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, satuan) values" & strValue2.ToString & ""
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                ''Hapus memchaced
                'AsMemcached.Remove("apliksasi1-M7_Ag~M7_Ag_Detail-" & result(4))

            Else
                result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

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
    Public Function M7_AgGetdataById(ByVal param As String) As String

        'M7_AgGetdataById Utama --------------------------------------------------------
        'agid, agcabang, aglokasi, agsumber, agautonotransaksi, agnotransaksi, agtgl, 
        'agkodepa, agbagianag, agbagianagkontak, agmatauang, agkurs, aguraian, agcatatan, 
        'agnoref, agtglnoref, agstatus, agstatussebelumnya, agjmlrevisi, agcetakanke, aginputuser, 
        'aginputtgl, agmodifikasiuser, agmodifikasitgl, agposting, agpostingtgl, agtutupperiode, agisclose, 
        'agcustomtext1, agcustomtext2, agcustomtext3, agcustomtext4, agcustomtext5, agcustomint1, agcustomint2, 
        'agcustomint3, agcustomdbl1, agcustomdbl2, agcustomdbl3, agcustomdate1, agcustomdate2, agcustomdate3, 
        'agcabangnama, aglokasinama, agbagianagkode, agbagianagnama, agstatusnama, agstatussebelumnyanama, aginputusernama, 
        'agmodifikasiusernama

        'M7_AgGetdataById Detail -------------------------------------------------------
        'idagdetail, idag, idasset, namaasset, jml, matauang, 
        'kurs, rekasset, cabang, lokasi, costcenter, divisi, subdivisi, 
        'proyek, catatan, urutan, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodeasset, 
        'rekassetnama, cabangnama, lokasinama, costcenternama, divisinama, subdivisinama, proyeknama,'
        'satuan

        'M7_AeGetdataById Asset -------------------------------------------------------
        'aid, akode, anama, akategori, acabang, alokasi, adivisi, 
        'asubdivisi, acatatan, anomor, atglbeli, atglpakai, amatauang, akurs, 
        'ahargabeli, anilairesidu, aumurekonomis, abebanperbln, aakumulasibeban, anilaibuku, ametode, 
        'atabelpenyusutan, aintangible, afiskal, aatastengahbulan, arekasset, arekakumdepresiasi, arekdepresiasi, 
        'arekpenghapusan, aprodusen, atglpensiun, apenyusutanke, anilaimenurun, adispose, apembelian, 
        'apenjualan, alocked, astatus, astatussebelumnya, aisclose, ainputuser, ainputtgl, 
        'amodifikasiuser, amodifikasitgl, akategorinama, acabangnama, alokasinama, adivisinama, asubdivisinama, 
        'ametodenama, arekassetnama, arekakumdepresiasinama, arekdepresiasinama, arekpenghapusannama, aprodusenkode, aprodusennama, 
        'astatusnama, astatussebelumnyanama, ainputusernama, amodifikasiusernama, acustomtext1, acustomtext2, acustomtext3, acustomtext4, acustomtext5, acustomint1, acustomint2, acustomint3, acustomdbl1, acustomdbl2, acustomdbl3, acustomdate1,
        'acustomdate2, acustomdate3, asatuan, aharga, adiskon, ajmldiskon, apajak1, ajmlpajak1, apajak2, ajmlpajak2

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

        Dim utama As String = "", detail As String = "", asset As String = "", idtransaksi As String = ""

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
            Filter = "agid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "agid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m7_ag_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                     FxDB(drutama("agid"), ""), sptField,
                     FxDB(drutama("agcabang"), ""), sptField,
                     FxDB(drutama("aglokasi"), ""), sptField,
                     FxDB(drutama("agsumber"), ""), sptField,
                     FxDB(drutama("agautonotransaksi"), 0), sptField,
                     FxDB(drutama("agnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("agtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("agkodepa"), ""), sptField,
                     FxDB(drutama("agbagianag"), ""), sptField,
                     FxDB(drutama("agbagianagkontak"), ""), sptField,
                     FxDB(drutama("agmatauang"), ""), sptField,
                     FxDB(drutama("agkurs"), 0), sptField,
                     FxDB(drutama("aguraian"), ""), sptField,
                     FxDB(drutama("agcatatan"), ""), sptField,
                     FxDB(drutama("agnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("agtglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("agstatus"), 0), sptField,
                     FxDB(drutama("agstatussebelumnya"), 0), sptField,
                     FxDB(drutama("agjmlrevisi"), 0), sptField,
                     FxDB(drutama("agcetakanke"), 0), sptField,
                     FxDB(drutama("aginputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aginputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("agmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("agmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("agposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("agpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("agtutupperiode"), 0), sptField,
                     FxDB(drutama("agisclose"), 0), sptField,
                     FxDB(drutama("agcustomtext1"), ""), sptField,
                     FxDB(drutama("agcustomtext2"), ""), sptField,
                     FxDB(drutama("agcustomtext3"), ""), sptField,
                     FxDB(drutama("agcustomtext4"), ""), sptField,
                     FxDB(drutama("agcustomtext5"), ""), sptField,
                     FxDB(drutama("agcustomint1"), 0), sptField,
                     FxDB(drutama("agcustomint2"), 0), sptField,
                     FxDB(drutama("agcustomint3"), 0), sptField,
                     FxDB(drutama("agcustomdbl1"), 0), sptField,
                     FxDB(drutama("agcustomdbl2"), 0), sptField,
                     FxDB(drutama("agcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("agcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("agcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("agcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("agcabangnama"), ""), sptField,
                     FxDB(drutama("aglokasinama"), ""), sptField,
                     FxDB(drutama("agbagianagkode"), ""), sptField,
                     FxDB(drutama("agbagianagnama"), ""), sptField,
                     FxDB(drutama("agstatusnama"), ""), sptField,
                     FxDB(drutama("agstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("aginputusernama"), ""), sptField,
                     FxDB(drutama("agmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                      FxDB(dr("idagdetail"), ""), sptField,
                     FxDB(dr("idag"), ""), sptField,
                     FxDB(dr("idasset"), ""), sptField,
                     FxDB(dr("namaasset"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("rekasset"), ""), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
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
                     FxDB(dr("kodeasset"), ""), sptField,
                     FxDB(dr("rekassetnama"), ""), sptField,
                     FxDB(dr("cabangnama"), ""), sptField,
                     FxDB(dr("lokasinama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("satuan"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA Asset
            sql = "select `a`.`aid` AS `aid`,`a`.`akode` AS `akode`,`a`.`anama` AS `anama`,`a`.`akategori` AS `akategori`,`a`.`acabang` AS `acabang`,`a`.`alokasi` AS `alokasi`,`a`.`adivisi` AS `adivisi`,`a`.`asubdivisi` AS `asubdivisi`,`a`.`acatatan` AS `acatatan`,`a`.`anomor` AS `anomor`,`a`.`atglbeli` AS `atglbeli`,`a`.`atglpakai` AS `atglpakai`,`a`.`amatauang` AS `amatauang`,`a`.`akurs` AS `akurs`,`a`.`ahargabeli` AS `ahargabeli`,`a`.`anilairesidu` AS `anilairesidu`,`a`.`aumurekonomis` AS `aumurekonomis`,`a`.`abebanperbln` AS `abebanperbln`,`a`.`aakumulasibeban` AS `aakumulasibeban`,`a`.`anilaibuku` AS `anilaibuku`,`a`.`ametode` AS `ametode`,`a`.`atabelpenyusutan` AS `atabelpenyusutan`,`a`.`aintangible` AS `aintangible`,`a`.`afiskal` AS `afiskal`,`a`.`aatastengahbulan` AS `aatastengahbulan`,`a`.`arekasset` AS `arekasset`,`a`.`arekakumdepresiasi` AS `arekakumdepresiasi`,`a`.`arekdepresiasi` AS `arekdepresiasi`,`a`.`arekpenghapusan` AS `arekpenghapusan`,`a`.`aprodusen` AS `aprodusen`,`a`.`atglpensiun` AS `atglpensiun`,`a`.`apenyusutanke` AS `apenyusutanke`,`a`.`anilaimenurun` AS `anilaimenurun`,`a`.`adispose` AS `adispose`,`a`.`apembelian` AS `apembelian`,`a`.`apenjualan` AS `apenjualan`,`a`.`alocked` AS `alocked`,`a`.`astatus` AS `astatus`,`a`.`astatussebelumnya` AS `astatussebelumnya`,`a`.`aisclose` AS `aisclose`,`a`.`ainputuser` AS `ainputuser`,`a`.`ainputtgl` AS `ainputtgl`,`a`.`amodifikasiuser` AS `amodifikasiuser`,`a`.`amodifikasitgl` AS `amodifikasitgl`,`ac`.`acnama` AS `akategorinama`,`br`.`bnama` AS `acabangnama`,`l`.`lnama` AS `alokasinama`,`d`.`dnama` AS `adivisinama`,`sd`.`sdnama` AS `asubdivisinama`,`dc`.`nama` AS `ametodenama`,`coa1`.`cnama` AS `arekassetnama`,`coa2`.`cnama` AS `arekakumdepresiasinama`,`coa3`.`cnama` AS `arekdepresiasinama`,`coa4`.`cnama` AS `arekpenghapusannama`,`c1`.`kkode` AS `aprodusenkode`,`c1`.`knama` AS `aprodusennama`,`sp1`.`nama` AS `astatusnama`,`sp2`.`nama` AS `astatussebelumnyanama`,`u1`.`unama` AS `ainputusernama`,`u2`.`unama` AS `amodifikasiusernama`,`a`.`acustomtext1` AS `acustomtext1`,`a`.`acustomtext2` AS `acustomtext2`,`a`.`acustomtext3` AS `acustomtext3`,`a`.`acustomtext4` AS `acustomtext4`,`a`.`acustomtext5` AS `acustomtext5`,`a`.`acustomint1` AS `acustomint1`,`a`.`acustomint2` AS `acustomint2`,`a`.`acustomint3` AS `acustomint3`,`a`.`acustomdbl1` AS `acustomdbl1`,`a`.`acustomdbl2` AS `acustomdbl2`,`a`.`acustomdbl3` AS `acustomdbl3`,`a`.`acustomdate1` AS `acustomdate1`,`a`.`acustomdate2` AS `acustomdate2`,`a`.`acustomdate3` AS `acustomdate3`,`a`.`asatuan` AS `asatuan`, `a`.`aharga` AS `aharga`, `a`.`adiskon` AS `adiskon`,`a`.`ajmldiskon` AS `ajmldiskon`,`a`.`apajak1` AS `apajak1`,`a`.`ajmlpajak1` AS `ajmlpajak1`,`a`.`apajak2` AS `apajak2`,`a`.`ajmlpajak2` AS `ajmlpajak2` from ((((((((((((((((`m7_asset` `a` left join `m7_asset_category` `ac` on((`a`.`akategori` = `ac`.`ackode`))) left join `m1_branch` `br` on((`a`.`acabang` = `br`.`bkode`))) left join `m1_location` `l` on((`a`.`alokasi` = `l`.`lkode`))) left join `m1_division` `d` on((`a`.`adivisi` = `d`.`dkode`))) left join `m1_subdivision` `sd` on((`a`.`asubdivisi` = `sd`.`sdkode`))) left join `m7_depreciation_category` `dc` on((`a`.`ametode` = `dc`.`kode`))) left join `m1_coa` `coa1` on((`a`.`arekasset` = `coa1`.`cnomor`))) left join `m1_coa` `coa2` on((`a`.`arekakumdepresiasi` = `coa2`.`cnomor`))) left join `m1_coa` `coa3` on((`a`.`arekdepresiasi` = `coa3`.`cnomor`))) left join `m1_coa` `coa4` on((`a`.`arekpenghapusan` = `coa4`.`cnomor`))) left join `m1_contact` `c1` on((`a`.`aprodusen` = `c1`.`kid`))) left join `m0_status_progress` `sp1` on((`a`.`astatus` = `sp1`.`kode`))) left join `m0_status_progress` `sp2` on((`a`.`astatussebelumnya` = `sp2`.`kode`))) left join `m0_user` `u1` on((`a`.`ainputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`a`.`amodifikasiuser` = `u2`.`userid`))) left join `m7_ae_detail` `ae` on((`a`.`aid` = `ae`.`idasset`)))"
            Dim dtasset As New DataTable
            dtasset = AmbilData("aplikasi1-m7_asset", "idag = '" & idtransaksi & "'", "", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtasset.Rows
                asset = String.Concat(asset,
                      FxDB(dr("aid"), ""), sptField,
                     FxDB(dr("akode"), ""), sptField,
                     FxDB(dr("anama"), ""), sptField,
                     FxDB(dr("akategori"), ""), sptField,
                     FxDB(dr("acabang"), ""), sptField,
                     FxDB(dr("alokasi"), ""), sptField,
                     FxDB(dr("adivisi"), ""), sptField,
                     FxDB(dr("asubdivisi"), ""), sptField,
                     FxDB(dr("acatatan"), ""), sptField,
                     FxDB(dr("anomor"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("atglbeli"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("atglpakai"), ""), formatTgl), sptField,
                     FxDB(dr("amatauang"), ""), sptField,
                     FxDB(dr("akurs"), 0), sptField,
                     FxDB(dr("ahargabeli"), 0), sptField,
                     FxDB(dr("anilairesidu"), 0), sptField,
                     FxDB(dr("aumurekonomis"), 0), sptField,
                     FxDB(dr("abebanperbln"), 0), sptField,
                     FxDB(dr("aakumulasibeban"), 0), sptField,
                     FxDB(dr("anilaibuku"), 0), sptField,
                     FxDB(dr("ametode"), 0), sptField,
                     FxDB(dr("atabelpenyusutan"), ""), sptField,
                     FxDB(dr("aintangible"), 0), sptField,
                     FxDB(dr("afiskal"), 0), sptField,
                     FxDB(dr("aatastengahbulan"), 0), sptField,
                     FxDB(dr("arekasset"), ""), sptField,
                     FxDB(dr("arekakumdepresiasi"), ""), sptField,
                     FxDB(dr("arekdepresiasi"), ""), sptField,
                     FxDB(dr("arekpenghapusan"), ""), sptField,
                     FxDB(dr("aprodusen"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("atglpensiun"), ""), formatTgl), sptField,
                     FxDB(dr("apenyusutanke"), 0), sptField,
                     FxDB(dr("anilaimenurun"), 0), sptField,
                     FxDB(dr("adispose"), 0), sptField,
                     FxDB(dr("apembelian"), 0), sptField,
                     FxDB(dr("apenjualan"), 0), sptField,
                     FxDB(dr("alocked"), 0), sptField,
                     FxDB(dr("astatus"), 0), sptField,
                     FxDB(dr("astatussebelumnya"), 0), sptField,
                     FxDB(dr("aisclose"), 0), sptField,
                     FxDB(dr("ainputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ainputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("amodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("amodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("akategorinama"), ""), sptField,
                     FxDB(dr("acabangnama"), ""), sptField,
                     FxDB(dr("alokasinama"), ""), sptField,
                     FxDB(dr("adivisinama"), ""), sptField,
                     FxDB(dr("asubdivisinama"), ""), sptField,
                     FxDB(dr("ametodenama"), ""), sptField,
                     FxDB(dr("arekassetnama"), ""), sptField,
                     FxDB(dr("arekakumdepresiasinama"), ""), sptField,
                     FxDB(dr("arekdepresiasinama"), ""), sptField,
                     FxDB(dr("arekpenghapusannama"), ""), sptField,
                     FxDB(dr("aprodusenkode"), ""), sptField,
                     FxDB(dr("aprodusennama"), ""), sptField,
                     FxDB(dr("astatusnama"), ""), sptField,
                     FxDB(dr("astatussebelumnyanama"), ""), sptField,
                     FxDB(dr("ainputusernama"), ""), sptField,
                     FxDB(dr("amodifikasiusernama"), ""), sptField,
                     FxDB(dr("acustomtext1"), ""), sptField,
                     FxDB(dr("acustomtext2"), ""), sptField,
                     FxDB(dr("acustomtext3"), ""), sptField,
                     FxDB(dr("acustomtext4"), ""), sptField,
                     FxDB(dr("acustomtext5"), ""), sptField,
                     FxDB(dr("acustomint1"), 0), sptField,
                     FxDB(dr("acustomint2"), 0), sptField,
                     FxDB(dr("acustomint3"), 0), sptField,
                     FxDB(dr("acustomdbl1"), 0), sptField,
                     FxDB(dr("acustomdbl2"), 0), sptField,
                     FxDB(dr("acustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("acustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("acustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("acustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("asatuan"), ""), sptField,
                     FxDB(dr("aharga"), ""), sptField,
                     FxDB(dr("adiskon"), ""), sptField,
                     FxDB(dr("ajmldiskon"), ""), sptField,
                     FxDB(dr("apajak1"), ""), sptField,
                     FxDB(dr("ajmlpajak1"), ""), sptField,
                     FxDB(dr("apajak2"), ""), sptField,
                     FxDB(dr("ajmlpajak2"), ""), sptRow)
            Next
            If asset.Length > 0 Then asset = asset.Substring(0, asset.Length - sptRow.Length) Else asset = asset
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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, asset)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("agid, agcabang, aglokasi, agsumber, agautonotransaksi, agnotransaksi, agtgl, agkodepa, agbagianag, agbagianagkontak, agmatauang, agkurs, aguraian, agcatatan, agnoref, agtglnoref, agstatus, agstatussebelumnya, agjmlrevisi, agcetakanke, aginputuser, aginputtgl, agmodifikasiuser, agmodifikasitgl, agposting,agpostingtgl, agtutupperiode, agisclose, agcustomtext1, agcustomtext2, agcustomtext3, agcustomtext4, agcustomtext5, agcustomint1, agcustomint2, agcustomint3,agcustomdbl1, agcustomdbl2, agcustomdbl3, agcustomdate1, agcustomdate2, agcustomdate3, agcabangnama, aglokasinama, agbagianagkode, agbagianagnama, agstatusnama,agstatussebelumnyanama, aginputusernama, agmodifikasiusernama" & sptSubParam & "idagdetail, idag, idasset, namaasset, jml, matauang, kurs, rekasset, cabang, lokasi, costcenter, divisi, subdivisi, proyek, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodeasset, rekassetnama, cabangnama, lokasinama, costcenternama, divisinama, subdivisinama, proyeknama, satuan" & sptSubParam & "aid, akode, anama, akategori, acabang, alokasi, adivisi, asubdivisi, acatatan, anomor, atglbeli, atglpakai, amatauang, akurs, ahargabeli, anilairesidu, aumurekonomis, abebanperbln, aakumulasibeban, anilaibuku, ametode, atabelpenyusutan, aintangible, afiskal, aatastengahbulan, arekasset, arekakumdepresiasi,arekdepresiasi, arekpenghapusan, aprodusen, atglpensiun, apenyusutanke, anilaimenurun, adispose, apembelian, apenjualan, alocked, astatus, astatussebelumnya, aisclose, ainputuser, ainputtgl, amodifikasiuser, amodifikasitgl, akategorinama, acabangnama, alokasinama,adivisinama, asubdivisinama, ametodenama, arekassetnama, arekakumdepresiasinama, arekdepresiasinama, arekpenghapusannama, aprodusenkode, aprodusennama, astatusnama, astatussebelumnyanama, ainputusernama, amodifikasiusernama, acustomtext1, acustomtext2, acustomtext3, acustomtext4, acustomtext5, acustomint1, acustomint2, acustomint3, acustomdbl1, acustomdbl2, acustomdbl3, acustomdate1, acustomdate2, acustomdate3, asatuan, aharga, adiskon, ajmldiskon, apajak1, ajmlpajak1, apajak2, ajmlpajak2"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M7_AgSearch(ByVal param As String) As String
        'M7_AgSearch --------------------------------------------------------
        'agid, agcabang, aglokasi, agsumber, agautonotransaksi, agnotransaksi, agtgl, 
        'agkodepa, agbagianag, agbagianagkontak, agmatauang, agkurs, aguraian, agcatatan, 
        'agnoref, agtglnoref, agstatus, agstatussebelumnya, agjmlrevisi, agcetakanke, aginputuser, 
        'aginputtgl, agmodifikasiuser, agmodifikasitgl, agposting, agpostingtgl, agtutupperiode, agisclose, 
        'agcabangnama, aglokasinama, agbagianagkode, agbagianagnama, agstatusnama, agstatussebelumnyanama, aginputusernama, 
        'agmodifikasiusernama

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
            Filter = Filter.Replace("agbagianagkode", "c1.kkode")
            Filter = Filter.Replace("agbagianagnama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m7_ag_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M3_Ib", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("agid"), ""), sptField,
                     FxDB(dr("agcabang"), ""), sptField,
                     FxDB(dr("aglokasi"), ""), sptField,
                     FxDB(dr("agsumber"), ""), sptField,
                     FxDB(dr("agautonotransaksi"), 0), sptField,
                     FxDB(dr("agnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("agtgl"), ""), formatTgl), sptField,
                     FxDB(dr("agkodepa"), ""), sptField,
                     FxDB(dr("agbagianag"), ""), sptField,
                     FxDB(dr("agbagianagkontak"), ""), sptField,
                     FxDB(dr("agmatauang"), ""), sptField,
                     FxDB(dr("agkurs"), 0), sptField,
                     FxDB(dr("aguraian"), ""), sptField,
                     FxDB(dr("agcatatan"), ""), sptField,
                     FxDB(dr("agnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("agtglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("agstatus"), 0), sptField,
                     FxDB(dr("agstatussebelumnya"), 0), sptField,
                     FxDB(dr("agjmlrevisi"), 0), sptField,
                     FxDB(dr("agcetakanke"), 0), sptField,
                     FxDB(dr("aginputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aginputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("agmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("agmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("agposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("agpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("agtutupperiode"), 0), sptField,
                     FxDB(dr("agisclose"), 0), sptField,
                     FxDB(dr("agcabangnama"), ""), sptField,
                     FxDB(dr("aglokasinama"), ""), sptField,
                     FxDB(dr("agbagianagkode"), ""), sptField,
                     FxDB(dr("agbagianagnama"), ""), sptField,
                     FxDB(dr("agstatusnama"), ""), sptField,
                     FxDB(dr("agstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("aginputusernama"), ""), sptField,
                     FxDB(dr("agmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("agid, agcabang, aglokasi, agsumber, agautonotransaksi, agnotransaksi, agtgl, agkodepa, agbagianag, agbagianagkontak, agmatauang, agkurs, aguraian, agcatatan,agnoref, agtglnoref, agstatus, agstatussebelumnya, agjmlrevisi, agcetakanke, aginputuser, aginputtgl, agmodifikasiuser, agmodifikasitgl, agposting, agpostingtgl, agtutupperiode, agisclose, agcabangnama, aglokasinama, agbagianagkode, agbagianagnama, agstatusnama, agstatussebelumnyanama, aginputusernama, agmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M7_AgUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("agbagianagkode", "c1.kkode")
            Filter = Filter.Replace("agbagianagnama", "c1.knama")
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
            Dim sumber As String = "AG", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Agtgl, Agnotransaksi, Agstatus FROM M7_Ag WHERE Agid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Agstatussebelumnya" : jnsaktivitas = 17
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
            'Dim SimpanHistory As New m3_ib_history
            'Dim rsSimpanHistory As String = SimpanHistory.M3_Ib_HistorySimpan("" & paramSplit(0) & "★M3_Ib_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            'If (rsSplitResult(1) = 0) Then
            '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            'End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then

                'CEK TERKAIT ====================================================================
                'PANGGIL QUERY TERKAIT
                Dim query As New m0_query
                sql = query.PanggilQuery("m7_ag_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================



                'UPDATE STOK DAN OUTSTANDING ====================================================
                Dim ftHppI As String = "", ftHppF As String = ""
                Dim ftExistStok As String = "", ftStok As String = ""
                Dim updStokOut As String = "", gudangOut As String = ""
                Dim updStokBarang As String = "", ftStokBarang As String = ""
                Dim idbarang As Integer = 0, idibdetail As Integer = 0, jmlbarang As Double = 0

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDB("SELECT idibdetail, idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, gudang, urutan FROM M3_Ib_detail WHERE idib = '" & idtransaksi & "'")
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        '1. SET NILAI
                        idbarang = dr1("idbarang") : idibdetail = dr1("idibdetail") : jmlbarang = dr1("jmlbarang") : gudangOut = dr1("gudang")

                        '2. BUAT FILTER CEK HPP KHUSUS(I)
                        ftHppI = IIf(Len(ftHppI.ToString) = 0, "", ftHppI & " OR ")
                        ftHppI = String.Concat(ftHppI, "(idbarang = '" & idbarang & "' AND idtransaksi = '" & idibdetail & "' AND sumber = 'IB')")

                        '3. BUAT FILER CEK HPP FIFO(F)
                        ftHppF = IIf(Len(ftHppF.ToString) = 0, "", ftHppF & " OR ")
                        ftHppF = String.Concat(ftHppF, "(cfiidbarang = '" & idbarang & "' AND cfiidtransaksi = '" & idibdetail & "' AND cfisumber = 'IB')")

                        '4. BUAT FILTER CEK STOCK EXIST
                        ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                        ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                        '5. BUAT FILTER CEK JML STOCK
                        Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudang='" & gudangOut & "'")
                        ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
                        'ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")
                        ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > isw.stok) ")

                        '6. SET NILAI UPDATE STOK KELUAR
                        updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
                        updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

                        '8 SET NILAI UPDATE STOK BARANG
                        Dim stokBarang As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang)
                        updStokBarang = String.Concat("WHEN '" & idbarang & "' THEN ROUND(bstok - '" & stokBarang & "', 5) ", updStokBarang)

                        '9. SET FILTERUPDATE STOK BARANG
                        ftStokBarang = IIf(Len(ftStokBarang.ToString) = 0, "", ftStokBarang & " OR ")
                        ftStokBarang = String.Concat(ftStokBarang, "(bid = '" & idbarang & "')")

                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                'VALIDASI HPP, STOK ==========================================================
                'ValidasiSimpan
                'Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftHppI, ftHppF, ftExistStok, ftStok)
                'If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'END OF VALIDASI HPP, STOK ===================================================


                'DELETE HPP KHUSUS (I)
                sql = "DELETE FROM m1_cogs_special_in WHERE " & ftHppI
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()


                'DELETE HPP FIFO (F)
                sql = "DELETE FROM m1_cogs_fifo_in WHERE " & ftHppF
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()


                'DELETE NO BATCH IN MASUK ---------------------------
                sql = "DELETE FROM m1_no_batch_in WHERE nbisumber = '" & sumber & "' AND nbiidtransaksi = '" & idtransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()


                'DELETE NO SERIAL IN MASUK --------------------------
                sql = "DELETE FROM m1_no_serial_in WHERE nsisumber = '" & sumber & "' AND nsiidtransaksi = '" & idtransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()


                'UPDATE STOK ==================================================================
                'STOK KELUAR
                sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokOut & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'STOK BARANG m1_item
                sql = "UPDATE m1_item SET bstok = (CASE bid " & updStokBarang & " ELSE bstok END) WHERE " & ftStokBarang
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF UPDATE STOK ===========================================================


                'DELETE TRANSAKSI BARANG ======================================================
                'HAPUS DI M1_ITEM_TRANSACTION
                sql = "DELETE FROM m1_item_transaction WHERE sumber = '" & sumber & "' AND idutama = '" & idtransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF DELETE TRANSAKSI BARANG ===============================================


                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = '" & sumber & "' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M3_Ib SET IBstatus = " & nilaiStatus & ", IBmodifikasiuser='" & userid & "', IBmodifikasitgl = NOW(), IBposting = 0, IBpostingtgl = '1971-01-01 00:00:00', IBjmlrevisi = IBjmlrevisi + 1 WHERE IBid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M7_AgSearch(PostWsSearch(paramSplit(0), "M3_IbSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M3_IbDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("ibbagianibkode", "c1.kkode")
            Filter = Filter.Replace("ibbagianibnama", "c1.knama")
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
            Dim sumber As String = "Ib", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Ibid, Ibnotransaksi FROM M3_Ib WHERE Ibid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT ibcabang, iblokasi, ibsumber, ibautonotransaksi, ibnotransaksi, ibtgl"
            sql &= " FROM M3_ib"
            sql &= " WHERE ibid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("ibcabang")
                lokasi = dtNomorNext.Rows(0)("iblokasi")
                sumber = dtNomorNext.Rows(0)("ibsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("ibautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("ibnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("ibtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'HAPUS BATCH
            sql = "Delete from M1_No_Batch_Transaction where nbtidtransaksi = '" & idtransaksi & "' AND nbtsumber = '" & sumber & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'HAPUS SERIAL
            sql = "Delete from M1_No_Serial_Transaction where nstidtransaksi = '" & idtransaksi & "' AND nstsumber = '" & sumber & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE DETAIL
            sql = "DELETE FROM M3_Ib_Detail WHERE idIb = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M3_Ib WHERE Ibid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M7_AgSearch(PostWsSearch(paramSplit(0), "M3_IbSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
