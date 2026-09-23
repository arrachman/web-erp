Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m11_km_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M11_Km_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m_11_km_history(SELECT 0, km.* FROM m_11_km km WHERE km.kmid = '" & idtransaksi & "')"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY UTAMA --------------------------------


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
    Public Function M11_Km_HistorySearch(ByVal param As String) As String
        'M11_Km_HistorySearch --------------------------------------------------------
        'kmidhistory, kmid, kmcabang, kmlokasi, kmgudang, kmsumber, 
        'kmautonotransaksi, kmnotransaksi, kmtgl, kmkodepa, kmcustomer,
        'kmcustomerkontak, kmuraian, kmcatatan, kmnoref, kmtglnoref,
        'kmmatauang, kmkurs, kmidkj, kmkamar, kmkasur, 
        'kmtglmasuk, kmtglkeluar, kmjmlhari, kmharga, kmtotaltransaksi,
        'kmstatusrealisasi, kmstatus, kmstatussebekmmnya, kmjmlrevisi, kmcetakanke,
        'kminputuser, kminputtgl, kmmodifikasiuser, kmmodifikasitgl, kmisclose,
        'kmcabangnama, kmlokasinama, kmgudangnama, kmcustomerkode, kmcustomernama,
        'kmnotransaksikj, kmkamarnama, kmmasurnama, kmstatusnama, kmstatussebekmmnyanama,
        'kminputusernama, kmmodifikasiusernama, kmjenislab, kmperawatan, kmkategoripasien

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
            Filter = Filter.Replace("kmnotransaksikj", "kj.kjnotransaksi")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m11_km_v_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M11_Km_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("kmidhistory"), 0), sptField,
                     FxDB(dr("kmid"), 0), sptField,
                     FxDB(dr("kmcabang"), ""), sptField,
                     FxDB(dr("kmlokasi"), ""), sptField,
                     FxDB(dr("kmgudang"), ""), sptField,
                     FxDB(dr("kmsumber"), ""), sptField,
                     FxDB(dr("kmautonotransaksi"), 0), sptField,
                     FxDB(dr("kmnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("kmtgl"), ""), formatTgl), sptField,
                     FxDB(dr("kmkodepa"), 0), sptField,
                     FxDB(dr("kmcustomer"), 0), sptField,
                     FxDB(dr("kmcustomerkontak"), ""), sptField,
                     FxDB(dr("kmuraian"), ""), sptField,
                     FxDB(dr("kmcatatan"), ""), sptField,
                     FxDB(dr("kmnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("kmtglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("kmmatauang"), ""), sptField,
                     FxDB(dr("kmkurs"), 0), sptField,
                     FxDB(dr("kmidkj"), 0), sptField,
                     FxDB(dr("kmkamar"), ""), sptField,
                     FxDB(dr("kmkasur"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("kmtglmasuk"), ""), formatTglWaktu), sptField,
                     AsFormatTanggal(FxDB(dr("kmtglkeluar"), ""), formatTglWaktu), sptField,
                     FxDB(dr("kmjmlhari"), 0), sptField,
                     FxDB(dr("kmharga"), 0), sptField,
                     FxDB(dr("kmtotaltransaksi"), 0), sptField,
                     FxDB(dr("kmstatusrealisasi"), 0), sptField,
                     FxDB(dr("kmstatus"), 0), sptField,
                     FxDB(dr("kmstatussebelumnya"), 0), sptField,
                     FxDB(dr("kmjmlrevisi"), 0), sptField,
                     FxDB(dr("kmcetakanke"), 0), sptField,
                     FxDB(dr("kminputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("kminputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("kmmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("kmmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("kmisclose"), 0), sptField,
                     FxDB(dr("kmcabangnama"), ""), sptField,
                     FxDB(dr("kmlokasinama"), ""), sptField,
                     FxDB(dr("kmgudangnama"), ""), sptField,
                     FxDB(dr("kmcustomerkode"), ""), sptField,
                     FxDB(dr("kmcustomernama"), ""), sptField,
                     FxDB(dr("kmnotransaksikj"), ""), sptField,
                     FxDB(dr("kmkamarnama"), ""), sptField,
                     FxDB(dr("kmkasurnama"), ""), sptField,
                     FxDB(dr("kmstatusnama"), ""), sptField,
                     FxDB(dr("kmstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("kminputusernama"), ""), sptField,
                     FxDB(dr("kmmodifikasiusernama"), ""), sptField,
                     FxDB(dr("kmjenislab"), ""), sptField,
                     FxDB(dr("kmperawatan"), ""), sptField,
                     FxDB(dr("kmkategoripasien"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kmidhistory, kmid, kmcabang, kmlokasi, kmgudang, kmsumber, kmautonotransaksi, kmnotransaksi, kmtgl, kmkodepa, kmcustomer, kmcustomerkontak, kmuraian, kmcatatan, kmnoref, kmtglnoref, kmmatauang, kmkurs, kmidkj, kmkamar, kmkasur, kmtglmasuk, kmtglkeluar, kmjmlhari, kmharga, kmtotaltransaksi, kmstatusrealisasi, kmstatus, kmstatussebelumnya, kmjmlrevisi, kmcetakanke, kminputuser, kminputtgl, kmmodifikasiuser, kmmodifikasitgl, kmisclose, kmcabangnama, kmlokasinama, kmgudangnama, kmcustomerkode, kmcustomernama, kmnotransaksikj, kmkamarnama, kmkasurnama, kmstatusnama, kmstatussebekmmnyanama, kminputusernama, kmmodifikasiusernama, kmjenislab, kmperawatan, kmkategoripasien"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_KmHistoryGetdataById(ByVal param As String) As String
        'M11_Km_GetdataById Utama --------------------------------------------------------
        'kmidhistory, kmid, kmcabang, kmlokasi, kmgudang, kmsumber, 
        'kmautonotransaksi, kmnotransaksi, kmtgl, kmkodepa, kmcustomer,
        'kmcustomerkontak, kmuraian, kmcatatan, kmnoref, kmtglnoref,
        'kmmatauang, kmkurs, kmidkj, kmkamar, kmkasur, kmtglmasuk, kmtglkeluar,
        'kmjmlhari, kmharga, kmtotaltransaksi, kmrekpersediaan, kmrekhargapokok, 
        'kmrekdiskonpenjualan, kmrekpenjualan, kmstatusrealisasi, kmstatus,
        'kmstatussebelumnya, kmjmlrevisi, kmcetakanke, kminputuser, kminputtgl,
        'kmmodifikasiuser, kmmodifikasitgl, kmposting, kmisclose, kmcustomtext1, kmcustomtext2,
        'kmcustomtext3, kmcustomtext4, kmcustomtext5, kmcustomtext6, kmcustomtext7,
        'kmcustomtext8, kmcustomtext9, kmcustomtext10, kmcustomtext11, kmcustomtext12,
        'kmcustomtext13, kmcustomtext14, kmcustomtext15, kmcustomtext16, kmcustomtext17,
        'kmcustomtext18, kmcustomtext19, kmcustomtext20, kmcustomint1, kmcustomint2,
        'kmcustomint3, kmcustomint4, kmcustomint5, kmcustomint6, kmcustomint7,
        'kmcustomint8, kmcustomint9, kmcustomint10, kmcustomint11, kmcustomint12,
        'kmcustomint13, kmcustomint14, kmcustomint15, kmcustomint16, kmcustomint17,
        'kmcustomint18, kmcustomint19, kmcustomint20, kmcustomdbl1, kmcustomdbl2,
        'kmcustomdbl3, kmcustomdbl4, kmcustomdbl5, kmcustomdbl6, kmcustomdbl7,
        'kmcustomdbl8, kmcustomdbl9, kmcustomdbl10, kmcustomdbl11, kmcustomdbl12,
        'kmcustomdbl13, kmcustomdbl14, kmcustomdbl15, kmcustomdbl16, kmcustomdbl17,
        'kmcustomdbl18, kmcustomdbl19, kmcustomdbl20, kmcustomdate1, kmcustomdate2,
        'kmcustomdate3, kmcustomdate4, kmcustomdate5, kmcustomdate6, kmcustomdate7,
        'kmcustomdate8, kmcustomdate9, kmcustomdate10, kmcustomdate11, kmcustomdate12,
        'kmcustomdate13, kmcustomdate14, kmcustomdate15, kmcustomdate16, kmcustomdate17,
        'kmcustomdate18, kmcustomdate19, kmcustomdate20, kmcabangnama, kmlokasinama,
        'kmgudangnama, kmcustomerkode, kmcustomernama, kmnotransaksikj, kmkamarnama,
        'kmkasurnama, kmstatusnama, kmstatussebelumnyanama, kminputusernama, kmmodifikasiusernama
        'kmtingkatjual, kmjenislab, kmperawatan, kmkategoripasien

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

        Dim NmMemcached As String = "aplikasi1-M11_Km~M11_Km_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "kmidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "kmidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m11_km_getdata_history")

        dt = AmbilData("aplikasi1-M11_km_getdata_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("kmidhistory"), 0), sptField,
                     FxDB(drutama("kmid"), 0), sptField,
                     FxDB(drutama("kmcabang"), ""), sptField,
                     FxDB(drutama("kmlokasi"), ""), sptField,
                     FxDB(drutama("kmgudang"), ""), sptField,
                     FxDB(drutama("kmsumber"), ""), sptField,
                     FxDB(drutama("kmautonotransaksi"), 0), sptField,
                     FxDB(drutama("kmnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("kmtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("kmkodepa"), 0), sptField,
                     FxDB(drutama("kmcustomer"), 0), sptField,
                     FxDB(drutama("kmcustomerkontak"), ""), sptField,
                     FxDB(drutama("kmuraian"), ""), sptField,
                     FxDB(drutama("kmcatatan"), ""), sptField,
                     FxDB(drutama("kmnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("kmtglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("kmmatauang"), ""), sptField,
                     FxDB(drutama("kmkurs"), 0), sptField,
                     FxDB(drutama("kmidkj"), 0), sptField,
                     FxDB(drutama("kmkamar"), ""), sptField,
                     FxDB(drutama("kmkasur"), ""), sptField,
                     FxDB(drutama("kmtglmasuk"), ""), sptField,
                     FxDB(drutama("kmtglkeluar"), ""), sptField,
                     FxDB(drutama("kmjmlhari"), 0), sptField,
                     FxDB(drutama("kmharga"), 0), sptField,
                     FxDB(drutama("kmtotaltransaksi"), 0), sptField,
                     FxDB(drutama("kmrekpersediaan"), ""), sptField,
                     FxDB(drutama("kmrekhargapokok"), ""), sptField,
                     FxDB(drutama("kmrekdiskonpenjualan"), ""), sptField,
                     FxDB(drutama("kmrekpenjualan"), ""), sptField,
                     FxDB(drutama("kmstatusrealisasi"), 0), sptField,
                     FxDB(drutama("kmstatus"), 0), sptField,
                     FxDB(drutama("kmstatussebelumnya"), 0), sptField,
                     FxDB(drutama("kmjmlrevisi"), 0), sptField,
                     FxDB(drutama("kmcetakanke"), 0), sptField,
                     FxDB(drutama("kminputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kminputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("kmmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kmmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("kmposting"), 0), sptField,
                     FxDB(drutama("kmisclose"), 0), sptField,
                     FxDB(drutama("kmcustomtext1"), ""), sptField,
                     FxDB(drutama("kmcustomtext2"), ""), sptField,
                     FxDB(drutama("kmcustomtext3"), ""), sptField,
                     FxDB(drutama("kmcustomtext4"), ""), sptField,
                     FxDB(drutama("kmcustomtext5"), ""), sptField,
                     FxDB(drutama("kmcustomtext6"), ""), sptField,
                     FxDB(drutama("kmcustomtext7"), ""), sptField,
                     FxDB(drutama("kmcustomtext8"), ""), sptField,
                     FxDB(drutama("kmcustomtext9"), ""), sptField,
                     FxDB(drutama("kmcustomtext10"), ""), sptField,
                     FxDB(drutama("kmcustomtext11"), ""), sptField,
                     FxDB(drutama("kmcustomtext12"), ""), sptField,
                     FxDB(drutama("kmcustomtext13"), ""), sptField,
                     FxDB(drutama("kmcustomtext14"), ""), sptField,
                     FxDB(drutama("kmcustomtext15"), ""), sptField,
                     FxDB(drutama("kmcustomtext16"), ""), sptField,
                     FxDB(drutama("kmcustomtext17"), ""), sptField,
                     FxDB(drutama("kmcustomtext18"), ""), sptField,
                     FxDB(drutama("kmcustomtext19"), ""), sptField,
                     FxDB(drutama("kmcustomtext20"), ""), sptField,
                     FxDB(drutama("kmcustomint1"), 0), sptField,
                     FxDB(drutama("kmcustomint2"), 0), sptField,
                     FxDB(drutama("kmcustomint3"), 0), sptField,
                     FxDB(drutama("kmcustomint4"), 0), sptField,
                     FxDB(drutama("kmcustomint5"), 0), sptField,
                     FxDB(drutama("kmcustomint6"), 0), sptField,
                     FxDB(drutama("kmcustomint7"), 0), sptField,
                     FxDB(drutama("kmcustomint8"), 0), sptField,
                     FxDB(drutama("kmcustomint9"), 0), sptField,
                     FxDB(drutama("kmcustomint10"), 0), sptField,
                     FxDB(drutama("kmcustomint11"), 0), sptField,
                     FxDB(drutama("kmcustomint12"), 0), sptField,
                     FxDB(drutama("kmcustomint13"), 0), sptField,
                     FxDB(drutama("kmcustomint14"), 0), sptField,
                     FxDB(drutama("kmcustomint15"), 0), sptField,
                     FxDB(drutama("kmcustomint16"), 0), sptField,
                     FxDB(drutama("kmcustomint17"), 0), sptField,
                     FxDB(drutama("kmcustomint18"), 0), sptField,
                     FxDB(drutama("kmcustomint19"), 0), sptField,
                     FxDB(drutama("kmcustomint20"), 0), sptField,
                     FxDB(drutama("kmcustomdbl1"), 0), sptField,
                     FxDB(drutama("kmcustomdbl2"), 0), sptField,
                     FxDB(drutama("kmcustomdbl3"), 0), sptField,
                     FxDB(drutama("kmcustomdbl4"), 0), sptField,
                     FxDB(drutama("kmcustomdbl5"), 0), sptField,
                     FxDB(drutama("kmcustomdbl6"), 0), sptField,
                     FxDB(drutama("kmcustomdbl7"), 0), sptField,
                     FxDB(drutama("kmcustomdbl8"), 0), sptField,
                     FxDB(drutama("kmcustomdbl9"), 0), sptField,
                     FxDB(drutama("kmcustomdbl10"), 0), sptField,
                     FxDB(drutama("kmcustomdbl11"), 0), sptField,
                     FxDB(drutama("kmcustomdbl12"), 0), sptField,
                     FxDB(drutama("kmcustomdbl13"), 0), sptField,
                     FxDB(drutama("kmcustomdbl14"), 0), sptField,
                     FxDB(drutama("kmcustomdbl15"), 0), sptField,
                     FxDB(drutama("kmcustomdbl16"), 0), sptField,
                     FxDB(drutama("kmcustomdbl17"), 0), sptField,
                     FxDB(drutama("kmcustomdbl18"), 0), sptField,
                     FxDB(drutama("kmcustomdbl19"), 0), sptField,
                     FxDB(drutama("kmcustomdbl20"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate10"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate11"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate12"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate13"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate14"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate15"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate16"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate17"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate18"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate19"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate20"), ""), formatTgl), sptField,
                     FxDB(drutama("kmcabangnama"), ""), sptField,
                     FxDB(drutama("kmlokasinama"), ""), sptField,
                     FxDB(drutama("kmgudangnama"), ""), sptField,
                     FxDB(drutama("kmcustomerkode"), ""), sptField,
                     FxDB(drutama("kmcustomernama"), ""), sptField,
                     FxDB(drutama("kmnotransaksikj"), ""), sptField,
                     FxDB(drutama("kmkamarnama"), ""), sptField,
                     FxDB(drutama("kmkasurnama"), ""), sptField,
                     FxDB(drutama("kmstatusnama"), ""), sptField,
                     FxDB(drutama("kmstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("kminputusernama"), ""), sptField,
                     FxDB(drutama("kmmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("kmtingkatjual"), ""), sptField,
                     FxDB(drutama("kmjenislab"), ""), sptField,
                     FxDB(drutama("kmperawatan"), ""), sptField,
                     FxDB(drutama("kmkategoripasien"), ""))

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
        strResultData = String.Concat(utama)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kmidhistory, kmid, kmcabang, kmlokasi, kmgudang, kmsumber, kmautonotransaksi, kmnotransaksi, kmtgl, kmkodepa, kmcustomer, kmcustomerkontak, kmuraian, kmcatatan, kmnoref, kmtglnoref, kmmatauang, kmkurs, kmidkj, kmkamar, kmkasur, kmtglmasuk, kmtglkeluar, kmjmlhari, kmharga, kmtotaltransaksi, kmrekpersediaan, kmrekhargapokok, kmrekdiskonpenjualan, kmrekpenjualan, kmstatusrealisasi, kmstatus, kmstatussebelumnya, kmjmlrevisi, kmcetakanke, kminputuser, kminputtgl, kmmodifikasiuser, kmmodifikasitgl, kmposting, kmisclose, kmcustomtext1, kmcustomtext2, kmcustomtext3, kmcustomtext4, kmcustomtext5, kmcustomtext6, kmcustomtext7, kmcustomtext8, kmcustomtext9, kmcustomtext10, kmcustomtext11, kmcustomtext12, kmcustomtext13, kmcustomtext14, kmcustomtext15, kmcustomtext16, kmcustomtext17, kmcustomtext18, kmcustomtext19, kmcustomtext20, kmcustomint1, kmcustomint2, kmcustomint3, kmcustomint4, kmcustomint5, kmcustomint6, kmcustomint7, kmcustomint8, kmcustomint9, kmcustomint10, kmcustomint11, kmcustomint12, kmcustomint13, kmcustomint14, kmcustomint15, kmcustomint16, kmcustomint17, kmcustomint18, kmcustomint19, kmcustomint20, kmcustomdbl1, kmcustomdbl2, kmcustomdbl3, kmcustomdbl4, kmcustomdbl5, kmcustomdbl6, kmcustomdbl7, kmcustomdbl8, kmcustomdbl9, kmcustomdbl10, kmcustomdbl11, kmcustomdbl12, kmcustomdbl13, kmcustomdbl14, kmcustomdbl15, kmcustomdbl16, kmcustomdbl17, kmcustomdbl18, kmcustomdbl19, kmcustomdbl20, kmcustomdate1, kmcustomdate2, kmcustomdate3,  kmcustomdate4, kmcustomdate5, kmcustomdate6, kmcustomdate7, kmcustomdate8, kmcustomdate9, kmcustomdate10, kmcustomdate11, kmcustomdate12, kmcustomdate13, kmcustomdate14, kmcustomdate15, kmcustomdate16, kmcustomdate17, kmcustomdate18, kmcustomdate19, kmcustomdate20, kmcabangnama, kmlokasinama, kmgudangnama, kmcustomerkode, kmcustomernama, kmnotransaksikj, kmkamarnama, kmkasurnama, kmstatusnama, kmstatussebelumnyanama, kminputusernama, kmmodifikasiusernama, kmtingkatjual, kmjenislab, kmperawatan, kmkategoripasien"))

        Return wsResult
    End Function


End Class