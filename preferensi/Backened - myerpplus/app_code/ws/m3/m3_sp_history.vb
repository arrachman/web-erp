Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m3_sp_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M3_Sp_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m3_sp_history(SELECT 0, sp.* FROM m3_sp sp WHERE sp.spid = '" & idtransaksi & "')"
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
            sql = "SELECT spidhistory FROM m3_sp_history WHERE spid = '" & idtransaksi & "' ORDER BY spmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m3_sp_detail_history (SELECT 0, '" & result(4) & "', sp.* FROM m3_sp_detail sp WHERE sp.idsp = '" & idtransaksi & "' )"
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
    Public Function M3_Sp_HistorySearch(ByVal param As String) As String
        'M3_Sp_HistorySearch --------------------------------------------------------
        'spidhistory, spid, spcabang, splokasi, spgudang, spsumber, spautonotransaksi, spnotransaksi, 
        'sptgl, spkodepa, spbagiansp, spbagianspkontak, spuraian, spcatatan, spnoref, 
        'sptglnoref, spstatussa, spstatus, spstatussebelumnya, spjmlrevisi, spcetakanke, spinputuser, 
        'spinputtgl, spmodifikasiuser, spmodifikasitgl, spposting, sppostingtgl, sptutupperiode, spisclose, 
        'spcabangnama, splokasinama, spgudangnama, spbagianspkode, spbagianspnama, spstatusnama, spstatussebelumnyanama, 
        'spinputusernama, spmodifikasiusernama, spstepke

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
            Filter = Filter.Replace("spbagianspkode", "c1.kkode")
            Filter = Filter.Replace("spbagianspnama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m3_sp_v_history")
        sql = "select `sp`.`spidhistory` AS `spidhistory`, `sp`.`spid` AS `spid`,`sp`.`spcabang` AS `spcabang`,`sp`.`splokasi` AS `splokasi`,`sp`.`spgudang` AS `spgudang`,`sp`.`spsumber` AS `spsumber`,`sp`.`spautonotransaksi` AS `spautonotransaksi`,`sp`.`spnotransaksi` AS `spnotransaksi`,`sp`.`sptgl` AS `sptgl`,`sp`.`spkodepa` AS `spkodepa`,`sp`.`spbagiansp` AS `spbagiansp`,`sp`.`spbagianspkontak` AS `spbagianspkontak`,`sp`.`spuraian` AS `spuraian`,`sp`.`spcatatan` AS `spcatatan`,`sp`.`spnoref` AS `spnoref`,`sp`.`sptglnoref` AS `sptglnoref`,`sp`.`spstatussa` AS `spstatussa`,`sp`.`spstatus` AS `spstatus`,`sp`.`spstatussebelumnya` AS `spstatussebelumnya`,`sp`.`spjmlrevisi` AS `spjmlrevisi`,`sp`.`spcetakanke` AS `spcetakanke`,`sp`.`spinputuser` AS `spinputuser`,`sp`.`spinputtgl` AS `spinputtgl`,`sp`.`spmodifikasiuser` AS `spmodifikasiuser`,`sp`.`spmodifikasitgl` AS `spmodifikasitgl`,`sp`.`spposting` AS `spposting`,`sp`.`sppostingtgl` AS `sppostingtgl`,`sp`.`sptutupperiode` AS `sptutupperiode`,`sp`.`spisclose` AS `spisclose`,`br`.`bnama` AS `spcabangnama`,`lc`.`lnama` AS `splokasinama`,`wh`.`wnama` AS `spgudangnama`,`c1`.`kkode` AS `spbagianspkode`,`c1`.`knama` AS `spbagianspnama`,`st1`.`nama` AS `spstatusnama`,`st2`.`nama` AS `spstatussebelumnyanama`,`u1`.`unama` AS `spinputusernama`,`u2`.`unama` AS `spmodifikasiusernama`, sp.spstepke from ((((((((`m3_sp_history` `sp` left join `m1_branch` `br` on((`br`.`bkode` = `sp`.`spcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `sp`.`splokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `sp`.`spgudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `sp`.`spbagiansp`))) left join `m0_status` `st1` on((`st1`.`kode` = `sp`.`spstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `sp`.`spstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `sp`.`spinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `sp`.`spmodifikasiuser`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M3_Sp_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("spid"), 0), sptField,
                     FxDB(dr("spidhistory"), 0), sptField,
                     FxDB(dr("spcabang"), ""), sptField,
                     FxDB(dr("splokasi"), ""), sptField,
                     FxDB(dr("spgudang"), ""), sptField,
                     FxDB(dr("spsumber"), ""), sptField,
                     FxDB(dr("spautonotransaksi"), 0), sptField,
                     FxDB(dr("spnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sptgl"), ""), formatTgl), sptField,
                     FxDB(dr("spkodepa"), 0), sptField,
                     FxDB(dr("spbagiansp"), 0), sptField,
                     FxDB(dr("spbagianspkontak"), ""), sptField,
                     FxDB(dr("spuraian"), ""), sptField,
                     FxDB(dr("spcatatan"), ""), sptField,
                     FxDB(dr("spnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sptglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("spstatussa"), 0), sptField,
                     FxDB(dr("spstatus"), 0), sptField,
                     FxDB(dr("spstatussebelumnya"), 0), sptField,
                     FxDB(dr("spjmlrevisi"), 0), sptField,
                     FxDB(dr("spcetakanke"), 0), sptField,
                     FxDB(dr("spinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("spinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("spmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("spmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("spposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sppostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sptutupperiode"), 0), sptField,
                     FxDB(dr("spisclose"), 0), sptField,
                     FxDB(dr("spcabangnama"), ""), sptField,
                     FxDB(dr("splokasinama"), ""), sptField,
                     FxDB(dr("spgudangnama"), ""), sptField,
                     FxDB(dr("spbagianspkode"), ""), sptField,
                     FxDB(dr("spbagianspnama"), ""), sptField,
                     FxDB(dr("spstatusnama"), ""), sptField,
                     FxDB(dr("spstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("spinputusernama"), ""), sptField,
                     FxDB(dr("spmodifikasiusernama"), ""), sptField,
                     FxDB(dr("spstepke"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("spidhistory, spid, spcabang, splokasi, spgudang, spsumber, spautonotransaksi, spnotransaksi, sptgl, spkodepa, spbagiansp, spbagianspkontak, spuraian, spcatatan, spnoref, sptglnoref, spstatussa, spstatus, spstatussebelumnya, spjmlrevisi, spcetakanke, spinputuser, spinputtgl, spmodifikasiuser, spmodifikasitgl, spposting, sppostingtgl, sptutupperiode, spisclose, spcabangnama, splokasinama, spgudangnama, spbagianspkode, spbagianspnama, spstatusnama, spstatussebelumnyanama, spinputusernama, spmodifikasiusernama, spstepke"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_SpHistoryGetdataById(ByVal param As String) As String

        'M3_SpHistoryGetdataById Utama --------------------------------------------------------
        'spidhistory, spid, spcabang, splokasi, spgudang, spsumber, spautonotransaksi, spnotransaksi, 
        'sptgl, spkodepa, spbagiansp, spbagianspkontak, spuraian, spcatatan, spnoref, 
        'sptglnoref, spstatussa, spstatus, spstatussebelumnya, spjmlrevisi, spcetakanke, spinputuser, 
        'spinputtgl, spmodifikasiuser, spmodifikasitgl, spposting, sppostingtgl, sptutupperiode, spisclose, 
        'spcustomtext1, spcustomtext2, spcustomtext3, spcustomtext4, spcustomtext5, spcustomint1, spcustomint2, 
        'spcustomint3, spcustomdbl1, spcustomdbl2, spcustomdbl3, spcustomdate1, spcustomdate2, spcustomdate3, 
        'spcabangnama, splokasinama, spgudangnama, spbagianspkode, spbagianspnama, spstatusnama, spstatussebelumnyanama, 
        'spinputusernama, spmodifikasiusernama, spstepke

        'M3_SpHistoryGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idspdetail, idsp, idbarang, namabarang, tipebarang, 
        'jmlsistem, jmlfisik, jmlbagus, jmlrusak, selisih, satuan, nilaisatuan, 
        'jmlbarangsistem, jmlbarangfisik, jmlbarangbagus, jmlbarangrusak, selisihbarang, satuanbarang, cabang, 
        'lokasi, gudang, lokasibarang, jmlsa, statussa, costcenter, divisi, 
        'subdivisi, proyek, catatan, urutan, isclose, customtext1, customtext2, 
        'customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'kodebarang, cabangnama, lokasinama, gudangnama, lokasibarangnama, costcenternama, divisinama, 
        'subdivisinama, proyeknama


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

        Dim NmMemcached As String = "aplikasi1-M3_Sp_history~M3_Sp_Detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "spidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "spidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m3_sp_getdata_history")
        sql = "select `sp`.`spidhistory` AS `spidhistory`,`sp`.`spid` AS `spid`,`sp`.`spcabang` AS `spcabang`,`sp`.`splokasi` AS `splokasi`,`sp`.`spgudang` AS `spgudang`,`sp`.`spsumber` AS `spsumber`,`sp`.`spautonotransaksi` AS `spautonotransaksi`,`sp`.`spnotransaksi` AS `spnotransaksi`,`sp`.`sptgl` AS `sptgl`,`sp`.`spkodepa` AS `spkodepa`,`sp`.`spbagiansp` AS `spbagiansp`,`sp`.`spbagianspkontak` AS `spbagianspkontak`,`sp`.`spuraian` AS `spuraian`,`sp`.`spcatatan` AS `spcatatan`,`sp`.`spnoref` AS `spnoref`,`sp`.`sptglnoref` AS `sptglnoref`,`sp`.`spstatussa` AS `spstatussa`,`sp`.`spstatus` AS `spstatus`,`sp`.`spstatussebelumnya` AS `spstatussebelumnya`,`sp`.`spjmlrevisi` AS `spjmlrevisi`,`sp`.`spcetakanke` AS `spcetakanke`,`sp`.`spinputuser` AS `spinputuser`,`sp`.`spinputtgl` AS `spinputtgl`,`sp`.`spmodifikasiuser` AS `spmodifikasiuser`,`sp`.`spmodifikasitgl` AS `spmodifikasitgl`,`sp`.`spposting` AS `spposting`,`sp`.`sppostingtgl` AS `sppostingtgl`,`sp`.`sptutupperiode` AS `sptutupperiode`,`sp`.`spisclose` AS `spisclose`,`sp`.`spcustomtext1` AS `spcustomtext1`,`sp`.`spcustomtext2` AS `spcustomtext2`,`sp`.`spcustomtext3` AS `spcustomtext3`,`sp`.`spcustomtext4` AS `spcustomtext4`,`sp`.`spcustomtext5` AS `spcustomtext5`,`sp`.`spcustomint1` AS `spcustomint1`,`sp`.`spcustomint2` AS `spcustomint2`,`sp`.`spcustomint3` AS `spcustomint3`,`sp`.`spcustomdbl1` AS `spcustomdbl1`,`sp`.`spcustomdbl2` AS `spcustomdbl2`,`sp`.`spcustomdbl3` AS `spcustomdbl3`,`sp`.`spcustomdate1` AS `spcustomdate1`,`sp`.`spcustomdate2` AS `spcustomdate2`,`sp`.`spcustomdate3` AS `spcustomdate3`,`br`.`bnama` AS `spcabangnama`,`lc`.`lnama` AS `splokasinama`,`wh`.`wnama` AS `spgudangnama`,`c1`.`kkode` AS `spbagianspkode`,`c1`.`knama` AS `spbagianspnama`,`st1`.`nama` AS `spstatusnama`,`st2`.`nama` AS `spstatussebelumnyanama`,`u1`.`unama` AS `spinputusernama`,`u2`.`unama` AS `spmodifikasiusernama`,`spd`.`idhistorydetail` AS `idhistorydetail`,`spd`.`idhistory` AS `idhistory`,`spd`.`idspdetail` AS `idspdetail`,`spd`.`idsp` AS `idsp`,`spd`.`idbarang` AS `idbarang`,`spd`.`namabarang` AS `namabarang`,`spd`.`tipebarang` AS `tipebarang`,`spd`.`jmlsistem` AS `jmlsistem`,`spd`.`jmlfisik` AS `jmlfisik`,`spd`.`jmlbagus` AS `jmlbagus`,`spd`.`jmlrusak` AS `jmlrusak`,`spd`.`selisih` AS `selisih`,`spd`.`satuan` AS `satuan`,`spd`.`nilaisatuan` AS `nilaisatuan`,`spd`.`jmlbarangsistem` AS `jmlbarangsistem`,`spd`.`jmlbarangfisik` AS `jmlbarangfisik`,`spd`.`jmlbarangbagus` AS `jmlbarangbagus`,`spd`.`jmlbarangrusak` AS `jmlbarangrusak`,`spd`.`selisihbarang` AS `selisihbarang`,`spd`.`satuanbarang` AS `satuanbarang`,`spd`.`cabang` AS `cabang`,`spd`.`lokasi` AS `lokasi`,`spd`.`gudang` AS `gudang`,`spd`.`lokasibarang` AS `lokasibarang`,`spd`.`jmlsa` AS `jmlsa`,`spd`.`statussa` AS `statussa`,`spd`.`costcenter` AS `costcenter`,`spd`.`divisi` AS `divisi`,`spd`.`subdivisi` AS `subdivisi`,`spd`.`proyek` AS `proyek`,`spd`.`catatan` AS `catatan`,`spd`.`urutan` AS `urutan`,`spd`.`isclose` AS `isclose`,`spd`.`customtext1` AS `customtext1`,`spd`.`customtext2` AS `customtext2`,`spd`.`customtext3` AS `customtext3`,`spd`.`customdbl1` AS `customdbl1`,`spd`.`customdbl2` AS `customdbl2`,`spd`.`customdbl3` AS `customdbl3`,`spd`.`customdate1` AS `customdate1`,`spd`.`customdate2` AS `customdate2`,`spd`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`brd`.`bnama` AS `cabangnama`,`lcd`.`lnama` AS `lokasinama`,`whd`.`wnama` AS `gudangnama`,`il`.`ilnama` AS `lokasibarangnama`,`cc`.`ccnama` AS `costcenternama`,`d`.`dnama` AS `divisinama`,`sd`.`sdnama` AS `subdivisinama`,`p`.`pnama` AS `proyeknama`, sp.spstepke from ((((((((((((((((((`m3_sp_history` `sp` join `m3_sp_detail_history` `spd` on((`sp`.`spidhistory` = `spd`.`idhistory`))) left join `m1_branch` `br` on((`br`.`bkode` = `sp`.`spcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `sp`.`splokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `sp`.`spgudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `sp`.`spbagiansp`))) left join `m0_status` `st1` on((`st1`.`kode` = `sp`.`spstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `sp`.`spstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `sp`.`spinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `sp`.`spmodifikasiuser`))) left join `m1_item` `i` on((`i`.`bid` = `spd`.`idbarang`))) left join `m1_branch` `brd` on((`spd`.`cabang` = `brd`.`bkode`))) left join `m1_location` `lcd` on((`spd`.`lokasi` = `lcd`.`lkode`))) left join `m1_warehouse` `whd` on((`spd`.`gudang` = `whd`.`wkode`))) left join `m1_item_location` `il` on((`spd`.`lokasibarang` = `il`.`ilkode`))) left join `m1_cost_center` `cc` on((`spd`.`costcenter` = `cc`.`cckode`))) left join `m1_division` `d` on((`spd`.`divisi` = `d`.`dkode`))) left join `m1_subdivision` `sd` on((`spd`.`subdivisi` = `sd`.`sdkode`))) left join `m1_project` `p` on((`spd`.`proyek` = `p`.`pkode`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("spidhistory"), 0), sptField,
                     FxDB(drutama("spid"), 0), sptField,
                     FxDB(drutama("spcabang"), ""), sptField,
                     FxDB(drutama("splokasi"), ""), sptField,
                     FxDB(drutama("spgudang"), ""), sptField,
                     FxDB(drutama("spsumber"), ""), sptField,
                     FxDB(drutama("spautonotransaksi"), 0), sptField,
                     FxDB(drutama("spnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sptgl"), ""), formatTgl), sptField,
                     FxDB(drutama("spkodepa"), 0), sptField,
                     FxDB(drutama("spbagiansp"), 0), sptField,
                     FxDB(drutama("spbagianspkontak"), ""), sptField,
                     FxDB(drutama("spuraian"), ""), sptField,
                     FxDB(drutama("spcatatan"), ""), sptField,
                     FxDB(drutama("spnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sptglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("spstatussa"), 0), sptField,
                     FxDB(drutama("spstatus"), 0), sptField,
                     FxDB(drutama("spstatussebelumnya"), 0), sptField,
                     FxDB(drutama("spjmlrevisi"), 0), sptField,
                     FxDB(drutama("spcetakanke"), 0), sptField,
                     FxDB(drutama("spinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("spinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("spmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("spmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("spposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sppostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sptutupperiode"), 0), sptField,
                     FxDB(drutama("spisclose"), 0), sptField,
                     FxDB(drutama("spcustomtext1"), ""), sptField,
                     FxDB(drutama("spcustomtext2"), ""), sptField,
                     FxDB(drutama("spcustomtext3"), ""), sptField,
                     FxDB(drutama("spcustomtext4"), ""), sptField,
                     FxDB(drutama("spcustomtext5"), ""), sptField,
                     FxDB(drutama("spcustomint1"), 0), sptField,
                     FxDB(drutama("spcustomint2"), 0), sptField,
                     FxDB(drutama("spcustomint3"), 0), sptField,
                     FxDB(drutama("spcustomdbl1"), 0), sptField,
                     FxDB(drutama("spcustomdbl2"), 0), sptField,
                     FxDB(drutama("spcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("spcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("spcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("spcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("spcabangnama"), ""), sptField,
                     FxDB(drutama("splokasinama"), ""), sptField,
                     FxDB(drutama("spgudangnama"), ""), sptField,
                     FxDB(drutama("spbagianspkode"), ""), sptField,
                     FxDB(drutama("spbagianspnama"), ""), sptField,
                     FxDB(drutama("spstatusnama"), ""), sptField,
                     FxDB(drutama("spstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("spinputusernama"), ""), sptField,
                     FxDB(drutama("spmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("spstepke"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idspdetail"), 0), sptField,
                     FxDB(dr("idsp"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jmlsistem"), 0), sptField,
                     FxDB(dr("jmlfisik"), 0), sptField,
                     FxDB(dr("jmlbagus"), 0), sptField,
                     FxDB(dr("jmlrusak"), 0), sptField,
                     FxDB(dr("selisih"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarangsistem"), 0), sptField,
                     FxDB(dr("jmlbarangfisik"), 0), sptField,
                     FxDB(dr("jmlbarangbagus"), 0), sptField,
                     FxDB(dr("jmlbarangrusak"), 0), sptField,
                     FxDB(dr("selisihbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("lokasibarang"), ""), sptField,
                     FxDB(dr("jmlsa"), 0), sptField,
                     FxDB(dr("statussa"), 0), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
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
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("cabangnama"), ""), sptField,
                     FxDB(dr("lokasinama"), ""), sptField,
                     FxDB(dr("gudangnama"), ""), sptField,
                     FxDB(dr("lokasibarangnama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("spidhistory, spid, spcabang, splokasi, spgudang, spsumber, spautonotransaksi, spnotransaksi, sptgl, spkodepa, spbagiansp, spbagianspkontak, spuraian, spcatatan, spnoref, sptglnoref, spstatussa, spstatus, spstatussebelumnya, spjmlrevisi, spcetakanke, spinputuser, spinputtgl, spmodifikasiuser, spmodifikasitgl, spposting, sppostingtgl, sptutupperiode, spisclose, spcustomtext1, spcustomtext2, spcustomtext3, spcustomtext4, spcustomtext5, spcustomint1, spcustomint2, spcustomint3, spcustomdbl1, spcustomdbl2, spcustomdbl3, spcustomdate1, spcustomdate2, spcustomdate3, spcabangnama, splokasinama, spgudangnama, spbagianspkode, spbagianspnama, spstatusnama, spstatussebelumnyanama, spinputusernama, spmodifikasiusernama, spstepke" & sptSubParam & "idhistorydetail, idhistory, idspdetail, idsp, idbarang, namabarang, tipebarang, jmlsistem, jmlfisik, jmlbagus, jmlrusak, selisih, satuan, nilaisatuan, jmlbarangsistem, jmlbarangfisik, jmlbarangbagus, jmlbarangrusak, selisihbarang, satuanbarang, cabang, lokasi, gudang, lokasibarang, jmlsa, statussa, costcenter, divisi, subdivisi, proyek, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, cabangnama, lokasinama, gudangnama, lokasibarangnama, costcenternama, divisinama, subdivisinama, proyeknama"))

        Return wsResult
    End Function

End Class