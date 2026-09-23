Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m1_department
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M1_DepartmentSimpan(ByVal param As String) As String
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
        'dpkode(0) As String, dpnama(1) As String, dpdivisi(2) As String, dpsubdivisi(3) As String, dpcatatan(4) As String, 
        'dpaktif(5) As Integer, dpinputuser(6) As Integer, dpinputtgl(7) As DateTime, dpmodifikasiuser(8) As Integer, dpmodifikasitgl(9) As DateTime, 
        'dpcustomtext1(10) As String, dpcustomtext2(11) As String, dpcustomtext3(12) As String, dpcustomtext4(13) As String, dpcustomtext5(14) As String, 
        'dpcustomint1(15) As Integer, dpcustomint2(16) As Integer, dpcustomint3(17) As Integer, dpcustomdbl1(18) As Double, dpcustomdbl2(19) As Double, 
        'dpcustomdbl3(20) As Double, dpcustomdate1(21) As Date, dpcustomdate2(22) As Date, dpcustomdate3(23) As Date, dpindexbarcode(24) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'dpkode, dpnama, dpdivisi, dpsubdivisi, dpcatatan, dpaktif, dpinputuser, 
        'dpinputtgl, dpmodifikasiuser, dpmodifikasitgl, dpcustomtext1, dpcustomtext2, dpcustomtext3, dpcustomtext4, 
        'dpcustomtext5, dpcustomint1, dpcustomint2, dpcustomint3, dpcustomdbl1, dpcustomdbl2, dpcustomdbl3, 
        'dpcustomdate1, dpcustomdate2, dpcustomdate3, dpindexbarcode

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "dpkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dpnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dpdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dpsubdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dpcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dpaktif", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dpinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dpinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dpmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dpmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dpcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dpcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dpcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dpcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dpcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dpcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dpcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dpcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dpcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dpcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dpcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dpcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dpcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dpcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "dpindexbarcode", AsEnumTypeData.AsString)

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
            'dpaktif(5) As Integer
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - dpaktif required numeric." : GoTo selesai
            End If
            'dpinputuser(6) As Integer
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - dpinputuser required numeric." : GoTo selesai
            End If
            'dpinputtgl(7) As DateTime
            If (IsDate(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - dpinputtgl required date." : GoTo selesai
            End If
            'dpmodifikasiuser(8) As Integer
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - dpmodifikasiuser required numeric." : GoTo selesai
            End If
            'dpmodifikasitgl(9) As DateTime
            If (IsDate(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - dpmodifikasitgl required date." : GoTo selesai
            End If
            'dpcustomint1(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - dpcustomint1 required numeric." : GoTo selesai
            End If
            'dpcustomint2(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - dpcustomint2 required numeric." : GoTo selesai
            End If
            'dpcustomint3(17) As Integer
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - dpcustomint3 required numeric." : GoTo selesai
            End If
            'dpcustomdbl1(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - dpcustomdbl1 required numeric." : GoTo selesai
            End If
            'dpcustomdbl2(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - dpcustomdbl2 required numeric." : GoTo selesai
            End If
            'dpcustomdbl3(20) As Double
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - dpcustomdbl3 required numeric." : GoTo selesai
            End If
            'dpcustomdate1(21) As Date
            If (IsDate(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - dpcustomdate1 required date." : GoTo selesai
            End If
            'dpcustomdate2(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - dpcustomdate2 required date." : GoTo selesai
            End If
            'dpcustomdate3(23) As Date
            If (IsDate(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - dpcustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'dpkode(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - dpkode can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 50 Then
                result(2) = "Row : " & i & " - dpkode should not be more than 50 character." : GoTo selesai
            End If

            'dpnama(1) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - dpnama can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 50 Then
                result(2) = "Row : " & i & " - dpnama should not be more than 50 character." : GoTo selesai
            End If

            'dpinputtgl(7) As DateTime
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - dpinputtgl can't be empty" : GoTo selesai
            End If

            'dpmodifikasitgl(9) As DateTime
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - dpmodifikasitgl can't be empty" : GoTo selesai
            End If

            'dpcustomdbl1(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - dpcustomdbl1 can't be empty" : GoTo selesai
            End If

            'dpcustomdbl2(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - dpcustomdbl2 can't be empty" : GoTo selesai
            End If

            'dpcustomdbl3(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - dpcustomdbl3 can't be empty" : GoTo selesai
            End If

            'dpcustomdate1(21) As Date
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - dpcustomdate1 can't be empty" : GoTo selesai
            End If

            'dpcustomdate2(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - dpcustomdate2 can't be empty" : GoTo selesai
            End If

            'dpcustomdate3(23) As Date
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - dpcustomdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "dpkode~dpnama~dpdivisi~dpsubdivisi~dpcatatan~dpaktif~dpinputuser~dpinputtgl~dpmodifikasiuser~dpmodifikasitgl~dpcustomtext1~dpcustomtext2~dpcustomtext3~dpcustomtext4~dpcustomtext5~dpcustomint1~dpcustomint2~dpcustomint3~dpcustomdbl1~dpcustomdbl2~dpcustomdbl3~dpcustomdate1~dpcustomdate2~dpcustomdate3~dpindexbarcode", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24)) = False Then
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
                        Dim SimpanHistory As New m1_department_history
                        Dim areaSimpanHistory As String = SimpanHistory.M1_DepartmentHistorySimpan("" & paramSplit(0) & "★M1_DepartmentHistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(dr1("dpkode")) & "")
                        Dim areaSplit() As String = areaSimpanHistory.Split(sptParam)
                        Dim areaSplitResult() As String = areaSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (areaSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & areaSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("dpkode")) & "', '" & FixQuotes(dr1("dpnama")) & "', '" & FixQuotes(dr1("dpdivisi")) & "', '" & FixQuotes(dr1("dpsubdivisi")) & "', '" & FixQuotes(dr1("dpcatatan")) & "', " & dr1("dpaktif") & ", " & dr1("dpinputuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("dpinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("dpmodifikasiuser") & ", NOW(), '" & FixQuotes(dr1("dpcustomtext1")) & "', '" & FixQuotes(dr1("dpcustomtext2")) & "', '" & FixQuotes(dr1("dpcustomtext3")) & "', '" & FixQuotes(dr1("dpcustomtext4")) & "', '" & FixQuotes(dr1("dpcustomtext5")) & "', " & dr1("dpcustomint1") & ", " & dr1("dpcustomint2") & ", " & dr1("dpcustomint3") & ", '" & FixDouble(dr1("dpcustomdbl1")) & "', '" & FixDouble(dr1("dpcustomdbl2")) & "', '" & FixDouble(dr1("dpcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("dpcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dpcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dpcustomdate3"))) & "', '" & FixQuotes(dr1("dpindexbarcode")) & "')")
                    Next

                Else
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("dpkode")) & "', '" & FixQuotes(dr1("dpnama")) & "', '" & FixQuotes(dr1("dpdivisi")) & "', '" & FixQuotes(dr1("dpsubdivisi")) & "', '" & FixQuotes(dr1("dpcatatan")) & "', " & dr1("dpaktif") & ", " & dr1("dpinputuser") & ", NOW(), 0, '1971-01-01 00:00:00', '" & FixQuotes(dr1("dpcustomtext1")) & "', '" & FixQuotes(dr1("dpcustomtext2")) & "', '" & FixQuotes(dr1("dpcustomtext3")) & "', '" & FixQuotes(dr1("dpcustomtext4")) & "', '" & FixQuotes(dr1("dpcustomtext5")) & "', " & dr1("dpcustomint1") & ", " & dr1("dpcustomint2") & ", " & dr1("dpcustomint3") & ", '" & FixDouble(dr1("dpcustomdbl1")) & "', '" & FixDouble(dr1("dpcustomdbl2")) & "', '" & FixDouble(dr1("dpcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("dpcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dpcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("dpcustomdate3"))) & "', '" & FixQuotes(dr1("dpindexbarcode")) & "')")
                    Next
                End If

                If Len(strValue2.ToString) > 0 Then
                    sql = "Insert into M1_Department(dpkode, dpnama, dpdivisi, dpsubdivisi, dpcatatan, dpaktif, dpinputuser, dpinputtgl, dpmodifikasiuser, dpmodifikasitgl, dpcustomtext1, dpcustomtext2, dpcustomtext3, dpcustomtext4, dpcustomtext5, dpcustomint1, dpcustomint2, dpcustomint3, dpcustomdbl1, dpcustomdbl2, dpcustomdbl3, dpcustomdate1, dpcustomdate2, dpcustomdate3, dpindexbarcode) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE dpnama = VALUES(dpnama), dpdivisi = VALUES(dpdivisi), dpsubdivisi = VALUES(dpsubdivisi), dpcatatan = VALUES(dpcatatan), dpaktif = VALUES(dpaktif), dpinputuser = VALUES(dpinputuser), dpinputtgl = VALUES(dpinputtgl), dpmodifikasiuser = VALUES(dpmodifikasiuser), dpmodifikasitgl = VALUES(dpmodifikasitgl), dpcustomtext1 = VALUES(dpcustomtext1), dpcustomtext2 = VALUES(dpcustomtext2), dpcustomtext3 = VALUES(dpcustomtext3), dpcustomtext4 = VALUES(dpcustomtext4), dpcustomtext5 = VALUES(dpcustomtext5), dpcustomint1 = VALUES(dpcustomint1), dpcustomint2 = VALUES(dpcustomint2), dpcustomint3 = VALUES(dpcustomint3), dpcustomdbl1 = VALUES(dpcustomdbl1), dpcustomdbl2 = VALUES(dpcustomdbl2), dpcustomdbl3 = VALUES(dpcustomdbl3), dpcustomdate1 = VALUES(dpcustomdate1), dpcustomdate2 = VALUES(dpcustomdate2), dpcustomdate3 = VALUES(dpcustomdate3), dpindexbarcode = VALUES(dpindexbarcode)"
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
            Dim paramSearch As String = M1_DepartmentSearch(PostWsSearch(paramSplit(0), "M1_DepartmentSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_DepartmentDelete(ByVal param As String) As String

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
            result(2) = "dpkode can't be empty." : GoTo selesai
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
            Dim paramTerkait As String = M1_DepartmentTerkait(PostWsTerkait(paramSplit(0), "M1_DepartmentTerkait", pagingSplit(0), pagingSplit(1), "", "", formatTgl, formatTglWaktu, idtransaksi))
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
            Dim SimpanHistory As New m1_department_history
            Dim areaSimpanHistory As String = SimpanHistory.M1_DepartmentHistorySimpan("" & paramSplit(0) & "★M1_DepartmentHistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(idtransaksi) & "")
            Dim areaSplit() As String = areaSimpanHistory.Split(sptParam)
            Dim areaSplitResult() As String = areaSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (areaSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & areaSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            'DELETE
            sql = "DELETE FROM M1_Department WHERE dpkode = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M1_DepartmentSearch(PostWsSearch(paramSplit(0), "M1_DepartmentSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_DepartmentSearch(ByVal param As String) As String
        'M1_DepartmentSearch --------------------------------------------------------
        'dpkode, dpnama, dpdivisi, dpsubdivisi, dpcatatan, dpaktif, dpinputuser, 
        'dpinputtgl, dpmodifikasiuser, dpmodifikasitgl, dpcustomtext1, dpcustomtext2, dpcustomtext3, dpcustomtext4, 
        'dpcustomtext5, dpcustomint1, dpcustomint2, dpcustomint3, dpcustomdbl1, dpcustomdbl2, dpcustomdbl3, 
        'dpcustomdate1, dpcustomdate2, dpcustomdate3, dpdivisinama, dpsubdivisinama, dpinputusernama, dpmodifikasiusernama, 
        'dpindexbarcode

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
        sql = "SELECT dp.dpkode, dp.dpnama, dp.dpdivisi, dp.dpsubdivisi, dp.dpcatatan, dp.dpaktif, dp.dpinputuser, dp.dpinputtgl, dp.dpmodifikasiuser, dp.dpmodifikasitgl, dp.dpcustomtext1, dp.dpcustomtext2, dp.dpcustomtext3, dp.dpcustomtext4, dp.dpcustomtext5, dp.dpcustomint1, dp.dpcustomint2, dp.dpcustomint3, dp.dpcustomdbl1, dp.dpcustomdbl2, dp.dpcustomdbl3, dp.dpcustomdate1, dp.dpcustomdate2, dp.dpcustomdate3, d.dnama as dpdivisinama, sd.sdnama as dpsubdivisinama, u1.unama as dpinputusernama, u2.unama as dpmodifikasiusernama, dp.dpindexbarcode FROM m1_department dp LEFT JOIN m1_division d ON dp.dpdivisi = d.dkode LEFT JOIN m1_subdivision sd ON dp.dpsubdivisi = sd.sdkode LEFT JOIN m0_user u1 ON dp.dpinputuser = u1.userid LEFT JOIN m0_user u2 ON dp.dpmodifikasiuser = u2.userid"

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Department", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("dpkode"), ""), sptField,
                     FxDB(dr("dpnama"), ""), sptField,
                     FxDB(dr("dpdivisi"), ""), sptField,
                     FxDB(dr("dpsubdivisi"), ""), sptField,
                     FxDB(dr("dpcatatan"), ""), sptField,
                     FxDB(dr("dpaktif"), 0), sptField,
                     FxDB(dr("dpinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dpinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("dpmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dpmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("dpcustomtext1"), ""), sptField,
                     FxDB(dr("dpcustomtext2"), ""), sptField,
                     FxDB(dr("dpcustomtext3"), ""), sptField,
                     FxDB(dr("dpcustomtext4"), ""), sptField,
                     FxDB(dr("dpcustomtext5"), ""), sptField,
                     FxDB(dr("dpcustomint1"), 0), sptField,
                     FxDB(dr("dpcustomint2"), 0), sptField,
                     FxDB(dr("dpcustomint3"), 0), sptField,
                     FxDB(dr("dpcustomdbl1"), 0), sptField,
                     FxDB(dr("dpcustomdbl2"), 0), sptField,
                     FxDB(dr("dpcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("dpcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("dpcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("dpcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("dpdivisinama"), ""), sptField,
                     FxDB(dr("dpsubdivisinama"), ""), sptField,
                     FxDB(dr("dpinputusernama"), ""), sptField,
                     FxDB(dr("dpmodifikasiusernama"), ""), sptField,
                     FxDB(dr("dpindexbarcode"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Department data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("dpkode, dpnama, dpdivisi, dpsubdivisi, dpcatatan, dpaktif, dpinputuser, dpinputtgl, dpmodifikasiuser, dpmodifikasitgl, dpcustomtext1, dpcustomtext2, dpcustomtext3, dpcustomtext4, dpcustomtext5, dpcustomint1, dpcustomint2, dpcustomint3, dpcustomdbl1, dpcustomdbl2, dpcustomdbl3, dpcustomdate1, dpcustomdate2, dpcustomdate3, dpdivisinama, dpsubdivisinama, dpinputusernama, dpmodifikasiusernama, dpindexbarcode"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_DepartmentCekId(ByVal param As String) As String

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
            result(2) = "dpkode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'CEK DI DATABASE ================================================================
        Dim dt As DataTable
        Dim exist As Integer = 0
        dt = AsDataTableAmbilDariDB("SELECT COUNT(dpkode) FROM M1_Department WHERE dpkode='" & idtransaksi & "'")
        exist = dt.Rows(0)(0)

        If (exist > 0) Then
            result(2) = "'" & idtransaksi & "' already exist for column dpkode." : GoTo selesai
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
    Public Function M1_DepartmentTerkait(ByVal param As String) As String
        'M1_DepartmentTerkait --------------------------------------------------------
        'dpkode, dpnama, sumber, idterkait

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
            result(2) = "dpkode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        sql = "SELECT dp.dpkode as dpkode, dp.dpnama as dpnama, 'Sub Department' as sumber, sdp.sdpkode as idterkait FROM m1_subdepartment sdp JOIN m1_department dp ON sdp.sdpdepartemen = dp.dpkode AND dp.dpkode = 'valkode' GROUP BY dp.dpkode, sdp.sdpkode UNION ALL SELECT dp.dpkode as dpkode, dp.dpnama as dpnama, 'Item' as sumber, i.bkode as idterkait FROM m1_item i JOIN m1_department dp ON i.bdepartemen = dp.dpkode AND dp.dpkode = 'valkode' GROUP BY dp.dpkode, i.bid"
        sql = sql.Replace("valkode", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Department", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("dpkode"), ""), sptField,
                             FxDB(dr("dpnama"), ""), sptField,
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
            result(2) = "Related Department data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("dpkode, dpnama, sumber, idterkait"))

        Return wsResult
    End Function

End Class
