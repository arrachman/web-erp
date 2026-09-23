Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_ri_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_Ri_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m4_ri_history(SELECT 0, ri.* FROM m4_ri ri WHERE ri.riid = '" & idtransaksi & "')"
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
            sql = "SELECT riidhistory FROM m4_ri_history WHERE riid = '" & idtransaksi & "' ORDER BY rimodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY COST --------------------------------------
            sql = "INSERT INTO m4_ri_cost_history (SELECT 0, '" & result(4) & "', ri.* FROM m4_ri_cost ri WHERE ri.idri = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY COST -------------------------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m4_ri_detail_history (SELECT 0, '" & result(4) & "', ri.* FROM m4_ri_detail ri WHERE ri.idri = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY DETAIL -------------------------------


            'PROSES INSERT HISTORY PAY --------------------------------------
            sql = "INSERT INTO m4_ri_pay_history (SELECT 0, '" & result(4) & "', ri.* FROM m4_ri_pay ri WHERE ri.idri = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY PAY -------------------------------


            'PROSES INSERT HISTORY BATCH ---------------------------------------
            sql = "INSERT INTO m1_no_batch_transaction_history(SELECT 0, '" & result(4) & "', nb.* FROM m1_no_batch_transaction nb WHERE nb.nbtidtransaksi = '" & idtransaksi & "' and nb.nbtsumber = 'RI')"
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
            sql = "INSERT INTO m1_no_serial_transaction_history(SELECT 0, '" & result(4) & "', ns.* FROM m1_no_serial_transaction ns WHERE ns.nstidtransaksi = '" & idtransaksi & "' and ns.nstsumber = 'RI')"
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
            sql = "INSERT INTO m7_asset_transaction_history(SELECT 0, '" & result(4) & "', atr.* FROM m7_asset_transaction atr WHERE atr.atidutama = '" & idtransaksi & "' and atr.atsumber = 'RI')"
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
    Public Function M4_Ri_HistoryBSearch(ByVal param As String) As String
        'M4_RiBSearch --------------------------------------------------------
        'riid, ricabang, rilokasi, rigudang, riasalbarang, riasalbarangkategori, rijenispembelian, 
        'rijenispembeliankategori, ricarabayar, risumber, riautonotransaksi, rinotransaksi, ritgl, rikodepa, 
        'risupplier, risupplierkontak, ri1alamat1, ri1alamat2, ri1alamat3, ri2alamat1, ri2alamat2, 
        'ri2alamat3, ribagianpembelian, ritermin, ritgljatuhtempo, riuraian, ricatatan, rinoref, 
        'ritglnoref, ritglpenutupan, rimatauang, rikurs, rihargatermasukpajak, ritotal, ridiskonpersen, 
        'rijmldiskon, ritotalpajak1detail, ritotalpajak2detail, ribiayalainpersen, ribiayalain, ritotaltransaksi, rijmlbayar, 
        'ristatuslunas, ritgllunas, rinofakturpajak, risdhbayarpajak, ritglbayarpajak, rirekdiskon, rirekpajak1, 
        'rirekpajak2, rirekbiayalain, rirekbayar, riidpr, riidcs, riidrq, riidbs, 
        'riidpo, riidipc, riidgrn, ristatusdnr, ristatusprt, ristatusrealisasi, ristatus, 
        'ristatussebelumnya, rijmlrevisi, ricetakanke, riinputuser, riinputtgl, rimodifikasiuser, rimodifikasitgl, 
        'riposting, ripostingtgl, ritutupperiode, riisclose, ricabangnama, rilokasinama, rigudangnama, 
        'risupplierkode, risuppliernama, ribagianpembeliankode, ribagianpembeliannama, ponotransaksi, ipcnotransaksi, grnnotransaksi, 
        'ristatusnama, ristatussebelumnyanama, riinputusernama, rimodifikasiusernama, ricustomtext1, ricustomtext2, ricustomtext3, 
        'ricustomtext4, ricustomtext5, ricustomint1, ricustomint2, ricustomint3, ricustomdbl1, ricustomdbl2, ricustomdbl3, ricustomdate1,
        'ricustomdate2, ricustomdate3

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
                Filter = "riidhistory = " + paramSplit(3)
            End If
        End If
        'END OF VALIDASI PARAMETER PAGING ==================================================

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
            Filter = Filter.Replace("risupplierkode", "c1.kkode")
            Filter = Filter.Replace("risuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select ri.riidhistory, `ri`.`riid` AS `riid`,`ri`.`ricabang` AS `ricabang`,`ri`.`rilokasi` AS `rilokasi`,`ri`.`rigudang` AS `rigudang`,`ri`.`riasalbarang` AS `riasalbarang`,`ri`.`riasalbarangkategori` AS `riasalbarangkategori`,`ri`.`rijenispembelian` AS `rijenispembelian`,`ri`.`rijenispembeliankategori` AS `rijenispembeliankategori`,`ri`.`ricarabayar` AS `ricarabayar`,`ri`.`risumber` AS `risumber`,`ri`.`riautonotransaksi` AS `riautonotransaksi`,`ri`.`rinotransaksi` AS `rinotransaksi`,`ri`.`ritgl` AS `ritgl`,`ri`.`rikodepa` AS `rikodepa`,`ri`.`risupplier` AS `risupplier`,`ri`.`risupplierkontak` AS `risupplierkontak`,`ri`.`ri1alamat1` AS `ri1alamat1`,`ri`.`ri1alamat2` AS `ri1alamat2`,`ri`.`ri1alamat3` AS `ri1alamat3`,`ri`.`ri2alamat1` AS `ri2alamat1`,`ri`.`ri2alamat2` AS `ri2alamat2`,`ri`.`ri2alamat3` AS `ri2alamat3`,`ri`.`ribagianpembelian` AS `ribagianpembelian`,`ri`.`ritermin` AS `ritermin`,`ri`.`ritgljatuhtempo` AS `ritgljatuhtempo`,`ri`.`riuraian` AS `riuraian`,`ri`.`ricatatan` AS `ricatatan`,`ri`.`rinoref` AS `rinoref`,`ri`.`ritglnoref` AS `ritglnoref`,`ri`.`ritglpenutupan` AS `ritglpenutupan`,`ri`.`rimatauang` AS `rimatauang`,`ri`.`rikurs` AS `rikurs`,`ri`.`rihargatermasukpajak` AS `rihargatermasukpajak`,`ri`.`ritotal` AS `ritotal`,`ri`.`ridiskonpersen` AS `ridiskonpersen`,`ri`.`rijmldiskon` AS `rijmldiskon`,`ri`.`ritotalpajak1detail` AS `ritotalpajak1detail`,`ri`.`ritotalpajak2detail` AS `ritotalpajak2detail`,`ri`.`ribiayalainpersen` AS `ribiayalainpersen`,`ri`.`ribiayalain` AS `ribiayalain`,`ri`.`ritotaltransaksi` AS `ritotaltransaksi`,`ri`.`rijmlbayar` AS `rijmlbayar`,`ri`.`ristatuslunas` AS `ristatuslunas`,`ri`.`ritgllunas` AS `ritgllunas`,`ri`.`rinofakturpajak` AS `rinofakturpajak`,`ri`.`risdhbayarpajak` AS `risdhbayarpajak`,`ri`.`ritglbayarpajak` AS `ritglbayarpajak`,`ri`.`rirekdiskon` AS `rirekdiskon`,`ri`.`rirekpajak1` AS `rirekpajak1`,`ri`.`rirekpajak2` AS `rirekpajak2`,`ri`.`rirekbiayalain` AS `rirekbiayalain`,`ri`.`rirekbayar` AS `rirekbayar`,`ri`.`riidpr` AS `riidpr`,`ri`.`riidcs` AS `riidcs`,`ri`.`riidrq` AS `riidrq`,`ri`.`riidbs` AS `riidbs`,`ri`.`riidpo` AS `riidpo`,`ri`.`riidipc` AS `riidipc`,`ri`.`riidgrn` AS `riidgrn`,`ri`.`ristatusdnr` AS `ristatusdnr`,`ri`.`ristatusprt` AS `ristatusprt`,`ri`.`ristatusrealisasi` AS `ristatusrealisasi`,`ri`.`ristatus` AS `ristatus`,`ri`.`ristatussebelumnya` AS `ristatussebelumnya`,`ri`.`rijmlrevisi` AS `rijmlrevisi`,`ri`.`ricetakanke` AS `ricetakanke`,`ri`.`riinputuser` AS `riinputuser`,`ri`.`riinputtgl` AS `riinputtgl`,`ri`.`rimodifikasiuser` AS `rimodifikasiuser`,`ri`.`rimodifikasitgl` AS `rimodifikasitgl`,`ri`.`riposting` AS `riposting`,`ri`.`ripostingtgl` AS `ripostingtgl`,`ri`.`ritutupperiode` AS `ritutupperiode`,`ri`.`riisclose` AS `riisclose`,`br`.`bnama` AS `ricabangnama`,`lc`.`lnama` AS `rilokasinama`,`wh`.`wnama` AS `rigudangnama`,`c1`.`kkode` AS `risupplierkode`,`c1`.`knama` AS `risuppliernama`,`c2`.`kkode` AS `ribagianpembeliankode`,`c2`.`knama` AS `ribagianpembeliannama`,`po`.`ponotransaksi` AS `ponotransaksi`,`ipc`.`ipcnotransaksi` AS `ipcnotransaksi`,`grn`.`grnnotransaksi` AS `grnnotransaksi`,`st1`.`nama` AS `ristatusnama`,`st2`.`nama` AS `ristatussebelumnyanama`,`u1`.`unama` AS `riinputusernama`,`u2`.`unama` AS `rimodifikasiusernama`, `ri`.`ricustomtext1` AS `ricustomtext1`, `ri`.`ricustomtext2` AS `ricustomtext2`, `ri`.`ricustomtext3` AS `ricustomtext3`, `ri`.`ricustomtext4` AS `ricustomtext4`, `ri`.`ricustomtext5` AS `ricustomtext5`, `ri`.`ricustomint1` AS `ricustomint1`, `ri`.`ricustomint2` AS `ricustomint2`, `ri`.`ricustomint3` AS `ricustomint3`, `ri`.`ricustomdbl1` AS `ricustomdbl1`, `ri`.`ricustomdbl2` AS `ricustomdbl2`, `ri`.`ricustomdbl3` AS `ricustomdbl3`, `ri`.`ricustomdate1` AS `ricustomdate1`, `ri`.`ricustomdate1` AS `ricustomdate1`, `ri`.`ricustomdate2` AS `ricustomdate2`, `ri`.`ricustomdate3` AS `ricustomdate3`, cdis.cnama AS rirekdiskonnama, cpa.cnama AS rirekpajak1nama, cpa2.cnama AS rirekpajak2nama, cba.cnama AS rirekbiayalainnama from ((((((((((((`m4_ri_history` `ri` left join `m1_branch` `br` on((`br`.`bkode` = `ri`.`ricabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `ri`.`rilokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `ri`.`rigudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `ri`.`risupplier`))) left join `m1_contact` `c2` on((`c2`.`kid` = `ri`.`ribagianpembelian`))) left join `m4_po` `po` on((`ri`.`riidpo` = `po`.`poid`))) left join `m4_ipc` `ipc` on((`ri`.`riidipc` = `ipc`.`ipcid`))) left join `m4_grn` `grn` on((`ri`.`riidgrn` = `grn`.`grnid`))) left join `m0_status` `st1` on((`st1`.`kode` = `ri`.`ristatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `ri`.`ristatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `ri`.`riinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `ri`.`rimodifikasiuser`))) LEFT JOIN m1_coa cdis ON cdis.cnomor = ri.rirekdiskon LEFT JOIN m1_coa cpa ON cpa.cnomor = ri.rirekpajak1 LEFT JOIN m1_coa cpa2 ON cpa2.cnomor = ri.rirekpajak2 LEFT JOIN m1_coa cba ON cba.cnomor = ri.rirekbiayalain"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Ri", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("riid"), 0), sptField,
                     FxDB(dr("riidhistory"), 0), sptField,
                     FxDB(dr("ricabang"), ""), sptField,
                     FxDB(dr("rilokasi"), ""), sptField,
                     FxDB(dr("rigudang"), ""), sptField,
                     FxDB(dr("riasalbarang"), ""), sptField,
                     FxDB(dr("riasalbarangkategori"), 0), sptField,
                     FxDB(dr("rijenispembelian"), ""), sptField,
                     FxDB(dr("rijenispembeliankategori"), 0), sptField,
                     FxDB(dr("ricarabayar"), 0), sptField,
                     FxDB(dr("risumber"), ""), sptField,
                     FxDB(dr("riautonotransaksi"), 0), sptField,
                     FxDB(dr("rinotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ritgl"), ""), formatTgl), sptField,
                     FxDB(dr("rikodepa"), 0), sptField,
                     FxDB(dr("risupplier"), 0), sptField,
                     FxDB(dr("risupplierkontak"), ""), sptField,
                     FxDB(dr("ri1alamat1"), ""), sptField,
                     FxDB(dr("ri1alamat2"), ""), sptField,
                     FxDB(dr("ri1alamat3"), ""), sptField,
                     FxDB(dr("ri2alamat1"), ""), sptField,
                     FxDB(dr("ri2alamat2"), ""), sptField,
                     FxDB(dr("ri2alamat3"), ""), sptField,
                     FxDB(dr("ribagianpembelian"), 0), sptField,
                     FxDB(dr("ritermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ritgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("riuraian"), ""), sptField,
                     FxDB(dr("ricatatan"), ""), sptField,
                     FxDB(dr("rinoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ritglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ritglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("rimatauang"), ""), sptField,
                     FxDB(dr("rikurs"), 0), sptField,
                     FxDB(dr("rihargatermasukpajak"), 0), sptField,
                     FxDB(dr("ritotal"), 0), sptField,
                     FxDB(dr("ridiskonpersen"), ""), sptField,
                     FxDB(dr("rijmldiskon"), 0), sptField,
                     FxDB(dr("ritotalpajak1detail"), 0), sptField,
                     FxDB(dr("ritotalpajak2detail"), 0), sptField,
                     FxDB(dr("ribiayalainpersen"), ""), sptField,
                     FxDB(dr("ribiayalain"), 0), sptField,
                     FxDB(dr("ritotaltransaksi"), 0), sptField,
                     FxDB(dr("rijmlbayar"), 0), sptField,
                     FxDB(dr("ristatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ritgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("rinofakturpajak"), ""), sptField,
                     FxDB(dr("risdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ritglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(dr("rirekdiskon"), ""), sptField,
                     FxDB(dr("rirekpajak1"), ""), sptField,
                     FxDB(dr("rirekpajak2"), ""), sptField,
                     FxDB(dr("rirekbiayalain"), ""), sptField,
                     FxDB(dr("rirekbayar"), ""), sptField,
                     FxDB(dr("riidpr"), 0), sptField,
                     FxDB(dr("riidcs"), 0), sptField,
                     FxDB(dr("riidrq"), 0), sptField,
                     FxDB(dr("riidbs"), 0), sptField,
                     FxDB(dr("riidpo"), 0), sptField,
                     FxDB(dr("riidipc"), 0), sptField,
                     FxDB(dr("riidgrn"), 0), sptField,
                     FxDB(dr("ristatusdnr"), 0), sptField,
                     FxDB(dr("ristatusprt"), 0), sptField,
                     FxDB(dr("ristatusrealisasi"), 0), sptField,
                     FxDB(dr("ristatus"), 0), sptField,
                     FxDB(dr("ristatussebelumnya"), 0), sptField,
                     FxDB(dr("rijmlrevisi"), 0), sptField,
                     FxDB(dr("ricetakanke"), 0), sptField,
                     FxDB(dr("riinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("riinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rimodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rimodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("riposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ripostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ritutupperiode"), 0), sptField,
                     FxDB(dr("riisclose"), 0), sptField,
                     FxDB(dr("ricabangnama"), ""), sptField,
                     FxDB(dr("rilokasinama"), ""), sptField,
                     FxDB(dr("rigudangnama"), ""), sptField,
                     FxDB(dr("risupplierkode"), ""), sptField,
                     FxDB(dr("risuppliernama"), ""), sptField,
                     FxDB(dr("ribagianpembeliankode"), ""), sptField,
                     FxDB(dr("ribagianpembeliannama"), ""), sptField,
                     FxDB(dr("ponotransaksi"), ""), sptField,
                     FxDB(dr("ipcnotransaksi"), ""), sptField,
                     FxDB(dr("grnnotransaksi"), ""), sptField,
                     FxDB(dr("ristatusnama"), ""), sptField,
                     FxDB(dr("ristatussebelumnyanama"), ""), sptField,
                     FxDB(dr("riinputusernama"), ""), sptField,
                     FxDB(dr("rimodifikasiusernama"), ""), sptField,
                     FxDB(dr("ricustomtext1"), ""), sptField,
                     FxDB(dr("ricustomtext2"), ""), sptField,
                     FxDB(dr("ricustomtext3"), ""), sptField,
                     FxDB(dr("ricustomtext4"), ""), sptField,
                     FxDB(dr("ricustomtext5"), ""), sptField,
                     FxDB(dr("ricustomint1"), 0), sptField,
                     FxDB(dr("ricustomint2"), 0), sptField,
                     FxDB(dr("ricustomint3"), 0), sptField,
                     FxDB(dr("ricustomdbl1"), 0), sptField,
                     FxDB(dr("ricustomdbl2"), 0), sptField,
                     FxDB(dr("ricustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ricustomdate1"), ""), formatTglWaktu), sptField,
                     AsFormatTanggal(FxDB(dr("ricustomdate2"), ""), formatTglWaktu), sptField,
                     AsFormatTanggal(FxDB(dr("ricustomdate3"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rirekdiskonnama"), ""), sptField,
                     FxDB(dr("rirekpajak1nama"), ""), sptField,
                     FxDB(dr("rirekpajak2nama"), ""), sptField,
                     FxDB(dr("rirekbiayalainnama"), ""), sptRow)

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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("riidhistory, riid, ricabang, rilokasi, rigudang, riasalbarang, riasalbarangkategori, rijenispembelian, rijenispembeliankategori, ricarabayar, risumber, riautonotransaksi, rinotransaksi, ritgl, rikodepa, risupplier, risupplierkontak, ri1alamat1, ri1alamat2, ri1alamat3, ri2alamat1, ri2alamat2, ri2alamat3, ribagianpembelian, ritermin, ritgljatuhtempo, riuraian, ricatatan, rinoref, ritglnoref, ritglpenutupan, rimatauang, rikurs, rihargatermasukpajak, ritotal, ridiskonpersen, rijmldiskon, ritotalpajak1detail, ritotalpajak2detail, ribiayalainpersen, ribiayalain, ritotaltransaksi, rijmlbayar, ristatuslunas, ritgllunas, rinofakturpajak, risdhbayarpajak, ritglbayarpajak, rirekdiskon, rirekpajak1, rirekpajak2, rirekbiayalain, rirekbayar, riidpr, riidcs, riidrq, riidbs, riidpo, riidipc, riidgrn, ristatusdnr, ristatusprt, ristatusrealisasi, ristatus, ristatussebelumnya, rijmlrevisi, ricetakanke, riinputuser, riinputtgl, rimodifikasiuser, rimodifikasitgl, riposting, ripostingtgl, ritutupperiode, riisclose, ricabangnama, rilokasinama, rigudangnama, risupplierkode, risuppliernama, ribagianpembeliankode, ribagianpembeliannama, ponotransaksi, ipcnotransaksi, grnnotransaksi, ristatusnama, ristatussebelumnyanama, riinputusernama, rimodifikasiusernama, ricustomtext1, ricustomtext2, ricustomtext3, ricustomtext4, ricustomtext5, ricustomint1, ricustomint2, ricustomint3, ricustomdbl1, ricustomdbl2, ricustomdbl3, ricustomdate1, ricustomdate2, ricustomdate3, rirekdiskonnama, rirekpajak1nama, rirekpajak2nama, rirekbiayalainnama"))

        Return wsResult
    End Function


    <WebMethod()>
    Public Function M4_Ri_HistorySearch(ByVal param As String) As String
        'M4_Ri_HistorySearch --------------------------------------------------------
        'riidhistory, riid, ricabang, rilokasi, rigudang, riasalbarang, riasalbarangkategori, rijenispembelian, 
        'rijenispembeliankategori, ricarabayar, risumber, riautonotransaksi, rinotransaksi, ritgl, rikodepa, 
        'risupplier, risupplierkontak, ri1alamat1, ri1alamat2, ri1alamat3, ri2alamat1, ri2alamat2, 
        'ri2alamat3, ribagianpembelian, ritermin, ritgljatuhtempo, riuraian, ricatatan, rinoref, 
        'ritglnoref, ritglpenutupan, rimatauang, rikurs, rihargatermasukpajak, ritotal, ridiskonpersen, 
        'rijmldiskon, ritotalpajak1detail, ritotalpajak2detail, ribiayalainpersen, ribiayalain, ritotaltransaksi, rijmlbayar, 
        'ristatuslunas, ritgllunas, rinofakturpajak, risdhbayarpajak, ritglbayarpajak, rirekdiskon, rirekpajak1, 
        'rirekpajak2, rirekbiayalain, rirekbayar, riidpr, riidcs, riidrq, riidbs, 
        'riidpo, riidipc, riidgrn, ristatusdnr, ristatusprt, ristatusrealisasi, ristatus, 
        'ristatussebelumnya, rijmlrevisi, ricetakanke, riinputuser, riinputtgl, rimodifikasiuser, rimodifikasitgl, 
        'riposting, ripostingtgl, ritutupperiode, riisclose, ricabangnama, rilokasinama, rigudangnama, 
        'risupplierkode, risuppliernama, ribagianpembeliankode, ribagianpembeliannama, ponotransaksi, ipcnotransaksi, grnnotransaksi, 
        'ristatusnama, ristatussebelumnyanama, riinputusernama, rimodifikasiusernama, ricustomtext1, ricustomtext2, ricustomtext3, ricustomtext4, ricustomtext5, 
        'ricustomint1, ricustomint2, ricustomint3, ricustomdbl1, ricustomdbl2, ricustomdbl3, ricustomdate1, ricustomdate2, ricustomdate3, ricarabayarnama

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
            Filter = Filter.Replace("risupplierkode", "c1.kkode")
            Filter = Filter.Replace("risuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_ri_v_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Ri_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("riid"), 0), sptField,
                     FxDB(dr("riidhistory"), 0), sptField,
                     FxDB(dr("ricabang"), ""), sptField,
                     FxDB(dr("rilokasi"), ""), sptField,
                     FxDB(dr("rigudang"), ""), sptField,
                     FxDB(dr("riasalbarang"), ""), sptField,
                     FxDB(dr("riasalbarangkategori"), 0), sptField,
                     FxDB(dr("rijenispembelian"), ""), sptField,
                     FxDB(dr("rijenispembeliankategori"), 0), sptField,
                     FxDB(dr("ricarabayar"), 0), sptField,
                     FxDB(dr("risumber"), ""), sptField,
                     FxDB(dr("riautonotransaksi"), 0), sptField,
                     FxDB(dr("rinotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ritgl"), ""), formatTgl), sptField,
                     FxDB(dr("rikodepa"), 0), sptField,
                     FxDB(dr("risupplier"), 0), sptField,
                     FxDB(dr("risupplierkontak"), ""), sptField,
                     FxDB(dr("ri1alamat1"), ""), sptField,
                     FxDB(dr("ri1alamat2"), ""), sptField,
                     FxDB(dr("ri1alamat3"), ""), sptField,
                     FxDB(dr("ri2alamat1"), ""), sptField,
                     FxDB(dr("ri2alamat2"), ""), sptField,
                     FxDB(dr("ri2alamat3"), ""), sptField,
                     FxDB(dr("ribagianpembelian"), 0), sptField,
                     FxDB(dr("ritermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ritgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("riuraian"), ""), sptField,
                     FxDB(dr("ricatatan"), ""), sptField,
                     FxDB(dr("rinoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ritglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ritglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("rimatauang"), ""), sptField,
                     FxDB(dr("rikurs"), 0), sptField,
                     FxDB(dr("rihargatermasukpajak"), 0), sptField,
                     FxDB(dr("ritotal"), 0), sptField,
                     FxDB(dr("ridiskonpersen"), ""), sptField,
                     FxDB(dr("rijmldiskon"), 0), sptField,
                     FxDB(dr("ritotalpajak1detail"), 0), sptField,
                     FxDB(dr("ritotalpajak2detail"), 0), sptField,
                     FxDB(dr("ribiayalainpersen"), ""), sptField,
                     FxDB(dr("ribiayalain"), 0), sptField,
                     FxDB(dr("ritotaltransaksi"), 0), sptField,
                     FxDB(dr("rijmlbayar"), 0), sptField,
                     FxDB(dr("ristatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ritgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("rinofakturpajak"), ""), sptField,
                     FxDB(dr("risdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ritglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(dr("rirekdiskon"), ""), sptField,
                     FxDB(dr("rirekpajak1"), ""), sptField,
                     FxDB(dr("rirekpajak2"), ""), sptField,
                     FxDB(dr("rirekbiayalain"), ""), sptField,
                     FxDB(dr("rirekbayar"), ""), sptField,
                     FxDB(dr("riidpr"), 0), sptField,
                     FxDB(dr("riidcs"), 0), sptField,
                     FxDB(dr("riidrq"), 0), sptField,
                     FxDB(dr("riidbs"), 0), sptField,
                     FxDB(dr("riidpo"), 0), sptField,
                     FxDB(dr("riidipc"), 0), sptField,
                     FxDB(dr("riidgrn"), 0), sptField,
                     FxDB(dr("ristatusdnr"), 0), sptField,
                     FxDB(dr("ristatusprt"), 0), sptField,
                     FxDB(dr("ristatusrealisasi"), 0), sptField,
                     FxDB(dr("ristatus"), 0), sptField,
                     FxDB(dr("ristatussebelumnya"), 0), sptField,
                     FxDB(dr("rijmlrevisi"), 0), sptField,
                     FxDB(dr("ricetakanke"), 0), sptField,
                     FxDB(dr("riinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("riinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rimodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rimodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("riposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ripostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ritutupperiode"), 0), sptField,
                     FxDB(dr("riisclose"), 0), sptField,
                     FxDB(dr("ricabangnama"), ""), sptField,
                     FxDB(dr("rilokasinama"), ""), sptField,
                     FxDB(dr("rigudangnama"), ""), sptField,
                     FxDB(dr("risupplierkode"), ""), sptField,
                     FxDB(dr("risuppliernama"), ""), sptField,
                     FxDB(dr("ribagianpembeliankode"), ""), sptField,
                     FxDB(dr("ribagianpembeliannama"), ""), sptField,
                     FxDB(dr("ponotransaksi"), ""), sptField,
                     FxDB(dr("ipcnotransaksi"), ""), sptField,
                     FxDB(dr("grnnotransaksi"), ""), sptField,
                     FxDB(dr("ristatusnama"), ""), sptField,
                     FxDB(dr("ristatussebelumnyanama"), ""), sptField,
                     FxDB(dr("riinputusernama"), ""), sptField,
                     FxDB(dr("rimodifikasiusernama"), ""), sptField,
                     FxDB(dr("ricustomtext1"), ""), sptField,
                     FxDB(dr("ricustomtext2"), ""), sptField,
                     FxDB(dr("ricustomtext3"), ""), sptField,
                     FxDB(dr("ricustomtext4"), ""), sptField,
                     FxDB(dr("ricustomtext5"), ""), sptField,
                     FxDB(dr("ricustomint1"), 0), sptField,
                     FxDB(dr("ricustomint2"), 0), sptField,
                     FxDB(dr("ricustomint3"), 0), sptField,
                     FxDB(dr("ricustomdbl1"), 0), sptField,
                     FxDB(dr("ricustomdbl2"), 0), sptField,
                     FxDB(dr("ricustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ricustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ricustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ricustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("ricarabayarnama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("riidhistory, riid, ricabang, rilokasi, rigudang, riasalbarang, riasalbarangkategori, rijenispembelian, rijenispembeliankategori, ricarabayar, risumber, riautonotransaksi, rinotransaksi, ritgl, rikodepa, risupplier, risupplierkontak, ri1alamat1, ri1alamat2, ri1alamat3, ri2alamat1, ri2alamat2, ri2alamat3, ribagianpembelian, ritermin, ritgljatuhtempo, riuraian, ricatatan, rinoref, ritglnoref, ritglpenutupan, rimatauang, rikurs, rihargatermasukpajak, ritotal, ridiskonpersen, rijmldiskon, ritotalpajak1detail, ritotalpajak2detail, ribiayalainpersen, ribiayalain, ritotaltransaksi, rijmlbayar, ristatuslunas, ritgllunas, rinofakturpajak, risdhbayarpajak, ritglbayarpajak, rirekdiskon, rirekpajak1, rirekpajak2, rirekbiayalain, rirekbayar, riidpr, riidcs, riidrq, riidbs, riidpo, riidipc, riidgrn, ristatusdnr, ristatusprt, ristatusrealisasi, ristatus, ristatussebelumnya, rijmlrevisi, ricetakanke, riinputuser, riinputtgl, rimodifikasiuser, rimodifikasitgl, riposting, ripostingtgl, ritutupperiode, riisclose, ricabangnama, rilokasinama, rigudangnama, risupplierkode, risuppliernama, ribagianpembeliankode, ribagianpembeliannama, ponotransaksi, ipcnotransaksi, grnnotransaksi, ristatusnama, ristatussebelumnyanama, riinputusernama, rimodifikasiusernama, ricustomtext1, ricustomtext2, ricustomtext3, ricustomtext4, ricustomtext5, ricustomint1, ricustomint2, ricustomint3, ricustomdbl1, ricustomdbl2, ricustomdbl3, ricustomdate1, ricustomdate2, ricustomdate3, ricarabayarnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_RiHistoryGetdataById(ByVal param As String) As String

        'M4_RiGetdataById Utama --------------------------------------------------------
        'riid, ricabang, rilokasi, rigudang, riasalbarang, riasalbarangkategori, rijenispembelian, 
        'rijenispembeliankategori, ricarabayar, risumber, riautonotransaksi, rinotransaksi, ritgl, rikodepa, 
        'risupplier, risupplierkontak, ri1alamat1, ri1alamat2, ri1alamat3, ri2alamat1, ri2alamat2, 
        'ri2alamat3, ribagianpembelian, ritermin, ritgljatuhtempo, riuraian, ricatatan, rinoref, 
        'ritglnoref, ritglpenutupan, rimatauang, rikurs, rihargatermasukpajak, ritotal, ridiskonpersen, 
        'rijmldiskon, ritotalpajak1detail, ritotalpajak2detail, ribiayalainpersen, ribiayalain, ritotaltransaksi, rijmlbayar, 
        'ristatuslunas, ritgllunas, rinofakturpajak, risdhbayarpajak, ritglbayarpajak, rirekdiskon, rirekpajak1, 
        'rirekpajak2, rirekbiayalain, rirekbayar, riidpr, riidcs, riidrq, riidbs, 
        'riidpo, riidipc, riidgrn, ristatusdnr, ristatusprt, ristatusrealisasi, ristatus, 
        'ristatussebelumnya, rijmlrevisi, ricetakanke, riinputuser, riinputtgl, rimodifikasiuser, rimodifikasitgl, 
        'riposting, ripostingtgl, ritutupperiode, riisclose, ricustomtext1, ricustomtext2, ricustomtext3, 
        'ricustomtext4, ricustomtext5, ricustomint1, ricustomint2, ricustomint3, ricustomdbl1, ricustomdbl2, 
        'ricustomdbl3, ricustomdate1, ricustomdate2, ricustomdate3, ricabangnama, rilokasinama, rigudangnama, 
        'risupplierkode, risuppliernama, ribagianpembeliankode, ribagianpembeliannama, riterminnama, riterminharijatuhtempo, rirekdiskonnama, 
        'rirekpajak1nama, rirekpajak2nama, rirekbiayalainnama, rirekbayarnama, rinotransaksipo, rinotransaksiipc, rinotransaksigrn, 
        'ristatusnama, ristatussebelumnyanama, riinputusernama, rimodifikasiusernama, kpkp, rijmluangmuka, rirekuangmuka, riidap, rirekuangmukanama, apnotransaksi

        'M4_RiGetdataById Detail -------------------------------------------------------
        'idridetail, idri, idbarang, 
        'namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, 
        'matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, 
        'jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, rekpersediaan, 
        'rekdiskonpembelian, rekhutangsementara, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, 
        'idgrndetail, jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, 
        'bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, 
        'gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, ponotransaksi, ipcnotransaksi, 
        'grnnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan

        'M4_RiGetdataById Batch --------------------------------------------------------
        'nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang

        'M4_RiGetdataById Serial --------------------------------------------------------
        'nstid, nstjenismutasi, nstidserialin, nstgudang,  nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, 
        'nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, 
        'nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang

        'M4_RiGetdataById Cost --------------------------------------------------------
        'idricost, idri, kodecost, matauang, kurs, jumlah, rekdebit, 
        'rekkredit, catatan, costcenter, divisi, subdivisi, proyek, urutan, 
        'idprcost, idcscost, idrqcost, idbscost, idpocost, idipccost, idgrncost, 
        'jumlahbayar, statusbayar, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3, kodecostnama, rekdebitnama, 
        'rekkreditnama, costcenternama, divisinama, subdivisinama, proyeknama, kontak, kontakkode, kontaknama, termasukhpp

        'M4_RiGetdataById Pay -------------------------------------------------------
        'idricarabayar, idri, carabayar, matauang, 
        'kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, 
        'rekbank, rekgiro, catatan, urutan, isclose, carabayarnama, banknama, 
        'rekbanknama, rekgironama, sumber, idtransaksi, totaltransaksi, terbayar, notransaksi, tgl

        'M4_RiGetdataById Asset --------------------------------------------------------
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

        Dim utama As String = "", detail As String = "", batch As String = "", serial As String = "", cost As String = "", idtransaksi As String = ""
        Dim pay As String = "", asset As String = ""

        Dim sumber As String = "RI"

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

        Dim NmMemcached As String = "aplikasi1-m4_ri_history~m4_ri_detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "riidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "riidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m4_ri_getdata")
        sql = "select ri.riid AS riid,ri.ricabang AS ricabang,ri.rilokasi AS rilokasi,ri.rigudang AS rigudang,ri.riasalbarang AS riasalbarang,ri.riasalbarangkategori AS riasalbarangkategori,ri.rijenispembelian AS rijenispembelian,ri.rijenispembeliankategori AS rijenispembeliankategori,ri.ricarabayar AS ricarabayar,ri.risumber AS risumber,ri.riautonotransaksi AS riautonotransaksi,ri.rinotransaksi AS rinotransaksi,ri.ritgl AS ritgl,ri.rikodepa AS rikodepa,ri.risupplier AS risupplier,ri.risupplierkontak AS risupplierkontak,ri.ri1alamat1 AS ri1alamat1,ri.ri1alamat2 AS ri1alamat2,ri.ri1alamat3 AS ri1alamat3,ri.ri2alamat1 AS ri2alamat1,ri.ri2alamat2 AS ri2alamat2,ri.ri2alamat3 AS ri2alamat3,ri.ribagianpembelian AS ribagianpembelian,ri.ritermin AS ritermin,ri.ritgljatuhtempo AS ritgljatuhtempo,ri.riuraian AS riuraian,ri.ricatatan AS ricatatan,ri.rinoref AS rinoref,ri.ritglnoref AS ritglnoref,ri.ritglpenutupan AS ritglpenutupan,ri.rimatauang AS rimatauang,ri.rikurs AS rikurs,ri.rihargatermasukpajak AS rihargatermasukpajak,ri.ritotal AS ritotal,ri.ridiskonpersen AS ridiskonpersen,ri.rijmldiskon AS rijmldiskon,ri.ritotalpajak1detail AS ritotalpajak1detail,ri.ritotalpajak2detail AS ritotalpajak2detail,ri.ribiayalainpersen AS ribiayalainpersen,ri.ribiayalain AS ribiayalain,ri.ritotaltransaksi AS ritotaltransaksi,ri.rijmlbayar AS rijmlbayar,ri.ristatuslunas AS ristatuslunas,ri.ritgllunas AS ritgllunas,ri.rinofakturpajak AS rinofakturpajak,ri.risdhbayarpajak AS risdhbayarpajak,ri.ritglbayarpajak AS ritglbayarpajak,ri.rirekdiskon AS rirekdiskon,ri.rirekpajak1 AS rirekpajak1,ri.rirekpajak2 AS rirekpajak2,ri.rirekbiayalain AS rirekbiayalain,ri.rirekbayar AS rirekbayar,ri.riidpr AS riidpr,ri.riidcs AS riidcs,ri.riidrq AS riidrq,ri.riidbs AS riidbs,ri.riidpo AS riidpo,ri.riidipc AS riidipc,ri.riidgrn AS riidgrn,ri.ristatusdnr AS ristatusdnr,ri.ristatusprt AS ristatusprt,ri.ristatusrealisasi AS ristatusrealisasi,ri.ristatus AS ristatus,ri.ristatussebelumnya AS ristatussebelumnya,ri.rijmlrevisi AS rijmlrevisi,ri.ricetakanke AS ricetakanke,ri.riinputuser AS riinputuser,ri.riinputtgl AS riinputtgl,ri.rimodifikasiuser AS rimodifikasiuser,ri.rimodifikasitgl AS rimodifikasitgl,ri.riposting AS riposting,ri.ripostingtgl AS ripostingtgl,ri.ritutupperiode AS ritutupperiode,ri.riisclose AS riisclose,ri.ricustomtext1 AS ricustomtext1,ri.ricustomtext2 AS ricustomtext2,ri.ricustomtext3 AS ricustomtext3,ri.ricustomtext4 AS ricustomtext4,ri.ricustomtext5 AS ricustomtext5,ri.ricustomint1 AS ricustomint1,ri.ricustomint2 AS ricustomint2,ri.ricustomint3 AS ricustomint3,ri.ricustomdbl1 AS ricustomdbl1,ri.ricustomdbl2 AS ricustomdbl2,ri.ricustomdbl3 AS ricustomdbl3,ri.ricustomdate1 AS ricustomdate1,ri.ricustomdate2 AS ricustomdate2,ri.ricustomdate3 AS ricustomdate3,br.bnama AS ricabangnama,lc.lnama AS rilokasinama,wh.wnama AS rigudangnama,c1.kkode AS risupplierkode,c1.knama AS risuppliernama,c2.kkode AS ribagianpembeliankode,c2.knama AS ribagianpembeliannama,tr.trnama AS riterminnama,tr.trharijatuhtempo AS riterminharijatuhtempo,coa1.cnama AS rirekdiskonnama,coa2.cnama AS rirekpajak1nama,coa3.cnama AS rirekpajak2nama,coa4.cnama AS rirekbiayalainnama,coa5.cnama AS rirekbayarnama,po.ponotransaksi AS rinotransaksipo,ipc.ipcnotransaksi AS rinotransaksiipc,grn.grnnotransaksi AS rinotransaksigrn,st1.nama AS ristatusnama,st2.nama AS ristatussebelumnyanama,u1.unama AS riinputusernama,u2.unama AS rimodifikasiusernama,rid.idridetail AS idridetail,rid.idri AS idri,rid.idbarang AS idbarang,rid.namabarang AS namabarang,rid.tipebarang AS tipebarang,rid.jml AS jml,rid.satuan AS satuan,rid.nilaisatuan AS nilaisatuan,rid.jmlbarang AS jmlbarang,rid.satuanbarang AS satuanbarang,rid.matauang AS matauang,rid.kurs AS kurs,rid.hargafix AS hargafix,rid.harga AS harga,rid.diskon AS diskon,rid.jmldiskon AS jmldiskon,rid.pajak1 AS pajak1,rid.jmlpajak1 AS jmlpajak1,rid.pajak2 AS pajak2,rid.jmlpajak2 AS jmlpajak2,rid.cabang AS cabang,rid.lokasi AS lokasi,rid.gudang AS gudang,i.brekpersediaan AS rekpersediaan,i.brekdiskonpembelian AS rekdiskonpembelian,rid.rekhutangsementara AS rekhutangsementara,rid.costcenter AS costcenter,rid.divisi AS divisi,rid.subdivisi AS subdivisi,rid.proyek AS proyek,rid.catatan AS catatan,rid.urutan AS urutan,rid.idprdetail AS idprdetail,rid.idcsdetail AS idcsdetail,rid.idrqdetail AS idrqdetail,rid.idbsdetail AS idbsdetail,rid.idpodetail AS idpodetail,rid.idipcdetail AS idipcdetail,rid.idgrndetail AS idgrndetail,rid.jmldnr AS jmldnr,rid.statusdnr AS statusdnr,rid.jmlprt AS jmlprt,rid.statusprt AS statusprt,rid.jmlrealisasi AS jmlrealisasi,rid.statusrealisasi AS statusrealisasi,rid.isclose AS isclose,rid.customtext1 AS customtext1,rid.customtext2 AS customtext2,rid.customtext3 AS customtext3,rid.customdbl1 AS customdbl1,rid.customdbl2 AS customdbl2,rid.customdbl3 AS customdbl3,rid.customdate1 AS customdate1,rid.customdate2 AS customdate2,rid.customdate3 AS customdate3,i.bkode AS kodebarang,i.bhpp AS bhpp,i.bjenis AS bjenis,i.bserial AS bserial,i.bbatch AS bbatch,i.basset AS basset,t1.tnama AS pajak1nama,t1.tnilai AS pajak1nilai,t2.tnama AS pajak2nama,t2.tnilai AS pajak2nilai,brd.bnama AS cabangnama,lcd.lnama AS lokasinama,whd.wnama AS gudangnama,cc.ccnama AS costcenternama,d.dnama AS divisinama,sd.sdnama AS subdivisinama,p.pnama AS proyeknama,po2.ponotransaksi AS ponotransaksi,ipc2.ipcnotransaksi AS ipcnotransaksi,grn2.grnnotransaksi AS grnnotransaksi, c1.kpkp, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan, ri.rijmluangmuka, ri.rirekuangmuka, ri.riidap, coa6.cnama as rirekuangmukanama, ap.apnotransaksi as apnotransaksi  from m4_ri_history ri join m4_ri_detail_history rid on ri.riid = rid.idri left join m1_branch br on br.bkode = ri.ricabang left join m1_location lc on lc.lkode = ri.rilokasi left join m1_warehouse wh on wh.wkode = ri.rigudang left join m1_contact c1 on c1.kid = ri.risupplier left join m1_contact c2 on c2.kid = ri.ribagianpembelian left join m1_terms tr on ri.ritermin = tr.trkode left join m1_coa coa1 on ri.rirekdiskon = coa1.cnomor left join m1_coa coa2 on ri.rirekpajak1 = coa2.cnomor left join m1_coa coa3 on ri.rirekpajak2 = coa3.cnomor left join m1_coa coa4 on ri.rirekbiayalain = coa4.cnomor left join m1_coa coa5 on ri.rirekbayar = coa5.cnomor left join m4_po po on ri.riidpo = po.poid left join m4_ipc ipc on ri.riidipc = ipc.ipcid left join m4_grn grn on ri.riidgrn = grn.grnid left join m0_status st1 on st1.kode = ri.ristatus left join m0_status st2 on st2.kode = ri.ristatussebelumnya left join m0_user u1 on u1.userid = ri.riinputuser left join m0_user u2 on u2.userid = ri.rimodifikasiuser left join m1_item i on i.bid = rid.idbarang left join m1_tax t1 on rid.pajak1 = t1.tkode left join m1_tax t2 on rid.pajak2 = t2.tkode left join m1_branch brd on rid.cabang = brd.bkode left join m1_location lcd on rid.lokasi = lcd.lkode left join m1_warehouse whd on rid.gudang = whd.wkode left join m1_project p on rid.proyek = p.pkode left join m4_po_detail pod on rid.idpodetail = pod.idpodetail left join m4_po po2 on pod.idpo = po2.poid left join m4_ipc_detail ipcd on rid.idipcdetail = ipcd.idipcdetail left join m4_ipc ipc2 on ipcd.idipc = ipc2.ipcid left join m4_grn_detail grnd on rid.idgrndetail = grnd.idgrndetail left join m4_grn grn2 on grnd.idgrn = grn2.grnid left join m1_cost_center cc on rid.costcenter = cc.cckode left join m1_division d on rid.divisi = d.dkode left join m1_subdivision sd on rid.subdivisi = sd.sdkode left join m1_coa coa6 on ri.rirekuangmuka = coa6.cnomor left join m4_ap ap on ri.riidap = ap.apid"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("riid"), 0), sptField,
                     FxDB(drutama("ricabang"), ""), sptField,
                     FxDB(drutama("rilokasi"), ""), sptField,
                     FxDB(drutama("rigudang"), ""), sptField,
                     FxDB(drutama("riasalbarang"), ""), sptField,
                     FxDB(drutama("riasalbarangkategori"), 0), sptField,
                     FxDB(drutama("rijenispembelian"), ""), sptField,
                     FxDB(drutama("rijenispembeliankategori"), 0), sptField,
                     FxDB(drutama("ricarabayar"), 0), sptField,
                     FxDB(drutama("risumber"), ""), sptField,
                     FxDB(drutama("riautonotransaksi"), 0), sptField,
                     FxDB(drutama("rinotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ritgl"), ""), formatTgl), sptField,
                     FxDB(drutama("rikodepa"), 0), sptField,
                     FxDB(drutama("risupplier"), 0), sptField,
                     FxDB(drutama("risupplierkontak"), ""), sptField,
                     FxDB(drutama("ri1alamat1"), ""), sptField,
                     FxDB(drutama("ri1alamat2"), ""), sptField,
                     FxDB(drutama("ri1alamat3"), ""), sptField,
                     FxDB(drutama("ri2alamat1"), ""), sptField,
                     FxDB(drutama("ri2alamat2"), ""), sptField,
                     FxDB(drutama("ri2alamat3"), ""), sptField,
                     FxDB(drutama("ribagianpembelian"), 0), sptField,
                     FxDB(drutama("ritermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ritgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("riuraian"), ""), sptField,
                     FxDB(drutama("ricatatan"), ""), sptField,
                     FxDB(drutama("rinoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ritglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ritglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("rimatauang"), ""), sptField,
                     FxDB(drutama("rikurs"), 0), sptField,
                     FxDB(drutama("rihargatermasukpajak"), 0), sptField,
                     FxDB(drutama("ritotal"), 0), sptField,
                     FxDB(drutama("ridiskonpersen"), ""), sptField,
                     FxDB(drutama("rijmldiskon"), 0), sptField,
                     FxDB(drutama("ritotalpajak1detail"), 0), sptField,
                     FxDB(drutama("ritotalpajak2detail"), 0), sptField,
                     FxDB(drutama("ribiayalainpersen"), ""), sptField,
                     FxDB(drutama("ribiayalain"), 0), sptField,
                     FxDB(drutama("ritotaltransaksi"), 0), sptField,
                     FxDB(drutama("rijmlbayar"), 0), sptField,
                     FxDB(drutama("ristatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ritgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("rinofakturpajak"), ""), sptField,
                     FxDB(drutama("risdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ritglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(drutama("rirekdiskon"), ""), sptField,
                     FxDB(drutama("rirekpajak1"), ""), sptField,
                     FxDB(drutama("rirekpajak2"), ""), sptField,
                     FxDB(drutama("rirekbiayalain"), ""), sptField,
                     FxDB(drutama("rirekbayar"), ""), sptField,
                     FxDB(drutama("riidpr"), 0), sptField,
                     FxDB(drutama("riidcs"), 0), sptField,
                     FxDB(drutama("riidrq"), 0), sptField,
                     FxDB(drutama("riidbs"), 0), sptField,
                     FxDB(drutama("riidpo"), 0), sptField,
                     FxDB(drutama("riidipc"), 0), sptField,
                     FxDB(drutama("riidgrn"), 0), sptField,
                     FxDB(drutama("ristatusdnr"), 0), sptField,
                     FxDB(drutama("ristatusprt"), 0), sptField,
                     FxDB(drutama("ristatusrealisasi"), 0), sptField,
                     FxDB(drutama("ristatus"), 0), sptField,
                     FxDB(drutama("ristatussebelumnya"), 0), sptField,
                     FxDB(drutama("rijmlrevisi"), 0), sptField,
                     FxDB(drutama("ricetakanke"), 0), sptField,
                     FxDB(drutama("riinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("riinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rimodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rimodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("riposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ripostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("ritutupperiode"), 0), sptField,
                     FxDB(drutama("riisclose"), 0), sptField,
                     FxDB(drutama("ricustomtext1"), ""), sptField,
                     FxDB(drutama("ricustomtext2"), ""), sptField,
                     FxDB(drutama("ricustomtext3"), ""), sptField,
                     FxDB(drutama("ricustomtext4"), ""), sptField,
                     FxDB(drutama("ricustomtext5"), ""), sptField,
                     FxDB(drutama("ricustomint1"), 0), sptField,
                     FxDB(drutama("ricustomint2"), 0), sptField,
                     FxDB(drutama("ricustomint3"), 0), sptField,
                     FxDB(drutama("ricustomdbl1"), 0), sptField,
                     FxDB(drutama("ricustomdbl2"), 0), sptField,
                     FxDB(drutama("ricustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ricustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ricustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ricustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("ricabangnama"), ""), sptField,
                     FxDB(drutama("rilokasinama"), ""), sptField,
                     FxDB(drutama("rigudangnama"), ""), sptField,
                     FxDB(drutama("risupplierkode"), ""), sptField,
                     FxDB(drutama("risuppliernama"), ""), sptField,
                     FxDB(drutama("ribagianpembeliankode"), ""), sptField,
                     FxDB(drutama("ribagianpembeliannama"), ""), sptField,
                     FxDB(drutama("riterminnama"), ""), sptField,
                     FxDB(drutama("riterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("rirekdiskonnama"), ""), sptField,
                     FxDB(drutama("rirekpajak1nama"), ""), sptField,
                     FxDB(drutama("rirekpajak2nama"), ""), sptField,
                     FxDB(drutama("rirekbiayalainnama"), ""), sptField,
                     FxDB(drutama("rirekbayarnama"), ""), sptField,
                     FxDB(drutama("rinotransaksipo"), ""), sptField,
                     FxDB(drutama("rinotransaksiipc"), ""), sptField,
                     FxDB(drutama("rinotransaksigrn"), ""), sptField,
                     FxDB(drutama("ristatusnama"), ""), sptField,
                     FxDB(drutama("ristatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("riinputusernama"), ""), sptField,
                     FxDB(drutama("rimodifikasiusernama"), ""), sptField,
                     FxDB(drutama("kpkp"), 0), sptField,
                     FxDB(drutama("rijmluangmuka"), 0), sptField,
                     FxDB(drutama("rirekuangmuka"), ""), sptField,
                     FxDB(drutama("riidap"), 0), sptField,
                     FxDB(drutama("rirekuangmukanama"), ""), sptField,
                     FxDB(drutama("apnotransaksi"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idridetail"), 0), sptField,
                     FxDB(dr("idri"), 0), sptField,
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
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("diskon"), ""), sptField,
                     FxDB(dr("jmldiskon"), 0), sptField,
                     FxDB(dr("pajak1"), ""), sptField,
                     FxDB(dr("jmlpajak1"), 0), sptField,
                     FxDB(dr("pajak2"), ""), sptField,
                     FxDB(dr("jmlpajak2"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("rekpersediaan"), ""), sptField,
                     FxDB(dr("rekdiskonpembelian"), ""), sptField,
                     FxDB(dr("rekhutangsementara"), ""), sptField,
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
                     FxDB(dr("jmldnr"), 0), sptField,
                     FxDB(dr("statusdnr"), 0), sptField,
                     FxDB(dr("jmlprt"), 0), sptField,
                     FxDB(dr("statusprt"), 0), sptField,
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
                     FxDB(dr("basset"), 0), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("cabangnama"), ""), sptField,
                     FxDB(dr("lokasinama"), ""), sptField,
                     FxDB(dr("gudangnama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("ponotransaksi"), ""), sptField,
                     FxDB(dr("ipcnotransaksi"), ""), sptField,
                     FxDB(dr("grnnotransaksi"), ""), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)


            'AMBIL DATA BATCH
            sql = "select `nbt`.`nbtid` AS `nbtid`,`nbt`.`nbtjenismutasi` AS `nbtjenismutasi`,`nbt`.`nbtidbatchin` AS `nbtidbatchin`,`nbt`.`nbtgudang` AS `nbtgudang`,`nbt`.`nbtidbarang` AS `nbtidbarang`,`nbt`.`nbtkode` AS `nbtkode`,`nbt`.`nbtsumber` AS `nbtsumber`,`nbt`.`nbtidtransaksi` AS `nbtidtransaksi`,`nbt`.`nbtsatuan` AS `nbtsatuan`,`nbt`.`nbtjml` AS `nbtjml`,`nbt`.`nbtcustomtext1` AS `nbtcustomtext1`,`nbt`.`nbtcustomtext2` AS `nbtcustomtext2`,`nbt`.`nbtcustomtext3` AS `nbtcustomtext3`,`nbt`.`nbtcustomdbl1` AS `nbtcustomdbl1`,`nbt`.`nbtcustomdbl2` AS `nbtcustomdbl2`,`nbt`.`nbtcustomdbl3` AS `nbtcustomdbl3`,`nbt`.`nbtcustomdate1` AS `nbtcustomdate1`,`nbt`.`nbtcustomdate2` AS `nbtcustomdate2`,`nbt`.`nbtcustomdate3` AS `nbtcustomdate3`,`i`.`bkode` AS `kodebarang` from ((`m1_no_batch_transaction` `nbt` join `m1_item` `i` on((`nbt`.`nbtidbarang` = `i`.`bid`))) left join `m1_no_batch_in` `nbi` on((`nbt`.`nbtidbatchin` = `nbi`.`nbiidbatchin`)))"
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
                     FxDB(dr("kodebarang"), ""), sptRow)
            Next
            If batch.Length > 0 Then batch = batch.Substring(0, batch.Length - sptRow.Length) Else batch = batch

            'AMBIL DATA SERIAL
            sql = "select `nst`.`nstid` AS `nstid`,`nst`.`nstjenismutasi` AS `nstjenismutasi`,`nst`.`nstidserialin` AS `nstidserialin`,`nst`.`nstgudang` AS `nstgudang`,`nst`.`nstidbarang` AS `nstidbarang`,`nst`.`nstkode` AS `nstkode`,`nst`.`nstsumber` AS `nstsumber`,`nst`.`nstidtransaksi` AS `nstidtransaksi`,`nst`.`nstsatuan` AS `nstsatuan`,`nst`.`nstjml` AS `nstjml`,`nst`.`nstcustomtext1` AS `nstcustomtext1`,`nst`.`nstcustomtext2` AS `nstcustomtext2`,`nst`.`nstcustomtext3` AS `nstcustomtext3`,`nst`.`nstcustomdbl1` AS `nstcustomdbl1`,`nst`.`nstcustomdbl2` AS `nstcustomdbl2`,`nst`.`nstcustomdbl3` AS `nstcustomdbl3`,`nst`.`nstcustomdate1` AS `nstcustomdate1`,`nst`.`nstcustomdate2` AS `nstcustomdate2`,`nst`.`nstcustomdate3` AS `nstcustomdate3`,`i`.`bkode` AS `kodebarang` from ((`m1_no_serial_transaction` `nst` join `m1_item` `i` on((`nst`.`nstidbarang` = `i`.`bid`))) left join `m1_no_serial_in` `nsi` on((`nst`.`nstidserialin` = `nsi`.`nsiidserialin`)))"
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
                     FxDB(dr("kodebarang"), ""), sptRow)
            Next
            If serial.Length > 0 Then serial = serial.Substring(0, serial.Length - sptRow.Length) Else serial = serial


            'AMBIL DATA COST
            sql = "SELECT rc.idricost, rc.idri, rc.kodecost, rc.matauang, rc.kurs, rc.jumlah, rc.rekdebit, rc.rekkredit, rc.catatan, rc.costcenter, rc.divisi, rc.subdivisi, rc.proyek, rc.urutan, rc.idprcost, rc.idcscost, rc.idrqcost, rc.idbscost, rc.idpocost, rc.idipccost, rc.idgrncost, rc.jumlahbayar, rc.statusbayar, rc.isclose, rc.customtext1, rc.customtext2, rc.customtext3, rc.customdbl1, rc.customdbl2, rc.customdbl3, rc.customdate1, rc.customdate2, rc.customdate3, oc.ocnama as kodecostnama, coa1.cnama as rekdebitnama, coa2.cnama as rekkreditnama, cc.ccnama as costcenternama, d.dnama as divisinama, sd.sdnama as subdivisinama, p.pnama as proyeknama, rc.kontak, c.kkode as kontakkode, c.knama as kontaknama, rc.termasukhpp FROM m4_ri_cost rc JOIN m4_ri_history ri ON rc.idri = ri.riid LEFT JOIN m1_other_cost oc ON rc.kodecost = oc.ockode LEFT JOIN m1_coa coa1 ON rc.rekdebit = coa1.cnomor LEFT JOIN m1_coa coa2 ON rc.rekkredit = coa2.cnomor LEFT JOIN m1_cost_center cc ON rc.costcenter = cc.cckode LEFT JOIN m1_division d ON rc.divisi = d.dkode LEFT JOIN m1_subdivision sd ON rc.subdivisi = sd.sdkode LEFT JOIN m1_project p ON rc.proyek = p.pkode LEFT JOIN m1_contact c ON rc.kontak = c.kid"
            Dim dtcost As New DataTable
            dtcost = AmbilData("aplikasi1-m4_ri_cost", Filter, "rc.urutan", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtcost.Rows
                cost = String.Concat(cost,
                     FxDB(dr("idricost"), 0), sptField,
                     FxDB(dr("idri"), 0), sptField,
                     FxDB(dr("kodecost"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("jumlah"), 0), sptField,
                     FxDB(dr("rekdebit"), ""), sptField,
                     FxDB(dr("rekkredit"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idprcost"), 0), sptField,
                     FxDB(dr("idcscost"), 0), sptField,
                     FxDB(dr("idrqcost"), 0), sptField,
                     FxDB(dr("idbscost"), 0), sptField,
                     FxDB(dr("idpocost"), 0), sptField,
                     FxDB(dr("idipccost"), 0), sptField,
                     FxDB(dr("idgrncost"), 0), sptField,
                     FxDB(dr("jumlahbayar"), 0), sptField,
                     FxDB(dr("statusbayar"), 0), sptField,
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
                     FxDB(dr("kodecostnama"), ""), sptField,
                     FxDB(dr("rekdebitnama"), ""), sptField,
                     FxDB(dr("rekkreditnama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("kontak"), 0), sptField,
                     FxDB(dr("kontakkode"), ""), sptField,
                     FxDB(dr("kontaknama"), ""), sptField,
                     FxDB(dr("termasukhpp"), ""), sptRow)
            Next
            If cost.Length > 0 Then cost = cost.Substring(0, cost.Length - sptRow.Length) Else cost = cost


            'AMBIL DATA PAY
            'sql = "SELECT rip.idricarabayar AS idricarabayar, rip.idri AS idri, rip.carabayar AS carabayar, rip.matauang AS matauang, rip.kurs AS kurs, rip.jumlah AS jumlah, rip.jumlahvalas AS jumlahvalas, rip.nogiro AS nogiro, rip.tgljt AS tgljt, rip.bank AS bank, rip.noacbank AS noacbank, rip.rekbank AS rekbank, rip.rekgiro AS rekgiro, rip.catatan AS catatan, rip.urutan AS urutan, rip.isclose AS isclose, pm.nama AS carabayarnama, b.bnama AS banknama, coa1.cnama AS rekbanknama, coa2.cnama AS rekgironama, rip.sumber, rip.idtransaksi, rip.totaltransaksi, rip.terbayar, notransaksi, tgl FROM M4_ri_pay AS rip LEFT JOIN m0_payment_method AS pm ON rip.carabayar = pm.kode LEFT JOIN m1_bank AS b ON rip.bank = b.bkode LEFT JOIN m1_coa AS coa1 ON rip.rekbank = coa1.cnomor LEFT JOIN m1_coa AS coa2 ON rip.rekgiro = coa2.cnomor"
            sql = "SELECT rip.idricarabayar AS idricarabayar, rip.idri AS idri, rip.carabayar AS carabayar, rip.matauang AS matauang, rip.kurs AS kurs, rip.jumlah AS jumlah, rip.jumlahvalas AS jumlahvalas, rip.nogiro AS nogiro, rip.tgljt AS tgljt, rip.bank AS bank, rip.noacbank AS noacbank, rip.rekbank AS rekbank, rip.rekgiro AS rekgiro, rip.catatan AS catatan, rip.urutan AS urutan, rip.isclose AS isclose, pm.nama AS carabayarnama, b.bnama AS banknama, coa1.cnama AS rekbanknama, coa2.cnama AS rekgironama, rip.sumber, rip.idtransaksi, rip.totaltransaksi, rip.terbayar, ap.apnotransaksi as notransaksi, IFNULL(ap.aptgl,rip.tgljt) as tgl FROM M4_ri_pay AS rip LEFT JOIN m0_payment_method AS pm ON rip.carabayar = pm.kode LEFT JOIN m1_bank AS b ON rip.bank = b.bkode LEFT JOIN m1_coa AS coa1 ON rip.rekbank = coa1.cnomor LEFT JOIN m1_coa AS coa2 ON rip.rekgiro = coa2.cnomor LEFT JOIN m4_ap ap ON rip.sumber = ap.apsumber AND rip.idtransaksi = ap.apid"
            Dim dtpay As New DataTable
            dtpay = AmbilData("aplikasi1-M4_ri_Pay", "idri=" & idtransaksi, "idri ASC, urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtpay.Rows
                pay = String.Concat(pay,
                     FxDB(dr("idricarabayar"), 0), sptField,
                     FxDB(dr("idri"), 0), sptField,
                     FxDB(dr("carabayar"), 0), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("jumlah"), 0), sptField,
                     FxDB(dr("jumlahvalas"), 0), sptField,
                     FxDB(dr("nogiro"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgljt"), ""), formatTgl), sptField,
                     FxDB(dr("bank"), ""), sptField,
                     FxDB(dr("noacbank"), ""), sptField,
                     FxDB(dr("rekbank"), ""), sptField,
                     FxDB(dr("rekgiro"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("carabayarnama"), ""), sptField,
                     FxDB(dr("banknama"), ""), sptField,
                     FxDB(dr("rekbanknama"), ""), sptField,
                     FxDB(dr("rekgironama"), ""), sptField,
                     FxDB(dr("sumber"), ""), sptField,
                     FxDB(dr("idtransaksi"), 0), sptField,
                     FxDB(dr("totaltransaksi"), 0), sptField,
                     FxDB(dr("terbayar"), 0), sptField,
                     FxDB(dr("notransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptRow)
            Next
            If pay.Length > 0 Then pay = pay.Substring(0, pay.Length - sptRow.Length) Else pay = pay

            'AMBIL DATA ASSET
            'sql = "select atr.atid AS atid, atr.atasetid AS atasetid, atr.atjenismutasi AS atjenismutasi, atr.atsumber AS atsumber, atr.atidutama AS atidutama,atr.atidbarang AS atidbarang,atr.atkode AS atkode, atr.atnama AS atnama, atr.atkategori AS atkategori, atr.atcabang AS atcabang, atr.atlokasi AS atlokasi, atr.atgudang AS atgudang,atr.atdivisi AS atdivisi, atr.atsubdivisi AS atsubdivisi, atr.atcostcenter AS atcostcenter, atr.atproyek AS atproyek, atr.atcatatan AS atcatatan, atr.atnomor AS atnomor, atr.attglbeli AS attglbeli, atr.attglpakai AS attglpakai, atr.atjml AS atjml, atr.atsatuan AS atsatuan, atr.atmatauang AS atmatauang, atr.atkurs AS atkurs, atr.atharga AS atharga, atr.atdiskon AS atdiskon, atr.atjmldiskon AS atjmldiskon, atr.atpajak1 AS atpajak1, atr.atjmlpajak1 AS atjmlpajak1, atr.atpajak2 AS atpajak2, atr.atjmlpajak2 AS atjmlpajak2, atr.athargabeli AS athargabeli, atr.atnilairesidu AS atnilairesidu, atr.atumurekonomis AS atumurekonomis, atr.atbebanperbln AS atbebanperbln, atr.atakumulasibeban AS atakumulasibeban, atr.atnilaibuku AS atnilaibuku, (CASE WHEN atr.atnilaibuku < atr.atbebanperbln THEN atr.atnilaibuku ELSE atr.atbebanperbln END) as atnilaipenyusutan, atr.atmetode AS atmetode, atr.attabelpenyusutan AS attabelpenyusutan, atr.atintangible AS atintangible, atr.atfiskal AS atfiskal, atr.atatastengahbulan AS atatastengahbulan, atr.atrekasset AS atrekasset, atr.atrekakumdepresiasi AS atrekakumdepresiasi, atr.atrekdepresiasi AS atrekdepresiasi, atr.atrekpenghapusan AS atrekpenghapusan, atr.atprodusen AS atprodusen, atr.attglpensiun AS attglpensiun, atr.atpenyusutanke AS atpenyusutanke, atr.atnilaimenurun AS atnilaimenurun, atr.atdispose AS atdispose, atr.atpembelian AS atpembelian, atr.atpenjualan AS atpenjualan, atr.atlocked AS atlocked, atr.atstatus AS atstatus, atr.atstatussebelumnya AS atstatussebelumnya, atr.atisclose AS atisclose, atr.atinputuser AS atinputuser, atr.atinputtgl AS atinputtgl, atr.atmodifikasiuser AS atmodifikasiuser, atr.atmodifikasitgl AS atmodifikasitgl, atr.atcustomtext1,atr.atcustomtext2,atr.atcustomtext3,atr.atcustomtext4,atr.atcustomtext5,atr.atcustomint1,atr.atcustomint2,atr.atcustomint3,atr.atcustomint4,atr.atcustomint5,atr.atcustomdbl1,atr.atcustomdbl2,atr.atcustomdbl3,atr.atcustomdbl4,atr.atcustomdbl5,atr.atcustomdate1,atr.atcustomdate2,atr.atcustomdate3,atr.atcustomdate4,atr.atcustomdate5,ac.acnama AS atkategorinama, br.bnama AS atcabangnama, l.lnama AS atlokasinama, w.wnama AS atgudangnama,d.dnama AS atdivisinama, sd.sdnama AS atsubdivisinama, cc.ccnama AS atcostcenternama, p.pnama AS atproyeknama, dc.nama AS atmetodenama, t1.tnama AS atpajak1nama, ifnull(t1.tnilai, 0) AS atpajak1nilai, t2.tnama AS atpajak2nama, ifnull(t2.tnilai, 0) AS atpajak2nilai,coa1.cnama AS atrekassetnama, coa2.cnama AS atrekakumdepresiasinama, coa3.cnama AS atrekdepresiasinama, coa4.cnama AS atrekpenghapusannama, c1.kkode AS atprodusenkode, c1.knama AS atprodusennama,  sp1.nama AS atstatusnama,  sp2.nama AS atstatussebelumnyanama,  u1.unama AS atinputusernama,  u2.unama AS atmodifikasiusernama from m7_asset_transaction atr left join m7_asset_category ac on atr.atkategori = ac.ackode left join m1_branch br on atr.atcabang = br.bkode left join m1_location l on atr.atlokasi = l.lkode left join m1_warehouse w on atr.atgudang = w.wkode left join m1_division d on atr.atdivisi = d.dkode left join m1_subdivision sd on atr.atsubdivisi = sd.sdkode left join m7_depreciation_category dc on atr.atmetode = dc.kode left join m1_coa coa1 on atr.atrekasset = coa1.cnomor left join m1_coa coa2 on atr.atrekakumdepresiasi = coa2.cnomor left join m1_coa coa3 on atr.atrekdepresiasi = coa3.cnomor left join m1_coa coa4 on atr.atrekpenghapusan = coa4.cnomor left join m1_contact c1 on atr.atprodusen = c1.kid left join m0_status_progress sp1 on atr.atstatus = sp1.kode left join m0_status_progress sp2 on atr.atstatussebelumnya = sp2.kode left join m0_user u1 on atr.atinputuser = u1.userid left join m0_user u2 on atr.atmodifikasiuser = u2.userid left join m1_cost_center cc on atr.atcostcenter = cc.cckode left join m1_project p on atr.atproyek = p.pkode left join m1_tax t1 on atr.atpajak1 = t1.tkode left join m1_tax t2 on atr.atpajak2 = t2.tkode"
            sql = "select atr.atid AS atid, atr.atasetid AS atasetid, atr.atjenismutasi AS atjenismutasi, atr.atsumber AS atsumber, atr.atidutama AS atidutama,atr.atidbarang AS atidbarang,atr.atkode AS atkode, atr.atnama AS atnama, atr.atkategori AS atkategori, atr.atcabang AS atcabang, atr.atlokasi AS atlokasi, atr.atgudang AS atgudang,atr.atdivisi AS atdivisi, atr.atsubdivisi AS atsubdivisi, atr.atcostcenter AS atcostcenter, atr.atproyek AS atproyek, atr.atcatatan AS atcatatan, atr.atnomor AS atnomor, atr.attglbeli AS attglbeli, atr.attglpakai AS attglpakai, atr.atjml AS atjml, atr.atsatuan AS atsatuan, atr.atmatauang AS atmatauang, atr.atkurs AS atkurs, atr.atharga AS atharga, atr.atdiskon AS atdiskon, atr.atjmldiskon AS atjmldiskon, atr.atpajak1 AS atpajak1, atr.atjmlpajak1 AS atjmlpajak1, atr.atpajak2 AS atpajak2, atr.atjmlpajak2 AS atjmlpajak2, atr.athargabeli AS athargabeli, atr.atnilairesidu AS atnilairesidu, atr.atumurekonomis AS atumurekonomis, atr.atbebanperbln AS atbebanperbln, atr.atakumulasibeban AS atakumulasibeban, atr.atnilaibuku AS atnilaibuku, (CASE WHEN atr.atnilaibuku < atr.atbebanperbln THEN atr.atnilaibuku ELSE atr.atbebanperbln END) as atnilaipenyusutan, atr.atmetode AS atmetode, atr.attabelpenyusutan AS attabelpenyusutan, atr.atintangible AS atintangible, atr.atfiskal AS atfiskal, atr.atatastengahbulan AS atatastengahbulan, atr.atrekasset AS atrekasset, atr.atrekakumdepresiasi AS atrekakumdepresiasi, atr.atrekdepresiasi AS atrekdepresiasi, atr.atrekpenghapusan AS atrekpenghapusan, atr.atprodusen AS atprodusen, atr.attglpensiun AS attglpensiun, atr.atpenyusutanke AS atpenyusutanke, atr.atnilaimenurun AS atnilaimenurun, atr.atdispose AS atdispose, atr.atpembelian AS atpembelian, atr.atpenjualan AS atpenjualan, atr.atlocked AS atlocked, atr.atstatus AS atstatus, atr.atstatussebelumnya AS atstatussebelumnya, atr.atisclose AS atisclose, atr.atinputuser AS atinputuser, atr.atinputtgl AS atinputtgl, atr.atmodifikasiuser AS atmodifikasiuser, atr.atmodifikasitgl AS atmodifikasitgl, atr.atcustomtext1,atr.atcustomtext2,atr.atcustomtext3,atr.atcustomtext4,atr.atcustomtext5,atr.atcustomint1,atr.atcustomint2,atr.atcustomint3,atr.atcustomint4,atr.atcustomint5,atr.atcustomdbl1,atr.atcustomdbl2,atr.atcustomdbl3,atr.atcustomdbl4,atr.atcustomdbl5,atr.atcustomdate1,atr.atcustomdate2,atr.atcustomdate3,atr.atcustomdate4,atr.atcustomdate5,ac.acnama AS atkategorinama, br.bnama AS atcabangnama, l.lnama AS atlokasinama, w.wnama AS atgudangnama,d.dnama AS atdivisinama, sd.sdnama AS atsubdivisinama, cc.ccnama AS atcostcenternama, p.pnama AS atproyeknama, dc.nama AS atmetodenama, t1.tnama AS atpajak1nama, ifnull(t1.tnilai, 0) AS atpajak1nilai, t2.tnama AS atpajak2nama, ifnull(t2.tnilai, 0) AS atpajak2nilai,coa1.cnama AS atrekassetnama, coa2.cnama AS atrekakumdepresiasinama, coa3.cnama AS atrekdepresiasinama, coa4.cnama AS atrekpenghapusannama, c1.kkode AS atprodusenkode, c1.knama AS atprodusennama,  sp1.nama AS atstatusnama,  sp2.nama AS atstatussebelumnyanama,  u1.unama AS atinputusernama,  u2.unama AS atmodifikasiusernama, i.bkode as kodebarang from m7_asset_transaction atr left join m7_asset_category ac on atr.atkategori = ac.ackode left join m1_branch br on atr.atcabang = br.bkode left join m1_location l on atr.atlokasi = l.lkode left join m1_warehouse w on atr.atgudang = w.wkode left join m1_division d on atr.atdivisi = d.dkode left join m1_subdivision sd on atr.atsubdivisi = sd.sdkode left join m7_depreciation_category dc on atr.atmetode = dc.kode left join m1_coa coa1 on atr.atrekasset = coa1.cnomor left join m1_coa coa2 on atr.atrekakumdepresiasi = coa2.cnomor left join m1_coa coa3 on atr.atrekdepresiasi = coa3.cnomor left join m1_coa coa4 on atr.atrekpenghapusan = coa4.cnomor left join m1_contact c1 on atr.atprodusen = c1.kid left join m0_status_progress sp1 on atr.atstatus = sp1.kode left join m0_status_progress sp2 on atr.atstatussebelumnya = sp2.kode left join m0_user u1 on atr.atinputuser = u1.userid left join m0_user u2 on atr.atmodifikasiuser = u2.userid left join m1_cost_center cc on atr.atcostcenter = cc.cckode left join m1_project p on atr.atproyek = p.pkode left join m1_tax t1 on atr.atpajak1 = t1.tkode left join m1_tax t2 on atr.atpajak2 = t2.tkode JOIN m1_item i ON i.bid = atr.atidbarang"
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
                     FxDB(dr("atmodifikasiusernama"), ""), sptField,
                     FxDB(dr("kodebarang"), ""), sptRow)
            Next
            If asset.Length > 0 Then asset = asset.Substring(0, asset.Length - sptRow.Length) Else asset = asset


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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, batch, sptSubParam, serial, sptSubParam, cost, sptSubParam, pay, sptSubParam, asset)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("riid, ricabang, rilokasi, rigudang, riasalbarang, riasalbarangkategori, rijenispembelian, rijenispembeliankategori, ricarabayar, risumber, riautonotransaksi, rinotransaksi, ritgl, rikodepa, risupplier, risupplierkontak, ri1alamat1, ri1alamat2, ri1alamat3, ri2alamat1, ri2alamat2, ri2alamat3, ribagianpembelian, ritermin, ritgljatuhtempo, riuraian, ricatatan, rinoref, ritglnoref, ritglpenutupan, rimatauang, rikurs, rihargatermasukpajak, ritotal, ridiskonpersen, rijmldiskon, ritotalpajak1detail, ritotalpajak2detail, ribiayalainpersen, ribiayalain, ritotaltransaksi, rijmlbayar, ristatuslunas, ritgllunas, rinofakturpajak, risdhbayarpajak, ritglbayarpajak, rirekdiskon, rirekpajak1, rirekpajak2, rirekbiayalain, rirekbayar, riidpr, riidcs, riidrq, riidbs, riidpo, riidipc, riidgrn, ristatusdnr, ristatusprt, ristatusrealisasi, ristatus, ristatussebelumnya, rijmlrevisi, ricetakanke, riinputuser, riinputtgl, rimodifikasiuser, rimodifikasitgl, riposting, ripostingtgl, ritutupperiode, riisclose, ricustomtext1, ricustomtext2, ricustomtext3, ricustomtext4, ricustomtext5, ricustomint1, ricustomint2, ricustomint3, ricustomdbl1, ricustomdbl2, ricustomdbl3, ricustomdate1, ricustomdate2, ricustomdate3, ricabangnama, rilokasinama, rigudangnama, risupplierkode, risuppliernama, ribagianpembeliankode, ribagianpembeliannama, riterminnama, riterminharijatuhtempo, rirekdiskonnama, rirekpajak1nama, rirekpajak2nama, rirekbiayalainnama, rirekbayarnama, rinotransaksipo, rinotransaksiipc, rinotransaksigrn, ristatusnama, ristatussebelumnyanama, riinputusernama, rimodifikasiusernama, kpkp, rijmluangmuka, rirekuangmuka, riidap, rirekuangmukanama, apnotransaksi" & sptSubParam & "idridetail, idri, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, rekpersediaan, rekdiskonpembelian, rekhutangsementara, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, idgrndetail, jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, basset, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, ponotransaksi, ipcnotransaksi, grnnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan" & sptSubParam & "nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang" & sptSubParam & "nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang" & sptSubParam & "idricost, idri, kodecost, matauang, kurs, jumlah, rekdebit, rekkredit, catatan, costcenter, divisi, subdivisi, proyek, urutan, idprcost, idcscost, idrqcost, idbscost, idpocost, idipccost, idgrncost, jumlahbayar, statusbayar, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodecostnama, rekdebitnama, rekkreditnama, costcenternama, divisinama, subdivisinama, proyeknama, kontak, kontakkode, kontaknama, termasukhpp" & sptSubParam & "idricarabayar, idri, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose, carabayarnama, banknama, rekbanknama, rekgironama, sumber, idtransaksi, totaltransaksi, terbayar, notransaksi, tgl" & sptSubParam & "atid, atasetid, atjenismutasi, atsumber, atidutama, atidbarang, atkode, atnama, atkategori, atcabang, atlokasi, atgudang, atdivisi, atsubdivisi, atcostcenter, atproyek, atcatatan, atnomor, attglbeli, attglpakai, atjml, atsatuan, atmatauang, atkurs, atharga, atdiskon, atjmldiskon, atpajak1, atjmlpajak1, atpajak2, atjmlpajak2, athargabeli, atnilairesidu, atumurekonomis, atbebanperbln, atakumulasibeban, atnilaibuku, atnilaipenyusutan, atmetode, attabelpenyusutan, atintangible, atfiskal, atatastengahbulan, atrekasset, atrekakumdepresiasi, atrekdepresiasi, atrekpenghapusan, atprodusen, attglpensiun, atpenyusutanke, atnilaimenurun, atdispose, atpembelian, atpenjualan, atlocked, atstatus, atstatussebelumnya, atisclose, atinputuser, atinputtgl, atmodifikasiuser, atmodifikasitgl, atcustomtext1, atcustomtext2, atcustomtext3, atcustomtext4, atcustomtext5, atcustomint1, atcustomint2, atcustomint3, atcustomint4, atcustomint5, atcustomdbl1, atcustomdbl2, atcustomdbl3, atcustomdbl4, atcustomdbl5, atcustomdate1, atcustomdate2, atcustomdate3, atcustomdate4, atcustomdate5, atkategorinama, atcabangnama, atlokasinama, atgudangnama, atdivisinama, atsubdivisinama, atcostcenternama, atproyeknama, atmetodenama, atpajak1nama, atpajak1nilai, atpajak2nama, atpajak2nilai, atrekassetnama, atrekakumdepresiasinama, atrekdepresiasinama, atrekpenghapusannama, atprodusenkode, atprodusennama, atstatusnama, atstatussebelumnyanama, atinputusernama, atmodifikasiusernama, kodebarang"))

        Return wsResult
    End Function


    <WebMethod()>
    Public Function M4_RiHistoryGetdataById_lama(ByVal param As String) As String

        'M4_RiHistoryGetdataById Utama --------------------------------------------------------
        'riidhistory, riid, ricabang, rilokasi, rigudang, riasalbarang, riasalbarangkategori, rijenispembelian, 
        'rijenispembeliankategori, ricarabayar, risumber, riautonotransaksi, rinotransaksi, ritgl, rikodepa, 
        'risupplier, risupplierkontak, ri1alamat1, ri1alamat2, ri1alamat3, ri2alamat1, ri2alamat2, 
        'ri2alamat3, ribagianpembelian, ritermin, ritgljatuhtempo, riuraian, ricatatan, rinoref, 
        'ritglnoref, ritglpenutupan, rimatauang, rikurs, rihargatermasukpajak, ritotal, ridiskonpersen, 
        'rijmldiskon, ritotalpajak1detail, ritotalpajak2detail, ribiayalainpersen, ribiayalain, ritotaltransaksi, rijmlbayar, 
        'ristatuslunas, ritgllunas, rinofakturpajak, risdhbayarpajak, ritglbayarpajak, rirekdiskon, rirekpajak1, 
        'rirekpajak2, rirekbiayalain, rirekbayar, riidpr, riidcs, riidrq, riidbs, 
        'riidpo, riidipc, riidgrn, ristatusdnr, ristatusprt, ristatusrealisasi, ristatus, 
        'ristatussebelumnya, rijmlrevisi, ricetakanke, riinputuser, riinputtgl, rimodifikasiuser, rimodifikasitgl, 
        'riposting, ripostingtgl, ritutupperiode, riisclose, ricustomtext1, ricustomtext2, ricustomtext3, 
        'ricustomtext4, ricustomtext5, ricustomint1, ricustomint2, ricustomint3, ricustomdbl1, ricustomdbl2, 
        'ricustomdbl3, ricustomdate1, ricustomdate2, ricustomdate3, ricabangnama, rilokasinama, rigudangnama, 
        'risupplierkode, risuppliernama, ribagianpembeliankode, ribagianpembeliannama, riterminnama, riterminharijatuhtempo, rirekdiskonnama, 
        'rirekpajak1nama, rirekpajak2nama, rirekbiayalainnama, rirekbayarnama, rinotransaksipo, rinotransaksiipc, rinotransaksigrn, 
        'ristatusnama, ristatussebelumnyanama, riinputusernama, rimodifikasiusernama

        'M4_RiHistoryGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idridetail, idri, idbarang, 
        'namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, 
        'matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, 
        'jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, rekpersediaan, 
        'rekdiskonpembelian, rekhutangsementara, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, 
        'idgrndetail, jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, 
        'bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, 
        'gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, ponotransaksi, ipcnotransaksi, 
        'grnnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan

        'M4_RiHistoryGetdataById Batch --------------------------------------------------------
        'nbtidhistory, nbtidtransaksihistory, nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang

        'M4_RiHistoryGetdataById Serial --------------------------------------------------------
        'nstidhistory, nstidtransaksihistory, nstid, nstjenismutasi, nstidserialin, nstgudang,  nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, 
        'nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, 
        'nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang

        'M4_RiHistoryGetdataById Cost --------------------------------------------------------
        'idhistorycost, idhistory, idricost, idri, kodecost, matauang, kurs, jumlah, rekdebit, 
        'rekkredit, catatan, costcenter, divisi, subdivisi, proyek, urutan, 
        'idprcost, idcscost, idrqcost, idbscost, idpocost, idipccost, idgrncost, 
        'jumlahbayar, statusbayar, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3, kodecostnama, rekdebitnama, 
        'rekkreditnama, costcenternama, divisinama, subdivisinama, proyeknama, kontak, kontakkode, kontaknama, termasukhpp

        'M4_RiHistoryGetdataById Pay -------------------------------------------------------
        'idhistorycarabayar, idhistory, idricarabayar, idri, carabayar, matauang, 
        'kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, 
        'rekbank, rekgiro, catatan, urutan, isclose, carabayarnama, banknama, 
        'rekbanknama, rekgironama

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

        Dim utama As String = "", detail As String = "", batch As String = "", serial As String = "", cost As String = "", idtransaksi As String = ""
        Dim pay As String = "", sumber As String = "RI"

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

        Dim NmMemcached As String = "aplikasi1-M4_Ri_history~M4_Ri_Detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "riidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "riidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m4_ri_getdata_history")
        sql = "select ri.riidhistory, rid.idhistorydetail, rid.idhistory, ri.riid AS riid,ri.ricabang AS ricabang,ri.rilokasi AS rilokasi,ri.rigudang AS rigudang,ri.riasalbarang AS riasalbarang,ri.riasalbarangkategori AS riasalbarangkategori,ri.rijenispembelian AS rijenispembelian,ri.rijenispembeliankategori AS rijenispembeliankategori,ri.ricarabayar AS ricarabayar,ri.risumber AS risumber,ri.riautonotransaksi AS riautonotransaksi,ri.rinotransaksi AS rinotransaksi,ri.ritgl AS ritgl,ri.rikodepa AS rikodepa,ri.risupplier AS risupplier,ri.risupplierkontak AS risupplierkontak,ri.ri1alamat1 AS ri1alamat1,ri.ri1alamat2 AS ri1alamat2,ri.ri1alamat3 AS ri1alamat3,ri.ri2alamat1 AS ri2alamat1,ri.ri2alamat2 AS ri2alamat2,ri.ri2alamat3 AS ri2alamat3,ri.ribagianpembelian AS ribagianpembelian,ri.ritermin AS ritermin,ri.ritgljatuhtempo AS ritgljatuhtempo,ri.riuraian AS riuraian,ri.ricatatan AS ricatatan,ri.rinoref AS rinoref,ri.ritglnoref AS ritglnoref,ri.ritglpenutupan AS ritglpenutupan,ri.rimatauang AS rimatauang,ri.rikurs AS rikurs,ri.rihargatermasukpajak AS rihargatermasukpajak,ri.ritotal AS ritotal,ri.ridiskonpersen AS ridiskonpersen,ri.rijmldiskon AS rijmldiskon,ri.ritotalpajak1detail AS ritotalpajak1detail,ri.ritotalpajak2detail AS ritotalpajak2detail,ri.ribiayalainpersen AS ribiayalainpersen,ri.ribiayalain AS ribiayalain,ri.ritotaltransaksi AS ritotaltransaksi,ri.rijmlbayar AS rijmlbayar,ri.ristatuslunas AS ristatuslunas,ri.ritgllunas AS ritgllunas,ri.rinofakturpajak AS rinofakturpajak,ri.risdhbayarpajak AS risdhbayarpajak,ri.ritglbayarpajak AS ritglbayarpajak,ri.rirekdiskon AS rirekdiskon,ri.rirekpajak1 AS rirekpajak1,ri.rirekpajak2 AS rirekpajak2,ri.rirekbiayalain AS rirekbiayalain,ri.rirekbayar AS rirekbayar,ri.riidpr AS riidpr,ri.riidcs AS riidcs,ri.riidrq AS riidrq,ri.riidbs AS riidbs,ri.riidpo AS riidpo,ri.riidipc AS riidipc,ri.riidgrn AS riidgrn,ri.ristatusdnr AS ristatusdnr,ri.ristatusprt AS ristatusprt,ri.ristatusrealisasi AS ristatusrealisasi,ri.ristatus AS ristatus,ri.ristatussebelumnya AS ristatussebelumnya,ri.rijmlrevisi AS rijmlrevisi,ri.ricetakanke AS ricetakanke,ri.riinputuser AS riinputuser,ri.riinputtgl AS riinputtgl,ri.rimodifikasiuser AS rimodifikasiuser,ri.rimodifikasitgl AS rimodifikasitgl,ri.riposting AS riposting,ri.ripostingtgl AS ripostingtgl,ri.ritutupperiode AS ritutupperiode,ri.riisclose AS riisclose,ri.ricustomtext1 AS ricustomtext1,ri.ricustomtext2 AS ricustomtext2,ri.ricustomtext3 AS ricustomtext3,ri.ricustomtext4 AS ricustomtext4,ri.ricustomtext5 AS ricustomtext5,ri.ricustomint1 AS ricustomint1,ri.ricustomint2 AS ricustomint2,ri.ricustomint3 AS ricustomint3,ri.ricustomdbl1 AS ricustomdbl1,ri.ricustomdbl2 AS ricustomdbl2,ri.ricustomdbl3 AS ricustomdbl3,ri.ricustomdate1 AS ricustomdate1,ri.ricustomdate2 AS ricustomdate2,ri.ricustomdate3 AS ricustomdate3,br.bnama AS ricabangnama,lc.lnama AS rilokasinama,wh.wnama AS rigudangnama,c1.kkode AS risupplierkode,c1.knama AS risuppliernama,c2.kkode AS ribagianpembeliankode,c2.knama AS ribagianpembeliannama,tr.trnama AS riterminnama,tr.trharijatuhtempo AS riterminharijatuhtempo,coa1.cnama AS rirekdiskonnama,coa2.cnama AS rirekpajak1nama,coa3.cnama AS rirekpajak2nama,coa4.cnama AS rirekbiayalainnama,coa5.cnama AS rirekbayarnama,po.ponotransaksi AS rinotransaksipo,ipc.ipcnotransaksi AS rinotransaksiipc,grn.grnnotransaksi AS rinotransaksigrn,st1.nama AS ristatusnama,st2.nama AS ristatussebelumnyanama,u1.unama AS riinputusernama,u2.unama AS rimodifikasiusernama,rid.idridetail AS idridetail,rid.idri AS idri,rid.idbarang AS idbarang,rid.namabarang AS namabarang,rid.tipebarang AS tipebarang,rid.jml AS jml,rid.satuan AS satuan,rid.nilaisatuan AS nilaisatuan,rid.jmlbarang AS jmlbarang,rid.satuanbarang AS satuanbarang,rid.matauang AS matauang,rid.kurs AS kurs,rid.hargafix AS hargafix,rid.harga AS harga,rid.diskon AS diskon,rid.jmldiskon AS jmldiskon,rid.pajak1 AS pajak1,rid.jmlpajak1 AS jmlpajak1,rid.pajak2 AS pajak2,rid.jmlpajak2 AS jmlpajak2,rid.cabang AS cabang,rid.lokasi AS lokasi,rid.gudang AS gudang,i.brekpersediaan AS rekpersediaan,i.brekdiskonpembelian AS rekdiskonpembelian,rid.rekhutangsementara AS rekhutangsementara,rid.costcenter AS costcenter,rid.divisi AS divisi,rid.subdivisi AS subdivisi,rid.proyek AS proyek,rid.catatan AS catatan,rid.urutan AS urutan,rid.idprdetail AS idprdetail,rid.idcsdetail AS idcsdetail,rid.idrqdetail AS idrqdetail,rid.idbsdetail AS idbsdetail,rid.idpodetail AS idpodetail,rid.idipcdetail AS idipcdetail,rid.idgrndetail AS idgrndetail,rid.jmldnr AS jmldnr,rid.statusdnr AS statusdnr,rid.jmlprt AS jmlprt,rid.statusprt AS statusprt,rid.jmlrealisasi AS jmlrealisasi,rid.statusrealisasi AS statusrealisasi,rid.isclose AS isclose,rid.customtext1 AS customtext1,rid.customtext2 AS customtext2,rid.customtext3 AS customtext3,rid.customdbl1 AS customdbl1,rid.customdbl2 AS customdbl2,rid.customdbl3 AS customdbl3,rid.customdate1 AS customdate1,rid.customdate2 AS customdate2,rid.customdate3 AS customdate3,i.bkode AS kodebarang,i.bhpp AS bhpp,i.bjenis AS bjenis,i.bserial AS bserial,i.bbatch AS bbatch,i.basset AS basset,t1.tnama AS pajak1nama,t1.tnilai AS pajak1nilai,t2.tnama AS pajak2nama,t2.tnilai AS pajak2nilai,brd.bnama AS cabangnama,lcd.lnama AS lokasinama,whd.wnama AS gudangnama,cc.ccnama AS costcenternama,d.dnama AS divisinama,sd.sdnama AS subdivisinama,p.pnama AS proyeknama,po2.ponotransaksi AS ponotransaksi,ipc2.ipcnotransaksi AS ipcnotransaksi,grn2.grnnotransaksi AS grnnotransaksi, c1.kpkp, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan, ri.rijmluangmuka, ri.rirekuangmuka, ri.riidap, coa6.cnama as rirekuangmukanama, ap.apnotransaksi as apnotransaksi  from m4_ri_history ri join m4_ri_detail_history rid on ri.riid = rid.idri left join m1_branch br on br.bkode = ri.ricabang left join m1_location lc on lc.lkode = ri.rilokasi left join m1_warehouse wh on wh.wkode = ri.rigudang left join m1_contact c1 on c1.kid = ri.risupplier left join m1_contact c2 on c2.kid = ri.ribagianpembelian left join m1_terms tr on ri.ritermin = tr.trkode left join m1_coa coa1 on ri.rirekdiskon = coa1.cnomor left join m1_coa coa2 on ri.rirekpajak1 = coa2.cnomor left join m1_coa coa3 on ri.rirekpajak2 = coa3.cnomor left join m1_coa coa4 on ri.rirekbiayalain = coa4.cnomor left join m1_coa coa5 on ri.rirekbayar = coa5.cnomor left join m4_po po on ri.riidpo = po.poid left join m4_ipc ipc on ri.riidipc = ipc.ipcid left join m4_grn grn on ri.riidgrn = grn.grnid left join m0_status st1 on st1.kode = ri.ristatus left join m0_status st2 on st2.kode = ri.ristatussebelumnya left join m0_user u1 on u1.userid = ri.riinputuser left join m0_user u2 on u2.userid = ri.rimodifikasiuser left join m1_item i on i.bid = rid.idbarang left join m1_tax t1 on rid.pajak1 = t1.tkode left join m1_tax t2 on rid.pajak2 = t2.tkode left join m1_branch brd on rid.cabang = brd.bkode left join m1_location lcd on rid.lokasi = lcd.lkode left join m1_warehouse whd on rid.gudang = whd.wkode left join m1_project p on rid.proyek = p.pkode left join m4_po_detail pod on rid.idpodetail = pod.idpodetail left join m4_po po2 on pod.idpo = po2.poid left join m4_ipc_detail ipcd on rid.idipcdetail = ipcd.idipcdetail left join m4_ipc ipc2 on ipcd.idipc = ipc2.ipcid left join m4_grn_detail grnd on rid.idgrndetail = grnd.idgrndetail left join m4_grn grn2 on grnd.idgrn = grn2.grnid left join m1_cost_center cc on rid.costcenter = cc.cckode left join m1_division d on rid.divisi = d.dkode left join m1_subdivision sd on rid.subdivisi = sd.sdkode left join m1_coa coa6 on ri.rirekuangmuka = coa6.cnomor left join m4_ap ap on ri.riidap = ap.apid"


        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                     FxDB(drutama("riidhistory"), 0), sptField,
                     FxDB(drutama("riid"), 0), sptField,
                     FxDB(drutama("ricabang"), ""), sptField,
                     FxDB(drutama("rilokasi"), ""), sptField,
                     FxDB(drutama("rigudang"), ""), sptField,
                     FxDB(drutama("riasalbarang"), ""), sptField,
                     FxDB(drutama("riasalbarangkategori"), 0), sptField,
                     FxDB(drutama("rijenispembelian"), ""), sptField,
                     FxDB(drutama("rijenispembeliankategori"), 0), sptField,
                     FxDB(drutama("ricarabayar"), 0), sptField,
                     FxDB(drutama("risumber"), ""), sptField,
                     FxDB(drutama("riautonotransaksi"), 0), sptField,
                     FxDB(drutama("rinotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ritgl"), ""), formatTgl), sptField,
                     FxDB(drutama("rikodepa"), 0), sptField,
                     FxDB(drutama("risupplier"), 0), sptField,
                     FxDB(drutama("risupplierkontak"), ""), sptField,
                     FxDB(drutama("ri1alamat1"), ""), sptField,
                     FxDB(drutama("ri1alamat2"), ""), sptField,
                     FxDB(drutama("ri1alamat3"), ""), sptField,
                     FxDB(drutama("ri2alamat1"), ""), sptField,
                     FxDB(drutama("ri2alamat2"), ""), sptField,
                     FxDB(drutama("ri2alamat3"), ""), sptField,
                     FxDB(drutama("ribagianpembelian"), 0), sptField,
                     FxDB(drutama("ritermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ritgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("riuraian"), ""), sptField,
                     FxDB(drutama("ricatatan"), ""), sptField,
                     FxDB(drutama("rinoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ritglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ritglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("rimatauang"), ""), sptField,
                     FxDB(drutama("rikurs"), 0), sptField,
                     FxDB(drutama("rihargatermasukpajak"), 0), sptField,
                     FxDB(drutama("ritotal"), 0), sptField,
                     FxDB(drutama("ridiskonpersen"), ""), sptField,
                     FxDB(drutama("rijmldiskon"), 0), sptField,
                     FxDB(drutama("ritotalpajak1detail"), 0), sptField,
                     FxDB(drutama("ritotalpajak2detail"), 0), sptField,
                     FxDB(drutama("ribiayalainpersen"), ""), sptField,
                     FxDB(drutama("ribiayalain"), 0), sptField,
                     FxDB(drutama("ritotaltransaksi"), 0), sptField,
                     FxDB(drutama("rijmlbayar"), 0), sptField,
                     FxDB(drutama("ristatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ritgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("rinofakturpajak"), ""), sptField,
                     FxDB(drutama("risdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ritglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(drutama("rirekdiskon"), ""), sptField,
                     FxDB(drutama("rirekpajak1"), ""), sptField,
                     FxDB(drutama("rirekpajak2"), ""), sptField,
                     FxDB(drutama("rirekbiayalain"), ""), sptField,
                     FxDB(drutama("rirekbayar"), ""), sptField,
                     FxDB(drutama("riidpr"), 0), sptField,
                     FxDB(drutama("riidcs"), 0), sptField,
                     FxDB(drutama("riidrq"), 0), sptField,
                     FxDB(drutama("riidbs"), 0), sptField,
                     FxDB(drutama("riidpo"), 0), sptField,
                     FxDB(drutama("riidipc"), 0), sptField,
                     FxDB(drutama("riidgrn"), 0), sptField,
                     FxDB(drutama("ristatusdnr"), 0), sptField,
                     FxDB(drutama("ristatusprt"), 0), sptField,
                     FxDB(drutama("ristatusrealisasi"), 0), sptField,
                     FxDB(drutama("ristatus"), 0), sptField,
                     FxDB(drutama("ristatussebelumnya"), 0), sptField,
                     FxDB(drutama("rijmlrevisi"), 0), sptField,
                     FxDB(drutama("ricetakanke"), 0), sptField,
                     FxDB(drutama("riinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("riinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rimodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rimodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("riposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ripostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("ritutupperiode"), 0), sptField,
                     FxDB(drutama("riisclose"), 0), sptField,
                     FxDB(drutama("ricustomtext1"), ""), sptField,
                     FxDB(drutama("ricustomtext2"), ""), sptField,
                     FxDB(drutama("ricustomtext3"), ""), sptField,
                     FxDB(drutama("ricustomtext4"), ""), sptField,
                     FxDB(drutama("ricustomtext5"), ""), sptField,
                     FxDB(drutama("ricustomint1"), 0), sptField,
                     FxDB(drutama("ricustomint2"), 0), sptField,
                     FxDB(drutama("ricustomint3"), 0), sptField,
                     FxDB(drutama("ricustomdbl1"), 0), sptField,
                     FxDB(drutama("ricustomdbl2"), 0), sptField,
                     FxDB(drutama("ricustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ricustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ricustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ricustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("ricabangnama"), ""), sptField,
                     FxDB(drutama("rilokasinama"), ""), sptField,
                     FxDB(drutama("rigudangnama"), ""), sptField,
                     FxDB(drutama("risupplierkode"), ""), sptField,
                     FxDB(drutama("risuppliernama"), ""), sptField,
                     FxDB(drutama("ribagianpembeliankode"), ""), sptField,
                     FxDB(drutama("ribagianpembeliannama"), ""), sptField,
                     FxDB(drutama("riterminnama"), ""), sptField,
                     FxDB(drutama("riterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("rirekdiskonnama"), ""), sptField,
                     FxDB(drutama("rirekpajak1nama"), ""), sptField,
                     FxDB(drutama("rirekpajak2nama"), ""), sptField,
                     FxDB(drutama("rirekbiayalainnama"), ""), sptField,
                     FxDB(drutama("rirekbayarnama"), ""), sptField,
                     FxDB(drutama("rinotransaksipo"), ""), sptField,
                     FxDB(drutama("rinotransaksiipc"), ""), sptField,
                     FxDB(drutama("rinotransaksigrn"), ""), sptField,
                     FxDB(drutama("ristatusnama"), ""), sptField,
                     FxDB(drutama("ristatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("riinputusernama"), ""), sptField,
                     FxDB(drutama("rimodifikasiusernama"), ""), sptField,
                     FxDB(drutama("kpkp"), 0), sptField,
                     FxDB(drutama("rijmluangmuka"), 0), sptField,
                     FxDB(drutama("rirekuangmuka"), ""), sptField,
                     FxDB(drutama("riidap"), 0), sptField,
                     FxDB(drutama("rirekuangmukanama"), ""), sptField,
                     FxDB(drutama("apnotransaksi"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idhistorydetail"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idridetail"), 0), sptField,
                     FxDB(dr("idri"), 0), sptField,
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
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("diskon"), ""), sptField,
                     FxDB(dr("jmldiskon"), 0), sptField,
                     FxDB(dr("pajak1"), ""), sptField,
                     FxDB(dr("jmlpajak1"), 0), sptField,
                     FxDB(dr("pajak2"), ""), sptField,
                     FxDB(dr("jmlpajak2"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("rekpersediaan"), ""), sptField,
                     FxDB(dr("rekdiskonpembelian"), ""), sptField,
                     FxDB(dr("rekhutangsementara"), ""), sptField,
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
                     FxDB(dr("jmldnr"), 0), sptField,
                     FxDB(dr("statusdnr"), 0), sptField,
                     FxDB(dr("jmlprt"), 0), sptField,
                     FxDB(dr("statusprt"), 0), sptField,
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
                     FxDB(dr("gudangnama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("ponotransaksi"), ""), sptField,
                     FxDB(dr("ipcnotransaksi"), ""), sptField,
                     FxDB(dr("grnnotransaksi"), ""), sptField,
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
                batch = String.Concat(batch,
                     FxDB(dr("nbtidhistory"), 0), sptField,
                     FxDB(dr("nbtidtransaksihistory"), 0), sptField,
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
                serial = String.Concat(serial,
                     FxDB(dr("nstidhistory"), 0), sptField,
                     FxDB(dr("nstidtransaksihistory"), 0), sptField,
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


            'AMBIL DATA COST
            sql = "SELECT rc.idhistorycost, rc.idhistory, rc.idricost, rc.idri, rc.kodecost, rc.matauang, rc.kurs, rc.jumlah, rc.rekdebit, rc.rekkredit, rc.catatan, rc.costcenter, rc.divisi, rc.subdivisi, rc.proyek, rc.urutan, rc.idprcost, rc.idcscost, rc.idrqcost, rc.idbscost, rc.idpocost, rc.idipccost, rc.idgrncost, rc.jumlahbayar, rc.statusbayar, rc.isclose, rc.customtext1, rc.customtext2, rc.customtext3, rc.customdbl1, rc.customdbl2, rc.customdbl3, rc.customdate1, rc.customdate2, rc.customdate3, oc.ocnama as kodecostnama, coa1.cnama as rekdebitnama, coa2.cnama as rekkreditnama, cc.ccnama as costcenternama, d.dnama as divisinama, sd.sdnama as subdivisinama, p.pnama as proyeknama, rc.kontak, c.kkode as kontakkode, c.knama as kontaknama, rc.termasukhpp FROM m4_ri_cost_history rc JOIN m4_ri_history ri ON rc.idhistory = ri.riidhistory LEFT JOIN m1_other_cost oc ON rc.kodecost = oc.ockode LEFT JOIN m1_coa coa1 ON rc.rekdebit = coa1.cnomor LEFT JOIN m1_coa coa2 ON rc.rekkredit = coa2.cnomor LEFT JOIN m1_cost_center cc ON rc.costcenter = cc.cckode LEFT JOIN m1_division d ON rc.divisi = d.dkode LEFT JOIN m1_subdivision sd ON rc.subdivisi = sd.sdkode LEFT JOIN m1_project p ON rc.proyek = p.pkode LEFT JOIN m1_contact c ON rc.kontak = c.kid"
            Dim dtcost As New DataTable
            dtcost = AmbilData("aplikasi1-m4_ri_cost", "rc.idhistory = " & idtransaksi, "rc.urutan", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtcost.Rows
                cost = String.Concat(cost,
                     FxDB(dr("idhistorycost"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idricost"), 0), sptField,
                     FxDB(dr("idri"), 0), sptField,
                     FxDB(dr("kodecost"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("jumlah"), 0), sptField,
                     FxDB(dr("rekdebit"), ""), sptField,
                     FxDB(dr("rekkredit"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idprcost"), 0), sptField,
                     FxDB(dr("idcscost"), 0), sptField,
                     FxDB(dr("idrqcost"), 0), sptField,
                     FxDB(dr("idbscost"), 0), sptField,
                     FxDB(dr("idpocost"), 0), sptField,
                     FxDB(dr("idipccost"), 0), sptField,
                     FxDB(dr("idgrncost"), 0), sptField,
                     FxDB(dr("jumlahbayar"), 0), sptField,
                     FxDB(dr("statusbayar"), 0), sptField,
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
                     FxDB(dr("kodecostnama"), ""), sptField,
                     FxDB(dr("rekdebitnama"), ""), sptField,
                     FxDB(dr("rekkreditnama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("kontak"), 0), sptField,
                     FxDB(dr("kontakkode"), ""), sptField,
                     FxDB(dr("kontaknama"), ""), sptField,
                     FxDB(dr("termasukhpp"), ""), sptRow)
            Next
            If cost.Length > 0 Then cost = cost.Substring(0, cost.Length - sptRow.Length) Else cost = cost


            'AMBIL DATA PAY
            sql = "SELECT rip.idhistorycarabayar, rip.idhistory, rip.idricarabayar AS idricarabayar, rip.idri AS idri, rip.carabayar AS carabayar, rip.matauang AS matauang, rip.kurs AS kurs, rip.jumlah AS jumlah, rip.jumlahvalas AS jumlahvalas, rip.nogiro AS nogiro, rip.tgljt AS tgljt, rip.bank AS bank, rip.noacbank AS noacbank, rip.rekbank AS rekbank, rip.rekgiro AS rekgiro, rip.catatan AS catatan, rip.urutan AS urutan, rip.isclose AS isclose, pm.nama AS carabayarnama, b.bnama AS banknama, coa1.cnama AS rekbanknama, coa2.cnama AS rekgironama FROM m4_ri_pay_history AS rip LEFT JOIN m0_payment_method AS pm ON rip.carabayar = pm.kode LEFT JOIN m1_bank AS b ON rip.bank = b.bkode LEFT JOIN m1_coa AS coa1 ON rip.rekbank = coa1.cnomor LEFT JOIN m1_coa AS coa2 ON rip.rekgiro = coa2.cnomor"
            Dim dtpay As New DataTable
            dtpay = AmbilData("aplikasi1-m4_ri_Pay", "idhistory=" & idtransaksi, "idhistory ASC, urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtpay.Rows
                pay = String.Concat(pay,
                     FxDB(dr("idhistorycarabayar"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idricarabayar"), 0), sptField,
                     FxDB(dr("idri"), 0), sptField,
                     FxDB(dr("carabayar"), 0), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("jumlah"), 0), sptField,
                     FxDB(dr("jumlahvalas"), 0), sptField,
                     FxDB(dr("nogiro"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgljt"), ""), formatTgl), sptField,
                     FxDB(dr("bank"), ""), sptField,
                     FxDB(dr("noacbank"), ""), sptField,
                     FxDB(dr("rekbank"), ""), sptField,
                     FxDB(dr("rekgiro"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("carabayarnama"), ""), sptField,
                     FxDB(dr("banknama"), ""), sptField,
                     FxDB(dr("rekbanknama"), ""), sptField,
                     FxDB(dr("rekgironama"), ""), sptRow)
            Next
            If pay.Length > 0 Then pay = pay.Substring(0, pay.Length - sptRow.Length) Else pay = pay


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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("riidhistory, riid, ricabang, rilokasi, rigudang, riasalbarang, riasalbarangkategori, rijenispembelian, rijenispembeliankategori, ricarabayar, risumber, riautonotransaksi, rinotransaksi, ritgl, rikodepa, risupplier, risupplierkontak, ri1alamat1, ri1alamat2, ri1alamat3, ri2alamat1, ri2alamat2, ri2alamat3, ribagianpembelian, ritermin, ritgljatuhtempo, riuraian, ricatatan, rinoref, ritglnoref, ritglpenutupan, rimatauang, rikurs, rihargatermasukpajak, ritotal, ridiskonpersen, rijmldiskon, ritotalpajak1detail, ritotalpajak2detail, ribiayalainpersen, ribiayalain, ritotaltransaksi, rijmlbayar, ristatuslunas, ritgllunas, rinofakturpajak, risdhbayarpajak, ritglbayarpajak, rirekdiskon, rirekpajak1, rirekpajak2, rirekbiayalain, rirekbayar, riidpr, riidcs, riidrq, riidbs, riidpo, riidipc, riidgrn, ristatusdnr, ristatusprt, ristatusrealisasi, ristatus, ristatussebelumnya, rijmlrevisi, ricetakanke, riinputuser, riinputtgl, rimodifikasiuser, rimodifikasitgl, riposting, ripostingtgl, ritutupperiode, riisclose, ricustomtext1, ricustomtext2, ricustomtext3, ricustomtext4, ricustomtext5, ricustomint1, ricustomint2, ricustomint3, ricustomdbl1, ricustomdbl2, ricustomdbl3, ricustomdate1, ricustomdate2, ricustomdate3, ricabangnama, rilokasinama, rigudangnama, risupplierkode, risuppliernama, ribagianpembeliankode, ribagianpembeliannama, riterminnama, riterminharijatuhtempo, rirekdiskonnama, rirekpajak1nama, rirekpajak2nama, rirekbiayalainnama, rirekbayarnama, rinotransaksipo, rinotransaksiipc, rinotransaksigrn, ristatusnama, ristatussebelumnyanama, riinputusernama, rimodifikasiusernama, kpkp, rijmluangmuka, rirekuangmuka, riidap, rirekuangmukanama, apnotransaksi" & sptSubParam & "idhistorydetail, idhistory, idridetail, idri, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, rekpersediaan, rekdiskonpembelian, rekhutangsementara, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, idgrndetail, jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, ponotransaksi, ipcnotransaksi, grnnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan" & sptSubParam & "nbtidhistory, nbtidtransaksihistory, nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang" & sptSubParam & "nstidhistory, nstidtransaksihistory, nstid, nstjenismutasi, nstidserialin, nstgudang,  nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang" & sptSubParam & "idhistorycost, idhistory, idricost, idri, kodecost, matauang, kurs, jumlah, rekdebit, rekkredit, catatan, costcenter, divisi, subdivisi, proyek, urutan, idprcost, idcscost, idrqcost, idbscost, idpocost, idipccost, idgrncost, jumlahbayar, statusbayar, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodecostnama, rekdebitnama, rekkreditnama, costcenternama, divisinama, subdivisinama, proyeknama, kontak, kontakkode, kontaknama, termasukhpp" & sptSubParam & "idhistorycarabayar, idhistory, idricarabayar, idri, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose, carabayarnama, banknama, rekbanknama, rekgironama"))

        Return wsResult
    End Function

End Class
