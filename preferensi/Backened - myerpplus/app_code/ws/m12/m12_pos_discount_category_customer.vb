Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_pos_discount_category_customer
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_Pos_Discount_Category_CustomerSimpan(ByVal param As String) As String
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
        'dcckategori(0) As String, dcckategoricustomer(1) As String, dccoperator(2) As String, dccjml1(3) As Double, dccjml2(4) As Double, 
        'dcckriteria(5) As Integer, dccnilai(6) As String, dcctgl1(7) As Date, dcctgl2(8) As Date, dccjam1(9) As Date, 
        'dccjam2(10) As Date, dcccustomtext1(11) As String, dcccustomtext2(12) As String, dcccustomtext3(13) As String, dcccustomtext4(14) As String, 
        'dcccustomtext5(15) As String, dcccustomint1(16) As Integer, dcccustomint2(17) As Integer, dcccustomint3(18) As Integer, dcccustomdbl1(19) As Double, 
        'dcccustomdbl2(20) As Double, dcccustomdbl3(21) As Double, dcccustomdate1(22) As Date, dcccustomdate2(23) As Date, dcccustomdate3(24) As Date,
        'dcccabang(25) As String, dccjeniskategori(26) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'dcckategori, dcckategoricustomer, dccoperator, dccjml1, dccjml2, dcckriteria, dccnilai, 
        'dcctgl1, dcctgl2, dccjam1, dccjam2, dcccustomtext1, dcccustomtext2, dcccustomtext3, 
        'dcccustomtext4, dcccustomtext5, dcccustomint1, dcccustomint2, dcccustomint3, dcccustomdbl1, dcccustomdbl2, 
        'dcccustomdbl3, dcccustomdate1, dcccustomdate2, dcccustomdate3,
        'dcccabang, dccjeniskategori

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "dcckategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcckategoricustomer", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dccoperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dccjml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dccjml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcckriteria", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dccnilai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcctgl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcctgl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dccjam1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dccjam2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcccustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcccustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcccustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcccustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcccustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcccustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dcccustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dcccustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dcccustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcccustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcccustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcccustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcccustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcccustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcccabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dccjeniskategori", AsEnumTypeData.AsInt64)

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
            If (dataRowDetail.Length <> 27) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'dccjml1(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - dccjml1 required numeric." : GoTo selesai
            End If
            'dccjml2(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - dccjml2 required numeric." : GoTo selesai
            End If
            'dcckriteria(5) As Integer
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - dcckriteria required numeric." : GoTo selesai
            ElseIf dataRowDetail(5) <> 0 And dataRowDetail(5) <> 1 And dataRowDetail(5) <> 2 Then
                result(2) = "Row : " & i & " - invalid dcckriteria value" : GoTo selesai
            End If
            'dcctgl1(7) As Date
            If (IsDate(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - dcctgl1 required date." : GoTo selesai
            End If
            'dcctgl2(8) As Date
            If (IsDate(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - dcctgl2 required date." : GoTo selesai
            End If

            'dccjam1(9) As Date
            If (IsDate(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - dccjam1 required time format (H:mm:ss)." : GoTo selesai
            End If

            'dccjam2(10) As Date
            If (IsDate(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - dccjam2 required time format (H:mm:ss)." : GoTo selesai
            End If

            'dcccustomint1(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - dcccustomint1 required numeric." : GoTo selesai
            End If
            'dcccustomint2(17) As Integer
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - dcccustomint2 required numeric." : GoTo selesai
            End If
            'dcccustomint3(18) As Integer
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - dcccustomint3 required numeric." : GoTo selesai
            End If
            'dcccustomdbl1(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - dcccustomdbl1 required numeric." : GoTo selesai
            End If
            'dcccustomdbl2(20) As Double
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - dcccustomdbl2 required numeric." : GoTo selesai
            End If
            'dcccustomdbl3(21) As Double
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - dcccustomdbl3 required numeric." : GoTo selesai
            End If
            'dcccustomdate1(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - dcccustomdate1 required date." : GoTo selesai
            End If
            'dcccustomdate2(23) As Date
            If (IsDate(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - dcccustomdate2 required date." : GoTo selesai
            End If
            'dcccustomdate3(24) As Date
            If (IsDate(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - dcccustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'dcckategori(0) As String
            'If Len(dataRowDetail(0)) = 0 Then
            '    result(2) = "Row : " & i & " - dcckategori can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - dcckategori should not be more than 25 character." : GoTo selesai
            End If

            'dcckategoricustomer(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - dcckategoricustomer can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - dcckategoricustomer should not be more than 25 character." : GoTo selesai
            End If

            'dccoperator(2) As String
            If IsNumeric(dataRowDetail(2)) = False Then
                result(2) = "Row : " & i & " - dccoperator required numeric" : GoTo selesai
            ElseIf dataRowDetail(2) <> 0 And dataRowDetail(2) <> 1 And dataRowDetail(2) <> 2 Then
                result(2) = "Row : " & i & " - invalid dccoperator value" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - dccoperator should not be more than 25 character." : GoTo selesai
            End If

            'dccjml1(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - dccjml1 can't be empty" : GoTo selesai
            End If

            'dccjml2(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - dccjml2 can't be empty" : GoTo selesai
            End If

            'dccnilai(6) As String
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - dccnilai can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(6)) > 25 Then
                result(2) = "Row : " & i & " - dccnilai should not be more than 25 character." : GoTo selesai
            End If

            'dcctgl1(7) As Date
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - dcctgl1 can't be empty" : GoTo selesai
            End If

            'dcctgl2(8) As Date
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - dcctgl2 can't be empty" : GoTo selesai
            End If

            'dccjam1(9) As Date
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - dccjam1 can't be empty" : GoTo selesai
            End If

            'dccjam2(10) As Date
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - dccjam2 can't be empty" : GoTo selesai
            End If

            'dcccustomdbl1(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - dcccustomdbl1 can't be empty" : GoTo selesai
            End If

            'dcccustomdbl2(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - dcccustomdbl2 can't be empty" : GoTo selesai
            End If

            'dcccustomdbl3(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - dcccustomdbl3 can't be empty" : GoTo selesai
            End If

            'dcccustomdate1(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - dcccustomdate1 can't be empty" : GoTo selesai
            End If

            'dcccustomdate2(23) As Date
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - dcccustomdate2 can't be empty" : GoTo selesai
            End If

            'dcccustomdate3(24) As Date
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - dcccustomdate3 can't be empty" : GoTo selesai
            End If

            'dcccabang(25) As String
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - dcccabang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(25)) > 25 Then
                result(2) = "Row : " & i & " - dcccabang should not be more than 25 character." : GoTo selesai
            End If

            'dccjeniskategori(26) As Double
            If (IsNumeric(dataRowDetail(26)) = False) Then
                result(2) = "Row : " & i & " - dccjeniskategori required numeric." : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "dcckategori~dcckategoricustomer~dccoperator~dccjml1~dccjml2~dcckriteria~dccnilai~dcctgl1~dcctgl2~dccjam1~dccjam2~dcccustomtext1~dcccustomtext2~dcccustomtext3~dcccustomtext4~dcccustomtext5~dcccustomint1~dcccustomint2~dcccustomint3~dcccustomdbl1~dcccustomdbl2~dcccustomdbl3~dcccustomdate1~dcccustomdate2~dcccustomdate3~dcccabang~dccjeniskategori", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26)) = False Then
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
                Dim ftDelKategori As String = "", ftInsKategori As String = ""

                If drutama("dccjeniskategori") = 1 Then 'JIKA PER KATEGORI
                    ftDelKategori = "dcckategori = '" & FixQuotes(drutama("dcckategori")) & "'"
                    ftInsKategori = "pckode = '" & FixQuotes(drutama("dcckategori")) & "'"

                ElseIf drutama("dccjeniskategori") = 2 Then 'JIKA PER CABANG, FILTER KATEGORI SESUAI CABANG
                    Dim dtCatPOS As DataTable = AsDataTableAmbilDariDBCon("SELECT GROUP_CONCAT(" & Chr(34) & "'" & Chr(34) & ",l.lkategoripos," & Chr(34) & "'" & Chr(34) & ") as kategoripos FROM m1_location l WHERE l.lkategoripos <> '' AND l.lcabang = '" & FixQuotes(drutama("dcccabang")) & "'", myConn)
                    If dtCatPOS.Rows.Count > 0 Then
                        If Len(FxDB(dtCatPOS.Rows(0)(0), "")) > 0 Then
                            ftDelKategori = "dcckategori IN (" & dtCatPOS.Rows(0)(0) & ")"
                            ftInsKategori = "pckode IN (" & dtCatPOS.Rows(0)(0) & ")"
                        End If
                    End If

                Else 'JIKA SEMUA KATEGORI
                    ftDelKategori = "dcckategori LIKE '%'"
                    ftInsKategori = "pckode LIKE '%'"

                End If

                ''BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                'ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                'ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drutama("dcckategori")) & "' "

                'HAPUS DATA KATEGORI DAN customer YANG SAMA
                If Len(ftDelKategori) > 0 Then
                    sql = "DELETE FROM M_12_Pos_Discount_Category_Customer WHERE " & ftDelKategori & " AND dcckategoricustomer = '" & FixQuotes(drutama("dcckategoricustomer")) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                Dim strValue2 As New StringBuilder
                Dim dtOperator As New DataTable
                Dim vOperator As String = ""

                'INSERT
                If Len(ftInsKategori) > 0 Then
                    Dim dtCatPOS As DataTable = AsDataTableAmbilDariDBCon("select pckode from m_12_pos_category WHERE " & ftInsKategori, myConn)
                    If dtCatPOS.Rows.Count > 0 Then
                        For Each drCatPos As DataRow In dtCatPOS.Rows
                            For Each dr1 As DataRow In dtdetail.Rows
                                'CEK OPERATOR :
                                'JIKA BETWEEN (0) ATAU >= (1) MAKA BOLEH LEBIH DARI SATU KONDISI 
                                '=> BEBERAPA KONDISI BETWEEN DAN SATU KONDISI >= (1)
                                'JIKA KELIPATAN (2) MAKA HANYA BOLEH SATU KONDISI
                                sql = "SELECT dcc.dcckategori as kategori, dcc.dcckategoricustomer as kategoricustomer, dcc.dccoperator as operator, cc.ccnama, (CASE dcc.dccoperator WHEN 0 THEN 'Between' WHEN 1 THEN '>=' WHEN 2 THEN 'Multiple' ELSE 'Unknown' END) as operatornama FROM M_12_Pos_Discount_Category_Customer dcc JOIN m1_customer_category cc ON dcc.dcckategoricustomer = cc.cckode WHERE dcc.dcckategori = '" & FxDB(drCatPos("pckode"), "") & "' AND dcc.dcckategoricustomer = '" & FxDB(dr1("dcckategoricustomer"), "") & "' GROUP BY dcc.dccoperator ORDER BY dcc.dccoperator"
                                dtOperator = AsDataTableAmbilDariDBCon(sql, myConn)
                                If dtOperator.Rows.Count > 0 Then
                                    For Each dr2 As DataRow In dtOperator.Rows
                                        vOperator = FxDB(dr2("operator").ToString, "")
                                        If Len(vOperator) > 0 Then
                                            If vOperator = 2 Then
                                                'JIKA SUDAH TERDAPAT OPERATOR KELIPATAN (2)
                                                result(2) = "Customer Category : " & FxDB(dr2("ccnama"), "") & " - already has '" & FxDB(dr2("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                            Else
                                                'JIKA BELUM TERDAPAT OPERATOR KELIPATAN (2), CEK KONDISI OPERATOR YANG SUDAH DIINPUTKAN
                                                'JIKA OPERATOR YANG DIINPUTKAN ADALAH KELIPATAN (2) MAKA TAMPILKAN ERRMESSAGE
                                                'JIKA SUDAH TERDAPAT OPERATOR >= (1) DAN YANG DIINPUTKAN ADALAH OPERATOR >= (1) LAGI MAKA TAMPILKAN ERRMESSAGE
                                                If dr1("dccoperator") = 2 Or (vOperator = 1 And dr1("dccoperator") = vOperator) Then
                                                    result(2) = "Customer Category : " & FxDB(dr2("ccnama"), "") & " - already has '" & FxDB(dr2("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                                End If
                                            End If
                                        End If
                                    Next
                                End If

                                'strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                                strValue2.Clear()
                                strValue2.Append("('" & FixQuotes(drCatPos("pckode")) & "', '" & FixQuotes(dr1("dcckategoricustomer")) & "', '" & FixQuotes(dr1("dccoperator")) & "', '" & FixDouble(dr1("dccjml1")) & "', '" & FixDouble(dr1("dccjml2")) & "', " & dr1("dcckriteria") & ", '" & FixQuotes(dr1("dccnilai")) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcctgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcctgl2"))) & "', '" & FixQuotes(dr1("dccjam1")) & "', '" & FixQuotes(dr1("dccjam2")) & "', '" & FixQuotes(dr1("dcccustomtext1")) & "', '" & FixQuotes(dr1("dcccustomtext2")) & "', '" & FixQuotes(dr1("dcccustomtext3")) & "', '" & FixQuotes(dr1("dcccustomtext4")) & "', '" & FixQuotes(dr1("dcccustomtext5")) & "', " & dr1("dcccustomint1") & ", " & dr1("dcccustomint2") & ", " & dr1("dcccustomint3") & ", '" & FixDouble(dr1("dcccustomdbl1")) & "', '" & FixDouble(dr1("dcccustomdbl2")) & "', '" & FixDouble(dr1("dcccustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcccustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcccustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcccustomdate3"))) & "')")

                                sql = "Insert into M_12_Pos_Discount_Category_Customer(dcckategori, dcckategoricustomer, dccoperator, dccjml1, dccjml2, dcckriteria, dccnilai, dcctgl1, dcctgl2, dccjam1, dccjam2, dcccustomtext1, dcccustomtext2, dcccustomtext3, dcccustomtext4, dcccustomtext5, dcccustomint1, dcccustomint2, dcccustomint3, dcccustomdbl1, dcccustomdbl2, dcccustomdbl3, dcccustomdate1, dcccustomdate2, dcccustomdate3) values" & strValue2.ToString & ""
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()
                            Next
                        Next
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
            Dim paramSearch As String = M12_Pos_Discount_Category_CustomerSearch(PostWsSearch(paramSplit(0), "M12_Pos_Discount_Category_CustomerSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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


            ''PROSES LOGOUT USER =====================================================
            'If Len(ftKategoriPOS) > 0 Then
            '    'USER YG LOGIN DILOGOUT AGAR SETTING POS TERLOAD ULANG
            '    sql = "SELECT ul.ulid, u.userid FROM m0_userlogin ul JOIN m0_user u ON ul.uluser = u.userid JOIN m1_location l ON u.ulokasi = l.lkode WHERE (" & ftKategoriPOS & ")"
            '    Dim dtUser As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            '    If dtUser.Rows.Count > 0 Then
            '        Dim WsLogout As New m0_login
            '        Dim rsLogout As String = ""
            '        For Each drUser As DataRow In dtUser.Rows
            '            'LOGOUT USER SESUAI KATEGORI POS YG DISETTING
            '            rsLogout = WsLogout.M0_Logout(drUser("ulid") & "★M0_Logout★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & drUser("userid") & "★0★" & Application("AppCode") & "")
            '        Next
            '    End If

            'End If
            ''END OF PROSES LOGOUT USER ==============================================

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
    Public Function M12_Pos_Discount_Category_CustomerDelete(ByVal param As String) As String

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
        Dim dcckategori As String = "", dcckategoricustomer As String = "", dccoperator As String = "", dccjml1 As String = "", dccjml2 As String = ""
        Dim idtrans(3) As String
        idtrans = paramSplit(5).Split(sptSubParam)
        If (idtrans.Length <> 5) Then
            result(2) = "Invalid primary key parameter." : GoTo selesai
        Else
            'CEK dcckategori
            If (Len(idtrans(0)) = 0) Then
                result(2) = "dcckategori can't be empty." : GoTo selesai
            Else
                dcckategori = idtrans(0)
            End If
            'CEK dcckategoricustomer
            If (Len(idtrans(1)) = 0) Then
                result(2) = "dcckategoricustomer can't be empty." : GoTo selesai
            Else
                dcckategoricustomer = idtrans(1)
            End If
            'CEK dccoperator
            If (Len(idtrans(2)) = 0) Then
                result(2) = "dccoperator can't be empty." : GoTo selesai
            Else
                dccoperator = idtrans(2)
            End If
            'CEK dccjml1
            If (IsNumeric(idtrans(3)) = False) Then
                result(2) = "dccjml1 required numeric." : GoTo selesai
            Else
                dccjml1 = idtrans(3)
            End If
            'CEK dccjml2
            If (IsNumeric(idtrans(4)) = False) Then
                result(2) = "dccjml2 required numeric." : GoTo selesai
            Else
                dccjml2 = idtrans(4)
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
            sql = "SELECT dcckategori as kategoripos FROM M_12_Pos_Discount_Category_Customer WHERE dcckategori = '" & dcckategori & "' AND dcckategoricustomer = '" & dcckategoricustomer & "' AND dccoperator = '" & dccoperator & "' AND dccjml1 = '" & dccjml1 & "' AND dccjml2 = '" & dccjml2 & "' GROUP BY dcckategori"
            Dim dtKategoriPOS As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtKategoriPOS.Rows.Count > 0 Then
                For Each drKategoriPOS As DataRow In dtKategoriPOS.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drKategoriPOS("kategoripos")) & "' "
                Next
            End If

            'DELETE
            sql = "DELETE FROM M_12_Pos_Discount_Category_Customer WHERE dcckategori = '" & dcckategori & "' AND dcckategoricustomer = '" & dcckategoricustomer & "' AND dccoperator = '" & dccoperator & "' AND dccjml1 = '" & dccjml1 & "' AND dccjml2 = '" & dccjml2 & "'"
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
            Dim paramSearch As String = M12_Pos_Discount_Category_CustomerSearch(PostWsSearch(paramSplit(0), "M12_Pos_Discount_Category_CustomerSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Discount_Category_CustomerImport(ByVal param As String) As String
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
        'dcckategori(0) As String, dcckategoricustomer(1) As String, dccoperator(2) As String, dccjml1(3) As Double, dccjml2(4) As Double, 
        'dcckriteria(5) As Integer, dccnilai(6) As String, dcctgl1(7) As Date, dcctgl2(8) As Date, dccjam1(9) As Date, 
        'dccjam2(10) As Date, dcccustomtext1(11) As String, dcccustomtext2(12) As String, dcccustomtext3(13) As String, dcccustomtext4(14) As String, 
        'dcccustomtext5(15) As String, dcccustomint1(16) As Integer, dcccustomint2(17) As Integer, dcccustomint3(18) As Integer, dcccustomdbl1(19) As Double, 
        'dcccustomdbl2(20) As Double, dcccustomdbl3(21) As Double, dcccustomdate1(22) As Date, dcccustomdate2(23) As Date, dcccustomdate3(24) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'dcckategori, dcckategoricustomer, dccoperator, dccjml1, dccjml2, dcckriteria, dccnilai, 
        'dcctgl1, dcctgl2, dccjam1, dccjam2, dcccustomtext1, dcccustomtext2, dcccustomtext3, 
        'dcccustomtext4, dcccustomtext5, dcccustomint1, dcccustomint2, dcccustomint3, dcccustomdbl1, dcccustomdbl2, 
        'dcccustomdbl3, dcccustomdate1, dcccustomdate2, dcccustomdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "dcckategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcckategoricustomer", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dccoperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dccjml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dccjml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcckriteria", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dccnilai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcctgl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcctgl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dccjam1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dccjam2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcccustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcccustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcccustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcccustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcccustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcccustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dcccustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dcccustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dcccustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcccustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcccustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcccustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcccustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dcccustomdate3", AsEnumTypeData.AsString)

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
            'dccjml1(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - dccjml1 required numeric." : GoTo selesai
            End If
            'dccjml2(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - dccjml2 required numeric." : GoTo selesai
            End If
            'dcckriteria(5) As Integer
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - dcckriteria required numeric." : GoTo selesai
            ElseIf dataRowDetail(5) <> 0 And dataRowDetail(5) <> 1 And dataRowDetail(5) <> 2 Then
                result(2) = "Row : " & i & " - invalid dcckriteria value" : GoTo selesai
            End If
            'dcctgl1(7) As Date
            If (IsDate(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - dcctgl1 required date." : GoTo selesai
            End If
            'dcctgl2(8) As Date
            If (IsDate(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - dcctgl2 required date." : GoTo selesai
            End If
            'dccjam1(9) As Date
            If (IsDate(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - dccjam1 required date." : GoTo selesai
            End If
            'dccjam2(10) As Date
            If (IsDate(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - dccjam2 required date." : GoTo selesai
            End If
            'dcccustomint1(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - dcccustomint1 required numeric." : GoTo selesai
            End If
            'dcccustomint2(17) As Integer
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - dcccustomint2 required numeric." : GoTo selesai
            End If
            'dcccustomint3(18) As Integer
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - dcccustomint3 required numeric." : GoTo selesai
            End If
            'dcccustomdbl1(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - dcccustomdbl1 required numeric." : GoTo selesai
            End If
            'dcccustomdbl2(20) As Double
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - dcccustomdbl2 required numeric." : GoTo selesai
            End If
            'dcccustomdbl3(21) As Double
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - dcccustomdbl3 required numeric." : GoTo selesai
            End If
            'dcccustomdate1(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - dcccustomdate1 required date." : GoTo selesai
            End If
            'dcccustomdate2(23) As Date
            If (IsDate(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - dcccustomdate2 required date." : GoTo selesai
            End If
            'dcccustomdate3(24) As Date
            If (IsDate(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - dcccustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'dcckategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - dcckategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - dcckategori should not be more than 25 character." : GoTo selesai
            End If

            'dcckategoricustomer(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - dcckategoricustomer can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - dcckategoricustomer should not be more than 25 character." : GoTo selesai
            End If

            'dccoperator(2) As String
            If IsNumeric(dataRowDetail(2)) = False Then
                result(2) = "Row : " & i & " - dccoperator required numeric" : GoTo selesai
            ElseIf dataRowDetail(2) <> 0 And dataRowDetail(2) <> 1 And dataRowDetail(2) <> 2 Then
                result(2) = "Row : " & i & " - invalid dccoperator value" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - dccoperator should not be more than 25 character." : GoTo selesai
            End If

            'dccjml1(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - dccjml1 can't be empty" : GoTo selesai
            End If

            'dccjml2(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - dccjml2 can't be empty" : GoTo selesai
            End If

            'dccnilai(6) As String
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - dccnilai can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(6)) > 25 Then
                result(2) = "Row : " & i & " - dccnilai should not be more than 25 character." : GoTo selesai
            End If

            'dcctgl1(7) As Date
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - dcctgl1 can't be empty" : GoTo selesai
            End If

            'dcctgl2(8) As Date
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - dcctgl2 can't be empty" : GoTo selesai
            End If

            'dccjam1(9) As Date
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - dccjam1 can't be empty" : GoTo selesai
            End If

            'dccjam2(10) As Date
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - dccjam2 can't be empty" : GoTo selesai
            End If

            'dcccustomdbl1(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - dcccustomdbl1 can't be empty" : GoTo selesai
            End If

            'dcccustomdbl2(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - dcccustomdbl2 can't be empty" : GoTo selesai
            End If

            'dcccustomdbl3(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - dcccustomdbl3 can't be empty" : GoTo selesai
            End If

            'dcccustomdate1(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - dcccustomdate1 can't be empty" : GoTo selesai
            End If

            'dcccustomdate2(23) As Date
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - dcccustomdate2 can't be empty" : GoTo selesai
            End If

            'dcccustomdate3(24) As Date
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - dcccustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "dcckategori~dcckategoricustomer~dccoperator~dccjml1~dccjml2~dcckriteria~dccnilai~dcctgl1~dcctgl2~dccjam1~dccjam2~dcccustomtext1~dcccustomtext2~dcccustomtext3~dcccustomtext4~dcccustomtext5~dcccustomint1~dcccustomint2~dcccustomint3~dcccustomdbl1~dcccustomdbl2~dcccustomdbl3~dcccustomdate1~dcccustomdate2~dcccustomdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24)) = False Then
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
                    strValue2.Append("('" & FixQuotes(dr1("dcckategori")) & "', '" & FixQuotes(dr1("dcckategoricustomer")) & "', '" & FixQuotes(dr1("dccoperator")) & "', '" & FixDouble(dr1("dccjml1")) & "', '" & FixDouble(dr1("dccjml2")) & "', " & dr1("dcckriteria") & ", '" & FixQuotes(dr1("dccnilai")) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcctgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcctgl2"))) & "', '" & FixQuotes(dr1("dccjam1")) & "', '" & FixQuotes(dr1("dccjam2")) & "', '" & FixQuotes(dr1("dcccustomtext1")) & "', '" & FixQuotes(dr1("dcccustomtext2")) & "', '" & FixQuotes(dr1("dcccustomtext3")) & "', '" & FixQuotes(dr1("dcccustomtext4")) & "', '" & FixQuotes(dr1("dcccustomtext5")) & "', " & dr1("dcccustomint1") & ", " & dr1("dcccustomint2") & ", " & dr1("dcccustomint3") & ", '" & FixDouble(dr1("dcccustomdbl1")) & "', '" & FixDouble(dr1("dcccustomdbl2")) & "', '" & FixDouble(dr1("dcccustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcccustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcccustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dcccustomdate3"))) & "')")
                Next

                If Len(strValue2.ToString) > 0 Then
                    'DELETE
                    sql = "DELETE FROM M_12_Pos_Discount_Category_Customer"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'insert jika data belum ada, dan update jika data sudah ada
                    sql = "Insert into M_12_Pos_Discount_Category_Customer(dcckategori, dcckategoricustomer, dccoperator, dccjml1, dccjml2, dcckriteria, dccnilai, dcctgl1, dcctgl2, dccjam1, dccjam2, dcccustomtext1, dcccustomtext2, dcccustomtext3, dcccustomtext4, dcccustomtext5, dcccustomint1, dcccustomint2, dcccustomint3, dcccustomdbl1, dcccustomdbl2, dcccustomdbl3, dcccustomdate1, dcccustomdate2, dcccustomdate3) values" & strValue2.ToString & ""
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
            Dim paramSearch As String = M12_Pos_Discount_Category_CustomerSearch(PostWsSearch(paramSplit(0), "M12_Pos_Discount_Category_CustomerSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Discount_Category_CustomerSearch(ByVal param As String) As String
        'JIKA MENAMBAHKAN FIELD DISINI MAKA HARUS TAMBAHKAN JUGA LENGTH ARRAY PADA VALIDASI AMBIL USERID WS M0_LOGIN
        'M12_Pos_Discount_Category_CustomerSearch --------------------------------------------------------
        'dcckategori, dcckategoricustomer, dccoperator, dccjml1, dccjml2, dcckriteria, dccnilai, 
        'dcctgl1, dcctgl2, dccjam1, dccjam2, dcccustomtext1, dcccustomtext2, dcccustomtext3, 
        'dcccustomtext4, dcccustomtext5, dcccustomint1, dcccustomint2, dcccustomint3, dcccustomdbl1, dcccustomdbl2, 
        'dcccustomdbl3, dcccustomdate1, dcccustomdate2, dcccustomdate3, pcnama, ccnama, dcckriterianama, dccoperatornama

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
        sql = "select `dcc`.`dcckategori` AS `dcckategori`,`dcc`.`dcckategoricustomer` AS `dcckategoricustomer`,`dcc`.`dccoperator` AS `dccoperator`,`dcc`.`dccjml1` AS `dccjml1`,`dcc`.`dccjml2` AS `dccjml2`,`dcc`.`dcckriteria` AS `dcckriteria`,`dcc`.`dccnilai` AS `dccnilai`,`dcc`.`dcctgl1` AS `dcctgl1`,`dcc`.`dcctgl2` AS `dcctgl2`,`dcc`.`dccjam1` AS `dccjam1`,`dcc`.`dccjam2` AS `dccjam2`,`dcc`.`dcccustomtext1` AS `dcccustomtext1`,`dcc`.`dcccustomtext2` AS `dcccustomtext2`,`dcc`.`dcccustomtext3` AS `dcccustomtext3`,`dcc`.`dcccustomtext4` AS `dcccustomtext4`,`dcc`.`dcccustomtext5` AS `dcccustomtext5`,`dcc`.`dcccustomint1` AS `dcccustomint1`,`dcc`.`dcccustomint2` AS `dcccustomint2`,`dcc`.`dcccustomint3` AS `dcccustomint3`,`dcc`.`dcccustomdbl1` AS `dcccustomdbl1`,`dcc`.`dcccustomdbl2` AS `dcccustomdbl2`,`dcc`.`dcccustomdbl3` AS `dcccustomdbl3`,`dcc`.`dcccustomdate1` AS `dcccustomdate1`,`dcc`.`dcccustomdate2` AS `dcccustomdate2`,`dcc`.`dcccustomdate3` AS `dcccustomdate3`,`pc`.`pcnama` AS `pcnama`,`cc`.`ccnama` AS `ccnama`,(case `dcc`.`dcckriteria` when 0 then 'Price' when 1 then 'Discount Percent' when 2 then 'Discount Nominal' else 'Unknown' end) AS `dcckriterianama`,(case `dcc`.`dccoperator` when 0 then 'Between' when 1 then '>=' when 2 then 'Multiple' else 'Unknown' end) AS `dccoperatornama` from ((`M_12_Pos_Discount_Category_Customer` `dcc` join `m_12_pos_category` `pc` on((`dcc`.`dcckategori` = `pc`.`pckode`))) join `m1_customer_category` `cc` on((`dcc`.`dcckategoricustomer` = `cc`.`cckode`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M_12_Pos_Discount_Category_Customer", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("dcckategori"), ""), sptField,
                     FxDB(dr("dcckategoricustomer"), ""), sptField,
                     FxDB(dr("dccoperator"), ""), sptField,
                     FxDB(dr("dccjml1"), 0), sptField,
                     FxDB(dr("dccjml2"), 0), sptField,
                     FxDB(dr("dcckriteria"), 0), sptField,
                     FxDB(dr("dccnilai"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dcctgl1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("dcctgl2"), ""), formatTgl), sptField,
                     FxDB(dr("dccjam1").ToString, ""), sptField,
                     FxDB(dr("dccjam2").ToString, ""), sptField,
                     FxDB(dr("dcccustomtext1"), ""), sptField,
                     FxDB(dr("dcccustomtext2"), ""), sptField,
                     FxDB(dr("dcccustomtext3"), ""), sptField,
                     FxDB(dr("dcccustomtext4"), ""), sptField,
                     FxDB(dr("dcccustomtext5"), ""), sptField,
                     FxDB(dr("dcccustomint1"), 0), sptField,
                     FxDB(dr("dcccustomint2"), 0), sptField,
                     FxDB(dr("dcccustomint3"), 0), sptField,
                     FxDB(dr("dcccustomdbl1"), 0), sptField,
                     FxDB(dr("dcccustomdbl2"), 0), sptField,
                     FxDB(dr("dcccustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("dcccustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("dcccustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("dcccustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("pcnama"), ""), sptField,
                     FxDB(dr("ccnama"), ""), sptField,
                     FxDB(dr("dcckriterianama"), ""), sptField,
                     FxDB(dr("dccoperatornama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Discount Category Customer data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("dcckategori, dcckategoricustomer, dccoperator, dccjml1, dccjml2, dcckriteria, dccnilai, dcctgl1, dcctgl2, dccjam1, dccjam2, dcccustomtext1, dcccustomtext2, dcccustomtext3, dcccustomtext4, dcccustomtext5, dcccustomint1, dcccustomint2, dcccustomint3, dcccustomdbl1, dcccustomdbl2, dcccustomdbl3, dcccustomdate1, dcccustomdate2, dcccustomdate3, pcnama, ccnama, dcckriterianama, dccoperatornama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_Discount_Category_CustomerDownload(ByVal param As String) As String
        'M12_Pos_Discount_Category_CustomerDownload --------------------------------------------------------
        'dcckategori, dcckategoricustomer, dccoperator, dccjml1, dccjml2, dcckriteria, dccnilai, 
        'dcctgl1, dcctgl2, dccjam1, dccjam2, dcccustomtext1, dcccustomtext2, dcccustomtext3, 
        'dcccustomtext4, dcccustomtext5, dcccustomint1, dcccustomint2, dcccustomint3, dcccustomdbl1, dcccustomdbl2, 
        'dcccustomdbl3, dcccustomdate1, dcccustomdate2, dcccustomdate3

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

        dt = AmbilData("aplikasi1-M_12_Pos_Discount_Category_Customer", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("dcckategori"), ""), sptField,
                     FxDB(dr("dcckategoricustomer"), ""), sptField,
                     FxDB(dr("dccoperator"), ""), sptField,
                     FxDB(dr("dccjml1"), 0), sptField,
                     FxDB(dr("dccjml2"), 0), sptField,
                     FxDB(dr("dcckriteria"), 0), sptField,
                     FxDB(dr("dccnilai"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dcctgl1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("dcctgl2"), ""), formatTgl), sptField,
                     FxDB(dr("dccjam1"), ""), sptField,
                     FxDB(dr("dccjam2"), ""), sptField,
                     FxDB(dr("dcccustomtext1"), ""), sptField,
                     FxDB(dr("dcccustomtext2"), ""), sptField,
                     FxDB(dr("dcccustomtext3"), ""), sptField,
                     FxDB(dr("dcccustomtext4"), ""), sptField,
                     FxDB(dr("dcccustomtext5"), ""), sptField,
                     FxDB(dr("dcccustomint1"), 0), sptField,
                     FxDB(dr("dcccustomint2"), 0), sptField,
                     FxDB(dr("dcccustomint3"), 0), sptField,
                     FxDB(dr("dcccustomdbl1"), 0), sptField,
                     FxDB(dr("dcccustomdbl2"), 0), sptField,
                     FxDB(dr("dcccustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("dcccustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("dcccustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("dcccustomdate3"), ""), formatTgl), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Discount Category Customer data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("dcckategori, dcckategoricustomer, dccoperator, dccjml1, dccjml2, dcckriteria, dccnilai, dcctgl1, dcctgl2, dccjam1, dccjam2, dcccustomtext1, dcccustomtext2, dcccustomtext3, dcccustomtext4, dcccustomtext5, dcccustomint1, dcccustomint2, dcccustomint3, dcccustomdbl1, dcccustomdbl2, dcccustomdbl3, dcccustomdate1, dcccustomdate2, dcccustomdate3"))

        Return wsResult
    End Function

End Class
