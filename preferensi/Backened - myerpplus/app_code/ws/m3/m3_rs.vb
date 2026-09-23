Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m3_rs
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M3_RsSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataBatch(), dataRowBatch(), dataSerial(), dataRowSerial() As String
        Dim dataAsset(), dataRowAsset() As String

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
        If (dataSplit.Length <> 4 And dataSplit.Length <> 5) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'rsid(0) As Integer, rscabang(1) As String, rslokasi(2) As String, rsgudangasal(3) As String, rsgudangtransit(4) As String, 
        'rsgudangtujuan(5) As String, rssumber(6) As String, rsautonotransaksi(7) As Integer, rsnotransaksi(8) As String, rstgl(9) As Date, 
        'rskodepa(10) As Integer, rsbagianterima(11) As Integer, rsbagianterimakontak(12) As String, rsuraian(13) As String, rscatatan(14) As String, 
        'rsnoref(15) As String, rstglnoref(16) As Date, rsidmr(17) As Integer, rsidts(18) As Integer, rsstatus(19) As Integer, 
        'rsstatussebelumnya(20) As Integer, rsjmlrevisi(21) As Integer, rscetakanke(22) As Integer, rsinputuser(23) As Integer, rsinputtgl(24) As DateTime, 
        'rsmodifikasiuser(25) As Integer, rsmodifikasitgl(26) As DateTime, rsisclose(27) As Integer, rscustomtext1(28) As String, rscustomtext2(29) As String, 
        'rscustomtext3(30) As String, rscustomtext4(31) As String, rscustomtext5(32) As String, rscustomint1(33) As Integer, rscustomint2(34) As Integer, 
        'rscustomint3(35) As Integer, rscustomdbl1(36) As Double, rscustomdbl2(37) As Double, rscustomdbl3(38) As Double, rscustomdate1(39) As Date, 
        'rscustomdate2(40) As Date, rscustomdate3(41) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'rsid, rscabang, rslokasi, rsgudangasal, rsgudangtransit, rsgudangtujuan, rssumber, 
        'rsautonotransaksi, rsnotransaksi, rstgl, rskodepa, rsbagianterima, rsbagianterimakontak, rsuraian, 
        'rscatatan, rsnoref, rstglnoref, rsidmr, rsidts, rsstatus, rsstatussebelumnya, 
        'rsjmlrevisi, rscetakanke, rsinputuser, rsinputtgl, rsmodifikasiuser, rsmodifikasitgl, rsisclose, 
        'rscustomtext1, rscustomtext2, rscustomtext3, rscustomtext4, rscustomtext5, rscustomint1, rscustomint2, 
        'rscustomint3, rscustomdbl1, rscustomdbl2, rscustomdbl3, rscustomdate1, rscustomdate2, rscustomdate3


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 42) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'rsid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "rsid required numeric." : GoTo selesai
        End If
        'rsautonotransaksi(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "rsautonotransaksi required numeric." : GoTo selesai
        End If
        'rstgl(9) As Date
        If (IsDate(dataUtama(9)) = False) Then
            result(2) = "rstgl required date." : GoTo selesai
        End If
        'rskodepa(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "rskodepa required numeric." : GoTo selesai
        End If
        'rsbagianterima(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "rsbagianterima required numeric." : GoTo selesai
        End If
        If (dataUtama(11) < 1) Then
            result(2) = "rsbagianterima can't be empty." : GoTo selesai
        End If
        'rstglnoref(16) As Date
        If (IsDate(dataUtama(16)) = False) Then
            result(2) = "rstglnoref required date." : GoTo selesai
        End If
        'rsidmr(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "rsidmr required numeric." : GoTo selesai
        End If
        'rsidts(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "rsidts required numeric." : GoTo selesai
        End If
        'rsstatus(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "rsstatus required numeric." : GoTo selesai
        End If
        'rsstatussebelumnya(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "rsstatussebelumnya required numeric." : GoTo selesai
        End If
        'rsjmlrevisi(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "rsjmlrevisi required numeric." : GoTo selesai
        End If
        'rscetakanke(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "rscetakanke required numeric." : GoTo selesai
        End If
        'rsinputuser(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "rsinputuser required numeric." : GoTo selesai
        End If
        'rsinputtgl(24) As DateTime
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "rsinputtgl required date." : GoTo selesai
        End If
        'rsmodifikasiuser(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "rsmodifikasiuser required numeric." : GoTo selesai
        End If
        'rsmodifikasitgl(26) As DateTime
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "rsmodifikasitgl required date." : GoTo selesai
        End If
        'rsisclose(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "rsisclose required numeric." : GoTo selesai
        End If
        'rscustomint1(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "rscustomint1 required numeric." : GoTo selesai
        End If
        'rscustomint2(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "rscustomint2 required numeric." : GoTo selesai
        End If
        'rscustomint3(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "rscustomint3 required numeric." : GoTo selesai
        End If
        'rscustomdbl1(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "rscustomdbl1 required numeric." : GoTo selesai
        End If
        'rscustomdbl2(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "rscustomdbl2 required numeric." : GoTo selesai
        End If
        'rscustomdbl3(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "rscustomdbl3 required numeric." : GoTo selesai
        End If
        'rscustomdate1(39) As Date
        If (IsDate(dataUtama(39)) = False) Then
            result(2) = "rscustomdate1 required date." : GoTo selesai
        End If
        'rscustomdate2(40) As Date
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "rscustomdate2 required date." : GoTo selesai
        End If
        'rscustomdate3(41) As Date
        If (IsDate(dataUtama(41)) = False) Then
            result(2) = "rscustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'rscabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "rscabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "rscabang should not be more than 25 character." : GoTo selesai
        End If

        'rslokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "rslokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "rslokasi should not be more than 25 character." : GoTo selesai
        End If

        'rssumber(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "rssumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 10 Then
            result(2) = "rssumber should not be more than 10 character." : GoTo selesai
        End If

        'rsnotransaksi(8) As String
        If Len(dataUtama(8)) = 0 Then
            result(2) = "rsnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 50 Then
            result(2) = "rsnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'rstgl(9) As Date
        If Len(dataUtama(9)) = 0 Then
            result(2) = "rstgl can't be empty" : GoTo selesai
        End If

        'rstglnoref(16) As Date
        If Len(dataUtama(16)) = 0 Then
            result(2) = "rstglnoref can't be empty" : GoTo selesai
        End If

        'rsinputtgl(24) As DateTime
        If Len(dataUtama(24)) = 0 Then
            result(2) = "rsinputtgl can't be empty" : GoTo selesai
        End If

        'rsmodifikasitgl(26) As DateTime
        If Len(dataUtama(26)) = 0 Then
            result(2) = "rsmodifikasitgl can't be empty" : GoTo selesai
        End If

        'rscustomdbl1(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "rscustomdbl1 can't be empty" : GoTo selesai
        End If

        'rscustomdbl2(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "rscustomdbl2 can't be empty" : GoTo selesai
        End If

        'rscustomdbl3(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "rscustomdbl3 can't be empty" : GoTo selesai
        End If

        'rscustomdate1(39) As Date
        If Len(dataUtama(39)) = 0 Then
            result(2) = "rscustomdate1 can't be empty" : GoTo selesai
        End If

        'rscustomdate2(40) As Date
        If Len(dataUtama(40)) = 0 Then
            result(2) = "rscustomdate2 can't be empty" : GoTo selesai
        End If

        'rscustomdate3(41) As Date
        If Len(dataUtama(41)) = 0 Then
            result(2) = "rscustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "rsid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rscabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rslokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rsgudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rsgudangtransit", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rsgudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rssumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rsautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rsnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rstgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rskodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rsbagianterima", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rsbagianterimakontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rsuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rscatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rsnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rstglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rsidmr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rsidts", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rsstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rsstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rsjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rscetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rsinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rsinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rsmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rsmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rsisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rscustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rscustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rscustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rscustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rscustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rscustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rscustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rscustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rscustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rscustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rscustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rscustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rscustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rscustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "rsid~rscabang~rslokasi~rsgudangasal~rsgudangtransit~rsgudangtujuan~rssumber~rsautonotransaksi~rsnotransaksi~rstgl~rskodepa~rsbagianterima~rsbagianterimakontak~rsuraian~rscatatan~rsnoref~rstglnoref~rsidmr~rsidts~rsstatus~rsstatussebelumnya~rsjmlrevisi~rscetakanke~rsinputuser~rsinputtgl~rsmodifikasiuser~rsmodifikasitgl~rsisclose~rscustomtext1~rscustomtext2~rscustomtext3~rscustomtext4~rscustomtext5~rscustomint1~rscustomint2~rscustomint3~rscustomdbl1~rscustomdbl2~rscustomdbl3~rscustomdate1~rscustomdate2~rscustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idrsdetail(0) As Integer, idrs(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'cabang(10) As String, lokasi(11) As String, gudangasal(12) As String, gudangtransit(13) As String, gudangtujuan(14) As String, 
        'costcenter(15) As String, divisi(16) As String, subdivisi(17) As String, proyek(18) As String, catatan(19) As String, 
        'urutan(20) As Integer, idmrdetail(21) As Integer, idtsdetail(22) As Integer, isclose(23) As Integer, customtext1(24) As String, 
        'customtext2(25) As String, customtext3(26) As String, customdbl1(27) As Double, customdbl2(28) As Double, customdbl3(29) As Double, 
        'customdate1(30) As Date, customdate2(31) As Date, customdate3(32) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idrsdetail, idrs, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, cabang, lokasi, gudangasal, gudangtransit, 
        'gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idmrdetail, idtsdetail, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idrsdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idrs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangtransit", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idmrdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idtsdetail", AsEnumTypeData.AsInt64)
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

        'Variabel ValidasiBatchSerial
        Dim ftBarang As String = ""

        'Variabel ValidasiSimpan
        Dim ftExistOutstanding As String = "", ftOutstanding As String = "", ftExistStok As String = "", ftStok As String = ""
        Dim updNilai As String = "", updFilter As String = "", updStokIn As String = "", updStokOut As String = "", updNilaiMR As String = "", updFilterMR As String = ""
        Dim gudangIn As String = "", gudangOut As String = ""
        Dim idbarang As Integer = 0, idtsdetail As Integer = 0, idmrdetail As Integer = 0, jmlbarang As Double = 0

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 33) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idrsdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idrsdetail required numeric." : GoTo selesai
            End If
            'idrs(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idrs required numeric." : GoTo selesai
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
            'urutan(20) As Integer
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idmrdetail(21) As Integer
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - idmrdetail required numeric." : GoTo selesai
            End If
            'idtsdetail(22) As Integer
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - idtsdetail required numeric." : GoTo selesai
            End If
            'isclose(23) As Integer
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(27) As Double
            If (IsNumeric(dataRowDetail(27)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(28) As Double
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(29) As Double
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(30) As Date
            If (IsDate(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(31) As Date
            If (IsDate(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(32) As Date
            If (IsDate(dataRowDetail(32)) = False) Then
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

            'cabang(10) As String
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - cabang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(10)) > 25 Then
                result(2) = "Row : " & i & " - cabang should not be more than 25 character." : GoTo selesai
            End If

            'lokasi(11) As String
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - lokasi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(11)) > 25 Then
                result(2) = "Row : " & i & " - lokasi should not be more than 25 character." : GoTo selesai
            End If

            'gudangasal(12) As String
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - gudangasal can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(12)) > 25 Then
                result(2) = "Row : " & i & " - gudangasal should not be more than 25 character." : GoTo selesai
            End If

            'gudangtransit(13) As String
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - gudangtransit can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(13)) > 25 Then
                result(2) = "Row : " & i & " - gudangtransit should not be more than 25 character." : GoTo selesai
            End If

            'gudangtujuan(14) As String
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - gudangtujuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(14)) > 25 Then
                result(2) = "Row : " & i & " - gudangtujuan should not be more than 25 character." : GoTo selesai
            End If

            'customdbl1(27) As Double
            If Len(dataRowDetail(27)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(28) As Double
            If Len(dataRowDetail(28)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(29) As Double
            If Len(dataRowDetail(29)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(30) As Date
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(31) As Date
            If Len(dataRowDetail(31)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(32) As Date
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idrsdetail~idrs~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~cabang~lokasi~gudangasal~gudangtransit~gudangtujuan~costcenter~divisi~subdivisi~proyek~catatan~urutan~idmrdetail~idtsdetail~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , gudangtransit(13) As String   , gudangtujuan(14) As String   , idmrdetail(21) As Integer      , idtsdetail(22) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : gudangOut = dataRowDetail(13) : gudangIn = dataRowDetail(14) : idmrdetail = dataRowDetail(21) : idtsdetail = dataRowDetail(22)

            'ValidasiBatchSerial
            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")

            'VALIDASI OUTSTANDING -------------------------
            If idtsdetail <> 0 Then
                '1. CEK DATA EXIST
                ftExistOutstanding = IIf(Len(ftExistOutstanding.ToString) = 0, "", ftExistOutstanding & " UNION ")
                ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m3_ts_detail JOIN m3_ts ON idts = tsid WHERE idtsdetail = '" & idtsdetail & "' AND (tsstatus = 2 OR tsstatus = 3 OR tsstatus = 4 OR tsstatus = 7) LIMIT 1) as rowExists, '" & idtsdetail & "' as idtsdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
                'ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m3_ts_detail JOIN m3_ts ON idts = tsid WHERE idtsdetail = '" & idtsdetail & "' AND (tsstatus = 2 OR tsstatus = 3) LIMIT 1) as rowExists, '" & idtsdetail & "' as idtsdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idtsdetail=" & idtsdetail)
                ftOutstanding = IIf(Len(ftOutstanding.ToString) = 0, "", ftOutstanding & " OR ")
                ftOutstanding = String.Concat(ftOutstanding, " (tsd.idtsdetail = " & idtsdetail & " AND " & Outstanding & " > (tsd.jmlbarang - tsd.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING
                updNilai = String.Concat("WHEN '" & idtsdetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilai)

                '4. SET FILTER UPDATE OUTSTANDING
                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                updFilter = String.Concat(updFilter, "(idtsdetail = '" & idtsdetail & "')")
            End If

            If idmrdetail <> 0 Then
                '1. SET NILAI UPDATE OUTSTANDING MR
                Dim OutstandingMR As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idmrdetail=" & idmrdetail)
                updNilaiMR = String.Concat("WHEN '" & idmrdetail & "' THEN ROUND(jmlrealisasi + '" & OutstandingMR & "', 5) ", updNilaiMR)

                '2. SET FILTER UPDATE OUTSTANDING MR
                updFilterMR = IIf(Len(updFilterMR.ToString) = 0, "", updFilterMR & " OR ")
                updFilterMR = String.Concat(updFilterMR, "(idmrdetail = '" & idmrdetail & "')")
            End If

            'VALIDASI STOK -------------------------------
            '1. CEK DATA EXIST STOK KELUAR
            ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
            ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

            '2. CEK JML STOK KELUAR
            Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangtransit='" & gudangOut & "'")
            ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
            ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")

            '3. SET NILAI UPDATE STOK KELUAR
            updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
            updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

            '4. SET NILAI UPDATE STOK MASUK
            updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
            updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'MAPPING BUAT WS DATA BATCH -------------------------------------------------------
        'nbtid(0) As Integer, nbtjenismutasi(1) As Integer, nbtidbarang(2) As Integer, nbtkode(3) As String, nbtsumber(4) As String, 
        'nbtidtransaksi(5) As Integer, nbtsatuan(6) As String, nbtjml(7) As Double, nbtcustomtext1(8) As String, nbtcustomtext2(9) As String, 
        'nbtcustomtext3(10) As String, nbtcustomdbl1(11) As Double, nbtcustomdbl2(12) As Double, nbtcustomdbl3(13) As Double, nbtcustomdate1(14) As Date, 
        'nbtcustomdate2(15) As Date, nbtcustomdate3(16) As Date, nbtgudang(17) As String, nbtidbatchin(18) As Integer

        'MAPPING BUAT FLEX DATA BATCH -----------------------------------------------------
        'nbtid, nbtjenismutasi, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, nbtgudang, nbtidbatchin

        'Buat datatable BATCH
        Dim dtbatch As New DataTable
        AsDataTableTambahField(dtbatch, "nbtid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtjenismutasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbatch, "nbtidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbatch, "nbtkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtidtransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbatch, "nbtsatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtjml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtbatch, "nbtcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtidbatchin", AsEnumTypeData.AsInt64)

        'ValidasiSimpan
        Dim ftExistBatch As String = "", ftBatch As String = ""
        Dim nbtkode As String = "", nbtgudang As String = "", nbtidbatchin As Integer = 0
        Dim updNilaiBatch As String = "", updFilterBatch As String = ""

        'CEK PARAMETER DATA BATCH
        If dataSplit(2).Length > 0 Then

            'VALIDASI DAN SET DATA BATCH ======================================================
            'SPLIT PARAMETER DATA BATCH
            dataBatch = dataSplit(2).Split(sptRow)
            'END OF VALIDASI DAN SET DATA BATCH ===============================================

            'VALIDASI DAN SET DATA ROW BATCH ==================================================
            Dim JmlDtBatch As Integer = dataBatch.Length
            For i = 1 To JmlDtBatch
                'SPLIT DATA DETAIL
                dataRowBatch = dataBatch(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA BATCH -----------------------------------
                'CEK ARRAY DATA BATCH
                If (dataRowBatch.Length <> 19) Then
                    result(2) = "Batch Row : " & i & " - Invalid batch number data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW BATCH ----------------------------

                'VALIDASI TIPE DATA BATCH ------------------------------------------
                'nbtid(0) As Integer
                If (IsNumeric(dataRowBatch(0)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtid required numeric." : GoTo selesai
                End If
                'nbtjenismutasi(1) As Integer
                'JENISMUTASI BARANG MASUK = 1, KELUAR = 0
                dataRowBatch(1) = 0
                If (IsNumeric(dataRowBatch(1)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtjenismutasi required numeric." : GoTo selesai
                End If
                'nbtidbarang(2) As Integer
                If (IsNumeric(dataRowBatch(2)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtidbarang required numeric." : GoTo selesai
                End If
                'nbtidtransaksi(5) As Integer
                If (IsNumeric(dataRowBatch(5)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtidtransaksi required numeric." : GoTo selesai
                End If
                'nbtjml(7) As Double
                If (IsNumeric(dataRowBatch(7)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtjml required numeric." : GoTo selesai
                End If
                'nbtcustomdbl1(11) As Double
                If (IsNumeric(dataRowBatch(11)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl1 required numeric." : GoTo selesai
                End If
                'nbtcustomdbl2(12) As Double
                If (IsNumeric(dataRowBatch(12)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl2 required numeric." : GoTo selesai
                End If
                'nbtcustomdbl3(13) As Double
                If (IsNumeric(dataRowBatch(13)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl3 required numeric." : GoTo selesai
                End If
                'nbtcustomdate1(14) As Date
                If (IsDate(dataRowBatch(14)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate1 required date." : GoTo selesai
                End If
                'nbtcustomdate2(15) As Date
                If (IsDate(dataRowBatch(15)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate2 required date." : GoTo selesai
                End If
                'nbtcustomdate3(16) As Date
                If (IsDate(dataRowBatch(16)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate3 required date." : GoTo selesai
                End If
                'nbtidbatchin(18) As Integer
                If (IsNumeric(dataRowBatch(18)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtidbatchin required numeric." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA BATCH -----------------------------------

                'VALIDASI DATA BATCH ---------------------------------------
                'nbtkode(3) As String
                If Len(dataRowBatch(3)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtkode can't be empty" : GoTo selesai
                End If
                If Len(dataRowBatch(3)) > 100 Then
                    result(2) = "Batch Row : " & i & " - nbtkode should not be more than 100 character." : GoTo selesai
                End If

                'nbtsumber(4) As String
                If Len(dataRowBatch(4)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtsumber can't be empty" : GoTo selesai
                End If
                If Len(dataRowBatch(4)) > 10 Then
                    result(2) = "Batch Row : " & i & " - nbtsumber should not be more than 10 character." : GoTo selesai
                End If

                'nbtsatuan(6) As String
                If Len(dataRowBatch(6)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtsatuan can't be empty" : GoTo selesai
                End If
                If Len(dataRowBatch(6)) > 25 Then
                    result(2) = "Batch Row : " & i & " - nbtsatuan should not be more than 25 character." : GoTo selesai
                End If

                'nbtjml(7) As Double
                If Len(dataRowBatch(7)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtjml can't be empty" : GoTo selesai
                End If

                'nbtcustomdbl1(11) As Double
                If Len(dataRowBatch(11)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl1 can't be empty" : GoTo selesai
                End If

                'nbtcustomdbl2(12) As Double
                If Len(dataRowBatch(12)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl2 can't be empty" : GoTo selesai
                End If

                'nbtcustomdbl3(13) As Double
                If Len(dataRowBatch(13)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl3 can't be empty" : GoTo selesai
                End If

                'nbtcustomdate1(14) As Date
                If Len(dataRowBatch(14)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate1 can't be empty" : GoTo selesai
                End If

                'nbtcustomdate2(15) As Date
                If Len(dataRowBatch(15)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate2 can't be empty" : GoTo selesai
                End If

                'nbtcustomdate3(16) As Date
                If Len(dataRowBatch(16)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate3 can't be empty" : GoTo selesai
                End If

                'nbtgudang(17) As String
                If Len(dataRowBatch(17)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtgudang can't be empty" : GoTo selesai
                End If
                'END OF VALIDASI DATA BATCH --------------------------------

                If AsDataTableTambahData(dtbatch, "nbtid~nbtjenismutasi~nbtidbarang~nbtkode~nbtsumber~nbtidtransaksi~nbtsatuan~nbtjml~nbtcustomtext1~nbtcustomtext2~nbtcustomtext3~nbtcustomdbl1~nbtcustomdbl2~nbtcustomdbl3~nbtcustomdate1~nbtcustomdate2~nbtcustomdate3~nbtgudang~nbtidbatchin", dataRowBatch(0) & "~" & dataRowBatch(1) & "~" & dataRowBatch(2) & "~" & dataRowBatch(3) & "~" & dataRowBatch(4) & "~" & dataRowBatch(5) & "~" & dataRowBatch(6) & "~" & dataRowBatch(7) & "~" & dataRowBatch(8) & "~" & dataRowBatch(9) & "~" & dataRowBatch(10) & "~" & dataRowBatch(11) & "~" & dataRowBatch(12) & "~" & dataRowBatch(13) & "~" & dataRowBatch(14) & "~" & dataRowBatch(15) & "~" & dataRowBatch(16) & "~" & dataRowBatch(17) & "~" & dataRowBatch(18)) = False Then
                    result(2) = "Batch Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

                'ValidasiSimpan
                'BUAT FILTER UNTUK VALIDASI ---------------------------------
                'nbtidbarang(2) As Integer , nbtkode(3) As String      , nbtjml(7) As Double         , nbtgudang(17) As String      , nbtidbatchin(18) As Integer
                idbarang = dataRowBatch(2) : nbtkode = dataRowBatch(3) : jmlbarang = dataRowBatch(7) : nbtgudang = dataRowBatch(17) : nbtidbatchin = dataRowBatch(18)

                'VALIDASI BATCH -------------------------------
                '1. CEK DATA EXIST BATCH KELUAR 
                ftExistBatch = IIf(Len(ftExistBatch.ToString) = 0, "", ftExistBatch & " UNION ")
                ftExistBatch = String.Concat(ftExistBatch, "SELECT EXISTS(SELECT 1 FROM m1_no_batch_in WHERE nbiidbatchin = '" & nbtidbatchin & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & nbtkode & "' as nbikode, '" & nbtgudang & "' as nbigudang FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML BATCH KELUAR 
                Dim jmlKeluar As Double = AsDataTableDSum(dtbatch, "nbtjml", "nbtidbatchin = " & nbtidbatchin & "")
                ftBatch = IIf(Len(ftBatch.ToString) = 0, "", ftBatch & " OR ")
                ftBatch = String.Concat(ftBatch, " (nbi.nbiidbatchin = " & nbtidbatchin & " AND " & jmlKeluar & " > nbi.nbijmlsisa) ")

                '3. SET NILAI UPDATE BATCH IN 
                updNilaiBatch = String.Concat("WHEN nbiidbatchin = '" & nbtidbatchin & "' THEN ROUND(nbijmlkeluar + '" & jmlKeluar & "', 5) ", updNilaiBatch)

                '4. SET FILTER UPDATE BATCH IN 
                updFilterBatch = IIf(Len(updFilterBatch.ToString) = 0, "", updFilterBatch & " OR ")
                updFilterBatch = String.Concat(updFilterBatch, "(nbiidbatchin = '" & nbtidbatchin & "')")
                'END OF BUAT FILTER UNTUK VALIDASI --------------------------

            Next
            'END OF VALIDASI DAN SET ROW DATA BATCH ===========================================

        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'nstid(0) As Integer, nstjenismutasi(1) As Integer, nstidbarang(2) As Integer, nstkode(3) As String, nstsumber(4) As String, 
        'nstidtransaksi(5) As Integer, nstsatuan(6) As String, nstjml(7) As Double, nstcustomtext1(8) As String, nstcustomtext2(9) As String, 
        'nstcustomtext3(10) As String, nstcustomdbl1(11) As Double, nstcustomdbl2(12) As Double, nstcustomdbl3(13) As Double, nstcustomdate1(14) As Date, 
        'nstcustomdate2(15) As Date, nstcustomdate3(16) As Date, nstgudang(17) As String, nstidserialin(18) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'nstid, nstjenismutasi, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, 
        'nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, 
        'nstcustomdate1, nstcustomdate2, nstcustomdate3, nstgudang, nstidserialin

        'Buat datatable serial
        Dim dtserial As New DataTable
        AsDataTableTambahField(dtserial, "nstid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstjenismutasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtserial, "nstidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtserial, "nstkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstidtransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtserial, "nstsatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstjml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtserial, "nstcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstidserialin", AsEnumTypeData.AsInt64)

        'ValidasiSimpan
        Dim ftExistSerial As String = "", ftSerial As String = ""
        Dim nstkode As String = "", nstgudang As String = "", nstidserialin As Integer = 0
        Dim updNilaiSerial As String = "", updFilterSerial As String = ""

        'CEK PARAMETER DATA SERIAL
        If dataSplit(3).Length > 0 Then
            'VALIDASI DAN SET DATA SERIAL ======================================================
            'SPLIT PARAMETER DATA SERIAL
            dataSerial = dataSplit(3).Split(sptRow)
            'END OF VALIDASI DAN SET DATA SERIAL ===============================================

            'VALIDASI DAN SET DATA ROW SERIAL ==================================================
            Dim JmlDtSerial As Integer = dataSerial.Length
            For i = 1 To JmlDtSerial
                'SPLIT DATA SERIAL
                dataRowSerial = dataSerial(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA SERIAL -----------------------------------
                'CEK ARRAY DATA SERIAL
                If (dataRowSerial.Length <> 19) Then
                    result(2) = "Serial Row : " & i & " - Invalid serial number data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW SERIAL ----------------------------

                'VALIDASI TIPE DATA SERIAL ------------------------------------------
                'nstid(0) As Integer
                If (IsNumeric(dataRowSerial(0)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstid required numeric." : GoTo selesai
                End If
                'nstjenismutasi(1) As Integer
                'JENISMUTASI BARANG MASUK = 1, KELUAR = 0
                dataRowSerial(1) = 0
                If (IsNumeric(dataRowSerial(1)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstjenismutasi required numeric." : GoTo selesai
                End If
                'nstidbarang(2) As Integer
                If (IsNumeric(dataRowSerial(2)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstidbarang required numeric." : GoTo selesai
                End If
                'nstidtransaksi(5) As Integer
                If (IsNumeric(dataRowSerial(5)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstidtransaksi required numeric." : GoTo selesai
                End If
                'nstjml(7) As Double
                If (IsNumeric(dataRowSerial(7)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstjml required numeric." : GoTo selesai
                End If
                'nstcustomdbl1(11) As Double
                If (IsNumeric(dataRowSerial(11)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl1 required numeric." : GoTo selesai
                End If
                'nstcustomdbl2(12) As Double
                If (IsNumeric(dataRowSerial(12)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl2 required numeric." : GoTo selesai
                End If
                'nstcustomdbl3(13) As Double
                If (IsNumeric(dataRowSerial(13)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl3 required numeric." : GoTo selesai
                End If
                'nstcustomdate1(14) As Date
                If (IsDate(dataRowSerial(14)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate1 required date." : GoTo selesai
                End If
                'nstcustomdate2(15) As Date
                If (IsDate(dataRowSerial(15)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate2 required date." : GoTo selesai
                End If
                'nstcustomdate3(16) As Date
                If (IsDate(dataRowSerial(16)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate3 required date." : GoTo selesai
                End If
                'nstidserialin(18) As Integer
                If (IsNumeric(dataRowSerial(18)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstidserialin required numeric." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA SERIAL -----------------------------------

                'VALIDASI DATA SERIAL ---------------------------------------
                'nstkode(3) As String
                If Len(dataRowSerial(3)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstkode can't be empty" : GoTo selesai
                End If
                If Len(dataRowSerial(3)) > 100 Then
                    result(2) = "Serial Row : " & i & " - nstkode should not be more than 100 character." : GoTo selesai
                End If

                'nstsumber(4) As String
                If Len(dataRowSerial(4)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstsumber can't be empty" : GoTo selesai
                End If
                If Len(dataRowSerial(4)) > 10 Then
                    result(2) = "Serial Row : " & i & " - nstsumber should not be more than 10 character." : GoTo selesai
                End If

                'nstsatuan(6) As String
                If Len(dataRowSerial(6)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstsatuan can't be empty" : GoTo selesai
                End If
                If Len(dataRowSerial(6)) > 25 Then
                    result(2) = "Serial Row : " & i & " - nstsatuan should not be more than 25 character." : GoTo selesai
                End If

                'nstjml(7) As Double
                If Len(dataRowSerial(7)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstjml can't be empty" : GoTo selesai
                End If

                'nstcustomdbl1(11) As Double
                If Len(dataRowSerial(11)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl1 can't be empty" : GoTo selesai
                End If

                'nstcustomdbl2(12) As Double
                If Len(dataRowSerial(12)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl2 can't be empty" : GoTo selesai
                End If

                'nstcustomdbl3(13) As Double
                If Len(dataRowSerial(13)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl3 can't be empty" : GoTo selesai
                End If

                'nstcustomdate1(14) As Date
                If Len(dataRowSerial(14)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate1 can't be empty" : GoTo selesai
                End If

                'nstcustomdate2(15) As Date
                If Len(dataRowSerial(15)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate2 can't be empty" : GoTo selesai
                End If

                'nstcustomdate3(16) As Date
                If Len(dataRowSerial(16)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate3 can't be empty" : GoTo selesai
                End If

                'nstgudang(17) As String
                If Len(dataRowSerial(16)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstgudang can't be empty" : GoTo selesai
                End If
                'END OF VALIDASI DATA SERIAL --------------------------------

                If AsDataTableTambahData(dtserial, "nstid~nstjenismutasi~nstidbarang~nstkode~nstsumber~nstidtransaksi~nstsatuan~nstjml~nstcustomtext1~nstcustomtext2~nstcustomtext3~nstcustomdbl1~nstcustomdbl2~nstcustomdbl3~nstcustomdate1~nstcustomdate2~nstcustomdate3~nstgudang~nstidserialin", dataRowSerial(0) & "~" & dataRowSerial(1) & "~" & dataRowSerial(2) & "~" & dataRowSerial(3) & "~" & dataRowSerial(4) & "~" & dataRowSerial(5) & "~" & dataRowSerial(6) & "~" & dataRowSerial(7) & "~" & dataRowSerial(8) & "~" & dataRowSerial(9) & "~" & dataRowSerial(10) & "~" & dataRowSerial(11) & "~" & dataRowSerial(12) & "~" & dataRowSerial(13) & "~" & dataRowSerial(14) & "~" & dataRowSerial(15) & "~" & dataRowSerial(16) & "~" & dataRowSerial(17) & "~" & dataRowSerial(18)) = False Then
                    result(2) = "Serial Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

                'ValidasiSimpan
                'BUAT FILTER UNTUK VALIDASI ---------------------------------
                'nstidbarang(2) As Integer  , nstkode(3) As String       , nstjml(7) As Double          , nstgudang(17) As String       , nstidserialin(18) As Integer
                idbarang = dataRowSerial(2) : nstkode = dataRowSerial(3) : jmlbarang = dataRowSerial(7) : nstgudang = dataRowSerial(17) : nstidserialin = dataRowSerial(18)

                'VALIDASI SERIAL -------------------------------
                '1. CEK DATA EXIST SERIAL KELUAR
                ftExistSerial = IIf(Len(ftExistSerial.ToString) = 0, "", ftExistSerial & " UNION ")
                ftExistSerial = String.Concat(ftExistSerial, "SELECT EXISTS(SELECT 1 FROM m1_no_serial_in WHERE nsiidserialin = '" & nstidserialin & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & nstkode & "' as nsikode, '" & nstgudang & "' as nsigudang FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML SERIAL KELUAR 
                Dim jmlKeluar As Double = AsDataTableDSum(dtserial, "nstjml", "nstidserialin = " & nstidserialin & "")
                ftSerial = IIf(Len(ftSerial.ToString) = 0, "", ftSerial & " OR ")
                ftSerial = String.Concat(ftSerial, " (nsi.nsiidserialin = " & nstidserialin & " AND " & jmlKeluar & " > nsi.nsijmlsisa) ")

                '3. SET NILAI UPDATE SERIAL IN 
                updNilaiSerial = String.Concat("WHEN nsiidserialin = '" & nstidserialin & "' THEN ROUND(nsijmlkeluar + '" & jmlKeluar & "', 5) ", updNilaiSerial)

                '4. SET FILTER UPDATE SERIAL IN 
                updFilterSerial = IIf(Len(updFilterSerial.ToString) = 0, "", updFilterSerial & " OR ")
                updFilterSerial = String.Concat(updFilterSerial, "(nsiidserialin = '" & nstidserialin & "')")
                'END OF BUAT FILTER UNTUK VALIDASI --------------------------

            Next
            'END OF VALIDASI DAN SET ROW DATA SERIAL ===========================================
        End If


        'MAPPING BUAT WS DATA ASSET -------------------------------------------------------
        'atid(0) As Integer, atasetid(1) As Integer, atjenismutasi(2) As Integer, atsumber(3) As String, atidutama(4) As Integer, 
        'atidbarang(5) As Integer, atkode(6) As String, atnama(7) As String, atkategori(8) As String, atcabang(9) As String, 
        'atlokasi(10) As String, atgudang(11) As String, atdivisi(12) As String, atsubdivisi(13) As String, atcostcenter(14) As String, 
        'atproyek(15) As String, atcatatan(16) As String, atnomor(17) As String, attglbeli(18) As Date, attglpakai(19) As Date, 
        'atjml(20) As Double, atsatuan(21) As String, atmatauang(22) As String, atkurs(23) As Double, atharga(24) As Double, 
        'atdiskon(25) As String, atjmldiskon(26) As Double, atpajak1(27) As String, atjmlpajak1(28) As Double, atpajak2(29) As String, 
        'atjmlpajak2(30) As Double, athargabeli(31) As Double, atnilairesidu(32) As Double, atumurekonomis(33) As Double, atbebanperbln(34) As Double, 
        'atakumulasibeban(35) As Double, atnilaibuku(36) As Double, atmetode(37) As Integer, attabelpenyusutan(38) As String, atintangible(39) As Integer, 
        'atfiskal(40) As Integer, atatastengahbulan(41) As Integer, atrekasset(42) As String, atrekakumdepresiasi(43) As String, atrekdepresiasi(44) As String, 
        'atrekpenghapusan(45) As String, atprodusen(46) As Integer, attglpensiun(47) As Date, atpenyusutanke(48) As Double, atnilaimenurun(49) As Double, 
        'atdispose(50) As Integer, atpembelian(51) As Integer, atpenjualan(52) As Integer, atlocked(53) As Integer, atstatus(54) As Integer, 
        'atstatussebelumnya(55) As Integer, atisclose(56) As Integer, atinputuser(57) As Integer, atinputtgl(58) As DateTime, atmodifikasiuser(59) As Integer, 
        'atmodifikasitgl(60) As DateTime, atcustomtext1(61) As String, atcustomtext2(62) As String, atcustomtext3(63) As String, atcustomtext4(64) As String, 
        'atcustomtext5(65) As String, atcustomint1(66) As Integer, atcustomint2(67) As Integer, atcustomint3(68) As Integer, atcustomint4(69) As Integer, 
        'atcustomint5(70) As Integer, atcustomdbl1(71) As Double, atcustomdbl2(72) As Double, atcustomdbl3(73) As Double, atcustomdbl4(74) As Double, 
        'atcustomdbl5(75) As Double, atcustomdate1(76) As Date, atcustomdate2(77) As Date, atcustomdate3(78) As Date, atcustomdate4(79) As Date, 
        'atcustomdate5(80) As Date

        'MAPPING BUAT FLEX DATA ASSET -----------------------------------------------------
        'atid, atasetid, atjenismutasi, atsumber, atidutama, atidbarang, atkode, 
        'atnama, atkategori, atcabang, atlokasi, atgudang, atdivisi, atsubdivisi, 
        'atcostcenter, atproyek, atcatatan, atnomor, attglbeli, attglpakai, atjml, 
        'atsatuan, atmatauang, atkurs, atharga, atdiskon, atjmldiskon, atpajak1, 
        'atjmlpajak1, atpajak2, atjmlpajak2, athargabeli, atnilairesidu, atumurekonomis, atbebanperbln, 
        'atakumulasibeban, atnilaibuku, atmetode, attabelpenyusutan, atintangible, atfiskal, atatastengahbulan, 
        'atrekasset, atrekakumdepresiasi, atrekdepresiasi, atrekpenghapusan, atprodusen, attglpensiun, atpenyusutanke, 
        'atnilaimenurun, atdispose, atpembelian, atpenjualan, atlocked, atstatus, atstatussebelumnya, 
        'atisclose, atinputuser, atinputtgl, atmodifikasiuser, atmodifikasitgl, atcustomtext1, atcustomtext2, 
        'atcustomtext3, atcustomtext4, atcustomtext5, atcustomint1, atcustomint2, atcustomint3, atcustomint4, 
        'atcustomint5, atcustomdbl1, atcustomdbl2, atcustomdbl3, atcustomdbl4, atcustomdbl5, atcustomdate1, 
        'atcustomdate2, atcustomdate3, atcustomdate4, atcustomdate5

        'Buat datatable asset
        Dim dtasset As New DataTable
        AsDataTableTambahField(dtasset, "atid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atasetid", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atjenismutasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atidutama", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atkategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atsubdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcostcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atproyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atnomor", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "attglbeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "attglpakai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atjml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtasset, "atsatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atharga", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atjmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atjmlpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atjmlpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "athargabeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atnilairesidu", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atumurekonomis", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atbebanperbln", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atakumulasibeban", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atnilaibuku", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atmetode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "attabelpenyusutan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atintangible", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atfiskal", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atatastengahbulan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atrekasset", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atrekakumdepresiasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atrekdepresiasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atrekpenghapusan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atprodusen", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "attglpensiun", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atpenyusutanke", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atnilaimenurun", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atdispose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atpembelian", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atlocked", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atcustomint4", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atcustomint5", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomdbl4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomdbl5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomdate4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomdate5", AsEnumTypeData.AsString)


        'CEK PARAMETER DATA ASSET
        If dataSplit.Length > 4 Then
            If dataSplit(4).Length > 0 Then

                'VALIDASI DAN SET DATA ASSET ======================================================
                'SPLIT PARAMETER DATA ASSET
                dataAsset = dataSplit(4).Split(sptRow)
                'END OF VALIDASI DAN SET DATA ASSET ===============================================


                'VALIDASI DAN SET DATA ROW ASSET ==================================================
                Dim JmlDtAsset As Integer = dataAsset.Length
                For i = 1 To JmlDtAsset
                    'SPLIT DATA ASSET
                    dataRowAsset = dataAsset(i - 1).Split(sptField)

                    'VALIDASI DAN SET ROW DATA ASSET -----------------------------------
                    'CEK ARRAY DATA ASSET
                    If (dataRowAsset.Length <> 81) Then
                        result(2) = "Asset Row : " & i & " - Invalid asset transaction data parameter." : GoTo selesai
                    End If
                    'END OF VALIDASI DAN SET DATA ROW ASSET ----------------------------

                    'VALIDASI TIPE DATA ASSET ------------------------------------------
                    'atjenismutasi(2) As Integer
                    'JENISMUTASI BARANG MASUK = 1, KELUAR = 0
                    dataRowAsset(2) = 0
                    If (IsNumeric(dataRowAsset(2)) = False) Then
                        result(2) = "Asset Row : " & i & " - atjenismutasi required numeric." : GoTo selesai
                    End If
                    'attglbeli(18) As Date
                    If (IsDate(dataRowAsset(18)) = False) Then
                        result(2) = "Asset Row : " & i & " - attglbeli required date." : GoTo selesai
                    End If
                    'attglpakai(19) As Date
                    If (IsDate(dataRowAsset(19)) = False) Then
                        result(2) = "Asset Row : " & i & " - attglpakai required date." : GoTo selesai
                    End If
                    'atjml(20) As Double
                    If (IsNumeric(dataRowAsset(20)) = False) Then
                        result(2) = "Asset Row : " & i & " - atjml required numeric." : GoTo selesai
                    End If
                    'atkurs(23) As Double
                    If (IsNumeric(dataRowAsset(23)) = False) Then
                        result(2) = "Asset Row : " & i & " - atkurs required numeric." : GoTo selesai
                    End If
                    'atharga(24) As Double
                    If (IsNumeric(dataRowAsset(24)) = False) Then
                        result(2) = "Asset Row : " & i & " - atharga required numeric." : GoTo selesai
                    End If
                    'atjmldiskon(26) As Double
                    If (IsNumeric(dataRowAsset(26)) = False) Then
                        result(2) = "Asset Row : " & i & " - atjmldiskon required numeric." : GoTo selesai
                    End If
                    'atjmlpajak1(28) As Double
                    If (IsNumeric(dataRowAsset(28)) = False) Then
                        result(2) = "Asset Row : " & i & " - atjmlpajak1 required numeric." : GoTo selesai
                    End If
                    'atjmlpajak2(30) As Double
                    If (IsNumeric(dataRowAsset(30)) = False) Then
                        result(2) = "Asset Row : " & i & " - atjmlpajak2 required numeric." : GoTo selesai
                    End If
                    'athargabeli(31) As Double
                    If (IsNumeric(dataRowAsset(31)) = False) Then
                        result(2) = "Asset Row : " & i & " - athargabeli required numeric." : GoTo selesai
                    End If
                    'atnilairesidu(32) As Double
                    If (IsNumeric(dataRowAsset(32)) = False) Then
                        result(2) = "Asset Row : " & i & " - atnilairesidu required numeric." : GoTo selesai
                    End If
                    'atumurekonomis(33) As Double
                    If (IsNumeric(dataRowAsset(33)) = False) Then
                        result(2) = "Asset Row : " & i & " - atumurekonomis required numeric." : GoTo selesai
                    End If
                    'atbebanperbln(34) As Double
                    If (IsNumeric(dataRowAsset(34)) = False) Then
                        result(2) = "Asset Row : " & i & " - atbebanperbln required numeric." : GoTo selesai
                    End If
                    'atakumulasibeban(35) As Double
                    If (IsNumeric(dataRowAsset(35)) = False) Then
                        result(2) = "Asset Row : " & i & " - atakumulasibeban required numeric." : GoTo selesai
                    End If
                    'atnilaibuku(36) As Double
                    If (IsNumeric(dataRowAsset(36)) = False) Then
                        result(2) = "Asset Row : " & i & " - atnilaibuku required numeric." : GoTo selesai
                    End If
                    'atmetode(37) As Integer
                    If (IsNumeric(dataRowAsset(37)) = False) Then
                        result(2) = "Asset Row : " & i & " - atmetode required numeric." : GoTo selesai
                    End If
                    'atintangible(39) As Integer
                    If (IsNumeric(dataRowAsset(39)) = False) Then
                        result(2) = "Asset Row : " & i & " - atintangible required numeric." : GoTo selesai
                    End If
                    'atfiskal(40) As Integer
                    If (IsNumeric(dataRowAsset(40)) = False) Then
                        result(2) = "Asset Row : " & i & " - atfiskal required numeric." : GoTo selesai
                    End If
                    'atatastengahbulan(41) As Integer
                    If (IsNumeric(dataRowAsset(41)) = False) Then
                        result(2) = "Asset Row : " & i & " - atatastengahbulan required numeric." : GoTo selesai
                    End If
                    'attglpensiun(47) As Date
                    If (IsDate(dataRowAsset(47)) = False) Then
                        result(2) = "Asset Row : " & i & " - attglpensiun required date." : GoTo selesai
                    End If
                    'atpenyusutanke(48) As Double
                    If (IsNumeric(dataRowAsset(48)) = False) Then
                        result(2) = "Asset Row : " & i & " - atpenyusutanke required numeric." : GoTo selesai
                    End If
                    'atnilaimenurun(49) As Double
                    If (IsNumeric(dataRowAsset(49)) = False) Then
                        result(2) = "Asset Row : " & i & " - atnilaimenurun required numeric." : GoTo selesai
                    End If
                    'atdispose(50) As Integer
                    If (IsNumeric(dataRowAsset(50)) = False) Then
                        result(2) = "Asset Row : " & i & " - atdispose required numeric." : GoTo selesai
                    End If
                    'atpembelian(51) As Integer
                    If (IsNumeric(dataRowAsset(51)) = False) Then
                        result(2) = "Asset Row : " & i & " - atpembelian required numeric." : GoTo selesai
                    End If
                    'atpenjualan(52) As Integer
                    If (IsNumeric(dataRowAsset(52)) = False) Then
                        result(2) = "Asset Row : " & i & " - atpenjualan required numeric." : GoTo selesai
                    End If
                    'atlocked(53) As Integer
                    If (IsNumeric(dataRowAsset(53)) = False) Then
                        result(2) = "Asset Row : " & i & " - atlocked required numeric." : GoTo selesai
                    End If
                    'atstatus(54) As Integer
                    If (IsNumeric(dataRowAsset(54)) = False) Then
                        result(2) = "Asset Row : " & i & " - atstatus required numeric." : GoTo selesai
                    End If
                    'atstatussebelumnya(55) As Integer
                    If (IsNumeric(dataRowAsset(55)) = False) Then
                        result(2) = "Asset Row : " & i & " - atstatussebelumnya required numeric." : GoTo selesai
                    End If
                    'atisclose(56) As Integer
                    If (IsNumeric(dataRowAsset(56)) = False) Then
                        result(2) = "Asset Row : " & i & " - atisclose required numeric." : GoTo selesai
                    End If
                    'atinputtgl(58) As DateTime
                    If (IsDate(dataRowAsset(58)) = False) Then
                        result(2) = "Asset Row : " & i & " - atinputtgl required date." : GoTo selesai
                    End If
                    'atmodifikasitgl(60) As DateTime
                    If (IsDate(dataRowAsset(60)) = False) Then
                        result(2) = "Asset Row : " & i & " - atmodifikasitgl required date." : GoTo selesai
                    End If
                    'atcustomint1(66) As Integer
                    If (IsNumeric(dataRowAsset(66)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomint1 required numeric." : GoTo selesai
                    End If
                    'atcustomint2(67) As Integer
                    If (IsNumeric(dataRowAsset(67)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomint2 required numeric." : GoTo selesai
                    End If
                    'atcustomint3(68) As Integer
                    If (IsNumeric(dataRowAsset(68)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomint3 required numeric." : GoTo selesai
                    End If
                    'atcustomint4(69) As Integer
                    If (IsNumeric(dataRowAsset(69)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomint4 required numeric." : GoTo selesai
                    End If
                    'atcustomint5(70) As Integer
                    If (IsNumeric(dataRowAsset(70)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomint5 required numeric." : GoTo selesai
                    End If
                    'atcustomdbl1(71) As Double
                    If (IsNumeric(dataRowAsset(71)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomdbl1 required numeric." : GoTo selesai
                    End If
                    'atcustomdbl2(72) As Double
                    If (IsNumeric(dataRowAsset(72)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomdbl2 required numeric." : GoTo selesai
                    End If
                    'atcustomdbl3(73) As Double
                    If (IsNumeric(dataRowAsset(73)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomdbl3 required numeric." : GoTo selesai
                    End If
                    'atcustomdbl4(74) As Double
                    If (IsNumeric(dataRowAsset(74)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomdbl4 required numeric." : GoTo selesai
                    End If
                    'atcustomdbl5(75) As Double
                    If (IsNumeric(dataRowAsset(75)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomdbl5 required numeric." : GoTo selesai
                    End If
                    'atcustomdate1(76) As Date
                    If (IsDate(dataRowAsset(76)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomdate1 required date." : GoTo selesai
                    End If
                    'atcustomdate2(77) As Date
                    If (IsDate(dataRowAsset(77)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomdate2 required date." : GoTo selesai
                    End If
                    'atcustomdate3(78) As Date
                    If (IsDate(dataRowAsset(78)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomdate3 required date." : GoTo selesai
                    End If
                    'atcustomdate4(79) As Date
                    If (IsDate(dataRowAsset(79)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomdate4 required date." : GoTo selesai
                    End If
                    'atcustomdate5(80) As Date
                    If (IsDate(dataRowAsset(80)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomdate5 required date." : GoTo selesai
                    End If
                    'END OF VALIDASI TIPE DATA ASSET -----------------------------------

                    'VALIDASI DATA ASSET ---------------------------------------
                    'atid(0) As 
                    If Len(dataRowAsset(0)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atid can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(0)) > 20 Then
                        result(2) = "Asset Row : " & i & " - atid should not be more than 20 character." : GoTo selesai
                    End If

                    'atasetid(1) As 
                    If Len(dataRowAsset(1)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atasetid can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(1)) > 20 Then
                        result(2) = "Asset Row : " & i & " - atasetid should not be more than 20 character." : GoTo selesai
                    End If

                    'atsumber(3) As String
                    If Len(dataRowAsset(3)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atsumber can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(3)) > 25 Then
                        result(2) = "Asset Row : " & i & " - atsumber should not be more than 25 character." : GoTo selesai
                    End If

                    'atidutama(4) As 
                    If Len(dataRowAsset(4)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atidutama can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(4)) > 20 Then
                        result(2) = "Asset Row : " & i & " - atidutama should not be more than 20 character." : GoTo selesai
                    End If

                    'atidbarang(5) As 
                    If Len(dataRowAsset(5)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atidbarang can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(5)) > 20 Then
                        result(2) = "Asset Row : " & i & " - atidbarang should not be more than 20 character." : GoTo selesai
                    End If

                    'atkode(6) As String
                    If Len(dataRowAsset(6)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atkode can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(6)) > 25 Then
                        result(2) = "Asset Row : " & i & " - atkode should not be more than 25 character." : GoTo selesai
                    End If

                    'atnama(7) As String
                    If Len(dataRowAsset(7)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atnama can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(7)) > 100 Then
                        result(2) = "Asset Row : " & i & " - atnama should not be more than 100 character." : GoTo selesai
                    End If

                    'atkategori(8) As String
                    If Len(dataRowAsset(8)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atkategori can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(8)) > 25 Then
                        result(2) = "Asset Row : " & i & " - atkategori should not be more than 25 character." : GoTo selesai
                    End If

                    'attglbeli(18) As Date
                    If Len(dataRowAsset(18)) = 0 Then
                        result(2) = "Asset Row : " & i & " - attglbeli can't be empty" : GoTo selesai
                    End If

                    'attglpakai(19) As Date
                    If Len(dataRowAsset(19)) = 0 Then
                        result(2) = "Asset Row : " & i & " - attglpakai can't be empty" : GoTo selesai
                    End If

                    'atjml(20) As Double
                    If Len(dataRowAsset(20)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atjml can't be empty" : GoTo selesai
                    End If

                    'atsatuan(21) As String
                    If Len(dataRowAsset(21)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atsatuan can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(21)) > 25 Then
                        result(2) = "Asset Row : " & i & " - atsatuan should not be more than 25 character." : GoTo selesai
                    End If

                    'atmatauang(22) As String
                    If Len(dataRowAsset(22)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atmatauang can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(22)) > 25 Then
                        result(2) = "Asset Row : " & i & " - atmatauang should not be more than 25 character." : GoTo selesai
                    End If

                    'atkurs(23) As Double
                    If Len(dataRowAsset(23)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atkurs can't be empty" : GoTo selesai
                    End If

                    'atharga(24) As Double
                    If Len(dataRowAsset(24)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atharga can't be empty" : GoTo selesai
                    End If

                    'atdiskon(25) As String
                    If Len(dataRowAsset(25)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atdiskon can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(25)) > 25 Then
                        result(2) = "Asset Row : " & i & " - atdiskon should not be more than 25 character." : GoTo selesai
                    End If

                    'atjmldiskon(26) As Double
                    If Len(dataRowAsset(26)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atjmldiskon can't be empty" : GoTo selesai
                    End If

                    'atjmlpajak1(28) As Double
                    If Len(dataRowAsset(28)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atjmlpajak1 can't be empty" : GoTo selesai
                    End If

                    'atjmlpajak2(30) As Double
                    If Len(dataRowAsset(30)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atjmlpajak2 can't be empty" : GoTo selesai
                    End If

                    'athargabeli(31) As Double
                    If Len(dataRowAsset(31)) = 0 Then
                        result(2) = "Asset Row : " & i & " - athargabeli can't be empty" : GoTo selesai
                    End If

                    'atnilairesidu(32) As Double
                    If Len(dataRowAsset(32)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atnilairesidu can't be empty" : GoTo selesai
                    End If

                    'atumurekonomis(33) As Double
                    If Len(dataRowAsset(33)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atumurekonomis can't be empty" : GoTo selesai
                    End If

                    'atbebanperbln(34) As Double
                    If Len(dataRowAsset(34)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atbebanperbln can't be empty" : GoTo selesai
                    End If

                    'atakumulasibeban(35) As Double
                    If Len(dataRowAsset(35)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atakumulasibeban can't be empty" : GoTo selesai
                    End If

                    'atnilaibuku(36) As Double
                    If Len(dataRowAsset(36)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atnilaibuku can't be empty" : GoTo selesai
                    End If

                    'atrekasset(42) As String
                    If Len(dataRowAsset(42)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atrekasset can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(42)) > 25 Then
                        result(2) = "Asset Row : " & i & " - atrekasset should not be more than 25 character." : GoTo selesai
                    End If

                    'atrekakumdepresiasi(43) As String
                    If Len(dataRowAsset(43)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atrekakumdepresiasi can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(43)) > 25 Then
                        result(2) = "Asset Row : " & i & " - atrekakumdepresiasi should not be more than 25 character." : GoTo selesai
                    End If

                    'atrekdepresiasi(44) As String
                    If Len(dataRowAsset(44)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atrekdepresiasi can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(44)) > 25 Then
                        result(2) = "Asset Row : " & i & " - atrekdepresiasi should not be more than 25 character." : GoTo selesai
                    End If

                    'atprodusen(46) As 
                    If Len(dataRowAsset(46)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atprodusen can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(46)) > 20 Then
                        result(2) = "Asset Row : " & i & " - atprodusen should not be more than 20 character." : GoTo selesai
                    End If

                    'attglpensiun(47) As Date
                    If Len(dataRowAsset(47)) = 0 Then
                        result(2) = "Asset Row : " & i & " - attglpensiun can't be empty" : GoTo selesai
                    End If

                    'atpenyusutanke(48) As Double
                    If Len(dataRowAsset(48)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atpenyusutanke can't be empty" : GoTo selesai
                    End If

                    'atnilaimenurun(49) As Double
                    If Len(dataRowAsset(49)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atnilaimenurun can't be empty" : GoTo selesai
                    End If

                    'atinputuser(57) As 
                    If Len(dataRowAsset(57)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atinputuser can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(57)) > 20 Then
                        result(2) = "Asset Row : " & i & " - atinputuser should not be more than 20 character." : GoTo selesai
                    End If

                    'atinputtgl(58) As DateTime
                    If Len(dataRowAsset(58)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atinputtgl can't be empty" : GoTo selesai
                    End If

                    'atmodifikasiuser(59) As 
                    If Len(dataRowAsset(59)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atmodifikasiuser can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(59)) > 20 Then
                        result(2) = "Asset Row : " & i & " - atmodifikasiuser should not be more than 20 character." : GoTo selesai
                    End If

                    'atmodifikasitgl(60) As DateTime
                    If Len(dataRowAsset(60)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atmodifikasitgl can't be empty" : GoTo selesai
                    End If

                    'atcustomdbl1(71) As Double
                    If Len(dataRowAsset(71)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atcustomdbl1 can't be empty" : GoTo selesai
                    End If

                    'atcustomdbl2(72) As Double
                    If Len(dataRowAsset(72)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atcustomdbl2 can't be empty" : GoTo selesai
                    End If

                    'atcustomdbl3(73) As Double
                    If Len(dataRowAsset(73)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atcustomdbl3 can't be empty" : GoTo selesai
                    End If

                    'atcustomdbl4(74) As Double
                    If Len(dataRowAsset(74)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atcustomdbl4 can't be empty" : GoTo selesai
                    End If

                    'atcustomdbl5(75) As Double
                    If Len(dataRowAsset(75)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atcustomdbl5 can't be empty" : GoTo selesai
                    End If

                    'atcustomdate1(76) As Date
                    If Len(dataRowAsset(76)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atcustomdate1 can't be empty" : GoTo selesai
                    End If

                    'atcustomdate2(77) As Date
                    If Len(dataRowAsset(77)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atcustomdate2 can't be empty" : GoTo selesai
                    End If

                    'atcustomdate3(78) As Date
                    If Len(dataRowAsset(78)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atcustomdate3 can't be empty" : GoTo selesai
                    End If

                    'atcustomdate4(79) As Date
                    If Len(dataRowAsset(79)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atcustomdate4 can't be empty" : GoTo selesai
                    End If

                    'atcustomdate5(80) As Date
                    If Len(dataRowAsset(80)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atcustomdate5 can't be empty" : GoTo selesai
                    End If
                    'END OF VALIDASI DATA ASSET --------------------------------

                    If AsDataTableTambahData(dtasset, "atid~atasetid~atjenismutasi~atsumber~atidutama~atidbarang~atkode~atnama~atkategori~atcabang~atlokasi~atgudang~atdivisi~atsubdivisi~atcostcenter~atproyek~atcatatan~atnomor~attglbeli~attglpakai~atjml~atsatuan~atmatauang~atkurs~atharga~atdiskon~atjmldiskon~atpajak1~atjmlpajak1~atpajak2~atjmlpajak2~athargabeli~atnilairesidu~atumurekonomis~atbebanperbln~atakumulasibeban~atnilaibuku~atmetode~attabelpenyusutan~atintangible~atfiskal~atatastengahbulan~atrekasset~atrekakumdepresiasi~atrekdepresiasi~atrekpenghapusan~atprodusen~attglpensiun~atpenyusutanke~atnilaimenurun~atdispose~atpembelian~atpenjualan~atlocked~atstatus~atstatussebelumnya~atisclose~atinputuser~atinputtgl~atmodifikasiuser~atmodifikasitgl~atcustomtext1~atcustomtext2~atcustomtext3~atcustomtext4~atcustomtext5~atcustomint1~atcustomint2~atcustomint3~atcustomint4~atcustomint5~atcustomdbl1~atcustomdbl2~atcustomdbl3~atcustomdbl4~atcustomdbl5~atcustomdate1~atcustomdate2~atcustomdate3~atcustomdate4~atcustomdate5", dataRowAsset(0) & "~" & dataRowAsset(1) & "~" & dataRowAsset(2) & "~" & dataRowAsset(3) & "~" & dataRowAsset(4) & "~" & dataRowAsset(5) & "~" & dataRowAsset(6) & "~" & dataRowAsset(7) & "~" & dataRowAsset(8) & "~" & dataRowAsset(9) & "~" & dataRowAsset(10) & "~" & dataRowAsset(11) & "~" & dataRowAsset(12) & "~" & dataRowAsset(13) & "~" & dataRowAsset(14) & "~" & dataRowAsset(15) & "~" & dataRowAsset(16) & "~" & dataRowAsset(17) & "~" & dataRowAsset(18) & "~" & dataRowAsset(19) & "~" & dataRowAsset(20) & "~" & dataRowAsset(21) & "~" & dataRowAsset(22) & "~" & dataRowAsset(23) & "~" & dataRowAsset(24) & "~" & dataRowAsset(25) & "~" & dataRowAsset(26) & "~" & dataRowAsset(27) & "~" & dataRowAsset(28) & "~" & dataRowAsset(29) & "~" & dataRowAsset(30) & "~" & dataRowAsset(31) & "~" & dataRowAsset(32) & "~" & dataRowAsset(33) & "~" & dataRowAsset(34) & "~" & dataRowAsset(35) & "~" & dataRowAsset(36) & "~" & dataRowAsset(37) & "~" & dataRowAsset(38) & "~" & dataRowAsset(39) & "~" & dataRowAsset(40) & "~" & dataRowAsset(41) & "~" & dataRowAsset(42) & "~" & dataRowAsset(43) & "~" & dataRowAsset(44) & "~" & dataRowAsset(45) & "~" & dataRowAsset(46) & "~" & dataRowAsset(47) & "~" & dataRowAsset(48) & "~" & dataRowAsset(49) & "~" & dataRowAsset(50) & "~" & dataRowAsset(51) & "~" & dataRowAsset(52) & "~" & dataRowAsset(53) & "~" & dataRowAsset(54) & "~" & dataRowAsset(55) & "~" & dataRowAsset(56) & "~" & dataRowAsset(57) & "~" & dataRowAsset(58) & "~" & dataRowAsset(59) & "~" & dataRowAsset(60) & "~" & dataRowAsset(61) & "~" & dataRowAsset(62) & "~" & dataRowAsset(63) & "~" & dataRowAsset(64) & "~" & dataRowAsset(65) & "~" & dataRowAsset(66) & "~" & dataRowAsset(67) & "~" & dataRowAsset(68) & "~" & dataRowAsset(69) & "~" & dataRowAsset(70) & "~" & dataRowAsset(71) & "~" & dataRowAsset(72) & "~" & dataRowAsset(73) & "~" & dataRowAsset(74) & "~" & dataRowAsset(75) & "~" & dataRowAsset(76) & "~" & dataRowAsset(77) & "~" & dataRowAsset(78) & "~" & dataRowAsset(79) & "~" & dataRowAsset(80)) = False Then
                        result(2) = "Asset Row : " & i & " - insert into datatable failed." : GoTo selesai
                    End If

                Next
                'END OF VALIDASI DAN SET ROW DATA ASSET ===========================================

            End If
        End If

        'SIMPAN KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0
        Dim vStatus As Integer = 0, vTgl As String = ""

        Try
            'Proses utama
            If (dtutama.Rows.Count > 0) Then
                Dim drutama As DataRow = dtutama.Rows(0)
                vStatus = drutama("rsstatus")
                vTgl = AsFormatTanggal(drutama("rstgl"))

                'CEK HAK AKSES STATUS ============================
                Dim vAkses As Integer = 0, msgAkses As String = ""
                'MODUL DAN MENU HARUS DISESUAIKAN
                Dim vModuleId As Integer = 3, vMenuId As Integer = 5
                Select Case drutama("rsstatus")
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


                'CEK PERIODE AKUNTANSI ==================================
                Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("rstgl")), AsFormatTanggal(drutama("rstgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================

                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("rsstatus") = 2 Or drutama("rsstatus") = 1 Or drutama("rsstatus") = 8 Or drutama("rsstatus") = 9 Or drutama("rsstatus") = 10 Or drutama("rsstatus") = 11 Then

                    'VALIDASI BATCH SERIAL ---------------
                    'ValidasiBatchSerial
                    Dim rsValidasi As String = ValidasiBatchSerial(dtdetail, dtbatch, dtserial, ftBarang, "jmlbarang", 0)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    'END OF VALIDASI BATCH SERIAL --------

                    'VALIDASI ASSET ----------------------
                    'ValidasiAsset
                    rsValidasi = ValidasiAsset(dtdetail, dtasset, ftBarang, "jmlbarang", 0)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    'END OF VALIDASI ASSET ---------------

                    'VALIDASI GUDANG ASSET ---------------
                    'ValidasiGudangAsset
                    rsValidasi = ValidasiGudangAsset(dtasset, gudangOut)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai

                    rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstanding, ftOutstanding, ftExistStok, ftStok, ftExistBatch, ftBatch, ftExistSerial, ftSerial, "gudangtransit")
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================

                If isUpdate Then
                    result(4) = drutama("rsid")
                    notransaksi = drutama("rsnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(rsid), rsnotransaksi FROM M3_Rs WHERE rsid='" & result(4) & "' AND rsstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("rsautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rscabang"), drutama("rslokasi"), drutama("rssumber"), drutama("rstgl"), drutama("rssumber"), 3)
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(rsid) FROM m3_rs WHERE rsnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m3_rs_history
                        Dim rsSimpanHistory As String = SimpanHistory.M3_Rs_HistorySimpan("" & paramSplit(0) & "★M3_Rs_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("rssumber")) & "▼" & FixQuotes(drutama("rsid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M3_Rs set rscabang  = '" & FixQuotes(drutama("rscabang")) & "', rslokasi  = '" & FixQuotes(drutama("rslokasi")) & "', rsgudangasal  = '" & FixQuotes(drutama("rsgudangasal")) & "', rsgudangtransit  = '" & FixQuotes(drutama("rsgudangtransit")) & "', rsgudangtujuan  = '" & FixQuotes(drutama("rsgudangtujuan")) & "', rssumber  = '" & FixQuotes(drutama("rssumber")) & "', rsautonotransaksi  = " & drutama("rsautonotransaksi") & ", rsnotransaksi  = '" & notransaksi & "', rstgl  = '" & FixQuotes(AsFormatTanggal(drutama("rstgl"))) & "', rskodepa  = " & drutama("rskodepa") & ", rsbagianterima  = " & drutama("rsbagianterima") & ", rsbagianterimakontak  = '" & FixQuotes(drutama("rsbagianterimakontak")) & "', rsuraian  = '" & FixQuotes(drutama("rsuraian")) & "', rscatatan  = '" & FixQuotes(drutama("rscatatan")) & "', rsnoref  = '" & FixQuotes(drutama("rsnoref")) & "', rstglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("rstglnoref"))) & "', rsidmr  = " & drutama("rsidmr") & ", rsidts  = " & drutama("rsidts") & ", rsstatus  = " & drutama("rsstatus") & ", rsstatussebelumnya  = " & drutama("rsstatussebelumnya") & ", rsjmlrevisi  = rsjmlrevisi+1, rscetakanke  = " & drutama("rscetakanke") & ", rsmodifikasiuser  = " & drutama("rsmodifikasiuser") & ", rsmodifikasitgl  = NOW(), rscustomtext1  = '" & FixQuotes(drutama("rscustomtext1")) & "', rscustomtext2  = '" & FixQuotes(drutama("rscustomtext2")) & "', rscustomtext3  = '" & FixQuotes(drutama("rscustomtext3")) & "', rscustomtext4  = '" & FixQuotes(drutama("rscustomtext4")) & "', rscustomtext5  = '" & FixQuotes(drutama("rscustomtext5")) & "', rscustomint1  = " & drutama("rscustomint1") & ", rscustomint2  = " & drutama("rscustomint2") & ", rscustomint3  = " & drutama("rscustomint3") & ", rscustomdbl1  = '" & FixDouble(drutama("rscustomdbl1")) & "', rscustomdbl2  = '" & FixDouble(drutama("rscustomdbl2")) & "', rscustomdbl3  = '" & FixDouble(drutama("rscustomdbl3")) & "', rscustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("rscustomdate1"))) & "', rscustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("rscustomdate2"))) & "', rscustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("rscustomdate3"))) & "' where rsid = '" & drutama("rsid") & "'"
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

                    If drutama("rsautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rscabang"), drutama("rslokasi"), drutama("rssumber"), drutama("rstgl"), drutama("rssumber"), 3)
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
                        notransaksi = drutama("rsnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(rsid) FROM m3_rs WHERE rsnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M3_Rs (rscabang, rslokasi, rsgudangasal, rsgudangtransit, rsgudangtujuan, rssumber, rsautonotransaksi, rsnotransaksi, rstgl, rskodepa, rsbagianterima, rsbagianterimakontak, rsuraian, rscatatan, rsnoref, rstglnoref, rsidmr, rsidts, rsstatus, rsstatussebelumnya, rsjmlrevisi, rscetakanke, rsinputuser, rsinputtgl, rsmodifikasiuser, rsmodifikasitgl, rsisclose, rscustomtext1, rscustomtext2, rscustomtext3, rscustomtext4, rscustomtext5, rscustomint1, rscustomint2, rscustomint3, rscustomdbl1, rscustomdbl2, rscustomdbl3, rscustomdate1, rscustomdate2, rscustomdate3) values('" & FixQuotes(drutama("rscabang")) & "', '" & FixQuotes(drutama("rslokasi")) & "', '" & FixQuotes(drutama("rsgudangasal")) & "', '" & FixQuotes(drutama("rsgudangtransit")) & "', '" & FixQuotes(drutama("rsgudangtujuan")) & "', '" & FixQuotes(drutama("rssumber")) & "', " & drutama("rsautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("rstgl"))) & "', " & drutama("rskodepa") & ", " & drutama("rsbagianterima") & ", '" & FixQuotes(drutama("rsbagianterimakontak")) & "', '" & FixQuotes(drutama("rsuraian")) & "', '" & FixQuotes(drutama("rscatatan")) & "', '" & FixQuotes(drutama("rsnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rstglnoref"))) & "', " & drutama("rsidmr") & ", " & drutama("rsidts") & ", " & drutama("rsstatus") & ", " & drutama("rsstatussebelumnya") & ", " & drutama("rsjmlrevisi") & ", " & drutama("rscetakanke") & ", " & drutama("rsinputuser") & ", NOW(), " & drutama("rsmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("rsisclose") & ", '" & FixQuotes(drutama("rscustomtext1")) & "', '" & FixQuotes(drutama("rscustomtext2")) & "', '" & FixQuotes(drutama("rscustomtext3")) & "', '" & FixQuotes(drutama("rscustomtext4")) & "', '" & FixQuotes(drutama("rscustomtext5")) & "', " & drutama("rscustomint1") & ", " & drutama("rscustomint2") & ", " & drutama("rscustomint3") & ", '" & FixDouble(drutama("rscustomdbl1")) & "', '" & FixDouble(drutama("rscustomdbl2")) & "', '" & FixDouble(drutama("rscustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rscustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rscustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rscustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select rsid from M3_rs where rsnotransaksi='" & notransaksi & "' AND rsinputuser= '" & userid & "' order by rsmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M3_Rs_Detail where idrs = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idrsdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idmrdetail") & ", " & dr1("idtsdetail") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M3_Rs_Detail(idrsdetail, idrs, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idmrdetail, idtsdetail, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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


                'Hapus batch ketika update
                If (isUpdate) Then
                    sql = "Delete from M1_No_Batch_Transaction where nbtidtransaksi  = '" & result(4) & "' AND nbtsumber = 'RS'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses batch
                If (dtbatch.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtbatch.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & 0 & ", " & dr1("nbtjenismutasi") & ", " & dr1("nbtidbatchin") & ", '" & FixQuotes(dr1("nbtgudang")) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', '" & FixQuotes(dr1("nbtcustomtext1")) & "', '" & FixQuotes(dr1("nbtcustomtext2")) & "', '" & FixQuotes(dr1("nbtcustomtext3")) & "', '" & FixDouble(dr1("nbtcustomdbl1")) & "', '" & FixDouble(dr1("nbtcustomdbl2")) & "', '" & FixDouble(dr1("nbtcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate3"))) & "')")
                    Next
                    sql = "Insert into M1_No_Batch_Transaction(nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Hapus serial ketika update
                If (isUpdate) Then
                    sql = "Delete from M1_No_Serial_Transaction  where nstidtransaksi  = '" & result(4) & "' AND nstsumber = 'RS'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses serial
                If (dtserial.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtserial.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & 0 & ", " & dr1("nstjenismutasi") & ", " & dr1("nstidserialin") & ", '" & FixQuotes(dr1("nstgudang")) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', '" & FixQuotes(dr1("nstcustomtext1")) & "', '" & FixQuotes(dr1("nstcustomtext2")) & "', '" & FixQuotes(dr1("nstcustomtext3")) & "', '" & FixDouble(dr1("nstcustomdbl1")) & "', '" & FixDouble(dr1("nstcustomdbl2")) & "', '" & FixDouble(dr1("nstcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate3"))) & "')")
                    Next
                    sql = "Insert into M1_No_Serial_Transaction(nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                'Hapus asset ketika update
                If (isUpdate) Then
                    sql = "Delete from M7_Asset_Transaction where atidutama  = '" & result(4) & "' AND atsumber = 'RS'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses asset
                If (dtasset.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtasset.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('0', '" & FixQuotes(dr1("atasetid")) & "', " & dr1("atjenismutasi") & ", '" & FixQuotes(dr1("atsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("atidbarang")) & "', '" & FixQuotes(dr1("atkode")) & "', '" & FixQuotes(dr1("atnama")) & "', '" & FixQuotes(dr1("atkategori")) & "', '" & FixQuotes(dr1("atcabang")) & "', '" & FixQuotes(dr1("atlokasi")) & "', '" & FixQuotes(dr1("atgudang")) & "', '" & FixQuotes(dr1("atdivisi")) & "', '" & FixQuotes(dr1("atsubdivisi")) & "', '" & FixQuotes(dr1("atcostcenter")) & "', '" & FixQuotes(dr1("atproyek")) & "', '" & FixQuotes(dr1("atcatatan")) & "', '" & FixQuotes(dr1("atnomor")) & "', '" & FixQuotes(AsFormatTanggal(dr1("attglbeli"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("attglpakai"))) & "', '" & FixDouble(dr1("atjml")) & "', '" & FixQuotes(dr1("atsatuan")) & "', '" & FixQuotes(dr1("atmatauang")) & "', '" & FixDouble(dr1("atkurs")) & "', '" & FixDouble(dr1("atharga")) & "', '" & FixQuotes(dr1("atdiskon")) & "', '" & FixDouble(dr1("atjmldiskon")) & "', '" & FixQuotes(dr1("atpajak1")) & "', '" & FixDouble(dr1("atjmlpajak1")) & "', '" & FixQuotes(dr1("atpajak2")) & "', '" & FixDouble(dr1("atjmlpajak2")) & "', '" & FixDouble(dr1("athargabeli")) & "', '" & FixDouble(dr1("atnilairesidu")) & "', '" & FixDouble(dr1("atumurekonomis")) & "', '" & FixDouble(dr1("atbebanperbln")) & "', '" & FixDouble(dr1("atakumulasibeban")) & "', '" & FixDouble(dr1("atnilaibuku")) & "', " & dr1("atmetode") & ", '" & FixQuotes(dr1("attabelpenyusutan")) & "', " & dr1("atintangible") & ", " & dr1("atfiskal") & ", " & dr1("atatastengahbulan") & ", '" & FixQuotes(dr1("atrekasset")) & "', '" & FixQuotes(dr1("atrekakumdepresiasi")) & "', '" & FixQuotes(dr1("atrekdepresiasi")) & "', '" & FixQuotes(dr1("atrekpenghapusan")) & "', '" & FixQuotes(dr1("atprodusen")) & "', '" & FixQuotes(AsFormatTanggal(dr1("attglpensiun"))) & "', '" & FixDouble(dr1("atpenyusutanke")) & "', '" & FixDouble(dr1("atnilaimenurun")) & "', " & dr1("atdispose") & ", " & dr1("atpembelian") & ", " & dr1("atpenjualan") & ", " & dr1("atlocked") & ", " & vStatus & ", " & dr1("atstatussebelumnya") & ", " & dr1("atisclose") & ", '" & FixQuotes(dr1("atinputuser")) & "', '" & FixQuotes(AsFormatTanggal(dr1("atinputtgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(dr1("atmodifikasiuser")) & "', '" & FixQuotes(AsFormatTanggal(dr1("atmodifikasitgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(dr1("atcustomtext1")) & "', '" & FixQuotes(dr1("atcustomtext2")) & "', '" & FixQuotes(dr1("atcustomtext3")) & "', '" & FixQuotes(dr1("atcustomtext4")) & "', '" & FixQuotes(dr1("atcustomtext5")) & "', " & dr1("atcustomint1") & ", " & dr1("atcustomint2") & ", " & dr1("atcustomint3") & ", " & dr1("atcustomint4") & ", " & dr1("atcustomint5") & ", '" & FixDouble(dr1("atcustomdbl1")) & "', '" & FixDouble(dr1("atcustomdbl2")) & "', '" & FixDouble(dr1("atcustomdbl3")) & "', '" & FixDouble(dr1("atcustomdbl4")) & "', '" & FixDouble(dr1("atcustomdbl5")) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate4"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate5"))) & "', '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(vTgl)) & "')")
                    Next
                    sql = "Insert into M7_Asset_Transaction(atid, atasetid, atjenismutasi, atsumber, atidutama, atidbarang, atkode, atnama, atkategori, atcabang, atlokasi, atgudang, atdivisi, atsubdivisi, atcostcenter, atproyek, atcatatan, atnomor, attglbeli, attglpakai, atjml, atsatuan, atmatauang, atkurs, atharga, atdiskon, atjmldiskon, atpajak1, atjmlpajak1, atpajak2, atjmlpajak2, athargabeli, atnilairesidu, atumurekonomis, atbebanperbln, atakumulasibeban, atnilaibuku, atmetode, attabelpenyusutan, atintangible, atfiskal, atatastengahbulan, atrekasset, atrekakumdepresiasi, atrekdepresiasi, atrekpenghapusan, atprodusen, attglpensiun, atpenyusutanke, atnilaimenurun, atdispose, atpembelian, atpenjualan, atlocked, atstatus, atstatussebelumnya, atisclose, atinputuser, atinputtgl, atmodifikasiuser, atmodifikasitgl, atcustomtext1, atcustomtext2, atcustomtext3, atcustomtext4, atcustomtext5, atcustomint1, atcustomint2, atcustomint3, atcustomint4, atcustomint5, atcustomdbl1, atcustomdbl2, atcustomdbl3, atcustomdbl4, atcustomdbl5, atcustomdate1, atcustomdate2, atcustomdate3, atcustomdate4, atcustomdate5, atnotransaksi, attgl) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                If drutama("rsstatus") = 2 Then
                    If Len(updNilai) > 0 Then
                        'UPDATE OUTSTANDING TRANSAKSI ===============================================
                        'UPDATE DETAIL
                        sql = "UPDATE m3_ts_detail SET jmlrealisasi = (CASE idtsdetail " & updNilai & " ELSE jmlrealisasi END) WHERE " & updFilter
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE UTAMA
                        Dim ftDetail As String = "", statusOut As Integer = 0
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idts FROM m3_ts_detail WHERE " & updFilter & " GROUP BY idts", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idts = '" & dr1("idts") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idts, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m3_ts_detail WHERE " & ftDetail & " GROUP BY idts", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilai = "" : updFilter = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                    statusOut = 2
                                ElseIf dr1("jmlrealisasi") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilai = String.Concat(updNilai, "WHEN '" & dr1("idts") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                                updFilter = String.Concat(updFilter, "(tsid = '" & dr1("idts") & "')")
                            Next

                            sql = "UPDATE m3_ts SET tsstatusrealisasi = (CASE tsid " & updNilai & " ELSE tsstatusrealisasi END) WHERE " & updFilter
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                        'END OF UPDATE OUTSTANDING TRANSAKSI ========================================
                    End If

                    If Len(updNilaiMR) > 0 Then
                        'UPDATE OUTSTANDING TRANSAKSI MR ============================================
                        'UPDATE DETAIL
                        sql = "UPDATE m3_mr_detail SET jmlrealisasi = (CASE idmrdetail " & updNilaiMR & " ELSE jmlrealisasi END) WHERE " & updFilterMR
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE UTAMA
                        Dim ftDetail As String = "", statusOut As Integer = 0
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idmr FROM m3_mr_detail WHERE " & updFilterMR & " GROUP BY idmr", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idmr = '" & dr1("idmr") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idmr, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m3_mr_detail WHERE " & ftDetail & " GROUP BY idmr", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiMR = "" : updFilterMR = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                    statusOut = 2
                                ElseIf dr1("jmlrealisasi") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilaiMR = String.Concat(updNilaiMR, "WHEN '" & dr1("idmr") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterMR = IIf(Len(updFilterMR.ToString) = 0, "", updFilterMR & " OR ")
                                updFilterMR = String.Concat(updFilterMR, "(mrid = '" & dr1("idmr") & "')")
                            Next

                            sql = "UPDATE m3_mr SET mrstatusrealisasi = (CASE mrid " & updNilaiMR & " ELSE mrstatusrealisasi END) WHERE " & updFilterMR
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                        'END OF UPDATE OUTSTANDING TRANSAKSI MR =====================================
                    End If


                    'AMBIL GUDANG TUJUAN DARI UTAMA =================================================
                    'GUDANG TUJUAN UTAMA DIGUNAKAN UNTUK NO SERIAL DAN BATCH MASUK
                    'MISAL : GUDANG TRANSIT 'I', MAKA :
                    '-- NO SERIAL DAN BATCH GUDANG 'I' BERKURANG
                    '-- NO SERIAL DAN BATCH GUDANG TUJUAN BERTAMBAH
                    Dim SetGudang As String = drutama("rsgudangtujuan")
                    'END OF AMBIL GUDANG TUJUAN DARI UTAMA ==========================================


                    'INSERT NO BATCH ================================================================
                    If dtbatch.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder, strValue3 As New StringBuilder
                        For Each dr1 As DataRow In dtbatch.Rows
                            'QUERY INSERT NO BATCH OUT
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping             nboid,            nboidbatchin,                           nbogudang,                  nboidbarang,                           nbokode,                             nbosumber,            nboidtransaksi,                     nbosatuan,                         nbojmlkeluar,       nboisclose
                            strValue2.Append("(" & 0 & ", " & dr1("nbtidbatchin") & ", '" & FixQuotes(dr1("nbtgudang")) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', " & 0 & ")")

                            'QUERY INSERT NO BATCH IN
                            strValue3.Append(IIf(Len(strValue3.ToString) = 0, "", ", "))
                            'mapping        nbiidbatchin,                nbigudang,                nbiidbarang,                           nbikode,                             nbisumber,            nbiidtransaksi,                     nbisatuan,                 nbijmlmasuk,       nbijmlkeluar,                  nbijmlsisa, nbiisclose,                     nbicustomtext1,                             nbicustomtext2,                             nbicustomtext3,                             nbicustomdbl1,                             nbicustomdbl2,                             nbicustomdbl3,                                             nbicustomdate1,                                              nbicustomdate2,                                              nbicustomdate3
                            strValue3.Append("(" & 0 & ", '" & FixQuotes(SetGudang) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixQuotes(dr1("nbtcustomtext1")) & "', '" & FixQuotes(dr1("nbtcustomtext2")) & "', '" & FixQuotes(dr1("nbtcustomtext3")) & "', '" & FixDouble(dr1("nbtcustomdbl1")) & "', '" & FixDouble(dr1("nbtcustomdbl2")) & "', '" & FixDouble(dr1("nbtcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate3"))) & "')")
                        Next

                        'INSERT NO BATCH OUT ---------------------------------
                        sql = "Insert into M1_No_Batch_Out(nboid, nboidbatchin, nbogudang, nboidbarang, nbokode, nbosumber, nboidtransaksi, nbosatuan, nbojmlkeluar, nboisclose) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE NO BATCH IN KELUAR ---------------------------
                        If Len(updNilaiBatch) > 0 Then
                            sql = "UPDATE m1_no_batch_in SET nbijmlkeluar =  (CASE " & updNilaiBatch & " ELSE nbijmlkeluar END) WHERE " & updFilterBatch
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If

                        'INSERT NO BATCH IN MASUK ----------------------------
                        sql = "Insert into M1_No_Batch_In(nbiidbatchin, nbigudang, nbiidbarang, nbikode, nbisumber, nbiidtransaksi, nbisatuan, nbijmlmasuk, nbijmlkeluar, nbijmlsisa, nbiisclose, nbicustomtext1, nbicustomtext2, nbicustomtext3, nbicustomdbl1, nbicustomdbl2, nbicustomdbl3, nbicustomdate1, nbicustomdate2, nbicustomdate3) values" & strValue3.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    End If
                    'END OF INSERT NO BATCH =========================================================

                    'INSERT NO SERIAL ===============================================================
                    If dtserial.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder, strValue3 As New StringBuilder
                        For Each dr1 As DataRow In dtserial.Rows
                            'QUERY INSERT NO SERIAL OUT
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping            nsoid,             nsoidserialin,                           nsogudang,                  nsoidbarang,                           nsokode,                             nsosumber,            nsoidtransaksi,                     nsosatuan,                          nsojmlkeluar,      nsoisclose
                            strValue2.Append("(" & 0 & ", " & dr1("nstidserialin") & ", '" & FixQuotes(dr1("nstgudang")) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', " & 0 & ")")

                            'QUERY INSERT NO SERIAL IN
                            strValue3.Append(IIf(Len(strValue3.ToString) = 0, "", ", "))
                            'mapping       nsiidserialin,                nsigudang,                nsiidbarang,                           nsikode,                             nsisumber,            nsiidtransaksi,                     nsisatuan,                       nsijmlmasuk, nsijmlkeluar,                  nsijmlsisa, nsiisclose,                     nsicustomtext1,                             nsicustomtext2,                             nsicustomtext3,                             nsicustomdbl1,                             nsicustomdbl2,                             nsicustomdbl3,                                             nsicustomdate1,                                              nsicustomdate2,                                              nsicustomdate3
                            strValue3.Append("(" & 0 & ", '" & FixQuotes(SetGudang) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixQuotes(dr1("nstcustomtext1")) & "', '" & FixQuotes(dr1("nstcustomtext2")) & "', '" & FixQuotes(dr1("nstcustomtext3")) & "', '" & FixDouble(dr1("nstcustomdbl1")) & "', '" & FixDouble(dr1("nstcustomdbl2")) & "', '" & FixDouble(dr1("nstcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate3"))) & "')")
                        Next

                        'INSERT NO SERIAL OUT --------------------------------
                        sql = "Insert into M1_No_Serial_Out(nsoid, nsoidserialin, nsogudang, nsoidbarang, nsokode, nsosumber, nsoidtransaksi, nsosatuan, nsojmlkeluar, nsoisclose) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE NO SERIAL IN KELUAR --------------------------
                        If Len(updNilaiSerial) > 0 Then
                            sql = "UPDATE m1_no_serial_in SET nsijmlkeluar =  (CASE " & updNilaiSerial & " ELSE nsijmlkeluar END) WHERE " & updFilterSerial
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If

                        'INSERT NO SERIAL IN MASUK ---------------------------
                        sql = "Insert into M1_No_Serial_In(nsiidserialin, nsigudang, nsiidbarang, nsikode, nsisumber, nsiidtransaksi, nsisatuan, nsijmlmasuk, nsijmlkeluar, nsijmlsisa, nsiisclose, nsicustomtext1, nsicustomtext2, nsicustomtext3, nsicustomdbl1, nsicustomdbl2, nsicustomdbl3, nsicustomdate1, nsicustomdate2, nsicustomdate3) values" & strValue3.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    End If
                    'END OF INSERT NO SERIAL ========================================================


                    'INSERT NO ASSET ===============================================================
                    If dtasset.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder
                        For Each dr1 As DataRow In dtasset.Rows
                            'QUERY INSERT NO ASSET IN
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            strValue2.Append(FixDouble(dr1("atasetid")))
                        Next
                        sql = "UPDATE m7_asset a SET a.agudang = '" & SetGudang & "' WHERE a.aid IN(" & strValue2.ToString & ")"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF INSERT NO ASSET ========================================================


                    'UPDATE STOK ====================================================================
                    'STOK KELUAR
                    sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokOut & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'STOK MASUK
                    sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokIn & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                    'END OF UPDATE STOK =============================================================


                    'INSERT ITEM TRANSACTION ========================================================
                    'AMBIL DATA DETAIL YANG BARU
                    sql = "SELECT rsd.idrsdetail, rsd.idbarang, rsd.namabarang, rsd.tipebarang, rsd.jml, rsd.satuan, rsd.jmlbarang, rsd.satuanbarang, rsd.gudangasal, rsd.gudangtransit, rsd.gudangtujuan, rsd.catatan, rsd.costcenter, rsd.divisi, rsd.subdivisi, rsd.proyek, rs.rsinputtgl, i.bhpp, i.bhppaverage FROM m3_rs_detail rsd JOIN m3_rs rs ON rsd.idrs = rs.rsid JOIN m1_item i ON rsd.idbarang = i.bid WHERE rsd.idrs = '" & result(4) & "'"
                    Dim dtDetailNew As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                    Dim hpp As Double = 0, jenismutasi As Double = 0, postinghpp As Double = 0
                    Dim strTransaksiBarang As New StringBuilder

                    If dtDetailNew.Rows.Count > 0 Then

                        'AMBIL MATAUANG FUNGSIONAL DARI SETTING
                        Dim dtMatauang As DataTable = AsDataTableAmbilDariDBCon("SELECT skode, snilai FROM m0_setting WHERE smodule = 0 AND sgrup = 'accounting' AND (skode = 'MataUangFungsional' OR skode = 'Kurs')", myConn)
                        Dim uangFungsional As String = AsDataTableDLookup(dtMatauang, "snilai", "skode = 'MataUangFungsional'", "Not found")
                        If uangFungsional = "Not found" Then
                            result(2) = "Setting Functional Currency not found." : GoTo selesai
                        End If
                        Dim kursFungsional As String = AsDataTableDLookup(dtMatauang, "snilai", "skode = 'Kurs'", "Not found")
                        If kursFungsional = "Not found" Then
                            result(2) = "Setting Exchange Rate Functional Currency not found." : GoTo selesai
                        End If

                        For Each dr1 As DataRow In dtDetailNew.Rows
                            'jenismutasi dan postinghpp 
                            '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 1
                            '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                            '- untuk transaksi mutasi saja maka postinghpp = 0
                            postinghpp = 0

                            'hitung hpp = hpp
                            hpp = Double.Parse(dr1("bhppaverage"))

                            'POSTING BARANG KELUAR (gudangtransit)
                            jenismutasi = 0
                            'QUERY INSERT TRANSAKSI BARANG KELUAR
                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                            'mapping                        id,                            cabang,                                    lokasi,                                 gudang,                        kodepa,           jenismutasi,                              sumber,                    idutama,             iddetail,                      notransaksi,                                                 tgl,                              kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                               kurs,                    harga,                 diskon,              jmldiskon,             idhppikm,  idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("rscabang")) & "', '" & FixQuotes(drutama("rslokasi")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', " & drutama("rskodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("rssumber")) & "', " & result(4) & ", " & dr1("idrsdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("rstgl"))) & "', " & drutama("rsbagianterima") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(uangFungsional) & "', '" & FixDouble(kursFungsional) & "', '" & FixDouble(hpp) & "', '" & FixQuotes(0) & "', '" & FixDouble(0) & "', " & 0 & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("rsuraian")) & "', '" & FixQuotes(drutama("rscatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("rsinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("rsinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")

                            'POSTING BARANG MASUK (gudangtujuan)
                            jenismutasi = 1
                            'QUERY INSERT TRANSAKSI BARANG MASUK
                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                            'mapping                        id,                            cabang,                                    lokasi,                                   gudang,                        kodepa,           jenismutasi,                              sumber,                    idutama,             iddetail,                      notransaksi,                                                 tgl,                              kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                               kurs,                    harga,                 diskon,              jmldiskon,          idhppikm,  idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("rscabang")) & "', '" & FixQuotes(drutama("rslokasi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', " & drutama("rskodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("rssumber")) & "', " & result(4) & ", " & dr1("idrsdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("rstgl"))) & "', " & drutama("rsbagianterima") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(uangFungsional) & "', '" & FixDouble(kursFungsional) & "', '" & FixDouble(hpp) & "', '" & FixQuotes(0) & "', '" & FixDouble(0) & "', " & 0 & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("rsuraian")) & "', '" & FixQuotes(drutama("rscatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("rsinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("rsinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                        Next

                        sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    Else
                        result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF INSERT ITEM TRANSACTION =================================================

                End If

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "RS", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M3_RsUpdateStatus(ByVal param As String) As String

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
        Dim dtdetail As DataTable, dtasset As DataTable
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
            Filter = Filter.Replace("rsbagianterimakode", "c1.kkode")
            Filter = Filter.Replace("rsbagianterimanama", "c1.knama")
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
            Dim sumber As String = "RS", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT rstgl, rsnotransaksi, rsstatus FROM m3_rs WHERE rsid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Rsstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m3_rs_history
            Dim rsSimpanHistory As String = SimpanHistory.M3_Rs_HistorySimpan("" & paramSplit(0) & "★M3_Rs_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then

                'CEK NO BATCH DAN SERIAL ========================================================
                'BATCH
                dtdetail = AsDataTableAmbilDariDBCon("SELECT bkode, nbikode FROM m1_no_batch_in JOIN m1_item ON nbiidbarang = bid WHERE nbisumber = '" & sumber & "' AND nbiidtransaksi = '" & idtransaksi & "' AND nbijmlkeluar > 0", myConn)
                If dtdetail.Rows.Count > 0 Then result(2) = "Item : " & dtdetail.Rows(0)("bkode") & " | No. Batch : " & dtdetail.Rows(0)("nbikode") & " has related transactions." : Trans.Rollback() : GoTo selesai

                'SERIAL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT bkode, nsikode FROM m1_no_serial_in JOIN m1_item ON nsiidbarang = bid WHERE nsisumber = '" & sumber & "' AND nsiidtransaksi = '" & idtransaksi & "' AND nsijmlkeluar > 0", myConn)
                If dtdetail.Rows.Count > 0 Then result(2) = "Item : " & dtdetail.Rows(0)("bkode") & " | No. Serial : " & dtdetail.Rows(0)("nsikode") & " has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK NO BATCH DAN SERIAL =================================================


                Dim idbarang As Integer = 0, jmlbarang As Double = 0, gudangOut As String = "", gudangIn As String = "", idtsdetail As Integer = 0, idmrdetail As Integer = 0
                Dim updNilai As String = "", updFilter As String = "", updNilaiMR As String = "", updFilterMR As String = "", ftExistStok As String = "", ftStok As String = "", updStokIn As String = "", updStokOut As String = ""

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, gudangtransit, gudangtujuan, idmrdetail, idtsdetail, urutan FROM m3_rs_detail WHERE idrs = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : gudangIn = dr1("gudangtransit") : gudangOut = dr1("gudangtujuan") : idmrdetail = dr1("idmrdetail") : idtsdetail = dr1("idtsdetail")

                        'UPDATE OUTSTANDING ---------------------------
                        If idtsdetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idtsdetail=" & idtsdetail)
                            updNilai = String.Concat("WHEN '" & idtsdetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilai)

                            '2. SET FILTERUPDATE OUTSTANDING
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(idtsdetail = '" & idtsdetail & "')")
                        End If

                        If idmrdetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING MR
                            Dim OutstandingMR As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idmrdetail=" & idmrdetail)
                            updNilaiMR = String.Concat("WHEN '" & idmrdetail & "' THEN ROUND(jmlrealisasi - '" & OutstandingMR & "', 5) ", updNilaiMR)

                            '2. SET FILTERUPDATE OUTSTANDING MR
                            updFilterMR = IIf(Len(updFilterMR.ToString) = 0, "", updFilterMR & " OR ")
                            updFilterMR = String.Concat(updFilterMR, "(idmrdetail = '" & idmrdetail & "')")
                        End If

                        'VALIDASI STOK -------------------------------
                        '1. CEK DATA EXIST
                        ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                        ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idtsdetail & "' as idtsdetail, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                        '2. CEK JML STOK
                        Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangtujuan='" & gudangOut & "'")
                        ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
                        ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")

                        '3. SET NILAI UPDATE STOK KELUAR
                        updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
                        updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

                        '4. SET NILAI UPDATE STOK MASUK
                        updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
                        updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok
                        'END OF BUAT FILTER UNTUK UPDATE --------------------------
                    Next

                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If


                'VALIDASI STOK ----------------------------------
                Dim rsValidasi As String = ValidasiSimpan(dtdetail, "", "", ftExistStok, ftStok, "", "", "", "", "")
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'END OF VALIDASI STOK ---------------------------


                'VALIDASI GUDANG ASSET ---------------
                'ValidasiGudangAsset
                dtasset = AsDataTableAmbilDariDBCon("SELECT atasetid, atidbarang, atkode FROM M7_Asset_Transaction WHERE atsumber = '" & sumber & "' AND atidutama = '" & idtransaksi & "' ", myConn)
                rsValidasi = ValidasiGudangAsset(dtasset, gudangOut)
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                'END OF VALIDASI GUDANG ASSET --------


                If Len(updFilter) > 0 Then
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m3_ts_detail SET jmlrealisasi = (CASE idtsdetail " & updNilai & " ELSE jmlrealisasi END) WHERE " & updFilter
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                    'END OF UPDATE OUTSTANDING DETAIL ---------------

                    'UPDATE OUTSTANDING UTAMA -----------------------
                    Dim ftDetail As String = "", statusOut As Integer = 0
                    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idts FROM m3_ts_detail WHERE " & updFilter & " GROUP BY idts", myConn)
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idts = '" & dr1("idts") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDBCon("SELECT idts, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m3_ts_detail WHERE " & ftDetail & " GROUP BY idts", myConn)
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilai = "" : updFilter = ""
                        For Each dr1 As DataRow In dtOut.Rows
                            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                            If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                statusOut = 2
                            ElseIf dr1("jmlrealisasi") < 1 Then
                                statusOut = 0
                            Else
                                statusOut = 1
                            End If
                            '2. SET NILAI UPDATE OUTSTANDING
                            updNilai = String.Concat(updNilai, "WHEN '" & dr1("idts") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(tsid = '" & dr1("idts") & "')")
                        Next

                        sql = "UPDATE m3_ts SET tsstatusrealisasi = (CASE tsid " & updNilai & " ELSE tsstatusrealisasi END) WHERE " & updFilter
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF UPDATE OUTSTANDING UTAMA ----------------
                End If

                If Len(updFilterMR) > 0 Then
                    'UPDATE OUTSTANDING DETAIL MR -------------------
                    sql = "UPDATE m3_mr_detail SET jmlrealisasi = (CASE idmrdetail " & updNilaiMR & " ELSE jmlrealisasi END) WHERE " & updFilterMR
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                    'END OF UPDATE OUTSTANDING DETAIL MR ------------

                    'UPDATE OUTSTANDING UTAMA MR --------------------
                    Dim ftDetail As String = "", statusOut As Integer = 0
                    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idmr FROM m3_mr_detail WHERE " & updFilterMR & " GROUP BY idmr", myConn)
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idmr = '" & dr1("idmr") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDBCon("SELECT idmr, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m3_mr_detail WHERE " & ftDetail & " GROUP BY idmr", myConn)
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiMR = "" : updFilterMR = ""
                        For Each dr1 As DataRow In dtOut.Rows
                            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                            If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                statusOut = 2
                            ElseIf dr1("jmlrealisasi") < 1 Then
                                statusOut = 0
                            Else
                                statusOut = 1
                            End If
                            '2. SET NILAI UPDATE OUTSTANDING MR
                            updNilaiMR = String.Concat(updNilaiMR, "WHEN '" & dr1("idmr") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING MR
                            updFilterMR = IIf(Len(updFilterMR.ToString) = 0, "", updFilterMR & " OR ")
                            updFilterMR = String.Concat(updFilterMR, "(mrid = '" & dr1("idmr") & "')")
                        Next

                        sql = "UPDATE m3_mr SET mrstatusrealisasi = (CASE mrid " & updNilaiMR & " ELSE mrstatusrealisasi END) WHERE " & updFilterMR
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF UPDATE OUTSTANDING UTAMA MR -------------
                End If


                'UPDATE NO BATCH ================================================================
                Dim updNilaiBatch As String = "", updFilterBatch As String = ""
                Dim dtBatch As DataTable = AsDataTableAmbilDariDBCon("SELECT nboidbatchin, nbogudang, nboidbarang, nbokode, nbojmlkeluar FROM m1_no_batch_out WHERE nbosumber = '" & sumber & "' AND nboidtransaksi = '" & idtransaksi & "'", myConn)
                If dtBatch.Rows.Count > 0 Then
                    'DELETE NO BATCH IN MASUK ---------------------------
                    sql = "DELETE FROM m1_no_batch_in WHERE nbisumber = '" & sumber & "' AND nbiidtransaksi = '" & idtransaksi & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'DELETE NO BATCH OUT --------------------------------
                    sql = "DELETE FROM m1_no_batch_out WHERE nbosumber = '" & sumber & "' AND nboidtransaksi = '" & idtransaksi & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE NO BATCH IN KELUAR --------------------------
                    For Each dr1 As DataRow In dtBatch.Rows
                        'SET NILAI UPDATE BATCH IN
                        Dim jmlKeluar As Double = AsDataTableDSum(dtBatch, "nbojmlkeluar", "nboidbatchin = " & dr1("nboidbatchin") & "")
                        updNilaiBatch = String.Concat("WHEN nbiidbatchin = '" & dr1("nboidbatchin") & "' THEN ROUND(nbijmlkeluar - '" & jmlKeluar & "', 5) ", updNilaiBatch)

                        'SET FILTER UPDATE BATCH IN
                        updFilterBatch = IIf(Len(updFilterBatch.ToString) = 0, "", updFilterBatch & " OR ")
                        updFilterBatch = String.Concat(updFilterBatch, "(nbiidbatchin = '" & dr1("nboidbatchin") & "')")
                    Next
                    If Len(updNilaiBatch) > 0 Then
                        sql = "UPDATE m1_no_batch_in SET nbijmlkeluar =  (CASE " & updNilaiBatch & " ELSE nbijmlkeluar END) WHERE " & updFilterBatch
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
                'END OF UPDATE NO BATCH =========================================================


                'UPDATE NO SERIAL ===============================================================
                Dim updNilaiSerial As String = "", updFilterSerial As String = ""
                Dim dtSerial As DataTable = AsDataTableAmbilDariDBCon("SELECT nsoidserialin, nsogudang, nsoidbarang, nsokode, nsojmlkeluar FROM m1_no_serial_out WHERE nsosumber = '" & sumber & "' AND nsoidtransaksi = '" & idtransaksi & "'", myConn)
                If dtSerial.Rows.Count > 0 Then
                    'DELETE NO SERIAL IN MASUK --------------------------
                    sql = "DELETE FROM m1_no_serial_in WHERE nsisumber = '" & sumber & "' AND nsiidtransaksi = '" & idtransaksi & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'DELETE NO SERIAL OUT -------------------------------
                    sql = "DELETE FROM m1_no_serial_out WHERE nsosumber = '" & sumber & "' AND nsoidtransaksi = '" & idtransaksi & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE NO SERIAL IN KELUAR -------------------------
                    For Each dr1 As DataRow In dtSerial.Rows
                        'SET NILAI UPDATE SERIAL IN
                        Dim jmlKeluar As Double = AsDataTableDSum(dtSerial, "nsojmlkeluar", "nsoidserialin = " & dr1("nsoidserialin") & "")
                        updNilaiSerial = String.Concat("WHEN nsiidserialin = '" & dr1("nsoidserialin") & "' THEN ROUND(nsijmlkeluar - '" & jmlKeluar & "', 5) ", updNilaiSerial)

                        'SET FILTER UPDATE SERIAL IN
                        updFilterSerial = IIf(Len(updFilterSerial.ToString) = 0, "", updFilterSerial & " OR ")
                        updFilterSerial = String.Concat(updFilterSerial, "(nsiidserialin = '" & dr1("nsoidserialin") & "')")
                    Next
                    If Len(updNilaiSerial) > 0 Then
                        sql = "UPDATE m1_no_serial_in SET nsijmlkeluar =  (CASE " & updNilaiSerial & " ELSE nsijmlkeluar END) WHERE " & updFilterSerial
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
                'END OF UPDATE NO SERIAL =======================================================


                'UPDATE NO ASSET ===============================================================
                If dtasset.Rows.Count > 0 Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtasset.Rows
                        'QUERY INSERT NO ASSET IN
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append(FixDouble(dr1("atasetid")))
                    Next
                    sql = "UPDATE m7_asset a SET a.agudang = '" & gudangIn & "' WHERE a.aid IN(" & strValue2.ToString & ")"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE NO ASSET ========================================================


                'UPDATE STOK ==================================================================
                'STOK KELUAR
                sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokOut & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'STOK MASUK
                sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokIn & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
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
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF DELETE TRANSAKSI BARANG ===============================================

            End If

            'update status utama
            sql = "UPDATE M3_rs SET rsstatus = " & nilaiStatus & ", rsmodifikasiuser='" & userid & "', rsmodifikasitgl = NOW(), rsposting = 0, rspostingtgl = '1971-01-01 00:00:00', rsjmlrevisi = rsjmlrevisi + 1 WHERE rsid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M3_RsSearch(PostWsSearch(paramSplit(0), "M3_RsSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M3_RsDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("rsbagianterimakode", "c1.kkode")
            Filter = Filter.Replace("rsbagianterimanama", "c1.knama")
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
            Dim sumber As String = "RS", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Rsid, Rsnotransaksi FROM m3_Rs WHERE Rsid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT rscabang, rslokasi, rssumber, rsautonotransaksi, rsnotransaksi, rstgl"
            sql &= " FROM M3_rs"
            sql &= " WHERE rsid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("rscabang")
                lokasi = dtNomorNext.Rows(0)("rslokasi")
                sumber = dtNomorNext.Rows(0)("rssumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("rsautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("rsnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("rstgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'HAPUS BATCH
            sql = "Delete from M1_No_Batch_Transaction where nbtidtransaksi = '" & idtransaksi & "' AND nbtsumber = '" & sumber & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'HAPUS SERIAL
            sql = "Delete from M1_No_Serial_Transaction where nstidtransaksi = '" & idtransaksi & "' AND nstsumber = '" & sumber & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'HAPUS ASSET
            sql = "Delete from M7_Asset_Transaction where atidutama = '" & idtransaksi & "' AND atsumber = '" & sumber & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE DETAIL
            sql = "DELETE FROM M3_Rs_Detail WHERE idrs = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE UTAMA
            sql = "DELETE FROM M3_Rs WHERE rsid = '" & idtransaksi & "'"
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
                Dim rsNomorNext As String = M0_DeleteNotransaksi(cabang, lokasi, sumber, tgl, notransaksi, sumber, 3)
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
            Dim paramSearch As String = M3_RsSearch(PostWsSearch(paramSplit(0), "M3_RsSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M3_RsGetdataById(ByVal param As String) As String

        'M3_RsGetdataById Utama --------------------------------------------------------
        'rsid, rscabang, rslokasi, rsgudangasal, rsgudangtransit, rsgudangtujuan, rssumber, 
        'rsautonotransaksi, rsnotransaksi, rstgl, rskodepa, rsbagianterima, rsbagianterimakontak, rsuraian, 
        'rscatatan, rsnoref, rstglnoref, rsidmr, rsidts, rsstatus, rsstatussebelumnya, 
        'rsjmlrevisi, rscetakanke, rsinputuser, rsinputtgl, rsmodifikasiuser, rsmodifikasitgl, rsposting, 
        'rspostingtgl, rsisclose, rscustomtext1, rscustomtext2, rscustomtext3, rscustomtext4, rscustomtext5, 
        'rscustomint1, rscustomint2, rscustomint3, rscustomdbl1, rscustomdbl2, rscustomdbl3, rscustomdate1, 
        'rscustomdate2, rscustomdate3, rscabangnama, rslokasinama, rsgudangasalnama, rsgudangtransitnama, rsgudangtujuannama, 
        'rsbagianterimakode, rsbagianterimanama, rsmrnotransaksi, rstsnotransaksi, rsstatusnama, rsstatussebelumnyanama, rsinputusernama, 
        'rsmodifikasiusernama

        'M3_RsGetdataById Detail -------------------------------------------------------
        'idrsdetail, idrs, idbarang, namabarang, tipebarang, jml, 
        'satuan, nilaisatuan, jmlbarang, satuanbarang, cabang, lokasi, gudangasal, 
        'gudangtransit, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idmrdetail, idtsdetail, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, 
        'bhpp, bhppaverage, bjenis, bserial, bbatch, cabangnama, lokasinama, 
        'gudangasalnama, gudangtransitnama, gudangtujuannama, costcenternama, divisinama, subdivisinama, proyeknama, 
        'mrnotransaksi, tsnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan

        'M3_RsGetdataById Batch --------------------------------------------------------
        'nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang

        'M3_RsGetdataById Serial --------------------------------------------------------
        'nstid, nstjenismutasi, nstidserialin, nstgudang,  nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, 
        'nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, 
        'nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang

        'M3_RsGetdataById Asset --------------------------------------------------------
        'atid, atasetid, atjenismutasi, atsumber, atidutama, atidbarang, atkode, 
        'atnama, atkategori, atcabang, atlokasi, atgudang, atdivisi, atsubdivisi, 
        'atcostcenter, atproyek, atcatatan, atnomor, attglbeli, attglpakai, atjml, 
        'atsatuan, atmatauang, atkurs, atharga, atdiskon, atjmldiskon, atpajak1, 
        'atjmlpajak1, atpajak2, atjmlpajak2, athargabeli, atnilairesidu, atumurekonomis, atbebanperbln, 
        'atakumulasibeban, atnilaibuku, atnilaipenyusutan, atmetode, attabelpenyusutan, atintangible, atfiskal, 
        'atatastengahbulan, atrekasset, atrekakumdepresiasi, atrekdepresiasi, atrekpenghapusan, atprodusen, attglpensiun, 
        'atpenyusutanke, atnilaimenurun, atdispose, atpembelian, atpenjualan, atlocked, atstatus, 
        'atstatussebelumnya, atisclose, atinputuser, atinputtgl, atmodifikasiuser, atmodifikasitgl, atcustomtext1, 
        'atcustomtext2, atcustomtext3, atcustomtext4, atcustomtext5, atcustomint1, atcustomint2, atcustomint3, 
        'atcustomint4, atcustomint5, atcustomdbl1, atcustomdbl2, atcustomdbl3, atcustomdbl4, atcustomdbl5, 
        'atcustomdate1, atcustomdate2, atcustomdate3, atcustomdate4, atcustomdate5, atkategorinama, atcabangnama, 
        'atlokasinama, atgudangnama, atdivisinama, atsubdivisinama, atcostcenternama, atproyeknama, atmetodenama, 
        'atpajak1nama, atpajak1nilai, atpajak2nama, atpajak2nilai, atrekassetnama, atrekakumdepresiasinama, atrekdepresiasinama, 
        'atrekpenghapusannama, atprodusenkode, atprodusennama, atstatusnama, atstatussebelumnyanama, atinputusernama, atmodifikasiusernama

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

        Dim utama As String = "", detail As String = "", batch As String = "", serial As String = "", idtransaksi As String = ""
        Dim sumber As String = "RS", asset As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M3_Rs~M3_Rs_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "rsid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "rsid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m3_rs_getdata")
        sql = "select `rs`.`rsid` AS `rsid`,`rs`.`rscabang` AS `rscabang`,`rs`.`rslokasi` AS `rslokasi`,`rs`.`rsgudangasal` AS `rsgudangasal`,`rs`.`rsgudangtransit` AS `rsgudangtransit`,`rs`.`rsgudangtujuan` AS `rsgudangtujuan`,`rs`.`rssumber` AS `rssumber`,`rs`.`rsautonotransaksi` AS `rsautonotransaksi`,`rs`.`rsnotransaksi` AS `rsnotransaksi`,`rs`.`rstgl` AS `rstgl`,`rs`.`rskodepa` AS `rskodepa`,`rs`.`rsbagianterima` AS `rsbagianterima`,`rs`.`rsbagianterimakontak` AS `rsbagianterimakontak`,`rs`.`rsuraian` AS `rsuraian`,`rs`.`rscatatan` AS `rscatatan`,`rs`.`rsnoref` AS `rsnoref`,`rs`.`rstglnoref` AS `rstglnoref`,`rs`.`rsidmr` AS `rsidmr`,`rs`.`rsidts` AS `rsidts`,`rs`.`rsstatus` AS `rsstatus`,`rs`.`rsstatussebelumnya` AS `rsstatussebelumnya`,`rs`.`rsjmlrevisi` AS `rsjmlrevisi`,`rs`.`rscetakanke` AS `rscetakanke`,`rs`.`rsinputuser` AS `rsinputuser`,`rs`.`rsinputtgl` AS `rsinputtgl`,`rs`.`rsmodifikasiuser` AS `rsmodifikasiuser`,`rs`.`rsmodifikasitgl` AS `rsmodifikasitgl`,`rs`.`rsposting` AS `rsposting`,`rs`.`rspostingtgl` AS `rspostingtgl`,`rs`.`rsisclose` AS `rsisclose`,`rs`.`rscustomtext1` AS `rscustomtext1`,`rs`.`rscustomtext2` AS `rscustomtext2`,`rs`.`rscustomtext3` AS `rscustomtext3`,`rs`.`rscustomtext4` AS `rscustomtext4`,`rs`.`rscustomtext5` AS `rscustomtext5`,`rs`.`rscustomint1` AS `rscustomint1`,`rs`.`rscustomint2` AS `rscustomint2`,`rs`.`rscustomint3` AS `rscustomint3`,`rs`.`rscustomdbl1` AS `rscustomdbl1`,`rs`.`rscustomdbl2` AS `rscustomdbl2`,`rs`.`rscustomdbl3` AS `rscustomdbl3`,`rs`.`rscustomdate1` AS `rscustomdate1`,`rs`.`rscustomdate2` AS `rscustomdate2`,`rs`.`rscustomdate3` AS `rscustomdate3`,`br`.`bnama` AS `rscabangnama`,`lc`.`lnama` AS `rslokasinama`,`wh1`.`wnama` AS `rsgudangasalnama`,`wh2`.`wnama` AS `rsgudangtransitnama`,`wh3`.`wnama` AS `rsgudangtujuannama`,`c1`.`kkode` AS `rsbagianterimakode`,`c1`.`knama` AS `rsbagianterimanama`,`mr`.`mrnotransaksi` AS `rsmrnotransaksi`,`ts`.`tsnotransaksi` AS `rstsnotransaksi`,`st1`.`nama` AS `rsstatusnama`,`st2`.`nama` AS `rsstatussebelumnyanama`,`u1`.`unama` AS `rsinputusernama`,`u2`.`unama` AS `rsmodifikasiusernama`,`rsd`.`idrsdetail` AS `idrsdetail`,`rsd`.`idrs` AS `idrs`,`rsd`.`idbarang` AS `idbarang`,`rsd`.`namabarang` AS `namabarang`,`rsd`.`tipebarang` AS `tipebarang`,`rsd`.`jml` AS `jml`,`rsd`.`satuan` AS `satuan`,`rsd`.`nilaisatuan` AS `nilaisatuan`,`rsd`.`jmlbarang` AS `jmlbarang`,`rsd`.`satuanbarang` AS `satuanbarang`,`rsd`.`cabang` AS `cabang`,`rsd`.`lokasi` AS `lokasi`,`rsd`.`gudangasal` AS `gudangasal`,`rsd`.`gudangtransit` AS `gudangtransit`,`rsd`.`gudangtujuan` AS `gudangtujuan`,`rsd`.`costcenter` AS `costcenter`,`rsd`.`divisi` AS `divisi`,`rsd`.`subdivisi` AS `subdivisi`,`rsd`.`proyek` AS `proyek`,`rsd`.`catatan` AS `catatan`,`rsd`.`urutan` AS `urutan`,`rsd`.`idmrdetail` AS `idmrdetail`,`rsd`.`idtsdetail` AS `idtsdetail`,`rsd`.`isclose` AS `isclose`,`rsd`.`customtext1` AS `customtext1`,`rsd`.`customtext2` AS `customtext2`,`rsd`.`customtext3` AS `customtext3`,`rsd`.`customdbl1` AS `customdbl1`,`rsd`.`customdbl2` AS `customdbl2`,`rsd`.`customdbl3` AS `customdbl3`,`rsd`.`customdate1` AS `customdate1`,`rsd`.`customdate2` AS `customdate2`,`rsd`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bhppaverage` AS `bhppaverage`,`i`.`bjenis` AS `bjenis`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`i`.`basset` AS `basset`,`brd`.`bnama` AS `cabangnama`,`lcd`.`lnama` AS `lokasinama`,`whd1`.`wnama` AS `gudangasalnama`,`whd2`.`wnama` AS `gudangtransitnama`,`whd2`.`wnama` AS `gudangtujuannama`,`cc`.`ccnama` AS `costcenternama`,`d`.`dnama` AS `divisinama`,`sd`.`sdnama` AS `subdivisinama`,`p`.`pnama` AS `proyeknama`,`mr2`.`mrnotransaksi` AS `mrnotransaksi`,`ts2`.`tsnotransaksi` AS `tsnotransaksi`, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan from (((((((((((((((((((((((((((`m3_rs` `rs` join `m3_rs_detail` `rsd` on((`rsd`.`idrs` = `rs`.`rsid`))) left join `m1_branch` `br` on((`br`.`bkode` = `rs`.`rscabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `rs`.`rslokasi`))) left join `m1_warehouse` `wh1` on((`wh1`.`wkode` = `rs`.`rsgudangasal`))) left join `m1_warehouse` `wh2` on((`wh2`.`wkode` = `rs`.`rsgudangtransit`))) left join `m1_warehouse` `wh3` on((`wh3`.`wkode` = `rs`.`rsgudangtujuan`))) left join `m1_contact` `c1` on((`c1`.`kid` = `rs`.`rsbagianterima`))) left join `m3_mr` `mr` on((`mr`.`mrid` = `rs`.`rsidmr`))) left join `m3_ts` `ts` on((`ts`.`tsid` = `rs`.`rsidts`))) left join `m0_status` `st1` on((`st1`.`kode` = `rs`.`rsstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `rs`.`rsstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `rs`.`rsinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `rs`.`rsmodifikasiuser`))) left join `m1_item` `i` on((`i`.`bid` = `rsd`.`idbarang`))) left join `m1_branch` `brd` on((`rsd`.`cabang` = `brd`.`bkode`))) left join `m1_location` `lcd` on((`rsd`.`lokasi` = `lcd`.`lkode`))) left join `m1_warehouse` `whd1` on((`rsd`.`gudangasal` = `whd1`.`wkode`))) left join `m1_warehouse` `whd2` on((`rsd`.`gudangtransit` = `whd2`.`wkode`))) left join `m1_warehouse` `whd3` on((`rsd`.`gudangtujuan` = `whd3`.`wkode`))) left join `m1_cost_center` `cc` on((`rsd`.`costcenter` = `cc`.`cckode`))) left join `m1_division` `d` on((`rsd`.`divisi` = `d`.`dkode`))) left join `m1_subdivision` `sd` on((`rsd`.`subdivisi` = `sd`.`sdkode`))) left join `m1_project` `p` on((`rsd`.`proyek` = `p`.`pkode`))) left join `m3_mr_detail` `mrd` on((`rsd`.`idmrdetail` = `mrd`.`idmrdetail`))) left join `m3_mr` `mr2` on((`mrd`.`idmr` = `mr2`.`mrid`))) left join `m3_ts_detail` `tsd` on((`rsd`.`idtsdetail` = `tsd`.`idtsdetail`))) left join `m3_ts` `ts2` on((`tsd`.`idts` = `ts2`.`tsid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("rsid"), 0), sptField,
                     FxDB(drutama("rscabang"), ""), sptField,
                     FxDB(drutama("rslokasi"), ""), sptField,
                     FxDB(drutama("rsgudangasal"), ""), sptField,
                     FxDB(drutama("rsgudangtransit"), ""), sptField,
                     FxDB(drutama("rsgudangtujuan"), ""), sptField,
                     FxDB(drutama("rssumber"), ""), sptField,
                     FxDB(drutama("rsautonotransaksi"), 0), sptField,
                     FxDB(drutama("rsnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rstgl"), ""), formatTgl), sptField,
                     FxDB(drutama("rskodepa"), 0), sptField,
                     FxDB(drutama("rsbagianterima"), 0), sptField,
                     FxDB(drutama("rsbagianterimakontak"), ""), sptField,
                     FxDB(drutama("rsuraian"), ""), sptField,
                     FxDB(drutama("rscatatan"), ""), sptField,
                     FxDB(drutama("rsnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rstglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("rsidmr"), 0), sptField,
                     FxDB(drutama("rsidts"), 0), sptField,
                     FxDB(drutama("rsstatus"), 0), sptField,
                     FxDB(drutama("rsstatussebelumnya"), 0), sptField,
                     FxDB(drutama("rsjmlrevisi"), 0), sptField,
                     FxDB(drutama("rscetakanke"), 0), sptField,
                     FxDB(drutama("rsinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rsinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rsmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rsmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rsposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rspostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rsisclose"), 0), sptField,
                     FxDB(drutama("rscustomtext1"), ""), sptField,
                     FxDB(drutama("rscustomtext2"), ""), sptField,
                     FxDB(drutama("rscustomtext3"), ""), sptField,
                     FxDB(drutama("rscustomtext4"), ""), sptField,
                     FxDB(drutama("rscustomtext5"), ""), sptField,
                     FxDB(drutama("rscustomint1"), 0), sptField,
                     FxDB(drutama("rscustomint2"), 0), sptField,
                     FxDB(drutama("rscustomint3"), 0), sptField,
                     FxDB(drutama("rscustomdbl1"), 0), sptField,
                     FxDB(drutama("rscustomdbl2"), 0), sptField,
                     FxDB(drutama("rscustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rscustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rscustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rscustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("rscabangnama"), ""), sptField,
                     FxDB(drutama("rslokasinama"), ""), sptField,
                     FxDB(drutama("rsgudangasalnama"), ""), sptField,
                     FxDB(drutama("rsgudangtransitnama"), ""), sptField,
                     FxDB(drutama("rsgudangtujuannama"), ""), sptField,
                     FxDB(drutama("rsbagianterimakode"), ""), sptField,
                     FxDB(drutama("rsbagianterimanama"), ""), sptField,
                     FxDB(drutama("rsmrnotransaksi"), ""), sptField,
                     FxDB(drutama("rstsnotransaksi"), ""), sptField,
                     FxDB(drutama("rsstatusnama"), ""), sptField,
                     FxDB(drutama("rsstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("rsinputusernama"), ""), sptField,
                     FxDB(drutama("rsmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idrsdetail"), 0), sptField,
                     FxDB(dr("idrs"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudangasal"), ""), sptField,
                     FxDB(dr("gudangtransit"), ""), sptField,
                     FxDB(dr("gudangtujuan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idmrdetail"), 0), sptField,
                     FxDB(dr("idtsdetail"), 0), sptField,
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
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bhppaverage"), 0), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("basset"), 0), sptField,
                     FxDB(dr("cabangnama"), ""), sptField,
                     FxDB(dr("lokasinama"), ""), sptField,
                     FxDB(dr("gudangasalnama"), ""), sptField,
                     FxDB(dr("gudangtransitnama"), ""), sptField,
                     FxDB(dr("gudangtujuannama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("mrnotransaksi"), ""), sptField,
                     FxDB(dr("tsnotransaksi"), ""), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptRow)

            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA BATCH
            sql = "select `nbt`.`nbtid` AS `nbtid`,`nbt`.`nbtjenismutasi` AS `nbtjenismutasi`,`nbt`.`nbtidbatchin` AS `nbtidbatchin`,`nbt`.`nbtgudang` AS `nbtgudang`,`nbt`.`nbtidbarang` AS `nbtidbarang`,`nbt`.`nbtkode` AS `nbtkode`,`nbt`.`nbtsumber` AS `nbtsumber`,`nbt`.`nbtidtransaksi` AS `nbtidtransaksi`,`nbt`.`nbtsatuan` AS `nbtsatuan`,`nbt`.`nbtjml` AS `nbtjml`,`nbt`.`nbtcustomtext1` AS `nbtcustomtext1`,`nbt`.`nbtcustomtext2` AS `nbtcustomtext2`,`nbt`.`nbtcustomtext3` AS `nbtcustomtext3`,`nbt`.`nbtcustomdbl1` AS `nbtcustomdbl1`,`nbt`.`nbtcustomdbl2` AS `nbtcustomdbl2`,`nbt`.`nbtcustomdbl3` AS `nbtcustomdbl3`,`nbt`.`nbtcustomdate1` AS `nbtcustomdate1`,`nbt`.`nbtcustomdate2` AS `nbtcustomdate2`,`nbt`.`nbtcustomdate3` AS `nbtcustomdate3`,`i`.`bkode` AS `kodebarang`, nbi.nbinotransaksi from ((`m1_no_batch_transaction` `nbt` join `m1_item` `i` on((`nbt`.`nbtidbarang` = `i`.`bid`))) left join `m1_no_batch_in` `nbi` on((`nbt`.`nbtidbatchin` = `nbi`.`nbiidbatchin`)))"
            Dim dtbatch As New DataTable
            dtbatch = AmbilData("aplikasi1-m1_no_batch_out", "nbtidtransaksi = '" & idtransaksi & "' AND nbtsumber = '" & sumber & "' AND (nbtjenismutasi = 1 OR nbiidbarang IS NOT NULL)", "nbtidbarang, nbtkode ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtbatch.Rows
                batch = String.Concat(batch,
                     FxDB(dr("nbtid"), 0), sptField,
                     FxDB(dr("nbtjenismutasi"), 0), sptField,
                     FxDB(dr("nbtidbatchin"), 0), sptField,
                     FxDB(dr("nbtgudang"), ""), sptField,
                     FxDB(dr("nbtidbarang"), 0), sptField,
                     FxDB(dr("nbtkode"), ""), sptField,
                     FxDB(dr("nbtsumber"), ""), sptField,
                     FxDB(dr("nbtidtransaksi"), 0), sptField,
                     FxDB(dr("nbtsatuan"), ""), sptField,
                     FxDB(dr("nbtjml"), 0), sptField,
                     FxDB(dr("nbtcustomtext1"), ""), sptField,
                     FxDB(dr("nbtcustomtext2"), ""), sptField,
                     FxDB(dr("nbtcustomtext3"), ""), sptField,
                     FxDB(dr("nbtcustomdbl1"), 0), sptField,
                     FxDB(dr("nbtcustomdbl2"), 0), sptField,
                     FxDB(dr("nbtcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("nbtcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("nbtcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("nbtcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("nbinotransaksi"), ""), sptRow)
            Next
            If batch.Length > 0 Then batch = batch.Substring(0, batch.Length - sptRow.Length) Else batch = batch

            'AMBIL DATA SERIAL
            sql = "select `nst`.`nstid` AS `nstid`,`nst`.`nstjenismutasi` AS `nstjenismutasi`,`nst`.`nstidserialin` AS `nstidserialin`,`nst`.`nstgudang` AS `nstgudang`,`nst`.`nstidbarang` AS `nstidbarang`,`nst`.`nstkode` AS `nstkode`,`nst`.`nstsumber` AS `nstsumber`,`nst`.`nstidtransaksi` AS `nstidtransaksi`,`nst`.`nstsatuan` AS `nstsatuan`,`nst`.`nstjml` AS `nstjml`,`nst`.`nstcustomtext1` AS `nstcustomtext1`,`nst`.`nstcustomtext2` AS `nstcustomtext2`,`nst`.`nstcustomtext3` AS `nstcustomtext3`,`nst`.`nstcustomdbl1` AS `nstcustomdbl1`,`nst`.`nstcustomdbl2` AS `nstcustomdbl2`,`nst`.`nstcustomdbl3` AS `nstcustomdbl3`,`nst`.`nstcustomdate1` AS `nstcustomdate1`,`nst`.`nstcustomdate2` AS `nstcustomdate2`,`nst`.`nstcustomdate3` AS `nstcustomdate3`,`i`.`bkode` AS `kodebarang`, nsi.nsinotransaksi from ((`m1_no_serial_transaction` `nst` join `m1_item` `i` on((`nst`.`nstidbarang` = `i`.`bid`))) left join `m1_no_serial_in` `nsi` on((`nst`.`nstidserialin` = `nsi`.`nsiidserialin`)))"
            Dim dtserial As New DataTable
            dtserial = AmbilData("aplikasi1-m1_no_serial_out", "nstidtransaksi = '" & idtransaksi & "' AND nstsumber = '" & sumber & "' AND (nstjenismutasi = 1 OR nsiidbarang IS NOT NULL)", "nstidbarang, nstkode ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtserial.Rows
                serial = String.Concat(serial,
                     FxDB(dr("nstid"), 0), sptField,
                     FxDB(dr("nstjenismutasi"), 0), sptField,
                     FxDB(dr("nstidserialin"), 0), sptField,
                     FxDB(dr("nstgudang"), ""), sptField,
                     FxDB(dr("nstidbarang"), 0), sptField,
                     FxDB(dr("nstkode"), ""), sptField,
                     FxDB(dr("nstsumber"), ""), sptField,
                     FxDB(dr("nstidtransaksi"), 0), sptField,
                     FxDB(dr("nstsatuan"), ""), sptField,
                     FxDB(dr("nstjml"), 0), sptField,
                     FxDB(dr("nstcustomtext1"), ""), sptField,
                     FxDB(dr("nstcustomtext2"), ""), sptField,
                     FxDB(dr("nstcustomtext3"), ""), sptField,
                     FxDB(dr("nstcustomdbl1"), 0), sptField,
                     FxDB(dr("nstcustomdbl2"), 0), sptField,
                     FxDB(dr("nstcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("nstcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("nstcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("nstcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("nsinotransaksi"), ""), sptRow)
            Next
            If serial.Length > 0 Then serial = serial.Substring(0, serial.Length - sptRow.Length) Else serial = serial


            'AMBIL DATA ASSET
            sql = "select atr.atid AS atid, atr.atasetid AS atasetid, atr.atjenismutasi AS atjenismutasi, atr.atsumber AS atsumber, atr.atidutama AS atidutama,atr.atidbarang AS atidbarang,atr.atkode AS atkode, atr.atnama AS atnama, atr.atkategori AS atkategori, atr.atcabang AS atcabang, atr.atlokasi AS atlokasi, atr.atgudang AS atgudang,atr.atdivisi AS atdivisi, atr.atsubdivisi AS atsubdivisi, atr.atcostcenter AS atcostcenter, atr.atproyek AS atproyek, atr.atcatatan AS atcatatan, atr.atnomor AS atnomor, atr.attglbeli AS attglbeli, atr.attglpakai AS attglpakai, atr.atjml AS atjml, atr.atsatuan AS atsatuan, atr.atmatauang AS atmatauang, atr.atkurs AS atkurs, atr.atharga AS atharga, atr.atdiskon AS atdiskon, atr.atjmldiskon AS atjmldiskon, atr.atpajak1 AS atpajak1, atr.atjmlpajak1 AS atjmlpajak1, atr.atpajak2 AS atpajak2, atr.atjmlpajak2 AS atjmlpajak2, atr.athargabeli AS athargabeli, atr.atnilairesidu AS atnilairesidu, atr.atumurekonomis AS atumurekonomis, atr.atbebanperbln AS atbebanperbln, atr.atakumulasibeban AS atakumulasibeban, atr.atnilaibuku AS atnilaibuku, (CASE WHEN atr.atnilaibuku < atr.atbebanperbln THEN atr.atnilaibuku ELSE atr.atbebanperbln END) as atnilaipenyusutan, atr.atmetode AS atmetode, atr.attabelpenyusutan AS attabelpenyusutan, atr.atintangible AS atintangible, atr.atfiskal AS atfiskal, atr.atatastengahbulan AS atatastengahbulan, atr.atrekasset AS atrekasset, atr.atrekakumdepresiasi AS atrekakumdepresiasi, atr.atrekdepresiasi AS atrekdepresiasi, atr.atrekpenghapusan AS atrekpenghapusan, atr.atprodusen AS atprodusen, atr.attglpensiun AS attglpensiun, atr.atpenyusutanke AS atpenyusutanke, atr.atnilaimenurun AS atnilaimenurun, atr.atdispose AS atdispose, atr.atpembelian AS atpembelian, atr.atpenjualan AS atpenjualan, atr.atlocked AS atlocked, atr.atstatus AS atstatus, atr.atstatussebelumnya AS atstatussebelumnya, atr.atisclose AS atisclose, atr.atinputuser AS atinputuser, atr.atinputtgl AS atinputtgl, atr.atmodifikasiuser AS atmodifikasiuser, atr.atmodifikasitgl AS atmodifikasitgl, atr.atcustomtext1,atr.atcustomtext2,atr.atcustomtext3,atr.atcustomtext4,atr.atcustomtext5,atr.atcustomint1,atr.atcustomint2,atr.atcustomint3,atr.atcustomint4,atr.atcustomint5,atr.atcustomdbl1,atr.atcustomdbl2,atr.atcustomdbl3,atr.atcustomdbl4,atr.atcustomdbl5,atr.atcustomdate1,atr.atcustomdate2,atr.atcustomdate3,atr.atcustomdate4,atr.atcustomdate5,ac.acnama AS atkategorinama, br.bnama AS atcabangnama, l.lnama AS atlokasinama, w.wnama AS atgudangnama,d.dnama AS atdivisinama, sd.sdnama AS atsubdivisinama, cc.ccnama AS atcostcenternama, p.pnama AS atproyeknama, dc.nama AS atmetodenama, t1.tnama AS atpajak1nama, ifnull(t1.tnilai, 0) AS atpajak1nilai, t2.tnama AS atpajak2nama, ifnull(t2.tnilai, 0) AS atpajak2nilai,coa1.cnama AS atrekassetnama, coa2.cnama AS atrekakumdepresiasinama, coa3.cnama AS atrekdepresiasinama, coa4.cnama AS atrekpenghapusannama, c1.kkode AS atprodusenkode, c1.knama AS atprodusennama,  sp1.nama AS atstatusnama,  sp2.nama AS atstatussebelumnyanama,  u1.unama AS atinputusernama,  u2.unama AS atmodifikasiusernama from m7_asset_transaction atr left join m7_asset_category ac on atr.atkategori = ac.ackode left join m1_branch br on atr.atcabang = br.bkode left join m1_location l on atr.atlokasi = l.lkode left join m1_warehouse w on atr.atgudang = w.wkode left join m1_division d on atr.atdivisi = d.dkode left join m1_subdivision sd on atr.atsubdivisi = sd.sdkode left join m7_depreciation_category dc on atr.atmetode = dc.kode left join m1_coa coa1 on atr.atrekasset = coa1.cnomor left join m1_coa coa2 on atr.atrekakumdepresiasi = coa2.cnomor left join m1_coa coa3 on atr.atrekdepresiasi = coa3.cnomor left join m1_coa coa4 on atr.atrekpenghapusan = coa4.cnomor left join m1_contact c1 on atr.atprodusen = c1.kid left join m0_status_progress sp1 on atr.atstatus = sp1.kode left join m0_status_progress sp2 on atr.atstatussebelumnya = sp2.kode left join m0_user u1 on atr.atinputuser = u1.userid left join m0_user u2 on atr.atmodifikasiuser = u2.userid left join m1_cost_center cc on atr.atcostcenter = cc.cckode left join m1_project p on atr.atproyek = p.pkode left join m1_tax t1 on atr.atpajak1 = t1.tkode left join m1_tax t2 on atr.atpajak2 = t2.tkode"
            Dim dtasset As New DataTable
            dtasset = AmbilData("aplikasi1-asset", "atidutama = '" & idtransaksi & "' AND atsumber = '" & sumber & "'", "atidbarang, atkode ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtasset.Rows
                asset = String.Concat(asset,
                     FxDB(dr("atid"), ""), sptField,
                     FxDB(dr("atasetid"), ""), sptField,
                     FxDB(dr("atjenismutasi"), 0), sptField,
                     FxDB(dr("atsumber"), ""), sptField,
                     FxDB(dr("atidutama"), ""), sptField,
                     FxDB(dr("atidbarang"), ""), sptField,
                     FxDB(dr("atkode"), ""), sptField,
                     FxDB(dr("atnama"), ""), sptField,
                     FxDB(dr("atkategori"), ""), sptField,
                     FxDB(dr("atcabang"), ""), sptField,
                     FxDB(dr("atlokasi"), ""), sptField,
                     FxDB(dr("atgudang"), ""), sptField,
                     FxDB(dr("atdivisi"), ""), sptField,
                     FxDB(dr("atsubdivisi"), ""), sptField,
                     FxDB(dr("atcostcenter"), ""), sptField,
                     FxDB(dr("atproyek"), ""), sptField,
                     FxDB(dr("atcatatan"), ""), sptField,
                     FxDB(dr("atnomor"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("attglbeli"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("attglpakai"), ""), formatTgl), sptField,
                     FxDB(dr("atjml"), 0), sptField,
                     FxDB(dr("atsatuan"), ""), sptField,
                     FxDB(dr("atmatauang"), ""), sptField,
                     FxDB(dr("atkurs"), 0), sptField,
                     FxDB(dr("atharga"), 0), sptField,
                     FxDB(dr("atdiskon"), ""), sptField,
                     FxDB(dr("atjmldiskon"), 0), sptField,
                     FxDB(dr("atpajak1"), ""), sptField,
                     FxDB(dr("atjmlpajak1"), 0), sptField,
                     FxDB(dr("atpajak2"), ""), sptField,
                     FxDB(dr("atjmlpajak2"), 0), sptField,
                     FxDB(dr("athargabeli"), 0), sptField,
                     FxDB(dr("atnilairesidu"), 0), sptField,
                     FxDB(dr("atumurekonomis"), 0), sptField,
                     FxDB(dr("atbebanperbln"), 0), sptField,
                     FxDB(dr("atakumulasibeban"), 0), sptField,
                     FxDB(dr("atnilaibuku"), 0), sptField,
                     FxDB(dr("atnilaipenyusutan"), 0), sptField,
                     FxDB(dr("atmetode"), 0), sptField,
                     FxDB(dr("attabelpenyusutan"), ""), sptField,
                     FxDB(dr("atintangible"), 0), sptField,
                     FxDB(dr("atfiskal"), 0), sptField,
                     FxDB(dr("atatastengahbulan"), 0), sptField,
                     FxDB(dr("atrekasset"), ""), sptField,
                     FxDB(dr("atrekakumdepresiasi"), ""), sptField,
                     FxDB(dr("atrekdepresiasi"), ""), sptField,
                     FxDB(dr("atrekpenghapusan"), ""), sptField,
                     FxDB(dr("atprodusen"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("attglpensiun"), ""), formatTgl), sptField,
                     FxDB(dr("atpenyusutanke"), 0), sptField,
                     FxDB(dr("atnilaimenurun"), 0), sptField,
                     FxDB(dr("atdispose"), 0), sptField,
                     FxDB(dr("atpembelian"), 0), sptField,
                     FxDB(dr("atpenjualan"), 0), sptField,
                     FxDB(dr("atlocked"), 0), sptField,
                     FxDB(dr("atstatus"), 0), sptField,
                     FxDB(dr("atstatussebelumnya"), 0), sptField,
                     FxDB(dr("atisclose"), 0), sptField,
                     FxDB(dr("atinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("atinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("atmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("atmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("atcustomtext1"), ""), sptField,
                     FxDB(dr("atcustomtext2"), ""), sptField,
                     FxDB(dr("atcustomtext3"), ""), sptField,
                     FxDB(dr("atcustomtext4"), ""), sptField,
                     FxDB(dr("atcustomtext5"), ""), sptField,
                     FxDB(dr("atcustomint1"), 0), sptField,
                     FxDB(dr("atcustomint2"), 0), sptField,
                     FxDB(dr("atcustomint3"), 0), sptField,
                     FxDB(dr("atcustomint4"), 0), sptField,
                     FxDB(dr("atcustomint5"), 0), sptField,
                     FxDB(dr("atcustomdbl1"), 0), sptField,
                     FxDB(dr("atcustomdbl2"), 0), sptField,
                     FxDB(dr("atcustomdbl3"), 0), sptField,
                     FxDB(dr("atcustomdbl4"), 0), sptField,
                     FxDB(dr("atcustomdbl5"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("atcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("atcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("atcustomdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("atcustomdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("atcustomdate5"), ""), formatTgl), sptField,
                     FxDB(dr("atkategorinama"), ""), sptField,
                     FxDB(dr("atcabangnama"), ""), sptField,
                     FxDB(dr("atlokasinama"), ""), sptField,
                     FxDB(dr("atgudangnama"), ""), sptField,
                     FxDB(dr("atdivisinama"), ""), sptField,
                     FxDB(dr("atsubdivisinama"), ""), sptField,
                     FxDB(dr("atcostcenternama"), ""), sptField,
                     FxDB(dr("atproyeknama"), ""), sptField,
                     FxDB(dr("atmetodenama"), ""), sptField,
                     FxDB(dr("atpajak1nama"), ""), sptField,
                     FxDB(dr("atpajak1nilai"), 0), sptField,
                     FxDB(dr("atpajak2nama"), ""), sptField,
                     FxDB(dr("atpajak2nilai"), 0), sptField,
                     FxDB(dr("atrekassetnama"), ""), sptField,
                     FxDB(dr("atrekakumdepresiasinama"), ""), sptField,
                     FxDB(dr("atrekdepresiasinama"), ""), sptField,
                     FxDB(dr("atrekpenghapusannama"), ""), sptField,
                     FxDB(dr("atprodusenkode"), ""), sptField,
                     FxDB(dr("atprodusennama"), ""), sptField,
                     FxDB(dr("atstatusnama"), ""), sptField,
                     FxDB(dr("atstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("atinputusernama"), ""), sptField,
                     FxDB(dr("atmodifikasiusernama"), ""), sptRow)
            Next
            If asset.Length > 0 Then asset = asset.Substring(0, asset.Length - sptRow.Length) Else asset = asset

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "RS transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, batch, sptSubParam, serial, sptSubParam, asset)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rsid, rscabang, rslokasi, rsgudangasal, rsgudangtransit, rsgudangtujuan, rssumber, rsautonotransaksi, rsnotransaksi, rstgl, rskodepa, rsbagianterima, rsbagianterimakontak, rsuraian, rscatatan, rsnoref, rstglnoref, rsidmr, rsidts, rsstatus, rsstatussebelumnya, rsjmlrevisi, rscetakanke, rsinputuser, rsinputtgl, rsmodifikasiuser, rsmodifikasitgl, rsposting, rspostingtgl, rsisclose, rscustomtext1, rscustomtext2, rscustomtext3, rscustomtext4, rscustomtext5, rscustomint1, rscustomint2, rscustomint3, rscustomdbl1, rscustomdbl2, rscustomdbl3, rscustomdate1, rscustomdate2, rscustomdate3, rscabangnama, rslokasinama, rsgudangasalnama, rsgudangtransitnama, rsgudangtujuannama, rsbagianterimakode, rsbagianterimanama, rsmrnotransaksi, rstsnotransaksi, rsstatusnama, rsstatussebelumnyanama, rsinputusernama, rsmodifikasiusernama" &
            sptSubParam & "idrsdetail, idrs, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idmrdetail, idtsdetail, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bhppaverage, bjenis, bserial, bbatch, basset, cabangnama, lokasinama, gudangasalnama, gudangtransitnama, gudangtujuannama, costcenternama, divisinama, subdivisinama, proyeknama, mrnotransaksi, tsnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan" &
            sptSubParam & "nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang, nbtnotransaksi" &
            sptSubParam & "nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang, nstnotransaksi" &
            sptSubParam & "atid, atasetid, atjenismutasi, atsumber, atidutama, atidbarang, atkode, atnama, atkategori, atcabang, atlokasi, atgudang, atdivisi, atsubdivisi, atcostcenter, atproyek, atcatatan, atnomor, attglbeli, attglpakai, atjml, atsatuan, atmatauang, atkurs, atharga, atdiskon, atjmldiskon, atpajak1, atjmlpajak1, atpajak2, atjmlpajak2, athargabeli, atnilairesidu, atumurekonomis, atbebanperbln, atakumulasibeban, atnilaibuku, atnilaipenyusutan, atmetode, attabelpenyusutan, atintangible, atfiskal, atatastengahbulan, atrekasset, atrekakumdepresiasi, atrekdepresiasi, atrekpenghapusan, atprodusen, attglpensiun, atpenyusutanke, atnilaimenurun, atdispose, atpembelian, atpenjualan, atlocked, atstatus, atstatussebelumnya, atisclose, atinputuser, atinputtgl, atmodifikasiuser, atmodifikasitgl, atcustomtext1, atcustomtext2, atcustomtext3, atcustomtext4, atcustomtext5, atcustomint1, atcustomint2, atcustomint3, atcustomint4, atcustomint5, atcustomdbl1, atcustomdbl2, atcustomdbl3, atcustomdbl4, atcustomdbl5, atcustomdate1, atcustomdate2, atcustomdate3, atcustomdate4, atcustomdate5, atkategorinama, atcabangnama, atlokasinama, atgudangnama, atdivisinama, atsubdivisinama, atcostcenternama, atproyeknama, atmetodenama, atpajak1nama, atpajak1nilai, atpajak2nama, atpajak2nilai, atrekassetnama, atrekakumdepresiasinama, atrekdepresiasinama, atrekpenghapusannama, atprodusenkode, atprodusennama, atstatusnama, atstatussebelumnyanama, atinputusernama, atmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_RsSearch(ByVal param As String) As String
        'M3_RsSearch --------------------------------------------------------
        'rsid, rscabang, rslokasi, rsgudangasal, rsgudangtransit, rsgudangtujuan, rssumber, 
        'rsautonotransaksi, rsnotransaksi, rstgl, rskodepa, rsbagianterima, rsbagianterimakontak, rsuraian, 
        'rscatatan, rsnoref, rstglnoref, rsidmr, rsidts, rsstatus, rsstatussebelumnya, 
        'rsjmlrevisi, rscetakanke, rsinputuser, rsinputtgl, rsmodifikasiuser, rsmodifikasitgl, rsposting, 
        'rspostingtgl, rsisclose, rscabangnama, rslokasinama, rsgudangasalnama, rsgudangtransitnama, rsgudangtujuannama, 
        'rsbagianterimakode, rsbagianterimanama, mrnotransaksi, tsnotransaksi, rsstatusnama, rsstatussebelumnyanama, rsinputusernama, 
        'rsmodifikasiusernama

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
            Filter = Filter.Replace("rsbagianterimakode", "c1.kkode")
            Filter = Filter.Replace("rsbagianterimanama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m3_rs_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M3_Rs", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rsid"), 0), sptField,
                     FxDB(dr("rscabang"), ""), sptField,
                     FxDB(dr("rslokasi"), ""), sptField,
                     FxDB(dr("rsgudangasal"), ""), sptField,
                     FxDB(dr("rsgudangtransit"), ""), sptField,
                     FxDB(dr("rsgudangtujuan"), ""), sptField,
                     FxDB(dr("rssumber"), ""), sptField,
                     FxDB(dr("rsautonotransaksi"), 0), sptField,
                     FxDB(dr("rsnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rstgl"), ""), formatTgl), sptField,
                     FxDB(dr("rskodepa"), 0), sptField,
                     FxDB(dr("rsbagianterima"), 0), sptField,
                     FxDB(dr("rsbagianterimakontak"), ""), sptField,
                     FxDB(dr("rsuraian"), ""), sptField,
                     FxDB(dr("rscatatan"), ""), sptField,
                     FxDB(dr("rsnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rstglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("rsidmr"), 0), sptField,
                     FxDB(dr("rsidts"), 0), sptField,
                     FxDB(dr("rsstatus"), 0), sptField,
                     FxDB(dr("rsstatussebelumnya"), 0), sptField,
                     FxDB(dr("rsjmlrevisi"), 0), sptField,
                     FxDB(dr("rscetakanke"), 0), sptField,
                     FxDB(dr("rsinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rsinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rsmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rsmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rsposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rspostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rsisclose"), 0), sptField,
                     FxDB(dr("rscabangnama"), ""), sptField,
                     FxDB(dr("rslokasinama"), ""), sptField,
                     FxDB(dr("rsgudangasalnama"), ""), sptField,
                     FxDB(dr("rsgudangtransitnama"), ""), sptField,
                     FxDB(dr("rsgudangtujuannama"), ""), sptField,
                     FxDB(dr("rsbagianterimakode"), ""), sptField,
                     FxDB(dr("rsbagianterimanama"), ""), sptField,
                     FxDB(dr("mrnotransaksi"), ""), sptField,
                     FxDB(dr("tsnotransaksi"), ""), sptField,
                     FxDB(dr("rsstatusnama"), ""), sptField,
                     FxDB(dr("rsstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("rsinputusernama"), ""), sptField,
                     FxDB(dr("rsmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rsid, rscabang, rslokasi, rsgudangasal, rsgudangtransit, rsgudangtujuan, rssumber, rsautonotransaksi, rsnotransaksi, rstgl, rskodepa, rsbagianterima, rsbagianterimakontak, rsuraian, rscatatan, rsnoref, rstglnoref, rsidmr, rsidts, rsstatus, rsstatussebelumnya, rsjmlrevisi, rscetakanke, rsinputuser, rsinputtgl, rsmodifikasiuser, rsmodifikasitgl, rsposting, rspostingtgl, rsisclose, rscabangnama, rslokasinama, rsgudangasalnama, rsgudangtransitnama, rsgudangtujuannama, rsbagianterimakode, rsbagianterimanama, mrnotransaksi, tsnotransaksi, rsstatusnama, rsstatussebelumnyanama, rsinputusernama, rsmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_RsTerkait(ByVal param As String) As String
        'M3_RsTerkait --------------------------------------------------------
        'rsid, rsnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
        'modifikasitglterkait, jenisterkait

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
        If ClsValidKey.ApaBisaAkses(1, 1, 3) = False Then
            result(2) = "Access denied for get data"
        End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        Dim idtransaksi As String = ""
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "tsid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2) & " AND rsid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "rsid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.m3_rs_terkait(Filter)

        'result(2) = sql & " ORDER BY " & Sorting : GoTo selesai

        dt = AmbilData("aplikasi1-M3_ts_Terkait", , "", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rsid"), 0), sptField,
                     FxDB(dr("rsnotransaksi"), ""), sptField,
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
            result(2) = "Related RS data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rsid, rsnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstanding As String, ByVal ftOutstanding As String, ByVal ftExistStok As String, ByVal ftStok As String, ByVal ftExistBatch As String, ByVal ftBatch As String, ByVal ftExistSerial As String, ByVal ftSerial As String, ByVal gudangBatchSerial As String) As String
        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaiSatuan As Double = 0, sisa As Double = 0
        Dim filterLookup As String = "", gudang As String = "", urutan As String = "", noBatch As String = "", noSerial As String = ""

        'VALIDASI OUTSTANDING ---------------------------------------
        If Len(ftExistOutstanding) > 0 Then 'ftExistOutstanding = rowExists, idtsdetail, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstanding)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "idtsdetail=" & dtval.Rows(0)("idtsdetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in TS" : GoTo selesai
            End If

            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
            sql = "SELECT tsd.idtsdetail, (tsd.jmlbarang - tsd.jmlrealisasi) as sisarealisasi, i.bid, i.bkode FROM m3_ts_detail AS tsd INNER JOIN m1_item AS i ON tsd.idbarang = i.bid WHERE " & ftOutstanding
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisarealisasi")

                filterLookup = "idtsdetail=" & dtval.Rows(0)("idtsdetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in TS, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If
        'END OF VALIDASI OUTSTANDING --------------------------------


        Dim ProsesValidasiStok As String = F_getSetting(0, "company", "ValidasiStok")
        If ProsesValidasiStok.Equals("0") = False Then
            'VALIDASI STOK ----------------------------------------------
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistStok) 'ftExistStok = rowExists, idbarang, bkode, gudang
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                gudang = dtval.Rows(0)("gudang")

                filterLookup = "idbarang=" & dtval.Rows(0)("idbarang")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in '" & gudang & "' warehouse" : GoTo selesai
            End If

            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA STOK PERGUDANG YG TERSEDIA
            'sql = "SELECT isw.idbarang, isw.kgudang, isw.stok - IFNULL(isb.jmlbooking,0) as stok, i.bkode FROM m1_item_stock_warehouse isw JOIN m1_item i ON isw.idbarang = i.bid AND i.bjenis <> 'J' LEFT JOIN m1_item_booking isb ON isw.idbarang = isb.idbarang WHERE " & ftStok
            sql = "SELECT isw.idbarang, isw.kgudang, isw.stok - IFNULL(isb.jmlbooking,0) as stok, i.bkode FROM m1_item_stock_warehouse isw JOIN m1_item i ON isw.idbarang = i.bid AND i.bjenis <> 'J' LEFT JOIN m1_warehouse w ON isw.kgudang = w.wkode LEFT JOIN m1_item_booking isb ON isw.idbarang = isb.idbarang AND w.wbookingstok = 1 WHERE " & ftStok
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("stok")
                gudang = dtval.Rows(0)("kgudang")

                filterLookup = "idbarang=" & dtval.Rows(0)("idbarang")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of stock in '" & gudang & "' warehouse, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
            'END OF VALIDASI STOK ---------------------------------------
        End If



        'VALIDASI BATCH ---------------------------------------------
        'CEK DATA EXIST/TIDAK
        If Len(ftExistBatch) > 0 Then
            dtval = AsDataTableAmbilDariDB(ftExistBatch) 'ftExistBatch = rowExists, idbarang, bkode, nbikode, nbigudang
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                noBatch = dtval.Rows(0)("nbikode")
                gudang = dtval.Rows(0)("nbigudang")

                filterLookup = "idbarang = " & dtval.Rows(0)("idbarang") & " AND " & gudangBatchSerial & " = '" & dtval.Rows(0)("nbigudang") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " | No. Batch : " & noBatch & " doesn't exists in No. Batch list." : GoTo selesai
            End If
        End If

        'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA BATCH YG TERSEDIA
        If Len(ftBatch) > 0 Then
            sql = "SELECT nbi.nbiidbarang, nbi.nbikode, nbi.nbigudang, nbi.nbijmlsisa, i.bkode FROM m1_no_batch_in nbi JOIN m1_item i ON nbi.nbiidbarang = i.bid WHERE " & ftBatch
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("nbijmlsisa")
                noBatch = dtval.Rows(0)("nbikode")
                gudang = dtval.Rows(0)("nbigudang")

                filterLookup = "idbarang = " & dtval.Rows(0)("nbiidbarang") & " AND " & gudangBatchSerial & " = '" & dtval.Rows(0)("nbigudang") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " | No. Batch : " & noBatch & " exceeds the number of stock in No. Batch list, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If
        'END OF VALIDASI BATCH --------------------------------------

        'VALIDASI SERIAL ---------------------------------------------
        'CEK DATA EXIST/TIDAK
        If Len(ftExistSerial) > 0 Then
            dtval = AsDataTableAmbilDariDB(ftExistSerial) 'ftExistSerial = rowExists, idbarang, bkode, nsikode, nsigudang
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                noSerial = dtval.Rows(0)("nsikode")
                gudang = dtval.Rows(0)("nsigudang")

                filterLookup = "idbarang = " & dtval.Rows(0)("idbarang") & " AND " & gudangBatchSerial & " = '" & dtval.Rows(0)("nsigudang") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " | No. Serial : " & noSerial & " doesn't exists in No. Serial list." : GoTo selesai
            End If
        End If

        'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA SERIAL YG TERSEDIA
        If Len(ftSerial) > 0 Then
            sql = "SELECT nsi.nsiidbarang, nsi.nsikode, nsi.nsigudang, nsi.nsijmlsisa, i.bkode FROM m1_no_serial_in nsi JOIN m1_item i ON nsi.nsiidbarang = i.bid WHERE " & ftSerial
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("nsijmlsisa")
                noSerial = dtval.Rows(0)("nsikode")
                gudang = dtval.Rows(0)("nsigudang")

                filterLookup = "idbarang = " & dtval.Rows(0)("nsiidbarang") & " AND " & gudangBatchSerial & " = '" & dtval.Rows(0)("nsigudang") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " | No. Serial : " & noSerial & " exceeds the number of stock in No. Serial list, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If
        'END OF VALIDASI SERIAL --------------------------------------

selesai:
        Return errmessage
    End Function

    <WebMethod()>
    Public Function M3_RsSimpanOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataBatch(), dataRowBatch(), dataSerial(), dataRowSerial() As String

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
        If (dataSplit.Length <> 4) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'rsid(0) As Integer, rscabang(1) As String, rslokasi(2) As String, rsgudangasal(3) As String, rsgudangtransit(4) As String, 
        'rsgudangtujuan(5) As String, rssumber(6) As String, rsautonotransaksi(7) As Integer, rsnotransaksi(8) As String, rstgl(9) As Date, 
        'rskodepa(10) As Integer, rsbagianterima(11) As Integer, rsbagianterimakontak(12) As String, rsuraian(13) As String, rscatatan(14) As String, 
        'rsnoref(15) As String, rstglnoref(16) As Date, rsidmr(17) As Integer, rsidts(18) As Integer, rsstatus(19) As Integer, 
        'rsstatussebelumnya(20) As Integer, rsjmlrevisi(21) As Integer, rscetakanke(22) As Integer, rsinputuser(23) As Integer, rsinputtgl(24) As DateTime, 
        'rsmodifikasiuser(25) As Integer, rsmodifikasitgl(26) As DateTime, rsisclose(27) As Integer, rscustomtext1(28) As String, rscustomtext2(29) As String, 
        'rscustomtext3(30) As String, rscustomtext4(31) As String, rscustomtext5(32) As String, rscustomint1(33) As Integer, rscustomint2(34) As Integer, 
        'rscustomint3(35) As Integer, rscustomdbl1(36) As Double, rscustomdbl2(37) As Double, rscustomdbl3(38) As Double, rscustomdate1(39) As Date, 
        'rscustomdate2(40) As Date, rscustomdate3(41) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'rsid, rscabang, rslokasi, rsgudangasal, rsgudangtransit, rsgudangtujuan, rssumber, 
        'rsautonotransaksi, rsnotransaksi, rstgl, rskodepa, rsbagianterima, rsbagianterimakontak, rsuraian, 
        'rscatatan, rsnoref, rstglnoref, rsidmr, rsidts, rsstatus, rsstatussebelumnya, 
        'rsjmlrevisi, rscetakanke, rsinputuser, rsinputtgl, rsmodifikasiuser, rsmodifikasitgl, rsisclose, 
        'rscustomtext1, rscustomtext2, rscustomtext3, rscustomtext4, rscustomtext5, rscustomint1, rscustomint2, 
        'rscustomint3, rscustomdbl1, rscustomdbl2, rscustomdbl3, rscustomdate1, rscustomdate2, rscustomdate3


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 42) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'rsid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "rsid required numeric." : GoTo selesai
        End If
        'rsautonotransaksi(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "rsautonotransaksi required numeric." : GoTo selesai
        End If
        'rstgl(9) As Date
        If (IsDate(dataUtama(9)) = False) Then
            result(2) = "rstgl required date." : GoTo selesai
        End If
        'rskodepa(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "rskodepa required numeric." : GoTo selesai
        End If
        'rsbagianterima(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "rsbagianterima required numeric." : GoTo selesai
        End If
        If (dataUtama(11) < 1) Then
            result(2) = "rsbagianterima can't be empty." : GoTo selesai
        End If
        'rstglnoref(16) As Date
        If (IsDate(dataUtama(16)) = False) Then
            result(2) = "rstglnoref required date." : GoTo selesai
        End If
        'rsidmr(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "rsidmr required numeric." : GoTo selesai
        End If
        'rsidts(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "rsidts required numeric." : GoTo selesai
        End If
        'rsstatus(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "rsstatus required numeric." : GoTo selesai
        End If
        'rsstatussebelumnya(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "rsstatussebelumnya required numeric." : GoTo selesai
        End If
        'rsjmlrevisi(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "rsjmlrevisi required numeric." : GoTo selesai
        End If
        'rscetakanke(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "rscetakanke required numeric." : GoTo selesai
        End If
        'rsinputuser(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "rsinputuser required numeric." : GoTo selesai
        End If
        'rsinputtgl(24) As DateTime
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "rsinputtgl required date." : GoTo selesai
        End If
        'rsmodifikasiuser(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "rsmodifikasiuser required numeric." : GoTo selesai
        End If
        'rsmodifikasitgl(26) As DateTime
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "rsmodifikasitgl required date." : GoTo selesai
        End If
        'rsisclose(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "rsisclose required numeric." : GoTo selesai
        End If
        'rscustomint1(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "rscustomint1 required numeric." : GoTo selesai
        End If
        'rscustomint2(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "rscustomint2 required numeric." : GoTo selesai
        End If
        'rscustomint3(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "rscustomint3 required numeric." : GoTo selesai
        End If
        'rscustomdbl1(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "rscustomdbl1 required numeric." : GoTo selesai
        End If
        'rscustomdbl2(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "rscustomdbl2 required numeric." : GoTo selesai
        End If
        'rscustomdbl3(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "rscustomdbl3 required numeric." : GoTo selesai
        End If
        'rscustomdate1(39) As Date
        If (IsDate(dataUtama(39)) = False) Then
            result(2) = "rscustomdate1 required date." : GoTo selesai
        End If
        'rscustomdate2(40) As Date
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "rscustomdate2 required date." : GoTo selesai
        End If
        'rscustomdate3(41) As Date
        If (IsDate(dataUtama(41)) = False) Then
            result(2) = "rscustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'rscabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "rscabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "rscabang should not be more than 25 character." : GoTo selesai
        End If

        'rslokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "rslokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "rslokasi should not be more than 25 character." : GoTo selesai
        End If

        'rssumber(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "rssumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 10 Then
            result(2) = "rssumber should not be more than 10 character." : GoTo selesai
        End If

        'rsnotransaksi(8) As String
        If Len(dataUtama(8)) = 0 Then
            result(2) = "rsnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 50 Then
            result(2) = "rsnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'rstgl(9) As Date
        If Len(dataUtama(9)) = 0 Then
            result(2) = "rstgl can't be empty" : GoTo selesai
        End If

        'rstglnoref(16) As Date
        If Len(dataUtama(16)) = 0 Then
            result(2) = "rstglnoref can't be empty" : GoTo selesai
        End If

        'rsinputtgl(24) As DateTime
        If Len(dataUtama(24)) = 0 Then
            result(2) = "rsinputtgl can't be empty" : GoTo selesai
        End If

        'rsmodifikasitgl(26) As DateTime
        If Len(dataUtama(26)) = 0 Then
            result(2) = "rsmodifikasitgl can't be empty" : GoTo selesai
        End If

        'rscustomdbl1(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "rscustomdbl1 can't be empty" : GoTo selesai
        End If

        'rscustomdbl2(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "rscustomdbl2 can't be empty" : GoTo selesai
        End If

        'rscustomdbl3(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "rscustomdbl3 can't be empty" : GoTo selesai
        End If

        'rscustomdate1(39) As Date
        If Len(dataUtama(39)) = 0 Then
            result(2) = "rscustomdate1 can't be empty" : GoTo selesai
        End If

        'rscustomdate2(40) As Date
        If Len(dataUtama(40)) = 0 Then
            result(2) = "rscustomdate2 can't be empty" : GoTo selesai
        End If

        'rscustomdate3(41) As Date
        If Len(dataUtama(41)) = 0 Then
            result(2) = "rscustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "rsid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rscabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rslokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rsgudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rsgudangtransit", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rsgudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rssumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rsautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rsnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rstgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rskodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rsbagianterima", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rsbagianterimakontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rsuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rscatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rsnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rstglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rsidmr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rsidts", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rsstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rsstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rsjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rscetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rsinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rsinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rsmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rsmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rsisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rscustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rscustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rscustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rscustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rscustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rscustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rscustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rscustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rscustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rscustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rscustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rscustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rscustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rscustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "rsid~rscabang~rslokasi~rsgudangasal~rsgudangtransit~rsgudangtujuan~rssumber~rsautonotransaksi~rsnotransaksi~rstgl~rskodepa~rsbagianterima~rsbagianterimakontak~rsuraian~rscatatan~rsnoref~rstglnoref~rsidmr~rsidts~rsstatus~rsstatussebelumnya~rsjmlrevisi~rscetakanke~rsinputuser~rsinputtgl~rsmodifikasiuser~rsmodifikasitgl~rsisclose~rscustomtext1~rscustomtext2~rscustomtext3~rscustomtext4~rscustomtext5~rscustomint1~rscustomint2~rscustomint3~rscustomdbl1~rscustomdbl2~rscustomdbl3~rscustomdate1~rscustomdate2~rscustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idrsdetail(0) As Integer, idrs(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'cabang(10) As String, lokasi(11) As String, gudangasal(12) As String, gudangtransit(13) As String, gudangtujuan(14) As String, 
        'costcenter(15) As String, divisi(16) As String, subdivisi(17) As String, proyek(18) As String, catatan(19) As String, 
        'urutan(20) As Integer, idmrdetail(21) As Integer, idtsdetail(22) As Integer, isclose(23) As Integer, customtext1(24) As String, 
        'customtext2(25) As String, customtext3(26) As String, customdbl1(27) As Double, customdbl2(28) As Double, customdbl3(29) As Double, 
        'customdate1(30) As Date, customdate2(31) As Date, customdate3(32) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idrsdetail, idrs, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, cabang, lokasi, gudangasal, gudangtransit, 
        'gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idmrdetail, idtsdetail, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idrsdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idrs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangtransit", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idmrdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idtsdetail", AsEnumTypeData.AsInt64)
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

        'Variabel ValidasiBatchSerial
        Dim ftBarang As String = ""

        'Variabel ValidasiSimpan
        Dim ftExistOutstanding As String = "", ftOutstanding As String = "", ftExistStok As String = "", ftStok As String = ""
        Dim updNilai As String = "", updFilter As String = "", updStokIn As String = "", updStokOut As String = "", updNilaiMR As String = "", updFilterMR As String = ""
        Dim gudangIn As String = "", gudangOut As String = ""
        Dim idbarang As Integer = 0, idtsdetail As Integer = 0, idmrdetail As Integer = 0, jmlbarang As Double = 0

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 33) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idrsdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idrsdetail required numeric." : GoTo selesai
            End If
            'idrs(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idrs required numeric." : GoTo selesai
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
            'urutan(20) As Integer
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idmrdetail(21) As Integer
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - idmrdetail required numeric." : GoTo selesai
            End If
            'idtsdetail(22) As Integer
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - idtsdetail required numeric." : GoTo selesai
            End If
            'isclose(23) As Integer
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(27) As Double
            If (IsNumeric(dataRowDetail(27)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(28) As Double
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(29) As Double
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(30) As Date
            If (IsDate(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(31) As Date
            If (IsDate(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(32) As Date
            If (IsDate(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'namabarang(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - namabarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 100 Then
                result(2) = "Row : " & i & " - namabarang should not be more than 100 character." : GoTo selesai
            End If

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

            'cabang(10) As String
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - cabang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(10)) > 25 Then
                result(2) = "Row : " & i & " - cabang should not be more than 25 character." : GoTo selesai
            End If

            'lokasi(11) As String
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - lokasi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(11)) > 25 Then
                result(2) = "Row : " & i & " - lokasi should not be more than 25 character." : GoTo selesai
            End If

            'gudangasal(12) As String
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - gudangasal can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(12)) > 25 Then
                result(2) = "Row : " & i & " - gudangasal should not be more than 25 character." : GoTo selesai
            End If

            'gudangtransit(13) As String
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - gudangtransit can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(13)) > 25 Then
                result(2) = "Row : " & i & " - gudangtransit should not be more than 25 character." : GoTo selesai
            End If

            'gudangtujuan(14) As String
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - gudangtujuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(14)) > 25 Then
                result(2) = "Row : " & i & " - gudangtujuan should not be more than 25 character." : GoTo selesai
            End If

            'customdbl1(27) As Double
            If Len(dataRowDetail(27)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(28) As Double
            If Len(dataRowDetail(28)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(29) As Double
            If Len(dataRowDetail(29)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(30) As Date
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(31) As Date
            If Len(dataRowDetail(31)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(32) As Date
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idrsdetail~idrs~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~cabang~lokasi~gudangasal~gudangtransit~gudangtujuan~costcenter~divisi~subdivisi~proyek~catatan~urutan~idmrdetail~idtsdetail~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , gudangtransit(13) As String   , gudangtujuan(14) As String   , idmrdetail(21) As Integer      , idtsdetail(22) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : gudangOut = dataRowDetail(13) : gudangIn = dataRowDetail(14) : idmrdetail = dataRowDetail(21) : idtsdetail = dataRowDetail(22)

            'ValidasiBatchSerial
            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")

            'VALIDASI OUTSTANDING -------------------------
            If idtsdetail <> 0 Then
                '1. CEK DATA EXIST
                ftExistOutstanding = IIf(Len(ftExistOutstanding.ToString) = 0, "", ftExistOutstanding & " UNION ")
                ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m3_ts_detail JOIN m3_ts ON idts = tsid WHERE idtsdetail = '" & idtsdetail & "' AND (tsstatus = 2 OR tsstatus = 3 OR tsstatus = 4 OR tsstatus = 7) LIMIT 1) as rowExists, '" & idtsdetail & "' as idtsdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
                'ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m3_ts_detail JOIN m3_ts ON idts = tsid WHERE idtsdetail = '" & idtsdetail & "' AND (tsstatus = 2 OR tsstatus = 3) LIMIT 1) as rowExists, '" & idtsdetail & "' as idtsdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idtsdetail=" & idtsdetail)
                ftOutstanding = IIf(Len(ftOutstanding.ToString) = 0, "", ftOutstanding & " OR ")
                ftOutstanding = String.Concat(ftOutstanding, " (tsd.idtsdetail = " & idtsdetail & " AND " & Outstanding & " > (tsd.jmlbarang - tsd.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING
                updNilai = String.Concat("WHEN '" & idtsdetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilai)

                '4. SET FILTER UPDATE OUTSTANDING
                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                updFilter = String.Concat(updFilter, "(idtsdetail = '" & idtsdetail & "')")
            End If

            If idmrdetail <> 0 Then
                '1. SET NILAI UPDATE OUTSTANDING MR
                Dim OutstandingMR As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idmrdetail=" & idmrdetail)
                updNilaiMR = String.Concat("WHEN '" & idmrdetail & "' THEN ROUND(jmlrealisasi + '" & OutstandingMR & "', 5) ", updNilaiMR)

                '2. SET FILTER UPDATE OUTSTANDING MR
                updFilterMR = IIf(Len(updFilterMR.ToString) = 0, "", updFilterMR & " OR ")
                updFilterMR = String.Concat(updFilterMR, "(idmrdetail = '" & idmrdetail & "')")
            End If

            'VALIDASI STOK -------------------------------
            '1. CEK DATA EXIST STOK KELUAR
            ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
            ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

            '2. CEK JML STOK KELUAR
            Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangtransit='" & gudangOut & "'")
            ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
            ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")

            '3. SET NILAI UPDATE STOK KELUAR
            updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
            updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

            '4. SET NILAI UPDATE STOK MASUK
            updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
            updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'MAPPING BUAT WS DATA BATCH -------------------------------------------------------
        'nbtid(0) As Integer, nbtjenismutasi(1) As Integer, nbtidbarang(2) As Integer, nbtkode(3) As String, nbtsumber(4) As String, 
        'nbtidtransaksi(5) As Integer, nbtsatuan(6) As String, nbtjml(7) As Double, nbtcustomtext1(8) As String, nbtcustomtext2(9) As String, 
        'nbtcustomtext3(10) As String, nbtcustomdbl1(11) As Double, nbtcustomdbl2(12) As Double, nbtcustomdbl3(13) As Double, nbtcustomdate1(14) As Date, 
        'nbtcustomdate2(15) As Date, nbtcustomdate3(16) As Date, nbtgudang(17) As String, nbtidbatchin(18) As Integer

        'MAPPING BUAT FLEX DATA BATCH -----------------------------------------------------
        'nbtid, nbtjenismutasi, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, nbtgudang, nbtidbatchin

        'Buat datatable BATCH
        Dim dtbatch As New DataTable
        AsDataTableTambahField(dtbatch, "nbtid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtjenismutasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbatch, "nbtidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbatch, "nbtkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtidtransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbatch, "nbtsatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtjml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtbatch, "nbtcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtidbatchin", AsEnumTypeData.AsInt64)

        'ValidasiSimpan
        Dim ftExistBatch As String = "", ftBatch As String = ""
        Dim nbtkode As String = "", nbtgudang As String = "", nbtidbatchin As Integer = 0
        Dim updNilaiBatch As String = "", updFilterBatch As String = ""

        'CEK PARAMETER DATA BATCH
        If dataSplit(2).Length > 0 Then

            'VALIDASI DAN SET DATA BATCH ======================================================
            'SPLIT PARAMETER DATA BATCH
            dataBatch = dataSplit(2).Split(sptRow)
            'END OF VALIDASI DAN SET DATA BATCH ===============================================

            'VALIDASI DAN SET DATA ROW BATCH ==================================================
            Dim JmlDtBatch As Integer = dataBatch.Length
            For i = 1 To JmlDtBatch
                'SPLIT DATA DETAIL
                dataRowBatch = dataBatch(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA BATCH -----------------------------------
                'CEK ARRAY DATA BATCH
                If (dataRowBatch.Length <> 19) Then
                    result(2) = "Batch Row : " & i & " - Invalid batch number data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW BATCH ----------------------------

                'VALIDASI TIPE DATA BATCH ------------------------------------------
                'nbtid(0) As Integer
                If (IsNumeric(dataRowBatch(0)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtid required numeric." : GoTo selesai
                End If
                'nbtjenismutasi(1) As Integer
                'JENISMUTASI BARANG MASUK = 1, KELUAR = 0
                dataRowBatch(1) = 0
                If (IsNumeric(dataRowBatch(1)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtjenismutasi required numeric." : GoTo selesai
                End If
                'nbtidbarang(2) As Integer
                If (IsNumeric(dataRowBatch(2)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtidbarang required numeric." : GoTo selesai
                End If
                'nbtidtransaksi(5) As Integer
                If (IsNumeric(dataRowBatch(5)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtidtransaksi required numeric." : GoTo selesai
                End If
                'nbtjml(7) As Double
                If (IsNumeric(dataRowBatch(7)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtjml required numeric." : GoTo selesai
                End If
                'nbtcustomdbl1(11) As Double
                If (IsNumeric(dataRowBatch(11)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl1 required numeric." : GoTo selesai
                End If
                'nbtcustomdbl2(12) As Double
                If (IsNumeric(dataRowBatch(12)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl2 required numeric." : GoTo selesai
                End If
                'nbtcustomdbl3(13) As Double
                If (IsNumeric(dataRowBatch(13)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl3 required numeric." : GoTo selesai
                End If
                'nbtcustomdate1(14) As Date
                If (IsDate(dataRowBatch(14)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate1 required date." : GoTo selesai
                End If
                'nbtcustomdate2(15) As Date
                If (IsDate(dataRowBatch(15)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate2 required date." : GoTo selesai
                End If
                'nbtcustomdate3(16) As Date
                If (IsDate(dataRowBatch(16)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate3 required date." : GoTo selesai
                End If
                'nbtidbatchin(18) As Integer
                If (IsNumeric(dataRowBatch(18)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtidbatchin required numeric." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA BATCH -----------------------------------

                'VALIDASI DATA BATCH ---------------------------------------
                'nbtkode(3) As String
                If Len(dataRowBatch(3)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtkode can't be empty" : GoTo selesai
                End If
                If Len(dataRowBatch(3)) > 100 Then
                    result(2) = "Batch Row : " & i & " - nbtkode should not be more than 100 character." : GoTo selesai
                End If

                'nbtsumber(4) As String
                If Len(dataRowBatch(4)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtsumber can't be empty" : GoTo selesai
                End If
                If Len(dataRowBatch(4)) > 10 Then
                    result(2) = "Batch Row : " & i & " - nbtsumber should not be more than 10 character." : GoTo selesai
                End If

                'nbtsatuan(6) As String
                If Len(dataRowBatch(6)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtsatuan can't be empty" : GoTo selesai
                End If
                If Len(dataRowBatch(6)) > 25 Then
                    result(2) = "Batch Row : " & i & " - nbtsatuan should not be more than 25 character." : GoTo selesai
                End If

                'nbtjml(7) As Double
                If Len(dataRowBatch(7)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtjml can't be empty" : GoTo selesai
                End If

                'nbtcustomdbl1(11) As Double
                If Len(dataRowBatch(11)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl1 can't be empty" : GoTo selesai
                End If

                'nbtcustomdbl2(12) As Double
                If Len(dataRowBatch(12)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl2 can't be empty" : GoTo selesai
                End If

                'nbtcustomdbl3(13) As Double
                If Len(dataRowBatch(13)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl3 can't be empty" : GoTo selesai
                End If

                'nbtcustomdate1(14) As Date
                If Len(dataRowBatch(14)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate1 can't be empty" : GoTo selesai
                End If

                'nbtcustomdate2(15) As Date
                If Len(dataRowBatch(15)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate2 can't be empty" : GoTo selesai
                End If

                'nbtcustomdate3(16) As Date
                If Len(dataRowBatch(16)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate3 can't be empty" : GoTo selesai
                End If

                'nbtgudang(17) As String
                If Len(dataRowBatch(17)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtgudang can't be empty" : GoTo selesai
                End If
                'END OF VALIDASI DATA BATCH --------------------------------

                If AsDataTableTambahData(dtbatch, "nbtid~nbtjenismutasi~nbtidbarang~nbtkode~nbtsumber~nbtidtransaksi~nbtsatuan~nbtjml~nbtcustomtext1~nbtcustomtext2~nbtcustomtext3~nbtcustomdbl1~nbtcustomdbl2~nbtcustomdbl3~nbtcustomdate1~nbtcustomdate2~nbtcustomdate3~nbtgudang~nbtidbatchin", dataRowBatch(0) & "~" & dataRowBatch(1) & "~" & dataRowBatch(2) & "~" & dataRowBatch(3) & "~" & dataRowBatch(4) & "~" & dataRowBatch(5) & "~" & dataRowBatch(6) & "~" & dataRowBatch(7) & "~" & dataRowBatch(8) & "~" & dataRowBatch(9) & "~" & dataRowBatch(10) & "~" & dataRowBatch(11) & "~" & dataRowBatch(12) & "~" & dataRowBatch(13) & "~" & dataRowBatch(14) & "~" & dataRowBatch(15) & "~" & dataRowBatch(16) & "~" & dataRowBatch(17) & "~" & dataRowBatch(18)) = False Then
                    result(2) = "Batch Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

                'ValidasiSimpan
                'BUAT FILTER UNTUK VALIDASI ---------------------------------
                'nbtidbarang(2) As Integer , nbtkode(3) As String      , nbtjml(7) As Double         , nbtgudang(17) As String      , nbtidbatchin(18) As Integer
                idbarang = dataRowBatch(2) : nbtkode = dataRowBatch(3) : jmlbarang = dataRowBatch(7) : nbtgudang = dataRowBatch(17) : nbtidbatchin = dataRowBatch(18)

                'VALIDASI BATCH -------------------------------
                '1. CEK DATA EXIST BATCH KELUAR 
                ftExistBatch = IIf(Len(ftExistBatch.ToString) = 0, "", ftExistBatch & " UNION ")
                ftExistBatch = String.Concat(ftExistBatch, "SELECT EXISTS(SELECT 1 FROM m1_no_batch_in WHERE nbiidbatchin = '" & nbtidbatchin & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & nbtkode & "' as nbikode, '" & nbtgudang & "' as nbigudang FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML BATCH KELUAR 
                Dim jmlKeluar As Double = AsDataTableDSum(dtbatch, "nbtjml", "nbtidbatchin = " & nbtidbatchin & "")
                ftBatch = IIf(Len(ftBatch.ToString) = 0, "", ftBatch & " OR ")
                ftBatch = String.Concat(ftBatch, " (nbi.nbiidbatchin = " & nbtidbatchin & " AND " & jmlKeluar & " > nbi.nbijmlsisa) ")

                '3. SET NILAI UPDATE BATCH IN 
                updNilaiBatch = String.Concat("WHEN nbiidbatchin = '" & nbtidbatchin & "' THEN ROUND(nbijmlkeluar + '" & jmlKeluar & "', 5) ", updNilaiBatch)

                '4. SET FILTER UPDATE BATCH IN 
                updFilterBatch = IIf(Len(updFilterBatch.ToString) = 0, "", updFilterBatch & " OR ")
                updFilterBatch = String.Concat(updFilterBatch, "(nbiidbatchin = '" & nbtidbatchin & "')")
                'END OF BUAT FILTER UNTUK VALIDASI --------------------------

            Next
            'END OF VALIDASI DAN SET ROW DATA BATCH ===========================================

        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'nstid(0) As Integer, nstjenismutasi(1) As Integer, nstidbarang(2) As Integer, nstkode(3) As String, nstsumber(4) As String, 
        'nstidtransaksi(5) As Integer, nstsatuan(6) As String, nstjml(7) As Double, nstcustomtext1(8) As String, nstcustomtext2(9) As String, 
        'nstcustomtext3(10) As String, nstcustomdbl1(11) As Double, nstcustomdbl2(12) As Double, nstcustomdbl3(13) As Double, nstcustomdate1(14) As Date, 
        'nstcustomdate2(15) As Date, nstcustomdate3(16) As Date, nstgudang(17) As String, nstidserialin(18) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'nstid, nstjenismutasi, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, 
        'nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, 
        'nstcustomdate1, nstcustomdate2, nstcustomdate3, nstgudang, nstidserialin

        'Buat datatable serial
        Dim dtserial As New DataTable
        AsDataTableTambahField(dtserial, "nstid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstjenismutasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtserial, "nstidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtserial, "nstkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstidtransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtserial, "nstsatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstjml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtserial, "nstcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstidserialin", AsEnumTypeData.AsInt64)

        'ValidasiSimpan
        Dim ftExistSerial As String = "", ftSerial As String = ""
        Dim nstkode As String = "", nstgudang As String = "", nstidserialin As Integer = 0
        Dim updNilaiSerial As String = "", updFilterSerial As String = ""

        'CEK PARAMETER DATA SERIAL
        If dataSplit(3).Length > 0 Then
            'VALIDASI DAN SET DATA SERIAL ======================================================
            'SPLIT PARAMETER DATA SERIAL
            dataSerial = dataSplit(3).Split(sptRow)
            'END OF VALIDASI DAN SET DATA SERIAL ===============================================

            'VALIDASI DAN SET DATA ROW SERIAL ==================================================
            Dim JmlDtSerial As Integer = dataSerial.Length
            For i = 1 To JmlDtSerial
                'SPLIT DATA SERIAL
                dataRowSerial = dataSerial(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA SERIAL -----------------------------------
                'CEK ARRAY DATA SERIAL
                If (dataRowSerial.Length <> 19) Then
                    result(2) = "Serial Row : " & i & " - Invalid serial number data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW SERIAL ----------------------------

                'VALIDASI TIPE DATA SERIAL ------------------------------------------
                'nstid(0) As Integer
                If (IsNumeric(dataRowSerial(0)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstid required numeric." : GoTo selesai
                End If
                'nstjenismutasi(1) As Integer
                'JENISMUTASI BARANG MASUK = 1, KELUAR = 0
                dataRowSerial(1) = 0
                If (IsNumeric(dataRowSerial(1)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstjenismutasi required numeric." : GoTo selesai
                End If
                'nstidbarang(2) As Integer
                If (IsNumeric(dataRowSerial(2)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstidbarang required numeric." : GoTo selesai
                End If
                'nstidtransaksi(5) As Integer
                If (IsNumeric(dataRowSerial(5)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstidtransaksi required numeric." : GoTo selesai
                End If
                'nstjml(7) As Double
                If (IsNumeric(dataRowSerial(7)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstjml required numeric." : GoTo selesai
                End If
                'nstcustomdbl1(11) As Double
                If (IsNumeric(dataRowSerial(11)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl1 required numeric." : GoTo selesai
                End If
                'nstcustomdbl2(12) As Double
                If (IsNumeric(dataRowSerial(12)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl2 required numeric." : GoTo selesai
                End If
                'nstcustomdbl3(13) As Double
                If (IsNumeric(dataRowSerial(13)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl3 required numeric." : GoTo selesai
                End If
                'nstcustomdate1(14) As Date
                If (IsDate(dataRowSerial(14)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate1 required date." : GoTo selesai
                End If
                'nstcustomdate2(15) As Date
                If (IsDate(dataRowSerial(15)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate2 required date." : GoTo selesai
                End If
                'nstcustomdate3(16) As Date
                If (IsDate(dataRowSerial(16)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate3 required date." : GoTo selesai
                End If
                'nstidserialin(18) As Integer
                If (IsNumeric(dataRowSerial(18)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstidserialin required numeric." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA SERIAL -----------------------------------

                'VALIDASI DATA SERIAL ---------------------------------------
                'nstkode(3) As String
                If Len(dataRowSerial(3)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstkode can't be empty" : GoTo selesai
                End If
                If Len(dataRowSerial(3)) > 100 Then
                    result(2) = "Serial Row : " & i & " - nstkode should not be more than 100 character." : GoTo selesai
                End If

                'nstsumber(4) As String
                If Len(dataRowSerial(4)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstsumber can't be empty" : GoTo selesai
                End If
                If Len(dataRowSerial(4)) > 10 Then
                    result(2) = "Serial Row : " & i & " - nstsumber should not be more than 10 character." : GoTo selesai
                End If

                'nstsatuan(6) As String
                If Len(dataRowSerial(6)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstsatuan can't be empty" : GoTo selesai
                End If
                If Len(dataRowSerial(6)) > 25 Then
                    result(2) = "Serial Row : " & i & " - nstsatuan should not be more than 25 character." : GoTo selesai
                End If

                'nstjml(7) As Double
                If Len(dataRowSerial(7)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstjml can't be empty" : GoTo selesai
                End If

                'nstcustomdbl1(11) As Double
                If Len(dataRowSerial(11)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl1 can't be empty" : GoTo selesai
                End If

                'nstcustomdbl2(12) As Double
                If Len(dataRowSerial(12)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl2 can't be empty" : GoTo selesai
                End If

                'nstcustomdbl3(13) As Double
                If Len(dataRowSerial(13)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl3 can't be empty" : GoTo selesai
                End If

                'nstcustomdate1(14) As Date
                If Len(dataRowSerial(14)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate1 can't be empty" : GoTo selesai
                End If

                'nstcustomdate2(15) As Date
                If Len(dataRowSerial(15)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate2 can't be empty" : GoTo selesai
                End If

                'nstcustomdate3(16) As Date
                If Len(dataRowSerial(16)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate3 can't be empty" : GoTo selesai
                End If

                'nstgudang(17) As String
                If Len(dataRowSerial(16)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstgudang can't be empty" : GoTo selesai
                End If
                'END OF VALIDASI DATA SERIAL --------------------------------

                If AsDataTableTambahData(dtserial, "nstid~nstjenismutasi~nstidbarang~nstkode~nstsumber~nstidtransaksi~nstsatuan~nstjml~nstcustomtext1~nstcustomtext2~nstcustomtext3~nstcustomdbl1~nstcustomdbl2~nstcustomdbl3~nstcustomdate1~nstcustomdate2~nstcustomdate3~nstgudang~nstidserialin", dataRowSerial(0) & "~" & dataRowSerial(1) & "~" & dataRowSerial(2) & "~" & dataRowSerial(3) & "~" & dataRowSerial(4) & "~" & dataRowSerial(5) & "~" & dataRowSerial(6) & "~" & dataRowSerial(7) & "~" & dataRowSerial(8) & "~" & dataRowSerial(9) & "~" & dataRowSerial(10) & "~" & dataRowSerial(11) & "~" & dataRowSerial(12) & "~" & dataRowSerial(13) & "~" & dataRowSerial(14) & "~" & dataRowSerial(15) & "~" & dataRowSerial(16) & "~" & dataRowSerial(17) & "~" & dataRowSerial(18)) = False Then
                    result(2) = "Serial Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

                'ValidasiSimpan
                'BUAT FILTER UNTUK VALIDASI ---------------------------------
                'nstidbarang(2) As Integer  , nstkode(3) As String       , nstjml(7) As Double          , nstgudang(17) As String       , nstidserialin(18) As Integer
                idbarang = dataRowSerial(2) : nstkode = dataRowSerial(3) : jmlbarang = dataRowSerial(7) : nstgudang = dataRowSerial(17) : nstidserialin = dataRowSerial(18)

                'VALIDASI SERIAL -------------------------------
                '1. CEK DATA EXIST SERIAL KELUAR
                ftExistSerial = IIf(Len(ftExistSerial.ToString) = 0, "", ftExistSerial & " UNION ")
                ftExistSerial = String.Concat(ftExistSerial, "SELECT EXISTS(SELECT 1 FROM m1_no_serial_in WHERE nsiidserialin = '" & nstidserialin & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & nstkode & "' as nsikode, '" & nstgudang & "' as nsigudang FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML SERIAL KELUAR 
                Dim jmlKeluar As Double = AsDataTableDSum(dtserial, "nstjml", "nstidserialin = " & nstidserialin & "")
                ftSerial = IIf(Len(ftSerial.ToString) = 0, "", ftSerial & " OR ")
                ftSerial = String.Concat(ftSerial, " (nsi.nsiidserialin = " & nstidserialin & " AND " & jmlKeluar & " > nsi.nsijmlsisa) ")

                '3. SET NILAI UPDATE SERIAL IN 
                updNilaiSerial = String.Concat("WHEN nsiidserialin = '" & nstidserialin & "' THEN ROUND(nsijmlkeluar + '" & jmlKeluar & "', 5) ", updNilaiSerial)

                '4. SET FILTER UPDATE SERIAL IN 
                updFilterSerial = IIf(Len(updFilterSerial.ToString) = 0, "", updFilterSerial & " OR ")
                updFilterSerial = String.Concat(updFilterSerial, "(nsiidserialin = '" & nstidserialin & "')")
                'END OF BUAT FILTER UNTUK VALIDASI --------------------------

            Next
            'END OF VALIDASI DAN SET ROW DATA SERIAL ===========================================
        End If


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

                'CEK PERIODE AKUNTANSI ==================================
                Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("rstgl")), AsFormatTanggal(drutama("rstgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================

                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("rsstatus") = 2 Then

                    'VALIDASI BATCH SERIAL ---------------
                    'ValidasiBatchSerial
                    Dim rsValidasi As String = ValidasiBatchSerial(dtdetail, dtbatch, dtserial, ftBarang, "jmlbarang", 0)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    'END OF VALIDASI BATCH SERIAL --------

                    rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstanding, ftOutstanding, ftExistStok, ftStok, ftExistBatch, ftBatch, ftExistSerial, ftSerial, "gudangtransit")
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================

                If isUpdate Then
                    result(4) = drutama("rsid")
                    notransaksi = drutama("rsnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(rsid), rsnotransaksi FROM M3_Rs WHERE rsid='" & result(4) & "' AND rsstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(rsid) FROM m3_rs WHERE rsnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m3_rs_history
                        Dim rsSimpanHistory As String = SimpanHistory.M3_Rs_HistorySimpan("" & paramSplit(0) & "★M3_Rs_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("rssumber")) & "▼" & FixQuotes(drutama("rsid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M3_Rs set rscabang  = '" & FixQuotes(drutama("rscabang")) & "', rslokasi  = '" & FixQuotes(drutama("rslokasi")) & "', rsgudangasal  = '" & FixQuotes(drutama("rsgudangasal")) & "', rsgudangtransit  = '" & FixQuotes(drutama("rsgudangtransit")) & "', rsgudangtujuan  = '" & FixQuotes(drutama("rsgudangtujuan")) & "', rssumber  = '" & FixQuotes(drutama("rssumber")) & "', rsautonotransaksi  = " & drutama("rsautonotransaksi") & ", rsnotransaksi  = '" & notransaksi & "', rstgl  = '" & FixQuotes(AsFormatTanggal(drutama("rstgl"))) & "', rskodepa  = " & drutama("rskodepa") & ", rsbagianterima  = " & drutama("rsbagianterima") & ", rsbagianterimakontak  = '" & FixQuotes(drutama("rsbagianterimakontak")) & "', rsuraian  = '" & FixQuotes(drutama("rsuraian")) & "', rscatatan  = '" & FixQuotes(drutama("rscatatan")) & "', rsnoref  = '" & FixQuotes(drutama("rsnoref")) & "', rstglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("rstglnoref"))) & "', rsidmr  = " & drutama("rsidmr") & ", rsidts  = " & drutama("rsidts") & ", rsstatus  = " & drutama("rsstatus") & ", rsstatussebelumnya  = " & drutama("rsstatussebelumnya") & ", rsjmlrevisi  = rsjmlrevisi+1, rscetakanke  = " & drutama("rscetakanke") & ", rsmodifikasiuser  = " & drutama("rsmodifikasiuser") & ", rsmodifikasitgl  = NOW(), rscustomtext1  = '" & FixQuotes(drutama("rscustomtext1")) & "', rscustomtext2  = '" & FixQuotes(drutama("rscustomtext2")) & "', rscustomtext3  = '" & FixQuotes(drutama("rscustomtext3")) & "', rscustomtext4  = '" & FixQuotes(drutama("rscustomtext4")) & "', rscustomtext5  = '" & FixQuotes(drutama("rscustomtext5")) & "', rscustomint1  = " & drutama("rscustomint1") & ", rscustomint2  = " & drutama("rscustomint2") & ", rscustomint3  = " & drutama("rscustomint3") & ", rscustomdbl1  = '" & FixDouble(drutama("rscustomdbl1")) & "', rscustomdbl2  = '" & FixDouble(drutama("rscustomdbl2")) & "', rscustomdbl3  = '" & FixDouble(drutama("rscustomdbl3")) & "', rscustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("rscustomdate1"))) & "', rscustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("rscustomdate2"))) & "', rscustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("rscustomdate3"))) & "' where rsid = '" & drutama("rsid") & "'"
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

                    If drutama("rsautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rscabang"), drutama("rslokasi"), drutama("rssumber"), drutama("rstgl"))
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
                        notransaksi = drutama("rsnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(rsid) FROM m3_rs WHERE rsnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M3_Rs (rscabang, rslokasi, rsgudangasal, rsgudangtransit, rsgudangtujuan, rssumber, rsautonotransaksi, rsnotransaksi, rstgl, rskodepa, rsbagianterima, rsbagianterimakontak, rsuraian, rscatatan, rsnoref, rstglnoref, rsidmr, rsidts, rsstatus, rsstatussebelumnya, rsjmlrevisi, rscetakanke, rsinputuser, rsinputtgl, rsmodifikasiuser, rsmodifikasitgl, rsisclose, rscustomtext1, rscustomtext2, rscustomtext3, rscustomtext4, rscustomtext5, rscustomint1, rscustomint2, rscustomint3, rscustomdbl1, rscustomdbl2, rscustomdbl3, rscustomdate1, rscustomdate2, rscustomdate3) values('" & FixQuotes(drutama("rscabang")) & "', '" & FixQuotes(drutama("rslokasi")) & "', '" & FixQuotes(drutama("rsgudangasal")) & "', '" & FixQuotes(drutama("rsgudangtransit")) & "', '" & FixQuotes(drutama("rsgudangtujuan")) & "', '" & FixQuotes(drutama("rssumber")) & "', " & drutama("rsautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("rstgl"))) & "', " & drutama("rskodepa") & ", " & drutama("rsbagianterima") & ", '" & FixQuotes(drutama("rsbagianterimakontak")) & "', '" & FixQuotes(drutama("rsuraian")) & "', '" & FixQuotes(drutama("rscatatan")) & "', '" & FixQuotes(drutama("rsnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rstglnoref"))) & "', " & drutama("rsidmr") & ", " & drutama("rsidts") & ", " & drutama("rsstatus") & ", " & drutama("rsstatussebelumnya") & ", " & drutama("rsjmlrevisi") & ", " & drutama("rscetakanke") & ", " & drutama("rsinputuser") & ", NOW(), " & drutama("rsmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("rsisclose") & ", '" & FixQuotes(drutama("rscustomtext1")) & "', '" & FixQuotes(drutama("rscustomtext2")) & "', '" & FixQuotes(drutama("rscustomtext3")) & "', '" & FixQuotes(drutama("rscustomtext4")) & "', '" & FixQuotes(drutama("rscustomtext5")) & "', " & drutama("rscustomint1") & ", " & drutama("rscustomint2") & ", " & drutama("rscustomint3") & ", '" & FixDouble(drutama("rscustomdbl1")) & "', '" & FixDouble(drutama("rscustomdbl2")) & "', '" & FixDouble(drutama("rscustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rscustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rscustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rscustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select rsid from M3_rs where rsnotransaksi='" & notransaksi & "' AND rsinputuser= '" & userid & "' order by rsmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M3_Rs_Detail where idrs = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idrsdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idmrdetail") & ", " & dr1("idtsdetail") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M3_Rs_Detail(idrsdetail, idrs, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idmrdetail, idtsdetail, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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


                'Hapus batch ketika update
                If (isUpdate) Then
                    sql = "Delete from M1_No_Batch_Transaction where nbtidtransaksi  = '" & result(4) & "' AND nbtsumber = 'RS'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses batch
                If (dtbatch.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtbatch.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & 0 & ", " & dr1("nbtjenismutasi") & ", " & dr1("nbtidbatchin") & ", '" & FixQuotes(dr1("nbtgudang")) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', '" & FixQuotes(dr1("nbtcustomtext1")) & "', '" & FixQuotes(dr1("nbtcustomtext2")) & "', '" & FixQuotes(dr1("nbtcustomtext3")) & "', '" & FixDouble(dr1("nbtcustomdbl1")) & "', '" & FixDouble(dr1("nbtcustomdbl2")) & "', '" & FixDouble(dr1("nbtcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate3"))) & "')")
                    Next
                    sql = "Insert into M1_No_Batch_Transaction(nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Hapus serial ketika update
                If (isUpdate) Then
                    sql = "Delete from M1_No_Serial_Transaction  where nstidtransaksi  = '" & result(4) & "' AND nstsumber = 'RS'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses serial
                If (dtserial.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtserial.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & 0 & ", " & dr1("nstjenismutasi") & ", " & dr1("nstidserialin") & ", '" & FixQuotes(dr1("nstgudang")) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', '" & FixQuotes(dr1("nstcustomtext1")) & "', '" & FixQuotes(dr1("nstcustomtext2")) & "', '" & FixQuotes(dr1("nstcustomtext3")) & "', '" & FixDouble(dr1("nstcustomdbl1")) & "', '" & FixDouble(dr1("nstcustomdbl2")) & "', '" & FixDouble(dr1("nstcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate3"))) & "')")
                    Next
                    sql = "Insert into M1_No_Serial_Transaction(nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                If drutama("rsstatus") = 2 Then
                    If Len(updNilai) > 0 Then
                        'UPDATE OUTSTANDING TRANSAKSI ===============================================
                        'UPDATE DETAIL
                        sql = "UPDATE m3_ts_detail SET jmlrealisasi = (CASE idtsdetail " & updNilai & " ELSE jmlrealisasi END) WHERE " & updFilter
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE UTAMA
                        Dim ftDetail As String = "", statusOut As Integer = 0
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idts FROM m3_ts_detail WHERE " & updFilter & " GROUP BY idts")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idts = '" & dr1("idts") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDB("SELECT idts, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m3_ts_detail WHERE " & ftDetail & " GROUP BY idts")
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilai = "" : updFilter = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                    statusOut = 2
                                ElseIf dr1("jmlrealisasi") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilai = String.Concat(updNilai, "WHEN '" & dr1("idts") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                                updFilter = String.Concat(updFilter, "(tsid = '" & dr1("idts") & "')")
                            Next

                            sql = "UPDATE m3_ts SET tsstatusrealisasi = (CASE tsid " & updNilai & " ELSE tsstatusrealisasi END) WHERE " & updFilter
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                        'END OF UPDATE OUTSTANDING TRANSAKSI ========================================
                    End If

                    If Len(updNilaiMR) > 0 Then
                        'UPDATE OUTSTANDING TRANSAKSI MR ============================================
                        'UPDATE DETAIL
                        sql = "UPDATE m3_mr_detail SET jmlrealisasi = (CASE idmrdetail " & updNilaiMR & " ELSE jmlrealisasi END) WHERE " & updFilterMR
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE UTAMA
                        Dim ftDetail As String = "", statusOut As Integer = 0
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idmr FROM m3_mr_detail WHERE " & updFilterMR & " GROUP BY idmr")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idmr = '" & dr1("idmr") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDB("SELECT idmr, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m3_mr_detail WHERE " & ftDetail & " GROUP BY idmr")
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiMR = "" : updFilterMR = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                    statusOut = 2
                                ElseIf dr1("jmlrealisasi") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilaiMR = String.Concat(updNilaiMR, "WHEN '" & dr1("idmr") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterMR = IIf(Len(updFilterMR.ToString) = 0, "", updFilterMR & " OR ")
                                updFilterMR = String.Concat(updFilterMR, "(mrid = '" & dr1("idmr") & "')")
                            Next

                            sql = "UPDATE m3_mr SET mrstatusrealisasi = (CASE mrid " & updNilaiMR & " ELSE mrstatusrealisasi END) WHERE " & updFilterMR
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                        'END OF UPDATE OUTSTANDING TRANSAKSI MR =====================================
                    End If


                    'AMBIL GUDANG TUJUAN DARI UTAMA =================================================
                    'GUDANG TUJUAN UTAMA DIGUNAKAN UNTUK NO SERIAL DAN BATCH MASUK
                    'MISAL : GUDANG TRANSIT 'I', MAKA :
                    '-- NO SERIAL DAN BATCH GUDANG 'I' BERKURANG
                    '-- NO SERIAL DAN BATCH GUDANG TUJUAN BERTAMBAH
                    Dim SetGudang As String = drutama("rsgudangtujuan")
                    'END OF AMBIL GUDANG TUJUAN DARI UTAMA ==========================================


                    'INSERT NO BATCH ================================================================
                    If dtbatch.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder, strValue3 As New StringBuilder
                        For Each dr1 As DataRow In dtbatch.Rows
                            'QUERY INSERT NO BATCH OUT
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping             nboid,            nboidbatchin,                           nbogudang,                  nboidbarang,                           nbokode,                             nbosumber,            nboidtransaksi,                     nbosatuan,                         nbojmlkeluar,       nboisclose
                            strValue2.Append("(" & 0 & ", " & dr1("nbtidbatchin") & ", '" & FixQuotes(dr1("nbtgudang")) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', " & 0 & ")")

                            'QUERY INSERT NO BATCH IN
                            strValue3.Append(IIf(Len(strValue3.ToString) = 0, "", ", "))
                            'mapping        nbiidbatchin,                nbigudang,                nbiidbarang,                           nbikode,                             nbisumber,            nbiidtransaksi,                     nbisatuan,                 nbijmlmasuk,       nbijmlkeluar,                  nbijmlsisa, nbiisclose,                     nbicustomtext1,                             nbicustomtext2,                             nbicustomtext3,                             nbicustomdbl1,                             nbicustomdbl2,                             nbicustomdbl3,                                             nbicustomdate1,                                              nbicustomdate2,                                              nbicustomdate3
                            strValue3.Append("(" & 0 & ", '" & FixQuotes(SetGudang) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixQuotes(dr1("nbtcustomtext1")) & "', '" & FixQuotes(dr1("nbtcustomtext2")) & "', '" & FixQuotes(dr1("nbtcustomtext3")) & "', '" & FixDouble(dr1("nbtcustomdbl1")) & "', '" & FixDouble(dr1("nbtcustomdbl2")) & "', '" & FixDouble(dr1("nbtcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate3"))) & "')")
                        Next

                        'INSERT NO BATCH OUT ---------------------------------
                        sql = "Insert into M1_No_Batch_Out(nboid, nboidbatchin, nbogudang, nboidbarang, nbokode, nbosumber, nboidtransaksi, nbosatuan, nbojmlkeluar, nboisclose) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE NO BATCH IN KELUAR ---------------------------
                        If Len(updNilaiBatch) > 0 Then
                            sql = "UPDATE m1_no_batch_in SET nbijmlkeluar =  (CASE " & updNilaiBatch & " ELSE nbijmlkeluar END) WHERE " & updFilterBatch
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If

                        'INSERT NO BATCH IN MASUK ----------------------------
                        sql = "Insert into M1_No_Batch_In(nbiidbatchin, nbigudang, nbiidbarang, nbikode, nbisumber, nbiidtransaksi, nbisatuan, nbijmlmasuk, nbijmlkeluar, nbijmlsisa, nbiisclose, nbicustomtext1, nbicustomtext2, nbicustomtext3, nbicustomdbl1, nbicustomdbl2, nbicustomdbl3, nbicustomdate1, nbicustomdate2, nbicustomdate3) values" & strValue3.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    End If
                    'END OF INSERT NO BATCH =========================================================

                    'INSERT NO SERIAL ===============================================================
                    If dtserial.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder, strValue3 As New StringBuilder
                        For Each dr1 As DataRow In dtserial.Rows
                            'QUERY INSERT NO SERIAL OUT
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping            nsoid,             nsoidserialin,                           nsogudang,                  nsoidbarang,                           nsokode,                             nsosumber,            nsoidtransaksi,                     nsosatuan,                          nsojmlkeluar,      nsoisclose
                            strValue2.Append("(" & 0 & ", " & dr1("nstidserialin") & ", '" & FixQuotes(dr1("nstgudang")) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', " & 0 & ")")

                            'QUERY INSERT NO SERIAL IN
                            strValue3.Append(IIf(Len(strValue3.ToString) = 0, "", ", "))
                            'mapping       nsiidserialin,                nsigudang,                nsiidbarang,                           nsikode,                             nsisumber,            nsiidtransaksi,                     nsisatuan,                       nsijmlmasuk, nsijmlkeluar,                  nsijmlsisa, nsiisclose,                     nsicustomtext1,                             nsicustomtext2,                             nsicustomtext3,                             nsicustomdbl1,                             nsicustomdbl2,                             nsicustomdbl3,                                             nsicustomdate1,                                              nsicustomdate2,                                              nsicustomdate3
                            strValue3.Append("(" & 0 & ", '" & FixQuotes(SetGudang) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixQuotes(dr1("nstcustomtext1")) & "', '" & FixQuotes(dr1("nstcustomtext2")) & "', '" & FixQuotes(dr1("nstcustomtext3")) & "', '" & FixDouble(dr1("nstcustomdbl1")) & "', '" & FixDouble(dr1("nstcustomdbl2")) & "', '" & FixDouble(dr1("nstcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate3"))) & "')")
                        Next

                        'INSERT NO SERIAL OUT --------------------------------
                        sql = "Insert into M1_No_Serial_Out(nsoid, nsoidserialin, nsogudang, nsoidbarang, nsokode, nsosumber, nsoidtransaksi, nsosatuan, nsojmlkeluar, nsoisclose) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE NO SERIAL IN KELUAR --------------------------
                        If Len(updNilaiSerial) > 0 Then
                            sql = "UPDATE m1_no_serial_in SET nsijmlkeluar =  (CASE " & updNilaiSerial & " ELSE nsijmlkeluar END) WHERE " & updFilterSerial
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If

                        'INSERT NO SERIAL IN MASUK ---------------------------
                        sql = "Insert into M1_No_Serial_In(nsiidserialin, nsigudang, nsiidbarang, nsikode, nsisumber, nsiidtransaksi, nsisatuan, nsijmlmasuk, nsijmlkeluar, nsijmlsisa, nsiisclose, nsicustomtext1, nsicustomtext2, nsicustomtext3, nsicustomdbl1, nsicustomdbl2, nsicustomdbl3, nsicustomdate1, nsicustomdate2, nsicustomdate3) values" & strValue3.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    End If
                    'END OF INSERT NO SERIAL ========================================================


                    'UPDATE STOK ====================================================================
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

                    'STOK MASUK
                    sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokIn & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                    'END OF UPDATE STOK =============================================================


                    'INSERT ITEM TRANSACTION ========================================================
                    'AMBIL DATA DETAIL YANG BARU
                    sql = "SELECT rsd.idrsdetail, rsd.idbarang, rsd.namabarang, rsd.tipebarang, rsd.jml, rsd.satuan, rsd.jmlbarang, rsd.satuanbarang, rsd.gudangasal, rsd.gudangtransit, rsd.gudangtujuan, rsd.catatan, rsd.costcenter, rsd.divisi, rsd.subdivisi, rsd.proyek, rs.rsinputtgl, i.bhpp, i.bhppaverage FROM m3_rs_detail rsd JOIN m3_rs rs ON rsd.idrs = rs.rsid JOIN m1_item i ON rsd.idbarang = i.bid WHERE rsd.idrs = '" & result(4) & "'"
                    Dim dtDetailNew As DataTable = AsDataTableAmbilDariDB(sql)
                    Dim hpp As Double = 0, jenismutasi As Double = 0, postinghpp As Double = 0
                    Dim strTransaksiBarang As New StringBuilder

                    If dtDetailNew.Rows.Count > 0 Then

                        'AMBIL MATAUANG FUNGSIONAL DARI SETTING
                        Dim dtMatauang As DataTable = AsDataTableAmbilDariDB("SELECT skode, snilai FROM m0_setting WHERE smodule = 0 AND sgrup = 'accounting' AND (skode = 'MataUangFungsional' OR skode = 'Kurs')")
                        Dim uangFungsional As String = AsDataTableDLookup(dtMatauang, "snilai", "skode = 'MataUangFungsional'", "Not found")
                        If uangFungsional = "Not found" Then
                            result(2) = "Setting Functional Currency not found." : GoTo selesai
                        End If
                        Dim kursFungsional As String = AsDataTableDLookup(dtMatauang, "snilai", "skode = 'Kurs'", "Not found")
                        If kursFungsional = "Not found" Then
                            result(2) = "Setting Exchange Rate Functional Currency not found." : GoTo selesai
                        End If

                        For Each dr1 As DataRow In dtDetailNew.Rows
                            'jenismutasi dan postinghpp 
                            '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 1
                            '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                            '- untuk transaksi mutasi saja maka postinghpp = 0
                            postinghpp = 0

                            'hitung hpp = hpp
                            hpp = Double.Parse(dr1("bhppaverage"))

                            'POSTING BARANG KELUAR (gudangtransit)
                            jenismutasi = 0
                            'QUERY INSERT TRANSAKSI BARANG KELUAR
                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                            'mapping                        id,                            cabang,                                    lokasi,                                 gudang,                        kodepa,           jenismutasi,                              sumber,                    idutama,             iddetail,                      notransaksi,                                                 tgl,                              kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                               kurs,                    harga,                 diskon,              jmldiskon,             idhppikm,  idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("rscabang")) & "', '" & FixQuotes(drutama("rslokasi")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', " & drutama("rskodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("rssumber")) & "', " & result(4) & ", " & dr1("idrsdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("rstgl"))) & "', " & drutama("rsbagianterima") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(uangFungsional) & "', '" & FixDouble(kursFungsional) & "', '" & FixDouble(hpp) & "', '" & FixQuotes(0) & "', '" & FixDouble(0) & "', " & 0 & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("rsuraian")) & "', '" & FixQuotes(drutama("rscatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("rsinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("rsinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")

                            'POSTING BARANG MASUK (gudangtujuan)
                            jenismutasi = 1
                            'QUERY INSERT TRANSAKSI BARANG MASUK
                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                            'mapping                        id,                            cabang,                                    lokasi,                                   gudang,                        kodepa,           jenismutasi,                              sumber,                    idutama,             iddetail,                      notransaksi,                                                 tgl,                              kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                               kurs,                    harga,                 diskon,              jmldiskon,          idhppikm,  idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("rscabang")) & "', '" & FixQuotes(drutama("rslokasi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', " & drutama("rskodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("rssumber")) & "', " & result(4) & ", " & dr1("idrsdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("rstgl"))) & "', " & drutama("rsbagianterima") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(uangFungsional) & "', '" & FixDouble(kursFungsional) & "', '" & FixDouble(hpp) & "', '" & FixQuotes(0) & "', '" & FixDouble(0) & "', " & 0 & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("rsuraian")) & "', '" & FixQuotes(drutama("rscatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("rsinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("rsinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                        Next

                        sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    Else
                        result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF INSERT ITEM TRANSACTION =================================================

                End If

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "RS", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M3_RsUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("rsbagianterimakode", "c1.kkode")
            Filter = Filter.Replace("rsbagianterimanama", "c1.knama")
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
            Dim sumber As String = "RS", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT rstgl, rsnotransaksi, rsstatus FROM m3_rs WHERE rsid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Rsstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m3_rs_history
            Dim rsSimpanHistory As String = SimpanHistory.M3_Rs_HistorySimpan("" & paramSplit(0) & "★M3_Rs_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then

                'CEK NO BATCH DAN SERIAL ========================================================
                'BATCH
                dtdetail = AsDataTableAmbilDariDB("SELECT bkode, nbikode FROM m1_no_batch_in JOIN m1_item ON nbiidbarang = bid WHERE nbisumber = '" & sumber & "' AND nbiidtransaksi = '" & idtransaksi & "' AND nbijmlkeluar > 0")
                If dtdetail.Rows.Count > 0 Then result(2) = "Item : " & dtdetail.Rows(0)("bkode") & " | No. Batch : " & dtdetail.Rows(0)("nbikode") & " has related transactions." : Trans.Rollback() : GoTo selesai

                'SERIAL
                dtdetail = AsDataTableAmbilDariDB("SELECT bkode, nsikode FROM m1_no_serial_in JOIN m1_item ON nsiidbarang = bid WHERE nsisumber = '" & sumber & "' AND nsiidtransaksi = '" & idtransaksi & "' AND nsijmlkeluar > 0")
                If dtdetail.Rows.Count > 0 Then result(2) = "Item : " & dtdetail.Rows(0)("bkode") & " | No. Serial : " & dtdetail.Rows(0)("nsikode") & " has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK NO BATCH DAN SERIAL =================================================


                Dim idbarang As Integer = 0, jmlbarang As Double = 0, gudangOut As String = "", gudangIn As String = "", idtsdetail As Integer = 0, idmrdetail As Integer = 0
                Dim updNilai As String = "", updFilter As String = "", updNilaiMR As String = "", updFilterMR As String = "", ftExistStok As String = "", ftStok As String = "", updStokIn As String = "", updStokOut As String = ""

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDB("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, gudangtransit, gudangtujuan, idmrdetail, idtsdetail, urutan FROM m3_rs_detail WHERE idrs = '" & idtransaksi & "'")
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : gudangIn = dr1("gudangtransit") : gudangOut = dr1("gudangtujuan") : idmrdetail = dr1("idmrdetail") : idtsdetail = dr1("idtsdetail")

                        'UPDATE OUTSTANDING ---------------------------
                        If idtsdetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idtsdetail=" & idtsdetail)
                            updNilai = String.Concat("WHEN '" & idtsdetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilai)

                            '2. SET FILTERUPDATE OUTSTANDING
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(idtsdetail = '" & idtsdetail & "')")
                        End If

                        If idmrdetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING MR
                            Dim OutstandingMR As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idmrdetail=" & idmrdetail)
                            updNilaiMR = String.Concat("WHEN '" & idmrdetail & "' THEN ROUND(jmlrealisasi - '" & OutstandingMR & "', 5) ", updNilaiMR)

                            '2. SET FILTERUPDATE OUTSTANDING MR
                            updFilterMR = IIf(Len(updFilterMR.ToString) = 0, "", updFilterMR & " OR ")
                            updFilterMR = String.Concat(updFilterMR, "(idmrdetail = '" & idmrdetail & "')")
                        End If

                        'VALIDASI STOK -------------------------------
                        '1. CEK DATA EXIST
                        ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                        ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idtsdetail & "' as idtsdetail, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                        '2. CEK JML STOK
                        Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangtujuan='" & gudangOut & "'")
                        ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
                        ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")

                        '3. SET NILAI UPDATE STOK KELUAR
                        updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
                        updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

                        '4. SET NILAI UPDATE STOK MASUK
                        updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
                        updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok
                        'END OF BUAT FILTER UNTUK UPDATE --------------------------
                    Next

                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If


                'VALIDASI STOK ----------------------------------
                Dim rsValidasi As String = ValidasiSimpan(dtdetail, "", "", ftExistStok, ftStok, "", "", "", "", "")
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'END OF VALIDASI STOK ---------------------------


                If Len(updFilter) > 0 Then
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m3_ts_detail SET jmlrealisasi = (CASE idtsdetail " & updNilai & " ELSE jmlrealisasi END) WHERE " & updFilter
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                    'END OF UPDATE OUTSTANDING DETAIL ---------------

                    'UPDATE OUTSTANDING UTAMA -----------------------
                    Dim ftDetail As String = "", statusOut As Integer = 0
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idts FROM m3_ts_detail WHERE " & updFilter & " GROUP BY idts")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idts = '" & dr1("idts") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDB("SELECT idts, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m3_ts_detail WHERE " & ftDetail & " GROUP BY idts")
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilai = "" : updFilter = ""
                        For Each dr1 As DataRow In dtOut.Rows
                            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                            If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                statusOut = 2
                            ElseIf dr1("jmlrealisasi") < 1 Then
                                statusOut = 0
                            Else
                                statusOut = 1
                            End If
                            '2. SET NILAI UPDATE OUTSTANDING
                            updNilai = String.Concat(updNilai, "WHEN '" & dr1("idts") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(tsid = '" & dr1("idts") & "')")
                        Next

                        sql = "UPDATE m3_ts SET tsstatusrealisasi = (CASE tsid " & updNilai & " ELSE tsstatusrealisasi END) WHERE " & updFilter
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF UPDATE OUTSTANDING UTAMA ----------------
                End If

                If Len(updFilterMR) > 0 Then
                    'UPDATE OUTSTANDING DETAIL MR -------------------
                    sql = "UPDATE m3_mr_detail SET jmlrealisasi = (CASE idmrdetail " & updNilaiMR & " ELSE jmlrealisasi END) WHERE " & updFilterMR
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                    'END OF UPDATE OUTSTANDING DETAIL MR ------------

                    'UPDATE OUTSTANDING UTAMA MR --------------------
                    Dim ftDetail As String = "", statusOut As Integer = 0
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idmr FROM m3_mr_detail WHERE " & updFilterMR & " GROUP BY idmr")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idmr = '" & dr1("idmr") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDB("SELECT idmr, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m3_mr_detail WHERE " & ftDetail & " GROUP BY idmr")
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiMR = "" : updFilterMR = ""
                        For Each dr1 As DataRow In dtOut.Rows
                            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                            If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                statusOut = 2
                            ElseIf dr1("jmlrealisasi") < 1 Then
                                statusOut = 0
                            Else
                                statusOut = 1
                            End If
                            '2. SET NILAI UPDATE OUTSTANDING MR
                            updNilaiMR = String.Concat(updNilaiMR, "WHEN '" & dr1("idmr") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING MR
                            updFilterMR = IIf(Len(updFilterMR.ToString) = 0, "", updFilterMR & " OR ")
                            updFilterMR = String.Concat(updFilterMR, "(mrid = '" & dr1("idmr") & "')")
                        Next

                        sql = "UPDATE m3_mr SET mrstatusrealisasi = (CASE mrid " & updNilaiMR & " ELSE mrstatusrealisasi END) WHERE " & updFilterMR
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF UPDATE OUTSTANDING UTAMA MR -------------
                End If


                'UPDATE NO BATCH ================================================================
                Dim updNilaiBatch As String = "", updFilterBatch As String = ""
                Dim dtBatch As DataTable = AsDataTableAmbilDariDB("SELECT nboidbatchin, nbogudang, nboidbarang, nbokode, nbojmlkeluar FROM m1_no_batch_out WHERE nbosumber = '" & sumber & "' AND nboidtransaksi = '" & idtransaksi & "'")
                If dtBatch.Rows.Count > 0 Then
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

                    'DELETE NO BATCH OUT --------------------------------
                    sql = "DELETE FROM m1_no_batch_out WHERE nbosumber = '" & sumber & "' AND nboidtransaksi = '" & idtransaksi & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE NO BATCH IN KELUAR --------------------------
                    For Each dr1 As DataRow In dtBatch.Rows
                        'SET NILAI UPDATE BATCH IN
                        Dim jmlKeluar As Double = AsDataTableDSum(dtBatch, "nbojmlkeluar", "nboidbatchin = " & dr1("nboidbatchin") & "")
                        updNilaiBatch = String.Concat("WHEN nbiidbatchin = '" & dr1("nboidbatchin") & "' THEN ROUND(nbijmlkeluar - '" & jmlKeluar & "', 5) ", updNilaiBatch)

                        'SET FILTER UPDATE BATCH IN
                        updFilterBatch = IIf(Len(updFilterBatch.ToString) = 0, "", updFilterBatch & " OR ")
                        updFilterBatch = String.Concat(updFilterBatch, "(nbiidbatchin = '" & dr1("nboidbatchin") & "')")
                    Next
                    If Len(updNilaiBatch) > 0 Then
                        sql = "UPDATE m1_no_batch_in SET nbijmlkeluar =  (CASE " & updNilaiBatch & " ELSE nbijmlkeluar END) WHERE " & updFilterBatch
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
                'END OF UPDATE NO BATCH =========================================================


                'UPDATE NO SERIAL ===============================================================
                Dim updNilaiSerial As String = "", updFilterSerial As String = ""
                Dim dtSerial As DataTable = AsDataTableAmbilDariDB("SELECT nsoidserialin, nsogudang, nsoidbarang, nsokode, nsojmlkeluar FROM m1_no_serial_out WHERE nsosumber = '" & sumber & "' AND nsoidtransaksi = '" & idtransaksi & "'")
                If dtSerial.Rows.Count > 0 Then
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

                    'DELETE NO SERIAL OUT -------------------------------
                    sql = "DELETE FROM m1_no_serial_out WHERE nsosumber = '" & sumber & "' AND nsoidtransaksi = '" & idtransaksi & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE NO SERIAL IN KELUAR -------------------------
                    For Each dr1 As DataRow In dtSerial.Rows
                        'SET NILAI UPDATE SERIAL IN
                        Dim jmlKeluar As Double = AsDataTableDSum(dtSerial, "nsojmlkeluar", "nsoidserialin = " & dr1("nsoidserialin") & "")
                        updNilaiSerial = String.Concat("WHEN nsiidserialin = '" & dr1("nsoidserialin") & "' THEN ROUND(nsijmlkeluar - '" & jmlKeluar & "', 5) ", updNilaiSerial)

                        'SET FILTER UPDATE SERIAL IN
                        updFilterSerial = IIf(Len(updFilterSerial.ToString) = 0, "", updFilterSerial & " OR ")
                        updFilterSerial = String.Concat(updFilterSerial, "(nsiidserialin = '" & dr1("nsoidserialin") & "')")
                    Next
                    If Len(updNilaiSerial) > 0 Then
                        sql = "UPDATE m1_no_serial_in SET nsijmlkeluar =  (CASE " & updNilaiSerial & " ELSE nsijmlkeluar END) WHERE " & updFilterSerial
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
                'END OF UPDATE NO SERIAL =======================================================


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

                'STOK MASUK
                sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokIn & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
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

            End If

            'update status utama
            sql = "UPDATE M3_rs SET rsstatus = " & nilaiStatus & ", rsmodifikasiuser='" & userid & "', rsmodifikasitgl = NOW(), rsposting = 0, rspostingtgl = '1971-01-01 00:00:00', rsjmlrevisi = rsjmlrevisi + 1 WHERE rsid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M3_RsSearch(PostWsSearch(paramSplit(0), "M3_RsSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M3_RsDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("rsbagianterimakode", "c1.kkode")
            Filter = Filter.Replace("rsbagianterimanama", "c1.knama")
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
            Dim sumber As String = "RS", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Rsid, Rsnotransaksi FROM m3_Rs WHERE Rsid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT rscabang, rslokasi, rssumber, rsautonotransaksi, rsnotransaksi, rstgl"
            sql &= " FROM M3_rs"
            sql &= " WHERE rsid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("rscabang")
                lokasi = dtNomorNext.Rows(0)("rslokasi")
                sumber = dtNomorNext.Rows(0)("rssumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("rsautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("rsnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("rstgl"))
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
            sql = "DELETE FROM M3_Rs_Detail WHERE idrs = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE UTAMA
            sql = "DELETE FROM M3_Rs WHERE rsid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M3_RsSearch(PostWsSearch(paramSplit(0), "M3_RsSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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