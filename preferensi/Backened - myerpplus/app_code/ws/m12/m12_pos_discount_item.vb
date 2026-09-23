Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_pos_discount_item
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_Pos_Discount_ItemSimpan(ByVal param As String) As String
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
        'dikategori(0) As String, diidbarang(1) As Double, dioperator(2) As String, dijml1(3) As Double, dijml2(4) As Double, 
        'dikriteria(5) As Integer, dinilai(6) As String, ditgl1(7) As Date, ditgl2(8) As Date, dijam1(9) As Date, 
        'dijam2(10) As Date, dicustomtext1(11) As String, dicustomtext2(12) As String, dicustomtext3(13) As String, dicustomtext4(14) As String, 
        'dicustomtext5(15) As String, dicustomint1(16) As Integer, dicustomint2(17) As Integer, dicustomint3(18) As Integer, dicustomdbl1(19) As Double, 
        'dicustomdbl2(20) As Double, dicustomdbl3(21) As Double, dicustomdate1(22) As Date, dicustomdate2(23) As Date, dicustomdate3(24) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'dikategori, diidbarang, dioperator, dijml1, dijml2, dikriteria, dinilai, 
        'ditgl1, ditgl2, dijam1, dijam2, dicustomtext1, dicustomtext2, dicustomtext3, 
        'dicustomtext4, dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, 
        'dicustomdbl3, dicustomdate1, dicustomdate2, dicustomdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "dikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dioperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dijml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dijml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dikriteria", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dinilai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ditgl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ditgl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dijam1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dijam2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomdate3", AsEnumTypeData.AsString)

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
            'diidbarang(1) As Double
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - diidbarang required numeric." : GoTo selesai
            End If
            'dijml1(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - dijml1 required numeric." : GoTo selesai
            End If
            'dijml2(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - dijml2 required numeric." : GoTo selesai
            End If
            'dikriteria(5) As Integer
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - dikriteria required numeric." : GoTo selesai
            ElseIf dataRowDetail(5) <> 0 And dataRowDetail(5) <> 1 And dataRowDetail(5) <> 2 Then
                result(2) = "Row : " & i & " - invalid dikriteria value" : GoTo selesai
            End If
            'ditgl1(7) As Date
            If (IsDate(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - ditgl1 required date." : GoTo selesai
            End If
            'ditgl2(8) As Date
            If (IsDate(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - ditgl2 required date." : GoTo selesai
            End If

            'dijam1(9) As Date
            If (IsDate(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - dijam1 required time format (H:mm:ss)." : GoTo selesai
            End If

            'dijam2(10) As Date
            If (IsDate(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - dijam2 required time format (H:mm:ss)." : GoTo selesai
            End If

            'dicustomint1(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - dicustomint1 required numeric." : GoTo selesai
            End If
            'dicustomint2(17) As Integer
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - dicustomint2 required numeric." : GoTo selesai
            End If
            'dicustomint3(18) As Integer
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - dicustomint3 required numeric." : GoTo selesai
            End If
            'dicustomdbl1(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - dicustomdbl1 required numeric." : GoTo selesai
            End If
            'dicustomdbl2(20) As Double
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - dicustomdbl2 required numeric." : GoTo selesai
            End If
            'dicustomdbl3(21) As Double
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - dicustomdbl3 required numeric." : GoTo selesai
            End If
            'dicustomdate1(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - dicustomdate1 required date." : GoTo selesai
            End If
            'dicustomdate2(23) As Date
            If (IsDate(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - dicustomdate2 required date." : GoTo selesai
            End If
            'dicustomdate3(24) As Date
            If (IsDate(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - dicustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'dikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - dikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - dikategori should not be more than 25 character." : GoTo selesai
            End If

            'diidbarang(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - diidbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - diidbarang should not be more than 20 character." : GoTo selesai
            End If

            'dioperator(2) As String
            If IsNumeric(dataRowDetail(2)) = False Then
                result(2) = "Row : " & i & " - dioperator required numeric" : GoTo selesai
            ElseIf dataRowDetail(2) <> 0 And dataRowDetail(2) <> 1 And dataRowDetail(2) <> 2 Then
                result(2) = "Row : " & i & " - invalid dioperator value" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - dioperator should not be more than 25 character." : GoTo selesai
            End If

            'dijml1(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - dijml1 can't be empty" : GoTo selesai
            End If

            'dijml2(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - dijml2 can't be empty" : GoTo selesai
            End If

            'dinilai(6) As String
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - dinilai can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(6)) > 25 Then
                result(2) = "Row : " & i & " - dinilai should not be more than 25 character." : GoTo selesai
            End If

            'ditgl1(7) As Date
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - ditgl1 can't be empty" : GoTo selesai
            End If

            'ditgl2(8) As Date
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - ditgl2 can't be empty" : GoTo selesai
            End If

            'dijam1(9) As Date
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - dijam1 can't be empty" : GoTo selesai
            End If

            'dijam2(10) As Date
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - dijam2 can't be empty" : GoTo selesai
            End If

            'dicustomdbl1(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - dicustomdbl1 can't be empty" : GoTo selesai
            End If

            'dicustomdbl2(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - dicustomdbl2 can't be empty" : GoTo selesai
            End If

            'dicustomdbl3(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - dicustomdbl3 can't be empty" : GoTo selesai
            End If

            'dicustomdate1(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - dicustomdate1 can't be empty" : GoTo selesai
            End If

            'dicustomdate2(23) As Date
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - dicustomdate2 can't be empty" : GoTo selesai
            End If

            'dicustomdate3(24) As Date
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - dicustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "dikategori~diidbarang~dioperator~dijml1~dijml2~dikriteria~dinilai~ditgl1~ditgl2~dijam1~dijam2~dicustomtext1~dicustomtext2~dicustomtext3~dicustomtext4~dicustomtext5~dicustomint1~dicustomint2~dicustomint3~dicustomdbl1~dicustomdbl2~dicustomdbl3~dicustomdate1~dicustomdate2~dicustomdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24)) = False Then
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
                ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drutama("dikategori")) & "' "

                'HAPUS DATA KATEGORI DAN BARANG YANG SAMA
                sql = "DELETE FROM m_12_pos_discount_item WHERE dikategori = '" & FixQuotes(drutama("dikategori")) & "' AND diidbarang = '" & FixQuotes(drutama("diidbarang")) & "'"
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
                    sql = "SELECT di.dikategori as kategori, di.diidbarang as idbarang, di.dioperator as operator, i.bkode, (CASE di.dioperator WHEN 0 THEN 'Between' WHEN 1 THEN '>=' WHEN 2 THEN 'Multiple' ELSE 'Unknown' END) as operatornama FROM m_12_pos_discount_item di JOIN m1_item i ON di.diidbarang = i.bid WHERE di.dikategori = '" & FxDB(dr1("dikategori"), "") & "' AND di.diidbarang = '" & FxDB(dr1("diidbarang"), "") & "' GROUP BY di.dioperator ORDER BY di.dioperator"
                    dtOperator = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtOperator.Rows.Count > 0 Then
                        For Each dr2 As DataRow In dtOperator.Rows
                            vOperator = FxDB(dr2("operator").ToString, "")
                            If Len(vOperator) > 0 Then
                                If vOperator = 2 Then
                                    'JIKA SUDAH TERDAPAT OPERATOR KELIPATAN (2)
                                    result(2) = "Item : " & FxDB(dr2("bkode"), "") & " - already has '" & FxDB(dr2("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                Else
                                    'JIKA BELUM TERDAPAT OPERATOR KELIPATAN (2), CEK KONDISI OPERATOR YANG SUDAH DIINPUTKAN
                                    'JIKA OPERATOR YANG DIINPUTKAN ADALAH KELIPATAN (2) MAKA TAMPILKAN ERRMESSAGE
                                    'JIKA SUDAH TERDAPAT OPERATOR >= (1) DAN YANG DIINPUTKAN ADALAH OPERATOR >= (1) LAGI MAKA TAMPILKAN ERRMESSAGE
                                    If dr1("dioperator") = 2 Or (vOperator = 1 And dr1("dioperator") = vOperator) Then
                                        result(2) = "Item : " & FxDB(dr2("bkode"), "") & " - already has '" & FxDB(dr2("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                    End If
                                End If
                            End If
                        Next
                    End If

                    'strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Clear()
                    strValue2.Append("('" & FixQuotes(dr1("dikategori")) & "', '" & FixQuotes(dr1("diidbarang")) & "', '" & FixQuotes(dr1("dioperator")) & "', '" & FixDouble(dr1("dijml1")) & "', '" & FixDouble(dr1("dijml2")) & "', " & dr1("dikriteria") & ", '" & FixQuotes(dr1("dinilai")) & "', '" & FixQuotes(AsFormatTanggal(dr1("ditgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("ditgl2"))) & "', '" & FixQuotes(dr1("dijam1")) & "', '" & FixQuotes(dr1("dijam2")) & "', '" & FixQuotes(dr1("dicustomtext1")) & "', '" & FixQuotes(dr1("dicustomtext2")) & "', '" & FixQuotes(dr1("dicustomtext3")) & "', '" & FixQuotes(dr1("dicustomtext4")) & "', '" & FixQuotes(dr1("dicustomtext5")) & "', " & dr1("dicustomint1") & ", " & dr1("dicustomint2") & ", " & dr1("dicustomint3") & ", '" & FixDouble(dr1("dicustomdbl1")) & "', '" & FixDouble(dr1("dicustomdbl2")) & "', '" & FixDouble(dr1("dicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("dicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dicustomdate3"))) & "')")

                    sql = "Insert into M_12_Pos_Discount_Item(dikategori, diidbarang, dioperator, dijml1, dijml2, dikriteria, dinilai, ditgl1, ditgl2, dijam1, dijam2, dicustomtext1, dicustomtext2, dicustomtext3, dicustomtext4, dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, dicustomdbl3, dicustomdate1, dicustomdate2, dicustomdate3) values" & strValue2.ToString & ""
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
            Dim paramSearch As String = M12_Pos_Discount_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Discount_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Discount_ItemDelete(ByVal param As String) As String

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
        Dim dikategori As String = "", diidbarang As String = "", dioperator As String = "", dijml1 As String = "", dijml2 As String = ""
        Dim idtrans(3) As String
        idtrans = paramSplit(5).Split(sptSubParam)
        If (idtrans.Length <> 5) Then
            result(2) = "Invalid primary key parameter." : GoTo selesai
        Else
            'CEK dikategori
            If (Len(idtrans(0)) = 0) Then
                result(2) = "dikategori can't be empty." : GoTo selesai
            Else
                dikategori = idtrans(0)
            End If
            'CEK diidbarang
            If (IsNumeric(idtrans(1)) = False) Then
                result(2) = "diidbarang required numeric." : GoTo selesai
            Else
                diidbarang = idtrans(1)
            End If
            'CEK dioperator
            If (Len(idtrans(2)) = 0) Then
                result(2) = "dioperator can't be empty." : GoTo selesai
            Else
                dioperator = idtrans(2)
            End If
            'CEK dijml1
            If (IsNumeric(idtrans(3)) = False) Then
                result(2) = "dijml1 required numeric." : GoTo selesai
            Else
                dijml1 = idtrans(3)
            End If
            'CEK dijml2
            If (IsNumeric(idtrans(4)) = False) Then
                result(2) = "dijml2 required numeric." : GoTo selesai
            Else
                dijml2 = idtrans(4)
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
            sql = "SELECT dikategori as kategoripos FROM M_12_Pos_Discount_Item WHERE dikategori = '" & dikategori & "' AND diidbarang = '" & diidbarang & "' AND dioperator = '" & dioperator & "' AND dijml1 = '" & dijml1 & "' AND dijml2 = '" & dijml2 & "' GROUP BY dikategori"
            Dim dtKategoriPOS As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtKategoriPOS.Rows.Count > 0 Then
                For Each drKategoriPOS As DataRow In dtKategoriPOS.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drKategoriPOS("kategoripos")) & "' "
                Next
            End If

            'DELETE
            sql = "DELETE FROM M_12_Pos_Discount_Item WHERE dikategori = '" & dikategori & "' AND diidbarang = '" & diidbarang & "' AND dioperator = '" & dioperator & "' AND dijml1 = '" & dijml1 & "' AND dijml2 = '" & dijml2 & "'"
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
            Dim paramSearch As String = M12_Pos_Discount_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Discount_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Discount_ItemImport(ByVal param As String) As String
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
        'dikategori(0) As String, diidbarang(1) As Double, dioperator(2) As String, dijml1(3) As Double, dijml2(4) As Double, 
        'dikriteria(5) As Integer, dinilai(6) As String, ditgl1(7) As Date, ditgl2(8) As Date, dijam1(9) As Date, 
        'dijam2(10) As Date, dicustomtext1(11) As String, dicustomtext2(12) As String, dicustomtext3(13) As String, dicustomtext4(14) As String, 
        'dicustomtext5(15) As String, dicustomint1(16) As Integer, dicustomint2(17) As Integer, dicustomint3(18) As Integer, dicustomdbl1(19) As Double, 
        'dicustomdbl2(20) As Double, dicustomdbl3(21) As Double, dicustomdate1(22) As Date, dicustomdate2(23) As Date, dicustomdate3(24) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'dikategori, diidbarang, dioperator, dijml1, dijml2, dikriteria, dinilai, 
        'ditgl1, ditgl2, dijam1, dijam2, dicustomtext1, dicustomtext2, dicustomtext3, 
        'dicustomtext4, dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, 
        'dicustomdbl3, dicustomdate1, dicustomdate2, dicustomdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "dikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dioperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dijml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dijml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dikriteria", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dinilai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ditgl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ditgl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dijam1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dijam2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomdate3", AsEnumTypeData.AsString)

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
            'diidbarang(1) As Double
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - diidbarang required numeric." : GoTo selesai
            End If
            'dijml1(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - dijml1 required numeric." : GoTo selesai
            End If
            'dijml2(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - dijml2 required numeric." : GoTo selesai
            End If
            'dikriteria(5) As Integer
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - dikriteria required numeric." : GoTo selesai
            ElseIf dataRowDetail(5) <> 0 And dataRowDetail(5) <> 1 And dataRowDetail(5) <> 2 Then
                result(2) = "Row : " & i & " - invalid dikriteria value" : GoTo selesai
            End If
            'ditgl1(7) As Date
            If (IsDate(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - ditgl1 required date." : GoTo selesai
            End If
            'ditgl2(8) As Date
            If (IsDate(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - ditgl2 required date." : GoTo selesai
            End If
            'dijam1(9) As Date
            If (IsDate(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - dijam1 required date." : GoTo selesai
            End If
            'dijam2(10) As Date
            If (IsDate(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - dijam2 required date." : GoTo selesai
            End If
            'dicustomint1(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - dicustomint1 required numeric." : GoTo selesai
            End If
            'dicustomint2(17) As Integer
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - dicustomint2 required numeric." : GoTo selesai
            End If
            'dicustomint3(18) As Integer
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - dicustomint3 required numeric." : GoTo selesai
            End If
            'dicustomdbl1(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - dicustomdbl1 required numeric." : GoTo selesai
            End If
            'dicustomdbl2(20) As Double
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - dicustomdbl2 required numeric." : GoTo selesai
            End If
            'dicustomdbl3(21) As Double
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - dicustomdbl3 required numeric." : GoTo selesai
            End If
            'dicustomdate1(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - dicustomdate1 required date." : GoTo selesai
            End If
            'dicustomdate2(23) As Date
            If (IsDate(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - dicustomdate2 required date." : GoTo selesai
            End If
            'dicustomdate3(24) As Date
            If (IsDate(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - dicustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'dikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - dikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - dikategori should not be more than 25 character." : GoTo selesai
            End If

            'diidbarang(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - diidbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - diidbarang should not be more than 20 character." : GoTo selesai
            End If

            'dioperator(2) As String
            If IsNumeric(dataRowDetail(2)) = False Then
                result(2) = "Row : " & i & " - dioperator required numeric" : GoTo selesai
            ElseIf dataRowDetail(2) <> 0 And dataRowDetail(2) <> 1 And dataRowDetail(2) <> 2 Then
                result(2) = "Row : " & i & " - invalid dioperator value" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - dioperator should not be more than 25 character." : GoTo selesai
            End If

            'dijml1(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - dijml1 can't be empty" : GoTo selesai
            End If

            'dijml2(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - dijml2 can't be empty" : GoTo selesai
            End If

            'dinilai(6) As String
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - dinilai can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(6)) > 25 Then
                result(2) = "Row : " & i & " - dinilai should not be more than 25 character." : GoTo selesai
            End If

            'ditgl1(7) As Date
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - ditgl1 can't be empty" : GoTo selesai
            End If

            'ditgl2(8) As Date
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - ditgl2 can't be empty" : GoTo selesai
            End If

            'dijam1(9) As Date
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - dijam1 can't be empty" : GoTo selesai
            End If

            'dijam2(10) As Date
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - dijam2 can't be empty" : GoTo selesai
            End If

            'dicustomdbl1(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - dicustomdbl1 can't be empty" : GoTo selesai
            End If

            'dicustomdbl2(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - dicustomdbl2 can't be empty" : GoTo selesai
            End If

            'dicustomdbl3(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - dicustomdbl3 can't be empty" : GoTo selesai
            End If

            'dicustomdate1(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - dicustomdate1 can't be empty" : GoTo selesai
            End If

            'dicustomdate2(23) As Date
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - dicustomdate2 can't be empty" : GoTo selesai
            End If

            'dicustomdate3(24) As Date
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - dicustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "dikategori~diidbarang~dioperator~dijml1~dijml2~dikriteria~dinilai~ditgl1~ditgl2~dijam1~dijam2~dicustomtext1~dicustomtext2~dicustomtext3~dicustomtext4~dicustomtext5~dicustomint1~dicustomint2~dicustomint3~dicustomdbl1~dicustomdbl2~dicustomdbl3~dicustomdate1~dicustomdate2~dicustomdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24)) = False Then
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
                    strValue2.Append("('" & FixQuotes(dr1("dikategori")) & "', '" & FixQuotes(dr1("diidbarang")) & "', '" & FixQuotes(dr1("dioperator")) & "', '" & FixDouble(dr1("dijml1")) & "', '" & FixDouble(dr1("dijml2")) & "', " & dr1("dikriteria") & ", '" & FixQuotes(dr1("dinilai")) & "', '" & FixQuotes(AsFormatTanggal(dr1("ditgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("ditgl2"))) & "', '" & FixQuotes(dr1("dijam1")) & "', '" & FixQuotes(dr1("dijam2")) & "', '" & FixQuotes(dr1("dicustomtext1")) & "', '" & FixQuotes(dr1("dicustomtext2")) & "', '" & FixQuotes(dr1("dicustomtext3")) & "', '" & FixQuotes(dr1("dicustomtext4")) & "', '" & FixQuotes(dr1("dicustomtext5")) & "', " & dr1("dicustomint1") & ", " & dr1("dicustomint2") & ", " & dr1("dicustomint3") & ", '" & FixDouble(dr1("dicustomdbl1")) & "', '" & FixDouble(dr1("dicustomdbl2")) & "', '" & FixDouble(dr1("dicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("dicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dicustomdate3"))) & "')")
                Next

                If Len(strValue2.ToString) > 0 Then
                    'DELETE
                    sql = "DELETE FROM M_12_Pos_Discount_Item"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'insert jika data belum ada, dan update jika data sudah ada
                    sql = "Insert into M_12_Pos_Discount_Item(dikategori, diidbarang, dioperator, dijml1, dijml2, dikriteria, dinilai, ditgl1, ditgl2, dijam1, dijam2, dicustomtext1, dicustomtext2, dicustomtext3, dicustomtext4, dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, dicustomdbl3, dicustomdate1, dicustomdate2, dicustomdate3) values" & strValue2.ToString & ""
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
            Dim paramSearch As String = M12_Pos_Discount_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Discount_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Discount_ItemSimpanOld(ByVal param As String) As String
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
        'dikategori(0) As String, diidbarang(1) As Double, dioperator(2) As String, dijml1(3) As Double, dijml2(4) As Double, 
        'dikriteria(5) As Integer, dinilai(6) As String, ditgl1(7) As Date, ditgl2(8) As Date, dijam1(9) As Date, 
        'dijam2(10) As Date, dicustomtext1(11) As String, dicustomtext2(12) As String, dicustomtext3(13) As String, dicustomtext4(14) As String, 
        'dicustomtext5(15) As String, dicustomint1(16) As Integer, dicustomint2(17) As Integer, dicustomint3(18) As Integer, dicustomdbl1(19) As Double, 
        'dicustomdbl2(20) As Double, dicustomdbl3(21) As Double, dicustomdate1(22) As Date, dicustomdate2(23) As Date, dicustomdate3(24) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'dikategori, diidbarang, dioperator, dijml1, dijml2, dikriteria, dinilai, 
        'ditgl1, ditgl2, dijam1, dijam2, dicustomtext1, dicustomtext2, dicustomtext3, 
        'dicustomtext4, dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, 
        'dicustomdbl3, dicustomdate1, dicustomdate2, dicustomdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "dikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dioperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dijml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dijml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dikriteria", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dinilai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ditgl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ditgl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dijam1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dijam2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomdate3", AsEnumTypeData.AsString)

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
            'diidbarang(1) As Double
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - diidbarang required numeric." : GoTo selesai
            End If
            'dijml1(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - dijml1 required numeric." : GoTo selesai
            End If
            'dijml2(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - dijml2 required numeric." : GoTo selesai
            End If
            'dikriteria(5) As Integer
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - dikriteria required numeric." : GoTo selesai
            ElseIf dataRowDetail(5) <> 0 And dataRowDetail(5) <> 1 And dataRowDetail(5) <> 2 Then
                result(2) = "Row : " & i & " - invalid dikriteria value" : GoTo selesai
            End If
            'ditgl1(7) As Date
            If (IsDate(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - ditgl1 required date." : GoTo selesai
            End If
            'ditgl2(8) As Date
            If (IsDate(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - ditgl2 required date." : GoTo selesai
            End If

            'dijam1(9) As Date
            If (IsDate(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - dijam1 required time format (H:mm:ss)." : GoTo selesai
            End If

            'dijam2(10) As Date
            If (IsDate(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - dijam2 required time format (H:mm:ss)." : GoTo selesai
            End If

            'dicustomint1(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - dicustomint1 required numeric." : GoTo selesai
            End If
            'dicustomint2(17) As Integer
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - dicustomint2 required numeric." : GoTo selesai
            End If
            'dicustomint3(18) As Integer
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - dicustomint3 required numeric." : GoTo selesai
            End If
            'dicustomdbl1(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - dicustomdbl1 required numeric." : GoTo selesai
            End If
            'dicustomdbl2(20) As Double
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - dicustomdbl2 required numeric." : GoTo selesai
            End If
            'dicustomdbl3(21) As Double
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - dicustomdbl3 required numeric." : GoTo selesai
            End If
            'dicustomdate1(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - dicustomdate1 required date." : GoTo selesai
            End If
            'dicustomdate2(23) As Date
            If (IsDate(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - dicustomdate2 required date." : GoTo selesai
            End If
            'dicustomdate3(24) As Date
            If (IsDate(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - dicustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'dikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - dikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - dikategori should not be more than 25 character." : GoTo selesai
            End If

            'diidbarang(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - diidbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - diidbarang should not be more than 20 character." : GoTo selesai
            End If

            'dioperator(2) As String
            If IsNumeric(dataRowDetail(2)) = False Then
                result(2) = "Row : " & i & " - dioperator required numeric" : GoTo selesai
            ElseIf dataRowDetail(2) <> 0 And dataRowDetail(2) <> 1 And dataRowDetail(2) <> 2 Then
                result(2) = "Row : " & i & " - invalid dioperator value" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - dioperator should not be more than 25 character." : GoTo selesai
            End If

            'dijml1(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - dijml1 can't be empty" : GoTo selesai
            End If

            'dijml2(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - dijml2 can't be empty" : GoTo selesai
            End If

            'dinilai(6) As String
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - dinilai can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(6)) > 25 Then
                result(2) = "Row : " & i & " - dinilai should not be more than 25 character." : GoTo selesai
            End If

            'ditgl1(7) As Date
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - ditgl1 can't be empty" : GoTo selesai
            End If

            'ditgl2(8) As Date
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - ditgl2 can't be empty" : GoTo selesai
            End If

            'dijam1(9) As Date
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - dijam1 can't be empty" : GoTo selesai
            End If

            'dijam2(10) As Date
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - dijam2 can't be empty" : GoTo selesai
            End If

            'dicustomdbl1(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - dicustomdbl1 can't be empty" : GoTo selesai
            End If

            'dicustomdbl2(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - dicustomdbl2 can't be empty" : GoTo selesai
            End If

            'dicustomdbl3(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - dicustomdbl3 can't be empty" : GoTo selesai
            End If

            'dicustomdate1(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - dicustomdate1 can't be empty" : GoTo selesai
            End If

            'dicustomdate2(23) As Date
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - dicustomdate2 can't be empty" : GoTo selesai
            End If

            'dicustomdate3(24) As Date
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - dicustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "dikategori~diidbarang~dioperator~dijml1~dijml2~dikriteria~dinilai~ditgl1~ditgl2~dijam1~dijam2~dicustomtext1~dicustomtext2~dicustomtext3~dicustomtext4~dicustomtext5~dicustomint1~dicustomint2~dicustomint3~dicustomdbl1~dicustomdbl2~dicustomdbl3~dicustomdate1~dicustomdate2~dicustomdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24)) = False Then
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
                ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drutama("dikategori")) & "' "

                'HAPUS DATA KATEGORI DAN BARANG YANG SAMA
                sql = "DELETE FROM m_12_pos_discount_item WHERE dikategori = '" & FixQuotes(drutama("dikategori")) & "' AND diidbarang = '" & FixQuotes(drutama("diidbarang")) & "'"
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
                    sql = "SELECT di.dikategori as kategori, di.diidbarang as idbarang, di.dioperator as operator, i.bkode, (CASE di.dioperator WHEN 0 THEN 'Between' WHEN 1 THEN '>=' WHEN 2 THEN 'Multiple' ELSE 'Unknown' END) as operatornama FROM m_12_pos_discount_item di JOIN m1_item i ON di.diidbarang = i.bid WHERE di.dikategori = '" & FxDB(dr1("dikategori"), "") & "' AND di.diidbarang = '" & FxDB(dr1("diidbarang"), "") & "' GROUP BY di.dioperator ORDER BY di.dioperator"
                    dtOperator = AsDataTableAmbilDariDB(sql)
                    If dtOperator.Rows.Count > 0 Then
                        For Each dr2 As DataRow In dtOperator.Rows
                            vOperator = FxDB(dr2("operator").ToString, "")
                            If Len(vOperator) > 0 Then
                                If vOperator = 2 Then
                                    'JIKA SUDAH TERDAPAT OPERATOR KELIPATAN (2)
                                    result(2) = "Item : " & FxDB(dr2("bkode"), "") & " - already has '" & FxDB(dr2("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                Else
                                    'JIKA BELUM TERDAPAT OPERATOR KELIPATAN (2), CEK KONDISI OPERATOR YANG SUDAH DIINPUTKAN
                                    'JIKA OPERATOR YANG DIINPUTKAN ADALAH KELIPATAN (2) MAKA TAMPILKAN ERRMESSAGE
                                    'JIKA SUDAH TERDAPAT OPERATOR >= (1) DAN YANG DIINPUTKAN ADALAH OPERATOR >= (1) LAGI MAKA TAMPILKAN ERRMESSAGE
                                    If dr1("dioperator") = 2 Or (vOperator = 1 And dr1("dioperator") = vOperator) Then
                                        result(2) = "Item : " & FxDB(dr2("bkode"), "") & " - already has '" & FxDB(dr2("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                    End If
                                End If
                            End If
                        Next
                    End If

                    'strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Clear()
                    strValue2.Append("('" & FixQuotes(dr1("dikategori")) & "', '" & FixQuotes(dr1("diidbarang")) & "', '" & FixQuotes(dr1("dioperator")) & "', '" & FixDouble(dr1("dijml1")) & "', '" & FixDouble(dr1("dijml2")) & "', " & dr1("dikriteria") & ", '" & FixQuotes(dr1("dinilai")) & "', '" & FixQuotes(AsFormatTanggal(dr1("ditgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("ditgl2"))) & "', '" & FixQuotes(dr1("dijam1")) & "', '" & FixQuotes(dr1("dijam2")) & "', '" & FixQuotes(dr1("dicustomtext1")) & "', '" & FixQuotes(dr1("dicustomtext2")) & "', '" & FixQuotes(dr1("dicustomtext3")) & "', '" & FixQuotes(dr1("dicustomtext4")) & "', '" & FixQuotes(dr1("dicustomtext5")) & "', " & dr1("dicustomint1") & ", " & dr1("dicustomint2") & ", " & dr1("dicustomint3") & ", '" & FixDouble(dr1("dicustomdbl1")) & "', '" & FixDouble(dr1("dicustomdbl2")) & "', '" & FixDouble(dr1("dicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("dicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dicustomdate3"))) & "')")

                    sql = "Insert into M_12_Pos_Discount_Item(dikategori, diidbarang, dioperator, dijml1, dijml2, dikriteria, dinilai, ditgl1, ditgl2, dijam1, dijam2, dicustomtext1, dicustomtext2, dicustomtext3, dicustomtext4, dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, dicustomdbl3, dicustomdate1, dicustomdate2, dicustomdate3) values" & strValue2.ToString & ""
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
            Dim paramSearch As String = M12_Pos_Discount_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Discount_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Discount_ItemDeleteOld(ByVal param As String) As String

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
        Dim dikategori As String = "", diidbarang As String = "", dioperator As String = "", dijml1 As String = "", dijml2 As String = ""
        Dim idtrans(3) As String
        idtrans = paramSplit(5).Split(sptSubParam)
        If (idtrans.Length <> 5) Then
            result(2) = "Invalid primary key parameter." : GoTo selesai
        Else
            'CEK dikategori
            If (Len(idtrans(0)) = 0) Then
                result(2) = "dikategori can't be empty." : GoTo selesai
            Else
                dikategori = idtrans(0)
            End If
            'CEK diidbarang
            If (IsNumeric(idtrans(1)) = False) Then
                result(2) = "diidbarang required numeric." : GoTo selesai
            Else
                diidbarang = idtrans(1)
            End If
            'CEK dioperator
            If (Len(idtrans(2)) = 0) Then
                result(2) = "dioperator can't be empty." : GoTo selesai
            Else
                dioperator = idtrans(2)
            End If
            'CEK dijml1
            If (IsNumeric(idtrans(3)) = False) Then
                result(2) = "dijml1 required numeric." : GoTo selesai
            Else
                dijml1 = idtrans(3)
            End If
            'CEK dijml2
            If (IsNumeric(idtrans(4)) = False) Then
                result(2) = "dijml2 required numeric." : GoTo selesai
            Else
                dijml2 = idtrans(4)
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
            sql = "SELECT dikategori as kategoripos FROM M_12_Pos_Discount_Item WHERE dikategori = '" & dikategori & "' AND diidbarang = '" & diidbarang & "' AND dioperator = '" & dioperator & "' AND dijml1 = '" & dijml1 & "' AND dijml2 = '" & dijml2 & "' GROUP BY dikategori"
            Dim dtKategoriPOS As DataTable = AsDataTableAmbilDariDB(sql)
            If dtKategoriPOS.Rows.Count > 0 Then
                For Each drKategoriPOS As DataRow In dtKategoriPOS.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drKategoriPOS("kategoripos")) & "' "
                Next
            End If

            'DELETE
            sql = "DELETE FROM M_12_Pos_Discount_Item WHERE dikategori = '" & dikategori & "' AND diidbarang = '" & diidbarang & "' AND dioperator = '" & dioperator & "' AND dijml1 = '" & dijml1 & "' AND dijml2 = '" & dijml2 & "'"
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
            Dim paramSearch As String = M12_Pos_Discount_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Discount_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Discount_ItemImportOld(ByVal param As String) As String
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
        'dikategori(0) As String, diidbarang(1) As Double, dioperator(2) As String, dijml1(3) As Double, dijml2(4) As Double, 
        'dikriteria(5) As Integer, dinilai(6) As String, ditgl1(7) As Date, ditgl2(8) As Date, dijam1(9) As Date, 
        'dijam2(10) As Date, dicustomtext1(11) As String, dicustomtext2(12) As String, dicustomtext3(13) As String, dicustomtext4(14) As String, 
        'dicustomtext5(15) As String, dicustomint1(16) As Integer, dicustomint2(17) As Integer, dicustomint3(18) As Integer, dicustomdbl1(19) As Double, 
        'dicustomdbl2(20) As Double, dicustomdbl3(21) As Double, dicustomdate1(22) As Date, dicustomdate2(23) As Date, dicustomdate3(24) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'dikategori, diidbarang, dioperator, dijml1, dijml2, dikriteria, dinilai, 
        'ditgl1, ditgl2, dijam1, dijam2, dicustomtext1, dicustomtext2, dicustomtext3, 
        'dicustomtext4, dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, 
        'dicustomdbl3, dicustomdate1, dicustomdate2, dicustomdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "dikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dioperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dijml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dijml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dikriteria", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dinilai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ditgl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ditgl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dijam1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dijam2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dicustomdate3", AsEnumTypeData.AsString)

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
            'diidbarang(1) As Double
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - diidbarang required numeric." : GoTo selesai
            End If
            'dijml1(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - dijml1 required numeric." : GoTo selesai
            End If
            'dijml2(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - dijml2 required numeric." : GoTo selesai
            End If
            'dikriteria(5) As Integer
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - dikriteria required numeric." : GoTo selesai
            ElseIf dataRowDetail(5) <> 0 And dataRowDetail(5) <> 1 And dataRowDetail(5) <> 2 Then
                result(2) = "Row : " & i & " - invalid dikriteria value" : GoTo selesai
            End If
            'ditgl1(7) As Date
            If (IsDate(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - ditgl1 required date." : GoTo selesai
            End If
            'ditgl2(8) As Date
            If (IsDate(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - ditgl2 required date." : GoTo selesai
            End If
            'dijam1(9) As Date
            If (IsDate(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - dijam1 required date." : GoTo selesai
            End If
            'dijam2(10) As Date
            If (IsDate(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - dijam2 required date." : GoTo selesai
            End If
            'dicustomint1(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - dicustomint1 required numeric." : GoTo selesai
            End If
            'dicustomint2(17) As Integer
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - dicustomint2 required numeric." : GoTo selesai
            End If
            'dicustomint3(18) As Integer
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - dicustomint3 required numeric." : GoTo selesai
            End If
            'dicustomdbl1(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - dicustomdbl1 required numeric." : GoTo selesai
            End If
            'dicustomdbl2(20) As Double
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - dicustomdbl2 required numeric." : GoTo selesai
            End If
            'dicustomdbl3(21) As Double
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - dicustomdbl3 required numeric." : GoTo selesai
            End If
            'dicustomdate1(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - dicustomdate1 required date." : GoTo selesai
            End If
            'dicustomdate2(23) As Date
            If (IsDate(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - dicustomdate2 required date." : GoTo selesai
            End If
            'dicustomdate3(24) As Date
            If (IsDate(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - dicustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'dikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - dikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - dikategori should not be more than 25 character." : GoTo selesai
            End If

            'diidbarang(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - diidbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - diidbarang should not be more than 20 character." : GoTo selesai
            End If

            'dioperator(2) As String
            If IsNumeric(dataRowDetail(2)) = False Then
                result(2) = "Row : " & i & " - dioperator required numeric" : GoTo selesai
            ElseIf dataRowDetail(2) <> 0 And dataRowDetail(2) <> 1 And dataRowDetail(2) <> 2 Then
                result(2) = "Row : " & i & " - invalid dioperator value" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - dioperator should not be more than 25 character." : GoTo selesai
            End If

            'dijml1(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - dijml1 can't be empty" : GoTo selesai
            End If

            'dijml2(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - dijml2 can't be empty" : GoTo selesai
            End If

            'dinilai(6) As String
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - dinilai can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(6)) > 25 Then
                result(2) = "Row : " & i & " - dinilai should not be more than 25 character." : GoTo selesai
            End If

            'ditgl1(7) As Date
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - ditgl1 can't be empty" : GoTo selesai
            End If

            'ditgl2(8) As Date
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - ditgl2 can't be empty" : GoTo selesai
            End If

            'dijam1(9) As Date
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - dijam1 can't be empty" : GoTo selesai
            End If

            'dijam2(10) As Date
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - dijam2 can't be empty" : GoTo selesai
            End If

            'dicustomdbl1(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - dicustomdbl1 can't be empty" : GoTo selesai
            End If

            'dicustomdbl2(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - dicustomdbl2 can't be empty" : GoTo selesai
            End If

            'dicustomdbl3(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - dicustomdbl3 can't be empty" : GoTo selesai
            End If

            'dicustomdate1(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - dicustomdate1 can't be empty" : GoTo selesai
            End If

            'dicustomdate2(23) As Date
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - dicustomdate2 can't be empty" : GoTo selesai
            End If

            'dicustomdate3(24) As Date
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - dicustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "dikategori~diidbarang~dioperator~dijml1~dijml2~dikriteria~dinilai~ditgl1~ditgl2~dijam1~dijam2~dicustomtext1~dicustomtext2~dicustomtext3~dicustomtext4~dicustomtext5~dicustomint1~dicustomint2~dicustomint3~dicustomdbl1~dicustomdbl2~dicustomdbl3~dicustomdate1~dicustomdate2~dicustomdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24)) = False Then
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
                    strValue2.Append("('" & FixQuotes(dr1("dikategori")) & "', '" & FixQuotes(dr1("diidbarang")) & "', '" & FixQuotes(dr1("dioperator")) & "', '" & FixDouble(dr1("dijml1")) & "', '" & FixDouble(dr1("dijml2")) & "', " & dr1("dikriteria") & ", '" & FixQuotes(dr1("dinilai")) & "', '" & FixQuotes(AsFormatTanggal(dr1("ditgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("ditgl2"))) & "', '" & FixQuotes(dr1("dijam1")) & "', '" & FixQuotes(dr1("dijam2")) & "', '" & FixQuotes(dr1("dicustomtext1")) & "', '" & FixQuotes(dr1("dicustomtext2")) & "', '" & FixQuotes(dr1("dicustomtext3")) & "', '" & FixQuotes(dr1("dicustomtext4")) & "', '" & FixQuotes(dr1("dicustomtext5")) & "', " & dr1("dicustomint1") & ", " & dr1("dicustomint2") & ", " & dr1("dicustomint3") & ", '" & FixDouble(dr1("dicustomdbl1")) & "', '" & FixDouble(dr1("dicustomdbl2")) & "', '" & FixDouble(dr1("dicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("dicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dicustomdate3"))) & "')")
                Next

                If Len(strValue2.ToString) > 0 Then
                    'DELETE
                    sql = "DELETE FROM M_12_Pos_Discount_Item"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'insert jika data belum ada, dan update jika data sudah ada
                    sql = "Insert into M_12_Pos_Discount_Item(dikategori, diidbarang, dioperator, dijml1, dijml2, dikriteria, dinilai, ditgl1, ditgl2, dijam1, dijam2, dicustomtext1, dicustomtext2, dicustomtext3, dicustomtext4, dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, dicustomdbl3, dicustomdate1, dicustomdate2, dicustomdate3) values" & strValue2.ToString & ""
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
            Dim paramSearch As String = M12_Pos_Discount_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Discount_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Discount_ItemSearch(ByVal param As String) As String
        'JIKA MENAMBAHKAN FIELD DISINI MAKA HARUS TAMBAHKAN JUGA LENGTH ARRAY PADA VALIDASI AMBIL USERID WS M0_LOGIN
        'M12_Pos_Discount_ItemSearch --------------------------------------------------------
        'dikategori, diidbarang, dioperator, dijml1, dijml2, dikriteria, dinilai, 
        'ditgl1, ditgl2, dijam1, dijam2, dicustomtext1, dicustomtext2, dicustomtext3, 
        'dicustomtext4, dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, 
        'dicustomdbl3, dicustomdate1, dicustomdate2, dicustomdate3, pcnama, bkode, bnama, 
        'btipe, bsatuan, dikriterianama, dioperatornama

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
        sql = "select `di`.`dikategori` AS `dikategori`,`di`.`diidbarang` AS `diidbarang`,`di`.`dioperator` AS `dioperator`,`di`.`dijml1` AS `dijml1`,`di`.`dijml2` AS `dijml2`,`di`.`dikriteria` AS `dikriteria`,`di`.`dinilai` AS `dinilai`,`di`.`ditgl1` AS `ditgl1`,`di`.`ditgl2` AS `ditgl2`,`di`.`dijam1` AS `dijam1`,`di`.`dijam2` AS `dijam2`,`di`.`dicustomtext1` AS `dicustomtext1`,`di`.`dicustomtext2` AS `dicustomtext2`,`di`.`dicustomtext3` AS `dicustomtext3`,`di`.`dicustomtext4` AS `dicustomtext4`,`di`.`dicustomtext5` AS `dicustomtext5`,`di`.`dicustomint1` AS `dicustomint1`,`di`.`dicustomint2` AS `dicustomint2`,`di`.`dicustomint3` AS `dicustomint3`,`di`.`dicustomdbl1` AS `dicustomdbl1`,`di`.`dicustomdbl2` AS `dicustomdbl2`,`di`.`dicustomdbl3` AS `dicustomdbl3`,`di`.`dicustomdate1` AS `dicustomdate1`,`di`.`dicustomdate2` AS `dicustomdate2`,`di`.`dicustomdate3` AS `dicustomdate3`,`pc`.`pcnama` AS `pcnama`,`i`.`bkode` AS `bkode`,`i`.`bnama` AS `bnama`,`i`.`btipe` AS `btipe`,`i`.`bsatuan` AS `bsatuan`,(case `di`.`dikriteria` when 0 then 'Price' when 1 then 'Discount Percent' when 2 then 'Discount Nominal' else 'Unknown' end) AS `dikriterianama`,(case `di`.`dioperator` when 0 then 'Between' when 1 then '>=' when 2 then 'Multiple' else 'Unknown' end) AS `dioperatornama` from ((`m_12_pos_discount_item` `di` join `m_12_pos_category` `pc` on((`di`.`dikategori` = `pc`.`pckode`))) join `m1_item` `i` on((`di`.`diidbarang` = `i`.`bid`)))"
        'result(2) = sql : GoTo selesai
        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M_12_Pos_Discount_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("dikategori"), ""), sptField,
                     FxDB(dr("diidbarang"), ""), sptField,
                     FxDB(dr("dioperator"), ""), sptField,
                     FxDB(dr("dijml1"), 0), sptField,
                     FxDB(dr("dijml2"), 0), sptField,
                     FxDB(dr("dikriteria"), 0), sptField,
                     FxDB(dr("dinilai"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ditgl1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ditgl2"), ""), formatTgl), sptField,
                     FxDB(dr("dijam1").ToString, ""), sptField,
                     FxDB(dr("dijam2").ToString, ""), sptField,
                     FxDB(dr("dicustomtext1"), ""), sptField,
                     FxDB(dr("dicustomtext2"), ""), sptField,
                     FxDB(dr("dicustomtext3"), ""), sptField,
                     FxDB(dr("dicustomtext4"), ""), sptField,
                     FxDB(dr("dicustomtext5"), ""), sptField,
                     FxDB(dr("dicustomint1"), 0), sptField,
                     FxDB(dr("dicustomint2"), 0), sptField,
                     FxDB(dr("dicustomint3"), 0), sptField,
                     FxDB(dr("dicustomdbl1"), 0), sptField,
                     FxDB(dr("dicustomdbl2"), 0), sptField,
                     FxDB(dr("dicustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("dicustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("dicustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("dicustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("pcnama"), ""), sptField,
                     FxDB(dr("bkode"), ""), sptField,
                     FxDB(dr("bnama"), ""), sptField,
                     FxDB(dr("btipe"), ""), sptField,
                     FxDB(dr("bsatuan"), ""), sptField,
                     FxDB(dr("dikriterianama"), ""), sptField,
                     FxDB(dr("dioperatornama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Discount Item data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("dikategori, diidbarang, dioperator, dijml1, dijml2, dikriteria, dinilai, ditgl1, ditgl2, dijam1, dijam2, dicustomtext1, dicustomtext2, dicustomtext3, dicustomtext4, dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, dicustomdbl3, dicustomdate1, dicustomdate2, dicustomdate3, pcnama, bkode, bnama, btipe, bsatuan, dikriterianama, dioperatornama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_Discount_ItemDownload(ByVal param As String) As String
        'M12_Pos_Discount_ItemDownload --------------------------------------------------------
        'dikategori, diidbarang, dioperator, dijml1, dijml2, dikriteria, dinilai, 
        'ditgl1, ditgl2, dijam1, dijam2, dicustomtext1, dicustomtext2, dicustomtext3, 
        'dicustomtext4, dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, 
        'dicustomdbl3, dicustomdate1, dicustomdate2, dicustomdate3

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

        dt = AmbilData("aplikasi1-M_12_Pos_Discount_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("dikategori"), ""), sptField,
                     FxDB(dr("diidbarang"), ""), sptField,
                     FxDB(dr("dioperator"), ""), sptField,
                     FxDB(dr("dijml1"), 0), sptField,
                     FxDB(dr("dijml2"), 0), sptField,
                     FxDB(dr("dikriteria"), 0), sptField,
                     FxDB(dr("dinilai"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ditgl1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ditgl2"), ""), formatTgl), sptField,
                     FxDB(dr("dijam1"), ""), sptField,
                     FxDB(dr("dijam2"), ""), sptField,
                     FxDB(dr("dicustomtext1"), ""), sptField,
                     FxDB(dr("dicustomtext2"), ""), sptField,
                     FxDB(dr("dicustomtext3"), ""), sptField,
                     FxDB(dr("dicustomtext4"), ""), sptField,
                     FxDB(dr("dicustomtext5"), ""), sptField,
                     FxDB(dr("dicustomint1"), 0), sptField,
                     FxDB(dr("dicustomint2"), 0), sptField,
                     FxDB(dr("dicustomint3"), 0), sptField,
                     FxDB(dr("dicustomdbl1"), 0), sptField,
                     FxDB(dr("dicustomdbl2"), 0), sptField,
                     FxDB(dr("dicustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("dicustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("dicustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("dicustomdate3"), ""), formatTgl), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Discount Item data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("dikategori, diidbarang, dioperator, dijml1, dijml2, dikriteria, dinilai, ditgl1, ditgl2, dijam1, dijam2, dicustomtext1, dicustomtext2, dicustomtext3, dicustomtext4, dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, dicustomdbl3, dicustomdate1, dicustomdate2, dicustomdate3"))

        Return wsResult
    End Function

End Class
