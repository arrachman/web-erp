Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m2_bd_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    Public Function M2_Bd_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m2_bd_history(SELECT 0, bd.* FROM m2_bd bd WHERE bd.bdid = '" & idtransaksi & "')"
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
            sql = "SELECT bdidhistory FROM m2_bd_history WHERE bdid = '" & idtransaksi & "' ORDER BY bdmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m2_bd_detail_history (SELECT 0, '" & result(4) & "', bd.* FROM m2_bd_detail bd WHERE bd.idbd = '" & idtransaksi & "' )"
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
    Public Function M2_Bd_HistorySearch(ByVal param As String) As String
        'M2_Bd_HistorySearch --------------------------------------------------------
        'bdidhistory, bdid, bdcabang, bdlokasi, bdsumber, bdautonotransaksi, bdnotransaksi, bdtgl, 
        'bdtglanggaran, bdkodepa, bdkontak, bdkontakperson, bdanggarankategori, bdanggarancabang, bdanggaranlokasi, 
        'bdanggarancostcenter, bdanggarandivisi, bdanggaransubdivisi, bdanggaranproyek, bduraian, bdcatatan, bdmatauang, 
        'bdkurs, bdstatus, bdstatussebelumnya, bdjmlrevisi, bdcetakanke, bdisclose, bdinputuser, 
        'bdinputtgl, bdmodifikasiuser, bdmodifikasitgl, bdposting, bdpostingtgl, bdcabangnama, bdlokasinama, 
        'bdkontakkode, bdkontaknama, bdanggarankategorinama, bdanggarancabangnama, bdanggaranlokasinama, bdanggarancostcenternama, bdanggarandivisinama, 
        'bdanggaransubdivisinama, bdanggaranproyeknama, bdstatusnama, bdstatussebelumnyanama, bdinputusernama, bdmodifikasiusernama

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
            Filter = Filter.Replace("bdkontakkode", "c.kkode")
            Filter = Filter.Replace("bdkontaknama", "c.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        ''PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m2_bd_v")

        sql = "select `bd`.`bdidhistory` AS `bdidhistory`,`bd`.`bdid` AS `bdid`,`bd`.`bdcabang` AS `bdcabang`,`bd`.`bdlokasi` AS `bdlokasi`,`bd`.`bdsumber` AS `bdsumber`,`bd`.`bdautonotransaksi` AS `bdautonotransaksi`,`bd`.`bdnotransaksi` AS `bdnotransaksi`,`bd`.`bdtgl` AS `bdtgl`,`bd`.`bdtglanggaran` AS `bdtglanggaran`,`bd`.`bdkodepa` AS `bdkodepa`,`bd`.`bdkontak` AS `bdkontak`,`bd`.`bdkontakperson` AS `bdkontakperson`,`bd`.`bdanggarankategori` AS `bdanggarankategori`,`bd`.`bdanggarancabang` AS `bdanggarancabang`,`bd`.`bdanggaranlokasi` AS `bdanggaranlokasi`,`bd`.`bdanggarancostcenter` AS `bdanggarancostcenter`,`bd`.`bdanggarandivisi` AS `bdanggarandivisi`,`bd`.`bdanggaransubdivisi` AS `bdanggaransubdivisi`,`bd`.`bdanggaranproyek` AS `bdanggaranproyek`,`bd`.`bduraian` AS `bduraian`,`bd`.`bdcatatan` AS `bdcatatan`,`bd`.`bdmatauang` AS `bdmatauang`,`bd`.`bdkurs` AS `bdkurs`,`bd`.`bdstatus` AS `bdstatus`,`bd`.`bdstatussebelumnya` AS `bdstatussebelumnya`,`bd`.`bdjmlrevisi` AS `bdjmlrevisi`,`bd`.`bdcetakanke` AS `bdcetakanke`,`bd`.`bdisclose` AS `bdisclose`,`bd`.`bdinputuser` AS `bdinputuser`,`bd`.`bdinputtgl` AS `bdinputtgl`,`bd`.`bdmodifikasiuser` AS `bdmodifikasiuser`,`bd`.`bdmodifikasitgl` AS `bdmodifikasitgl`,`bd`.`bdposting` AS `bdposting`,`bd`.`bdpostingtgl` AS `bdpostingtgl`,`br`.`bnama` AS `bdcabangnama`,`lc`.`lnama` AS `bdlokasinama`,`c`.`kkode` AS `bdkontakkode`,`c`.`knama` AS `bdkontaknama`,`rc`.`nama` AS `bdanggarankategorinama`,`br2`.`bnama` AS `bdanggarancabangnama`,`lc2`.`lnama` AS `bdanggaranlokasinama`,`cc2`.`ccnama` AS `bdanggarancostcenternama`,`d2`.`dnama` AS `bdanggarandivisinama`,`sd2`.`sdnama` AS `bdanggaransubdivisinama`,`p2`.`pnama` AS `bdanggaranproyeknama`,`st1`.`nama` AS `bdstatusnama`,`st2`.`nama` AS `bdstatussebelumnyanama`,`u1`.`unama` AS `bdinputusernama`,`u2`.`unama` AS `bdmodifikasiusernama` from ((((((((((((((`m2_bd_history` `bd` join `m0_status` `st1` on((`bd`.`bdstatus` = `st1`.`kode`))) join `m0_status` `st2` on((`bd`.`bdstatussebelumnya` = `st2`.`kode`))) join `m0_realization_category` `rc` on((`bd`.`bdanggarankategori` = `rc`.`kode`))) left join `m1_branch` `br` on((`bd`.`bdcabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`bd`.`bdlokasi` = `lc`.`lkode`))) left join `m1_contact` `c` on((`bd`.`bdkontak` = `c`.`kid`))) left join `m1_branch` `br2` on((`bd`.`bdanggarancabang` = `br2`.`bkode`))) left join `m1_location` `lc2` on((`bd`.`bdanggaranlokasi` = `lc2`.`lkode`))) left join `m1_cost_center` `cc2` on((`bd`.`bdanggarancostcenter` = `cc2`.`cckode`))) left join `m1_division` `d2` on((`bd`.`bdanggarandivisi` = `d2`.`dkode`))) left join `m1_subdivision` `sd2` on((`bd`.`bdanggaransubdivisi` = `sd2`.`sdkode`))) left join `m1_project` `p2` on((`bd`.`bdanggaranproyek` = `p2`.`pkode`))) left join `m0_user` `u1` on((`bd`.`bdinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`bd`.`bdmodifikasiuser` = `u2`.`userid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Bd", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("bdidhistory"), ""), sptField,
                     FxDB(dr("bdid"), ""), sptField,
                     FxDB(dr("bdcabang"), ""), sptField,
                     FxDB(dr("bdlokasi"), ""), sptField,
                     FxDB(dr("bdsumber"), ""), sptField,
                     FxDB(dr("bdautonotransaksi"), 0), sptField,
                     FxDB(dr("bdnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("bdtgl"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("bdtglanggaran"), ""), formatTgl), sptField,
                     FxDB(dr("bdkodepa"), ""), sptField,
                     FxDB(dr("bdkontak"), ""), sptField,
                     FxDB(dr("bdkontakperson"), ""), sptField,
                     FxDB(dr("bdanggarankategori"), 0), sptField,
                     FxDB(dr("bdanggarancabang"), ""), sptField,
                     FxDB(dr("bdanggaranlokasi"), ""), sptField,
                     FxDB(dr("bdanggarancostcenter"), ""), sptField,
                     FxDB(dr("bdanggarandivisi"), ""), sptField,
                     FxDB(dr("bdanggaransubdivisi"), ""), sptField,
                     FxDB(dr("bdanggaranproyek"), ""), sptField,
                     FxDB(dr("bduraian"), ""), sptField,
                     FxDB(dr("bdcatatan"), ""), sptField,
                     FxDB(dr("bdmatauang"), ""), sptField,
                     FxDB(dr("bdkurs"), 0), sptField,
                     FxDB(dr("bdstatus"), 0), sptField,
                     FxDB(dr("bdstatussebelumnya"), 0), sptField,
                     FxDB(dr("bdjmlrevisi"), 0), sptField,
                     FxDB(dr("bdcetakanke"), 0), sptField,
                     FxDB(dr("bdisclose"), 0), sptField,
                     FxDB(dr("bdinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("bdinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bdmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("bdmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bdposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("bdpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bdcabangnama"), ""), sptField,
                     FxDB(dr("bdlokasinama"), ""), sptField,
                     FxDB(dr("bdkontakkode"), ""), sptField,
                     FxDB(dr("bdkontaknama"), ""), sptField,
                     FxDB(dr("bdanggarankategorinama"), ""), sptField,
                     FxDB(dr("bdanggarancabangnama"), ""), sptField,
                     FxDB(dr("bdanggaranlokasinama"), ""), sptField,
                     FxDB(dr("bdanggarancostcenternama"), ""), sptField,
                     FxDB(dr("bdanggarandivisinama"), ""), sptField,
                     FxDB(dr("bdanggaransubdivisinama"), ""), sptField,
                     FxDB(dr("bdanggaranproyeknama"), ""), sptField,
                     FxDB(dr("bdstatusnama"), ""), sptField,
                     FxDB(dr("bdstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("bdinputusernama"), ""), sptField,
                     FxDB(dr("bdmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bdidhistory, bdid, bdcabang, bdlokasi, bdsumber, bdautonotransaksi, bdnotransaksi, bdtgl, bdtglanggaran, bdkodepa, bdkontak, bdkontakperson, bdanggarankategori, bdanggarancabang, bdanggaranlokasi, bdanggarancostcenter, bdanggarandivisi, bdanggaransubdivisi, bdanggaranproyek, bduraian, bdcatatan, bdmatauang, bdkurs, bdstatus, bdstatussebelumnya, bdjmlrevisi, bdcetakanke, bdisclose, bdinputuser, bdinputtgl, bdmodifikasiuser, bdmodifikasitgl, bdposting, bdpostingtgl, bdcabangnama, bdlokasinama, bdkontakkode, bdkontaknama, bdanggarankategorinama, bdanggarancabangnama, bdanggaranlokasinama, bdanggarancostcenternama, bdanggarandivisinama, bdanggaransubdivisinama, bdanggaranproyeknama, bdstatusnama, bdstatussebelumnyanama, bdinputusernama, bdmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_BdHistoryGetdataById(ByVal param As String) As String

        'M2_BdGetdataById Utama --------------------------------------------------------
        'bdidhistory, bdid, bdcabang, bdlokasi, bdsumber, bdautonotransaksi, bdnotransaksi, bdtgl, 
        'bdtglanggaran, bdkodepa, bdkontak, bdkontakperson, bdanggarankategori, bdanggarancabang, bdanggaranlokasi, 
        'bdanggarancostcenter, bdanggarandivisi, bdanggaransubdivisi, bdanggaranproyek, bduraian, bdcatatan, bdmatauang, 
        'bdkurs, bdstatus, bdstatussebelumnya, bdjmlrevisi, bdcetakanke, bdisclose, bdinputuser, 
        'bdinputtgl, bdmodifikasiuser, bdmodifikasitgl, bdposting, bdpostingtgl, bdcustomtext1, bdcustomtext2, 
        'bdcustomtext3, bdcustomtext4, bdcustomtext5, bdcustomint1, bdcustomint2, bdcustomint3, bdcustomdbl1, 
        'bdcustomdbl2, bdcustomdbl3, bdcustomdate1, bdcustomdate2, bdcustomdate3, bdcabangnama, bdlokasinama, 
        'bdkontakkode, bdkontaknama, bdanggarankategorinama, bdanggarancabangnama, bdanggaranlokasinama, bdanggarancostcenternama, bdanggarandivisinama, 
        'bdanggaransubdivisinama, bdanggaranproyeknama, bdstatusnama, bdstatussebelumnyanama, bdinputusernama, bdmodifikasiusernama

        'M2_BdGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idbddetail, 
        'idbd, norek, matauang, kurs, jumlah, jumlahvalas, catatan, 
        'costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, 
        'customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3, noreknama

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


        Dim NmMemcached As String = "aplikasi1-M2_Bd~M2_Bd_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "bdidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "bdidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        ''PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m2_bd_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "select `bd`.`bdidhistory` AS `bdidhistory`,`bd`.`bdid` AS `bdid`,`bd`.`bdcabang` AS `bdcabang`,`bd`.`bdlokasi` AS `bdlokasi`,`bd`.`bdsumber` AS `bdsumber`,`bd`.`bdautonotransaksi` AS `bdautonotransaksi`,`bd`.`bdnotransaksi` AS `bdnotransaksi`,`bd`.`bdtgl` AS `bdtgl`,`bd`.`bdtglanggaran` AS `bdtglanggaran`,`bd`.`bdkodepa` AS `bdkodepa`,`bd`.`bdkontak` AS `bdkontak`,`bd`.`bdkontakperson` AS `bdkontakperson`,`bd`.`bdanggarankategori` AS `bdanggarankategori`,`bd`.`bdanggarancabang` AS `bdanggarancabang`,`bd`.`bdanggaranlokasi` AS `bdanggaranlokasi`,`bd`.`bdanggarancostcenter` AS `bdanggarancostcenter`,`bd`.`bdanggarandivisi` AS `bdanggarandivisi`,`bd`.`bdanggaransubdivisi` AS `bdanggaransubdivisi`,`bd`.`bdanggaranproyek` AS `bdanggaranproyek`,`bd`.`bduraian` AS `bduraian`,`bd`.`bdcatatan` AS `bdcatatan`,`bd`.`bdmatauang` AS `bdmatauang`,`bd`.`bdkurs` AS `bdkurs`,`bd`.`bdstatus` AS `bdstatus`,`bd`.`bdstatussebelumnya` AS `bdstatussebelumnya`,`bd`.`bdjmlrevisi` AS `bdjmlrevisi`,`bd`.`bdcetakanke` AS `bdcetakanke`,`bd`.`bdisclose` AS `bdisclose`,`bd`.`bdinputuser` AS `bdinputuser`,`bd`.`bdinputtgl` AS `bdinputtgl`,`bd`.`bdmodifikasiuser` AS `bdmodifikasiuser`,`bd`.`bdmodifikasitgl` AS `bdmodifikasitgl`,`bd`.`bdposting` AS `bdposting`,`bd`.`bdpostingtgl` AS `bdpostingtgl`,`bd`.`bdcustomtext1` AS `bdcustomtext1`,`bd`.`bdcustomtext2` AS `bdcustomtext2`,`bd`.`bdcustomtext3` AS `bdcustomtext3`,`bd`.`bdcustomtext4` AS `bdcustomtext4`,`bd`.`bdcustomtext5` AS `bdcustomtext5`,`bd`.`bdcustomint1` AS `bdcustomint1`,`bd`.`bdcustomint2` AS `bdcustomint2`,`bd`.`bdcustomint3` AS `bdcustomint3`,`bd`.`bdcustomdbl1` AS `bdcustomdbl1`,`bd`.`bdcustomdbl2` AS `bdcustomdbl2`,`bd`.`bdcustomdbl3` AS `bdcustomdbl3`,`bd`.`bdcustomdate1` AS `bdcustomdate1`,`bd`.`bdcustomdate2` AS `bdcustomdate2`,`bd`.`bdcustomdate3` AS `bdcustomdate3`,`br`.`bnama` AS `bdcabangnama`,`lc`.`lnama` AS `bdlokasinama`,`c`.`kkode` AS `bdkontakkode`,`c`.`knama` AS `bdkontaknama`,`rc`.`nama` AS `bdanggarankategorinama`,`br2`.`bnama` AS `bdanggarancabangnama`,`lc2`.`lnama` AS `bdanggaranlokasinama`,`cc2`.`ccnama` AS `bdanggarancostcenternama`,`d2`.`dnama` AS `bdanggarandivisinama`,`sd2`.`sdnama` AS `bdanggaransubdivisinama`,`p2`.`pnama` AS `bdanggaranproyeknama`,`st1`.`nama` AS `bdstatusnama`,`st2`.`nama` AS `bdstatussebelumnyanama`,`u1`.`unama` AS `bdinputusernama`,`u2`.`unama` AS `bdmodifikasiusernama`,`bdd`.`idhistorydetail` AS `idhistorydetail`,`bdd`.`idhistory` AS `idhistory`,`bdd`.`idbddetail` AS `idbddetail`,`bdd`.`idbd` AS `idbd`,`bdd`.`norek` AS `norek`,`bdd`.`matauang` AS `matauang`,`bdd`.`kurs` AS `kurs`,`bdd`.`jumlah` AS `jumlah`,`bdd`.`jumlahvalas` AS `jumlahvalas`,`bdd`.`catatan` AS `catatan`,`bdd`.`costcenter` AS `costcenter`,`bdd`.`divisi` AS `divisi`,`bdd`.`subdivisi` AS `subdivisi`,`bdd`.`proyek` AS `proyek`,`bdd`.`urutan` AS `urutan`,`bdd`.`isclose` AS `isclose`,`bdd`.`customtext1` AS `customtext1`,`bdd`.`customtext2` AS `customtext2`,`bdd`.`customtext3` AS `customtext3`,`bdd`.`customdbl1` AS `customdbl1`,`bdd`.`customdbl2` AS `customdbl2`,`bdd`.`customdbl3` AS `customdbl3`,`bdd`.`customdate1` AS `customdate1`,`bdd`.`customdate2` AS `customdate2`,`bdd`.`customdate3` AS `customdate3`,`coa`.`cnama` AS `noreknama` from ((((((((((((((((`m2_bd_history` `bd` join `m0_status` `st1` on((`bd`.`bdstatus` = `st1`.`kode`))) join `m0_status` `st2` on((`bd`.`bdstatussebelumnya` = `st2`.`kode`))) join `m0_realization_category` `rc` on((`bd`.`bdanggarankategori` = `rc`.`kode`))) join `m2_bd_detail_history` `bdd` on((`bd`.`bdidhistory` = `bdd`.`idhistory`))) left join `m1_branch` `br` on((`bd`.`bdcabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`bd`.`bdlokasi` = `lc`.`lkode`))) left join `m1_contact` `c` on((`bd`.`bdkontak` = `c`.`kid`))) left join `m1_branch` `br2` on((`bd`.`bdanggarancabang` = `br2`.`bkode`))) left join `m1_location` `lc2` on((`bd`.`bdanggaranlokasi` = `lc2`.`lkode`))) left join `m1_cost_center` `cc2` on((`bd`.`bdanggarancostcenter` = `cc2`.`cckode`))) left join `m1_division` `d2` on((`bd`.`bdanggarandivisi` = `d2`.`dkode`))) left join `m1_subdivision` `sd2` on((`bd`.`bdanggaransubdivisi` = `sd2`.`sdkode`))) left join `m1_project` `p2` on((`bd`.`bdanggaranproyek` = `p2`.`pkode`))) left join `m0_user` `u1` on((`bd`.`bdinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`bd`.`bdmodifikasiuser` = `u2`.`userid`))) left join `m1_coa` `coa` on((`bdd`.`norek` = `coa`.`cnomor`)))"

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                     FxDB(drutama("bdidhistory"), ""), sptField,
                     FxDB(drutama("bdid"), ""), sptField,
                     FxDB(drutama("bdcabang"), ""), sptField,
                     FxDB(drutama("bdlokasi"), ""), sptField,
                     FxDB(drutama("bdsumber"), ""), sptField,
                     FxDB(drutama("bdautonotransaksi"), 0), sptField,
                     FxDB(drutama("bdnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("bdtgl"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("bdtglanggaran"), ""), formatTgl), sptField,
                     FxDB(drutama("bdkodepa"), ""), sptField,
                     FxDB(drutama("bdkontak"), ""), sptField,
                     FxDB(drutama("bdkontakperson"), ""), sptField,
                     FxDB(drutama("bdanggarankategori"), 0), sptField,
                     FxDB(drutama("bdanggarancabang"), ""), sptField,
                     FxDB(drutama("bdanggaranlokasi"), ""), sptField,
                     FxDB(drutama("bdanggarancostcenter"), ""), sptField,
                     FxDB(drutama("bdanggarandivisi"), ""), sptField,
                     FxDB(drutama("bdanggaransubdivisi"), ""), sptField,
                     FxDB(drutama("bdanggaranproyek"), ""), sptField,
                     FxDB(drutama("bduraian"), ""), sptField,
                     FxDB(drutama("bdcatatan"), ""), sptField,
                     FxDB(drutama("bdmatauang"), ""), sptField,
                     FxDB(drutama("bdkurs"), 0), sptField,
                     FxDB(drutama("bdstatus"), 0), sptField,
                     FxDB(drutama("bdstatussebelumnya"), 0), sptField,
                     FxDB(drutama("bdjmlrevisi"), 0), sptField,
                     FxDB(drutama("bdcetakanke"), 0), sptField,
                     FxDB(drutama("bdisclose"), 0), sptField,
                     FxDB(drutama("bdinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("bdinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("bdmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("bdmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("bdposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bdpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("bdcustomtext1"), ""), sptField,
                     FxDB(drutama("bdcustomtext2"), ""), sptField,
                     FxDB(drutama("bdcustomtext3"), ""), sptField,
                     FxDB(drutama("bdcustomtext4"), ""), sptField,
                     FxDB(drutama("bdcustomtext5"), ""), sptField,
                     FxDB(drutama("bdcustomint1"), 0), sptField,
                     FxDB(drutama("bdcustomint2"), 0), sptField,
                     FxDB(drutama("bdcustomint3"), 0), sptField,
                     FxDB(drutama("bdcustomdbl1"), 0), sptField,
                     FxDB(drutama("bdcustomdbl2"), 0), sptField,
                     FxDB(drutama("bdcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bdcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("bdcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("bdcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("bdcabangnama"), ""), sptField,
                     FxDB(drutama("bdlokasinama"), ""), sptField,
                     FxDB(drutama("bdkontakkode"), ""), sptField,
                     FxDB(drutama("bdkontaknama"), ""), sptField,
                     FxDB(drutama("bdanggarankategorinama"), ""), sptField,
                     FxDB(drutama("bdanggarancabangnama"), ""), sptField,
                     FxDB(drutama("bdanggaranlokasinama"), ""), sptField,
                     FxDB(drutama("bdanggarancostcenternama"), ""), sptField,
                     FxDB(drutama("bdanggarandivisinama"), ""), sptField,
                     FxDB(drutama("bdanggaransubdivisinama"), ""), sptField,
                     FxDB(drutama("bdanggaranproyeknama"), ""), sptField,
                     FxDB(drutama("bdstatusnama"), ""), sptField,
                     FxDB(drutama("bdstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("bdinputusernama"), ""), sptField,
                     FxDB(drutama("bdmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idhistorydetail"), ""), sptField,
                     FxDB(dr("idhistory"), ""), sptField,
                     FxDB(dr("idbddetail"), ""), sptField,
                     FxDB(dr("idbd"), ""), sptField,
                     FxDB(dr("norek"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("jumlah"), 0), sptField,
                     FxDB(dr("jumlahvalas"), 0), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
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
                     FxDB(dr("noreknama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bdidhistory, bdid, bdcabang, bdlokasi, bdsumber, bdautonotransaksi, bdnotransaksi, bdtgl, bdtglanggaran, bdkodepa, bdkontak, bdkontakperson, bdanggarankategori, bdanggarancabang, bdanggaranlokasi, bdanggarancostcenter, bdanggarandivisi, bdanggaransubdivisi, bdanggaranproyek, bduraian, bdcatatan, bdmatauang, bdkurs, bdstatus, bdstatussebelumnya, bdjmlrevisi, bdcetakanke, bdisclose, bdinputuser, bdinputtgl, bdmodifikasiuser, bdmodifikasitgl, bdposting, bdpostingtgl, bdcustomtext1, bdcustomtext2, bdcustomtext3, bdcustomtext4, bdcustomtext5, bdcustomint1, bdcustomint2, bdcustomint3, bdcustomdbl1, bdcustomdbl2, bdcustomdbl3, bdcustomdate1, bdcustomdate2, bdcustomdate3, bdcabangnama, bdlokasinama, bdkontakkode, bdkontaknama, bdanggarankategorinama, bdanggarancabangnama, bdanggaranlokasinama, bdanggarancostcenternama, bdanggarandivisinama, bdanggaransubdivisinama, bdanggaranproyeknama, bdstatusnama, bdstatussebelumnyanama, bdinputusernama, bdmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idbddetail, idbd, norek, matauang, kurs, jumlah, jumlahvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, noreknama"))

        Return wsResult
    End Function

End Class