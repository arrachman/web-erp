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
Public Class m1_production_activity
    Inherits System.Web.Services.WebService
    Public ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi

    <WebMethod()>
    Public Function M1_Production_ActivitySimpan(ByVal param As String) As String
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
        If (dataUtama.Length <> 25) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'paaktif(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "paaktif required numeric." : GoTo selesai
        End If
        'painputtgl(6) As DateTime
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "painputtgl required date." : GoTo selesai
        End If
        'pamodifikasitgl(8) As DateTime
        If (IsDate(dataUtama(8)) = False) Then
            result(2) = "pamodifikasitgl required date." : GoTo selesai
        End If
        'pacustomint1(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "pacustomint1 required numeric." : GoTo selesai
        End If
        'pacustomint2(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "pacustomint2 required numeric." : GoTo selesai
        End If
        'pacustomint3(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "pacustomint3 required numeric." : GoTo selesai
        End If
        'pacustomdbl1(17) As Double
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "pacustomdbl1 required numeric." : GoTo selesai
        End If
        'pacustomdbl2(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "pacustomdbl2 required numeric." : GoTo selesai
        End If
        'pacustomdbl3(19) As Double
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "pacustomdbl3 required numeric." : GoTo selesai
        End If
        'pacustomdate1(20) As Date
        If (IsDate(dataUtama(20)) = False) Then
            result(2) = "pacustomdate1 required date." : GoTo selesai
        End If
        'pacustomdate2(21) As Date
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "pacustomdate2 required date." : GoTo selesai
        End If
        'pacustomdate3(22) As Date
        If (IsDate(dataUtama(22)) = False) Then
            result(2) = "pacustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'paid(0) As Integer 
        If Len(dataUtama(0)) = 0 Then
            result(2) = "paid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 20 Then
            result(2) = "paid should not be more than 20 character." : GoTo selesai
        End If

        'pakode(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "pakode can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "pakode should not be more than 25 character." : GoTo selesai
        End If

        'panama(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "panama can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 100 Then
            result(2) = "panama should not be more than 100 character." : GoTo selesai
        End If

        'painputuser(5) As Integer
        If Len(dataUtama(5)) = 0 Then
            result(2) = "painputuser can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 20 Then
            result(2) = "painputuser should not be more than 20 character." : GoTo selesai
        End If

        'painputtgl(6) As DateTime
        If Len(dataUtama(6)) = 0 Then
            result(2) = "painputtgl can't be empty" : GoTo selesai
        End If

        'pamodifikasiuser(7) As Integer
        If Len(dataUtama(7)) = 0 Then
            result(2) = "pamodifikasiuser can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 20 Then
            result(2) = "pamodifikasiuser should not be more than 20 character." : GoTo selesai
        End If

        'pamodifikasitgl(8) As DateTime
        If Len(dataUtama(8)) = 0 Then
            result(2) = "pamodifikasitgl can't be empty" : GoTo selesai
        End If

        'pacustomdbl1(17) As Double
        If Len(dataUtama(17)) = 0 Then
            result(2) = "pacustomdbl1 can't be empty" : GoTo selesai
        End If

        'pacustomdbl2(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "pacustomdbl2 can't be empty" : GoTo selesai
        End If

        'pacustomdbl3(19) As Double
        If Len(dataUtama(19)) = 0 Then
            result(2) = "pacustomdbl3 can't be empty" : GoTo selesai
        End If

        'pacustomdate1(20) As Date
        If Len(dataUtama(20)) = 0 Then
            result(2) = "pacustomdate1 can't be empty" : GoTo selesai
        End If

        'pacustomdate2(21) As Date
        If Len(dataUtama(21)) = 0 Then
            result(2) = "pacustomdate2 can't be empty" : GoTo selesai
        End If

        'pacustomdate3(22) As Date
        If Len(dataUtama(22)) = 0 Then
            result(2) = "pacustomdate3 can't be empty" : GoTo selesai
        End If

        'pagudangbahan(23) As String
        If Len(dataUtama(23)) = 0 Then
            result(2) = "pagudangbahan can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(23)) > 250 Then
            result(2) = "pagudangbahan should not be more than 250 character." : GoTo selesai
        End If

        'pagudanghasil(24) As String
        If Len(dataUtama(24)) = 0 Then
            result(2) = "pagudanghasil can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(24)) > 250 Then
            result(2) = "pagudanghasil should not be more than 250 character." : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "paid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pakode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "panama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "paaktif", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "painputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "painputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pamodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pamodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pacustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pacustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pacustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pagudangbahan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pagudanghasil", AsEnumTypeData.AsString)
        AsDataTableTambahData(dtutama, "paid~pakode~panama~pacatatan~paaktif~painputuser~painputtgl~pamodifikasiuser~pamodifikasitgl~pacustomtext1~pacustomtext2~pacustomtext3~pacustomtext4~pacustomtext5~pacustomint1~pacustomint2~pacustomint3~pacustomdbl1~pacustomdbl2~pacustomdbl3~pacustomdate1~pacustomdate2~pacustomdate3~pagudangbahan~pagudanghasil", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24))

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
        AsDataTableTambahField(dtdetail, "idpadetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "hpppersen", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "hpp", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "rekpersediaan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangproduksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangtujuan", AsEnumTypeData.AsString)
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
            If (dataRowDetail.Length <> 38) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'jml(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "jml required numeric." : GoTo selesai
            End If
            'nilaisatuan(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarang(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "jmlbarang required numeric." : GoTo selesai
            End If
            'kurs(11) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "kurs required numeric." : GoTo selesai
            End If
            'harga(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "harga required numeric." : GoTo selesai
            End If
            'hpppersen(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "hpppersen required numeric." : GoTo selesai
            End If
            'hpp(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "hpp required numeric." : GoTo selesai
            End If
            'urutan(26) As Integer
            If (IsNumeric(dataRowDetail(26)) = False) Then
                result(2) = "urutan required numeric." : GoTo selesai
            End If
            'customdbl1(32) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(33) As Double
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(34) As Double
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(35) As Date
            If (IsDate(dataRowDetail(35)) = False) Then
                result(2) = "customdate1 required date." : GoTo selesai
            End If
            'customdate2(36) As Date
            If (IsDate(dataRowDetail(36)) = False) Then
                result(2) = "customdate2 required date." : GoTo selesai
            End If
            'customdate3(37) As Date
            If (IsDate(dataRowDetail(37)) = False) Then
                result(2) = "customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'idpadetail(0) As Integer
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idpadetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idpadetail should not be more than 20 character." : GoTo selesai
            End If

            'idpa(1) As Integer 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idpa can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idpa should not be more than 20 character." : GoTo selesai
            End If

            'idbarang(2) As Integer 
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - idbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 20 Then
                result(2) = "Row : " & i & " - idbarang should not be more than 20 character." : GoTo selesai
            End If

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

            'harga(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'hpppersen(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - hpppersen can't be empty" : GoTo selesai
            End If

            'hpp(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - hpp can't be empty" : GoTo selesai
            End If

            'idbom(27) As Integer 
            If Len(dataRowDetail(27)) = 0 Then
                result(2) = "Row : " & i & " - idbom can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(27)) > 20 Then
                result(2) = "Row : " & i & " - idbom should not be more than 20 character." : GoTo selesai
            End If

            'idbomin(28) As Integer 
            If Len(dataRowDetail(28)) = 0 Then
                result(2) = "Row : " & i & " - idbomin can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(28)) > 20 Then
                result(2) = "Row : " & i & " - idbomin should not be more than 20 character." : GoTo selesai
            End If

            'customdbl1(32) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(33) As Double
            If Len(dataRowDetail(33)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(34) As Double
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(35) As Date
            If Len(dataRowDetail(35)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(36) As Date
            If Len(dataRowDetail(36)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(37) As Date
            If Len(dataRowDetail(37)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            AsDataTableTambahData(dtdetail, "idpadetail~idpa~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpppersen~hpp~rekpersediaan~cabang~lokasi~gudangasal~gudangproduksi~gudangtujuan~costcenter~divisi~subdivisi~proyek~catatan~urutan~idbom~idbomin~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37))

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
                    result(4) = drutama("paid")
                    notransaksi = drutama("pakode")

                    'SIMPAN HISTORY ========================
                    Dim SimpanHistory As New m1_production_activity_history
                    Dim rsSimpanHistory As String = SimpanHistory.M1_Production_Activity_HistorySimpan("" & paramSplit(0) & "★M1_Production_Activity_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("pakode")) & "▼" & FixQuotes(drutama("paid")) & "")
                    Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                    Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                    'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                    If (rsSplitResult(1) = 0) Then
                        result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF SIMPAN HISTORY ==================

                    sql = "Update M1_Production_Activity set pakode  = '" & FixQuotes(drutama("pakode")) & "', panama  = '" & FixQuotes(drutama("panama")) & "', pacatatan  = '" & FixQuotes(drutama("pacatatan")) & "', paaktif  = " & drutama("paaktif") & ", pamodifikasiuser  = '" & FixQuotes(drutama("pamodifikasiuser")) & "', pamodifikasitgl  = NOW(), pacustomtext1  = '" & FixQuotes(drutama("pacustomtext1")) & "', pacustomtext2  = '" & FixQuotes(drutama("pacustomtext2")) & "', pacustomtext3  = '" & FixQuotes(drutama("pacustomtext3")) & "', pacustomtext4  = '" & FixQuotes(drutama("pacustomtext4")) & "', pacustomtext5  = '" & FixQuotes(drutama("pacustomtext5")) & "', pacustomint1  = " & drutama("pacustomint1") & ", pacustomint2  = " & drutama("pacustomint2") & ", pacustomint3  = " & drutama("pacustomint3") & ", pacustomdbl1  = '" & FixDouble(drutama("pacustomdbl1")) & "', pacustomdbl2  = '" & FixDouble(drutama("pacustomdbl2")) & "', pacustomdbl3  = '" & FixDouble(drutama("pacustomdbl3")) & "', pacustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("pacustomdate1"))) & "', pacustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("pacustomdate2"))) & "', pacustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("pacustomdate3"))) & "', pagudangbahan  = '" & FixQuotes(drutama("pagudangbahan")) & "', pagudanghasil  = '" & FixQuotes(drutama("pagudanghasil")) & "' where paid = " & drutama("paid") & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Else

                    sql = "Insert into M1_Production_Activity (pakode, panama, pacatatan, paaktif, painputuser, painputtgl, pamodifikasiuser, pamodifikasitgl, pacustomtext1, pacustomtext2, pacustomtext3, pacustomtext4, pacustomtext5, pacustomint1, pacustomint2, pacustomint3, pacustomdbl1, pacustomdbl2, pacustomdbl3, pacustomdate1, pacustomdate2, pacustomdate3, pagudangbahan, pagudanghasil) values('" & FixQuotes(drutama("pakode")) & "', '" & FixQuotes(drutama("panama")) & "', '" & FixQuotes(drutama("pacatatan")) & "', " & drutama("paaktif") & ", '" & FixQuotes(drutama("painputuser")) & "', '" & FixQuotes(AsFormatTanggal(drutama("painputtgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(drutama("pamodifikasiuser")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pamodifikasitgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(drutama("pacustomtext1")) & "', '" & FixQuotes(drutama("pacustomtext2")) & "', '" & FixQuotes(drutama("pacustomtext3")) & "', '" & FixQuotes(drutama("pacustomtext4")) & "', '" & FixQuotes(drutama("pacustomtext5")) & "', " & drutama("pacustomint1") & ", " & drutama("pacustomint2") & ", " & drutama("pacustomint3") & ", '" & FixDouble(drutama("pacustomdbl1")) & "', '" & FixDouble(drutama("pacustomdbl2")) & "', '" & FixDouble(drutama("pacustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pacustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("pacustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("pacustomdate3"))) & "', '" & FixQuotes(drutama("pagudangbahan")) & "', '" & FixQuotes(drutama("pagudanghasil")) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select paid from m1_production_activity where pakode='" & FixQuotes(drutama("pakode")) & "' AND painputuser= '" & userid & "' order by pamodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from m1_production_activity_Detail where idpa = '" & result(4) & "'"
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
                        strValue2.Append("('" & FixQuotes(dr1("idpadetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("idbarang")) & "', '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hpppersen")) & "', '" & FixDouble(dr1("hpp")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangproduksi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", '" & FixQuotes(dr1("idbom")) & "', '" & FixQuotes(dr1("idbomin")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M1_Production_Activity_Detail(idpadetail, idpa, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbom, idbomin, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
    Public Function M1_Production_ActivityDelete(ByVal param As String) As String

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
            Dim paramTerkait As String = M1_Production_ActivityTerkait(PostWsTerkait(paramSplit(0), "M1_Production_ActivityTerkait", pagingSplit(0), pagingSplit(1), "", "", formatTgl, formatTglWaktu, idtransaksi))
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
            sql = "DELETE FROM M1_Production_Activity_Detail WHERE idpa = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M1_Production_Activity WHERE paid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M1_Production_ActivitySearch(PostWsSearch(paramSplit(0), "M1_Production_ActivitySearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_Production_ActivitySearch(ByVal param As String) As String
        'M1_Production_ActivitySearch --------------------------------------------------------
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
        sql = "SELECT pa.paid, pa.pakode, pa.panama, pa.pacatatan, pa.paaktif, pa.painputuser, pa.painputtgl, pa.pamodifikasiuser, pa.pamodifikasitgl, pa.pacustomtext1, pa.pacustomtext2, pa.pacustomtext3, pa.pacustomtext4, pa.pacustomtext5, pa.pacustomint1, pa.pacustomint2, pa.pacustomint3, pa.pacustomdbl1, pa.pacustomdbl2, pa.pacustomdbl3, pa.pacustomdate1, pa.pacustomdate2, pa.pacustomdate3, u1.unama as painputusernama, u2.unama as pamodifikasiusernama, pa.pagudangbahan, pa.pagudanghasil, wh1.wnama as pagudangbahannama, wh2.wnama as pagudanghasilnama FROM m1_production_activity pa LEFT JOIN m0_user u1 ON pa.painputuser = u1.userid LEFT JOIN m0_user u2 ON pa.pamodifikasiuser = u2.userid LEFT JOIN m1_warehouse wh1 ON pa.pagudangbahan = wh1.wkode LEFT JOIN m1_warehouse wh2 ON pa.pagudanghasil = wh2.wkode"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Production_Activity", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("paid"), ""), sptField,
                     FxDB(dr("pakode"), ""), sptField,
                     FxDB(dr("panama"), ""), sptField,
                     FxDB(dr("pacatatan"), ""), sptField,
                     FxDB(dr("paaktif"), 0), sptField,
                     FxDB(dr("painputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("painputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pamodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pamodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pacustomtext1"), ""), sptField,
                     FxDB(dr("pacustomtext2"), ""), sptField,
                     FxDB(dr("pacustomtext3"), ""), sptField,
                     FxDB(dr("pacustomtext4"), ""), sptField,
                     FxDB(dr("pacustomtext5"), ""), sptField,
                     FxDB(dr("pacustomint1"), 0), sptField,
                     FxDB(dr("pacustomint2"), 0), sptField,
                     FxDB(dr("pacustomint3"), 0), sptField,
                     FxDB(dr("pacustomdbl1"), 0), sptField,
                     FxDB(dr("pacustomdbl2"), 0), sptField,
                     FxDB(dr("pacustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pacustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("pacustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("pacustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("painputusernama"), ""), sptField,
                     FxDB(dr("pamodifikasiusernama"), ""), sptField,
                     FxDB(dr("pagudangbahan"), ""), sptField,
                     FxDB(dr("pagudanghasil"), ""), sptField,
                     FxDB(dr("pagudangbahannama"), ""), sptField,
                     FxDB(dr("pagudanghasilnama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Production Activity data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("paid, pakode, panama, pacatatan, paaktif, painputuser, painputtgl, pamodifikasiuser, pamodifikasitgl, pacustomtext1, pacustomtext2, pacustomtext3, pacustomtext4, pacustomtext5, pacustomint1, pacustomint2, pacustomint3, pacustomdbl1, pacustomdbl2, pacustomdbl3, pacustomdate1, pacustomdate2, pacustomdate3, painputusernama, pamodifikasiusernama, pagudangbahan, pagudanghasil, pagudangbahannama, pagudanghasilnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Production_ActivityCekId(ByVal param As String) As String

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
            result(2) = "pakode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'CEK DI DATABASE ================================================================
        Dim dt As DataTable
        Dim exist As Integer = 0
        dt = AsDataTableAmbilDariDB("SELECT COUNT(pakode) FROM m1_production_activity WHERE pakode='" & idtransaksi & "'")
        exist = dt.Rows(0)(0)

        If (exist > 0) Then
            result(2) = "'" & idtransaksi & "' already exist for column pakode." : GoTo selesai
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
    Public Function M1_Production_ActivityTerkait(ByVal param As String) As String
        'M1_Production_ActivityTerkait --------------------------------------------------------
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
        'CEK IDTRANSAKSI
        Dim idtransaksi As String = ""
        If (Len(paramSplit(5)) = 0) Then
            result(2) = "pakode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m1_production_activity_terkait")
        sql = sql.Replace("valkode", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Activity", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("pakode"), ""), sptField,
                             FxDB(dr("panama"), ""), sptField,
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
            result(2) = "Related Production Activity data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pakode, panama, sumber, idterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Production_ActivityGetdataById(ByVal param As String) As String

        'M1_Production_ActivityGetdataById Utama --------------------------------------------------------
        'paid, pakode, panama, pacatatan, paaktif, painputuser, painputtgl, 
        'pamodifikasiuser, pamodifikasitgl, pacustomtext1, pacustomtext2, pacustomtext3, pacustomtext4, pacustomtext5, 
        'pacustomint1, pacustomint2, pacustomint3, pacustomdbl1, pacustomdbl2, pacustomdbl3, pacustomdate1, 
        'pacustomdate2, pacustomdate3, painputusernama, pamodifikasiusernama, pagudangbahan, pagudanghasil, pagudangbahannama, pagudanghasilnama

        'M1_Production_ActivityGetdataById Detail -------------------------------------------------------
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

        Dim NmMemcached As String = "aplikasi1-M1_Production_Activity~M1_Production_Activity_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "paid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "paid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m2_aj_getdata")
        sql = "SELECT pa.paid, pa.pakode, pa.panama, pa.pacatatan, pa.paaktif, pa.painputuser, pa.painputtgl, pa.pamodifikasiuser, pa.pamodifikasitgl, pa.pacustomtext1, pa.pacustomtext2, pa.pacustomtext3, pa.pacustomtext4, pa.pacustomtext5, pa.pacustomint1, pa.pacustomint2, pa.pacustomint3, pa.pacustomdbl1, pa.pacustomdbl2, pa.pacustomdbl3, pa.pacustomdate1, pa.pacustomdate2, pa.pacustomdate3, u1.unama AS painputusernama, u2.unama AS pamodifikasiusernama, pad.idpadetail, pad.idpa, pad.idbarang, pad.namabarang, pad.tipebarang, pad.jml, pad.satuan, pad.nilaisatuan, pad.jmlbarang, pad.satuanbarang, pad.matauang, pad.kurs, pad.harga, pad.hpppersen, pad.hpp, i.brekpersediaan as rekpersediaan, pad.cabang, pad.lokasi, pad.gudangasal, pad.gudangproduksi, pad.gudangtujuan, pad.costcenter, pad.divisi, pad.subdivisi, pad.proyek, pad.catatan, pad.urutan, pad.idbom, pad.idbomin, pad.customtext1, pad.customtext2, pad.customtext3, pad.customdbl1, pad.customdbl2, pad.customdbl3, pad.customdate1, pad.customdate2, pad.customdate3, i.bkode AS kodebarang,  i.bhpp,  i.bjenis,  i.bserial,  i.bbatch,  i.bjmllapangan,  i.bsatuanlapangan, i.basset, pa.pagudangbahan, pa.pagudanghasil, wh1.wnama as pagudangbahannama, wh2.wnama as pagudanghasilnama FROM m1_production_activity pa JOIN m1_production_activity_detail pad ON pa.paid = pad.idpa JOIN m1_item i ON pad.idbarang = i.bid LEFT JOIN m0_user u1 ON pa.painputuser = u1.userid LEFT JOIN m0_user u2 ON pa.pamodifikasiuser = u2.userid LEFT JOIN m1_warehouse wh1 ON pa.pagudangbahan = wh1.wkode LEFT JOIN m1_warehouse wh2 ON pa.pagudanghasil = wh2.wkode"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("paid"), ""), sptField,
                     FxDB(drutama("pakode"), ""), sptField,
                     FxDB(drutama("panama"), ""), sptField,
                     FxDB(drutama("pacatatan"), ""), sptField,
                     FxDB(drutama("paaktif"), 0), sptField,
                     FxDB(drutama("painputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("painputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pamodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pamodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pacustomtext1"), ""), sptField,
                     FxDB(drutama("pacustomtext2"), ""), sptField,
                     FxDB(drutama("pacustomtext3"), ""), sptField,
                     FxDB(drutama("pacustomtext4"), ""), sptField,
                     FxDB(drutama("pacustomtext5"), ""), sptField,
                     FxDB(drutama("pacustomint1"), 0), sptField,
                     FxDB(drutama("pacustomint2"), 0), sptField,
                     FxDB(drutama("pacustomint3"), 0), sptField,
                     FxDB(drutama("pacustomdbl1"), 0), sptField,
                     FxDB(drutama("pacustomdbl2"), 0), sptField,
                     FxDB(drutama("pacustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pacustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pacustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pacustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("painputusernama"), ""), sptField,
                     FxDB(drutama("pamodifikasiusernama"), ""), sptField,
                     FxDB(drutama("pagudangbahan"), ""), sptField,
                     FxDB(drutama("pagudanghasil"), ""), sptField,
                     FxDB(drutama("pagudangbahannama"), ""), sptField,
                     FxDB(drutama("pagudanghasilnama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idpadetail"), ""), sptField,
                     FxDB(dr("idpa"), ""), sptField,
                     FxDB(dr("idbarang"), ""), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("hpppersen"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("rekpersediaan"), ""), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudangasal"), ""), sptField,
                     FxDB(dr("gudangproduksi"), ""), sptField,
                     FxDB(dr("gudangtujuan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idbom"), ""), sptField,
                     FxDB(dr("idbomin"), ""), sptField,
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
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptField,
                     FxDB(dr("basset"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("paid, pakode, panama, pacatatan, paaktif, painputuser, painputtgl, pamodifikasiuser, pamodifikasitgl, pacustomtext1, pacustomtext2, pacustomtext3, pacustomtext4, pacustomtext5, pacustomint1, pacustomint2, pacustomint3, pacustomdbl1, pacustomdbl2, pacustomdbl3, pacustomdate1, pacustomdate2, pacustomdate3, painputusernama, pamodifikasiusernama, pagudangbahan, pagudanghasil, pagudangbahannama, pagudanghasilnama" & sptSubParam & "idpadetail, idpa, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbom, idbomin, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, bjmllapangan, bsatuanlapangan, basset"))

        Return wsResult
    End Function

End Class