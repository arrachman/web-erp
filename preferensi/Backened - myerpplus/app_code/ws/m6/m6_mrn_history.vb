Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m6_mrn_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function m6_Mrn_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m6_mrn_history(SELECT 0, mrn.* FROM m6_mrn mrn WHERE mrn.mrnid = '" & idtransaksi & "')"
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
            sql = "SELECT mrnidhistory FROM m6_mrn_history WHERE mrnid = '" & idtransaksi & "' ORDER BY mrnmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY Out --------------------------------------
            sql = "INSERT INTO m6_mrn_out_history (SELECT 0, '" & result(4) & "', mrn.* FROM m6_mrn_out mrn WHERE mrn.idmrn = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY Out -------------------------------

            'PROSES INSERT HISTORY BATCH ---------------------------------------
            sql = "INSERT INTO m1_no_batch_transaction_history(SELECT 0, '" & result(4) & "', nb.* FROM m1_no_batch_transaction nb WHERE nb.nbtidtransaksi = '" & idtransaksi & "' and nb.nbtsumber = 'mrn')"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY BATCH --------------------------------

            'PROSES INSERT HISTORY SERIAL ---------------------------------------
            sql = "INSERT INTO m1_no_serial_transaction_history(SELECT 0, '" & result(4) & "', ns.* FROM m1_no_serial_transaction ns WHERE ns.nstidtransaksi = '" & idtransaksi & "' and ns.nstsumber = 'mrn')"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY SERIAL --------------------------------

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
    Public Function M6_Mrn_HistorySearch(ByVal param As String) As String
        'M6_Mrn_HistorySearch --------------------------------------------------------
        'mrnidhistory, mrnid, mrncabang, mrnlokasi, mrngudangasal, mrngudangproduksi, mrngudangtujuan, mrnsumber, 
        'mrnjenis, mrnautonotransaksi, mrnnotransaksi, mrntgl, mrnkodepa, mrnbagianmrn, mrnbagianmrnkontak, 
        'mrntgldipakai, mrnestimasikerja, mrnmatauang, mrnkurs, mrntotalhargain, mrntotalhargaout, mrntotalhppin, 
        'mrntotalhppout, mrnuraian, mrncatatan, mrnnoref, mrntglnoref, mrnidbom, mrnidpdr, 
        'mrnidwo, mrnidmrs, mrnstatuspdin, mrnstatuspdout, mrnstatusrealisasiin, mrnstatusrealisasiout, mrnstatus, 
        'mrnstatussebelumnya, mrnjmlrevisi, mrncetakanke, mrninputuser, mrninputtgl, mrnmodifikasiuser, mrnmodifikasitgl, 
        'mrnposting, mrnpostingtgl, mrnisclose, mrncabangnama, mrnlokasinama, mrngudangasalnama, mrngudangproduksinama, 
        'mrngudangtujuannama, mrnjenisnama, mrnbagianmrnkode, mrnbagianmrnnama, mrnestimasikerjanama, mrnnotransaksibom, mrnnotransaksipdr, 
        'mrnnotransaksiwo, mrnnotransaksimrs, mrnstatusnama, mrnstatussebelumnyanama, mrninputusernama, mrnmodifikasiusernama

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strplrt(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", sorting As String = ""
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
            sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m6_mrn_v_history")

        dt = AmbilData("aplikasi1-M5_pl_v", Filter, sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("mrnid"), 0), sptField,
                     FxDB(dr("mrnidhistory"), 0), sptField,
                     FxDB(dr("mrncabang"), ""), sptField,
                     FxDB(dr("mrnlokasi"), ""), sptField,
                     FxDB(dr("mrngudangasal"), ""), sptField,
                     FxDB(dr("mrngudangproduksi"), ""), sptField,
                     FxDB(dr("mrngudangtujuan"), ""), sptField,
                     FxDB(dr("mrnsumber"), ""), sptField,
                     FxDB(dr("mrnjenis"), ""), sptField,
                     FxDB(dr("mrnautonotransaksi"), 0), sptField,
                     FxDB(dr("mrnnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("mrntgl"), ""), formatTgl), sptField,
                     FxDB(dr("mrnkodepa"), 0), sptField,
                     FxDB(dr("mrnbagianmrn"), 0), sptField,
                     FxDB(dr("mrnbagianmrnkontak"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("mrntgldipakai"), ""), formatTgl), sptField,
                     FxDB(dr("mrnestimasikerja"), ""), sptField,
                     FxDB(dr("mrnmatauang"), ""), sptField,
                     FxDB(dr("mrnkurs"), 0), sptField,
                     FxDB(dr("mrntotalhargain"), 0), sptField,
                     FxDB(dr("mrntotalhargaout"), 0), sptField,
                     FxDB(dr("mrntotalhppin"), 0), sptField,
                     FxDB(dr("mrntotalhppout"), 0), sptField,
                     FxDB(dr("mrnuraian"), ""), sptField,
                     FxDB(dr("mrncatatan"), ""), sptField,
                     FxDB(dr("mrnnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("mrntglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("mrnidbom"), 0), sptField,
                     FxDB(dr("mrnidpdr"), 0), sptField,
                     FxDB(dr("mrnidwo"), 0), sptField,
                     FxDB(dr("mrnidmrs"), 0), sptField,
                     FxDB(dr("mrnstatuspdin"), 0), sptField,
                     FxDB(dr("mrnstatuspdout"), 0), sptField,
                     FxDB(dr("mrnstatusrealisasiin"), 0), sptField,
                     FxDB(dr("mrnstatusrealisasiout"), 0), sptField,
                     FxDB(dr("mrnstatus"), 0), sptField,
                     FxDB(dr("mrnstatussebelumnya"), 0), sptField,
                     FxDB(dr("mrnjmlrevisi"), 0), sptField,
                     FxDB(dr("mrncetakanke"), 0), sptField,
                     FxDB(dr("mrninputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("mrninputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("mrnmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("mrnmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("mrnposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("mrnpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("mrnisclose"), 0), sptField,
                     FxDB(dr("mrncabangnama"), ""), sptField,
                     FxDB(dr("mrnlokasinama"), ""), sptField,
                     FxDB(dr("mrngudangasalnama"), ""), sptField,
                     FxDB(dr("mrngudangproduksinama"), ""), sptField,
                     FxDB(dr("mrngudangtujuannama"), ""), sptField,
                     FxDB(dr("mrnjenisnama"), ""), sptField,
                     FxDB(dr("mrnbagianmrnkode"), ""), sptField,
                     FxDB(dr("mrnbagianmrnnama"), ""), sptField,
                     FxDB(dr("mrnestimasikerjanama"), ""), sptField,
                     FxDB(dr("mrnnotransaksibom"), ""), sptField,
                     FxDB(dr("mrnnotransaksipdr"), ""), sptField,
                     FxDB(dr("mrnnotransaksiwo"), ""), sptField,
                     FxDB(dr("mrnnotransaksimrs"), ""), sptField,
                     FxDB(dr("mrnstatusnama"), ""), sptField,
                     FxDB(dr("mrnstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("mrninputusernama"), ""), sptField,
                     FxDB(dr("mrnmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("mrnidhistory, mrnid, mrncabang, mrnlokasi, mrngudangasal, mrngudangproduksi, mrngudangtujuan, mrnsumber, mrnjenis, mrnautonotransaksi, mrnnotransaksi, mrntgl, mrnkodepa, mrnbagianmrn, mrnbagianmrnkontak, mrntgldipakai, mrnestimasikerja, mrnmatauang, mrnkurs, mrntotalhargain, mrntotalhargaout, mrntotalhppin, mrntotalhppout, mrnuraian, mrncatatan, mrnnoref, mrntglnoref, mrnidbom, mrnidpdr, mrnidwo, mrnidmrs, mrnstatuspdin, mrnstatuspdout, mrnstatusrealisasiin, mrnstatusrealisasiout, mrnstatus, mrnstatussebelumnya, mrnjmlrevisi, mrncetakanke, mrninputuser, mrninputtgl, mrnmodifikasiuser, mrnmodifikasitgl, mrnposting, mrnpostingtgl, mrnisclose, mrncabangnama, mrnlokasinama, mrngudangasalnama, mrngudangproduksinama, mrngudangtujuannama, mrnjenisnama, mrnbagianmrnkode, mrnbagianmrnnama, mrnestimasikerjanama, mrnnotransaksibom, mrnnotransaksipdr, mrnnotransaksiwo, mrnnotransaksimrs, mrnstatusnama, mrnstatussebelumnyanama, mrninputusernama, mrnmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_MrnHistoryGetdataById(ByVal param As String) As String
        'M6_MrnHistoryGetdataById Utama --------------------------------------------------------
        'mrnidhistory, mrnid, mrncabang, mrnlokasi, mrngudangasal, mrngudangproduksi, mrngudangtujuan, mrnsumber, 
        'mrnjenis, mrnautonotransaksi, mrnnotransaksi, mrntgl, mrnkodepa, mrnbagianmrn, mrnbagianmrnkontak, 
        'mrntgldipakai, mrnestimasikerja, mrnmatauang, mrnkurs, mrntotalhargain, mrntotalhargaout, mrntotalhppin, 
        'mrntotalhppout, mrnuraian, mrncatatan, mrnnoref, mrntglnoref, mrnidbom, mrnidpdr, 
        'mrnidwo, mrnidmrs, mrnstatuspdin, mrnstatuspdout, mrnstatusrealisasiin, mrnstatusrealisasiout, mrnstatus, 
        'mrnstatussebelumnya, mrnjmlrevisi, mrncetakanke, mrninputuser, mrninputtgl, mrnmodifikasiuser, mrnmodifikasitgl, 
        'mrnposting, mrnpostingtgl, mrnisclose, mrncustomtext1, mrncustomtext2, mrncustomtext3, mrncustomtext4, 
        'mrncustomtext5, mrncustomint1, mrncustomint2, mrncustomint3, mrncustomdbl1, mrncustomdbl2, mrncustomdbl3, 
        'mrncustomdate1, mrncustomdate2, mrncustomdate3, mrncabangnama, mrnlokasinama, mrngudangasalnama, mrngudangproduksinama, 
        'mrngudangtujuannama, mrnjenisnama, mrnbagianmrnkode, mrnbagianmrnnama, mrnestimasikerjanama, mrnnotransaksibom, mrnnotransaksipdr, 
        'mrnnotransaksiwo, mrnnotransaksimrs, mrnstatusnama, mrnstatussebelumnyanama, mrninputusernama, mrnmodifikasiusernama

        'M6_MrnHistoryGetdataById Out --------------------------------------------------------
        'idhistoryout, idhistory, idmrnout, idmrn, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, 
        'idhppkhususkeluar, idhppfifokeluar, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, 
        'gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idbomout, idpdrout, idwoout, idmrsout, jmlpd, statuspd, jmlrealisasi, 
        'statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, 
        'bserial, bbatch, costcenternama, divisinama, subdivisinama, proyeknama, notransaksi, 
        'bomnotransaksi, pdrnotransaksi, wonotransaksi, mrsnotransaksi, jmlsisapd, jmlsisarealisasi

        'M6_MrnHistoryGetdataById Batch --------------------------------------------------------
        'nbtidhistory, nbtidtransaksihistory, nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang

        'M6_MrnHistoryGetdataById Serial --------------------------------------------------------
        'nstidhistory, nstidtransaksihistory, nstid, nstjenismutasi, nstidserialin, nstgudang,  nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, 
        'nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, 
        'nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang

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

        Dim utama As String = "", detail As String = "", batch As String = "", serial As String = "", idtransaksi As String = ""
        Dim sumber As String = "MRN"

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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        Dim NmMemcached As String = "aplikasi1-M5_pl~M5_pl_Detail-" & idtransaksi
        Dim Filter2 As String = ""

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
            Filter = Filter.Replace("statusrealisasi", "mrno.statusrealisasi")

            Filter2 = pagingSplit(2)
            '#Taruh fungsi replace disini...
            Filter2 = Filter2.Replace("statusrealisasi", "mrno.statusrealisasi")
        End If

        'Set filter utama
        If Len(Filter) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "mrnidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "mrnidhistory = " & idtransaksi & " and " & Filter
        End If

        'Set filter detail 2
        If Len(Filter2) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter2 = "idmrn = '" & idtransaksi & "'"
        Else ' jika filter diisi
            Filter2 = "idmrn = '" & idtransaksi & "' and " & Filter2
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m6_mrn_getdata_history")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("mrnidhistory"), 0), sptField, FxDB(drutama("mrnid"), 0), sptField,
                     FxDB(drutama("mrncabang"), ""), sptField,
                     FxDB(drutama("mrnlokasi"), ""), sptField,
                     FxDB(drutama("mrngudangasal"), ""), sptField,
                     FxDB(drutama("mrngudangproduksi"), ""), sptField,
                     FxDB(drutama("mrngudangtujuan"), ""), sptField,
                     FxDB(drutama("mrnsumber"), ""), sptField,
                     FxDB(drutama("mrnjenis"), ""), sptField,
                     FxDB(drutama("mrnautonotransaksi"), 0), sptField,
                     FxDB(drutama("mrnnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("mrntgl"), ""), formatTgl), sptField,
                     FxDB(drutama("mrnkodepa"), 0), sptField,
                     FxDB(drutama("mrnbagianmrn"), 0), sptField,
                     FxDB(drutama("mrnbagianmrnkontak"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("mrntgldipakai"), ""), formatTgl), sptField,
                     FxDB(drutama("mrnestimasikerja"), ""), sptField,
                     FxDB(drutama("mrnmatauang"), ""), sptField,
                     FxDB(drutama("mrnkurs"), 0), sptField,
                     FxDB(drutama("mrntotalhargain"), 0), sptField,
                     FxDB(drutama("mrntotalhargaout"), 0), sptField,
                     FxDB(drutama("mrntotalhppin"), 0), sptField,
                     FxDB(drutama("mrntotalhppout"), 0), sptField,
                     FxDB(drutama("mrnuraian"), ""), sptField,
                     FxDB(drutama("mrncatatan"), ""), sptField,
                     FxDB(drutama("mrnnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("mrntglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("mrnidbom"), 0), sptField,
                     FxDB(drutama("mrnidpdr"), 0), sptField,
                     FxDB(drutama("mrnidwo"), 0), sptField,
                     FxDB(drutama("mrnidmrs"), 0), sptField,
                     FxDB(drutama("mrnstatuspdin"), 0), sptField,
                     FxDB(drutama("mrnstatuspdout"), 0), sptField,
                     FxDB(drutama("mrnstatusrealisasiin"), 0), sptField,
                     FxDB(drutama("mrnstatusrealisasiout"), 0), sptField,
                     FxDB(drutama("mrnstatus"), 0), sptField,
                     FxDB(drutama("mrnstatussebelumnya"), 0), sptField,
                     FxDB(drutama("mrnjmlrevisi"), 0), sptField,
                     FxDB(drutama("mrncetakanke"), 0), sptField,
                     FxDB(drutama("mrninputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("mrninputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("mrnmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("mrnmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("mrnposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("mrnpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("mrnisclose"), 0), sptField,
                     FxDB(drutama("mrncustomtext1"), ""), sptField,
                     FxDB(drutama("mrncustomtext2"), ""), sptField,
                     FxDB(drutama("mrncustomtext3"), ""), sptField,
                     FxDB(drutama("mrncustomtext4"), ""), sptField,
                     FxDB(drutama("mrncustomtext5"), ""), sptField,
                     FxDB(drutama("mrncustomint1"), 0), sptField,
                     FxDB(drutama("mrncustomint2"), 0), sptField,
                     FxDB(drutama("mrncustomint3"), 0), sptField,
                     FxDB(drutama("mrncustomdbl1"), 0), sptField,
                     FxDB(drutama("mrncustomdbl2"), 0), sptField,
                     FxDB(drutama("mrncustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("mrncustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("mrncustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("mrncustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("mrncabangnama"), ""), sptField,
                     FxDB(drutama("mrnlokasinama"), ""), sptField,
                     FxDB(drutama("mrngudangasalnama"), ""), sptField,
                     FxDB(drutama("mrngudangproduksinama"), ""), sptField,
                     FxDB(drutama("mrngudangtujuannama"), ""), sptField,
                     FxDB(drutama("mrnjenisnama"), ""), sptField,
                     FxDB(drutama("mrnbagianmrnkode"), ""), sptField,
                     FxDB(drutama("mrnbagianmrnnama"), ""), sptField,
                     FxDB(drutama("mrnestimasikerjanama"), ""), sptField,
                     FxDB(drutama("mrnnotransaksibom"), ""), sptField,
                     FxDB(drutama("mrnnotransaksipdr"), ""), sptField,
                     FxDB(drutama("mrnnotransaksiwo"), ""), sptField,
                     FxDB(drutama("mrnnotransaksimrs"), ""), sptField,
                     FxDB(drutama("mrnstatusnama"), ""), sptField,
                     FxDB(drutama("mrnstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("mrninputusernama"), ""), sptField,
                     FxDB(drutama("mrnmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistoryout"), 0), sptField, FxDB(dr("idhistory"), 0), sptField, FxDB(dr("idmrnout"), 0), sptField,
                     FxDB(dr("idmrn"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("idhppkhususkeluar"), 0), sptField,
                     FxDB(dr("idhppfifokeluar"), 0), sptField,
                     FxDB(dr("rekpersediaan"), ""), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudangasal"), ""), sptField,
                     FxDB(dr("gudangproduksi"), ""), sptField,
                     FxDB(dr("gudangtujuan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idbomout"), 0), sptField,
                     FxDB(dr("idpdrout"), 0), sptField,
                     FxDB(dr("idwoout"), 0), sptField,
                     FxDB(dr("idmrsout"), 0), sptField,
                     FxDB(dr("jmlpd"), 0), sptField,
                     FxDB(dr("statuspd"), 0), sptField,
                     FxDB(dr("jmlrealisasi"), 0), sptField,
                     FxDB(dr("statusrealisasi"), 0), sptField,
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
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("notransaksi"), ""), sptField,
                     FxDB(dr("bomnotransaksi"), ""), sptField,
                     FxDB(dr("pdrnotransaksi"), ""), sptField,
                     FxDB(dr("wonotransaksi"), ""), sptField,
                     FxDB(dr("mrsnotransaksi"), ""), sptField,
                     FxDB(dr("jmlsisapd"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA BATCH
            sql = "select `nbt`.`nbtidhistory` AS `nbtidhistory`, `nbt`.`nbtidtransaksihistory` AS `nbtidtransaksihistory`,`nbt`.`nbtid` AS `nbtid`,`nbt`.`nbtjenismutasi` AS `nbtjenismutasi`,`nbt`.`nbtidbatchin` AS `nbtidbatchin`,`nbt`.`nbtgudang` AS `nbtgudang`,`nbt`.`nbtidbarang` AS `nbtidbarang`,`nbt`.`nbtkode` AS `nbtkode`,`nbt`.`nbtsumber` AS `nbtsumber`,`nbt`.`nbtidtransaksi` AS `nbtidtransaksi`,`nbt`.`nbtsatuan` AS `nbtsatuan`,`nbt`.`nbtjml` AS `nbtjml`,`nbt`.`nbtcustomtext1` AS `nbtcustomtext1`,`nbt`.`nbtcustomtext2` AS `nbtcustomtext2`,`nbt`.`nbtcustomtext3` AS `nbtcustomtext3`,`nbt`.`nbtcustomdbl1` AS `nbtcustomdbl1`,`nbt`.`nbtcustomdbl2` AS `nbtcustomdbl2`,`nbt`.`nbtcustomdbl3` AS `nbtcustomdbl3`,`nbt`.`nbtcustomdate1` AS `nbtcustomdate1`,`nbt`.`nbtcustomdate2` AS `nbtcustomdate2`,`nbt`.`nbtcustomdate3` AS `nbtcustomdate3`,`i`.`bkode` AS `kodebarang` from ((`m1_no_batch_transaction_history` `nbt` join `m1_item` `i` on((`nbt`.`nbtidbarang` = `i`.`bid`))) left join `m1_no_batch_in` `nbi` on((`nbt`.`nbtidbatchin` = `nbi`.`nbiidbatchin`)))"
            Dim dtbatch As New DataTable
            dtbatch = AmbilData("aplikasi1-m1_no_batch_out", "nbtidtransaksihistory = '" & idtransaksi & "' AND nbtsumber = '" & sumber & "' AND (nbtjenismutasi = 1 OR nbiidbarang IS NOT NULL)", "nbtidbarang, nbtkode ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtbatch.Rows
                batch = String.Concat(batch, FxDB(dr("nbtidhistory"), 0), sptField, FxDB(dr("nbtidtransaksihistory"), 0), sptField,
                     FxDB(dr("nbtid"), 0), sptField,
                     FxDB(dr("nbtjenismutasi"), 0), sptField,
                     FxDB(dr("nbtidbatchin"), 0), sptField,
                     FxDB(dr("nbtgudang"), ""), sptField,
                     FxDB(dr("nbtidbarang"), 0), sptField,
                     FxDB(dr("nbtkode"), ""), sptField,
                     FxDB(dr("nbtsumber"), ""), sptField,
                     FxDB(dr("nbtidtransaksi"), 0), sptField,
                     FxDB(dr("nbtsatuan"), ""), sptField,
                     FxDB(dr("nbtjml"), 0), sptField,
                     FxDB(dr("nbtcustomtext1"), ""), sptField,
                     FxDB(dr("nbtcustomtext2"), ""), sptField,
                     FxDB(dr("nbtcustomtext3"), ""), sptField,
                     FxDB(dr("nbtcustomdbl1"), 0), sptField,
                     FxDB(dr("nbtcustomdbl2"), 0), sptField,
                     FxDB(dr("nbtcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("nbtcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("nbtcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("nbtcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kodebarang"), ""), sptRow)
            Next
            If batch.Length > 0 Then batch = batch.Substring(0, batch.Length - sptRow.Length) Else batch = batch

            'AMBIL DATA SERIAL
            sql = "select `nst`.`nstidhistory` AS `nstidhistory`,`nst`.`nstidtransaksihistory` AS `nstidtransaksihistory`,`nst`.`nstid` AS `nstid`,`nst`.`nstjenismutasi` AS `nstjenismutasi`,`nst`.`nstidserialin` AS `nstidserialin`,`nst`.`nstgudang` AS `nstgudang`,`nst`.`nstidbarang` AS `nstidbarang`,`nst`.`nstkode` AS `nstkode`,`nst`.`nstsumber` AS `nstsumber`,`nst`.`nstidtransaksi` AS `nstidtransaksi`,`nst`.`nstsatuan` AS `nstsatuan`,`nst`.`nstjml` AS `nstjml`,`nst`.`nstcustomtext1` AS `nstcustomtext1`,`nst`.`nstcustomtext2` AS `nstcustomtext2`,`nst`.`nstcustomtext3` AS `nstcustomtext3`,`nst`.`nstcustomdbl1` AS `nstcustomdbl1`,`nst`.`nstcustomdbl2` AS `nstcustomdbl2`,`nst`.`nstcustomdbl3` AS `nstcustomdbl3`,`nst`.`nstcustomdate1` AS `nstcustomdate1`,`nst`.`nstcustomdate2` AS `nstcustomdate2`,`nst`.`nstcustomdate3` AS `nstcustomdate3`,`i`.`bkode` AS `kodebarang` from ((`m1_no_serial_transaction_history` `nst` join `m1_item` `i` on((`nst`.`nstidbarang` = `i`.`bid`))) left join `m1_no_serial_in` `nsi` on((`nst`.`nstidserialin` = `nsi`.`nsiidserialin`)))"
            Dim dtserial As New DataTable
            dtserial = AmbilData("aplikasi1-m1_no_serial_out", "nstidtransaksihistory = '" & idtransaksi & "' AND nstsumber = '" & sumber & "' AND (nstjenismutasi = 1 OR nsiidbarang IS NOT NULL)", "nstidbarang, nstkode ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtserial.Rows
                serial = String.Concat(serial, FxDB(dr("nstidhistory"), 0), sptField, FxDB(dr("nstidtransaksihistory"), 0), sptField,
                     FxDB(dr("nstid"), 0), sptField,
                     FxDB(dr("nstjenismutasi"), 0), sptField,
                     FxDB(dr("nstidserialin"), 0), sptField,
                     FxDB(dr("nstgudang"), ""), sptField,
                     FxDB(dr("nstidbarang"), 0), sptField,
                     FxDB(dr("nstkode"), ""), sptField,
                     FxDB(dr("nstsumber"), ""), sptField,
                     FxDB(dr("nstidtransaksi"), 0), sptField,
                     FxDB(dr("nstsatuan"), ""), sptField,
                     FxDB(dr("nstjml"), 0), sptField,
                     FxDB(dr("nstcustomtext1"), ""), sptField,
                     FxDB(dr("nstcustomtext2"), ""), sptField,
                     FxDB(dr("nstcustomtext3"), ""), sptField,
                     FxDB(dr("nstcustomdbl1"), 0), sptField,
                     FxDB(dr("nstcustomdbl2"), 0), sptField,
                     FxDB(dr("nstcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("nstcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("nstcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("nstcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kodebarang"), ""), sptRow)
            Next
            If serial.Length > 0 Then serial = serial.Substring(0, serial.Length - sptRow.Length) Else serial = serial

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, batch, sptSubParam, serial)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("mrnidhistory, mrnid, mrncabang, mrnlokasi, mrngudangasal, mrngudangproduksi, mrngudangtujuan, mrnsumber, mrnjenis, mrnautonotransaksi, mrnnotransaksi, mrntgl, mrnkodepa, mrnbagianmrn, mrnbagianmrnkontak, mrntgldipakai, mrnestimasikerja, mrnmatauang, mrnkurs, mrntotalhargain, mrntotalhargaout, mrntotalhppin, mrntotalhppout, mrnuraian, mrncatatan, mrnnoref, mrntglnoref, mrnidbom, mrnidpdr, mrnidwo, mrnidmrs, mrnstatuspdin, mrnstatuspdout, mrnstatusrealisasiin, mrnstatusrealisasiout, mrnstatus, mrnstatussebelumnya, mrnjmlrevisi, mrncetakanke, mrninputuser, mrninputtgl, mrnmodifikasiuser, mrnmodifikasitgl, mrnposting, mrnpostingtgl, mrnisclose, mrncustomtext1, mrncustomtext2, mrncustomtext3, mrncustomtext4, mrncustomtext5, mrncustomint1, mrncustomint2, mrncustomint3, mrncustomdbl1, mrncustomdbl2, mrncustomdbl3, mrncustomdate1, mrncustomdate2, mrncustomdate3, mrncabangnama, mrnlokasinama, mrngudangasalnama, mrngudangproduksinama, mrngudangtujuannama, mrnjenisnama, mrnbagianmrnkode, mrnbagianmrnnama, mrnestimasikerjanama, mrnnotransaksibom, mrnnotransaksipdr, mrnnotransaksiwo, mrnnotransaksimrs, mrnstatusnama, mrnstatussebelumnyanama, mrninputusernama, mrnmodifikasiusernama" & sptSubParam & "idhistoryout, idhistory, idmrnout, idmrn, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, idhppkhususkeluar, idhppfifokeluar, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomout, idpdrout, idwoout, idmrsout, jmlpd, statuspd, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, subdivisinama, proyeknama, notransaksi, bomnotransaksi, pdrnotransaksi, wonotransaksi, mrsnotransaksi, jmlsisapd, jmlsisarealisasi" & sptSubParam & "nbtidhistory, nbtidtransaksihistory, nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang" & sptSubParam & "nstidhistory, nstidtransaksihistory, nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang"))

        Return wsResult
    End Function

End Class
