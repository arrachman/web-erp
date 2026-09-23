Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m0_nomor
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M0_NomorSimpan(ByVal param As String) As String

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
        Dim dataRowUtama() As String

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
        'kodetabel(0) As String, moduleid(1) As Integer, menuid(2) As Integer, awalan(3) As String, jmldigit(4) As Integer, 
        'uraian(5) As String, transaksifa(6) As Integer, transaksibarang(7) As Integer, transaksihpp(8) As Integer

        'MAPPING BUAT FLEX --------------------------------------------------------
        'kodetabel, moduleid, menuid, awalan, jmldigit, uraian, transaksifa, 
        'transaksibarang, transaksihpp

        'MAPPING BUAT WS ----------------------------------------------------------
        'kodetabel(0) As String, moduleid(1) As Integer, menuid(2) As Integer, awalan(3) As String, jmldigit(4) As Integer, 
        'uraian(5) As String, transaksifa(6) As Integer, transaksibarang(7) As Integer, transaksihpp(8) As Integer

        'MAPPING BUAT FLEX --------------------------------------------------------
        'kodetabel, moduleid, menuid, awalan, jmldigit, uraian, transaksifa, 
        'transaksibarang, transaksihpp

        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptRow)
        Dim strValue2 As New StringBuilder
        Dim JmlDtDetail As Integer = dataUtama.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowUtama = dataUtama(i - 1).Split(sptField)
            'CEK ARRAY DATA
            If (dataRowUtama.Length <> 9) Then
                result(2) = "Invalid data parameter." + dataUtama.Length.ToString : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ================================================

            'VALIDASI TIPE DATA ==========================================================
            'moduleid(1) As Integer
            If (IsNumeric(dataRowUtama(1)) = False) Then
                result(2) = "moduleid required numeric." : GoTo selesai
            End If
            'menuid(2) As Integer
            If (IsNumeric(dataRowUtama(2)) = False) Then
                result(2) = "menuid required numeric." : GoTo selesai
            End If
            'jmldigit(4) As Integer
            If (IsNumeric(dataRowUtama(4)) = False) Then
                result(2) = "jmldigit required numeric." : GoTo selesai
            End If
            'transaksifa(6) As Integer
            If (IsNumeric(dataRowUtama(6)) = False) Then
                result(2) = "transaksifa required numeric." : GoTo selesai
            End If
            'transaksibarang(7) As Integer
            If (IsNumeric(dataRowUtama(7)) = False) Then
                result(2) = "transaksibarang required numeric." : GoTo selesai
            End If
            'transaksihpp(8) As Integer
            If (IsNumeric(dataRowUtama(8)) = False) Then
                result(2) = "transaksihpp required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA ===================================================

            'VALIDASI DATA ===============================================================
            'kodetabel(0) As String
            If Len(dataRowUtama(0)) = 0 Then
                result(2) = "kodetabel can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(0)) > 10 Then
                result(2) = "kodetabel should not be more than 10 character." : GoTo selesai
            End If

            'awalan(3) As String
            If Len(dataRowUtama(3)) = 0 Then
                result(2) = "awalan can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(3)) > 10 Then
                result(2) = "awalan should not be more than 10 character." : GoTo selesai
            End If

            'uraian(5) As String
            If Len(dataRowUtama(5)) = 0 Then
                result(2) = "uraian can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(5)) > 250 Then
                result(2) = "uraian should not be more than 250 character." : GoTo selesai
            End If

            'END OF VALIDASI DATA ========================================================
            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
            strValue2.Append("('" & FixQuotes(dataRowUtama(0)) & "', '" & FixQuotes(dataRowUtama(1)) & "', '" & FixQuotes(dataRowUtama(2)) & "', '" & FixQuotes(dataRowUtama(3)) & "', '" & FixQuotes(dataRowUtama(4)) & "', '" & FixQuotes(dataRowUtama(5)) & "', '" & FixQuotes(dataRowUtama(6)) & "', '" & FixQuotes(dataRowUtama(7)) & "', '" & FixQuotes(dataRowUtama(8)) & "')")

        Next
        'SIMPAN KE DATABASE ==========================================================

        'SIMPAN KE DATABASE ==========================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0

        Try
            'JIKA UPDATE CEK JML ROW PADA DATABASE

            'insert jika data belum ada, dan update jika data sudah ada                                                                                                                    
            sql = "INSERT INTO m0_nomor(kodetabel, moduleid, menuid, awalan, jmldigit, uraian, transaksifa, transaksibarang, transaksihpp) VALUES " & strValue2.ToString & " ON DUPLICATE KEY UPDATE awalan = VALUES(awalan), jmldigit = VALUES(jmldigit), uraian = VALUES(uraian)"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

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
        strResultData = ""
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M0_Nomor_SSimpan(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String, dataRowUtama() As String

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
        'akode(0) As String, anama(1) As String, acatatan(2) As String, aaktif(3) As Integer, ainputuser(4) As Integer, 
        'ainputtgl(5) As DateTime, amodifikasiuser(6) As Integer, amodifikasitgl(7) As DateTime

        'MAPPING BUAT FLEX --------------------------------------------------------
        'akode, anama, acatatan, aaktif, ainputuser, ainputtgl, amodifikasiuser, 
        'amodifikasitgl

        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA ================================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "kodetabel", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "awalan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmldigit", AsEnumTypeData.AsInt64)


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtUtama As Integer = dataUtama.Length
        For i = 1 To JmlDtUtama
            'SPLIT DATA DETAIL
            dataRowUtama = dataUtama(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowUtama.Length <> 3) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "kodetabel~awalan~jmldigit", dataRowUtama(0) & "~" & dataRowUtama(1) & "~" & dataRowUtama(2)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================

        'SIMPAN KE DATABASE ==========================================================
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

                    sql = "Update m0_nomor set awalan  = '" & FixQuotes(dr1("awalan")) & "', jmldigit  = " & FixQuotes(dr1("jmldigit")) & " where kodetabel = '" & FixQuotes(dr1("kodetabel")) & "'"
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
            'Dim paramSearch As String = M0_Menu_Lang_SSearch(PostWsSearch(paramSplit(0), "M0_Menu_Lang_SSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
            'Dim hasilSearch As New RsHasilWsSearch
            'hasilSearch = GetWsSearch(paramSearch)

            ''result(1) = hasilSearch.success
            ''result(2) = hasilSearch.errmessage

            'resultPaging(0) = hasilSearch.isPaging
            'resultPaging(1) = hasilSearch.isNext
            'resultPaging(2) = hasilSearch.isPrevious
            'resultPaging(3) = hasilSearch.countPage
            'resultPaging(4) = hasilSearch.countRow

            'search = hasilSearch.data
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
    Public Function M0_NomorDelete(ByVal param As String) As String

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
            result(2) = "kodetabel can't be empty." : GoTo selesai
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
            'DELETE
            sql = "DELETE FROM M0_Nomor WHERE kodetabel = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M0_NomorSearch(PostWsSearch(paramSplit(0), "M0_NomorSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M0_NomorSearch(ByVal param As String) As String
        'JIKA MENAMBAHKAN FIELD DISINI MAKA HARUS TAMBAHKAN JUGA LENGTH ARRAY PADA VALIDASI AMBIL USERID WS M0_LOGIN
        'M0_NomorSearch --------------------------------------------------------
        'kodetabel, moduleid, menuid, awalan, jmldigit,
        'uraian, transaksifa, transaksibarang, transaksihpp, catatan


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

        dt = AmbilData("aplikasi1-M0_Nomor", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1) ' Ambil data ke databases
        pg1 = pg1

        ''TUTUP KONEKSI
        'myCon.Close()
        'myCon = Nothing

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("kodetabel"), ""), sptField,
                             FxDB(dr("moduleid"), 0), sptField,
                             FxDB(dr("menuid"), 0), sptField,
                             FxDB(dr("awalan"), ""), sptField,
                             FxDB(dr("jmldigit"), 0), sptField,
                             FxDB(dr("uraian"), ""), sptField,
                             FxDB(dr("transaksifa"), 0), sptField,
                             FxDB(dr("transaksibarang"), 0), sptField,
                             FxDB(dr("transaksihpp"), 0), sptField,
                             FxDB(dr("catatan"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Nomor data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search, sptParam, ReplaceMapping("kodetabel, moduleid, menuid, awalan, jmldigit, uraian, transaksifa, transaksibarang, transaksihpp, catatan"))

        Return wsResult
    End Function

    '<WebMethod()>
    Public Function M0_Notransaksi(ByVal cabang As String, ByVal lokasi As String, ByVal kodetabel As String, ByVal tgl As String, Optional ByVal sumber As String = "", Optional ByVal smodule As Integer = 0, Optional ByVal matauang As String = "", Optional ByVal userid As Integer = 0) As String
        On Error GoTo selesai

        Dim dt As DataTable
        Dim notransaksi As String = "", withSumber As String = "1", mukodenotransaksi As String = ""
        Dim awalan As String = "", withCabang As String = "1", withLokasi As String = "1", resetBulan As String = "1"
        Dim sqlambil As String = "", sql As String = "", withTahun As String = "1", withBulan As String = "1"
        Dim success As Integer = 0, jmldigit As Integer = 0, noberikutnya As Integer = 0
        Dim errmessage As String = "", rsSetting As String = ""
        Dim sgrup As String = IIf(smodule = 0, "accounting", "options")

        'AMBIL SETTING, PAKAI CABANG ATAU TIDAK
        rsSetting = F_getSetting(smodule, sgrup, sumber & "NoTransactionCabang")
        If Len(rsSetting) > 0 Then withCabang = rsSetting
        If withCabang <> 1 Then cabang = "--"

        'AMBIL SETTING, PAKAI LOKASI ATAU TIDAK
        rsSetting = F_getSetting(smodule, sgrup, sumber & "NoTransactionLokasi")
        If Len(rsSetting) > 0 Then withLokasi = rsSetting
        If withLokasi <> 1 Then lokasi = "--"

        'AMBIL SETTING, PAKAI SUMBER ATAU TIDAK
        rsSetting = F_getSetting(smodule, sgrup, sumber & "NoTransactionSumber")
        If Len(rsSetting) > 0 Then withSumber = rsSetting

        'AMBIL SETTING, PAKAI TAHUN ATAU TIDAK
        rsSetting = F_getSetting(smodule, sgrup, sumber & "NoTransactionTahun")
        If Len(rsSetting) > 0 Then withTahun = rsSetting

        'AMBIL SETTING, PAKAI BULAN ATAU TIDAK
        rsSetting = F_getSetting(smodule, sgrup, sumber & "NoTransactionBulan")
        If Len(rsSetting) > 0 Then withBulan = rsSetting

        'AMBIL SETTING, RESET PERBULAN ATAU PERTAHUN
        rsSetting = F_getSetting(smodule, sgrup, sumber & "NoTransactionPeriode")
        If Len(rsSetting) > 0 Then resetBulan = rsSetting

        'SET TAHUN
        Dim thn As String = Year(tgl).ToString.Substring(2, 2)
        'SET BULAN
        Dim bln As String = Month(tgl)

        Dim blnFilter As String = bln
        If resetBulan <> 1 Then blnFilter = "1"

        'If withCabang = 1 Then
        '    If kodetabel = "PR" Then
        '        cabang = "KI"
        '    End If
        'End If

        'Dim vAkunPD As String = "", vAkunSI As String = ""
        'sqlambil = "SELECT IFNULL(c.cnomor,'') as akunPD, IFNULL(c2.cnomor,'') as akunSI FROM m1_location l LEFT JOIN m1_coa c ON l.lalamat2 = c.cnomor LEFT JOIN m1_coa c2 ON l.lkota = c2.cnomor WHERE l.lkode = '" & lokasi & "'"
        'dt = AsDataTableAmbilDariDB(sqlambil)
        'If dt.Rows.Count > 0 Then
        '    vAkunPD = FxDB(dt.Rows(0)("akunPD"), "")
        '    vAkunSI = FxDB(dt.Rows(0)("akunSI"), "")
        'Else
        '    errmessage = "Could not find Transaction Code for '" & lokasi & "' location." : GoTo selesai
        'End If


        If withLokasi = 1 Then
            If kodetabel = "SQ" Then
                'AMBIL LOKASI USER
                sqlambil = "SELECT ulokasi FROM m0_user WHERE userid = '" & userid & "'"
                dt = AsDataTableAmbilDariDB(sqlambil)
                If dt.Rows.Count > 0 Then
                    lokasi = dt.Rows(0)("ulokasi")
                Else
                    errmessage = "Could not find Location for userid : '" & userid & "'." : GoTo selesai
                End If

                'AMBIL KODE TRANSAKSI LOKASI
                sqlambil = "SELECT lkodetransaksi FROM m1_location WHERE lkode = '" & lokasi & "'"
                dt = AsDataTableAmbilDariDB(sqlambil)
                If dt.Rows.Count > 0 Then
                    lokasi = dt.Rows(0)("lkodetransaksi")
                Else
                    errmessage = "Could not find Transaction Code for '" & lokasi & "' location." : GoTo selesai
                End If

            Else
                'AMBIL KODE TRANSAKSI LOKASI
                sqlambil = "SELECT lkodetransaksi FROM m1_location WHERE lkode = '" & lokasi & "'"
                dt = AsDataTableAmbilDariDB(sqlambil)
                If dt.Rows.Count > 0 Then
                    lokasi = dt.Rows(0)("lkodetransaksi")
                Else
                    errmessage = "Could not find Transaction Code for '" & lokasi & "' location." : GoTo selesai
                End If
            End If
        End If

        'AMBIL KODE NO TRANSAKSI MATAUANG
        sqlambil = "SELECT c.ckodenotransaksi FROM m1_currency c WHERE c.ckode = '" & matauang & "'"
        dt = AsDataTableAmbilDariDB(sqlambil)
        If (dt.Rows.Count > 0) Then
            mukodenotransaksi = FxDB(dt.Rows(0)(0), "")
        End If

        'AMBIL AWALAN, JMLDIGIT, NOBERIKUTNYA BERDASARKAN KODETABEL, CABANG, LOKASI, TAHUN, BULAN
        sqlambil = "SELECT n.awalan, n.jmldigit, nb.noberikutnya FROM m0_nomor n JOIN m0_nomor_next nb ON n.kodetabel=SUBSTR(nb.kodetabel FROM 1 FOR length(nb.kodetabel) - length('" & mukodenotransaksi & "')) WHERE n.kodetabel='" & kodetabel & "' AND nb.kodetabel='" & kodetabel & mukodenotransaksi & "' AND nb.cabang='" & cabang & "' AND nb.lokasi='" & lokasi & "' AND nb.tahun='" & thn & "' AND nb.bulan='" & blnFilter & "'"
        dt = AsDataTableAmbilDariDB(sqlambil)
        If (dt.Rows.Count > 0) Then
            awalan = dt.Rows(0)(0) & mukodenotransaksi
            jmldigit = Val(dt.Rows(0)(1))
            noberikutnya = Val(dt.Rows(0)(2))

            'SET SQL
            sql = "UPDATE M0_Nomor_Next SET noberikutnya = '" & noberikutnya + 1 & "' WHERE cabang='" & cabang & "' AND lokasi='" & lokasi & "' AND kodetabel='" & kodetabel & mukodenotransaksi & "' AND tahun='" & Val(thn) & "' AND bulan='" & Val(blnFilter) & "';"

        Else
            'AMBIL AWALAN, JMLDIGIT BERDASARKAN KODETABEL
            sqlambil = "SELECT awalan, jmldigit FROM m0_nomor WHERE kodetabel='" & kodetabel & "'"
            dt = AsDataTableAmbilDariDB(sqlambil)
            If (dt.Rows.Count > 0) Then
                awalan = dt.Rows(0)(0) & mukodenotransaksi
                jmldigit = Val(dt.Rows(0)(1))

                noberikutnya = 1

                'SET SQL
                sql = "Insert into M0_Nomor_Next (cabang, lokasi, kodetabel, tahun, bulan, noberikutnya) values('" & cabang & "', '" & lokasi & "', '" & kodetabel & mukodenotransaksi & "', " & Val(thn) & ", " & Val(blnFilter) & ", '" & noberikutnya + 1 & "');"
            Else
                errmessage = "Could not find '" & kodetabel & "' in m0_nomor." : GoTo selesai
            End If
        End If

        'JIKA TEMPORARY (TE) MAKA TANPA AWALAN SUMBER
        ''If (kodetabel = "SO" And (lokasi = "TE" Or lokasi = "TES")) Or (kodetabel = "SA" And lokasi = "CL") Then
        'If (kodetabel = "SO" And (lokasi = "TE" Or lokasi = "TES")) Then
        '    awalan = ""
        'End If

        'SET NOTRANSAKSI
        'If awalan = "RI" Then
        'notransaksi = String.Concat(IIf(withCabang = 1, cabang, ""), IIf(withLokasi = 1, lokasi, ""), IIf(withTahun = 1, thn, ""))
        'ElseIf awalan = "PR" Or awalan = "PO" Or awalan = "GRN" Or awalan = "RI" Or awalan = "PDR" Or awalan = "MO" Then
        '	notransaksi = String.Concat(IIf(withCabang = 1, cabang, ""), IIf(withSumber = 1, awalan, ""), IIf(withLokasi = 1, lokasi, ""), IIf(withTahun = 1, thn, ""))
        'Else
        'If awalan = "SQ" Then
        'notransaksi = String.Concat(IIf(withCabang = 1, cabang, ""), IIf(withLokasi = 1, lokasi, ""), IIf(withSumber = 1, awalan, ""), IIf(withTahun = 1, thn, ""))
        'ElseIf awalan = "PR" Then
        'notransaksi = String.Concat(IIf(withCabang = 1, cabang, ""), IIf(withSumber = 1, awalan, ""), IIf(withLokasi = 1, lokasi, ""), IIf(withTahun = 1, thn, ""))
        'Else
        notransaksi = String.Concat(IIf(withCabang = 1, cabang, ""), IIf(withLokasi = 1, lokasi, ""), IIf(withSumber = 1, awalan, ""), IIf(withTahun = 1, thn, ""))
        'End If
        'notransaksi = String.Concat(IIf(withCabang = 1, cabang, ""), IIf(withLokasi = 1, lokasi, ""), IIf(withSumber = 1, awalan, ""), IIf(withTahun = 1, thn, ""))

        If withBulan = 1 Then
            'SET BULAN NOTRANSAKSI
            If (bln.Length > 1) Then
                notransaksi = String.Concat(notransaksi, bln)
            Else
                notransaksi = String.Concat(notransaksi, "0", bln)
            End If
        End If

        'SET DIGIT NOTRANSAKSI
        Dim digit As String = noberikutnya.ToString
        For i As Integer = digit.Length + 1 To jmldigit
            digit = "0" & digit
        Next

        notransaksi = String.Concat(notransaksi, digit)
        'notransaksi = String.Concat(notransaksi, "-", digit)

        'If kodetabel.ToUpper = "PDR" Or kodetabel.ToUpper = "CL" Then
        '    sql &= "INSERT INTO `m1_cost_center` (`cckode`, `ccnama`, `ccakun`, `cccatatan`) VALUES ('" & FixQuotes(notransaksi) & "', '" & FixQuotes(notransaksi) & "', '" & FixQuotes(vAkunPD) & "', '" & FixQuotes(vAkunSI) & "') ON DUPLICATE KEY UPDATE cckode = VALUES(cckode);"
        'End If

        success = 1
selesai:
        Return String.Concat(success, sptSubParam, errmessage, sptSubParam, notransaksi, sptSubParam, sql)
    End Function

    <WebMethod()>
    Public Function M0_NotransaksiKJ(ByVal cabang As String, ByVal lokasi As String, ByVal kodetabel As String, ByVal tgl As String) As String
        On Error GoTo selesai

        Dim dt As DataTable
        Dim notransaksi As String = ""
        Dim awalan As String = ""
        Dim sqlambil As String = "", sql As String = ""
        Dim success As Integer = 0, jmldigit As Integer = 0, noberikutnya As Integer = 0
        Dim errmessage As String = ""

        'SET TAHUN
        Dim thn As String = Year(tgl).ToString.Substring(2, 2)
        'SET BULAN
        Dim bln As String = Month(tgl)

        ''AMBIL KODE TRANSAKSI LOKASI
        'sqlambil = "SELECT lkodetransaksi FROM m1_location WHERE lkode = '" & lokasi & "'"
        'dt = AsDataTableAmbilDariDB(sqlambil)
        'If dt.Rows.Count > 0 Then
        '    lokasi = dt.Rows(0)("lkodetransaksi")
        'Else
        '    errmessage = "Could not find Transaction Code for '" & lokasi & "' location." : GoTo selesai
        'End If

        'AMBIL AWALAN, JMLDIGIT, NOBERIKUTNYA BERDASARKAN KODETABEL, CABANG, LOKASI, TAHUN, BULAN
        sqlambil = "SELECT n.awalan, n.jmldigit, nb.noberikutnya FROM m0_nomor n JOIN m0_nomor_next nb ON n.kodetabel=nb.kodetabel WHERE n.kodetabel='" & kodetabel & "' AND nb.cabang='" & cabang & "' AND nb.lokasi='" & lokasi & "' AND nb.tahun='" & thn & "' AND nb.bulan='" & bln & "'"
        dt = AsDataTableAmbilDariDB(sqlambil)
        If (dt.Rows.Count > 0) Then
            awalan = dt.Rows(0)(0)
            jmldigit = Val(dt.Rows(0)(1))
            noberikutnya = Val(dt.Rows(0)(2))

            'SET SQL
            sql = "UPDATE M0_Nomor_Next SET noberikutnya = '" & noberikutnya + 1 & "' WHERE cabang='" & cabang & "' AND lokasi='" & lokasi & "' AND kodetabel='" & kodetabel & "' AND tahun='" & Val(thn) & "' AND bulan='" & Val(bln) & "'"

        Else
            'AMBIL AWALAN, JMLDIGIT BERDASARKAN KODETABEL
            sqlambil = "SELECT awalan, jmldigit FROM m0_nomor WHERE kodetabel='" & kodetabel & "'"
            dt = AsDataTableAmbilDariDB(sqlambil)
            If (dt.Rows.Count > 0) Then
                awalan = dt.Rows(0)(0)
                jmldigit = Val(dt.Rows(0)(1))
                noberikutnya = 1

                'SET SQL
                sql = "Insert into M0_Nomor_Next (cabang, lokasi, kodetabel, tahun, bulan, noberikutnya) values('" & cabang & "', '" & lokasi & "', '" & kodetabel & "', " & Val(thn) & ", " & Val(bln) & ", '" & 2 & "')"
            Else
                errmessage = "Could not find '" & kodetabel & "' in m0_nomor." : GoTo selesai
            End If
        End If

        'SET NOTRANSAKSI
        notransaksi = String.Concat(cabang, lokasi, awalan, thn)
        'SET BULAN NOTRANSAKSI
        If (bln.Length > 1) Then
            notransaksi = String.Concat(notransaksi, bln)
        Else
            notransaksi = String.Concat(notransaksi, "0", bln)
        End If

        'SET DIGIT NOTRANSAKSI
        Dim digit As String = noberikutnya.ToString
        For i As Integer = digit.Length + 1 To jmldigit
            digit = "0" & digit
        Next

        notransaksi = String.Concat(notransaksi, digit)
        'notransaksi = String.Concat(notransaksi, "-", digit)

        success = 1
selesai:
        Return String.Concat(success, sptSubParam, errmessage, sptSubParam, notransaksi, sptSubParam, sql)
    End Function

    <WebMethod()>
    Public Function M0_NoResepRJ(ByVal perawatan As String) As String
        On Error GoTo selesai

        Dim dt As DataTable
        Dim notransaksi As String = ""
        Dim noresep As String = ""
        Dim awalan As String = ""
        Dim sqlambil As String = "", sql As String = ""
        Dim success As Integer = 0, jmldigit As Integer = 0, noberikutnya As Integer = 0
        Dim errmessage As String = ""

        'SET TAHUN
        'Dim thn As String = Year(tgl).ToString.Substring(2, 2)
        ''SET BULAN
        'Dim bln As String = Month(tgl)

        ''AMBIL KODE TRANSAKSI LOKASI
        'sqlambil = "SELECT lkodetransaksi FROM m1_location WHERE lkode = '" & lokasi & "'"
        'dt = AsDataTableAmbilDariDB(sqlambil)
        'If dt.Rows.Count > 0 Then
        '    lokasi = dt.Rows(0)("lkodetransaksi")
        'Else
        '    errmessage = "Could not find Transaction Code for '" & lokasi & "' location." : GoTo selesai
        'End If

        'AMBIL AWALAN, JMLDIGIT, NOBERIKUTNYA BERDASARKAN KODETABEL, CABANG, LOKASI, TAHUN, BULAN
        sqlambil = "SELECT aknoref, aknotransaksi FROM m_11_ak WHERE akperawatan = '" & perawatan & "' ORDER BY aknoref DESC"
        dt = AsDataTableAmbilDariDB(sqlambil)
        If (dt.Rows.Count > 0) Then
            noresep = dt.Rows(0)(0)


            '    'SET SQL
            '    sql = "UPDATE M0_Nomor_Next SET noberikutnya = '" & noberikutnya + 1 & "' WHERE cabang='" & cabang & "' AND lokasi='" & lokasi & "' AND kodetabel='" & kodetabel & "' AND tahun='" & Val(thn) & "' AND bulan='" & Val(bln) & "'"

            'Else
            '    'AMBIL AWALAN, JMLDIGIT BERDASARKAN KODETABEL
            '    sqlambil = "SELECT awalan, jmldigit FROM m0_nomor WHERE kodetabel='" & kodetabel & "'"
            '    dt = AsDataTableAmbilDariDB(sqlambil)
            '    If (dt.Rows.Count > 0) Then
            '        awalan = dt.Rows(0)(0)
            '        jmldigit = Val(dt.Rows(0)(1))
            '        noberikutnya = 1

            '        'SET SQL
            '        sql = "Insert into M0_Nomor_Next (cabang, lokasi, kodetabel, tahun, bulan, noberikutnya) values('" & cabang & "', '" & lokasi & "', '" & kodetabel & "', " & Val(thn) & ", " & Val(bln) & ", '" & 2 & "')"
            '    Else
            '        errmessage = "Could not find '" & kodetabel & "' in m0_nomor." : GoTo selesai
            '    End If
        End If

        jmldigit = Integer.Parse(noresep)
        noberikutnya = jmldigit + 1
        'SET NOTRANSAKSI
        'notransaksi = String.Concat(cabang, lokasi, awalan, thn)
        ''SET BULAN NOTRANSAKSI
        'If (bln.Length > 1) Then
        '    notransaksi = String.Concat(notransaksi, bln)
        'Else
        '    notransaksi = String.Concat(notransaksi, "0", bln)
        'End If

        'SET DIGIT NOTRANSAKSI
        Dim digit As String = noberikutnya.ToString
        For i As Integer = 1 To 3 - digit.Length
            digit = "0" & digit
        Next

        notransaksi = digit
        'notransaksi = String.Concat(notransaksi, "-", digit)

        success = 1
selesai:
        Return String.Concat(success, sptSubParam, errmessage, sptSubParam, notransaksi, sptSubParam, sqlambil)
    End Function

    <WebMethod()>
    Public Function M0_NogrupRQ(ByVal cabang As String, ByVal lokasi As String, ByVal tgl As String) As String
        On Error GoTo selesai

        Dim dt As DataTable
        Dim nogrup As String = "RQ"
        Dim sqlambil As String = "", sql As String = ""
        Dim success As Integer = 0, jmldigit As Integer = 0, noberikutnya As Integer = 0
        Dim errmessage As String = ""

        'SET TAHUN
        Dim thn As String = Year(tgl).ToString.Substring(2, 2)
        'SET BULAN
        Dim bln As String = Month(tgl)

        'AMBIL JMLDIGIT, NOBERIKUTNYA BERDASARKAN TAHUN, BULAN
        sqlambil = "SELECT grq.cabang, grq.lokasi, grq.tahun, grq.bulan, grq.noberikutnya, (SELECT snilai FROM m0_setting s WHERE smodule=0 AND sgrup='options' AND skode='DigitGroupRQ') as jmldigit FROM m0_group_rq grq WHERE cabang='A' AND lokasi='A' AND tahun='" & thn & "' AND bulan='" & bln & "'"
        dt = AsDataTableAmbilDariDB(sqlambil)
        If (dt.Rows.Count > 0) Then
            jmldigit = Val(dt.Rows(0)(5))
            noberikutnya = Val(dt.Rows(0)(4))

            'SET SQL UPDATE noberikutnya PADA TABEL m0_group_rq
            sql = "UPDATE m0_group_rq SET noberikutnya = '" & noberikutnya + 1 & "' WHERE cabang='A' AND lokasi='A' AND tahun='" & thn & "' AND bulan='" & bln & "'"
        Else
            'AMBIL JMLDIGIT BERDASARKAN KODETABEL
            sqlambil = "SELECT snilai FROM m0_setting s WHERE smodule=0 AND sgrup='options' AND skode='DigitGroupRQ'"
            dt = AsDataTableAmbilDariDB(sqlambil)
            If (dt.Rows.Count > 0) Then
                jmldigit = Val(dt.Rows(0)(0))
                noberikutnya = 1

                'SET SQL
                sql = "INSERT INTO m0_group_rq (cabang, lokasi, tahun, bulan, noberikutnya) VALUES ('A', 'A', '" & thn & "', '" & bln & "', '" & noberikutnya & "')"
            Else
                errmessage = "Could not find setting for RQ Group Number in m0_setting." : GoTo selesai
            End If
        End If

        'SET NOTRANSAKSI
        nogrup = String.Concat(nogrup, thn)
        'SET BULAN NOTRANSAKSI
        If (bln.Length > 1) Then
            nogrup = String.Concat(nogrup, bln)
        Else
            nogrup = String.Concat(nogrup, "0", bln)
        End If
        'SET DIGIT NOTRANSAKSI
        Dim digit As String = noberikutnya.ToString
        For i As Integer = digit.Length + 1 To jmldigit
            digit = "0" & digit
        Next
        nogrup = String.Concat(nogrup, digit)
        success = 1
selesai:
        Return String.Concat(success, sptSubParam, errmessage, sptSubParam, nogrup, sptSubParam, sql)
    End Function

    <WebMethod()>
    Public Function M0_NogrupAQ(ByVal cabang As String, ByVal lokasi As String, ByVal tgl As String) As String
        On Error GoTo selesai

        Dim dt As DataTable
        Dim nogrup As String = "AQ"
        Dim sqlambil As String = "", sql As String = ""
        Dim success As Integer = 0, jmldigit As Integer = 0, noberikutnya As Integer = 0
        Dim errmessage As String = ""

        'SET TAHUN
        Dim thn As String = Year(tgl).ToString.Substring(2, 2)
        'SET BULAN
        Dim bln As String = Month(tgl)

        'AMBIL JMLDIGIT, NOBERIKUTNYA BERDASARKAN TAHUN, BULAN
        sqlambil = "SELECT grq.cabang, grq.lokasi, grq.tahun, grq.bulan, grq.noberikutnya, (SELECT snilai FROM m0_setting s WHERE smodule=0 AND sgrup='options' AND skode='DigitGroupAQ') as jmldigit FROM m0_group_rq grq WHERE cabang='A' AND lokasi='A' AND tahun='" & thn & "' AND bulan='" & bln & "'"
        dt = AsDataTableAmbilDariDB(sqlambil)
        If (dt.Rows.Count > 0) Then
            jmldigit = Val(dt.Rows(0)(5))
            noberikutnya = Val(dt.Rows(0)(4))

            'SET SQL UPDATE noberikutnya PADA TABEL m0_group_rq
            sql = "UPDATE m0_group_rq SET noberikutnya = '" & noberikutnya + 1 & "' WHERE cabang='A' AND lokasi='A' AND tahun='" & thn & "' AND bulan='" & bln & "'"
        Else
            'AMBIL JMLDIGIT BERDASARKAN KODETABEL
            sqlambil = "SELECT snilai FROM m0_setting s WHERE smodule=0 AND sgrup='options' AND skode='DigitGroupAQ'"
            dt = AsDataTableAmbilDariDB(sqlambil)
            If (dt.Rows.Count > 0) Then
                jmldigit = Val(dt.Rows(0)(0))
                noberikutnya = 1

                'SET SQL
                sql = "INSERT INTO m0_group_aq (cabang, lokasi, tahun, bulan, noberikutnya) VALUES ('A', 'A', '" & thn & "', '" & bln & "', '" & noberikutnya & "')"
            Else
                errmessage = "Could not find setting for AQ Group Number in m0_setting." : GoTo selesai
            End If
        End If

        'SET NOTRANSAKSI
        nogrup = String.Concat(nogrup, thn)
        'SET BULAN NOTRANSAKSI
        If (bln.Length > 1) Then
            nogrup = String.Concat(nogrup, bln)
        Else
            nogrup = String.Concat(nogrup, "0", bln)
        End If
        'SET DIGIT NOTRANSAKSI
        Dim digit As String = noberikutnya.ToString
        For i As Integer = digit.Length + 1 To jmldigit
            digit = "0" & digit
        Next
        nogrup = String.Concat(nogrup, digit)
        success = 1
selesai:
        Return String.Concat(success, sptSubParam, errmessage, sptSubParam, nogrup, sptSubParam, sql)
    End Function

    <WebMethod()>
    Public Function M0_GenerateNoSerial(ByVal param As String) As String

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
        Dim awalan As String = "", format As String = "", noAwal As Double = 0, jml As Double = 0
        Dim idtrans(2) As String
        idtrans = paramSplit(5).Split(sptSubParam)
        If (idtrans.Length <> 4) Then
            result(2) = "Invalid primary key parameter." : GoTo selesai
        Else
            'CEK AWALAN
            awalan = idtrans(0)
            'CEK FORMAT
            If (Len(idtrans(1)) = 0) Then
                result(2) = "format can't be empty" : GoTo selesai
            Else
                format = idtrans(1)
            End If
            'CEK NOAWAL
            If (IsNumeric(idtrans(2)) = False) Then
                result(2) = "noAwal required numeric" : GoTo selesai
            Else
                noAwal = idtrans(2)
            End If
            'CEK JML
            If (IsNumeric(idtrans(3)) = False) Then
                result(2) = "jml required numeric" : GoTo selesai
            Else
                jml = idtrans(3)
            End If
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        Dim noserial As String = "", formatting As String = ""

        For i As Integer = noAwal To noAwal + jml - 1
            If format.Length > 0 Then formatting = format.Substring(0, format.Length - i.ToString.Length)
            noserial = awalan & formatting & i
            If search.Length > 0 Then search = String.Concat(search & sptField & noserial) Else search = String.Concat(search & noserial)
        Next

        result(1) = 1
        result(2) = ""
        result(3) = 0
        result(4) = idtransaksi

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
    Public Function M0_GenerateBarcode(ByVal param As String) As String

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", nobarcode As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean

        Dim dt As New DataTable

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
        'kategoribarang(0) As String, divisi(1) As String, subdivisi(2) As String, departemen(3) As String, subdepartemen(4) As String, 
        'kelas(5) As String, subkelas(6) As String, warna(7) As String, designer(8) As String, model(9) As String, 
        'merk(10) As String, bahan(11) As String, oem(12) As String, golongan(13) As String, ukuran(14) As String, 
        'satuan(15) As String, vendor(16) As String

        'MAPPING BUAT FLEX --------------------------------------------------------
        'kategoribarang, divisi, subdivisi, departemen, subdepartemen, 
        'kelas, subkelas, warna, designer, model, 
        'merk, bahan, oem, golongan, ukuran, 
        'satuan, vendor


        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 17) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================


        'AMBIL SETTING BARCODE
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        Dim idxBarcode As String = ""
        sql = "SELECT snilai FROM m0_setting WHERE smodule = 0 AND sgrup = 'options' AND skode = 'IndexBarcode'"
        dt = AsDataTableAmbilDariDB(sql)
        If dt.Rows.Count > 0 Then
            idxBarcode = FxDB(dt.Rows(0)("snilai"), "")
        End If

        Dim cat As String = "", dvs As String = "", sdv As String = "", dpt As String = "", sdp As String = ""
        Dim cls As String = "", scl As String = "", clr As String = "", dsg As String = "", mdl As String = ""
        Dim mrk As String = "", mtr As String = "", oem As String = "", sct As String = "", sze As String = ""
        Dim unt As String = "", vdr As String = ""

        'KODE MASTER INDEX
        'cat : kategori barang
        'dvs : divisi
        'sdv : subdivisi
        'dpt : departemen
        'sdp : subdepartemen
        'cls : kelas
        'scl : subkelas
        'clr : warna
        'dsg : designer
        'mdl : model (style)
        'mrk : merk
        'mtr : bahan (material)
        'oem : oem
        'sct : golongan (section)
        'sze : ukuran
        'unt : satuan
        'vdr : vendor

        'VALIDASI DATA ===============================================================
        'kategoribarang(0) As String
        If idxBarcode.Contains("cat") And Len(dataUtama(0)) = 0 Then
            result(2) = "item category can't be empty" : GoTo selesai
        Else
            cat = dataUtama(0)
        End If

        'divisi(1) As String
        If idxBarcode.Contains("dvs") And Len(dataUtama(1)) = 0 Then
            result(2) = "division can't be empty" : GoTo selesai
        Else
            dvs = dataUtama(1)
        End If

        'subdivisi(2) As String
        If idxBarcode.Contains("sdv") And Len(dataUtama(2)) = 0 Then
            result(2) = "sub division can't be empty" : GoTo selesai
        Else
            sdv = dataUtama(2)
        End If

        'departemen(3) As String
        If idxBarcode.Contains("dpt") And Len(dataUtama(3)) = 0 Then
            result(2) = "department can't be empty" : GoTo selesai
        Else
            dpt = dataUtama(3)
        End If

        'subdepartemen(4) As String
        If idxBarcode.Contains("sdp") And Len(dataUtama(4)) = 0 Then
            result(2) = "sub department can't be empty" : GoTo selesai
        Else
            sdp = dataUtama(4)
        End If

        'kelas(5) As String
        If idxBarcode.Contains("cls") And Len(dataUtama(5)) = 0 Then
            result(2) = "class can't be empty" : GoTo selesai
        Else
            cls = dataUtama(5)
        End If

        'subkelas(6) As String
        If idxBarcode.Contains("scl") And Len(dataUtama(6)) = 0 Then
            result(2) = "sub class can't be empty" : GoTo selesai
        Else
            scl = dataUtama(6)
        End If

        'warna(7) As String
        If idxBarcode.Contains("clr") And Len(dataUtama(7)) = 0 Then
            result(2) = "color can't be empty" : GoTo selesai
        Else
            clr = dataUtama(7)
        End If

        'designer(8) As String
        If idxBarcode.Contains("dsg") And Len(dataUtama(8)) = 0 Then
            result(2) = "designer can't be empty" : GoTo selesai
        Else
            dsg = dataUtama(8)
        End If

        'model(9) As String
        If idxBarcode.Contains("mdl") And Len(dataUtama(9)) = 0 Then
            result(2) = "model can't be empty" : GoTo selesai
        Else
            mdl = dataUtama(9)
        End If

        'merk(10) As String
        If idxBarcode.Contains("mrk") And Len(dataUtama(10)) = 0 Then
            result(2) = "merk can't be empty" : GoTo selesai
        Else
            mrk = dataUtama(10)
        End If

        'bahan(11) As String
        If idxBarcode.Contains("mtr") And Len(dataUtama(11)) = 0 Then
            result(2) = "material can't be empty" : GoTo selesai
        Else
            mtr = dataUtama(11)
        End If

        'oem(12) As String
        If idxBarcode.Contains("oem") And Len(dataUtama(12)) = 0 Then
            result(2) = "oem can't be empty" : GoTo selesai
        Else
            oem = dataUtama(12)
        End If

        'golongan(13) As String
        If idxBarcode.Contains("sct") And Len(dataUtama(13)) = 0 Then
            result(2) = "section can't be empty" : GoTo selesai
        Else
            sct = dataUtama(13)
        End If

        'ukuran(14) As String
        If idxBarcode.Contains("sze") And Len(dataUtama(14)) = 0 Then
            result(2) = "size can't be empty" : GoTo selesai
        Else
            sze = dataUtama(14)
        End If

        'satuan(15) As String
        If idxBarcode.Contains("unt") And Len(dataUtama(15)) = 0 Then
            result(2) = "unit can't be empty" : GoTo selesai
        Else
            unt = dataUtama(15)
        End If

        'vendor(16) As String
        If idxBarcode.Contains("vdr") And Len(dataUtama(16)) = 0 Then
            result(2) = "vendor can't be empty" : GoTo selesai
        Else
            vdr = dataUtama(16)
        End If
        'END OF VALIDASI DATA ========================================================


        'GENERATE BARCODE ============================================================
        Try
            'BARCODE DIGENERATE BERDASARKAN INDEX PADA SETTING + URUTAN
            'KODE INDEX ADA DI MASING2 MASTER DATA YANG TERCANTUM
            'CONTOH SETTING IDXBARCODE dpt~cls~scl~mdl~mtr~clr~sze~sct~vdr~mrk~dsg
            'MAKA AMBIL INDEX DARI MASTER DEPARTMENT~CLASS~SUBCLASS, DST... SESUAI KODE MASTER INDEX
            'KODE MASTER INDEX
            'cat : kategori barang
            'dvs : divisi
            'sdv : subdivisi
            'dpt : departemen
            'sdp : subdepartemen
            'cls : kelas
            'scl : subkelas
            'clr : warna
            'dsg : designer
            'mdl : model (style)
            'mrk : merk
            'mtr : bahan (material)
            'oem : oem
            'sct : golongan (section)
            'sze : ukuran
            'unt : satuan
            'vdr : vendor

            'SPLIT SETTING IDXBARCODE DENGAN ~
            Dim idxSplit() As String = idxBarcode.Split("~")
            If idxSplit.Length > 0 Then

                'PERULANGAN UNTUK AMBIL KODE INDEX BARCODE SESUAI SETTING STRUKTUR BARCODE
                For i As Integer = 0 To idxSplit.Length - 1

                    'RESET DATATABLE
                    dt.Clear()

                    'BUAT SQL AMBIL KODE INDEX BARCODE DARI MASING2 MASTER
                    Select Case idxSplit(i)
                        Case "cat" : sql = "SELECT icindexbarcode  as indexbarcode FROM m1_item_category WHERE ickode  = '" & FixQuotes(cat) & "'"
                        Case "dvs" : sql = "SELECT dindexbarcode   as indexbarcode FROM m1_division      WHERE dkode   = '" & FixQuotes(dvs) & "'"
                        Case "sdv" : sql = "SELECT sdindexbarcode  as indexbarcode FROM m1_subdivision   WHERE sdkode  = '" & FixQuotes(sdv) & "'"
                        Case "dpt" : sql = "SELECT dpindexbarcode  as indexbarcode FROM m1_department    WHERE dpkode  = '" & FixQuotes(dpt) & "'"
                        Case "sdp" : sql = "SELECT sdpindexbarcode as indexbarcode FROM m1_subdepartment WHERE sdpkode = '" & FixQuotes(sdp) & "'"
                        Case "cls" : sql = "SELECT cindexbarcode   as indexbarcode FROM m1_class         WHERE ckode   = '" & FixQuotes(cls) & "'"
                        Case "scl" : sql = "SELECT scindexbarcode  as indexbarcode FROM m1_subclass      WHERE sckode  = '" & FixQuotes(scl) & "'"
                        Case "clr" : sql = "SELECT cindexbarcode   as indexbarcode FROM m1_color         WHERE ckode   = '" & FixQuotes(clr) & "'"
                        Case "dsg" : sql = "SELECT dindexbarcode   as indexbarcode FROM m1_designer      WHERE dkode   = '" & FixQuotes(dsg) & "'"
                        Case "mdl" : sql = "SELECT mindexbarcode   as indexbarcode FROM m1_model         WHERE mkode   = '" & FixQuotes(mdl) & "'"
                        Case "mrk" : sql = "SELECT mindexbarcode   as indexbarcode FROM m1_merk          WHERE mkode   = '" & FixQuotes(mrk) & "'"
                        Case "mtr" : sql = "SELECT mindexbarcode   as indexbarcode FROM m1_material      WHERE mkode   = '" & FixQuotes(mtr) & "'"
                        Case "oem" : sql = "SELECT oindexbarcode   as indexbarcode FROM m1_oem           WHERE okode   = '" & FixQuotes(oem) & "'"
                        Case "sct" : sql = "SELECT sindexbarcode   as indexbarcode FROM m1_section       WHERE skode   = '" & FixQuotes(sct) & "'"
                        Case "sze" : sql = "SELECT sindexbarcode   as indexbarcode FROM m1_size          WHERE skode   = '" & FixQuotes(sze) & "'"
                        Case "unt" : sql = "SELECT uindexbarcode   as indexbarcode FROM m1_unit          WHERE ukode   = '" & FixQuotes(unt) & "'"
                        Case "vdr" : sql = "SELECT vindexbarcode   as indexbarcode FROM m1_vendor        WHERE vkode   = '" & FixQuotes(vdr) & "'"
                        Case Else : sql = "SELECT '" & FixQuotes(idxSplit(i)) & "' as indexbarcode"
                    End Select

                    'AMBIL KODE INDEX BARCODE DARI MASING2 MASTER
                    If Len(sql) > 0 Then
                        dt = AsDataTableAmbilDariDB(sql)
                    End If
                    'TAMBAHKAN KODE INDEX UNTUK BARCODE
                    If dt.Rows.Count > 0 Then
                        nobarcode &= dt.Rows(0)(0)
                    End If
                Next

            End If

            'AMBIL NO URUT BARCODE
            Dim nourut As Double = 1
            sql = "SELECT noberikutnya FROM m0_barcode_next WHERE awalan = '" & FixQuotes(nobarcode) & "'"
            dt = AsDataTableAmbilDariDB(sql)
            If dt.Rows.Count > 0 Then
                nourut = Double.Parse(FxDB(dt.Rows(0)(0), 1))
            End If

            'AMBIL JMLDIGIT NO URUT BARCODE
            Dim jmldigit As Double = 0
            sql = "SELECT snilai FROM m0_setting WHERE smodule = 0 AND sgrup = 'options' AND skode = 'JmlDigitBarcode'"
            dt = AsDataTableAmbilDariDB(sql)
            If dt.Rows.Count > 0 Then
                If IsNumeric(FxDB(dt.Rows(0)(0), 0)) Then
                    jmldigit = Double.Parse(FxDB(dt.Rows(0)(0), 0))
                Else
                    result(2) = "Generate failed : Setting JmlDigitBarcode required numeric."
                End If
            Else
                result(2) = "Generate failed : Setting JmlDigitBarcode not found."
            End If

            'SET DIGIT NO URUT BARCODE
            Dim digit As String = nourut.ToString
            For i As Integer = digit.Length + 1 To jmldigit
                digit = "0" & digit
            Next
            nobarcode = String.Concat(nobarcode, digit)

            result(1) = 1
            result(2) = nobarcode
            result(3) = 0
            result(4) = result(4)

        Catch ex As Exception

            result(1) = 0
            result(2) = "Generate failed : " & ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try
        'END OF GENERATE BARCODE =======================================================

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

End Class