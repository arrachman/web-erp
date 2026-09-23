Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_area
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_AreaSimpan(ByVal param As String) As String
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
        'akategori(0) As String, akode(1) As String, anama(2) As String, acatatan(3) As String, aaktif(4) As Integer, 
        'ainputuser(5) As Integer, ainputtgl(6) As DateTime, amodifikasiuser(7) As Integer, amodifikasitgl(8) As DateTime, acustomtext1(9) As String, 
        'acustomtext2(10) As String, acustomtext3(11) As String, acustomtext4(12) As String, acustomtext5(13) As String, acustomint1(14) As Integer, 
        'acustomint2(15) As Integer, acustomint3(16) As Integer, acustomdbl1(17) As Double, acustomdbl2(18) As Double, acustomdbl3(19) As Double, 
        'acustomdate1(20) As Date, acustomdate2(21) As Date, acustomdate3(22) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'akategori, akode, anama, acatatan, aaktif, ainputuser, ainputtgl, 
        'amodifikasiuser, amodifikasitgl, acustomtext1, acustomtext2, acustomtext3, acustomtext4, acustomtext5, 
        'acustomint1, acustomint2, acustomint3, acustomdbl1, acustomdbl2, acustomdbl3, acustomdate1, 
        'acustomdate2, acustomdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "akategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "akode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "anama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "aaktif", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "ainputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "ainputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "amodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "amodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "acustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "acustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "acustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acustomdate3", AsEnumTypeData.AsString)

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
            If (dataRowDetail.Length <> 23) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'aaktif(4) As Integer
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - aaktif required numeric." : GoTo selesai
            End If
            'ainputuser(5) As Integer
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - ainputuser required numeric." : GoTo selesai
            End If
            'ainputtgl(6) As DateTime
            If (IsDate(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - ainputtgl required date." : GoTo selesai
            End If
            'amodifikasiuser(7) As Integer
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - amodifikasiuser required numeric." : GoTo selesai
            End If
            'amodifikasitgl(8) As DateTime
            If (IsDate(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - amodifikasitgl required date." : GoTo selesai
            End If
            'acustomint1(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - acustomint1 required numeric." : GoTo selesai
            End If
            'acustomint2(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - acustomint2 required numeric." : GoTo selesai
            End If
            'acustomint3(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - acustomint3 required numeric." : GoTo selesai
            End If
            'acustomdbl1(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - acustomdbl1 required numeric." : GoTo selesai
            End If
            'acustomdbl2(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - acustomdbl2 required numeric." : GoTo selesai
            End If
            'acustomdbl3(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - acustomdbl3 required numeric." : GoTo selesai
            End If
            'acustomdate1(20) As Date
            If (IsDate(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - acustomdate1 required date." : GoTo selesai
            End If
            'acustomdate2(21) As Date
            If (IsDate(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - acustomdate2 required date." : GoTo selesai
            End If
            'acustomdate3(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - acustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'akategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - akategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - akategori should not be more than 25 character." : GoTo selesai
            End If

            'akode(1) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - akode can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - akode should not be more than 25 character." : GoTo selesai
            End If

            'anama(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - anama can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 100 Then
                result(2) = "Row : " & i & " - anama should not be more than 100 character." : GoTo selesai
            End If

            'ainputtgl(6) As DateTime
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - ainputtgl can't be empty" : GoTo selesai
            End If

            'amodifikasitgl(8) As DateTime
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - amodifikasitgl can't be empty" : GoTo selesai
            End If

            'acustomdbl1(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - acustomdbl1 can't be empty" : GoTo selesai
            End If

            'acustomdbl2(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - acustomdbl2 can't be empty" : GoTo selesai
            End If

            'acustomdbl3(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - acustomdbl3 can't be empty" : GoTo selesai
            End If

            'acustomdate1(20) As Date
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - acustomdate1 can't be empty" : GoTo selesai
            End If

            'acustomdate2(21) As Date
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - acustomdate2 can't be empty" : GoTo selesai
            End If

            'acustomdate3(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - acustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "akategori~akode~anama~acatatan~aaktif~ainputuser~ainputtgl~amodifikasiuser~amodifikasitgl~acustomtext1~acustomtext2~acustomtext3~acustomtext4~acustomtext5~acustomint1~acustomint2~acustomint3~acustomdbl1~acustomdbl2~acustomdbl3~acustomdate1~acustomdate2~acustomdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22)) = False Then
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
                        Dim SimpanHistory As New m12_area_history
                        Dim areaSimpanHistory As String = SimpanHistory.M12_Area_HistorySimpan("" & paramSplit(0) & "★M12_Area_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(dr1("akode")) & "")
                        Dim areaSplit() As String = areaSimpanHistory.Split(sptParam)
                        Dim areaSplitResult() As String = areaSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (areaSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & areaSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("akategori")) & "', '" & FixQuotes(dr1("akode")) & "', '" & FixQuotes(dr1("anama")) & "', '" & FixQuotes(dr1("acatatan")) & "', " & dr1("aaktif") & ", " & dr1("ainputuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("ainputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("amodifikasiuser") & ", NOW(), '" & FixQuotes(dr1("acustomtext1")) & "', '" & FixQuotes(dr1("acustomtext2")) & "', '" & FixQuotes(dr1("acustomtext3")) & "', '" & FixQuotes(dr1("acustomtext4")) & "', '" & FixQuotes(dr1("acustomtext5")) & "', " & dr1("acustomint1") & ", " & dr1("acustomint2") & ", " & dr1("acustomint3") & ", '" & FixDouble(dr1("acustomdbl1")) & "', '" & FixDouble(dr1("acustomdbl2")) & "', '" & FixDouble(dr1("acustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("acustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("acustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("acustomdate3"))) & "')")
                    Next

                Else
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("akategori")) & "', '" & FixQuotes(dr1("akode")) & "', '" & FixQuotes(dr1("anama")) & "', '" & FixQuotes(dr1("acatatan")) & "', " & dr1("aaktif") & ", " & dr1("ainputuser") & ", NOW(), " & dr1("amodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("amodifikasitgl"), "yyyy-MM-dd H:mm:ss")) & "', '" & FixQuotes(dr1("acustomtext1")) & "', '" & FixQuotes(dr1("acustomtext2")) & "', '" & FixQuotes(dr1("acustomtext3")) & "', '" & FixQuotes(dr1("acustomtext4")) & "', '" & FixQuotes(dr1("acustomtext5")) & "', " & dr1("acustomint1") & ", " & dr1("acustomint2") & ", " & dr1("acustomint3") & ", '" & FixDouble(dr1("acustomdbl1")) & "', '" & FixDouble(dr1("acustomdbl2")) & "', '" & FixDouble(dr1("acustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("acustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("acustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("acustomdate3"))) & "')")
                    Next
                End If

                If Len(strValue2.ToString) > 0 Then
                    sql = "Insert into M_12_Area(akategori, akode, anama, acatatan, aaktif, ainputuser, ainputtgl, amodifikasiuser, amodifikasitgl, acustomtext1, acustomtext2, acustomtext3, acustomtext4, acustomtext5, acustomint1, acustomint2, acustomint3, acustomdbl1, acustomdbl2, acustomdbl3, acustomdate1, acustomdate2, acustomdate3) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE akategori = VALUES(akategori), anama = VALUES(anama), acatatan = VALUES(acatatan), aaktif = VALUES(aaktif), amodifikasiuser = VALUES(amodifikasiuser), amodifikasitgl = NOW(), acustomtext1 = VALUES(acustomtext1), acustomtext2 = VALUES(acustomtext2), acustomtext3 = VALUES(acustomtext3), acustomtext4 = VALUES(acustomtext4), acustomtext5 = VALUES(acustomtext5), acustomint1 = VALUES(acustomint1), acustomint2 = VALUES(acustomint2), acustomint3 = VALUES(acustomint3), acustomdbl1 = VALUES(acustomdbl1), acustomdbl2 = VALUES(acustomdbl2), acustomdbl3 = VALUES(acustomdbl3), acustomdate1 = VALUES(acustomdate1), acustomdate2 = VALUES(acustomdate2), acustomdate3 = VALUES(acustomdate3)"
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
            Dim paramSearch As String = M12_AreaSearch(PostWsSearch(paramSplit(0), "M12_AreaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_AreaDelete(ByVal param As String) As String

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
            result(2) = "akode can't be empty." : GoTo selesai
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
            Dim paramTerkait As String = M12_AreaTerkait(PostWsTerkait(paramSplit(0), "M12_AreaTerkait", pagingSplit(0), pagingSplit(1), "", "", formatTgl, formatTglWaktu, idtransaksi))
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
            Dim SimpanHistory As New m12_area_history
            Dim areaSimpanHistory As String = SimpanHistory.M12_Area_HistorySimpan("" & paramSplit(0) & "★M12_Area_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(idtransaksi) & "")
            Dim areaSplit() As String = areaSimpanHistory.Split(sptParam)
            Dim areaSplitResult() As String = areaSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (areaSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & areaSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            'DELETE
            sql = "DELETE FROM M_12_Area WHERE akode = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_AreaSearch(PostWsSearch(paramSplit(0), "M12_AreaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_AreaSearch(ByVal param As String) As String
        'M12_AreaSearch --------------------------------------------------------
        'akategori, akode, anama, acatatan, aaktif, ainputuser, ainputtgl, 
        'amodifikasiuser, amodifikasitgl, acustomtext1, acustomtext2, acustomtext3, acustomtext4, acustomtext5, 
        'acustomint1, acustomint2, acustomint3, acustomdbl1, acustomdbl2, acustomdbl3, acustomdate1, 
        'acustomdate2, acustomdate3, acnama, ainputusernama, amodifikasiusernama

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
        sql = "select `a`.`akategori` AS `akategori`,`a`.`akode` AS `akode`,`a`.`anama` AS `anama`,`a`.`acatatan` AS `acatatan`,`a`.`aaktif` AS `aaktif`,`a`.`ainputuser` AS `ainputuser`,`a`.`ainputtgl` AS `ainputtgl`,`a`.`amodifikasiuser` AS `amodifikasiuser`,`a`.`amodifikasitgl` AS `amodifikasitgl`,`a`.`acustomtext1` AS `acustomtext1`,`a`.`acustomtext2` AS `acustomtext2`,`a`.`acustomtext3` AS `acustomtext3`,`a`.`acustomtext4` AS `acustomtext4`,`a`.`acustomtext5` AS `acustomtext5`,`a`.`acustomint1` AS `acustomint1`,`a`.`acustomint2` AS `acustomint2`,`a`.`acustomint3` AS `acustomint3`,`a`.`acustomdbl1` AS `acustomdbl1`,`a`.`acustomdbl2` AS `acustomdbl2`,`a`.`acustomdbl3` AS `acustomdbl3`,`a`.`acustomdate1` AS `acustomdate1`,`a`.`acustomdate2` AS `acustomdate2`,`a`.`acustomdate3` AS `acustomdate3`,`ac`.`acnama` AS `acnama`,`u1`.`unama` AS `ainputusernama`,`u2`.`unama` AS `amodifikasiusernama` from (((`m_12_area` `a` left join `m_12_area_category` `ac` on((`a`.`akategori` = `ac`.`ackode`))) left join `m0_user` `u1` on((`a`.`ainputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`a`.`amodifikasiuser` = `u2`.`userid`)))"

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Area", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("akategori"), ""), sptField,
                     FxDB(dr("akode"), ""), sptField,
                     FxDB(dr("anama"), ""), sptField,
                     FxDB(dr("acatatan"), ""), sptField,
                     FxDB(dr("aaktif"), 0), sptField,
                     FxDB(dr("ainputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ainputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("amodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("amodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("acustomtext1"), ""), sptField,
                     FxDB(dr("acustomtext2"), ""), sptField,
                     FxDB(dr("acustomtext3"), ""), sptField,
                     FxDB(dr("acustomtext4"), ""), sptField,
                     FxDB(dr("acustomtext5"), ""), sptField,
                     FxDB(dr("acustomint1"), 0), sptField,
                     FxDB(dr("acustomint2"), 0), sptField,
                     FxDB(dr("acustomint3"), 0), sptField,
                     FxDB(dr("acustomdbl1"), 0), sptField,
                     FxDB(dr("acustomdbl2"), 0), sptField,
                     FxDB(dr("acustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("acustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("acustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("acustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("acnama"), ""), sptField,
                     FxDB(dr("ainputusernama"), ""), sptField,
                     FxDB(dr("amodifikasiusernama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Area data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("akategori, akode, anama, acatatan, aaktif, ainputuser, ainputtgl, amodifikasiuser, amodifikasitgl, acustomtext1, acustomtext2, acustomtext3, acustomtext4, acustomtext5, acustomint1, acustomint2, acustomint3, acustomdbl1, acustomdbl2, acustomdbl3, acustomdate1, acustomdate2, acustomdate3, acnama, ainputusernama, amodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_AreaCekId(ByVal param As String) As String

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
            result(2) = "akode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'CEK DI DATABASE ================================================================
        Dim dt As DataTable
        Dim exist As Integer = 0
        dt = AsDataTableAmbilDariDB("SELECT COUNT(akode) FROM M_12_Area WHERE akode='" & idtransaksi & "'")
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
    Public Function M12_AreaTerkait(ByVal param As String) As String
        'M12_AreaTerkait --------------------------------------------------------
        'akode, anama, sumber, idterkait

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
            result(2) = "akode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        'sql = "SELECT ac.akode, ac.anama, 'Area' as sumber, a.anama as idterkait FROM m_12_area a JOIN M_12_Area ac ON a.akategori = ac.akode WHERE ac.akode = 'valkode' GROUP BY ac.akode, a.akode"
        sql = sql.Replace("valkode", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("akode"), ""), sptField,
                             FxDB(dr("anama"), ""), sptField,
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
            result(2) = "Related Area data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("akode, anama, sumber, idterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_AreaDownload(ByVal param As String) As String
        'M12_AreaDownload --------------------------------------------------------
        'akategori, akode, anama, acatatan, aaktif, ainputuser, ainputtgl, 
        'amodifikasiuser, amodifikasitgl, acustomtext1, acustomtext2, acustomtext3, acustomtext4, acustomtext5, 
        'acustomint1, acustomint2, acustomint3, acustomdbl1, acustomdbl2, acustomdbl3, acustomdate1, 
        'acustomdate2, acustomdate3

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
        sql = "select `a`.`akategori` AS `akategori`,`a`.`akode` AS `akode`,`a`.`anama` AS `anama`,`a`.`acatatan` AS `acatatan`,`a`.`aaktif` AS `aaktif`,`a`.`ainputuser` AS `ainputuser`,`a`.`ainputtgl` AS `ainputtgl`,`a`.`amodifikasiuser` AS `amodifikasiuser`,`a`.`amodifikasitgl` AS `amodifikasitgl`,`a`.`acustomtext1` AS `acustomtext1`,`a`.`acustomtext2` AS `acustomtext2`,`a`.`acustomtext3` AS `acustomtext3`,`a`.`acustomtext4` AS `acustomtext4`,`a`.`acustomtext5` AS `acustomtext5`,`a`.`acustomint1` AS `acustomint1`,`a`.`acustomint2` AS `acustomint2`,`a`.`acustomint3` AS `acustomint3`,`a`.`acustomdbl1` AS `acustomdbl1`,`a`.`acustomdbl2` AS `acustomdbl2`,`a`.`acustomdbl3` AS `acustomdbl3`,`a`.`acustomdate1` AS `acustomdate1`,`a`.`acustomdate2` AS `acustomdate2`,`a`.`acustomdate3` AS `acustomdate3`,`ac`.`acnama` AS `acnama`,`u1`.`unama` AS `ainputusernama`,`u2`.`unama` AS `amodifikasiusernama` from (((`m_12_area` `a` left join `m_12_area_category` `ac` on((`a`.`akategori` = `ac`.`ackode`))) left join `m0_user` `u1` on((`a`.`ainputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`a`.`amodifikasiuser` = `u2`.`userid`)))"

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Area", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("akategori"), ""), sptField,
                     FxDB(dr("akode"), ""), sptField,
                     FxDB(dr("anama"), ""), sptField,
                     FxDB(dr("acatatan"), ""), sptField,
                     FxDB(dr("aaktif"), 0), sptField,
                     FxDB(dr("ainputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ainputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("amodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("amodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("acustomtext1"), ""), sptField,
                     FxDB(dr("acustomtext2"), ""), sptField,
                     FxDB(dr("acustomtext3"), ""), sptField,
                     FxDB(dr("acustomtext4"), ""), sptField,
                     FxDB(dr("acustomtext5"), ""), sptField,
                     FxDB(dr("acustomint1"), 0), sptField,
                     FxDB(dr("acustomint2"), 0), sptField,
                     FxDB(dr("acustomint3"), 0), sptField,
                     FxDB(dr("acustomdbl1"), 0), sptField,
                     FxDB(dr("acustomdbl2"), 0), sptField,
                     FxDB(dr("acustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("acustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("acustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("acustomdate3"), ""), formatTgl), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Area data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("akategori, akode, anama, acatatan, aaktif, ainputuser, ainputtgl, amodifikasiuser, amodifikasitgl, acustomtext1, acustomtext2, acustomtext3, acustomtext4, acustomtext5, acustomint1, acustomint2, acustomint3, acustomdbl1, acustomdbl2, acustomdbl3, acustomdate1, acustomdate2, acustomdate3"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_AreaImport(ByVal param As String) As String
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
        'akategori(0) As String, akode(1) As String, anama(2) As String, acatatan(3) As String, aaktif(4) As Integer, 
        'ainputuser(5) As Integer, ainputtgl(6) As DateTime, amodifikasiuser(7) As Integer, amodifikasitgl(8) As DateTime, acustomtext1(9) As String, 
        'acustomtext2(10) As String, acustomtext3(11) As String, acustomtext4(12) As String, acustomtext5(13) As String, acustomint1(14) As Integer, 
        'acustomint2(15) As Integer, acustomint3(16) As Integer, acustomdbl1(17) As Double, acustomdbl2(18) As Double, acustomdbl3(19) As Double, 
        'acustomdate1(20) As Date, acustomdate2(21) As Date, acustomdate3(22) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'akategori, akode, anama, acatatan, aaktif, ainputuser, ainputtgl, 
        'amodifikasiuser, amodifikasitgl, acustomtext1, acustomtext2, acustomtext3, acustomtext4, acustomtext5, 
        'acustomint1, acustomint2, acustomint3, acustomdbl1, acustomdbl2, acustomdbl3, acustomdate1, 
        'acustomdate2, acustomdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "akategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "akode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "anama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "aaktif", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "ainputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "ainputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "amodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "amodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "acustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "acustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "acustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "acustomdate3", AsEnumTypeData.AsString)

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
            If (dataRowDetail.Length <> 23) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'aaktif(4) As Integer
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - aaktif required numeric." : GoTo selesai
            End If
            'ainputuser(5) As Integer
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - ainputuser required numeric." : GoTo selesai
            End If
            'ainputtgl(6) As DateTime
            If (IsDate(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - ainputtgl required date." : GoTo selesai
            End If
            'amodifikasiuser(7) As Integer
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - amodifikasiuser required numeric." : GoTo selesai
            End If
            'amodifikasitgl(8) As DateTime
            If (IsDate(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - amodifikasitgl required date." : GoTo selesai
            End If
            'acustomint1(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - acustomint1 required numeric." : GoTo selesai
            End If
            'acustomint2(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - acustomint2 required numeric." : GoTo selesai
            End If
            'acustomint3(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - acustomint3 required numeric." : GoTo selesai
            End If
            'acustomdbl1(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - acustomdbl1 required numeric." : GoTo selesai
            End If
            'acustomdbl2(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - acustomdbl2 required numeric." : GoTo selesai
            End If
            'acustomdbl3(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - acustomdbl3 required numeric." : GoTo selesai
            End If
            'acustomdate1(20) As Date
            If (IsDate(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - acustomdate1 required date." : GoTo selesai
            End If
            'acustomdate2(21) As Date
            If (IsDate(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - acustomdate2 required date." : GoTo selesai
            End If
            'acustomdate3(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - acustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'akategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - akategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - akategori should not be more than 25 character." : GoTo selesai
            End If

            'akode(1) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - akode can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - akode should not be more than 25 character." : GoTo selesai
            End If

            'anama(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - anama can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 100 Then
                result(2) = "Row : " & i & " - anama should not be more than 100 character." : GoTo selesai
            End If

            'ainputtgl(6) As DateTime
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - ainputtgl can't be empty" : GoTo selesai
            End If

            'amodifikasitgl(8) As DateTime
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - amodifikasitgl can't be empty" : GoTo selesai
            End If

            'acustomdbl1(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - acustomdbl1 can't be empty" : GoTo selesai
            End If

            'acustomdbl2(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - acustomdbl2 can't be empty" : GoTo selesai
            End If

            'acustomdbl3(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - acustomdbl3 can't be empty" : GoTo selesai
            End If

            'acustomdate1(20) As Date
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - acustomdate1 can't be empty" : GoTo selesai
            End If

            'acustomdate2(21) As Date
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - acustomdate2 can't be empty" : GoTo selesai
            End If

            'acustomdate3(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - acustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "akategori~akode~anama~acatatan~aaktif~ainputuser~ainputtgl~amodifikasiuser~amodifikasitgl~acustomtext1~acustomtext2~acustomtext3~acustomtext4~acustomtext5~acustomint1~acustomint2~acustomint3~acustomdbl1~acustomdbl2~acustomdbl3~acustomdate1~acustomdate2~acustomdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22)) = False Then
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
                    strValue2.Append("('" & FixQuotes(dr1("akategori")) & "', '" & FixQuotes(dr1("akode")) & "', '" & FixQuotes(dr1("anama")) & "', '" & FixQuotes(dr1("acatatan")) & "', " & dr1("aaktif") & ", " & dr1("ainputuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("ainputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("amodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("amodifikasitgl"), "yyyy-MM-dd H:mm:ss")) & "', '" & FixQuotes(dr1("acustomtext1")) & "', '" & FixQuotes(dr1("acustomtext2")) & "', '" & FixQuotes(dr1("acustomtext3")) & "', '" & FixQuotes(dr1("acustomtext4")) & "', '" & FixQuotes(dr1("acustomtext5")) & "', " & dr1("acustomint1") & ", " & dr1("acustomint2") & ", " & dr1("acustomint3") & ", '" & FixDouble(dr1("acustomdbl1")) & "', '" & FixDouble(dr1("acustomdbl2")) & "', '" & FixDouble(dr1("acustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("acustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("acustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("acustomdate3"))) & "')")
                Next

                If Len(strValue2.ToString) > 0 Then
                    'DELETE
                    sql = "DELETE FROM M_12_Area"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'INSERT
                    sql = "Insert into M_12_Area(akategori, akode, anama, acatatan, aaktif, ainputuser, ainputtgl, amodifikasiuser, amodifikasitgl, acustomtext1, acustomtext2, acustomtext3, acustomtext4, acustomtext5, acustomint1, acustomint2, acustomint3, acustomdbl1, acustomdbl2, acustomdbl3, acustomdate1, acustomdate2, acustomdate3) values" & strValue2.ToString & ""
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
            Dim paramSearch As String = M12_AreaSearch(PostWsSearch(paramSplit(0), "M12_AreaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
