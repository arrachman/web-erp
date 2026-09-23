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
Public Class m7_asset_category
    Inherits System.Web.Services.WebService
    Public ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi

    <WebMethod()>
    Public Function M7_Asset_CategorySimpan(ByVal param As String) As String

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
        'ackode(0) As String, acnama(1) As String, ackategoripajak(2) As String, acrekakumdepresiasi(3) As String, acrekdepresiasi(4) As String, 
        'acrekasset(5) As String, acinputuser(6) As Integer, acinputtgl(7) As DateTime, acmodifikasiuser(8) As Integer, acmodifikasitgl(9) As DateTime, 
        'accustomtext1(10) As String, accustomtext2(11) As String, accustomtext3(12) As String, accustomtext4(13) As String, accustomtext5(14) As String, 
        'accustomint1(15) As Integer, accustomint2(16) As Integer, accustomint3(17) As Integer, accustomdbl1(18) As Double, accustomdbl2(19) As Double, 
        'accustomdbl3(20) As Double, accustomdate1(21) As Date, accustomdate2(22) As Date, accustomdate3(23) As Date

        'MAPPING BUAT FLEX --------------------------------------------------------
        'ackode, acnama, ackategoripajak, acrekakumdepresiasi, acrekdepresiasi, acrekasset, acinputuser, 
        'acinputtgl, acmodifikasiuser, acmodifikasitgl, accustomtext1, accustomtext2, accustomtext3, accustomtext4, 
        'accustomtext5, accustomint1, accustomint2, accustomint3, accustomdbl1, accustomdbl2, accustomdbl3, 
        'accustomdate1, accustomdate2, accustomdate3


        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 24) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================


        'VALIDASI TIPE DATA ==========================================================
        'acinputuser(6) As Integer
        If (IsNumeric(dataUtama(6)) = False) Then
            result(2) = "acinputuser required numeric." : GoTo selesai
        End If
        'acinputtgl(7) As DateTime
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "acinputtgl required date." : GoTo selesai
        End If
        'acmodifikasiuser(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "acmodifikasiuser required numeric." : GoTo selesai
        End If
        'acmodifikasitgl(9) As DateTime
        If (IsDate(dataUtama(9)) = False) Then
            result(2) = "acmodifikasitgl required date." : GoTo selesai
        End If
        'accustomint1(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "accustomint1 required numeric." : GoTo selesai
        End If
        'accustomint2(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "accustomint2 required numeric." : GoTo selesai
        End If
        'accustomint3(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "accustomint3 required numeric." : GoTo selesai
        End If
        'accustomdbl1(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "accustomdbl1 required numeric." : GoTo selesai
        End If
        'accustomdbl2(19) As Double
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "accustomdbl2 required numeric." : GoTo selesai
        End If
        'accustomdbl3(20) As Double
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "accustomdbl3 required numeric." : GoTo selesai
        End If
        'accustomdate1(21) As Date
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "accustomdate1 required date." : GoTo selesai
        End If
        'accustomdate2(22) As Date
        If (IsDate(dataUtama(22)) = False) Then
            result(2) = "accustomdate2 required date." : GoTo selesai
        End If
        'accustomdate3(23) As Date
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "accustomdate3 required date." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA ===================================================


        'VALIDASI DATA ===============================================================
        'ackode(0) As String
        If Len(dataUtama(0)) = 0 Then
            result(2) = "ackode can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 25 Then
            result(2) = "ackode should not be more than 25 character." : GoTo selesai
        End If

        'acnama(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "acnama can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 50 Then
            result(2) = "acnama should not be more than 50 character." : GoTo selesai
        End If

        'ackategoripajak(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "ackategoripajak can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "ackategoripajak should not be more than 25 character." : GoTo selesai
        End If

        'acekakumdepresiasi(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "acrekakumdepresiasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "acrekakumdepresiasi should not be more than 25 character." : GoTo selesai
        End If

        'acekdepresiasi(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "acrekdepresiasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 25 Then
            result(2) = "acrekdepresiasi should not be more than 25 character." : GoTo selesai
        End If

        'acinputtgl(7) As DateTime
        If Len(dataUtama(7)) = 0 Then
            result(2) = "acinputtgl can't be empty" : GoTo selesai
        End If

        'acmodifikasitgl(9) As DateTime
        If Len(dataUtama(9)) = 0 Then
            result(2) = "acmodifikasitgl can't be empty" : GoTo selesai
        End If

        'accustomdbl1(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "accustomdbl1 can't be empty" : GoTo selesai
        End If

        'accustomdbl2(19) As Double
        If Len(dataUtama(19)) = 0 Then
            result(2) = "accustomdbl2 can't be empty" : GoTo selesai
        End If

        'accustomdbl3(20) As Double
        If Len(dataUtama(20)) = 0 Then
            result(2) = "accustomdbl3 can't be empty" : GoTo selesai
        End If

        'accustomdate1(21) As Date
        If Len(dataUtama(21)) = 0 Then
            result(2) = "accustomdate1 can't be empty" : GoTo selesai
        End If

        'accustomdate2(22) As Date
        If Len(dataUtama(22)) = 0 Then
            result(2) = "accustomdate2 can't be empty" : GoTo selesai
        End If

        'accustomdate3(23) As Date
        If Len(dataUtama(23)) = 0 Then
            result(2) = "accustomdate3 can't be empty" : GoTo selesai
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
                dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(ackode) FROM M7_Asset_Category WHERE ackode = '" & dataUtama(0) & "'")
                rowUpdate = dtupdate.Rows(0)(0)

                If (rowUpdate > 0) Then
                    'SIMPAN HISTORY ========================
                    Dim SimpanHistory As New m7_asset_category_history
                    Dim assetcategorySimpanHistory As String = SimpanHistory.M7_Asset_Category_HistorySimpan("" & paramSplit(0) & "★M7_Asset_Category_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(dataUtama(0)) & "")
                    Dim assetcategorySplit() As String = assetcategorySimpanHistory.Split(sptParam)
                    Dim assetcategorySplitResult() As String = assetcategorySplit(0).Split(sptSubParam)
                    'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                    If (assetcategorySplitResult(1) = 0) Then
                        result(2) = "Insert history failed : " & assetcategorySplitResult(2) : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF SIMPAN HISTORY ==================

                    sql = "Update M7_Asset_Category set acnama  = '" & FixQuotes(dataUtama(1)) & "', ackategoripajak  = '" & FixQuotes(dataUtama(2)) & "', acrekakumdepresiasi  = '" & FixQuotes(dataUtama(3)) & "', acrekdepresiasi  = '" & FixQuotes(dataUtama(4)) & "', acrekasset  = '" & FixQuotes(dataUtama(5)) & "', acmodifikasiuser  = " & dataUtama(8) & ", acmodifikasitgl  = NOW(), accustomtext1  = '" & FixQuotes(dataUtama(10)) & "', accustomtext2  = '" & FixQuotes(dataUtama(11)) & "', accustomtext3  = '" & FixQuotes(dataUtama(12)) & "', accustomtext4  = '" & FixQuotes(dataUtama(13)) & "', accustomtext5  = '" & FixQuotes(dataUtama(14)) & "', accustomint1  = " & dataUtama(15) & ", accustomint2  = " & dataUtama(16) & ", accustomint3  = " & dataUtama(17) & ", accustomdbl1  = '" & FixDouble(dataUtama(18)) & "', accustomdbl2  = '" & FixDouble(dataUtama(19)) & "', accustomdbl3  = '" & FixDouble(dataUtama(20)) & "', accustomdate1  = '" & FixQuotes(AsFormatTanggal(dataUtama(21))) & "', accustomdate2  = '" & FixQuotes(AsFormatTanggal(dataUtama(22))) & "', accustomdate3  = '" & FixQuotes(AsFormatTanggal(dataUtama(23))) & "' where ackode = '" & dataUtama(0) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Else
                    result(2) = "Data not found." : GoTo selesai
                End If

            Else
                sql = "Insert into M7_Asset_Category (ackode, acnama, ackategoripajak, acrekakumdepresiasi, acrekdepresiasi, acrekasset, acinputuser, acinputtgl, acmodifikasiuser, acmodifikasitgl, accustomtext1, accustomtext2, accustomtext3, accustomtext4, accustomtext5, accustomint1, accustomint2, accustomint3, accustomdbl1, accustomdbl2, accustomdbl3, accustomdate1, accustomdate2, accustomdate3) values('" & FixQuotes(dataUtama(0)) & "', '" & FixQuotes(dataUtama(1)) & "', '" & FixQuotes(dataUtama(2)) & "', '" & FixQuotes(dataUtama(3)) & "', '" & FixQuotes(dataUtama(4)) & "', '" & FixQuotes(dataUtama(5)) & "', " & dataUtama(6) & ", NOW(), " & dataUtama(8) & ", '1971-01-01 00:00:00', '" & FixQuotes(dataUtama(10)) & "', '" & FixQuotes(dataUtama(11)) & "', '" & FixQuotes(dataUtama(12)) & "', '" & FixQuotes(dataUtama(13)) & "', '" & FixQuotes(dataUtama(14)) & "', " & dataUtama(15) & ", " & dataUtama(16) & ", " & dataUtama(17) & ", '" & FixDouble(dataUtama(18)) & "', '" & FixDouble(dataUtama(19)) & "', '" & FixDouble(dataUtama(20)) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(21))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(22))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(23))) & "')"
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
            Dim paramSearch As String = M7_Asset_CategorySearch(PostWsSearch(paramSplit(0), "M7_Asset_CategorySearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M7_Asset_CategoryDelete(ByVal param As String) As String

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
            result(2) = "ackode can't be empty." : GoTo selesai
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
            Dim paramTerkait As String = M7_Asset_CategoryTerkait(PostWsTerkait(paramSplit(0), "M7_Asset_CategoryTerkait", pagingSplit(0), pagingSplit(1), "", "", formatTgl, formatTglWaktu, idtransaksi))
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
            Dim SimpanHistory As New m7_asset_category_history
            Dim assetcategorySimpanHistory As String = SimpanHistory.M7_Asset_Category_HistorySimpan("" & paramSplit(0) & "★M7_Asset_Category_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(idtransaksi) & "")
            Dim assetcategorySplit() As String = assetcategorySimpanHistory.Split(sptParam)
            Dim assetcategorySplitResult() As String = assetcategorySplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (assetcategorySplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & assetcategorySplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            'DELETE
            sql = "DELETE FROM M7_Asset_Category WHERE ackode = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M7_Asset_CategorySearch(PostWsSearch(paramSplit(0), "M7_Asset_CategorySearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M7_Asset_CategorySearch(ByVal param As String) As String
        'M7_Asset_CategorySearch --------------------------------------------------------
        'ackode, acnama, ackategoripajak, acrekakumdepresiasi, acrekdepresiasi, acrekasset, acinputuser, 
        'acinputtgl, acmodifikasiuser, acmodifikasitgl, accustomtext1, accustomtext2, accustomtext3, accustomtext4, 
        'accustomtext5, accustomint1, accustomint2, accustomint3, accustomdbl1, accustomdbl2, accustomdbl3, 
        'accustomdate1, accustomdate2, accustomdate3, ackategoripajaknama, ackategoripajakmetode, ackategoripajakmetodenama, ackategoripajakumur, 
        'ackategoripajakpenyusutan, acrekakumdepresiasinama, acrekdepresiasinama, acrekassetnama, acinputusernama, acmodifikasiusernama

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
            formatTglWaktu = "yyy-MM-dd H:mm:ss"
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
        sql = query.PanggilQuery("m7_asset_category_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-m7_asset_category_v", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("ackode"), ""), sptField,
                     FxDB(dr("acnama"), ""), sptField,
                     FxDB(dr("ackategoripajak"), ""), sptField,
                     FxDB(dr("acrekakumdepresiasi"), ""), sptField,
                     FxDB(dr("acrekdepresiasi"), ""), sptField,
                     FxDB(dr("acrekasset"), ""), sptField,
                     FxDB(dr("acinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("acinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("acmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("acmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("accustomtext1"), ""), sptField,
                     FxDB(dr("accustomtext2"), ""), sptField,
                     FxDB(dr("accustomtext3"), ""), sptField,
                     FxDB(dr("accustomtext4"), ""), sptField,
                     FxDB(dr("accustomtext5"), ""), sptField,
                     FxDB(dr("accustomint1"), 0), sptField,
                     FxDB(dr("accustomint2"), 0), sptField,
                     FxDB(dr("accustomint3"), 0), sptField,
                     FxDB(dr("accustomdbl1"), 0), sptField,
                     FxDB(dr("accustomdbl2"), 0), sptField,
                     FxDB(dr("accustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("accustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("accustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("accustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("ackategoripajaknama"), ""), sptField,
                     FxDB(dr("ackategoripajakmetode"), ""), sptField,
                     FxDB(dr("ackategoripajakmetodenama"), ""), sptField,
                     FxDB(dr("ackategoripajakumur"), 0), sptField,
                     FxDB(dr("ackategoripajakpenyusutan"), 0), sptField,
                     FxDB(dr("acrekakumdepresiasinama"), ""), sptField,
                     FxDB(dr("acrekdepresiasinama"), ""), sptField,
                     FxDB(dr("acrekassetnama"), ""), sptField,
                     FxDB(dr("acinputusernama"), ""), sptField,
                     FxDB(dr("acmodifikasiusernama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Asset Category data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ackode, acnama, ackategoripajak, acrekakumdepresiasi, acrekdepresiasi, acrekasset, acinputuser, acinputtgl, acmodifikasiuser, acmodifikasitgl, accustomtext1, accustomtext2, accustomtext3, accustomtext4, accustomtext5, accustomint1, accustomint2, accustomint3, accustomdbl1, accustomdbl2, accustomdbl3, accustomdate1, accustomdate2, accustomdate3, ackategoripajaknama, ackategoripajakmetode, ackategoripajakmetodenama, ackategoripajakumur, ackategoripajakpenyusutan, acrekakumdepresiasinama, acrekdepresiasinama, acrekassetnama, acinputusernama, acmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M7_Asset_CategoryCekId(ByVal param As String) As String

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
            result(2) = "ackode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================


        'CEK DI DATABASE ================================================================
        Dim dt As DataTable
        Dim exist As Integer = 0
        dt = AsDataTableAmbilDariDB("SELECT COUNT(ackode) FROM m7_asset_category WHERE ackode = '" & idtransaksi & "'")
        exist = dt.Rows(0)(0)

        If (exist > 0) Then
            result(2) = "'" & idtransaksi & "' already exist for column ackode." : GoTo selesai
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
    Public Function M7_Asset_CategoryTerkait(ByVal param As String) As String
        'M7_Asset_CategoryTerkait --------------------------------------------------------
        'ackode, acnama, sumber, idterkait

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
            result(2) = "akode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m7_asset_category_terkait")
        sql = sql.Replace("valkode", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("ackode"), ""), sptField,
                             FxDB(dr("acnama"), ""), sptField,
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
            result(2) = "Related Asset Category data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ackode, acnama, sumber, idterkait"))

        Return wsResult
    End Function

End Class