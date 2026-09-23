Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class wsm3_sa
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M3_SaSimpan(ByVal param As String) As String
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
        Dim Filter As String = "", Sorting As String = ""
        Dim vCabang As String = "", vLokasi As String = "", vGudang As String = ""

        'Dim cekBatch As Boolean

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


        'CEK NOREFF ========================================================================
        'CEK NOREFF UNTUK UPLOAD DATA POS, JIKA NOREFF TERISI MAKA CEK DATA YANG SUDAH ADA DI TABEL
        'JIKA NOREFF SUDAH ADA MAKA BERI KEMBALIAN BERHASIL
        'JIKA NOREF TIDAK ADA MAKA JALANKAN PROSES SIMPAN
        If Len(Filter) > 0 Then
            sql = "SELECT said, sanotransaksi FROM m3_sa WHERE sanoref = '" & FixQuotes(Filter) & "'"
            Dim dtNoreff As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNoreff.Rows.Count > 0 Then
                If Len(dtNoreff.Rows(0)("said")) > 0 Then
                    result(1) = 1
                    result(2) = dtNoreff.Rows(0)("sanotransaksi")
                    result(3) = 0
                    result(4) = dtNoreff.Rows(0)("said")
                    GoTo selesai
                End If
            End If

        Else
            Dim validKey As RsValidKey
            validKey = ValidateKey(paramSplit(0))
            If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        End If
        'END OF CEK NOREFF =================================================================


        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 4 And dataSplit.Length <> 5) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'said(0) As Integer, sacabang(1) As String, salokasi(2) As String, sagudang(3) As String, sasumber(4) As String, 
        'sajenis(5) As String, saautonotransaksi(6) As Integer, sanotransaksi(7) As String, satgl(8) As Date, sakodepa(9) As Integer, 
        'sabagiansa(10) As Integer, sabagiansakontak(11) As String, sauraian(12) As String, sacatatan(13) As String, sanoref(14) As String, 
        'satglnoref(15) As Date, saidsp(16) As Integer, sastatus(17) As Integer, sastatussebelumnya(18) As Integer, sajmlrevisi(19) As Integer, 
        'sacetakanke(20) As Integer, sainputuser(21) As Integer, sainputtgl(22) As DateTime, samodifikasiuser(23) As Integer, samodifikasitgl(24) As DateTime, 
        'saposting(25) As Integer, satutupperiode(26) As Integer, saisclose(27) As Integer, sacustomtext1(28) As String, sacustomtext2(29) As String, 
        'sacustomtext3(30) As String, sacustomtext4(31) As String, sacustomtext5(32) As String, sacustomint1(33) As Integer, sacustomint2(34) As Integer, 
        'sacustomint3(35) As Integer, sacustomdbl1(36) As Double, sacustomdbl2(37) As Double, sacustomdbl3(38) As Double, sacustomdate1(39) As Date, 
        'sacustomdate2(40) As Date, sacustomdate3(41) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'said, sacabang, salokasi, sagudang, sasumber, sajenis, saautonotransaksi, 
        'sanotransaksi, satgl, sakodepa, sabagiansa, sabagiansakontak, sauraian, sacatatan, 
        'sanoref, satglnoref, saidsp, sastatus, sastatussebelumnya, sajmlrevisi, sacetakanke, 
        'sainputuser, sainputtgl, samodifikasiuser, samodifikasitgl, saposting, satutupperiode, saisclose, 
        'sacustomtext1, sacustomtext2, sacustomtext3, sacustomtext4, sacustomtext5, sacustomint1, sacustomint2, 
        'sacustomint3, sacustomdbl1, sacustomdbl2, sacustomdbl3, sacustomdate1, sacustomdate2, sacustomdate3


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 42) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'said(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "said required numeric." : GoTo selesai
        End If
        'saautonotransaksi(6) As Integer
        If (IsNumeric(dataUtama(6)) = False) Then
            result(2) = "saautonotransaksi required numeric." : GoTo selesai
        End If
        'satgl(8) As Date
        If (IsDate(dataUtama(8)) = False) Then
            result(2) = "satgl required date." : GoTo selesai
        End If
        'sakodepa(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "sakodepa required numeric." : GoTo selesai
        End If
        'sabagiansa(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "sabagiansa required numeric." : GoTo selesai
        End If
        If (dataUtama(10) < 1) Then
            result(2) = "sabagiansa can't be empty." : GoTo selesai
        End If
        'satglnoref(15) As Date
        If (IsDate(dataUtama(15)) = False) Then
            result(2) = "satglnoref required date." : GoTo selesai
        End If
        'saidsp(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "saidsp required numeric." : GoTo selesai
        End If
        'sastatus(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "sastatus required numeric." : GoTo selesai
        End If
        'sastatussebelumnya(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "sastatussebelumnya required numeric." : GoTo selesai
        End If
        'sajmlrevisi(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "sajmlrevisi required numeric." : GoTo selesai
        End If
        'sacetakanke(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "sacetakanke required numeric." : GoTo selesai
        End If
        'sainputuser(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "sainputuser required numeric." : GoTo selesai
        End If
        'sainputtgl(22) As DateTime
        If (IsDate(dataUtama(22)) = False) Then
            result(2) = "sainputtgl required date." : GoTo selesai
        End If
        'samodifikasiuser(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "samodifikasiuser required numeric." : GoTo selesai
        End If
        'samodifikasitgl(24) As DateTime
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "samodifikasitgl required date." : GoTo selesai
        End If
        'saposting(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "saposting required numeric." : GoTo selesai
        End If
        'satutupperiode(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "satutupperiode required numeric." : GoTo selesai
        End If
        'saisclose(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "saisclose required numeric." : GoTo selesai
        End If
        'sacustomint1(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "sacustomint1 required numeric." : GoTo selesai
        End If
        'sacustomint2(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "sacustomint2 required numeric." : GoTo selesai
        End If
        'sacustomint3(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "sacustomint3 required numeric." : GoTo selesai
        End If
        'sacustomdbl1(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "sacustomdbl1 required numeric." : GoTo selesai
        End If
        'sacustomdbl2(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "sacustomdbl2 required numeric." : GoTo selesai
        End If
        'sacustomdbl3(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "sacustomdbl3 required numeric." : GoTo selesai
        End If
        'sacustomdate1(39) As Date
        If (IsDate(dataUtama(39)) = False) Then
            result(2) = "sacustomdate1 required date." : GoTo selesai
        End If
        'sacustomdate2(40) As Date
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "sacustomdate2 required date." : GoTo selesai
        End If
        'sacustomdate3(41) As Date
        If (IsDate(dataUtama(41)) = False) Then
            result(2) = "sacustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'sacabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "sacabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "sacabang should not be more than 25 character." : GoTo selesai
        End If
        vCabang = dataUtama(1)

        'salokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "salokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "salokasi should not be more than 25 character." : GoTo selesai
        End If
        vLokasi = dataUtama(2)

        'sagudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "sagudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "sagudang should not be more than 25 character." : GoTo selesai
        End If
        vGudang = dataUtama(3)

        'sasumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "sasumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "sasumber should not be more than 10 character." : GoTo selesai
        End If

        'sanotransaksi(7) As String
        If Len(dataUtama(7)) = 0 Then
            result(2) = "sanotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 50 Then
            result(2) = "sanotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'satgl(8) As Date
        If Len(dataUtama(8)) = 0 Then
            result(2) = "satgl can't be empty" : GoTo selesai
        End If

        'satglnoref(15) As Date
        If Len(dataUtama(15)) = 0 Then
            result(2) = "satglnoref can't be empty" : GoTo selesai
        End If

        'sainputtgl(22) As DateTime
        If Len(dataUtama(22)) = 0 Then
            result(2) = "sainputtgl can't be empty" : GoTo selesai
        End If

        'samodifikasitgl(24) As DateTime
        If Len(dataUtama(24)) = 0 Then
            result(2) = "samodifikasitgl can't be empty" : GoTo selesai
        End If

        'sacustomdbl1(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "sacustomdbl1 can't be empty" : GoTo selesai
        End If

        'sacustomdbl2(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "sacustomdbl2 can't be empty" : GoTo selesai
        End If

        'sacustomdbl3(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "sacustomdbl3 can't be empty" : GoTo selesai
        End If

        'sacustomdate1(39) As Date
        If Len(dataUtama(39)) = 0 Then
            result(2) = "sacustomdate1 can't be empty" : GoTo selesai
        End If

        'sacustomdate2(40) As Date
        If Len(dataUtama(40)) = 0 Then
            result(2) = "sacustomdate2 can't be empty" : GoTo selesai
        End If

        'sacustomdate3(41) As Date
        If Len(dataUtama(41)) = 0 Then
            result(2) = "sacustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "said", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sacabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "salokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sagudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sasumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sajenis", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "saautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sanotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "satgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sakodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sabagiansa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sabagiansakontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sauraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sacatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sanoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "satglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "saidsp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sastatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sastatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sajmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sacetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sainputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sainputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "samodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "samodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "saposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "satutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "saisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sacustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sacustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sacustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sacustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sacustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sacustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sacustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sacustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sacustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sacustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sacustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sacustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sacustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sacustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "said~sacabang~salokasi~sagudang~sasumber~sajenis~saautonotransaksi~sanotransaksi~satgl~sakodepa~sabagiansa~sabagiansakontak~sauraian~sacatatan~sanoref~satglnoref~saidsp~sastatus~sastatussebelumnya~sajmlrevisi~sacetakanke~sainputuser~sainputtgl~samodifikasiuser~samodifikasitgl~saposting~satutupperiode~saisclose~sacustomtext1~sacustomtext2~sacustomtext3~sacustomtext4~sacustomtext5~sacustomint1~sacustomint2~sacustomint3~sacustomdbl1~sacustomdbl2~sacustomdbl3~sacustomdate1~sacustomdate2~sacustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idsadetail(0) As Integer, idsa(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jmlmasuk(5) As Double, jmlkeluar(6) As Double, satuan(7) As String, nilaisatuan(8) As Double, jmlbarangmasuk(9) As Double, 
        'jmlbarangkeluar(10) As Double, satuanbarang(11) As String, idhppkhususmasuk(12) As Integer, hpplama(13) As Double, hpp(14) As Double, 
        'rekpersediaan(15) As String, reklawan(16) As String, idspdetail(17) As Integer, cabang(18) As String, lokasi(19) As String, 
        'gudang(20) As String, costcenter(21) As String, divisi(22) As String, subdivisi(23) As String, proyek(24) As String, 
        'catatan(25) As String, urutan(26) As Integer, isclose(27) As Integer, customtext1(28) As String, customtext2(29) As String, 
        'customtext3(30) As String, customdbl1(31) As Double, customdbl2(32) As Double, customdbl3(33) As Double, customdate1(34) As Date, 
        'customdate2(35) As Date, customdate3(36) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idsadetail, idsa, idbarang, namabarang, tipebarang, jmlmasuk, jmlkeluar, 
        'satuan, nilaisatuan, jmlbarangmasuk, jmlbarangkeluar, satuanbarang, idhppkhususmasuk, hpplama, 
        'hpp, rekpersediaan, reklawan, idspdetail, cabang, lokasi, gudang, 
        'costcenter, divisi, subdivisi, proyek, catatan, urutan, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idsadetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idsa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlmasuk", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlkeluar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarangmasuk", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmlbarangkeluar", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idhppkhususmasuk", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "hpplama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hpp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekpersediaan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "reklawan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idspdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudang", AsEnumTypeData.AsString)
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

        'Variable ValidasiBatchSerial
        Dim ftBarangIn As String = "", ftBarangOut As String = "", ftBarangBatchHapus As String = ""

        'Variabel Hpp
        Dim ftBarang As String = ""

        'Variabel ValidasiSimpan
        Dim ftExistOutstanding As String = "", ftOutstanding As String = "", ftExistStok As String = "", ftStokAvailable As String = ""
        Dim updNilai As String = "", updFilter As String = "", gudang As String = ""
        Dim ftStokAvailableCase As String = ""
        'Dim ftExistOutstandingSO As String = "", ftOutstandingSO As String = ""
        'Dim updNilaiSO As String = "", updFilterSO As String = ""
        Dim idbarang As Integer = 0, idspdetail As Integer = 0, jmlbarangMasuk As Double = 0, jmlbarangKeluar As Double = 0, jmlbarang As Double = 0
        Dim isPlus As Boolean = False, jenismutasi As Double = 0 ', nobatch As String
        'Dim idsodetail As Double = 0

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 37) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idsadetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idsadetail required numeric." : GoTo selesai
            End If
            'idsa(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idsa required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'jmlmasuk(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - jmlmasuk required numeric." : GoTo selesai
            End If
            'jmlkeluar(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - jmlkeluar required numeric." : GoTo selesai
            End If
            'nilaisatuan(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarangmasuk(9) As Double
            'jmlbarangmasuk = jmlmasuk * nilaisatuan
            dataRowDetail(9) = Double.Parse(dataRowDetail(5)) * Double.Parse(dataRowDetail(8))
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - jmlbarangmasuk required numeric." : GoTo selesai
            End If
            'jmlbarangkeluar(10) As Double
            'jmlbarangkeluar = jmlkeluar * nilaisatuan
            dataRowDetail(10) = Double.Parse(dataRowDetail(6)) * Double.Parse(dataRowDetail(8))
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - jmlbarangkeluar required numeric." : GoTo selesai
            End If
            'idhppkhususmasuk(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - idhppkhususmasuk required numeric." : GoTo selesai
            End If
            'hpplama(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - hpplama required numeric." : GoTo selesai
            End If
            'hpp(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - hpp required numeric." : GoTo selesai
            End If
            'idspdetail(17) As Integer
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - idspdetail required numeric." : GoTo selesai
            End If
            'urutan(26) As Integer
            If (IsNumeric(dataRowDetail(26)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'isclose(27) As Integer
            If (IsNumeric(dataRowDetail(27)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(31) As Double
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(32) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(33) As Double
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(34) As Date
            If (IsDate(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(35) As Date
            If (IsDate(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(36) As Date
            If (IsDate(dataRowDetail(36)) = False) Then
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

            'jmlmasuk(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - jmlmasuk can't be empty" : GoTo selesai
            End If
            If dataRowDetail(5) < 0 Then
                result(2) = "Row : " & i & " - jmlmasuk can't be less than zero" : GoTo selesai
            End If

            'jmlkeluar(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - jmlkeluar can't be empty" : GoTo selesai
            End If
            If dataRowDetail(6) < 0 Then
                result(2) = "Row : " & i & " - jmlkeluar can't be less than zero" : GoTo selesai
            End If

            'JIKA sacustomint1 = 0 (PENYESUAIAN STOK), JIKA sacustomint1 = 1 (PENYESUAIAN HPP)
            If dataUtama(33) = 0 Then
                'jmlmasuk dan jmlkeluar tidak boleh keduanya diisi, harus salah satu
                If Double.Parse(dataRowDetail(5)) <> 0 And Double.Parse(dataRowDetail(6)) <> 0 Then
                    result(2) = "Row : " & i & " - jmlmasuk and jmlkeluar can't be filled in both." : GoTo selesai
                ElseIf Double.Parse(dataRowDetail(5)) = 0 And Double.Parse(dataRowDetail(6)) = 0 Then
                    result(2) = "Row : " & i & " - jmlmasuk and jmlkeluar can't be zero." : GoTo selesai
                End If
            End If


            'satuan(7) As String
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(7)) > 25 Then
                result(2) = "Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'nilaisatuan(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmlbarangmasuk(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarangmasuk can't be empty" : GoTo selesai
            End If
            If dataRowDetail(9) < 0 Then
                result(2) = "Row : " & i & " - jmlbarangmasuk can't be less than zero" : GoTo selesai
            End If

            'jmlbarangkeluar(10) As Double
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarangkeluar can't be empty" : GoTo selesai
            End If
            If dataRowDetail(10) < 0 Then
                result(2) = "Row : " & i & " - jmlbarangkeluar can't be less than zero" : GoTo selesai
            End If

            'JIKA sacustomint1 = 0 (PENYESUAIAN STOK), JIKA sacustomint1 = 1 (PENYESUAIAN HPP)
            If dataUtama(33) = 0 Then
                'jmlbarangmasuk dan jmlbarangkeluar tidak boleh keduanya diisi, harus salah satu
                If Double.Parse(dataRowDetail(9)) <> 0 And Double.Parse(dataRowDetail(10)) <> 0 Then
                    result(2) = "Row : " & i & " - jmlbarangmasuk and jmlbarangkeluar can't be filled in both." : GoTo selesai
                ElseIf Double.Parse(dataRowDetail(9)) = 0 And Double.Parse(dataRowDetail(10)) = 0 Then
                    result(2) = "Row : " & i & " - jmlbarangmasuk and jmlbarangkeluar can't be zero." : GoTo selesai
                End If
            End If

            'satuanbarang(11) As String
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(11)) > 25 Then
                result(2) = "Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
            End If

            'hpplama(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - hpplama can't be empty" : GoTo selesai
            End If

            'hpp(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - hpp can't be empty" : GoTo selesai
            End If
            'If Double.Parse(dataRowDetail(9)) <> 0 And dataRowDetail(14) <= 0 Then
            '    result(2) = "Row : " & i & " - hpp can't be less than or equal to zero" : GoTo selesai
            'End If
            If Double.Parse(dataRowDetail(9)) <> 0 And dataRowDetail(14) <= 0 Then
                If Not dataRowDetail(3).ToLower.Contains("maklon") And Not dataRowDetail(3).ToLower.Contains("hedging") Then
                    result(2) = "Row : " & i & " - hpp can't be less than or equal to zero" : GoTo selesai
                End If
            End If

            'rekpersediaan(15) As String
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - rekpersediaan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(15)) > 25 Then
                result(2) = "Row : " & i & " - rekpersediaan should not be more than 25 character." : GoTo selesai
            End If

            'reklawan(16) As String
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - reklawan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(16)) > 25 Then
                result(2) = "Row : " & i & " - reklawan should not be more than 25 character." : GoTo selesai
            End If

            'cabang(18) As String
            dataRowDetail(18) = vCabang

            'lokasi(19) As String
            dataRowDetail(19) = vLokasi

            'gudang(20) As String
            dataRowDetail(20) = vGudang
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - gudang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(20)) > 25 Then
                result(2) = "Row : " & i & " - gudang should not be more than 25 character." : GoTo selesai
            End If

            'customdbl1(31) As Double
            If Len(dataRowDetail(31)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(32) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(33) As Double
            If Len(dataRowDetail(33)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(34) As Date
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(35) As Date
            If Len(dataRowDetail(35)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(36) As Date
            If Len(dataRowDetail(36)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idsadetail~idsa~idbarang~namabarang~tipebarang~jmlmasuk~jmlkeluar~satuan~nilaisatuan~jmlbarangmasuk~jmlbarangkeluar~satuanbarang~idhppkhususmasuk~hpplama~hpp~rekpersediaan~reklawan~idspdetail~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'If (dataRowDetail(9) > 0) Then
            '    If (i = 1) Then
            '        dataSplit(2) = ""
            '    End If
            'End If
            ''Cek APAKAH NO BATCH SUDAH TERISI ATAU BELUM
            'cekBatch = False
            'If (Len(dataRowDetail(28)) > 0) Then
            '    cekBatch = True
            'End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarangmasuk(9) As Double                     , jmlbarangkeluar(10) As Double                     , gudang(20) As String       , idspdetail(17) As Integer
            idbarang = dataRowDetail(2) : jmlbarangMasuk = Double.Parse(dataRowDetail(9)) : jmlbarangKeluar = Double.Parse(dataRowDetail(10)) : gudang = dataRowDetail(20) : idspdetail = dataRowDetail(17)
            'customdbl2(32) As Double
            'idsodetail = dataRowDetail(32)

            'ValidasiHpp
            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")

            ftBarangBatchHapus = IIf(Len(ftBarangBatchHapus.ToString) = 0, "", ftBarangBatchHapus & ", ")
            ftBarangBatchHapus = String.Concat(ftBarangBatchHapus, "'" & idbarang & "'")

            'ValidasiBatchSerial
            If jmlbarangMasuk > 0 Then
                'JIKA BARANG MASUK MAKA FILTER BATCH DAN SERIAL MASUK
                ftBarangIn = IIf(Len(ftBarangIn.ToString) = 0, "", ftBarangIn & " OR ")
                ftBarangIn = String.Concat(ftBarangIn, "(bid = '" & idbarang & "')")

            ElseIf jmlbarangKeluar > 0 Then
                'JIKA BARANG KELUAR MAKA FILTER BATCH DAN SERIAL KELUAR
                ftBarangOut = IIf(Len(ftBarangOut.ToString) = 0, "", ftBarangOut & " OR ")
                ftBarangOut = String.Concat(ftBarangOut, "(bid = '" & idbarang & "')")
            End If

            'VALIDASI OUTSTANDING -------------------------
            If idspdetail <> 0 Then
                '1. CEK DATA EXIST
                ftExistOutstanding = IIf(Len(ftExistOutstanding.ToString) = 0, "", ftExistOutstanding & " UNION ")
                ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m3_sp_detail JOIN m3_sp ON idsp = spid WHERE idspdetail = '" & idspdetail & "' AND (spstatus = 2 OR spstatus = 3 OR spstatus = 4 OR spstatus = 7) LIMIT 1) as rowExists, '" & idspdetail & "' as idspdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
                'ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m3_sp_detail JOIN m3_sp ON idsp = spid WHERE idspdetail = '" & idspdetail & "' AND (spstatus = 2 OR spstatus = 3) LIMIT 1) as rowExists, '" & idspdetail & "' as idspdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING
                Dim OutstandingMasuk As Double = AsDataTableDSum(dtdetail, "jmlbarangmasuk", "idspdetail=" & idspdetail)
                Dim OutstandingKeluar As Double = AsDataTableDSum(dtdetail, "jmlbarangkeluar", "idspdetail=" & idspdetail)
                Dim Outstanding As Double = Math.Abs(OutstandingMasuk - OutstandingKeluar)
                ftOutstanding = IIf(Len(ftOutstanding.ToString) = 0, "", ftOutstanding & " OR ")
                ftOutstanding = String.Concat(ftOutstanding, " (spd.idspdetail = " & idspdetail & " AND " & Outstanding & " > (ABS(spd.selisihbarang) - spd.jmlsa)) ")

                '3. SET NILAI UPDATE OUTSTANDING
                updNilai = String.Concat("WHEN '" & idspdetail & "' THEN ROUND(jmlsa + '" & Outstanding & "', 5) ", updNilai)

                '4. SET FILTER UPDATE OUTSTANDING
                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                updFilter = String.Concat(updFilter, "(idspdetail = '" & idspdetail & "')")
            End If

            'If idsodetail <> 0 Then
            '    '1. CEK DATA EXIST
            '    ftExistOutstandingSO = IIf(Len(ftExistOutstandingSO.ToString) = 0, "", ftExistOutstandingSO & " UNION ")
            '    ftExistOutstandingSO = String.Concat(ftExistOutstandingSO, "SELECT EXISTS(SELECT 1 FROM m5_so_detail JOIN m5_so ON idso = soid WHERE idsodetail = '" & idsodetail & "' AND (sostatus = 2 OR sostatus = 3 OR sostatus = 4 OR sostatus = 7) LIMIT 1) as rowExists, '" & idsodetail & "' as idsodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

            '    '2. CEK JML OUTSTANDING
            '    Dim OutstandingMasuk As Double = AsDataTableDSum(dtdetail, "jmlbarangmasuk", "customdbl2=" & idsodetail)
            '    Dim OutstandingKeluar As Double = AsDataTableDSum(dtdetail, "jmlbarangkeluar", "customdbl2=" & idsodetail)
            '    Dim Outstanding As Double = Math.Abs(OutstandingKeluar - OutstandingMasuk)
            '    ftOutstandingSO = IIf(Len(ftOutstandingSO.ToString) = 0, "", ftOutstandingSO & " OR ")
            '    ftOutstandingSO = String.Concat(ftOutstandingSO, " (sod.idsodetail = " & idsodetail & " AND " & Outstanding & " > ((sod.jmlbarang) - sod.customdbl2)) ")

            '    '3. SET NILAI UPDATE OUTSTANDING
            '    updNilaiSO = String.Concat("WHEN '" & idsodetail & "' THEN ROUND(customdbl2 + '" & Outstanding & "', 5) ", updNilaiSO)

            '    '4. SET FILTER UPDATE OUTSTANDING
            '    updFilterSO = IIf(Len(updFilterSO.ToString) = 0, "", updFilterSO & " OR ")
            '    updFilterSO = String.Concat(updFilterSO, "(idsodetail = '" & idsodetail & "')")
            'End If

            'VALIDASI STOK -------------------------------
            '1. CEK TRANSAKSI STOK MASUK/KELUAR
            Dim StokMasuk As Double = AsDataTableDSum(dtdetail, "jmlbarangmasuk", "idbarang=" & idbarang & " AND gudang='" & gudang & "'")
            Dim StokKeluar As Double = AsDataTableDSum(dtdetail, "jmlbarangkeluar", "idbarang=" & idbarang & " AND gudang='" & gudang & "'")
            Dim Stok As Double = StokMasuk - StokKeluar

            If Stok > -1 Then isPlus = True Else isPlus = False
            Stok = Math.Abs(Stok)

            '   'JIKA STOK KELUAR
            If isPlus = False Then
                '2. CEK DATA EXIST STOK KELUAR
                ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                'ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudang & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudang & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")
                ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudang & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudang & "' as gudang, 0 as stoktersedia, '" & Stok & "' as stokjual FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                '3. CEK JML STOK KELUAR
                ftStokAvailable = IIf(Len(ftStokAvailable.ToString) = 0, "", ftStokAvailable & " OR ")
                ftStokAvailable = String.Concat(ftStokAvailable, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudang & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")

                ftStokAvailableCase = String.Concat("WHEN isw.idbarang = " & idbarang & " AND isw.kgudang = '" & gudang & "' THEN " & Stok & " ", ftStokAvailableCase)

            End If
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

            ''nbtid, nbtjenismutasi, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
            ''nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
            ''nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, nbtgudang, nbtidbatchin
            'dataSplit(2) += "▲0▼1▼" & idbarang & "▼" & dataRowDetail(28) & "▼SA▼0▼" & dataRowDetail(7) & "▼" & jmlbarangMasuk & "▼▼▼▼" & dataRowDetail(31) & "▼" & dataRowDetail(32) & "▼" & dataRowDetail(33) & "▼" & dataRowDetail(34) & "▼" & dataRowDetail(35) & "▼" & dataRowDetail(36) & "▼" & dataRowDetail(20) & "▼0"

        Next

        'dataSplit(2) = dataSplit(2).Substring(1)


        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


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
                    'result(2) = JmlDtBatch : GoTo selesai
                End If

                ''GROUPING JIKA BARANG DAN KODE SAMA
                'Dim jmlb As Integer = dataDetail.Length
                'Dim a As Integer = 0
                'For a = 1 To jmlb
                '    dataRowDetail = dataDetail(a - 1).Split(sptField)
                '    If (dataRowBatch(2) = dataRowDetail(3)) Then
                '        If (dataRowBatch(3) = dataRowDetail(28)) Then
                '            result(2) = "a" : GoTo selesai
                '        End If
                '    End If
                'Next

                'END OF VALIDASI DAN SET DATA ROW BATCH ----------------------------

                'VALIDASI TIPE DATA BATCH ------------------------------------------

                If ftBarangBatchHapus.Contains("'" & dataRowBatch(2) & "'") Then

                    'nbtid(0) As Integer
                    If (IsNumeric(dataRowBatch(0)) = False) Then
                        result(2) = "Batch Row : " & i & " - nbtid required numeric." : GoTo selesai
                    End If
                    'nbtjenismutasi(1) As Integer
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
                    'nbtjenismutasi(1) As Integer
                    jenismutasi = dataRowBatch(1)
                    'nbtidbarang(2) As Integer , nbtkode(3) As String      , nbtjml(7) As Double         , nbtgudang(17) As String      , nbtidbatchin(18) As Integer
                    idbarang = dataRowBatch(2) : nbtkode = dataRowBatch(3) : jmlbarang = dataRowBatch(7) : nbtgudang = dataRowBatch(17) : nbtidbatchin = dataRowBatch(18)

                    'VALIDASI HANYA UNTUK BARANG KELUAR SAJA
                    If jenismutasi = 0 Then
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
                    End If

                    'END OF BUAT FILTER UNTUK VALIDASI --------------------------

                End If

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
                'nstjenismutasi(1) As Integer
                jenismutasi = dataRowSerial(1)
                'nstidbarang(2) As Integer  , nstkode(3) As String       , nstjml(7) As Double          , nstgudang(17) As String       , nstidserialin(18) As Integer
                idbarang = dataRowSerial(2) : nstkode = dataRowSerial(3) : jmlbarang = dataRowSerial(7) : nstgudang = dataRowSerial(17) : nstidserialin = dataRowSerial(18)


                'VALIDASI HANYA UNTUK BARANG KELUAR SAJA
                If jenismutasi = 0 Then
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
                End If
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
                vStatus = drutama("sastatus")
                vTgl = AsFormatTanggal(drutama("satgl"))

                'CEK HAK AKSES STATUS ============================
                Dim vAkses As Integer = 0, msgAkses As String = ""
                'MODUL DAN MENU HARUS DISESUAIKAN
                Dim vModuleId As Integer = 3, vMenuId As Integer = 6
                Select Case drutama("sastatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("satgl")), AsFormatTanggal(drutama("satgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================

                'VALIDASI SIMPAN ========================================
                If drutama("sastatus") = 2 Or drutama("sastatus") = 1 Or drutama("sastatus") = 8 Or drutama("sastatus") = 9 Or drutama("sastatus") = 10 Or drutama("sastatus") = 11 Then

                    Dim rsValidasi As String

                    'VALIDASI BARANG IN ---------------
                    'ValidasiBatchSerial
                    If Len(ftBarangIn) > 0 Then
                        rsValidasi = ValidasiBatchSerial(dtdetail, dtbatch, dtserial, ftBarangIn, "jmlbarangmasuk", 1)
                        If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai

                        'ValidasiAsset
                        rsValidasi = ValidasiAsset(dtdetail, dtasset, ftBarangIn, "jmlbarangmasuk", 1)
                        If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    End If
                    'END OF VALIDASI BARANG IN --------

                    'VALIDASI BARANG OUT ---------------
                    'ValidasiBatchSerial
                    If Len(ftBarangOut) > 0 Then
                        rsValidasi = ValidasiBatchSerial(dtdetail, dtbatch, dtserial, ftBarangOut, "jmlbarangkeluar", 0)
                        If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai

                        'ValidasiAsset
                        rsValidasi = ValidasiAsset(dtdetail, dtasset, ftBarangOut, "jmlbarangkeluar", 0)
                        If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai

                        'ValidasiGudangAsset
                        rsValidasi = ValidasiGudangAsset(dtasset, gudang, 0)
                        If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    End If
                    'END OF VALIDASI BARANG OUT --------

                    If Len(ftBarangOut) > 0 Then
                        'ValidasiHppI
                        rsValidasi = ValidasiHppI(dtdetail, ftBarangOut)
                        If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai

                        ''ValidasiHppF
                        'rsValidasi = ValidasiHppF(dtdetail, ftBarangOut)
                        'If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                    End If

                    'ValidasiSimpan
                    'rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstanding, ftOutstanding, ftExistOutstandingSO, ftOutstandingSO, ftExistStok, "", ftStokAvailable, "", "", ftExistBatch, ftBatch, ftExistSerial, ftSerial, "gudang")
                    rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstanding, ftOutstanding, ftExistStok, "", ftStokAvailable, "", "", ftExistBatch, ftBatch, ftExistSerial, ftSerial, "gudang", ftStokAvailableCase)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================

                If isUpdate Then
                    result(4) = drutama("said")
                    notransaksi = drutama("sanotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(said), sanotransaksi FROM M3_sa WHERE said='" & result(4) & "' AND sastatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("saautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("sacabang"), drutama("salokasi"), drutama("sasumber"), drutama("satgl"), drutama("sasumber"), 3)
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(said) FROM m3_sa WHERE sanotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m3_sa_history
                        Dim rsSimpanHistory As String = SimpanHistory.M3_Sa_HistorySimpan("" & paramSplit(0) & "★M3_Sa_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("sasumber")) & "▼" & FixQuotes(drutama("said")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M3_Sa set sacabang  = '" & FixQuotes(drutama("sacabang")) & "', salokasi  = '" & FixQuotes(drutama("salokasi")) & "', sagudang  = '" & FixQuotes(drutama("sagudang")) & "', sasumber  = '" & FixQuotes(drutama("sasumber")) & "', sajenis  = '" & FixQuotes(drutama("sajenis")) & "', saautonotransaksi  = " & drutama("saautonotransaksi") & ", sanotransaksi  = '" & notransaksi & "', satgl  = '" & FixQuotes(AsFormatTanggal(drutama("satgl"))) & "', sakodepa  = " & drutama("sakodepa") & ", sabagiansa  = " & drutama("sabagiansa") & ", sabagiansakontak  = '" & FixQuotes(drutama("sabagiansakontak")) & "', sauraian  = '" & FixQuotes(drutama("sauraian")) & "', sacatatan  = '" & FixQuotes(drutama("sacatatan")) & "', sanoref  = '" & FixQuotes(drutama("sanoref")) & "', satglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("satglnoref"))) & "', saidsp  = " & drutama("saidsp") & ", sastatus  = " & drutama("sastatus") & ", sastatussebelumnya  = " & drutama("sastatussebelumnya") & ", sajmlrevisi  = sajmlrevisi+1, sacetakanke  = " & drutama("sacetakanke") & ", samodifikasiuser  = " & drutama("samodifikasiuser") & ", samodifikasitgl  = NOW(), saposting  = 0, satutupperiode  = " & drutama("satutupperiode") & ", sacustomtext1  = '" & FixQuotes(drutama("sacustomtext1")) & "', sacustomtext2  = '" & FixQuotes(drutama("sacustomtext2")) & "', sacustomtext3  = '" & FixQuotes(drutama("sacustomtext3")) & "', sacustomtext4  = '" & FixQuotes(drutama("sacustomtext4")) & "', sacustomtext5  = '" & FixQuotes(drutama("sacustomtext5")) & "', sacustomint1  = " & drutama("sacustomint1") & ", sacustomint2  = " & drutama("sacustomint2") & ", sacustomint3  = " & drutama("sacustomint3") & ", sacustomdbl1  = '" & FixDouble(drutama("sacustomdbl1")) & "', sacustomdbl2  = '" & FixDouble(drutama("sacustomdbl2")) & "', sacustomdbl3  = '" & FixDouble(drutama("sacustomdbl3")) & "', sacustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("sacustomdate1"))) & "', sacustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("sacustomdate2"))) & "', sacustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("sacustomdate3"))) & "' where said = '" & drutama("said") & "'"
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

                    If drutama("saautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("sacabang"), drutama("salokasi"), drutama("sasumber"), drutama("satgl"), drutama("sasumber"), 3)
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
                        notransaksi = drutama("sanotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(said) FROM m3_sa WHERE sanotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M3_Sa (sacabang, salokasi, sagudang, sasumber, sajenis, saautonotransaksi, sanotransaksi, satgl, sakodepa, sabagiansa, sabagiansakontak, sauraian, sacatatan, sanoref, satglnoref, saidsp, sastatus, sastatussebelumnya, sajmlrevisi, sacetakanke, sainputuser, sainputtgl, samodifikasiuser, samodifikasitgl, saposting, satutupperiode, saisclose, sacustomtext1, sacustomtext2, sacustomtext3, sacustomtext4, sacustomtext5, sacustomint1, sacustomint2, sacustomint3, sacustomdbl1, sacustomdbl2, sacustomdbl3, sacustomdate1, sacustomdate2, sacustomdate3) values('" & FixQuotes(drutama("sacabang")) & "', '" & FixQuotes(drutama("salokasi")) & "', '" & FixQuotes(drutama("sagudang")) & "', '" & FixQuotes(drutama("sasumber")) & "', '" & FixQuotes(drutama("sajenis")) & "', " & drutama("saautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("satgl"))) & "', " & drutama("sakodepa") & ", " & drutama("sabagiansa") & ", '" & FixQuotes(drutama("sabagiansakontak")) & "', '" & FixQuotes(drutama("sauraian")) & "', '" & FixQuotes(drutama("sacatatan")) & "', '" & FixQuotes(drutama("sanoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("satglnoref"))) & "', " & drutama("saidsp") & ", " & drutama("sastatus") & ", " & drutama("sastatussebelumnya") & ", " & drutama("sajmlrevisi") & ", " & drutama("sacetakanke") & ", " & drutama("sainputuser") & ", NOW(), " & drutama("samodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("satutupperiode") & ", " & drutama("saisclose") & ", '" & FixQuotes(drutama("sacustomtext1")) & "', '" & FixQuotes(drutama("sacustomtext2")) & "', '" & FixQuotes(drutama("sacustomtext3")) & "', '" & FixQuotes(drutama("sacustomtext4")) & "', '" & FixQuotes(drutama("sacustomtext5")) & "', " & drutama("sacustomint1") & ", " & drutama("sacustomint2") & ", " & drutama("sacustomint3") & ", '" & FixDouble(drutama("sacustomdbl1")) & "', '" & FixDouble(drutama("sacustomdbl2")) & "', '" & FixDouble(drutama("sacustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sacustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sacustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sacustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select said from M3_sa where sanotransaksi='" & notransaksi & "' AND sainputuser= '" & userid & "' order by samodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M3_Sa_Detail where idsa = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idsadetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jmlmasuk")) & "', '" & FixDouble(dr1("jmlkeluar")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarangmasuk")) & "', '" & FixDouble(dr1("jmlbarangkeluar")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', " & dr1("idhppkhususmasuk") & ", '" & FixDouble(dr1("hpplama")) & "', '" & FixDouble(dr1("hpp")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("reklawan")) & "', " & dr1("idspdetail") & ", '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M3_Sa_Detail(idsadetail, idsa, idbarang, namabarang, tipebarang, jmlmasuk, jmlkeluar, satuan, nilaisatuan, jmlbarangmasuk, jmlbarangkeluar, satuanbarang, idhppkhususmasuk, hpplama, hpp, rekpersediaan, reklawan, idspdetail, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                    sql = "Delete from M1_No_Batch_Transaction where nbtidtransaksi  = '" & result(4) & "' AND nbtsumber = 'SA'"
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
                    sql = "Delete from M1_No_Serial_Transaction  where nstidtransaksi  = '" & result(4) & "' AND nstsumber = 'SA'"
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
                    sql = "Delete from M7_Asset_Transaction where atidutama  = '" & result(4) & "' AND atsumber = 'SA'"
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


                If drutama("sastatus") = 2 Then
                    If Len(updNilai) > 0 Then
                        'UPDATE OUTSTANDING TRANSAKSI ===================================================
                        'UPDATE DETAIL
                        sql = "UPDATE m3_sp_detail SET jmlsa = (CASE idspdetail " & updNilai & " ELSE jmlsa END) WHERE " & updFilter
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE UTAMA
                        Dim updUtama As String = "", statusOut As Integer = 0
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idsp, SUM(ABS(selisihbarang)) as selisihbarang, SUM(jmlsa) as jmlsa FROM m3_sp_detail WHERE " & updFilter & " GROUP BY idsp", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilai = "" : updFilter = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlsa") >= dr1("selisihbarang") Then
                                    statusOut = 2
                                ElseIf dr1("jmlsa") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilai = String.Concat(updNilai, "WHEN '" & dr1("idsp") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                                updFilter = String.Concat(updFilter, "(spid = '" & dr1("idsp") & "')")
                            Next

                            sql = "UPDATE m3_sp SET spstatussa = (CASE spid " & updNilai & " ELSE spstatussa END) WHERE " & updFilter
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                        'END OF UPDATE OUTSTANDING TRANSAKSI ============================================
                    End If


                    'If Len(updNilaiSO) > 0 Then
                    '    'UPDATE OUTSTANDING TRANSAKSI SO =======================================================
                    '    'UPDATE DETAIL
                    '    sql = "UPDATE m5_so_detail SET customdbl2 = (CASE idsodetail " & updNilaiSO & " ELSE customdbl2 END) WHERE " & updFilterSO
                    '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    '    With objCmd
                    '        .Connection = myconn
                    '        .Transaction = Trans
                    '        .CommandType = CommandType.Text
                    '        .CommandText = sql
                    '    End With
                    '    objCmd.ExecuteNonQuery()

                    '    'UPDATE UTAMA
                    '    Dim ftDetail As String = "", statusOut As Integer = 0
                    '    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idso FROM m5_so_detail WHERE " & updFilterSO & " GROUP BY idso")
                    '    If dtOut.Rows.Count > 0 Then
                    '        For Each dr1 As DataRow In dtOut.Rows
                    '            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                    '            ftDetail = String.Concat(ftDetail, "(idso = '" & dr1("idso") & "')")
                    '        Next
                    '    End If
                    '    dtOut = AsDataTableAmbilDariDBCon("SELECT idso, SUM(jmlbarang) as jmlbarang, SUM(customdbl2) as jmlrealisasi FROM m5_so_detail WHERE " & ftDetail & " GROUP BY idso")
                    '    If dtOut.Rows.Count > 0 Then
                    '        'KOSONGKAN VARIABEL NILAI DAN FILTER
                    '        updNilaiSO = "" : updFilterSO = ""
                    '        For Each dr1 As DataRow In dtOut.Rows
                    '            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                    '            If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                    '                statusOut = 2
                    '            ElseIf dr1("jmlrealisasi") < 1 Then
                    '                statusOut = 0
                    '            Else
                    '                statusOut = 1
                    '            End If
                    '            '2. SET NILAI UPDATE OUTSTANDING
                    '            updNilaiSO = String.Concat(updNilaiSO, "WHEN '" & dr1("idso") & "' THEN '" & statusOut & "' ")
                    '            '3. SET FILTERUPDATE OUTSTANDING
                    '            updFilterSO = IIf(Len(updFilterSO.ToString) = 0, "", updFilterSO & " OR ")
                    '            updFilterSO = String.Concat(updFilterSO, "(soid = '" & dr1("idso") & "')")
                    '        Next

                    '        sql = "UPDATE m5_so SET socustomint3 = (CASE soid " & updNilaiSO & " ELSE socustomint3 END) WHERE " & updFilterSO
                    '        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    '        With objCmd
                    '            .Connection = myconn
                    '            .Transaction = Trans
                    '            .CommandType = CommandType.Text
                    '            .CommandText = sql
                    '        End With
                    '        objCmd.ExecuteNonQuery()
                    '    End If
                    '    'END OF UPDATE OUTSTANDING TRANSAKSI SO ================================================
                    'End If


                    'INSERT NO BATCH IN =================================================================
                    Dim dtBatchIn As DataTable = AsDataTableFilterSortDt(dtbatch, "nbtjenismutasi = '1'")
                    If dtBatchIn.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder
                        For Each dr1 As DataRow In dtBatchIn.Rows
                            'QUERY INSERT NO BATCH IN
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping        nbiidbatchin,                     nbigudang,                  nbiidbarang,                           nbikode,                             nbisumber,            nbiidtransaksi,                     nbisatuan,                 nbijmlmasuk,       nbijmlkeluar,                  nbijmlsisa, nbiisclose,                     nbicustomtext1,                             nbicustomtext2,                             nbicustomtext3,                             nbicustomdbl1,                             nbicustomdbl2,                             nbicustomdbl3,                                             nbicustomdate1,                                              nbicustomdate2,                                              nbicustomdate3
                            strValue2.Append("(" & 0 & ", '" & FixQuotes(dr1("nbtgudang")) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixQuotes(dr1("nbtcustomtext1")) & "', '" & FixQuotes(dr1("nbtcustomtext2")) & "', '" & FixQuotes(dr1("nbtcustomtext3")) & "', '" & FixDouble(dr1("nbtcustomdbl1")) & "', '" & FixDouble(dr1("nbtcustomdbl2")) & "', '" & FixDouble(dr1("nbtcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate3"))) & "')")
                        Next
                        sql = "Insert into M1_No_Batch_In(nbiidbatchin, nbigudang, nbiidbarang, nbikode, nbisumber, nbiidtransaksi, nbisatuan, nbijmlmasuk, nbijmlkeluar, nbijmlsisa, nbiisclose, nbicustomtext1, nbicustomtext2, nbicustomtext3, nbicustomdbl1, nbicustomdbl2, nbicustomdbl3, nbicustomdate1, nbicustomdate2, nbicustomdate3) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF INSERT NO BATCH IN =========================================================


                    'INSERT NO SERIAL IN ===============================================================
                    Dim dtSerialIn As DataTable = AsDataTableFilterSortDt(dtserial, "nstjenismutasi = '1'")
                    If dtSerialIn.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder
                        For Each dr1 As DataRow In dtSerialIn.Rows
                            'QUERY INSERT NO SERIAL IN
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping       nsiidserialin,                     nsigudang,                  nsiidbarang,                           nsikode,                             nsisumber,            nsiidtransaksi,                     nsisatuan,                       nsijmlmasuk, nsijmlkeluar,                  nsijmlsisa, nsiisclose,                     nsicustomtext1,                             nsicustomtext2,                             nsicustomtext3,                             nsicustomdbl1,                             nsicustomdbl2,                             nsicustomdbl3,                                             nsicustomdate1,                                              nsicustomdate2,                                              nsicustomdate3
                            strValue2.Append("(" & 0 & ", '" & FixQuotes(dr1("nstgudang")) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixQuotes(dr1("nstcustomtext1")) & "', '" & FixQuotes(dr1("nstcustomtext2")) & "', '" & FixQuotes(dr1("nstcustomtext3")) & "', '" & FixDouble(dr1("nstcustomdbl1")) & "', '" & FixDouble(dr1("nstcustomdbl2")) & "', '" & FixDouble(dr1("nstcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate3"))) & "')")
                        Next
                        sql = "Insert into M1_No_Serial_In(nsiidserialin, nsigudang, nsiidbarang, nsikode, nsisumber, nsiidtransaksi, nsisatuan, nsijmlmasuk, nsijmlkeluar, nsijmlsisa, nsiisclose, nsicustomtext1, nsicustomtext2, nsicustomtext3, nsicustomdbl1, nsicustomdbl2, nsicustomdbl3, nsicustomdate1, nsicustomdate2, nsicustomdate3) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF INSERT NO SERIAL IN =====================================================


                    'INSERT NO ASSET ===============================================================
                    Dim dtAssetIn As DataTable = AsDataTableFilterSortDt(dtasset, "atjenismutasi = '1'")
                    If dtAssetIn.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder
                        For Each dr1 As DataRow In dtAssetIn.Rows
                            'QUERY INSERT NO ASSET IN
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            strValue2.Append("('" & 0 & "', '" & FixQuotes(dr1("atkode")) & "', '" & FixQuotes(dr1("atnama")) & "', '" & FixQuotes(dr1("atkategori")) & "', '" & FixQuotes(dr1("atcabang")) & "', '" & FixQuotes(dr1("atlokasi")) & "', '" & FixQuotes(dr1("atgudang")) & "', '" & FixQuotes(dr1("atdivisi")) & "', '" & FixQuotes(dr1("atsubdivisi")) & "', '" & FixQuotes(dr1("atcostcenter")) & "', '" & FixQuotes(dr1("atproyek")) & "', '" & FixQuotes(dr1("atcatatan")) & "', '" & FixQuotes(dr1("atnomor")) & "', '" & FixQuotes(AsFormatTanggal(dr1("attglbeli"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("attglpakai"))) & "', '" & FixDouble(dr1("atjml")) & "', '" & FixQuotes(dr1("atsatuan")) & "', '" & FixQuotes(dr1("atmatauang")) & "', '" & FixDouble(dr1("atkurs")) & "', '" & FixDouble(dr1("atharga")) & "', '" & FixQuotes(dr1("atdiskon")) & "', '" & FixDouble(dr1("atjmldiskon")) & "', '" & FixQuotes(dr1("atpajak1")) & "', '" & FixDouble(dr1("atjmlpajak1")) & "', '" & FixQuotes(dr1("atpajak2")) & "', '" & FixDouble(dr1("atjmlpajak2")) & "', '" & FixDouble(dr1("athargabeli")) & "', '" & FixDouble(dr1("atnilairesidu")) & "', '" & FixDouble(dr1("atumurekonomis")) & "', '" & FixDouble(dr1("atbebanperbln")) & "', '" & FixDouble(dr1("atakumulasibeban")) & "', '" & FixDouble(dr1("atnilaibuku")) & "', " & dr1("atmetode") & ", '" & FixQuotes(dr1("attabelpenyusutan")) & "', " & dr1("atintangible") & ", " & dr1("atfiskal") & ", " & dr1("atatastengahbulan") & ", '" & FixQuotes(dr1("atrekasset")) & "', '" & FixQuotes(dr1("atrekakumdepresiasi")) & "', '" & FixQuotes(dr1("atrekdepresiasi")) & "', '" & FixQuotes(dr1("atrekpenghapusan")) & "', '" & FixQuotes(dr1("atprodusen")) & "', '" & FixQuotes(AsFormatTanggal(dr1("attglpensiun"))) & "', '" & FixDouble(dr1("atpenyusutanke")) & "', '" & FixDouble(dr1("atnilaimenurun")) & "', " & dr1("atdispose") & ", " & dr1("atpembelian") & ", " & dr1("atpenjualan") & ", " & dr1("atlocked") & ", " & dr1("atstatus") & ", " & dr1("atstatussebelumnya") & ", " & dr1("atisclose") & ", '" & FixQuotes(dr1("atinputuser")) & "', '" & FixQuotes(AsFormatTanggal(dr1("atinputtgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(dr1("atmodifikasiuser")) & "', '" & FixQuotes(AsFormatTanggal(dr1("atmodifikasitgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(dr1("atcustomtext1")) & "', '" & FixQuotes(dr1("atcustomtext2")) & "', '" & FixQuotes(dr1("atcustomtext3")) & "', '" & FixQuotes(dr1("atcustomtext4")) & "', '" & FixQuotes(dr1("atcustomtext5")) & "', " & dr1("atcustomint1") & ", " & dr1("atcustomint2") & ", " & dr1("atcustomint3") & ", " & dr1("atcustomint4") & ", " & dr1("atcustomint5") & ", '" & FixDouble(dr1("atcustomdbl1")) & "', '" & FixDouble(dr1("atcustomdbl2")) & "', '" & FixDouble(dr1("atcustomdbl3")) & "', '" & FixDouble(dr1("atcustomdbl4")) & "', '" & FixDouble(dr1("atcustomdbl5")) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate4"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate5"))) & "', '" & FixQuotes(dr1("atidbarang")) & "')")
                        Next
                        sql = "Insert into M7_Asset(aid, akode, anama, akategori, acabang, alokasi, agudang, adivisi, asubdivisi, acostcenter, aproyek, acatatan, anomor, atglbeli, atglpakai, ajml, asatuan, amatauang, akurs, aharga, adiskon, ajmldiskon, apajak1, ajmlpajak1, apajak2, ajmlpajak2, ahargabeli, anilairesidu, aumurekonomis, abebanperbln, aakumulasibeban, anilaibuku, ametode, atabelpenyusutan, aintangible, afiskal, aatastengahbulan, arekasset, arekakumdepresiasi, arekdepresiasi, arekpenghapusan, aprodusen, atglpensiun, apenyusutanke, anilaimenurun, adispose, apembelian, apenjualan, alocked, astatus, astatussebelumnya, aisclose, ainputuser, ainputtgl, amodifikasiuser, amodifikasitgl, acustomtext1, acustomtext2, acustomtext3, acustomtext4, acustomtext5, acustomint1, acustomint2, acustomint3, acustomint4, acustomint5, acustomdbl1, acustomdbl2, acustomdbl3, acustomdbl4, acustomdbl5, acustomdate1, acustomdate2, acustomdate3, acustomdate4, acustomdate5, aidbarang) values" & strValue2.ToString & ""
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


                    'INSERT NO BATCH OUT ============================================================
                    Dim dtBatchOut = AsDataTableFilterSortDt(dtbatch, "nbtjenismutasi = '0'")
                    If dtBatchOut.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder
                        For Each dr1 As DataRow In dtbatch.Rows
                            'QUERY INSERT NO BATCH OUT
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping             nboid,            nboidbatchin,                           nbogudang,                  nboidbarang,                           nbokode,                             nbosumber,            nboidtransaksi,                     nbosatuan,                         nbojmlkeluar,       nboisclose
                            strValue2.Append("(" & 0 & ", " & dr1("nbtidbatchin") & ", '" & FixQuotes(dr1("nbtgudang")) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', " & 0 & ")")
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
                    End If
                    'END OF INSERT NO BATCH OUT =====================================================


                    'INSERT NO SERIAL OUT ===========================================================
                    Dim dtSerialOut = AsDataTableFilterSortDt(dtserial, "nstjenismutasi = '0'")
                    If dtSerialOut.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder
                        For Each dr1 As DataRow In dtserial.Rows
                            'QUERY INSERT NO SERIAL OUT
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping            nsoid,             nsoidserialin,                           nsogudang,                  nsoidbarang,                           nsokode,                             nsosumber,            nsoidtransaksi,                     nsosatuan,                          nsojmlkeluar,      nsoisclose
                            strValue2.Append("(" & 0 & ", " & dr1("nstidserialin") & ", '" & FixQuotes(dr1("nstgudang")) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', " & 0 & ")")
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
                    End If
                    'END OF INSERT NO SERIAL OUT ====================================================


                    'UPDATE NO ASSET ===============================================================
                    Dim dtAssetOut As DataTable = AsDataTableFilterSortDt(dtasset, "atjenismutasi = '0'")
                    If dtAssetOut.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder
                        For Each dr1 As DataRow In dtAssetOut.Rows
                            'QUERY INSERT NO ASSET IN
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            strValue2.Append(FixDouble(dr1("atasetid")))
                        Next
                        sql = "UPDATE m7_asset a SET a.aakumulasibeban = 0, a.anilaibuku = 0, a.aisclose = 1, a.atglclose = '" & AsFormatTanggal(vTgl) & "' WHERE a.aid IN(" & strValue2.ToString & ")"
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


                    'AMBIL DATA DETAIL YANG BARU ++++++++++++++++++++++++++++++++++++++++++++++++++
                    'JIKA sacustomint1 = 1, MAKA PENYESUAIAN HPP. JIKA sacustomint1 = 0, MAKA PENYESUAIAN STOK
                    If drutama("sacustomint1") = 1 Then
                        sql = "SELECT sad.idsadetail, sad.idbarang, sad.namabarang, sad.tipebarang, (CASE j.jenismutasi WHEN 1 THEN isw.stok / sad.nilaisatuan ELSE 0 END) as jmlmasuk, (CASE j.jenismutasi WHEN 0 THEN isw.stok / sad.nilaisatuan ELSE 0 END) as jmlkeluar, sad.satuan, sad.nilaisatuan, (CASE j.jenismutasi WHEN 1 THEN isw.stok ELSE 0 END) as jmlbarangmasuk, (CASE j.jenismutasi WHEN 0 THEN isw.stok ELSE 0 END) as jmlbarangkeluar, sad.satuanbarang, sad.hpp, sad.idhppkhususmasuk, isw.kgudang as gudang, sad.catatan, sad.costcenter, sad.divisi, sad.subdivisi, sad.customdbl1, sad.proyek, sa.sainputtgl, i.bhpp FROM m3_sa_detail sad JOIN m3_sa sa ON sad.idsa = sa.said JOIN m1_item i ON sad.idbarang = i.bid JOIN m0_jenismutasi j JOIN m1_item_stock_warehouse isw ON sad.idbarang = isw.idbarang AND isw.stok <> 0 WHERE sad.idsa = '" & result(4) & "' ORDER BY sad.urutan, j.jenismutasi, isw.kgudang, sad.idsadetail"
                    Else
                        sql = "SELECT sad.idsadetail, sad.idbarang, sad.namabarang, sad.tipebarang, sad.jmlmasuk, sad.jmlkeluar, sad.satuan, sad.jmlbarangmasuk, sad.jmlbarangkeluar, sad.satuanbarang, sad.hpp, sad.idhppkhususmasuk, sad.gudang, sad.catatan, sad.costcenter, sad.divisi, sad.subdivisi, sad.customdbl1, sad.proyek, sa.sainputtgl, i.bhpp FROM m3_sa_detail sad JOIN m3_sa sa ON sad.idsa = sa.said JOIN m1_item i ON sad.idbarang = i.bid WHERE sad.idsa = '" & result(4) & "'"
                    End If
                    Dim dtDetailNew As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)

                    Dim hpp As Double = 0, postinghpp As Double = 0, bstok As Double = 0
                    Dim saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0
                    Dim strTransaksiBarang As New StringBuilder, dtSaldo As New DataTable

                    If dtDetailNew.Rows.Count > 0 Then

                        'INSERT ITEM TRANSACTION ==================================================
                        Dim sqlStokGudang As String = "", jmltransaksi As Double = 0

                        'AMBIL MATAUANG FUNGSIONAL DARI SETTING
                        Dim dtMatauang As DataTable = AsDataTableAmbilDariDBCon("SELECT skode, snilai FROM m0_setting WHERE smodule = 0 AND sgrup = 'accounting' AND (skode = 'MataUangFungsional' OR skode = 'Kurs')", myConn)
                        Dim matauang As String = AsDataTableDLookup(dtMatauang, "snilai", "skode = 'MataUangFungsional'", "Not found")
                        If matauang = "Not found" Then
                            result(2) = "Setting Functional Currency not found." : Trans.Rollback() : GoTo selesai
                        End If
                        Dim kurs As String = AsDataTableDLookup(dtMatauang, "snilai", "skode = 'Kurs'", "Not found")
                        If kurs = "Not found" Then
                            result(2) = "Setting Exchange Rate Functional Currency not found." : Trans.Rollback() : GoTo selesai
                        End If

                        'PERULANGAN DATA DETAIL
                        For Each dr1 As DataRow In dtDetailNew.Rows
                            'SET NILAI VARIABEL
                            idbarang = Double.Parse(dr1("idbarang"))
                            gudang = dr1("gudang")

                            'AMBIL DATA STOK DAN HPPAVERAGE TERBARU
                            sql = "SELECT bstok FROM m1_item WHERE bid = '" & FixDouble(idbarang) & "'"
                            dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                            If dtSaldo.Rows.Count > 0 Then
                                'set nilai stok
                                bstok = Double.Parse(dtSaldo.Rows(0)("bstok"))

                                'BARANG MASUK ATAU KELUAR
                                If Double.Parse(dr1("jmlbarangmasuk")) > 0 Then
                                    jmlbarang = Double.Parse(dr1("jmlbarangmasuk"))
                                    jmltransaksi = Double.Parse(dr1("jmlmasuk"))

                                    'jenismutasi dan postinghpp 
                                    '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 0
                                    '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                    jenismutasi = 1 : postinghpp = 0

                                    'hitung saldojml = bstok + jmlbarang
                                    saldojml = bstok + jmlbarang

                                    'hitung hpp = 0, saldohpp = 0, saldonilai = 0
                                    hpp = 0 : saldohpp = 0 : saldonilai = 0

                                    'sql stok pergudang
                                    sqlStokGudang = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES ('" & idbarang & "','" & gudang & "','" & jmlbarang & "') ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"

                                Else
                                    jmlbarang = Double.Parse(dr1("jmlbarangkeluar"))
                                    jmltransaksi = Double.Parse(dr1("jmlkeluar"))

                                    'jenismutasi dan postinghpp 
                                    '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 0
                                    '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                    jenismutasi = 0 : postinghpp = 0

                                    'hitung saldojml = bstok - jmlbarang
                                    saldojml = bstok - jmlbarang

                                    'hitung hpp = 0, saldohpp = 0, saldonilai = 0
                                    hpp = 0 : saldohpp = 0 : saldonilai = 0

                                    'sql stok pergudang
                                    sqlStokGudang = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES ('" & idbarang & "','" & gudang & "','-" & jmlbarang & "') ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"

                                End If

                                'QUERY INSERT TRANSAKSI BARANG
                                strTransaksiBarang.Clear()
                                'mapping                        id,                             cabang,                                   lokasi,                             gudang,                        kodepa,           jenismutasi,                               sumber,              idutama,                 iddetail,                      notransaksi,                                                 tgl,                           kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                       jmlbarang,                           satuanbarang,                        matauang,                      kurs,                    harga,                 diskon,              jmldiskon,                        idhppikm,        idhppikk,                hpp,                                 uraian,                                   catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                             inputtgl,                                               inputuser,  postingtgl, updatehpp,     postinghpp,     hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("sacabang")) & "', '" & FixQuotes(drutama("salokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', " & drutama("sakodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("sasumber")) & "', " & result(4) & ", " & dr1("idsadetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("satgl"))) & "', " & drutama("sabagiansa") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(jmltransaksi) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(jmlbarang) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(matauang) & "', '" & FixDouble(kurs) & "', '" & FixDouble(hpp) & "', '" & FixQuotes(0) & "', '" & FixDouble(0) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("sauraian")) & "', '" & FixQuotes(drutama("sacatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr1("sainputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("sainputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                                'UPDATE STOK PERGUDANG
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sqlStokGudang
                                End With
                                objCmd.ExecuteNonQuery()

                                'UPDATE STOK GLOBAL
                                'sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "' WHERE bid = '" & idbarang & "'"
                                'TAMBAHKAN KONDISI JIKA CUSTOMDBL <> 0 MAKA UPDATE BHARGABELI
                                sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "', bhargabeli = (CASE " & FixDouble(dr1("customdbl1")) & " WHEN 0 THEN bhargabeli ELSE " & FixDouble(dr1("customdbl1")) & " END), baktiftgl = '" & drutama("satgl") & "' WHERE bid = '" & idbarang & "'"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                            End If

                        Next
                        'END OF INSERT ITEM TRANSACTION ===========================================

                    Else
                        'JIKA sacustomint1 = 1, MAKA PENYESUAIAN HPP. JIKA sacustomint1 = 0, MAKA PENYESUAIAN STOK
                        If drutama("sacustomint1") = 0 Then
                            result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                        End If

                    End If
                End If


                'INSERT MSMQ COGS =================================================================
                Dim sumber As String = "SA", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("sastatus") = 2 Then
                    Dim Security As New ClsSecurity, mjid As String = "", hasilMsmq As String = ""
                    'BUAT ID UNIQUE
                    mjid = Security.MD5CalcString(userid & sumber & result(4) & Now) 'RandomId.Generate(15)

                    'MSMQ TABEL
                    sql = "Insert into M0_Msmq_Cogs(mcid, mcsumber, mcidtransaksi, mcprogress, mcpesan, mctglantrian, mctglselesai, mcuserid) values ('" _
                        & mjid & "', '" & sumber & "', '" & result(4) & "', '" & 0 & "', " & "''" & ", NOW(), '1971-01-01 00:00:00', '" & userid & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'MSMQ ANTRIAN
                    Dim ProsesHpp As String = F_getSetting(0, "accounting", "ProsesHpp")
                    If ProsesHpp.Equals("0") = False Then
                        hasilMsmq = SendMsmq(dirMsmq, "C", mjid, sumber, result(4), userid)
                        If Len(hasilMsmq) > 0 Then
                            result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
                        End If
                    End If

                End If
                'END OF INSERT MSMQ COGS ==========================================================


                'INSERT USER LOG ==================================================================
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
    Public Function M3_SaUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("sabagiansakode", "c1.kkode")
            Filter = Filter.Replace("sabagiansanama", "c1.knama")
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
            Dim sumber As String = "SA", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Satgl, Sanotransaksi, Sastatus FROM m3_Sa WHERE Said='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Sastatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m3_sa_history
            Dim rsSimpanHistory As String = SimpanHistory.M3_Sa_HistorySimpan("" & paramSplit(0) & "★M3_Sa_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m3_sa_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================


                'CEK NO BATCH DAN SERIAL IN =====================================================
                'BATCH
                dtdetail = AsDataTableAmbilDariDBCon("SELECT bkode, nbikode FROM m1_no_batch_in JOIN m1_item ON nbiidbarang = bid WHERE nbisumber = 'SA' AND nbiidtransaksi = '" & idtransaksi & "' AND nbijmlkeluar > 0", myConn)
                If dtdetail.Rows.Count > 0 Then result(2) = "Item : " & dtdetail.Rows(0)("bkode") & " | No. Batch : " & dtdetail.Rows(0)("nbikode") & " has related transactions." : Trans.Rollback() : GoTo selesai

                'SERIAL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT bkode, nsikode FROM m1_no_serial_in JOIN m1_item ON nsiidbarang = bid WHERE nsisumber = 'SA' AND nsiidtransaksi = '" & idtransaksi & "' AND nsijmlkeluar > 0", myConn)
                If dtdetail.Rows.Count > 0 Then result(2) = "Item : " & dtdetail.Rows(0)("bkode") & " | No. Serial : " & dtdetail.Rows(0)("nsikode") & " has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK NO BATCH DAN SERIAL IN ==============================================

                'Variabel ValidasiSimpan
                Dim ftHppI As String = "", ftHppF As String = ""
                Dim updNilaiHppI As String = "", updFilterHppI As String = "", delFilterHppI As String = ""
                Dim filterHppF As String = "", updNilaiHppF As String = "", updFilterHppF As String = "", delFilterHppF As String = ""
                Dim ftExistStok As String = "", ftStok As String = "", gudang As String = ""
                Dim updNilai As String = "", updFilter As String = "", updStokIn As String = "", updStokOut As String = ""
                Dim updStokBarangMasuk As String = "", ftStokBarangMasuk As String = ""
                Dim updStokBarangKeluar As String = "", ftStokBarangKeluar As String = ""
                'Dim updNilaiSO As String = "", updFilterSO As String = ""

                Dim idbarang As Integer = 0, idsadetail As Integer = 0, idspdetail As Integer = 0
                'Dim idsodetail As Integer = 0
                Dim jmlbarangMasuk As Double = 0, jmlbarangKeluar As Double = 0, idhppkhususmasuk As Integer = 0
                Dim isPlus As Boolean = False

                'AMBIL DATA DETAIL
                'dtdetail = AsDataTableAmbilDariDBCon("SELECT idsadetail, idbarang, jmlbarangmasuk, jmlbarangkeluar, idhppkhususmasuk, gudang, idspdetail  FROM m3_sa_detail WHERE idsa = '" & idtransaksi & "'")
                dtdetail = AsDataTableAmbilDariDBCon("SELECT idsadetail, idbarang, jmlbarangmasuk, jmlbarangkeluar, idhppkhususmasuk, gudang, idspdetail, tipebarang, namabarang, urutan, satuan, nilaisatuan, customdbl2  FROM m3_sa_detail WHERE idsa = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idsadetail = dr1("idsadetail") : idbarang = dr1("idbarang") : idhppkhususmasuk = dr1("idhppkhususmasuk")
                        jmlbarangMasuk = dr1("jmlbarangmasuk") : jmlbarangKeluar = dr1("jmlbarangkeluar") : gudang = dr1("gudang")
                        idspdetail = dr1("idspdetail")
                        'idsodetail = dr1("customdbl2")

                        'UPDATE OUTSTANDING ---------------------------
                        If idspdetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING ----------
                            Dim OutstandingMasuk As Double = AsDataTableDSum(dtdetail, "jmlbarangmasuk", "idspdetail=" & idspdetail)
                            Dim OutstandingKeluar As Double = AsDataTableDSum(dtdetail, "jmlbarangkeluar", "idspdetail=" & idspdetail)
                            Dim Outstanding As Double = Math.Abs(OutstandingMasuk - OutstandingKeluar)
                            updNilai = String.Concat("WHEN '" & idspdetail & "' THEN ROUND(jmlsa - '" & Outstanding & "', 5) ", updNilai)

                            '2. SET FILTERUPDATE OUTSTANDING ----------
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(idspdetail = '" & idspdetail & "')")
                        End If

                        'If idsodetail <> 0 Then
                        '    '1. SET NILAI UPDATE OUTSTANDING ----------
                        '    Dim OutstandingMasuk As Double = AsDataTableDSum(dtdetail, "jmlbarangmasuk", "customdbl2=" & idsodetail)
                        '    Dim OutstandingKeluar As Double = AsDataTableDSum(dtdetail, "jmlbarangkeluar", "customdbl2=" & idsodetail)
                        '    Dim Outstanding As Double = Math.Abs(OutstandingKeluar - OutstandingMasuk)
                        '    updNilaiSO = String.Concat("WHEN '" & idsodetail & "' THEN ROUND(customdbl2 - '" & Outstanding & "', 5) ", updNilaiSO)

                        '    '2. SET FILTERUPDATE OUTSTANDING ----------
                        '    updFilterSO = IIf(Len(updFilterSO.ToString) = 0, "", updFilterSO & " OR ")
                        '    updFilterSO = String.Concat(updFilterSO, "(idsodetail = '" & idsodetail & "')")
                        'End If

                        'VALIDASI STOK -------------------------------
                        'CEK TRANSAKSI STOK MASUK/KELUAR
                        Dim StokMasuk As Double = AsDataTableDSum(dtdetail, "jmlbarangmasuk", "idbarang=" & idbarang & " AND gudang='" & gudang & "'")
                        Dim StokKeluar As Double = AsDataTableDSum(dtdetail, "jmlbarangkeluar", "idbarang=" & idbarang & " AND gudang='" & gudang & "'")
                        Dim Stok As Double = StokMasuk - StokKeluar
                        Stok = Math.Abs(Stok)

                        If jmlbarangMasuk <> 0 Then isPlus = True Else isPlus = False

                        '   'JIKA TRANSAKSI STOK MASUK, MAKA STOK DIKELUARKAN
                        If isPlus = True Then

                            'CEK DATA EXIST STOK KELUAR
                            ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                            ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudang & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudang & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                            'CEK JML STOK KELUAR
                            ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
                            'ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudang & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")
                            ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudang & "' AND " & Stok & " > isw.stok) ")

                            'SET NILAI UPDATE STOK KELUAR
                            updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
                            updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudang & "', ('-" & jmlbarangMasuk & "'))") ' idbarang, kgudang, stok

                            'BUAT FILTER CEK HPP KHUSUS(I)
                            ftHppI = IIf(Len(ftHppI.ToString) = 0, "", ftHppI & " OR ")
                            ftHppI = String.Concat(ftHppI, "(idbarang = '" & idbarang & "' AND idtransaksi = '" & idsadetail & "' AND sumber = 'SA')")

                            'BUAT FILER CEK HPP FIFO(F)
                            ftHppF = IIf(Len(ftHppF.ToString) = 0, "", ftHppF & " OR ")
                            ftHppF = String.Concat(ftHppF, "(cfiidbarang = '" & idbarang & "' AND cfiidtransaksi = '" & idsadetail & "' AND cfisumber = 'SA')")

                            'SET NILAI UPDATE STOK KELUAR M1_ITEM
                            Dim jmlkeluar As Double = AsDataTableDSum(dtdetail, "jmlbarangmasuk", "idbarang=" & idbarang)
                            ftStokBarangKeluar = IIf(Len(ftStokBarangKeluar.ToString) = 0, "", ftStokBarangKeluar & " OR ")
                            ftStokBarangKeluar = String.Concat(ftStokBarangKeluar, " (bid = '" & idbarang & "') ")
                            updStokBarangKeluar = String.Concat("WHEN '" & idbarang & "' THEN ROUND(bstok - '" & jmlkeluar & "', 5) ", updStokBarangKeluar)

                        ElseIf jmlbarangKeluar <> 0 Then
                            'JIKA TRANSAKSI STOK KELUAR, MAKA STOK DIKEMBALIKAN

                            'SET NILAI UPDATE STOK MASUK 
                            updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
                            updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudang & "', '" & jmlbarangKeluar & "')") ' idbarang, kgudang, stok

                            'BUAT FILTER UPDATE HPP KHUSUS (I)
                            If idhppkhususmasuk <> 0 Then
                                'SET NILAI UPDATE HPP KHUSUS IN
                                Dim jmlKeluar As Double = AsDataTableDSum(dtdetail, "jmlbarangkeluar", "idhppkhususmasuk='" & idhppkhususmasuk & "'")
                                updNilaiHppI = String.Concat("WHEN '" & idhppkhususmasuk & "' THEN ROUND(jmlkeluar - '" & jmlKeluar & "', 5) ", updNilaiHppI)

                                'SET FILTER UPDATE HPP KHUSUS IN
                                updFilterHppI = IIf(Len(updFilterHppI.ToString) = 0, "", updFilterHppI & " OR ")
                                updFilterHppI = String.Concat(updFilterHppI, "(idhppikm = '" & idhppkhususmasuk & "')")

                                'SET FILTER DELETE HPP KHUSUS OUT
                                delFilterHppI = IIf(Len(delFilterHppI.ToString) = 0, "", delFilterHppI & " OR ")
                                delFilterHppI = String.Concat(delFilterHppI, "(sumber = 'SA' AND idtransaksi = '" & idsadetail & "')")
                            End If

                            'BUAT FILTER UPDATE HPP FIFO (F)
                            filterHppF = IIf(Len(filterHppF.ToString) = 0, "", filterHppF & " OR ")
                            filterHppF = String.Concat(filterHppF, "(cfosumber = 'SA' AND cfoidtransaksi = '" & idsadetail & "')")

                            'SET NILAI UPDATE STOK MASUK M1_ITEM
                            Dim jmlmasuk As Double = AsDataTableDSum(dtdetail, "jmlbarangkeluar", "idbarang=" & idbarang)
                            ftStokBarangMasuk = IIf(Len(ftStokBarangMasuk.ToString) = 0, "", ftStokBarangMasuk & " OR ")
                            ftStokBarangMasuk = String.Concat(ftStokBarangMasuk, " (bid = '" & idbarang & "') ")
                            updStokBarangMasuk = String.Concat("WHEN '" & idbarang & "' THEN ROUND(bstok + '" & jmlmasuk & "', 5) ", updStokBarangMasuk)

                        ElseIf jmlbarangMasuk = 0 And jmlbarangKeluar = 0 Then

                            'BUAT FILTER CEK HPP KHUSUS(I)
                            ftHppI = IIf(Len(ftHppI.ToString) = 0, "", ftHppI & " OR ")
                            ftHppI = String.Concat(ftHppI, "(idbarang = '" & idbarang & "' AND idtransaksi = '" & idsadetail & "' AND sumber = 'SA')")

                            'BUAT FILER CEK HPP FIFO(F)
                            ftHppF = IIf(Len(ftHppF.ToString) = 0, "", ftHppF & " OR ")
                            ftHppF = String.Concat(ftHppF, "(cfiidbarang = '" & idbarang & "' AND cfiidtransaksi = '" & idsadetail & "' AND cfisumber = 'SA')")

                            'BUAT FILTER UPDATE HPP KHUSUS (I)
                            If idhppkhususmasuk <> 0 Then
                                'SET NILAI UPDATE HPP KHUSUS IN
                                Dim jmlKeluar As Double = AsDataTableDSum(dtdetail, "jmlbarangkeluar", "idhppkhususmasuk='" & idhppkhususmasuk & "'")
                                updNilaiHppI = String.Concat("WHEN '" & idhppkhususmasuk & "' THEN ROUND(jmlkeluar - '" & jmlKeluar & "', 5) ", updNilaiHppI)

                                'SET FILTER UPDATE HPP KHUSUS IN
                                updFilterHppI = IIf(Len(updFilterHppI.ToString) = 0, "", updFilterHppI & " OR ")
                                updFilterHppI = String.Concat(updFilterHppI, "(idhppikm = '" & idhppkhususmasuk & "')")

                                'SET FILTER DELETE HPP KHUSUS OUT
                                delFilterHppI = IIf(Len(delFilterHppI.ToString) = 0, "", delFilterHppI & " OR ")
                                delFilterHppI = String.Concat(delFilterHppI, "(sumber = 'SA' AND idtransaksi = '" & idsadetail & "')")
                            End If

                            'BUAT FILTER UPDATE HPP FIFO (F)
                            filterHppF = IIf(Len(filterHppF.ToString) = 0, "", filterHppF & " OR ")
                            filterHppF = String.Concat(filterHppF, "(cfosumber = 'SA' AND cfoidtransaksi = '" & idsadetail & "')")

                        End If
                        'END OF BUAT FILTER UNTUK UPDATE --------------------------
                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If


                'VALIDASI HPP, STOK ----------------------------------
                'Dim rsValidasi As String = ValidasiSimpan(dtdetail, "", "", "", "", ftExistStok, ftStok, "", ftHppI, ftHppF, "", "", "", "", "")
                Dim rsValidasi As String = ValidasiSimpan(dtdetail, "", "", ftExistStok, ftStok, "", ftHppI, ftHppF, "", "", "", "", "", "")
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'END OF VALIDASI HPP, STOK ---------------------------


                'CEK HPP FIFO ====================================================================
                'AMBIL DATA DARI HPP FIFO KELUAR - m1_cogs_fifo_out
                Dim dtHppF As DataTable = AsDataTableAmbilDariDBCon("SELECT * FROM m1_cogs_fifo_out WHERE " & filterHppF, myConn)
                If dtHppF.Rows.Count > 0 Then
                    Dim idhppfifoin As Integer = 0
                    For Each dr1 As DataRow In dtHppF.Rows
                        'SET NILAI VARIABEL
                        idhppfifoin = dr1("cfoidcfi")

                        'SET FILTER DELETE HPP FIFO OUT
                        delFilterHppF = IIf(Len(delFilterHppF.ToString) = 0, "", delFilterHppF & " OR ")
                        delFilterHppF = String.Concat(delFilterHppF, "(cfosumber = 'SA' AND cfoidtransaksi = '" & dr1("cfoidtransaksi") & "')")

                        'SET NILAI UPDATE HPP FIFO IN
                        Dim jmlKeluar As Double = AsDataTableDSum(dtHppF, "cfojmlkeluar", "cfoidcfi='" & idhppfifoin & "'")
                        updNilaiHppF = String.Concat("WHEN '" & idhppfifoin & "' THEN ROUND(cfijmlkeluar - '" & jmlKeluar & "', 5) ", updNilaiHppF)

                        'SET FILTER UPDATE HPP FIFO IN
                        updFilterHppF = IIf(Len(updFilterHppF.ToString) = 0, "", updFilterHppF & " OR ")
                        updFilterHppF = String.Concat(updFilterHppF, "(cfiid = '" & idhppfifoin & "')")
                    Next
                End If
                'END OF CEK HPP FIFO =============================================================


                'UPDATE OUTSTANDING TRANSAKSI ====================================================
                If Len(updFilter) > 0 Then
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m3_sp_detail SET jmlsa = (CASE idspdetail " & updNilai & " ELSE jmlsa END) WHERE " & updFilter
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
                    Dim updUtama As String = "", statusOut As Integer = 0
                    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idsp, SUM(ABS(selisihbarang)) as selisihbarang, SUM(jmlsa) as jmlsa FROM m3_sp_detail WHERE " & updFilter & " GROUP BY idsp", myConn)
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilai = "" : updFilter = ""
                        For Each dr1 As DataRow In dtOut.Rows
                            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                            If dr1("jmlsa") >= dr1("selisihbarang") Then
                                statusOut = 2
                            ElseIf dr1("jmlsa") < 1 Then
                                statusOut = 0
                            Else
                                statusOut = 1
                            End If
                            '2. SET NILAI UPDATE OUTSTANDING
                            updNilai = String.Concat(updNilai, "WHEN '" & dr1("idsp") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(spid = '" & dr1("idsp") & "')")
                        Next

                        sql = "UPDATE m3_sp SET spstatussa = (CASE spid " & updNilai & " ELSE spstatussa END) WHERE " & updFilter
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

                'If Len(updFilterSO) > 0 Then
                '    'UPDATE OUTSTANDING DETAIL ----------------------
                '    sql = "UPDATE m5_so_detail SET customdbl2 = (CASE idsodetail " & updNilaiSO & " ELSE customdbl2 END) WHERE " & updFilterSO
                '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '    With objCmd
                '        .Connection = myconn
                '        .Transaction = Trans
                '        .CommandType = CommandType.Text
                '        .CommandText = sql
                '    End With
                '    objCmd.ExecuteNonQuery()
                '    'END OF UPDATE OUTSTANDING DETAIL ---------------

                '    'UPDATE OUTSTANDING UTAMA -----------------------
                '    Dim ftDetail As String = "", statusOut As Integer = 0
                '    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idso FROM m5_so_detail WHERE " & updFilterSO & " GROUP BY idso")
                '    If dtOut.Rows.Count > 0 Then
                '        For Each dr1 As DataRow In dtOut.Rows
                '            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                '            ftDetail = String.Concat(ftDetail, "(idso = '" & dr1("idso") & "')")
                '        Next
                '    End If
                '    dtOut = AsDataTableAmbilDariDBCon("SELECT idso, SUM(jmlbarang) as jmlbarang, SUM(customdbl2) as jmlrealisasi FROM m5_so_detail WHERE " & ftDetail & " GROUP BY idso")
                '    If dtOut.Rows.Count > 0 Then
                '        'KOSONGKAN VARIABEL NILAI DAN FILTER
                '        updNilaiSO = "" : updFilterSO = ""
                '        For Each dr1 As DataRow In dtOut.Rows
                '            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                '            If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                '                statusOut = 2
                '            ElseIf dr1("jmlrealisasi") < 1 Then
                '                statusOut = 0
                '            Else
                '                statusOut = 1
                '            End If
                '            '2. SET NILAI UPDATE OUTSTANDING
                '            updNilaiSO = String.Concat(updNilaiSO, "WHEN '" & dr1("idso") & "' THEN '" & statusOut & "' ")
                '            '3. SET FILTERUPDATE OUTSTANDING
                '            updFilterSO = IIf(Len(updFilterSO.ToString) = 0, "", updFilterSO & " OR ")
                '            updFilterSO = String.Concat(updFilterSO, "(soid = '" & dr1("idso") & "')")
                '        Next

                '        sql = "UPDATE m5_so SET socustomint3 = (CASE soid " & updNilaiSO & " ELSE socustomint3 END) WHERE " & updFilterSO
                '        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '        With objCmd
                '            .Connection = myconn
                '            .Transaction = Trans
                '            .CommandType = CommandType.Text
                '            .CommandText = sql
                '        End With
                '        objCmd.ExecuteNonQuery()
                '    End If
                '    'END OF UPDATE OUTSTANDING UTAMA ----------------
                'End If
                'END OF UPDATE OUTSTANDING TRANSAKSI =============================================


                'UPDATE HPP KHUSUS (I) =========================================================
                'DELETE HPP KHUSUS OUT
                If Len(delFilterHppI) > 0 Then
                    sql = "DELETE FROM m1_cogs_special_out WHERE " & delFilterHppI
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'UPDATE HPP KHUSUS IN
                If Len(updNilaiHppI) > 0 Then
                    sql = "UPDATE m1_cogs_special_in SET jmlkeluar = (CASE idhppikm " & updNilaiHppI & " ELSE jmlkeluar END) WHERE " & updFilterHppI
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE HPP KHUSUS (I) ==================================================


                'UPDATE HPP FIFO (F) ===========================================================
                'DELETE HPP FIFO OUT
                If Len(delFilterHppF) > 0 Then
                    sql = "DELETE FROM m1_cogs_fifo_out WHERE " & delFilterHppF
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'UPDATE HPP FIFO IN
                If Len(updNilaiHppF) > 0 Then
                    sql = "UPDATE m1_cogs_fifo_in SET cfijmlkeluar = (CASE cfiid " & updNilaiHppF & " ELSE cfijmlkeluar END) WHERE " & updFilterHppF
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE HPP FIFO (F) ====================================================


                'DELETE HPP KHUSUS MASUK (I)
                If Len(ftHppI) > 0 Then
                    sql = "DELETE FROM m1_cogs_special_in WHERE " & ftHppI
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                'DELETE HPP FIFO MASUK (F)
                If Len(ftHppF) > 0 Then
                    sql = "DELETE FROM m1_cogs_fifo_in WHERE " & ftHppF
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


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


                'UPDATE NO ASSET ===============================================================
                Dim dtasset As DataTable = AsDataTableAmbilDariDBCon("SELECT atasetid FROM m7_asset_transaction WHERE atsumber = '" & sumber & "' AND atidutama = '" & idtransaksi & "'", myConn)

                'HAPUS ASET IN
                Dim dtAssetIn As DataTable = AsDataTableFilterSortDt(dtasset, "atjenismutasi = '1'")
                If dtAssetIn.Rows.Count > 0 Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtAssetIn.Rows
                        'QUERY INSERT NO ASSET IN
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append(FixDouble(dr1("atasetid")))
                    Next
                    sql = "DELETE FROM m7_asset a WHERE a.aid IN(" & strValue2.ToString & ")"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'KEMBALIKAN ASET OUT
                Dim dtAssetOut As DataTable = AsDataTableFilterSortDt(dtasset, "atjenismutasi = '0'")
                If dtAssetOut.Rows.Count > 0 Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtAssetOut.Rows
                        'QUERY INSERT NO ASSET IN
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append(FixDouble(dr1("atasetid")))
                    Next
                    sql = "UPDATE m7_asset a SET a.aakumulasibeban = a.aakumulasibebansebelumnya, a.anilaibuku = a.anilaibukusebelumnya, a.aisclose = 0, a.atglclose = '1900-01-01' WHERE a.aid IN(" & strValue2.ToString & ")"
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


                'UPDATE NO BATCH ================================================================
                Dim updNilaiBatch As String = "", updFilterBatch As String = ""
                Dim dtBatch As DataTable = AsDataTableAmbilDariDBCon("SELECT nboidbatchin, nbogudang, nboidbarang, nbokode, nbojmlkeluar FROM m1_no_batch_out WHERE nbosumber = '" & sumber & "' AND nboidtransaksi = '" & idtransaksi & "'", myConn)
                If dtBatch.Rows.Count > 0 Then
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


                'UPDATE STOK ===================================================================
                'STOK KELUAR
                If Len(updStokOut) > 0 Then
                    sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokOut & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'STOK KELUAR BARANG m1_item
                If Len(updStokBarangKeluar) > 0 Then
                    sql = "UPDATE m1_item SET bstok = (CASE bid " & updStokBarangKeluar & " ELSE bstok END) WHERE " & ftStokBarangKeluar
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'STOK MASUK
                If Len(updStokIn) > 0 Then
                    sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokIn & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'STOK MASUK BARANG m1_item
                If Len(updStokBarangMasuk) > 0 Then
                    sql = "UPDATE m1_item SET bstok = (CASE bid " & updStokBarangMasuk & " ELSE bstok END) WHERE " & ftStokBarangMasuk
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
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


                'UPDATE BHPPAVERAGE M1_ITEM ===================================================
                'sql = "  UPDATE m1_item i"
                'sql &= " JOIN m3_sa_detail sad ON i.bid = sad.idbarang AND sad.jmlbarangmasuk <> 0 AND sad.idsa = '" & FixDouble(idtransaksi) & "'"
                'sql &= " LEFT JOIN"
                'sql &= " (SELECT i.bid as idbarang, ROUND(SUM(it.jmlbarang * it.hpp) / SUM(it.jmlbarang),2) as hppaverage"
                'sql &= " FROM m1_item_transaction it"
                'sql &= " JOIN m1_item i ON it.idbarang = i.bid AND it.jenismutasi = 1"
                'sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1"
                'sql &= " JOIN m3_sa_detail sad ON it.idbarang = sad.idbarang AND sad.jmlbarangmasuk <> 0 AND sad.idsa = '" & FixDouble(idtransaksi) & "'"
                'sql &= " JOIN m3_sa sa ON sad.idsa = sa.said AND CONCAT(it.sumber,it.idutama) <> CONCAT(sa.sasumber,sa.said)"
                'sql &= " GROUP BY it.idbarang) as h ON i.bid = h.idbarang"
                'sql &= " SET i.bhppaverage = (CASE i.bjenis WHEN 'P' THEN (CASE i.bstok WHEN 0 THEN 0 ELSE IFNULL(h.hppaverage,0) END) ELSE IFNULL(h.hppaverage,0) END)"

                'SA MASUK
                sql = "  UPDATE m1_item i"
                sql &= " JOIN ("
                sql &= " SELECT sad.idbarang, ROUND(SUM(sad.jmlbarangmasuk * sad.hpp),2) as nilai, SUM(sad.jmlbarangmasuk) as jumlah"
                sql &= " FROM m3_sa_detail sad"
                sql &= " WHERE sad.jmlbarangmasuk <> 0 AND sad.idsa = '" & FixDouble(idtransaksi) & "'"
                sql &= " GROUP BY sad.idbarang"
                sql &= " ) as h ON i.bid = h.idbarang"
                sql &= " SET i.bhppaverage = (CASE i.bjenis WHEN 'P' THEN (CASE i.bstok WHEN 0 THEN 0 ELSE ROUND((((i.bstok + h.jumlah) * i.bhppaverage) - (h.nilai)) / (i.bstok),2) END) ELSE IFNULL(ROUND((((i.bstok + h.jumlah) * i.bhppaverage) - (h.nilai)) / (i.bstok),2),0) END)"
                'result(2) = sql : Trans.Rollback() : GoTo selesai
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'SA KELUAR
                sql = "  UPDATE m1_item i"
                sql &= " JOIN ("
                sql &= " SELECT sad.idbarang, ROUND(SUM(sad.jmlbarangkeluar * sad.hpp),2) as nilai, SUM(sad.jmlbarangkeluar) as jumlah"
                sql &= " FROM m3_sa_detail sad"
                sql &= " WHERE sad.jmlbarangkeluar <> 0 AND sad.idsa = '" & FixDouble(idtransaksi) & "'"
                sql &= " GROUP BY sad.idbarang"
                sql &= " ) as h ON i.bid = h.idbarang"
                sql &= " SET i.bhppaverage = (CASE i.bjenis WHEN 'P' THEN (CASE i.bstok WHEN 0 THEN 0 ELSE ROUND((((i.bstok - h.jumlah) * i.bhppaverage) + (h.nilai)) / (i.bstok),2) END) ELSE IFNULL(ROUND((((i.bstok - h.jumlah) * i.bhppaverage) + (h.nilai)) / (i.bstok),2),0) END)"
                'result(2) = sql : Trans.Rollback() : GoTo selesai
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF UPDATE BHPPAVERAGE M1_ITEM ============================================


                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = '" & sumber & "' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

            End If

            'update status utama
            sql = "UPDATE M3_Sa SET Sastatus = " & nilaiStatus & ", Samodifikasiuser='" & userid & "', Samodifikasitgl = NOW(), Saposting = 0, Sapostingtgl = '1971-01-01 00:00:00', Sajmlrevisi = Sajmlrevisi + 1 WHERE Said = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M3_SaSearch(PostWsSearch(paramSplit(0), "M3_SaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
            result(2) = ex.Message & " === " & sql
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
    Public Function M3_SaDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("sabagiansakode", "c1.kkode")
            Filter = Filter.Replace("sabagiansanama", "c1.knama")
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
            Dim sumber As String = "SA", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Said, Sanotransaksi FROM M3_Sa WHERE Said='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT sacabang, salokasi, sasumber, saautonotransaksi, sanotransaksi, satgl"
            sql &= " FROM M3_sa"
            sql &= " WHERE said = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("sacabang")
                lokasi = dtNomorNext.Rows(0)("salokasi")
                sumber = dtNomorNext.Rows(0)("sasumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("saautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("sanotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("satgl"))
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
            sql = "DELETE FROM M3_Sa_Detail WHERE idsa = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE UTAMA
            sql = "DELETE FROM M3_Sa WHERE said = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M3_SaSearch(PostWsSearch(paramSplit(0), "M3_SaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M3_SaGetdataById(ByVal param As String) As String

        'M3_SaGetdataById Utama --------------------------------------------------------
        'said, sacabang, salokasi, sagudang, sasumber, sajenis, saautonotransaksi, 
        'sanotransaksi, satgl, sakodepa, sabagiansa, sabagiansakontak, sauraian, sacatatan, 
        'sanoref, satglnoref, saidsp, sastatus, sastatussebelumnya, sajmlrevisi, sacetakanke, 
        'sainputuser, sainputtgl, samodifikasiuser, samodifikasitgl, saposting, sapostingtgl, satutupperiode, 
        'saisclose, sacustomtext1, sacustomtext2, sacustomtext3, sacustomtext4, sacustomtext5, sacustomint1, 
        'sacustomint2, sacustomint3, sacustomdbl1, sacustomdbl2, sacustomdbl3, sacustomdate1, sacustomdate2, 
        'sacustomdate3, sacabangnama, salokasinama, sagudangnama, sajenisnama, sajenisrek, sabagiansakode, 
        'sabagiansanama, sanotransaksisp, sastatusnama, sastatussebelumnyanama, sainputusernama, samodifikasiusernama

        'M3_SaGetdataById Detail -------------------------------------------------------
        'idsadetail, idsa, idbarang, namabarang, tipebarang, jmlmasuk, jmlkeluar, satuan, 
        'nilaisatuan, jmlbarangmasuk, jmlbarangkeluar, satuanbarang, idhppkhususmasuk, hpplama, hpp, 
        'rekpersediaan, reklawan, idspdetail, cabang, lokasi, gudang, costcenter, 
        'divisi, subdivisi, proyek, catatan, urutan, isclose, customtext1, 
        'customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3, kodebarang, bhpp, bhppaverage, bjenis, bserial, bbatch, 
        'rekpersediaannama, reklawannama, spnotransaksi, cabangnama, lokasinama, gudangnama, costcenternama, 
        'divisinama, subdivisinama, proyeknama, bapanjang, balebar, batinggi

        'M3_SaGetdataById Batch --------------------------------------------------------
        'nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang

        'M3_SaGetdataById Serial --------------------------------------------------------
        'nstid, nstjenismutasi, nstidserialin, nstgudang,  nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, 
        'nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, 
        'nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang

        'M3_SaGetdataById Asset --------------------------------------------------------
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

        Dim utama As String = "", detail As String = "", idtransaksi As String = "", batch As String = "", serial As String = ""
        Dim sumber As String = "SA", asset As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M3_Sa~M3_Sa_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "said = " & idtransaksi
        Else ' jika filter diisi
            Filter = "said = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m3_sa_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("said"), 0), sptField,
                     FxDB(drutama("sacabang"), ""), sptField,
                     FxDB(drutama("salokasi"), ""), sptField,
                     FxDB(drutama("sagudang"), ""), sptField,
                     FxDB(drutama("sasumber"), ""), sptField,
                     FxDB(drutama("sajenis"), ""), sptField,
                     FxDB(drutama("saautonotransaksi"), 0), sptField,
                     FxDB(drutama("sanotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("satgl"), ""), formatTgl), sptField,
                     FxDB(drutama("sakodepa"), 0), sptField,
                     FxDB(drutama("sabagiansa"), 0), sptField,
                     FxDB(drutama("sabagiansakontak"), ""), sptField,
                     FxDB(drutama("sauraian"), ""), sptField,
                     FxDB(drutama("sacatatan"), ""), sptField,
                     FxDB(drutama("sanoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("satglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("saidsp"), 0), sptField,
                     FxDB(drutama("sastatus"), 0), sptField,
                     FxDB(drutama("sastatussebelumnya"), 0), sptField,
                     FxDB(drutama("sajmlrevisi"), 0), sptField,
                     FxDB(drutama("sacetakanke"), 0), sptField,
                     FxDB(drutama("sainputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sainputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("samodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("samodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("saposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sapostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("satutupperiode"), 0), sptField,
                     FxDB(drutama("saisclose"), 0), sptField,
                     FxDB(drutama("sacustomtext1"), ""), sptField,
                     FxDB(drutama("sacustomtext2"), ""), sptField,
                     FxDB(drutama("sacustomtext3"), ""), sptField,
                     FxDB(drutama("sacustomtext4"), ""), sptField,
                     FxDB(drutama("sacustomtext5"), ""), sptField,
                     FxDB(drutama("sacustomint1"), 0), sptField,
                     FxDB(drutama("sacustomint2"), 0), sptField,
                     FxDB(drutama("sacustomint3"), 0), sptField,
                     FxDB(drutama("sacustomdbl1"), 0), sptField,
                     FxDB(drutama("sacustomdbl2"), 0), sptField,
                     FxDB(drutama("sacustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sacustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sacustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sacustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("sacabangnama"), ""), sptField,
                     FxDB(drutama("salokasinama"), ""), sptField,
                     FxDB(drutama("sagudangnama"), ""), sptField,
                     FxDB(drutama("sajenisnama"), ""), sptField,
                     FxDB(drutama("sajenisrek"), ""), sptField,
                     FxDB(drutama("sabagiansakode"), ""), sptField,
                     FxDB(drutama("sabagiansanama"), ""), sptField,
                     FxDB(drutama("sanotransaksisp"), ""), sptField,
                     FxDB(drutama("sastatusnama"), ""), sptField,
                     FxDB(drutama("sastatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("sainputusernama"), ""), sptField,
                     FxDB(drutama("samodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idsadetail"), 0), sptField,
                     FxDB(dr("idsa"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jmlmasuk"), 0), sptField,
                     FxDB(dr("jmlkeluar"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarangmasuk"), 0), sptField,
                     FxDB(dr("jmlbarangkeluar"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("idhppkhususmasuk"), 0), sptField,
                     FxDB(dr("hpplama"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("rekpersediaan"), ""), sptField,
                     FxDB(dr("reklawan"), ""), sptField,
                     FxDB(dr("idspdetail"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
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
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bhppaverage"), 0), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("basset"), 0), sptField,
                     FxDB(dr("rekpersediaannama"), ""), sptField,
                     FxDB(dr("reklawannama"), ""), sptField,
                     FxDB(dr("spnotransaksi"), ""), sptField,
                     FxDB(dr("cabangnama"), ""), sptField,
                     FxDB(dr("lokasinama"), ""), sptField,
                     FxDB(dr("gudangnama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA BATCH
            sql = "select `nbt`.`nbtid` AS `nbtid`,`nbt`.`nbtjenismutasi` AS `nbtjenismutasi`,`nbt`.`nbtidbatchin` AS `nbtidbatchin`,`nbt`.`nbtgudang` AS `nbtgudang`,`nbt`.`nbtidbarang` AS `nbtidbarang`,`nbt`.`nbtkode` AS `nbtkode`,`nbt`.`nbtsumber` AS `nbtsumber`,`nbt`.`nbtidtransaksi` AS `nbtidtransaksi`,`nbt`.`nbtsatuan` AS `nbtsatuan`,`nbt`.`nbtjml` AS `nbtjml`,`nbt`.`nbtcustomtext1` AS `nbtcustomtext1`,`nbt`.`nbtcustomtext2` AS `nbtcustomtext2`,`nbt`.`nbtcustomtext3` AS `nbtcustomtext3`,`nbt`.`nbtcustomdbl1` AS `nbtcustomdbl1`,`nbt`.`nbtcustomdbl2` AS `nbtcustomdbl2`,`nbt`.`nbtcustomdbl3` AS `nbtcustomdbl3`,`nbt`.`nbtcustomdate1` AS `nbtcustomdate1`,`nbt`.`nbtcustomdate2` AS `nbtcustomdate2`,`nbt`.`nbtcustomdate3` AS `nbtcustomdate3`,`i`.`bkode` AS `kodebarang`, nbinotransaksi from ((`m1_no_batch_transaction` `nbt` join `m1_item` `i` on((`nbt`.`nbtidbarang` = `i`.`bid`))) left join `m1_no_batch_in` `nbi` on((`nbt`.`nbtidbatchin` = `nbi`.`nbiidbatchin`)))"
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
            sql = "select `nst`.`nstid` AS `nstid`,`nst`.`nstjenismutasi` AS `nstjenismutasi`,`nst`.`nstidserialin` AS `nstidserialin`,`nst`.`nstgudang` AS `nstgudang`,`nst`.`nstidbarang` AS `nstidbarang`,`nst`.`nstkode` AS `nstkode`,`nst`.`nstsumber` AS `nstsumber`,`nst`.`nstidtransaksi` AS `nstidtransaksi`,`nst`.`nstsatuan` AS `nstsatuan`,`nst`.`nstjml` AS `nstjml`,`nst`.`nstcustomtext1` AS `nstcustomtext1`,`nst`.`nstcustomtext2` AS `nstcustomtext2`,`nst`.`nstcustomtext3` AS `nstcustomtext3`,`nst`.`nstcustomdbl1` AS `nstcustomdbl1`,`nst`.`nstcustomdbl2` AS `nstcustomdbl2`,`nst`.`nstcustomdbl3` AS `nstcustomdbl3`,`nst`.`nstcustomdate1` AS `nstcustomdate1`,`nst`.`nstcustomdate2` AS `nstcustomdate2`,`nst`.`nstcustomdate3` AS `nstcustomdate3`,`i`.`bkode` AS `kodebarang`, nsinotransaksi from ((`m1_no_serial_transaction` `nst` join `m1_item` `i` on((`nst`.`nstidbarang` = `i`.`bid`))) left join `m1_no_serial_in` `nsi` on((`nst`.`nstidserialin` = `nsi`.`nsiidserialin`)))"
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
            'sql = "select atr.atid AS atid, atr.atasetid AS atasetid, atr.atjenismutasi AS atjenismutasi, atr.atsumber AS atsumber, atr.atidutama AS atidutama,atr.atidbarang AS atidbarang,atr.atkode AS atkode, atr.atnama AS atnama, atr.atkategori AS atkategori, atr.atcabang AS atcabang, atr.atlokasi AS atlokasi, atr.atgudang AS atgudang,atr.atdivisi AS atdivisi, atr.atsubdivisi AS atsubdivisi, atr.atcostcenter AS atcostcenter, atr.atproyek AS atproyek, atr.atcatatan AS atcatatan, atr.atnomor AS atnomor, atr.attglbeli AS attglbeli, atr.attglpakai AS attglpakai, atr.atjml AS atjml, atr.atsatuan AS atsatuan, atr.atmatauang AS atmatauang, atr.atkurs AS atkurs, atr.atharga AS atharga, atr.atdiskon AS atdiskon, atr.atjmldiskon AS atjmldiskon, atr.atpajak1 AS atpajak1, atr.atjmlpajak1 AS atjmlpajak1, atr.atpajak2 AS atpajak2, atr.atjmlpajak2 AS atjmlpajak2, atr.athargabeli AS athargabeli, atr.atnilairesidu AS atnilairesidu, atr.atumurekonomis AS atumurekonomis, atr.atbebanperbln AS atbebanperbln, atr.atakumulasibeban AS atakumulasibeban, atr.atnilaibuku AS atnilaibuku, (CASE WHEN atr.atnilaibuku < atr.atbebanperbln THEN atr.atnilaibuku ELSE atr.atbebanperbln END) as atnilaipenyusutan, atr.atmetode AS atmetode, atr.attabelpenyusutan AS attabelpenyusutan, atr.atintangible AS atintangible, atr.atfiskal AS atfiskal, atr.atatastengahbulan AS atatastengahbulan, atr.atrekasset AS atrekasset, atr.atrekakumdepresiasi AS atrekakumdepresiasi, atr.atrekdepresiasi AS atrekdepresiasi, atr.atrekpenghapusan AS atrekpenghapusan, atr.atprodusen AS atprodusen, atr.attglpensiun AS attglpensiun, atr.atpenyusutanke AS atpenyusutanke, atr.atnilaimenurun AS atnilaimenurun, atr.atdispose AS atdispose, atr.atpembelian AS atpembelian, atr.atpenjualan AS atpenjualan, atr.atlocked AS atlocked, atr.atstatus AS atstatus, atr.atstatussebelumnya AS atstatussebelumnya, atr.atisclose AS atisclose, atr.atinputuser AS atinputuser, atr.atinputtgl AS atinputtgl, atr.atmodifikasiuser AS atmodifikasiuser, atr.atmodifikasitgl AS atmodifikasitgl, atr.atcustomtext1,atr.atcustomtext2,atr.atcustomtext3,atr.atcustomtext4,atr.atcustomtext5,atr.atcustomint1,atr.atcustomint2,atr.atcustomint3,atr.atcustomint4,atr.atcustomint5,atr.atcustomdbl1,atr.atcustomdbl2,atr.atcustomdbl3,atr.atcustomdbl4,atr.atcustomdbl5,atr.atcustomdate1,atr.atcustomdate2,atr.atcustomdate3,atr.atcustomdate4,atr.atcustomdate5,ac.acnama AS atkategorinama, br.bnama AS atcabangnama, l.lnama AS atlokasinama, w.wnama AS atgudangnama,d.dnama AS atdivisinama, sd.sdnama AS atsubdivisinama, cc.ccnama AS atcostcenternama, p.pnama AS atproyeknama, dc.nama AS atmetodenama, t1.tnama AS atpajak1nama, ifnull(t1.tnilai, 0) AS atpajak1nilai, t2.tnama AS atpajak2nama, ifnull(t2.tnilai, 0) AS atpajak2nilai,coa1.cnama AS atrekassetnama, coa2.cnama AS atrekakumdepresiasinama, coa3.cnama AS atrekdepresiasinama, coa4.cnama AS atrekpenghapusannama, c1.kkode AS atprodusenkode, c1.knama AS atprodusennama,  sp1.nama AS atstatusnama,  sp2.nama AS atstatussebelumnyanama,  u1.unama AS atinputusernama,  u2.unama AS atmodifikasiusernama from m7_asset_transaction atr left join m7_asset_category ac on atr.atkategori = ac.ackode left join m1_branch br on atr.atcabang = br.bkode left join m1_location l on atr.atlokasi = l.lkode left join m1_warehouse w on atr.atgudang = w.wkode left join m1_division d on atr.atdivisi = d.dkode left join m1_subdivision sd on atr.atsubdivisi = sd.sdkode left join m7_depreciation_category dc on atr.atmetode = dc.kode left join m1_coa coa1 on atr.atrekasset = coa1.cnomor left join m1_coa coa2 on atr.atrekakumdepresiasi = coa2.cnomor left join m1_coa coa3 on atr.atrekdepresiasi = coa3.cnomor left join m1_coa coa4 on atr.atrekpenghapusan = coa4.cnomor left join m1_contact c1 on atr.atprodusen = c1.kid left join m0_status_progress sp1 on atr.atstatus = sp1.kode left join m0_status_progress sp2 on atr.atstatussebelumnya = sp2.kode left join m0_user u1 on atr.atinputuser = u1.userid left join m0_user u2 on atr.atmodifikasiuser = u2.userid left join m1_cost_center cc on atr.atcostcenter = cc.cckode left join m1_project p on atr.atproyek = p.pkode left join m1_tax t1 on atr.atpajak1 = t1.tkode left join m1_tax t2 on atr.atpajak2 = t2.tkode"
            sql = "select atr.atid AS atid, atr.atasetid AS atasetid, atr.atjenismutasi AS atjenismutasi, atr.atsumber AS atsumber, atr.atidutama AS atidutama,atr.atidbarang AS atidbarang,atr.atkode AS atkode, atr.atnama AS atnama, atr.atkategori AS atkategori, atr.atcabang AS atcabang, atr.atlokasi AS atlokasi, atr.atgudang AS atgudang,atr.atdivisi AS atdivisi, atr.atsubdivisi AS atsubdivisi, atr.atcostcenter AS atcostcenter, atr.atproyek AS atproyek, atr.atcatatan AS atcatatan, atr.atnomor AS atnomor, atr.attglbeli AS attglbeli, atr.attglpakai AS attglpakai, atr.atjml AS atjml, atr.atsatuan AS atsatuan, atr.atmatauang AS atmatauang, atr.atkurs AS atkurs, atr.atharga AS atharga, atr.atdiskon AS atdiskon, atr.atjmldiskon AS atjmldiskon, atr.atpajak1 AS atpajak1, atr.atjmlpajak1 AS atjmlpajak1, atr.atpajak2 AS atpajak2, atr.atjmlpajak2 AS atjmlpajak2, atr.athargabeli AS athargabeli, atr.atnilairesidu AS atnilairesidu, atr.atumurekonomis AS atumurekonomis, atr.atbebanperbln AS atbebanperbln, atr.atakumulasibeban AS atakumulasibeban, atr.atnilaibuku AS atnilaibuku, (CASE WHEN atr.atnilaibuku < atr.atbebanperbln THEN atr.atnilaibuku ELSE atr.atbebanperbln END) as atnilaipenyusutan, atr.atmetode AS atmetode, atr.attabelpenyusutan AS attabelpenyusutan, atr.atintangible AS atintangible, atr.atfiskal AS atfiskal, atr.atatastengahbulan AS atatastengahbulan, atr.atrekasset AS atrekasset, atr.atrekakumdepresiasi AS atrekakumdepresiasi, atr.atrekdepresiasi AS atrekdepresiasi, atr.atrekpenghapusan AS atrekpenghapusan, atr.atprodusen AS atprodusen, atr.attglpensiun AS attglpensiun, atr.atpenyusutanke AS atpenyusutanke, atr.atnilaimenurun AS atnilaimenurun, atr.atdispose AS atdispose, atr.atpembelian AS atpembelian, atr.atpenjualan AS atpenjualan, atr.atlocked AS atlocked, atr.atstatus AS atstatus, atr.atstatussebelumnya AS atstatussebelumnya, atr.atisclose AS atisclose, atr.atinputuser AS atinputuser, atr.atinputtgl AS atinputtgl, atr.atmodifikasiuser AS atmodifikasiuser, atr.atmodifikasitgl AS atmodifikasitgl, atr.atcustomtext1,atr.atcustomtext2,atr.atcustomtext3,atr.atcustomtext4,atr.atcustomtext5,atr.atcustomint1,atr.atcustomint2,atr.atcustomint3,atr.atcustomint4,atr.atcustomint5,atr.atcustomdbl1,atr.atcustomdbl2,atr.atcustomdbl3,atr.atcustomdbl4,atr.atcustomdbl5,atr.atcustomdate1,atr.atcustomdate2,atr.atcustomdate3,atr.atcustomdate4,atr.atcustomdate5,ac.acnama AS atkategorinama, br.bnama AS atcabangnama, l.lnama AS atlokasinama, w.wnama AS atgudangnama,d.dnama AS atdivisinama, sd.sdnama AS atsubdivisinama, cc.ccnama AS atcostcenternama, p.pnama AS atproyeknama, dc.nama AS atmetodenama, t1.tnama AS atpajak1nama, ifnull(t1.tnilai, 0) AS atpajak1nilai, t2.tnama AS atpajak2nama, ifnull(t2.tnilai, 0) AS atpajak2nilai,coa1.cnama AS atrekassetnama, coa2.cnama AS atrekakumdepresiasinama, coa3.cnama AS atrekdepresiasinama, coa4.cnama AS atrekpenghapusannama, c1.kkode AS atprodusenkode, c1.knama AS atprodusennama,  sp1.nama AS atstatusnama,  sp2.nama AS atstatussebelumnyanama,  u1.unama AS atinputusernama,  u2.unama AS atmodifikasiusernama, i.bkode as kodebarang from m7_asset_transaction atr left join m7_asset_category ac on atr.atkategori = ac.ackode left join m1_branch br on atr.atcabang = br.bkode left join m1_location l on atr.atlokasi = l.lkode left join m1_warehouse w on atr.atgudang = w.wkode left join m1_division d on atr.atdivisi = d.dkode left join m1_subdivision sd on atr.atsubdivisi = sd.sdkode left join m7_depreciation_category dc on atr.atmetode = dc.kode left join m1_coa coa1 on atr.atrekasset = coa1.cnomor left join m1_coa coa2 on atr.atrekakumdepresiasi = coa2.cnomor left join m1_coa coa3 on atr.atrekdepresiasi = coa3.cnomor left join m1_coa coa4 on atr.atrekpenghapusan = coa4.cnomor left join m1_contact c1 on atr.atprodusen = c1.kid left join m0_status_progress sp1 on atr.atstatus = sp1.kode left join m0_status_progress sp2 on atr.atstatussebelumnya = sp2.kode left join m0_user u1 on atr.atinputuser = u1.userid left join m0_user u2 on atr.atmodifikasiuser = u2.userid left join m1_cost_center cc on atr.atcostcenter = cc.cckode left join m1_project p on atr.atproyek = p.pkode left join m1_tax t1 on atr.atpajak1 = t1.tkode left join m1_tax t2 on atr.atpajak2 = t2.tkode JOIN m1_item i on i.bid = atr.atidbarang"
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
                     FxDB(dr("atmodifikasiusernama"), ""), sptField,
                     FxDB(dr("kodebarang"), ""), sptRow)
            Next
            If asset.Length > 0 Then asset = asset.Substring(0, asset.Length - sptRow.Length) Else asset = asset


            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "SA transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, batch, sptSubParam, serial, sptSubParam, asset)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("said, sacabang, salokasi, sagudang, sasumber, sajenis, saautonotransaksi, sanotransaksi, satgl, sakodepa, sabagiansa, sabagiansakontak, sauraian, sacatatan, sanoref, satglnoref, saidsp, sastatus, sastatussebelumnya, sajmlrevisi, sacetakanke, sainputuser, sainputtgl, samodifikasiuser, samodifikasitgl, saposting, sapostingtgl, satutupperiode, saisclose, sacustomtext1, sacustomtext2, sacustomtext3, sacustomtext4, sacustomtext5, sacustomint1, sacustomint2, sacustomint3, sacustomdbl1, sacustomdbl2, sacustomdbl3, sacustomdate1, sacustomdate2, sacustomdate3, sacabangnama, salokasinama, sagudangnama, sajenisnama, sajenisrek, sabagiansakode, sabagiansanama, sanotransaksisp, sastatusnama, sastatussebelumnyanama, sainputusernama, samodifikasiusernama" & sptSubParam & "idsadetail, idsa, idbarang, namabarang, tipebarang, jmlmasuk, jmlkeluar, satuan, nilaisatuan, jmlbarangmasuk, jmlbarangkeluar, satuanbarang, idhppkhususmasuk, hpplama, hpp, rekpersediaan, reklawan, idspdetail, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bhppaverage, bjenis, bserial, bbatch, basset, rekpersediaannama, reklawannama, spnotransaksi, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, bapanjang, balebar, batinggi" & sptSubParam & "nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang, nbtnotransaksi" & sptSubParam & "nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang, nstnotransaksi" & sptSubParam & "atid, atasetid, atjenismutasi, atsumber, atidutama, atidbarang, atkode, atnama, atkategori, atcabang, atlokasi, atgudang, atdivisi, atsubdivisi, atcostcenter, atproyek, atcatatan, atnomor, attglbeli, attglpakai, atjml, atsatuan, atmatauang, atkurs, atharga, atdiskon, atjmldiskon, atpajak1, atjmlpajak1, atpajak2, atjmlpajak2, athargabeli, atnilairesidu, atumurekonomis, atbebanperbln, atakumulasibeban, atnilaibuku, atnilaipenyusutan, atmetode, attabelpenyusutan, atintangible, atfiskal, atatastengahbulan, atrekasset, atrekakumdepresiasi, atrekdepresiasi, atrekpenghapusan, atprodusen, attglpensiun, atpenyusutanke, atnilaimenurun, atdispose, atpembelian, atpenjualan, atlocked, atstatus, atstatussebelumnya, atisclose, atinputuser, atinputtgl, atmodifikasiuser, atmodifikasitgl, atcustomtext1, atcustomtext2, atcustomtext3, atcustomtext4, atcustomtext5, atcustomint1, atcustomint2, atcustomint3, atcustomint4, atcustomint5, atcustomdbl1, atcustomdbl2, atcustomdbl3, atcustomdbl4, atcustomdbl5, atcustomdate1, atcustomdate2, atcustomdate3, atcustomdate4, atcustomdate5, atkategorinama, atcabangnama, atlokasinama, atgudangnama, atdivisinama, atsubdivisinama, atcostcenternama, atproyeknama, atmetodenama, atpajak1nama, atpajak1nilai, atpajak2nama, atpajak2nilai, atrekassetnama, atrekakumdepresiasinama, atrekdepresiasinama, atrekpenghapusannama, atprodusenkode, atprodusennama, atstatusnama, atstatussebelumnyanama, atinputusernama, atmodifikasiusernama, kodebarang"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_SaSearch(ByVal param As String) As String
        'M3_SaSearch --------------------------------------------------------
        'said, sacabang, salokasi, sagudang, sasumber, sajenis, saautonotransaksi, 
        'sanotransaksi, satgl, sakodepa, sabagiansa, sabagiansakontak, sauraian, sacatatan, 
        'sanoref, satglnoref, saidsp, sastatus, sastatussebelumnya, sajmlrevisi, sacetakanke, 
        'sainputuser, sainputtgl, samodifikasiuser, samodifikasitgl, saposting, sapostingtgl, satutupperiode, 
        'saisclose, sacabangnama, salokasinama, sagudangnama, sajenisnama, sabagiansakode, sabagiansanama, 
        'sanotransaksisp, sastatusnama, sastatussebelumnyanama, sainputusernama, samodifikasiusernama

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
            Filter = Filter.Replace("sabagiansakode", "c1.kkode")
            Filter = Filter.Replace("sabagiansanama", "c1.knama")
            Filter = Filter.Replace("Sainputusernama", "u1.unama")
            Filter = Filter.Replace("Samodifikasiusernama", "u2.unama")
            Filter = Filter.Replace("Sajenisnama", "tsa.tsanama")
            Filter = Filter.Replace("Sastatusnama", "st1.nama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m3_sa_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M3_Sa", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("said"), 0), sptField,
                     FxDB(dr("sacabang"), ""), sptField,
                     FxDB(dr("salokasi"), ""), sptField,
                     FxDB(dr("sagudang"), ""), sptField,
                     FxDB(dr("sasumber"), ""), sptField,
                     FxDB(dr("sajenis"), ""), sptField,
                     FxDB(dr("saautonotransaksi"), 0), sptField,
                     FxDB(dr("sanotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("satgl"), ""), formatTgl), sptField,
                     FxDB(dr("sakodepa"), 0), sptField,
                     FxDB(dr("sabagiansa"), 0), sptField,
                     FxDB(dr("sabagiansakontak"), ""), sptField,
                     FxDB(dr("sauraian"), ""), sptField,
                     FxDB(dr("sacatatan"), ""), sptField,
                     FxDB(dr("sanoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("satglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("saidsp"), 0), sptField,
                     FxDB(dr("sastatus"), 0), sptField,
                     FxDB(dr("sastatussebelumnya"), 0), sptField,
                     FxDB(dr("sajmlrevisi"), 0), sptField,
                     FxDB(dr("sacetakanke"), 0), sptField,
                     FxDB(dr("sainputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sainputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("samodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("samodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("saposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sapostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("satutupperiode"), 0), sptField,
                     FxDB(dr("saisclose"), 0), sptField,
                     FxDB(dr("sacabangnama"), ""), sptField,
                     FxDB(dr("salokasinama"), ""), sptField,
                     FxDB(dr("sagudangnama"), ""), sptField,
                     FxDB(dr("sajenisnama"), ""), sptField,
                     FxDB(dr("sabagiansakode"), ""), sptField,
                     FxDB(dr("sabagiansanama"), ""), sptField,
                     FxDB(dr("sanotransaksisp"), ""), sptField,
                     FxDB(dr("sastatusnama"), ""), sptField,
                     FxDB(dr("sastatussebelumnyanama"), ""), sptField,
                     FxDB(dr("sainputusernama"), ""), sptField,
                     FxDB(dr("samodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("said, sacabang, salokasi, sagudang, sasumber, sajenis, saautonotransaksi, sanotransaksi, satgl, sakodepa, sabagiansa, sabagiansakontak, sauraian, sacatatan, sanoref, satglnoref, saidsp, sastatus, sastatussebelumnya, sajmlrevisi, sacetakanke, sainputuser, sainputtgl, samodifikasiuser, samodifikasitgl, saposting, sapostingtgl, satutupperiode, saisclose, sacabangnama, salokasinama, sagudangnama, sajenisnama, sabagiansakode, sabagiansanama, sanotransaksisp, sastatusnama, sastatussebelumnyanama, sainputusernama, samodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_SaTerkait(ByVal param As String) As String
        'M3_SaTerkait --------------------------------------------------------
        'said, sanotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        Dim idtransaksi As String = ""
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "said required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m3_sa_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("said"), 0), sptField,
                     FxDB(dr("sanotransaksi"), ""), sptField,
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
            result(2) = "Related SA data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("said, sanotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    Private Function ValidasiHppI(ByVal dtdetail As DataTable, ByVal ftBarang As String) As String
        Dim errmessage As String = "", sql As String = ""

        Dim dtval As New DataTable, dtbarang As New DataTable, dtHppI As New DataTable, dtLookup As New DataTable
        Dim ftExistHppI As String = "", ftHppI As String = "", filterLookup As String = ""
        Dim kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaisatuan As Double = 0, urutan As Double = 0, sisa As Double = 0

        '1. AMBIL BARANG HPP KHUSUS (I)
        dtbarang = AsDataTableAmbilDariDB("SELECT bid, bkode FROM m1_item WHERE (bjenis <> 'J') AND (bhpp = 'I') AND (" & ftBarang & ")")
        '2. CEK ID HPP KHUSUS MASUK
        If dtbarang.Rows.Count > 0 Then
            '3. PERULANGAN SEBANYAK BARANG HPP KHUSUS
            For Each dr1 As DataRow In dtbarang.Rows
                '4. AMBIL BARANG HPP KHUSUS DARI DETAIL
                dtHppI = AsDataTableFilterSortDt(dtdetail, "idbarang = '" & dr1("bid") & "' AND jmlbarangkeluar <> 0")
                If dtHppI.Rows.Count > 0 Then
                    For Each dr2 As DataRow In dtHppI.Rows
                        '5. BUAT FILTER CEK DATA EXIST HPP KHUSUS
                        ftExistHppI = IIf(Len(ftExistHppI.ToString) = 0, "", ftExistHppI & " UNION ")
                        ftExistHppI = String.Concat(ftExistHppI, "SELECT EXISTS(SELECT 1 FROM m1_cogs_special_in WHERE idhppikm = '" & dr2("idhppkhususmasuk") & "' LIMIT 1) as rowExists, '" & dr1("bid") & "' as idbarang, bkode FROM m1_item WHERE bid = '" & dr1("bid") & "'")

                        '6. BUAT FILTER CEK JML HPP KHUSUS
                        Dim StokHppI As Double = AsDataTableDSum(dtdetail, "jmlbarangkeluar", "idhppkhususmasuk=" & dr2("idhppkhususmasuk") & "")
                        ftHppI = IIf(Len(ftHppI.ToString) = 0, "", ftHppI & " OR ")
                        ftHppI = String.Concat(ftHppI, " (csi.idhppikm = " & dr2("idhppkhususmasuk") & " AND " & StokHppI & " > csi.sisa) ")
                    Next
                End If
            Next

            'VALIDASI HPP KHUSUS (I) ------------------------------------
            'CEK DATA EXIST/TIDAK
            If Len(ftExistHppI) > 0 Then
                dtval = AsDataTableAmbilDariDB(ftExistHppI) 'ftExistHppI = rowExists, idbarang, bkode
                filterLookup = "rowExists = 0"
                dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
                If dtval.Rows.Count > 0 Then
                    'Ambil informasi utk errmessage
                    kodebarang = dtval.Rows(0)("bkode")

                    filterLookup = "idbarang=" & dtval.Rows(0)("idbarang") & " AND jmlbarangkeluar <> 0"
                    dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    urutan = dtLookup.Rows(0)("urutan")

                    errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS Special list." : GoTo selesai
                End If
            End If

            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA YG TERSEDIA
            If Len(ftHppI) > 0 Then
                sql = "SELECT csi.idhppikm, csi.idbarang, csi.sisa, i.bkode FROM m1_cogs_special_in csi JOIN m1_item i ON csi.idbarang = i.bid WHERE " & ftHppI
                dtval = AsDataTableAmbilDariDB(sql)
                If dtval.Rows.Count > 0 Then
                    'Ambil informasi utk errmessage
                    kodebarang = dtval.Rows(0)("bkode")
                    sisa = dtval.Rows(0)("sisa")

                    filterLookup = "idhppkhususmasuk=" & dtval.Rows(0)("idhppikm")
                    dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                    If dtLookup.Rows.Count > 0 Then
                        tipebarang = dtLookup.Rows(0)("tipebarang")
                        namabarang = dtLookup.Rows(0)("namabarang")
                        satuan = dtLookup.Rows(0)("satuan")
                        nilaisatuan = dtLookup.Rows(0)("nilaiSatuan")
                        urutan = dtLookup.Rows(0)("urutan")
                    End If
                    errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of stock in COGS Special, item(s) available " & sisa / nilaisatuan & " " & satuan : GoTo selesai
                End If
            End If
            'END OF VALIDASI HPP KHUSUS (I) -----------------------------
        End If

selesai:
        Return errmessage
    End Function

    Private Function ValidasiHppF(ByVal dtdetail As DataTable, ByVal ftBarang As String) As String
        Dim errmessage As String = "", sql As String = ""

        Dim dtval As New DataTable, dtbarang As New DataTable, dtHppF As New DataTable, dtLookup As New DataTable
        Dim ftExistHppF As String = "", ftHppF As String = "", havingHppF As String = "", filterLookup As String = ""
        Dim kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaisatuan As Double = 0, urutan As Double = 0, sisa As Double = 0

        '1. AMBIL BARANG HPP FIFO (F)
        dtbarang = AsDataTableAmbilDariDB("SELECT bid, bkode FROM m1_item WHERE (bjenis <> 'J') AND (bhpp = 'F') AND (" & ftBarang & ")")
        '2. CEK ID HPP FIFO MASUK
        If dtbarang.Rows.Count > 0 Then
            '3. PERULANGAN SEBANYAK BARANG HPP FIFO
            For Each dr1 As DataRow In dtbarang.Rows
                '4. AMBIL BARANG HPP FIFO DARI DETAIL
                dtHppF = AsDataTableFilterSortDt(dtdetail, "idbarang = '" & dr1("bid") & "'  AND jmlbarangkeluar <> 0")
                If dtHppF.Rows.Count > 0 Then
                    For Each dr2 As DataRow In dtHppF.Rows
                        '5. BUAT FILTER CEK DATA EXIST HPP FIFO
                        ftExistHppF = IIf(Len(ftExistHppF.ToString) = 0, "", ftExistHppF & " UNION ")
                        ftExistHppF = String.Concat(ftExistHppF, "SELECT EXISTS(SELECT 1 FROM m1_cogs_fifo_in WHERE cfiisclose = 0 AND cfiidbarang = '" & dr1("bid") & "' LIMIT 1) as rowExists, '" & dr1("bid") & "' as idbarang, bkode FROM m1_item WHERE bid = '" & dr1("bid") & "'")
                        '6. BUAT FILTER CEK JML HPP FIFO
                        Dim StokHppF As Double = AsDataTableDSum(dtdetail, "jmlbarangkeluar", "idbarang=" & dr1("bid") & "")
                        ftHppF = IIf(Len(ftHppF.ToString) = 0, "", ftHppF & " OR ")
                        ftHppF = String.Concat(ftHppF, " (cfiidbarang = '" & dr1("bid") & "' AND cfiisclose = 0) ")
                        havingHppF = IIf(Len(havingHppF.ToString) = 0, "", havingHppF & " OR ")
                        havingHppF = String.Concat(havingHppF, " (cfiidbarang = '" & dr1("bid") & "' AND " & StokHppF & " > cfitotalsisa) ")
                    Next
                End If
            Next

            'VALIDASI HPP FIFO (F) ------------------------------------
            'CEK DATA EXIST/TIDAK
            If Len(ftExistHppF) > 0 Then
                dtval = AsDataTableAmbilDariDB(ftExistHppF) 'ftExistHppI = rowExists, idbarang, bkode
                filterLookup = "rowExists = 0"
                dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
                If dtval.Rows.Count > 0 Then
                    'Ambil informasi utk errmessage
                    kodebarang = dtval.Rows(0)("bkode")

                    filterLookup = "idbarang=" & dtval.Rows(0)("idbarang") & " AND jmlbarangkeluar <> 0"
                    dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    urutan = dtLookup.Rows(0)("urutan")

                    errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list." : GoTo selesai
                End If
            End If

            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA YG TERSEDIA
            If Len(ftHppF) > 0 Then
                sql = "SELECT bkode, cfiidbarang, SUM(cfisisa) as cfitotalsisa FROM m1_cogs_fifo_in JOIN m1_item ON cfiidbarang = bid WHERE " & ftHppF & " GROUP BY cfiidbarang HAVING " & havingHppF
                dtval = AsDataTableAmbilDariDB(sql)
                If dtval.Rows.Count > 0 Then
                    'Ambil informasi utk errmessage
                    kodebarang = dtval.Rows(0)("bkode")
                    sisa = dtval.Rows(0)("cfitotalsisa")

                    filterLookup = "idbarang=" & dtval.Rows(0)("cfiidbarang") & " AND jmlbarangkeluar <> 0"
                    dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                    If dtLookup.Rows.Count > 0 Then
                        tipebarang = dtLookup.Rows(0)("tipebarang")
                        namabarang = dtLookup.Rows(0)("namabarang")
                        satuan = dtLookup.Rows(0)("satuan")
                        nilaisatuan = dtLookup.Rows(0)("nilaiSatuan")
                        urutan = dtLookup.Rows(0)("urutan")
                    End If
                    errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of stock in COGS FIFO, item(s) available " & sisa / nilaisatuan & " " & satuan : GoTo selesai
                End If
            End If
            'END OF VALIDASI HPP FIFO (F) -----------------------------
        End If

selesai:
        Return errmessage
    End Function

    'Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstanding As String, ByVal ftOutstanding As String, ByVal ftExistOutstandingSO As String, ByVal ftOutstandingSO As String, ByVal ftExistStok As String, ByVal ftStok As String, ByVal ftStokAvailable As String, ByVal ftHppI As String, ByVal ftHppF As String, ByVal ftExistBatch As String, ByVal ftBatch As String, ByVal ftExistSerial As String, ByVal ftSerial As String, ByVal gudangBatchSerial As String) As String
    'Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstanding As String, ByVal ftOutstanding As String, ByVal ftExistStok As String, ByVal ftStok As String, ByVal ftStokAvailable As String, ByVal ftHppI As String, ByVal ftHppF As String, ByVal ftExistBatch As String, ByVal ftBatch As String, ByVal ftExistSerial As String, ByVal ftSerial As String, ByVal gudangBatchSerial As String) As String

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstanding As String, ByVal ftOutstanding As String, ByVal ftExistStok As String, ByVal ftStok As String, ByVal ftStokAvailable As String, ByVal ftHppI As String, ByVal ftHppF As String, ByVal ftExistBatch As String, ByVal ftBatch As String, ByVal ftExistSerial As String, ByVal ftSerial As String, ByVal gudangBatchSerial As String, ByVal ftStokAvailableCase As String) As String

        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable
        Dim strInsertStokKurang As String = "" 'variabel Insert ke tabel pembantu untuk barang stok tidak mencukupi m2r_stok_gagal_upload

        Dim dtLookup As New DataTable, kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaiSatuan As Double = 0, sisa As Double = 0
        Dim filterLookup As String = "", gudang As String = "", urutan As String = "", noBatch As String = "", noSerial As String = ""

        'VALIDASI OUTSTANDING ---------------------------------------
        If Len(ftExistOutstanding) > 0 Then 'ftExistOutstanding = rowExists, idspdetail, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstanding)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "idspdetail=" & dtval.Rows(0)("idspdetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in SP" : GoTo selesai
            End If

            ''PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
            'sql = "SELECT spd.idspdetail, (ABS(spd.selisihbarang) - spd.jmlsa) as sisasa, i.bid, i.bkode FROM m3_sp_detail AS spd INNER JOIN m1_item AS i ON spd.idbarang = i.bid WHERE " & ftOutstanding
            'dtval = AsDataTableAmbilDariDB(sql)
            'If dtval.Rows.Count > 0 Then
            '    'Ambil informasi utk errmessage
            '    kodebarang = dtval.Rows(0)("bkode")
            '    sisa = dtval.Rows(0)("sisasa")

            '    filterLookup = "idspdetail=" & dtval.Rows(0)("idspdetail")
            '    dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
            '    If dtLookup.Rows.Count > 0 Then
            '        tipebarang = dtLookup.Rows(0)("tipebarang")
            '        namabarang = dtLookup.Rows(0)("namabarang")
            '        satuan = dtLookup.Rows(0)("satuan")
            '        nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
            '        urutan = dtLookup.Rows(0)("urutan")
            '    End If
            '    errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in SP, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            'End If
        End If
        'END OF VALIDASI OUTSTANDING --------------------------------


        ''VALIDASI OUTSTANDING SO ---------------------------------------
        'If Len(ftExistOutstandingSO) > 0 Then 'ftExistOutstanding = rowExists, idsodetail, bkode
        '    'CEK DATA EXIST/TIDAK
        '    dtval = AsDataTableAmbilDariDB(ftExistOutstandingSO)
        '    filterLookup = "rowExists = 0"
        '    dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
        '    If dtval.Rows.Count > 0 Then
        '        'Ambil informasi utk errmessage
        '        kodebarang = dtval.Rows(0)("bkode")

        '        filterLookup = "customdbl2=" & dtval.Rows(0)("idsodetail")
        '        dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

        '        tipebarang = dtLookup.Rows(0)("tipebarang")
        '        namabarang = dtLookup.Rows(0)("namabarang")
        '        urutan = dtLookup.Rows(0)("urutan")

        '        errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in SO" : GoTo selesai
        '    End If


        '    'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
        '    sql = "SELECT sod.idsodetail, (sod.jmlbarang - sod.customdbl2) as sisarealisasi, i.bid, i.bkode FROM m5_so_detail AS sod INNER JOIN m1_item AS i ON sod.idbarang = i.bid WHERE " & ftOutstandingSO
        '    dtval = AsDataTableAmbilDariDB(sql)
        '    If dtval.Rows.Count > 0 Then
        '        'Ambil informasi utk errmessage
        '        kodebarang = dtval.Rows(0)("bkode")
        '        sisa = dtval.Rows(0)("sisarealisasi")

        '        filterLookup = "customdbl2=" & dtval.Rows(0)("idsodetail")
        '        dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
        '        If dtLookup.Rows.Count > 0 Then
        '            tipebarang = dtLookup.Rows(0)("tipebarang")
        '            namabarang = dtLookup.Rows(0)("namabarang")
        '            satuan = dtLookup.Rows(0)("satuan")
        '            nilaiSatuan = dtLookup.Rows(0)("nilaisatuan")
        '            urutan = dtLookup.Rows(0)("urutan")
        '        End If
        '        errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in SO, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
        '    End If
        'End If
        ''END OF VALIDASI OUTSTANDING --------------------------------


        Dim ProsesValidasiStok As String = F_getSetting(0, "company", "ValidasiStok")
        If ProsesValidasiStok.Equals("0") = False Then
            'VALIDASI STOK ----------------------------------------------
            If Len(ftExistStok) > 0 Then
                'CEK DATA EXIST/TIDAK
                dtval = AsDataTableAmbilDariDB(ftExistStok) 'ftExistStok = rowExists, idbarang, bkode, gudang, stoktersedia, stokjual
                filterLookup = "rowExists = 0"
                dtval = AsDataTableFilterSortDt(dtval, filterLookup)
                If dtval.Rows.Count > 0 Then

                    'Insert ke tabel pembantu untuk barang stok tidak mencukupi m2r_stok_gagal_upload
                    For Each drstok As DataRow In dtval.Rows
                        strInsertStokKurang = String.Concat(strInsertStokKurang, IIf(Len(strInsertStokKurang.ToString) = 0, "", ", "))
                        'idbarang, gudang, stoktersedia, stokjual
                        strInsertStokKurang = String.Concat(strInsertStokKurang, "('" & drstok("idbarang") & "', '" & drstok("gudang") & "', '" & drstok("stoktersedia") & "', '" & drstok("stokjual") & "')")
                    Next

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
            End If

            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA STOK PERGUDANG YG TERSEDIA
            If Len(ftStok) > 0 Then
                sql = "SELECT isw.idbarang, isw.kgudang, isw.stok, i.bkode FROM m1_item_stock_warehouse isw JOIN m1_item i ON isw.idbarang = i.bid AND i.bjenis <> 'J' WHERE " & ftStok
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
                        nilaiSatuan = dtLookup.Rows(0)("nilaisatuan")
                        urutan = dtLookup.Rows(0)("urutan")
                    End If
                    errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of stock in '" & gudang & "' warehouse, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
                End If
            End If


            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA STOK AVAILABLE PERGUDANG YG TERSEDIA
            If Len(ftStokAvailable) > 0 Then
                'sql = "SELECT isw.idbarang, isw.kgudang, isw.stok - IFNULL(isb.jmlbooking,0) as stok, i.bkode FROM m1_item_stock_warehouse isw JOIN m1_item i ON isw.idbarang = i.bid AND i.bjenis <> 'J' LEFT JOIN m1_item_booking isb ON isw.idbarang = isb.idbarang WHERE " & ftStokAvailable
                'sql = "SELECT isw.idbarang, isw.kgudang, isw.stok - IFNULL(isb.jmlbooking,0) as stok, i.bkode FROM m1_item_stock_warehouse isw JOIN m1_item i ON isw.idbarang = i.bid AND i.bjenis <> 'J' LEFT JOIN m1_warehouse w ON isw.kgudang = w.wkode LEFT JOIN m1_item_booking isb ON isw.idbarang = isb.idbarang AND w.wbookingstok = 1 WHERE " & ftStokAvailable
                sql = "SELECT isw.idbarang, isw.kgudang, isw.stok - IFNULL(isb.jmlbooking,0) as stok, i.bkode, (CASE " & ftStokAvailableCase & " ELSE 0 END) as stokjual FROM m1_item_stock_warehouse isw JOIN m1_item i ON isw.idbarang = i.bid AND i.bjenis <> 'J' LEFT JOIN m1_warehouse w ON isw.kgudang = w.wkode LEFT JOIN m1_item_booking isb ON isw.idbarang = isb.idbarang AND w.wbookingstok = 1 WHERE " & ftStokAvailable
                dtval = AsDataTableAmbilDariDB(sql)
                If dtval.Rows.Count > 0 Then

                    'Insert ke tabel pembantu untuk barang stok tidak mencukupi m2r_stok_gagal_upload
                    For Each drstok As DataRow In dtval.Rows
                        strInsertStokKurang = String.Concat(strInsertStokKurang, IIf(Len(strInsertStokKurang.ToString) = 0, "", ", "))
                        'idbarang, gudang, stoktersedia, stokjual
                        strInsertStokKurang = String.Concat(strInsertStokKurang, "('" & drstok("idbarang") & "', '" & drstok("kgudang") & "', '" & drstok("stok") & "', '" & drstok("stokjual") & "')")
                    Next

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
                        nilaiSatuan = dtLookup.Rows(0)("nilaisatuan")
                        urutan = dtLookup.Rows(0)("urutan")
                    End If
                    errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of stock in '" & gudang & "' warehouse, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
                End If
            End If
            'END OF VALIDASI STOK ---------------------------------------
        End If


        'VALIDASI HPP -----------------------------------------------
        'HPP KHUSUS (I)
        If Len(ftHppI) > 0 Then
            dtval = AsDataTableAmbilDariDB("SELECT idbarang, bkode FROM m1_cogs_special_in JOIN m1_item ON idbarang = bid AND bjenis <> 'J' WHERE (" & ftHppI & ") AND jmlkeluar > 0")
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                filterLookup = "idbarang=" & dtval.Rows(0)("idbarang")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")
                errmessage = "COGS Special for Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " has related transactions." : GoTo selesai
            End If
        End If

        'HPP FIFO (F)
        If Len(ftHppF) > 0 Then
            dtval = AsDataTableAmbilDariDB("SELECT cfiidbarang, bkode FROM m1_cogs_fifo_in JOIN m1_item ON cfiidbarang = bid AND bjenis <> 'J' WHERE (" & ftHppI & ") AND cfijmlkeluar > 0")
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                filterLookup = "idbarang=" & dtval.Rows(0)("cfiidbarang")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")
                errmessage = "COGS FIFO for Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " has related transactions." : GoTo selesai
            End If
        End If
        'END OF VALIDASI HPP ----------------------------------------


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
                    nilaiSatuan = dtLookup.Rows(0)("nilaisatuan")
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
                    nilaiSatuan = dtLookup.Rows(0)("nilaisatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " | No. Serial : " & noSerial & " exceeds the number of stock in No. Serial list, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If
        'END OF VALIDASI SERIAL --------------------------------------

selesai:

        'Insert ke tabel pembantu untuk barang stok tidak mencukupi m2r_stok_gagal_upload
        If Len(strInsertStokKurang) > 0 Then

            Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
            Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

            Con2 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
            Con2.Open()

            '*** Start Transaction ***'  
            Trans = Con2.BeginTransaction(IsolationLevel.ReadCommitted)

            Try

                'INSERT STOK GAGAL UPLOAD
                sql = "INSERT INTO m2r_stok_gagal_upload (idbarang, gudang, stoktersedia, stokjual) VALUES " & strInsertStokKurang
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con2
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                Trans.Commit()  '*** Commit Transaction ***'

            Catch ex As Exception
                Trans.Rollback() '*** RollBack Transaction ***'  
                errmessage = ex.Message

            End Try

        End If

        Return errmessage
    End Function

    <WebMethod()>
    Public Function M3_SaSimpanOld(ByVal param As String) As String
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
        Dim Filter As String = "", Sorting As String = ""

        'Dim cekBatch As Boolean

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


        'CEK NOREFF ========================================================================
        'CEK NOREFF UNTUK UPLOAD DATA POS, JIKA NOREFF TERISI MAKA CEK DATA YANG SUDAH ADA DI TABEL
        'JIKA NOREFF SUDAH ADA MAKA BERI KEMBALIAN BERHASIL
        'JIKA NOREF TIDAK ADA MAKA JALANKAN PROSES SIMPAN
        If Len(Filter) > 0 Then
            sql = "SELECT said, sanotransaksi FROM m3_sa WHERE sanoref = '" & FixQuotes(Filter) & "'"
            Dim dtNoreff As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNoreff.Rows.Count > 0 Then
                If Len(dtNoreff.Rows(0)("said")) > 0 Then
                    result(1) = 1
                    result(2) = dtNoreff.Rows(0)("sanotransaksi")
                    result(3) = 0
                    result(4) = dtNoreff.Rows(0)("said")
                    GoTo selesai
                End If
            End If

        Else
            Dim validKey As RsValidKey
            validKey = ValidateKey(paramSplit(0))
            If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        End If
        'END OF CEK NOREFF =================================================================


        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 4) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'said(0) As Integer, sacabang(1) As String, salokasi(2) As String, sagudang(3) As String, sasumber(4) As String, 
        'sajenis(5) As String, saautonotransaksi(6) As Integer, sanotransaksi(7) As String, satgl(8) As Date, sakodepa(9) As Integer, 
        'sabagiansa(10) As Integer, sabagiansakontak(11) As String, sauraian(12) As String, sacatatan(13) As String, sanoref(14) As String, 
        'satglnoref(15) As Date, saidsp(16) As Integer, sastatus(17) As Integer, sastatussebelumnya(18) As Integer, sajmlrevisi(19) As Integer, 
        'sacetakanke(20) As Integer, sainputuser(21) As Integer, sainputtgl(22) As DateTime, samodifikasiuser(23) As Integer, samodifikasitgl(24) As DateTime, 
        'saposting(25) As Integer, satutupperiode(26) As Integer, saisclose(27) As Integer, sacustomtext1(28) As String, sacustomtext2(29) As String, 
        'sacustomtext3(30) As String, sacustomtext4(31) As String, sacustomtext5(32) As String, sacustomint1(33) As Integer, sacustomint2(34) As Integer, 
        'sacustomint3(35) As Integer, sacustomdbl1(36) As Double, sacustomdbl2(37) As Double, sacustomdbl3(38) As Double, sacustomdate1(39) As Date, 
        'sacustomdate2(40) As Date, sacustomdate3(41) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'said, sacabang, salokasi, sagudang, sasumber, sajenis, saautonotransaksi, 
        'sanotransaksi, satgl, sakodepa, sabagiansa, sabagiansakontak, sauraian, sacatatan, 
        'sanoref, satglnoref, saidsp, sastatus, sastatussebelumnya, sajmlrevisi, sacetakanke, 
        'sainputuser, sainputtgl, samodifikasiuser, samodifikasitgl, saposting, satutupperiode, saisclose, 
        'sacustomtext1, sacustomtext2, sacustomtext3, sacustomtext4, sacustomtext5, sacustomint1, sacustomint2, 
        'sacustomint3, sacustomdbl1, sacustomdbl2, sacustomdbl3, sacustomdate1, sacustomdate2, sacustomdate3


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 42) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'said(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "said required numeric." : GoTo selesai
        End If
        'saautonotransaksi(6) As Integer
        If (IsNumeric(dataUtama(6)) = False) Then
            result(2) = "saautonotransaksi required numeric." : GoTo selesai
        End If
        'satgl(8) As Date
        If (IsDate(dataUtama(8)) = False) Then
            result(2) = "satgl required date." : GoTo selesai
        End If
        'sakodepa(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "sakodepa required numeric." : GoTo selesai
        End If
        'sabagiansa(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "sabagiansa required numeric." : GoTo selesai
        End If
        If (dataUtama(10) < 1) Then
            result(2) = "sabagiansa can't be empty." : GoTo selesai
        End If
        'satglnoref(15) As Date
        If (IsDate(dataUtama(15)) = False) Then
            result(2) = "satglnoref required date." : GoTo selesai
        End If
        'saidsp(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "saidsp required numeric." : GoTo selesai
        End If
        'sastatus(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "sastatus required numeric." : GoTo selesai
        End If
        'sastatussebelumnya(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "sastatussebelumnya required numeric." : GoTo selesai
        End If
        'sajmlrevisi(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "sajmlrevisi required numeric." : GoTo selesai
        End If
        'sacetakanke(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "sacetakanke required numeric." : GoTo selesai
        End If
        'sainputuser(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "sainputuser required numeric." : GoTo selesai
        End If
        'sainputtgl(22) As DateTime
        If (IsDate(dataUtama(22)) = False) Then
            result(2) = "sainputtgl required date." : GoTo selesai
        End If
        'samodifikasiuser(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "samodifikasiuser required numeric." : GoTo selesai
        End If
        'samodifikasitgl(24) As DateTime
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "samodifikasitgl required date." : GoTo selesai
        End If
        'saposting(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "saposting required numeric." : GoTo selesai
        End If
        'satutupperiode(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "satutupperiode required numeric." : GoTo selesai
        End If
        'saisclose(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "saisclose required numeric." : GoTo selesai
        End If
        'sacustomint1(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "sacustomint1 required numeric." : GoTo selesai
        End If
        'sacustomint2(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "sacustomint2 required numeric." : GoTo selesai
        End If
        'sacustomint3(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "sacustomint3 required numeric." : GoTo selesai
        End If
        'sacustomdbl1(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "sacustomdbl1 required numeric." : GoTo selesai
        End If
        'sacustomdbl2(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "sacustomdbl2 required numeric." : GoTo selesai
        End If
        'sacustomdbl3(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "sacustomdbl3 required numeric." : GoTo selesai
        End If
        'sacustomdate1(39) As Date
        If (IsDate(dataUtama(39)) = False) Then
            result(2) = "sacustomdate1 required date." : GoTo selesai
        End If
        'sacustomdate2(40) As Date
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "sacustomdate2 required date." : GoTo selesai
        End If
        'sacustomdate3(41) As Date
        If (IsDate(dataUtama(41)) = False) Then
            result(2) = "sacustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'sacabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "sacabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "sacabang should not be more than 25 character." : GoTo selesai
        End If

        'salokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "salokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "salokasi should not be more than 25 character." : GoTo selesai
        End If

        'sasumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "sasumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "sasumber should not be more than 10 character." : GoTo selesai
        End If

        'sanotransaksi(7) As String
        If Len(dataUtama(7)) = 0 Then
            result(2) = "sanotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 50 Then
            result(2) = "sanotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'satgl(8) As Date
        If Len(dataUtama(8)) = 0 Then
            result(2) = "satgl can't be empty" : GoTo selesai
        End If

        'satglnoref(15) As Date
        If Len(dataUtama(15)) = 0 Then
            result(2) = "satglnoref can't be empty" : GoTo selesai
        End If

        'sainputtgl(22) As DateTime
        If Len(dataUtama(22)) = 0 Then
            result(2) = "sainputtgl can't be empty" : GoTo selesai
        End If

        'samodifikasitgl(24) As DateTime
        If Len(dataUtama(24)) = 0 Then
            result(2) = "samodifikasitgl can't be empty" : GoTo selesai
        End If

        'sacustomdbl1(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "sacustomdbl1 can't be empty" : GoTo selesai
        End If

        'sacustomdbl2(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "sacustomdbl2 can't be empty" : GoTo selesai
        End If

        'sacustomdbl3(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "sacustomdbl3 can't be empty" : GoTo selesai
        End If

        'sacustomdate1(39) As Date
        If Len(dataUtama(39)) = 0 Then
            result(2) = "sacustomdate1 can't be empty" : GoTo selesai
        End If

        'sacustomdate2(40) As Date
        If Len(dataUtama(40)) = 0 Then
            result(2) = "sacustomdate2 can't be empty" : GoTo selesai
        End If

        'sacustomdate3(41) As Date
        If Len(dataUtama(41)) = 0 Then
            result(2) = "sacustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "said", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sacabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "salokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sagudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sasumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sajenis", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "saautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sanotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "satgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sakodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sabagiansa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sabagiansakontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sauraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sacatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sanoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "satglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "saidsp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sastatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sastatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sajmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sacetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sainputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sainputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "samodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "samodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "saposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "satutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "saisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sacustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sacustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sacustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sacustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sacustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sacustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sacustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sacustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sacustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sacustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sacustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sacustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sacustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sacustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "said~sacabang~salokasi~sagudang~sasumber~sajenis~saautonotransaksi~sanotransaksi~satgl~sakodepa~sabagiansa~sabagiansakontak~sauraian~sacatatan~sanoref~satglnoref~saidsp~sastatus~sastatussebelumnya~sajmlrevisi~sacetakanke~sainputuser~sainputtgl~samodifikasiuser~samodifikasitgl~saposting~satutupperiode~saisclose~sacustomtext1~sacustomtext2~sacustomtext3~sacustomtext4~sacustomtext5~sacustomint1~sacustomint2~sacustomint3~sacustomdbl1~sacustomdbl2~sacustomdbl3~sacustomdate1~sacustomdate2~sacustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idsadetail(0) As Integer, idsa(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jmlmasuk(5) As Double, jmlkeluar(6) As Double, satuan(7) As String, nilaisatuan(8) As Double, jmlbarangmasuk(9) As Double, 
        'jmlbarangkeluar(10) As Double, satuanbarang(11) As String, idhppkhususmasuk(12) As Integer, hpplama(13) As Double, hpp(14) As Double, 
        'rekpersediaan(15) As String, reklawan(16) As String, idspdetail(17) As Integer, cabang(18) As String, lokasi(19) As String, 
        'gudang(20) As String, costcenter(21) As String, divisi(22) As String, subdivisi(23) As String, proyek(24) As String, 
        'catatan(25) As String, urutan(26) As Integer, isclose(27) As Integer, customtext1(28) As String, customtext2(29) As String, 
        'customtext3(30) As String, customdbl1(31) As Double, customdbl2(32) As Double, customdbl3(33) As Double, customdate1(34) As Date, 
        'customdate2(35) As Date, customdate3(36) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idsadetail, idsa, idbarang, namabarang, tipebarang, jmlmasuk, jmlkeluar, 
        'satuan, nilaisatuan, jmlbarangmasuk, jmlbarangkeluar, satuanbarang, idhppkhususmasuk, hpplama, 
        'hpp, rekpersediaan, reklawan, idspdetail, cabang, lokasi, gudang, 
        'costcenter, divisi, subdivisi, proyek, catatan, urutan, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idsadetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idsa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlmasuk", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlkeluar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarangmasuk", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmlbarangkeluar", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idhppkhususmasuk", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "hpplama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hpp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekpersediaan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "reklawan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idspdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudang", AsEnumTypeData.AsString)
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

        'Variable ValidasiBatchSerial
        Dim ftBarangIn As String = "", ftBarangOut As String = ""

        'Variabel Hpp
        Dim ftBarang As String = ""

        'Variabel ValidasiSimpan
        Dim ftExistOutstanding As String = "", ftOutstanding As String = "", ftExistStok As String = "", ftStokAvailable As String = ""
        Dim updNilai As String = "", updFilter As String = "", gudang As String = ""
        Dim ftStokAvailableCase As String = ""
        'Dim ftExistOutstandingSO As String = "", ftOutstandingSO As String = ""
        'Dim updNilaiSO As String = "", updFilterSO As String = ""
        Dim idbarang As Integer = 0, idspdetail As Integer = 0, jmlbarangMasuk As Double = 0, jmlbarangKeluar As Double = 0, jmlbarang As Double = 0
        Dim isPlus As Boolean = False, jenismutasi As Double = 0 ', nobatch As String
        'Dim idsodetail As Double = 0

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 37) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idsadetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idsadetail required numeric." : GoTo selesai
            End If
            'idsa(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idsa required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'jmlmasuk(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - jmlmasuk required numeric." : GoTo selesai
            End If
            'jmlkeluar(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - jmlkeluar required numeric." : GoTo selesai
            End If
            'nilaisatuan(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarangmasuk(9) As Double
            'jmlbarangmasuk = jmlmasuk * nilaisatuan
            dataRowDetail(9) = Double.Parse(dataRowDetail(5)) * Double.Parse(dataRowDetail(8))
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - jmlbarangmasuk required numeric." : GoTo selesai
            End If
            'jmlbarangkeluar(10) As Double
            'jmlbarangkeluar = jmlkeluar * nilaisatuan
            dataRowDetail(10) = Double.Parse(dataRowDetail(6)) * Double.Parse(dataRowDetail(8))
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - jmlbarangkeluar required numeric." : GoTo selesai
            End If
            'idhppkhususmasuk(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - idhppkhususmasuk required numeric." : GoTo selesai
            End If
            'hpplama(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - hpplama required numeric." : GoTo selesai
            End If
            'hpp(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - hpp required numeric." : GoTo selesai
            End If
            'idspdetail(17) As Integer
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - idspdetail required numeric." : GoTo selesai
            End If
            'urutan(26) As Integer
            If (IsNumeric(dataRowDetail(26)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'isclose(27) As Integer
            If (IsNumeric(dataRowDetail(27)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(31) As Double
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(32) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(33) As Double
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(34) As Date
            If (IsDate(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(35) As Date
            If (IsDate(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(36) As Date
            If (IsDate(dataRowDetail(36)) = False) Then
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

            'jmlmasuk(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - jmlmasuk can't be empty" : GoTo selesai
            End If
            If dataRowDetail(5) < 0 Then
                result(2) = "Row : " & i & " - jmlmasuk can't be less than zero" : GoTo selesai
            End If

            'jmlkeluar(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - jmlkeluar can't be empty" : GoTo selesai
            End If
            If dataRowDetail(6) < 0 Then
                result(2) = "Row : " & i & " - jmlkeluar can't be less than zero" : GoTo selesai
            End If

            'jmlmasuk dan jmlkeluar tidak boleh keduanya diisi, harus salah satu
            If Double.Parse(dataRowDetail(5)) <> 0 And Double.Parse(dataRowDetail(6)) <> 0 Then
                result(2) = "Row : " & i & " - jmlmasuk and jmlkeluar can't be filled in both." : GoTo selesai
            ElseIf Double.Parse(dataRowDetail(5)) = 0 And Double.Parse(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - jmlmasuk and jmlkeluar can't be zero." : GoTo selesai
            End If

            'satuan(7) As String
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(7)) > 25 Then
                result(2) = "Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'nilaisatuan(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmlbarangmasuk(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarangmasuk can't be empty" : GoTo selesai
            End If
            If dataRowDetail(9) < 0 Then
                result(2) = "Row : " & i & " - jmlbarangmasuk can't be less than zero" : GoTo selesai
            End If

            'jmlbarangkeluar(10) As Double
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarangkeluar can't be empty" : GoTo selesai
            End If
            If dataRowDetail(10) < 0 Then
                result(2) = "Row : " & i & " - jmlbarangkeluar can't be less than zero" : GoTo selesai
            End If

            'jmlbarangmasuk dan jmlbarangkeluar tidak boleh keduanya diisi, harus salah satu
            If Double.Parse(dataRowDetail(9)) <> 0 And Double.Parse(dataRowDetail(10)) <> 0 Then
                result(2) = "Row : " & i & " - jmlbarangmasuk and jmlbarangkeluar can't be filled in both." : GoTo selesai
            ElseIf Double.Parse(dataRowDetail(9)) = 0 And Double.Parse(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarangmasuk and jmlbarangkeluar can't be zero." : GoTo selesai
            End If

            'satuanbarang(11) As String
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(11)) > 25 Then
                result(2) = "Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
            End If

            'hpplama(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - hpplama can't be empty" : GoTo selesai
            End If

            'hpp(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - hpp can't be empty" : GoTo selesai
            End If
            'If Double.Parse(dataRowDetail(9)) <> 0 And dataRowDetail(14) <= 0 Then
            '    result(2) = "Row : " & i & " - hpp can't be less than or equal to zero" : GoTo selesai
            'End If

            'rekpersediaan(15) As String
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - rekpersediaan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(15)) > 25 Then
                result(2) = "Row : " & i & " - rekpersediaan should not be more than 25 character." : GoTo selesai
            End If

            'reklawan(16) As String
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - reklawan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(16)) > 25 Then
                result(2) = "Row : " & i & " - reklawan should not be more than 25 character." : GoTo selesai
            End If

            'gudang(20) As String
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - gudang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(20)) > 25 Then
                result(2) = "Row : " & i & " - gudang should not be more than 25 character." : GoTo selesai
            End If

            'customdbl1(31) As Double
            If Len(dataRowDetail(31)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(32) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(33) As Double
            If Len(dataRowDetail(33)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(34) As Date
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(35) As Date
            If Len(dataRowDetail(35)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(36) As Date
            If Len(dataRowDetail(36)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idsadetail~idsa~idbarang~namabarang~tipebarang~jmlmasuk~jmlkeluar~satuan~nilaisatuan~jmlbarangmasuk~jmlbarangkeluar~satuanbarang~idhppkhususmasuk~hpplama~hpp~rekpersediaan~reklawan~idspdetail~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'If (dataRowDetail(9) > 0) Then
            '    If (i = 1) Then
            '        dataSplit(2) = ""
            '    End If
            'End If
            ''Cek APAKAH NO BATCH SUDAH TERISI ATAU BELUM
            'cekBatch = False
            'If (Len(dataRowDetail(28)) > 0) Then
            '    cekBatch = True
            'End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarangmasuk(9) As Double                     , jmlbarangkeluar(10) As Double                     , gudang(20) As String       , idspdetail(17) As Integer
            idbarang = dataRowDetail(2) : jmlbarangMasuk = Double.Parse(dataRowDetail(9)) : jmlbarangKeluar = Double.Parse(dataRowDetail(10)) : gudang = dataRowDetail(20) : idspdetail = dataRowDetail(17)
            'customdbl2(32) As Double
            'idsodetail = dataRowDetail(32)

            'ValidasiHpp
            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")

            'ValidasiBatchSerial
            If jmlbarangMasuk > 0 Then
                'JIKA BARANG MASUK MAKA FILTER BATCH DAN SERIAL MASUK
                ftBarangIn = IIf(Len(ftBarangIn.ToString) = 0, "", ftBarangIn & " OR ")
                ftBarangIn = String.Concat(ftBarangIn, "(bid = '" & idbarang & "')")

            ElseIf jmlbarangKeluar > 0 Then
                'JIKA BARANG KELUAR MAKA FILTER BATCH DAN SERIAL KELUAR
                ftBarangOut = IIf(Len(ftBarangOut.ToString) = 0, "", ftBarangOut & " OR ")
                ftBarangOut = String.Concat(ftBarangOut, "(bid = '" & idbarang & "')")
            End If

            'VALIDASI OUTSTANDING -------------------------
            If idspdetail <> 0 Then
                '1. CEK DATA EXIST
                ftExistOutstanding = IIf(Len(ftExistOutstanding.ToString) = 0, "", ftExistOutstanding & " UNION ")
                ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m3_sp_detail JOIN m3_sp ON idsp = spid WHERE idspdetail = '" & idspdetail & "' AND (spstatus = 2 OR spstatus = 3 OR spstatus = 4 OR spstatus = 7) LIMIT 1) as rowExists, '" & idspdetail & "' as idspdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
                'ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m3_sp_detail JOIN m3_sp ON idsp = spid WHERE idspdetail = '" & idspdetail & "' AND (spstatus = 2 OR spstatus = 3) LIMIT 1) as rowExists, '" & idspdetail & "' as idspdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING
                Dim OutstandingMasuk As Double = AsDataTableDSum(dtdetail, "jmlbarangmasuk", "idspdetail=" & idspdetail)
                Dim OutstandingKeluar As Double = AsDataTableDSum(dtdetail, "jmlbarangkeluar", "idspdetail=" & idspdetail)
                Dim Outstanding As Double = Math.Abs(OutstandingMasuk - OutstandingKeluar)
                ftOutstanding = IIf(Len(ftOutstanding.ToString) = 0, "", ftOutstanding & " OR ")
                ftOutstanding = String.Concat(ftOutstanding, " (spd.idspdetail = " & idspdetail & " AND " & Outstanding & " > (ABS(spd.selisihbarang) - spd.jmlsa)) ")

                '3. SET NILAI UPDATE OUTSTANDING
                updNilai = String.Concat("WHEN '" & idspdetail & "' THEN ROUND(jmlsa + '" & Outstanding & "', 5) ", updNilai)

                '4. SET FILTER UPDATE OUTSTANDING
                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                updFilter = String.Concat(updFilter, "(idspdetail = '" & idspdetail & "')")
            End If

            'If idsodetail <> 0 Then
            '    '1. CEK DATA EXIST
            '    ftExistOutstandingSO = IIf(Len(ftExistOutstandingSO.ToString) = 0, "", ftExistOutstandingSO & " UNION ")
            '    ftExistOutstandingSO = String.Concat(ftExistOutstandingSO, "SELECT EXISTS(SELECT 1 FROM m5_so_detail JOIN m5_so ON idso = soid WHERE idsodetail = '" & idsodetail & "' AND (sostatus = 2 OR sostatus = 3 OR sostatus = 4 OR sostatus = 7) LIMIT 1) as rowExists, '" & idsodetail & "' as idsodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

            '    '2. CEK JML OUTSTANDING
            '    Dim OutstandingMasuk As Double = AsDataTableDSum(dtdetail, "jmlbarangmasuk", "customdbl2=" & idsodetail)
            '    Dim OutstandingKeluar As Double = AsDataTableDSum(dtdetail, "jmlbarangkeluar", "customdbl2=" & idsodetail)
            '    Dim Outstanding As Double = Math.Abs(OutstandingKeluar - OutstandingMasuk)
            '    ftOutstandingSO = IIf(Len(ftOutstandingSO.ToString) = 0, "", ftOutstandingSO & " OR ")
            '    ftOutstandingSO = String.Concat(ftOutstandingSO, " (sod.idsodetail = " & idsodetail & " AND " & Outstanding & " > ((sod.jmlbarang) - sod.customdbl2)) ")

            '    '3. SET NILAI UPDATE OUTSTANDING
            '    updNilaiSO = String.Concat("WHEN '" & idsodetail & "' THEN ROUND(customdbl2 + '" & Outstanding & "' ", updNilaiSO)

            '    '4. SET FILTER UPDATE OUTSTANDING
            '    updFilterSO = IIf(Len(updFilterSO.ToString) = 0, "", updFilterSO & " OR ")
            '    updFilterSO = String.Concat(updFilterSO, "(idsodetail = '" & idsodetail & "')")
            'End If

            'VALIDASI STOK -------------------------------
            '1. CEK TRANSAKSI STOK MASUK/KELUAR
            Dim StokMasuk As Double = AsDataTableDSum(dtdetail, "jmlbarangmasuk", "idbarang=" & idbarang & " AND gudang='" & gudang & "'")
            Dim StokKeluar As Double = AsDataTableDSum(dtdetail, "jmlbarangkeluar", "idbarang=" & idbarang & " AND gudang='" & gudang & "'")
            Dim Stok As Double = StokMasuk - StokKeluar

            If Stok > -1 Then isPlus = True Else isPlus = False
            Stok = Math.Abs(Stok)

            '   'JIKA STOK KELUAR
            If isPlus = False Then
                '2. CEK DATA EXIST STOK KELUAR
                ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                'ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudang & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudang & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")
                ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudang & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudang & "' as gudang, 0 as stoktersedia, '" & Stok & "' as stokjual FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                '3. CEK JML STOK KELUAR
                ftStokAvailable = IIf(Len(ftStokAvailable.ToString) = 0, "", ftStokAvailable & " OR ")
                ftStokAvailable = String.Concat(ftStokAvailable, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudang & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")

                ftStokAvailableCase = String.Concat("WHEN isw.idbarang = " & idbarang & " AND isw.kgudang = '" & gudang & "' THEN " & Stok & " ", ftStokAvailableCase)

            End If
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

            ''nbtid, nbtjenismutasi, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
            ''nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
            ''nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, nbtgudang, nbtidbatchin
            'dataSplit(2) += "▲0▼1▼" & idbarang & "▼" & dataRowDetail(28) & "▼SA▼0▼" & dataRowDetail(7) & "▼" & jmlbarangMasuk & "▼▼▼▼" & dataRowDetail(31) & "▼" & dataRowDetail(32) & "▼" & dataRowDetail(33) & "▼" & dataRowDetail(34) & "▼" & dataRowDetail(35) & "▼" & dataRowDetail(36) & "▼" & dataRowDetail(20) & "▼0"

        Next

        'dataSplit(2) = dataSplit(2).Substring(1)


        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


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
                    'result(2) = JmlDtBatch : GoTo selesai
                End If

                ''GROUPING JIKA BARANG DAN KODE SAMA
                'Dim jmlb As Integer = dataDetail.Length
                'Dim a As Integer = 0
                'For a = 1 To jmlb
                '    dataRowDetail = dataDetail(a - 1).Split(sptField)
                '    If (dataRowBatch(2) = dataRowDetail(3)) Then
                '        If (dataRowBatch(3) = dataRowDetail(28)) Then
                '            result(2) = "a" : GoTo selesai
                '        End If
                '    End If
                'Next

                'END OF VALIDASI DAN SET DATA ROW BATCH ----------------------------

                'VALIDASI TIPE DATA BATCH ------------------------------------------
                'nbtid(0) As Integer
                If (IsNumeric(dataRowBatch(0)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtid required numeric." : GoTo selesai
                End If
                'nbtjenismutasi(1) As Integer
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
                'nbtjenismutasi(1) As Integer
                jenismutasi = dataRowBatch(1)
                'nbtidbarang(2) As Integer , nbtkode(3) As String      , nbtjml(7) As Double         , nbtgudang(17) As String      , nbtidbatchin(18) As Integer
                idbarang = dataRowBatch(2) : nbtkode = dataRowBatch(3) : jmlbarang = dataRowBatch(7) : nbtgudang = dataRowBatch(17) : nbtidbatchin = dataRowBatch(18)

                'VALIDASI HANYA UNTUK BARANG KELUAR SAJA
                If jenismutasi = 0 Then
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
                End If

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
                'nstjenismutasi(1) As Integer
                jenismutasi = dataRowSerial(1)
                'nstidbarang(2) As Integer  , nstkode(3) As String       , nstjml(7) As Double          , nstgudang(17) As String       , nstidserialin(18) As Integer
                idbarang = dataRowSerial(2) : nstkode = dataRowSerial(3) : jmlbarang = dataRowSerial(7) : nstgudang = dataRowSerial(17) : nstidserialin = dataRowSerial(18)


                'VALIDASI HANYA UNTUK BARANG KELUAR SAJA
                If jenismutasi = 0 Then
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
                End If
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("satgl")), AsFormatTanggal(drutama("satgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================

                'VALIDASI SIMPAN ========================================
                If drutama("sastatus") = 2 Then

                    Dim rsValidasi As String

                    'VALIDASI BATCH SERIAL IN ---------------
                    'ValidasiBatchSerial
                    If Len(ftBarangIn) > 0 Then
                        rsValidasi = ValidasiBatchSerial(dtdetail, dtbatch, dtserial, ftBarangIn, "jmlbarangmasuk", 1)
                        If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    End If
                    'END OF VALIDASI BATCH SERIAL IN --------

                    'VALIDASI BATCH SERIAL OUT ---------------
                    'ValidasiBatchSerial
                    If Len(ftBarangOut) > 0 Then
                        rsValidasi = ValidasiBatchSerial(dtdetail, dtbatch, dtserial, ftBarangOut, "jmlbarangkeluar", 0)
                        If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    End If
                    'END OF VALIDASI BATCH SERIAL OUT --------

                    If Len(ftBarangOut) > 0 Then
                        'ValidasiHppI
                        rsValidasi = ValidasiHppI(dtdetail, ftBarangOut)
                        If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                        ''ValidasiHppF
                        'rsValidasi = ValidasiHppF(dtdetail, ftBarangOut)
                        'If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                    End If

                    'ValidasiSimpan
                    'rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstanding, ftOutstanding, ftExistOutstandingSO, ftOutstandingSO, ftExistStok, "", ftStokAvailable, "", "", ftExistBatch, ftBatch, ftExistSerial, ftSerial, "gudang")
                    rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstanding, ftOutstanding, ftExistStok, "", ftStokAvailable, "", "", ftExistBatch, ftBatch, ftExistSerial, ftSerial, "gudang", ftStokAvailableCase)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================

                If isUpdate Then
                    result(4) = drutama("said")
                    notransaksi = drutama("sanotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(said), sanotransaksi FROM M3_sa WHERE said='" & result(4) & "' AND sastatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(said) FROM m3_sa WHERE sanotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m3_sa_history
                        Dim rsSimpanHistory As String = SimpanHistory.M3_Sa_HistorySimpan("" & paramSplit(0) & "★M3_Sa_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("sasumber")) & "▼" & FixQuotes(drutama("said")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M3_Sa set sacabang  = '" & FixQuotes(drutama("sacabang")) & "', salokasi  = '" & FixQuotes(drutama("salokasi")) & "', sagudang  = '" & FixQuotes(drutama("sagudang")) & "', sasumber  = '" & FixQuotes(drutama("sasumber")) & "', sajenis  = '" & FixQuotes(drutama("sajenis")) & "', saautonotransaksi  = " & drutama("saautonotransaksi") & ", sanotransaksi  = '" & notransaksi & "', satgl  = '" & FixQuotes(AsFormatTanggal(drutama("satgl"))) & "', sakodepa  = " & drutama("sakodepa") & ", sabagiansa  = " & drutama("sabagiansa") & ", sabagiansakontak  = '" & FixQuotes(drutama("sabagiansakontak")) & "', sauraian  = '" & FixQuotes(drutama("sauraian")) & "', sacatatan  = '" & FixQuotes(drutama("sacatatan")) & "', sanoref  = '" & FixQuotes(drutama("sanoref")) & "', satglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("satglnoref"))) & "', saidsp  = " & drutama("saidsp") & ", sastatus  = " & drutama("sastatus") & ", sastatussebelumnya  = " & drutama("sastatussebelumnya") & ", sajmlrevisi  = sajmlrevisi+1, sacetakanke  = " & drutama("sacetakanke") & ", samodifikasiuser  = " & drutama("samodifikasiuser") & ", samodifikasitgl  = NOW(), saposting  = 0, satutupperiode  = " & drutama("satutupperiode") & ", sacustomtext1  = '" & FixQuotes(drutama("sacustomtext1")) & "', sacustomtext2  = '" & FixQuotes(drutama("sacustomtext2")) & "', sacustomtext3  = '" & FixQuotes(drutama("sacustomtext3")) & "', sacustomtext4  = '" & FixQuotes(drutama("sacustomtext4")) & "', sacustomtext5  = '" & FixQuotes(drutama("sacustomtext5")) & "', sacustomint1  = " & drutama("sacustomint1") & ", sacustomint2  = " & drutama("sacustomint2") & ", sacustomint3  = " & drutama("sacustomint3") & ", sacustomdbl1  = '" & FixDouble(drutama("sacustomdbl1")) & "', sacustomdbl2  = '" & FixDouble(drutama("sacustomdbl2")) & "', sacustomdbl3  = '" & FixDouble(drutama("sacustomdbl3")) & "', sacustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("sacustomdate1"))) & "', sacustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("sacustomdate2"))) & "', sacustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("sacustomdate3"))) & "' where said = '" & drutama("said") & "'"
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

                    If drutama("saautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("sacabang"), drutama("salokasi"), drutama("sasumber"), drutama("satgl"))
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
                        notransaksi = drutama("sanotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(said) FROM m3_sa WHERE sanotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M3_Sa (sacabang, salokasi, sagudang, sasumber, sajenis, saautonotransaksi, sanotransaksi, satgl, sakodepa, sabagiansa, sabagiansakontak, sauraian, sacatatan, sanoref, satglnoref, saidsp, sastatus, sastatussebelumnya, sajmlrevisi, sacetakanke, sainputuser, sainputtgl, samodifikasiuser, samodifikasitgl, saposting, satutupperiode, saisclose, sacustomtext1, sacustomtext2, sacustomtext3, sacustomtext4, sacustomtext5, sacustomint1, sacustomint2, sacustomint3, sacustomdbl1, sacustomdbl2, sacustomdbl3, sacustomdate1, sacustomdate2, sacustomdate3) values('" & FixQuotes(drutama("sacabang")) & "', '" & FixQuotes(drutama("salokasi")) & "', '" & FixQuotes(drutama("sagudang")) & "', '" & FixQuotes(drutama("sasumber")) & "', '" & FixQuotes(drutama("sajenis")) & "', " & drutama("saautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("satgl"))) & "', " & drutama("sakodepa") & ", " & drutama("sabagiansa") & ", '" & FixQuotes(drutama("sabagiansakontak")) & "', '" & FixQuotes(drutama("sauraian")) & "', '" & FixQuotes(drutama("sacatatan")) & "', '" & FixQuotes(drutama("sanoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("satglnoref"))) & "', " & drutama("saidsp") & ", " & drutama("sastatus") & ", " & drutama("sastatussebelumnya") & ", " & drutama("sajmlrevisi") & ", " & drutama("sacetakanke") & ", " & drutama("sainputuser") & ", NOW(), " & drutama("samodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("satutupperiode") & ", " & drutama("saisclose") & ", '" & FixQuotes(drutama("sacustomtext1")) & "', '" & FixQuotes(drutama("sacustomtext2")) & "', '" & FixQuotes(drutama("sacustomtext3")) & "', '" & FixQuotes(drutama("sacustomtext4")) & "', '" & FixQuotes(drutama("sacustomtext5")) & "', " & drutama("sacustomint1") & ", " & drutama("sacustomint2") & ", " & drutama("sacustomint3") & ", '" & FixDouble(drutama("sacustomdbl1")) & "', '" & FixDouble(drutama("sacustomdbl2")) & "', '" & FixDouble(drutama("sacustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sacustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sacustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sacustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select said from M3_sa where sanotransaksi='" & notransaksi & "' AND sainputuser= '" & userid & "' order by samodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M3_Sa_Detail where idsa = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idsadetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jmlmasuk")) & "', '" & FixDouble(dr1("jmlkeluar")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarangmasuk")) & "', '" & FixDouble(dr1("jmlbarangkeluar")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', " & dr1("idhppkhususmasuk") & ", '" & FixDouble(dr1("hpplama")) & "', '" & FixDouble(dr1("hpp")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("reklawan")) & "', " & dr1("idspdetail") & ", '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M3_Sa_Detail(idsadetail, idsa, idbarang, namabarang, tipebarang, jmlmasuk, jmlkeluar, satuan, nilaisatuan, jmlbarangmasuk, jmlbarangkeluar, satuanbarang, idhppkhususmasuk, hpplama, hpp, rekpersediaan, reklawan, idspdetail, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                    sql = "Delete from M1_No_Batch_Transaction where nbtidtransaksi  = '" & result(4) & "' AND nbtsumber = 'SA'"
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
                    sql = "Delete from M1_No_Serial_Transaction  where nstidtransaksi  = '" & result(4) & "' AND nstsumber = 'SA'"
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


                If drutama("sastatus") = 2 Then
                    If Len(updNilai) > 0 Then
                        'UPDATE OUTSTANDING TRANSAKSI ===================================================
                        'UPDATE DETAIL
                        sql = "UPDATE m3_sp_detail SET jmlsa = (CASE idspdetail " & updNilai & " ELSE jmlsa END) WHERE " & updFilter
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE UTAMA
                        Dim updUtama As String = "", statusOut As Integer = 0
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idsp, SUM(ABS(selisihbarang)) as selisihbarang, SUM(jmlsa) as jmlsa FROM m3_sp_detail WHERE " & updFilter & " GROUP BY idsp")
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilai = "" : updFilter = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlsa") >= dr1("selisihbarang") Then
                                    statusOut = 2
                                ElseIf dr1("jmlsa") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilai = String.Concat(updNilai, "WHEN '" & dr1("idsp") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                                updFilter = String.Concat(updFilter, "(spid = '" & dr1("idsp") & "')")
                            Next

                            sql = "UPDATE m3_sp SET spstatussa = (CASE spid " & updNilai & " ELSE spstatussa END) WHERE " & updFilter
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                        'END OF UPDATE OUTSTANDING TRANSAKSI ============================================
                    End If


                    'If Len(updNilaiSO) > 0 Then
                    '    'UPDATE OUTSTANDING TRANSAKSI SO =======================================================
                    '    'UPDATE DETAIL
                    '    sql = "UPDATE m5_so_detail SET customdbl2 = (CASE idsodetail " & updNilaiSO & " ELSE customdbl2 END) WHERE " & updFilterSO
                    '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    '    With objCmd
                    '        .Connection = Con1
                    '        .Transaction = Trans
                    '        .CommandType = CommandType.Text
                    '        .CommandText = sql
                    '    End With
                    '    objCmd.ExecuteNonQuery()

                    '    'UPDATE UTAMA
                    '    Dim ftDetail As String = "", statusOut As Integer = 0
                    '    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idso FROM m5_so_detail WHERE " & updFilterSO & " GROUP BY idso")
                    '    If dtOut.Rows.Count > 0 Then
                    '        For Each dr1 As DataRow In dtOut.Rows
                    '            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                    '            ftDetail = String.Concat(ftDetail, "(idso = '" & dr1("idso") & "')")
                    '        Next
                    '    End If
                    '    dtOut = AsDataTableAmbilDariDB("SELECT idso, SUM(jmlbarang) as jmlbarang, SUM(customdbl2) as jmlrealisasi FROM m5_so_detail WHERE " & ftDetail & " GROUP BY idso")
                    '    If dtOut.Rows.Count > 0 Then
                    '        'KOSONGKAN VARIABEL NILAI DAN FILTER
                    '        updNilaiSO = "" : updFilterSO = ""
                    '        For Each dr1 As DataRow In dtOut.Rows
                    '            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                    '            If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                    '                statusOut = 2
                    '            ElseIf dr1("jmlrealisasi") < 1 Then
                    '                statusOut = 0
                    '            Else
                    '                statusOut = 1
                    '            End If
                    '            '2. SET NILAI UPDATE OUTSTANDING
                    '            updNilaiSO = String.Concat(updNilaiSO, "WHEN '" & dr1("idso") & "' THEN '" & statusOut & "' ")
                    '            '3. SET FILTERUPDATE OUTSTANDING
                    '            updFilterSO = IIf(Len(updFilterSO.ToString) = 0, "", updFilterSO & " OR ")
                    '            updFilterSO = String.Concat(updFilterSO, "(soid = '" & dr1("idso") & "')")
                    '        Next

                    '        sql = "UPDATE m5_so SET socustomint3 = (CASE soid " & updNilaiSO & " ELSE socustomint3 END) WHERE " & updFilterSO
                    '        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    '        With objCmd
                    '            .Connection = Con1
                    '            .Transaction = Trans
                    '            .CommandType = CommandType.Text
                    '            .CommandText = sql
                    '        End With
                    '        objCmd.ExecuteNonQuery()
                    '    End If
                    '    'END OF UPDATE OUTSTANDING TRANSAKSI SO ================================================
                    'End If


                    'INSERT NO BATCH IN =================================================================
                    Dim dtBatchIn = AsDataTableFilterSortDt(dtbatch, "nbtjenismutasi = '1'")
                    If dtBatchIn.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder
                        For Each dr1 As DataRow In dtBatchIn.Rows
                            'QUERY INSERT NO BATCH IN
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping        nbiidbatchin,                     nbigudang,                  nbiidbarang,                           nbikode,                             nbisumber,            nbiidtransaksi,                     nbisatuan,                 nbijmlmasuk,       nbijmlkeluar,                  nbijmlsisa, nbiisclose,                     nbicustomtext1,                             nbicustomtext2,                             nbicustomtext3,                             nbicustomdbl1,                             nbicustomdbl2,                             nbicustomdbl3,                                             nbicustomdate1,                                              nbicustomdate2,                                              nbicustomdate3
                            strValue2.Append("(" & 0 & ", '" & FixQuotes(dr1("nbtgudang")) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixQuotes(dr1("nbtcustomtext1")) & "', '" & FixQuotes(dr1("nbtcustomtext2")) & "', '" & FixQuotes(dr1("nbtcustomtext3")) & "', '" & FixDouble(dr1("nbtcustomdbl1")) & "', '" & FixDouble(dr1("nbtcustomdbl2")) & "', '" & FixDouble(dr1("nbtcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate3"))) & "')")
                        Next
                        sql = "Insert into M1_No_Batch_In(nbiidbatchin, nbigudang, nbiidbarang, nbikode, nbisumber, nbiidtransaksi, nbisatuan, nbijmlmasuk, nbijmlkeluar, nbijmlsisa, nbiisclose, nbicustomtext1, nbicustomtext2, nbicustomtext3, nbicustomdbl1, nbicustomdbl2, nbicustomdbl3, nbicustomdate1, nbicustomdate2, nbicustomdate3) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF INSERT NO BATCH IN =========================================================


                    'INSERT NO SERIAL IN ===============================================================
                    Dim dtSerialIn = AsDataTableFilterSortDt(dtserial, "nstjenismutasi = '1'")
                    If dtSerialIn.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder
                        For Each dr1 As DataRow In dtSerialIn.Rows
                            'QUERY INSERT NO SERIAL IN
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping       nsiidserialin,                     nsigudang,                  nsiidbarang,                           nsikode,                             nsisumber,            nsiidtransaksi,                     nsisatuan,                       nsijmlmasuk, nsijmlkeluar,                  nsijmlsisa, nsiisclose,                     nsicustomtext1,                             nsicustomtext2,                             nsicustomtext3,                             nsicustomdbl1,                             nsicustomdbl2,                             nsicustomdbl3,                                             nsicustomdate1,                                              nsicustomdate2,                                              nsicustomdate3
                            strValue2.Append("(" & 0 & ", '" & FixQuotes(dr1("nstgudang")) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixQuotes(dr1("nstcustomtext1")) & "', '" & FixQuotes(dr1("nstcustomtext2")) & "', '" & FixQuotes(dr1("nstcustomtext3")) & "', '" & FixDouble(dr1("nstcustomdbl1")) & "', '" & FixDouble(dr1("nstcustomdbl2")) & "', '" & FixDouble(dr1("nstcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate3"))) & "')")
                        Next
                        sql = "Insert into M1_No_Serial_In(nsiidserialin, nsigudang, nsiidbarang, nsikode, nsisumber, nsiidtransaksi, nsisatuan, nsijmlmasuk, nsijmlkeluar, nsijmlsisa, nsiisclose, nsicustomtext1, nsicustomtext2, nsicustomtext3, nsicustomdbl1, nsicustomdbl2, nsicustomdbl3, nsicustomdate1, nsicustomdate2, nsicustomdate3) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF INSERT NO SERIAL IN =====================================================


                    'INSERT NO BATCH OUT ============================================================
                    Dim dtBatchOut = AsDataTableFilterSortDt(dtbatch, "nbtjenismutasi = '0'")
                    If dtBatchOut.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder
                        For Each dr1 As DataRow In dtbatch.Rows
                            'QUERY INSERT NO BATCH OUT
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping             nboid,            nboidbatchin,                           nbogudang,                  nboidbarang,                           nbokode,                             nbosumber,            nboidtransaksi,                     nbosatuan,                         nbojmlkeluar,       nboisclose
                            strValue2.Append("(" & 0 & ", " & dr1("nbtidbatchin") & ", '" & FixQuotes(dr1("nbtgudang")) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', " & 0 & ")")
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
                    End If
                    'END OF INSERT NO BATCH OUT =====================================================


                    'INSERT NO SERIAL OUT ===========================================================
                    Dim dtSerialOut = AsDataTableFilterSortDt(dtserial, "nstjenismutasi = '0'")
                    If dtSerialOut.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder
                        For Each dr1 As DataRow In dtserial.Rows
                            'QUERY INSERT NO SERIAL OUT
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping            nsoid,             nsoidserialin,                           nsogudang,                  nsoidbarang,                           nsokode,                             nsosumber,            nsoidtransaksi,                     nsosatuan,                          nsojmlkeluar,      nsoisclose
                            strValue2.Append("(" & 0 & ", " & dr1("nstidserialin") & ", '" & FixQuotes(dr1("nstgudang")) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', " & 0 & ")")
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
                    End If
                    'END OF INSERT NO SERIAL OUT ====================================================


                    'AMBIL DATA DETAIL YANG BARU ++++++++++++++++++++++++++++++++++++++++++++++++++
                    Dim dtDetailNew As DataTable = AsDataTableAmbilDariDB("SELECT sad.idsadetail, sad.idbarang, sad.namabarang, sad.tipebarang, sad.jmlmasuk, sad.jmlkeluar, sad.satuan, sad.jmlbarangmasuk, sad.jmlbarangkeluar, sad.satuanbarang, sad.hpp, sad.idhppkhususmasuk, sad.gudang, sad.catatan, sad.costcenter, sad.divisi, sad.subdivisi, sad.customdbl1, sad.proyek, sa.sainputtgl, i.bhpp FROM m3_sa_detail sad JOIN m3_sa sa ON sad.idsa = sa.said JOIN m1_item i ON sad.idbarang = i.bid WHERE sad.idsa = '" & result(4) & "'")

                    Dim hpp As Double = 0, postinghpp As Double = 0, bstok As Double = 0
                    Dim saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0
                    Dim strTransaksiBarang As New StringBuilder, dtSaldo As New DataTable

                    If dtDetailNew.Rows.Count > 0 Then

                        'INSERT ITEM TRANSACTION ==================================================
                        Dim sqlStokGudang As String = "", jmltransaksi As Double = 0

                        'AMBIL MATAUANG FUNGSIONAL DARI SETTING
                        Dim dtMatauang As DataTable = AsDataTableAmbilDariDB("SELECT skode, snilai FROM m0_setting WHERE smodule = 0 AND sgrup = 'accounting' AND (skode = 'MataUangFungsional' OR skode = 'Kurs')")
                        Dim matauang As String = AsDataTableDLookup(dtMatauang, "snilai", "skode = 'MataUangFungsional'", "Not found")
                        If matauang = "Not found" Then
                            result(2) = "Setting Functional Currency not found." : Trans.Rollback() : GoTo selesai
                        End If
                        Dim kurs As String = AsDataTableDLookup(dtMatauang, "snilai", "skode = 'Kurs'", "Not found")
                        If kurs = "Not found" Then
                            result(2) = "Setting Exchange Rate Functional Currency not found." : Trans.Rollback() : GoTo selesai
                        End If

                        'PERULANGAN DATA DETAIL
                        For Each dr1 As DataRow In dtDetailNew.Rows
                            'SET NILAI VARIABEL
                            idbarang = Double.Parse(dr1("idbarang"))
                            gudang = dr1("gudang")

                            'AMBIL DATA STOK DAN HPPAVERAGE TERBARU
                            sql = "SELECT bstok FROM m1_item WHERE bid = '" & FixDouble(idbarang) & "'"
                            dtSaldo = AsDataTableAmbilDariDB(sql)
                            If dtSaldo.Rows.Count > 0 Then
                                'set nilai stok
                                bstok = Double.Parse(dtSaldo.Rows(0)("bstok"))

                                'BARANG MASUK ATAU KELUAR
                                If Double.Parse(dr1("jmlbarangmasuk")) > 0 Then
                                    jmlbarang = Double.Parse(dr1("jmlbarangmasuk"))
                                    jmltransaksi = Double.Parse(dr1("jmlmasuk"))

                                    'jenismutasi dan postinghpp 
                                    '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 0
                                    '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                    jenismutasi = 1 : postinghpp = 0

                                    'hitung saldojml = bstok + jmlbarang
                                    saldojml = bstok + jmlbarang

                                    'hitung hpp = 0, saldohpp = 0, saldonilai = 0
                                    hpp = 0 : saldohpp = 0 : saldonilai = 0

                                    'sql stok pergudang
                                    sqlStokGudang = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES ('" & idbarang & "','" & gudang & "','" & jmlbarang & "') ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"

                                Else
                                    jmlbarang = Double.Parse(dr1("jmlbarangkeluar"))
                                    jmltransaksi = Double.Parse(dr1("jmlkeluar"))

                                    'jenismutasi dan postinghpp 
                                    '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 0
                                    '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                    jenismutasi = 0 : postinghpp = 0

                                    'hitung saldojml = bstok - jmlbarang
                                    saldojml = bstok - jmlbarang

                                    'hitung hpp = 0, saldohpp = 0, saldonilai = 0
                                    hpp = 0 : saldohpp = 0 : saldonilai = 0

                                    'sql stok pergudang
                                    sqlStokGudang = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES ('" & idbarang & "','" & gudang & "','-" & jmlbarang & "') ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"

                                End If

                                'QUERY INSERT TRANSAKSI BARANG
                                strTransaksiBarang.Clear()
                                'mapping                        id,                             cabang,                                   lokasi,                             gudang,                        kodepa,           jenismutasi,                               sumber,              idutama,                 iddetail,                      notransaksi,                                                 tgl,                           kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                       jmlbarang,                           satuanbarang,                        matauang,                      kurs,                    harga,                 diskon,              jmldiskon,                        idhppikm,        idhppikk,                hpp,                                 uraian,                                   catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                             inputtgl,                                               inputuser,  postingtgl, updatehpp,     postinghpp,     hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("sacabang")) & "', '" & FixQuotes(drutama("salokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', " & drutama("sakodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("sasumber")) & "', " & result(4) & ", " & dr1("idsadetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("satgl"))) & "', " & drutama("sabagiansa") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(jmltransaksi) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(jmlbarang) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(matauang) & "', '" & FixDouble(kurs) & "', '" & FixDouble(hpp) & "', '" & FixQuotes(0) & "', '" & FixDouble(0) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("sauraian")) & "', '" & FixQuotes(drutama("sacatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr1("sainputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("sainputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = Con1
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                                'UPDATE STOK PERGUDANG
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = Con1
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sqlStokGudang
                                End With
                                objCmd.ExecuteNonQuery()

                                'UPDATE STOK GLOBAL
                                'sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "' WHERE bid = '" & idbarang & "'"
                                'TAMBAHKAN KONDISI JIKA CUSTOMDBL <> 0 MAKA UPDATE BHARGABELI
                                sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "', bhargabeli = (CASE " & FixDouble(dr1("customdbl1")) & " WHEN 0 THEN bhargabeli ELSE " & FixDouble(dr1("customdbl1")) & " END) WHERE bid = '" & idbarang & "'"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = Con1
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                            End If

                        Next
                        'END OF INSERT ITEM TRANSACTION ===========================================

                    Else
                        result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                    End If
                End If


                'INSERT MSMQ COGS =================================================================
                Dim sumber As String = "SA", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("sastatus") = 2 Then
                    Dim Security As New ClsSecurity, mjid As String = "", hasilMsmq As String = ""
                    'BUAT ID UNIQUE
                    mjid = Security.MD5CalcString(userid & sumber & result(4) & Now) 'RandomId.Generate(15)

                    'MSMQ TABEL
                    sql = "Insert into M0_Msmq_Cogs(mcid, mcsumber, mcidtransaksi, mcprogress, mcpesan, mctglantrian, mctglselesai, mcuserid) values ('" _
                        & mjid & "', '" & sumber & "', '" & result(4) & "', '" & 0 & "', " & "''" & ", NOW(), '1971-01-01 00:00:00', '" & userid & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'MSMQ ANTRIAN
                    Dim ProsesHpp As String = F_getSetting(0, "accounting", "ProsesHpp")
                    If ProsesHpp.Equals("0") = False Then
                        hasilMsmq = SendMsmq(dirMsmq, "C", mjid, sumber, result(4), userid)
                        If Len(hasilMsmq) > 0 Then
                            result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
                        End If
                    End If

                End If
                'END OF INSERT MSMQ COGS ==========================================================


                'INSERT USER LOG ==================================================================
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
    Public Function M3_SaUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("sabagiansakode", "c1.kkode")
            Filter = Filter.Replace("sabagiansanama", "c1.knama")
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
            Dim sumber As String = "SA", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Satgl, Sanotransaksi, Sastatus FROM m3_Sa WHERE Said='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Sastatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m3_sa_history
            Dim rsSimpanHistory As String = SimpanHistory.M3_Sa_HistorySimpan("" & paramSplit(0) & "★M3_Sa_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m3_sa_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================


                'CEK NO BATCH DAN SERIAL IN =====================================================
                'BATCH
                dtdetail = AsDataTableAmbilDariDB("SELECT bkode, nbikode FROM m1_no_batch_in JOIN m1_item ON nbiidbarang = bid WHERE nbisumber = 'SA' AND nbiidtransaksi = '" & idtransaksi & "' AND nbijmlkeluar > 0")
                If dtdetail.Rows.Count > 0 Then result(2) = "Item : " & dtdetail.Rows(0)("bkode") & " | No. Batch : " & dtdetail.Rows(0)("nbikode") & " has related transactions." : Trans.Rollback() : GoTo selesai

                'SERIAL
                dtdetail = AsDataTableAmbilDariDB("SELECT bkode, nsikode FROM m1_no_serial_in JOIN m1_item ON nsiidbarang = bid WHERE nsisumber = 'SA' AND nsiidtransaksi = '" & idtransaksi & "' AND nsijmlkeluar > 0")
                If dtdetail.Rows.Count > 0 Then result(2) = "Item : " & dtdetail.Rows(0)("bkode") & " | No. Serial : " & dtdetail.Rows(0)("nsikode") & " has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK NO BATCH DAN SERIAL IN ==============================================

                'Variabel ValidasiSimpan
                Dim ftHppI As String = "", ftHppF As String = ""
                Dim updNilaiHppI As String = "", updFilterHppI As String = "", delFilterHppI As String = ""
                Dim filterHppF As String = "", updNilaiHppF As String = "", updFilterHppF As String = "", delFilterHppF As String = ""
                Dim ftExistStok As String = "", ftStok As String = "", gudang As String = ""
                Dim updNilai As String = "", updFilter As String = "", updStokIn As String = "", updStokOut As String = ""
                Dim updStokBarangMasuk As String = "", ftStokBarangMasuk As String = ""
                Dim updStokBarangKeluar As String = "", ftStokBarangKeluar As String = ""
                'Dim updNilaiSO As String = "", updFilterSO As String = ""

                Dim idbarang As Integer = 0, idsadetail As Integer = 0, idspdetail As Integer = 0
                'Dim idsodetail As Integer = 0
                Dim jmlbarangMasuk As Double = 0, jmlbarangKeluar As Double = 0, idhppkhususmasuk As Integer = 0
                Dim isPlus As Boolean = False

                'AMBIL DATA DETAIL
                'dtdetail = AsDataTableAmbilDariDB("SELECT idsadetail, idbarang, jmlbarangmasuk, jmlbarangkeluar, idhppkhususmasuk, gudang, idspdetail  FROM m3_sa_detail WHERE idsa = '" & idtransaksi & "'")
                dtdetail = AsDataTableAmbilDariDB("SELECT idsadetail, idbarang, jmlbarangmasuk, jmlbarangkeluar, idhppkhususmasuk, gudang, idspdetail, tipebarang, namabarang, urutan, satuan, nilaisatuan, customdbl2  FROM m3_sa_detail WHERE idsa = '" & idtransaksi & "'")
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idsadetail = dr1("idsadetail") : idbarang = dr1("idbarang") : idhppkhususmasuk = dr1("idhppkhususmasuk")
                        jmlbarangMasuk = dr1("jmlbarangmasuk") : jmlbarangKeluar = dr1("jmlbarangkeluar") : gudang = dr1("gudang")
                        idspdetail = dr1("idspdetail")
                        'idsodetail = dr1("customdbl2")

                        'UPDATE OUTSTANDING ---------------------------
                        If idspdetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING ----------
                            Dim OutstandingMasuk As Double = AsDataTableDSum(dtdetail, "jmlbarangmasuk", "idspdetail=" & idspdetail)
                            Dim OutstandingKeluar As Double = AsDataTableDSum(dtdetail, "jmlbarangkeluar", "idspdetail=" & idspdetail)
                            Dim Outstanding As Double = Math.Abs(OutstandingMasuk - OutstandingKeluar)
                            updNilai = String.Concat("WHEN '" & idspdetail & "' THEN ROUND(jmlsa - '" & Outstanding & "', 5) ", updNilai)

                            '2. SET FILTERUPDATE OUTSTANDING ----------
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(idspdetail = '" & idspdetail & "')")
                        End If

                        'If idsodetail <> 0 Then
                        '    '1. SET NILAI UPDATE OUTSTANDING ----------
                        '    Dim OutstandingMasuk As Double = AsDataTableDSum(dtdetail, "jmlbarangmasuk", "customdbl2=" & idsodetail)
                        '    Dim OutstandingKeluar As Double = AsDataTableDSum(dtdetail, "jmlbarangkeluar", "customdbl2=" & idsodetail)
                        '    Dim Outstanding As Double = Math.Abs(OutstandingKeluar - OutstandingMasuk)
                        '    updNilaiSO = String.Concat("WHEN '" & idsodetail & "' THEN ROUND(customdbl2 - '" & Outstanding & "', 5) ", updNilaiSO)

                        '    '2. SET FILTERUPDATE OUTSTANDING ----------
                        '    updFilterSO = IIf(Len(updFilterSO.ToString) = 0, "", updFilterSO & " OR ")
                        '    updFilterSO = String.Concat(updFilterSO, "(idsodetail = '" & idsodetail & "')")
                        'End If

                        'VALIDASI STOK -------------------------------
                        'CEK TRANSAKSI STOK MASUK/KELUAR
                        Dim StokMasuk As Double = AsDataTableDSum(dtdetail, "jmlbarangmasuk", "idbarang=" & idbarang & " AND gudang='" & gudang & "'")
                        Dim StokKeluar As Double = AsDataTableDSum(dtdetail, "jmlbarangkeluar", "idbarang=" & idbarang & " AND gudang='" & gudang & "'")
                        Dim Stok As Double = StokMasuk - StokKeluar
                        Stok = Math.Abs(Stok)

                        If jmlbarangMasuk <> 0 Then isPlus = True Else isPlus = False

                        '   'JIKA TRANSAKSI STOK MASUK, MAKA STOK DIKELUARKAN
                        If isPlus = True Then

                            'CEK DATA EXIST STOK KELUAR
                            ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                            ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudang & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudang & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                            'CEK JML STOK KELUAR
                            ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
                            'ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudang & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")
                            ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudang & "' AND " & Stok & " > isw.stok) ")

                            'SET NILAI UPDATE STOK KELUAR
                            updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
                            updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudang & "', ('-" & jmlbarangMasuk & "'))") ' idbarang, kgudang, stok

                            'BUAT FILTER CEK HPP KHUSUS(I)
                            ftHppI = IIf(Len(ftHppI.ToString) = 0, "", ftHppI & " OR ")
                            ftHppI = String.Concat(ftHppI, "(idbarang = '" & idbarang & "' AND idtransaksi = '" & idsadetail & "' AND sumber = 'SA')")

                            'BUAT FILER CEK HPP FIFO(F)
                            ftHppF = IIf(Len(ftHppF.ToString) = 0, "", ftHppF & " OR ")
                            ftHppF = String.Concat(ftHppF, "(cfiidbarang = '" & idbarang & "' AND cfiidtransaksi = '" & idsadetail & "' AND cfisumber = 'SA')")

                            'SET NILAI UPDATE STOK KELUAR M1_ITEM
                            Dim jmlkeluar As Double = AsDataTableDSum(dtdetail, "jmlbarangmasuk", "idbarang=" & idbarang)
                            ftStokBarangKeluar = IIf(Len(ftStokBarangKeluar.ToString) = 0, "", ftStokBarangKeluar & " OR ")
                            ftStokBarangKeluar = String.Concat(ftStokBarangKeluar, " (bid = '" & idbarang & "') ")
                            updStokBarangKeluar = String.Concat("WHEN '" & idbarang & "' THEN ROUND(bstok - '" & jmlkeluar & "', 5) ", updStokBarangKeluar)

                        Else
                            'JIKA TRANSAKSI STOK KELUAR, MAKA STOK DIKEMBALIKAN

                            'SET NILAI UPDATE STOK MASUK 
                            updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
                            updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudang & "', '" & jmlbarangKeluar & "')") ' idbarang, kgudang, stok

                            'BUAT FILTER UPDATE HPP KHUSUS (I)
                            If idhppkhususmasuk <> 0 Then
                                'SET NILAI UPDATE HPP KHUSUS IN
                                Dim jmlKeluar As Double = AsDataTableDSum(dtdetail, "jmlbarangkeluar", "idhppkhususmasuk='" & idhppkhususmasuk & "'")
                                updNilaiHppI = String.Concat("WHEN '" & idhppkhususmasuk & "' THEN ROUND(jmlkeluar - '" & jmlKeluar & "', 5) ", updNilaiHppI)

                                'SET FILTER UPDATE HPP KHUSUS IN
                                updFilterHppI = IIf(Len(updFilterHppI.ToString) = 0, "", updFilterHppI & " OR ")
                                updFilterHppI = String.Concat(updFilterHppI, "(idhppikm = '" & idhppkhususmasuk & "')")

                                'SET FILTER DELETE HPP KHUSUS OUT
                                delFilterHppI = IIf(Len(delFilterHppI.ToString) = 0, "", delFilterHppI & " OR ")
                                delFilterHppI = String.Concat(delFilterHppI, "(sumber = 'SA' AND idtransaksi = '" & idsadetail & "')")
                            End If

                            'BUAT FILTER UPDATE HPP FIFO (F)
                            filterHppF = IIf(Len(filterHppF.ToString) = 0, "", filterHppF & " OR ")
                            filterHppF = String.Concat(filterHppF, "(cfosumber = 'SA' AND cfoidtransaksi = '" & idsadetail & "')")

                            'SET NILAI UPDATE STOK MASUK M1_ITEM
                            Dim jmlmasuk As Double = AsDataTableDSum(dtdetail, "jmlbarangkeluar", "idbarang=" & idbarang)
                            ftStokBarangMasuk = IIf(Len(ftStokBarangMasuk.ToString) = 0, "", ftStokBarangMasuk & " OR ")
                            ftStokBarangMasuk = String.Concat(ftStokBarangMasuk, " (bid = '" & idbarang & "') ")
                            updStokBarangMasuk = String.Concat("WHEN '" & idbarang & "' THEN ROUND(bstok + '" & jmlmasuk & "', 5) ", updStokBarangMasuk)

                        End If
                        'END OF BUAT FILTER UNTUK UPDATE --------------------------
                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If


                'VALIDASI HPP, STOK ----------------------------------
                'Dim rsValidasi As String = ValidasiSimpan(dtdetail, "", "", "", "", ftExistStok, ftStok, "", ftHppI, ftHppF, "", "", "", "", "")
                Dim rsValidasi As String = ValidasiSimpan(dtdetail, "", "", ftExistStok, ftStok, "", ftHppI, ftHppF, "", "", "", "", "", "")
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'END OF VALIDASI HPP, STOK ---------------------------


                'CEK HPP FIFO ====================================================================
                'AMBIL DATA DARI HPP FIFO KELUAR - m1_cogs_fifo_out
                Dim dtHppF As DataTable = AsDataTableAmbilDariDB("SELECT * FROM m1_cogs_fifo_out WHERE " & filterHppF)
                If dtHppF.Rows.Count > 0 Then
                    Dim idhppfifoin As Integer = 0
                    For Each dr1 As DataRow In dtHppF.Rows
                        'SET NILAI VARIABEL
                        idhppfifoin = dr1("cfoidcfi")

                        'SET FILTER DELETE HPP FIFO OUT
                        delFilterHppF = IIf(Len(delFilterHppF.ToString) = 0, "", delFilterHppF & " OR ")
                        delFilterHppF = String.Concat(delFilterHppF, "(cfosumber = 'SA' AND cfoidtransaksi = '" & dr1("cfoidtransaksi") & "')")

                        'SET NILAI UPDATE HPP FIFO IN
                        Dim jmlKeluar As Double = AsDataTableDSum(dtHppF, "cfojmlkeluar", "cfoidcfi='" & idhppfifoin & "'")
                        updNilaiHppF = String.Concat("WHEN '" & idhppfifoin & "' THEN ROUND(cfijmlkeluar - '" & jmlKeluar & "', 5) ", updNilaiHppF)

                        'SET FILTER UPDATE HPP FIFO IN
                        updFilterHppF = IIf(Len(updFilterHppF.ToString) = 0, "", updFilterHppF & " OR ")
                        updFilterHppF = String.Concat(updFilterHppF, "(cfiid = '" & idhppfifoin & "')")
                    Next
                End If
                'END OF CEK HPP FIFO =============================================================


                'UPDATE OUTSTANDING TRANSAKSI ====================================================
                If Len(updFilter) > 0 Then
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m3_sp_detail SET jmlsa = (CASE idspdetail " & updNilai & " ELSE jmlsa END) WHERE " & updFilter
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
                    Dim updUtama As String = "", statusOut As Integer = 0
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idsp, SUM(ABS(selisihbarang)) as selisihbarang, SUM(jmlsa) as jmlsa FROM m3_sp_detail WHERE " & updFilter & " GROUP BY idsp")
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilai = "" : updFilter = ""
                        For Each dr1 As DataRow In dtOut.Rows
                            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                            If dr1("jmlsa") >= dr1("selisihbarang") Then
                                statusOut = 2
                            ElseIf dr1("jmlsa") < 1 Then
                                statusOut = 0
                            Else
                                statusOut = 1
                            End If
                            '2. SET NILAI UPDATE OUTSTANDING
                            updNilai = String.Concat(updNilai, "WHEN '" & dr1("idsp") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(spid = '" & dr1("idsp") & "')")
                        Next

                        sql = "UPDATE m3_sp SET spstatussa = (CASE spid " & updNilai & " ELSE spstatussa END) WHERE " & updFilter
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

                'If Len(updFilterSO) > 0 Then
                '    'UPDATE OUTSTANDING DETAIL ----------------------
                '    sql = "UPDATE m5_so_detail SET customdbl2 = (CASE idsodetail " & updNilaiSO & " ELSE customdbl2 END) WHERE " & updFilterSO
                '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '    With objCmd
                '        .Connection = Con1
                '        .Transaction = Trans
                '        .CommandType = CommandType.Text
                '        .CommandText = sql
                '    End With
                '    objCmd.ExecuteNonQuery()
                '    'END OF UPDATE OUTSTANDING DETAIL ---------------

                '    'UPDATE OUTSTANDING UTAMA -----------------------
                '    Dim ftDetail As String = "", statusOut As Integer = 0
                '    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idso FROM m5_so_detail WHERE " & updFilterSO & " GROUP BY idso")
                '    If dtOut.Rows.Count > 0 Then
                '        For Each dr1 As DataRow In dtOut.Rows
                '            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                '            ftDetail = String.Concat(ftDetail, "(idso = '" & dr1("idso") & "')")
                '        Next
                '    End If
                '    dtOut = AsDataTableAmbilDariDB("SELECT idso, SUM(jmlbarang) as jmlbarang, SUM(customdbl2) as jmlrealisasi FROM m5_so_detail WHERE " & ftDetail & " GROUP BY idso")
                '    If dtOut.Rows.Count > 0 Then
                '        'KOSONGKAN VARIABEL NILAI DAN FILTER
                '        updNilaiSO = "" : updFilterSO = ""
                '        For Each dr1 As DataRow In dtOut.Rows
                '            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                '            If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                '                statusOut = 2
                '            ElseIf dr1("jmlrealisasi") < 1 Then
                '                statusOut = 0
                '            Else
                '                statusOut = 1
                '            End If
                '            '2. SET NILAI UPDATE OUTSTANDING
                '            updNilaiSO = String.Concat(updNilaiSO, "WHEN '" & dr1("idso") & "' THEN '" & statusOut & "' ")
                '            '3. SET FILTERUPDATE OUTSTANDING
                '            updFilterSO = IIf(Len(updFilterSO.ToString) = 0, "", updFilterSO & " OR ")
                '            updFilterSO = String.Concat(updFilterSO, "(soid = '" & dr1("idso") & "')")
                '        Next

                '        sql = "UPDATE m5_so SET socustomint3 = (CASE soid " & updNilaiSO & " ELSE socustomint3 END) WHERE " & updFilterSO
                '        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '        With objCmd
                '            .Connection = Con1
                '            .Transaction = Trans
                '            .CommandType = CommandType.Text
                '            .CommandText = sql
                '        End With
                '        objCmd.ExecuteNonQuery()
                '    End If
                '    'END OF UPDATE OUTSTANDING UTAMA ----------------
                'End If
                'END OF UPDATE OUTSTANDING TRANSAKSI =============================================


                'UPDATE HPP KHUSUS (I) =========================================================
                'DELETE HPP KHUSUS OUT
                If Len(delFilterHppI) > 0 Then
                    sql = "DELETE FROM m1_cogs_special_out WHERE " & delFilterHppI
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'UPDATE HPP KHUSUS IN
                If Len(updNilaiHppI) > 0 Then
                    sql = "UPDATE m1_cogs_special_in SET jmlkeluar = (CASE idhppikm " & updNilaiHppI & " ELSE jmlkeluar END) WHERE " & updFilterHppI
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE HPP KHUSUS (I) ==================================================


                'UPDATE HPP FIFO (F) ===========================================================
                'DELETE HPP FIFO OUT
                If Len(delFilterHppF) > 0 Then
                    sql = "DELETE FROM m1_cogs_fifo_out WHERE " & delFilterHppF
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'UPDATE HPP FIFO IN
                If Len(updNilaiHppF) > 0 Then
                    sql = "UPDATE m1_cogs_fifo_in SET cfijmlkeluar = (CASE cfiid " & updNilaiHppF & " ELSE cfijmlkeluar END) WHERE " & updFilterHppF
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE HPP FIFO (F) ====================================================


                'DELETE HPP KHUSUS MASUK (I)
                If Len(ftHppI) > 0 Then
                    sql = "DELETE FROM m1_cogs_special_in WHERE " & ftHppI
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                'DELETE HPP FIFO MASUK (F)
                If Len(ftHppF) > 0 Then
                    sql = "DELETE FROM m1_cogs_fifo_in WHERE " & ftHppF
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


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


                'UPDATE NO BATCH ================================================================
                Dim updNilaiBatch As String = "", updFilterBatch As String = ""
                Dim dtBatch As DataTable = AsDataTableAmbilDariDB("SELECT nboidbatchin, nbogudang, nboidbarang, nbokode, nbojmlkeluar FROM m1_no_batch_out WHERE nbosumber = '" & sumber & "' AND nboidtransaksi = '" & idtransaksi & "'")
                If dtBatch.Rows.Count > 0 Then
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


                'UPDATE STOK ===================================================================
                'STOK KELUAR
                If Len(updStokOut) > 0 Then
                    sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokOut & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'STOK KELUAR BARANG m1_item
                If Len(updStokBarangKeluar) > 0 Then
                    sql = "UPDATE m1_item SET bstok = (CASE bid " & updStokBarangKeluar & " ELSE bstok END) WHERE " & ftStokBarangKeluar
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'STOK MASUK
                If Len(updStokIn) > 0 Then
                    sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokIn & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'STOK MASUK BARANG m1_item
                If Len(updStokBarangMasuk) > 0 Then
                    sql = "UPDATE m1_item SET bstok = (CASE bid " & updStokBarangMasuk & " ELSE bstok END) WHERE " & ftStokBarangMasuk
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
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


                'UPDATE BHPPAVERAGE M1_ITEM ===================================================
                'sql = "  UPDATE m1_item i"
                'sql &= " JOIN m3_sa_detail sad ON i.bid = sad.idbarang AND sad.jmlbarangmasuk <> 0 AND sad.idsa = '" & FixDouble(idtransaksi) & "'"
                'sql &= " LEFT JOIN"
                'sql &= " (SELECT i.bid as idbarang, ROUND(SUM(it.jmlbarang * it.hpp) / SUM(it.jmlbarang),2) as hppaverage"
                'sql &= " FROM m1_item_transaction it"
                'sql &= " JOIN m1_item i ON it.idbarang = i.bid AND it.jenismutasi = 1"
                'sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1"
                'sql &= " JOIN m3_sa_detail sad ON it.idbarang = sad.idbarang AND sad.jmlbarangmasuk <> 0 AND sad.idsa = '" & FixDouble(idtransaksi) & "'"
                'sql &= " JOIN m3_sa sa ON sad.idsa = sa.said AND CONCAT(it.sumber,it.idutama) <> CONCAT(sa.sasumber,sa.said)"
                'sql &= " GROUP BY it.idbarang) as h ON i.bid = h.idbarang"
                'sql &= " SET i.bhppaverage = (CASE i.bjenis WHEN 'P' THEN (CASE i.bstok WHEN 0 THEN 0 ELSE IFNULL(h.hppaverage,0) END) ELSE IFNULL(h.hppaverage,0) END)"

                'SA MASUK
                sql = "  UPDATE m1_item i"
                sql &= " JOIN ("
                sql &= " SELECT sad.idbarang, ROUND(SUM(sad.jmlbarangmasuk * sad.hpp),2) as nilai, SUM(sad.jmlbarangmasuk) as jumlah"
                sql &= " FROM m3_sa_detail sad"
                sql &= " WHERE sad.jmlbarangmasuk <> 0 AND sad.idsa = '" & FixDouble(idtransaksi) & "'"
                sql &= " GROUP BY sad.idbarang"
                sql &= " ) as h ON i.bid = h.idbarang"
                sql &= " SET i.bhppaverage = (CASE i.bjenis WHEN 'P' THEN (CASE i.bstok WHEN 0 THEN 0 ELSE ROUND((((i.bstok + h.jumlah) * i.bhppaverage) - (h.nilai)) / (i.bstok),2) END) ELSE IFNULL(ROUND((((i.bstok + h.jumlah) * i.bhppaverage) - (h.nilai)) / (i.bstok),2),0) END)"
                'result(2) = sql : Trans.Rollback() : GoTo selesai
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'SA KELUAR
                sql = "  UPDATE m1_item i"
                sql &= " JOIN ("
                sql &= " SELECT sad.idbarang, ROUND(SUM(sad.jmlbarangkeluar * sad.hpp),2) as nilai, SUM(sad.jmlbarangkeluar) as jumlah"
                sql &= " FROM m3_sa_detail sad"
                sql &= " WHERE sad.jmlbarangkeluar <> 0 AND sad.idsa = '" & FixDouble(idtransaksi) & "'"
                sql &= " GROUP BY sad.idbarang"
                sql &= " ) as h ON i.bid = h.idbarang"
                sql &= " SET i.bhppaverage = (CASE i.bjenis WHEN 'P' THEN (CASE i.bstok WHEN 0 THEN 0 ELSE ROUND((((i.bstok - h.jumlah) * i.bhppaverage) + (h.nilai)) / (i.bstok),2) END) ELSE IFNULL(ROUND((((i.bstok - h.jumlah) * i.bhppaverage) + (h.nilai)) / (i.bstok),2),0) END)"
                'result(2) = sql : Trans.Rollback() : GoTo selesai
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF UPDATE BHPPAVERAGE M1_ITEM ============================================


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
            sql = "UPDATE M3_Sa SET Sastatus = " & nilaiStatus & ", Samodifikasiuser='" & userid & "', Samodifikasitgl = NOW(), Saposting = 0, Sapostingtgl = '1971-01-01 00:00:00', Sajmlrevisi = Sajmlrevisi + 1 WHERE Said = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M3_SaSearch(PostWsSearch(paramSplit(0), "M3_SaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
            result(2) = ex.Message & " === " & sql
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
    Public Function M3_SaDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("sabagiansakode", "c1.kkode")
            Filter = Filter.Replace("sabagiansanama", "c1.knama")
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
            Dim sumber As String = "SA", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Said, Sanotransaksi FROM M3_Sa WHERE Said='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT sacabang, salokasi, sasumber, saautonotransaksi, sanotransaksi, satgl"
            sql &= " FROM M3_sa"
            sql &= " WHERE said = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("sacabang")
                lokasi = dtNomorNext.Rows(0)("salokasi")
                sumber = dtNomorNext.Rows(0)("sasumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("saautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("sanotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("satgl"))
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
            sql = "DELETE FROM M3_Sa_Detail WHERE idsa = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE UTAMA
            sql = "DELETE FROM M3_Sa WHERE said = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M3_SaSearch(PostWsSearch(paramSplit(0), "M3_SaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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