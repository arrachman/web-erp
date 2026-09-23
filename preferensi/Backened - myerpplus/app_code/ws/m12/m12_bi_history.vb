Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_bi_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_Bi_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m_12_bi_history(SELECT 0, bi.* FROM m_12_bi bi WHERE bi.biid = '" & idtransaksi & "')"
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
            sql = "SELECT biidhistory FROM m_12_bi_history WHERE biid = '" & idtransaksi & "' ORDER BY bimodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m_12_bi_detail_history (SELECT 0, '" & result(4) & "', bi.* FROM m_12_bi_detail bi WHERE bi.idbi = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY DETAIL -------------------------------

            'PROSES INSERT HISTORY BONUS --------------------------------------
            sql = "INSERT INTO m_12_bi_bonus_history (SELECT 0, '" & result(4) & "', bi.* FROM m_12_bi_bonus bi WHERE bi.idbi = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY BONUS -------------------------------

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
    Public Function M12_Bi_HistorySearch(ByVal param As String) As String
        'M12_Bi_HistorySearch --------------------------------------------------------
        'biid, bicabang, bilokasi, bisumber, bikategoripos, biautonotransaksi, binotransaksi, 
        'bitgl, bikodepa, bikontak, bikontakperson, biuraian, bicatatan, bistatus, 
        'bistatussebelumnya, bijmlrevisi, bicetakanke, biisclose, biinputuser, biinputtgl, bimodifikasiuser, 
        'bimodifikasitgl, biposting, bipostingtgl, bicustomtext1, bicustomtext2, bicustomtext3, bicustomtext4, 
        'bicustomtext5, bicustomint1, bicustomint2, bicustomint3, bicustomdbl1, bicustomdbl2, bicustomdbl3, 
        'bicustomdate1, bicustomdate2, bicustomdate3, bicabangnama, bilokasinama, bikontakkode, 
        'bikontaknama, bistatusnama, bistatussebelumnyanama, biinputusernama, bimodifikasiusernama

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
            formatTglWaktu = "yyy-MM-dd Hh:mm:ss"
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
        sql = "select `bi`.`biidhistory` AS `biidhistory`,`bi`.`biid` AS `biid`,`bi`.`bicabang` AS `bicabang`,`bi`.`bilokasi` AS `bilokasi`,`bi`.`bisumber` AS `bisumber`,`bi`.`biautonotransaksi` AS `biautonotransaksi`,`bi`.`binotransaksi` AS `binotransaksi`,`bi`.`bitgl` AS `bitgl`,`bi`.`bikodepa` AS `bikodepa`,`bi`.`bikontak` AS `bikontak`,`bi`.`bikontakperson` AS `bikontakperson`,`bi`.`bikategoripos` AS `bikategoripos`,`bi`.`biuraian` AS `biuraian`,`bi`.`bicatatan` AS `bicatatan`,`bi`.`bistatus` AS `bistatus`,`bi`.`bistatussebelumnya` AS `bistatussebelumnya`,`bi`.`bijmlrevisi` AS `bijmlrevisi`,`bi`.`bicetakanke` AS `bicetakanke`,`bi`.`biisclose` AS `biisclose`,`bi`.`biinputuser` AS `biinputuser`,`bi`.`biinputtgl` AS `biinputtgl`,`bi`.`bimodifikasiuser` AS `bimodifikasiuser`,`bi`.`bimodifikasitgl` AS `bimodifikasitgl`,`bi`.`biposting` AS `biposting`,`bi`.`bipostingtgl` AS `bipostingtgl`,`bi`.`bicustomtext1` AS `bicustomtext1`,`bi`.`bicustomtext2` AS `bicustomtext2`,`bi`.`bicustomtext3` AS `bicustomtext3`,`bi`.`bicustomtext4` AS `bicustomtext4`,`bi`.`bicustomtext5` AS `bicustomtext5`,`bi`.`bicustomint1` AS `bicustomint1`,`bi`.`bicustomint2` AS `bicustomint2`,`bi`.`bicustomint3` AS `bicustomint3`,`bi`.`bicustomdbl1` AS `bicustomdbl1`,`bi`.`bicustomdbl2` AS `bicustomdbl2`,`bi`.`bicustomdbl3` AS `bicustomdbl3`,`bi`.`bicustomdate1` AS `bicustomdate1`,`bi`.`bicustomdate2` AS `bicustomdate2`,`bi`.`bicustomdate3` AS `bicustomdate3`,`br`.`bnama` AS `bicabangnama`,`lc`.`lnama` AS `bilokasinama`,`c`.`kkode` AS `bikontakkode`,`c`.`knama` AS `bikontaknama`,`st1`.`nama` AS `bistatusnama`,`st2`.`nama` AS `bistatussebelumnyanama`,`u1`.`unama` AS `biinputusernama`,`u2`.`unama` AS `bimodifikasiusernama` from (((((((`m_12_bi_history` `bi` left join `m1_branch` `br` on((`bi`.`bicabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`bi`.`bilokasi` = `lc`.`lkode`))) left join `m1_contact` `c` on((`bi`.`bikontak` = `c`.`kid`))) left join `m0_status` `st1` on((`bi`.`bistatus` = `st1`.`kode`))) left join `m0_status` `st2` on((`bi`.`bistatussebelumnya` = `st2`.`kode`))) left join `m0_user` `u1` on((`bi`.`biinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`bi`.`bimodifikasiuser` = `u2`.`userid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M12_Bi", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("biidhistory"), 0), sptField,
                             FxDB(dr("biid"), 0), sptField,
                             FxDB(dr("bicabang"), ""), sptField,
                             FxDB(dr("bilokasi"), ""), sptField,
                             FxDB(dr("bisumber"), ""), sptField,
                             FxDB(dr("bikategoripos"), ""), sptField,
                             FxDB(dr("biautonotransaksi"), 0), sptField,
                             FxDB(dr("binotransaksi"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("bitgl"), ""), formatTgl), sptField,
                             FxDB(dr("bikodepa"), ""), sptField,
                             FxDB(dr("bikontak"), ""), sptField,
                             FxDB(dr("bikontakperson"), ""), sptField,
                             FxDB(dr("biuraian"), ""), sptField,
                             FxDB(dr("bicatatan"), ""), sptField,
                             FxDB(dr("bistatus"), 0), sptField,
                             FxDB(dr("bistatussebelumnya"), 0), sptField,
                             FxDB(dr("bijmlrevisi"), 0), sptField,
                             FxDB(dr("bicetakanke"), 0), sptField,
                             FxDB(dr("biisclose"), 0), sptField,
                             FxDB(dr("biinputuser"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("biinputtgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("bimodifikasiuser"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("bimodifikasitgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("biposting"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("bipostingtgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("bicustomtext1"), ""), sptField,
                             FxDB(dr("bicustomtext2"), ""), sptField,
                             FxDB(dr("bicustomtext3"), ""), sptField,
                             FxDB(dr("bicustomtext4"), ""), sptField,
                             FxDB(dr("bicustomtext5"), ""), sptField,
                             FxDB(dr("bicustomint1"), 0), sptField,
                             FxDB(dr("bicustomint2"), 0), sptField,
                             FxDB(dr("bicustomint3"), 0), sptField,
                             FxDB(dr("bicustomdbl1"), 0), sptField,
                             FxDB(dr("bicustomdbl2"), 0), sptField,
                             FxDB(dr("bicustomdbl3"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("bicustomdate1"), ""), formatTgl), sptField,
                             AsFormatTanggal(FxDB(dr("bicustomdate2"), ""), formatTgl), sptField,
                             AsFormatTanggal(FxDB(dr("bicustomdate3"), ""), formatTgl), sptField,
                             FxDB(dr("bicabangnama"), ""), sptField,
                             FxDB(dr("bilokasinama"), ""), sptField,
                             FxDB(dr("bikontakkode"), ""), sptField,
                             FxDB(dr("bikontaknama"), ""), sptField,
                             FxDB(dr("bistatusnama"), ""), sptField,
                             FxDB(dr("bistatussebelumnyanama"), ""), sptField,
                             FxDB(dr("biinputusernama"), ""), sptField,
                             FxDB(dr("bimodifikasiusernama"), ""), sptRow)

            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = pg1.isPaging
            resultPaging(1) = pg1.isNext
            resultPaging(2) = pg1.isPrev
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Transaction data not found. - 1"
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("biidhistory, biid, bicabang, bilokasi, bisumber, bikategoripos, biautonotransaksi, binotransaksi, bitgl, bikodepa, bikontak, bikontakperson, biuraian, bicatatan, bistatus, bistatussebelumnya, bijmlrevisi, bicetakanke, biisclose, biinputuser, biinputtgl, bimodifikasiuser, bimodifikasitgl, biposting, bipostingtgl, bicustomtext1, bicustomtext2, bicustomtext3, bicustomtext4, bicustomtext5, bicustomint1, bicustomint2, bicustomint3, bicustomdbl1, bicustomdbl2, bicustomdbl3, bicustomdate1, bicustomdate2, bicustomdate3, bicabangnama, bilokasinama, bikontakkode, bikontaknama, bistatusnama, bistatussebelumnyanama, biinputusernama, bimodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_BiHistoryGetdataById(ByVal param As String) As String

        'M12_BiHistoryGetdataById Utama --------------------------------------------------------
        'biidhistory, biid, bicabang, bilokasi, bisumber, bikategoripos, biautonotransaksi, binotransaksi, 
        'bitgl, bikodepa, bikontak, bikontakperson, biuraian, bicatatan, bistatus, 
        'bistatussebelumnya, bijmlrevisi, bicetakanke, biisclose, biinputuser, biinputtgl, bimodifikasiuser, 
        'bimodifikasitgl, biposting, bipostingtgl, bicustomtext1, bicustomtext2, bicustomtext3, bicustomtext4, 
        'bicustomtext5, bicustomint1, bicustomint2, bicustomint3, bicustomdbl1, bicustomdbl2, bicustomdbl3, 
        'bicustomdate1, bicustomdate2, bicustomdate3, bicabangnama, bilokasinama, bikontakkode, 
        'bikontaknama, bistatusnama, bistatussebelumnyanama, biinputusernama, bimodifikasiusernama, bikategoriposnama, bijeniskategori

        'M12_BiHistoryGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idbidetail, bikategori, idbarang, operator, jml1, jml2, customtext1, 
        'customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, tgl1, 
        'tgl2, nopromo, kodebarang, namabarang, catatan, urutan

        'M12_BiHistoryGetdataById Bonus -------------------------------------------------------
        'idhistorybonus, idhistory, idbonus, idbidetail, idbarang, jml, satuan, customtext1, customtext2, 
        'customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, namabarang, urutan



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

        Dim utama As String = "", detail As String = "", bonus As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M12_Bi_History~M12_Bi_Detail_History-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "biidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "biidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        sql = "select `bi`.`biidhistory` AS `biidhistory`,`bi`.`biid` AS `biid`,`bi`.`bicabang` AS `bicabang`,`bi`.`bilokasi` AS `bilokasi`,`bi`.`bisumber` AS `bisumber`,`bi`.`bikategoripos` AS `bikategoripos`,`bi`.`biautonotransaksi` AS `biautonotransaksi`,`bi`.`binotransaksi` AS `binotransaksi`,`bi`.`bitgl` AS `bitgl`,`bi`.`bikodepa` AS `bikodepa`,`bi`.`bikontak` AS `bikontak`,`bi`.`bikontakperson` AS `bikontakperson`,`bi`.`biuraian` AS `biuraian`,`bi`.`bicatatan` AS `bicatatan`,`bi`.`bistatus` AS `bistatus`,`bi`.`bistatussebelumnya` AS `bistatussebelumnya`,`bi`.`bijmlrevisi` AS `bijmlrevisi`,`bi`.`bicetakanke` AS `bicetakanke`,`bi`.`biisclose` AS `biisclose`,`bi`.`biinputuser` AS `biinputuser`,`bi`.`biinputtgl` AS `biinputtgl`,`bi`.`bimodifikasiuser` AS `bimodifikasiuser`,`bi`.`bimodifikasitgl` AS `bimodifikasitgl`,`bi`.`biposting` AS `biposting`,`bi`.`bipostingtgl` AS `bipostingtgl`,`bi`.`bicustomtext1` AS `bicustomtext1`,`bi`.`bicustomtext2` AS `bicustomtext2`,`bi`.`bicustomtext3` AS `bicustomtext3`,`bi`.`bicustomtext4` AS `bicustomtext4`,`bi`.`bicustomtext5` AS `bicustomtext5`,`bi`.`bicustomint1` AS `bicustomint1`,`bi`.`bicustomint2` AS `bicustomint2`,`bi`.`bicustomint3` AS `bicustomint3`,`bi`.`bicustomdbl1` AS `bicustomdbl1`,`bi`.`bicustomdbl2` AS `bicustomdbl2`,`bi`.`bicustomdbl3` AS `bicustomdbl3`,`bi`.`bicustomdate1` AS `bicustomdate1`,`bi`.`bicustomdate2` AS `bicustomdate2`,`bi`.`bicustomdate3` AS `bicustomdate3`,`br`.`bnama` AS `bicabangnama`,`lc`.`lnama` AS `bilokasinama`,`c`.`kkode` AS `bikontakkode`,`c`.`knama` AS `bikontaknama`,`st1`.`nama` AS `bistatusnama`,`st2`.`nama` AS `bistatussebelumnyanama`,`u1`.`unama` AS `biinputusernama`,`u2`.`unama` AS `bimodifikasiusernama`,`pc`.`pcnama` AS `bikategoriposnama`,`bi`.`bijeniskategori` AS `bijeniskategori`,`bid`.`idhistorydetail` AS `idhistorydetail`,`bid`.`idhistory` AS `idhistory`,`bid`.`idbidetail` AS `idbidetail`,`bid`.`idbi` AS `idbi`,`bid`.`bikategori` AS `bikategori`,`bid`.`idbarang` AS `idbarang`,`bid`.`operator` AS `operator`,`bid`.`jml1` AS `jml1`,`bid`.`jml2` AS `jml2`,`bid`.`customtext1` AS `customtext1`,`bid`.`customtext2` AS `customtext2`,`bid`.`customtext3` AS `customtext3`,`bid`.`customtext4` AS `customtext4`,`bid`.`customtext5` AS `customtext5`,`bid`.`customint1` AS `customint1`,`bid`.`customint2` AS `customint2`,`bid`.`customint3` AS `customint3`,`bid`.`customdbl1` AS `customdbl1`,`bid`.`customdbl2` AS `customdbl2`,`bid`.`customdbl3` AS `customdbl3`,`bid`.`customdate1` AS `customdate1`,`bid`.`customdate2` AS `customdate2`,`bid`.`customdate3` AS `customdate3`,`bid`.`tgl1` AS `tgl1`,`bid`.`tgl2` AS `tgl2`,`bid`.`nopromo` AS `nopromo`,`i`.`bkode` AS `kodebarang`,`i`.`bnama` AS `namabarang`, `bid`.`catatan` AS `catatan`, `bid`.`urutan` AS `urutan`  from ((((((((((`m_12_bi_history` `bi` join `m_12_bi_detail_history` `bid` on((`bi`.`biidhistory` = `bid`.`idhistory`))) left join `m1_branch` `br` on((`bi`.`bicabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`bi`.`bilokasi` = `lc`.`lkode`))) left join `m1_contact` `c` on((`bi`.`bikontak` = `c`.`kid`))) left join `m0_status` `st1` on((`bi`.`bistatus` = `st1`.`kode`))) left join `m0_status` `st2` on((`bi`.`bistatussebelumnya` = `st2`.`kode`))) left join `m0_user` `u1` on((`bi`.`biinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`bi`.`bimodifikasiuser` = `u2`.`userid`))) left join `m1_item` `i` on((`bid`.`idbarang` = `i`.`bid`)))  left join `m_12_pos_category` `pc` on((`bi`.`bikategoripos` = `pc`.`pckode`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("biidhistory"), 0), sptField,
                     FxDB(drutama("biid"), 0), sptField,
                     FxDB(drutama("bicabang"), ""), sptField,
                     FxDB(drutama("bilokasi"), ""), sptField,
                     FxDB(drutama("bisumber"), ""), sptField,
                     FxDB(drutama("bikategoripos"), ""), sptField,
                     FxDB(drutama("biautonotransaksi"), 0), sptField,
                     FxDB(drutama("binotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("bitgl"), ""), formatTgl), sptField,
                     FxDB(drutama("bikodepa"), ""), sptField,
                     FxDB(drutama("bikontak"), ""), sptField,
                     FxDB(drutama("bikontakperson"), ""), sptField,
                     FxDB(drutama("biuraian"), ""), sptField,
                     FxDB(drutama("bicatatan"), ""), sptField,
                     FxDB(drutama("bistatus"), 0), sptField,
                     FxDB(drutama("bistatussebelumnya"), 0), sptField,
                     FxDB(drutama("bijmlrevisi"), 0), sptField,
                     FxDB(drutama("bicetakanke"), 0), sptField,
                     FxDB(drutama("biisclose"), 0), sptField,
                     FxDB(drutama("biinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("biinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("bimodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("bimodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("biposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bipostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("bicustomtext1"), ""), sptField,
                     FxDB(drutama("bicustomtext2"), ""), sptField,
                     FxDB(drutama("bicustomtext3"), ""), sptField,
                     FxDB(drutama("bicustomtext4"), ""), sptField,
                     FxDB(drutama("bicustomtext5"), ""), sptField,
                     FxDB(drutama("bicustomint1"), 0), sptField,
                     FxDB(drutama("bicustomint2"), 0), sptField,
                     FxDB(drutama("bicustomint3"), 0), sptField,
                     FxDB(drutama("bicustomdbl1"), 0), sptField,
                     FxDB(drutama("bicustomdbl2"), 0), sptField,
                     FxDB(drutama("bicustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bicustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("bicustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("bicustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("bicabangnama"), ""), sptField,
                     FxDB(drutama("bilokasinama"), ""), sptField,
                     FxDB(drutama("bikontakkode"), ""), sptField,
                     FxDB(drutama("bikontaknama"), ""), sptField,
                     FxDB(drutama("bistatusnama"), ""), sptField,
                     FxDB(drutama("bistatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("biinputusernama"), ""), sptField,
                     FxDB(drutama("bimodifikasiusernama"), ""), sptField,
                     FxDB(drutama("bikategoriposnama"), ""), sptField,
                     FxDB(drutama("bijeniskategori"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idhistorydetail"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idbidetail"), 0), sptField,
                     FxDB(dr("idbi"), 0), sptField,
                     FxDB(dr("bikategori"), ""), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("operator"), 0), sptField,
                     FxDB(dr("jml1"), 0), sptField,
                     FxDB(dr("jml2"), 0), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customtext4"), ""), sptField,
                     FxDB(dr("customtext5"), ""), sptField,
                     FxDB(dr("customint1"), 0), sptField,
                     FxDB(dr("customint2"), 0), sptField,
                     FxDB(dr("customint3"), 0), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("tgl1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("tgl2"), ""), formatTgl), sptField,
                     FxDB(dr("nopromo"), ""), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA BONUS
            sql = "select `bib`.`idhistorybonus` AS `idhistorybonus`, `bib`.`idhistory` AS `idhistory`, `bib`.`idbonus` AS `idbonus`, `bib`.`idbidetail` AS `idbidetail`,`bib`.`idbi` AS `idbi`,`bib`.`idbarang` AS `idbarang`,`bib`.`jml` AS `jml`,`bib`.`satuan` AS `satuan`,`bib`.`customtext1` AS `customtext1`,`bib`.`customtext2` AS `customtext2`,`bib`.`customtext3` AS `customtext3`,`bib`.`customtext4` AS `customtext4`,`bib`.`customtext5` AS `customtext5`,`bib`.`customint1` AS `customint1`,`bib`.`customint2` AS `customint2`,`bib`.`customint3` AS `customint3`,`bib`.`customdbl1` AS `customdbl1`,`bib`.`customdbl2` AS `customdbl2`,`bib`.`customdbl3` AS `customdbl3`,`bib`.`customdate1` AS `customdate1`,`bib`.`customdate2` AS `customdate2`,`bib`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`i`.`bnama` AS `namabarang`,`bib`.`urutan` AS `urutan` FROM `m_12_bi_bonus_history` `bib` JOIN m1_item `i` ON (`bib`.`idbarang` = `i`.bid) WHERE `bib`.idhistory='" & idtransaksi & "' ORDER BY `bib`.`urutan` ASC"
            'result(2) = sql : GoTo selesai
            Dim dtbonus As New DataTable
            dtbonus = AmbilData("aplikasi1-M_12_Bi_Bonus_History", "", "", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each drbonus As DataRow In dtbonus.Rows
                bonus = String.Concat(bonus,
                     FxDB(drbonus("idhistorybonus"), 0), sptField,
                     FxDB(drbonus("idhistory"), 0), sptField,
                     FxDB(drbonus("idbonus"), 0), sptField,
                     FxDB(drbonus("idbidetail"), 0), sptField,
                     FxDB(drbonus("idbi"), 0), sptField,
                     FxDB(drbonus("idbarang"), 0), sptField,
                     FxDB(drbonus("jml"), 0), sptField,
                     FxDB(drbonus("satuan"), ""), sptField,
                     FxDB(drbonus("customtext1"), ""), sptField,
                     FxDB(drbonus("customtext2"), ""), sptField,
                     FxDB(drbonus("customtext3"), ""), sptField,
                     FxDB(drbonus("customtext4"), ""), sptField,
                     FxDB(drbonus("customtext5"), ""), sptField,
                     FxDB(drbonus("customint1"), 0), sptField,
                     FxDB(drbonus("customint2"), 0), sptField,
                     FxDB(drbonus("customint3"), 0), sptField,
                     FxDB(drbonus("customdbl1"), 0), sptField,
                     FxDB(drbonus("customdbl2"), 0), sptField,
                     FxDB(drbonus("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drbonus("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drbonus("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drbonus("customdate3"), ""), formatTgl), sptField,
                     FxDB(drbonus("kodebarang"), 0), sptField,
                     FxDB(drbonus("namabarang"), 0), sptField,
                     FxDB(drbonus("urutan"), 0), sptRow)
            Next
            bonus = bonus.Substring(0, bonus.Length - sptRow.Length)

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, bonus)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("biidhistory, biid, bicabang, bilokasi, bisumber, bikategoripos, biautonotransaksi, binotransaksi, bitgl, bikodepa, bikontak, bikontakperson, biuraian, bicatatan, bistatus, bistatussebelumnya, bijmlrevisi, bicetakanke, biisclose, biinputuser, biinputtgl, bimodifikasiuser, bimodifikasitgl, biposting, bipostingtgl, bicustomtext1, bicustomtext2, bicustomtext3, bicustomtext4, bicustomtext5, bicustomint1, bicustomint2, bicustomint3, bicustomdbl1, bicustomdbl2, bicustomdbl3, bicustomdate1, bicustomdate2, bicustomdate3, bicabangnama, bilokasinama, bikontakkode, bikontaknama, bistatusnama, bistatussebelumnyanama, biinputusernama, bimodifikasiusernama, bikategoriposnama, bijeniskategori" & sptSubParam & "idhistorydetail, idhistory, idbidetail, idbi, bikategori, idbarang, operator, jml1, jml2, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, tgl1, tgl2, nopromo, kodebarang, namabarang, catatan, urutan" & sptSubParam & "idhistorybonus, idhistory, idbonus, idbidetail, idbi, idbarang, jml, satuan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, namabarang, urutan"))

        Return wsResult
    End Function


End Class