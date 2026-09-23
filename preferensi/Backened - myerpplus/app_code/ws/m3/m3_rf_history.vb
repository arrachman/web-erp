Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m3_rf_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M3_Rf_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m3_rf_history(SELECT 0, rf.* FROM m3_rf rf WHERE rf.rfid = '" & idtransaksi & "')"
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
            sql = "SELECT rfidhistory FROM m3_rf_history WHERE rfid = '" & idtransaksi & "' ORDER BY rfmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m3_rf_detail_history (SELECT 0, '" & result(4) & "', rf.* FROM m3_rf_detail rf WHERE rf.idrf = '" & idtransaksi & "' )"
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
    Public Function M3_Rf_HistorySearch(ByVal param As String) As String
        'M3_Rf_HistorySearch --------------------------------------------------------
        'rfidhistory, rfid, rfcabang, rflokasi, rfgudangasal, rfgudangtujuan, rfsumber, rfautonotransaksi, 
        'rfnotransaksi, rftgl, rfkodepa, rfdimintaoleh, rfdimintaolehkontak, rfmintake, rftgldipakai, 
        'rfuraian, rfcatatan, rfnoref, rftglnoref, rfstatusts, rfstatusrs, rfstatusrealisasi, 
        'rfstatus, rfstatussebelumnya, rfjmlrevisi, rfcetakanke, rfinputuser, rfinputtgl, rfmodifikasiuser, 
        'rfmodifikasitgl, rfposting, rfpostingtgl, rfisclose, rfcabangnama, rflokasinama, rfgudangasalnama, 
        'rfgudangtujuannama, rfdimintaolehkode, rfdimintaolehnama, rfmintakekode, rfmintakenama, rfstatusnama, rfstatussebelumnyanama, 
        'rfinputusernama, rfmodifikasiusernama

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
            Filter = Filter.Replace("rfdimintaolehkode", "c1.kkode")
            Filter = Filter.Replace("rfdimintaolehnama", "c1.knama")
            Filter = Filter.Replace("rfmintakekode", "c2.kkode")
            Filter = Filter.Replace("rfmintakenama", "c2.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m3_rf_v_history")
        sql = "select `rf`.`rfidhistory` AS `rfidhistory`,`rf`.`rfid` AS `rfid`,`rf`.`rfcabang` AS `rfcabang`,`rf`.`rflokasi` AS `rflokasi`,`rf`.`rfgudangasal` AS `rfgudangasal`,`rf`.`rfgudangtujuan` AS `rfgudangtujuan`,`rf`.`rfsumber` AS `rfsumber`,`rf`.`rfautonotransaksi` AS `rfautonotransaksi`,`rf`.`rfnotransaksi` AS `rfnotransaksi`,`rf`.`rftgl` AS `rftgl`,`rf`.`rfkodepa` AS `rfkodepa`,`rf`.`rfdimintaoleh` AS `rfdimintaoleh`,`rf`.`rfdimintaolehkontak` AS `rfdimintaolehkontak`,`rf`.`rfmintake` AS `rfmintake`,`rf`.`rftgldipakai` AS `rftgldipakai`,`rf`.`rfuraian` AS `rfuraian`,`rf`.`rfcatatan` AS `rfcatatan`,`rf`.`rfnoref` AS `rfnoref`,`rf`.`rftglnoref` AS `rftglnoref`,`rf`.`rfstatusts` AS `rfstatusts`,`rf`.`rfstatusrs` AS `rfstatusrs`,`rf`.`rfstatusrealisasi` AS `rfstatusrealisasi`,`rf`.`rfstatus` AS `rfstatus`,`rf`.`rfstatussebelumnya` AS `rfstatussebelumnya`,`rf`.`rfjmlrevisi` AS `rfjmlrevisi`,`rf`.`rfcetakanke` AS `rfcetakanke`,`rf`.`rfinputuser` AS `rfinputuser`,`rf`.`rfinputtgl` AS `rfinputtgl`,`rf`.`rfmodifikasiuser` AS `rfmodifikasiuser`,`rf`.`rfmodifikasitgl` AS `rfmodifikasitgl`,`rf`.`rfposting` AS `rfposting`,`rf`.`rfpostingtgl` AS `rfpostingtgl`,`rf`.`rfisclose` AS `rfisclose`,`br`.`bnama` AS `rfcabangnama`,`lc`.`lnama` AS `rflokasinama`,`wh1`.`wnama` AS `rfgudangasalnama`,`wh2`.`wnama` AS `rfgudangtujuannama`,`c1`.`kkode` AS `rfdimintaolehkode`,`c1`.`knama` AS `rfdimintaolehnama`,`c2`.`kkode` AS `rfmintakekode`,`c2`.`knama` AS `rfmintakenama`,`st1`.`nama` AS `rfstatusnama`,`st2`.`nama` AS `rfstatussebelumnyanama`,`u1`.`unama` AS `rfinputusernama`,`u2`.`unama` AS `rfmodifikasiusernama` from ((((((((((`m3_rf_history` `rf` left join `m1_branch` `br` on((`br`.`bkode` = `rf`.`rfcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `rf`.`rflokasi`))) left join `m1_warehouse` `wh1` on((`wh1`.`wkode` = `rf`.`rfgudangasal`))) left join `m1_warehouse` `wh2` on((`wh2`.`wkode` = `rf`.`rfgudangtujuan`))) left join `m1_contact` `c1` on((`c1`.`kid` = `rf`.`rfdimintaoleh`))) left join `m1_contact` `c2` on((`c2`.`kid` = `rf`.`rfmintake`))) left join `m0_status` `st1` on((`st1`.`kode` = `rf`.`rfstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `rf`.`rfstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `rf`.`rfinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `rf`.`rfmodifikasiuser`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M3_Rf_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("rfidhistory"), 0), sptField,
                     FxDB(dr("rfid"), 0), sptField,
                     FxDB(dr("rfcabang"), ""), sptField,
                     FxDB(dr("rflokasi"), ""), sptField,
                     FxDB(dr("rfgudangasal"), ""), sptField,
                     FxDB(dr("rfgudangtujuan"), ""), sptField,
                     FxDB(dr("rfsumber"), ""), sptField,
                     FxDB(dr("rfautonotransaksi"), 0), sptField,
                     FxDB(dr("rfnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rftgl"), ""), formatTgl), sptField,
                     FxDB(dr("rfkodepa"), 0), sptField,
                     FxDB(dr("rfdimintaoleh"), 0), sptField,
                     FxDB(dr("rfdimintaolehkontak"), ""), sptField,
                     FxDB(dr("rfmintake"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rftgldipakai"), ""), formatTgl), sptField,
                     FxDB(dr("rfuraian"), ""), sptField,
                     FxDB(dr("rfcatatan"), ""), sptField,
                     FxDB(dr("rfnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rftglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("rfstatusts"), 0), sptField,
                     FxDB(dr("rfstatusrs"), 0), sptField,
                     FxDB(dr("rfstatusrealisasi"), 0), sptField,
                     FxDB(dr("rfstatus"), 0), sptField,
                     FxDB(dr("rfstatussebelumnya"), 0), sptField,
                     FxDB(dr("rfjmlrevisi"), 0), sptField,
                     FxDB(dr("rfcetakanke"), 0), sptField,
                     FxDB(dr("rfinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rfinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rfmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rfmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rfposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rfpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rfisclose"), 0), sptField,
                     FxDB(dr("rfcabangnama"), ""), sptField,
                     FxDB(dr("rflokasinama"), ""), sptField,
                     FxDB(dr("rfgudangasalnama"), ""), sptField,
                     FxDB(dr("rfgudangtujuannama"), ""), sptField,
                     FxDB(dr("rfdimintaolehkode"), ""), sptField,
                     FxDB(dr("rfdimintaolehnama"), ""), sptField,
                     FxDB(dr("rfmintakekode"), ""), sptField,
                     FxDB(dr("rfmintakenama"), ""), sptField,
                     FxDB(dr("rfstatusnama"), ""), sptField,
                     FxDB(dr("rfstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("rfinputusernama"), ""), sptField,
                     FxDB(dr("rfmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rfidhistory, rfid, rfcabang, rflokasi, rfgudangasal, rfgudangtujuan, rfsumber, rfautonotransaksi, rfnotransaksi, rftgl, rfkodepa, rfdimintaoleh, rfdimintaolehkontak, rfmintake, rftgldipakai, rfuraian, rfcatatan, rfnoref, rftglnoref, rfstatusts, rfstatusrs, rfstatusrealisasi, rfstatus, rfstatussebelumnya, rfjmlrevisi, rfcetakanke, rfinputuser, rfinputtgl, rfmodifikasiuser, rfmodifikasitgl, rfposting, rfpostingtgl, rfisclose, rfcabangnama, rflokasinama, rfgudangasalnama, rfgudangtujuannama, rfdimintaolehkode, rfdimintaolehnama, rfmintakekode, rfmintakenama, rfstatusnama, rfstatussebelumnyanama, rfinputusernama, rfmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_RfHistoryGetdataById(ByVal param As String) As String

        'M3_RfHistoryGetdataById Utama --------------------------------------------------------
        'rfidhistory, rfid, rfcabang, rflokasi, rfgudangasal, rfgudangtujuan, rfsumber, rfautonotransaksi, 
        'rfnotransaksi, rftgl, rfkodepa, rfdimintaoleh, rfdimintaolehkontak, rfmintake, rftgldipakai, 
        'rfuraian, rfcatatan, rfnoref, rftglnoref, rfstatusts, rfstatusrs, rfstatusrealisasi, 
        'rfstatus, rfstatussebelumnya, rfjmlrevisi, rfcetakanke, rfinputuser, rfinputtgl, rfmodifikasiuser, 
        'rfmodifikasitgl, rfposting, rfpostingtgl, rfisclose, rfcustomtext1, rfcustomtext2, rfcustomtext3, 
        'rfcustomtext4, rfcustomtext5, rfcustomint1, rfcustomint2, rfcustomint3, rfcustomdbl1, rfcustomdbl2, 
        'rfcustomdbl3, rfcustomdate1, rfcustomdate2, rfcustomdate3, rfcabangnama, rflokasinama, rfgudangasalnama, 
        'rfgudangtujuannama, rfdimintaolehkode, rfdimintaolehnama, rfmintakekode, rfmintakenama, rfstatusnama, rfstatussebelumnyanama, 
        'rfinputusernama, rfmodifikasiusernama

        'M3_RfHistoryGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idrfdetail, idrf, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargabeli, hargajual, 
        'stokterakhir, cabang, lokasi, gudangasal, gudangtujuan, costcenter, divisi, 
        'subdivisi, proyek, catatan, urutan, jmlts, statusts, jmlrs, 
        'statusrs, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, 
        'cabangnama, lokasinama, gudangasalnama, gudangtujuannama, costcenternama, divisinama, subdivisinama, 
        'proyeknama

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

        Dim NmMemcached As String = "aplikasi1-M3_Rf_history~M3_Rf_Detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "rfidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "rfidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m3_rf_getdata_history")
        sql = "select `rf`.`rfidhistory` AS `rfidhistory`,`rf`.`rfid` AS `rfid`,`rf`.`rfcabang` AS `rfcabang`,`rf`.`rflokasi` AS `rflokasi`,`rf`.`rfgudangasal` AS `rfgudangasal`,`rf`.`rfgudangtujuan` AS `rfgudangtujuan`,`rf`.`rfsumber` AS `rfsumber`,`rf`.`rfautonotransaksi` AS `rfautonotransaksi`,`rf`.`rfnotransaksi` AS `rfnotransaksi`,`rf`.`rftgl` AS `rftgl`,`rf`.`rfkodepa` AS `rfkodepa`,`rf`.`rfdimintaoleh` AS `rfdimintaoleh`,`rf`.`rfdimintaolehkontak` AS `rfdimintaolehkontak`,`rf`.`rfmintake` AS `rfmintake`,`rf`.`rftgldipakai` AS `rftgldipakai`,`rf`.`rfuraian` AS `rfuraian`,`rf`.`rfcatatan` AS `rfcatatan`,`rf`.`rfnoref` AS `rfnoref`,`rf`.`rftglnoref` AS `rftglnoref`,`rf`.`rfstatusts` AS `rfstatusts`,`rf`.`rfstatusrs` AS `rfstatusrs`,`rf`.`rfstatusrealisasi` AS `rfstatusrealisasi`,`rf`.`rfstatus` AS `rfstatus`,`rf`.`rfstatussebelumnya` AS `rfstatussebelumnya`,`rf`.`rfjmlrevisi` AS `rfjmlrevisi`,`rf`.`rfcetakanke` AS `rfcetakanke`,`rf`.`rfinputuser` AS `rfinputuser`,`rf`.`rfinputtgl` AS `rfinputtgl`,`rf`.`rfmodifikasiuser` AS `rfmodifikasiuser`,`rf`.`rfmodifikasitgl` AS `rfmodifikasitgl`,`rf`.`rfposting` AS `rfposting`,`rf`.`rfpostingtgl` AS `rfpostingtgl`,`rf`.`rfisclose` AS `rfisclose`,`rf`.`rfcustomtext1` AS `rfcustomtext1`,`rf`.`rfcustomtext2` AS `rfcustomtext2`,`rf`.`rfcustomtext3` AS `rfcustomtext3`,`rf`.`rfcustomtext4` AS `rfcustomtext4`,`rf`.`rfcustomtext5` AS `rfcustomtext5`,`rf`.`rfcustomint1` AS `rfcustomint1`,`rf`.`rfcustomint2` AS `rfcustomint2`,`rf`.`rfcustomint3` AS `rfcustomint3`,`rf`.`rfcustomdbl1` AS `rfcustomdbl1`,`rf`.`rfcustomdbl2` AS `rfcustomdbl2`,`rf`.`rfcustomdbl3` AS `rfcustomdbl3`,`rf`.`rfcustomdate1` AS `rfcustomdate1`,`rf`.`rfcustomdate2` AS `rfcustomdate2`,`rf`.`rfcustomdate3` AS `rfcustomdate3`,`br`.`bnama` AS `rfcabangnama`,`lc`.`lnama` AS `rflokasinama`,`wh1`.`wnama` AS `rfgudangasalnama`,`wh2`.`wnama` AS `rfgudangtujuannama`,`c1`.`kkode` AS `rfdimintaolehkode`,`c1`.`knama` AS `rfdimintaolehnama`,`c2`.`kkode` AS `rfmintakekode`,`c2`.`knama` AS `rfmintakenama`,`st1`.`nama` AS `rfstatusnama`,`st2`.`nama` AS `rfstatussebelumnyanama`,`u1`.`unama` AS `rfinputusernama`,`u2`.`unama` AS `rfmodifikasiusernama`,`rfd`.`idhistorydetail` AS `idhistorydetail`,`rfd`.`idhistory` AS `idhistory`,`rfd`.`idrfdetail` AS `idrfdetail`,`rfd`.`idrf` AS `idrf`,`rfd`.`idbarang` AS `idbarang`,`rfd`.`namabarang` AS `namabarang`,`rfd`.`tipebarang` AS `tipebarang`,`rfd`.`jml` AS `jml`,`rfd`.`satuan` AS `satuan`,`rfd`.`nilaisatuan` AS `nilaisatuan`,`rfd`.`jmlbarang` AS `jmlbarang`,`rfd`.`satuanbarang` AS `satuanbarang`,`rfd`.`matauang` AS `matauang`,`rfd`.`kurs` AS `kurs`,`rfd`.`hargabeli` AS `hargabeli`,`rfd`.`hargajual` AS `hargajual`,`rfd`.`stokterakhir` AS `stokterakhir`,`rfd`.`cabang` AS `cabang`,`rfd`.`lokasi` AS `lokasi`,`rfd`.`gudangasal` AS `gudangasal`,`rfd`.`gudangtujuan` AS `gudangtujuan`,`rfd`.`costcenter` AS `costcenter`,`rfd`.`divisi` AS `divisi`,`rfd`.`subdivisi` AS `subdivisi`,`rfd`.`proyek` AS `proyek`,`rfd`.`catatan` AS `catatan`,`rfd`.`urutan` AS `urutan`,`rfd`.`jmlts` AS `jmlts`,`rfd`.`statusts` AS `statusts`,`rfd`.`jmlrs` AS `jmlrs`,`rfd`.`statusrs` AS `statusrs`,`rfd`.`jmlrealisasi` AS `jmlrealisasi`,`rfd`.`statusrealisasi` AS `statusrealisasi`,`rfd`.`isclose` AS `isclose`,`rfd`.`customtext1` AS `customtext1`,`rfd`.`customtext2` AS `customtext2`,`rfd`.`customtext3` AS `customtext3`,`rfd`.`customdbl1` AS `customdbl1`,`rfd`.`customdbl2` AS `customdbl2`,`rfd`.`customdbl3` AS `customdbl3`,`rfd`.`customdate1` AS `customdate1`,`rfd`.`customdate2` AS `customdate2`,`rfd`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`brd`.`bnama` AS `cabangnama`,`lcd`.`lnama` AS `lokasinama`,`whd1`.`wnama` AS `gudangasalnama`,`whd2`.`wnama` AS `gudangtujuannama`,`cc`.`ccnama` AS `costcenternama`,`d`.`dnama` AS `divisinama`,`sd`.`sdnama` AS `subdivisinama`,`p`.`pnama` AS `proyeknama` from ((((((((((((((((((((`m3_rf_history` `rf` join `m3_rf_detail_history` `rfd` on((`rf`.`rfidhistory` = `rfd`.`idhistory`))) left join `m1_branch` `br` on((`br`.`bkode` = `rf`.`rfcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `rf`.`rflokasi`))) left join `m1_warehouse` `wh1` on((`wh1`.`wkode` = `rf`.`rfgudangasal`))) left join `m1_warehouse` `wh2` on((`wh2`.`wkode` = `rf`.`rfgudangtujuan`))) left join `m1_contact` `c1` on((`c1`.`kid` = `rf`.`rfdimintaoleh`))) left join `m1_contact` `c2` on((`c2`.`kid` = `rf`.`rfmintake`))) left join `m0_status` `st1` on((`st1`.`kode` = `rf`.`rfstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `rf`.`rfstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `rf`.`rfinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `rf`.`rfmodifikasiuser`))) left join `m1_item_hauling` `i` on((`i`.`bid` = `rfd`.`idbarang`))) left join `m1_branch` `brd` on((`rfd`.`cabang` = `brd`.`bkode`))) left join `m1_location` `lcd` on((`rfd`.`lokasi` = `lcd`.`lkode`))) left join `m1_warehouse` `whd1` on((`rfd`.`gudangasal` = `whd1`.`wkode`))) left join `m1_warehouse` `whd2` on((`rfd`.`gudangtujuan` = `whd2`.`wkode`))) left join `m1_cost_center` `cc` on((`rfd`.`costcenter` = `cc`.`cckode`))) left join `m1_division` `d` on((`rfd`.`divisi` = `d`.`dkode`))) left join `m1_subdivision` `sd` on((`rfd`.`subdivisi` = `sd`.`sdkode`))) left join `m1_project` `p` on((`rfd`.`proyek` = `p`.`pkode`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("rfidhistory"), 0), sptField,
                     FxDB(drutama("rfid"), 0), sptField,
                     FxDB(drutama("rfcabang"), ""), sptField,
                     FxDB(drutama("rflokasi"), ""), sptField,
                     FxDB(drutama("rfgudangasal"), ""), sptField,
                     FxDB(drutama("rfgudangtujuan"), ""), sptField,
                     FxDB(drutama("rfsumber"), ""), sptField,
                     FxDB(drutama("rfautonotransaksi"), 0), sptField,
                     FxDB(drutama("rfnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rftgl"), ""), formatTgl), sptField,
                     FxDB(drutama("rfkodepa"), 0), sptField,
                     FxDB(drutama("rfdimintaoleh"), 0), sptField,
                     FxDB(drutama("rfdimintaolehkontak"), ""), sptField,
                     FxDB(drutama("rfmintake"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rftgldipakai"), ""), formatTgl), sptField,
                     FxDB(drutama("rfuraian"), ""), sptField,
                     FxDB(drutama("rfcatatan"), ""), sptField,
                     FxDB(drutama("rfnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rftglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("rfstatusts"), 0), sptField,
                     FxDB(drutama("rfstatusrs"), 0), sptField,
                     FxDB(drutama("rfstatusrealisasi"), 0), sptField,
                     FxDB(drutama("rfstatus"), 0), sptField,
                     FxDB(drutama("rfstatussebelumnya"), 0), sptField,
                     FxDB(drutama("rfjmlrevisi"), 0), sptField,
                     FxDB(drutama("rfcetakanke"), 0), sptField,
                     FxDB(drutama("rfinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rfinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rfmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rfmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rfposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rfpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rfisclose"), 0), sptField,
                     FxDB(drutama("rfcustomtext1"), ""), sptField,
                     FxDB(drutama("rfcustomtext2"), ""), sptField,
                     FxDB(drutama("rfcustomtext3"), ""), sptField,
                     FxDB(drutama("rfcustomtext4"), ""), sptField,
                     FxDB(drutama("rfcustomtext5"), ""), sptField,
                     FxDB(drutama("rfcustomint1"), 0), sptField,
                     FxDB(drutama("rfcustomint2"), 0), sptField,
                     FxDB(drutama("rfcustomint3"), 0), sptField,
                     FxDB(drutama("rfcustomdbl1"), 0), sptField,
                     FxDB(drutama("rfcustomdbl2"), 0), sptField,
                     FxDB(drutama("rfcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rfcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rfcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rfcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("rfcabangnama"), ""), sptField,
                     FxDB(drutama("rflokasinama"), ""), sptField,
                     FxDB(drutama("rfgudangasalnama"), ""), sptField,
                     FxDB(drutama("rfgudangtujuannama"), ""), sptField,
                     FxDB(drutama("rfdimintaolehkode"), ""), sptField,
                     FxDB(drutama("rfdimintaolehnama"), ""), sptField,
                     FxDB(drutama("rfmintakekode"), ""), sptField,
                     FxDB(drutama("rfmintakenama"), ""), sptField,
                     FxDB(drutama("rfstatusnama"), ""), sptField,
                     FxDB(drutama("rfstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("rfinputusernama"), ""), sptField,
                     FxDB(drutama("rfmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idrfdetail"), 0), sptField,
                     FxDB(dr("idrf"), 0), sptField,
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
                     FxDB(dr("hargabeli"), 0), sptField,
                     FxDB(dr("hargajual"), 0), sptField,
                     FxDB(dr("stokterakhir"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudangasal"), ""), sptField,
                     FxDB(dr("gudangtujuan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("jmlts"), 0), sptField,
                     FxDB(dr("statusts"), 0), sptField,
                     FxDB(dr("jmlrs"), 0), sptField,
                     FxDB(dr("statusrs"), 0), sptField,
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
                     FxDB(dr("cabangnama"), ""), sptField,
                     FxDB(dr("lokasinama"), ""), sptField,
                     FxDB(dr("gudangasalnama"), ""), sptField,
                     FxDB(dr("gudangtujuannama"), ""), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rfidhistory, rfid, rfcabang, rflokasi, rfgudangasal, rfgudangtujuan, rfsumber, rfautonotransaksi, rfnotransaksi, rftgl, rfkodepa, rfdimintaoleh, rfdimintaolehkontak, rfmintake, rftgldipakai, rfuraian, rfcatatan, rfnoref, rftglnoref, rfstatusts, rfstatusrs, rfstatusrealisasi, rfstatus, rfstatussebelumnya, rfjmlrevisi, rfcetakanke, rfinputuser, rfinputtgl, rfmodifikasiuser, rfmodifikasitgl, rfposting, rfpostingtgl, rfisclose, rfcustomtext1, rfcustomtext2, rfcustomtext3, rfcustomtext4, rfcustomtext5, rfcustomint1, rfcustomint2, rfcustomint3, rfcustomdbl1, rfcustomdbl2, rfcustomdbl3, rfcustomdate1, rfcustomdate2, rfcustomdate3, rfcabangnama, rflokasinama, rfgudangasalnama, rfgudangtujuannama, rfdimintaolehkode, rfdimintaolehnama, rfmintakekode, rfmintakenama, rfstatusnama, rfstatussebelumnyanama, rfinputusernama, rfmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idrfdetail, idrf, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargabeli, hargajual, stokterakhir, cabang, lokasi, gudangasal, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlts, statusts, jmlrs, statusrs, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, cabangnama, lokasinama, gudangasalnama, gudangtujuannama, costcenternama, divisinama, subdivisinama, proyeknama"))

        Return wsResult
    End Function


End Class