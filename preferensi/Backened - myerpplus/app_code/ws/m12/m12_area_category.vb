Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_area_category
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_Area_CategorySimpan(ByVal param As String) As String
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
        'ackode(0) As String, acnama(1) As String, accatatan(2) As String, acaktif(3) As Integer, acinputuser(4) As Integer, 
        'acinputtgl(5) As DateTime, acmodifikasiuser(6) As Integer, acmodifikasitgl(7) As DateTime, accustomtext1(8) As String, accustomtext2(9) As String, 
        'accustomtext3(10) As String, accustomtext4(11) As String, accustomtext5(12) As String, accustomint1(13) As Integer, accustomint2(14) As Integer, 
        'accustomint3(15) As Integer, accustomdbl1(16) As Double, accustomdbl2(17) As Double, accustomdbl3(18) As Double, accustomdate1(19) As Date, 
        'accustomdate2(20) As Date, accustomdate3(21) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'ackode, acnama, accatatan, acaktif, acinputuser, acinputtgl, acmodifikasiuser, 
        'acmodifikasitgl, accustomtext1, accustomtext2, accustomtext3, accustomtext4, accustomtext5, accustomint1, 
        'accustomint2, accustomint3, accustomdbl1, accustomdbl2, accustomdbl3, accustomdate1, accustomdate2, 
        'accustomdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "ackode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "accatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acaktif", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "acinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "acinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "acmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "accustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "accustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "accustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "accustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "accustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "accustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "accustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "accustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "accustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "accustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "accustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "accustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "accustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "accustomdate3", AsEnumTypeData.AsString)

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
            'acaktif(3) As Integer
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - acaktif required numeric." : GoTo selesai
            End If
            'acinputuser(4) As Integer
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - acinputuser required numeric." : GoTo selesai
            End If
            'acinputtgl(5) As DateTime
            If (IsDate(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - acinputtgl required date." : GoTo selesai
            End If
            'acmodifikasiuser(6) As Integer
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - acmodifikasiuser required numeric." : GoTo selesai
            End If
            'acmodifikasitgl(7) As DateTime
            If (IsDate(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - acmodifikasitgl required date." : GoTo selesai
            End If
            'accustomint1(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - accustomint1 required numeric." : GoTo selesai
            End If
            'accustomint2(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - accustomint2 required numeric." : GoTo selesai
            End If
            'accustomint3(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - accustomint3 required numeric." : GoTo selesai
            End If
            'accustomdbl1(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - accustomdbl1 required numeric." : GoTo selesai
            End If
            'accustomdbl2(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - accustomdbl2 required numeric." : GoTo selesai
            End If
            'accustomdbl3(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - accustomdbl3 required numeric." : GoTo selesai
            End If
            'accustomdate1(19) As Date
            If (IsDate(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - accustomdate1 required date." : GoTo selesai
            End If
            'accustomdate2(20) As Date
            If (IsDate(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - accustomdate2 required date." : GoTo selesai
            End If
            'accustomdate3(21) As Date
            If (IsDate(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - accustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'ackode(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - ackode can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - ackode should not be more than 25 character." : GoTo selesai
            End If

            'acnama(1) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - acnama can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 100 Then
                result(2) = "Row : " & i & " - acnama should not be more than 100 character." : GoTo selesai
            End If

            'acinputtgl(5) As DateTime
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - acinputtgl can't be empty" : GoTo selesai
            End If

            'acmodifikasitgl(7) As DateTime
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - acmodifikasitgl can't be empty" : GoTo selesai
            End If

            'accustomdbl1(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - accustomdbl1 can't be empty" : GoTo selesai
            End If

            'accustomdbl2(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - accustomdbl2 can't be empty" : GoTo selesai
            End If

            'accustomdbl3(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - accustomdbl3 can't be empty" : GoTo selesai
            End If

            'accustomdate1(19) As Date
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - accustomdate1 can't be empty" : GoTo selesai
            End If

            'accustomdate2(20) As Date
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - accustomdate2 can't be empty" : GoTo selesai
            End If

            'accustomdate3(21) As Date
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - accustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "ackode~acnama~accatatan~acaktif~acinputuser~acinputtgl~acmodifikasiuser~acmodifikasitgl~accustomtext1~accustomtext2~accustomtext3~accustomtext4~accustomtext5~accustomint1~accustomint2~accustomint3~accustomdbl1~accustomdbl2~accustomdbl3~accustomdate1~accustomdate2~accustomdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21)) = False Then
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
                        Dim SimpanHistory As New m12_area_category_history
                        Dim areaSimpanHistory As String = SimpanHistory.M12_Area_CategoryHistorySimpan("" & paramSplit(0) & "★M12_Area_CategoryHistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(dr1("ackode")) & "")
                        Dim areaSplit() As String = areaSimpanHistory.Split(sptParam)
                        Dim areaSplitResult() As String = areaSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (areaSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & areaSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("ackode")) & "', '" & FixQuotes(dr1("acnama")) & "', '" & FixQuotes(dr1("accatatan")) & "', " & dr1("acaktif") & ", " & dr1("acinputuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("acinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("acmodifikasiuser") & ", NOW(), '" & FixQuotes(dr1("accustomtext1")) & "', '" & FixQuotes(dr1("accustomtext2")) & "', '" & FixQuotes(dr1("accustomtext3")) & "', '" & FixQuotes(dr1("accustomtext4")) & "', '" & FixQuotes(dr1("accustomtext5")) & "', " & dr1("accustomint1") & ", " & dr1("accustomint2") & ", " & dr1("accustomint3") & ", '" & FixDouble(dr1("accustomdbl1")) & "', '" & FixDouble(dr1("accustomdbl2")) & "', '" & FixDouble(dr1("accustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("accustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("accustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("accustomdate3"))) & "')")
                    Next

                Else
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("ackode")) & "', '" & FixQuotes(dr1("acnama")) & "', '" & FixQuotes(dr1("accatatan")) & "', " & dr1("acaktif") & ", " & dr1("acinputuser") & ", NOW(), " & dr1("acmodifikasiuser") & ", '1971-01-01 00:00:00', '" & FixQuotes(dr1("accustomtext1")) & "', '" & FixQuotes(dr1("accustomtext2")) & "', '" & FixQuotes(dr1("accustomtext3")) & "', '" & FixQuotes(dr1("accustomtext4")) & "', '" & FixQuotes(dr1("accustomtext5")) & "', " & dr1("accustomint1") & ", " & dr1("accustomint2") & ", " & dr1("accustomint3") & ", '" & FixDouble(dr1("accustomdbl1")) & "', '" & FixDouble(dr1("accustomdbl2")) & "', '" & FixDouble(dr1("accustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("accustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("accustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("accustomdate3"))) & "')")
                    Next
                End If

                If Len(strValue2.ToString) > 0 Then
                    sql = "Insert into M_12_Area_Category(ackode, acnama, accatatan, acaktif, acinputuser, acinputtgl, acmodifikasiuser, acmodifikasitgl, accustomtext1, accustomtext2, accustomtext3, accustomtext4, accustomtext5, accustomint1, accustomint2, accustomint3, accustomdbl1, accustomdbl2, accustomdbl3, accustomdate1, accustomdate2, accustomdate3) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE acnama = VALUES(acnama), accatatan = VALUES(accatatan), acaktif = VALUES(acaktif), acmodifikasiuser = VALUES(acmodifikasiuser), acmodifikasitgl = NOW(), accustomtext1 = VALUES(accustomtext1), accustomtext2 = VALUES(accustomtext2), accustomtext3 = VALUES(accustomtext3), accustomtext4 = VALUES(accustomtext4), accustomtext5 = VALUES(accustomtext5), accustomint1 = VALUES(accustomint1), accustomint2 = VALUES(accustomint2), accustomint3 = VALUES(accustomint3), accustomdbl1 = VALUES(accustomdbl1), accustomdbl2 = VALUES(accustomdbl2), accustomdbl3 = VALUES(accustomdbl3), accustomdate1 = VALUES(accustomdate1), accustomdate2 = VALUES(accustomdate2), accustomdate3 = VALUES(accustomdate3)"
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
            Dim paramSearch As String = M12_Area_CategorySearch(PostWsSearch(paramSplit(0), "M12_Area_CategorySearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Area_CategoryDelete(ByVal param As String) As String

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
            result(2) = "ackode can't be empty." : GoTo selesai
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
            Dim paramTerkait As String = M12_Area_CategoryTerkait(PostWsTerkait(paramSplit(0), "M12_Area_CategoryTerkait", pagingSplit(0), pagingSplit(1), "", "", formatTgl, formatTglWaktu, idtransaksi))
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
            Dim SimpanHistory As New m12_area_category_history
            Dim areaSimpanHistory As String = SimpanHistory.M12_Area_CategoryHistorySimpan("" & paramSplit(0) & "★M12_Area_CategoryHistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(idtransaksi) & "")
            Dim areaSplit() As String = areaSimpanHistory.Split(sptParam)
            Dim areaSplitResult() As String = areaSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (areaSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & areaSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            'DELETE
            sql = "DELETE FROM M_12_Area_Category WHERE ackode = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_Area_CategorySearch(PostWsSearch(paramSplit(0), "M12_Area_CategorySearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Area_CategorySearch(ByVal param As String) As String
        'M12_Area_CategorySearch --------------------------------------------------------
        'ackode, acnama, accatatan, acaktif, acinputuser, acinputtgl, acmodifikasiuser, 
        'acmodifikasitgl, accustomtext1, accustomtext2, accustomtext3, accustomtext4, accustomtext5, accustomint1, 
        'accustomint2, accustomint3, accustomdbl1, accustomdbl2, accustomdbl3, accustomdate1, accustomdate2, 
        'accustomdate3, acinputusernama, acmodifikasiusernama

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
        sql = "select `ac`.`ackode` AS `ackode`,`ac`.`acnama` AS `acnama`,`ac`.`accatatan` AS `accatatan`,`ac`.`acaktif` AS `acaktif`,`ac`.`acinputuser` AS `acinputuser`,`ac`.`acinputtgl` AS `acinputtgl`,`ac`.`acmodifikasiuser` AS `acmodifikasiuser`,`ac`.`acmodifikasitgl` AS `acmodifikasitgl`,`ac`.`accustomtext1` AS `accustomtext1`,`ac`.`accustomtext2` AS `accustomtext2`,`ac`.`accustomtext3` AS `accustomtext3`,`ac`.`accustomtext4` AS `accustomtext4`,`ac`.`accustomtext5` AS `accustomtext5`,`ac`.`accustomint1` AS `accustomint1`,`ac`.`accustomint2` AS `accustomint2`,`ac`.`accustomint3` AS `accustomint3`,`ac`.`accustomdbl1` AS `accustomdbl1`,`ac`.`accustomdbl2` AS `accustomdbl2`,`ac`.`accustomdbl3` AS `accustomdbl3`,`ac`.`accustomdate1` AS `accustomdate1`,`ac`.`accustomdate2` AS `accustomdate2`,`ac`.`accustomdate3` AS `accustomdate3`,`u1`.`unama` AS `acinputusernama`,`u2`.`unama` AS `acmodifikasiusernama` from ((`m_12_area_category` `ac` left join `m0_user` `u1` on((`ac`.`acinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`ac`.`acmodifikasiuser` = `u2`.`userid`)))"

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Area", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("ackode"), ""), sptField,
                     FxDB(dr("acnama"), ""), sptField,
                     FxDB(dr("accatatan"), ""), sptField,
                     FxDB(dr("acaktif"), 0), sptField,
                     FxDB(dr("acinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("acinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("acmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("acmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("accustomtext1"), ""), sptField,
                     FxDB(dr("accustomtext2"), ""), sptField,
                     FxDB(dr("accustomtext3"), ""), sptField,
                     FxDB(dr("accustomtext4"), ""), sptField,
                     FxDB(dr("accustomtext5"), ""), sptField,
                     FxDB(dr("accustomint1"), 0), sptField,
                     FxDB(dr("accustomint2"), 0), sptField,
                     FxDB(dr("accustomint3"), 0), sptField,
                     FxDB(dr("accustomdbl1"), 0), sptField,
                     FxDB(dr("accustomdbl2"), 0), sptField,
                     FxDB(dr("accustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("accustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("accustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("accustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("acinputusernama"), ""), sptField,
                     FxDB(dr("acmodifikasiusernama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Area Category data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ackode, acnama, accatatan, acaktif, acinputuser, acinputtgl, acmodifikasiuser, acmodifikasitgl, accustomtext1, accustomtext2, accustomtext3, accustomtext4, accustomtext5, accustomint1, accustomint2, accustomint3, accustomdbl1, accustomdbl2, accustomdbl3, accustomdate1, accustomdate2, accustomdate3, acinputusernama, acmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Area_CategoryCekId(ByVal param As String) As String

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
            result(2) = "ackode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'CEK DI DATABASE ================================================================
        Dim dt As DataTable
        Dim exist As Integer = 0
        dt = AsDataTableAmbilDariDB("SELECT COUNT(ackode) FROM m_12_area_category WHERE ackode='" & idtransaksi & "'")
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
    Public Function M12_Area_CategoryTerkait(ByVal param As String) As String
        'M12_Area_CategoryTerkait --------------------------------------------------------
        'ackode, acnama, sumber, idterkait

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
        sql = "SELECT ac.ackode, ac.acnama, 'Area' as sumber, a.anama as idterkait FROM m_12_area a JOIN m_12_area_category ac ON a.akategori = ac.ackode WHERE ac.ackode = 'valkode' GROUP BY ac.ackode, a.akode"
        sql = sql.Replace("valkode", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("ackode"), ""), sptField,
                             FxDB(dr("acnama"), ""), sptField,
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
            result(2) = "Related Area Category data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ackode, acnama, sumber, idterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Area_CategoryDownload(ByVal param As String) As String
        'M12_Area_CategoryDownload --------------------------------------------------------
        'ackode, acnama, accatatan, acaktif, acinputuser, acinputtgl, acmodifikasiuser, 
        'acmodifikasitgl, accustomtext1, accustomtext2, accustomtext3, accustomtext4, accustomtext5, accustomint1, 
        'accustomint2, accustomint3, accustomdbl1, accustomdbl2, accustomdbl3, accustomdate1, accustomdate2, 
        'accustomdate3

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
        sql = "select `ac`.`ackode` AS `ackode`,`ac`.`acnama` AS `acnama`,`ac`.`accatatan` AS `accatatan`,`ac`.`acaktif` AS `acaktif`,`ac`.`acinputuser` AS `acinputuser`,`ac`.`acinputtgl` AS `acinputtgl`,`ac`.`acmodifikasiuser` AS `acmodifikasiuser`,`ac`.`acmodifikasitgl` AS `acmodifikasitgl`,`ac`.`accustomtext1` AS `accustomtext1`,`ac`.`accustomtext2` AS `accustomtext2`,`ac`.`accustomtext3` AS `accustomtext3`,`ac`.`accustomtext4` AS `accustomtext4`,`ac`.`accustomtext5` AS `accustomtext5`,`ac`.`accustomint1` AS `accustomint1`,`ac`.`accustomint2` AS `accustomint2`,`ac`.`accustomint3` AS `accustomint3`,`ac`.`accustomdbl1` AS `accustomdbl1`,`ac`.`accustomdbl2` AS `accustomdbl2`,`ac`.`accustomdbl3` AS `accustomdbl3`,`ac`.`accustomdate1` AS `accustomdate1`,`ac`.`accustomdate2` AS `accustomdate2`,`ac`.`accustomdate3` AS `accustomdate3`,`u1`.`unama` AS `acinputusernama`,`u2`.`unama` AS `acmodifikasiusernama` from ((`m_12_area_category` `ac` left join `m0_user` `u1` on((`ac`.`acinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`ac`.`acmodifikasiuser` = `u2`.`userid`)))"

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Area", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("ackode"), ""), sptField,
                     FxDB(dr("acnama"), ""), sptField,
                     FxDB(dr("accatatan"), ""), sptField,
                     FxDB(dr("acaktif"), 0), sptField,
                     FxDB(dr("acinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("acinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("acmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("acmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("accustomtext1"), ""), sptField,
                     FxDB(dr("accustomtext2"), ""), sptField,
                     FxDB(dr("accustomtext3"), ""), sptField,
                     FxDB(dr("accustomtext4"), ""), sptField,
                     FxDB(dr("accustomtext5"), ""), sptField,
                     FxDB(dr("accustomint1"), 0), sptField,
                     FxDB(dr("accustomint2"), 0), sptField,
                     FxDB(dr("accustomint3"), 0), sptField,
                     FxDB(dr("accustomdbl1"), 0), sptField,
                     FxDB(dr("accustomdbl2"), 0), sptField,
                     FxDB(dr("accustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("accustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("accustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("accustomdate3"), ""), formatTgl), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Area Category data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ackode, acnama, accatatan, acaktif, acinputuser, acinputtgl, acmodifikasiuser, acmodifikasitgl, accustomtext1, accustomtext2, accustomtext3, accustomtext4, accustomtext5, accustomint1, accustomint2, accustomint3, accustomdbl1, accustomdbl2, accustomdbl3, accustomdate1, accustomdate2, accustomdate3"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Area_CategoryImport(ByVal param As String) As String
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
        'ackode(0) As String, acnama(1) As String, accatatan(2) As String, acaktif(3) As Integer, acinputuser(4) As Integer, 
        'acinputtgl(5) As DateTime, acmodifikasiuser(6) As Integer, acmodifikasitgl(7) As DateTime, accustomtext1(8) As String, accustomtext2(9) As String, 
        'accustomtext3(10) As String, accustomtext4(11) As String, accustomtext5(12) As String, accustomint1(13) As Integer, accustomint2(14) As Integer, 
        'accustomint3(15) As Integer, accustomdbl1(16) As Double, accustomdbl2(17) As Double, accustomdbl3(18) As Double, accustomdate1(19) As Date, 
        'accustomdate2(20) As Date, accustomdate3(21) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'ackode, acnama, accatatan, acaktif, acinputuser, acinputtgl, acmodifikasiuser, 
        'acmodifikasitgl, accustomtext1, accustomtext2, accustomtext3, accustomtext4, accustomtext5, accustomint1, 
        'accustomint2, accustomint3, accustomdbl1, accustomdbl2, accustomdbl3, accustomdate1, accustomdate2, 
        'accustomdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "ackode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "accatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acaktif", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "acinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "acinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "acmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "accustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "accustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "accustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "accustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "accustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "accustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "accustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "accustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "accustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "accustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "accustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "accustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "accustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "accustomdate3", AsEnumTypeData.AsString)

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
            'acaktif(3) As Integer
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - acaktif required numeric." : GoTo selesai
            End If
            'acinputuser(4) As Integer
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - acinputuser required numeric." : GoTo selesai
            End If
            'acinputtgl(5) As DateTime
            If (IsDate(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - acinputtgl required date." : GoTo selesai
            End If
            'acmodifikasiuser(6) As Integer
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - acmodifikasiuser required numeric." : GoTo selesai
            End If
            'acmodifikasitgl(7) As DateTime
            If (IsDate(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - acmodifikasitgl required date." : GoTo selesai
            End If
            'accustomint1(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - accustomint1 required numeric." : GoTo selesai
            End If
            'accustomint2(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - accustomint2 required numeric." : GoTo selesai
            End If
            'accustomint3(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - accustomint3 required numeric." : GoTo selesai
            End If
            'accustomdbl1(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - accustomdbl1 required numeric." : GoTo selesai
            End If
            'accustomdbl2(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - accustomdbl2 required numeric." : GoTo selesai
            End If
            'accustomdbl3(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - accustomdbl3 required numeric." : GoTo selesai
            End If
            'accustomdate1(19) As Date
            If (IsDate(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - accustomdate1 required date." : GoTo selesai
            End If
            'accustomdate2(20) As Date
            If (IsDate(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - accustomdate2 required date." : GoTo selesai
            End If
            'accustomdate3(21) As Date
            If (IsDate(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - accustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'ackode(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - ackode can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - ackode should not be more than 25 character." : GoTo selesai
            End If

            'acnama(1) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - acnama can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 100 Then
                result(2) = "Row : " & i & " - acnama should not be more than 100 character." : GoTo selesai
            End If

            'acinputtgl(5) As DateTime
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - acinputtgl can't be empty" : GoTo selesai
            End If

            'acmodifikasitgl(7) As DateTime
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - acmodifikasitgl can't be empty" : GoTo selesai
            End If

            'accustomdbl1(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - accustomdbl1 can't be empty" : GoTo selesai
            End If

            'accustomdbl2(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - accustomdbl2 can't be empty" : GoTo selesai
            End If

            'accustomdbl3(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - accustomdbl3 can't be empty" : GoTo selesai
            End If

            'accustomdate1(19) As Date
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - accustomdate1 can't be empty" : GoTo selesai
            End If

            'accustomdate2(20) As Date
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - accustomdate2 can't be empty" : GoTo selesai
            End If

            'accustomdate3(21) As Date
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - accustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "ackode~acnama~accatatan~acaktif~acinputuser~acinputtgl~acmodifikasiuser~acmodifikasitgl~accustomtext1~accustomtext2~accustomtext3~accustomtext4~accustomtext5~accustomint1~accustomint2~accustomint3~accustomdbl1~accustomdbl2~accustomdbl3~accustomdate1~accustomdate2~accustomdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21)) = False Then
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
                    strValue2.Append("('" & FixQuotes(dr1("ackode")) & "', '" & FixQuotes(dr1("acnama")) & "', '" & FixQuotes(dr1("accatatan")) & "', " & dr1("acaktif") & ", " & dr1("acinputuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("acinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("acmodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("acmodifikasitgl"), "yyyy-MM-dd H:mm:ss")) & "', '" & FixQuotes(dr1("accustomtext1")) & "', '" & FixQuotes(dr1("accustomtext2")) & "', '" & FixQuotes(dr1("accustomtext3")) & "', '" & FixQuotes(dr1("accustomtext4")) & "', '" & FixQuotes(dr1("accustomtext5")) & "', " & dr1("accustomint1") & ", " & dr1("accustomint2") & ", " & dr1("accustomint3") & ", '" & FixDouble(dr1("accustomdbl1")) & "', '" & FixDouble(dr1("accustomdbl2")) & "', '" & FixDouble(dr1("accustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("accustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("accustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("accustomdate3"))) & "')")
                Next

                If Len(strValue2.ToString) > 0 Then
                    'DELETE
                    sql = "DELETE FROM M_12_Area_Category"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'INSERT
                    sql = "Insert into M_12_Area_Category(ackode, acnama, accatatan, acaktif, acinputuser, acinputtgl, acmodifikasiuser, acmodifikasitgl, accustomtext1, accustomtext2, accustomtext3, accustomtext4, accustomtext5, accustomint1, accustomint2, accustomint3, accustomdbl1, accustomdbl2, accustomdbl3, accustomdate1, accustomdate2, accustomdate3) values" & strValue2.ToString & ""
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
            Dim paramSearch As String = M12_Area_CategorySearch(PostWsSearch(paramSplit(0), "M12_Area_CategorySearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
