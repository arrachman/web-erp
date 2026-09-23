Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")>
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Public Class m0_menu_s
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M0_Menu_SerenitySearch(ByVal param As String) As String
        'M0_MenuSearch ---------------------------------
        'mnmoduleid, mnid, mnname, mnurl, mnparent, mntype, mnlevel, 
        'mnurutan, mnactive, mnidtransaksi, mname

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

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
            If (Filter.Contains("INA")) Then
                sql = "SELECT CONCAT(m.mnmoduleid,m.mnid) As id, m.mnmoduleid, m.mnid, m.mnname As mnsubjek, m.mnname, m.mnurl, m.mnparent, m.mntype, m.mnlevel, m.mnurutan, m.mnactive, m.mnicon, m.mnidtransaksi, ms.rmakses FROM m0_user u JOIN m0_user_role_s ur ON ur.userid = u.userid JOIN m0_role_menu_s ms ON ms.rmrole = ur.role JOIN m0_menu_s m ON m.mnmoduleid = ms.rmmoduleid AND m.mnid = ms.rmmenuid"
                Filter = "mnactive = 1 AND " + Sorting
            Else
                sql = "SELECT CONCAT(m.mnmoduleid,m.mnid) As id, m.mnmoduleid, m.mnid, m.mnname As mnsubjek, IFNULL(l.mnltranslate, m.mnname) AS mnname, m.mnurl, m.mnparent, m.mntype, m.mnlevel, m.mnurutan, m.mnactive, m.mnicon, m.mnidtransaksi, ms.rmakses FROM m0_user u JOIN m0_user_role_s ur ON ur.userid = u.userid JOIN m0_role_menu_s ms ON ms.rmrole = ur.role JOIN m0_menu_s m ON m.mnmoduleid = ms.rmmoduleid AND m.mnid = ms.rmmenuid LEFT JOIN m0_menu_s_lang l ON l.mnlmnid = m.mnid AND l.mnlmoduleid = m.mnmoduleid AND l.mnllanguage = '" + Filter + "'"
                Filter = "mnactive = 1 AND " + Sorting
            End If
        Else
            sql = "SELECT CONCAT(m.mnmoduleid,m.mnid) As id, m.mnmoduleid, m.mnid, m.mnname As mnsubjek, m.mnname, m.mnurl, m.mnparent, m.mntype, m.mnlevel, m.mnurutan, m.mnactive, m.mnicon, m.mnidtransaksi, ms.rmakses FROM m0_user u JOIN m0_user_role_s ur ON ur.userid = u.userid JOIN m0_role_menu_s ms ON ms.rmrole = ur.role JOIN m0_menu_s m ON m.mnmoduleid = ms.rmmoduleid AND m.mnid = ms.rmmenuid"
            Filter = "mnactive = 1 AND " + Sorting
        End If

        Sorting = "mnparent, mnurutan, mnid"

        'result(2) = "Menu data not found. " + sql + "where " + Filter : GoTo selesai

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-m0_menu_s", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "id", sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                Dim namamenu As String = FxDB(dr("mnname"), "")
                If (namamenu.Length = 0) Then
                    namamenu = FxDB(dr("mnsubjek"), "")
                End If
                search = String.Concat(search,
                     FxDB(dr("mnmoduleid"), 0), sptField,
                     FxDB(dr("mnid"), 0), sptField,
                     namamenu, sptField,
                     FxDB(dr("mnurl"), ""), sptField,
                     FxDB(dr("mnparent"), ""), sptField,
                     FxDB(dr("mntype"), 0), sptField,
                     FxDB(dr("mnlevel"), 0), sptField,
                     FxDB(dr("mnurutan"), 0), sptField,
                     FxDB(dr("mnactive"), 0), sptField,
                     FxDB(dr("mnicon"), ""), sptField,
                     FxDB(dr("mnidtransaksi"), 0), sptField,
                     FxDB(dr("mnsubjek"), ""), sptField,
                     FxDB(dr("rmakses"), "0000000000000"), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Menu data not found. " + sql + "where " + Filter
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("mnmoduleid, mnid, mnname, mnurl, mnparent, mntype, mnlevel, mnurutan, mnactive, mnicon, mnidtransaksi, mnsubjek, mnakses"))

        Return wsResult
    End Function


    <WebMethod()>
    Public Function M0_Menu_SRoleSearch(ByVal param As String) As String
        'M1_AreaSearch --------------------------------------------------------
        'akode, anama, acatatan, aaktif, ainputuser, ainputtgl, amodifikasiuser, 
        'amodifikasitgl

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
        Else
            'result(2) = "Filter required" : GoTo selesai
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT CONCAT(mnmoduleid,mnid) AS idutama, REPLACE(mnparent,'-','') AS idparent, mnname AS kode, mnname AS nama, 0 AS akses  FROM m0_menu_s"

        dt = AmbilData("aplikasi1-m0_menu_s", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idutama"), 0), sptField,
                     FxDB(dr("idparent"), 0), sptField,
                     FxDB(dr("kode"), ""), sptField,
                     FxDB(dr("nama"), ""), sptField,
                     FxDB(dr("akses"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "data not found. "
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idutama, idparent, kode, nama, akses"))

        Return wsResult
    End Function


End Class

