Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m3_dc_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M3_Dc_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO M3_Dc_history(SELECT 0, dc.* FROM M3_Dc dc WHERE dc.dcid = '" & idtransaksi & "')"
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
            sql = "SELECT dcidhistory FROM M3_Dc_History WHERE dcid = '" & idtransaksi & "' ORDER BY dcmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO M3_Dc_Detail_History (SELECT 0, '" & result(4) & "', dc.* FROM M3_Dc_Detail dc WHERE dc.iddc = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY DETAIL -------------------------------


            'PROSES INSERT HISTORY CHECK --------------------------------------
            sql = "INSERT INTO M3_Dc_Check_History (SELECT 0, '" & result(4) & "', dc.* FROM M3_Dc_Check dc WHERE dc.iddc = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY CHECK -------------------------------


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
    Public Function M3_Dc_HistorySearch(ByVal param As String) As String
        'M3_Dc_HistorySearch --------------------------------------------------------
        'dcidhistory, dcid, dccabang, dclokasi, dcgudangasal, dcgudangtujuan, dcsumber, dcautonotransaksi, 
        'dcnotransaksi, dctgl, dckodepa, dcdimintaoleh, dcdimintaolehkontak, dcmintake, dctgldipakai, 
        'dcuraian, dccatatan, dcnoref, dctglnoref, dcstatusts, dcstatusrs, dcstatusrealisasi, 
        'dcstatus, dcstatussebelumnya, dcjmlrevisi, dccetakanke, dcinputuser, dcinputtgl, dcmodifikasiuser, 
        'dcmodifikasitgl, dcposting, dcpostingtgl, dcisclose, dccabangnama, dclokasinama, dcgudangasalnama, 
        'dcgudangtujuannama, dcdimintaolehkode, dcdimintaolehnama, dcmintakekode, dcmintakenama, dcstatusnama, dcstatussebelumnyanama, 
        'dcinputusernama, dcmodifikasiusernama, dcshift, dcidbarang, dcnamabarang, dctipebarang, dchmstart, 
        'dchmstop, dchmtotal, dckodebarang

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
            Filter = Filter.Replace("dcdimintaolehkode", "c1.kkode")
            Filter = Filter.Replace("dcdimintaolehnama", "c1.knama")
            Filter = Filter.Replace("dcmintakekode", "c2.kkode")
            Filter = Filter.Replace("dcmintakenama", "c2.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m3_dc_v")
        sql = "select `dc`.`dcidhistory` AS `dcidhistory`,`dc`.`dcid` AS `dcid`,`dc`.`dccabang` AS `dccabang`,`dc`.`dclokasi` AS `dclokasi`,`dc`.`dcgudangasal` AS `dcgudangasal`,`dc`.`dcgudangtujuan` AS `dcgudangtujuan`,`dc`.`dcsumber` AS `dcsumber`,`dc`.`dcautonotransaksi` AS `dcautonotransaksi`,`dc`.`dcnotransaksi` AS `dcnotransaksi`,`dc`.`dctgl` AS `dctgl`,`dc`.`dckodepa` AS `dckodepa`,`dc`.`dcdimintaoleh` AS `dcdimintaoleh`,`dc`.`dcdimintaolehkontak` AS `dcdimintaolehkontak`,`dc`.`dcmintake` AS `dcmintake`,`dc`.`dctgldipakai` AS `dctgldipakai`,`dc`.`dcuraian` AS `dcuraian`,`dc`.`dccatatan` AS `dccatatan`,`dc`.`dcnoref` AS `dcnoref`,`dc`.`dctglnoref` AS `dctglnoref`,`dc`.`dcstatusts` AS `dcstatusts`,`dc`.`dcstatusrs` AS `dcstatusrs`,`dc`.`dcstatusrealisasi` AS `dcstatusrealisasi`,`dc`.`dcstatus` AS `dcstatus`,`dc`.`dcstatussebelumnya` AS `dcstatussebelumnya`,`dc`.`dcjmlrevisi` AS `dcjmlrevisi`,`dc`.`dccetakanke` AS `dccetakanke`,`dc`.`dcinputuser` AS `dcinputuser`,`dc`.`dcinputtgl` AS `dcinputtgl`,`dc`.`dcmodifikasiuser` AS `dcmodifikasiuser`,`dc`.`dcmodifikasitgl` AS `dcmodifikasitgl`,`dc`.`dcposting` AS `dcposting`,`dc`.`dcpostingtgl` AS `dcpostingtgl`,`dc`.`dcisclose` AS `dcisclose`,`br`.`bnama` AS `dccabangnama`,`lc`.`lnama` AS `dclokasinama`,`wh1`.`wnama` AS `dcgudangasalnama`,`wh2`.`wnama` AS `dcgudangtujuannama`,`c1`.`kkode` AS `dcdimintaolehkode`,`c1`.`knama` AS `dcdimintaolehnama`,`c2`.`kkode` AS `dcmintakekode`,`c2`.`knama` AS `dcmintakenama`,`st1`.`nama` AS `dcstatusnama`,`st2`.`nama` AS `dcstatussebelumnyanama`,`u1`.`unama` AS `dcinputusernama`,`u2`.`unama` AS `dcmodifikasiusernama`,`dc`.`dcshift` AS `dcshift`,`dc`.`dcidbarang` AS `dcidbarang`,`dc`.`dcnamabarang` AS `dcnamabarang`,`dc`.`dctipebarang` AS `dctipebarang`,`dc`.`dchmstart` AS `dchmstart`,`dc`.`dchmstop` AS `dchmstop`,`dc`.`dchmtotal` AS `dchmtotal`,`ih`.`bkode` AS `dckodebarang` from (((((((((((`m3_dc_history` `dc` left join `m1_item_hauling` `ih` on((`dc`.`dcidbarang` = `ih`.`bid`))) left join `m1_branch` `br` on((`br`.`bkode` = `dc`.`dccabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `dc`.`dclokasi`))) left join `m1_warehouse` `wh1` on((`wh1`.`wkode` = `dc`.`dcgudangasal`))) left join `m1_warehouse` `wh2` on((`wh2`.`wkode` = `dc`.`dcgudangtujuan`))) left join `m1_contact` `c1` on((`c1`.`kid` = `dc`.`dcdimintaoleh`))) left join `m1_contact` `c2` on((`c2`.`kid` = `dc`.`dcmintake`))) left join `m0_status` `st1` on((`st1`.`kode` = `dc`.`dcstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `dc`.`dcstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `dc`.`dcinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `dc`.`dcmodifikasiuser`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M3_Dc", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("dcidhistory"), ""), sptField,
                     FxDB(dr("dcid"), ""), sptField,
                     FxDB(dr("dccabang"), ""), sptField,
                     FxDB(dr("dclokasi"), ""), sptField,
                     FxDB(dr("dcgudangasal"), ""), sptField,
                     FxDB(dr("dcgudangtujuan"), ""), sptField,
                     FxDB(dr("dcsumber"), ""), sptField,
                     FxDB(dr("dcautonotransaksi"), 0), sptField,
                     FxDB(dr("dcnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dctgl"), ""), formatTgl), sptField,
                     FxDB(dr("dckodepa"), ""), sptField,
                     FxDB(dr("dcdimintaoleh"), ""), sptField,
                     FxDB(dr("dcdimintaolehkontak"), ""), sptField,
                     FxDB(dr("dcmintake"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dctgldipakai"), ""), formatTgl), sptField,
                     FxDB(dr("dcuraian"), ""), sptField,
                     FxDB(dr("dccatatan"), ""), sptField,
                     FxDB(dr("dcnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dctglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("dcstatusts"), 0), sptField,
                     FxDB(dr("dcstatusrs"), 0), sptField,
                     FxDB(dr("dcstatusrealisasi"), 0), sptField,
                     FxDB(dr("dcstatus"), 0), sptField,
                     FxDB(dr("dcstatussebelumnya"), 0), sptField,
                     FxDB(dr("dcjmlrevisi"), 0), sptField,
                     FxDB(dr("dccetakanke"), 0), sptField,
                     FxDB(dr("dcinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dcinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("dcmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dcmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("dcposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("dcpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("dcisclose"), 0), sptField,
                     FxDB(dr("dccabangnama"), ""), sptField,
                     FxDB(dr("dclokasinama"), ""), sptField,
                     FxDB(dr("dcgudangasalnama"), ""), sptField,
                     FxDB(dr("dcgudangtujuannama"), ""), sptField,
                     FxDB(dr("dcdimintaolehkode"), ""), sptField,
                     FxDB(dr("dcdimintaolehnama"), ""), sptField,
                     FxDB(dr("dcmintakekode"), ""), sptField,
                     FxDB(dr("dcmintakenama"), ""), sptField,
                     FxDB(dr("dcstatusnama"), ""), sptField,
                     FxDB(dr("dcstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("dcinputusernama"), ""), sptField,
                     FxDB(dr("dcmodifikasiusernama"), ""), sptField,
                     FxDB(dr("dcshift"), 0), sptField,
                     FxDB(dr("dcidbarang"), ""), sptField,
                     FxDB(dr("dcnamabarang"), ""), sptField,
                     FxDB(dr("dctipebarang"), ""), sptField,
                     FxDB(dr("dchmstart"), 0), sptField,
                     FxDB(dr("dchmstop"), 0), sptField,
                     FxDB(dr("dchmtotal"), 0), sptField,
                     FxDB(dr("dckodebarang"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("dcidhistory, dcid, dccabang, dclokasi, dcgudangasal, dcgudangtujuan, dcsumber, dcautonotransaksi, dcnotransaksi, dctgl, dckodepa, dcdimintaoleh, dcdimintaolehkontak, dcmintake, dctgldipakai, dcuraian, dccatatan, dcnoref, dctglnoref, dcstatusts, dcstatusrs, dcstatusrealisasi, dcstatus, dcstatussebelumnya, dcjmlrevisi, dccetakanke, dcinputuser, dcinputtgl, dcmodifikasiuser, dcmodifikasitgl, dcposting, dcpostingtgl, dcisclose, dccabangnama, dclokasinama, dcgudangasalnama, dcgudangtujuannama, dcdimintaolehkode, dcdimintaolehnama, dcmintakekode, dcmintakenama, dcstatusnama, dcstatussebelumnyanama, dcinputusernama, dcmodifikasiusernama, dcshift, dcidbarang, dcnamabarang, dctipebarang, dchmstart, dchmstop, dchmtotal, dckodebarang"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_DcHistoryGetdataById(ByVal param As String) As String

        'M3_DcHistoryGetdataById Utama --------------------------------------------------------
        'dcidhistory, dcid, dccabang, dclokasi, dcgudangasal, dcgudangtujuan, dcsumber, dcautonotransaksi, 
        'dcnotransaksi, dctgl, dckodepa, dcdimintaoleh, dcdimintaolehkontak, dcmintake, dctgldipakai, 
        'dcuraian, dccatatan, dcnoref, dctglnoref, dcstatusts, dcstatusrs, dcstatusrealisasi, 
        'dcstatus, dcstatussebelumnya, dcjmlrevisi, dccetakanke, dcinputuser, dcinputtgl, dcmodifikasiuser, 
        'dcmodifikasitgl, dcposting, dcpostingtgl, dcisclose, dccustomtext1, dccustomtext2, dccustomtext3, 
        'dccustomtext4, dccustomtext5, dccustomint1, dccustomint2, dccustomint3, dccustomdbl1, dccustomdbl2, 
        'dccustomdbl3, dccustomdate1, dccustomdate2, dccustomdate3, dccabangnama, dclokasinama, dcgudangasalnama, 
        'dcgudangtujuannama, dcdimintaolehkode, dcdimintaolehnama, dcmintakekode, dcmintakenama, dcstatusnama, dcstatussebelumnyanama, 
        'dcinputusernama, dcmodifikasiusernama, dcshift, dcidbarang, dcnamabarang, dctipebarang, dchmstart, 
        'dchmstop, dchmtotal, dckodebarang

        'M3_DcHistoryGetdataById Detail -------------------------------------------------------
        'iddcdetailhistory, iddchistory, iddcdetail, iddc, opstart, opend, 
        'sbstart, sbend, spstart, spend, rfstart, rfend, bdstart, 
        'bdend, cabang, lokasi, gudangasal, gudangtujuan, costcenter, divisi, 
        'subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3, cabangnama, lokasinama, gudangasalnama, gudangtujuannama, costcenternama, 
        'divisinama, subdivisinama, proyeknama

        'M3_DcHistoryGetdataById Check --------------------------------------------------------
        'iddccheckhistory, iddchistory, iddccheck, iddc, idkategoricheck, catatan, status, urutan, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3, ccnama

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

        Dim utama As String = "", detail As String = "", detailCheck As String = "", idtransaksi As String = ""

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


        Dim NmMemcached As String = "aplikasi1-M3_Dc~M3_Dc_Detail-" & idtransaksi
        Dim Filter2 As String = ""

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "dcidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "dcidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        'Set filter detail 2
        If Len(Filter2) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter2 = "iddchistory = '" & idtransaksi & "'"
        Else ' jika filter diisi
            Filter2 = "iddchistory = '" & idtransaksi & "' and " & Filter2
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m3_dc_getdata")
        sql = "select `dc`.`dcidhistory` AS `dcidhistory`,`dc`.`dcid` AS `dcid`,`dc`.`dccabang` AS `dccabang`,`dc`.`dclokasi` AS `dclokasi`,`dc`.`dcgudangasal` AS `dcgudangasal`,`dc`.`dcgudangtujuan` AS `dcgudangtujuan`,`dc`.`dcsumber` AS `dcsumber`,`dc`.`dcautonotransaksi` AS `dcautonotransaksi`,`dc`.`dcnotransaksi` AS `dcnotransaksi`,`dc`.`dctgl` AS `dctgl`,`dc`.`dckodepa` AS `dckodepa`,`dc`.`dcdimintaoleh` AS `dcdimintaoleh`,`dc`.`dcdimintaolehkontak` AS `dcdimintaolehkontak`,`dc`.`dcmintake` AS `dcmintake`,`dc`.`dctgldipakai` AS `dctgldipakai`,`dc`.`dcuraian` AS `dcuraian`,`dc`.`dccatatan` AS `dccatatan`,`dc`.`dcnoref` AS `dcnoref`,`dc`.`dctglnoref` AS `dctglnoref`,`dc`.`dcstatusts` AS `dcstatusts`,`dc`.`dcstatusrs` AS `dcstatusrs`,`dc`.`dcstatusrealisasi` AS `dcstatusrealisasi`,`dc`.`dcstatus` AS `dcstatus`,`dc`.`dcstatussebelumnya` AS `dcstatussebelumnya`,`dc`.`dcjmlrevisi` AS `dcjmlrevisi`,`dc`.`dccetakanke` AS `dccetakanke`,`dc`.`dcinputuser` AS `dcinputuser`,`dc`.`dcinputtgl` AS `dcinputtgl`,`dc`.`dcmodifikasiuser` AS `dcmodifikasiuser`,`dc`.`dcmodifikasitgl` AS `dcmodifikasitgl`,`dc`.`dcposting` AS `dcposting`,`dc`.`dcpostingtgl` AS `dcpostingtgl`,`dc`.`dcisclose` AS `dcisclose`,`dc`.`dccustomtext1` AS `dccustomtext1`,`dc`.`dccustomtext2` AS `dccustomtext2`,`dc`.`dccustomtext3` AS `dccustomtext3`,`dc`.`dccustomtext4` AS `dccustomtext4`,`dc`.`dccustomtext5` AS `dccustomtext5`,`dc`.`dccustomint1` AS `dccustomint1`,`dc`.`dccustomint2` AS `dccustomint2`,`dc`.`dccustomint3` AS `dccustomint3`,`dc`.`dccustomdbl1` AS `dccustomdbl1`,`dc`.`dccustomdbl2` AS `dccustomdbl2`,`dc`.`dccustomdbl3` AS `dccustomdbl3`,`dc`.`dccustomdate1` AS `dccustomdate1`,`dc`.`dccustomdate2` AS `dccustomdate2`,`dc`.`dccustomdate3` AS `dccustomdate3`,`br`.`bnama` AS `dccabangnama`,`lc`.`lnama` AS `dclokasinama`,`wh1`.`wnama` AS `dcgudangasalnama`,`wh2`.`wnama` AS `dcgudangtujuannama`,`c1`.`kkode` AS `dcdimintaolehkode`,`c1`.`knama` AS `dcdimintaolehnama`,`c2`.`kkode` AS `dcmintakekode`,`c2`.`knama` AS `dcmintakenama`,`st1`.`nama` AS `dcstatusnama`,`st2`.`nama` AS `dcstatussebelumnyanama`,`u1`.`unama` AS `dcinputusernama`,`u2`.`unama` AS `dcmodifikasiusernama`,`dc`.`dcshift` AS `dcshift`,`dc`.`dcidbarang` AS `dcidbarang`,`dc`.`dcnamabarang` AS `dcnamabarang`,`dc`.`dctipebarang` AS `dctipebarang`,`dc`.`dchmstart` AS `dchmstart`,`dc`.`dchmstop` AS `dchmstop`,`dc`.`dchmtotal` AS `dchmtotal`,`ih`.`bkode` AS `dckodebarang`,`dcd`.`iddcdetailhistory` AS `iddcdetailhistory`,`dcd`.`iddchistory` AS `iddchistory`,`dcd`.`iddcdetail` AS `iddcdetail`,`dcd`.`iddc` AS `iddc`,`dcd`.`opstart` AS `opstart`,`dcd`.`opend` AS `opend`,`dcd`.`sbstart` AS `sbstart`,`dcd`.`sbend` AS `sbend`,`dcd`.`spstart` AS `spstart`,`dcd`.`spend` AS `spend`,`dcd`.`rfstart` AS `rfstart`,`dcd`.`rfend` AS `rfend`,`dcd`.`bdstart` AS `bdstart`,`dcd`.`bdend` AS `bdend`,`dcd`.`cabang` AS `cabang`,`dcd`.`lokasi` AS `lokasi`,`dcd`.`gudangasal` AS `gudangasal`,`dcd`.`gudangtujuan` AS `gudangtujuan`,`dcd`.`costcenter` AS `costcenter`,`dcd`.`divisi` AS `divisi`,`dcd`.`subdivisi` AS `subdivisi`,`dcd`.`proyek` AS `proyek`,`dcd`.`catatan` AS `catatan`,`dcd`.`urutan` AS `urutan`,`dcd`.`jmlrealisasi` AS `jmlrealisasi`,`dcd`.`statusrealisasi` AS `statusrealisasi`,`dcd`.`isclose` AS `isclose`,`dcd`.`customtext1` AS `customtext1`,`dcd`.`customtext2` AS `customtext2`,`dcd`.`customtext3` AS `customtext3`,`dcd`.`customdbl1` AS `customdbl1`,`dcd`.`customdbl2` AS `customdbl2`,`dcd`.`customdbl3` AS `customdbl3`,`dcd`.`customdate1` AS `customdate1`,`dcd`.`customdate2` AS `customdate2`,`dcd`.`customdate3` AS `customdate3`,`brd`.`bnama` AS `cabangnama`,`lcd`.`lnama` AS `lokasinama`,`whd1`.`wnama` AS `gudangasalnama`,`whd2`.`wnama` AS `gudangtujuannama`,`cc`.`ccnama` AS `costcenternama`,`d`.`dnama` AS `divisinama`,`sd`.`sdnama` AS `subdivisinama`,`p`.`pnama` AS `proyeknama` from ((((((((((((((((((((`m3_dc_history` `dc` join `m3_dc_detail_history` `dcd` on((`dc`.`dcid` = `dcd`.`iddc`))) left join `m1_branch` `br` on((`br`.`bkode` = `dc`.`dccabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `dc`.`dclokasi`))) left join `m1_warehouse` `wh1` on((`wh1`.`wkode` = `dc`.`dcgudangasal`))) left join `m1_warehouse` `wh2` on((`wh2`.`wkode` = `dc`.`dcgudangtujuan`))) left join `m1_contact` `c1` on((`c1`.`kid` = `dc`.`dcdimintaoleh`))) left join `m1_contact` `c2` on((`c2`.`kid` = `dc`.`dcmintake`))) left join `m1_item_hauling` `ih` on((`dc`.`dcidbarang` = `ih`.`bid`))) left join `m0_status` `st1` on((`st1`.`kode` = `dc`.`dcstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `dc`.`dcstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `dc`.`dcinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `dc`.`dcmodifikasiuser`))) left join `m1_branch` `brd` on((`dcd`.`cabang` = `brd`.`bkode`))) left join `m1_location` `lcd` on((`dcd`.`lokasi` = `lcd`.`lkode`))) left join `m1_warehouse` `whd1` on((`dcd`.`gudangasal` = `whd1`.`wkode`))) left join `m1_warehouse` `whd2` on((`dcd`.`gudangtujuan` = `whd2`.`wkode`))) left join `m1_cost_center` `cc` on((`dcd`.`costcenter` = `cc`.`cckode`))) left join `m1_division` `d` on((`dcd`.`divisi` = `d`.`dkode`))) left join `m1_subdivision` `sd` on((`dcd`.`subdivisi` = `sd`.`sdkode`))) left join `m1_project` `p` on((`dcd`.`proyek` = `p`.`pkode`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                     FxDB(drutama("dcidhistory"), ""), sptField,
                     FxDB(drutama("dcid"), ""), sptField,
                     FxDB(drutama("dccabang"), ""), sptField,
                     FxDB(drutama("dclokasi"), ""), sptField,
                     FxDB(drutama("dcgudangasal"), ""), sptField,
                     FxDB(drutama("dcgudangtujuan"), ""), sptField,
                     FxDB(drutama("dcsumber"), ""), sptField,
                     FxDB(drutama("dcautonotransaksi"), 0), sptField,
                     FxDB(drutama("dcnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("dctgl"), ""), formatTgl), sptField,
                     FxDB(drutama("dckodepa"), ""), sptField,
                     FxDB(drutama("dcdimintaoleh"), ""), sptField,
                     FxDB(drutama("dcdimintaolehkontak"), ""), sptField,
                     FxDB(drutama("dcmintake"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("dctgldipakai"), ""), formatTgl), sptField,
                     FxDB(drutama("dcuraian"), ""), sptField,
                     FxDB(drutama("dccatatan"), ""), sptField,
                     FxDB(drutama("dcnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("dctglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("dcstatusts"), 0), sptField,
                     FxDB(drutama("dcstatusrs"), 0), sptField,
                     FxDB(drutama("dcstatusrealisasi"), 0), sptField,
                     FxDB(drutama("dcstatus"), 0), sptField,
                     FxDB(drutama("dcstatussebelumnya"), 0), sptField,
                     FxDB(drutama("dcjmlrevisi"), 0), sptField,
                     FxDB(drutama("dccetakanke"), 0), sptField,
                     FxDB(drutama("dcinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("dcinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("dcmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("dcmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("dcposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("dcpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("dcisclose"), 0), sptField,
                     FxDB(drutama("dccustomtext1"), ""), sptField,
                     FxDB(drutama("dccustomtext2"), ""), sptField,
                     FxDB(drutama("dccustomtext3"), ""), sptField,
                     FxDB(drutama("dccustomtext4"), ""), sptField,
                     FxDB(drutama("dccustomtext5"), ""), sptField,
                     FxDB(drutama("dccustomint1"), 0), sptField,
                     FxDB(drutama("dccustomint2"), 0), sptField,
                     FxDB(drutama("dccustomint3"), 0), sptField,
                     FxDB(drutama("dccustomdbl1"), 0), sptField,
                     FxDB(drutama("dccustomdbl2"), 0), sptField,
                     FxDB(drutama("dccustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("dccustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("dccustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("dccustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("dccabangnama"), ""), sptField,
                     FxDB(drutama("dclokasinama"), ""), sptField,
                     FxDB(drutama("dcgudangasalnama"), ""), sptField,
                     FxDB(drutama("dcgudangtujuannama"), ""), sptField,
                     FxDB(drutama("dcdimintaolehkode"), ""), sptField,
                     FxDB(drutama("dcdimintaolehnama"), ""), sptField,
                     FxDB(drutama("dcmintakekode"), ""), sptField,
                     FxDB(drutama("dcmintakenama"), ""), sptField,
                     FxDB(drutama("dcstatusnama"), ""), sptField,
                     FxDB(drutama("dcstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("dcinputusernama"), ""), sptField,
                     FxDB(drutama("dcmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("dcshift"), 0), sptField,
                     FxDB(drutama("dcidbarang"), ""), sptField,
                     FxDB(drutama("dcnamabarang"), ""), sptField,
                     FxDB(drutama("dctipebarang"), ""), sptField,
                     FxDB(drutama("dchmstart"), 0), sptField,
                     FxDB(drutama("dchmstop"), 0), sptField,
                     FxDB(drutama("dchmtotal"), 0), sptField,
                     FxDB(drutama("dckodebarang"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("iddcdetailhistory"), ""), sptField,
                     FxDB(dr("iddchistory"), ""), sptField,
                     FxDB(dr("iddcdetail"), ""), sptField,
                     FxDB(dr("iddc"), ""), sptField,
                     IIf(Len(FxDB(dr("opstart").ToString, "")) > 0, Replace(FxDB(dr("opstart").ToString, ""), ":", ".").Substring(0, 5), ""), sptField,
                     IIf(Len(FxDB(dr("opend").ToString, "")) > 0, Replace(FxDB(dr("opend").ToString, ""), ":", ".").Substring(0, 5), ""), sptField,
                     IIf(Len(FxDB(dr("sbstart").ToString, "")) > 0, Replace(FxDB(dr("sbstart").ToString, ""), ":", ".").Substring(0, 5), ""), sptField,
                     IIf(Len(FxDB(dr("sbend").ToString, "")) > 0, Replace(FxDB(dr("sbend").ToString, ""), ":", ".").Substring(0, 5), ""), sptField,
                     IIf(Len(FxDB(dr("spstart").ToString, "")) > 0, Replace(FxDB(dr("spstart").ToString, ""), ":", ".").Substring(0, 5), ""), sptField,
                     IIf(Len(FxDB(dr("spend").ToString, "")) > 0, Replace(FxDB(dr("spend").ToString, ""), ":", ".").Substring(0, 5), ""), sptField,
                     IIf(Len(FxDB(dr("rfstart").ToString, "")) > 0, Replace(FxDB(dr("rfstart").ToString, ""), ":", ".").Substring(0, 5), ""), sptField,
                     IIf(Len(FxDB(dr("rfend").ToString, "")) > 0, Replace(FxDB(dr("rfend").ToString, ""), ":", ".").Substring(0, 5), ""), sptField,
                     IIf(Len(FxDB(dr("bdstart").ToString, "")) > 0, Replace(FxDB(dr("bdstart").ToString, ""), ":", ".").Substring(0, 5), ""), sptField,
                     IIf(Len(FxDB(dr("bdend").ToString, "")) > 0, Replace(FxDB(dr("bdend").ToString, ""), ":", ".").Substring(0, 5), ""), sptField,
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

            'AMBIL DATA OUT
            'Dim querygiro As New m0_query
            'sql = querygiro.PanggilQuery("m6_pd_getdata_out")
            sql = "select `dcc`.`iddccheckhistory` AS `iddccheckhistory`,`dcc`.`iddchistory` AS `iddchistory`,`dcc`.`iddccheck` AS `iddccheck`,`dcc`.`iddc` AS `iddc`,`dcc`.`idkategoricheck` AS `idkategoricheck`,`dcc`.`catatan` AS `catatan`,`dcc`.`status` AS `status`,`dcc`.`urutan` AS `urutan`,`dcc`.`isclose` AS `isclose`,`dcc`.`customtext1` AS `customtext1`,`dcc`.`customtext2` AS `customtext2`,`dcc`.`customtext3` AS `customtext3`,`dcc`.`customdbl1` AS `customdbl1`,`dcc`.`customdbl2` AS `customdbl2`,`dcc`.`customdbl3` AS `customdbl3`,`dcc`.`customdate1` AS `customdate1`,`dcc`.`customdate2` AS `customdate2`,`dcc`.`customdate3` AS `customdate3`,`chc`.`ccnama` AS `ccnama` from (`m3_dc_check_history` `dcc` left join `m1_checking_category` `chc` on((`dcc`.`idkategoricheck` = `chc`.`ccid`)))"

            Dim dtout As New DataTable
            dtout = AmbilData("aplikasi1-M6_Pd_Pack", Filter2, "urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases

            For Each dr As DataRow In dtout.Rows
                detailCheck = String.Concat(detailCheck,
                     FxDB(dr("iddccheckhistory"), 0), sptField,
                     FxDB(dr("iddchistory"), 0), sptField,
                     FxDB(dr("iddccheck"), 0), sptField,
                     FxDB(dr("iddc"), 0), sptField,
                     FxDB(dr("idkategoricheck"), 0), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("status"), 0), sptField,
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
                     FxDB(dr("ccnama"), ""), sptRow)
            Next
            detailCheck = detailCheck.Substring(0, detailCheck.Length - sptRow.Length)

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, detailCheck)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("dcidhistory, dcid, dccabang, dclokasi, dcgudangasal, dcgudangtujuan, dcsumber, dcautonotransaksi, dcnotransaksi, dctgl, dckodepa, dcdimintaoleh, dcdimintaolehkontak, dcmintake, dctgldipakai, dcuraian, dccatatan, dcnoref, dctglnoref, dcstatusts, dcstatusrs, dcstatusrealisasi, dcstatus, dcstatussebelumnya, dcjmlrevisi, dccetakanke, dcinputuser, dcinputtgl, dcmodifikasiuser, dcmodifikasitgl, dcposting, dcpostingtgl, dcisclose, dccustomtext1, dccustomtext2, dccustomtext3, dccustomtext4, dccustomtext5, dccustomint1, dccustomint2, dccustomint3, dccustomdbl1, dccustomdbl2, dccustomdbl3, dccustomdate1, dccustomdate2, dccustomdate3, dccabangnama, dclokasinama, dcgudangasalnama, dcgudangtujuannama, dcdimintaolehkode, dcdimintaolehnama, dcmintakekode, dcmintakenama, dcstatusnama, dcstatussebelumnyanama, dcinputusernama, dcmodifikasiusernama, dcshift, dcidbarang, dcnamabarang, dctipebarang, dchmstart, dchmstop, dchmtotal, dckodebarang" & sptSubParam & "iddcdetailhistory, iddchistory, iddcdetail, iddc, opstart, opend, sbstart, sbend, spstart, spend, rfstart, rfend, bdstart, bdend, cabang, lokasi, gudangasal, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, cabangnama, lokasinama, gudangasalnama, gudangtujuannama, costcenternama, divisinama, subdivisinama, proyeknama" & sptSubParam & "iddccheckhistory, iddchistory, iddccheck, iddc, idkategoricheck, catatan, status, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, ccnama"))

        Return wsResult
    End Function

End Class
