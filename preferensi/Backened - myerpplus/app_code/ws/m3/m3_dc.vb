Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m3_dc
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M3_DcSimpan(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataCheck(), dataRowCheck() As String

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
        'dcid(0) As Integer, dccabang(1) As String, dclokasi(2) As String, dcgudangasal(3) As String, dcgudangtujuan(4) As String, 
        'dcsumber(5) As String, dcautonotransaksi(6) As Integer, dcnotransaksi(7) As String, dctgl(8) As Date, dckodepa(9) As Integer, 
        'dcdimintaoleh(10) As Integer, dcdimintaolehkontak(11) As String, dcmintake(12) As Integer, dctgldipakai(13) As Date, dcuraian(14) As String, 
        'dccatatan(15) As String, dcnoref(16) As String, dctglnoref(17) As Date, dcstatusts(18) As Integer, dcstatusrs(19) As Integer, 
        'dcstatus(20) As Integer, dcstatussebelumnya(21) As Integer, dcjmlrevisi(22) As Integer, dccetakanke(23) As Integer, dcinputuser(24) As Integer, 
        'dcinputtgl(25) As DateTime, dcmodifikasiuser(26) As Integer, dcmodifikasitgl(27) As DateTime, dcisclose(28) As Integer, dccustomtext1(29) As String, 
        'dccustomtext2(30) As String, dccustomtext3(31) As String, dccustomtext4(32) As String, dccustomtext5(33) As String, dccustomint1(34) As Integer, 
        'dccustomint2(35) As Integer, dccustomint3(36) As Integer, dccustomdbl1(37) As Double, dccustomdbl2(38) As Double, dccustomdbl3(39) As Double, 
        'dccustomdate1(40) As Date, dccustomdate2(41) As Date, dccustomdate3(42) As Date,
        'dcshift(43) As Integer, dcidbarang(44) As Integer, dcnamabarang(45) As String, dctipebarang(46) As String, 
        'dchmstart(47) As Double, dchmstop(48) As Double, dchmtotal(49) As Double

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'dcid, dccabang, dclokasi, dcgudangasal, dcgudangtujuan, dcsumber, dcautonotransaksi, 
        'dcnotransaksi, dctgl, dckodepa, dcdimintaoleh, dcdimintaolehkontak, dcmintake, dctgldipakai, 
        'dcuraian, dccatatan, dcnoref, dctglnoref, dcstatusts, dcstatusrs, dcstatus, 
        'dcstatussebelumnya, dcjmlrevisi, dccetakanke, dcinputuser, dcinputtgl, dcmodifikasiuser, dcmodifikasitgl, 
        'dcisclose, dccustomtext1, dccustomtext2, dccustomtext3, dccustomtext4, dccustomtext5, dccustomint1, 
        'dccustomint2, dccustomint3, dccustomdbl1, dccustomdbl2, dccustomdbl3, dccustomdate1, dccustomdate2, 
        'dccustomdate3, dcshift, dcidbarang, dcnamabarang, dctipebarang, 
        'dchmstart, dchmstop, dchmtotal


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 50) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI TIPE DATA UTAMA ==========================================================
        'dcid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "dcid required numeric." : GoTo selesai
        End If
        'dcautonotransaksi(6) As Integer
        If (IsNumeric(dataUtama(6)) = False) Then
            result(2) = "dcautonotransaksi required numeric." : GoTo selesai
        End If
        'dctgl(8) As Date
        If (IsDate(dataUtama(8)) = False) Then
            result(2) = "dctgl required date." : GoTo selesai
        End If
        'dckodepa(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "dckodepa required numeric." : GoTo selesai
        End If
        'dcdimintaoleh(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "dcdimintaoleh required numeric." : GoTo selesai
        End If
        'If (dataUtama(10) < 1) Then
        '    result(2) = "dcdimintaoleh can't be empty." : GoTo selesai
        'End If
        'dcmintake(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "dcmintake required numeric." : GoTo selesai
        End If
        'dctgldipakai(13) As Date
        If (IsDate(dataUtama(13)) = False) Then
            result(2) = "dctgldipakai required date." : GoTo selesai
        End If
        'dctglnoref(17) As Date
        If (IsDate(dataUtama(17)) = False) Then
            result(2) = "dctglnoref required date." : GoTo selesai
        End If
        'dcstatusts(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "dcstatusts required numeric." : GoTo selesai
        End If
        'dcstatusrs(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "dcstatusrs required numeric." : GoTo selesai
        End If
        'dcstatus(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "dcstatus required numeric." : GoTo selesai
        End If
        'dcstatussebelumnya(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "dcstatussebelumnya required numeric." : GoTo selesai
        End If
        'dcjmlrevisi(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "dcjmlrevisi required numeric." : GoTo selesai
        End If
        'dccetakanke(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "dccetakanke required numeric." : GoTo selesai
        End If
        'dcinputuser(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "dcinputuser required numeric." : GoTo selesai
        End If
        'dcinputtgl(25) As DateTime
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "dcinputtgl required date." : GoTo selesai
        End If
        'dcmodifikasiuser(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "dcmodifikasiuser required numeric." : GoTo selesai
        End If
        'dcmodifikasitgl(27) As DateTime
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "dcmodifikasitgl required date." : GoTo selesai
        End If
        'dcisclose(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "dcisclose required numeric." : GoTo selesai
        End If
        'dccustomint1(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "dccustomint1 required numeric." : GoTo selesai
        End If
        'dccustomint2(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "dccustomint2 required numeric." : GoTo selesai
        End If
        'dccustomint3(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "dccustomint3 required numeric." : GoTo selesai
        End If
        'dccustomdbl1(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "dccustomdbl1 required numeric." : GoTo selesai
        End If
        'dccustomdbl2(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "dccustomdbl2 required numeric." : GoTo selesai
        End If
        'dccustomdbl3(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "dccustomdbl3 required numeric." : GoTo selesai
        End If
        'dccustomdate1(40) As Date
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "dccustomdate1 required date." : GoTo selesai
        End If
        'dccustomdate2(41) As Date
        If (IsDate(dataUtama(41)) = False) Then
            result(2) = "dccustomdate2 required date." : GoTo selesai
        End If
        'dccustomdate3(42) As Date
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "dccustomdate3 required date." : GoTo selesai
        End If

        'dcshift(43) As Integer
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "dcshift required numeric." : GoTo selesai
        End If

        'dcidbarang(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "dcidbarang required numeric." : GoTo selesai
        End If

        'dchmstart(47) As Double
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "dchmstart required numeric." : GoTo selesai
        End If

        'dchmstop(48) As Double
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "dchmstop required numeric." : GoTo selesai
        End If

        'dchmtotal(49) As Double
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "dchmtotal required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================


        'VALIDASI DATA UTAMA =======================================================
        'dccabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "dccabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "dccabang should not be more than 25 character." : GoTo selesai
        End If

        'dclokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "dclokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "dclokasi should not be more than 25 character." : GoTo selesai
        End If

        'dcgudangasal(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "dcgudangasal can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "dcgudangasal should not be more than 25 character." : GoTo selesai
        End If

        'dcgudangtujuan(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "dcgudangtujuan can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 25 Then
            result(2) = "dcgudangtujuan should not be more than 25 character." : GoTo selesai
        End If

        'dcsumber(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "dcsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 10 Then
            result(2) = "dcsumber should not be more than 10 character." : GoTo selesai
        End If

        'dcnotransaksi(7) As String
        If Len(dataUtama(7)) = 0 Then
            result(2) = "dcnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 50 Then
            result(2) = "dcnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'dctgl(8) As Date
        If Len(dataUtama(8)) = 0 Then
            result(2) = "dctgl can't be empty" : GoTo selesai
        End If

        'dctgldipakai(13) As Date
        If Len(dataUtama(13)) = 0 Then
            result(2) = "dctgldipakai can't be empty" : GoTo selesai
        End If

        'dctglnoref(17) As Date
        If Len(dataUtama(17)) = 0 Then
            result(2) = "dctglnoref can't be empty" : GoTo selesai
        End If

        'dcinputtgl(25) As DateTime
        If Len(dataUtama(25)) = 0 Then
            result(2) = "dcinputtgl can't be empty" : GoTo selesai
        End If

        'dcmodifikasitgl(27) As DateTime
        If Len(dataUtama(27)) = 0 Then
            result(2) = "dcmodifikasitgl can't be empty" : GoTo selesai
        End If

        'dccustomdbl1(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "dccustomdbl1 can't be empty" : GoTo selesai
        End If

        'dccustomdbl2(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "dccustomdbl2 can't be empty" : GoTo selesai
        End If

        'dccustomdbl3(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "dccustomdbl3 can't be empty" : GoTo selesai
        End If

        'dccustomdate1(40) As Date
        If Len(dataUtama(40)) = 0 Then
            result(2) = "dccustomdate1 can't be empty" : GoTo selesai
        End If

        'dccustomdate2(41) As Date
        If Len(dataUtama(41)) = 0 Then
            result(2) = "dccustomdate2 can't be empty" : GoTo selesai
        End If

        'dccustomdate3(42) As Date
        If Len(dataUtama(42)) = 0 Then
            result(2) = "dccustomdate3 can't be empty" : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================


        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "dcid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dccabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dclokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dcgudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dcgudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dcsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dcautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dcnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dctgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dckodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dcdimintaoleh", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dcdimintaolehkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dcmintake", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dctgldipakai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dcuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dccatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dcnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dctglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dcstatusts", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dcstatusrs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dcstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dcstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dcjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dccetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dcinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dcinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dcmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dcmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dcisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dccustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dccustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dccustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dccustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dccustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dccustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dccustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dccustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dccustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dccustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dccustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dccustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dccustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dccustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dcshift", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dcidbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dcnamabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dctipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dchmstart", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dchmstop", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dchmtotal", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "dcid~dccabang~dclokasi~dcgudangasal~dcgudangtujuan~dcsumber~dcautonotransaksi~dcnotransaksi~dctgl~dckodepa~dcdimintaoleh~dcdimintaolehkontak~dcmintake~dctgldipakai~dcuraian~dccatatan~dcnoref~dctglnoref~dcstatusts~dcstatusrs~dcstatus~dcstatussebelumnya~dcjmlrevisi~dccetakanke~dcinputuser~dcinputtgl~dcmodifikasiuser~dcmodifikasitgl~dcisclose~dccustomtext1~dccustomtext2~dccustomtext3~dccustomtext4~dccustomtext5~dccustomint1~dccustomint2~dccustomint3~dccustomdbl1~dccustomdbl2~dccustomdbl3~dccustomdate1~dccustomdate2~dccustomdate3~dcshift~dcidbarang~dcnamabarang~dctipebarang~dchmstart~dchmstop~dchmtotal", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'iddcdetail(0) As Integer, iddc(1) As Integer, opstart(2) As String, opend(3) As String, sbstart(4) As String, 
        'sbend(5) As String, spstart(6) As String, spend(7) As String, rfstart(8) As String, rfend(9) As String, 
        'bdstart(10) As String, bdend(11) As String, cabang(12) As String, lokasi(13) As String, gudangasal(14) As String, 
        'gudangtujuan(15) As String, costcenter(16) As String, divisi(17) As String, subdivisi(18) As String, proyek(19) As String, 
        'catatan(20) As String, urutan(21) As Integer, jmlrealisasi(22) As Double, statusrealisasi(23) As Integer, isclose(24) As Integer, 
        'customtext1(25) As String, customtext2(26) As String, customtext3(27) As String, customdbl1(28) As Double, customdbl2(29) As Double, 
        'customdbl3(30) As Double, customdate1(31) As Date, customdate2(32) As Date, customdate3(33) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'iddcdetail, iddc, opstart, opend, sbstart, sbend, spstart, 
        'spend, rfstart, rfend, bdstart, bdend, cabang, lokasi, 
        'gudangasal, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "iddcdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "iddc", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtdetail, "opstart", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "opend", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sbstart", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sbend", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "spstart", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "spend", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rfstart", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rfend", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "bdstart", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "bdend", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtdetail, "jmlrealisasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusrealisasi", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt16)
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
            If (dataRowDetail.Length <> 34) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'iddcdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - iddcdetail required numeric." : GoTo selesai
            End If
            'iddc(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - iddc required numeric." : GoTo selesai
            End If
            'urutan(21) As Integer
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'jmlrealisasi(22) As Double
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - jmlrealisasi required numeric." : GoTo selesai
            End If
            'statusrealisasi(23) As Integer
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - statusrealisasi required numeric." : GoTo selesai
            End If
            'isclose(24) As Integer
            If (IsNumeric(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(28) As Double
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(29) As Double
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(30) As Double
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(31) As Date
            If (IsDate(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(32) As Date
            If (IsDate(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(33) As Date
            If (IsDate(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------


            'VALIDASI DATA DETAIL ---------------------------------------
            'opstart(2) As String
            If Len(dataRowDetail(2)) > 0 Then
                Dim strVal As String() = dataRowDetail(2).Split(".")
                If strVal.Length <> 2 Then
                    result(2) = "Row : " & i & " - invalid time format of operation start." : GoTo selesai
                End If
                If Double.Parse(strVal(0)) > 24 Or Double.Parse(strVal(0)) < 0 Then
                    result(2) = "Row : " & i & " - invalid time format of operation start. (hour)" : GoTo selesai
                End If
                If Double.Parse(strVal(1)) > 59 Or Double.Parse(strVal(1)) < 0 Then
                    result(2) = "Row : " & i & " - invalid time format of operation start. (minute)" : GoTo selesai
                End If
                'SET FORMAT TIME
                dataRowDetail(2) = String.Concat(strVal(0), ":", strVal(1), ":", "00")
            Else
                'SET FORMAT TIME
                dataRowDetail(2) = String.Concat("00", ":", "00", ":", "00")
            End If

            'opend(3) As String
            If Len(dataRowDetail(3)) > 0 Then
                Dim strVal As String() = dataRowDetail(3).Split(".")
                If strVal.Length <> 2 Then
                    result(2) = "Row : " & i & " - invalid time format of operation end." : GoTo selesai
                End If
                If Double.Parse(strVal(0)) > 24 Or Double.Parse(strVal(0)) < 0 Then
                    result(2) = "Row : " & i & " - invalid time format of operation end. (hour)" : GoTo selesai
                End If
                If Double.Parse(strVal(1)) > 59 Or Double.Parse(strVal(1)) < 0 Then
                    result(2) = "Row : " & i & " - invalid time format of operation end. (minute)" : GoTo selesai
                End If
                'SET FORMAT TIME
                dataRowDetail(3) = String.Concat(strVal(0), ":", strVal(1), ":", "00")
            Else
                'SET FORMAT TIME
                dataRowDetail(3) = String.Concat("00", ":", "00", ":", "00")
            End If

            'sbstart(4) As String
            If Len(dataRowDetail(4)) > 0 Then
                Dim strVal As String() = dataRowDetail(4).Split(".")
                If strVal.Length <> 2 Then
                    result(2) = "Row : " & i & " - invalid time format of standby start." : GoTo selesai
                End If
                If Double.Parse(strVal(0)) > 24 Or Double.Parse(strVal(0)) < 0 Then
                    result(2) = "Row : " & i & " - invalid time format of standby start. (hour)" : GoTo selesai
                End If
                If Double.Parse(strVal(1)) > 59 Or Double.Parse(strVal(1)) < 0 Then
                    result(2) = "Row : " & i & " - invalid time format of standby start. (minute)" : GoTo selesai
                End If
                'SET FORMAT TIME
                dataRowDetail(4) = String.Concat(strVal(0), ":", strVal(1), ":", "00")
            Else
                'SET FORMAT TIME
                dataRowDetail(4) = String.Concat("00", ":", "00", ":", "00")
            End If

            'sbend(5) As String
            If Len(dataRowDetail(5)) > 0 Then
                Dim strVal As String() = dataRowDetail(5).Split(".")
                If strVal.Length <> 2 Then
                    result(2) = "Row : " & i & " - invalid time format of standby end." : GoTo selesai
                End If
                If Double.Parse(strVal(0)) > 24 Or Double.Parse(strVal(0)) < 0 Then
                    result(2) = "Row : " & i & " - invalid time format of standby end. (hour)" : GoTo selesai
                End If
                If Double.Parse(strVal(1)) > 59 Or Double.Parse(strVal(1)) < 0 Then
                    result(2) = "Row : " & i & " - invalid time format of standby end. (minute)" : GoTo selesai
                End If
                'SET FORMAT TIME
                dataRowDetail(5) = String.Concat(strVal(0), ":", strVal(1), ":", "00")
            Else
                'SET FORMAT TIME
                dataRowDetail(5) = String.Concat("00", ":", "00", ":", "00")
            End If

            'spstart(6) As String
            If Len(dataRowDetail(6)) > 0 Then
                Dim strVal As String() = dataRowDetail(6).Split(".")
                If strVal.Length <> 2 Then
                    result(2) = "Row : " & i & " - invalid time format of rain/slippery start." : GoTo selesai
                End If
                If Double.Parse(strVal(0)) > 24 Or Double.Parse(strVal(0)) < 0 Then
                    result(2) = "Row : " & i & " - invalid time format of rain/slippery start. (hour)" : GoTo selesai
                End If
                If Double.Parse(strVal(1)) > 59 Or Double.Parse(strVal(1)) < 0 Then
                    result(2) = "Row : " & i & " - invalid time format of rain/slippery start. (minute)" : GoTo selesai
                End If
                'SET FORMAT TIME
                dataRowDetail(6) = String.Concat(strVal(0), ":", strVal(1), ":", "00")
            Else
                'SET FORMAT TIME
                dataRowDetail(6) = String.Concat("00", ":", "00", ":", "00")
            End If

            'spend(7) As String
            If Len(dataRowDetail(7)) > 0 Then
                Dim strVal As String() = dataRowDetail(7).Split(".")
                If strVal.Length <> 2 Then
                    result(2) = "Row : " & i & " - invalid time format of rain/slippery end." : GoTo selesai
                End If
                If Double.Parse(strVal(0)) > 24 Or Double.Parse(strVal(0)) < 0 Then
                    result(2) = "Row : " & i & " - invalid time format of rain/slippery end. (hour)" : GoTo selesai
                End If
                If Double.Parse(strVal(1)) > 59 Or Double.Parse(strVal(1)) < 0 Then
                    result(2) = "Row : " & i & " - invalid time format of rain/slippery end. (minute)" : GoTo selesai
                End If
                'SET FORMAT TIME
                dataRowDetail(7) = String.Concat(strVal(0), ":", strVal(1), ":", "00")
            Else
                'SET FORMAT TIME
                dataRowDetail(7) = String.Concat("00", ":", "00", ":", "00")
            End If

            'rfstart(8) As String
            If Len(dataRowDetail(8)) > 0 Then
                Dim strVal As String() = dataRowDetail(8).Split(".")
                If strVal.Length <> 2 Then
                    result(2) = "Row : " & i & " - invalid time format of waiting/refueling start." : GoTo selesai
                End If
                If Double.Parse(strVal(0)) > 24 Or Double.Parse(strVal(0)) < 0 Then
                    result(2) = "Row : " & i & " - invalid time format of waiting/refueling start. (hour)" : GoTo selesai
                End If
                If Double.Parse(strVal(1)) > 59 Or Double.Parse(strVal(1)) < 0 Then
                    result(2) = "Row : " & i & " - invalid time format of waiting/refueling start. (minute)" : GoTo selesai
                End If
                'SET FORMAT TIME
                dataRowDetail(8) = String.Concat(strVal(0), ":", strVal(1), ":", "00")
            Else
                'SET FORMAT TIME
                dataRowDetail(8) = String.Concat("00", ":", "00", ":", "00")
            End If

            'rfend(9) As String
            If Len(dataRowDetail(9)) > 0 Then
                Dim strVal As String() = dataRowDetail(9).Split(".")
                If strVal.Length <> 2 Then
                    result(2) = "Row : " & i & " - invalid time format of waiting/refueling end." : GoTo selesai
                End If
                If Double.Parse(strVal(0)) > 24 Or Double.Parse(strVal(0)) < 0 Then
                    result(2) = "Row : " & i & " - invalid time format of waiting/refueling end. (hour)" : GoTo selesai
                End If
                If Double.Parse(strVal(1)) > 59 Or Double.Parse(strVal(1)) < 0 Then
                    result(2) = "Row : " & i & " - invalid time format of waiting/refueling end. (minute)" : GoTo selesai
                End If
                'SET FORMAT TIME
                dataRowDetail(9) = String.Concat(strVal(0), ":", strVal(1), ":", "00")
            Else
                'SET FORMAT TIME
                dataRowDetail(9) = String.Concat("00", ":", "00", ":", "00")
            End If

            'bdstart(10) As String
            If Len(dataRowDetail(10)) > 0 Then
                Dim strVal As String() = dataRowDetail(10).Split(".")
                If strVal.Length <> 2 Then
                    result(2) = "Row : " & i & " - invalid time format of breakdown start." : GoTo selesai
                End If
                If Double.Parse(strVal(0)) > 24 Or Double.Parse(strVal(0)) < 0 Then
                    result(2) = "Row : " & i & " - invalid time format of breakdown start. (hour)" : GoTo selesai
                End If
                If Double.Parse(strVal(1)) > 59 Or Double.Parse(strVal(1)) < 0 Then
                    result(2) = "Row : " & i & " - invalid time format of breakdown start. (minute)" : GoTo selesai
                End If
                'SET FORMAT TIME
                dataRowDetail(10) = String.Concat(strVal(0), ":", strVal(1), ":", "00")
            Else
                'SET FORMAT TIME
                dataRowDetail(10) = String.Concat("00", ":", "00", ":", "00")
            End If

            'bdend(11) As String
            If Len(dataRowDetail(11)) > 0 Then
                Dim strVal As String() = dataRowDetail(11).Split(".")
                If strVal.Length <> 2 Then
                    result(2) = "Row : " & i & " - invalid time format of breakdown end." : GoTo selesai
                End If
                If Double.Parse(strVal(0)) > 24 Or Double.Parse(strVal(0)) < 0 Then
                    result(2) = "Row : " & i & " - invalid time format of breakdown end. (hour)" : GoTo selesai
                End If
                If Double.Parse(strVal(1)) > 59 Or Double.Parse(strVal(1)) < 0 Then
                    result(2) = "Row : " & i & " - invalid time format of breakdown end. (minute)" : GoTo selesai
                End If
                'SET FORMAT TIME
                dataRowDetail(11) = String.Concat(strVal(0), ":", strVal(1), ":", "00")
            Else
                'SET FORMAT TIME
                dataRowDetail(11) = String.Concat("00", ":", "00", ":", "00")
            End If

            'catatan(20) As String
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - catatan can't be empty" : GoTo selesai
            End If

            'jmlrealisasi(22) As Double
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - jmlrealisasi can't be empty" : GoTo selesai
            End If

            'customdbl1(28) As Double
            If Len(dataRowDetail(28)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(29) As Double
            If Len(dataRowDetail(29)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(30) As Double
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(31) As Date
            If Len(dataRowDetail(31)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(32) As Date
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(33) As Date
            If Len(dataRowDetail(33)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "iddcdetail~iddc~opstart~opend~sbstart~sbend~spstart~spend~rfstart~rfend~bdstart~bdend~cabang~lokasi~gudangasal~gudangtujuan~costcenter~divisi~subdivisi~proyek~catatan~urutan~jmlrealisasi~statusrealisasi~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'MAPPING BUAT WS DATA Check -------------------------------------------------------
        'iddccheck(0) As Integer, iddc(1) As Integer, idkategoricheck(2) As Integer, catatan(3) As String, status(4) As Integer, 
        'urutan(5) As Integer, isclose(6) As Integer, customtext1(7) As String, customtext2(8) As String, customtext3(9) As String, 
        'customdbl1(10) As Double, customdbl2(11) As Double, customdbl3(12) As Double, customdate1(13) As Date, customdate2(14) As Date, 
        'customdate3(15) As Date

        'MAPPING BUAT FLEX DATA Check -----------------------------------------------------
        'iddccheck, iddc, idkategoricheck, catatan, status, urutan, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA Check ======================================================
        'SPLIT PARAMETER DATA Check
        dataCheck = dataSplit(2).Split(sptRow)
        'END OF VALIDASI DAN SET DATA Check ===============================================

        'Buat datatable Check
        Dim dtCheck As New DataTable
        AsDataTableTambahField(dtCheck, "iddccheck", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCheck, "iddc", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtCheck, "idkategoricheck", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtCheck, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCheck, "status", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtCheck, "urutan", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtCheck, "isclose", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtCheck, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCheck, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCheck, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCheck, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCheck, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCheck, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCheck, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCheck, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCheck, "customdate3", AsEnumTypeData.AsString)

        'VALIDASI DAN SET DATA ROW Check ==================================================
        Dim JmlDtCheck As Integer = dataCheck.Length
        For i = 1 To JmlDtCheck
            'SPLIT DATA Check
            dataRowCheck = dataCheck(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA Check -----------------------------------
            'CEK ARRAY DATA Check
            If (dataRowCheck.Length <> 16) Then
                result(2) = "Check Row : " & i & " - Invalid Check transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW Check ----------------------------

            'VALIDASI TIPE DATA Check ------------------------------------------
            'iddccheck(0) As Integer
            If (IsNumeric(dataRowCheck(0)) = False) Then
                result(2) = "Check Row : " & i & " - ciddccheck required numeric." : GoTo selesai
            End If
            'iddc(1) As Integer
            If (IsNumeric(dataRowCheck(1)) = False) Then
                result(2) = "Check Row : " & i & " - ciddc required numeric." : GoTo selesai
            End If
            'idkategoricheck(2) As Integer
            If (IsNumeric(dataRowCheck(2)) = False) Then
                result(2) = "Check Row : " & i & " - cidkategoricheck required numeric." : GoTo selesai
            End If
            'status(4) As Integer
            If (IsNumeric(dataRowCheck(4)) = False) Then
                result(2) = "Check Row : " & i & " - cstatus required numeric." : GoTo selesai
            End If
            'urutan(5) As Integer
            If (IsNumeric(dataRowCheck(5)) = False) Then
                result(2) = "Check Row : " & i & " - curutan required numeric." : GoTo selesai
            End If
            'isclose(6) As Integer
            If (IsNumeric(dataRowCheck(6)) = False) Then
                result(2) = "Check Row : " & i & " - cisclose required numeric." : GoTo selesai
            End If
            'customdbl1(10) As Double
            If (IsNumeric(dataRowCheck(10)) = False) Then
                result(2) = "Check Row : " & i & " - ccustomdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(11) As Double
            If (IsNumeric(dataRowCheck(11)) = False) Then
                result(2) = "Check Row : " & i & " - ccustomdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(12) As Double
            If (IsNumeric(dataRowCheck(12)) = False) Then
                result(2) = "Check Row : " & i & " - ccustomdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(13) As Date
            If (IsDate(dataRowCheck(13)) = False) Then
                result(2) = "Check Row : " & i & " - ccustomdate1 required date." : GoTo selesai
            End If
            'customdate2(14) As Date
            If (IsDate(dataRowCheck(14)) = False) Then
                result(2) = "Check Row : " & i & " - ccustomdate2 required date." : GoTo selesai
            End If
            'customdate3(15) As Date
            If (IsDate(dataRowCheck(15)) = False) Then
                result(2) = "Check Row : " & i & " - ccustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA Check -----------------------------------

            'VALIDASI DATA Check ---------------------------------------
            'customdbl1(10) As Double
            If Len(dataRowCheck(10)) = 0 Then
                result(2) = "Check Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(11) As Double
            If Len(dataRowCheck(11)) = 0 Then
                result(2) = "Check Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(12) As Double
            If Len(dataRowCheck(12)) = 0 Then
                result(2) = "Check Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(13) As Date
            If Len(dataRowCheck(13)) = 0 Then
                result(2) = "Check Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(14) As Date
            If Len(dataRowCheck(14)) = 0 Then
                result(2) = "Check Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(15) As Date
            If Len(dataRowCheck(15)) = 0 Then
                result(2) = "Check Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA Check --------------------------------

            If AsDataTableTambahData(dtCheck, "iddccheck~iddc~idkategoricheck~catatan~status~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowCheck(0) & "~" & dataRowCheck(1) & "~" & dataRowCheck(2) & "~" & dataRowCheck(3) & "~" & dataRowCheck(4) & "~" & dataRowCheck(5) & "~" & dataRowCheck(6) & "~" & dataRowCheck(7) & "~" & dataRowCheck(8) & "~" & dataRowCheck(9) & "~" & dataRowCheck(10) & "~" & dataRowCheck(11) & "~" & dataRowCheck(12) & "~" & dataRowCheck(13) & "~" & dataRowCheck(14) & "~" & dataRowCheck(15)) = False Then
                result(2) = "Check Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

        Next
        'END OF VALIDASI DAN SET ROW DATA Check ===========================================


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


                'CEK HAK AKSES STATUS ============================
                Dim vAkses As Integer = 0, msgAkses As String = ""
                'MODUL DAN MENU HARUS DISESUAIKAN
                Dim vModuleId As Integer = 3, vMenuId As Integer = 41
                Select Case drutama("dcstatus")
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("dctgl")), AsFormatTanggal(drutama("dctgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================

                If isUpdate Then
                    result(4) = drutama("dcid")
                    notransaksi = drutama("dcnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(dcid), dcnotransaksi FROM M3_Dc WHERE dcid='" & result(4) & "' AND dcstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("dcautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("dccabang"), drutama("dclokasi"), drutama("dcsumber"), drutama("dctgl"))
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

                        End If

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(dcid) FROM m3_dc WHERE dcnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m3_dc_history
                        Dim rsSimpanHistory As String = SimpanHistory.M3_Dc_HistorySimpan("" & paramSplit(0) & "★M3_Dc_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("dcsumber")) & "▼" & FixQuotes(drutama("dcid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M3_Dc set dccabang  = '" & FixQuotes(drutama("dccabang")) & "', dclokasi  = '" & FixQuotes(drutama("dclokasi")) & "', dcgudangasal  = '" & FixQuotes(drutama("dcgudangasal")) & "', dcgudangtujuan  = '" & FixQuotes(drutama("dcgudangtujuan")) & "', dcsumber  = '" & FixQuotes(drutama("dcsumber")) & "', dcautonotransaksi  = " & drutama("dcautonotransaksi") & ", dcnotransaksi  = '" & notransaksi & "', dctgl  = '" & FixQuotes(AsFormatTanggal(drutama("dctgl"))) & "', dckodepa  = " & drutama("dckodepa") & ", dcdimintaoleh  = " & drutama("dcdimintaoleh") & ", dcdimintaolehkontak  = '" & FixQuotes(drutama("dcdimintaolehkontak")) & "', dcmintake  = " & drutama("dcmintake") & ", dctgldipakai  = '" & FixQuotes(AsFormatTanggal(drutama("dctgldipakai"))) & "', dcuraian  = '" & FixQuotes(drutama("dcuraian")) & "', dccatatan  = '" & FixQuotes(drutama("dccatatan")) & "', dcnoref  = '" & FixQuotes(drutama("dcnoref")) & "', dctglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("dctglnoref"))) & "', dcstatusts  = " & drutama("dcstatusts") & ", dcstatusrs  = " & drutama("dcstatusrs") & ", dcstatus  = " & drutama("dcstatus") & ", dcstatussebelumnya  = " & drutama("dcstatussebelumnya") & ", dcjmlrevisi  = dcjmlrevisi+1, dccetakanke  = " & drutama("dccetakanke") & ", dcmodifikasiuser  = " & drutama("dcmodifikasiuser") & ", dcmodifikasitgl  = NOW(), dccustomtext1  = '" & FixQuotes(drutama("dccustomtext1")) & "', dccustomtext2  = '" & FixQuotes(drutama("dccustomtext2")) & "', dccustomtext3  = '" & FixQuotes(drutama("dccustomtext3")) & "', dccustomtext4  = '" & FixQuotes(drutama("dccustomtext4")) & "', dccustomtext5  = '" & FixQuotes(drutama("dccustomtext5")) & "', dccustomint1  = " & drutama("dccustomint1") & ", dccustomint2  = " & drutama("dccustomint2") & ", dccustomint3  = " & drutama("dccustomint3") & ", dccustomdbl1  = '" & FixDouble(drutama("dccustomdbl1")) & "', dccustomdbl2  = '" & FixDouble(drutama("dccustomdbl2")) & "', dccustomdbl3  = '" & FixDouble(drutama("dccustomdbl3")) & "', dccustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("dccustomdate1"))) & "', dccustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("dccustomdate2"))) & "', dccustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("dccustomdate3"))) & "', dcshift  = '" & FixQuotes(drutama("dcshift")) & "', dcidbarang  = '" & FixQuotes(drutama("dcidbarang")) & "', dcnamabarang  = '" & FixQuotes(drutama("dcnamabarang")) & "', dctipebarang  = '" & FixQuotes(drutama("dctipebarang")) & "', dchmstart  = '" & FixQuotes(drutama("dchmstart")) & "', dchmstop  = '" & FixQuotes(drutama("dchmstop")) & "', dchmtotal  = '" & FixQuotes(drutama("dchmtotal")) & "' where dcid = '" & drutama("dcid") & "'"
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

                    If drutama("dcautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("dccabang"), drutama("dclokasi"), drutama("dcsumber"), drutama("dctgl"))
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
                        notransaksi = drutama("dcnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(dcid) FROM m3_dc WHERE dcnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============


                    sql = "Insert into M3_Dc (dccabang, dclokasi, dcgudangasal, dcgudangtujuan, dcsumber, dcautonotransaksi, dcnotransaksi, dctgl, dckodepa, dcdimintaoleh, dcdimintaolehkontak, dcmintake, dctgldipakai, dcuraian, dccatatan, dcnoref, dctglnoref, dcstatusts, dcstatusrs, dcstatus, dcstatussebelumnya, dcjmlrevisi, dccetakanke, dcinputuser, dcinputtgl, dcmodifikasiuser, dcmodifikasitgl, dcisclose, dccustomtext1, dccustomtext2, dccustomtext3, dccustomtext4, dccustomtext5, dccustomint1, dccustomint2, dccustomint3, dccustomdbl1, dccustomdbl2, dccustomdbl3, dccustomdate1, dccustomdate2, dccustomdate3, dcshift, dcidbarang, dcnamabarang, dctipebarang, dchmstart, dchmstop, dchmtotal) values('" & FixQuotes(drutama("dccabang")) & "', '" & FixQuotes(drutama("dclokasi")) & "', '" & FixQuotes(drutama("dcgudangasal")) & "', '" & FixQuotes(drutama("dcgudangtujuan")) & "', '" & FixQuotes(drutama("dcsumber")) & "', " & drutama("dcautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("dctgl"))) & "', " & drutama("dckodepa") & ", " & drutama("dcdimintaoleh") & ", '" & FixQuotes(drutama("dcdimintaolehkontak")) & "', " & drutama("dcmintake") & ", '" & FixQuotes(AsFormatTanggal(drutama("dctgldipakai"))) & "', '" & FixQuotes(drutama("dcuraian")) & "', '" & FixQuotes(drutama("dccatatan")) & "', '" & FixQuotes(drutama("dcnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("dctglnoref"))) & "', " & drutama("dcstatusts") & ", " & drutama("dcstatusrs") & ", " & drutama("dcstatus") & ", " & drutama("dcstatussebelumnya") & ", " & drutama("dcjmlrevisi") & ", " & drutama("dccetakanke") & ", " & drutama("dcinputuser") & ", NOW(), " & drutama("dcmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("dcisclose") & ", '" & FixQuotes(drutama("dccustomtext1")) & "', '" & FixQuotes(drutama("dccustomtext2")) & "', '" & FixQuotes(drutama("dccustomtext3")) & "', '" & FixQuotes(drutama("dccustomtext4")) & "', '" & FixQuotes(drutama("dccustomtext5")) & "', " & drutama("dccustomint1") & ", " & drutama("dccustomint2") & ", " & drutama("dccustomint3") & ", '" & FixDouble(drutama("dccustomdbl1")) & "', '" & FixDouble(drutama("dccustomdbl2")) & "', '" & FixDouble(drutama("dccustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("dccustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("dccustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("dccustomdate3"))) & "', '" & FixQuotes(drutama("dcshift")) & "', '" & FixQuotes(drutama("dcidbarang")) & "', '" & FixQuotes(drutama("dcnamabarang")) & "', '" & FixQuotes(drutama("dctipebarang")) & "', '" & FixQuotes(drutama("dchmstart")) & "', '" & FixQuotes(drutama("dchmstop")) & "', '" & FixQuotes(drutama("dchmtotal")) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select dcid from M3_Dc where dcnotransaksi='" & notransaksi & "' AND Dcinputuser= '" & userid & "' order by Dcmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M3_Dc_Detail where iddc = '" & result(4) & "'"
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
                        strValue2.Append("('" & FixQuotes(dr1("iddcdetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("opstart")) & "', '" & FixQuotes(dr1("opend")) & "', '" & FixQuotes(dr1("sbstart")) & "', '" & FixQuotes(dr1("sbend")) & "', '" & FixQuotes(dr1("spstart")) & "', '" & FixQuotes(dr1("spend")) & "', '" & FixQuotes(dr1("rfstart")) & "', '" & FixQuotes(dr1("rfend")) & "', '" & FixQuotes(dr1("bdstart")) & "', '" & FixQuotes(dr1("bdend")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", '" & FixDouble(dr1("jmlrealisasi")) & "', " & dr1("statusrealisasi") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M3_Dc_Detail(iddcdetail, iddc, opstart, opend, sbstart, sbend, spstart, spend, rfstart, rfend, bdstart, bdend, cabang, lokasi, gudangasal, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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


                'Hapus check ketika update
                If (isUpdate) Then
                    sql = "Delete from M3_Dc_Check where iddc = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                'Proses Check
                If (dtCheck.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtCheck.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("iddccheck") & ", " & result(4) & ", " & dr1("idkategoricheck") & ", '" & FixQuotes(dr1("catatan")) & "', " & dr1("status") & ", " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M3_Dc_Check(iddccheck, iddc, idkategoricheck, catatan, status, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                'UPDATE HOUEMETER ITEM HAULING =====================================================
                If drutama("dcstatus") = 2 Then
                    'UPDATE HOURMETER KE MASTER DATA BARANG HAULING
                    sql = "UPDATE m1_item_hauling SET bahourmeter = '" & Double.Parse(drutama("dchmstop")) & "' WHERE bid = '" & Double.Parse(drutama("dcidbarang")) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE HOUEMETER ITEM HAULING ==============================================


                'INSERT USER LOG ====================================================================
                Dim sumber As String = "DC", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M3_DcUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("dcdimintaolehkode", "c1.kkode")
            Filter = Filter.Replace("dcdimintaolehnama", "c1.knama")
            Filter = Filter.Replace("dcmintakekode", "c2.kkode")
            Filter = Filter.Replace("dcmintakenama", "c2.knama")
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
            Dim sumber As String = "Dc", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Dctgl, Dcnotransaksi, Dcstatus FROM m3_Dc WHERE Dcid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Dcstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m3_dc_history
            Dim rsSimpanHistory As String = SimpanHistory.M3_Dc_HistorySimpan("" & paramSplit(0) & "★M3_Dc_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================


            If isDelete Then

                ''CEK TERKAIT ====================================================================
                ''PANGGIL QUERY TERKAIT
                'Dim query As New m0_query
                'sql = query.m3_dc_terkait("dcid = '" & idtransaksi & "'")
                'Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                'dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                'If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                ''END OF CEK TERKAIT =============================================================

                sql = "SELECT dcidbarang, dchmtotal FROM m3_dc WHERE dcid = '" & idtransaksi & "'"
                dtdetail = AsDataTableAmbilDariDB(sql)
                If dtdetail.Rows.Count > 0 Then
                    'UPDATE HOURMETER KE MASTER DATA BARANG HAULING
                    sql = "UPDATE m1_item_hauling SET bahourmeter = bahourmeter - " & Double.Parse(dtdetail.Rows(0)("dchmtotal")) & " WHERE bid = '" & Double.Parse(dtdetail.Rows(0)("dcidbarang")) & "'"
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

            'update status utama
            sql = "UPDATE M3_Dc SET Dcstatus = " & nilaiStatus & ", dcmodifikasiuser='" & userid & "', dcmodifikasitgl = NOW(), dcposting = 0, dcpostingtgl = '1971-01-01 00:00:00', dcjmlrevisi = dcjmlrevisi + 1 WHERE dcid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M3_DcSearch(PostWsSearch(paramSplit(0), "M3_DcSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M3_DcDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("dcdimintaolehkode", "c1.kkode")
            Filter = Filter.Replace("dcdimintaolehnama", "c1.knama")
            Filter = Filter.Replace("dcmintakekode", "c2.kkode")
            Filter = Filter.Replace("dcmintakenama", "c2.knama")
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
            Dim sumber As String = "Dc", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Dcid, Dcnotransaksi FROM m3_Dc WHERE Dcid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT dccabang, dclokasi, dcsumber, dcautonotransaksi, dcnotransaksi, dctgl"
            sql &= " FROM M3_dc"
            sql &= " WHERE dcid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("dccabang")
                lokasi = dtNomorNext.Rows(0)("dclokasi")
                sumber = dtNomorNext.Rows(0)("dcsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("dcautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("dcnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("dctgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE CHECK
            sql = "DELETE FROM M3_Dc_Check WHERE iddc = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M3_Dc_Detail WHERE iddc = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M3_Dc WHERE dcid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M3_DcSearch(PostWsSearch(paramSplit(0), "M3_DcSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M3_DcGetdataById(ByVal param As String) As String

        'M3_DcGetdataById Utama --------------------------------------------------------
        'dcid, dccabang, dclokasi, dcgudangasal, dcgudangtujuan, dcsumber, dcautonotransaksi, 
        'dcnotransaksi, dctgl, dckodepa, dcdimintaoleh, dcdimintaolehkontak, dcmintake, dctgldipakai, 
        'dcuraian, dccatatan, dcnoref, dctglnoref, dcstatusts, dcstatusrs, dcstatusrealisasi, 
        'dcstatus, dcstatussebelumnya, dcjmlrevisi, dccetakanke, dcinputuser, dcinputtgl, dcmodifikasiuser, 
        'dcmodifikasitgl, dcposting, dcpostingtgl, dcisclose, dccustomtext1, dccustomtext2, dccustomtext3, 
        'dccustomtext4, dccustomtext5, dccustomint1, dccustomint2, dccustomint3, dccustomdbl1, dccustomdbl2, 
        'dccustomdbl3, dccustomdate1, dccustomdate2, dccustomdate3, dccabangnama, dclokasinama, dcgudangasalnama, 
        'dcgudangtujuannama, dcdimintaolehkode, dcdimintaolehnama, dcmintakekode, dcmintakenama, dcstatusnama, dcstatussebelumnyanama, 
        'dcinputusernama, dcmodifikasiusernama, dcshift, dcidbarang, dcnamabarang, dctipebarang, dchmstart, 
        'dchmstop, dchmtotal, dckodebarang

        'M3_DcGetdataById Detail -------------------------------------------------------
        'iddcdetail, iddc, opstart, opend, 
        'sbstart, sbend, spstart, spend, rfstart, rfend, bdstart, 
        'bdend, cabang, lokasi, gudangasal, gudangtujuan, costcenter, divisi, 
        'subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3, cabangnama, lokasinama, gudangasalnama, gudangtujuannama, costcenternama, 
        'divisinama, subdivisinama, proyeknama

        'M3_DcGetdataById Check --------------------------------------------------------
        'iddccheck, iddc, idkategoricheck, catatan, status, urutan, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3, ccnama

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

        Dim utama As String = "", detail As String = "", detailCheck As String = "", idtransaksi As String = ""

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


        Dim NmMemcached As String = "aplikasi1-M3_Dc~M3_Dc_Detail-" & idtransaksi
        Dim Filter2 As String = ""

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "dcid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "dcid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        'Set filter detail 2
        If Len(Filter2) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter2 = "iddc = '" & idtransaksi & "'"
        Else ' jika filter diisi
            Filter2 = "iddc = '" & idtransaksi & "' and " & Filter2
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m3_dc_getdata")
        sql = "select `dc`.`dcid` AS `dcid`,`dc`.`dccabang` AS `dccabang`,`dc`.`dclokasi` AS `dclokasi`,`dc`.`dcgudangasal` AS `dcgudangasal`,`dc`.`dcgudangtujuan` AS `dcgudangtujuan`,`dc`.`dcsumber` AS `dcsumber`,`dc`.`dcautonotransaksi` AS `dcautonotransaksi`,`dc`.`dcnotransaksi` AS `dcnotransaksi`,`dc`.`dctgl` AS `dctgl`,`dc`.`dckodepa` AS `dckodepa`,`dc`.`dcdimintaoleh` AS `dcdimintaoleh`,`dc`.`dcdimintaolehkontak` AS `dcdimintaolehkontak`,`dc`.`dcmintake` AS `dcmintake`,`dc`.`dctgldipakai` AS `dctgldipakai`,`dc`.`dcuraian` AS `dcuraian`,`dc`.`dccatatan` AS `dccatatan`,`dc`.`dcnoref` AS `dcnoref`,`dc`.`dctglnoref` AS `dctglnoref`,`dc`.`dcstatusts` AS `dcstatusts`,`dc`.`dcstatusrs` AS `dcstatusrs`,`dc`.`dcstatusrealisasi` AS `dcstatusrealisasi`,`dc`.`dcstatus` AS `dcstatus`,`dc`.`dcstatussebelumnya` AS `dcstatussebelumnya`,`dc`.`dcjmlrevisi` AS `dcjmlrevisi`,`dc`.`dccetakanke` AS `dccetakanke`,`dc`.`dcinputuser` AS `dcinputuser`,`dc`.`dcinputtgl` AS `dcinputtgl`,`dc`.`dcmodifikasiuser` AS `dcmodifikasiuser`,`dc`.`dcmodifikasitgl` AS `dcmodifikasitgl`,`dc`.`dcposting` AS `dcposting`,`dc`.`dcpostingtgl` AS `dcpostingtgl`,`dc`.`dcisclose` AS `dcisclose`,`dc`.`dccustomtext1` AS `dccustomtext1`,`dc`.`dccustomtext2` AS `dccustomtext2`,`dc`.`dccustomtext3` AS `dccustomtext3`,`dc`.`dccustomtext4` AS `dccustomtext4`,`dc`.`dccustomtext5` AS `dccustomtext5`,`dc`.`dccustomint1` AS `dccustomint1`,`dc`.`dccustomint2` AS `dccustomint2`,`dc`.`dccustomint3` AS `dccustomint3`,`dc`.`dccustomdbl1` AS `dccustomdbl1`,`dc`.`dccustomdbl2` AS `dccustomdbl2`,`dc`.`dccustomdbl3` AS `dccustomdbl3`,`dc`.`dccustomdate1` AS `dccustomdate1`,`dc`.`dccustomdate2` AS `dccustomdate2`,`dc`.`dccustomdate3` AS `dccustomdate3`,`br`.`bnama` AS `dccabangnama`,`lc`.`lnama` AS `dclokasinama`,`wh1`.`wnama` AS `dcgudangasalnama`,`wh2`.`wnama` AS `dcgudangtujuannama`,`c1`.`kkode` AS `dcdimintaolehkode`,`c1`.`knama` AS `dcdimintaolehnama`,`c2`.`kkode` AS `dcmintakekode`,`c2`.`knama` AS `dcmintakenama`,`st1`.`nama` AS `dcstatusnama`,`st2`.`nama` AS `dcstatussebelumnyanama`,`u1`.`unama` AS `dcinputusernama`,`u2`.`unama` AS `dcmodifikasiusernama`,`dc`.`dcshift` AS `dcshift`,`dc`.`dcidbarang` AS `dcidbarang`,`dc`.`dcnamabarang` AS `dcnamabarang`,`dc`.`dctipebarang` AS `dctipebarang`,`dc`.`dchmstart` AS `dchmstart`,`dc`.`dchmstop` AS `dchmstop`,`dc`.`dchmtotal` AS `dchmtotal`,`ih`.`bkode` AS `dckodebarang`,`dcd`.`iddcdetail` AS `iddcdetail`,`dcd`.`iddc` AS `iddc`,`dcd`.`opstart` AS `opstart`,`dcd`.`opend` AS `opend`,`dcd`.`sbstart` AS `sbstart`,`dcd`.`sbend` AS `sbend`,`dcd`.`spstart` AS `spstart`,`dcd`.`spend` AS `spend`,`dcd`.`rfstart` AS `rfstart`,`dcd`.`rfend` AS `rfend`,`dcd`.`bdstart` AS `bdstart`,`dcd`.`bdend` AS `bdend`,`dcd`.`cabang` AS `cabang`,`dcd`.`lokasi` AS `lokasi`,`dcd`.`gudangasal` AS `gudangasal`,`dcd`.`gudangtujuan` AS `gudangtujuan`,`dcd`.`costcenter` AS `costcenter`,`dcd`.`divisi` AS `divisi`,`dcd`.`subdivisi` AS `subdivisi`,`dcd`.`proyek` AS `proyek`,`dcd`.`catatan` AS `catatan`,`dcd`.`urutan` AS `urutan`,`dcd`.`jmlrealisasi` AS `jmlrealisasi`,`dcd`.`statusrealisasi` AS `statusrealisasi`,`dcd`.`isclose` AS `isclose`,`dcd`.`customtext1` AS `customtext1`,`dcd`.`customtext2` AS `customtext2`,`dcd`.`customtext3` AS `customtext3`,`dcd`.`customdbl1` AS `customdbl1`,`dcd`.`customdbl2` AS `customdbl2`,`dcd`.`customdbl3` AS `customdbl3`,`dcd`.`customdate1` AS `customdate1`,`dcd`.`customdate2` AS `customdate2`,`dcd`.`customdate3` AS `customdate3`,`brd`.`bnama` AS `cabangnama`,`lcd`.`lnama` AS `lokasinama`,`whd1`.`wnama` AS `gudangasalnama`,`whd2`.`wnama` AS `gudangtujuannama`,`cc`.`ccnama` AS `costcenternama`,`d`.`dnama` AS `divisinama`,`sd`.`sdnama` AS `subdivisinama`,`p`.`pnama` AS `proyeknama` from ((((((((((((((((((((`m3_dc` `dc` join `m3_dc_detail` `dcd` on((`dc`.`dcid` = `dcd`.`iddc`))) left join `m1_branch` `br` on((`br`.`bkode` = `dc`.`dccabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `dc`.`dclokasi`))) left join `m1_warehouse` `wh1` on((`wh1`.`wkode` = `dc`.`dcgudangasal`))) left join `m1_warehouse` `wh2` on((`wh2`.`wkode` = `dc`.`dcgudangtujuan`))) left join `m1_contact` `c1` on((`c1`.`kid` = `dc`.`dcdimintaoleh`))) left join `m1_contact` `c2` on((`c2`.`kid` = `dc`.`dcmintake`))) left join `m1_item_hauling` `ih` on((`dc`.`dcidbarang` = `ih`.`bid`))) left join `m0_status` `st1` on((`st1`.`kode` = `dc`.`dcstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `dc`.`dcstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `dc`.`dcinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `dc`.`dcmodifikasiuser`))) left join `m1_branch` `brd` on((`dcd`.`cabang` = `brd`.`bkode`))) left join `m1_location` `lcd` on((`dcd`.`lokasi` = `lcd`.`lkode`))) left join `m1_warehouse` `whd1` on((`dcd`.`gudangasal` = `whd1`.`wkode`))) left join `m1_warehouse` `whd2` on((`dcd`.`gudangtujuan` = `whd2`.`wkode`))) left join `m1_cost_center` `cc` on((`dcd`.`costcenter` = `cc`.`cckode`))) left join `m1_division` `d` on((`dcd`.`divisi` = `d`.`dkode`))) left join `m1_subdivision` `sd` on((`dcd`.`subdivisi` = `sd`.`sdkode`))) left join `m1_project` `p` on((`dcd`.`proyek` = `p`.`pkode`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("dcid"), ""), sptField,
                     FxDB(drutama("dccabang"), ""), sptField,
                     FxDB(drutama("dclokasi"), ""), sptField,
                     FxDB(drutama("dcgudangasal"), ""), sptField,
                     FxDB(drutama("dcgudangtujuan"), ""), sptField,
                     FxDB(drutama("dcsumber"), ""), sptField,
                     FxDB(drutama("dcautonotransaksi"), 0), sptField,
                     FxDB(drutama("dcnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("dctgl"), ""), formatTgl), sptField,
                     FxDB(drutama("dckodepa"), ""), sptField,
                     FxDB(drutama("dcdimintaoleh"), ""), sptField,
                     FxDB(drutama("dcdimintaolehkontak"), ""), sptField,
                     FxDB(drutama("dcmintake"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("dctgldipakai"), ""), formatTgl), sptField,
                     FxDB(drutama("dcuraian"), ""), sptField,
                     FxDB(drutama("dccatatan"), ""), sptField,
                     FxDB(drutama("dcnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("dctglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("dcstatusts"), 0), sptField,
                     FxDB(drutama("dcstatusrs"), 0), sptField,
                     FxDB(drutama("dcstatusrealisasi"), 0), sptField,
                     FxDB(drutama("dcstatus"), 0), sptField,
                     FxDB(drutama("dcstatussebelumnya"), 0), sptField,
                     FxDB(drutama("dcjmlrevisi"), 0), sptField,
                     FxDB(drutama("dccetakanke"), 0), sptField,
                     FxDB(drutama("dcinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("dcinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("dcmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("dcmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("dcposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("dcpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("dcisclose"), 0), sptField,
                     FxDB(drutama("dccustomtext1"), ""), sptField,
                     FxDB(drutama("dccustomtext2"), ""), sptField,
                     FxDB(drutama("dccustomtext3"), ""), sptField,
                     FxDB(drutama("dccustomtext4"), ""), sptField,
                     FxDB(drutama("dccustomtext5"), ""), sptField,
                     FxDB(drutama("dccustomint1"), 0), sptField,
                     FxDB(drutama("dccustomint2"), 0), sptField,
                     FxDB(drutama("dccustomint3"), 0), sptField,
                     FxDB(drutama("dccustomdbl1"), 0), sptField,
                     FxDB(drutama("dccustomdbl2"), 0), sptField,
                     FxDB(drutama("dccustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("dccustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("dccustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("dccustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("dccabangnama"), ""), sptField,
                     FxDB(drutama("dclokasinama"), ""), sptField,
                     FxDB(drutama("dcgudangasalnama"), ""), sptField,
                     FxDB(drutama("dcgudangtujuannama"), ""), sptField,
                     FxDB(drutama("dcdimintaolehkode"), ""), sptField,
                     FxDB(drutama("dcdimintaolehnama"), ""), sptField,
                     FxDB(drutama("dcmintakekode"), ""), sptField,
                     FxDB(drutama("dcmintakenama"), ""), sptField,
                     FxDB(drutama("dcstatusnama"), ""), sptField,
                     FxDB(drutama("dcstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("dcinputusernama"), ""), sptField,
                     FxDB(drutama("dcmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("dcshift"), 0), sptField,
                     FxDB(drutama("dcidbarang"), ""), sptField,
                     FxDB(drutama("dcnamabarang"), ""), sptField,
                     FxDB(drutama("dctipebarang"), ""), sptField,
                     FxDB(drutama("dchmstart"), 0), sptField,
                     FxDB(drutama("dchmstop"), 0), sptField,
                     FxDB(drutama("dchmtotal"), 0), sptField,
                     FxDB(drutama("dckodebarang"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("iddcdetail"), ""), sptField,
                     FxDB(dr("iddc"), ""), sptField,
                     IIf(Len(FxDB(dr("opstart").ToString, "")) > 0, Replace(FxDB(dr("opstart").ToString, ""), ":", ".").Substring(0, 5), ""), sptField,
                     IIf(Len(FxDB(dr("opend").ToString, "")) > 0, Replace(FxDB(dr("opend").ToString, ""), ":", ".").Substring(0, 5), ""), sptField,
                     IIf(Len(FxDB(dr("sbstart").ToString, "")) > 0, Replace(FxDB(dr("sbstart").ToString, ""), ":", ".").Substring(0, 5), ""), sptField,
                     IIf(Len(FxDB(dr("sbend").ToString, "")) > 0, Replace(FxDB(dr("sbend").ToString, ""), ":", ".").Substring(0, 5), ""), sptField,
                     IIf(Len(FxDB(dr("spstart").ToString, "")) > 0, Replace(FxDB(dr("spstart").ToString, ""), ":", ".").Substring(0, 5), ""), sptField,
                     IIf(Len(FxDB(dr("spend").ToString, "")) > 0, Replace(FxDB(dr("spend").ToString, ""), ":", ".").Substring(0, 5), ""), sptField,
                     IIf(Len(FxDB(dr("rfstart").ToString, "")) > 0, Replace(FxDB(dr("rfstart").ToString, ""), ":", ".").Substring(0, 5), ""), sptField,
                     IIf(Len(FxDB(dr("rfend").ToString, "")) > 0, Replace(FxDB(dr("rfend").ToString, ""), ":", ".").Substring(0, 5), ""), sptField,
                     IIf(Len(FxDB(dr("bdstart").ToString, "")) > 0, Replace(FxDB(dr("bdstart").ToString, ""), ":", ".").Substring(0, 5), ""), sptField,
                     IIf(Len(FxDB(dr("bdend").ToString, "")) > 0, Replace(FxDB(dr("bdend").ToString, ""), ":", ".").Substring(0, 5), ""), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudangasal"), ""), sptField,
                     FxDB(dr("gudangtujuan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("jmlrealisasi"), 0), sptField,
                     FxDB(dr("statusrealisasi"), 0), sptField,
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
                     FxDB(dr("cabangnama"), ""), sptField,
                     FxDB(dr("lokasinama"), ""), sptField,
                     FxDB(dr("gudangasalnama"), ""), sptField,
                     FxDB(dr("gudangtujuannama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA OUT
            'Dim querygiro As New m0_query
            'sql = querygiro.PanggilQuery("m6_pd_getdata_out")
            sql = "select `dcc`.`iddccheck` AS `iddccheck`,`dcc`.`iddc` AS `iddc`,`dcc`.`idkategoricheck` AS `idkategoricheck`,`dcc`.`catatan` AS `catatan`,`dcc`.`status` AS `status`,`dcc`.`urutan` AS `urutan`,`dcc`.`isclose` AS `isclose`,`dcc`.`customtext1` AS `customtext1`,`dcc`.`customtext2` AS `customtext2`,`dcc`.`customtext3` AS `customtext3`,`dcc`.`customdbl1` AS `customdbl1`,`dcc`.`customdbl2` AS `customdbl2`,`dcc`.`customdbl3` AS `customdbl3`,`dcc`.`customdate1` AS `customdate1`,`dcc`.`customdate2` AS `customdate2`,`dcc`.`customdate3` AS `customdate3`,`chc`.`ccnama` AS `ccnama` from (`m3_dc_check` `dcc` left join `m1_checking_category` `chc` on((`dcc`.`idkategoricheck` = `chc`.`ccid`)))"

            Dim dtout As New DataTable
            dtout = AmbilData("aplikasi1-M6_Pd_Pack", Filter2, "urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases

            For Each dr As DataRow In dtout.Rows
                detailCheck = String.Concat(detailCheck,
                     FxDB(dr("iddccheck"), 0), sptField,
                     FxDB(dr("iddc"), 0), sptField,
                     FxDB(dr("idkategoricheck"), 0), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("status"), 0), sptField,
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
                     FxDB(dr("ccnama"), ""), sptRow)
            Next
            detailCheck = detailCheck.Substring(0, detailCheck.Length - sptRow.Length)

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, detailCheck)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("dcid, dccabang, dclokasi, dcgudangasal, dcgudangtujuan, dcsumber, dcautonotransaksi, dcnotransaksi, dctgl, dckodepa, dcdimintaoleh, dcdimintaolehkontak, dcmintake, dctgldipakai, dcuraian, dccatatan, dcnoref, dctglnoref, dcstatusts, dcstatusrs, dcstatusrealisasi, dcstatus, dcstatussebelumnya, dcjmlrevisi, dccetakanke, dcinputuser, dcinputtgl, dcmodifikasiuser, dcmodifikasitgl, dcposting, dcpostingtgl, dcisclose, dccustomtext1, dccustomtext2, dccustomtext3, dccustomtext4, dccustomtext5, dccustomint1, dccustomint2, dccustomint3, dccustomdbl1, dccustomdbl2, dccustomdbl3, dccustomdate1, dccustomdate2, dccustomdate3, dccabangnama, dclokasinama, dcgudangasalnama, dcgudangtujuannama, dcdimintaolehkode, dcdimintaolehnama, dcmintakekode, dcmintakenama, dcstatusnama, dcstatussebelumnyanama, dcinputusernama, dcmodifikasiusernama, dcshift, dcidbarang, dcnamabarang, dctipebarang, dchmstart, dchmstop, dchmtotal, dckodebarang" & sptSubParam & "iddcdetail, iddc, opstart, opend, sbstart, sbend, spstart, spend, rfstart, rfend, bdstart, bdend, cabang, lokasi, gudangasal, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, cabangnama, lokasinama, gudangasalnama, gudangtujuannama, costcenternama, divisinama, subdivisinama, proyeknama" & sptSubParam & "iddccheck, iddc, idkategoricheck, catatan, status, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, ccnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_DcSearch(ByVal param As String) As String
        'M3_DcSearch --------------------------------------------------------
        'dcid, dccabang, dclokasi, dcgudangasal, dcgudangtujuan, dcsumber, dcautonotransaksi, 
        'dcnotransaksi, dctgl, dckodepa, dcdimintaoleh, dcdimintaolehkontak, dcmintake, dctgldipakai, 
        'dcuraian, dccatatan, dcnoref, dctglnoref, dcstatusts, dcstatusrs, dcstatusrealisasi, 
        'dcstatus, dcstatussebelumnya, dcjmlrevisi, dccetakanke, dcinputuser, dcinputtgl, dcmodifikasiuser, 
        'dcmodifikasitgl, dcposting, dcpostingtgl, dcisclose, dccabangnama, dclokasinama, dcgudangasalnama, 
        'dcgudangtujuannama, dcdimintaolehkode, dcdimintaolehnama, dcmintakekode, dcmintakenama, dcstatusnama, dcstatussebelumnyanama, 
        'dcinputusernama, dcmodifikasiusernama, dcshift, dcidbarang, dcnamabarang, dctipebarang, dchmstart, 
        'dchmstop, dchmtotal, dckodebarang

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
            Filter = Filter.Replace("dcdimintaolehkode", "c1.kkode")
            Filter = Filter.Replace("dcdimintaolehnama", "c1.knama")
            Filter = Filter.Replace("dcmintakekode", "c2.kkode")
            Filter = Filter.Replace("dcmintakenama", "c2.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m3_dc_v")
        sql = "select `dc`.`dcid` AS `dcid`,`dc`.`dccabang` AS `dccabang`,`dc`.`dclokasi` AS `dclokasi`,`dc`.`dcgudangasal` AS `dcgudangasal`,`dc`.`dcgudangtujuan` AS `dcgudangtujuan`,`dc`.`dcsumber` AS `dcsumber`,`dc`.`dcautonotransaksi` AS `dcautonotransaksi`,`dc`.`dcnotransaksi` AS `dcnotransaksi`,`dc`.`dctgl` AS `dctgl`,`dc`.`dckodepa` AS `dckodepa`,`dc`.`dcdimintaoleh` AS `dcdimintaoleh`,`dc`.`dcdimintaolehkontak` AS `dcdimintaolehkontak`,`dc`.`dcmintake` AS `dcmintake`,`dc`.`dctgldipakai` AS `dctgldipakai`,`dc`.`dcuraian` AS `dcuraian`,`dc`.`dccatatan` AS `dccatatan`,`dc`.`dcnoref` AS `dcnoref`,`dc`.`dctglnoref` AS `dctglnoref`,`dc`.`dcstatusts` AS `dcstatusts`,`dc`.`dcstatusrs` AS `dcstatusrs`,`dc`.`dcstatusrealisasi` AS `dcstatusrealisasi`,`dc`.`dcstatus` AS `dcstatus`,`dc`.`dcstatussebelumnya` AS `dcstatussebelumnya`,`dc`.`dcjmlrevisi` AS `dcjmlrevisi`,`dc`.`dccetakanke` AS `dccetakanke`,`dc`.`dcinputuser` AS `dcinputuser`,`dc`.`dcinputtgl` AS `dcinputtgl`,`dc`.`dcmodifikasiuser` AS `dcmodifikasiuser`,`dc`.`dcmodifikasitgl` AS `dcmodifikasitgl`,`dc`.`dcposting` AS `dcposting`,`dc`.`dcpostingtgl` AS `dcpostingtgl`,`dc`.`dcisclose` AS `dcisclose`,`br`.`bnama` AS `dccabangnama`,`lc`.`lnama` AS `dclokasinama`,`wh1`.`wnama` AS `dcgudangasalnama`,`wh2`.`wnama` AS `dcgudangtujuannama`,`c1`.`kkode` AS `dcdimintaolehkode`,`c1`.`knama` AS `dcdimintaolehnama`,`c2`.`kkode` AS `dcmintakekode`,`c2`.`knama` AS `dcmintakenama`,`st1`.`nama` AS `dcstatusnama`,`st2`.`nama` AS `dcstatussebelumnyanama`,`u1`.`unama` AS `dcinputusernama`,`u2`.`unama` AS `dcmodifikasiusernama`,`dc`.`dcshift` AS `dcshift`,`dc`.`dcidbarang` AS `dcidbarang`,`dc`.`dcnamabarang` AS `dcnamabarang`,`dc`.`dctipebarang` AS `dctipebarang`,`dc`.`dchmstart` AS `dchmstart`,`dc`.`dchmstop` AS `dchmstop`,`dc`.`dchmtotal` AS `dchmtotal`,`ih`.`bkode` AS `dckodebarang` from (((((((((((`m3_dc` `dc` left join `m1_item_hauling` `ih` on((`dc`.`dcidbarang` = `ih`.`bid`))) left join `m1_branch` `br` on((`br`.`bkode` = `dc`.`dccabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `dc`.`dclokasi`))) left join `m1_warehouse` `wh1` on((`wh1`.`wkode` = `dc`.`dcgudangasal`))) left join `m1_warehouse` `wh2` on((`wh2`.`wkode` = `dc`.`dcgudangtujuan`))) left join `m1_contact` `c1` on((`c1`.`kid` = `dc`.`dcdimintaoleh`))) left join `m1_contact` `c2` on((`c2`.`kid` = `dc`.`dcmintake`))) left join `m0_status` `st1` on((`st1`.`kode` = `dc`.`dcstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `dc`.`dcstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `dc`.`dcinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `dc`.`dcmodifikasiuser`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M3_Dc", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("dcid"), ""), sptField,
                     FxDB(dr("dccabang"), ""), sptField,
                     FxDB(dr("dclokasi"), ""), sptField,
                     FxDB(dr("dcgudangasal"), ""), sptField,
                     FxDB(dr("dcgudangtujuan"), ""), sptField,
                     FxDB(dr("dcsumber"), ""), sptField,
                     FxDB(dr("dcautonotransaksi"), 0), sptField,
                     FxDB(dr("dcnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dctgl"), ""), formatTgl), sptField,
                     FxDB(dr("dckodepa"), ""), sptField,
                     FxDB(dr("dcdimintaoleh"), ""), sptField,
                     FxDB(dr("dcdimintaolehkontak"), ""), sptField,
                     FxDB(dr("dcmintake"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dctgldipakai"), ""), formatTgl), sptField,
                     FxDB(dr("dcuraian"), ""), sptField,
                     FxDB(dr("dccatatan"), ""), sptField,
                     FxDB(dr("dcnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dctglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("dcstatusts"), 0), sptField,
                     FxDB(dr("dcstatusrs"), 0), sptField,
                     FxDB(dr("dcstatusrealisasi"), 0), sptField,
                     FxDB(dr("dcstatus"), 0), sptField,
                     FxDB(dr("dcstatussebelumnya"), 0), sptField,
                     FxDB(dr("dcjmlrevisi"), 0), sptField,
                     FxDB(dr("dccetakanke"), 0), sptField,
                     FxDB(dr("dcinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dcinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("dcmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dcmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("dcposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("dcpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("dcisclose"), 0), sptField,
                     FxDB(dr("dccabangnama"), ""), sptField,
                     FxDB(dr("dclokasinama"), ""), sptField,
                     FxDB(dr("dcgudangasalnama"), ""), sptField,
                     FxDB(dr("dcgudangtujuannama"), ""), sptField,
                     FxDB(dr("dcdimintaolehkode"), ""), sptField,
                     FxDB(dr("dcdimintaolehnama"), ""), sptField,
                     FxDB(dr("dcmintakekode"), ""), sptField,
                     FxDB(dr("dcmintakenama"), ""), sptField,
                     FxDB(dr("dcstatusnama"), ""), sptField,
                     FxDB(dr("dcstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("dcinputusernama"), ""), sptField,
                     FxDB(dr("dcmodifikasiusernama"), ""), sptField,
                     FxDB(dr("dcshift"), 0), sptField,
                     FxDB(dr("dcidbarang"), ""), sptField,
                     FxDB(dr("dcnamabarang"), ""), sptField,
                     FxDB(dr("dctipebarang"), ""), sptField,
                     FxDB(dr("dchmstart"), 0), sptField,
                     FxDB(dr("dchmstop"), 0), sptField,
                     FxDB(dr("dchmtotal"), 0), sptField,
                     FxDB(dr("dckodebarang"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("dcid, dccabang, dclokasi, dcgudangasal, dcgudangtujuan, dcsumber, dcautonotransaksi, dcnotransaksi, dctgl, dckodepa, dcdimintaoleh, dcdimintaolehkontak, dcmintake, dctgldipakai, dcuraian, dccatatan, dcnoref, dctglnoref, dcstatusts, dcstatusrs, dcstatusrealisasi, dcstatus, dcstatussebelumnya, dcjmlrevisi, dccetakanke, dcinputuser, dcinputtgl, dcmodifikasiuser, dcmodifikasitgl, dcposting, dcpostingtgl, dcisclose, dccabangnama, dclokasinama, dcgudangasalnama, dcgudangtujuannama, dcdimintaolehkode, dcdimintaolehnama, dcmintakekode, dcmintakenama, dcstatusnama, dcstatussebelumnyanama, dcinputusernama, dcmodifikasiusernama, dcshift, dcidbarang, dcnamabarang, dctipebarang, dchmstart, dchmstop, dchmtotal, dckodebarang"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_DcTerkait(ByVal param As String) As String
        'M3_DcTerkait --------------------------------------------------------
        'dcid, dcnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
        'modifikasitglterkait, jenisterkait

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isdcev(2), countPage(3), countRow(4)

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


        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        Dim idtransaksi As String = ""
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "dcid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================


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
            Filter = pagingSplit(2) & " AND dcid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "dcid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.m3_dc_terkait(Filter)

        dt = AmbilData("aplikasi1-M3_Dc_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("dcid"), 0), sptField,
                     FxDB(dr("dcnotransaksi"), ""), sptField,
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
            result(2) = "Related DC data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("dcid, dcnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

End Class