Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_pie_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_Pie_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO M4_Pie_history(SELECT 0, pie.* FROM M4_Pie pie WHERE pie.pieid = '" & idtransaksi & "')"
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
            sql = "SELECT pieidhistory FROM m4_pie_history WHERE pieid = '" & idtransaksi & "' ORDER BY piemodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m4_pie_detail_history (SELECT 0, '" & result(4) & "', pie.* FROM m4_pie_detail pie WHERE pie.idpie = '" & idtransaksi & "' )"
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
    Public Function M4_Pie_HistorySearch(ByVal param As String) As String
        'M4_Grn_HistorySearch --------------------------------------------------------
        'pieidhistory, pieid, piecabang, pielokasi, piesumber, pienotransaksi, pietgl, pieuraian, 
        'piecatatan, piestatus, piestatussebelumnya, pieinputuser, pieinputtgl, piemodifikasiuser, piemodifikasitgl, 
        'piecabangnama, pielokasinama, piestatusnama, piestatussebelumnyanama, pieinputusernama, piemodifikasiusernama

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
            'Filter = Filter.Replace("grnsupplierkode", "c1.kkode")
            'Filter = Filter.Replace("grnsuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select `pie`.`pieidhistory` AS `pieidhistory`,`pie`.`pieid` AS `pieid`,`pie`.`piecabang` AS `piecabang`,`pie`.`pielokasi` AS `pielokasi`,`pie`.`piesumber` AS `piesumber`,`pie`.`pienotransaksi` AS `pienotransaksi`,`pie`.`pietgl` AS `pietgl`,`pie`.`pieuraian` AS `pieuraian`,`pie`.`piecatatan` AS `piecatatan`,`pie`.`piestatus` AS `piestatus`,`pie`.`piestatussebelumnya` AS `piestatussebelumnya`,`pie`.`pieinputuser` AS `pieinputuser`,`pie`.`pieinputtgl` AS `pieinputtgl`,`pie`.`piemodifikasiuser` AS `piemodifikasiuser`,`pie`.`piemodifikasitgl` AS `piemodifikasitgl`,`br`.`bnama` AS `piecabangnama`,`lc`.`lnama` AS `pielokasinama`,`st1`.`nama` AS `piestatusnama`,`st2`.`nama` AS `piestatussebelumnyanama`,`u1`.`unama` AS `pieinputusernama`,`u2`.`unama` AS `piemodifikasiusernama` from ((((((`m4_pie_history` `pie` join `m1_branch` `br` on((`pie`.`piecabang` = `br`.`bkode`))) join `m1_location` `lc` on((`pie`.`pielokasi` = `lc`.`lkode`))) join `m0_status` `st1` on((`pie`.`piestatus` = `st1`.`kode`))) join `m0_status` `st2` on((`pie`.`piestatussebelumnya` = `st2`.`kode`))) join `m0_user` `u1` on((`pie`.`pieinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`pie`.`piemodifikasiuser` = `u2`.`userid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Grn", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("pieid"), 0), sptField,
                     FxDB(dr("pieidhistory"), ""), sptField,
                     FxDB(dr("piecabang"), ""), sptField,
                     FxDB(dr("pielokasi"), ""), sptField,
                     FxDB(dr("piesumber"), ""), sptField,
                     FxDB(dr("pienotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pietgl"), ""), formatTgl), sptField,
                     FxDB(dr("pieuraian"), ""), sptField,
                     FxDB(dr("piecatatan"), ""), sptField,
                     FxDB(dr("piestatus"), 0), sptField,
                     FxDB(dr("piestatussebelumnya"), 0), sptField,
                     FxDB(dr("pieinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pieinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("piemodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("piemodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("piecabangnama"), ""), sptField,
                     FxDB(dr("pielokasinama"), ""), sptField,
                     FxDB(dr("piestatusnama"), ""), sptField,
                     FxDB(dr("piestatussebelumnyanama"), ""), sptField,
                     FxDB(dr("pieinputusernama"), ""), sptField,
                     FxDB(dr("piemodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pieidhistory, pieid, piecabang, pielokasi, piesumber, pienotransaksi, pietgl, pieuraian, piecatatan, piestatus, piestatussebelumnya, pieinputuser, pieinputtgl, piemodifikasiuser, piemodifikasitgl, piecabangnama, pielokasinama, piestatusnama, piestatussebelumnyanama, pieinputusernama, piemodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_PieHistoryGetdataById(ByVal param As String) As String

        'M4_PieHistoryGetdataById Utama --------------------------------------------------------
        'pieidhistory, pieid, piecabang, pielokasi, piesumber, pieautonotransaksi, pienotransaksi, pietgl, 
        'piekodepa, piekontak, piekontakperson, pie1alamat1, pie1alamat2, pie1alamat3, pie2alamat1, 
        'pie2alamat2, pie2alamat3, pieuraian, piecatatan, pienoref, pietglnoref, piestatus, 
        'piestatussebelumnya, piejmlrevisi, piecetakanke, pieinputuser, pieinputtgl, piemodifikasiuser, piemodifikasitgl, 
        'pieposting, piepostingtgl, pieisclose, piecustomtext1, piecustomtext2, piecustomtext3, piecustomtext4, 
        'piecustomtext5, piecustomint1, piecustomint2, piecustomint3, piecustomdbl1, piecustomdbl2, piecustomdbl3, 
        'piecustomdate1, piecustomdate2, piecustomdate3

        'M4_PieHistoryGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idpiedetail, idpie, sumber, idtransaksi, 
        'catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3, cabang, lokasi, 
        'gudang, notransaksi, tgl, supplier, supplierkode, suppliernama, supplierkontak, 
        'termin, uraian, matauang, kurs, totaltransaksi, jmlbayar

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

        Dim NmMemcached As String = "aplikasi1-M4_Pr_history~M4_Pr_Detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "pieidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "pieidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select `pie`.`pieidhistory` AS `pieidhistory`,`pie`.`pieid` AS `pieid`,`pie`.`piecabang` AS `piecabang`,`pie`.`pielokasi` AS `pielokasi`,`pie`.`piesumber` AS `piesumber`,`pie`.`pieautonotransaksi` AS `pieautonotransaksi`,`pie`.`pienotransaksi` AS `pienotransaksi`,`pie`.`pietgl` AS `pietgl`,`pie`.`piekodepa` AS `piekodepa`,`pie`.`piekontak` AS `piekontak`,`pie`.`piekontakperson` AS `piekontakperson`,`pie`.`pie1alamat1` AS `pie1alamat1`,`pie`.`pie1alamat2` AS `pie1alamat2`,`pie`.`pie1alamat3` AS `pie1alamat3`,`pie`.`pie2alamat1` AS `pie2alamat1`,`pie`.`pie2alamat2` AS `pie2alamat2`,`pie`.`pie2alamat3` AS `pie2alamat3`,`pie`.`pieuraian` AS `pieuraian`,`pie`.`piecatatan` AS `piecatatan`,`pie`.`pienoref` AS `pienoref`,`pie`.`pietglnoref` AS `pietglnoref`,`pie`.`piestatus` AS `piestatus`,`pie`.`piestatussebelumnya` AS `piestatussebelumnya`,`pie`.`piejmlrevisi` AS `piejmlrevisi`,`pie`.`piecetakanke` AS `piecetakanke`,`pie`.`pieinputuser` AS `pieinputuser`,`pie`.`pieinputtgl` AS `pieinputtgl`,`pie`.`piemodifikasiuser` AS `piemodifikasiuser`,`pie`.`piemodifikasitgl` AS `piemodifikasitgl`,`pie`.`pieposting` AS `pieposting`,`pie`.`piepostingtgl` AS `piepostingtgl`,`pie`.`pieisclose` AS `pieisclose`,`pie`.`piecustomtext1` AS `piecustomtext1`,`pie`.`piecustomtext2` AS `piecustomtext2`,`pie`.`piecustomtext3` AS `piecustomtext3`,`pie`.`piecustomtext4` AS `piecustomtext4`,`pie`.`piecustomtext5` AS `piecustomtext5`,`pie`.`piecustomint1` AS `piecustomint1`,`pie`.`piecustomint2` AS `piecustomint2`,`pie`.`piecustomint3` AS `piecustomint3`,`pie`.`piecustomdbl1` AS `piecustomdbl1`,`pie`.`piecustomdbl2` AS `piecustomdbl2`,`pie`.`piecustomdbl3` AS `piecustomdbl3`,`pie`.`piecustomdate1` AS `piecustomdate1`,`pie`.`piecustomdate2` AS `piecustomdate2`,`pie`.`piecustomdate3` AS `piecustomdate3`,`pied`.`idhistorydetail` AS `idhistorydetail`,`pied`.`idhistory` AS `idhistory`,`pied`.`idpiedetail` AS `idpiedetail`,`pied`.`idpie` AS `idpie`,`pied`.`sumber` AS `sumber`,`pied`.`idtransaksi` AS `idtransaksi`,`pied`.`catatan` AS `catatan`,`pied`.`urutan` AS `urutan`,`pied`.`isclose` AS `isclose`,`pied`.`customtext1` AS `customtext1`,`pied`.`customtext2` AS `customtext2`,`pied`.`customtext3` AS `customtext3`,`pied`.`customdbl1` AS `customdbl1`,`pied`.`customdbl2` AS `customdbl2`,`pied`.`customdbl3` AS `customdbl3`,`pied`.`customdate1` AS `customdate1`,`pied`.`customdate2` AS `customdate2`,`pied`.`customdate3` AS `customdate3`,ifnull(`ri`.`ricabang`,`prt`.`prtcabang`) AS `cabang`,ifnull(`ri`.`rilokasi`,`prt`.`prtlokasi`) AS `lokasi`,ifnull(`ri`.`rigudang`,`prt`.`prtgudang`) AS `gudang`,ifnull(`ri`.`rinotransaksi`,`prt`.`prtnotransaksi`) AS `notransaksi`,ifnull(`ri`.`ritgl`,`prt`.`prttgl`) AS `tgl`,ifnull(`ri`.`risupplier`,`prt`.`prtsupplier`) AS `supplier`,ifnull(`c`.`kkode`,'') AS `supplierkode`,ifnull(`c`.`knama`,'') AS `suppliernama`,ifnull(`ri`.`risupplierkontak`,`prt`.`prtsupplierkontak`) AS `supplierkontak`,ifnull(`ri`.`ritermin`,`prt`.`prttermin`) AS `termin`,ifnull(`ri`.`riuraian`,`prt`.`prturaian`) AS `uraian`,ifnull(`ri`.`rimatauang`,`prt`.`prtmatauang`) AS `matauang`,ifnull(`ri`.`rikurs`,`prt`.`prtkurs`) AS `kurs`,ifnull(`ri`.`ritotaltransaksi`,`prt`.`prttotaltransaksi`) AS `totaltransaksi`,ifnull(`ri`.`rijmlbayar`,`prt`.`prtjmlbayar`) AS `jmlbayar` from ((((`m4_pie_history` `pie` join `m4_pie_detail_history` `pied` on((`pie`.`pieid` = `pied`.`idpie`))) left join `m4_ri` `ri` on(((`pied`.`sumber` = `ri`.`risumber`) and (`pied`.`idtransaksi` = `ri`.`riid`)))) left join `m4_prt` `prt` on(((`pied`.`sumber` = `prt`.`prtsumber`) and (`pied`.`idtransaksi` = `prt`.`prtid`)))) left join `m1_contact` `c` on((ifnull(`ri`.`risupplier`,`prt`.`prtsupplier`) = `c`.`kid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                     FxDB(drutama("pieidhistory"), 0), sptField,
                     FxDB(drutama("pieid"), ""), sptField,
                     FxDB(drutama("piecabang"), ""), sptField,
                     FxDB(drutama("pielokasi"), ""), sptField,
                     FxDB(drutama("piesumber"), ""), sptField,
                     FxDB(drutama("pieautonotransaksi"), 0), sptField,
                     FxDB(drutama("pienotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pietgl"), ""), formatTgl), sptField,
                     FxDB(drutama("piekodepa"), ""), sptField,
                     FxDB(drutama("piekontak"), ""), sptField,
                     FxDB(drutama("piekontakperson"), ""), sptField,
                     FxDB(drutama("pie1alamat1"), ""), sptField,
                     FxDB(drutama("pie1alamat2"), ""), sptField,
                     FxDB(drutama("pie1alamat3"), ""), sptField,
                     FxDB(drutama("pie2alamat1"), ""), sptField,
                     FxDB(drutama("pie2alamat2"), ""), sptField,
                     FxDB(drutama("pie2alamat3"), ""), sptField,
                     FxDB(drutama("pieuraian"), ""), sptField,
                     FxDB(drutama("piecatatan"), ""), sptField,
                     FxDB(drutama("pienoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pietglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("piestatus"), 0), sptField,
                     FxDB(drutama("piestatussebelumnya"), 0), sptField,
                     FxDB(drutama("piejmlrevisi"), 0), sptField,
                     FxDB(drutama("piecetakanke"), 0), sptField,
                     FxDB(drutama("pieinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pieinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("piemodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("piemodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pieposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("piepostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pieisclose"), 0), sptField,
                     FxDB(drutama("piecustomtext1"), ""), sptField,
                     FxDB(drutama("piecustomtext2"), ""), sptField,
                     FxDB(drutama("piecustomtext3"), ""), sptField,
                     FxDB(drutama("piecustomtext4"), ""), sptField,
                     FxDB(drutama("piecustomtext5"), ""), sptField,
                     FxDB(drutama("piecustomint1"), 0), sptField,
                     FxDB(drutama("piecustomint2"), 0), sptField,
                     FxDB(drutama("piecustomint3"), 0), sptField,
                     FxDB(drutama("piecustomdbl1"), 0), sptField,
                     FxDB(drutama("piecustomdbl2"), 0), sptField,
                     FxDB(drutama("piecustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("piecustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("piecustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("piecustomdate3"), ""), formatTgl))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idhistorydetail"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idpiedetail"), ""), sptField,
                     FxDB(dr("idpie"), ""), sptField,
                     FxDB(dr("sumber"), ""), sptField,
                     FxDB(dr("idtransaksi"), ""), sptField,
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
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("notransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                     FxDB(dr("supplier"), ""), sptField,
                     FxDB(dr("supplierkode"), ""), sptField,
                     FxDB(dr("suppliernama"), ""), sptField,
                     FxDB(dr("supplierkontak"), ""), sptField,
                     FxDB(dr("termin"), ""), sptField,
                     FxDB(dr("uraian"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("totaltransaksi"), 0), sptField,
                     FxDB(dr("jmlbayar"), 0), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, detail)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pieidhistory, pieid, piecabang, pielokasi, piesumber, pieautonotransaksi, pienotransaksi, pietgl, piekodepa, piekontak, piekontakperson, pie1alamat1, pie1alamat2, pie1alamat3, pie2alamat1, pie2alamat2, pie2alamat3, pieuraian, piecatatan, pienoref, pietglnoref, piestatus, piestatussebelumnya, piejmlrevisi, piecetakanke, pieinputuser, pieinputtgl, piemodifikasiuser, piemodifikasitgl, pieposting, piepostingtgl, pieisclose, piecustomtext1, piecustomtext2, piecustomtext3, piecustomtext4, piecustomtext5, piecustomint1, piecustomint2, piecustomint3, piecustomdbl1, piecustomdbl2, piecustomdbl3, piecustomdate1, piecustomdate2, piecustomdate3" & sptSubParam & "idhistorydetail, idhistory, idpiedetail, idpie, sumber, idtransaksi, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, cabang, lokasi, gudang, notransaksi, tgl, supplier, supplierkode, suppliernama, supplierkontak, termin, uraian, matauang, kurs, totaltransaksi, jmlbayar"))

        Return wsResult
    End Function

End Class
