Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_spa_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_Spa_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO M5_Spa_history(SELECT 0, spa.* FROM M5_Spa spa WHERE spa.spaid = '" & idtransaksi & "')"
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
            sql = "SELECT spaidhistory FROM M5_Spa_history WHERE spaid = '" & idtransaksi & "' ORDER BY spamodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO M5_Spa_detail_history (SELECT 0, '" & result(4) & "', spa.* FROM M5_Spa_detail spa WHERE spa.idspa = '" & idtransaksi & "' )"
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
    Public Function M5_Spa_HistorySearch(ByVal param As String) As String
        'M5_Spa_HistorySearch --------------------------------------------------------
        'spaidhistory, spaid, spacabang, spalokasi, spasumber, spaautonotransaksi, spanotransaksi, spatgl, 
        'spakodepa, spakontak, spakontakperson, spauraian, spacatatan, spastatus, spastatussebelumnya, 
        'spajmlrevisi, spacetakanke, spaisclose, spainputuser, spainputtgl, spamodifikasiuser, spamodifikasitgl, 
        'spaposting, spapostingtgl, spacabangnama, spalokasinama, spakontakkode, spakontaknama, spastatusnama, 
        'spastatussebelumnyanama, spainputusernama, spamodifikasiusernama

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
        sql = "select `spa`.`spaidhistory` AS `spaidhistory`,`spa`.`spaid` AS `spaid`,`spa`.`spacabang` AS `spacabang`,`spa`.`spalokasi` AS `spalokasi`,`spa`.`spasumber` AS `spasumber`,`spa`.`spaautonotransaksi` AS `spaautonotransaksi`,`spa`.`spanotransaksi` AS `spanotransaksi`,`spa`.`spatgl` AS `spatgl`,`spa`.`spakodepa` AS `spakodepa`,`spa`.`spakontak` AS `spakontak`,`spa`.`spakontakperson` AS `spakontakperson`,`spa`.`spauraian` AS `spauraian`,`spa`.`spacatatan` AS `spacatatan`,`spa`.`spastatus` AS `spastatus`,`spa`.`spastatussebelumnya` AS `spastatussebelumnya`,`spa`.`spajmlrevisi` AS `spajmlrevisi`,`spa`.`spacetakanke` AS `spacetakanke`,`spa`.`spaisclose` AS `spaisclose`,`spa`.`spainputuser` AS `spainputuser`,`spa`.`spainputtgl` AS `spainputtgl`,`spa`.`spamodifikasiuser` AS `spamodifikasiuser`,`spa`.`spamodifikasitgl` AS `spamodifikasitgl`,`spa`.`spaposting` AS `spaposting`,`spa`.`spapostingtgl` AS `spapostingtgl`,`br`.`bnama` AS `spacabangnama`,`lc`.`lnama` AS `spalokasinama`,`c1`.`kkode` AS `spakontakkode`,`c1`.`knama` AS `spakontaknama`,`st1`.`nama` AS `spastatusnama`,`st2`.`nama` AS `spastatussebelumnyanama`,`u1`.`unama` AS `spainputusernama`,`u2`.`unama` AS `spamodifikasiusernama` from (((((((`m5_spa_history` `spa` join `m0_status` `st1` on((`spa`.`spastatus` = `st1`.`kode`))) join `m0_status` `st2` on((`spa`.`spastatussebelumnya` = `st2`.`kode`))) left join `m1_branch` `br` on((`spa`.`spacabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`spa`.`spalokasi` = `lc`.`lkode`))) left join `m1_contact` `c1` on((`spa`.`spakontak` = `c1`.`kid`))) left join `m0_user` `u1` on((`spa`.`spainputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`spa`.`spamodifikasiuser` = `u2`.`userid`)))"

        dt = AmbilData("aplikasi1-M5_Spa_V", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("spaid"), ""), sptField,
                     FxDB(dr("spaidhistory"), ""), sptField,
                     FxDB(dr("spacabang"), ""), sptField,
                     FxDB(dr("spalokasi"), ""), sptField,
                     FxDB(dr("spasumber"), ""), sptField,
                     FxDB(dr("spaautonotransaksi"), 0), sptField,
                     FxDB(dr("spanotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("spatgl"), ""), formatTgl), sptField,
                     FxDB(dr("spakodepa"), ""), sptField,
                     FxDB(dr("spakontak"), ""), sptField,
                     FxDB(dr("spakontakperson"), ""), sptField,
                     FxDB(dr("spauraian"), ""), sptField,
                     FxDB(dr("spacatatan"), ""), sptField,
                     FxDB(dr("spastatus"), 0), sptField,
                     FxDB(dr("spastatussebelumnya"), 0), sptField,
                     FxDB(dr("spajmlrevisi"), 0), sptField,
                     FxDB(dr("spacetakanke"), 0), sptField,
                     FxDB(dr("spaisclose"), 0), sptField,
                     FxDB(dr("spainputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("spainputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("spamodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("spamodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("spaposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("spapostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("spacabangnama"), ""), sptField,
                     FxDB(dr("spalokasinama"), ""), sptField,
                     FxDB(dr("spakontakkode"), ""), sptField,
                     FxDB(dr("spakontaknama"), ""), sptField,
                     FxDB(dr("spastatusnama"), ""), sptField,
                     FxDB(dr("spastatussebelumnyanama"), ""), sptField,
                     FxDB(dr("spainputusernama"), ""), sptField,
                     FxDB(dr("spamodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("spaidhistory, spaid, spacabang, spalokasi, spasumber, spaautonotransaksi, spanotransaksi, spatgl, spakodepa, spakontak, spakontakperson, spauraian, spacatatan, spastatus, spastatussebelumnya, spajmlrevisi, spacetakanke, spaisclose, spainputuser, spainputtgl, spamodifikasiuser, spamodifikasitgl, spaposting, spapostingtgl, spacabangnama, spalokasinama, spakontakkode, spakontaknama, spastatusnama, spastatussebelumnyanama, spainputusernama, spamodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SpaHistoryGetdataById(ByVal param As String) As String
        'M5_SpaHistoryGetdataById Utama --------------------------------------------------------
        'spaidhistory, spaid, spacabang, spalokasi, spasumber, spaautonotransaksi, spanotransaksi, spatgl, 
        'spakodepa, spakontak, spakontakperson, spauraian, spacatatan, spastatus, spastatussebelumnya, 
        'spajmlrevisi, spacetakanke, spaisclose, spainputuser, spainputtgl, spamodifikasiuser, spamodifikasitgl, 
        'spaposting, spapostingtgl, spacustomtext1, spacustomtext2, spacustomtext3, spacustomtext4, spacustomtext5, 
        'spacustomint1, spacustomint2, spacustomint3, spacustomdbl1, spacustomdbl2, spacustomdbl3, spacustomdate1, 
        'spacustomdate2, spacustomdate3, spacabangnama, spalokasinama, spakontakkode, spakontaknama, spastatusnama, 
        'spastatussebelumnyanama, spainputusernama, spamodifikasiusernama

        'M5_SpaHistoryGetdataById Detail --------------------------------------------------------
        'idhistorydetail, idhistory, idspadetail, idspa, kontak, poinlama, 
        'poinmasuk, poinkeluar, poinbaru, catatan, urutan, isclose, customtext1, 
        'customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3, kontakkode, kontaknama, kontakkategori, kontakkategorinama, kontakkategorisalesman, kontakkategorisalesmannama, 
        'kontakarea, kontakareanama

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

        Dim NmMemcached As String = "aplikasi1-M5_Spa~M5_Spa_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "spaid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "spaid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select `spa`.`spaidhistory` AS `spaidhistory`,`spa`.`spaid` AS `spaid`,`spa`.`spacabang` AS `spacabang`,`spa`.`spalokasi` AS `spalokasi`,`spa`.`spasumber` AS `spasumber`,`spa`.`spaautonotransaksi` AS `spaautonotransaksi`,`spa`.`spanotransaksi` AS `spanotransaksi`,`spa`.`spatgl` AS `spatgl`,`spa`.`spakodepa` AS `spakodepa`,`spa`.`spakontak` AS `spakontak`,`spa`.`spakontakperson` AS `spakontakperson`,`spa`.`spauraian` AS `spauraian`,`spa`.`spacatatan` AS `spacatatan`,`spa`.`spastatus` AS `spastatus`,`spa`.`spastatussebelumnya` AS `spastatussebelumnya`,`spa`.`spajmlrevisi` AS `spajmlrevisi`,`spa`.`spacetakanke` AS `spacetakanke`,`spa`.`spaisclose` AS `spaisclose`,`spa`.`spainputuser` AS `spainputuser`,`spa`.`spainputtgl` AS `spainputtgl`,`spa`.`spamodifikasiuser` AS `spamodifikasiuser`,`spa`.`spamodifikasitgl` AS `spamodifikasitgl`,`spa`.`spaposting` AS `spaposting`,`spa`.`spapostingtgl` AS `spapostingtgl`,`spa`.`spacustomtext1` AS `spacustomtext1`,`spa`.`spacustomtext2` AS `spacustomtext2`,`spa`.`spacustomtext3` AS `spacustomtext3`,`spa`.`spacustomtext4` AS `spacustomtext4`,`spa`.`spacustomtext5` AS `spacustomtext5`,`spa`.`spacustomint1` AS `spacustomint1`,`spa`.`spacustomint2` AS `spacustomint2`,`spa`.`spacustomint3` AS `spacustomint3`,`spa`.`spacustomdbl1` AS `spacustomdbl1`,`spa`.`spacustomdbl2` AS `spacustomdbl2`,`spa`.`spacustomdbl3` AS `spacustomdbl3`,`spa`.`spacustomdate1` AS `spacustomdate1`,`spa`.`spacustomdate2` AS `spacustomdate2`,`spa`.`spacustomdate3` AS `spacustomdate3`,`br`.`bnama` AS `spacabangnama`,`lc`.`lnama` AS `spalokasinama`,`c1`.`kkode` AS `spakontakkode`,`c1`.`knama` AS `spakontaknama`,`st1`.`nama` AS `spastatusnama`,`st2`.`nama` AS `spastatussebelumnyanama`,`u1`.`unama` AS `spainputusernama`,`u2`.`unama` AS `spamodifikasiusernama`,`spad`.`idhistorydetail` AS `idhistorydetail`,`spad`.`idhistory` AS `idhistory`,`spad`.`idspadetail` AS `idspadetail`,`spad`.`idspa` AS `idspa`,`spad`.`kontak` AS `kontak`,`spad`.`poinlama` AS `poinlama`,`spad`.`poinmasuk` AS `poinmasuk`,`spad`.`poinkeluar` AS `poinkeluar`,`spad`.`poinbaru` AS `poinbaru`,`spad`.`catatan` AS `catatan`,`spad`.`urutan` AS `urutan`,`spad`.`isclose` AS `isclose`,`spad`.`customtext1` AS `customtext1`,`spad`.`customtext2` AS `customtext2`,`spad`.`customtext3` AS `customtext3`,`spad`.`customdbl1` AS `customdbl1`,`spad`.`customdbl2` AS `customdbl2`,`spad`.`customdbl3` AS `customdbl3`,`spad`.`customdate1` AS `customdate1`,`spad`.`customdate2` AS `customdate2`,`spad`.`customdate3` AS `customdate3`,`c2`.`kkode` AS `kontakkode`,`c2`.`knama` AS `kontaknama`,`c2`.`kkategori` AS `kontakkategori`,`cc`.`ccnama` AS `kontakkategorinama`,`c2`.`kkategorisalesman` AS `kontakkategorisalesman`,`sc`.`scnama` AS `kontakkategorisalesmannama`,`c2`.`karea` AS `kontakarea`,`a`.`anama` AS `kontakareanama` from ((((((((((((`m5_spa_history` `spa` join `m0_status` `st1` on((`spa`.`spastatus` = `st1`.`kode`))) join `m0_status` `st2` on((`spa`.`spastatussebelumnya` = `st2`.`kode`))) join `m5_spa_detail_history` `spad` on((`spa`.`spaidhistory` = `spad`.`idhistory`))) left join `m1_branch` `br` on((`spa`.`spacabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`spa`.`spalokasi` = `lc`.`lkode`))) left join `m1_contact` `c1` on((`spa`.`spakontak` = `c1`.`kid`))) left join `m0_user` `u1` on((`spa`.`spainputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`spa`.`spamodifikasiuser` = `u2`.`userid`))) left join `m1_contact` `c2` on((`spad`.`kontak` = `c2`.`kid`))) left join `m1_contact_category` `cc` on((`c2`.`kkategori` = `cc`.`cckode`))) left join `m1_salesman_category` `sc` on((`c2`.`kkategorisalesman` = `sc`.`sckode`))) left join `m1_area` `a` on((`c2`.`karea` = `a`.`akode`)))"

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                     FxDB(drutama("spaidhistory"), ""), sptField,
                     FxDB(drutama("spaid"), ""), sptField,
                     FxDB(drutama("spacabang"), ""), sptField,
                     FxDB(drutama("spalokasi"), ""), sptField,
                     FxDB(drutama("spasumber"), ""), sptField,
                     FxDB(drutama("spaautonotransaksi"), 0), sptField,
                     FxDB(drutama("spanotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("spatgl"), ""), formatTgl), sptField,
                     FxDB(drutama("spakodepa"), ""), sptField,
                     FxDB(drutama("spakontak"), ""), sptField,
                     FxDB(drutama("spakontakperson"), ""), sptField,
                     FxDB(drutama("spauraian"), ""), sptField,
                     FxDB(drutama("spacatatan"), ""), sptField,
                     FxDB(drutama("spastatus"), 0), sptField,
                     FxDB(drutama("spastatussebelumnya"), 0), sptField,
                     FxDB(drutama("spajmlrevisi"), 0), sptField,
                     FxDB(drutama("spacetakanke"), 0), sptField,
                     FxDB(drutama("spaisclose"), 0), sptField,
                     FxDB(drutama("spainputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("spainputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("spamodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("spamodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("spaposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("spapostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("spacustomtext1"), ""), sptField,
                     FxDB(drutama("spacustomtext2"), ""), sptField,
                     FxDB(drutama("spacustomtext3"), ""), sptField,
                     FxDB(drutama("spacustomtext4"), ""), sptField,
                     FxDB(drutama("spacustomtext5"), ""), sptField,
                     FxDB(drutama("spacustomint1"), 0), sptField,
                     FxDB(drutama("spacustomint2"), 0), sptField,
                     FxDB(drutama("spacustomint3"), 0), sptField,
                     FxDB(drutama("spacustomdbl1"), 0), sptField,
                     FxDB(drutama("spacustomdbl2"), 0), sptField,
                     FxDB(drutama("spacustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("spacustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("spacustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("spacustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("spacabangnama"), ""), sptField,
                     FxDB(drutama("spalokasinama"), ""), sptField,
                     FxDB(drutama("spakontakkode"), ""), sptField,
                     FxDB(drutama("spakontaknama"), ""), sptField,
                     FxDB(drutama("spastatusnama"), ""), sptField,
                     FxDB(drutama("spastatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("spainputusernama"), ""), sptField,
                     FxDB(drutama("spamodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idspadetail"), ""), sptField,
                     FxDB(dr("idspa"), ""), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("spaid, spacabang, spalokasi, spasumber, spaautonotransaksi, spanotransaksi, spatgl, spakodepa, spakontak, spakontakperson, spauraian, spacatatan, spastatus, spastatussebelumnya, spajmlrevisi, spacetakanke, spaisclose, spainputuser, spainputtgl, spamodifikasiuser, spamodifikasitgl, spaposting, spapostingtgl, spacustomtext1, spacustomtext2, spacustomtext3, spacustomtext4, spacustomtext5, spacustomint1, spacustomint2, spacustomint3, spacustomdbl1, spacustomdbl2, spacustomdbl3, spacustomdate1, spacustomdate2, spacustomdate3, spacabangnama, spalokasinama, spakontakkode, spakontaknama, spastatusnama, spastatussebelumnyanama, spainputusernama, spamodifikasiusernama" & sptSubParam & "idspadetail, idspa, kontak, poinlama, poinmasuk, poinkeluar, poinbaru, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kontakkode, kontaknama, kontakkategori, kontakkategorinama, kontakkategorisalesman, kontakkategorisalesmannama, kontakarea, kontakareanama"))

        Return wsResult
    End Function

End Class
