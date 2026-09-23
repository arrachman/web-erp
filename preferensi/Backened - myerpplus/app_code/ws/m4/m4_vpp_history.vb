Imports Microsoft.VisualBasic

Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_vpp_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_Vpp_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m4_vpp_history(SELECT 0, vpp.* FROM m4_vpp vpp WHERE vpp.vppid = '" & idtransaksi & "')"
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
            sql = "SELECT vppidhistory FROM m4_vpp_history WHERE vppid = '" & idtransaksi & "' ORDER BY vppmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m4_vpp_detail_history (SELECT 0, '" & result(4) & "', vpp.* FROM m4_vpp_detail vpp WHERE vpp.idvpp = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY DETAIL -------------------------------

            'PROSES INSERT HISTORY Pay --------------------------------------
            sql = "INSERT INTO m4_vpp_pay_history (SELECT 0, '" & result(4) & "', vpp.* FROM m4_vpp_pay vpp WHERE vpp.idvpp = '" & idtransaksi & "' )"
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
    Public Function M4_Vpp_HistorySearch(ByVal param As String) As String
        'M4_Vpp_HistorySearch --------------------------------------------------------
        'vppidhistory, vppid, vppcabang, vpplokasi, vppgudang, vppsumber, vppautonotransaksi, vppnotransaksi, 
        'vpptgl, vppkodepa, vppsupplier, vppsupplierkontak, vpp1alamat1, vpp1alamat2, vpp1alamat3, 
        'vpp2alamat1, vpp2alamat2, vpp2alamat3, vppbagianpembayaran, vppuraian, vppcatatan, vppnoref, 
        'vpptglnoref, vppcarabayar, vpptglbayar, vppmatauang, vppkurs, vpptotalap, vpptotalapvalas, 
        'vpptotalar, vpptotalarvalas, vppbayar, vppbayarvalas, vppselisihkurs, vpprekselisihkurs, vppdiskontermin, 
        'vppdiskonterminvalas, vpprekdiskontermin, vppstatusvp, vppstatus, vppstatussebelumnya, vppjmlrevisi, vppcetakanke, 
        'vppinputuser, vppinputtgl, vppmodifikasiuser, vppmodifikasitgl, vppposting, vpppostingtgl, vppisclose, 
        'vppcabangnama, vpplokasinama, vppgudangnama, vppsupplierkode, vppsuppliernama, vppbagianpembayarankode, vppbagianpembayarannama, 
        'vppcarabayarnama, vpprekselisihkursnama, vpprekdiskonterminnama, vppstatusnama, vppstatussebelumnyanama, vppinputusernama, vppmodifikasiusernama

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
            Filter = Filter.Replace("vppsupplierkode", "c1.kkode")
            Filter = Filter.Replace("vppsuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_vpp_v_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Vpp_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("vppid"), 0), sptField,
                     FxDB(dr("vppidhistory"), 0), sptField,
                     FxDB(dr("vppcabang"), ""), sptField,
                     FxDB(dr("vpplokasi"), ""), sptField,
                     FxDB(dr("vppgudang"), ""), sptField,
                     FxDB(dr("vppsumber"), ""), sptField,
                     FxDB(dr("vppautonotransaksi"), 0), sptField,
                     FxDB(dr("vppnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("vpptgl"), ""), formatTgl), sptField,
                     FxDB(dr("vppkodepa"), 0), sptField,
                     FxDB(dr("vppsupplier"), 0), sptField,
                     FxDB(dr("vppsupplierkontak"), ""), sptField,
                     FxDB(dr("vpp1alamat1"), ""), sptField,
                     FxDB(dr("vpp1alamat2"), ""), sptField,
                     FxDB(dr("vpp1alamat3"), ""), sptField,
                     FxDB(dr("vpp2alamat1"), ""), sptField,
                     FxDB(dr("vpp2alamat2"), ""), sptField,
                     FxDB(dr("vpp2alamat3"), ""), sptField,
                     FxDB(dr("vppbagianpembayaran"), 0), sptField,
                     FxDB(dr("vppuraian"), ""), sptField,
                     FxDB(dr("vppcatatan"), ""), sptField,
                     FxDB(dr("vppnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("vpptglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("vppcarabayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("vpptglbayar"), ""), formatTgl), sptField,
                     FxDB(dr("vppmatauang"), ""), sptField,
                     FxDB(dr("vppkurs"), 0), sptField,
                     FxDB(dr("vpptotalap"), 0), sptField,
                     FxDB(dr("vpptotalapvalas"), 0), sptField,
                     FxDB(dr("vpptotalar"), 0), sptField,
                     FxDB(dr("vpptotalarvalas"), 0), sptField,
                     FxDB(dr("vppbayar"), 0), sptField,
                     FxDB(dr("vppbayarvalas"), 0), sptField,
                     FxDB(dr("vppselisihkurs"), 0), sptField,
                     FxDB(dr("vpprekselisihkurs"), ""), sptField,
                     FxDB(dr("vppdiskontermin"), 0), sptField,
                     FxDB(dr("vppdiskonterminvalas"), 0), sptField,
                     FxDB(dr("vpprekdiskontermin"), ""), sptField,
                     FxDB(dr("vppstatusvp"), 0), sptField,
                     FxDB(dr("vppstatus"), 0), sptField,
                     FxDB(dr("vppstatussebelumnya"), 0), sptField,
                     FxDB(dr("vppjmlrevisi"), 0), sptField,
                     FxDB(dr("vppcetakanke"), 0), sptField,
                     FxDB(dr("vppinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("vppinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("vppmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("vppmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("vppposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("vpppostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("vppisclose"), 0), sptField,
                     FxDB(dr("vppcabangnama"), ""), sptField,
                     FxDB(dr("vpplokasinama"), ""), sptField,
                     FxDB(dr("vppgudangnama"), ""), sptField,
                     FxDB(dr("vppsupplierkode"), ""), sptField,
                     FxDB(dr("vppsuppliernama"), ""), sptField,
                     FxDB(dr("vppbagianpembayarankode"), ""), sptField,
                     FxDB(dr("vppbagianpembayarannama"), ""), sptField,
                     FxDB(dr("vppcarabayarnama"), ""), sptField,
                     FxDB(dr("vpprekselisihkursnama"), ""), sptField,
                     FxDB(dr("vpprekdiskonterminnama"), ""), sptField,
                     FxDB(dr("vppstatusnama"), ""), sptField,
                     FxDB(dr("vppstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("vppinputusernama"), ""), sptField,
                     FxDB(dr("vppmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("vppidhistory, vppid, vppcabang, vpplokasi, vppgudang, vppsumber, vppautonotransaksi, vppnotransaksi, vpptgl, vppkodepa, vppsupplier, vppsupplierkontak, vpp1alamat1, vpp1alamat2, vpp1alamat3, vpp2alamat1, vpp2alamat2, vpp2alamat3, vppbagianpembayaran, vppuraian, vppcatatan, vppnoref, vpptglnoref, vppcarabayar, vpptglbayar, vppmatauang, vppkurs, vpptotalap, vpptotalapvalas, vpptotalar, vpptotalarvalas, vppbayar, vppbayarvalas, vppselisihkurs, vpprekselisihkurs, vppdiskontermin, vppdiskonterminvalas, vpprekdiskontermin, vppstatusvp, vppstatus, vppstatussebelumnya, vppjmlrevisi, vppcetakanke, vppinputuser, vppinputtgl, vppmodifikasiuser, vppmodifikasitgl, vppposting, vpppostingtgl, vppisclose, vppcabangnama, vpplokasinama, vppgudangnama, vppsupplierkode, vppsuppliernama, vppbagianpembayarankode, vppbagianpembayarannama, vppcarabayarnama, vpprekselisihkursnama, vpprekdiskonterminnama, vppstatusnama, vppstatussebelumnyanama, vppinputusernama, vppmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_VppHistoryGetdataById(ByVal param As String) As String

        'M4_VppHistoryGetdataById Utama --------------------------------------------------------
        'vppidhistory, vppid, vppcabang, vpplokasi, vppgudang, vppsumber, vppautonotransaksi, vppnotransaksi, 
        'vpptgl, vppkodepa, vppsupplier, vppsupplierkontak, vpp1alamat1, vpp1alamat2, vpp1alamat3, 
        'vpp2alamat1, vpp2alamat2, vpp2alamat3, vppbagianpembayaran, vppuraian, vppcatatan, vppnoref, 
        'vpptglnoref, vppcarabayar, vpptglbayar, vppmatauang, vppkurs, vpptotalap, vpptotalapvalas, 
        'vpptotalar, vpptotalarvalas, vppbayar, vppbayarvalas, vppselisihkurs, vpprekselisihkurs, vppdiskontermin, 
        'vppdiskonterminvalas, vpprekdiskontermin, vppstatusvp, vppstatus, vppstatussebelumnya, vppjmlrevisi, vppcetakanke, 
        'vppinputuser, vppinputtgl, vppmodifikasiuser, vppmodifikasitgl, vppposting, vpppostingtgl, vppisclose, 
        'vppcustomtext1, vppcustomtext2, vppcustomtext3, vppcustomtext4, vppcustomtext5, vppcustomint1, vppcustomint2, 
        'vppcustomint3, vppcustomdbl1, vppcustomdbl2, vppcustomdbl3, vppcustomdate1, vppcustomdate2, vppcustomdate3, 
        'vppcabangnama, vpplokasinama, vppgudangnama, vppsupplierkode, vppsuppliernama, vppbagianpembayarankode, vppbagianpembayarannama, 
        'vppcarabayarnama, vpprekselisihkursnama, vpprekdiskonterminnama, vppstatusnama, vppstatussebelumnyanama, vppinputusernama, vppmodifikasiusernama

        'M4_VppHistoryGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idvppdetail, idvpp, 
        'sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, 
        'jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, rekhutangpiutang, catatan, 
        'costcenter, divisi, subdivisi, proyek, jmlvp, jmlvpvalas, statusvp, 
        'urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, 
        'termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, 
        'haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, notransaksivpp, 
        'inputtgl

        'M4_VppHistoryGetdataById Pay -------------------------------------------------------
        'idhistorycarabayar, idhistory, idvppcarabayar, idvpp, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, jmlvp, jmlvpvalas, statusvp, isclose, carabayarnama, banknama, 
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

        Dim utama As String = "", detail As String = "", pay As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M4_Vpp_history~M4_Vpp_Detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "vppidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "vppidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_vpp_getdata_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("vppidhistory"), 0), sptField,
                     FxDB(drutama("vppid"), 0), sptField,
                     FxDB(drutama("vppcabang"), ""), sptField,
                     FxDB(drutama("vpplokasi"), ""), sptField,
                     FxDB(drutama("vppgudang"), ""), sptField,
                     FxDB(drutama("vppsumber"), ""), sptField,
                     FxDB(drutama("vppautonotransaksi"), 0), sptField,
                     FxDB(drutama("vppnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("vpptgl"), ""), formatTgl), sptField,
                     FxDB(drutama("vppkodepa"), 0), sptField,
                     FxDB(drutama("vppsupplier"), 0), sptField,
                     FxDB(drutama("vppsupplierkontak"), ""), sptField,
                     FxDB(drutama("vpp1alamat1"), ""), sptField,
                     FxDB(drutama("vpp1alamat2"), ""), sptField,
                     FxDB(drutama("vpp1alamat3"), ""), sptField,
                     FxDB(drutama("vpp2alamat1"), ""), sptField,
                     FxDB(drutama("vpp2alamat2"), ""), sptField,
                     FxDB(drutama("vpp2alamat3"), ""), sptField,
                     FxDB(drutama("vppbagianpembayaran"), 0), sptField,
                     FxDB(drutama("vppuraian"), ""), sptField,
                     FxDB(drutama("vppcatatan"), ""), sptField,
                     FxDB(drutama("vppnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("vpptglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("vppcarabayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vpptglbayar"), ""), formatTgl), sptField,
                     FxDB(drutama("vppmatauang"), ""), sptField,
                     FxDB(drutama("vppkurs"), 0), sptField,
                     FxDB(drutama("vpptotalap"), 0), sptField,
                     FxDB(drutama("vpptotalapvalas"), 0), sptField,
                     FxDB(drutama("vpptotalar"), 0), sptField,
                     FxDB(drutama("vpptotalarvalas"), 0), sptField,
                     FxDB(drutama("vppbayar"), 0), sptField,
                     FxDB(drutama("vppbayarvalas"), 0), sptField,
                     FxDB(drutama("vppselisihkurs"), 0), sptField,
                     FxDB(drutama("vpprekselisihkurs"), ""), sptField,
                     FxDB(drutama("vppdiskontermin"), 0), sptField,
                     FxDB(drutama("vppdiskonterminvalas"), 0), sptField,
                     FxDB(drutama("vpprekdiskontermin"), ""), sptField,
                     FxDB(drutama("vppstatusvp"), 0), sptField,
                     FxDB(drutama("vppstatus"), 0), sptField,
                     FxDB(drutama("vppstatussebelumnya"), 0), sptField,
                     FxDB(drutama("vppjmlrevisi"), 0), sptField,
                     FxDB(drutama("vppcetakanke"), 0), sptField,
                     FxDB(drutama("vppinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vppinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("vppmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vppmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("vppposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vpppostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("vppisclose"), 0), sptField,
                     FxDB(drutama("vppcustomtext1"), ""), sptField,
                     FxDB(drutama("vppcustomtext2"), ""), sptField,
                     FxDB(drutama("vppcustomtext3"), ""), sptField,
                     FxDB(drutama("vppcustomtext4"), ""), sptField,
                     FxDB(drutama("vppcustomtext5"), ""), sptField,
                     FxDB(drutama("vppcustomint1"), 0), sptField,
                     FxDB(drutama("vppcustomint2"), 0), sptField,
                     FxDB(drutama("vppcustomint3"), 0), sptField,
                     FxDB(drutama("vppcustomdbl1"), 0), sptField,
                     FxDB(drutama("vppcustomdbl2"), 0), sptField,
                     FxDB(drutama("vppcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vppcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("vppcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("vppcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("vppcabangnama"), ""), sptField,
                     FxDB(drutama("vpplokasinama"), ""), sptField,
                     FxDB(drutama("vppgudangnama"), ""), sptField,
                     FxDB(drutama("vppsupplierkode"), ""), sptField,
                     FxDB(drutama("vppsuppliernama"), ""), sptField,
                     FxDB(drutama("vppbagianpembayarankode"), ""), sptField,
                     FxDB(drutama("vppbagianpembayarannama"), ""), sptField,
                     FxDB(drutama("vppcarabayarnama"), ""), sptField,
                     FxDB(drutama("vpprekselisihkursnama"), ""), sptField,
                     FxDB(drutama("vpprekdiskonterminnama"), ""), sptField,
                     FxDB(drutama("vppstatusnama"), ""), sptField,
                     FxDB(drutama("vppstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("vppinputusernama"), ""), sptField,
                     FxDB(drutama("vppmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField, FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idvppdetail"), 0), sptField,
                     FxDB(dr("idvpp"), 0), sptField,
                     FxDB(dr("sumber"), ""), sptField,
                     FxDB(dr("idtransaksi"), 0), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("totaltransaksi"), 0), sptField,
                     FxDB(dr("terbayar"), 0), sptField,
                     FxDB(dr("sisa"), 0), sptField,
                     FxDB(dr("jmlbayar"), 0), sptField,
                     FxDB(dr("jmlbayarvalas"), 0), sptField,
                     FxDB(dr("diskontermin"), ""), sptField,
                     FxDB(dr("jmldiskontermin"), 0), sptField,
                     FxDB(dr("jmldiskonterminvalas"), 0), sptField,
                     FxDB(dr("rekhutangpiutang"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("jmlvp"), 0), sptField,
                     FxDB(dr("jmlvpvalas"), 0), sptField,
                     FxDB(dr("statusvp"), 0), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     FxDB(dr("notransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                     FxDB(dr("carabayar"), 0), sptField,
                     FxDB(dr("termin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("rencana"), 0), sptField,
                     FxDB(dr("statuslunas"), 0), sptField,
                     FxDB(dr("diskon1"), 0), sptField,
                     FxDB(dr("haridiskon1"), 0), sptField,
                     FxDB(dr("diskon2"), 0), sptField,
                     FxDB(dr("haridiskon2"), 0), sptField,
                     FxDB(dr("rekhutangpiutangnama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("notransaksivpp"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA PAY
            sql = query.PanggilQuery("m4_vpp_getdata_pay_history")
            Dim dtpay As New DataTable
            dtpay = AmbilData("aplikasi1-m4_vpp_getdata_pay_history", "idhistory='" & idtransaksi & "'", "urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtpay.Rows
                pay = String.Concat(pay, FxDB(dr("idhistorycarabayar"), 0), sptField, FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idvppcarabayar"), 0), sptField,
                     FxDB(dr("idvpp"), 0), sptField,
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
                     FxDB(dr("jmlvp"), 0), sptField,
                     FxDB(dr("jmlvpvalas"), 0), sptField,
                     FxDB(dr("statusvp"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("carabayarnama"), ""), sptField,
                     FxDB(dr("banknama"), ""), sptField,
                     FxDB(dr("rekbanknama"), ""), sptField,
                     FxDB(dr("rekgironama"), ""), sptRow)
            Next
            If pay.Length > 0 Then pay = pay.Substring(0, pay.Length - sptRow.Length) Else pay = pay

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, pay)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("vppidhistory, vppid, vppcabang, vpplokasi, vppgudang, vppsumber, vppautonotransaksi, vppnotransaksi, vpptgl, vppkodepa, vppsupplier, vppsupplierkontak, vpp1alamat1, vpp1alamat2, vpp1alamat3, vpp2alamat1, vpp2alamat2, vpp2alamat3, vppbagianpembayaran, vppuraian, vppcatatan, vppnoref, vpptglnoref, vppcarabayar, vpptglbayar, vppmatauang, vppkurs, vpptotalap, vpptotalapvalas, vpptotalar, vpptotalarvalas, vppbayar, vppbayarvalas, vppselisihkurs, vpprekselisihkurs, vppdiskontermin, vppdiskonterminvalas, vpprekdiskontermin, vppstatusvp, vppstatus, vppstatussebelumnya, vppjmlrevisi, vppcetakanke, vppinputuser, vppinputtgl, vppmodifikasiuser, vppmodifikasitgl, vppposting, vpppostingtgl, vppisclose, vppcustomtext1, vppcustomtext2, vppcustomtext3, vppcustomtext4, vppcustomtext5, vppcustomint1, vppcustomint2, vppcustomint3, vppcustomdbl1, vppcustomdbl2, vppcustomdbl3, vppcustomdate1, vppcustomdate2, vppcustomdate3, vppcabangnama, vpplokasinama, vppgudangnama, vppsupplierkode, vppsuppliernama, vppbagianpembayarankode, vppbagianpembayarannama, vppcarabayarnama, vpprekselisihkursnama, vpprekdiskonterminnama, vppstatusnama, vppstatussebelumnyanama, vppinputusernama, vppmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idvppdetail, idvpp, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, jmlvp, jmlvpvalas, statusvp, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, notransaksivpp, inputtgl" & sptSubParam & "idhistorycarabayar, idhistory, idvppcarabayar, idvpp, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, jmlvp, jmlvpvalas, statusvp, isclose, carabayarnama, banknama, rekbanknama, rekgironama"))

        Return wsResult
    End Function

End Class
