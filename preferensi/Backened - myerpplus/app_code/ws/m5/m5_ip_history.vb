Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_ip_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_Ip_HistorySimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean

        Dim sumber As String = "", idtransaksi As String = ""

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


        'MAPPING BUAT WS ----------------------------------------------------------
        'sumber(0) As String, idtransaksi(1) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'sumber, idtransaksi


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = paramSplit(5).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 2) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI DATA UTAMA ===============================================================
        'sumber(0) As String
        If Len(dataUtama(0)) = 0 Then
            result(2) = "sumber can't be empty" : GoTo selesai
        Else
            sumber = dataUtama(0)
        End If

        'idtransaksi(1) As Integer
        If (IsNumeric(dataUtama(1)) = False) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
        Else
            idtransaksi = dataUtama(1)
        End If
        'END OF VALIDASI DATA UTAMA ========================================================


        'SIMPAN KE DATABASE ================================================================
        Con2 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con2.Open()

        '*** Start Transaction ***'  
        Trans = Con2.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'PROSES INSERT HISTORY UTAMA ---------------------------------------
            sql = "INSERT INTO m5_ip_history(SELECT 0, ip.* FROM m5_ip ip WHERE ip.ipid = '" & idtransaksi & "')"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY UTAMA --------------------------------


            'PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT --------------------
            Dim dt2 As New DataTable
            sql = "SELECT ipidhistory FROM m5_ip_history WHERE ipid = '" & idtransaksi & "' ORDER BY ipmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m5_ip_pay_history (SELECT 0, '" & result(4) & "', ip.* FROM m5_ip_pay ip WHERE ip.idip = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY DETAIL -------------------------------


            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

        Catch ex As Exception
            Trans.Rollback() '*** RollBack Transaction ***'  
            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try

        objCmd = Nothing
        'Con2.Close()
        'Con2 = Nothing
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

    <WebMethod()>
    Public Function M5_Ip_HistorySearch(ByVal param As String) As String
        'M5_Ip_HistorySearch --------------------------------------------------------
        'ipidhistory, ipid, ipcabang, iplokasi, ipjenis, ipsumber, ipautonotransaksi, ipnotransaksi, 
        'iptgl, ipkodepa, ipkontak, ipkontakperson, ip1alamat1, ip1alamat2, ip1alamat3, 
        'ip2alamat1, ip2alamat2, ip2alamat3, ipbagianterima, iptermin, iptgljatuhtempo, ipidso, 
        'ipnorek, ipuraian, ipcatatan, ipnoref, iptglnoref, ipmatauang, ipkurs, 
        'ipjumlah, ipjumlahvalas, ipjumlahbayar, ipjumlahbayarvalas, ipstatusbayar, iptgllunas, ipcostcenter, 
        'ipdivisi, ipsubdivisi, ipproyek, ipstatus, ipstatussebelumnya, ipjmlrevisi, ipcetakanke, 
        'ipinputuser, ipinputtgl, ipmodifikasiuser, ipmodifikasitgl, ipposting, ippostingtgl, ipisclose, 
        'ipcabangnama, iplokasinama, ipjenisnama, ipkontakkode, ipkontaknama, ipbagianterimakode, ipbagianterimanama, 
        'sonotransaksi, ipnoreknama, ipstatusnama, ipstatussebelumnyanama, ipinputusernama, ipmodifikasiusernama

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
            Filter = Filter.Replace("ipkontakkode", "c1.kkode")
            Filter = Filter.Replace("ipkontaknama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_ip_v_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M5_Ip_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("ipid"), 0), sptField,
                     FxDB(dr("ipidhistory"), 0), sptField,
                     FxDB(dr("ipcabang"), ""), sptField,
                     FxDB(dr("iplokasi"), ""), sptField,
                     FxDB(dr("ipjenis"), 0), sptField,
                     FxDB(dr("ipsumber"), ""), sptField,
                     FxDB(dr("ipautonotransaksi"), 0), sptField,
                     FxDB(dr("ipnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("iptgl"), ""), formatTgl), sptField,
                     FxDB(dr("ipkodepa"), 0), sptField,
                     FxDB(dr("ipkontak"), 0), sptField,
                     FxDB(dr("ipkontakperson"), ""), sptField,
                     FxDB(dr("ip1alamat1"), ""), sptField,
                     FxDB(dr("ip1alamat2"), ""), sptField,
                     FxDB(dr("ip1alamat3"), ""), sptField,
                     FxDB(dr("ip2alamat1"), ""), sptField,
                     FxDB(dr("ip2alamat2"), ""), sptField,
                     FxDB(dr("ip2alamat3"), ""), sptField,
                     FxDB(dr("ipbagianterima"), 0), sptField,
                     FxDB(dr("iptermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("iptgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("ipidso"), 0), sptField,
                     FxDB(dr("ipnorek"), ""), sptField,
                     FxDB(dr("ipuraian"), ""), sptField,
                     FxDB(dr("ipcatatan"), ""), sptField,
                     FxDB(dr("ipnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("iptglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("ipmatauang"), ""), sptField,
                     FxDB(dr("ipkurs"), 0), sptField,
                     FxDB(dr("ipjumlah"), 0), sptField,
                     FxDB(dr("ipjumlahvalas"), 0), sptField,
                     FxDB(dr("ipjumlahbayar"), 0), sptField,
                     FxDB(dr("ipjumlahbayarvalas"), 0), sptField,
                     FxDB(dr("ipstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("iptgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("ipcostcenter"), ""), sptField,
                     FxDB(dr("ipdivisi"), ""), sptField,
                     FxDB(dr("ipsubdivisi"), ""), sptField,
                     FxDB(dr("ipproyek"), ""), sptField,
                     FxDB(dr("ipstatus"), 0), sptField,
                     FxDB(dr("ipstatussebelumnya"), 0), sptField,
                     FxDB(dr("ipjmlrevisi"), 0), sptField,
                     FxDB(dr("ipcetakanke"), 0), sptField,
                     FxDB(dr("ipinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ipinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ipmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ipmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ipposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ippostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ipisclose"), 0), sptField,
                     FxDB(dr("ipcabangnama"), ""), sptField,
                     FxDB(dr("iplokasinama"), ""), sptField,
                     FxDB(dr("ipjenisnama"), ""), sptField,
                     FxDB(dr("ipkontakkode"), ""), sptField,
                     FxDB(dr("ipkontaknama"), ""), sptField,
                     FxDB(dr("ipbagianterimakode"), ""), sptField,
                     FxDB(dr("ipbagianterimanama"), ""), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("ipnoreknama"), ""), sptField,
                     FxDB(dr("ipstatusnama"), ""), sptField,
                     FxDB(dr("ipstatussebelumnyanama"), ""), sptField,
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
            result(2) = "Transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ipidhistory, ipid, ipcabang, iplokasi, ipjenis, ipsumber, ipautonotransaksi, ipnotransaksi, iptgl, ipkodepa, ipkontak, ipkontakperson, ip1alamat1, ip1alamat2, ip1alamat3, ip2alamat1, ip2alamat2, ip2alamat3, ipbagianterima, iptermin, iptgljatuhtempo, ipidso, ipnorek, ipuraian, ipcatatan, ipnoref, iptglnoref, ipmatauang, ipkurs, ipjumlah, ipjumlahvalas, ipjumlahbayar, ipjumlahbayarvalas, ipstatusbayar, iptgllunas, ipcostcenter, ipdivisi, ipsubdivisi, riproyek, ipstatus, ipstatussebelumnya, ipjmlrevisi, ipcetakanke, ipinputuser, ipinputtgl, ipmodifikasiuser, ipmodifikasitgl, riposting, ripostingtgl, ipisclose, ipcabangnama, iplokasinama, ipjenisnama, ipkontakkode, ipkontaknama, ipbagianterimakode, ipbagianterimanama, sonotransaksi, ipnoreknama, ipstatusnama, ipstatussebelumnyanama, ipinputusernama, ipmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_Ip_HistoryGetdataById(ByVal param As String) As String
        'M5_Ip_HistoryGetdataById Utama --------------------------------------------------------
        'ipidhistory, ipid, ipcabang, iplokasi, ipjenis, ipsumber, ipautonotransaksi, ipnotransaksi, 
        'iptgl, ipkodepa, ipkontak, ipkontakperson, ip1alamat1, ip1alamat2, ip1alamat3, 
        'ip2alamat1, ip2alamat2, ip2alamat3, ipbagianterima, iptermin, iptgljatuhtempo, ipidso, 
        'ipnorek, ipuraian, ipcatatan, ipnoref, iptglnoref, ipmatauang, ipkurs, 
        'ipjumlah, ipjumlahvalas, ipjumlahbayar, ipjumlahbayarvalas, ipstatusbayar, iptgllunas, ipcostcenter, 
        'ipdivisi, ipsubdivisi, ipproyek, ipstatus, ipstatussebelumnya, ipjmlrevisi, ipcetakanke, 
        'ipinputuser, ipinputtgl, ipmodifikasiuser, ipmodifikasitgl, ipposting, ippostingtgl, ipisclose, 
        'ipcustomtext1, ipcustomtext2, ipcustomtext3, ipcustomtext4, ipcustomtext5, ipcustomint1, ipcustomint2, 
        'ipcustomint3, ipcustomdbl1, ipcustomdbl2, ipcustomdbl3, ipcustomdate1, ipcustomdate2, ipcustomdate3, 
        'ipcabangnama, iplokasinama, ipkontakkode, ipkontaknama, ipbagianterimakode, ipbagianterimanama, ipterminnama, 
        'ipterminharijatuhtempo, sonotransaksi, ipnoreknama, ipcostcenternama, ipdivisinama, ipsubdivisinama, ipproyeknama, 
        'ipstatusnama, ipstatussebelumnyanama, ipinputusernama, ipmodifikasiusernama

        'M5_Ip_HistoryGetdataById Pay -------------------------------------------------------
        'idiphistorycarabayar, idhistory, idipcarabayar, idip, carabayar, matauang, 
        'kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, 
        'rekbank, rekgiro, catatan, urutan, isclose, carabayarnama, banknama, 
        'rekbanknama, rekgironama

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

        Dim NmMemcached As String = "aplikasi1-M5_Ip_history~M5_Ip_Pay_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "ipidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "ipidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_ip_getdata_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("ipidhistory"), 0), sptField, FxDB(drutama("ipid"), 0), sptField,
                     FxDB(drutama("ipcabang"), ""), sptField,
                     FxDB(drutama("iplokasi"), ""), sptField,
                     FxDB(drutama("ipjenis"), 0), sptField,
                     FxDB(drutama("ipsumber"), ""), sptField,
                     FxDB(drutama("ipautonotransaksi"), 0), sptField,
                     FxDB(drutama("ipnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("iptgl"), ""), formatTgl), sptField,
                     FxDB(drutama("ipkodepa"), 0), sptField,
                     FxDB(drutama("ipkontak"), 0), sptField,
                     FxDB(drutama("ipkontakperson"), ""), sptField,
                     FxDB(drutama("ip1alamat1"), ""), sptField,
                     FxDB(drutama("ip1alamat2"), ""), sptField,
                     FxDB(drutama("ip1alamat3"), ""), sptField,
                     FxDB(drutama("ip2alamat1"), ""), sptField,
                     FxDB(drutama("ip2alamat2"), ""), sptField,
                     FxDB(drutama("ip2alamat3"), ""), sptField,
                     FxDB(drutama("ipbagianterima"), 0), sptField,
                     FxDB(drutama("iptermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("iptgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("ipidso"), 0), sptField,
                     FxDB(drutama("ipnorek"), ""), sptField,
                     FxDB(drutama("ipuraian"), ""), sptField,
                     FxDB(drutama("ipcatatan"), ""), sptField,
                     FxDB(drutama("ipnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("iptglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("ipmatauang"), ""), sptField,
                     FxDB(drutama("ipkurs"), 0), sptField,
                     FxDB(drutama("ipjumlah"), 0), sptField,
                     FxDB(drutama("ipjumlahvalas"), 0), sptField,
                     FxDB(drutama("ipjumlahbayar"), 0), sptField,
                     FxDB(drutama("ipjumlahbayarvalas"), 0), sptField,
                     FxDB(drutama("ipstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("iptgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("ipcostcenter"), ""), sptField,
                     FxDB(drutama("ipdivisi"), ""), sptField,
                     FxDB(drutama("ipsubdivisi"), ""), sptField,
                     FxDB(drutama("ipproyek"), ""), sptField,
                     FxDB(drutama("ipstatus"), 0), sptField,
                     FxDB(drutama("ipstatussebelumnya"), 0), sptField,
                     FxDB(drutama("ipjmlrevisi"), 0), sptField,
                     FxDB(drutama("ipcetakanke"), 0), sptField,
                     FxDB(drutama("ipinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ipinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("ipmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ipmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("ipposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ippostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("ipisclose"), 0), sptField,
                     FxDB(drutama("ipcustomtext1"), ""), sptField,
                     FxDB(drutama("ipcustomtext2"), ""), sptField,
                     FxDB(drutama("ipcustomtext3"), ""), sptField,
                     FxDB(drutama("ipcustomtext4"), ""), sptField,
                     FxDB(drutama("ipcustomtext5"), ""), sptField,
                     FxDB(drutama("ipcustomint1"), 0), sptField,
                     FxDB(drutama("ipcustomint2"), 0), sptField,
                     FxDB(drutama("ipcustomint3"), 0), sptField,
                     FxDB(drutama("ipcustomdbl1"), 0), sptField,
                     FxDB(drutama("ipcustomdbl2"), 0), sptField,
                     FxDB(drutama("ipcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ipcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ipcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ipcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("ipcabangnama"), ""), sptField,
                     FxDB(drutama("iplokasinama"), ""), sptField,
                     FxDB(drutama("ipkontakkode"), ""), sptField,
                     FxDB(drutama("ipkontaknama"), ""), sptField,
                     FxDB(drutama("ipbagianterimakode"), ""), sptField,
                     FxDB(drutama("ipbagianterimanama"), ""), sptField,
                     FxDB(drutama("ipterminnama"), ""), sptField,
                     FxDB(drutama("ipterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("sonotransaksi"), ""), sptField,
                     FxDB(drutama("ipnoreknama"), ""), sptField,
                     FxDB(drutama("ipcostcenternama"), ""), sptField,
                     FxDB(drutama("ipdivisinama"), ""), sptField,
                     FxDB(drutama("ipsubdivisinama"), ""), sptField,
                     FxDB(drutama("ipproyeknama"), ""), sptField,
                     FxDB(drutama("ipstatusnama"), ""), sptField,
                     FxDB(drutama("ipstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("ipinputusernama"), ""), sptField,
                     FxDB(drutama("ipmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorycarabayar"), 0), sptField, FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idipcarabayar"), 0), sptField,
                     FxDB(dr("idip"), 0), sptField,
                     FxDB(dr("carabayar"), 0), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("jumlah"), 0), sptField,
                     FxDB(dr("jumlahvalas"), 0), sptField,
                     FxDB(dr("nogiro"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgljt"), ""), formatTgl), sptField,
                     FxDB(dr("bank"), ""), sptField,
                     FxDB(dr("noacbank"), ""), sptField,
                     FxDB(dr("rekbank"), ""), sptField,
                     FxDB(dr("rekgiro"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("carabayarnama"), ""), sptField,
                     FxDB(dr("banknama"), ""), sptField,
                     FxDB(dr("rekbanknama"), ""), sptField,
                     FxDB(dr("rekgironama"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = " transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, detail)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ipidhistory, ipid, ipcabang, iplokasi, ipjenis, ipsumber, ipautonotransaksi, ipnotransaksi, iptgl, ipkodepa, ipkontak, ipkontakperson, ip1alamat1, ip1alamat2, ip1alamat3, ip2alamat1, ip2alamat2, ip2alamat3, ipbagianterima, iptermin, iptgljatuhtempo, ipidso, ipnorek, ipuraian, ipcatatan, ipnoref, iptglnoref, ipmatauang, ipkurs, ipjumlah, ipjumlahvalas, ipjumlahbayar, ipjumlahbayarvalas, ipstatusbayar, iptgllunas, ipcostcenter, ipdivisi, ipsubdivisi, ipproyek, ipstatus, ipstatussebelumnya, ipjmlrevisi, ipcetakanke, ipinputuser, ipinputtgl, ipmodifikasiuser, ipmodifikasitgl, ipposting, ippostingtgl, ipisclose, ipcustomtext1, ipcustomtext2, ipcustomtext3, ipcustomtext4, ipcustomtext5, ipcustomint1, ipcustomint2, ipcustomint3, ipcustomdbl1, ipcustomdbl2, ipcustomdbl3, ipcustomdate1, ipcustomdate2, ipcustomdate3, ipcabangnama, iplokasinama, ipkontakkode, ipkontaknama, ipbagianterimakode, ipbagianterimanama, ipterminnama, ipterminharijatuhtempo, sonotransaksi, ipnoreknama, ipcostcenternama, ipdivisinama, ipsubdivisinama, ipproyeknama, ipstatusnama, ipstatussebelumnyanama, ipinputusernama, ipmodifikasiusernama"), sptSubParam, ReplaceMapping("idhistorycarabayar, idhistory, idipcarabayar, idip, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose, carabayarnama, banknama, rekbanknama, rekgironama"))

        Return wsResult
    End Function

End Class
