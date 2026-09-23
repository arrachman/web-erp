Imports Microsoft.VisualBasic

Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_vp_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_Vp_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m4_vp_history(SELECT 0, vp.* FROM m4_vp vp WHERE vp.vpid = '" & idtransaksi & "')"
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
            sql = "SELECT vpidhistory FROM m4_vp_history WHERE vpid = '" & idtransaksi & "' ORDER BY vpmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m4_vp_detail_history (SELECT 0, '" & result(4) & "', vp.* FROM m4_vp_detail vp WHERE vp.idvp = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY DETAIL -------------------------------

            'PROSES INSERT HISTORY Pay --------------------------------------
            sql = "INSERT INTO m4_vp_pay_history (SELECT 0, '" & result(4) & "', vp.* FROM m4_vp_pay vp WHERE vp.idvp = '" & idtransaksi & "' )"
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
    Public Function M4_Vp_HistorySearch(ByVal param As String) As String
        'M4_VpSearch --------------------------------------------------------
        'vpidhistory, vpid, vpcabang, vplokasi, vpgudang, vpsumber, vpautonotransaksi, vpnotransaksi, 
        'vptgl, vpkodepa, vpsupplier, vpsupplierkontak, vp1alamat1, vp1alamat2, vp1alamat3, 
        'vp2alamat1, vp2alamat2, vp2alamat3, vpbagianpembayaran, vpuraian, vpcatatan, vpnoref, 
        'vptglnoref, vpcarabayar, vptglbayar, vpmatauang, vpkurs, vptotalap, vptotalapvalas, 
        'vptotalar, vptotalarvalas, vpbayar, vpbayarvalas, vpselisihkurs, vprekselisihkurs, vpdiskontermin, 
        'vpdiskonterminvalas, vprekdiskontermin, vpidvpp, vpstatus, vpstatussebelumnya, vpjmlrevisi, vpcetakanke, 
        'vpinputuser, vpinputtgl, vpmodifikasiuser, vpmodifikasitgl, vpposting, vppostingtgl, vpisclose, 
        'vpcabangnama, vplokasinama, vpgudangnama, vpsupplierkode, vpsuppliernama, vpbagianpembayarankode, vpbagianpembayarannama, 
        'vpcarabayarnama, vprekselisihkursnama, vprekdiskonterminnama, vppnotransaksi, vpstatusnama, vpstatussebelumnyanama, vpinputusernama, 
        'vpmodifikasiusernama

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
            Filter = Filter.Replace("vpsupplierkode", "c1.kkode")
            Filter = Filter.Replace("vpsuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_vp_v_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Vp_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("vpid"), 0), sptField,
                     FxDB(dr("vpidhistory"), 0), sptField,
                     FxDB(dr("vpcabang"), ""), sptField,
                     FxDB(dr("vplokasi"), ""), sptField,
                     FxDB(dr("vpgudang"), ""), sptField,
                     FxDB(dr("vpsumber"), ""), sptField,
                     FxDB(dr("vpautonotransaksi"), 0), sptField,
                     FxDB(dr("vpnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("vptgl"), ""), formatTgl), sptField,
                     FxDB(dr("vpkodepa"), 0), sptField,
                     FxDB(dr("vpsupplier"), 0), sptField,
                     FxDB(dr("vpsupplierkontak"), ""), sptField,
                     FxDB(dr("vp1alamat1"), ""), sptField,
                     FxDB(dr("vp1alamat2"), ""), sptField,
                     FxDB(dr("vp1alamat3"), ""), sptField,
                     FxDB(dr("vp2alamat1"), ""), sptField,
                     FxDB(dr("vp2alamat2"), ""), sptField,
                     FxDB(dr("vp2alamat3"), ""), sptField,
                     FxDB(dr("vpbagianpembayaran"), 0), sptField,
                     FxDB(dr("vpuraian"), ""), sptField,
                     FxDB(dr("vpcatatan"), ""), sptField,
                     FxDB(dr("vpnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("vptglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("vpcarabayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("vptglbayar"), ""), formatTgl), sptField,
                     FxDB(dr("vpmatauang"), ""), sptField,
                     FxDB(dr("vpkurs"), 0), sptField,
                     FxDB(dr("vptotalap"), 0), sptField,
                     FxDB(dr("vptotalapvalas"), 0), sptField,
                     FxDB(dr("vptotalar"), 0), sptField,
                     FxDB(dr("vptotalarvalas"), 0), sptField,
                     FxDB(dr("vpbayar"), 0), sptField,
                     FxDB(dr("vpbayarvalas"), 0), sptField,
                     FxDB(dr("vpselisihkurs"), 0), sptField,
                     FxDB(dr("vprekselisihkurs"), ""), sptField,
                     FxDB(dr("vpdiskontermin"), 0), sptField,
                     FxDB(dr("vpdiskonterminvalas"), 0), sptField,
                     FxDB(dr("vprekdiskontermin"), ""), sptField,
                     FxDB(dr("vpidvpp"), 0), sptField,
                     FxDB(dr("vpstatus"), 0), sptField,
                     FxDB(dr("vpstatussebelumnya"), 0), sptField,
                     FxDB(dr("vpjmlrevisi"), 0), sptField,
                     FxDB(dr("vpcetakanke"), 0), sptField,
                     FxDB(dr("vpinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("vpinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("vpmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("vpmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("vpposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("vppostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("vpisclose"), 0), sptField,
                     FxDB(dr("vpcabangnama"), ""), sptField,
                     FxDB(dr("vplokasinama"), ""), sptField,
                     FxDB(dr("vpgudangnama"), ""), sptField,
                     FxDB(dr("vpsupplierkode"), ""), sptField,
                     FxDB(dr("vpsuppliernama"), ""), sptField,
                     FxDB(dr("vpbagianpembayarankode"), ""), sptField,
                     FxDB(dr("vpbagianpembayarannama"), ""), sptField,
                     FxDB(dr("vpcarabayarnama"), ""), sptField,
                     FxDB(dr("vprekselisihkursnama"), ""), sptField,
                     FxDB(dr("vprekdiskonterminnama"), ""), sptField,
                     FxDB(dr("vppnotransaksi"), ""), sptField,
                     FxDB(dr("vpstatusnama"), ""), sptField,
                     FxDB(dr("vpstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("vpinputusernama"), ""), sptField,
                     FxDB(dr("vpmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("vpidhistory, vpid, vpcabang, vplokasi, vpgudang, vpsumber, vpautonotransaksi, vpnotransaksi, vptgl, vpkodepa, vpsupplier, vpsupplierkontak, vp1alamat1, vp1alamat2, vp1alamat3, vp2alamat1, vp2alamat2, vp2alamat3, vpbagianpembayaran, vpuraian, vpcatatan, vpnoref, vptglnoref, vpcarabayar, vptglbayar, vpmatauang, vpkurs, vptotalap, vptotalapvalas, vptotalar, vptotalarvalas, vpbayar, vpbayarvalas, vpselisihkurs, vprekselisihkurs, vpdiskontermin, vpdiskonterminvalas, vprekdiskontermin, vpidvpp, vpstatus, vpstatussebelumnya, vpjmlrevisi, vpcetakanke, vpinputuser, vpinputtgl, vpmodifikasiuser, vpmodifikasitgl, vpposting, vppostingtgl, vpisclose, vpcabangnama, vplokasinama, vpgudangnama, vpsupplierkode, vpsuppliernama, vpbagianpembayarankode, vpbagianpembayarannama, vpcarabayarnama, vprekselisihkursnama, vprekdiskonterminnama, vppnotransaksi, vpstatusnama, vpstatussebelumnyanama, vpinputusernama, vpmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_VpHistoryGetdataById(ByVal param As String) As String

        'M4_VpHistoryGetdataById Utama --------------------------------------------------------
        'vpidhistory, vpid, vpcabang, vplokasi, vpgudang, vpsumber, vpautonotransaksi, vpnotransaksi, 
        'vptgl, vpkodepa, vpsupplier, vpsupplierkontak, vp1alamat1, vp1alamat2, vp1alamat3, 
        'vp2alamat1, vp2alamat2, vp2alamat3, vpbagianpembayaran, vpuraian, vpcatatan, vpnoref, 
        'vptglnoref, vpcarabayar, vptglbayar, vpmatauang, vpkurs, vptotalap, vptotalapvalas, 
        'vptotalar, vptotalarvalas, vpbayar, vpbayarvalas, vpselisihkurs, vprekselisihkurs, vpdiskontermin, 
        'vpdiskonterminvalas, vprekdiskontermin, vpidvpp, vpstatus, vpstatussebelumnya, vpjmlrevisi, vpcetakanke, 
        'vpinputuser, vpinputtgl, vpmodifikasiuser, vpmodifikasitgl, vpposting, vppostingtgl, vpisclose, 
        'vpcustomtext1, vpcustomtext2, vpcustomtext3, vpcustomtext4, vpcustomtext5, vpcustomint1, vpcustomint2, 
        'vpcustomint3, vpcustomdbl1, vpcustomdbl2, vpcustomdbl3, vpcustomdate1, vpcustomdate2, vpcustomdate3, 
        'vpcabangnama, vplokasinama, vpgudangnama, vpsupplierkode, vpsuppliernama, vpbagianpembayarankode, vpbagianpembayarannama, 
        'vpcarabayarnama, vprekselisihkursnama, vprekdiskonterminnama, vpnotransaksivpp, vpstatusnama, vpstatussebelumnyanama, vpinputusernama, 
        'vpmodifikasiusernama

        'M4_VpHistoryGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idvpdetail, idvp, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, 
        'sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, rekhutangpiutang, 
        'catatan, costcenter, divisi, subdivisi, proyek, idvppdetail, urutan, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, 
        'tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, 
        'rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, vppnotransaksi, inputtgl

        'M4_VpGetdataById Pay ----------------------------------------------------------
        'idhistorycarabayar, idhistory, idvpcarabayar, idvp, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, idvppcarabayar, isclose, carabayarnama, banknama, rekbanknama, rekgironama

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

        Dim utama As String = "", detail As String = "", pay As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M4_Vp_history~M4_Vp_Detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "vpidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "vpidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_vp_getdata_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("vpidhistory"), 0), sptField, FxDB(drutama("vpid"), 0), sptField,
                     FxDB(drutama("vpcabang"), ""), sptField,
                     FxDB(drutama("vplokasi"), ""), sptField,
                     FxDB(drutama("vpgudang"), ""), sptField,
                     FxDB(drutama("vpsumber"), ""), sptField,
                     FxDB(drutama("vpautonotransaksi"), 0), sptField,
                     FxDB(drutama("vpnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("vptgl"), ""), formatTgl), sptField,
                     FxDB(drutama("vpkodepa"), 0), sptField,
                     FxDB(drutama("vpsupplier"), 0), sptField,
                     FxDB(drutama("vpsupplierkontak"), ""), sptField,
                     FxDB(drutama("vp1alamat1"), ""), sptField,
                     FxDB(drutama("vp1alamat2"), ""), sptField,
                     FxDB(drutama("vp1alamat3"), ""), sptField,
                     FxDB(drutama("vp2alamat1"), ""), sptField,
                     FxDB(drutama("vp2alamat2"), ""), sptField,
                     FxDB(drutama("vp2alamat3"), ""), sptField,
                     FxDB(drutama("vpbagianpembayaran"), 0), sptField,
                     FxDB(drutama("vpuraian"), ""), sptField,
                     FxDB(drutama("vpcatatan"), ""), sptField,
                     FxDB(drutama("vpnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("vptglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("vpcarabayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vptglbayar"), ""), formatTgl), sptField,
                     FxDB(drutama("vpmatauang"), ""), sptField,
                     FxDB(drutama("vpkurs"), 0), sptField,
                     FxDB(drutama("vptotalap"), 0), sptField,
                     FxDB(drutama("vptotalapvalas"), 0), sptField,
                     FxDB(drutama("vptotalar"), 0), sptField,
                     FxDB(drutama("vptotalarvalas"), 0), sptField,
                     FxDB(drutama("vpbayar"), 0), sptField,
                     FxDB(drutama("vpbayarvalas"), 0), sptField,
                     FxDB(drutama("vpselisihkurs"), 0), sptField,
                     FxDB(drutama("vprekselisihkurs"), ""), sptField,
                     FxDB(drutama("vpdiskontermin"), 0), sptField,
                     FxDB(drutama("vpdiskonterminvalas"), 0), sptField,
                     FxDB(drutama("vprekdiskontermin"), ""), sptField,
                     FxDB(drutama("vpidvpp"), 0), sptField,
                     FxDB(drutama("vpstatus"), 0), sptField,
                     FxDB(drutama("vpstatussebelumnya"), 0), sptField,
                     FxDB(drutama("vpjmlrevisi"), 0), sptField,
                     FxDB(drutama("vpcetakanke"), 0), sptField,
                     FxDB(drutama("vpinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vpinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("vpmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vpmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("vpposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vppostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("vpisclose"), 0), sptField,
                     FxDB(drutama("vpcustomtext1"), ""), sptField,
                     FxDB(drutama("vpcustomtext2"), ""), sptField,
                     FxDB(drutama("vpcustomtext3"), ""), sptField,
                     FxDB(drutama("vpcustomtext4"), ""), sptField,
                     FxDB(drutama("vpcustomtext5"), ""), sptField,
                     FxDB(drutama("vpcustomint1"), 0), sptField,
                     FxDB(drutama("vpcustomint2"), 0), sptField,
                     FxDB(drutama("vpcustomint3"), 0), sptField,
                     FxDB(drutama("vpcustomdbl1"), 0), sptField,
                     FxDB(drutama("vpcustomdbl2"), 0), sptField,
                     FxDB(drutama("vpcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vpcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("vpcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("vpcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("vpcabangnama"), ""), sptField,
                     FxDB(drutama("vplokasinama"), ""), sptField,
                     FxDB(drutama("vpgudangnama"), ""), sptField,
                     FxDB(drutama("vpsupplierkode"), ""), sptField,
                     FxDB(drutama("vpsuppliernama"), ""), sptField,
                     FxDB(drutama("vpbagianpembayarankode"), ""), sptField,
                     FxDB(drutama("vpbagianpembayarannama"), ""), sptField,
                     FxDB(drutama("vpcarabayarnama"), ""), sptField,
                     FxDB(drutama("vprekselisihkursnama"), ""), sptField,
                     FxDB(drutama("vprekdiskonterminnama"), ""), sptField,
                     FxDB(drutama("vpnotransaksivpp"), ""), sptField,
                     FxDB(drutama("vpstatusnama"), ""), sptField,
                     FxDB(drutama("vpstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("vpinputusernama"), ""), sptField,
                     FxDB(drutama("vpmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField, FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idvpdetail"), 0), sptField,
                     FxDB(dr("idvp"), 0), sptField,
                     FxDB(dr("sumber"), ""), sptField,
                     FxDB(dr("idtransaksi"), 0), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("totaltransaksi"), 0), sptField,
                     FxDB(dr("terbayar"), 0), sptField,
                     FxDB(dr("sisa"), 0), sptField,
                     FxDB(dr("jmlbayar"), 0), sptField,
                     FxDB(dr("jmlbayarvalas"), 0), sptField,
                     FxDB(dr("diskontermin"), ""), sptField,
                     FxDB(dr("jmldiskontermin"), 0), sptField,
                     FxDB(dr("jmldiskonterminvalas"), 0), sptField,
                     FxDB(dr("rekhutangpiutang"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("idvppdetail"), 0), sptField,
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
                     FxDB(dr("notransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                     FxDB(dr("carabayar"), 0), sptField,
                     FxDB(dr("termin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("rencana"), 0), sptField,
                     FxDB(dr("statuslunas"), 0), sptField,
                     FxDB(dr("diskon1"), 0), sptField,
                     FxDB(dr("haridiskon1"), 0), sptField,
                     FxDB(dr("diskon2"), 0), sptField,
                     FxDB(dr("haridiskon2"), 0), sptField,
                     FxDB(dr("rekhutangpiutangnama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("vppnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'PANGGIL QUERY
            sql = query.PanggilQuery("m4_vp_getdata_pay_history")

            'AMBIL DATA PAY
            Dim dtpay As New DataTable
            dtpay = AmbilData("aplikasi1-M4_Vp_Pay_history", "idhistory=" & idtransaksi, "idhistory ASC, urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtpay.Rows
                pay = String.Concat(pay, FxDB(dr("idhistorycarabayar"), 0), sptField, FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idvpcarabayar"), 0), sptField,
                     FxDB(dr("idvp"), 0), sptField,
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
                     FxDB(dr("idvppcarabayar"), 0), sptField,
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
            result(2) = "VP transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, pay)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("vpidhistory, vpid, vpcabang, vplokasi, vpgudang, vpsumber, vpautonotransaksi, vpnotransaksi, vptgl, vpkodepa, vpsupplier, vpsupplierkontak, vp1alamat1, vp1alamat2, vp1alamat3, vp2alamat1, vp2alamat2, vp2alamat3, vpbagianpembayaran, vpuraian, vpcatatan, vpnoref, vptglnoref, vpcarabayar, vptglbayar, vpmatauang, vpkurs, vptotalap, vptotalapvalas, vptotalar, vptotalarvalas, vpbayar, vpbayarvalas, vpselisihkurs, vprekselisihkurs, vpdiskontermin, vpdiskonterminvalas, vprekdiskontermin, vpidvpp, vpstatus, vpstatussebelumnya, vpjmlrevisi, vpcetakanke, vpinputuser, vpinputtgl, vpmodifikasiuser, vpmodifikasitgl, vpposting, vppostingtgl, vpisclose, vpcustomtext1, vpcustomtext2, vpcustomtext3, vpcustomtext4, vpcustomtext5, vpcustomint1, vpcustomint2, vpcustomint3, vpcustomdbl1, vpcustomdbl2, vpcustomdbl3, vpcustomdate1, vpcustomdate2, vpcustomdate3, vpcabangnama, vplokasinama, vpgudangnama, vpsupplierkode, vpsuppliernama, vpbagianpembayarankode, vpbagianpembayarannama, vpcarabayarnama, vprekselisihkursnama, vprekdiskonterminnama, vpnotransaksivpp, vpstatusnama, vpstatussebelumnyanama, vpinputusernama, vpmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idvpdetail, idvp, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, idvppdetail, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, vppnotransaksi, inputtgl" & sptSubParam & "idhistorycarabayar, idhistory, idvpcarabayar, idvp, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, idvppcarabayar, isclose, carabayarnama, banknama, rekbanknama, rekgironama"))

        Return wsResult
    End Function

End Class
