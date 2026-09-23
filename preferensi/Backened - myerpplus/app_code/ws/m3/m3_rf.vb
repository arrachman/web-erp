Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m3_rf
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M3_RfSimpan(ByVal param As String) As String
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
        'rfid(0) As Integer, rfcabang(1) As String, rflokasi(2) As String, rfgudangasal(3) As String, rfgudangtujuan(4) As String, 
        'rfsumber(5) As String, rfautonotransaksi(6) As Integer, rfnotransaksi(7) As String, rftgl(8) As Date, rfkodepa(9) As Integer, 
        'rfdimintaoleh(10) As Integer, rfdimintaolehkontak(11) As String, rfmintake(12) As Integer, rftgldipakai(13) As Date, rfuraian(14) As String, 
        'rfcatatan(15) As String, rfnoref(16) As String, rftglnoref(17) As Date, rfstatusts(18) As Integer, rfstatusrs(19) As Integer, 
        'rfstatus(20) As Integer, rfstatussebelumnya(21) As Integer, rfjmlrevisi(22) As Integer, rfcetakanke(23) As Integer, rfinputuser(24) As Integer, 
        'rfinputtgl(25) As DateTime, rfmodifikasiuser(26) As Integer, rfmodifikasitgl(27) As DateTime, rfisclose(28) As Integer, rfcustomtext1(29) As String, 
        'rfcustomtext2(30) As String, rfcustomtext3(31) As String, rfcustomtext4(32) As String, rfcustomtext5(33) As String, rfcustomint1(34) As Integer, 
        'rfcustomint2(35) As Integer, rfcustomint3(36) As Integer, rfcustomdbl1(37) As Double, rfcustomdbl2(38) As Double, rfcustomdbl3(39) As Double, 
        'rfcustomdate1(40) As Date, rfcustomdate2(41) As Date, rfcustomdate3(42) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'rfid, rfcabang, rflokasi, rfgudangasal, rfgudangtujuan, rfsumber, rfautonotransaksi, 
        'rfnotransaksi, rftgl, rfkodepa, rfdimintaoleh, rfdimintaolehkontak, rfmintake, rftgldipakai, 
        'rfuraian, rfcatatan, rfnoref, rftglnoref, rfstatusts, rfstatusrs, rfstatus, 
        'rfstatussebelumnya, rfjmlrevisi, rfcetakanke, rfinputuser, rfinputtgl, rfmodifikasiuser, rfmodifikasitgl, 
        'rfisclose, rfcustomtext1, rfcustomtext2, rfcustomtext3, rfcustomtext4, rfcustomtext5, rfcustomint1, 
        'rfcustomint2, rfcustomint3, rfcustomdbl1, rfcustomdbl2, rfcustomdbl3, rfcustomdate1, rfcustomdate2, 
        'rfcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 43) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'rfid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "rfid required numeric." : GoTo selesai
        End If
        'rfautonotransaksi(6) As Integer
        If (IsNumeric(dataUtama(6)) = False) Then
            result(2) = "rfautonotransaksi required numeric." : GoTo selesai
        End If
        'rftgl(8) As Date
        If (IsDate(dataUtama(8)) = False) Then
            result(2) = "rftgl required date." : GoTo selesai
        End If
        'rfkodepa(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "rfkodepa required numeric." : GoTo selesai
        End If
        'rfdimintaoleh(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "rfdimintaoleh required numeric." : GoTo selesai
        End If
        If (dataUtama(10) < 1) Then
            result(2) = "rfdimintaoleh can't be empty." : GoTo selesai
        End If
        'rfmintake(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "rfmintake required numeric." : GoTo selesai
        End If
        'rftgldipakai(13) As Date
        If (IsDate(dataUtama(13)) = False) Then
            result(2) = "rftgldipakai required date." : GoTo selesai
        End If
        'rftglnoref(17) As Date
        If (IsDate(dataUtama(17)) = False) Then
            result(2) = "rftglnoref required date." : GoTo selesai
        End If
        'rfstatusts(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "rfstatusts required numeric." : GoTo selesai
        End If
        'rfstatusrs(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "rfstatusrs required numeric." : GoTo selesai
        End If
        'rfstatus(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "rfstatus required numeric." : GoTo selesai
        End If
        'rfstatussebelumnya(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "rfstatussebelumnya required numeric." : GoTo selesai
        End If
        'rfjmlrevisi(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "rfjmlrevisi required numeric." : GoTo selesai
        End If
        'rfcetakanke(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "rfcetakanke required numeric." : GoTo selesai
        End If
        'rfinputuser(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "rfinputuser required numeric." : GoTo selesai
        End If
        'rfinputtgl(25) As DateTime
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "rfinputtgl required date." : GoTo selesai
        End If
        'rfmodifikasiuser(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "rfmodifikasiuser required numeric." : GoTo selesai
        End If
        'rfmodifikasitgl(27) As DateTime
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "rfmodifikasitgl required date." : GoTo selesai
        End If
        'rfisclose(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "rfisclose required numeric." : GoTo selesai
        End If
        'rfcustomint1(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "rfcustomint1 required numeric." : GoTo selesai
        End If
        'rfcustomint2(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "rfcustomint2 required numeric." : GoTo selesai
        End If
        'rfcustomint3(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "rfcustomint3 required numeric." : GoTo selesai
        End If
        'rfcustomdbl1(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "rfcustomdbl1 required numeric." : GoTo selesai
        End If
        'rfcustomdbl2(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "rfcustomdbl2 required numeric." : GoTo selesai
        End If
        'rfcustomdbl3(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "rfcustomdbl3 required numeric." : GoTo selesai
        End If
        'rfcustomdate1(40) As Date
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "rfcustomdate1 required date." : GoTo selesai
        End If
        'rfcustomdate2(41) As Date
        If (IsDate(dataUtama(41)) = False) Then
            result(2) = "rfcustomdate2 required date." : GoTo selesai
        End If
        'rfcustomdate3(42) As Date
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "rfcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'rfcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "rfcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "rfcabang should not be more than 25 character." : GoTo selesai
        End If

        'rflokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "rflokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "rflokasi should not be more than 25 character." : GoTo selesai
        End If

        'rfgudangasal(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "rfgudangasal can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "rfgudangasal should not be more than 25 character." : GoTo selesai
        End If

        'rfgudangtujuan(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "rfgudangtujuan can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 25 Then
            result(2) = "rfgudangtujuan should not be more than 25 character." : GoTo selesai
        End If

        'rfsumber(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "rfsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 10 Then
            result(2) = "rfsumber should not be more than 10 character." : GoTo selesai
        End If

        'rfnotransaksi(7) As String
        If Len(dataUtama(7)) = 0 Then
            result(2) = "rfnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 50 Then
            result(2) = "rfnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'rftgl(8) As Date
        If Len(dataUtama(8)) = 0 Then
            result(2) = "rftgl can't be empty" : GoTo selesai
        End If

        'rftgldipakai(13) As Date
        If Len(dataUtama(13)) = 0 Then
            result(2) = "rftgldipakai can't be empty" : GoTo selesai
        End If

        'rftglnoref(17) As Date
        If Len(dataUtama(17)) = 0 Then
            result(2) = "rftglnoref can't be empty" : GoTo selesai
        End If

        'rfinputtgl(25) As DateTime
        If Len(dataUtama(25)) = 0 Then
            result(2) = "rfinputtgl can't be empty" : GoTo selesai
        End If

        'rfmodifikasitgl(27) As DateTime
        If Len(dataUtama(27)) = 0 Then
            result(2) = "rfmodifikasitgl can't be empty" : GoTo selesai
        End If

        'rfcustomdbl1(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "rfcustomdbl1 can't be empty" : GoTo selesai
        End If

        'rfcustomdbl2(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "rfcustomdbl2 can't be empty" : GoTo selesai
        End If

        'rfcustomdbl3(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "rfcustomdbl3 can't be empty" : GoTo selesai
        End If

        'rfcustomdate1(40) As Date
        If Len(dataUtama(40)) = 0 Then
            result(2) = "rfcustomdate1 can't be empty" : GoTo selesai
        End If

        'rfcustomdate2(41) As Date
        If Len(dataUtama(41)) = 0 Then
            result(2) = "rfcustomdate2 can't be empty" : GoTo selesai
        End If

        'rfcustomdate3(42) As Date
        If Len(dataUtama(42)) = 0 Then
            result(2) = "rfcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "rfid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rflokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfgudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfgudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rftgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfdimintaoleh", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfdimintaolehkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfmintake", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rftgldipakai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rftglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfstatusts", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfstatusrs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rfcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rfcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "rfid~rfcabang~rflokasi~rfgudangasal~rfgudangtujuan~rfsumber~rfautonotransaksi~rfnotransaksi~rftgl~rfkodepa~rfdimintaoleh~rfdimintaolehkontak~rfmintake~rftgldipakai~rfuraian~rfcatatan~rfnoref~rftglnoref~rfstatusts~rfstatusrs~rfstatus~rfstatussebelumnya~rfjmlrevisi~rfcetakanke~rfinputuser~rfinputtgl~rfmodifikasiuser~rfmodifikasitgl~rfisclose~rfcustomtext1~rfcustomtext2~rfcustomtext3~rfcustomtext4~rfcustomtext5~rfcustomint1~rfcustomint2~rfcustomint3~rfcustomdbl1~rfcustomdbl2~rfcustomdbl3~rfcustomdate1~rfcustomdate2~rfcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idrfdetail(0) As Integer, idrf(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, hargabeli(12) As Double, hargajual(13) As Double, stokterakhir(14) As Double, 
        'cabang(15) As String, lokasi(16) As String, gudangasal(17) As String, gudangtujuan(18) As String, costcenter(19) As String, 
        'divisi(20) As String, subdivisi(21) As String, proyek(22) As String, catatan(23) As String, urutan(24) As Integer, 
        'jmlts(25) As Double, statusts(26) As Integer, jmlrs(27) As Double, statusrs(28) As Integer, isclose(29) As Integer, 
        'customtext1(30) As String, customtext2(31) As String, customtext3(32) As String, customdbl1(33) As Double, customdbl2(34) As Double, 
        'customdbl3(35) As Double, customdate1(36) As Date, customdate2(37) As Date, customdate3(38) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idrfdetail, idrf, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargabeli, hargajual, 
        'stokterakhir, cabang, lokasi, gudangasal, gudangtujuan, costcenter, divisi, 
        'subdivisi, proyek, catatan, urutan, jmlts, statusts, jmlrs, 
        'statusrs, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idrfdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idrf", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargabeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "stokterakhir", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlts", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusts", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlrs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusrs", AsEnumTypeData.AsInt64)
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
            If (dataRowDetail.Length <> 39) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idrfdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "idrfdetail required numeric." : GoTo selesai
            End If
            'idrf(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idrf required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'jml(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'nilaisatuan(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarang(8) As Double
            'jmlbarang = jml * nilaisatuan
            dataRowDetail(8) = Double.Parse(dataRowDetail(5)) * Double.Parse(dataRowDetail(7))
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - jmlbarang required numeric." : GoTo selesai
            End If
            'kurs(11) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'hargabeli(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - hargabeli required numeric." : GoTo selesai
            End If
            'hargajual(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - hargajual required numeric." : GoTo selesai
            End If
            'stokterakhir(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - stokterakhir required numeric." : GoTo selesai
            End If
            'urutan(24) As Integer
            If (IsNumeric(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'jmlts(25) As Double
            If (IsNumeric(dataRowDetail(25)) = False) Then
                result(2) = "Row : " & i & " - jmlts required numeric." : GoTo selesai
            End If
            'statusts(26) As Integer
            If (IsNumeric(dataRowDetail(26)) = False) Then
                result(2) = "Row : " & i & " - statusts required numeric." : GoTo selesai
            End If
            'jmlrs(27) As Double
            If (IsNumeric(dataRowDetail(27)) = False) Then
                result(2) = "Row : " & i & " - jmlrs required numeric." : GoTo selesai
            End If
            'statusrs(28) As Integer
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - statusrs required numeric." : GoTo selesai
            End If
            'isclose(29) As Integer
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(33) As Double
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(34) As Double
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(35) As Double
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(36) As Date
            If (IsDate(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(37) As Date
            If (IsDate(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(38) As Date
            If (IsDate(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'namabarang(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - namabarang can't be empty" : GoTo selesai
            End If
            'If Len(dataRowDetail(3)) > 100 Then
            '    result(2) = "Row : " & i & " - namabarang should not be more than 100 character." : GoTo selesai
            'End If

            'jml(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - jml can't be empty" : GoTo selesai
            End If
            If dataRowDetail(5) <= 0 Then
                result(2) = "Row : " & i & " - jml can't be less than or equal to zero" : GoTo selesai
            End If

            'satuan(6) As String
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(6)) > 25 Then
                result(2) = "Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'nilaisatuan(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmlbarang(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarang can't be empty" : GoTo selesai
            End If
            If dataRowDetail(8) <= 0 Then
                result(2) = "Row : " & i & " - jmlbarang can't be less than or equal to zero" : GoTo selesai
            End If

            'satuanbarang(9) As String
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(9)) > 25 Then
                result(2) = "Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(11) As Double
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'hargabeli(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - hargabeli can't be empty" : GoTo selesai
            End If

            'hargajual(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - hargajual can't be empty" : GoTo selesai
            End If

            'stokterakhir(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - stokterakhir can't be empty" : GoTo selesai
            End If

            'jmlts(25) As Double
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - jmlts can't be empty" : GoTo selesai
            End If

            'jmlrs(27) As Double
            If Len(dataRowDetail(27)) = 0 Then
                result(2) = "Row : " & i & " - jmlrs can't be empty" : GoTo selesai
            End If

            'customdbl1(33) As Double
            If Len(dataRowDetail(33)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(34) As Double
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(35) As Double
            If Len(dataRowDetail(35)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(36) As Date
            If Len(dataRowDetail(36)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(37) As Date
            If Len(dataRowDetail(37)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(38) As Date
            If Len(dataRowDetail(38)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idrfdetail~idrf~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~hargabeli~hargajual~stokterakhir~cabang~lokasi~gudangasal~gudangtujuan~costcenter~divisi~subdivisi~proyek~catatan~urutan~jmlts~statusts~jmlrs~statusrs~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

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

                'CEK HAK AKSES STATUS ============================
                Dim vAkses As Integer = 0, msgAkses As String = ""
                'MODUL DAN MENU HARUS DISESUAIKAN
                Dim vModuleId As Integer = 3, vMenuId As Integer = 40
                Select Case drutama("rfstatus")
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("rftgl")), AsFormatTanggal(drutama("rftgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================

                If isUpdate Then
                    result(4) = drutama("rfid")
                    notransaksi = drutama("rfnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(rfid), rfnotransaksi FROM M3_Rf WHERE rfid='" & result(4) & "' AND rfstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("rfautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rfcabang"), drutama("rflokasi"), drutama("rfsumber"), drutama("rftgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(rfid) FROM m3_rf WHERE rfnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m3_rf_history
                        Dim rsSimpanHistory As String = SimpanHistory.M3_Rf_HistorySimpan("" & paramSplit(0) & "★M3_Rf_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("rfsumber")) & "▼" & FixQuotes(drutama("rfid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M3_Rf set rfcabang  = '" & FixQuotes(drutama("rfcabang")) & "', rflokasi  = '" & FixQuotes(drutama("rflokasi")) & "', rfgudangasal  = '" & FixQuotes(drutama("rfgudangasal")) & "', rfgudangtujuan  = '" & FixQuotes(drutama("rfgudangtujuan")) & "', rfsumber  = '" & FixQuotes(drutama("rfsumber")) & "', rfautonotransaksi  = " & drutama("rfautonotransaksi") & ", rfnotransaksi  = '" & notransaksi & "', rftgl  = '" & FixQuotes(AsFormatTanggal(drutama("rftgl"))) & "', rfkodepa  = " & drutama("rfkodepa") & ", rfdimintaoleh  = " & drutama("rfdimintaoleh") & ", rfdimintaolehkontak  = '" & FixQuotes(drutama("rfdimintaolehkontak")) & "', rfmintake  = " & drutama("rfmintake") & ", rftgldipakai  = '" & FixQuotes(AsFormatTanggal(drutama("rftgldipakai"))) & "', rfuraian  = '" & FixQuotes(drutama("rfuraian")) & "', rfcatatan  = '" & FixQuotes(drutama("rfcatatan")) & "', rfnoref  = '" & FixQuotes(drutama("rfnoref")) & "', rftglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("rftglnoref"))) & "', rfstatusts  = " & drutama("rfstatusts") & ", rfstatusrs  = " & drutama("rfstatusrs") & ", rfstatus  = " & drutama("rfstatus") & ", rfstatussebelumnya  = " & drutama("rfstatussebelumnya") & ", rfjmlrevisi  = rfjmlrevisi+1, rfcetakanke  = " & drutama("rfcetakanke") & ", rfmodifikasiuser  = " & drutama("rfmodifikasiuser") & ", rfmodifikasitgl  = NOW(), rfcustomtext1  = '" & FixQuotes(drutama("rfcustomtext1")) & "', rfcustomtext2  = '" & FixQuotes(drutama("rfcustomtext2")) & "', rfcustomtext3  = '" & FixQuotes(drutama("rfcustomtext3")) & "', rfcustomtext4  = '" & FixQuotes(drutama("rfcustomtext4")) & "', rfcustomtext5  = '" & FixQuotes(drutama("rfcustomtext5")) & "', rfcustomint1  = " & drutama("rfcustomint1") & ", rfcustomint2  = " & drutama("rfcustomint2") & ", rfcustomint3  = " & drutama("rfcustomint3") & ", rfcustomdbl1  = '" & FixDouble(drutama("rfcustomdbl1")) & "', rfcustomdbl2  = '" & FixDouble(drutama("rfcustomdbl2")) & "', rfcustomdbl3  = '" & FixDouble(drutama("rfcustomdbl3")) & "', rfcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("rfcustomdate1"))) & "', rfcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("rfcustomdate2"))) & "', rfcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("rfcustomdate3"))) & "' where rfid = '" & drutama("rfid") & "'"
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

                    If drutama("rfautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rfcabang"), drutama("rflokasi"), drutama("rfsumber"), drutama("rftgl"))
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
                        notransaksi = drutama("rfnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(rfid) FROM m3_rf WHERE rfnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M3_Rf (rfcabang, rflokasi, rfgudangasal, rfgudangtujuan, rfsumber, rfautonotransaksi, rfnotransaksi, rftgl, rfkodepa, rfdimintaoleh, rfdimintaolehkontak, rfmintake, rftgldipakai, rfuraian, rfcatatan, rfnoref, rftglnoref, rfstatusts, rfstatusrs, rfstatus, rfstatussebelumnya, rfjmlrevisi, rfcetakanke, rfinputuser, rfinputtgl, rfmodifikasiuser, rfmodifikasitgl, rfisclose, rfcustomtext1, rfcustomtext2, rfcustomtext3, rfcustomtext4, rfcustomtext5, rfcustomint1, rfcustomint2, rfcustomint3, rfcustomdbl1, rfcustomdbl2, rfcustomdbl3, rfcustomdate1, rfcustomdate2, rfcustomdate3) values('" & FixQuotes(drutama("rfcabang")) & "', '" & FixQuotes(drutama("rflokasi")) & "', '" & FixQuotes(drutama("rfgudangasal")) & "', '" & FixQuotes(drutama("rfgudangtujuan")) & "', '" & FixQuotes(drutama("rfsumber")) & "', " & drutama("rfautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("rftgl"))) & "', " & drutama("rfkodepa") & ", " & drutama("rfdimintaoleh") & ", '" & FixQuotes(drutama("rfdimintaolehkontak")) & "', " & drutama("rfmintake") & ", '" & FixQuotes(AsFormatTanggal(drutama("rftgldipakai"))) & "', '" & FixQuotes(drutama("rfuraian")) & "', '" & FixQuotes(drutama("rfcatatan")) & "', '" & FixQuotes(drutama("rfnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rftglnoref"))) & "', " & drutama("rfstatusts") & ", " & drutama("rfstatusrs") & ", " & drutama("rfstatus") & ", " & drutama("rfstatussebelumnya") & ", " & drutama("rfjmlrevisi") & ", " & drutama("rfcetakanke") & ", " & drutama("rfinputuser") & ", NOW(), " & drutama("rfmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("rfisclose") & ", '" & FixQuotes(drutama("rfcustomtext1")) & "', '" & FixQuotes(drutama("rfcustomtext2")) & "', '" & FixQuotes(drutama("rfcustomtext3")) & "', '" & FixQuotes(drutama("rfcustomtext4")) & "', '" & FixQuotes(drutama("rfcustomtext5")) & "', " & drutama("rfcustomint1") & ", " & drutama("rfcustomint2") & ", " & drutama("rfcustomint3") & ", '" & FixDouble(drutama("rfcustomdbl1")) & "', '" & FixDouble(drutama("rfcustomdbl2")) & "', '" & FixDouble(drutama("rfcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rfcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rfcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rfcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select rfid from M3_Rf where rfnotransaksi='" & notransaksi & "' AND Rfinputuser= '" & userid & "' order by Rfmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M3_Rf_Detail where idrf = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idrfdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("hargabeli")) & "', '" & FixDouble(dr1("hargajual")) & "', '" & FixDouble(dr1("stokterakhir")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", '" & FixDouble(dr1("jmlts")) & "', " & dr1("statusts") & ", '" & FixDouble(dr1("jmlrs")) & "', " & dr1("statusrs") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M3_Rf_Detail(idrfdetail, idrf, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargabeli, hargajual, stokterakhir, cabang, lokasi, gudangasal, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlts, statusts, jmlrs, statusrs, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "RF", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M3_RfUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("rfdimintaolehkode", "c1.kkode")
            Filter = Filter.Replace("rfdimintaolehnama", "c1.knama")
            Filter = Filter.Replace("rfmintakekode", "c2.kkode")
            Filter = Filter.Replace("rfmintakenama", "c2.knama")
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
            Dim sumber As String = "Rf", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Rftgl, Rfnotransaksi, Rfstatus FROM m3_Rf WHERE Rfid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Rfstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m3_rf_history
            Dim rsSimpanHistory As String = SimpanHistory.M3_Rf_HistorySimpan("" & paramSplit(0) & "★M3_Rf_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                'sql = query.m3_rf_terkait("rfid = '" & idtransaksi & "'")
                'Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                'dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                'If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                ''END OF CEK TERKAIT =============================================================

            End If

            'update status utama
            sql = "UPDATE M3_Rf SET Rfstatus = " & nilaiStatus & ", rfmodifikasiuser='" & userid & "', rfmodifikasitgl = NOW(), rfposting = 0, rfpostingtgl = '1971-01-01 00:00:00', rfjmlrevisi = rfjmlrevisi + 1 WHERE rfid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M3_RfSearch(PostWsSearch(paramSplit(0), "M3_RfSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M3_RfDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("rfdimintaolehkode", "c1.kkode")
            Filter = Filter.Replace("rfdimintaolehnama", "c1.knama")
            Filter = Filter.Replace("rfmintakekode", "c2.kkode")
            Filter = Filter.Replace("rfmintakenama", "c2.knama")
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
            Dim sumber As String = "Rf", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Rfid, Rfnotransaksi FROM m3_Rf WHERE Rfid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT rfcabang, rflokasi, rfsumber, rfautonotransaksi, rfnotransaksi, rftgl"
            sql &= " FROM M3_rf"
            sql &= " WHERE rfid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("rfcabang")
                lokasi = dtNomorNext.Rows(0)("rflokasi")
                sumber = dtNomorNext.Rows(0)("rfsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("rfautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("rfnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("rftgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M3_Rf_Detail WHERE idrf = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M3_Rf WHERE rfid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M3_RfSearch(PostWsSearch(paramSplit(0), "M3_RfSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M3_RfGetdataById(ByVal param As String) As String

        'M3_RfGetdataById Utama --------------------------------------------------------
        'rfid, rfcabang, rflokasi, rfgudangasal, rfgudangtujuan, rfsumber, rfautonotransaksi, 
        'rfnotransaksi, rftgl, rfkodepa, rfdimintaoleh, rfdimintaolehkontak, rfmintake, rftgldipakai, 
        'rfuraian, rfcatatan, rfnoref, rftglnoref, rfstatusts, rfstatusrs, rfstatusrealisasi, 
        'rfstatus, rfstatussebelumnya, rfjmlrevisi, rfcetakanke, rfinputuser, rfinputtgl, rfmodifikasiuser, 
        'rfmodifikasitgl, rfposting, rfpostingtgl, rfisclose, rfcustomtext1, rfcustomtext2, rfcustomtext3, 
        'rfcustomtext4, rfcustomtext5, rfcustomint1, rfcustomint2, rfcustomint3, rfcustomdbl1, rfcustomdbl2, 
        'rfcustomdbl3, rfcustomdate1, rfcustomdate2, rfcustomdate3, rfcabangnama, rflokasinama, rfgudangasalnama, 
        'rfgudangtujuannama, rfdimintaolehkode, rfdimintaolehnama, rfmintakekode, rfmintakenama, rfstatusnama, rfstatussebelumnyanama, 
        'rfinputusernama, rfmodifikasiusernama

        'M3_RfGetdataById Detail -------------------------------------------------------
        'idrfdetail, idrf, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargabeli, hargajual, 
        'stokterakhir, cabang, lokasi, gudangasal, gudangtujuan, costcenter, divisi, 
        'subdivisi, proyek, catatan, urutan, jmlts, statusts, jmlrs, 
        'statusrs, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, 
        'cabangnama, lokasinama, gudangasalnama, gudangtujuannama, costcenternama, divisinama, subdivisinama, 
        'proyeknama

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

        Dim NmMemcached As String = "aplikasi1-M3_Rf~M3_Rf_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "rfid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "rfid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m3_rf_getdata")
        sql = "select `rf`.`rfid` AS `rfid`,`rf`.`rfcabang` AS `rfcabang`,`rf`.`rflokasi` AS `rflokasi`,`rf`.`rfgudangasal` AS `rfgudangasal`,`rf`.`rfgudangtujuan` AS `rfgudangtujuan`,`rf`.`rfsumber` AS `rfsumber`,`rf`.`rfautonotransaksi` AS `rfautonotransaksi`,`rf`.`rfnotransaksi` AS `rfnotransaksi`,`rf`.`rftgl` AS `rftgl`,`rf`.`rfkodepa` AS `rfkodepa`,`rf`.`rfdimintaoleh` AS `rfdimintaoleh`,`rf`.`rfdimintaolehkontak` AS `rfdimintaolehkontak`,`rf`.`rfmintake` AS `rfmintake`,`rf`.`rftgldipakai` AS `rftgldipakai`,`rf`.`rfuraian` AS `rfuraian`,`rf`.`rfcatatan` AS `rfcatatan`,`rf`.`rfnoref` AS `rfnoref`,`rf`.`rftglnoref` AS `rftglnoref`,`rf`.`rfstatusts` AS `rfstatusts`,`rf`.`rfstatusrs` AS `rfstatusrs`,`rf`.`rfstatusrealisasi` AS `rfstatusrealisasi`,`rf`.`rfstatus` AS `rfstatus`,`rf`.`rfstatussebelumnya` AS `rfstatussebelumnya`,`rf`.`rfjmlrevisi` AS `rfjmlrevisi`,`rf`.`rfcetakanke` AS `rfcetakanke`,`rf`.`rfinputuser` AS `rfinputuser`,`rf`.`rfinputtgl` AS `rfinputtgl`,`rf`.`rfmodifikasiuser` AS `rfmodifikasiuser`,`rf`.`rfmodifikasitgl` AS `rfmodifikasitgl`,`rf`.`rfposting` AS `rfposting`,`rf`.`rfpostingtgl` AS `rfpostingtgl`,`rf`.`rfisclose` AS `rfisclose`,`rf`.`rfcustomtext1` AS `rfcustomtext1`,`rf`.`rfcustomtext2` AS `rfcustomtext2`,`rf`.`rfcustomtext3` AS `rfcustomtext3`,`rf`.`rfcustomtext4` AS `rfcustomtext4`,`rf`.`rfcustomtext5` AS `rfcustomtext5`,`rf`.`rfcustomint1` AS `rfcustomint1`,`rf`.`rfcustomint2` AS `rfcustomint2`,`rf`.`rfcustomint3` AS `rfcustomint3`,`rf`.`rfcustomdbl1` AS `rfcustomdbl1`,`rf`.`rfcustomdbl2` AS `rfcustomdbl2`,`rf`.`rfcustomdbl3` AS `rfcustomdbl3`,`rf`.`rfcustomdate1` AS `rfcustomdate1`,`rf`.`rfcustomdate2` AS `rfcustomdate2`,`rf`.`rfcustomdate3` AS `rfcustomdate3`,`br`.`bnama` AS `rfcabangnama`,`lc`.`lnama` AS `rflokasinama`,`wh1`.`wnama` AS `rfgudangasalnama`,`wh2`.`wnama` AS `rfgudangtujuannama`,`c1`.`kkode` AS `rfdimintaolehkode`,`c1`.`knama` AS `rfdimintaolehnama`,`c2`.`kkode` AS `rfmintakekode`,`c2`.`knama` AS `rfmintakenama`,`st1`.`nama` AS `rfstatusnama`,`st2`.`nama` AS `rfstatussebelumnyanama`,`u1`.`unama` AS `rfinputusernama`,`u2`.`unama` AS `rfmodifikasiusernama`,`rfd`.`idrfdetail` AS `idrfdetail`,`rfd`.`idrf` AS `idrf`,`rfd`.`idbarang` AS `idbarang`,`rfd`.`namabarang` AS `namabarang`,`rfd`.`tipebarang` AS `tipebarang`,`rfd`.`jml` AS `jml`,`rfd`.`satuan` AS `satuan`,`rfd`.`nilaisatuan` AS `nilaisatuan`,`rfd`.`jmlbarang` AS `jmlbarang`,`rfd`.`satuanbarang` AS `satuanbarang`,`rfd`.`matauang` AS `matauang`,`rfd`.`kurs` AS `kurs`,`rfd`.`hargabeli` AS `hargabeli`,`rfd`.`hargajual` AS `hargajual`,`rfd`.`stokterakhir` AS `stokterakhir`,`rfd`.`cabang` AS `cabang`,`rfd`.`lokasi` AS `lokasi`,`rfd`.`gudangasal` AS `gudangasal`,`rfd`.`gudangtujuan` AS `gudangtujuan`,`rfd`.`costcenter` AS `costcenter`,`rfd`.`divisi` AS `divisi`,`rfd`.`subdivisi` AS `subdivisi`,`rfd`.`proyek` AS `proyek`,`rfd`.`catatan` AS `catatan`,`rfd`.`urutan` AS `urutan`,`rfd`.`jmlts` AS `jmlts`,`rfd`.`statusts` AS `statusts`,`rfd`.`jmlrs` AS `jmlrs`,`rfd`.`statusrs` AS `statusrs`,`rfd`.`jmlrealisasi` AS `jmlrealisasi`,`rfd`.`statusrealisasi` AS `statusrealisasi`,`rfd`.`isclose` AS `isclose`,`rfd`.`customtext1` AS `customtext1`,`rfd`.`customtext2` AS `customtext2`,`rfd`.`customtext3` AS `customtext3`,`rfd`.`customdbl1` AS `customdbl1`,`rfd`.`customdbl2` AS `customdbl2`,`rfd`.`customdbl3` AS `customdbl3`,`rfd`.`customdate1` AS `customdate1`,`rfd`.`customdate2` AS `customdate2`,`rfd`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`brd`.`bnama` AS `cabangnama`,`lcd`.`lnama` AS `lokasinama`,`whd1`.`wnama` AS `gudangasalnama`,`whd2`.`wnama` AS `gudangtujuannama`,`cc`.`ccnama` AS `costcenternama`,`d`.`dnama` AS `divisinama`,`sd`.`sdnama` AS `subdivisinama`,`p`.`pnama` AS `proyeknama` from ((((((((((((((((((((`m3_rf` `rf` join `m3_rf_detail` `rfd` on((`rf`.`rfid` = `rfd`.`idrf`))) left join `m1_branch` `br` on((`br`.`bkode` = `rf`.`rfcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `rf`.`rflokasi`))) left join `m1_warehouse` `wh1` on((`wh1`.`wkode` = `rf`.`rfgudangasal`))) left join `m1_warehouse` `wh2` on((`wh2`.`wkode` = `rf`.`rfgudangtujuan`))) left join `m1_contact` `c1` on((`c1`.`kid` = `rf`.`rfdimintaoleh`))) left join `m1_contact` `c2` on((`c2`.`kid` = `rf`.`rfmintake`))) left join `m0_status` `st1` on((`st1`.`kode` = `rf`.`rfstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `rf`.`rfstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `rf`.`rfinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `rf`.`rfmodifikasiuser`))) left join `m1_item_hauling` `i` on((`i`.`bid` = `rfd`.`idbarang`))) left join `m1_branch` `brd` on((`rfd`.`cabang` = `brd`.`bkode`))) left join `m1_location` `lcd` on((`rfd`.`lokasi` = `lcd`.`lkode`))) left join `m1_warehouse` `whd1` on((`rfd`.`gudangasal` = `whd1`.`wkode`))) left join `m1_warehouse` `whd2` on((`rfd`.`gudangtujuan` = `whd2`.`wkode`))) left join `m1_cost_center` `cc` on((`rfd`.`costcenter` = `cc`.`cckode`))) left join `m1_division` `d` on((`rfd`.`divisi` = `d`.`dkode`))) left join `m1_subdivision` `sd` on((`rfd`.`subdivisi` = `sd`.`sdkode`))) left join `m1_project` `p` on((`rfd`.`proyek` = `p`.`pkode`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("rfid"), 0), sptField,
                     FxDB(drutama("rfcabang"), ""), sptField,
                     FxDB(drutama("rflokasi"), ""), sptField,
                     FxDB(drutama("rfgudangasal"), ""), sptField,
                     FxDB(drutama("rfgudangtujuan"), ""), sptField,
                     FxDB(drutama("rfsumber"), ""), sptField,
                     FxDB(drutama("rfautonotransaksi"), 0), sptField,
                     FxDB(drutama("rfnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rftgl"), ""), formatTgl), sptField,
                     FxDB(drutama("rfkodepa"), 0), sptField,
                     FxDB(drutama("rfdimintaoleh"), 0), sptField,
                     FxDB(drutama("rfdimintaolehkontak"), ""), sptField,
                     FxDB(drutama("rfmintake"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rftgldipakai"), ""), formatTgl), sptField,
                     FxDB(drutama("rfuraian"), ""), sptField,
                     FxDB(drutama("rfcatatan"), ""), sptField,
                     FxDB(drutama("rfnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rftglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("rfstatusts"), 0), sptField,
                     FxDB(drutama("rfstatusrs"), 0), sptField,
                     FxDB(drutama("rfstatusrealisasi"), 0), sptField,
                     FxDB(drutama("rfstatus"), 0), sptField,
                     FxDB(drutama("rfstatussebelumnya"), 0), sptField,
                     FxDB(drutama("rfjmlrevisi"), 0), sptField,
                     FxDB(drutama("rfcetakanke"), 0), sptField,
                     FxDB(drutama("rfinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rfinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rfmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rfmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rfposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rfpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rfisclose"), 0), sptField,
                     FxDB(drutama("rfcustomtext1"), ""), sptField,
                     FxDB(drutama("rfcustomtext2"), ""), sptField,
                     FxDB(drutama("rfcustomtext3"), ""), sptField,
                     FxDB(drutama("rfcustomtext4"), ""), sptField,
                     FxDB(drutama("rfcustomtext5"), ""), sptField,
                     FxDB(drutama("rfcustomint1"), 0), sptField,
                     FxDB(drutama("rfcustomint2"), 0), sptField,
                     FxDB(drutama("rfcustomint3"), 0), sptField,
                     FxDB(drutama("rfcustomdbl1"), 0), sptField,
                     FxDB(drutama("rfcustomdbl2"), 0), sptField,
                     FxDB(drutama("rfcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rfcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rfcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rfcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("rfcabangnama"), ""), sptField,
                     FxDB(drutama("rflokasinama"), ""), sptField,
                     FxDB(drutama("rfgudangasalnama"), ""), sptField,
                     FxDB(drutama("rfgudangtujuannama"), ""), sptField,
                     FxDB(drutama("rfdimintaolehkode"), ""), sptField,
                     FxDB(drutama("rfdimintaolehnama"), ""), sptField,
                     FxDB(drutama("rfmintakekode"), ""), sptField,
                     FxDB(drutama("rfmintakenama"), ""), sptField,
                     FxDB(drutama("rfstatusnama"), ""), sptField,
                     FxDB(drutama("rfstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("rfinputusernama"), ""), sptField,
                     FxDB(drutama("rfmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idrfdetail"), 0), sptField,
                     FxDB(dr("idrf"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("hargabeli"), 0), sptField,
                     FxDB(dr("hargajual"), 0), sptField,
                     FxDB(dr("stokterakhir"), 0), sptField,
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
                     FxDB(dr("jmlts"), 0), sptField,
                     FxDB(dr("statusts"), 0), sptField,
                     FxDB(dr("jmlrs"), 0), sptField,
                     FxDB(dr("statusrs"), 0), sptField,
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
                     FxDB(dr("kodebarang"), ""), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rfid, rfcabang, rflokasi, rfgudangasal, rfgudangtujuan, rfsumber, rfautonotransaksi, rfnotransaksi, rftgl, rfkodepa, rfdimintaoleh, rfdimintaolehkontak, rfmintake, rftgldipakai, rfuraian, rfcatatan, rfnoref, rftglnoref, rfstatusts, rfstatusrs, rfstatusrealisasi, rfstatus, rfstatussebelumnya, rfjmlrevisi, rfcetakanke, rfinputuser, rfinputtgl, rfmodifikasiuser, rfmodifikasitgl, rfposting, rfpostingtgl, rfisclose, rfcustomtext1, rfcustomtext2, rfcustomtext3, rfcustomtext4, rfcustomtext5, rfcustomint1, rfcustomint2, rfcustomint3, rfcustomdbl1, rfcustomdbl2, rfcustomdbl3, rfcustomdate1, rfcustomdate2, rfcustomdate3, rfcabangnama, rflokasinama, rfgudangasalnama, rfgudangtujuannama, rfdimintaolehkode, rfdimintaolehnama, rfmintakekode, rfmintakenama, rfstatusnama, rfstatussebelumnyanama, rfinputusernama, rfmodifikasiusernama" & sptSubParam & "idrfdetail, idrf, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargabeli, hargajual, stokterakhir, cabang, lokasi, gudangasal, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlts, statusts, jmlrs, statusrs, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, cabangnama, lokasinama, gudangasalnama, gudangtujuannama, costcenternama, divisinama, subdivisinama, proyeknama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_RfSearch(ByVal param As String) As String
        'M3_RfSearch --------------------------------------------------------
        'rfid, rfcabang, rflokasi, rfgudangasal, rfgudangtujuan, rfsumber, rfautonotransaksi, 
        'rfnotransaksi, rftgl, rfkodepa, rfdimintaoleh, rfdimintaolehkontak, rfmintake, rftgldipakai, 
        'rfuraian, rfcatatan, rfnoref, rftglnoref, rfstatusts, rfstatusrs, rfstatusrealisasi, 
        'rfstatus, rfstatussebelumnya, rfjmlrevisi, rfcetakanke, rfinputuser, rfinputtgl, rfmodifikasiuser, 
        'rfmodifikasitgl, rfposting, rfpostingtgl, rfisclose, rfcabangnama, rflokasinama, rfgudangasalnama, 
        'rfgudangtujuannama, rfdimintaolehkode, rfdimintaolehnama, rfmintakekode, rfmintakenama, rfstatusnama, rfstatussebelumnyanama, 
        'rfinputusernama, rfmodifikasiusernama

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
            Filter = Filter.Replace("rfdimintaolehkode", "c1.kkode")
            Filter = Filter.Replace("rfdimintaolehnama", "c1.knama")
            Filter = Filter.Replace("rfmintakekode", "c2.kkode")
            Filter = Filter.Replace("rfmintakenama", "c2.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m3_rf_v")
        sql = "select `rf`.`rfid` AS `rfid`,`rf`.`rfcabang` AS `rfcabang`,`rf`.`rflokasi` AS `rflokasi`,`rf`.`rfgudangasal` AS `rfgudangasal`,`rf`.`rfgudangtujuan` AS `rfgudangtujuan`,`rf`.`rfsumber` AS `rfsumber`,`rf`.`rfautonotransaksi` AS `rfautonotransaksi`,`rf`.`rfnotransaksi` AS `rfnotransaksi`,`rf`.`rftgl` AS `rftgl`,`rf`.`rfkodepa` AS `rfkodepa`,`rf`.`rfdimintaoleh` AS `rfdimintaoleh`,`rf`.`rfdimintaolehkontak` AS `rfdimintaolehkontak`,`rf`.`rfmintake` AS `rfmintake`,`rf`.`rftgldipakai` AS `rftgldipakai`,`rf`.`rfuraian` AS `rfuraian`,`rf`.`rfcatatan` AS `rfcatatan`,`rf`.`rfnoref` AS `rfnoref`,`rf`.`rftglnoref` AS `rftglnoref`,`rf`.`rfstatusts` AS `rfstatusts`,`rf`.`rfstatusrs` AS `rfstatusrs`,`rf`.`rfstatusrealisasi` AS `rfstatusrealisasi`,`rf`.`rfstatus` AS `rfstatus`,`rf`.`rfstatussebelumnya` AS `rfstatussebelumnya`,`rf`.`rfjmlrevisi` AS `rfjmlrevisi`,`rf`.`rfcetakanke` AS `rfcetakanke`,`rf`.`rfinputuser` AS `rfinputuser`,`rf`.`rfinputtgl` AS `rfinputtgl`,`rf`.`rfmodifikasiuser` AS `rfmodifikasiuser`,`rf`.`rfmodifikasitgl` AS `rfmodifikasitgl`,`rf`.`rfposting` AS `rfposting`,`rf`.`rfpostingtgl` AS `rfpostingtgl`,`rf`.`rfisclose` AS `rfisclose`,`br`.`bnama` AS `rfcabangnama`,`lc`.`lnama` AS `rflokasinama`,`wh1`.`wnama` AS `rfgudangasalnama`,`wh2`.`wnama` AS `rfgudangtujuannama`,`c1`.`kkode` AS `rfdimintaolehkode`,`c1`.`knama` AS `rfdimintaolehnama`,`c2`.`kkode` AS `rfmintakekode`,`c2`.`knama` AS `rfmintakenama`,`st1`.`nama` AS `rfstatusnama`,`st2`.`nama` AS `rfstatussebelumnyanama`,`u1`.`unama` AS `rfinputusernama`,`u2`.`unama` AS `rfmodifikasiusernama` from ((((((((((`m3_rf` `rf` left join `m1_branch` `br` on((`br`.`bkode` = `rf`.`rfcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `rf`.`rflokasi`))) left join `m1_warehouse` `wh1` on((`wh1`.`wkode` = `rf`.`rfgudangasal`))) left join `m1_warehouse` `wh2` on((`wh2`.`wkode` = `rf`.`rfgudangtujuan`))) left join `m1_contact` `c1` on((`c1`.`kid` = `rf`.`rfdimintaoleh`))) left join `m1_contact` `c2` on((`c2`.`kid` = `rf`.`rfmintake`))) left join `m0_status` `st1` on((`st1`.`kode` = `rf`.`rfstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `rf`.`rfstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `rf`.`rfinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `rf`.`rfmodifikasiuser`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M3_Rf", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rfid"), 0), sptField,
                     FxDB(dr("rfcabang"), ""), sptField,
                     FxDB(dr("rflokasi"), ""), sptField,
                     FxDB(dr("rfgudangasal"), ""), sptField,
                     FxDB(dr("rfgudangtujuan"), ""), sptField,
                     FxDB(dr("rfsumber"), ""), sptField,
                     FxDB(dr("rfautonotransaksi"), 0), sptField,
                     FxDB(dr("rfnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rftgl"), ""), formatTgl), sptField,
                     FxDB(dr("rfkodepa"), 0), sptField,
                     FxDB(dr("rfdimintaoleh"), 0), sptField,
                     FxDB(dr("rfdimintaolehkontak"), ""), sptField,
                     FxDB(dr("rfmintake"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rftgldipakai"), ""), formatTgl), sptField,
                     FxDB(dr("rfuraian"), ""), sptField,
                     FxDB(dr("rfcatatan"), ""), sptField,
                     FxDB(dr("rfnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rftglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("rfstatusts"), 0), sptField,
                     FxDB(dr("rfstatusrs"), 0), sptField,
                     FxDB(dr("rfstatusrealisasi"), 0), sptField,
                     FxDB(dr("rfstatus"), 0), sptField,
                     FxDB(dr("rfstatussebelumnya"), 0), sptField,
                     FxDB(dr("rfjmlrevisi"), 0), sptField,
                     FxDB(dr("rfcetakanke"), 0), sptField,
                     FxDB(dr("rfinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rfinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rfmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rfmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rfposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rfpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rfisclose"), 0), sptField,
                     FxDB(dr("rfcabangnama"), ""), sptField,
                     FxDB(dr("rflokasinama"), ""), sptField,
                     FxDB(dr("rfgudangasalnama"), ""), sptField,
                     FxDB(dr("rfgudangtujuannama"), ""), sptField,
                     FxDB(dr("rfdimintaolehkode"), ""), sptField,
                     FxDB(dr("rfdimintaolehnama"), ""), sptField,
                     FxDB(dr("rfmintakekode"), ""), sptField,
                     FxDB(dr("rfmintakenama"), ""), sptField,
                     FxDB(dr("rfstatusnama"), ""), sptField,
                     FxDB(dr("rfstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("rfinputusernama"), ""), sptField,
                     FxDB(dr("rfmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rfid, rfcabang, rflokasi, rfgudangasal, rfgudangtujuan, rfsumber, rfautonotransaksi, rfnotransaksi, rftgl, rfkodepa, rfdimintaoleh, rfdimintaolehkontak, rfmintake, rftgldipakai, rfuraian, rfcatatan, rfnoref, rftglnoref, rfstatusts, rfstatusrs, rfstatusrealisasi, rfstatus, rfstatussebelumnya, rfjmlrevisi, rfcetakanke, rfinputuser, rfinputtgl, rfmodifikasiuser, rfmodifikasitgl, rfposting, rfpostingtgl, rfisclose, rfcabangnama, rflokasinama, rfgudangasalnama, rfgudangtujuannama, rfdimintaolehkode, rfdimintaolehnama, rfmintakekode, rfmintakenama, rfstatusnama, rfstatussebelumnyanama, rfinputusernama, rfmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_Rf_Detail_VSearch(ByVal param As String) As String
        'M3_Rf_Detail_VSearch --------------------------------------------------------
        'idrfdetail, idrf, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargabeli, hargajual, 
        'stokterakhir, cabang, lokasi, gudangasal, gudangtujuan, costcenter, divisi, 
        'subdivisi, proyek, catatan, urutan, jmlts, statusts, jmlrs, 
        'statusrs, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, rfnotransaksi, 
        'kodebarang, bhpp, bhppaverage, bjenis, bserial, bbatch, jmlsisats, 
        'jmlsisars, jmlsisarealisasi

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
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m3_rf_detail_v")
        sql = "select `rfd`.`idrfdetail` AS `idrfdetail`,`rfd`.`idrf` AS `idrf`,`rfd`.`idbarang` AS `idbarang`,`rfd`.`namabarang` AS `namabarang`,`rfd`.`tipebarang` AS `tipebarang`,`rfd`.`jml` AS `jml`,`rfd`.`satuan` AS `satuan`,`rfd`.`nilaisatuan` AS `nilaisatuan`,`rfd`.`jmlbarang` AS `jmlbarang`,`rfd`.`satuanbarang` AS `satuanbarang`,`rfd`.`matauang` AS `matauang`,`rfd`.`kurs` AS `kurs`,`rfd`.`hargabeli` AS `hargabeli`,`rfd`.`hargajual` AS `hargajual`,`rfd`.`stokterakhir` AS `stokterakhir`,`rfd`.`cabang` AS `cabang`,`rfd`.`lokasi` AS `lokasi`,`rfd`.`gudangasal` AS `gudangasal`,`rfd`.`gudangtujuan` AS `gudangtujuan`,`rfd`.`costcenter` AS `costcenter`,`rfd`.`divisi` AS `divisi`,`rfd`.`subdivisi` AS `subdivisi`,`rfd`.`proyek` AS `proyek`,`rfd`.`catatan` AS `catatan`,`rfd`.`urutan` AS `urutan`,`rfd`.`jmlts` AS `jmlts`,`rfd`.`statusts` AS `statusts`,`rfd`.`jmlrs` AS `jmlrs`,`rfd`.`statusrs` AS `statusrs`,`rfd`.`jmlrealisasi` AS `jmlrealisasi`,`rfd`.`statusrealisasi` AS `statusrealisasi`,`rfd`.`isclose` AS `isclose`,`rfd`.`customtext1` AS `customtext1`,`rfd`.`customtext2` AS `customtext2`,`rfd`.`customtext3` AS `customtext3`,`rfd`.`customdbl1` AS `customdbl1`,`rfd`.`customdbl2` AS `customdbl2`,`rfd`.`customdbl3` AS `customdbl3`,`rfd`.`customdate1` AS `customdate1`,`rfd`.`customdate2` AS `customdate2`,`rfd`.`customdate3` AS `customdate3`,`rf`.`rfnotransaksi` AS `rfnotransaksi`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bhppaverage` AS `bhppaverage`,`i`.`bjenis` AS `bjenis`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,((`rfd`.`jmlbarang` - `rfd`.`jmlts`) / `rfd`.`nilaisatuan`) AS `jmlsisats`,((`rfd`.`jmlbarang` - `rfd`.`jmlrs`) / `rfd`.`nilaisatuan`) AS `jmlsisars`,((`rfd`.`jmlbarang` - `rfd`.`jmlrealisasi`) / `rfd`.`nilaisatuan`) AS `jmlsisarealisasi` from ((`m3_rf_detail` `rfd` join `m3_rf` `rf` on((`rfd`.`idrf` = `rf`.`rfid`))) join `m1_item_hauling` `i` on((`rfd`.`idbarang` = `i`.`bid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M3_Rf_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idrfdetail"), 0), sptField,
                     FxDB(dr("idrf"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("hargabeli"), 0), sptField,
                     FxDB(dr("hargajual"), 0), sptField,
                     FxDB(dr("stokterakhir"), 0), sptField,
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
                     FxDB(dr("jmlts"), 0), sptField,
                     FxDB(dr("statusts"), 0), sptField,
                     FxDB(dr("jmlrs"), 0), sptField,
                     FxDB(dr("statusrs"), 0), sptField,
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
                     FxDB(dr("rfnotransaksi"), ""), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bhppaverage"), 0), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("jmlsisats"), 0), sptField,
                     FxDB(dr("jmlsisars"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idrfdetail, idrf, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargabeli, hargajual, stokterakhir, cabang, lokasi, gudangasal, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlts, statusts, jmlrs, statusrs, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, rfnotransaksi, kodebarang, bhpp, bhppaverage, bjenis, bserial, bbatch, jmlsisats, jmlsisars, jmlsisarealisasi"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_RfTerkait(ByVal param As String) As String
        'M3_RfTerkait --------------------------------------------------------
        'rfid, rfnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
        'modifikasitglterkait, jenisterkait

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isrfev(2), countPage(3), countRow(4)

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
            result(2) = "rfid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2) & " AND rfid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "rfid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.m3_rf_terkait(Filter)

        dt = AmbilData("aplikasi1-M3_Rf_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rfid"), 0), sptField,
                     FxDB(dr("rfnotransaksi"), ""), sptField,
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
            result(2) = "Related RF data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rfid, rfnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

End Class