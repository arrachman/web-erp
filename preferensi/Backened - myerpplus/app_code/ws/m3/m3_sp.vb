Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class wsm3_sp
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M3_SpSimpan(ByVal param As String) As String
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
        Dim Filter As String = "", Sorting As String = ""

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
            sql = "SELECT spid, spnotransaksi FROM m3_sp WHERE spnoref = '" & FixQuotes(Filter) & "'"
            Dim dtNoreff As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNoreff.Rows.Count > 0 Then
                If Len(dtNoreff.Rows(0)("spid")) > 0 Then
                    result(1) = 1
                    result(2) = dtNoreff.Rows(0)("spnotransaksi")
                    result(3) = 0
                    result(4) = dtNoreff.Rows(0)("spid")
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
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'spid(0) As Integer, spcabang(1) As String, splokasi(2) As String, spgudang(3) As String, spsumber(4) As String, 
        'spautonotransaksi(5) As Integer, spnotransaksi(6) As String, sptgl(7) As Date, spkodepa(8) As Integer, spbagiansp(9) As Integer, 
        'spbagianspkontak(10) As String, spuraian(11) As String, spcatatan(12) As String, spnoref(13) As String, sptglnoref(14) As Date, 
        'spstatussa(15) As Integer, spstatus(16) As Integer, spstatussebelumnya(17) As Integer, spjmlrevisi(18) As Integer, spcetakanke(19) As Integer, 
        'spinputuser(20) As Integer, spinputtgl(21) As DateTime, spmodifikasiuser(22) As Integer, spmodifikasitgl(23) As DateTime, spposting(24) As Integer, 
        'sptutupperiode(25) As Integer, spisclose(26) As Integer, spcustomtext1(27) As String, spcustomtext2(28) As String, spcustomtext3(29) As String, 
        'spcustomtext4(30) As String, spcustomtext5(31) As String, spcustomint1(32) As Integer, spcustomint2(33) As Integer, spcustomint3(34) As Integer, 
        'spcustomdbl1(35) As Double, spcustomdbl2(36) As Double, spcustomdbl3(37) As Double, spcustomdate1(38) As Date, spcustomdate2(39) As Date, 
        'spcustomdate3(40) As Date, spstepke(41) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'spid, spcabang, splokasi, spgudang, spsumber, spautonotransaksi, spnotransaksi, 
        'sptgl, spkodepa, spbagiansp, spbagianspkontak, spuraian, spcatatan, spnoref, 
        'sptglnoref, spstatussa, spstatus, spstatussebelumnya, spjmlrevisi, spcetakanke, spinputuser, 
        'spinputtgl, spmodifikasiuser, spmodifikasitgl, spposting, sptutupperiode, spisclose, spcustomtext1, 
        'spcustomtext2, spcustomtext3, spcustomtext4, spcustomtext5, spcustomint1, spcustomint2, spcustomint3, 
        'spcustomdbl1, spcustomdbl2, spcustomdbl3, spcustomdate1, spcustomdate2, spcustomdate3, spstepke

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 42) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'spid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "spid required numeric." : GoTo selesai
        End If
        'spautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "spautonotransaksi required numeric." : GoTo selesai
        End If
        'sptgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "sptgl required date." : GoTo selesai
        End If
        'spkodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "spkodepa required numeric." : GoTo selesai
        End If
        'spbagiansp(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "spbagiansp required numeric." : GoTo selesai
        End If
        If (dataUtama(9) < 1) Then
            result(2) = "spbagiansp can't be empty." : GoTo selesai
        End If
        'sptglnoref(14) As Date
        If (IsDate(dataUtama(14)) = False) Then
            result(2) = "sptglnoref required date." : GoTo selesai
        End If
        'spstatussa(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "spstatussa required numeric." : GoTo selesai
        End If
        'spstatus(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "spstatus required numeric." : GoTo selesai
        End If
        'spstatussebelumnya(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "spstatussebelumnya required numeric." : GoTo selesai
        End If
        'spjmlrevisi(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "spjmlrevisi required numeric." : GoTo selesai
        End If
        'spcetakanke(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "spcetakanke required numeric." : GoTo selesai
        End If
        'spinputuser(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "spinputuser required numeric." : GoTo selesai
        End If
        'spinputtgl(21) As DateTime
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "spinputtgl required date." : GoTo selesai
        End If
        'spmodifikasiuser(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "spmodifikasiuser required numeric." : GoTo selesai
        End If
        'spmodifikasitgl(23) As DateTime
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "spmodifikasitgl required date." : GoTo selesai
        End If
        'spposting(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "spposting required numeric." : GoTo selesai
        End If
        'sptutupperiode(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "sptutupperiode required numeric." : GoTo selesai
        End If
        'spisclose(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "spisclose required numeric." : GoTo selesai
        End If
        'spcustomint1(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "spcustomint1 required numeric." : GoTo selesai
        End If
        'spcustomint2(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "spcustomint2 required numeric." : GoTo selesai
        End If
        'spcustomint3(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "spcustomint3 required numeric." : GoTo selesai
        End If
        'spcustomdbl1(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "spcustomdbl1 required numeric." : GoTo selesai
        End If
        'spcustomdbl2(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "spcustomdbl2 required numeric." : GoTo selesai
        End If
        'spcustomdbl3(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "spcustomdbl3 required numeric." : GoTo selesai
        End If
        'spcustomdate1(38) As Date
        If (IsDate(dataUtama(38)) = False) Then
            result(2) = "spcustomdate1 required date." : GoTo selesai
        End If
        'spcustomdate2(39) As Date
        If (IsDate(dataUtama(39)) = False) Then
            result(2) = "spcustomdate2 required date." : GoTo selesai
        End If
        'spcustomdate3(40) As Date
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "spcustomdate3 required date." : GoTo selesai
        End If
        'spstepke(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "spstepke required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'spcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "spcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "spcabang should not be more than 25 character." : GoTo selesai
        End If

        'splokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "splokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "splokasi should not be more than 25 character." : GoTo selesai
        End If

        'spsumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "spsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "spsumber should not be more than 10 character." : GoTo selesai
        End If

        'spnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "spnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "spnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'sptgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "sptgl can't be empty" : GoTo selesai
        End If

        'sptglnoref(14) As Date
        If Len(dataUtama(14)) = 0 Then
            result(2) = "sptglnoref can't be empty" : GoTo selesai
        End If

        'spinputtgl(21) As DateTime
        If Len(dataUtama(21)) = 0 Then
            result(2) = "spinputtgl can't be empty" : GoTo selesai
        End If

        'spmodifikasitgl(23) As DateTime
        If Len(dataUtama(23)) = 0 Then
            result(2) = "spmodifikasitgl can't be empty" : GoTo selesai
        End If

        'spcustomdbl1(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "spcustomdbl1 can't be empty" : GoTo selesai
        End If

        'spcustomdbl2(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "spcustomdbl2 can't be empty" : GoTo selesai
        End If

        'spcustomdbl3(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "spcustomdbl3 can't be empty" : GoTo selesai
        End If

        'spcustomdate1(38) As Date
        If Len(dataUtama(38)) = 0 Then
            result(2) = "spcustomdate1 can't be empty" : GoTo selesai
        End If

        'spcustomdate2(39) As Date
        If Len(dataUtama(39)) = 0 Then
            result(2) = "spcustomdate2 can't be empty" : GoTo selesai
        End If

        'spcustomdate3(40) As Date
        If Len(dataUtama(40)) = 0 Then
            result(2) = "spcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "spid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "splokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sptgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spbagiansp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spbagianspkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sptglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spstatussa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sptutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spstepke", AsEnumTypeData.AsInt64)
        If AsDataTableTambahData(dtutama, "spid~spcabang~splokasi~spgudang~spsumber~spautonotransaksi~spnotransaksi~sptgl~spkodepa~spbagiansp~spbagianspkontak~spuraian~spcatatan~spnoref~sptglnoref~spstatussa~spstatus~spstatussebelumnya~spjmlrevisi~spcetakanke~spinputuser~spinputtgl~spmodifikasiuser~spmodifikasitgl~spposting~sptutupperiode~spisclose~spcustomtext1~spcustomtext2~spcustomtext3~spcustomtext4~spcustomtext5~spcustomint1~spcustomint2~spcustomint3~spcustomdbl1~spcustomdbl2~spcustomdbl3~spcustomdate1~spcustomdate2~spcustomdate3~spstepke", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idspdetail(0) As Integer, idsp(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jmlsistem(5) As Double, jmlfisik(6) As Double, jmlbagus(7) As Double, jmlrusak(8) As Double, selisih(9) As Double, 
        'satuan(10) As String, nilaisatuan(11) As Double, jmlbarangsistem(12) As Double, jmlbarangfisik(13) As Double, jmlbarangbagus(14) As Double, 
        'jmlbarangrusak(15) As Double, selisihbarang(16) As Double, satuanbarang(17) As String, cabang(18) As String, lokasi(19) As String, 
        'gudang(20) As String, lokasibarang(21) As String, jmlsa(22) As Double, statussa(23) As Integer, costcenter(24) As String, 
        'divisi(25) As String, subdivisi(26) As String, proyek(27) As String, catatan(28) As String, urutan(29) As Integer, 
        'isclose(30) As Integer, customtext1(31) As String, customtext2(32) As String, customtext3(33) As String, customdbl1(34) As Double, 
        'customdbl2(35) As Double, customdbl3(36) As Double, customdate1(37) As Date, customdate2(38) As Date, customdate3(39) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idspdetail, idsp, idbarang, namabarang, tipebarang, jmlsistem, jmlfisik, 
        'jmlbagus, jmlrusak, selisih, satuan, nilaisatuan, jmlbarangsistem, jmlbarangfisik, 
        'jmlbarangbagus, jmlbarangrusak, selisihbarang, satuanbarang, cabang, lokasi, gudang, 
        'lokasibarang, jmlsa, statussa, costcenter, divisi, subdivisi, proyek, 
        'catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idspdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idsp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlsistem", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlfisik", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbagus", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlrusak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "selisih", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarangsistem", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarangfisik", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarangbagus", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarangrusak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "selisihbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasibarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlsa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statussa", AsEnumTypeData.AsInt64)
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

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 40) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idspdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idspdetail required numeric." : GoTo selesai
            End If
            'idsp(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idsp required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'jmlsistem(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - jmlsistem required numeric." : GoTo selesai
            End If
            'jmlfisik(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - jmlfisik required numeric." : GoTo selesai
            End If
            'jmlbagus(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - jmlbagus required numeric." : GoTo selesai
            End If
            'jmlrusak(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - jmlrusak required numeric." : GoTo selesai
            End If
            'selisih(9) As Double
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - selisih required numeric." : GoTo selesai
            End If
            'nilaisatuan(11) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarangsistem(12) As Double
            'jmlbarangsistem = jmlsistem * nilaisatuan
            dataRowDetail(12) = Double.Parse(dataRowDetail(5)) * Double.Parse(dataRowDetail(11))
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - jmlbarangsistem required numeric." : GoTo selesai
            End If
            'jmlbarangfisik(13) As Double
            'jmlbarangfisik = jmlfisik * nilaisatuan
            dataRowDetail(13) = Double.Parse(dataRowDetail(6)) * Double.Parse(dataRowDetail(11))
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - jmlbarangfisik required numeric." : GoTo selesai
            End If
            'jmlbarangbagus(14) As Double
            'jmlbarangbagus = jmlbagus * nilaisatuan
            dataRowDetail(14) = Double.Parse(dataRowDetail(7)) * Double.Parse(dataRowDetail(11))
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - jmlbarangbagus required numeric." : GoTo selesai
            End If
            'jmlbarangrusak(15) As Double
            'jmlbarangrusak = jmlrusak * nilaisatuan
            dataRowDetail(15) = Double.Parse(dataRowDetail(8)) * Double.Parse(dataRowDetail(11))
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - jmlbarangrusak required numeric." : GoTo selesai
            End If
            'selisihbarang(16) As Double
            'selisihbarang = selisih * nilaisatuan
            dataRowDetail(16) = Double.Parse(dataRowDetail(9)) * Double.Parse(dataRowDetail(11))
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - selisihbarang required numeric." : GoTo selesai
            End If
            'jmlsa(22) As Double
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - jmlsa required numeric." : GoTo selesai
            End If
            'statussa(23) As Integer
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - statussa required numeric." : GoTo selesai
            End If
            'urutan(29) As Integer
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'isclose(30) As Integer
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(34) As Double
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(35) As Double
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(36) As Double
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(37) As Date
            If (IsDate(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(38) As Date
            If (IsDate(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(39) As Date
            If (IsDate(dataRowDetail(39)) = False) Then
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

            'jmlsistem(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - jmlsistem can't be empty" : GoTo selesai
            End If

            'jmlfisik(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - jmlfisik can't be empty" : GoTo selesai
            End If

            'jmlbagus(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - jmlbagus can't be empty" : GoTo selesai
            End If

            'jmlrusak(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - jmlrusak can't be empty" : GoTo selesai
            End If

            'selisih(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - selisih can't be empty" : GoTo selesai
            End If

            'satuan(10) As String
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(10)) > 25 Then
                result(2) = "Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'nilaisatuan(11) As Double
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmlbarangsistem(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarangsistem can't be empty" : GoTo selesai
            End If

            'jmlbarangfisik(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarangfisik can't be empty" : GoTo selesai
            End If

            'jmlbarangbagus(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarangbagus can't be empty" : GoTo selesai
            End If

            'jmlbarangrusak(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarangrusak can't be empty" : GoTo selesai
            End If

            'selisihbarang(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - selisihbarang can't be empty" : GoTo selesai
            End If

            'satuanbarang(17) As String
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(17)) > 25 Then
                result(2) = "Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
            End If

            'lokasibarang(21) As String
            'If Len(dataRowDetail(21)) = 0 Then
            '    result(2) = "Row : " & i & " - lokasibarang can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(21)) > 25 Then
                result(2) = "Row : " & i & " - lokasibarang should not be more than 25 character." : GoTo selesai
            End If

            'jmlsa(22) As Double
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - jmlsa can't be empty" : GoTo selesai
            End If

            'customdbl1(34) As Double
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(35) As Double
            If Len(dataRowDetail(35)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(36) As Double
            If Len(dataRowDetail(36)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(37) As Date
            If Len(dataRowDetail(37)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(38) As Date
            If Len(dataRowDetail(38)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(39) As Date
            If Len(dataRowDetail(39)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idspdetail~idsp~idbarang~namabarang~tipebarang~jmlsistem~jmlfisik~jmlbagus~jmlrusak~selisih~satuan~nilaisatuan~jmlbarangsistem~jmlbarangfisik~jmlbarangbagus~jmlbarangrusak~selisihbarang~satuanbarang~cabang~lokasi~gudang~lokasibarang~jmlsa~statussa~costcenter~divisi~subdivisi~proyek~catatan~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39)) = False Then
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
                Dim vModuleId As Integer = 3, vMenuId As Integer = 7
                Select Case drutama("spstatus")
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("sptgl")), AsFormatTanggal(drutama("sptgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================

                If isUpdate Then
                    result(4) = drutama("spid")
                    notransaksi = drutama("spnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(spid), spnotransaksi FROM M3_Sp WHERE spid='" & result(4) & "' AND spstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("spautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("spcabang"), drutama("splokasi"), drutama("spsumber"), drutama("sptgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(spid) FROM m3_sp WHERE spnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m3_sp_history
                        Dim rsSimpanHistory As String = SimpanHistory.M3_Sp_HistorySimpan("" & paramSplit(0) & "★M3_Sp_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("spsumber")) & "▼" & FixQuotes(drutama("spid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M3_Sp set spcabang  = '" & FixQuotes(drutama("spcabang")) & "', splokasi  = '" & FixQuotes(drutama("splokasi")) & "', spgudang  = '" & FixQuotes(drutama("spgudang")) & "', spsumber  = '" & FixQuotes(drutama("spsumber")) & "', spautonotransaksi  = " & drutama("spautonotransaksi") & ", spnotransaksi  = '" & notransaksi & "', sptgl  = '" & FixQuotes(AsFormatTanggal(drutama("sptgl"))) & "', spkodepa  = " & drutama("spkodepa") & ", spbagiansp  = " & drutama("spbagiansp") & ", spbagianspkontak  = '" & FixQuotes(drutama("spbagianspkontak")) & "', spuraian  = '" & FixQuotes(drutama("spuraian")) & "', spcatatan  = '" & FixQuotes(drutama("spcatatan")) & "', spnoref  = '" & FixQuotes(drutama("spnoref")) & "', sptglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("sptglnoref"))) & "', spstatussa  = " & drutama("spstatussa") & ", spstatus  = " & drutama("spstatus") & ", spstatussebelumnya  = " & drutama("spstatussebelumnya") & ", spjmlrevisi  = spjmlrevisi+1, spcetakanke  = " & drutama("spcetakanke") & ", spmodifikasiuser  = " & drutama("spmodifikasiuser") & ", spmodifikasitgl  = NOW(), spposting  = 0, sptutupperiode  = " & drutama("sptutupperiode") & ", spcustomtext1  = '" & FixQuotes(drutama("spcustomtext1")) & "', spcustomtext2  = '" & FixQuotes(drutama("spcustomtext2")) & "', spcustomtext3  = '" & FixQuotes(drutama("spcustomtext3")) & "', spcustomtext4  = '" & FixQuotes(drutama("spcustomtext4")) & "', spcustomtext5  = '" & FixQuotes(drutama("spcustomtext5")) & "', spcustomint1  = " & drutama("spcustomint1") & ", spcustomint2  = " & drutama("spcustomint2") & ", spcustomint3  = " & drutama("spcustomint3") & ", spcustomdbl1  = '" & FixDouble(drutama("spcustomdbl1")) & "', spcustomdbl2  = '" & FixDouble(drutama("spcustomdbl2")) & "', spcustomdbl3  = '" & FixDouble(drutama("spcustomdbl3")) & "', spcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("spcustomdate1"))) & "', spcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("spcustomdate2"))) & "', spcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("spcustomdate3"))) & "', spstepke = '" & drutama("spstepke") & "' where spid = '" & drutama("spid") & "'"
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

                    If drutama("spautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("spcabang"), drutama("splokasi"), drutama("spsumber"), drutama("sptgl"))
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
                        notransaksi = drutama("spnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(spid) FROM m3_sp WHERE spnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M3_Sp (spcabang, splokasi, spgudang, spsumber, spautonotransaksi, spnotransaksi, sptgl, spkodepa, spbagiansp, spbagianspkontak, spuraian, spcatatan, spnoref, sptglnoref, spstatussa, spstatus, spstatussebelumnya, spjmlrevisi, spcetakanke, spinputuser, spinputtgl, spmodifikasiuser, spmodifikasitgl, spposting, sptutupperiode, spisclose, spcustomtext1, spcustomtext2, spcustomtext3, spcustomtext4, spcustomtext5, spcustomint1, spcustomint2, spcustomint3, spcustomdbl1, spcustomdbl2, spcustomdbl3, spcustomdate1, spcustomdate2, spcustomdate3, spstepke) values('" & FixQuotes(drutama("spcabang")) & "', '" & FixQuotes(drutama("splokasi")) & "', '" & FixQuotes(drutama("spgudang")) & "', '" & FixQuotes(drutama("spsumber")) & "', " & drutama("spautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("sptgl"))) & "', " & drutama("spkodepa") & ", " & drutama("spbagiansp") & ", '" & FixQuotes(drutama("spbagianspkontak")) & "', '" & FixQuotes(drutama("spuraian")) & "', '" & FixQuotes(drutama("spcatatan")) & "', '" & FixQuotes(drutama("spnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sptglnoref"))) & "', " & drutama("spstatussa") & ", " & drutama("spstatus") & ", " & drutama("spstatussebelumnya") & ", " & drutama("spjmlrevisi") & ", " & drutama("spcetakanke") & ", " & drutama("spinputuser") & ", NOW(), " & drutama("spmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("sptutupperiode") & ", " & drutama("spisclose") & ", '" & FixQuotes(drutama("spcustomtext1")) & "', '" & FixQuotes(drutama("spcustomtext2")) & "', '" & FixQuotes(drutama("spcustomtext3")) & "', '" & FixQuotes(drutama("spcustomtext4")) & "', '" & FixQuotes(drutama("spcustomtext5")) & "', " & drutama("spcustomint1") & ", " & drutama("spcustomint2") & ", " & drutama("spcustomint3") & ", '" & FixDouble(drutama("spcustomdbl1")) & "', '" & FixDouble(drutama("spcustomdbl2")) & "', '" & FixDouble(drutama("spcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("spcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("spcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("spcustomdate3"))) & "', '" & drutama("spstepke") & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select spid from M3_Sp where spnotransaksi='" & notransaksi & "' AND spinputuser= '" & userid & "' order by spmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main progress transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M3_Sp_Detail where idsp = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idspdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jmlsistem")) & "', '" & FixDouble(dr1("jmlfisik")) & "', '" & FixDouble(dr1("jmlbagus")) & "', '" & FixDouble(dr1("jmlrusak")) & "', '" & FixDouble(dr1("selisih")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarangsistem")) & "', '" & FixDouble(dr1("jmlbarangfisik")) & "', '" & FixDouble(dr1("jmlbarangbagus")) & "', '" & FixDouble(dr1("jmlbarangrusak")) & "', '" & FixDouble(dr1("selisihbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("lokasibarang")) & "', '" & FixDouble(dr1("jmlsa")) & "', " & dr1("statussa") & ", '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M3_Sp_Detail(idspdetail, idsp, idbarang, namabarang, tipebarang, jmlsistem, jmlfisik, jmlbagus, jmlrusak, selisih, satuan, nilaisatuan, jmlbarangsistem, jmlbarangfisik, jmlbarangbagus, jmlbarangrusak, selisihbarang, satuanbarang, cabang, lokasi, gudang, lokasibarang, jmlsa, statussa, costcenter, divisi, subdivisi, proyek, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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


                'INSERT KE TABEL PEMBANTU UNTUK KEPENTINGAN CETAK LAPORAN REKAP MUTASI STOK
                'ADA KONDISI KETIKA BELUM INPUT SP MAKA TIDAK BISA CETAK LAPORAN REKAP MUTASI STOK
                If drutama("spstatus") = 2 Then
                    sql = "INSERT INTO m2r_mutasi_stok_custom(csumber, ccabang, clokasi, cgudang, ctgl, cnotransaksi) VALUES('" & FixQuotes(drutama("spsumber")) & "', '" & FixQuotes(drutama("spcabang")) & "', '" & FixQuotes(drutama("splokasi")) & "', '" & FixQuotes(drutama("spgudang")) & "', '" & AsFormatTanggal(FixQuotes(drutama("sptgl"))) & "', '" & FixQuotes(notransaksi) & "') ON DUPLICATE KEY UPDATE cnotransaksi = VALUES(cnotransaksi)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                ''SIMPAN PROGRESS ========================
                'Dim rsSimpanProgress As String = M3_Sp_ProgressSimpan("" & paramSplit(0) & "★M3_Sp_ProgressSimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("spstepke")) & "▼" & FixQuotes(result(4)) & "")
                'Dim rsSplitProgress() As String = rsSimpanProgress.Split(sptParam)
                'Dim rsSplitResultProgress() As String = rsSplitProgress(0).Split(sptSubParam)
                ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                'If (rsSplitResultProgress(1) = 0) Then
                '    result(2) = "Insert Progress failed : " & rsSplitResultProgress(2) : Trans.Rollback() : GoTo selesai
                'End If
                ''END OF SIMPAN PROGRESS ==================


                'PROSES HAPUS PROGRESS TAHAP DAN IDTRANSAKSI YANG SAMA --------------
                sql = "DELETE spd, sp FROM m3_sp_detail_progress spd JOIN m3_sp_progress sp ON spd.idprogress = sp.spidprogress WHERE sp.spid = '" & FixQuotes(result(4)) & "' AND sp.spstepke = '" & FixQuotes(drutama("spstepke")) & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF PROSES HAPUS PROGRESS TAHAP DAN IDTRANSAKSI YANG SAMA -------


                'PROSES INSERT Progress UTAMA ---------------------------------------
                sql = "INSERT INTO m3_sp_progress(SELECT 0, sp.* FROM m3_sp sp WHERE sp.spid = '" & FixQuotes(result(4)) & "')"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF PROSES INSERT Progress UTAMA --------------------------------


                'PROSES AMBIL ID Progress YANG BARUSAJA DIINSERT --------------------
                Dim dt3 As New DataTable, idProgress As Double = 0
                sql = "SELECT spidprogress FROM m3_sp_progress WHERE spid = '" & FixQuotes(result(4)) & "' ORDER BY spmodifikasitgl DESC LIMIT 1"
                dt3 = AsDataTableAmbilDariDBCon(sql, myConn)
                If dt3.Rows.Count > 0 Then idProgress = dt3.Rows(0)(0) Else result(2) = "Progress main transaction data not found." : Trans.Rollback() : GoTo selesai
                'END OF PROSES AMBIL ID Progress YANG BARUSAJA DIINSERT -------------


                'PROSES INSERT Progress DETAIL --------------------------------------
                sql = "INSERT INTO m3_sp_detail_progress (SELECT 0, '" & idProgress & "', sp.* FROM m3_sp_detail sp WHERE sp.idsp = '" & FixQuotes(result(4)) & "' )"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF PROSES INSERT Progress DETAIL -------------------------------


                'INSERT USER LOG ====================================================================
                Dim sumber As String = "Sp", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M3_SpUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("spbagianspkode", "c1.kkode")
            Filter = Filter.Replace("spbagianspnama", "c1.knama")
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
            Dim sumber As String = "Sp", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Sptgl, Spnotransaksi, Spstatus FROM m3_Sp WHERE Spid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Spstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m3_sp_history
            Dim rsSimpanHistory As String = SimpanHistory.M3_Sp_HistorySimpan("" & paramSplit(0) & "★M3_Sp_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m3_sp_terkait("spid = '" & idtransaksi & "'")
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================
            End If

            'update status utama
            sql = "UPDATE M3_Sp SET Spstatus = " & nilaiStatus & ", Spmodifikasiuser='" & userid & "', Spmodifikasitgl = NOW(), Spposting = 0, Sppostingtgl = '1971-01-01 00:00:00', Spjmlrevisi = Spjmlrevisi + 1 WHERE Spid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M3_SpSearch(PostWsSearch(paramSplit(0), "M3_SpSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M3_SpDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("spbagianspkode", "c1.kkode")
            Filter = Filter.Replace("spbagianspnama", "c1.knama")
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
            Dim sumber As String = "Sp", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Spid, Spnotransaksi FROM M3_Sp WHERE Spid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT spcabang, splokasi, spsumber, spautonotransaksi, spnotransaksi, sptgl"
            sql &= " FROM M3_sp"
            sql &= " WHERE spid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("spcabang")
                lokasi = dtNomorNext.Rows(0)("splokasi")
                sumber = dtNomorNext.Rows(0)("spsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("spautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("spnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("sptgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M3_Sp_Detail WHERE idsp = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M3_Sp WHERE spid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M3_SpSearch(PostWsSearch(paramSplit(0), "M3_SpSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M3_SpGetdataById(ByVal param As String) As String

        'M3_SpGetdataById Utama --------------------------------------------------------
        'spid, spcabang, splokasi, spgudang, spsumber, spautonotransaksi, spnotransaksi, 
        'sptgl, spkodepa, spbagiansp, spbagianspkontak, spuraian, spcatatan, spnoref, 
        'sptglnoref, spstatussa, spstatus, spstatussebelumnya, spjmlrevisi, spcetakanke, spinputuser, 
        'spinputtgl, spmodifikasiuser, spmodifikasitgl, spposting, sppostingtgl, sptutupperiode, spisclose, 
        'spcustomtext1, spcustomtext2, spcustomtext3, spcustomtext4, spcustomtext5, spcustomint1, spcustomint2, 
        'spcustomint3, spcustomdbl1, spcustomdbl2, spcustomdbl3, spcustomdate1, spcustomdate2, spcustomdate3, 
        'spcabangnama, splokasinama, spgudangnama, spbagianspkode, spbagianspnama, spstatusnama, spstatussebelumnyanama, 
        'spinputusernama, spmodifikasiusernama, spstepke

        'M3_SpGetdataById Detail -------------------------------------------------------
        'idspdetail, idsp, idbarang, namabarang, tipebarang, 
        'jmlsistem, jmlfisik, jmlbagus, jmlrusak, selisih, satuan, nilaisatuan, 
        'jmlbarangsistem, jmlbarangfisik, jmlbarangbagus, jmlbarangrusak, selisihbarang, satuanbarang, cabang, 
        'lokasi, gudang, lokasibarang, jmlsa, statussa, costcenter, divisi, 
        'subdivisi, proyek, catatan, urutan, isclose, customtext1, customtext2, 
        'customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'kodebarang, cabangnama, lokasinama, gudangnama, lokasibarangnama, costcenternama, divisinama, 
        'subdivisinama, proyeknama


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

        Dim NmMemcached As String = "aplikasi1-M3_Sp~M3_Sp_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "spid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "spid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m3_sp_getdata")
        sql = "select `sp`.`spid` AS `spid`,`sp`.`spcabang` AS `spcabang`,`sp`.`splokasi` AS `splokasi`,`sp`.`spgudang` AS `spgudang`,`sp`.`spsumber` AS `spsumber`,`sp`.`spautonotransaksi` AS `spautonotransaksi`,`sp`.`spnotransaksi` AS `spnotransaksi`,`sp`.`sptgl` AS `sptgl`,`sp`.`spkodepa` AS `spkodepa`,`sp`.`spbagiansp` AS `spbagiansp`,`sp`.`spbagianspkontak` AS `spbagianspkontak`,`sp`.`spuraian` AS `spuraian`,`sp`.`spcatatan` AS `spcatatan`,`sp`.`spnoref` AS `spnoref`,`sp`.`sptglnoref` AS `sptglnoref`,`sp`.`spstatussa` AS `spstatussa`,`sp`.`spstatus` AS `spstatus`,`sp`.`spstatussebelumnya` AS `spstatussebelumnya`,`sp`.`spjmlrevisi` AS `spjmlrevisi`,`sp`.`spcetakanke` AS `spcetakanke`,`sp`.`spinputuser` AS `spinputuser`,`sp`.`spinputtgl` AS `spinputtgl`,`sp`.`spmodifikasiuser` AS `spmodifikasiuser`,`sp`.`spmodifikasitgl` AS `spmodifikasitgl`,`sp`.`spposting` AS `spposting`,`sp`.`sppostingtgl` AS `sppostingtgl`,`sp`.`sptutupperiode` AS `sptutupperiode`,`sp`.`spisclose` AS `spisclose`,`sp`.`spcustomtext1` AS `spcustomtext1`,`sp`.`spcustomtext2` AS `spcustomtext2`,`sp`.`spcustomtext3` AS `spcustomtext3`,`sp`.`spcustomtext4` AS `spcustomtext4`,`sp`.`spcustomtext5` AS `spcustomtext5`,`sp`.`spcustomint1` AS `spcustomint1`,`sp`.`spcustomint2` AS `spcustomint2`,`sp`.`spcustomint3` AS `spcustomint3`,`sp`.`spcustomdbl1` AS `spcustomdbl1`,`sp`.`spcustomdbl2` AS `spcustomdbl2`,`sp`.`spcustomdbl3` AS `spcustomdbl3`,`sp`.`spcustomdate1` AS `spcustomdate1`,`sp`.`spcustomdate2` AS `spcustomdate2`,`sp`.`spcustomdate3` AS `spcustomdate3`,`br`.`bnama` AS `spcabangnama`,`lc`.`lnama` AS `splokasinama`,`wh`.`wnama` AS `spgudangnama`,`c1`.`kkode` AS `spbagianspkode`,`c1`.`knama` AS `spbagianspnama`,`st1`.`nama` AS `spstatusnama`,`st2`.`nama` AS `spstatussebelumnyanama`,`u1`.`unama` AS `spinputusernama`,`u2`.`unama` AS `spmodifikasiusernama`,`spd`.`idspdetail` AS `idspdetail`,`spd`.`idsp` AS `idsp`,`spd`.`idbarang` AS `idbarang`,`spd`.`namabarang` AS `namabarang`,`spd`.`tipebarang` AS `tipebarang`,`spd`.`jmlsistem` AS `jmlsistem`,`spd`.`jmlfisik` AS `jmlfisik`,`spd`.`jmlbagus` AS `jmlbagus`,`spd`.`jmlrusak` AS `jmlrusak`,`spd`.`selisih` AS `selisih`,`spd`.`satuan` AS `satuan`,`spd`.`nilaisatuan` AS `nilaisatuan`,`spd`.`jmlbarangsistem` AS `jmlbarangsistem`,`spd`.`jmlbarangfisik` AS `jmlbarangfisik`,`spd`.`jmlbarangbagus` AS `jmlbarangbagus`,`spd`.`jmlbarangrusak` AS `jmlbarangrusak`,`spd`.`selisihbarang` AS `selisihbarang`,`spd`.`satuanbarang` AS `satuanbarang`,`spd`.`cabang` AS `cabang`,`spd`.`lokasi` AS `lokasi`,`spd`.`gudang` AS `gudang`,`spd`.`lokasibarang` AS `lokasibarang`,`spd`.`jmlsa` AS `jmlsa`,`spd`.`statussa` AS `statussa`,`spd`.`costcenter` AS `costcenter`,`spd`.`divisi` AS `divisi`,`spd`.`subdivisi` AS `subdivisi`,`spd`.`proyek` AS `proyek`,`spd`.`catatan` AS `catatan`,`spd`.`urutan` AS `urutan`,`spd`.`isclose` AS `isclose`,`spd`.`customtext1` AS `customtext1`,`spd`.`customtext2` AS `customtext2`,`spd`.`customtext3` AS `customtext3`,`spd`.`customdbl1` AS `customdbl1`,`spd`.`customdbl2` AS `customdbl2`,`spd`.`customdbl3` AS `customdbl3`,`spd`.`customdate1` AS `customdate1`,`spd`.`customdate2` AS `customdate2`,`spd`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`brd`.`bnama` AS `cabangnama`,`lcd`.`lnama` AS `lokasinama`,`whd`.`wnama` AS `gudangnama`,`il`.`ilnama` AS `lokasibarangnama`,`cc`.`ccnama` AS `costcenternama`,`d`.`dnama` AS `divisinama`,`sd`.`sdnama` AS `subdivisinama`,`p`.`pnama` AS `proyeknama`, sp.spstepke from ((((((((((((((((((`m3_sp` `sp` join `m3_sp_detail` `spd` on((`sp`.`spid` = `spd`.`idsp`))) left join `m1_branch` `br` on((`br`.`bkode` = `sp`.`spcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `sp`.`splokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `sp`.`spgudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `sp`.`spbagiansp`))) left join `m0_status` `st1` on((`st1`.`kode` = `sp`.`spstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `sp`.`spstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `sp`.`spinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `sp`.`spmodifikasiuser`))) left join `m1_item` `i` on((`i`.`bid` = `spd`.`idbarang`))) left join `m1_branch` `brd` on((`spd`.`cabang` = `brd`.`bkode`))) left join `m1_location` `lcd` on((`spd`.`lokasi` = `lcd`.`lkode`))) left join `m1_warehouse` `whd` on((`spd`.`gudang` = `whd`.`wkode`))) left join `m1_item_location` `il` on((`spd`.`lokasibarang` = `il`.`ilkode` AND spd.gudang = il.ilgudang))) left join `m1_cost_center` `cc` on((`spd`.`costcenter` = `cc`.`cckode`))) left join `m1_division` `d` on((`spd`.`divisi` = `d`.`dkode`))) left join `m1_subdivision` `sd` on((`spd`.`subdivisi` = `sd`.`sdkode`))) left join `m1_project` `p` on((`spd`.`proyek` = `p`.`pkode`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                     FxDB(drutama("spid"), 0), sptField,
                     FxDB(drutama("spcabang"), ""), sptField,
                     FxDB(drutama("splokasi"), ""), sptField,
                     FxDB(drutama("spgudang"), ""), sptField,
                     FxDB(drutama("spsumber"), ""), sptField,
                     FxDB(drutama("spautonotransaksi"), 0), sptField,
                     FxDB(drutama("spnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sptgl"), ""), formatTgl), sptField,
                     FxDB(drutama("spkodepa"), 0), sptField,
                     FxDB(drutama("spbagiansp"), 0), sptField,
                     FxDB(drutama("spbagianspkontak"), ""), sptField,
                     FxDB(drutama("spuraian"), ""), sptField,
                     FxDB(drutama("spcatatan"), ""), sptField,
                     FxDB(drutama("spnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sptglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("spstatussa"), 0), sptField,
                     FxDB(drutama("spstatus"), 0), sptField,
                     FxDB(drutama("spstatussebelumnya"), 0), sptField,
                     FxDB(drutama("spjmlrevisi"), 0), sptField,
                     FxDB(drutama("spcetakanke"), 0), sptField,
                     FxDB(drutama("spinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("spinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("spmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("spmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("spposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sppostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sptutupperiode"), 0), sptField,
                     FxDB(drutama("spisclose"), 0), sptField,
                     FxDB(drutama("spcustomtext1"), ""), sptField,
                     FxDB(drutama("spcustomtext2"), ""), sptField,
                     FxDB(drutama("spcustomtext3"), ""), sptField,
                     FxDB(drutama("spcustomtext4"), ""), sptField,
                     FxDB(drutama("spcustomtext5"), ""), sptField,
                     FxDB(drutama("spcustomint1"), 0), sptField,
                     FxDB(drutama("spcustomint2"), 0), sptField,
                     FxDB(drutama("spcustomint3"), 0), sptField,
                     FxDB(drutama("spcustomdbl1"), 0), sptField,
                     FxDB(drutama("spcustomdbl2"), 0), sptField,
                     FxDB(drutama("spcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("spcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("spcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("spcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("spcabangnama"), ""), sptField,
                     FxDB(drutama("splokasinama"), ""), sptField,
                     FxDB(drutama("spgudangnama"), ""), sptField,
                     FxDB(drutama("spbagianspkode"), ""), sptField,
                     FxDB(drutama("spbagianspnama"), ""), sptField,
                     FxDB(drutama("spstatusnama"), ""), sptField,
                     FxDB(drutama("spstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("spinputusernama"), ""), sptField,
                     FxDB(drutama("spmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("spstepke"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idspdetail"), 0), sptField,
                     FxDB(dr("idsp"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jmlsistem"), 0), sptField,
                     FxDB(dr("jmlfisik"), 0), sptField,
                     FxDB(dr("jmlbagus"), 0), sptField,
                     FxDB(dr("jmlrusak"), 0), sptField,
                     FxDB(dr("selisih"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarangsistem"), 0), sptField,
                     FxDB(dr("jmlbarangfisik"), 0), sptField,
                     FxDB(dr("jmlbarangbagus"), 0), sptField,
                     FxDB(dr("jmlbarangrusak"), 0), sptField,
                     FxDB(dr("selisihbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("lokasibarang"), ""), sptField,
                     FxDB(dr("jmlsa"), 0), sptField,
                     FxDB(dr("statussa"), 0), sptField,
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
                     FxDB(dr("cabangnama"), ""), sptField,
                     FxDB(dr("lokasinama"), ""), sptField,
                     FxDB(dr("gudangnama"), ""), sptField,
                     FxDB(dr("lokasibarangnama"), ""), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("spid, spcabang, splokasi, spgudang, spsumber, spautonotransaksi, spnotransaksi, sptgl, spkodepa, spbagiansp, spbagianspkontak, spuraian, spcatatan, spnoref, sptglnoref, spstatussa, spstatus, spstatussebelumnya, spjmlrevisi, spcetakanke, spinputuser, spinputtgl, spmodifikasiuser, spmodifikasitgl, spposting, sppostingtgl, sptutupperiode, spisclose, spcustomtext1, spcustomtext2, spcustomtext3, spcustomtext4, spcustomtext5, spcustomint1, spcustomint2, spcustomint3, spcustomdbl1, spcustomdbl2, spcustomdbl3, spcustomdate1, spcustomdate2, spcustomdate3, spcabangnama, splokasinama, spgudangnama, spbagianspkode, spbagianspnama, spstatusnama, spstatussebelumnyanama, spinputusernama, spmodifikasiusernama, spstepke" & sptSubParam & "idspdetail, idsp, idbarang, namabarang, tipebarang, jmlsistem, jmlfisik, jmlbagus, jmlrusak, selisih, satuan, nilaisatuan, jmlbarangsistem, jmlbarangfisik, jmlbarangbagus, jmlbarangrusak, selisihbarang, satuanbarang, cabang, lokasi, gudang, lokasibarang, jmlsa, statussa, costcenter, divisi, subdivisi, proyek, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, cabangnama, lokasinama, gudangnama, lokasibarangnama, costcenternama, divisinama, subdivisinama, proyeknama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_SpSearch(ByVal param As String) As String
        'M3_SpSearch --------------------------------------------------------
        'spid, spcabang, splokasi, spgudang, spsumber, spautonotransaksi, spnotransaksi, 
        'sptgl, spkodepa, spbagiansp, spbagianspkontak, spuraian, spcatatan, spnoref, 
        'sptglnoref, spstatussa, spstatus, spstatussebelumnya, spjmlrevisi, spcetakanke, spinputuser, 
        'spinputtgl, spmodifikasiuser, spmodifikasitgl, spposting, sppostingtgl, sptutupperiode, spisclose, 
        'spcabangnama, splokasinama, spgudangnama, spbagianspkode, spbagianspnama, spstatusnama, spstatussebelumnyanama, 
        'spinputusernama, spmodifikasiusernama, spstepke

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
            Filter = Filter.Replace("spbagianspkode", "c1.kkode")
            Filter = Filter.Replace("spbagianspnama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m3_sp_v")
        sql = "select `sp`.`spid` AS `spid`,`sp`.`spcabang` AS `spcabang`,`sp`.`splokasi` AS `splokasi`,`sp`.`spgudang` AS `spgudang`,`sp`.`spsumber` AS `spsumber`,`sp`.`spautonotransaksi` AS `spautonotransaksi`,`sp`.`spnotransaksi` AS `spnotransaksi`,`sp`.`sptgl` AS `sptgl`,`sp`.`spkodepa` AS `spkodepa`,`sp`.`spbagiansp` AS `spbagiansp`,`sp`.`spbagianspkontak` AS `spbagianspkontak`,`sp`.`spuraian` AS `spuraian`,`sp`.`spcatatan` AS `spcatatan`,`sp`.`spnoref` AS `spnoref`,`sp`.`sptglnoref` AS `sptglnoref`,`sp`.`spstatussa` AS `spstatussa`,`sp`.`spstatus` AS `spstatus`,`sp`.`spstatussebelumnya` AS `spstatussebelumnya`,`sp`.`spjmlrevisi` AS `spjmlrevisi`,`sp`.`spcetakanke` AS `spcetakanke`,`sp`.`spinputuser` AS `spinputuser`,`sp`.`spinputtgl` AS `spinputtgl`,`sp`.`spmodifikasiuser` AS `spmodifikasiuser`,`sp`.`spmodifikasitgl` AS `spmodifikasitgl`,`sp`.`spposting` AS `spposting`,`sp`.`sppostingtgl` AS `sppostingtgl`,`sp`.`sptutupperiode` AS `sptutupperiode`,`sp`.`spisclose` AS `spisclose`,`br`.`bnama` AS `spcabangnama`,`lc`.`lnama` AS `splokasinama`,`wh`.`wnama` AS `spgudangnama`,`c1`.`kkode` AS `spbagianspkode`,`c1`.`knama` AS `spbagianspnama`,`st1`.`nama` AS `spstatusnama`,`st2`.`nama` AS `spstatussebelumnyanama`,`u1`.`unama` AS `spinputusernama`,`u2`.`unama` AS `spmodifikasiusernama`, sp.spstepke from ((((((((`m3_sp` `sp` left join `m1_branch` `br` on((`br`.`bkode` = `sp`.`spcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `sp`.`splokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `sp`.`spgudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `sp`.`spbagiansp`))) left join `m0_status` `st1` on((`st1`.`kode` = `sp`.`spstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `sp`.`spstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `sp`.`spinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `sp`.`spmodifikasiuser`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M3_Sp", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("spid"), 0), sptField,
                     FxDB(dr("spcabang"), ""), sptField,
                     FxDB(dr("splokasi"), ""), sptField,
                     FxDB(dr("spgudang"), ""), sptField,
                     FxDB(dr("spsumber"), ""), sptField,
                     FxDB(dr("spautonotransaksi"), 0), sptField,
                     FxDB(dr("spnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sptgl"), ""), formatTgl), sptField,
                     FxDB(dr("spkodepa"), 0), sptField,
                     FxDB(dr("spbagiansp"), 0), sptField,
                     FxDB(dr("spbagianspkontak"), ""), sptField,
                     FxDB(dr("spuraian"), ""), sptField,
                     FxDB(dr("spcatatan"), ""), sptField,
                     FxDB(dr("spnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sptglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("spstatussa"), 0), sptField,
                     FxDB(dr("spstatus"), 0), sptField,
                     FxDB(dr("spstatussebelumnya"), 0), sptField,
                     FxDB(dr("spjmlrevisi"), 0), sptField,
                     FxDB(dr("spcetakanke"), 0), sptField,
                     FxDB(dr("spinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("spinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("spmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("spmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("spposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sppostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sptutupperiode"), 0), sptField,
                     FxDB(dr("spisclose"), 0), sptField,
                     FxDB(dr("spcabangnama"), ""), sptField,
                     FxDB(dr("splokasinama"), ""), sptField,
                     FxDB(dr("spgudangnama"), ""), sptField,
                     FxDB(dr("spbagianspkode"), ""), sptField,
                     FxDB(dr("spbagianspnama"), ""), sptField,
                     FxDB(dr("spstatusnama"), ""), sptField,
                     FxDB(dr("spstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("spinputusernama"), ""), sptField,
                     FxDB(dr("spmodifikasiusernama"), ""), sptField,
                     FxDB(dr("spstepke"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("spid, spcabang, splokasi, spgudang, spsumber, spautonotransaksi, spnotransaksi, sptgl, spkodepa, spbagiansp, spbagianspkontak, spuraian, spcatatan, spnoref, sptglnoref, spstatussa, spstatus, spstatussebelumnya, spjmlrevisi, spcetakanke, spinputuser, spinputtgl, spmodifikasiuser, spmodifikasitgl, spposting, sppostingtgl, sptutupperiode, spisclose, spcabangnama, splokasinama, spgudangnama, spbagianspkode, spbagianspnama, spstatusnama, spstatussebelumnyanama, spinputusernama, spmodifikasiusernama, spstepke"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_Sp_Detail_VSearch(ByVal param As String) As String
        'M3_Sp_Detail_VSearch --------------------------------------------------------
        'idspdetail, idsp, idbarang, namabarang, tipebarang, jmlsistem, jmlfisik, 
        'jmlbagus, jmlrusak, selisih, satuan, nilaisatuan, jmlbarangsistem, jmlbarangfisik, 
        'jmlbarangbagus, jmlbarangrusak, selisihbarang, satuanbarang, cabang, lokasi, gudang, 
        'lokasibarang, jmlsa, statussa, costcenter, divisi, subdivisi, proyek, 
        'catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3, spnotransaksi, kodebarang, 
        'bhpp, bhppaverage, bjenis, bserial, bbatch, brekpersediaan, jenisselisih, 
        'selisihsisasa

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
            Filter = Filter.Replace("selisihsisasa", "abs(((spd.selisihbarang - spd.jmlsa) / spd.nilaisatuan))")
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m3_sp_detail_v")
        sql = "select `spd`.`idspdetail` AS `idspdetail`,`spd`.`idsp` AS `idsp`,`spd`.`idbarang` AS `idbarang`,`spd`.`namabarang` AS `namabarang`,`spd`.`tipebarang` AS `tipebarang`,`spd`.`jmlsistem` AS `jmlsistem`,`spd`.`jmlfisik` AS `jmlfisik`,`spd`.`jmlbagus` AS `jmlbagus`,`spd`.`jmlrusak` AS `jmlrusak`,`spd`.`selisih` AS `selisih`,`spd`.`satuan` AS `satuan`,`spd`.`nilaisatuan` AS `nilaisatuan`,`spd`.`jmlbarangsistem` AS `jmlbarangsistem`,`spd`.`jmlbarangfisik` AS `jmlbarangfisik`,`spd`.`jmlbarangbagus` AS `jmlbarangbagus`,`spd`.`jmlbarangrusak` AS `jmlbarangrusak`,`spd`.`selisihbarang` AS `selisihbarang`,`spd`.`satuanbarang` AS `satuanbarang`,`spd`.`cabang` AS `cabang`,`spd`.`lokasi` AS `lokasi`,`spd`.`gudang` AS `gudang`,`spd`.`lokasibarang` AS `lokasibarang`,`spd`.`jmlsa` AS `jmlsa`,`spd`.`statussa` AS `statussa`,`spd`.`costcenter` AS `costcenter`,`spd`.`divisi` AS `divisi`,`spd`.`subdivisi` AS `subdivisi`,`spd`.`proyek` AS `proyek`,`spd`.`catatan` AS `catatan`,`spd`.`urutan` AS `urutan`,`spd`.`isclose` AS `isclose`,`spd`.`customtext1` AS `customtext1`,`spd`.`customtext2` AS `customtext2`,`spd`.`customtext3` AS `customtext3`,`spd`.`customdbl1` AS `customdbl1`,`spd`.`customdbl2` AS `customdbl2`,`spd`.`customdbl3` AS `customdbl3`,`spd`.`customdate1` AS `customdate1`,`spd`.`customdate2` AS `customdate2`,`spd`.`customdate3` AS `customdate3`,`sp`.`spnotransaksi` AS `spnotransaksi`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bhppaverage` AS `bhppaverage`,`i`.`bjenis` AS `bjenis`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`i`.`brekpersediaan` AS `brekpersediaan`,if((`spd`.`selisihbarang` < 0),0,1) AS `jenisselisih`,abs(((`spd`.`selisihbarang` - `spd`.`jmlsa`) / `spd`.`nilaisatuan`)) AS `selisihsisasa` from ((`m3_sp_detail` `spd` left join `m3_sp` `sp` on((`spd`.`idsp` = `sp`.`spid`))) left join `m1_item` `i` on((`spd`.`idbarang` = `i`.`bid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M3_Sp", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idspdetail"), 0), sptField,
                     FxDB(dr("idsp"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jmlsistem"), 0), sptField,
                     FxDB(dr("jmlfisik"), 0), sptField,
                     FxDB(dr("jmlbagus"), 0), sptField,
                     FxDB(dr("jmlrusak"), 0), sptField,
                     FxDB(dr("selisih"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarangsistem"), 0), sptField,
                     FxDB(dr("jmlbarangfisik"), 0), sptField,
                     FxDB(dr("jmlbarangbagus"), 0), sptField,
                     FxDB(dr("jmlbarangrusak"), 0), sptField,
                     FxDB(dr("selisihbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("lokasibarang"), ""), sptField,
                     FxDB(dr("jmlsa"), 0), sptField,
                     FxDB(dr("statussa"), 0), sptField,
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
                     FxDB(dr("spnotransaksi"), ""), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bhppaverage"), 0), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("brekpersediaan"), ""), sptField,
                     FxDB(dr("jenisselisih"), 0), sptField,
                     FxDB(dr("selisihsisasa"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idspdetail, idsp, idbarang, namabarang, tipebarang, jmlsistem, jmlfisik, jmlbagus, jmlrusak, selisih, satuan, nilaisatuan, jmlbarangsistem, jmlbarangfisik, jmlbarangbagus, jmlbarangrusak, selisihbarang, satuanbarang, cabang, lokasi, gudang, lokasibarang, jmlsa, statussa, costcenter, divisi, subdivisi, proyek, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, spnotransaksi, kodebarang, bhpp, bhppaverage, bjenis, bserial, bbatch, brekpersediaan, jenisselisih, selisihsisasa"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_Sp_Detail_VSearchPenjualan(ByVal param As String) As String
        'M3_Sp_Detail_VSearch --------------------------------------------------------
        'idspdetail, idsp, idbarang, namabarang, tipebarang, jmlsistem, jmlfisik, 
        'jmlbagus, jmlrusak, selisih, satuan, nilaisatuan, jmlbarangsistem, jmlbarangfisik, 
        'jmlbarangbagus, jmlbarangrusak, selisihbarang, satuanbarang, cabang, lokasi, gudang, 
        'lokasibarang, jmlsa, statussa, costcenter, divisi, subdivisi, proyek, 
        'catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3, spnotransaksi, kodebarang, 
        'bhpp, bhppaverage, bjenis, bserial, bbatch, brekpersediaan, jenisselisih, 
        'selisihsisasa

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
            'Filter = Filter.Replace("selisihsisasa", "abs(((spd.selisihbarang - spd.jmlsa) / spd.nilaisatuan))")
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m3_sp_detail_v")
        sql = "select spd.idspdetail AS idspdetail, spd.idsp AS idsp, spd.idbarang AS idbarang, spd.namabarang AS namabarang, spd.tipebarang AS tipebarang, spd.jmlsistem AS jmlsistem, spd.jmlfisik AS jmlfisik, spd.jmlbagus AS jmlbagus, spd.jmlrusak AS jmlrusak, SUM((CASE LENGTH(IFNULL(si.siid,'')) WHEN 0 THEN 0 ELSE sid.jmlbarang END)) / spd.nilaisatuan as jmljual, (spd.jmlbarangsistem - SUM((CASE LENGTH(IFNULL(si.siid,'')) WHEN 0 THEN 0 ELSE sid.jmlbarang END)) - spd.jmlbarangfisik) / spd.nilaisatuan AS selisih, spd.satuan AS satuan, spd.nilaisatuan AS nilaisatuan, spd.jmlbarangsistem AS jmlbarangsistem, spd.jmlbarangfisik AS jmlbarangfisik, spd.jmlbarangbagus AS jmlbarangbagus, spd.jmlbarangrusak AS jmlbarangrusak, SUM((CASE LENGTH(IFNULL(si.siid,'')) WHEN 0 THEN 0 ELSE sid.jmlbarang END))as jmlbarangjual, (spd.jmlbarangsistem - SUM((CASE LENGTH(IFNULL(si.siid,'')) WHEN 0 THEN 0 ELSE sid.jmlbarang END)) - spd.jmlbarangfisik) AS selisihbarang, spd.satuanbarang AS satuanbarang, spd.cabang AS cabang, spd.lokasi AS lokasi, spd.gudang AS gudang, spd.lokasibarang AS lokasibarang, spd.jmlsa AS jmlsa, spd.statussa AS statussa, spd.costcenter AS costcenter, spd.divisi AS divisi, spd.subdivisi AS subdivisi, spd.proyek AS proyek, spd.catatan AS catatan, spd.urutan AS urutan, spd.isclose AS isclose, spd.customtext1 AS customtext1, spd.customtext2 AS customtext2, spd.customtext3 AS customtext3, spd.customdbl1 AS customdbl1, spd.customdbl2 AS customdbl2, spd.customdbl3 AS customdbl3, spd.customdate1 AS customdate1, spd.customdate2 AS customdate2, spd.customdate3 AS customdate3, sp.spnotransaksi AS spnotransaksi, i.bkode AS kodebarang, i.bhpp AS bhpp, i.bhppaverage AS bhppaverage, i.bjenis AS bjenis, i.bserial AS bserial, i.bbatch AS bbatch, i.brekpersediaan AS brekpersediaan, IF(((spd.jmlbarangsistem - SUM((CASE LENGTH(IFNULL(si.siid,'')) WHEN 0 THEN 0 ELSE sid.jmlbarang END)) - spd.jmlbarangfisik) < 0), 0, 1) AS jenisselisih, ABS((((spd.jmlbarangsistem - SUM((CASE LENGTH(IFNULL(si.siid,'')) WHEN 0 THEN 0 ELSE sid.jmlbarang END)) - spd.jmlbarangfisik) - spd.jmlsa) / spd.nilaisatuan)) AS selisihsisasa FROM m3_sp_detail spd JOIN m3_sp sp ON spd.idsp = sp.spid JOIN m1_item i ON spd.idbarang = i.bid LEFT JOIN m5_si_detail sid ON spd.idbarang = sid.idbarang LEFT JOIN m5_si si ON sid.idsi = si.siid AND sp.sptgl = si.sitgl AND sp.spgudang = si.sigudang AND si.sistatus IN(2,3,4,7)"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M3_Sp", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "spd.idspdetail HAVING selisihbarang <> 0", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idspdetail"), 0), sptField,
                     FxDB(dr("idsp"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jmlsistem"), 0), sptField,
                     FxDB(dr("jmlfisik"), 0), sptField,
                     FxDB(dr("jmlbagus"), 0), sptField,
                     FxDB(dr("jmlrusak"), 0), sptField,
                     FxDB(dr("selisih"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarangsistem"), 0), sptField,
                     FxDB(dr("jmlbarangfisik"), 0), sptField,
                     FxDB(dr("jmlbarangbagus"), 0), sptField,
                     FxDB(dr("jmlbarangrusak"), 0), sptField,
                     FxDB(dr("selisihbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("lokasibarang"), ""), sptField,
                     FxDB(dr("jmlsa"), 0), sptField,
                     FxDB(dr("statussa"), 0), sptField,
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
                     FxDB(dr("spnotransaksi"), ""), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bhppaverage"), 0), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("brekpersediaan"), ""), sptField,
                     FxDB(dr("jenisselisih"), 0), sptField,
                     FxDB(dr("selisihsisasa"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idspdetail, idsp, idbarang, namabarang, tipebarang, jmlsistem, jmlfisik, jmlbagus, jmlrusak, selisih, satuan, nilaisatuan, jmlbarangsistem, jmlbarangfisik, jmlbarangbagus, jmlbarangrusak, selisihbarang, satuanbarang, cabang, lokasi, gudang, lokasibarang, jmlsa, statussa, costcenter, divisi, subdivisi, proyek, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, spnotransaksi, kodebarang, bhpp, bhppaverage, bjenis, bserial, bbatch, brekpersediaan, jenisselisih, selisihsisasa"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_Sp_TakedataSearch(ByVal param As String) As String
        'M3_Sp_Takedatasearch --------------------------------------------------------
        'bid, bkode, btipe, bnama, bsatuan, bnilaisatuan, kgudang, 
        'blggudangnama, blgidlokasi, blgkodelokasi, blgnamalokasi, stok, bkategori, bkategorinama, 
        'bhpp, bhppaverage, brekpersediaan

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
            formatTglWaktu = "yyy-MM-dd H:mm:ss"
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
        'sql = query.PanggilQuery("m3_take_data")
        sql = "select `b`.`bid` AS `bid`,`b`.`bkode` AS `bkode`,`b`.`btipe` AS `btipe`,`b`.`bnama` AS `bnama`,`b`.`bsatuan` AS `bsatuan`,`b`.`bnilaisatuan` AS `bnilaisatuan`,`bp`.`kgudang` AS `kgudang`,`w`.`wnama` AS `blggudangnama`,`blg`.`blgidlokasi` AS `blgidlokasi`,`blg`.`blgkodelokasi` AS `blgkodelokasi`,`blg`.`blgnamalokasi` AS `blgnamalokasi`,`bp`.`stok` AS `stok`,`b`.`bkategori` AS `bkategori`,`bk`.`icnama` AS `bkategorinama`,`b`.`bhpp` AS `bhpp`,`b`.`bhppaverage` AS `bhppaverage`,`b`.`brekpersediaan` AS `brekpersediaan`, b.bbarcode from ((((`m1_item` `b` left join `m1_item_stock_warehouse` `bp` on((`b`.`bid` = `bp`.`idbarang`))) left join `m1_item_location_warehouse` `blg` on(((`b`.`bid` = `blg`.`blgidbarang`) and (`bp`.`kgudang` = `blg`.`blggudang`)))) left join `m1_warehouse` `w` on((`w`.`wkode` = `bp`.`kgudang`))) left join `m1_item_category` `bk` on((`bk`.`ickode` = `b`.`bkategori`)))"
		sql = "select `b`.`bid` AS `bid`,`b`.`bkode` AS `bkode`,`b`.`btipe` AS `btipe`,`b`.`bnama` AS `bnama`,`b`.`bsatuan` AS `bsatuan`,`b`.`bnilaisatuan` AS `bnilaisatuan`,w.wkode AS `kgudang`,`w`.`wnama` AS `blggudangnama`,`blg`.`blgidlokasi` AS `blgidlokasi`,`blg`.`blgkodelokasi` AS `blgkodelokasi`,`blg`.`blgnamalokasi` AS `blgnamalokasi`,IFNULL(`bp`.`stok`,0) AS `stok`,`b`.`bkategori` AS `bkategori`,`bk`.`icnama` AS `bkategorinama`,`b`.`bhpp` AS `bhpp`,`b`.`bhppaverage` AS `bhppaverage`,`b`.`brekpersediaan` AS `brekpersediaan`, b.bbarcode from `m1_item` `b` join `m1_warehouse` `w` left join `m1_item_stock_warehouse` `bp` on `b`.`bid` = `bp`.`idbarang` and w.wkode = bp.kgudang left join `m1_item_location_warehouse` `blg` on `b`.`bid` = `blg`.`blgidbarang` and w.wkode = `blg`.`blggudang` left join `m1_item_category` `bk` on `bk`.`ickode` = `b`.`bkategori`"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M3_Sp_Takedata", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("bid"), 0), sptField,
                     FxDB(dr("bkode"), ""), sptField,
                     FxDB(dr("btipe"), ""), sptField,
                     FxDB(dr("bnama"), ""), sptField,
                     FxDB(dr("bsatuan"), ""), sptField,
                     FxDB(dr("bnilaisatuan"), 0), sptField,
                     FxDB(dr("kgudang"), ""), sptField,
                     FxDB(dr("blggudangnama"), ""), sptField,
                     FxDB(dr("blgidlokasi"), 0), sptField,
                     FxDB(dr("blgkodelokasi"), ""), sptField,
                     FxDB(dr("blgnamalokasi"), ""), sptField,
                     FxDB(dr("stok"), 0), sptField,
                     FxDB(dr("bkategori"), ""), sptField,
                     FxDB(dr("bkategorinama"), ""), sptField,
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bhppaverage"), 0), sptField,
                     FxDB(dr("brekpersediaan"), ""), sptField,
                     FxDB(dr("bbarcode"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bid, bkode, btipe, bnama, bsatuan, bnilaisatuan, kgudang, blggudangnama, blgidlokasi, blgkodelokasi, blgnamalokasi, stok, bkategori, bkategorinama, bhpp, bhppaverage, brekpersediaan, bbarcode"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_SpTerkait(ByVal param As String) As String
        'M3_SpTerkait --------------------------------------------------------
        'spid, spnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "spid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2) & " AND spid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "spid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.m3_sp_terkait(Filter)


        dt = AmbilData("aplikasi1-M3_sp_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("spid"), 0), sptField,
                     FxDB(dr("spnotransaksi"), ""), sptField,
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
            result(2) = "Related SP data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("spid, spnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_Sp_ProgressSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean

        Dim tahapke As String = "", idtransaksi As String = ""

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


        'MAPPING BUAT WS ----------------------------------------------------------
        'tahapke(0) As String, idtransaksi(1) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'tahapke, idtransaksi


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = paramSplit(5).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 2) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI DATA UTAMA ===============================================================
        'tahapke(0) As String
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "tahapke  required numeric." : GoTo selesai
        Else
            tahapke = dataUtama(0)
        End If

        'idtransaksi(1) As Integer
        If (IsNumeric(dataUtama(1)) = False) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
        Else
            idtransaksi = dataUtama(1)
        End If
        'END OF VALIDASI DATA UTAMA ========================================================


        'SIMPAN KE DATABASE ================================================================
        Con2 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con2.Open()

        '*** Start Transaction ***'  
        Trans = Con2.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'PROSES HAPUS PROGRESS TAHAP DAN IDTRANSAKSI YANG SAMA --------------
            'sql = "DELETE spd FROM m3_sp_detail_progress spd JOIN m3_sp_progress sp ON spd.idprogress = sp.spidprogress WHERE sp.spid = '" & idtransaksi & "' AND sp.spstepke = '" & tahapke & "'"
            sql = "DELETE spd, sp FROM m3_sp_detail_progress spd JOIN m3_sp_progress sp ON spd.idprogress = sp.spidprogress WHERE sp.spid = '" & idtransaksi & "' AND sp.spstepke = '" & tahapke & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            ''UTAMA
            'sql = "DELETE sp FROM m3_sp_progress sp WHERE sp.spid = '" & idtransaksi & "' AND sp.spstepke = '" & tahapke & "'"
            'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            'With objCmd
            '    .Connection = Con2
            '    .Transaction = Trans
            '    .CommandType = CommandType.Text
            '    .CommandText = sql
            'End With
            'objCmd.ExecuteNonQuery()
            'END OF PROSES HAPUS PROGRESS TAHAP DAN IDTRANSAKSI YANG SAMA -------


            'PROSES INSERT Progress UTAMA ---------------------------------------
            sql = "INSERT INTO m3_sp_progress(SELECT 0, sp.* FROM m3_sp sp WHERE sp.spid = '" & idtransaksi & "')"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT Progress UTAMA --------------------------------


            'PROSES AMBIL ID Progress YANG BARUSAJA DIINSERT --------------------
            Dim dt2 As New DataTable
            sql = "SELECT spidprogress FROM m3_sp_progress WHERE spid = '" & idtransaksi & "' ORDER BY spmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Progress main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID Progress YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT Progress DETAIL --------------------------------------
            sql = "INSERT INTO m3_sp_detail_progress (SELECT 0, '" & result(4) & "', sp.* FROM m3_sp_detail sp WHERE sp.idsp = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT Progress DETAIL -------------------------------


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
        'Con2.Close()
        'Con2 = Nothing
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
    Public Function M3_SpSimpanOld(ByVal param As String) As String
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
        Dim Filter As String = "", Sorting As String = ""

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
            sql = "SELECT spid, spnotransaksi FROM m3_sp WHERE spnoref = '" & FixQuotes(Filter) & "'"
            Dim dtNoreff As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNoreff.Rows.Count > 0 Then
                If Len(dtNoreff.Rows(0)("spid")) > 0 Then
                    result(1) = 1
                    result(2) = dtNoreff.Rows(0)("spnotransaksi")
                    result(3) = 0
                    result(4) = dtNoreff.Rows(0)("spid")
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
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'spid(0) As Integer, spcabang(1) As String, splokasi(2) As String, spgudang(3) As String, spsumber(4) As String, 
        'spautonotransaksi(5) As Integer, spnotransaksi(6) As String, sptgl(7) As Date, spkodepa(8) As Integer, spbagiansp(9) As Integer, 
        'spbagianspkontak(10) As String, spuraian(11) As String, spcatatan(12) As String, spnoref(13) As String, sptglnoref(14) As Date, 
        'spstatussa(15) As Integer, spstatus(16) As Integer, spstatussebelumnya(17) As Integer, spjmlrevisi(18) As Integer, spcetakanke(19) As Integer, 
        'spinputuser(20) As Integer, spinputtgl(21) As DateTime, spmodifikasiuser(22) As Integer, spmodifikasitgl(23) As DateTime, spposting(24) As Integer, 
        'sptutupperiode(25) As Integer, spisclose(26) As Integer, spcustomtext1(27) As String, spcustomtext2(28) As String, spcustomtext3(29) As String, 
        'spcustomtext4(30) As String, spcustomtext5(31) As String, spcustomint1(32) As Integer, spcustomint2(33) As Integer, spcustomint3(34) As Integer, 
        'spcustomdbl1(35) As Double, spcustomdbl2(36) As Double, spcustomdbl3(37) As Double, spcustomdate1(38) As Date, spcustomdate2(39) As Date, 
        'spcustomdate3(40) As Date, spstepke(41) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'spid, spcabang, splokasi, spgudang, spsumber, spautonotransaksi, spnotransaksi, 
        'sptgl, spkodepa, spbagiansp, spbagianspkontak, spuraian, spcatatan, spnoref, 
        'sptglnoref, spstatussa, spstatus, spstatussebelumnya, spjmlrevisi, spcetakanke, spinputuser, 
        'spinputtgl, spmodifikasiuser, spmodifikasitgl, spposting, sptutupperiode, spisclose, spcustomtext1, 
        'spcustomtext2, spcustomtext3, spcustomtext4, spcustomtext5, spcustomint1, spcustomint2, spcustomint3, 
        'spcustomdbl1, spcustomdbl2, spcustomdbl3, spcustomdate1, spcustomdate2, spcustomdate3, spstepke

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 42) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'spid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "spid required numeric." : GoTo selesai
        End If
        'spautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "spautonotransaksi required numeric." : GoTo selesai
        End If
        'sptgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "sptgl required date." : GoTo selesai
        End If
        'spkodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "spkodepa required numeric." : GoTo selesai
        End If
        'spbagiansp(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "spbagiansp required numeric." : GoTo selesai
        End If
        If (dataUtama(9) < 1) Then
            result(2) = "spbagiansp can't be empty." : GoTo selesai
        End If
        'sptglnoref(14) As Date
        If (IsDate(dataUtama(14)) = False) Then
            result(2) = "sptglnoref required date." : GoTo selesai
        End If
        'spstatussa(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "spstatussa required numeric." : GoTo selesai
        End If
        'spstatus(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "spstatus required numeric." : GoTo selesai
        End If
        'spstatussebelumnya(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "spstatussebelumnya required numeric." : GoTo selesai
        End If
        'spjmlrevisi(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "spjmlrevisi required numeric." : GoTo selesai
        End If
        'spcetakanke(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "spcetakanke required numeric." : GoTo selesai
        End If
        'spinputuser(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "spinputuser required numeric." : GoTo selesai
        End If
        'spinputtgl(21) As DateTime
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "spinputtgl required date." : GoTo selesai
        End If
        'spmodifikasiuser(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "spmodifikasiuser required numeric." : GoTo selesai
        End If
        'spmodifikasitgl(23) As DateTime
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "spmodifikasitgl required date." : GoTo selesai
        End If
        'spposting(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "spposting required numeric." : GoTo selesai
        End If
        'sptutupperiode(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "sptutupperiode required numeric." : GoTo selesai
        End If
        'spisclose(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "spisclose required numeric." : GoTo selesai
        End If
        'spcustomint1(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "spcustomint1 required numeric." : GoTo selesai
        End If
        'spcustomint2(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "spcustomint2 required numeric." : GoTo selesai
        End If
        'spcustomint3(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "spcustomint3 required numeric." : GoTo selesai
        End If
        'spcustomdbl1(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "spcustomdbl1 required numeric." : GoTo selesai
        End If
        'spcustomdbl2(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "spcustomdbl2 required numeric." : GoTo selesai
        End If
        'spcustomdbl3(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "spcustomdbl3 required numeric." : GoTo selesai
        End If
        'spcustomdate1(38) As Date
        If (IsDate(dataUtama(38)) = False) Then
            result(2) = "spcustomdate1 required date." : GoTo selesai
        End If
        'spcustomdate2(39) As Date
        If (IsDate(dataUtama(39)) = False) Then
            result(2) = "spcustomdate2 required date." : GoTo selesai
        End If
        'spcustomdate3(40) As Date
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "spcustomdate3 required date." : GoTo selesai
        End If
        'spstepke(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "spstepke required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'spcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "spcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "spcabang should not be more than 25 character." : GoTo selesai
        End If

        'splokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "splokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "splokasi should not be more than 25 character." : GoTo selesai
        End If

        'spsumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "spsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "spsumber should not be more than 10 character." : GoTo selesai
        End If

        'spnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "spnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "spnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'sptgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "sptgl can't be empty" : GoTo selesai
        End If

        'sptglnoref(14) As Date
        If Len(dataUtama(14)) = 0 Then
            result(2) = "sptglnoref can't be empty" : GoTo selesai
        End If

        'spinputtgl(21) As DateTime
        If Len(dataUtama(21)) = 0 Then
            result(2) = "spinputtgl can't be empty" : GoTo selesai
        End If

        'spmodifikasitgl(23) As DateTime
        If Len(dataUtama(23)) = 0 Then
            result(2) = "spmodifikasitgl can't be empty" : GoTo selesai
        End If

        'spcustomdbl1(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "spcustomdbl1 can't be empty" : GoTo selesai
        End If

        'spcustomdbl2(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "spcustomdbl2 can't be empty" : GoTo selesai
        End If

        'spcustomdbl3(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "spcustomdbl3 can't be empty" : GoTo selesai
        End If

        'spcustomdate1(38) As Date
        If Len(dataUtama(38)) = 0 Then
            result(2) = "spcustomdate1 can't be empty" : GoTo selesai
        End If

        'spcustomdate2(39) As Date
        If Len(dataUtama(39)) = 0 Then
            result(2) = "spcustomdate2 can't be empty" : GoTo selesai
        End If

        'spcustomdate3(40) As Date
        If Len(dataUtama(40)) = 0 Then
            result(2) = "spcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "spid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "splokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sptgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spbagiansp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spbagianspkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sptglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spstatussa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sptutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spstepke", AsEnumTypeData.AsInt64)
        If AsDataTableTambahData(dtutama, "spid~spcabang~splokasi~spgudang~spsumber~spautonotransaksi~spnotransaksi~sptgl~spkodepa~spbagiansp~spbagianspkontak~spuraian~spcatatan~spnoref~sptglnoref~spstatussa~spstatus~spstatussebelumnya~spjmlrevisi~spcetakanke~spinputuser~spinputtgl~spmodifikasiuser~spmodifikasitgl~spposting~sptutupperiode~spisclose~spcustomtext1~spcustomtext2~spcustomtext3~spcustomtext4~spcustomtext5~spcustomint1~spcustomint2~spcustomint3~spcustomdbl1~spcustomdbl2~spcustomdbl3~spcustomdate1~spcustomdate2~spcustomdate3~spstepke", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idspdetail(0) As Integer, idsp(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jmlsistem(5) As Double, jmlfisik(6) As Double, jmlbagus(7) As Double, jmlrusak(8) As Double, selisih(9) As Double, 
        'satuan(10) As String, nilaisatuan(11) As Double, jmlbarangsistem(12) As Double, jmlbarangfisik(13) As Double, jmlbarangbagus(14) As Double, 
        'jmlbarangrusak(15) As Double, selisihbarang(16) As Double, satuanbarang(17) As String, cabang(18) As String, lokasi(19) As String, 
        'gudang(20) As String, lokasibarang(21) As String, jmlsa(22) As Double, statussa(23) As Integer, costcenter(24) As String, 
        'divisi(25) As String, subdivisi(26) As String, proyek(27) As String, catatan(28) As String, urutan(29) As Integer, 
        'isclose(30) As Integer, customtext1(31) As String, customtext2(32) As String, customtext3(33) As String, customdbl1(34) As Double, 
        'customdbl2(35) As Double, customdbl3(36) As Double, customdate1(37) As Date, customdate2(38) As Date, customdate3(39) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idspdetail, idsp, idbarang, namabarang, tipebarang, jmlsistem, jmlfisik, 
        'jmlbagus, jmlrusak, selisih, satuan, nilaisatuan, jmlbarangsistem, jmlbarangfisik, 
        'jmlbarangbagus, jmlbarangrusak, selisihbarang, satuanbarang, cabang, lokasi, gudang, 
        'lokasibarang, jmlsa, statussa, costcenter, divisi, subdivisi, proyek, 
        'catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idspdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idsp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlsistem", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlfisik", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbagus", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlrusak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "selisih", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarangsistem", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarangfisik", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarangbagus", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarangrusak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "selisihbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasibarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlsa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statussa", AsEnumTypeData.AsInt64)
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

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 40) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idspdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idspdetail required numeric." : GoTo selesai
            End If
            'idsp(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idsp required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'jmlsistem(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - jmlsistem required numeric." : GoTo selesai
            End If
            'jmlfisik(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - jmlfisik required numeric." : GoTo selesai
            End If
            'jmlbagus(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - jmlbagus required numeric." : GoTo selesai
            End If
            'jmlrusak(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - jmlrusak required numeric." : GoTo selesai
            End If
            'selisih(9) As Double
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - selisih required numeric." : GoTo selesai
            End If
            'nilaisatuan(11) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarangsistem(12) As Double
            'jmlbarangsistem = jmlsistem * nilaisatuan
            dataRowDetail(12) = Double.Parse(dataRowDetail(5)) * Double.Parse(dataRowDetail(11))
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - jmlbarangsistem required numeric." : GoTo selesai
            End If
            'jmlbarangfisik(13) As Double
            'jmlbarangfisik = jmlfisik * nilaisatuan
            dataRowDetail(13) = Double.Parse(dataRowDetail(6)) * Double.Parse(dataRowDetail(11))
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - jmlbarangfisik required numeric." : GoTo selesai
            End If
            'jmlbarangbagus(14) As Double
            'jmlbarangbagus = jmlbagus * nilaisatuan
            dataRowDetail(14) = Double.Parse(dataRowDetail(7)) * Double.Parse(dataRowDetail(11))
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - jmlbarangbagus required numeric." : GoTo selesai
            End If
            'jmlbarangrusak(15) As Double
            'jmlbarangrusak = jmlrusak * nilaisatuan
            dataRowDetail(15) = Double.Parse(dataRowDetail(8)) * Double.Parse(dataRowDetail(11))
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - jmlbarangrusak required numeric." : GoTo selesai
            End If
            'selisihbarang(16) As Double
            'selisihbarang = selisih * nilaisatuan
            dataRowDetail(16) = Double.Parse(dataRowDetail(9)) * Double.Parse(dataRowDetail(11))
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - selisihbarang required numeric." : GoTo selesai
            End If
            'jmlsa(22) As Double
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - jmlsa required numeric." : GoTo selesai
            End If
            'statussa(23) As Integer
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - statussa required numeric." : GoTo selesai
            End If
            'urutan(29) As Integer
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'isclose(30) As Integer
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(34) As Double
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(35) As Double
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(36) As Double
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(37) As Date
            If (IsDate(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(38) As Date
            If (IsDate(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(39) As Date
            If (IsDate(dataRowDetail(39)) = False) Then
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

            'jmlsistem(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - jmlsistem can't be empty" : GoTo selesai
            End If

            'jmlfisik(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - jmlfisik can't be empty" : GoTo selesai
            End If

            'jmlbagus(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - jmlbagus can't be empty" : GoTo selesai
            End If

            'jmlrusak(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - jmlrusak can't be empty" : GoTo selesai
            End If

            'selisih(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - selisih can't be empty" : GoTo selesai
            End If

            'satuan(10) As String
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(10)) > 25 Then
                result(2) = "Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'nilaisatuan(11) As Double
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmlbarangsistem(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarangsistem can't be empty" : GoTo selesai
            End If

            'jmlbarangfisik(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarangfisik can't be empty" : GoTo selesai
            End If

            'jmlbarangbagus(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarangbagus can't be empty" : GoTo selesai
            End If

            'jmlbarangrusak(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarangrusak can't be empty" : GoTo selesai
            End If

            'selisihbarang(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - selisihbarang can't be empty" : GoTo selesai
            End If

            'satuanbarang(17) As String
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(17)) > 25 Then
                result(2) = "Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
            End If

            'lokasibarang(21) As String
            'If Len(dataRowDetail(21)) = 0 Then
            '    result(2) = "Row : " & i & " - lokasibarang can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(21)) > 25 Then
                result(2) = "Row : " & i & " - lokasibarang should not be more than 25 character." : GoTo selesai
            End If

            'jmlsa(22) As Double
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - jmlsa can't be empty" : GoTo selesai
            End If

            'customdbl1(34) As Double
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(35) As Double
            If Len(dataRowDetail(35)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(36) As Double
            If Len(dataRowDetail(36)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(37) As Date
            If Len(dataRowDetail(37)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(38) As Date
            If Len(dataRowDetail(38)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(39) As Date
            If Len(dataRowDetail(39)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idspdetail~idsp~idbarang~namabarang~tipebarang~jmlsistem~jmlfisik~jmlbagus~jmlrusak~selisih~satuan~nilaisatuan~jmlbarangsistem~jmlbarangfisik~jmlbarangbagus~jmlbarangrusak~selisihbarang~satuanbarang~cabang~lokasi~gudang~lokasibarang~jmlsa~statussa~costcenter~divisi~subdivisi~proyek~catatan~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39)) = False Then
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("sptgl")), AsFormatTanggal(drutama("sptgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================

                If isUpdate Then
                    result(4) = drutama("spid")
                    notransaksi = drutama("spnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(spid), spnotransaksi FROM M3_Sp WHERE spid='" & result(4) & "' AND spstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(spid) FROM m3_sp WHERE spnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m3_sp_history
                        Dim rsSimpanHistory As String = SimpanHistory.M3_Sp_HistorySimpan("" & paramSplit(0) & "★M3_Sp_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("spsumber")) & "▼" & FixQuotes(drutama("spid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M3_Sp set spcabang  = '" & FixQuotes(drutama("spcabang")) & "', splokasi  = '" & FixQuotes(drutama("splokasi")) & "', spgudang  = '" & FixQuotes(drutama("spgudang")) & "', spsumber  = '" & FixQuotes(drutama("spsumber")) & "', spautonotransaksi  = " & drutama("spautonotransaksi") & ", spnotransaksi  = '" & notransaksi & "', sptgl  = '" & FixQuotes(AsFormatTanggal(drutama("sptgl"))) & "', spkodepa  = " & drutama("spkodepa") & ", spbagiansp  = " & drutama("spbagiansp") & ", spbagianspkontak  = '" & FixQuotes(drutama("spbagianspkontak")) & "', spuraian  = '" & FixQuotes(drutama("spuraian")) & "', spcatatan  = '" & FixQuotes(drutama("spcatatan")) & "', spnoref  = '" & FixQuotes(drutama("spnoref")) & "', sptglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("sptglnoref"))) & "', spstatussa  = " & drutama("spstatussa") & ", spstatus  = " & drutama("spstatus") & ", spstatussebelumnya  = " & drutama("spstatussebelumnya") & ", spjmlrevisi  = spjmlrevisi+1, spcetakanke  = " & drutama("spcetakanke") & ", spmodifikasiuser  = " & drutama("spmodifikasiuser") & ", spmodifikasitgl  = NOW(), spposting  = 0, sptutupperiode  = " & drutama("sptutupperiode") & ", spcustomtext1  = '" & FixQuotes(drutama("spcustomtext1")) & "', spcustomtext2  = '" & FixQuotes(drutama("spcustomtext2")) & "', spcustomtext3  = '" & FixQuotes(drutama("spcustomtext3")) & "', spcustomtext4  = '" & FixQuotes(drutama("spcustomtext4")) & "', spcustomtext5  = '" & FixQuotes(drutama("spcustomtext5")) & "', spcustomint1  = " & drutama("spcustomint1") & ", spcustomint2  = " & drutama("spcustomint2") & ", spcustomint3  = " & drutama("spcustomint3") & ", spcustomdbl1  = '" & FixDouble(drutama("spcustomdbl1")) & "', spcustomdbl2  = '" & FixDouble(drutama("spcustomdbl2")) & "', spcustomdbl3  = '" & FixDouble(drutama("spcustomdbl3")) & "', spcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("spcustomdate1"))) & "', spcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("spcustomdate2"))) & "', spcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("spcustomdate3"))) & "', spstepke = '" & drutama("spstepke") & "' where spid = '" & drutama("spid") & "'"
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

                    If drutama("spautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("spcabang"), drutama("splokasi"), drutama("spsumber"), drutama("sptgl"))
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
                        notransaksi = drutama("spnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(spid) FROM m3_sp WHERE spnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M3_Sp (spcabang, splokasi, spgudang, spsumber, spautonotransaksi, spnotransaksi, sptgl, spkodepa, spbagiansp, spbagianspkontak, spuraian, spcatatan, spnoref, sptglnoref, spstatussa, spstatus, spstatussebelumnya, spjmlrevisi, spcetakanke, spinputuser, spinputtgl, spmodifikasiuser, spmodifikasitgl, spposting, sptutupperiode, spisclose, spcustomtext1, spcustomtext2, spcustomtext3, spcustomtext4, spcustomtext5, spcustomint1, spcustomint2, spcustomint3, spcustomdbl1, spcustomdbl2, spcustomdbl3, spcustomdate1, spcustomdate2, spcustomdate3, spstepke) values('" & FixQuotes(drutama("spcabang")) & "', '" & FixQuotes(drutama("splokasi")) & "', '" & FixQuotes(drutama("spgudang")) & "', '" & FixQuotes(drutama("spsumber")) & "', " & drutama("spautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("sptgl"))) & "', " & drutama("spkodepa") & ", " & drutama("spbagiansp") & ", '" & FixQuotes(drutama("spbagianspkontak")) & "', '" & FixQuotes(drutama("spuraian")) & "', '" & FixQuotes(drutama("spcatatan")) & "', '" & FixQuotes(drutama("spnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sptglnoref"))) & "', " & drutama("spstatussa") & ", " & drutama("spstatus") & ", " & drutama("spstatussebelumnya") & ", " & drutama("spjmlrevisi") & ", " & drutama("spcetakanke") & ", " & drutama("spinputuser") & ", NOW(), " & drutama("spmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("sptutupperiode") & ", " & drutama("spisclose") & ", '" & FixQuotes(drutama("spcustomtext1")) & "', '" & FixQuotes(drutama("spcustomtext2")) & "', '" & FixQuotes(drutama("spcustomtext3")) & "', '" & FixQuotes(drutama("spcustomtext4")) & "', '" & FixQuotes(drutama("spcustomtext5")) & "', " & drutama("spcustomint1") & ", " & drutama("spcustomint2") & ", " & drutama("spcustomint3") & ", '" & FixDouble(drutama("spcustomdbl1")) & "', '" & FixDouble(drutama("spcustomdbl2")) & "', '" & FixDouble(drutama("spcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("spcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("spcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("spcustomdate3"))) & "', '" & drutama("spstepke") & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select spid from M3_Sp where spnotransaksi='" & notransaksi & "' AND spinputuser= '" & userid & "' order by spmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main progress transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M3_Sp_Detail where idsp = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idspdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jmlsistem")) & "', '" & FixDouble(dr1("jmlfisik")) & "', '" & FixDouble(dr1("jmlbagus")) & "', '" & FixDouble(dr1("jmlrusak")) & "', '" & FixDouble(dr1("selisih")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarangsistem")) & "', '" & FixDouble(dr1("jmlbarangfisik")) & "', '" & FixDouble(dr1("jmlbarangbagus")) & "', '" & FixDouble(dr1("jmlbarangrusak")) & "', '" & FixDouble(dr1("selisihbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("lokasibarang")) & "', '" & FixDouble(dr1("jmlsa")) & "', " & dr1("statussa") & ", '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M3_Sp_Detail(idspdetail, idsp, idbarang, namabarang, tipebarang, jmlsistem, jmlfisik, jmlbagus, jmlrusak, selisih, satuan, nilaisatuan, jmlbarangsistem, jmlbarangfisik, jmlbarangbagus, jmlbarangrusak, selisihbarang, satuanbarang, cabang, lokasi, gudang, lokasibarang, jmlsa, statussa, costcenter, divisi, subdivisi, proyek, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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


                'INSERT KE TABEL PEMBANTU UNTUK KEPENTINGAN CETAK LAPORAN REKAP MUTASI STOK
                'ADA KONDISI KETIKA BELUM INPUT SP MAKA TIDAK BISA CETAK LAPORAN REKAP MUTASI STOK
                If drutama("spstatus") = 2 Then
                    sql = "INSERT INTO m2r_mutasi_stok_custom(csumber, ccabang, clokasi, cgudang, ctgl, cnotransaksi) VALUES('" & FixQuotes(drutama("spsumber")) & "', '" & FixQuotes(drutama("spcabang")) & "', '" & FixQuotes(drutama("splokasi")) & "', '" & FixQuotes(drutama("spgudang")) & "', '" & AsFormatTanggal(FixQuotes(drutama("sptgl"))) & "', '" & FixQuotes(notransaksi) & "') ON DUPLICATE KEY UPDATE cnotransaksi = VALUES(cnotransaksi)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                ''SIMPAN PROGRESS ========================
                'Dim rsSimpanProgress As String = M3_Sp_ProgressSimpan("" & paramSplit(0) & "★M3_Sp_ProgressSimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("spstepke")) & "▼" & FixQuotes(result(4)) & "")
                'Dim rsSplitProgress() As String = rsSimpanProgress.Split(sptParam)
                'Dim rsSplitResultProgress() As String = rsSplitProgress(0).Split(sptSubParam)
                ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                'If (rsSplitResultProgress(1) = 0) Then
                '    result(2) = "Insert Progress failed : " & rsSplitResultProgress(2) : Trans.Rollback() : GoTo selesai
                'End If
                ''END OF SIMPAN PROGRESS ==================


                'PROSES HAPUS PROGRESS TAHAP DAN IDTRANSAKSI YANG SAMA --------------
                sql = "DELETE spd, sp FROM m3_sp_detail_progress spd JOIN m3_sp_progress sp ON spd.idprogress = sp.spidprogress WHERE sp.spid = '" & FixQuotes(result(4)) & "' AND sp.spstepke = '" & FixQuotes(drutama("spstepke")) & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF PROSES HAPUS PROGRESS TAHAP DAN IDTRANSAKSI YANG SAMA -------


                'PROSES INSERT Progress UTAMA ---------------------------------------
                sql = "INSERT INTO m3_sp_progress(SELECT 0, sp.* FROM m3_sp sp WHERE sp.spid = '" & FixQuotes(result(4)) & "')"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF PROSES INSERT Progress UTAMA --------------------------------


                'PROSES AMBIL ID Progress YANG BARUSAJA DIINSERT --------------------
                Dim dt3 As New DataTable, idProgress As Double = 0
                sql = "SELECT spidprogress FROM m3_sp_progress WHERE spid = '" & FixQuotes(result(4)) & "' ORDER BY spmodifikasitgl DESC LIMIT 1"
                dt3 = AsDataTableAmbilDariDB(sql)
                If dt3.Rows.Count > 0 Then idProgress = dt3.Rows(0)(0) Else result(2) = "Progress main transaction data not found." : Trans.Rollback() : GoTo selesai
                'END OF PROSES AMBIL ID Progress YANG BARUSAJA DIINSERT -------------


                'PROSES INSERT Progress DETAIL --------------------------------------
                sql = "INSERT INTO m3_sp_detail_progress (SELECT 0, '" & idProgress & "', sp.* FROM m3_sp_detail sp WHERE sp.idsp = '" & FixQuotes(result(4)) & "' )"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF PROSES INSERT Progress DETAIL -------------------------------


                'INSERT USER LOG ====================================================================
                Dim sumber As String = "Sp", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M3_SpUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("spbagianspkode", "c1.kkode")
            Filter = Filter.Replace("spbagianspnama", "c1.knama")
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
            Dim sumber As String = "Sp", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Sptgl, Spnotransaksi, Spstatus FROM m3_Sp WHERE Spid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Spstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m3_sp_history
            Dim rsSimpanHistory As String = SimpanHistory.M3_Sp_HistorySimpan("" & paramSplit(0) & "★M3_Sp_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m3_sp_terkait("spid = '" & idtransaksi & "'")
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================
            End If

            'update status utama
            sql = "UPDATE M3_Sp SET Spstatus = " & nilaiStatus & ", Spmodifikasiuser='" & userid & "', Spmodifikasitgl = NOW(), Spposting = 0, Sppostingtgl = '1971-01-01 00:00:00', Spjmlrevisi = Spjmlrevisi + 1 WHERE Spid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M3_SpSearch(PostWsSearch(paramSplit(0), "M3_SpSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M3_SpDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("spbagianspkode", "c1.kkode")
            Filter = Filter.Replace("spbagianspnama", "c1.knama")
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
            Dim sumber As String = "Sp", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Spid, Spnotransaksi FROM M3_Sp WHERE Spid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT spcabang, splokasi, spsumber, spautonotransaksi, spnotransaksi, sptgl"
            sql &= " FROM M3_sp"
            sql &= " WHERE spid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("spcabang")
                lokasi = dtNomorNext.Rows(0)("splokasi")
                sumber = dtNomorNext.Rows(0)("spsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("spautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("spnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("sptgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M3_Sp_Detail WHERE idsp = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M3_Sp WHERE spid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M3_SpSearch(PostWsSearch(paramSplit(0), "M3_SpSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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