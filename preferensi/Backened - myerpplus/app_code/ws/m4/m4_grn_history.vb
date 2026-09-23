Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_grn_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function m4_Grn_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m4_grn_history(SELECT 0, grn.* FROM m4_grn grn WHERE grn.grnid = '" & idtransaksi & "')"
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
            sql = "SELECT grnidhistory FROM m4_grn_history WHERE grnid = '" & idtransaksi & "' ORDER BY grnmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m4_grn_detail_history (SELECT 0, '" & result(4) & "', grn.* FROM m4_grn_detail grn WHERE grn.idgrn = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY DETAIL -------------------------------

            'PROSES INSERT HISTORY COST --------------------------------------
            sql = "INSERT INTO m4_grn_cost_history (SELECT 0, '" & result(4) & "', grn.* FROM m4_grn_cost grn WHERE grn.idgrn = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY COST -------------------------------

            'PROSES INSERT HISTORY BATCH ---------------------------------------
            sql = "INSERT INTO m1_no_batch_transaction_history(SELECT 0, '" & result(4) & "', nb.* FROM m1_no_batch_transaction nb WHERE nb.nbtidtransaksi = '" & idtransaksi & "' and nb.nbtsumber = 'GRN')"
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
            sql = "INSERT INTO m1_no_serial_transaction_history(SELECT 0, '" & result(4) & "', ns.* FROM m1_no_serial_transaction ns WHERE ns.nstidtransaksi = '" & idtransaksi & "' and ns.nstsumber = 'GRN')"
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
            sql = "INSERT INTO m7_asset_transaction_history(SELECT 0, '" & result(4) & "', atr.* FROM m7_asset_transaction atr WHERE atr.atidutama = '" & idtransaksi & "' and atr.atsumber = 'GRN')"
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
    Public Function M4_Grn_HistorySearch(ByVal param As String) As String
        'M4_Grn_HistorySearch --------------------------------------------------------
        'grnidhistory, grnid, grncabang, grnlokasi, grngudang, grnasalbarang, grnasalbarangkategori, grnjenispembelian, 
        'grnjenispembeliankategori, grncarabayar, grnsumber, grnautonotransaksi, grnnotransaksi, grntgl, grnkodepa, 
        'grnsupplier, grnsupplierkontak, grn1alamat1, grn1alamat2, grn1alamat3, grn2alamat1, grn2alamat2, 
        'grn2alamat3, grnbagianpembelian, grntermin, grntgljatuhtempo, grnuraian, grncatatan, grnnoref, 
        'grntglnoref, grntglpenutupan, grnmatauang, grnkurs, grnhargatermasukpajak, grntotal, grndiskonpersen, 
        'grnjmldiskon, grntotalpajak1detail, grntotalpajak2detail, grnbiayalainpersen, grnbiayalain, grntotaltransaksi, grnjmlbayar, 
        'grnrekdiskon, grnrekpajak1, grnrekpajak2, grnrekbiayalain, grnrekbayar, grnidpr, grnidcs, 
        'grnidrq, grnidbs, grnidpo, grnidipc, grnstatusri, grnstatusdnr, grnstatusprt, 
        'grnstatusrealisasi, grnstatus, grnstatussebelumnya, grnjmlrevisi, grncetakanke, grninputuser, grninputtgl, 
        'grnmodifikasiuser, grnmodifikasitgl, grnposting, grnpostingtgl, grntutupperiode, grnisclose, grncabangnama, 
        'grnlokasinama, grngudangnama, grnsupplierkode, grnsuppliernama, grnbagianpembeliankode, grnbagianpembeliannama, prnotransaksi, 
        'csnotransaksi, rqnotransaksi, bsnotransaksi, ponotransaksi, ipcnotransaksi, grnstatusnama, grnstatussebelumnyanama, 
        'grninputusernama, grnmodifikasiusernama

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
            Filter = Filter.Replace("grnsupplierkode", "c1.kkode")
            Filter = Filter.Replace("grnsuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_grn_v_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Grn", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("grnid"), 0), sptField,
                     FxDB(dr("grnidhistory"), 0), sptField,
                     FxDB(dr("grncabang"), ""), sptField,
                     FxDB(dr("grnlokasi"), ""), sptField,
                     FxDB(dr("grngudang"), ""), sptField,
                     FxDB(dr("grnasalbarang"), ""), sptField,
                     FxDB(dr("grnasalbarangkategori"), 0), sptField,
                     FxDB(dr("grnjenispembelian"), ""), sptField,
                     FxDB(dr("grnjenispembeliankategori"), 0), sptField,
                     FxDB(dr("grncarabayar"), 0), sptField,
                     FxDB(dr("grnsumber"), ""), sptField,
                     FxDB(dr("grnautonotransaksi"), 0), sptField,
                     FxDB(dr("grnnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("grntgl"), ""), formatTgl), sptField,
                     FxDB(dr("grnkodepa"), 0), sptField,
                     FxDB(dr("grnsupplier"), 0), sptField,
                     FxDB(dr("grnsupplierkontak"), ""), sptField,
                     FxDB(dr("grn1alamat1"), ""), sptField,
                     FxDB(dr("grn1alamat2"), ""), sptField,
                     FxDB(dr("grn1alamat3"), ""), sptField,
                     FxDB(dr("grn2alamat1"), ""), sptField,
                     FxDB(dr("grn2alamat2"), ""), sptField,
                     FxDB(dr("grn2alamat3"), ""), sptField,
                     FxDB(dr("grnbagianpembelian"), 0), sptField,
                     FxDB(dr("grntermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("grntgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("grnuraian"), ""), sptField,
                     FxDB(dr("grncatatan"), ""), sptField,
                     FxDB(dr("grnnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("grntglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("grntglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("grnmatauang"), ""), sptField,
                     FxDB(dr("grnkurs"), 0), sptField,
                     FxDB(dr("grnhargatermasukpajak"), 0), sptField,
                     FxDB(dr("grntotal"), 0), sptField,
                     FxDB(dr("grndiskonpersen"), ""), sptField,
                     FxDB(dr("grnjmldiskon"), 0), sptField,
                     FxDB(dr("grntotalpajak1detail"), 0), sptField,
                     FxDB(dr("grntotalpajak2detail"), 0), sptField,
                     FxDB(dr("grnbiayalainpersen"), ""), sptField,
                     FxDB(dr("grnbiayalain"), 0), sptField,
                     FxDB(dr("grntotaltransaksi"), 0), sptField,
                     FxDB(dr("grnjmlbayar"), 0), sptField,
                     FxDB(dr("grnrekdiskon"), ""), sptField,
                     FxDB(dr("grnrekpajak1"), ""), sptField,
                     FxDB(dr("grnrekpajak2"), ""), sptField,
                     FxDB(dr("grnrekbiayalain"), ""), sptField,
                     FxDB(dr("grnrekbayar"), ""), sptField,
                     FxDB(dr("grnidpr"), 0), sptField,
                     FxDB(dr("grnidcs"), 0), sptField,
                     FxDB(dr("grnidrq"), 0), sptField,
                     FxDB(dr("grnidbs"), 0), sptField,
                     FxDB(dr("grnidpo"), 0), sptField,
                     FxDB(dr("grnidipc"), 0), sptField,
                     FxDB(dr("grnstatusri"), 0), sptField,
                     FxDB(dr("grnstatusdnr"), 0), sptField,
                     FxDB(dr("grnstatusprt"), 0), sptField,
                     FxDB(dr("grnstatusrealisasi"), 0), sptField,
                     FxDB(dr("grnstatus"), 0), sptField,
                     FxDB(dr("grnstatussebelumnya"), 0), sptField,
                     FxDB(dr("grnjmlrevisi"), 0), sptField,
                     FxDB(dr("grncetakanke"), 0), sptField,
                     FxDB(dr("grninputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("grninputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("grnmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("grnmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("grnposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("grnpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("grntutupperiode"), 0), sptField,
                     FxDB(dr("grnisclose"), 0), sptField,
                     FxDB(dr("grncabangnama"), ""), sptField,
                     FxDB(dr("grnlokasinama"), ""), sptField,
                     FxDB(dr("grngudangnama"), ""), sptField,
                     FxDB(dr("grnsupplierkode"), ""), sptField,
                     FxDB(dr("grnsuppliernama"), ""), sptField,
                     FxDB(dr("grnbagianpembeliankode"), ""), sptField,
                     FxDB(dr("grnbagianpembeliannama"), ""), sptField,
                     FxDB(dr("prnotransaksi"), ""), sptField,
                     FxDB(dr("csnotransaksi"), ""), sptField,
                     FxDB(dr("rqnotransaksi"), ""), sptField,
                     FxDB(dr("bsnotransaksi"), ""), sptField,
                     FxDB(dr("ponotransaksi"), ""), sptField,
                     FxDB(dr("ipcnotransaksi"), ""), sptField,
                     FxDB(dr("grnstatusnama"), ""), sptField,
                     FxDB(dr("grnstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("grninputusernama"), ""), sptField,
                     FxDB(dr("grnmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("grnidhistory, grnid, grncabang, grnlokasi, grngudang, grnasalbarang, grnasalbarangkategori, grnjenispembelian, grnjenispembeliankategori, grncarabayar, grnsumber, grnautonotransaksi, grnnotransaksi, grntgl, grnkodepa, grnsupplier, grnsupplierkontak, grn1alamat1, grn1alamat2, grn1alamat3, grn2alamat1, grn2alamat2, grn2alamat3, grnbagianpembelian, grntermin, grntgljatuhtempo, grnuraian, grncatatan, grnnoref, grntglnoref, grntglpenutupan, grnmatauang, grnkurs, grnhargatermasukpajak, grntotal, grndiskonpersen, grnjmldiskon, grntotalpajak1detail, grntotalpajak2detail, grnbiayalainpersen, grnbiayalain, grntotaltransaksi, grnjmlbayar, grnrekdiskon, grnrekpajak1, grnrekpajak2, grnrekbiayalain, grnrekbayar, grnidpr, grnidcs, grnidrq, grnidbs, grnidpo, grnidipc, grnstatusri, grnstatusdnr, grnstatusprt, grnstatusrealisasi, grnstatus, grnstatussebelumnya, grnjmlrevisi, grncetakanke, grninputuser, grninputtgl, grnmodifikasiuser, grnmodifikasitgl, grnposting, grnpostingtgl, grntutupperiode, grnisclose, grncabangnama, grnlokasinama, grngudangnama, grnsupplierkode, grnsuppliernama, grnbagianpembeliankode, grnbagianpembeliannama, prnotransaksi, csnotransaksi, rqnotransaksi, bsnotransaksi, ponotransaksi, ipcnotransaksi, grnstatusnama, grnstatussebelumnyanama, grninputusernama, grnmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_GrnHistoryGetdataById(ByVal param As String) As String

        'M4_GrnHistoryGetdataById Utama --------------------------------------------------------
        'grnidhistory, grnid, grncabang, grnlokasi, grngudang, grnasalbarang, grnasalbarangkategori, grnjenispembelian, 
        'grnjenispembeliankategori, grncarabayar, grnsumber, grnautonotransaksi, grnnotransaksi, grntgl, grnkodepa, 
        'grnsupplier, grnsupplierkontak, grn1alamat1, grn1alamat2, grn1alamat3, grn2alamat1, grn2alamat2, 
        'grn2alamat3, grnbagianpembelian, grntermin, grntgljatuhtempo, grnuraian, grncatatan, grnnoref, 
        'grntglnoref, grntglpenutupan, grnmatauang, grnkurs, grnhargatermasukpajak, grntotal, grndiskonpersen, 
        'grnjmldiskon, grntotalpajak1detail, grntotalpajak2detail, grnbiayalainpersen, grnbiayalain, grntotaltransaksi, grnjmlbayar, 
        'grnrekdiskon, grnrekpajak1, grnrekpajak2, grnrekbiayalain, grnrekbayar, grnidpr, grnidcs, 
        'grnidrq, grnidbs, grnidpo, grnidipc, grnstatusri, grnstatusdnr, grnstatusprt, 
        'grnstatusrealisasi, grnstatus, grnstatussebelumnya, grnjmlrevisi, grncetakanke, grninputuser, grninputtgl, 
        'grnmodifikasiuser, grnmodifikasitgl, grnposting, grnpostingtgl, grntutupperiode, grnisclose, grncustomtext1, 
        'grncustomtext2, grncustomtext3, grncustomtext4, grncustomtext5, grncustomint1, grncustomint2, grncustomint3, 
        'grncustomdbl1, grncustomdbl2, grncustomdbl3, grncustomdate1, grncustomdate2, grncustomdate3, grncabangnama, 
        'grnlokasinama, grngudangnama, grnsupplierkode, grnsuppliernama, grnbagianpembeliankode, grnbagianpembeliannama, grnterminnama, 
        'grnterminharijatuhtempo, grnrekdiskonnama, grnrekpajak1nama, grnrekpajak2nama, grnrekbiayalainnama, grnrekbayarnama, grnnotransaksipr, 
        'grnnotransaksics, grnnotransaksirq, grnnotransaksibs, grnnotransaksipo, grnnotransaksiipc, grnstatusnama, grnstatussebelumnyanama, 
        'grninputusernama, grnmodifikasiusernama

        'M4_GrnHistoryGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idgrndetail, idgrn, idbarang, namabarang, tipebarang, 
        'jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, 
        'hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, 
        'jmlpajak2, cabang, lokasi, gudang, rekpersediaan, rekdiskonpembelian, rekhutangsementara, 
        'costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, 
        'idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, jmlri, statusri, 
        'jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, 
        'pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, 
        'costcenternama, divisinama, subdivisinama, proyeknama, ponotransaksi, ipcnotransaksi, 
        'bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan

        'M4_GrnHistoryGetdataById Batch --------------------------------------------------------
        'nbtidhistory, nbtidtransaksihistory, nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang

        'M4_GrnHistoryGetdataById Serial --------------------------------------------------------
        'nstidhistory, nstidtransaksihistory, nstid, nstjenismutasi, nstidserialin, nstgudang,  nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, 
        'nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, 
        'nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang

        'M4_GrnHistoryGetdataById Cost --------------------------------------------------------
        'idhistorydetail, idhistory, idgrncost, idgrn, kodecost, matauang, kurs, jumlah, rekdebit, 
        'rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, 
        'proyek, urutan, idprcost, idcscost, idrqcost, idbscost, idpocost, 
        'idipccost, jumlahri, statusri, jumlahbayar, statusbayar, isclose, customtext1, 
        'customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3, kodecostnama, rekdebitnama, rekkreditnama, kontakkode, kontaknama, costcenternama, 
        'divisinama, subdivisinama

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
        Dim sumber As String = "GRN"

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

        Dim NmMemcached As String = "aplikasi1-M4_Grn~M4_Grn_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "grnidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "grnidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_grn_getdata_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("grnidhistory"), 0), sptField, FxDB(drutama("grnid"), 0), sptField,
                     FxDB(drutama("grncabang"), ""), sptField,
                     FxDB(drutama("grnlokasi"), ""), sptField,
                     FxDB(drutama("grngudang"), ""), sptField,
                     FxDB(drutama("grnasalbarang"), ""), sptField,
                     FxDB(drutama("grnasalbarangkategori"), 0), sptField,
                     FxDB(drutama("grnjenispembelian"), ""), sptField,
                     FxDB(drutama("grnjenispembeliankategori"), 0), sptField,
                     FxDB(drutama("grncarabayar"), 0), sptField,
                     FxDB(drutama("grnsumber"), ""), sptField,
                     FxDB(drutama("grnautonotransaksi"), 0), sptField,
                     FxDB(drutama("grnnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("grntgl"), ""), formatTgl), sptField,
                     FxDB(drutama("grnkodepa"), 0), sptField,
                     FxDB(drutama("grnsupplier"), 0), sptField,
                     FxDB(drutama("grnsupplierkontak"), ""), sptField,
                     FxDB(drutama("grn1alamat1"), ""), sptField,
                     FxDB(drutama("grn1alamat2"), ""), sptField,
                     FxDB(drutama("grn1alamat3"), ""), sptField,
                     FxDB(drutama("grn2alamat1"), ""), sptField,
                     FxDB(drutama("grn2alamat2"), ""), sptField,
                     FxDB(drutama("grn2alamat3"), ""), sptField,
                     FxDB(drutama("grnbagianpembelian"), 0), sptField,
                     FxDB(drutama("grntermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("grntgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("grnuraian"), ""), sptField,
                     FxDB(drutama("grncatatan"), ""), sptField,
                     FxDB(drutama("grnnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("grntglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("grntglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("grnmatauang"), ""), sptField,
                     FxDB(drutama("grnkurs"), 0), sptField,
                     FxDB(drutama("grnhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("grntotal"), 0), sptField,
                     FxDB(drutama("grndiskonpersen"), ""), sptField,
                     FxDB(drutama("grnjmldiskon"), 0), sptField,
                     FxDB(drutama("grntotalpajak1detail"), 0), sptField,
                     FxDB(drutama("grntotalpajak2detail"), 0), sptField,
                     FxDB(drutama("grnbiayalainpersen"), ""), sptField,
                     FxDB(drutama("grnbiayalain"), 0), sptField,
                     FxDB(drutama("grntotaltransaksi"), 0), sptField,
                     FxDB(drutama("grnjmlbayar"), 0), sptField,
                     FxDB(drutama("grnrekdiskon"), ""), sptField,
                     FxDB(drutama("grnrekpajak1"), ""), sptField,
                     FxDB(drutama("grnrekpajak2"), ""), sptField,
                     FxDB(drutama("grnrekbiayalain"), ""), sptField,
                     FxDB(drutama("grnrekbayar"), ""), sptField,
                     FxDB(drutama("grnidpr"), 0), sptField,
                     FxDB(drutama("grnidcs"), 0), sptField,
                     FxDB(drutama("grnidrq"), 0), sptField,
                     FxDB(drutama("grnidbs"), 0), sptField,
                     FxDB(drutama("grnidpo"), 0), sptField,
                     FxDB(drutama("grnidipc"), 0), sptField,
                     FxDB(drutama("grnstatusri"), 0), sptField,
                     FxDB(drutama("grnstatusdnr"), 0), sptField,
                     FxDB(drutama("grnstatusprt"), 0), sptField,
                     FxDB(drutama("grnstatusrealisasi"), 0), sptField,
                     FxDB(drutama("grnstatus"), 0), sptField,
                     FxDB(drutama("grnstatussebelumnya"), 0), sptField,
                     FxDB(drutama("grnjmlrevisi"), 0), sptField,
                     FxDB(drutama("grncetakanke"), 0), sptField,
                     FxDB(drutama("grninputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("grninputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("grnmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("grnmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("grnposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("grnpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("grntutupperiode"), 0), sptField,
                     FxDB(drutama("grnisclose"), 0), sptField,
                     FxDB(drutama("grncustomtext1"), ""), sptField,
                     FxDB(drutama("grncustomtext2"), ""), sptField,
                     FxDB(drutama("grncustomtext3"), ""), sptField,
                     FxDB(drutama("grncustomtext4"), ""), sptField,
                     FxDB(drutama("grncustomtext5"), ""), sptField,
                     FxDB(drutama("grncustomint1"), 0), sptField,
                     FxDB(drutama("grncustomint2"), 0), sptField,
                     FxDB(drutama("grncustomint3"), 0), sptField,
                     FxDB(drutama("grncustomdbl1"), 0), sptField,
                     FxDB(drutama("grncustomdbl2"), 0), sptField,
                     FxDB(drutama("grncustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("grncustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("grncustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("grncustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("grncabangnama"), ""), sptField,
                     FxDB(drutama("grnlokasinama"), ""), sptField,
                     FxDB(drutama("grngudangnama"), ""), sptField,
                     FxDB(drutama("grnsupplierkode"), ""), sptField,
                     FxDB(drutama("grnsuppliernama"), ""), sptField,
                     FxDB(drutama("grnbagianpembeliankode"), ""), sptField,
                     FxDB(drutama("grnbagianpembeliannama"), ""), sptField,
                     FxDB(drutama("grnterminnama"), ""), sptField,
                     FxDB(drutama("grnterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("grnrekdiskonnama"), ""), sptField,
                     FxDB(drutama("grnrekpajak1nama"), ""), sptField,
                     FxDB(drutama("grnrekpajak2nama"), ""), sptField,
                     FxDB(drutama("grnrekbiayalainnama"), ""), sptField,
                     FxDB(drutama("grnrekbayarnama"), ""), sptField,
                     FxDB(drutama("grnnotransaksipr"), ""), sptField,
                     FxDB(drutama("grnnotransaksics"), ""), sptField,
                     FxDB(drutama("grnnotransaksirq"), ""), sptField,
                     FxDB(drutama("grnnotransaksibs"), ""), sptField,
                     FxDB(drutama("grnnotransaksipo"), ""), sptField,
                     FxDB(drutama("grnnotransaksiipc"), ""), sptField,
                     FxDB(drutama("grnstatusnama"), ""), sptField,
                     FxDB(drutama("grnstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("grninputusernama"), ""), sptField,
                     FxDB(drutama("grnmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField, FxDB(dr("idhistory"), 0), sptField, FxDB(dr("idgrndetail"), 0), sptField,
                     FxDB(dr("idgrn"), 0), sptField,
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
                     FxDB(dr("jmlri"), 0), sptField,
                     FxDB(dr("statusri"), 0), sptField,
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

            'AMBIL DATA COST
            sql = "SELECT grnc.idhistorycost, grnc.idhistory, grnc.idgrncost, grnc.idgrn, grnc.kodecost, grnc.matauang, grnc.kurs, grnc.jumlah, grnc.rekdebit, grnc.rekkredit, grnc.kontak, grnc.termasukhpp, grnc.catatan, grnc.costcenter, grnc.divisi, grnc.subdivisi, grnc.proyek, grnc.urutan, grnc.idprcost, grnc.idcscost, grnc.idrqcost, grnc.idbscost, grnc.idpocost, grnc.idipccost, grnc.jumlahri, grnc.statusri, grnc.jumlahbayar, grnc.statusbayar, grnc.isclose, grnc.customtext1, grnc.customtext2, grnc.customtext3, grnc.customdbl1, grnc.customdbl2, grnc.customdbl3, grnc.customdate1, grnc.customdate2, grnc.customdate3, oc.ocnama AS kodecostnama, coa1.cnama AS rekdebitnama, coa2.cnama AS rekkreditnama, c.kkode AS kontakkode, c.knama AS kontaknama, cc.ccnama AS costcenternama, d.dnama AS divisinama, sd.sddivisi AS subdivisinama FROM m4_grn_cost_history grnc JOIN m4_grn_history grn ON grnc.idhistory = grn.grnidhistory LEFT JOIN m1_other_cost oc ON grnc.kodecost = oc.ockode LEFT JOIN m1_coa coa1 ON grnc.rekdebit = coa1.cnomor LEFT JOIN m1_coa coa2 ON grnc.rekkredit = coa2.cnomor LEFT JOIN m1_contact c ON grnc.kontak = c.kid LEFT JOIN m1_cost_center cc ON grnc.costcenter = cc.cckode LEFT JOIN m1_division d ON grnc.divisi = d.dkode LEFT JOIN m1_subdivision sd ON grnc.subdivisi = sd.sdkode"
            Dim dtcost As New DataTable
            dtcost = AmbilData("aplikasi1-m4_po_cost", Filter, "poc.urutan", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtcost.Rows
                cost = String.Concat(cost,
                     FxDB(dr("idhistorycost"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idgrncost"), ""), sptField,
                     FxDB(dr("idgrn"), ""), sptField,
                     FxDB(dr("kodecost"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("jumlah"), 0), sptField,
                     FxDB(dr("rekdebit"), ""), sptField,
                     FxDB(dr("rekkredit"), ""), sptField,
                     FxDB(dr("kontak"), ""), sptField,
                     FxDB(dr("termasukhpp"), 0), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idprcost"), ""), sptField,
                     FxDB(dr("idcscost"), ""), sptField,
                     FxDB(dr("idrqcost"), ""), sptField,
                     FxDB(dr("idbscost"), ""), sptField,
                     FxDB(dr("idpocost"), ""), sptField,
                     FxDB(dr("idipccost"), ""), sptField,
                     FxDB(dr("jumlahri"), 0), sptField,
                     FxDB(dr("statusri"), 0), sptField,
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
                     FxDB(dr("kontakkode"), ""), sptField,
                     FxDB(dr("kontaknama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptRow)
            Next
            If cost.Length > 0 Then cost = cost.Substring(0, cost.Length - sptRow.Length) Else cost = cost

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, batch, sptSubParam, serial, sptSubParam, cost)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("grnidhistory, grnid, grncabang, grnlokasi, grngudang, grnasalbarang, grnasalbarangkategori, grnjenispembelian, grnjenispembeliankategori, grncarabayar, grnsumber, grnautonotransaksi, grnnotransaksi, grntgl, grnkodepa, grnsupplier, grnsupplierkontak, grn1alamat1, grn1alamat2, grn1alamat3, grn2alamat1, grn2alamat2, grn2alamat3, grnbagianpembelian, grntermin, grntgljatuhtempo, grnuraian, grncatatan, grnnoref, grntglnoref, grntglpenutupan, grnmatauang, grnkurs, grnhargatermasukpajak, grntotal, grndiskonpersen, grnjmldiskon, grntotalpajak1detail, grntotalpajak2detail, grnbiayalainpersen, grnbiayalain, grntotaltransaksi, grnjmlbayar, grnrekdiskon, grnrekpajak1, grnrekpajak2, grnrekbiayalain, grnrekbayar, grnidpr, grnidcs, grnidrq, grnidbs, grnidpo, grnidipc, grnstatusri, grnstatusdnr, grnstatusprt, grnstatusrealisasi, grnstatus, grnstatussebelumnya, grnjmlrevisi, grncetakanke, grninputuser, grninputtgl, grnmodifikasiuser, grnmodifikasitgl, grnposting, grnpostingtgl, grntutupperiode, grnisclose, grncustomtext1, grncustomtext2, grncustomtext3, grncustomtext4, grncustomtext5, grncustomint1, grncustomint2, grncustomint3, grncustomdbl1, grncustomdbl2, grncustomdbl3, grncustomdate1, grncustomdate2, grncustomdate3, grncabangnama, grnlokasinama, grngudangnama, grnsupplierkode, grnsuppliernama, grnbagianpembeliankode, grnbagianpembeliannama, grnterminnama, grnterminharijatuhtempo, grnrekdiskonnama, grnrekpajak1nama, grnrekpajak2nama, grnrekbiayalainnama, grnrekbayarnama, grnnotransaksipr, grnnotransaksics, grnnotransaksirq, grnnotransaksibs, grnnotransaksipo, grnnotransaksiipc, grnstatusnama, grnstatussebelumnyanama, grninputusernama, grnmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idgrndetail, idgrn, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, rekpersediaan, rekdiskonpembelian, rekhutangsementara, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, ponotransaksi, ipcnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan" & sptSubParam & "nbtidhistory, nbtidtransaksihistory, nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang" & sptSubParam & "nstidhistory, nstidtransaksihistory, nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang" & sptSubParam & "idhistorydetail, idhistory, idgrncost, idgrn, kodecost, matauang, kurs, jumlah, rekdebit, rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, proyek, urutan, idprcost, idcscost, idrqcost, idbscost, idpocost, idipccost, jumlahri, statusri, jumlahbayar, statusbayar, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodecostnama, rekdebitnama, rekkreditnama, kontakkode, kontaknama, costcenternama, divisinama, subdivisinama"))

        Return wsResult
    End Function

End Class
