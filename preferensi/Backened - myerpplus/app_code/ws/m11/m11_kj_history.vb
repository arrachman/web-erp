Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m11_kj_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M11_Kj_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m_11_kj_history(SELECT 0, kj.* FROM m_11_kj kj WHERE kj.kjid = '" & idtransaksi & "')"
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
            sql = "SELECT kjidhistory FROM m_11_kj_history WHERE kjid = '" & idtransaksi & "' ORDER BY kjmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            'sql = "INSERT INTO m5_so_detail_history (SELECT 0, '" & result(4) & "', so.* FROM m5_so_detail so WHERE so.idso = '" & idtransaksi & "' )"
            'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            'With objCmd
            '    .Connection = Con2
            '    .Transaction = Trans
            '    .CommandType = CommandType.Text
            '    .CommandText = sql
            'End With
            'objCmd.ExecuteNonQuery()
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
    Public Function M5_So_HistorySearch(ByVal param As String) As String
        'M5_So_HistorySearch --------------------------------------------------------
        'soidhistory, soid, socabang, solokasi, sogudang, soasalbarang, soasalbarangkategori, sojenispenjualan, 
        'sojenispenjualankategori, socarabayar, sosumber, soautonotransaksi, sonotransaksi, sotgl, sokodepa, 
        'socustomer, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, 
        'so2alamat3, sobagianpenjualan, soekspedisi, sotglkirim, sotermin, sotgljatuhtempo, souraian, 
        'socatatan, sonoref, sotglnoref, sotglpenutupan, somatauang, sokurs, sohargatermasukpajak, 
        'sototal, sodiskonpersen, sojmldiskon, sototalpajak1detail, sototalpajak2detail, sobiayalainpersen, sobiayalain, 
        'sototaltransaksi, sojmlbayar, sorekdiskon, sorekpajak1, sorekpajak2, sorekbiayalain, sorekbayar, 
        'soidsq, sostatuspl, sostatusdo, sostatusdr, sostatuspi, sostatussi, sostatusrnr, 
        'sostatussr, sostatusrealisasi, sostatus, sostatussebelumnya, sojmlrevisi, socetakanke, soinputuser, 
        'soinputtgl, somodifikasiuser, somodifikasitgl, soposting, sopostingtgl, soisclose, socabangnama, 
        'solokasinama, sogudangnama, socustomerkode, socustomernama, sobagianpenjualankode, sobagianpenjualannama, soekspedisinama, 
        'sqnotransaksi, sostatusnama, sostatussebelumnyanama, soinputusernama, somodifikasiusernama

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

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_so_v_history")

        dt = AmbilData("aplikasi1-M5_so_v_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("soidhistory"), 0), sptField,
                     FxDB(dr("soid"), 0), sptField,
                     FxDB(dr("socabang"), ""), sptField,
                     FxDB(dr("solokasi"), ""), sptField,
                     FxDB(dr("sogudang"), ""), sptField,
                     FxDB(dr("soasalbarang"), ""), sptField,
                     FxDB(dr("soasalbarangkategori"), 0), sptField,
                     FxDB(dr("sojenispenjualan"), ""), sptField,
                     FxDB(dr("sojenispenjualankategori"), 0), sptField,
                     FxDB(dr("socarabayar"), 0), sptField,
                     FxDB(dr("sosumber"), ""), sptField,
                     FxDB(dr("soautonotransaksi"), 0), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sotgl"), ""), formatTgl), sptField,
                     FxDB(dr("sokodepa"), 0), sptField,
                     FxDB(dr("socustomer"), 0), sptField,
                     FxDB(dr("socustomerkontak"), ""), sptField,
                     FxDB(dr("so1alamat1"), ""), sptField,
                     FxDB(dr("so1alamat2"), ""), sptField,
                     FxDB(dr("so1alamat3"), ""), sptField,
                     FxDB(dr("so2alamat1"), ""), sptField,
                     FxDB(dr("so2alamat2"), ""), sptField,
                     FxDB(dr("so2alamat3"), ""), sptField,
                     FxDB(dr("sobagianpenjualan"), 0), sptField,
                     FxDB(dr("soekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sotglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("sotermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sotgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("souraian"), ""), sptField,
                     FxDB(dr("socatatan"), ""), sptField,
                     FxDB(dr("sonoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sotglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sotglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("somatauang"), ""), sptField,
                     FxDB(dr("sokurs"), 0), sptField,
                     FxDB(dr("sohargatermasukpajak"), 0), sptField,
                     FxDB(dr("sototal"), 0), sptField,
                     FxDB(dr("sodiskonpersen"), ""), sptField,
                     FxDB(dr("sojmldiskon"), 0), sptField,
                     FxDB(dr("sototalpajak1detail"), 0), sptField,
                     FxDB(dr("sototalpajak2detail"), 0), sptField,
                     FxDB(dr("sobiayalainpersen"), 0), sptField,
                     FxDB(dr("sobiayalain"), 0), sptField,
                     FxDB(dr("sototaltransaksi"), 0), sptField,
                     FxDB(dr("sojmlbayar"), 0), sptField,
                     FxDB(dr("sorekdiskon"), ""), sptField,
                     FxDB(dr("sorekpajak1"), ""), sptField,
                     FxDB(dr("sorekpajak2"), ""), sptField,
                     FxDB(dr("sorekbiayalain"), ""), sptField,
                     FxDB(dr("sorekbayar"), ""), sptField,
                     FxDB(dr("soidsq"), 0), sptField,
                     FxDB(dr("sostatuspl"), 0), sptField,
                     FxDB(dr("sostatusdo"), 0), sptField,
                     FxDB(dr("sostatusdr"), 0), sptField,
                     FxDB(dr("sostatuspi"), 0), sptField,
                     FxDB(dr("sostatussi"), 0), sptField,
                     FxDB(dr("sostatusrnr"), 0), sptField,
                     FxDB(dr("sostatussr"), 0), sptField,
                     FxDB(dr("sostatusrealisasi"), 0), sptField,
                     FxDB(dr("sostatus"), 0), sptField,
                     FxDB(dr("sostatussebelumnya"), 0), sptField,
                     FxDB(dr("sojmlrevisi"), 0), sptField,
                     FxDB(dr("socetakanke"), 0), sptField,
                     FxDB(dr("soinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("soinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("somodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("somodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("soposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sopostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("soisclose"), 0), sptField,
                     FxDB(dr("socabangnama"), ""), sptField,
                     FxDB(dr("solokasinama"), ""), sptField,
                     FxDB(dr("sogudangnama"), ""), sptField,
                     FxDB(dr("socustomerkode"), ""), sptField,
                     FxDB(dr("socustomernama"), ""), sptField,
                     FxDB(dr("sobagianpenjualankode"), ""), sptField,
                     FxDB(dr("sobagianpenjualannama"), ""), sptField,
                     FxDB(dr("soekspedisinama"), ""), sptField,
                     FxDB(dr("sqnotransaksi"), ""), sptField,
                     FxDB(dr("sostatusnama"), ""), sptField,
                     FxDB(dr("sostatussebelumnyanama"), ""), sptField,
                     FxDB(dr("soinputusernama"), ""), sptField,
                     FxDB(dr("somodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("soidhistory, soid, socabang, solokasi, sogudang, soasalbarang, soasalbarangkategori, sojenispenjualan, sojenispenjualankategori, socarabayar, sosumber, soautonotransaksi, sonotransaksi, sotgl, sokodepa, socustomer, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, so2alamat3, sobagianpenjualan, soekspedisi, sotglkirim, sotermin, sotgljatuhtempo, souraian, socatatan, sonoref, sotglnoref, sotglpenutupan, somatauang, sokurs, sohargatermasukpajak, sototal, sodiskonpersen, sojmldiskon, sototalpajak1detail, sototalpajak2detail, sobiayalainpersen, sobiayalain, sototaltransaksi, sojmlbayar, sorekdiskon, sorekpajak1, sorekpajak2, sorekbiayalain, sorekbayar, soidsq, sostatuspl, sostatusdo, sostatusdr, sostatuspi, sostatussi, sostatusrnr, sostatussr, sostatusrealisasi, sostatus, sostatussebelumnya, sojmlrevisi, socetakanke, soinputuser, soinputtgl, somodifikasiuser, somodifikasitgl, soposting, sopostingtgl, soisclose, socabangnama, solokasinama, sogudangnama, socustomerkode, socustomernama, sobagianpenjualankode, sobagianpenjualannama, soekspedisinama, sqnotransaksi, sostatusnama, sostatussebelumnyanama, soinputusernama, somodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SoHistoryGetdataById(ByVal param As String) As String
        'M5_SoHistoryGetdataById Utama --------------------------------------------------------
        'soidhistory, soid, socabang, solokasi, sogudang, soasalbarang, soasalbarangkategori, sojenispenjualan, 
        'sojenispenjualankategori, socarabayar, sosumber, soautonotransaksi, sonotransaksi, sotgl, sokodepa, 
        'socustomer, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, 
        'so2alamat3, sobagianpenjualan, soekspedisi, sotglkirim, sotermin, sotgljatuhtempo, souraian, 
        'socatatan, sonoref, sotglnoref, sotglpenutupan, somatauang, sokurs, sohargatermasukpajak, 
        'sototal, sodiskonpersen, sojmldiskon, sototalpajak1detail, sototalpajak2detail, sobiayalainpersen, sobiayalain, 
        'sototaltransaksi, sojmlbayar, sorekdiskon, sorekpajak1, sorekpajak2, sorekbiayalain, sorekbayar, 
        'soidsq, sostatuspl, sostatusdo, sostatusdr, sostatuspi, sostatussi, sostatusrnr, 
        'sostatussr, sostatusrealisasi, sostatus, sostatussebelumnya, sojmlrevisi, socetakanke, soinputuser, 
        'soinputtgl, somodifikasiuser, somodifikasitgl, soposting, sopostingtgl, soisclose, socustomtext1, 
        'socustomtext2, socustomtext3, socustomtext4, socustomtext5, socustomint1, socustomint2, socustomint3, 
        'socustomdbl1, socustomdbl2, socustomdbl3, socustomdate1, socustomdate2, socustomdate3, socabangnama, 
        'solokasinama, sogudangnama, socustomerkode, socustomernama, sobagianpenjualankode, sobagianpenjualannama, soekspedisinama, 
        'soterminnama, soterminharijatuhtempo, sorekdiskonnama, sorekpajak1nama, sorekpajak2nama, sorekbiayalainnama, sorekbayarnama, 
        'sonotransaksisq, sostatusnama, sostatussebelumnyanama, soinputusernama, somodifikasiusernama

        'M5_SoHistoryGetdataById Detail --------------------------------------------------------
        'idhistorydetail, idhistory, idsodetail, idso, 
        'idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, 
        'satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, 
        'jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, 
        'divisi, subdivisi, proyek, catatan, urutan, idsqdetail, jmlpl, 
        'statuspl, jmldo, statusdo, jmldr, statusdr, jmlpi, statuspi, 
        'jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, 
        'statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3, kodebarang, pajak1nama, pajak1nilai, 
        'pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, 
        'subdivisinama, proyeknama, sqnotransaksi

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

        Dim NmMemcached As String = "aplikasi1-M5_So_history~M5_So_Detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "soidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "soidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_so_getdata_history")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("soidhistory"), 0), sptField,
                     FxDB(drutama("soid"), 0), sptField,
                     FxDB(drutama("socabang"), ""), sptField,
                     FxDB(drutama("solokasi"), ""), sptField,
                     FxDB(drutama("sogudang"), ""), sptField,
                     FxDB(drutama("soasalbarang"), ""), sptField,
                     FxDB(drutama("soasalbarangkategori"), 0), sptField,
                     FxDB(drutama("sojenispenjualan"), ""), sptField,
                     FxDB(drutama("sojenispenjualankategori"), 0), sptField,
                     FxDB(drutama("socarabayar"), 0), sptField,
                     FxDB(drutama("sosumber"), ""), sptField,
                     FxDB(drutama("soautonotransaksi"), 0), sptField,
                     FxDB(drutama("sonotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sotgl"), ""), formatTgl), sptField,
                     FxDB(drutama("sokodepa"), 0), sptField,
                     FxDB(drutama("socustomer"), 0), sptField,
                     FxDB(drutama("socustomerkontak"), ""), sptField,
                     FxDB(drutama("so1alamat1"), ""), sptField,
                     FxDB(drutama("so1alamat2"), ""), sptField,
                     FxDB(drutama("so1alamat3"), ""), sptField,
                     FxDB(drutama("so2alamat1"), ""), sptField,
                     FxDB(drutama("so2alamat2"), ""), sptField,
                     FxDB(drutama("so2alamat3"), ""), sptField,
                     FxDB(drutama("sobagianpenjualan"), 0), sptField,
                     FxDB(drutama("soekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sotglkirim"), ""), formatTgl), sptField,
                     FxDB(drutama("sotermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sotgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("souraian"), ""), sptField,
                     FxDB(drutama("socatatan"), ""), sptField,
                     FxDB(drutama("sonoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sotglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sotglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("somatauang"), ""), sptField,
                     FxDB(drutama("sokurs"), 0), sptField,
                     FxDB(drutama("sohargatermasukpajak"), 0), sptField,
                     FxDB(drutama("sototal"), 0), sptField,
                     FxDB(drutama("sodiskonpersen"), ""), sptField,
                     FxDB(drutama("sojmldiskon"), 0), sptField,
                     FxDB(drutama("sototalpajak1detail"), 0), sptField,
                     FxDB(drutama("sototalpajak2detail"), 0), sptField,
                     FxDB(drutama("sobiayalainpersen"), 0), sptField,
                     FxDB(drutama("sobiayalain"), 0), sptField,
                     FxDB(drutama("sototaltransaksi"), 0), sptField,
                     FxDB(drutama("sojmlbayar"), 0), sptField,
                     FxDB(drutama("sorekdiskon"), ""), sptField,
                     FxDB(drutama("sorekpajak1"), ""), sptField,
                     FxDB(drutama("sorekpajak2"), ""), sptField,
                     FxDB(drutama("sorekbiayalain"), ""), sptField,
                     FxDB(drutama("sorekbayar"), ""), sptField,
                     FxDB(drutama("soidsq"), 0), sptField,
                     FxDB(drutama("sostatuspl"), 0), sptField,
                     FxDB(drutama("sostatusdo"), 0), sptField,
                     FxDB(drutama("sostatusdr"), 0), sptField,
                     FxDB(drutama("sostatuspi"), 0), sptField,
                     FxDB(drutama("sostatussi"), 0), sptField,
                     FxDB(drutama("sostatusrnr"), 0), sptField,
                     FxDB(drutama("sostatussr"), 0), sptField,
                     FxDB(drutama("sostatusrealisasi"), 0), sptField,
                     FxDB(drutama("sostatus"), 0), sptField,
                     FxDB(drutama("sostatussebelumnya"), 0), sptField,
                     FxDB(drutama("sojmlrevisi"), 0), sptField,
                     FxDB(drutama("socetakanke"), 0), sptField,
                     FxDB(drutama("soinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("soinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("somodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("somodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("soposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sopostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("soisclose"), 0), sptField,
                     FxDB(drutama("socustomtext1"), ""), sptField,
                     FxDB(drutama("socustomtext2"), ""), sptField,
                     FxDB(drutama("socustomtext3"), ""), sptField,
                     FxDB(drutama("socustomtext4"), ""), sptField,
                     FxDB(drutama("socustomtext5"), ""), sptField,
                     FxDB(drutama("socustomint1"), 0), sptField,
                     FxDB(drutama("socustomint2"), 0), sptField,
                     FxDB(drutama("socustomint3"), 0), sptField,
                     FxDB(drutama("socustomdbl1"), 0), sptField,
                     FxDB(drutama("socustomdbl2"), 0), sptField,
                     FxDB(drutama("socustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("socustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("socustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("socustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("socabangnama"), ""), sptField,
                     FxDB(drutama("solokasinama"), ""), sptField,
                     FxDB(drutama("sogudangnama"), ""), sptField,
                     FxDB(drutama("socustomerkode"), ""), sptField,
                     FxDB(drutama("socustomernama"), ""), sptField,
                     FxDB(drutama("sobagianpenjualankode"), ""), sptField,
                     FxDB(drutama("sobagianpenjualannama"), ""), sptField,
                     FxDB(drutama("soekspedisinama"), ""), sptField,
                     FxDB(drutama("soterminnama"), ""), sptField,
                     FxDB(drutama("soterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("sorekdiskonnama"), ""), sptField,
                     FxDB(drutama("sorekpajak1nama"), ""), sptField,
                     FxDB(drutama("sorekpajak2nama"), ""), sptField,
                     FxDB(drutama("sorekbiayalainnama"), ""), sptField,
                     FxDB(drutama("sorekbayarnama"), ""), sptField,
                     FxDB(drutama("sonotransaksisq"), ""), sptField,
                     FxDB(drutama("sostatusnama"), ""), sptField,
                     FxDB(drutama("sostatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("soinputusernama"), ""), sptField,
                     FxDB(drutama("somodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idsodetail"), 0), sptField,
                     FxDB(dr("idso"), 0), sptField,
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
                     FxDB(dr("idsqdetail"), 0), sptField,
                     FxDB(dr("jmlpl"), 0), sptField,
                     FxDB(dr("statuspl"), 0), sptField,
                     FxDB(dr("jmldo"), 0), sptField,
                     FxDB(dr("statusdo"), 0), sptField,
                     FxDB(dr("jmldr"), 0), sptField,
                     FxDB(dr("statusdr"), 0), sptField,
                     FxDB(dr("jmlpi"), 0), sptField,
                     FxDB(dr("statuspi"), 0), sptField,
                     FxDB(dr("jmlsi"), 0), sptField,
                     FxDB(dr("statussi"), 0), sptField,
                     FxDB(dr("jmlrnr"), 0), sptField,
                     FxDB(dr("statusrnr"), 0), sptField,
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
                     FxDB(dr("sqnotransaksi"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("soidhistory, soid, socabang, solokasi, sogudang, soasalbarang, soasalbarangkategori, sojenispenjualan, sojenispenjualankategori, socarabayar, sosumber, soautonotransaksi, sonotransaksi, sotgl, sokodepa, socustomer, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, so2alamat3, sobagianpenjualan, soekspedisi, sotglkirim, sotermin, sotgljatuhtempo, souraian, socatatan, sonoref, sotglnoref, sotglpenutupan, somatauang, sokurs, sohargatermasukpajak, sototal, sodiskonpersen, sojmldiskon, sototalpajak1detail, sototalpajak2detail, sobiayalainpersen, sobiayalain, sototaltransaksi, sojmlbayar, sorekdiskon, sorekpajak1, sorekpajak2, sorekbiayalain, sorekbayar, soidsq, sostatuspl, sostatusdo, sostatusdr, sostatuspi, sostatussi, sostatusrnr, sostatussr, sostatusrealisasi, sostatus, sostatussebelumnya, sojmlrevisi, socetakanke, soinputuser, soinputtgl, somodifikasiuser, somodifikasitgl, soposting, sopostingtgl, soisclose, socustomtext1, socustomtext2, socustomtext3, socustomtext4, socustomtext5, socustomint1, socustomint2, socustomint3, socustomdbl1, socustomdbl2, socustomdbl3, socustomdate1, socustomdate2, socustomdate3, socabangnama, solokasinama, sogudangnama, socustomerkode, socustomernama, sobagianpenjualankode, sobagianpenjualannama, soekspedisinama, soterminnama, soterminharijatuhtempo, sorekdiskonnama, sorekpajak1nama, sorekpajak2nama, sorekbiayalainnama, sorekbayarnama, sonotransaksisq, sostatusnama, sostatussebelumnyanama, soinputusernama, somodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idsodetail, idso, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, sqnotransaksi"))

        Return wsResult
    End Function

End Class
