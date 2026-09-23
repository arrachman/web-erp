Imports System.Web
Imports System.Web.Services
'Imports System.Web.Services.Protocols
'Imports System.Web.Script.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization

'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m1_project
    Inherits System.Web.Services.WebService
    Public ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi

    <WebMethod()>
    Public Function M1_ProjectSimpan(ByVal param As String) As String

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
        Dim search As String = ""
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
        'pkode(0) As String, pnama(1) As String, pkategori(2) As String, paktif(3) As Integer, ptglorder(4) As Date, 
        'ptglmulairencana(5) As Date, ptglmulairealisasi(6) As Date, ptglselesairencana(7) As Date, ptglselesairealisasi(8) As Date, pprioritas(9) As String, 
        'pselesai(10) As Double, pkontak(11) As Integer, pkontakperson(12) As String, ppimpinanproyek(13) As Integer, pdivisi(14) As String, 
        'pketerangan(15) As String, ptglkontrak(16) As Date, pnokontrak(17) As String, pnilaikontrak(18) As Double, psubdari(19) As Integer, 
        'pparent(20) As String, plevel(21) As Integer, pcustom1(22) As String, pcustom2(23) As String, pcustom3(24) As String, 
        'pcustom4(25) As String, pcustom5(26) As String, pinputuser(27) As Integer, pinputtgl(28) As DateTime, pmodifikasiuser(29) As Integer, 
        'pmodifikasitgl(30) As DateTime, pgd(31) As String, pstatus(32) As String

        'MAPPING BUAT FLEX --------------------------------------------------------
        'pkode, pnama, pkategori, paktif, ptglorder, ptglmulairencana, ptglmulairealisasi, 
        'ptglselesairencana, ptglselesairealisasi, pprioritas, pselesai, pkontak, pkontakperson, ppimpinanproyek, 
        'pdivisi, pketerangan, ptglkontrak, pnokontrak, pnilaikontrak, psubdari, pparent, 
        'plevel, pcustom1, pcustom2, pcustom3, pcustom4, pcustom5, pinputuser, 
        'pinputtgl, pmodifikasiuser, pmodifikasitgl, pgd, pstatus

        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 33) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================

        'VALIDASI TIPE DATA ==========================================================
        'paktif(3) As Integer
        If (IsNumeric(dataUtama(3)) = False) Then
            result(2) = "paktif required numeric." : GoTo selesai
        End If
        'ptglorder(4) As Date
        If (IsDate(dataUtama(4)) = False) Then
            result(2) = "ptglorder required date." : GoTo selesai
        End If
        'ptglmulairencana(5) As Date
        If (IsDate(dataUtama(5)) = False) Then
            result(2) = "ptglmulairencana required date." : GoTo selesai
        End If
        'ptglmulairealisasi(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "ptglmulairealisasi required date." : GoTo selesai
        End If
        'ptglselesairencana(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "ptglselesairencana required date." : GoTo selesai
        End If
        'ptglselesairealisasi(8) As Date
        If (IsDate(dataUtama(8)) = False) Then
            result(2) = "ptglselesairealisasi required date." : GoTo selesai
        End If
        'pselesai(10) As Double
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "pselesai required numeric." : GoTo selesai
        End If
        'pkontak(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "pkontak required numeric." : GoTo selesai
        End If
        'ppimpinanproyek(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "ppimpinanproyek required numeric." : GoTo selesai
        End If
        'ptglkontrak(16) As Date
        If (IsDate(dataUtama(16)) = False) Then
            result(2) = "ptglkontrak required date." : GoTo selesai
        End If
        'pnilaikontrak(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "pnilaikontrak required numeric." : GoTo selesai
        End If
        'psubdari(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "psubdari required numeric." : GoTo selesai
        End If
        'plevel(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "plevel required numeric." : GoTo selesai
        End If
        'pinputuser(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "pinputuser required numeric." : GoTo selesai
        End If
        'pinputtgl(28) As DateTime
        If (IsDate(dataUtama(28)) = False) Then
            result(2) = "pinputtgl required date." : GoTo selesai
        End If
        'pmodifikasiuser(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "pmodifikasiuser required numeric." : GoTo selesai
        End If
        'pmodifikasitgl(30) As DateTime
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "pmodifikasitgl required date." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA ===================================================

        'VALIDASI DATA ===============================================================
        'pkode(0) As String
        If Len(dataUtama(0)) = 0 Then
            result(2) = "pkode can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 25 Then
            result(2) = "pkode should not be more than 25 character." : GoTo selesai
        End If

        'pnama(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "pnama can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 100 Then
            result(2) = "pnama should not be more than 100 character." : GoTo selesai
        End If

        'pinputtgl(28) As DateTime
        If Len(dataUtama(28)) = 0 Then
            result(2) = "pinputtgl can't be empty" : GoTo selesai
        End If

        'pmodifikasitgl(30) As DateTime
        If Len(dataUtama(30)) = 0 Then
            result(2) = "pmodifikasitgl can't be empty" : GoTo selesai
        End If

        'pgd(31) As String
        If Len(dataUtama(31)) > 2 Then
            result(2) = "pgd should not be more than 2 character." : GoTo selesai
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
                dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(pkode), pnilaikontrak FROM M1_Project WHERE pkode ='" & dataUtama(0) & "'")
                rowUpdate = dtupdate.Rows(0)(0)

                If (rowUpdate > 0) Then
                    'SIMPAN HISTORY ========================
                    Dim SimpanHistory As New m1_project_history
                    Dim projectSimpanHistory As String = SimpanHistory.M1_Project_HistorySimpan("" & paramSplit(0) & "★M1_Project_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(dataUtama(0)) & "")
                    Dim projectSplit() As String = projectSimpanHistory.Split(sptParam)
                    Dim projectSplitResult() As String = projectSplit(0).Split(sptSubParam)
                    'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                    If (projectSplitResult(1) = 0) Then
                        result(2) = "Insert history failed : " & projectSplitResult(2) : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF SIMPAN HISTORY ==================

                    If dtupdate.Rows(0)("pnilaikontrak") <> 0 And dtupdate.Rows(0)("pnilaikontrak") <> dataUtama(18) Then
                        Dim vHAC As String = HakAksesCustom(1, 4, "Project Value Revision", userid)
                        If Len(vHAC) <> 0 Then
                            result(2) = vHAC : Trans.Rollback() : GoTo selesai
                        End If
                    End If

                    sql = "Update M1_Project set pnama  = '" & FixQuotes(dataUtama(1)) & "', pkategori  = '" & FixQuotes(dataUtama(2)) & "', paktif  = " & dataUtama(3) & ", ptglorder  = '" & FixQuotes(AsFormatTanggal(dataUtama(4))) & "', ptglmulairencana  = '" & FixQuotes(AsFormatTanggal(dataUtama(5))) & "', ptglmulairealisasi  = '" & FixQuotes(AsFormatTanggal(dataUtama(6))) & "', ptglselesairencana  = '" & FixQuotes(AsFormatTanggal(dataUtama(7))) & "', ptglselesairealisasi  = '" & FixQuotes(AsFormatTanggal(dataUtama(8))) & "', pprioritas  = '" & FixQuotes(dataUtama(9)) & "', pselesai  = '" & FixDouble(dataUtama(10)) & "', pkontak  = " & dataUtama(11) & ", pkontakperson  = '" & FixQuotes(dataUtama(12)) & "', ppimpinanproyek  = " & dataUtama(13) & ", pdivisi  = '" & FixQuotes(dataUtama(14)) & "', pketerangan  = '" & FixQuotes(dataUtama(15)) & "', ptglkontrak  = '" & FixQuotes(AsFormatTanggal(dataUtama(16))) & "', pnokontrak  = '" & FixQuotes(dataUtama(17)) & "', pnilaikontrak  = '" & FixDouble(dataUtama(18)) & "', psubdari  = " & dataUtama(19) & ", pparent  = '" & FixQuotes(dataUtama(20)) & "', plevel  = " & dataUtama(21) & ", pcustom1  = '" & FixQuotes(dataUtama(22)) & "', pcustom2  = '" & FixQuotes(dataUtama(23)) & "', pcustom3  = '" & FixQuotes(dataUtama(24)) & "', pcustom4  = '" & FixQuotes(dataUtama(25)) & "', pcustom5  = '" & FixQuotes(dataUtama(26)) & "', pmodifikasiuser  = " & dataUtama(29) & ", pmodifikasitgl  = NOW(), pgd  = '" & FixQuotes(dataUtama(31)) & "', pstatus  = '" & FixQuotes(dataUtama(32)) & "' where pkode = '" & dataUtama(0) & "'"
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
                sql = "Insert into M1_Project (pkode, pnama, pkategori, paktif, ptglorder, ptglmulairencana, ptglmulairealisasi, ptglselesairencana, ptglselesairealisasi, pprioritas, pselesai, pkontak, pkontakperson, ppimpinanproyek, pdivisi, pketerangan, ptglkontrak, pnokontrak, pnilaikontrak, psubdari, pparent, plevel, pcustom1, pcustom2, pcustom3, pcustom4, pcustom5, pinputuser, pinputtgl, pmodifikasiuser, pmodifikasitgl, pgd, pstatus) values('" & FixQuotes(dataUtama(0)) & "', '" & FixQuotes(dataUtama(1)) & "', '" & FixQuotes(dataUtama(2)) & "', " & dataUtama(3) & ", '" & FixQuotes(AsFormatTanggal(dataUtama(4))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(5))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(6))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(7))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(8))) & "', '" & FixQuotes(dataUtama(9)) & "', '" & FixDouble(dataUtama(10)) & "', " & dataUtama(11) & ", '" & FixQuotes(dataUtama(12)) & "', " & dataUtama(13) & ", '" & FixQuotes(dataUtama(14)) & "', '" & FixQuotes(dataUtama(15)) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(16))) & "', '" & FixQuotes(dataUtama(17)) & "', '" & FixDouble(dataUtama(18)) & "', " & dataUtama(19) & ", '" & FixQuotes(dataUtama(20)) & "', " & dataUtama(21) & ", '" & FixQuotes(dataUtama(22)) & "', '" & FixQuotes(dataUtama(23)) & "', '" & FixQuotes(dataUtama(24)) & "', '" & FixQuotes(dataUtama(25)) & "', '" & FixQuotes(dataUtama(26)) & "', " & dataUtama(27) & ", NOW(), " & dataUtama(29) & ", '1971-01-01 00:00:00', '" & FixQuotes(dataUtama(31)) & "', '" & FixQuotes(dataUtama(32)) & "')"
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
            Dim paramSearch As String = M1_ProjectSearch(PostWsSearch(paramSplit(0), "M1_ProjectSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_ProjectDelete(ByVal param As String) As String

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
            result(2) = "pkode can't be empty." : GoTo selesai
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
            Dim paramTerkait As String = M1_ProjectTerkait(PostWsTerkait(paramSplit(0), "M1_ProjectTerkait", pagingSplit(0), pagingSplit(1), "", "", formatTgl, formatTglWaktu, idtransaksi))
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
            Dim SimpanHistory As New m1_project_history
            Dim projectSimpanHistory As String = SimpanHistory.M1_Project_HistorySimpan("" & paramSplit(0) & "★M1_Project_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(idtransaksi) & "")
            Dim projectSplit() As String = projectSimpanHistory.Split(sptParam)
            Dim projectSplitResult() As String = projectSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (projectSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & projectSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            'DELETE
            sql = "DELETE FROM M1_Project WHERE pkode = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M1_ProjectSearch(PostWsSearch(paramSplit(0), "M1_ProjectSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_ProjectSearch(ByVal param As String) As String
        'M1_ProjectSearch --------------------------------------------------------
        'pkode, pnama, pkategori, paktif, ptglorder, ptglmulairencana, ptglmulairealisasi, 
        'ptglselesairencana, ptglselesairealisasi, pprioritas, pselesai, pkontak, pkontakperson, ppimpinanproyek, 
        'pdivisi, pketerangan, ptglkontrak, pnokontrak, pnilaikontrak, psubdari, pparent, 
        'plevel, pcustom1, pcustom2, pcustom3, pcustom4, pcustom5, pinputuser, 
        'pinputtgl, pmodifikasiuser, pmodifikasitgl, pgd, pstatus, pkontakkode, pkontaknama, 
        'ppimpinanproyekkode, ppimpinanproyeknama, pdivisinama

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

        'BUAT QUERY
        sql = "select `p`.`pkode` AS `pkode`,`p`.`pnama` AS `pnama`,`p`.`pkategori` AS `pkategori`,`p`.`paktif` AS `paktif`,`p`.`ptglorder` AS `ptglorder`,`p`.`ptglmulairencana` AS `ptglmulairencana`,`p`.`ptglmulairealisasi` AS `ptglmulairealisasi`,`p`.`ptglselesairencana` AS `ptglselesairencana`,`p`.`ptglselesairealisasi` AS `ptglselesairealisasi`,`p`.`pprioritas` AS `pprioritas`,`p`.`pselesai` AS `pselesai`,`p`.`pkontak` AS `pkontak`,`p`.`pkontakperson` AS `pkontakperson`,`p`.`ppimpinanproyek` AS `ppimpinanproyek`,`p`.`pdivisi` AS `pdivisi`,`p`.`pketerangan` AS `pketerangan`,`p`.`ptglkontrak` AS `ptglkontrak`,`p`.`pnokontrak` AS `pnokontrak`,`p`.`pnilaikontrak` AS `pnilaikontrak`,`p`.`psubdari` AS `psubdari`,`p`.`pparent` AS `pparent`,`p`.`plevel` AS `plevel`,`p`.`pcustom1` AS `pcustom1`,`p`.`pcustom2` AS `pcustom2`,`p`.`pcustom3` AS `pcustom3`,`p`.`pcustom4` AS `pcustom4`,`p`.`pcustom5` AS `pcustom5`,`p`.`pinputuser` AS `pinputuser`,`p`.`pinputtgl` AS `pinputtgl`,`p`.`pmodifikasiuser` AS `pmodifikasiuser`,`p`.`pmodifikasitgl` AS `pmodifikasitgl`,`p`.`pgd` AS `pgd`,`p`.`pstatus` AS `pstatus`,`c1`.`kkode` AS `pkontakkode`,`c1`.`knama` AS `pkontaknama`,`c2`.`kkode` AS `ppimpinanproyekkode`,`c2`.`knama` AS `ppimpinanproyeknama`,`d`.`dnama` AS `pdivisinama` from (((`m1_project` `p` left join `m1_contact` `c1` on((`p`.`pkontak` = `c1`.`kid`))) left join `m1_contact` `c2` on((`p`.`ppimpinanproyek` = `c2`.`kid`))) left join `m1_division` `d` on((`p`.`pdivisi` = `d`.`dkode`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Project", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("pkode"), ""), sptField,
                     FxDB(dr("pnama"), ""), sptField,
                     FxDB(dr("pkategori"), ""), sptField,
                     FxDB(dr("paktif"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ptglorder"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ptglmulairencana"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ptglmulairealisasi"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ptglselesairencana"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ptglselesairealisasi"), ""), formatTgl), sptField,
                     FxDB(dr("pprioritas"), ""), sptField,
                     FxDB(dr("pselesai"), 0), sptField,
                     FxDB(dr("pkontak"), 0), sptField,
                     FxDB(dr("pkontakperson"), ""), sptField,
                     FxDB(dr("ppimpinanproyek"), 0), sptField,
                     FxDB(dr("pdivisi"), ""), sptField,
                     FxDB(dr("pketerangan"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ptglkontrak"), ""), formatTgl), sptField,
                     FxDB(dr("pnokontrak"), ""), sptField,
                     FxDB(dr("pnilaikontrak"), 0), sptField,
                     FxDB(dr("psubdari"), 0), sptField,
                     FxDB(dr("pparent"), ""), sptField,
                     FxDB(dr("plevel"), 0), sptField,
                     FxDB(dr("pcustom1"), ""), sptField,
                     FxDB(dr("pcustom2"), ""), sptField,
                     FxDB(dr("pcustom3"), ""), sptField,
                     FxDB(dr("pcustom4"), ""), sptField,
                     FxDB(dr("pcustom5"), ""), sptField,
                     FxDB(dr("pinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pgd"), ""), sptField,
                     FxDB(dr("pstatus"), ""), sptField,
                     FxDB(dr("pkontakkode"), ""), sptField,
                     FxDB(dr("pkontaknama"), ""), sptField,
                     FxDB(dr("ppimpinanproyekkode"), ""), sptField,
                     FxDB(dr("ppimpinanproyeknama"), ""), sptField,
                     FxDB(dr("pdivisinama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Project data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pkode, pnama, pkategori, paktif, ptglorder, ptglmulairencana, ptglmulairealisasi, ptglselesairencana, ptglselesairealisasi, pprioritas, pselesai, pkontak, pkontakperson, ppimpinanproyek, pdivisi, pketerangan, ptglkontrak, pnokontrak, pnilaikontrak, psubdari, pparent, plevel, pcustom1, pcustom2, pcustom3, pcustom4, pcustom5, pinputuser, pinputtgl, pmodifikasiuser, pmodifikasitgl, pgd, pstatus, pkontakkode, pkontaknama, ppimpinanproyekkode, ppimpinanproyeknama, pdivisinama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_ProjectCekId(ByVal param As String) As String

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
            result(2) = "pkode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'CEK DI DATABASE ================================================================
        Dim dt As DataTable
        Dim exist As Integer = 0
        dt = AsDataTableAmbilDariDB("SELECT COUNT(pkode) FROM m1_project WHERE pkode='" & idtransaksi & "'")
        exist = dt.Rows(0)(0)

        If (exist > 0) Then
            result(2) = "'" & idtransaksi & "' already exist for column pkode." : GoTo selesai
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
    Public Function M1_ProjectTerkait(ByVal param As String) As String
        'M1_ProjectTerkait --------------------------------------------------------
        'pkode, pnama, sumber, idterkait

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
            result(2) = "pkode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m1_project_terkait")
        sql = sql.Replace("valkode", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("pkode"), ""), sptField,
                             FxDB(dr("pnama"), ""), sptField,
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
            result(2) = "Related Project data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pkode, pnama, sumber, idterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_ProjectDownload(ByVal param As String) As String
        'M1_ProjectDownload --------------------------------------------------------
        'pkode, pnama, pkategori, paktif, ptglorder, ptglmulairencana, ptglmulairealisasi, 
        'ptglselesairencana, ptglselesairealisasi, pprioritas, pselesai, pkontak, pkontakperson, ppimpinanproyek, 
        'pdivisi, pketerangan, ptglkontrak, pnokontrak, pnilaikontrak, psubdari, pparent, 
        'plevel, pcustom1, pcustom2, pcustom3, pcustom4, pcustom5, pinputuser, 
        'pinputtgl, pmodifikasiuser, pmodifikasitgl, pgd, pstatus

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

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Project", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("pkode"), ""), sptField,
                     FxDB(dr("pnama"), ""), sptField,
                     FxDB(dr("pkategori"), ""), sptField,
                     FxDB(dr("paktif"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ptglorder"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ptglmulairencana"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ptglmulairealisasi"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ptglselesairencana"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ptglselesairealisasi"), ""), formatTgl), sptField,
                     FxDB(dr("pprioritas"), ""), sptField,
                     FxDB(dr("pselesai"), 0), sptField,
                     FxDB(dr("pkontak"), ""), sptField,
                     FxDB(dr("pkontakperson"), ""), sptField,
                     FxDB(dr("ppimpinanproyek"), ""), sptField,
                     FxDB(dr("pdivisi"), ""), sptField,
                     FxDB(dr("pketerangan"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ptglkontrak"), ""), formatTgl), sptField,
                     FxDB(dr("pnokontrak"), ""), sptField,
                     FxDB(dr("pnilaikontrak"), 0), sptField,
                     FxDB(dr("psubdari"), 0), sptField,
                     FxDB(dr("pparent"), ""), sptField,
                     FxDB(dr("plevel"), 0), sptField,
                     FxDB(dr("pcustom1"), ""), sptField,
                     FxDB(dr("pcustom2"), ""), sptField,
                     FxDB(dr("pcustom3"), ""), sptField,
                     FxDB(dr("pcustom4"), ""), sptField,
                     FxDB(dr("pcustom5"), ""), sptField,
                     FxDB(dr("pinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pgd"), ""), sptField,
                     FxDB(dr("pstatus"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Project data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pkode, pnama, pkategori, paktif, ptglorder, ptglmulairencana, ptglmulairealisasi, ptglselesairencana, ptglselesairealisasi, pprioritas, pselesai, pkontak, pkontakperson, ppimpinanproyek, pdivisi, pketerangan, ptglkontrak, pnokontrak, pnilaikontrak, psubdari, pparent, plevel, pcustom1, pcustom2, pcustom3, pcustom4, pcustom5, pinputuser, pinputtgl, pmodifikasiuser, pmodifikasitgl, pgd, pstatus"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_ProjectImport(ByVal param As String) As String
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
        'pkode(0) As String, pnama(1) As String, pkategori(2) As String, paktif(3) As Integer, ptglorder(4) As Date, 
        'ptglmulairencana(5) As Date, ptglmulairealisasi(6) As Date, ptglselesairencana(7) As Date, ptglselesairealisasi(8) As Date, pprioritas(9) As String, 
        'pselesai(10) As Double, pkontak(11) As Integer, pkontakperson(12) As String, ppimpinanproyek(13) As Integer, pdivisi(14) As String, 
        'pketerangan(15) As String, ptglkontrak(16) As Date, pnokontrak(17) As String, pnilaikontrak(18) As Double, psubdari(19) As Integer, 
        'pparent(20) As String, plevel(21) As Integer, pcustom1(22) As String, pcustom2(23) As String, pcustom3(24) As String, 
        'pcustom4(25) As String, pcustom5(26) As String, pinputuser(27) As Integer, pinputtgl(28) As DateTime, pmodifikasiuser(29) As Integer, 
        'pmodifikasitgl(30) As DateTime, pgd(31) As String, pstatus(32) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'pkode, pnama, pkategori, paktif, ptglorder, ptglmulairencana, ptglmulairealisasi, 
        'ptglselesairencana, ptglselesairealisasi, pprioritas, pselesai, pkontak, pkontakperson, ppimpinanproyek, 
        'pdivisi, pketerangan, ptglkontrak, pnokontrak, pnilaikontrak, psubdari, pparent, 
        'plevel, pcustom1, pcustom2, pcustom3, pcustom4, pcustom5, pinputuser, 
        'pinputtgl, pmodifikasiuser, pmodifikasitgl, pgd, pstatus

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pkategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "paktif", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "ptglorder", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptglmulairencana", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptglmulairealisasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptglselesairencana", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptglselesairealisasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pprioritas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pselesai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ppimpinanproyek", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pketerangan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptglkontrak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pnokontrak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pnilaikontrak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "psubdari", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pparent", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "plevel", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pcustom1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcustom2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcustom3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcustom4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcustom5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pgd", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pstatus", AsEnumTypeData.AsString)

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
            If (dataRowDetail.Length <> 33) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'paktif(3) As Integer
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - paktif required numeric." : GoTo selesai
            End If
            'ptglorder(4) As Date
            If (IsDate(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - ptglorder required date." : GoTo selesai
            End If
            'ptglmulairencana(5) As Date
            If (IsDate(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - ptglmulairencana required date." : GoTo selesai
            End If
            'ptglmulairealisasi(6) As Date
            If (IsDate(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - ptglmulairealisasi required date." : GoTo selesai
            End If
            'ptglselesairencana(7) As Date
            If (IsDate(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - ptglselesairencana required date." : GoTo selesai
            End If
            'ptglselesairealisasi(8) As Date
            If (IsDate(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - ptglselesairealisasi required date." : GoTo selesai
            End If
            'pselesai(10) As Double
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - pselesai required numeric." : GoTo selesai
            End If
            'pkontak(11) As Integer
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - pkontak required numeric." : GoTo selesai
            End If
            'ppimpinanproyek(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - ppimpinanproyek required numeric." : GoTo selesai
            End If
            'ptglkontrak(16) As Date
            If (IsDate(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - ptglkontrak required date." : GoTo selesai
            End If
            'pnilaikontrak(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - pnilaikontrak required numeric." : GoTo selesai
            End If
            'psubdari(19) As Integer
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - psubdari required numeric." : GoTo selesai
            End If
            'plevel(21) As Integer
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - plevel required numeric." : GoTo selesai
            End If
            'pinputuser(27) As Integer
            If (IsNumeric(dataRowDetail(27)) = False) Then
                result(2) = "Row : " & i & " - pinputuser required numeric." : GoTo selesai
            End If
            'pinputtgl(28) As DateTime
            If (IsDate(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - pinputtgl required date." : GoTo selesai
            End If
            'pmodifikasiuser(29) As Integer
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - pmodifikasiuser required numeric." : GoTo selesai
            End If
            'pmodifikasitgl(30) As DateTime
            If (IsDate(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - pmodifikasitgl required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'pkode(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pkode can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pkode should not be more than 25 character." : GoTo selesai
            End If

            'pnama(1) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - pnama can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 100 Then
                result(2) = "Row : " & i & " - pnama should not be more than 100 character." : GoTo selesai
            End If

            'pinputtgl(28) As DateTime
            If Len(dataRowDetail(28)) = 0 Then
                result(2) = "Row : " & i & " - pinputtgl can't be empty" : GoTo selesai
            End If

            'pmodifikasitgl(30) As DateTime
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Row : " & i & " - pmodifikasitgl can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pkode~pnama~pkategori~paktif~ptglorder~ptglmulairencana~ptglmulairealisasi~ptglselesairencana~ptglselesairealisasi~pprioritas~pselesai~pkontak~pkontakperson~ppimpinanproyek~pdivisi~pketerangan~ptglkontrak~pnokontrak~pnilaikontrak~psubdari~pparent~plevel~pcustom1~pcustom2~pcustom3~pcustom4~pcustom5~pinputuser~pinputtgl~pmodifikasiuser~pmodifikasitgl~pgd~pstatus", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32)) = False Then
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
                For Each dr1 As DataRow In dtdetail.Rows
                    strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Append("('" & FixQuotes(dr1("pkode")) & "', '" & FixQuotes(dr1("pnama")) & "', '" & FixQuotes(dr1("pkategori")) & "', " & dr1("paktif") & ", '" & FixQuotes(AsFormatTanggal(dr1("ptglorder"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("ptglmulairencana"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("ptglmulairealisasi"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("ptglselesairencana"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("ptglselesairealisasi"))) & "', '" & FixQuotes(dr1("pprioritas")) & "', '" & FixDouble(dr1("pselesai")) & "', " & dr1("pkontak") & ", '" & FixQuotes(dr1("pkontakperson")) & "', " & dr1("ppimpinanproyek") & ", '" & FixQuotes(dr1("pdivisi")) & "', '" & FixQuotes(dr1("pketerangan")) & "', '" & FixQuotes(AsFormatTanggal(dr1("ptglkontrak"))) & "', '" & FixQuotes(dr1("pnokontrak")) & "', '" & FixDouble(dr1("pnilaikontrak")) & "', " & dr1("psubdari") & ", '" & FixQuotes(dr1("pparent")) & "', " & dr1("plevel") & ", '" & FixQuotes(dr1("pcustom1")) & "', '" & FixQuotes(dr1("pcustom2")) & "', '" & FixQuotes(dr1("pcustom3")) & "', '" & FixQuotes(dr1("pcustom4")) & "', '" & FixQuotes(dr1("pcustom5")) & "', " & dr1("pinputuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("pinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("pmodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("pmodifikasitgl"), "yyyy-MM-dd H:mm:ss")) & "', '" & FixQuotes(dr1("pgd")) & "', '" & FixQuotes(dr1("pstatus")) & "')")
                Next

                If Len(strValue2.ToString) > 0 Then
                    'DELETE
                    sql = "DELETE FROM M1_Project"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'insert jika data belum ada, dan update jika data sudah ada
                    sql = "Insert into M1_Project(pkode, pnama, pkategori, paktif, ptglorder, ptglmulairencana, ptglmulairealisasi, ptglselesairencana, ptglselesairealisasi, pprioritas, pselesai, pkontak, pkontakperson, ppimpinanproyek, pdivisi, pketerangan, ptglkontrak, pnokontrak, pnilaikontrak, psubdari, pparent, plevel, pcustom1, pcustom2, pcustom3, pcustom4, pcustom5, pinputuser, pinputtgl, pmodifikasiuser, pmodifikasitgl, pgd, pstatus) values" & strValue2.ToString & ""
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
            Dim paramSearch As String = M1_ProjectSearch(PostWsSearch(paramSplit(0), "M1_ProjectSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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

End Class