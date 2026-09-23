Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_pos_additional_item
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_Pos_Additional_ItemSimpan(ByVal param As String) As String
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
        'aiid(0) As Integer, aikategori(1) As String, aiidbarang(2) As Integer, aioperator(3) As String, aijml1(4) As Double, 
        'aijml2(5) As Double, aicustomtext1(6) As String, aicustomtext2(7) As String, aicustomtext3(8) As String, aicustomtext4(9) As String, 
        'aicustomtext5(10) As String, aicustomint1(11) As Integer, aicustomint2(12) As Integer, aicustomint3(13) As Integer, aicustomdbl1(14) As Double, 
        'aicustomdbl2(15) As Double, aicustomdbl3(16) As Double, aicustomdate1(17) As Date, aicustomdate2(18) As Date, aicustomdate3(19) As Date
        'aitgl1(20) As Date, aitgl2(21) As Date, ainopromo(21) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'aiid, aikategori, aiidbarang, aioperator, aijml1, aijml2, aicustomtext1, 
        'aicustomtext2, aicustomtext3, aicustomtext4, aicustomtext5, aicustomint1, aicustomint2, aicustomint3, 
        'aicustomdbl1, aicustomdbl2, aicustomdbl3, aicustomdate1, aicustomdate2, aicustomdate3
        'aitgl1, aitgl2, ainopromo

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 23) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'aiid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "aiid required numeric." : GoTo selesai
        End If
        'aiidbarang(2) As Integer
        If (IsNumeric(dataUtama(2)) = False) Then
            result(2) = "aiidbarang required numeric." : GoTo selesai
        End If
        'aijml1(4) As Double
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "aijml1 required numeric." : GoTo selesai
        End If
        'aijml2(5) As Double
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "aijml2 required numeric." : GoTo selesai
        End If
        'aicustomint1(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "aicustomint1 required numeric." : GoTo selesai
        End If
        'aicustomint2(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "aicustomint2 required numeric." : GoTo selesai
        End If
        'aicustomint3(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "aicustomint3 required numeric." : GoTo selesai
        End If
        'aicustomdbl1(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "aicustomdbl1 required numeric." : GoTo selesai
        End If
        'aicustomdbl2(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "aicustomdbl2 required numeric." : GoTo selesai
        End If
        'aicustomdbl3(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "aicustomdbl3 required numeric." : GoTo selesai
        End If
        'aicustomdate1(17) As Date
        If (IsDate(dataUtama(17)) = False) Then
            result(2) = "aicustomdate1 required date." : GoTo selesai
        End If
        'aicustomdate2(18) As Date
        If (IsDate(dataUtama(18)) = False) Then
            result(2) = "aicustomdate2 required date." : GoTo selesai
        End If
        'aicustomdate3(19) As Date
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "aicustomdate3 required date." : GoTo selesai
        End If
        'aitgl1(20) As Date
        If (IsDate(dataUtama(20)) = False) Then
            result(2) = "aitgl1 required date." : GoTo selesai
        End If
        'aitgl2(21) As Date
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "aitgl1 required date." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'aiid(0) As Integer
        If Len(dataUtama(0)) = 0 Then
            result(2) = "aiid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 20 Then
            result(2) = "aiid should not be more than 20 character." : GoTo selesai
        End If

        'aikategori(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "aikategori can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "aikategori should not be more than 25 character." : GoTo selesai
        End If

        'aiidbarang(2) As Integer
        If Len(dataUtama(2)) = 0 Then
            result(2) = "aiidbarang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 20 Then
            result(2) = "aiidbarang should not be more than 20 character." : GoTo selesai
        End If

        'aioperator(3) As String
        If IsNumeric(dataUtama(3)) = False Then
            result(2) = "aioperator required numeric" : GoTo selesai
        ElseIf dataUtama(3) <> 0 And dataUtama(3) <> 1 And dataUtama(3) <> 2 Then
            result(2) = "invalid aioperator value" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "aioperator should not be more than 25 character." : GoTo selesai
        End If

        'aijml1(4) As Double
        If Len(dataUtama(4)) = 0 Then
            result(2) = "aijml1 can't be empty" : GoTo selesai
        End If

        'aijml2(5) As Double
        If Len(dataUtama(5)) = 0 Then
            result(2) = "aijml2 can't be empty" : GoTo selesai
        End If

        'aicustomdbl1(14) As Double
        If Len(dataUtama(14)) = 0 Then
            result(2) = "aicustomdbl1 can't be empty" : GoTo selesai
        End If

        'aicustomdbl2(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "aicustomdbl2 can't be empty" : GoTo selesai
        End If

        'aicustomdbl3(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "aicustomdbl3 can't be empty" : GoTo selesai
        End If

        'aicustomdate1(17) As Date
        If Len(dataUtama(17)) = 0 Then
            result(2) = "aicustomdate1 can't be empty" : GoTo selesai
        End If

        'aicustomdate2(18) As Date
        If Len(dataUtama(18)) = 0 Then
            result(2) = "aicustomdate2 can't be empty" : GoTo selesai
        End If

        'aicustomdate3(19) As Date
        If Len(dataUtama(19)) = 0 Then
            result(2) = "aicustomdate3 can't be empty" : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "aiid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aiidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aioperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aijml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aijml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aitgl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aitgl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ainopromo", AsEnumTypeData.AsString)

        If AsDataTableTambahData(dtutama, "aiid~aikategori~aiidbarang~aioperator~aijml1~aijml2~aicustomtext1~aicustomtext2~aicustomtext3~aicustomtext4~aicustomtext5~aicustomint1~aicustomint2~aicustomint3~aicustomdbl1~aicustomdbl2~aicustomdbl3~aicustomdate1~aicustomdate2~aicustomdate3~aitgl1~aitgl2~ainopromo", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idaidetail(0) As Integer, idai(1) As Integer, idbarang(2) As Integer, jml(3) As Double, satuan(4) As String, 
        'customtext1(5) As String, customtext2(6) As String, customtext3(7) As String, customtext4(8) As String, customtext5(9) As String, 
        'customint1(10) As Integer, customint2(11) As Integer, customint3(12) As Integer, customdbl1(13) As Double, customdbl2(14) As Double, 
        'customdbl3(15) As Double, customdate1(16) As Date, customdate2(17) As Date, customdate3(18) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idaidetail, idai, idbarang, jml, satuan, customtext1, customtext2, 
        'customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idaidetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idai", AsEnumTypeData.AsInt64)
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
            'idaidetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idaidetail required numeric." : GoTo selesai
            End If
            'idai(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idai required numeric." : GoTo selesai
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
            'idaidetail(0) As Integer
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idaidetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idaidetail should not be more than 20 character." : GoTo selesai
            End If

            'idai(1) As Integer
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idai can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idai should not be more than 20 character." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idaidetail~idai~idbarang~jml~satuan~customtext1~customtext2~customtext3~customtext4~customtext5~customint1~customint2~customint3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18)) = False Then
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

                'CEK OPERATOR :
                'JIKA BETWEEN (0) ATAU >= (1) MAKA BOLEH LEBIH DARI SATU KONDISI 
                '=> BEBERAPA KONDISI BETWEEN DAN SATU KONDISI >= (1)
                'JIKA KELIPATAN (2) MAKA HANYA BOLEH SATU KONDISI
                Dim dtOperator As New DataTable
                sql = "SELECT ai.aikategori as kategori, ai.aiidbarang as idbarang, ai.aioperator as operator, i.bkode, (CASE ai.aioperator WHEN 0 THEN 'Between' WHEN 1 THEN '>=' WHEN 2 THEN 'Multiple' ELSE 'Unknown' END) as operatornama FROM m_12_pos_additional_item ai JOIN m1_item i ON ai.aiidbarang = i.bid WHERE ai.aikategori = '" & FxDB(drutama("aikategori"), "") & "' AND ai.aiidbarang = '" & FxDB(drutama("aiidbarang"), "") & "' AND ai.aiid <> '" & FxDB(drutama("aiid"), "") & "' GROUP BY ai.aioperator ORDER BY ai.aioperator"
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
                                If drutama("aioperator") = 2 Or (vOperator = 1 And drutama("aioperator") = vOperator) Then
                                    result(2) = "Item : " & FxDB(dr1("bkode"), "") & " - already has '" & FxDB(dr1("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                End If
                            End If
                        End If
                    Next
                End If

                'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drutama("aikategori")) & "' "

                If isUpdate Then
                    result(4) = drutama("aiid")
                    notransaksi = drutama("aikategori")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(aiid) FROM M_12_Pos_Additional_Item WHERE aiid = '" & result(4) & "'", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then
                        sql = "Update M_12_Pos_Additional_Item set aikategori  = '" & FixQuotes(drutama("aikategori")) & "', aiidbarang  = '" & FixQuotes(drutama("aiidbarang")) & "', aioperator  = '" & FixQuotes(drutama("aioperator")) & "', aijml1  = '" & FixDouble(drutama("aijml1")) & "', aijml2  = '" & FixDouble(drutama("aijml2")) & "', aicustomtext1  = '" & FixQuotes(drutama("aicustomtext1")) & "', aicustomtext2  = '" & FixQuotes(drutama("aicustomtext2")) & "', aicustomtext3  = '" & FixQuotes(drutama("aicustomtext3")) & "', aicustomtext4  = '" & FixQuotes(drutama("aicustomtext4")) & "', aicustomtext5  = '" & FixQuotes(drutama("aicustomtext5")) & "', aicustomint1  = " & drutama("aicustomint1") & ", aicustomint2  = " & drutama("aicustomint2") & ", aicustomint3  = " & drutama("aicustomint3") & ", aicustomdbl1  = '" & FixDouble(drutama("aicustomdbl1")) & "', aicustomdbl2  = '" & FixDouble(drutama("aicustomdbl2")) & "', aicustomdbl3  = '" & FixDouble(drutama("aicustomdbl3")) & "', aicustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("aicustomdate1"))) & "', aicustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("aicustomdate2"))) & "', aicustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("aicustomdate3"))) & "', aitgl1  = '" & FixQuotes(AsFormatTanggal(drutama("aitgl1"))) & "', aitgl2  = '" & FixQuotes(AsFormatTanggal(drutama("aitgl2"))) & "', ainopromo  = '" & FixQuotes(drutama("ainopromo")) & "' where aiid = '" & drutama("aiid") & "'"
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

                    sql = "Insert into M_12_Pos_Additional_Item (aikategori, aiidbarang, aioperator, aijml1, aijml2, aicustomtext1, aicustomtext2, aicustomtext3, aicustomtext4, aicustomtext5, aicustomint1, aicustomint2, aicustomint3, aicustomdbl1, aicustomdbl2, aicustomdbl3, aicustomdate1, aicustomdate2, aicustomdate3, aitgl1, aitgl2, ainopromo) values('" & FixQuotes(drutama("aikategori")) & "', '" & FixQuotes(drutama("aiidbarang")) & "', '" & FixQuotes(drutama("aioperator")) & "', '" & FixDouble(drutama("aijml1")) & "', '" & FixDouble(drutama("aijml2")) & "', '" & FixQuotes(drutama("aicustomtext1")) & "', '" & FixQuotes(drutama("aicustomtext2")) & "', '" & FixQuotes(drutama("aicustomtext3")) & "', '" & FixQuotes(drutama("aicustomtext4")) & "', '" & FixQuotes(drutama("aicustomtext5")) & "', " & drutama("aicustomint1") & ", " & drutama("aicustomint2") & ", " & drutama("aicustomint3") & ", '" & FixDouble(drutama("aicustomdbl1")) & "', '" & FixDouble(drutama("aicustomdbl2")) & "', '" & FixDouble(drutama("aicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("aicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("aicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("aicustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("aitgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("aitgl2"))) & "', '" & FixQuotes(drutama("ainopromo")) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select aiid from M_12_Pos_Additional_Item where aikategori = '" & drutama("aikategori") & "' AND aiidbarang = '" & drutama("aiidbarang") & "' AND aioperator = '" & drutama("aioperator") & "' AND aijml1 = '" & drutama("aijml1") & "' AND aijml2 = '" & drutama("aijml2") & "' limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M_12_Pos_Additional_Item_Detail where idai = '" & result(4) & "'"
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
                        strValue2.Append("('" & FixQuotes(dr1("idaidetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("idbarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M_12_Pos_Additional_Item_Detail(idaidetail, idai, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                Dim paramSearch As String = M12_Pos_Additional_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Additional_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Additional_ItemDelete(ByVal param As String) As String

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
            sql = "SELECT aikategori as kategoripos FROM M_12_Pos_Additional_Item WHERE aiid = '" & idtransaksi & "' GROUP BY aikategori"
            Dim dtKategoriPOS As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtKategoriPOS.Rows.Count > 0 Then
                For Each drKategoriPOS As DataRow In dtKategoriPOS.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drKategoriPOS("kategoripos")) & "' "
                Next
            End If

            'DELETE DETAIL
            sql = "DELETE FROM M_12_Pos_Additional_Item_Detail WHERE idai = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M_12_Pos_Additional_Item WHERE aiid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_Pos_Additional_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Additional_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Additional_ItemImport(ByVal param As String) As String
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
        'aiid(0) As Integer, aikategori(1) As String, aiidbarang(2) As Integer, aioperator(3) As String, aijml1(4) As Double, 
        'aijml2(5) As Double, aicustomtext1(6) As String, aicustomtext2(7) As String, aicustomtext3(8) As String, aicustomtext4(9) As String, 
        'aicustomtext5(10) As String, aicustomint1(11) As Integer, aicustomint2(12) As Integer, aicustomint3(13) As Integer, aicustomdbl1(14) As Double, 
        'aicustomdbl2(15) As Double, aicustomdbl3(16) As Double, aicustomdate1(17) As Date, aicustomdate2(18) As Date, aicustomdate3(19) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'aiid, aikategori, aiidbarang, aioperator, aijml1, aijml2, aicustomtext1, 
        'aicustomtext2, aicustomtext3, aicustomtext4, aicustomtext5, aicustomint1, aicustomint2, aicustomint3, 
        'aicustomdbl1, aicustomdbl2, aicustomdbl3, aicustomdate1, aicustomdate2, aicustomdate3


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptRow)    'SPLIT PARAMETER DATA UTAMA
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "aiid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aiidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aioperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aijml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aijml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdate3", AsEnumTypeData.AsString)

        Dim JmlDtUtama As Integer = dataUtama.Length
        For i = 1 To JmlDtUtama
            'SPLIT DATA UTAMA
            dataRowUtama = dataUtama(i - 1).Split(sptField)

            'CEK ARRAY DATA UTAMA
            If (dataRowUtama.Length <> 20) Then
                result(2) = "Main Row : " & i & " - Invalid main transaction data parameter." : GoTo selesai
            End If

            'VALIDASI TIPE DATA UTAMA ==========================================================
            'aiid(0) As Integer
            If (IsNumeric(dataRowUtama(0)) = False) Then
                result(2) = "Main Row : " & i & " - aiid required numeric." : GoTo selesai
            End If
            'aiidbarang(2) As Integer
            If (IsNumeric(dataRowUtama(2)) = False) Then
                result(2) = "Main Row : " & i & " - aiidbarang required numeric." : GoTo selesai
            End If
            'aijml1(4) As Double
            If (IsNumeric(dataRowUtama(4)) = False) Then
                result(2) = "Main Row : " & i & " - aijml1 required numeric." : GoTo selesai
            End If
            'aijml2(5) As Double
            If (IsNumeric(dataRowUtama(5)) = False) Then
                result(2) = "Main Row : " & i & " - aijml2 required numeric." : GoTo selesai
            End If
            'aicustomint1(11) As Integer
            If (IsNumeric(dataRowUtama(11)) = False) Then
                result(2) = "Main Row : " & i & " - aicustomint1 required numeric." : GoTo selesai
            End If
            'aicustomint2(12) As Integer
            If (IsNumeric(dataRowUtama(12)) = False) Then
                result(2) = "Main Row : " & i & " - aicustomint2 required numeric." : GoTo selesai
            End If
            'aicustomint3(13) As Integer
            If (IsNumeric(dataRowUtama(13)) = False) Then
                result(2) = "Main Row : " & i & " - aicustomint3 required numeric." : GoTo selesai
            End If
            'aicustomdbl1(14) As Double
            If (IsNumeric(dataRowUtama(14)) = False) Then
                result(2) = "Main Row : " & i & " - aicustomdbl1 required numeric." : GoTo selesai
            End If
            'aicustomdbl2(15) As Double
            If (IsNumeric(dataRowUtama(15)) = False) Then
                result(2) = "Main Row : " & i & " - aicustomdbl2 required numeric." : GoTo selesai
            End If
            'aicustomdbl3(16) As Double
            If (IsNumeric(dataRowUtama(16)) = False) Then
                result(2) = "Main Row : " & i & " - aicustomdbl3 required numeric." : GoTo selesai
            End If
            'aicustomdate1(17) As Date
            If (IsDate(dataRowUtama(17)) = False) Then
                result(2) = "Main Row : " & i & " - aicustomdate1 required date." : GoTo selesai
            End If
            'aicustomdate2(18) As Date
            If (IsDate(dataRowUtama(18)) = False) Then
                result(2) = "Main Row : " & i & " - aicustomdate2 required date." : GoTo selesai
            End If
            'aicustomdate3(19) As Date
            If (IsDate(dataRowUtama(19)) = False) Then
                result(2) = "Main Row : " & i & " - aicustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA UTAMA ===================================================

            'VALIDASI DATA UTAMA =======================================================
            'aiid(0) As Integer
            If Len(dataRowUtama(0)) = 0 Then
                result(2) = "Main Row : " & i & " - aiid can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(0)) > 20 Then
                result(2) = "Main Row : " & i & " - aiid should not be more than 20 character." : GoTo selesai
            End If

            'aikategori(1) As String
            If Len(dataRowUtama(1)) = 0 Then
                result(2) = "Main Row : " & i & " - aikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(1)) > 25 Then
                result(2) = "Main Row : " & i & " - aikategori should not be more than 25 character." : GoTo selesai
            End If

            'aiidbarang(2) As Integer
            If Len(dataRowUtama(2)) = 0 Then
                result(2) = "Main Row : " & i & " - aiidbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(2)) > 20 Then
                result(2) = "Main Row : " & i & " - aiidbarang should not be more than 20 character." : GoTo selesai
            End If

            'aioperator(3) As String
            If IsNumeric(dataRowUtama(3)) = False Then
                result(2) = "Main Row : " & i & " - aioperator required numeric" : GoTo selesai
            ElseIf dataRowUtama(3) <> 0 And dataRowUtama(3) <> 1 And dataRowUtama(3) <> 2 Then
                result(2) = "Main Row : " & i & " - invalid aioperator value" : GoTo selesai
            End If
            If Len(dataRowUtama(3)) > 25 Then
                result(2) = "Main Row : " & i & " - aioperator should not be more than 25 character." : GoTo selesai
            End If

            'aijml1(4) As Double
            If Len(dataRowUtama(4)) = 0 Then
                result(2) = "Main Row : " & i & " - aijml1 can't be empty" : GoTo selesai
            End If

            'aijml2(5) As Double
            If Len(dataRowUtama(5)) = 0 Then
                result(2) = "Main Row : " & i & " - aijml2 can't be empty" : GoTo selesai
            End If

            'aicustomdbl1(14) As Double
            If Len(dataRowUtama(14)) = 0 Then
                result(2) = "Main Row : " & i & " - aicustomdbl1 can't be empty" : GoTo selesai
            End If

            'aicustomdbl2(15) As Double
            If Len(dataRowUtama(15)) = 0 Then
                result(2) = "Main Row : " & i & " - aicustomdbl2 can't be empty" : GoTo selesai
            End If

            'aicustomdbl3(16) As Double
            If Len(dataRowUtama(16)) = 0 Then
                result(2) = "Main Row : " & i & " - aicustomdbl3 can't be empty" : GoTo selesai
            End If

            'aicustomdate1(17) As Date
            If Len(dataRowUtama(17)) = 0 Then
                result(2) = "Main Row : " & i & " - aicustomdate1 can't be empty" : GoTo selesai
            End If

            'aicustomdate2(18) As Date
            If Len(dataRowUtama(18)) = 0 Then
                result(2) = "Main Row : " & i & " - aicustomdate2 can't be empty" : GoTo selesai
            End If

            'aicustomdate3(19) As Date
            If Len(dataRowUtama(19)) = 0 Then
                result(2) = "Main Row : " & i & " - aicustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA UTAMA ================================================


            If AsDataTableTambahData(dtutama, "aiid~aikategori~aiidbarang~aioperator~aijml1~aijml2~aicustomtext1~aicustomtext2~aicustomtext3~aicustomtext4~aicustomtext5~aicustomint1~aicustomint2~aicustomint3~aicustomdbl1~aicustomdbl2~aicustomdbl3~aicustomdate1~aicustomdate2~aicustomdate3", dataRowUtama(0) & "~" & dataRowUtama(1) & "~" & dataRowUtama(2) & "~" & dataRowUtama(3) & "~" & dataRowUtama(4) & "~" & dataRowUtama(5) & "~" & dataRowUtama(6) & "~" & dataRowUtama(7) & "~" & dataRowUtama(8) & "~" & dataRowUtama(9) & "~" & dataRowUtama(10) & "~" & dataRowUtama(11) & "~" & dataRowUtama(12) & "~" & dataRowUtama(13) & "~" & dataRowUtama(14) & "~" & dataRowUtama(15) & "~" & dataRowUtama(16) & "~" & dataRowUtama(17) & "~" & dataRowUtama(18) & "~" & dataRowUtama(19)) = False Then
                result(2) = "Main Row : " & i & " - Insert into main datatable failed." : GoTo selesai
            End If

        Next


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idaidetail(0) As Integer, idai(1) As Integer, idbarang(2) As Integer, jml(3) As Double, satuan(4) As String, 
        'customtext1(5) As String, customtext2(6) As String, customtext3(7) As String, customtext4(8) As String, customtext5(9) As String, 
        'customint1(10) As Integer, customint2(11) As Integer, customint3(12) As Integer, customdbl1(13) As Double, customdbl2(14) As Double, 
        'customdbl3(15) As Double, customdate1(16) As Date, customdate2(17) As Date, customdate3(18) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idaidetail, idai, idbarang, jml, satuan, customtext1, customtext2, 
        'customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idaidetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idai", AsEnumTypeData.AsInt64)
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
            'idaidetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idaidetail required numeric." : GoTo selesai
            End If
            'idai(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idai required numeric." : GoTo selesai
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
            'idaidetail(0) As Integer
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idaidetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idaidetail should not be more than 20 character." : GoTo selesai
            End If

            'idai(1) As Integer
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idai can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idai should not be more than 20 character." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idaidetail~idai~idbarang~jml~satuan~customtext1~customtext2~customtext3~customtext4~customtext5~customint1~customint2~customint3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18)) = False Then
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
                sql = "Delete from M_12_Pos_Additional_Item"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'Hapus detail
                sql = "Delete from M_12_Pos_Additional_Item_Detail"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()


                'Proses utama
                Dim strValue1 As New StringBuilder
                For Each dr1 As DataRow In dtutama.Rows
                    strValue1.Append(IIf(Len(strValue1.ToString) = 0, "", ", "))
                    strValue1.Append("('" & FixQuotes(dr1("aiid")) & "', '" & FixQuotes(dr1("aikategori")) & "', '" & FixQuotes(dr1("aiidbarang")) & "', '" & FixQuotes(dr1("aioperator")) & "', '" & FixDouble(dr1("aijml1")) & "', '" & FixDouble(dr1("aijml2")) & "', '" & FixQuotes(dr1("aicustomtext1")) & "', '" & FixQuotes(dr1("aicustomtext2")) & "', '" & FixQuotes(dr1("aicustomtext3")) & "', '" & FixQuotes(dr1("aicustomtext4")) & "', '" & FixQuotes(dr1("aicustomtext5")) & "', " & dr1("aicustomint1") & ", " & dr1("aicustomint2") & ", " & dr1("aicustomint3") & ", '" & FixDouble(dr1("aicustomdbl1")) & "', '" & FixDouble(dr1("aicustomdbl2")) & "', '" & FixDouble(dr1("aicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("aicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("aicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("aicustomdate3"))) & "')")
                Next
                sql = "Insert into M_12_Pos_Additional_Item(aiid, aikategori, aiidbarang, aioperator, aijml1, aijml2, aicustomtext1, aicustomtext2, aicustomtext3, aicustomtext4, aicustomtext5, aicustomint1, aicustomint2, aicustomint3, aicustomdbl1, aicustomdbl2, aicustomdbl3, aicustomdate1, aicustomdate2, aicustomdate3) values" & strValue1.ToString & ""
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
                        strValue2.Append("('" & FixQuotes(dr1("idaidetail")) & "', '" & FixQuotes(dr1("idai")) & "', '" & FixQuotes(dr1("idbarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M_12_Pos_Additional_Item_Detail(idaidetail, idai, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
    Public Function M12_Pos_Additional_ItemSimpanOld(ByVal param As String) As String
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
        'aiid(0) As Integer, aikategori(1) As String, aiidbarang(2) As Integer, aioperator(3) As String, aijml1(4) As Double, 
        'aijml2(5) As Double, aicustomtext1(6) As String, aicustomtext2(7) As String, aicustomtext3(8) As String, aicustomtext4(9) As String, 
        'aicustomtext5(10) As String, aicustomint1(11) As Integer, aicustomint2(12) As Integer, aicustomint3(13) As Integer, aicustomdbl1(14) As Double, 
        'aicustomdbl2(15) As Double, aicustomdbl3(16) As Double, aicustomdate1(17) As Date, aicustomdate2(18) As Date, aicustomdate3(19) As Date
        'aitgl1(20) As Date, aitgl2(21) As Date, ainopromo(21) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'aiid, aikategori, aiidbarang, aioperator, aijml1, aijml2, aicustomtext1, 
        'aicustomtext2, aicustomtext3, aicustomtext4, aicustomtext5, aicustomint1, aicustomint2, aicustomint3, 
        'aicustomdbl1, aicustomdbl2, aicustomdbl3, aicustomdate1, aicustomdate2, aicustomdate3
        'aitgl1, aitgl2, ainopromo

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 23) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'aiid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "aiid required numeric." : GoTo selesai
        End If
        'aiidbarang(2) As Integer
        If (IsNumeric(dataUtama(2)) = False) Then
            result(2) = "aiidbarang required numeric." : GoTo selesai
        End If
        'aijml1(4) As Double
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "aijml1 required numeric." : GoTo selesai
        End If
        'aijml2(5) As Double
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "aijml2 required numeric." : GoTo selesai
        End If
        'aicustomint1(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "aicustomint1 required numeric." : GoTo selesai
        End If
        'aicustomint2(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "aicustomint2 required numeric." : GoTo selesai
        End If
        'aicustomint3(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "aicustomint3 required numeric." : GoTo selesai
        End If
        'aicustomdbl1(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "aicustomdbl1 required numeric." : GoTo selesai
        End If
        'aicustomdbl2(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "aicustomdbl2 required numeric." : GoTo selesai
        End If
        'aicustomdbl3(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "aicustomdbl3 required numeric." : GoTo selesai
        End If
        'aicustomdate1(17) As Date
        If (IsDate(dataUtama(17)) = False) Then
            result(2) = "aicustomdate1 required date." : GoTo selesai
        End If
        'aicustomdate2(18) As Date
        If (IsDate(dataUtama(18)) = False) Then
            result(2) = "aicustomdate2 required date." : GoTo selesai
        End If
        'aicustomdate3(19) As Date
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "aicustomdate3 required date." : GoTo selesai
        End If
        'aitgl1(20) As Date
        If (IsDate(dataUtama(20)) = False) Then
            result(2) = "aitgl1 required date." : GoTo selesai
        End If
        'aitgl2(21) As Date
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "aitgl1 required date." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'aiid(0) As Integer
        If Len(dataUtama(0)) = 0 Then
            result(2) = "aiid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 20 Then
            result(2) = "aiid should not be more than 20 character." : GoTo selesai
        End If

        'aikategori(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "aikategori can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "aikategori should not be more than 25 character." : GoTo selesai
        End If

        'aiidbarang(2) As Integer
        If Len(dataUtama(2)) = 0 Then
            result(2) = "aiidbarang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 20 Then
            result(2) = "aiidbarang should not be more than 20 character." : GoTo selesai
        End If

        'aioperator(3) As String
        If IsNumeric(dataUtama(3)) = False Then
            result(2) = "aioperator required numeric" : GoTo selesai
        ElseIf dataUtama(3) <> 0 And dataUtama(3) <> 1 And dataUtama(3) <> 2 Then
            result(2) = "invalid aioperator value" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "aioperator should not be more than 25 character." : GoTo selesai
        End If

        'aijml1(4) As Double
        If Len(dataUtama(4)) = 0 Then
            result(2) = "aijml1 can't be empty" : GoTo selesai
        End If

        'aijml2(5) As Double
        If Len(dataUtama(5)) = 0 Then
            result(2) = "aijml2 can't be empty" : GoTo selesai
        End If

        'aicustomdbl1(14) As Double
        If Len(dataUtama(14)) = 0 Then
            result(2) = "aicustomdbl1 can't be empty" : GoTo selesai
        End If

        'aicustomdbl2(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "aicustomdbl2 can't be empty" : GoTo selesai
        End If

        'aicustomdbl3(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "aicustomdbl3 can't be empty" : GoTo selesai
        End If

        'aicustomdate1(17) As Date
        If Len(dataUtama(17)) = 0 Then
            result(2) = "aicustomdate1 can't be empty" : GoTo selesai
        End If

        'aicustomdate2(18) As Date
        If Len(dataUtama(18)) = 0 Then
            result(2) = "aicustomdate2 can't be empty" : GoTo selesai
        End If

        'aicustomdate3(19) As Date
        If Len(dataUtama(19)) = 0 Then
            result(2) = "aicustomdate3 can't be empty" : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "aiid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aiidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aioperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aijml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aijml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aitgl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aitgl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ainopromo", AsEnumTypeData.AsString)

        If AsDataTableTambahData(dtutama, "aiid~aikategori~aiidbarang~aioperator~aijml1~aijml2~aicustomtext1~aicustomtext2~aicustomtext3~aicustomtext4~aicustomtext5~aicustomint1~aicustomint2~aicustomint3~aicustomdbl1~aicustomdbl2~aicustomdbl3~aicustomdate1~aicustomdate2~aicustomdate3~aitgl1~aitgl2~ainopromo", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idaidetail(0) As Integer, idai(1) As Integer, idbarang(2) As Integer, jml(3) As Double, satuan(4) As String, 
        'customtext1(5) As String, customtext2(6) As String, customtext3(7) As String, customtext4(8) As String, customtext5(9) As String, 
        'customint1(10) As Integer, customint2(11) As Integer, customint3(12) As Integer, customdbl1(13) As Double, customdbl2(14) As Double, 
        'customdbl3(15) As Double, customdate1(16) As Date, customdate2(17) As Date, customdate3(18) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idaidetail, idai, idbarang, jml, satuan, customtext1, customtext2, 
        'customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idaidetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idai", AsEnumTypeData.AsInt64)
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
            'idaidetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idaidetail required numeric." : GoTo selesai
            End If
            'idai(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idai required numeric." : GoTo selesai
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
            'idaidetail(0) As Integer
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idaidetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idaidetail should not be more than 20 character." : GoTo selesai
            End If

            'idai(1) As Integer
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idai can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idai should not be more than 20 character." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idaidetail~idai~idbarang~jml~satuan~customtext1~customtext2~customtext3~customtext4~customtext5~customint1~customint2~customint3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18)) = False Then
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

                'CEK OPERATOR :
                'JIKA BETWEEN (0) ATAU >= (1) MAKA BOLEH LEBIH DARI SATU KONDISI 
                '=> BEBERAPA KONDISI BETWEEN DAN SATU KONDISI >= (1)
                'JIKA KELIPATAN (2) MAKA HANYA BOLEH SATU KONDISI
                Dim dtOperator As New DataTable
                sql = "SELECT ai.aikategori as kategori, ai.aiidbarang as idbarang, ai.aioperator as operator, i.bkode, (CASE ai.aioperator WHEN 0 THEN 'Between' WHEN 1 THEN '>=' WHEN 2 THEN 'Multiple' ELSE 'Unknown' END) as operatornama FROM m_12_pos_additional_item ai JOIN m1_item i ON ai.aiidbarang = i.bid WHERE ai.aikategori = '" & FxDB(drutama("aikategori"), "") & "' AND ai.aiidbarang = '" & FxDB(drutama("aiidbarang"), "") & "' AND ai.aiid <> '" & FxDB(drutama("aiid"), "") & "' GROUP BY ai.aioperator ORDER BY ai.aioperator"
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
                                If drutama("aioperator") = 2 Or (vOperator = 1 And drutama("aioperator") = vOperator) Then
                                    result(2) = "Item : " & FxDB(dr1("bkode"), "") & " - already has '" & FxDB(dr1("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                End If
                            End If
                        End If
                    Next
                End If

                'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drutama("aikategori")) & "' "

                If isUpdate Then
                    result(4) = drutama("aiid")
                    notransaksi = drutama("aikategori")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(aiid) FROM M_12_Pos_Additional_Item WHERE aiid = '" & result(4) & "'")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then
                        sql = "Update M_12_Pos_Additional_Item set aikategori  = '" & FixQuotes(drutama("aikategori")) & "', aiidbarang  = '" & FixQuotes(drutama("aiidbarang")) & "', aioperator  = '" & FixQuotes(drutama("aioperator")) & "', aijml1  = '" & FixDouble(drutama("aijml1")) & "', aijml2  = '" & FixDouble(drutama("aijml2")) & "', aicustomtext1  = '" & FixQuotes(drutama("aicustomtext1")) & "', aicustomtext2  = '" & FixQuotes(drutama("aicustomtext2")) & "', aicustomtext3  = '" & FixQuotes(drutama("aicustomtext3")) & "', aicustomtext4  = '" & FixQuotes(drutama("aicustomtext4")) & "', aicustomtext5  = '" & FixQuotes(drutama("aicustomtext5")) & "', aicustomint1  = " & drutama("aicustomint1") & ", aicustomint2  = " & drutama("aicustomint2") & ", aicustomint3  = " & drutama("aicustomint3") & ", aicustomdbl1  = '" & FixDouble(drutama("aicustomdbl1")) & "', aicustomdbl2  = '" & FixDouble(drutama("aicustomdbl2")) & "', aicustomdbl3  = '" & FixDouble(drutama("aicustomdbl3")) & "', aicustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("aicustomdate1"))) & "', aicustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("aicustomdate2"))) & "', aicustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("aicustomdate3"))) & "', aitgl1  = '" & FixQuotes(AsFormatTanggal(drutama("aitgl1"))) & "', aitgl2  = '" & FixQuotes(AsFormatTanggal(drutama("aitgl2"))) & "', ainopromo  = '" & FixQuotes(drutama("ainopromo")) & "' where aiid = '" & drutama("aiid") & "'"
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

                    sql = "Insert into M_12_Pos_Additional_Item (aikategori, aiidbarang, aioperator, aijml1, aijml2, aicustomtext1, aicustomtext2, aicustomtext3, aicustomtext4, aicustomtext5, aicustomint1, aicustomint2, aicustomint3, aicustomdbl1, aicustomdbl2, aicustomdbl3, aicustomdate1, aicustomdate2, aicustomdate3, aitgl1, aitgl2, ainopromo) values('" & FixQuotes(drutama("aikategori")) & "', '" & FixQuotes(drutama("aiidbarang")) & "', '" & FixQuotes(drutama("aioperator")) & "', '" & FixDouble(drutama("aijml1")) & "', '" & FixDouble(drutama("aijml2")) & "', '" & FixQuotes(drutama("aicustomtext1")) & "', '" & FixQuotes(drutama("aicustomtext2")) & "', '" & FixQuotes(drutama("aicustomtext3")) & "', '" & FixQuotes(drutama("aicustomtext4")) & "', '" & FixQuotes(drutama("aicustomtext5")) & "', " & drutama("aicustomint1") & ", " & drutama("aicustomint2") & ", " & drutama("aicustomint3") & ", '" & FixDouble(drutama("aicustomdbl1")) & "', '" & FixDouble(drutama("aicustomdbl2")) & "', '" & FixDouble(drutama("aicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("aicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("aicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("aicustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("aitgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("aitgl2"))) & "', '" & FixQuotes(drutama("ainopromo")) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select aiid from M_12_Pos_Additional_Item where aikategori = '" & drutama("aikategori") & "' AND aiidbarang = '" & drutama("aiidbarang") & "' AND aioperator = '" & drutama("aioperator") & "' AND aijml1 = '" & drutama("aijml1") & "' AND aijml2 = '" & drutama("aijml2") & "' limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M_12_Pos_Additional_Item_Detail where idai = '" & result(4) & "'"
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
                        strValue2.Append("('" & FixQuotes(dr1("idaidetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("idbarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M_12_Pos_Additional_Item_Detail(idaidetail, idai, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                Dim paramSearch As String = M12_Pos_Additional_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Additional_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Additional_ItemDeleteOld(ByVal param As String) As String

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
            sql = "SELECT aikategori as kategoripos FROM M_12_Pos_Additional_Item WHERE aiid = '" & idtransaksi & "' GROUP BY aikategori"
            Dim dtKategoriPOS As DataTable = AsDataTableAmbilDariDB(sql)
            If dtKategoriPOS.Rows.Count > 0 Then
                For Each drKategoriPOS As DataRow In dtKategoriPOS.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drKategoriPOS("kategoripos")) & "' "
                Next
            End If

            'DELETE DETAIL
            sql = "DELETE FROM M_12_Pos_Additional_Item_Detail WHERE idai = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M_12_Pos_Additional_Item WHERE aiid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_Pos_Additional_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Additional_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Additional_ItemImportOld(ByVal param As String) As String
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
        'aiid(0) As Integer, aikategori(1) As String, aiidbarang(2) As Integer, aioperator(3) As String, aijml1(4) As Double, 
        'aijml2(5) As Double, aicustomtext1(6) As String, aicustomtext2(7) As String, aicustomtext3(8) As String, aicustomtext4(9) As String, 
        'aicustomtext5(10) As String, aicustomint1(11) As Integer, aicustomint2(12) As Integer, aicustomint3(13) As Integer, aicustomdbl1(14) As Double, 
        'aicustomdbl2(15) As Double, aicustomdbl3(16) As Double, aicustomdate1(17) As Date, aicustomdate2(18) As Date, aicustomdate3(19) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'aiid, aikategori, aiidbarang, aioperator, aijml1, aijml2, aicustomtext1, 
        'aicustomtext2, aicustomtext3, aicustomtext4, aicustomtext5, aicustomint1, aicustomint2, aicustomint3, 
        'aicustomdbl1, aicustomdbl2, aicustomdbl3, aicustomdate1, aicustomdate2, aicustomdate3


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptRow)    'SPLIT PARAMETER DATA UTAMA
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "aiid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aiidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aioperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aijml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aijml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aicustomdate3", AsEnumTypeData.AsString)

        Dim JmlDtUtama As Integer = dataUtama.Length
        For i = 1 To JmlDtUtama
            'SPLIT DATA UTAMA
            dataRowUtama = dataUtama(i - 1).Split(sptField)

            'CEK ARRAY DATA UTAMA
            If (dataRowUtama.Length <> 20) Then
                result(2) = "Main Row : " & i & " - Invalid main transaction data parameter." : GoTo selesai
            End If

            'VALIDASI TIPE DATA UTAMA ==========================================================
            'aiid(0) As Integer
            If (IsNumeric(dataRowUtama(0)) = False) Then
                result(2) = "Main Row : " & i & " - aiid required numeric." : GoTo selesai
            End If
            'aiidbarang(2) As Integer
            If (IsNumeric(dataRowUtama(2)) = False) Then
                result(2) = "Main Row : " & i & " - aiidbarang required numeric." : GoTo selesai
            End If
            'aijml1(4) As Double
            If (IsNumeric(dataRowUtama(4)) = False) Then
                result(2) = "Main Row : " & i & " - aijml1 required numeric." : GoTo selesai
            End If
            'aijml2(5) As Double
            If (IsNumeric(dataRowUtama(5)) = False) Then
                result(2) = "Main Row : " & i & " - aijml2 required numeric." : GoTo selesai
            End If
            'aicustomint1(11) As Integer
            If (IsNumeric(dataRowUtama(11)) = False) Then
                result(2) = "Main Row : " & i & " - aicustomint1 required numeric." : GoTo selesai
            End If
            'aicustomint2(12) As Integer
            If (IsNumeric(dataRowUtama(12)) = False) Then
                result(2) = "Main Row : " & i & " - aicustomint2 required numeric." : GoTo selesai
            End If
            'aicustomint3(13) As Integer
            If (IsNumeric(dataRowUtama(13)) = False) Then
                result(2) = "Main Row : " & i & " - aicustomint3 required numeric." : GoTo selesai
            End If
            'aicustomdbl1(14) As Double
            If (IsNumeric(dataRowUtama(14)) = False) Then
                result(2) = "Main Row : " & i & " - aicustomdbl1 required numeric." : GoTo selesai
            End If
            'aicustomdbl2(15) As Double
            If (IsNumeric(dataRowUtama(15)) = False) Then
                result(2) = "Main Row : " & i & " - aicustomdbl2 required numeric." : GoTo selesai
            End If
            'aicustomdbl3(16) As Double
            If (IsNumeric(dataRowUtama(16)) = False) Then
                result(2) = "Main Row : " & i & " - aicustomdbl3 required numeric." : GoTo selesai
            End If
            'aicustomdate1(17) As Date
            If (IsDate(dataRowUtama(17)) = False) Then
                result(2) = "Main Row : " & i & " - aicustomdate1 required date." : GoTo selesai
            End If
            'aicustomdate2(18) As Date
            If (IsDate(dataRowUtama(18)) = False) Then
                result(2) = "Main Row : " & i & " - aicustomdate2 required date." : GoTo selesai
            End If
            'aicustomdate3(19) As Date
            If (IsDate(dataRowUtama(19)) = False) Then
                result(2) = "Main Row : " & i & " - aicustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA UTAMA ===================================================

            'VALIDASI DATA UTAMA =======================================================
            'aiid(0) As Integer
            If Len(dataRowUtama(0)) = 0 Then
                result(2) = "Main Row : " & i & " - aiid can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(0)) > 20 Then
                result(2) = "Main Row : " & i & " - aiid should not be more than 20 character." : GoTo selesai
            End If

            'aikategori(1) As String
            If Len(dataRowUtama(1)) = 0 Then
                result(2) = "Main Row : " & i & " - aikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(1)) > 25 Then
                result(2) = "Main Row : " & i & " - aikategori should not be more than 25 character." : GoTo selesai
            End If

            'aiidbarang(2) As Integer
            If Len(dataRowUtama(2)) = 0 Then
                result(2) = "Main Row : " & i & " - aiidbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(2)) > 20 Then
                result(2) = "Main Row : " & i & " - aiidbarang should not be more than 20 character." : GoTo selesai
            End If

            'aioperator(3) As String
            If IsNumeric(dataRowUtama(3)) = False Then
                result(2) = "Main Row : " & i & " - aioperator required numeric" : GoTo selesai
            ElseIf dataRowUtama(3) <> 0 And dataRowUtama(3) <> 1 And dataRowUtama(3) <> 2 Then
                result(2) = "Main Row : " & i & " - invalid aioperator value" : GoTo selesai
            End If
            If Len(dataRowUtama(3)) > 25 Then
                result(2) = "Main Row : " & i & " - aioperator should not be more than 25 character." : GoTo selesai
            End If

            'aijml1(4) As Double
            If Len(dataRowUtama(4)) = 0 Then
                result(2) = "Main Row : " & i & " - aijml1 can't be empty" : GoTo selesai
            End If

            'aijml2(5) As Double
            If Len(dataRowUtama(5)) = 0 Then
                result(2) = "Main Row : " & i & " - aijml2 can't be empty" : GoTo selesai
            End If

            'aicustomdbl1(14) As Double
            If Len(dataRowUtama(14)) = 0 Then
                result(2) = "Main Row : " & i & " - aicustomdbl1 can't be empty" : GoTo selesai
            End If

            'aicustomdbl2(15) As Double
            If Len(dataRowUtama(15)) = 0 Then
                result(2) = "Main Row : " & i & " - aicustomdbl2 can't be empty" : GoTo selesai
            End If

            'aicustomdbl3(16) As Double
            If Len(dataRowUtama(16)) = 0 Then
                result(2) = "Main Row : " & i & " - aicustomdbl3 can't be empty" : GoTo selesai
            End If

            'aicustomdate1(17) As Date
            If Len(dataRowUtama(17)) = 0 Then
                result(2) = "Main Row : " & i & " - aicustomdate1 can't be empty" : GoTo selesai
            End If

            'aicustomdate2(18) As Date
            If Len(dataRowUtama(18)) = 0 Then
                result(2) = "Main Row : " & i & " - aicustomdate2 can't be empty" : GoTo selesai
            End If

            'aicustomdate3(19) As Date
            If Len(dataRowUtama(19)) = 0 Then
                result(2) = "Main Row : " & i & " - aicustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA UTAMA ================================================


            If AsDataTableTambahData(dtutama, "aiid~aikategori~aiidbarang~aioperator~aijml1~aijml2~aicustomtext1~aicustomtext2~aicustomtext3~aicustomtext4~aicustomtext5~aicustomint1~aicustomint2~aicustomint3~aicustomdbl1~aicustomdbl2~aicustomdbl3~aicustomdate1~aicustomdate2~aicustomdate3", dataRowUtama(0) & "~" & dataRowUtama(1) & "~" & dataRowUtama(2) & "~" & dataRowUtama(3) & "~" & dataRowUtama(4) & "~" & dataRowUtama(5) & "~" & dataRowUtama(6) & "~" & dataRowUtama(7) & "~" & dataRowUtama(8) & "~" & dataRowUtama(9) & "~" & dataRowUtama(10) & "~" & dataRowUtama(11) & "~" & dataRowUtama(12) & "~" & dataRowUtama(13) & "~" & dataRowUtama(14) & "~" & dataRowUtama(15) & "~" & dataRowUtama(16) & "~" & dataRowUtama(17) & "~" & dataRowUtama(18) & "~" & dataRowUtama(19)) = False Then
                result(2) = "Main Row : " & i & " - Insert into main datatable failed." : GoTo selesai
            End If

        Next


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idaidetail(0) As Integer, idai(1) As Integer, idbarang(2) As Integer, jml(3) As Double, satuan(4) As String, 
        'customtext1(5) As String, customtext2(6) As String, customtext3(7) As String, customtext4(8) As String, customtext5(9) As String, 
        'customint1(10) As Integer, customint2(11) As Integer, customint3(12) As Integer, customdbl1(13) As Double, customdbl2(14) As Double, 
        'customdbl3(15) As Double, customdate1(16) As Date, customdate2(17) As Date, customdate3(18) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idaidetail, idai, idbarang, jml, satuan, customtext1, customtext2, 
        'customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idaidetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idai", AsEnumTypeData.AsInt64)
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
            'idaidetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idaidetail required numeric." : GoTo selesai
            End If
            'idai(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idai required numeric." : GoTo selesai
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
            'idaidetail(0) As Integer
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idaidetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idaidetail should not be more than 20 character." : GoTo selesai
            End If

            'idai(1) As Integer
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idai can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idai should not be more than 20 character." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idaidetail~idai~idbarang~jml~satuan~customtext1~customtext2~customtext3~customtext4~customtext5~customint1~customint2~customint3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18)) = False Then
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
                sql = "Delete from M_12_Pos_Additional_Item"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'Hapus detail
                sql = "Delete from M_12_Pos_Additional_Item_Detail"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()


                'Proses utama
                Dim strValue1 As New StringBuilder
                For Each dr1 As DataRow In dtutama.Rows
                    strValue1.Append(IIf(Len(strValue1.ToString) = 0, "", ", "))
                    strValue1.Append("('" & FixQuotes(dr1("aiid")) & "', '" & FixQuotes(dr1("aikategori")) & "', '" & FixQuotes(dr1("aiidbarang")) & "', '" & FixQuotes(dr1("aioperator")) & "', '" & FixDouble(dr1("aijml1")) & "', '" & FixDouble(dr1("aijml2")) & "', '" & FixQuotes(dr1("aicustomtext1")) & "', '" & FixQuotes(dr1("aicustomtext2")) & "', '" & FixQuotes(dr1("aicustomtext3")) & "', '" & FixQuotes(dr1("aicustomtext4")) & "', '" & FixQuotes(dr1("aicustomtext5")) & "', " & dr1("aicustomint1") & ", " & dr1("aicustomint2") & ", " & dr1("aicustomint3") & ", '" & FixDouble(dr1("aicustomdbl1")) & "', '" & FixDouble(dr1("aicustomdbl2")) & "', '" & FixDouble(dr1("aicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("aicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("aicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("aicustomdate3"))) & "')")
                Next
                sql = "Insert into M_12_Pos_Additional_Item(aiid, aikategori, aiidbarang, aioperator, aijml1, aijml2, aicustomtext1, aicustomtext2, aicustomtext3, aicustomtext4, aicustomtext5, aicustomint1, aicustomint2, aicustomint3, aicustomdbl1, aicustomdbl2, aicustomdbl3, aicustomdate1, aicustomdate2, aicustomdate3) values" & strValue1.ToString & ""
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
                        strValue2.Append("('" & FixQuotes(dr1("idaidetail")) & "', '" & FixQuotes(dr1("idai")) & "', '" & FixQuotes(dr1("idbarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M_12_Pos_Additional_Item_Detail(idaidetail, idai, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
    Public Function M12_Pos_Additional_ItemGetdataById(ByVal param As String) As String

        'M12_Pos_Additional_ItemGetdataById Utama --------------------------------------------------------
        'aiid, aikategori, aiidbarang, aioperator, aijml1, aijml2, aicustomtext1, 
        'aicustomtext2, aicustomtext3, aicustomtext4, aicustomtext5, aicustomint1, aicustomint2, aicustomint3, 
        'aicustomdbl1, aicustomdbl2, aicustomdbl3, aicustomdate1, aicustomdate2, aicustomdate3, pcnama, 
        'bkode, bnama, btipe, bsatuan, aioperatornama, aitgl1, aitgl2, ainopromo

        'M12_Pos_Additional_ItemGetdataById Detail -------------------------------------------------------
        'idaidetail, idai, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, 
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

        Dim NmMemcached As String = "aplikasi1-M_12_Pos_Additional_Item~M_12_Pos_Additional_Item_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "aiid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "aiid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        sql = "select `ai`.`aiid` AS `aiid`,`ai`.`aikategori` AS `aikategori`,`ai`.`aiidbarang` AS `aiidbarang`,`ai`.`aioperator` AS `aioperator`,`ai`.`aijml1` AS `aijml1`,`ai`.`aijml2` AS `aijml2`,`ai`.`aicustomtext1` AS `aicustomtext1`,`ai`.`aicustomtext2` AS `aicustomtext2`,`ai`.`aicustomtext3` AS `aicustomtext3`,`ai`.`aicustomtext4` AS `aicustomtext4`,`ai`.`aicustomtext5` AS `aicustomtext5`,`ai`.`aicustomint1` AS `aicustomint1`,`ai`.`aicustomint2` AS `aicustomint2`,`ai`.`aicustomint3` AS `aicustomint3`,`ai`.`aicustomdbl1` AS `aicustomdbl1`,`ai`.`aicustomdbl2` AS `aicustomdbl2`,`ai`.`aicustomdbl3` AS `aicustomdbl3`,`ai`.`aicustomdate1` AS `aicustomdate1`,`ai`.`aicustomdate2` AS `aicustomdate2`,`ai`.`aicustomdate3` AS `aicustomdate3`,`pc`.`pcnama` AS `pcnama`,`i`.`bkode` AS `bkode`,`i`.`bnama` AS `bnama`,`i`.`btipe` AS `btipe`,`i`.`bsatuan` AS `bsatuan`,(case `ai`.`aioperator` when 0 then 'Between' when 1 then '>=' when 2 then 'Multiple' else 'Unknown' end) AS `aioperatornama`, `ai`.`aitgl1` AS `aitgl1`, `ai`.`aitgl2` AS `aitgl2`, `ai`.`ainopromo` AS `ainopromo`,`aid`.`idaidetail` AS `idaidetail`,`aid`.`idai` AS `idai`,`aid`.`idbarang` AS `idbarang`,`aid`.`jml` AS `jml`,`i2`.`bsatuan` AS `satuan`,`aid`.`customtext1` AS `customtext1`,`aid`.`customtext2` AS `customtext2`,`aid`.`customtext3` AS `customtext3`,`aid`.`customtext4` AS `customtext4`,`aid`.`customtext5` AS `customtext5`,`aid`.`customint1` AS `customint1`,`aid`.`customint2` AS `customint2`,`aid`.`customint3` AS `customint3`,`aid`.`customdbl1` AS `customdbl1`,`aid`.`customdbl2` AS `customdbl2`,`aid`.`customdbl3` AS `customdbl3`,`aid`.`customdate1` AS `customdate1`,`aid`.`customdate2` AS `customdate2`,`aid`.`customdate3` AS `customdate3`,`i2`.`bkode` AS `kodebarang`,`i2`.`bnama` AS `namabarang`,`i2`.`btipe` AS `tipebarang` from ((((`M_12_Pos_Additional_Item` `ai` join `m_12_pos_category` `pc` on((`ai`.`aikategori` = `pc`.`pckode`))) join `m1_item` `i` on((`ai`.`aiidbarang` = `i`.`bid`))) join `M_12_Pos_Additional_Item_detail` `aid` on((`ai`.`aiid` = `aid`.`idai`))) join `m1_item` `i2` on((`aid`.`idbarang` = `i2`.`bid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                     FxDB(drutama("aiid"), ""), sptField,
                     FxDB(drutama("aikategori"), ""), sptField,
                     FxDB(drutama("aiidbarang"), ""), sptField,
                     FxDB(drutama("aioperator"), ""), sptField,
                     FxDB(drutama("aijml1"), 0), sptField,
                     FxDB(drutama("aijml2"), 0), sptField,
                     FxDB(drutama("aicustomtext1"), ""), sptField,
                     FxDB(drutama("aicustomtext2"), ""), sptField,
                     FxDB(drutama("aicustomtext3"), ""), sptField,
                     FxDB(drutama("aicustomtext4"), ""), sptField,
                     FxDB(drutama("aicustomtext5"), ""), sptField,
                     FxDB(drutama("aicustomint1"), 0), sptField,
                     FxDB(drutama("aicustomint2"), 0), sptField,
                     FxDB(drutama("aicustomint3"), 0), sptField,
                     FxDB(drutama("aicustomdbl1"), 0), sptField,
                     FxDB(drutama("aicustomdbl2"), 0), sptField,
                     FxDB(drutama("aicustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("aicustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("aicustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("aicustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("pcnama"), ""), sptField,
                     FxDB(drutama("bkode"), ""), sptField,
                     FxDB(drutama("bnama"), ""), sptField,
                     FxDB(drutama("btipe"), ""), sptField,
                     FxDB(drutama("bsatuan"), ""), sptField,
                     FxDB(drutama("aioperatornama"), ""), sptField,
                     FxDB(drutama("aitgl1"), ""), sptField,
                     FxDB(drutama("aitgl2"), ""), sptField,
                     FxDB(drutama("ainopromo"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idaidetail"), ""), sptField,
                     FxDB(dr("idai"), ""), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("aiid, aikategori, aiidbarang, aioperator, aijml1, aijml2, aicustomtext1, aicustomtext2, aicustomtext3, aicustomtext4, aicustomtext5, aicustomint1, aicustomint2, aicustomint3, aicustomdbl1, aicustomdbl2, aicustomdbl3, aicustomdate1, aicustomdate2, aicustomdate3, pcnama, bkode, bnama, btipe, bsatuan, aioperatornama, aitgl1, aitgl2, ainopromo" & sptSubParam & "idaidetail, idai, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, namabarang, tipebarang"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_Additional_ItemSearch(ByVal param As String) As String
        'JIKA MENAMBAHKAN FIELD DISINI MAKA HARUS TAMBAHKAN JUGA LENGTH ARRAY PADA VALIDASI AMBIL USERID WS M0_LOGIN
        'M12_Pos_Additional_ItemSearch --------------------------------------------------------
        'aiid, aikategori, aiidbarang, aioperator, aijml1, aijml2, aicustomtext1, 
        'aicustomtext2, aicustomtext3, aicustomtext4, aicustomtext5, aicustomint1, aicustomint2, aicustomint3, 
        'aicustomdbl1, aicustomdbl2, aicustomdbl3, aicustomdate1, aicustomdate2, aicustomdate3, pcnama, 
        'bkode, bnama, btipe, bsatuan, aioperatornama, aitgl1, aitgl2, ainopromo

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
        sql = "select `ai`.`aiid` AS `aiid`,`ai`.`aikategori` AS `aikategori`,`ai`.`aiidbarang` AS `aiidbarang`,`ai`.`aioperator` AS `aioperator`,`ai`.`aijml1` AS `aijml1`,`ai`.`aijml2` AS `aijml2`,`ai`.`aicustomtext1` AS `aicustomtext1`,`ai`.`aicustomtext2` AS `aicustomtext2`,`ai`.`aicustomtext3` AS `aicustomtext3`,`ai`.`aicustomtext4` AS `aicustomtext4`,`ai`.`aicustomtext5` AS `aicustomtext5`,`ai`.`aicustomint1` AS `aicustomint1`,`ai`.`aicustomint2` AS `aicustomint2`,`ai`.`aicustomint3` AS `aicustomint3`,`ai`.`aicustomdbl1` AS `aicustomdbl1`,`ai`.`aicustomdbl2` AS `aicustomdbl2`,`ai`.`aicustomdbl3` AS `aicustomdbl3`,`ai`.`aicustomdate1` AS `aicustomdate1`,`ai`.`aicustomdate2` AS `aicustomdate2`,`ai`.`aicustomdate3` AS `aicustomdate3`,`pc`.`pcnama` AS `pcnama`,`i`.`bkode` AS `bkode`,`i`.`bnama` AS `bnama`,`i`.`btipe` AS `btipe`,`i`.`bsatuan` AS `bsatuan`,(case `ai`.`aioperator` when 0 then 'Between' when 1 then '>=' when 2 then 'Multiple' else 'Unknown' end) AS `aioperatornama`, `aitgl1` AS `aitgl1`, `aitgl2` AS `aitgl2`, `ainopromo` AS `ainopromo` from ((`M_12_Pos_Additional_Item` `ai` join `m_12_pos_category` `pc` on((`ai`.`aikategori` = `pc`.`pckode`))) join `m1_item` `i` on((`ai`.`aiidbarang` = `i`.`bid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M_12_Pos_Additional_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("aiid"), ""), sptField,
                     FxDB(dr("aikategori"), ""), sptField,
                     FxDB(dr("aiidbarang"), ""), sptField,
                     FxDB(dr("aioperator"), ""), sptField,
                     FxDB(dr("aijml1"), 0), sptField,
                     FxDB(dr("aijml2"), 0), sptField,
                     FxDB(dr("aicustomtext1"), ""), sptField,
                     FxDB(dr("aicustomtext2"), ""), sptField,
                     FxDB(dr("aicustomtext3"), ""), sptField,
                     FxDB(dr("aicustomtext4"), ""), sptField,
                     FxDB(dr("aicustomtext5"), ""), sptField,
                     FxDB(dr("aicustomint1"), 0), sptField,
                     FxDB(dr("aicustomint2"), 0), sptField,
                     FxDB(dr("aicustomint3"), 0), sptField,
                     FxDB(dr("aicustomdbl1"), 0), sptField,
                     FxDB(dr("aicustomdbl2"), 0), sptField,
                     FxDB(dr("aicustomdbl3"), 0), sptField,
                     FxDB(dr("aicustomdate1"), ""), sptField,
                     FxDB(dr("aicustomdate2"), ""), sptField,
                     FxDB(dr("aicustomdate3"), ""), sptField,
                     FxDB(dr("pcnama"), ""), sptField,
                     FxDB(dr("bkode"), ""), sptField,
                     FxDB(dr("bnama"), ""), sptField,
                     FxDB(dr("btipe"), ""), sptField,
                     FxDB(dr("bsatuan"), ""), sptField,
                     FxDB(dr("aioperatornama"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aitgl1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("aitgl2"), ""), formatTgl), sptField,
                     FxDB(dr("ainopromo"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("aiid, aikategori, aiidbarang, aioperator, aijml1, aijml2, aicustomtext1, aicustomtext2, aicustomtext3, aicustomtext4, aicustomtext5, aicustomint1, aicustomint2, aicustomint3, aicustomdbl1, aicustomdbl2, aicustomdbl3, aicustomdate1, aicustomdate2, aicustomdate3, pcnama, bkode, bnama, btipe, bsatuan, aioperatornama, aitgl1, aitgl2, ainopromo"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_Additional_Item_DetailSearch(ByVal param As String) As String
        'M12_Pos_Additional_Item_DetailSearch --------------------------------------------------------
        'idaidetail, idai, idbarang, jml, satuan, customtext1, customtext2, 
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
        sql = "select `aid`.`idaidetail` AS `idaidetail`,`aid`.`idai` AS `idai`,`aid`.`idbarang` AS `idbarang`,`aid`.`jml` AS `jml`,`i`.`bsatuan` AS `satuan`,`aid`.`customtext1` AS `customtext1`,`aid`.`customtext2` AS `customtext2`,`aid`.`customtext3` AS `customtext3`,`aid`.`customtext4` AS `customtext4`,`aid`.`customtext5` AS `customtext5`,`aid`.`customint1` AS `customint1`,`aid`.`customint2` AS `customint2`,`aid`.`customint3` AS `customint3`,`aid`.`customdbl1` AS `customdbl1`,`aid`.`customdbl2` AS `customdbl2`,`aid`.`customdbl3` AS `customdbl3`,`aid`.`customdate1` AS `customdate1`,`aid`.`customdate2` AS `customdate2`,`aid`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`i`.`bnama` AS `namabarang`,`i`.`btipe` AS `tipebarang` from (`m_12_pos_additional_item_detail` `aid` join `m1_item` `i` on((`aid`.`idbarang` = `i`.`bid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M_12_Pos_Additional_Item_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idaidetail"), ""), sptField,
                     FxDB(dr("idai"), ""), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idaidetail, idai, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, namabarang, tipebarang"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_Additional_Item_DetailSetting(ByVal param As String) As String
        'JIKA MENAMBAHKAN FIELD DISINI MAKA HARUS TAMBAHKAN JUGA LENGTH ARRAY PADA VALIDASI AMBIL USERID WS M0_LOGIN
        'M12_Pos_Additional_Item_DetailSetting --------------------------------------------------------
        'aiid, bid, bkode, bnama, btipe, bjenis, bkategori, bsatuan, 
        'bsatuandefault, bhpp, bbarcode, bhargabeli, bhppaverage, bhargajual1, bhargajual2, 
        'bhargajual3, bhargajual4, bhargajual5, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, 
        'bdiskonjual5, bstok, bstokbooking, bmarginminimal, brekpersediaan, brekpenjualan, brekreturpenjualan, brekdiskonpenjualan, 
        'brekhargapokok, brekreturpembelian, brekdiskonpembelian, brekkonsinyasi, bserial, bbatch, bnilaisatuan, 
        'bnilaisatuandefault, bsuplier, bsuplierkode, bsupliernama, bnamafile, bapanjang, balebar, batinggi,
        'bstokminimal, bstokmaksimal, breorder, jml, 
        'customtext1, customtext2, customtext3, customtext4, customtext5, 
        'customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3


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
        sql = "SELECT ai.aiid, i.bid AS bid, i.bkode AS bkode, i.bnama AS bnama, i.btipe AS btipe, i.bjenis AS bjenis, i.bkategori AS bkategori, i.bsatuan AS bsatuan, i.bsatuandefault AS bsatuandefault, i.bhpp AS bhpp, i.bbarcode AS bbarcode, i.bhargabeli AS bhargabeli, i.bhppaverage AS bhppaverage, pi.pihargajual1 AS bhargajual1, pi.pihargajual2 AS bhargajual2, pi.pihargajual3 AS bhargajual3, pi.pihargajual4 AS bhargajual4, pi.pihargajual5 AS bhargajual5, pi.pidiskonjual1 AS bdiskonjual1, pi.pidiskonjual2 AS bdiskonjual2, pi.pidiskonjual3 AS bdiskonjual3, pi.pidiskonjual4 AS bdiskonjual4, pi.pidiskonjual5 AS bdiskonjual5, i.bstok AS bstok, ifnull(sum(`ib`.`jmlbooking`),0) AS bstokbooking, i.bmarginminimal AS bmarginminimal, i.brekpersediaan AS brekpersediaan, i.brekpenjualan AS brekpenjualan, i.brekreturpenjualan AS brekreturpenjualan, i.brekdiskonpenjualan AS brekdiskonpenjualan, i.brekhargapokok AS brekhargapokok, i.brekreturpembelian AS brekreturpembelian, i.brekdiskonpembelian AS brekdiskonpembelian, i.brekkonsinyasi AS brekkonsinyasi, i.bserial AS bserial, i.bbatch AS bbatch, i.bnilaisatuan AS bnilaisatuan, i.bnilaisatuandefault AS bnilaisatuandefault, i.bsuplier AS bsuplier, c.kkode AS bsuplierkode, c.knama AS bsupliernama, f.fnamafile AS bnamafile, i.bapanjang AS bapanjang, i.balebar AS balebar, i.batinggi AS batinggi, pi.pistokminimal AS bstokminimal, pi.pistokmaksimal AS bstokmaksimal, pi.pistokreorder AS breorder, aid.jml, aid.customtext1, aid.customtext2, aid.customtext3, aid.customtext4, aid.customtext5, aid.customint1, aid.customint2, aid.customint3, aid.customdbl1, aid.customdbl2, aid.customdbl3, aid.customdate1, aid.customdate2, aid.customdate3 from `m1_item` `i`  JOIN m_12_pos_additional_item_detail aid ON i.bid = aid.idbarang JOIN m_12_pos_additional_item ai ON aid.idai = ai.aiid JOIN m_12_pos_item pi ON aid.idbarang = pi.piidbarang AND ai.aikategori = pi.pikategori left join `m1_item_booking` `ib` on `i`.`bid` = `ib`.`idbarang` left join `m1_contact` `c` on `i`.`bsuplier` = `c`.`kid` left join `m1_files` `f` on `f`.`fsumber` = 'Item' and `i`.`bid` = `f`.`fidtransaksi` and `f`.`fdefault` = 1"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "i.bid, aid.idaidetail", sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("aiid"), 0), sptField,
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
                     FxDB(dr("jml"), 0), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("aiid, bid, bkode, bnama, btipe, bjenis, bkategori, bsatuan, bsatuandefault, bhpp, bbarcode, bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bstokbooking, bmarginminimal, brekpersediaan, brekpenjualan, brekreturpenjualan, brekdiskonpenjualan, brekhargapokok, brekreturpembelian, brekdiskonpembelian, brekkonsinyasi, bserial, bbatch, bnilaisatuan, bnilaisatuandefault, bsuplier, bsuplierkode, bsupliernama, bnamafile, bapanjang, balebar, batinggi, bstokminimal, bstokmaksimal, breorder, jml, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_Additional_ItemDownload(ByVal param As String) As String
        'M12_Pos_Additional_ItemDownload --------------------------------------------------------
        'Utama
        'aiid, aikategori, aiidbarang, aioperator, aijml1, aijml2, aicustomtext1, 
        'aicustomtext2, aicustomtext3, aicustomtext4, aicustomtext5, aicustomint1, aicustomint2, aicustomint3, 
        'aicustomdbl1, aicustomdbl2, aicustomdbl3, aicustomdate1, aicustomdate2, aicustomdate3

        'Detail
        'idaidetail, idai, idbarang, jml, satuan, customtext1, customtext2, 
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

        dt = AmbilData("aplikasi1-M_12_Pos_Additional_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("aiid"), ""), sptField,
                     FxDB(dr("aikategori"), ""), sptField,
                     FxDB(dr("aiidbarang"), ""), sptField,
                     FxDB(dr("aioperator"), ""), sptField,
                     FxDB(dr("aijml1"), 0), sptField,
                     FxDB(dr("aijml2"), 0), sptField,
                     FxDB(dr("aicustomtext1"), ""), sptField,
                     FxDB(dr("aicustomtext2"), ""), sptField,
                     FxDB(dr("aicustomtext3"), ""), sptField,
                     FxDB(dr("aicustomtext4"), ""), sptField,
                     FxDB(dr("aicustomtext5"), ""), sptField,
                     FxDB(dr("aicustomint1"), 0), sptField,
                     FxDB(dr("aicustomint2"), 0), sptField,
                     FxDB(dr("aicustomint3"), 0), sptField,
                     FxDB(dr("aicustomdbl1"), 0), sptField,
                     FxDB(dr("aicustomdbl2"), 0), sptField,
                     FxDB(dr("aicustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("aicustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("aicustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("aicustomdate3"), ""), formatTgl), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)


            'AMBIL DATA DETAIL
            sql = "SELECT aid.idaidetail, aid.idai, aid.idbarang, aid.jml, aid.satuan, aid.customtext1, aid.customtext2, aid.customtext3, aid.customtext4, aid.customtext5, aid.customint1, aid.customint2, aid.customint3, aid.customdbl1, aid.customdbl2, aid.customdbl3, aid.customdate1, aid.customdate2, aid.customdate3 FROM m_12_pos_additional_item ai JOIN m_12_pos_additional_item_detail aid ON ai.aiid = aid.idai"

            Dim dtdetail As New DataTable
            dtdetail = AmbilData("aplikasi1-M_12_Pos_Additional_Item_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtdetail.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idaidetail"), ""), sptField,
                     FxDB(dr("idai"), ""), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("aiid, aikategori, aiidbarang, aioperator, aijml1, aijml2, aicustomtext1, aicustomtext2, aicustomtext3, aicustomtext4, aicustomtext5, aicustomint1, aicustomint2, aicustomint3, aicustomdbl1, aicustomdbl2, aicustomdbl3, aicustomdate1, aicustomdate2, aicustomdate3" & sptSubParam & "idaidetail, idai, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3"))

        Return wsResult
    End Function

End Class
