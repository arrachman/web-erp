Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_dnr_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function m4_Dnr_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m4_dnr_history(SELECT 0, dnr.* FROM m4_dnr dnr WHERE dnr.dnrid = '" & idtransaksi & "')"
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
            sql = "SELECT dnridhistory FROM m4_dnr_history WHERE dnrid = '" & idtransaksi & "' ORDER BY dnrmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m4_dnr_detail_history (SELECT 0, '" & result(4) & "', dnr.* FROM m4_dnr_detail dnr WHERE dnr.iddnr = '" & idtransaksi & "' )"
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
            sql = "INSERT INTO m1_no_batch_transaction_history(SELECT 0, '" & result(4) & "', nb.* FROM m1_no_batch_transaction nb WHERE nb.nbtidtransaksi = '" & idtransaksi & "' and nb.nbtsumber = 'DNR')"
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
            sql = "INSERT INTO m1_no_serial_transaction_history(SELECT 0, '" & result(4) & "', ns.* FROM m1_no_serial_transaction ns WHERE ns.nstidtransaksi = '" & idtransaksi & "' and ns.nstsumber = 'DNR')"
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
            sql = "INSERT INTO m7_asset_transaction_history(SELECT 0, '" & result(4) & "', atr.* FROM m7_asset_transaction atr WHERE atr.atidutama = '" & idtransaksi & "' and atr.atsumber = 'DNR')"
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
    Public Function M4_Dnr_HistorySearch(ByVal param As String) As String
        'M4_Dnr_HistorySearch --------------------------------------------------------
        'dnridhistory, dnrid, dnrcabang, dnrlokasi, dnrgudang, dnrasalbarang, dnrasalbarangkategori, dnrjenispembelian, 
        'dnrjenispembeliankategori, dnrcarabayar, dnrsumber, dnrautonotransaksi, dnrnotransaksi, dnrtgl, dnrkodepa, 
        'dnrsupplier, dnrsupplierkontak, dnr1alamat1, dnr1alamat2, dnr1alamat3, dnr2alamat1, dnr2alamat2, 
        'dnr2alamat3, dnrbagianpembelian, dnrtermin, dnrtgljatuhtempo, dnruraian, dnrcatatan, dnrnoref, 
        'dnrtglnoref, dnrtglpenutupan, dnrmatauang, dnrkurs, dnrhargatermasukpajak, dnrtotal, dnrdiskonpersen, 
        'dnrjmldiskon, dnrtotalpajak1detail, dnrtotalpajak2detail, dnrbiayalainpersen, dnrbiayalain, dnrtotaltransaksi, dnrjmlbayar, 
        'dnrstatuslunas, dnrtgllunas, dnrnofakturpajak, dnrsdhbayarpajak, dnrtglbayarpajak, dnrrekdiskon, dnrrekpajak1, 
        'dnrrekpajak2, dnrrekbiayalain, dnrrekbayar, dnridpr, dnridcs, dnridrq, dnridbs, 
        'dnridpo, dnridipc, dnridgrn, dnridri, dnrstatusprt, dnrstatusrealisasi, dnrstatus, 
        'dnrstatussebelumnya, dnrjmlrevisi, dnrcetakanke, dnrinputuser, dnrinputtgl, dnrmodifikasiuser, dnrmodifikasitgl, 
        'dnrposting, dnrpostingtgl, dnrtutupperiode, dnrisclose, dnrcabangnama, dnrlokasinama, dnrgudangnama, 
        'dnrsupplierkode, dnrsuppliernama, dnrbagianpembeliankode, dnrbagianpembeliannama, grnnotransaksi, rinotransaksi, dnrstatusnama, 
        'dnrstatussebelumnyanama, dnrinputusernama, dnrmodifikasiusernama

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
            Filter = Filter.Replace("dnrsupplierkode", "c1.kkode")
            Filter = Filter.Replace("dnrsuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_dnr_v_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Dnr", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("dnrid"), 0), sptField,
                     FxDB(dr("dnridhistory"), 0), sptField,
                     FxDB(dr("dnrcabang"), ""), sptField,
                     FxDB(dr("dnrlokasi"), ""), sptField,
                     FxDB(dr("dnrgudang"), ""), sptField,
                     FxDB(dr("dnrasalbarang"), ""), sptField,
                     FxDB(dr("dnrasalbarangkategori"), 0), sptField,
                     FxDB(dr("dnrjenispembelian"), ""), sptField,
                     FxDB(dr("dnrjenispembeliankategori"), 0), sptField,
                     FxDB(dr("dnrcarabayar"), 0), sptField,
                     FxDB(dr("dnrsumber"), ""), sptField,
                     FxDB(dr("dnrautonotransaksi"), 0), sptField,
                     FxDB(dr("dnrnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dnrtgl"), ""), formatTgl), sptField,
                     FxDB(dr("dnrkodepa"), 0), sptField,
                     FxDB(dr("dnrsupplier"), 0), sptField,
                     FxDB(dr("dnrsupplierkontak"), ""), sptField,
                     FxDB(dr("dnr1alamat1"), ""), sptField,
                     FxDB(dr("dnr1alamat2"), ""), sptField,
                     FxDB(dr("dnr1alamat3"), ""), sptField,
                     FxDB(dr("dnr2alamat1"), ""), sptField,
                     FxDB(dr("dnr2alamat2"), ""), sptField,
                     FxDB(dr("dnr2alamat3"), ""), sptField,
                     FxDB(dr("dnrbagianpembelian"), 0), sptField,
                     FxDB(dr("dnrtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dnrtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("dnruraian"), ""), sptField,
                     FxDB(dr("dnrcatatan"), ""), sptField,
                     FxDB(dr("dnrnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dnrtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("dnrtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("dnrmatauang"), ""), sptField,
                     FxDB(dr("dnrkurs"), 0), sptField,
                     FxDB(dr("dnrhargatermasukpajak"), 0), sptField,
                     FxDB(dr("dnrtotal"), 0), sptField,
                     FxDB(dr("dnrdiskonpersen"), ""), sptField,
                     FxDB(dr("dnrjmldiskon"), 0), sptField,
                     FxDB(dr("dnrtotalpajak1detail"), 0), sptField,
                     FxDB(dr("dnrtotalpajak2detail"), 0), sptField,
                     FxDB(dr("dnrbiayalainpersen"), ""), sptField,
                     FxDB(dr("dnrbiayalain"), 0), sptField,
                     FxDB(dr("dnrtotaltransaksi"), 0), sptField,
                     FxDB(dr("dnrjmlbayar"), 0), sptField,
                     FxDB(dr("dnrstatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("dnrtgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("dnrnofakturpajak"), ""), sptField,
                     FxDB(dr("dnrsdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("dnrtglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(dr("dnrrekdiskon"), ""), sptField,
                     FxDB(dr("dnrrekpajak1"), ""), sptField,
                     FxDB(dr("dnrrekpajak2"), ""), sptField,
                     FxDB(dr("dnrrekbiayalain"), ""), sptField,
                     FxDB(dr("dnrrekbayar"), ""), sptField,
                     FxDB(dr("dnridpr"), 0), sptField,
                     FxDB(dr("dnridcs"), 0), sptField,
                     FxDB(dr("dnridrq"), 0), sptField,
                     FxDB(dr("dnridbs"), 0), sptField,
                     FxDB(dr("dnridpo"), 0), sptField,
                     FxDB(dr("dnridipc"), 0), sptField,
                     FxDB(dr("dnridgrn"), 0), sptField,
                     FxDB(dr("dnridri"), 0), sptField,
                     FxDB(dr("dnrstatusprt"), 0), sptField,
                     FxDB(dr("dnrstatusrealisasi"), 0), sptField,
                     FxDB(dr("dnrstatus"), 0), sptField,
                     FxDB(dr("dnrstatussebelumnya"), 0), sptField,
                     FxDB(dr("dnrjmlrevisi"), 0), sptField,
                     FxDB(dr("dnrcetakanke"), 0), sptField,
                     FxDB(dr("dnrinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("dnrinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("dnrmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("dnrmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("dnrposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("dnrpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("dnrtutupperiode"), 0), sptField,
                     FxDB(dr("dnrisclose"), 0), sptField,
                     FxDB(dr("dnrcabangnama"), ""), sptField,
                     FxDB(dr("dnrlokasinama"), ""), sptField,
                     FxDB(dr("dnrgudangnama"), ""), sptField,
                     FxDB(dr("dnrsupplierkode"), ""), sptField,
                     FxDB(dr("dnrsuppliernama"), ""), sptField,
                     FxDB(dr("dnrbagianpembeliankode"), ""), sptField,
                     FxDB(dr("dnrbagianpembeliannama"), ""), sptField,
                     FxDB(dr("grnnotransaksi"), ""), sptField,
                     FxDB(dr("rinotransaksi"), ""), sptField,
                     FxDB(dr("dnrstatusnama"), ""), sptField,
                     FxDB(dr("dnrstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("dnrinputusernama"), ""), sptField,
                     FxDB(dr("dnrmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("dnridhistory,dnrid, dnrcabang, dnrlokasi, dnrgudang, dnrasalbarang, dnrasalbarangkategori, dnrjenispembelian, dnrjenispembeliankategori, dnrcarabayar, dnrsumber, dnrautonotransaksi, dnrnotransaksi, dnrtgl, dnrkodepa, dnrsupplier, dnrsupplierkontak, dnr1alamat1, dnr1alamat2, dnr1alamat3, dnr2alamat1, dnr2alamat2, dnr2alamat3, dnrbagianpembelian, dnrtermin, dnrtgljatuhtempo, dnruraian, dnrcatatan, dnrnoref, dnrtglnoref, dnrtglpenutupan, dnrmatauang, dnrkurs, dnrhargatermasukpajak, dnrtotal, dnrdiskonpersen, dnrjmldiskon, dnrtotalpajak1detail, dnrtotalpajak2detail, dnrbiayalainpersen, dnrbiayalain, dnrtotaltransaksi, dnrjmlbayar, dnrstatuslunas, dnrtgllunas, dnrnofakturpajak, dnrsdhbayarpajak, dnrtglbayarpajak, dnrrekdiskon, dnrrekpajak1, dnrrekpajak2, dnrrekbiayalain, dnrrekbayar, dnridpr, dnridcs, dnridrq, dnridbs, dnridpo, dnridipc, dnridgrn, dnridri, dnrstatusprt, dnrstatusrealisasi, dnrstatus, dnrstatussebelumnya, dnrjmlrevisi, dnrcetakanke, dnrinputuser, dnrinputtgl, dnrmodifikasiuser, dnrmodifikasitgl, dnrposting, dnrpostingtgl, dnrtutupperiode, dnrisclose, dnrcabangnama, dnrlokasinama, dnrgudangnama, dnrsupplierkode, dnrsuppliernama, dnrbagianpembeliankode, dnrbagianpembeliannama, grnnotransaksi, rinotransaksi, dnrstatusnama, dnrstatussebelumnyanama, dnrinputusernama, dnrmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_DnrHistoryGetdataById(ByVal param As String) As String

        'M4_DnrHistoryGetdataById Utama --------------------------------------------------------
        'dnrhistory, dnrid, dnrcabang, dnrlokasi, dnrgudang, dnrasalbarang, dnrasalbarangkategori, dnrjenispembelian, 
        'dnrjenispembeliankategori, dnrcarabayar, dnrsumber, dnrautonotransaksi, dnrnotransaksi, dnrtgl, dnrkodepa, 
        'dnrsupplier, dnrsupplierkontak, dnr1alamat1, dnr1alamat2, dnr1alamat3, dnr2alamat1, dnr2alamat2, 
        'dnr2alamat3, dnrbagianpembelian, dnrtermin, dnrtgljatuhtempo, dnruraian, dnrcatatan, dnrnoref, 
        'dnrtglnoref, dnrtglpenutupan, dnrmatauang, dnrkurs, dnrhargatermasukpajak, dnrtotal, dnrdiskonpersen, 
        'dnrjmldiskon, dnrtotalpajak1detail, dnrtotalpajak2detail, dnrbiayalainpersen, dnrbiayalain, dnrtotaltransaksi, dnrjmlbayar, 
        'dnrstatuslunas, dnrtgllunas, dnrnofakturpajak, dnrsdhbayarpajak, dnrtglbayarpajak, dnrrekdiskon, dnrrekpajak1, 
        'dnrrekpajak2, dnrrekbiayalain, dnrrekbayar, dnridpr, dnridcs, dnridrq, dnridbs, 
        'dnridpo, dnridipc, dnridgrn, dnridri, dnrstatusprt, dnrstatusrealisasi, dnrstatus, 
        'dnrstatussebelumnya, dnrjmlrevisi, dnrcetakanke, dnrinputuser, dnrinputtgl, dnrmodifikasiuser, dnrmodifikasitgl, 
        'dnrposting, dnrpostingtgl, dnrtutupperiode, dnrisclose, dnrcustomtext1, dnrcustomtext2, dnrcustomtext3, 
        'dnrcustomtext4, dnrcustomtext5, dnrcustomint1, dnrcustomint2, dnrcustomint3, dnrcustomdbl1, dnrcustomdbl2, 
        'dnrcustomdbl3, dnrcustomdate1, dnrcustomdate2, dnrcustomdate3, dnrcabangnama, dnrlokasinama, dnrgudangnama, 
        'dnrsupplierkode, dnrsuppliernama, dnrbagianpembeliankode, dnrbagianpembeliannama, dnrterminnama, dnrterminharijatuhtempo, dnrrekdiskonnama, 
        'dnrrekpajak1nama, dnrrekpajak2nama, dnrrekbiayalainnama, dnrrekbayarnama, dnrnotransaksigrn, dnrnotransaksiri, dnrstatusnama, 
        'dnrstatussebelumnyanama, dnrinputusernama, dnrmodifikasiusernama

        'M4_DnrHistoryGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, iddnrdetail, iddnr, idbarang, namabarang, 
        'tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, 
        'kurs, hargafix, idhppkhususmasuk, idhppfifomasuk, hpp, harga, diskon, 
        'jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, 
        'gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekdiskonpembelian, rekhargapokok, rekreturpembelian, 
        'costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, 
        'idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, idgrndetail, idridetail, 
        'jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, 
        'customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'kodebarang, bhpp, bjenis, bserial, bbatch, pajak1nama, pajak1nilai, 
        'pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangasalnama, gudangtransitnama, gudangtujuannama, 
        'costcenternama, divisinama, subdivisinama, proyeknama, idgrn, grnnotransaksi, rinotransaksi, 
        'bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan

        'M4_DnrHistoryGetdataById Batch --------------------------------------------------------
        'nbtidhistory, nbtidtransaksihistory, nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang

        'M4_DnrHistoryGetdataById Serial --------------------------------------------------------
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
        Dim sumber As String = "DNR"

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

        Dim NmMemcached As String = "aplikasi1-M4_Dnr~M4_Dnr_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "dnridhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "dnridhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_dnr_getdata_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("dnridhistory"), 0), sptField,
                     FxDB(drutama("dnrid"), 0), sptField,
                     FxDB(drutama("dnrcabang"), ""), sptField,
                     FxDB(drutama("dnrlokasi"), ""), sptField,
                     FxDB(drutama("dnrgudang"), ""), sptField,
                     FxDB(drutama("dnrasalbarang"), ""), sptField,
                     FxDB(drutama("dnrasalbarangkategori"), 0), sptField,
                     FxDB(drutama("dnrjenispembelian"), ""), sptField,
                     FxDB(drutama("dnrjenispembeliankategori"), 0), sptField,
                     FxDB(drutama("dnrcarabayar"), 0), sptField,
                     FxDB(drutama("dnrsumber"), ""), sptField,
                     FxDB(drutama("dnrautonotransaksi"), 0), sptField,
                     FxDB(drutama("dnrnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("dnrtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("dnrkodepa"), 0), sptField,
                     FxDB(drutama("dnrsupplier"), 0), sptField,
                     FxDB(drutama("dnrsupplierkontak"), ""), sptField,
                     FxDB(drutama("dnr1alamat1"), ""), sptField,
                     FxDB(drutama("dnr1alamat2"), ""), sptField,
                     FxDB(drutama("dnr1alamat3"), ""), sptField,
                     FxDB(drutama("dnr2alamat1"), ""), sptField,
                     FxDB(drutama("dnr2alamat2"), ""), sptField,
                     FxDB(drutama("dnr2alamat3"), ""), sptField,
                     FxDB(drutama("dnrbagianpembelian"), 0), sptField,
                     FxDB(drutama("dnrtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("dnrtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("dnruraian"), ""), sptField,
                     FxDB(drutama("dnrcatatan"), ""), sptField,
                     FxDB(drutama("dnrnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("dnrtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("dnrtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("dnrmatauang"), ""), sptField,
                     FxDB(drutama("dnrkurs"), 0), sptField,
                     FxDB(drutama("dnrhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("dnrtotal"), 0), sptField,
                     FxDB(drutama("dnrdiskonpersen"), ""), sptField,
                     FxDB(drutama("dnrjmldiskon"), 0), sptField,
                     FxDB(drutama("dnrtotalpajak1detail"), 0), sptField,
                     FxDB(drutama("dnrtotalpajak2detail"), 0), sptField,
                     FxDB(drutama("dnrbiayalainpersen"), ""), sptField,
                     FxDB(drutama("dnrbiayalain"), 0), sptField,
                     FxDB(drutama("dnrtotaltransaksi"), 0), sptField,
                     FxDB(drutama("dnrjmlbayar"), 0), sptField,
                     FxDB(drutama("dnrstatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("dnrtgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("dnrnofakturpajak"), ""), sptField,
                     FxDB(drutama("dnrsdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("dnrtglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(drutama("dnrrekdiskon"), ""), sptField,
                     FxDB(drutama("dnrrekpajak1"), ""), sptField,
                     FxDB(drutama("dnrrekpajak2"), ""), sptField,
                     FxDB(drutama("dnrrekbiayalain"), ""), sptField,
                     FxDB(drutama("dnrrekbayar"), ""), sptField,
                     FxDB(drutama("dnridpr"), 0), sptField,
                     FxDB(drutama("dnridcs"), 0), sptField,
                     FxDB(drutama("dnridrq"), 0), sptField,
                     FxDB(drutama("dnridbs"), 0), sptField,
                     FxDB(drutama("dnridpo"), 0), sptField,
                     FxDB(drutama("dnridipc"), 0), sptField,
                     FxDB(drutama("dnridgrn"), 0), sptField,
                     FxDB(drutama("dnridri"), 0), sptField,
                     FxDB(drutama("dnrstatusprt"), 0), sptField,
                     FxDB(drutama("dnrstatusrealisasi"), 0), sptField,
                     FxDB(drutama("dnrstatus"), 0), sptField,
                     FxDB(drutama("dnrstatussebelumnya"), 0), sptField,
                     FxDB(drutama("dnrjmlrevisi"), 0), sptField,
                     FxDB(drutama("dnrcetakanke"), 0), sptField,
                     FxDB(drutama("dnrinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("dnrinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("dnrmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("dnrmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("dnrposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("dnrpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("dnrtutupperiode"), 0), sptField,
                     FxDB(drutama("dnrisclose"), 0), sptField,
                     FxDB(drutama("dnrcustomtext1"), ""), sptField,
                     FxDB(drutama("dnrcustomtext2"), ""), sptField,
                     FxDB(drutama("dnrcustomtext3"), ""), sptField,
                     FxDB(drutama("dnrcustomtext4"), ""), sptField,
                     FxDB(drutama("dnrcustomtext5"), ""), sptField,
                     FxDB(drutama("dnrcustomint1"), 0), sptField,
                     FxDB(drutama("dnrcustomint2"), 0), sptField,
                     FxDB(drutama("dnrcustomint3"), 0), sptField,
                     FxDB(drutama("dnrcustomdbl1"), 0), sptField,
                     FxDB(drutama("dnrcustomdbl2"), 0), sptField,
                     FxDB(drutama("dnrcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("dnrcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("dnrcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("dnrcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("dnrcabangnama"), ""), sptField,
                     FxDB(drutama("dnrlokasinama"), ""), sptField,
                     FxDB(drutama("dnrgudangnama"), ""), sptField,
                     FxDB(drutama("dnrsupplierkode"), ""), sptField,
                     FxDB(drutama("dnrsuppliernama"), ""), sptField,
                     FxDB(drutama("dnrbagianpembeliankode"), ""), sptField,
                     FxDB(drutama("dnrbagianpembeliannama"), ""), sptField,
                     FxDB(drutama("dnrterminnama"), ""), sptField,
                     FxDB(drutama("dnrterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("dnrrekdiskonnama"), ""), sptField,
                     FxDB(drutama("dnrrekpajak1nama"), ""), sptField,
                     FxDB(drutama("dnrrekpajak2nama"), ""), sptField,
                     FxDB(drutama("dnrrekbiayalainnama"), ""), sptField,
                     FxDB(drutama("dnrrekbayarnama"), ""), sptField,
                     FxDB(drutama("dnrnotransaksigrn"), ""), sptField,
                     FxDB(drutama("dnrnotransaksiri"), ""), sptField,
                     FxDB(drutama("dnrstatusnama"), ""), sptField,
                     FxDB(drutama("dnrstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("dnrinputusernama"), ""), sptField,
                     FxDB(drutama("dnrmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idhistorydetail"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("iddnrdetail"), 0), sptField,
                     FxDB(dr("iddnr"), 0), sptField,
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
                     FxDB(dr("gudangasalnama"), ""), sptField,
                     FxDB(dr("gudangtransitnama"), ""), sptField,
                     FxDB(dr("gudangtujuannama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("idgrn"), 0), sptField,
                     FxDB(dr("grnnotransaksi"), ""), sptField,
                     FxDB(dr("rinotransaksi"), ""), sptField,
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
            result(2) = "transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, batch, sptSubParam, serial)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("dnridhistory, dnrid, dnrcabang, dnrlokasi, dnrgudang, dnrasalbarang, dnrasalbarangkategori, dnrjenispembelian, dnrjenispembeliankategori, dnrcarabayar, dnrsumber, dnrautonotransaksi, dnrnotransaksi, dnrtgl, dnrkodepa, dnrsupplier, dnrsupplierkontak, dnr1alamat1, dnr1alamat2, dnr1alamat3, dnr2alamat1, dnr2alamat2, dnr2alamat3, dnrbagianpembelian, dnrtermin, dnrtgljatuhtempo, dnruraian, dnrcatatan, dnrnoref, dnrtglnoref, dnrtglpenutupan, dnrmatauang, dnrkurs, dnrhargatermasukpajak, dnrtotal, dnrdiskonpersen, dnrjmldiskon, dnrtotalpajak1detail, dnrtotalpajak2detail, dnrbiayalainpersen, dnrbiayalain, dnrtotaltransaksi, dnrjmlbayar, dnrstatuslunas, dnrtgllunas, dnrnofakturpajak, dnrsdhbayarpajak, dnrtglbayarpajak, dnrrekdiskon, dnrrekpajak1, dnrrekpajak2, dnrrekbiayalain, dnrrekbayar, dnridpr, dnridcs, dnridrq, dnridbs, dnridpo, dnridipc, dnridgrn, dnridri, dnrstatusprt, dnrstatusrealisasi, dnrstatus, dnrstatussebelumnya, dnrjmlrevisi, dnrcetakanke, dnrinputuser, dnrinputtgl, dnrmodifikasiuser, dnrmodifikasitgl, dnrposting, dnrpostingtgl, dnrtutupperiode, dnrisclose, dnrcustomtext1, dnrcustomtext2, dnrcustomtext3, dnrcustomtext4, dnrcustomtext5, dnrcustomint1, dnrcustomint2, dnrcustomint3, dnrcustomdbl1, dnrcustomdbl2, dnrcustomdbl3, dnrcustomdate1, dnrcustomdate2, dnrcustomdate3, dnrcabangnama, dnrlokasinama, dnrgudangnama, dnrsupplierkode, dnrsuppliernama, dnrbagianpembeliankode, dnrbagianpembeliannama, dnrterminnama, dnrterminharijatuhtempo, dnrrekdiskonnama, dnrrekpajak1nama, dnrrekpajak2nama, dnrrekbiayalainnama, dnrrekbayarnama, dnrnotransaksigrn, dnrnotransaksiri, dnrstatusnama, dnrstatussebelumnyanama, dnrinputusernama, dnrmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, iddnrdetail, iddnr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, idhppkhususmasuk, idhppfifomasuk, hpp, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekdiskonpembelian, rekhargapokok, rekreturpembelian, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, idgrndetail, idridetail, jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangasalnama, gudangtransitnama, gudangtujuannama, costcenternama, divisinama, subdivisinama, proyeknama, idgrn, grnnotransaksi, rinotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan" & sptSubParam & "nbtidhistory, nbtidtransaksihistory, nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang" & sptSubParam & "nstidhistory, nstidtransaksihistory, nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang"))

        Return wsResult
    End Function
End Class
