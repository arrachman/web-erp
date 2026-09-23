Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m1_subdepartment
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M1_SubdepartmentSimpan(ByVal param As String) As String
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
        'sdpkode(0) As String, sdpnama(1) As String, sdpdepartemen(2) As String, sdpdivisi(3) As String, sdpsubdivisi(4) As String, 
        'sdpcatatan(5) As String, sdpaktif(6) As Integer, sdpinputuser(7) As Integer, sdpinputtgl(8) As DateTime, sdpmodifikasiuser(9) As Integer, 
        'sdpmodifikasitgl(10) As DateTime, sdpcustomtext1(11) As String, sdpcustomtext2(12) As String, sdpcustomtext3(13) As String, sdpcustomtext4(14) As String, 
        'sdpcustomtext5(15) As String, sdpcustomint1(16) As Integer, sdpcustomint2(17) As Integer, sdpcustomint3(18) As Integer, sdpcustomdbl1(19) As Double, 
        'sdpcustomdbl2(20) As Double, sdpcustomdbl3(21) As Double, sdpcustomdate1(22) As Date, sdpcustomdate2(23) As Date, sdpcustomdate3(24) As Date,
        'sdpindexbarcode(25) As String


        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'sdpkode, sdpnama, sdpdepartemen, sdpdivisi, sdpsubdivisi, sdpcatatan, sdpaktif, 
        'sdpinputuser, sdpinputtgl, sdpmodifikasiuser, sdpmodifikasitgl, sdpcustomtext1, sdpcustomtext2, sdpcustomtext3, 
        'sdpcustomtext4, sdpcustomtext5, sdpcustomint1, sdpcustomint2, sdpcustomint3, sdpcustomdbl1, sdpcustomdbl2, 
        'sdpcustomdbl3, sdpcustomdate1, sdpcustomdate2, sdpcustomdate3, sdpindexbarcode

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "sdpkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sdpnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sdpdepartemen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sdpdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sdpsubdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sdpcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sdpaktif", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sdpinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sdpinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sdpmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sdpmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sdpcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sdpcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sdpcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sdpcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sdpcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sdpcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sdpcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sdpcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sdpcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sdpcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sdpcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sdpcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sdpcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sdpcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sdpindexbarcode", AsEnumTypeData.AsString)

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
            If (dataRowDetail.Length <> 26) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'sdpaktif(6) As Integer
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - sdpaktif required numeric." : GoTo selesai
            End If
            'sdpinputuser(7) As Integer
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - sdpinputuser required numeric." : GoTo selesai
            End If
            'sdpinputtgl(8) As DateTime
            If (IsDate(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - sdpinputtgl required date." : GoTo selesai
            End If
            'sdpmodifikasiuser(9) As Integer
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - sdpmodifikasiuser required numeric." : GoTo selesai
            End If
            'sdpmodifikasitgl(10) As DateTime
            If (IsDate(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - sdpmodifikasitgl required date." : GoTo selesai
            End If
            'sdpcustomint1(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - sdpcustomint1 required numeric." : GoTo selesai
            End If
            'sdpcustomint2(17) As Integer
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - sdpcustomint2 required numeric." : GoTo selesai
            End If
            'sdpcustomint3(18) As Integer
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - sdpcustomint3 required numeric." : GoTo selesai
            End If
            'sdpcustomdbl1(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - sdpcustomdbl1 required numeric." : GoTo selesai
            End If
            'sdpcustomdbl2(20) As Double
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - sdpcustomdbl2 required numeric." : GoTo selesai
            End If
            'sdpcustomdbl3(21) As Double
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - sdpcustomdbl3 required numeric." : GoTo selesai
            End If
            'sdpcustomdate1(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - sdpcustomdate1 required date." : GoTo selesai
            End If
            'sdpcustomdate2(23) As Date
            If (IsDate(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - sdpcustomdate2 required date." : GoTo selesai
            End If
            'sdpcustomdate3(24) As Date
            If (IsDate(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - sdpcustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'sdpkode(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - sdpkode can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 50 Then
                result(2) = "Row : " & i & " - sdpkode should not be more than 50 character." : GoTo selesai
            End If

            'sdpnama(1) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - sdpnama can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 50 Then
                result(2) = "Row : " & i & " - sdpnama should not be more than 50 character." : GoTo selesai
            End If

            'sdpdepartemen(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - sdpdepartemen can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 50 Then
                result(2) = "Row : " & i & " - sdpdepartemen should not be more than 50 character." : GoTo selesai
            End If

            'sdpinputtgl(8) As DateTime
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - sdpinputtgl can't be empty" : GoTo selesai
            End If

            'sdpmodifikasitgl(10) As DateTime
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - sdpmodifikasitgl can't be empty" : GoTo selesai
            End If

            'sdpcustomdbl1(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - sdpcustomdbl1 can't be empty" : GoTo selesai
            End If

            'sdpcustomdbl2(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - sdpcustomdbl2 can't be empty" : GoTo selesai
            End If

            'sdpcustomdbl3(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - sdpcustomdbl3 can't be empty" : GoTo selesai
            End If

            'sdpcustomdate1(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - sdpcustomdate1 can't be empty" : GoTo selesai
            End If

            'sdpcustomdate2(23) As Date
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - sdpcustomdate2 can't be empty" : GoTo selesai
            End If

            'sdpcustomdate3(24) As Date
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - sdpcustomdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "sdpkode~sdpnama~sdpdepartemen~sdpdivisi~sdpsubdivisi~sdpcatatan~sdpaktif~sdpinputuser~sdpinputtgl~sdpmodifikasiuser~sdpmodifikasitgl~sdpcustomtext1~sdpcustomtext2~sdpcustomtext3~sdpcustomtext4~sdpcustomtext5~sdpcustomint1~sdpcustomint2~sdpcustomint3~sdpcustomdbl1~sdpcustomdbl2~sdpcustomdbl3~sdpcustomdate1~sdpcustomdate2~sdpcustomdate3~sdpindexbarcode", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25)) = False Then
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
                        Dim SimpanHistory As New m1_subdepartment_history
                        Dim areaSimpanHistory As String = SimpanHistory.M1_SubdepartmentHistorySimpan("" & paramSplit(0) & "★M1_SubdepartmentHistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(dr1("sdpkode")) & "")
                        Dim areaSplit() As String = areaSimpanHistory.Split(sptParam)
                        Dim areaSplitResult() As String = areaSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (areaSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & areaSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("sdpkode")) & "', '" & FixQuotes(dr1("sdpnama")) & "', '" & FixQuotes(dr1("sdpdepartemen")) & "', '" & FixQuotes(dr1("sdpdivisi")) & "', '" & FixQuotes(dr1("sdpsubdivisi")) & "', '" & FixQuotes(dr1("sdpcatatan")) & "', " & dr1("sdpaktif") & ", " & dr1("sdpinputuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("sdpinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("sdpmodifikasiuser") & ", NOW(), '" & FixQuotes(dr1("sdpcustomtext1")) & "', '" & FixQuotes(dr1("sdpcustomtext2")) & "', '" & FixQuotes(dr1("sdpcustomtext3")) & "', '" & FixQuotes(dr1("sdpcustomtext4")) & "', '" & FixQuotes(dr1("sdpcustomtext5")) & "', " & dr1("sdpcustomint1") & ", " & dr1("sdpcustomint2") & ", " & dr1("sdpcustomint3") & ", '" & FixDouble(dr1("sdpcustomdbl1")) & "', '" & FixDouble(dr1("sdpcustomdbl2")) & "', '" & FixDouble(dr1("sdpcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("sdpcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("sdpcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("sdpcustomdate3"))) & "', '" & FixQuotes(dr1("sdpindexbarcode")) & "')")
                    Next

                Else
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("sdpkode")) & "', '" & FixQuotes(dr1("sdpnama")) & "', '" & FixQuotes(dr1("sdpdepartemen")) & "', '" & FixQuotes(dr1("sdpdivisi")) & "', '" & FixQuotes(dr1("sdpsubdivisi")) & "', '" & FixQuotes(dr1("sdpcatatan")) & "', " & dr1("sdpaktif") & ", " & dr1("sdpinputuser") & ", NOW(), 0, '1971-01-01 00:00:00', '" & FixQuotes(dr1("sdpcustomtext1")) & "', '" & FixQuotes(dr1("sdpcustomtext2")) & "', '" & FixQuotes(dr1("sdpcustomtext3")) & "', '" & FixQuotes(dr1("sdpcustomtext4")) & "', '" & FixQuotes(dr1("sdpcustomtext5")) & "', " & dr1("sdpcustomint1") & ", " & dr1("sdpcustomint2") & ", " & dr1("sdpcustomint3") & ", '" & FixDouble(dr1("sdpcustomdbl1")) & "', '" & FixDouble(dr1("sdpcustomdbl2")) & "', '" & FixDouble(dr1("sdpcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("sdpcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("sdpcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("sdpcustomdate3"))) & "', '" & FixQuotes(dr1("sdpindexbarcode")) & "')")
                    Next
                End If

                If Len(strValue2.ToString) > 0 Then
                    sql = "Insert into M1_Subdepartment(sdpkode, sdpnama, sdpdepartemen, sdpdivisi, sdpsubdivisi, sdpcatatan, sdpaktif, sdpinputuser, sdpinputtgl, sdpmodifikasiuser, sdpmodifikasitgl, sdpcustomtext1, sdpcustomtext2, sdpcustomtext3, sdpcustomtext4, sdpcustomtext5, sdpcustomint1, sdpcustomint2, sdpcustomint3, sdpcustomdbl1, sdpcustomdbl2, sdpcustomdbl3, sdpcustomdate1, sdpcustomdate2, sdpcustomdate3, sdpindexbarcode) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE sdpnama = VALUES(sdpnama), sdpdepartemen = VALUES(sdpdepartemen), sdpdivisi = VALUES(sdpdivisi), sdpsubdivisi = VALUES(sdpsubdivisi), sdpcatatan = VALUES(sdpcatatan), sdpaktif = VALUES(sdpaktif), sdpinputuser = VALUES(sdpinputuser), sdpinputtgl = VALUES(sdpinputtgl), sdpmodifikasiuser = VALUES(sdpmodifikasiuser), sdpmodifikasitgl = VALUES(sdpmodifikasitgl), sdpcustomtext1 = VALUES(sdpcustomtext1), sdpcustomtext2 = VALUES(sdpcustomtext2), sdpcustomtext3 = VALUES(sdpcustomtext3), sdpcustomtext4 = VALUES(sdpcustomtext4), sdpcustomtext5 = VALUES(sdpcustomtext5), sdpcustomint1 = VALUES(sdpcustomint1), sdpcustomint2 = VALUES(sdpcustomint2), sdpcustomint3 = VALUES(sdpcustomint3), sdpcustomdbl1 = VALUES(sdpcustomdbl1), sdpcustomdbl2 = VALUES(sdpcustomdbl2), sdpcustomdbl3 = VALUES(sdpcustomdbl3), sdpcustomdate1 = VALUES(sdpcustomdate1), sdpcustomdate2 = VALUES(sdpcustomdate2), sdpcustomdate3 = VALUES(sdpcustomdate3), sdpindexbarcode = VALUES(sdpindexbarcode)"
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
            Dim paramSearch As String = M1_SubdepartmentSearch(PostWsSearch(paramSplit(0), "M1_SubdepartmentSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_SubdepartmentDelete(ByVal param As String) As String

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
            result(2) = "sdpkode can't be empty." : GoTo selesai
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
            Dim paramTerkait As String = M1_SubdepartmentTerkait(PostWsTerkait(paramSplit(0), "M1_SubdepartmentTerkait", pagingSplit(0), pagingSplit(1), "", "", formatTgl, formatTglWaktu, idtransaksi))
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
            Dim SimpanHistory As New m1_subdepartment_history
            Dim areaSimpanHistory As String = SimpanHistory.M1_SubdepartmentHistorySimpan("" & paramSplit(0) & "★M1_SubdepartmentHistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(idtransaksi) & "")
            Dim areaSplit() As String = areaSimpanHistory.Split(sptParam)
            Dim areaSplitResult() As String = areaSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (areaSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & areaSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            'DELETE
            sql = "DELETE FROM M1_Subdepartment WHERE sdpkode = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M1_SubdepartmentSearch(PostWsSearch(paramSplit(0), "M1_SubdepartmentSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_SubdepartmentSearch(ByVal param As String) As String
        'M1_SubdepartmentSearch --------------------------------------------------------
        'sdpkode, sdpnama, sdpdepartemen, sdpdivisi, sdpsubdivisi, sdpcatatan, sdpaktif, 
        'sdpinputuser, sdpinputtgl, sdpmodifikasiuser, sdpmodifikasitgl, sdpcustomtext1, sdpcustomtext2, sdpcustomtext3, 
        'sdpcustomtext4, sdpcustomtext5, sdpcustomint1, sdpcustomint2, sdpcustomint3, sdpcustomdbl1, sdpcustomdbl2, 
        'sdpcustomdbl3, sdpcustomdate1, sdpcustomdate2, sdpcustomdate3, sdpdepartemennama, sdpdivisinama, sdpsubdivisinama, 
        'sdpinputusernama, sdpmodifikasiusernama, sdpindexbarcode

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
        sql = "SELECT sdp.sdpkode, sdp.sdpnama, sdp.sdpdepartemen, sdp.sdpdivisi, sdp.sdpsubdivisi, sdp.sdpcatatan, sdp.sdpaktif, sdp.sdpinputuser, sdp.sdpinputtgl, sdp.sdpmodifikasiuser, sdp.sdpmodifikasitgl, sdp.sdpcustomtext1, sdp.sdpcustomtext2, sdp.sdpcustomtext3, sdp.sdpcustomtext4, sdp.sdpcustomtext5, sdp.sdpcustomint1, sdp.sdpcustomint2, sdp.sdpcustomint3, sdp.sdpcustomdbl1, sdp.sdpcustomdbl2, sdp.sdpcustomdbl3, sdp.sdpcustomdate1, sdp.sdpcustomdate2, sdp.sdpcustomdate3, dp.dpnama as sdpdepartemennama, d.dnama as sdpdivisinama, sd.sdnama as sdpsubdivisinama, u1.unama as sdpinputusernama, u2.unama as sdpmodifikasiusernama, sdp.sdpindexbarcode FROM m1_subdepartment sdp LEFT JOIN m1_department dp ON sdp.sdpdepartemen = dp.dpkode LEFT JOIN m1_subdivision sd ON sdp.sdpsubdivisi = sd.sdkode LEFT JOIN m1_division d ON sdp.sdpdivisi = d.dkode LEFT JOIN m0_user u1 ON sdp.sdpinputuser = u1.userid LEFT JOIN m0_user u2 ON sdp.sdpmodifikasiuser = u2.userid"

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Subdepartment", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("sdpkode"), ""), sptField,
                     FxDB(dr("sdpnama"), ""), sptField,
                     FxDB(dr("sdpdepartemen"), ""), sptField,
                     FxDB(dr("sdpdivisi"), ""), sptField,
                     FxDB(dr("sdpsubdivisi"), ""), sptField,
                     FxDB(dr("sdpcatatan"), ""), sptField,
                     FxDB(dr("sdpaktif"), 0), sptField,
                     FxDB(dr("sdpinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sdpinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sdpmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sdpmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sdpcustomtext1"), ""), sptField,
                     FxDB(dr("sdpcustomtext2"), ""), sptField,
                     FxDB(dr("sdpcustomtext3"), ""), sptField,
                     FxDB(dr("sdpcustomtext4"), ""), sptField,
                     FxDB(dr("sdpcustomtext5"), ""), sptField,
                     FxDB(dr("sdpcustomint1"), 0), sptField,
                     FxDB(dr("sdpcustomint2"), 0), sptField,
                     FxDB(dr("sdpcustomint3"), 0), sptField,
                     FxDB(dr("sdpcustomdbl1"), 0), sptField,
                     FxDB(dr("sdpcustomdbl2"), 0), sptField,
                     FxDB(dr("sdpcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sdpcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sdpcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sdpcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("sdpdepartemennama"), ""), sptField,
                     FxDB(dr("sdpdivisinama"), ""), sptField,
                     FxDB(dr("sdpsubdivisinama"), ""), sptField,
                     FxDB(dr("sdpinputusernama"), ""), sptField,
                     FxDB(dr("sdpmodifikasiusernama"), ""), sptField,
                     FxDB(dr("sdpindexbarcode"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Subdepartment data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sdpkode, sdpnama, sdpdepartemen, sdpdivisi, sdpsubdivisi, sdpcatatan, sdpaktif, sdpinputuser, sdpinputtgl, sdpmodifikasiuser, sdpmodifikasitgl, sdpcustomtext1, sdpcustomtext2, sdpcustomtext3, sdpcustomtext4, sdpcustomtext5, sdpcustomint1, sdpcustomint2, sdpcustomint3, sdpcustomdbl1, sdpcustomdbl2, sdpcustomdbl3, sdpcustomdate1, sdpcustomdate2, sdpcustomdate3, sdpdepartemennama, sdpdivisinama, sdpsubdivisinama, sdpinputusernama, sdpmodifikasiusernama, sdpindexbarcode"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_SubdepartmentCekId(ByVal param As String) As String

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
            result(2) = "sdpkode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'CEK DI DATABASE ================================================================
        Dim dt As DataTable
        Dim exist As Integer = 0
        dt = AsDataTableAmbilDariDB("SELECT COUNT(sdpkode) FROM M1_Subdepartment WHERE sdpkode='" & idtransaksi & "'")
        exist = dt.Rows(0)(0)

        If (exist > 0) Then
            result(2) = "'" & idtransaksi & "' already exist for column sdpkode." : GoTo selesai
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
    Public Function M1_SubdepartmentTerkait(ByVal param As String) As String
        'M1_SubdepartmentTerkait --------------------------------------------------------
        'sdpkode, sdpnama, sumber, idterkait

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
            result(2) = "sdpkode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        sql = "SELECT sdp.sdpkode as sdpkode, sdp.sdpnama as sdpnama, 'Item' as sumber, i.bkode as idterkait FROM m1_item i JOIN m1_subdepartment sdp ON i.bsubdepartemen = sdp.sdpkode AND sdp.sdpkode = 'valkode' GROUP BY sdp.sdpkode, i.bid"
        sql = sql.Replace("valkode", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Subdepartment", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("sdpkode"), ""), sptField,
                             FxDB(dr("sdpnama"), ""), sptField,
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
            result(2) = "Related Subdepartment data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sdpkode, sdpnama, sumber, idterkait"))

        Return wsResult
    End Function

End Class
