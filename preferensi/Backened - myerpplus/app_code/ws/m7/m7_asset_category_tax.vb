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
Public Class m7_asset_category_tax
    Inherits System.Web.Services.WebService
    Public ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi

    <WebMethod()>
    Public Function M7_Asset_Category_TaxSimpan(ByVal param As String) As String

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
        'actkode(0) As String, actnama(1) As String, actmetode(2) As , actumur(3) As Double, actpenyusutan(4) As Double, 
        'actinputuser(5) As Integer, actinputtgl(6) As DateTime, actmodifikasiuser(7) As Integer, actmodifikasitgl(8) As DateTime, actcustomtext1(9) As String, 
        'actcustomtext2(10) As String, actcustomtext3(11) As String, actcustomtext4(12) As String, actcustomtext5(13) As String, actcustomint1(14) As Integer, 
        'actcustomint2(15) As Integer, actcustomint3(16) As Integer, actcustomdbl1(17) As Double, actcustomdbl2(18) As Double, actcustomdbl3(19) As Double, 
        'actcustomdate1(20) As Date, actcustomdate2(21) As Date, actcustomdate3(22) As Date

        'MAPPING BUAT FLEX --------------------------------------------------------
        'actkode, actnama, actmetode, actumur, actpenyusutan, actinputuser, actinputtgl, 
        'actmodifikasiuser, actmodifikasitgl, actcustomtext1, actcustomtext2, actcustomtext3, actcustomtext4, actcustomtext5, 
        'actcustomint1, actcustomint2, actcustomint3, actcustomdbl1, actcustomdbl2, actcustomdbl3, actcustomdate1, 
        'actcustomdate2, actcustomdate3


        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 23) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================


        'VALIDASI TIPE DATA ==========================================================
        'actumur(3) As Double
        If (IsNumeric(dataUtama(3)) = False) Then
            result(2) = "actumur required numeric." : GoTo selesai
        End If
        'actpenyusutan(4) As Double
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "actpenyusutan required numeric." : GoTo selesai
        End If
        'actinputuser(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "actinputuser required numeric." : GoTo selesai
        End If
        'actinputtgl(6) As DateTime
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "actinputtgl required date." : GoTo selesai
        End If
        'actmodifikasiuser(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "actmodifikasiuser required numeric." : GoTo selesai
        End If
        'actmodifikasitgl(8) As DateTime
        If (IsDate(dataUtama(8)) = False) Then
            result(2) = "actmodifikasitgl required date." : GoTo selesai
        End If
        'actcustomint1(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "actcustomint1 required numeric." : GoTo selesai
        End If
        'actcustomint2(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "actcustomint2 required numeric." : GoTo selesai
        End If
        'actcustomint3(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "actcustomint3 required numeric." : GoTo selesai
        End If
        'actcustomdbl1(17) As Double
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "actcustomdbl1 required numeric." : GoTo selesai
        End If
        'actcustomdbl2(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "actcustomdbl2 required numeric." : GoTo selesai
        End If
        'actcustomdbl3(19) As Double
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "actcustomdbl3 required numeric." : GoTo selesai
        End If
        'actcustomdate1(20) As Date
        If (IsDate(dataUtama(20)) = False) Then
            result(2) = "actcustomdate1 required date." : GoTo selesai
        End If
        'actcustomdate2(21) As Date
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "actcustomdate2 required date." : GoTo selesai
        End If
        'actcustomdate3(22) As Date
        If (IsDate(dataUtama(22)) = False) Then
            result(2) = "actcustomdate3 required date." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA ===================================================


        'VALIDASI DATA ===============================================================
        'actkode(0) As String
        If Len(dataUtama(0)) = 0 Then
            result(2) = "actkode can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 25 Then
            result(2) = "actkode should not be more than 25 character." : GoTo selesai
        End If

        'actnama(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "actnama can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 50 Then
            result(2) = "actnama should not be more than 50 character." : GoTo selesai
        End If

        'actmetode(2) As 
        If Len(dataUtama(2)) = 0 Then
            result(2) = "actmetode can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 20 Then
            result(2) = "actmetode should not be more than 20 character." : GoTo selesai
        End If

        'actumur(3) As Double
        If Len(dataUtama(3)) = 0 Then
            result(2) = "actumur can't be empty" : GoTo selesai
        End If

        'actpenyusutan(4) As Double
        If Len(dataUtama(4)) = 0 Then
            result(2) = "actpenyusutan can't be empty" : GoTo selesai
        End If

        'actinputtgl(6) As DateTime
        If Len(dataUtama(6)) = 0 Then
            result(2) = "actinputtgl can't be empty" : GoTo selesai
        End If

        'actmodifikasitgl(8) As DateTime
        If Len(dataUtama(8)) = 0 Then
            result(2) = "actmodifikasitgl can't be empty" : GoTo selesai
        End If

        'actcustomdbl1(17) As Double
        If Len(dataUtama(17)) = 0 Then
            result(2) = "actcustomdbl1 can't be empty" : GoTo selesai
        End If

        'actcustomdbl2(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "actcustomdbl2 can't be empty" : GoTo selesai
        End If

        'actcustomdbl3(19) As Double
        If Len(dataUtama(19)) = 0 Then
            result(2) = "actcustomdbl3 can't be empty" : GoTo selesai
        End If

        'actcustomdate1(20) As Date
        If Len(dataUtama(20)) = 0 Then
            result(2) = "actcustomdate1 can't be empty" : GoTo selesai
        End If

        'actcustomdate2(21) As Date
        If Len(dataUtama(21)) = 0 Then
            result(2) = "actcustomdate2 can't be empty" : GoTo selesai
        End If

        'actcustomdate3(22) As Date
        If Len(dataUtama(22)) = 0 Then
            result(2) = "actcustomdate3 can't be empty" : GoTo selesai
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
                dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(actkode) FROM M7_Asset_Category_Tax WHERE actkode = '" & dataUtama(0) & "'")
                rowUpdate = dtupdate.Rows(0)(0)

                If (rowUpdate > 0) Then
                    'SIMPAN HISTORY ========================
                    Dim SimpanHistory As New m7_asset_category_tax_history
                    Dim assetcategorytaxSimpanHistory As String = SimpanHistory.M7_Asset_Category_Tax_HistorySimpan("" & paramSplit(0) & "★M7_Asset_Category_Tax_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(dataUtama(0)) & "")
                    Dim assetcategorytaxSplit() As String = assetcategorytaxSimpanHistory.Split(sptParam)
                    Dim assetcategorytaxSplitResult() As String = assetcategorytaxSplit(0).Split(sptSubParam)
                    'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                    If (assetcategorytaxSplitResult(1) = 0) Then
                        result(2) = "Insert history failed : " & assetcategorytaxSplitResult(2) : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF SIMPAN HISTORY ==================

                    sql = "Update M7_Asset_Category_Tax set actnama  = '" & FixQuotes(dataUtama(1)) & "', actmetode  = '" & FixQuotes(dataUtama(2)) & "', actumur  = '" & FixDouble(dataUtama(3)) & "', actpenyusutan  = '" & FixDouble(dataUtama(4)) & "', actmodifikasiuser  = " & dataUtama(7) & ", actmodifikasitgl  = NOW(), actcustomtext1  = '" & FixQuotes(dataUtama(9)) & "', actcustomtext2  = '" & FixQuotes(dataUtama(10)) & "', actcustomtext3  = '" & FixQuotes(dataUtama(11)) & "', actcustomtext4  = '" & FixQuotes(dataUtama(12)) & "', actcustomtext5  = '" & FixQuotes(dataUtama(13)) & "', actcustomint1  = " & dataUtama(14) & ", actcustomint2  = " & dataUtama(15) & ", actcustomint3  = " & dataUtama(16) & ", actcustomdbl1  = '" & FixDouble(dataUtama(17)) & "', actcustomdbl2  = '" & FixDouble(dataUtama(18)) & "', actcustomdbl3  = '" & FixDouble(dataUtama(19)) & "', actcustomdate1  = '" & FixQuotes(AsFormatTanggal(dataUtama(20))) & "', actcustomdate2  = '" & FixQuotes(AsFormatTanggal(dataUtama(21))) & "', actcustomdate3  = '" & FixQuotes(AsFormatTanggal(dataUtama(22))) & "' where actkode = '" & dataUtama(0) & "'"
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
                sql = "Insert into M7_Asset_Category_Tax (actkode, actnama, actmetode, actumur, actpenyusutan, actinputuser, actinputtgl, actmodifikasiuser, actmodifikasitgl, actcustomtext1, actcustomtext2, actcustomtext3, actcustomtext4, actcustomtext5, actcustomint1, actcustomint2, actcustomint3, actcustomdbl1, actcustomdbl2, actcustomdbl3, actcustomdate1, actcustomdate2, actcustomdate3) values('" & FixQuotes(dataUtama(0)) & "', '" & FixQuotes(dataUtama(1)) & "', '" & FixQuotes(dataUtama(2)) & "', '" & FixDouble(dataUtama(3)) & "', '" & FixDouble(dataUtama(4)) & "', " & dataUtama(5) & ", NOW(), " & dataUtama(7) & ", '1971-01-01 00:00:00', '" & FixQuotes(dataUtama(9)) & "', '" & FixQuotes(dataUtama(10)) & "', '" & FixQuotes(dataUtama(11)) & "', '" & FixQuotes(dataUtama(12)) & "', '" & FixQuotes(dataUtama(13)) & "', " & dataUtama(14) & ", " & dataUtama(15) & ", " & dataUtama(16) & ", '" & FixDouble(dataUtama(17)) & "', '" & FixDouble(dataUtama(18)) & "', '" & FixDouble(dataUtama(19)) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(20))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(21))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(22))) & "')"
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
            Dim paramSearch As String = M7_Asset_Category_TaxSearch(PostWsSearch(paramSplit(0), "M7_Asset_Category_TaxSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M7_Asset_Category_TaxDelete(ByVal param As String) As String

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
            result(2) = "akode can't be empty." : GoTo selesai
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
            Dim paramTerkait As String = M7_Asset_Category_TaxTerkait(PostWsTerkait(paramSplit(0), "M7_Asset_Category_TaxTerkait", pagingSplit(0), pagingSplit(1), "", "", formatTgl, formatTglWaktu, idtransaksi))
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
            Dim SimpanHistory As New m7_asset_category_tax_history
            Dim assetcategorytaxSimpanHistory As String = SimpanHistory.M7_Asset_Category_Tax_HistorySimpan("" & paramSplit(0) & "★M7_Asset_Category_Tax_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(idtransaksi) & "")
            Dim assetcategorytaxSplit() As String = assetcategorytaxSimpanHistory.Split(sptParam)
            Dim assetcategorytaxSplitResult() As String = assetcategorytaxSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (assetcategorytaxSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & assetcategorytaxSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            'DELETE
            sql = "DELETE FROM M7_Asset_Category_Tax WHERE actkode = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M7_Asset_Category_TaxSearch(PostWsSearch(paramSplit(0), "M7_Asset_Category_TaxSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M7_Asset_Category_TaxSearch(ByVal param As String) As String
        'M7_Asset_Category_TaxSearch --------------------------------------------------------
        'actkode, actnama, actmetode, actumur, actpenyusutan, actinputuser, actinputtgl, 
        'actmodifikasiuser, actmodifikasitgl, actcustomtext1, actcustomtext2, actcustomtext3, actcustomtext4, actcustomtext5, 
        'actcustomint1, actcustomint2, actcustomint3, actcustomdbl1, actcustomdbl2, actcustomdbl3, actcustomdate1, 
        'actcustomdate2, actcustomdate3, actmetodenama, actinputusernama, actmodifikasiusernama

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
        Dim query As New m0_query
        sql = query.PanggilQuery("m7_asset_category_tax_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Coa", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("actkode"), ""), sptField,
                     FxDB(dr("actnama"), ""), sptField,
                     FxDB(dr("actmetode"), ""), sptField,
                     FxDB(dr("actumur"), 0), sptField,
                     FxDB(dr("actpenyusutan"), 0), sptField,
                     FxDB(dr("actinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("actinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("actmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("actmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("actcustomtext1"), ""), sptField,
                     FxDB(dr("actcustomtext2"), ""), sptField,
                     FxDB(dr("actcustomtext3"), ""), sptField,
                     FxDB(dr("actcustomtext4"), ""), sptField,
                     FxDB(dr("actcustomtext5"), ""), sptField,
                     FxDB(dr("actcustomint1"), 0), sptField,
                     FxDB(dr("actcustomint2"), 0), sptField,
                     FxDB(dr("actcustomint3"), 0), sptField,
                     FxDB(dr("actcustomdbl1"), 0), sptField,
                     FxDB(dr("actcustomdbl2"), 0), sptField,
                     FxDB(dr("actcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("actcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("actcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("actcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("actmetodenama"), ""), sptField,
                     FxDB(dr("actinputusernama"), ""), sptField,
                     FxDB(dr("actmodifikasiusernama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Asset Category Tax data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("actkode, actnama, actmetode, actumur, actpenyusutan, actinputuser, actinputtgl, actmodifikasiuser, actmodifikasitgl, actcustomtext1, actcustomtext2, actcustomtext3, actcustomtext4, actcustomtext5, actcustomint1, actcustomint2, actcustomint3, actcustomdbl1, actcustomdbl2, actcustomdbl3, actcustomdate1, actcustomdate2, actcustomdate3, actmetodenama, actinputusernama, actmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M7_Asset_Category_TaxCekId(ByVal param As String) As String

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
            result(2) = "actkode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================


        'CEK DI DATABASE ================================================================
        Dim dt As DataTable
        Dim exist As Integer = 0
        dt = AsDataTableAmbilDariDB("SELECT COUNT(actkode) FROM m7_asset_category_tax WHERE actkode ='" & idtransaksi & "'")
        exist = dt.Rows(0)(0)

        If (exist > 0) Then
            result(2) = "'" & idtransaksi & "' already exist for column actkode." : GoTo selesai
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
    Public Function M7_Asset_Category_TaxTerkait(ByVal param As String) As String
        'M7_Asset_Category_TaxTerkait --------------------------------------------------------
        'actkode, actnama, sumber, idterkait

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
            result(2) = "actkode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================


        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m7_asset_category_tax_terkait")
        sql = sql.Replace("valkode", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("actkode"), ""), sptField,
                             FxDB(dr("actnama"), ""), sptField,
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
            result(2) = "Related Asset Tax Category data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("actkode, actnama, sumber, idterkait"))

        Return wsResult
    End Function

End Class