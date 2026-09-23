Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_cpa_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_Cpa_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO M_12_Cpa_history(SELECT 0, cpa.* FROM M_12_Cpa cpa WHERE cpa.cpaid = '" & idtransaksi & "')"
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
            sql = "SELECT cpaidhistory FROM M_12_Cpa_history WHERE cpaid = '" & idtransaksi & "' ORDER BY cpamodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO M_12_Cpa_detail_history (SELECT 0, '" & result(4) & "', cpa.* FROM M_12_Cpa_detail cpa WHERE cpa.idcpa = '" & idtransaksi & "' )"
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
    Public Function M12_Cpa_HistorySearch(ByVal param As String) As String
        'M12_Cpa_HistorySearch --------------------------------------------------------
        'cpaidhistory, cpaid, cpacabang, cpalokasi, cpasumber, cpaautonotransaksi, cpanotransaksi, cpatgl, 
        'cpakodepa, cpakontak, cpakontakperson, cpauraian, cpacatatan, cpastatus, cpastatussebelumnya, 
        'cpajmlrevisi, cpacetakanke, cpaisclose, cpainputuser, cpainputtgl, cpamodifikasiuser, cpamodifikasitgl, 
        'cpaposting, cpapostingtgl, cpacabangnama, cpalokasinama, cpakontakkode, cpakontaknama, cpastatusnama, 
        'cpastatussebelumnyanama, cpainputusernama, cpamodifikasiusernama

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
        sql = "select `cpa`.`cpaidhistory` AS `cpaidhistory`,`cpa`.`cpaid` AS `cpaid`,`cpa`.`cpacabang` AS `cpacabang`,`cpa`.`cpalokasi` AS `cpalokasi`,`cpa`.`cpasumber` AS `cpasumber`,`cpa`.`cpaautonotransaksi` AS `cpaautonotransaksi`,`cpa`.`cpanotransaksi` AS `cpanotransaksi`,`cpa`.`cpatgl` AS `cpatgl`,`cpa`.`cpakodepa` AS `cpakodepa`,`cpa`.`cpakontak` AS `cpakontak`,`cpa`.`cpakontakperson` AS `cpakontakperson`,`cpa`.`cpauraian` AS `cpauraian`,`cpa`.`cpacatatan` AS `cpacatatan`,`cpa`.`cpastatus` AS `cpastatus`,`cpa`.`cpastatussebelumnya` AS `cpastatussebelumnya`,`cpa`.`cpajmlrevisi` AS `cpajmlrevisi`,`cpa`.`cpacetakanke` AS `cpacetakanke`,`cpa`.`cpaisclose` AS `cpaisclose`,`cpa`.`cpainputuser` AS `cpainputuser`,`cpa`.`cpainputtgl` AS `cpainputtgl`,`cpa`.`cpamodifikasiuser` AS `cpamodifikasiuser`,`cpa`.`cpamodifikasitgl` AS `cpamodifikasitgl`,`cpa`.`cpaposting` AS `cpaposting`,`cpa`.`cpapostingtgl` AS `cpapostingtgl`,`br`.`bnama` AS `cpacabangnama`,`lc`.`lnama` AS `cpalokasinama`,`c1`.`kkode` AS `cpakontakkode`,`c1`.`knama` AS `cpakontaknama`,`st1`.`nama` AS `cpastatusnama`,`st2`.`nama` AS `cpastatussebelumnyanama`,`u1`.`unama` AS `cpainputusernama`,`u2`.`unama` AS `cpamodifikasiusernama` from (((((((`m_12_cpa_history` `cpa` join `m0_status` `st1` on((`cpa`.`cpastatus` = `st1`.`kode`))) join `m0_status` `st2` on((`cpa`.`cpastatussebelumnya` = `st2`.`kode`))) left join `m1_branch` `br` on((`cpa`.`cpacabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`cpa`.`cpalokasi` = `lc`.`lkode`))) left join `m1_contact` `c1` on((`cpa`.`cpakontak` = `c1`.`kid`))) left join `m0_user` `u1` on((`cpa`.`cpainputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`cpa`.`cpamodifikasiuser` = `u2`.`userid`)))"

        dt = AmbilData("aplikasi1-M_12_Cpa_V", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("cpaidhistory"), ""), sptField,
                     FxDB(dr("cpaid"), ""), sptField,
                     FxDB(dr("cpacabang"), ""), sptField,
                     FxDB(dr("cpalokasi"), ""), sptField,
                     FxDB(dr("cpasumber"), ""), sptField,
                     FxDB(dr("cpaautonotransaksi"), 0), sptField,
                     FxDB(dr("cpanotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("cpatgl"), ""), formatTgl), sptField,
                     FxDB(dr("cpakodepa"), ""), sptField,
                     FxDB(dr("cpakontak"), ""), sptField,
                     FxDB(dr("cpakontakperson"), ""), sptField,
                     FxDB(dr("cpauraian"), ""), sptField,
                     FxDB(dr("cpacatatan"), ""), sptField,
                     FxDB(dr("cpastatus"), 0), sptField,
                     FxDB(dr("cpastatussebelumnya"), 0), sptField,
                     FxDB(dr("cpajmlrevisi"), 0), sptField,
                     FxDB(dr("cpacetakanke"), 0), sptField,
                     FxDB(dr("cpaisclose"), 0), sptField,
                     FxDB(dr("cpainputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("cpainputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("cpamodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("cpamodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("cpaposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("cpapostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("cpacabangnama"), ""), sptField,
                     FxDB(dr("cpalokasinama"), ""), sptField,
                     FxDB(dr("cpakontakkode"), ""), sptField,
                     FxDB(dr("cpakontaknama"), ""), sptField,
                     FxDB(dr("cpastatusnama"), ""), sptField,
                     FxDB(dr("cpastatussebelumnyanama"), ""), sptField,
                     FxDB(dr("cpainputusernama"), ""), sptField,
                     FxDB(dr("cpamodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("cpaidhistory, cpaid, cpacabang, cpalokasi, cpasumber, cpaautonotransaksi, cpanotransaksi, cpatgl, cpakodepa, cpakontak, cpakontakperson, cpauraian, cpacatatan, cpastatus, cpastatussebelumnya, cpajmlrevisi, cpacetakanke, cpaisclose, cpainputuser, cpainputtgl, cpamodifikasiuser, cpamodifikasitgl, cpaposting, cpapostingtgl, cpacabangnama, cpalokasinama, cpakontakkode, cpakontaknama, cpastatusnama, cpastatussebelumnyanama, cpainputusernama, cpamodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_CpaHistoryGetdataById(ByVal param As String) As String
        'M12_CpaHistoryGetdataById Utama --------------------------------------------------------
        'cpaidhistory, cpaid, cpacabang, cpalokasi, cpasumber, cpaautonotransaksi, cpanotransaksi, cpatgl, 
        'cpakodepa, cpakontak, cpakontakperson, cpauraian, cpacatatan, cpastatus, cpastatussebelumnya, 
        'cpajmlrevisi, cpacetakanke, cpaisclose, cpainputuser, cpainputtgl, cpamodifikasiuser, cpamodifikasitgl, 
        'cpaposting, cpapostingtgl, cpacustomtext1, cpacustomtext2, cpacustomtext3, cpacustomtext4, cpacustomtext5, 
        'cpacustomint1, cpacustomint2, cpacustomint3, cpacustomdbl1, cpacustomdbl2, cpacustomdbl3, cpacustomdate1, 
        'cpacustomdate2, cpacustomdate3, cpacabangnama, cpalokasinama, cpakontakkode, cpakontaknama, cpastatusnama, 
        'cpastatussebelumnyanama, cpainputusernama, cpamodifikasiusernama

        'M12_CpaHistoryGetdataById Detail --------------------------------------------------------
        'idhistorydetail, idhistory, idcpadetail, idcpa, kontak, poinlama, 
        'poinmasuk, poinkeluar, poinbaru, catatan, urutan, isclose, customtext1, 
        'customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3, kontakkode, kontaknama, kontakkategori, kontakkategorinama, kontakkategorisalesman, kontakkategorisalesmannama, 
        'kontakarea, kontakareanama

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'icpaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = "", strResultData As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = ""
        Dim dt As New DataTable

        Dim utama As String = "", detail As String = "", idtransaksi As String = ""

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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        Dim NmMemcached As String = "aplikasi1-M_12_Cpa~M_12_Cpa_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "cpaid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "cpaid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select `cpa`.`cpaidhistory` AS `cpaidhistory`,`cpa`.`cpaid` AS `cpaid`,`cpa`.`cpacabang` AS `cpacabang`,`cpa`.`cpalokasi` AS `cpalokasi`,`cpa`.`cpasumber` AS `cpasumber`,`cpa`.`cpaautonotransaksi` AS `cpaautonotransaksi`,`cpa`.`cpanotransaksi` AS `cpanotransaksi`,`cpa`.`cpatgl` AS `cpatgl`,`cpa`.`cpakodepa` AS `cpakodepa`,`cpa`.`cpakontak` AS `cpakontak`,`cpa`.`cpakontakperson` AS `cpakontakperson`,`cpa`.`cpauraian` AS `cpauraian`,`cpa`.`cpacatatan` AS `cpacatatan`,`cpa`.`cpastatus` AS `cpastatus`,`cpa`.`cpastatussebelumnya` AS `cpastatussebelumnya`,`cpa`.`cpajmlrevisi` AS `cpajmlrevisi`,`cpa`.`cpacetakanke` AS `cpacetakanke`,`cpa`.`cpaisclose` AS `cpaisclose`,`cpa`.`cpainputuser` AS `cpainputuser`,`cpa`.`cpainputtgl` AS `cpainputtgl`,`cpa`.`cpamodifikasiuser` AS `cpamodifikasiuser`,`cpa`.`cpamodifikasitgl` AS `cpamodifikasitgl`,`cpa`.`cpaposting` AS `cpaposting`,`cpa`.`cpapostingtgl` AS `cpapostingtgl`,`cpa`.`cpacustomtext1` AS `cpacustomtext1`,`cpa`.`cpacustomtext2` AS `cpacustomtext2`,`cpa`.`cpacustomtext3` AS `cpacustomtext3`,`cpa`.`cpacustomtext4` AS `cpacustomtext4`,`cpa`.`cpacustomtext5` AS `cpacustomtext5`,`cpa`.`cpacustomint1` AS `cpacustomint1`,`cpa`.`cpacustomint2` AS `cpacustomint2`,`cpa`.`cpacustomint3` AS `cpacustomint3`,`cpa`.`cpacustomdbl1` AS `cpacustomdbl1`,`cpa`.`cpacustomdbl2` AS `cpacustomdbl2`,`cpa`.`cpacustomdbl3` AS `cpacustomdbl3`,`cpa`.`cpacustomdate1` AS `cpacustomdate1`,`cpa`.`cpacustomdate2` AS `cpacustomdate2`,`cpa`.`cpacustomdate3` AS `cpacustomdate3`,`br`.`bnama` AS `cpacabangnama`,`lc`.`lnama` AS `cpalokasinama`,`c1`.`kkode` AS `cpakontakkode`,`c1`.`knama` AS `cpakontaknama`,`st1`.`nama` AS `cpastatusnama`,`st2`.`nama` AS `cpastatussebelumnyanama`,`u1`.`unama` AS `cpainputusernama`,`u2`.`unama` AS `cpamodifikasiusernama`,`cpad`.`idhistorydetail` AS `idhistorydetail`,`cpad`.`idhistory` AS `idhistory`,`cpad`.`idcpadetail` AS `idcpadetail`,`cpad`.`idcpa` AS `idcpa`,`cpad`.`kontak` AS `kontak`,`cpad`.`poinlama` AS `poinlama`,`cpad`.`poinmasuk` AS `poinmasuk`,`cpad`.`poinkeluar` AS `poinkeluar`,`cpad`.`poinbaru` AS `poinbaru`,`cpad`.`catatan` AS `catatan`,`cpad`.`urutan` AS `urutan`,`cpad`.`isclose` AS `isclose`,`cpad`.`customtext1` AS `customtext1`,`cpad`.`customtext2` AS `customtext2`,`cpad`.`customtext3` AS `customtext3`,`cpad`.`customdbl1` AS `customdbl1`,`cpad`.`customdbl2` AS `customdbl2`,`cpad`.`customdbl3` AS `customdbl3`,`cpad`.`customdate1` AS `customdate1`,`cpad`.`customdate2` AS `customdate2`,`cpad`.`customdate3` AS `customdate3`,`c2`.`kkode` AS `kontakkode`,`c2`.`knama` AS `kontaknama`,`c2`.`kkategori` AS `kontakkategori`,`cc`.`ccnama` AS `kontakkategorinama`,`c2`.`kkategorisalesman` AS `kontakkategorisalesman`,`sc`.`scnama` AS `kontakkategorisalesmannama`,`c2`.`karea` AS `kontakarea`,`a`.`anama` AS `kontakareanama` from ((((((((((((`m_12_cpa_history` `cpa` join `m0_status` `st1` on((`cpa`.`cpastatus` = `st1`.`kode`))) join `m0_status` `st2` on((`cpa`.`cpastatussebelumnya` = `st2`.`kode`))) join `m_12_cpa_detail_history` `cpad` on((`cpa`.`cpaidhistory` = `cpad`.`idhistory`))) left join `m1_branch` `br` on((`cpa`.`cpacabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`cpa`.`cpalokasi` = `lc`.`lkode`))) left join `m1_contact` `c1` on((`cpa`.`cpakontak` = `c1`.`kid`))) left join `m0_user` `u1` on((`cpa`.`cpainputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`cpa`.`cpamodifikasiuser` = `u2`.`userid`))) left join `m1_contact` `c2` on((`cpad`.`kontak` = `c2`.`kid`))) left join `m1_contact_category` `cc` on((`c2`.`kkategori` = `cc`.`cckode`))) left join `m1_salesman_category` `sc` on((`c2`.`kkategorisalesman` = `sc`.`sckode`))) left join `m1_area` `a` on((`c2`.`karea` = `a`.`akode`)))"

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                     FxDB(drutama("cpaidhistory"), ""), sptField,
                     FxDB(drutama("cpaid"), ""), sptField,
                     FxDB(drutama("cpacabang"), ""), sptField,
                     FxDB(drutama("cpalokasi"), ""), sptField,
                     FxDB(drutama("cpasumber"), ""), sptField,
                     FxDB(drutama("cpaautonotransaksi"), 0), sptField,
                     FxDB(drutama("cpanotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("cpatgl"), ""), formatTgl), sptField,
                     FxDB(drutama("cpakodepa"), ""), sptField,
                     FxDB(drutama("cpakontak"), ""), sptField,
                     FxDB(drutama("cpakontakperson"), ""), sptField,
                     FxDB(drutama("cpauraian"), ""), sptField,
                     FxDB(drutama("cpacatatan"), ""), sptField,
                     FxDB(drutama("cpastatus"), 0), sptField,
                     FxDB(drutama("cpastatussebelumnya"), 0), sptField,
                     FxDB(drutama("cpajmlrevisi"), 0), sptField,
                     FxDB(drutama("cpacetakanke"), 0), sptField,
                     FxDB(drutama("cpaisclose"), 0), sptField,
                     FxDB(drutama("cpainputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("cpainputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("cpamodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("cpamodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("cpaposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("cpapostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("cpacustomtext1"), ""), sptField,
                     FxDB(drutama("cpacustomtext2"), ""), sptField,
                     FxDB(drutama("cpacustomtext3"), ""), sptField,
                     FxDB(drutama("cpacustomtext4"), ""), sptField,
                     FxDB(drutama("cpacustomtext5"), ""), sptField,
                     FxDB(drutama("cpacustomint1"), 0), sptField,
                     FxDB(drutama("cpacustomint2"), 0), sptField,
                     FxDB(drutama("cpacustomint3"), 0), sptField,
                     FxDB(drutama("cpacustomdbl1"), 0), sptField,
                     FxDB(drutama("cpacustomdbl2"), 0), sptField,
                     FxDB(drutama("cpacustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("cpacustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("cpacustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("cpacustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("cpacabangnama"), ""), sptField,
                     FxDB(drutama("cpalokasinama"), ""), sptField,
                     FxDB(drutama("cpakontakkode"), ""), sptField,
                     FxDB(drutama("cpakontaknama"), ""), sptField,
                     FxDB(drutama("cpastatusnama"), ""), sptField,
                     FxDB(drutama("cpastatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("cpainputusernama"), ""), sptField,
                     FxDB(drutama("cpamodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idcpadetail"), ""), sptField,
                     FxDB(dr("idcpa"), ""), sptField,
                     FxDB(dr("kontak"), ""), sptField,
                     FxDB(dr("poinlama"), 0), sptField,
                     FxDB(dr("poinmasuk"), 0), sptField,
                     FxDB(dr("poinkeluar"), 0), sptField,
                     FxDB(dr("poinbaru"), 0), sptField,
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
                     FxDB(dr("kontakkode"), ""), sptField,
                     FxDB(dr("kontaknama"), ""), sptField,
                     FxDB(dr("kontakkategori"), ""), sptField,
                     FxDB(dr("kontakkategorinama"), ""), sptField,
                     FxDB(dr("kontakkategorisalesman"), ""), sptField,
                     FxDB(dr("kontakkategorisalesmannama"), ""), sptField,
                     FxDB(dr("kontakarea"), ""), sptField,
                     FxDB(dr("kontakareanama"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

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
        strResultData = String.Concat(utama, sptSubParam, detail)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("cpaid, cpacabang, cpalokasi, cpasumber, cpaautonotransaksi, cpanotransaksi, cpatgl, cpakodepa, cpakontak, cpakontakperson, cpauraian, cpacatatan, cpastatus, cpastatussebelumnya, cpajmlrevisi, cpacetakanke, cpaisclose, cpainputuser, cpainputtgl, cpamodifikasiuser, cpamodifikasitgl, cpaposting, cpapostingtgl, cpacustomtext1, cpacustomtext2, cpacustomtext3, cpacustomtext4, cpacustomtext5, cpacustomint1, cpacustomint2, cpacustomint3, cpacustomdbl1, cpacustomdbl2, cpacustomdbl3, cpacustomdate1, cpacustomdate2, cpacustomdate3, cpacabangnama, cpalokasinama, cpakontakkode, cpakontaknama, cpastatusnama, cpastatussebelumnyanama, cpainputusernama, cpamodifikasiusernama" & sptSubParam & "idcpadetail, idcpa, kontak, poinlama, poinmasuk, poinkeluar, poinbaru, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kontakkode, kontaknama, kontakkategori, kontakkategorinama, kontakkategorisalesman, kontakkategorisalesmannama, kontakarea, kontakareanama"))

        Return wsResult
    End Function

End Class
