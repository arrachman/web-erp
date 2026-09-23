Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m0_service
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M0_Service_ReportSearch(ByVal param As String) As String
        'M0_Service_ReportSearch --------------------------------------------------------
        'rfilename, rsql, rfrom, rfilter, rorderby, rgroupby, rquery, rparam1, rparam2, rparam3, rparam4, rparam5

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

        ''VALIDASI WEBSITEACCESSKEY =========================================================
        'If Len(paramSplit(0)) = 0 Then
        '    result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        'End If

        ''Cek apakah WebsiteAccessKey valid
        'Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
        '    result(2) = "Access denied for insert/update data"
        'End If
        ''END OF VALIDASI WEBSITEACCESSKEY ==================================================

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

        dt = AmbilData("aplikasi1-M0_Report", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1) ' Ambil data ke databases
        pg1 = pg1
        'rfilename, rsql, rfrom, rfilter, rorderby, rgroupby, rquery, rparam1, rparam2, rparam3, rparam4, rparam5
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rfilename"), ""), sptField,
                     FxDB(dr("rsql"), ""), sptField,
                     FxDB(dr("rfrom"), ""), sptField,
                     FxDB(dr("rfilter"), ""), sptField,
                     FxDB(dr("rorderby"), ""), sptField,
                     FxDB(dr("rgroupby"), ""), sptField,
                     FxDB(dr("rquery"), 0), sptField,
                     FxDB(dr("rparam1"), ""), sptField,
                     FxDB(dr("rparam2"), ""), sptField,
                     FxDB(dr("rparam3"), ""), sptField,
                     FxDB(dr("rparam4"), ""), sptField,
                     FxDB(dr("rparam5"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Report data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rfilename, rsql, rfrom, rfilter, rorderby, rgroupby, rquery, rparam1, rparam2, rparam3, rparam4, rparam5"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M0_Service_ReportGetSetting(ByVal param As String) As String
        'M0_Service_ReportGetSetting --------------------------------------------------------
        'snilai

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

        ''VALIDASI WEBSITEACCESSKEY =========================================================
        'If Len(paramSplit(0)) = 0 Then
        '    result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        'End If

        ''Cek apakah WebsiteAccessKey valid
        'Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
        '    result(2) = "Access denied for insert/update data"
        'End If
        ''END OF VALIDASI WEBSITEACCESSKEY ==================================================

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

        dt = AmbilData("aplikasi1-M0_Setting", "smodule = '0' AND sgrup = 'report' AND skode = 'Agentreport'", Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("snilai"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Setting Report Agent data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("snilai"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M0_Service_Report(ByVal param As String) As String
        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = "", setting As String = ""

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

        ''VALIDASI WEBSITEACCESSKEY =========================================================
        'If Len(paramSplit(0)) = 0 Then
        '    result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        'End If

        ''Cek apakah WebsiteAccessKey valid
        'Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
        '    result(2) = "Access denied for insert/update data"
        'End If
        ''END OF VALIDASI WEBSITEACCESSKEY ==================================================

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

        'AMBIL SETTING AGENT PRINT
        dt = AmbilData("aplikasi1-M0_Setting", "(smodule = '0' AND sgrup = 'report' AND skode = 'Agentreport') or (smodule = '0' AND sgrup = 'company' AND skode = 'NamaPerusahaan')", "smodule, sgrup, skode", True, , , 0, 0, pg1) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 1 Then
            For Each dr As DataRow In dt.Rows
                setting = String.Concat(setting,
                     FxDB(dr("snilai"), ""), sptRow)
            Next
            setting = setting.Substring(0, setting.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Setting Company Name or Report Agent data not found." : GoTo selesai
        End If

        'AMBIL M0_REPORT
        dt = AmbilData("aplikasi1-M0_Report", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rmoduleid"), 0), sptField,
                     FxDB(dr("rmenuid"), 0), sptField,
                     FxDB(dr("ritem"), 0), sptField,
                     FxDB(dr("rfilename"), ""), sptField,
                     FxDB(dr("rsql"), ""), sptField,
                     FxDB(dr("rfrom"), ""), sptField,
                     FxDB(dr("rfilter"), ""), sptField,
                     FxDB(dr("rorderby"), ""), sptField,
                     FxDB(dr("rgroupby"), ""), sptField,
                     FxDB(dr("rquery"), 0), sptField,
                     FxDB(dr("rparam1"), ""), sptField,
                     FxDB(dr("rparam2"), ""), sptField,
                     FxDB(dr("rparam3"), ""), sptField,
                     FxDB(dr("rparam4"), ""), sptField,
                     FxDB(dr("rparam5"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Report data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, setting, sptSubParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("snilai"), sptSubParam, ReplaceMapping("rmoduleid, rmenuid, ritem, rfilename, rsql, rfrom, rfilter, rorderby, rgroupby, rquery, rparam1, rparam2, rparam3, rparam4, rparam5"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M0_Service_UpdateRelationTable(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", formatTgl As String = "", formatTglWaktu As String = ""

        Dim pg1 As New RsPaging
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = ""

        'SET DEFAULT RESULT
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

        'VALIDASI PARAMETER GLOBAL ===================================================
        'SPILIT PARAM
        paramSplit = param.Split(sptParam)

        'CEK ARRAY PARAM
        If (paramSplit.Length <> 6) Then
            result(2) = "Invalid parameter." : GoTo selesai
        End If
        'END OF VALIDASI PARAMETER GLOBAL ============================================

        ''VALIDASI WEBSITEACCESSKEY ===================================================
        'If Len(paramSplit(0)) = 0 Then
        '    result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        'End If

        ''Cek apakah WebsiteAccessKey valid
        'Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
        '    result(2) = "Access denied for insert/update data"
        'End If
        ''END OF VALIDASI WEBSITEACCESSKEY ============================================


        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 6) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'trsid(0) As Integer, trstable(1) As String, trskey(2) As String, trsvalue(3) As String, trsstatus(4) As Integer, 
        'trserrstep(5) As Integer

        'MAPPING BUAT FLEX --------------------------------------------------------
        'trsid, trstable, trskey, trsvalue, trsstatus, trserrstep

        'DEKLARASI VARIABEL
        Dim trsid As Integer = 0, trstable As String = "", trskey As String = "", trsvalue As String = "", trsstatus As Integer = 0, trserrstep As Integer = 0

        'VALIDASI TIPE DATA ==========================================================
        'trsid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "trsid required numeric." : GoTo selesai
        Else
            trsid = dataUtama(0)
        End If
        'trsstatus(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "trsstatus required numeric." : GoTo selesai
        Else
            trsstatus = dataUtama(4)
        End If
        'trserrstep(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "trserrstep required numeric." : GoTo selesai
        Else
            trserrstep = dataUtama(5)
        End If
        'END OF VALIDASI TIPE DATA ===================================================

        'VALIDASI DATA ===============================================================
        'trstable(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "trstable can't be empty" : GoTo selesai
        ElseIf Len(dataUtama(1)) > 250 Then
            result(2) = "trstable should not be more than 250 character." : GoTo selesai
        Else
            trstable = dataUtama(1)
        End If

        'trskey(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "trskey can't be empty" : GoTo selesai
        ElseIf Len(dataUtama(1)) > 250 Then
            result(2) = "trskey should not be more than 250 character." : GoTo selesai
        Else
            trskey = dataUtama(2)
        End If

        'trsvalue(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "trsvalue can't be empty" : GoTo selesai
        ElseIf Len(dataUtama(3)) > 250 Then
            result(2) = "trskey should not be more than 250 character." : GoTo selesai
        Else
            trsvalue = dataUtama(3)
        End If
        'END OF VALIDASI DATA ========================================================

        'SIMPAN KE DATABASE ==========================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'primary key master data     , value primary key master data, filter master data
        Dim pkMasterField() As String, pkMasterValue() As String, ftMaster As String = ""
        'datatable master data       , datatable tabel terkait
        Dim dtMaster As New DataTable, dtUpdate As New DataTable
        'pk terkait blm displit      , pk terkait sdh split, filter tabel terkait   , filter tabel terkait tergabung
        Dim pkUpdateSplit() As String, pkUpdate() As String, ftUpdate As String = "", ftUpdateFix As String = ""
        'mapping master blm displit    , mapping master sdh split, mapping terkait blm displit, mapping terkait sdh split
        Dim mappMasterSplit() As String, mappMaster() As String, mappUpdateSplit() As String, mappUpdate() As String
        'query update terkait
        Dim queryUpdate As String = ""

        'AMBIL FIELD DAN NILAI PRIMARY KEY MASTER DATA
        pkMasterField = trskey.Split(spt2) 'primary key diambil dari trskey, di split dengan "~"
        pkMasterValue = trsvalue.Split(spt2) ' value primary key diambil dari trskey, di split dengan "~"
        If pkMasterField.Length < 1 Then result(2) = "invalid trskey." : GoTo selesai 'jika primary key kosong maka error

        If pkMasterField.Length <> pkMasterValue.Length Then 'jika jumlah primary key dan nilai primary key tidak sama maka error
            result(2) = "invalid trskey and trsvalue." : GoTo selesai
        End If

        'BUAT FILTER UNTUK AMBIL DATA SESUAI FIELD DAN NILAI PRIMARY KEY MASTER DATA
        For i As Integer = 1 To pkMasterField.Length
            ftMaster = IIf(Len(ftMaster.ToString) = 0, "", ftMaster & " AND ")
            ftMaster = String.Concat(ftMaster, "(" & FixQuotes(pkMasterField(i - 1)) & " = '" & FixQuotes(pkMasterValue(i - 1)) & "')")
        Next

        'AMBIL MASTER DATA KE DATABASE SESUAI FILTER YG TELAH DIBUAT
        sql = "SELECT * FROM " & FixQuotes(trstable) & " WHERE " & ftMaster
        dtMaster = AsDataTableAmbilDariDB(sql)

        If dtMaster.Rows.Count > 0 Then ' jika data master datanya ada maka update tabel terkait
            Dim drMaster As DataRow = dtMaster.Rows(0)

            'AMBIL DAFTAR TABEL TERKAIT, limit berdasarkan errstep tabel terkait
            sql = "SELECT * FROM m0_table_relation WHERE derivedtable = '" & FixQuotes(trstable) & "' ORDER BY destinationtable LIMIT " & FixDouble(trserrstep) & ", 18446744073709551615"
            dtUpdate = AsDataTableAmbilDariDB(sql)
            'mapping : derivedtable, destinationtable, derivedtablekey, destinationtablekey, derivedtablemapping, destinationtablemapping

            If dtUpdate.Rows.Count > 0 Then
                For Each dr1 As DataRow In dtUpdate.Rows
                    'BUAT FILTER UPDATE TABEL TERKAIT ===========================================
                    'AMBIL PRIMARY KEY
                    pkMasterField = dr1("derivedtablekey").ToString.Split(spt2)
                    'If pkMasterField.Length < 1 Then result(2) = trserrstep & ". " & FixQuotes(dr1("destinationtable")) & " - " & "invalid derivedtablekey from " & dr1("derivedtable") & " for " & dr1("destinationtable") : GoTo selesai 'jika primary key kosong maka error

                    'AMBIL PK TABEL TERKAIT BLM SPLIT
                    pkUpdateSplit = dr1("destinationtablekey").ToString.Split(spt1)
                    'If pkUpdateSplit.Length < 1 Then result(2) = trserrstep & ". " & FixQuotes(dr1("destinationtable")) & " - " & "#1. invalid destinationtablekey from " & dr1("derivedtable") & " for " & dr1("destinationtable") : GoTo selesai 'jika primary key terkait kosong maka error

                    'VARIABEL FILTER UNTUK CASE UPDATE TABEL TERKAIT
                    Dim caseUpdate(pkUpdateSplit.Length) As String

                    'SPLIT PK TABEL TERKAIT
                    ftUpdateFix = ""
                    For i As Integer = 1 To pkUpdateSplit.Length
                        pkUpdate = pkUpdateSplit(i - 1).Split(spt2)
                        If pkMasterField.Length <> pkUpdate.Length Then result(2) = trserrstep & ". " & FixQuotes(dr1("destinationtable")) & " - " & "#2. invalid destinationtablekey from " & dr1("derivedtable") & " for " & dr1("destinationtable") : GoTo selesai 'jika PK terkait tidak sesuai dgn PK master maka error

                        'BUAT FILTER UPDATE TABEL TERKAIT
                        ftUpdate = ""
                        For j As Integer = 1 To pkUpdate.Length
                            ftUpdate = IIf(Len(ftUpdate.ToString) = 0, "", ftUpdate & " AND ")
                            ftUpdate = String.Concat(ftUpdate, FixQuotes(pkUpdate(j - 1)) & " = '" & drMaster(FixQuotes(pkMasterField(j - 1))) & "'")
                        Next

                        'GABUNG FILTER UPDATE TABEL TERKAIT
                        ftUpdateFix = IIf(Len(ftUpdateFix.ToString) = 0, "(" & ftUpdate & ")", ftUpdateFix & " OR (" & ftUpdate & ")")

                        'TAMBAHKAN FILTER UNTUK CASE UPDATE TABEL TERKAIT
                        caseUpdate(i - 1) = ftUpdate
                    Next
                    'END OF BUAT FILTER UPDATE TABEL TERKAIT ====================================


                    'BUAT QUERY UPDATE TABEL TERKAIT ============================================
                    'AMBIL MAPPING MASTER DATA
                    mappMasterSplit = dr1("derivedtablemapping").ToString.Split(spt1)
                    'If mappMasterSplit.Length < 1 Then result(2) = trserrstep & ". " & FixQuotes(dr1("destinationtable")) & " - " & "#1. invalid derivedtablemapping from " & dr1("derivedtable") & " for " & dr1("destinationtable") : GoTo selesai 'jika mapping master data kosong maka error

                    'AMBIL MAPPING TABEL TERKAIT
                    mappUpdateSplit = dr1("destinationtablemapping").ToString.Split(spt1)
                    'If mappUpdateSplit.Length < 1 Then result(2) = trserrstep & ". " & FixQuotes(dr1("destinationtable")) & " - " & "#1. invalid destinationtablemapping from " & dr1("derivedtable") & " for " & dr1("destinationtable") : GoTo selesai 'jika mapping tabel terkait kosong maka error

                    'SPLIT MAPPING
                    queryUpdate = ""
                    For i As Integer = 1 To mappUpdateSplit.Length
                        'SPLIT MAPPING MASTER DATA
                        mappMaster = mappMasterSplit(i - 1).Split(spt2)
                        'If mappMaster.Length < 1 Then result(2) = trserrstep & ". " & FixQuotes(dr1("destinationtable")) & " - " & "#2. invalid derivedtablemapping from " & dr1("derivedtable") & " for " & dr1("destinationtable") : GoTo selesai 'jika mapping master data kosong maka error

                        'SPLIT MAPPING TABEL TERKAIT
                        mappUpdate = mappUpdateSplit(i - 1).Split(spt2)
                        'If mappUpdate.Length < 1 Then result(2) = trserrstep & ". " & FixQuotes(dr1("destinationtable")) & " - " & "#2. invalid destinationtablemapping from " & dr1("derivedtable") & " for " & dr1("destinationtable") : GoTo selesai 'jika mapping tabel terkait kosong maka error

                        'BANDINGKAN JML MAPPING MASTER DATA DAN TABEL TERKAIT
                        If mappMaster.Length <> mappUpdate.Length Then result(2) = trserrstep & ". " & FixQuotes(dr1("destinationtable")) & " - " & "#2. invalid destinationtablemapping from " & dr1("derivedtable") & " for " & dr1("destinationtable") : GoTo selesai 'jika mapping terkait tidak sesuai dgn mapping master maka error

                        'BUAT CASE UPDATE TABEL TERKAIT
                        For j As Integer = 1 To mappUpdate.Length
                            queryUpdate = IIf(Len(queryUpdate.ToString) = 0, "", queryUpdate & ", ")
                            queryUpdate = String.Concat(queryUpdate, FixQuotes(mappUpdate(j - 1)) & " = " & "(CASE WHEN " & caseUpdate(i - 1) & " THEN '" & drMaster(FixQuotes(mappMaster(j - 1))) & "' ELSE " & FixQuotes(mappUpdate(j - 1)) & " END)")
                        Next
                    Next
                    'END OF BUAT QUERY UPDATE TABEL TERKAIT =====================================


                    'UPDATE KE DATABASE =========================================================
                    '*** Start Transaction ***'  
                    Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

                    Try
                        'UPDATE TABEL TERKAIT
                        sql = "UPDATE " & FixQuotes(dr1("destinationtable")) & " SET " & queryUpdate & " WHERE " & ftUpdateFix
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
                        result(2) = ""
                        result(3) = 0
                        result(4) = result(4)

                    Catch ex As Exception

                        Trans.Rollback() '*** RollBack Transaction ***'  
                        result(1) = 0
                        result(2) = "Transaction Rollback : " & trserrstep & ". " & FixQuotes(dr1("destinationtable")) & " - " & ex.Message & " - " & sql : GoTo selesai
                        result(3) = 0
                        result(4) = trserrstep

                    End Try

                    objCmd = Nothing
                    'END OF UPDATE KE DATABASE ==================================================


                    'INCREAMENT ERRSTEP =========================================================
                    trserrstep = FixDouble(trserrstep) + 1
                    'END OF INCREAMENT ERRSTEP ==================================================
                Next
            End If

        Else
            result(2) = "Data not found FROM " & trstable & " WHERE " & ftMaster : GoTo selesai
        End If
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
