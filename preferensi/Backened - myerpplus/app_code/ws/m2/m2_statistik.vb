Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m2_statistik
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M2_Statistik_KasBankSearch(ByVal param As String) As String

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

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_statistik_kasbank_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_StatistikKasBank", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "co.cnomor", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("cid"), 0), sptField,
                     FxDB(dr("cnomor"), ""), sptField,
                     FxDB(dr("cnama"), ""), sptField,
                     FxDB(dr("cmatauang"), ""), sptField,
                     FxDB(dr("csaldo"), ""), sptField,
                     FxDB(dr("csaldovalas"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("cid, cnomor, cnama, cmatauang, csaldo, csaldovalas"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_Statistik_GiroSearch(ByVal param As String) As String
        'M2_SgSearch --------------------------------------------------------
        'sgid, sgcabang, sglokasi, sgsumber, sgautonotransaksi, sgnotransaksi, sgtgl, 
        'sgkodepa, sgkontak, sgkontakperson, sguraian, sgcatatan, sgmatauang, sgkurs, 
        'sgjumlah, sgjumlahvalas, sgstatussgc, sgstatus, sgstatussebelumnya, sgjmlrevisi, sgcetakanke, 
        'sgisclose, sginputuser, sginputtgl, sgmodifikasiuser, sgmodifikasitgl, sgposting, sgpostingtgl, 
        'sgcabangnama, sglokasinama, sgkontakkode, sgkontaknama, sgstatusnama, sgstatussebelumnyanama, sginputusernama, 
        'sgmodifikasiusernama

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

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_statistik_giro_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_StatistikGiro", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("gltgljthtempo"), ""), sptField,
                     FxDB(dr("glnamakontak"), ""), sptField,
                     FxDB(dr("glmatauang"), ""), sptField,
                     FxDB(dr("gljumlah"), 0), sptField,
                     FxDB(dr("gljenis"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("gltgljthtempo, glnamakontak, glmatauang, gljumlah, gljenis"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2S_CashBank(ByVal param As String) As String
        'M2S_CashBank Utama --------------------------------------------------------
        'cid, cnomor, cnama, cmatauang, csaldo, csaldovalas

        'M2S_CashBank Detail -------------------------------------------------------
        'cid, cnomor, cnama, cmatauang, csaldo, csaldovalas

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = "", search2 As String = ""

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

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2s_cashbank")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Cr", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "co.cnomor", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("cid"), ""), sptField,
                     FxDB(dr("cnomor"), ""), sptField,
                     FxDB(dr("cnama"), ""), sptField,
                     FxDB(dr("cmatauang"), ""), sptField,
                     FxDB(dr("csaldo"), 0), sptField,
                     FxDB(dr("csaldovalas"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
            'Else
            '    result(2) = "Transaction data not found. #1"
        End If

        'PANGGIL QUERY
        sql = query.PanggilQuery("m2s_cashbank")

        dt = AmbilData("aplikasi1-M2_Cr", Filter, "csaldo DESC", True, , , 1, 5, pg1, , , "co.cnomor", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search2 = String.Concat(search2,
                     FxDB(dr("cid"), ""), sptField,
                     FxDB(dr("cnomor"), ""), sptField,
                     FxDB(dr("cnama"), ""), sptField,
                     FxDB(dr("cmatauang"), ""), sptField,
                     FxDB(dr("csaldo"), 0), sptField,
                     FxDB(dr("csaldovalas"), 0), sptRow)
            Next
            search2 = search2.Substring(0, search2.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
            'Else
            '    result(2) = "Transaction data not found. #2"
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search, sptSubParam, search2)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("cid, cnomor, cnama, cmatauang, csaldo, csaldovalas" & sptSubParam & "cid, cnomor, cnama, cmatauang, csaldo, csaldovalas"))
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2S_Giro(ByVal param As String) As String
        'M2S_Giro Grid Masuk --------------------------------------------------------
        'glnogiro, glnotransaksi, glkontak, gljenis, glbank, glnoacbank, glmatauang, 
        'glkurs, gljumlah, gljumlahvalas, gltgljthtempo, glstatus, glkontakkode, glkontaknama, 
        'glbanknama, glumur, glumurklasifikasi

        'M2S_Giro Grid Keluar -------------------------------------------------------
        'glnogiro, glnotransaksi, glkontak, gljenis, glbank, glnoacbank, glmatauang, 
        'glkurs, gljumlah, gljumlahvalas, gltgljthtempo, glstatus, glkontakkode, glkontaknama, 
        'glbanknama, glumur, glumurklasifikasi

        'M2S_Giro Grafik Masuk -------------------------------------------------------
        'glnogiro, glnotransaksi, glkontak, gljenis, glbank, glnoacbank, glmatauang, 
        'glkurs, gljumlah, gljumlahvalas, gltgljthtempo, glstatus, glkontakkode, glkontaknama, 
        'glbanknama, glumur, glumurklasifikasi

        'M2S_Giro Grafik Keluar -------------------------------------------------------
        'glnogiro, glnotransaksi, glkontak, gljenis, glbank, glnoacbank, glmatauang, 
        'glkurs, gljumlah, gljumlahvalas, gltgljthtempo, glstatus, glkontakkode, glkontaknama, 
        'glbanknama, glumur, glumurklasifikasi

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = ""
        Dim search As String = "", search2 As String = "", search3 As String = "", search4 As String = ""

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

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2s_giro")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Cr", IIf(Len(Filter) > 0, "(" & Filter & ") AND gljenis = 0 AND glstatus = 0", "gljenis = 0 AND glstatus = 0"), Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "glnogiro", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("glnogiro"), ""), sptField,
                     FxDB(dr("glnotransaksi"), ""), sptField,
                     FxDB(dr("glkontak"), ""), sptField,
                     FxDB(dr("gljenis"), 0), sptField,
                     FxDB(dr("glbank"), ""), sptField,
                     FxDB(dr("glnoacbank"), ""), sptField,
                     FxDB(dr("glmatauang"), ""), sptField,
                     FxDB(dr("glkurs"), 0), sptField,
                     FxDB(dr("gljumlah"), 0), sptField,
                     FxDB(dr("gljumlahvalas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("gltgljthtempo"), ""), formatTgl), sptField,
                     FxDB(dr("glstatus"), 0), sptField,
                     FxDB(dr("glkontakkode"), ""), sptField,
                     FxDB(dr("glkontaknama"), ""), sptField,
                     FxDB(dr("glbanknama"), ""), sptField,
                     FxDB(dr("glumur"), 0), sptField,
                     FxDB(dr("glumurklasifikasi"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
            'Else
            '    result(2) = "Transaction data not found. #1"
        End If

        'PANGGIL QUERY
        sql = query.PanggilQuery("m2s_giro")

        dt = AmbilData("aplikasi1-M2_Cr", IIf(Len(Filter) > 0, "(" & Filter & ") AND gljenis = 1 AND glstatus = 0", "gljenis = 1 AND glstatus = 0"), Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "glnogiro", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search2 = String.Concat(search2,
                     FxDB(dr("glnogiro"), ""), sptField,
                     FxDB(dr("glnotransaksi"), ""), sptField,
                     FxDB(dr("glkontak"), ""), sptField,
                     FxDB(dr("gljenis"), 0), sptField,
                     FxDB(dr("glbank"), ""), sptField,
                     FxDB(dr("glnoacbank"), ""), sptField,
                     FxDB(dr("glmatauang"), ""), sptField,
                     FxDB(dr("glkurs"), 0), sptField,
                     FxDB(dr("gljumlah"), 0), sptField,
                     FxDB(dr("gljumlahvalas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("gltgljthtempo"), ""), formatTgl), sptField,
                     FxDB(dr("glstatus"), 0), sptField,
                     FxDB(dr("glkontakkode"), ""), sptField,
                     FxDB(dr("glkontaknama"), ""), sptField,
                     FxDB(dr("glbanknama"), ""), sptField,
                     FxDB(dr("glumur"), 0), sptField,
                     FxDB(dr("glumurklasifikasi"), 0), sptRow)
            Next
            search2 = search2.Substring(0, search2.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
            'Else
            '    result(2) = "Transaction data not found. #2"
        End If

        'PANGGIL QUERY
        sql = query.PanggilQuery("m2s_giro")

        dt = AmbilData("aplikasi1-M2_Cr", IIf(Len(Filter) > 0, "(" & Filter & ") AND gljenis = 0 AND glstatus = 0", "gljenis = 0 AND glstatus = 0"), Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "glumurklasifikasi", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search3 = String.Concat(search3,
                     FxDB(dr("glnogiro"), ""), sptField,
                     FxDB(dr("glnotransaksi"), ""), sptField,
                     FxDB(dr("glkontak"), ""), sptField,
                     FxDB(dr("gljenis"), 0), sptField,
                     FxDB(dr("glbank"), ""), sptField,
                     FxDB(dr("glnoacbank"), ""), sptField,
                     FxDB(dr("glmatauang"), ""), sptField,
                     FxDB(dr("glkurs"), 0), sptField,
                     FxDB(dr("gljumlah"), 0), sptField,
                     FxDB(dr("gljumlahvalas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("gltgljthtempo"), ""), formatTgl), sptField,
                     FxDB(dr("glstatus"), 0), sptField,
                     FxDB(dr("glkontakkode"), ""), sptField,
                     FxDB(dr("glkontaknama"), ""), sptField,
                     FxDB(dr("glbanknama"), ""), sptField,
                     FxDB(dr("glumur"), 0), sptField,
                     FxDB(dr("glumurklasifikasi"), 0), sptRow)
            Next
            search3 = search3.Substring(0, search3.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
            'Else
            '    result(2) = "Transaction data not found. #3"
        End If

        'PANGGIL QUERY
        sql = query.PanggilQuery("m2s_giro")

        dt = AmbilData("aplikasi1-M2_Cr", IIf(Len(Filter) > 0, "(" & Filter & ") AND gljenis = 1 AND glstatus = 0", "gljenis = 1 AND glstatus = 0"), Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "glumurklasifikasi", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search4 = String.Concat(search4,
                     FxDB(dr("glnogiro"), ""), sptField,
                     FxDB(dr("glnotransaksi"), ""), sptField,
                     FxDB(dr("glkontak"), ""), sptField,
                     FxDB(dr("gljenis"), 0), sptField,
                     FxDB(dr("glbank"), ""), sptField,
                     FxDB(dr("glnoacbank"), ""), sptField,
                     FxDB(dr("glmatauang"), ""), sptField,
                     FxDB(dr("glkurs"), 0), sptField,
                     FxDB(dr("gljumlah"), 0), sptField,
                     FxDB(dr("gljumlahvalas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("gltgljthtempo"), ""), formatTgl), sptField,
                     FxDB(dr("glstatus"), 0), sptField,
                     FxDB(dr("glkontakkode"), ""), sptField,
                     FxDB(dr("glkontaknama"), ""), sptField,
                     FxDB(dr("glbanknama"), ""), sptField,
                     FxDB(dr("glumur"), 0), sptField,
                     FxDB(dr("glumurklasifikasi"), 0), sptRow)
            Next
            search4 = search4.Substring(0, search4.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
            'Else
            '    result(2) = "Transaction data not found. #4"
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search, sptSubParam, search2, sptSubParam, search3, sptSubParam, search4)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("glnogiro, glnotransaksi, glkontak, gljenis, glbank, glnoacbank, glmatauang, glkurs, gljumlah, gljumlahvalas, gltgljthtempo, glstatus, glkontakkode, glkontaknama, glbanknama, glumur, glumurklasifikasi" & sptSubParam & "glnogiro, glnotransaksi, glkontak, gljenis, glbank, glnoacbank, glmatauang, glkurs, gljumlah, gljumlahvalas, gltgljthtempo, glstatus, glkontakkode, glkontaknama, glbanknama, glumur, glumurklasifikasi" & sptSubParam & "glnogiro, glnotransaksi, glkontak, gljenis, glbank, glnoacbank, glmatauang, glkurs, gljumlah, gljumlahvalas, gltgljthtempo, glstatus, glkontakkode, glkontaknama, glbanknama, glumur, glumurklasifikasi" & sptSubParam & "glnogiro, glnotransaksi, glkontak, gljenis, glbank, glnoacbank, glmatauang, glkurs, gljumlah, gljumlahvalas, gltgljthtempo, glstatus, glkontakkode, glkontaknama, glbanknama, glumur, glumurklasifikasi"))
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2S_HutangPiutang(ByVal param As String) As String
        'Hutang --------------------------------------------------------
        'tkontak, tkontakkode, tkontaknama, tmatauang, tsaldo

        'Piutang -------------------------------------------------------
        'tkontak, tkontakkode, tkontaknama, tmatauang, tsaldo

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = "", search2 As String = ""

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

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2s_hutang")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Cr", "t.tstatus IN(2, 3, 4, 7)", "tsaldo DESC", True, , , 1, 10, pg1, , , "t.tkontak", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("tkontak"), ""), sptField,
                     FxDB(dr("tkontakkode"), ""), sptField,
                     FxDB(dr("tkontaknama"), ""), sptField,
                     FxDB(dr("tmatauang"), ""), sptField,
                     FxDB(dr("tsaldo"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
            'Else
            '    result(2) = "Transaction data not found. #1"
        End If

        'PANGGIL QUERY
        sql = query.PanggilQuery("m2s_piutang")

        dt = AmbilData("aplikasi1-M2_Cr", "t.tstatus IN(2, 3, 4, 7)", "tsaldo DESC", True, , , 1, 10, pg1, , , "t.tkontak", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search2 = String.Concat(search2,
                     FxDB(dr("tkontak"), ""), sptField,
                     FxDB(dr("tkontakkode"), ""), sptField,
                     FxDB(dr("tkontaknama"), ""), sptField,
                     FxDB(dr("tmatauang"), ""), sptField,
                     FxDB(dr("tsaldo"), 0), sptRow)
            Next
            search2 = search2.Substring(0, search2.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
            'Else
            '    result(2) = "Transaction data not found. #2"
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search, sptSubParam, search2)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("tkontak, tkontakkode, tkontaknama, tmatauang, tsaldo" & sptSubParam & "tkontak, tkontakkode, tkontaknama, tmatauang, tsaldo"))
        Return wsResult
    End Function

End Class