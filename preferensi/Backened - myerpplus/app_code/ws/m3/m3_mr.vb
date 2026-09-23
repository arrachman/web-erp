Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m3_mr
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M3_MrSimpan(ByVal param As String) As String
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
        'mrid(0) As Integer, mrcabang(1) As String, mrlokasi(2) As String, mrgudangasal(3) As String, mrgudangtujuan(4) As String, 
        'mrsumber(5) As String, mrautonotransaksi(6) As Integer, mrnotransaksi(7) As String, mrtgl(8) As Date, mrkodepa(9) As Integer, 
        'mrdimintaoleh(10) As Integer, mrdimintaolehkontak(11) As String, mrmintake(12) As Integer, mrtgldipakai(13) As Date, mruraian(14) As String, 
        'mrcatatan(15) As String, mrnoref(16) As String, mrtglnoref(17) As Date, mrstatusts(18) As Integer, mrstatusrs(19) As Integer, 
        'mrstatus(20) As Integer, mrstatussebelumnya(21) As Integer, mrjmlrevisi(22) As Integer, mrcetakanke(23) As Integer, mrinputuser(24) As Integer, 
        'mrinputtgl(25) As DateTime, mrmodifikasiuser(26) As Integer, mrmodifikasitgl(27) As DateTime, mrisclose(28) As Integer, mrcustomtext1(29) As String, 
        'mrcustomtext2(30) As String, mrcustomtext3(31) As String, mrcustomtext4(32) As String, mrcustomtext5(33) As String, mrcustomint1(34) As Integer, 
        'mrcustomint2(35) As Integer, mrcustomint3(36) As Integer, mrcustomdbl1(37) As Double, mrcustomdbl2(38) As Double, mrcustomdbl3(39) As Double, 
        'mrcustomdate1(40) As Date, mrcustomdate2(41) As Date, mrcustomdate3(42) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'mrid, mrcabang, mrlokasi, mrgudangasal, mrgudangtujuan, mrsumber, mrautonotransaksi, 
        'mrnotransaksi, mrtgl, mrkodepa, mrdimintaoleh, mrdimintaolehkontak, mrmintake, mrtgldipakai, 
        'mruraian, mrcatatan, mrnoref, mrtglnoref, mrstatusts, mrstatusrs, mrstatus, 
        'mrstatussebelumnya, mrjmlrevisi, mrcetakanke, mrinputuser, mrinputtgl, mrmodifikasiuser, mrmodifikasitgl, 
        'mrisclose, mrcustomtext1, mrcustomtext2, mrcustomtext3, mrcustomtext4, mrcustomtext5, mrcustomint1, 
        'mrcustomint2, mrcustomint3, mrcustomdbl1, mrcustomdbl2, mrcustomdbl3, mrcustomdate1, mrcustomdate2, 
        'mrcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 43) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'mrid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "mrid required numeric." : GoTo selesai
        End If
        'mrautonotransaksi(6) As Integer
        If (IsNumeric(dataUtama(6)) = False) Then
            result(2) = "mrautonotransaksi required numeric." : GoTo selesai
        End If
        'mrtgl(8) As Date
        If (IsDate(dataUtama(8)) = False) Then
            result(2) = "mrtgl required date." : GoTo selesai
        End If
        'mrkodepa(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "mrkodepa required numeric." : GoTo selesai
        End If
        'mrdimintaoleh(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "mrdimintaoleh required numeric." : GoTo selesai
        End If
        If (dataUtama(10) < 1) Then
            result(2) = "mrdimintaoleh can't be empty." : GoTo selesai
        End If
        'mrmintake(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "mrmintake required numeric." : GoTo selesai
        End If
        'mrtgldipakai(13) As Date
        If (IsDate(dataUtama(13)) = False) Then
            result(2) = "mrtgldipakai required date." : GoTo selesai
        End If
        'mrtglnoref(17) As Date
        If (IsDate(dataUtama(17)) = False) Then
            result(2) = "mrtglnoref required date." : GoTo selesai
        End If
        'mrstatusts(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "mrstatusts required numeric." : GoTo selesai
        End If
        'mrstatusrs(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "mrstatusrs required numeric." : GoTo selesai
        End If
        'mrstatus(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "mrstatus required numeric." : GoTo selesai
        End If
        'mrstatussebelumnya(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "mrstatussebelumnya required numeric." : GoTo selesai
        End If
        'mrjmlrevisi(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "mrjmlrevisi required numeric." : GoTo selesai
        End If
        'mrcetakanke(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "mrcetakanke required numeric." : GoTo selesai
        End If
        'mrinputuser(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "mrinputuser required numeric." : GoTo selesai
        End If
        'mrinputtgl(25) As DateTime
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "mrinputtgl required date." : GoTo selesai
        End If
        'mrmodifikasiuser(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "mrmodifikasiuser required numeric." : GoTo selesai
        End If
        'mrmodifikasitgl(27) As DateTime
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "mrmodifikasitgl required date." : GoTo selesai
        End If
        'mrisclose(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "mrisclose required numeric." : GoTo selesai
        End If
        'mrcustomint1(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "mrcustomint1 required numeric." : GoTo selesai
        End If
        'mrcustomint2(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "mrcustomint2 required numeric." : GoTo selesai
        End If
        'mrcustomint3(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "mrcustomint3 required numeric." : GoTo selesai
        End If
        'mrcustomdbl1(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "mrcustomdbl1 required numeric." : GoTo selesai
        End If
        'mrcustomdbl2(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "mrcustomdbl2 required numeric." : GoTo selesai
        End If
        'mrcustomdbl3(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "mrcustomdbl3 required numeric." : GoTo selesai
        End If
        'mrcustomdate1(40) As Date
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "mrcustomdate1 required date." : GoTo selesai
        End If
        'mrcustomdate2(41) As Date
        If (IsDate(dataUtama(41)) = False) Then
            result(2) = "mrcustomdate2 required date." : GoTo selesai
        End If
        'mrcustomdate3(42) As Date
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "mrcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'mrcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "mrcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "mrcabang should not be more than 25 character." : GoTo selesai
        End If

        'mrlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "mrlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "mrlokasi should not be more than 25 character." : GoTo selesai
        End If

        'mrgudangasal(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "mrgudangasal can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "mrgudangasal should not be more than 25 character." : GoTo selesai
        End If

        'mrgudangtujuan(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "mrgudangtujuan can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 25 Then
            result(2) = "mrgudangtujuan should not be more than 25 character." : GoTo selesai
        End If

        'mrsumber(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "mrsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 10 Then
            result(2) = "mrsumber should not be more than 10 character." : GoTo selesai
        End If

        'mrnotransaksi(7) As String
        If Len(dataUtama(7)) = 0 Then
            result(2) = "mrnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 50 Then
            result(2) = "mrnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'mrtgl(8) As Date
        If Len(dataUtama(8)) = 0 Then
            result(2) = "mrtgl can't be empty" : GoTo selesai
        End If

        'mrtgldipakai(13) As Date
        If Len(dataUtama(13)) = 0 Then
            result(2) = "mrtgldipakai can't be empty" : GoTo selesai
        End If

        'mrtglnoref(17) As Date
        If Len(dataUtama(17)) = 0 Then
            result(2) = "mrtglnoref can't be empty" : GoTo selesai
        End If

        'mrinputtgl(25) As DateTime
        If Len(dataUtama(25)) = 0 Then
            result(2) = "mrinputtgl can't be empty" : GoTo selesai
        End If

        'mrmodifikasitgl(27) As DateTime
        If Len(dataUtama(27)) = 0 Then
            result(2) = "mrmodifikasitgl can't be empty" : GoTo selesai
        End If

        'mrcustomdbl1(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "mrcustomdbl1 can't be empty" : GoTo selesai
        End If

        'mrcustomdbl2(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "mrcustomdbl2 can't be empty" : GoTo selesai
        End If

        'mrcustomdbl3(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "mrcustomdbl3 can't be empty" : GoTo selesai
        End If

        'mrcustomdate1(40) As Date
        If Len(dataUtama(40)) = 0 Then
            result(2) = "mrcustomdate1 can't be empty" : GoTo selesai
        End If

        'mrcustomdate2(41) As Date
        If Len(dataUtama(41)) = 0 Then
            result(2) = "mrcustomdate2 can't be empty" : GoTo selesai
        End If

        'mrcustomdate3(42) As Date
        If Len(dataUtama(42)) = 0 Then
            result(2) = "mrcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "mrid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrgudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrgudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrdimintaoleh", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrdimintaolehkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrmintake", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrtgldipakai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mruraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrstatusts", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrstatusrs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "mrid~mrcabang~mrlokasi~mrgudangasal~mrgudangtujuan~mrsumber~mrautonotransaksi~mrnotransaksi~mrtgl~mrkodepa~mrdimintaoleh~mrdimintaolehkontak~mrmintake~mrtgldipakai~mruraian~mrcatatan~mrnoref~mrtglnoref~mrstatusts~mrstatusrs~mrstatus~mrstatussebelumnya~mrjmlrevisi~mrcetakanke~mrinputuser~mrinputtgl~mrmodifikasiuser~mrmodifikasitgl~mrisclose~mrcustomtext1~mrcustomtext2~mrcustomtext3~mrcustomtext4~mrcustomtext5~mrcustomint1~mrcustomint2~mrcustomint3~mrcustomdbl1~mrcustomdbl2~mrcustomdbl3~mrcustomdate1~mrcustomdate2~mrcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idmrdetail(0) As Integer, idmr(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, hargabeli(12) As Double, hargajual(13) As Double, stokterakhir(14) As Double, 
        'cabang(15) As String, lokasi(16) As String, gudangasal(17) As String, gudangtujuan(18) As String, costcenter(19) As String, 
        'divisi(20) As String, subdivisi(21) As String, proyek(22) As String, catatan(23) As String, urutan(24) As Integer, 
        'jmlts(25) As Double, statusts(26) As Integer, jmlrs(27) As Double, statusrs(28) As Integer, isclose(29) As Integer, 
        'customtext1(30) As String, customtext2(31) As String, customtext3(32) As String, customdbl1(33) As Double, customdbl2(34) As Double, 
        'customdbl3(35) As Double, customdate1(36) As Date, customdate2(37) As Date, customdate3(38) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idmrdetail, idmr, idbarang, namabarang, tipebarang, jml, satuan, 
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
        AsDataTableTambahField(dtdetail, "idmrdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idmr", AsEnumTypeData.AsInt64)
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
            'idmrdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "idmrdetail required numeric." : GoTo selesai
            End If
            'idmr(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idmr required numeric." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idmrdetail~idmr~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~hargabeli~hargajual~stokterakhir~cabang~lokasi~gudangasal~gudangtujuan~costcenter~divisi~subdivisi~proyek~catatan~urutan~jmlts~statusts~jmlrs~statusrs~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

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
                Dim vModuleId As Integer = 3, vMenuId As Integer = 3
                Select Case drutama("mrstatus")
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("mrtgl")), AsFormatTanggal(drutama("mrtgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================

                If isUpdate Then
                    result(4) = drutama("mrid")
                    notransaksi = drutama("mrnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(mrid), mrnotransaksi FROM M3_Mr WHERE mrid='" & result(4) & "' AND mrstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("mrautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("mrcabang"), drutama("mrlokasi"), drutama("mrsumber"), drutama("mrtgl"), drutama("mrsumber"), 3)
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(mrid) FROM m3_mr WHERE mrnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m3_mr_history
                        Dim rsSimpanHistory As String = SimpanHistory.M3_Mr_HistorySimpan("" & paramSplit(0) & "★M3_Mr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("mrsumber")) & "▼" & FixQuotes(drutama("mrid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M3_Mr set mrcabang  = '" & FixQuotes(drutama("mrcabang")) & "', mrlokasi  = '" & FixQuotes(drutama("mrlokasi")) & "', mrgudangasal  = '" & FixQuotes(drutama("mrgudangasal")) & "', mrgudangtujuan  = '" & FixQuotes(drutama("mrgudangtujuan")) & "', mrsumber  = '" & FixQuotes(drutama("mrsumber")) & "', mrautonotransaksi  = " & drutama("mrautonotransaksi") & ", mrnotransaksi  = '" & notransaksi & "', mrtgl  = '" & FixQuotes(AsFormatTanggal(drutama("mrtgl"))) & "', mrkodepa  = " & drutama("mrkodepa") & ", mrdimintaoleh  = " & drutama("mrdimintaoleh") & ", mrdimintaolehkontak  = '" & FixQuotes(drutama("mrdimintaolehkontak")) & "', mrmintake  = " & drutama("mrmintake") & ", mrtgldipakai  = '" & FixQuotes(AsFormatTanggal(drutama("mrtgldipakai"))) & "', mruraian  = '" & FixQuotes(drutama("mruraian")) & "', mrcatatan  = '" & FixQuotes(drutama("mrcatatan")) & "', mrnoref  = '" & FixQuotes(drutama("mrnoref")) & "', mrtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("mrtglnoref"))) & "', mrstatusts  = " & drutama("mrstatusts") & ", mrstatusrs  = " & drutama("mrstatusrs") & ", mrstatus  = " & drutama("mrstatus") & ", mrstatussebelumnya  = " & drutama("mrstatussebelumnya") & ", mrjmlrevisi  = mrjmlrevisi+1, mrcetakanke  = " & drutama("mrcetakanke") & ", mrmodifikasiuser  = " & drutama("mrmodifikasiuser") & ", mrmodifikasitgl  = NOW(), mrcustomtext1  = '" & FixQuotes(drutama("mrcustomtext1")) & "', mrcustomtext2  = '" & FixQuotes(drutama("mrcustomtext2")) & "', mrcustomtext3  = '" & FixQuotes(drutama("mrcustomtext3")) & "', mrcustomtext4  = '" & FixQuotes(drutama("mrcustomtext4")) & "', mrcustomtext5  = '" & FixQuotes(drutama("mrcustomtext5")) & "', mrcustomint1  = " & drutama("mrcustomint1") & ", mrcustomint2  = " & drutama("mrcustomint2") & ", mrcustomint3  = " & drutama("mrcustomint3") & ", mrcustomdbl1  = '" & FixDouble(drutama("mrcustomdbl1")) & "', mrcustomdbl2  = '" & FixDouble(drutama("mrcustomdbl2")) & "', mrcustomdbl3  = '" & FixDouble(drutama("mrcustomdbl3")) & "', mrcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("mrcustomdate1"))) & "', mrcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("mrcustomdate2"))) & "', mrcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("mrcustomdate3"))) & "' where mrid = '" & drutama("mrid") & "'"
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

                    If drutama("mrautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("mrcabang"), drutama("mrlokasi"), drutama("mrsumber"), drutama("mrtgl"), drutama("mrsumber"), 3)
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
                        notransaksi = drutama("mrnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(mrid) FROM m3_mr WHERE mrnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M3_Mr (mrcabang, mrlokasi, mrgudangasal, mrgudangtujuan, mrsumber, mrautonotransaksi, mrnotransaksi, mrtgl, mrkodepa, mrdimintaoleh, mrdimintaolehkontak, mrmintake, mrtgldipakai, mruraian, mrcatatan, mrnoref, mrtglnoref, mrstatusts, mrstatusrs, mrstatus, mrstatussebelumnya, mrjmlrevisi, mrcetakanke, mrinputuser, mrinputtgl, mrmodifikasiuser, mrmodifikasitgl, mrisclose, mrcustomtext1, mrcustomtext2, mrcustomtext3, mrcustomtext4, mrcustomtext5, mrcustomint1, mrcustomint2, mrcustomint3, mrcustomdbl1, mrcustomdbl2, mrcustomdbl3, mrcustomdate1, mrcustomdate2, mrcustomdate3) values('" & FixQuotes(drutama("mrcabang")) & "', '" & FixQuotes(drutama("mrlokasi")) & "', '" & FixQuotes(drutama("mrgudangasal")) & "', '" & FixQuotes(drutama("mrgudangtujuan")) & "', '" & FixQuotes(drutama("mrsumber")) & "', " & drutama("mrautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("mrtgl"))) & "', " & drutama("mrkodepa") & ", " & drutama("mrdimintaoleh") & ", '" & FixQuotes(drutama("mrdimintaolehkontak")) & "', " & drutama("mrmintake") & ", '" & FixQuotes(AsFormatTanggal(drutama("mrtgldipakai"))) & "', '" & FixQuotes(drutama("mruraian")) & "', '" & FixQuotes(drutama("mrcatatan")) & "', '" & FixQuotes(drutama("mrnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrtglnoref"))) & "', " & drutama("mrstatusts") & ", " & drutama("mrstatusrs") & ", " & drutama("mrstatus") & ", " & drutama("mrstatussebelumnya") & ", " & drutama("mrjmlrevisi") & ", " & drutama("mrcetakanke") & ", " & drutama("mrinputuser") & ", NOW(), " & drutama("mrmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("mrisclose") & ", '" & FixQuotes(drutama("mrcustomtext1")) & "', '" & FixQuotes(drutama("mrcustomtext2")) & "', '" & FixQuotes(drutama("mrcustomtext3")) & "', '" & FixQuotes(drutama("mrcustomtext4")) & "', '" & FixQuotes(drutama("mrcustomtext5")) & "', " & drutama("mrcustomint1") & ", " & drutama("mrcustomint2") & ", " & drutama("mrcustomint3") & ", '" & FixDouble(drutama("mrcustomdbl1")) & "', '" & FixDouble(drutama("mrcustomdbl2")) & "', '" & FixDouble(drutama("mrcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select mrid from M3_Mr where mrnotransaksi='" & notransaksi & "' AND Mrinputuser= '" & userid & "' order by Mrmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M3_Mr_Detail where idmr = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idmrdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("hargabeli")) & "', '" & FixDouble(dr1("hargajual")) & "', '" & FixDouble(dr1("stokterakhir")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", '" & FixDouble(dr1("jmlts")) & "', " & dr1("statusts") & ", '" & FixDouble(dr1("jmlrs")) & "', " & dr1("statusrs") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M3_Mr_Detail(idmrdetail, idmr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargabeli, hargajual, stokterakhir, cabang, lokasi, gudangasal, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlts, statusts, jmlrs, statusrs, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "MR", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M3_MrUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("mrdimintaolehkode", "c1.kkode")
            Filter = Filter.Replace("mrdimintaolehnama", "c1.knama")
            Filter = Filter.Replace("mrmintakekode", "c2.kkode")
            Filter = Filter.Replace("mrmintakenama", "c2.knama")
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
            Dim sumber As String = "Mr", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Mrtgl, Mrnotransaksi, Mrstatus FROM m3_Mr WHERE Mrid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Mrstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m3_mr_history
            Dim rsSimpanHistory As String = SimpanHistory.M3_Mr_HistorySimpan("" & paramSplit(0) & "★M3_Mr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                Dim query As New m0_query
                sql = query.m3_mr_terkait("mrid = '" & idtransaksi & "'")
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================
            End If

            'update status utama
            sql = "UPDATE M3_Mr SET Mrstatus = " & nilaiStatus & ", mrmodifikasiuser='" & userid & "', mrmodifikasitgl = NOW(), mrposting = 0, mrpostingtgl = '1971-01-01 00:00:00', mrjmlrevisi = mrjmlrevisi + 1 WHERE mrid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M3_MrSearch(PostWsSearch(paramSplit(0), "M3_MrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M3_MrDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("mrdimintaolehkode", "c1.kkode")
            Filter = Filter.Replace("mrdimintaolehnama", "c1.knama")
            Filter = Filter.Replace("mrmintakekode", "c2.kkode")
            Filter = Filter.Replace("mrmintakenama", "c2.knama")
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
            Dim sumber As String = "Mr", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Mrid, Mrnotransaksi FROM m3_Mr WHERE Mrid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT mrcabang, mrlokasi, mrsumber, mrautonotransaksi, mrnotransaksi, mrtgl"
            sql &= " FROM M3_mr"
            sql &= " WHERE mrid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("mrcabang")
                lokasi = dtNomorNext.Rows(0)("mrlokasi")
                sumber = dtNomorNext.Rows(0)("mrsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("mrautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("mrnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("mrtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M3_Mr_Detail WHERE idmr = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M3_Mr WHERE mrid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M3_MrSearch(PostWsSearch(paramSplit(0), "M3_MrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M3_MrGetdataById(ByVal param As String) As String

        'M3_MrGetdataById Utama --------------------------------------------------------
        'mrid, mrcabang, mrlokasi, mrgudangasal, mrgudangtujuan, mrsumber, mrautonotransaksi, 
        'mrnotransaksi, mrtgl, mrkodepa, mrdimintaoleh, mrdimintaolehkontak, mrmintake, mrtgldipakai, 
        'mruraian, mrcatatan, mrnoref, mrtglnoref, mrstatusts, mrstatusrs, mrstatusrealisasi, 
        'mrstatus, mrstatussebelumnya, mrjmlrevisi, mrcetakanke, mrinputuser, mrinputtgl, mrmodifikasiuser, 
        'mrmodifikasitgl, mrposting, mrpostingtgl, mrisclose, mrcustomtext1, mrcustomtext2, mrcustomtext3, 
        'mrcustomtext4, mrcustomtext5, mrcustomint1, mrcustomint2, mrcustomint3, mrcustomdbl1, mrcustomdbl2, 
        'mrcustomdbl3, mrcustomdate1, mrcustomdate2, mrcustomdate3, mrcabangnama, mrlokasinama, mrgudangasalnama, 
        'mrgudangtujuannama, mrdimintaolehkode, mrdimintaolehnama, mrmintakekode, mrmintakenama, mrstatusnama, mrstatussebelumnyanama, 
        'mrinputusernama, mrmodifikasiusernama

        'M3_MrGetdataById Detail -------------------------------------------------------
        'idmrdetail, idmr, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargabeli, hargajual, 
        'stokterakhir, cabang, lokasi, gudangasal, gudangtujuan, costcenter, divisi, 
        'subdivisi, proyek, catatan, urutan, jmlts, statusts, jmlrs, 
        'statusrs, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, 
        'cabangnama, lokasinama, gudangasalnama, gudangtujuannama, costcenternama, divisinama, subdivisinama, 
        'proyeknama, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan

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

        Dim NmMemcached As String = "aplikasi1-M3_Mr~M3_Mr_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "mrid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "mrid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m3_mr_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                     FxDB(drutama("mrid"), 0), sptField,
                     FxDB(drutama("mrcabang"), ""), sptField,
                     FxDB(drutama("mrlokasi"), ""), sptField,
                     FxDB(drutama("mrgudangasal"), ""), sptField,
                     FxDB(drutama("mrgudangtujuan"), ""), sptField,
                     FxDB(drutama("mrsumber"), ""), sptField,
                     FxDB(drutama("mrautonotransaksi"), 0), sptField,
                     FxDB(drutama("mrnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("mrtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("mrkodepa"), 0), sptField,
                     FxDB(drutama("mrdimintaoleh"), 0), sptField,
                     FxDB(drutama("mrdimintaolehkontak"), ""), sptField,
                     FxDB(drutama("mrmintake"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("mrtgldipakai"), ""), formatTgl), sptField,
                     FxDB(drutama("mruraian"), ""), sptField,
                     FxDB(drutama("mrcatatan"), ""), sptField,
                     FxDB(drutama("mrnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("mrtglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("mrstatusts"), 0), sptField,
                     FxDB(drutama("mrstatusrs"), 0), sptField,
                     FxDB(drutama("mrstatusrealisasi"), 0), sptField,
                     FxDB(drutama("mrstatus"), 0), sptField,
                     FxDB(drutama("mrstatussebelumnya"), 0), sptField,
                     FxDB(drutama("mrjmlrevisi"), 0), sptField,
                     FxDB(drutama("mrcetakanke"), 0), sptField,
                     FxDB(drutama("mrinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("mrinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("mrmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("mrmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("mrposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("mrpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("mrisclose"), 0), sptField,
                     FxDB(drutama("mrcustomtext1"), ""), sptField,
                     FxDB(drutama("mrcustomtext2"), ""), sptField,
                     FxDB(drutama("mrcustomtext3"), ""), sptField,
                     FxDB(drutama("mrcustomtext4"), ""), sptField,
                     FxDB(drutama("mrcustomtext5"), ""), sptField,
                     FxDB(drutama("mrcustomint1"), 0), sptField,
                     FxDB(drutama("mrcustomint2"), 0), sptField,
                     FxDB(drutama("mrcustomint3"), 0), sptField,
                     FxDB(drutama("mrcustomdbl1"), 0), sptField,
                     FxDB(drutama("mrcustomdbl2"), 0), sptField,
                     FxDB(drutama("mrcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("mrcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("mrcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("mrcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("mrcabangnama"), ""), sptField,
                     FxDB(drutama("mrlokasinama"), ""), sptField,
                     FxDB(drutama("mrgudangasalnama"), ""), sptField,
                     FxDB(drutama("mrgudangtujuannama"), ""), sptField,
                     FxDB(drutama("mrdimintaolehkode"), ""), sptField,
                     FxDB(drutama("mrdimintaolehnama"), ""), sptField,
                     FxDB(drutama("mrmintakekode"), ""), sptField,
                     FxDB(drutama("mrmintakenama"), ""), sptField,
                     FxDB(drutama("mrstatusnama"), ""), sptField,
                     FxDB(drutama("mrstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("mrinputusernama"), ""), sptField,
                     FxDB(drutama("mrmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idmrdetail"), 0), sptField,
                     FxDB(dr("idmr"), 0), sptField,
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
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("mrid, mrcabang, mrlokasi, mrgudangasal, mrgudangtujuan, mrsumber, mrautonotransaksi, mrnotransaksi, mrtgl, mrkodepa, mrdimintaoleh, mrdimintaolehkontak, mrmintake, mrtgldipakai, mruraian, mrcatatan, mrnoref, mrtglnoref, mrstatusts, mrstatusrs, mrstatusrealisasi, mrstatus, mrstatussebelumnya, mrjmlrevisi, mrcetakanke, mrinputuser, mrinputtgl, mrmodifikasiuser, mrmodifikasitgl, mrposting, mrpostingtgl, mrisclose, mrcustomtext1, mrcustomtext2, mrcustomtext3, mrcustomtext4, mrcustomtext5, mrcustomint1, mrcustomint2, mrcustomint3, mrcustomdbl1, mrcustomdbl2, mrcustomdbl3, mrcustomdate1, mrcustomdate2, mrcustomdate3, mrcabangnama, mrlokasinama, mrgudangasalnama, mrgudangtujuannama, mrdimintaolehkode, mrdimintaolehnama, mrmintakekode, mrmintakenama, mrstatusnama, mrstatussebelumnyanama, mrinputusernama, mrmodifikasiusernama" & sptSubParam & "idmrdetail, idmr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargabeli, hargajual, stokterakhir, cabang, lokasi, gudangasal, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlts, statusts, jmlrs, statusrs, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, cabangnama, lokasinama, gudangasalnama, gudangtujuannama, costcenternama, divisinama, subdivisinama, proyeknama, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_MrSearch(ByVal param As String) As String
        'M3_MrSearch --------------------------------------------------------
        'mrid, mrcabang, mrlokasi, mrgudangasal, mrgudangtujuan, mrsumber, mrautonotransaksi, 
        'mrnotransaksi, mrtgl, mrkodepa, mrdimintaoleh, mrdimintaolehkontak, mrmintake, mrtgldipakai, 
        'mruraian, mrcatatan, mrnoref, mrtglnoref, mrstatusts, mrstatusrs, mrstatusrealisasi, 
        'mrstatus, mrstatussebelumnya, mrjmlrevisi, mrcetakanke, mrinputuser, mrinputtgl, mrmodifikasiuser, 
        'mrmodifikasitgl, mrposting, mrpostingtgl, mrisclose, mrcabangnama, mrlokasinama, mrgudangasalnama, 
        'mrgudangtujuannama, mrdimintaolehkode, mrdimintaolehnama, mrmintakekode, mrmintakenama, mrstatusnama, mrstatussebelumnyanama, 
        'mrinputusernama, mrmodifikasiusernama

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
            Filter = Filter.Replace("mrdimintaolehkode", "c1.kkode")
            Filter = Filter.Replace("mrdimintaolehnama", "c1.knama")
            Filter = Filter.Replace("mrmintakekode", "c2.kkode")
            Filter = Filter.Replace("mrmintakenama", "c2.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m3_mr_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M3_Mr", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("mrid"), 0), sptField,
                     FxDB(dr("mrcabang"), ""), sptField,
                     FxDB(dr("mrlokasi"), ""), sptField,
                     FxDB(dr("mrgudangasal"), ""), sptField,
                     FxDB(dr("mrgudangtujuan"), ""), sptField,
                     FxDB(dr("mrsumber"), ""), sptField,
                     FxDB(dr("mrautonotransaksi"), 0), sptField,
                     FxDB(dr("mrnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("mrtgl"), ""), formatTgl), sptField,
                     FxDB(dr("mrkodepa"), 0), sptField,
                     FxDB(dr("mrdimintaoleh"), 0), sptField,
                     FxDB(dr("mrdimintaolehkontak"), ""), sptField,
                     FxDB(dr("mrmintake"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("mrtgldipakai"), ""), formatTgl), sptField,
                     FxDB(dr("mruraian"), ""), sptField,
                     FxDB(dr("mrcatatan"), ""), sptField,
                     FxDB(dr("mrnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("mrtglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("mrstatusts"), 0), sptField,
                     FxDB(dr("mrstatusrs"), 0), sptField,
                     FxDB(dr("mrstatusrealisasi"), 0), sptField,
                     FxDB(dr("mrstatus"), 0), sptField,
                     FxDB(dr("mrstatussebelumnya"), 0), sptField,
                     FxDB(dr("mrjmlrevisi"), 0), sptField,
                     FxDB(dr("mrcetakanke"), 0), sptField,
                     FxDB(dr("mrinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("mrinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("mrmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("mrmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("mrposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("mrpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("mrisclose"), 0), sptField,
                     FxDB(dr("mrcabangnama"), ""), sptField,
                     FxDB(dr("mrlokasinama"), ""), sptField,
                     FxDB(dr("mrgudangasalnama"), ""), sptField,
                     FxDB(dr("mrgudangtujuannama"), ""), sptField,
                     FxDB(dr("mrdimintaolehkode"), ""), sptField,
                     FxDB(dr("mrdimintaolehnama"), ""), sptField,
                     FxDB(dr("mrmintakekode"), ""), sptField,
                     FxDB(dr("mrmintakenama"), ""), sptField,
                     FxDB(dr("mrstatusnama"), ""), sptField,
                     FxDB(dr("mrstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("mrinputusernama"), ""), sptField,
                     FxDB(dr("mrmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("mrid, mrcabang, mrlokasi, mrgudangasal, mrgudangtujuan, mrsumber, mrautonotransaksi, mrnotransaksi, mrtgl, mrkodepa, mrdimintaoleh, mrdimintaolehkontak, mrmintake, mrtgldipakai, mruraian, mrcatatan, mrnoref, mrtglnoref, mrstatusts, mrstatusrs, mrstatusrealisasi, mrstatus, mrstatussebelumnya, mrjmlrevisi, mrcetakanke, mrinputuser, mrinputtgl, mrmodifikasiuser, mrmodifikasitgl, mrposting, mrpostingtgl, mrisclose, mrcabangnama, mrlokasinama, mrgudangasalnama, mrgudangtujuannama, mrdimintaolehkode, mrdimintaolehnama, mrmintakekode, mrmintakenama, mrstatusnama, mrstatussebelumnyanama, mrinputusernama, mrmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_Mr_Detail_VSearch(ByVal param As String) As String
        'M3_Mr_Detail_VSearch --------------------------------------------------------
        'idmrdetail, idmr, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargabeli, hargajual, 
        'stokterakhir, cabang, lokasi, gudangasal, gudangtujuan, costcenter, divisi, 
        'subdivisi, proyek, catatan, urutan, jmlts, statusts, jmlrs, 
        'statusrs, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, mrnotransaksi, 
        'kodebarang, bhpp, bhppaverage, bjenis, bserial, bbatch, jmlsisats, 
        'jmlsisars, jmlsisarealisasi, bapanjang, balebar, batinggi, btagpermintaanmutasi, btagmutasicabang, 
        'bjmllapangan, bsatuanlapangan, basset

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
            Filter = Filter.Replace("btagjual", "ip.ipjual")
            Filter = Filter.Replace("btagmutasipusat", "ip.ipmutasipusat")
            Filter = Filter.Replace("btagpermintaanmutasi", "ip.ippermintaanmutasi")
            Filter = Filter.Replace("btagmutasicabang", "ip.ipmutasicabang")
            Filter = Filter.Replace("btagretursupplier", "ip.ipretursupplier")
            Filter = Filter.Replace("btagpermintaanpembelian", "ip.ippermintaanpembelian")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m3_mr_detail_v")
        sql = "select `mrd`.`idmrdetail` AS `idmrdetail`, `mrd`.`idmr` AS `idmr`, `mrd`.`idbarang` AS `idbarang`, `mrd`.`namabarang` AS `namabarang`, `mrd`.`tipebarang` AS `tipebarang`, `mrd`.`jml` AS `jml`, `mrd`.`satuan` AS `satuan`, `mrd`.`nilaisatuan` AS `nilaisatuan`, `mrd`.`jmlbarang` AS `jmlbarang`, `mrd`.`satuanbarang` AS `satuanbarang`, `mrd`.`matauang` AS `matauang`, `mrd`.`kurs` AS `kurs`, `mrd`.`hargabeli` AS `hargabeli`, `mrd`.`hargajual` AS `hargajual`, `mrd`.`stokterakhir` AS `stokterakhir`, `mrd`.`cabang` AS `cabang`, `mrd`.`lokasi` AS `lokasi`, `mrd`.`gudangasal` AS `gudangasal`, `mrd`.`gudangtujuan` AS `gudangtujuan`, `mrd`.`costcenter` AS `costcenter`, `mrd`.`divisi` AS `divisi`, `mrd`.`subdivisi` AS `subdivisi`, `mrd`.`proyek` AS `proyek`, `mrd`.`catatan` AS `catatan`, `mrd`.`urutan` AS `urutan`, `mrd`.`jmlts` AS `jmlts`, `mrd`.`statusts` AS `statusts`, `mrd`.`jmlrs` AS `jmlrs`, `mrd`.`statusrs` AS `statusrs`, `mrd`.`jmlrealisasi` AS `jmlrealisasi`, `mrd`.`statusrealisasi` AS `statusrealisasi`, `mrd`.`isclose` AS `isclose`, `mrd`.`customtext1` AS `customtext1`, `mrd`.`customtext2` AS `customtext2`, `mrd`.`customtext3` AS `customtext3`, `mrd`.`customdbl1` AS `customdbl1`, `mrd`.`customdbl2` AS `customdbl2`, `mrd`.`customdbl3` AS `customdbl3`, `mrd`.`customdate1` AS `customdate1`, `mrd`.`customdate2` AS `customdate2`, `mrd`.`customdate3` AS `customdate3`, `mr`.`mrnotransaksi` AS `mrnotransaksi`, `i`.`bkode` AS `kodebarang`, `i`.`bhpp` AS `bhpp`, `i`.`bhppaverage` AS `bhppaverage`, `i`.`bjenis` AS `bjenis`, `i`.`bserial` AS `bserial`, `i`.`bbatch` AS `bbatch`, ((`mrd`.`jmlbarang` - `mrd`.`jmlts`) / `mrd`.`nilaisatuan`) AS `jmlsisats`, ((`mrd`.`jmlbarang` - `mrd`.`jmlrs`) / `mrd`.`nilaisatuan`) AS `jmlsisars`, ((`mrd`.`jmlbarang` - `mrd`.`jmlrealisasi`) / `mrd`.`nilaisatuan`) AS `jmlsisarealisasi`,  i.bapanjang,  i.balebar,  i.batinggi,`i`.`btag` AS `btag`,ip.ipjual AS btagjual, ip.ipmutasipusat AS btagmutasipusat, ip.ippermintaanmutasi AS btagpermintaanmutasi ,ip.ipmutasicabang AS btagmutasicabang, ip.ipretursupplier AS btagretursupplier, ip.ippermintaanpembelian AS btagpermintaanpembelian, i.bjmllapangan, i.bsatuanlapangan, i.basset from ((`m3_mr_detail` `mrd` join `m3_mr` `mr` on((`mrd`.`idmr` = `mr`.`mrid`))) join `m1_item` `i` on((`mrd`.`idbarang` = `i`.`bid`)) join `m1_item_permission` `ip` on((`i`.`btag` = `ip`.`ipkode`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M3_Mr_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idmrdetail"), 0), sptField,
                     FxDB(dr("idmr"), 0), sptField,
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
                     FxDB(dr("mrnotransaksi"), ""), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bhppaverage"), 0), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("jmlsisats"), 0), sptField,
                     FxDB(dr("jmlsisars"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("btag"), ""), sptField,
                     FxDB(dr("btagjual"), 0), sptField,
                     FxDB(dr("btagmutasipusat"), 0), sptField,
                     FxDB(dr("btagpermintaanmutasi"), 0), sptField,
                     FxDB(dr("btagmutasicabang"), 0), sptField,
                     FxDB(dr("btagretursupplier"), 0), sptField,
                     FxDB(dr("btagpermintaanpembelian"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptField,
                     FxDB(dr("basset"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idmrdetail, idmr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargabeli, hargajual, stokterakhir, cabang, lokasi, gudangasal, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlts, statusts, jmlrs, statusrs, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, mrnotransaksi, kodebarang, bhpp, bhppaverage, bjenis, bserial, bbatch, jmlsisats, jmlsisars, jmlsisarealisasi, bapanjang, balebar, batinggi, btag , btagjual, btagmutasipusat, btagpermintaanmutasi, btagmutasicabang, btagretursupplier, btagpermintaanpembelian, bjmllapangan, bsatuanlapangan, basset"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_MrTerkait(ByVal param As String) As String
        'M3_MrTerkait --------------------------------------------------------
        'mrid, mrnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
        'modifikasitglterkait, jenisterkait

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), ismrev(2), countPage(3), countRow(4)

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
            result(2) = "mrid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2) & " AND mrid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "mrid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
            Sorting = ""
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.m3_mr_terkait(Filter)

        dt = AmbilData("aplikasi1-M3_Mr_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("mrid"), 0), sptField,
                     FxDB(dr("mrnotransaksi"), ""), sptField,
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
            result(2) = "Related MR data not found. "
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("mrid, mrnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_MrSimpanOld(ByVal param As String) As String
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
        'mrid(0) As Integer, mrcabang(1) As String, mrlokasi(2) As String, mrgudangasal(3) As String, mrgudangtujuan(4) As String, 
        'mrsumber(5) As String, mrautonotransaksi(6) As Integer, mrnotransaksi(7) As String, mrtgl(8) As Date, mrkodepa(9) As Integer, 
        'mrdimintaoleh(10) As Integer, mrdimintaolehkontak(11) As String, mrmintake(12) As Integer, mrtgldipakai(13) As Date, mruraian(14) As String, 
        'mrcatatan(15) As String, mrnoref(16) As String, mrtglnoref(17) As Date, mrstatusts(18) As Integer, mrstatusrs(19) As Integer, 
        'mrstatus(20) As Integer, mrstatussebelumnya(21) As Integer, mrjmlrevisi(22) As Integer, mrcetakanke(23) As Integer, mrinputuser(24) As Integer, 
        'mrinputtgl(25) As DateTime, mrmodifikasiuser(26) As Integer, mrmodifikasitgl(27) As DateTime, mrisclose(28) As Integer, mrcustomtext1(29) As String, 
        'mrcustomtext2(30) As String, mrcustomtext3(31) As String, mrcustomtext4(32) As String, mrcustomtext5(33) As String, mrcustomint1(34) As Integer, 
        'mrcustomint2(35) As Integer, mrcustomint3(36) As Integer, mrcustomdbl1(37) As Double, mrcustomdbl2(38) As Double, mrcustomdbl3(39) As Double, 
        'mrcustomdate1(40) As Date, mrcustomdate2(41) As Date, mrcustomdate3(42) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'mrid, mrcabang, mrlokasi, mrgudangasal, mrgudangtujuan, mrsumber, mrautonotransaksi, 
        'mrnotransaksi, mrtgl, mrkodepa, mrdimintaoleh, mrdimintaolehkontak, mrmintake, mrtgldipakai, 
        'mruraian, mrcatatan, mrnoref, mrtglnoref, mrstatusts, mrstatusrs, mrstatus, 
        'mrstatussebelumnya, mrjmlrevisi, mrcetakanke, mrinputuser, mrinputtgl, mrmodifikasiuser, mrmodifikasitgl, 
        'mrisclose, mrcustomtext1, mrcustomtext2, mrcustomtext3, mrcustomtext4, mrcustomtext5, mrcustomint1, 
        'mrcustomint2, mrcustomint3, mrcustomdbl1, mrcustomdbl2, mrcustomdbl3, mrcustomdate1, mrcustomdate2, 
        'mrcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 43) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'mrid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "mrid required numeric." : GoTo selesai
        End If
        'mrautonotransaksi(6) As Integer
        If (IsNumeric(dataUtama(6)) = False) Then
            result(2) = "mrautonotransaksi required numeric." : GoTo selesai
        End If
        'mrtgl(8) As Date
        If (IsDate(dataUtama(8)) = False) Then
            result(2) = "mrtgl required date." : GoTo selesai
        End If
        'mrkodepa(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "mrkodepa required numeric." : GoTo selesai
        End If
        'mrdimintaoleh(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "mrdimintaoleh required numeric." : GoTo selesai
        End If
        If (dataUtama(10) < 1) Then
            result(2) = "mrdimintaoleh can't be empty." : GoTo selesai
        End If
        'mrmintake(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "mrmintake required numeric." : GoTo selesai
        End If
        'mrtgldipakai(13) As Date
        If (IsDate(dataUtama(13)) = False) Then
            result(2) = "mrtgldipakai required date." : GoTo selesai
        End If
        'mrtglnoref(17) As Date
        If (IsDate(dataUtama(17)) = False) Then
            result(2) = "mrtglnoref required date." : GoTo selesai
        End If
        'mrstatusts(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "mrstatusts required numeric." : GoTo selesai
        End If
        'mrstatusrs(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "mrstatusrs required numeric." : GoTo selesai
        End If
        'mrstatus(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "mrstatus required numeric." : GoTo selesai
        End If
        'mrstatussebelumnya(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "mrstatussebelumnya required numeric." : GoTo selesai
        End If
        'mrjmlrevisi(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "mrjmlrevisi required numeric." : GoTo selesai
        End If
        'mrcetakanke(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "mrcetakanke required numeric." : GoTo selesai
        End If
        'mrinputuser(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "mrinputuser required numeric." : GoTo selesai
        End If
        'mrinputtgl(25) As DateTime
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "mrinputtgl required date." : GoTo selesai
        End If
        'mrmodifikasiuser(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "mrmodifikasiuser required numeric." : GoTo selesai
        End If
        'mrmodifikasitgl(27) As DateTime
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "mrmodifikasitgl required date." : GoTo selesai
        End If
        'mrisclose(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "mrisclose required numeric." : GoTo selesai
        End If
        'mrcustomint1(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "mrcustomint1 required numeric." : GoTo selesai
        End If
        'mrcustomint2(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "mrcustomint2 required numeric." : GoTo selesai
        End If
        'mrcustomint3(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "mrcustomint3 required numeric." : GoTo selesai
        End If
        'mrcustomdbl1(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "mrcustomdbl1 required numeric." : GoTo selesai
        End If
        'mrcustomdbl2(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "mrcustomdbl2 required numeric." : GoTo selesai
        End If
        'mrcustomdbl3(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "mrcustomdbl3 required numeric." : GoTo selesai
        End If
        'mrcustomdate1(40) As Date
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "mrcustomdate1 required date." : GoTo selesai
        End If
        'mrcustomdate2(41) As Date
        If (IsDate(dataUtama(41)) = False) Then
            result(2) = "mrcustomdate2 required date." : GoTo selesai
        End If
        'mrcustomdate3(42) As Date
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "mrcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'mrcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "mrcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "mrcabang should not be more than 25 character." : GoTo selesai
        End If

        'mrlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "mrlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "mrlokasi should not be more than 25 character." : GoTo selesai
        End If

        'mrgudangasal(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "mrgudangasal can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "mrgudangasal should not be more than 25 character." : GoTo selesai
        End If

        'mrgudangtujuan(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "mrgudangtujuan can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 25 Then
            result(2) = "mrgudangtujuan should not be more than 25 character." : GoTo selesai
        End If

        'mrsumber(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "mrsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 10 Then
            result(2) = "mrsumber should not be more than 10 character." : GoTo selesai
        End If

        'mrnotransaksi(7) As String
        If Len(dataUtama(7)) = 0 Then
            result(2) = "mrnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 50 Then
            result(2) = "mrnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'mrtgl(8) As Date
        If Len(dataUtama(8)) = 0 Then
            result(2) = "mrtgl can't be empty" : GoTo selesai
        End If

        'mrtgldipakai(13) As Date
        If Len(dataUtama(13)) = 0 Then
            result(2) = "mrtgldipakai can't be empty" : GoTo selesai
        End If

        'mrtglnoref(17) As Date
        If Len(dataUtama(17)) = 0 Then
            result(2) = "mrtglnoref can't be empty" : GoTo selesai
        End If

        'mrinputtgl(25) As DateTime
        If Len(dataUtama(25)) = 0 Then
            result(2) = "mrinputtgl can't be empty" : GoTo selesai
        End If

        'mrmodifikasitgl(27) As DateTime
        If Len(dataUtama(27)) = 0 Then
            result(2) = "mrmodifikasitgl can't be empty" : GoTo selesai
        End If

        'mrcustomdbl1(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "mrcustomdbl1 can't be empty" : GoTo selesai
        End If

        'mrcustomdbl2(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "mrcustomdbl2 can't be empty" : GoTo selesai
        End If

        'mrcustomdbl3(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "mrcustomdbl3 can't be empty" : GoTo selesai
        End If

        'mrcustomdate1(40) As Date
        If Len(dataUtama(40)) = 0 Then
            result(2) = "mrcustomdate1 can't be empty" : GoTo selesai
        End If

        'mrcustomdate2(41) As Date
        If Len(dataUtama(41)) = 0 Then
            result(2) = "mrcustomdate2 can't be empty" : GoTo selesai
        End If

        'mrcustomdate3(42) As Date
        If Len(dataUtama(42)) = 0 Then
            result(2) = "mrcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "mrid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrgudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrgudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrdimintaoleh", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrdimintaolehkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrmintake", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrtgldipakai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mruraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrstatusts", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrstatusrs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "mrid~mrcabang~mrlokasi~mrgudangasal~mrgudangtujuan~mrsumber~mrautonotransaksi~mrnotransaksi~mrtgl~mrkodepa~mrdimintaoleh~mrdimintaolehkontak~mrmintake~mrtgldipakai~mruraian~mrcatatan~mrnoref~mrtglnoref~mrstatusts~mrstatusrs~mrstatus~mrstatussebelumnya~mrjmlrevisi~mrcetakanke~mrinputuser~mrinputtgl~mrmodifikasiuser~mrmodifikasitgl~mrisclose~mrcustomtext1~mrcustomtext2~mrcustomtext3~mrcustomtext4~mrcustomtext5~mrcustomint1~mrcustomint2~mrcustomint3~mrcustomdbl1~mrcustomdbl2~mrcustomdbl3~mrcustomdate1~mrcustomdate2~mrcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idmrdetail(0) As Integer, idmr(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, hargabeli(12) As Double, hargajual(13) As Double, stokterakhir(14) As Double, 
        'cabang(15) As String, lokasi(16) As String, gudangasal(17) As String, gudangtujuan(18) As String, costcenter(19) As String, 
        'divisi(20) As String, subdivisi(21) As String, proyek(22) As String, catatan(23) As String, urutan(24) As Integer, 
        'jmlts(25) As Double, statusts(26) As Integer, jmlrs(27) As Double, statusrs(28) As Integer, isclose(29) As Integer, 
        'customtext1(30) As String, customtext2(31) As String, customtext3(32) As String, customdbl1(33) As Double, customdbl2(34) As Double, 
        'customdbl3(35) As Double, customdate1(36) As Date, customdate2(37) As Date, customdate3(38) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idmrdetail, idmr, idbarang, namabarang, tipebarang, jml, satuan, 
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
        AsDataTableTambahField(dtdetail, "idmrdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idmr", AsEnumTypeData.AsInt64)
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
            'idmrdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "idmrdetail required numeric." : GoTo selesai
            End If
            'idmr(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idmr required numeric." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idmrdetail~idmr~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~hargabeli~hargajual~stokterakhir~cabang~lokasi~gudangasal~gudangtujuan~costcenter~divisi~subdivisi~proyek~catatan~urutan~jmlts~statusts~jmlrs~statusrs~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38)) = False Then
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

                ''CEK PERIODE AKUNTANSI ==================================
                'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("mrtgl")), AsFormatTanggal(drutama("mrtgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================

                If isUpdate Then
                    result(4) = drutama("mrid")
                    notransaksi = drutama("mrnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(mrid), mrnotransaksi FROM M3_Mr WHERE mrid='" & result(4) & "' AND mrstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(mrid) FROM m3_mr WHERE mrnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m3_mr_history
                        Dim rsSimpanHistory As String = SimpanHistory.M3_Mr_HistorySimpan("" & paramSplit(0) & "★M3_Mr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("mrsumber")) & "▼" & FixQuotes(drutama("mrid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M3_Mr set mrcabang  = '" & FixQuotes(drutama("mrcabang")) & "', mrlokasi  = '" & FixQuotes(drutama("mrlokasi")) & "', mrgudangasal  = '" & FixQuotes(drutama("mrgudangasal")) & "', mrgudangtujuan  = '" & FixQuotes(drutama("mrgudangtujuan")) & "', mrsumber  = '" & FixQuotes(drutama("mrsumber")) & "', mrautonotransaksi  = " & drutama("mrautonotransaksi") & ", mrnotransaksi  = '" & notransaksi & "', mrtgl  = '" & FixQuotes(AsFormatTanggal(drutama("mrtgl"))) & "', mrkodepa  = " & drutama("mrkodepa") & ", mrdimintaoleh  = " & drutama("mrdimintaoleh") & ", mrdimintaolehkontak  = '" & FixQuotes(drutama("mrdimintaolehkontak")) & "', mrmintake  = " & drutama("mrmintake") & ", mrtgldipakai  = '" & FixQuotes(AsFormatTanggal(drutama("mrtgldipakai"))) & "', mruraian  = '" & FixQuotes(drutama("mruraian")) & "', mrcatatan  = '" & FixQuotes(drutama("mrcatatan")) & "', mrnoref  = '" & FixQuotes(drutama("mrnoref")) & "', mrtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("mrtglnoref"))) & "', mrstatusts  = " & drutama("mrstatusts") & ", mrstatusrs  = " & drutama("mrstatusrs") & ", mrstatus  = " & drutama("mrstatus") & ", mrstatussebelumnya  = " & drutama("mrstatussebelumnya") & ", mrjmlrevisi  = mrjmlrevisi+1, mrcetakanke  = " & drutama("mrcetakanke") & ", mrmodifikasiuser  = " & drutama("mrmodifikasiuser") & ", mrmodifikasitgl  = NOW(), mrcustomtext1  = '" & FixQuotes(drutama("mrcustomtext1")) & "', mrcustomtext2  = '" & FixQuotes(drutama("mrcustomtext2")) & "', mrcustomtext3  = '" & FixQuotes(drutama("mrcustomtext3")) & "', mrcustomtext4  = '" & FixQuotes(drutama("mrcustomtext4")) & "', mrcustomtext5  = '" & FixQuotes(drutama("mrcustomtext5")) & "', mrcustomint1  = " & drutama("mrcustomint1") & ", mrcustomint2  = " & drutama("mrcustomint2") & ", mrcustomint3  = " & drutama("mrcustomint3") & ", mrcustomdbl1  = '" & FixDouble(drutama("mrcustomdbl1")) & "', mrcustomdbl2  = '" & FixDouble(drutama("mrcustomdbl2")) & "', mrcustomdbl3  = '" & FixDouble(drutama("mrcustomdbl3")) & "', mrcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("mrcustomdate1"))) & "', mrcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("mrcustomdate2"))) & "', mrcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("mrcustomdate3"))) & "' where mrid = '" & drutama("mrid") & "'"
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

                    If drutama("mrautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("mrcabang"), drutama("mrlokasi"), drutama("mrsumber"), drutama("mrtgl"))
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
                        notransaksi = drutama("mrnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(mrid) FROM m3_mr WHERE mrnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M3_Mr (mrcabang, mrlokasi, mrgudangasal, mrgudangtujuan, mrsumber, mrautonotransaksi, mrnotransaksi, mrtgl, mrkodepa, mrdimintaoleh, mrdimintaolehkontak, mrmintake, mrtgldipakai, mruraian, mrcatatan, mrnoref, mrtglnoref, mrstatusts, mrstatusrs, mrstatus, mrstatussebelumnya, mrjmlrevisi, mrcetakanke, mrinputuser, mrinputtgl, mrmodifikasiuser, mrmodifikasitgl, mrisclose, mrcustomtext1, mrcustomtext2, mrcustomtext3, mrcustomtext4, mrcustomtext5, mrcustomint1, mrcustomint2, mrcustomint3, mrcustomdbl1, mrcustomdbl2, mrcustomdbl3, mrcustomdate1, mrcustomdate2, mrcustomdate3) values('" & FixQuotes(drutama("mrcabang")) & "', '" & FixQuotes(drutama("mrlokasi")) & "', '" & FixQuotes(drutama("mrgudangasal")) & "', '" & FixQuotes(drutama("mrgudangtujuan")) & "', '" & FixQuotes(drutama("mrsumber")) & "', " & drutama("mrautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("mrtgl"))) & "', " & drutama("mrkodepa") & ", " & drutama("mrdimintaoleh") & ", '" & FixQuotes(drutama("mrdimintaolehkontak")) & "', " & drutama("mrmintake") & ", '" & FixQuotes(AsFormatTanggal(drutama("mrtgldipakai"))) & "', '" & FixQuotes(drutama("mruraian")) & "', '" & FixQuotes(drutama("mrcatatan")) & "', '" & FixQuotes(drutama("mrnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrtglnoref"))) & "', " & drutama("mrstatusts") & ", " & drutama("mrstatusrs") & ", " & drutama("mrstatus") & ", " & drutama("mrstatussebelumnya") & ", " & drutama("mrjmlrevisi") & ", " & drutama("mrcetakanke") & ", " & drutama("mrinputuser") & ", NOW(), " & drutama("mrmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("mrisclose") & ", '" & FixQuotes(drutama("mrcustomtext1")) & "', '" & FixQuotes(drutama("mrcustomtext2")) & "', '" & FixQuotes(drutama("mrcustomtext3")) & "', '" & FixQuotes(drutama("mrcustomtext4")) & "', '" & FixQuotes(drutama("mrcustomtext5")) & "', " & drutama("mrcustomint1") & ", " & drutama("mrcustomint2") & ", " & drutama("mrcustomint3") & ", '" & FixDouble(drutama("mrcustomdbl1")) & "', '" & FixDouble(drutama("mrcustomdbl2")) & "', '" & FixDouble(drutama("mrcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select mrid from M3_Mr where mrnotransaksi='" & notransaksi & "' AND Mrinputuser= '" & userid & "' order by Mrmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M3_Mr_Detail where idmr = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idmrdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("hargabeli")) & "', '" & FixDouble(dr1("hargajual")) & "', '" & FixDouble(dr1("stokterakhir")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", '" & FixDouble(dr1("jmlts")) & "', " & dr1("statusts") & ", '" & FixDouble(dr1("jmlrs")) & "', " & dr1("statusrs") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M3_Mr_Detail(idmrdetail, idmr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargabeli, hargajual, stokterakhir, cabang, lokasi, gudangasal, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlts, statusts, jmlrs, statusrs, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                Dim sumber As String = "MR", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M3_MrUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("mrdimintaolehkode", "c1.kkode")
            Filter = Filter.Replace("mrdimintaolehnama", "c1.knama")
            Filter = Filter.Replace("mrmintakekode", "c2.kkode")
            Filter = Filter.Replace("mrmintakenama", "c2.knama")
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
            Dim sumber As String = "Mr", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Mrtgl, Mrnotransaksi, Mrstatus FROM m3_Mr WHERE Mrid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Mrstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m3_mr_history
            Dim rsSimpanHistory As String = SimpanHistory.M3_Mr_HistorySimpan("" & paramSplit(0) & "★M3_Mr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                Dim query As New m0_query
                sql = query.m3_mr_terkait("mrid = '" & idtransaksi & "'")
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================
            End If

            'update status utama
            sql = "UPDATE M3_Mr SET Mrstatus = " & nilaiStatus & ", mrmodifikasiuser='" & userid & "', mrmodifikasitgl = NOW(), mrposting = 0, mrpostingtgl = '1971-01-01 00:00:00', mrjmlrevisi = mrjmlrevisi + 1 WHERE mrid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M3_MrSearch(PostWsSearch(paramSplit(0), "M3_MrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M3_MrDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("mrdimintaolehkode", "c1.kkode")
            Filter = Filter.Replace("mrdimintaolehnama", "c1.knama")
            Filter = Filter.Replace("mrmintakekode", "c2.kkode")
            Filter = Filter.Replace("mrmintakenama", "c2.knama")
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
            Dim sumber As String = "Mr", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Mrid, Mrnotransaksi FROM m3_Mr WHERE Mrid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT mrcabang, mrlokasi, mrsumber, mrautonotransaksi, mrnotransaksi, mrtgl"
            sql &= " FROM M3_mr"
            sql &= " WHERE mrid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("mrcabang")
                lokasi = dtNomorNext.Rows(0)("mrlokasi")
                sumber = dtNomorNext.Rows(0)("mrsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("mrautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("mrnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("mrtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M3_Mr_Detail WHERE idmr = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M3_Mr WHERE mrid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M3_MrSearch(PostWsSearch(paramSplit(0), "M3_MrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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