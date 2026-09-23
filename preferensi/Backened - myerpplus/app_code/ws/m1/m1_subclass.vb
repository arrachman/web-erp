Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m1_subclass
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M1_SubclassSimpan(ByVal param As String) As String
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
        'sckode(0) As String, scnama(1) As String, sccatatan(2) As String, scaktif(3) As Integer, scinputuser(4) As Integer, 
        'scinputtgl(5) As DateTime, scmodifikasiuser(6) As Integer, scmodifikasitgl(7) As DateTime, sccustomtext1(8) As String, sccustomtext2(9) As String, 
        'sccustomtext3(10) As String, sccustomtext4(11) As String, sccustomtext5(12) As String, sccustomint1(13) As Integer, sccustomint2(14) As Integer, 
        'sccustomint3(15) As Integer, sccustomdbl1(16) As Double, sccustomdbl2(17) As Double, sccustomdbl3(18) As Double, sccustomdate1(19) As Date, 
        'sccustomdate2(20) As Date, sccustomdate3(21) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'sckode, scnama, sccatatan, scaktif, scinputuser, scinputtgl, scmodifikasiuser, 
        'scmodifikasitgl, sccustomtext1, sccustomtext2, sccustomtext3, sccustomtext4, sccustomtext5, sccustomint1, 
        'sccustomint2, sccustomint3, sccustomdbl1, sccustomdbl2, sccustomdbl3, sccustomdate1, sccustomdate2, 
        'sccustomdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "sckode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "scnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sccatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "scaktif", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "scinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "scinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "scmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "scmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sccustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sccustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sccustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sccustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sccustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sccustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sccustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sccustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sccustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sccustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sccustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sccustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sccustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sccustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "scindexbarcode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sckelas", AsEnumTypeData.AsString)

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
            If (dataRowDetail.Length <> 24) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'scaktif(3) As Integer
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - scaktif required numeric." : GoTo selesai
            End If
            'scinputuser(4) As Integer
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - scinputuser required numeric." : GoTo selesai
            End If
            'scinputtgl(5) As DateTime
            If (IsDate(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - scinputtgl required date." : GoTo selesai
            End If
            'scmodifikasiuser(6) As Integer
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - scmodifikasiuser required numeric." : GoTo selesai
            End If
            'scmodifikasitgl(7) As DateTime
            If (IsDate(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - scmodifikasitgl required date." : GoTo selesai
            End If
            'sccustomint1(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - sccustomint1 required numeric." : GoTo selesai
            End If
            'sccustomint2(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - sccustomint2 required numeric." : GoTo selesai
            End If
            'sccustomint3(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - sccustomint3 required numeric." : GoTo selesai
            End If
            'sccustomdbl1(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - sccustomdbl1 required numeric." : GoTo selesai
            End If
            'sccustomdbl2(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - sccustomdbl2 required numeric." : GoTo selesai
            End If
            'sccustomdbl3(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - sccustomdbl3 required numeric." : GoTo selesai
            End If
            'sccustomdate1(19) As Date
            If (IsDate(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - sccustomdate1 required date." : GoTo selesai
            End If
            'sccustomdate2(20) As Date
            If (IsDate(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - sccustomdate2 required date." : GoTo selesai
            End If
            'sccustomdate3(21) As Date
            If (IsDate(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - sccustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'sckode(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - sckode can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - sckode should not be more than 25 character." : GoTo selesai
            End If

            'scnama(1) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - scnama can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 100 Then
                result(2) = "Row : " & i & " - scnama should not be more than 100 character." : GoTo selesai
            End If

            'scinputtgl(5) As DateTime
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - scinputtgl can't be empty" : GoTo selesai
            End If

            'scmodifikasitgl(7) As DateTime
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - scmodifikasitgl can't be empty" : GoTo selesai
            End If

            'sccustomdbl1(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - sccustomdbl1 can't be empty" : GoTo selesai
            End If

            'sccustomdbl2(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - sccustomdbl2 can't be empty" : GoTo selesai
            End If

            'sccustomdbl3(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - sccustomdbl3 can't be empty" : GoTo selesai
            End If

            'sccustomdate1(19) As Date
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - sccustomdate1 can't be empty" : GoTo selesai
            End If

            'sccustomdate2(20) As Date
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - sccustomdate2 can't be empty" : GoTo selesai
            End If

            'sccustomdate3(21) As Date
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - sccustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "sckode~scnama~sccatatan~scaktif~scinputuser~scinputtgl~scmodifikasiuser~scmodifikasitgl~sccustomtext1~sccustomtext2~sccustomtext3~sccustomtext4~sccustomtext5~sccustomint1~sccustomint2~sccustomint3~sccustomdbl1~sccustomdbl2~sccustomdbl3~sccustomdate1~sccustomdate2~sccustomdate3~scindexbarcode~sckelas", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23)) = False Then
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
                        Dim SimpanHistory As New m1_subclass_history
                        Dim areaSimpanHistory As String = SimpanHistory.M1_SubclassHistorySimpan("" & paramSplit(0) & "★M1_SubclassHistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(dr1("sckode")) & "")
                        Dim areaSplit() As String = areaSimpanHistory.Split(sptParam)
                        Dim areaSplitResult() As String = areaSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (areaSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & areaSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("sckode")) & "', '" & FixQuotes(dr1("scnama")) & "', '" & FixQuotes(dr1("sccatatan")) & "', " & dr1("scaktif") & ", " & dr1("scinputuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("scinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("scmodifikasiuser") & ", NOW(), '" & FixQuotes(dr1("sccustomtext1")) & "', '" & FixQuotes(dr1("sccustomtext2")) & "', '" & FixQuotes(dr1("sccustomtext3")) & "', '" & FixQuotes(dr1("sccustomtext4")) & "', '" & FixQuotes(dr1("sccustomtext5")) & "', " & dr1("sccustomint1") & ", " & dr1("sccustomint2") & ", " & dr1("sccustomint3") & ", '" & FixDouble(dr1("sccustomdbl1")) & "', '" & FixDouble(dr1("sccustomdbl2")) & "', '" & FixDouble(dr1("sccustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("sccustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("sccustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("sccustomdate3"))) & "', '" & FixQuotes(dr1("scindexbarcode")) & "', '" & FixQuotes(dr1("sckelas")) & "')")
                    Next

                Else
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("sckode")) & "', '" & FixQuotes(dr1("scnama")) & "', '" & FixQuotes(dr1("sccatatan")) & "', " & dr1("scaktif") & ", " & dr1("scinputuser") & ", NOW(), " & dr1("scmodifikasiuser") & ", '1971-01-01 00:00:00', '" & FixQuotes(dr1("sccustomtext1")) & "', '" & FixQuotes(dr1("sccustomtext2")) & "', '" & FixQuotes(dr1("sccustomtext3")) & "', '" & FixQuotes(dr1("sccustomtext4")) & "', '" & FixQuotes(dr1("sccustomtext5")) & "', " & dr1("sccustomint1") & ", " & dr1("sccustomint2") & ", " & dr1("sccustomint3") & ", '" & FixDouble(dr1("sccustomdbl1")) & "', '" & FixDouble(dr1("sccustomdbl2")) & "', '" & FixDouble(dr1("sccustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("sccustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("sccustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("sccustomdate3"))) & "', '" & FixQuotes(dr1("scindexbarcode")) & "', '" & FixQuotes(dr1("sckelas")) & "')")
                    Next
                End If

                If Len(strValue2.ToString) > 0 Then
                    sql = "Insert into M1_Subclass(sckode, scnama, sccatatan, scaktif, scinputuser, scinputtgl, scmodifikasiuser, scmodifikasitgl, sccustomtext1, sccustomtext2, sccustomtext3, sccustomtext4, sccustomtext5, sccustomint1, sccustomint2, sccustomint3, sccustomdbl1, sccustomdbl2, sccustomdbl3, sccustomdate1, sccustomdate2, sccustomdate3, scindexbarcode, sckelas) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE scnama = VALUES(scnama), sccatatan = VALUES(sccatatan), scaktif = VALUES(scaktif), scmodifikasiuser = VALUES(scmodifikasiuser), scmodifikasitgl = NOW(), sccustomtext1 = VALUES(sccustomtext1), sccustomtext2 = VALUES(sccustomtext2), sccustomtext3 = VALUES(sccustomtext3), sccustomtext4 = VALUES(sccustomtext4), sccustomtext5 = VALUES(sccustomtext5), sccustomint1 = VALUES(sccustomint1), sccustomint2 = VALUES(sccustomint2), sccustomint3 = VALUES(sccustomint3), sccustomdbl1 = VALUES(sccustomdbl1), sccustomdbl2 = VALUES(sccustomdbl2), sccustomdbl3 = VALUES(sccustomdbl3), sccustomdate1 = VALUES(sccustomdate1), sccustomdate2 = VALUES(sccustomdate2), sccustomdate3 = VALUES(sccustomdate3), scindexbarcode = VALUES(scindexbarcode), sckelas = VALUES(sckelas)"
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
            Dim paramSearch As String = M1_SubclassSearch(PostWsSearch(paramSplit(0), "M1_SubclassSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_SubclassDelete(ByVal param As String) As String

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
            result(2) = "sckode can't be empty." : GoTo selesai
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
            Dim paramTerkait As String = M1_SubclassTerkait(PostWsTerkait(paramSplit(0), "M1_SubclassTerkait", pagingSplit(0), pagingSplit(1), "", "", formatTgl, formatTglWaktu, idtransaksi))
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
            Dim SimpanHistory As New m1_subclass_history
            Dim areaSimpanHistory As String = SimpanHistory.M1_SubclassHistorySimpan("" & paramSplit(0) & "★M1_SubclassHistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(idtransaksi) & "")
            Dim areaSplit() As String = areaSimpanHistory.Split(sptParam)
            Dim areaSplitResult() As String = areaSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (areaSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & areaSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            'DELETE
            sql = "DELETE FROM M1_Subclass WHERE sckode = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M1_SubclassSearch(PostWsSearch(paramSplit(0), "M1_SubclassSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_SubclassSearch(ByVal param As String) As String
        'M1_SubclassSearch --------------------------------------------------------
        'sckode, scnama, sccatatan, scaktif, scinputuser, scinputtgl, scmodifikasiuser, 
        'scmodifikasitgl, sccustomtext1, sccustomtext2, sccustomtext3, sccustomtext4, sccustomtext5, sccustomint1, 
        'sccustomint2, sccustomint3, sccustomdbl1, sccustomdbl2, sccustomdbl3, sccustomdate1, sccustomdate2, 
        'sccustomdate3, scinputusernama, scmodifikasiusernama

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
        sql = "select `sc`.`sckode` AS `sckode`,`sc`.`scnama` AS `scnama`,`sc`.`sccatatan` AS `sccatatan`,`sc`.`scaktif` AS `scaktif`,`sc`.`scinputuser` AS `scinputuser`,`sc`.`scinputtgl` AS `scinputtgl`,`sc`.`scmodifikasiuser` AS `scmodifikasiuser`,`sc`.`scmodifikasitgl` AS `scmodifikasitgl`,`sc`.`sccustomtext1` AS `sccustomtext1`,`sc`.`sccustomtext2` AS `sccustomtext2`,`sc`.`sccustomtext3` AS `sccustomtext3`,`sc`.`sccustomtext4` AS `sccustomtext4`,`sc`.`sccustomtext5` AS `sccustomtext5`,`sc`.`sccustomint1` AS `sccustomint1`,`sc`.`sccustomint2` AS `sccustomint2`,`sc`.`sccustomint3` AS `sccustomint3`,`sc`.`sccustomdbl1` AS `sccustomdbl1`,`sc`.`sccustomdbl2` AS `sccustomdbl2`,`sc`.`sccustomdbl3` AS `sccustomdbl3`,`sc`.`sccustomdate1` AS `sccustomdate1`,`sc`.`sccustomdate2` AS `sccustomdate2`,`sc`.`sccustomdate3` AS `sccustomdate3`,`sc`.`scindexbarcode` AS `scindexbarcode`,`sc`.`sckelas` AS `sckelas`,`u1`.`unama` AS `scinputusernama`,`u2`.`unama` AS `scmodifikasiusernama` from ((`M1_Subclass` `sc` left join `m0_user` `u1` on((`sc`.`scinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`sc`.`scmodifikasiuser` = `u2`.`userid`)))"

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Subclass", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("sckode"), ""), sptField,
                     FxDB(dr("scnama"), ""), sptField,
                     FxDB(dr("sccatatan"), ""), sptField,
                     FxDB(dr("scaktif"), 0), sptField,
                     FxDB(dr("scinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("scinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("scmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("scmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sccustomtext1"), ""), sptField,
                     FxDB(dr("sccustomtext2"), ""), sptField,
                     FxDB(dr("sccustomtext3"), ""), sptField,
                     FxDB(dr("sccustomtext4"), ""), sptField,
                     FxDB(dr("sccustomtext5"), ""), sptField,
                     FxDB(dr("sccustomint1"), 0), sptField,
                     FxDB(dr("sccustomint2"), 0), sptField,
                     FxDB(dr("sccustomint3"), 0), sptField,
                     FxDB(dr("sccustomdbl1"), 0), sptField,
                     FxDB(dr("sccustomdbl2"), 0), sptField,
                     FxDB(dr("sccustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sccustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sccustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sccustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("scindexbarcode"), ""), sptField,
                     FxDB(dr("sckelas"), ""), sptField,
                     FxDB(dr("scinputusernama"), ""), sptField,
                     FxDB(dr("scmodifikasiusernama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Sub Class data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sckode, scnama, sccatatan, scaktif, scinputuser, scinputtgl, scmodifikasiuser, scmodifikasitgl, sccustomtext1, sccustomtext2, sccustomtext3, sccustomtext4, sccustomtext5, sccustomint1, sccustomint2, sccustomint3, sccustomdbl1, sccustomdbl2, sccustomdbl3, sccustomdate1, sccustomdate2, sccustomdate3, scindexbarcode, sckelas, scinputusernama, scmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_SubclassCekId(ByVal param As String) As String

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
            result(2) = "sckode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'CEK DI DATABASE ================================================================
        Dim dt As DataTable
        Dim exist As Integer = 0
        dt = AsDataTableAmbilDariDB("SELECT COUNT(sckode) FROM M1_Subclass WHERE sckode='" & idtransaksi & "'")
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
    Public Function M1_SubclassTerkait(ByVal param As String) As String
        'M1_SubclassTerkait --------------------------------------------------------
        'sckode, scnama, sumber, idterkait

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
            result(2) = "sckode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        sql = "select sc.sckode AS sckode, sc.scnama AS scnama, s.skode AS sumber, CONCAT('Setting : ', s.smodule, ' - ', s.sgrup, ' - ', s.skode) AS idterkait from m0_setting s join M1_Subclass sc on (s.smodule = 0 AND s.sgrup = 'options' AND s.skode = 'KelasProduk' AND s.snilai = sc.sckode) WHERE sc.sckode = 'valkode' union all SELECT sc.sckode as sckode, sc.scnama as scnama, 'Item' as sumber, i.bkode as idterkait FROM m1_item i JOIN M1_Subclass sc ON i.bkelasproduk = sc.sckode AND sc.sckode = 'valkode' GROUP BY sc.sckode, i.bid UNION ALL SELECT sc.sckode as sckode, sc.scnama as scnama, 'POS Type' as sumber, ptsc.tipepos as idterkait FROM m_12_pos_type_class_product ptsc JOIN M1_Subclass sc ON ptsc.kelasproduk = sc.sckode AND sc.sckode = 'valkode' GROUP BY sc.sckode, ptsc.tipepos"
        sql = sql.Replace("valkode", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Subclass", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("sckode"), ""), sptField,
                             FxDB(dr("scnama"), ""), sptField,
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
            result(2) = "Related Sub Class data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sckode, scnama, sumber, idterkait"))

        Return wsResult
    End Function

End Class
