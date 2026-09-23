Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_di_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_Di_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m_12_di_history(SELECT 0, di.* FROM m_12_di di WHERE di.diid = '" & idtransaksi & "')"
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
            sql = "SELECT diidhistory FROM m_12_di_history WHERE diid = '" & idtransaksi & "' ORDER BY dimodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m_12_di_detail_history (SELECT 0, '" & result(4) & "', di.* FROM m_12_di_detail di WHERE di.iddi = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY DETAIL ----------------------------------

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
    Public Function M12_Di_HistorySearch(ByVal param As String) As String
        'M12_Di_HistorySearch --------------------------------------------------------
        'diidhistory, diid, dicabang, dilokasi, disumber, dikategoripos, diautonotransaksi, dinotransaksi, 
        'ditgl, dikodepa, dikontak, dikontakperson, diuraian, dicatatan, distatus, 
        'distatussebelumnya, dijmlrevisi, dicetakanke, diisclose, diinputuser, diinputtgl, dimodifikasiuser, 
        'dimodifikasitgl, diposting, dipostingtgl, dicustomtext1, dicustomtext2, dicustomtext3, dicustomtext4, 
        'dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, dicustomdbl3, 
        'dicustomdate1, dicustomdate2, dicustomdate3, dicabangnama, dilokasinama, dikontakkode, 
        'dikontaknama, distatusnama, distatussebelumnyanama, diinputusernama, dimodifikasiusernama

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
        sql = "select `di`.`diidhistory` AS `diidhistory`, `di`.`diid` AS `diid`,`di`.`dicabang` AS `dicabang`,`di`.`dilokasi` AS `dilokasi`,`di`.`disumber` AS `disumber`,`di`.`diautonotransaksi` AS `diautonotransaksi`,`di`.`dinotransaksi` AS `dinotransaksi`,`di`.`ditgl` AS `ditgl`,`di`.`dikodepa` AS `dikodepa`,`di`.`dikontak` AS `dikontak`,`di`.`dikontakperson` AS `dikontakperson`,`di`.`dikategoripos` AS `dikategoripos`,`di`.`diuraian` AS `diuraian`,`di`.`dicatatan` AS `dicatatan`,`di`.`distatus` AS `distatus`,`di`.`distatussebelumnya` AS `distatussebelumnya`,`di`.`dijmlrevisi` AS `dijmlrevisi`,`di`.`dicetakanke` AS `dicetakanke`,`di`.`diisclose` AS `diisclose`,`di`.`diinputuser` AS `diinputuser`,`di`.`diinputtgl` AS `diinputtgl`,`di`.`dimodifikasiuser` AS `dimodifikasiuser`,`di`.`dimodifikasitgl` AS `dimodifikasitgl`,`di`.`diposting` AS `diposting`,`di`.`dipostingtgl` AS `dipostingtgl`,`di`.`dicustomtext1` AS `dicustomtext1`,`di`.`dicustomtext2` AS `dicustomtext2`,`di`.`dicustomtext3` AS `dicustomtext3`,`di`.`dicustomtext4` AS `dicustomtext4`,`di`.`dicustomtext5` AS `dicustomtext5`,`di`.`dicustomint1` AS `dicustomint1`,`di`.`dicustomint2` AS `dicustomint2`,`di`.`dicustomint3` AS `dicustomint3`,`di`.`dicustomdbl1` AS `dicustomdbl1`,`di`.`dicustomdbl2` AS `dicustomdbl2`,`di`.`dicustomdbl3` AS `dicustomdbl3`,`di`.`dicustomdate1` AS `dicustomdate1`,`di`.`dicustomdate2` AS `dicustomdate2`,`di`.`dicustomdate3` AS `dicustomdate3`,`br`.`bnama` AS `dicabangnama`,`lc`.`lnama` AS `dilokasinama`,`c`.`kkode` AS `dikontakkode`,`c`.`knama` AS `dikontaknama`,`st1`.`nama` AS `distatusnama`,`st2`.`nama` AS `distatussebelumnyanama`,`u1`.`unama` AS `diinputusernama`,`u2`.`unama` AS `dimodifikasiusernama` from (((((((`m_12_di_history` `di` left join `m1_branch` `br` on((`di`.`dicabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`di`.`dilokasi` = `lc`.`lkode`))) left join `m1_contact` `c` on((`di`.`dikontak` = `c`.`kid`))) left join `m0_status` `st1` on((`di`.`distatus` = `st1`.`kode`))) left join `m0_status` `st2` on((`di`.`distatussebelumnya` = `st2`.`kode`))) left join `m0_user` `u1` on((`di`.`diinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`di`.`dimodifikasiuser` = `u2`.`userid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Cr~M2_Cr_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("diidhistory"), 0), sptField,
                             FxDB(dr("diid"), 0), sptField,
                             FxDB(dr("dicabang"), ""), sptField,
                             FxDB(dr("dilokasi"), ""), sptField,
                             FxDB(dr("disumber"), ""), sptField,
                             FxDB(dr("dikategoripos"), ""), sptField,
                             FxDB(dr("diautonotransaksi"), 0), sptField,
                             FxDB(dr("dinotransaksi"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("ditgl"), ""), formatTgl), sptField,
                             FxDB(dr("dikodepa"), ""), sptField,
                             FxDB(dr("dikontak"), ""), sptField,
                             FxDB(dr("dikontakperson"), ""), sptField,
                             FxDB(dr("diuraian"), ""), sptField,
                             FxDB(dr("dicatatan"), ""), sptField,
                             FxDB(dr("distatus"), 0), sptField,
                             FxDB(dr("distatussebelumnya"), 0), sptField,
                             FxDB(dr("dijmlrevisi"), 0), sptField,
                             FxDB(dr("dicetakanke"), 0), sptField,
                             FxDB(dr("diisclose"), 0), sptField,
                             FxDB(dr("diinputuser"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("diinputtgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("dimodifikasiuser"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("dimodifikasitgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("diposting"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("dipostingtgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("dicustomtext1"), ""), sptField,
                             FxDB(dr("dicustomtext2"), ""), sptField,
                             FxDB(dr("dicustomtext3"), ""), sptField,
                             FxDB(dr("dicustomtext4"), ""), sptField,
                             FxDB(dr("dicustomtext5"), ""), sptField,
                             FxDB(dr("dicustomint1"), 0), sptField,
                             FxDB(dr("dicustomint2"), 0), sptField,
                             FxDB(dr("dicustomint3"), 0), sptField,
                             FxDB(dr("dicustomdbl1"), 0), sptField,
                             FxDB(dr("dicustomdbl2"), 0), sptField,
                             FxDB(dr("dicustomdbl3"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("dicustomdate1"), ""), formatTgl), sptField,
                             AsFormatTanggal(FxDB(dr("dicustomdate2"), ""), formatTgl), sptField,
                             AsFormatTanggal(FxDB(dr("dicustomdate3"), ""), formatTgl), sptField,
                             FxDB(dr("dicabangnama"), ""), sptField,
                             FxDB(dr("dilokasinama"), ""), sptField,
                             FxDB(dr("dikontakkode"), ""), sptField,
                             FxDB(dr("dikontaknama"), ""), sptField,
                             FxDB(dr("distatusnama"), ""), sptField,
                             FxDB(dr("distatussebelumnyanama"), ""), sptField,
                             FxDB(dr("diinputusernama"), ""), sptField,
                             FxDB(dr("dimodifikasiusernama"), ""), sptRow)

            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = pg1.isPaging
            resultPaging(1) = pg1.isNext
            resultPaging(2) = pg1.isPrev
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("diidhistory, diid, dicabang, dilokasi, disumber, dikategoripos, diautonotransaksi, dinotransaksi, ditgl, dikodepa, dikontak, dikontakperson, diuraian, dicatatan, distatus, distatussebelumnya, dijmlrevisi, dicetakanke, diisclose, diinputuser, diinputtgl, dimodifikasiuser, dimodifikasitgl, diposting, dipostingtgl, dicustomtext1, dicustomtext2, dicustomtext3, dicustomtext4, dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, dicustomdbl3, dicustomdate1, dicustomdate2, dicustomdate3, dicabangnama, dilokasinama, dikontakkode, dikontaknama, distatusnama, distatussebelumnyanama, diinputusernama, dimodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_DiHistoryGetdataById(ByVal param As String) As String

        'M12_BiHistoryGetdataById Utama --------------------------------------------------------
        'diidhistory, diid, dicabang, dilokasi, disumber, dikategoripos, diautonotransaksi, dinotransaksi, 
        'ditgl, dikodepa, dikontak, dikontakperson, diuraian, dicatatan, distatus, 
        'distatussebelumnya, dijmlrevisi, dicetakanke, diisclose, diinputuser, diinputtgl, dimodifikasiuser, 
        'dimodifikasitgl, diposting, dipostingtgl, dicustomtext1, dicustomtext2, dicustomtext3, dicustomtext4, 
        'dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, dicustomdbl3, 
        'dicustomdate1, dicustomdate2, dicustomdate3, dicabangnama, dilokasinama, dikontakkode, dikontaknama
        'distatusnama, distatussebelumnyanama, diinputusernama, dimodifikasiusernama, dikategoriposnama, dijeniskategori

        'M12_BiHistoryGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, iddidetail, iddi, dikategori, idbarang, operator, jml1, jml2, kriteria, 
        'nilai, tgl1, tgl2, jam1, jam2, catatan, urutan, 
        'customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, 
        'customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'nopromo, kodebarang, namabarang



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

        Dim utama As String = "", detail As String = "", discount As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M2_Cr~M2_Cr_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "diidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "diidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        sql = "select `di`.`diidhistory` AS `diidhistory`,`di`.`diid` AS `diid`,`di`.`dicabang` AS `dicabang`,`di`.`dilokasi` AS `dilokasi`,`di`.`disumber` AS `disumber`,`di`.`dikategoripos` AS `dikategoripos`,`di`.`diautonotransaksi` AS `diautonotransaksi`,`di`.`dinotransaksi` AS `dinotransaksi`,`di`.`ditgl` AS `ditgl`,`di`.`dikodepa` AS `dikodepa`,`di`.`dikontak` AS `dikontak`,`di`.`dikontakperson` AS `dikontakperson`,`di`.`diuraian` AS `diuraian`,`di`.`dicatatan` AS `dicatatan`,`di`.`distatus` AS `distatus`,`di`.`distatussebelumnya` AS `distatussebelumnya`,`di`.`dijmlrevisi` AS `dijmlrevisi`,`di`.`dicetakanke` AS `dicetakanke`,`di`.`diisclose` AS `diisclose`,`di`.`diinputuser` AS `diinputuser`,`di`.`diinputtgl` AS `diinputtgl`,`di`.`dimodifikasiuser` AS `dimodifikasiuser`,`di`.`dimodifikasitgl` AS `dimodifikasitgl`,`di`.`diposting` AS `diposting`,`di`.`dipostingtgl` AS `dipostingtgl`,`di`.`dicustomtext1` AS `dicustomtext1`,`di`.`dicustomtext2` AS `dicustomtext2`,`di`.`dicustomtext3` AS `dicustomtext3`,`di`.`dicustomtext4` AS `dicustomtext4`,`di`.`dicustomtext5` AS `dicustomtext5`,`di`.`dicustomint1` AS `dicustomint1`,`di`.`dicustomint2` AS `dicustomint2`,`di`.`dicustomint3` AS `dicustomint3`,`di`.`dicustomdbl1` AS `dicustomdbl1`,`di`.`dicustomdbl2` AS `dicustomdbl2`,`di`.`dicustomdbl3` AS `dicustomdbl3`,`di`.`dicustomdate1` AS `dicustomdate1`,`di`.`dicustomdate2` AS `dicustomdate2`,`di`.`dicustomdate3` AS `dicustomdate3`,`br`.`bnama` AS `dicabangnama`,`lc`.`lnama` AS `dilokasinama`,`c`.`kkode` AS `dikontakkode`,`c`.`knama` AS `dikontaknama`,`st1`.`nama` AS `distatusnama`,`st2`.`nama` AS `distatussebelumnyanama`,`u1`.`unama` AS `diinputusernama`,`u2`.`unama` AS `dimodifikasiusernama`,`pc`.`pcnama` AS `dikategoriposnama`,`di`.`dijeniskategori` AS `dijeniskategori`,`did`.`idhistorydetail` AS `idhistorydetail`,`did`.`idhistory` AS `idhistory`,`did`.`iddidetail` AS `iddidetail`,`did`.`iddi` AS `iddi`,`did`.`dikategori` AS `dikategori`,`did`.`idbarang` AS `idbarang`,`did`.`operator` AS `operator`,`did`.`jml1` AS `jml1`,`did`.`jml2` AS `jml2`,`did`.`kriteria` AS `kriteria`,`did`.`nilai` AS `nilai`,`did`.`customtext1` AS `customtext1`,`did`.`customtext2` AS `customtext2`,`did`.`customtext3` AS `customtext3`,`did`.`customtext4` AS `customtext4`,`did`.`customtext5` AS `customtext5`,`did`.`customint1` AS `customint1`,`did`.`customint2` AS `customint2`,`did`.`customint3` AS `customint3`,`did`.`customdbl1` AS `customdbl1`,`did`.`customdbl2` AS `customdbl2`,`did`.`customdbl3` AS `customdbl3`,`did`.`customdate1` AS `customdate1`,`did`.`customdate2` AS `customdate2`,`did`.`customdate3` AS `customdate3`,`did`.`tgl1` AS `tgl1`,`did`.`tgl2` AS `tgl2`,`did`.`nopromo` AS `nopromo`,`did`.`jam1` AS `jam1`,`did`.`jam2` AS `jam2`,`i`.`bkode` AS `kodebarang`,`i`.`bnama` AS `namabarang`, `did`.`catatan` AS `catatan`, `did`.`urutan` AS `urutan`  from ((((((((((`m_12_di_history` `di` join `m_12_di_detail_history` `did` on((`di`.`diidhistory` = `did`.`idhistory`))) left join `m1_branch` `br` on((`di`.`dicabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`di`.`dilokasi` = `lc`.`lkode`))) left join `m1_contact` `c` on((`di`.`dikontak` = `c`.`kid`))) left join `m0_status` `st1` on((`di`.`distatus` = `st1`.`kode`))) left join `m0_status` `st2` on((`di`.`distatussebelumnya` = `st2`.`kode`))) left join `m0_user` `u1` on((`di`.`diinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`di`.`dimodifikasiuser` = `u2`.`userid`))) left join `m1_item` `i` on((`did`.`idbarang` = `i`.`bid`)))  left join `m_12_pos_category` `pc` on((`di`.`dikategoripos` = `pc`.`pckode`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                     FxDB(drutama("diidhistory"), 0), sptField,
                     FxDB(drutama("diid"), 0), sptField,
                     FxDB(drutama("dicabang"), ""), sptField,
                     FxDB(drutama("dilokasi"), ""), sptField,
                     FxDB(drutama("disumber"), ""), sptField,
                     FxDB(drutama("dikategoripos"), ""), sptField,
                     FxDB(drutama("diautonotransaksi"), 0), sptField,
                     FxDB(drutama("dinotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ditgl"), ""), formatTgl), sptField,
                     FxDB(drutama("dikodepa"), ""), sptField,
                     FxDB(drutama("dikontak"), ""), sptField,
                     FxDB(drutama("dikontakperson"), ""), sptField,
                     FxDB(drutama("diuraian"), ""), sptField,
                     FxDB(drutama("dicatatan"), ""), sptField,
                     FxDB(drutama("distatus"), 0), sptField,
                     FxDB(drutama("distatussebelumnya"), 0), sptField,
                     FxDB(drutama("dijmlrevisi"), 0), sptField,
                     FxDB(drutama("dicetakanke"), 0), sptField,
                     FxDB(drutama("diisclose"), 0), sptField,
                     FxDB(drutama("diinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("diinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("dimodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("dimodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("diposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("dipostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("dicustomtext1"), ""), sptField,
                     FxDB(drutama("dicustomtext2"), ""), sptField,
                     FxDB(drutama("dicustomtext3"), ""), sptField,
                     FxDB(drutama("dicustomtext4"), ""), sptField,
                     FxDB(drutama("dicustomtext5"), ""), sptField,
                     FxDB(drutama("dicustomint1"), 0), sptField,
                     FxDB(drutama("dicustomint2"), 0), sptField,
                     FxDB(drutama("dicustomint3"), 0), sptField,
                     FxDB(drutama("dicustomdbl1"), 0), sptField,
                     FxDB(drutama("dicustomdbl2"), 0), sptField,
                     FxDB(drutama("dicustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("dicustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("dicustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("dicustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("dicabangnama"), ""), sptField,
                     FxDB(drutama("dilokasinama"), ""), sptField,
                     FxDB(drutama("dikontakkode"), ""), sptField,
                     FxDB(drutama("dikontaknama"), ""), sptField,
                     FxDB(drutama("distatusnama"), ""), sptField,
                     FxDB(drutama("distatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("diinputusernama"), ""), sptField,
                     FxDB(drutama("dimodifikasiusernama"), ""), sptField,
                     FxDB(drutama("dikategoriposnama"), ""), sptField,
                     FxDB(drutama("dijeniskategori"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), ""), sptField,
                     FxDB(dr("idhistory"), ""), sptField,
                     FxDB(dr("iddidetail"), ""), sptField,
                     FxDB(dr("iddi"), ""), sptField,
                     FxDB(dr("dikategori"), ""), sptField,
                     FxDB(dr("idbarang"), ""), sptField,
                     FxDB(dr("operator"), ""), sptField,
                     FxDB(dr("jml1"), 0), sptField,
                     FxDB(dr("jml2"), 0), sptField,
                     FxDB(dr("kriteria"), 0), sptField,
                     FxDB(dr("nilai"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("tgl2"), ""), formatTgl), sptField,
                     FxDB(dr("jam1"), ""), sptField,
                     FxDB(dr("jam2"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), ""), sptField,
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
                     FxDB(dr("nopromo"), ""), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("namabarang"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("diidhistory, diid, dicabang, dilokasi, disumber, dikategoripos, diautonotransaksi, dinotransaksi, ditgl, dikodepa, dikontak, dikontakperson, diuraian, dicatatan, distatus, distatussebelumnya, dijmlrevisi, dicetakanke, diisclose, diinputuser, diinputtgl, dimodifikasiuser, dimodifikasitgl, diposting, dipostingtgl, dicustomtext1, dicustomtext2, dicustomtext3, dicustomtext4, dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, dicustomdbl3, dicustomdate1, dicustomdate2, dicustomdate3, dicabangnama, dilokasinama, dikontakkode, dikontaknama, distatusnama, distatussebelumnyanama, diinputusernama, dimodifikasiusernama, dikategoriposnama, dijeniskategori" & sptSubParam & "idhistorydetail, idhistory, iddidetail, iddi, dikategori, idbarang, operator, jml1, jml2, kriteria, nilai, tgl1, tgl2, jam1, jam2, catatan, urutan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, nopromo, kodebarang, namabarang"))

        Return wsResult
    End Function


End Class