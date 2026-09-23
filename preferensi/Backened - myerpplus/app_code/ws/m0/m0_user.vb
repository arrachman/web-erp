Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m0_user
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = "Nawi"     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M0_UserSimpan2(ByVal param As String) As String

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

        Dim Filter As String = "", Sorting As String = "", search As String = ""

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
        'userid(0) As Integer, ukode(1) As String, unama(2) As String, upassword(3) As String, ukontak(4) As Integer, 
        'ucabang(5) As String, ulokasi(6) As String, ugudang(7) As String, ukota(8) As String, ugambar(9) As String, 
        'uaktif(10) As Integer, utglexpired(11) As Date, ulevel(12) As Integer, ugrup(13) As Integer, ubahasa(14) As String, 
        'udefaultview(15) As Integer

        'MAPPING BUAT FLEX --------------------------------------------------------
        'userid, ukode, unama, upassword, ukontak, ucabang, ulokasi, 
        'ugudang, ukota, ugambar, uaktif, utglexpired, ulevel, ugrup, 
        'ubahasa, udefaultview

        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 16) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================

        'VALIDASI TIPE DATA ==========================================================
        'userid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If
        'ukontak(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "ukontak required numeric." : GoTo selesai
        End If
        'uaktif(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "uaktif required numeric." : GoTo selesai
        End If
        'utglexpired(11) As Date
        If (IsDate(dataUtama(11)) = False) Then
            result(2) = "utglexpired required date." : GoTo selesai
        End If
        'ulevel(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "ulevel required numeric." : GoTo selesai
        End If
        'ugrup(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "ugrup required numeric." : GoTo selesai
        End If
        'udefaultview(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "udefaultview required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA ===================================================

        'VALIDASI DATA ===============================================================
        'ukode(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "ukode can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "ukode should not be more than 25 character." : GoTo selesai
        End If

        'unama(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "unama can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 50 Then
            result(2) = "unama should not be more than 50 character." : GoTo selesai
        End If

        'upassword(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "upassword can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 250 Then
            result(2) = "upassword should not be more than 250 character." : GoTo selesai
        End If

        'ubahasa(14) As String
        If Len(dataUtama(14)) = 0 Then
            result(2) = "ubahasa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(14)) > 2 Then
            result(2) = "ubahasa should not be more than 2 character." : GoTo selesai
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
                dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(userid) FROM M0_User WHERE userid='" & dataUtama(0) & "'")
                rowUpdate = dtupdate.Rows(0)(0)

                If (rowUpdate > 0) Then
                    sql = "Update M0_User set ukode  = '" & FixQuotes(dataUtama(1)) & "', unama  = '" & FixQuotes(dataUtama(2)) & "', upassword  = '" & FixQuotes(dataUtama(3)) & "', ukontak  = " & dataUtama(4) & ", ucabang  = '" & FixQuotes(dataUtama(5)) & "', ulokasi  = '" & FixQuotes(dataUtama(6)) & "', ugudang  = '" & FixQuotes(dataUtama(7)) & "', ukota  = '" & FixQuotes(dataUtama(8)) & "', ugambar  = '" & FixQuotes(dataUtama(9)) & "', uaktif  = " & dataUtama(10) & ", utglexpired  = '" & FixQuotes(AsFormatTanggal(dataUtama(11))) & "', ulevel  = " & dataUtama(12) & ", ugrup  = " & dataUtama(13) & ", ubahasa  = '" & FixQuotes(dataUtama(14)) & "', udefaultview  = " & dataUtama(15) & " where userid = '" & dataUtama(0) & "'"
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
                sql = "Insert into M0_User (ukode, unama, upassword, ukontak, ucabang, ulokasi, ugudang, ukota, ugambar, uaktif, utglexpired, ulevel, ugrup, ubahasa, udefaultview) values('" & FixQuotes(dataUtama(1)) & "', '" & FixQuotes(dataUtama(2)) & "', '" & FixQuotes(dataUtama(3)) & "', " & dataUtama(4) & ", '" & FixQuotes(dataUtama(5)) & "', '" & FixQuotes(dataUtama(6)) & "', '" & FixQuotes(dataUtama(7)) & "', '" & FixQuotes(dataUtama(8)) & "', '" & FixQuotes(dataUtama(9)) & "', " & dataUtama(10) & ", '" & FixQuotes(AsFormatTanggal(dataUtama(11))) & "', " & dataUtama(12) & ", " & dataUtama(13) & ", '" & FixQuotes(dataUtama(14)) & "', " & dataUtama(15) & ")"
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
            Dim paramSearch As String = M0_User_VSearch(PostWsSearch(paramSplit(0), "M0_User_VSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
            Dim hasilSearch As New RsHasilWsSearch
            hasilSearch = GetWsSearch(paramSearch)

            result(1) = hasilSearch.success
            result(2) = hasilSearch.errmessage

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
    Public Function M0_UserDelete2(ByVal param As String) As String

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
        If (IsNumeric(paramSplit(5)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
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
            sql = "DELETE FROM M0_User WHERE userid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M0_User_VSearch(PostWsSearch(paramSplit(0), "M0_User_VSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M0_User_VSearch(ByVal param As String) As String
        'JIKA MENAMBAHKAN FIELD DISINI MAKA HARUS TAMBAHKAN JUGA LENGTH ARRAY PADA VALIDASI AMBIL USERID WS M0_LOGIN
        'M0_User_VSearch --------------------------------------------------------
        'userid, ukode, unama, upassword, ukontak, ukontakkode, ukontaknama, 
        'ucabang, ucabangnama, ulokasi, ulokasinama, ugudang, ugudangnama, ukota, 
        'ugambar, uaktif, utglexpired, ulevel, ugrup, ugrupnama, ubahasa, 
        'udefaultview, ubahasanama, ukotanama, kontakperson, kontakpersonalamat

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
            Filter = Filter.Replace("ukontakkode", "c.kkode")
            Filter = Filter.Replace("ukontaknama", "c.knama")
            Filter = Filter.Replace("ucabangnama", "b.bnama")
            Filter = Filter.Replace("ulokasinama", "l.lnama")
            Filter = Filter.Replace("ugudangnama", "w.wnama")
            Filter = Filter.Replace("ugrupnama", "ug.ugnama")
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m0_user_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M0_User_V", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("userid"), 0), sptField,
                     FxDB(dr("ukode"), ""), sptField,
                     FxDB(dr("unama"), ""), sptField,
                     "", sptField,
                     FxDB(dr("ukontak"), 0), sptField,
                     FxDB(dr("ukontakkode"), ""), sptField,
                     FxDB(dr("ukontaknama"), ""), sptField,
                     FxDB(dr("ucabang"), ""), sptField,
                     FxDB(dr("ucabangnama"), ""), sptField,
                     FxDB(dr("ulokasi"), ""), sptField,
                     FxDB(dr("ulokasinama"), ""), sptField,
                     FxDB(dr("ugudang"), ""), sptField,
                     FxDB(dr("ugudangnama"), ""), sptField,
                     FxDB(dr("ukota"), ""), sptField,
                     FxDB(dr("ugambar"), ""), sptField,
                     FxDB(dr("uaktif"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("utglexpired"), ""), formatTgl), sptField,
                     FxDB(dr("ulevel"), 0), sptField,
                     FxDB(dr("ugrup"), 0), sptField,
                     FxDB(dr("ugrupnama"), ""), sptField,
                     FxDB(dr("ubahasa"), ""), sptField,
                     FxDB(dr("udefaultview"), 0), sptField,
                     FxDB(dr("ubahasanama"), ""), sptField,
                     FxDB(dr("ukotanama"), ""), sptField,
                     FxDB(dr("kontakperson"), ""), sptField,
                     FxDB(dr("kontakpersonalamat"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "User data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("userid, ukode, unama, upassword, ukontak, ukontakkode, ukontaknama, ucabang, ucabangnama, ulokasi, ulokasinama, ugudang, ugudangnama, ukota, ugambar, uaktif, utglexpired, ulevel, ugrup, ugrupnama, ubahasa, udefaultview, ubahasanama, ukotanama, kontakperson, kontakpersonalamat"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M0_UserSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataCabang(), dataRowCabang(), dataLokasi(), dataRowLokasi(), dataGudang(), dataRowGudang(), dataCoa(), dataRowCoa() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = "", search As String = ""
        Dim dt As New DataTable

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

        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 6) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'userid(0) As Integer, ukode(1) As String, unama(2) As String, upassword(3) As String, ukontak(4) As Integer, 
        'ucabang(5) As String, ulokasi(6) As String, ugudang(7) As String, ukota(8) As String, ugambar(9) As String, 
        'uaktif(10) As Integer, utglexpired(11) As Date, ulevel(12) As Integer, ugrup(13) As Integer, ubahasa(14) As String, 
        'udefaultview(15) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'userid, ukode, unama, upassword, ukontak, ucabang, ulokasi, 
        'ugudang, ukota, ugambar, uaktif, utglexpired, ulevel, ugrup, 
        'ubahasa, udefaultview

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 16) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'userid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If
        'ukontak(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "ukontak required numeric." : GoTo selesai
        End If
        'uaktif(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "uaktif required numeric." : GoTo selesai
        End If
        'utglexpired(11) As Date
        If (IsDate(dataUtama(11)) = False) Then
            result(2) = "utglexpired required date." : GoTo selesai
        End If
        'ulevel(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "ulevel required numeric." : GoTo selesai
        End If
        'ugrup(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "ugrup required numeric." : GoTo selesai
        End If
        'udefaultview(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "udefaultview required numeric." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'ukode(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "ukode can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "ukode should not be more than 25 character." : GoTo selesai
        End If

        'unama(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "unama can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 100 Then
            result(2) = "unama should not be more than 100 character." : GoTo selesai
        End If

        'upassword(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "upassword can't be empty" : GoTo selesai
        End If
        If isUpdate = False Then
            If Len(dataUtama(3)) < 6 Then
                result(2) = "upassword should not be less than 6 character." : GoTo selesai
            End If
        End If
        If Len(dataUtama(3)) > 250 Then
            result(2) = "upassword should not be more than 250 character." : GoTo selesai
        End If
        'generate password
        dataUtama(3) = CreateSHAHash(dataUtama(3), "AlEuPj13")


        'ubahasa(14) As String
        If Len(dataUtama(14)) = 0 Then
            'result(2) = "ubahasa can't be empty" : GoTo selesai
            dataUtama(14) = "INA"
        End If
        If Len(dataUtama(14)) > 25 Then
            result(2) = "ubahasa should not be more than 25 character." : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "userid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ukode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "unama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "upassword", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ukontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ucabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ulokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ugudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ukota", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ugambar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "uaktif", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "utglexpired", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ulevel", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ugrup", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ubahasa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "udefaultview", AsEnumTypeData.AsInt64)
        If AsDataTableTambahData(dtutama, "userid~ukode~unama~upassword~ukontak~ucabang~ulokasi~ugudang~ukota~ugambar~uaktif~utglexpired~ulevel~ugrup~ubahasa~udefaultview", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'userid(0) As Integer, role(1) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'userid, role

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "userid", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "role", AsEnumTypeData.AsString)

        If (Len(dataSplit(1)) > 0) Then

            'VALIDASI DAN SET DATA DETAIL ======================================================
            'SPLIT PARAMETER DATA DETAIL
            dataDetail = dataSplit(1).Split(sptRow)
            'END OF VALIDASI DAN SET DATA DETAIL ===============================================

            'VALIDASI DAN SET DATA ROW DETAIL ==================================================
            Dim JmlDtDetail As Integer = dataDetail.Length
            For i = 1 To JmlDtDetail
                'SPLIT DATA DETAIL
                dataRowDetail = dataDetail(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
                'CEK ARRAY DATA DETAIL
                If (dataRowDetail.Length <> 2) Then
                    result(2) = "Role Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

                'VALIDASI TIPE DATA DETAIL ------------------------------------------
                'userid(0) As Integer
                If (IsNumeric(dataRowDetail(0)) = False) Then
                    result(2) = "Role Row : " & i & " - userid required numeric." : GoTo selesai
                End If
                'role(1) As Integer
                If (Len(dataRowDetail(1)) = 0) Then
                    result(2) = "Role Row : " & i & " - role can't be empty." : GoTo selesai
                End If
                If (Len(dataRowDetail(1)) > 25) Then
                    result(2) = "Role Row : " & i & " - role should not be more than 25 character." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

                'VALIDASI DATA DETAIL ---------------------------------------
                'END OF VALIDASI DATA DETAIL --------------------------------

                If AsDataTableTambahData(dtdetail, "userid~role", dataRowDetail(0) & "~" & dataRowDetail(1)) = False Then
                    result(2) = "Role Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================

        End If


        'MAPPING BUAT WS DATA CABANG -------------------------------------------------------
        'userid(0) As Integer, cabang(1) As String

        'MAPPING BUAT FLEX DATA CABANG -----------------------------------------------------
        'userid, cabang

        'Buat datatable cabang
        Dim dtcabang As New DataTable
        AsDataTableTambahField(dtcabang, "userid", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcabang, "cabang", AsEnumTypeData.AsString)

        ''Tambahkan cabang utama
        ''userid(0) As Integer, ucabang(5) As String
        'If AsDataTableTambahData(dtcabang, "userid~cabang", dataUtama(0) & "~" & dataUtama(5)) = False Then
        '    result(2) = "Main Branch : insert into datatable failed." : GoTo selesai
        'End If

        If (Len(dataSplit(2)) > 0) Then

            'VALIDASI DAN SET DATA CABANG ======================================================
            'SPLIT PARAMETER DATA CABANG
            dataCabang = dataSplit(2).Split(sptRow)
            'END OF VALIDASI DAN SET DATA CABANG ===============================================

            'VALIDASI DAN SET DATA ROW CABANG ==================================================
            Dim JmlDtCabang As Integer = dataCabang.Length
            For i = 1 To JmlDtCabang
                'SPLIT DATA CABANG
                dataRowCabang = dataCabang(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA CABANG -----------------------------------
                'CEK ARRAY DATA CABANG
                If (dataRowCabang.Length <> 2) Then
                    result(2) = "Branch Row : " & i & " - Invalid branch transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW CABANG ----------------------------

                'VALIDASI TIPE DATA CABANG ------------------------------------------
                'userid(0) As Integer
                If (IsNumeric(dataRowCabang(0)) = False) Then
                    result(2) = "Branch Row : " & i & " - userid required numeric." : GoTo selesai
                End If
                'cabang(1) As String
                If (Len(dataRowCabang(1)) = 0) Then
                    result(2) = "Branch Row : " & i & " - cabang can't be empty." : GoTo selesai
                End If
                If (Len(dataRowCabang(1)) > 25) Then
                    result(2) = "Branch Row : " & i & " - cabang should not be more than 25 character." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA CABANG -----------------------------------

                'VALIDASI DATA CABANG ---------------------------------------
                'END OF VALIDASI DATA CABANG --------------------------------

                If AsDataTableTambahData(dtcabang, "userid~cabang", dataRowCabang(0) & "~" & dataRowCabang(1)) = False Then
                    result(2) = "Branch Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA CABANG ===========================================

        End If


        'MAPPING BUAT WS DATA LOKASI -------------------------------------------------------
        'userid(0) As Integer, lokasi(1) As String

        'MAPPING BUAT FLEX DATA LOKASI -----------------------------------------------------
        'userid, lokasi

        'Buat datatable lokasi
        Dim dtlokasi As New DataTable
        AsDataTableTambahField(dtlokasi, "userid", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtlokasi, "lokasi", AsEnumTypeData.AsString)

        ''Tambahkan lokasi utama
        ''userid(0) As Integer, ulokasi(6) As String
        'If AsDataTableTambahData(dtlokasi, "userid~lokasi", dataUtama(0) & "~" & dataUtama(6)) = False Then
        '    result(2) = "Main Location : insert into datatable failed." : GoTo selesai
        'End If

        If (Len(dataSplit(3)) > 0) Then

            'VALIDASI DAN SET DATA LOKASI ======================================================
            'SPLIT PARAMETER DATA LOKASI
            dataLokasi = dataSplit(3).Split(sptRow)
            'END OF VALIDASI DAN SET DATA LOKASI ===============================================

            'VALIDASI DAN SET DATA ROW LOKASI ==================================================
            Dim JmlDtLokasi As Integer = dataLokasi.Length
            For i = 1 To JmlDtLokasi
                'SPLIT DATA LOKASI
                dataRowLokasi = dataLokasi(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA LOKASI -----------------------------------
                'CEK ARRAY DATA LOKASI
                If (dataRowLokasi.Length <> 2) Then
                    result(2) = "Location Row : " & i & " - Invalid location transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW LOKASI ----------------------------

                'VALIDASI TIPE DATA LOKASI ------------------------------------------
                'userid(0) As Integer
                If (IsNumeric(dataRowLokasi(0)) = False) Then
                    result(2) = "Location Row : " & i & " - userid required numeric." : GoTo selesai
                End If
                'lokasi(1) As String
                If (Len(dataRowLokasi(1)) = 0) Then
                    result(2) = "Location Row : " & i & " - lokasi can't be empty." : GoTo selesai
                End If
                If (Len(dataRowLokasi(1)) > 25) Then
                    result(2) = "Location Row : " & i & " - lokasi should not be more than 25 character." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA LOKASI -----------------------------------

                'VALIDASI DATA LOKASI ---------------------------------------
                'END OF VALIDASI DATA LOKASI --------------------------------

                If AsDataTableTambahData(dtlokasi, "userid~lokasi", dataRowLokasi(0) & "~" & dataRowLokasi(1)) = False Then
                    result(2) = "Location Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA LOKASI ===========================================

        End If


        'MAPPING BUAT WS DATA GUDANG -------------------------------------------------------
        'userid(0) As Integer, gudang(1) As String

        'MAPPING BUAT FLEX DATA GUDANG -----------------------------------------------------
        'userid, gudang

        'Buat datatable gudang
        Dim dtgudang As New DataTable
        AsDataTableTambahField(dtgudang, "userid", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtgudang, "gudang", AsEnumTypeData.AsString)

        ''Tambahkan gudang utama
        ''userid(0) As Integer, ugudang(7) As String
        'If AsDataTableTambahData(dtgudang, "userid~gudang", dataUtama(0) & "~" & dataUtama(7)) = False Then
        '    result(2) = "Main Warehouse : insert into datatable failed." : GoTo selesai
        'End If

        If (Len(dataSplit(4)) > 0) Then

            'VALIDASI DAN SET DATA GUDANG ======================================================
            'SPLIT PARAMETER DATA GUDANG
            dataGudang = dataSplit(4).Split(sptRow)
            'END OF VALIDASI DAN SET DATA GUDANG ===============================================

            'VALIDASI DAN SET DATA ROW GUDANG ==================================================
            Dim JmlDtGudang As Integer = dataGudang.Length
            For i = 1 To JmlDtGudang
                'SPLIT DATA GUDANG
                dataRowGudang = dataGudang(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA GUDANG -----------------------------------
                'CEK ARRAY DATA GUDANG
                If (dataRowGudang.Length <> 2) Then
                    result(2) = "Warehouse Row : " & i & " - Invalid warehouse transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW GUDANG ----------------------------

                'VALIDASI TIPE DATA GUDANG ------------------------------------------
                'userid(0) As Integer
                If (IsNumeric(dataRowGudang(0)) = False) Then
                    result(2) = "Warehouse Row : " & i & " - userid required numeric." : GoTo selesai
                End If
                'gudang(1) As String
                If (Len(dataRowGudang(1)) = 0) Then
                    result(2) = "Warehouse Row : " & i & " - gudang can't be empty." : GoTo selesai
                End If
                If (Len(dataRowGudang(1)) > 25) Then
                    result(2) = "Warehouse Row : " & i & " - gudang should not be more than 25 character." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA GUDANG -----------------------------------

                'VALIDASI DATA GUDANG ---------------------------------------
                'END OF VALIDASI DATA GUDANG --------------------------------

                If AsDataTableTambahData(dtgudang, "userid~gudang", dataRowGudang(0) & "~" & dataRowGudang(1)) = False Then
                    result(2) = "Warehouse Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA GUDANG ===========================================

        End If


        'MAPPING BUAT WS DATA Coa -------------------------------------------------------
        'userid(0) As Integer, norek(1) As String

        'MAPPING BUAT FLEX DATA Coa -----------------------------------------------------
        'userid, norek

        'Buat datatable Coa
        Dim dtCoa As New DataTable
        AsDataTableTambahField(dtCoa, "userid", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtCoa, "norek", AsEnumTypeData.AsString)

        If (Len(dataSplit(5)) > 0) Then

            'VALIDASI DAN SET DATA Coa ======================================================
            'SPLIT PARAMETER DATA Coa
            dataCoa = dataSplit(5).Split(sptRow)
            'END OF VALIDASI DAN SET DATA Coa ===============================================

            'VALIDASI DAN SET DATA ROW Coa ==================================================
            Dim JmlDtCoa As Integer = dataCoa.Length
            For i = 1 To JmlDtCoa
                'SPLIT DATA Coa
                dataRowCoa = dataCoa(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA Coa -----------------------------------
                'CEK ARRAY DATA Coa
                If (dataRowCoa.Length <> 2) Then
                    result(2) = "Coa Row : " & i & " - Invalid Coa transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW Coa ----------------------------

                'VALIDASI TIPE DATA Coa ------------------------------------------
                'userid(0) As Integer
                If (IsNumeric(dataRowCoa(0)) = False) Then
                    result(2) = "Coa Row : " & i & " - userid required numeric." : GoTo selesai
                End If
                'Coa(1) As String
                If (Len(dataRowCoa(1)) = 0) Then
                    result(2) = "Coa Row : " & i & " - norek can't be empty." : GoTo selesai
                End If
                If (Len(dataRowCoa(1)) > 25) Then
                    result(2) = "Coa Row : " & i & " - norek should not be more than 25 character." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA Coa -----------------------------------

                'VALIDASI DATA Coa ---------------------------------------
                'END OF VALIDASI DATA Coa --------------------------------

                If AsDataTableTambahData(dtCoa, "userid~norek", dataRowCoa(0) & "~" & dataRowCoa(1)) = False Then
                    result(2) = "Coa Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA Coa ===========================================

        End If


        'SIMPAN KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0

        Try
            'Proses utama
            If (dtutama.Rows.Count > 0) Then
                Dim dr1 As DataRow = dtutama.Rows(0)
                If isUpdate Then
                    result(4) = dr1("userid")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(userid) FROM M0_User WHERE userid=" & result(4))
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then
                        'sql = "Update M0_User set ukode  = '" & FixQuotes(dr1("ukode")) & "', unama  = '" & FixQuotes(dr1("unama")) & "', upassword  = '" & FixQuotes(dr1("upassword")) & "', ukontak  = " & dr1("ukontak") & ", ucabang  = '" & FixQuotes(dr1("ucabang")) & "', ulokasi  = '" & FixQuotes(dr1("ulokasi")) & "', ugudang  = '" & FixQuotes(dr1("ugudang")) & "', ukota  = '" & FixQuotes(dr1("ukota")) & "', ugambar  = '" & FixQuotes(dr1("ugambar")) & "', uaktif  = " & dr1("uaktif") & ", utglexpired  = '" & FixQuotes(AsFormatTanggal(dr1("utglexpired"))) & "', ulevel  = " & dr1("ulevel") & ", ugrup  = " & dr1("ugrup") & ", ubahasa  = '" & FixQuotes(dr1("ubahasa")) & "', udefaultview  = " & dr1("udefaultview") & " where userid = " & dr1("userid") & ""
                        'PASSWORD TIDAK DI UPDATE, ADA FASILITAS SENDIRI UNTUK UPDATE PASSWORD
                        sql = "Update M0_User set ukode  = '" & FixQuotes(dr1("ukode")) & "', unama  = '" & FixQuotes(dr1("unama")) & "', ukontak  = " & dr1("ukontak") & ", ucabang  = '" & FixQuotes(dr1("ucabang")) & "', ulokasi  = '" & FixQuotes(dr1("ulokasi")) & "', ugudang  = '" & FixQuotes(dr1("ugudang")) & "', ukota  = '" & FixQuotes(dr1("ukota")) & "', ugambar  = '" & FixQuotes(dr1("ugambar")) & "', uaktif  = " & dr1("uaktif") & ", utglexpired  = '" & FixQuotes(AsFormatTanggal(dr1("utglexpired"))) & "', ulevel  = " & dr1("ulevel") & ", ugrup  = " & dr1("ugrup") & ", ubahasa  = '" & FixQuotes(dr1("ubahasa")) & "', udefaultview  = " & dr1("udefaultview") & " where userid = " & dr1("userid") & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                        'update user serenity
                        sql = "Update users set DisplayName  = '" & FixQuotes(dr1("unama")) & "' where Username = '" & FixQuotes(dr1("ukode")) & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Else
                        result(2) = "Transaction data not found." : Trans.Rollback() : GoTo selesai
                    End If
                Else

                    ''CEK JML USER ====================================
                    ''USER HANYA DIPERBOLEHKAN SEBANYAK 15 USER
                    'Dim jmlUser As Double = 15
                    'sql = "SELECT userid FROM m0_user"
                    'Dim dtRowUser As DataTable = AsDataTableAmbilDariDB(sql)
                    'If dtRowUser.Rows.Count >= jmlUser Then
                    '    result(2) = "Couldn't add new user. Count of user has been reached maximum limit (" & FixDouble(jmlUser) & " User)." : Trans.Rollback() : GoTo selesai
                    'End If
                    ''END OF CEK JML USER =============================

                    sql = "Insert into M0_User (ukode, unama, upassword, ukontak, ucabang, ulokasi, ugudang, ukota, ugambar, uaktif, utglexpired, ulevel, ugrup, ubahasa, udefaultview) values('" & FixQuotes(dr1("ukode")) & "', '" & FixQuotes(dr1("unama")) & "', '" & FixQuotes(dr1("upassword")) & "', " & dr1("ukontak") & ", '" & FixQuotes(dr1("ucabang")) & "', '" & FixQuotes(dr1("ulokasi")) & "', '" & FixQuotes(dr1("ugudang")) & "', '" & FixQuotes(dr1("ukota")) & "', '" & FixQuotes(dr1("ugambar")) & "', " & dr1("uaktif") & ", '" & FixQuotes(AsFormatTanggal(dr1("utglexpired"))) & "', " & dr1("ulevel") & ", " & dr1("ugrup") & ", '" & FixQuotes(dr1("ubahasa")) & "', " & dr1("udefaultview") & ")"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'insert user serenity
                    sql = "INSERT INTO users(UserId, Username, DisplayName, Email, Source, PasswordHash, PasswordSalt, LastDirectoryUpdate, UserImage, InsertDate, InsertUserId, UpdateDate, UpdateUserId, IsActive, MobilePhoneNumber, MobilePhoneVerified, TwoFactorAuth) VALUES (0, '" & FixQuotes(dr1("ukode")) & "', '" & FixQuotes(dr1("unama")) & "', 'myerpplus@gmail.com', 'site', '72voACKERAWQthCRTJ0pnmhTacu2wbXLYF6XxSOcTTvENICuQNbqnJfOOdSCku/hTnFGX4scWjNo84gfP8ko/g', '4ir4m', NOW(), '', NOW(), 1, NOW(), 1, 1, '', 0, NULL)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                    'ambil id user serenity
                    Dim dtsu As New DataTable
                    dtsu = AsDataTableAmbilDariDB("SELECT userid FROM `users` WHERE Username = '" & dr1("ukode") & "'")
                    If dtsu.Rows.Count > 0 Then
                        'insert serenity permission
                        sql = "INSERT INTO userpermissions(UserPermissionId, UserId, PermissionKey, Granted) VALUES (0, " & dtsu.Rows(0)(0) & ", 'Administration:General', 1)"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Else
                        result(2) = "Main transaction Serenity data not found. " & sql
                        Trans.Rollback()
                        GoTo selesai
                    End If


                    Dim dt2 As New DataTable
                    'Sql disesuaikan sendiri, untuk parameternya disesuaikan sendiri.
                    dt2 = AsDataTableAmbilDariDB("select userid from M0_user where ukode='" & dr1("ukode") & "' AND upassword= '" & dr1("upassword") & "'")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

            Else
                result(2) = "Main Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            'Hapus detail ketika update
            If (isUpdate) Then
                sql = "Delete from M0_User_Role where userid = " & result(4)
                'result(2) = sql : GoTo selesai
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                ' delete user_role serenity
                sql = "Delete from m0_user_role_s where userid = " & result(4)
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'Proses detail
            If (dtdetail.Rows.Count > 0) Then
                Dim strValue2 As New StringBuilder
                For Each dr1 As DataRow In dtdetail.Rows
                    strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Append("(" & result(4) & ", '" & dr1("role") & "')")
                Next
                sql = "Insert into M0_User_Role(userid, role) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE role = VALUES(role)"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                ' insert user_role serenity
                sql = "Insert into m0_user_role_s(userid, role) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE role = VALUES(role)"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If


            'Hapus cabang ketika update
            If (isUpdate) Then
                sql = "Delete from M0_User_Branch where userid = " & result(4)
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'Proses cabang
            If (dtcabang.Rows.Count > 0) Then
                Dim strValue2 As New StringBuilder
                For Each dr1 As DataRow In dtcabang.Rows
                    strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Append("(" & result(4) & ", '" & dr1("cabang") & "')")
                Next
                sql = "Insert into M0_User_Branch(userid, cabang) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE cabang = VALUES(cabang)"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If


            'Hapus lokasi ketika update
            If (isUpdate) Then
                sql = "Delete from M0_User_Location where userid = " & result(4)
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'Proses lokasi
            If (dtlokasi.Rows.Count > 0) Then
                Dim strValue2 As New StringBuilder
                For Each dr1 As DataRow In dtlokasi.Rows
                    strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Append("(" & result(4) & ", '" & dr1("lokasi") & "')")
                Next
                sql = "Insert into M0_User_Location(userid, lokasi) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE lokasi = VALUES(lokasi)"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If


            'Hapus gudang ketika update
            If (isUpdate) Then
                sql = "Delete from M0_User_Warehouse where userid = " & result(4)
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'Proses gudang
            If (dtgudang.Rows.Count > 0) Then
                Dim strValue2 As New StringBuilder
                For Each dr1 As DataRow In dtgudang.Rows
                    strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Append("(" & result(4) & ", '" & dr1("gudang") & "')")
                Next
                sql = "Insert into M0_User_Warehouse(userid, gudang) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE gudang = VALUES(gudang)"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'Hapus coa ketika update
            If (isUpdate) Then
                sql = "Delete from M0_User_Coa where userid = " & result(4)
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'Proses coa
            If (dtCoa.Rows.Count > 0) Then
                Dim strValue2 As New StringBuilder
                For Each dr1 As DataRow In dtCoa.Rows
                    strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Append("(" & result(4) & ", '" & dr1("norek") & "')")
                Next
                sql = "Insert into M0_User_Coa(userid, norek) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE norek = VALUES(norek)"
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
            Dim paramSearch As String = M0_User_VSearch(PostWsSearch(paramSplit(0), "M0_User_VSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
            Dim hasilSearch As New RsHasilWsSearch
            hasilSearch = GetWsSearch(paramSearch)

            result(1) = hasilSearch.success
            result(2) = hasilSearch.errmessage

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
    Public Function M0_UserDelete(ByVal param As String) As String

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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(5)) = False) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
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

            'DELETE AKUN
            sql = "DELETE FROM M0_User_Coa WHERE userid = " & idtransaksi
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE GUDANG
            sql = "DELETE FROM M0_User_Warehouse WHERE userid = " & idtransaksi
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE LOKASI
            sql = "DELETE FROM M0_User_Location WHERE userid = " & idtransaksi
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE CABANG
            sql = "DELETE FROM M0_User_Branch WHERE userid = " & idtransaksi
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M0_User_Role WHERE userid = " & idtransaksi
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE role serenity
            sql = "DELETE FROM m0_user_role_s WHERE userid = " & idtransaksi
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'ambil id user serenity
            Dim dtsu As New DataTable
            dtsu = AsDataTableAmbilDariDB("SELECT u.UserId FROM users u JOIN m0_user m ON m.ukode = u.Username WHERE m.userid = " & idtransaksi)
            If dtsu.Rows.Count > 0 Then
                'DELETE userpermissions serenity
                sql = "DELETE FROM userpermissions WHERE UserId = " & dtsu.Rows(0)(0)
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'DELETE user serenity
                sql = "DELETE FROM users WHERE UserId = " & dtsu.Rows(0)(0)
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            Else
                result(2) = "ID user Serenity data not found."
                Trans.Rollback()
                GoTo selesai
            End If

            'DELETE UTAMA
            sql = "DELETE FROM M0_User WHERE userid = " & idtransaksi
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
            Dim paramSearch As String = M0_User_VSearch(PostWsSearch(paramSplit(0), "M0_User_VSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
            Dim hasilSearch As New RsHasilWsSearch
            hasilSearch = GetWsSearch(paramSearch)

            result(1) = hasilSearch.success
            result(2) = hasilSearch.errmessage

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
    Public Function M0_UserGetdataById(ByVal param As String) As String

        'M0_UserGetdataById Utama --------------------------------------------------------
        'userid, ukode, unama, upassword, ukontak, ucabang, ulokasi, 
        'ugudang, ukota, ugambar, uaktif, utglexpired, ulevel, ugrup, 
        'ubahasa, udefaultview, ukontakkode, ukontaknama, ucabangnama, ulokasinama, ugudangnama, 
        'ubahasanama

        'M0_UserGetdataById Detail -------------------------------------------------------
        'uruserid, role, rolenama

        'M0_UserGetdataById Cabang -------------------------------------------------------
        'ubuserid, cabang, bnama

        'M0_UserGetdataById Lokasi -------------------------------------------------------
        'uluserid, lokasi, lnama, lcabang

        'M0_UserGetdataById Gudang -------------------------------------------------------
        'uwuserid, gudang, wnama, wlokasi

        'M0_UserGetdataById Coa -------------------------------------------------------
        'ucuserid, norek, cnama

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = "", strResultData As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = ""
        Dim dt As New DataTable

        Dim utama As String = "", detail As String = "", cabang As String = "", lokasi As String = "", gudang As String = "", coa As String = "", idtransaksi As String = ""

        'SET DEFAULT RESULT
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0
        result(2) = ""
        result(3) = 0
        result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0
        resultPaging(1) = 0
        resultPaging(2) = 0
        resultPaging(3) = 0
        resultPaging(4) = 0

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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        Dim NmMemcached As String = "aplikasi1-M0_User~M0_User_Role-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "u.userid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "u.userid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m0_user_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("userid"), 0), sptField,
                     FxDB(drutama("ukode"), ""), sptField,
                     FxDB(drutama("unama"), ""), sptField,
                     "", sptField,
                     FxDB(drutama("ukontak"), 0), sptField,
                     FxDB(drutama("ucabang"), ""), sptField,
                     FxDB(drutama("ulokasi"), ""), sptField,
                     FxDB(drutama("ugudang"), ""), sptField,
                     FxDB(drutama("ukota"), ""), sptField,
                     FxDB(drutama("ugambar"), ""), sptField,
                     FxDB(drutama("uaktif"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("utglexpired"), ""), formatTgl), sptField,
                     FxDB(drutama("ulevel"), 0), sptField,
                     FxDB(drutama("ugrup"), 0), sptField,
                     FxDB(drutama("ubahasa"), ""), sptField,
                     FxDB(drutama("udefaultview"), 0), sptField,
                     FxDB(drutama("ukontakkode"), ""), sptField,
                     FxDB(drutama("ukontaknama"), ""), sptField,
                     FxDB(drutama("ucabangnama"), ""), sptField,
                     FxDB(drutama("ulokasinama"), ""), sptField,
                     FxDB(drutama("ugudangnama"), ""), sptField,
                     FxDB(drutama("ubahasanama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("uruserid"), 0), sptField,
                                               FxDB(dr("role"), ""), sptField,
                                               FxDB(dr("rolenama"), ""), sptRow)
            Next
            If detail.Length > sptRow.Length Then detail = detail.Substring(0, detail.Length - sptRow.Length)


            'AMBIL DATA CABANG
            sql = "SELECT ub.userid as ubuserid, ub.cabang, b.bnama FROM m0_user_branch ub JOIN m1_branch b ON ub.cabang = b.bkode"
            Dim dtcabang As New DataTable
            dtcabang = AmbilData("aplikasi1-M0_User_Branch", "userid = '" & idtransaksi & "'", "cabang ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtcabang.Rows
                cabang = String.Concat(cabang,
                     FxDB(dr("ubuserid"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("bnama"), ""), sptRow)
            Next
            If cabang.Length > sptRow.Length Then cabang = cabang.Substring(0, cabang.Length - sptRow.Length)


            'AMBIL DATA LOKASI
            sql = "SELECT ul.userid as uluserid, ul.lokasi, l.lnama, l.lcabang FROM m0_user_location ul JOIN m1_location l ON ul.lokasi = l.lkode"
            Dim dtlokasi As New DataTable
            dtlokasi = AmbilData("aplikasi1-M0_User_Location", "userid = '" & idtransaksi & "'", "lokasi ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtlokasi.Rows
                lokasi = String.Concat(lokasi,
                     FxDB(dr("uluserid"), 0), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("lnama"), ""), sptField,
                     FxDB(dr("lcabang"), ""), sptRow)
            Next
            If lokasi.Length > sptRow.Length Then lokasi = lokasi.Substring(0, lokasi.Length - sptRow.Length)


            'AMBIL DATA GUDANG
            sql = "SELECT uw.userid as uwuserid, uw.gudang, w.wnama, w.wlokasi FROM m0_user_warehouse uw JOIN m1_warehouse w ON uw.gudang = w.wkode"
            Dim dtgudang As New DataTable
            dtgudang = AmbilData("aplikasi1-M0_User_Warehouse", "userid = '" & idtransaksi & "'", "gudang ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtgudang.Rows
                gudang = String.Concat(gudang,
                     FxDB(dr("uwuserid"), 0), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("wnama"), ""), sptField,
                     FxDB(dr("wlokasi"), ""), sptRow)
            Next
            If gudang.Length > sptRow.Length Then gudang = gudang.Substring(0, gudang.Length - sptRow.Length)


            'AMBIL DATA COA
            sql = "SELECT uc.userid as ucuserid, uc.norek, c.cnama FROM m0_user_coa uc JOIN m1_coa c ON uc.norek = c.cnomor"
            Dim dtcoa As New DataTable
            dtcoa = AmbilData("aplikasi1-M0_User_Coa", "userid = '" & idtransaksi & "'", "norek ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtcoa.Rows
                coa = String.Concat(coa,
                     FxDB(dr("ucuserid"), 0), sptField,
                     FxDB(dr("norek"), ""), sptField,
                     FxDB(dr("cnama"), ""), sptRow)
            Next
            If coa.Length > sptRow.Length Then coa = coa.Substring(0, coa.Length - sptRow.Length)


            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow

        Else
            result(2) = "User data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, cabang, sptSubParam, lokasi, sptSubParam, gudang, sptSubParam, coa)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("userid, ukode, unama, upassword, ukontak, ucabang, ulokasi, ugudang, ukota, ugambar, uaktif, utglexpired, ulevel, ugrup, ubahasa, udefaultview, ukontakkode, ukontaknama, ucabangnama, ulokasinama, ugudangnama, ubahasanama"), sptSubParam, ReplaceMapping("uruserid, role, rolenama"), sptSubParam, ReplaceMapping("ubuserid, cabang, bnama"), sptSubParam, ReplaceMapping("uluserid, lokasi, lnama, lcabang"), sptSubParam, ReplaceMapping("uwuserid, gudang, wnama, wlokasi"), sptSubParam, ReplaceMapping("ucuserid, norek, cnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M0_User_SGetdataById(ByVal param As String) As String

        'M0_UserGetdataById Utama --------------------------------------------------------
        'userid, ukode, unama, upassword, ukontak, ucabang, ulokasi, 
        'ugudang, ukota, ugambar, uaktif, utglexpired, ulevel, ugrup, 
        'ubahasa, udefaultview, ukontakkode, ukontaknama, ucabangnama, ulokasinama, ugudangnama, 
        'ubahasanama

        'M0_UserGetdataById Detail -------------------------------------------------------
        'uruserid, role, rolenama

        'M0_UserGetdataById Cabang -------------------------------------------------------
        'ubuserid, cabang, bnama

        'M0_UserGetdataById Lokasi -------------------------------------------------------
        'uluserid, lokasi, lnama, lcabang

        'M0_UserGetdataById Gudang -------------------------------------------------------
        'uwuserid, gudang, wnama, wlokasi

        'M0_UserGetdataById Coa -------------------------------------------------------
        'ucuserid, norek, cnama

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = "", strResultData As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = ""
        Dim dt As New DataTable

        Dim utama As String = "", detail As String = "", cabang As String = "", lokasi As String = "", gudang As String = "", coa As String = "", idtransaksi As String = ""

        'SET DEFAULT RESULT
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0
        result(2) = ""
        result(3) = 0
        result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0
        resultPaging(1) = 0
        resultPaging(2) = 0
        resultPaging(3) = 0
        resultPaging(4) = 0

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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        Dim NmMemcached As String = "aplikasi1-M0_User~M0_User_Role-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "u.userid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "u.userid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select `u`.`userid` AS `userid`,`u`.`ukode` AS `ukode`,`u`.`unama` AS `unama`,`u`.`upassword` AS `upassword`,`u`.`ukontak` AS `ukontak`,`u`.`ucabang` AS `ucabang`,`u`.`ulokasi` AS `ulokasi`,`u`.`ugudang` AS `ugudang`,`u`.`ukota` AS `ukota`,`u`.`ugambar` AS `ugambar`,`u`.`uaktif` AS `uaktif`,`u`.`utglexpired` AS `utglexpired`,`u`.`ulevel` AS `ulevel`,`u`.`ugrup` AS `ugrup`,`u`.`ubahasa` AS `ubahasa`,`u`.`udefaultview` AS `udefaultview`,`c`.`kkode` AS `ukontakkode`,`c`.`knama` AS `ukontaknama`,`br`.`bnama` AS `ucabangnama`,`lc`.`lnama` AS `ulokasinama`,`wh`.`wnama` AS `ugudangnama`,`l`.`lnama` AS `ubahasanama`,`ur`.`userid` AS `uruserid`,`ur`.`role` AS `role`,`r`.`rnama` AS `rolenama` from (((((((`m0_user` `u` left join `m0_user_role` `ur` on((`u`.`userid` = `ur`.`userid`))) left join `m1_contact` `c` on((`u`.`ukontak` = `c`.`kid`))) left join `m1_branch` `br` on((`u`.`ucabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`u`.`ulokasi` = `lc`.`lkode`))) left join `m1_warehouse` `wh` on((`u`.`ugudang` = `wh`.`wkode`))) left join `m0_language` `l` on((`u`.`ubahasa` = `l`.`lkode`))) left join `m0_role_s` `r` on((`ur`.`role` = `r`.`rkode`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("userid"), 0), sptField,
                     FxDB(drutama("ukode"), ""), sptField,
                     FxDB(drutama("unama"), ""), sptField,
                     "", sptField,
                     FxDB(drutama("ukontak"), 0), sptField,
                     FxDB(drutama("ucabang"), ""), sptField,
                     FxDB(drutama("ulokasi"), ""), sptField,
                     FxDB(drutama("ugudang"), ""), sptField,
                     FxDB(drutama("ukota"), ""), sptField,
                     FxDB(drutama("ugambar"), ""), sptField,
                     FxDB(drutama("uaktif"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("utglexpired"), ""), formatTgl), sptField,
                     FxDB(drutama("ulevel"), 0), sptField,
                     FxDB(drutama("ugrup"), 0), sptField,
                     FxDB(drutama("ubahasa"), ""), sptField,
                     FxDB(drutama("udefaultview"), 0), sptField,
                     FxDB(drutama("ukontakkode"), ""), sptField,
                     FxDB(drutama("ukontaknama"), ""), sptField,
                     FxDB(drutama("ucabangnama"), ""), sptField,
                     FxDB(drutama("ulokasinama"), ""), sptField,
                     FxDB(drutama("ugudangnama"), ""), sptField,
                     FxDB(drutama("ubahasanama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("uruserid"), 0), sptField,
                                               FxDB(dr("role"), ""), sptField,
                                               FxDB(dr("rolenama"), ""), sptRow)
            Next
            If detail.Length > sptRow.Length Then detail = detail.Substring(0, detail.Length - sptRow.Length)


            'AMBIL DATA CABANG
            sql = "SELECT ub.userid as ubuserid, ub.cabang, b.bnama FROM m0_user_branch ub JOIN m1_branch b ON ub.cabang = b.bkode"
            Dim dtcabang As New DataTable
            dtcabang = AmbilData("aplikasi1-M0_User_Branch", "userid = '" & idtransaksi & "'", "cabang ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtcabang.Rows
                cabang = String.Concat(cabang,
                     FxDB(dr("ubuserid"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("bnama"), ""), sptRow)
            Next
            If cabang.Length > sptRow.Length Then cabang = cabang.Substring(0, cabang.Length - sptRow.Length)


            'AMBIL DATA LOKASI
            sql = "SELECT ul.userid as uluserid, ul.lokasi, l.lnama, l.lcabang FROM m0_user_location ul JOIN m1_location l ON ul.lokasi = l.lkode"
            Dim dtlokasi As New DataTable
            dtlokasi = AmbilData("aplikasi1-M0_User_Location", "userid = '" & idtransaksi & "'", "lokasi ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtlokasi.Rows
                lokasi = String.Concat(lokasi,
                     FxDB(dr("uluserid"), 0), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("lnama"), ""), sptField,
                     FxDB(dr("lcabang"), ""), sptRow)
            Next
            If lokasi.Length > sptRow.Length Then lokasi = lokasi.Substring(0, lokasi.Length - sptRow.Length)


            'AMBIL DATA GUDANG
            sql = "SELECT uw.userid as uwuserid, uw.gudang, w.wnama, w.wlokasi FROM m0_user_warehouse uw JOIN m1_warehouse w ON uw.gudang = w.wkode"
            Dim dtgudang As New DataTable
            dtgudang = AmbilData("aplikasi1-M0_User_Warehouse", "userid = '" & idtransaksi & "'", "gudang ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtgudang.Rows
                gudang = String.Concat(gudang,
                     FxDB(dr("uwuserid"), 0), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("wnama"), ""), sptField,
                     FxDB(dr("wlokasi"), ""), sptRow)
            Next
            If gudang.Length > sptRow.Length Then gudang = gudang.Substring(0, gudang.Length - sptRow.Length)


            'AMBIL DATA COA
            sql = "SELECT uc.userid as ucuserid, uc.norek, c.cnama FROM m0_user_coa uc JOIN m1_coa c ON uc.norek = c.cnomor"
            Dim dtcoa As New DataTable
            dtcoa = AmbilData("aplikasi1-M0_User_Coa", "userid = '" & idtransaksi & "'", "norek ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtcoa.Rows
                coa = String.Concat(coa,
                     FxDB(dr("ucuserid"), 0), sptField,
                     FxDB(dr("norek"), ""), sptField,
                     FxDB(dr("cnama"), ""), sptRow)
            Next
            If coa.Length > sptRow.Length Then coa = coa.Substring(0, coa.Length - sptRow.Length)


            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow

        Else
            result(2) = "User data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, cabang, sptSubParam, lokasi, sptSubParam, gudang, sptSubParam, coa)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("userid, ukode, unama, upassword, ukontak, ucabang, ulokasi, ugudang, ukota, ugambar, uaktif, utglexpired, ulevel, ugrup, ubahasa, udefaultview, ukontakkode, ukontaknama, ucabangnama, ulokasinama, ugudangnama, ubahasanama"), sptSubParam, ReplaceMapping("uruserid, role, rolenama"), sptSubParam, ReplaceMapping("ubuserid, cabang, bnama"), sptSubParam, ReplaceMapping("uluserid, lokasi, lnama, lcabang"), sptSubParam, ReplaceMapping("uwuserid, gudang, wnama, wlokasi"), sptSubParam, ReplaceMapping("ucuserid, norek, cnama"))

        Return wsResult
    End Function


    <WebMethod()>
    Public Function M0_UserUpdatePassword(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = ""
        Dim oldpass As String = "", newpass As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = "", search As String = ""
        Dim dt As New DataTable

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

        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptField)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS -----------------------------------------------------------
        'upasswordlama(0) As String, upasswordbaru(1) As String

        'MAPPING BUAT FLEX ---------------------------------------------------------
        'upasswordlama, upasswordbaru

        'VALIDASI DATA UTAMA =======================================================
        'upasswordlama(0) As String
        If Len(dataSplit(0)) = 0 Then
            result(2) = "Old password can't be empty" : GoTo selesai
        Else
            oldpass = dataSplit(0).ToString
        End If

        'upasswordbaru(1) As String
        If Len(dataSplit(1)) = 0 Then
            result(2) = "New password can't be empty" : GoTo selesai
        End If
        'generate password baru
        dataSplit(1) = CreateSHAHash(dataSplit(1), "AlEuPj13")
        If Len(dataSplit(1)) > 250 Then
            result(2) = "New password should not be more than 250 character." : GoTo selesai
        Else
            newpass = dataSplit(1).ToString
        End If
        'END OF VALIDASI DATA UTAMA ================================================

        'CEK akses custom =========================================================
        Dim dtHACustom As DataTable = AsDataTableAmbilDariDB("SELECT rc.rcmoduleid, rc.rcidpc, rc.rcrole, rc.rcakses FROM m0_permissions_custom pc JOIN m0_role_custom rc ON pc.pcmodule = rc.rcmoduleid AND pc.pcid = rc.rcidpc AND pc.pcmodule = 0 AND pc.pcid = 1 JOIN m0_user_role ur ON rc.rcrole = ur.role AND ur.userid = '" & userid & "' ORDER BY rc.rcakses DESC LIMIT 1")
        If dtHACustom.Rows.Count > 0 Then
            If dtHACustom.Rows(0)("rcakses") = 1 Then
                GoTo validasipasslama
            End If
        End If
        result(2) = "You doesn't have permission update password other user" : GoTo selesai
        'endakses custom =========================================================


        'CEK PASSWORD LAMA =========================================================
validasipasslama:
        Dim dtold As DataTable = AsDataTableAmbilDariDB("SELECT upassword FROM m0_user WHERE userid ='" & userid & "' ")
        If dtold.Rows.Count > 0 Then
            'JIKA PUNYA HAK AKSES MAKA BISA MERUBAH PASSWAORD TANPA CEK PASSWORD LAMA
            Dim rsHakAksesCustom As String = HakAksesCustom(0, 1, "Update Password", userid)
            If Len(rsHakAksesCustom) <> 0 Then
                If dtold.Rows(0)("upassword") <> CreateSHAHash(oldpass, "AlEuPj13") Then
                    result(2) = "Invalid old password." : GoTo selesai
                End If
            End If

        Else
            result(2) = "User data not found." : GoTo selesai
        End If
        'END OF CEK PASSWORD LAMA ==================================================


        'SIMPAN KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Try
            sql = "Update M0_User set upassword  = '" & FixQuotes(newpass) & "' where userid = '" & userid & "'"
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
            result(4) = userid

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
    Public Function M0_UserUpdatePassword_S20241205(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = ""
        Dim oldpass As String = "", newpass As String = "", username As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = "", search As String = ""
        Dim dt As New DataTable

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

        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptField)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 3) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS -----------------------------------------------------------
        'upasswordlama(0) As String, upasswordbaru(1) As String

        'MAPPING BUAT FLEX ---------------------------------------------------------
        'upasswordlama, upasswordbaru

        'VALIDASI DATA UTAMA =======================================================
        'upasswordlama(0) As String
        If Len(dataSplit(0)) = 0 Then
            result(2) = "Old password can't be empty" : GoTo selesai
        Else
            oldpass = dataSplit(0).ToString
        End If

        'upasswordbaru(1) As String
        If Len(dataSplit(1)) = 0 Then
            result(2) = "New password can't be empty" : GoTo selesai
        End If
        'generate password baru
        dataSplit(1) = CreateSHAHash(dataSplit(1), "AlEuPj13")
        If Len(dataSplit(1)) > 250 Then
            result(2) = "New password should not be more than 250 character." : GoTo selesai
        Else
            newpass = dataSplit(1).ToString
        End If

        'upasswordbaru(1) As String
        If Len(dataSplit(2)) = 0 Then
            result(2) = "username can't be empty" : GoTo selesai
        End If
        username = dataSplit(2).ToString
        'END OF VALIDASI DATA UTAMA ================================================


        'CEK akses custom =========================================================
        Dim dtHACustom As DataTable = AsDataTableAmbilDariDB("SELECT rc.rcmoduleid, rc.rcidpc, rc.rcrole, rc.rcakses FROM m0_permissions_custom pc JOIN m0_role_custom rc ON pc.pcmodule = rc.rcmoduleid AND pc.pcid = rc.rcidpc AND pc.pcmodule = 0 AND pc.pcid = 1 JOIN m0_user_role ur ON rc.rcrole = ur.role AND ur.userid = '" & userid & "' ORDER BY rc.rcakses DESC LIMIT 1")
        If dtHACustom.Rows.Count > 0 Then
            If dtHACustom.Rows(0)("rcakses") = 1 Then
                GoTo validasipasslama
            End If
        End If
        result(2) = "You doesn't have permission update password other user" : GoTo selesai
        'endakses custom =========================================================


        'CEK PASSWORD LAMA =========================================================
validasipasslama:
        Dim dtold As DataTable = AsDataTableAmbilDariDB("SELECT upassword FROM m0_user WHERE ukode ='" & username & "' ")
        If dtold.Rows.Count > 0 Then
            If dtold.Rows(0)("upassword") <> CreateSHAHash(oldpass, "AlEuPj13") Then
                result(2) = "Invalid old password." : GoTo selesai
            End If
        Else
            result(2) = "User data not found." : GoTo selesai
        End If
        'END OF CEK PASSWORD LAMA ==================================================

        'result(1) = 0
        'result(2) = "test " & dataSplit.ToString : GoTo selesai

        'SIMPAN KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Try
            sql = "Update M0_User set upassword  = '" & FixQuotes(newpass) & "' where ukode ='" & username & "' "
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
            result(4) = userid

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
    Public Function M0_UserUpdatePassword_S(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = ""
        Dim oldpass As String = "", newpass As String = "", username As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = "", search As String = ""
        Dim dt As New DataTable

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

        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptField)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 3) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS -----------------------------------------------------------
        'upasswordlama(0) As String, upasswordbaru(1) As String

        'MAPPING BUAT FLEX ---------------------------------------------------------
        'upasswordlama, upasswordbaru

        'VALIDASI DATA UTAMA =======================================================
        'upasswordlama(0) As String
        If Len(dataSplit(0)) = 0 Then
            result(2) = "Old password can't be empty" : GoTo selesai
        Else
            oldpass = dataSplit(0).ToString
        End If

        'upasswordbaru(1) As String
        If Len(dataSplit(1)) = 0 Then
            result(2) = "New password can't be empty" : GoTo selesai
        End If
        'generate password baru
        dataSplit(1) = CreateSHAHash(dataSplit(1), "AlEuPj13")
        If Len(dataSplit(1)) > 250 Then
            result(2) = "New password should not be more than 250 character." : GoTo selesai
        Else
            newpass = dataSplit(1).ToString
        End If

        'upasswordbaru(1) As String
        If Len(dataSplit(2)) = 0 Then
            result(2) = "username can't be empty" : GoTo selesai
        End If
        username = dataSplit(2).ToString
        'END OF VALIDASI DATA UTAMA ================================================


        '        'CEK akses custom =========================================================
        '        Dim dtHACustom As DataTable = AsDataTableAmbilDariDB("SELECT rc.rcmoduleid, rc.rcidpc, rc.rcrole, rc.rcakses FROM m0_permissions_custom pc JOIN m0_role_custom rc ON pc.pcmodule = rc.rcmoduleid AND pc.pcid = rc.rcidpc AND pc.pcmodule = 0 AND pc.pcid = 1 JOIN m0_user_role ur ON rc.rcrole = ur.role AND ur.userid = '" & userid & "' ORDER BY rc.rcakses DESC LIMIT 1")
        '        If dtHACustom.Rows.Count > 0 Then
        '            If dtHACustom.Rows(0)("rcakses") = 1 Then
        '                GoTo validasipasslama
        '            End If
        '        End If
        '        result(2) = "You doesn't have permission update password other user" : GoTo selesai
        '        'endakses custom =========================================================


        '        'CEK PASSWORD LAMA =========================================================
        'validasipasslama:
        '        Dim dtold As DataTable = AsDataTableAmbilDariDB("SELECT upassword FROM m0_user WHERE ukode ='" & username & "' ")
        '        If dtold.Rows.Count > 0 Then
        '            If dtold.Rows(0)("upassword") <> CreateSHAHash(oldpass, "AlEuPj13") Then
        '                result(2) = "Invalid old password." : GoTo selesai
        '            End If
        '        Else
        '            result(2) = "User data not found." : GoTo selesai
        '        End If
        '        'END OF CEK PASSWORD LAMA ==================================================

        'CEK PASSWORD LAMA =========================================================
        Dim dtold As DataTable = AsDataTableAmbilDariDB("SELECT upassword FROM m0_user WHERE userid ='" & userid & "' ")
        If dtold.Rows.Count > 0 Then
            'CEK HAK AKSES CUSTOM UBAH PASSWORD
            'JIKA PUNYA HAK AKSES MAKA BISA MERUBAH PASSWAORD TANPA CEK PASSWORD LAMA
            Dim rsHakAksesCustom As String = HakAksesCustom(0, 1, "Update Password", userid)
            If Len(rsHakAksesCustom) <> 0 Then
                If dtold.Rows(0)("upassword") <> CreateSHAHash(oldpass, "AlEuPj13") Then
                    result(2) = "Invalid old password." : GoTo selesai
                End If
            End If

        Else
            result(2) = "User data not found." : GoTo selesai
        End If
        'END OF CEK PASSWORD LAMA ==================================================

        'result(1) = 0
        'result(2) = "test " & dataSplit.ToString : GoTo selesai

        'SIMPAN KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Try
            sql = "Update M0_User set upassword  = '" & FixQuotes(newpass) & "' where ukode ='" & username & "' "
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
            result(4) = userid

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
    Public Function M0_UserResetPassword_S(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = ""
        Dim username As String = "", newpass As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = "", search As String = ""
        Dim dt As New DataTable

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

        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptField)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS -----------------------------------------------------------
        'upasswordlama(0) As String, upasswordbaru(1) As String

        'MAPPING BUAT FLEX ---------------------------------------------------------
        'upasswordlama, upasswordbaru

        'VALIDASI DATA UTAMA =======================================================
        'upasswordlama(0) As String
        If Len(dataSplit(0)) = 0 Then
            result(2) = "username can't be empty" : GoTo selesai
        Else
            username = dataSplit(0).ToString
        End If

        'upasswordbaru(1) As String
        If Len(dataSplit(1)) = 0 Then
            result(2) = "New password can't be empty" : GoTo selesai
        End If
        'generate password baru
        dataSplit(1) = CreateSHAHash(dataSplit(1), "AlEuPj13")
        If Len(dataSplit(1)) > 250 Then
            result(2) = "New password should not be more than 250 character." : GoTo selesai
        Else
            newpass = dataSplit(1).ToString
        End If
        'END OF VALIDASI DATA UTAMA ================================================


        '        'CEK akses custom =========================================================
        '        Dim dtHACustom As DataTable = AsDataTableAmbilDariDB("SELECT rc.rcmoduleid, rc.rcidpc, rc.rcrole, rc.rcakses FROM m0_permissions_custom pc JOIN m0_role_custom rc ON pc.pcmodule = rc.rcmoduleid AND pc.pcid = rc.rcidpc AND pc.pcmodule = 0 AND pc.pcid = 1 JOIN m0_user_role ur ON rc.rcrole = ur.role AND ur.userid = '" & userid & "' ORDER BY rc.rcakses DESC LIMIT 1")
        '        If dtHACustom.Rows.Count > 0 Then
        '            If dtHACustom.Rows(0)("rcakses") = 1 Then
        '                GoTo validasipasslama
        '            End If
        '        End If
        '        result(2) = "You doesn't have permission update password other user" : GoTo selesai
        '        'endakses custom =========================================================
        'validasipasslama:

        'CEK PASSWORD LAMA =========================================================
        'CEK HAK AKSES CUSTOM UBAH PASSWORD
        'JIKA PUNYA HAK AKSES MAKA BISA MERUBAH PASSWAORD TANPA CEK PASSWORD LAMA
        Dim rsHakAksesCustom As String = HakAksesCustom(0, 1, "Update Password", userid)
        If Len(rsHakAksesCustom) <> 0 Then
            result(2) = rsHakAksesCustom : GoTo selesai
        End If
        'END OF CEK PASSWORD LAMA ==================================================

        'SIMPAN KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Try
            sql = "Update M0_User set upassword  = '" & FixQuotes(newpass) & "' where ukode = '" & username & "'"
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
            result(4) = userid

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
    Public Function M0_UserSearch(ByVal param As String) As String
        'M0_UserSearch --------------------------------------------------------
        'userid, ukode, unama, upassword, ukontak, ucabang, ulokasi, 
        'ugudang, ukota, ugambar, uaktif, utglexpired, ulevel, ugrup, 
        'ubahasa, udefaultview

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

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M0_User", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("userid"), ""), sptField,
                     FxDB(dr("ukode"), ""), sptField,
                     FxDB(dr("unama"), ""), sptField,
                     FxDB(dr("upassword"), ""), sptField,
                     FxDB(dr("ukontak"), ""), sptField,
                     FxDB(dr("ucabang"), ""), sptField,
                     FxDB(dr("ulokasi"), ""), sptField,
                     FxDB(dr("ugudang"), ""), sptField,
                     FxDB(dr("ukota"), ""), sptField,
                     FxDB(dr("ugambar"), ""), sptField,
                     FxDB(dr("uaktif"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("utglexpired"), ""), formatTgl), sptField,
                     FxDB(dr("ulevel"), 0), sptField,
                     FxDB(dr("ugrup"), 0), sptField,
                     FxDB(dr("ubahasa"), ""), sptField,
                     FxDB(dr("udefaultview"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "User data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("userid, ukode, unama, upassword, ukontak, ucabang, ulokasi, ugudang, ukota, ugambar, uaktif, utglexpired, ulevel, ugrup, ubahasa, udefaultview"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M0_UserDownload(ByVal param As String) As String
        'M0_UserDownload --------------------------------------------------------
        'Utama
        'userid, ukode, unama, upassword, ukontak, ucabang, ulokasi, 
        'ugudang, ukota, ugambar, uaktif, utglexpired, ulevel, ugrup, 
        'ubahasa, udefaultview

        'Detail
        'userid, role

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = "", detail As String = ""

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

        dt = AmbilData("aplikasi1-M0_User", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("userid"), ""), sptField,
                     FxDB(dr("ukode"), ""), sptField,
                     FxDB(dr("unama"), ""), sptField,
                     FxDB(dr("upassword"), ""), sptField,
                     FxDB(dr("ukontak"), ""), sptField,
                     FxDB(dr("ucabang"), ""), sptField,
                     FxDB(dr("ulokasi"), ""), sptField,
                     FxDB(dr("ugudang"), ""), sptField,
                     FxDB(dr("ukota"), ""), sptField,
                     FxDB(dr("ugambar"), ""), sptField,
                     FxDB(dr("uaktif"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("utglexpired"), ""), formatTgl), sptField,
                     FxDB(dr("ulevel"), 0), sptField,
                     FxDB(dr("ugrup"), 0), sptField,
                     FxDB(dr("ubahasa"), ""), sptField,
                     FxDB(dr("udefaultview"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)


            'AMBIL DATA DETAIL
            sql = "SELECT ur.userid, ur.role FROM m0_user_role ur JOIN m0_user u ON ur.userid = u.userid"

            Dim dtdetail As New DataTable
            dtdetail = AmbilData("aplikasi1-M0_User_Role", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtdetail.Rows
                detail = String.Concat(detail,
                     FxDB(dr("userid"), ""), sptField,
                     FxDB(dr("role"), ""), sptRow)
            Next
            If detail.Length > 0 Then detail = detail.Substring(0, detail.Length - sptRow.Length) Else detail = detail


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
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search, sptSubParam, detail)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("userid, ukode, unama, upassword, ukontak, ucabang, ulokasi, ugudang, ukota, ugambar, uaktif, utglexpired, ulevel, ugrup, ubahasa, udefaultview" & sptSubParam & "userid, role"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M0_UserImport(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataRowUtama(), dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean

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

        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'userid(0) As Integer, ukode(1) As String, unama(2) As String, upassword(3) As String, ukontak(4) As Integer, 
        'ucabang(5) As String, ulokasi(6) As String, ugudang(7) As String, ukota(8) As String, ugambar(9) As String, 
        'uaktif(10) As Integer, utglexpired(11) As Date, ulevel(12) As Integer, ugrup(13) As Integer, ubahasa(14) As String, 
        'udefaultview(15) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'userid, ukode, unama, upassword, ukontak, ucabang, ulokasi, 
        'ugudang, ukota, ugambar, uaktif, utglexpired, ulevel, ugrup, 
        'ubahasa, udefaultview


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptRow)    'SPLIT PARAMETER DATA UTAMA
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "userid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ukode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "unama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "upassword", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ukontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ucabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ulokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ugudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ukota", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ugambar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "uaktif", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "utglexpired", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ulevel", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ugrup", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ubahasa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "udefaultview", AsEnumTypeData.AsInt64)

        Dim JmlDtUtama As Integer = dataUtama.Length
        For i = 1 To JmlDtUtama
            'SPLIT DATA UTAMA
            dataRowUtama = dataUtama(i - 1).Split(sptField)

            'CEK ARRAY DATA UTAMA
            If (dataRowUtama.Length <> 16) Then
                result(2) = "Main Row : " & i & " - Invalid main transaction data parameter." : GoTo selesai
            End If

            'VALIDASI TIPE DATA UTAMA ==========================================================
            'userid(0) As Integer
            If (IsNumeric(dataRowUtama(0)) = False) Then
                result(2) = "Main Row : " & i & " - userid required numeric." : GoTo selesai
            End If
            'ukontak(4) As Integer
            If (IsNumeric(dataRowUtama(4)) = False) Then
                result(2) = "Main Row : " & i & " - ukontak required numeric." : GoTo selesai
            End If
            'uaktif(10) As Integer
            If (IsNumeric(dataRowUtama(10)) = False) Then
                result(2) = "Main Row : " & i & " - uaktif required numeric." : GoTo selesai
            End If
            'utglexpired(11) As Date
            If (IsDate(dataRowUtama(11)) = False) Then
                result(2) = "Main Row : " & i & " - utglexpired required date." : GoTo selesai
            End If
            'ulevel(12) As Integer
            If (IsNumeric(dataRowUtama(12)) = False) Then
                result(2) = "Main Row : " & i & " - ulevel required numeric." : GoTo selesai
            End If
            'ugrup(13) As Integer
            If (IsNumeric(dataRowUtama(13)) = False) Then
                result(2) = "Main Row : " & i & " - ugrup required numeric." : GoTo selesai
            End If
            'udefaultview(15) As Integer
            If (IsNumeric(dataRowUtama(15)) = False) Then
                result(2) = "Main Row : " & i & " - udefaultview required numeric." : GoTo selesai
            End If

            'END OF VALIDASI TIPE DATA UTAMA ===================================================

            'VALIDASI DATA UTAMA =======================================================
            'ukode(1) As String
            If Len(dataRowUtama(1)) = 0 Then
                result(2) = "Main Row : " & i & " - ukode can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(1)) > 25 Then
                result(2) = "Main Row : " & i & " - ukode should not be more than 25 character." : GoTo selesai
            End If

            'unama(2) As String
            If Len(dataRowUtama(2)) = 0 Then
                result(2) = "Main Row : " & i & " - unama can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(2)) > 100 Then
                result(2) = "Main Row : " & i & " - unama should not be more than 100 character." : GoTo selesai
            End If

            'upassword(3) As String
            If Len(dataRowUtama(3)) = 0 Then
                result(2) = "Main Row : " & i & " - upassword can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(3)) > 250 Then
                result(2) = "Main Row : " & i & " - upassword should not be more than 250 character." : GoTo selesai
            End If

            'ubahasa(14) As String
            If Len(dataRowUtama(14)) = 0 Then
                result(2) = "Main Row : " & i & " - ubahasa can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(14)) > 25 Then
                result(2) = "Main Row : " & i & " - ubahasa should not be more than 25 character." : GoTo selesai
            End If
            'END OF VALIDASI DATA UTAMA ================================================


            If AsDataTableTambahData(dtutama, "userid~ukode~unama~upassword~ukontak~ucabang~ulokasi~ugudang~ukota~ugambar~uaktif~utglexpired~ulevel~ugrup~ubahasa~udefaultview", dataRowUtama(0) & "~" & dataRowUtama(1) & "~" & dataRowUtama(2) & "~" & dataRowUtama(3) & "~" & dataRowUtama(4) & "~" & dataRowUtama(5) & "~" & dataRowUtama(6) & "~" & dataRowUtama(7) & "~" & dataRowUtama(8) & "~" & dataRowUtama(9) & "~" & dataRowUtama(10) & "~" & dataRowUtama(11) & "~" & dataRowUtama(12) & "~" & dataRowUtama(13) & "~" & dataRowUtama(14) & "~" & dataRowUtama(15)) = False Then
                result(2) = "Main Row : " & i & " - Insert into main datatable failed." : GoTo selesai
            End If

        Next


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'userid(0) As Integer, role(1) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'userid, role

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "userid", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "role", AsEnumTypeData.AsString)


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 2) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'userid(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - userid required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'role(1) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - role can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - role should not be more than 25 character." : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "userid~role", dataRowDetail(0) & "~" & dataRowDetail(1)) = False Then
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
            'Proses utama
            If (dtutama.Rows.Count > 0) Then
                'Hapus user
                If (isUpdate) Then
                    sql = "Delete from M0_User"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Hapus role user
                If (isUpdate) Then
                    sql = "Delete from M0_User_Role"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                'Proses utama
                Dim strValue1 As New StringBuilder
                For Each dr1 As DataRow In dtutama.Rows
                    strValue1.Append(IIf(Len(strValue1.ToString) = 0, "", ", "))
                    strValue1.Append("('" & FixQuotes(dr1("userid")) & "', '" & FixQuotes(dr1("ukode")) & "', '" & FixQuotes(dr1("unama")) & "', '" & FixQuotes(dr1("upassword")) & "', " & dr1("ukontak") & ", '" & FixQuotes(dr1("ucabang")) & "', '" & FixQuotes(dr1("ulokasi")) & "', '" & FixQuotes(dr1("ugudang")) & "', '" & FixQuotes(dr1("ukota")) & "', '" & FixQuotes(dr1("ugambar")) & "', " & dr1("uaktif") & ", '" & FixQuotes(AsFormatTanggal(dr1("utglexpired"))) & "', " & dr1("ulevel") & ", " & dr1("ugrup") & ", '" & FixQuotes(dr1("ubahasa")) & "', " & dr1("udefaultview") & ")")
                Next
                sql = "Insert into M0_User(userid, ukode, unama, upassword, ukontak, ucabang, ulokasi, ugudang, ukota, ugambar, uaktif, utglexpired, ulevel, ugrup, ubahasa, udefaultview) values" & strValue1.ToString & ""
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()


                'Proses detail
                If (dtdetail.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("userid")) & "', '" & FixQuotes(dr1("role")) & "')")
                    Next
                    sql = "Insert into M0_User_Role(userid, role) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Else
                    result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                Trans.Commit()  '*** Commit Transaction ***'
                result(1) = 1
                result(2) = notransaksi
                result(3) = 0
                result(4) = result(4)

            Else
                result(2) = "#1. Main transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

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
        strResultData = ""
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)
        Return wsResult
    End Function

End Class
