Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_rnr_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_RnrHistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m5_rnr_history(SELECT 0, rnr.* FROM m5_rnr rnr WHERE rnr.rnrid = '" & idtransaksi & "')"
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
            sql = "SELECT rnridhistory FROM m5_rnr_history WHERE rnrid = '" & idtransaksi & "' ORDER BY rnrmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m5_rnr_detail_history (SELECT 0, '" & result(4) & "', rnr.* FROM m5_rnr_detail rnr WHERE rnr.idrnr = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY DETAIL -------------------------------

            'PROSES INSERT HISTORY BATCH ---------------------------------------
            sql = "INSERT INTO m1_no_batch_transaction_history(SELECT 0, '" & result(4) & "', nb.* FROM m1_no_batch_transaction nb WHERE nb.nbtidtransaksi = '" & idtransaksi & "' and nb.nbtsumber = 'RNR')"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY BATCH --------------------------------

            'PROSES INSERT HISTORY SERIAL ---------------------------------------
            sql = "INSERT INTO m1_no_serial_transaction_history(SELECT 0, '" & result(4) & "', ns.* FROM m1_no_serial_transaction ns WHERE ns.nstidtransaksi = '" & idtransaksi & "' and ns.nstsumber = 'RNR')"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY SERIAL --------------------------------

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
    Public Function M5_RnrHistorySearch(ByVal param As String) As String
        'M5_Sr_HistorySearch --------------------------------------------------------
        'rnridhistory, rnrid, rnrcabang, rnrlokasi, rnrgudang, rnrasalbarang, rnrasalbarangkategori, rnrjenispenjualan, 
        'rnrjenispenjualankategori, rnrcarabayar, rnrsumber, rnrautonotransaksi, rnrnotransaksi, rnrtgl, rnrkodepa, 
        'rnrcustomer, rnrcustomerkontak, rnr1alamat1, rnr1alamat2, rnr1alamat3, rnr2alamat1, rnr2alamat2, 
        'rnr2alamat3, rnrbagianpenjualan, rnrekspedisi, rnrtglkirim, rnrtermin, rnrtgljatuhtempo, rnruraian, 
        'rnrcatatan, rnrnoref, rnrtglnoref, rnrtglpenutupan, rnrmatauang, rnrkurs, rnrhargatermasukpajak, 
        'rnrtotal, rnrdiskonpersen, rnrjmldiskon, rnrtotalpajak1detail, rnrtotalpajak2detail, rnrbiayalainpersen, rnrbiayalain, 
        'rnrtotaltransaksi, rnrjmlbayar, rnrstatuslunas, rnrtgllunas, rnrnofakturpajak, rnrsdhbayarpajak, rnrtglbayarpajak, 
        'rnrrekdiskon, rnrrekpajak1, rnrrekpajak2, rnrrekbiayalain, rnrrekbayar, rnridsq, rnridso, 
        'rnridpl, rnriddo, rnriddr, rnridpi, rnridsi, rnrstatussr, rnrstatusrealisasi, 
        'rnrstatus, rnrstatussebelumnya, rnrjmlrevisi, rnrcetakanke, rnrinputuser, rnrinputtgl, rnrmodifikasiuser, 
        'rnrmodifikasitgl, rnrposting, rnrpostingtgl, rnrtutupperiode, rnrisclose, rnrcabangnama, rnrlokasinama, 
        'rnrgudangnama, rnrcustomerkode, rnrcustomernama, rnrbagianpenjualankode, rnrbagianpenjualannama, rnrekspedisinama, donotransaksi, 
        'drnotransaksi, pinotransaksi, sinotransaksi, rnrstatusnama, rnrstatussebelumnyanama, rnrinputusernama, rnrmodifikasiusernama

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
            Filter = Filter.Replace("rnrcustomerkode", "c1.kkode")
            Filter = Filter.Replace("rnrcustomernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m5_sr_v_history")
        sql = "select rnr.rnridhistory, `rnr`.`rnrid` AS `rnrid`,`rnr`.`rnrcabang` AS `rnrcabang`,`rnr`.`rnrlokasi` AS `rnrlokasi`,`rnr`.`rnrgudang` AS `rnrgudang`,`rnr`.`rnrasalbarang` AS `rnrasalbarang`,`rnr`.`rnrasalbarangkategori` AS `rnrasalbarangkategori`,`rnr`.`rnrjenispenjualan` AS `rnrjenispenjualan`,`rnr`.`rnrjenispenjualankategori` AS `rnrjenispenjualankategori`,`rnr`.`rnrcarabayar` AS `rnrcarabayar`,`rnr`.`rnrsumber` AS `rnrsumber`,`rnr`.`rnrautonotransaksi` AS `rnrautonotransaksi`,`rnr`.`rnrnotransaksi` AS `rnrnotransaksi`,`rnr`.`rnrtgl` AS `rnrtgl`,`rnr`.`rnrkodepa` AS `rnrkodepa`,`rnr`.`rnrcustomer` AS `rnrcustomer`,`rnr`.`rnrcustomerkontak` AS `rnrcustomerkontak`,`rnr`.`rnr1alamat1` AS `rnr1alamat1`,`rnr`.`rnr1alamat2` AS `rnr1alamat2`,`rnr`.`rnr1alamat3` AS `rnr1alamat3`,`rnr`.`rnr2alamat1` AS `rnr2alamat1`,`rnr`.`rnr2alamat2` AS `rnr2alamat2`,`rnr`.`rnr2alamat3` AS `rnr2alamat3`,`rnr`.`rnrbagianpenjualan` AS `rnrbagianpenjualan`,`rnr`.`rnrekspedisi` AS `rnrekspedisi`,`rnr`.`rnrtglkirim` AS `rnrtglkirim`,`rnr`.`rnrtermin` AS `rnrtermin`,`rnr`.`rnrtgljatuhtempo` AS `rnrtgljatuhtempo`,`rnr`.`rnruraian` AS `rnruraian`,`rnr`.`rnrcatatan` AS `rnrcatatan`,`rnr`.`rnrnoref` AS `rnrnoref`,`rnr`.`rnrtglnoref` AS `rnrtglnoref`,`rnr`.`rnrtglpenutupan` AS `rnrtglpenutupan`,`rnr`.`rnrmatauang` AS `rnrmatauang`,`rnr`.`rnrkurs` AS `rnrkurs`,`rnr`.`rnrhargatermasukpajak` AS `rnrhargatermasukpajak`,`rnr`.`rnrtotal` AS `rnrtotal`,`rnr`.`rnrdiskonpersen` AS `rnrdiskonpersen`,`rnr`.`rnrjmldiskon` AS `rnrjmldiskon`,`rnr`.`rnrtotalpajak1detail` AS `rnrtotalpajak1detail`,`rnr`.`rnrtotalpajak2detail` AS `rnrtotalpajak2detail`,`rnr`.`rnrbiayalainpersen` AS `rnrbiayalainpersen`,`rnr`.`rnrbiayalain` AS `rnrbiayalain`,`rnr`.`rnrtotaltransaksi` AS `rnrtotaltransaksi`,`rnr`.`rnrjmlbayar` AS `rnrjmlbayar`,`rnr`.`rnrstatuslunas` AS `rnrstatuslunas`,`rnr`.`rnrtgllunas` AS `rnrtgllunas`,`rnr`.`rnrnofakturpajak` AS `rnrnofakturpajak`,`rnr`.`rnrsdhbayarpajak` AS `rnrsdhbayarpajak`,`rnr`.`rnrtglbayarpajak` AS `rnrtglbayarpajak`,`rnr`.`rnrrekdiskon` AS `rnrrekdiskon`,`rnr`.`rnrrekpajak1` AS `rnrrekpajak1`,`rnr`.`rnrrekpajak2` AS `rnrrekpajak2`,`rnr`.`rnrrekbiayalain` AS `rnrrekbiayalain`,`rnr`.`rnrrekbayar` AS `rnrrekbayar`,`rnr`.`rnridsq` AS `rnridsq`,`rnr`.`rnridso` AS `rnridso`,`rnr`.`rnridpl` AS `rnridpl`,`rnr`.`rnriddo` AS `rnriddo`,`rnr`.`rnriddr` AS `rnriddr`,`rnr`.`rnridpi` AS `rnridpi`,`rnr`.`rnridsi` AS `rnridsi`,`rnr`.`rnrstatussr` AS `rnrstatussr`,`rnr`.`rnrstatusrealisasi` AS `rnrstatusrealisasi`,`rnr`.`rnrstatus` AS `rnrstatus`,`rnr`.`rnrstatussebelumnya` AS `rnrstatussebelumnya`,`rnr`.`rnrjmlrevisi` AS `rnrjmlrevisi`,`rnr`.`rnrcetakanke` AS `rnrcetakanke`,`rnr`.`rnrinputuser` AS `rnrinputuser`,`rnr`.`rnrinputtgl` AS `rnrinputtgl`,`rnr`.`rnrmodifikasiuser` AS `rnrmodifikasiuser`,`rnr`.`rnrmodifikasitgl` AS `rnrmodifikasitgl`,`rnr`.`rnrposting` AS `rnrposting`,`rnr`.`rnrpostingtgl` AS `rnrpostingtgl`,`rnr`.`rnrtutupperiode` AS `rnrtutupperiode`,`rnr`.`rnrisclose` AS `rnrisclose`,`br`.`bnama` AS `rnrcabangnama`,`lc`.`lnama` AS `rnrlokasinama`,`wh`.`wnama` AS `rnrgudangnama`,`c1`.`kkode` AS `rnrcustomerkode`,`c1`.`knama` AS `rnrcustomernama`,`c2`.`kkode` AS `rnrbagianpenjualankode`,`c2`.`knama` AS `rnrbagianpenjualannama`,`e`.`enama` AS `rnrekspedisinama`,`do`.`donotransaksi` AS `donotransaksi`,`dr`.`drnotransaksi` AS `drnotransaksi`,`pi`.`pinotransaksi` AS `pinotransaksi`,`si`.`sinotransaksi` AS `sinotransaksi`,`st1`.`nama` AS `rnrstatusnama`,`st2`.`nama` AS `rnrstatussebelumnyanama`,`u1`.`unama` AS `rnrinputusernama`,`u2`.`unama` AS `rnrmodifikasiusernama` from ((((((((((((((`m5_rnr_history` `rnr` left join `m1_branch` `br` on((`br`.`bkode` = `rnr`.`rnrcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `rnr`.`rnrlokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `rnr`.`rnrgudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `rnr`.`rnrcustomer`))) left join `m1_contact` `c2` on((`c2`.`kid` = `rnr`.`rnrbagianpenjualan`))) left join `m1_expedition` `e` on((`rnr`.`rnrekspedisi` = `e`.`ekode`))) left join `m5_do` `do` on((`rnr`.`rnriddo` = `do`.`doid`))) left join `m5_dr` `dr` on((`rnr`.`rnriddr` = `dr`.`drid`))) left join `m5_pi` `pi` on((`rnr`.`rnridpi` = `pi`.`piid`))) left join `m5_si` `si` on((`rnr`.`rnridsi` = `si`.`siid`))) left join `m0_status` `st1` on((`st1`.`kode` = `rnr`.`rnrstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `rnr`.`rnrstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `rnr`.`rnrinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `rnr`.`rnrmodifikasiuser`)))"

        dt = AmbilData("aplikasi1-M5_Sr", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("rnrid"), 0), sptField,
                     FxDB(dr("rnridhistory"), 0), sptField,
                     FxDB(dr("rnrcabang"), ""), sptField,
                     FxDB(dr("rnrlokasi"), ""), sptField,
                     FxDB(dr("rnrgudang"), ""), sptField,
                     FxDB(dr("rnrasalbarang"), ""), sptField,
                     FxDB(dr("rnrasalbarangkategori"), 0), sptField,
                     FxDB(dr("rnrjenispenjualan"), ""), sptField,
                     FxDB(dr("rnrjenispenjualankategori"), 0), sptField,
                     FxDB(dr("rnrcarabayar"), 0), sptField,
                     FxDB(dr("rnrsumber"), ""), sptField,
                     FxDB(dr("rnrautonotransaksi"), 0), sptField,
                     FxDB(dr("rnrnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rnrtgl"), ""), formatTgl), sptField,
                     FxDB(dr("rnrkodepa"), 0), sptField,
                     FxDB(dr("rnrcustomer"), 0), sptField,
                     FxDB(dr("rnrcustomerkontak"), ""), sptField,
                     FxDB(dr("rnr1alamat1"), ""), sptField,
                     FxDB(dr("rnr1alamat2"), ""), sptField,
                     FxDB(dr("rnr1alamat3"), ""), sptField,
                     FxDB(dr("rnr2alamat1"), ""), sptField,
                     FxDB(dr("rnr2alamat2"), ""), sptField,
                     FxDB(dr("rnr2alamat3"), ""), sptField,
                     FxDB(dr("rnrbagianpenjualan"), 0), sptField,
                     FxDB(dr("rnrekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rnrtglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("rnrtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rnrtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("rnruraian"), ""), sptField,
                     FxDB(dr("rnrcatatan"), ""), sptField,
                     FxDB(dr("rnrnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rnrtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("rnrtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("rnrmatauang"), ""), sptField,
                     FxDB(dr("rnrkurs"), 0), sptField,
                     FxDB(dr("rnrhargatermasukpajak"), 0), sptField,
                     FxDB(dr("rnrtotal"), 0), sptField,
                     FxDB(dr("rnrdiskonpersen"), ""), sptField,
                     FxDB(dr("rnrjmldiskon"), 0), sptField,
                     FxDB(dr("rnrtotalpajak1detail"), 0), sptField,
                     FxDB(dr("rnrtotalpajak2detail"), 0), sptField,
                     FxDB(dr("rnrbiayalainpersen"), 0), sptField,
                     FxDB(dr("rnrbiayalain"), 0), sptField,
                     FxDB(dr("rnrtotaltransaksi"), 0), sptField,
                     FxDB(dr("rnrjmlbayar"), 0), sptField,
                     FxDB(dr("rnrstatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rnrtgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("rnrnofakturpajak"), ""), sptField,
                     FxDB(dr("rnrsdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rnrtglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(dr("rnrrekdiskon"), ""), sptField,
                     FxDB(dr("rnrrekpajak1"), ""), sptField,
                     FxDB(dr("rnrrekpajak2"), ""), sptField,
                     FxDB(dr("rnrrekbiayalain"), ""), sptField,
                     FxDB(dr("rnrrekbayar"), ""), sptField,
                     FxDB(dr("rnridsq"), 0), sptField,
                     FxDB(dr("rnridso"), 0), sptField,
                     FxDB(dr("rnridpl"), 0), sptField,
                     FxDB(dr("rnriddo"), 0), sptField,
                     FxDB(dr("rnriddr"), 0), sptField,
                     FxDB(dr("rnridpi"), 0), sptField,
                     FxDB(dr("rnridsi"), 0), sptField,
                     FxDB(dr("rnrstatussr"), 0), sptField,
                     FxDB(dr("rnrstatusrealisasi"), 0), sptField,
                     FxDB(dr("rnrstatus"), 0), sptField,
                     FxDB(dr("rnrstatussebelumnya"), 0), sptField,
                     FxDB(dr("rnrjmlrevisi"), 0), sptField,
                     FxDB(dr("rnrcetakanke"), 0), sptField,
                     FxDB(dr("rnrinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rnrinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rnrmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rnrmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rnrposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rnrpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rnrtutupperiode"), 0), sptField,
                     FxDB(dr("rnrisclose"), 0), sptField,
                     FxDB(dr("rnrcabangnama"), ""), sptField,
                     FxDB(dr("rnrlokasinama"), ""), sptField,
                     FxDB(dr("rnrgudangnama"), ""), sptField,
                     FxDB(dr("rnrcustomerkode"), ""), sptField,
                     FxDB(dr("rnrcustomernama"), ""), sptField,
                     FxDB(dr("rnrbagianpenjualankode"), ""), sptField,
                     FxDB(dr("rnrbagianpenjualannama"), ""), sptField,
                     FxDB(dr("rnrekspedisinama"), ""), sptField,
                     FxDB(dr("donotransaksi"), ""), sptField,
                     FxDB(dr("drnotransaksi"), ""), sptField,
                     FxDB(dr("pinotransaksi"), ""), sptField,
                     FxDB(dr("sinotransaksi"), ""), sptField,
                     FxDB(dr("rnrstatusnama"), ""), sptField,
                     FxDB(dr("rnrstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("rnrinputusernama"), ""), sptField,
                     FxDB(dr("rnrmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rnridhistory, rnrid, rnrcabang, rnrlokasi, rnrgudang, rnrasalbarang, rnrasalbarangkategori, rnrjenispenjualan, rnrjenispenjualankategori, rnrcarabayar, rnrsumber, rnrautonotransaksi, rnrnotransaksi, rnrtgl, rnrkodepa, rnrcustomer, rnrcustomerkontak, rnr1alamat1, rnr1alamat2, rnr1alamat3, rnr2alamat1, rnr2alamat2, rnr2alamat3, rnrbagianpenjualan, rnrekspedisi, rnrtglkirim, rnrtermin, rnrtgljatuhtempo, rnruraian, rnrcatatan, rnrnoref, rnrtglnoref, rnrtglpenutupan, rnrmatauang, rnrkurs, rnrhargatermasukpajak, rnrtotal, rnrdiskonpersen, rnrjmldiskon, rnrtotalpajak1detail, rnrtotalpajak2detail, rnrbiayalainpersen, rnrbiayalain, rnrtotaltransaksi, rnrjmlbayar, rnrstatuslunas, rnrtgllunas, rnrnofakturpajak, rnrsdhbayarpajak, rnrtglbayarpajak, rnrrekdiskon, rnrrekpajak1, rnrrekpajak2, rnrrekbiayalain, rnrrekbayar, rnridsq, rnridso, rnridpl, rnriddo, rnriddr, rnridpi, rnridsi, rnrstatussr, rnrstatusrealisasi, rnrstatus, rnrstatussebelumnya, rnrjmlrevisi, rnrcetakanke, rnrinputuser, rnrinputtgl, rnrmodifikasiuser, rnrmodifikasitgl, rnrposting, rnrpostingtgl, rnrtutupperiode, rnrisclose, rnrcabangnama, rnrlokasinama, rnrgudangnama, rnrcustomerkode, rnrcustomernama, rnrbagianpenjualankode, rnrbagianpenjualannama, rnrekspedisinama, donotransaksi, drnotransaksi, pinotransaksi, sinotransaksi, rnrstatusnama, rnrstatussebelumnyanama, rnrinputusernama, rnrmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_RnrHistoryGetdataById(ByVal param As String) As String

        'M5_RnrHistoryGetdataById Utama --------------------------------------------------------
        'rnridhistory, rnrid, rnrcabang, rnrlokasi, rnrgudang, rnrasalbarang, rnrasalbarangkategori, rnrjenispenjualan, 
        'rnrjenispenjualankategori, rnrcarabayar, rnrsumber, rnrautonotransaksi, rnrnotransaksi, rnrtgl, rnrkodepa, 
        'rnrcustomer, rnrcustomerkontak, rnr1alamat1, rnr1alamat2, rnr1alamat3, rnr2alamat1, rnr2alamat2, 
        'rnr2alamat3, rnrbagianpenjualan, rnrekspedisi, rnrtglkirim, rnrtermin, rnrtgljatuhtempo, rnruraian, 
        'rnrcatatan, rnrnoref, rnrtglnoref, rnrtglpenutupan, rnrmatauang, rnrkurs, rnrhargatermasukpajak, 
        'rnrtotal, rnrdiskonpersen, rnrjmldiskon, rnrtotalpajak1detail, rnrtotalpajak2detail, rnrbiayalainpersen, rnrbiayalain, 
        'rnrtotaltransaksi, rnrjmlbayar, rnrstatuslunas, rnrtgllunas, rnrnofakturpajak, rnrsdhbayarpajak, rnrtglbayarpajak, 
        'rnrrekdiskon, rnrrekpajak1, rnrrekpajak2, rnrrekbiayalain, rnrrekbayar, rnridsq, rnridso, 
        'rnridpl, rnriddo, rnriddr, rnridpi, rnridsi, rnrstatussr, rnrstatusrealisasi, 
        'rnrstatus, rnrstatussebelumnya, rnrjmlrevisi, rnrcetakanke, rnrinputuser, rnrinputtgl, rnrmodifikasiuser, 
        'rnrmodifikasitgl, rnrposting, rnrpostingtgl, rnrtutupperiode, rnrisclose, rnrcustomtext1, rnrcustomtext2, 
        'rnrcustomtext3, rnrcustomtext4, rnrcustomtext5, rnrcustomint1, rnrcustomint2, rnrcustomint3, rnrcustomdbl1, 
        'rnrcustomdbl2, rnrcustomdbl3, rnrcustomdate1, rnrcustomdate2, rnrcustomdate3, rnrcabangnama, rnrlokasinama, 
        'rnrgudangnama, rnrcustomerkode, rnrcustomernama, rnrbagianpenjualankode, rnrbagianpenjualannama, rnrekspedisinama, rnrterminnama, 
        'rnrterminharijatuhtempo, rnrrekdiskonnama, rnrrekpajak1nama, rnrrekpajak2nama, rnrrekbiayalainnama, rnrrekbayarnama, rnrnotransaksipi, 
        'rnrnotransaksisi, rnrstatusnama, rnrstatussebelumnyanama, rnrinputusernama, rnrmodifikasiusernama, ktingkatjual, kpkp

        'M5_RnrHistoryGetdataById Detail --------------------------------------------------------
        'idhistorydetail, idhistory, idrnrdetail, idrnr, 
        'idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, 
        'satuanbarang, matauang, kurs, idhppkhususkeluar, idhppfifokeluar, harga, hargapricelist, 
        'hpp, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, 
        'cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekhargapokok, 
        'rekdiskonpenjualan, rekreturpenjualan, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idsqdetail, idsodetail, idpldetail, iddodetail, iddrdetail, idpidetail, 
        'idsidetail, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, 
        'customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, pajak1nama, 
        'pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangasalnama, gudangtransitnama, 
        'gudangtujuannama, costcenternama, divisinama, subdivisinama, proyeknama, pinotransaksi, sinotransaksi,
        'bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan

        'M5_RnrHistoryGetdataById Batch --------------------------------------------------------
        'nbtidhistory, nbtidtransaksihistory, nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang

        'M5_RnrHistoryGetdataById Serial --------------------------------------------------------
        'nstidhistory, nstidtransaksihistory, nstid, nstjenismutasi, nstidserialin, nstgudang,  nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, 
        'nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, 
        'nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang

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

        Dim utama As String = "", detail As String = "", batch As String = "", serial As String = "", idtransaksi As String = ""
        Dim sumber As String = "SR"

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

        Dim NmMemcached As String = "aplikasi1-M5_sr~M5_sr_Detail-" & idtransaksi

        'replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "sridhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "sridhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m5_sr_getdata_history")
        sql = "select rnr.rnridhistory, rnrd.idhistorydetail, rnrd.idhistory, `rnr`.`rnrid` AS `rnrid`,`rnr`.`rnrcabang` AS `rnrcabang`,`rnr`.`rnrlokasi` AS `rnrlokasi`,`rnr`.`rnrgudang` AS `rnrgudang`,`rnr`.`rnrasalbarang` AS `rnrasalbarang`,`rnr`.`rnrasalbarangkategori` AS `rnrasalbarangkategori`,`rnr`.`rnrjenispenjualan` AS `rnrjenispenjualan`,`rnr`.`rnrjenispenjualankategori` AS `rnrjenispenjualankategori`,`rnr`.`rnrcarabayar` AS `rnrcarabayar`,`rnr`.`rnrsumber` AS `rnrsumber`,`rnr`.`rnrautonotransaksi` AS `rnrautonotransaksi`,`rnr`.`rnrnotransaksi` AS `rnrnotransaksi`,`rnr`.`rnrtgl` AS `rnrtgl`,`rnr`.`rnrkodepa` AS `rnrkodepa`,`rnr`.`rnrcustomer` AS `rnrcustomer`,`rnr`.`rnrcustomerkontak` AS `rnrcustomerkontak`,`rnr`.`rnr1alamat1` AS `rnr1alamat1`,`rnr`.`rnr1alamat2` AS `rnr1alamat2`,`rnr`.`rnr1alamat3` AS `rnr1alamat3`,`rnr`.`rnr2alamat1` AS `rnr2alamat1`,`rnr`.`rnr2alamat2` AS `rnr2alamat2`,`rnr`.`rnr2alamat3` AS `rnr2alamat3`,`rnr`.`rnrbagianpenjualan` AS `rnrbagianpenjualan`,`rnr`.`rnrekspedisi` AS `rnrekspedisi`,`rnr`.`rnrtglkirim` AS `rnrtglkirim`,`rnr`.`rnrtermin` AS `rnrtermin`,`rnr`.`rnrtgljatuhtempo` AS `rnrtgljatuhtempo`,`rnr`.`rnruraian` AS `rnruraian`,`rnr`.`rnrcatatan` AS `rnrcatatan`,`rnr`.`rnrnoref` AS `rnrnoref`,`rnr`.`rnrtglnoref` AS `rnrtglnoref`,`rnr`.`rnrtglpenutupan` AS `rnrtglpenutupan`,`rnr`.`rnrmatauang` AS `rnrmatauang`,`rnr`.`rnrkurs` AS `rnrkurs`,`rnr`.`rnrhargatermasukpajak` AS `rnrhargatermasukpajak`,`rnr`.`rnrtotal` AS `rnrtotal`,`rnr`.`rnrdiskonpersen` AS `rnrdiskonpersen`,`rnr`.`rnrjmldiskon` AS `rnrjmldiskon`,`rnr`.`rnrtotalpajak1detail` AS `rnrtotalpajak1detail`,`rnr`.`rnrtotalpajak2detail` AS `rnrtotalpajak2detail`,`rnr`.`rnrbiayalainpersen` AS `rnrbiayalainpersen`,`rnr`.`rnrbiayalain` AS `rnrbiayalain`,`rnr`.`rnrtotaltransaksi` AS `rnrtotaltransaksi`,`rnr`.`rnrjmlbayar` AS `rnrjmlbayar`,`rnr`.`rnrstatuslunas` AS `rnrstatuslunas`,`rnr`.`rnrtgllunas` AS `rnrtgllunas`,`rnr`.`rnrnofakturpajak` AS `rnrnofakturpajak`,`rnr`.`rnrsdhbayarpajak` AS `rnrsdhbayarpajak`,`rnr`.`rnrtglbayarpajak` AS `rnrtglbayarpajak`,`rnr`.`rnrrekdiskon` AS `rnrrekdiskon`,`rnr`.`rnrrekpajak1` AS `rnrrekpajak1`,`rnr`.`rnrrekpajak2` AS `rnrrekpajak2`,`rnr`.`rnrrekbiayalain` AS `rnrrekbiayalain`,`rnr`.`rnrrekbayar` AS `rnrrekbayar`,`rnr`.`rnridsq` AS `rnridsq`,`rnr`.`rnridso` AS `rnridso`,`rnr`.`rnridpl` AS `rnridpl`,`rnr`.`rnriddo` AS `rnriddo`,`rnr`.`rnriddr` AS `rnriddr`,`rnr`.`rnridpi` AS `rnridpi`,`rnr`.`rnridsi` AS `rnridsi`,`rnr`.`rnrstatussr` AS `rnrstatussr`,`rnr`.`rnrstatusrealisasi` AS `rnrstatusrealisasi`,`rnr`.`rnrstatus` AS `rnrstatus`,`rnr`.`rnrstatussebelumnya` AS `rnrstatussebelumnya`,`rnr`.`rnrjmlrevisi` AS `rnrjmlrevisi`,`rnr`.`rnrcetakanke` AS `rnrcetakanke`,`rnr`.`rnrinputuser` AS `rnrinputuser`,`rnr`.`rnrinputtgl` AS `rnrinputtgl`,`rnr`.`rnrmodifikasiuser` AS `rnrmodifikasiuser`,`rnr`.`rnrmodifikasitgl` AS `rnrmodifikasitgl`,`rnr`.`rnrposting` AS `rnrposting`,`rnr`.`rnrpostingtgl` AS `rnrpostingtgl`,`rnr`.`rnrtutupperiode` AS `rnrtutupperiode`,`rnr`.`rnrisclose` AS `rnrisclose`,`rnr`.`rnrcustomtext1` AS `rnrcustomtext1`,`rnr`.`rnrcustomtext2` AS `rnrcustomtext2`,`rnr`.`rnrcustomtext3` AS `rnrcustomtext3`,`rnr`.`rnrcustomtext4` AS `rnrcustomtext4`,`rnr`.`rnrcustomtext5` AS `rnrcustomtext5`,`rnr`.`rnrcustomint1` AS `rnrcustomint1`,`rnr`.`rnrcustomint2` AS `rnrcustomint2`,`rnr`.`rnrcustomint3` AS `rnrcustomint3`,`rnr`.`rnrcustomdbl1` AS `rnrcustomdbl1`,`rnr`.`rnrcustomdbl2` AS `rnrcustomdbl2`,`rnr`.`rnrcustomdbl3` AS `rnrcustomdbl3`,`rnr`.`rnrcustomdate1` AS `rnrcustomdate1`,`rnr`.`rnrcustomdate2` AS `rnrcustomdate2`,`rnr`.`rnrcustomdate3` AS `rnrcustomdate3`,`br`.`bnama` AS `rnrcabangnama`,`lc`.`lnama` AS `rnrlokasinama`,`wh`.`wnama` AS `rnrgudangnama`,`c1`.`ktingkatjual`,`c1`.`kkode` AS `rnrcustomerkode`,`c1`.`knama` AS `rnrcustomernama`,`c2`.`kkode` AS `rnrbagianpenjualankode`,`c2`.`knama` AS `rnrbagianpenjualannama`,`e`.`enama` AS `rnrekspedisinama`,`tr`.`trnama` AS `rnrterminnama`,`tr`.`trharijatuhtempo` AS `rnrterminharijatuhtempo`,`coa1`.`cnama` AS `rnrrekdiskonnama`,`coa2`.`cnama` AS `rnrrekpajak1nama`,`coa3`.`cnama` AS `rnrrekpajak2nama`,`coa4`.`cnama` AS `rnrrekbiayalainnama`,`coa5`.`cnama` AS `rnrrekbayarnama`,`pi`.`pinotransaksi` AS `rnrnotransaksipi`,`si`.`sinotransaksi` AS `rnrnotransaksisi`,`st1`.`nama` AS `rnrstatusnama`,`st2`.`nama` AS `rnrstatussebelumnyanama`,`u1`.`unama` AS `rnrinputusernama`,`u2`.`unama` AS `rnrmodifikasiusernama`,`rnrd`.`idrnrdetail` AS `idrnrdetail`,`rnrd`.`idrnr` AS `idrnr`,`rnrd`.`idbarang` AS `idbarang`,`rnrd`.`namabarang` AS `namabarang`,`rnrd`.`tipebarang` AS `tipebarang`,`rnrd`.`jml` AS `jml`,`rnrd`.`satuan` AS `satuan`,`rnrd`.`nilaisatuan` AS `nilaisatuan`,`rnrd`.`jmlbarang` AS `jmlbarang`,`rnrd`.`satuanbarang` AS `satuanbarang`,`rnrd`.`matauang` AS `matauang`,`rnrd`.`kurs` AS `kurs`,`rnrd`.`idhppkhususkeluar` AS `idhppkhususkeluar`,`rnrd`.`idhppfifokeluar` AS `idhppfifokeluar`,`rnrd`.`harga` AS `harga`,`rnrd`.`hargapricelist` AS `hargapricelist`,`rnrd`.`hpp` AS `hpp`,`rnrd`.`diskon` AS `diskon`,`rnrd`.`jmldiskon` AS `jmldiskon`,`rnrd`.`pajak1` AS `pajak1`,`rnrd`.`jmlpajak1` AS `jmlpajak1`,`rnrd`.`pajak2` AS `pajak2`,`rnrd`.`jmlpajak2` AS `jmlpajak2`,`rnrd`.`cabang` AS `cabang`,`rnrd`.`lokasi` AS `lokasi`,`rnrd`.`gudangasal` AS `gudangasal`,`rnrd`.`gudangtransit` AS `gudangtransit`,`rnrd`.`gudangtujuan` AS `gudangtujuan`,`i`.`brekpersediaan` AS `rekpersediaan`,`rnrd`.`rekhargapokok` AS `rekhargapokok`,`rnrd`.`rekdiskonpenjualan` AS `rekdiskonpenjualan`,`rnrd`.`rekreturpenjualan` AS `rekreturpenjualan`,`rnrd`.`costcenter` AS `costcenter`,`rnrd`.`divisi` AS `divisi`,`rnrd`.`subdivisi` AS `subdivisi`,`rnrd`.`proyek` AS `proyek`,`rnrd`.`catatan` AS `catatan`,`rnrd`.`urutan` AS `urutan`,`rnrd`.`idsqdetail` AS `idsqdetail`,`rnrd`.`idsodetail` AS `idsodetail`,`rnrd`.`idpldetail` AS `idpldetail`,`rnrd`.`iddodetail` AS `iddodetail`,`rnrd`.`iddrdetail` AS `iddrdetail`,`rnrd`.`idpidetail` AS `idpidetail`,`rnrd`.`idsidetail` AS `idsidetail`,`rnrd`.`jmlsr` AS `jmlsr`,`rnrd`.`statussr` AS `statussr`,`rnrd`.`jmlrealisasi` AS `jmlrealisasi`,`rnrd`.`statusrealisasi` AS `statusrealisasi`,`rnrd`.`isclose` AS `isclose`,`rnrd`.`customtext1` AS `customtext1`,`rnrd`.`customtext2` AS `customtext2`,`rnrd`.`customtext3` AS `customtext3`,`rnrd`.`customdbl1` AS `customdbl1`,`rnrd`.`customdbl2` AS `customdbl2`,`rnrd`.`customdbl3` AS `customdbl3`,`rnrd`.`customdate1` AS `customdate1`,`rnrd`.`customdate2` AS `customdate2`,`rnrd`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bjenis` AS `bjenis`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,`brd`.`bnama` AS `cabangnama`,`lcd`.`lnama` AS `lokasinama`,`whd1`.`wnama` AS `gudangasalnama`,`whd2`.`wnama` AS `gudangtransitnama`,`whd3`.`wnama` AS `gudangtujuannama`,`cc`.`ccnama` AS `costcenternama`,`d`.`dnama` AS `divisinama`,`sd`.`sdnama` AS `subdivisinama`,`p`.`pnama` AS `proyeknama`,`pi2`.`pinotransaksi` AS `pinotransaksi`,`si2`.`sinotransaksi` AS `sinotransaksi`, c1.kpkp, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan from `m5_rnr_history` `rnr` join `m5_rnr_detail_history` `rnrd` on `rnr`.`rnridhistory` = `rnrd`.`idhistory` left join `m1_branch` `br` on `br`.`bkode` = `rnr`.`rnrcabang` left join `m1_location` `lc` on `lc`.`lkode` = `rnr`.`rnrlokasi` left join `m1_warehouse` `wh` on `wh`.`wkode` = `rnr`.`rnrgudang` left join `m1_contact` `c1` on `c1`.`kid` = `rnr`.`rnrcustomer` left join `m1_contact` `c2` on `c2`.`kid` = `rnr`.`rnrbagianpenjualan` left join `m1_expedition` `e` on `rnr`.`rnrekspedisi` = `e`.`ekode` left join `m1_terms` `tr` on `rnr`.`rnrtermin` = `tr`.`trkode` left join `m1_coa` `coa1` on `rnr`.`rnrrekdiskon` = `coa1`.`cnomor` left join `m1_coa` `coa2` on `rnr`.`rnrrekpajak1` = `coa2`.`cnomor` left join `m1_coa` `coa3` on `rnr`.`rnrrekpajak2` = `coa3`.`cnomor` left join `m1_coa` `coa4` on `rnr`.`rnrrekbiayalain` = `coa4`.`cnomor` left join `m1_coa` `coa5` on `rnr`.`rnrrekbayar` = `coa5`.`cnomor` left join `m5_pi` `pi` on `rnr`.`rnridpi` = `pi`.`piid` left join `m5_si` `si` on `rnr`.`rnridsi` = `si`.`siid` left join `m0_status` `st1` on `st1`.`kode` = `rnr`.`rnrstatus` left join `m0_status` `st2` on `st2`.`kode` = `rnr`.`rnrstatussebelumnya` left join `m0_user` `u1` on `u1`.`userid` = `rnr`.`rnrinputuser` left join `m0_user` `u2` on `u2`.`userid` = `rnr`.`rnrmodifikasiuser` left join `m1_item` `i` on `i`.`bid` = `rnrd`.`idbarang` left join `m1_tax` `t1` on `rnrd`.`pajak1` = `t1`.`tkode` left join `m1_tax` `t2` on `rnrd`.`pajak2` = `t2`.`tkode` left join `m1_branch` `brd` on `rnrd`.`cabang` = `brd`.`bkode` left join `m1_location` `lcd` on `rnrd`.`lokasi` = `lcd`.`lkode` left join `m1_warehouse` `whd1` on `rnrd`.`gudangasal` = `whd1`.`wkode` left join `m1_warehouse` `whd2` on `rnrd`.`gudangtransit` = `whd2`.`wkode` left join `m1_warehouse` `whd3` on `rnrd`.`gudangtujuan` = `whd3`.`wkode` left join `m1_cost_center` `cc` on `rnrd`.`costcenter` = `cc`.`cckode` left join `m1_division` `d` on `rnrd`.`divisi` = `d`.`dkode` left join `m1_subdivision` `sd` on `rnrd`.`subdivisi` = `sd`.`sdkode` left join `m1_project` `p` on `rnrd`.`proyek` = `p`.`pkode` left join `m5_pi_detail` `pid` on `rnrd`.`idpidetail` = `pid`.`idpidetail` left join `m5_pi` `pi2` on `pid`.`idpi` = `pi2`.`piid` left join `m5_si_detail` `sid` on `rnrd`.`idsidetail` = `sid`.`idsidetail` left join `m5_si` `si2` on `sid`.`idsi` = `si2`.`siid`"

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("rnridhistory"), 0), sptField,
                     FxDB(drutama("rnrid"), 0), sptField,
                     FxDB(drutama("rnrcabang"), ""), sptField,
                     FxDB(drutama("rnrlokasi"), ""), sptField,
                     FxDB(drutama("rnrgudang"), ""), sptField,
                     FxDB(drutama("rnrasalbarang"), ""), sptField,
                     FxDB(drutama("rnrasalbarangkategori"), 0), sptField,
                     FxDB(drutama("rnrjenispenjualan"), ""), sptField,
                     FxDB(drutama("rnrjenispenjualankategori"), 0), sptField,
                     FxDB(drutama("rnrcarabayar"), 0), sptField,
                     FxDB(drutama("rnrsumber"), ""), sptField,
                     FxDB(drutama("rnrautonotransaksi"), 0), sptField,
                     FxDB(drutama("rnrnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("rnrkodepa"), 0), sptField,
                     FxDB(drutama("rnrcustomer"), 0), sptField,
                     FxDB(drutama("rnrcustomerkontak"), ""), sptField,
                     FxDB(drutama("rnr1alamat1"), ""), sptField,
                     FxDB(drutama("rnr1alamat2"), ""), sptField,
                     FxDB(drutama("rnr1alamat3"), ""), sptField,
                     FxDB(drutama("rnr2alamat1"), ""), sptField,
                     FxDB(drutama("rnr2alamat2"), ""), sptField,
                     FxDB(drutama("rnr2alamat3"), ""), sptField,
                     FxDB(drutama("rnrbagianpenjualan"), 0), sptField,
                     FxDB(drutama("rnrekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrtglkirim"), ""), formatTgl), sptField,
                     FxDB(drutama("rnrtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("rnruraian"), ""), sptField,
                     FxDB(drutama("rnrcatatan"), ""), sptField,
                     FxDB(drutama("rnrnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("rnrmatauang"), ""), sptField,
                     FxDB(drutama("rnrkurs"), 0), sptField,
                     FxDB(drutama("rnrhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("rnrtotal"), 0), sptField,
                     FxDB(drutama("rnrdiskonpersen"), ""), sptField,
                     FxDB(drutama("rnrjmldiskon"), 0), sptField,
                     FxDB(drutama("rnrtotalpajak1detail"), 0), sptField,
                     FxDB(drutama("rnrtotalpajak2detail"), 0), sptField,
                     FxDB(drutama("rnrbiayalainpersen"), 0), sptField,
                     FxDB(drutama("rnrbiayalain"), 0), sptField,
                     FxDB(drutama("rnrtotaltransaksi"), 0), sptField,
                     FxDB(drutama("rnrjmlbayar"), 0), sptField,
                     FxDB(drutama("rnrstatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrtgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("rnrnofakturpajak"), ""), sptField,
                     FxDB(drutama("rnrsdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrtglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(drutama("rnrrekdiskon"), ""), sptField,
                     FxDB(drutama("rnrrekpajak1"), ""), sptField,
                     FxDB(drutama("rnrrekpajak2"), ""), sptField,
                     FxDB(drutama("rnrrekbiayalain"), ""), sptField,
                     FxDB(drutama("rnrrekbayar"), ""), sptField,
                     FxDB(drutama("rnridsq"), 0), sptField,
                     FxDB(drutama("rnridso"), 0), sptField,
                     FxDB(drutama("rnridpl"), 0), sptField,
                     FxDB(drutama("rnriddo"), 0), sptField,
                     FxDB(drutama("rnriddr"), 0), sptField,
                     FxDB(drutama("rnridpi"), 0), sptField,
                     FxDB(drutama("rnridsi"), 0), sptField,
                     FxDB(drutama("rnrstatussr"), 0), sptField,
                     FxDB(drutama("rnrstatusrealisasi"), 0), sptField,
                     FxDB(drutama("rnrstatus"), 0), sptField,
                     FxDB(drutama("rnrstatussebelumnya"), 0), sptField,
                     FxDB(drutama("rnrjmlrevisi"), 0), sptField,
                     FxDB(drutama("rnrcetakanke"), 0), sptField,
                     FxDB(drutama("rnrinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rnrmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rnrposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rnrtutupperiode"), 0), sptField,
                     FxDB(drutama("rnrisclose"), 0), sptField,
                     FxDB(drutama("rnrcustomtext1"), ""), sptField,
                     FxDB(drutama("rnrcustomtext2"), ""), sptField,
                     FxDB(drutama("rnrcustomtext3"), ""), sptField,
                     FxDB(drutama("rnrcustomtext4"), ""), sptField,
                     FxDB(drutama("rnrcustomtext5"), ""), sptField,
                     FxDB(drutama("rnrcustomint1"), 0), sptField,
                     FxDB(drutama("rnrcustomint2"), 0), sptField,
                     FxDB(drutama("rnrcustomint3"), 0), sptField,
                     FxDB(drutama("rnrcustomdbl1"), 0), sptField,
                     FxDB(drutama("rnrcustomdbl2"), 0), sptField,
                     FxDB(drutama("rnrcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("rnrcabangnama"), ""), sptField,
                     FxDB(drutama("rnrlokasinama"), ""), sptField,
                     FxDB(drutama("rnrgudangnama"), ""), sptField,
                     FxDB(drutama("rnrcustomerkode"), ""), sptField,
                     FxDB(drutama("rnrcustomernama"), ""), sptField,
                     FxDB(drutama("rnrbagianpenjualankode"), ""), sptField,
                     FxDB(drutama("rnrbagianpenjualannama"), ""), sptField,
                     FxDB(drutama("rnrekspedisinama"), ""), sptField,
                     FxDB(drutama("rnrterminnama"), ""), sptField,
                     FxDB(drutama("rnrterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("rnrrekdiskonnama"), ""), sptField,
                     FxDB(drutama("rnrrekpajak1nama"), ""), sptField,
                     FxDB(drutama("rnrrekpajak2nama"), ""), sptField,
                     FxDB(drutama("rnrrekbiayalainnama"), ""), sptField,
                     FxDB(drutama("rnrrekbayarnama"), ""), sptField,
                     FxDB(drutama("rnrnotransaksipi"), ""), sptField,
                     FxDB(drutama("rnrnotransaksisi"), ""), sptField,
                     FxDB(drutama("rnrstatusnama"), ""), sptField,
                     FxDB(drutama("rnrstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("rnrinputusernama"), ""), sptField,
                     FxDB(drutama("rnrmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("ktingkatjual"), 0), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idrnrdetail"), 0), sptField,
                     FxDB(dr("idrnr"), 0), sptField,
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
                     FxDB(dr("idhppkhususkeluar"), 0), sptField,
                     FxDB(dr("idhppfifokeluar"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("hargapricelist"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("diskon"), ""), sptField,
                     FxDB(dr("jmldiskon"), 0), sptField,
                     FxDB(dr("pajak1"), ""), sptField,
                     FxDB(dr("jmlpajak1"), 0), sptField,
                     FxDB(dr("pajak2"), ""), sptField,
                     FxDB(dr("jmlpajak2"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudangasal"), ""), sptField,
                     FxDB(dr("gudangtransit"), ""), sptField,
                     FxDB(dr("gudangtujuan"), ""), sptField,
                     FxDB(dr("rekpersediaan"), ""), sptField,
                     FxDB(dr("rekhargapokok"), ""), sptField,
                     FxDB(dr("rekdiskonpenjualan"), ""), sptField,
                     FxDB(dr("rekreturpenjualan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idsqdetail"), 0), sptField,
                     FxDB(dr("idsodetail"), 0), sptField,
                     FxDB(dr("idpldetail"), 0), sptField,
                     FxDB(dr("iddodetail"), 0), sptField,
                     FxDB(dr("iddrdetail"), 0), sptField,
                     FxDB(dr("idpidetail"), 0), sptField,
                     FxDB(dr("idsidetail"), 0), sptField,
                     FxDB(dr("jmlsr"), 0), sptField,
                     FxDB(dr("statussr"), 0), sptField,
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
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("cabangnama"), ""), sptField,
                     FxDB(dr("lokasinama"), ""), sptField,
                     FxDB(dr("gudangasalnama"), ""), sptField,
                     FxDB(dr("gudangtransitnama"), ""), sptField,
                     FxDB(dr("gudangtujuannama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("pinotransaksi"), ""), sptField,
                     FxDB(dr("sinotransaksi"), ""), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA BATCH
            sql = "select `nbt`.`nbtidhistory` AS `nbtidhistory`, `nbt`.`nbtidtransaksihistory` AS `nbtidtransaksihistory`,`nbt`.`nbtid` AS `nbtid`,`nbt`.`nbtjenismutasi` AS `nbtjenismutasi`,`nbt`.`nbtidbatchin` AS `nbtidbatchin`,`nbt`.`nbtgudang` AS `nbtgudang`,`nbt`.`nbtidbarang` AS `nbtidbarang`,`nbt`.`nbtkode` AS `nbtkode`,`nbt`.`nbtsumber` AS `nbtsumber`,`nbt`.`nbtidtransaksi` AS `nbtidtransaksi`,`nbt`.`nbtsatuan` AS `nbtsatuan`,`nbt`.`nbtjml` AS `nbtjml`,`nbt`.`nbtcustomtext1` AS `nbtcustomtext1`,`nbt`.`nbtcustomtext2` AS `nbtcustomtext2`,`nbt`.`nbtcustomtext3` AS `nbtcustomtext3`,`nbt`.`nbtcustomdbl1` AS `nbtcustomdbl1`,`nbt`.`nbtcustomdbl2` AS `nbtcustomdbl2`,`nbt`.`nbtcustomdbl3` AS `nbtcustomdbl3`,`nbt`.`nbtcustomdate1` AS `nbtcustomdate1`,`nbt`.`nbtcustomdate2` AS `nbtcustomdate2`,`nbt`.`nbtcustomdate3` AS `nbtcustomdate3`,`i`.`bkode` AS `kodebarang` from ((`m1_no_batch_transaction_history` `nbt` join `m1_item` `i` on((`nbt`.`nbtidbarang` = `i`.`bid`))) left join `m1_no_batch_in` `nbi` on((`nbt`.`nbtidbatchin` = `nbi`.`nbiidbatchin`)))"
            Dim dtbatch As New DataTable
            dtbatch = AmbilData("aplikasi1-m1_no_batch_out", "nbtidtransaksihistory = '" & idtransaksi & "' AND nbtsumber = '" & sumber & "' AND (nbtjenismutasi = 1 OR nbiidbarang IS NOT NULL)", "nbtidbarang, nbtkode ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtbatch.Rows
                batch = String.Concat(batch, FxDB(dr("nbtidhistory"), 0), sptField, FxDB(dr("nbtidtransaksihistory"), 0), sptField,
                     FxDB(dr("nbtid"), 0), sptField,
                     FxDB(dr("nbtjenismutasi"), 0), sptField,
                     FxDB(dr("nbtidbatchin"), 0), sptField,
                     FxDB(dr("nbtgudang"), ""), sptField,
                     FxDB(dr("nbtidbarang"), 0), sptField,
                     FxDB(dr("nbtkode"), ""), sptField,
                     FxDB(dr("nbtsumber"), ""), sptField,
                     FxDB(dr("nbtidtransaksi"), 0), sptField,
                     FxDB(dr("nbtsatuan"), ""), sptField,
                     FxDB(dr("nbtjml"), 0), sptField,
                     FxDB(dr("nbtcustomtext1"), ""), sptField,
                     FxDB(dr("nbtcustomtext2"), ""), sptField,
                     FxDB(dr("nbtcustomtext3"), ""), sptField,
                     FxDB(dr("nbtcustomdbl1"), 0), sptField,
                     FxDB(dr("nbtcustomdbl2"), 0), sptField,
                     FxDB(dr("nbtcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("nbtcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("nbtcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("nbtcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kodebarang"), ""), sptRow)
            Next
            If batch.Length > 0 Then batch = batch.Substring(0, batch.Length - sptRow.Length) Else batch = batch

            'AMBIL DATA SERIAL
            sql = "select `nst`.`nstidhistory` AS `nstidhistory`,`nst`.`nstidtransaksihistory` AS `nstidtransaksihistory`,`nst`.`nstid` AS `nstid`,`nst`.`nstjenismutasi` AS `nstjenismutasi`,`nst`.`nstidserialin` AS `nstidserialin`,`nst`.`nstgudang` AS `nstgudang`,`nst`.`nstidbarang` AS `nstidbarang`,`nst`.`nstkode` AS `nstkode`,`nst`.`nstsumber` AS `nstsumber`,`nst`.`nstidtransaksi` AS `nstidtransaksi`,`nst`.`nstsatuan` AS `nstsatuan`,`nst`.`nstjml` AS `nstjml`,`nst`.`nstcustomtext1` AS `nstcustomtext1`,`nst`.`nstcustomtext2` AS `nstcustomtext2`,`nst`.`nstcustomtext3` AS `nstcustomtext3`,`nst`.`nstcustomdbl1` AS `nstcustomdbl1`,`nst`.`nstcustomdbl2` AS `nstcustomdbl2`,`nst`.`nstcustomdbl3` AS `nstcustomdbl3`,`nst`.`nstcustomdate1` AS `nstcustomdate1`,`nst`.`nstcustomdate2` AS `nstcustomdate2`,`nst`.`nstcustomdate3` AS `nstcustomdate3`,`i`.`bkode` AS `kodebarang` from ((`m1_no_serial_transaction_history` `nst` join `m1_item` `i` on((`nst`.`nstidbarang` = `i`.`bid`))) left join `m1_no_serial_in` `nsi` on((`nst`.`nstidserialin` = `nsi`.`nsiidserialin`)))"
            Dim dtserial As New DataTable
            dtserial = AmbilData("aplikasi1-m1_no_serial_out", "nstidtransaksihistory = '" & idtransaksi & "' AND nstsumber = '" & sumber & "' AND (nstjenismutasi = 1 OR nsiidbarang IS NOT NULL)", "nstidbarang, nstkode ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtserial.Rows
                serial = String.Concat(serial, FxDB(dr("nstidhistory"), 0), sptField, FxDB(dr("nstidtransaksihistory"), 0), sptField,
                     FxDB(dr("nstid"), 0), sptField,
                     FxDB(dr("nstjenismutasi"), 0), sptField,
                     FxDB(dr("nstidserialin"), 0), sptField,
                     FxDB(dr("nstgudang"), ""), sptField,
                     FxDB(dr("nstidbarang"), 0), sptField,
                     FxDB(dr("nstkode"), ""), sptField,
                     FxDB(dr("nstsumber"), ""), sptField,
                     FxDB(dr("nstidtransaksi"), 0), sptField,
                     FxDB(dr("nstsatuan"), ""), sptField,
                     FxDB(dr("nstjml"), 0), sptField,
                     FxDB(dr("nstcustomtext1"), ""), sptField,
                     FxDB(dr("nstcustomtext2"), ""), sptField,
                     FxDB(dr("nstcustomtext3"), ""), sptField,
                     FxDB(dr("nstcustomdbl1"), 0), sptField,
                     FxDB(dr("nstcustomdbl2"), 0), sptField,
                     FxDB(dr("nstcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("nstcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("nstcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("nstcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kodebarang"), ""), sptRow)
            Next
            If serial.Length > 0 Then serial = serial.Substring(0, serial.Length - sptRow.Length) Else serial = serial

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, batch, sptSubParam, serial)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rnridhistory, rnrid, rnrcabang, rnrlokasi, rnrgudang, rnrasalbarang, rnrasalbarangkategori, rnrjenispenjualan, rnrjenispenjualankategori, rnrcarabayar, rnrsumber, rnrautonotransaksi, rnrnotransaksi, rnrtgl, rnrkodepa, rnrcustomer, rnrcustomerkontak, rnr1alamat1, rnr1alamat2, rnr1alamat3, rnr2alamat1, rnr2alamat2, rnr2alamat3, rnrbagianpenjualan, rnrekspedisi, rnrtglkirim, rnrtermin, rnrtgljatuhtempo, rnruraian, rnrcatatan, rnrnoref, rnrtglnoref, rnrtglpenutupan, rnrmatauang, rnrkurs, rnrhargatermasukpajak, rnrtotal, rnrdiskonpersen, rnrjmldiskon, rnrtotalpajak1detail, rnrtotalpajak2detail, rnrbiayalainpersen, rnrbiayalain, rnrtotaltransaksi, rnrjmlbayar, rnrstatuslunas, rnrtgllunas, rnrnofakturpajak, rnrsdhbayarpajak, rnrtglbayarpajak, rnrrekdiskon, rnrrekpajak1, rnrrekpajak2, rnrrekbiayalain, rnrrekbayar, rnridsq, rnridso, rnridpl, rnriddo, rnriddr, rnridpi, rnridsi, rnrstatussr, rnrstatusrealisasi, rnrstatus, rnrstatussebelumnya, rnrjmlrevisi, rnrcetakanke, rnrinputuser, rnrinputtgl, rnrmodifikasiuser, rnrmodifikasitgl, rnrposting, rnrpostingtgl, rnrtutupperiode, rnrisclose, rnrcustomtext1, rnrcustomtext2, rnrcustomtext3, rnrcustomtext4, rnrcustomtext5, rnrcustomint1, rnrcustomint2, rnrcustomint3, rnrcustomdbl1, rnrcustomdbl2, rnrcustomdbl3, rnrcustomdate1, rnrcustomdate2, rnrcustomdate3, rnrcabangnama, rnrlokasinama, rnrgudangnama, rnrcustomerkode, rnrcustomernama, rnrbagianpenjualankode, rnrbagianpenjualannama, rnrekspedisinama, rnrterminnama, rnrterminharijatuhtempo, rnrrekdiskonnama, rnrrekpajak1nama, rnrrekpajak2nama, rnrrekbiayalainnama, rnrrekbayarnama, rnrnotransaksipi, rnrnotransaksisi, rnrstatusnama, rnrstatussebelumnyanama, rnrinputusernama, rnrmodifikasiusernama, ktingkatjual, kpkp" & sptSubParam & "idhistorydetail, idhistory, idrnrdetail, idrnr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, idhppkhususkeluar, idhppfifokeluar, harga, hargapricelist, hpp, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekreturpenjualan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpldetail, iddodetail, iddrdetail, idpidetail, idsidetail, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangasalnama, gudangtransitnama, gudangtujuannama, costcenternama, divisinama, subdivisinama, proyeknama, pinotransaksi, sinotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan" & sptSubParam & "nbtidhistory, nbtidtransaksihistory, nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang" & sptSubParam & "nstidhistory, nstidtransaksihistory, nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang"))

        Return wsResult
    End Function

End Class
