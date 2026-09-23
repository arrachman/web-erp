Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m1_oem
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M1_OemSimpan(ByVal param As String) As String
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
        'okode(0) As String, onama(1) As String, ocatatan(2) As String, oaktif(3) As Integer, oinputuser(4) As Integer, 
        'oinputtgl(5) As DateTime, omodifikasiuser(6) As Integer, omodifikasitgl(7) As DateTime, ocustomtext1(8) As String, ocustomtext2(9) As String, 
        'ocustomtext3(10) As String, ocustomtext4(11) As String, ocustomtext5(12) As String, ocustomint1(13) As Integer, ocustomint2(14) As Integer, 
        'ocustomint3(15) As Integer, ocustomdbl1(16) As Double, ocustomdbl2(17) As Double, ocustomdbl3(18) As Double, ocustomdate1(19) As Date, 
        'ocustomdate2(20) As Date, ocustomdate3(21) As Date, oindexbarcode(22) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'okode, onama, ocatatan, oaktif, oinputuser, oinputtgl, omodifikasiuser, 
        'omodifikasitgl, ocustomtext1, ocustomtext2, ocustomtext3, ocustomtext4, ocustomtext5, ocustomint1, 
        'ocustomint2, ocustomint3, ocustomdbl1, ocustomdbl2, ocustomdbl3, ocustomdate1, ocustomdate2, 
        'ocustomdate3, oindexbarcode

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "okode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "onama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ocatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "oaktif", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "oinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "oinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "omodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "omodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ocustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ocustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ocustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ocustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ocustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ocustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "ocustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "ocustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "ocustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ocustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ocustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ocustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ocustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ocustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "oindexbarcode", AsEnumTypeData.AsString)

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
            'oaktif(3) As Integer
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - oaktif required numeric." : GoTo selesai
            End If
            'oinputuser(4) As Integer
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - oinputuser required numeric." : GoTo selesai
            End If
            'oinputtgl(5) As DateTime
            If (IsDate(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - oinputtgl required date." : GoTo selesai
            End If
            'omodifikasiuser(6) As Integer
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - omodifikasiuser required numeric." : GoTo selesai
            End If
            'omodifikasitgl(7) As DateTime
            If (IsDate(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - omodifikasitgl required date." : GoTo selesai
            End If
            'ocustomint1(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - ocustomint1 required numeric." : GoTo selesai
            End If
            'ocustomint2(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - ocustomint2 required numeric." : GoTo selesai
            End If
            'ocustomint3(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - ocustomint3 required numeric." : GoTo selesai
            End If
            'ocustomdbl1(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - ocustomdbl1 required numeric." : GoTo selesai
            End If
            'ocustomdbl2(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - ocustomdbl2 required numeric." : GoTo selesai
            End If
            'ocustomdbl3(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - ocustomdbl3 required numeric." : GoTo selesai
            End If
            'ocustomdate1(19) As Date
            If (IsDate(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - ocustomdate1 required date." : GoTo selesai
            End If
            'ocustomdate2(20) As Date
            If (IsDate(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - ocustomdate2 required date." : GoTo selesai
            End If
            'ocustomdate3(21) As Date
            If (IsDate(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - ocustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'okode(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - okode can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - okode should not be more than 25 character." : GoTo selesai
            End If

            'onama(1) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - onama can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 100 Then
                result(2) = "Row : " & i & " - onama should not be more than 100 character." : GoTo selesai
            End If

            'oinputtgl(5) As DateTime
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - oinputtgl can't be empty" : GoTo selesai
            End If

            'omodifikasitgl(7) As DateTime
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - omodifikasitgl can't be empty" : GoTo selesai
            End If

            'ocustomdbl1(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - ocustomdbl1 can't be empty" : GoTo selesai
            End If

            'ocustomdbl2(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - ocustomdbl2 can't be empty" : GoTo selesai
            End If

            'ocustomdbl3(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - ocustomdbl3 can't be empty" : GoTo selesai
            End If

            'ocustomdate1(19) As Date
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - ocustomdate1 can't be empty" : GoTo selesai
            End If

            'ocustomdate2(20) As Date
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - ocustomdate2 can't be empty" : GoTo selesai
            End If

            'ocustomdate3(21) As Date
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - ocustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "okode~onama~ocatatan~oaktif~oinputuser~oinputtgl~omodifikasiuser~omodifikasitgl~ocustomtext1~ocustomtext2~ocustomtext3~ocustomtext4~ocustomtext5~ocustomint1~ocustomint2~ocustomint3~ocustomdbl1~ocustomdbl2~ocustomdbl3~ocustomdate1~ocustomdate2~ocustomdate3~oindexbarcode", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22)) = False Then
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
                        Dim SimpanHistory As New m1_oem_history
                        Dim areaSimpanHistory As String = SimpanHistory.M1_OemHistorySimpan("" & paramSplit(0) & "★M1_OemHistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(dr1("okode")) & "")
                        Dim areaSplit() As String = areaSimpanHistory.Split(sptParam)
                        Dim areaSplitResult() As String = areaSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (areaSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & areaSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("okode")) & "', '" & FixQuotes(dr1("onama")) & "', '" & FixQuotes(dr1("ocatatan")) & "', " & dr1("oaktif") & ", " & dr1("oinputuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("oinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("omodifikasiuser") & ", NOW(), '" & FixQuotes(dr1("ocustomtext1")) & "', '" & FixQuotes(dr1("ocustomtext2")) & "', '" & FixQuotes(dr1("ocustomtext3")) & "', '" & FixQuotes(dr1("ocustomtext4")) & "', '" & FixQuotes(dr1("ocustomtext5")) & "', " & dr1("ocustomint1") & ", " & dr1("ocustomint2") & ", " & dr1("ocustomint3") & ", '" & FixDouble(dr1("ocustomdbl1")) & "', '" & FixDouble(dr1("ocustomdbl2")) & "', '" & FixDouble(dr1("ocustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("ocustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("ocustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("ocustomdate3"))) & "', '" & FixQuotes(dr1("oindexbarcode")) & "')")
                    Next

                Else
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("okode")) & "', '" & FixQuotes(dr1("onama")) & "', '" & FixQuotes(dr1("ocatatan")) & "', " & dr1("oaktif") & ", " & dr1("oinputuser") & ", NOW(), " & dr1("omodifikasiuser") & ", '1971-01-01 00:00:00', '" & FixQuotes(dr1("ocustomtext1")) & "', '" & FixQuotes(dr1("ocustomtext2")) & "', '" & FixQuotes(dr1("ocustomtext3")) & "', '" & FixQuotes(dr1("ocustomtext4")) & "', '" & FixQuotes(dr1("ocustomtext5")) & "', " & dr1("ocustomint1") & ", " & dr1("ocustomint2") & ", " & dr1("ocustomint3") & ", '" & FixDouble(dr1("ocustomdbl1")) & "', '" & FixDouble(dr1("ocustomdbl2")) & "', '" & FixDouble(dr1("ocustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("ocustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("ocustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("ocustomdate3"))) & "', '" & FixQuotes(dr1("oindexbarcode")) & "')")
                    Next
                End If

                If Len(strValue2.ToString) > 0 Then
                    sql = "Insert into M1_Oem(okode, onama, ocatatan, oaktif, oinputuser, oinputtgl, omodifikasiuser, omodifikasitgl, ocustomtext1, ocustomtext2, ocustomtext3, ocustomtext4, ocustomtext5, ocustomint1, ocustomint2, ocustomint3, ocustomdbl1, ocustomdbl2, ocustomdbl3, ocustomdate1, ocustomdate2, ocustomdate3, oindexbarcode) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE onama = VALUES(onama), ocatatan = VALUES(ocatatan), oaktif = VALUES(oaktif), omodifikasiuser = VALUES(omodifikasiuser), omodifikasitgl = NOW(), ocustomtext1 = VALUES(ocustomtext1), ocustomtext2 = VALUES(ocustomtext2), ocustomtext3 = VALUES(ocustomtext3), ocustomtext4 = VALUES(ocustomtext4), ocustomtext5 = VALUES(ocustomtext5), ocustomint1 = VALUES(ocustomint1), ocustomint2 = VALUES(ocustomint2), ocustomint3 = VALUES(ocustomint3), ocustomdbl1 = VALUES(ocustomdbl1), ocustomdbl2 = VALUES(ocustomdbl2), ocustomdbl3 = VALUES(ocustomdbl3), ocustomdate1 = VALUES(ocustomdate1), ocustomdate2 = VALUES(ocustomdate2), ocustomdate3 = VALUES(ocustomdate3), oindexbarcode = VALUES(oindexbarcode)"
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
            Dim paramSearch As String = M1_OemSearch(PostWsSearch(paramSplit(0), "M1_OemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_OemDelete(ByVal param As String) As String

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
            result(2) = "okode can't be empty." : GoTo selesai
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
            Dim paramTerkait As String = M1_OemTerkait(PostWsTerkait(paramSplit(0), "M1_OemTerkait", pagingSplit(0), pagingSplit(1), "", "", formatTgl, formatTglWaktu, idtransaksi))
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
            Dim SimpanHistory As New m1_oem_history
            Dim areaSimpanHistory As String = SimpanHistory.M1_OemHistorySimpan("" & paramSplit(0) & "★M1_OemHistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(idtransaksi) & "")
            Dim areaSplit() As String = areaSimpanHistory.Split(sptParam)
            Dim areaSplitResult() As String = areaSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (areaSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & areaSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            'DELETE
            sql = "DELETE FROM M1_Oem WHERE okode = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M1_OemSearch(PostWsSearch(paramSplit(0), "M1_OemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_OemSearch(ByVal param As String) As String
        'M1_OemSearch --------------------------------------------------------
        'okode, onama, ocatatan, oaktif, oinputuser, oinputtgl, omodifikasiuser, 
        'omodifikasitgl, ocustomtext1, ocustomtext2, ocustomtext3, ocustomtext4, ocustomtext5, ocustomint1, 
        'ocustomint2, ocustomint3, ocustomdbl1, ocustomdbl2, ocustomdbl3, ocustomdate1, ocustomdate2, 
        'ocustomdate3, oinputusernama, omodifikasiusernama, oindexbarcode

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
        sql = "select `o`.`okode` AS `okode`,`o`.`onama` AS `onama`,`o`.`ocatatan` AS `ocatatan`,`o`.`oaktif` AS `oaktif`,`o`.`oinputuser` AS `oinputuser`,`o`.`oinputtgl` AS `oinputtgl`,`o`.`omodifikasiuser` AS `omodifikasiuser`,`o`.`omodifikasitgl` AS `omodifikasitgl`,`o`.`ocustomtext1` AS `ocustomtext1`,`o`.`ocustomtext2` AS `ocustomtext2`,`o`.`ocustomtext3` AS `ocustomtext3`,`o`.`ocustomtext4` AS `ocustomtext4`,`o`.`ocustomtext5` AS `ocustomtext5`,`o`.`ocustomint1` AS `ocustomint1`,`o`.`ocustomint2` AS `ocustomint2`,`o`.`ocustomint3` AS `ocustomint3`,`o`.`ocustomdbl1` AS `ocustomdbl1`,`o`.`ocustomdbl2` AS `ocustomdbl2`,`o`.`ocustomdbl3` AS `ocustomdbl3`,`o`.`ocustomdate1` AS `ocustomdate1`,`o`.`ocustomdate2` AS `ocustomdate2`,`o`.`ocustomdate3` AS `ocustomdate3`,`u1`.`unama` AS `oinputusernama`,`u2`.`unama` AS `omodifikasiusernama`,`o`.`oindexbarcode` AS `oindexbarcode` from ((`M1_Oem` `o` left join `m0_user` `u1` on((`o`.`oinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`o`.`omodifikasiuser` = `u2`.`userid`)))"

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Oem", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("okode"), ""), sptField,
                     FxDB(dr("onama"), ""), sptField,
                     FxDB(dr("ocatatan"), ""), sptField,
                     FxDB(dr("oaktif"), 0), sptField,
                     FxDB(dr("oinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("oinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("omodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("omodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ocustomtext1"), ""), sptField,
                     FxDB(dr("ocustomtext2"), ""), sptField,
                     FxDB(dr("ocustomtext3"), ""), sptField,
                     FxDB(dr("ocustomtext4"), ""), sptField,
                     FxDB(dr("ocustomtext5"), ""), sptField,
                     FxDB(dr("ocustomint1"), 0), sptField,
                     FxDB(dr("ocustomint2"), 0), sptField,
                     FxDB(dr("ocustomint3"), 0), sptField,
                     FxDB(dr("ocustomdbl1"), 0), sptField,
                     FxDB(dr("ocustomdbl2"), 0), sptField,
                     FxDB(dr("ocustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ocustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ocustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ocustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("oinputusernama"), ""), sptField,
                     FxDB(dr("omodifikasiusernama"), ""), sptField,
                     FxDB(dr("oindexbarcode"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Oem data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("okode, onama, ocatatan, oaktif, oinputuser, oinputtgl, omodifikasiuser, omodifikasitgl, ocustomtext1, ocustomtext2, ocustomtext3, ocustomtext4, ocustomtext5, ocustomint1, ocustomint2, ocustomint3, ocustomdbl1, ocustomdbl2, ocustomdbl3, ocustomdate1, ocustomdate2, ocustomdate3, oinputusernama, omodifikasiusernama, oindexbarcode"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_OemCekId(ByVal param As String) As String

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
            result(2) = "okode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'CEK DI DATABASE ================================================================
        Dim dt As DataTable
        Dim exist As Integer = 0
        dt = AsDataTableAmbilDariDB("SELECT COUNT(okode) FROM M1_Oem WHERE okode='" & idtransaksi & "'")
        exist = dt.Rows(0)(0)

        If (exist > 0) Then
            result(2) = "'" & idtransaksi & "' already exist for column okode." : GoTo selesai
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
    Public Function M1_OemTerkait(ByVal param As String) As String
        'M1_OemTerkait --------------------------------------------------------
        'okode, onama, sumber, idterkait

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
            result(2) = "okode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        sql = "select o.okode AS okode, o.onama AS onama, s.skode AS sumber, CONCAT('Setting : ', s.smodule, ' - ', s.sgrup, ' - ', s.skode) AS idterkait from m0_setting s join m1_oem o on (s.smodule = 0 AND s.sgrup = 'options' AND s.skode = 'KelasProduk' AND s.snilai = o.okode) WHERE o.okode = 'valkode' union all SELECT o.okode as okode, o.onama as onama, 'Item' as sumber, i.bkode as idterkait FROM m1_item i JOIN m1_oem o ON i.bkelasproduk = o.okode AND o.okode = 'valkode' GROUP BY o.okode, i.bid UNION ALL SELECT o.okode as okode, o.onama as onama, 'POS Type' as sumber, ptc.tipepos as idterkait FROM m_12_pos_type_oem ptc JOIN m1_oem o ON ptc.kelasproduk = o.okode AND o.okode = 'valkode' GROUP BY o.okode, ptc.tipepos"
        sql = sql.Replace("valkode", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Oem", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("okode"), ""), sptField,
                             FxDB(dr("onama"), ""), sptField,
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
            result(2) = "Related Oem data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("okode, onama, sumber, idterkait"))

        Return wsResult
    End Function

End Class
