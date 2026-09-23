Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_pos_discount_category_item
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_Pos_Discount_Category_ItemSimpan(ByVal param As String) As String
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
        'dcikategori(0) As String, dcikategoribarang(1) As String, dcioperator(2) As String, dcijml1(3) As Double, dcijml2(4) As Double, 
        'dcikriteria(5) As Integer, dcinilai(6) As String, dcitgl1(7) As Date, dcitgl2(8) As Date, dcijam1(9) As Date, 
        'dcijam2(10) As Date, dcicustomtext1(11) As String, dcicustomtext2(12) As String, dcicustomtext3(13) As String, dcicustomtext4(14) As String, 
        'dcicustomtext5(15) As String, dcicustomint1(16) As Integer, dcicustomint2(17) As Integer, dcicustomint3(18) As Integer, dcicustomdbl1(19) As Double, 
        'dcicustomdbl2(20) As Double, dcicustomdbl3(21) As Double, dcicustomdate1(22) As Date, dcicustomdate2(23) As Date, dcicustomdate3(24) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'dcikategori, dcikategoribarang, dcioperator, dcijml1, dcijml2, dcikriteria, dcinilai, 
        'dcitgl1, dcitgl2, dcijam1, dcijam2, dcicustomtext1, dcicustomtext2, dcicustomtext3, 
        'dcicustomtext4, dcicustomtext5, dcicustomint1, dcicustomint2, dcicustomint3, dcicustomdbl1, dcicustomdbl2, 
        'dcicustomdbl3, dcicustomdate1, dcicustomdate2, dcicustomdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "dcikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcikategoribarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcioperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcijml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcijml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcikriteria", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dcinilai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcitgl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcitgl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcijam1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcijam2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dcicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dcicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dcicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomdate3", AsEnumTypeData.AsString)

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
            If (dataRowDetail.Length <> 25) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'dcijml1(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - dcijml1 required numeric." : GoTo selesai
            End If
            'dcijml2(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - dcijml2 required numeric." : GoTo selesai
            End If
            'dcikriteria(5) As Integer
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - dcikriteria required numeric." : GoTo selesai
            ElseIf dataRowDetail(5) <> 0 And dataRowDetail(5) <> 1 And dataRowDetail(5) <> 2 Then
                result(2) = "Row : " & i & " - invalid dcikriteria value" : GoTo selesai
            End If
            'dcitgl1(7) As Date
            If (IsDate(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - dcitgl1 required date." : GoTo selesai
            End If
            'dcitgl2(8) As Date
            If (IsDate(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - dcitgl2 required date." : GoTo selesai
            End If

            'dcijam1(9) As Date
            If (IsDate(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - dcijam1 required time format (H:mm:ss)." : GoTo selesai
            End If

            'dcijam2(10) As Date
            If (IsDate(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - dcijam2 required time format (H:mm:ss)." : GoTo selesai
            End If

            'dcicustomint1(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - dcicustomint1 required numeric." : GoTo selesai
            End If
            'dcicustomint2(17) As Integer
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - dcicustomint2 required numeric." : GoTo selesai
            End If
            'dcicustomint3(18) As Integer
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - dcicustomint3 required numeric." : GoTo selesai
            End If
            'dcicustomdbl1(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - dcicustomdbl1 required numeric." : GoTo selesai
            End If
            'dcicustomdbl2(20) As Double
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - dcicustomdbl2 required numeric." : GoTo selesai
            End If
            'dcicustomdbl3(21) As Double
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - dcicustomdbl3 required numeric." : GoTo selesai
            End If
            'dcicustomdate1(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - dcicustomdate1 required date." : GoTo selesai
            End If
            'dcicustomdate2(23) As Date
            If (IsDate(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - dcicustomdate2 required date." : GoTo selesai
            End If
            'dcicustomdate3(24) As Date
            If (IsDate(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - dcicustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'dcikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - dcikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - dcikategori should not be more than 25 character." : GoTo selesai
            End If

            'dcikategoribarang(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - dcikategoribarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - dcikategoribarang should not be more than 25 character." : GoTo selesai
            End If

            'dcioperator(2) As String
            If IsNumeric(dataRowDetail(2)) = False Then
                result(2) = "Row : " & i & " - dcioperator required numeric" : GoTo selesai
            ElseIf dataRowDetail(2) <> 0 And dataRowDetail(2) <> 1 And dataRowDetail(2) <> 2 Then
                result(2) = "Row : " & i & " - invalid dcioperator value" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - dcioperator should not be more than 25 character." : GoTo selesai
            End If

            'dcijml1(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - dcijml1 can't be empty" : GoTo selesai
            End If

            'dcijml2(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - dcijml2 can't be empty" : GoTo selesai
            End If

            'dcinilai(6) As String
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - dcinilai can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(6)) > 25 Then
                result(2) = "Row : " & i & " - dcinilai should not be more than 25 character." : GoTo selesai
            End If

            'dcitgl1(7) As Date
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - dcitgl1 can't be empty" : GoTo selesai
            End If

            'dcitgl2(8) As Date
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - dcitgl2 can't be empty" : GoTo selesai
            End If

            'dcijam1(9) As Date
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - dcijam1 can't be empty" : GoTo selesai
            End If

            'dcijam2(10) As Date
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - dcijam2 can't be empty" : GoTo selesai
            End If

            'dcicustomdbl1(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - dcicustomdbl1 can't be empty" : GoTo selesai
            End If

            'dcicustomdbl2(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - dcicustomdbl2 can't be empty" : GoTo selesai
            End If

            'dcicustomdbl3(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - dcicustomdbl3 can't be empty" : GoTo selesai
            End If

            'dcicustomdate1(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - dcicustomdate1 can't be empty" : GoTo selesai
            End If

            'dcicustomdate2(23) As Date
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - dcicustomdate2 can't be empty" : GoTo selesai
            End If

            'dcicustomdate3(24) As Date
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - dcicustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "dcikategori~dcikategoribarang~dcioperator~dcijml1~dcijml2~dcikriteria~dcinilai~dcitgl1~dcitgl2~dcijam1~dcijam2~dcicustomtext1~dcicustomtext2~dcicustomtext3~dcicustomtext4~dcicustomtext5~dcicustomint1~dcicustomint2~dcicustomint3~dcicustomdbl1~dcicustomdbl2~dcicustomdbl3~dcicustomdate1~dcicustomdate2~dcicustomdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24)) = False Then
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
                Dim drutama As DataRow = dtdetail.Rows(0)

                'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drutama("dcikategori")) & "' "

                'HAPUS DATA KATEGORI DAN BARANG YANG SAMA
                sql = "DELETE FROM M_12_Pos_Discount_Category_Item WHERE dcikategori = '" & FixQuotes(drutama("dcikategori")) & "' AND dcikategoribarang = '" & FixQuotes(drutama("dcikategoribarang")) & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'INSERT
                Dim strValue2 As New StringBuilder
                Dim dtOperator As New DataTable
                Dim vOperator As String = ""
                For Each dr1 As DataRow In dtdetail.Rows
                    'CEK OPERATOR :
                    'JIKA BETWEEN (0) ATAU >= (1) MAKA BOLEH LEBIH DARI SATU KONDISI 
                    '=> BEBERAPA KONDISI BETWEEN DAN SATU KONDISI >= (1)
                    'JIKA KELIPATAN (2) MAKA HANYA BOLEH SATU KONDISI
                    sql = "SELECT dci.dcikategori as kategori, dci.dcikategoribarang as kategoribarang, dci.dcioperator as operator, ic.icnama, (CASE dci.dcioperator WHEN 0 THEN 'Between' WHEN 1 THEN '>=' WHEN 2 THEN 'Multiple' ELSE 'Unknown' END) as operatornama FROM M_12_Pos_Discount_Category_Item dci JOIN m1_item_category ic ON dci.dcikategoribarang = ic.ickode WHERE dci.dcikategori = '" & FxDB(dr1("dcikategori"), "") & "' AND dci.dcikategoribarang = '" & FxDB(dr1("dcikategoribarang"), "") & "' GROUP BY dci.dcioperator ORDER BY dci.dcioperator"
                    dtOperator = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtOperator.Rows.Count > 0 Then
                        For Each dr2 As DataRow In dtOperator.Rows
                            vOperator = FxDB(dr2("operator").ToString, "")
                            If Len(vOperator) > 0 Then
                                If vOperator = 2 Then
                                    'JIKA SUDAH TERDAPAT OPERATOR KELIPATAN (2)
                                    result(2) = "Item Category : " & FxDB(dr2("icnama"), "") & " - already has '" & FxDB(dr2("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                Else
                                    'JIKA BELUM TERDAPAT OPERATOR KELIPATAN (2), CEK KONDISI OPERATOR YANG SUDAH DIINPUTKAN
                                    'JIKA OPERATOR YANG DIINPUTKAN ADALAH KELIPATAN (2) MAKA TAMPILKAN ERRMESSAGE
                                    'JIKA SUDAH TERDAPAT OPERATOR >= (1) DAN YANG DIINPUTKAN ADALAH OPERATOR >= (1) LAGI MAKA TAMPILKAN ERRMESSAGE
                                    If dr1("dcioperator") = 2 Or (vOperator = 1 And dr1("dcioperator") = vOperator) Then
                                        result(2) = "Item Category : " & FxDB(dr2("icnama"), "") & " - already has '" & FxDB(dr2("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                    End If
                                End If
                            End If
                        Next
                    End If

                    'strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Clear()
                    strValue2.Append("('" & FixQuotes(dr1("dcikategori")) & "', '" & FixQuotes(dr1("dcikategoribarang")) & "', '" & FixQuotes(dr1("dcioperator")) & "', '" & FixDouble(dr1("dcijml1")) & "', '" & FixDouble(dr1("dcijml2")) & "', " & dr1("dcikriteria") & ", '" & FixQuotes(dr1("dcinilai")) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcitgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcitgl2"))) & "', '" & FixQuotes(dr1("dcijam1")) & "', '" & FixQuotes(dr1("dcijam2")) & "', '" & FixQuotes(dr1("dcicustomtext1")) & "', '" & FixQuotes(dr1("dcicustomtext2")) & "', '" & FixQuotes(dr1("dcicustomtext3")) & "', '" & FixQuotes(dr1("dcicustomtext4")) & "', '" & FixQuotes(dr1("dcicustomtext5")) & "', " & dr1("dcicustomint1") & ", " & dr1("dcicustomint2") & ", " & dr1("dcicustomint3") & ", '" & FixDouble(dr1("dcicustomdbl1")) & "', '" & FixDouble(dr1("dcicustomdbl2")) & "', '" & FixDouble(dr1("dcicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcicustomdate3"))) & "')")

                    sql = "Insert into M_12_Pos_Discount_Category_Item(dcikategori, dcikategoribarang, dcioperator, dcijml1, dcijml2, dcikriteria, dcinilai, dcitgl1, dcitgl2, dcijam1, dcijam2, dcicustomtext1, dcicustomtext2, dcicustomtext3, dcicustomtext4, dcicustomtext5, dcicustomint1, dcicustomint2, dcicustomint3, dcicustomdbl1, dcicustomdbl2, dcicustomdbl3, dcicustomdate1, dcicustomdate2, dcicustomdate3) values" & strValue2.ToString & ""
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
            Dim paramSearch As String = M12_Pos_Discount_Category_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Discount_Category_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Discount_Category_ItemDelete(ByVal param As String) As String

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
        Dim dcikategori As String = "", dcikategoribarang As String = "", dcioperator As String = "", dcijml1 As String = "", dcijml2 As String = ""
        Dim idtrans(3) As String
        idtrans = paramSplit(5).Split(sptSubParam)
        If (idtrans.Length <> 5) Then
            result(2) = "Invalid primary key parameter." : GoTo selesai
        Else
            'CEK dcikategori
            If (Len(idtrans(0)) = 0) Then
                result(2) = "dcikategori can't be empty." : GoTo selesai
            Else
                dcikategori = idtrans(0)
            End If
            'CEK dcikategoribarang
            If (Len(idtrans(1)) = 0) Then
                result(2) = "dcikategoribarang can't be empty." : GoTo selesai
            Else
                dcikategoribarang = idtrans(1)
            End If
            'CEK dcioperator
            If (Len(idtrans(2)) = 0) Then
                result(2) = "dcioperator can't be empty." : GoTo selesai
            Else
                dcioperator = idtrans(2)
            End If
            'CEK dcijml1
            If (IsNumeric(idtrans(3)) = False) Then
                result(2) = "dcijml1 required numeric." : GoTo selesai
            Else
                dcijml1 = idtrans(3)
            End If
            'CEK dcijml2
            If (IsNumeric(idtrans(4)) = False) Then
                result(2) = "dcijml2 required numeric." : GoTo selesai
            Else
                dcijml2 = idtrans(4)
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
            sql = "SELECT dcikategori as kategoripos FROM M_12_Pos_Discount_Category_Item WHERE dcikategori = '" & dcikategori & "' AND dcikategoribarang = '" & dcikategoribarang & "' AND dcioperator = '" & dcioperator & "' AND dcijml1 = '" & dcijml1 & "' AND dcijml2 = '" & dcijml2 & "' GROUP BY dcikategori"
            Dim dtKategoriPOS As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtKategoriPOS.Rows.Count > 0 Then
                For Each drKategoriPOS As DataRow In dtKategoriPOS.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drKategoriPOS("kategoripos")) & "' "
                Next
            End If

            'DELETE
            sql = "DELETE FROM M_12_Pos_Discount_Category_Item WHERE dcikategori = '" & dcikategori & "' AND dcikategoribarang = '" & dcikategoribarang & "' AND dcioperator = '" & dcioperator & "' AND dcijml1 = '" & dcijml1 & "' AND dcijml2 = '" & dcijml2 & "'"
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
            Dim paramSearch As String = M12_Pos_Discount_Category_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Discount_Category_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Discount_Category_ItemImport(ByVal param As String) As String
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
        'dcikategori(0) As String, dcikategoribarang(1) As String, dcioperator(2) As String, dcijml1(3) As Double, dcijml2(4) As Double, 
        'dcikriteria(5) As Integer, dcinilai(6) As String, dcitgl1(7) As Date, dcitgl2(8) As Date, dcijam1(9) As Date, 
        'dcijam2(10) As Date, dcicustomtext1(11) As String, dcicustomtext2(12) As String, dcicustomtext3(13) As String, dcicustomtext4(14) As String, 
        'dcicustomtext5(15) As String, dcicustomint1(16) As Integer, dcicustomint2(17) As Integer, dcicustomint3(18) As Integer, dcicustomdbl1(19) As Double, 
        'dcicustomdbl2(20) As Double, dcicustomdbl3(21) As Double, dcicustomdate1(22) As Date, dcicustomdate2(23) As Date, dcicustomdate3(24) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'dcikategori, dcikategoribarang, dcioperator, dcijml1, dcijml2, dcikriteria, dcinilai, 
        'dcitgl1, dcitgl2, dcijam1, dcijam2, dcicustomtext1, dcicustomtext2, dcicustomtext3, 
        'dcicustomtext4, dcicustomtext5, dcicustomint1, dcicustomint2, dcicustomint3, dcicustomdbl1, dcicustomdbl2, 
        'dcicustomdbl3, dcicustomdate1, dcicustomdate2, dcicustomdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "dcikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcikategoribarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcioperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcijml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcijml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcikriteria", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dcinilai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcitgl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcitgl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcijam1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcijam2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dcicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dcicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dcicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomdate3", AsEnumTypeData.AsString)

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
            If (dataRowDetail.Length <> 25) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'dcijml1(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - dcijml1 required numeric." : GoTo selesai
            End If
            'dcijml2(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - dcijml2 required numeric." : GoTo selesai
            End If
            'dcikriteria(5) As Integer
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - dcikriteria required numeric." : GoTo selesai
            ElseIf dataRowDetail(5) <> 0 And dataRowDetail(5) <> 1 And dataRowDetail(5) <> 2 Then
                result(2) = "Row : " & i & " - invalid dcikriteria value" : GoTo selesai
            End If
            'dcitgl1(7) As Date
            If (IsDate(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - dcitgl1 required date." : GoTo selesai
            End If
            'dcitgl2(8) As Date
            If (IsDate(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - dcitgl2 required date." : GoTo selesai
            End If
            'dcijam1(9) As Date
            If (IsDate(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - dcijam1 required date." : GoTo selesai
            End If
            'dcijam2(10) As Date
            If (IsDate(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - dcijam2 required date." : GoTo selesai
            End If
            'dcicustomint1(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - dcicustomint1 required numeric." : GoTo selesai
            End If
            'dcicustomint2(17) As Integer
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - dcicustomint2 required numeric." : GoTo selesai
            End If
            'dcicustomint3(18) As Integer
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - dcicustomint3 required numeric." : GoTo selesai
            End If
            'dcicustomdbl1(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - dcicustomdbl1 required numeric." : GoTo selesai
            End If
            'dcicustomdbl2(20) As Double
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - dcicustomdbl2 required numeric." : GoTo selesai
            End If
            'dcicustomdbl3(21) As Double
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - dcicustomdbl3 required numeric." : GoTo selesai
            End If
            'dcicustomdate1(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - dcicustomdate1 required date." : GoTo selesai
            End If
            'dcicustomdate2(23) As Date
            If (IsDate(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - dcicustomdate2 required date." : GoTo selesai
            End If
            'dcicustomdate3(24) As Date
            If (IsDate(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - dcicustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'dcikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - dcikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - dcikategori should not be more than 25 character." : GoTo selesai
            End If

            'dcikategoribarang(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - dcikategoribarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - dcikategoribarang should not be more than 25 character." : GoTo selesai
            End If

            'dcioperator(2) As String
            If IsNumeric(dataRowDetail(2)) = False Then
                result(2) = "Row : " & i & " - dcioperator required numeric" : GoTo selesai
            ElseIf dataRowDetail(2) <> 0 And dataRowDetail(2) <> 1 And dataRowDetail(2) <> 2 Then
                result(2) = "Row : " & i & " - invalid dcioperator value" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - dcioperator should not be more than 25 character." : GoTo selesai
            End If

            'dcijml1(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - dcijml1 can't be empty" : GoTo selesai
            End If

            'dcijml2(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - dcijml2 can't be empty" : GoTo selesai
            End If

            'dcinilai(6) As String
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - dcinilai can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(6)) > 25 Then
                result(2) = "Row : " & i & " - dcinilai should not be more than 25 character." : GoTo selesai
            End If

            'dcitgl1(7) As Date
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - dcitgl1 can't be empty" : GoTo selesai
            End If

            'dcitgl2(8) As Date
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - dcitgl2 can't be empty" : GoTo selesai
            End If

            'dcijam1(9) As Date
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - dcijam1 can't be empty" : GoTo selesai
            End If

            'dcijam2(10) As Date
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - dcijam2 can't be empty" : GoTo selesai
            End If

            'dcicustomdbl1(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - dcicustomdbl1 can't be empty" : GoTo selesai
            End If

            'dcicustomdbl2(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - dcicustomdbl2 can't be empty" : GoTo selesai
            End If

            'dcicustomdbl3(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - dcicustomdbl3 can't be empty" : GoTo selesai
            End If

            'dcicustomdate1(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - dcicustomdate1 can't be empty" : GoTo selesai
            End If

            'dcicustomdate2(23) As Date
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - dcicustomdate2 can't be empty" : GoTo selesai
            End If

            'dcicustomdate3(24) As Date
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - dcicustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "dcikategori~dcikategoribarang~dcioperator~dcijml1~dcijml2~dcikriteria~dcinilai~dcitgl1~dcitgl2~dcijam1~dcijam2~dcicustomtext1~dcicustomtext2~dcicustomtext3~dcicustomtext4~dcicustomtext5~dcicustomint1~dcicustomint2~dcicustomint3~dcicustomdbl1~dcicustomdbl2~dcicustomdbl3~dcicustomdate1~dcicustomdate2~dcicustomdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24)) = False Then
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
                    strValue2.Append("('" & FixQuotes(dr1("dcikategori")) & "', '" & FixQuotes(dr1("dcikategoribarang")) & "', '" & FixQuotes(dr1("dcioperator")) & "', '" & FixDouble(dr1("dcijml1")) & "', '" & FixDouble(dr1("dcijml2")) & "', " & dr1("dcikriteria") & ", '" & FixQuotes(dr1("dcinilai")) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcitgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcitgl2"))) & "', '" & FixQuotes(dr1("dcijam1")) & "', '" & FixQuotes(dr1("dcijam2")) & "', '" & FixQuotes(dr1("dcicustomtext1")) & "', '" & FixQuotes(dr1("dcicustomtext2")) & "', '" & FixQuotes(dr1("dcicustomtext3")) & "', '" & FixQuotes(dr1("dcicustomtext4")) & "', '" & FixQuotes(dr1("dcicustomtext5")) & "', " & dr1("dcicustomint1") & ", " & dr1("dcicustomint2") & ", " & dr1("dcicustomint3") & ", '" & FixDouble(dr1("dcicustomdbl1")) & "', '" & FixDouble(dr1("dcicustomdbl2")) & "', '" & FixDouble(dr1("dcicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcicustomdate3"))) & "')")
                Next

                If Len(strValue2.ToString) > 0 Then
                    'DELETE
                    sql = "DELETE FROM M_12_Pos_Discount_Category_Item"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'insert jika data belum ada, dan update jika data sudah ada
                    sql = "Insert into M_12_Pos_Discount_Category_Item(dcikategori, dcikategoribarang, dcioperator, dcijml1, dcijml2, dcikriteria, dcinilai, dcitgl1, dcitgl2, dcijam1, dcijam2, dcicustomtext1, dcicustomtext2, dcicustomtext3, dcicustomtext4, dcicustomtext5, dcicustomint1, dcicustomint2, dcicustomint3, dcicustomdbl1, dcicustomdbl2, dcicustomdbl3, dcicustomdate1, dcicustomdate2, dcicustomdate3) values" & strValue2.ToString & ""
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
            Dim paramSearch As String = M12_Pos_Discount_Category_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Discount_Category_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Discount_Category_ItemSimpanOld(ByVal param As String) As String
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
        'dcikategori(0) As String, dcikategoribarang(1) As String, dcioperator(2) As String, dcijml1(3) As Double, dcijml2(4) As Double, 
        'dcikriteria(5) As Integer, dcinilai(6) As String, dcitgl1(7) As Date, dcitgl2(8) As Date, dcijam1(9) As Date, 
        'dcijam2(10) As Date, dcicustomtext1(11) As String, dcicustomtext2(12) As String, dcicustomtext3(13) As String, dcicustomtext4(14) As String, 
        'dcicustomtext5(15) As String, dcicustomint1(16) As Integer, dcicustomint2(17) As Integer, dcicustomint3(18) As Integer, dcicustomdbl1(19) As Double, 
        'dcicustomdbl2(20) As Double, dcicustomdbl3(21) As Double, dcicustomdate1(22) As Date, dcicustomdate2(23) As Date, dcicustomdate3(24) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'dcikategori, dcikategoribarang, dcioperator, dcijml1, dcijml2, dcikriteria, dcinilai, 
        'dcitgl1, dcitgl2, dcijam1, dcijam2, dcicustomtext1, dcicustomtext2, dcicustomtext3, 
        'dcicustomtext4, dcicustomtext5, dcicustomint1, dcicustomint2, dcicustomint3, dcicustomdbl1, dcicustomdbl2, 
        'dcicustomdbl3, dcicustomdate1, dcicustomdate2, dcicustomdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "dcikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcikategoribarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcioperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcijml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcijml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcikriteria", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dcinilai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcitgl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcitgl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcijam1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcijam2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dcicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dcicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dcicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomdate3", AsEnumTypeData.AsString)

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
            If (dataRowDetail.Length <> 25) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'dcijml1(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - dcijml1 required numeric." : GoTo selesai
            End If
            'dcijml2(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - dcijml2 required numeric." : GoTo selesai
            End If
            'dcikriteria(5) As Integer
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - dcikriteria required numeric." : GoTo selesai
            ElseIf dataRowDetail(5) <> 0 And dataRowDetail(5) <> 1 And dataRowDetail(5) <> 2 Then
                result(2) = "Row : " & i & " - invalid dcikriteria value" : GoTo selesai
            End If
            'dcitgl1(7) As Date
            If (IsDate(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - dcitgl1 required date." : GoTo selesai
            End If
            'dcitgl2(8) As Date
            If (IsDate(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - dcitgl2 required date." : GoTo selesai
            End If

            'dcijam1(9) As Date
            If (IsDate(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - dcijam1 required time format (H:mm:ss)." : GoTo selesai
            End If

            'dcijam2(10) As Date
            If (IsDate(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - dcijam2 required time format (H:mm:ss)." : GoTo selesai
            End If

            'dcicustomint1(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - dcicustomint1 required numeric." : GoTo selesai
            End If
            'dcicustomint2(17) As Integer
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - dcicustomint2 required numeric." : GoTo selesai
            End If
            'dcicustomint3(18) As Integer
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - dcicustomint3 required numeric." : GoTo selesai
            End If
            'dcicustomdbl1(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - dcicustomdbl1 required numeric." : GoTo selesai
            End If
            'dcicustomdbl2(20) As Double
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - dcicustomdbl2 required numeric." : GoTo selesai
            End If
            'dcicustomdbl3(21) As Double
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - dcicustomdbl3 required numeric." : GoTo selesai
            End If
            'dcicustomdate1(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - dcicustomdate1 required date." : GoTo selesai
            End If
            'dcicustomdate2(23) As Date
            If (IsDate(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - dcicustomdate2 required date." : GoTo selesai
            End If
            'dcicustomdate3(24) As Date
            If (IsDate(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - dcicustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'dcikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - dcikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - dcikategori should not be more than 25 character." : GoTo selesai
            End If

            'dcikategoribarang(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - dcikategoribarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - dcikategoribarang should not be more than 25 character." : GoTo selesai
            End If

            'dcioperator(2) As String
            If IsNumeric(dataRowDetail(2)) = False Then
                result(2) = "Row : " & i & " - dcioperator required numeric" : GoTo selesai
            ElseIf dataRowDetail(2) <> 0 And dataRowDetail(2) <> 1 And dataRowDetail(2) <> 2 Then
                result(2) = "Row : " & i & " - invalid dcioperator value" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - dcioperator should not be more than 25 character." : GoTo selesai
            End If

            'dcijml1(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - dcijml1 can't be empty" : GoTo selesai
            End If

            'dcijml2(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - dcijml2 can't be empty" : GoTo selesai
            End If

            'dcinilai(6) As String
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - dcinilai can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(6)) > 25 Then
                result(2) = "Row : " & i & " - dcinilai should not be more than 25 character." : GoTo selesai
            End If

            'dcitgl1(7) As Date
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - dcitgl1 can't be empty" : GoTo selesai
            End If

            'dcitgl2(8) As Date
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - dcitgl2 can't be empty" : GoTo selesai
            End If

            'dcijam1(9) As Date
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - dcijam1 can't be empty" : GoTo selesai
            End If

            'dcijam2(10) As Date
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - dcijam2 can't be empty" : GoTo selesai
            End If

            'dcicustomdbl1(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - dcicustomdbl1 can't be empty" : GoTo selesai
            End If

            'dcicustomdbl2(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - dcicustomdbl2 can't be empty" : GoTo selesai
            End If

            'dcicustomdbl3(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - dcicustomdbl3 can't be empty" : GoTo selesai
            End If

            'dcicustomdate1(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - dcicustomdate1 can't be empty" : GoTo selesai
            End If

            'dcicustomdate2(23) As Date
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - dcicustomdate2 can't be empty" : GoTo selesai
            End If

            'dcicustomdate3(24) As Date
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - dcicustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "dcikategori~dcikategoribarang~dcioperator~dcijml1~dcijml2~dcikriteria~dcinilai~dcitgl1~dcitgl2~dcijam1~dcijam2~dcicustomtext1~dcicustomtext2~dcicustomtext3~dcicustomtext4~dcicustomtext5~dcicustomint1~dcicustomint2~dcicustomint3~dcicustomdbl1~dcicustomdbl2~dcicustomdbl3~dcicustomdate1~dcicustomdate2~dcicustomdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24)) = False Then
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
                Dim drutama As DataRow = dtdetail.Rows(0)

                'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drutama("dcikategori")) & "' "

                'HAPUS DATA KATEGORI DAN BARANG YANG SAMA
                sql = "DELETE FROM M_12_Pos_Discount_Category_Item WHERE dcikategori = '" & FixQuotes(drutama("dcikategori")) & "' AND dcikategoribarang = '" & FixQuotes(drutama("dcikategoribarang")) & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'INSERT
                Dim strValue2 As New StringBuilder
                Dim dtOperator As New DataTable
                Dim vOperator As String = ""
                For Each dr1 As DataRow In dtdetail.Rows
                    'CEK OPERATOR :
                    'JIKA BETWEEN (0) ATAU >= (1) MAKA BOLEH LEBIH DARI SATU KONDISI 
                    '=> BEBERAPA KONDISI BETWEEN DAN SATU KONDISI >= (1)
                    'JIKA KELIPATAN (2) MAKA HANYA BOLEH SATU KONDISI
                    sql = "SELECT dci.dcikategori as kategori, dci.dcikategoribarang as kategoribarang, dci.dcioperator as operator, ic.icnama, (CASE dci.dcioperator WHEN 0 THEN 'Between' WHEN 1 THEN '>=' WHEN 2 THEN 'Multiple' ELSE 'Unknown' END) as operatornama FROM M_12_Pos_Discount_Category_Item dci JOIN m1_item_category ic ON dci.dcikategoribarang = ic.ickode WHERE dci.dcikategori = '" & FxDB(dr1("dcikategori"), "") & "' AND dci.dcikategoribarang = '" & FxDB(dr1("dcikategoribarang"), "") & "' GROUP BY dci.dcioperator ORDER BY dci.dcioperator"
                    dtOperator = AsDataTableAmbilDariDB(sql)
                    If dtOperator.Rows.Count > 0 Then
                        For Each dr2 As DataRow In dtOperator.Rows
                            vOperator = FxDB(dr2("operator").ToString, "")
                            If Len(vOperator) > 0 Then
                                If vOperator = 2 Then
                                    'JIKA SUDAH TERDAPAT OPERATOR KELIPATAN (2)
                                    result(2) = "Item Category : " & FxDB(dr2("icnama"), "") & " - already has '" & FxDB(dr2("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                Else
                                    'JIKA BELUM TERDAPAT OPERATOR KELIPATAN (2), CEK KONDISI OPERATOR YANG SUDAH DIINPUTKAN
                                    'JIKA OPERATOR YANG DIINPUTKAN ADALAH KELIPATAN (2) MAKA TAMPILKAN ERRMESSAGE
                                    'JIKA SUDAH TERDAPAT OPERATOR >= (1) DAN YANG DIINPUTKAN ADALAH OPERATOR >= (1) LAGI MAKA TAMPILKAN ERRMESSAGE
                                    If dr1("dcioperator") = 2 Or (vOperator = 1 And dr1("dcioperator") = vOperator) Then
                                        result(2) = "Item Category : " & FxDB(dr2("icnama"), "") & " - already has '" & FxDB(dr2("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                    End If
                                End If
                            End If
                        Next
                    End If

                    'strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Clear()
                    strValue2.Append("('" & FixQuotes(dr1("dcikategori")) & "', '" & FixQuotes(dr1("dcikategoribarang")) & "', '" & FixQuotes(dr1("dcioperator")) & "', '" & FixDouble(dr1("dcijml1")) & "', '" & FixDouble(dr1("dcijml2")) & "', " & dr1("dcikriteria") & ", '" & FixQuotes(dr1("dcinilai")) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcitgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcitgl2"))) & "', '" & FixQuotes(dr1("dcijam1")) & "', '" & FixQuotes(dr1("dcijam2")) & "', '" & FixQuotes(dr1("dcicustomtext1")) & "', '" & FixQuotes(dr1("dcicustomtext2")) & "', '" & FixQuotes(dr1("dcicustomtext3")) & "', '" & FixQuotes(dr1("dcicustomtext4")) & "', '" & FixQuotes(dr1("dcicustomtext5")) & "', " & dr1("dcicustomint1") & ", " & dr1("dcicustomint2") & ", " & dr1("dcicustomint3") & ", '" & FixDouble(dr1("dcicustomdbl1")) & "', '" & FixDouble(dr1("dcicustomdbl2")) & "', '" & FixDouble(dr1("dcicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcicustomdate3"))) & "')")

                    sql = "Insert into M_12_Pos_Discount_Category_Item(dcikategori, dcikategoribarang, dcioperator, dcijml1, dcijml2, dcikriteria, dcinilai, dcitgl1, dcitgl2, dcijam1, dcijam2, dcicustomtext1, dcicustomtext2, dcicustomtext3, dcicustomtext4, dcicustomtext5, dcicustomint1, dcicustomint2, dcicustomint3, dcicustomdbl1, dcicustomdbl2, dcicustomdbl3, dcicustomdate1, dcicustomdate2, dcicustomdate3) values" & strValue2.ToString & ""
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
            Dim paramSearch As String = M12_Pos_Discount_Category_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Discount_Category_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Discount_Category_ItemDeleteOld(ByVal param As String) As String

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
        Dim dcikategori As String = "", dcikategoribarang As String = "", dcioperator As String = "", dcijml1 As String = "", dcijml2 As String = ""
        Dim idtrans(3) As String
        idtrans = paramSplit(5).Split(sptSubParam)
        If (idtrans.Length <> 5) Then
            result(2) = "Invalid primary key parameter." : GoTo selesai
        Else
            'CEK dcikategori
            If (Len(idtrans(0)) = 0) Then
                result(2) = "dcikategori can't be empty." : GoTo selesai
            Else
                dcikategori = idtrans(0)
            End If
            'CEK dcikategoribarang
            If (Len(idtrans(1)) = 0) Then
                result(2) = "dcikategoribarang can't be empty." : GoTo selesai
            Else
                dcikategoribarang = idtrans(1)
            End If
            'CEK dcioperator
            If (Len(idtrans(2)) = 0) Then
                result(2) = "dcioperator can't be empty." : GoTo selesai
            Else
                dcioperator = idtrans(2)
            End If
            'CEK dcijml1
            If (IsNumeric(idtrans(3)) = False) Then
                result(2) = "dcijml1 required numeric." : GoTo selesai
            Else
                dcijml1 = idtrans(3)
            End If
            'CEK dcijml2
            If (IsNumeric(idtrans(4)) = False) Then
                result(2) = "dcijml2 required numeric." : GoTo selesai
            Else
                dcijml2 = idtrans(4)
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
            sql = "SELECT dcikategori as kategoripos FROM M_12_Pos_Discount_Category_Item WHERE dcikategori = '" & dcikategori & "' AND dcikategoribarang = '" & dcikategoribarang & "' AND dcioperator = '" & dcioperator & "' AND dcijml1 = '" & dcijml1 & "' AND dcijml2 = '" & dcijml2 & "' GROUP BY dcikategori"
            Dim dtKategoriPOS As DataTable = AsDataTableAmbilDariDB(sql)
            If dtKategoriPOS.Rows.Count > 0 Then
                For Each drKategoriPOS As DataRow In dtKategoriPOS.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drKategoriPOS("kategoripos")) & "' "
                Next
            End If

            'DELETE
            sql = "DELETE FROM M_12_Pos_Discount_Category_Item WHERE dcikategori = '" & dcikategori & "' AND dcikategoribarang = '" & dcikategoribarang & "' AND dcioperator = '" & dcioperator & "' AND dcijml1 = '" & dcijml1 & "' AND dcijml2 = '" & dcijml2 & "'"
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
            Dim paramSearch As String = M12_Pos_Discount_Category_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Discount_Category_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Discount_Category_ItemImportOld(ByVal param As String) As String
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
        'dcikategori(0) As String, dcikategoribarang(1) As String, dcioperator(2) As String, dcijml1(3) As Double, dcijml2(4) As Double, 
        'dcikriteria(5) As Integer, dcinilai(6) As String, dcitgl1(7) As Date, dcitgl2(8) As Date, dcijam1(9) As Date, 
        'dcijam2(10) As Date, dcicustomtext1(11) As String, dcicustomtext2(12) As String, dcicustomtext3(13) As String, dcicustomtext4(14) As String, 
        'dcicustomtext5(15) As String, dcicustomint1(16) As Integer, dcicustomint2(17) As Integer, dcicustomint3(18) As Integer, dcicustomdbl1(19) As Double, 
        'dcicustomdbl2(20) As Double, dcicustomdbl3(21) As Double, dcicustomdate1(22) As Date, dcicustomdate2(23) As Date, dcicustomdate3(24) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'dcikategori, dcikategoribarang, dcioperator, dcijml1, dcijml2, dcikriteria, dcinilai, 
        'dcitgl1, dcitgl2, dcijam1, dcijam2, dcicustomtext1, dcicustomtext2, dcicustomtext3, 
        'dcicustomtext4, dcicustomtext5, dcicustomint1, dcicustomint2, dcicustomint3, dcicustomdbl1, dcicustomdbl2, 
        'dcicustomdbl3, dcicustomdate1, dcicustomdate2, dcicustomdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "dcikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcikategoribarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcioperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcijml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcijml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcikriteria", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dcinilai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcitgl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcitgl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcijam1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcijam2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dcicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dcicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dcicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcicustomdate3", AsEnumTypeData.AsString)

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
            If (dataRowDetail.Length <> 25) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'dcijml1(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - dcijml1 required numeric." : GoTo selesai
            End If
            'dcijml2(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - dcijml2 required numeric." : GoTo selesai
            End If
            'dcikriteria(5) As Integer
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - dcikriteria required numeric." : GoTo selesai
            ElseIf dataRowDetail(5) <> 0 And dataRowDetail(5) <> 1 And dataRowDetail(5) <> 2 Then
                result(2) = "Row : " & i & " - invalid dcikriteria value" : GoTo selesai
            End If
            'dcitgl1(7) As Date
            If (IsDate(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - dcitgl1 required date." : GoTo selesai
            End If
            'dcitgl2(8) As Date
            If (IsDate(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - dcitgl2 required date." : GoTo selesai
            End If
            'dcijam1(9) As Date
            If (IsDate(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - dcijam1 required date." : GoTo selesai
            End If
            'dcijam2(10) As Date
            If (IsDate(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - dcijam2 required date." : GoTo selesai
            End If
            'dcicustomint1(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - dcicustomint1 required numeric." : GoTo selesai
            End If
            'dcicustomint2(17) As Integer
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - dcicustomint2 required numeric." : GoTo selesai
            End If
            'dcicustomint3(18) As Integer
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - dcicustomint3 required numeric." : GoTo selesai
            End If
            'dcicustomdbl1(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - dcicustomdbl1 required numeric." : GoTo selesai
            End If
            'dcicustomdbl2(20) As Double
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - dcicustomdbl2 required numeric." : GoTo selesai
            End If
            'dcicustomdbl3(21) As Double
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - dcicustomdbl3 required numeric." : GoTo selesai
            End If
            'dcicustomdate1(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - dcicustomdate1 required date." : GoTo selesai
            End If
            'dcicustomdate2(23) As Date
            If (IsDate(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - dcicustomdate2 required date." : GoTo selesai
            End If
            'dcicustomdate3(24) As Date
            If (IsDate(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - dcicustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'dcikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - dcikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - dcikategori should not be more than 25 character." : GoTo selesai
            End If

            'dcikategoribarang(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - dcikategoribarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - dcikategoribarang should not be more than 25 character." : GoTo selesai
            End If

            'dcioperator(2) As String
            If IsNumeric(dataRowDetail(2)) = False Then
                result(2) = "Row : " & i & " - dcioperator required numeric" : GoTo selesai
            ElseIf dataRowDetail(2) <> 0 And dataRowDetail(2) <> 1 And dataRowDetail(2) <> 2 Then
                result(2) = "Row : " & i & " - invalid dcioperator value" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - dcioperator should not be more than 25 character." : GoTo selesai
            End If

            'dcijml1(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - dcijml1 can't be empty" : GoTo selesai
            End If

            'dcijml2(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - dcijml2 can't be empty" : GoTo selesai
            End If

            'dcinilai(6) As String
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - dcinilai can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(6)) > 25 Then
                result(2) = "Row : " & i & " - dcinilai should not be more than 25 character." : GoTo selesai
            End If

            'dcitgl1(7) As Date
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - dcitgl1 can't be empty" : GoTo selesai
            End If

            'dcitgl2(8) As Date
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - dcitgl2 can't be empty" : GoTo selesai
            End If

            'dcijam1(9) As Date
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - dcijam1 can't be empty" : GoTo selesai
            End If

            'dcijam2(10) As Date
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - dcijam2 can't be empty" : GoTo selesai
            End If

            'dcicustomdbl1(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - dcicustomdbl1 can't be empty" : GoTo selesai
            End If

            'dcicustomdbl2(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - dcicustomdbl2 can't be empty" : GoTo selesai
            End If

            'dcicustomdbl3(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - dcicustomdbl3 can't be empty" : GoTo selesai
            End If

            'dcicustomdate1(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - dcicustomdate1 can't be empty" : GoTo selesai
            End If

            'dcicustomdate2(23) As Date
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - dcicustomdate2 can't be empty" : GoTo selesai
            End If

            'dcicustomdate3(24) As Date
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - dcicustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "dcikategori~dcikategoribarang~dcioperator~dcijml1~dcijml2~dcikriteria~dcinilai~dcitgl1~dcitgl2~dcijam1~dcijam2~dcicustomtext1~dcicustomtext2~dcicustomtext3~dcicustomtext4~dcicustomtext5~dcicustomint1~dcicustomint2~dcicustomint3~dcicustomdbl1~dcicustomdbl2~dcicustomdbl3~dcicustomdate1~dcicustomdate2~dcicustomdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24)) = False Then
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
                    strValue2.Append("('" & FixQuotes(dr1("dcikategori")) & "', '" & FixQuotes(dr1("dcikategoribarang")) & "', '" & FixQuotes(dr1("dcioperator")) & "', '" & FixDouble(dr1("dcijml1")) & "', '" & FixDouble(dr1("dcijml2")) & "', " & dr1("dcikriteria") & ", '" & FixQuotes(dr1("dcinilai")) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcitgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcitgl2"))) & "', '" & FixQuotes(dr1("dcijam1")) & "', '" & FixQuotes(dr1("dcijam2")) & "', '" & FixQuotes(dr1("dcicustomtext1")) & "', '" & FixQuotes(dr1("dcicustomtext2")) & "', '" & FixQuotes(dr1("dcicustomtext3")) & "', '" & FixQuotes(dr1("dcicustomtext4")) & "', '" & FixQuotes(dr1("dcicustomtext5")) & "', " & dr1("dcicustomint1") & ", " & dr1("dcicustomint2") & ", " & dr1("dcicustomint3") & ", '" & FixDouble(dr1("dcicustomdbl1")) & "', '" & FixDouble(dr1("dcicustomdbl2")) & "', '" & FixDouble(dr1("dcicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcicustomdate3"))) & "')")
                Next

                If Len(strValue2.ToString) > 0 Then
                    'DELETE
                    sql = "DELETE FROM M_12_Pos_Discount_Category_Item"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'insert jika data belum ada, dan update jika data sudah ada
                    sql = "Insert into M_12_Pos_Discount_Category_Item(dcikategori, dcikategoribarang, dcioperator, dcijml1, dcijml2, dcikriteria, dcinilai, dcitgl1, dcitgl2, dcijam1, dcijam2, dcicustomtext1, dcicustomtext2, dcicustomtext3, dcicustomtext4, dcicustomtext5, dcicustomint1, dcicustomint2, dcicustomint3, dcicustomdbl1, dcicustomdbl2, dcicustomdbl3, dcicustomdate1, dcicustomdate2, dcicustomdate3) values" & strValue2.ToString & ""
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
            Dim paramSearch As String = M12_Pos_Discount_Category_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Discount_Category_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Discount_Category_ItemSearch(ByVal param As String) As String
        'JIKA MENAMBAHKAN FIELD DISINI MAKA HARUS TAMBAHKAN JUGA LENGTH ARRAY PADA VALIDASI AMBIL USERID WS M0_LOGIN
        'M12_Pos_Discount_Category_ItemSearch --------------------------------------------------------
        'dcikategori, dcikategoribarang, dcioperator, dcijml1, dcijml2, dcikriteria, dcinilai, 
        'dcitgl1, dcitgl2, dcijam1, dcijam2, dcicustomtext1, dcicustomtext2, dcicustomtext3, 
        'dcicustomtext4, dcicustomtext5, dcicustomint1, dcicustomint2, dcicustomint3, dcicustomdbl1, dcicustomdbl2, 
        'dcicustomdbl3, dcicustomdate1, dcicustomdate2, dcicustomdate3, pcnama, icnama, dcikriterianama, dcioperatornama

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = "", SFilterSplit() As String = {}, SFilter As String = ""

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
        sql = "select `dci`.`dcikategori` AS `dcikategori`,`dci`.`dcikategoribarang` AS `dcikategoribarang`,`dci`.`dcioperator` AS `dcioperator`,`dci`.`dcijml1` AS `dcijml1`,`dci`.`dcijml2` AS `dcijml2`,`dci`.`dcikriteria` AS `dcikriteria`,`dci`.`dcinilai` AS `dcinilai`,`dci`.`dcitgl1` AS `dcitgl1`,`dci`.`dcitgl2` AS `dcitgl2`,`dci`.`dcijam1` AS `dcijam1`,`dci`.`dcijam2` AS `dcijam2`,`dci`.`dcicustomtext1` AS `dcicustomtext1`,`dci`.`dcicustomtext2` AS `dcicustomtext2`,`dci`.`dcicustomtext3` AS `dcicustomtext3`,`dci`.`dcicustomtext4` AS `dcicustomtext4`,`dci`.`dcicustomtext5` AS `dcicustomtext5`,`dci`.`dcicustomint1` AS `dcicustomint1`,`dci`.`dcicustomint2` AS `dcicustomint2`,`dci`.`dcicustomint3` AS `dcicustomint3`,`dci`.`dcicustomdbl1` AS `dcicustomdbl1`,`dci`.`dcicustomdbl2` AS `dcicustomdbl2`,`dci`.`dcicustomdbl3` AS `dcicustomdbl3`,`dci`.`dcicustomdate1` AS `dcicustomdate1`,`dci`.`dcicustomdate2` AS `dcicustomdate2`,`dci`.`dcicustomdate3` AS `dcicustomdate3`,`pc`.`pcnama` AS `pcnama`,`ic`.`icnama` AS `icnama`,(case `dci`.`dcikriteria` when 0 then 'Price' when 1 then 'Discount Percent' when 2 then 'Discount Nominal' else 'Unknown' end) AS `dcikriterianama`,(case `dci`.`dcioperator` when 0 then 'Between' when 1 then '>=' when 2 then 'Multiple' else 'Unknown' end) AS `dcioperatornama` from ((`M_12_Pos_Discount_Category_Item` `dci` join `m_12_pos_category` `pc` on((`dci`.`dcikategori` = `pc`.`pckode`))) join `m1_item_category` `ic` on((`dci`.`dcikategoribarang` = `ic`.`ickode`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M_12_Pos_Discount_Category_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("dcikategori"), ""), sptField,
                     FxDB(dr("dcikategoribarang"), ""), sptField,
                     FxDB(dr("dcioperator"), ""), sptField,
                     FxDB(dr("dcijml1"), 0), sptField,
                     FxDB(dr("dcijml2"), 0), sptField,
                     FxDB(dr("dcikriteria"), 0), sptField,
                     FxDB(dr("dcinilai"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dcitgl1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("dcitgl2"), ""), formatTgl), sptField,
                     FxDB(dr("dcijam1").ToString, ""), sptField,
                     FxDB(dr("dcijam2").ToString, ""), sptField,
                     FxDB(dr("dcicustomtext1"), ""), sptField,
                     FxDB(dr("dcicustomtext2"), ""), sptField,
                     FxDB(dr("dcicustomtext3"), ""), sptField,
                     FxDB(dr("dcicustomtext4"), ""), sptField,
                     FxDB(dr("dcicustomtext5"), ""), sptField,
                     FxDB(dr("dcicustomint1"), 0), sptField,
                     FxDB(dr("dcicustomint2"), 0), sptField,
                     FxDB(dr("dcicustomint3"), 0), sptField,
                     FxDB(dr("dcicustomdbl1"), 0), sptField,
                     FxDB(dr("dcicustomdbl2"), 0), sptField,
                     FxDB(dr("dcicustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("dcicustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("dcicustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("dcicustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("pcnama"), ""), sptField,
                     FxDB(dr("icnama"), ""), sptField,
                     FxDB(dr("dcikriterianama"), ""), sptField,
                     FxDB(dr("dcioperatornama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Discount Category Item data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("dcikategori, dcikategoribarang, dcioperator, dcijml1, dcijml2, dcikriteria, dcinilai, dcitgl1, dcitgl2, dcijam1, dcijam2, dcicustomtext1, dcicustomtext2, dcicustomtext3, dcicustomtext4, dcicustomtext5, dcicustomint1, dcicustomint2, dcicustomint3, dcicustomdbl1, dcicustomdbl2, dcicustomdbl3, dcicustomdate1, dcicustomdate2, dcicustomdate3, pcnama, icnama, dcikriterianama, dcioperatornama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_Discount_Category_ItemDownload(ByVal param As String) As String
        'M12_Pos_Discount_Category_ItemDownload --------------------------------------------------------
        'dcikategori, dcikategoribarang, dcioperator, dcijml1, dcijml2, dcikriteria, dcinilai, 
        'dcitgl1, dcitgl2, dcijam1, dcijam2, dcicustomtext1, dcicustomtext2, dcicustomtext3, 
        'dcicustomtext4, dcicustomtext5, dcicustomint1, dcicustomint2, dcicustomint3, dcicustomdbl1, dcicustomdbl2, 
        'dcicustomdbl3, dcicustomdate1, dcicustomdate2, dcicustomdate3

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = "", SFilterSplit() As String = {}, SFilter As String = ""

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

        dt = AmbilData("aplikasi1-M_12_Pos_Discount_Category_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("dcikategori"), ""), sptField,
                     FxDB(dr("dcikategoribarang"), ""), sptField,
                     FxDB(dr("dcioperator"), ""), sptField,
                     FxDB(dr("dcijml1"), 0), sptField,
                     FxDB(dr("dcijml2"), 0), sptField,
                     FxDB(dr("dcikriteria"), 0), sptField,
                     FxDB(dr("dcinilai"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dcitgl1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("dcitgl2"), ""), formatTgl), sptField,
                     FxDB(dr("dcijam1"), ""), sptField,
                     FxDB(dr("dcijam2"), ""), sptField,
                     FxDB(dr("dcicustomtext1"), ""), sptField,
                     FxDB(dr("dcicustomtext2"), ""), sptField,
                     FxDB(dr("dcicustomtext3"), ""), sptField,
                     FxDB(dr("dcicustomtext4"), ""), sptField,
                     FxDB(dr("dcicustomtext5"), ""), sptField,
                     FxDB(dr("dcicustomint1"), 0), sptField,
                     FxDB(dr("dcicustomint2"), 0), sptField,
                     FxDB(dr("dcicustomint3"), 0), sptField,
                     FxDB(dr("dcicustomdbl1"), 0), sptField,
                     FxDB(dr("dcicustomdbl2"), 0), sptField,
                     FxDB(dr("dcicustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("dcicustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("dcicustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("dcicustomdate3"), ""), formatTgl), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Discount Category Item data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("dcikategori, dcikategoribarang, dcioperator, dcijml1, dcijml2, dcikriteria, dcinilai, dcitgl1, dcitgl2, dcijam1, dcijam2, dcicustomtext1, dcicustomtext2, dcicustomtext3, dcicustomtext4, dcicustomtext5, dcicustomint1, dcicustomint2, dcicustomint3, dcicustomdbl1, dcicustomdbl2, dcicustomdbl3, dcicustomdate1, dcicustomdate2, dcicustomdate3"))

        Return wsResult
    End Function

End Class
