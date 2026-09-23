Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_pos_substitution_item
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_Pos_Substitution_ItemSimpan(ByVal param As String) As String
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

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean

        Dim pg1 As New RsPaging
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = ""

        '//FILTER KATEGORI POS UNTUK LOGOUT USER KATEGORI TERSEBUT, AGAR LOAD SETTING POS YG TERBARU
        Dim ftKategoriPOS As String = ""

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

        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'siid(0) As Integer, sikategori(1) As String, siidbarang(2) As Integer, sioperator(3) As String, sijml1(4) As Double, 
        'sijml2(5) As Double, sicustomtext1(6) As String, sicustomtext2(7) As String, sicustomtext3(8) As String, sicustomtext4(9) As String, 
        'sicustomtext5(10) As String, sicustomint1(11) As Integer, sicustomint2(12) As Integer, sicustomint3(13) As Integer, sicustomdbl1(14) As Double, 
        'sicustomdbl2(15) As Double, sicustomdbl3(16) As Double, sicustomdate1(17) As Date, sicustomdate2(18) As Date, sicustomdate3(19) As Date,
        'sitgl1(20) As Date, sitgl2(21) As Date, sinopromo(22) As String

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'siid, sikategori, siidbarang, sioperator, sijml1, sijml2, sicustomtext1, 
        'sicustomtext2, sicustomtext3, sicustomtext4, sicustomtext5, sicustomint1, sicustomint2, sicustomint3, 
        'sicustomdbl1, sicustomdbl2, sicustomdbl3, sicustomdate1, sicustomdate2, 
        'sitgl1, sitgl2, sinopromo


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 23) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'siid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "siid required numeric." : GoTo selesai
        End If
        'siidbarang(2) As Integer
        If (IsNumeric(dataUtama(2)) = False) Then
            result(2) = "siidbarang required numeric." : GoTo selesai
        End If
        'sijml1(4) As Double
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "sijml1 required numeric." : GoTo selesai
        End If
        'sijml2(5) As Double
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "sijml2 required numeric." : GoTo selesai
        End If
        'sicustomint1(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "sicustomint1 required numeric." : GoTo selesai
        End If
        'sicustomint2(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "sicustomint2 required numeric." : GoTo selesai
        End If
        'sicustomint3(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "sicustomint3 required numeric." : GoTo selesai
        End If
        'sicustomdbl1(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "sicustomdbl1 required numeric." : GoTo selesai
        End If
        'sicustomdbl2(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "sicustomdbl2 required numeric." : GoTo selesai
        End If
        'sicustomdbl3(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "sicustomdbl3 required numeric." : GoTo selesai
        End If
        'sicustomdate1(17) As Date
        If (IsDate(dataUtama(17)) = False) Then
            result(2) = "sicustomdate1 required date." : GoTo selesai
        End If
        'sicustomdate2(18) As Date
        If (IsDate(dataUtama(18)) = False) Then
            result(2) = "sicustomdate2 required date." : GoTo selesai
        End If
        'sicustomdate3(19) As Date
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "sicustomdate3 required date." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'siid(0) As Integer
        If Len(dataUtama(0)) = 0 Then
            result(2) = "siid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 20 Then
            result(2) = "siid should not be more than 20 character." : GoTo selesai
        End If

        'sikategori(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "sikategori can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "sikategori should not be more than 25 character." : GoTo selesai
        End If

        'siidbarang(2) As Integer
        If Len(dataUtama(2)) = 0 Then
            result(2) = "siidbarang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 20 Then
            result(2) = "siidbarang should not be more than 20 character." : GoTo selesai
        End If

        'sioperator(3) As String
        If IsNumeric(dataUtama(3)) = False Then
            result(2) = "sioperator required numeric" : GoTo selesai
        ElseIf dataUtama(3) <> 0 And dataUtama(3) <> 1 And dataUtama(3) <> 2 Then
            result(2) = "invalid sioperator value" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "sioperator should not be more than 25 character." : GoTo selesai
        End If

        'sijml1(4) As Double
        If Len(dataUtama(4)) = 0 Then
            result(2) = "sijml1 can't be empty" : GoTo selesai
        End If

        'sijml2(5) As Double
        If Len(dataUtama(5)) = 0 Then
            result(2) = "sijml2 can't be empty" : GoTo selesai
        End If

        'sicustomdbl1(14) As Double
        If Len(dataUtama(14)) = 0 Then
            result(2) = "sicustomdbl1 can't be empty" : GoTo selesai
        End If

        'sicustomdbl2(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "sicustomdbl2 can't be empty" : GoTo selesai
        End If

        'sicustomdbl3(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "sicustomdbl3 can't be empty" : GoTo selesai
        End If

        'sicustomdate1(17) As Date
        If Len(dataUtama(17)) = 0 Then
            result(2) = "sicustomdate1 can't be empty" : GoTo selesai
        End If

        'sicustomdate2(18) As Date
        If Len(dataUtama(18)) = 0 Then
            result(2) = "sicustomdate2 can't be empty" : GoTo selesai
        End If

        'sicustomdate3(19) As Date
        If Len(dataUtama(19)) = 0 Then
            result(2) = "sicustomdate3 can't be empty" : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "siid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sioperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sijml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sijml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sitgl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sitgl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sinopromo", AsEnumTypeData.AsString)

        If AsDataTableTambahData(dtutama, "siid~sikategori~siidbarang~sioperator~sijml1~sijml2~sicustomtext1~sicustomtext2~sicustomtext3~sicustomtext4~sicustomtext5~sicustomint1~sicustomint2~sicustomint3~sicustomdbl1~sicustomdbl2~sicustomdbl3~sicustomdate1~sicustomdate2~sicustomdate3~sitgl1~sitgl2~sinopromo", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idsidetail(0) As Integer, idsi(1) As Integer, idbarang(2) As Integer, jml(3) As Double, satuan(4) As String, 
        'customtext1(5) As String, customtext2(6) As String, customtext3(7) As String, customtext4(8) As String, customtext5(9) As String, 
        'customint1(10) As Integer, customint2(11) As Integer, customint3(12) As Integer, customdbl1(13) As Double, customdbl2(14) As Double, 
        'customdbl3(15) As Double, customdate1(16) As Date, customdate2(17) As Date, customdate3(18) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idsidetail, idsi, idbarang, jml, satuan, customtext1, customtext2, 
        'customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idsidetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idsi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint3", AsEnumTypeData.AsInt64)
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
            If (dataRowDetail.Length <> 19) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idsidetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idsidetail required numeric." : GoTo selesai
            End If
            'idsi(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idsi required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'jml(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'customint1(10) As Integer
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - customint1 required numeric." : GoTo selesai
            End If
            'customint2(11) As Integer
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - customint2 required numeric." : GoTo selesai
            End If
            'customint3(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - customint3 required numeric." : GoTo selesai
            End If
            'customdbl1(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(16) As Date
            If (IsDate(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(17) As Date
            If (IsDate(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(18) As Date
            If (IsDate(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'idsidetail(0) As Integer
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idsidetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idsidetail should not be more than 20 character." : GoTo selesai
            End If

            'idsi(1) As Integer
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idsi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idsi should not be more than 20 character." : GoTo selesai
            End If

            'idbarang(2) As 
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - idbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 20 Then
                result(2) = "Row : " & i & " - idbarang should not be more than 20 character." : GoTo selesai
            End If

            'jml(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - jml can't be empty" : GoTo selesai
            End If

            'satuan(4) As String
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(4)) > 25 Then
                result(2) = "Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'customdbl1(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(16) As Date
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(17) As Date
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(18) As Date
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idsidetail~idsi~idbarang~jml~satuan~customtext1~customtext2~customtext3~customtext4~customtext5~customint1~customint2~customint3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18)) = False Then
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

                'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drutama("sikategori")) & "' "

                'CEK OPERATOR :
                'JIKA BETWEEN (0) ATAU >= (1) MAKA BOLEH LEBIH DARI SATU KONDISI 
                '=> BEBERAPA KONDISI BETWEEN DAN SATU KONDISI >= (1)
                'JIKA KELIPATAN (2) MAKA HANYA BOLEH SATU KONDISI
                Dim dtOperator As New DataTable
                sql = "SELECT si.sikategori as kategori, si.siidbarang as idbarang, si.sioperator as operator, i.bkode, (CASE si.sioperator WHEN 0 THEN 'Between' WHEN 1 THEN '>=' WHEN 2 THEN 'Multiple' ELSE 'Unknown' END) as operatornama FROM m_12_pos_substitution_item si JOIN m1_item i ON si.siidbarang = i.bid WHERE si.sikategori = '" & FxDB(drutama("sikategori"), "") & "' AND si.siidbarang = '" & FxDB(drutama("siidbarang"), "") & "' AND si.siid <> '" & FxDB(drutama("siid"), "") & "' GROUP BY si.sioperator ORDER BY si.sioperator"
                dtOperator = AsDataTableAmbilDariDBCon(sql, myConn)
                If dtOperator.Rows.Count > 0 Then
                    Dim vOperator As String = ""
                    For Each dr1 As DataRow In dtOperator.Rows
                        vOperator = FxDB(dr1("operator").ToString, "")
                        If Len(vOperator) > 0 Then
                            If vOperator = 2 Then
                                'JIKA SUDAH TERDAPAT OPERATOR KELIPATAN (2)
                                result(2) = "Item : " & FxDB(dr1("bkode"), "") & " - already has '" & FxDB(dr1("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                            Else
                                'JIKA BELUM TERDAPAT OPERATOR KELIPATAN (2), CEK KONDISI OPERATOR YANG SUDAH DIINPUTKAN
                                'JIKA OPERATOR YANG DIINPUTKAN ADALAH KELIPATAN (2) MAKA TAMPILKAN ERRMESSAGE
                                'JIKA SUDAH TERDAPAT OPERATOR >= (1) DAN YANG DIINPUTKAN ADALAH OPERATOR >= (1) LAGI MAKA TAMPILKAN ERRMESSAGE
                                If drutama("sioperator") = 2 Or (vOperator = 1 And drutama("sioperator") = vOperator) Then
                                    result(2) = "Item : " & FxDB(dr1("bkode"), "") & " - already has '" & FxDB(dr1("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                End If
                            End If
                        End If
                    Next
                End If

                If isUpdate Then
                    result(4) = drutama("siid")
                    notransaksi = drutama("sikategori")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(siid) FROM M_12_Pos_Substitution_Item WHERE siid = '" & result(4) & "'", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then
                        sql = "Update M_12_Pos_Substitution_Item set sikategori  = '" & FixQuotes(drutama("sikategori")) & "', siidbarang  = '" & FixQuotes(drutama("siidbarang")) & "', sioperator  = '" & FixQuotes(drutama("sioperator")) & "', sijml1  = '" & FixDouble(drutama("sijml1")) & "', sijml2  = '" & FixDouble(drutama("sijml2")) & "', sicustomtext1  = '" & FixQuotes(drutama("sicustomtext1")) & "', sicustomtext2  = '" & FixQuotes(drutama("sicustomtext2")) & "', sicustomtext3  = '" & FixQuotes(drutama("sicustomtext3")) & "', sicustomtext4  = '" & FixQuotes(drutama("sicustomtext4")) & "', sicustomtext5  = '" & FixQuotes(drutama("sicustomtext5")) & "', sicustomint1  = " & drutama("sicustomint1") & ", sicustomint2  = " & drutama("sicustomint2") & ", sicustomint3  = " & drutama("sicustomint3") & ", sicustomdbl1  = '" & FixDouble(drutama("sicustomdbl1")) & "', sicustomdbl2  = '" & FixDouble(drutama("sicustomdbl2")) & "', sicustomdbl3  = '" & FixDouble(drutama("sicustomdbl3")) & "', sicustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("sicustomdate1"))) & "', sicustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("sicustomdate2"))) & "', sicustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("sicustomdate3"))) & "', sitgl1  = '" & FixQuotes(AsFormatTanggal(drutama("sitgl1"))) & "', sitgl2  = '" & FixQuotes(AsFormatTanggal(drutama("sitgl2"))) & "', sinopromo  = '" & FixQuotes(drutama("sinopromo")) & "' where siid = '" & drutama("siid") & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Else
                        result(2) = "Can't update '" & notransaksi & "' - Transaction data not found." : Trans.Rollback() : GoTo selesai
                    End If

                Else

                    sql = "Insert into M_12_Pos_Substitution_Item (sikategori, siidbarang, sioperator, sijml1, sijml2, sicustomtext1, sicustomtext2, sicustomtext3, sicustomtext4, sicustomtext5, sicustomint1, sicustomint2, sicustomint3, sicustomdbl1, sicustomdbl2, sicustomdbl3, sicustomdate1, sicustomdate2, sicustomdate3, sitgl1, sitgl2, sinopromo) values('" & FixQuotes(drutama("sikategori")) & "', '" & FixQuotes(drutama("siidbarang")) & "', '" & FixQuotes(drutama("sioperator")) & "', '" & FixDouble(drutama("sijml1")) & "', '" & FixDouble(drutama("sijml2")) & "', '" & FixQuotes(drutama("sicustomtext1")) & "', '" & FixQuotes(drutama("sicustomtext2")) & "', '" & FixQuotes(drutama("sicustomtext3")) & "', '" & FixQuotes(drutama("sicustomtext4")) & "', '" & FixQuotes(drutama("sicustomtext5")) & "', " & drutama("sicustomint1") & ", " & drutama("sicustomint2") & ", " & drutama("sicustomint3") & ", '" & FixDouble(drutama("sicustomdbl1")) & "', '" & FixDouble(drutama("sicustomdbl2")) & "', '" & FixDouble(drutama("sicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sicustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sitgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sitgl2"))) & "', '" & FixQuotes(drutama("sinopromo")) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select siid from M_12_Pos_Substitution_Item where sikategori = '" & drutama("sikategori") & "' AND siidbarang = '" & drutama("siidbarang") & "' AND sioperator = '" & drutama("sioperator") & "' AND sijml1 = '" & drutama("sijml1") & "' AND sijml2 = '" & drutama("sijml2") & "' limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M_12_Pos_Substitution_Item_Detail where idsi = '" & result(4) & "'"
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
                        strValue2.Append("('" & FixQuotes(dr1("idsidetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("idbarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M_12_Pos_Substitution_Item_Detail(idsidetail, idsi, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                'AMBIL DATA =============================================================
                Dim paramSearch As String = M12_Pos_Substitution_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Substitution_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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

            Else
                result(2) = "#1. Main transaction data not found." : Trans.Rollback() : GoTo selesai
            End If


            'PROSES LOGOUT USER =====================================================
            If Len(ftKategoriPOS) > 0 Then
                'USER YG LOGIN DILOGOUT AGAR SETTING POS TERLOAD ULANG
                sql = "SELECT ul.ulid, u.userid FROM m0_userlogin ul JOIN m0_user u ON ul.uluser = u.userid JOIN m1_location l ON u.ulokasi = l.lkode WHERE (" & ftKategoriPOS & ")"
                Dim dtUser As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                If dtUser.Rows.Count > 0 Then
                    Dim WsLogout As New m0_login
                    Dim rsLogout As String = ""
                    For Each drUser As DataRow In dtUser.Rows
                        'LOGOUT USER SESUAI KATEGORI POS YG DISETTING
                        rsLogout = WsLogout.M0_Logout(drUser("ulid") & "★M0_Logout★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & drUser("userid") & "★0★" & Application("AppCode") & "")
                    Next
                End If

            End If
            'END OF PROSES LOGOUT USER ==============================================


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
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_Substitution_ItemDelete(ByVal param As String) As String

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

        '//FILTER KATEGORI POS UNTUK LOGOUT USER KATEGORI TERSEBUT, AGAR LOAD SETTING POS YG TERBARU
        Dim ftKategoriPOS As String = ""

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

            'AMBIL KATEGORI POS TRANSAKSI UNTUK FILTER USER LOGIN
            sql = "SELECT sikategori as kategoripos FROM M_12_Pos_Substitution_Item WHERE siid = '" & idtransaksi & "' GROUP BY sikategori"
            Dim dtKategoriPOS As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtKategoriPOS.Rows.Count > 0 Then
                For Each drKategoriPOS As DataRow In dtKategoriPOS.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drKategoriPOS("kategoripos")) & "' "
                Next
            End If

            'DELETE DETAIL
            sql = "DELETE FROM M_12_Pos_Substitution_Item_Detail WHERE idsi = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M_12_Pos_Substitution_Item WHERE siid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_Pos_Substitution_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Substitution_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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


            'PROSES LOGOUT USER =====================================================
            If Len(ftKategoriPOS) > 0 Then
                'USER YG LOGIN DILOGOUT AGAR SETTING POS TERLOAD ULANG
                sql = "SELECT ul.ulid, u.userid FROM m0_userlogin ul JOIN m0_user u ON ul.uluser = u.userid JOIN m1_location l ON u.ulokasi = l.lkode WHERE (" & ftKategoriPOS & ")"
                Dim dtUser As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                If dtUser.Rows.Count > 0 Then
                    Dim WsLogout As New m0_login
                    Dim rsLogout As String = ""
                    For Each drUser As DataRow In dtUser.Rows
                        'LOGOUT USER SESUAI KATEGORI POS YG DISETTING
                        rsLogout = WsLogout.M0_Logout(drUser("ulid") & "★M0_Logout★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & drUser("userid") & "★0★" & Application("AppCode") & "")
                    Next
                End If

            End If
            'END OF PROSES LOGOUT USER ==============================================


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
    Public Function M12_Pos_Substitution_ItemImport(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataRowUtama(), dataDetail(), dataRowDetail() As String

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
        'siid(0) As Integer, sikategori(1) As String, siidbarang(2) As Integer, sioperator(3) As String, sijml1(4) As Double, 
        'sijml2(5) As Double, sicustomtext1(6) As String, sicustomtext2(7) As String, sicustomtext3(8) As String, sicustomtext4(9) As String, 
        'sicustomtext5(10) As String, sicustomint1(11) As Integer, sicustomint2(12) As Integer, sicustomint3(13) As Integer, sicustomdbl1(14) As Double, 
        'sicustomdbl2(15) As Double, sicustomdbl3(16) As Double, sicustomdate1(17) As Date, sicustomdate2(18) As Date, sicustomdate3(19) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'siid, sikategori, siidbarang, sioperator, sijml1, sijml2, sicustomtext1, 
        'sicustomtext2, sicustomtext3, sicustomtext4, sicustomtext5, sicustomint1, sicustomint2, sicustomint3, 
        'sicustomdbl1, sicustomdbl2, sicustomdbl3, sicustomdate1, sicustomdate2, sicustomdate3


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptRow)    'SPLIT PARAMETER DATA UTAMA
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "siid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sioperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sijml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sijml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomdate3", AsEnumTypeData.AsString)

        Dim JmlDtUtama As Integer = dataUtama.Length
        For i = 1 To JmlDtUtama
            'SPLIT DATA UTAMA
            dataRowUtama = dataUtama(i - 1).Split(sptField)

            'CEK ARRAY DATA UTAMA
            If (dataRowUtama.Length <> 20) Then
                result(2) = "Main Row : " & i & " - Invalid main transaction data parameter." : GoTo selesai
            End If

            'VALIDASI TIPE DATA UTAMA ==========================================================
            'siid(0) As Integer
            If (IsNumeric(dataRowUtama(0)) = False) Then
                result(2) = "Main Row : " & i & " - siid required numeric." : GoTo selesai
            End If
            'siidbarang(2) As Integer
            If (IsNumeric(dataRowUtama(2)) = False) Then
                result(2) = "Main Row : " & i & " - siidbarang required numeric." : GoTo selesai
            End If
            'sijml1(4) As Double
            If (IsNumeric(dataRowUtama(4)) = False) Then
                result(2) = "Main Row : " & i & " - sijml1 required numeric." : GoTo selesai
            End If
            'sijml2(5) As Double
            If (IsNumeric(dataRowUtama(5)) = False) Then
                result(2) = "Main Row : " & i & " - sijml2 required numeric." : GoTo selesai
            End If
            'sicustomint1(11) As Integer
            If (IsNumeric(dataRowUtama(11)) = False) Then
                result(2) = "Main Row : " & i & " - sicustomint1 required numeric." : GoTo selesai
            End If
            'sicustomint2(12) As Integer
            If (IsNumeric(dataRowUtama(12)) = False) Then
                result(2) = "Main Row : " & i & " - sicustomint2 required numeric." : GoTo selesai
            End If
            'sicustomint3(13) As Integer
            If (IsNumeric(dataRowUtama(13)) = False) Then
                result(2) = "Main Row : " & i & " - sicustomint3 required numeric." : GoTo selesai
            End If
            'sicustomdbl1(14) As Double
            If (IsNumeric(dataRowUtama(14)) = False) Then
                result(2) = "Main Row : " & i & " - sicustomdbl1 required numeric." : GoTo selesai
            End If
            'sicustomdbl2(15) As Double
            If (IsNumeric(dataRowUtama(15)) = False) Then
                result(2) = "Main Row : " & i & " - sicustomdbl2 required numeric." : GoTo selesai
            End If
            'sicustomdbl3(16) As Double
            If (IsNumeric(dataRowUtama(16)) = False) Then
                result(2) = "Main Row : " & i & " - sicustomdbl3 required numeric." : GoTo selesai
            End If
            'sicustomdate1(17) As Date
            If (IsDate(dataRowUtama(17)) = False) Then
                result(2) = "Main Row : " & i & " - sicustomdate1 required date." : GoTo selesai
            End If
            'sicustomdate2(18) As Date
            If (IsDate(dataRowUtama(18)) = False) Then
                result(2) = "Main Row : " & i & " - sicustomdate2 required date." : GoTo selesai
            End If
            'sicustomdate3(19) As Date
            If (IsDate(dataRowUtama(19)) = False) Then
                result(2) = "Main Row : " & i & " - sicustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA UTAMA ===================================================

            'VALIDASI DATA UTAMA =======================================================
            'siid(0) As Integer
            If Len(dataRowUtama(0)) = 0 Then
                result(2) = "Main Row : " & i & " - siid can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(0)) > 20 Then
                result(2) = "Main Row : " & i & " - siid should not be more than 20 character." : GoTo selesai
            End If

            'sikategori(1) As String
            If Len(dataRowUtama(1)) = 0 Then
                result(2) = "Main Row : " & i & " - sikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(1)) > 25 Then
                result(2) = "Main Row : " & i & " - sikategori should not be more than 25 character." : GoTo selesai
            End If

            'siidbarang(2) As Integer
            If Len(dataRowUtama(2)) = 0 Then
                result(2) = "Main Row : " & i & " - siidbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(2)) > 20 Then
                result(2) = "Main Row : " & i & " - siidbarang should not be more than 20 character." : GoTo selesai
            End If

            'sioperator(3) As String
            If IsNumeric(dataRowUtama(3)) = False Then
                result(2) = "Main Row : " & i & " - sioperator required numeric" : GoTo selesai
            ElseIf dataRowUtama(3) <> 0 And dataRowUtama(3) <> 1 And dataRowUtama(3) <> 2 Then
                result(2) = "Main Row : " & i & " - invalid sioperator value" : GoTo selesai
            End If
            If Len(dataRowUtama(3)) > 25 Then
                result(2) = "Main Row : " & i & " - sioperator should not be more than 25 character." : GoTo selesai
            End If

            'sijml1(4) As Double
            If Len(dataRowUtama(4)) = 0 Then
                result(2) = "Main Row : " & i & " - sijml1 can't be empty" : GoTo selesai
            End If

            'sijml2(5) As Double
            If Len(dataRowUtama(5)) = 0 Then
                result(2) = "Main Row : " & i & " - sijml2 can't be empty" : GoTo selesai
            End If

            'sicustomdbl1(14) As Double
            If Len(dataRowUtama(14)) = 0 Then
                result(2) = "Main Row : " & i & " - sicustomdbl1 can't be empty" : GoTo selesai
            End If

            'sicustomdbl2(15) As Double
            If Len(dataRowUtama(15)) = 0 Then
                result(2) = "Main Row : " & i & " - sicustomdbl2 can't be empty" : GoTo selesai
            End If

            'sicustomdbl3(16) As Double
            If Len(dataRowUtama(16)) = 0 Then
                result(2) = "Main Row : " & i & " - sicustomdbl3 can't be empty" : GoTo selesai
            End If

            'sicustomdate1(17) As Date
            If Len(dataRowUtama(17)) = 0 Then
                result(2) = "Main Row : " & i & " - sicustomdate1 can't be empty" : GoTo selesai
            End If

            'sicustomdate2(18) As Date
            If Len(dataRowUtama(18)) = 0 Then
                result(2) = "Main Row : " & i & " - sicustomdate2 can't be empty" : GoTo selesai
            End If

            'sicustomdate3(19) As Date
            If Len(dataRowUtama(19)) = 0 Then
                result(2) = "Main Row : " & i & " - sicustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA UTAMA ================================================


            If AsDataTableTambahData(dtutama, "Main Row : " & i & " - siid~sikategori~siidbarang~sioperator~sijml1~sijml2~sicustomtext1~sicustomtext2~sicustomtext3~sicustomtext4~sicustomtext5~sicustomint1~sicustomint2~sicustomint3~sicustomdbl1~sicustomdbl2~sicustomdbl3~sicustomdate1~sicustomdate2~sicustomdate3", dataRowUtama(0) & "~" & dataRowUtama(1) & "~" & dataRowUtama(2) & "~" & dataRowUtama(3) & "~" & dataRowUtama(4) & "~" & dataRowUtama(5) & "~" & dataRowUtama(6) & "~" & dataRowUtama(7) & "~" & dataRowUtama(8) & "~" & dataRowUtama(9) & "~" & dataRowUtama(10) & "~" & dataRowUtama(11) & "~" & dataRowUtama(12) & "~" & dataRowUtama(13) & "~" & dataRowUtama(14) & "~" & dataRowUtama(15) & "~" & dataRowUtama(16) & "~" & dataRowUtama(17) & "~" & dataRowUtama(18) & "~" & dataRowUtama(19)) = False Then
                result(2) = "Main Row : " & i & " - Insert into main datatable failed." : GoTo selesai
            End If

        Next


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idsidetail(0) As Integer, idsi(1) As Integer, idbarang(2) As Integer, jml(3) As Double, satuan(4) As String, 
        'customtext1(5) As String, customtext2(6) As String, customtext3(7) As String, customtext4(8) As String, customtext5(9) As String, 
        'customint1(10) As Integer, customint2(11) As Integer, customint3(12) As Integer, customdbl1(13) As Double, customdbl2(14) As Double, 
        'customdbl3(15) As Double, customdate1(16) As Date, customdate2(17) As Date, customdate3(18) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idsidetail, idsi, idbarang, jml, satuan, customtext1, customtext2, 
        'customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idsidetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idsi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint3", AsEnumTypeData.AsInt64)
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
            If (dataRowDetail.Length <> 19) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idsidetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idsidetail required numeric." : GoTo selesai
            End If
            'idsi(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idsi required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'jml(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'customint1(10) As Integer
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - customint1 required numeric." : GoTo selesai
            End If
            'customint2(11) As Integer
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - customint2 required numeric." : GoTo selesai
            End If
            'customint3(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - customint3 required numeric." : GoTo selesai
            End If
            'customdbl1(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(16) As Date
            If (IsDate(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(17) As Date
            If (IsDate(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(18) As Date
            If (IsDate(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'idsidetail(0) As Integer
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idsidetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idsidetail should not be more than 20 character." : GoTo selesai
            End If

            'idsi(1) As Integer
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idsi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idsi should not be more than 20 character." : GoTo selesai
            End If

            'idbarang(2) As 
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - idbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 20 Then
                result(2) = "Row : " & i & " - idbarang should not be more than 20 character." : GoTo selesai
            End If

            'jml(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - jml can't be empty" : GoTo selesai
            End If

            'satuan(4) As String
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(4)) > 25 Then
                result(2) = "Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'customdbl1(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(16) As Date
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(17) As Date
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(18) As Date
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idsidetail~idsi~idbarang~jml~satuan~customtext1~customtext2~customtext3~customtext4~customtext5~customint1~customint2~customint3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18)) = False Then
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
                'Hapus utama
                If (isUpdate) Then
                    sql = "Delete from M_12_Pos_Substitution_Item"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Hapus detail
                If (isUpdate) Then
                    sql = "Delete from M_12_Pos_Substitution_Item_Detail"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                'Proses utama
                Dim strValue1 As New StringBuilder
                For Each dr1 As DataRow In dtutama.Rows
                    strValue1.Append(IIf(Len(strValue1.ToString) = 0, "", ", "))
                    strValue1.Append("('" & FixQuotes(dr1("siid")) & "', '" & FixQuotes(dr1("sikategori")) & "', '" & FixQuotes(dr1("siidbarang")) & "', '" & FixQuotes(dr1("sioperator")) & "', '" & FixDouble(dr1("sijml1")) & "', '" & FixDouble(dr1("sijml2")) & "', '" & FixQuotes(dr1("sicustomtext1")) & "', '" & FixQuotes(dr1("sicustomtext2")) & "', '" & FixQuotes(dr1("sicustomtext3")) & "', '" & FixQuotes(dr1("sicustomtext4")) & "', '" & FixQuotes(dr1("sicustomtext5")) & "', " & dr1("sicustomint1") & ", " & dr1("sicustomint2") & ", " & dr1("sicustomint3") & ", '" & FixDouble(dr1("sicustomdbl1")) & "', '" & FixDouble(dr1("sicustomdbl2")) & "', '" & FixDouble(dr1("sicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("sicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("sicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("sicustomdate3"))) & "')")
                Next
                sql = "Insert into M_12_Pos_Substitution_Item(siid, sikategori, siidbarang, sioperator, sijml1, sijml2, sicustomtext1, sicustomtext2, sicustomtext3, sicustomtext4, sicustomtext5, sicustomint1, sicustomint2, sicustomint3, sicustomdbl1, sicustomdbl2, sicustomdbl3, sicustomdate1, sicustomdate2, sicustomdate3) values" & strValue1.ToString & ""
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()


                'Proses detail
                If (dtdetail.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("idsidetail")) & "', '" & FixQuotes(dr1("idsi")) & "', '" & FixQuotes(dr1("idbarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M_12_Pos_Substitution_Item_Detail(idsidetail, idsi, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
    Public Function M12_Pos_Substitution_ItemSimpanOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean

        Dim pg1 As New RsPaging
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = ""

        '//FILTER KATEGORI POS UNTUK LOGOUT USER KATEGORI TERSEBUT, AGAR LOAD SETTING POS YG TERBARU
        Dim ftKategoriPOS As String = ""

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

        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'siid(0) As Integer, sikategori(1) As String, siidbarang(2) As Integer, sioperator(3) As String, sijml1(4) As Double, 
        'sijml2(5) As Double, sicustomtext1(6) As String, sicustomtext2(7) As String, sicustomtext3(8) As String, sicustomtext4(9) As String, 
        'sicustomtext5(10) As String, sicustomint1(11) As Integer, sicustomint2(12) As Integer, sicustomint3(13) As Integer, sicustomdbl1(14) As Double, 
        'sicustomdbl2(15) As Double, sicustomdbl3(16) As Double, sicustomdate1(17) As Date, sicustomdate2(18) As Date, sicustomdate3(19) As Date,
        'sitgl1(20) As Date, sitgl2(21) As Date, sinopromo(22) As String

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'siid, sikategori, siidbarang, sioperator, sijml1, sijml2, sicustomtext1, 
        'sicustomtext2, sicustomtext3, sicustomtext4, sicustomtext5, sicustomint1, sicustomint2, sicustomint3, 
        'sicustomdbl1, sicustomdbl2, sicustomdbl3, sicustomdate1, sicustomdate2, 
        'sitgl1, sitgl2, sinopromo


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 23) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'siid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "siid required numeric." : GoTo selesai
        End If
        'siidbarang(2) As Integer
        If (IsNumeric(dataUtama(2)) = False) Then
            result(2) = "siidbarang required numeric." : GoTo selesai
        End If
        'sijml1(4) As Double
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "sijml1 required numeric." : GoTo selesai
        End If
        'sijml2(5) As Double
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "sijml2 required numeric." : GoTo selesai
        End If
        'sicustomint1(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "sicustomint1 required numeric." : GoTo selesai
        End If
        'sicustomint2(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "sicustomint2 required numeric." : GoTo selesai
        End If
        'sicustomint3(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "sicustomint3 required numeric." : GoTo selesai
        End If
        'sicustomdbl1(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "sicustomdbl1 required numeric." : GoTo selesai
        End If
        'sicustomdbl2(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "sicustomdbl2 required numeric." : GoTo selesai
        End If
        'sicustomdbl3(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "sicustomdbl3 required numeric." : GoTo selesai
        End If
        'sicustomdate1(17) As Date
        If (IsDate(dataUtama(17)) = False) Then
            result(2) = "sicustomdate1 required date." : GoTo selesai
        End If
        'sicustomdate2(18) As Date
        If (IsDate(dataUtama(18)) = False) Then
            result(2) = "sicustomdate2 required date." : GoTo selesai
        End If
        'sicustomdate3(19) As Date
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "sicustomdate3 required date." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'siid(0) As Integer
        If Len(dataUtama(0)) = 0 Then
            result(2) = "siid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 20 Then
            result(2) = "siid should not be more than 20 character." : GoTo selesai
        End If

        'sikategori(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "sikategori can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "sikategori should not be more than 25 character." : GoTo selesai
        End If

        'siidbarang(2) As Integer
        If Len(dataUtama(2)) = 0 Then
            result(2) = "siidbarang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 20 Then
            result(2) = "siidbarang should not be more than 20 character." : GoTo selesai
        End If

        'sioperator(3) As String
        If IsNumeric(dataUtama(3)) = False Then
            result(2) = "sioperator required numeric" : GoTo selesai
        ElseIf dataUtama(3) <> 0 And dataUtama(3) <> 1 And dataUtama(3) <> 2 Then
            result(2) = "invalid sioperator value" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "sioperator should not be more than 25 character." : GoTo selesai
        End If

        'sijml1(4) As Double
        If Len(dataUtama(4)) = 0 Then
            result(2) = "sijml1 can't be empty" : GoTo selesai
        End If

        'sijml2(5) As Double
        If Len(dataUtama(5)) = 0 Then
            result(2) = "sijml2 can't be empty" : GoTo selesai
        End If

        'sicustomdbl1(14) As Double
        If Len(dataUtama(14)) = 0 Then
            result(2) = "sicustomdbl1 can't be empty" : GoTo selesai
        End If

        'sicustomdbl2(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "sicustomdbl2 can't be empty" : GoTo selesai
        End If

        'sicustomdbl3(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "sicustomdbl3 can't be empty" : GoTo selesai
        End If

        'sicustomdate1(17) As Date
        If Len(dataUtama(17)) = 0 Then
            result(2) = "sicustomdate1 can't be empty" : GoTo selesai
        End If

        'sicustomdate2(18) As Date
        If Len(dataUtama(18)) = 0 Then
            result(2) = "sicustomdate2 can't be empty" : GoTo selesai
        End If

        'sicustomdate3(19) As Date
        If Len(dataUtama(19)) = 0 Then
            result(2) = "sicustomdate3 can't be empty" : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "siid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sioperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sijml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sijml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sitgl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sitgl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sinopromo", AsEnumTypeData.AsString)

        If AsDataTableTambahData(dtutama, "siid~sikategori~siidbarang~sioperator~sijml1~sijml2~sicustomtext1~sicustomtext2~sicustomtext3~sicustomtext4~sicustomtext5~sicustomint1~sicustomint2~sicustomint3~sicustomdbl1~sicustomdbl2~sicustomdbl3~sicustomdate1~sicustomdate2~sicustomdate3~sitgl1~sitgl2~sinopromo", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idsidetail(0) As Integer, idsi(1) As Integer, idbarang(2) As Integer, jml(3) As Double, satuan(4) As String, 
        'customtext1(5) As String, customtext2(6) As String, customtext3(7) As String, customtext4(8) As String, customtext5(9) As String, 
        'customint1(10) As Integer, customint2(11) As Integer, customint3(12) As Integer, customdbl1(13) As Double, customdbl2(14) As Double, 
        'customdbl3(15) As Double, customdate1(16) As Date, customdate2(17) As Date, customdate3(18) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idsidetail, idsi, idbarang, jml, satuan, customtext1, customtext2, 
        'customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idsidetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idsi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint3", AsEnumTypeData.AsInt64)
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
            If (dataRowDetail.Length <> 19) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idsidetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idsidetail required numeric." : GoTo selesai
            End If
            'idsi(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idsi required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'jml(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'customint1(10) As Integer
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - customint1 required numeric." : GoTo selesai
            End If
            'customint2(11) As Integer
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - customint2 required numeric." : GoTo selesai
            End If
            'customint3(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - customint3 required numeric." : GoTo selesai
            End If
            'customdbl1(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(16) As Date
            If (IsDate(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(17) As Date
            If (IsDate(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(18) As Date
            If (IsDate(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'idsidetail(0) As Integer
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idsidetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idsidetail should not be more than 20 character." : GoTo selesai
            End If

            'idsi(1) As Integer
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idsi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idsi should not be more than 20 character." : GoTo selesai
            End If

            'idbarang(2) As 
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - idbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 20 Then
                result(2) = "Row : " & i & " - idbarang should not be more than 20 character." : GoTo selesai
            End If

            'jml(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - jml can't be empty" : GoTo selesai
            End If

            'satuan(4) As String
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(4)) > 25 Then
                result(2) = "Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'customdbl1(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(16) As Date
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(17) As Date
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(18) As Date
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idsidetail~idsi~idbarang~jml~satuan~customtext1~customtext2~customtext3~customtext4~customtext5~customint1~customint2~customint3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18)) = False Then
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

                'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drutama("sikategori")) & "' "

                'CEK OPERATOR :
                'JIKA BETWEEN (0) ATAU >= (1) MAKA BOLEH LEBIH DARI SATU KONDISI 
                '=> BEBERAPA KONDISI BETWEEN DAN SATU KONDISI >= (1)
                'JIKA KELIPATAN (2) MAKA HANYA BOLEH SATU KONDISI
                Dim dtOperator As New DataTable
                sql = "SELECT si.sikategori as kategori, si.siidbarang as idbarang, si.sioperator as operator, i.bkode, (CASE si.sioperator WHEN 0 THEN 'Between' WHEN 1 THEN '>=' WHEN 2 THEN 'Multiple' ELSE 'Unknown' END) as operatornama FROM m_12_pos_substitution_item si JOIN m1_item i ON si.siidbarang = i.bid WHERE si.sikategori = '" & FxDB(drutama("sikategori"), "") & "' AND si.siidbarang = '" & FxDB(drutama("siidbarang"), "") & "' AND si.siid <> '" & FxDB(drutama("siid"), "") & "' GROUP BY si.sioperator ORDER BY si.sioperator"
                dtOperator = AsDataTableAmbilDariDB(sql)
                If dtOperator.Rows.Count > 0 Then
                    Dim vOperator As String = ""
                    For Each dr1 As DataRow In dtOperator.Rows
                        vOperator = FxDB(dr1("operator").ToString, "")
                        If Len(vOperator) > 0 Then
                            If vOperator = 2 Then
                                'JIKA SUDAH TERDAPAT OPERATOR KELIPATAN (2)
                                result(2) = "Item : " & FxDB(dr1("bkode"), "") & " - already has '" & FxDB(dr1("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                            Else
                                'JIKA BELUM TERDAPAT OPERATOR KELIPATAN (2), CEK KONDISI OPERATOR YANG SUDAH DIINPUTKAN
                                'JIKA OPERATOR YANG DIINPUTKAN ADALAH KELIPATAN (2) MAKA TAMPILKAN ERRMESSAGE
                                'JIKA SUDAH TERDAPAT OPERATOR >= (1) DAN YANG DIINPUTKAN ADALAH OPERATOR >= (1) LAGI MAKA TAMPILKAN ERRMESSAGE
                                If drutama("sioperator") = 2 Or (vOperator = 1 And drutama("sioperator") = vOperator) Then
                                    result(2) = "Item : " & FxDB(dr1("bkode"), "") & " - already has '" & FxDB(dr1("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                End If
                            End If
                        End If
                    Next
                End If

                If isUpdate Then
                    result(4) = drutama("siid")
                    notransaksi = drutama("sikategori")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(siid) FROM M_12_Pos_Substitution_Item WHERE siid = '" & result(4) & "'")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then
                        sql = "Update M_12_Pos_Substitution_Item set sikategori  = '" & FixQuotes(drutama("sikategori")) & "', siidbarang  = '" & FixQuotes(drutama("siidbarang")) & "', sioperator  = '" & FixQuotes(drutama("sioperator")) & "', sijml1  = '" & FixDouble(drutama("sijml1")) & "', sijml2  = '" & FixDouble(drutama("sijml2")) & "', sicustomtext1  = '" & FixQuotes(drutama("sicustomtext1")) & "', sicustomtext2  = '" & FixQuotes(drutama("sicustomtext2")) & "', sicustomtext3  = '" & FixQuotes(drutama("sicustomtext3")) & "', sicustomtext4  = '" & FixQuotes(drutama("sicustomtext4")) & "', sicustomtext5  = '" & FixQuotes(drutama("sicustomtext5")) & "', sicustomint1  = " & drutama("sicustomint1") & ", sicustomint2  = " & drutama("sicustomint2") & ", sicustomint3  = " & drutama("sicustomint3") & ", sicustomdbl1  = '" & FixDouble(drutama("sicustomdbl1")) & "', sicustomdbl2  = '" & FixDouble(drutama("sicustomdbl2")) & "', sicustomdbl3  = '" & FixDouble(drutama("sicustomdbl3")) & "', sicustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("sicustomdate1"))) & "', sicustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("sicustomdate2"))) & "', sicustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("sicustomdate3"))) & "', sitgl1  = '" & FixQuotes(AsFormatTanggal(drutama("sitgl1"))) & "', sitgl2  = '" & FixQuotes(AsFormatTanggal(drutama("sitgl2"))) & "', sinopromo  = '" & FixQuotes(drutama("sinopromo")) & "' where siid = '" & drutama("siid") & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Else
                        result(2) = "Can't update '" & notransaksi & "' - Transaction data not found." : Trans.Rollback() : GoTo selesai
                    End If

                Else

                    sql = "Insert into M_12_Pos_Substitution_Item (sikategori, siidbarang, sioperator, sijml1, sijml2, sicustomtext1, sicustomtext2, sicustomtext3, sicustomtext4, sicustomtext5, sicustomint1, sicustomint2, sicustomint3, sicustomdbl1, sicustomdbl2, sicustomdbl3, sicustomdate1, sicustomdate2, sicustomdate3, sitgl1, sitgl2, sinopromo) values('" & FixQuotes(drutama("sikategori")) & "', '" & FixQuotes(drutama("siidbarang")) & "', '" & FixQuotes(drutama("sioperator")) & "', '" & FixDouble(drutama("sijml1")) & "', '" & FixDouble(drutama("sijml2")) & "', '" & FixQuotes(drutama("sicustomtext1")) & "', '" & FixQuotes(drutama("sicustomtext2")) & "', '" & FixQuotes(drutama("sicustomtext3")) & "', '" & FixQuotes(drutama("sicustomtext4")) & "', '" & FixQuotes(drutama("sicustomtext5")) & "', " & drutama("sicustomint1") & ", " & drutama("sicustomint2") & ", " & drutama("sicustomint3") & ", '" & FixDouble(drutama("sicustomdbl1")) & "', '" & FixDouble(drutama("sicustomdbl2")) & "', '" & FixDouble(drutama("sicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sicustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sitgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sitgl2"))) & "', '" & FixQuotes(drutama("sinopromo")) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select siid from M_12_Pos_Substitution_Item where sikategori = '" & drutama("sikategori") & "' AND siidbarang = '" & drutama("siidbarang") & "' AND sioperator = '" & drutama("sioperator") & "' AND sijml1 = '" & drutama("sijml1") & "' AND sijml2 = '" & drutama("sijml2") & "' limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M_12_Pos_Substitution_Item_Detail where idsi = '" & result(4) & "'"
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
                        strValue2.Append("('" & FixQuotes(dr1("idsidetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("idbarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M_12_Pos_Substitution_Item_Detail(idsidetail, idsi, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                Trans.Commit()  '*** Commit Transaction ***'
                result(1) = 1
                result(2) = notransaksi
                result(3) = 0
                result(4) = result(4)

                'AMBIL DATA =============================================================
                Dim paramSearch As String = M12_Pos_Substitution_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Substitution_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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

            Else
                result(2) = "#1. Main transaction data not found." : Trans.Rollback() : GoTo selesai
            End If


            'PROSES LOGOUT USER =====================================================
            If Len(ftKategoriPOS) > 0 Then
                'USER YG LOGIN DILOGOUT AGAR SETTING POS TERLOAD ULANG
                sql = "SELECT ul.ulid, u.userid FROM m0_userlogin ul JOIN m0_user u ON ul.uluser = u.userid JOIN m1_location l ON u.ulokasi = l.lkode WHERE (" & ftKategoriPOS & ")"
                Dim dtUser As DataTable = AsDataTableAmbilDariDB(sql)
                If dtUser.Rows.Count > 0 Then
                    Dim WsLogout As New m0_login
                    Dim rsLogout As String = ""
                    For Each drUser As DataRow In dtUser.Rows
                        'LOGOUT USER SESUAI KATEGORI POS YG DISETTING
                        rsLogout = WsLogout.M0_Logout(drUser("ulid") & "★M0_Logout★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & drUser("userid") & "★0★" & Application("AppCode") & "")
                    Next
                End If

            End If
            'END OF PROSES LOGOUT USER ==============================================


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
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_Substitution_ItemDeleteOld(ByVal param As String) As String

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

        '//FILTER KATEGORI POS UNTUK LOGOUT USER KATEGORI TERSEBUT, AGAR LOAD SETTING POS YG TERBARU
        Dim ftKategoriPOS As String = ""

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
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)
        Try

            'AMBIL KATEGORI POS TRANSAKSI UNTUK FILTER USER LOGIN
            sql = "SELECT sikategori as kategoripos FROM M_12_Pos_Substitution_Item WHERE siid = '" & idtransaksi & "' GROUP BY sikategori"
            Dim dtKategoriPOS As DataTable = AsDataTableAmbilDariDB(sql)
            If dtKategoriPOS.Rows.Count > 0 Then
                For Each drKategoriPOS As DataRow In dtKategoriPOS.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drKategoriPOS("kategoripos")) & "' "
                Next
            End If

            'DELETE DETAIL
            sql = "DELETE FROM M_12_Pos_Substitution_Item_Detail WHERE idsi = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M_12_Pos_Substitution_Item WHERE siid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
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
            Dim paramSearch As String = M12_Pos_Substitution_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Substitution_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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


            'PROSES LOGOUT USER =====================================================
            If Len(ftKategoriPOS) > 0 Then
                'USER YG LOGIN DILOGOUT AGAR SETTING POS TERLOAD ULANG
                sql = "SELECT ul.ulid, u.userid FROM m0_userlogin ul JOIN m0_user u ON ul.uluser = u.userid JOIN m1_location l ON u.ulokasi = l.lkode WHERE (" & ftKategoriPOS & ")"
                Dim dtUser As DataTable = AsDataTableAmbilDariDB(sql)
                If dtUser.Rows.Count > 0 Then
                    Dim WsLogout As New m0_login
                    Dim rsLogout As String = ""
                    For Each drUser As DataRow In dtUser.Rows
                        'LOGOUT USER SESUAI KATEGORI POS YG DISETTING
                        rsLogout = WsLogout.M0_Logout(drUser("ulid") & "★M0_Logout★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & drUser("userid") & "★0★" & Application("AppCode") & "")
                    Next
                End If

            End If
            'END OF PROSES LOGOUT USER ==============================================


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
    Public Function M12_Pos_Substitution_ItemImportOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataRowUtama(), dataDetail(), dataRowDetail() As String

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
        'siid(0) As Integer, sikategori(1) As String, siidbarang(2) As Integer, sioperator(3) As String, sijml1(4) As Double, 
        'sijml2(5) As Double, sicustomtext1(6) As String, sicustomtext2(7) As String, sicustomtext3(8) As String, sicustomtext4(9) As String, 
        'sicustomtext5(10) As String, sicustomint1(11) As Integer, sicustomint2(12) As Integer, sicustomint3(13) As Integer, sicustomdbl1(14) As Double, 
        'sicustomdbl2(15) As Double, sicustomdbl3(16) As Double, sicustomdate1(17) As Date, sicustomdate2(18) As Date, sicustomdate3(19) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'siid, sikategori, siidbarang, sioperator, sijml1, sijml2, sicustomtext1, 
        'sicustomtext2, sicustomtext3, sicustomtext4, sicustomtext5, sicustomint1, sicustomint2, sicustomint3, 
        'sicustomdbl1, sicustomdbl2, sicustomdbl3, sicustomdate1, sicustomdate2, sicustomdate3


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptRow)    'SPLIT PARAMETER DATA UTAMA
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "siid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "siidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sioperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sijml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sijml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sicustomdate3", AsEnumTypeData.AsString)

        Dim JmlDtUtama As Integer = dataUtama.Length
        For i = 1 To JmlDtUtama
            'SPLIT DATA UTAMA
            dataRowUtama = dataUtama(i - 1).Split(sptField)

            'CEK ARRAY DATA UTAMA
            If (dataRowUtama.Length <> 20) Then
                result(2) = "Main Row : " & i & " - Invalid main transaction data parameter." : GoTo selesai
            End If

            'VALIDASI TIPE DATA UTAMA ==========================================================
            'siid(0) As Integer
            If (IsNumeric(dataRowUtama(0)) = False) Then
                result(2) = "Main Row : " & i & " - siid required numeric." : GoTo selesai
            End If
            'siidbarang(2) As Integer
            If (IsNumeric(dataRowUtama(2)) = False) Then
                result(2) = "Main Row : " & i & " - siidbarang required numeric." : GoTo selesai
            End If
            'sijml1(4) As Double
            If (IsNumeric(dataRowUtama(4)) = False) Then
                result(2) = "Main Row : " & i & " - sijml1 required numeric." : GoTo selesai
            End If
            'sijml2(5) As Double
            If (IsNumeric(dataRowUtama(5)) = False) Then
                result(2) = "Main Row : " & i & " - sijml2 required numeric." : GoTo selesai
            End If
            'sicustomint1(11) As Integer
            If (IsNumeric(dataRowUtama(11)) = False) Then
                result(2) = "Main Row : " & i & " - sicustomint1 required numeric." : GoTo selesai
            End If
            'sicustomint2(12) As Integer
            If (IsNumeric(dataRowUtama(12)) = False) Then
                result(2) = "Main Row : " & i & " - sicustomint2 required numeric." : GoTo selesai
            End If
            'sicustomint3(13) As Integer
            If (IsNumeric(dataRowUtama(13)) = False) Then
                result(2) = "Main Row : " & i & " - sicustomint3 required numeric." : GoTo selesai
            End If
            'sicustomdbl1(14) As Double
            If (IsNumeric(dataRowUtama(14)) = False) Then
                result(2) = "Main Row : " & i & " - sicustomdbl1 required numeric." : GoTo selesai
            End If
            'sicustomdbl2(15) As Double
            If (IsNumeric(dataRowUtama(15)) = False) Then
                result(2) = "Main Row : " & i & " - sicustomdbl2 required numeric." : GoTo selesai
            End If
            'sicustomdbl3(16) As Double
            If (IsNumeric(dataRowUtama(16)) = False) Then
                result(2) = "Main Row : " & i & " - sicustomdbl3 required numeric." : GoTo selesai
            End If
            'sicustomdate1(17) As Date
            If (IsDate(dataRowUtama(17)) = False) Then
                result(2) = "Main Row : " & i & " - sicustomdate1 required date." : GoTo selesai
            End If
            'sicustomdate2(18) As Date
            If (IsDate(dataRowUtama(18)) = False) Then
                result(2) = "Main Row : " & i & " - sicustomdate2 required date." : GoTo selesai
            End If
            'sicustomdate3(19) As Date
            If (IsDate(dataRowUtama(19)) = False) Then
                result(2) = "Main Row : " & i & " - sicustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA UTAMA ===================================================

            'VALIDASI DATA UTAMA =======================================================
            'siid(0) As Integer
            If Len(dataRowUtama(0)) = 0 Then
                result(2) = "Main Row : " & i & " - siid can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(0)) > 20 Then
                result(2) = "Main Row : " & i & " - siid should not be more than 20 character." : GoTo selesai
            End If

            'sikategori(1) As String
            If Len(dataRowUtama(1)) = 0 Then
                result(2) = "Main Row : " & i & " - sikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(1)) > 25 Then
                result(2) = "Main Row : " & i & " - sikategori should not be more than 25 character." : GoTo selesai
            End If

            'siidbarang(2) As Integer
            If Len(dataRowUtama(2)) = 0 Then
                result(2) = "Main Row : " & i & " - siidbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(2)) > 20 Then
                result(2) = "Main Row : " & i & " - siidbarang should not be more than 20 character." : GoTo selesai
            End If

            'sioperator(3) As String
            If IsNumeric(dataRowUtama(3)) = False Then
                result(2) = "Main Row : " & i & " - sioperator required numeric" : GoTo selesai
            ElseIf dataRowUtama(3) <> 0 And dataRowUtama(3) <> 1 And dataRowUtama(3) <> 2 Then
                result(2) = "Main Row : " & i & " - invalid sioperator value" : GoTo selesai
            End If
            If Len(dataRowUtama(3)) > 25 Then
                result(2) = "Main Row : " & i & " - sioperator should not be more than 25 character." : GoTo selesai
            End If

            'sijml1(4) As Double
            If Len(dataRowUtama(4)) = 0 Then
                result(2) = "Main Row : " & i & " - sijml1 can't be empty" : GoTo selesai
            End If

            'sijml2(5) As Double
            If Len(dataRowUtama(5)) = 0 Then
                result(2) = "Main Row : " & i & " - sijml2 can't be empty" : GoTo selesai
            End If

            'sicustomdbl1(14) As Double
            If Len(dataRowUtama(14)) = 0 Then
                result(2) = "Main Row : " & i & " - sicustomdbl1 can't be empty" : GoTo selesai
            End If

            'sicustomdbl2(15) As Double
            If Len(dataRowUtama(15)) = 0 Then
                result(2) = "Main Row : " & i & " - sicustomdbl2 can't be empty" : GoTo selesai
            End If

            'sicustomdbl3(16) As Double
            If Len(dataRowUtama(16)) = 0 Then
                result(2) = "Main Row : " & i & " - sicustomdbl3 can't be empty" : GoTo selesai
            End If

            'sicustomdate1(17) As Date
            If Len(dataRowUtama(17)) = 0 Then
                result(2) = "Main Row : " & i & " - sicustomdate1 can't be empty" : GoTo selesai
            End If

            'sicustomdate2(18) As Date
            If Len(dataRowUtama(18)) = 0 Then
                result(2) = "Main Row : " & i & " - sicustomdate2 can't be empty" : GoTo selesai
            End If

            'sicustomdate3(19) As Date
            If Len(dataRowUtama(19)) = 0 Then
                result(2) = "Main Row : " & i & " - sicustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA UTAMA ================================================


            If AsDataTableTambahData(dtutama, "Main Row : " & i & " - siid~sikategori~siidbarang~sioperator~sijml1~sijml2~sicustomtext1~sicustomtext2~sicustomtext3~sicustomtext4~sicustomtext5~sicustomint1~sicustomint2~sicustomint3~sicustomdbl1~sicustomdbl2~sicustomdbl3~sicustomdate1~sicustomdate2~sicustomdate3", dataRowUtama(0) & "~" & dataRowUtama(1) & "~" & dataRowUtama(2) & "~" & dataRowUtama(3) & "~" & dataRowUtama(4) & "~" & dataRowUtama(5) & "~" & dataRowUtama(6) & "~" & dataRowUtama(7) & "~" & dataRowUtama(8) & "~" & dataRowUtama(9) & "~" & dataRowUtama(10) & "~" & dataRowUtama(11) & "~" & dataRowUtama(12) & "~" & dataRowUtama(13) & "~" & dataRowUtama(14) & "~" & dataRowUtama(15) & "~" & dataRowUtama(16) & "~" & dataRowUtama(17) & "~" & dataRowUtama(18) & "~" & dataRowUtama(19)) = False Then
                result(2) = "Main Row : " & i & " - Insert into main datatable failed." : GoTo selesai
            End If

        Next


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idsidetail(0) As Integer, idsi(1) As Integer, idbarang(2) As Integer, jml(3) As Double, satuan(4) As String, 
        'customtext1(5) As String, customtext2(6) As String, customtext3(7) As String, customtext4(8) As String, customtext5(9) As String, 
        'customint1(10) As Integer, customint2(11) As Integer, customint3(12) As Integer, customdbl1(13) As Double, customdbl2(14) As Double, 
        'customdbl3(15) As Double, customdate1(16) As Date, customdate2(17) As Date, customdate3(18) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idsidetail, idsi, idbarang, jml, satuan, customtext1, customtext2, 
        'customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idsidetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idsi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint3", AsEnumTypeData.AsInt64)
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
            If (dataRowDetail.Length <> 19) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idsidetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idsidetail required numeric." : GoTo selesai
            End If
            'idsi(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idsi required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'jml(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'customint1(10) As Integer
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - customint1 required numeric." : GoTo selesai
            End If
            'customint2(11) As Integer
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - customint2 required numeric." : GoTo selesai
            End If
            'customint3(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - customint3 required numeric." : GoTo selesai
            End If
            'customdbl1(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(16) As Date
            If (IsDate(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(17) As Date
            If (IsDate(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(18) As Date
            If (IsDate(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'idsidetail(0) As Integer
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idsidetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idsidetail should not be more than 20 character." : GoTo selesai
            End If

            'idsi(1) As Integer
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idsi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idsi should not be more than 20 character." : GoTo selesai
            End If

            'idbarang(2) As 
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - idbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 20 Then
                result(2) = "Row : " & i & " - idbarang should not be more than 20 character." : GoTo selesai
            End If

            'jml(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - jml can't be empty" : GoTo selesai
            End If

            'satuan(4) As String
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(4)) > 25 Then
                result(2) = "Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'customdbl1(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(16) As Date
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(17) As Date
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(18) As Date
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idsidetail~idsi~idbarang~jml~satuan~customtext1~customtext2~customtext3~customtext4~customtext5~customint1~customint2~customint3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18)) = False Then
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
                'Hapus utama
                If (isUpdate) Then
                    sql = "Delete from M_12_Pos_Substitution_Item"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Hapus detail
                If (isUpdate) Then
                    sql = "Delete from M_12_Pos_Substitution_Item_Detail"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                'Proses utama
                Dim strValue1 As New StringBuilder
                For Each dr1 As DataRow In dtutama.Rows
                    strValue1.Append(IIf(Len(strValue1.ToString) = 0, "", ", "))
                    strValue1.Append("('" & FixQuotes(dr1("siid")) & "', '" & FixQuotes(dr1("sikategori")) & "', '" & FixQuotes(dr1("siidbarang")) & "', '" & FixQuotes(dr1("sioperator")) & "', '" & FixDouble(dr1("sijml1")) & "', '" & FixDouble(dr1("sijml2")) & "', '" & FixQuotes(dr1("sicustomtext1")) & "', '" & FixQuotes(dr1("sicustomtext2")) & "', '" & FixQuotes(dr1("sicustomtext3")) & "', '" & FixQuotes(dr1("sicustomtext4")) & "', '" & FixQuotes(dr1("sicustomtext5")) & "', " & dr1("sicustomint1") & ", " & dr1("sicustomint2") & ", " & dr1("sicustomint3") & ", '" & FixDouble(dr1("sicustomdbl1")) & "', '" & FixDouble(dr1("sicustomdbl2")) & "', '" & FixDouble(dr1("sicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("sicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("sicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("sicustomdate3"))) & "')")
                Next
                sql = "Insert into M_12_Pos_Substitution_Item(siid, sikategori, siidbarang, sioperator, sijml1, sijml2, sicustomtext1, sicustomtext2, sicustomtext3, sicustomtext4, sicustomtext5, sicustomint1, sicustomint2, sicustomint3, sicustomdbl1, sicustomdbl2, sicustomdbl3, sicustomdate1, sicustomdate2, sicustomdate3) values" & strValue1.ToString & ""
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()


                'Proses detail
                If (dtdetail.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("idsidetail")) & "', '" & FixQuotes(dr1("idsi")) & "', '" & FixQuotes(dr1("idbarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M_12_Pos_Substitution_Item_Detail(idsidetail, idsi, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
    Public Function M12_Pos_Substitution_ItemGetdataById(ByVal param As String) As String

        'M12_Pos_Substitution_ItemGetdataById Utama --------------------------------------------------------
        'siid, sikategori, siidbarang, sioperator, sijml1, sijml2, sicustomtext1, 
        'sicustomtext2, sicustomtext3, sicustomtext4, sicustomtext5, sicustomint1, sicustomint2, sicustomint3, 
        'sicustomdbl1, sicustomdbl2, sicustomdbl3, sicustomdate1, sicustomdate2, sicustomdate3, pcnama, 
        'bkode, bnama, btipe, bsatuan, sioperatornama, sitgl1, sitgl2, sinopromo

        'M12_Pos_Substitution_ItemGetdataById Detail -------------------------------------------------------
        'idsidetail, idsi, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, 
        'customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, kodebarang, namabarang, tipebarang

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

        Dim NmMemcached As String = "aplikasi1-M_12_Pos_Substitution_Item~M_12_Pos_Substitution_Item_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "siid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "siid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        sql = "select `si`.`siid` AS `siid`,`si`.`sikategori` AS `sikategori`,`si`.`siidbarang` AS `siidbarang`,`si`.`sioperator` AS `sioperator`,`si`.`sijml1` AS `sijml1`,`si`.`sijml2` AS `sijml2`,`si`.`sicustomtext1` AS `sicustomtext1`,`si`.`sicustomtext2` AS `sicustomtext2`,`si`.`sicustomtext3` AS `sicustomtext3`,`si`.`sicustomtext4` AS `sicustomtext4`,`si`.`sicustomtext5` AS `sicustomtext5`,`si`.`sicustomint1` AS `sicustomint1`,`si`.`sicustomint2` AS `sicustomint2`,`si`.`sicustomint3` AS `sicustomint3`,`si`.`sicustomdbl1` AS `sicustomdbl1`,`si`.`sicustomdbl2` AS `sicustomdbl2`,`si`.`sicustomdbl3` AS `sicustomdbl3`,`si`.`sicustomdate1` AS `sicustomdate1`,`si`.`sicustomdate2` AS `sicustomdate2`,`si`.`sicustomdate3` AS `sicustomdate3`,`pc`.`pcnama` AS `pcnama`,`i`.`bkode` AS `bkode`,`i`.`bnama` AS `bnama`,`i`.`btipe` AS `btipe`,`i`.`bsatuan` AS `bsatuan`,(case `si`.`sioperator` when 0 then 'Between' when 1 then '>=' when 2 then 'Multiple' else 'Unknown' end) AS `sioperatornama`, `si`.`sitgl1` AS `sitgl1`, `si`.`sitgl2` AS `sitgl2`, `si`.`sitgl2` AS `sitgl2`, `si`.`sinopromo` AS `sinopromo`,`sid`.`idsidetail` AS `idsidetail`,`sid`.`idsi` AS `idsi`,`sid`.`idbarang` AS `idbarang`,`sid`.`jml` AS `jml`,`i2`.`bsatuan` AS `satuan`,`sid`.`customtext1` AS `customtext1`,`sid`.`customtext2` AS `customtext2`,`sid`.`customtext3` AS `customtext3`,`sid`.`customtext4` AS `customtext4`,`sid`.`customtext5` AS `customtext5`,`sid`.`customint1` AS `customint1`,`sid`.`customint2` AS `customint2`,`sid`.`customint3` AS `customint3`,`sid`.`customdbl1` AS `customdbl1`,`sid`.`customdbl2` AS `customdbl2`,`sid`.`customdbl3` AS `customdbl3`,`sid`.`customdate1` AS `customdate1`,`sid`.`customdate2` AS `customdate2`,`sid`.`customdate3` AS `customdate3`,`i2`.`bkode` AS `kodebarang`,`i2`.`bnama` AS `namabarang`,`i2`.`btipe` AS `tipebarang` from ((((`M_12_Pos_Substitution_Item` `si` join `m_12_pos_category` `pc` on((`si`.`sikategori` = `pc`.`pckode`))) join `m1_item` `i` on((`si`.`siidbarang` = `i`.`bid`))) join `M_12_Pos_Substitution_Item_detail` `sid` on((`si`.`siid` = `sid`.`idsi`))) join `m1_item` `i2` on((`sid`.`idbarang` = `i2`.`bid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                     FxDB(drutama("siid"), ""), sptField,
                     FxDB(drutama("sikategori"), ""), sptField,
                     FxDB(drutama("siidbarang"), ""), sptField,
                     FxDB(drutama("sioperator"), ""), sptField,
                     FxDB(drutama("sijml1"), 0), sptField,
                     FxDB(drutama("sijml2"), 0), sptField,
                     FxDB(drutama("sicustomtext1"), ""), sptField,
                     FxDB(drutama("sicustomtext2"), ""), sptField,
                     FxDB(drutama("sicustomtext3"), ""), sptField,
                     FxDB(drutama("sicustomtext4"), ""), sptField,
                     FxDB(drutama("sicustomtext5"), ""), sptField,
                     FxDB(drutama("sicustomint1"), 0), sptField,
                     FxDB(drutama("sicustomint2"), 0), sptField,
                     FxDB(drutama("sicustomint3"), 0), sptField,
                     FxDB(drutama("sicustomdbl1"), 0), sptField,
                     FxDB(drutama("sicustomdbl2"), 0), sptField,
                     FxDB(drutama("sicustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sicustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sicustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sicustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("pcnama"), ""), sptField,
                     FxDB(drutama("bkode"), ""), sptField,
                     FxDB(drutama("bnama"), ""), sptField,
                     FxDB(drutama("btipe"), ""), sptField,
                     FxDB(drutama("bsatuan"), ""), sptField,
                     FxDB(drutama("sioperatornama"), ""), sptField,
                     FxDB(drutama("sitgl1"), ""), sptField,
                     FxDB(drutama("sitgl2"), ""), sptField,
                     FxDB(drutama("sinopromo"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idsidetail"), ""), sptField,
                     FxDB(dr("idsi"), ""), sptField,
                     FxDB(dr("idbarang"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customtext4"), ""), sptField,
                     FxDB(dr("customtext5"), ""), sptField,
                     FxDB(dr("customint1"), 0), sptField,
                     FxDB(dr("customint2"), 0), sptField,
                     FxDB(dr("customint3"), 0), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

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
        strResultData = String.Concat(utama, sptSubParam, detail)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("siid, sikategori, siidbarang, sioperator, sijml1, sijml2, sicustomtext1, sicustomtext2, sicustomtext3, sicustomtext4, sicustomtext5, sicustomint1, sicustomint2, sicustomint3, sicustomdbl1, sicustomdbl2, sicustomdbl3, sicustomdate1, sicustomdate2, sicustomdate3, pcnama, bkode, bnama, btipe, bsatuan, sioperatornama, sitgl1, sitgl2, sinopromo" & sptSubParam & "idsidetail, idsi, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, namabarang, tipebarang"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_Substitution_ItemSearch(ByVal param As String) As String
        'JIKA MENAMBAHKAN FIELD DISINI MAKA HARUS TAMBAHKAN JUGA LENGTH ARRAY PADA VALIDASI AMBIL USERID WS M0_LOGIN
        'M12_Pos_Substitution_ItemSearch --------------------------------------------------------
        'siid, sikategori, siidbarang, sioperator, sijml1, sijml2, sicustomtext1, 
        'sicustomtext2, sicustomtext3, sicustomtext4, sicustomtext5, sicustomint1, sicustomint2, sicustomint3, 
        'sicustomdbl1, sicustomdbl2, sicustomdbl3, sicustomdate1, sicustomdate2, sicustomdate3, pcnama, 
        'bkode, bnama, btipe, bsatuan, sioperatornama, sitgl1, sitgl2, sinopromo

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
        sql = "select `si`.`siid` AS `siid`,`si`.`sikategori` AS `sikategori`,`si`.`siidbarang` AS `siidbarang`,`si`.`sioperator` AS `sioperator`,`si`.`sijml1` AS `sijml1`,`si`.`sijml2` AS `sijml2`,`si`.`sicustomtext1` AS `sicustomtext1`,`si`.`sicustomtext2` AS `sicustomtext2`,`si`.`sicustomtext3` AS `sicustomtext3`,`si`.`sicustomtext4` AS `sicustomtext4`,`si`.`sicustomtext5` AS `sicustomtext5`,`si`.`sicustomint1` AS `sicustomint1`,`si`.`sicustomint2` AS `sicustomint2`,`si`.`sicustomint3` AS `sicustomint3`,`si`.`sicustomdbl1` AS `sicustomdbl1`,`si`.`sicustomdbl2` AS `sicustomdbl2`,`si`.`sicustomdbl3` AS `sicustomdbl3`,`si`.`sicustomdate1` AS `sicustomdate1`,`si`.`sicustomdate2` AS `sicustomdate2`,`si`.`sicustomdate3` AS `sicustomdate3`,`pc`.`pcnama` AS `pcnama`,`i`.`bkode` AS `bkode`,`i`.`bnama` AS `bnama`,`i`.`btipe` AS `btipe`,`i`.`bsatuan` AS `bsatuan`,(case `si`.`sioperator` when 0 then 'Between' when 1 then '>=' when 2 then 'Multiple' else 'Unknown' end) AS `sioperatornama`, si.`sitgl1` AS `sitgl1`, si.`sitgl2` AS `sitgl2`, si.`sinopromo` AS `sinopromo` from ((`M_12_Pos_Substitution_Item` `si` join `m_12_pos_category` `pc` on((`si`.`sikategori` = `pc`.`pckode`))) join `m1_item` `i` on((`si`.`siidbarang` = `i`.`bid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M_12_Pos_Substitution_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("siid"), ""), sptField,
                     FxDB(dr("sikategori"), ""), sptField,
                     FxDB(dr("siidbarang"), ""), sptField,
                     FxDB(dr("sioperator"), ""), sptField,
                     FxDB(dr("sijml1"), 0), sptField,
                     FxDB(dr("sijml2"), 0), sptField,
                     FxDB(dr("sicustomtext1"), ""), sptField,
                     FxDB(dr("sicustomtext2"), ""), sptField,
                     FxDB(dr("sicustomtext3"), ""), sptField,
                     FxDB(dr("sicustomtext4"), ""), sptField,
                     FxDB(dr("sicustomtext5"), ""), sptField,
                     FxDB(dr("sicustomint1"), 0), sptField,
                     FxDB(dr("sicustomint2"), 0), sptField,
                     FxDB(dr("sicustomint3"), 0), sptField,
                     FxDB(dr("sicustomdbl1"), 0), sptField,
                     FxDB(dr("sicustomdbl2"), 0), sptField,
                     FxDB(dr("sicustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sicustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sicustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sicustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("pcnama"), ""), sptField,
                     FxDB(dr("bkode"), ""), sptField,
                     FxDB(dr("bnama"), ""), sptField,
                     FxDB(dr("btipe"), ""), sptField,
                     FxDB(dr("bsatuan"), ""), sptField,
                     FxDB(dr("sioperatornama"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sitgl1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sitgl2"), ""), formatTgl), sptField,
                     FxDB(dr("sinopromo"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("siid, sikategori, siidbarang, sioperator, sijml1, sijml2, sicustomtext1, sicustomtext2, sicustomtext3, sicustomtext4, sicustomtext5, sicustomint1, sicustomint2, sicustomint3, sicustomdbl1, sicustomdbl2, sicustomdbl3, sicustomdate1, sicustomdate2, sicustomdate3, pcnama, bkode, bnama, btipe, bsatuan, sioperatornama, sitgl1, sitgl2, sinopromo"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_Substitution_Item_DetailSearch(ByVal param As String) As String
        'M12_Pos_Substitution_Item_DetailSearch --------------------------------------------------------
        'idsidetail, idsi, idbarang, jml, satuan, customtext1, customtext2, 
        'customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, namabarang, 
        'tipebarang

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
        sql = "select `sid`.`idsidetail` AS `idsidetail`,`sid`.`idsi` AS `idsi`,`sid`.`idbarang` AS `idbarang`,`sid`.`jml` AS `jml`,`i`.`bsatuan` AS `satuan`,`sid`.`customtext1` AS `customtext1`,`sid`.`customtext2` AS `customtext2`,`sid`.`customtext3` AS `customtext3`,`sid`.`customtext4` AS `customtext4`,`sid`.`customtext5` AS `customtext5`,`sid`.`customint1` AS `customint1`,`sid`.`customint2` AS `customint2`,`sid`.`customint3` AS `customint3`,`sid`.`customdbl1` AS `customdbl1`,`sid`.`customdbl2` AS `customdbl2`,`sid`.`customdbl3` AS `customdbl3`,`sid`.`customdate1` AS `customdate1`,`sid`.`customdate2` AS `customdate2`,`sid`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`i`.`bnama` AS `namabarang`,`i`.`btipe` AS `tipebarang` from (`m_12_pos_substitution_item_detail` `sid` join `m1_item` `i` on((`sid`.`idbarang` = `i`.`bid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M_12_Pos_Substitution_Item_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idsidetail"), ""), sptField,
                     FxDB(dr("idsi"), ""), sptField,
                     FxDB(dr("idbarang"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customtext4"), ""), sptField,
                     FxDB(dr("customtext5"), ""), sptField,
                     FxDB(dr("customint1"), 0), sptField,
                     FxDB(dr("customint2"), 0), sptField,
                     FxDB(dr("customint3"), 0), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idsidetail, idsi, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, namabarang, tipebarang"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_Substitution_Item_DetailSetting(ByVal param As String) As String
        'JIKA MENAMBAHKAN FIELD DISINI MAKA HARUS TAMBAHKAN JUGA LENGTH ARRAY PADA VALIDASI AMBIL USERID WS M0_LOGIN
        'M12_Pos_Substitution_Item_DetailSetting --------------------------------------------------------
        'siid, bid, bkode, bnama, btipe, bjenis, bkategori, bsatuan, 
        'bsatuandefault, bhpp, bbarcode, bhargabeli, bhppaverage, bhargajual1, bhargajual2, 
        'bhargajual3, bhargajual4, bhargajual5, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, 
        'bdiskonjual5, bstok, bstokbooking, bmarginminimal, brekpersediaan, brekpenjualan, brekreturpenjualan, brekdiskonpenjualan, 
        'brekhargapokok, brekreturpembelian, brekdiskonpembelian, brekkonsinyasi, bserial, bbatch, bnilaisatuan, 
        'bnilaisatuandefault, bsuplier, bsuplierkode, bsupliernama, bnamafile, bapanjang, balebar, batinggi,
        'bstokminimal, bstokmaksimal, breorder, jml


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

        'BUAT QUERY
        sql = "SELECT si.siid, i.bid AS bid, i.bkode AS bkode, i.bnama AS bnama, i.btipe AS btipe, i.bjenis AS bjenis, i.bkategori AS bkategori, i.bsatuan AS bsatuan, i.bsatuandefault AS bsatuandefault, i.bhpp AS bhpp, i.bbarcode AS bbarcode, i.bhargabeli AS bhargabeli, i.bhppaverage AS bhppaverage, pi.pihargajual1 AS bhargajual1, pi.pihargajual2 AS bhargajual2, pi.pihargajual3 AS bhargajual3, pi.pihargajual4 AS bhargajual4, pi.pihargajual5 AS bhargajual5, pi.pidiskonjual1 AS bdiskonjual1, pi.pidiskonjual2 AS bdiskonjual2, pi.pidiskonjual3 AS bdiskonjual3, pi.pidiskonjual4 AS bdiskonjual4, pi.pidiskonjual5 AS bdiskonjual5, i.bstok AS bstok, ifnull(sum(`ib`.`jmlbooking`),0) AS bstokbooking, i.bmarginminimal AS bmarginminimal, i.brekpersediaan AS brekpersediaan, i.brekpenjualan AS brekpenjualan, i.brekreturpenjualan AS brekreturpenjualan, i.brekdiskonpenjualan AS brekdiskonpenjualan, i.brekhargapokok AS brekhargapokok, i.brekreturpembelian AS brekreturpembelian, i.brekdiskonpembelian AS brekdiskonpembelian, i.brekkonsinyasi AS brekkonsinyasi, i.bserial AS bserial, i.bbatch AS bbatch, i.bnilaisatuan AS bnilaisatuan, i.bnilaisatuandefault AS bnilaisatuandefault, i.bsuplier AS bsuplier, c.kkode AS bsuplierkode, c.knama AS bsupliernama, f.fnamafile AS bnamafile, i.bapanjang AS bapanjang, i.balebar AS balebar, i.batinggi AS batinggi, pi.pistokminimal AS bstokminimal, pi.pistokmaksimal AS bstokmaksimal, pi.pistokreorder AS breorder, sid.jml from `m1_item` `i`  JOIN m_12_pos_substitution_item_detail sid ON i.bid = sid.idbarang JOIN m_12_pos_substitution_item si ON sid.idsi = si.siid JOIN m_12_pos_item pi ON sid.idbarang = pi.piidbarang AND si.sikategori = pi.pikategori left join `m1_item_booking` `ib` on `i`.`bid` = `ib`.`idbarang` left join `m1_contact` `c` on `i`.`bsuplier` = `c`.`kid` left join `m1_files` `f` on `f`.`fsumber` = 'Item' and `i`.`bid` = `f`.`fidtransaksi` and `f`.`fdefault` = 1"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "i.bid, sid.idsidetail", sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("siid"), 0), sptField,
                     FxDB(dr("bid"), 0), sptField,
                     FxDB(dr("bkode"), ""), sptField,
                     FxDB(dr("bnama"), ""), sptField,
                     FxDB(dr("btipe"), ""), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bkategori"), ""), sptField,
                     FxDB(dr("bsatuan"), ""), sptField,
                     FxDB(dr("bsatuandefault"), ""), sptField,
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bbarcode"), ""), sptField,
                     FxDB(dr("bhargabeli"), 0), sptField,
                     FxDB(dr("bhppaverage"), 0), sptField,
                     FxDB(dr("bhargajual1"), 0), sptField,
                     FxDB(dr("bhargajual2"), 0), sptField,
                     FxDB(dr("bhargajual3"), 0), sptField,
                     FxDB(dr("bhargajual4"), 0), sptField,
                     FxDB(dr("bhargajual5"), 0), sptField,
                     FxDB(dr("bdiskonjual1"), 0), sptField,
                     FxDB(dr("bdiskonjual2"), 0), sptField,
                     FxDB(dr("bdiskonjual3"), 0), sptField,
                     FxDB(dr("bdiskonjual4"), 0), sptField,
                     FxDB(dr("bdiskonjual5"), 0), sptField,
                     FxDB(dr("bstok"), 0), sptField,
                     FxDB(dr("bstokbooking"), 0), sptField,
                     FxDB(dr("bmarginminimal"), 0), sptField,
                     FxDB(dr("brekpersediaan"), ""), sptField,
                     FxDB(dr("brekpenjualan"), ""), sptField,
                     FxDB(dr("brekreturpenjualan"), ""), sptField,
                     FxDB(dr("brekdiskonpenjualan"), ""), sptField,
                     FxDB(dr("brekhargapokok"), ""), sptField,
                     FxDB(dr("brekreturpembelian"), ""), sptField,
                     FxDB(dr("brekdiskonpembelian"), ""), sptField,
                     FxDB(dr("brekkonsinyasi"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("bnilaisatuan"), 0), sptField,
                     FxDB(dr("bnilaisatuandefault"), 0), sptField,
                     FxDB(dr("bsuplier"), 0), sptField,
                     FxDB(dr("bsuplierkode"), ""), sptField,
                     FxDB(dr("bsupliernama"), ""), sptField,
                     FxDB(dr("bnamafile"), ""), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("bstokminimal"), 0), sptField,
                     FxDB(dr("bstokmaksimal"), 0), sptField,
                     FxDB(dr("breorder"), 0), sptField,
                     FxDB(dr("jml"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Item data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("siid, bid, bkode, bnama, btipe, bjenis, bkategori, bsatuan, bsatuandefault, bhpp, bbarcode, bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bstokbooking, bmarginminimal, brekpersediaan, brekpenjualan, brekreturpenjualan, brekdiskonpenjualan, brekhargapokok, brekreturpembelian, brekdiskonpembelian, brekkonsinyasi, bserial, bbatch, bnilaisatuan, bnilaisatuandefault, bsuplier, bsuplierkode, bsupliernama, bnamafile, bapanjang, balebar, batinggi, bstokminimal, bstokmaksimal, breorder, jml"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_Substitution_ItemDownload(ByVal param As String) As String
        'M12_Pos_Substitution_ItemDownload --------------------------------------------------------
        'Utama
        'siid, sikategori, siidbarang, sioperator, sijml1, sijml2, sicustomtext1, 
        'sicustomtext2, sicustomtext3, sicustomtext4, sicustomtext5, sicustomint1, sicustomint2, sicustomint3, 
        'sicustomdbl1, sicustomdbl2, sicustomdbl3, sicustomdate1, sicustomdate2, sicustomdate3

        'Detail
        'idsidetail, idsi, idbarang, jml, satuan, customtext1, customtext2, 
        'customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = "", detail As String = ""

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

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M_12_Pos_Substitution_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("siid"), ""), sptField,
                     FxDB(dr("sikategori"), ""), sptField,
                     FxDB(dr("siidbarang"), ""), sptField,
                     FxDB(dr("sioperator"), ""), sptField,
                     FxDB(dr("sijml1"), 0), sptField,
                     FxDB(dr("sijml2"), 0), sptField,
                     FxDB(dr("sicustomtext1"), ""), sptField,
                     FxDB(dr("sicustomtext2"), ""), sptField,
                     FxDB(dr("sicustomtext3"), ""), sptField,
                     FxDB(dr("sicustomtext4"), ""), sptField,
                     FxDB(dr("sicustomtext5"), ""), sptField,
                     FxDB(dr("sicustomint1"), 0), sptField,
                     FxDB(dr("sicustomint2"), 0), sptField,
                     FxDB(dr("sicustomint3"), 0), sptField,
                     FxDB(dr("sicustomdbl1"), 0), sptField,
                     FxDB(dr("sicustomdbl2"), 0), sptField,
                     FxDB(dr("sicustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sicustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sicustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sicustomdate3"), ""), formatTgl), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)


            'AMBIL DATA DETAIL
            sql = "SELECT sid.idsidetail, sid.idsi, sid.idbarang, sid.jml, sid.satuan, sid.customtext1, sid.customtext2, sid.customtext3, sid.customtext4, sid.customtext5, sid.customint1, sid.customint2, sid.customint3, sid.customdbl1, sid.customdbl2, sid.customdbl3, sid.customdate1, sid.customdate2, sid.customdate3 FROM m_12_pos_substitution_item si JOIN m_12_pos_substitution_item_detail sid ON si.siid = sid.idsi"

            Dim dtdetail As New DataTable
            dtdetail = AmbilData("aplikasi1-M_12_Pos_Substitution_Item_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtdetail.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idsidetail"), ""), sptField,
                     FxDB(dr("idsi"), ""), sptField,
                     FxDB(dr("idbarang"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customtext4"), ""), sptField,
                     FxDB(dr("customtext5"), ""), sptField,
                     FxDB(dr("customint1"), 0), sptField,
                     FxDB(dr("customint2"), 0), sptField,
                     FxDB(dr("customint3"), 0), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptRow)
            Next
            If detail.Length > 0 Then detail = detail.Substring(0, detail.Length - sptRow.Length) Else detail = detail


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
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search, sptSubParam, detail)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("siid, sikategori, siidbarang, sioperator, sijml1, sijml2, sicustomtext1, sicustomtext2, sicustomtext3, sicustomtext4, sicustomtext5, sicustomint1, sicustomint2, sicustomint3, sicustomdbl1, sicustomdbl2, sicustomdbl3, sicustomdate1, sicustomdate2, sicustomdate3" & sptSubParam & "idsidetail, idsi, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3"))

        Return wsResult
    End Function

End Class
