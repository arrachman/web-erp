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
Public Class m1_item_permission
    Inherits System.Web.Services.WebService
    Public ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi

    <WebMethod()>
    Public Function M1_Item_PermissionSimpan(ByVal param As String) As String

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
        'ipkode(0), ipnama(1), ipcatatan(2), ipjual(3), ipmutasipusat(4), ippermintaanmutasi(5),
        'ipmutasicabang(6), ipretursupplier(7), ippermintaanpembelian(8),
        'ipinputuser(9), ipinputtgl(10), ipmodifikasiuser(11), ipmodifikasipgl(12), 
        'ipcustomtext1(13), ipcustomtext2(14), ipcustomtext3(15), ipcustomtext4(16), 
        'ipcustomtext5(17), ipcustomint1(18), ipcustomint2(19),
        'ipcustomint3(20), ipcustomdbl1(21), ipcustomdbl2(22), ipcustomdbl3(23), ipcustomdate1(24), ipcustomdate2(25), ipcustomdate3(26)


        'MAPPING BUAT FLEX --------------------------------------------------------
        'ipkode, ipnama, ipcatatan, ipjual, ipmutasipusat, ippermintaanmutasi,
        'ipmutasicabang, ipretursupplier, ippermintaanpembelian,
        'ipinputuser, ipinputtgl, ipmodifikasiuser, ipmodifikasipgl, ipcustomtext1,
        'ipcustomtext2, ipcustomtext3, ipcustomtext4, ipcustomtext5, ipcustomint1, ipcustomint2,
        'ipcustomint3, ipcustomdbl1, ipcustomdbl2, ipcustomdbl3, ipcustomdate1, ipcustomdate2, ipcustomdate3

        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 27) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================

        'VALIDASI TIPE DATA ==========================================================
        'ipjual(3) As Integer
        If (IsNumeric(dataUtama(3)) = False) Then
            result(2) = " - ipjual required numeric." : GoTo selesai
        End If

        'ipmutasipusat(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = " - ipmutasipusat required numeric." : GoTo selesai
        End If
        'ippermintaanmutasi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = " - ippermintaanmutasi required numeric." : GoTo selesai
        End If
        'ipmutasicabang(6) As Integer
        If (IsNumeric(dataUtama(6)) = False) Then
            result(2) = " - ipmutasicabang required numeric." : GoTo selesai
        End If
        'ipipretursupplier(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = " - ipretursupplier required numeric." : GoTo selesai
        End If
        'ippermintaanpembelian(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = " - ippermintaanpembelian required numeric." : GoTo selesai
        End If
        'ipinputuser(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = " - ipinputuser required numeric." : GoTo selesai
        End If
        'ipinputtgl(10) As DateTime
        If (IsDate(dataUtama(10)) = False) Then
            result(2) = " - ipinputtgl required date." : GoTo selesai
        End If
        'ipmodifikasiuser(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = " - ipmodifikasiuser required numeric." : GoTo selesai
        End If
        'ipmodifikasitgl(12) As DateTime
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = " - ipmodifikasitgl required date." : GoTo selesai
        End If
        'ipcustomint1(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = " - ipcustomint1 required numeric." : GoTo selesai
        End If
        'ipcustomint2(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = " - ipcustomint2 required numeric." : GoTo selesai
        End If
        'ipcustomint3(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = " - ipcustomint3 required numeric." : GoTo selesai
        End If
        'ipcustomdbl1(21) As Double
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = " - ipcustomdbl1 required numeric." : GoTo selesai
        End If
        'ipcustomdbl2(22) As Double
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = " - ipcustomdbl2 required numeric." : GoTo selesai
        End If
        'ipcustomdbl3(23) As Double
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = " - ipcustomdbl3 required numeric." : GoTo selesai
        End If
        'ipcustomdate1(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = " - ipcustomdate1 required date." : GoTo selesai
        End If
        'ipcustomdate2(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = " - ipcustomdate2 required date." : GoTo selesai
        End If
        'ipcustomdate3(26) As Date
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = " - ipcustomdate3 required date." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA -----------------------------------

        'VALIDASI DATA ---------------------------------------
        'ipkode(0) As String
        If Len(dataUtama(0)) = 0 Then
            result(2) = "- ipkode can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 25 Then
            result(2) = "- ipkode should not be more than 25 character." : GoTo selesai
        End If

        'ipnama(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "- ipnama can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 100 Then
            result(2) = "- ipnama should not be more than 100 character." : GoTo selesai
        End If

        'ipinputtgl(9) As DateTime
        If Len(dataUtama(9)) = 0 Then
            result(2) = "- ipinputtgl can't be empty" : GoTo selesai
        End If

        'ipmodifikasitgl(11) As DateTime
        If Len(dataUtama(11)) = 0 Then
            result(2) = "- ipmodifikasitgl can't be empty" : GoTo selesai
        End If

        'ipcustomdbl1(21) As Double
        If Len(dataUtama(21)) = 0 Then
            result(2) = " - ipcustomdbl1 can't be empty" : GoTo selesai
        End If

        'ipcustomdbl2(22) As Double
        If Len(dataUtama(22)) = 0 Then
            result(2) = " - ipcustomdbl2 can't be empty" : GoTo selesai
        End If

        'ipcustomdbl3(23) As Double
        If Len(dataUtama(23)) = 0 Then
            result(2) = " - ipcustomdbl3 can't be empty" : GoTo selesai
        End If

        'ipcustomdate1(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = " - ipcustomdate1 can't be empty" : GoTo selesai
        End If

        'ipcustomdate2(20) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = " - ipcustomdate2 can't be empty" : GoTo selesai
        End If

        'ipcustomdate3(21) As Date
        If Len(dataUtama(26)) = 0 Then
            result(2) = " - ipcustomdate3 can't be empty" : GoTo selesai
        End If
        '---------------------------------------------------------------------------------

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
                dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(ip.ipkode) FROM M1_Item_Permission ip WHERE ip.ipkode ='" & FixQuotes(dataUtama(0)) & "'")
                rowUpdate = dtupdate.Rows(0)(0)
                If (rowUpdate > 0) Then
                    'SIMPAN HISTORY ========================
                    Dim SimpanHistory As New m1_item_permission_history
                    Dim ipSimpanHistory As String = SimpanHistory.M1_Item_Permission_HistorySimpan("" & paramSplit(0) & "★M1_Item_Permission_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(dataUtama(0)) & "")
                    Dim ipSplit() As String = ipSimpanHistory.Split(sptParam)
                    Dim ipSplitResult() As String = ipSplit(0).Split(sptSubParam)
                    'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                    If (ipSplitResult(1) = 0) Then
                        result(2) = "Insert history failed : " & ipSplitResult(2) : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF SIMPAN HISTORY ==================


                    sql = "Update M1_Item_Permission set ipnama  = '" & FixQuotes(dataUtama(1)) & "', ipcatatan  = '" & FixQuotes(dataUtama(2)) & "', ipjual  = '" & FixQuotes(dataUtama(3)) & "', ipmutasipusat  = '" & FixQuotes(dataUtama(4)) & "', ippermintaanmutasi  = '" & FixQuotes(dataUtama(5)) & "', ipmutasicabang  = '" & FixQuotes(dataUtama(6)) & "', ipretursupplier  = " & dataUtama(7) & ", ippermintaanpembelian  = " & dataUtama(8) & ", ipmodifikasiuser  = " & dataUtama(11) & ", ipmodifikasitgl  = NOW(), ipcustomtext1 = '" & FixQuotes(dataUtama(13)) & "', ipcustomtext2 = '" & FixQuotes(dataUtama(14)) & "', ipcustomtext3 = '" & FixQuotes(dataUtama(15)) & "', ipcustomtext4 = '" & FixQuotes(dataUtama(16)) & "', ipcustomtext5 = '" & FixQuotes(dataUtama(17)) & "', ipcustomint1 = " & FixQuotes(dataUtama(18)) & ", ipcustomint2 = " & FixQuotes(dataUtama(19)) & ", ipcustomint3 = " & FixQuotes(dataUtama(20)) & ", ipcustomdbl1 = " & FixQuotes(dataUtama(21)) & ", ipcustomdbl2 = " & FixQuotes(dataUtama(22)) & ", ipcustomdbl3 = " & FixQuotes(dataUtama(23)) & ", ipcustomdate1 = '" & FixQuotes(dataUtama(24)) & "', ipcustomdate2 = '" & FixQuotes(dataUtama(25)) & "', ipcustomdate3 = '" & FixQuotes(dataUtama(26)) & "' where ipkode = '" & dataUtama(0) & "'"
                    'result(2) = sql : GoTo selesai
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
                sql = "Insert into M1_Item_Permission (ipkode, ipnama, ipcatatan, ipjual, ipmutasipusat, ippermintaanmutasi, ipmutasicabang, ipretursupplier, ippermintaanpembelian, ipinputuser, ipinputtgl, ipmodifikasiuser, ipmodifikasitgl, ipcustomtext1, ipcustomtext2, ipcustomtext3, ipcustomtext4, ipcustomtext5, ipcustomint1, ipcustomint2, ipcustomint3, ipcustomdbl1, ipcustomdbl2, ipcustomdbl3, ipcustomdate1, ipcustomdate2, ipcustomdate3) values ('" & FixQuotes(dataUtama(0)) & "', '" & FixQuotes(dataUtama(1)) & "', '" & FixQuotes(dataUtama(2)) & "', '" & FixQuotes(dataUtama(3)) & "', '" & FixQuotes(dataUtama(4)) & "', '" & FixQuotes(dataUtama(5)) & "', '" & FixQuotes(dataUtama(6)) & "', " & dataUtama(7) & ", " & dataUtama(8) & ", " & dataUtama(9) & ", NOW(), " & dataUtama(11) & ", '1971-01-01 00:00:00', '" & dataUtama(13) & "', '" & dataUtama(14) & "', '" & dataUtama(15) & "', '" & dataUtama(16) & "', '" & dataUtama(17) & "', " & dataUtama(18) & ", " & dataUtama(19) & ", " & dataUtama(20) & ", " & dataUtama(21) & ", " & dataUtama(22) & ", " & dataUtama(23) & ", '" & dataUtama(24) & "', '" & dataUtama(25) & "', '" & dataUtama(26) & "')"
                'result(2) = sql : GoTo selesai
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
            Dim paramSearch As String = M1_Item_PermissionSearch(PostWsSearch(paramSplit(0), "M1_Item_PermissionSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_Item_PermissionDelete(ByVal param As String) As String

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
            result(2) = "pckode can't be empty." : GoTo selesai
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
            Dim paramTerkait As String = M1_Item_PermissionTerkait(PostWsTerkait(paramSplit(0), "M1_Item_PermissionTerkait", pagingSplit(0), pagingSplit(1), "", "", formatTgl, formatTglWaktu, idtransaksi))
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
            Dim SimpanHistory As New m1_item_permission_history
            Dim ipSimpanHistory As String = SimpanHistory.M1_Item_Permission_HistorySimpan("" & paramSplit(0) & "★M1_Item_Permission_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(idtransaksi) & "")
            Dim ipSplit() As String = ipSimpanHistory.Split(sptParam)
            Dim ipSplitResult() As String = ipSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (ipSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & ipSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            'DELETE
            sql = "DELETE FROM M1_Item_Permission WHERE ipkode = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M1_Item_PermissionSearch(PostWsSearch(paramSplit(0), "M1_Item_PermissionSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_Item_PermissionSearch(ByVal param As String) As String
        'M1_Item_PermissionSearch --------------------------------------------------------
        'ipkode, ipnama, ipcatatan, ipjual, ipmutasipusat, ippermintaanmutasi,
        'ipmutasicabang, ipretursupplier, ippermintaanpembelian,
        'ipinputuser, ipinputtgl, ipmodifikasiuser, ipmodifikasipgl, ipcustomtext1,
        'ipcustomtext2, ipcustomtext3, ipcustomtext4, ipcustomtext5, ipcustomint1, ipcustomint2,
        'ipcustomint3, ipcustomdbl1, ipcustomdbl2, ipcustomdbl3, ipcustomdate1, ipcustomdate2, ipcustomdate3

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
        sql = "select `ip`.`ipkode` AS `ipkode`, `ip`.`ipnama` AS `ipnama`, `ip`.`ipcatatan` AS `ipcatatan`,`ip`.`ipjual` AS `ipjual`, `ip`.`ipmutasipusat` AS `ipmutasipusat`, `ip`.`ippermintaanmutasi` AS `ippermintaanmutasi`, `ip`.`ipmutasicabang` AS `ipmutasicabang`, `ip`.`ipretursupplier` AS `ipretursupplier`, `ip`.`ippermintaanpembelian` AS `ippermintaanpembelian`, `ip`.`ipinputuser` AS `ipinputuser`, `ip`.`ipinputtgl` AS `ipinputtgl`, `ip`.`ipmodifikasiuser` AS `ipmodifikasiuser`, `ip`.`ipmodifikasitgl` AS `ipmodifikasitgl`, `ip`.`ipcustomtext1` AS `ipcustomtext1`, `ip`.`ipcustomtext2` AS `ipcustomtext2`, `ip`.`ipcustomtext3` AS `ipcustomtext3`, `ip`.`ipcustomtext4` AS `ipcustomtext4`, `ip`.`ipcustomtext5` AS `ipcustomtext5`, `ip`.`ipcustomint1` AS `ipcustomint1`, `ip`.`ipcustomint2` AS `ipcustomint2`, `ip`.`ipcustomint3` AS `ipcustomint3`, `ip`.`ipcustomdbl1` AS `ipcustomdbl1`, `ip`.`ipcustomdbl2` AS `ipcustomdbl2`, `ip`.`ipcustomdbl3` AS `ipcustomdbl3`, `ip`.`ipcustomdate1` AS `ipcustomdate1`, `ip`.`ipcustomdate2` AS `ipcustomdate2`, `ip`.`ipcustomdate3` AS `ipcustomdate3`, `u1`.`unama` AS `ipinputusernama`, `u2`.`unama` AS `ipmodifikasiusernama`from `m1_item_permission` `ip` left join `m0_user` `u1` on `ip`.`ipinputuser` = `u1`.`userid` left join `m0_user` `u2` on `ip`.`ipmodifikasiuser` = `u2`.`userid`"

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Item_Permission", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("ipkode"), ""), sptField,
                     FxDB(dr("ipnama"), ""), sptField,
                     FxDB(dr("ipcatatan"), ""), sptField,
                     FxDB(dr("ipjual"), 0), sptField,
                     FxDB(dr("ipmutasipusat"), 0), sptField,
                     FxDB(dr("ippermintaanmutasi"), 0), sptField,
                     FxDB(dr("ipmutasicabang"), 0), sptField,
                     FxDB(dr("ipretursupplier"), 0), sptField,
                     FxDB(dr("ippermintaanpembelian"), 0), sptField,
                     FxDB(dr("ipinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ipinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ipmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ipmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ipcustomtext1"), ""), sptField,
                     FxDB(dr("ipcustomtext2"), ""), sptField,
                     FxDB(dr("ipcustomtext3"), ""), sptField,
                     FxDB(dr("ipcustomtext4"), ""), sptField,
                     FxDB(dr("ipcustomtext5"), ""), sptField,
                     FxDB(dr("ipcustomint1"), 0), sptField,
                     FxDB(dr("ipcustomint2"), 0), sptField,
                     FxDB(dr("ipcustomint3"), 0), sptField,
                     FxDB(dr("ipcustomdbl1"), 0), sptField,
                     FxDB(dr("ipcustomdbl2"), 0), sptField,
                     FxDB(dr("ipcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ipcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ipcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ipcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("ipinputusernama"), ""), sptField,
                     FxDB(dr("ipmodifikasiusernama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Item Permission data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ipkode, ipnama, ipcatatan, ipjual, ipmutasipusat, ippermintaanmutasi, ipmutasicabang, ipretursupplier, ippermintaanpembelian, ipinputuser, ipinputtgl, ipmodifikasiuser, ipmodifikasipgl, ipcustomtext1, ipcustomtext2, ipcustomtext3, ipcustomtext4, ipcustomtext5, ipcustomint1, ipcustomint2, ipcustomint3, ipcustomdbl1, ipcustomdbl2, ipcustomdbl3, ipcustomdate1, ipcustomdate2, ipcustomdate3, ipinputusernama, ipmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Item_PermissionCekId(ByVal param As String) As String

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
            result(2) = "pckode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'CEK DI DATABASE ================================================================
        Dim dt As DataTable
        Dim exist As Integer = 0
        dt = AsDataTableAmbilDariDB("SELECT COUNT(ipkode) FROM M1_Item_Permission WHERE ipkode='" & idtransaksi & "'")
        exist = dt.Rows(0)(0)

        If (exist > 0) Then
            result(2) = "'" & idtransaksi & "' already exist for column ipkode." : GoTo selesai
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
    Public Function M1_Item_PermissionTerkait(ByVal param As String) As String
        'M1_Item_PermissionTerkait --------------------------------------------------------
        'ipkode, ipnama, sumber, idterkait

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
            result(2) = "ipkode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        sql = "select ip.ipkode AS ipkode, ip.ipnama AS ipnama, s.skode AS sumber, CONCAT('Setting : ', s.smodule, ' - ', s.sgrup, ' - ', s.skode) AS idterkait from m0_setting s join m1_item_permission ip on (s.smodule = 0 AND s.sgrup = 'options' AND s.skode = 'Tag' AND s.snilai = ip.ipkode) WHERE ip.ipkode = 'valkode' UNION ALL select ip.ipkode AS ipkode, ip.ipnama AS ipnama, 'Item' AS sumber, i.bkode AS idterkait from m1_item i join m1_item_permission ip on (i.btag = ip.ipkode) WHERE ip.ipkode = 'valkode'"
        sql = sql.Replace("valkode", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("ipkode"), ""), sptField,
                             FxDB(dr("ipnama"), ""), sptField,
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
            result(2) = "Related Item Permission data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ipkode, ipnama, sumber, idterkait"))

        Return wsResult
    End Function

End Class
