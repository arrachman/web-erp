Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_sr_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function m5_Sr_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m5_sr_history(SELECT 0, sr.* FROM m5_sr sr WHERE sr.srid = '" & idtransaksi & "')"
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
            sql = "SELECT sridhistory FROM m5_sr_history WHERE srid = '" & idtransaksi & "' ORDER BY srmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m5_sr_detail_history (SELECT 0, '" & result(4) & "', sr.* FROM m5_sr_detail sr WHERE sr.idsr = '" & idtransaksi & "' )"
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
            sql = "INSERT INTO m1_no_batch_transaction_history(SELECT 0, '" & result(4) & "', nb.* FROM m1_no_batch_transaction nb WHERE nb.nbtidtransaksi = '" & idtransaksi & "' and nb.nbtsumber = 'SR')"
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
            sql = "INSERT INTO m1_no_serial_transaction_history(SELECT 0, '" & result(4) & "', ns.* FROM m1_no_serial_transaction ns WHERE ns.nstidtransaksi = '" & idtransaksi & "' and ns.nstsumber = 'SR')"
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
            sql = "INSERT INTO m7_asset_transaction_history(SELECT 0, '" & result(4) & "', atr.* FROM m7_asset_transaction atr WHERE atr.atidutama = '" & idtransaksi & "' and atr.atsumber = 'SR')"
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
    Public Function M5_Sr_HistoryBSearch(ByVal param As String) As String
        'M5_SrBSearch --------------------------------------------------------
        'srid, srcabang, srlokasi, srgudang, srasalbarang, srasalbarangkategori, srjenispenjulan, 
        'srjenispenjualankategori, srcarabayar, srsumber, srautonotransaksi, srnotransaksi, srtgl, srkodepa, 
        'srcustomer, srcustomerkontak, sr1alamat1, sr1alamat2, sr1alamat3, sr2alamat1, sr2alamat2, 
        'sr2alamat3, srbagianpenjualan, srekspedisi, srtglkirim, srtermin, srtgljatuhtempo, sruraian, 
        'srcatatan, srnoref, srtglnoref, srtglpenutupan, srmatauang, srkurs, srhargatermasukpajak, 
        'srtotal, srdiskonpersen, srjmldiskon, srtotalpajak1detail, srtotalpajak2detail, srbiayalainpersen, srbiayalain, 
        'srtotaltransaksi, srsisatransaksi, srjmlbayar, srstatuslunas, srtgllunas, srnofakturpajak, srsdhbayarpajak, 
        'srtglbayarpajak, srrekdiskon, srrekpajak1, srrekpajak2, srrekbiayalain, srreksisa, srrekbayar, 
        'sridsq, sridso, sridpl, sriddo, sriddr, sridpi, sridsi, 
        'sridrnr, srstatus, srstatussebelumnya, srjmlrevisi, srcetakanke, srinputuser, srinputtgl, 
        'srmodifikasiuser, srmodifikasitgl, srposting, srpostingtgl, srtutupperiode, srisclose, srcabangnama, 
        'srlokasinama, srgudangnama, srcustomerkode, srcustomernama, srbagianpenjualankode, srbagianpenjualannama, srekspedisinama, 
        'sinotransaksi, rnrnotransaksi, srstatusnama, srstatussebelumnyanama, srinputusernama, srmodifikasiusernama, srcustomtext1, stcustomtext2
        'srcustomtext3, srcustomtext4, srcustomtext5, srcustomint1, srcustomint2, srcustomint3, srcustomdbl1, srcustomdbl2, srcustomdbl3,
        'srcustomdate1, srcustomdate2, srcustomdate3

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
                Filter = "sridhistory = " + paramSplit(3)
            End If
        End If
        'END OF VALIDASI PARAMETER PAGING ==================================================

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
            Filter = Filter.Replace("srcustomerkode", "c1.kkode")
            Filter = Filter.Replace("srcustomernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select sr.sridhistory, `sr`.`srid` AS `srid`,`sr`.`srcabang` AS `srcabang`,`sr`.`srlokasi` AS `srlokasi`,`sr`.`srgudang` AS `srgudang`,`sr`.`srasalbarang` AS `srasalbarang`,`sr`.`srasalbarangkategori` AS `srasalbarangkategori`,`sr`.`srjenispenjulan` AS `srjenispenjulan`,`sr`.`srjenispenjualankategori` AS `srjenispenjualankategori`,`sr`.`srcarabayar` AS `srcarabayar`,`sr`.`srsumber` AS `srsumber`,`sr`.`srautonotransaksi` AS `srautonotransaksi`,`sr`.`srnotransaksi` AS `srnotransaksi`,`sr`.`srtgl` AS `srtgl`,`sr`.`srkodepa` AS `srkodepa`,`sr`.`srcustomer` AS `srcustomer`,`sr`.`srcustomerkontak` AS `srcustomerkontak`,`sr`.`sr1alamat1` AS `sr1alamat1`,`sr`.`sr1alamat2` AS `sr1alamat2`,`sr`.`sr1alamat3` AS `sr1alamat3`,`sr`.`sr2alamat1` AS `sr2alamat1`,`sr`.`sr2alamat2` AS `sr2alamat2`,`sr`.`sr2alamat3` AS `sr2alamat3`,`sr`.`srbagianpenjualan` AS `srbagianpenjualan`,`sr`.`srekspedisi` AS `srekspedisi`,`sr`.`srtglkirim` AS `srtglkirim`,`sr`.`srtermin` AS `srtermin`,`sr`.`srtgljatuhtempo` AS `srtgljatuhtempo`,`sr`.`sruraian` AS `sruraian`,`sr`.`srcatatan` AS `srcatatan`,`sr`.`srnoref` AS `srnoref`,`sr`.`srtglnoref` AS `srtglnoref`,`sr`.`srtglpenutupan` AS `srtglpenutupan`,`sr`.`srmatauang` AS `srmatauang`,`sr`.`srkurs` AS `srkurs`,`sr`.`srhargatermasukpajak` AS `srhargatermasukpajak`,`sr`.`srtotal` AS `srtotal`,`sr`.`srdiskonpersen` AS `srdiskonpersen`,`sr`.`srjmldiskon` AS `srjmldiskon`,`sr`.`srtotalpajak1detail` AS `srtotalpajak1detail`,`sr`.`srtotalpajak2detail` AS `srtotalpajak2detail`,`sr`.`srbiayalainpersen` AS `srbiayalainpersen`,`sr`.`srbiayalain` AS `srbiayalain`,`sr`.`srtotaltransaksi` AS `srtotaltransaksi`,`sr`.`srsisatransaksi` AS `srsisatransaksi`,`sr`.`srjmlbayar` AS `srjmlbayar`,`sr`.`srstatuslunas` AS `srstatuslunas`,`sr`.`srtgllunas` AS `srtgllunas`,`sr`.`srnofakturpajak` AS `srnofakturpajak`,`sr`.`srsdhbayarpajak` AS `srsdhbayarpajak`,`sr`.`srtglbayarpajak` AS `srtglbayarpajak`,`sr`.`srrekdiskon` AS `srrekdiskon`,`sr`.`srrekpajak1` AS `srrekpajak1`,`sr`.`srrekpajak2` AS `srrekpajak2`,`sr`.`srrekbiayalain` AS `srrekbiayalain`,`sr`.`srreksisa` AS `srreksisa`,`sr`.`srrekbayar` AS `srrekbayar`,`sr`.`sridsq` AS `sridsq`,`sr`.`sridso` AS `sridso`,`sr`.`sridpl` AS `sridpl`,`sr`.`sriddo` AS `sriddo`,`sr`.`sriddr` AS `sriddr`,`sr`.`sridpi` AS `sridpi`,`sr`.`sridsi` AS `sridsi`,`sr`.`sridrnr` AS `sridrnr`,`sr`.`srstatus` AS `srstatus`,`sr`.`srstatussebelumnya` AS `srstatussebelumnya`,`sr`.`srjmlrevisi` AS `srjmlrevisi`,`sr`.`srcetakanke` AS `srcetakanke`,`sr`.`srinputuser` AS `srinputuser`,`sr`.`srinputtgl` AS `srinputtgl`,`sr`.`srmodifikasiuser` AS `srmodifikasiuser`,`sr`.`srmodifikasitgl` AS `srmodifikasitgl`,`sr`.`srposting` AS `srposting`,`sr`.`srpostingtgl` AS `srpostingtgl`,`sr`.`srtutupperiode` AS `srtutupperiode`,`sr`.`srisclose` AS `srisclose`,`br`.`bnama` AS `srcabangnama`,`lc`.`lnama` AS `srlokasinama`,`wh`.`wnama` AS `srgudangnama`,`c1`.`kkode` AS `srcustomerkode`,`c1`.`knama` AS `srcustomernama`,`c2`.`kkode` AS `srbagianpenjualankode`,`c2`.`knama` AS `srbagianpenjualannama`,`e`.`enama` AS `srekspedisinama`,`si`.`sinotransaksi` AS `sinotransaksi`,`rnr`.`rnrnotransaksi` AS `rnrnotransaksi`,`st1`.`nama` AS `srstatusnama`,`st2`.`nama` AS `srstatussebelumnyanama`,`u1`.`unama` AS `srinputusernama`,`u2`.`unama` AS `srmodifikasiusernama`, `sr`.`srcustomtext1` AS `srcustomtext1`, `sr`.`srcustomtext2` AS `srcustomtext2`, `sr`.`srcustomtext3` AS `srcustomtext3`, `sr`.`srcustomtext4` AS `srcustomtext4`, `sr`.`srcustomtext5` AS `srcustomtext5`, `sr`.`srcustomint1` AS `srcustomint1`, `sr`.`srcustomint2` AS `srcustomint2`, `sr`.`srcustomint3` AS `srcustomint3`, `sr`.`srcustomdbl1` AS `srcustomdbl1`, `sr`.`srcustomdbl2` AS `srcustomdbl2`, `sr`.`srcustomdbl3` AS `srcustomdbl3`, `sr`.`srcustomdate1` AS `srcustomdate1`, `sr`.`srcustomdate2` AS `srcustomdate2`, `sr`.`srcustomdate3` AS `srcustomdate3`, cdis.cnama AS srrekdiskonnama, cpa.cnama AS srrekpajak1nama, cpa2.cnama AS srrekpajak2nama, cba.cnama AS srrekbiayalainnama from ((((((((((((`m5_sr_history` `sr` left join `m1_branch` `br` on((`br`.`bkode` = `sr`.`srcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `sr`.`srlokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `sr`.`srgudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `sr`.`srcustomer`))) left join `m1_contact` `c2` on((`c2`.`kid` = `sr`.`srbagianpenjualan`))) left join `m1_expedition` `e` on((`sr`.`srekspedisi` = `e`.`ekode`))) left join `m5_si` `si` on((`sr`.`sridsi` = `si`.`siid`))) left join `m5_rnr` `rnr` on((`sr`.`sridrnr` = `rnr`.`rnrid`))) left join `m0_status` `st1` on((`st1`.`kode` = `sr`.`srstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `sr`.`srstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `sr`.`srinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `sr`.`srmodifikasiuser`))) LEFT JOIN m1_coa cdis ON cdis.cnomor = sr.srrekdiskon LEFT JOIN m1_coa cpa ON cpa.cnomor = sr.srrekpajak1 LEFT JOIN m1_coa cpa2 ON cpa2.cnomor = sr.srrekpajak2 LEFT JOIN m1_coa cba ON cba.cnomor = sr.srrekbiayalain"

        dt = AmbilData("aplikasi1-M5_Sr", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("srid"), 0), sptField,
                     FxDB(dr("sridhistory"), 0), sptField,
                     FxDB(dr("srcabang"), ""), sptField,
                     FxDB(dr("srlokasi"), ""), sptField,
                     FxDB(dr("srgudang"), ""), sptField,
                     FxDB(dr("srasalbarang"), ""), sptField,
                     FxDB(dr("srasalbarangkategori"), 0), sptField,
                     FxDB(dr("srjenispenjulan"), ""), sptField,
                     FxDB(dr("srjenispenjualankategori"), 0), sptField,
                     FxDB(dr("srcarabayar"), 0), sptField,
                     FxDB(dr("srsumber"), ""), sptField,
                     FxDB(dr("srautonotransaksi"), 0), sptField,
                     FxDB(dr("srnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("srtgl"), ""), formatTgl), sptField,
                     FxDB(dr("srkodepa"), 0), sptField,
                     FxDB(dr("srcustomer"), 0), sptField,
                     FxDB(dr("srcustomerkontak"), ""), sptField,
                     FxDB(dr("sr1alamat1"), ""), sptField,
                     FxDB(dr("sr1alamat2"), ""), sptField,
                     FxDB(dr("sr1alamat3"), ""), sptField,
                     FxDB(dr("sr2alamat1"), ""), sptField,
                     FxDB(dr("sr2alamat2"), ""), sptField,
                     FxDB(dr("sr2alamat3"), ""), sptField,
                     FxDB(dr("srbagianpenjualan"), 0), sptField,
                     FxDB(dr("srekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("srtglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("srtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("srtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("sruraian"), ""), sptField,
                     FxDB(dr("srcatatan"), ""), sptField,
                     FxDB(dr("srnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("srtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("srtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("srmatauang"), ""), sptField,
                     FxDB(dr("srkurs"), 0), sptField,
                     FxDB(dr("srhargatermasukpajak"), 0), sptField,
                     FxDB(dr("srtotal"), 0), sptField,
                     FxDB(dr("srdiskonpersen"), ""), sptField,
                     FxDB(dr("srjmldiskon"), 0), sptField,
                     FxDB(dr("srtotalpajak1detail"), 0), sptField,
                     FxDB(dr("srtotalpajak2detail"), 0), sptField,
                     FxDB(dr("srbiayalainpersen"), 0), sptField,
                     FxDB(dr("srbiayalain"), 0), sptField,
                     FxDB(dr("srtotaltransaksi"), 0), sptField,
                     FxDB(dr("srsisatransaksi"), 0), sptField,
                     FxDB(dr("srjmlbayar"), 0), sptField,
                     FxDB(dr("srstatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("srtgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("srnofakturpajak"), ""), sptField,
                     FxDB(dr("srsdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("srtglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(dr("srrekdiskon"), ""), sptField,
                     FxDB(dr("srrekpajak1"), ""), sptField,
                     FxDB(dr("srrekpajak2"), ""), sptField,
                     FxDB(dr("srrekbiayalain"), ""), sptField,
                     FxDB(dr("srreksisa"), ""), sptField,
                     FxDB(dr("srrekbayar"), ""), sptField,
                     FxDB(dr("sridsq"), 0), sptField,
                     FxDB(dr("sridso"), 0), sptField,
                     FxDB(dr("sridpl"), 0), sptField,
                     FxDB(dr("sriddo"), 0), sptField,
                     FxDB(dr("sriddr"), 0), sptField,
                     FxDB(dr("sridpi"), 0), sptField,
                     FxDB(dr("sridsi"), 0), sptField,
                     FxDB(dr("sridrnr"), 0), sptField,
                     FxDB(dr("srstatus"), 0), sptField,
                     FxDB(dr("srstatussebelumnya"), 0), sptField,
                     FxDB(dr("srjmlrevisi"), 0), sptField,
                     FxDB(dr("srcetakanke"), 0), sptField,
                     FxDB(dr("srinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("srinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("srmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("srmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("srposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("srpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("srtutupperiode"), 0), sptField,
                     FxDB(dr("srisclose"), 0), sptField,
                     FxDB(dr("srcabangnama"), ""), sptField,
                     FxDB(dr("srlokasinama"), ""), sptField,
                     FxDB(dr("srgudangnama"), ""), sptField,
                     FxDB(dr("srcustomerkode"), ""), sptField,
                     FxDB(dr("srcustomernama"), ""), sptField,
                     FxDB(dr("srbagianpenjualankode"), ""), sptField,
                     FxDB(dr("srbagianpenjualannama"), ""), sptField,
                     FxDB(dr("srekspedisinama"), ""), sptField,
                     FxDB(dr("sinotransaksi"), ""), sptField,
                     FxDB(dr("rnrnotransaksi"), ""), sptField,
                     FxDB(dr("srstatusnama"), ""), sptField,
                     FxDB(dr("srstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("srinputusernama"), ""), sptField,
                     FxDB(dr("srmodifikasiusernama"), ""), sptField,
                     FxDB(dr("srcustomtext1"), ""), sptField,
                     FxDB(dr("srcustomtext1"), ""), sptField,
                     FxDB(dr("srcustomtext1"), ""), sptField,
                     FxDB(dr("srcustomtext4"), ""), sptField,
                     FxDB(dr("srcustomtext5"), ""), sptField,
                     FxDB(dr("srcustomint1"), 0), sptField,
                     FxDB(dr("srcustomint2"), 0), sptField,
                     FxDB(dr("srcustomint3"), 0), sptField,
                     FxDB(dr("srcustomdbl1"), 0), sptField,
                     FxDB(dr("srcustomdbl2"), 0), sptField,
                     FxDB(dr("srcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("srcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("srcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("srcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("srrekdiskonnama"), ""), sptField,
                     FxDB(dr("srrekpajak1nama"), ""), sptField,
                     FxDB(dr("srrekpajak2nama"), ""), sptField,
                     FxDB(dr("srrekbiayalainnama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sridhistory, srid, srcabang, srlokasi, srgudang, srasalbarang, srasalbarangkategori, srjenispenjulan, srjenispenjualankategori, srcarabayar, srsumber, srautonotransaksi, srnotransaksi, srtgl, srkodepa, srcustomer, srcustomerkontak, sr1alamat1, sr1alamat2, sr1alamat3, sr2alamat1, sr2alamat2, sr2alamat3, srbagianpenjualan, srekspedisi, srtglkirim, srtermin, srtgljatuhtempo, sruraian, srcatatan, srnoref, srtglnoref, srtglpenutupan, srmatauang, srkurs, srhargatermasukpajak, srtotal, srdiskonpersen, srjmldiskon, srtotalpajak1detail, srtotalpajak2detail, srbiayalainpersen, srbiayalain, srtotaltransaksi, srsisatransaksi, srjmlbayar, srstatuslunas, srtgllunas, srnofakturpajak, srsdhbayarpajak, srtglbayarpajak, srrekdiskon, srrekpajak1, srrekpajak2, srrekbiayalain, srreksisa, srrekbayar, sridsq, sridso, sridpl, sriddo, sriddr, sridpi, sridsi, sridrnr, srstatus, srstatussebelumnya, srjmlrevisi, srcetakanke, srinputuser, srinputtgl, srmodifikasiuser, srmodifikasitgl, srposting, srpostingtgl, srtutupperiode, srisclose, srcabangnama, srlokasinama, srgudangnama, srcustomerkode, srcustomernama, srbagianpenjualankode, srbagianpenjualannama, srekspedisinama, sinotransaksi, rnrnotransaksi, srstatusnama, srstatussebelumnyanama, srinputusernama, srmodifikasiusernama, srcustomtext1, srcustomtext2, srcustomtext3, srcustomtext4, srcustomtext5, srcustomint1, srcustomint2, srcustomint3, srcustomdbl1, srcustomdbl2, srcustomdbl3, srcustomdate1, srcustomdate2, srcustomdate3, srrekdiskonnama, srrekpajak1nama, srrekpajak2nama, srrekbiayalainnama"))

        Return wsResult
    End Function


    <WebMethod()>
    Public Function M5_Sr_HistorySearch(ByVal param As String) As String
        'M5_Sr_HistorySearch --------------------------------------------------------
        'sridhistory, srid, srcabang, srlokasi, srgudang, srasalbarang, srasalbarangkategori, srjenispenjulan, 
        'srjenispenjualankategori, srcarabayar, srsumber, srautonotransaksi, srnotransaksi, srtgl, srkodepa, 
        'srcustomer, srcustomerkontak, sr1alamat1, sr1alamat2, sr1alamat3, sr2alamat1, sr2alamat2, 
        'sr2alamat3, srbagianpenjualan, srekspedisi, srtglkirim, srtermin, srtgljatuhtempo, sruraian, 
        'srcatatan, srnoref, srtglnoref, srtglpenutupan, srmatauang, srkurs, srhargatermasukpajak, 
        'srtotal, srdiskonpersen, srjmldiskon, srtotalpajak1detail, srtotalpajak2detail, srbiayalainpersen, srbiayalain, 
        'srtotaltransaksi, srsisatransaksi, srjmlbayar, srstatuslunas, srtgllunas, srnofakturpajak, srsdhbayarpajak, 
        'srtglbayarpajak, srrekdiskon, srrekpajak1, srrekpajak2, srrekbiayalain, srreksisa, srrekbayar, 
        'sridsq, sridso, sridpl, sriddo, sriddr, sridpi, sridsi, 
        'sridrnr, srstatus, srstatussebelumnya, srjmlrevisi, srcetakanke, srinputuser, srinputtgl, 
        'srmodifikasiuser, srmodifikasitgl, srposting, srpostingtgl, srtutupperiode, srisclose, srcabangnama, 
        'srlokasinama, srgudangnama, srcustomerkode, srcustomernama, srbagianpenjualankode, srbagianpenjualannama, srekspedisinama, 
        'sinotransaksi, rnrnotransaksi, srstatusnama, srstatussebelumnyanama, srinputusernama, srmodifikasiusernama, srcustomtext1,
        'srcustomtext2, srcustomtext3, srcustomtext4, srcustotmext5, srcustomint1, srcustomint2, srcustomint3, srcustomdbl1, srcustomdbl2,
        'srcustomdbl3, srcustomdate1, srcustomdate2, srcustomdate3, srjenis, srjenisnama

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
            Filter = Filter.Replace("srcustomerkode", "c1.kkode")
            Filter = Filter.Replace("srcustomernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_sr_v_history")

        dt = AmbilData("aplikasi1-M5_Sr", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("srid"), 0), sptField,
                     FxDB(dr("sridhistory"), 0), sptField,
                     FxDB(dr("srcabang"), ""), sptField,
                     FxDB(dr("srlokasi"), ""), sptField,
                     FxDB(dr("srgudang"), ""), sptField,
                     FxDB(dr("srasalbarang"), ""), sptField,
                     FxDB(dr("srasalbarangkategori"), 0), sptField,
                     FxDB(dr("srjenispenjulan"), ""), sptField,
                     FxDB(dr("srjenispenjualankategori"), 0), sptField,
                     FxDB(dr("srcarabayar"), 0), sptField,
                     FxDB(dr("srsumber"), ""), sptField,
                     FxDB(dr("srautonotransaksi"), 0), sptField,
                     FxDB(dr("srnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("srtgl"), ""), formatTgl), sptField,
                     FxDB(dr("srkodepa"), 0), sptField,
                     FxDB(dr("srcustomer"), 0), sptField,
                     FxDB(dr("srcustomerkontak"), ""), sptField,
                     FxDB(dr("sr1alamat1"), ""), sptField,
                     FxDB(dr("sr1alamat2"), ""), sptField,
                     FxDB(dr("sr1alamat3"), ""), sptField,
                     FxDB(dr("sr2alamat1"), ""), sptField,
                     FxDB(dr("sr2alamat2"), ""), sptField,
                     FxDB(dr("sr2alamat3"), ""), sptField,
                     FxDB(dr("srbagianpenjualan"), 0), sptField,
                     FxDB(dr("srekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("srtglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("srtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("srtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("sruraian"), ""), sptField,
                     FxDB(dr("srcatatan"), ""), sptField,
                     FxDB(dr("srnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("srtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("srtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("srmatauang"), ""), sptField,
                     FxDB(dr("srkurs"), 0), sptField,
                     FxDB(dr("srhargatermasukpajak"), 0), sptField,
                     FxDB(dr("srtotal"), 0), sptField,
                     FxDB(dr("srdiskonpersen"), ""), sptField,
                     FxDB(dr("srjmldiskon"), 0), sptField,
                     FxDB(dr("srtotalpajak1detail"), 0), sptField,
                     FxDB(dr("srtotalpajak2detail"), 0), sptField,
                     FxDB(dr("srbiayalainpersen"), 0), sptField,
                     FxDB(dr("srbiayalain"), 0), sptField,
                     FxDB(dr("srtotaltransaksi"), 0), sptField,
                     FxDB(dr("srsisatransaksi"), 0), sptField,
                     FxDB(dr("srjmlbayar"), 0), sptField,
                     FxDB(dr("srstatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("srtgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("srnofakturpajak"), ""), sptField,
                     FxDB(dr("srsdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("srtglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(dr("srrekdiskon"), ""), sptField,
                     FxDB(dr("srrekpajak1"), ""), sptField,
                     FxDB(dr("srrekpajak2"), ""), sptField,
                     FxDB(dr("srrekbiayalain"), ""), sptField,
                     FxDB(dr("srreksisa"), ""), sptField,
                     FxDB(dr("srrekbayar"), ""), sptField,
                     FxDB(dr("sridsq"), 0), sptField,
                     FxDB(dr("sridso"), 0), sptField,
                     FxDB(dr("sridpl"), 0), sptField,
                     FxDB(dr("sriddo"), 0), sptField,
                     FxDB(dr("sriddr"), 0), sptField,
                     FxDB(dr("sridpi"), 0), sptField,
                     FxDB(dr("sridsi"), 0), sptField,
                     FxDB(dr("sridrnr"), 0), sptField,
                     FxDB(dr("srstatus"), 0), sptField,
                     FxDB(dr("srstatussebelumnya"), 0), sptField,
                     FxDB(dr("srjmlrevisi"), 0), sptField,
                     FxDB(dr("srcetakanke"), 0), sptField,
                     FxDB(dr("srinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("srinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("srmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("srmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("srposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("srpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("srtutupperiode"), 0), sptField,
                     FxDB(dr("srisclose"), 0), sptField,
                     FxDB(dr("srcabangnama"), ""), sptField,
                     FxDB(dr("srlokasinama"), ""), sptField,
                     FxDB(dr("srgudangnama"), ""), sptField,
                     FxDB(dr("srcustomerkode"), ""), sptField,
                     FxDB(dr("srcustomernama"), ""), sptField,
                     FxDB(dr("srbagianpenjualankode"), ""), sptField,
                     FxDB(dr("srbagianpenjualannama"), ""), sptField,
                     FxDB(dr("srekspedisinama"), ""), sptField,
                     FxDB(dr("sinotransaksi"), ""), sptField,
                     FxDB(dr("rnrnotransaksi"), ""), sptField,
                     FxDB(dr("srstatusnama"), ""), sptField,
                     FxDB(dr("srstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("srinputusernama"), ""), sptField,
                     FxDB(dr("srmodifikasiusernama"), ""), sptField,
                     FxDB(dr("srcustomtext1"), ""), sptField,
                     FxDB(dr("srcustomtext2"), ""), sptField,
                     FxDB(dr("srcustomtext3"), ""), sptField,
                     FxDB(dr("srcustomtext4"), ""), sptField,
                     FxDB(dr("srcustomtext5"), ""), sptField,
                     FxDB(dr("srcustomint1"), 0), sptField,
                     FxDB(dr("srcustomint2"), 0), sptField,
                     FxDB(dr("srcustomint3"), 0), sptField,
                     FxDB(dr("srcustomdbl1"), 0), sptField,
                     FxDB(dr("srcustomdbl2"), 0), sptField,
                     FxDB(dr("srcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("srcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("srcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("srcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("srjenis"), 0), sptField,
                     FxDB(dr("srjenisnama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sridhistory, srid, srcabang, srlokasi, srgudang, srasalbarang, srasalbarangkategori, srjenispenjulan, srjenispenjualankategori, srcarabayar, srsumber, srautonotransaksi, srnotransaksi, srtgl, srkodepa, srcustomer, srcustomerkontak, sr1alamat1, sr1alamat2, sr1alamat3, sr2alamat1, sr2alamat2, sr2alamat3, srbagianpenjualan, srekspedisi, srtglkirim, srtermin, srtgljatuhtempo, sruraian, srcatatan, srnoref, srtglnoref, srtglpenutupan, srmatauang, srkurs, srhargatermasukpajak, srtotal, srdiskonpersen, srjmldiskon, srtotalpajak1detail, srtotalpajak2detail, srbiayalainpersen, srbiayalain, srtotaltransaksi, srsisatransaksi, srjmlbayar, srstatuslunas, srtgllunas, srnofakturpajak, srsdhbayarpajak, srtglbayarpajak, srrekdiskon, srrekpajak1, srrekpajak2, srrekbiayalain, srreksisa, srrekbayar, sridsq, sridso, sridpl, sriddo, sriddr, sridpi, sridsi, sridrnr, srstatus, srstatussebelumnya, srjmlrevisi, srcetakanke, srinputuser, srinputtgl, srmodifikasiuser, srmodifikasitgl, srposting, srpostingtgl, srtutupperiode, srisclose, srcabangnama, srlokasinama, srgudangnama, srcustomerkode, srcustomernama, srbagianpenjualankode, srbagianpenjualannama, srekspedisinama, sinotransaksi, rnrnotransaksi, srstatusnama, srstatussebelumnyanama, srinputusernama, srmodifikasiusernama, srcustomtext1, srcustomtext2, srcustomtext3, srcustomtext4, srcustomtext5, srcustomint1, srcustomint2, srcustomint3, srcustomdbl1, srcustomdbl2, srcustomdbl3, srcustomdate1, srcustomdate2, srcustomdate3, srjenis, srjenisnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SrHistoryGetdataById(ByVal param As String) As String

        'M5_SrHistoryGetdataById Utama --------------------------------------------------------
        'sridhistory, srid, srcabang, srlokasi, srgudang, srasalbarang, srasalbarangkategori, srjenispenjulan, 
        'srjenispenjualankategori, srcarabayar, srsumber, srautonotransaksi, srnotransaksi, srtgl, srkodepa, 
        'srcustomer, srcustomerkontak, sr1alamat1, sr1alamat2, sr1alamat3, sr2alamat1, sr2alamat2, 
        'sr2alamat3, srbagianpenjualan, srekspedisi, srtglkirim, srtermin, srtgljatuhtempo, sruraian, 
        'srcatatan, srnoref, srtglnoref, srtglpenutupan, srmatauang, srkurs, srhargatermasukpajak, 
        'srtotal, srdiskonpersen, srjmldiskon, srtotalpajak1detail, srtotalpajak2detail, srbiayalainpersen, srbiayalain, 
        'srtotaltransaksi, srsisatransaksi, srjmlbayar, srstatuslunas, srtgllunas, srnofakturpajak, srsdhbayarpajak, 
        'srtglbayarpajak, srrekdiskon, srrekpajak1, srrekpajak2, srrekbiayalain, srreksisa, srrekbayar, 
        'sridsq, sridso, sridpl, sriddo, sriddr, sridpi, sridsi, 
        'sridrnr, srstatus, srstatussebelumnya, srjmlrevisi, srcetakanke, srinputuser, srinputtgl, 
        'srmodifikasiuser, srmodifikasitgl, srposting, srpostingtgl, srtutupperiode, srisclose, srcustomtext1, 
        'srcustomtext2, srcustomtext3, srcustomtext4, srcustomtext5, srcustomint1, srcustomint2, srcustomint3, 
        'srcustomdbl1, srcustomdbl2, srcustomdbl3, srcustomdate1, srcustomdate2, srcustomdate3, srcabangnama, 
        'srlokasinama, srgudangnama, srcustomerkode, srcustomernama, srbagianpenjualankode, srbagianpenjualannama, srekspedisinama, 
        'srterminnama, srterminharijatuhtempo, srrekdiskonnama, srrekpajak1nama, srrekpajak2nama, srrekbiayalainnama, srrekbayarnama, 
        'srreksisanama, srnotransaksisi, srnotransaksirnr, srstatusnama, srstatussebelumnyanama, srinputusernama, srmodifikasiusernama, srjenis

        'M5_SrHistoryGetdataById Detail --------------------------------------------------------
        'idhistorydetail, idhistory, idsrdetail, idsr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, 
        'jmlbarang, satuanbarang, matauang, kurs, idhppkhususkeluar, idhppfifokeluar, harga, 
        'hargapricelist, hpp, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, 
        'jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, 
        'rekhargapokok, rekdiskonpenjualan, rekreturpenjualan, costcenter, divisi, subdivisi, proyek, 
        'catatan, urutan, idsqdetail, idsodetail, idpldetail, iddodetail, iddrdetail, 
        'idpidetail, idsidetail, idrnrdetail, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, 
        'bhpp, bjenis, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, 
        'pajak2nilai, cabangnama, lokasinama, gudangasalnama, gudangtransitnama, gudangtujuannama, costcenternama, 
        'divisinama, subdivisinama, proyeknama, sinotransaksi, rnrnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan

        'M5_SrHistoryGetdataById Batch --------------------------------------------------------
        'nbtidhistory, nbtidtransaksihistory, nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang

        'M5_SrHistoryGetdataById Serial --------------------------------------------------------
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
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_sr_getdata_history")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("sridhistory"), 0), sptField, FxDB(drutama("srid"), 0), sptField,
                     FxDB(drutama("srcabang"), ""), sptField,
                     FxDB(drutama("srlokasi"), ""), sptField,
                     FxDB(drutama("srgudang"), ""), sptField,
                     FxDB(drutama("srasalbarang"), ""), sptField,
                     FxDB(drutama("srasalbarangkategori"), 0), sptField,
                     FxDB(drutama("srjenispenjulan"), ""), sptField,
                     FxDB(drutama("srjenispenjualankategori"), 0), sptField,
                     FxDB(drutama("srcarabayar"), 0), sptField,
                     FxDB(drutama("srsumber"), ""), sptField,
                     FxDB(drutama("srautonotransaksi"), 0), sptField,
                     FxDB(drutama("srnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("srtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("srkodepa"), 0), sptField,
                     FxDB(drutama("srcustomer"), 0), sptField,
                     FxDB(drutama("srcustomerkontak"), ""), sptField,
                     FxDB(drutama("sr1alamat1"), ""), sptField,
                     FxDB(drutama("sr1alamat2"), ""), sptField,
                     FxDB(drutama("sr1alamat3"), ""), sptField,
                     FxDB(drutama("sr2alamat1"), ""), sptField,
                     FxDB(drutama("sr2alamat2"), ""), sptField,
                     FxDB(drutama("sr2alamat3"), ""), sptField,
                     FxDB(drutama("srbagianpenjualan"), 0), sptField,
                     FxDB(drutama("srekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("srtglkirim"), ""), formatTgl), sptField,
                     FxDB(drutama("srtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("srtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("sruraian"), ""), sptField,
                     FxDB(drutama("srcatatan"), ""), sptField,
                     FxDB(drutama("srnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("srtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("srtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("srmatauang"), ""), sptField,
                     FxDB(drutama("srkurs"), 0), sptField,
                     FxDB(drutama("srhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("srtotal"), 0), sptField,
                     FxDB(drutama("srdiskonpersen"), ""), sptField,
                     FxDB(drutama("srjmldiskon"), 0), sptField,
                     FxDB(drutama("srtotalpajak1detail"), 0), sptField,
                     FxDB(drutama("srtotalpajak2detail"), 0), sptField,
                     FxDB(drutama("srbiayalainpersen"), 0), sptField,
                     FxDB(drutama("srbiayalain"), 0), sptField,
                     FxDB(drutama("srtotaltransaksi"), 0), sptField,
                     FxDB(drutama("srsisatransaksi"), 0), sptField,
                     FxDB(drutama("srjmlbayar"), 0), sptField,
                     FxDB(drutama("srstatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("srtgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("srnofakturpajak"), ""), sptField,
                     FxDB(drutama("srsdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("srtglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(drutama("srrekdiskon"), ""), sptField,
                     FxDB(drutama("srrekpajak1"), ""), sptField,
                     FxDB(drutama("srrekpajak2"), ""), sptField,
                     FxDB(drutama("srrekbiayalain"), ""), sptField,
                     FxDB(drutama("srreksisa"), ""), sptField,
                     FxDB(drutama("srrekbayar"), ""), sptField,
                     FxDB(drutama("sridsq"), 0), sptField,
                     FxDB(drutama("sridso"), 0), sptField,
                     FxDB(drutama("sridpl"), 0), sptField,
                     FxDB(drutama("sriddo"), 0), sptField,
                     FxDB(drutama("sriddr"), 0), sptField,
                     FxDB(drutama("sridpi"), 0), sptField,
                     FxDB(drutama("sridsi"), 0), sptField,
                     FxDB(drutama("sridrnr"), 0), sptField,
                     FxDB(drutama("srstatus"), 0), sptField,
                     FxDB(drutama("srstatussebelumnya"), 0), sptField,
                     FxDB(drutama("srjmlrevisi"), 0), sptField,
                     FxDB(drutama("srcetakanke"), 0), sptField,
                     FxDB(drutama("srinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("srinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("srmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("srmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("srposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("srpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("srtutupperiode"), 0), sptField,
                     FxDB(drutama("srisclose"), 0), sptField,
                     FxDB(drutama("srcustomtext1"), ""), sptField,
                     FxDB(drutama("srcustomtext2"), ""), sptField,
                     FxDB(drutama("srcustomtext3"), ""), sptField,
                     FxDB(drutama("srcustomtext4"), ""), sptField,
                     FxDB(drutama("srcustomtext5"), ""), sptField,
                     FxDB(drutama("srcustomint1"), 0), sptField,
                     FxDB(drutama("srcustomint2"), 0), sptField,
                     FxDB(drutama("srcustomint3"), 0), sptField,
                     FxDB(drutama("srcustomdbl1"), 0), sptField,
                     FxDB(drutama("srcustomdbl2"), 0), sptField,
                     FxDB(drutama("srcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("srcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("srcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("srcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("srcabangnama"), ""), sptField,
                     FxDB(drutama("srlokasinama"), ""), sptField,
                     FxDB(drutama("srgudangnama"), ""), sptField,
                     FxDB(drutama("srcustomerkode"), ""), sptField,
                     FxDB(drutama("srcustomernama"), ""), sptField,
                     FxDB(drutama("srbagianpenjualankode"), ""), sptField,
                     FxDB(drutama("srbagianpenjualannama"), ""), sptField,
                     FxDB(drutama("srekspedisinama"), ""), sptField,
                     FxDB(drutama("srterminnama"), ""), sptField,
                     FxDB(drutama("srterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("srrekdiskonnama"), ""), sptField,
                     FxDB(drutama("srrekpajak1nama"), ""), sptField,
                     FxDB(drutama("srrekpajak2nama"), ""), sptField,
                     FxDB(drutama("srrekbiayalainnama"), ""), sptField,
                     FxDB(drutama("srrekbayarnama"), ""), sptField,
                     FxDB(drutama("srreksisanama"), ""), sptField,
                     FxDB(drutama("srnotransaksisi"), ""), sptField,
                     FxDB(drutama("srnotransaksirnr"), ""), sptField,
                     FxDB(drutama("srstatusnama"), ""), sptField,
                     FxDB(drutama("srstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("srinputusernama"), ""), sptField,
                     FxDB(drutama("srmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("srjenis"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField, FxDB(dr("idhistory"), 0), sptField, FxDB(dr("idsrdetail"), 0), sptField,
                     FxDB(dr("idsr"), 0), sptField,
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
                     FxDB(dr("idrnrdetail"), 0), sptField,
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
                     FxDB(dr("sinotransaksi"), ""), sptField,
                     FxDB(dr("rnrnotransaksi"), ""), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sridhistory, srid, srcabang, srlokasi, srgudang, srasalbarang, srasalbarangkategori, srjenispenjulan, srjenispenjualankategori, srcarabayar, srsumber, srautonotransaksi, srnotransaksi, srtgl, srkodepa, srcustomer, srcustomerkontak, sr1alamat1, sr1alamat2, sr1alamat3, sr2alamat1, sr2alamat2, sr2alamat3, srbagianpenjualan, srekspedisi, srtglkirim, srtermin, srtgljatuhtempo, sruraian, srcatatan, srnoref, srtglnoref, srtglpenutupan, srmatauang, srkurs, srhargatermasukpajak, srtotal, srdiskonpersen, srjmldiskon, srtotalpajak1detail, srtotalpajak2detail, srbiayalainpersen, srbiayalain, srtotaltransaksi, srsisatransaksi, srjmlbayar, srstatuslunas, srtgllunas, srnofakturpajak, srsdhbayarpajak, srtglbayarpajak, srrekdiskon, srrekpajak1, srrekpajak2, srrekbiayalain, srreksisa, srrekbayar, sridsq, sridso, sridpl, sriddo, sriddr, sridpi, sridsi, sridrnr, srstatus, srstatussebelumnya, srjmlrevisi, srcetakanke, srinputuser, srinputtgl, srmodifikasiuser, srmodifikasitgl, srposting, srpostingtgl, srtutupperiode, srisclose, srcustomtext1, srcustomtext2, srcustomtext3, srcustomtext4, srcustomtext5, srcustomint1, srcustomint2, srcustomint3, srcustomdbl1, srcustomdbl2, srcustomdbl3, srcustomdate1, srcustomdate2, srcustomdate3, srcabangnama, srlokasinama, srgudangnama, srcustomerkode, srcustomernama, srbagianpenjualankode, srbagianpenjualannama, srekspedisinama, srterminnama, srterminharijatuhtempo, srrekdiskonnama, srrekpajak1nama, srrekpajak2nama, srrekbiayalainnama, srrekbayarnama, srreksisanama, srnotransaksisi, srnotransaksirnr, srstatusnama, srstatussebelumnyanama, srinputusernama, srmodifikasiusernama, srjenis" & sptSubParam & "idhistorydetail, idhistory, idsrdetail, idsr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, idhppkhususkeluar, idhppfifokeluar, harga, hargapricelist, hpp, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekreturpenjualan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpldetail, iddodetail, iddrdetail, idpidetail, idsidetail, idrnrdetail, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangasalnama, gudangtransitnama, gudangtujuannama, costcenternama, divisinama, subdivisinama, proyeknama, sinotransaksi, rnrnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan" & sptSubParam & "nbtidhistory, nbtidtransaksihistory, nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang" & sptSubParam & "nstidhistory, nstidtransaksihistory, nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang"))

        Return wsResult
    End Function

End Class
