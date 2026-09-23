Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m1_price_category
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M1_Price_CategorySimpan(ByVal param As String) As String
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

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'pckode(0) As String, pcnama(1) As String, pccatatan(2) As String, pcaktif(3) As Integer, pcinputuser(4) As Integer, 
        'pcinputtgl(5) As DateTime, pcmodifikasiuser(6) As Integer, pcmodifikasitgl(7) As DateTime, pccustomtext1(8) As String, pccustomtext2(9) As String, 
        'pccustomtext3(10) As String, pccustomtext4(11) As String, pccustomtext5(12) As String, pccustomint1(13) As Integer, pccustomint2(14) As Integer, 
        'pccustomint3(15) As Integer, pccustomdbl1(16) As Double, pccustomdbl2(17) As Double, pccustomdbl3(18) As Double, pccustomdate1(19) As Date, 
        'pccustomdate2(20) As Date, pccustomdate3(21) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'pckode, pcnama, pccatatan, pcaktif, pcinputuser, pcinputtgl, pcmodifikasiuser, 
        'pcmodifikasitgl, pccustomtext1, pccustomtext2, pccustomtext3, pccustomtext4, pccustomtext5, pccustomint1, 
        'pccustomint2, pccustomint3, pccustomdbl1, pccustomdbl2, pccustomdbl3, pccustomdate1, pccustomdate2, 
        'pccustomdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pckode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pccatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcaktif", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pcinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pcinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pcmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pccustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pccustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pccustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pccustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pccustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pccustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pccustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pccustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pccustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pccustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pccustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pccustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pccustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pccustomdate3", AsEnumTypeData.AsString)

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
            If (dataRowDetail.Length <> 22) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'pcaktif(3) As Integer
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - pcaktif required numeric." : GoTo selesai
            End If
            'pcinputuser(4) As Integer
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - pcinputuser required numeric." : GoTo selesai
            End If
            'pcinputtgl(5) As DateTime
            If (IsDate(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - pcinputtgl required date." : GoTo selesai
            End If
            'pcmodifikasiuser(6) As Integer
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - pcmodifikasiuser required numeric." : GoTo selesai
            End If
            'pcmodifikasitgl(7) As DateTime
            If (IsDate(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - pcmodifikasitgl required date." : GoTo selesai
            End If
            'pccustomint1(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - pccustomint1 required numeric." : GoTo selesai
            End If
            'pccustomint2(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - pccustomint2 required numeric." : GoTo selesai
            End If
            'pccustomint3(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - pccustomint3 required numeric." : GoTo selesai
            End If
            'pccustomdbl1(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - pccustomdbl1 required numeric." : GoTo selesai
            End If
            'pccustomdbl2(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - pccustomdbl2 required numeric." : GoTo selesai
            End If
            'pccustomdbl3(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - pccustomdbl3 required numeric." : GoTo selesai
            End If
            'pccustomdate1(19) As Date
            If (IsDate(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - pccustomdate1 required date." : GoTo selesai
            End If
            'pccustomdate2(20) As Date
            If (IsDate(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - pccustomdate2 required date." : GoTo selesai
            End If
            'pccustomdate3(21) As Date
            If (IsDate(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - pccustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'pckode(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pckode can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pckode should not be more than 25 character." : GoTo selesai
            End If

            'pcnama(1) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - pcnama can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 100 Then
                result(2) = "Row : " & i & " - pcnama should not be more than 100 character." : GoTo selesai
            End If

            'pcinputtgl(5) As DateTime
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - pcinputtgl can't be empty" : GoTo selesai
            End If

            'pcmodifikasitgl(7) As DateTime
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - pcmodifikasitgl can't be empty" : GoTo selesai
            End If

            'pccustomdbl1(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - pccustomdbl1 can't be empty" : GoTo selesai
            End If

            'pccustomdbl2(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - pccustomdbl2 can't be empty" : GoTo selesai
            End If

            'pccustomdbl3(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - pccustomdbl3 can't be empty" : GoTo selesai
            End If

            'pccustomdate1(19) As Date
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - pccustomdate1 can't be empty" : GoTo selesai
            End If

            'pccustomdate2(20) As Date
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - pccustomdate2 can't be empty" : GoTo selesai
            End If

            'pccustomdate3(21) As Date
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - pccustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pckode~pcnama~pccatatan~pcaktif~pcinputuser~pcinputtgl~pcmodifikasiuser~pcmodifikasitgl~pccustomtext1~pccustomtext2~pccustomtext3~pccustomtext4~pccustomtext5~pccustomint1~pccustomint2~pccustomint3~pccustomdbl1~pccustomdbl2~pccustomdbl3~pccustomdate1~pccustomdate2~pccustomdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21)) = False Then
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

                If isUpdate Then
                    For Each dr1 As DataRow In dtdetail.Rows

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m1_price_category_history
                        Dim PriceSimpanHistory As String = SimpanHistory.M1_Price_CategoryHistorySimpan("" & paramSplit(0) & "★M1_Price_CategoryHistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(dr1("pckode")) & "")
                        Dim PriceSplit() As String = PriceSimpanHistory.Split(sptParam)
                        Dim PriceSplitResult() As String = PriceSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (PriceSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & PriceSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("pckode")) & "', '" & FixQuotes(dr1("pcnama")) & "', '" & FixQuotes(dr1("pccatatan")) & "', " & dr1("pcaktif") & ", " & dr1("pcinputuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("pcinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("pcmodifikasiuser") & ", NOW(), '" & FixQuotes(dr1("pccustomtext1")) & "', '" & FixQuotes(dr1("pccustomtext2")) & "', '" & FixQuotes(dr1("pccustomtext3")) & "', '" & FixQuotes(dr1("pccustomtext4")) & "', '" & FixQuotes(dr1("pccustomtext5")) & "', " & dr1("pccustomint1") & ", " & dr1("pccustomint2") & ", " & dr1("pccustomint3") & ", '" & FixDouble(dr1("pccustomdbl1")) & "', '" & FixDouble(dr1("pccustomdbl2")) & "', '" & FixDouble(dr1("pccustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("pccustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("pccustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("pccustomdate3"))) & "')")
                    Next

                Else
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("pckode")) & "', '" & FixQuotes(dr1("pcnama")) & "', '" & FixQuotes(dr1("pccatatan")) & "', " & dr1("pcaktif") & ", " & dr1("pcinputuser") & ", NOW(), " & dr1("pcmodifikasiuser") & ", '1971-01-01 00:00:00', '" & FixQuotes(dr1("pccustomtext1")) & "', '" & FixQuotes(dr1("pccustomtext2")) & "', '" & FixQuotes(dr1("pccustomtext3")) & "', '" & FixQuotes(dr1("pccustomtext4")) & "', '" & FixQuotes(dr1("pccustomtext5")) & "', " & dr1("pccustomint1") & ", " & dr1("pccustomint2") & ", " & dr1("pccustomint3") & ", '" & FixDouble(dr1("pccustomdbl1")) & "', '" & FixDouble(dr1("pccustomdbl2")) & "', '" & FixDouble(dr1("pccustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("pccustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("pccustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("pccustomdate3"))) & "')")
                    Next
                End If

                If Len(strValue2.ToString) > 0 Then
                    sql = "Insert into M1_Price_Category(pckode, pcnama, pccatatan, pcaktif, pcinputuser, pcinputtgl, pcmodifikasiuser, pcmodifikasitgl, pccustomtext1, pccustomtext2, pccustomtext3, pccustomtext4, pccustomtext5, pccustomint1, pccustomint2, pccustomint3, pccustomdbl1, pccustomdbl2, pccustomdbl3, pccustomdate1, pccustomdate2, pccustomdate3) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE pcnama = VALUES(pcnama), pccatatan = VALUES(pccatatan), pcaktif = VALUES(pcaktif), pcmodifikasiuser = VALUES(pcmodifikasiuser), pcmodifikasitgl = NOW(), pccustomtext1 = VALUES(pccustomtext1), pccustomtext2 = VALUES(pccustomtext2), pccustomtext3 = VALUES(pccustomtext3), pccustomtext4 = VALUES(pccustomtext4), pccustomtext5 = VALUES(pccustomtext5), pccustomint1 = VALUES(pccustomint1), pccustomint2 = VALUES(pccustomint2), pccustomint3 = VALUES(pccustomint3), pccustomdbl1 = VALUES(pccustomdbl1), pccustomdbl2 = VALUES(pccustomdbl2), pccustomdbl3 = VALUES(pccustomdbl3), pccustomdate1 = VALUES(pccustomdate1), pccustomdate2 = VALUES(pccustomdate2), pccustomdate3 = VALUES(pccustomdate3)"
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
            Dim paramSearch As String = M1_Price_CategorySearch(PostWsSearch(paramSplit(0), "M1_Price_CategorySearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_Price_CategoryDelete(ByVal param As String) As String

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

        Dim pg1 As New RsPaging

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
        If (Len(paramSplit(5)) = 0) Then
            result(2) = "pckode can't be empty." : GoTo selesai
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

            'CEK TERKAIT =============================================================
            Dim paramTerkait As String = M1_Price_CategoryTerkait(PostWsTerkait(paramSplit(0), "M1_Price_CategoryTerkait", pagingSplit(0), pagingSplit(1), "", "", formatTgl, formatTglWaktu, idtransaksi))
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

            'SIMPAN HISTORY ========================
            Dim SimpanHistory As New m1_price_category_history
            Dim PriceSimpanHistory As String = SimpanHistory.M1_Price_CategoryHistorySimpan("" & paramSplit(0) & "★M1_Price_CategoryHistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(idtransaksi) & "")
            Dim PriceSplit() As String = PriceSimpanHistory.Split(sptParam)
            Dim PriceSplitResult() As String = PriceSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (PriceSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & PriceSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            'DELETE
            sql = "DELETE FROM M1_Price_Category WHERE pckode = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M1_Price_CategorySearch(PostWsSearch(paramSplit(0), "M1_Price_CategorySearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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

    <WebMethod()>
    Public Function M1_Price_CategorySearch(ByVal param As String) As String
        'M1_Price_CategorySearch --------------------------------------------------------
        'pckode, pcnama, pccatatan, pcaktif, pcinputuser, pcinputtgl, pcmodifikasiuser, 
        'pcmodifikasitgl, pccustomtext1, pccustomtext2, pccustomtext3, pccustomtext4, pccustomtext5, pccustomint1, 
        'pccustomint2, pccustomint3, pccustomdbl1, pccustomdbl2, pccustomdbl3, pccustomdate1, pccustomdate2, 
        'pccustomdate3, pcinputusernama, pcmodifikasiusernama

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
        sql = "select `pc`.`pckode` AS `pckode`,`pc`.`pcnama` AS `pcnama`,`pc`.`pccatatan` AS `pccatatan`,`pc`.`pcaktif` AS `pcaktif`,`pc`.`pcinputuser` AS `pcinputuser`,`pc`.`pcinputtgl` AS `pcinputtgl`,`pc`.`pcmodifikasiuser` AS `pcmodifikasiuser`,`pc`.`pcmodifikasitgl` AS `pcmodifikasitgl`,`pc`.`pccustomtext1` AS `pccustomtext1`,`pc`.`pccustomtext2` AS `pccustomtext2`,`pc`.`pccustomtext3` AS `pccustomtext3`,`pc`.`pccustomtext4` AS `pccustomtext4`,`pc`.`pccustomtext5` AS `pccustomtext5`,`pc`.`pccustomint1` AS `pccustomint1`,`pc`.`pccustomint2` AS `pccustomint2`,`pc`.`pccustomint3` AS `pccustomint3`,`pc`.`pccustomdbl1` AS `pccustomdbl1`,`pc`.`pccustomdbl2` AS `pccustomdbl2`,`pc`.`pccustomdbl3` AS `pccustomdbl3`,`pc`.`pccustomdate1` AS `pccustomdate1`,`pc`.`pccustomdate2` AS `pccustomdate2`,`pc`.`pccustomdate3` AS `pccustomdate3`,`u1`.`unama` AS `pcinputusernama`,`u2`.`unama` AS `pcmodifikasiusernama` from ((`M1_Price_category` `pc` left join `m0_user` `u1` on((`pc`.`pcinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`pc`.`pcmodifikasiuser` = `u2`.`userid`)))"

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Price", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("pckode"), ""), sptField,
                     FxDB(dr("pcnama"), ""), sptField,
                     FxDB(dr("pccatatan"), ""), sptField,
                     FxDB(dr("pcaktif"), 0), sptField,
                     FxDB(dr("pcinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pcinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pcmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pcmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pccustomtext1"), ""), sptField,
                     FxDB(dr("pccustomtext2"), ""), sptField,
                     FxDB(dr("pccustomtext3"), ""), sptField,
                     FxDB(dr("pccustomtext4"), ""), sptField,
                     FxDB(dr("pccustomtext5"), ""), sptField,
                     FxDB(dr("pccustomint1"), 0), sptField,
                     FxDB(dr("pccustomint2"), 0), sptField,
                     FxDB(dr("pccustomint3"), 0), sptField,
                     FxDB(dr("pccustomdbl1"), 0), sptField,
                     FxDB(dr("pccustomdbl2"), 0), sptField,
                     FxDB(dr("pccustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pccustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("pccustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("pccustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("pcinputusernama"), ""), sptField,
                     FxDB(dr("pcmodifikasiusernama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Price Category data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pckode, pcnama, pccatatan, pcaktif, pcinputuser, pcinputtgl, pcmodifikasiuser, pcmodifikasitgl, pccustomtext1, pccustomtext2, pccustomtext3, pccustomtext4, pccustomtext5, pccustomint1, pccustomint2, pccustomint3, pccustomdbl1, pccustomdbl2, pccustomdbl3, pccustomdate1, pccustomdate2, pccustomdate3, pcinputusernama, pcmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Price_CategoryCekId(ByVal param As String) As String

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
            result(2) = "pckode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'CEK DI DATABASE ================================================================
        Dim dt As DataTable
        Dim exist As Integer = 0
        dt = AsDataTableAmbilDariDB("SELECT COUNT(pckode) FROM M1_Price_category WHERE pckode='" & idtransaksi & "'")
        exist = dt.Rows(0)(0)

        If (exist > 0) Then
            result(2) = "'" & idtransaksi & "' already exist for column akode." : GoTo selesai
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
    Public Function M1_Price_CategoryTerkait(ByVal param As String) As String
        'M1_Price_CategoryTerkait --------------------------------------------------------
        'pckode, pcnama, sumber, idterkait

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
            result(2) = "ackode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        sql = "SELECT pc.pckode, pc.pcnama, 'Price' as sumber, a.anama as idterkait FROM M1_Price a JOIN M1_Price_category pc ON a.akategori = pc.pckode WHERE pc.pckode = 'valkode' GROUP BY pc.pckode, a.akode"
        sql = sql.Replace("valkode", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("pckode"), ""), sptField,
                             FxDB(dr("pcnama"), ""), sptField,
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
            result(2) = "Related Price Category data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pckode, pcnama, sumber, idterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Price_CategoryDownload(ByVal param As String) As String
        'M1_Price_CategoryDownload --------------------------------------------------------
        'pckode, pcnama, pccatatan, pcaktif, pcinputuser, pcinputtgl, pcmodifikasiuser, 
        'pcmodifikasitgl, pccustomtext1, pccustomtext2, pccustomtext3, pccustomtext4, pccustomtext5, pccustomint1, 
        'pccustomint2, pccustomint3, pccustomdbl1, pccustomdbl2, pccustomdbl3, pccustomdate1, pccustomdate2, 
        'pccustomdate3

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
        sql = "select `pc`.`pckode` AS `pckode`,`pc`.`pcnama` AS `pcnama`,`pc`.`pccatatan` AS `pccatatan`,`pc`.`pcaktif` AS `pcaktif`,`pc`.`pcinputuser` AS `pcinputuser`,`pc`.`pcinputtgl` AS `pcinputtgl`,`pc`.`pcmodifikasiuser` AS `pcmodifikasiuser`,`pc`.`pcmodifikasitgl` AS `pcmodifikasitgl`,`pc`.`pccustomtext1` AS `pccustomtext1`,`pc`.`pccustomtext2` AS `pccustomtext2`,`pc`.`pccustomtext3` AS `pccustomtext3`,`pc`.`pccustomtext4` AS `pccustomtext4`,`pc`.`pccustomtext5` AS `pccustomtext5`,`pc`.`pccustomint1` AS `pccustomint1`,`pc`.`pccustomint2` AS `pccustomint2`,`pc`.`pccustomint3` AS `pccustomint3`,`pc`.`pccustomdbl1` AS `pccustomdbl1`,`pc`.`pccustomdbl2` AS `pccustomdbl2`,`pc`.`pccustomdbl3` AS `pccustomdbl3`,`pc`.`pccustomdate1` AS `pccustomdate1`,`pc`.`pccustomdate2` AS `pccustomdate2`,`pc`.`pccustomdate3` AS `pccustomdate3`,`u1`.`unama` AS `pcinputusernama`,`u2`.`unama` AS `pcmodifikasiusernama` from ((`M1_Price_category` `pc` left join `m0_user` `u1` on((`pc`.`pcinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`pc`.`pcmodifikasiuser` = `u2`.`userid`)))"

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Price", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("pckode"), ""), sptField,
                     FxDB(dr("pcnama"), ""), sptField,
                     FxDB(dr("pccatatan"), ""), sptField,
                     FxDB(dr("pcaktif"), 0), sptField,
                     FxDB(dr("pcinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pcinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pcmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pcmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pccustomtext1"), ""), sptField,
                     FxDB(dr("pccustomtext2"), ""), sptField,
                     FxDB(dr("pccustomtext3"), ""), sptField,
                     FxDB(dr("pccustomtext4"), ""), sptField,
                     FxDB(dr("pccustomtext5"), ""), sptField,
                     FxDB(dr("pccustomint1"), 0), sptField,
                     FxDB(dr("pccustomint2"), 0), sptField,
                     FxDB(dr("pccustomint3"), 0), sptField,
                     FxDB(dr("pccustomdbl1"), 0), sptField,
                     FxDB(dr("pccustomdbl2"), 0), sptField,
                     FxDB(dr("pccustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pccustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("pccustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("pccustomdate3"), ""), formatTgl), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Price Category data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pckode, pcnama, pccatatan, pcaktif, pcinputuser, pcinputtgl, pcmodifikasiuser, pcmodifikasitgl, pccustomtext1, pccustomtext2, pccustomtext3, pccustomtext4, pccustomtext5, pccustomint1, pccustomint2, pccustomint3, pccustomdbl1, pccustomdbl2, pccustomdbl3, pccustomdate1, pccustomdate2, pccustomdate3"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Price_CategoryImport(ByVal param As String) As String
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

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'pckode(0) As String, pcnama(1) As String, pccatatan(2) As String, pcaktif(3) As Integer, pcinputuser(4) As Integer, 
        'pcinputtgl(5) As DateTime, pcmodifikasiuser(6) As Integer, pcmodifikasitgl(7) As DateTime, pccustomtext1(8) As String, pccustomtext2(9) As String, 
        'pccustomtext3(10) As String, pccustomtext4(11) As String, pccustomtext5(12) As String, pccustomint1(13) As Integer, pccustomint2(14) As Integer, 
        'pccustomint3(15) As Integer, pccustomdbl1(16) As Double, pccustomdbl2(17) As Double, pccustomdbl3(18) As Double, pccustomdate1(19) As Date, 
        'pccustomdate2(20) As Date, pccustomdate3(21) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'pckode, pcnama, pccatatan, pcaktif, pcinputuser, pcinputtgl, pcmodifikasiuser, 
        'pcmodifikasitgl, pccustomtext1, pccustomtext2, pccustomtext3, pccustomtext4, pccustomtext5, pccustomint1, 
        'pccustomint2, pccustomint3, pccustomdbl1, pccustomdbl2, pccustomdbl3, pccustomdate1, pccustomdate2, 
        'pccustomdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pckode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pccatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcaktif", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pcinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pcinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pcmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pccustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pccustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pccustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pccustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pccustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pccustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pccustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pccustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pccustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pccustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pccustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pccustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pccustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pccustomdate3", AsEnumTypeData.AsString)

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
            If (dataRowDetail.Length <> 22) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'pcaktif(3) As Integer
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - pcaktif required numeric." : GoTo selesai
            End If
            'pcinputuser(4) As Integer
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - pcinputuser required numeric." : GoTo selesai
            End If
            'pcinputtgl(5) As DateTime
            If (IsDate(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - pcinputtgl required date." : GoTo selesai
            End If
            'pcmodifikasiuser(6) As Integer
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - pcmodifikasiuser required numeric." : GoTo selesai
            End If
            'pcmodifikasitgl(7) As DateTime
            If (IsDate(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - pcmodifikasitgl required date." : GoTo selesai
            End If
            'pccustomint1(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - pccustomint1 required numeric." : GoTo selesai
            End If
            'pccustomint2(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - pccustomint2 required numeric." : GoTo selesai
            End If
            'pccustomint3(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - pccustomint3 required numeric." : GoTo selesai
            End If
            'pccustomdbl1(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - pccustomdbl1 required numeric." : GoTo selesai
            End If
            'pccustomdbl2(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - pccustomdbl2 required numeric." : GoTo selesai
            End If
            'pccustomdbl3(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - pccustomdbl3 required numeric." : GoTo selesai
            End If
            'pccustomdate1(19) As Date
            If (IsDate(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - pccustomdate1 required date." : GoTo selesai
            End If
            'pccustomdate2(20) As Date
            If (IsDate(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - pccustomdate2 required date." : GoTo selesai
            End If
            'pccustomdate3(21) As Date
            If (IsDate(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - pccustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'pckode(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pckode can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pckode should not be more than 25 character." : GoTo selesai
            End If

            'pcnama(1) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - pcnama can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 100 Then
                result(2) = "Row : " & i & " - pcnama should not be more than 100 character." : GoTo selesai
            End If

            'pcinputtgl(5) As DateTime
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - pcinputtgl can't be empty" : GoTo selesai
            End If

            'pcmodifikasitgl(7) As DateTime
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - pcmodifikasitgl can't be empty" : GoTo selesai
            End If

            'pccustomdbl1(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - pccustomdbl1 can't be empty" : GoTo selesai
            End If

            'pccustomdbl2(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - pccustomdbl2 can't be empty" : GoTo selesai
            End If

            'pccustomdbl3(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - pccustomdbl3 can't be empty" : GoTo selesai
            End If

            'pccustomdate1(19) As Date
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - pccustomdate1 can't be empty" : GoTo selesai
            End If

            'pccustomdate2(20) As Date
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - pccustomdate2 can't be empty" : GoTo selesai
            End If

            'pccustomdate3(21) As Date
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - pccustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pckode~pcnama~pccatatan~pcaktif~pcinputuser~pcinputtgl~pcmodifikasiuser~pcmodifikasitgl~pccustomtext1~pccustomtext2~pccustomtext3~pccustomtext4~pccustomtext5~pccustomint1~pccustomint2~pccustomint3~pccustomdbl1~pccustomdbl2~pccustomdbl3~pccustomdate1~pccustomdate2~pccustomdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21)) = False Then
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
                    strValue2.Append("('" & FixQuotes(dr1("pckode")) & "', '" & FixQuotes(dr1("pcnama")) & "', '" & FixQuotes(dr1("pccatatan")) & "', " & dr1("pcaktif") & ", " & dr1("pcinputuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("pcinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("pcmodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("pcmodifikasitgl"), "yyyy-MM-dd H:mm:ss")) & "', '" & FixQuotes(dr1("pccustomtext1")) & "', '" & FixQuotes(dr1("pccustomtext2")) & "', '" & FixQuotes(dr1("pccustomtext3")) & "', '" & FixQuotes(dr1("pccustomtext4")) & "', '" & FixQuotes(dr1("pccustomtext5")) & "', " & dr1("pccustomint1") & ", " & dr1("pccustomint2") & ", " & dr1("pccustomint3") & ", '" & FixDouble(dr1("pccustomdbl1")) & "', '" & FixDouble(dr1("pccustomdbl2")) & "', '" & FixDouble(dr1("pccustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("pccustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("pccustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("pccustomdate3"))) & "')")
                Next

                If Len(strValue2.ToString) > 0 Then
                    'DELETE
                    sql = "DELETE FROM M1_Price_Category"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'INSERT
                    sql = "Insert into M1_Price_Category(pckode, pcnama, pccatatan, pcaktif, pcinputuser, pcinputtgl, pcmodifikasiuser, pcmodifikasitgl, pccustomtext1, pccustomtext2, pccustomtext3, pccustomtext4, pccustomtext5, pccustomint1, pccustomint2, pccustomint3, pccustomdbl1, pccustomdbl2, pccustomdbl3, pccustomdate1, pccustomdate2, pccustomdate3) values" & strValue2.ToString & ""
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
            Dim paramSearch As String = M1_Price_CategorySearch(PostWsSearch(paramSplit(0), "M1_Price_CategorySearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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

End Class
