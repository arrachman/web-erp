Imports System.Web
Imports System.Web.Services
'Imports System.Web.Services.Protocols
'Imports System.Web.Script.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization

'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m1_other_cost
    Inherits System.Web.Services.WebService
    Public ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi

    <WebMethod()>
    Public Function M1_Other_CostSimpanOld(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean
        Dim search As String = ""
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

        'MAPPING BUAT WS ----------------------------------------------------------
        'ockode(0) As String, ocnama(1) As String, ocrekdebit(2) As String, ocrekkredit(3) As String, ocinputuser(4) As Integer, 
        'ocinputtgl(5) As DateTime, ocmodifikasiuser(6) As Integer, ocmodifikasitgl(7) As DateTime

        'MAPPING BUAT FLEX --------------------------------------------------------
        'ockode, ocnama, ocrekdebit, ocrekkredit, ocinputuser, ocinputtgl, ocmodifikasiuser, 
        'ocmodifikasitgl

        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 8) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================

        'VALIDASI TIPE DATA ==========================================================
        'ocinputuser(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "ocinputuser required numeric." : GoTo selesai
        End If
        'ocinputtgl(5) As DateTime
        If (IsDate(dataUtama(5)) = False) Then
            result(2) = "ocinputtgl required date." : GoTo selesai
        End If
        'ocmodifikasiuser(6) As Integer
        If (IsNumeric(dataUtama(6)) = False) Then
            result(2) = "ocmodifikasiuser required numeric." : GoTo selesai
        End If
        'ocmodifikasitgl(7) As DateTime
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "ocmodifikasitgl required date." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA ===================================================

        'VALIDASI DATA ===============================================================
        'ockode(0) As String
        If Len(dataUtama(0)) = 0 Then
            result(2) = "ockode can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 25 Then
            result(2) = "ockode should not be more than 25 character." : GoTo selesai
        End If

        'ocnama(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "ocnama can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 100 Then
            result(2) = "ocnama should not be more than 100 character." : GoTo selesai
        End If

        'ocinputtgl(5) As DateTime
        If Len(dataUtama(5)) = 0 Then
            result(2) = "ocinputtgl can't be empty" : GoTo selesai
        End If

        'ocmodifikasitgl(7) As DateTime
        If Len(dataUtama(7)) = 0 Then
            result(2) = "ocmodifikasitgl can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA ========================================================

        'SIMPAN KE DATABASE ==========================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0

        Try
            If isUpdate Then
                'JIKA UPDATE CEK JML ROW PADA DATABASE
                dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(ockode) FROM M1_Other_Cost WHERE ockode ='" & dataUtama(0) & "'")
                rowUpdate = dtupdate.Rows(0)(0)

                If (rowUpdate > 0) Then
                    'SIMPAN HISTORY ========================
                    Dim SimpanHistory As New m1_other_cost_history
                    Dim o_costSimpanHistory As String = SimpanHistory.M1_Other_Cost_HistorySimpan("" & paramSplit(0) & "★M1_Other_Cost_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(dataUtama(0)) & "")
                    Dim o_costSplit() As String = o_costSimpanHistory.Split(sptParam)
                    Dim o_costSplitResult() As String = o_costSplit(0).Split(sptSubParam)
                    'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                    If (o_costSplitResult(1) = 0) Then
                        result(2) = "Insert history failed : " & o_costSplitResult(2) : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF SIMPAN HISTORY ==================

                    sql = "Update M1_Other_Cost set ocnama  = '" & FixQuotes(dataUtama(1)) & "', ocrekdebit  = '" & FixQuotes(dataUtama(2)) & "', ocrekkredit  = '" & FixQuotes(dataUtama(3)) & "', ocmodifikasiuser  = " & dataUtama(6) & ", ocmodifikasitgl  = NOW() where ockode = '" & dataUtama(0) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Else
                    result(2) = "Data not found." : Trans.Rollback() : GoTo selesai
                End If

            Else
                sql = "Insert into M1_Other_Cost (ockode, ocnama, ocrekdebit, ocrekkredit, ocinputuser, ocinputtgl, ocmodifikasiuser, ocmodifikasitgl) values('" & FixQuotes(dataUtama(0)) & "', '" & FixQuotes(dataUtama(1)) & "', '" & FixQuotes(dataUtama(2)) & "', '" & FixQuotes(dataUtama(3)) & "', " & dataUtama(4) & ", NOW(), " & dataUtama(6) & ", '1971-01-01 00:00:00')"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M1_Other_CostSearch(PostWsSearch(paramSplit(0), "M1_Other_CostSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
            result(2) = "Transaction Rollback : " & ex.Message
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
    Public Function M1_Other_CostSimpan(ByVal param As String) As String
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
        'ockode(0) As String, ocnama(1) As String, ocrekdebit(2) As String, ocrekkredit(3) As String, octermasukhpp(4) As Integer, 
        'occatatan(5) As String, ocinputuser(6) As Integer, ocinputtgl(7) As DateTime, ocmodifikasiuser(8) As Integer, ocmodifikasitgl(9) As DateTime, 
        'occustomtext1(10) As String, occustomtext2(11) As String, occustomtext3(12) As String, occustomtext4(13) As String, occustomtext5(14) As String, 
        'occustomint1(15) As Integer, occustomint2(16) As Integer, occustomint3(17) As Integer, occustomdbl1(18) As Double, occustomdbl2(19) As Double, 
        'occustomdbl3(20) As Double, occustomdate1(21) As Date, occustomdate2(22) As Date, occustomdate3(23) As Date, ockontak(24) As Double

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'ockode, ocnama, ocrekdebit, ocrekkredit, octermasukhpp, occatatan, ocinputuser, 
        'ocinputtgl, ocmodifikasiuser, ocmodifikasitgl, occustomtext1, occustomtext2, occustomtext3, occustomtext4, 
        'occustomtext5, occustomint1, occustomint2, occustomint3, occustomdbl1, occustomdbl2, occustomdbl3, 
        'occustomdate1, occustomdate2, occustomdate3, ockontak

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "ockode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ocnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ocrekdebit", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ocrekkredit", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "octermasukhpp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "occatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ocinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "ocinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ocmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "ocmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "occustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "occustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "occustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "occustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "occustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "occustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "occustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "occustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "occustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "occustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "occustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "occustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "occustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "occustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ockontak", AsEnumTypeData.AsInt64)

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
            'octermasukhpp(4) As Integer
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - octermasukhpp required numeric." : GoTo selesai
            End If
            'ocinputuser(6) As Integer
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - ocinputuser required numeric." : GoTo selesai
            End If
            'ocinputtgl(7) As DateTime
            If (IsDate(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - ocinputtgl required date." : GoTo selesai
            End If
            'ocmodifikasiuser(8) As Integer
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - ocmodifikasiuser required numeric." : GoTo selesai
            End If
            'ocmodifikasitgl(9) As DateTime
            If (IsDate(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - ocmodifikasitgl required date." : GoTo selesai
            End If
            'occustomint1(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - occustomint1 required numeric." : GoTo selesai
            End If
            'occustomint2(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - occustomint2 required numeric." : GoTo selesai
            End If
            'occustomint3(17) As Integer
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - occustomint3 required numeric." : GoTo selesai
            End If
            'occustomdbl1(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - occustomdbl1 required numeric." : GoTo selesai
            End If
            'occustomdbl2(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - occustomdbl2 required numeric." : GoTo selesai
            End If
            'occustomdbl3(20) As Double
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - occustomdbl3 required numeric." : GoTo selesai
            End If
            'occustomdate1(21) As Date
            If (IsDate(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - occustomdate1 required date." : GoTo selesai
            End If
            'occustomdate2(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - occustomdate2 required date." : GoTo selesai
            End If
            'occustomdate3(23) As Date
            If (IsDate(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - occustomdate3 required date." : GoTo selesai
            End If
            'ockontak(24) As Double
            If (IsNumeric(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - ockontak required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'ockode(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - ockode can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - ockode should not be more than 25 character." : GoTo selesai
            End If

            'ocnama(1) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - ocnama can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 100 Then
                result(2) = "Row : " & i & " - ocnama should not be more than 100 character." : GoTo selesai
            End If

            'ocrekdebit(2) As String
            If dataRowDetail(4) = 0 Then
                If Len(dataRowDetail(2)) = 0 Then
                    result(2) = "Row : " & i & " - ocrekdebit can't be empty" : GoTo selesai
                End If
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - ocrekdebit should not be more than 25 character." : GoTo selesai
            End If

            'ocrekkredit(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - ocrekkredit can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 25 Then
                result(2) = "Row : " & i & " - ocrekkredit should not be more than 25 character." : GoTo selesai
            End If

            'ocinputtgl(7) As DateTime
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - ocinputtgl can't be empty" : GoTo selesai
            End If

            'ocmodifikasitgl(9) As DateTime
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - ocmodifikasitgl can't be empty" : GoTo selesai
            End If

            'occustomdbl1(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - occustomdbl1 can't be empty" : GoTo selesai
            End If

            'occustomdbl2(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - occustomdbl2 can't be empty" : GoTo selesai
            End If

            'occustomdbl3(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - occustomdbl3 can't be empty" : GoTo selesai
            End If

            'occustomdate1(21) As Date
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - occustomdate1 can't be empty" : GoTo selesai
            End If

            'occustomdate2(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - occustomdate2 can't be empty" : GoTo selesai
            End If

            'occustomdate3(23) As Date
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - occustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "ockode~ocnama~ocrekdebit~ocrekkredit~octermasukhpp~occatatan~ocinputuser~ocinputtgl~ocmodifikasiuser~ocmodifikasitgl~occustomtext1~occustomtext2~occustomtext3~occustomtext4~occustomtext5~occustomint1~occustomint2~occustomint3~occustomdbl1~occustomdbl2~occustomdbl3~occustomdate1~occustomdate2~occustomdate3~ockontak", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24)) = False Then
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
                        Dim SimpanHistory As New m1_other_cost_history
                        Dim areaSimpanHistory As String = SimpanHistory.M1_Other_Cost_HistorySimpan("" & paramSplit(0) & "★M1_Other_Cost_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(dr1("ockode")) & "")
                        Dim areaSplit() As String = areaSimpanHistory.Split(sptParam)
                        Dim areaSplitResult() As String = areaSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (areaSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & areaSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("ockode")) & "', '" & FixQuotes(dr1("ocnama")) & "', '" & FixQuotes(dr1("ocrekdebit")) & "', '" & FixQuotes(dr1("ocrekkredit")) & "', " & dr1("octermasukhpp") & ", '" & FixQuotes(dr1("occatatan")) & "', " & dr1("ocinputuser") & ", NOW(), " & dr1("ocmodifikasiuser") & ", '1971-01-01 00:00:00', '" & FixQuotes(dr1("occustomtext1")) & "', '" & FixQuotes(dr1("occustomtext2")) & "', '" & FixQuotes(dr1("occustomtext3")) & "', '" & FixQuotes(dr1("occustomtext4")) & "', '" & FixQuotes(dr1("occustomtext5")) & "', " & dr1("occustomint1") & ", " & dr1("occustomint2") & ", " & dr1("occustomint3") & ", '" & FixDouble(dr1("occustomdbl1")) & "', '" & FixDouble(dr1("occustomdbl2")) & "', '" & FixDouble(dr1("occustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("occustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("occustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("occustomdate3"))) & "', '" & FixQuotes(dr1("ockontak")) & "')")
                    Next

                Else
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("ockode")) & "', '" & FixQuotes(dr1("ocnama")) & "', '" & FixQuotes(dr1("ocrekdebit")) & "', '" & FixQuotes(dr1("ocrekkredit")) & "', " & dr1("octermasukhpp") & ", '" & FixQuotes(dr1("occatatan")) & "', " & dr1("ocinputuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("ocinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("ocmodifikasiuser") & ", NOW(), '" & FixQuotes(dr1("occustomtext1")) & "', '" & FixQuotes(dr1("occustomtext2")) & "', '" & FixQuotes(dr1("occustomtext3")) & "', '" & FixQuotes(dr1("occustomtext4")) & "', '" & FixQuotes(dr1("occustomtext5")) & "', " & dr1("occustomint1") & ", " & dr1("occustomint2") & ", " & dr1("occustomint3") & ", '" & FixDouble(dr1("occustomdbl1")) & "', '" & FixDouble(dr1("occustomdbl2")) & "', '" & FixDouble(dr1("occustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("occustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("occustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("occustomdate3"))) & "', '" & FixQuotes(dr1("ockontak")) & "')")
                    Next
                End If

                If Len(strValue2.ToString) > 0 Then
                    sql = "Insert into M1_Other_Cost(ockode, ocnama, ocrekdebit, ocrekkredit, octermasukhpp, occatatan, ocinputuser, ocinputtgl, ocmodifikasiuser, ocmodifikasitgl, occustomtext1, occustomtext2, occustomtext3, occustomtext4, occustomtext5, occustomint1, occustomint2, occustomint3, occustomdbl1, occustomdbl2, occustomdbl3, occustomdate1, occustomdate2, occustomdate3, ockontak) values " & strValue2.ToString & " ON DUPLICATE KEY UPDATE ocnama = VALUES(ocnama), ocrekdebit = VALUES(ocrekdebit), ocrekkredit = VALUES(ocrekkredit), octermasukhpp = VALUES(octermasukhpp), occatatan = VALUES(occatatan), ocinputuser = VALUES(ocinputuser), ocinputtgl = VALUES(ocinputtgl), ocmodifikasiuser = VALUES(ocmodifikasiuser), ocmodifikasitgl = VALUES(ocmodifikasitgl), occustomtext1 = VALUES(occustomtext1), occustomtext2 = VALUES(occustomtext2), occustomtext3 = VALUES(occustomtext3), occustomtext4 = VALUES(occustomtext4), occustomtext5 = VALUES(occustomtext5), occustomint1 = VALUES(occustomint1), occustomint2 = VALUES(occustomint2), occustomint3 = VALUES(occustomint3), occustomdbl1 = VALUES(occustomdbl1), occustomdbl2 = VALUES(occustomdbl2), occustomdbl3 = VALUES(occustomdbl3), occustomdate1 = VALUES(occustomdate1), occustomdate2 = VALUES(occustomdate2), occustomdate3 = VALUES(occustomdate3), ockontak = VALUES(ockontak)"
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
            Dim paramSearch As String = M1_Other_CostSearch(PostWsSearch(paramSplit(0), "M1_Other_CostSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_Other_CostDelete(ByVal param As String) As String

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
            result(2) = "ockode can't be empty." : GoTo selesai
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
            Dim paramTerkait As String = M1_Other_CostTerkait(PostWsTerkait(paramSplit(0), "M1_Other_CostTerkait", pagingSplit(0), pagingSplit(1), "", "", formatTgl, formatTglWaktu, idtransaksi))
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
            Dim SimpanHistory As New m1_other_cost_history
            Dim o_costSimpanHistory As String = SimpanHistory.M1_Other_Cost_HistorySimpan("" & paramSplit(0) & "★M1_Other_Cost_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(idtransaksi) & "")
            Dim o_costSplit() As String = o_costSimpanHistory.Split(sptParam)
            Dim o_costSplitResult() As String = o_costSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (o_costSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & o_costSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            'DELETE
            sql = "DELETE FROM M1_Other_Cost WHERE ockode = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M1_Other_CostSearch(PostWsSearch(paramSplit(0), "M1_Other_CostSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_Other_CostSearch(ByVal param As String) As String
        'M1_Other_CostSearch --------------------------------------------------------
        'ockode, ocnama, ocrekdebit, ocrekkredit, octermasukhpp, occatatan, ocinputuser, 
        'ocinputtgl, ocmodifikasiuser, ocmodifikasitgl, occustomtext1, occustomtext2, occustomtext3, occustomtext4, 
        'occustomtext5, occustomint1, occustomint2, occustomint3, occustomdbl1, occustomdbl2, occustomdbl3, 
        'occustomdate1, occustomdate2, occustomdate3, ocrekdebitnama, ocrekkreditnama, ocinputusernama, ocmodifikasiusernama,
        'ockontak, ockontakkode, ockontaknama

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

        'BUAT QUERY 
        sql = "SELECT oc.ockode, oc.ocnama, oc.ocrekdebit, oc.ocrekkredit, oc.octermasukhpp, oc.occatatan, oc.ocinputuser, oc.ocinputtgl, oc.ocmodifikasiuser, oc.ocmodifikasitgl, oc.occustomtext1, oc.occustomtext2, oc.occustomtext3, oc.occustomtext4, oc.occustomtext5, oc.occustomint1, oc.occustomint2, oc.occustomint3, oc.occustomdbl1, oc.occustomdbl2, oc.occustomdbl3, oc.occustomdate1, oc.occustomdate2, oc.occustomdate3, coa1.cnama as ocrekdebitnama, coa2.cnama as ocrekkreditnama, u1.unama as ocinputusernama, u2.unama as ocmodifikasiusernama, oc.ockontak, c.kkode as ockontakkode, c.knama as ockontaknama FROM m1_other_cost oc LEFT JOIN m1_coa coa1 ON oc.ocrekdebit = coa1.cnomor LEFT JOIN m1_coa coa2 ON oc.ocrekkredit = coa2.cnomor LEFT JOIN m0_user u1 ON oc.ocinputuser = u1.userid LEFT JOIN m0_user u2 ON oc.ocmodifikasiuser = u2.userid LEFT JOIN m1_contact c on oc.ockontak = c.kid"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Other_Cost", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("ockode"), ""), sptField,
                     FxDB(dr("ocnama"), ""), sptField,
                     FxDB(dr("ocrekdebit"), ""), sptField,
                     FxDB(dr("ocrekkredit"), ""), sptField,
                     FxDB(dr("octermasukhpp"), 0), sptField,
                     FxDB(dr("occatatan"), ""), sptField,
                     FxDB(dr("ocinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ocinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ocmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ocmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("occustomtext1"), ""), sptField,
                     FxDB(dr("occustomtext2"), ""), sptField,
                     FxDB(dr("occustomtext3"), ""), sptField,
                     FxDB(dr("occustomtext4"), ""), sptField,
                     FxDB(dr("occustomtext5"), ""), sptField,
                     FxDB(dr("occustomint1"), 0), sptField,
                     FxDB(dr("occustomint2"), 0), sptField,
                     FxDB(dr("occustomint3"), 0), sptField,
                     FxDB(dr("occustomdbl1"), 0), sptField,
                     FxDB(dr("occustomdbl2"), 0), sptField,
                     FxDB(dr("occustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("occustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("occustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("occustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("ocrekdebitnama"), ""), sptField,
                     FxDB(dr("ocrekkreditnama"), ""), sptField,
                     FxDB(dr("ocinputusernama"), ""), sptField,
                     FxDB(dr("ocmodifikasiusernama"), ""), sptField,
                     FxDB(dr("ockontak"), 0), sptField,
                     FxDB(dr("ockontakkode"), ""), sptField,
                     FxDB(dr("ockontaknama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Other Cost data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ockode, ocnama, ocrekdebit, ocrekkredit, octermasukhpp, occatatan, ocinputuser, ocinputtgl, ocmodifikasiuser, ocmodifikasitgl, occustomtext1, occustomtext2, occustomtext3, occustomtext4, occustomtext5, occustomint1, occustomint2, occustomint3, occustomdbl1, occustomdbl2, occustomdbl3, occustomdate1, occustomdate2, occustomdate3, ocrekdebitnama, ocrekkreditnama, ocinputusernama, ocmodifikasiusernama, ockontak, ockontakkode, ockontaknama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Other_CostCekId(ByVal param As String) As String

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
            result(2) = "ockode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'CEK DI DATABASE ================================================================
        Dim dt As DataTable
        Dim exist As Integer = 0
        dt = AsDataTableAmbilDariDB("SELECT COUNT(ockode) FROM m1_other_cost WHERE ockode='" & idtransaksi & "'")
        exist = dt.Rows(0)(0)

        If (exist > 0) Then
            result(2) = "'" & idtransaksi & "' already exist for column ockode." : GoTo selesai
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
    Public Function M1_Other_CostTerkait(ByVal param As String) As String
        'M1_LocationTerkait --------------------------------------------------------
        'ockode, ocnama, sumber, idterkait

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
            result(2) = "ockode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        Dim query As String = "SELECT oc.ockode as ockode, oc.ocnama as ocnama, 'PO' as sumber, po.ponotransaksi as idterkait FROM m4_po_cost poc JOIN m4_po po ON poc.idpo = po.poid JOIN m1_other_cost oc ON poc.kodecost = oc.ockode WHERE oc.ockode = 'valkode' GROUP BY oc.ockode, po.poid UNION ALL SELECT oc.ockode as ockode, oc.ocnama as ocnama, 'GRN' as sumber, grn.grnnotransaksi as idterkait FROM m4_grn_cost grnc JOIN m4_grn grn ON grnc.idgrn = grn.grnid JOIN m1_other_cost oc ON grnc.kodecost = oc.ockode WHERE oc.ockode = 'valkode' GROUP BY oc.ockode, grn.grnid UNION ALL SELECT oc.ockode as ockode, oc.ocnama as ocnama, 'RI' as sumber,  ri.rinotransaksi as idterkait FROM m4_ri_cost ric JOIN m4_ri ri ON ric.idri = ri.riid JOIN m1_other_cost oc ON ric.kodecost = oc.ockode WHERE oc.ockode = 'valkode' GROUP BY oc.ockode, ri.riid"
        query = query.Replace("valkode", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-m1_other_cost", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , query) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("ockode"), ""), sptField,
                             FxDB(dr("ocnama"), ""), sptField,
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
            result(2) = "Related Other Cost data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ockode, ocnama, sumber, idterkait"))

        Return wsResult
    End Function

End Class