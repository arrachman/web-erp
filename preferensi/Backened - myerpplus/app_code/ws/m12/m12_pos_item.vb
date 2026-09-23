Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_pos_item
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_Pos_ItemSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
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
        'pikategori(0) As String, piidbarang(1) As Integer, pistokminimal(2) As Double, pistokmaksimal(3) As Double, pistokreorder(4) As Double, 
        'pihargajual1(5) As Double, pihargajual2(6) As Double, pihargajual3(7) As Double, pihargajual4(8) As Double, pihargajual5(9) As Double, 
        'pidiskonjual1(10) As String, pidiskonjual2(11) As String, pidiskonjual3(12) As String, pidiskonjual4(13) As String, pidiskonjual5(14) As String, 
        'picustomtext1(15) As String, picustomtext2(16) As String, picustomtext3(17) As String, picustomtext4(18) As String, picustomtext5(19) As String, 
        'picustomint1(20) As Integer, picustomint2(21) As Integer, picustomint3(22) As Integer, picustomdbl1(23) As Double, picustomdbl2(24) As Double, 
        'picustomdbl3(25) As Double, picustomdate1(26) As Date, picustomdate2(27) As Date, picustomdate3(28) As Date, piaksi(29) As Integer, pistokminorder(30) As Double

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'pikategori, piidbarang, pistokminimal, pistokmaksimal, pistokreorder, pihargajual1, pihargajual2, 
        'pihargajual3, pihargajual4, pihargajual5, pidiskonjual1, pidiskonjual2, pidiskonjual3, pidiskonjual4, 
        'pidiskonjual5, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, 
        'picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, 
        'picustomdate3, piaksi, pistokminorder

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "piidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pistokminimal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pistokmaksimal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pistokreorder", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pihargajual1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pihargajual2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pihargajual3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pihargajual4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pihargajual5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pidiskonjual1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pidiskonjual2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pidiskonjual3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pidiskonjual4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pidiskonjual5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "picustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "picustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "picustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "piaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pistokminorder", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pihargaedited", AsEnumTypeData.AsString) 'tambahan pihargaedited'

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


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            'If (dataRowDetail.Length <> 31) Then
            '    result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            'End If
            If (dataRowDetail.Length <> 32) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'piidbarang(1) As Double
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - piidbarang required numeric." : GoTo selesai
            End If
            'pistokminimal(2) As Double
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - pistokminimal required numeric." : GoTo selesai
            End If
            'pistokmaksimal(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - pistokmaksimal required numeric." : GoTo selesai
            End If
            'pistokreorder(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - pistokreorder required numeric." : GoTo selesai
            End If
            'pihargajual1(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - pihargajual1 required numeric." : GoTo selesai
            End If
            'pihargajual2(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - pihargajual2 required numeric." : GoTo selesai
            End If
            'pihargajual3(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - pihargajual3 required numeric." : GoTo selesai
            End If
            'pihargajual4(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - pihargajual4 required numeric." : GoTo selesai
            End If
            'pihargajual5(9) As Double
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - pihargajual5 required numeric." : GoTo selesai
            End If
            'picustomint1(20) As Integer
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - picustomint1 required numeric." : GoTo selesai
            End If
            'picustomint2(21) As Integer
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - picustomint2 required numeric." : GoTo selesai
            End If
            'picustomint3(22) As Integer
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - picustomint3 required numeric." : GoTo selesai
            End If
            'picustomdbl1(23) As Double
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - picustomdbl1 required numeric." : GoTo selesai
            End If
            'picustomdbl2(24) As Double
            If (IsNumeric(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - picustomdbl2 required numeric." : GoTo selesai
            End If
            'picustomdbl3(25) As Double
            If (IsNumeric(dataRowDetail(25)) = False) Then
                result(2) = "Row : " & i & " - picustomdbl3 required numeric." : GoTo selesai
            End If
            'picustomdate1(26) As Date
            If (IsDate(dataRowDetail(26)) = False) Then
                result(2) = "Row : " & i & " - picustomdate1 required date." : GoTo selesai
            End If
            'picustomdate2(27) As Date
            If (IsDate(dataRowDetail(27)) = False) Then
                result(2) = "Row : " & i & " - picustomdate2 required date." : GoTo selesai
            End If
            'picustomdate3(28) As Date
            If (IsDate(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - picustomdate3 required date." : GoTo selesai
            End If
            'piaksi(29) As Integer
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - piaksi required numeric." : GoTo selesai
            ElseIf (dataRowDetail(29) <> 0 And dataRowDetail(29) <> 1) Then
                result(2) = "Row : " & i & " - invalid piaksi value. (0:delete, 1:insert/update)" : GoTo selesai
            End If
            'pistokminorder(30) As Double
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - pistokminorder required numeric." : GoTo selesai
            End If

            'pihargaedited(30) As Double
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - pihargaedited required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If

            'piidbarang(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - piidbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - piidbarang should not be more than 20 character." : GoTo selesai
            End If

            'pistokminimal(2) As Double
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - pistokminimal can't be empty" : GoTo selesai
            End If

            'pistokmaksimal(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - pistokmaksimal can't be empty" : GoTo selesai
            End If

            'pistokreorder(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - pistokreorder can't be empty" : GoTo selesai
            End If

            'pihargajual1(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - pihargajual1 can't be empty" : GoTo selesai
            End If

            'pihargajual2(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - pihargajual2 can't be empty" : GoTo selesai
            End If

            'pihargajual3(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - pihargajual3 can't be empty" : GoTo selesai
            End If

            'pihargajual4(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - pihargajual4 can't be empty" : GoTo selesai
            End If

            'pihargajual5(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - pihargajual5 can't be empty" : GoTo selesai
            End If

            'pidiskonjual1(10) As String
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - pidiskonjual1 can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(10)) > 25 Then
                result(2) = "Row : " & i & " - pidiskonjual1 should not be more than 25 character." : GoTo selesai
            End If

            'pidiskonjual2(11) As String
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - pidiskonjual2 can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(11)) > 25 Then
                result(2) = "Row : " & i & " - pidiskonjual2 should not be more than 25 character." : GoTo selesai
            End If

            'pidiskonjual3(12) As String
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - pidiskonjual3 can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(12)) > 25 Then
                result(2) = "Row : " & i & " - pidiskonjual3 should not be more than 25 character." : GoTo selesai
            End If

            'pidiskonjual4(13) As String
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - pidiskonjual4 can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(13)) > 25 Then
                result(2) = "Row : " & i & " - pidiskonjual4 should not be more than 25 character." : GoTo selesai
            End If

            'pidiskonjual5(14) As String
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - pidiskonjual5 can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(14)) > 25 Then
                result(2) = "Row : " & i & " - pidiskonjual5 should not be more than 25 character." : GoTo selesai
            End If

            'picustomdbl1(23) As Double
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - picustomdbl1 can't be empty" : GoTo selesai
            End If

            'picustomdbl2(24) As Double
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - picustomdbl2 can't be empty" : GoTo selesai
            End If

            'picustomdbl3(25) As Double
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - picustomdbl3 can't be empty" : GoTo selesai
            End If

            'picustomdate1(26) As Date
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - picustomdate1 can't be empty" : GoTo selesai
            End If

            'picustomdate2(27) As Date
            If Len(dataRowDetail(27)) = 0 Then
                result(2) = "Row : " & i & " - picustomdate2 can't be empty" : GoTo selesai
            End If

            'picustomdate3(28) As Date
            If Len(dataRowDetail(28)) = 0 Then
                result(2) = "Row : " & i & " - picustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            'If AsDataTableTambahData(dtdetail, "pikategori~piidbarang~pistokminimal~pistokmaksimal~pistokreorder~pihargajual1~pihargajual2~pihargajual3~pihargajual4~pihargajual5~pidiskonjual1~pidiskonjual2~pidiskonjual3~pidiskonjual4~pidiskonjual5~picustomtext1~picustomtext2~picustomtext3~picustomtext4~picustomtext5~picustomint1~picustomint2~picustomint3~picustomdbl1~picustomdbl2~picustomdbl3~picustomdate1~picustomdate2~picustomdate3~piaksi~pistokminorder~pihargaedited", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30)) = False Then
            '    result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            'End If

            If AsDataTableTambahData(dtdetail, "pikategori~piidbarang~pistokminimal~pistokmaksimal~pistokreorder~pihargajual1~pihargajual2~pihargajual3~pihargajual4~pihargajual5~pidiskonjual1~pidiskonjual2~pidiskonjual3~pidiskonjual4~pidiskonjual5~picustomtext1~picustomtext2~picustomtext3~picustomtext4~picustomtext5~picustomint1~picustomint2~picustomint3~picustomdbl1~picustomdbl2~picustomdbl3~picustomdate1~picustomdate2~picustomdate3~piaksi~pistokminorder~pihargaedited", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31)) = False Then
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
            'Proses detail
            If (dtdetail.Rows.Count > 0) Then
                Dim strValue1, strValue2 As New StringBuilder
                Dim dtInsert, dtDelete As New DataTable

                'Proses delete barang : piaksi = 0
                dtDelete = AsDataTableFilterSortDt(dtdetail, "piaksi = 0")
                Dim KategoriPOS As String
                If dtDelete.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtDelete.Rows
                        'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                        ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                        ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(dr1("pikategori")) & "' "

                        strValue1.Append(IIf(Len(strValue1.ToString) = 0, "", " OR "))
                        KategoriPOS = FixQuotes(dr1("pikategori"))

                        'CEK TERKAIT =============================================================
                        Dim paramTerkait As String = M12_Pos_ItemTerkait(PostWsTerkait(paramSplit(0), "M12_Pos_ItemTerkait", pagingSplit(0), pagingSplit(1), "", "", formatTgl, formatTglWaktu, FixQuotes(dr1("pikategori")) & "△" & FixQuotes(dr1("piidbarang"))))
                        Dim hasilTerkait As New RsHasilWsSearch
                        hasilTerkait = GetWsSearch(paramTerkait)
                        If hasilTerkait.success = 1 Then
                            result(2) = "This Item has related transactions."

                            resultPaging(0) = hasilTerkait.isPaging
                            resultPaging(1) = hasilTerkait.isNext
                            resultPaging(2) = hasilTerkait.isPrevious
                            resultPaging(3) = hasilTerkait.countPage
                            resultPaging(4) = hasilTerkait.countRow

                            search = hasilTerkait.data : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF CEK TERKAIT ======================================================

                        strValue1.Append("(pikategori = '" & FixQuotes(dr1("pikategori")) & "' AND piidbarang = '" & FixQuotes(dr1("piidbarang")) & "')")
                    Next
                    'hapus barang
                    If Len(strValue1.ToString) > 0 Then



                        sql = "Delete from M_12_Pos_Item where " & strValue1.ToString & ""
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

                'Proses insert barang : piaksi = 1
                dtInsert = AsDataTableFilterSortDt(dtdetail, "piaksi = 1")
                If dtInsert.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtInsert.Rows
                        'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                        ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                        ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(dr1("pikategori")) & "' "

                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        'strValue2.Append("('" & FixQuotes(dr1("pikategori")) & "', '" & FixQuotes(dr1("piidbarang")) & "', '" & FixDouble(dr1("pistokminimal")) & "', '" & FixDouble(dr1("pistokmaksimal")) & "', '" & FixDouble(dr1("pistokreorder")) & "', '" & FixDouble(dr1("pihargajual1")) & "', '" & FixDouble(dr1("pihargajual2")) & "', '" & FixDouble(dr1("pihargajual3")) & "', '" & FixDouble(dr1("pihargajual4")) & "', '" & FixDouble(dr1("pihargajual5")) & "', '" & FixQuotes(dr1("pidiskonjual1")) & "', '" & FixQuotes(dr1("pidiskonjual2")) & "', '" & FixQuotes(dr1("pidiskonjual3")) & "', '" & FixQuotes(dr1("pidiskonjual4")) & "', '" & FixQuotes(dr1("pidiskonjual5")) & "', '" & FixQuotes(dr1("picustomtext1")) & "', '" & FixQuotes(dr1("picustomtext2")) & "', '" & FixQuotes(dr1("picustomtext3")) & "', '" & FixQuotes(dr1("picustomtext4")) & "', '" & FixQuotes(dr1("picustomtext5")) & "', " & dr1("picustomint1") & ", " & dr1("picustomint2") & ", " & dr1("picustomint3") & ", '" & FixDouble(dr1("picustomdbl1")) & "', '" & FixDouble(dr1("picustomdbl2")) & "', '" & FixDouble(dr1("picustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate3"))) & "', '" & FixDouble(dr1("pistokminorder")) & "')")
                        strValue2.Append("('" & FixQuotes(dr1("pikategori")) & "', '" & FixQuotes(dr1("piidbarang")) & "', '" & FixDouble(dr1("pistokminimal")) & "', '" & FixDouble(dr1("pistokmaksimal")) & "', '" & FixDouble(dr1("pistokreorder")) & "', '" & FixDouble(dr1("pihargajual1")) & "', '" & FixDouble(dr1("pihargajual2")) & "', '" & FixDouble(dr1("pihargajual3")) & "', '" & FixDouble(dr1("pihargajual4")) & "', '" & FixDouble(dr1("pihargajual5")) & "', '" & FixQuotes(dr1("pidiskonjual1")) & "', '" & FixQuotes(dr1("pidiskonjual2")) & "', '" & FixQuotes(dr1("pidiskonjual3")) & "', '" & FixQuotes(dr1("pidiskonjual4")) & "', '" & FixQuotes(dr1("pidiskonjual5")) & "', '" & FixQuotes(dr1("picustomtext1")) & "', '" & FixQuotes(dr1("picustomtext2")) & "', '" & FixQuotes(dr1("picustomtext3")) & "', '" & FixQuotes(dr1("picustomtext4")) & "', '" & FixQuotes(dr1("picustomtext5")) & "', " & dr1("picustomint1") & ", " & dr1("picustomint2") & ", " & dr1("picustomint3") & ", '" & FixDouble(dr1("picustomdbl1")) & "', '" & FixDouble(dr1("picustomdbl2")) & "', '" & FixDouble(dr1("picustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate3"))) & "', '" & FixDouble(dr1("pistokminorder")) & "', '" & FixQuotes(dr1("pihargaedited")) & "')")
                    Next
                    'insert jika data belum ada, dan update jika data sudah ada
                    If Len(strValue2.ToString) > 0 Then
                        'sql = "Insert into M_12_Pos_Item(pikategori, piidbarang, pistokminimal, pistokmaksimal, pistokreorder, pihargajual1, pihargajual2, pihargajual3, pihargajual4, pihargajual5, pidiskonjual1, pidiskonjual2, pidiskonjual3, pidiskonjual4, pidiskonjual5, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3, pistokminorder) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE pistokminimal = VALUES(pistokminimal), pistokmaksimal = VALUES(pistokmaksimal), pistokreorder = VALUES(pistokreorder), pihargajual1 = VALUES(pihargajual1), pihargajual2 = VALUES(pihargajual2), pihargajual3 = VALUES(pihargajual3), pihargajual4 = VALUES(pihargajual4), pihargajual5 = VALUES(pihargajual5), pidiskonjual1 = VALUES(pidiskonjual1), pidiskonjual2 = VALUES(pidiskonjual2), pidiskonjual3 = VALUES(pidiskonjual3), pidiskonjual4 = VALUES(pidiskonjual4), pidiskonjual5 = VALUES(pidiskonjual5), picustomtext1 = VALUES(picustomtext1), picustomtext2 = VALUES(picustomtext2), picustomtext3 = VALUES(picustomtext3), picustomtext4 = VALUES(picustomtext4), picustomtext5 = VALUES(picustomtext5), picustomint1 = VALUES(picustomint1), picustomint2 = VALUES(picustomint2), picustomint3 = VALUES(picustomint3), picustomdbl1 = VALUES(picustomdbl1), picustomdbl2 = VALUES(picustomdbl2), picustomdbl3 = VALUES(picustomdbl3), picustomdate1 = VALUES(picustomdate1), picustomdate2 = VALUES(picustomdate2), picustomdate3 = VALUES(picustomdate3), pistokminorder = VALUES(pistokminorder)"
                        sql = "Insert into M_12_Pos_Item(pikategori, piidbarang, pistokminimal, pistokmaksimal, pistokreorder, pihargajual1, pihargajual2, pihargajual3, pihargajual4, pihargajual5, pidiskonjual1, pidiskonjual2, pidiskonjual3, pidiskonjual4, pidiskonjual5, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3, pistokminorder, pihargaedited) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE pistokminimal = VALUES(pistokminimal), pistokmaksimal = VALUES(pistokmaksimal), pistokreorder = VALUES(pistokreorder), pihargajual1 = VALUES(pihargajual1), pihargajual2 = VALUES(pihargajual2), pihargajual3 = VALUES(pihargajual3), pihargajual4 = VALUES(pihargajual4), pihargajual5 = VALUES(pihargajual5), pidiskonjual1 = VALUES(pidiskonjual1), pidiskonjual2 = VALUES(pidiskonjual2), pidiskonjual3 = VALUES(pidiskonjual3), pidiskonjual4 = VALUES(pidiskonjual4), pidiskonjual5 = VALUES(pidiskonjual5), picustomtext1 = VALUES(picustomtext1), picustomtext2 = VALUES(picustomtext2), picustomtext3 = VALUES(picustomtext3), picustomtext4 = VALUES(picustomtext4), picustomtext5 = VALUES(picustomtext5), picustomint1 = VALUES(picustomint1), picustomint2 = VALUES(picustomint2), picustomint3 = VALUES(picustomint3), picustomdbl1 = VALUES(picustomdbl1), picustomdbl2 = VALUES(picustomdbl2), picustomdbl3 = VALUES(picustomdbl3), picustomdate1 = VALUES(picustomdate1), picustomdate2 = VALUES(picustomdate2), picustomdate3 = VALUES(picustomdate3), pistokminorder = VALUES(pistokminorder), pihargaedited = VALUES(pihargaedited)"
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

            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_ItemDelete(ByVal param As String) As String

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
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""
        Dim Filter As String = "", Sorting As String = ""

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
        If ClsValidKey.ApaBisaAkses(1, 1, 2) = False Then
            result(2) = "Access denied for delete data"
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
        Dim pikategori As String = "", piidbarang As String = ""
        Dim idtrans(3) As String
        idtrans = paramSplit(5).Split(sptSubParam)
        If (idtrans.Length <> 2) Then
            result(2) = "Invalid primary key parameter." : GoTo selesai
        Else
            'CEK pikategori
            If (Len(idtrans(0)) = 0) Then
                result(2) = "pikategori can't be empty." : GoTo selesai
            Else
                pikategori = idtrans(0)
            End If
            'CEK piidbarang
            If (IsNumeric(idtrans(1)) = False) Then
                result(2) = "piidbarang required numeric." : GoTo selesai
            Else
                piidbarang = idtrans(1)
            End If
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'DELETE DI DATABASE ================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'AMBIL KATEGORI POS TRANSAKSI UNTUK FILTER USER LOGIN
            sql = "SELECT pikategori as kategoripos FROM M_12_Pos_Item WHERE pikategori = '" & pikategori & "' AND piidbarang = '" & piidbarang & "' GROUP BY pikategori"
            Dim dtKategoriPOS As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtKategoriPOS.Rows.Count > 0 Then
                For Each drKategoriPOS As DataRow In dtKategoriPOS.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drKategoriPOS("kategoripos")) & "' "
                Next
            End If

            'DELETE
            sql = "DELETE FROM M_12_Pos_Item WHERE pikategori = '" & pikategori & "' AND piidbarang = '" & piidbarang & "'"
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
            Dim paramSearch As String = M12_Pos_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_ItemImport(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean

        Dim pg1 As New RsPaging
        Dim search As String = ""
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
        'pikategori(0) As String, piidbarang(1) As Integer, pistokminimal(2) As Double, pistokmaksimal(3) As Double, pistokreorder(4) As Double, 
        'pihargajual1(5) As Double, pihargajual2(6) As Double, pihargajual3(7) As Double, pihargajual4(8) As Double, pihargajual5(9) As Double, 
        'pidiskonjual1(10) As String, pidiskonjual2(11) As String, pidiskonjual3(12) As String, pidiskonjual4(13) As String, pidiskonjual5(14) As String, 
        'picustomtext1(15) As String, picustomtext2(16) As String, picustomtext3(17) As String, picustomtext4(18) As String, picustomtext5(19) As String, 
        'picustomint1(20) As Integer, picustomint2(21) As Integer, picustomint3(22) As Integer, picustomdbl1(23) As Double, picustomdbl2(24) As Double, 
        'picustomdbl3(25) As Double, picustomdate1(26) As Date, picustomdate2(27) As Date, picustomdate3(28) As Date, piaksi(29) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'pikategori, piidbarang, pistokminimal, pistokmaksimal, pistokreorder, pihargajual1, pihargajual2, 
        'pihargajual3, pihargajual4, pihargajual5, pidiskonjual1, pidiskonjual2, pidiskonjual3, pidiskonjual4, 
        'pidiskonjual5, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, 
        'picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, 
        'picustomdate3, piaksi

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "piidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pistokminimal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pistokmaksimal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pistokreorder", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pihargajual1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pihargajual2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pihargajual3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pihargajual4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pihargajual5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pidiskonjual1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pidiskonjual2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pidiskonjual3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pidiskonjual4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pidiskonjual5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "picustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "picustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "picustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "piaksi", AsEnumTypeData.AsInt64)

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


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 30) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'piidbarang(1) As Double
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - piidbarang required numeric." : GoTo selesai
            End If
            'pistokminimal(2) As Double
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - pistokminimal required numeric." : GoTo selesai
            End If
            'pistokmaksimal(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - pistokmaksimal required numeric." : GoTo selesai
            End If
            'pistokreorder(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - pistokreorder required numeric." : GoTo selesai
            End If
            'pihargajual1(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - pihargajual1 required numeric." : GoTo selesai
            End If
            'pihargajual2(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - pihargajual2 required numeric." : GoTo selesai
            End If
            'pihargajual3(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - pihargajual3 required numeric." : GoTo selesai
            End If
            'pihargajual4(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - pihargajual4 required numeric." : GoTo selesai
            End If
            'pihargajual5(9) As Double
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - pihargajual5 required numeric." : GoTo selesai
            End If
            'picustomint1(20) As Integer
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - picustomint1 required numeric." : GoTo selesai
            End If
            'picustomint2(21) As Integer
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - picustomint2 required numeric." : GoTo selesai
            End If
            'picustomint3(22) As Integer
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - picustomint3 required numeric." : GoTo selesai
            End If
            'picustomdbl1(23) As Double
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - picustomdbl1 required numeric." : GoTo selesai
            End If
            'picustomdbl2(24) As Double
            If (IsNumeric(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - picustomdbl2 required numeric." : GoTo selesai
            End If
            'picustomdbl3(25) As Double
            If (IsNumeric(dataRowDetail(25)) = False) Then
                result(2) = "Row : " & i & " - picustomdbl3 required numeric." : GoTo selesai
            End If
            'picustomdate1(26) As Date
            If (IsDate(dataRowDetail(26)) = False) Then
                result(2) = "Row : " & i & " - picustomdate1 required date." : GoTo selesai
            End If
            'picustomdate2(27) As Date
            If (IsDate(dataRowDetail(27)) = False) Then
                result(2) = "Row : " & i & " - picustomdate2 required date." : GoTo selesai
            End If
            'picustomdate3(28) As Date
            If (IsDate(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - picustomdate3 required date." : GoTo selesai
            End If
            'piaksi(29) As Integer
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - piaksi required numeric." : GoTo selesai
            ElseIf (dataRowDetail(29) <> 0 And dataRowDetail(29) <> 1) Then
                result(2) = "Row : " & i & " - invalid piaksi value. (0:delete, 1:insert/update)" : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If

            'piidbarang(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - piidbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - piidbarang should not be more than 20 character." : GoTo selesai
            End If

            'pistokminimal(2) As Double
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - pistokminimal can't be empty" : GoTo selesai
            End If

            'pistokmaksimal(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - pistokmaksimal can't be empty" : GoTo selesai
            End If

            'pistokreorder(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - pistokreorder can't be empty" : GoTo selesai
            End If

            'pihargajual1(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - pihargajual1 can't be empty" : GoTo selesai
            End If

            'pihargajual2(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - pihargajual2 can't be empty" : GoTo selesai
            End If

            'pihargajual3(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - pihargajual3 can't be empty" : GoTo selesai
            End If

            'pihargajual4(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - pihargajual4 can't be empty" : GoTo selesai
            End If

            'pihargajual5(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - pihargajual5 can't be empty" : GoTo selesai
            End If

            'pidiskonjual1(10) As String
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - pidiskonjual1 can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(10)) > 25 Then
                result(2) = "Row : " & i & " - pidiskonjual1 should not be more than 25 character." : GoTo selesai
            End If

            'pidiskonjual2(11) As String
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - pidiskonjual2 can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(11)) > 25 Then
                result(2) = "Row : " & i & " - pidiskonjual2 should not be more than 25 character." : GoTo selesai
            End If

            'pidiskonjual3(12) As String
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - pidiskonjual3 can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(12)) > 25 Then
                result(2) = "Row : " & i & " - pidiskonjual3 should not be more than 25 character." : GoTo selesai
            End If

            'pidiskonjual4(13) As String
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - pidiskonjual4 can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(13)) > 25 Then
                result(2) = "Row : " & i & " - pidiskonjual4 should not be more than 25 character." : GoTo selesai
            End If

            'pidiskonjual5(14) As String
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - pidiskonjual5 can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(14)) > 25 Then
                result(2) = "Row : " & i & " - pidiskonjual5 should not be more than 25 character." : GoTo selesai
            End If

            'picustomdbl1(23) As Double
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - picustomdbl1 can't be empty" : GoTo selesai
            End If

            'picustomdbl2(24) As Double
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - picustomdbl2 can't be empty" : GoTo selesai
            End If

            'picustomdbl3(25) As Double
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - picustomdbl3 can't be empty" : GoTo selesai
            End If

            'picustomdate1(26) As Date
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - picustomdate1 can't be empty" : GoTo selesai
            End If

            'picustomdate2(27) As Date
            If Len(dataRowDetail(27)) = 0 Then
                result(2) = "Row : " & i & " - picustomdate2 can't be empty" : GoTo selesai
            End If

            'picustomdate3(28) As Date
            If Len(dataRowDetail(28)) = 0 Then
                result(2) = "Row : " & i & " - picustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pikategori~piidbarang~pistokminimal~pistokmaksimal~pistokreorder~pihargajual1~pihargajual2~pihargajual3~pihargajual4~pihargajual5~pidiskonjual1~pidiskonjual2~pidiskonjual3~pidiskonjual4~pidiskonjual5~picustomtext1~picustomtext2~picustomtext3~picustomtext4~picustomtext5~picustomint1~picustomint2~picustomint3~picustomdbl1~picustomdbl2~picustomdbl3~picustomdate1~picustomdate2~picustomdate3~piaksi", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29)) = False Then
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
            'Proses detail
            If (dtdetail.Rows.Count > 0) Then
                Dim strValue2 As New StringBuilder
                For Each dr1 As DataRow In dtdetail.Rows
                    strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Append("('" & FixQuotes(dr1("pikategori")) & "', '" & FixQuotes(dr1("piidbarang")) & "', '" & FixDouble(dr1("pistokminimal")) & "', '" & FixDouble(dr1("pistokmaksimal")) & "', '" & FixDouble(dr1("pistokreorder")) & "', '" & FixDouble(dr1("pihargajual1")) & "', '" & FixDouble(dr1("pihargajual2")) & "', '" & FixDouble(dr1("pihargajual3")) & "', '" & FixDouble(dr1("pihargajual4")) & "', '" & FixDouble(dr1("pihargajual5")) & "', '" & FixQuotes(dr1("pidiskonjual1")) & "', '" & FixQuotes(dr1("pidiskonjual2")) & "', '" & FixQuotes(dr1("pidiskonjual3")) & "', '" & FixQuotes(dr1("pidiskonjual4")) & "', '" & FixQuotes(dr1("pidiskonjual5")) & "', '" & FixQuotes(dr1("picustomtext1")) & "', '" & FixQuotes(dr1("picustomtext2")) & "', '" & FixQuotes(dr1("picustomtext3")) & "', '" & FixQuotes(dr1("picustomtext4")) & "', '" & FixQuotes(dr1("picustomtext5")) & "', " & dr1("picustomint1") & ", " & dr1("picustomint2") & ", " & dr1("picustomint3") & ", '" & FixDouble(dr1("picustomdbl1")) & "', '" & FixDouble(dr1("picustomdbl2")) & "', '" & FixDouble(dr1("picustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate3"))) & "')")
                Next

                If Len(strValue2.ToString) > 0 Then
                    'DELETE
                    sql = "DELETE FROM M_12_Pos_Item"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'insert jika data belum ada, dan update jika data sudah ada
                    sql = "Insert into M_12_Pos_Item(pikategori, piidbarang, pistokminimal, pistokmaksimal, pistokreorder, pihargajual1, pihargajual2, pihargajual3, pihargajual4, pihargajual5, pidiskonjual1, pidiskonjual2, pidiskonjual3, pidiskonjual4, pidiskonjual5, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE pistokminimal = VALUES(pistokminimal), pistokmaksimal = VALUES(pistokmaksimal), pistokreorder = VALUES(pistokreorder), pihargajual1 = VALUES(pihargajual1), pihargajual2 = VALUES(pihargajual2), pihargajual3 = VALUES(pihargajual3), pihargajual4 = VALUES(pihargajual4), pihargajual5 = VALUES(pihargajual5), pidiskonjual1 = VALUES(pidiskonjual1), pidiskonjual2 = VALUES(pidiskonjual2), pidiskonjual3 = VALUES(pidiskonjual3), pidiskonjual4 = VALUES(pidiskonjual4), pidiskonjual5 = VALUES(pidiskonjual5), picustomtext1 = VALUES(picustomtext1), picustomtext2 = VALUES(picustomtext2), picustomtext3 = VALUES(picustomtext3), picustomtext4 = VALUES(picustomtext4), picustomtext5 = VALUES(picustomtext5), picustomint1 = VALUES(picustomint1), picustomint2 = VALUES(picustomint2), picustomint3 = VALUES(picustomint3), picustomdbl1 = VALUES(picustomdbl1), picustomdbl2 = VALUES(picustomdbl2), picustomdbl3 = VALUES(picustomdbl3), picustomdate1 = VALUES(picustomdate1), picustomdate2 = VALUES(picustomdate2), picustomdate3 = VALUES(picustomdate3)"
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
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_ItemSimpanPerKategori(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
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
        'pikategori(0) As String, pikategoribarang(1) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'pikategori, pikategoribarang

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pikategoribarang", AsEnumTypeData.AsString)


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


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 2) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------


            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If

            'pikategoribarang(1) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - pikategoribarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - pikategoribarang should not be more than 25 character." : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pikategori~pikategoribarang", dataRowDetail(0) & "~" & dataRowDetail(1)) = False Then
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
            'Proses detail
            If (dtdetail.Rows.Count > 0) Then
                For Each dr1 As DataRow In dtdetail.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(dr1("pikategori")) & "' "

                    'sql = "Insert into M_12_Pos_Item(SELECT '" & FixQuotes(dr1("pikategori")) & "' as pikategori, bid as piidbarang, bstokminimal as pistokminimal, bstokmaksimal as pistokmaksimal, breorder as pistokreorder, bminorder as pistokminorder, bhargajual1 as pihargajual1, bhargajual2 as pihargajual2, bhargajual3 as pihargajual3, bhargajual4 as pihargajual4, bhargajual5 as pihargajual5, bdiskonjual1 as pidiskonjual1, bdiskonjual2 as pidiskonjual2, bdiskonjual3 as pidiskonjual3, bdiskonjual4 as pidiskonjual4, bdiskonjual5 as pidiskonjual5, '' as picustomtext1, '' as picustomtext2, '' as picustomtext3, '' as picustomtext4, '' as picustomtext5, 0 as picustomint1, 0 as picustomint2, 0 as picustomint3, 0 as picustomdbl1, 0 as picustomdbl2, 0 as picustomdbl3, '1900-01-01' as picustomdate1, '1900-01-01' as picustomdate2, '1900-01-01' as picustomdate3, 0 as pidownloaded FROM m1_item WHERE bkategori = '" & FixQuotes(dr1("pikategoribarang")) & "') ON DUPLICATE KEY UPDATE pistokminimal = VALUES(pistokminimal), pistokmaksimal = VALUES(pistokmaksimal), pistokreorder = VALUES(pistokreorder), pistokminorder = VALUES(pistokminorder), pihargajual1 = VALUES(pihargajual1), pihargajual2 = VALUES(pihargajual2), pihargajual3 = VALUES(pihargajual3), pihargajual4 = VALUES(pihargajual4), pihargajual5 = VALUES(pihargajual5), pidiskonjual1 = VALUES(pidiskonjual1), pidiskonjual2 = VALUES(pidiskonjual2), pidiskonjual3 = VALUES(pidiskonjual3), pidiskonjual4 = VALUES(pidiskonjual4), pidiskonjual5 = VALUES(pidiskonjual5), picustomtext1 = VALUES(picustomtext1), picustomtext2 = VALUES(picustomtext2), picustomtext3 = VALUES(picustomtext3), picustomtext4 = VALUES(picustomtext4), picustomtext5 = VALUES(picustomtext5), picustomint1 = VALUES(picustomint1), picustomint2 = VALUES(picustomint2), picustomint3 = VALUES(picustomint3), picustomdbl1 = VALUES(picustomdbl1), picustomdbl2 = VALUES(picustomdbl2), picustomdbl3 = VALUES(picustomdbl3), picustomdate1 = VALUES(picustomdate1), picustomdate2 = VALUES(picustomdate2), picustomdate3 = VALUES(picustomdate3), pidownloaded = VALUES(pidownloaded)"
                    sql = "Insert into M_12_Pos_Item(SELECT '" & FixQuotes(dr1("pikategori")) & "' as pikategori, bid as piidbarang, bstokminimal as pistokminimal, bstokmaksimal as pistokmaksimal, breorder as pistokreorder, bminorder as pistokminorder, bhargajual1 as pihargajual1, bhargajual2 as pihargajual2, bhargajual3 as pihargajual3, bhargajual4 as pihargajual4, bhargajual5 as pihargajual5, bdiskonjual1 as pidiskonjual1, bdiskonjual2 as pidiskonjual2, bdiskonjual3 as pidiskonjual3, bdiskonjual4 as pidiskonjual4, bdiskonjual5 as pidiskonjual5, '' as picustomtext1, '' as picustomtext2, '' as picustomtext3, '' as picustomtext4, '' as picustomtext5, 0 as picustomint1, 0 as picustomint2, 0 as picustomint3, 0 as picustomdbl1, 0 as picustomdbl2, 0 as picustomdbl3, '1900-01-01' as picustomdate1, '1900-01-01' as picustomdate2, '1900-01-01' as picustomdate3, 0 as pidownloaded, 0 as pihargaedited FROM m1_item WHERE bkategori = '" & FixQuotes(dr1("pikategoribarang")) & "') ON DUPLICATE KEY UPDATE picustomtext1 = VALUES(picustomtext1), picustomtext2 = VALUES(picustomtext2), picustomtext3 = VALUES(picustomtext3), picustomtext4 = VALUES(picustomtext4), picustomtext5 = VALUES(picustomtext5), picustomint1 = VALUES(picustomint1), picustomint2 = VALUES(picustomint2), picustomint3 = VALUES(picustomint3), picustomdbl1 = VALUES(picustomdbl1), picustomdbl2 = VALUES(picustomdbl2), picustomdbl3 = VALUES(picustomdbl3), picustomdate1 = VALUES(picustomdate1), picustomdate2 = VALUES(picustomdate2), picustomdate3 = VALUES(picustomdate3)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Next

            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_ItemSimpanKelasProduk(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
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
        'pikategori(0) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'pikategori

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)


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


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 1) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------


            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pikategori", dataRowDetail(0)) = False Then
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
        Dim KategoriPOS As String = ""
        Try
            'Proses detail


            If (dtdetail.Rows.Count > 0) Then
                For Each dr1 As DataRow In dtdetail.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(dr1("pikategori")) & "' "
                    'select '
                    'SELECT `pi`.`piidbarang` AS idbarang,  `pp`.`kelasproduk`,  `i`.`bid`, `i`.bkelasproduk,  `i`.`bnama`,  `pi`.`pikategori`,  pc.pctipepos  FROM m_12_pos_item `pi` JOIN m1_item `i` ON pi.piidbarang = i.bid JOIN m_12_pos_category `pc` ON pi.pikategori = pc.pckode JOIN m_12_pos_type `pt` ON pc.pctipepos = pt.ptkode JOIN m_12_pos_type_class_product `pp` ON pp.tipepos = pc.pctipepos  WHERE pi.pikategori = "valkode" AND i.bkelasproduk = (SELECT pph.kelasproduk FROM m_12_pos_category `pch`  JOIN m_12_pos_type `pth` ON pch.pctipepos = pth.ptkode JOIN m_12_pos_type_class_product `pph` ON pph.tipepos = pch.pctipepos LIMIT 1 ) GROUP BY pi.piidbarang , pp.kelasproduk
                    KategoriPOS = FixQuotes(dr1("pikategori"))


                    'sql = "Insert into M_12_Pos_Item(SELECT '" & FixQuotes(dr1("pikategori")) & "' as pikategori, bid as piidbarang, bstokminimal as pistokminimal, bstokmaksimal as pistokmaksimal, breorder as pistokreorder, bminorder as pistokminorder, bhargajual1 as pihargajual1, bhargajual2 as pihargajual2, bhargajual3 as pihargajual3, bhargajual4 as pihargajual4, bhargajual5 as pihargajual5, bdiskonjual1 as pidiskonjual1, bdiskonjual2 as pidiskonjual2, bdiskonjual3 as pidiskonjual3, bdiskonjual4 as pidiskonjual4, bdiskonjual5 as pidiskonjual5, '' as picustomtext1, '' as picustomtext2, '' as picustomtext3, '' as picustomtext4, '' as picustomtext5, 0 as picustomint1, 0 as picustomint2, 0 as picustomint3, 0 as picustomdbl1, 0 as picustomdbl2, 0 as picustomdbl3, '1900-01-01' as picustomdate1, '1900-01-01' as picustomdate2, '1900-01-01' as picustomdate3, 0 as pidownloaded FROM m1_item i JOIN m_12_pos_category pc ON pc.pckode = '" & FixQuotes(dr1("pikategori")) & "' JOIN m_12_pos_type pt ON pc.pctipepos = pt.ptkode JOIN m_12_pos_type_class_product ptcp ON pt.ptkode = ptcp.tipepos AND i.bkelasproduk = ptcp.kelasproduk GROUP BY i.bid) ON DUPLICATE KEY UPDATE pistokminimal = VALUES(pistokminimal), pistokmaksimal = VALUES(pistokmaksimal), pistokreorder = VALUES(pistokreorder), pistokminorder = VALUES(pistokminorder), pihargajual1 = VALUES(pihargajual1), pihargajual2 = VALUES(pihargajual2), pihargajual3 = VALUES(pihargajual3), pihargajual4 = VALUES(pihargajual4), pihargajual5 = VALUES(pihargajual5), pidiskonjual1 = VALUES(pidiskonjual1), pidiskonjual2 = VALUES(pidiskonjual2), pidiskonjual3 = VALUES(pidiskonjual3), pidiskonjual4 = VALUES(pidiskonjual4), pidiskonjual5 = VALUES(pidiskonjual5), picustomtext1 = VALUES(picustomtext1), picustomtext2 = VALUES(picustomtext2), picustomtext3 = VALUES(picustomtext3), picustomtext4 = VALUES(picustomtext4), picustomtext5 = VALUES(picustomtext5), picustomint1 = VALUES(picustomint1), picustomint2 = VALUES(picustomint2), picustomint3 = VALUES(picustomint3), picustomdbl1 = VALUES(picustomdbl1), picustomdbl2 = VALUES(picustomdbl2), picustomdbl3 = VALUES(picustomdbl3), picustomdate1 = VALUES(picustomdate1), picustomdate2 = VALUES(picustomdate2), picustomdate3 = VALUES(picustomdate3), pidownloaded = VALUES(pidownloaded)"
                    sql = "Insert into M_12_Pos_Item(SELECT '" & FixQuotes(dr1("pikategori")) & "' as pikategori, bid as piidbarang, bstokminimal as pistokminimal, bstokmaksimal as pistokmaksimal, breorder as pistokreorder, bminorder as pistokminorder, bhargajual1 as pihargajual1, bhargajual2 as pihargajual2, bhargajual3 as pihargajual3, bhargajual4 as pihargajual4, bhargajual5 as pihargajual5, bdiskonjual1 as pidiskonjual1, bdiskonjual2 as pidiskonjual2, bdiskonjual3 as pidiskonjual3, bdiskonjual4 as pidiskonjual4, bdiskonjual5 as pidiskonjual5, '' as picustomtext1, '' as picustomtext2, '' as picustomtext3, '' as picustomtext4, '' as picustomtext5, 0 as picustomint1, 0 as picustomint2, 0 as picustomint3, 0 as picustomdbl1, 0 as picustomdbl2, 0 as picustomdbl3, '1900-01-01' as picustomdate1, '1900-01-01' as picustomdate2, '1900-01-01' as picustomdate3, 0 as pidownloaded, 0 as pihargaedited FROM m1_item i JOIN m_12_pos_category pc ON pc.pckode = '" & FixQuotes(dr1("pikategori")) & "' JOIN m_12_pos_type pt ON pc.pctipepos = pt.ptkode JOIN m_12_pos_type_class_product ptcp ON pt.ptkode = ptcp.tipepos AND i.bkelasproduk = ptcp.kelasproduk GROUP BY i.bid) ON DUPLICATE KEY UPDATE pidownloaded = VALUES(pidownloaded), pihargaedited = VALUES(pihargaedited)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Next

                'HAPUS DATA BARANG POS YANG KELAS PRODUKNYA TIDAK SESUAI DENGAN KELAS PRODUK KATEGORI POS YANG DIPILIH ==========================
                Dim filterkp As String = ""
                Dim dtkp As New DataTable
                dtkp = AsDataTableAmbilDariDBCon("SELECT pph.kelasproduk as kelasproduk FROM m_12_pos_category `pch` JOIN m_12_pos_type `pth` ON pch.pctipepos = pth.ptkode JOIN m_12_pos_type_class_product `pph` ON pph.tipepos = pch.pctipepos WHERE pch.pckode = '" & KategoriPOS & "'", myConn)
                If dtkp.Rows.Count > 0 Then
                    For Each drkp As DataRow In dtkp.Rows
                        filterkp = IIf(filterkp.Length > 0, filterkp & " AND ", "")
                        filterkp &= " i.bkelasproduk <> '" & FixQuotes(drkp("kelasproduk")) & "' "
                    Next
                End If

                Dim query As String = "SELECT `pi`.`piidbarang` AS idbarang FROM m_12_pos_item `pi` JOIN m1_item `i` ON pi.piidbarang = i.bid JOIN m_12_pos_category `pc` ON pi.pikategori = pc.pckode JOIN m_12_pos_type `pt` ON pc.pctipepos = pt.ptkode JOIN m_12_pos_type_class_product `pp` ON pp.tipepos = pc.pctipepos  WHERE pi.pikategori = '" & KategoriPOS & "' AND (" & filterkp & ") GROUP BY pi.piidbarang , pp.kelasproduk"
                Dim dt2 As New DataTable
                dt2 = AsDataTableAmbilDariDBCon(query, myConn)
                If dt2.Rows.Count > 0 Then

                    For Each dr2 As DataRow In dt2.Rows
                        sql = "DELETE FROM m_12_pos_item WHERE pikategori = '" & KategoriPOS & "' AND piidbarang = '" & FixQuotes(dr2("idbarang")) & "'"

                        'result(2) = sql : Trans.Rollback() : GoTo selesai
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Next

                End If
                'END OF HAPUS DATA BARANG POS YANG KELAS PRODUKNYA TIDAK SESUAI DENGAN KELAS PRODUK KATEGORI POS YANG DIPILIH ==========================


            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)
            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_ItemSimpanSemua(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
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
        'pikategori(0) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'pikategori

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)


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


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 1) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------


            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pikategori", dataRowDetail(0)) = False Then
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
            'Proses detail
            If (dtdetail.Rows.Count > 0) Then
                For Each dr1 As DataRow In dtdetail.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(dr1("pikategori")) & "' "

                    'sql = "Insert into M_12_Pos_Item(SELECT '" & FixQuotes(dr1("pikategori")) & "' as pikategori, bid as piidbarang, bstokminimal as pistokminimal, bstokmaksimal as pistokmaksimal, breorder as pistokreorder, bminorder as pistokminorder, bhargajual1 as pihargajual1, bhargajual2 as pihargajual2, bhargajual3 as pihargajual3, bhargajual4 as pihargajual4, bhargajual5 as pihargajual5, bdiskonjual1 as pidiskonjual1, bdiskonjual2 as pidiskonjual2, bdiskonjual3 as pidiskonjual3, bdiskonjual4 as pidiskonjual4, bdiskonjual5 as pidiskonjual5, '' as picustomtext1, '' as picustomtext2, '' as picustomtext3, '' as picustomtext4, '' as picustomtext5, 0 as picustomint1, 0 as picustomint2, 0 as picustomint3, 0 as picustomdbl1, 0 as picustomdbl2, 0 as picustomdbl3, '1900-01-01' as picustomdate1, '1900-01-01' as picustomdate2, '1900-01-01' as picustomdate3, 0 as pidownloaded, 0 as pidownloaded FROM m1_item) ON DUPLICATE KEY UPDATE pistokminimal = VALUES(pistokminimal), pistokmaksimal = VALUES(pistokmaksimal), pistokreorder = VALUES(pistokreorder), pistokminorder = VALUES(pistokminorder), pihargajual1 = VALUES(pihargajual1), pihargajual2 = VALUES(pihargajual2), pihargajual3 = VALUES(pihargajual3), pihargajual4 = VALUES(pihargajual4), pihargajual5 = VALUES(pihargajual5), pidiskonjual1 = VALUES(pidiskonjual1), pidiskonjual2 = VALUES(pidiskonjual2), pidiskonjual3 = VALUES(pidiskonjual3), pidiskonjual4 = VALUES(pidiskonjual4), pidiskonjual5 = VALUES(pidiskonjual5), picustomtext1 = VALUES(picustomtext1), picustomtext2 = VALUES(picustomtext2), picustomtext3 = VALUES(picustomtext3), picustomtext4 = VALUES(picustomtext4), picustomtext5 = VALUES(picustomtext5), picustomint1 = VALUES(picustomint1), picustomint2 = VALUES(picustomint2), picustomint3 = VALUES(picustomint3), picustomdbl1 = VALUES(picustomdbl1), picustomdbl2 = VALUES(picustomdbl2), picustomdbl3 = VALUES(picustomdbl3), picustomdate1 = VALUES(picustomdate1), picustomdate2 = VALUES(picustomdate2), picustomdate3 = VALUES(picustomdate3), pidownloaded = VALUES(pidownloaded)"
                    sql = "Insert into M_12_Pos_Item(SELECT '" & FixQuotes(dr1("pikategori")) & "' as pikategori, bid as piidbarang, bstokminimal as pistokminimal, bstokmaksimal as pistokmaksimal, breorder as pistokreorder, bminorder as pistokminorder, bhargajual1 as pihargajual1, bhargajual2 as pihargajual2, bhargajual3 as pihargajual3, bhargajual4 as pihargajual4, bhargajual5 as pihargajual5, bdiskonjual1 as pidiskonjual1, bdiskonjual2 as pidiskonjual2, bdiskonjual3 as pidiskonjual3, bdiskonjual4 as pidiskonjual4, bdiskonjual5 as pidiskonjual5, '' as picustomtext1, '' as picustomtext2, '' as picustomtext3, '' as picustomtext4, '' as picustomtext5, 0 as picustomint1, 0 as picustomint2, 0 as picustomint3, 0 as picustomdbl1, 0 as picustomdbl2, 0 as picustomdbl3, '1900-01-01' as picustomdate1, '1900-01-01' as picustomdate2, '1900-01-01' as picustomdate3, 0 as pidownloaded, 0 as pihargaedited FROM m1_item) ON DUPLICATE KEY UPDATE picustomtext1 = VALUES(picustomtext1), picustomtext2 = VALUES(picustomtext2), picustomtext3 = VALUES(picustomtext3), picustomtext4 = VALUES(picustomtext4), picustomtext5 = VALUES(picustomtext5), picustomint1 = VALUES(picustomint1), picustomint2 = VALUES(picustomint2), picustomint3 = VALUES(picustomint3), picustomdbl1 = VALUES(picustomdbl1), picustomdbl2 = VALUES(picustomdbl2), picustomdbl3 = VALUES(picustomdbl3), picustomdate1 = VALUES(picustomdate1), picustomdate2 = VALUES(picustomdate2), picustomdate3 = VALUES(picustomdate3)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Next

            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_ItemSimpanLokasiLain(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
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
        'pikategori(0) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'pikategori

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pikategorilain", AsEnumTypeData.AsString)


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


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 2) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------


            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If

            'pikategorilain(0) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - pikategorilain can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategorilain should not be more than 25 character." : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pikategori~pikategorilain", dataRowDetail(0) & "~" & dataRowDetail(1)) = False Then
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
        Dim KategoriPOS As String = ""
        Try
            'Proses detail


            If (dtdetail.Rows.Count > 0) Then
                For Each dr1 As DataRow In dtdetail.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(dr1("pikategori")) & "' "


                    'sql = "Insert into M_12_Pos_Item(SELECT '" & FixQuotes(dr1("pikategori")) & "' as pikategori, bid as piidbarang, bstokminimal as pistokminimal, bstokmaksimal as pistokmaksimal, breorder as pistokreorder, bminorder as pistokminorder, bhargajual1 as pihargajual1, bhargajual2 as pihargajual2, bhargajual3 as pihargajual3, bhargajual4 as pihargajual4, bhargajual5 as pihargajual5, bdiskonjual1 as pidiskonjual1, bdiskonjual2 as pidiskonjual2, bdiskonjual3 as pidiskonjual3, bdiskonjual4 as pidiskonjual4, bdiskonjual5 as pidiskonjual5, '' as picustomtext1, '' as picustomtext2, '' as picustomtext3, '' as picustomtext4, '' as picustomtext5, 0 as picustomint1, 0 as picustomint2, 0 as picustomint3, 0 as picustomdbl1, 0 as picustomdbl2, 0 as picustomdbl3, '1900-01-01' as picustomdate1, '1900-01-01' as picustomdate2, '1900-01-01' as picustomdate3, 0 as pidownloaded FROM m1_item i JOIN m_12_pos_category pc ON pc.pckode = '" & FixQuotes(dr1("pikategori")) & "' JOIN m_12_pos_type pt ON pc.pctipepos = pt.ptkode JOIN m_12_pos_type_class_product ptcp ON pt.ptkode = ptcp.tipepos AND i.bkelasproduk = ptcp.kelasproduk GROUP BY i.bid) ON DUPLICATE KEY UPDATE pistokminimal = VALUES(pistokminimal), pistokmaksimal = VALUES(pistokmaksimal), pistokreorder = VALUES(pistokreorder), pistokminorder = VALUES(pistokminorder), pihargajual1 = VALUES(pihargajual1), pihargajual2 = VALUES(pihargajual2), pihargajual3 = VALUES(pihargajual3), pihargajual4 = VALUES(pihargajual4), pihargajual5 = VALUES(pihargajual5), pidiskonjual1 = VALUES(pidiskonjual1), pidiskonjual2 = VALUES(pidiskonjual2), pidiskonjual3 = VALUES(pidiskonjual3), pidiskonjual4 = VALUES(pidiskonjual4), pidiskonjual5 = VALUES(pidiskonjual5), picustomtext1 = VALUES(picustomtext1), picustomtext2 = VALUES(picustomtext2), picustomtext3 = VALUES(picustomtext3), picustomtext4 = VALUES(picustomtext4), picustomtext5 = VALUES(picustomtext5), picustomint1 = VALUES(picustomint1), picustomint2 = VALUES(picustomint2), picustomint3 = VALUES(picustomint3), picustomdbl1 = VALUES(picustomdbl1), picustomdbl2 = VALUES(picustomdbl2), picustomdbl3 = VALUES(picustomdbl3), picustomdate1 = VALUES(picustomdate1), picustomdate2 = VALUES(picustomdate2), picustomdate3 = VALUES(picustomdate3), pidownloaded = VALUES(pidownloaded)"
                    sql = "Insert into M_12_Pos_Item(SELECT '" & FixQuotes(dr1("pikategori")) & "' as pikategori, piidbarang as piidbarang, pistokminimal as pistokminimal, pistokmaksimal as pistokmaksimal, pistokreorder as pistokreorder, pistokminorder as pistokminorder, pihargajual1 as pihargajual1, pihargajual2 as pihargajual2, pihargajual3 as pihargajual3, pihargajual4 as pihargajual4, pihargajual5 as pihargajual5, pidiskonjual1 as pidiskonjual1, pidiskonjual2 as pidiskonjual2, pidiskonjual3 as pidiskonjual3, pidiskonjual4 as pidiskonjual4, pidiskonjual5 as pidiskonjual5, picustomtext1 as picustomtext1, picustomtext2 as picustomtext2, picustomtext3 as picustomtext3, picustomtext4 as picustomtext4, picustomtext5 as picustomtext5, picustomint1 as picustomint1, picustomint2 as picustomint2, picustomint3 as picustomint3, picustomdbl1 as picustomdbl1, picustomdbl2 as picustomdbl2, picustomdbl3 as picustomdbl3, picustomdate1 as picustomdate1, picustomdate2 as picustomdate2, picustomdate3 as picustomdate3, pidownloaded as pidownloaded, pihargaedited as pihargaedited FROM m_12_pos_item where pikategori = '" & FixQuotes(dr1("pikategorilain")) & "')  ON DUPLICATE KEY UPDATE pistokminimal = VALUES(pistokminimal), pistokmaksimal = VALUES(pistokmaksimal), pistokreorder = VALUES(pistokreorder), pistokminorder = VALUES(pistokminorder), pihargajual1 = VALUES(pihargajual1), pihargajual2 = VALUES(pihargajual2), pihargajual3 = VALUES(pihargajual3), pihargajual4 = VALUES(pihargajual4), pihargajual5 = VALUES(pihargajual5), pidiskonjual1 = VALUES(pidiskonjual1), pidiskonjual2 = VALUES(pidiskonjual2), pidiskonjual3 = VALUES(pidiskonjual3), pidiskonjual4 = VALUES(pidiskonjual4), pidiskonjual5 = VALUES(pidiskonjual5), picustomtext1 = VALUES(picustomtext1), picustomtext2 = VALUES(picustomtext2), picustomtext3 = VALUES(picustomtext3), picustomtext4 = VALUES(picustomtext4), picustomtext5 = VALUES(picustomtext5), picustomint1 = VALUES(picustomint1), picustomint2 = VALUES(picustomint2), picustomint3 = VALUES(picustomint3), picustomdbl1 = VALUES(picustomdbl1), picustomdbl2 = VALUES(picustomdbl2), picustomdbl3 = VALUES(picustomdbl3), picustomdate1 = VALUES(picustomdate1), picustomdate2 = VALUES(picustomdate2), picustomdate3 = VALUES(picustomdate3), pidownloaded = VALUES(pidownloaded)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Next

            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)
            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_ItemDeletePerKategori(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
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
        'pikategori(0) As String, pikategoribarang(1) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'pikategori, pikategoribarang

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pikategoribarang", AsEnumTypeData.AsString)


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


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 2) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------


            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If

            'pikategoribarang(1) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - pikategoribarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - pikategoribarang should not be more than 25 character." : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pikategori~pikategoribarang", dataRowDetail(0) & "~" & dataRowDetail(1)) = False Then
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
            'Proses detail
            If (dtdetail.Rows.Count > 0) Then
                For Each dr1 As DataRow In dtdetail.Rows
                    'AMBIL KATEGORI POS TRANSAKSI UNTUK FILTER USER LOGIN
                    sql = "SELECT pikategori as kategoripos FROM m_12_pos_item pi WHERE pi.pikategori = '" & FixQuotes(dr1("pikategori")) & "' GROUP BY pikategori"
                    Dim dtKategoriPOS As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtKategoriPOS.Rows.Count > 0 Then
                        For Each drKategoriPOS As DataRow In dtKategoriPOS.Rows
                            'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                            ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                            ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drKategoriPOS("kategoripos")) & "' "
                        Next
                    End If

                    sql = "DELETE pi FROM m_12_pos_item pi JOIN m1_item i ON pi.piidbarang = i.bid WHERE pi.pikategori = '" & FixQuotes(dr1("pikategori")) & "' AND i.bkategori = '" & FixQuotes(dr1("pikategoribarang")) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Next

            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_ItemDeleteSemua(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
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
        'pikategori(0) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'pikategori

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)


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


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 1) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------


            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pikategori", dataRowDetail(0)) = False Then
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
            'Proses detail
            If (dtdetail.Rows.Count > 0) Then
                For Each dr1 As DataRow In dtdetail.Rows
                    'AMBIL KATEGORI POS TRANSAKSI UNTUK FILTER USER LOGIN
                    sql = "SELECT pikategori as kategoripos FROM m_12_pos_item pi WHERE pi.pikategori = '" & FixQuotes(dr1("pikategori")) & "' GROUP BY pikategori"
                    Dim dtKategoriPOS As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtKategoriPOS.Rows.Count > 0 Then
                        For Each drKategoriPOS As DataRow In dtKategoriPOS.Rows
                            'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                            ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                            ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drKategoriPOS("kategoripos")) & "' "
                        Next
                    End If

                    sql = "DELETE pi FROM m_12_pos_item pi WHERE pi.pikategori = '" & FixQuotes(dr1("pikategori")) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Next

            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_ItemDeleteKelasProduk(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
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

        'Kategori POS'
        Dim KategoriPOS As String
        KategoriPOS = paramSplit(5)

        'MAPPING BUAT WS ----------------------------------------------------------
        'pikategori(0) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'pikategori

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)


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


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 1) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------


            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pikategori", dataRowDetail(0)) = False Then
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
            'Proses detail
            If (dtdetail.Rows.Count > 0) Then
                For Each dr1 As DataRow In dtdetail.Rows
                    'AMBIL KATEGORI POS TRANSAKSI UNTUK FILTER USER LOGIN
                    sql = "SELECT pikategori as kategoripos FROM m_12_pos_item pi WHERE pi.pikategori = '" & FixQuotes(dr1("pikategori")) & "' GROUP BY pikategori"
                    Dim dtKategoriPOS As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtKategoriPOS.Rows.Count > 0 Then
                        For Each drKategoriPOS As DataRow In dtKategoriPOS.Rows
                            'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                            ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                            ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drKategoriPOS("kategoripos")) & "' "
                        Next
                    End If

                    sql = "DELETE pi FROM m_12_pos_item pi JOIN m1_item i ON pi.piidbarang = i.bid JOIN m_12_pos_category pc ON pi.pikategori = pc.pckode AND pi.pikategori = '" & FixQuotes(dr1("pikategori")) & "' JOIN m_12_pos_type pt ON pc.pctipepos = pt.ptkode JOIN m_12_pos_type_class_product ptcp ON pt.ptkode = ptcp.tipepos AND i.bkelasproduk = ptcp.kelasproduk"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Next

            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_ItemSetHargaIndeks(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
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
        'pikategori(0) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'pikategori

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)


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


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 1) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------


            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pikategori", dataRowDetail(0)) = False Then
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
            'Proses detail
            If (dtdetail.Rows.Count > 0) Then
                For Each dr1 As DataRow In dtdetail.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(dr1("pikategori")) & "' "
                    'sql = "UPDATE m1_item i JOIN m_12_pos_item pi ON i.bid = pi.piidbarang JOIN m_12_pos_category pc ON pi.pikategori = pc.pckode AND pc.pckode = '" & FixQuotes(dr1("pikategori")) & "' JOIN m1_index_price ip ON pc.pcindeksharga = ip.ipkode SET pi.pihargajual1 = ROUND((CASE WHEN ip.ipmargin = 0 THEN i.bhppaverage ELSE i.bhppaverage + ((ip.ipmargin / 100) * i.bhppaverage) END),2)"
                    sql = "UPDATE m1_item i JOIN m_12_pos_item pi ON i.bid = pi.piidbarang JOIN m_12_pos_category pc ON pi.pikategori = pc.pckode AND pc.pckode = '" & FixQuotes(dr1("pikategori")) & "' JOIN m1_index_price ip ON pc.pcindeksharga = ip.ipkode SET pi.pihargajual1 = ROUND((CASE WHEN ip.ipmargin = 0 THEN i.bhargabeli ELSE i.bhargabeli + ((ip.ipmargin / 100) * i.bhargabeli) END),2), pihargaedited = 1 WHERE pihargaedited = '0'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Next

            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_ItemCekKelasProduk(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
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
        'pikategori(0) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'pikategori

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)


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


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 1) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------


            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pikategori", dataRowDetail(0)) = False Then
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
            'Proses detail
            If (dtdetail.Rows.Count > 0) Then
                For Each dr1 As DataRow In dtdetail.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(dr1("pikategori")) & "' "
                    'select '
                    sql = "Insert into M_12_Pos_Item(SELECT '" & FixQuotes(dr1("pikategori")) & "' as pikategori, bid as piidbarang, bstokminimal as pistokminimal, bstokmaksimal as pistokmaksimal, breorder as pistokreorder, bminorder as pistokminorder, bhargajual1 as pihargajual1, bhargajual2 as pihargajual2, bhargajual3 as pihargajual3, bhargajual4 as pihargajual4, bhargajual5 as pihargajual5, bdiskonjual1 as pidiskonjual1, bdiskonjual2 as pidiskonjual2, bdiskonjual3 as pidiskonjual3, bdiskonjual4 as pidiskonjual4, bdiskonjual5 as pidiskonjual5, '' as picustomtext1, '' as picustomtext2, '' as picustomtext3, '' as picustomtext4, '' as picustomtext5, 0 as picustomint1, 0 as picustomint2, 0 as picustomint3, 0 as picustomdbl1, 0 as picustomdbl2, 0 as picustomdbl3, '1900-01-01' as picustomdate1, '1900-01-01' as picustomdate2, '1900-01-01' as picustomdate3, 0 as pidownloaded FROM m1_item i JOIN m_12_pos_category pc ON pc.pckode = '" & FixQuotes(dr1("pikategori")) & "' JOIN m_12_pos_type pt ON pc.pctipepos = pt.ptkode JOIN m_12_pos_type_class_product ptcp ON pt.ptkode = ptcp.tipepos AND i.bkelasproduk = ptcp.kelasproduk GROUP BY i.bid) ON DUPLICATE KEY UPDATE pistokminimal = VALUES(pistokminimal), pistokmaksimal = VALUES(pistokmaksimal), pistokreorder = VALUES(pistokreorder), pistokminorder = VALUES(pistokminorder), pihargajual1 = VALUES(pihargajual1), pihargajual2 = VALUES(pihargajual2), pihargajual3 = VALUES(pihargajual3), pihargajual4 = VALUES(pihargajual4), pihargajual5 = VALUES(pihargajual5), pidiskonjual1 = VALUES(pidiskonjual1), pidiskonjual2 = VALUES(pidiskonjual2), pidiskonjual3 = VALUES(pidiskonjual3), pidiskonjual4 = VALUES(pidiskonjual4), pidiskonjual5 = VALUES(pidiskonjual5), picustomtext1 = VALUES(picustomtext1), picustomtext2 = VALUES(picustomtext2), picustomtext3 = VALUES(picustomtext3), picustomtext4 = VALUES(picustomtext4), picustomtext5 = VALUES(picustomtext5), picustomint1 = VALUES(picustomint1), picustomint2 = VALUES(picustomint2), picustomint3 = VALUES(picustomint3), picustomdbl1 = VALUES(picustomdbl1), picustomdbl2 = VALUES(picustomdbl2), picustomdbl3 = VALUES(picustomdbl3), picustomdate1 = VALUES(picustomdate1), picustomdate2 = VALUES(picustomdate2), picustomdate3 = VALUES(picustomdate3), pidownloaded = VALUES(pidownloaded)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Next

            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_ItemSimpanOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
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
        'pikategori(0) As String, piidbarang(1) As Integer, pistokminimal(2) As Double, pistokmaksimal(3) As Double, pistokreorder(4) As Double, 
        'pihargajual1(5) As Double, pihargajual2(6) As Double, pihargajual3(7) As Double, pihargajual4(8) As Double, pihargajual5(9) As Double, 
        'pidiskonjual1(10) As String, pidiskonjual2(11) As String, pidiskonjual3(12) As String, pidiskonjual4(13) As String, pidiskonjual5(14) As String, 
        'picustomtext1(15) As String, picustomtext2(16) As String, picustomtext3(17) As String, picustomtext4(18) As String, picustomtext5(19) As String, 
        'picustomint1(20) As Integer, picustomint2(21) As Integer, picustomint3(22) As Integer, picustomdbl1(23) As Double, picustomdbl2(24) As Double, 
        'picustomdbl3(25) As Double, picustomdate1(26) As Date, picustomdate2(27) As Date, picustomdate3(28) As Date, piaksi(29) As Integer, pistokminorder(30) As Double

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'pikategori, piidbarang, pistokminimal, pistokmaksimal, pistokreorder, pihargajual1, pihargajual2, 
        'pihargajual3, pihargajual4, pihargajual5, pidiskonjual1, pidiskonjual2, pidiskonjual3, pidiskonjual4, 
        'pidiskonjual5, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, 
        'picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, 
        'picustomdate3, piaksi, pistokminorder

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "piidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pistokminimal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pistokmaksimal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pistokreorder", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pihargajual1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pihargajual2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pihargajual3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pihargajual4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pihargajual5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pidiskonjual1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pidiskonjual2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pidiskonjual3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pidiskonjual4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pidiskonjual5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "picustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "picustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "picustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "piaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pistokminorder", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pihargaedited", AsEnumTypeData.AsString) 'tambahan pihargaedited'

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


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            'If (dataRowDetail.Length <> 31) Then
            '    result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            'End If
            If (dataRowDetail.Length <> 32) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'piidbarang(1) As Double
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - piidbarang required numeric." : GoTo selesai
            End If
            'pistokminimal(2) As Double
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - pistokminimal required numeric." : GoTo selesai
            End If
            'pistokmaksimal(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - pistokmaksimal required numeric." : GoTo selesai
            End If
            'pistokreorder(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - pistokreorder required numeric." : GoTo selesai
            End If
            'pihargajual1(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - pihargajual1 required numeric." : GoTo selesai
            End If
            'pihargajual2(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - pihargajual2 required numeric." : GoTo selesai
            End If
            'pihargajual3(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - pihargajual3 required numeric." : GoTo selesai
            End If
            'pihargajual4(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - pihargajual4 required numeric." : GoTo selesai
            End If
            'pihargajual5(9) As Double
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - pihargajual5 required numeric." : GoTo selesai
            End If
            'picustomint1(20) As Integer
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - picustomint1 required numeric." : GoTo selesai
            End If
            'picustomint2(21) As Integer
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - picustomint2 required numeric." : GoTo selesai
            End If
            'picustomint3(22) As Integer
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - picustomint3 required numeric." : GoTo selesai
            End If
            'picustomdbl1(23) As Double
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - picustomdbl1 required numeric." : GoTo selesai
            End If
            'picustomdbl2(24) As Double
            If (IsNumeric(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - picustomdbl2 required numeric." : GoTo selesai
            End If
            'picustomdbl3(25) As Double
            If (IsNumeric(dataRowDetail(25)) = False) Then
                result(2) = "Row : " & i & " - picustomdbl3 required numeric." : GoTo selesai
            End If
            'picustomdate1(26) As Date
            If (IsDate(dataRowDetail(26)) = False) Then
                result(2) = "Row : " & i & " - picustomdate1 required date." : GoTo selesai
            End If
            'picustomdate2(27) As Date
            If (IsDate(dataRowDetail(27)) = False) Then
                result(2) = "Row : " & i & " - picustomdate2 required date." : GoTo selesai
            End If
            'picustomdate3(28) As Date
            If (IsDate(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - picustomdate3 required date." : GoTo selesai
            End If
            'piaksi(29) As Integer
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - piaksi required numeric." : GoTo selesai
            ElseIf (dataRowDetail(29) <> 0 And dataRowDetail(29) <> 1) Then
                result(2) = "Row : " & i & " - invalid piaksi value. (0:delete, 1:insert/update)" : GoTo selesai
            End If
            'pistokminorder(30) As Double
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - pistokminorder required numeric." : GoTo selesai
            End If

            'pihargaedited(30) As Double
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - pihargaedited required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If

            'piidbarang(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - piidbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - piidbarang should not be more than 20 character." : GoTo selesai
            End If

            'pistokminimal(2) As Double
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - pistokminimal can't be empty" : GoTo selesai
            End If

            'pistokmaksimal(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - pistokmaksimal can't be empty" : GoTo selesai
            End If

            'pistokreorder(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - pistokreorder can't be empty" : GoTo selesai
            End If

            'pihargajual1(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - pihargajual1 can't be empty" : GoTo selesai
            End If

            'pihargajual2(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - pihargajual2 can't be empty" : GoTo selesai
            End If

            'pihargajual3(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - pihargajual3 can't be empty" : GoTo selesai
            End If

            'pihargajual4(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - pihargajual4 can't be empty" : GoTo selesai
            End If

            'pihargajual5(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - pihargajual5 can't be empty" : GoTo selesai
            End If

            'pidiskonjual1(10) As String
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - pidiskonjual1 can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(10)) > 25 Then
                result(2) = "Row : " & i & " - pidiskonjual1 should not be more than 25 character." : GoTo selesai
            End If

            'pidiskonjual2(11) As String
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - pidiskonjual2 can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(11)) > 25 Then
                result(2) = "Row : " & i & " - pidiskonjual2 should not be more than 25 character." : GoTo selesai
            End If

            'pidiskonjual3(12) As String
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - pidiskonjual3 can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(12)) > 25 Then
                result(2) = "Row : " & i & " - pidiskonjual3 should not be more than 25 character." : GoTo selesai
            End If

            'pidiskonjual4(13) As String
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - pidiskonjual4 can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(13)) > 25 Then
                result(2) = "Row : " & i & " - pidiskonjual4 should not be more than 25 character." : GoTo selesai
            End If

            'pidiskonjual5(14) As String
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - pidiskonjual5 can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(14)) > 25 Then
                result(2) = "Row : " & i & " - pidiskonjual5 should not be more than 25 character." : GoTo selesai
            End If

            'picustomdbl1(23) As Double
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - picustomdbl1 can't be empty" : GoTo selesai
            End If

            'picustomdbl2(24) As Double
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - picustomdbl2 can't be empty" : GoTo selesai
            End If

            'picustomdbl3(25) As Double
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - picustomdbl3 can't be empty" : GoTo selesai
            End If

            'picustomdate1(26) As Date
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - picustomdate1 can't be empty" : GoTo selesai
            End If

            'picustomdate2(27) As Date
            If Len(dataRowDetail(27)) = 0 Then
                result(2) = "Row : " & i & " - picustomdate2 can't be empty" : GoTo selesai
            End If

            'picustomdate3(28) As Date
            If Len(dataRowDetail(28)) = 0 Then
                result(2) = "Row : " & i & " - picustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            'If AsDataTableTambahData(dtdetail, "pikategori~piidbarang~pistokminimal~pistokmaksimal~pistokreorder~pihargajual1~pihargajual2~pihargajual3~pihargajual4~pihargajual5~pidiskonjual1~pidiskonjual2~pidiskonjual3~pidiskonjual4~pidiskonjual5~picustomtext1~picustomtext2~picustomtext3~picustomtext4~picustomtext5~picustomint1~picustomint2~picustomint3~picustomdbl1~picustomdbl2~picustomdbl3~picustomdate1~picustomdate2~picustomdate3~piaksi~pistokminorder~pihargaedited", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30)) = False Then
            '    result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            'End If

            If AsDataTableTambahData(dtdetail, "pikategori~piidbarang~pistokminimal~pistokmaksimal~pistokreorder~pihargajual1~pihargajual2~pihargajual3~pihargajual4~pihargajual5~pidiskonjual1~pidiskonjual2~pidiskonjual3~pidiskonjual4~pidiskonjual5~picustomtext1~picustomtext2~picustomtext3~picustomtext4~picustomtext5~picustomint1~picustomint2~picustomint3~picustomdbl1~picustomdbl2~picustomdbl3~picustomdate1~picustomdate2~picustomdate3~piaksi~pistokminorder~pihargaedited", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31)) = False Then
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
            'Proses detail
            If (dtdetail.Rows.Count > 0) Then
                Dim strValue1, strValue2 As New StringBuilder
                Dim dtInsert, dtDelete As New DataTable

                'Proses delete barang : piaksi = 0
                dtDelete = AsDataTableFilterSortDt(dtdetail, "piaksi = 0")
                Dim KategoriPOS As String
                If dtDelete.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtDelete.Rows
                        'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                        ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                        ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(dr1("pikategori")) & "' "

                        strValue1.Append(IIf(Len(strValue1.ToString) = 0, "", " OR "))
                        KategoriPOS = FixQuotes(dr1("pikategori"))

                        'CEK TERKAIT =============================================================
                        Dim paramTerkait As String = M12_Pos_ItemTerkait(PostWsTerkait(paramSplit(0), "M12_Pos_ItemTerkait", pagingSplit(0), pagingSplit(1), "", "", formatTgl, formatTglWaktu, FixQuotes(dr1("pikategori")) & "△" & FixQuotes(dr1("piidbarang"))))
                        Dim hasilTerkait As New RsHasilWsSearch
                        hasilTerkait = GetWsSearch(paramTerkait)
                        If hasilTerkait.success = 1 Then
                            result(2) = "This Item has related transactions."

                            resultPaging(0) = hasilTerkait.isPaging
                            resultPaging(1) = hasilTerkait.isNext
                            resultPaging(2) = hasilTerkait.isPrevious
                            resultPaging(3) = hasilTerkait.countPage
                            resultPaging(4) = hasilTerkait.countRow

                            search = hasilTerkait.data : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF CEK TERKAIT ======================================================

                        strValue1.Append("(pikategori = '" & FixQuotes(dr1("pikategori")) & "' AND piidbarang = '" & FixQuotes(dr1("piidbarang")) & "')")
                    Next
                    'hapus barang
                    If Len(strValue1.ToString) > 0 Then



                        sql = "Delete from M_12_Pos_Item where " & strValue1.ToString & ""
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

                'Proses insert barang : piaksi = 1
                dtInsert = AsDataTableFilterSortDt(dtdetail, "piaksi = 1")
                If dtInsert.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtInsert.Rows
                        'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                        ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                        ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(dr1("pikategori")) & "' "

                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        'strValue2.Append("('" & FixQuotes(dr1("pikategori")) & "', '" & FixQuotes(dr1("piidbarang")) & "', '" & FixDouble(dr1("pistokminimal")) & "', '" & FixDouble(dr1("pistokmaksimal")) & "', '" & FixDouble(dr1("pistokreorder")) & "', '" & FixDouble(dr1("pihargajual1")) & "', '" & FixDouble(dr1("pihargajual2")) & "', '" & FixDouble(dr1("pihargajual3")) & "', '" & FixDouble(dr1("pihargajual4")) & "', '" & FixDouble(dr1("pihargajual5")) & "', '" & FixQuotes(dr1("pidiskonjual1")) & "', '" & FixQuotes(dr1("pidiskonjual2")) & "', '" & FixQuotes(dr1("pidiskonjual3")) & "', '" & FixQuotes(dr1("pidiskonjual4")) & "', '" & FixQuotes(dr1("pidiskonjual5")) & "', '" & FixQuotes(dr1("picustomtext1")) & "', '" & FixQuotes(dr1("picustomtext2")) & "', '" & FixQuotes(dr1("picustomtext3")) & "', '" & FixQuotes(dr1("picustomtext4")) & "', '" & FixQuotes(dr1("picustomtext5")) & "', " & dr1("picustomint1") & ", " & dr1("picustomint2") & ", " & dr1("picustomint3") & ", '" & FixDouble(dr1("picustomdbl1")) & "', '" & FixDouble(dr1("picustomdbl2")) & "', '" & FixDouble(dr1("picustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate3"))) & "', '" & FixDouble(dr1("pistokminorder")) & "')")
                        strValue2.Append("('" & FixQuotes(dr1("pikategori")) & "', '" & FixQuotes(dr1("piidbarang")) & "', '" & FixDouble(dr1("pistokminimal")) & "', '" & FixDouble(dr1("pistokmaksimal")) & "', '" & FixDouble(dr1("pistokreorder")) & "', '" & FixDouble(dr1("pihargajual1")) & "', '" & FixDouble(dr1("pihargajual2")) & "', '" & FixDouble(dr1("pihargajual3")) & "', '" & FixDouble(dr1("pihargajual4")) & "', '" & FixDouble(dr1("pihargajual5")) & "', '" & FixQuotes(dr1("pidiskonjual1")) & "', '" & FixQuotes(dr1("pidiskonjual2")) & "', '" & FixQuotes(dr1("pidiskonjual3")) & "', '" & FixQuotes(dr1("pidiskonjual4")) & "', '" & FixQuotes(dr1("pidiskonjual5")) & "', '" & FixQuotes(dr1("picustomtext1")) & "', '" & FixQuotes(dr1("picustomtext2")) & "', '" & FixQuotes(dr1("picustomtext3")) & "', '" & FixQuotes(dr1("picustomtext4")) & "', '" & FixQuotes(dr1("picustomtext5")) & "', " & dr1("picustomint1") & ", " & dr1("picustomint2") & ", " & dr1("picustomint3") & ", '" & FixDouble(dr1("picustomdbl1")) & "', '" & FixDouble(dr1("picustomdbl2")) & "', '" & FixDouble(dr1("picustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate3"))) & "', '" & FixDouble(dr1("pistokminorder")) & "', '" & FixQuotes(dr1("pihargaedited")) & "')")
                    Next
                    'insert jika data belum ada, dan update jika data sudah ada
                    If Len(strValue2.ToString) > 0 Then
                        'sql = "Insert into M_12_Pos_Item(pikategori, piidbarang, pistokminimal, pistokmaksimal, pistokreorder, pihargajual1, pihargajual2, pihargajual3, pihargajual4, pihargajual5, pidiskonjual1, pidiskonjual2, pidiskonjual3, pidiskonjual4, pidiskonjual5, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3, pistokminorder) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE pistokminimal = VALUES(pistokminimal), pistokmaksimal = VALUES(pistokmaksimal), pistokreorder = VALUES(pistokreorder), pihargajual1 = VALUES(pihargajual1), pihargajual2 = VALUES(pihargajual2), pihargajual3 = VALUES(pihargajual3), pihargajual4 = VALUES(pihargajual4), pihargajual5 = VALUES(pihargajual5), pidiskonjual1 = VALUES(pidiskonjual1), pidiskonjual2 = VALUES(pidiskonjual2), pidiskonjual3 = VALUES(pidiskonjual3), pidiskonjual4 = VALUES(pidiskonjual4), pidiskonjual5 = VALUES(pidiskonjual5), picustomtext1 = VALUES(picustomtext1), picustomtext2 = VALUES(picustomtext2), picustomtext3 = VALUES(picustomtext3), picustomtext4 = VALUES(picustomtext4), picustomtext5 = VALUES(picustomtext5), picustomint1 = VALUES(picustomint1), picustomint2 = VALUES(picustomint2), picustomint3 = VALUES(picustomint3), picustomdbl1 = VALUES(picustomdbl1), picustomdbl2 = VALUES(picustomdbl2), picustomdbl3 = VALUES(picustomdbl3), picustomdate1 = VALUES(picustomdate1), picustomdate2 = VALUES(picustomdate2), picustomdate3 = VALUES(picustomdate3), pistokminorder = VALUES(pistokminorder)"
                        sql = "Insert into M_12_Pos_Item(pikategori, piidbarang, pistokminimal, pistokmaksimal, pistokreorder, pihargajual1, pihargajual2, pihargajual3, pihargajual4, pihargajual5, pidiskonjual1, pidiskonjual2, pidiskonjual3, pidiskonjual4, pidiskonjual5, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3, pistokminorder, pihargaedited) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE pistokminimal = VALUES(pistokminimal), pistokmaksimal = VALUES(pistokmaksimal), pistokreorder = VALUES(pistokreorder), pihargajual1 = VALUES(pihargajual1), pihargajual2 = VALUES(pihargajual2), pihargajual3 = VALUES(pihargajual3), pihargajual4 = VALUES(pihargajual4), pihargajual5 = VALUES(pihargajual5), pidiskonjual1 = VALUES(pidiskonjual1), pidiskonjual2 = VALUES(pidiskonjual2), pidiskonjual3 = VALUES(pidiskonjual3), pidiskonjual4 = VALUES(pidiskonjual4), pidiskonjual5 = VALUES(pidiskonjual5), picustomtext1 = VALUES(picustomtext1), picustomtext2 = VALUES(picustomtext2), picustomtext3 = VALUES(picustomtext3), picustomtext4 = VALUES(picustomtext4), picustomtext5 = VALUES(picustomtext5), picustomint1 = VALUES(picustomint1), picustomint2 = VALUES(picustomint2), picustomint3 = VALUES(picustomint3), picustomdbl1 = VALUES(picustomdbl1), picustomdbl2 = VALUES(picustomdbl2), picustomdbl3 = VALUES(picustomdbl3), picustomdate1 = VALUES(picustomdate1), picustomdate2 = VALUES(picustomdate2), picustomdate3 = VALUES(picustomdate3), pistokminorder = VALUES(pistokminorder), pihargaedited = VALUES(pihargaedited)"
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

            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_ItemDeleteOld(ByVal param As String) As String

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
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""
        Dim Filter As String = "", Sorting As String = ""

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
        If ClsValidKey.ApaBisaAkses(1, 1, 2) = False Then
            result(2) = "Access denied for delete data"
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
        Dim pikategori As String = "", piidbarang As String = ""
        Dim idtrans(3) As String
        idtrans = paramSplit(5).Split(sptSubParam)
        If (idtrans.Length <> 2) Then
            result(2) = "Invalid primary key parameter." : GoTo selesai
        Else
            'CEK pikategori
            If (Len(idtrans(0)) = 0) Then
                result(2) = "pikategori can't be empty." : GoTo selesai
            Else
                pikategori = idtrans(0)
            End If
            'CEK piidbarang
            If (IsNumeric(idtrans(1)) = False) Then
                result(2) = "piidbarang required numeric." : GoTo selesai
            Else
                piidbarang = idtrans(1)
            End If
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'DELETE DI DATABASE ================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'AMBIL KATEGORI POS TRANSAKSI UNTUK FILTER USER LOGIN
            sql = "SELECT pikategori as kategoripos FROM M_12_Pos_Item WHERE pikategori = '" & pikategori & "' AND piidbarang = '" & piidbarang & "' GROUP BY pikategori"
            Dim dtKategoriPOS As DataTable = AsDataTableAmbilDariDB(sql)
            If dtKategoriPOS.Rows.Count > 0 Then
                For Each drKategoriPOS As DataRow In dtKategoriPOS.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drKategoriPOS("kategoripos")) & "' "
                Next
            End If

            'DELETE
            sql = "DELETE FROM M_12_Pos_Item WHERE pikategori = '" & pikategori & "' AND piidbarang = '" & piidbarang & "'"
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
            Dim paramSearch As String = M12_Pos_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_ItemImportOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean

        Dim pg1 As New RsPaging
        Dim search As String = ""
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
        'pikategori(0) As String, piidbarang(1) As Integer, pistokminimal(2) As Double, pistokmaksimal(3) As Double, pistokreorder(4) As Double, 
        'pihargajual1(5) As Double, pihargajual2(6) As Double, pihargajual3(7) As Double, pihargajual4(8) As Double, pihargajual5(9) As Double, 
        'pidiskonjual1(10) As String, pidiskonjual2(11) As String, pidiskonjual3(12) As String, pidiskonjual4(13) As String, pidiskonjual5(14) As String, 
        'picustomtext1(15) As String, picustomtext2(16) As String, picustomtext3(17) As String, picustomtext4(18) As String, picustomtext5(19) As String, 
        'picustomint1(20) As Integer, picustomint2(21) As Integer, picustomint3(22) As Integer, picustomdbl1(23) As Double, picustomdbl2(24) As Double, 
        'picustomdbl3(25) As Double, picustomdate1(26) As Date, picustomdate2(27) As Date, picustomdate3(28) As Date, piaksi(29) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'pikategori, piidbarang, pistokminimal, pistokmaksimal, pistokreorder, pihargajual1, pihargajual2, 
        'pihargajual3, pihargajual4, pihargajual5, pidiskonjual1, pidiskonjual2, pidiskonjual3, pidiskonjual4, 
        'pidiskonjual5, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, 
        'picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, 
        'picustomdate3, piaksi

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "piidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pistokminimal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pistokmaksimal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pistokreorder", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pihargajual1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pihargajual2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pihargajual3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pihargajual4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pihargajual5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pidiskonjual1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pidiskonjual2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pidiskonjual3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pidiskonjual4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pidiskonjual5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "picustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "picustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "picustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "piaksi", AsEnumTypeData.AsInt64)

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


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 30) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'piidbarang(1) As Double
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - piidbarang required numeric." : GoTo selesai
            End If
            'pistokminimal(2) As Double
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - pistokminimal required numeric." : GoTo selesai
            End If
            'pistokmaksimal(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - pistokmaksimal required numeric." : GoTo selesai
            End If
            'pistokreorder(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - pistokreorder required numeric." : GoTo selesai
            End If
            'pihargajual1(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - pihargajual1 required numeric." : GoTo selesai
            End If
            'pihargajual2(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - pihargajual2 required numeric." : GoTo selesai
            End If
            'pihargajual3(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - pihargajual3 required numeric." : GoTo selesai
            End If
            'pihargajual4(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - pihargajual4 required numeric." : GoTo selesai
            End If
            'pihargajual5(9) As Double
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - pihargajual5 required numeric." : GoTo selesai
            End If
            'picustomint1(20) As Integer
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - picustomint1 required numeric." : GoTo selesai
            End If
            'picustomint2(21) As Integer
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - picustomint2 required numeric." : GoTo selesai
            End If
            'picustomint3(22) As Integer
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - picustomint3 required numeric." : GoTo selesai
            End If
            'picustomdbl1(23) As Double
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - picustomdbl1 required numeric." : GoTo selesai
            End If
            'picustomdbl2(24) As Double
            If (IsNumeric(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - picustomdbl2 required numeric." : GoTo selesai
            End If
            'picustomdbl3(25) As Double
            If (IsNumeric(dataRowDetail(25)) = False) Then
                result(2) = "Row : " & i & " - picustomdbl3 required numeric." : GoTo selesai
            End If
            'picustomdate1(26) As Date
            If (IsDate(dataRowDetail(26)) = False) Then
                result(2) = "Row : " & i & " - picustomdate1 required date." : GoTo selesai
            End If
            'picustomdate2(27) As Date
            If (IsDate(dataRowDetail(27)) = False) Then
                result(2) = "Row : " & i & " - picustomdate2 required date." : GoTo selesai
            End If
            'picustomdate3(28) As Date
            If (IsDate(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - picustomdate3 required date." : GoTo selesai
            End If
            'piaksi(29) As Integer
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - piaksi required numeric." : GoTo selesai
            ElseIf (dataRowDetail(29) <> 0 And dataRowDetail(29) <> 1) Then
                result(2) = "Row : " & i & " - invalid piaksi value. (0:delete, 1:insert/update)" : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If

            'piidbarang(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - piidbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - piidbarang should not be more than 20 character." : GoTo selesai
            End If

            'pistokminimal(2) As Double
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - pistokminimal can't be empty" : GoTo selesai
            End If

            'pistokmaksimal(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - pistokmaksimal can't be empty" : GoTo selesai
            End If

            'pistokreorder(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - pistokreorder can't be empty" : GoTo selesai
            End If

            'pihargajual1(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - pihargajual1 can't be empty" : GoTo selesai
            End If

            'pihargajual2(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - pihargajual2 can't be empty" : GoTo selesai
            End If

            'pihargajual3(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - pihargajual3 can't be empty" : GoTo selesai
            End If

            'pihargajual4(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - pihargajual4 can't be empty" : GoTo selesai
            End If

            'pihargajual5(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - pihargajual5 can't be empty" : GoTo selesai
            End If

            'pidiskonjual1(10) As String
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - pidiskonjual1 can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(10)) > 25 Then
                result(2) = "Row : " & i & " - pidiskonjual1 should not be more than 25 character." : GoTo selesai
            End If

            'pidiskonjual2(11) As String
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - pidiskonjual2 can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(11)) > 25 Then
                result(2) = "Row : " & i & " - pidiskonjual2 should not be more than 25 character." : GoTo selesai
            End If

            'pidiskonjual3(12) As String
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - pidiskonjual3 can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(12)) > 25 Then
                result(2) = "Row : " & i & " - pidiskonjual3 should not be more than 25 character." : GoTo selesai
            End If

            'pidiskonjual4(13) As String
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - pidiskonjual4 can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(13)) > 25 Then
                result(2) = "Row : " & i & " - pidiskonjual4 should not be more than 25 character." : GoTo selesai
            End If

            'pidiskonjual5(14) As String
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - pidiskonjual5 can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(14)) > 25 Then
                result(2) = "Row : " & i & " - pidiskonjual5 should not be more than 25 character." : GoTo selesai
            End If

            'picustomdbl1(23) As Double
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - picustomdbl1 can't be empty" : GoTo selesai
            End If

            'picustomdbl2(24) As Double
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - picustomdbl2 can't be empty" : GoTo selesai
            End If

            'picustomdbl3(25) As Double
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - picustomdbl3 can't be empty" : GoTo selesai
            End If

            'picustomdate1(26) As Date
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - picustomdate1 can't be empty" : GoTo selesai
            End If

            'picustomdate2(27) As Date
            If Len(dataRowDetail(27)) = 0 Then
                result(2) = "Row : " & i & " - picustomdate2 can't be empty" : GoTo selesai
            End If

            'picustomdate3(28) As Date
            If Len(dataRowDetail(28)) = 0 Then
                result(2) = "Row : " & i & " - picustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pikategori~piidbarang~pistokminimal~pistokmaksimal~pistokreorder~pihargajual1~pihargajual2~pihargajual3~pihargajual4~pihargajual5~pidiskonjual1~pidiskonjual2~pidiskonjual3~pidiskonjual4~pidiskonjual5~picustomtext1~picustomtext2~picustomtext3~picustomtext4~picustomtext5~picustomint1~picustomint2~picustomint3~picustomdbl1~picustomdbl2~picustomdbl3~picustomdate1~picustomdate2~picustomdate3~piaksi", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29)) = False Then
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
            'Proses detail
            If (dtdetail.Rows.Count > 0) Then
                Dim strValue2 As New StringBuilder
                For Each dr1 As DataRow In dtdetail.Rows
                    strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Append("('" & FixQuotes(dr1("pikategori")) & "', '" & FixQuotes(dr1("piidbarang")) & "', '" & FixDouble(dr1("pistokminimal")) & "', '" & FixDouble(dr1("pistokmaksimal")) & "', '" & FixDouble(dr1("pistokreorder")) & "', '" & FixDouble(dr1("pihargajual1")) & "', '" & FixDouble(dr1("pihargajual2")) & "', '" & FixDouble(dr1("pihargajual3")) & "', '" & FixDouble(dr1("pihargajual4")) & "', '" & FixDouble(dr1("pihargajual5")) & "', '" & FixQuotes(dr1("pidiskonjual1")) & "', '" & FixQuotes(dr1("pidiskonjual2")) & "', '" & FixQuotes(dr1("pidiskonjual3")) & "', '" & FixQuotes(dr1("pidiskonjual4")) & "', '" & FixQuotes(dr1("pidiskonjual5")) & "', '" & FixQuotes(dr1("picustomtext1")) & "', '" & FixQuotes(dr1("picustomtext2")) & "', '" & FixQuotes(dr1("picustomtext3")) & "', '" & FixQuotes(dr1("picustomtext4")) & "', '" & FixQuotes(dr1("picustomtext5")) & "', " & dr1("picustomint1") & ", " & dr1("picustomint2") & ", " & dr1("picustomint3") & ", '" & FixDouble(dr1("picustomdbl1")) & "', '" & FixDouble(dr1("picustomdbl2")) & "', '" & FixDouble(dr1("picustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate3"))) & "')")
                Next

                If Len(strValue2.ToString) > 0 Then
                    'DELETE
                    sql = "DELETE FROM M_12_Pos_Item"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'insert jika data belum ada, dan update jika data sudah ada
                    sql = "Insert into M_12_Pos_Item(pikategori, piidbarang, pistokminimal, pistokmaksimal, pistokreorder, pihargajual1, pihargajual2, pihargajual3, pihargajual4, pihargajual5, pidiskonjual1, pidiskonjual2, pidiskonjual3, pidiskonjual4, pidiskonjual5, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE pistokminimal = VALUES(pistokminimal), pistokmaksimal = VALUES(pistokmaksimal), pistokreorder = VALUES(pistokreorder), pihargajual1 = VALUES(pihargajual1), pihargajual2 = VALUES(pihargajual2), pihargajual3 = VALUES(pihargajual3), pihargajual4 = VALUES(pihargajual4), pihargajual5 = VALUES(pihargajual5), pidiskonjual1 = VALUES(pidiskonjual1), pidiskonjual2 = VALUES(pidiskonjual2), pidiskonjual3 = VALUES(pidiskonjual3), pidiskonjual4 = VALUES(pidiskonjual4), pidiskonjual5 = VALUES(pidiskonjual5), picustomtext1 = VALUES(picustomtext1), picustomtext2 = VALUES(picustomtext2), picustomtext3 = VALUES(picustomtext3), picustomtext4 = VALUES(picustomtext4), picustomtext5 = VALUES(picustomtext5), picustomint1 = VALUES(picustomint1), picustomint2 = VALUES(picustomint2), picustomint3 = VALUES(picustomint3), picustomdbl1 = VALUES(picustomdbl1), picustomdbl2 = VALUES(picustomdbl2), picustomdbl3 = VALUES(picustomdbl3), picustomdate1 = VALUES(picustomdate1), picustomdate2 = VALUES(picustomdate2), picustomdate3 = VALUES(picustomdate3)"
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
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_ItemSimpanPerKategoriOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
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
        'pikategori(0) As String, pikategoribarang(1) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'pikategori, pikategoribarang

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pikategoribarang", AsEnumTypeData.AsString)


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


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 2) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------


            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If

            'pikategoribarang(1) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - pikategoribarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - pikategoribarang should not be more than 25 character." : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pikategori~pikategoribarang", dataRowDetail(0) & "~" & dataRowDetail(1)) = False Then
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
            'Proses detail
            If (dtdetail.Rows.Count > 0) Then
                For Each dr1 As DataRow In dtdetail.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(dr1("pikategori")) & "' "

                    'sql = "Insert into M_12_Pos_Item(SELECT '" & FixQuotes(dr1("pikategori")) & "' as pikategori, bid as piidbarang, bstokminimal as pistokminimal, bstokmaksimal as pistokmaksimal, breorder as pistokreorder, bminorder as pistokminorder, bhargajual1 as pihargajual1, bhargajual2 as pihargajual2, bhargajual3 as pihargajual3, bhargajual4 as pihargajual4, bhargajual5 as pihargajual5, bdiskonjual1 as pidiskonjual1, bdiskonjual2 as pidiskonjual2, bdiskonjual3 as pidiskonjual3, bdiskonjual4 as pidiskonjual4, bdiskonjual5 as pidiskonjual5, '' as picustomtext1, '' as picustomtext2, '' as picustomtext3, '' as picustomtext4, '' as picustomtext5, 0 as picustomint1, 0 as picustomint2, 0 as picustomint3, 0 as picustomdbl1, 0 as picustomdbl2, 0 as picustomdbl3, '1900-01-01' as picustomdate1, '1900-01-01' as picustomdate2, '1900-01-01' as picustomdate3, 0 as pidownloaded FROM m1_item WHERE bkategori = '" & FixQuotes(dr1("pikategoribarang")) & "') ON DUPLICATE KEY UPDATE pistokminimal = VALUES(pistokminimal), pistokmaksimal = VALUES(pistokmaksimal), pistokreorder = VALUES(pistokreorder), pistokminorder = VALUES(pistokminorder), pihargajual1 = VALUES(pihargajual1), pihargajual2 = VALUES(pihargajual2), pihargajual3 = VALUES(pihargajual3), pihargajual4 = VALUES(pihargajual4), pihargajual5 = VALUES(pihargajual5), pidiskonjual1 = VALUES(pidiskonjual1), pidiskonjual2 = VALUES(pidiskonjual2), pidiskonjual3 = VALUES(pidiskonjual3), pidiskonjual4 = VALUES(pidiskonjual4), pidiskonjual5 = VALUES(pidiskonjual5), picustomtext1 = VALUES(picustomtext1), picustomtext2 = VALUES(picustomtext2), picustomtext3 = VALUES(picustomtext3), picustomtext4 = VALUES(picustomtext4), picustomtext5 = VALUES(picustomtext5), picustomint1 = VALUES(picustomint1), picustomint2 = VALUES(picustomint2), picustomint3 = VALUES(picustomint3), picustomdbl1 = VALUES(picustomdbl1), picustomdbl2 = VALUES(picustomdbl2), picustomdbl3 = VALUES(picustomdbl3), picustomdate1 = VALUES(picustomdate1), picustomdate2 = VALUES(picustomdate2), picustomdate3 = VALUES(picustomdate3), pidownloaded = VALUES(pidownloaded)"
                    sql = "Insert into M_12_Pos_Item(SELECT '" & FixQuotes(dr1("pikategori")) & "' as pikategori, bid as piidbarang, bstokminimal as pistokminimal, bstokmaksimal as pistokmaksimal, breorder as pistokreorder, bminorder as pistokminorder, bhargajual1 as pihargajual1, bhargajual2 as pihargajual2, bhargajual3 as pihargajual3, bhargajual4 as pihargajual4, bhargajual5 as pihargajual5, bdiskonjual1 as pidiskonjual1, bdiskonjual2 as pidiskonjual2, bdiskonjual3 as pidiskonjual3, bdiskonjual4 as pidiskonjual4, bdiskonjual5 as pidiskonjual5, '' as picustomtext1, '' as picustomtext2, '' as picustomtext3, '' as picustomtext4, '' as picustomtext5, 0 as picustomint1, 0 as picustomint2, 0 as picustomint3, 0 as picustomdbl1, 0 as picustomdbl2, 0 as picustomdbl3, '1900-01-01' as picustomdate1, '1900-01-01' as picustomdate2, '1900-01-01' as picustomdate3, 0 as pidownloaded, 0 as pihargaedited FROM m1_item WHERE bkategori = '" & FixQuotes(dr1("pikategoribarang")) & "') ON DUPLICATE KEY UPDATE picustomtext1 = VALUES(picustomtext1), picustomtext2 = VALUES(picustomtext2), picustomtext3 = VALUES(picustomtext3), picustomtext4 = VALUES(picustomtext4), picustomtext5 = VALUES(picustomtext5), picustomint1 = VALUES(picustomint1), picustomint2 = VALUES(picustomint2), picustomint3 = VALUES(picustomint3), picustomdbl1 = VALUES(picustomdbl1), picustomdbl2 = VALUES(picustomdbl2), picustomdbl3 = VALUES(picustomdbl3), picustomdate1 = VALUES(picustomdate1), picustomdate2 = VALUES(picustomdate2), picustomdate3 = VALUES(picustomdate3)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Next

            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_ItemSimpanKelasProdukOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
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
        'pikategori(0) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'pikategori

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)


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


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 1) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------


            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pikategori", dataRowDetail(0)) = False Then
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
        Dim KategoriPOS As String = ""
        Try
            'Proses detail


            If (dtdetail.Rows.Count > 0) Then
                For Each dr1 As DataRow In dtdetail.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(dr1("pikategori")) & "' "
                    'select '
                    'SELECT `pi`.`piidbarang` AS idbarang,  `pp`.`kelasproduk`,  `i`.`bid`, `i`.bkelasproduk,  `i`.`bnama`,  `pi`.`pikategori`,  pc.pctipepos  FROM m_12_pos_item `pi` JOIN m1_item `i` ON pi.piidbarang = i.bid JOIN m_12_pos_category `pc` ON pi.pikategori = pc.pckode JOIN m_12_pos_type `pt` ON pc.pctipepos = pt.ptkode JOIN m_12_pos_type_class_product `pp` ON pp.tipepos = pc.pctipepos  WHERE pi.pikategori = "valkode" AND i.bkelasproduk = (SELECT pph.kelasproduk FROM m_12_pos_category `pch`  JOIN m_12_pos_type `pth` ON pch.pctipepos = pth.ptkode JOIN m_12_pos_type_class_product `pph` ON pph.tipepos = pch.pctipepos LIMIT 1 ) GROUP BY pi.piidbarang , pp.kelasproduk
                    KategoriPOS = FixQuotes(dr1("pikategori"))


                    'sql = "Insert into M_12_Pos_Item(SELECT '" & FixQuotes(dr1("pikategori")) & "' as pikategori, bid as piidbarang, bstokminimal as pistokminimal, bstokmaksimal as pistokmaksimal, breorder as pistokreorder, bminorder as pistokminorder, bhargajual1 as pihargajual1, bhargajual2 as pihargajual2, bhargajual3 as pihargajual3, bhargajual4 as pihargajual4, bhargajual5 as pihargajual5, bdiskonjual1 as pidiskonjual1, bdiskonjual2 as pidiskonjual2, bdiskonjual3 as pidiskonjual3, bdiskonjual4 as pidiskonjual4, bdiskonjual5 as pidiskonjual5, '' as picustomtext1, '' as picustomtext2, '' as picustomtext3, '' as picustomtext4, '' as picustomtext5, 0 as picustomint1, 0 as picustomint2, 0 as picustomint3, 0 as picustomdbl1, 0 as picustomdbl2, 0 as picustomdbl3, '1900-01-01' as picustomdate1, '1900-01-01' as picustomdate2, '1900-01-01' as picustomdate3, 0 as pidownloaded FROM m1_item i JOIN m_12_pos_category pc ON pc.pckode = '" & FixQuotes(dr1("pikategori")) & "' JOIN m_12_pos_type pt ON pc.pctipepos = pt.ptkode JOIN m_12_pos_type_class_product ptcp ON pt.ptkode = ptcp.tipepos AND i.bkelasproduk = ptcp.kelasproduk GROUP BY i.bid) ON DUPLICATE KEY UPDATE pistokminimal = VALUES(pistokminimal), pistokmaksimal = VALUES(pistokmaksimal), pistokreorder = VALUES(pistokreorder), pistokminorder = VALUES(pistokminorder), pihargajual1 = VALUES(pihargajual1), pihargajual2 = VALUES(pihargajual2), pihargajual3 = VALUES(pihargajual3), pihargajual4 = VALUES(pihargajual4), pihargajual5 = VALUES(pihargajual5), pidiskonjual1 = VALUES(pidiskonjual1), pidiskonjual2 = VALUES(pidiskonjual2), pidiskonjual3 = VALUES(pidiskonjual3), pidiskonjual4 = VALUES(pidiskonjual4), pidiskonjual5 = VALUES(pidiskonjual5), picustomtext1 = VALUES(picustomtext1), picustomtext2 = VALUES(picustomtext2), picustomtext3 = VALUES(picustomtext3), picustomtext4 = VALUES(picustomtext4), picustomtext5 = VALUES(picustomtext5), picustomint1 = VALUES(picustomint1), picustomint2 = VALUES(picustomint2), picustomint3 = VALUES(picustomint3), picustomdbl1 = VALUES(picustomdbl1), picustomdbl2 = VALUES(picustomdbl2), picustomdbl3 = VALUES(picustomdbl3), picustomdate1 = VALUES(picustomdate1), picustomdate2 = VALUES(picustomdate2), picustomdate3 = VALUES(picustomdate3), pidownloaded = VALUES(pidownloaded)"
                    sql = "Insert into M_12_Pos_Item(SELECT '" & FixQuotes(dr1("pikategori")) & "' as pikategori, bid as piidbarang, bstokminimal as pistokminimal, bstokmaksimal as pistokmaksimal, breorder as pistokreorder, bminorder as pistokminorder, bhargajual1 as pihargajual1, bhargajual2 as pihargajual2, bhargajual3 as pihargajual3, bhargajual4 as pihargajual4, bhargajual5 as pihargajual5, bdiskonjual1 as pidiskonjual1, bdiskonjual2 as pidiskonjual2, bdiskonjual3 as pidiskonjual3, bdiskonjual4 as pidiskonjual4, bdiskonjual5 as pidiskonjual5, '' as picustomtext1, '' as picustomtext2, '' as picustomtext3, '' as picustomtext4, '' as picustomtext5, 0 as picustomint1, 0 as picustomint2, 0 as picustomint3, 0 as picustomdbl1, 0 as picustomdbl2, 0 as picustomdbl3, '1900-01-01' as picustomdate1, '1900-01-01' as picustomdate2, '1900-01-01' as picustomdate3, 0 as pidownloaded, 0 as pihargaedited FROM m1_item i JOIN m_12_pos_category pc ON pc.pckode = '" & FixQuotes(dr1("pikategori")) & "' JOIN m_12_pos_type pt ON pc.pctipepos = pt.ptkode JOIN m_12_pos_type_class_product ptcp ON pt.ptkode = ptcp.tipepos AND i.bkelasproduk = ptcp.kelasproduk GROUP BY i.bid) ON DUPLICATE KEY UPDATE pidownloaded = VALUES(pidownloaded), pihargaedited = VALUES(pihargaedited)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Next

                'HAPUS DATA BARANG POS YANG KELAS PRODUKNYA TIDAK SESUAI DENGAN KELAS PRODUK KATEGORI POS YANG DIPILIH ==========================
                Dim filterkp As String = ""
                Dim dtkp As New DataTable
                dtkp = AsDataTableAmbilDariDB("SELECT pph.kelasproduk as kelasproduk FROM m_12_pos_category `pch` JOIN m_12_pos_type `pth` ON pch.pctipepos = pth.ptkode JOIN m_12_pos_type_class_product `pph` ON pph.tipepos = pch.pctipepos WHERE pch.pckode = '" & KategoriPOS & "'")
                If dtkp.Rows.Count > 0 Then
                    For Each drkp As DataRow In dtkp.Rows
                        filterkp = IIf(filterkp.Length > 0, filterkp & " AND ", "")
                        filterkp &= " i.bkelasproduk <> '" & FixQuotes(drkp("kelasproduk")) & "' "
                    Next
                End If

                Dim query As String = "SELECT `pi`.`piidbarang` AS idbarang FROM m_12_pos_item `pi` JOIN m1_item `i` ON pi.piidbarang = i.bid JOIN m_12_pos_category `pc` ON pi.pikategori = pc.pckode JOIN m_12_pos_type `pt` ON pc.pctipepos = pt.ptkode JOIN m_12_pos_type_class_product `pp` ON pp.tipepos = pc.pctipepos  WHERE pi.pikategori = '" & KategoriPOS & "' AND (" & filterkp & ") GROUP BY pi.piidbarang , pp.kelasproduk"
                Dim dt2 As New DataTable
                dt2 = AsDataTableAmbilDariDB(query)
                If dt2.Rows.Count > 0 Then

                    For Each dr2 As DataRow In dt2.Rows
                        sql = "DELETE FROM m_12_pos_item WHERE pikategori = '" & KategoriPOS & "' AND piidbarang = '" & FixQuotes(dr2("idbarang")) & "'"

                        'result(2) = sql : Trans.Rollback() : GoTo selesai
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Next

                End If
                'END OF HAPUS DATA BARANG POS YANG KELAS PRODUKNYA TIDAK SESUAI DENGAN KELAS PRODUK KATEGORI POS YANG DIPILIH ==========================


            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)
            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_ItemSimpanSemuaOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
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
        'pikategori(0) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'pikategori

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)


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


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 1) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------


            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pikategori", dataRowDetail(0)) = False Then
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
            'Proses detail
            If (dtdetail.Rows.Count > 0) Then
                For Each dr1 As DataRow In dtdetail.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(dr1("pikategori")) & "' "

                    'sql = "Insert into M_12_Pos_Item(SELECT '" & FixQuotes(dr1("pikategori")) & "' as pikategori, bid as piidbarang, bstokminimal as pistokminimal, bstokmaksimal as pistokmaksimal, breorder as pistokreorder, bminorder as pistokminorder, bhargajual1 as pihargajual1, bhargajual2 as pihargajual2, bhargajual3 as pihargajual3, bhargajual4 as pihargajual4, bhargajual5 as pihargajual5, bdiskonjual1 as pidiskonjual1, bdiskonjual2 as pidiskonjual2, bdiskonjual3 as pidiskonjual3, bdiskonjual4 as pidiskonjual4, bdiskonjual5 as pidiskonjual5, '' as picustomtext1, '' as picustomtext2, '' as picustomtext3, '' as picustomtext4, '' as picustomtext5, 0 as picustomint1, 0 as picustomint2, 0 as picustomint3, 0 as picustomdbl1, 0 as picustomdbl2, 0 as picustomdbl3, '1900-01-01' as picustomdate1, '1900-01-01' as picustomdate2, '1900-01-01' as picustomdate3, 0 as pidownloaded, 0 as pidownloaded FROM m1_item) ON DUPLICATE KEY UPDATE pistokminimal = VALUES(pistokminimal), pistokmaksimal = VALUES(pistokmaksimal), pistokreorder = VALUES(pistokreorder), pistokminorder = VALUES(pistokminorder), pihargajual1 = VALUES(pihargajual1), pihargajual2 = VALUES(pihargajual2), pihargajual3 = VALUES(pihargajual3), pihargajual4 = VALUES(pihargajual4), pihargajual5 = VALUES(pihargajual5), pidiskonjual1 = VALUES(pidiskonjual1), pidiskonjual2 = VALUES(pidiskonjual2), pidiskonjual3 = VALUES(pidiskonjual3), pidiskonjual4 = VALUES(pidiskonjual4), pidiskonjual5 = VALUES(pidiskonjual5), picustomtext1 = VALUES(picustomtext1), picustomtext2 = VALUES(picustomtext2), picustomtext3 = VALUES(picustomtext3), picustomtext4 = VALUES(picustomtext4), picustomtext5 = VALUES(picustomtext5), picustomint1 = VALUES(picustomint1), picustomint2 = VALUES(picustomint2), picustomint3 = VALUES(picustomint3), picustomdbl1 = VALUES(picustomdbl1), picustomdbl2 = VALUES(picustomdbl2), picustomdbl3 = VALUES(picustomdbl3), picustomdate1 = VALUES(picustomdate1), picustomdate2 = VALUES(picustomdate2), picustomdate3 = VALUES(picustomdate3), pidownloaded = VALUES(pidownloaded)"
                    sql = "Insert into M_12_Pos_Item(SELECT '" & FixQuotes(dr1("pikategori")) & "' as pikategori, bid as piidbarang, bstokminimal as pistokminimal, bstokmaksimal as pistokmaksimal, breorder as pistokreorder, bminorder as pistokminorder, bhargajual1 as pihargajual1, bhargajual2 as pihargajual2, bhargajual3 as pihargajual3, bhargajual4 as pihargajual4, bhargajual5 as pihargajual5, bdiskonjual1 as pidiskonjual1, bdiskonjual2 as pidiskonjual2, bdiskonjual3 as pidiskonjual3, bdiskonjual4 as pidiskonjual4, bdiskonjual5 as pidiskonjual5, '' as picustomtext1, '' as picustomtext2, '' as picustomtext3, '' as picustomtext4, '' as picustomtext5, 0 as picustomint1, 0 as picustomint2, 0 as picustomint3, 0 as picustomdbl1, 0 as picustomdbl2, 0 as picustomdbl3, '1900-01-01' as picustomdate1, '1900-01-01' as picustomdate2, '1900-01-01' as picustomdate3, 0 as pidownloaded, 0 as pihargaedited FROM m1_item) ON DUPLICATE KEY UPDATE picustomtext1 = VALUES(picustomtext1), picustomtext2 = VALUES(picustomtext2), picustomtext3 = VALUES(picustomtext3), picustomtext4 = VALUES(picustomtext4), picustomtext5 = VALUES(picustomtext5), picustomint1 = VALUES(picustomint1), picustomint2 = VALUES(picustomint2), picustomint3 = VALUES(picustomint3), picustomdbl1 = VALUES(picustomdbl1), picustomdbl2 = VALUES(picustomdbl2), picustomdbl3 = VALUES(picustomdbl3), picustomdate1 = VALUES(picustomdate1), picustomdate2 = VALUES(picustomdate2), picustomdate3 = VALUES(picustomdate3)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Next

            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_ItemSimpanLokasiLainOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
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
        'pikategori(0) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'pikategori

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pikategorilain", AsEnumTypeData.AsString)


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


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 2) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------


            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If

            'pikategorilain(0) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - pikategorilain can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategorilain should not be more than 25 character." : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pikategori~pikategorilain", dataRowDetail(0) & "~" & dataRowDetail(1)) = False Then
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
        Dim KategoriPOS As String = ""
        Try
            'Proses detail


            If (dtdetail.Rows.Count > 0) Then
                For Each dr1 As DataRow In dtdetail.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(dr1("pikategori")) & "' "


                    'sql = "Insert into M_12_Pos_Item(SELECT '" & FixQuotes(dr1("pikategori")) & "' as pikategori, bid as piidbarang, bstokminimal as pistokminimal, bstokmaksimal as pistokmaksimal, breorder as pistokreorder, bminorder as pistokminorder, bhargajual1 as pihargajual1, bhargajual2 as pihargajual2, bhargajual3 as pihargajual3, bhargajual4 as pihargajual4, bhargajual5 as pihargajual5, bdiskonjual1 as pidiskonjual1, bdiskonjual2 as pidiskonjual2, bdiskonjual3 as pidiskonjual3, bdiskonjual4 as pidiskonjual4, bdiskonjual5 as pidiskonjual5, '' as picustomtext1, '' as picustomtext2, '' as picustomtext3, '' as picustomtext4, '' as picustomtext5, 0 as picustomint1, 0 as picustomint2, 0 as picustomint3, 0 as picustomdbl1, 0 as picustomdbl2, 0 as picustomdbl3, '1900-01-01' as picustomdate1, '1900-01-01' as picustomdate2, '1900-01-01' as picustomdate3, 0 as pidownloaded FROM m1_item i JOIN m_12_pos_category pc ON pc.pckode = '" & FixQuotes(dr1("pikategori")) & "' JOIN m_12_pos_type pt ON pc.pctipepos = pt.ptkode JOIN m_12_pos_type_class_product ptcp ON pt.ptkode = ptcp.tipepos AND i.bkelasproduk = ptcp.kelasproduk GROUP BY i.bid) ON DUPLICATE KEY UPDATE pistokminimal = VALUES(pistokminimal), pistokmaksimal = VALUES(pistokmaksimal), pistokreorder = VALUES(pistokreorder), pistokminorder = VALUES(pistokminorder), pihargajual1 = VALUES(pihargajual1), pihargajual2 = VALUES(pihargajual2), pihargajual3 = VALUES(pihargajual3), pihargajual4 = VALUES(pihargajual4), pihargajual5 = VALUES(pihargajual5), pidiskonjual1 = VALUES(pidiskonjual1), pidiskonjual2 = VALUES(pidiskonjual2), pidiskonjual3 = VALUES(pidiskonjual3), pidiskonjual4 = VALUES(pidiskonjual4), pidiskonjual5 = VALUES(pidiskonjual5), picustomtext1 = VALUES(picustomtext1), picustomtext2 = VALUES(picustomtext2), picustomtext3 = VALUES(picustomtext3), picustomtext4 = VALUES(picustomtext4), picustomtext5 = VALUES(picustomtext5), picustomint1 = VALUES(picustomint1), picustomint2 = VALUES(picustomint2), picustomint3 = VALUES(picustomint3), picustomdbl1 = VALUES(picustomdbl1), picustomdbl2 = VALUES(picustomdbl2), picustomdbl3 = VALUES(picustomdbl3), picustomdate1 = VALUES(picustomdate1), picustomdate2 = VALUES(picustomdate2), picustomdate3 = VALUES(picustomdate3), pidownloaded = VALUES(pidownloaded)"
                    sql = "Insert into M_12_Pos_Item(SELECT '" & FixQuotes(dr1("pikategori")) & "' as pikategori, piidbarang as piidbarang, pistokminimal as pistokminimal, pistokmaksimal as pistokmaksimal, pistokreorder as pistokreorder, pistokminorder as pistokminorder, pihargajual1 as pihargajual1, pihargajual2 as pihargajual2, pihargajual3 as pihargajual3, pihargajual4 as pihargajual4, pihargajual5 as pihargajual5, pidiskonjual1 as pidiskonjual1, pidiskonjual2 as pidiskonjual2, pidiskonjual3 as pidiskonjual3, pidiskonjual4 as pidiskonjual4, pidiskonjual5 as pidiskonjual5, picustomtext1 as picustomtext1, picustomtext2 as picustomtext2, picustomtext3 as picustomtext3, picustomtext4 as picustomtext4, picustomtext5 as picustomtext5, picustomint1 as picustomint1, picustomint2 as picustomint2, picustomint3 as picustomint3, picustomdbl1 as picustomdbl1, picustomdbl2 as picustomdbl2, picustomdbl3 as picustomdbl3, picustomdate1 as picustomdate1, picustomdate2 as picustomdate2, picustomdate3 as picustomdate3, pidownloaded as pidownloaded, pihargaedited as pihargaedited FROM m_12_pos_item where pikategori = '" & FixQuotes(dr1("pikategorilain")) & "')  ON DUPLICATE KEY UPDATE pistokminimal = VALUES(pistokminimal), pistokmaksimal = VALUES(pistokmaksimal), pistokreorder = VALUES(pistokreorder), pistokminorder = VALUES(pistokminorder), pihargajual1 = VALUES(pihargajual1), pihargajual2 = VALUES(pihargajual2), pihargajual3 = VALUES(pihargajual3), pihargajual4 = VALUES(pihargajual4), pihargajual5 = VALUES(pihargajual5), pidiskonjual1 = VALUES(pidiskonjual1), pidiskonjual2 = VALUES(pidiskonjual2), pidiskonjual3 = VALUES(pidiskonjual3), pidiskonjual4 = VALUES(pidiskonjual4), pidiskonjual5 = VALUES(pidiskonjual5), picustomtext1 = VALUES(picustomtext1), picustomtext2 = VALUES(picustomtext2), picustomtext3 = VALUES(picustomtext3), picustomtext4 = VALUES(picustomtext4), picustomtext5 = VALUES(picustomtext5), picustomint1 = VALUES(picustomint1), picustomint2 = VALUES(picustomint2), picustomint3 = VALUES(picustomint3), picustomdbl1 = VALUES(picustomdbl1), picustomdbl2 = VALUES(picustomdbl2), picustomdbl3 = VALUES(picustomdbl3), picustomdate1 = VALUES(picustomdate1), picustomdate2 = VALUES(picustomdate2), picustomdate3 = VALUES(picustomdate3), pidownloaded = VALUES(pidownloaded)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Next

            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)
            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_ItemDeletePerKategoriOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
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
        'pikategori(0) As String, pikategoribarang(1) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'pikategori, pikategoribarang

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pikategoribarang", AsEnumTypeData.AsString)


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


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 2) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------


            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If

            'pikategoribarang(1) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - pikategoribarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - pikategoribarang should not be more than 25 character." : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pikategori~pikategoribarang", dataRowDetail(0) & "~" & dataRowDetail(1)) = False Then
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
            'Proses detail
            If (dtdetail.Rows.Count > 0) Then
                For Each dr1 As DataRow In dtdetail.Rows
                    'AMBIL KATEGORI POS TRANSAKSI UNTUK FILTER USER LOGIN
                    sql = "SELECT pikategori as kategoripos FROM m_12_pos_item pi WHERE pi.pikategori = '" & FixQuotes(dr1("pikategori")) & "' GROUP BY pikategori"
                    Dim dtKategoriPOS As DataTable = AsDataTableAmbilDariDB(sql)
                    If dtKategoriPOS.Rows.Count > 0 Then
                        For Each drKategoriPOS As DataRow In dtKategoriPOS.Rows
                            'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                            ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                            ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drKategoriPOS("kategoripos")) & "' "
                        Next
                    End If

                    sql = "DELETE pi FROM m_12_pos_item pi JOIN m1_item i ON pi.piidbarang = i.bid WHERE pi.pikategori = '" & FixQuotes(dr1("pikategori")) & "' AND i.bkategori = '" & FixQuotes(dr1("pikategoribarang")) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Next

            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_ItemDeleteSemuaOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
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
        'pikategori(0) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'pikategori

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)


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


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 1) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------


            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pikategori", dataRowDetail(0)) = False Then
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
            'Proses detail
            If (dtdetail.Rows.Count > 0) Then
                For Each dr1 As DataRow In dtdetail.Rows
                    'AMBIL KATEGORI POS TRANSAKSI UNTUK FILTER USER LOGIN
                    sql = "SELECT pikategori as kategoripos FROM m_12_pos_item pi WHERE pi.pikategori = '" & FixQuotes(dr1("pikategori")) & "' GROUP BY pikategori"
                    Dim dtKategoriPOS As DataTable = AsDataTableAmbilDariDB(sql)
                    If dtKategoriPOS.Rows.Count > 0 Then
                        For Each drKategoriPOS As DataRow In dtKategoriPOS.Rows
                            'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                            ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                            ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drKategoriPOS("kategoripos")) & "' "
                        Next
                    End If

                    sql = "DELETE pi FROM m_12_pos_item pi WHERE pi.pikategori = '" & FixQuotes(dr1("pikategori")) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Next

            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_ItemDeleteKelasProdukOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
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

        'Kategori POS'
        Dim KategoriPOS As String
        KategoriPOS = paramSplit(5)

        'MAPPING BUAT WS ----------------------------------------------------------
        'pikategori(0) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'pikategori

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)


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


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 1) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------


            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pikategori", dataRowDetail(0)) = False Then
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
            'Proses detail
            If (dtdetail.Rows.Count > 0) Then
                For Each dr1 As DataRow In dtdetail.Rows
                    'AMBIL KATEGORI POS TRANSAKSI UNTUK FILTER USER LOGIN
                    sql = "SELECT pikategori as kategoripos FROM m_12_pos_item pi WHERE pi.pikategori = '" & FixQuotes(dr1("pikategori")) & "' GROUP BY pikategori"
                    Dim dtKategoriPOS As DataTable = AsDataTableAmbilDariDB(sql)
                    If dtKategoriPOS.Rows.Count > 0 Then
                        For Each drKategoriPOS As DataRow In dtKategoriPOS.Rows
                            'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                            ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                            ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drKategoriPOS("kategoripos")) & "' "
                        Next
                    End If

                    sql = "DELETE pi FROM m_12_pos_item pi JOIN m1_item i ON pi.piidbarang = i.bid JOIN m_12_pos_category pc ON pi.pikategori = pc.pckode AND pi.pikategori = '" & FixQuotes(dr1("pikategori")) & "' JOIN m_12_pos_type pt ON pc.pctipepos = pt.ptkode JOIN m_12_pos_type_class_product ptcp ON pt.ptkode = ptcp.tipepos AND i.bkelasproduk = ptcp.kelasproduk"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Next

            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_ItemSetHargaIndeksOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
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
        'pikategori(0) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'pikategori

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)


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


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 1) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------


            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pikategori", dataRowDetail(0)) = False Then
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
            'Proses detail
            If (dtdetail.Rows.Count > 0) Then
                For Each dr1 As DataRow In dtdetail.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(dr1("pikategori")) & "' "
                    'sql = "UPDATE m1_item i JOIN m_12_pos_item pi ON i.bid = pi.piidbarang JOIN m_12_pos_category pc ON pi.pikategori = pc.pckode AND pc.pckode = '" & FixQuotes(dr1("pikategori")) & "' JOIN m1_index_price ip ON pc.pcindeksharga = ip.ipkode SET pi.pihargajual1 = ROUND((CASE WHEN ip.ipmargin = 0 THEN i.bhppaverage ELSE i.bhppaverage + ((ip.ipmargin / 100) * i.bhppaverage) END),2)"
                    sql = "UPDATE m1_item i JOIN m_12_pos_item pi ON i.bid = pi.piidbarang JOIN m_12_pos_category pc ON pi.pikategori = pc.pckode AND pc.pckode = '" & FixQuotes(dr1("pikategori")) & "' JOIN m1_index_price ip ON pc.pcindeksharga = ip.ipkode SET pi.pihargajual1 = ROUND((CASE WHEN ip.ipmargin = 0 THEN i.bhargabeli ELSE i.bhargabeli + ((ip.ipmargin / 100) * i.bhargabeli) END),2), pihargaedited = 1 WHERE pihargaedited = '0'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Next

            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_ItemCekKelasProdukOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
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
        'pikategori(0) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'pikategori

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)


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


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 1) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------


            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pikategori", dataRowDetail(0)) = False Then
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
            'Proses detail
            If (dtdetail.Rows.Count > 0) Then
                For Each dr1 As DataRow In dtdetail.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(dr1("pikategori")) & "' "
                    'select '
                    sql = "Insert into M_12_Pos_Item(SELECT '" & FixQuotes(dr1("pikategori")) & "' as pikategori, bid as piidbarang, bstokminimal as pistokminimal, bstokmaksimal as pistokmaksimal, breorder as pistokreorder, bminorder as pistokminorder, bhargajual1 as pihargajual1, bhargajual2 as pihargajual2, bhargajual3 as pihargajual3, bhargajual4 as pihargajual4, bhargajual5 as pihargajual5, bdiskonjual1 as pidiskonjual1, bdiskonjual2 as pidiskonjual2, bdiskonjual3 as pidiskonjual3, bdiskonjual4 as pidiskonjual4, bdiskonjual5 as pidiskonjual5, '' as picustomtext1, '' as picustomtext2, '' as picustomtext3, '' as picustomtext4, '' as picustomtext5, 0 as picustomint1, 0 as picustomint2, 0 as picustomint3, 0 as picustomdbl1, 0 as picustomdbl2, 0 as picustomdbl3, '1900-01-01' as picustomdate1, '1900-01-01' as picustomdate2, '1900-01-01' as picustomdate3, 0 as pidownloaded FROM m1_item i JOIN m_12_pos_category pc ON pc.pckode = '" & FixQuotes(dr1("pikategori")) & "' JOIN m_12_pos_type pt ON pc.pctipepos = pt.ptkode JOIN m_12_pos_type_class_product ptcp ON pt.ptkode = ptcp.tipepos AND i.bkelasproduk = ptcp.kelasproduk GROUP BY i.bid) ON DUPLICATE KEY UPDATE pistokminimal = VALUES(pistokminimal), pistokmaksimal = VALUES(pistokmaksimal), pistokreorder = VALUES(pistokreorder), pistokminorder = VALUES(pistokminorder), pihargajual1 = VALUES(pihargajual1), pihargajual2 = VALUES(pihargajual2), pihargajual3 = VALUES(pihargajual3), pihargajual4 = VALUES(pihargajual4), pihargajual5 = VALUES(pihargajual5), pidiskonjual1 = VALUES(pidiskonjual1), pidiskonjual2 = VALUES(pidiskonjual2), pidiskonjual3 = VALUES(pidiskonjual3), pidiskonjual4 = VALUES(pidiskonjual4), pidiskonjual5 = VALUES(pidiskonjual5), picustomtext1 = VALUES(picustomtext1), picustomtext2 = VALUES(picustomtext2), picustomtext3 = VALUES(picustomtext3), picustomtext4 = VALUES(picustomtext4), picustomtext5 = VALUES(picustomtext5), picustomint1 = VALUES(picustomint1), picustomint2 = VALUES(picustomint2), picustomint3 = VALUES(picustomint3), picustomdbl1 = VALUES(picustomdbl1), picustomdbl2 = VALUES(picustomdbl2), picustomdbl3 = VALUES(picustomdbl3), picustomdate1 = VALUES(picustomdate1), picustomdate2 = VALUES(picustomdate2), picustomdate3 = VALUES(picustomdate3), pidownloaded = VALUES(pidownloaded)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Next

            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_ItemSearch(ByVal param As String) As String
        'M12_Pos_ItemSearch --------------------------------------------------------
        'pikategori, piidbarang, pistokminimal, pistokmaksimal, pistokreorder, pihargajual1, pihargajual2, 
        'pihargajual3, pihargajual4, pihargajual5, pidiskonjual1, pidiskonjual2, pidiskonjual3, pidiskonjual4, 
        'pidiskonjual5, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, 
        'picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, 
        'picustomdate3, pcnama, bkode, bnama, btipe, bsatuan, pistokminorder

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
        sql = "select `pi`.`pikategori` AS `pikategori`,`pi`.`piidbarang` AS `piidbarang`,`pi`.`pistokminimal` AS `pistokminimal`,`pi`.`pistokmaksimal` AS `pistokmaksimal`,`pi`.`pistokreorder` AS `pistokreorder`,`pi`.`pihargajual1` AS `pihargajual1`,`pi`.`pihargajual2` AS `pihargajual2`,`pi`.`pihargajual3` AS `pihargajual3`,`pi`.`pihargajual4` AS `pihargajual4`,`pi`.`pihargajual5` AS `pihargajual5`,`pi`.`pidiskonjual1` AS `pidiskonjual1`,`pi`.`pidiskonjual2` AS `pidiskonjual2`,`pi`.`pidiskonjual3` AS `pidiskonjual3`,`pi`.`pidiskonjual4` AS `pidiskonjual4`,`pi`.`pidiskonjual5` AS `pidiskonjual5`,`pi`.`picustomtext1` AS `picustomtext1`,`pi`.`picustomtext2` AS `picustomtext2`,`pi`.`picustomtext3` AS `picustomtext3`,`pi`.`picustomtext4` AS `picustomtext4`,`pi`.`picustomtext5` AS `picustomtext5`,`pi`.`picustomint1` AS `picustomint1`,`pi`.`picustomint2` AS `picustomint2`,`pi`.`picustomint3` AS `picustomint3`,`pi`.`picustomdbl1` AS `picustomdbl1`,`pi`.`picustomdbl2` AS `picustomdbl2`,`pi`.`picustomdbl3` AS `picustomdbl3`,`pi`.`picustomdate1` AS `picustomdate1`,`pi`.`picustomdate2` AS `picustomdate2`,`pi`.`picustomdate3` AS `picustomdate3`,`pc`.`pcnama` AS `pcnama`,`i`.`bkode` AS `bkode`,`i`.`bnama` AS `bnama`,`i`.`btipe` AS `btipe`,`i`.`bsatuan` AS `bsatuan`, pi.pistokminorder, pi.pihargaedited from ((`m_12_pos_item` `pi` join `m_12_pos_category` `pc` on((`pi`.`pikategori` = `pc`.`pckode`))) join `m1_item` `i` on((`pi`.`piidbarang` = `i`.`bid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M_12_Pos_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("pikategori"), ""), sptField,
                     FxDB(dr("piidbarang"), ""), sptField,
                     FxDB(dr("pistokminimal"), 0), sptField,
                     FxDB(dr("pistokmaksimal"), 0), sptField,
                     FxDB(dr("pistokreorder"), 0), sptField,
                     FxDB(dr("pihargajual1"), 0), sptField,
                     FxDB(dr("pihargajual2"), 0), sptField,
                     FxDB(dr("pihargajual3"), 0), sptField,
                     FxDB(dr("pihargajual4"), 0), sptField,
                     FxDB(dr("pihargajual5"), 0), sptField,
                     FxDB(dr("pidiskonjual1"), ""), sptField,
                     FxDB(dr("pidiskonjual2"), ""), sptField,
                     FxDB(dr("pidiskonjual3"), ""), sptField,
                     FxDB(dr("pidiskonjual4"), ""), sptField,
                     FxDB(dr("pidiskonjual5"), ""), sptField,
                     FxDB(dr("picustomtext1"), ""), sptField,
                     FxDB(dr("picustomtext2"), ""), sptField,
                     FxDB(dr("picustomtext3"), ""), sptField,
                     FxDB(dr("picustomtext4"), ""), sptField,
                     FxDB(dr("picustomtext5"), ""), sptField,
                     FxDB(dr("picustomint1"), 0), sptField,
                     FxDB(dr("picustomint2"), 0), sptField,
                     FxDB(dr("picustomint3"), 0), sptField,
                     FxDB(dr("picustomdbl1"), 0), sptField,
                     FxDB(dr("picustomdbl2"), 0), sptField,
                     FxDB(dr("picustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("picustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("picustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("picustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("pcnama"), ""), sptField,
                     FxDB(dr("bkode"), ""), sptField,
                     FxDB(dr("bnama"), ""), sptField,
                     FxDB(dr("btipe"), ""), sptField,
                     FxDB(dr("bsatuan"), ""), sptField,
                     FxDB(dr("pistokminorder"), 0), sptField,
                     FxDB(dr("pihargaedited"), 0), sptRow) 'tambahan harga sudah pernah diedit atau belum'

            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "POS Item data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        'wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pikategori, piidbarang, pistokminimal, pistokmaksimal, pistokreorder, pihargajual1, pihargajual2, pihargajual3, pihargajual4, pihargajual5, pidiskonjual1, pidiskonjual2, pidiskonjual3, pidiskonjual4, pidiskonjual5, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3, pcnama, bkode, bnama, btipe, bsatuan, pistokminorder"))
        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pikategori, piidbarang, pistokminimal, pistokmaksimal, pistokreorder, pihargajual1, pihargajual2, pihargajual3, pihargajual4, pihargajual5, pidiskonjual1, pidiskonjual2, pidiskonjual3, pidiskonjual4, pidiskonjual5, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3, pcnama, bkode, bnama, btipe, bsatuan, pistokminorder, pihargaedited"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_ItemDownload(ByVal param As String) As String
        'M12_Pos_ItemDownload --------------------------------------------------------
        'pikategori, piidbarang, pistokminimal, pistokmaksimal, pistokreorder, pihargajual1, pihargajual2, 
        'pihargajual3, pihargajual4, pihargajual5, pidiskonjual1, pidiskonjual2, pidiskonjual3, pidiskonjual4, 
        'pidiskonjual5, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, 
        'picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, 
        'picustomdate3

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

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M_12_Pos_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("pikategori"), ""), sptField,
                     FxDB(dr("piidbarang"), ""), sptField,
                     FxDB(dr("pistokminimal"), 0), sptField,
                     FxDB(dr("pistokmaksimal"), 0), sptField,
                     FxDB(dr("pistokreorder"), 0), sptField,
                     FxDB(dr("pihargajual1"), 0), sptField,
                     FxDB(dr("pihargajual2"), 0), sptField,
                     FxDB(dr("pihargajual3"), 0), sptField,
                     FxDB(dr("pihargajual4"), 0), sptField,
                     FxDB(dr("pihargajual5"), 0), sptField,
                     FxDB(dr("pidiskonjual1"), ""), sptField,
                     FxDB(dr("pidiskonjual2"), ""), sptField,
                     FxDB(dr("pidiskonjual3"), ""), sptField,
                     FxDB(dr("pidiskonjual4"), ""), sptField,
                     FxDB(dr("pidiskonjual5"), ""), sptField,
                     FxDB(dr("picustomtext1"), ""), sptField,
                     FxDB(dr("picustomtext2"), ""), sptField,
                     FxDB(dr("picustomtext3"), ""), sptField,
                     FxDB(dr("picustomtext4"), ""), sptField,
                     FxDB(dr("picustomtext5"), ""), sptField,
                     FxDB(dr("picustomint1"), 0), sptField,
                     FxDB(dr("picustomint2"), 0), sptField,
                     FxDB(dr("picustomint3"), 0), sptField,
                     FxDB(dr("picustomdbl1"), 0), sptField,
                     FxDB(dr("picustomdbl2"), 0), sptField,
                     FxDB(dr("picustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("picustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("picustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("picustomdate3"), ""), formatTgl), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "POS Item data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pikategori, piidbarang, pistokminimal, pistokmaksimal, pistokreorder, pihargajual1, pihargajual2, pihargajual3, pihargajual4, pihargajual5, pidiskonjual1, pidiskonjual2, pidiskonjual3, pidiskonjual4, pidiskonjual5, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_ItemTerkait(ByVal param As String) As String

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
        Dim kategoriPOS As String = ""
        Dim idbarang As String = ""
        'If (IsNumeric(paramSplit(5)) = False) Then
        '    result(2) = "bid required numeric." : GoTo selesai
        'Else
        'SET IDTRANSAKSI
        Dim splitID As Array
        splitID = paramSplit(5).Split(sptSubParam)
        kategoriPOS = splitID(0)
        idbarang = splitID(1)
        'End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        sql = "SELECT `pi`.`piidbarang` AS `piidbarang` FROM (m_12_pos_bonus_item `pbi` JOIN m_12_pos_item `pi` ON ((pbi.biidbarang = `pi`.`piidbarang`))) WHERE (`pi`.`pikategori` = 'valkode' AND `pi`.`piidbarang` = 'fidbarang') UNION ALL SELECT `pbid`.`idbarang` AS `piidbarang` FROM (m_12_pos_bonus_item_detail `pbid` JOIN m_12_pos_bonus_item `pbi` ON ((pbid.idbi = `pbi`.`biid`))) WHERE (`pbi`.`bikategori` = 'valkode' AND `pbid`.`idbarang` = 'fidbarang') UNION ALL SELECT `pi`.`piidbarang` AS `piidbarang` FROM (m_12_pos_additional_item `pai` JOIN m_12_pos_item `pi` ON ((pai.aiidbarang = `pi`.`piidbarang`))) WHERE (`pi`.`pikategori` = 'valkode' AND `pi`.`piidbarang` = 'fidbarang') UNION ALL SELECT `paid`.`idbarang` AS `piidbarang` FROM (m_12_pos_additional_item_detail `paid` JOIN m_12_pos_additional_item `pai` ON ((paid.idai = `pai`.`aiid`))) WHERE (`pai`.`aikategori` = 'valkode' AND `paid`.`idbarang` = 'fidbarang') UNION ALL SELECT `pi`.`piidbarang` AS `piidbarang` FROM (m_12_pos_additional_item `pai` JOIN m_12_pos_item `pi` ON ((pai.aiidbarang = `pi`.`piidbarang`))) WHERE (`pi`.`pikategori` = 'valkode' AND `pi`.`piidbarang` = 'fidbarang') UNION ALL SELECT `paid`.`idbarang` AS `piidbarang` FROM (m_12_pos_additional_item_detail `paid` JOIN m_12_pos_additional_item `pai` ON ((paid.idai = `pai`.`aiid`))) WHERE (`pai`.`aikategori` = 'valkode' AND `paid`.`idbarang` = 'fidbarang') UNION ALL SELECT `pi`.`piidbarang` AS `piidbarang` FROM (m_12_pos_substitution_item `psi` JOIN m_12_pos_item `pi` ON ((psi.siidbarang = `pi`.`piidbarang`))) WHERE (`pi`.`pikategori` = 'valkode' AND `pi`.`piidbarang` = 'fidbarang') UNION ALL SELECT `psid`.`idbarang` AS `piidbarang` FROM (m_12_pos_substitution_item_detail `psid` JOIN m_12_pos_substitution_item `psi` ON ((psid.idsi = `psi`.`siid`))) WHERE (`psi`.`sikategori` = 'valkode' AND `psid`.`idbarang` = 'fidbarang') UNION ALL SELECT `pi`.`piidbarang` AS `piidbarang` FROM (m_12_pos_discount_item `pdi` JOIN m_12_pos_item `pi` ON ((pdi.diidbarang = `pi`.`piidbarang`))) WHERE (`pi`.`pikategori` = 'valkode' AND `pi`.`piidbarang` = 'fidbarang') UNION ALL SELECT `ppi`.`piidbarang` AS `piidbarang` FROM (m_12_pos_point_item `ppi` JOIN m_12_pos_item `pi` ON ((ppi.piidbarang = `pi`.`piidbarang`))) WHERE (`ppi`.`pikategori` = 'valkode' AND `pi`.`piidbarang` = 'fidbarang')"
        sql = sql.Replace("valkode", kategoriPOS)
        sql = sql.Replace("fidbarang", idbarang)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("piidbarang"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Related Item data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("piidbarang"))

        Return wsResult
    End Function

End Class
