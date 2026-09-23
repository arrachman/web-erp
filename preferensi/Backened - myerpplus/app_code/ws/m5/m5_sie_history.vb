Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_sie_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_Sie_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO M5_Sie_history(SELECT 0, sie.* FROM M5_Sie sie WHERE sie.sieid = '" & idtransaksi & "')"
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
            sql = "SELECT sieidhistory FROM M5_sie_history WHERE sieid = '" & idtransaksi & "' ORDER BY siemodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO M5_sie_detail_history (SELECT 0, '" & result(4) & "', sie.* FROM M5_sie_detail sie WHERE sie.idsie = '" & idtransaksi & "' )"
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
    Public Function M5_Sie_HistorySearch(ByVal param As String) As String
        'M5_Sie_HistorySearch --------------------------------------------------------
        'sieidhistory, sieid, siecabang, sielokasi, siesumber, sienotransaksi, sietgl, sieuraian, 
        'siecatatan, siestatus, siestatussebelumnya, sieinputuser, sieinputtgl, siemodifikasiuser, siemodifikasitgl, 
        'siecabangnama, sielokasinama, siestatusnama, siestatussebelumnyanama, sieinputusernama, siemodifikasiusernama

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
            'Filter = Filter.Replace("grncustomerkode", "c1.kkode")
            'Filter = Filter.Replace("grncustomernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select `sie`.`sieidhistory` AS `sieidhistory`,`sie`.`sieid` AS `sieid`,`sie`.`siecabang` AS `siecabang`,`sie`.`sielokasi` AS `sielokasi`,`sie`.`siesumber` AS `siesumber`,`sie`.`sienotransaksi` AS `sienotransaksi`,`sie`.`sietgl` AS `sietgl`,`sie`.`sieuraian` AS `sieuraian`,`sie`.`siecatatan` AS `siecatatan`,`sie`.`siestatus` AS `siestatus`,`sie`.`siestatussebelumnya` AS `siestatussebelumnya`,`sie`.`sieinputuser` AS `sieinputuser`,`sie`.`sieinputtgl` AS `sieinputtgl`,`sie`.`siemodifikasiuser` AS `siemodifikasiuser`,`sie`.`siemodifikasitgl` AS `siemodifikasitgl`,`br`.`bnama` AS `siecabangnama`,`lc`.`lnama` AS `sielokasinama`,`st1`.`nama` AS `siestatusnama`,`st2`.`nama` AS `siestatussebelumnyanama`,`u1`.`unama` AS `sieinputusernama`,`u2`.`unama` AS `siemodifikasiusernama` from ((((((`M5_sie_history` `sie` join `m1_branch` `br` on((`sie`.`siecabang` = `br`.`bkode`))) join `m1_location` `lc` on((`sie`.`sielokasi` = `lc`.`lkode`))) join `m0_status` `st1` on((`sie`.`siestatus` = `st1`.`kode`))) join `m0_status` `st2` on((`sie`.`siestatussebelumnya` = `st2`.`kode`))) join `m0_user` `u1` on((`sie`.`sieinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`sie`.`siemodifikasiuser` = `u2`.`userid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M5_Grn", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("sieid"), 0), sptField,
                     FxDB(dr("sieidhistory"), ""), sptField,
                     FxDB(dr("siecabang"), ""), sptField,
                     FxDB(dr("sielokasi"), ""), sptField,
                     FxDB(dr("siesumber"), ""), sptField,
                     FxDB(dr("sienotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sietgl"), ""), formatTgl), sptField,
                     FxDB(dr("sieuraian"), ""), sptField,
                     FxDB(dr("siecatatan"), ""), sptField,
                     FxDB(dr("siestatus"), 0), sptField,
                     FxDB(dr("siestatussebelumnya"), 0), sptField,
                     FxDB(dr("sieinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sieinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("siemodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("siemodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("siecabangnama"), ""), sptField,
                     FxDB(dr("sielokasinama"), ""), sptField,
                     FxDB(dr("siestatusnama"), ""), sptField,
                     FxDB(dr("siestatussebelumnyanama"), ""), sptField,
                     FxDB(dr("sieinputusernama"), ""), sptField,
                     FxDB(dr("siemodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sieidhistory, sieid, siecabang, sielokasi, siesumber, sienotransaksi, sietgl, sieuraian, siecatatan, siestatus, siestatussebelumnya, sieinputuser, sieinputtgl, siemodifikasiuser, siemodifikasitgl, siecabangnama, sielokasinama, siestatusnama, siestatussebelumnyanama, sieinputusernama, siemodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SieHistoryGetdataById(ByVal param As String) As String

        'M5_SieHistoryGetdataById Utama --------------------------------------------------------
        'sieidhistory, sieid, siecabang, sielokasi, siesumber, sieautonotransaksi, sienotransaksi, sietgl, 
        'siekodepa, siekontak, siekontakperson, sie1alamat1, sie1alamat2, sie1alamat3, sie2alamat1, 
        'sie2alamat2, sie2alamat3, sieuraian, siecatatan, sienoref, sietglnoref, siestatus, 
        'siestatussebelumnya, siejmlrevisi, siecetakanke, sieinputuser, sieinputtgl, siemodifikasiuser, siemodifikasitgl, 
        'sieposting, siepostingtgl, sieisclose, siecustomtext1, siecustomtext2, siecustomtext3, siecustomtext4, 
        'siecustomtext5, siecustomint1, siecustomint2, siecustomint3, siecustomdbl1, siecustomdbl2, siecustomdbl3, 
        'siecustomdate1, siecustomdate2, siecustomdate3

        'M5_SieHistoryGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idsiedetail, idsie, sumber, idtransaksi, 
        'catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3, cabang, lokasi, 
        'gudang, notransaksi, tgl, customer, customerkode, customernama, customerkontak, 
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

        Dim NmMemcached As String = "aplikasi1-M5_Pr_history~M5_Pr_Detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "sieidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "sieidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select `sie`.`sieidhistory` AS `sieidhistory`,`sie`.`sieid` AS `sieid`,`sie`.`siecabang` AS `siecabang`,`sie`.`sielokasi` AS `sielokasi`,`sie`.`siesumber` AS `siesumber`,`sie`.`sieautonotransaksi` AS `sieautonotransaksi`,`sie`.`sienotransaksi` AS `sienotransaksi`,`sie`.`sietgl` AS `sietgl`,`sie`.`siekodepa` AS `siekodepa`,`sie`.`siekontak` AS `siekontak`,`sie`.`siekontakperson` AS `siekontakperson`,`sie`.`sie1alamat1` AS `sie1alamat1`,`sie`.`sie1alamat2` AS `sie1alamat2`,`sie`.`sie1alamat3` AS `sie1alamat3`,`sie`.`sie2alamat1` AS `sie2alamat1`,`sie`.`sie2alamat2` AS `sie2alamat2`,`sie`.`sie2alamat3` AS `sie2alamat3`,`sie`.`sieuraian` AS `sieuraian`,`sie`.`siecatatan` AS `siecatatan`,`sie`.`sienoref` AS `sienoref`,`sie`.`sietglnoref` AS `sietglnoref`,`sie`.`siestatus` AS `siestatus`,`sie`.`siestatussebelumnya` AS `siestatussebelumnya`,`sie`.`siejmlrevisi` AS `siejmlrevisi`,`sie`.`siecetakanke` AS `siecetakanke`,`sie`.`sieinputuser` AS `sieinputuser`,`sie`.`sieinputtgl` AS `sieinputtgl`,`sie`.`siemodifikasiuser` AS `siemodifikasiuser`,`sie`.`siemodifikasitgl` AS `siemodifikasitgl`,`sie`.`sieposting` AS `sieposting`,`sie`.`siepostingtgl` AS `siepostingtgl`,`sie`.`sieisclose` AS `sieisclose`,`sie`.`siecustomtext1` AS `siecustomtext1`,`sie`.`siecustomtext2` AS `siecustomtext2`,`sie`.`siecustomtext3` AS `siecustomtext3`,`sie`.`siecustomtext4` AS `siecustomtext4`,`sie`.`siecustomtext5` AS `siecustomtext5`,`sie`.`siecustomint1` AS `siecustomint1`,`sie`.`siecustomint2` AS `siecustomint2`,`sie`.`siecustomint3` AS `siecustomint3`,`sie`.`siecustomdbl1` AS `siecustomdbl1`,`sie`.`siecustomdbl2` AS `siecustomdbl2`,`sie`.`siecustomdbl3` AS `siecustomdbl3`,`sie`.`siecustomdate1` AS `siecustomdate1`,`sie`.`siecustomdate2` AS `siecustomdate2`,`sie`.`siecustomdate3` AS `siecustomdate3`,`sied`.`idhistorydetail` AS `idhistorydetail`,`sied`.`idhistory` AS `idhistory`,`sied`.`idsiedetail` AS `idsiedetail`,`sied`.`idsie` AS `idsie`,`sied`.`sumber` AS `sumber`,`sied`.`idtransaksi` AS `idtransaksi`,`sied`.`catatan` AS `catatan`,`sied`.`urutan` AS `urutan`,`sied`.`isclose` AS `isclose`,`sied`.`customtext1` AS `customtext1`,`sied`.`customtext2` AS `customtext2`,`sied`.`customtext3` AS `customtext3`,`sied`.`customdbl1` AS `customdbl1`,`sied`.`customdbl2` AS `customdbl2`,`sied`.`customdbl3` AS `customdbl3`,`sied`.`customdate1` AS `customdate1`,`sied`.`customdate2` AS `customdate2`,`sied`.`customdate3` AS `customdate3`,ifnull(`si`.`sicabang`,`sr`.`srcabang`) AS `cabang`,ifnull(`si`.`silokasi`,`sr`.`srlokasi`) AS `lokasi`,ifnull(`si`.`sigudang`,`sr`.`srgudang`) AS `gudang`,ifnull(`si`.`sinotransaksi`,`sr`.`srnotransaksi`) AS `notransaksi`,ifnull(`si`.`sitgl`,`sr`.`srtgl`) AS `tgl`,ifnull(`si`.`sicustomer`,`sr`.`srcustomer`) AS `customer`,ifnull(`c`.`kkode`,'') AS `customerkode`,ifnull(`c`.`knama`,'') AS `customernama`,ifnull(`si`.`sicustomerkontak`,`sr`.`srcustomerkontak`) AS `customerkontak`,ifnull(`si`.`sitermin`,`sr`.`srtermin`) AS `termin`,ifnull(`si`.`siuraian`,`sr`.`sruraian`) AS `uraian`,ifnull(`si`.`simatauang`,`sr`.`srmatauang`) AS `matauang`,ifnull(`si`.`sikurs`,`sr`.`srkurs`) AS `kurs`,ifnull(`si`.`sitotaltransaksi`,`sr`.`srtotaltransaksi`) AS `totaltransaksi`,ifnull(`si`.`sijmlbayar`,`sr`.`srjmlbayar`) AS `jmlbayar` from ((((`M5_sie_history` `sie` join `M5_sie_detail_history` `sied` on((`sie`.`sieid` = `sied`.`idsie`))) left join `M5_Si` `si` on(((`sied`.`sumber` = `si`.`sisumber`) and (`sied`.`idtransaksi` = `si`.`siid`)))) left join `M5_Sr` `sr` on(((`sied`.`sumber` = `sr`.`srsumber`) and (`sied`.`idtransaksi` = `sr`.`srid`)))) left join `m1_contact` `c` on((ifnull(`si`.`sicustomer`,`sr`.`srcustomer`) = `c`.`kid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                     FxDB(drutama("sieidhistory"), 0), sptField,
                     FxDB(drutama("sieid"), ""), sptField,
                     FxDB(drutama("siecabang"), ""), sptField,
                     FxDB(drutama("sielokasi"), ""), sptField,
                     FxDB(drutama("siesumber"), ""), sptField,
                     FxDB(drutama("sieautonotransaksi"), 0), sptField,
                     FxDB(drutama("sienotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sietgl"), ""), formatTgl), sptField,
                     FxDB(drutama("siekodepa"), ""), sptField,
                     FxDB(drutama("siekontak"), ""), sptField,
                     FxDB(drutama("siekontakperson"), ""), sptField,
                     FxDB(drutama("sie1alamat1"), ""), sptField,
                     FxDB(drutama("sie1alamat2"), ""), sptField,
                     FxDB(drutama("sie1alamat3"), ""), sptField,
                     FxDB(drutama("sie2alamat1"), ""), sptField,
                     FxDB(drutama("sie2alamat2"), ""), sptField,
                     FxDB(drutama("sie2alamat3"), ""), sptField,
                     FxDB(drutama("sieuraian"), ""), sptField,
                     FxDB(drutama("siecatatan"), ""), sptField,
                     FxDB(drutama("sienoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sietglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("siestatus"), 0), sptField,
                     FxDB(drutama("siestatussebelumnya"), 0), sptField,
                     FxDB(drutama("siejmlrevisi"), 0), sptField,
                     FxDB(drutama("siecetakanke"), 0), sptField,
                     FxDB(drutama("sieinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sieinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("siemodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("siemodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sieposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("siepostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sieisclose"), 0), sptField,
                     FxDB(drutama("siecustomtext1"), ""), sptField,
                     FxDB(drutama("siecustomtext2"), ""), sptField,
                     FxDB(drutama("siecustomtext3"), ""), sptField,
                     FxDB(drutama("siecustomtext4"), ""), sptField,
                     FxDB(drutama("siecustomtext5"), ""), sptField,
                     FxDB(drutama("siecustomint1"), 0), sptField,
                     FxDB(drutama("siecustomint2"), 0), sptField,
                     FxDB(drutama("siecustomint3"), 0), sptField,
                     FxDB(drutama("siecustomdbl1"), 0), sptField,
                     FxDB(drutama("siecustomdbl2"), 0), sptField,
                     FxDB(drutama("siecustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("siecustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("siecustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("siecustomdate3"), ""), formatTgl))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idhistorydetail"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idsiedetail"), ""), sptField,
                     FxDB(dr("idsie"), ""), sptField,
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
                     FxDB(dr("customer"), ""), sptField,
                     FxDB(dr("customerkode"), ""), sptField,
                     FxDB(dr("customernama"), ""), sptField,
                     FxDB(dr("customerkontak"), ""), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sieidhistory, sieid, siecabang, sielokasi, siesumber, sieautonotransaksi, sienotransaksi, sietgl, siekodepa, siekontak, siekontakperson, sie1alamat1, sie1alamat2, sie1alamat3, sie2alamat1, sie2alamat2, sie2alamat3, sieuraian, siecatatan, sienoref, sietglnoref, siestatus, siestatussebelumnya, siejmlrevisi, siecetakanke, sieinputuser, sieinputtgl, siemodifikasiuser, siemodifikasitgl, sieposting, siepostingtgl, sieisclose, siecustomtext1, siecustomtext2, siecustomtext3, siecustomtext4, siecustomtext5, siecustomint1, siecustomint2, siecustomint3, siecustomdbl1, siecustomdbl2, siecustomdbl3, siecustomdate1, siecustomdate2, siecustomdate3" & sptSubParam & "idhistorydetail, idhistory, idsiedetail, idsie, sumber, idtransaksi, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, cabang, lokasi, gudang, notransaksi, tgl, customer, customerkode, customernama, customerkontak, termin, uraian, matauang, kurs, totaltransaksi, jmlbayar"))

        Return wsResult
    End Function

End Class
