Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_rq_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_Rq_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m4_rq_history(SELECT 0, rq.* FROM m4_rq rq WHERE rq.rqid = '" & idtransaksi & "')"
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
            sql = "SELECT rqidhistory FROM m4_rq_history WHERE rqid = '" & idtransaksi & "' ORDER BY rqmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m4_rq_detail_history (SELECT 0, '" & result(4) & "', rq.* FROM m4_rq_detail rq WHERE rq.idrq = '" & idtransaksi & "' )"
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
    Public Function M4_Rq_HistorySearch(ByVal param As String) As String
        'M4_RqSearch --------------------------------------------------------
        'rqidhistory, rqcabang, rqlokasi, rqgudang, rqasalbarang, rqasalbarangkategori, rqjenispembelian, 
        'rqjenispembeliankategori, rqcarabayar, rqsumber, rqautonogrup, rqnogrup, rqautonotransaksi, rqnotransaksi, 
        'rqtgl, rqkodepa, rqsupplier, rqsupplierkontak, rq1alamat1, rq1alamat2, rq1alamat3, 
        'rq2alamat1, rq2alamat2, rq2alamat3, rqbagianpembelian, rqtgldipenuhi, rqtermin, rqtgljatuhtempo, 
        'rquraian, rqcatatan, rqnoref, rqtglnoref, rqtglpenutupan, rqmatauang, rqkurs, 
        'rqhargatermasukpajak, rqtotal, rqdiskonpersen, rqdiskon, rqtotalpajak1detail, rqtotalpajak2detail, rqbiayalainpersen, 
        'rqbiayalain, rqtotaltransaksi, rqidpr, rqidcs, rqstatuspo, rqstatusipc, rqstatusgrn, 
        'rqstatusri, rqstatusdnr, rqstatusprt, rqstatusrealisasi, rqstatus, rqstatussebelumnya, rqjmlrevisi, 
        'rqcetakanke, rqinputuser, rqinputtgl, rqmodifikasiuser, rqmodifikasitgl, rqposting, rqpostingtgl, 
        'rqisclose, rqcabangnama, rqlokasinama, rqgudangnama, rqsupplierkode, rqsuppliernama, rqbagianpembeliankode, 
        'rqbagianpembeliannama, prnotransaksi, csnotransaksi, rqstatusnama, rqstatussebelumnyanama, rqinputusernama, rqmodifikasiusernama

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
            Filter = Filter.Replace("rqsupplierkode", "c1.kkode")
            Filter = Filter.Replace("rqsuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        Dim query As New m0_query
        sql = query.PanggilQuery("m4_rq_v_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Rq_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("rqid"), 0), sptField,
                     FxDB(dr("rqidhistory"), 0), sptField,
                     FxDB(dr("rqcabang"), ""), sptField,
                     FxDB(dr("rqlokasi"), ""), sptField,
                     FxDB(dr("rqgudang"), ""), sptField,
                     FxDB(dr("rqasalbarang"), ""), sptField,
                     FxDB(dr("rqasalbarangkategori"), 0), sptField,
                     FxDB(dr("rqjenispembelian"), ""), sptField,
                     FxDB(dr("rqjenispembeliankategori"), 0), sptField,
                     FxDB(dr("rqcarabayar"), 0), sptField,
                     FxDB(dr("rqsumber"), ""), sptField,
                     FxDB(dr("rqautonogrup"), 0), sptField,
                     FxDB(dr("rqnogrup"), ""), sptField,
                     FxDB(dr("rqautonotransaksi"), 0), sptField,
                     FxDB(dr("rqnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rqtgl"), ""), formatTgl), sptField,
                     FxDB(dr("rqkodepa"), 0), sptField,
                     FxDB(dr("rqsupplier"), 0), sptField,
                     FxDB(dr("rqsupplierkontak"), ""), sptField,
                     FxDB(dr("rq1alamat1"), ""), sptField,
                     FxDB(dr("rq1alamat2"), ""), sptField,
                     FxDB(dr("rq1alamat3"), ""), sptField,
                     FxDB(dr("rq2alamat1"), ""), sptField,
                     FxDB(dr("rq2alamat2"), ""), sptField,
                     FxDB(dr("rq2alamat3"), ""), sptField,
                     FxDB(dr("rqbagianpembelian"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rqtgldipenuhi"), ""), formatTgl), sptField,
                     FxDB(dr("rqtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rqtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("rquraian"), ""), sptField,
                     FxDB(dr("rqcatatan"), ""), sptField,
                     FxDB(dr("rqnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rqtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("rqtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("rqmatauang"), ""), sptField,
                     FxDB(dr("rqkurs"), 0), sptField,
                     FxDB(dr("rqhargatermasukpajak"), 0), sptField,
                     FxDB(dr("rqtotal"), 0), sptField,
                     FxDB(dr("rqdiskonpersen"), ""), sptField,
                     FxDB(dr("rqdiskon"), 0), sptField,
                     FxDB(dr("rqtotalpajak1detail"), 0), sptField,
                     FxDB(dr("rqtotalpajak2detail"), 0), sptField,
                     FxDB(dr("rqbiayalainpersen"), ""), sptField,
                     FxDB(dr("rqbiayalain"), 0), sptField,
                     FxDB(dr("rqtotaltransaksi"), 0), sptField,
                     FxDB(dr("rqidpr"), 0), sptField,
                     FxDB(dr("rqidcs"), 0), sptField,
                     FxDB(dr("rqstatuspo"), 0), sptField,
                     FxDB(dr("rqstatusipc"), 0), sptField,
                     FxDB(dr("rqstatusgrn"), 0), sptField,
                     FxDB(dr("rqstatusri"), 0), sptField,
                     FxDB(dr("rqstatusdnr"), 0), sptField,
                     FxDB(dr("rqstatusprt"), 0), sptField,
                     FxDB(dr("rqstatusrealisasi"), 0), sptField,
                     FxDB(dr("rqstatus"), 0), sptField,
                     FxDB(dr("rqstatussebelumnya"), 0), sptField,
                     FxDB(dr("rqjmlrevisi"), 0), sptField,
                     FxDB(dr("rqcetakanke"), 0), sptField,
                     FxDB(dr("rqinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rqinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rqmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rqmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rqposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rqpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rqisclose"), 0), sptField,
                     FxDB(dr("rqcabangnama"), ""), sptField,
                     FxDB(dr("rqlokasinama"), ""), sptField,
                     FxDB(dr("rqgudangnama"), ""), sptField,
                     FxDB(dr("rqsupplierkode"), ""), sptField,
                     FxDB(dr("rqsuppliernama"), ""), sptField,
                     FxDB(dr("rqbagianpembeliankode"), ""), sptField,
                     FxDB(dr("rqbagianpembeliannama"), ""), sptField,
                     FxDB(dr("prnotransaksi"), ""), sptField,
                     FxDB(dr("csnotransaksi"), ""), sptField,
                     FxDB(dr("rqstatusnama"), ""), sptField,
                     FxDB(dr("rqstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("rqinputusernama"), ""), sptField,
                     FxDB(dr("rqmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rqidhistory, rqid, rqcabang, rqlokasi, rqgudang, rqasalbarang, rqasalbarangkategori, rqjenispembelian, rqjenispembeliankategori, rqcarabayar, rqsumber, rqautonogrup, rqnogrup, rqautonotransaksi, rqnotransaksi, rqtgl, rqkodepa, rqsupplier, rqsupplierkontak, rq1alamat1, rq1alamat2, rq1alamat3, rq2alamat1, rq2alamat2, rq2alamat3, rqbagianpembelian, rqtgldipenuhi, rqtermin, rqtgljatuhtempo, rquraian, rqcatatan, rqnoref, rqtglnoref, rqtglpenutupan, rqmatauang, rqkurs, rqhargatermasukpajak, rqtotal, rqdiskonpersen, rqdiskon, rqtotalpajak1detail, rqtotalpajak2detail, rqbiayalainpersen, rqbiayalain, rqtotaltransaksi, rqidpr, rqidcs, rqstatuspo, rqstatusipc, rqstatusgrn, rqstatusri, rqstatusdnr, rqstatusprt, rqstatusrealisasi, rqstatus, rqstatussebelumnya, rqjmlrevisi, rqcetakanke, rqinputuser, rqinputtgl, rqmodifikasiuser, rqmodifikasitgl, rqposting, rqpostingtgl, rqisclose, rqcabangnama, rqlokasinama, rqgudangnama, rqsupplierkode, rqsuppliernama, rqbagianpembeliankode, rqbagianpembeliannama, prnotransaksi, csnotransaksi, rqstatusnama, rqstatussebelumnyanama, rqinputusernama, rqmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_RqHistoryGetdataById(ByVal param As String) As String

        'M4_RqGetdataById Utama --------------------------------------------------------
        'rqidhistory, rqid, rqcabang, rqlokasi, rqgudang, rqasalbarang, rqasalbarangkategori, rqjenispembelian, 
        'rqjenispembeliankategori, rqcarabayar, rqsumber, rqautonogrup, rqnogrup, rqautonotransaksi, rqnotransaksi, 
        'rqtgl, rqkodepa, rqsupplier, rqsupplierkontak, rq1alamat1, rq1alamat2, rq1alamat3, 
        'rq2alamat1, rq2alamat2, rq2alamat3, rqbagianpembelian, rqtgldipenuhi, rqtermin, rqtgljatuhtempo, 
        'rquraian, rqcatatan, rqnoref, rqtglnoref, rqtglpenutupan, rqmatauang, rqkurs, 
        'rqhargatermasukpajak, rqtotal, rqdiskonpersen, rqdiskon, rqtotalpajak1detail, rqtotalpajak2detail, rqbiayalainpersen, 
        'rqbiayalain, rqtotaltransaksi, rqidpr, rqidcs, rqstatuspo, rqstatusipc, rqstatusgrn, 
        'rqstatusri, rqstatusdnr, rqstatusprt, rqstatusrealisasi, rqstatus, rqstatussebelumnya, rqjmlrevisi, 
        'rqcetakanke, rqinputuser, rqinputtgl, rqmodifikasiuser, rqmodifikasitgl, rqposting, rqpostingtgl, 
        'rqisclose, rqcustomtext1, rqcustomtext2, rqcustomtext3, rqcustomtext4, rqcustomtext5, rqcustomint1, 
        'rqcustomint2, rqcustomint3, rqcustomdbl1, rqcustomdbl2, rqcustomdbl3, rqcustomdate1, rqcustomdate2, 
        'rqcustomdate3, rqcabangnama, rqlokasinama, rqgudangnama, rqsupplierkode, rqsuppliernama, rqbagianpembeliankode, 
        'rqbagianpembeliannama, rqterminnama, rqtermindiskon1, rqterminharidiskon1, rqtermindiskon2, rqterminharidiskon2, rqtermindenda, 
        'rqtermindendaper, rqterminharijatuhtempo, rqnotransaksipr, rqnotransaksics, rqstatusnama, rqstatussebelumnyanama, rqinputusernama, 
        'rqmodifikasiusernama

        'M4_RqGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idrqdetail, idrq, idbarang, namabarang, tipebarang, jml, 
        'satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idprdetail, idcsdetail, jmlpo, statuspo, jmlipc, statusipc, 
        'jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, 
        'statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, 
        'pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, 
        'costcenternama, divisinama, subdivisinama, proyeknama, prnotransaksi, csnotransaksi, jmlsisapo, 
        'jmlsisarealisasi

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

        Dim NmMemcached As String = "aplikasi1-M4_Rq_history~M4_Rq_Detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "rqidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "rqidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m4_rq_getdata_history")
        sql = "select rq.rqidhistory, `rq`.`rqid` AS `rqid`,`rq`.`rqcabang` AS `rqcabang`,`rq`.`rqlokasi` AS `rqlokasi`,`rq`.`rqgudang` AS `rqgudang`,`rq`.`rqasalbarang` AS `rqasalbarang`,`rq`.`rqasalbarangkategori` AS `rqasalbarangkategori`,`rq`.`rqjenispembelian` AS `rqjenispembelian`,`rq`.`rqjenispembeliankategori` AS `rqjenispembeliankategori`,`rq`.`rqcarabayar` AS `rqcarabayar`,`rq`.`rqsumber` AS `rqsumber`,`rq`.`rqautonogrup` AS `rqautonogrup`,`rq`.`rqnogrup` AS `rqnogrup`,`rq`.`rqautonotransaksi` AS `rqautonotransaksi`,`rq`.`rqnotransaksi` AS `rqnotransaksi`,`rq`.`rqtgl` AS `rqtgl`,`rq`.`rqkodepa` AS `rqkodepa`,`rq`.`rqsupplier` AS `rqsupplier`,`rq`.`rqsupplierkontak` AS `rqsupplierkontak`,`rq`.`rq1alamat1` AS `rq1alamat1`,`rq`.`rq1alamat2` AS `rq1alamat2`,`rq`.`rq1alamat3` AS `rq1alamat3`,`rq`.`rq2alamat1` AS `rq2alamat1`,`rq`.`rq2alamat2` AS `rq2alamat2`,`rq`.`rq2alamat3` AS `rq2alamat3`,`rq`.`rqbagianpembelian` AS `rqbagianpembelian`,`rq`.`rqtgldipenuhi` AS `rqtgldipenuhi`,`rq`.`rqtermin` AS `rqtermin`,`rq`.`rqtgljatuhtempo` AS `rqtgljatuhtempo`,`rq`.`rquraian` AS `rquraian`,`rq`.`rqcatatan` AS `rqcatatan`,`rq`.`rqnoref` AS `rqnoref`,`rq`.`rqtglnoref` AS `rqtglnoref`,`rq`.`rqtglpenutupan` AS `rqtglpenutupan`,`rq`.`rqmatauang` AS `rqmatauang`,`rq`.`rqkurs` AS `rqkurs`,`rq`.`rqhargatermasukpajak` AS `rqhargatermasukpajak`,`rq`.`rqtotal` AS `rqtotal`,`rq`.`rqdiskonpersen` AS `rqdiskonpersen`,`rq`.`rqdiskon` AS `rqdiskon`,`rq`.`rqtotalpajak1detail` AS `rqtotalpajak1detail`,`rq`.`rqtotalpajak2detail` AS `rqtotalpajak2detail`,`rq`.`rqbiayalainpersen` AS `rqbiayalainpersen`,`rq`.`rqbiayalain` AS `rqbiayalain`,`rq`.`rqtotaltransaksi` AS `rqtotaltransaksi`,`rq`.`rqidpr` AS `rqidpr`,`rq`.`rqidcs` AS `rqidcs`,`rq`.`rqstatuspo` AS `rqstatuspo`,`rq`.`rqstatusipc` AS `rqstatusipc`,`rq`.`rqstatusgrn` AS `rqstatusgrn`,`rq`.`rqstatusri` AS `rqstatusri`,`rq`.`rqstatusdnr` AS `rqstatusdnr`,`rq`.`rqstatusprt` AS `rqstatusprt`,`rq`.`rqstatusrealisasi` AS `rqstatusrealisasi`,`rq`.`rqstatus` AS `rqstatus`,`rq`.`rqstatussebelumnya` AS `rqstatussebelumnya`,`rq`.`rqjmlrevisi` AS `rqjmlrevisi`,`rq`.`rqcetakanke` AS `rqcetakanke`,`rq`.`rqinputuser` AS `rqinputuser`,`rq`.`rqinputtgl` AS `rqinputtgl`,`rq`.`rqmodifikasiuser` AS `rqmodifikasiuser`,`rq`.`rqmodifikasitgl` AS `rqmodifikasitgl`,`rq`.`rqposting` AS `rqposting`,`rq`.`rqpostingtgl` AS `rqpostingtgl`,`rq`.`rqisclose` AS `rqisclose`,`rq`.`rqcustomtext1` AS `rqcustomtext1`,`rq`.`rqcustomtext2` AS `rqcustomtext2`,`rq`.`rqcustomtext3` AS `rqcustomtext3`,`rq`.`rqcustomtext4` AS `rqcustomtext4`,`rq`.`rqcustomtext5` AS `rqcustomtext5`,`rq`.`rqcustomint1` AS `rqcustomint1`,`rq`.`rqcustomint2` AS `rqcustomint2`,`rq`.`rqcustomint3` AS `rqcustomint3`,`rq`.`rqcustomdbl1` AS `rqcustomdbl1`,`rq`.`rqcustomdbl2` AS `rqcustomdbl2`,`rq`.`rqcustomdbl3` AS `rqcustomdbl3`,`rq`.`rqcustomdate1` AS `rqcustomdate1`,`rq`.`rqcustomdate2` AS `rqcustomdate2`,`rq`.`rqcustomdate3` AS `rqcustomdate3`,`br`.`bnama` AS `rqcabangnama`,`lc`.`lnama` AS `rqlokasinama`,`wh`.`wnama` AS `rqgudangnama`,`c1`.`kkode` AS `rqsupplierkode`,`c1`.`knama` AS `rqsuppliernama`,`c2`.`kkode` AS `rqbagianpembeliankode`,`c2`.`knama` AS `rqbagianpembeliannama`,`tr`.`trnama` AS `rqterminnama`,`tr`.`trdiskon1` AS `rqtermindiskon1`,`tr`.`trharidiskon1` AS `rqterminharidiskon1`,`tr`.`trdiskon2` AS `rqtermindiskon2`,`tr`.`trharidiskon2` AS `rqterminharidiskon2`,`tr`.`trdenda` AS `rqtermindenda`,`tr`.`trdendaper` AS `rqtermindendaper`,`tr`.`trharijatuhtempo` AS `rqterminharijatuhtempo`,`pr`.`prnotransaksi` AS `rqnotransaksipr`,`cs`.`csnotransaksi` AS `rqnotransaksics`,`st1`.`nama` AS `rqstatusnama`,`st2`.`nama` AS `rqstatussebelumnyanama`,`u1`.`unama` AS `rqinputusernama`,`u2`.`unama` AS `rqmodifikasiusernama`,`rqd`.`idrqdetail` AS `idrqdetail`,`rqd`.`idrq` AS `idrq`,`rqd`.`idbarang` AS `idbarang`,`rqd`.`namabarang` AS `namabarang`,`rqd`.`tipebarang` AS `tipebarang`,`rqd`.`jml` AS `jml`,`rqd`.`satuan` AS `satuan`,`rqd`.`nilaisatuan` AS `nilaisatuan`,`rqd`.`jmlbarang` AS `jmlbarang`,`rqd`.`satuanbarang` AS `satuanbarang`,`rqd`.`matauang` AS `matauang`,`rqd`.`kurs` AS `kurs`,`rqd`.`harga` AS `harga`,`rqd`.`diskon` AS `diskon`,`rqd`.`jmldiskon` AS `jmldiskon`,`rqd`.`pajak1` AS `pajak1`,`rqd`.`jmlpajak1` AS `jmlpajak1`,`rqd`.`pajak2` AS `pajak2`,`rqd`.`jmlpajak2` AS `jmlpajak2`,`rqd`.`cabang` AS `cabang`,`rqd`.`lokasi` AS `lokasi`,`rqd`.`gudang` AS `gudang`,`rqd`.`costcenter` AS `costcenter`,`rqd`.`divisi` AS `divisi`,`rqd`.`subdivisi` AS `subdivisi`,`rqd`.`proyek` AS `proyek`,`rqd`.`catatan` AS `catatan`,`rqd`.`urutan` AS `urutan`,`rqd`.`idprdetail` AS `idprdetail`,`rqd`.`idcsdetail` AS `idcsdetail`,`rqd`.`jmlpo` AS `jmlpo`,`rqd`.`statuspo` AS `statuspo`,`rqd`.`jmlipc` AS `jmlipc`,`rqd`.`statusipc` AS `statusipc`,`rqd`.`jmlgrn` AS `jmlgrn`,`rqd`.`statusgrn` AS `statusgrn`,`rqd`.`jmlri` AS `jmlri`,`rqd`.`statusri` AS `statusri`,`rqd`.`jmldnr` AS `jmldnr`,`rqd`.`statusdnr` AS `statusdnr`,`rqd`.`jmlprt` AS `jmlprt`,`rqd`.`statusprt` AS `statusprt`,`rqd`.`jmlrealisasi` AS `jmlrealisasi`,`rqd`.`statusrealisasi` AS `statusrealisasi`,`rqd`.`isclose` AS `isclose`,`rqd`.`customtext1` AS `customtext1`,`rqd`.`customtext2` AS `customtext2`,`rqd`.`customtext3` AS `customtext3`,`rqd`.`customdbl1` AS `customdbl1`,`rqd`.`customdbl2` AS `customdbl2`,`rqd`.`customdbl3` AS `customdbl3`,`rqd`.`customdate1` AS `customdate1`,`rqd`.`customdate2` AS `customdate2`,`rqd`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,`brd`.`bnama` AS `cabangnama`,`lcd`.`lnama` AS `lokasinama`,`whd`.`wnama` AS `gudangnama`,`cc`.`ccnama` AS `costcenternama`,`d`.`dnama` AS `divisinama`,`sd`.`sdnama` AS `subdivisinama`,`p`.`pnama` AS `proyeknama`,`pr2`.`prnotransaksi` AS `prnotransaksi`,`cs2`.`csnotransaksi` AS `csnotransaksi`,((`rqd`.`jmlbarang` - `rqd`.`jmlpo`) / `rqd`.`nilaisatuan`) AS `jmlsisapo`,((`rqd`.`jmlbarang` - `rqd`.`jmlrealisasi`) / `rqd`.`nilaisatuan`) AS `jmlsisarealisasi`, c1.kpkp from (((((((((((((((((((((((((((`m4_rq_history` `rq` join `m4_rq_detail_history` `rqd` on((`rq`.`rqid` = `rqd`.`idrq`))) left join `m1_branch` `br` on((`br`.`bkode` = `rq`.`rqcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `rq`.`rqlokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `rq`.`rqgudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `rq`.`rqsupplier`))) left join `m1_contact` `c2` on((`c2`.`kid` = `rq`.`rqbagianpembelian`))) left join `m1_terms` `tr` on((`rq`.`rqtermin` = `tr`.`trkode`))) left join `m4_pr` `pr` on((`rq`.`rqidpr` = `pr`.`prid`))) left join `m4_cs` `cs` on((`rq`.`rqidcs` = `cs`.`csid`))) left join `m0_status` `st1` on((`st1`.`kode` = `rq`.`rqstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `rq`.`rqstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `rq`.`rqinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `rq`.`rqmodifikasiuser`))) left join `m1_item` `i` on((`i`.`bid` = `rqd`.`idbarang`))) left join `m1_tax` `t1` on((`rqd`.`pajak1` = `t1`.`tkode`))) left join `m1_tax` `t2` on((`rqd`.`pajak2` = `t2`.`tkode`))) left join `m1_branch` `brd` on((`rqd`.`cabang` = `brd`.`bkode`))) left join `m1_location` `lcd` on((`rqd`.`lokasi` = `lcd`.`lkode`))) left join `m1_warehouse` `whd` on((`rqd`.`gudang` = `whd`.`wkode`))) left join `m1_cost_center` `cc` on((`rqd`.`costcenter` = `cc`.`cckode`))) left join `m1_division` `d` on((`rqd`.`divisi` = `d`.`dkode`))) left join `m1_subdivision` `sd` on((`rqd`.`subdivisi` = `sd`.`sdkode`))) left join `m1_project` `p` on((`rqd`.`proyek` = `p`.`pkode`))) left join `m4_pr_detail` `prd` on((`rqd`.`idprdetail` = `prd`.`idprdetail`))) left join `m4_pr` `pr2` on((`prd`.`idpr` = `pr2`.`prid`))) left join `m4_cs_detail` `csd` on((`rqd`.`idcsdetail` = `csd`.`idcsdetail`))) left join `m4_cs` `cs2` on((`csd`.`idcs` = `cs2`.`csid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("rqidhistory"), 0), sptField,
                     FxDB(drutama("rqid"), 0), sptField,
                     FxDB(drutama("rqcabang"), ""), sptField,
                     FxDB(drutama("rqlokasi"), ""), sptField,
                     FxDB(drutama("rqgudang"), ""), sptField,
                     FxDB(drutama("rqasalbarang"), ""), sptField,
                     FxDB(drutama("rqasalbarangkategori"), 0), sptField,
                     FxDB(drutama("rqjenispembelian"), ""), sptField,
                     FxDB(drutama("rqjenispembeliankategori"), 0), sptField,
                     FxDB(drutama("rqcarabayar"), 0), sptField,
                     FxDB(drutama("rqsumber"), ""), sptField,
                     FxDB(drutama("rqautonogrup"), 0), sptField,
                     FxDB(drutama("rqnogrup"), ""), sptField,
                     FxDB(drutama("rqautonotransaksi"), 0), sptField,
                     FxDB(drutama("rqnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rqtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("rqkodepa"), 0), sptField,
                     FxDB(drutama("rqsupplier"), 0), sptField,
                     FxDB(drutama("rqsupplierkontak"), ""), sptField,
                     FxDB(drutama("rq1alamat1"), ""), sptField,
                     FxDB(drutama("rq1alamat2"), ""), sptField,
                     FxDB(drutama("rq1alamat3"), ""), sptField,
                     FxDB(drutama("rq2alamat1"), ""), sptField,
                     FxDB(drutama("rq2alamat2"), ""), sptField,
                     FxDB(drutama("rq2alamat3"), ""), sptField,
                     FxDB(drutama("rqbagianpembelian"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rqtgldipenuhi"), ""), formatTgl), sptField,
                     FxDB(drutama("rqtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rqtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("rquraian"), ""), sptField,
                     FxDB(drutama("rqcatatan"), ""), sptField,
                     FxDB(drutama("rqnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rqtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rqtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("rqmatauang"), ""), sptField,
                     FxDB(drutama("rqkurs"), 0), sptField,
                     FxDB(drutama("rqhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("rqtotal"), 0), sptField,
                     FxDB(drutama("rqdiskonpersen"), ""), sptField,
                     FxDB(drutama("rqdiskon"), 0), sptField,
                     FxDB(drutama("rqtotalpajak1detail"), 0), sptField,
                     FxDB(drutama("rqtotalpajak2detail"), 0), sptField,
                     FxDB(drutama("rqbiayalainpersen"), ""), sptField,
                     FxDB(drutama("rqbiayalain"), 0), sptField,
                     FxDB(drutama("rqtotaltransaksi"), 0), sptField,
                     FxDB(drutama("rqidpr"), 0), sptField,
                     FxDB(drutama("rqidcs"), 0), sptField,
                     FxDB(drutama("rqstatuspo"), 0), sptField,
                     FxDB(drutama("rqstatusipc"), 0), sptField,
                     FxDB(drutama("rqstatusgrn"), 0), sptField,
                     FxDB(drutama("rqstatusri"), 0), sptField,
                     FxDB(drutama("rqstatusdnr"), 0), sptField,
                     FxDB(drutama("rqstatusprt"), 0), sptField,
                     FxDB(drutama("rqstatusrealisasi"), 0), sptField,
                     FxDB(drutama("rqstatus"), 0), sptField,
                     FxDB(drutama("rqstatussebelumnya"), 0), sptField,
                     FxDB(drutama("rqjmlrevisi"), 0), sptField,
                     FxDB(drutama("rqcetakanke"), 0), sptField,
                     FxDB(drutama("rqinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rqinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rqmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rqmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rqposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rqpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rqisclose"), 0), sptField,
                     FxDB(drutama("rqcustomtext1"), ""), sptField,
                     FxDB(drutama("rqcustomtext2"), ""), sptField,
                     FxDB(drutama("rqcustomtext3"), ""), sptField,
                     FxDB(drutama("rqcustomtext4"), ""), sptField,
                     FxDB(drutama("rqcustomtext5"), ""), sptField,
                     FxDB(drutama("rqcustomint1"), 0), sptField,
                     FxDB(drutama("rqcustomint2"), 0), sptField,
                     FxDB(drutama("rqcustomint3"), 0), sptField,
                     FxDB(drutama("rqcustomdbl1"), 0), sptField,
                     FxDB(drutama("rqcustomdbl2"), 0), sptField,
                     FxDB(drutama("rqcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rqcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rqcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rqcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("rqcabangnama"), ""), sptField,
                     FxDB(drutama("rqlokasinama"), ""), sptField,
                     FxDB(drutama("rqgudangnama"), ""), sptField,
                     FxDB(drutama("rqsupplierkode"), ""), sptField,
                     FxDB(drutama("rqsuppliernama"), ""), sptField,
                     FxDB(drutama("rqbagianpembeliankode"), ""), sptField,
                     FxDB(drutama("rqbagianpembeliannama"), ""), sptField,
                     FxDB(drutama("rqterminnama"), ""), sptField,
                     FxDB(drutama("rqtermindiskon1"), 0), sptField,
                     FxDB(drutama("rqterminharidiskon1"), 0), sptField,
                     FxDB(drutama("rqtermindiskon2"), 0), sptField,
                     FxDB(drutama("rqterminharidiskon2"), 0), sptField,
                     FxDB(drutama("rqtermindenda"), 0), sptField,
                     FxDB(drutama("rqtermindendaper"), 0), sptField,
                     FxDB(drutama("rqterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("rqnotransaksipr"), ""), sptField,
                     FxDB(drutama("rqnotransaksics"), ""), sptField,
                     FxDB(drutama("rqstatusnama"), ""), sptField,
                     FxDB(drutama("rqstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("rqinputusernama"), ""), sptField,
                     FxDB(drutama("rqmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idrqdetail"), 0), sptField,
                     FxDB(dr("idrq"), 0), sptField,
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
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idprdetail"), 0), sptField,
                     FxDB(dr("idcsdetail"), 0), sptField,
                     FxDB(dr("jmlpo"), 0), sptField,
                     FxDB(dr("statuspo"), 0), sptField,
                     FxDB(dr("jmlipc"), 0), sptField,
                     FxDB(dr("statusipc"), 0), sptField,
                     FxDB(dr("jmlgrn"), 0), sptField,
                     FxDB(dr("statusgrn"), 0), sptField,
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
                     FxDB(dr("prnotransaksi"), ""), sptField,
                     FxDB(dr("csnotransaksi"), ""), sptField,
                     FxDB(dr("jmlsisapo"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptRow)

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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rqidhistory, rqid, rqcabang, rqlokasi, rqgudang, rqasalbarang, rqasalbarangkategori, rqjenispembelian, rqjenispembeliankategori, rqcarabayar, rqsumber, rqautonogrup, rqnogrup, rqautonotransaksi, rqnotransaksi, rqtgl, rqkodepa, rqsupplier, rqsupplierkontak, rq1alamat1, rq1alamat2, rq1alamat3, rq2alamat1, rq2alamat2, rq2alamat3, rqbagianpembelian, rqtgldipenuhi, rqtermin, rqtgljatuhtempo, rquraian, rqcatatan, rqnoref, rqtglnoref, rqtglpenutupan, rqmatauang, rqkurs, rqhargatermasukpajak, rqtotal, rqdiskonpersen, rqdiskon, rqtotalpajak1detail, rqtotalpajak2detail, rqbiayalainpersen, rqbiayalain, rqtotaltransaksi, rqidpr, rqidcs, rqstatuspo, rqstatusipc, rqstatusgrn, rqstatusri, rqstatusdnr, rqstatusprt, rqstatusrealisasi, rqstatus, rqstatussebelumnya, rqjmlrevisi, rqcetakanke, rqinputuser, rqinputtgl, rqmodifikasiuser, rqmodifikasitgl, rqposting, rqpostingtgl, rqisclose, rqcustomtext1, rqcustomtext2, rqcustomtext3, rqcustomtext4, rqcustomtext5, rqcustomint1, rqcustomint2, rqcustomint3, rqcustomdbl1, rqcustomdbl2, rqcustomdbl3, rqcustomdate1, rqcustomdate2, rqcustomdate3, rqcabangnama, rqlokasinama, rqgudangnama, rqsupplierkode, rqsuppliernama, rqbagianpembeliankode, rqbagianpembeliannama, rqterminnama, rqtermindiskon1, rqterminharidiskon1, rqtermindiskon2, rqterminharidiskon2, rqtermindenda, rqtermindendaper, rqterminharijatuhtempo, rqnotransaksipr, rqnotransaksics, rqstatusnama, rqstatussebelumnyanama, rqinputusernama, rqmodifikasiusernama, kpkp" & sptSubParam & "idhistorydetail, idhistory, idrqdetail, idrq, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, jmlpo, statuspo, jmlipc, statusipc, jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, prnotransaksi, csnotransaksi, jmlsisapo, jmlsisarealisasi"))

        Return wsResult
    End Function
End Class
