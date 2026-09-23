Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_prt_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function m4_Prt_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m4_prt_history(SELECT 0, prt.* FROM m4_prt prt WHERE prt.prtid = '" & idtransaksi & "')"
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
            sql = "SELECT prtidhistory FROM m4_prt_history WHERE prtid = '" & idtransaksi & "' ORDER BY prtmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m4_prt_detail_history (SELECT 0, '" & result(4) & "', prt.* FROM m4_prt_detail prt WHERE prt.idprt = '" & idtransaksi & "' )"
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
            sql = "INSERT INTO m1_no_batch_transaction_history(SELECT 0, '" & result(4) & "', nb.* FROM m1_no_batch_transaction nb WHERE nb.nbtidtransaksi = '" & idtransaksi & "' and nb.nbtsumber = 'PRT')"
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
            sql = "INSERT INTO m1_no_serial_transaction_history(SELECT 0, '" & result(4) & "', ns.* FROM m1_no_serial_transaction ns WHERE ns.nstidtransaksi = '" & idtransaksi & "' and ns.nstsumber = 'PRT')"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY SERIAL --------------------------------

            'PROSES INSERT HISTORY ASSET ---------------------------------------
            sql = "INSERT INTO m7_asset_transaction_history(SELECT 0, '" & result(4) & "', atr.* FROM m7_asset_transaction atr WHERE atr.atidutama = '" & idtransaksi & "' and atr.atsumber = 'PRT')"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY ASSET --------------------------------

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
    Public Function M4_Prt_HistoryBSearch(ByVal param As String) As String
        'M4_PrtBSearch --------------------------------------------------------
        'prtid, prtcabang, prtlokasi, prtgudang, prtasalbarang, prtasalbarangkategori, prtjenispembelian, 
        'prtjenispembeliankategori, prtcarabayar, prtsumber, prtautonotransaksi, prtnotransaksi, prttgl, prtkodepa, 
        'prtsupplier, prtsupplierkontak, prt1alamat1, prt1alamat2, prt1alamat3, prt2alamat1, prt2alamat2, 
        'prt2alamat3, prtbagianpembelian, prttermin, prttgljatuhtempo, prturaian, prtcatatan, prtnoref, 
        'prttglnoref, prttglpenutupan, prtmatauang, prtkurs, prthargatermasukpajak, prttotal, prtdiskonpersen, 
        'prtjmldiskon, prttotalpajak1detail, prttotalpajak2detail, prtbiayalainpersen, prtbiayalain, prttotaltransaksi, prtsisatransaksi, 
        'prtjmlbayar, prtstatuslunas, prttgllunas, prtnofakturpajak, prtsdhbayarpajak, prttglbayarpajak, prtrekdiskon, 
        'prtrekpajak1, prtrekpajak2, prtrekbiayalain, prtrekbayar, prtreksisa, prtidpr, prtidcs, 
        'prtidrq, prtidbs, prtidpo, prtidipc, prtidgrn, prtidri, prtiddnr, 
        'prtstatus, prtstatussebelumnya, prtjmlrevisi, prtcetakanke, prtinputuser, prtinputtgl, prtmodifikasiuser, 
        'prtmodifikasitgl, prtposting, prtpostingtgl, prttutupperiode, prtisclose, prtcabangnama, prtlokasinama, 
        'prtgudangnama, prtsupplierkode, prtsuppliernama, prtbagianpembeliankode, prtbagianpembeliannama, rinotransaksi, dnrnotransaksi, 
        'prtstatusnama, prtstatussebelumnyanama, prtinputusernama, prtmodifikasiusernama

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

        'CEK ID
        If Len(paramSplit(3)) > 0 Then
            If (paramSplit(3) <> 0) Then
                Filter = "prtidhistory = " + paramSplit(3)
            End If
        End If
        'END OF VALIDASI PARAMETER PAGING ==================================================

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
            Filter = Filter.Replace("prtsupplierkode", "c1.kkode")
            Filter = Filter.Replace("prtsuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select prt.prtidhistory, `prt`.`prtid` AS `prtid`,`prt`.`prtcabang` AS `prtcabang`,`prt`.`prtlokasi` AS `prtlokasi`,`prt`.`prtgudang` AS `prtgudang`,`prt`.`prtasalbarang` AS `prtasalbarang`,`prt`.`prtasalbarangkategori` AS `prtasalbarangkategori`,`prt`.`prtjenispembelian` AS `prtjenispembelian`,`prt`.`prtjenispembeliankategori` AS `prtjenispembeliankategori`,`prt`.`prtcarabayar` AS `prtcarabayar`,`prt`.`prtsumber` AS `prtsumber`,`prt`.`prtautonotransaksi` AS `prtautonotransaksi`,`prt`.`prtnotransaksi` AS `prtnotransaksi`,`prt`.`prttgl` AS `prttgl`,`prt`.`prtkodepa` AS `prtkodepa`,`prt`.`prtsupplier` AS `prtsupplier`,`prt`.`prtsupplierkontak` AS `prtsupplierkontak`,`prt`.`prt1alamat1` AS `prt1alamat1`,`prt`.`prt1alamat2` AS `prt1alamat2`,`prt`.`prt1alamat3` AS `prt1alamat3`,`prt`.`prt2alamat1` AS `prt2alamat1`,`prt`.`prt2alamat2` AS `prt2alamat2`,`prt`.`prt2alamat3` AS `prt2alamat3`,`prt`.`prtbagianpembelian` AS `prtbagianpembelian`,`prt`.`prttermin` AS `prttermin`,`prt`.`prttgljatuhtempo` AS `prttgljatuhtempo`,`prt`.`prturaian` AS `prturaian`,`prt`.`prtcatatan` AS `prtcatatan`,`prt`.`prtnoref` AS `prtnoref`,`prt`.`prttglnoref` AS `prttglnoref`,`prt`.`prttglpenutupan` AS `prttglpenutupan`,`prt`.`prtmatauang` AS `prtmatauang`,`prt`.`prtkurs` AS `prtkurs`,`prt`.`prthargatermasukpajak` AS `prthargatermasukpajak`,`prt`.`prttotal` AS `prttotal`,`prt`.`prtdiskonpersen` AS `prtdiskonpersen`,`prt`.`prtjmldiskon` AS `prtjmldiskon`,`prt`.`prttotalpajak1detail` AS `prttotalpajak1detail`,`prt`.`prttotalpajak2detail` AS `prttotalpajak2detail`,`prt`.`prtbiayalainpersen` AS `prtbiayalainpersen`,`prt`.`prtbiayalain` AS `prtbiayalain`,`prt`.`prttotaltransaksi` AS `prttotaltransaksi`,`prt`.`prtsisatransaksi` AS `prtsisatransaksi`,`prt`.`prtjmlbayar` AS `prtjmlbayar`,`prt`.`prtstatuslunas` AS `prtstatuslunas`,`prt`.`prttgllunas` AS `prttgllunas`,`prt`.`prtnofakturpajak` AS `prtnofakturpajak`,`prt`.`prtsdhbayarpajak` AS `prtsdhbayarpajak`,`prt`.`prttglbayarpajak` AS `prttglbayarpajak`,`prt`.`prtrekdiskon` AS `prtrekdiskon`,`prt`.`prtrekpajak1` AS `prtrekpajak1`,`prt`.`prtrekpajak2` AS `prtrekpajak2`,`prt`.`prtrekbiayalain` AS `prtrekbiayalain`,`prt`.`prtrekbayar` AS `prtrekbayar`,`prt`.`prtreksisa` AS `prtreksisa`,`prt`.`prtidpr` AS `prtidpr`,`prt`.`prtidcs` AS `prtidcs`,`prt`.`prtidrq` AS `prtidrq`,`prt`.`prtidbs` AS `prtidbs`,`prt`.`prtidpo` AS `prtidpo`,`prt`.`prtidipc` AS `prtidipc`,`prt`.`prtidgrn` AS `prtidgrn`,`prt`.`prtidri` AS `prtidri`,`prt`.`prtiddnr` AS `prtiddnr`,`prt`.`prtstatus` AS `prtstatus`,`prt`.`prtstatussebelumnya` AS `prtstatussebelumnya`,`prt`.`prtjmlrevisi` AS `prtjmlrevisi`,`prt`.`prtcetakanke` AS `prtcetakanke`,`prt`.`prtinputuser` AS `prtinputuser`,`prt`.`prtinputtgl` AS `prtinputtgl`,`prt`.`prtmodifikasiuser` AS `prtmodifikasiuser`,`prt`.`prtmodifikasitgl` AS `prtmodifikasitgl`,`prt`.`prtposting` AS `prtposting`,`prt`.`prtpostingtgl` AS `prtpostingtgl`,`prt`.`prttutupperiode` AS `prttutupperiode`,`prt`.`prtisclose` AS `prtisclose`,`br`.`bnama` AS `prtcabangnama`,`lc`.`lnama` AS `prtlokasinama`,`wh`.`wnama` AS `prtgudangnama`,`c1`.`kkode` AS `prtsupplierkode`,`c1`.`knama` AS `prtsuppliernama`,`c2`.`kkode` AS `prtbagianpembeliankode`,`c2`.`knama` AS `prtbagianpembeliannama`,`ri`.`rinotransaksi` AS `rinotransaksi`,`dnr`.`dnrnotransaksi` AS `dnrnotransaksi`,`st1`.`nama` AS `prtstatusnama`,`st2`.`nama` AS `prtstatussebelumnyanama`,`u1`.`unama` AS `prtinputusernama`,`u2`.`unama` AS `prtmodifikasiusernama`, `prt`.`prtcustomtext1` AS `prtcustomtext1`, `prt`.`prtcustomtext2` AS `prtcustomtext2`, `prt`.`prtcustomtext3` AS `prtcustomtext3`, `prt`.`prtcustomtext4` AS `prtcustomtext4`, `prt`.`prtcustomtext5` AS `prtcustomtext5`, `prt`.`prtcustomint1` AS `prtcustomint1`, `prt`.`prtcustomint2` AS `prtcustomint2`, `prt`.`prtcustomint3` AS `prtcustomint3`, `prt`.`prtcustomdbl1` AS `prtcustomdbl1`, `prt`.`prtcustomdbl2` AS `prtcustomdbl2`, `prt`.`prtcustomdbl3` AS `prtcustomdbl3`, `prt`.`prtcustomdate1` AS `prtcustomdate1`, `prt`.`prtcustomdate2` AS `prtcustomdate2`, `prt`.`prtcustomdate3` AS `prtcustomdate3`, cdis.cnama AS prtrekdiskonnama, cpa.cnama AS prtrekpajak1nama, cpa2.cnama AS prtrekpajak2nama, cba.cnama AS prtrekbiayalainnama from (((((((((((`m4_prt_history` `prt` left join `m1_branch` `br` on((`br`.`bkode` = `prt`.`prtcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `prt`.`prtlokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `prt`.`prtgudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `prt`.`prtsupplier`))) left join `m1_contact` `c2` on((`c2`.`kid` = `prt`.`prtbagianpembelian`))) left join `m4_ri` `ri` on((`prt`.`prtidri` = `ri`.`riid`))) left join `m4_dnr` `dnr` on((`prt`.`prtid` = `dnr`.`dnrid`))) left join `m0_status` `st1` on((`st1`.`kode` = `prt`.`prtstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `prt`.`prtstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `prt`.`prtinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `prt`.`prtmodifikasiuser`))) LEFT JOIN m1_coa cdis ON cdis.cnomor = prt.prtrekdiskon LEFT JOIN m1_coa cpa ON cpa.cnomor = prt.prtrekpajak1 LEFT JOIN m1_coa cpa2 ON cpa2.cnomor = prt.prtrekpajak2 LEFT JOIN m1_coa cba ON cba.cnomor = prt.prtrekbiayalain"
        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Prt", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("prtid"), 0), sptField,
                     FxDB(dr("prtidhistory"), 0), sptField,
                     FxDB(dr("prtcabang"), ""), sptField,
                     FxDB(dr("prtlokasi"), ""), sptField,
                     FxDB(dr("prtgudang"), ""), sptField,
                     FxDB(dr("prtasalbarang"), ""), sptField,
                     FxDB(dr("prtasalbarangkategori"), 0), sptField,
                     FxDB(dr("prtjenispembelian"), ""), sptField,
                     FxDB(dr("prtjenispembeliankategori"), 0), sptField,
                     FxDB(dr("prtcarabayar"), 0), sptField,
                     FxDB(dr("prtsumber"), ""), sptField,
                     FxDB(dr("prtautonotransaksi"), 0), sptField,
                     FxDB(dr("prtnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("prttgl"), ""), formatTgl), sptField,
                     FxDB(dr("prtkodepa"), 0), sptField,
                     FxDB(dr("prtsupplier"), 0), sptField,
                     FxDB(dr("prtsupplierkontak"), ""), sptField,
                     FxDB(dr("prt1alamat1"), ""), sptField,
                     FxDB(dr("prt1alamat2"), ""), sptField,
                     FxDB(dr("prt1alamat3"), ""), sptField,
                     FxDB(dr("prt2alamat1"), ""), sptField,
                     FxDB(dr("prt2alamat2"), ""), sptField,
                     FxDB(dr("prt2alamat3"), ""), sptField,
                     FxDB(dr("prtbagianpembelian"), 0), sptField,
                     FxDB(dr("prttermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("prttgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("prturaian"), ""), sptField,
                     FxDB(dr("prtcatatan"), ""), sptField,
                     FxDB(dr("prtnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("prttglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("prttglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("prtmatauang"), ""), sptField,
                     FxDB(dr("prtkurs"), 0), sptField,
                     FxDB(dr("prthargatermasukpajak"), 0), sptField,
                     FxDB(dr("prttotal"), 0), sptField,
                     FxDB(dr("prtdiskonpersen"), ""), sptField,
                     FxDB(dr("prtjmldiskon"), 0), sptField,
                     FxDB(dr("prttotalpajak1detail"), 0), sptField,
                     FxDB(dr("prttotalpajak2detail"), 0), sptField,
                     FxDB(dr("prtbiayalainpersen"), ""), sptField,
                     FxDB(dr("prtbiayalain"), 0), sptField,
                     FxDB(dr("prttotaltransaksi"), 0), sptField,
                     FxDB(dr("prtsisatransaksi"), 0), sptField,
                     FxDB(dr("prtjmlbayar"), 0), sptField,
                     FxDB(dr("prtstatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prttgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("prtnofakturpajak"), ""), sptField,
                     FxDB(dr("prtsdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prttglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(dr("prtrekdiskon"), ""), sptField,
                     FxDB(dr("prtrekpajak1"), ""), sptField,
                     FxDB(dr("prtrekpajak2"), ""), sptField,
                     FxDB(dr("prtrekbiayalain"), ""), sptField,
                     FxDB(dr("prtrekbayar"), ""), sptField,
                     FxDB(dr("prtreksisa"), ""), sptField,
                     FxDB(dr("prtidpr"), 0), sptField,
                     FxDB(dr("prtidcs"), 0), sptField,
                     FxDB(dr("prtidrq"), 0), sptField,
                     FxDB(dr("prtidbs"), 0), sptField,
                     FxDB(dr("prtidpo"), 0), sptField,
                     FxDB(dr("prtidipc"), 0), sptField,
                     FxDB(dr("prtidgrn"), 0), sptField,
                     FxDB(dr("prtidri"), 0), sptField,
                     FxDB(dr("prtiddnr"), 0), sptField,
                     FxDB(dr("prtstatus"), 0), sptField,
                     FxDB(dr("prtstatussebelumnya"), 0), sptField,
                     FxDB(dr("prtjmlrevisi"), 0), sptField,
                     FxDB(dr("prtcetakanke"), 0), sptField,
                     FxDB(dr("prtinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prtinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("prtmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prtmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("prtposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prtpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("prttutupperiode"), 0), sptField,
                     FxDB(dr("prtisclose"), 0), sptField,
                     FxDB(dr("prtcabangnama"), ""), sptField,
                     FxDB(dr("prtlokasinama"), ""), sptField,
                     FxDB(dr("prtgudangnama"), ""), sptField,
                     FxDB(dr("prtsupplierkode"), ""), sptField,
                     FxDB(dr("prtsuppliernama"), ""), sptField,
                     FxDB(dr("prtbagianpembeliankode"), ""), sptField,
                     FxDB(dr("prtbagianpembeliannama"), ""), sptField,
                     FxDB(dr("rinotransaksi"), ""), sptField,
                     FxDB(dr("dnrnotransaksi"), ""), sptField,
                     FxDB(dr("prtstatusnama"), ""), sptField,
                     FxDB(dr("prtstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("prtinputusernama"), ""), sptField,
                     FxDB(dr("prtmodifikasiusernama"), ""), sptField,
                     FxDB(dr("prtcustomtext1"), ""), sptField,
                     FxDB(dr("prtcustomtext2"), ""), sptField,
                     FxDB(dr("prtcustomtext3"), ""), sptField,
                     FxDB(dr("prtcustomtext4"), ""), sptField,
                     FxDB(dr("prtcustomtext5"), ""), sptField,
                     FxDB(dr("prtcustomint1"), 0), sptField,
                     FxDB(dr("prtcustomint2"), 0), sptField,
                     FxDB(dr("prtcustomint3"), 0), sptField,
                     FxDB(dr("prtcustomdbl1"), 0), sptField,
                     FxDB(dr("prtcustomdbl2"), 0), sptField,
                     FxDB(dr("prtcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prtcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("prtcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("prtcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("prtrekdiskonnama"), ""), sptField,
                     FxDB(dr("prtrekpajak1nama"), ""), sptField,
                     FxDB(dr("prtrekpajak2nama"), ""), sptField,
                     FxDB(dr("prtrekbiayalainnama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("prtidhistory, prtid, prtcabang, prtlokasi, prtgudang, prtasalbarang, prtasalbarangkategori, prtjenispembelian, prtjenispembeliankategori, prtcarabayar, prtsumber, prtautonotransaksi, prtnotransaksi, prttgl, prtkodepa, prtsupplier, prtsupplierkontak, prt1alamat1, prt1alamat2, prt1alamat3, prt2alamat1, prt2alamat2, prt2alamat3, prtbagianpembelian, prttermin, prttgljatuhtempo, prturaian, prtcatatan, prtnoref, prttglnoref, prttglpenutupan, prtmatauang, prtkurs, prthargatermasukpajak, prttotal, prtdiskonpersen, prtjmldiskon, prttotalpajak1detail, prttotalpajak2detail, prtbiayalainpersen, prtbiayalain, prttotaltransaksi, prtsisatransaksi, prtjmlbayar, prtstatuslunas, prttgllunas, prtnofakturpajak, prtsdhbayarpajak, prttglbayarpajak, prtrekdiskon, prtrekpajak1, prtrekpajak2, prtrekbiayalain, prtrekbayar, prtreksisa, prtidpr, prtidcs, prtidrq, prtidbs, prtidpo, prtidipc, prtidgrn, prtidri, prtiddnr, prtstatus, prtstatussebelumnya, prtjmlrevisi, prtcetakanke, prtinputuser, prtinputtgl, prtmodifikasiuser, prtmodifikasitgl, prtposting, prtpostingtgl, prttutupperiode, prtisclose, prtcabangnama, prtlokasinama, prtgudangnama, prtsupplierkode, prtsuppliernama, prtbagianpembeliankode, prtbagianpembeliannama, rinotransaksi, dnrnotransaksi, prtstatusnama, prtstatussebelumnyanama, prtinputusernama, prtmodifikasiusernama, prtcustomtext1, prtcustomtext2, prtcustomtext3, prtcustomtext4, prtcustomtext5, prtcustomint1, prtcustomint2, prtcustomint3, prtcustomdbl1, prtcustomdbl2, prtcustomdbl3, prtcustomdate1, prtcustomdate2, prtcustomdate3, prtrekdiskonnama, prtrekpajak1nama, prtrekpajak2nama, prtrekbiayalainnama"))

        Return wsResult
    End Function


    <WebMethod()>
    Public Function M4_Prt_HistorySearch(ByVal param As String) As String
        'M4_Prt_HistorySearch --------------------------------------------------------
        'prtidhistory, prtid, prtcabang, prtlokasi, prtgudang, prtasalbarang, prtasalbarangkategori, prtjenispembelian, 
        'prtjenispembeliankategori, prtcarabayar, prtsumber, prtautonotransaksi, prtnotransaksi, prttgl, prtkodepa, 
        'prtsupplier, prtsupplierkontak, prt1alamat1, prt1alamat2, prt1alamat3, prt2alamat1, prt2alamat2, 
        'prt2alamat3, prtbagianpembelian, prttermin, prttgljatuhtempo, prturaian, prtcatatan, prtnoref, 
        'prttglnoref, prttglpenutupan, prtmatauang, prtkurs, prthargatermasukpajak, prttotal, prtdiskonpersen, 
        'prtjmldiskon, prttotalpajak1detail, prttotalpajak2detail, prtbiayalainpersen, prtbiayalain, prttotaltransaksi, prtsisatransaksi, 
        'prtjmlbayar, prtstatuslunas, prttgllunas, prtnofakturpajak, prtsdhbayarpajak, prttglbayarpajak, prtrekdiskon, 
        'prtrekpajak1, prtrekpajak2, prtrekbiayalain, prtrekbayar, prtreksisa, prtidpr, prtidcs, 
        'prtidrq, prtidbs, prtidpo, prtidipc, prtidgrn, prtidri, prtiddnr, 
        'prtstatus, prtstatussebelumnya, prtjmlrevisi, prtcetakanke, prtinputuser, prtinputtgl, prtmodifikasiuser, 
        'prtmodifikasitgl, prtposting, prtpostingtgl, prttutupperiode, prtisclose, prtcabangnama, prtlokasinama, 
        'prtgudangnama, prtsupplierkode, prtsuppliernama, prtbagianpembeliankode, prtbagianpembeliannama, rinotransaksi, dnrnotransaksi, 
        'prtstatusnama, prtstatussebelumnyanama, prtinputusernama, prtmodifikasiusernama, prtcustomtext1, prtcustomtext2, prtcustomtext3, prtcustomtext4, prtcustomtext5,
        'prtcustomint1, prtcustomint2, prtcustomint3, prtcustomdbl1, prtcustomdbl2, prtcustomdbl3, prtcustomdate1, prtcustomdate2, prtcustomdate3, prtjenis, prtjenisnama

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
            Filter = Filter.Replace("prtsupplierkode", "c1.kkode")
            Filter = Filter.Replace("prtsuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_prt_v_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Prt", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("prtid"), 0), sptField,
                     FxDB(dr("prtidhistory"), 0), sptField,
                     FxDB(dr("prtcabang"), ""), sptField,
                     FxDB(dr("prtlokasi"), ""), sptField,
                     FxDB(dr("prtgudang"), ""), sptField,
                     FxDB(dr("prtasalbarang"), ""), sptField,
                     FxDB(dr("prtasalbarangkategori"), 0), sptField,
                     FxDB(dr("prtjenispembelian"), ""), sptField,
                     FxDB(dr("prtjenispembeliankategori"), 0), sptField,
                     FxDB(dr("prtcarabayar"), 0), sptField,
                     FxDB(dr("prtsumber"), ""), sptField,
                     FxDB(dr("prtautonotransaksi"), 0), sptField,
                     FxDB(dr("prtnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("prttgl"), ""), formatTgl), sptField,
                     FxDB(dr("prtkodepa"), 0), sptField,
                     FxDB(dr("prtsupplier"), 0), sptField,
                     FxDB(dr("prtsupplierkontak"), ""), sptField,
                     FxDB(dr("prt1alamat1"), ""), sptField,
                     FxDB(dr("prt1alamat2"), ""), sptField,
                     FxDB(dr("prt1alamat3"), ""), sptField,
                     FxDB(dr("prt2alamat1"), ""), sptField,
                     FxDB(dr("prt2alamat2"), ""), sptField,
                     FxDB(dr("prt2alamat3"), ""), sptField,
                     FxDB(dr("prtbagianpembelian"), 0), sptField,
                     FxDB(dr("prttermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("prttgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("prturaian"), ""), sptField,
                     FxDB(dr("prtcatatan"), ""), sptField,
                     FxDB(dr("prtnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("prttglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("prttglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("prtmatauang"), ""), sptField,
                     FxDB(dr("prtkurs"), 0), sptField,
                     FxDB(dr("prthargatermasukpajak"), 0), sptField,
                     FxDB(dr("prttotal"), 0), sptField,
                     FxDB(dr("prtdiskonpersen"), ""), sptField,
                     FxDB(dr("prtjmldiskon"), 0), sptField,
                     FxDB(dr("prttotalpajak1detail"), 0), sptField,
                     FxDB(dr("prttotalpajak2detail"), 0), sptField,
                     FxDB(dr("prtbiayalainpersen"), ""), sptField,
                     FxDB(dr("prtbiayalain"), 0), sptField,
                     FxDB(dr("prttotaltransaksi"), 0), sptField,
                     FxDB(dr("prtsisatransaksi"), 0), sptField,
                     FxDB(dr("prtjmlbayar"), 0), sptField,
                     FxDB(dr("prtstatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prttgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("prtnofakturpajak"), ""), sptField,
                     FxDB(dr("prtsdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prttglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(dr("prtrekdiskon"), ""), sptField,
                     FxDB(dr("prtrekpajak1"), ""), sptField,
                     FxDB(dr("prtrekpajak2"), ""), sptField,
                     FxDB(dr("prtrekbiayalain"), ""), sptField,
                     FxDB(dr("prtrekbayar"), ""), sptField,
                     FxDB(dr("prtreksisa"), ""), sptField,
                     FxDB(dr("prtidpr"), 0), sptField,
                     FxDB(dr("prtidcs"), 0), sptField,
                     FxDB(dr("prtidrq"), 0), sptField,
                     FxDB(dr("prtidbs"), 0), sptField,
                     FxDB(dr("prtidpo"), 0), sptField,
                     FxDB(dr("prtidipc"), 0), sptField,
                     FxDB(dr("prtidgrn"), 0), sptField,
                     FxDB(dr("prtidri"), 0), sptField,
                     FxDB(dr("prtiddnr"), 0), sptField,
                     FxDB(dr("prtstatus"), 0), sptField,
                     FxDB(dr("prtstatussebelumnya"), 0), sptField,
                     FxDB(dr("prtjmlrevisi"), 0), sptField,
                     FxDB(dr("prtcetakanke"), 0), sptField,
                     FxDB(dr("prtinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prtinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("prtmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prtmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("prtposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prtpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("prttutupperiode"), 0), sptField,
                     FxDB(dr("prtisclose"), 0), sptField,
                     FxDB(dr("prtcabangnama"), ""), sptField,
                     FxDB(dr("prtlokasinama"), ""), sptField,
                     FxDB(dr("prtgudangnama"), ""), sptField,
                     FxDB(dr("prtsupplierkode"), ""), sptField,
                     FxDB(dr("prtsuppliernama"), ""), sptField,
                     FxDB(dr("prtbagianpembeliankode"), ""), sptField,
                     FxDB(dr("prtbagianpembeliannama"), ""), sptField,
                     FxDB(dr("rinotransaksi"), ""), sptField,
                     FxDB(dr("dnrnotransaksi"), ""), sptField,
                     FxDB(dr("prtstatusnama"), ""), sptField,
                     FxDB(dr("prtstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("prtinputusernama"), ""), sptField,
                     FxDB(dr("prtmodifikasiusernama"), ""), sptField,
                     FxDB(dr("prtcustomtext1"), ""), sptField,
                     FxDB(dr("prtcustomtext2"), ""), sptField,
                     FxDB(dr("prtcustomtext3"), ""), sptField,
                     FxDB(dr("prtcustomtext4"), ""), sptField,
                     FxDB(dr("prtcustomtext5"), ""), sptField,
                     FxDB(dr("prtcustomint1"), 0), sptField,
                     FxDB(dr("prtcustomint2"), 0), sptField,
                     FxDB(dr("prtcustomint3"), 0), sptField,
                     FxDB(dr("prtcustomdbl1"), 0), sptField,
                     FxDB(dr("prtcustomdbl2"), 0), sptField,
                     FxDB(dr("prtcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prtcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("prtcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("prtcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("prtjenis"), 0), sptField,
                     FxDB(dr("prtjenisnama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("prtidhistory, prtid, prtcabang, prtlokasi, prtgudang, prtasalbarang, prtasalbarangkategori, prtjenispembelian, prtjenispembeliankategori, prtcarabayar, prtsumber, prtautonotransaksi, prtnotransaksi, prttgl, prtkodepa, prtsupplier, prtsupplierkontak, prt1alamat1, prt1alamat2, prt1alamat3, prt2alamat1, prt2alamat2, prt2alamat3, prtbagianpembelian, prttermin, prttgljatuhtempo, prturaian, prtcatatan, prtnoref, prttglnoref, prttglpenutupan, prtmatauang, prtkurs, prthargatermasukpajak, prttotal, prtdiskonpersen, prtjmldiskon, prttotalpajak1detail, prttotalpajak2detail, prtbiayalainpersen, prtbiayalain, prttotaltransaksi, prtsisatransaksi, prtjmlbayar, prtstatuslunas, prttgllunas, prtnofakturpajak, prtsdhbayarpajak, prttglbayarpajak, prtrekdiskon, prtrekpajak1, prtrekpajak2, prtrekbiayalain, prtrekbayar, prtreksisa, prtidpr, prtidcs, prtidrq, prtidbs, prtidpo, prtidipc, prtidgrn, prtidri, prtiddnr, prtstatus, prtstatussebelumnya, prtjmlrevisi, prtcetakanke, prtinputuser, prtinputtgl, prtmodifikasiuser, prtmodifikasitgl, prtposting, prtpostingtgl, prttutupperiode, prtisclose, prtcabangnama, prtlokasinama, prtgudangnama, prtsupplierkode, prtsuppliernama, prtbagianpembeliankode, prtbagianpembeliannama, rinotransaksi, dnrnotransaksi, prtstatusnama, prtstatussebelumnyanama, prtinputusernama, prtmodifikasiusernama, prtcustomtext1, prtcustomtext2, prtcustomtext3, prtcustomtext4, prtcustomtext5, prtcustomint1, prtcustomint2, prtcustomint3, prtcustomdbl1, prtcustomdbl2, prtcustomdbl3, prtcustomdate1, prtcustomdate2, prtcustomdate3, prtjenis, prtjenisnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_PrtHistoryGetdataById(ByVal param As String) As String

        'M4_PrtGetdataById Utama --------------------------------------------------------
        'prtid, prtcabang, prtlokasi, prtgudang, prtasalbarang, prtasalbarangkategori, prtjenispembelian, 
        'prtjenispembeliankategori, prtcarabayar, prtsumber, prtautonotransaksi, prtnotransaksi, prttgl, prtkodepa, 
        'prtsupplier, prtsupplierkontak, prt1alamat1, prt1alamat2, prt1alamat3, prt2alamat1, prt2alamat2, 
        'prt2alamat3, prtbagianpembelian, prttermin, prttgljatuhtempo, prturaian, prtcatatan, prtnoref, 
        'prttglnoref, prttglpenutupan, prtmatauang, prtkurs, prthargatermasukpajak, prttotal, prtdiskonpersen, 
        'prtjmldiskon, prttotalpajak1detail, prttotalpajak2detail, prtbiayalainpersen, prtbiayalain, prttotaltransaksi, prtsisatransaksi, 
        'prtjmlbayar, prtstatuslunas, prttgllunas, prtnofakturpajak, prtsdhbayarpajak, prttglbayarpajak, prtrekdiskon, 
        'prtrekpajak1, prtrekpajak2, prtrekbiayalain, prtrekbayar, prtreksisa, prtidpr, prtidcs, 
        'prtidrq, prtidbs, prtidpo, prtidipc, prtidgrn, prtidri, prtiddnr, 
        'prtstatus, prtstatussebelumnya, prtjmlrevisi, prtcetakanke, prtinputuser, prtinputtgl, prtmodifikasiuser, 
        'prtmodifikasitgl, prtposting, prtpostingtgl, prttutupperiode, prtisclose, prtcustomtext1, prtcustomtext2, 
        'prtcustomtext3, prtcustomtext4, prtcustomtext5, prtcustomint1, prtcustomint2, prtcustomint3, prtcustomdbl1, 
        'prtcustomdbl2, prtcustomdbl3, prtcustomdate1, prtcustomdate2, prtcustomdate3, prtcabangnama, prtlokasinama, 
        'prtgudangnama, prtsupplierkode, prtsuppliernama, prtbagianpembeliankode, prtbagianpembeliannama, prtterminnama, prtterminharijatuhtempo, 
        'prtrekdiskonnama, prtrekpajak1nama, prtrekpajak2nama, prtrekbiayalainnama, prtrekbayarnama, prtreksisanama, prtnotransaksiri, 
        'prtnotransaksidnr, prtstatusnama, prtstatussebelumnyanama, prtinputusernama, prtmodifikasiusernama, prtjenis, kpkp

        'M4_PrtGetdataById Detail -------------------------------------------------------
        'idprtdetail, idprt, 
        'idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, 
        'satuanbarang, matauang, kurs, hargafix, idhppkhususmasuk, idhppfifomasuk, hpp, 
        'harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, 
        'cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekdiskonpembelian, 
        'rekhargapokok, rekreturpembelian, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, 
        'idgrndetail, idridetail, iddnrdetail, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, 
        'bhpp, bjenis, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, 
        'pajak2nilai, cabangnama, lokasinama, gudangasalnama, gudangtransitnama, gudangtujuannama, costcenternama, 
        'divisinama, subdivisinama, proyeknama, rinotransaksi, dnrnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan

        'M4_PrtGetdataById Batch --------------------------------------------------------
        'nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang

        'M4_PrtGetdataById Serial --------------------------------------------------------
        'nstid, nstjenismutasi, nstidserialin, nstgudang,  nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, 
        'nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, 
        'nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang

        'M4_PrtGetdataById Asset --------------------------------------------------------
        'atid, atasetid, atjenismutasi, atsumber, atidutama, atidbarang, atkode, 
        'atnama, atkategori, atcabang, atlokasi, atgudang, atdivisi, atsubdivisi, 
        'atcostcenter, atproyek, atcatatan, atnomor, attglbeli, attglpakai, atjml, 
        'atsatuan, atmatauang, atkurs, atharga, atdiskon, atjmldiskon, atpajak1, 
        'atjmlpajak1, atpajak2, atjmlpajak2, athargabeli, atnilairesidu, atumurekonomis, atbebanperbln, 
        'atakumulasibeban, atnilaibuku, atnilaipenyusutan, atmetode, attabelpenyusutan, atintangible, atfiskal, 
        'atatastengahbulan, atrekasset, atrekakumdepresiasi, atrekdepresiasi, atrekpenghapusan, atprodusen, attglpensiun, 
        'atpenyusutanke, atnilaimenurun, atdispose, atpembelian, atpenjualan, atlocked, atstatus, 
        'atstatussebelumnya, atisclose, atinputuser, atinputtgl, atmodifikasiuser, atmodifikasitgl, atcustomtext1, 
        'atcustomtext2, atcustomtext3, atcustomtext4, atcustomtext5, atcustomint1, atcustomint2, atcustomint3, 
        'atcustomint4, atcustomint5, atcustomdbl1, atcustomdbl2, atcustomdbl3, atcustomdbl4, atcustomdbl5, 
        'atcustomdate1, atcustomdate2, atcustomdate3, atcustomdate4, atcustomdate5, atkategorinama, atcabangnama, 
        'atlokasinama, atgudangnama, atdivisinama, atsubdivisinama, atcostcenternama, atproyeknama, atmetodenama, 
        'atpajak1nama, atpajak1nilai, atpajak2nama, atpajak2nilai, atrekassetnama, atrekakumdepresiasinama, atrekdepresiasinama, 
        'atrekpenghapusannama, atprodusenkode, atprodusennama, atstatusnama, atstatussebelumnyanama, atinputusernama, atmodifikasiusernama

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
        Dim sumber As String = "PRT", asset As String = ""

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

        Dim NmMemcached As String = "aplikasi1-m4_prt_history~m4_prt_detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "prtidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "prtidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m4_prt_getdata")
        sql = "select `prt`.`prtid` AS `prtid`,`prt`.`prtcabang` AS `prtcabang`,`prt`.`prtlokasi` AS `prtlokasi`,`prt`.`prtgudang` AS `prtgudang`,`prt`.`prtasalbarang` AS `prtasalbarang`,`prt`.`prtasalbarangkategori` AS `prtasalbarangkategori`,`prt`.`prtjenispembelian` AS `prtjenispembelian`,`prt`.`prtjenispembeliankategori` AS `prtjenispembeliankategori`,`prt`.`prtcarabayar` AS `prtcarabayar`,`prt`.`prtsumber` AS `prtsumber`,`prt`.`prtautonotransaksi` AS `prtautonotransaksi`,`prt`.`prtnotransaksi` AS `prtnotransaksi`,`prt`.`prttgl` AS `prttgl`,`prt`.`prtkodepa` AS `prtkodepa`,`prt`.`prtsupplier` AS `prtsupplier`,`prt`.`prtsupplierkontak` AS `prtsupplierkontak`,`prt`.`prt1alamat1` AS `prt1alamat1`,`prt`.`prt1alamat2` AS `prt1alamat2`,`prt`.`prt1alamat3` AS `prt1alamat3`,`prt`.`prt2alamat1` AS `prt2alamat1`,`prt`.`prt2alamat2` AS `prt2alamat2`,`prt`.`prt2alamat3` AS `prt2alamat3`,`prt`.`prtbagianpembelian` AS `prtbagianpembelian`,`prt`.`prttermin` AS `prttermin`,`prt`.`prttgljatuhtempo` AS `prttgljatuhtempo`,`prt`.`prturaian` AS `prturaian`,`prt`.`prtcatatan` AS `prtcatatan`,`prt`.`prtnoref` AS `prtnoref`,`prt`.`prttglnoref` AS `prttglnoref`,`prt`.`prttglpenutupan` AS `prttglpenutupan`,`prt`.`prtmatauang` AS `prtmatauang`,`prt`.`prtkurs` AS `prtkurs`,`prt`.`prthargatermasukpajak` AS `prthargatermasukpajak`,`prt`.`prttotal` AS `prttotal`,`prt`.`prtdiskonpersen` AS `prtdiskonpersen`,`prt`.`prtjmldiskon` AS `prtjmldiskon`,`prt`.`prttotalpajak1detail` AS `prttotalpajak1detail`,`prt`.`prttotalpajak2detail` AS `prttotalpajak2detail`,`prt`.`prtbiayalainpersen` AS `prtbiayalainpersen`,`prt`.`prtbiayalain` AS `prtbiayalain`,`prt`.`prttotaltransaksi` AS `prttotaltransaksi`,`prt`.`prtsisatransaksi` AS `prtsisatransaksi`,`prt`.`prtjmlbayar` AS `prtjmlbayar`,`prt`.`prtstatuslunas` AS `prtstatuslunas`,`prt`.`prttgllunas` AS `prttgllunas`,`prt`.`prtnofakturpajak` AS `prtnofakturpajak`,`prt`.`prtsdhbayarpajak` AS `prtsdhbayarpajak`,`prt`.`prttglbayarpajak` AS `prttglbayarpajak`,`prt`.`prtrekdiskon` AS `prtrekdiskon`,`prt`.`prtrekpajak1` AS `prtrekpajak1`,`prt`.`prtrekpajak2` AS `prtrekpajak2`,`prt`.`prtrekbiayalain` AS `prtrekbiayalain`,`prt`.`prtrekbayar` AS `prtrekbayar`,`prt`.`prtreksisa` AS `prtreksisa`,`prt`.`prtidpr` AS `prtidpr`,`prt`.`prtidcs` AS `prtidcs`,`prt`.`prtidrq` AS `prtidrq`,`prt`.`prtidbs` AS `prtidbs`,`prt`.`prtidpo` AS `prtidpo`,`prt`.`prtidipc` AS `prtidipc`,`prt`.`prtidgrn` AS `prtidgrn`,`prt`.`prtidri` AS `prtidri`,`prt`.`prtiddnr` AS `prtiddnr`,`prt`.`prtstatus` AS `prtstatus`,`prt`.`prtstatussebelumnya` AS `prtstatussebelumnya`,`prt`.`prtjmlrevisi` AS `prtjmlrevisi`,`prt`.`prtcetakanke` AS `prtcetakanke`,`prt`.`prtinputuser` AS `prtinputuser`,`prt`.`prtinputtgl` AS `prtinputtgl`,`prt`.`prtmodifikasiuser` AS `prtmodifikasiuser`,`prt`.`prtmodifikasitgl` AS `prtmodifikasitgl`,`prt`.`prtposting` AS `prtposting`,`prt`.`prtpostingtgl` AS `prtpostingtgl`,`prt`.`prttutupperiode` AS `prttutupperiode`,`prt`.`prtisclose` AS `prtisclose`,`prt`.`prtcustomtext1` AS `prtcustomtext1`,`prt`.`prtcustomtext2` AS `prtcustomtext2`,`prt`.`prtcustomtext3` AS `prtcustomtext3`,`prt`.`prtcustomtext4` AS `prtcustomtext4`,`prt`.`prtcustomtext5` AS `prtcustomtext5`,`prt`.`prtcustomint1` AS `prtcustomint1`,`prt`.`prtcustomint2` AS `prtcustomint2`,`prt`.`prtcustomint3` AS `prtcustomint3`,`prt`.`prtcustomdbl1` AS `prtcustomdbl1`,`prt`.`prtcustomdbl2` AS `prtcustomdbl2`,`prt`.`prtcustomdbl3` AS `prtcustomdbl3`,`prt`.`prtcustomdate1` AS `prtcustomdate1`,`prt`.`prtcustomdate2` AS `prtcustomdate2`,`prt`.`prtcustomdate3` AS `prtcustomdate3`,`br`.`bnama` AS `prtcabangnama`,`lc`.`lnama` AS `prtlokasinama`,`wh`.`wnama` AS `prtgudangnama`,`c1`.`kkode` AS `prtsupplierkode`,`c1`.`knama` AS `prtsuppliernama`,`c2`.`kkode` AS `prtbagianpembeliankode`,`c2`.`knama` AS `prtbagianpembeliannama`,`tr`.`trnama` AS `prtterminnama`,`tr`.`trharijatuhtempo` AS `prtterminharijatuhtempo`,`coa1`.`cnama` AS `prtrekdiskonnama`,`coa2`.`cnama` AS `prtrekpajak1nama`,`coa3`.`cnama` AS `prtrekpajak2nama`,`coa4`.`cnama` AS `prtrekbiayalainnama`,`coa5`.`cnama` AS `prtrekbayarnama`,`coa6`.`cnama` AS `prtreksisanama`,`ri`.`rinotransaksi` AS `prtnotransaksiri`,`dnr`.`dnrnotransaksi` AS `prtnotransaksidnr`,`st1`.`nama` AS `prtstatusnama`,`st2`.`nama` AS `prtstatussebelumnyanama`,`u1`.`unama` AS `prtinputusernama`,`u2`.`unama` AS `prtmodifikasiusernama`, prt.prtjenis, `prtd`.`idprtdetail` AS `idprtdetail`,`prtd`.`idprt` AS `idprt`,`prtd`.`idbarang` AS `idbarang`,`prtd`.`namabarang` AS `namabarang`,`prtd`.`tipebarang` AS `tipebarang`,`prtd`.`jml` AS `jml`,`prtd`.`satuan` AS `satuan`,`prtd`.`nilaisatuan` AS `nilaisatuan`,`prtd`.`jmlbarang` AS `jmlbarang`,`prtd`.`satuanbarang` AS `satuanbarang`,`prtd`.`matauang` AS `matauang`,`prtd`.`kurs` AS `kurs`,`prtd`.`hargafix` AS `hargafix`,`prtd`.`idhppkhususmasuk` AS `idhppkhususmasuk`,`prtd`.`idhppfifomasuk` AS `idhppfifomasuk`,`prtd`.`hpp` AS `hpp`,`prtd`.`harga` AS `harga`,`prtd`.`diskon` AS `diskon`,`prtd`.`jmldiskon` AS `jmldiskon`,`prtd`.`pajak1` AS `pajak1`,`prtd`.`jmlpajak1` AS `jmlpajak1`,`prtd`.`pajak2` AS `pajak2`,`prtd`.`jmlpajak2` AS `jmlpajak2`,`prtd`.`cabang` AS `cabang`,`prtd`.`lokasi` AS `lokasi`,`prtd`.`gudangasal` AS `gudangasal`,`prtd`.`gudangtransit` AS `gudangtransit`,`prtd`.`gudangtujuan` AS `gudangtujuan`,`i`.`brekpersediaan` AS `rekpersediaan`,`i`.`brekdiskonpembelian` AS `rekdiskonpembelian`,`i`.`brekhargapokok` AS `rekhargapokok`,`i`.`brekreturpembelian` AS `rekreturpembelian`,`prtd`.`costcenter` AS `costcenter`,`prtd`.`divisi` AS `divisi`,`prtd`.`subdivisi` AS `subdivisi`,`prtd`.`proyek` AS `proyek`,`prtd`.`catatan` AS `catatan`,`prtd`.`urutan` AS `urutan`,`prtd`.`idprdetail` AS `idprdetail`,`prtd`.`idcsdetail` AS `idcsdetail`,`prtd`.`idrqdetail` AS `idrqdetail`,`prtd`.`idbsdetail` AS `idbsdetail`,`prtd`.`idpodetail` AS `idpodetail`,`prtd`.`idipcdetail` AS `idipcdetail`,`prtd`.`idgrndetail` AS `idgrndetail`,`prtd`.`idridetail` AS `idridetail`,`prtd`.`iddnrdetail` AS `iddnrdetail`,`prtd`.`isclose` AS `isclose`,`prtd`.`customtext1` AS `customtext1`,`prtd`.`customtext2` AS `customtext2`,`prtd`.`customtext3` AS `customtext3`,`prtd`.`customdbl1` AS `customdbl1`,`prtd`.`customdbl2` AS `customdbl2`,`prtd`.`customdbl3` AS `customdbl3`,`prtd`.`customdate1` AS `customdate1`,`prtd`.`customdate2` AS `customdate2`,`prtd`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bjenis` AS `bjenis`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`i`.`basset` AS `basset`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,`brd`.`bnama` AS `cabangnama`,`lcd`.`lnama` AS `lokasinama`,`whd1`.`wnama` AS `gudangasalnama`,`whd2`.`wnama` AS `gudangtransitnama`,`whd3`.`wnama` AS `gudangtujuannama`,`cc`.`ccnama` AS `costcenternama`,`d`.`dnama` AS `divisinama`,`sd`.`sdnama` AS `subdivisinama`,`p`.`pnama` AS `proyeknama`,`ri2`.`rinotransaksi` AS `rinotransaksi`,`dnr2`.`dnrnotransaksi` AS `dnrnotransaksi`, c1.kpkp, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan from (((((((((((((((((((((((((((((((((((`m4_prt_history` `prt` join `m4_prt_detail_history` `prtd` on((`prt`.`prtid` = `prtd`.`idprt`))) left join `m1_branch` `br` on((`br`.`bkode` = `prt`.`prtcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `prt`.`prtlokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `prt`.`prtgudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `prt`.`prtsupplier`))) left join `m1_contact` `c2` on((`c2`.`kid` = `prt`.`prtbagianpembelian`))) left join `m1_terms` `tr` on((`prt`.`prttermin` = `tr`.`trkode`))) left join `m1_coa` `coa1` on((`prt`.`prtrekdiskon` = `coa1`.`cnomor`))) left join `m1_coa` `coa2` on((`prt`.`prtrekpajak1` = `coa2`.`cnomor`))) left join `m1_coa` `coa3` on((`prt`.`prtrekpajak2` = `coa3`.`cnomor`))) left join `m1_coa` `coa4` on((`prt`.`prtrekbiayalain` = `coa4`.`cnomor`))) left join `m1_coa` `coa5` on((`prt`.`prtrekbayar` = `coa5`.`cnomor`))) left join `m1_coa` `coa6` on((`prt`.`prtreksisa` = `coa6`.`cnomor`))) left join `m4_ri` `ri` on((`prt`.`prtidri` = `ri`.`riid`))) left join `m4_dnr` `dnr` on((`prt`.`prtiddnr` = `dnr`.`dnrid`))) left join `m0_status` `st1` on((`st1`.`kode` = `prt`.`prtstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `prt`.`prtstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `prt`.`prtinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `prt`.`prtmodifikasiuser`))) left join `m1_item` `i` on((`i`.`bid` = `prtd`.`idbarang`))) left join `m1_tax` `t1` on((`prtd`.`pajak1` = `t1`.`tkode`))) left join `m1_tax` `t2` on((`prtd`.`pajak2` = `t2`.`tkode`))) left join `m4_dnr_detail` `dnrd` on((`prtd`.`iddnrdetail` = `dnrd`.`iddnrdetail`))) left join `m4_dnr` `dnr2` on((`dnrd`.`iddnr` = `dnr2`.`dnrid`))) left join `m1_branch` `brd` on((`prtd`.`cabang` = `brd`.`bkode`))) left join `m1_location` `lcd` on((`prtd`.`lokasi` = `lcd`.`lkode`))) left join `m1_warehouse` `whd1` on((`prtd`.`gudangasal` = `whd1`.`wkode`))) left join `m1_warehouse` `whd2` on((`prtd`.`gudangtransit` = `whd2`.`wkode`))) left join `m1_warehouse` `whd3` on((`prtd`.`gudangtujuan` = `whd3`.`wkode`))) left join `m1_cost_center` `cc` on((`prtd`.`costcenter` = `cc`.`cckode`))) left join `m1_division` `d` on((`prtd`.`divisi` = `d`.`dkode`))) left join `m1_subdivision` `sd` on((`prtd`.`subdivisi` = `sd`.`sdkode`))) left join `m1_project` `p` on((`prtd`.`proyek` = `p`.`pkode`))) left join `m4_ri_detail` `rid` on((`prtd`.`idridetail` = `rid`.`idridetail`))) left join `m4_ri` `ri2` on((`rid`.`idri` = `ri2`.`riid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("prtid"), 0), sptField,
                     FxDB(drutama("prtcabang"), ""), sptField,
                     FxDB(drutama("prtlokasi"), ""), sptField,
                     FxDB(drutama("prtgudang"), ""), sptField,
                     FxDB(drutama("prtasalbarang"), ""), sptField,
                     FxDB(drutama("prtasalbarangkategori"), 0), sptField,
                     FxDB(drutama("prtjenispembelian"), ""), sptField,
                     FxDB(drutama("prtjenispembeliankategori"), 0), sptField,
                     FxDB(drutama("prtcarabayar"), 0), sptField,
                     FxDB(drutama("prtsumber"), ""), sptField,
                     FxDB(drutama("prtautonotransaksi"), 0), sptField,
                     FxDB(drutama("prtnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("prttgl"), ""), formatTgl), sptField,
                     FxDB(drutama("prtkodepa"), 0), sptField,
                     FxDB(drutama("prtsupplier"), 0), sptField,
                     FxDB(drutama("prtsupplierkontak"), ""), sptField,
                     FxDB(drutama("prt1alamat1"), ""), sptField,
                     FxDB(drutama("prt1alamat2"), ""), sptField,
                     FxDB(drutama("prt1alamat3"), ""), sptField,
                     FxDB(drutama("prt2alamat1"), ""), sptField,
                     FxDB(drutama("prt2alamat2"), ""), sptField,
                     FxDB(drutama("prt2alamat3"), ""), sptField,
                     FxDB(drutama("prtbagianpembelian"), 0), sptField,
                     FxDB(drutama("prttermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("prttgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("prturaian"), ""), sptField,
                     FxDB(drutama("prtcatatan"), ""), sptField,
                     FxDB(drutama("prtnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("prttglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("prttglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("prtmatauang"), ""), sptField,
                     FxDB(drutama("prtkurs"), 0), sptField,
                     FxDB(drutama("prthargatermasukpajak"), 0), sptField,
                     FxDB(drutama("prttotal"), 0), sptField,
                     FxDB(drutama("prtdiskonpersen"), ""), sptField,
                     FxDB(drutama("prtjmldiskon"), 0), sptField,
                     FxDB(drutama("prttotalpajak1detail"), 0), sptField,
                     FxDB(drutama("prttotalpajak2detail"), 0), sptField,
                     FxDB(drutama("prtbiayalainpersen"), ""), sptField,
                     FxDB(drutama("prtbiayalain"), 0), sptField,
                     FxDB(drutama("prttotaltransaksi"), 0), sptField,
                     FxDB(drutama("prtsisatransaksi"), 0), sptField,
                     FxDB(drutama("prtjmlbayar"), 0), sptField,
                     FxDB(drutama("prtstatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prttgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("prtnofakturpajak"), ""), sptField,
                     FxDB(drutama("prtsdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prttglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(drutama("prtrekdiskon"), ""), sptField,
                     FxDB(drutama("prtrekpajak1"), ""), sptField,
                     FxDB(drutama("prtrekpajak2"), ""), sptField,
                     FxDB(drutama("prtrekbiayalain"), ""), sptField,
                     FxDB(drutama("prtrekbayar"), ""), sptField,
                     FxDB(drutama("prtreksisa"), ""), sptField,
                     FxDB(drutama("prtidpr"), 0), sptField,
                     FxDB(drutama("prtidcs"), 0), sptField,
                     FxDB(drutama("prtidrq"), 0), sptField,
                     FxDB(drutama("prtidbs"), 0), sptField,
                     FxDB(drutama("prtidpo"), 0), sptField,
                     FxDB(drutama("prtidipc"), 0), sptField,
                     FxDB(drutama("prtidgrn"), 0), sptField,
                     FxDB(drutama("prtidri"), 0), sptField,
                     FxDB(drutama("prtiddnr"), 0), sptField,
                     FxDB(drutama("prtstatus"), 0), sptField,
                     FxDB(drutama("prtstatussebelumnya"), 0), sptField,
                     FxDB(drutama("prtjmlrevisi"), 0), sptField,
                     FxDB(drutama("prtcetakanke"), 0), sptField,
                     FxDB(drutama("prtinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prtinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("prtmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prtmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("prtposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prtpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("prttutupperiode"), 0), sptField,
                     FxDB(drutama("prtisclose"), 0), sptField,
                     FxDB(drutama("prtcustomtext1"), ""), sptField,
                     FxDB(drutama("prtcustomtext2"), ""), sptField,
                     FxDB(drutama("prtcustomtext3"), ""), sptField,
                     FxDB(drutama("prtcustomtext4"), ""), sptField,
                     FxDB(drutama("prtcustomtext5"), ""), sptField,
                     FxDB(drutama("prtcustomint1"), 0), sptField,
                     FxDB(drutama("prtcustomint2"), 0), sptField,
                     FxDB(drutama("prtcustomint3"), 0), sptField,
                     FxDB(drutama("prtcustomdbl1"), 0), sptField,
                     FxDB(drutama("prtcustomdbl2"), 0), sptField,
                     FxDB(drutama("prtcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prtcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("prtcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("prtcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("prtcabangnama"), ""), sptField,
                     FxDB(drutama("prtlokasinama"), ""), sptField,
                     FxDB(drutama("prtgudangnama"), ""), sptField,
                     FxDB(drutama("prtsupplierkode"), ""), sptField,
                     FxDB(drutama("prtsuppliernama"), ""), sptField,
                     FxDB(drutama("prtbagianpembeliankode"), ""), sptField,
                     FxDB(drutama("prtbagianpembeliannama"), ""), sptField,
                     FxDB(drutama("prtterminnama"), ""), sptField,
                     FxDB(drutama("prtterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("prtrekdiskonnama"), ""), sptField,
                     FxDB(drutama("prtrekpajak1nama"), ""), sptField,
                     FxDB(drutama("prtrekpajak2nama"), ""), sptField,
                     FxDB(drutama("prtrekbiayalainnama"), ""), sptField,
                     FxDB(drutama("prtrekbayarnama"), ""), sptField,
                     FxDB(drutama("prtreksisanama"), ""), sptField,
                     FxDB(drutama("prtnotransaksiri"), ""), sptField,
                     FxDB(drutama("prtnotransaksidnr"), ""), sptField,
                     FxDB(drutama("prtstatusnama"), ""), sptField,
                     FxDB(drutama("prtstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("prtinputusernama"), ""), sptField,
                     FxDB(drutama("prtmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("prtjenis"), 0), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idprtdetail"), 0), sptField,
                     FxDB(dr("idprt"), 0), sptField,
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
                     FxDB(dr("hargafix"), 0), sptField,
                     FxDB(dr("idhppkhususmasuk"), 0), sptField,
                     FxDB(dr("idhppfifomasuk"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
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
                     FxDB(dr("rekdiskonpembelian"), ""), sptField,
                     FxDB(dr("rekhargapokok"), ""), sptField,
                     FxDB(dr("rekreturpembelian"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idprdetail"), 0), sptField,
                     FxDB(dr("idcsdetail"), 0), sptField,
                     FxDB(dr("idrqdetail"), 0), sptField,
                     FxDB(dr("idbsdetail"), 0), sptField,
                     FxDB(dr("idpodetail"), 0), sptField,
                     FxDB(dr("idipcdetail"), 0), sptField,
                     FxDB(dr("idgrndetail"), 0), sptField,
                     FxDB(dr("idridetail"), 0), sptField,
                     FxDB(dr("iddnrdetail"), 0), sptField,
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
                     FxDB(dr("basset"), 0), sptField,
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
                     FxDB(dr("rinotransaksi"), ""), sptField,
                     FxDB(dr("dnrnotransaksi"), ""), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA BATCH
            sql = "select `nbt`.`nbtid` AS `nbtid`,`nbt`.`nbtjenismutasi` AS `nbtjenismutasi`,`nbt`.`nbtidbatchin` AS `nbtidbatchin`,`nbt`.`nbtgudang` AS `nbtgudang`,`nbt`.`nbtidbarang` AS `nbtidbarang`,`nbt`.`nbtkode` AS `nbtkode`,`nbt`.`nbtsumber` AS `nbtsumber`,`nbt`.`nbtidtransaksi` AS `nbtidtransaksi`,`nbt`.`nbtsatuan` AS `nbtsatuan`,`nbt`.`nbtjml` AS `nbtjml`,`nbt`.`nbtcustomtext1` AS `nbtcustomtext1`,`nbt`.`nbtcustomtext2` AS `nbtcustomtext2`,`nbt`.`nbtcustomtext3` AS `nbtcustomtext3`,`nbt`.`nbtcustomdbl1` AS `nbtcustomdbl1`,`nbt`.`nbtcustomdbl2` AS `nbtcustomdbl2`,`nbt`.`nbtcustomdbl3` AS `nbtcustomdbl3`,`nbt`.`nbtcustomdate1` AS `nbtcustomdate1`,`nbt`.`nbtcustomdate2` AS `nbtcustomdate2`,`nbt`.`nbtcustomdate3` AS `nbtcustomdate3`,`i`.`bkode` AS `kodebarang`, nbi.nbinotransaksi from ((`m1_no_batch_transaction` `nbt` join `m1_item` `i` on((`nbt`.`nbtidbarang` = `i`.`bid`))) left join `m1_no_batch_in` `nbi` on((`nbt`.`nbtidbatchin` = `nbi`.`nbiidbatchin`)))"
            Dim dtbatch As New DataTable
            dtbatch = AmbilData("aplikasi1-m1_no_batch_out", "nbtidtransaksi = '" & idtransaksi & "' AND nbtsumber = '" & sumber & "' AND (nbtjenismutasi = 1 OR nbiidbarang IS NOT NULL)", "nbtidbarang, nbtkode ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtbatch.Rows
                batch = String.Concat(batch,
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
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("nbinotransaksi"), ""), sptRow)
            Next
            If batch.Length > 0 Then batch = batch.Substring(0, batch.Length - sptRow.Length) Else batch = batch

            'AMBIL DATA SERIAL
            sql = "select `nst`.`nstid` AS `nstid`,`nst`.`nstjenismutasi` AS `nstjenismutasi`,`nst`.`nstidserialin` AS `nstidserialin`,`nst`.`nstgudang` AS `nstgudang`,`nst`.`nstidbarang` AS `nstidbarang`,`nst`.`nstkode` AS `nstkode`,`nst`.`nstsumber` AS `nstsumber`,`nst`.`nstidtransaksi` AS `nstidtransaksi`,`nst`.`nstsatuan` AS `nstsatuan`,`nst`.`nstjml` AS `nstjml`,`nst`.`nstcustomtext1` AS `nstcustomtext1`,`nst`.`nstcustomtext2` AS `nstcustomtext2`,`nst`.`nstcustomtext3` AS `nstcustomtext3`,`nst`.`nstcustomdbl1` AS `nstcustomdbl1`,`nst`.`nstcustomdbl2` AS `nstcustomdbl2`,`nst`.`nstcustomdbl3` AS `nstcustomdbl3`,`nst`.`nstcustomdate1` AS `nstcustomdate1`,`nst`.`nstcustomdate2` AS `nstcustomdate2`,`nst`.`nstcustomdate3` AS `nstcustomdate3`,`i`.`bkode` AS `kodebarang`, nsi.nsinotransaksi from ((`m1_no_serial_transaction` `nst` join `m1_item` `i` on((`nst`.`nstidbarang` = `i`.`bid`))) left join `m1_no_serial_in` `nsi` on((`nst`.`nstidserialin` = `nsi`.`nsiidserialin`)))"
            Dim dtserial As New DataTable
            dtserial = AmbilData("aplikasi1-m1_no_serial_out", "nstidtransaksi = '" & idtransaksi & "' AND nstsumber = '" & sumber & "' AND (nstjenismutasi = 1 OR nsiidbarang IS NOT NULL)", "nstidbarang, nstkode ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtserial.Rows
                serial = String.Concat(serial,
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
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("nsinotransaksi"), ""), sptRow)
            Next
            If serial.Length > 0 Then serial = serial.Substring(0, serial.Length - sptRow.Length) Else serial = serial

            'AMBIL DATA ASSET
            sql = "select atr.atid AS atid, atr.atasetid AS atasetid, atr.atjenismutasi AS atjenismutasi, atr.atsumber AS atsumber, atr.atidutama AS atidutama,atr.atidbarang AS atidbarang,atr.atkode AS atkode, atr.atnama AS atnama, atr.atkategori AS atkategori, atr.atcabang AS atcabang, atr.atlokasi AS atlokasi, atr.atgudang AS atgudang,atr.atdivisi AS atdivisi, atr.atsubdivisi AS atsubdivisi, atr.atcostcenter AS atcostcenter, atr.atproyek AS atproyek, atr.atcatatan AS atcatatan, atr.atnomor AS atnomor, atr.attglbeli AS attglbeli, atr.attglpakai AS attglpakai, atr.atjml AS atjml, atr.atsatuan AS atsatuan, atr.atmatauang AS atmatauang, atr.atkurs AS atkurs, atr.atharga AS atharga, atr.atdiskon AS atdiskon, atr.atjmldiskon AS atjmldiskon, atr.atpajak1 AS atpajak1, atr.atjmlpajak1 AS atjmlpajak1, atr.atpajak2 AS atpajak2, atr.atjmlpajak2 AS atjmlpajak2, atr.athargabeli AS athargabeli, atr.atnilairesidu AS atnilairesidu, atr.atumurekonomis AS atumurekonomis, atr.atbebanperbln AS atbebanperbln, atr.atakumulasibeban AS atakumulasibeban, atr.atnilaibuku AS atnilaibuku, (CASE WHEN atr.atnilaibuku < atr.atbebanperbln THEN atr.atnilaibuku ELSE atr.atbebanperbln END) as atnilaipenyusutan, atr.atmetode AS atmetode, atr.attabelpenyusutan AS attabelpenyusutan, atr.atintangible AS atintangible, atr.atfiskal AS atfiskal, atr.atatastengahbulan AS atatastengahbulan, atr.atrekasset AS atrekasset, atr.atrekakumdepresiasi AS atrekakumdepresiasi, atr.atrekdepresiasi AS atrekdepresiasi, atr.atrekpenghapusan AS atrekpenghapusan, atr.atprodusen AS atprodusen, atr.attglpensiun AS attglpensiun, atr.atpenyusutanke AS atpenyusutanke, atr.atnilaimenurun AS atnilaimenurun, atr.atdispose AS atdispose, atr.atpembelian AS atpembelian, atr.atpenjualan AS atpenjualan, atr.atlocked AS atlocked, atr.atstatus AS atstatus, atr.atstatussebelumnya AS atstatussebelumnya, atr.atisclose AS atisclose, atr.atinputuser AS atinputuser, atr.atinputtgl AS atinputtgl, atr.atmodifikasiuser AS atmodifikasiuser, atr.atmodifikasitgl AS atmodifikasitgl, atr.atcustomtext1,atr.atcustomtext2,atr.atcustomtext3,atr.atcustomtext4,atr.atcustomtext5,atr.atcustomint1,atr.atcustomint2,atr.atcustomint3,atr.atcustomint4,atr.atcustomint5,atr.atcustomdbl1,atr.atcustomdbl2,atr.atcustomdbl3,atr.atcustomdbl4,atr.atcustomdbl5,atr.atcustomdate1,atr.atcustomdate2,atr.atcustomdate3,atr.atcustomdate4,atr.atcustomdate5,ac.acnama AS atkategorinama, br.bnama AS atcabangnama, l.lnama AS atlokasinama, w.wnama AS atgudangnama,d.dnama AS atdivisinama, sd.sdnama AS atsubdivisinama, cc.ccnama AS atcostcenternama, p.pnama AS atproyeknama, dc.nama AS atmetodenama, t1.tnama AS atpajak1nama, ifnull(t1.tnilai, 0) AS atpajak1nilai, t2.tnama AS atpajak2nama, ifnull(t2.tnilai, 0) AS atpajak2nilai,coa1.cnama AS atrekassetnama, coa2.cnama AS atrekakumdepresiasinama, coa3.cnama AS atrekdepresiasinama, coa4.cnama AS atrekpenghapusannama, c1.kkode AS atprodusenkode, c1.knama AS atprodusennama,  sp1.nama AS atstatusnama,  sp2.nama AS atstatussebelumnyanama,  u1.unama AS atinputusernama,  u2.unama AS atmodifikasiusernama from m7_asset_transaction atr left join m7_asset_category ac on atr.atkategori = ac.ackode left join m1_branch br on atr.atcabang = br.bkode left join m1_location l on atr.atlokasi = l.lkode left join m1_warehouse w on atr.atgudang = w.wkode left join m1_division d on atr.atdivisi = d.dkode left join m1_subdivision sd on atr.atsubdivisi = sd.sdkode left join m7_depreciation_category dc on atr.atmetode = dc.kode left join m1_coa coa1 on atr.atrekasset = coa1.cnomor left join m1_coa coa2 on atr.atrekakumdepresiasi = coa2.cnomor left join m1_coa coa3 on atr.atrekdepresiasi = coa3.cnomor left join m1_coa coa4 on atr.atrekpenghapusan = coa4.cnomor left join m1_contact c1 on atr.atprodusen = c1.kid left join m0_status_progress sp1 on atr.atstatus = sp1.kode left join m0_status_progress sp2 on atr.atstatussebelumnya = sp2.kode left join m0_user u1 on atr.atinputuser = u1.userid left join m0_user u2 on atr.atmodifikasiuser = u2.userid left join m1_cost_center cc on atr.atcostcenter = cc.cckode left join m1_project p on atr.atproyek = p.pkode left join m1_tax t1 on atr.atpajak1 = t1.tkode left join m1_tax t2 on atr.atpajak2 = t2.tkode"
            Dim dtasset As New DataTable
            dtasset = AmbilData("aplikasi1-asset", "atidutama = '" & idtransaksi & "' AND atsumber = '" & sumber & "'", "atidbarang, atkode ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtasset.Rows
                asset = String.Concat(asset,
                     FxDB(dr("atid"), ""), sptField,
                     FxDB(dr("atasetid"), ""), sptField,
                     FxDB(dr("atjenismutasi"), 0), sptField,
                     FxDB(dr("atsumber"), ""), sptField,
                     FxDB(dr("atidutama"), ""), sptField,
                     FxDB(dr("atidbarang"), ""), sptField,
                     FxDB(dr("atkode"), ""), sptField,
                     FxDB(dr("atnama"), ""), sptField,
                     FxDB(dr("atkategori"), ""), sptField,
                     FxDB(dr("atcabang"), ""), sptField,
                     FxDB(dr("atlokasi"), ""), sptField,
                     FxDB(dr("atgudang"), ""), sptField,
                     FxDB(dr("atdivisi"), ""), sptField,
                     FxDB(dr("atsubdivisi"), ""), sptField,
                     FxDB(dr("atcostcenter"), ""), sptField,
                     FxDB(dr("atproyek"), ""), sptField,
                     FxDB(dr("atcatatan"), ""), sptField,
                     FxDB(dr("atnomor"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("attglbeli"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("attglpakai"), ""), formatTgl), sptField,
                     FxDB(dr("atjml"), 0), sptField,
                     FxDB(dr("atsatuan"), ""), sptField,
                     FxDB(dr("atmatauang"), ""), sptField,
                     FxDB(dr("atkurs"), 0), sptField,
                     FxDB(dr("atharga"), 0), sptField,
                     FxDB(dr("atdiskon"), ""), sptField,
                     FxDB(dr("atjmldiskon"), 0), sptField,
                     FxDB(dr("atpajak1"), ""), sptField,
                     FxDB(dr("atjmlpajak1"), 0), sptField,
                     FxDB(dr("atpajak2"), ""), sptField,
                     FxDB(dr("atjmlpajak2"), 0), sptField,
                     FxDB(dr("athargabeli"), 0), sptField,
                     FxDB(dr("atnilairesidu"), 0), sptField,
                     FxDB(dr("atumurekonomis"), 0), sptField,
                     FxDB(dr("atbebanperbln"), 0), sptField,
                     FxDB(dr("atakumulasibeban"), 0), sptField,
                     FxDB(dr("atnilaibuku"), 0), sptField,
                     FxDB(dr("atnilaipenyusutan"), 0), sptField,
                     FxDB(dr("atmetode"), 0), sptField,
                     FxDB(dr("attabelpenyusutan"), ""), sptField,
                     FxDB(dr("atintangible"), 0), sptField,
                     FxDB(dr("atfiskal"), 0), sptField,
                     FxDB(dr("atatastengahbulan"), 0), sptField,
                     FxDB(dr("atrekasset"), ""), sptField,
                     FxDB(dr("atrekakumdepresiasi"), ""), sptField,
                     FxDB(dr("atrekdepresiasi"), ""), sptField,
                     FxDB(dr("atrekpenghapusan"), ""), sptField,
                     FxDB(dr("atprodusen"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("attglpensiun"), ""), formatTgl), sptField,
                     FxDB(dr("atpenyusutanke"), 0), sptField,
                     FxDB(dr("atnilaimenurun"), 0), sptField,
                     FxDB(dr("atdispose"), 0), sptField,
                     FxDB(dr("atpembelian"), 0), sptField,
                     FxDB(dr("atpenjualan"), 0), sptField,
                     FxDB(dr("atlocked"), 0), sptField,
                     FxDB(dr("atstatus"), 0), sptField,
                     FxDB(dr("atstatussebelumnya"), 0), sptField,
                     FxDB(dr("atisclose"), 0), sptField,
                     FxDB(dr("atinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("atinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("atmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("atmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("atcustomtext1"), ""), sptField,
                     FxDB(dr("atcustomtext2"), ""), sptField,
                     FxDB(dr("atcustomtext3"), ""), sptField,
                     FxDB(dr("atcustomtext4"), ""), sptField,
                     FxDB(dr("atcustomtext5"), ""), sptField,
                     FxDB(dr("atcustomint1"), 0), sptField,
                     FxDB(dr("atcustomint2"), 0), sptField,
                     FxDB(dr("atcustomint3"), 0), sptField,
                     FxDB(dr("atcustomint4"), 0), sptField,
                     FxDB(dr("atcustomint5"), 0), sptField,
                     FxDB(dr("atcustomdbl1"), 0), sptField,
                     FxDB(dr("atcustomdbl2"), 0), sptField,
                     FxDB(dr("atcustomdbl3"), 0), sptField,
                     FxDB(dr("atcustomdbl4"), 0), sptField,
                     FxDB(dr("atcustomdbl5"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("atcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("atcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("atcustomdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("atcustomdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("atcustomdate5"), ""), formatTgl), sptField,
                     FxDB(dr("atkategorinama"), ""), sptField,
                     FxDB(dr("atcabangnama"), ""), sptField,
                     FxDB(dr("atlokasinama"), ""), sptField,
                     FxDB(dr("atgudangnama"), ""), sptField,
                     FxDB(dr("atdivisinama"), ""), sptField,
                     FxDB(dr("atsubdivisinama"), ""), sptField,
                     FxDB(dr("atcostcenternama"), ""), sptField,
                     FxDB(dr("atproyeknama"), ""), sptField,
                     FxDB(dr("atmetodenama"), ""), sptField,
                     FxDB(dr("atpajak1nama"), ""), sptField,
                     FxDB(dr("atpajak1nilai"), 0), sptField,
                     FxDB(dr("atpajak2nama"), ""), sptField,
                     FxDB(dr("atpajak2nilai"), 0), sptField,
                     FxDB(dr("atrekassetnama"), ""), sptField,
                     FxDB(dr("atrekakumdepresiasinama"), ""), sptField,
                     FxDB(dr("atrekdepresiasinama"), ""), sptField,
                     FxDB(dr("atrekpenghapusannama"), ""), sptField,
                     FxDB(dr("atprodusenkode"), ""), sptField,
                     FxDB(dr("atprodusennama"), ""), sptField,
                     FxDB(dr("atstatusnama"), ""), sptField,
                     FxDB(dr("atstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("atinputusernama"), ""), sptField,
                     FxDB(dr("atmodifikasiusernama"), ""), sptRow)
            Next
            If asset.Length > 0 Then asset = asset.Substring(0, asset.Length - sptRow.Length) Else asset = asset

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, batch, sptSubParam, serial, sptSubParam, asset)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("prtid, prtcabang, prtlokasi, prtgudang, prtasalbarang, prtasalbarangkategori, prtjenispembelian, prtjenispembeliankategori, prtcarabayar, prtsumber, prtautonotransaksi, prtnotransaksi, prttgl, prtkodepa, prtsupplier, prtsupplierkontak, prt1alamat1, prt1alamat2, prt1alamat3, prt2alamat1, prt2alamat2, prt2alamat3, prtbagianpembelian, prttermin, prttgljatuhtempo, prturaian, prtcatatan, prtnoref, prttglnoref, prttglpenutupan, prtmatauang, prtkurs, prthargatermasukpajak, prttotal, prtdiskonpersen, prtjmldiskon, prttotalpajak1detail, prttotalpajak2detail, prtbiayalainpersen, prtbiayalain, prttotaltransaksi, prtsisatransaksi, prtjmlbayar, prtstatuslunas, prttgllunas, prtnofakturpajak, prtsdhbayarpajak, prttglbayarpajak, prtrekdiskon, prtrekpajak1, prtrekpajak2, prtrekbiayalain, prtrekbayar, prtreksisa, prtidpr, prtidcs, prtidrq, prtidbs, prtidpo, prtidipc, prtidgrn, prtidri, prtiddnr, prtstatus, prtstatussebelumnya, prtjmlrevisi, prtcetakanke, prtinputuser, prtinputtgl, prtmodifikasiuser, prtmodifikasitgl, prtposting, prtpostingtgl, prttutupperiode, prtisclose, prtcustomtext1, prtcustomtext2, prtcustomtext3, prtcustomtext4, prtcustomtext5, prtcustomint1, prtcustomint2, prtcustomint3, prtcustomdbl1, prtcustomdbl2, prtcustomdbl3, prtcustomdate1, prtcustomdate2, prtcustomdate3, prtcabangnama, prtlokasinama, prtgudangnama, prtsupplierkode, prtsuppliernama, prtbagianpembeliankode, prtbagianpembeliannama, prtterminnama, prtterminharijatuhtempo, prtrekdiskonnama, prtrekpajak1nama, prtrekpajak2nama, prtrekbiayalainnama, prtrekbayarnama, prtreksisanama, prtnotransaksiri, prtnotransaksidnr, prtstatusnama, prtstatussebelumnyanama, prtinputusernama, prtmodifikasiusernama, prtjenis, kpkp" &
            sptSubParam & "idprtdetail, idprt, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, idhppkhususmasuk, idhppfifomasuk, hpp, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekdiskonpembelian, rekhargapokok, rekreturpembelian, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, idgrndetail, idridetail, iddnrdetail, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, basset, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangasalnama, gudangtransitnama, gudangtujuannama, costcenternama, divisinama, subdivisinama, proyeknama, rinotransaksi, dnrnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan" &
            sptSubParam & "nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang, nbtnotransaksi" &
            sptSubParam & "nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang, nstnotransaksi" &
            sptSubParam & "atid, atasetid, atjenismutasi, atsumber, atidutama, atidbarang, atkode, atnama, atkategori, atcabang, atlokasi, atgudang, atdivisi, atsubdivisi, atcostcenter, atproyek, atcatatan, atnomor, attglbeli, attglpakai, atjml, atsatuan, atmatauang, atkurs, atharga, atdiskon, atjmldiskon, atpajak1, atjmlpajak1, atpajak2, atjmlpajak2, athargabeli, atnilairesidu, atumurekonomis, atbebanperbln, atakumulasibeban, atnilaibuku, atnilaipenyusutan, atmetode, attabelpenyusutan, atintangible, atfiskal, atatastengahbulan, atrekasset, atrekakumdepresiasi, atrekdepresiasi, atrekpenghapusan, atprodusen, attglpensiun, atpenyusutanke, atnilaimenurun, atdispose, atpembelian, atpenjualan, atlocked, atstatus, atstatussebelumnya, atisclose, atinputuser, atinputtgl, atmodifikasiuser, atmodifikasitgl, atcustomtext1, atcustomtext2, atcustomtext3, atcustomtext4, atcustomtext5, atcustomint1, atcustomint2, atcustomint3, atcustomint4, atcustomint5, atcustomdbl1, atcustomdbl2, atcustomdbl3, atcustomdbl4, atcustomdbl5, atcustomdate1, atcustomdate2, atcustomdate3, atcustomdate4, atcustomdate5, atkategorinama, atcabangnama, atlokasinama, atgudangnama, atdivisinama, atsubdivisinama, atcostcenternama, atproyeknama, atmetodenama, atpajak1nama, atpajak1nilai, atpajak2nama, atpajak2nilai, atrekassetnama, atrekakumdepresiasinama, atrekdepresiasinama, atrekpenghapusannama, atprodusenkode, atprodusennama, atstatusnama, atstatussebelumnyanama, atinputusernama, atmodifikasiusernama"))

        Return wsResult
    End Function


    <WebMethod()>
    Public Function M4_PrtHistoryGetdataById_lama(ByVal param As String) As String

        'M4_PrtHistoryGetdataById Utama --------------------------------------------------------
        'prtidhistory, prtid, prtcabang, prtlokasi, prtgudang, prtasalbarang, prtasalbarangkategori, prtjenispembelian, 
        'prtjenispembeliankategori, prtcarabayar, prtsumber, prtautonotransaksi, prtnotransaksi, prttgl, prtkodepa, 
        'prtsupplier, prtsupplierkontak, prt1alamat1, prt1alamat2, prt1alamat3, prt2alamat1, prt2alamat2, 
        'prt2alamat3, prtbagianpembelian, prttermin, prttgljatuhtempo, prturaian, prtcatatan, prtnoref, 
        'prttglnoref, prttglpenutupan, prtmatauang, prtkurs, prthargatermasukpajak, prttotal, prtdiskonpersen, 
        'prtjmldiskon, prttotalpajak1detail, prttotalpajak2detail, prtbiayalainpersen, prtbiayalain, prttotaltransaksi, prtsisatransaksi, 
        'prtjmlbayar, prtstatuslunas, prttgllunas, prtnofakturpajak, prtsdhbayarpajak, prttglbayarpajak, prtrekdiskon, 
        'prtrekpajak1, prtrekpajak2, prtrekbiayalain, prtrekbayar, prtreksisa, prtidpr, prtidcs, 
        'prtidrq, prtidbs, prtidpo, prtidipc, prtidgrn, prtidri, prtiddnr, 
        'prtstatus, prtstatussebelumnya, prtjmlrevisi, prtcetakanke, prtinputuser, prtinputtgl, prtmodifikasiuser, 
        'prtmodifikasitgl, prtposting, prtpostingtgl, prttutupperiode, prtisclose, prtcustomtext1, prtcustomtext2, 
        'prtcustomtext3, prtcustomtext4, prtcustomtext5, prtcustomint1, prtcustomint2, prtcustomint3, prtcustomdbl1, 
        'prtcustomdbl2, prtcustomdbl3, prtcustomdate1, prtcustomdate2, prtcustomdate3, prtcabangnama, prtlokasinama, 
        'prtgudangnama, prtsupplierkode, prtsuppliernama, prtbagianpembeliankode, prtbagianpembeliannama, prtterminnama, prtterminharijatuhtempo, 
        'prtrekdiskonnama, prtrekpajak1nama, prtrekpajak2nama, prtrekbiayalainnama, prtrekbayarnama, prtreksisanama, prtnotransaksiri, 
        'prtnotransaksidnr, prtstatusnama, prtstatussebelumnyanama, prtinputusernama, prtmodifikasiusernama, prtjenis

        'M4_PrtHistoryGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idprtdetail, idprt, 
        'idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, 
        'satuanbarang, matauang, kurs, hargafix, idhppkhususmasuk, idhppfifomasuk, hpp, 
        'harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, 
        'cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekdiskonpembelian, 
        'rekhargapokok, rekreturpembelian, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, 
        'idgrndetail, idridetail, iddnrdetail, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, 
        'bhpp, bjenis, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, 
        'pajak2nilai, cabangnama, lokasinama, gudangasalnama, gudangtransitnama, gudangtujuannama, costcenternama, 
        'divisinama, subdivisinama, proyeknama, rinotransaksi, dnrnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan

        'M4_PrtHistoryGetdataById Batch --------------------------------------------------------
        'nbtidhistory, nbtidtransaksihistory, nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang

        'M4_PrtHistoryGetdataById Serial --------------------------------------------------------
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
        Dim sumber As String = "PRT"

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

        Dim NmMemcached As String = "aplikasi1-M4_Prt~M4_Prt_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "prtidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "prtidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_prt_getdata_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("prtidhistory"), 0), sptField, FxDB(drutama("prtid"), 0), sptField,
                     FxDB(drutama("prtcabang"), ""), sptField,
                     FxDB(drutama("prtlokasi"), ""), sptField,
                     FxDB(drutama("prtgudang"), ""), sptField,
                     FxDB(drutama("prtasalbarang"), ""), sptField,
                     FxDB(drutama("prtasalbarangkategori"), 0), sptField,
                     FxDB(drutama("prtjenispembelian"), ""), sptField,
                     FxDB(drutama("prtjenispembeliankategori"), 0), sptField,
                     FxDB(drutama("prtcarabayar"), 0), sptField,
                     FxDB(drutama("prtsumber"), ""), sptField,
                     FxDB(drutama("prtautonotransaksi"), 0), sptField,
                     FxDB(drutama("prtnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("prttgl"), ""), formatTgl), sptField,
                     FxDB(drutama("prtkodepa"), 0), sptField,
                     FxDB(drutama("prtsupplier"), 0), sptField,
                     FxDB(drutama("prtsupplierkontak"), ""), sptField,
                     FxDB(drutama("prt1alamat1"), ""), sptField,
                     FxDB(drutama("prt1alamat2"), ""), sptField,
                     FxDB(drutama("prt1alamat3"), ""), sptField,
                     FxDB(drutama("prt2alamat1"), ""), sptField,
                     FxDB(drutama("prt2alamat2"), ""), sptField,
                     FxDB(drutama("prt2alamat3"), ""), sptField,
                     FxDB(drutama("prtbagianpembelian"), 0), sptField,
                     FxDB(drutama("prttermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("prttgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("prturaian"), ""), sptField,
                     FxDB(drutama("prtcatatan"), ""), sptField,
                     FxDB(drutama("prtnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("prttglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("prttglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("prtmatauang"), ""), sptField,
                     FxDB(drutama("prtkurs"), 0), sptField,
                     FxDB(drutama("prthargatermasukpajak"), 0), sptField,
                     FxDB(drutama("prttotal"), 0), sptField,
                     FxDB(drutama("prtdiskonpersen"), ""), sptField,
                     FxDB(drutama("prtjmldiskon"), 0), sptField,
                     FxDB(drutama("prttotalpajak1detail"), 0), sptField,
                     FxDB(drutama("prttotalpajak2detail"), 0), sptField,
                     FxDB(drutama("prtbiayalainpersen"), ""), sptField,
                     FxDB(drutama("prtbiayalain"), 0), sptField,
                     FxDB(drutama("prttotaltransaksi"), 0), sptField,
                     FxDB(drutama("prtsisatransaksi"), 0), sptField,
                     FxDB(drutama("prtjmlbayar"), 0), sptField,
                     FxDB(drutama("prtstatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prttgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("prtnofakturpajak"), ""), sptField,
                     FxDB(drutama("prtsdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prttglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(drutama("prtrekdiskon"), ""), sptField,
                     FxDB(drutama("prtrekpajak1"), ""), sptField,
                     FxDB(drutama("prtrekpajak2"), ""), sptField,
                     FxDB(drutama("prtrekbiayalain"), ""), sptField,
                     FxDB(drutama("prtrekbayar"), ""), sptField,
                     FxDB(drutama("prtreksisa"), ""), sptField,
                     FxDB(drutama("prtidpr"), 0), sptField,
                     FxDB(drutama("prtidcs"), 0), sptField,
                     FxDB(drutama("prtidrq"), 0), sptField,
                     FxDB(drutama("prtidbs"), 0), sptField,
                     FxDB(drutama("prtidpo"), 0), sptField,
                     FxDB(drutama("prtidipc"), 0), sptField,
                     FxDB(drutama("prtidgrn"), 0), sptField,
                     FxDB(drutama("prtidri"), 0), sptField,
                     FxDB(drutama("prtiddnr"), 0), sptField,
                     FxDB(drutama("prtstatus"), 0), sptField,
                     FxDB(drutama("prtstatussebelumnya"), 0), sptField,
                     FxDB(drutama("prtjmlrevisi"), 0), sptField,
                     FxDB(drutama("prtcetakanke"), 0), sptField,
                     FxDB(drutama("prtinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prtinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("prtmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prtmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("prtposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prtpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("prttutupperiode"), 0), sptField,
                     FxDB(drutama("prtisclose"), 0), sptField,
                     FxDB(drutama("prtcustomtext1"), ""), sptField,
                     FxDB(drutama("prtcustomtext2"), ""), sptField,
                     FxDB(drutama("prtcustomtext3"), ""), sptField,
                     FxDB(drutama("prtcustomtext4"), ""), sptField,
                     FxDB(drutama("prtcustomtext5"), ""), sptField,
                     FxDB(drutama("prtcustomint1"), 0), sptField,
                     FxDB(drutama("prtcustomint2"), 0), sptField,
                     FxDB(drutama("prtcustomint3"), 0), sptField,
                     FxDB(drutama("prtcustomdbl1"), 0), sptField,
                     FxDB(drutama("prtcustomdbl2"), 0), sptField,
                     FxDB(drutama("prtcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prtcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("prtcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("prtcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("prtcabangnama"), ""), sptField,
                     FxDB(drutama("prtlokasinama"), ""), sptField,
                     FxDB(drutama("prtgudangnama"), ""), sptField,
                     FxDB(drutama("prtsupplierkode"), ""), sptField,
                     FxDB(drutama("prtsuppliernama"), ""), sptField,
                     FxDB(drutama("prtbagianpembeliankode"), ""), sptField,
                     FxDB(drutama("prtbagianpembeliannama"), ""), sptField,
                     FxDB(drutama("prtterminnama"), ""), sptField,
                     FxDB(drutama("prtterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("prtrekdiskonnama"), ""), sptField,
                     FxDB(drutama("prtrekpajak1nama"), ""), sptField,
                     FxDB(drutama("prtrekpajak2nama"), ""), sptField,
                     FxDB(drutama("prtrekbiayalainnama"), ""), sptField,
                     FxDB(drutama("prtrekbayarnama"), ""), sptField,
                     FxDB(drutama("prtreksisanama"), ""), sptField,
                     FxDB(drutama("prtnotransaksiri"), ""), sptField,
                     FxDB(drutama("prtnotransaksidnr"), ""), sptField,
                     FxDB(drutama("prtstatusnama"), ""), sptField,
                     FxDB(drutama("prtstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("prtinputusernama"), ""), sptField,
                     FxDB(drutama("prtmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("prtjenis"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField, FxDB(dr("idhistory"), 0), sptField, FxDB(dr("idprtdetail"), 0), sptField,
                     FxDB(dr("idprt"), 0), sptField,
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
                     FxDB(dr("hargafix"), 0), sptField,
                     FxDB(dr("idhppkhususmasuk"), 0), sptField,
                     FxDB(dr("idhppfifomasuk"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
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
                     FxDB(dr("rekdiskonpembelian"), ""), sptField,
                     FxDB(dr("rekhargapokok"), ""), sptField,
                     FxDB(dr("rekreturpembelian"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idprdetail"), 0), sptField,
                     FxDB(dr("idcsdetail"), 0), sptField,
                     FxDB(dr("idrqdetail"), 0), sptField,
                     FxDB(dr("idbsdetail"), 0), sptField,
                     FxDB(dr("idpodetail"), 0), sptField,
                     FxDB(dr("idipcdetail"), 0), sptField,
                     FxDB(dr("idgrndetail"), 0), sptField,
                     FxDB(dr("idridetail"), 0), sptField,
                     FxDB(dr("iddnrdetail"), 0), sptField,
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
                     FxDB(dr("rinotransaksi"), ""), sptField,
                     FxDB(dr("dnrnotransaksi"), ""), sptField,
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
            result(2) = " transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, batch, sptSubParam, serial)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("prtid, prtcabang, prtlokasi, prtgudang, prtasalbarang, prtasalbarangkategori, prtjenispembelian, prtjenispembeliankategori, prtcarabayar, prtsumber, prtautonotransaksi, prtnotransaksi, prttgl, prtkodepa, prtsupplier, prtsupplierkontak, prt1alamat1, prt1alamat2, prt1alamat3, prt2alamat1, prt2alamat2, prt2alamat3, prtbagianpembelian, prttermin, prttgljatuhtempo, prturaian, prtcatatan, prtnoref, prttglnoref, prttglpenutupan, prtmatauang, prtkurs, prthargatermasukpajak, prttotal, prtdiskonpersen, prtjmldiskon, prttotalpajak1detail, prttotalpajak2detail, prtbiayalainpersen, prtbiayalain, prttotaltransaksi, prtsisatransaksi, prtjmlbayar, prtstatuslunas, prttgllunas, prtnofakturpajak, prtsdhbayarpajak, prttglbayarpajak, prtrekdiskon, prtrekpajak1, prtrekpajak2, prtrekbiayalain, prtrekbayar, prtreksisa, prtidpr, prtidcs, prtidrq, prtidbs, prtidpo, prtidipc, prtidgrn, prtidri, prtiddnr, prtstatus, prtstatussebelumnya, prtjmlrevisi, prtcetakanke, prtinputuser, prtinputtgl, prtmodifikasiuser, prtmodifikasitgl, prtposting, prtpostingtgl, prttutupperiode, prtisclose, prtcustomtext1, prtcustomtext2, prtcustomtext3, prtcustomtext4, prtcustomtext5, prtcustomint1, prtcustomint2, prtcustomint3, prtcustomdbl1, prtcustomdbl2, prtcustomdbl3, prtcustomdate1, prtcustomdate2, prtcustomdate3, prtcabangnama, prtlokasinama, prtgudangnama, prtsupplierkode, prtsuppliernama, prtbagianpembeliankode, prtbagianpembeliannama, prtterminnama, prtterminharijatuhtempo, prtrekdiskonnama, prtrekpajak1nama, prtrekpajak2nama, prtrekbiayalainnama, prtrekbayarnama, prtreksisanama, prtnotransaksiri, prtnotransaksidnr, prtstatusnama, prtstatussebelumnyanama, prtinputusernama, prtmodifikasiusernama, prtjenis" & sptSubParam & "idprtdetail, idprt, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, idhppkhususmasuk, idhppfifomasuk, hpp, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekdiskonpembelian, rekhargapokok, rekreturpembelian, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, idgrndetail, idridetail, iddnrdetail, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangasalnama, gudangtransitnama, gudangtujuannama, costcenternama, divisinama, subdivisinama, proyeknama, rinotransaksi, dnrnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan" & sptSubParam & "nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang" & sptSubParam & "nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang"))

        Return wsResult
    End Function

End Class
