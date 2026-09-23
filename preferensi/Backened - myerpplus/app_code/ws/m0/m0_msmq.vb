Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m0_msmq
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M0_MsmqSimpan(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama() As String

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

        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'id(0) As String, module(1) As Integer, menu(2) As Integer, item(3) As Integer, filter(4) As String, 
        'sort(5) As String, fileformat(6) As Integer, print(7) As Integer, param1(8) As String, param2(9) As String, 
        'param3(10) As String, param4(11) As String, param5(12) As String, jmldata(13) As Integer, progress(14) As Integer, 
        'userid(15) As Integer, tglantrian(16) As DateTime, tglselesai(17) As DateTime, pesan(18) As String, groupby(19) As String,
        'sumber(20) As String, idtransaksi(21) As Integer, progresspersen(22) As Double, filename(23) As String

        'MAPPING BUAT FLEX --------------------------------------------------------
        'id, module, menu, item, filter, sort, fileformat, 
        'print, param1, param2, param3, param4, param5, jmldata, 
        'progress, userid, tglantrian, tglselesai, pesan, groupby,
        'sumber, idtransaksi, progresspersen, filename


        'VALIDASI DAN SET DATA UTAMA =================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 24) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ==========================================


        'VALIDASI TIPE DATA ==========================================================
        'module(1) As Integer
        If (IsNumeric(dataUtama(1)) = False) Then
            result(2) = "module required numeric." : GoTo selesai
        End If
        'menu(2) As Integer
        If (IsNumeric(dataUtama(2)) = False) Then
            result(2) = "menu required numeric." : GoTo selesai
        End If
        'item(3) As Integer
        If (IsNumeric(dataUtama(3)) = False) Then
            result(2) = "item required numeric." : GoTo selesai
        End If
        'fileformat(6) As Integer
        If (IsNumeric(dataUtama(6)) = False) Then
            result(2) = "fileformat required numeric." : GoTo selesai
        End If
        'print(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "print required numeric." : GoTo selesai
        End If
        'jmldata(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "jmldata required numeric." : GoTo selesai
        End If
        'progress(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "progress required numeric." : GoTo selesai
        End If
        'userid(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If
        'idtransaksi(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
        End If
        'progresspersen(22) As Double
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "progresspersen required numeric." : GoTo selesai
        End If
        ''tglantrian(16) As DateTime
        'If (IsDate(dataUtama(16)) = False) Then
        '    result(2) = "tglantrian required date." : GoTo selesai
        'End If
        ''tglselesai(17) As DateTime
        'If (IsDate(dataUtama(17)) = False) Then
        '    result(2) = "tglselesai required date." : GoTo selesai
        'End If
        'END OF VALIDASI TIPE DATA ===================================================

        'VALIDASI DATA ===============================================================
        'id(0) As String
        If Len(dataUtama(0)) = 0 Then
            result(2) = "id can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 50 Then
            result(2) = "id should not be more than 50 character." : GoTo selesai
        End If

        ''tglantrian(16) As DateTime
        'If Len(dataUtama(16)) = 0 Then
        '    result(2) = "tglantrian can't be empty" : GoTo selesai
        'End If

        ''tglselesai(17) As DateTime
        'If Len(dataUtama(17)) = 0 Then
        '    result(2) = "tglselesai can't be empty" : GoTo selesai
        'End If

        'filename(23) As String
        If Len(dataUtama(23)) = 0 Then
            result(2) = "filename can't be empty" : GoTo selesai
        End If
        'END OF VALIDASI DATA ========================================================


        'SIMPAN KE DATABASE ==========================================================
        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        '*** Start Transaction ***'  
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0

        Try
            If isUpdate Then
                'JIKA UPDATE CEK JML ROW PADA DATABASE
                'dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(id) FROM M0_Msmq WHERE id ='" & dataUtama(0) & "'")
                'If dtupdate.Rows.Count > 0 Then
                '    rowUpdate = dtupdate.Rows(0)(0)
                'Else
                '    result(2) = "Id not found." : Trans.Rollback() : GoTo selesai
                'End If
                rowUpdate = 1
                If (rowUpdate > 0) Then
                    sql = "Update M0_Msmq set progress  = " & dataUtama(14) & ", tglselesai  = NOW(), pesan  = '" & FixQuotes(dataUtama(18)) & "', progresspersen = '" & FixDouble(dataUtama(22)) & "', filename  = '" & FixQuotes(dataUtama(23)) & "' where id = '" & dataUtama(0) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Else
                    result(2) = "Data not found." : Trans.Rollback() : GoTo selesai
                End If

            Else

                ''CUSTOM CETAKAN KE -----------------
                ''Jika modul, menu, dan item tertentu maka update kolom cetakanke pada transaksi
                'If dataUtama(1) = 5 And dataUtama(2) = 10 And dataUtama(3) = 5 Then
                '    sql = "  INSERT INTO m5_print "
                '    sql &= " (SELECT si.sisumber as pisumber, si.siid as pidtransaksi, " & dataUtama(1) & " as pmodul, " & dataUtama(2) & " as pmenu, " & dataUtama(3) & " as pitem, 1 as pcetakanke FROM m5_si si "
                '    sql &= IIf(Len(dataUtama(4)) > 0, " WHERE " & dataUtama(4), "")
                '    sql &= " ) "
                '    sql &= " ON DUPLICATE KEY UPDATE pcetakanke = pcetakanke + 1 "
                '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '    With objCmd
                '        .Connection = myConn
                '        .Transaction = Trans
                '        .CommandType = CommandType.Text
                '        .CommandText = sql
                '    End With
                '    objCmd.ExecuteNonQuery()

                '    'sql = "UPDATE m5_si SET sicetakanke = sicetakanke + 1 " & IIf(Len(dataUtama(4)) > 0, " WHERE " & dataUtama(4), "")
                '    'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '    'With objCmd
                '    '    .Connection = myConn
                '    '    .Transaction = Trans
                '    '    .CommandType = CommandType.Text
                '    '    .CommandText = sql
                '    'End With
                '    'objCmd.ExecuteNonQuery()
                'End If
                ''END OF CUSTOM CETAKAN KE ----------

                sql = "Insert into M0_Msmq (id, module, menu, item, filter, sort, fileformat, print, param1, param2, param3, param4, param5, jmldata, progress, userid, tglantrian, tglselesai, pesan, groupby, sumber, idtransaksi, progresspersen, filename) values('" & FixQuotes(dataUtama(0)) & "', " & dataUtama(1) & ", " & dataUtama(2) & ", " & dataUtama(3) & ", '" & FixQuotes(dataUtama(4)) & "', '" & FixQuotes(dataUtama(5)) & "', " & dataUtama(6) & ", " & dataUtama(7) & ", '" & FixQuotes(dataUtama(8)) & "', '" & FixQuotes(dataUtama(9)) & "', '" & FixQuotes(dataUtama(10)) & "', '" & FixQuotes(dataUtama(11)) & "', '" & FixQuotes(dataUtama(12)) & "', " & dataUtama(13) & ", " & dataUtama(14) & ", " & dataUtama(15) & ", NOW(), '1971-01-01 00:00:00', '" & FixQuotes(dataUtama(18)) & "', '" & FixQuotes(dataUtama(19)) & "', '" & FixQuotes(dataUtama(20)) & "', '" & FixDouble(dataUtama(21)) & "', '" & FixDouble(dataUtama(22)) & "', '" & FixQuotes(dataUtama(23)) & "')"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'MSMQ ANTRIAN
                If dataSplit(1).ToString.Length > 0 Then
                    Dim hasilMsmq As String = ""
                    hasilMsmq = SendMsmqReport(dirMsmq, dataSplit(1))
                    If Len(hasilMsmq) > 0 Then
                        result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
                    End If
                Else
                    result(2) = "MSMQ parameter not found." : Trans.Rollback() : GoTo selesai
                End If

            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            ''AMBIL DATA =============================================================
            'Dim paramSearch As String = M1_AreaSearch(PostWsSearch(paramSplit(0), "M1_AreaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
            'Dim hasilSearch As New RsHasilWsSearch
            'hasilSearch = GetWsSearch(paramSearch)

            'result(1) = hasilSearch.success
            'result(2) = hasilSearch.errmessage

            'resultPaging(0) = hasilSearch.isPaging
            'resultPaging(1) = hasilSearch.isNext
            'resultPaging(2) = hasilSearch.isPrevious
            'resultPaging(3) = hasilSearch.countPage
            'resultPaging(4) = hasilSearch.countRow

            'search = hasilSearch.data
            ''END OF AMBIL DATA ======================================================

        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***'  
            result(1) = 0
            result(2) = "Transaction Rollback : " & ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try

        objCmd = Nothing
        myConn.Close()
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
    Public Function M0_MsmqGetdataById(ByVal param As String) As String

        'M0_MsmqGetdataById Utama --------------------------------------------------------
        'progress, pesan, progresspersen

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

        Dim utama As String = "", detail As String = "", idtransaksi As String = ""

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
        'If ClsValidKey.ApaBisaAkses(1, 1, 3) = False Then
        '    result(2) = "Access denied for get data"
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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (Len(paramSplit(3)) = 0) Then
            result(2) = "ID MSMQ can't be empty." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M0_Msmq", "id='" & idtransaksi & "'", Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , )

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("progress"), 0), sptField,
                     FxDB(drutama("pesan"), ""), sptField,
                     FxDB(drutama("progresspersen"), ""))

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "MSMQ data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, detail)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("progress, pesan, progresspersen"))

        Return wsResult
    End Function

End Class