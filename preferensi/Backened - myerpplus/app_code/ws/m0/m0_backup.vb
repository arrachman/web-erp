Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
Imports System.Net.Sockets

'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m0_backup_data
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""
    Public AppCode As String
    Public PortServerCetak As Integer = 423

    <WebMethod()>
    Public Function m0_backup_dataSimpan(ByVal param As String) As String
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        'On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""
        Dim paramSearch As String = "", hasilSearch As New RsHasilWsSearch
        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String
        Dim Sql As String = ""

        Dim pg1 As New RsPaging
        Dim dt As New DataTable

        'SET DEFAULT 
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPLIT PARAM
        paramSplit = param.Split(sptParam)
        pagingSplit = paramSplit(2).Split(sptSubParam)
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
        'END OF VALIDASI WEBSITEACCESSKEY ================================================
        Try

            Sql = "SELECT id, status FROM m0_backup ORDER BY id DESC LIMIT 1"

            dt = AmbilData("aplikasi1-", "", "", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , Sql) ' Ambil data ke databases

            If dt.Rows.Count > 0 Then
                If dt.Rows(0)("status") = 0 Or dt.Rows(0)("status") = 1 Then
                    result(2) = "Waiting for backup is now done" : GoTo selesai
                End If
            End If

            Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)
            With New MySql.Data.MySqlClient.MySqlCommand()
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = "INSERT INTO m0_backup (id, namafile, status, tglmulai, keterangan) VALUES (0, 'Auto', 0, NOW(), '')"
                .ExecuteNonQuery()
            End With

            Sql = "SELECT id FROM m0_backup WHERE status = 0 ORDER BY id DESC LIMIT 1"

            dt = AmbilData("aplikasi1-", "", "", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , Sql) ' Ambil data ke databases
            pg1 = pg1

            If dt.Rows.Count > 0 Then
                Dim strValue As String = ""
                strValue = String.Concat("Backup", sptField, dt.Rows(0)("id"), sptField, "backup", sptField, "", sptField, userid, sptField, AppCode, sptField, HttpContext.Current.Server.MapPath("~")).Replace(sptField, "<^>") + "$$$"


                Dim clientSocket As New System.Net.Sockets.TcpClient()
                clientSocket = New System.Net.Sockets.TcpClient()
                clientSocket.Connect("127.0.0.1", PortServerCetak)
                Dim serverStream As NetworkStream = clientSocket.GetStream()
                Dim outStream As Byte() = System.Text.Encoding.ASCII.GetBytes(strValue)
                serverStream.Write(outStream, 0, outStream.Length)

                serverStream.Flush()
                serverStream.Close()
                serverStream.Dispose()
                clientSocket.Close()
            Else
                result(2) = "Backup data not found." : Trans.Rollback() : GoTo selesai
            End If



        Catch ex As Exception
            result(2) = Err.Description : GoTo selesai
        End Try

        Trans.Commit()
        result(1) = 1
        Return m0_backup_dataSearch(param)
selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam)

        Return wsResult
    End Function

    <WebMethod()>
    Public Function m0_backup_dataSearch(ByVal param As String) As String
        'M0_SetFileLibrary --------------------------------------------------------
        'namaFile, content
        '===> namaFile : namaFolder/namaFile.extensi
        '===> namaFolder : "grid" atau "report"

        'On Error GoTo selesai
        Dim searchmap As String = "" ', paramSearch As String, hasilSearch As New RsHasilWsSearch
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = "", search2 As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String
        Dim dataSplit() As String

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
        pagingSplit = paramSplit(2).Split(sptSubParam)
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If
        'PANGGIL QUERY
        sql = "SELECT id, namafile, status, tglmulai, tglselesai, keterangan FROM m0_backup"

        dt = AmbilData("aplikasi1-", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        search = ""
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("id"), 0), sptField,
                     FxDB(dr("namafile"), ""), sptField,
                     FxDB(dr("status"), 0), sptField,
                     FxDB(dr("tglmulai"), ""), sptField,
                     FxDB(dr("tglselesai"), ""), sptField,
                     FxDB(dr("keterangan"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)
        Else
            result(2) = "Backup data not found." : GoTo selesai
        End If

        result(1) = 1
        resultPaging(0) = Math.Abs(Val(pg1.isPaging))
        resultPaging(1) = Math.Abs(Val(pg1.isNext))
        resultPaging(2) = Math.Abs(Val(pg1.isPrev))
        resultPaging(3) = pg1.countPage
        resultPaging(4) = pg1.countRow

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1))
        wsResult = String.Concat(wsResult, sptParam, search, sptParam, ReplaceMapping("id, namafile, status, tglmulai, tglselesai, keterangan"))

        Return wsResult
    End Function

    '    <WebMethod()>
    '    Public Function m0_backup_dataRestore(ByVal param As String) As String
    '        'M0_SetFileLibrary --------------------------------------------------------
    '        'namaFile, content
    '        '===> namaFile : namaFolder/namaFile.extensi
    '        '===> namaFolder : "grid" atau "report"

    '        'On Error GoTo selesai
    '        Dim searchmap As String = "", hasilSearch As New RsHasilWsSearch
    '        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = "", search2 As String = ""

    '        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
    '        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

    '        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
    '        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

    '        Dim wsResult As String = ""
    '        Dim strResult, strResultPaging As String
    '        Dim dataSplit() As String

    '        Dim sql As String = ""

    '        Dim pg1 As New RsPaging
    '        Dim Filter As String = "", Sorting As String = ""
    '        Dim dt As New DataTable

    '        'SET DEFAULT 
    '        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
    '        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

    '        'SET DEFAULT PAGING
    '        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

    '        'VALIDASI PARAMETER GLOBAL =========================================================
    '        'SPLIT PARAM
    '        paramSplit = param.Split(sptParam)

    '        'CEK ARRAY PARAM
    '        If (paramSplit.Length <> 6) Then
    '            result(2) = "Invalid parameter." : GoTo selesai
    '        End If
    '        'END OF VALIDASI PARAMETER GLOBAL ==================================================

    '        'VALIDASI WEBSITEACCESSKEY =========================================================
    '        If Len(paramSplit(0)) = 0 Then
    '            result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
    '        End If

    '        'Cek apakah WebsiteAccessKey valid
    '        Dim ClsValidKey As New ClsSecurity
    '        Dim validKey As RsValidKey
    '        validKey = ValidateKey(paramSplit(0))
    '        If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

    '        '///Validasi Hak akses. Cek ModuleID dan MenuID
    '        If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
    '            result(2) = "Access denied for insert/update data"
    '        End If
    '        pagingSplit = paramSplit(2).Split(sptSubParam)
    '        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


    '        'VALIDASI DAN SET DATA =============================================================
    '        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

    '        If (pagingSplit(2).Length > 0) Then
    '            Filter = pagingSplit(2)
    '            '#Taruh fungsi replace disini...
    '        End If
    '        If (pagingSplit(3).Length > 0) Then
    '            Sorting = pagingSplit(3)
    '            '#Taruh fungsi replace disini...
    '        End If

    '        sql = "SELECT id, status FROM m0_backup ORDER BY id DESC LIMIT 1"

    '        dt = AmbilData("aplikasi1-", "", "", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases

    '        If dt.Rows.Count > 0 Then
    '            If dt.Rows(0)("status") = 0 Or dt.Rows(0)("status") = 1 Then
    '                result(2) = "Waiting for backup is now done" : GoTo selesai
    '            End If
    '        End If

    '        'PROSES LOGOUT USER =====================================================
    '        'USER YG LOGIN DILOGOUT AGAR SETTING POS TERLOAD ULANG
    '        sql = "SELECT ul.ulid, u.userid FROM m0_userlogin ul JOIN m0_user u ON ul.uluser = u.userid JOIN m1_location l ON u.ulokasi = l.lkode"
    '        Dim dtUser As DataTable = AsDataTableAmbilDariDB(sql)
    '        If dtUser.Rows.Count > 0 Then
    '            Using WsLogout As New m0_login
    '                Dim rsLogout As String = ""
    '                For Each drUser As DataRow In dtUser.Rows
    '                    'LOGOUT USER SESUAI KATEGORI POS YG DISETTING
    '                    rsLogout = WsLogout.M0_Logout(drUser("ulid") & "★M0_Logout★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & drUser("userid") & "★0★" & AppCode & "")
    '                Next
    '            End Using
    '        End If
    '        'END OF PROSES LOGOUT USER ==============================================

    '        Dim myConnx As MySql.Data.MySqlClient.MySqlConnection
    '        Dim lokasiconfig As String = HttpContext.Current.Server.MapPath("~/") + "\report\config\config"
    '        myConnx = New MySql.Data.MySqlClient.MySqlConnection(Encoding.UTF8.GetString(Convert.FromBase64String(File.ReadAllText(lokasiconfig))))
    '        myConnx.Open()

    '        With New MySql.Data.MySqlClient.MySqlCommand()
    '            .Connection = myConnx
    '            .CommandType = CommandType.Text
    '            .CommandText = "update pelanggan set aktif = 2, catatan_tdk_aktif = 'proses restore mulai' where kode = '" + AppCode + "'"
    '            .ExecuteNonQuery()
    '        End With
    '        myConnx.Close()

    '        Dim strValue As String = ""
    '        strValue = String.Concat("Backup", sptField, paramSplit(5), sptField, "restore", sptField, "", sptField, userid, sptField, AppCode, sptField, HttpContext.Current.Server.MapPath("~")).Replace(sptField, "<^>") + "$$$"

    '        Dim clientSocket As New System.Net.Sockets.TcpClient()
    '        clientSocket = New System.Net.Sockets.TcpClient()
    '        clientSocket.Connect("127.0.0.1", PortServerCetak)
    '        Dim serverStream As NetworkStream = clientSocket.GetStream()
    '        Dim outStream As Byte() = System.Text.Encoding.ASCII.GetBytes(strValue)
    '        serverStream.Write(outStream, 0, outStream.Length)

    '        serverStream.Flush()
    '        serverStream.Close()
    '        serverStream.Dispose()
    '        clientSocket.Close()

    '        result(1) = 0
    '        result(2) = "Invalid Website Access Key."
    '        resultPaging(0) = Math.Abs(Val(pg1.isPaging))
    '        resultPaging(1) = Math.Abs(Val(pg1.isNext))
    '        resultPaging(2) = Math.Abs(Val(pg1.isPrev))
    '        resultPaging(3) = pg1.countPage
    '        resultPaging(4) = pg1.countRow

    'selesai:
    '        If result(1) = 0 Then
    '            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
    '        End If

    '        strResult = String.Join(sptSubParam, result)
    '        strResultPaging = String.Join(sptSubParam, resultPaging)
    '        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1))
    '        wsResult = String.Concat(wsResult, sptParam, search, sptParam, ReplaceMapping("id, namafile, status, tglmulai, tglselesai, keterangan"))

    '        Return wsResult
    '    End Function

End Class