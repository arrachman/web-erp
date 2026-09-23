Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_rfq_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_Rfq_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m4_rfq_history(SELECT 0, rfq.* FROM m4_rfq rfq WHERE rfq.rfqid = '" & idtransaksi & "')"
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
            sql = "SELECT rfqidhistory FROM m4_rfq_history WHERE rfqid = '" & idtransaksi & "' ORDER BY rfqmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m4_rfq_detail_history (SELECT 0, '" & result(4) & "', rfq.* FROM m4_rfq_detail rfq WHERE rfq.idrfq = '" & idtransaksi & "' )"
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
    Public Function M4_Rfq_HistorySearch(ByVal param As String) As String
        'M4_RfqSearch --------------------------------------------------------
        'rfqidhistory, rfqcabang, rfqlokasi, rfqgudang, rfqasalbarang, rfqasalbarangkategori, rfqjenispembelian, 
        'rfqjenispembeliankategori, rfqcarabayar, rfqsumber, rfqautonogrup, rfqnogrup, rfqautonotransaksi, rfqnotransaksi, 
        'rfqtgl, rfqkodepa, rfqsupplier, rfqsupplierkontak, rfq1alamat1, rfq1alamat2, rfq1alamat3, 
        'rfq2alamat1, rfq2alamat2, rfq2alamat3, rfqbagianpembelian, rfqtgldipenuhi, rfqtermin, rfqtgljatuhtempo, 
        'rfquraian, rfqcatatan, rfqnoref, rfqtglnoref, rfqtglpenutupan, rfqmatauang, rfqkurs, 
        'rfqhargatermasukpajak, rfqtotal, rfqdiskonpersen, rfqdiskon, rfqtotalpajak1detail, rfqtotalpajak2detail, rfqbiayalainpersen, 
        'rfqbiayalain, rfqtotaltransaksi, rfqidpr, rfqidcs, rfqstatuspo, rfqstatusipc, rfqstatusgrn, 
        'rfqstatusri, rfqstatusdnr, rfqstatusprt, rfqstatusrealisasi, rfqstatus, rfqstatussebelumnya, rfqjmlrevisi, 
        'rfqcetakanke, rfqinputuser, rfqinputtgl, rfqmodifikasiuser, rfqmodifikasitgl, rfqposting, rfqpostingtgl, 
        'rfqisclose, rfqcabangnama, rfqlokasinama, rfqgudangnama, rfqsupplierkode, rfqsuppliernama, rfqbagianpembeliankode, 
        'rfqbagianpembeliannama, prnotransaksi, csnotransaksi, rfqstatusnama, rfqstatussebelumnyanama, rfqinputusernama, rfqmodifikasiusernama

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
            Filter = Filter.Replace("rfqsupplierkode", "c1.kkode")
            Filter = Filter.Replace("rfqsuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'Dim query As New m0_query
        sql = m4_rfq_v_history()

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Rfq_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("rfqid"), 0), sptField,
                     FxDB(dr("rfqidhistory"), 0), sptField,
                     FxDB(dr("rfqcabang"), ""), sptField,
                     FxDB(dr("rfqlokasi"), ""), sptField,
                     FxDB(dr("rfqgudang"), ""), sptField,
                     FxDB(dr("rfqasalbarang"), ""), sptField,
                     FxDB(dr("rfqasalbarangkategori"), 0), sptField,
                     FxDB(dr("rfqjenispembelian"), ""), sptField,
                     FxDB(dr("rfqjenispembeliankategori"), 0), sptField,
                     FxDB(dr("rfqcarabayar"), 0), sptField,
                     FxDB(dr("rfqsumber"), ""), sptField,
                     FxDB(dr("rfqautonogrup"), 0), sptField,
                     FxDB(dr("rfqnogrup"), ""), sptField,
                     FxDB(dr("rfqautonotransaksi"), 0), sptField,
                     FxDB(dr("rfqnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rfqtgl"), ""), formatTgl), sptField,
                     FxDB(dr("rfqkodepa"), 0), sptField,
                     FxDB(dr("rfqsupplier"), 0), sptField,
                     FxDB(dr("rfqsupplierkontak"), ""), sptField,
                     FxDB(dr("rfq1alamat1"), ""), sptField,
                     FxDB(dr("rfq1alamat2"), ""), sptField,
                     FxDB(dr("rfq1alamat3"), ""), sptField,
                     FxDB(dr("rfq2alamat1"), ""), sptField,
                     FxDB(dr("rfq2alamat2"), ""), sptField,
                     FxDB(dr("rfq2alamat3"), ""), sptField,
                     FxDB(dr("rfqbagianpembelian"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rfqtgldipenuhi"), ""), formatTgl), sptField,
                     FxDB(dr("rfqtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rfqtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("rfquraian"), ""), sptField,
                     FxDB(dr("rfqcatatan"), ""), sptField,
                     FxDB(dr("rfqnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rfqtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("rfqtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("rfqmatauang"), ""), sptField,
                     FxDB(dr("rfqkurs"), 0), sptField,
                     FxDB(dr("rfqhargatermasukpajak"), 0), sptField,
                     FxDB(dr("rfqtotal"), 0), sptField,
                     FxDB(dr("rfqdiskonpersen"), ""), sptField,
                     FxDB(dr("rfqdiskon"), 0), sptField,
                     FxDB(dr("rfqtotalpajak1detail"), 0), sptField,
                     FxDB(dr("rfqtotalpajak2detail"), 0), sptField,
                     FxDB(dr("rfqbiayalainpersen"), ""), sptField,
                     FxDB(dr("rfqbiayalain"), 0), sptField,
                     FxDB(dr("rfqtotaltransaksi"), 0), sptField,
                     FxDB(dr("rfqidpr"), 0), sptField,
                     FxDB(dr("rfqidcs"), 0), sptField,
                     FxDB(dr("rfqstatuspo"), 0), sptField,
                     FxDB(dr("rfqstatusipc"), 0), sptField,
                     FxDB(dr("rfqstatusgrn"), 0), sptField,
                     FxDB(dr("rfqstatusri"), 0), sptField,
                     FxDB(dr("rfqstatusdnr"), 0), sptField,
                     FxDB(dr("rfqstatusprt"), 0), sptField,
                     FxDB(dr("rfqstatusrealisasi"), 0), sptField,
                     FxDB(dr("rfqstatus"), 0), sptField,
                     FxDB(dr("rfqstatussebelumnya"), 0), sptField,
                     FxDB(dr("rfqjmlrevisi"), 0), sptField,
                     FxDB(dr("rfqcetakanke"), 0), sptField,
                     FxDB(dr("rfqinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rfqinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rfqmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rfqmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rfqposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rfqpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rfqisclose"), 0), sptField,
                     FxDB(dr("rfqcabangnama"), ""), sptField,
                     FxDB(dr("rfqlokasinama"), ""), sptField,
                     FxDB(dr("rfqgudangnama"), ""), sptField,
                     FxDB(dr("rfqsupplierkode"), ""), sptField,
                     FxDB(dr("rfqsuppliernama"), ""), sptField,
                     FxDB(dr("rfqbagianpembeliankode"), ""), sptField,
                     FxDB(dr("rfqbagianpembeliannama"), ""), sptField,
                     FxDB(dr("prnotransaksi"), ""), sptField,
                     FxDB(dr("csnotransaksi"), ""), sptField,
                     FxDB(dr("rfqstatusnama"), ""), sptField,
                     FxDB(dr("rfqstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("rfqinputusernama"), ""), sptField,
                     FxDB(dr("rfqmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rfqidhistory, rfqid, rfqcabang, rfqlokasi, rfqgudang, rfqasalbarang, rfqasalbarangkategori, rfqjenispembelian, rfqjenispembeliankategori, rfqcarabayar, rfqsumber, rfqautonogrup, rfqnogrup, rfqautonotransaksi, rfqnotransaksi, rfqtgl, rfqkodepa, rfqsupplier, rfqsupplierkontak, rfq1alamat1, rfq1alamat2, rfq1alamat3, rfq2alamat1, rfq2alamat2, rfq2alamat3, rfqbagianpembelian, rfqtgldipenuhi, rfqtermin, rfqtgljatuhtempo, rfquraian, rfqcatatan, rfqnoref, rfqtglnoref, rfqtglpenutupan, rfqmatauang, rfqkurs, rfqhargatermasukpajak, rfqtotal, rfqdiskonpersen, rfqdiskon, rfqtotalpajak1detail, rfqtotalpajak2detail, rfqbiayalainpersen, rfqbiayalain, rfqtotaltransaksi, rfqidpr, rfqidcs, rfqstatuspo, rfqstatusipc, rfqstatusgrn, rfqstatusri, rfqstatusdnr, rfqstatusprt, rfqstatusrealisasi, rfqstatus, rfqstatussebelumnya, rfqjmlrevisi, rfqcetakanke, rfqinputuser, rfqinputtgl, rfqmodifikasiuser, rfqmodifikasitgl, rfqposting, rfqpostingtgl, rfqisclose, rfqcabangnama, rfqlokasinama, rfqgudangnama, rfqsupplierkode, rfqsuppliernama, rfqbagianpembeliankode, rfqbagianpembeliannama, prnotransaksi, csnotransaksi, rfqstatusnama, rfqstatussebelumnyanama, rfqinputusernama, rfqmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_RfqHistoryGetdataById(ByVal param As String) As String

        'M4_RfqGetdataById Utama --------------------------------------------------------
        'rfqidhistory, rfqid, rfqcabang, rfqlokasi, rfqgudang, rfqasalbarang, rfqasalbarangkategori, rfqjenispembelian, 
        'rfqjenispembeliankategori, rfqcarabayar, rfqsumber, rfqautonogrup, rfqnogrup, rfqautonotransaksi, rfqnotransaksi, 
        'rfqtgl, rfqkodepa, rfqsupplier, rfqsupplierkontak, rfq1alamat1, rfq1alamat2, rfq1alamat3, 
        'rfq2alamat1, rfq2alamat2, rfq2alamat3, rfqbagianpembelian, rfqtgldipenuhi, rfqtermin, rfqtgljatuhtempo, 
        'rfquraian, rfqcatatan, rfqnoref, rfqtglnoref, rfqtglpenutupan, rfqmatauang, rfqkurs, 
        'rfqhargatermasukpajak, rfqtotal, rfqdiskonpersen, rfqdiskon, rfqtotalpajak1detail, rfqtotalpajak2detail, rfqbiayalainpersen, 
        'rfqbiayalain, rfqtotaltransaksi, rfqidpr, rfqidcs, rfqstatuspo, rfqstatusipc, rfqstatusgrn, 
        'rfqstatusri, rfqstatusdnr, rfqstatusprt, rfqstatusrealisasi, rfqstatus, rfqstatussebelumnya, rfqjmlrevisi, 
        'rfqcetakanke, rfqinputuser, rfqinputtgl, rfqmodifikasiuser, rfqmodifikasitgl, rfqposting, rfqpostingtgl, 
        'rfqisclose, rfqcustomtext1, rfqcustomtext2, rfqcustomtext3, rfqcustomtext4, rfqcustomtext5, rfqcustomint1, 
        'rfqcustomint2, rfqcustomint3, rfqcustomdbl1, rfqcustomdbl2, rfqcustomdbl3, rfqcustomdate1, rfqcustomdate2, 
        'rfqcustomdate3, rfqcabangnama, rfqlokasinama, rfqgudangnama, rfqsupplierkode, rfqsuppliernama, rfqbagianpembeliankode, 
        'rfqbagianpembeliannama, rfqterminnama, rfqtermindiskon1, rfqterminharidiskon1, rfqtermindiskon2, rfqterminharidiskon2, rfqtermindenda, 
        'rfqtermindendaper, rfqterminharijatuhtempo, rfqnotransaksipr, rfqnotransaksics, rfqstatusnama, rfqstatussebelumnyanama, rfqinputusernama, 
        'rfqmodifikasiusernama

        'M4_RfqGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idrfqdetail, idrfq, idbarang, namabarang, tipebarang, jml, 
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

        Dim NmMemcached As String = "aplikasi1-M4_Rfq_history~M4_Rfq_Detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "rfqidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "rfqidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        sql = m4_rfq_getdata_history()

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("rfqidhistory"), 0), sptField,
                     FxDB(drutama("rfqid"), 0), sptField,
                     FxDB(drutama("rfqcabang"), ""), sptField,
                     FxDB(drutama("rfqlokasi"), ""), sptField,
                     FxDB(drutama("rfqgudang"), ""), sptField,
                     FxDB(drutama("rfqasalbarang"), ""), sptField,
                     FxDB(drutama("rfqasalbarangkategori"), 0), sptField,
                     FxDB(drutama("rfqjenispembelian"), ""), sptField,
                     FxDB(drutama("rfqjenispembeliankategori"), 0), sptField,
                     FxDB(drutama("rfqcarabayar"), 0), sptField,
                     FxDB(drutama("rfqsumber"), ""), sptField,
                     FxDB(drutama("rfqautonogrup"), 0), sptField,
                     FxDB(drutama("rfqnogrup"), ""), sptField,
                     FxDB(drutama("rfqautonotransaksi"), 0), sptField,
                     FxDB(drutama("rfqnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rfqtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("rfqkodepa"), 0), sptField,
                     FxDB(drutama("rfqsupplier"), 0), sptField,
                     FxDB(drutama("rfqsupplierkontak"), ""), sptField,
                     FxDB(drutama("rfq1alamat1"), ""), sptField,
                     FxDB(drutama("rfq1alamat2"), ""), sptField,
                     FxDB(drutama("rfq1alamat3"), ""), sptField,
                     FxDB(drutama("rfq2alamat1"), ""), sptField,
                     FxDB(drutama("rfq2alamat2"), ""), sptField,
                     FxDB(drutama("rfq2alamat3"), ""), sptField,
                     FxDB(drutama("rfqbagianpembelian"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rfqtgldipenuhi"), ""), formatTgl), sptField,
                     FxDB(drutama("rfqtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rfqtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("rfquraian"), ""), sptField,
                     FxDB(drutama("rfqcatatan"), ""), sptField,
                     FxDB(drutama("rfqnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rfqtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rfqtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("rfqmatauang"), ""), sptField,
                     FxDB(drutama("rfqkurs"), 0), sptField,
                     FxDB(drutama("rfqhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("rfqtotal"), 0), sptField,
                     FxDB(drutama("rfqdiskonpersen"), ""), sptField,
                     FxDB(drutama("rfqdiskon"), 0), sptField,
                     FxDB(drutama("rfqtotalpajak1detail"), 0), sptField,
                     FxDB(drutama("rfqtotalpajak2detail"), 0), sptField,
                     FxDB(drutama("rfqbiayalainpersen"), ""), sptField,
                     FxDB(drutama("rfqbiayalain"), 0), sptField,
                     FxDB(drutama("rfqtotaltransaksi"), 0), sptField,
                     FxDB(drutama("rfqidpr"), 0), sptField,
                     FxDB(drutama("rfqidcs"), 0), sptField,
                     FxDB(drutama("rfqstatuspo"), 0), sptField,
                     FxDB(drutama("rfqstatusipc"), 0), sptField,
                     FxDB(drutama("rfqstatusgrn"), 0), sptField,
                     FxDB(drutama("rfqstatusri"), 0), sptField,
                     FxDB(drutama("rfqstatusdnr"), 0), sptField,
                     FxDB(drutama("rfqstatusprt"), 0), sptField,
                     FxDB(drutama("rfqstatusrealisasi"), 0), sptField,
                     FxDB(drutama("rfqstatus"), 0), sptField,
                     FxDB(drutama("rfqstatussebelumnya"), 0), sptField,
                     FxDB(drutama("rfqjmlrevisi"), 0), sptField,
                     FxDB(drutama("rfqcetakanke"), 0), sptField,
                     FxDB(drutama("rfqinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rfqinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rfqmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rfqmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rfqposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rfqpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rfqisclose"), 0), sptField,
                     FxDB(drutama("rfqcustomtext1"), ""), sptField,
                     FxDB(drutama("rfqcustomtext2"), ""), sptField,
                     FxDB(drutama("rfqcustomtext3"), ""), sptField,
                     FxDB(drutama("rfqcustomtext4"), ""), sptField,
                     FxDB(drutama("rfqcustomtext5"), ""), sptField,
                     FxDB(drutama("rfqcustomint1"), 0), sptField,
                     FxDB(drutama("rfqcustomint2"), 0), sptField,
                     FxDB(drutama("rfqcustomint3"), 0), sptField,
                     FxDB(drutama("rfqcustomdbl1"), 0), sptField,
                     FxDB(drutama("rfqcustomdbl2"), 0), sptField,
                     FxDB(drutama("rfqcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rfqcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rfqcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rfqcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("rfqcabangnama"), ""), sptField,
                     FxDB(drutama("rfqlokasinama"), ""), sptField,
                     FxDB(drutama("rfqgudangnama"), ""), sptField,
                     FxDB(drutama("rfqsupplierkode"), ""), sptField,
                     FxDB(drutama("rfqsuppliernama"), ""), sptField,
                     FxDB(drutama("rfqbagianpembeliankode"), ""), sptField,
                     FxDB(drutama("rfqbagianpembeliannama"), ""), sptField,
                     FxDB(drutama("rfqterminnama"), ""), sptField,
                     FxDB(drutama("rfqtermindiskon1"), 0), sptField,
                     FxDB(drutama("rfqterminharidiskon1"), 0), sptField,
                     FxDB(drutama("rfqtermindiskon2"), 0), sptField,
                     FxDB(drutama("rfqterminharidiskon2"), 0), sptField,
                     FxDB(drutama("rfqtermindenda"), 0), sptField,
                     FxDB(drutama("rfqtermindendaper"), 0), sptField,
                     FxDB(drutama("rfqterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("rfqnotransaksipr"), ""), sptField,
                     FxDB(drutama("rfqnotransaksics"), ""), sptField,
                     FxDB(drutama("rfqstatusnama"), ""), sptField,
                     FxDB(drutama("rfqstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("rfqinputusernama"), ""), sptField,
                     FxDB(drutama("rfqmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idrfqdetail"), 0), sptField,
                     FxDB(dr("idrfq"), 0), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rfqidhistory, rfqid, rfqcabang, rfqlokasi, rfqgudang, rfqasalbarang, rfqasalbarangkategori, rfqjenispembelian, rfqjenispembeliankategori, rfqcarabayar, rfqsumber, rfqautonogrup, rfqnogrup, rfqautonotransaksi, rfqnotransaksi, rfqtgl, rfqkodepa, rfqsupplier, rfqsupplierkontak, rfq1alamat1, rfq1alamat2, rfq1alamat3, rfq2alamat1, rfq2alamat2, rfq2alamat3, rfqbagianpembelian, rfqtgldipenuhi, rfqtermin, rfqtgljatuhtempo, rfquraian, rfqcatatan, rfqnoref, rfqtglnoref, rfqtglpenutupan, rfqmatauang, rfqkurs, rfqhargatermasukpajak, rfqtotal, rfqdiskonpersen, rfqdiskon, rfqtotalpajak1detail, rfqtotalpajak2detail, rfqbiayalainpersen, rfqbiayalain, rfqtotaltransaksi, rfqidpr, rfqidcs, rfqstatuspo, rfqstatusipc, rfqstatusgrn, rfqstatusri, rfqstatusdnr, rfqstatusprt, rfqstatusrealisasi, rfqstatus, rfqstatussebelumnya, rfqjmlrevisi, rfqcetakanke, rfqinputuser, rfqinputtgl, rfqmodifikasiuser, rfqmodifikasitgl, rfqposting, rfqpostingtgl, rfqisclose, rfqcustomtext1, rfqcustomtext2, rfqcustomtext3, rfqcustomtext4, rfqcustomtext5, rfqcustomint1, rfqcustomint2, rfqcustomint3, rfqcustomdbl1, rfqcustomdbl2, rfqcustomdbl3, rfqcustomdate1, rfqcustomdate2, rfqcustomdate3, rfqcabangnama, rfqlokasinama, rfqgudangnama, rfqsupplierkode, rfqsuppliernama, rfqbagianpembeliankode, rfqbagianpembeliannama, rfqterminnama, rfqtermindiskon1, rfqterminharidiskon1, rfqtermindiskon2, rfqterminharidiskon2, rfqtermindenda, rfqtermindendaper, rfqterminharijatuhtempo, rfqnotransaksipr, rfqnotransaksics, rfqstatusnama, rfqstatussebelumnyanama, rfqinputusernama, rfqmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idrfqdetail, idrfq, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, jmlpo, statuspo, jmlipc, statusipc, jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, prnotransaksi, csnotransaksi, jmlsisapo, jmlsisarealisasi"))

        Return wsResult
    End Function

    Private Function m4_rfq_v_history() As String
        Dim sql As String
        'query
        sql = "select `rfq`.`rfqidhistory` AS `rfqidhistory`,`rfq`.`rfqid` AS `rfqid`,`rfq`.`rfqcabang` AS `rfqcabang`,`rfq`.`rfqlokasi` AS `rfqlokasi`,`rfq`.`rfqgudang` AS `rfqgudang`,`rfq`.`rfqasalbarang` AS `rfqasalbarang`,`rfq`.`rfqasalbarangkategori` AS `rfqasalbarangkategori`,`rfq`.`rfqjenispembelian` AS `rfqjenispembelian`,`rfq`.`rfqjenispembeliankategori` AS `rfqjenispembeliankategori`,`rfq`.`rfqcarabayar` AS `rfqcarabayar`,`rfq`.`rfqsumber` AS `rfqsumber`,`rfq`.`rfqautonogrup` AS `rfqautonogrup`,`rfq`.`rfqnogrup` AS `rfqnogrup`,`rfq`.`rfqautonotransaksi` AS `rfqautonotransaksi`,`rfq`.`rfqnotransaksi` AS `rfqnotransaksi`,`rfq`.`rfqtgl` AS `rfqtgl`,`rfq`.`rfqkodepa` AS `rfqkodepa`,`rfq`.`rfqsupplier` AS `rfqsupplier`,`rfq`.`rfqsupplierkontak` AS `rfqsupplierkontak`,`rfq`.`rfq1alamat1` AS `rfq1alamat1`,`rfq`.`rfq1alamat2` AS `rfq1alamat2`,`rfq`.`rfq1alamat3` AS `rfq1alamat3`,`rfq`.`rfq2alamat1` AS `rfq2alamat1`,`rfq`.`rfq2alamat2` AS `rfq2alamat2`,`rfq`.`rfq2alamat3` AS `rfq2alamat3`,`rfq`.`rfqbagianpembelian` AS `rfqbagianpembelian`,`rfq`.`rfqtgldipenuhi` AS `rfqtgldipenuhi`,`rfq`.`rfqtermin` AS `rfqtermin`,`rfq`.`rfqtgljatuhtempo` AS `rfqtgljatuhtempo`,`rfq`.`rfquraian` AS `rfquraian`,`rfq`.`rfqcatatan` AS `rfqcatatan`,`rfq`.`rfqnoref` AS `rfqnoref`,`rfq`.`rfqtglnoref` AS `rfqtglnoref`,`rfq`.`rfqtglpenutupan` AS `rfqtglpenutupan`,`rfq`.`rfqmatauang` AS `rfqmatauang`,`rfq`.`rfqkurs` AS `rfqkurs`,`rfq`.`rfqhargatermasukpajak` AS `rfqhargatermasukpajak`,`rfq`.`rfqtotal` AS `rfqtotal`,`rfq`.`rfqdiskonpersen` AS `rfqdiskonpersen`,`rfq`.`rfqdiskon` AS `rfqdiskon`,`rfq`.`rfqtotalpajak1detail` AS `rfqtotalpajak1detail`,`rfq`.`rfqtotalpajak2detail` AS `rfqtotalpajak2detail`,`rfq`.`rfqbiayalainpersen` AS `rfqbiayalainpersen`,`rfq`.`rfqbiayalain` AS `rfqbiayalain`,`rfq`.`rfqtotaltransaksi` AS `rfqtotaltransaksi`,`rfq`.`rfqidpr` AS `rfqidpr`,`rfq`.`rfqidcs` AS `rfqidcs`,`rfq`.`rfqstatuspo` AS `rfqstatuspo`,`rfq`.`rfqstatusipc` AS `rfqstatusipc`,`rfq`.`rfqstatusgrn` AS `rfqstatusgrn`,`rfq`.`rfqstatusri` AS `rfqstatusri`,`rfq`.`rfqstatusdnr` AS `rfqstatusdnr`,`rfq`.`rfqstatusprt` AS `rfqstatusprt`,`rfq`.`rfqstatusrealisasi` AS `rfqstatusrealisasi`,`rfq`.`rfqstatus` AS `rfqstatus`,`rfq`.`rfqstatussebelumnya` AS `rfqstatussebelumnya`,`rfq`.`rfqjmlrevisi` AS `rfqjmlrevisi`,`rfq`.`rfqcetakanke` AS `rfqcetakanke`,`rfq`.`rfqinputuser` AS `rfqinputuser`,`rfq`.`rfqinputtgl` AS `rfqinputtgl`,`rfq`.`rfqmodifikasiuser` AS `rfqmodifikasiuser`,`rfq`.`rfqmodifikasitgl` AS `rfqmodifikasitgl`,`rfq`.`rfqposting` AS `rfqposting`,`rfq`.`rfqpostingtgl` AS `rfqpostingtgl`,`rfq`.`rfqisclose` AS `rfqisclose`,`br`.`bnama` AS `rfqcabangnama`,`lc`.`lnama` AS `rfqlokasinama`,`wh`.`wnama` AS `rfqgudangnama`,`c1`.`kkode` AS `rfqsupplierkode`,`c1`.`knama` AS `rfqsuppliernama`,`c2`.`kkode` AS `rfqbagianpembeliankode`,`c2`.`knama` AS `rfqbagianpembeliannama`,`pr`.`prnotransaksi` AS `prnotransaksi`,`cs`.`csnotransaksi` AS `csnotransaksi`,`st1`.`nama` AS `rfqstatusnama`,`st2`.`nama` AS `rfqstatussebelumnyanama`,`u1`.`unama` AS `rfqinputusernama`,`u2`.`unama` AS `rfqmodifikasiusernama` from (((((((((((`m4_rfq_history` `rfq` left join `m1_branch` `br` on((`br`.`bkode` = `rfq`.`rfqcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `rfq`.`rfqlokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `rfq`.`rfqgudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `rfq`.`rfqsupplier`))) left join `m1_contact` `c2` on((`c2`.`kid` = `rfq`.`rfqbagianpembelian`))) left join `m4_pr` `pr` on((`rfq`.`rfqidpr` = `pr`.`prid`))) left join `m4_cs` `cs` on((`rfq`.`rfqidcs` = `cs`.`csid`))) left join `m0_status` `st1` on((`st1`.`kode` = `rfq`.`rfqstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `rfq`.`rfqstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `rfq`.`rfqinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `rfq`.`rfqmodifikasiuser`)))"
        Return sql
    End Function

    Private Function m4_rfq_getdata_history() As String
        Dim sql As String
        'query
        sql = "select `rfq`.`rfqidhistory` AS `rfqidhistory`,`rfq`.`rfqid` AS `rfqid`,`rfq`.`rfqid` AS `rfqid`,`rfq`.`rfqcabang` AS `rfqcabang`,`rfq`.`rfqlokasi` AS `rfqlokasi`,`rfq`.`rfqgudang` AS `rfqgudang`,`rfq`.`rfqasalbarang` AS `rfqasalbarang`,`rfq`.`rfqasalbarangkategori` AS `rfqasalbarangkategori`,`rfq`.`rfqjenispembelian` AS `rfqjenispembelian`,`rfq`.`rfqjenispembeliankategori` AS `rfqjenispembeliankategori`,`rfq`.`rfqcarabayar` AS `rfqcarabayar`,`rfq`.`rfqsumber` AS `rfqsumber`,`rfq`.`rfqautonogrup` AS `rfqautonogrup`,`rfq`.`rfqnogrup` AS `rfqnogrup`,`rfq`.`rfqautonotransaksi` AS `rfqautonotransaksi`,`rfq`.`rfqnotransaksi` AS `rfqnotransaksi`,`rfq`.`rfqtgl` AS `rfqtgl`,`rfq`.`rfqkodepa` AS `rfqkodepa`,`rfq`.`rfqsupplier` AS `rfqsupplier`,`rfq`.`rfqsupplierkontak` AS `rfqsupplierkontak`,`rfq`.`rfq1alamat1` AS `rfq1alamat1`,`rfq`.`rfq1alamat2` AS `rfq1alamat2`,`rfq`.`rfq1alamat3` AS `rfq1alamat3`,`rfq`.`rfq2alamat1` AS `rfq2alamat1`,`rfq`.`rfq2alamat2` AS `rfq2alamat2`,`rfq`.`rfq2alamat3` AS `rfq2alamat3`,`rfq`.`rfqbagianpembelian` AS `rfqbagianpembelian`,`rfq`.`rfqtgldipenuhi` AS `rfqtgldipenuhi`,`rfq`.`rfqtermin` AS `rfqtermin`,`rfq`.`rfqtgljatuhtempo` AS `rfqtgljatuhtempo`,`rfq`.`rfquraian` AS `rfquraian`,`rfq`.`rfqcatatan` AS `rfqcatatan`,`rfq`.`rfqnoref` AS `rfqnoref`,`rfq`.`rfqtglnoref` AS `rfqtglnoref`,`rfq`.`rfqtglpenutupan` AS `rfqtglpenutupan`,`rfq`.`rfqmatauang` AS `rfqmatauang`,`rfq`.`rfqkurs` AS `rfqkurs`,`rfq`.`rfqhargatermasukpajak` AS `rfqhargatermasukpajak`,`rfq`.`rfqtotal` AS `rfqtotal`,`rfq`.`rfqdiskonpersen` AS `rfqdiskonpersen`,`rfq`.`rfqdiskon` AS `rfqdiskon`,`rfq`.`rfqtotalpajak1detail` AS `rfqtotalpajak1detail`,`rfq`.`rfqtotalpajak2detail` AS `rfqtotalpajak2detail`,`rfq`.`rfqbiayalainpersen` AS `rfqbiayalainpersen`,`rfq`.`rfqbiayalain` AS `rfqbiayalain`,`rfq`.`rfqtotaltransaksi` AS `rfqtotaltransaksi`,`rfq`.`rfqidpr` AS `rfqidpr`,`rfq`.`rfqidcs` AS `rfqidcs`,`rfq`.`rfqstatuspo` AS `rfqstatuspo`,`rfq`.`rfqstatusipc` AS `rfqstatusipc`,`rfq`.`rfqstatusgrn` AS `rfqstatusgrn`,`rfq`.`rfqstatusri` AS `rfqstatusri`,`rfq`.`rfqstatusdnr` AS `rfqstatusdnr`,`rfq`.`rfqstatusprt` AS `rfqstatusprt`,`rfq`.`rfqstatusrealisasi` AS `rfqstatusrealisasi`,`rfq`.`rfqstatus` AS `rfqstatus`,`rfq`.`rfqstatussebelumnya` AS `rfqstatussebelumnya`,`rfq`.`rfqjmlrevisi` AS `rfqjmlrevisi`,`rfq`.`rfqcetakanke` AS `rfqcetakanke`,`rfq`.`rfqinputuser` AS `rfqinputuser`,`rfq`.`rfqinputtgl` AS `rfqinputtgl`,`rfq`.`rfqmodifikasiuser` AS `rfqmodifikasiuser`,`rfq`.`rfqmodifikasitgl` AS `rfqmodifikasitgl`,`rfq`.`rfqposting` AS `rfqposting`,`rfq`.`rfqpostingtgl` AS `rfqpostingtgl`,`rfq`.`rfqisclose` AS `rfqisclose`,`rfq`.`rfqcustomtext1` AS `rfqcustomtext1`,`rfq`.`rfqcustomtext2` AS `rfqcustomtext2`,`rfq`.`rfqcustomtext3` AS `rfqcustomtext3`,`rfq`.`rfqcustomtext4` AS `rfqcustomtext4`,`rfq`.`rfqcustomtext5` AS `rfqcustomtext5`,`rfq`.`rfqcustomint1` AS `rfqcustomint1`,`rfq`.`rfqcustomint2` AS `rfqcustomint2`,`rfq`.`rfqcustomint3` AS `rfqcustomint3`,`rfq`.`rfqcustomdbl1` AS `rfqcustomdbl1`,`rfq`.`rfqcustomdbl2` AS `rfqcustomdbl2`,`rfq`.`rfqcustomdbl3` AS `rfqcustomdbl3`,`rfq`.`rfqcustomdate1` AS `rfqcustomdate1`,`rfq`.`rfqcustomdate2` AS `rfqcustomdate2`,`rfq`.`rfqcustomdate3` AS `rfqcustomdate3`,`br`.`bnama` AS `rfqcabangnama`,`lc`.`lnama` AS `rfqlokasinama`,`wh`.`wnama` AS `rfqgudangnama`,`c1`.`kkode` AS `rfqsupplierkode`,`c1`.`knama` AS `rfqsuppliernama`,`c2`.`kkode` AS `rfqbagianpembeliankode`,`c2`.`knama` AS `rfqbagianpembeliannama`,`tr`.`trnama` AS `rfqterminnama`,`tr`.`trdiskon1` AS `rfqtermindiskon1`,`tr`.`trharidiskon1` AS `rfqterminharidiskon1`,`tr`.`trdiskon2` AS `rfqtermindiskon2`,`tr`.`trharidiskon2` AS `rfqterminharidiskon2`,`tr`.`trdenda` AS `rfqtermindenda`,`tr`.`trdendaper` AS `rfqtermindendaper`,`tr`.`trharijatuhtempo` AS `rfqterminharijatuhtempo`,`pr`.`prnotransaksi` AS `rfqnotransaksipr`,`cs`.`csnotransaksi` AS `rfqnotransaksics`,`st1`.`nama` AS `rfqstatusnama`,`st2`.`nama` AS `rfqstatussebelumnyanama`,`u1`.`unama` AS `rfqinputusernama`,`u2`.`unama` AS `rfqmodifikasiusernama`,`rfqd`.`idhistorydetail` AS `idhistorydetail`,`rfqd`.`idhistory` AS `idhistory`,`rfqd`.`idrfqdetail` AS `idrfqdetail`,`rfqd`.`idrfq` AS `idrfq`,`rfqd`.`idbarang` AS `idbarang`,`rfqd`.`namabarang` AS `namabarang`,`rfqd`.`tipebarang` AS `tipebarang`,`rfqd`.`jml` AS `jml`,`rfqd`.`satuan` AS `satuan`,`rfqd`.`nilaisatuan` AS `nilaisatuan`,`rfqd`.`jmlbarang` AS `jmlbarang`,`rfqd`.`satuanbarang` AS `satuanbarang`,`rfqd`.`matauang` AS `matauang`,`rfqd`.`kurs` AS `kurs`,`rfqd`.`harga` AS `harga`,`rfqd`.`diskon` AS `diskon`,`rfqd`.`jmldiskon` AS `jmldiskon`,`rfqd`.`pajak1` AS `pajak1`,`rfqd`.`jmlpajak1` AS `jmlpajak1`,`rfqd`.`pajak2` AS `pajak2`,`rfqd`.`jmlpajak2` AS `jmlpajak2`,`rfqd`.`cabang` AS `cabang`,`rfqd`.`lokasi` AS `lokasi`,`rfqd`.`gudang` AS `gudang`,`rfqd`.`costcenter` AS `costcenter`,`rfqd`.`divisi` AS `divisi`,`rfqd`.`subdivisi` AS `subdivisi`,`rfqd`.`proyek` AS `proyek`,`rfqd`.`catatan` AS `catatan`,`rfqd`.`urutan` AS `urutan`,`rfqd`.`idprdetail` AS `idprdetail`,`rfqd`.`idcsdetail` AS `idcsdetail`,`rfqd`.`jmlpo` AS `jmlpo`,`rfqd`.`statuspo` AS `statuspo`,`rfqd`.`jmlipc` AS `jmlipc`,`rfqd`.`statusipc` AS `statusipc`,`rfqd`.`jmlgrn` AS `jmlgrn`,`rfqd`.`statusgrn` AS `statusgrn`,`rfqd`.`jmlri` AS `jmlri`,`rfqd`.`statusri` AS `statusri`,`rfqd`.`jmldnr` AS `jmldnr`,`rfqd`.`statusdnr` AS `statusdnr`,`rfqd`.`jmlprt` AS `jmlprt`,`rfqd`.`statusprt` AS `statusprt`,`rfqd`.`jmlrealisasi` AS `jmlrealisasi`,`rfqd`.`statusrealisasi` AS `statusrealisasi`,`rfqd`.`isclose` AS `isclose`,`rfqd`.`customtext1` AS `customtext1`,`rfqd`.`customtext2` AS `customtext2`,`rfqd`.`customtext3` AS `customtext3`,`rfqd`.`customdbl1` AS `customdbl1`,`rfqd`.`customdbl2` AS `customdbl2`,`rfqd`.`customdbl3` AS `customdbl3`,`rfqd`.`customdate1` AS `customdate1`,`rfqd`.`customdate2` AS `customdate2`,`rfqd`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,`brd`.`bnama` AS `cabangnama`,`lcd`.`lnama` AS `lokasinama`,`whd`.`wnama` AS `gudangnama`,`cc`.`ccnama` AS `costcenternama`,`d`.`dnama` AS `divisinama`,`sd`.`sdnama` AS `subdivisinama`,`p`.`pnama` AS `proyeknama`,`pr2`.`prnotransaksi` AS `prnotransaksi`,`cs2`.`csnotransaksi` AS `csnotransaksi`,((`rfqd`.`jmlbarang` - `rfqd`.`jmlpo`) / `rfqd`.`nilaisatuan`) AS `jmlsisapo`,((`rfqd`.`jmlbarang` - `rfqd`.`jmlrealisasi`) / `rfqd`.`nilaisatuan`) AS `jmlsisarealisasi` from (((((((((((((((((((((((((((`m4_rfq_history` `rfq` join `m4_rfq_detail_history` `rfqd` on((`rfq`.`rfqidhistory` = `rfqd`.`idhistory`))) left join `m1_branch` `br` on((`br`.`bkode` = `rfq`.`rfqcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `rfq`.`rfqlokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `rfq`.`rfqgudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `rfq`.`rfqsupplier`))) left join `m1_contact` `c2` on((`c2`.`kid` = `rfq`.`rfqbagianpembelian`))) left join `m1_terms` `tr` on((`rfq`.`rfqtermin` = `tr`.`trkode`))) left join `m4_pr` `pr` on((`rfq`.`rfqidpr` = `pr`.`prid`))) left join `m4_cs` `cs` on((`rfq`.`rfqidcs` = `cs`.`csid`))) left join `m0_status` `st1` on((`st1`.`kode` = `rfq`.`rfqstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `rfq`.`rfqstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `rfq`.`rfqinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `rfq`.`rfqmodifikasiuser`))) left join `m1_item` `i` on((`i`.`bid` = `rfqd`.`idbarang`))) left join `m1_tax` `t1` on((`rfqd`.`pajak1` = `t1`.`tkode`))) left join `m1_tax` `t2` on((`rfqd`.`pajak2` = `t2`.`tkode`))) left join `m1_branch` `brd` on((`rfqd`.`cabang` = `brd`.`bkode`))) left join `m1_location` `lcd` on((`rfqd`.`lokasi` = `lcd`.`lkode`))) left join `m1_warehouse` `whd` on((`rfqd`.`gudang` = `whd`.`wkode`))) left join `m1_cost_center` `cc` on((`rfqd`.`costcenter` = `cc`.`cckode`))) left join `m1_division` `d` on((`rfqd`.`divisi` = `d`.`dkode`))) left join `m1_subdivision` `sd` on((`rfqd`.`subdivisi` = `sd`.`sdkode`))) left join `m1_project` `p` on((`rfqd`.`proyek` = `p`.`pkode`))) left join `m4_pr_detail` `prd` on((`rfqd`.`idprdetail` = `prd`.`idprdetail`))) left join `m4_pr` `pr2` on((`prd`.`idpr` = `pr2`.`prid`))) left join `m4_cs_detail` `csd` on((`rfqd`.`idcsdetail` = `csd`.`idcsdetail`))) left join `m4_cs` `cs2` on((`csd`.`idcs` = `cs2`.`csid`)))"
        Return sql
    End Function

End Class
