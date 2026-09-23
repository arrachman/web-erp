Imports System.Web
Imports System.Web.Services
'Imports System.Web.Services.Protocols
'Imports System.Web.Script.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization

'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m1_production_route
    Inherits System.Web.Services.WebService
    Public ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi

    <WebMethod()>
    Public Function M1_Production_RouteSimpan(ByVal param As String) As String
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
        Dim strRekCostCenter As String = ""

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
        'paid(0) As Integer, pakode(1) As String, panama(2) As String, pacatatan(3) As String, paaktif(4) As Integer, 
        'painputuser(5) As Integer, painputtgl(6) As DateTime, pamodifikasiuser(7) As Integer, pamodifikasitgl(8) As DateTime, pacustomtext1(9) As String, 
        'pacustomtext2(10) As String, pacustomtext3(11) As String, pacustomtext4(12) As String, pacustomtext5(13) As String, pacustomint1(14) As Integer, 
        'pacustomint2(15) As Integer, pacustomint3(16) As Integer, pacustomdbl1(17) As Double, pacustomdbl2(18) As Double, pacustomdbl3(19) As Double, 
        'pacustomdate1(20) As Date, pacustomdate2(21) As Date, pacustomdate3(22) As Date, pagudangbahan(23) As String, pagudanghasil(24) As String

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'paid, pakode, panama, pacatatan, paaktif, painputuser, painputtgl, 
        'pamodifikasiuser, pamodifikasitgl, pacustomtext1, pacustomtext2, pacustomtext3, pacustomtext4, pacustomtext5, 
        'pacustomint1, pacustomint2, pacustomint3, pacustomdbl1, pacustomdbl2, pacustomdbl3, pacustomdate1, 
        'pacustomdate2, pacustomdate3, pagudangbahan, pagudanghasil

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 23) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'paaktif(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "praktif required numeric." : GoTo selesai
        End If
        'painputtgl(6) As DateTime
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "prinputtgl required date." : GoTo selesai
        End If
        'pamodifikasitgl(8) As DateTime
        If (IsDate(dataUtama(8)) = False) Then
            result(2) = "prmodifikasitgl required date." : GoTo selesai
        End If
        'pacustomint1(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "prcustomint1 required numeric." : GoTo selesai
        End If
        'pacustomint2(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "prcustomint2 required numeric." : GoTo selesai
        End If
        'pacustomint3(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "prcustomint3 required numeric." : GoTo selesai
        End If
        'pacustomdbl1(17) As Double
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "prcustomdbl1 required numeric." : GoTo selesai
        End If
        'pacustomdbl2(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "prcustomdbl2 required numeric." : GoTo selesai
        End If
        'pacustomdbl3(19) As Double
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "prcustomdbl3 required numeric." : GoTo selesai
        End If
        'pacustomdate1(20) As Date
        If (IsDate(dataUtama(20)) = False) Then
            result(2) = "prcustomdate1 required date." : GoTo selesai
        End If
        'pacustomdate2(21) As Date
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "prcustomdate2 required date." : GoTo selesai
        End If
        'pacustomdate3(22) As Date
        If (IsDate(dataUtama(22)) = False) Then
            result(2) = "prcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'paid(0) As Integer 
        If Len(dataUtama(0)) = 0 Then
            result(2) = "prid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 20 Then
            result(2) = "prid should not be more than 20 character." : GoTo selesai
        End If

        'pakode(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "prkode can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "prkode should not be more than 25 character." : GoTo selesai
        End If

        'panama(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "prnama can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 100 Then
            result(2) = "prnama should not be more than 100 character." : GoTo selesai
        End If

        'painputuser(5) As Integer
        If Len(dataUtama(5)) = 0 Then
            result(2) = "prinputuser can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 20 Then
            result(2) = "prinputuser should not be more than 20 character." : GoTo selesai
        End If

        'painputtgl(6) As DateTime
        If Len(dataUtama(6)) = 0 Then
            result(2) = "prinputtgl can't be empty" : GoTo selesai
        End If

        'pamodifikasiuser(7) As Integer
        If Len(dataUtama(7)) = 0 Then
            result(2) = "prmodifikasiuser can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 20 Then
            result(2) = "prmodifikasiuser should not be more than 20 character." : GoTo selesai
        End If

        'pamodifikasitgl(8) As DateTime
        If Len(dataUtama(8)) = 0 Then
            result(2) = "prmodifikasitgl can't be empty" : GoTo selesai
        End If

        'pacustomdbl1(17) As Double
        If Len(dataUtama(17)) = 0 Then
            result(2) = "prcustomdbl1 can't be empty" : GoTo selesai
        End If

        'pacustomdbl2(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "prcustomdbl2 can't be empty" : GoTo selesai
        End If

        'pacustomdbl3(19) As Double
        If Len(dataUtama(19)) = 0 Then
            result(2) = "prcustomdbl3 can't be empty" : GoTo selesai
        End If

        'pacustomdate1(20) As Date
        If Len(dataUtama(20)) = 0 Then
            result(2) = "prcustomdate1 can't be empty" : GoTo selesai
        End If

        'pacustomdate2(21) As Date
        If Len(dataUtama(21)) = 0 Then
            result(2) = "prcustomdate2 can't be empty" : GoTo selesai
        End If

        'pacustomdate3(22) As Date
        If Len(dataUtama(22)) = 0 Then
            result(2) = "prcustomdate3 can't be empty" : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "prid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "praktif", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahData(dtutama, "prid~prkode~prnama~prcatatan~praktif~prinputuser~prinputtgl~prmodifikasiuser~prmodifikasitgl~prcustomtext1~prcustomtext2~prcustomtext3~prcustomtext4~prcustomtext5~prcustomint1~prcustomint2~prcustomint3~prcustomdbl1~prcustomdbl2~prcustomdbl3~prcustomdate1~prcustomdate2~prcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22))

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idpadetail(0) As Integer, idpa(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, hpppersen(13) As Double, hpp(14) As Double, 
        'rekpersediaan(15) As String, cabang(16) As String, lokasi(17) As String, gudangasal(18) As String, gudangproduksi(19) As String, 
        'gudangtujuan(20) As String, costcenter(21) As String, divisi(22) As String, subdivisi(23) As String, proyek(24) As String, 
        'catatan(25) As String, urutan(26) As Integer, idbom(27) As Integer, idbomin(28) As Integer, customtext1(29) As String, 
        'customtext2(30) As String, customtext3(31) As String, customdbl1(32) As Double, customdbl2(33) As Double, customdbl3(34) As Double, 
        'customdate1(35) As Date, customdate2(36) As Date, customdate3(37) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idpadetail, idpa, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, 
        'hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, 
        'costcenter, divisi, subdivisi, proyek, catatan, urutan, idbom, 
        'idbomin, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idprdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idpa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namaaktivitas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kodemesin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbom", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbomin", AsEnumTypeData.AsInt64)
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
            If (dataRowDetail.Length <> 20) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            
            'urutan(26) As Integer
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "urutan required numeric." : GoTo selesai
            End If
            'customdbl1(32) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(33) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(34) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(35) As Date
            If (IsDate(dataRowDetail(17)) = False) Then
                result(2) = "customdate1 required date." : GoTo selesai
            End If
            'customdate2(36) As Date
            If (IsDate(dataRowDetail(18)) = False) Then
                result(2) = "customdate2 required date." : GoTo selesai
            End If
            'customdate3(37) As Date
            If (IsDate(dataRowDetail(19)) = False) Then
                result(2) = "customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'idpadetail(0) As Integer
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idprdetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idprdetail should not be more than 20 character." : GoTo selesai
            End If

            'idpa(1) As Integer 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idpr can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idpr should not be more than 20 character." : GoTo selesai
            End If

            'idbarang(2) As Integer 
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - idpa can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 20 Then
                result(2) = "Row : " & i & " - idpa should not be more than 20 character." : GoTo selesai
            End If

            'namabarang(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - namaaktivitas can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 100 Then
                result(2) = "Row : " & i & " - namaaktivitas should not be more than 100 character." : GoTo selesai
            End If

            'customdbl1(32) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(33) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(34) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(35) As Date
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(36) As Date
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(37) As Date
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            AsDataTableTambahData(dtdetail, "idprdetail~idpr~idpa~namaaktivitas~kodemesin~costcenter~divisi~subdivisi~proyek~catatan~urutan~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19))

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

                If isUpdate Then
                    result(4) = drutama("prid")
                    notransaksi = drutama("prkode")

                    'SIMPAN HISTORY ========================
                    Dim SimpanHistory As New m1_production_activity_history
                    Dim rsSimpanHistory As String = SimpanHistory.M1_Production_Activity_HistorySimpan("" & paramSplit(0) & "★M1_Production_Route_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("prkode")) & "▼" & FixQuotes(drutama("prid")) & "")
                    Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                    Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                    'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                    If (rsSplitResult(1) = 0) Then
                        result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF SIMPAN HISTORY ==================

                    sql = "Update M1_Production_Route set prkode  = '" & FixQuotes(drutama("prkode")) & "', prnama  = '" & FixQuotes(drutama("prnama")) & "', prcatatan  = '" & FixQuotes(drutama("prcatatan")) & "', praktif  = " & drutama("praktif") & ", prmodifikasiuser  = '" & FixQuotes(drutama("prmodifikasiuser")) & "', prmodifikasitgl  = NOW(), prcustomtext1  = '" & FixQuotes(drutama("prcustomtext1")) & "', prcustomtext2  = '" & FixQuotes(drutama("prcustomtext2")) & "', prcustomtext3  = '" & FixQuotes(drutama("prcustomtext3")) & "', prcustomtext4  = '" & FixQuotes(drutama("prcustomtext4")) & "', prcustomtext5  = '" & FixQuotes(drutama("prcustomtext5")) & "', prcustomint1  = " & drutama("prcustomint1") & ", prcustomint2  = " & drutama("prcustomint2") & ", prcustomint3  = " & drutama("prcustomint3") & ", prcustomdbl1  = '" & FixDouble(drutama("prcustomdbl1")) & "', prcustomdbl2  = '" & FixDouble(drutama("prcustomdbl2")) & "', prcustomdbl3  = '" & FixDouble(drutama("prcustomdbl3")) & "', prcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("prcustomdate1"))) & "', prcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("prcustomdate2"))) & "', prcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("prcustomdate3"))) & "' where prid = " & drutama("prid") & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Else

                    sql = "Insert into M1_Production_Route (prkode, prnama, prcatatan, praktif, prinputuser, prinputtgl, prmodifikasiuser, prmodifikasitgl, prcustomtext1, prcustomtext2, prcustomtext3, prcustomtext4, prcustomtext5, prcustomint1, prcustomint2, prcustomint3, prcustomdbl1, prcustomdbl2, prcustomdbl3, prcustomdate1, prcustomdate2, prcustomdate3) values('" & FixQuotes(drutama("prkode")) & "', '" & FixQuotes(drutama("prnama")) & "', '" & FixQuotes(drutama("prcatatan")) & "', " & drutama("praktif") & ", '" & FixQuotes(drutama("prinputuser")) & "', '" & FixQuotes(AsFormatTanggal(drutama("prinputtgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(drutama("prmodifikasiuser")) & "', '" & FixQuotes(AsFormatTanggal(drutama("prmodifikasitgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(drutama("prcustomtext1")) & "', '" & FixQuotes(drutama("prcustomtext2")) & "', '" & FixQuotes(drutama("prcustomtext3")) & "', '" & FixQuotes(drutama("prcustomtext4")) & "', '" & FixQuotes(drutama("prcustomtext5")) & "', " & drutama("prcustomint1") & ", " & drutama("prcustomint2") & ", " & drutama("prcustomint3") & ", '" & FixDouble(drutama("prcustomdbl1")) & "', '" & FixDouble(drutama("prcustomdbl2")) & "', '" & FixDouble(drutama("prcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("prcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("prcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("prcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select prid from M1_Production_Route where prkode='" & FixQuotes(drutama("prkode")) & "' AND prinputuser= '" & userid & "' order by prmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M1_Production_Route_Detail where idpr = '" & result(4) & "'"
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
                        strValue2.Append("('" & FixQuotes(dr1("idprdetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("idpa")) & "', '" & FixQuotes(dr1("namaaktivitas")) & "', '" & FixQuotes(dr1("kodemesin")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M1_Production_Route_Detail(idprdetail, idpr, idpa, namaaktivitas, kodemesin, costcenter, divisi, subdivisi, proyek, catatan, urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
    Public Function M1_Production_RouteDelete(ByVal param As String) As String

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

            'CEK TERKAIT =============================================================
            Dim paramTerkait As String = M1_Production_RouteTerkait(PostWsTerkait(paramSplit(0), "M1_Production_RouteTerkait", pagingSplit(0), pagingSplit(1), "", "", formatTgl, formatTglWaktu, idtransaksi))
            Dim hasilTerkait As New RsHasilWsSearch
            hasilTerkait = GetWsSearch(paramTerkait)
            If hasilTerkait.success = 1 Then
                result(2) = "It has related transactions."

                resultPaging(0) = hasilTerkait.isPaging
                resultPaging(1) = hasilTerkait.isNext
                resultPaging(2) = hasilTerkait.isPrevious
                resultPaging(3) = hasilTerkait.countPage
                resultPaging(4) = hasilTerkait.countRow

                search = hasilTerkait.data : Trans.Rollback() : GoTo selesai
            End If
            'END OF CEK TERKAIT ======================================================

            'DELETE DETAIL
            sql = "DELETE FROM M1_Production_Route_Detail WHERE idpr = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M1_Production_Route WHERE prid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            Trans.Commit()  '*** Commit Transaction ***'.
            result(1) = 1
            result(2) = ""
            result(3) = 0
            result(4) = idtransaksi

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M1_Production_RouteSearch(PostWsSearch(paramSplit(0), "M1_Production_RouteSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_Production_RouteSearch(ByVal param As String) As String
        'M1_Production_RouteSearch --------------------------------------------------------
        'paid, pakode, panama, pacatatan, paaktif, painputuser, painputtgl, 
        'pamodifikasiuser, pamodifikasitgl, pacustomtext1, pacustomtext2, pacustomtext3, pacustomtext4, pacustomtext5, 
        'pacustomint1, pacustomint2, pacustomint3, pacustomdbl1, pacustomdbl2, pacustomdbl3, pacustomdate1, 
        'pacustomdate2, pacustomdate3, painputusernama, pamodifikasiusernama, pagudangbahan, pagudanghasil, pagudangbahannama, pagudanghasilnama

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

        'PANGGIL QUERY
        sql = "SELECT pr.prid, pr.prkode, pr.prnama, pr.prcatatan, pr.praktif, pr.prinputuser, pr.prinputtgl, pr.prmodifikasiuser, pr.prmodifikasitgl, pr.prcustomtext1, pr.prcustomtext2, pr.prcustomtext3, pr.prcustomtext4, pr.prcustomtext5, pr.prcustomint1, pr.prcustomint2, pr.prcustomint3, pr.prcustomdbl1, pr.prcustomdbl2, pr.prcustomdbl3, pr.prcustomdate1, pr.prcustomdate2, pr.prcustomdate3, u1.unama as prinputusernama, u2.unama as prmodifikasiusernama FROM M1_Production_Route pr LEFT JOIN m0_user u1 ON pr.prinputuser = u1.userid LEFT JOIN m0_user u2 ON pr.prmodifikasiuser = u2.userid"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Production_Route", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("prid"), ""), sptField,
                     FxDB(dr("prkode"), ""), sptField,
                     FxDB(dr("prnama"), ""), sptField,
                     FxDB(dr("prcatatan"), ""), sptField,
                     FxDB(dr("praktif"), 0), sptField,
                     FxDB(dr("prinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("prinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("prmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("prmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("prcustomtext1"), ""), sptField,
                     FxDB(dr("prcustomtext2"), ""), sptField,
                     FxDB(dr("prcustomtext3"), ""), sptField,
                     FxDB(dr("prcustomtext4"), ""), sptField,
                     FxDB(dr("prcustomtext5"), ""), sptField,
                     FxDB(dr("prcustomint1"), 0), sptField,
                     FxDB(dr("prcustomint2"), 0), sptField,
                     FxDB(dr("prcustomint3"), 0), sptField,
                     FxDB(dr("prcustomdbl1"), 0), sptField,
                     FxDB(dr("prcustomdbl2"), 0), sptField,
                     FxDB(dr("prcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("prcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("prcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("prinputusernama"), ""), sptField,
                     FxDB(dr("prmodifikasiusernama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Production Route data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("prid, prkode, prnama, prcatatan, praktif, prinputuser, prinputtgl, prmodifikasiuser, prmodifikasitgl, prcustomtext1, prcustomtext2, prcustomtext3, prcustomtext4, prcustomtext5, prcustomint1, prcustomint2, prcustomint3, prcustomdbl1, prcustomdbl2, prcustomdbl3, prcustomdate1, prcustomdate2, prcustomdate3, prinputusernama, prmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Production_RouteCekId(ByVal param As String) As String

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim idSplit(1) As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", idtransaksi As String = ""

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
        If ClsValidKey.ApaBisaAkses(1, 1, 2) = False Then
            result(2) = "Access denied for delete data"
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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (Len(paramSplit(5)) = 0) Then
            result(2) = "prkode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'CEK DI DATABASE ================================================================
        Dim dt As DataTable
        Dim exist As Integer = 0
        dt = AsDataTableAmbilDariDB("SELECT COUNT(prkode) FROM M1_Production_Route WHERE prkode='" & idtransaksi & "'")
        exist = dt.Rows(0)(0)

        If (exist > 0) Then
            result(2) = "'" & idtransaksi & "' already exist for column prkode." : GoTo selesai
        End If

        result(1) = 1
        result(2) = ""
        result(3) = 0
        result(4) = idtransaksi
        'END OF CEK DI DATABASE ==========================================================


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
    Public Function M1_Production_RouteTerkait(ByVal param As String) As String
        'M1_Production_RouteTerkait --------------------------------------------------------
        'pakode, panama, sumber, idterkait

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
            result(2) = "prgeNumber required numeric." : GoTo selesai
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
        'CEK IDTRANSAKSI
        Dim idtransaksi As String = ""
        If (Len(paramSplit(5)) = 0) Then
            result(2) = "prkode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("M1_Production_Route_terkait")
        sql = sql.Replace("valkode", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Activity", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("prkode"), ""), sptField,
                             FxDB(dr("prnama"), ""), sptField,
                             FxDB(dr("sumber"), ""), sptField,
                             FxDB(dr("idterkait"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Related Production Route data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("prkode, prnama, sumber, idterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Production_RouteGetdataById(ByVal param As String) As String

        'M1_Production_RouteGetdataById Utama --------------------------------------------------------
        'paid, pakode, panama, pacatatan, paaktif, painputuser, painputtgl, 
        'pamodifikasiuser, pamodifikasitgl, pacustomtext1, pacustomtext2, pacustomtext3, pacustomtext4, pacustomtext5, 
        'pacustomint1, pacustomint2, pacustomint3, pacustomdbl1, pacustomdbl2, pacustomdbl3, pacustomdate1, 
        'pacustomdate2, pacustomdate3, painputusernama, pamodifikasiusernama, pagudangbahan, pagudanghasil, pagudangbahannama, pagudanghasilnama

        'M1_Production_RouteGetdataById Detail -------------------------------------------------------
        'idpadetail, idpa, idbarang, 
        'namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, 
        'matauang, kurs, harga, hpppersen, hpp, rekpersediaan, cabang, 
        'lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, 
        'proyek, catatan, urutan, idbom, idbomin, customtext1, customtext2, 
        'customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'bkode, bhpp, bjenis, bserial, bbatch, bjmllapangan, bsatuanlapangan, 
        'basset

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

        Dim NmMemcached As String = "aplikasi1-M1_Production_Route~M1_Production_Route_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "prid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "prid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m2_aj_getdata")
        sql = "SELECT pr.prid, pr.prkode, pr.prnama, pr.prcatatan, pr.praktif, pr.prinputuser, pr.prinputtgl, pr.prmodifikasiuser, pr.prmodifikasitgl, pr.prcustomtext1, pr.prcustomtext2, pr.prcustomtext3, pr.prcustomtext4, pr.prcustomtext5, pr.prcustomint1, pr.prcustomint2, pr.prcustomint3, pr.prcustomdbl1, pr.prcustomdbl2, pr.prcustomdbl3, pr.prcustomdate1, pr.prcustomdate2, pr.prcustomdate3, u1.unama AS prinputusernama, u2.unama AS prmodifikasiusernama, prd.idprdetail, prd.idpr, prd.idpa, prd.namaaktivitas, prd.kodemesin, prd.costcenter, prd.divisi, prd.subdivisi, prd.proyek, prd.catatan, prd.urutan, prd.customtext1, prd.customtext2, prd.customtext3, prd.customdbl1, prd.customdbl2, prd.customdbl3, prd.customdate1, prd.customdate2, prd.customdate3, pa.pakode AS kodeaktivitas, m.mnama AS namamesin FROM M1_Production_Route pr JOIN M1_Production_Route_detail prd ON pr.prid = prd.idpr LEFT JOIN m0_user u1 ON pr.prinputuser = u1.userid LEFT JOIN m0_user u2 ON pr.prmodifikasiuser = u2.userid LEFT JOIN m1_production_activity pa ON prd.idpa = pa.paid LEFT JOIN m1_machine m ON prd.kodemesin = m.mkode"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("prid"), ""), sptField,
                     FxDB(drutama("prkode"), ""), sptField,
                     FxDB(drutama("prnama"), ""), sptField,
                     FxDB(drutama("prcatatan"), ""), sptField,
                     FxDB(drutama("praktif"), 0), sptField,
                     FxDB(drutama("prinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("prinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("prmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("prmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("prcustomtext1"), ""), sptField,
                     FxDB(drutama("prcustomtext2"), ""), sptField,
                     FxDB(drutama("prcustomtext3"), ""), sptField,
                     FxDB(drutama("prcustomtext4"), ""), sptField,
                     FxDB(drutama("prcustomtext5"), ""), sptField,
                     FxDB(drutama("prcustomint1"), 0), sptField,
                     FxDB(drutama("prcustomint2"), 0), sptField,
                     FxDB(drutama("prcustomint3"), 0), sptField,
                     FxDB(drutama("prcustomdbl1"), 0), sptField,
                     FxDB(drutama("prcustomdbl2"), 0), sptField,
                     FxDB(drutama("prcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("prcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("prcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("prinputusernama"), ""), sptField,
                     FxDB(drutama("prmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idprdetail"), ""), sptField,
                     FxDB(dr("idpr"), ""), sptField,
                     FxDB(dr("idpa"), ""), sptField,
                     FxDB(dr("namaaktivitas"), ""), sptField,
                     FxDB(dr("kodemesin"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kodeaktivitas"), ""), sptField,
                     FxDB(dr("namamesin"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("prid, prkode, prnama, prcatatan, praktif, prinputuser, prinputtgl, prmodifikasiuser, prmodifikasitgl, prcustomtext1, prcustomtext2, prcustomtext3, prcustomtext4, prcustomtext5, prcustomint1, prcustomint2, prcustomint3, prcustomdbl1, prcustomdbl2, prcustomdbl3, prcustomdate1, prcustomdate2, prcustomdate3, prinputusernama, prmodifikasiusernama" & sptSubParam & "idprdetail, idpr, idpa, namaaktivitas, kodemesin, costcenter, divisi, subdivisi, proyek, catatan, urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodeaktivitas, namamesin"))

        Return wsResult
    End Function

End Class